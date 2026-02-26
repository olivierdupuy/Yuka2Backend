using Microsoft.EntityFrameworkCore;
using Yuka2Back.Models;

namespace Yuka2Back.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ScanHistory> ScanHistories => Set<ScanHistory>();
    public DbSet<FavoriteProduct> FavoriteProducts => Set<FavoriteProduct>();

    // New features
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
    public DbSet<ShoppingListItem> ShoppingListItems => Set<ShoppingListItem>();
    public DbSet<ProductComparison> ProductComparisons => Set<ProductComparison>();
    public DbSet<AllergenAlert> AllergenAlerts => Set<AllergenAlert>();

    // Analytics
    public DbSet<AppSession> AppSessions => Set<AppSession>();
    public DbSet<PageView> PageViews => Set<PageView>();
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<ApiLog> ApiLogs => Set<ApiLog>();
    public DbSet<AppConfig> AppConfigs => Set<AppConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Existing configurations ---

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Barcode)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(r => r.Token)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ScanHistory>()
            .HasOne(s => s.User)
            .WithMany(u => u.ScanHistories)
            .HasForeignKey(s => s.UserId);

        modelBuilder.Entity<ScanHistory>()
            .HasOne(s => s.Product)
            .WithMany(p => p.ScanHistories)
            .HasForeignKey(s => s.ProductId);

        modelBuilder.Entity<FavoriteProduct>()
            .HasOne(f => f.User)
            .WithMany(u => u.FavoriteProducts)
            .HasForeignKey(f => f.UserId);

        modelBuilder.Entity<FavoriteProduct>()
            .HasOne(f => f.Product)
            .WithMany(p => p.FavoriteProducts)
            .HasForeignKey(f => f.ProductId);

        modelBuilder.Entity<FavoriteProduct>()
            .HasIndex(f => new { f.UserId, f.ProductId })
            .IsUnique();

        // --- New features configurations ---

        // ProductReview
        modelBuilder.Entity<ProductReview>()
            .HasOne(r => r.User)
            .WithMany(u => u.ProductReviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductReview>()
            .HasOne(r => r.Product)
            .WithMany(p => p.ProductReviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductReview>()
            .HasIndex(r => new { r.UserId, r.ProductId })
            .IsUnique();

        modelBuilder.Entity<ProductReview>()
            .HasIndex(r => r.CreatedAt);

        // ShoppingList
        modelBuilder.Entity<ShoppingList>()
            .HasOne(sl => sl.User)
            .WithMany(u => u.ShoppingLists)
            .HasForeignKey(sl => sl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ShoppingList>()
            .HasIndex(sl => sl.UserId);

        // ShoppingListItem
        modelBuilder.Entity<ShoppingListItem>()
            .HasOne(sli => sli.ShoppingList)
            .WithMany(sl => sl.Items)
            .HasForeignKey(sli => sli.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ShoppingListItem>()
            .HasOne(sli => sli.Product)
            .WithMany()
            .HasForeignKey(sli => sli.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        // ProductComparison
        modelBuilder.Entity<ProductComparison>()
            .HasOne(pc => pc.User)
            .WithMany(u => u.ProductComparisons)
            .HasForeignKey(pc => pc.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ProductComparison>()
            .HasIndex(pc => pc.CreatedAt);

        // AllergenAlert
        modelBuilder.Entity<AllergenAlert>()
            .HasOne(aa => aa.User)
            .WithMany(u => u.AllergenAlerts)
            .HasForeignKey(aa => aa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AllergenAlert>()
            .HasIndex(aa => aa.UserId);

        modelBuilder.Entity<AllergenAlert>()
            .HasIndex(aa => aa.CreatedAt);

        // --- Analytics configurations ---

        // AppSession indexes
        modelBuilder.Entity<AppSession>()
            .HasIndex(s => s.UserId);

        modelBuilder.Entity<AppSession>()
            .HasIndex(s => s.DeviceId);

        modelBuilder.Entity<AppSession>()
            .HasIndex(s => s.StartedAt);

        // PageView relationships and indexes
        modelBuilder.Entity<PageView>()
            .HasOne(pv => pv.Session)
            .WithMany(s => s.PageViews)
            .HasForeignKey(pv => pv.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PageView>()
            .HasIndex(pv => pv.SessionId);

        modelBuilder.Entity<PageView>()
            .HasIndex(pv => pv.UserId);

        modelBuilder.Entity<PageView>()
            .HasIndex(pv => pv.PageName);

        modelBuilder.Entity<PageView>()
            .HasIndex(pv => pv.EnteredAt);

        // AnalyticsEvent relationships and indexes
        modelBuilder.Entity<AnalyticsEvent>()
            .HasOne(e => e.Session)
            .WithMany(s => s.Events)
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AnalyticsEvent>()
            .HasIndex(e => e.SessionId);

        modelBuilder.Entity<AnalyticsEvent>()
            .HasIndex(e => e.UserId);

        modelBuilder.Entity<AnalyticsEvent>()
            .HasIndex(e => e.EventType);

        modelBuilder.Entity<AnalyticsEvent>()
            .HasIndex(e => e.CreatedAt);

        modelBuilder.Entity<AnalyticsEvent>()
            .HasIndex(e => e.DeviceId);

        // AdminUser indexes
        modelBuilder.Entity<AdminUser>()
            .HasIndex(a => a.Username)
            .IsUnique();

        modelBuilder.Entity<AdminUser>()
            .HasIndex(a => a.Email)
            .IsUnique();

        // ApiLog indexes
        modelBuilder.Entity<ApiLog>()
            .HasIndex(l => l.CreatedAt);

        modelBuilder.Entity<ApiLog>()
            .HasIndex(l => l.Path);

        modelBuilder.Entity<ApiLog>()
            .HasIndex(l => l.UserId);

        modelBuilder.Entity<ApiLog>()
            .HasIndex(l => l.StatusCode);

        // Seed data
        SeedProducts(modelBuilder);
    }

    private static void SeedProducts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Barcode = "3017620422003", Name = "Nutella", Brand = "Ferrero", NutriScore = "E", NovaGroup = 4, HealthScore = 15, Calories = 539, Fat = 30.9, SaturatedFat = 10.6, Sugars = 56.3, Salt = 0.107, Fiber = 0, Proteins = 6.3, Carbohydrates = 57.5, Ingredients = "Sucre, huile de palme, noisettes 13%, cacao maigre 7.4%, lait écrémé en poudre 6.6%, lactosérum en poudre, émulsifiants : lécithines (soja), vanilline", Allergens = "Lait, Soja, Fruits à coque", Categories = "Pâtes à tartiner", Quantity = "400g", IsOrganic = false, IsPalmOilFree = false, IsVegan = false },
            new Product { Id = 2, Barcode = "3175680011480", Name = "Eau minérale naturelle", Brand = "Volvic", NutriScore = "A", NovaGroup = 1, HealthScore = 100, Calories = 0, Fat = 0, SaturatedFat = 0, Sugars = 0, Salt = 0, Fiber = 0, Proteins = 0, Carbohydrates = 0, Ingredients = "Eau minérale naturelle", Allergens = "", Categories = "Boissons, Eaux", Quantity = "1.5L", IsOrganic = false, IsPalmOilFree = true, IsVegan = true },
            new Product { Id = 3, Barcode = "3228857000166", Name = "Camembert", Brand = "Président", NutriScore = "D", NovaGroup = 3, HealthScore = 35, Calories = 299, Fat = 24, SaturatedFat = 16.6, Sugars = 0.5, Salt = 1.6, Fiber = 0, Proteins = 20, Carbohydrates = 0.5, Ingredients = "Lait pasteurisé, sel, ferments lactiques, coagulant", Allergens = "Lait", Categories = "Fromages", Quantity = "250g", IsOrganic = false, IsPalmOilFree = true, IsVegan = false },
            new Product { Id = 4, Barcode = "3560070472734", Name = "Salade mêlée", Brand = "Carrefour Bio", NutriScore = "A", NovaGroup = 1, HealthScore = 92, Calories = 19, Fat = 0.3, SaturatedFat = 0, Sugars = 0.6, Salt = 0.02, Fiber = 1.8, Proteins = 1.5, Carbohydrates = 1.8, Ingredients = "Salade mêlée issue de l'agriculture biologique", Allergens = "", Categories = "Légumes, Salades", Quantity = "150g", IsOrganic = true, IsPalmOilFree = true, IsVegan = true },
            new Product { Id = 5, Barcode = "3017760000109", Name = "Prince Chocolat", Brand = "LU", NutriScore = "D", NovaGroup = 4, HealthScore = 22, Calories = 468, Fat = 17, SaturatedFat = 8.2, Sugars = 32, Salt = 0.68, Fiber = 2.3, Proteins = 6.2, Carbohydrates = 70, Ingredients = "Farine de blé 47%, sucre, huiles végétales (palme, colza), cacao maigre en poudre 5.1%, sirop de glucose, amidon de blé, poudre à lever", Allergens = "Gluten, Lait, Soja", Categories = "Biscuits, Snacks sucrés", Quantity = "300g", IsOrganic = false, IsPalmOilFree = false, IsVegan = false },
            new Product { Id = 6, Barcode = "3274080005003", Name = "Compote Pomme", Brand = "Andros", NutriScore = "A", NovaGroup = 1, HealthScore = 78, Calories = 52, Fat = 0.1, SaturatedFat = 0, Sugars = 10.7, Salt = 0, Fiber = 1.7, Proteins = 0.3, Carbohydrates = 11.9, Ingredients = "Pommes 100%", Allergens = "", Categories = "Compotes, Fruits", Quantity = "750g", IsOrganic = false, IsPalmOilFree = true, IsVegan = true },
            new Product { Id = 7, Barcode = "3256540001305", Name = "Yaourt Nature", Brand = "Danone", NutriScore = "A", NovaGroup = 1, HealthScore = 85, Calories = 46, Fat = 1, SaturatedFat = 0.7, Sugars = 4.7, Salt = 0.13, Fiber = 0, Proteins = 4.2, Carbohydrates = 4.7, Ingredients = "Lait écrémé, ferments lactiques", Allergens = "Lait", Categories = "Produits laitiers, Yaourts", Quantity = "4x125g", IsOrganic = false, IsPalmOilFree = true, IsVegan = false },
            new Product { Id = 8, Barcode = "3560070452712", Name = "Chips nature", Brand = "Lay's", NutriScore = "C", NovaGroup = 3, HealthScore = 38, Calories = 536, Fat = 35, SaturatedFat = 3, Sugars = 0.5, Salt = 1.3, Fiber = 4.3, Proteins = 6.5, Carbohydrates = 50, Ingredients = "Pommes de terre, huile de tournesol 33%, sel", Allergens = "", Categories = "Chips, Snacks salés", Quantity = "150g", IsOrganic = false, IsPalmOilFree = true, IsVegan = true },
            new Product { Id = 9, Barcode = "3073781155211", Name = "Ratatouille cuisinée", Brand = "Bonduelle", NutriScore = "A", NovaGroup = 3, HealthScore = 82, Calories = 57, Fat = 2.3, SaturatedFat = 0.3, Sugars = 4.3, Salt = 0.65, Fiber = 1.6, Proteins = 1.1, Carbohydrates = 6.4, Ingredients = "Tomates 38%, courgettes 21%, poivrons rouges 12%, aubergines 9%, oignons, huile de tournesol, sel, ail, basilic, thym, poivre", Allergens = "", Categories = "Conserves, Légumes", Quantity = "660g", IsOrganic = false, IsPalmOilFree = true, IsVegan = true },
            new Product { Id = 10, Barcode = "3046920028363", Name = "Chocolat noir 70%", Brand = "Lindt", NutriScore = "C", NovaGroup = 3, HealthScore = 55, Calories = 545, Fat = 41, SaturatedFat = 25, Sugars = 26, Salt = 0.02, Fiber = 11, Proteins = 9.3, Carbohydrates = 33, Ingredients = "Pâte de cacao, sucre, beurre de cacao, vanille", Allergens = "Lait, Soja", Categories = "Chocolats", Quantity = "100g", IsOrganic = false, IsPalmOilFree = true, IsVegan = false }
        );
    }
}
