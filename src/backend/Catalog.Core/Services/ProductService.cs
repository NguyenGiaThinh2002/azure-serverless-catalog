using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Core.Services
{
    public class ProductService : IProductService
    {

        private readonly IBaseRepository<Product> _repo;
        public ProductService(IBaseRepository<Product> repo) => _repo = repo;

        public Task<IEnumerable<Product>> GetAllProductsAsync() => _repo.GetAllAsync();

        public Task<Product> GetProductByIdAsync(string id) => _repo.GetByIdAsync(id);

        public Task<Product> CreateProductAsync(Product product) => _repo.AddAsync(product);

    }
}
