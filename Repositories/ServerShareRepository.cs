using Dapper;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APMD.Data.Repositories
{
    class ServerShareRepository
    {
        private readonly IDbConnection _db;
        private readonly string sqlSelect = "SELECT PK_SERVERSHARE_ID, Server, Share FROM ServerShare";
        public ServerShareRepository(System.Data.IDbConnection connection)
        {
            _db = connection;
        }

        public ServerShareRepository(string connectionString) : this(new MySqlConnection(connectionString)) { }

        public List<ServerShare> GetAll() =>
            _db.Query<ServerShare>(sqlSelect).ToList();

        public ServerShare? GetById(int id) =>
            _db.QueryFirstOrDefault<ServerShare>($@"{sqlSelect} WHERE PK_SERVERSHARE_ID = @PK_SERVERSHARE_ID", new { id });

        public int Insert(ServerShare item)
        {
            _db.Execute("INSERT INTO ServerShare (Server, Share) VALUES (@Server, @Share)", item);
            return _db.QuerySingle<int>("SELECT LAST_INSERT_ID()");
        }

        public int Update(ServerShare item) =>
            _db.Execute("UPDATE ServerShare SET Server = @Server, Share = @Share WHERE PK_SERVERSHARE_ID = @PK_SERVERSHARE_ID", item);

        public int Delete(int id) =>
            _db.Execute("DELETE FROM ServerShare WHERE PK_DISK_ID = @id", new { id });
    }

}

