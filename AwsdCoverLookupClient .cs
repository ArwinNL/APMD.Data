using AWSD.Repositories;
using AWSD.Entities;
using AWSD.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using NPoco;

namespace APMD.Data.Services.Covers;

public interface IAwsdCoverLookupClient
{
    Task<AwsdCoverSearchResult> FindCoverAsync(
        AwsdCoverSearchRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class AwsdCoverSearchRequest
{
    public int SetId { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime? PublishedAt { get; init; }
    public int? WebsiteId { get; init; }
    public string WebsiteName { get; init; } = string.Empty;
    public IReadOnlyList<string> ModelNames { get; init; } = Array.Empty<string>();
}

public sealed class AwsdCoverSearchResult
{
    public bool Found { get; init; }
    public long? CoverNumber { get; init; }
    public string? Reason { get; init; }
}

public sealed class AwsdCoverLookupClient : IAwsdCoverLookupClient
{
    private readonly PhotosetRepository _photosetRepo = new();
    private readonly ModelRepository _modelRepo = new();
    private readonly WebsiteRepository _websiteRepo = new();
    private IDatabase db;

    public AwsdCoverLookupClient()
    {
        // Initialize DbFactory with connection string from configuration
        db = DbFactory.Create();
    }

    public async Task<AwsdCoverSearchResult> FindCoverAsync(
        AwsdCoverSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        // NOTE: NPoco is sync → dus wrap in Task.Run
        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            //var all = _photosetRepo.GetAll();
            //db = AWSD.Data.DbFactory.Create();


            var all = db.Fetch<PhotosetEntity>(
                "SELECT * FROM Photoset WHERE LOWER(Title) LIKE @0 LIMIT 50",
                $"%{request.Title}%");

            all.AddRange(db.Fetch<PhotosetEntity>(
                "SELECT * FROM Photoset WHERE LOWER(Models) LIKE @0 LIMIT 50",
                $"%{request.ModelNames[0]}%"));

            all.AddRange(db.Fetch<PhotosetEntity>(
                "SELECT * FROM Photoset WHERE Coverdate LIKE @0 LIMIT 50",
                $"%{request.PublishedAt?.ToString("yyyy-MM-dd")}%"));

        if (all.Count > 0)
            {
                // 1. Exact title
                var exact = all.FirstOrDefault(p =>
                    string.Equals(p.Title, request.Title, StringComparison.OrdinalIgnoreCase));

                if (exact != null)
                    return Result(exact, "Exact title");

                // 2. Title + date (±1 dag)
                if (request.PublishedAt.HasValue)
                {
                    var dateMatch = all.FirstOrDefault(p =>
                        string.Equals(p.Title, request.Title, StringComparison.InvariantCultureIgnoreCase) &&
                        Math.Abs((p.CoverDate - request.PublishedAt.Value).TotalDays) <= 1);

                    if (dateMatch != null)
                        return Result(dateMatch, "Title + date");
                }

                // 2.1 Date + model
                if (request.PublishedAt.HasValue)
                {
                    var dateMatchModel = all.FirstOrDefault(p =>
                        string.Equals(p.Models, request.ModelNames[0], StringComparison.InvariantCultureIgnoreCase) &&
                        Math.Abs((p.CoverDate - request.PublishedAt.Value).TotalDays) <= 1);
                    if (dateMatchModel != null)
                        return Result(dateMatchModel, "Model + date");
                }


            // 3. Title + model overlap
            if (request.ModelNames.Any())
                {
                    var normalizedModels = request.ModelNames
                        .Select(Normalize)
                        .ToList();

                    foreach (var p in all)
                    {
                        if (!TitleSimilar(p.Title, request.Title))
                            continue;

                        var modelString = p.Models ?? string.Empty;

                        if (normalizedModels.Any(m =>
                            Normalize(modelString).Contains(m)))
                        {
                            return Result(p, "Title + model match");
                        }
                    }
                }

                // 4. Contains fallback
                var contains = all.FirstOrDefault(p =>
                    Normalize(p.Title).Contains(Normalize(request.Title)));

                if (contains != null)
                    return Result(contains, "Contains fallback");

            }

            return new AwsdCoverSearchResult
            {
                Found = false,
                Reason = "No match"
            };

        }, cancellationToken);
    }

    private static AwsdCoverSearchResult Result(PhotosetEntity p, string reason)
    {
        return new AwsdCoverSearchResult
        {
            Found = true,
            CoverNumber = p.Id,
            Reason = reason
        };
    }

    private static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return new string(input
            .ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
            .ToArray());
    }

    private static bool TitleSimilar(string a, string b)
    {
        var na = Normalize(a);
        var nb = Normalize(b);

        return na.Contains(nb) || nb.Contains(na);
    }

    public static int LookupWebsiteId(string name)
    {
        // Lookup website by name (case-insensitive)
        var website = new WebsiteRepository().EnsureWebsite(name);
        return website;
    }

    public void Cleanup()
    {
        db.Dispose();
    }
}