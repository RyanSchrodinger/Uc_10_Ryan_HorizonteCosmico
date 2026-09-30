namespace Uc_10_Ryan_HorizonteCosmico.Domain.Conteudos;

public class Astrofotografia
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Imagem { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string? CreditoImagem { get; set; }
    public string? LocalRegistro { get; set; }
    public DateTime? DataRegistro { get; set; }
    public string? Equipamento { get; set; }
    public string? Link { get; set; }
    public DateTime? DataPublicacao { get; set; }
    public string Status { get; set; } = "Rascunho";
    public bool Destaque { get; set; }
    public string? UsuarioId { get; set; }
}
