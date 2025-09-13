
namespace SampleWebApiApp.BusinessLogic;

public interface IItemsManager
{
    Task<Action1Response> HandleAction1Async(int departmentId, int param1);
}

public class Action1Response
{
    public bool IsValid { get; set; }
    public string? Response { get; set; }
    public string? ErrorMessage { get; set; }
}