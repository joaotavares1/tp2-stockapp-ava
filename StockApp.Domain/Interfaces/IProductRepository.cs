﻿using StockApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockApp.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetProducts();

        Task<Product> GetById(int? id);
        Task<Product> Create(Product product);
        Task<Product> Update(Product product);
        Task<Product> Remove(Product product);
        Task BulkUpdateAsync(List<Product> products);
        Task<IEnumerable<Product>> GetLowStockAsync(int threshold);
        Task<IEnumerable<Product>> GetProductByIds(List<int> productIds);
        IQueryable<Product> Products { get; }

    }
}
