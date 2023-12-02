using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Models
{

    public class Sale
    {
        public Sale()
        {
            SaleProducts = new List<SaleProduct>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório")]
        public int ClienteId { get; set; }

        [Precision(18, 2)]
        public decimal? Value { get; set; }

        [DataType(DataType.Date)]
        public DateTime SellDate { get; set; }
        public Client? Cliente { get; set; }
        public List<SaleProduct> SaleProducts { get; set; }
    }
}