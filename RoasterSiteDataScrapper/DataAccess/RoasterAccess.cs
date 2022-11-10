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
    public static class RoasterAccess
    {
        #region Select Roaster
        public static async Task<List<RoasterModel>> GetRoasters()
        {
            var collection = GetRoasterCollection();

            var results = await collection.FindAsync(_ => true);

            return results.ToList();
        }
        #endregion

        #region Insert Roaster
        public static async Task<bool> AddRoaster(RoasterModel newRoaster)
        {
            var collection = GetRoasterCollection();

            if(collection == null)
            {
                return false;
            }    

            try
            {
                await collection.InsertOneAsync(newRoaster);

                return true;
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc);
                return false;
            }
        }

        public static async Task<bool> AddRoasters(List<RoasterModel> newRoasters)
        {
            var collection = GetRoasterCollection();

            if (collection == null)
            {
                return false;
            }

            try
            {
                await collection.InsertManyAsync(newRoasters);

                return true;
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc);
                return false;
            }
        }
        #endregion

        #region Replace Roaster
        public static async Task<bool> ReplaceRoaster(RoasterModel oldRoaster, RoasterModel newRoaster)
        {
            var collection = GetRoasterCollection();

            if (collection == null)
            {
                return false;
            }

            try
            {
                await collection.ReplaceOneAsync(r => r.Id == oldRoaster.Id, newRoaster);

                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                return false;
            }

        }
        #endregion

        #region Delete Roaster
        public static async Task<bool> DeleteRoaster(RoasterModel delRoaster)
        {
            var collection = GetRoasterCollection();

            if (collection == null)
            {
                return false;
            }

            try
            {
                await collection.DeleteOneAsync(r => r.Id == delRoaster.Id);

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
        private static IMongoCollection<RoasterModel>? GetRoasterCollection()
        {
            string connString = Credentials.GetConnectionString();
            string dbName = "SeattleRoasters";
            string collectionName = "RoastersList";

            var client = new MongoClient(connString);
            var db = client.GetDatabase(dbName);
            return db.GetCollection<RoasterModel>(collectionName);
        }
        #endregion


    }
}
