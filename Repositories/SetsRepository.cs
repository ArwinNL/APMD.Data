using APMD.Data.Models;
using Dapper;
using Dapper.Mapper;
using MySqlConnector;
using System.Data;
using System.Data.Common;
using System.Text;

namespace APMD.Data
{
    public class SetsRepository
    {
        const string sql_set_byid = @"
            SELECT 
                s.* 
            FROM 
                Sets s
            WHERE 
                PK_SET_ID = @id
            ";

        const string sql_set_all = @"
            SELECT 
                s.*, sm.*, w.*
            FROM 
                Sets s 
            INNER JOIN 
                SetModels sm 
                on sm.FK_SET_ID = s.PK_SET_ID 
            INNER JOIN 
                Models m 
                ON m.PK_MODEL_ID = sm.FK_MODEL_ID
            INNER JOIN 
                Websites w
                ON w.PK_WEBSITE_ID = s.FK_WEBSITE_ID
            ";

        internal const string sql_insert = @"
            INSERT INTO 
            Sets
                (
                    FK_WEBSITE_ID,
                    Title,
                    FK_PHOTO_ID,
                    PublishedAt,
                    Archived,
                    Tagged,
                    AllPhotosStored
                )
            VALUES
                (
                    @FK_WEBSITE_ID,
                    @Title,
                    @FK_PHOTO_ID,
                    @PublishedAt,
                    @Archived,
                    @Tagged,
                    @AllPhotosStored
                );
            SELECT LAST_INSERT_ID();
            ";

        const string sql_update = @"
            UPDATE 
                Sets
            SET 
                Title = @Title,
                FK_PHOTO_ID = @FK_PHOTO_ID,
                FK_WEBSITE_ID = @FK_WEBSITE_ID,
                Archived = @Archived,
                Tagged = @Tagged,
                PublishedAt = @PublishedAt,
                AllPhotosStored = @AllPhotosStored
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
                sm.FK_MODEL_ID = @keyModel
            ";
        const string sql_set_detele = @"
            DELETE FROM 
                Sets 
            WHERE 
                PK_SET_ID = @id
            ";

        const string sql_deletereference = @"
            DELETE FROM 
                SetTags
            WHERE 
                FK_SET_ID = @FK_SET_ID 
                AND 
                FK_TAG_ID = @FK_TAG_ID
            ";

        const string sql_set_insert_model = @"
            INSERT INTO 
                SetModels (FK_SET_ID, FK_MODEL_ID) 
            VALUES 
                (@FK_SET_ID, @FK_MODEL_ID);
            ";

        const string sql_set_delete_model = @"
            DELETE FROM 
                SetModels 
            WHERE 
                FK_SET_ID = @FK_SET_ID 
            AND 
                FK_MODEL_ID = @FK_MODEL_ID;
            ";

        const string sql_set_insert_tag = @"
            INSERT INTO 
                SetTags (FK_SET_ID, FK_TAG_ID) 
            VALUES 
                (@PK_SET_ID, @PK_TAG_ID)
            ";

        const string sql_set_get_tag = @"
            SELECT 
                st.* 
            FROM 
                SetTags st 
            WHERE 
                st.FK_SET_ID = @PK_SET_ID 
            AND 
                st.FK_TAG_ID = @PK_TAG_ID
            ";

        const string sql_select_avg_sets_model = @"
            SELECT
                m.PK_MODEL_ID,
                COUNT(DISTINCT sm.FK_SET_ID) AS NumSets,
                COUNT(p.PK_PHOTO_ID)         AS NumPhotos
            FROM Models m
            LEFT JOIN SetModels sm
              ON sm.FK_MODEL_ID = m.PK_MODEL_ID
            LEFT JOIN Photo p
              ON p.FK_SET_ID = sm.FK_SET_ID
            GROUP BY m.PK_MODEL_ID;
            ";

        const string sql_select_double_sets = @"
            SELECT 
                s.*,
                sm.FK_MODEL_ID
            FROM Sets s
            JOIN SetModels sm 
                ON sm.FK_SET_ID = s.PK_SET_ID
            JOIN (
                SELECT 
                    sm2.FK_MODEL_ID,
                    s2.Title,
                    s2.FK_WEBSITE_ID
                FROM Sets s2
                JOIN SetModels sm2 
                    ON sm2.FK_SET_ID = s2.PK_SET_ID
                GROUP BY 
                    sm2.FK_MODEL_ID,
                    s2.Title,
                    s2.FK_WEBSITE_ID
                HAVING COUNT(*) > 1
            ) dup 
                ON dup.FK_MODEL_ID   = sm.FK_MODEL_ID
               AND dup.Title         = s.Title
               AND dup.FK_WEBSITE_ID = s.FK_WEBSITE_ID
            ORDER BY 
                sm.FK_MODEL_ID,
                s.FK_WEBSITE_ID,
                s.Title,
                s.PK_SET_ID;
            ";

        const string sql_update_all_photos_stored = @"
            UPDATE 
                Sets
            SET 
                AllPhotosStored = @AllPhotosStored
            WHERE 
                PK_SET_ID = @PK_SET_ID;
            ";

        const string sql_refresh_all_photos_stored = @"
            UPDATE Sets s
            LEFT JOIN (
              SELECT
                p.FK_SET_ID,
                CASE
                  WHEN SUM(p.Stored = 0) > 0 THEN 0   -- at least 1 missing
                  ELSE 1                                -- none missing
                END AS NewAllPhotosStored
              FROM Photo p
              GROUP BY p.FK_SET_ID
            ) x ON x.FK_SET_ID  = s.PK_SET_ID
            SET s.AllPhotosStored = COALESCE(x.NewAllPhotosStored, 1);
            ";
        private readonly IDbConnection _db;

        public SetsRepository(string connectionString)
        {
            _db = new MySqlConnection(connectionString);
            Dapper.SqlMapper.AddTypeHandler(new SqlDateOnlyTypeHandler());
        }
        internal async Task<IEnumerable<Set>> GetAll()
        {
            //public IEnumerable<Sets> GetAll() =>
            var sets = await _db.QueryAsync<Set, Model, Websites, Set>(
                sql_set_all,
                (set, model, website) =>
                {
                    set.Models.Add(model);
                    set.Website = website;
                    return set;
                },
                splitOn: "PK_MODEL_ID,PK_WEBSITE_ID"
            );
            return sets;
        }

        internal IEnumerable<Set> GetAllForModel(long keyModel)
        {
            var result = _db.Query<Set>(
                sql_for_model,
                new { keyModel }
            );
            return result;
        }

        internal Set GetById(long id, bool throwExeception = true)
        {

            var result = _db.QueryFirstOrDefault<Set>(sql_set_byid, new { id });
            if (result == null && throwExeception)
                throw new KeyNotFoundException($"Set with ID {id} not found.");
            return result;
        }

        public Task<IEnumerable<ModelStats>> GetSetInfo(long? keyModel = null)
        {
            var sql = new StringBuilder();
            var parameters = new DynamicParameters();

            // Build SELECT clause
            sql.Append(sql_select_avg_sets_model);


            if (!(keyModel == null))
            {
                sql.Replace("GROUP BY m.PK_MODEL_ID;", "WHERE m.PK_MODEL_ID = @keyModel GROUP BY m.PK_MODEL_ID;");
                parameters.Add("keyModel", keyModel);
            }

            return _db.QueryAsync<ModelStats>(sql.ToString(), parameters);
        }


        internal long Insert(Set item)
        {
            item.PK_SET_ID = _db.ExecuteScalar<long>(sql_insert, item);
            return item.PK_SET_ID;
        }

        internal int Update(Set item)
        {
            try
            {
                var result = _db.Execute(sql_update, item);
                if (result == 0)
                {
                    var msgError = $"Set with ID {item.PK_SET_ID} not updated.";
                    throw new DataUpdateFailedException(msgError);
                }
                return result;
            }
            catch (DbException ex)
            {
                throw new DataUpdateFailedException("Error updating the set.", ex);
            }
        }

        internal long Delete(long id) =>
            _db.Execute(sql_set_detele, new { id });



        // SetTags CRUD
        internal SetTags AddTagToSet(Tag tag, Set set)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PK_SET_ID", set.PK_SET_ID);
            parameters.Add("@PK_TAG_ID", tag.PK_TAG_ID);
            _db.Execute(sql_set_insert_tag, parameters);
            var setTags = _db.QuerySingle<SetTags>(sql_set_get_tag, parameters);
            return setTags;
        }

        internal int DeleteReference(int keyTag, long keySet) =>
            _db.Execute(sql_deletereference, new { FK_SET_ID = keySet, FK_TAG_ID = keyTag });
        internal int DeleteReference(Tag tag, Set currentSet) =>
            DeleteReference(tag.PK_TAG_ID, currentSet.PK_SET_ID);
        internal int DeleteReference(SetTags setTags) =>
            DeleteReference(setTags.FK_TAG_ID, setTags.FK_SET_ID);

        internal void AddModelToSet(long pK_MODEL_ID, long pK_SET_ID)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@FK_SET_ID", pK_SET_ID);
            parameters.Add("@FK_MODEL_ID", pK_MODEL_ID);
            var result = _db.Execute(sql_set_insert_model, parameters);
            if (result != 1)
                throw new DataUpdateFailedException($"Addition of model {pK_MODEL_ID} to set {pK_SET_ID} failed");
        }

        internal void RemoveModelFromSet(long pK_MODEL_ID, long pK_SET_ID)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@FK_SET_ID", pK_SET_ID);
            parameters.Add("@FK_MODEL_ID", pK_MODEL_ID);
            var result = _db.Execute(sql_set_delete_model, parameters);
            if (result != 1)
                throw new DataUpdateFailedException($"Removall of model {pK_MODEL_ID} from set {pK_SET_ID} failed");
        }

        public PagedResult<Set> GetAllUnassigned(int pageSize, int offset)
        {
            var page = (offset / pageSize) + 1;

            var items = _db.Query<Set>(@"
                SELECT
                    s.* 
                FROM 
                    SetWithoutModels s
                ",
                new { pageSize, offset }
                );

            int total = _db.ExecuteScalar<int>(
                @"
                SELECT 
                    COUNT(*) 
                FROM 
                    SetWithoutModels s
                ");

            return new PagedResult<Set>(items.ToList(), offset, pageSize, total);
        }

        public List<Set> GetDoubled()
        {
            var items = _db.Query<Set>(sql_select_double_sets).ToList();

            return items;
        }


        internal IEnumerable<Set> Search(string search)
        {
            var sql = @"
                SELECT 
                    s.* 
                FROM 
                    Sets s
                WHERE 
                    s.Title LIKE @search
                ";
            var parameters = new DynamicParameters();
            parameters.Add("search", $"%{search}%");
            var result = _db.Query<Set>(sql, parameters);
            return result;
        }

        internal IEnumerable<Set> GetAllArchived()
        {
            var sql = @"
                SELECT 
                    s.* 
                FROM 
                    Sets s
                WHERE 
                    s.Archived = 1
                ";
            var result = _db.Query<Set>(sql);
            return result;
        }
    }
}
