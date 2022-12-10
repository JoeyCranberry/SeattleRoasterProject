namespace SeattleRoasterProject.Data.Services
{
	public class EnviromentSettings
	{
		public bool ShowProductionInvisible { get; set; } = false;
		public string EnviromentName { get; set; } = "Production";

		public enum Enviroment
		{ 
			DEVELOPMENT,
			STAGING,
			PRODUCTION
		}

		public Enviroment GetEnviroment(IConfiguration config)
		{
			var envString = config.GetValue<string>("Enviroment", "Unknown");
			switch (envString)
			{
				case "Development":
					return Enviroment.DEVELOPMENT;
				case "Staging":
					return Enviroment.STAGING;
				case "Production":
				default:
					return Enviroment.PRODUCTION;
			}
		}

		public bool GetShowProductionInvisible(IConfiguration config)
		{
			return config.GetValue<bool>("ShowProductionInvisible", false);
		}
	}
}
