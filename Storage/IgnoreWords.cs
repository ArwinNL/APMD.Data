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
        private static readonly string filePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "APDM"), "ignorewords.json");
        public static List<string> LoadAll()
        {
            var result = new List<string>();
            if (File.Exists(filePath))
            {
                result = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(filePath));
            }
            return result;
        }
    }
}
