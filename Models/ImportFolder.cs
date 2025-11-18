using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APMD.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.IO;

    // ReSharper disable InconsistentNaming
    // ReSharper disable PartialTypeWithSinglePart

    /// <summary>
    /// Class voor het importeren van complete folders zonder eerst alle images weer te geven
    /// </summary>
    public class ImportFolder
    {
        private DirectoryInfo _folder;
        private Websites? _website;

        public string ImportSetName { get; set; }

        public string FolderPath
        {
            get { return _folder.FullName; }
            set {
                _folder = new DirectoryInfo(value); 
            }
        }

        public Websites? Website { get; set; }

        public bool Import { get; set; }

        public DateTime? FolderDate { get; set; }

        [NotMapped]
        public object Tag { get; set; }

        public ImportFolder(DirectoryInfo folder)
        {
            // Assuming Websites is a class with a default constructor
            _folder = folder;
            Import = false;
            ImportSetName = string.Empty;
        }

        public ImportFolder(Import import, Websites website) : this(new DirectoryInfo(import.FullPath))
        {
            ImportSetName = import.SetName;
            Import = true;
            _website = website;
            FolderDate = import.PublishedAt;
        }

        private List<string> GetFilesToImport()
        {
            // 1) Get all files under _folder
            var allFiles = _folder.GetFiles("*.*", SearchOption.AllDirectories);

            // 2) Define allowed extensions in a HashSet for fast, case‐insensitive lookup
            var allowedExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".gif"
                };

            // 3) Filter + select FileInfo.FullName and convert to List<string>
            var filesToImport = allFiles
                .Where(af => allowedExts.Contains(af.Extension))
                .Select(af => af.FullName)   // or af.Name if you only want the filename
                .ToList();

            return filesToImport;
        }
    }

}
