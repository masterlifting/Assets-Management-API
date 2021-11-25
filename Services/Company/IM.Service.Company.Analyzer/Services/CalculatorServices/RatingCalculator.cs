using System;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;

using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices;

public class RatingCalculator
{
    private readonly RepositorySet<DataAccess.Entities.Company> companyRepository;
    private readonly RepositorySet<Rating> ratingRepository;
    private readonly RepositorySet<Price> priceRepository;
    private readonly RepositorySet<Report> reportRepository;

    public RatingCalculator(
        RepositorySet<DataAccess.Entities.Company> companyRepository,
        RepositorySet<Rating> ratingRepository,
        RepositorySet<Price> priceRepository,
        RepositorySet<Report> reportRepository)
    {
        this.companyRepository = companyRepository;
        this.ratingRepository = ratingRepository;
        this.priceRepository = priceRepository;
        this.reportRepository = reportRepository;
    }

    public async Task CalculateAsync()
    {
        var companies = await companyRepository.GetSampleAsync(x => x.Id);
        var ratings = new List<Rating>(companies.Length);

        for (var i = 0; i < companies.Length; i++)
        {
            var index = i;

            var priceResults = await priceRepository.GetSampleAsync(
                x => x.CompanyId == companies[index] && x.StatusId == (byte)StatusType.Completed,
                x => x.Result);

            var reportResults = await reportRepository.GetSampleAsync(
                x => x.CompanyId == companies[index] && x.StatusId == (byte)StatusType.Completed,
                x => x.Result);

            var priceResult = RatingComparator.ComputeSampleResult(priceResults) * 10000;
            var reportResult = RatingComparator.ComputeSampleResult(reportResults);

            ratings.Add(new()
            {
                Place = i + 1,
                ResultPrice = priceResult,
                ResultReport = reportResult,
                Result = (priceResult + reportResult) * 0.5m,
                CompanyId = companies[i]
            });
        }

        ratings.Sort((x, y) => x.Result < y.Result ? 1 : x.Result > y.Result ? -1 : 0);

        for (var i = 0; i < ratings.Count; i++)
            ratings[i].Place = i + 1;

        var error = await ratingRepository.ReCreateAsync(ratings, "Rating");
        
        if(error is not null)
            throw new ArithmeticException($"Rating recreate failed! \nError: {error}");
    }
}