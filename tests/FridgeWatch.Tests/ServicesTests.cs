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

public class CsvParserTests
{
    [Fact]
    public void ParseCsvRecords_SimpleRows_ReturnsAllRecords()
    {
        var lines = new List<string>
        {
            "牛奶,乳制品,冷藏,2026-06-15,2026-06-25,2,盒",
            "鸡胸肉,肉类,冷冻,2026-06-10,2026-07-10,1.5,kg",
            "大米,主食,常温,2026-01-01,2026-12-31,10,kg"
        };

        var result = FoodItemService.ParseCsvRecords(lines);

        result.Should().HaveCount(3);
        result[0][0].Should().Be("牛奶");
        result[0][6].Should().Be("盒");
        result[2][0].Should().Be("大米");
        result[2][5].Should().Be("10");
    }

    [Fact]
    public void ParseCsvRecords_WithHeader_StillParsesAllRows()
    {
        var lines = new List<string>
        {
            "名称,分类,存放位置,购买日期,保质期,数量,单位",
            "牛奶,乳制品,冷藏,2026-06-15,2026-06-25,2,盒",
            "鸡胸肉,肉类,冷冻,2026-06-10,2026-07-10,1.5,kg"
        };

        var result = FoodItemService.ParseCsvRecords(lines);

        result.Should().HaveCount(3);
        result[0][0].Should().Be("名称");
        result[1][0].Should().Be("牛奶");
        result[2][0].Should().Be("鸡胸肉");
    }

    [Fact]
    public void ParseCsvRecords_QuotedFieldWithComma_PreservesComma()
    {
        var lines = new List<string>
        {
            "\"Butter, salted\",乳制品,冷藏,2026-06-15,2026-06-25,1,盒",
            "牛奶,乳制品,冷藏,2026-06-15,2026-06-25,2,盒"
        };

        var result = FoodItemService.ParseCsvRecords(lines);

        result.Should().HaveCount(2);
        result[0].Should().HaveCount(7);
        result[0][0].Should().Be("Butter, salted");
        result[0][1].Should().Be("乳制品");
        result[0][6].Should().Be("盒");
    }

    [Fact]
    public void ParseCsvRecords_DoubleQuotesInsideQuotedField_EscapesCorrectly()
    {
        var lines = new List<string>
        {
            "\"3\"\" 牛奶\",乳制品,冷藏,2026-06-15,2026-06-25,2,盒"
        };

        var result = FoodItemService.ParseCsvRecords(lines);

        result.Should().HaveCount(1);
        result[0][0].Should().Be("3\" 牛奶");
    }

    [Fact]
    public void ParseCsvRecords_MultilineQuotedField_SpansLines()
    {
        var lines = new List<string>
        {
            "\"有机\n牛奶\",乳制品,冷藏,2026-06-15,2026-06-25,2,盒",
            "大米,主食,常温,2026-01-01,2026-12-31,10,kg"
        };

        var result = FoodItemService.ParseCsvRecords(lines);

        result.Should().HaveCount(2);
        result[0][0].Should().Be("有机\n牛奶");
        result[0][1].Should().Be("乳制品");
        result[1][0].Should().Be("大米");
    }

    [Fact]
    public void ParseCsvRecords_EmptyFields_ParsedAsEmpty()
    {
        var lines = new List<string>
        {
            "牛奶,,冷藏,,2026-06-25,2,"
        };

        var result = FoodItemService.ParseCsvRecords(lines);

        result.Should().HaveCount(1);
        result[0].Should().HaveCount(7);
        result[0][0].Should().Be("牛奶");
        result[0][1].Should().Be("");
        result[0][3].Should().Be("");
        result[0][6].Should().Be("");
    }

    [Fact]
    public void ParseCsvRecords_EmptyQuotedField_ParsedAsEmpty()
    {
        var lines = new List<string>
        {
            "牛奶,\"\",冷藏,2026-06-15,2026-06-25,2,盒"
        };

        var result = FoodItemService.ParseCsvRecords(lines);

        result.Should().HaveCount(1);
        result[0][1].Should().Be("");
    }

    [Fact]
    public void ParseCsvRecords_EmptyLines_StillProduceRecords()
    {
        var lines = new List<string>
        {
            "牛奶,乳制品,冷藏,2026-06-15,2026-06-25,2,盒",
            "",
            "大米,主食,常温,2026-01-01,2026-12-31,10,kg"
        };

        var result = FoodItemService.ParseCsvRecords(lines);

        result.Should().HaveCount(3);
        result[0][0].Should().Be("牛奶");
        result[1][0].Should().Be("");
        result[1].Should().HaveCount(1);
        result[2][0].Should().Be("大米");
    }

    [Fact]
    public void IsHeaderRow_ValidHeader_ReturnsTrue()
    {
        var row = new FoodItemImportRowDto
        {
            Name = "名称",
            Category = "分类",
            StorageLocation = "存放位置",
            PurchaseDate = "购买日期",
            ExpiryDate = "保质期",
            Quantity = "数量",
            Unit = "单位"
        };

        FoodItemService.IsHeaderRow(row).Should().BeTrue();
    }

    [Fact]
    public void IsHeaderRow_PartialHeader_ReturnsTrue()
    {
        var row = new FoodItemImportRowDto
        {
            Name = "食物名称",
            Category = "分类",
            StorageLocation = "位置",
            PurchaseDate = "进货日期",
            ExpiryDate = "到期时间",
            Quantity = "数量",
            Unit = "单位"
        };

        FoodItemService.IsHeaderRow(row).Should().BeTrue();
    }

    [Fact]
    public void IsHeaderRow_DataRow_ReturnsFalse()
    {
        var row = new FoodItemImportRowDto
        {
            Name = "牛奶",
            Category = "乳制品",
            StorageLocation = "冷藏",
            PurchaseDate = "2026-06-15",
            ExpiryDate = "2026-06-25",
            Quantity = "2",
            Unit = "盒"
        };

        FoodItemService.IsHeaderRow(row).Should().BeFalse();
    }

    [Fact]
    public void IsHeaderRow_EmptyRow_ReturnsFalse()
    {
        var row = new FoodItemImportRowDto();

        FoodItemService.IsHeaderRow(row).Should().BeFalse();
    }

    [Fact]
    public void ParseCsvRecords_NoHeader_FirstRecordIsData()
    {
        var lines = new List<string>
        {
            "牛奶,乳制品,冷藏,2026-06-15,2026-06-25,2,盒",
            "大米,主食,常温,2026-01-01,2026-12-31,10,kg",
            "鸡胸肉,肉类,冷冻,2026-06-10,2026-07-10,1.5,kg"
        };

        var result = FoodItemService.ParseCsvRecords(lines);

        result.Should().HaveCount(3);
        result[0][0].Should().Be("牛奶");
        result[1][0].Should().Be("大米");
        result[2][0].Should().Be("鸡胸肉");
    }

    [Fact]
    public void ParseCsvRecords_MixedQuotedAndUnquoted_ParsesCorrectly()
    {
        var lines = new List<string>
        {
            "名称,分类,存放位置,购买日期,保质期,数量,单位",
            "\"有机,低脂牛奶\",乳制品,冷藏,2026-06-15,2026-06-25,2,盒",
            "大米,主食,常温,2026-01-01,2026-12-31,10,kg",
            "\"特级\n鸡胸肉\",肉类,冷冻,2026-06-10,2026-07-10,1.5,kg",
            "\"3\"\" 装鸡蛋\",蛋类,冷藏,2026-06-20,2026-07-20,30,个"
        };

        var result = FoodItemService.ParseCsvRecords(lines);

        result.Should().HaveCount(5);
        result[0][0].Should().Be("名称");
        result[1][0].Should().Be("有机,低脂牛奶");
        result[2][0].Should().Be("大米");
        result[3][0].Should().Be("特级\n鸡胸肉");
        result[4][0].Should().Be("3\" 装鸡蛋");
    }
}
