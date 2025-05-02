
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Xml.Linq;
using Dapper;
using MySqlConnector;

namespace APMD.Data
{
    public class TagRepository
    {
        const string sql_insert_tag = @"
            INSERT INTO 
                Tags(PK_TAG_ID, Name, FK_TAGGROUP_ID, FK_PHOTO_ID) 
            VALUES 
                (0, @Name, @TagGroup, @Photo);
            SELECT LAST_INSERT_ID();";

        const string sql_formodel = @"
            SELECT DISTINCT t.* FROM Tags t 
                INNER JOIN SetTags st 
                    ON t.PK_TAG_ID = st.FK_TAG_ID 
                INNER JOIN SetModels sm 
                    ON st.FK_SET_ID = sm.FK_SET_ID 
            WHERE 
                sm.FK_MODEL_ID = @modelId";

        const string sql_update_tag = @"
            UPDATE 
                Tags 
            SET 
                Name = @Name, FK_TAGGROUP_ID = @TagGroup, FK_PHOTO_ID = @Photo 
            WHERE 
                PK_TAG_ID = @PK_TAG_ID;";

        private readonly IDbConnection _db;

        public TagRepository(string connectionString)
        {
            _db = new MySqlConnection(connectionString);
        }

        public IEnumerable<Tag> GetAll()
        {
            var sql = @"
            SELECT 
                t.*, tg.*
            FROM 
                Tags t
            INNER JOIN 
                TagGroups tg 
                ON t.FK_TAGGROUP_ID = tg.PK_TAGGROUP_ID";

            return _db.Query<Tag, TagGroups, Tag>(
                sql,
                (tag, tagGroup) =>
                {
                    tag.TagGroup = tagGroup;
                    return tag;
                },
                splitOn: "PK_TAGGROUP_ID"
                );

        }

        public Tag GetById(int id) { 

            var sql = @"
            SELECT 
                t.*, tg.*
            FROM 
                Tags t
            INNER JOIN 
                TagGroups tg 
                ON t.FK_TAGGROUP_ID = tg.PK_TAGGROUP_ID
            WHERE PK_TAG_ID = @id";
            var result = new Tag() { PK_TAG_ID = 0, Name = String.Empty };
            var resultQuery = _db.Query<Tag, TagGroups, Tag>(
                sql,
                (tag, tagGroup) =>
                {
                    tag.TagGroup = tagGroup;
                    return tag;
                },
                new { id },
                splitOn: "PK_TAGGROUP_ID"
                );

            if (resultQuery == null)
                throw new KeyNotFoundException($"Tag with ID {id} not found.");
            else
                result = resultQuery.FirstOrDefault();

            return result;
        }

        public IEnumerable<Tag> GetUniqueTagsForModel(int modelId)
        {
            var sql = @"
            SELECT DISTINCT t.*, tg.*
            FROM Tags t
            INNER JOIN SetTags st ON t.PK_TAG_ID = st.FK_TAG_ID
            INNER JOIN TagGroups tg ON t.FK_TAGGROUP_ID = tg.PK_TAGGROUP_ID
            INNER JOIN Photos p ON t.FK_PHOTO_ID = p.PK_PHOTO_ID
            INNER JOIN SetPhotos sp ON p.PK_PHOTO_ID = sp.FK_PHOTO_ID
            INNER JOIN Sets s ON sp.FK_SET_ID = s.PK_SET_ID
            WHERE s.FK_MODEL_ID = @modelId";
            return _db.Query<Tag,TagGroups,Tag>(
                sql,
                (tag, tagGroup) =>
                {
                    tag.TagGroup = tagGroup;
                    return tag;
                },
                new { modelId },
                splitOn: "PK_TAGGROUP_ID"
                );
        }

        public IEnumerable<Tag> GetAllForGroup(int groupId)
        {
            var sql = @"
            SELECT 
                t.*, tg.*
            FROM 
                Tags t
            INNER JOIN 
                TagGroups tg 
                ON t.FK_TAGGROUP_ID = tg.PK_TAGGROUP_ID
            WHERE 
                tg.PK_TAGGROUP_ID = @groupId";

            return _db.Query<Tag, TagGroups, Tag>(
                sql,
                (tag, tagGroup) =>
                {
                    tag.TagGroup = tagGroup;
                    return tag;
                },
                new { groupId },
                splitOn: "PK_TAGGROUP_ID"
                );
        }

        public async Task<int> UpdateTagAsync(Tag item)
        {
            var id = await _db.ExecuteAsync(sql_update_tag, item);
            return id;
        }

        public int InsertTag(Tag item)
        {
            var parameters = new
            {
                Name = item.Name,
                TagGroup = item.FK_TAGGROUP_ID,
                Photo = item.FK_PHOTO_ID
            };
            var id = _db.ExecuteScalar<int>(sql_insert_tag, parameters);
            item.PK_TAG_ID = id; // Update the object with the generated ID
            return id;
        }
        public int Delete(int id) =>
            _db.Execute("DELETE FROM Tags WHERE PK_TAG_ID = @id", new { id });

        public IEnumerable<Tag> GetAllForSet(Set set) => 
            _db.Query<Tag>("SELECT t.* FROM Tags t INNER JOIN SetTags st ON t.PK_TAG_ID = st.FK_TAG_ID WHERE st.FK_SET_ID = @setId", new { setId = set.PK_SET_ID });

        public IEnumerable<Tag> GetAllForModel(Model model) =>
            _db.Query<Tag>(sql_formodel, new { modelId = model.PK_MODEL_ID });
    }
}
