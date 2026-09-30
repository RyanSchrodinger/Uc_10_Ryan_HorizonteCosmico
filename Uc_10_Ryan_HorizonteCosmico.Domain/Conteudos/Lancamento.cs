namespace Uc_10_Ryan_HorizonteCosmico.Domain.Conteudos;

public class Lancamento
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string EmpresaAgencia { get; set; } = string.Empty;
    public string? Foguete { get; set; }
    public string? LocalLancamento { get; set; }
    public DateTime? DataHoraLancamento { get; set; }
    public string? ObjetivoMissao { get; set; }
    public string? ImagemCapa { get; set; }
    public string? CreditoImagem { get; set; }
    public string? LinkTransmissao { get; set; }
    public string StatusLancamento { get; set; } = "Programado";
    public string StatusPublicacao { get; set; } = "Rascunho";
    public bool Destaque { get; set; }
    public string? UsuarioId { get; set; }
}
