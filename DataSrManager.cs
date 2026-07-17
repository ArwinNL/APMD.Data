using AWSD.Data;
using AWSD.Entities;
using AWSD.Models;
using AWSD.Repositories;
using NPoco;

namespace APMD.Data
{
    public class DataSrManager
    {
        private readonly DataManager _dataManager;
        private readonly IDatabase _db;
        private readonly PhotosetRepository _photosetRepository = new();
        private readonly ModelRepository _modelRepository = new();
        private readonly WebsiteRepository _websiteRepository = new();

        public DataSrManager(DataManager dataManager)
        {
            _dataManager = dataManager;
            _db = DbFactory.Create();
        }

        public async Task<PhotosetDetails?> GetCoverForSet(Set currentSet)
        {
            var result = await _photosetRepository.GetDetails(currentSet.FK_SR_COVER_ID);
            currentSet.TN_Cover = result;
            return result;
        }

        public List<ModelEntity> GetModelsForPhotoset(PhotosetEntity pse)
        {
            var result = _modelRepository.GetForPhotoset(pse.Id);
            return result;
        }
    }
}