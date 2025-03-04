namespace SeattleRoasterProject.Data.Services;

public class EnvironmentSettings
{
    public enum EnvironmentEnum
    {
        Development,
        Staging,
        Production
    }

    public EnvironmentEnum Environment;
    public bool IsDevelopment;

    public EnvironmentSettings(AppSettingsModel appSettings)
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