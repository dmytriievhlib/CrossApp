using Core.Domain;
using Core.Dto;

namespace Core.Import;

public static class ProductEntityImporter
{
    public static EntityImportResult<Product> FromImportResult(
        ImportResult<ProductDto> result)
    {
        var products = new List<Product>();
        var errors = new List<string>(result.Errors);

        for (int i = 0; i < result.Items.Count; i++)
        {
            ProductDto dto = result.Items[i];

            try
            {
                Product product = Product.FromDto(dto);
                products.Add(product);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                errors.Add(
                    $"елемент {i + 1} ({dto.Id}): {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                errors.Add(
                    $"елемент {i + 1} ({dto.Id}): {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                errors.Add(
                    $"елемент {i + 1} ({dto.Id}): {ex.Message}");
            }
        }

        return new EntityImportResult<Product>(
            products,
            errors);
    }
}