using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace APMD.Data.Storage
{
    public static class IgnoreWordsStorage
    {
        private static readonly string folderPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "APDM");

        private static readonly string filePath =
            Path.Combine(folderPath, "ignorewords.json");

        public static List<string> LoadAll()
        {
            try
            {
                if (!File.Exists(filePath))
                    return new List<string>();

                var json = File.ReadAllText(filePath);

                return JsonSerializer.Deserialize<List<string>>(json)
                       ?? new List<string>(); // handle null JSON
            }
            catch (Exception ex)
            {
                // Optional: log exception somewhere
                return new List<string>();
            }
        }
    }
}
