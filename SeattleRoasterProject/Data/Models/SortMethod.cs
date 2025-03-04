namespace SeattleRoasterProject.Data.Models;

public class SortMethod
{
    public enum SortField
    {
        Default,
        Price,
        Date_Added,
        Alphabetical,
        Roaster
    }

    public bool IsLowToHigh { get; set; } = false;
    public SortField SortByField { get; set; } = SortField.Default;
}