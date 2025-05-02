using APMD.Data.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

namespace APMD.Data
{
    public class DataManager
    {
        private readonly string _connectionString;
        private readonly DataTagManager _tag;
        private readonly DataPhotoManager _photo;
        private readonly DataModelManager _model;
        private readonly DataSetManager _set;
        private readonly DataDiskManager _disk;

        internal string ConnectionString => _connectionString;
        public DataTagManager Tag => _tag;
        public DataPhotoManager Photo => _photo;
        public DataModelManager Model => _model;
        public DataSetManager Set => _set;
        public DataDiskManager Disk => _disk;

        public delegate void ModelChangeHandler(object sender, EventArgsModel e);
        public event ModelChangeHandler ModelChange;

        public delegate void SetChangeHandler(object sender, EventArgsSet e);
        public event SetChangeHandler SetChange;

        public DataManager(string connectionString)
        {
            _connectionString = connectionString;

            _disk = new DataDiskManager(this);

            _tag = new DataTagManager(this);
            _photo = new DataPhotoManager(this);
            _model = new DataModelManager(this);
            _set = new DataSetManager(this);
        }

        internal void DoModelChange(object sender, EventArgsModel e)
        {
            ModelChange?.Invoke(sender, e);
        }
        internal void DoSetChange(object sender, EventArgsSet e)
        {
            SetChange?.Invoke(sender, e);
        }

    }

    public class DataTagManager
    {
        private DataManager _dataManager;
        private readonly string _connectionString;
        private TagRepository _tagsRepository;
        private TagGroupsRepository _tagGroupsRepository;
        private List<TagGroups> _tagGroups;
        public DataTagManager(DataManager dataManager)
        {
            _dataManager = dataManager;
            _connectionString = dataManager.ConnectionString;
            InitializeRepositories();
        }
        private void InitializeRepositories()
        {
            _tagsRepository = new TagRepository(_connectionString);
            _tagGroupsRepository = new TagGroupsRepository(_connectionString);
        }

        public List<TagGroups> AllGroups()
        {
            _tagGroups = _tagGroupsRepository.GetAllFull().ToList();
            return _tagGroups;
        }

        public TagGroups? GetGroupFullById(int id)
        {
            var group = _tagGroupsRepository.GetFullById(id);
            if (group == null)
            {
                Log.Error($"TagGroup record with key ${id} hasn't been found");
            }
            return group;
        }

        public void UpdateTagGroups(TagGroups updateGroup)
        {
            _tagGroupsRepository.Update(updateGroup);
        }

        public List<Tag> AllTagsForSet(Data.Set set)
        {
            set.Tags = _tagsRepository.GetAllForSet(set).ToList();
            return set.Tags;
        }

        public List<Tag> AllTagsForModel(Data.Model model)
        {
            var tags = _tagsRepository.GetAllForModel(model);
            return tags.ToList();
        }

        public void GetForSets(List<Set> sets)
        {
            if (sets == null) return;
            sets.ForEach( s => { AllTagsForSet(s); } );
        }

        public void DeleteTagGroups(TagGroups tagGroup)
        {
            _tagGroupsRepository.Delete(tagGroup.PK_TAGGROUP_ID);
        }

        public void InsertTagGroups(TagGroups tagGroup)
        {
            _tagGroupsRepository.Insert(tagGroup);
        }

        public void DeleteTag(Tag tag)
        {
            _tagsRepository.Delete(tag.PK_TAG_ID);
        }

        public async void InsertTag(Tag tag)
        {
            _tagsRepository.InsertTag(tag);
        }

        public async void UpdateTag(Tag tag)
        {
            await _tagsRepository.UpdateTagAsync(tag);
        }
    }

    public class DataPhotoManager
    {
        private DataManager _dataManager;
        private readonly string _connectionString;
        private PhotoRepository _photoRepository;
        private ModelsRepository _modelsRepository;
        private SetsRepository _setsRepository;
        public DataPhotoManager(DataManager dataManager)
        {
            _dataManager = dataManager;
            _connectionString = dataManager.ConnectionString;
            InitializeRepositories();
        }
        private void InitializeRepositories()
        {
            _photoRepository = new PhotoRepository(_connectionString);
        }

        public Photo? GetById(int value)
        {
            return _photoRepository.GetById(value);
        }

        public Photo? GetForSet(Set set)
        {
            if (set.SetPhoto == null)
            {
                set.Photos = _photoRepository.GetSetPhotos(set.PK_SET_ID);
                if (set.Photos != null && set.Photos.Count > 0)
                {
                    set.SetPhoto = set.Photos.First();
                }
            }
            return set.SetPhoto;
        }

        public bool PhotoExists(Photo photo)
        {
            try
            {
                return File.Exists(photo.PhotoFile);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Photo? GetForModel(Model model)
        {
            if (model.ModelPhoto != null) return model.ModelPhoto;

            if (model.Sets == null) 
                model.Sets = _dataManager.Set.GetAllForModel(model.PK_MODEL_ID);

            if (model.Sets != null)
            {
                if (model.Sets[0].SetPhoto != null) return model.Sets[0].SetPhoto;
                model.Sets[0].Photos = _photoRepository.GetSetPhotos(model.Sets[0].PK_SET_ID);

                if (model.Sets[0].Photos != null && model.Sets[0].Photos.Count > 0)
                {
                    return model.Sets[0].SetPhoto = model.Sets[0].Photos.First();
                }
            }
            return null;
        }

        public void Update(Photo photo)
        {
            _photoRepository.Update(photo);
        }
    }

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
            InitializeRepositories();
        }
        private void InitializeRepositories()
        {
            _modelsRepository = new ModelsRepository(_connectionString);
            _setsRepository = new SetsRepository(_connectionString);
        }

        public List<Model> GetAll()
        {
            return _modelsRepository.GetAll().Result.ToList();
        }

        public Model? GetById(int keyModel)
        {
            return _modelsRepository.GetById(keyModel);
        }

        public List<Model> GetByICG(Model currentModel, bool includeNull = false)
        {
            if (!includeNull && String.IsNullOrEmpty(currentModel.ICG))
                return new List<Model>();
            return _modelsRepository.GetByICG(currentModel.ICG);
        }

        public void GetChildren(Model currentModel)
        {
            if (currentModel.FK_MAIN_MODEL_ID != null)
                currentModel.MainModel = GetById(currentModel.FK_MAIN_MODEL_ID.Value);
            else
                currentModel.MainModel = null;
            currentModel.Models = GetByICG(currentModel);
            currentModel.Sets = _setsRepository.GetAllForModel(currentModel.PK_MODEL_ID).ToList();
            currentModel.Sets.ForEach(s => { s.Models = _dataManager.Model.GetAllForSet(s.PK_SET_ID); });
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

        internal List<Model> GetModelsForSet(Set s)
        {
            throw new NotImplementedException();
        }

        internal List<Model> GetAllForSet(int pK_SET_ID)
        {
           return _modelsRepository.GetAllForSet(pK_SET_ID);
        }
    }

    public class DataSetManager
    {
        private DataManager _dataManager;
        private readonly string _connectionString;
        private SetsRepository _setsRepository;
        public DataSetManager(DataManager dataManager)
        {
            _dataManager = dataManager;
            _connectionString = dataManager.ConnectionString;
            InitializeRepositories();
        }
        private void InitializeRepositories()
        {
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
            return null;
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

        public object? GetById(int keySet)
        {
            var result = _setsRepository.GetById(keySet);
            if (result == null)
            {
                Log.Error($"Set record with key ${keySet} hasn't been found");
            }
            return result;
        }

        public void Update(Set currentSet)
        {
            try
            {
                var result = _setsRepository.Update(currentSet);
                if (result == 0)
                {
                    Log.Error($"Set record with key ${currentSet.PK_SET_ID} hasn't been updated");
                    return;
                }
                _dataManager.DoSetChange(this, new EventArgsSet(currentSet,DataAction.Update));
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

    }

    public class DataDiskManager
    {
        private DataManager _dataManager;
        private readonly string _connectionString;
        private DiskCollection _diskCollection;
        public DataDiskManager(DataManager dataManager)
        {
            _dataManager = dataManager;
            _connectionString = dataManager.ConnectionString;
            InitializeRepositories();
        }
        private void InitializeRepositories()
        {
            _diskCollection = new DiskCollection(_connectionString);
        }
    }

    public enum DataAction
    {
        Insert,
        Update,
        Delete
    }

    public class EventArgsModel
    {
        public DataAction Action { get; set; }
        public Model Model { get; set;}
        public EventArgsModel(Model model, DataAction action)
        {
            Model = model;
            Action = action;
        }
    }

    public class EventArgsSet
    {
        public DataAction Action { get; set; }
        public Set Set { get; set; }
        public EventArgsSet(Set set, DataAction action) 
        { 
            Set = set;
            Action = action;
        }
    }


}