using Serilog;
using System.Text.RegularExpressions;

namespace APMD.Data
{
    public class DataModelManager
    {
        private DataManager _dataManager;
        private readonly string _connectionString;
        private ModelsRepository _modelsRepository;
        private SetsRepository _setsRepository;
        public DataModelManager(DataManager dataManager)
        {
            _dataManager = dataManager;
            _connectionString = dataManager.ConnectionString;
            _modelsRepository = new ModelsRepository(_connectionString);
            _setsRepository = new SetsRepository(_connectionString);
        }

        public async Task<IEnumerable<Model>> GetAll(List<Model> loadedModels, int count = -1, int skip = 0)
        {
            return await _modelsRepository.GetAll(loadedModels, count, skip).ConfigureAwait(false);
        }

        public Model? GetById(int keyModel)
        {
            return _modelsRepository.GetById(keyModel);
        }

        public List<Model> GetByICG(Model currentModel, bool includeNull = false)
        {
            if (!includeNull && String.IsNullOrEmpty(currentModel.ICG) || currentModel == null)
                return new List<Model>();
            if (String.IsNullOrEmpty(currentModel.ICG))
                return new List<Model>();
            return _modelsRepository.GetByICG(currentModel.ICG);
        }

        public void GetChildren(Model? currentModel)
        {
            if (currentModel == null) return;

            if (currentModel.FK_MAIN_MODEL_ID != null)
                currentModel.MainModel = GetById(currentModel.FK_MAIN_MODEL_ID.Value);
            else
                currentModel.MainModel = null;
            currentModel.Models = GetByICG(currentModel);
            currentModel.Sets = _setsRepository.GetAllForModel(currentModel.PK_MODEL_ID).ToList();
            _dataManager.Photo.GetForSets(currentModel.Sets);
            _dataManager.Tag.GetForSets(currentModel.Sets);
            currentModel.Sets.ForEach(s => { s.Models = _dataManager.Model.GetAllForSet(s.PK_SET_ID); });
            if (currentModel.FK_PHOTO_ID.HasValue && currentModel.FK_PHOTO_ID.Value > 0)
                currentModel.ModelPhoto = _dataManager.Photo.GetById(currentModel.FK_PHOTO_ID.Value);
        }

        public void Update(Model currentModel)
        {
            try
            {
                _modelsRepository.Update(currentModel);
                _dataManager.DoModelChange(this, new EventArgsModel(currentModel, DataAction.Update));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public void Insert(Model currentModel)
        {
            try
            {
                _modelsRepository.Insert(currentModel);
                _dataManager.DoModelChange(this, new EventArgsModel(currentModel, DataAction.Insert));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public void Delete(Model currentModel)
        {
            try
            {
                _modelsRepository.Delete(currentModel.PK_MODEL_ID);
                _dataManager.DoModelChange(this, new EventArgsModel(currentModel, DataAction.Delete));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        internal void GetModelsForSet(Set s)
        {
            s.Models = GetAllForSet(s.PK_SET_ID);
        }

        internal void GetAllForSet(Set set)
        {
            set.Models = _modelsRepository.GetAllForSet(set.PK_SET_ID);
        }
        internal List<Model> GetAllForSet(long pK_SET_ID)
        {
           return _modelsRepository.GetAllForSet(pK_SET_ID);
        }

        public bool CompareEqual(Model currentModel, bool save = false)
        {
            var modelDb = _modelsRepository.GetById(currentModel.PK_MODEL_ID);
            if (modelDb == null)
            {
                Log.Error($"Model record with key ${currentModel.PK_MODEL_ID} hasn't been found");
                return false;
            }
            else
            {
                if (currentModel.HasMappedChanges(modelDb))
                {
                    if (save)
                    {
                        _modelsRepository.Update(currentModel);
                        _dataManager.DoModelChange(this, new EventArgsModel(currentModel, DataAction.Update));
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        internal void DeleteModelsForSet(long pK_SET_ID)
        {
            var models = _modelsRepository.GetAllForSet(pK_SET_ID);
            int count = 0;
            models.ForEach(m => { count += _modelsRepository.Delete(m.PK_MODEL_ID); });
            if (count == models.Count) return;
            var msgError = $"Not all models have been deleted for set {pK_SET_ID}";
            Log.Error(msgError);
            throw new Exception(msgError);
        }

        public async Task GetAllDetails(Model model)
        {
            await _dataManager.Set.GetSetInfo(model);
            _dataManager.Photo.GetForModel(model);
        }

        public void AddToSet(Model clickedModel, Set currentSet)
        {
            _setsRepository.AddModelToSet(clickedModel.PK_MODEL_ID, currentSet.PK_SET_ID);
            currentSet.Models.Add(clickedModel);
        }

        public void RemoveFromSet(Model clickedModel, Set currentSet)
        {
            _setsRepository.RemoveModelFromSet(clickedModel.PK_MODEL_ID, currentSet.PK_SET_ID);
        }

        public List<Model> SearchOnWords(string searchText, Data.Websites website)
        {
            // Extract the part between [ and ] as one word
            var words = Regex.Matches(searchText, @"\[(.*?)\]")
                .Select(m => m.Groups[1].Value)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            // remove the extracted parts from the original string
            searchText = Regex.Replace(searchText, @"\[(.*?)\]", " ");
            searchText = searchText.Replace(" and ", "");
            searchText = Regex.Replace(searchText, @"[-&]", " ");   // replace those chars with space
            searchText = Regex.Replace(searchText, @"\b\w\b", "").Trim();

            // Split the search text into individual words
            words.AddRange(searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            var result = new List<Model>();
            foreach (var word in words)
            {
                Log.Information($"Searching for websites with word: {word}");
                if (website != null)
                {
                    var searchResult = _modelsRepository.SearchOnWord(word, website).ToList();
                    if (searchResult.Count == 0)
                    {
                        Log.Information($"No models found for word '{word}' on website '{website.Name}'. Searching globally.");
                        searchResult = _modelsRepository.SearchOnWord(word).ToList();
                    }
                    result.AddRange(searchResult);
                }
                else
                    result.AddRange([.. _modelsRepository.SearchOnWord(word)]);
            }
            return result.Distinct().ToList(); // Remove duplicates if any

        }
    }


}