using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace APMD.Data
{
    /// <summary>
    /// Represents a folder to import images from.
    /// Designed to be serializable and safe for persistence.
    /// </summary>
    [Serializable] // Optional (only needed for legacy binary serialization)
    public class ImportFolder
    {
        private DirectoryInfo? _folder;

        /// <summary>
        /// Gets or sets the name of the import set.
        /// </summary>
        public string ImportSetName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the folder path.
        /// Setting this recreates the internal <see cref="DirectoryInfo"/>.
        /// </summary>
        public string FolderPath
        {
            get => _folder?.FullName ?? string.Empty;
            set => _folder = string.IsNullOrWhiteSpace(value) ? null : new DirectoryInfo(value);
        }

        /// <summary>
        /// Gets or sets the associated website.
        /// </summary>
        public Websites? Website { get; set; }

        /// <summary>
        /// Gets or sets whether this folder should be imported.
        /// </summary>
        public bool Import { get; set; }

        /// <summary>
        /// Gets or sets the folder date.
        /// </summary>
        public DateTime? FolderDate { get; set; }

        /// <summary>
        /// Gets or sets a user-defined tag (not persisted).
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public object? Tag { get; set; } = null;

        public AWSD.Entities.PhotosetEntity? Photoset { get; set; } = null;

        /// <summary>
        /// Initializes a new empty instance (required for serializers).
        /// </summary>
        public ImportFolder()
        {
        }

        /// <summary>
        /// Initializes a new instance from a directory.
        /// </summary>
        public ImportFolder(DirectoryInfo folder)
        {
            _folder = folder ?? throw new ArgumentNullException(nameof(folder));
            Import = false;
        }

        /// <summary>
        /// Initializes from an import source.
        /// </summary>
        public ImportFolder(Import import, Websites website)
            : this(new DirectoryInfo(import.FullPath))
        {
            ImportSetName = import.SetName;
            Import = true;
            Website = website; // FIX: use property, not private field
            FolderDate = import.PublishedAt;
        }

        /// <summary>
        /// Gets all image files that should be imported.
        /// </summary>
        public List<string> GetFilesToImport()
        {
            if (_folder == null || !_folder.Exists)
                return new List<string>();

            var allowedExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif"
            };
            return _folder
                .GetFiles("*.*", SearchOption.AllDirectories)
                .Where(f => allowedExts.Contains(f.Extension))
                .Select(f => f.FullName)
                .ToList();
        }
    }
}