using System.Text.RegularExpressions;

namespace APMD.Data
{
    public class DataWebsiteManager
    {
        private DataManager _dataManager;
        private readonly string _connectionString;
        private WebsitesRepository _websiteRepository;
        private static readonly Serilog.ILogger Log = Serilog.Log.ForContext<DataWebsiteManager>();
        private static List<Websites> _websites = new List<Websites>();

        public DataWebsiteManager(DataManager dataManager)
        {
            _dataManager = dataManager;
            _connectionString = dataManager.ConnectionString;
            _websiteRepository = new WebsitesRepository(_connectionString);
            _websites = GetAll(true);
        }

        public List<Websites> GetAll(bool sorted = false)
        {
            return _websiteRepository.GetAll(sorted).ToList();
        }
        public Websites GetById(int keyWebsite)
        {
            if (_websites.Any(w => w.PK_WEBSITE_ID == keyWebsite))
            {
                return _websites.First(w => w.PK_WEBSITE_ID == keyWebsite);
            }
            var result = _websiteRepository.GetById(keyWebsite);
            if (result == null)
            {
                Log.Error($"Website record with key ${keyWebsite} hasn't been found");
                throw new KeyNotFoundException($"Website with ID {keyWebsite} not found.");
            }
            return result;
        }

        public List<Websites> SearchOnWords(string searchText)
        {
            // Extract the part between [ and ] as one word
            var words = Regex.Matches(searchText, @"\[(.*?)\]")
                .Select(m => m.Groups[1].Value)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            // remove the extracted parts from the original string
            searchText = Regex.Replace(searchText, @"\[(.*?)\]", " ");
            searchText = Regex.Replace(searchText, @"[-&]", " ");   // replace those chars with space

            // Split the search text into individual words
            words.AddRange(searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            var result = new List<Websites>();
            foreach (var word in words)
            {
                Log.Information($"Searching for websites with word: {word}");
                result.AddRange([.. _websiteRepository.SearchOnWord(word)]);
            }
            return result.Distinct().ToList(); // Remove duplicates if any
        }

        public void GetForSet(Set? set)
        {
            if (set == null || set.FK_WEBSITE_ID == -1)
                return;
            set.Website = GetById(set.FK_WEBSITE_ID);
        }

        public void GetWebsiteForModel(Model model)
        {
            if (model == null)
                return;
            model.Websites = _websiteRepository.GetByModelId(model.PK_MODEL_ID);
        }

        public bool Update(Websites website)
        {
            try
            {
                var rowsAffected = _websiteRepository.Update(website);
                if (rowsAffected > 0)
                {
                    // Update the in-memory list
                    var index = _websites.FindIndex(w => w.PK_WEBSITE_ID == website.PK_WEBSITE_ID);
                    if (index != -1)
                    {
                        _websites[index] = website;
                    }
                    else
                    {
                        _websites.Add(website);
                    }
                    Log.Information($"Website with ID {website.PK_WEBSITE_ID} updated successfully.");
                    return true;
                }
                else
                {
                    Log.Warning($"No rows affected while updating website with ID {website.PK_WEBSITE_ID}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error updating website with ID {website.PK_WEBSITE_ID}: {ex.Message}");
                return false;
            }
        }

        public bool Insert(Websites website)
        {
            try
            {
                var rowsAffected = _websiteRepository.Insert(website);
                if (rowsAffected > 0)
                {
                    // Add to the in-memory list
                    _websites.Add(website);
                    Log.Information($"Website with ID {website.PK_WEBSITE_ID} inserted successfully.");
                    return true;
                }
                else
                {
                    Log.Warning($"No rows affected while inserting website with ID {website.PK_WEBSITE_ID}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error inserting website with ID {website.PK_WEBSITE_ID}: {ex.Message}");
                return false;
            }
        }

        public List<Websites> GetAllWebsitesForModel(Model model)
        {
            return _websiteRepository.GetByModelId(model.PK_MODEL_ID);
        }

        internal int CountPhoto(long pK_PHOTO_ID)
        {
            return _websiteRepository.CountPhoto(pK_PHOTO_ID);
        }
    }
}