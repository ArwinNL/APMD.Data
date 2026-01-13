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
            var photo = _photoRepository.GetById(value);
            photo.ThumbnailServerShare = _dataManager.ServerShare.ThumbnailServerShare;
            return photo;
        }

        public Photo? GetForModel(Set set)
        {
            if (set.Models == null || set.Models.Count < 1)
                set.Models = _dataManager.Model.GetAllForSet(set.PK_SET_ID);

            if (set.Models != null && set.Models.Count > 0)
            {
                if (set.Models[0].ModelPhoto != null) return set.Models[0].ModelPhoto;
                if (set.Models[0].FK_PHOTO_ID != null) return GetById(set.Models[0].FK_PHOTO_ID.Value);
            }
            return null;
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
                    _dataManager.Set.Update(set);
                }
            }
            return set.SetPhoto;
        }

        public void GetAllForSet(Set set)
        {
            var result = _photoRepository.GetAllForSet(set.PK_SET_ID);
            result.ForEach(p => p.ThumbnailServerShare = _dataManager.ServerShare.ThumbnailServerShare);
            if (set.Archived)
                result.ForEach(p => { p.Archived = set.Archived; Photo.ArchiveServerShare = _dataManager.ServerShare.ArchiveServerShare; });
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

        public int Delete(Photo photo, bool removeFromSet = false)
        {
            int referenceCount = 0;
            referenceCount += _dataManager.Model.CountPhoto(photo.PK_PHOTO_ID);
            referenceCount += _dataManager.Tag.CountPhoto(photo.PK_PHOTO_ID);
            referenceCount += _dataManager.Website.CountPhoto(photo.PK_PHOTO_ID);
            if (referenceCount > 1)
            {
                throw new InvalidOperationException("Kan de foto niet verwijderen omdat er referenties zijn naar modellen en/of tags.");
            }
            if (removeFromSet)
            {
                photo.FK_SET_ID = null;
                _photoRepository.Update(photo);
            }
            return _photoRepository.Delete(photo.PK_PHOTO_ID);
        }

        public void Insert(Photo photo)
        {
            _photoRepository.Insert(photo);
            photo.ThumbnailServerShare = _dataManager.ServerShare.ThumbnailServerShare;
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