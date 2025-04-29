
using System.Collections.Generic;
using System.Data;
using Dapper;
using MySqlConnector;

namespace APMD.Data
{
    public class ModelsRepository
    {
        const string sql_delete = @"DELETE FROM Models WHERE PK_MODEL_ID = @id";

        const string sql_insert = @"
            UPDATE Models
            SET 
                Name = @Name,
                FK_MAIN_MODEL_ID = @FK_MAIN_MODEL_ID,
                FK_PHOTO_ID = @FK_PHOTO_ID,
                Rank = @Rank,
                ICG = @ICG
            WHERE 
                PK_MODEL_ID = @PK_MODEL_ID
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

        const string sql_selectForSet = @"SELECT m.* FROM SetModels sm INNER JOIN Models m ON sm.FK_MODEL_ID = m.PK_MODEL_ID WHERE sm.FK_SET_ID = @id";

        const string sql_icg = @"SELECT m.* FROM Models m WHERE m.ICG = @icg";

        private readonly IDbConnection _db;

        public ModelsRepository(string connectionString)
        {
            _db = new MySqlConnection(connectionString);
        }

        public Task<IEnumerable<Model>> GetAll()
        {
            var models = _db.QueryAsync<Model>("SELECT * FROM Models ORDER BY Name Asc");
            return models;
        }

        public Model GetById(int id)
        {
            var result = _db.QueryFirstOrDefault<Model>("SELECT * FROM Models WHERE PK_MODEL_ID = @id", new { id });
            if (result == null)
                throw new KeyNotFoundException($"Model with ID {id} not found.");
            result.MainModel = result.FK_MAIN_MODEL_ID != null ? GetById(result.FK_MAIN_MODEL_ID.Value) : null;
            return result;
        }

        public List<Model> GetByICG(string icg)
        {
            var result = _db.Query<Model>(sql_icg, new { icg });
            return result.AsList();
        }

        public int Insert(Model item) =>
            _db.Execute(sql_insert, item);

        public int Update(Model item) =>
            _db.Execute(sql_update, item);

        public int Delete(int id) =>
            _db.Execute(sql_delete, new { id });

        internal List<Model> GetAllForSet(int id)
        {
            var result = _db.Query<Model>(sql_selectForSet, new {id });
            return result.AsList();
        }
    }
}
