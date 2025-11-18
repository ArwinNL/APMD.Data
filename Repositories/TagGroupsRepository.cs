
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Dapper;
using MySqlConnector;

namespace APMD.Data
{
    public class TagGroupsRepository
    {
        const string sql_update = @"
            UPDATE 
                TagGroups 
            SET 
                Name = @Name,
                FK_MAIN_TAGGROUP_ID = @FK_MAIN_TAGGROUP_ID,
                FK_PHOTO_ID = @FK_PHOTO_ID
            WHERE 
                PK_TAGGROUP_ID = @PK_TAGGROUP_ID";

        const string sql_insert = @"
            INSERT INTO 
                TagGroups (Name, FK_MAIN_TAGGROUP_ID, FK_PHOTO_ID, Color)
            VALUES 
                (@Name, @FK_MAIN_TAGGROUP_ID, @FK_PHOTO_ID, @Color);
            SELECT LAST_INSERT_ID();";

        private readonly IDbConnection _db;
        private TagRepository _tagsRepository;

        public TagGroupsRepository(string connectionString)
        {
            _db = new MySqlConnection(connectionString);
            _tagsRepository = new TagRepository(connectionString);
        }

        public IEnumerable<TagGroups> GetAll() =>
            _db.Query<TagGroups>("SELECT * FROM TagGroups");

        public ObservableCollection<TagGroups> GetAllFull()
        {
            var groups = GetAll();
            foreach (var group in groups)
            {
                var tags = _tagsRepository.GetAllForGroup(group.PK_TAGGROUP_ID);
                group.Tags = new ObservableCollection<Tag>(tags);
            }
            return new ObservableCollection<TagGroups>(groups);
        }
            

        public TagGroups? GetById(int id) =>
            _db.QueryFirstOrDefault<TagGroups>("SELECT * FROM TagGroups WHERE PK_TAGGROUP_ID = @id", new { id });

        public int Insert(TagGroups item)
        {
            var id = _db.ExecuteScalar<int> (sql_insert, item);
            item.PK_TAGGROUP_ID = id;
            return id;
        }

        public int Update(TagGroups item) =>
            _db.Execute(sql_update, item);

        public int Delete(int id) =>
            _db.Execute("DELETE FROM TagGroups WHERE PK_TAGGROUP_ID = @id", new { id });

        internal TagGroups? GetFullById(int id)
        {
            var group = _db.QuerySingle<TagGroups>("SELECT * FROM TagGroups WHERE PK_TAGGROUP_ID = @id", new { id });
            if (group == null) 
                return null;
            var tags = _tagsRepository.GetAllForGroup(group.PK_TAGGROUP_ID);
            group.Tags = new ObservableCollection<Tag>(tags);
            return group;
        }
    }
}
