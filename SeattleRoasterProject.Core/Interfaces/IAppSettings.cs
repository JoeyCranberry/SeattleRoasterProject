namespace SeattleRoasterProject.Core.Interfaces;

public interface IAppSettings
{
    public string EnvironmentName { get; set; }
    public bool ShowProductionInvisible { get; set; }
}