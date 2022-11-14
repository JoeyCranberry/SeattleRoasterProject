using RoasterBeansDataAccess;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.DataAccess;
using SeattleRoasterProject;
using MongoDB.Driver;

namespace SeattleRoasterProject.Data.Services
{
    public class RoasterService
    {
        public async Task<List<RoasterModel>> GetRoastersFromDb()
        {
            var roasters = await RoasterAccess.GetAllRoasters();

            return roasters.OrderBy(r => r.Name).ToList() ?? new List<RoasterModel>();
        }

        public async Task<RoasterModel> GetRoasterFromDbByMongoId(string id)
        {
			var roasterMatch = await RoasterAccess.GetRoasterById(id);

			return roasterMatch ?? new RoasterModel();
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

        public async Task<(List<BeanModel> newListings, List<BeanModel> removedListings)> CheckForUpdate(RoasterModel roaster)
        {
            var results = await BeanDataScraper.GetNewRoasterBeans(roaster);

            return new(results.newListings ?? new List<BeanModel>(), results.removedListings ?? new List<BeanModel>());
        }
    }
}
