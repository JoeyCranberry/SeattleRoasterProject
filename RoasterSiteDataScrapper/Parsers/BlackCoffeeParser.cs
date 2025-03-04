using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

/*
 * This roaster has a LOT of non-coffee listings, and just one whole bean listing - so just keep static for now
 */
public class BlackCoffeeParser
{
    public static ParseContentResult ParseBeansForRoaster(RoasterModel roaster)
    {
        return ParseBeans(roaster);
    }

    private static ParseContentResult ParseBeans(RoasterModel roaster)
    {
        var result = new ParseContentResult();
        result.IsSuccessful = true;

        result.Listings = new List<BeanModel>
        {
            new()
            {
                RoasterId = roaster.RoasterId,
                MongoRoasterId = roaster.Id,
                FullName = "Black Coffee Whole beans",
                ProductURL = "https://black-coffee-northwest.square.site/product/black-coffee-whole-beans/188",
                PriceBeforeShipping = 20.00M,
                ImageURL =
                    "https://black-coffee-northwest.square.site/uploads/1/3/4/1/134197671/s515571055682279478_p188_i8_w1080.jpeg"
            }
        };

        return result;
    }
}