using Microsoft.JSInterop;
using System.Xml.Linq;
using Newtonsoft.Json;
using RoasterBeansDataAccess.Models;

namespace SeattleRoasterProject.Data.Services
{
	public class FavoritesService
	{
		public async Task<List<FavoriteEntry>> GetFavorites(IJSRuntime JSRuntime)
		{
			var favoritesString = await JSRuntime.InvokeAsync<string>("GetValueFromStorage", "Favorites");
			if(String.IsNullOrEmpty( favoritesString))
			{
				return new();
			}

			favoritesString = "{\"Favorites\":" + favoritesString + "}";

			if (String.IsNullOrEmpty(favoritesString))
			{
				return new();
			}

			FavoriteStorageObject? favorites = JsonConvert.DeserializeObject<FavoriteStorageObject>(favoritesString);
			if(favorites == null)
			{
				return new();
			}

			return favorites.Favorites.ToList();
		}

		public async Task AddBeanToFavorites(IJSRuntime JSRuntime, BeanModel bean)
		{
			await JSRuntime.InvokeVoidAsync("AddValueToList", "Favorites", new FavoriteEntry() { Id = bean.Id, DateAdded = DateTime.Now });
		}

		public async Task RemoveBeanToFavorites(IJSRuntime JSRuntime, BeanModel bean)
		{
			await JSRuntime.InvokeVoidAsync("RemoveValueFromList", "Favorites", new FavoriteEntry() { Id = bean.Id, DateAdded = DateTime.Now });
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
}
