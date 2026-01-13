// @nuget: Z.Dapper.Plus

namespace APMD.Data
{
    using Dapper.Contrib.Extensions;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Threading.Tasks;


    public class Model : INotifyPropertyChanged
    {
        private string? _icg;

        [Dapper.Contrib.Extensions.Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PK_MODEL_ID { get; set; }

        public required string Name { get; set; } = string.Empty;

        public int? FK_MAIN_MODEL_ID { get; set; }

        public long? FK_PHOTO_ID { get; set; }

        public required short Rank { get; set; } = 0;

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
        public string DisplayName => $"{Name} [{ICG}]".Trim();

        [NotMapped]
        public List<Set> Sets { get; set; } = new List<Set>();

        [NotMapped]
        public List<Model> Models { get; set; } = new List<Model>();

        [NotMapped]
        public int NumSets { get; set; }

        [NotMapped]
        public int NumPhotos { get; set; }

        [NotMapped]
        public int UniqueTagsInSets { get; set; }

        [NotMapped]
        public int ModelTags { get; set; }

        [Computed]
        public List<Websites> Websites { get; set; } = new List<Websites>();

        public event PropertyChangedEventHandler? PropertyChanged; // Fix for CS8612: Make the event nullable to match the interface.

        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName)); // Ensure null-safe invocation.

        public static async Task<Model> FromSummaryAsync(ModelSummary summary)
        {
            await Task.Delay(10); // Simulate I/O, remove or replace with real call

            return new Model
            {
                PK_MODEL_ID = summary.PK_MODEL_ID,
                Name = summary.Name,
                NumSets = summary.NumSets,
                UniqueTagsInSets = summary.UniqueTagsInSets,
                ModelTags = summary.ModelTags,
                Rank = summary.Rank
            };
        }
    }

    public class ModelSummary
    {
        public int PK_MODEL_ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int NumSets { get; set; }
        public int UniqueTagsInSets { get; set; }
        public int ModelTags { get; set; }
        public short Rank { get; set; }
    }

}