using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Application.Services;
using EcommerceCore.Domain.Models;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace EcommerceCore.Application.UnitTests;

public class ProductServiceTests
{
    private readonly Mock<IApplicationDbContext> _mockDbContext;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockDbContext = new Mock<IApplicationDbContext>();
        _productService = new ProductService(_mockDbContext.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new("Product 1",
                "Desc 1",
                100,
                10,
                "url1",
                "Cat1"),
            new("Product 2",
                "Desc 2",
                200,
                20,
                "url2",
                "Cat2")
        };
        _mockDbContext.Setup(c => c.Products).ReturnsDbSet(products);

        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Product 1");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product("Product 1",
            "Desc 1",
            100,
            10,
            "url1",
            "Cat1");
        // Reflection to set Id since it's private set
        typeof(Product).GetProperty("Id")!.SetValue(product,
            1);

        _mockDbContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _productService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Product 1");
    }

    [Fact]
    public async Task CreateAsync_ShouldAddProductAndSaveChanges()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "New Product",
            Description = "New Desc",
            Price = 150,
            Stock = 50,
            ImageUrl = "new-url",
            Category = "New Cat"
        };
        _mockDbContext.Setup(c => c.Products).ReturnsDbSet(new List<Product>());

        // Act
        var result = await _productService.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Product");
        _mockDbContext.Verify(c => c.Products.Add(It.IsAny<Product>()),
            Times.Once);
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_ShouldUpdateProduct()
    {
        // Arrange
        var product = new Product("Old Name",
            "Old Desc",
            100,
            10,
            "url",
            "Cat");
        _mockDbContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);

        var dto = new UpdateProductDto
        {
            Name = "Updated Name",
            Description = "Updated Desc",
            Price = 200,
            Stock = 15, // +5 stock
            ImageUrl = "updated-url",
            Category = "Updated Cat"
        };

        // Act
        await _productService.UpdateAsync(1,
            dto);

        // Assert
        product.Name.Should().Be("Updated Name");
        product.Stock.Should().Be(15);
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldRemoveProduct()
    {
        // Arrange
        var product = new Product("To Delete",
            "Desc",
            100,
            10,
            "url",
            "Cat");
        typeof(Product).GetProperty("Id")!.SetValue(product,
            1); // Set Id to 1

        _mockDbContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);

        // Act
        await _productService.DeleteAsync(1);

        // Assert
        _mockDbContext.Verify(c => c.Products.Remove(product),
            Times.Once);
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
