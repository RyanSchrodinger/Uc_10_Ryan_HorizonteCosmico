namespace Uc_10_Ryan_HorizonteCosmico.Domain.Usuarios;

public class CodigoConfirmacaoEmail
{
    public int Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime DataExpiracao { get; set; }
    public bool Utilizado { get; set; }
    public ApplicationUser Usuario { get; set; } = null!;
}
