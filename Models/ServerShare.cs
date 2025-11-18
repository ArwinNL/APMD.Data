
namespace APMD.Data
{
    using global::APMD.Data.Repositories;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ServerShareCollection
    {
        private ServerShareRepository _serverShareRepository;
        public List<ServerShare> ServerShares { get; set; } = new List<ServerShare>();

        public ServerShareCollection(string connectionString)
        {
            _serverShareRepository = new ServerShareRepository(connectionString);
            ServerShares = _serverShareRepository.GetAll().ToList();
        }
        public ServerShareCollection(System.Data.IDbConnection connection)
        {
            _serverShareRepository = new ServerShareRepository(connection);
            ServerShares = _serverShareRepository.GetAll().ToList();
        }

    }

    [Table("ServerShare")]
    public class ServerShare
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PK_SERVERSHARE_ID { get; set; }
        public required string Server { get; set; }
        public required string Share { get; set; }

        public string GetUNCPath() => $@"\\{Server}\{Share}";
    }
}
