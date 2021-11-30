using IM.Service.Company.Analyzer.DataAccess.Entities;

using System.Collections.Generic;
using System.Linq;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices;

public class RatingService
{
    private readonly IReadOnlyCollection<Rating> newRating;
    public RatingService(IEnumerable<RatingData> data) => newRating = GetRating(data);

    public IEnumerable<Rating> GetRating(IEnumerable<Rating>? oldRating)
    {
        if (oldRating is null)
            return newRating;

        var oldRatingArray = oldRating.ToArray();

        if (!oldRatingArray.Any())
            return newRating;

        var newRatingDictionary = newRating.ToDictionary(x => x.CompanyId);
        var rating = new List<Rating>(oldRatingArray.Length);

        foreach (var item in oldRatingArray)
            if (newRatingDictionary.ContainsKey(item.CompanyId))
            {
                var companyRating = newRatingDictionary[item.CompanyId];

                rating.Add(new Rating
                {
                    Place = 0,
                    CompanyId = item.CompanyId,
                    AgregateResult = item.AgregateResult + companyRating.Result,
                    Result = item.AgregateResult == 0
                        ? companyRating.Result
                        : RatingComparator.ComputeSampleResult(new[] { item.AgregateResult, companyRating.Result })
                });
            }
            else
                rating.Add(item);

        SetPlaces(ref rating);

        return rating;
    }
    private static IReadOnlyCollection<Rating> GetRating(IEnumerable<RatingData> data)
    {
        var ratings = data
            .GroupBy(x => x.CompanyId)
            .Select(x => new Rating
            {
                Place = 0,
                CompanyId = x.Key,
                Result = RatingComparator
                    .ComputeSampleResult(x
                    .Select(y => y.Result)
                    .ToArray())
            })
            .ToList();

        SetPlaces(ref ratings);

        return ratings;
    }
    private static void SetPlaces(ref List<Rating> ratings)
    {
        ratings.Sort((x, y) => x.Result < y.Result ? 1 : x.Result > y.Result ? -1 : 0);

        for (var i = 0; i < ratings.Count; i++)
            ratings[i].Place = i + 1;
    }
}