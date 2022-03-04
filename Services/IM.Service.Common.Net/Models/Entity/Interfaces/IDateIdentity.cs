using System;

namespace IM.Service.Common.Net.Models.Entity.Interfaces;

public interface IDateIdentity : IPeriod
{
    DateOnly Date { get; set; }
}