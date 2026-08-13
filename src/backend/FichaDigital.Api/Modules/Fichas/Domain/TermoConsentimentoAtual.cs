namespace FichaDigital.Api.Modules.Fichas.Domain;

public static class TermoConsentimentoAtual
{
    public const int Versao = 1;

    public const string Conteudo = """
        ATENÇÃO: TERMO PROVISÓRIO PARA DESENVOLVIMENTO E TESTES.

        Este conteúdo existe exclusivamente para demonstrar o fluxo eletrônico
        do projeto Ficha Digital e não foi revisado juridicamente.

        Ao registrar o aceite, declaro que:

        1. estou utilizando somente dados fictícios neste ambiente de testes;
        2. revisei as informações fornecidas no questionário;
        3. compreendo que o sistema registrará a versão e o conteúdo exato deste termo,
           meu nome declarado e o momento do aceite;
        4. compreendo que este registro não constitui assinatura digital certificada.

        NÃO UTILIZAR ESTE TEXTO COM CLIENTES REAIS.
        """;
}
