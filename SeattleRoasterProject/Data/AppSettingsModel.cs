namespace SeattleRoasterProject.Data
{
	public class AppSettingsModel
	{
		public const string SectionName = "Environment";

		public string EnvironmentName { get; set; }
		public bool ShowProductionInvisible { get; set; }
	}
}
