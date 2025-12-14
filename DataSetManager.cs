using APMD.Data.Models;
using Serilog;

namespace APMD.Data
{
    public class DataSetManager
    {
        private DataManager _dataManager;
        private readonly string _connectionString;
        private SetsRepository _setsRepository;
        public DataSetManager(DataManager dataManager)
        {
            _dataManager = dataManager;
            _connectionString = dataManager.ConnectionString;
            _setsRepository = new SetsRepository(_connectionString);
        }

        public void GetSetModels(List<Set> sets)
        {
            if (sets == null) return;
            sets.ForEach(s => { s.Models = _dataManager.Model.GetAllForSet(s.PK_SET_ID); });
        }

        public SetTags AddTag(Tag tag, Set set)
        {
            if (!set.Tags.Contains(tag))
            {
                try
                {
                    var setTag =_setsRepository.AddTagToSet(tag, set);
                    _dataManager.Tag.AllTagsForSet(set);
                    _dataManager.DoSetChange(this, new EventArgsSet(set, DataAction.Update));
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                    throw;
                }
                
            }
            return new SetTags
            {
                FK_SET_ID = set.PK_SET_ID,
                FK_TAG_ID = tag.PK_TAG_ID
            };
        }

        public List<Set> GetAllForModel(int keyModel)
        {
            return _setsRepository.GetAllForModel(keyModel).ToList();
        }

        public void RemoveTag(Tag tag, Set currentSet)
        {
            try
            {
                _setsRepository.DeleteReference(tag, currentSet);
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it, show a message, etc.)
                Console.WriteLine($"Error removing tag: {ex.Message}");
            }
            if (currentSet.Tags != null && currentSet.Tags.Contains(tag))
            {
                currentSet.Tags.Remove(tag);
                _dataManager.DoSetChange(this, new EventArgsSet(currentSet, DataAction.Delete));
            }
        }

        public Set? GetById(int keySet)
        {
            var result = _setsRepository.GetById(keySet);
            if (result == null)
            {
                Log.Error($"Set record with key ${keySet} hasn't been found");
            }
            return result;
        }

        public void Update(Set set, bool skipUpdates = false)
        {
            try
            {
                var result = _setsRepository.Update(set);
                if (result == 0)
                {
                    var msgError = $"Set record with key ${set.PK_SET_ID} hasn't been updated";
                    Log.Error(msgError);
                    throw new KeyNotFoundException($"Set with ID {set.PK_SET_ID} not found.");
                }
                // Update models in the set
                UpdateSetModels(set);
                if (skipUpdates)
                    return;
                _dataManager.Photo.GetAllForSet(set);
                _dataManager.Tag.AllTagsForSet(set);
                _dataManager.DoSetChange(this, new EventArgsSet(set,DataAction.Update));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public void Insert(Set set)
        {
            try
            {
                var result = _setsRepository.Insert(set);
                _dataManager.DoSetChange(this, new EventArgsSet(set, DataAction.Insert));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        public void UpdateSetModels(Set currentSet)
        {
            var currentModels = _dataManager.Model.GetAllForSet(currentSet.PK_SET_ID);
            // Remove models that are no longer in the set
            foreach (var model in currentModels)
            {
                if (!currentSet.Models.Any(m => m.PK_MODEL_ID == model.PK_MODEL_ID))
                {
                    _dataManager.Set.RemoveModelFromSet(model.PK_MODEL_ID, currentSet.PK_SET_ID);
                }
            }
            // Add new models to the set
            foreach (var model in currentSet.Models)
            {
                if (!currentModels.Any(m => m.PK_MODEL_ID == model.PK_MODEL_ID))
                {
                    _dataManager.Set.AddModelToSet(model.PK_MODEL_ID, currentSet.PK_SET_ID);
                }
            }
        }

        private void AddModelToSet(int pK_MODEL_ID, long pK_SET_ID)
        {
            _setsRepository.AddModelToSet(pK_MODEL_ID, pK_SET_ID);
        }

        private void RemoveModelFromSet(int pK_MODEL_ID, long pK_SET_ID)
        {
            _setsRepository.RemoveModelFromSet(pK_MODEL_ID, pK_SET_ID);
        }

        public bool Delete(Set set)
        {
            try
            {
                try
                {
                    if (set.Photos == null || set.Photos.Count == 0)
                        _dataManager.Photo.GetAllForSet(set);
                    if (set.Photos != null && set.Photos.Count > 0)
                    { 
                        foreach (var photo in set.Photos)
                        {
                            _dataManager.Photo.Delete(photo);
                        }
                    }
                    _dataManager.Tag.DeleteTagsForSet(set.PK_SET_ID);
                    _dataManager.Model.DeleteModelsForSet(set.PK_SET_ID);
                    _setsRepository.Delete(set.PK_SET_ID);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error deleting photos for set {set.PK_SET_ID}: {ex.Message}", ex);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return false;
            }
        }

        public async Task GetSetInfo(List<Model> models)
        {
            var result = await _setsRepository.GetSetInfo();

            var modelDict = result.ToDictionary(m => m.PK_MODEL_ID );

            foreach (var model in models)
            {
                if (modelDict.TryGetValue(model.PK_MODEL_ID, out var modelStats))
                {
                    model.NumSets = Convert.ToInt32(modelStats.NumSets);
                    model.NumPhotos = Convert.ToInt32(modelStats.NumPhotos);
                }
            }
        }

        public async Task GetSetInfo(Model model)
        {
            var result = await _setsRepository.GetSetInfo(model.PK_MODEL_ID);
            var modelStats = result.FirstOrDefault();

            if (modelStats != null)
            {
                model.NumSets = Convert.ToInt32(modelStats.NumSets);
                model.NumPhotos = Convert.ToInt32(modelStats.NumPhotos);
            }
        }

        public PagedResult<Set> GetAllUnassigned(int pageSize, int offset)
        {
            var result = _setsRepository.GetAllUnassigned(pageSize,offset);
            return result;
        }

        public void GetAllDetails(Set currentSet)
        {
            _dataManager.Website.GetForSet(currentSet);
            _dataManager.Photo.GetForSet(currentSet);
            _dataManager.Photo.GetAllForSet(currentSet);
            _dataManager.Model.GetAllForSet(currentSet);
            _dataManager.Tag.AllTagsForSet(currentSet);
        }

        public IEnumerable<Set> Search(string search)
        {
            return _setsRepository.Search(search);
        }

        public IEnumerable<Set> GetAllArchived()
        {
            return _setsRepository.GetAllArchived();
        }

        public List<Set> GetSetsWithDuplicateItems()
        {
            return _setsRepository.GetDoubled();
        }
    }


}