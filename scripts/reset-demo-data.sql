SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DELETE FROM [Despesas];
    DELETE FROM [Atendimentos];
    DELETE FROM [AceitesTermoConsentimento];
    DELETE FROM [QuestionariosSaude];
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
SELECT N'Atendimentos', COUNT_BIG(*)
FROM [Atendimentos]
UNION ALL
SELECT N'Despesas', COUNT_BIG(*)
FROM [Despesas]
UNION ALL
SELECT N'ConvitesFicha', COUNT_BIG(*)
FROM [ConvitesFicha]
UNION ALL
SELECT N'QuestionariosSaude', COUNT_BIG(*)
FROM [QuestionariosSaude]
UNION ALL
SELECT N'AceitesTermoConsentimento', COUNT_BIG(*)
FROM [AceitesTermoConsentimento];
