using StockApp.Domain.Entities;
using StockApp.Domain.Interfaces;
using StockApp.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace StockApp.Infra.Data.Repositories
{
    public class ProductRepository : IProductRepository
    {
        ApplicationDbContext _productContext;
        public ProductRepository(ApplicationDbContext context)
        {
            _productContext = context;
        }

        public async Task<Product> Create(Product product)
        {
            
            _productContext.Add(product);
            await _productContext.SaveChangesAsync();
            return product;
        }

        public async Task<Product> GetById(int? id)
        {
            return await _productContext.Products.FindAsync(id);
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await _productContext.Products.ToListAsync();
        }

        public async Task<Product> Remove(Product product)
        {
            _productContext.Remove(product);
            await _productContext.SaveChangesAsync();
            return product;
        }

        public async Task<Product> Update(Product product)
        {
            _productContext.Update(product);
            await _productContext.SaveChangesAsync();
            return product;
        }
        public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold)
        {
            return await _productContext.Products
                .Where(p => p.Stock <= threshold)
                .ToListAsync();
        }

        public async Task BulkUpdateAsync(List<Product> products)
        {
            var productIds = products.Select(p => p.Id).ToList();

            // Buscar os produtos existentes no banco
            var existingProducts = await _productContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var product in products)
            {
                // Localizar o produto existente no banco
                var existingProduct = existingProducts.FirstOrDefault(p => p.Id == product.Id);

                if (existingProduct != null)
                {
                    // Atualizar os valores do produto existente
                    _productContext.Entry(existingProduct).CurrentValues.SetValues(product);
                }
                else
                {
                    // Opcional: Adicionar o produto caso ele não exista
                    _productContext.Products.Add(product);
                }
            }

            // Salvar as mudanças no banco
            await _productContext.SaveChangesAsync();

        }

        public async Task<IEnumerable<Product>> GetProductByIds(List<int> productIds)
        {
            return await _productContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
        }
        public IQueryable<Product> Products => _productContext.Products.AsQueryable();
    }
}
