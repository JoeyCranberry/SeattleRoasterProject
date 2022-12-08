using RoasterBeansDataAccess;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.DataAccess;
using SeattleRoasterProject;
using MongoDB.Driver;

namespace SeattleRoasterProject.Data.Services
{
    public class RoasterService
    {
        public async Task<List<RoasterModel>> GetAllRoasters()
        {
            var roasters = await RoasterAccess.GetAllRoasters();

            return roasters.OrderBy(r => r.Name).ToList() ?? new List<RoasterModel>();
        }

        public async Task<RoasterModel> GetRoasterByMongoId(string id)
        {
			var roasterMatch = await RoasterAccess.GetRoasterById(id);

			return roasterMatch ?? new RoasterModel();
		}

        public async Task<List<RoasterModel>> GetRoastersByName(string name)
        {
            List<RoasterModel> results = new List<RoasterModel>();

            string[] terms = name.Split(' ');
            foreach(string term in terms)
            {
                var roasterMatch = await RoasterAccess.GetRoastersByName(term);
                if (roasterMatch != null)
                {
                    results.AddRange(roasterMatch);
				}
			}

            return results;

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

        public async Task<BeanListingDifference> CheckForUpdate(RoasterModel roaster)
        {
            BeanListingDifference results = await BeanDataScraper.GetBeanListingDifference(roaster);

            return results;
        }
    }
}
