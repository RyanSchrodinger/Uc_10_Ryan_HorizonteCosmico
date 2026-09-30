using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uc_10_Ryan_HorizonteCosmico.Domain.Conteudos;
using Uc_10_Ryan_HorizonteCosmico.Domain.Usuarios;
using Uc_10_Ryan_HorizonteCosmico.Infrastrucure;
using Uc_10_Ryan_HorizonteCosmico.Web.ViewModels;

namespace Uc_10_Ryan_HorizonteCosmico.Web.Controllers;

public class LancamentosController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public LancamentosController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? busca, string? status)
    {
        var publicadas = _db.Lancamentos
            .AsNoTracking()
            .Where(x => x.StatusPublicacao == "Publicado");

        var statuses = await publicadas
            .Select(x => x.StatusLancamento)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var destaques = await publicadas
            .Where(x => x.Destaque)
            .OrderBy(x => x.DataHoraLancamento ?? DateTime.MaxValue)
            .ThenByDescending(x => x.Id)
            .Take(3)
            .ToListAsync();

        if (destaques.Count < 3)
        {
            var ids = destaques.Select(x => x.Id).ToList();
            var complementos = await publicadas
                .Where(x => !ids.Contains(x.Id))
                .OrderBy(x => x.DataHoraLancamento ?? DateTime.MaxValue)
                .ThenByDescending(x => x.Id)
                .Take(3 - destaques.Count)
                .ToListAsync();
            destaques.AddRange(complementos);
        }

        var query = publicadas.AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
        {
            query = query.Where(x => x.Nome.Contains(busca) || x.EmpresaAgencia.Contains(busca) || x.Descricao.Contains(busca));
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.StatusLancamento == status);
        }

        var resultados = await query
            .OrderBy(x => x.DataHoraLancamento ?? DateTime.MaxValue)
            .ThenByDescending(x => x.Id)
            .ToListAsync();

        return View(new ListaLancamentosViewModel
        {
            Busca = busca,
            Status = status,
            StatusDisponiveis = statuses,
            Destaques = destaques,
            Resultados = resultados
        });
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.Lancamentos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.StatusPublicacao == "Publicado");

        return item is null ? NotFound() : View(item);
    }

    [Authorize(Roles = RolesSistema.Administrador)]
    public async Task<IActionResult> Gerenciar() => View(await _db.Lancamentos.OrderByDescending(x => x.Id).ToListAsync());

    [Authorize(Roles = RolesSistema.Administrador)]
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesSistema.Administrador)]
    public async Task<IActionResult> Create(Lancamento model)
    {
        if (!ModelState.IsValid) return View(model);
        model.UsuarioId = _userManager.GetUserId(User);
        _db.Lancamentos.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Gerenciar));
    }

    [Authorize(Roles = RolesSistema.Administrador)]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Lancamentos.FindAsync(id);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesSistema.Administrador)]
    public async Task<IActionResult> Edit(int id, Lancamento model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);
        var item = await _db.Lancamentos.FindAsync(id);
        if (item is null) return NotFound();
        _db.Entry(item).CurrentValues.SetValues(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Gerenciar));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesSistema.Administrador)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Lancamentos.FindAsync(id);
        if (item is null) return NotFound();
        _db.Lancamentos.Remove(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Gerenciar));
    }
}
