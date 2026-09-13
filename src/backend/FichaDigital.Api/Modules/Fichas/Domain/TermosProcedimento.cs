namespace FichaDigital.Api.Modules.Fichas.Domain;

public static class TermosProcedimento
{
    public const int VersaoModeloAtual = 1;
    public const int VersaoTermoTatuagemAtual = 1;
    public const int VersaoTermoPiercingAtual = 1;

    public static string ObterTatuagem(string cnpj)
    {
        return $$"""
            TERMO DE AUTORIZAÇÃO DE TATUAGEM
            CNPJ {{cnpj}}

            Declaro estar em pleno gozo de minhas faculdades mentais e psíquicas e autorizo o profissional responsável a realizar o procedimento de tatuagem em meu corpo. Confirmo que o decalque ou desenho free hand e o local da tatuagem foram apresentados e aprovados antes do procedimento e que estou de acordo com a arte, seu tamanho, suas cores e seus detalhes.

            Assumo a responsabilidade de seguir os cuidados pós-procedimento que me foram orientados. Atesto que fui esclarecido sobre o uso de materiais descartáveis no ato da tatuagem, incluindo agulhas e biqueiras, e que esses materiais serão abertos na minha presença.

            Estou ciente de que a tatuagem é um procedimento invasivo que consiste na introdução de pigmentos através da pele. Declaro que todas as informações fornecidas nesta ficha são verdadeiras e completas conforme meu conhecimento atual.

            Estou de acordo com o retorno para revisão da tatuagem e ciente de que o não comparecimento poderá implicar cobrança de valor mínimo para eventual retoque. Também estou ciente de que podem ocorrer falhas em traços ou pintura e de que poderá ser necessário retoque no prazo de 60 dias após a realização da tatuagem, sem custo adicional, conforme avaliação do profissional.

            Autorizo o uso de imagem para divulgação do trabalho do profissional em redes sociais, páginas e sites, conforme previsto nesta ficha.
            """;
    }

    public static string ObterPiercing(string cnpj)
    {
        return $$"""
            TERMO DE AUTORIZAÇÃO DE PIERCING
            CNPJ {{cnpj}}

            Declaro estar em pleno gozo de minhas faculdades mentais e psíquicas e autorizo o profissional responsável a realizar o procedimento de aplicação de piercing em meu corpo. Confirmo que a marcação e o local do piercing foram apresentados e aprovados antes da perfuração.

            Assumo a responsabilidade de seguir os cuidados pós-procedimento que me foram orientados. Atesto que fui esclarecido sobre o uso de materiais descartáveis no ato da perfuração e que esses materiais serão abertos na minha presença.

            Estou ciente de que a aplicação de piercing é um procedimento invasivo que consiste na perfuração e introdução da joia através da pele, mucosa ou outros tecidos corporais, com o objetivo de fixá-la no corpo. Declaro que todas as informações fornecidas nesta ficha são verdadeiras e completas conforme meu conhecimento atual.

            Estou de acordo com o retorno para revisão do piercing após 15 dias da perfuração. Estou ciente de que perdas de peças da joia ou bolinhas não são de responsabilidade do profissional e confirmo que a joia foi apresentada e escolhida por mim antes do procedimento, acompanhada de seu laudo técnico.

            Autorizo o uso de imagem para divulgação do trabalho do profissional em redes sociais, páginas e sites, conforme previsto nesta ficha.
            """;
    }
}
