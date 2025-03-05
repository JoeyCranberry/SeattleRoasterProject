using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;
using SeattleRoasterProject.Core.Models;

namespace SeattleRoasterProject.Data.Services;

public class TastingNoteService
{
    private readonly EnvironmentSettings _environmentSettings;
    private readonly bool _isDevelopment;

    public TastingNoteService(EnvironmentSettings environmentSettings)
    {
        _environmentSettings = environmentSettings;
        _isDevelopment = environmentSettings.IsDevelopment;
    }

    public async Task<TastingNoteModel> GetTastingNoteById(string id)
    {
        return await TastingNoteAccess.GetTastingNoteById(id, _isDevelopment);
    }

    public async Task<TastingNoteModel> GetTastingNoteNameOrAlias(string name)
    {
        return await TastingNoteAccess.GetTastingNoteByNameOrAlias(name, _isDevelopment);
    }

    public async Task<List<TastingNoteModel>> GetAllTastingNotes()
    {
        return await TastingNoteAccess.GetAllTastingNotes(_isDevelopment);
    }

    public async Task<bool> AddTastingNoteToDb(TastingNoteModel newNote)
    {
        return await TastingNoteAccess.AddTastingNote(newNote, _isDevelopment);
    }

    public async Task<bool> UpdateExistingBean(TastingNoteModel editNote)
    {
        return await TastingNoteAccess.UpdateTastingNote(editNote, _isDevelopment);
    }

    public async Task<bool> DeleteTastingNote(TastingNoteModel delNote)
    {
        return await TastingNoteAccess.DeleteTastingNote(delNote, _isDevelopment);
    }
}