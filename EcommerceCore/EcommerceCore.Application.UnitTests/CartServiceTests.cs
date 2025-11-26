using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Application.Services;
using EcommerceCore.Domain.Models;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace EcommerceCore.Application.UnitTests;

public class CartServiceTests
{
    private readonly Mock<IApplicationDbContext> _mockDbContext;
    private readonly CartService _cartService;

    public CartServiceTests()
    {
        _mockDbContext = new Mock<IApplicationDbContext>();
        _cartService = new CartService(_mockDbContext.Object);
    }

    [Fact]
    public async Task GetCartAsync_WhenCartExists_ShouldReturnCartDto()
    {
        // Arrange
        var userId = 1;
        var product = new Product("Prod 1",
            "Desc",
            100,
            10,
            "url",
            "Cat");
        var cart = new Cart(userId);

        // Setup cart with item
        var cartItem = new CartItem(1,
            1);
        cartItem.GetType().GetProperty("Product")!.SetValue(cartItem,
            product);

        cart.Items.Add(cartItem);

        _mockDbContext.Setup(c => c.Carts).ReturnsDbSet(new List<Cart> { cart });

        // Act
        var result = await _cartService.GetCartAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().ProductName.Should().Be("Prod 1");
    }

    [Fact]
    public async Task AddItemAsync_ShouldAddItemAndReturnUpdatedCart()
    {
        // Arrange
        var userId = 1;
        var dto = new AddToCartDto { ProductId = 1, Quantity = 1 };
        var product = new Product("Prod 1",
            "Desc",
            100,
            10,
            "url",
            "Cat");

        // Initial empty cart
        var cart = new Cart(userId);
        var carts = new List<Cart> { cart };

        // Setup mock to return the cart. 
        // Note: In a real integration test, SaveChanges would persist to DB and next query would get it.
        // With Moq, we need to simulate the state change or setup the second call to return the updated cart.
        // Since we are refetching, we need to ensure the second call to GetCartEntityAsync returns the cart WITH the new item and product loaded.

        // We can use a callback or simply setup the mock to always return the 'cart' object, 
        // and we manually add the item to the 'cart' object's internal list so that when it's queried again, it has it.
        // But Cart.AddItem does that. The missing piece is the 'Product' navigation property on the new item.

        _mockDbContext.Setup(c => c.Carts).ReturnsDbSet(carts);

        // We need to intercept the SaveChanges or AddItem to inject the Product into the new CartItem 
        // so that the subsequent MapToDto doesn't fail.
        // Or we can just ensure that 'cart' has the item with Product set.
        // Since we can't easily intercept, we might have to rely on the fact that we are testing the logic flow.

        // For this unit test, let's assume the refetch works. 
        // To make it pass without complex mocking of EF internals:
        // We can pre-populate the cart with the item but say it was added by the method? No.

        // Let's try to simulate the "Refetch" by having the mock return a DIFFERENT cart object on the second call?
        // Or just ensure the 'cart' object is updated.

        // Problem: cart.AddItem(id, qty) creates CartItem. CartItem.Product is null.
        // Service calls GetCartEntityAsync -> returns 'cart'. 'cart' has item with null Product.
        // MapToDto accesses item.Product.Name -> NullReferenceException.

        // Fix: In the test, we can manually set the Product on the item AFTER AddItem is called but BEFORE MapToDto?
        // We can't interrupt the method.

        // Alternative: Mock GetCartEntityAsync? We can't, it's a private method in the service.
        // We mock the DbContext.

        // If we want to test this strictly, we need a way to populate the Product.
        // Maybe we can Mock the DbSet to return a specific list that we modify?
        // The issue is the 'new CartItem' inside the service doesn't know about the Product entity.

        // This highlights a difficulty in unit testing EF Core logic that relies on navigation fixup.
        // A common approach is to use an In-Memory Database for these tests instead of Moq.
        // But the user has existing tests using Moq.

        // Let's stick to Moq but maybe relax the test expectation or use a trick.
        // Trick: We can setup the Mock to return a list. When SaveChanges is called, we can use a Callback to 
        // find the new item in the cart and set its Product property!

        _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback((CancellationToken ct) =>
            {
                // Find the item in the cart and set the product
                var item = cart.Items.FirstOrDefault(i => i.ProductId == 1);
                if (item != null)
                {
                    item.GetType().GetProperty("Product")!.SetValue(item,
                        product);
                }
            })
            .Returns(Task.FromResult(1));

        // Act
        var result = await _cartService.AddItemAsync(userId,
            dto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().ProductName.Should().Be("Prod 1");
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldRemoveItem()
    {
        // Arrange
        var userId = 1;
        var cart = new Cart(userId);
        var product = new Product("Prod 1",
            "Desc",
            100,
            10,
            "url",
            "Cat");

        // Add item
        var cartItem = new CartItem(1,
            1);
        cartItem.GetType().GetProperty("Product")!.SetValue(cartItem,
            product);
        cart.Items.Add(cartItem);

        _mockDbContext.Setup(c => c.Carts).ReturnsDbSet(new List<Cart> { cart });

        // Act
        var result = await _cartService.RemoveItemAsync(userId,
            1);

        // Assert
        result.Items.Should().BeEmpty();
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
