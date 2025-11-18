
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

        public IEnumerable<Websites> GetAll(bool sorted = false)
        {
            var query = "SELECT * FROM Websites";
            if (sorted)
            {
                query += " ORDER BY Name"; // Assuming 'Name' is a column in the Websites table
            }
            return _db.Query<Websites>(query);
        }

        public Websites? GetById(int id) =>
            _db.QueryFirstOrDefault<Websites>("SELECT * FROM Websites WHERE PK_WEBSITE_ID = @id", new { id });

        public int Insert(Websites item) =>
            _db.Execute("INSERT INTO Websites (...) VALUES (...)" /* TODO: Fill columns */, item);

        public int Update(Websites item) =>
            _db.Execute("UPDATE Websites SET ... WHERE PK_WEBSITE_ID = @PK_WEBSITE_ID" /* TODO: Fill columns */, item);

        public int Delete(int id) =>
            _db.Execute("DELETE FROM Websites WHERE PK_WEBSITE_ID = @id", new { id });

        internal IEnumerable<Websites> SearchOnWord(string word) =>
            _db.Query<Websites>("SELECT * FROM Websites WHERE Name LIKE @word", new { word = $"%{word}%" });

        internal List<Websites> GetByModelId(int pK_MODEL_ID)
        {
            const string sql_oud = @"
                SELECT w.* 
                FROM Websites w
                JOIN Sets s 
                ON s.FK_WEBSITE_ID = w.PK_WEBSITE_ID
                JOIN SetModels sm 
                ON  sm.FK_SET_ID = s.PK_SET_ID
                WHERE sm.FK_MODEL_ID = @PK_MODEL_ID";
            const string sql = @"
                SELECT w.*
                FROM Websites AS w
                WHERE EXISTS (
                  SELECT 1
                  FROM Sets AS s
                  JOIN SetModels AS sm
                        ON sm.FK_SET_ID = s.PK_SET_ID
                  WHERE s.FK_WEBSITE_ID = w.PK_WEBSITE_ID
                    AND sm.FK_MODEL_ID = @PK_MODEL_ID
                );
                ";
            return _db.Query<Websites>(sql, new { PK_MODEL_ID = pK_MODEL_ID }).ToList();
        }
    }
}
