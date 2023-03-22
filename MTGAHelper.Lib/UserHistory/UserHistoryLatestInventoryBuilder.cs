using MTGAHelper.Entity;
using MTGAHelper.Server.DataAccess;
using MTGAHelper.Server.DataAccess.Queries;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.UserHistory
{
    public class UserHistoryLatestInventoryBuilder
    {
        private readonly IQueryHandler<LatestInventoryQuery, Inventory> qLatestInventory;

        public UserHistoryLatestInventoryBuilder(
            IQueryHandler<LatestInventoryQuery, Inventory> qLatestInventoryOnDay
            )
        {
            this.qLatestInventory = qLatestInventoryOnDay;
        }

        public async Task<Inventory> Get(string userId)
        {
            var inventory = await qLatestInventory.Handle(new LatestInventoryQuery(userId));
            return inventory;
        }
    }
}