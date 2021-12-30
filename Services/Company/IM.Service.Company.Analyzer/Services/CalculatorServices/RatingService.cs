﻿using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Company.Analyzer.DataAccess.Comparators;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices;

public class RatingService
{
    private readonly Repository<AnalyzedEntity> analyzedEntityRepository;
    private readonly Repository<DataAccess.Entities.Company> companyRepository;
    private readonly Repository<Rating> ratingRepository;

    public RatingService(
        Repository<AnalyzedEntity> analyzedEntityRepository, 
        Repository<DataAccess.Entities.Company> companyRepository,
        Repository<Rating> ratingRepository)
    {
        this.analyzedEntityRepository = analyzedEntityRepository;
        this.companyRepository = companyRepository;
        this.ratingRepository = ratingRepository;
    }
    public async Task SetRatingAsync()
    {
        var companyIds = await companyRepository.GetSampleAsync(x => x.Id);
        List<Task<Rating>> ratingTasks = new(companyIds.Length);
        foreach (var companyId in companyIds)
        {
            var data = await analyzedEntityRepository.GetSampleAsync(x => x.CompanyId == companyId && x.StatusId == (byte)Enums.Statuses.Computed);
            ratingTasks.Add(CalculatorService.RatingHelper.GetRatingAsync(companyId, data));
        }

        var rating = await Task.WhenAll(ratingTasks);

        await ratingRepository.CreateUpdateDeleteAsync(rating, new RatingComparer(), nameof(SetRatingAsync));
    }
}