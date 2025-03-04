using Microsoft.AspNetCore.Components;

namespace SeattleRoasterProject.Data.Models;

public class SortMethodOptions
{
    public MarkupString Label;
    public SortMethod Method = new();
    public string SelectedClass = "";

    public static List<SortMethodOptions> GetSortMethods()
    {
        return new List<SortMethodOptions>
        {
            new SortMethodOptions
            {
                Label = (MarkupString)"Recommended",
                Method = new SortMethod { SortByField = SortMethod.SortField.Default }, SelectedClass = "selectedOption"
            },
            new SortMethodOptions
            {
                Label = (MarkupString)"Price $<i class=\"bi bi-arrow-right\"></i>$$$",
                Method = new SortMethod { SortByField = SortMethod.SortField.Price, IsLowToHigh = true }
            },
            new SortMethodOptions
            {
                Label = (MarkupString)"Price $$$<span class=\"bi bi-arrow-right\"></span>$",
                Method = new SortMethod { SortByField = SortMethod.SortField.Price, IsLowToHigh = false }
            },
            new SortMethodOptions
            {
                Label = (MarkupString)"Newest",
                Method = new SortMethod { SortByField = SortMethod.SortField.Date_Added, IsLowToHigh = false }
            },
            new SortMethodOptions
            {
                Label = (MarkupString)"Oldest",
                Method = new SortMethod { SortByField = SortMethod.SortField.Date_Added, IsLowToHigh = true }
            },
            new SortMethodOptions
            {
                Label = (MarkupString)"Name A<span class=\"bi bi-arrow-right\"></span>Z",
                Method = new SortMethod { SortByField = SortMethod.SortField.Alphabetical, IsLowToHigh = true }
            },
            new SortMethodOptions
            {
                Label = (MarkupString)"Name Z<span class=\"bi bi-arrow-right\"></span>A",
                Method = new SortMethod { SortByField = SortMethod.SortField.Alphabetical, IsLowToHigh = false }
            },
            new SortMethodOptions
            {
                Label = (MarkupString)"Roaster A<span class=\"bi bi-arrow-right\"></span>Z",
                Method = new SortMethod { SortByField = SortMethod.SortField.Roaster, IsLowToHigh = true }
            },
            new SortMethodOptions
            {
                Label = (MarkupString)"Roaster Z<span class=\"bi bi-arrow-right\"></span>A",
                Method = new SortMethod { SortByField = SortMethod.SortField.Roaster, IsLowToHigh = false }
            }
        };
    }
}