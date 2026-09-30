using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uc_10_Ryan_HorizonteCosmico.Domain.Usuarios
{
    public class Cliente
    {
        [Key]
        public string UsuarioId { get; set; } = string.Empty;

        public DateOnly DataNascimento { get; set; }    

        public string? FotoPerfil { get; set; } 

        [Required]
        [MaxLength(500)]
        public string Biografia { get; set; } = string.Empty;

        public ApplicationUser Usuario { get; set; } = null!; // Aqui estamos fazendo a associação entre o Cliente e o ApplicationUser, ou seja, um cliente pertence a exatamente um usuário.

    }
}
