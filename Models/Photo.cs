using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMD.Data
{
    public class Photo
    {
        public int PK_PHOTO_ID { get; set; }
        public required string OriginalFilename { get; set; }
        public required string Description { get; set; }
        public string? OriginalFolder { get; set; }
        public string? Extension { get; set; }
        public int? FK_SET_ID { get; set; }
        public bool Stored { get; set; }
        public bool Archived { get; set; }
        public int FK_DISK_ID { get; set; }

        [ForeignKey("FK_DISK_ID")]
        public Disks? Disk {  get; set; }

        [ForeignKey("FK_SET_ID")]
        public Sets? Set { get; set; }

        [NotMapped]
        public string FileName => $"APD{PK_PHOTO_ID:000000000000}";
        [NotMapped]
        public string FileExtension => $"{Extension}";
        [NotMapped]
        public string FilePath => $"{FileName.Substring(0, 3)}\\{FileName.Substring(3, 3)}\\{FileName.Substring(6, 3)}\\{FileName.Substring(9, 3)}\\";
        [NotMapped]
        public string PhotoFile => $"{Disk.Root_path}{FilePath}{FileName}{FileExtension}";

        [NotMapped]
        public Image PhotoImage {
            get
            {
                if (Stored)
                {
                    return Image.FromFile(PhotoFile);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}