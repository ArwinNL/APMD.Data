
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Dapper.Mapper;
using MySqlConnector;

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


        private readonly IDbConnection _db;
        private DiskCollection _diskCollection;

        public PhotoRepository(string connectionString): this(new MySqlConnection(connectionString))
        {
            _diskCollection = new DiskCollection(connectionString);
        }

        public PhotoRepository(System.Data.IDbConnection connection)
        {
            _db = connection;
            _diskCollection = new DiskCollection(connection);
        }


        public IEnumerable<Photo> GetAll() =>
            _db.Query<Photo>("SELECT * FROM Photo");

        public Photo GetById(int id)
        {
            var photo = _db.QueryFirstOrDefault<Photo>("SELECT * FROM Photo WHERE PK_PHOTO_ID = @id", new { id });

            if (photo != null) photo.Disk = _diskCollection.Disks.FirstOrDefault(d => d.PK_DISK_ID == photo.FK_DISK_ID);

            return photo;
        }

        public int Insert(Photo item) =>
            _db.Execute("INSERT INTO Photo (...) VALUES (...)" /* TODO: Fill columns */, item);

        public int Update(Photo item)
        {
            const string sql_photo_update = @"
                UPDATE 
                    Photo 
                SET 
                    FK_DISK_ID = @FK_DISK_ID,
                    FK_SET_ID = @FK_SET_ID,
                    Stored = @Stored,
                    Archived = @Archived,
                    `Order` = @Order
                WHERE 
                    PK_PHOTO_ID = @PK_PHOTO_ID";
            var result = _db.Execute(sql_photo_update, item);
            return result;
        }

        public int Delete(int id) =>
            _db.Execute("DELETE FROM Photo WHERE PK_PHOTO_ID = @id", new { id });

        public List<Photo> GetSetPhotos(int keySet)
        {
            var result = _db.Query<Photo>(sqlSetSelect, new { FK_SET_ID = keySet }).AsList();
            foreach (var item in result)
            {
                item.Disk = _diskCollection.Disks.FirstOrDefault((d => d.PK_DISK_ID == item.FK_DISK_ID));
            }
            return result;
        }
    }
}
