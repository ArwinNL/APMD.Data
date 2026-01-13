using Dapper;
using MySqlConnector;
using System.Data;

namespace APMD.Data
{
    public class WebsitesRepository
    {
        private readonly IDbConnection _db;

        const string sqlInsert = @"
            INSERT INTO Websites
            (
                Name,
                URL
            )
            VALUES
            (
                @Name,
                @URL
            );
            SELECT LAST_INSERT_ID();
            ";

        const string sqlUpdate = @"
            UPDATE Websites
            SET
                Name = @Name,
                URL = @URL
            WHERE PK_WEBSITE_ID = @PK_WEBSITE_ID;
            ";

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

        public int Insert(Websites item)
        {
            var key = _db.ExecuteScalar<int>(sqlInsert, item);
            item.PK_WEBSITE_ID = key;
            return key;
        }

        public int Update(Websites item) =>
            _db.Execute(sqlUpdate, item);

        public int Delete(int id) =>
            _db.Execute("DELETE FROM Websites WHERE PK_WEBSITE_ID = @id", new { id });

        internal IEnumerable<Websites> SearchOnWord(string word) =>
            _db.Query<Websites>("SELECT * FROM Websites WHERE Name LIKE @word", new { word = $"%{word}%" });

        internal List<Websites> GetByModelId(long pK_MODEL_ID)
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

        internal int CountPhoto(long pK_PHOTO_ID)
        {
            return _db.ExecuteScalar<int>(@"SELECT COUNT(*) FROM Websites w WHERE w.FK_PHOTO_ID = @pK_PHOTO_ID", new { pK_PHOTO_ID });
        }
    }
}
