using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Models
{
    public class SaleProduct
    {
        [Key]
        public int SaleProductId { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Este campo é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "a quantidade deve ser maior que zero")]
        public int Quantity { get; set; }
        public string? title { get; set; }
    }
}