using System.ComponentModel.DataAnnotations;

namespace Api.Contracts;

public sealed class SaveProductRequest
{
    [Required(ErrorMessage = "SKU є обов'язковим.")]
    [StringLength(50, ErrorMessage = "SKU не може перевищувати 50 символів.")]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "Назва товару є обов'язковою.")]
    [StringLength(150, ErrorMessage = "Назва не може перевищувати 150 символів.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Одиниця вимірювання є обов'язковою.")]
    [StringLength(20, ErrorMessage = "Одиниця вимірювання не може перевищувати 20 символів.")]
    public string Unit { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Кількість не може бути від'ємною.")]
    public int Quantity { get; set; }
}