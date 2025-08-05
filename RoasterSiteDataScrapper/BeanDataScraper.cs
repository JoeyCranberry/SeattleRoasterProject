namespace RoasterBeansDataAccess;

using DataAccess;
using Models;
using Parsers;

public static class BeanDataScraper
{
    public static async Task<BeanListingDifferenceModel> GetBeanListingDifference(RoasterModel roaster)
    {
        var parsedListings = await ParseListings(roaster);
        
        if (!parsedListings.IsSuccessful || parsedListings.Listings == null)
        {
            return new BeanListingDifferenceModel(parsedListings.Exceptions);
        }

        var storedListings = await BeanAccess.GetBeansByRoaster(roaster, true);

        List<BeanModel> newListings = new();
        List<BeanModel> activatedListings = new();

        foreach (var listing in parsedListings.Listings)
        {
            var matchedStoredListing = storedListings.FirstOrDefault(stored => stored.ProductURL == listing.ProductURL
                                                                               && stored.IsProductionVisible);

            if (matchedStoredListing != null)
            {
                if (matchedStoredListing.IsActiveListing != null && !matchedStoredListing.IsActiveListing.Value)
                {
                    activatedListings.Add(matchedStoredListing);
                    newListings.Remove(listing);
                }
            }
            else
            {
                newListings.Add(listing);
            }
        }

        // Add any listings where they exist in stored listings but not parsed listings
        var removedListings = storedListings.Where(b => parsedListings.Listings.All(parsed => parsed.ProductURL != b.ProductURL) && storedListings.Any(stored => stored.ProductURL == b.ProductURL)).ToList();

        // Removed any listings from parsed listings where product URL is already stored
        newListings.RemoveAll(b => storedListings.Any(stored =>
            stored.ProductURL == b.ProductURL && stored.IsActiveListing.HasValue &&
            stored.IsActiveListing.Value == false));

        return new BeanListingDifferenceModel(newListings, removedListings, activatedListings, true);
    }

    private static async Task<ParseContentResult> ParseListings(RoasterModel roaster)
    {
        switch (roaster.Id)
        {
            case "636c4d4c720cf76568f2d200":
                return await AnchorheadParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d202":
                return await ArmisticeParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d203":
                return await AvoleParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d21e":
                return await BatdorfBronsonParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d201":
                return await BellinghamParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d204":
                return BlackCoffeeParser.ParseBeansForRoaster(roaster);
            case "637567f889596e3d71617703":
                return await BlossomParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d205":
                return await BluebeardParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d206":
                return await BoonBoonaParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d207":
                return await BroadcastParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d208":
                return await CafeAllegroParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d20b":
                return await CaffeLadroParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d20c":
                return await CaffeLussoParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d20d":
                return await CaffeUmbriaParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d20e":
                return await CaffeVitaParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d20f":
                return await CamberParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d210":
                return await CampfireParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d212":
                return await CloudCityParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d213":
                return await ConduitParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d214":
                return await CuttersPointParser.ParseBeansForRoaster(roaster);
            case "6375688889596e3d71617704":
                return await DorotheaParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d215":
                return await DoteParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d217":
                return await ElmCoffeeParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d219":
                return await FincaBrewParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d21a":
                return await FonteParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d21b":
                return await FulcrumParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d21d":
                return await HaitiParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d21c":
                return await HerkimerParser.ParseBeansForRoaster(roaster);
            case "636ee3dfe54fd43508922c31":
                return await QEDParser.ParseBeansForRoaster(roaster);
            case "636ee4a4e54fd43508922c32":
                return await QueenAnneParser.ParseBeansForRoaster(roaster);
            case "636edcdee54fd43508922c29":
                return await SlateParser.ParseBeansForRoaster(roaster);
            case "636eddd2e54fd43508922c2a":
                return await StampActParser.ParseBeansForRoaster(roaster);
            case "636edeb2e54fd43508922c2b":
                return await ThrulineParser.ParseBeansForRoaster(roaster);
            case "636c4d4c720cf76568f2d21f":
                return await TrueNorthParser.ParseBeansForRoaster(roaster);
            case "636edf82e54fd43508922c2c":
                return await UglyMugParser.ParseBeansForRoaster(roaster);
            case "636ee007e54fd43508922c2d":
                return await VahallaParser.ParseBeansForRoaster(roaster);
            case "636ee09de54fd43508922c2e":
                return await VeltonParser.ParseBeansForRoaster(roaster);
            case "636ee20ee54fd43508922c2f":
                return await VictrolaParser.ParseBeansForRoaster(roaster);
            case "636ee2b1e54fd43508922c30":
                return await VinylParser.ParseBeansForRoaster(roaster);
            case "636ee4f7e54fd43508922c33":
                return await ZokaParser.ParseBeansForRoaster(roaster);
        }

        return new ParseContentResult
        {
            IsSuccessful = false
        };
    }
}

