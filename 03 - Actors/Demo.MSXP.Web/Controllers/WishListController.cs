using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demo.MSXP.Domain;
using Demo.MSXP.Interfaces.Actors;
using Demo.MSXP.Interfaces.Services;
using Demo.MSXP.Web.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Query;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Demo.MSXP.Web.Controllers
{
    [Route("api/user/wishlist")]
    public class WishListController : Controller
    {
        [HttpPost("")]
        public async Task<IActionResult> CreateWishList([FromBody] WishList wishlist)
        {

            var actor = ActorProxy.Create<IWishlistActor>(new ActorId(wishlist.Name));

            await actor.CreateWishlist(wishlist);

            foreach (var item in wishlist.Items)
            {
                await actor.AddItemToWishlist(item);
            }

            return NoContent();
        }

        [HttpGet("")]
        public async Task<IActionResult> GetWhishList()
        {
            ServiceUriBuilder builder = new ServiceUriBuilder("WishlistActorService");
            Uri serviceName = builder.ToUri();

            IList<WishList> results = new List<WishList>();

            FabricClient client = new FabricClient();
            ServicePartitionList partitions = await client.QueryManager.GetPartitionListAsync(serviceName);

            List<ActorInformation> activeActors = new List<ActorInformation>();

            foreach (var partition in partitions)
            {
                long minKey = (partition.PartitionInformation as Int64RangePartitionInformation).LowKey;

                var actorServiceProxy = ActorServiceProxy.Create(builder.ToUri(), minKey);

                ContinuationToken continuationToken = null;

                do
                {
                    PagedResult<ActorInformation> page = await actorServiceProxy.GetActorsAsync(continuationToken, CancellationToken.None);

                    activeActors.AddRange(page.Items.Where(x => x.IsActive));


                    continuationToken = page.ContinuationToken;
                }
                while (continuationToken != null);

            }


            foreach (var info in activeActors.Distinct())
            {
                var actor = ActorProxy.Create<IWishlistActor>(info.ActorId);

                var result = await actor.GetWishList();
                if (result != null)
                {
                    results.Add(result);
                }
            }


            
            return Ok(results);
        }

    }
}
