namespace FichaDigital.Api.Modules.Fichas.Application;

public interface IGeradorTokenConvite
{
    TokenConviteGerado Gerar();

    string CalcularHash(string tokenOriginal);
}
