namespace FichaDigital.Api.Modules.Fichas.Domain;

public static class TermoConsentimentoAtual
{
    public const int Versao = 2;

    public const string Conteudo = """
        ATENÇÃO: TERMO PROVISÓRIO PARA DESENVOLVIMENTO E TESTES.

        Ao registrar o aceite, declaro que:

        1. estou utilizando somente dados fictícios neste ambiente de testes;
        2. revisei as informações fornecidas no questionário;
        3. compreendo que o sistema registrará uma cópia exata dos dados pessoais e das
           respostas de saúde confirmados, do procedimento, do profissional responsável,
           da versão e do conteúdo deste termo, do meu nome declarado e do momento do aceite;
        4. compreendo que dados técnicos mínimos do acesso, como endereço IP e identificação
           do navegador, poderão ser registrados para segurança e auditoria;
        5. compreendo que este registro ainda não constitui assinatura digital certificada.
        """;
}
