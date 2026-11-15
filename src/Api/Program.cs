using Api.Exceptions;
using Api.Contracts;
using Api.Validation;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Services;
using Core.Storage;
using FluentValidation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ProblemDetails для стандартизованих повідомлень про помилки.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
// OpenAPI.
builder.Services.AddOpenApi();

// Реєстрація сховища та сервісу.
builder.Services.AddSingleton<ICatalogStore, InMemoryCatalogStore>();
builder.Services.AddScoped<CatalogService>();

// Реєстрація FluentValidation.
builder.Services.AddScoped<
    IValidator<SaveProductRequest>,
    SaveProductRequestValidator>();

var app = builder.Build();

// Обробка непередбачених помилок.
app.UseExceptionHandler();

// OpenAPI та Scalar доступні в Development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Перевірка працездатності API.
app.MapGet("/", () => Results.Ok(new
{
    Application = "CrossApp API",
    Status = "Running"
}));

// GET /api/products — отримати товари з пагінацією та пошуком.
app.MapGet("/api/products", (
    int page,
    int pageSize,
    string? search,
    CatalogService service) =>
{
    if (page < 1)
        return Results.BadRequest(new
        {
            Error = "Номер сторінки має бути більшим за 0."
        });

    if (pageSize < 1 || pageSize > 100)
        return Results.BadRequest(new
        {
            Error = "Розмір сторінки має бути від 1 до 100."
        });

    IEnumerable<Product> products = service.All();

    if (!string.IsNullOrWhiteSpace(search))
    {
        products = products.Where(product =>
            product.Name.Contains(
                search,
                StringComparison.OrdinalIgnoreCase) ||
            product.Sku.Contains(
                search,
                StringComparison.OrdinalIgnoreCase));
    }

    int total = products.Count();

    var items = products
        .OrderBy(product => product.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(product => product.ToDto())
        .ToList();

    return Results.Ok(new
    {
        Items = items,
        Total = total,
        Page = page,
        PageSize = pageSize
    });
})
.WithName("GetProducts")
.WithSummary("Отримати сторінку товарів із пошуком");

// GET /api/products/{id} — отримати товар за ID.
app.MapGet("/api/products/{id}", (
    string id,
    CatalogService service) =>
{
    Product? product = service.Find(id);

    return product is null
        ? Results.NotFound()
        : Results.Ok(product.ToDto());
})
.WithName("GetProductById")
.WithSummary("Отримати товар за ідентифікатором");

// POST /api/products — створити товар.
app.MapPost("/api/products", (
    SaveProductRequest request,
    CatalogService service) =>
{
    Product product = service.Add(
        request.Sku,
        request.Name,
        request.Unit,
        request.Quantity);

    ProductDto dto = product.ToDto();

    return Results.Created(
        $"/api/products/{dto.Id}",
        dto);
})
.WithName("CreateProduct")
.WithSummary("Створити товар")
.AddEndpointFilter<ValidationFilter<SaveProductRequest>>();

// PUT /api/products/{id} — оновити товар.
app.MapPut("/api/products/{id}", (
    string id,
    SaveProductRequest request,
    ICatalogStore store) =>
{
    Product? existing = store.GetById(id);

    if (existing is null)
        return Results.NotFound();

    Product updated = Product.Create(
        id,
        request.Sku,
        request.Name,
        request.Unit,
        request.Quantity);

    store.Update(updated);

    return Results.Ok(updated.ToDto());
})
.WithName("UpdateProduct")
.WithSummary("Оновити товар")
.AddEndpointFilter<ValidationFilter<SaveProductRequest>>();

// DELETE /api/products/{id} — видалити товар.
app.MapDelete("/api/products/{id}", (
    string id,
    CatalogService service) =>
{
    bool removed = service.Remove(id);

    return removed
        ? Results.NoContent()
        : Results.NotFound();
})
.WithName("DeleteProduct")
.WithSummary("Видалити товар");

app.Run();

// Дозволяє використовувати WebApplicationFactory в інтеграційних тестах.
public partial class Program;
