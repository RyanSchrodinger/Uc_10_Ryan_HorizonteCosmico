namespace Uc_10_Ryan_HorizonteCosmico.Domain.Conteudos;

public class Descoberta
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Resumo { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public DateTime DataDescoberta { get; set; }
    public DateTime? DataPublicacao { get; set; }
    public string? ImagemCapa { get; set; }
    public string? CreditoImagem { get; set; }
    public string? Fonte { get; set; }
    public string? LinkFonte { get; set; }
    public string Status { get; set; } = "Rascunho";
    public bool Destaque { get; set; }
    public string? UsuarioId { get; set; }
}
