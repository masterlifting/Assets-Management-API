using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace IM.Service.Portfolio.Services.Entity;

public class DerivativeService
{
    public ILogger<DealService> Logger { get; }
    private readonly Repository<Deal> dealRepo;
    private readonly Repository<Income> incomeRepo;
    private readonly Repository<Expense> expenseRepo;
    private readonly Repository<Derivative> derivativeRepo;

    public DerivativeService(
        ILogger<DealService> logger
        , Repository<Deal> dealRepo
        , Repository<Income> incomeRepo
        , Repository<Expense> expenseRepo
        , Repository<Derivative> derivativeRepo)
    {
        Logger = logger;
        this.dealRepo = dealRepo;
        this.incomeRepo = incomeRepo;
        this.expenseRepo = expenseRepo;
        this.derivativeRepo = derivativeRepo;
    }

    public Task ComputeToTransferAsync(IEnumerable<Derivative> entities)
    {
        return Logger.LogDefaultTask("ComputeToTransferAsync");
    }
    public Task ComputeToTransferAsync(Derivative entity)
    {
        return Logger.LogDefaultTask("ComputeToTransferAsync");
    }
}