using System.ComponentModel.DataAnnotations.Schema;

namespace APMD.Data
{
    public class Photo
    {
        private string ? originalFolder;
        private int _fkServerShareId;
        private ServerShare? _fkServerShare;

        public long PK_PHOTO_ID { get; set; }
        public string OriginalFilename { get; set; }
        public string Description { get; set; }
        public string? OriginalFolder 
        {   get => originalFolder;
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Contains(":") && !value.Contains(":\\"))
                    value = value.Replace(":", ":\\");
                originalFolder = value;
            }
        }
        public string? Extension { get; set; }
        public long? FK_SET_ID { get; set; }
        public int? Order { get; set; }
        public required bool Stored { get; set; }
        public required bool Archived { get; set; }
        public required int FK_SERVERSHARE_ID 
        { 
            get => _fkServerShareId;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("FK_SERVERSHARE_ID cannot be negative.");
                _fkServerShareId = value;
            }
        }

        [NotMapped]
        public ServerShare? ServerShare
        { get
            { 
                if((FK_SERVERSHARE_ID != 0) && _fkServerShare == null) 
                    throw new InvalidOperationException("ServerShare is not set. Ensure it is loaded from the repository.");
                return _fkServerShare;
            }
            set
            {
                _fkServerShare = value;
                if (value != null && FK_SERVERSHARE_ID != value.PK_SERVERSHARE_ID)
                {
                    FK_SERVERSHARE_ID = value.PK_SERVERSHARE_ID;
                }
            }
        }

        [NotMapped]
        public static ServerShare? ArchiveServerShare { get; set; }

        [ForeignKey("FK_SET_ID")]
        public Set? Set { get; set; }

        [NotMapped]
        public string FileName => $"APD{PK_PHOTO_ID:000000000000}";
        [NotMapped]
        internal string FileExtension => $"{Extension}";
        [NotMapped]
        internal string FilePath => $"{FileName.Substring(0, 3)}\\{FileName.Substring(3, 3)}\\{FileName.Substring(6, 3)}\\{FileName.Substring(9, 3)}\\";
        [NotMapped]
        internal string ThumbnailPath => $"{FileName.Substring(0, 3)}.T\\{FileName.Substring(3, 3)}\\{FileName.Substring(6, 3)}\\{FileName.Substring(9, 3)}\\";
        [NotMapped]
        public string PhotoFile => Archived ? ArchivePhotoFile : ServerShareFile;
        [NotMapped]
        public string OriginalPhotoFile => $"{OriginalFolder}\\{OriginalFilename}";
        [NotMapped]
        public string ArchivePhotoFile => $"{Photo.ArchiveServerShare?.GetUNCPath()}\\{FilePath}{FileName}{FileExtension}";
        [NotMapped]
        public string ThumbnailFile => $"{ServerShare?.GetUNCPath()}\\{FilePath}{FileName}{FileExtension}";
        [NotMapped]
        public string ServerShareFile => $"{ServerShare?.GetUNCPath()}\\{FilePath}{FileName}{FileExtension}";
        public Photo()
        {
            OriginalFilename = string.Empty;
            Description = string.Empty;
            OriginalFolder = string.Empty;
            Extension = string.Empty;
            Stored = false;
            Archived = false;
            FK_SERVERSHARE_ID = 0; // Default value, assuming no disk is set
        }
    }
}