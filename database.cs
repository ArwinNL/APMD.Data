using MySqlConnector;
using Meziantou.Framework.Win32;

namespace APMD.Data
{
    public static class Database
    {
        public static string GetConnectionString()
        {
            var cnnstring = "";
            var credential = CredentialManager.ReadCredential("APMD");
            if (credential != null)
            {
                var cnn = new MySqlConnectionStringBuilder();
                cnn.Database = "APMD";
                cnn.UserID = credential.UserName;
                cnn.Password = "BCOBR5ra9gIHprxsUOXr2iEc6!";//credential.Password;
                //cnn.ApplicationName = credential.ApplicationName;
                cnn.Server = "192.168.178.11";
                cnn.Port = 3306;
                //:/run/mysqld/mysqld10.sock
                cnnstring = cnn.ToString();
            }
            return cnnstring;
        }

        public static string GetTestConnectionString()
        {
            var cnnstring = "";
            var credential = CredentialManager.ReadCredential("APMD");
            if (credential != null)
            {
                var cnn = new MySqlConnectionStringBuilder();
                cnn.Database = "APMD.Test";
                cnn.UserID = credential.UserName;
                cnn.Password = "BCOBR5ra9gIHprxsUOXr2iEc6!";//credential.Password;
                //cnn.ApplicationName = credential.ApplicationName;
                cnn.Server = "192.168.178.11";
                cnn.Port = 3306;
                //:/run/mysqld/mysqld10.sock
                cnnstring = cnn.ToString();
            }
            return cnnstring;
        }
    }
}
