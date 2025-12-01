using System.Data;
using Dapper;
using MySqlConnector;

namespace APMD.Data
{
    public class ImportRepository
    {
        const string sql_insert = @"
            INSERT INTO 
                Import (SetName, FullPath, FK_WEBSITE_ID, PublishedAt, Processed)
            VALUES 
                (@SetName, @FullPath, @FK_WEBSITE_ID, @PublishedAt, 0);
            SELECT LAST_INSERT_ID();";

        const string sql_update = @"UPDATE Import
            SET 
                SetName = @SetName,
                FullPath = @FullPath,
                Processed = @Processed,
                FK_WEBSITE_ID = @FK_WEBSITE_ID,
                PublishedAt = @PublishedAt
            WHERE 
                PK_IMPORT_ID = @PK_IMPORT_ID;
            ";

        private static readonly Serilog.ILogger Log = Serilog.Log.ForContext<ImportRepository>();

        private readonly IDbConnection _db;

        public ImportRepository(System.Data.IDbConnection connection)
        {
            _db = connection;
            Dapper.SqlMapper.AddTypeHandler(new SqlDateOnlyTypeHandler());
        }

        public ImportRepository(string connectionString) : this(new MySqlConnection(connectionString)) { }

        public List<Import> GetAll() => _db.Query<Import>("SELECT * FROM Import").ToList();

        public int Insert(Import newImport)
        {
            try
            {
                var id = _db.ExecuteScalar<int>(sql_insert, newImport);
                newImport.PK_IMPORT_ID = id;
                return id;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inserting Import record");
                throw;
            }
        }

        public int Update(Import item)
        {
            try             
            {
                return _db.Execute(sql_update, item);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Import record with ID {ImportId}", item.PK_IMPORT_ID);
                throw;
            }
        }

        public Import? GetById(int id) =>
            _db.QueryFirstOrDefault<Import>("SELECT * FROM Import WHERE PK_IMPORT_ID = @id", new { id });

        internal IEnumerable<Import> GetAllUnprocessed() => _db.Query<Import>("SELECT * FROM Import WHERE Processed = 0").ToList();

        internal void UpdateAll(List<Import> imports)
        {
            foreach (var import in imports)
            {
                Update(import);
            }
        }
    }
}
