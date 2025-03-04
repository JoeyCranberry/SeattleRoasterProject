using Microsoft.JSInterop;
using Newtonsoft.Json;
using RoasterBeansDataAccess.Models;

namespace SeattleRoasterProject.Data.Services;

public class FavoritesService
{
    public async Task<List<FavoriteEntry>> GetFavorites(IJSRuntime JSRuntime)
    {
        var favoritesString = await JSRuntime.InvokeAsync<string>("GetValueFromStorage", "Favorites");
        if (string.IsNullOrEmpty(favoritesString))
        {
            return new List<FavoriteEntry>();
        }

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

    public async Task AddBeanToFavorites(IJSRuntime JSRuntime, BeanModel bean)
    {
        await JSRuntime.InvokeVoidAsync("AddValueToList", "Favorites",
            new FavoriteEntry { Id = bean.Id, DateAdded = DateTime.Now });
    }

    public async Task RemoveBeanToFavorites(IJSRuntime JSRuntime, BeanModel bean)
    {
        await JSRuntime.InvokeVoidAsync("RemoveValueFromList", "Favorites", bean.Id);
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