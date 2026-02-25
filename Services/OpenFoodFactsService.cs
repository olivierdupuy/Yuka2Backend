using System.Text.Json;
using Yuka2Back.Models;

namespace Yuka2Back.Services;

public class OpenFoodFactsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenFoodFactsService> _logger;

    public OpenFoodFactsService(HttpClient httpClient, ILogger<OpenFoodFactsService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://world.openfoodfacts.org/");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Yuka2App/1.0 (contact@yuka2.app)");
    }

    public async Task<Product?> GetProductByBarcodeAsync(string barcode)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v2/product/{barcode}.json");
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var status = root.GetProperty("status").GetInt32();
            if (status != 1)
                return null;

            var productNode = root.GetProperty("product");

            var product = new Product
            {
                Barcode = barcode,
                Name = GetString(productNode, "product_name_fr")
                       ?? GetString(productNode, "product_name")
                       ?? "Produit inconnu",
                Brand = GetString(productNode, "brands"),
                ImageUrl = GetString(productNode, "image_front_url")
                           ?? GetString(productNode, "image_url"),
                NutriScore = GetNutriScore(productNode),
                NovaGroup = GetNovaGroup(productNode),
                Ingredients = GetString(productNode, "ingredients_text_fr")
                              ?? GetString(productNode, "ingredients_text"),
                Allergens = FormatAllergens(productNode),
                Categories = GetCategories(productNode),
                Quantity = GetString(productNode, "quantity"),
                IsOrganic = CheckLabel(productNode, "bio", "organic"),
                IsPalmOilFree = CheckPalmOilFree(productNode),
                IsVegan = CheckLabel(productNode, "vegan"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            // Nutriments
            if (productNode.TryGetProperty("nutriments", out var nutriments))
            {
                product.Calories = GetDouble(nutriments, "energy-kcal_100g")
                                   ?? GetDouble(nutriments, "energy_100g");
                product.Fat = GetDouble(nutriments, "fat_100g");
                product.SaturatedFat = GetDouble(nutriments, "saturated-fat_100g");
                product.Sugars = GetDouble(nutriments, "sugars_100g");
                product.Salt = GetDouble(nutriments, "salt_100g");
                product.Fiber = GetDouble(nutriments, "fiber_100g");
                product.Proteins = GetDouble(nutriments, "proteins_100g");
                product.Carbohydrates = GetDouble(nutriments, "carbohydrates_100g");
            }

            // Calcul du Health Score (style Yuka)
            product.HealthScore = CalculateHealthScore(product);

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du produit {Barcode} depuis Open Food Facts", barcode);
            return null;
        }
    }

    public async Task<List<Product>> SearchProductsAsync(string query, int page = 1, int pageSize = 20)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"cgi/search.pl?search_terms={Uri.EscapeDataString(query)}&search_simple=1&action=process&json=1&page={page}&page_size={pageSize}&lc=fr");

            if (!response.IsSuccessStatusCode)
                return [];

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("products", out var productsArray))
                return [];

            var products = new List<Product>();
            foreach (var productNode in productsArray.EnumerateArray())
            {
                var barcode = GetString(productNode, "code");
                if (string.IsNullOrEmpty(barcode)) continue;

                var name = GetString(productNode, "product_name_fr")
                           ?? GetString(productNode, "product_name");
                if (string.IsNullOrEmpty(name)) continue;

                var product = new Product
                {
                    Barcode = barcode,
                    Name = name,
                    Brand = GetString(productNode, "brands"),
                    ImageUrl = GetString(productNode, "image_front_small_url")
                               ?? GetString(productNode, "image_front_url"),
                    NutriScore = GetNutriScore(productNode),
                    NovaGroup = GetNovaGroup(productNode),
                    Categories = GetCategories(productNode),
                    Quantity = GetString(productNode, "quantity"),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                if (productNode.TryGetProperty("nutriments", out var nutriments))
                {
                    product.Calories = GetDouble(nutriments, "energy-kcal_100g");
                    product.Fat = GetDouble(nutriments, "fat_100g");
                    product.SaturatedFat = GetDouble(nutriments, "saturated-fat_100g");
                    product.Sugars = GetDouble(nutriments, "sugars_100g");
                    product.Salt = GetDouble(nutriments, "salt_100g");
                    product.Fiber = GetDouble(nutriments, "fiber_100g");
                    product.Proteins = GetDouble(nutriments, "proteins_100g");
                    product.Carbohydrates = GetDouble(nutriments, "carbohydrates_100g");
                }

                product.HealthScore = CalculateHealthScore(product);
                products.Add(product);
            }

            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche Open Food Facts pour '{Query}'", query);
            return [];
        }
    }

    // --- Calcul du Health Score (algorithme inspiré Yuka) ---
    private static int CalculateHealthScore(Product p)
    {
        // Score basé sur le Nutri-Score + composition nutritionnelle
        double score = 50; // Base

        // Bonus/Malus Nutri-Score (40% du score)
        switch (p.NutriScore?.ToUpper())
        {
            case "A": score += 25; break;
            case "B": score += 15; break;
            case "C": score += 0; break;
            case "D": score -= 15; break;
            case "E": score -= 25; break;
        }

        // Malus éléments négatifs
        if (p.Sugars > 20) score -= 8;
        else if (p.Sugars > 10) score -= 4;

        if (p.SaturatedFat > 10) score -= 8;
        else if (p.SaturatedFat > 5) score -= 4;

        if (p.Salt > 1.5) score -= 6;
        else if (p.Salt > 0.5) score -= 3;

        if (p.Calories > 400) score -= 6;
        else if (p.Calories > 200) score -= 3;

        // Bonus éléments positifs
        if (p.Fiber > 3) score += 5;
        else if (p.Fiber > 1) score += 2;

        if (p.Proteins > 8) score += 5;
        else if (p.Proteins > 4) score += 2;

        // Bonus NOVA Group
        switch (p.NovaGroup)
        {
            case 1: score += 10; break;
            case 2: score += 5; break;
            case 3: score -= 2; break;
            case 4: score -= 10; break;
        }

        // Bonus labels
        if (p.IsOrganic) score += 5;
        if (p.IsPalmOilFree) score += 3;

        return Math.Clamp((int)Math.Round(score), 0, 100);
    }

    // --- Helpers d'extraction JSON ---
    private static string? GetString(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
        {
            var s = val.GetString()?.Trim();
            return string.IsNullOrEmpty(s) ? null : s;
        }
        return null;
    }

    private static double? GetDouble(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var val))
        {
            if (val.ValueKind == JsonValueKind.Number)
                return val.GetDouble();
            if (val.ValueKind == JsonValueKind.String && double.TryParse(val.GetString(), out var d))
                return d;
        }
        return null;
    }

    private static string? GetNutriScore(JsonElement productNode)
    {
        var grade = GetString(productNode, "nutriscore_grade")
                    ?? GetString(productNode, "nutrition_grades");
        return grade?.ToUpper() switch
        {
            "A" or "B" or "C" or "D" or "E" => grade.ToUpper(),
            _ => null
        };
    }

    private static int? GetNovaGroup(JsonElement productNode)
    {
        if (productNode.TryGetProperty("nova_group", out var val))
        {
            if (val.ValueKind == JsonValueKind.Number)
            {
                var n = val.GetInt32();
                return n is >= 1 and <= 4 ? n : null;
            }
            if (val.ValueKind == JsonValueKind.String && int.TryParse(val.GetString(), out var parsed))
                return parsed is >= 1 and <= 4 ? parsed : null;
        }
        return null;
    }

    private static string FormatAllergens(JsonElement productNode)
    {
        var allergens = GetString(productNode, "allergens_tags");
        if (allergens != null)
        {
            // Tags sont du type "en:milk,en:gluten" => "Lait, Gluten"
            return string.Join(", ", allergens.Split(',')
                .Select(a => a.Replace("en:", "").Replace("fr:", "").Trim())
                .Where(a => !string.IsNullOrEmpty(a))
                .Select(a => char.ToUpper(a[0]) + a[1..].Replace("-", " ")));
        }

        // Fallback sur allergens_from_ingredients
        var text = GetString(productNode, "allergens");
        if (!string.IsNullOrEmpty(text))
        {
            return string.Join(", ", text.Split(',')
                .Select(a => a.Replace("en:", "").Replace("fr:", "").Trim())
                .Where(a => !string.IsNullOrEmpty(a))
                .Select(a =>
                {
                    var clean = a.Trim();
                    return clean.Length > 0 ? char.ToUpper(clean[0]) + clean[1..].ToLower() : "";
                }));
        }

        return "";
    }

    private static string GetCategories(JsonElement productNode)
    {
        var cats = GetString(productNode, "categories_tags");
        if (cats != null)
        {
            var formatted = cats.Split(',')
                .Select(c => c.Replace("en:", "").Replace("fr:", "").Replace("-", " ").Trim())
                .Where(c => !string.IsNullOrEmpty(c))
                .Take(3)
                .Select(c => char.ToUpper(c[0]) + c[1..]);
            return string.Join(", ", formatted);
        }

        return GetString(productNode, "categories") ?? "";
    }

    private static bool CheckLabel(JsonElement productNode, params string[] keywords)
    {
        var labels = GetString(productNode, "labels_tags")?.ToLower()
                     ?? GetString(productNode, "labels")?.ToLower()
                     ?? "";
        return keywords.Any(k => labels.Contains(k));
    }

    private static bool CheckPalmOilFree(JsonElement productNode)
    {
        var ingredients = GetString(productNode, "ingredients_analysis_tags")?.ToLower() ?? "";
        if (ingredients.Contains("palm-oil-free")) return true;

        var text = GetString(productNode, "ingredients_text")?.ToLower() ?? "";
        return !text.Contains("huile de palme") && !text.Contains("palm oil");
    }
}
