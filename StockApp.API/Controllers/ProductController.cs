using System.Security.Cryptography.X509Certificates;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockApp.Application.DTOs;
using StockApp.Application.Interfaces;
using StockApp.Domain.Entities;

namespace HelpStockApp.API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IAuditService _auditService;
        private readonly IMapper _mapper;

        public ProductController(IProductService productService, IAuditService auditService, IMapper mapper)
        {
            _productService = productService;
            _auditService = auditService;
            _mapper = mapper;
        }

        // Endpoint para listar todos os produtos
        [HttpGet("get-products")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> Get()
        {
            var products = await _productService.GetProducts();
            if (products == null || !products.Any())  // Adicionando uma verificação para uma lista vazia
            {
                return NotFound("Products not found");
            }
            return Ok(products);
        }

        // Endpoint para buscar um produto por ID
        [HttpGet("getProductById")]  // Corrigido a rota
        public async Task<ActionResult<ProductDTO>> Get(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound("Product not found");
            }
            return Ok(product);
        }

        // Endpoint para criar um novo produto
        [HttpPost("create-product")]
        public async Task<ActionResult> CreateProduct([FromBody] ProductDTO productDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.Add(productDto);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, productDto);
        }

        [HttpPut("update-product")]
        public async Task<ActionResult> UpdateProduct(int id, [FromBody] ProductDTO productDto)
        {
            if (id != productDto.Id)
            {
                return BadRequest("Product ID mismatch.");
            }

            // Antes de atualizar o produto, vamos capturar as mudanças de estoque para auditoria
            var existingProduct = await _productService.GetProductById(id);
            if (existingProduct != null && existingProduct.Stock != productDto.Stock)
            {
                // Registrar a auditoria da mudança no estoque
                await _auditService.AuditStockChange(id, existingProduct.Stock, productDto.Stock);
            }

            await _productService.Update(productDto);
            return Ok(); // Produto atualizado com sucesso
        }

        [HttpDelete("delete-product")]
        public async Task<ActionResult<ProductDTO>> Detele(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            await _productService.Remove(id);

            return Ok(product);
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<Product>>> GetLowStock([FromQuery] int threshold)
        {
            if (threshold <= 0)
            {
                return BadRequest("The threshold must be greater than zero.");
            }

            var products = await _productService.GetLowStockAsync(threshold);

            if (!products.Any())
            {
                return Ok(new { message = "No products with low stock found.", count = 0 });
            }

            return Ok(new
            {
                message = "Low stock products retrieved successfully.",
                count = products.Count(),
                data = products
            });


        }

        [HttpPut("bulkUpdate-products")]
        public async Task<IActionResult> BulkUpdate([FromBody] List<Product> products)
        {
            var productsToUpdate = new List<Product>();
            foreach (var product in products)
            {
                var existingProduct = await _productService.GetProductById(product.Id);
                if (existingProduct != null && existingProduct.Stock != product.Stock)
                {
                    // Registrar a auditoria da mudança no estoque
                    await _auditService.AuditStockChange(existingProduct.Id, existingProduct.Stock, product.Stock);
                }

                var productEntity = _mapper.Map<Product>(product);
                productsToUpdate.Add(productEntity);
            }

            await _productService.BulkUpdateAsync(productsToUpdate);
            return Ok();
        }

        [HttpPost("compare-products")]
        public async Task<ActionResult<IEnumerable<Product>>> CompareProducts([FromBody] List<int> productIds)
        {
            var products = await _productService.GetProductByIds(productIds);
            return Ok(products);
        }

        [HttpGet("dashboard-stock")]
        public async Task<ActionResult<Product>> GetDashboardStockData()
        {
            var dashboardData = new DashboardStockDTO
            {
              
                TotalProducts = await _productService.Products.CountAsync(),
                TotalStockValue = await _productService.Products.SumAsync(p => p.Stock * p.Price),
                LowStockProducts = await _productService.Products
                    .Where(p => p.Stock < 10)
                    .Select(p => new ProductStockDTO
                    {
                        ProductName = p.Name,
                        Stock = p.Stock
                    })
                    .ToListAsync()
            };

            return Ok(dashboardData);
        }

    }
}