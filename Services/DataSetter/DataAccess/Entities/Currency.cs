using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Currency
    {
        public Currency()
        {
            AccountSummaries = new HashSet<AccountSummary>();
            AccountTransactions = new HashSet<AccountTransaction>();
            ComissionSummaries = new HashSet<ComissionSummary>();
            Comissions = new HashSet<Comission>();
            CompanySummaries = new HashSet<CompanySummary>();
            DividendSummaries = new HashSet<DividendSummary>();
            Dividends = new HashSet<Dividend>();
            ExchangeRateSummaries = new HashSet<ExchangeRateSummary>();
            ExchangeRates = new HashSet<ExchangeRate>();
            Prices = new HashSet<Price>();
            StockTransactions = new HashSet<StockTransaction>();
        }

        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<AccountSummary> AccountSummaries { get; set; }
        public virtual ICollection<AccountTransaction> AccountTransactions { get; set; }
        public virtual ICollection<ComissionSummary> ComissionSummaries { get; set; }
        public virtual ICollection<Comission> Comissions { get; set; }
        public virtual ICollection<CompanySummary> CompanySummaries { get; set; }
        public virtual ICollection<DividendSummary> DividendSummaries { get; set; }
        public virtual ICollection<Dividend> Dividends { get; set; }
        public virtual ICollection<ExchangeRateSummary> ExchangeRateSummaries { get; set; }
        public virtual ICollection<ExchangeRate> ExchangeRates { get; set; }
        public virtual ICollection<Price> Prices { get; set; }
        public virtual ICollection<StockTransaction> StockTransactions { get; set; }
    }
}
