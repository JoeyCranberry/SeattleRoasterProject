using RoasterBeansDataAccess;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.DataAccess;
using SeattleRoasterProject;

namespace SeattleRoasterProject.Data.Services
{
    public class RoasterService
    {
        public async Task<List<RoasterModel>> GetRoastersFromDb()
        {
            var roasters = await RoasterAccess.GetRoasters();

            return roasters ?? new List<RoasterModel>();
        }

        public async Task<bool> AddRoasterToFile(RoasterModel newRoaster)
		{
            return await RoasterAccess.AddRoasterAsync(newRoaster);
        }

        public async Task<bool> ReplaceRoasterInFile(RoasterModel oldRoaster, RoasterModel newRoaster)
        {
            return await RoasterAccess.ReplaceRoasterAsync(oldRoaster, newRoaster);
        }

        public async Task<bool> DeleteRoasterInFile(RoasterModel delRoaster)
        {
            return await RoasterAccess.DeleteRoasterAsync(delRoaster);
        }
    }
}
