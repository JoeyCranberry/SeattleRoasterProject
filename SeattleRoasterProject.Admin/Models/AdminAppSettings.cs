namespace SeattleRoasterProject.Admin.Models;

using Core.Interfaces;

public class AdminAppSettings : IAppSettings
{
    public static string SectionName = "EnvironmentEnum";
    public string EnvironmentName { get; set; } = string.Empty;
    public bool ShowProductionInvisible { get; set; }
}
