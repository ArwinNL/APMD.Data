
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

        private readonly IDbConnection _db;

        public TagsRepository(string connectionString)
        {
            _db = new MySqlConnection(connectionString);
        }

        public IEnumerable<Tag> GetAll() =>
            _db.Query<Tag>("SELECT * FROM Tags");

        public Tag GetById(int id) =>
            _db.QueryFirstOrDefault<Tag>("SELECT * FROM Tags WHERE PK_TAG_ID = @id", new { id });

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
    }
}
