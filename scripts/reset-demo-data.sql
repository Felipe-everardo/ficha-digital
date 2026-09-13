SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DELETE FROM [RequisicoesIdempotentes];
    DELETE FROM [RegistrosAuditoria];
    DELETE FROM [RegistrosTatuagem];
    DELETE FROM [RegistrosPiercing];
    DELETE FROM [RevisoesProfissionais];
    DELETE FROM [AceitesTermoConsentimento];
    DELETE FROM [QuestionariosSaude];
    DELETE FROM [DadosPessoaisFichas];
    DELETE FROM [ConvitesFicha];
    DELETE FROM [Fichas];
    DELETE FROM [Clientes];

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;

SELECT N'Clientes' AS [Tabela], COUNT_BIG(*) AS [Registros]
FROM [Clientes]
UNION ALL
SELECT N'Fichas', COUNT_BIG(*)
FROM [Fichas]
UNION ALL
SELECT N'ConvitesFicha', COUNT_BIG(*)
FROM [ConvitesFicha]
UNION ALL
SELECT N'QuestionariosSaude', COUNT_BIG(*)
FROM [QuestionariosSaude]
UNION ALL
SELECT N'DadosPessoaisFichas', COUNT_BIG(*)
FROM [DadosPessoaisFichas]
UNION ALL
SELECT N'AceitesTermoConsentimento', COUNT_BIG(*)
FROM [AceitesTermoConsentimento]
UNION ALL
SELECT N'RevisoesProfissionais', COUNT_BIG(*)
FROM [RevisoesProfissionais]
UNION ALL
SELECT N'RegistrosTatuagem', COUNT_BIG(*)
FROM [RegistrosTatuagem]
UNION ALL
SELECT N'RegistrosPiercing', COUNT_BIG(*)
FROM [RegistrosPiercing]
UNION ALL
SELECT N'RegistrosAuditoria', COUNT_BIG(*)
FROM [RegistrosAuditoria]
UNION ALL
SELECT N'RequisicoesIdempotentes', COUNT_BIG(*)
FROM [RequisicoesIdempotentes];
