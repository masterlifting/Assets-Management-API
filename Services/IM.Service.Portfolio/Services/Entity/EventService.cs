using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Domain.Entities.Catalogs;
using IM.Service.Shared.Helpers;
using IM.Service.Shared.RabbitMq;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Portfolio.Enums;

namespace IM.Service.Portfolio.Services.Entity;

public class EventService
{
    public ILogger<EventService> Logger { get; }
    private readonly Repository<Event> eventRepo;
    private readonly Repository<Derivative> derivativeRepo;
    private readonly Dictionary<OperationTypes, byte[]> operationTypes;

    public EventService(
        ILogger<EventService> logger
        , Repository<Event> eventRepo
        , Repository<Derivative> derivativeRepo
        , Repository<EventType> eventTypeRepo)
    {
        Logger = logger;
        this.eventRepo = eventRepo;
        this.derivativeRepo = derivativeRepo;
        operationTypes = new Dictionary<OperationTypes, byte[]>
        {
            {OperationTypes.Income, eventTypeRepo.Where(x => x.OperationTypeId == (byte)OperationTypes.Income).Select(x => x.Id).ToArray()},
            {OperationTypes.Expense, eventTypeRepo.Where(x => x.OperationTypeId == (byte)OperationTypes.Expense).Select(x => x.Id).ToArray()}
        };
    }

    public Task SetDerivativeBalancesAsync(QueueActions action, IEnumerable<Event> entities) => action switch
    {
        QueueActions.Create => Up(entities),
        QueueActions.Delete => Down(entities),
        QueueActions.Update => Update(entities),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };
    public Task SetDerivativeBalanceAsync(QueueActions action, Event entity) => action switch
    {
        QueueActions.Create => Up(entity),
        QueueActions.Delete => Down(entity),
        QueueActions.Update => Update(entity),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };

    private async Task Up(Event _event)
    {
        var derivative = await derivativeRepo.FindAsync(_event.DerivativeId, _event.DerivativeCode);

        if (derivative is null)
            throw new ApplicationException($"Derivative '{_event.DerivativeCode}' not found");

        derivative.Balance += Compute(_event);

        await derivativeRepo.UpdateAsync(new object[] { _event.DerivativeId, _event.DerivativeCode }, derivative, "up balance");
    }
    private async Task Down(Event _event)
    {
        var derivative = await derivativeRepo.FindAsync(_event.DerivativeId, _event.DerivativeCode);

        if (derivative is null)
            throw new ApplicationException($"Derivative '{_event.DerivativeCode}' not found");

        derivative.Balance += Compute(_event);

        await derivativeRepo.UpdateAsync(new object[] { _event.DerivativeId, _event.DerivativeCode }, derivative, "down balance");
    }
    private async Task Update(Event _event)
    {
        var derivative = await derivativeRepo.FindAsync(_event.DerivativeId, _event.DerivativeCode);

        if (derivative is null)
            throw new ApplicationException($"Derivative '{_event.DerivativeCode}' not found");

        var events = await eventRepo.GetSampleAsync(x =>
            x.DerivativeId == _event.DerivativeId
            && x.DerivativeCode == _event.DerivativeCode
            && x.AccountId == _event.AccountId
            && x.UserId == _event.UserId
            && x.ProviderId == _event.ProviderId
            && x.ExchangeId == _event.ExchangeId);

        var balance = Compute(events);

        derivative.Balance = balance;

        await derivativeRepo.UpdateAsync(new object[] { _event.DerivativeId, _event.DerivativeCode }, derivative, "update balance");
    }

    private async Task Up(IEnumerable<Event> events)
    {
        events = events.ToArray();

        var groupedEvents = events
            .GroupBy(x => (x.DerivativeId, x.DerivativeCode))
            .ToDictionary(x => x.Key, y => Compute(y.ToArray()));

        var derivativeIds = events.Select(x => x.DerivativeId).Distinct();
        var derivativeCodes = events.Select(x => x.DerivativeCode).Distinct().ToArray();

        var derivatives = await derivativeRepo.GetSampleAsync(x => derivativeIds.Contains(x.Id) && derivativeCodes.Contains(x.Code));
        var dicDerivatives = derivatives.ToDictionary(x => (x.Id, x.Code));
        foreach (var derivative in dicDerivatives)
            derivative.Value.Balance = 0;

        foreach (var (key, sum) in groupedEvents)
            dicDerivatives[key].Balance += sum;

        await derivativeRepo.UpdateRangeAsync(derivatives, "up balances");
    }
    private async Task Down(IEnumerable<Event> events)
    {
        events = events.ToArray();

        var groupedEvents = events
            .GroupBy(x => (x.DerivativeId, x.DerivativeCode))
            .ToDictionary(x => x.Key, y => Compute(y.ToArray()));

        var derivativeIds = events.Select(x => x.DerivativeId).Distinct();
        var derivativeCodes = events.Select(x => x.DerivativeCode).Distinct();

        var derivatives = await derivativeRepo.GetSampleAsync(x => derivativeIds.Contains(x.Id) && derivativeCodes.Contains(x.Code));
        var dicDerivatives = derivatives.ToDictionary(x => (x.Id, x.Code));

        var result = new List<Derivative>(groupedEvents.Count);
        foreach (var (key, sum) in groupedEvents)
            if (dicDerivatives.ContainsKey(key))
            {
                var derivative = dicDerivatives[key];
                derivative.Balance += sum;
                result.Add(derivative);
            }
            else
                Logger.LogWarning("Up derivative balances", key.DerivativeCode, "not found");

        await derivativeRepo.UpdateRangeAsync(result, "down balances");
    }
    private async Task Update(IEnumerable<Event> events)
    {
        events = events.ToArray();

        var dbEvents = await eventRepo.GetExistAsync(events);

        var groupedEvents = dbEvents
            .GroupBy(x => (x.DerivativeId, x.DerivativeCode))
            .ToDictionary(x => x.Key, y => Compute(y.ToArray()));

        var derivativeIds = events.Select(x => x.DerivativeId).Distinct();
        var derivativeCodes = events.Select(x => x.DerivativeCode).Distinct();

        var derivatives = await derivativeRepo.GetSampleAsync(x => derivativeIds.Contains(x.Id) && derivativeCodes.Contains(x.Code));
        var dicDerivatives = derivatives.ToDictionary(x => (x.Id, x.Code));

        var result = new List<Derivative>(groupedEvents.Count);
        foreach (var (key, sum) in groupedEvents)
            if (dicDerivatives.ContainsKey(key))
            {
                var derivative = dicDerivatives[key];
                derivative.Balance = sum;
                result.Add(derivative);
            }
            else
                Logger.LogWarning("Update derivative balances", key.DerivativeCode, "not found");

        await derivativeRepo.UpdateRangeAsync(result, "update balances");
    }

    private decimal Compute(Event _event) => operationTypes[OperationTypes.Income].Contains(_event.TypeId)
        ? _event.Value
        : operationTypes[OperationTypes.Expense].Contains(_event.TypeId)
            ? _event.Value
            : throw new ApplicationException($"Derivative '{_event.DerivativeCode}' balance not computed");
    private decimal Compute(Event[] events)
    {
        var income = events.Where(x => operationTypes[OperationTypes.Income].Contains(x.TypeId)).ToArray();
        var expense = events.Where(x => operationTypes[OperationTypes.Expense].Contains(x.TypeId)).ToArray();

        return income.Sum(x => x.Value) - expense.Sum(x => x.Value);
    }
}