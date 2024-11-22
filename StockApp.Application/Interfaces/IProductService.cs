using StockApp.Application.DTOs;
using StockApp.Domain.Entities;

namespace StockApp.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetProducts();
        Task<ProductDTO> GetProductById(int? id);
        Task<Product> Add(ProductDTO productDto);
        Task Update(ProductDTO product);
        Task Remove(int? id);
        Task<IEnumerable<Product>> GetLowStockAsync(int threshold);
        Task BulkUpdateAsync(List<Product> products);

    }
}
