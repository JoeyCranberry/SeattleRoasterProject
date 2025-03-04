using Microsoft.AspNetCore.Components;
using RoasterBeansDataAccess.Models;
using SeattleRoasterProject.Core.Enums;

namespace SeattleRoasterProject.Data.Services;

public class TastingNoteCategoryService
{
    private readonly TastingNoteService _tastingNoteService;

    private List<TastingNoteModel>? _allTastingNotes;

    public TastingNoteCategoryService(TastingNoteService tastingNoteService)
    {
        _tastingNoteService = tastingNoteService;
    }

    public async Task<MarkupString> GetIcon(string nameOrAlias)
    {
        if (_allTastingNotes == null)
        {
            await Initialize();
        }

        var matchingNote = _allTastingNotes?.FirstOrDefault(note =>
            note.NoteName == nameOrAlias || (note.Aliases != null && note.Aliases.Contains(nameOrAlias)));

        return (MarkupString)GetIconByTastingNote(matchingNote);
    }

    private string GetIconByTastingNote(TastingNoteModel? tastingNote)
    {
        if (tastingNote?.Categories == null)
        {
            return string.Empty;
        }

        foreach (var category in tastingNote.Categories)
        {
            var icon = GetIconByTastingCategory(category);

            if (icon != null)
            {
                return icon;
            }
        }

        return string.Empty;
    }

    private string? GetIconByTastingCategory(NoteCategory noteCategory)
    {
        switch (noteCategory)
        {
            case NoteCategory.Roasted:
                return "<span class='bi bi-fire'></span>";
            case NoteCategory.Spices:
                return "<span class='bi bi-fire'></span>";
            case NoteCategory.Nutty_Cocoa:
                return "<span class='bi bi-fire'></span>";
            case NoteCategory.Sweet:
                return "<span class='bi bi-fire'></span>";
            case NoteCategory.Floral:
                return "<span class='bi bi-flower2'></span>";
            case NoteCategory.Fruity:
                return "<span class='bi bi-fire'></span>";
            case NoteCategory.Sour_Fermented:
                return "fire";
            case NoteCategory.Green_Vegative:
                return "fire";
            case NoteCategory.Other:
                return "fire";
            default:
                return null;
        }
    }

    private async Task Initialize()
    {
        _allTastingNotes = await _tastingNoteService.GetAllTastingNotes();
    }
}