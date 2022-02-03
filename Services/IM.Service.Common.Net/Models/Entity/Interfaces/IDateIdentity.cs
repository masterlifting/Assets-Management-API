using System;

namespace IM.Service.Common.Net.Models.Entity.Interfaces;

public interface IDateIdentity
{
    DateOnly Date { get; set; }
}