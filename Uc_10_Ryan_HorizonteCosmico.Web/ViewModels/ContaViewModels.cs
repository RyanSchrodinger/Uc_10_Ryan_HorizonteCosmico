using System.ComponentModel.DataAnnotations;

namespace Uc_10_Ryan_HorizonteCosmico.Web.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;

    public bool LembrarMe { get; set; }
}

public class CadastroViewModel
{
    [Required, Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6)]
    public string Senha { get; set; } = string.Empty;

    [Required, Compare(nameof(Senha)), DataType(DataType.Password), Display(Name = "Confirmar senha")]
    public string ConfirmarSenha { get; set; } = string.Empty;
}

public class ConfirmarCodigoViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(6, MinimumLength = 6)]
    public string Codigo { get; set; } = string.Empty;
}

public class TrocarEmailViewModel
{
    [Required, EmailAddress, Display(Name = "Novo e-mail")]
    public string NovoEmail { get; set; } = string.Empty;
}

public class EsqueciSenhaViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class RedefinirSenhaViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6), Display(Name = "Nova senha")]
    public string NovaSenha { get; set; } = string.Empty;

    [Required, Compare(nameof(NovaSenha)), DataType(DataType.Password), Display(Name = "Confirmar nova senha")]
    public string ConfirmarNovaSenha { get; set; } = string.Empty;
}


public class EditarPerfilViewModel
{
    [Required, Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password), Display(Name = "Senha atual")]
    public string? SenhaAtual { get; set; }

    [DataType(DataType.Password), MinLength(6), Display(Name = "Nova senha")]
    public string? NovaSenha { get; set; }

    [DataType(DataType.Password), Compare(nameof(NovaSenha)), Display(Name = "Confirmar nova senha")]
    public string? ConfirmarNovaSenha { get; set; }
}
