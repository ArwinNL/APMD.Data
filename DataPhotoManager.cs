namespace APMD.Data
{
    public class DataPhotoManager
    {
        private DataManager _dataManager;
        private readonly string _connectionString;
        private PhotoRepository _photoRepository;
        //private ModelsRepository _modelsRepository;
        //private SetsRepository _setsRepository;

        public DataPhotoManager(DataManager dataManager)
        {
            _dataManager = dataManager;
            _connectionString = dataManager.ConnectionString;
            _photoRepository = new PhotoRepository(_connectionString);
            //_modelsRepository = new ModelsRepository(_connectionString);
            //_setsRepository = new SetsRepository(_connectionString);
        }

        public Photo? GetById(long value)
        {
            return _photoRepository.GetById(value);
        }

        public Photo? GetForSet(Set set)
        {
            if (set.SetPhoto == null)
            {
                if (set.FK_PHOTO_ID != null)
                {
                    set.SetPhoto = GetById(set.FK_PHOTO_ID.Value);
                    if (set.SetPhoto != null) return set.SetPhoto;
                }
                GetAllForSet(set);
                if (set.Photos != null && set.Photos.Count > 0)
                {
                    set.SetPhoto = set.Photos.First();
                }
            }
            return set.SetPhoto;
        }

        public void GetAllForSet(Set set)
        {
            var result = _photoRepository.GetAllForSet(set.PK_SET_ID);
            if (set.Archived)
                result.ForEach(p => { p.Archived = set.Archived; Photo.ArchiveServerShare = _dataManager.ServerShare.ArchiveServerShare; } );
            set.Photos = result;
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

            if (model.Sets == null || model.Sets.Count < 1)
                model.Sets = _dataManager.Set.GetAllForModel(model.PK_MODEL_ID);

            if (model.Sets != null && model.Sets.Count > 0)
            {
                if (model.Sets[0].SetPhoto != null) return model.Sets[0].SetPhoto;
                model.Sets[0].Photos = _photoRepository.GetAllForSet(model.Sets[0].PK_SET_ID);

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

        internal void GetForSets(List<Set> sets)
        {
            foreach (var set in sets)
            {
                GetForSet(set);
            }
        }

        public void Delete(Photo photo)
        {
            _photoRepository.Delete(photo.PK_PHOTO_ID);
        }

        public void Insert(Photo photo)
        {
            _photoRepository.Insert(photo);
        }

        public void SetUpdate(List<Photo> photos)
        {
            _photoRepository.BulkUpdate(photos);
        }

        public void SetInsert(List<Photo> photos)
        {
            for (int i = 0; i < photos.Count; i++)
            {
                _photoRepository.Insert(photos[i]);
            }
        }
    }


}