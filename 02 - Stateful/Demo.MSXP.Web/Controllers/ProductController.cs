using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Threading.Tasks;
using Demo.MSXP.Domain;
using Demo.MSXP.Interfaces;
using Demo.MSXP.Interfaces.Services;
using Demo.MSXP.Web.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Demo.MSXP.Web.Controllers
{
    [Route("api/catalog/products")]
    public class ProductController : Controller
    {
        [HttpPost("")]
        public async Task<IActionResult> PostProduct([FromBody] Product product)
        {

            var servicePartitionKey = new ServicePartitionKey(product.Id.GetHashCode());
            ServiceUriBuilder builder = new ServiceUriBuilder("CatalogService");

            ICatalogService service = ServiceProxy.Create<ICatalogService>(builder.ToUri(), servicePartitionKey);

            await service.CreateOrUpdateProductAsync(product);

            return CreatedAtAction(nameof(GetProduct), new { productId = product.Id }, product);
        }

        [HttpGet("{productId:guid}")]
        public async Task<IActionResult> GetProduct([FromRoute]Guid productId)
        {
            try
            {
                var servicePartitionKey = new ServicePartitionKey(productId.GetHashCode());
                ServiceUriBuilder builder = new ServiceUriBuilder("CatalogService");

                ICatalogService service = ServiceProxy.Create<ICatalogService>(builder.ToUri(), servicePartitionKey);

                var product = await service.GetProductAsync(productId);
                return Ok(product);
            }
            catch (Exception e) when (e is KeyNotFoundException || e.InnerException is KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{productId:guid}")]
        public async Task<IActionResult> DeleteProduct([FromRoute]Guid productId)
        {
            try
            {
                var servicePartitionKey = new ServicePartitionKey(productId.GetHashCode());
                ServiceUriBuilder builder = new ServiceUriBuilder("CatalogService");

                ICatalogService service = ServiceProxy.Create<ICatalogService>(builder.ToUri(), servicePartitionKey);

                await service.DeleteProductAsync(productId);
                return NoContent();
            }
            catch (Exception e) when (e is KeyNotFoundException || e.InnerException is KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetProductsCount()
        {

            try
            {

                ServiceUriBuilder builder = new ServiceUriBuilder("CatalogService");
                Uri serviceName = builder.ToUri();

                List<long> results = new List<long>();
                FabricClient client = new FabricClient();
                ServicePartitionList partitions = await client.QueryManager.GetPartitionListAsync(serviceName);

                foreach (var partition in partitions)
                {
                    long minKey = (partition.PartitionInformation as Int64RangePartitionInformation).LowKey;
                    ICatalogService service = ServiceProxy.Create<ICatalogService>(builder.ToUri(), new ServicePartitionKey(minKey));

                    long subResult = await service.GetProductsCountAsync();
                    results.Add(subResult);

                }

                return Ok(results.Sum());
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

    }
}
