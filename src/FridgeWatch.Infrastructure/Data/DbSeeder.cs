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

        if (!await context.Recipes.AnyAsync())
        {
            await SeedRecipesAsync(context);
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

    private static async Task SeedRecipesAsync(FridgeWatchDbContext context)
    {
        var recipes = new List<Recipe>
        {
            new Recipe
            {
                Name = "番茄炒鸡蛋",
                Description = "经典家常菜，酸甜可口，营养丰富，适合快速晚餐",
                Category = "家常菜",
                Difficulty = 1,
                CookTimeMinutes = 15,
                Instructions = "1. 西红柿洗净切块，鸡蛋打散备用\n2. 热锅下油，倒入蛋液炒至凝固盛出\n3. 锅中再加少许油，放入西红柿翻炒出汁\n4. 加入炒好的鸡蛋，加盐、糖调味\n5. 翻炒均匀即可出锅",
                ImageUrl = "https://images.unsplash.com/photo-1607532941433-304659e8198a?w=400",
                Servings = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                Ingredients = new List<RecipeIngredient>
                {
                    new() { IngredientName = "西红柿", IngredientCategory = "蔬菜", Quantity = 3, Unit = "个", IsOptional = false },
                    new() { IngredientName = "鸡蛋", IngredientCategory = "蛋类", Quantity = 3, Unit = "个", IsOptional = false },
                    new() { IngredientName = "食用油", IngredientCategory = "调料", Quantity = 2, Unit = "勺", IsOptional = false },
                    new() { IngredientName = "盐", IngredientCategory = "调料", Quantity = 1, Unit = "克", IsOptional = false },
                    new() { IngredientName = "白糖", IngredientCategory = "调料", Quantity = 1, Unit = "勺", IsOptional = true }
                }
            },
            new Recipe
            {
                Name = "香煎鸡胸肉",
                Description = "高蛋白低脂健康餐，外焦里嫩，适合健身人群",
                Category = "健身餐",
                Difficulty = 2,
                CookTimeMinutes = 20,
                Instructions = "1. 鸡胸肉洗净，用厨房纸吸干水分\n2. 两面撒盐、黑胡椒腌制10分钟\n3. 热锅加油，中火煎至两面金黄\n4. 加入蒜末、黄油增香\n5. 盛出静置3分钟后切片食用",
                ImageUrl = "https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=400",
                Servings = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                Ingredients = new List<RecipeIngredient>
                {
                    new() { IngredientName = "鸡胸肉", IngredientCategory = "肉类", Quantity = 0.5m, Unit = "公斤", IsOptional = false },
                    new() { IngredientName = "橄榄油", IngredientCategory = "调料", Quantity = 2, Unit = "勺", IsOptional = false },
                    new() { IngredientName = "黑胡椒", IngredientCategory = "调料", Quantity = 1, Unit = "克", IsOptional = false },
                    new() { IngredientName = "盐", IngredientCategory = "调料", Quantity = 2, Unit = "克", IsOptional = false },
                    new() { IngredientName = "大蒜", IngredientCategory = "蔬菜", Quantity = 3, Unit = "瓣", IsOptional = true },
                    new() { IngredientName = "黄油", IngredientCategory = "乳制品", Quantity = 10, Unit = "克", IsOptional = true }
                }
            },
            new Recipe
            {
                Name = "草莓奶昔",
                Description = "香甜顺滑的水果奶昔，早餐或下午茶的好选择",
                Category = "饮品",
                Difficulty = 1,
                CookTimeMinutes = 5,
                Instructions = "1. 草莓洗净去蒂\n2. 将草莓、牛奶、酸奶放入搅拌机\n3. 加入蜂蜜或白糖调味\n4. 搅拌至顺滑即可饮用\n5. 可加冰块做成冰沙",
                ImageUrl = "https://images.unsplash.com/photo-1553530666-ba11a7da3888?w=400",
                Servings = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                Ingredients = new List<RecipeIngredient>
                {
                    new() { IngredientName = "草莓", IngredientCategory = "水果", Quantity = 300, Unit = "克", IsOptional = false },
                    new() { IngredientName = "牛奶", IngredientCategory = "乳制品", Quantity = 250, Unit = "毫升", IsOptional = false },
                    new() { IngredientName = "酸奶", IngredientCategory = "乳制品", Quantity = 100, Unit = "克", IsOptional = true },
                    new() { IngredientName = "蜂蜜", IngredientCategory = "调料", Quantity = 1, Unit = "勺", IsOptional = true },
                    new() { IngredientName = "冰块", IngredientCategory = "其他", Quantity = 5, Unit = "块", IsOptional = true }
                }
            },
            new Recipe
            {
                Name = "三文鱼刺身",
                Description = "新鲜三文鱼切片，蘸芥末酱油食用，鲜美无比",
                Category = "海鲜",
                Difficulty = 1,
                CookTimeMinutes = 10,
                Instructions = "1. 三文鱼去皮，用干净的刀切片\n2. 摆盘，用萝卜丝装饰\n3. 准备芥末和酱油作为蘸料\n4. 直接食用即可",
                ImageUrl = "https://images.unsplash.com/photo-1599084993091-1cb5c0721cc6?w=400",
                Servings = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                Ingredients = new List<RecipeIngredient>
                {
                    new() { IngredientName = "三文鱼", IngredientCategory = "海鲜", Quantity = 300, Unit = "克", IsOptional = false },
                    new() { IngredientName = "酱油", IngredientCategory = "调料", Quantity = 2, Unit = "勺", IsOptional = false },
                    new() { IngredientName = "芥末", IngredientCategory = "调料", Quantity = 1, Unit = "克", IsOptional = true },
                    new() { IngredientName = "白萝卜", IngredientCategory = "蔬菜", Quantity = 50, Unit = "克", IsOptional = true }
                }
            },
            new Recipe
            {
                Name = "苹果沙拉",
                Description = "清爽健康的水果沙拉，简单易做",
                Category = "沙拉",
                Difficulty = 1,
                CookTimeMinutes = 10,
                Instructions = "1. 苹果洗净去皮切丁\n2. 其他水果（可选）同样处理\n3. 加入酸奶或沙拉酱拌匀\n4. 冷藏后食用更佳",
                ImageUrl = "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400",
                Servings = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                Ingredients = new List<RecipeIngredient>
                {
                    new() { IngredientName = "苹果", IngredientCategory = "水果", Quantity = 2, Unit = "个", IsOptional = false },
                    new() { IngredientName = "酸奶", IngredientCategory = "乳制品", Quantity = 100, Unit = "克", IsOptional = true },
                    new() { IngredientName = "沙拉酱", IngredientCategory = "调料", Quantity = 2, Unit = "勺", IsOptional = true },
                    new() { IngredientName = "蜂蜜", IngredientCategory = "调料", Quantity = 1, Unit = "勺", IsOptional = true }
                }
            },
            new Recipe
            {
                Name = "吐司煎蛋",
                Description = "简单快手早餐，面包酥脆配流心蛋",
                Category = "早餐",
                Difficulty = 1,
                CookTimeMinutes = 10,
                Instructions = "1. 面包片中间用杯子压出圆形\n2. 热锅融化黄油，放入面包\n3. 在圆孔中打入鸡蛋\n4. 煎至蛋白凝固，翻面煎熟\n5. 撒盐和黑胡椒调味",
                ImageUrl = "https://images.unsplash.com/photo-1525351484163-7529414344d8?w=400",
                Servings = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                Ingredients = new List<RecipeIngredient>
                {
                    new() { IngredientName = "面包", IngredientCategory = "主食", Quantity = 2, Unit = "片", IsOptional = false },
                    new() { IngredientName = "鸡蛋", IngredientCategory = "蛋类", Quantity = 2, Unit = "个", IsOptional = false },
                    new() { IngredientName = "黄油", IngredientCategory = "乳制品", Quantity = 10, Unit = "克", IsOptional = false },
                    new() { IngredientName = "盐", IngredientCategory = "调料", Quantity = 1, Unit = "克", IsOptional = true },
                    new() { IngredientName = "黑胡椒", IngredientCategory = "调料", Quantity = 1, Unit = "克", IsOptional = true }
                }
            },
            new Recipe
            {
                Name = "牛奶燕麦粥",
                Description = "营养丰富的暖胃早餐，适合全家食用",
                Category = "早餐",
                Difficulty = 1,
                CookTimeMinutes = 10,
                Instructions = "1. 锅中倒入牛奶加热\n2. 加入燕麦片，小火煮5分钟\n3. 不停搅拌防止粘锅\n4. 煮至浓稠后关火\n5. 可加蜂蜜、水果等调味",
                ImageUrl = "https://images.unsplash.com/photo-1517673400267-0251440c45dc?w=400",
                Servings = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                Ingredients = new List<RecipeIngredient>
                {
                    new() { IngredientName = "牛奶", IngredientCategory = "乳制品", Quantity = 500, Unit = "毫升", IsOptional = false },
                    new() { IngredientName = "燕麦片", IngredientCategory = "主食", Quantity = 80, Unit = "克", IsOptional = false },
                    new() { IngredientName = "蜂蜜", IngredientCategory = "调料", Quantity = 1, Unit = "勺", IsOptional = true },
                    new() { IngredientName = "草莓", IngredientCategory = "水果", Quantity = 5, Unit = "颗", IsOptional = true }
                }
            },
            new Recipe
            {
                Name = "速冻水饺",
                Description = "简单便捷的家常主食，适合忙碌的工作日",
                Category = "主食",
                Difficulty = 1,
                CookTimeMinutes = 15,
                Instructions = "1. 锅中加水烧开\n2. 放入饺子，轻轻搅拌防粘\n3. 水开后加半碗冷水，重复3次\n4. 饺子浮起且鼓胀即可捞出\n5. 配醋、蒜泥蘸料食用",
                ImageUrl = "https://images.unsplash.com/photo-1563245372-f21724e3856d?w=400",
                Servings = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                Ingredients = new List<RecipeIngredient>
                {
                    new() { IngredientName = "饺子", IngredientCategory = "速冻食品", Quantity = 30, Unit = "个", IsOptional = false },
                    new() { IngredientName = "醋", IngredientCategory = "调料", Quantity = 2, Unit = "勺", IsOptional = true },
                    new() { IngredientName = "大蒜", IngredientCategory = "蔬菜", Quantity = 2, Unit = "瓣", IsOptional = true }
                }
            }
        };

        await context.Recipes.AddRangeAsync(recipes);
        await context.SaveChangesAsync();
    }
}
