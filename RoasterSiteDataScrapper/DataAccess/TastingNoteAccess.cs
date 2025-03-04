using MongoDB.Driver;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.Mongo;

namespace RoasterBeansDataAccess.DataAccess;

public class TastingNoteAccess
{
    #region Delete

    public static async Task<bool> DeleteTastingNote(TastingNoteModel delNote, bool isDevelopment = false)
    {
        var collection = GetTastingNotesCollection(isDevelopment);

        if (collection == null)
        {
            return false;
        }

        try
        {
            await collection.DeleteOneAsync(note => note.Id == delNote.Id);

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

    private static IMongoCollection<TastingNoteModel>? GetTastingNotesCollection(bool isDevelopment = true)
    {
        var connString = Credentials.GetConnectionString(isDevelopment);
        var dbName = "SeattleRoasters";
        var collectionName = "TastingNotes";

        var client = new MongoClient(connString);
        var db = client.GetDatabase(dbName);
        return db.GetCollection<TastingNoteModel>(collectionName);
    }

    #endregion

    #region Select

    public static async Task<TastingNoteModel> GetTastingNoteByNameOrAlias(string name, bool isDevelopment = false)
    {
        var collection = GetTastingNotesCollection(isDevelopment);

        var results = await collection.FindAsync(note => note.NoteName == name || note.Aliases.Contains(name));

        return results.First();
    }

    public static async Task<TastingNoteModel> GetTastingNoteById(string id, bool isDevelopment = false)
    {
        var collection = GetTastingNotesCollection(isDevelopment);

        var results = await collection.FindAsync(note => note.Id == id);

        return results.First();
    }

    public static async Task<List<TastingNoteModel>> GetAllTastingNotes(bool isDevelopment = false)
    {
        var collection = GetTastingNotesCollection(isDevelopment);

        var results = await collection.FindAsync(_ => true);

        return results.ToList();
    }

    #endregion

    #region Insert

    public static async Task<bool> AddTastingNote(TastingNoteModel tastingNote, bool isDevelopment = false)
    {
        var collection = GetTastingNotesCollection(isDevelopment);

        if (collection == null)
        {
            return false;
        }

        var matchingNotesInDb = await collection.FindAsync(note => note.NoteName == tastingNote.NoteName);

        if (matchingNotesInDb.Any())
        {
            return false;
        }

        try
        {
            await collection.InsertOneAsync(tastingNote);

            return true;
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc);
            return false;
        }
    }

    public async Task<bool> AddTastingNotes(List<TastingNoteModel> tastingNotes, bool isDevelopment = false)
    {
        var collection = GetTastingNotesCollection(isDevelopment);

        if (collection == null)
        {
            return false;
        }

        try
        {
            await collection.InsertManyAsync(tastingNotes);

            return true;
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc);
            return false;
        }
    }

    #endregion

    #region Replace

    public static async Task<bool> ReplaceTastingNote(TastingNoteModel oldNote, TastingNoteModel newNote,
        bool isDevelopment = false)
    {
        var collection = GetTastingNotesCollection(isDevelopment);

        if (collection == null)
        {
            return false;
        }

        try
        {
            await collection.ReplaceOneAsync(note => note.Id == oldNote.Id, newNote);

            return true;
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc);
            return false;
        }
    }

    public static async Task<bool> UpdateTastingNote(TastingNoteModel editNote, bool isDevelopment = false)
    {
        var collection = GetTastingNotesCollection(isDevelopment);

        if (collection == null)
        {
            return false;
        }

        try
        {
            await collection.ReplaceOneAsync(note => note.Id == editNote.Id, editNote);

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