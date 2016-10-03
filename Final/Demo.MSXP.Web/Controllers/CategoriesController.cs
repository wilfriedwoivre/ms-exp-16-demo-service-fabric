using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Demo.MSXP.Domain;
using Demo.MSXP.Interfaces;
using Demo.MSXP.Interfaces.Services;
using Demo.MSXP.Web.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;


namespace Demo.MSXP.Web.Controllers
{
    [Route("api/catalog/categories")]
    public class CategoriesController : Controller
    {
        [HttpPost("")]
        public async Task<IActionResult> PostCategory([FromBody]Category category)
        {
            var servicePartitionKey = new ServicePartitionKey(category.Id.GetHashCode());
            ServiceUriBuilder builder = new ServiceUriBuilder("CatalogService");

            ICatalogService service = ServiceProxy.Create<ICatalogService>(builder.ToUri(), servicePartitionKey);

            await service.CreateOrUpdateCategoryAsync(category);
            return CreatedAtAction(nameof(GetCategory), new { categoryId = category.Id }, category);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetCategories()
        {
            ServiceUriBuilder builder = new ServiceUriBuilder("CatalogService");
            Uri serviceName = builder.ToUri();

            List<Category> results = new List<Category>();
            FabricClient client = new FabricClient();
            ServicePartitionList partitions = await client.QueryManager.GetPartitionListAsync(serviceName);

            foreach (var partition in partitions)
            {
                long minKey = (partition.PartitionInformation as Int64RangePartitionInformation).LowKey;
                ICatalogService service = ServiceProxy.Create<ICatalogService>(builder.ToUri(), new ServicePartitionKey(minKey));

                IEnumerable<Category> subResult = await service.GetCategoriesAsync();
                if (subResult != null)
                {
                    results.AddRange(subResult);
                }

            }
            return Ok(results);
        }

        [HttpGet("{categoryId:guid}")]
        public async Task<IActionResult> GetCategory([FromRoute] Guid categoryId)
        {
            try
            {
                var servicePartitionKey = new ServicePartitionKey(categoryId.GetHashCode());
                ServiceUriBuilder builder = new ServiceUriBuilder("CatalogService");

                ICatalogService service = ServiceProxy.Create<ICatalogService>(builder.ToUri(), servicePartitionKey);

                var category = await service.GetCategoryAsync(categoryId);

                return Ok(category);
            }
            catch (Exception e) when (e is KeyNotFoundException || e.InnerException is KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{categoryId:guid}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid categoryId)
        {
            try
            {
                var servicePartitionKey = new ServicePartitionKey(categoryId.GetHashCode());
                ServiceUriBuilder builder = new ServiceUriBuilder("CatalogService");

                ICatalogService service = ServiceProxy.Create<ICatalogService>(builder.ToUri(), servicePartitionKey);

                await service.DeleteCategoryAsync(categoryId);

                return NoContent();
            }
            catch (Exception e) when (e is KeyNotFoundException || e.InnerException is KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCategoriesCount()
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

                    long subResult = await service.GetCategoriesCountAsync();
                    results.Add(subResult);

                }


                return Ok(results.Sum());
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpGet("{categoryId:guid}/products")]
        public async Task<IActionResult> GetProducts([FromRoute] Guid categoryId)
        {
            try
            {
                var servicePartitionKey = new ServicePartitionKey(categoryId.GetHashCode());
                ServiceUriBuilder builder = new ServiceUriBuilder("CatalogService");

                ICatalogService service = ServiceProxy.Create<ICatalogService>(builder.ToUri(), servicePartitionKey);

                var productIds = await service.GetProductsInCategoryAsync(categoryId);
                return Ok(productIds);
            }
            catch (Exception e) when (e is KeyNotFoundException || e.InnerException is KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{categoryId:guid}/products/{productId:guid}")]
        public async Task<IActionResult> PutInCategory([FromRoute] Guid categoryId, [FromRoute] Guid productId)
        {
            try
            {
                var servicePartitionKey = new ServicePartitionKey(categoryId.GetHashCode());
                ServiceUriBuilder builder = new ServiceUriBuilder("CatalogService");

                ICatalogService service = ServiceProxy.Create<ICatalogService>(builder.ToUri(), servicePartitionKey);

                await service.AddProductToCategoryAsync(categoryId, productId);
                return NoContent();
            }
            catch (Exception e) when (e is KeyNotFoundException || e.InnerException is KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{categoryId:guid}/products/{productId:guid}")]
        public async Task<IActionResult> DeleteFromCategory([FromRoute] Guid categoryId, [FromRoute] Guid productId)
        {
            try
            {
                var servicePartitionKey = new ServicePartitionKey(categoryId.GetHashCode());
                ServiceUriBuilder builder = new ServiceUriBuilder("CatalogService");

                ICatalogService service = ServiceProxy.Create<ICatalogService>(builder.ToUri(), servicePartitionKey);

                await service.RemoveProductFromCategoryAsync(categoryId, productId);

                return NoContent();
            }
            catch (Exception e) when (e is KeyNotFoundException || e.InnerException is KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }


}
