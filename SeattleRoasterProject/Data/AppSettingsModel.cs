namespace SeattleRoasterProject.Data
{
	public class AppSettingsModel
	{
		public const string SectionName = "EnvironmentEnum";

		public string EnvironmentName { get; set; }
		public bool ShowProductionInvisible { get; set; }
	}
}
