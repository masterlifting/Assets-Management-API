using System;

namespace IM.Service.Shared.Models.Entity.Interfaces;

public interface IDateIdentity : IPeriod
{
    DateOnly Date { get; set; }
}