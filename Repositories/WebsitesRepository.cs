
using System.Collections.Generic;
using System.Data;
using Dapper;
using MySqlConnector;

namespace APMD.Data
{
    public class WebsitesRepository
    {
        private readonly IDbConnection _db;

        public WebsitesRepository(string connectionString)
        {
            _db = new MySqlConnection(connectionString);
        }

        public IEnumerable<Websites> GetAll() =>
            _db.Query<Websites>("SELECT * FROM Websites");

        public Websites GetById(int id) =>
            _db.QueryFirstOrDefault<Websites>("SELECT * FROM Websites WHERE PK_WEBSITE_ID = @id", new { id });

        public int Insert(Websites item) =>
            _db.Execute("INSERT INTO Websites (...) VALUES (...)" /* TODO: Fill columns */, item);

        public int Update(Websites item) =>
            _db.Execute("UPDATE Websites SET ... WHERE PK_WEBSITE_ID = @PK_WEBSITE_ID" /* TODO: Fill columns */, item);

        public int Delete(int id) =>
            _db.Execute("DELETE FROM Websites WHERE PK_WEBSITE_ID = @id", new { id });
    }
}
