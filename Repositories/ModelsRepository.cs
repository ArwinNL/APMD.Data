using Dapper;
using MySqlConnector;
using System.Data;
using System.Text;

namespace APMD.Data
{
    public class ModelsRepository
    {
        const string sql_delete = @"DELETE FROM Models WHERE PK_MODEL_ID = @id";

        const string sql_insert = @"
            INSERT INTO Models
            (
                Name,
                FK_PHOTO_ID,
                Rank,
                ICG
            )
            VALUES
            (
                @Name,
                @FK_PHOTO_ID,
                @Rank,
                @ICG
            );
            SELECT LAST_INSERT_ID();
            ";

        const string sql_update = @"UPDATE Models
            SET 
                Name = @Name,
                FK_MAIN_MODEL_ID = @FK_MAIN_MODEL_ID,
                FK_PHOTO_ID = @FK_PHOTO_ID,
                Rank = @Rank,
                ICG = @ICG
            WHERE 
                PK_MODEL_ID = @PK_MODEL_ID;
            ";

        const string sql_model_view = @"
            SELECT 
                msv.* 
            FROM 
                ModelSummaryView msv
            ";

        const string sql_model_search = @"
            SELECT DISTINCT
                m.PK_MODEL_ID,
                m.Name,
                m.FK_MAIN_MODEL_ID,
                m.FK_PHOTO_ID,
                m.Rank,
                m.ICG
            FROM Models      AS m
            JOIN SetModels   AS sm ON sm.FK_MODEL_ID = m.PK_MODEL_ID
            JOIN Sets        AS s  ON s.PK_SET_ID    = sm.FK_SET_ID
            WHERE
                s.FK_WEBSITE_ID = @PK_WEBSITE_ID     -- website filter
                AND m.Name LIKE CONCAT('%', @word, '%')  
            ORDER BY
                m.Name;";

        const string sql_delete_setmodel_by_setid = @"
            DELETE FROM SetModels
            WHERE FK_SET_ID = @pK_SET_ID;";

        const string sql_selectForSet = @"SELECT m.* FROM SetModels sm INNER JOIN Models m ON sm.FK_MODEL_ID = m.PK_MODEL_ID WHERE sm.FK_SET_ID = @id";

        const string sql_icg = @"SELECT m.* FROM Models m WHERE m.ICG = @icg";

        private readonly IDbConnection _db;
        private readonly IDbConnection _viewDb;
        private IDbConnection db
        {
            get
            {
                if (_db.State != ConnectionState.Open)
                    _db.Open();
                return _db;
            }
        }
        private IDbConnection ViewDb
        {
            get
            {
                if (_viewDb.State != ConnectionState.Open)
                    _viewDb.Open();
                return _viewDb;
            }
        }
        public ModelsRepository(string connectionString)
        {
            _db = new MySqlConnection(connectionString);
            _viewDb = new MySqlConnection(connectionString);
        }

        public async Task<IEnumerable<Model>> GetAll(int count, int skip)
        {
            var sql = new StringBuilder();
            var parameters = new DynamicParameters();

            sql.Append("SELECT * FROM ModelSummaryView");

            // no WHERE here (unless you add filters like search, active, etc.)

            //sql.Append(" ORDER BY Name ASC, PK_MODEL_ID ASC"); // stable ordering

            if (count >= 0)
            {
                sql.Append(" LIMIT @Limit OFFSET @Offset");
                parameters.Add("Limit", count);
                parameters.Add("Offset", skip);
            }

            var result = await ViewDb.QueryAsync<Model>(sql.ToString(), parameters, commandTimeout: 120);
            return result;
        }

        public async Task<IEnumerable<Model>> GetModels()
        {
            var summaries = await db.QueryAsync<ModelSummary>(sql_model_view);
            var modelTasks = summaries.Select(summary => Model.FromSummaryAsync(summary));
            var models = await Task.WhenAll(modelTasks); // Fixed variable name to 'modelTasks'
            return models;
        }


        public Model GetById(long id)
        {
            var result = db.QueryFirstOrDefault<Model>("SELECT * FROM Models WHERE PK_MODEL_ID = @id", new { id });
            if (result == null)
                throw new KeyNotFoundException($"Model with ID {id} not found.");
            result.MainModel = result.FK_MAIN_MODEL_ID != null ? GetById(result.FK_MAIN_MODEL_ID.Value) : null;
            return result;
        }

        public List<Model> GetByICG(string icg)
        {
            var result = db.Query<Model>(sql_icg, new { icg });
            return result.AsList();
        }

        public long Insert(Model item)
        {
            item.PK_MODEL_ID = db.ExecuteScalar<int>(sql_insert, item);
            return item.PK_MODEL_ID;
        }

        public int Update(Model item) =>
            db.Execute(sql_update, item);

        public int Delete(long id) =>
            db.Execute(sql_delete, new { id });

        internal List<Model> GetAllForSet(long id)
        {
            var result = db.Query<Model>(sql_selectForSet, new { id });
            return result.AsList();
        }

        internal IEnumerable<Model> SearchOnWord(string word, Websites? website)
        {
            if (website == null)
            {
                return SearchOnWord(word);
            }
            else
            {
                var parameters = new DynamicParameters();
                parameters.Add("@word", word.ToLowerInvariant());
                parameters.Add("@PK_Website_ID", website.PK_WEBSITE_ID);

                var result = db.Query<Model>(sql_model_search, parameters);
                return result;
            }
        }

        internal IEnumerable<Model> SearchOnWord(string word)
        {
            var result = db.Query<Model>(@$"SELECT * FROM Models
                WHERE Name LIKE LOWER(@word)
                ORDER BY Name ASC", new { word = $"%{word.ToLowerInvariant()}%" });
            return result;
        }

        internal int DeleteModelsForSet(long pK_SET_ID)
        {
            return db.Execute(sql_delete_setmodel_by_setid, new { pK_SET_ID });
        }

        internal int CountPhoto(long pK_PHOTO_ID)
        {
            return db.ExecuteScalar<int>(@"SELECT COUNT(*) FROM Models WHERE FK_PHOTO_ID = @pK_PHOTO_ID", new { pK_PHOTO_ID });
        }
    }
}
