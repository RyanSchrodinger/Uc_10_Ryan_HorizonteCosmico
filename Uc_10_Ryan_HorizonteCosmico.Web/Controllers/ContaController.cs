using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uc_10_Ryan_HorizonteCosmico.Domain.Usuarios;
using Uc_10_Ryan_HorizonteCosmico.Infrastrucure;
using Uc_10_Ryan_HorizonteCosmico.Infrastrucure.Email;
using Uc_10_Ryan_HorizonteCosmico.Web.ViewModels;

namespace Uc_10_Ryan_HorizonteCosmico.Web.Controllers;

public class ContaController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _db;
    private readonly IEmailSender _emailSender;

    public ContaController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext db,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _emailSender = emailSender;
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(MeuPerfil));

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(MeuPerfil));

        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            return View(model);
        }

        if (!user.EmailConfirmed)
        {
            TempData["EmailPendente"] = user.Email;
            return RedirectToAction(nameof(CadastroPendente));
        }

        if (!user.Ativo)
        {
            ModelState.AddModelError(string.Empty, "Esta conta está desativada.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Senha, model.LembrarMe, lockoutOnFailure: false);
        if (result.Succeeded)
            return LocalRedirect(returnUrl ?? Url.Action("Index", "Home")!);

        ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult Cadastro()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(MeuPerfil));

        return View();
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastro(CadastroViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(MeuPerfil));

        if (!ModelState.IsValid) return View(model);

        var existing = await _userManager.FindByEmailAsync(model.Email);
        if (existing is not null)
        {
            if (!existing.EmailConfirmed)
            {
                TempData["EmailPendente"] = existing.Email;
                return RedirectToAction(nameof(CadastroPendente));
            }

            ModelState.AddModelError(nameof(model.Email), "Este e-mail já está cadastrado.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            NomeCompleto = model.NomeCompleto.Trim(),
            DataCadastro = DateTime.UtcNow,
            Ativo = true,
            StatusCadastro = StatusCadastro.Pendente,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, model.Senha);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        var cliente = new Cliente
        {
            UsuarioId = user.Id,
            Biografia = string.Empty
        };
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();
        await _userManager.AddToRoleAsync(user, RolesSistema.Cliente);

        await EnviarCodigoAsync(user);
        TempData["EmailPendente"] = user.Email;
        return RedirectToAction(nameof(CadastroPendente));
    }

    [AllowAnonymous]
    public IActionResult CadastroPendente()
    {
        var email = TempData["EmailPendente"]?.ToString();
        if (string.IsNullOrWhiteSpace(email)) return RedirectToAction(nameof(Login));
        ViewBag.Email = email;
        return View();
    }

    [AllowAnonymous]
    public IActionResult ConfirmarCodigo(string? email)
    {
        return View(new ConfirmarCodigoViewModel { Email = email ?? string.Empty });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarCodigo(ConfirmarCodigoViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null || user.EmailConfirmed)
        {
            ModelState.AddModelError(string.Empty, "Código inválido.");
            return View(model);
        }

        var codigo = await _db.CodigosConfirmacaoEmail
            .Where(x => x.UsuarioId == user.Id && !x.Utilizado && x.DataExpiracao > DateTime.UtcNow)
            .OrderByDescending(x => x.DataCriacao)
            .FirstOrDefaultAsync();

        if (codigo is null || codigo.Codigo != model.Codigo.Trim())
        {
            ModelState.AddModelError(nameof(model.Codigo), "Código inválido ou expirado.");
            return View(model);
        }

        codigo.Utilizado = true;
        user.EmailConfirmed = true;
        user.StatusCadastro = StatusCadastro.Confirmado;
        await _userManager.UpdateAsync(user);
        await _db.SaveChangesAsync();

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReenviarCodigo(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is not null && !user.EmailConfirmed)
            await EnviarCodigoAsync(user);

        TempData["EmailPendente"] = email;
        TempData["Mensagem"] = "Se o cadastro estiver pendente, um novo código foi enviado.";
        return RedirectToAction(nameof(CadastroPendente));
    }

    [AllowAnonymous]
    public IActionResult TrocarEmail(string? email)
    {
        ViewBag.EmailAtual = email ?? string.Empty;
        return View(new TrocarEmailViewModel());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> TrocarEmail(string emailAtual, TrocarEmailViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.EmailAtual = emailAtual;
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(emailAtual);
        if (user is null || user.EmailConfirmed) return RedirectToAction(nameof(Login));

        var existing = await _userManager.FindByEmailAsync(model.NovoEmail);
        if (existing is not null && existing.Id != user.Id)
        {
            ModelState.AddModelError(nameof(model.NovoEmail), "Este e-mail já está em uso.");
            ViewBag.EmailAtual = emailAtual;
            return View(model);
        }

        var novoEmail = model.NovoEmail.Trim();
        var emailResult = await _userManager.SetEmailAsync(user, novoEmail);
        if (!emailResult.Succeeded)
        {
            AddIdentityErrors(emailResult);
            ViewBag.EmailAtual = emailAtual;
            return View(model);
        }

        await _userManager.SetUserNameAsync(user, novoEmail);
        user.EmailConfirmed = false;
        user.StatusCadastro = StatusCadastro.Pendente;
        await _userManager.UpdateAsync(user);
        await EnviarCodigoAsync(user);

        TempData["EmailPendente"] = user.Email;
        return RedirectToAction(nameof(CadastroPendente));
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirCadastro(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is not null && !user.EmailConfirmed)
        {
            await _userManager.DeleteAsync(user);
        }

        return RedirectToAction(nameof(Cadastro));
    }

    [AllowAnonymous]
    public IActionResult EsqueciSenha() => View();

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> EsqueciSenha(EsqueciSenhaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is not null && user.EmailConfirmed)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var url = Url.Action(nameof(RedefinirSenha), "Conta", new { userId = user.Id, token }, Request.Scheme);
            await _emailSender.SendAsync(user.Email!, "Redefinição de senha - Horizonte Cósmico",
                $"<p>Olá, {user.NomeCompleto}.</p><p>Acesse o link para criar uma nova senha:</p><p><a href=\"{url}\">Redefinir senha</a></p>");
        }

        TempData["Mensagem"] = "Se o e-mail estiver cadastrado, enviamos as instruções de recuperação.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult RedefinirSenha(string userId, string token) =>
        View(new RedefinirSenhaViewModel { UserId = userId, Token = token });

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> RedefinirSenha(RedefinirSenhaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null) return RedirectToAction(nameof(Login));

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NovaSenha);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        TempData["Mensagem"] = "Senha alterada com sucesso.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    public async Task<IActionResult> MeuPerfil()
    {
        var user = await _userManager.GetUserAsync(User);
        return user is null ? Challenge() : View(user);
    }

    [AllowAnonymous]
    public IActionResult AcessoNegado() => View();

    [Authorize]
    public async Task<IActionResult> EditarPerfil()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        return View(new EditarPerfilViewModel
        {
            NomeCompleto = user.NomeCompleto,
            Email = user.Email ?? string.Empty
        });
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        if (!ModelState.IsValid) return View(model);

        user.NomeCompleto = model.NomeCompleto.Trim();

        if (!string.Equals(user.Email, model.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            var emailEmUso = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (emailEmUso is not null && emailEmUso.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Este e-mail já está em uso.");
                return View(model);
            }

            var emailResult = await _userManager.SetEmailAsync(user, model.Email.Trim());
            if (!emailResult.Succeeded)
            {
                AddIdentityErrors(emailResult);
                return View(model);
            }

            await _userManager.SetUserNameAsync(user, model.Email.Trim());
            user.EmailConfirmed = false;
            user.StatusCadastro = StatusCadastro.Pendente;
            await _userManager.UpdateAsync(user);
            await _db.SaveChangesAsync();
            await EnviarCodigoAsync(user);
            await _signInManager.SignOutAsync();

            TempData["EmailPendente"] = user.Email;
            return RedirectToAction(nameof(CadastroPendente));
        }

        if (!string.IsNullOrWhiteSpace(model.NovaSenha))
        {
            if (string.IsNullOrWhiteSpace(model.SenhaAtual))
            {
                ModelState.AddModelError(nameof(model.SenhaAtual), "Informe sua senha atual.");
                return View(model);
            }

            var passwordResult = await _userManager.ChangePasswordAsync(user, model.SenhaAtual, model.NovaSenha);
            if (!passwordResult.Succeeded)
            {
                AddIdentityErrors(passwordResult);
                return View(model);
            }
        }

        await _userManager.UpdateAsync(user);
        TempData["Mensagem"] = "Seu perfil foi atualizado com sucesso.";
        return RedirectToAction(nameof(MeuPerfil));
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    private async Task EnviarCodigoAsync(ApplicationUser user)
    {
        var anteriores = await _db.CodigosConfirmacaoEmail
            .Where(x => x.UsuarioId == user.Id && !x.Utilizado)
            .ToListAsync();
        foreach (var item in anteriores) item.Utilizado = true;

        var codigo = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        _db.CodigosConfirmacaoEmail.Add(new CodigoConfirmacaoEmail
        {
            UsuarioId = user.Id,
            Codigo = codigo,
            DataCriacao = DateTime.UtcNow,
            DataExpiracao = DateTime.UtcNow.AddMinutes(15)
        });
        await _db.SaveChangesAsync();

        await _emailSender.SendAsync(user.Email!, "Código de confirmação - Horizonte Cósmico",
            $"<p>Olá, {user.NomeCompleto}.</p><p>Seu código de confirmação é:</p><h2>{codigo}</h2><p>Ele expira em 15 minutos.</p>");
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
    }
}
