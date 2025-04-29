using APMD.Data.Models;
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



        public DataManager(string connectionString)
        {
            _connectionString = connectionString;

            _disk = new DataDiskManager(this);

            _tag = new DataTagManager(this);
            _photo = new DataPhotoManager(this);
            _model = new DataModelManager(this);
            _set = new DataSetManager(this);
        }
    }

    public class DataTagManager
    {
        private DataManager _dataManager;
        private readonly string _connectionString;
        private TagsRepository _tagsRepository;
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
            _tagsRepository = new TagsRepository(_connectionString);
            _tagGroupsRepository = new TagGroupsRepository(_connectionString);
        }

        public List<TagGroups> AllGroups()
        {
            if (_tagGroups == null)
            {
                _tagGroups = _tagGroupsRepository.GetAllFull().ToList();
            }
            return _tagGroups;
        }

        public List<Tag> AllTagsForSet(Data.Set set)
        {
            var tags = _tagsRepository.GetAllForSet(set);
            return tags.ToList();
        }

        public List<Tag> AllTagsForModel(Data.Model model)
        {
            var tags = _tagsRepository.GetAllForModel(model);
            return tags.ToList();
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

        public object? GetById(int keyModel)
        {
            return _modelsRepository.GetById(keyModel);
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

        public SetTags AddTag(Tag tag, Set set)
        {
            if (!set.Tags.Contains(tag))
            {
                return _setsRepository.AddTagToSet(tag,set);
            }
            return null;
        }

        public List<Set> GetAllForModel(int keyModel)
        {
            return _setsRepository.GetAllForModel(keyModel).ToList();
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
}