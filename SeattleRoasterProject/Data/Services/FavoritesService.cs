using Microsoft.JSInterop;
using Newtonsoft.Json;
using RoasterBeansDataAccess.Models;

namespace SeattleRoasterProject.Data.Services;

public class FavoritesService
{
    private readonly JsInteropService _jsInteropService;
    private const string FavoritesKey = "Favorites";

    public FavoritesService(JsInteropService jsInteropService)
    {
        _jsInteropService = jsInteropService;
    }

    public async Task<List<FavoriteEntry>> GetFavorites()
    {
        var favoritesString = await _jsInteropService.GetValueFromStorage(FavoritesKey);

        if (string.IsNullOrEmpty(favoritesString))
        {
            return new List<FavoriteEntry>();
        }

        // TODO
        favoritesString = "{\"Favorites\":" + favoritesString + "}";

        if (string.IsNullOrEmpty(favoritesString))
        {
            return new List<FavoriteEntry>();
        }

        var favorites = JsonConvert.DeserializeObject<FavoriteStorageObject>(favoritesString);

        if (favorites == null)
        {
            return new List<FavoriteEntry>();
        }

        return favorites.Favorites.ToList();
    }

    public async Task AddBeanToFavorites(BeanModel bean)
    {
        var newEntry = new FavoriteEntry { Id = bean.Id, DateAdded = DateTime.Now };
        await _jsInteropService.AddValueToList(FavoritesKey, newEntry);
    }

    public async Task RemoveBeanToFavorites(BeanModel bean)
    {
        await _jsInteropService.RemoveValueFromListById(FavoritesKey, bean.Id);
    }
}

public class FavoriteStorageObject
{
    public FavoriteEntry[] Favorites { get; set; }
}

public class FavoriteEntry
{
    public string Id { get; set; }
    public DateTime DateAdded { get; set; }
}