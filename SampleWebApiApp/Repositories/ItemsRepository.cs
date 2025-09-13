namespace SampleWebApiApp.Repositories
{
    public class ItemsRepository : IItemsRepository
    {
        public Task<List<Item>> GetItems(int param1)
        {
            if (param1 < 100)
            {
                return Task.FromResult(new List<Item>()
                {
                    new Item(1, "Item1", 1000),
                    new Item(2, "Item2", 3000),
                    new Item(3, "Item3", 2000),
                });
            }
            else
            {
                return Task.FromResult(new List<Item>());
            }
        }
    }

    public record Item(int id, string name, double amount);
}
