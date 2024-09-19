using MongoDB.Bson.Serialization.Attributes;

namespace RoasterBeansDataAccess.Models
{
	public class TastingNoteModel
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string Id { get; set; }
		public string NoteName { get; set; } = string.Empty;
		public List<string>? Aliases { get; set; }
		public List<NoteCategory>? Categories { get; set; }
		public List<NoteSubCategory>? SubCategories { get; set; }

		public List<NoteSubCategory> GetPossibleSubCategories()
		{
			var possibleSubCategories = new List<NoteSubCategory>();

			if(Categories == null)
			{
				return possibleSubCategories;
			}

			foreach(var category in Categories)
			{
				possibleSubCategories.AddRange(GetSubCategoriesInCategory(category));
			}

			if(SubCategories != null)
			{
				possibleSubCategories.RemoveAll(subCategory => SubCategories.Contains(subCategory));
			}
			
			return possibleSubCategories;
		}

		public bool NoteMatchesNameOrAlias(string note)
		{
			return NoteName.ToLower().Trim() == note.ToLower().Trim()
				|| (Aliases != null && Aliases.Contains(note.ToLower().Trim()) );
		}

		public static string GetNoteCategoryDisplayName(NoteCategory category)
		{
			switch (category)
			{
				case NoteCategory.NuttyCocoa:
					return "Nutty/Cocoa";
				case NoteCategory.SourFermented:
					return "Source/Fermented";
				case NoteCategory.GreenVegative:
					return "Green/Vegative";
				default:
					return category.ToString();
			}
		}

		public static string GetSubNoteCategoryDisplayName(NoteSubCategory subCategory)
		{
			return subCategory.ToString();
		}

		private static List<NoteSubCategory> GetSubCategoriesInCategory(NoteCategory category)
		{
			switch(category)
			{
				case NoteCategory.Roasted:
					return Enum.GetValues<NoteSubCategory>().Where(val => (int)val < 100).ToList();
				case NoteCategory.Spices:
					return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 100 && (int)val < 200).ToList();
				case NoteCategory.NuttyCocoa:
					return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 200 && (int)val < 300).ToList();
				case NoteCategory.Sweet:
					return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 300 && (int)val < 400).ToList();
				case NoteCategory.Floral:
					return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 400 && (int)val < 500).ToList();
				case NoteCategory.Fruity:
					return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 500 && (int)val < 600).ToList();
				case NoteCategory.SourFermented:
					return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 600 && (int)val < 700).ToList();
				case NoteCategory.GreenVegative:
					return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 700 && (int)val < 800).ToList();
				case NoteCategory.Other:
					return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 800).ToList();
				default:
					return Enum.GetValues<NoteSubCategory>().ToList();
			}	
		}
	}

	public enum NoteCategory
	{
		Roasted = 0,
		Spices = 1,
		NuttyCocoa = 2,
		Sweet = 3,
		Floral = 4,
		Fruity = 5,
		SourFermented = 6,
		GreenVegative = 7,
		Other = 8
	}

	public enum NoteSubCategory
	{
		//Roasted,
		Cereal = 000,
		Burnt = 001,
		Tobacco = 002,
		PipeTobacco = 003,
		//Spices,
		BrownSpice = 100,
		Pepper = 101,
		Pungent = 102,
		//NuttyCocoa,
		Cocoa = 200,
		Nutty = 201,
		//Sweet,
		BrownSugar = 300,
		Vanilla = 301,
		Vanillin = 302,
		OverallSweet = 303,
		SweetAromatics = 304,
		//Floral,
		Floral = 400,
		BlackTea = 401,
		//Fruity,
		Berry = 500,
		DriedFruit = 501,
		OtherFruit = 502,
		CitrusFruit = 503,
		//SourFermented,
		Sour = 600,
		AlcoholFermented = 601,
		//GreenVegative,
		OliveOil = 700,
		Raw = 701,
		GreenVegative = 702,
		Beany = 703,
		//Other
		Chemical = 800,
		PaperyMusty = 801
	}
}
