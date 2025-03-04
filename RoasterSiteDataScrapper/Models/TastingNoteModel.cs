using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SeattleRoasterProject.Core.Enums;

namespace RoasterBeansDataAccess.Models;

public class TastingNoteModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string NoteName { get; set; } = string.Empty;
    public List<string>? Aliases { get; set; }
    public List<NoteCategory>? Categories { get; set; }
    public List<NoteSubCategory>? SubCategories { get; set; }

    public List<NoteSubCategory> GetPossibleSubCategories()
    {
        var possibleSubCategories = new List<NoteSubCategory>();

        if (Categories == null)
        {
            return possibleSubCategories;
        }

        foreach (var category in Categories)
        {
            possibleSubCategories.AddRange(GetSubCategoriesInCategory(category));
        }

        if (SubCategories != null)
        {
            possibleSubCategories.RemoveAll(subCategory => SubCategories.Contains(subCategory));
        }

        return possibleSubCategories;
    }

    public bool NoteMatchesNameOrAlias(string note)
    {
        return NoteName.ToLower().Trim() == note.ToLower().Trim()
               || (Aliases != null && Aliases.Contains(note.ToLower().Trim()));
    }

    public static string GetNoteCategoryDisplayName(NoteCategory category)
    {
        switch (category)
        {
            case NoteCategory.Nutty_Cocoa:
                return "Nutty/Cocoa";
            case NoteCategory.Sour_Fermented:
                return "Source/Fermented";
            case NoteCategory.Green_Vegative:
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
        switch (category)
        {
            case NoteCategory.Roasted:
                return Enum.GetValues<NoteSubCategory>().Where(val => (int)val < 100).ToList();
            case NoteCategory.Spices:
                return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 100 && (int)val < 200).ToList();
            case NoteCategory.Nutty_Cocoa:
                return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 200 && (int)val < 300).ToList();
            case NoteCategory.Sweet:
                return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 300 && (int)val < 400).ToList();
            case NoteCategory.Floral:
                return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 400 && (int)val < 500).ToList();
            case NoteCategory.Fruity:
                return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 500 && (int)val < 600).ToList();
            case NoteCategory.Sour_Fermented:
                return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 600 && (int)val < 700).ToList();
            case NoteCategory.Green_Vegative:
                return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 700 && (int)val < 800).ToList();
            case NoteCategory.Other:
                return Enum.GetValues<NoteSubCategory>().Where(val => (int)val >= 800).ToList();
            default:
                return Enum.GetValues<NoteSubCategory>().ToList();
        }
    }
}