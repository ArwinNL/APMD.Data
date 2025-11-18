
using Dapper;
using Dapper.Mapper;
using MySqlConnector;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Z.Dapper.Plus;

namespace APMD.Data
{
    public class PhotoRepository
    {
        const string sqlSetSelect = @"
            SELECT 
                p.*
            FROM
                Photo p 
            WHERE 
                p.FK_SET_ID = @FK_SET_ID
            ";

        const string sql_photo_update = @"
                UPDATE 
                    Photo 
                SET 
                    FK_SERVERSHARE_ID = @FK_SERVERSHARE_ID,
                    FK_SET_ID = @FK_SET_ID,
                    Stored = @Stored,
                    Archived = @Archived,
                    `Order` = @Order
                WHERE 
                    PK_PHOTO_ID = @PK_PHOTO_ID";

        const string sql_photo_insert = @"
                INSERT INTO 
                    Photo
                ( 
                    OriginalFileName,
                    OriginalFolder,
                    FK_SERVERSHARE_ID,
                    FK_SET_ID,
                    Stored,
                    Archived,
                    Extension,
                    `Order`
                )
                VALUES 
                ( 
                    @OriginalFileName,
                    @OriginalFolder,
                    @FK_SERVERSHARE_ID,
                    @FK_SET_ID,
                    @Stored,
                    @Archived,
                    @Extension,
                    @Order
                );
                SELECT LAST_INSERT_ID();";

        private readonly IDbConnection _db;
        private ServerShareCollection _serverShareCollection;

        public PhotoRepository(string connectionString) : this(new MySqlConnection(connectionString))
        {
            _serverShareCollection = new ServerShareCollection(connectionString);
        }

        public PhotoRepository(System.Data.IDbConnection connection)
        {
            _db = connection;
            _serverShareCollection = new ServerShareCollection(connection);
        }


        public IEnumerable<Photo> GetAll()
        {
            var result = _db.Query<Photo>("SELECT * FROM Photo");
            result.ToList().ForEach(photo =>
            {
                photo.ServerShare = _serverShareCollection.ServerShares.First(ss => ss.PK_SERVERSHARE_ID == photo.FK_SERVERSHARE_ID);
            });
            return result;
        }

        public Photo? GetById(long id)
        {
            var photo = _db.QueryFirstOrDefault<Photo>("SELECT * FROM Photo WHERE PK_PHOTO_ID = @id", new { id });

            if (photo != null)
                photo.ServerShare = _serverShareCollection.ServerShares.First(ss => ss.PK_SERVERSHARE_ID == photo.FK_SERVERSHARE_ID);

            return photo;
        }

        public long Insert(Photo item)
        {
            item.PK_PHOTO_ID = _db.ExecuteScalar<int>(sql_photo_insert, item);
            item.ServerShare = _serverShareCollection.ServerShares.First(ss => ss.PK_SERVERSHARE_ID == item.FK_SERVERSHARE_ID);
            return item.PK_PHOTO_ID;
        }

        public int Update(Photo item)
        {
            var result = _db.Execute(sql_photo_update, item);
            return result;
        }

        public int Delete(long id) =>
            _db.Execute("DELETE FROM Photo WHERE PK_PHOTO_ID = @id", new { id });

        public List<Photo> GetAllForSet(long keySet)
        {
            var result = _db.Query<Photo>(sqlSetSelect, new { FK_SET_ID = keySet }).AsList();
            foreach (var item in result)
            {
                item.ServerShare = _serverShareCollection.ServerShares.First((ss => ss.PK_SERVERSHARE_ID == item.FK_SERVERSHARE_ID));
            }
            return result;
        }

        internal void BulkUpdate(List<Photo> photos)
        {
            _db.BulkUpdate(photos);
        }

        internal void BulkInsert(List<Photo> photos)
        {
            _db.BulkInsert(photos);
        }
    }
}
