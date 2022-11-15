using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoasterBeansDataAccess.Mongo;
using RoasterBeansDataAccess.Models;

using MongoDB.Driver;

namespace RoasterBeansDataAccess.DataAccess
{
    public class BeanAccess
    {
        #region Select eEans
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

        public static async Task<List<BeanModel>> GetBeansByFilter(BeanFilter filter)
        {
            var collection = GetBeanCollection();

            var results = await collection.FindAsync(
                b => (!filter.IsSingleOrigin.IsActive || filter.IsSingleOrigin.CompareValue == b.IsSingleOrigin)
                    && (!filter.IsExcluded.IsActive || filter.IsExcluded.CompareValue == b.IsExcluded)
                    && (!filter.IsFairTradeCertified.IsActive || filter.IsFairTradeCertified.CompareValue == b.IsFairTradeCertified)
                    && (!filter.IsDirectTradeCertified.IsActive || filter.IsDirectTradeCertified.CompareValue == b.IsDirectTradeCertified)
				    && (!filter.IsInStock.IsActive || filter.IsInStock.CompareValue == b.InStock)
					&& (!filter.AvailablePreground.IsActive || filter.AvailablePreground.CompareValue == b.AvailablePreground)
				);

            if(results != null)
            {
				var filteredWithLists = results.ToList().Where(
					b => filter.CountryFilter.MatchesFilter(b.CountriesOfOrigin)
					&& filter.RoastFilter.MatchesFilter(b.RoastLevel)
					&& filter.ProcessFilter.MatchesFilter(b.ProcessingMethod)
					&& filter.OrganicFilter.MatchesFilter(b.OrganicCerification)
					&& filter.SearchTastingNotesString.MatchesFilter(b.TastingNotes)
                    && filter.RoasterNameSearch.MatchesFilter(b.MongoRoasterId)
			    );

                var afterListFilter = filteredWithLists.ToList();

                // Check if the search name is active, if so return matches if there are any, otherwise ignore the search name string
				if (filter.SearchNameString.IsActive)
                {
                    var searchNameMath = afterListFilter.Where(b => filter.SearchNameString.MatchesFilter(b.FullName)).ToList();
                    if(searchNameMath.Count > 0)
                    {
                        return searchNameMath;
                    }
				}    

				return afterListFilter.ToList();
			}
            else
            {
                return new List<BeanModel>();
            }
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

        #region
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

        #region Mongo Access
        private static IMongoCollection<BeanModel>? GetBeanCollection()
        {
            string connString = Credentials.GetConnectionString();
            string dbName = "SeattleRoasters";
            string collectionName = "BeanListings";

            var client = new MongoClient(connString);
            var db = client.GetDatabase(dbName);
            return db.GetCollection<BeanModel>(collectionName);
        }
        #endregion
    }
}
