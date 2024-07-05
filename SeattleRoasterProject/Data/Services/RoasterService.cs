using RoasterBeansDataAccess;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.DataAccess;

namespace SeattleRoasterProject.Data.Services
{
    public class RoasterService
    {
        private readonly EnvironmentSettings _environmentSettings;
        private readonly bool _isDevelopment = false;

		public RoasterService(EnvironmentSettings environmentSettings)
		{
			_environmentSettings = environmentSettings;
			_isDevelopment = environmentSettings.IsDevelopment;
		}

		public async Task<List<RoasterModel>> GetAllRoasters()
        {
            var roasters = await RoasterAccess.GetAllRoasters(_isDevelopment);

            return roasters.OrderBy(r => r.Name).ToList();
        }

		public async Task<List<RoasterModel>> GetAllRoastersByEnvironment()
		{
			var roasters = await RoasterAccess.GetAllRoasters(_isDevelopment);
            if(!_isDevelopment)
            {
                roasters.RemoveAll(r => r.RecievedPermission);
            }

            return roasters.OrderBy(r => r.Name).ToList();
		}

		public async Task<RoasterModel> GetRoasterByMongoId(string id)
		{
			var roasterMatch = await RoasterAccess.GetRoasterById(id, _isDevelopment);

			return roasterMatch ?? new RoasterModel();
		}

        public async Task<List<RoasterModel>> GetRoastersByName(string name)
        {
            var results = new List<RoasterModel>();

            var terms = name.Split(' ');
            foreach(var term in terms)
            {
                var roasterMatch = await RoasterAccess.GetRoastersByName(term, _isDevelopment);
                if (roasterMatch != null)
                {
                    results.AddRange(roasterMatch);
				}
			}

            return results;

		}

        public async Task<bool> AddRoasterToDb(RoasterModel newRoaster)
		{
            return await RoasterAccess.AddRoaster(newRoaster, _isDevelopment);
        }

        public async Task<bool> ReplaceRoasterInDb(RoasterModel oldRoaster, RoasterModel newRoaster)
        {
            return await RoasterAccess.ReplaceRoaster(oldRoaster, newRoaster, _isDevelopment);
        }

        public async Task<bool> DeleteRoasterInDb(RoasterModel delRoaster)
        {
            return await RoasterAccess.DeleteRoaster(delRoaster, _isDevelopment);
        }

        public async Task<BeanListingDifference> CheckForUpdate(RoasterModel roaster)
        {
            var results = await BeanDataScraper.GetBeanListingDifference(roaster);

            return results;
        }
    }
}
