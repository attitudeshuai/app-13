using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Infrastructure.Data;

namespace FridgeWatch.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedDataAsync(FridgeWatchDbContext context)
    {
        if (!await context.Users.AnyAsync())
        {
            await SeedUsersAsync(context);
        }

        if (!await context.Households.AnyAsync())
        {
            await SeedHouseholdsAsync(context);
        }

        if (!await context.HouseholdMembers.AnyAsync())
        {
            await SeedHouseholdMembersAsync(context);
        }

        if (!await context.FoodItems.AnyAsync())
        {
            await SeedFoodItemsAsync(context);
        }

        if (!await context.ExpiryAlerts.AnyAsync())
        {
            await SeedExpiryAlertsAsync(context);
        }

        if (!await context.ConsumptionRecords.AnyAsync())
        {
            await SeedConsumptionRecordsAsync(context);
        }
    }

    private static async Task SeedUsersAsync(FridgeWatchDbContext context)
    {
        var passwordHasher = new PasswordHasher<User>();

        var users = new List<User>
        {
            new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = passwordHasher.HashPassword(null!, "Admin@123"),
                Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=admin",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new User
            {
                Username = "alice",
                Email = "alice@example.com",
                PasswordHash = passwordHasher.HashPassword(null!, "Alice@123"),
                Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=alice",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new User
            {
                Username = "bob",
                Email = "bob@example.com",
                PasswordHash = passwordHasher.HashPassword(null!, "Bob@123"),
                Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=bob",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new User
            {
                Username = "charlie",
                Email = "charlie@example.com",
                PasswordHash = passwordHasher.HashPassword(null!, "Charlie@123"),
                Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=charlie",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new User
            {
                Username = "diana",
                Email = "diana@example.com",
                PasswordHash = passwordHasher.HashPassword(null!, "Diana@123"),
                Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=diana",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }

    private static async Task SeedHouseholdsAsync(FridgeWatchDbContext context)
    {
        var households = new List<Household>
        {
            new Household
            {
                Name = "温馨小窝",
                InviteCode = "WARM2024",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-28)
            },
            new Household
            {
                Name = "合租公寓",
                InviteCode = "SHARE2024",
                CreatedBy = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Household
            {
                Name = "三口之家",
                InviteCode = "FAMILY3",
                CreatedBy = 4,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        await context.Households.AddRangeAsync(households);
        await context.SaveChangesAsync();
    }

    private static async Task SeedHouseholdMembersAsync(FridgeWatchDbContext context)
    {
        var members = new List<HouseholdMember>
        {
            new HouseholdMember
            {
                HouseholdId = 1,
                UserId = 1,
                Role = HouseholdRole.Owner,
                JoinedAt = DateTime.UtcNow.AddDays(-28)
            },
            new HouseholdMember
            {
                HouseholdId = 1,
                UserId = 2,
                Role = HouseholdRole.Member,
                JoinedAt = DateTime.UtcNow.AddDays(-25)
            },
            new HouseholdMember
            {
                HouseholdId = 2,
                UserId = 2,
                Role = HouseholdRole.Owner,
                JoinedAt = DateTime.UtcNow.AddDays(-20)
            },
            new HouseholdMember
            {
                HouseholdId = 2,
                UserId = 3,
                Role = HouseholdRole.Member,
                JoinedAt = DateTime.UtcNow.AddDays(-18)
            },
            new HouseholdMember
            {
                HouseholdId = 2,
                UserId = 5,
                Role = HouseholdRole.Member,
                JoinedAt = DateTime.UtcNow.AddDays(-12)
            },
            new HouseholdMember
            {
                HouseholdId = 3,
                UserId = 4,
                Role = HouseholdRole.Owner,
                JoinedAt = DateTime.UtcNow.AddDays(-15)
            },
            new HouseholdMember
            {
                HouseholdId = 3,
                UserId = 1,
                Role = HouseholdRole.Member,
                JoinedAt = DateTime.UtcNow.AddDays(-10)
            }
        };

        await context.HouseholdMembers.AddRangeAsync(members);
        await context.SaveChangesAsync();
    }

    private static async Task SeedFoodItemsAsync(FridgeWatchDbContext context)
    {
        var today = DateTime.UtcNow.Date;

        var foodItems = new List<FoodItem>
        {
            new FoodItem
            {
                HouseholdId = 1,
                Name = "有机牛奶",
                Category = "乳制品",
                StorageLocation = StorageLocation.Fridge,
                PurchaseDate = today.AddDays(-3),
                ExpiryDate = today.AddDays(4),
                Quantity = 2,
                Unit = "升",
                PhotoUrl = "https://images.unsplash.com/photo-1563636619-e9143da7973b?w=200",
                Status = FoodStatus.Fresh,
                CreatedAt = today.AddDays(-3)
            },
            new FoodItem
            {
                HouseholdId = 1,
                Name = "土鸡蛋",
                Category = "蛋类",
                StorageLocation = StorageLocation.Fridge,
                PurchaseDate = today.AddDays(-5),
                ExpiryDate = today.AddDays(25),
                Quantity = 30,
                Unit = "个",
                PhotoUrl = "https://images.unsplash.com/photo-1582722872445-44dc5f7e3c8f?w=200",
                Status = FoodStatus.Fresh,
                CreatedAt = today.AddDays(-5)
            },
            new FoodItem
            {
                HouseholdId = 1,
                Name = "草莓",
                Category = "水果",
                StorageLocation = StorageLocation.Fridge,
                PurchaseDate = today.AddDays(-2),
                ExpiryDate = today.AddDays(1),
                Quantity = 1,
                Unit = "盒",
                PhotoUrl = "https://images.unsplash.com/photo-1464965911861-746a04b4bca6?w=200",
                Status = FoodStatus.NearExpiry,
                CreatedAt = today.AddDays(-2)
            },
            new FoodItem
            {
                HouseholdId = 1,
                Name = "鸡胸肉",
                Category = "肉类",
                StorageLocation = StorageLocation.Freezer,
                PurchaseDate = today.AddDays(-7),
                ExpiryDate = today.AddDays(60),
                Quantity = 2,
                Unit = "公斤",
                PhotoUrl = "https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=200",
                Status = FoodStatus.Fresh,
                CreatedAt = today.AddDays(-7)
            },
            new FoodItem
            {
                HouseholdId = 1,
                Name = "西红柿",
                Category = "蔬菜",
                StorageLocation = StorageLocation.Pantry,
                PurchaseDate = today.AddDays(-4),
                ExpiryDate = today.AddDays(3),
                Quantity = 5,
                Unit = "个",
                PhotoUrl = "https://images.unsplash.com/photo-1546470427-227c7eb6ae04?w=200",
                Status = FoodStatus.Fresh,
                CreatedAt = today.AddDays(-4)
            },
            new FoodItem
            {
                HouseholdId = 2,
                Name = "酸奶",
                Category = "乳制品",
                StorageLocation = StorageLocation.Fridge,
                PurchaseDate = today.AddDays(-6),
                ExpiryDate = today.AddDays(-1),
                Quantity = 4,
                Unit = "杯",
                PhotoUrl = "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=200",
                Status = FoodStatus.Expired,
                CreatedAt = today.AddDays(-6)
            },
            new FoodItem
            {
                HouseholdId = 2,
                Name = "全麦面包",
                Category = "主食",
                StorageLocation = StorageLocation.Pantry,
                PurchaseDate = today.AddDays(-2),
                ExpiryDate = today.AddDays(5),
                Quantity = 1,
                Unit = "条",
                PhotoUrl = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=200",
                Status = FoodStatus.Fresh,
                CreatedAt = today.AddDays(-2)
            },
            new FoodItem
            {
                HouseholdId = 2,
                Name = "速冻饺子",
                Category = "速冻食品",
                StorageLocation = StorageLocation.Freezer,
                PurchaseDate = today.AddDays(-10),
                ExpiryDate = today.AddDays(80),
                Quantity = 3,
                Unit = "袋",
                PhotoUrl = "https://images.unsplash.com/photo-1563245372-f21724e3856d?w=200",
                Status = FoodStatus.Fresh,
                CreatedAt = today.AddDays(-10)
            },
            new FoodItem
            {
                HouseholdId = 3,
                Name = "三文鱼",
                Category = "海鲜",
                StorageLocation = StorageLocation.Fridge,
                PurchaseDate = today.AddDays(-1),
                ExpiryDate = today.AddDays(2),
                Quantity = 1,
                Unit = "斤",
                PhotoUrl = "https://images.unsplash.com/photo-1599084993091-1cb5c0721cc6?w=200",
                Status = FoodStatus.Fresh,
                CreatedAt = today.AddDays(-1)
            },
            new FoodItem
            {
                HouseholdId = 3,
                Name = "苹果",
                Category = "水果",
                StorageLocation = StorageLocation.Fridge,
                PurchaseDate = today.AddDays(-5),
                ExpiryDate = today.AddDays(15),
                Quantity = 10,
                Unit = "个",
                PhotoUrl = "https://images.unsplash.com/photo-1560806887-1e4cd0b6cbd6?w=200",
                Status = FoodStatus.Fresh,
                CreatedAt = today.AddDays(-5)
            }
        };

        await context.FoodItems.AddRangeAsync(foodItems);
        await context.SaveChangesAsync();
    }

    private static async Task SeedExpiryAlertsAsync(FridgeWatchDbContext context)
    {
        var today = DateTime.UtcNow.Date;

        var alerts = new List<ExpiryAlert>
        {
            new ExpiryAlert
            {
                FoodItemId = 3,
                UserId = 1,
                AlertType = AlertType.NearExpiry,
                AlertDate = today.AddDays(-1),
                IsRead = false,
                CreatedAt = today.AddDays(-1)
            },
            new ExpiryAlert
            {
                FoodItemId = 6,
                UserId = 2,
                AlertType = AlertType.Expired,
                AlertDate = today,
                IsRead = false,
                CreatedAt = today
            },
            new ExpiryAlert
            {
                FoodItemId = 5,
                UserId = 1,
                AlertType = AlertType.NearExpiry,
                AlertDate = today,
                IsRead = true,
                CreatedAt = today
            },
            new ExpiryAlert
            {
                FoodItemId = 9,
                UserId = 4,
                AlertType = AlertType.NearExpiry,
                AlertDate = today.AddDays(1),
                IsRead = false,
                CreatedAt = today
            }
        };

        await context.ExpiryAlerts.AddRangeAsync(alerts);
        await context.SaveChangesAsync();
    }

    private static async Task SeedConsumptionRecordsAsync(FridgeWatchDbContext context)
    {
        var today = DateTime.UtcNow.Date;

        var records = new List<ConsumptionRecord>
        {
            new ConsumptionRecord
            {
                FoodItemId = 1,
                UserId = 1,
                ConsumedQuantity = 0.5m,
                ConsumedAt = today.AddDays(-1),
                Note = "早餐喝了半升",
                CreatedAt = today.AddDays(-1)
            },
            new ConsumptionRecord
            {
                FoodItemId = 2,
                UserId = 2,
                ConsumedQuantity = 2,
                ConsumedAt = today.AddDays(-2),
                Note = "煮了两个水煮蛋",
                CreatedAt = today.AddDays(-2)
            },
            new ConsumptionRecord
            {
                FoodItemId = 4,
                UserId = 1,
                ConsumedQuantity = 0.3m,
                ConsumedAt = today.AddDays(-3),
                Note = "做了鸡胸肉沙拉",
                CreatedAt = today.AddDays(-3)
            },
            new ConsumptionRecord
            {
                FoodItemId = 7,
                UserId = 3,
                ConsumedQuantity = 0.3m,
                ConsumedAt = today.AddDays(-1),
                Note = "早餐吃了几片",
                CreatedAt = today.AddDays(-1)
            },
            new ConsumptionRecord
            {
                FoodItemId = 10,
                UserId = 4,
                ConsumedQuantity = 2,
                ConsumedAt = today,
                Note = "饭后水果",
                CreatedAt = today
            }
        };

        await context.ConsumptionRecords.AddRangeAsync(records);
        await context.SaveChangesAsync();
    }
}
