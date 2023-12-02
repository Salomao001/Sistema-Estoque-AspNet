using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório")]
        [MaxLength(100, ErrorMessage = "Este campo deve conter entre 3 e 100 caracteres")]
        [MinLength(3, ErrorMessage = "Este campo deve conter entre 3 e 100 caracteres")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório")]
        [MaxLength(14)]
        [RegularExpression("[0-9][0-9][0-9].[0-9][0-9][0-9].[0-9][0-9][0-9]-[0-9][0-9]$", ErrorMessage = "Ex: (001.001.001-01)")]
        public string? Cpf { get; set; }
        public bool IsDeleted { get; set; }
    }
}