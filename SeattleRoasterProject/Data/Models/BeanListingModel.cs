using RoasterBeansDataAccess.Models;

namespace SeattleRoasterProject.Data.Models;

public class BeanListingModel
{
    public BeanModel Bean { get; set; } = new();
    public RoasterModel Roaster { get; set; } = new();

    public static BeanListingModel FromBeanAndRoasters(BeanModel bean, List<RoasterModel> roasters)
    {
        var roaster = roasters.FirstOrDefault(roaster => roaster.Id == bean.MongoRoasterId);

        if (roaster == null)
        {
            Console.WriteLine(
                $"Failed to match bean {bean.FullName} to a roaster. Checked list of {roasters.Count} roaster records.");
        }

        return new BeanListingModel
        {
            Bean = bean,
            Roaster = roaster ?? new RoasterModel()
        };
    }
}