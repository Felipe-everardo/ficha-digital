using FichaDigital.Api.Modules.Clientes.Domain;
using FichaDigital.Api.Shared.Domain;

namespace FichaDigital.IntegrationTests.Infrastructure;

public static class DadosPessoaisTeste
{
    public static DadosPessoaisInformados Criar(
        string nomeCompleto = "Ana Silva",
        string? nomeSocial = "Ana",
        string? pronomes = "ela/dela",
        DateOnly? dataNascimento = null,
        string celular = "21999999999",
        string? email = "ana@example.com",
        string? instagram = null,
        string? contatoEmergenciaNome = null,
        string? contatoEmergenciaCelular = null)
    {
        return new DadosPessoaisInformados(
            nomeCompleto,
            nomeSocial,
            pronomes,
            estadoCivil: "Solteira",
            dataNascimento ?? new DateOnly(1995, 6, 15),
            cpf: "52998224725",
            celular,
            telefoneAdicional: null,
            email,
            instagram,
            contatoEmergenciaNome,
            contatoEmergenciaCelular,
            cep: "20040002",
            logradouro: "Rua da Assembleia",
            numero: "10",
            complemento: null,
            bairro: "Centro",
            cidade: "Rio de Janeiro",
            estado: "RJ");
    }

    public static DadosPessoaisInformados CriarDe(Cliente cliente)
    {
        return new DadosPessoaisInformados(
            cliente.NomeCompleto!,
            cliente.NomeSocial,
            cliente.Pronomes,
            cliente.EstadoCivil!,
            cliente.DataNascimento!.Value,
            cliente.Cpf!,
            cliente.Celular!,
            cliente.TelefoneAdicional,
            cliente.Email,
            cliente.Instagram,
            cliente.ContatoEmergenciaNome,
            cliente.ContatoEmergenciaCelular,
            cliente.Cep!,
            cliente.Logradouro!,
            cliente.Numero!,
            cliente.Complemento,
            cliente.Bairro!,
            cliente.Cidade!,
            cliente.Estado!);
    }
}
