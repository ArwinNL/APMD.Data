
using System.Collections.Generic;
using System.Data;
using Dapper;
using MySqlConnector;

namespace APMD.Data
{
    public class TagGroupsRepository
    {
        private readonly IDbConnection _db;

        public TagGroupsRepository(string connectionString)
        {
            _db = new MySqlConnection(connectionString);
        }

        public IEnumerable<TagGroups> GetAll() =>
            _db.Query<TagGroups>("SELECT * FROM TagGroups");

        public TagGroups GetById(int id) =>
            _db.QueryFirstOrDefault<TagGroups>("SELECT * FROM TagGroups WHERE PK_TAGGROUP_ID = @id", new { id });

        public int Insert(TagGroups item) =>
            _db.Execute("INSERT INTO TagGroups (...) VALUES (...)" /* TODO: Fill columns */, item);

        public int Update(TagGroups item) =>
            _db.Execute("UPDATE TagGroups SET ... WHERE PK_TAGGROUP_ID = @PK_TAGGROUP_ID" /* TODO: Fill columns */, item);

        public int Delete(int id) =>
            _db.Execute("DELETE FROM TagGroups WHERE PK_TAGGROUP_ID = @id", new { id });
    }
}
