
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using APMD.Data.Models;
using Dapper;
using Dapper.Mapper;
using Microsoft.VisualBasic.ApplicationServices;
using MySqlConnector;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace APMD.Data
{
    public class SetsRepository
    {
        const string sql_insert = @"INSERT INTO Sets
            (
                FK_WEBSITE_ID,
                Title,
                Set_identifier,
                FK_PHOTO_ID,
                Archived,
                Tagged
            )
            VALUES
            (
                @FK_WEBSITE_ID,
                @Title,
                @Set_identifier,
                @FK_PHOTO_ID
                @Archived,
                @Tagged
            );";

        const string sql_update = @"UPDATE Sets
            SET 
                Title = @Title,
                Set_identifier = @Set_identifier,
                FK_PHOTO_ID = @FK_PHOTO_ID,
                FK_WEBSITE_ID = @FK_WEBSITE_ID,
                Archived = @Archived,
                Tagged = @Tagged
            WHERE 
                PK_SET_ID = @PK_SET_ID;
            ";

        const string sql_for_model = @"
            SELECT
                s.* 
            FROM 
                Sets s 
            INNER JOIN 
                SetModels sm 
            ON 
                s.PK_SET_ID = sm.FK_SET_ID 
            WHERE 
                sm.FK_MODEL_ID = @keyModel";

        private readonly IDbConnection _db;

        public SetsRepository(string connectionString)
        {
            _db = new MySqlConnection(connectionString);
        }
        public async Task<IEnumerable<Set>> GetAll()
        {
            //public IEnumerable<Sets> GetAll() =>
            var sets = await _db.QueryAsync<Set, Model, Set>("SELECT s.*, sm FROM Sets s INNER JOIN SetModels sm on sm.FK_SET_ID = s.PK_SET_ID INNER JOIN Models m ON m.PK_MODEL_ID = sm.FK_MODEL_ID"
                , (set, model) => { set.Models.Add(model);
                    return set;
                }, splitOn: "");
            return sets;
        }

        public IEnumerable<Set> GetAllForModel(int keyModel) =>
            _db.Query<Set>(sql_for_model, new { keyModel });

        public Set GetById(int id)
        {
            var result = _db.QueryFirstOrDefault<Set>("SELECT * FROM Sets WHERE PK_SET_ID = @id", new { id });
            if (result == null)
                throw new KeyNotFoundException($"Set with ID {id} not found.");
            return result;
        }

        public int Insert(Set item) =>
            _db.Execute(sql_insert, item);

        public int Update(Set item)
        {
            _db.Open();
            var transaction = _db.BeginTransaction();
            try
            {
                var result = _db.Execute(sql_update, item);
                if (result == 0)
                    throw new KeyNotFoundException($"Set with ID {item.PK_SET_ID} not found.");
                transaction.Commit();
                return result;
            }
            catch (DbException ex)
            {
                transaction.Rollback();
                throw new Exception("Error updating the set.", ex);
            }
        }

        public int Delete(int id) =>
            _db.Execute("DELETE FROM Sets WHERE PK_SET_ID = @id", new { id });

        public Photo? LoadPhoto(Set sets)
        {
            throw new NotImplementedException();
        }

        public SetTags AddTagToSet(Tag tag, Set set) 
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PK_SET_ID", set.PK_SET_ID);
            parameters.Add("@PK_TAG_ID", tag.PK_TAG_ID);

            _db.Execute("INSERT INTO SetTags (FK_SET_ID, FK_TAG_ID) VALUES (@PK_SET_ID, @PK_TAG_ID)", parameters);
            var setTags = _db.QuerySingle<SetTags>("SELECT * FROM SetTags WHERE FK_SET_ID = @PK_SET_ID AND FK_TAG_ID = @PK_TAG_ID", parameters);
            return setTags;
        }
    }
}
