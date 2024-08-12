using CsvHelper;
using CsvHelper.Configuration;
using HotelPricingEngine.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static HotelPricingEngine.Models.Enums;

public class PricingService
{
    private readonly ILogger<PricingService> _logger;
    private readonly string _csvFilePath = "historical.csv"; // Path to the CSV file for competitor prices

    public PricingService(ILogger<PricingService> logger)
    {
        _logger = logger;
    }

    // Base prices for different room types
    private readonly Dictionary<RoomType, double> _basePrices = new Dictionary<RoomType, double>
    {
        { RoomType.Standard, 90 },
        { RoomType.Deluxe, 140 },
        { RoomType.Suite, 240 }
    };
    public static string ToCsvSeason(Enums.Season season)
    {
        switch (season)
        {
            case Enums.Season.OffSeason:
                return "Off-Season";
            case Enums.Season.PeakSeason:
                return "Peak Season";
            default:
                throw new ArgumentException($"Unknown season: {season}");
        }
    }

    // Seasonality factors for adjusting prices
    private readonly Dictionary<Season, double> _seasonalityFactors = new Dictionary<Season, double>
    {
        { Season.OffSeason, -0.20 },
        { Season.PeakSeason, 0.30 }
    };

    // Constants to define the range for competitor price adjustments
    private const double MinCompetitorAdjustment = -0.10;
    private const double MaxCompetitorAdjustment = 0.10;

    // Calculates the adjusted price based on various factors
    public PricingResponse CalculateAdjustedPrice(PricingRequest request, double competitorAdjustment)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        if (!_basePrices.TryGetValue(request.RoomType, out var basePrice))
        {
            throw new ArgumentException("Invalid room type", nameof(request.RoomType));
        }

        double seasonalityFactor = _seasonalityFactors.GetValueOrDefault(request.Season, 0);
        double occupancyRateFactor = GetOccupancyRateFactor(request.OccupancyRate);

        double adjustedPrice = basePrice * (1 + seasonalityFactor + occupancyRateFactor + competitorAdjustment);

        return new PricingResponse
        {
            RoomType = request.RoomType,
            BasePrice = basePrice,
            AdjustedPrice = Math.Round(adjustedPrice, 2)
        };
    }

    // Gets the competitor adjustment based on competitor prices and the request
    public double GetCompetitorAdjustment(List<CompetitorPriceRecord> competitorPrices, PricingRequest request)
    {


        // Example implementation
        string season = ToCsvSeason(request.Season);
        if (competitorPrices == null) throw new ArgumentNullException(nameof(competitorPrices));
        if (request == null) throw new ArgumentNullException(nameof(request));

       
        var filteredPrices = competitorPrices
            .Where(cp => cp.RoomType == request.RoomType.ToString() &&
                         cp.Season == season &&
                         IsPriceInOccupancyRange(cp, request.OccupancyRate) &&
                         request.CompetitorPrices.Contains(cp.Competitor))
            .Select(cp => cp.BasePrice)
            .ToArray();

        double averageCompetitorPrice = filteredPrices.Length > 0
            ? filteredPrices.Average()
            : 0;

        double basePrice = _basePrices.GetValueOrDefault(request.RoomType, 0);
        double competitorAdjustment = 0.0;

        if (basePrice > 0 && averageCompetitorPrice > 0)
        {
            competitorAdjustment = (averageCompetitorPrice - basePrice) / basePrice;
            competitorAdjustment = competitorAdjustment > 0 ? 0.1 : -0.1;
        }

        competitorAdjustment = Math.Clamp(competitorAdjustment, MinCompetitorAdjustment, MaxCompetitorAdjustment);

        return competitorAdjustment;
    }

    // Gets the competitor adjustment automatically based on competitor prices and the request
    public double GetCompetitorAdjustmentAutamatcly(List<CompetitorPriceRecord> competitorPrices, PricingRequest request)
    {
        string season = ToCsvSeason(request.Season);
        if (competitorPrices == null) throw new ArgumentNullException(nameof(competitorPrices));
        if (request == null) throw new ArgumentNullException(nameof(request));
        Console.WriteLine(request.Season.ToString());
        var filteredPrices = competitorPrices
            .Where(cp => cp.RoomType == request.RoomType.ToString() && cp.Season == season)
            .Select(cp => cp.BasePrice)
            .ToArray();
        
        double averageCompetitorPrice = filteredPrices.Length > 0
            ? filteredPrices.Average()
            : 0;
        Console.WriteLine(averageCompetitorPrice);
        double basePrice = _basePrices.GetValueOrDefault(request.RoomType, 0);
        double competitorAdjustment = 0.0;

        if (basePrice > 0 && averageCompetitorPrice > 0)
        {
            competitorAdjustment = (averageCompetitorPrice - basePrice) / basePrice;
            competitorAdjustment = competitorAdjustment > 0 ? 0.1 : -0.1;
        }

        competitorAdjustment = Math.Clamp(competitorAdjustment, MinCompetitorAdjustment, MaxCompetitorAdjustment);

        return competitorAdjustment;
    }

    // Determines the factor based on the occupancy rate
    private double GetOccupancyRateFactor(int occupancyRate)
    {
        if (occupancyRate <= 30) return -0.10;
        if (occupancyRate <= 70) return 0.00;
        return 0.20;
    }

    // Checks if a competitor price is within the requested occupancy rate range
    private bool IsPriceInOccupancyRange(CompetitorPriceRecord cp, int occupancyRate)
    {
        return (occupancyRate <= 30 && cp.OccupancyRate <= 30) ||
               (occupancyRate <= 70 && cp.OccupancyRate <= 70) ||
               (occupancyRate > 70 && cp.OccupancyRate > 70);
    }

    // Method to read competitor prices from a CSV file
    public async Task<List<CompetitorPriceRecord>> GetCompetitorPricesAsync()
    {
        var records = new List<CompetitorPriceRecord>();

        try
        {
            using (var reader = new StreamReader(_csvFilePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            }))
            {
                records = csv.GetRecords<CompetitorPriceRecord>().ToList();
            }

            _logger.LogInformation($"Successfully read {records.Count} records from CSV.");
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading CSV file: {ex.Message}");
            return new List<CompetitorPriceRecord>();
        }
    }
    public async Task<List<string>> GetCompetitorNamesAsync()
    {
        var uniqueCompetitorNames = new HashSet<string>();

        try
        {
            // Define custom CSV configuration
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,  // Disables header validation
                MissingFieldFound = null, // Disables missing field validation
                IgnoreBlankLines = true, // Optional: ignores blank lines
            };

            using var reader = new StreamReader(_csvFilePath);
            using var csv = new CsvReader(reader, csvConfig);

            var records = csv.GetRecords<CompetitorPriceRecord>();

            foreach (var record in records)
            {
                uniqueCompetitorNames.Add(record.Competitor);
            }
        }
        catch (Exception ex)
        {
            // Log or handle exceptions
            throw new Exception("Error reading competitor names from CSV.", ex);
        }

        return uniqueCompetitorNames.ToList();
    }
}
