// @nuget: Z.Dapper.Plus

namespace APMD.Data
{
    using Dapper;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Model
    {
        private string? _icg;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PK_MODEL_ID { get; set; }
        public required string Name { get; set; }
        public int? FK_MAIN_MODEL_ID { get; set; }
        public int? FK_PHOTO_ID { get; set; }
        public required short Rank { get; set; }
        public string? ICG
        {
            get => _icg ?? "";
            set 
            {  
                if (value == null || value == "NULL")
                    _icg = "";
                else
                    _icg = value; 
            }
        }

        [ForeignKey("FK_PHOTO_ID")]
        public Photo? ModelPhoto { get; set; }

        [ForeignKey("FK_MAIN_MODEL_ID")]
        public Model? MainModel { get; set; }

        [NotMapped]
        public List<Set> Sets { get; set; } = new List<Set>();

        [NotMapped]
        public List<Model> Models { get; set; } = new List<Model>();
    }
}