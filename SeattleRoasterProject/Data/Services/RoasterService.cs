using RoasterBeansDataAccess;
using SeattleRoasterProject;

namespace SeattleRoasterProject.Data.Services
{
    public class RoasterService
    {
        private static string roastersFilePath = @"C:\Users\JoeMini\source\repos\SeattleRoasterProject\SeattleRoasterProject\Data\Sources\roasters.json";
        public List<Roaster> GetRoastersFromFile()
        {
            var roasters = RoasterStorage.LoadRoastersFromFile(roastersFilePath).OrderBy(r => r.Name).ToList();
            return roasters ?? new List<Roaster>();
        }

        public bool AddRoasterToFile(Roaster newRoaster)
		{
            return RoasterStorage.AddRoasterToFile(roastersFilePath, newRoaster);
        }

        public bool ReplaceRoasterInFile(Roaster oldRoaster, Roaster newRoaster)
        {
            return RoasterStorage.ReplaceRoasterInFile(roastersFilePath, oldRoaster, newRoaster);
        }

        public bool DeleteRoasterInFile(Roaster roasterToDelete)
        {
            return RoasterStorage.DeleteRoasterInFile(roastersFilePath, roasterToDelete);
        }
    }
}
