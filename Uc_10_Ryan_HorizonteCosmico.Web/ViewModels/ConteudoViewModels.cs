using Uc_10_Ryan_HorizonteCosmico.Domain.Conteudos;

namespace Uc_10_Ryan_HorizonteCosmico.Web.ViewModels;

public class ListaDescobertasViewModel
{
    public string? Busca { get; set; }
    public string? Categoria { get; set; }
    public List<string> Categorias { get; set; } = [];
    public List<Descoberta> Destaques { get; set; } = [];
    public List<Descoberta> Resultados { get; set; } = [];
}

public class ListaLancamentosViewModel
{
    public string? Busca { get; set; }
    public string? Status { get; set; }
    public List<string> StatusDisponiveis { get; set; } = [];
    public List<Lancamento> Destaques { get; set; } = [];
    public List<Lancamento> Resultados { get; set; } = [];
}

public class ListaAstrofotografiasViewModel
{
    public string? Busca { get; set; }
    public string? Categoria { get; set; }
    public List<string> Categorias { get; set; } = [];
    public List<Astrofotografia> Destaques { get; set; } = [];
    public List<Astrofotografia> Resultados { get; set; } = [];
}

public class ConteudoRecenteViewModel
{
    public string Tipo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class AdministracaoViewModel
{
    public int Usuarios { get; set; }
    public int Descobertas { get; set; }
    public int Lancamentos { get; set; }
    public int Astrofotografias { get; set; }
    public int ConteudosAtivos { get; set; }
    public int Rascunhos { get; set; }
    public List<ConteudoRecenteViewModel> Recentes { get; set; } = [];
}
