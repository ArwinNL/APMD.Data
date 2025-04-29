using MySqlConnector;

namespace APMD.Data
{
    public class DiskCollection
    {
        private DisksRepository _disksRepository;
        public List<Disk> Disks { get; set; } = new List<Disk>();

        public DiskCollection(string connectionString)
        {
            _disksRepository = new DisksRepository(connectionString);
            Disks = _disksRepository.GetAll().ToList();
        }
        public DiskCollection(System.Data.IDbConnection connection)
        {
            _disksRepository = new DisksRepository(connection);
            Disks = _disksRepository.GetAll().ToList();
        }

    }

    public class Disk
    {
        public int PK_DISK_ID { get; set; }
        public required string Name { get; set; }
        public required string Root_path { get; set; }
    }
}