using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StoreVisitTracker.Domain.Entities;
using StoreVisitTracker.Infrastructure.Db;

namespace StoreVisitTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;
        public ProductsController(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Redis ile cach işlemi yaparak performans artışı sağladık. Bu şekilde her sayfa için farklı cache key kullanılıyor.
        [HttpGet]
        [Authorize]
        // [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            string cacheKey = $"products_page_{page}_size_{pageSize}";
            var cachedProducts = await _cache.GetStringAsync(cacheKey);


            if (!string.IsNullOrEmpty(cachedProducts))
            {
                var productsFromCache = JsonSerializer.Deserialize<List<Product>>(cachedProducts);
                return Ok(productsFromCache);
            }

            var products = await _context.Products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            var serializedProducts = JsonSerializer.Serialize(products);
            await _cache.SetStringAsync(cacheKey, serializedProducts, options);

            return Ok(products);
        }
    }
}