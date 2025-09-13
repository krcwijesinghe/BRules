
namespace SampleWebApiApp.Repositories
{
    public interface IItemsRepository
    {
        Task<List<Item>> GetItems(int param1);
    }
}