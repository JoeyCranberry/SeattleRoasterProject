namespace SeattleRoasterProject.Data.Services
{
	public class EnvironmentSettings
	{
		public bool ShowProductionInvisible { get; set; } = false;
		public string EnvironmentName { get; set; } = "Production";

		public enum Environment
		{ 
			DEVELOPMENT,
			STAGING,
			PRODUCTION
		}

		public Environment GetEnvironment(IConfiguration config)
		{
			var envString = config.GetValue<string>("Enviroment", "Unknown");
			switch (envString)
			{
				case "Development":
					return Environment.DEVELOPMENT;
				case "Staging":
					return Environment.STAGING;
				case "Production":
				default:
					return Environment.PRODUCTION;
			}
		}

		public bool GetShowProductionInvisible(IConfiguration config)
		{
			return config.GetValue<bool>("ShowProductionInvisible", false);
		}
	}
}
