using Microsoft.AspNetCore.Mvc;
using StockApp.Application.DTOs;
using StockApp.Application.Interfaces;
using StockApp.Application.Services;
using StockApp.Domain.Entities;

namespace HelpStockApp.API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // Endpoint para listar todos os produtos
        [HttpGet(Name = "GetProducts")]
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
        [HttpGet("{id:int}", Name = "GetProduct")]  // Corrigido a rota
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
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> Create([FromBody] ProductDTO productDto)  // Usando ProductDTO
        {
            if (productDto == null)
            {
                return BadRequest("Product cannot be null");
            }

            // Chama o serviço para adicionar o produto
            await _productService.Add(productDto);

            // Retorna o código 201 (Created) e a URL do novo recurso
            return CreatedAtAction(nameof(Get), new { id = productDto.Id }, productDto);
        }

    }
}