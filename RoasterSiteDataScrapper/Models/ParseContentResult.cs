namespace RoasterBeansDataAccess.Models;

using SeattleRoasterProject.Core.Models;

public class ParseContentResult : BaseResponse
{
    public List<BeanModel>? Listings { get; set; }
    public List<Exception> Exceptions { get; set; } = new();
    public int FailedParses { get; set; }
}