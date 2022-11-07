using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoasterSiteDataScrapper
{
    public static class LoadFromFile
    {
        public static List<Roaster>? LoadRoastersFromFile(string filePath)
        {
            return GetLoadedRoasters(filePath).Roasters;
        }

        private static LoadedRoasters? GetLoadedRoasters(string filePath)
		{
            LoadedRoasters deserialized;

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string json = reader.ReadToEnd();
                    deserialized = JsonConvert.DeserializeObject<LoadedRoasters>(json);
                }
            }
            catch
            {
                return null;
            }

            return deserialized;
        }

        public static bool AddRoasterToFile(string filePath, Roaster newRoaster)
		{
            LoadedRoasters? loaded = GetLoadedRoasters(filePath);

            if (loaded == null)
			{
                return false;
			}
            else
			{
                loaded.Roasters.Add(newRoaster);
                File.WriteAllText(filePath, JsonConvert.SerializeObject(loaded));

                return true;
            }
        }
    }

    public class LoadedRoasters
    {
        public List<Roaster> Roasters;
    }
}
