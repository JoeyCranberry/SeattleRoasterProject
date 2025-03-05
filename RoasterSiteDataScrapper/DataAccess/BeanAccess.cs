using MongoDB.Driver;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.Mongo;

namespace RoasterBeansDataAccess.DataAccess;

public class BeanAccess
{
    #region Delete Beans

    public static async Task<bool> DeleteBean(BeanModel delBean, bool isDevelopment = false)
    {
        var collection = GetBeanCollection(isDevelopment);

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

    public static async Task<bool> UnsetField(string fieldName, bool isDevelopment = false)
    {
        var collection = GetBeanCollection(isDevelopment);

        var updateDefinitionBuilder = Builders<BeanModel>.Update;

        var updateDefinition = updateDefinitionBuilder.Unset(fieldName);

        await collection.UpdateManyAsync(_ => true, updateDefinition);

        return true;
    }

    //    public static async Task<bool> SetBrewMethods()
    //    {
    //        var collection = GetBeanCollection(isDevelopment);

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
        var connString = Credentials.GetConnectionString(isDevelopment);
        var dbName = "SeattleRoasters";
        var collectionName = "BeanListings";

        var client = new MongoClient(connString);
        var db = client.GetDatabase(dbName);
        return db.GetCollection<BeanModel>(collectionName);
    }

    #endregion

    #region Select Beans

    public static async Task<List<BeanModel>> GetAllBeans(bool isDevelopment = false)
    {
        var collection = GetBeanCollection(isDevelopment);

        var results = await collection.FindAsync(_ => true);

        return results.ToList();
    }

    public static async Task<List<BeanModel>> GetBeansByRoaster(RoasterModel roaster, bool isDevelopment = false)
    {
        var collection = GetBeanCollection(isDevelopment);

        var results = await collection.FindAsync(b => b.MongoRoasterId == roaster.Id);

        return results.ToList();
    }

    public static async Task<List<BeanModel>> GetAllBeansNotExcluded(bool isDevelopment = false)
    {
        var collection = GetBeanCollection(isDevelopment);

        var results = await collection.FindAsync(b => !b.IsExcluded);

        return results.ToList();
    }

    public static async Task<BeanGetResult> GetBeansByFilter(BeanFilter filter, bool isDevelopment = false)
    {
        var collection = GetBeanCollection(isDevelopment);

        var results = await collection.FindAsync(
            b => (!filter.IsSingleOrigin.IsActive || filter.IsSingleOrigin.CompareValue == b.IsSingleOrigin)
                 && (!filter.IsExcluded.IsActive || filter.IsExcluded.CompareValue == b.IsExcluded)
                 && (!filter.IsFairTradeCertified.IsActive ||
                     filter.IsFairTradeCertified.CompareValue == b.IsFairTradeCertified ||
                     filter.IsFairTradeCertified.CompareValue == b.IsAboveFairTradePricing)
                 && (!filter.IsDirectTradeCertified.IsActive ||
                     filter.IsDirectTradeCertified.CompareValue == b.IsDirectTradeCertified)
                 && (!filter.IsInStock.IsActive || filter.IsInStock.CompareValue == b.InStock)
                 && (!filter.AvailablePreground.IsActive ||
                     filter.AvailablePreground.CompareValue == b.AvailablePreground)
                 && (!filter.IsSupportingCause.IsActive || filter.IsSupportingCause.CompareValue == b.IsSupportingCause)
                 && (!filter.IsFromWomanOwnedFarms.IsActive ||
                     filter.IsFromWomanOwnedFarms.CompareValue == b.IsFromWomanOwnedFarms)
                 && (!filter.IsDecaf.IsActive || filter.IsDecaf.CompareValue == b.IsDecaf)
                 && (!filter.IsRainforestAllianceCertified.IsActive ||
                     filter.IsRainforestAllianceCertified.CompareValue == b.IsRainforestAllianceCertified)
                 && (!filter.IsActiveListing.IsActive || filter.IsActiveListing.CompareValue == b.IsActiveListing ||
                     b.IsActiveListing == null)
        );

        var getResult = new BeanGetResult();

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
                     && filter.TastingNotesFilter.MatchesFilter(b.TastingNotes)
            );

            var afterListFilter = filteredWithLists.ToList();

            // Check if the search name is active, if so return matches if there are any, otherwise ignore the search name string
            if (filter.SearchNameString.IsActive)
            {
                var searchNameMatch = afterListFilter.Where(b =>
                    filter.SearchNameString.MatchesFilter(b.FullName + " " + b.GetAllRegionsAndCities())).ToList();
                if (searchNameMatch.Count > 0)
                {
                    getResult.IsExactMatch = true;
                    getResult.Results = searchNameMatch;

                    return getResult;
                }

                getResult.IsExactMatch = false;
            }
            else
            {
                getResult.IsExactMatch = true;
            }

            getResult.Results = afterListFilter.ToList();

            return getResult;
        }

        return getResult;
    }

    public static async Task<List<BeanModel>> GetAllBeansByIds(List<string> beanIds, bool isDevelopment = false)
    {
        var collection = GetBeanCollection(isDevelopment);

        var results = await collection.FindAsync(b => beanIds.Contains(b.Id));

        return results.ToList();
    }

    public static async Task<BeanModel?> GetBeanById(string beanId, bool isDevelopment = false)
    {
        var collection = GetBeanCollection(isDevelopment);

        var results = await collection.FindAsync(bean => bean.Id == beanId);

        return results.FirstOrDefault();
    }

    public static async Task<List<BeanModel>> GetAllProductionInvisibleBeans(bool isDevelopment = false)
    {
        var collection = GetBeanCollection(isDevelopment);

        var results = await collection.FindAsync(b => b.IsProductionVisible == false && b.IsExcluded == false);

        return results.ToList();
    }

    #endregion

    #region Insert Beans

    public static async Task<bool> AddBean(BeanModel newBean, bool isDevelopment = false)
    {
        var collection = GetBeanCollection(isDevelopment);

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

    public static async Task<bool> AddBeans(List<BeanModel> newBeans, bool isDevelopment = false)
    {
        var collection = GetBeanCollection(isDevelopment);

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

    public static async Task<bool> ReplaceBean(BeanModel oldBean, BeanModel newBean, bool isDevelopment = false)
    {
        var collection = GetBeanCollection(isDevelopment);

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

    public static async Task<bool> UpdateBean(BeanModel editBean, bool isDevelopment = false)
    {
        var collection = GetBeanCollection(isDevelopment);

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
}

public class BeanGetResult
{
    public bool IsExactMatch = true;
    public List<BeanModel>? Results { get; set; }
}