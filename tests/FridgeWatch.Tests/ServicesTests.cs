using Xunit;
using FluentAssertions;
using Moq;
using AutoMapper;
using FridgeWatch.Application.Services;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Mappings;

namespace FridgeWatch.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _jwtServiceMock = new Mock<IJwtService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = config.CreateMapper();

        _authService = new AuthService(_unitOfWorkMock.Object, _mapper, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserExists_ReturnsUserDto()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.GetCurrentUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var userId = 999;
        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _authService.GetCurrentUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("用户不存在");
    }
}

public class HouseholdServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly HouseholdService _householdService;

    public HouseholdServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = config.CreateMapper();

        _householdService = new HouseholdService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task GetByIdAsync_WhenHouseholdExists_ReturnsHouseholdDto()
    {
        // Arrange
        var householdId = 1;
        var household = new Household
        {
            Id = householdId,
            Name = "测试家庭",
            InviteCode = "TEST1234",
            CreatedBy = 1,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWorkMock.Setup(u => u.Households.GetByIdAsync(householdId))
            .ReturnsAsync(household);

        // Act
        var result = await _householdService.GetByIdAsync(householdId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(householdId);
        result.Name.Should().Be("测试家庭");
        result.InviteCode.Should().Be("TEST1234");
    }

    [Fact]
    public async Task GetByIdAsync_WhenHouseholdNotFound_ThrowsException()
    {
        // Arrange
        var householdId = 999;
        _unitOfWorkMock.Setup(u => u.Households.GetByIdAsync(householdId))
            .ReturnsAsync((Household?)null);

        // Act
        Func<Task> act = async () => await _householdService.GetByIdAsync(householdId);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("家庭不存在");
    }
}

public class FoodItemServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly FoodItemService _foodItemService;

    public FoodItemServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = config.CreateMapper();

        _foodItemService = new FoodItemService(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFoodItemExists_ReturnsFoodItemDto()
    {
        // Arrange
        var foodItemId = 1;
        var foodItem = new FoodItem
        {
            Id = foodItemId,
            HouseholdId = 1,
            Name = "测试食材",
            Category = "测试分类",
            StorageLocation = StorageLocation.Fridge,
            PurchaseDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            Quantity = 10,
            Unit = "个",
            Status = FoodStatus.Fresh,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWorkMock.Setup(u => u.FoodItems.GetByIdAsync(foodItemId))
            .ReturnsAsync(foodItem);

        // Act
        var result = await _foodItemService.GetByIdAsync(foodItemId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(foodItemId);
        result.Name.Should().Be("测试食材");
        result.Status.Should().Be(FoodStatus.Fresh);
    }
}
