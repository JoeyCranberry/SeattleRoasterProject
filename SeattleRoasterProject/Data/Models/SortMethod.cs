namespace SeattleRoasterProject.Data.Models;

using Enums;

public class SortMethod
{
    public bool IsLowToHigh { get; set; } = false;
    public SortField SortByField { get; set; } = SortField.Default;
}