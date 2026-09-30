using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Uc_10_Ryan_HorizonteCosmico.Domain.Conteudos;
using Uc_10_Ryan_HorizonteCosmico.Domain.Usuarios;

namespace Uc_10_Ryan_HorizonteCosmico.Infrastrucure;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<CodigoConfirmacaoEmail> CodigosConfirmacaoEmail => Set<CodigoConfirmacaoEmail>();
    public DbSet<Descoberta> Descobertas => Set<Descoberta>();
    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();
    public DbSet<Astrofotografia> Astrofotografias => Set<Astrofotografia>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entidade =>
        {
            entidade.Property(x => x.NomeCompleto).HasMaxLength(120).IsRequired();
            entidade.Property(x => x.StatusCadastro).HasConversion<string>().HasMaxLength(20);
            entidade.Property(x => x.Ativo).HasDefaultValue(true);
        });

        builder.Entity<Cliente>(entidade =>
        {
            entidade.HasKey(x => x.UsuarioId);
            entidade.HasOne(x => x.Usuario).WithOne(x => x.Cliente)
                .HasForeignKey<Cliente>(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
            entidade.Property(x => x.Biografia).HasMaxLength(500).IsRequired();
            entidade.Property(x => x.FotoPerfil).HasMaxLength(500);
        });

        builder.Entity<CodigoConfirmacaoEmail>(entidade =>
        {
            entidade.HasKey(x => x.Id);
            entidade.Property(x => x.UsuarioId).IsRequired();
            entidade.Property(x => x.Codigo).HasMaxLength(6).IsRequired();
            entidade.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            entidade.HasIndex(x => new { x.UsuarioId, x.Codigo });
        });

        builder.Entity<Descoberta>(entidade => ConfigurarConteudo(entidade));
        builder.Entity<Lancamento>(entidade => ConfigurarConteudo(entidade));
        builder.Entity<Astrofotografia>(entidade => ConfigurarConteudo(entidade));
    }

    private static void ConfigurarConteudo<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entidade)
        where TEntity : class
    {
        entidade.HasKey("Id");
    }
}
