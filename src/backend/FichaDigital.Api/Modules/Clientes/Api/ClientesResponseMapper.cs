using FichaDigital.Api.Modules.Clientes.Application;

namespace FichaDigital.Api.Modules.Clientes.Api;

internal static class ClientesResponseMapper
{
    public static ClienteCriadoResponse ToResponse(this ClienteCriado cliente)
    {
        return new ClienteCriadoResponse(
            cliente.Id,
            cliente.NomeParaExibicao,
            cliente.CriadoEmUtc);
    }

    public static ClientesPaginadosResponse ToResponse(
        this PaginaClientesConsultada pagina)
    {
        return new ClientesPaginadosResponse(
            pagina.Itens.Select(ToResumoResponse).ToList(),
            pagina.Pagina,
            pagina.TamanhoPagina,
            pagina.TotalItens,
            pagina.TotalPaginas);
    }

    public static ClienteDetalheResponse ToResponse(
        this DetalheClienteConsultado cliente)
    {
        return new ClienteDetalheResponse(
            cliente.Id,
            cliente.NomeReferencia,
            cliente.NomeCompleto,
            cliente.NomeSocial,
            cliente.NomeParaExibicao,
            cliente.Pronomes,
            cliente.EstadoCivil,
            cliente.DataNascimento,
            cliente.Cpf,
            cliente.Celular,
            cliente.TelefoneAdicional,
            cliente.Email,
            cliente.Instagram,
            cliente.ContatoEmergenciaNome,
            cliente.ContatoEmergenciaCelular,
            cliente.Cep,
            cliente.Logradouro,
            cliente.Numero,
            cliente.Complemento,
            cliente.Bairro,
            cliente.Cidade,
            cliente.Estado,
            cliente.DadosPessoaisPreenchidosEmUtc,
            cliente.CriadoEmUtc,
            cliente.Fichas.Select(ToFichaResponse).ToList());
    }

    private static ClienteResumoResponse ToResumoResponse(
        ClienteConsultado cliente)
    {
        return new ClienteResumoResponse(
            cliente.Id,
            cliente.NomeReferencia,
            cliente.NomeCompleto,
            cliente.NomeParaExibicao,
            cliente.Pronomes,
            cliente.Celular,
            cliente.Email,
            cliente.Instagram,
            cliente.CriadoEmUtc,
            cliente.UltimaFicha is null
                ? null
                : ToFichaResponse(cliente.UltimaFicha));
    }

    private static FichaClienteResumoResponse ToFichaResponse(
        FichaClienteConsultada ficha)
    {
        return new FichaClienteResumoResponse(
            ficha.Id,
            ficha.Status,
            ficha.TipoProcedimento,
            ficha.ProfissionalResponsavelId,
            ficha.ProfissionalResponsavelNome,
            ficha.CriadaEmUtc);
    }
}
