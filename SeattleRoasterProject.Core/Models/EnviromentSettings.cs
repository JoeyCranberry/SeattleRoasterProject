namespace SeattleRoasterProject.Core.Models;

using Enums;
using Interfaces;

public class EnvironmentSettings
{
    public EnvironmentEnum Environment;
    public bool IsDevelopment;

    public EnvironmentSettings(IAppSettings appSettings)
    {
        EnvironmentName = appSettings.EnvironmentName;
        ShowProductionInvisible = appSettings.ShowProductionInvisible;
        Environment = GetEnvironment();
        IsDevelopment = Environment == EnvironmentEnum.Development;
    }

    public bool ShowProductionInvisible { get; set; }
    public string EnvironmentName { get; set; } = "Production";

    private EnvironmentEnum GetEnvironment()
    {
        switch (EnvironmentName)
        {
            case "Development":
                return EnvironmentEnum.Development;
            case "Staging":
                return EnvironmentEnum.Staging;
            default:
                return EnvironmentEnum.Production;
        }
    }
}