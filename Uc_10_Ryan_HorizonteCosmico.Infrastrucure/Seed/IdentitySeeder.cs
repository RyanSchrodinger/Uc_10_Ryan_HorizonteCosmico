using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Uc_10_Ryan_HorizonteCosmico.Domain.Usuarios;

namespace Uc_10_Ryan_HorizonteCosmico.Infrastrucure.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        string[] roles = [RolesSistema.Administrador, RolesSistema.Cliente];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var section = configuration.GetSection("AdminSeeder");
        var email = section["Email"] ?? "admin@horizontecosmico.com";
        var password = section["Password"] ?? "Admin@12345";
        var nome = section["Nome"] ?? "Administrador";

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                NomeCompleto = nome,
                Ativo = true,
                StatusCadastro = StatusCadastro.Confirmado,
                DataCadastro = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(admin, RolesSistema.Administrador))
            await userManager.AddToRoleAsync(admin, RolesSistema.Administrador);
    }
}
