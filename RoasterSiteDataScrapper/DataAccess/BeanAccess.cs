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
