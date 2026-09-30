using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uc_10_Ryan_HorizonteCosmico.Domain.Conteudos;
using Uc_10_Ryan_HorizonteCosmico.Domain.Usuarios;
using Uc_10_Ryan_HorizonteCosmico.Infrastrucure;
using Uc_10_Ryan_HorizonteCosmico.Web.ViewModels;

namespace Uc_10_Ryan_HorizonteCosmico.Web.Controllers;

public class DescobertasController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DescobertasController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? busca, string? categoria)
    {
        var publicadas = _db.Descobertas
            .AsNoTracking()
            .Where(x => x.Status == "Publicado");

        var categorias = await publicadas
            .Where(x => x.Categoria != "")
            .Select(x => x.Categoria)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var destaques = await publicadas
            .Where(x => x.Destaque)
            .OrderByDescending(x => x.DataPublicacao ?? x.DataDescoberta)
            .Take(3)
            .ToListAsync();

        if (destaques.Count < 3)
        {
            var ids = destaques.Select(x => x.Id).ToList();
            var complementos = await publicadas
                .Where(x => !ids.Contains(x.Id))
                .OrderByDescending(x => x.DataPublicacao ?? x.DataDescoberta)
                .Take(3 - destaques.Count)
                .ToListAsync();
            destaques.AddRange(complementos);
        }

        var query = publicadas.AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
        {
            query = query.Where(x => x.Titulo.Contains(busca) || x.Resumo.Contains(busca) || x.Categoria.Contains(busca));
        }
        if (!string.IsNullOrWhiteSpace(categoria))
        {
            query = query.Where(x => x.Categoria == categoria);
        }

        var resultados = await query
            .OrderByDescending(x => x.DataPublicacao ?? x.DataDescoberta)
            .ToListAsync();

        return View(new ListaDescobertasViewModel
        {
            Busca = busca,
            Categoria = categoria,
            Categorias = categorias,
            Destaques = destaques,
            Resultados = resultados
        });
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.Descobertas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.Status == "Publicado");

        return item is null ? NotFound() : View(item);
    }

    [Authorize(Roles = RolesSistema.Administrador)]
    public async Task<IActionResult> Gerenciar() => View(await _db.Descobertas.OrderByDescending(x => x.Id).ToListAsync());

    [Authorize(Roles = RolesSistema.Administrador)]
    public IActionResult Create() => View(new Descoberta { DataDescoberta = DateTime.Today });

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesSistema.Administrador)]
    public async Task<IActionResult> Create(Descoberta model)
    {
        if (!ModelState.IsValid) return View(model);
        model.UsuarioId = _userManager.GetUserId(User);
        if (model.Status == "Publicado") model.DataPublicacao ??= DateTime.UtcNow;
        _db.Descobertas.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Gerenciar));
    }

    [Authorize(Roles = RolesSistema.Administrador)]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Descobertas.FindAsync(id);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesSistema.Administrador)]
    public async Task<IActionResult> Edit(int id, Descoberta model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);
        var item = await _db.Descobertas.FindAsync(id);
        if (item is null) return NotFound();
        _db.Entry(item).CurrentValues.SetValues(model);
        if (item.Status == "Publicado") item.DataPublicacao ??= DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Gerenciar));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesSistema.Administrador)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Descobertas.FindAsync(id);
        if (item is null) return NotFound();
        _db.Descobertas.Remove(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Gerenciar));
    }
}
