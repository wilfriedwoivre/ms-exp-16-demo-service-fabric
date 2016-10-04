using System.Threading.Tasks;
using Demo.MSXP.Domain;
using Microsoft.ServiceFabric.Actors;

namespace Demo.MSXP.Interfaces.Actors
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IWishlistActor : IActor
    {
        Task CreateWishlist(WishList wishlist);

        Task AddItemToWishlist(WishListItem item);

        Task<WishList> GetWishList();
    }
}
