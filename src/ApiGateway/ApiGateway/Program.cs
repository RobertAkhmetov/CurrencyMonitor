var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddHttpClient()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(c => c.SwaggerDoc("gateway", new() { Title = "CurrencyMonitor Gateway", Version = "1.0" }))
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api-docs/merged.json", "UserService + FinanceService");
        c.SwaggerEndpoint("/swagger/gateway/swagger.json", "Gateway (обзор)");
    });
}

app.MapGet("/api-docs/merged.json", async (IHttpClientFactory httpClientFactory, IConfiguration config, HttpContext context) =>
{
    var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
    var userUrl = config["ReverseProxy:Clusters:user-cluster:Destinations:user-destination:Address"] ?? "http://localhost:5001/";
    var financeUrl = config["ReverseProxy:Clusters:finance-cluster:Destinations:finance-destination:Address"] ?? "http://localhost:5002/";

    var client = httpClientFactory.CreateClient();
    var userTask = client.GetStringAsync($"{userUrl.TrimEnd('/')}/swagger/v1/swagger.json");
    var financeTask = client.GetStringAsync($"{financeUrl.TrimEnd('/')}/swagger/v1/swagger.json");

    string? userJson = null, financeJson = null;
    try { userJson = await userTask; } catch { }
    try { financeJson = await financeTask; } catch { }

    if (string.IsNullOrEmpty(userJson) && string.IsNullOrEmpty(financeJson))
        return Results.Json(new { openapi = "3.0", info = new { title = "CurrencyMonitor", version = "1.0" }, paths = new { } });

    var merged = new Dictionary<string, object?>();
    var paths = new Dictionary<string, object?>();
    var components = new Dictionary<string, object?>();
    var schemas = new Dictionary<string, object?>();

    foreach (var json in new[] { userJson, financeJson })
    {
        if (string.IsNullOrEmpty(json)) continue;
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.TryGetProperty("paths", out var p))
            foreach (var prop in p.EnumerateObject())
                paths[prop.Name] = System.Text.Json.JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
        if (root.TryGetProperty("components", out var c) && c.TryGetProperty("schemas", out var s))
            foreach (var prop in s.EnumerateObject())
                schemas[prop.Name] = System.Text.Json.JsonSerializer.Deserialize<object>(prop.Value.GetRawText());
    }

    components["schemas"] = schemas;
    components["securitySchemes"] = new Dictionary<string, object?>
    {
        ["Bearer"] = new { type = "http", scheme = "bearer", bearerFormat = "JWT" }
    };
    merged["openapi"] = "3.0.0";
    merged["info"] = new { title = "CurrencyMonitor (UserService + FinanceService)", version = "1.0" };
    merged["servers"] = new[] { new { url = baseUrl } };
    merged["paths"] = paths;
    merged["components"] = components;
    merged["security"] = new[] { new Dictionary<string, object?> { ["Bearer"] = Array.Empty<object>() } };

    return Results.Json(merged);
}).ExcludeFromDescription();

app.MapReverseProxy();

app.Run();
