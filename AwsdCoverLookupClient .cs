using AWSD.Data;
using AWSD.Entities;
using AWSD.Repositories;
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

public sealed class AwsdCoverLookupClient : IAwsdCoverLookupClient, IDisposable
{
    public async Task<AwsdCoverSearchResult> FindCoverAsync(
        AwsdCoverSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var db = DbFactory.Create();

            var all = new List<PhotosetEntity>();

            var normalizedTitle = request.Title.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedTitle))
            {
                all.AddRange(db.Fetch<PhotosetEntity>(@"
                    SELECT DISTINCT p.*
                    FROM Photoset p
                    WHERE LOWER(p.Title) LIKE @0
                    LIMIT 50",
                    $"%{normalizedTitle.ToLowerInvariant()}%"));
            }

            if (request.ModelNames != null &&
                request.ModelNames.Count > 0 &&
                !string.IsNullOrWhiteSpace(request.ModelNames[0]))
            {
                var modelName = request.ModelNames[0].Trim();

                all.AddRange(db.Fetch<PhotosetEntity>(@"
                    SELECT DISTINCT p.*
                    FROM Photoset p
                    INNER JOIN `Photoset-Model` pm ON pm.ID_PHOTOSET = p.ID_PHOTOSET
                    INNER JOIN Model m ON m.ID_MODEL = pm.ID_MODEL
                    WHERE LOWER(m.Model) LIKE @0
                    LIMIT 50",
                    $"%{modelName.ToLowerInvariant()}%"));
            }

            if (request.PublishedAt.HasValue)
            {
                var date = request.PublishedAt.Value.Date;

                all.AddRange(db.Fetch<PhotosetEntity>(@"
                    SELECT DISTINCT p.*
                    FROM Photoset p
                    WHERE p.CoverDate BETWEEN @0 AND @1
                    LIMIT 50",
                    date.AddDays(-1), date.AddDays(1)));
            }

            if (request.WebsiteId.HasValue)
            {
                all.AddRange(db.Fetch<PhotosetEntity>(@"
                    SELECT DISTINCT p.*
                    FROM Photoset p
                    WHERE p.ID_WEBSITE = @0
                    LIMIT 50",
                    request.WebsiteId.Value));
            }
            else if (!string.IsNullOrWhiteSpace(request.WebsiteName))
            {
                all.AddRange(db.Fetch<PhotosetEntity>(@"
                    SELECT DISTINCT p.*
                    FROM Photoset p
                    INNER JOIN Website w ON w.ID_WEBSITE = p.ID_WEBSITE
                    WHERE LOWER(w.Website) = LOWER(@0)
                    LIMIT 50",
                    request.WebsiteName.Trim()));
            }

            all = all
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .ToList();

            if (all.Count == 0)
            {
                return new AwsdCoverSearchResult
                {
                    Found = false,
                    Reason = "No match"
                };
            }

            var modelNames = request.ModelNames?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(Normalize)
                .ToList() ?? new List<string>();

            var candidateIdsNeedingModels = all.Select(x => x.Id).ToList();
            var modelsByPhotosetId = LoadModelsByPhotosetId(db, candidateIdsNeedingModels);

            var exact = all.FirstOrDefault(p =>
                string.Equals(p.Title, request.Title, StringComparison.OrdinalIgnoreCase));

            if (exact != null)
                return Result(exact, "Exact title");

            if (request.PublishedAt.HasValue)
            {
                var dateMatch = all.FirstOrDefault(p =>
                    string.Equals(p.Title, request.Title, StringComparison.OrdinalIgnoreCase) &&
                    Math.Abs((p.CoverDate.Date - request.PublishedAt.Value.Date).TotalDays) <= 1);

                if (dateMatch != null)
                    return Result(dateMatch, "Title + date");
            }

            if (request.PublishedAt.HasValue && modelNames.Count > 0)
            {
                var dateModelMatch = all.FirstOrDefault(p =>
                    Math.Abs((p.CoverDate.Date - request.PublishedAt.Value.Date).TotalDays) <= 1 &&
                    modelsByPhotosetId.TryGetValue(p.Id, out var photoModels) &&
                    photoModels.Any(pm => modelNames.Contains(Normalize(pm))));

                if (dateModelMatch != null)
                    return Result(dateModelMatch, "Model + date");
            }

            if (modelNames.Count > 0)
            {
                foreach (var p in all)
                {
                    if (!TitleSimilar(p.Title, request.Title))
                        continue;

                    if (!modelsByPhotosetId.TryGetValue(p.Id, out var photoModels))
                        continue;

                    var normalizedPhotoModels = photoModels.Select(Normalize).ToList();

                    if (modelNames.Any(m => normalizedPhotoModels.Any(pm => pm.Contains(m) || m.Contains(pm))))
                        return Result(p, "Title + model match");
                }
            }

            var contains = all.FirstOrDefault(p =>
                Normalize(p.Title).Contains(Normalize(request.Title)));

            if (contains != null)
                return Result(contains, "Contains fallback");

            return new AwsdCoverSearchResult
            {
                Found = false,
                Reason = "No match"
            };
        }, cancellationToken);
    }

    private static Dictionary<long, List<string>> LoadModelsByPhotosetId(IDatabase db, IReadOnlyCollection<long> photosetIds)
    {
        var result = new Dictionary<long, List<string>>();

        if (photosetIds.Count == 0)
            return result;

        var sql = @"
            SELECT pm.ID_PHOTOSET, m.Model
            FROM `Photoset-Model` pm
            INNER JOIN Model m ON m.ID_MODEL = pm.ID_MODEL
            WHERE pm.ID_PHOTOSET IN (@0)";

        var rows = db.Fetch<PhotosetModelRow>(sql, photosetIds.ToArray());

        foreach (var row in rows)
        {
            if (!result.TryGetValue(row.ID_PHOTOSET, out var list))
            {
                list = new List<string>();
                result[row.ID_PHOTOSET] = list;
            }

            if (!string.IsNullOrWhiteSpace(row.Model))
                list.Add(row.Model);
        }

        return result;
    }

    private sealed class PhotosetModelRow
    {
        public long ID_PHOTOSET { get; set; }
        public string Model { get; set; } = string.Empty;
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

    public void Dispose()
    {
    }
    public static int LookupWebsiteId(string name)
    {
        // Lookup website by name (case-insensitive)
        var website = new WebsiteRepository().EnsureWebsite(name);
        return website;
    }

}