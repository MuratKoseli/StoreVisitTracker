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
    public class StoresControllerTests : IDisposable
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;
        private readonly Mock<IDistributedCache> _mockCache;
        private readonly StoresController _controller;

        public StoresControllerTests()
        {
            // InMemory veritabanı seçenekleri tanımlanıyor (benzersiz isimle her test izole çalışır)
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Test verileri veritabanına ekleniyor
            using (var context = new AppDbContext(_dbContextOptions))
            {
                context.Stores.AddRange(
                    new Store { Id = 1, Name = "Store 1", Location = "Location 1" },
                    new Store { Id = 2, Name = "Store 2", Location = "Location 2" },
                    new Store { Id = 3, Name = "Store 3", Location = "Location 3" },
                    new Store { Id = 4, Name = "Store 4", Location = "Location 4" },
                    new Store { Id = 5, Name = "Store 5", Location = "Location 5" }
                );
                context.SaveChanges();
            }

            // Mock cache oluşturuluyor
            _mockCache = new Mock<IDistributedCache>();

            // Controller örneği oluşturuluyor (mock cache ve test context ile)
            var contextForController = new AppDbContext(_dbContextOptions);
            _controller = new StoresController(contextForController, _mockCache.Object);
        }

        // Her test sonrası veritabanı silinir
        public void Dispose()
        {
            using (var context = new AppDbContext(_dbContextOptions))
            {
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task GetStores_ReturnsCachedData_WhenCacheExists()
        {
            // Arrange
            var page = 1;
            var pageSize = 10;
            var cacheKey = $"stores_page_{page}_size_{pageSize}";

            // Önbellekten dönmesi beklenen test verileri hazırlanıyor
            var testStores = new List<Store>
            {
                new Store { Id = 1, Name = "Store 1", Location = "Location 1" },
                new Store { Id = 2, Name = "Store 2", Location = "Location 2" }
            };

            // Serialize edilip byte dizisine çevrilerek mock cache'e veriliyor
            var serializedStores = JsonSerializer.Serialize(testStores);
            var cachedData = Encoding.UTF8.GetBytes(serializedStores);

            // Mock cache'den veri döndürülüyor
            _mockCache.Setup(x => x.GetAsync(cacheKey, default))
                .ReturnsAsync(cachedData);

            // Act
            var result = await _controller.GetStores(page, pageSize);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedStores = Assert.IsType<List<Store>>(okResult.Value);
            Assert.Equal(2, returnedStores.Count); // Cache'teki veri sayısıyla eşleşmeli

            _mockCache.Verify(x => x.GetAsync(cacheKey, default), Times.Once);
        }

        [Fact]
        public async Task GetStores_ReturnsDatabaseData_WhenCacheDoesNotExist()
        {
            // Arrange
            var page = 1;
            var pageSize = 10;
            var cacheKey = $"stores_page_{page}_size_{pageSize}";

            // Cache boş (null) olarak ayarlanıyor
            _mockCache.Setup(x => x.GetAsync(cacheKey, default)).ReturnsAsync((byte[])null!);

            // Cache'e veri yazımı bekleniyor
            _mockCache.Setup(x => x.SetAsync(
                    cacheKey,
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    default))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.GetStores(page, pageSize);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedStores = Assert.IsType<List<Store>>(okResult.Value);
            Assert.Equal(5, returnedStores.Count); // Tüm veritabanı kayıtları gelmeli

            _mockCache.Verify(x => x.GetAsync(cacheKey, default), Times.Once);
            _mockCache.Verify(x => x.SetAsync(
                cacheKey,
                It.IsAny<byte[]>(),
                It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(5)),
                default), Times.Once); // Cache'e yazıldığından emin olunuyor
        }

        [Fact]
        public async Task GetStores_ReturnsPaginatedResults()
        {
            // Arrange
            var page = 2;
            var pageSize = 2;
            var cacheKey = $"stores_page_{page}_size_{pageSize}";

            _mockCache.Setup(x => x.GetAsync(cacheKey, default)).ReturnsAsync((byte[])null!);

            // Act
            var result = await _controller.GetStores(page, pageSize);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedStores = Assert.IsType<List<Store>>(okResult.Value);
            Assert.Equal(2, returnedStores.Count);
            Assert.Equal(3, returnedStores[0].Id); // 2. sayfanın ilk kaydı
            Assert.Equal(4, returnedStores[1].Id); // 2. sayfanın ikinci kaydı
        }

        [Fact]
        public async Task CreateStore_AddsNewStore_WhenUserIsAdmin()
        {
            // Arrange
            var newStore = new Store { Name = "New Store", Location = "New Location" };

            // Act
            var result = await _controller.CreateStore(newStore);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedStore = Assert.IsType<Store>(createdAtActionResult.Value);
            Assert.Equal("New Store", returnedStore.Name);
            Assert.Equal("New Location", returnedStore.Location);

            // DB'de gerçekten kaydedildi mi kontrolü
            using (var context = new AppDbContext(_dbContextOptions))
            {
                var storeInDb = await context.Stores.FindAsync(returnedStore.Id);
                Assert.NotNull(storeInDb);
            }
        }

        [Fact]
        public async Task UpdateStore_UpdatesExistingStore_WhenUserIsAdmin()
        {
            // Arrange
            var updatedStore = new Store { Name = "Updated Store", Location = "Updated Location" };
            var storeId = 1;

            // Act
            var result = await _controller.UpdateStore(storeId, updatedStore);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedStore = Assert.IsType<Store>(okResult.Value);
            Assert.Equal("Updated Store", returnedStore.Name);
            Assert.Equal("Updated Location", returnedStore.Location);

            // DB'de gerçekten güncellendi mi kontrolü
            using (var context = new AppDbContext(_dbContextOptions))
            {
                var storeInDb = await context.Stores.FindAsync(storeId);
                Assert.Equal("Updated Store", storeInDb!.Name);
                Assert.Equal("Updated Location", storeInDb.Location);
            }
        }

        [Fact]
        public async Task UpdateStore_ReturnsNotFound_WhenStoreDoesNotExist()
        {
            // Arrange
            var updatedStore = new Store { Name = "Updated Store", Location = "Updated Location" };
            var nonExistentStoreId = 999;

            // Act
            var result = await _controller.UpdateStore(nonExistentStoreId, updatedStore);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteStore_RemovesStore_WhenUserIsAdmin()
        {
            // Arrange
            var storeId = 1;

            // Act
            var result = await _controller.DeleteStore(storeId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Veritabanında silinmiş mi kontrolü
            using (var context = new AppDbContext(_dbContextOptions))
            {
                var storeInDb = await context.Stores.FindAsync(storeId);
                Assert.Null(storeInDb);
            }
        }

        [Fact]
        public async Task DeleteStore_ReturnsNotFound_WhenStoreDoesNotExist()
        {
            // Arrange
            var nonExistentStoreId = 999;

            // Act
            var result = await _controller.DeleteStore(nonExistentStoreId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
