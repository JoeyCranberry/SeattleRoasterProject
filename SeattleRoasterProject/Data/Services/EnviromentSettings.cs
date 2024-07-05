namespace SeattleRoasterProject.Data.Services
{
	public class EnvironmentSettings
	{
		public bool ShowProductionInvisible { get; set; } = false;
		public string EnvironmentName { get; set; } = "Production";

        public EnvironmentSettings(AppSettingsModel appSettings)
        {
	        EnvironmentName = appSettings.EnvironmentName;
			ShowProductionInvisible = appSettings.ShowProductionInvisible;
		}

        public enum Environment
		{ 
			Development,
			Staging,
			Production
		}

		public Environment GetEnvironment()
		{
			switch (EnvironmentName)
			{
				case "Development":
					return Environment.Development;
				case "Staging":
					return Environment.Staging;
				default:
					return Environment.Production;
			}
		}
	}
}
