using Microsoft.EntityFrameworkCore;
using FridgeWatch.Domain.Entities;

namespace FridgeWatch.Infrastructure.Data;

public class FridgeWatchDbContext : DbContext
{
    public FridgeWatchDbContext(DbContextOptions<FridgeWatchDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Household> Households { get; set; }
    public DbSet<HouseholdMember> HouseholdMembers { get; set; }
    public DbSet<FoodItem> FoodItems { get; set; }
    public DbSet<ExpiryAlert> ExpiryAlerts { get; set; }
    public DbSet<ConsumptionRecord> ConsumptionRecords { get; set; }
    public DbSet<ShoppingList> ShoppingLists { get; set; }
    public DbSet<ShoppingListItem> ShoppingListItems { get; set; }
    public DbSet<ShareLink> ShareLinks { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User 配置
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
            entity.Property(u => u.Avatar).HasMaxLength(500);
            entity.HasOne(u => u.DefaultHousehold)
                  .WithMany()
                  .HasForeignKey(u => u.DefaultHouseholdId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Household 配置
        modelBuilder.Entity<Household>(entity =>
        {
            entity.HasIndex(h => h.InviteCode).IsUnique();
            entity.Property(h => h.Name).IsRequired().HasMaxLength(100);
            entity.Property(h => h.InviteCode).IsRequired().HasMaxLength(20);
        });

        // HouseholdMember 配置
        modelBuilder.Entity<HouseholdMember>(entity =>
        {
            entity.HasIndex(hm => new { hm.HouseholdId, hm.UserId }).IsUnique();
            entity.HasOne(hm => hm.Household)
                  .WithMany(h => h.HouseholdMembers)
                  .HasForeignKey(hm => hm.HouseholdId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(hm => hm.User)
                  .WithMany(u => u.HouseholdMembers)
                  .HasForeignKey(hm => hm.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // FoodItem 配置
        modelBuilder.Entity<FoodItem>(entity =>
        {
            entity.HasOne(f => f.Household)
                  .WithMany(h => h.FoodItems)
                  .HasForeignKey(f => f.HouseholdId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(f => f.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(f => f.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(f => f.Name).IsRequired().HasMaxLength(100);
            entity.Property(f => f.Category).IsRequired().HasMaxLength(50);
            entity.Property(f => f.Unit).IsRequired().HasMaxLength(20);
            entity.Property(f => f.PhotoUrl).HasMaxLength(500);
            entity.Property(f => f.Quantity).HasColumnType("decimal(18,2)");
        });

        // ExpiryAlert 配置
        modelBuilder.Entity<ExpiryAlert>(entity =>
        {
            entity.HasOne(ea => ea.FoodItem)
                  .WithMany(f => f.ExpiryAlerts)
                  .HasForeignKey(ea => ea.FoodItemId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ea => ea.User)
                  .WithMany(u => u.ExpiryAlerts)
                  .HasForeignKey(ea => ea.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ConsumptionRecord 配置
        modelBuilder.Entity<ConsumptionRecord>(entity =>
        {
            entity.HasOne(cr => cr.FoodItem)
                  .WithMany(f => f.ConsumptionRecords)
                  .HasForeignKey(cr => cr.FoodItemId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cr => cr.User)
                  .WithMany(u => u.ConsumptionRecords)
                  .HasForeignKey(cr => cr.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(cr => cr.ConsumedQuantity).HasColumnType("decimal(18,2)");
            entity.Property(cr => cr.Note).HasMaxLength(500);
        });

        // ShoppingList 配置
        modelBuilder.Entity<ShoppingList>(entity =>
        {
            entity.HasOne(sl => sl.Household)
                  .WithMany(h => h.ShoppingLists)
                  .HasForeignKey(sl => sl.HouseholdId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(sl => sl.Name).IsRequired().HasMaxLength(100);
        });

        // ShoppingListItem 配置
        modelBuilder.Entity<ShoppingListItem>(entity =>
        {
            entity.HasOne(sli => sli.ShoppingList)
                  .WithMany(sl => sl.Items)
                  .HasForeignKey(sli => sli.ShoppingListId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(sli => sli.Name).IsRequired().HasMaxLength(100);
            entity.Property(sli => sli.Category).IsRequired().HasMaxLength(50);
            entity.Property(sli => sli.Unit).IsRequired().HasMaxLength(20);
            entity.Property(sli => sli.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(sli => sli.Notes).HasMaxLength(500);
        });

        // ShareLink 配置
        modelBuilder.Entity<ShareLink>(entity =>
        {
            entity.HasIndex(sl => sl.Token).IsUnique();
            entity.HasOne(sl => sl.Household)
                  .WithMany()
                  .HasForeignKey(sl => sl.HouseholdId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(sl => sl.Token).IsRequired().HasMaxLength(64);
        });

        // Recipe 配置
        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Description).IsRequired().HasMaxLength(500);
            entity.Property(r => r.Category).IsRequired().HasMaxLength(50);
            entity.Property(r => r.Instructions).IsRequired().HasMaxLength(2000);
            entity.Property(r => r.ImageUrl).HasMaxLength(500);
        });

        // RecipeIngredient 配置
        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasOne(ri => ri.Recipe)
                  .WithMany(r => r.Ingredients)
                  .HasForeignKey(ri => ri.RecipeId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(ri => ri.IngredientName).IsRequired().HasMaxLength(100);
            entity.Property(ri => ri.IngredientCategory).IsRequired().HasMaxLength(50);
            entity.Property(ri => ri.Unit).IsRequired().HasMaxLength(20);
            entity.Property(ri => ri.Quantity).HasColumnType("decimal(18,2)");
        });
    }
}
