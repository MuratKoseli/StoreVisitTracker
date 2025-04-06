using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using StoreVisitTracker.Api.Controllers;
using StoreVisitTracker.Domain.Entities;
using StoreVisitTracker.Infrastructure.Db;
using Xunit;

namespace StoreVisitTracker.Api.Tests.Controllers
{
    // Her test için ayrı bir veritabanı oluşturarak izole test ortamı sağlıyor
    public class ProductsControllerTests : IDisposable
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;
        private readonly Mock<IDistributedCache> _mockCache;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            // Her test için benzersiz bir InMemory database oluşturuyoruz
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Veritabanına örnek ürünler ekliyoruz
            using (var context = new AppDbContext(_dbContextOptions))
            {
                context.Products.AddRange(
                    new Product { Id = 1, Name = "Product 1" },
                    new Product { Id = 2, Name = "Product 2" },
                    new Product { Id = 3, Name = "Product 3" },
                    new Product { Id = 4, Name = "Product 4" },
                    new Product { Id = 5, Name = "Product 5" }
                );
                context.SaveChanges();
            }

            // Cache mock’unu oluşturuyor
            _mockCache = new Mock<IDistributedCache>();

            // Controller örneğini oluşturuyor
            var contextForController = new AppDbContext(_dbContextOptions);
            _controller = new ProductsController(contextForController, _mockCache.Object);
        }

        // Test sonunda veritabanını temizliyor
        public void Dispose()
        {
            using (var context = new AppDbContext(_dbContextOptions))
            {
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task GetAll_ReturnsCachedData_WhenCacheExists()
        {
            // Önbellekte veri varsa, doğrudan oradan çekildiğini test ediyoruz
            var page = 1;
            var pageSize = 10;
            var cacheKey = $"products_page_{page}_size_{pageSize}";

            var testProducts = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1" },
                new Product { Id = 2, Name = "Product 2" }
            };

            var serializedProducts = JsonSerializer.Serialize(testProducts);
            var cachedData = Encoding.UTF8.GetBytes(serializedProducts);

            // Cache’den verinin geldiğini simüle ediyor
            _mockCache.Setup(x => x.GetAsync(cacheKey, default))
                .ReturnsAsync(cachedData);

            // Controller üzerinden GetAll çağrısını yapıyor
            var result = await _controller.GetAll(page, pageSize);

            // Sonuçların doğru türde ve sayıda geldiğini kontrol ediyor
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProducts = Assert.IsType<List<Product>>(okResult.Value);
            Assert.Equal(2, returnedProducts.Count);

            // Cache'in gerçekten çağrıldığını kontrol ediyor
            _mockCache.Verify(x => x.GetAsync(cacheKey, default), Times.Once);
        }

        [Fact]
        public async Task GetAll_ReturnsDatabaseData_WhenCacheDoesNotExist()
        {
            // Önbellekte veri yoksa veritabanından çekilip cache'e yazıldığını test ediyoruz
            var page = 1;
            var pageSize = 10;
            var cacheKey = $"products_page_{page}_size_{pageSize}";

            _mockCache.Setup(x => x.GetAsync(cacheKey, default)).ReturnsAsync((byte[])null!);

            _mockCache.Setup(x => x.SetAsync(
                    cacheKey,
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    default))
                .Returns(Task.CompletedTask);

            var result = await _controller.GetAll(page, pageSize);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProducts = Assert.IsType<List<Product>>(okResult.Value);
            Assert.Equal(5, returnedProducts.Count); // 5 ürün vardı, hepsi dönmeli

            // Cache get ve set işlemlerinin çağrıldığını doğruluyor
            _mockCache.Verify(x => x.GetAsync(cacheKey, default), Times.Once);
            _mockCache.Verify(x => x.SetAsync(
                cacheKey,
                It.IsAny<byte[]>(),
                It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(5)),
                default), Times.Once);
        }

        [Fact]
        public async Task GetAll_ReturnsPaginatedResults()
        {
            // Sayfalama kontrolü yapıyoruz: 2. sayfa ve 2 ürünlük sayfa boyutuyla
            var page = 2;
            var pageSize = 2;
            var cacheKey = $"products_page_{page}_size_{pageSize}";

            _mockCache.Setup(x => x.GetAsync(cacheKey, default)).ReturnsAsync((byte[])null!);

            var result = await _controller.GetAll(page, pageSize);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProducts = Assert.IsType<List<Product>>(okResult.Value);

            Assert.Equal(2, returnedProducts.Count);
            Assert.Equal(3, returnedProducts[0].Id); // 2. sayfa 1. ürün
            Assert.Equal(4, returnedProducts[1].Id); // 2. sayfa 2. ürün
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList_WhenNoProductsExist()
        {
            // Ürün hiç yoksa boş liste döndüğünü test ediyoruz
            var page = 1;
            var pageSize = 10;
            var cacheKey = $"products_page_{page}_size_{pageSize}";

            // Boş bir veritabanı oluşturuyor
            var emptyOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var emptyContext = new AppDbContext(emptyOptions);
            var controller = new ProductsController(emptyContext, _mockCache.Object);

            _mockCache.Setup(x => x.GetAsync(cacheKey, default)).ReturnsAsync((byte[])null!);

            var result = await controller.GetAll(page, pageSize);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProducts = Assert.IsType<List<Product>>(okResult.Value);
            Assert.Empty(returnedProducts); // Ürün yoksa boş liste dönmeli
        }
    }
}
