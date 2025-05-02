using Serilog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace APMD.Data
{
    public class Photo
    {
        public required int PK_PHOTO_ID { get; set; }
        public required string OriginalFilename { get; set; }
        public required string Description { get; set; }
        public string? OriginalFolder { get; set; }
        public string? Extension { get; set; }
        public int? FK_SET_ID { get; set; }
        public int? Order { get; set; }
        public required bool Stored { get; set; }
        public required bool Archived { get; set; }
        public required int FK_DISK_ID { get; set; }

        [ForeignKey("FK_DISK_ID")]
        public Disk? Disk {  get; set; }

        [ForeignKey("FK_SET_ID")]
        public Set? Set { get; set; }

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
                    try
                    {
                        var result = Image.FromFile(PhotoFile);
                        result.Tag = this;
                        return result;
                    }
                    catch (Exception ex)
                    {
                        // Handle the exception (e.g., log it, show a message, etc.)
                        Log.Error($"Image not found: {ex.Message}");
                        MessageBox.Show($"Image not found: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
    }
}