namespace RoasterBeansDataAccess.Models;

using SeattleRoasterProject.Core.Models;

public class BeanListingDifferenceModel : BaseResponse
{
    public List<BeanModel> NewListingsListings = new();
    public List<BeanModel> RemovedListingsListings = new();
    public List<BeanModel> ActivatedListingsListings = new();
    public List<Exception> ExceptionsDuringParsing = new();
    public BeanListingDifferenceModel()
    {
        IsSuccessful = false;
    }
    
    public BeanListingDifferenceModel(List<Exception>? exceptions)
    {
        IsSuccessful = false;
        ExceptionsDuringParsing = exceptions;
    }

    public BeanListingDifferenceModel(List<BeanModel> newListings, List<BeanModel> removedListings, List<BeanModel> activatedListings, bool isSuccessful)
    {
        NewListingsListings = newListings;
        RemovedListingsListings = removedListings;
        ActivatedListingsListings = activatedListings;
        IsSuccessful = isSuccessful;
    }
}