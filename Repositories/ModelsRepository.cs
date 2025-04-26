
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

        private readonly IDbConnection _db;

        public ModelsRepository(string connectionString)
        {
            _db = new MySqlConnection(connectionString);
        }

        public IEnumerable<Models> GetAll() =>
            _db.Query<Models>("SELECT * FROM Models");

        public Models GetById(int id)
        {
            var result = _db.QueryFirstOrDefault<Models>("SELECT * FROM Models WHERE PK_MODEL_ID = @id", new { id });
            if (result == null)
                throw new KeyNotFoundException($"Model with ID {id} not found.");
            result.MainModel = result.FK_MAIN_MODEL_ID != null ? GetById(result.FK_MAIN_MODEL_ID.Value) : null;
            return result;
        }

        public int Insert(Models item) =>
            _db.Execute(sql_insert, item);

        public int Update(Models item) =>
            _db.Execute(sql_update, item);

        public int Delete(int id) =>
            _db.Execute(sql_delete, new { id });

        internal List<Models> GetAllForSet(int id)
        {
            var result = _db.Query<Models>(sql_selectForSet, new {id });
            return result.AsList();
        }
    }
}
