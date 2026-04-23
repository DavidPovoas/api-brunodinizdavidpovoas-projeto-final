using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniShop.Cache;
using MiniShop.Controllers;
using MiniShop.Data;
using MiniShop.Models;
using Moq;
using Xunit;

namespace MiniShop.Tests
{
    public class ProductsControllerTests
    {
        private AppDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private ProductsController GetController(AppDbContext context)
        {
            var mockCache = new Mock<IRedisCacheService>();
            mockCache.Setup(c => c.GetAsync<List<Product>>(It.IsAny<string>()))
                     .ReturnsAsync((List<Product>?)null);
            mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<List<Product>>(), null))
                     .Returns(Task.CompletedTask);
            mockCache.Setup(c => c.RemoveAsync(It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

            return new ProductsController(context, mockCache.Object);
        }

        [Fact]
        public async Task GetProducts_ReturnsEmptyList_WhenNoProducts()
        {
            var context = GetInMemoryContext();
            var controller = GetController(context);
            var result = await controller.GetProducts();
            var okResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
            Assert.Empty(okResult.Value!);
        }

        [Fact]
        public async Task CreateProduct_ReturnsCreatedProduct()
        {
            var context = GetInMemoryContext();
            var controller = GetController(context);
            var product = new Product
            {
                Name = "iPhone 15",
                Description = "Smartphone Apple",
                Price = 999.99m,
                Stock = 10,
                SKU = "IPH15"
            };
            var result = await controller.CreateProduct(product);
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var created = Assert.IsType<Product>(createdResult.Value);
            Assert.Equal("iPhone 15", created.Name);
        }

        [Fact]
        public async Task GetProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            var context = GetInMemoryContext();
            var controller = GetController(context);
            var result = await controller.GetProduct(999);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNoContent_WhenProductExists()
        {
            var context = GetInMemoryContext();
            var product = new Product
            {
                Name = "Samsung S24",
                Description = "Smartphone Samsung",
                Price = 799.99m,
                Stock = 5,
                SKU = "SAM24"
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();
            var controller = GetController(context);
            var result = await controller.DeleteProduct(product.Id);
            Assert.IsType<NoContentResult>(result);
        }
    }
}