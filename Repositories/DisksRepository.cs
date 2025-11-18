
using System.Data;
using Dapper;
using MySqlConnector;

namespace APMD.Data
{
    //public class DisksRepository
    //{
    //    private readonly IDbConnection _db;

    //    public DisksRepository(System.Data.IDbConnection connection)
    //    {
    //        _db = connection;
    //    }

    //    public DisksRepository(string connectionString) : this(new MySqlConnection(connectionString)) { }

    //    public List<Disk> GetAll() =>
    //        _db.Query<Disk>("SELECT * FROM Disks").ToList();

    //    public Disk? GetById(int id) =>
    //        _db.QueryFirstOrDefault<Disk>("SELECT * FROM Disks WHERE PK_DISK_ID = @id", new { id });

    //    public int Insert(Disk item) =>
    //        _db.Execute("INSERT INTO Disks (...) VALUES (...)" /* TODO: Fill columns */, item);

    //    public int Update(Disk item) =>
    //        _db.Execute("UPDATE Disks SET ... WHERE PK_DISK_ID = @PK_DISK_ID" /* TODO: Fill columns */, item);

    //    public int Delete(int id) =>
    //        _db.Execute("DELETE FROM Disks WHERE PK_DISK_ID = @id", new { id });
    //}
}
