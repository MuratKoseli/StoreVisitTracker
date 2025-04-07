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
    public class StoresController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;
        public StoresController(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Redis ile cach işmei yaparak performans artışı sağladık.
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetStores([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            string cacheKey = $"stores_page_{page}_size_{pageSize}";
            var cachedStores = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedStores))
            {
                var storesFromCache = JsonSerializer.Deserialize<List<Store>>(cachedStores);
                return Ok(storesFromCache);
            }

            var stores = await _context.Stores
                                        .Skip((page - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            var serializedStores = JsonSerializer.Serialize(stores);
            await _cache.SetStringAsync(cacheKey, serializedStores, options);

            return Ok(stores);
        }


       
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateStore([FromBody] Store store)
        {
            _context.Stores.Add(store);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetStores), new { id = store.Id }, store);

        }


        
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStore(int id, [FromBody] Store updatedStore)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null)
            {
                return NotFound();
            }
            store.Name = updatedStore.Name;
            store.Location = updatedStore.Location;

            await _context.SaveChangesAsync();
            return Ok(store);
        }



     
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null)
            {
                return NotFound();
            }

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}