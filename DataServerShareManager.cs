using Serilog;

namespace APMD.Data
{
    public class DataServerShareManager
    {
        private DataManager _dataManager;
        private readonly string _connectionString;
        private ServerShareCollection _serverShareCollection;
        private ServerShare _archiveServerShare = null!;
        public ServerShare DefaultServerShare { get; internal set; } = null!;
        public ServerShare ArchiveServerShare
        {
            get => _archiveServerShare;
            internal set => _archiveServerShare = value;
        }
        public ServerShare ThumbnailServerShare => DefaultServerShare;

        public ServerShare GetServerShareById(int serverShareId)
        {
            if (_serverShareCollection.ServerShares.Count < 1)
            {
                Log.Error("No disks available in the collection.");
                throw new InvalidOperationException("No disks available in the collection.");
            }
            return _serverShareCollection.ServerShares.FirstOrDefault(ss => ss.PK_SERVERSHARE_ID == serverShareId, _serverShareCollection.ServerShares[0]);
        }

        public DataServerShareManager(DataManager dataManager)
        {
            _dataManager = dataManager;
            _connectionString = dataManager.ConnectionString;
            _serverShareCollection = new ServerShareCollection(_connectionString);

            // Initialize non-nullable members with a fallback
            if (_serverShareCollection.ServerShares.Count > 0)
            {
                DefaultServerShare = _serverShareCollection.ServerShares[0];
                _archiveServerShare = _serverShareCollection.ServerShares[0];
            }
            else
            {
                // If no shares, either throw or use null-forgiving assignment
                DefaultServerShare = null!;  // or throw new InvalidOperationException("No server shares");
                _archiveServerShare = null!;
            }
        }

        public ServerShare GetServerShare(string serverShare)
        {
            if (_serverShareCollection.ServerShares.Count < 1)
            {
                Log.Error("No disks available in the collection.");
                throw new InvalidOperationException("No disks available in the collection.");
            }
            return _serverShareCollection.ServerShares.FirstOrDefault(ss => ss.GetUNCPath() == serverShare, _serverShareCollection.ServerShares[0]);
        }

        public void SetDefault(string defaultDisk, string archiveServerShare)
        {
            DefaultServerShare = GetServerShare(defaultDisk);
            ArchiveServerShare = GetServerShare(archiveServerShare); // Ensure ArchiveShare is initialized
        }

        public List<ServerShare> GetAll()
        {
            return _serverShareCollection.ServerShares;
        }
    }


}