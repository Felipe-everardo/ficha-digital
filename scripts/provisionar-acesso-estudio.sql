-- Executar como administrador Entra, somente no banco do piloto.
-- Habilitar primeiro a identidade gerenciada do App Service fichadigital-estudio.
-- Não concede DDL, acesso ao servidor nem acesso ao banco da demonstração.
IF DB_NAME() <> N'FichaDigitalEstudioDb'
    THROW 50000, N'Banco incorreto: esperado FichaDigitalEstudioDb.', 1;

IF DATABASE_PRINCIPAL_ID(N'fichadigital-estudio') IS NULL
    CREATE USER [fichadigital-estudio] FROM EXTERNAL PROVIDER;

IF NOT EXISTS (
    SELECT 1 FROM sys.database_role_members
    WHERE role_principal_id = DATABASE_PRINCIPAL_ID(N'db_datareader')
      AND member_principal_id = DATABASE_PRINCIPAL_ID(N'fichadigital-estudio'))
    ALTER ROLE db_datareader ADD MEMBER [fichadigital-estudio];

IF NOT EXISTS (
    SELECT 1 FROM sys.database_role_members
    WHERE role_principal_id = DATABASE_PRINCIPAL_ID(N'db_datawriter')
      AND member_principal_id = DATABASE_PRINCIPAL_ID(N'fichadigital-estudio'))
    ALTER ROLE db_datawriter ADD MEMBER [fichadigital-estudio];

SELECT DB_NAME() AS Banco, p.name AS Identidade, r.name AS Permissao
FROM sys.database_role_members m
JOIN sys.database_principals p ON p.principal_id = m.member_principal_id
JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
WHERE p.name = N'fichadigital-estudio';
