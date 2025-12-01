using Serilog;

namespace APMD.Data
{
    public class DataImportManager
    {
        private DataManager _dataManager;
        private ImportRepository _importRepository;
        private readonly string _connectionString;
        public DataImportManager(DataManager dataManager)
        {
            _dataManager = dataManager;
            _connectionString = dataManager.ConnectionString;
            _importRepository = new ImportRepository(_connectionString);
        }

        public List<ImportFolder> GetAll()
        {
            var import =_importRepository.GetAll();
            var result = new List<ImportFolder>();
            import.ForEach(i =>
            {
                var folder = new ImportFolder(i, _dataManager.Website.GetById(i.FK_WEBSITE_ID));
                result.Add(folder);
            });

            return result;
        }

        public bool SaveAll(List<ImportFolder> saveImport)
        {
            try
            {
                foreach (var si in saveImport)
                {
                    var import = new Import(si);
                    _importRepository.Insert(import);
                    si.Tag = import;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return false;
            }
        }


        public List<Import> GetUnprocessedImport()
        {
            return _importRepository.GetAllUnprocessed().ToList();
        }

        public void MarkAsProcessed(int pK_IMPORT_ID)
        {
            var import = _importRepository.GetById(pK_IMPORT_ID);
            import.Processed = true;
            _importRepository.Update(import);
            //throw new NotImplementedException();
        }

        public void Insert(Import importSet)
        {
            _importRepository.Insert(importSet);
        }

        public void UpdateAll(List<ImportFolder> saveImport)
        {
            _importRepository.UpdateAll(saveImport.Select(i => (Import)i.Tag).ToList());
        }
    }


}