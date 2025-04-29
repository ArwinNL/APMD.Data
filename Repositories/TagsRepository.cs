
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Xml.Linq;
using Dapper;
using MySqlConnector;

namespace APMD.Data
{
    public class TagsRepository
    {
        const string sql_insert = "INSERT INTO Tags(PK_TAG_ID, Name, FK_TAGGROUP_ID, FK_PHOTO_ID) VALUES(0, @Name, @TagGroup, @Photo);";

        const string sql_formodel = @"
            SELECT t.* FROM Tags t 
                INNER JOIN SetTags st 
                    ON t.PK_TAG_ID = st.FK_TAG_ID 
                INNER JOIN SetModels sm 
                    ON st.FK_SET_ID = sm.FK_SET_ID 
            WHERE sm.FK_MODEL_ID = @modelId";

        private readonly IDbConnection _db;

        public TagsRepository(string connectionString)
        {
            _db = new MySqlConnection(connectionString);
        }

        public IEnumerable<Tag> GetAll() =>
            _db.Query<Tag>("SELECT * FROM Tags");

        public Tag GetById(int id) =>
            _db.QueryFirstOrDefault<Tag>("SELECT * FROM Tags WHERE PK_TAG_ID = @id", new { id });

        public IEnumerable<Tag> GetUniqueTagsForModel(int modelId)
        {
            var sql = @"
            SELECT DISTINCT t.*
            FROM Tags t
            INNER JOIN Photos p ON t.FK_PHOTO_ID = p.PK_PHOTO_ID
            INNER JOIN SetPhotos sp ON p.PK_PHOTO_ID = sp.FK_PHOTO_ID
            INNER JOIN Sets s ON sp.FK_SET_ID = s.PK_SET_ID
            WHERE s.FK_MODEL_ID = @modelId";
            return _db.Query<Tag>(sql, new { modelId });
        }

        public IEnumerable<Tag> GetAllForGroup(int groupId)
        {
            var sql = @"
            SELECT t.*
            FROM Tags t
            WHERE t.FK_TAGGROUP_ID = @groupId";
            return _db.Query<Tag>(sql, new { groupId });
        }

        public async Task<int> InsertTagAsync(Tag tag)
        {
            var sql = @"
            INSERT INTO Tags (Name, FK_TAGGROUP_ID, FK_PHOTO_ID)
            VALUES (@Name, @FK_TAGGROUP_ID, @FK_PHOTO_ID);
            SELECT LAST_INSERT_ID();";

            var id = await _db.ExecuteScalarAsync<int>(sql, tag);
            return id;
        }
        public int Update(Tag item) =>
            _db.Execute("UPDATE Tags SET ... WHERE PK_TAG_ID = @PK_TAG_ID" /* TODO: Fill columns */, item);

        public int Delete(int id) =>
            _db.Execute("DELETE FROM Tags WHERE PK_TAG_ID = @id", new { id });

        public IEnumerable<Tag> GetAllForSet(Set set) => 
            _db.Query<Tag>("SELECT t.* FROM Tags t INNER JOIN SetTags st ON t.PK_TAG_ID = st.FK_TAG_ID WHERE st.FK_SET_ID = @setId", new { setId = set.PK_SET_ID });

        public IEnumerable<Tag> GetAllForModel(Model model) =>
            _db.Query<Tag>(sql_formodel, new { modelId = model.PK_MODEL_ID });
    }
}
