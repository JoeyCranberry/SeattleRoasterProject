namespace SeattleRoasterProject.Core.Models;

public class BaseResponse
{
    public bool IsSuccessful { get; set; }
    public string Message { get; set; } = string.Empty;
}
