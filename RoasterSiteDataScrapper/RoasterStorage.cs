using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoasterBeansDataAccess
{
    public static class RoasterStorage
    {
        public static List<Roaster>? LoadRoastersFromFile(string filePath)
        {
            return GetLoadedRoasters(filePath).Roasters;
        }

        public static void UpdateRoasterIds(string filePath)
		{
            var loaded = GetLoadedRoasters(filePath);

            for (int i = 0; i < loaded.Roasters.Count; i++)
            {
                loaded.Roasters[i].RoasterId = i;
            }

            SaveRoastersToFile(filePath, loaded);
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

        private static void SaveRoastersToFile(string filePath, LoadedRoasters loaded)
		{
            File.WriteAllText(filePath, JsonConvert.SerializeObject(loaded));
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
                // Get a valid roaster id
                int roasterId = loaded.Roasters.Max(i => i.RoasterId);
                newRoaster.RoasterId = roasterId++;

                loaded.Roasters.Add(newRoaster);
                SaveRoastersToFile(filePath, loaded);

                return true;
            }
        }

        public static bool ReplaceRoasterInFile(string filePath, Roaster oldRoaster, Roaster newRoaster)
        {
            LoadedRoasters? loaded = GetLoadedRoasters(filePath);

            if (loaded == null)
            {
                return false;
            }
            else
            {
                loaded.Roasters.RemoveAll(r => r.RoasterId == oldRoaster.RoasterId);
                loaded.Roasters.Add(newRoaster);

                SaveRoastersToFile(filePath, loaded);

                return true;
			}
		}

		public static bool DeleteRoasterInFile(string filePath, Roaster roasterToBeDeleted)
		{
            LoadedRoasters? loaded = GetLoadedRoasters(filePath);

            if (loaded == null)
            {
                return false;
            }
            else
            {
                loaded.Roasters.RemoveAll(r => r.RoasterId == roasterToBeDeleted.RoasterId);
                SaveRoastersToFile(filePath, loaded);

                return true;
            }
        }
	}

	public class LoadedRoasters
    {
        public List<Roaster> Roasters;
    }
}
