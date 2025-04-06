using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using StoreVisitTracker.Api.Controllers;
using StoreVisitTracker.Api.Models;
using StoreVisitTracker.Domain.Entities;
using StoreVisitTracker.Infrastructure.Db;
using Xunit;

namespace StoreVisitTracker.Tests.Controllers
{
    public class VisitControllerTests
    {
        private VisitController GetControllerWithContext(AppDbContext context, int userId)
        {
            var controller = new VisitController(context);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task CreateVisit_ShouldCreateNewVisit_WhenValidRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);
            context.Users.Add(new User { Id = 99, Username = "testuser", Role = UserRole.Standard });
            context.Stores.Add(new Store { Id = 1, Name = "Test Store" });
            context.SaveChanges();

            var controller = GetControllerWithContext(context, 99);

            var request = new VisitCreateRequestDto
            {
                StoreId = 1
            };

            // Act
            var result = await controller.CreateVisit(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var visit = Assert.IsType<Visit>(okResult.Value);
            Assert.Equal(99, visit.UserId);
            Assert.Equal(1, visit.StoreId);
            Assert.Equal(VisitStatus.InProgress, visit.Status);
        }

        [Fact]
        public async Task CompleteVisit_ShouldUpdateVisitStatusToCompleted()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(new User { Id = 1, Username = "user1", Role = UserRole.Standard });
                context.Stores.Add(new Store { Id = 1, Name = "Test Store", Location = "Test City" });
                context.Visits.Add(new Visit
                {
                    Id = 1,
                    UserId = 1,
                    StoreId = 1,
                    VisitDate = DateTime.UtcNow,
                    Status = VisitStatus.InProgress
                });
                context.SaveChanges();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Name, "user1")
    };

            var identity = new ClaimsIdentity(claims, "Test");
            var user = new ClaimsPrincipal(identity);

            using (var context = new AppDbContext(options))
            {
                var controller = new VisitController(context)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = user }
                    }
                };

                // Act
                var result = await controller.CompleteVisit(1);

                // Assert
                var updatedVisit = await context.Visits.FindAsync(1);
                Assert.Equal(VisitStatus.Completed, updatedVisit!.Status);
            }
        }

        [Fact]
        public async Task GetAllVisits_ShouldReturnOnlyUserVisits_ForStandardUser()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(new User { Id = 1, Username = "user1", Role = UserRole.Standard });
                context.Users.Add(new User { Id = 2, Username = "user2", Role = UserRole.Standard });

                context.Stores.Add(new Store { Id = 1, Name = "Store 1", Location = "City A" });

                context.Visits.AddRange(
                    new Visit { Id = 1, UserId = 1, StoreId = 1, VisitDate = DateTime.UtcNow, Status = VisitStatus.InProgress },
                    new Visit { Id = 2, UserId = 2, StoreId = 1, VisitDate = DateTime.UtcNow, Status = VisitStatus.Completed }
                );

                context.SaveChanges();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Name, "user1")
    };

            var identity = new ClaimsIdentity(claims, "Test");
            var user = new ClaimsPrincipal(identity);

            using (var context = new AppDbContext(options))
            {
                var controller = new VisitController(context)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = user }
                    }
                };

                // Act
                var result = await controller.GetAllVisits();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var response = okResult.Value!;

                // Convert anonymous object using reflection
                var visitsProp = response.GetType().GetProperty("Visits");
                var visits = visitsProp?.GetValue(response) as IEnumerable<object>;

                Assert.NotNull(visits);
                Assert.Single(visits); // Sadece user1'e ait 1 ziyaret dönmeli
            }
        }

        [Fact]
        public async Task GetAllVisits_ShouldReturnAllVisits_ForAdminUser()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(new User { Id = 1, Username = "admin1", Role = UserRole.Admin });
                context.Users.Add(new User { Id = 2, Username = "user1", Role = UserRole.Standard });

                context.Stores.Add(new Store { Id = 1, Name = "Store 1", Location = "City A" });

                context.Visits.AddRange(
                    new Visit { Id = 1, UserId = 1, StoreId = 1, VisitDate = DateTime.UtcNow, Status = VisitStatus.InProgress },
                    new Visit { Id = 2, UserId = 2, StoreId = 1, VisitDate = DateTime.UtcNow, Status = VisitStatus.Completed }
                );

                context.SaveChanges();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Name, "admin1")
    };

            var identity = new ClaimsIdentity(claims, "Test");
            var user = new ClaimsPrincipal(identity);

            using (var context = new AppDbContext(options))
            {
                var controller = new VisitController(context)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = user }
                    }
                };

                // Act
                var result = await controller.GetAllVisits();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);

                var json = JsonSerializer.Serialize(okResult.Value);
                using var doc = JsonDocument.Parse(json);
                var visits = doc.RootElement.GetProperty("Visits").EnumerateArray();

                Assert.Equal(2, visits.Count()); // Admin tüm ziyaretleri görmeli
            }
        }

        [Fact]
        public async Task CreateVisit_ShouldAddNewVisit_ForAuthenticatedUser()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(new User { Id = 1, Username = "user1", Role = UserRole.Standard });
                context.Stores.Add(new Store { Id = 1, Name = "Store 1", Location = "City A" });
                await context.SaveChangesAsync();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Name, "user1")
    };
            var identity = new ClaimsIdentity(claims, "Test");
            var user = new ClaimsPrincipal(identity);

            var request = new VisitCreateRequestDto
            {
                StoreId = 1
            };

            using (var context = new AppDbContext(options))
            {
                var controller = new VisitController(context)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = user }
                    }
                };

                // Act
                var result = await controller.CreateVisit(request);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var visit = Assert.IsType<Visit>(okResult.Value);
                Assert.Equal(1, visit.UserId);
                Assert.Equal(1, visit.StoreId);
                Assert.Equal(VisitStatus.InProgress, visit.Status);
            }
        }

        // Bir kullanıcının kendi ziyaretini "Completed" durumuna geçirdiğini doğrular. Bu nedenle bu testi yapıyoruz.
        [Fact]
        public async Task CompleteVisit_ShouldUpdateStatus_WhenVisitExistsAndBelongsToUser()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(new User { Id = 1, Username = "user1", Role = UserRole.Standard });
                context.Stores.Add(new Store { Id = 1, Name = "Store 1", Location = "City A" });
                context.Visits.Add(new Visit
                {
                    Id = 1,
                    UserId = 1,
                    StoreId = 1,
                    VisitDate = DateTime.UtcNow,
                    Status = VisitStatus.InProgress
                });
                await context.SaveChangesAsync();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim(ClaimTypes.Name, "user1")
    };
            var identity = new ClaimsIdentity(claims, "Test");
            var user = new ClaimsPrincipal(identity);

            using (var context = new AppDbContext(options))
            {
                var controller = new VisitController(context)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = user }
                    }
                };

                // Act
                var result = await controller.CompleteVisit(1);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal("Ziyaret başarıyla tamamlandı.", okResult.Value);

                var visit = await context.Visits.FindAsync(1);
                Assert.Equal(VisitStatus.Completed, visit!.Status);
            }
        }

    }
}