using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demo.MSXP.Domain;
using Demo.MSXP.Interfaces.Actors;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;

namespace Demo.MSXP.WishlistActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class WishlistActor : Actor, IWishlistActor, IRemindable
    {
        /// <summary>
        /// Initializes a new instance of WishlistActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public WishlistActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override async Task OnActivateAsync()
        {
            var current = await GetWishlistAsync();
            if (current == null)
            {
                await this.StateManager.SetStateAsync<List<WishListItem>>("items", new List<WishListItem>());
                await this.StateManager.SetStateAsync<DateTime>("createdAt", DateTime.UtcNow);
                await this.StateManager.SetStateAsync<bool>("isActive", true);

                await this.RegisterReminderAsync("selfdestroy", null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            }
        }

        private async Task<IEnumerable<WishListItem>> GetWishlistAsync()
        {
            var items = await this.StateManager.TryGetStateAsync<List<WishListItem>>("items");

            return items.HasValue ? items.Value : null;
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
        {
            switch (reminderName)
            {
                case "selfdestroy":
                    var createdAt = await this.StateManager.GetStateAsync<DateTime>("createdAt");
                    var expiration = await this.StateManager.GetStateAsync<int>("expiration");

                    if (DateTime.UtcNow > createdAt.AddSeconds(expiration))
                    {
                        IActorService serviceProxy = ActorServiceProxy.Create(this.ServiceUri, this.Id);

                        await this.StateManager.SetStateAsync<bool>("isActive", false);
                        //await serviceProxy.DeleteActorAsync(this.Id, CancellationToken.None);
                    }
                    break;
            }
        }

        public async Task CreateWishlist(WishList wishlist)
        {
            await this.StateManager.SetStateAsync("name", wishlist.Name);
            await this.StateManager.SetStateAsync("expiration", wishlist.expiration);
        }

        public async Task AddItemToWishlist(WishListItem item)
        {
            List<WishListItem> items = null;

            if (await this.StateManager.GetStateAsync<bool>("isActive"))
            {
                items = await this.StateManager.GetStateAsync<List<WishListItem>>("items");
            }
            else
            {
                items = new List<WishListItem>();
                await this.StateManager.SetStateAsync<bool>("isActive", true);
            }

            if (items.Any(n => n.ProductId == item.ProductId))
            {
                var temp = items.First(n => n.ProductId == item.ProductId);
                items.RemoveAll(n => n.ProductId == item.ProductId);
                temp.Quantity += item.Quantity;
                items.Add(temp);
            }
            else
            {
                items.Add(item);
            }

            await this.StateManager.SetStateAsync("items", items);
        }

        public async Task<WishList> GetWishList()
        {
            if (await this.StateManager.GetStateAsync<bool>("isActive"))
            {
                WishList wish = new WishList
                {
                    Name = await this.StateManager.GetStateAsync<string>("name"),
                    Items = await this.StateManager.GetStateAsync<List<WishListItem>>("items")
                };



                return wish;
            }
            else
            {
                return null;
            }
        }

    }
}
