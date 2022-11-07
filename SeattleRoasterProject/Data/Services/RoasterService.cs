using RoasterSiteDataScrapper;
using SeattleRoasterProject;

namespace SeattleRoasterProject.Data.Services
{
    public class RoasterService
    {
        private static string roastersFilePath = @"C:\Users\JoeMini\source\repos\SeattleRoasterProject\SeattleRoasterProject\Data\Sources\roasters.json";
        public List<Roaster> GetRoastersFromFile()
        {
            var roasters = LoadFromFile.LoadRoastersFromFile(roastersFilePath);
            return roasters ?? new List<Roaster>();
        }

        public bool AddRoasterToFile(Roaster newRoaster)
		{
            return LoadFromFile.AddRoasterToFile(roastersFilePath, newRoaster);
        }
    }
}
