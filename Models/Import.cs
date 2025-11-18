using MySqlConnector;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMD.Data
{

    public class Import
    {
        public int PK_IMPORT_ID { get; set; }
        public string SetName { get; set; }
        public string FullPath { get; set; }
        public bool Processed { get; set; }
        public int FK_WEBSITE_ID { get; set; }
        public DateTime? PublishedAt { get; set; }
        [NotMapped]
        public object Tag { get; set; }

        public Import() 
        {
            SetName = string.Empty;
            FullPath = string.Empty;
            FK_WEBSITE_ID = 0; // Default value, assuming no website is set
            PublishedAt = null;
        }

        public Import(ImportFolder importFolder): this()
        {
            SetName = importFolder.ImportSetName;
            FullPath = importFolder.FolderPath;
            FK_WEBSITE_ID = importFolder.Website?.PK_WEBSITE_ID ?? 0; // Assuming Website is a class with PK_WEBSITE_ID
        }
    }
}
