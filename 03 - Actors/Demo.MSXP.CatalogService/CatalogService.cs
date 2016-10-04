using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demo.MSXP.Domain;
using Demo.MSXP.Interfaces;
using Demo.MSXP.Interfaces.Services;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Demo.MSXP.CatalogService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class CatalogService : StatefulService, ICatalogService
    {
        public CatalogService(StatefulServiceContext context)
            : base(context)
        { }


        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(this.CreateServiceRemotingListener)
            };

        }



        public async Task CreateOrUpdateCategoryAsync(Category category)
        {
            using (var txn = this.StateManager.CreateTransaction())
            {
                var categories = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Category>>("categories");
                await categories.GetOrAddAsync(txn, category.Id, category);
                await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, object>>($"products-category-{category.Id:N}");
                await txn.CommitAsync();
            }
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            using (var txn = this.StateManager.CreateTransaction())
            {
                var result = new List<Category>();
                var categories = await this.StateManager.TryGetAsync<IReliableDictionary<Guid, Category>>("categories");
                if (categories.HasValue)
                {
                    var enumerable = await categories.Value.CreateEnumerableAsync(txn, EnumerationMode.Unordered);
                    var enumerator = enumerable.GetAsyncEnumerator();
                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        result.Add(enumerator.Current.Value);
                    }
                }
                return result;
            }
        }

        public async Task<Category> GetCategoryAsync(Guid categoryId)
        {
            using (var txn = this.StateManager.CreateTransaction())
            {
                var categories = await this.StateManager.TryGetAsync<IReliableDictionary<Guid, Category>>("categories");
                if (!categories.HasValue)
                    throw new KeyNotFoundException($"Category {categoryId} was not found");

                var conditionalValue = await categories.Value.TryGetValueAsync(txn, categoryId);
                if (!conditionalValue.HasValue)
                    throw new KeyNotFoundException($"Category {categoryId} was not found");

                return conditionalValue.Value;
            }
        }

        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            using (var txn = this.StateManager.CreateTransaction())
            {
                var categories = await this.StateManager.TryGetAsync<IReliableDictionary<Guid, Category>>("categories");
                if (!categories.HasValue)
                    throw new KeyNotFoundException($"Category {categoryId} was not found");

                var conditionalValue = await categories.Value.TryRemoveAsync(txn, categoryId);
                if (!conditionalValue.HasValue)
                    throw new KeyNotFoundException($"Category {categoryId} was not found");

                await txn.CommitAsync();
            }
        }

        public async Task<long> GetCategoriesCountAsync()
        {
            using (var txn = this.StateManager.CreateTransaction())
            {
                var categories = await this.StateManager.TryGetAsync<IReliableDictionary<Guid, Category>>("categories");

                return await categories.Value.GetCountAsync(txn);
            }
        }

        public async Task CreateOrUpdateProductAsync(Product product)
        {
            try
            {
                using (var txn = this.StateManager.CreateTransaction())
                {
                    var products = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, Product>>("products");
                    await products.GetOrAddAsync(txn, product.Id, product);
                    await txn.CommitAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<Product> GetProductAsync(Guid productId)
        {
            using (var txn = this.StateManager.CreateTransaction())
            {
                var products = await this.StateManager.TryGetAsync<IReliableDictionary<Guid, Product>>("products");
                if (!products.HasValue)
                    throw new KeyNotFoundException($"Product {productId} was not found");

                var conditionalValue = await products.Value.TryGetValueAsync(txn, productId);
                if (!conditionalValue.HasValue)
                    throw new KeyNotFoundException($"Product {productId} was not found");

                return conditionalValue.Value;
            }
        }

        public async Task DeleteProductAsync(Guid productId)
        {
            using (var txn = this.StateManager.CreateTransaction())
            {
                var products = await this.StateManager.TryGetAsync<IReliableDictionary<Guid, Product>>("products");
                if (!products.HasValue)
                    throw new KeyNotFoundException($"Product {productId} was not found");

                var conditionalValue = await products.Value.TryRemoveAsync(txn, productId);
                if (!conditionalValue.HasValue)
                    throw new KeyNotFoundException($"Product {productId} was not found");

                await txn.CommitAsync();
            }
        }

        public async Task<long> GetProductsCountAsync()
        {
            using (var txn = this.StateManager.CreateTransaction())
            {
                var products = await this.StateManager.TryGetAsync<IReliableDictionary<Guid, Product>>("products");

                return await products.Value.GetCountAsync(txn);
            }
        }

        public async Task<List<Guid>> GetProductsInCategoryAsync(Guid categoryId)
        {
            using (var txn = this.StateManager.CreateTransaction())
            {
                var result = new List<Guid>();

                var productCategories = await this.StateManager.TryGetAsync<IReliableDictionary<Guid, object>>($"products-category-{categoryId:N}");
                if (!productCategories.HasValue)
                    throw new KeyNotFoundException($"Category {categoryId} was not found");

                var enumerable = await productCategories.Value.CreateEnumerableAsync(txn, EnumerationMode.Unordered);
                var enumerator = enumerable.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(CancellationToken.None))
                    result.Add(enumerator.Current.Key);

                return result;
            }
        }

        public async Task AddProductToCategoryAsync(Guid categoryId, Guid productId)
        {
            using (var txn = this.StateManager.CreateTransaction())
            {
                var productCategories = await this.StateManager.TryGetAsync<IReliableDictionary<Guid, object>>($"products-category-{categoryId:N}");
                if (!productCategories.HasValue)
                    throw new KeyNotFoundException($"Category {categoryId} was not found");

                await productCategories.Value.AddAsync(txn, productId, null);
                await txn.CommitAsync();
            }
        }

        public async Task RemoveProductFromCategoryAsync(Guid categoryId, Guid productId)
        {
            using (var txn = this.StateManager.CreateTransaction())
            {
                var productCategories = await this.StateManager.TryGetAsync<IReliableDictionary<Guid, object>>($"products-category-{categoryId:N}");
                if (!productCategories.HasValue)
                    throw new KeyNotFoundException($"Category {categoryId} was not found");

                var result = await productCategories.Value.TryRemoveAsync(txn, productId);
                if (!result.HasValue)
                    throw new KeyNotFoundException($"Product {productId} was not found");

                await txn.CommitAsync();
            }
        }
    }
}
