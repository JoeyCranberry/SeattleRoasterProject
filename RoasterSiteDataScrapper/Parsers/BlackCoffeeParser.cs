using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class BlackCoffeeParser
	{
		private static List<string> excludedTerms = new List<string> {  };
		private const string baseURL = "https://black-coffee-northwest.square.site/";

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			return new List<BeanModel>()
			{
				new BeanModel()
				{
					RoasterId = roaster.RoasterId,
					MongoRoasterId = roaster.Id,
					FullName = "Black Coffee Whole beans",
					ProductURL = "https://black-coffee-northwest.square.site/product/black-coffee-whole-beans/188",
					PriceBeforeShipping = 20.00M,
					ImageURL = "https://black-coffee-northwest.square.site/uploads/1/3/4/1/134197671/s515571055682279478_p188_i8_w1080.jpeg"
				}
			};
		}
	}
}
