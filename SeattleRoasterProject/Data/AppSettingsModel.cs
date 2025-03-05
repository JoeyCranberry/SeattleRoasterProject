using SeattleRoasterProject.Core.Interfaces;

namespace SeattleRoasterProject.Data;

public class AppSettingsModel : IAppSettings
{
    public const string SectionName = "EnvironmentEnum";

    public string EnvironmentName { get; set; } = string.Empty;
    public bool ShowProductionInvisible { get; set; }
}