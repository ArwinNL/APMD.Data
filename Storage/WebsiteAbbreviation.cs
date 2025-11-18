using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace APMD.Data.Storage
{

    public static class AbbreviationStorage
    {
        private static readonly string filePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "APDM"), "abbreviations.json");

        public static List<WebsiteAbbreviationSet> LoadAll()
        {
            var result = new List<WebsiteAbbreviationSet>();
            try
            {
                if (File.Exists(filePath))
                { 
                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Deserialize into List<WebsiteAbbreviationSet>
                result = JsonSerializer.Deserialize<List<WebsiteAbbreviationSet>>(json, options);
                }
            }
            catch (Exception ex)
            {
                // In a real app, log or show an error. For now, return empty list.
                Console.Error.WriteLine($"Error loading abbreviations: {ex}");
                return result;
            }
            return result;
        }

        public static void SaveAll(List<WebsiteAbbreviationSet> allSets)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (directory != null && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(allSets, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                // In a real app, log or show an error.
                Console.Error.WriteLine($"Error saving abbreviations: {ex}");
            }
        }
    }
}
