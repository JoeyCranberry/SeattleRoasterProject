using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace SeattleRoasterProject.Data.Services
{
	public class TastingNoteService
	{
		public async Task<TastingNoteModel> GetTastingNoteById(string id, EnvironmentSettings.Environment env)
		{
			return await TastingNoteAccess.GetTastingNoteById(id, env == EnvironmentSettings.Environment.DEVELOPMENT);
		}

		public async Task<TastingNoteModel> GetTastingNoteNameOrAlias(string name, EnvironmentSettings.Environment env)
		{
			return await TastingNoteAccess.GetTastingNoteByNameOrAlias(name, env == EnvironmentSettings.Environment.DEVELOPMENT);
		}

		public async Task<List<TastingNoteModel>> GetAllTastingNotes(EnvironmentSettings.Environment env)
		{
			return await TastingNoteAccess.GetAllTastingNotes(env == EnvironmentSettings.Environment.DEVELOPMENT);
		}

		public async Task<bool> AddTastingNoteToDb(TastingNoteModel newNote, EnvironmentSettings.Environment env)
		{
			return await TastingNoteAccess.AddTastingNote(newNote, env == EnvironmentSettings.Environment.DEVELOPMENT);
		}

		public async Task<bool> UpdateExistingBean(TastingNoteModel editNote, EnvironmentSettings.Environment env)
		{
			return await TastingNoteAccess.UpdateTastingNote(editNote, env == EnvironmentSettings.Environment.DEVELOPMENT);
		}

		public async Task<bool> DeleteTastingNote(TastingNoteModel delNote, EnvironmentSettings.Environment env)
		{
			return await TastingNoteAccess.DeleteTastingNote(delNote, env == EnvironmentSettings.Environment.DEVELOPMENT);
		}
	}
}
