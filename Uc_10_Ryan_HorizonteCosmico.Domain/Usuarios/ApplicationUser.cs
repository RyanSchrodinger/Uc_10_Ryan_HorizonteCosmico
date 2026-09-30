using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uc_10_Ryan_HorizonteCosmico.Domain.Usuarios;

namespace Uc_10_Ryan_HorizonteCosmico.Domain.Usuarios
{
    public class ApplicationUser : IdentityUser // O applicationUser, será um usuário do IdentityUser, que é a classe base do ASP.NET Core Identity para representar um usuário no sistema.
    //Porém será possível implementar propriedades específicas para o usuário, alem das que já vem com o IdentityUser. Possibilitando que nao fique preso as propriedades do identity
    {
        // Como é possível ver, coloquei algumas propriedades, que são específica para o meu projeto
        // propriedades essas que não se encontram na classe IdentityUser

        public string NomeCompleto { get; set; } = string.Empty; // string.Empty é para não deixar nulo, e sim vazio, vulgo " "
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow; // Data de cadastro do usuário, que vai ser preenchida automaticamente com a data atual

        public bool Ativo { get; set; } = true; // Já recebe true por padrão

        public StatusCadastro StatusCadastro { get; set; } = StatusCadastro.Pendente; // Aqui estamos utilizando o enum StatusCadastro, que criamos para representar o status do cadastro do usuário. O status inicial é Pendente, pois quando o usuário se cadastra, ele ainda não foi aprovado ou recusado.

        public Cliente? Cliente { get; set; } //Aqui fazemos a associação entre o ApplicationUser e o Cliente
        // a relação fica assim, ApplicationUser 1 - 0.1 Cliente. Isso acontece pois todo cliente pertence a exatamente um usuario
        // enquanto um usuario pode nao ter um cleinte associado a ele, ou seja, um usuario pode ser um administrador, e nesse caso não terá um cliente associado a ele.
    }
}




    

    //public Cliente? Cliente { get; set; } // Indica que o usuário pode ter um cliente associado, mas não é obrigatório. O ponto de interrogação indica que a propriedade pode ser nula.
    //                                      // Isso acontece, pois nem todo usuário será um cliente, mas todo cliente será um usuário. Por isso a relação conceitual é de Application 1 -- 0 ou 1 Cliente
    //                                      // Um cliente pertence a exatamente um usuário, mas um usuário pode não estar associado a nenhum cliente 

