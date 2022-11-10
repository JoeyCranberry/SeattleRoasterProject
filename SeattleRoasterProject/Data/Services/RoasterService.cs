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

        public async Task<bool> AddRoasterToDb(RoasterModel newRoaster)
		{
            return await RoasterAccess.AddRoaster(newRoaster);
        }

        public async Task<bool> ReplaceRoasterInDb(RoasterModel oldRoaster, RoasterModel newRoaster)
        {
            return await RoasterAccess.ReplaceRoaster(oldRoaster, newRoaster);
        }

        public async Task<bool> DeleteRoasterInDb(RoasterModel delRoaster)
        {
            return await RoasterAccess.DeleteRoaster(delRoaster);
        }
    }
}
