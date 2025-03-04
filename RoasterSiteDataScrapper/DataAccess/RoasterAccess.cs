using MongoDB.Driver;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.Mongo;

namespace RoasterBeansDataAccess.DataAccess;

public static class RoasterAccess
{
    #region Replace Roaster

    public static async Task<bool> ReplaceRoaster(RoasterModel oldRoaster, RoasterModel newRoaster,
        bool isDevelopment = false)
    {
        var collection = GetRoasterCollection(isDevelopment);

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

    public static async Task<bool> DeleteRoaster(RoasterModel delRoaster, bool isDevelopment = false)
    {
        var collection = GetRoasterCollection(isDevelopment);

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

    private static IMongoCollection<RoasterModel>? GetRoasterCollection(bool isDevelopment = true)
    {
        var connString = Credentials.GetConnectionString(isDevelopment);
        var dbName = "SeattleRoasters";
        var collectionName = "RoastersList";

        var client = new MongoClient(connString);
        var db = client.GetDatabase(dbName);
        return db.GetCollection<RoasterModel>(collectionName);
    }

    #endregion

    #region Select Roaster

    public static async Task<List<RoasterModel>> GetAllRoasters(bool isDevelopment = false)
    {
        var collection = GetRoasterCollection(isDevelopment);

        var results = await collection.FindAsync(r => !r.IsExcluded);

        return results.ToList();
    }

    public static async Task<RoasterModel?> GetRoasterById(string id, bool isDevelopment = false)
    {
        var collection = GetRoasterCollection(isDevelopment);

        var collectionResults = await collection.FindAsync(r => r.Id == id);
        var results = collectionResults.ToList();

        if (results.Count <= 0)
        {
            return null;
        }

        return results[0];
    }

    public static async Task<List<RoasterModel>?> GetRoastersByName(string searchTerm, bool isDevelopment = false)
    {
        var collection = GetRoasterCollection(isDevelopment);

        var collectionResults = await collection.FindAsync(_ => true);

        var results = collectionResults.ToList();

        if (results.Count <= 0)
        {
            return null;
        }

        List<RoasterModel> matchResults = new();
        foreach (var roaster in results)
        {
            if (roaster.Name.ToLower().Split(' ').ToList().Contains(searchTerm))
            {
                matchResults.Add(roaster);
            }
        }

        return matchResults;
    }

    #endregion

    #region Insert Roaster

    public static async Task<bool> AddRoaster(RoasterModel newRoaster, bool isDevelopment = false)
    {
        var collection = GetRoasterCollection(isDevelopment);

        if (collection == null)
        {
            return false;
        }

        try
        {
            await collection.InsertOneAsync(newRoaster);

            return true;
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc);
            return false;
        }
    }

    public static async Task<bool> AddRoasters(List<RoasterModel> newRoasters, bool isDevelopment = false)
    {
        var collection = GetRoasterCollection(isDevelopment);

        if (collection == null)
        {
            return false;
        }

        try
        {
            await collection.InsertManyAsync(newRoasters);

            return true;
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc);
            return false;
        }
    }

    #endregion
}