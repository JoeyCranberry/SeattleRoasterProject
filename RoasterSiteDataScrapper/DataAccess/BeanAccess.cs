using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoasterBeansDataAccess.Mongo;
using RoasterBeansDataAccess.Models;
using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq;

using MongoDB.Driver;

namespace RoasterBeansDataAccess.DataAccess
{
    public class BeanAccess
    {
        #region Select Beans
        public static async Task<List<BeanModel>> GetAllBeans()
        {
            var collection = GetBeanCollection();

			var results = await collection.FindAsync(_ => true);

            return results.ToList();
        }

        public static async Task<List<BeanModel>> GetBeansByRoaster(RoasterModel roaster)
        {
            var collection = GetBeanCollection();

            var results = await collection.FindAsync(b => b.MongoRoasterId == roaster.Id);

            return results.ToList();
        }

        public static async Task<List<BeanModel>> GetAllBeansNotExcluded()
        {
            var collection = GetBeanCollection();

            var results = await collection.FindAsync(b => !b.IsExcluded);

            return results.ToList();
        }

        public static async Task<BeanGetResult> GetBeansByFilter(BeanFilter filter)
        {
            var collection = GetBeanCollection();

            var results = await collection.FindAsync(
                b => (!filter.IsSingleOrigin.IsActive || filter.IsSingleOrigin.CompareValue == b.IsSingleOrigin)
                    && (!filter.IsExcluded.IsActive || filter.IsExcluded.CompareValue == b.IsExcluded)
                    && (!filter.IsFairTradeCertified.IsActive || filter.IsFairTradeCertified.CompareValue == b.IsFairTradeCertified || filter.IsFairTradeCertified.CompareValue == b.IsAboveFairTradePricing)
                    && (!filter.IsDirectTradeCertified.IsActive || filter.IsDirectTradeCertified.CompareValue == b.IsDirectTradeCertified)
				    && (!filter.IsInStock.IsActive || filter.IsInStock.CompareValue == b.InStock)
					&& (!filter.AvailablePreground.IsActive || filter.AvailablePreground.CompareValue == b.AvailablePreground)
					&& (!filter.IsSupportingCause.IsActive || filter.IsSupportingCause.CompareValue == b.IsSupportingCause)
					&& (!filter.IsFromWomanOwnedFarms.IsActive || filter.IsFromWomanOwnedFarms.CompareValue == b.IsFromWomanOwnedFarms)
					&& (!filter.IsDecaf.IsActive || filter.IsDecaf.CompareValue == b.IsDecaf)
					&& (!filter.IsRainforestAllianceCertified.IsActive || filter.IsRainforestAllianceCertified.CompareValue == b.IsRainforestAllianceCertified)
				);

            BeanGetResult getResult = new BeanGetResult();

			if (results != null)
            {
                var filteredWithLists = results.ToList().Where(
                    b => filter.CountryFilter.MatchesFilter(b.GetOriginCountries())
                    && filter.ValidRoasters.MatchesFilter(b.MongoRoasterId)
					&& filter.ChosenRoasters.MatchesFilter(b.MongoRoasterId)
					&& filter.RoastFilter.MatchesFilter(b.RoastLevel)
                    && filter.ProcessFilter.MatchesFilter(b.ProcessingMethods)
                    && filter.OrganicFilter.MatchesFilter(b.OrganicCerification)
                    && filter.SearchTastingNotesString.MatchesFilter(b.TastingNotes)
                    && filter.RoasterNameSearch.MatchesFilter(b.MongoRoasterId)
                    && filter.RegionFilter.MatchesFilter(b.GetAllRegionsAndCitiesList())
                );

                var afterListFilter = filteredWithLists.ToList();

                // Check if the search name is active, if so return matches if there are any, otherwise ignore the search name string
                if (filter.SearchNameString.IsActive)
                {
                    var searchNameMatch = afterListFilter.Where(b => filter.SearchNameString.MatchesFilter(b.FullName + " " + b.GetAllRegionsAndCities())).ToList();
                    if (searchNameMatch.Count > 0)
                    {
                        getResult.IsExactMatch = true;
						getResult.Results = searchNameMatch;

                        return getResult;
					}
                    else
                    {
						getResult.IsExactMatch = false;
					}
                }
                else
                {
                    getResult.IsExactMatch = true;
				}

				getResult.Results = afterListFilter.ToList();

				return getResult;
			}
            else
            {
                return getResult;
			}
        }

		public static async Task<List<BeanModel>> GetAllBeansByIds(List<string> beanIds)
		{
			var collection = GetBeanCollection();

			var results = await collection.FindAsync(b => beanIds.Contains(b.Id));

			return results.ToList();
		}

		public static async Task<List<BeanModel>> GetAllProductionInvisibleBeans()
		{
			var collection = GetBeanCollection();

			var results = await collection.FindAsync(b => b.IsProductionVisible == false && b.IsExcluded == false);

			return results.ToList();
		}

		#endregion

		#region Insert Beans
		public static async Task<bool> AddBean(BeanModel newBean)
        {
            var collection = GetBeanCollection();

            if (collection == null)
            {
                return false;
            }

            try
            {
                await collection.InsertOneAsync(newBean);

                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                return false;
            }
        }

        public static async Task<bool> AddBeans(List<BeanModel> newBeans)
        {
            var collection = GetBeanCollection();

            if (collection == null)
            {
                return false;
            }

            try
            {
                await collection.InsertManyAsync(newBeans);

                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                return false;
            }
        }
        #endregion

        #region Replace Bean
        public static async Task<bool> ReplaceBean(BeanModel oldBean, BeanModel newBean)
        {
            var collection = GetBeanCollection();

            if (collection == null)
            {
                return false;
            }

            try
            {
                await collection.ReplaceOneAsync(b => b.Id == oldBean.Id, newBean);

                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                return false;
            }
        }

        public static async Task<bool> UpdateBean(BeanModel editBean)
        {
            var collection = GetBeanCollection();

            if (collection == null)
            {
                return false;
            }

            try
            {
                await collection.ReplaceOneAsync(b => b.Id == editBean.Id, editBean);

                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                return false;
            }
        }
		#endregion

		#region Delete Beans
		public static async Task<bool> DeleteBean(BeanModel delBean)
        {
            var collection = GetBeanCollection();

            if (collection == null)
            {
                return false;
            }

            try
            {
                await collection.DeleteOneAsync(b => b.Id == delBean.Id);

                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                return false;
            }
        }
		#endregion

		#region Processing
        public static async Task<bool> UnsetField(string fieldName)
        {
			var collection = GetBeanCollection();

			UpdateDefinitionBuilder<BeanModel> updateDefinitionBuilder = Builders<BeanModel>.Update;

			UpdateDefinition<BeanModel> updateDefinition = updateDefinitionBuilder.Unset(fieldName);

			await collection.UpdateManyAsync(_ => true, updateDefinition);

            return true;
		}

        //    public static async Task<bool> SetBrewMethods()
        //    {
        //        var collection = GetBeanCollection();

        //        var results = await collection.FindAsync(_ => true);

        //        List<BeanModel> beans = results.ToList();

        //        foreach (var bean in beans)
        //        {
        //            if(bean.RecommendingBrewMethods != null)
        //            {
        //                bean.RecommendedBrewMethods = bean.RecommendingBrewMethods;
        //	await UpdateBean(bean);
        //}
        //        }

        //        return true;
        //    }
        #endregion

        #region Mongo Access
        private static IMongoCollection<BeanModel>? GetBeanCollection(bool isDevelopment = true)
        {
            string connString = Credentials.GetConnectionString(isDevelopment);
            string dbName = "SeattleRoasters";
            string collectionName = "BeanListings";

            var client = new MongoClient(connString);
            var db = client.GetDatabase(dbName);
            return db.GetCollection<BeanModel>(collectionName);
        }
        #endregion
    }

    public class BeanGetResult
    {
        public List<BeanModel>? Results { get; set;}
        public bool IsExactMatch = true;
    }
}
