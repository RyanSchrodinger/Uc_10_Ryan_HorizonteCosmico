using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uc_10_Ryan_HorizonteCosmico.Domain.Usuarios;
using Uc_10_Ryan_HorizonteCosmico.Infrastrucure;
using Uc_10_Ryan_HorizonteCosmico.Web.ViewModels;

namespace Uc_10_Ryan_HorizonteCosmico.Web.Controllers;

[Authorize(Roles = RolesSistema.Administrador)]
public class AdministracaoController : Controller
{
    private readonly ApplicationDbContext _db;
    public AdministracaoController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var descobertas = await _db.Descobertas.AsNoTracking().ToListAsync();
        var lancamentos = await _db.Lancamentos.AsNoTracking().ToListAsync();
        var fotografias = await _db.Astrofotografias.AsNoTracking().ToListAsync();

        var recentes = new List<ConteudoRecenteViewModel>();
        recentes.AddRange(descobertas.Select(x => new ConteudoRecenteViewModel
        {
            Tipo = "Descoberta",
            Titulo = x.Titulo,
            Data = x.DataPublicacao ?? x.DataDescoberta,
            Url = Url.Action("Edit", "Descobertas", new { id = x.Id }) ?? "#"
        }));
        recentes.AddRange(lancamentos.Select(x => new ConteudoRecenteViewModel
        {
            Tipo = "Lançamento",
            Titulo = x.Nome,
            Data = x.DataHoraLancamento ?? DateTime.MinValue,
            Url = Url.Action("Edit", "Lancamentos", new { id = x.Id }) ?? "#"
        }));
        recentes.AddRange(fotografias.Select(x => new ConteudoRecenteViewModel
        {
            Tipo = "Fotografia",
            Titulo = x.Titulo,
            Data = x.DataRegistro ?? DateTime.MinValue,
            Url = Url.Action("Edit", "Astrofotografias", new { id = x.Id }) ?? "#"
        }));

        return View(new AdministracaoViewModel
        {
            Usuarios = await _db.Users.CountAsync(),
            Descobertas = descobertas.Count,
            Lancamentos = lancamentos.Count,
            Astrofotografias = fotografias.Count,
            ConteudosAtivos = descobertas.Count(x => x.Status == "Publicado")
                + lancamentos.Count(x => x.StatusPublicacao == "Publicado")
                + fotografias.Count(x => x.Status == "Publicado"),
            Rascunhos = descobertas.Count(x => x.Status == "Rascunho")
                + lancamentos.Count(x => x.StatusPublicacao == "Rascunho")
                + fotografias.Count(x => x.Status == "Rascunho"),
            Recentes = recentes.OrderByDescending(x => x.Data).Take(8).ToList()
        });
    }
}
