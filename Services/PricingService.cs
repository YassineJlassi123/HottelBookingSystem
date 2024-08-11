using CsvHelper.Configuration;
using CsvHelper;
using HotelPricingEngine.Models;
using System.Globalization;

public class PricingService
{
    private readonly ILogger<PricingService> _logger;
    private readonly string _csvFilePath = "historical.csv"; // Path to the CSV file for competitor prices

    public PricingService(ILogger<PricingService> logger)
    {
        _logger = logger;
    }
    // Base prices for different room types
    private readonly Dictionary<string, double> _basePrices = new Dictionary<string, double>
    {
        { "Standard", 90 },
        { "Deluxe", 140 },
        { "Suite", 240 }
    };

    // Seasonality factors for adjusting prices
    private readonly Dictionary<string, double> _seasonalityFactors = new Dictionary<string, double>
    {
        { "Off-Season", -0.20 },
        { "Peak Season", 0.30 }
    };

    // Constants to define the range for competitor price adjustments
    private const double MinCompetitorAdjustment = -0.10;
    private const double MaxCompetitorAdjustment = 0.10;

    // Calculates the adjusted price based on various factors
    public PricingResponse CalculateAdjustedPrice(PricingRequest request, double competitorAdjustment)
    {
        // Check if request is null and throw an exception
        if (request == null) throw new ArgumentNullException(nameof(request));

        // Retrieve base price for the room type, throw an exception if not found
        if (!_basePrices.TryGetValue(request.RoomType, out var basePrice))
        {
            throw new ArgumentException("Invalid room type", nameof(request.RoomType));
        }

        // Get the seasonality factor from the dictionary or default to 0
        double seasonalityFactor = _seasonalityFactors.GetValueOrDefault(request.Season, 0);
        // Calculate occupancy rate factor
        double occupancyRateFactor = GetOccupancyRateFactor(request.OccupancyRate);

        // Calculate the adjusted price using the base price and adjustment factors
        double adjustedPrice = basePrice * (1 + seasonalityFactor + occupancyRateFactor + competitorAdjustment);

        // Return the final pricing response
        return new PricingResponse
        {
            RoomType = request.RoomType,
            BasePrice = basePrice,
            AdjustedPrice = Math.Round(adjustedPrice, 2) // Round to 2 decimal places
        };
    }

    // Gets the competitor adjustment based on competitor prices and the request
    public double GetCompetitorAdjustment(List<CompetitorPriceRecord> competitorPrices, PricingRequest request)
    {
        // Check if competitorPrices and request are not null
        if (competitorPrices == null) throw new ArgumentNullException(nameof(competitorPrices));
        if (request == null) throw new ArgumentNullException(nameof(request));

        // Filter and select competitor prices that match the criteria
        var filteredPrices = competitorPrices
            .Where(cp => cp.RoomType == request.RoomType &&
                         cp.Season == request.Season &&
                         IsPriceInOccupancyRange(cp, request.OccupancyRate) &&
                         request.CompetitorPrices.Contains(cp.Competitor))
            .Select(cp => cp.BasePrice)
            .ToArray();

        // Calculate the average competitor price
        double averageCompetitorPrice = filteredPrices.Length > 0
            ? filteredPrices.Average()
            : 0;

        // Calculate the adjustment factor based on the average competitor price
        double competitorAdjustment = averageCompetitorPrice > 0
            ? (averageCompetitorPrice - _basePrices.GetValueOrDefault(request.RoomType, 0)) / _basePrices.GetValueOrDefault(request.RoomType, 1)
            : 0;
        competitorAdjustment = Math.Round(competitorAdjustment * 10) / 10;

        // Apply a constraint to the adjustment factor
        competitorAdjustment = Math.Clamp(competitorAdjustment, MinCompetitorAdjustment, MaxCompetitorAdjustment);

        return competitorAdjustment;
    }

    public double GetCompetitorAdjustmentAutamatcly(List<CompetitorPriceRecord> competitorPrices, PricingRequest request)
    {
        // Check if competitorPrices and request are not null
        if (competitorPrices == null) throw new ArgumentNullException(nameof(competitorPrices));
        if (request == null) throw new ArgumentNullException(nameof(request));

        // Filter and select competitor prices that match the criteria
        var filteredPrices = competitorPrices
            .Where(cp => cp.RoomType == request.RoomType &&
                         cp.Season == request.Season
                        )
            .Select(cp => cp.BasePrice)
            .ToArray();

        // Calculate the average competitor price
        double averageCompetitorPrice = filteredPrices.Length > 0
            ? filteredPrices.Average()
            : 0;
        Console.WriteLine(averageCompetitorPrice);

        // Calculate the adjustment factor based on the average competitor price
        double competitorAdjustment = averageCompetitorPrice > 0
            ? (averageCompetitorPrice - _basePrices.GetValueOrDefault(request.RoomType, 0)) / _basePrices.GetValueOrDefault(request.RoomType, 1)
            : 0;
        competitorAdjustment = Math.Round(competitorAdjustment * 10) / 10;

        // Apply a constraint to the adjustment factor
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
                HeaderValidated = null, // Disable header validation
                MissingFieldFound = null // Ignore missing fields
            }))
            {
                records = csv.GetRecords<CompetitorPriceRecord>().ToList(); // Read records from CSV
            }

            _logger.LogInformation($"Successfully read {records.Count} records from CSV."); // Log success
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading CSV file: {ex.Message}"); // Log error and return an empty list
            return new List<CompetitorPriceRecord>();
        }
    }
}