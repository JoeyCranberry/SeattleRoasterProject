namespace SeattleRoasterProject.Data.Services
{
	public class EnvironmentSettings
	{
		public bool ShowProductionInvisible { get; set; } = false;
		public string EnvironmentName { get; set; } = "Production";
		public EnvironmentEnum Environment;
		public bool IsDevelopment = false;

        public EnvironmentSettings(AppSettingsModel appSettings)
        {
	        EnvironmentName = appSettings.EnvironmentName;
			ShowProductionInvisible = appSettings.ShowProductionInvisible;
			Environment = GetEnvironment();
			IsDevelopment = Environment == EnvironmentEnum.Development;
		}

        public enum EnvironmentEnum
		{ 
			Development,
			Staging,
			Production
		}

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
}
