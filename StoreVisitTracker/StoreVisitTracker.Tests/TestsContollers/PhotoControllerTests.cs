using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using StoreVisitTracker.Api.Controllers;
using StoreVisitTracker.Api.Models;
using StoreVisitTracker.Domain.Entities;
using StoreVisitTracker.Infrastructure.Db;
using Xunit;

public class PhotoControllerTests
{
    [Fact]
    public async Task UploadPhoto_ShouldReturnOk_WhenVisitBelongsToUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "PhotoUploadTestDb")
            .Options;

        var context = new AppDbContext(options);

        var userId = 1;

        var visit = new Visit { Id = 1, UserId = userId };
        context.Visits.Add(visit);
        await context.SaveChangesAsync();

        var controller = new PhotoController(context);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var request = new PhotoCreateRequestDto
        {
            VisitId = 1,
            ProductId = 123,
            Base64Image = "R0lGODlhAQABAIAAAAUEBA=="
        };

        // Act
        var result = await controller.UploadPhoto(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var photo = Assert.IsType<Photo>(okResult.Value);
        Assert.Equal(request.VisitId, photo.VisitId);
        Assert.Equal(request.ProductId, photo.ProductId);
    }
}
