using AutoMapper;
using StockApp.Application.DTOs;
using StockApp.Application.Interfaces;
using StockApp.Domain.Entities;
using StockApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockApp.Application.Services
{
    public class ProductService : IProductService
    {
        private IProductRepository _productRepository;
        private IMapper _mapper;
        private IAuditService _auditService;

        public ProductService(IProductRepository productRepository, IMapper mapper, IAuditService auditService)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _auditService = auditService;
        }

        public async Task<Product> Add(ProductDTO productDto)
        {
            var productEntity = _mapper.Map<Product>(productDto);
            await _productRepository.Create(productEntity);
            return productEntity;
        }

        public async Task<IEnumerable<ProductDTO>> GetProducts()
        {
            var productsEntity = await _productRepository.GetProducts();
            return _mapper.Map<IEnumerable<ProductDTO>>(productsEntity);
        }

        public async Task<ProductDTO> GetProductById(int? id)
        {
            var productEntity = await _productRepository.GetById(id);
            return _mapper.Map<ProductDTO>(productEntity);
        }

        public async Task Remove(int? id)
        {
            var productEntity = _productRepository.GetById(id).Result;
            await _productRepository.Remove(productEntity);
        }

        public async Task Update(ProductDTO productDto)
        {
            var existingProduct = await _productRepository.GetById(productDto.Id);
            if (existingProduct == null) throw new InvalidOperationException("Product not found.");

            if (existingProduct.Stock != productDto.Stock)
            {
                await _auditService.AuditStockChange(existingProduct.Id, existingProduct.Stock, productDto.Stock);
            }

            // Atualize as propriedades manualmente
            _mapper.Map(productDto, existingProduct);

            await _productRepository.Update(existingProduct);
        }

        public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold)
        {
            if (threshold <= 0)
            {
                throw new ArgumentException("Threshold must be greater than zero.", nameof(threshold));
            }

            return await _productRepository.GetLowStockAsync(threshold);
        }

        public async Task BulkUpdateAsync(List<Product> products)
        {
             await _productRepository.BulkUpdateAsync(products);
        }

        public async Task<IEnumerable<Product>> GetProductByIds(List<int> productIds)
        {
           return await _productRepository.GetProductByIds(productIds);
        }
    }
}
