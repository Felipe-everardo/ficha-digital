SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Hoje date = CAST(SYSUTCDATETIME() AS date);
DECLARE @ContaEstudio uniqueidentifier = '10000000-0000-0000-0000-000000000001';

BEGIN TRY
    BEGIN TRANSACTION;

    -- Preserva as contas profissionais existentes e substitui somente os
    -- dados fictícios de clientes e fichas.
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

    -- Remove apenas as antigas contas fictícias de equipe, substituídas pela
    -- conta universal do estúdio.
    DELETE FROM [AspNetUserClaims]
    WHERE [UserId] IN (
        '10000000-0000-0000-0000-000000000002',
        '10000000-0000-0000-0000-000000000003');
    DELETE FROM [AspNetUsers]
    WHERE [Id] IN (
        '10000000-0000-0000-0000-000000000002',
        '10000000-0000-0000-0000-000000000003');

    IF EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = @ContaEstudio)
    BEGIN
        UPDATE [AspNetUsers]
        SET [NomeCompleto] = N'Conta do Manuscrito Studio'
        WHERE [Id] = @ContaEstudio;
    END
    ELSE
    BEGIN
        INSERT INTO [AspNetUsers] (
            [Id], [NomeCompleto], [UserName],
            [NormalizedUserName], [Email], [NormalizedEmail],
            [EmailConfirmed], [PasswordHash], [SecurityStamp],
            [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed],
            [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled],
            [AccessFailedCount])
        VALUES (
            @ContaEstudio, N'Conta do Manuscrito Studio',
            N'estudio.demo@manuscrito.local',
            N'ESTUDIO.DEMO@MANUSCRITO.LOCAL', N'estudio.demo@manuscrito.local',
            N'ESTUDIO.DEMO@MANUSCRITO.LOCAL', 0, NULL,
            LOWER(CONVERT(varchar(36), NEWID())),
            LOWER(CONVERT(varchar(36), NEWID())), NULL, 0, 0, NULL, 1, 0);
    END;

    INSERT INTO [Clientes] (
        [Id], [NomeReferencia], [NomeCompleto], [NomeSocial], [Pronomes],
        [DataNascimento], [Celular], [Email], [Instagram],
        [ContatoEmergenciaNome], [ContatoEmergenciaCelular],
        [DadosPessoaisPreenchidosEmUtc], [CriadoEmUtc])
    VALUES
        ('20000000-0000-0000-0000-000000000001', N'Ana', N'Ana Carolina Souza', NULL, N'ela/dela', '1994-03-18', N'(11) 90000-0001', N'ana.souza@example.com', N'@anasouza.demo', N'Carlos Souza', N'(11) 90000-1001', DATEADD(day, -720, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00')), DATEADD(day, -721, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00'))),
        ('20000000-0000-0000-0000-000000000002', N'Bruno', N'Bruno Henrique Lima', NULL, N'ele/dele', '1988-11-02', N'(21) 90000-0002', N'bruno.lima@example.com', N'@brunolima.demo', N'Paula Lima', N'(21) 90000-1002', DATEADD(day, -90, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00')), DATEADD(day, -91, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00'))),
        ('20000000-0000-0000-0000-000000000003', N'Cami', N'Camila Nascimento', N'Cami', N'ela/dela', '2001-06-27', N'(31) 90000-0003', N'cami.nascimento@example.com', N'@cami.ink.demo', N'Lucas Nascimento', N'(31) 90000-1003', DATEADD(day, -60, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00')), DATEADD(day, -61, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00'))),
        ('20000000-0000-0000-0000-000000000004', N'Daniel', N'Daniel Rocha', NULL, N'ele/dele', '1997-01-14', N'(41) 90000-0004', N'daniel.rocha@example.com', NULL, N'Mariana Rocha', N'(41) 90000-1004', DATEADD(day, -12, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00')), DATEADD(day, -13, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00'))),
        ('20000000-0000-0000-0000-000000000005', N'Elisa', N'Elisa Martins', NULL, N'ela/dela', '1990-09-09', N'(51) 90000-0005', N'elisa.martins@example.com', N'@elisam.demo', N'Roberta Martins', N'(51) 90000-1005', DATEADD(day, -50, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00')), DATEADD(day, -51, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00'))),
        ('20000000-0000-0000-0000-000000000006', N'Gabriel', N'Gabriel Santos', NULL, N'ele/dele', '1985-12-21', N'(61) 90000-0006', N'gabriel.santos@example.com', N'@gabriels.demo', N'Fernanda Santos', N'(61) 90000-1006', DATEADD(day, -150, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00')), DATEADD(day, -151, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00'))),
        ('20000000-0000-0000-0000-000000000007', N'Helena', N'Helena Costa', NULL, N'ela/dela', '1999-04-05', N'(71) 90000-0007', N'helena.costa@example.com', NULL, N'Marcos Costa', N'(71) 90000-1007', DATEADD(year, -1, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00')), DATEADD(day, -1, DATEADD(year, -1, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00')))),
        ('20000000-0000-0000-0000-000000000008', N'João', N'João Pedro Almeida', NULL, N'ele/dele', '1992-08-30', N'(81) 90000-0008', N'joao.almeida@example.com', N'@joaopa.demo', N'Beatriz Almeida', N'(81) 90000-1008', DATEADD(day, -20, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00')), DATEADD(day, -21, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00'))),
        ('20000000-0000-0000-0000-000000000009', N'Marina', N'Marina Oliveira', NULL, N'ela/dela', '1996-02-11', N'(85) 90000-0009', N'marina.oliveira@example.com', N'@marinao.demo', N'André Oliveira', N'(85) 90000-1009', DATEADD(day, -25, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00')), DATEADD(day, -26, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00'))),
        ('20000000-0000-0000-0000-000000000010', N'Nicolas', N'Nicolas Ferreira', NULL, N'ele/dele', '2000-10-16', N'(91) 90000-0010', N'nicolas.ferreira@example.com', N'@nicolasf.demo', N'Laura Ferreira', N'(91) 90000-1010', DATEADD(day, -260, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00')), DATEADD(day, -261, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00'))),
        ('20000000-0000-0000-0000-000000000011', N'Rafael', N'Rafael Gomes', NULL, N'ele/dele', '1989-05-24', N'(11) 90000-0011', N'rafael.gomes@example.com', NULL, N'Silvia Gomes', N'(11) 90000-1011', DATEADD(day, -8, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00')), DATEADD(day, -9, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00'))),
        ('20000000-0000-0000-0000-000000000012', N'Vitória', N'Vitória Mendes', NULL, N'ela/dela', '1995-07-07', N'(19) 90000-0012', N'vitoria.mendes@example.com', N'@vitoriam.demo', N'Cláudia Mendes', N'(19) 90000-1012', DATEADD(day, -40, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00')), DATEADD(day, -41, TODATETIMEOFFSET(CAST(@Hoje AS datetime2), '+00:00')));

    DECLARE @FichasDemo TABLE (
        [FichaId] uniqueidentifier NOT NULL,
        [ClienteId] uniqueidentifier NOT NULL,
        [QuestionarioId] uniqueidentifier NOT NULL,
        [AceiteId] uniqueidentifier NOT NULL,
        [ProfissionalId] uniqueidentifier NOT NULL,
        [ProfissionalNome] nvarchar(150) NOT NULL,
        [TipoProcedimento] nvarchar(30) NOT NULL,
        [DataConclusao] date NOT NULL
    );

    INSERT INTO @FichasDemo VALUES
        ('30000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000001', @ContaEstudio, N'Lia', N'Tatuagem', @Hoje),
        ('30000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000002', '50000000-0000-0000-0000-000000000002', @ContaEstudio, N'Taty', N'Tatuagem', DATEADD(day, -35, @Hoje)),
        ('30000000-0000-0000-0000-000000000003', '20000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000003', '50000000-0000-0000-0000-000000000003', @ContaEstudio, N'Lia', N'Tatuagem', DATEADD(year, -1, @Hoje)),
        ('30000000-0000-0000-0000-000000000004', '20000000-0000-0000-0000-000000000002', '40000000-0000-0000-0000-000000000004', '50000000-0000-0000-0000-000000000004', @ContaEstudio, N'Thais', N'Piercing', DATEADD(day, -1, @Hoje)),
        ('30000000-0000-0000-0000-000000000005', '20000000-0000-0000-0000-000000000003', '40000000-0000-0000-0000-000000000005', '50000000-0000-0000-0000-000000000005', @ContaEstudio, N'Taty', N'Tatuagem', DATEADD(day, -45, @Hoje)),
        ('30000000-0000-0000-0000-000000000006', '20000000-0000-0000-0000-000000000003', '40000000-0000-0000-0000-000000000006', '50000000-0000-0000-0000-000000000006', @ContaEstudio, N'Taty', N'Tatuagem', DATEADD(day, -1, @Hoje)),
        ('30000000-0000-0000-0000-000000000007', '20000000-0000-0000-0000-000000000004', '40000000-0000-0000-0000-000000000007', '50000000-0000-0000-0000-000000000007', @ContaEstudio, N'Lia', N'Tatuagem', @Hoje),
        ('30000000-0000-0000-0000-000000000008', '20000000-0000-0000-0000-000000000005', '40000000-0000-0000-0000-000000000008', '50000000-0000-0000-0000-000000000008', @ContaEstudio, N'Thais', N'Piercing', DATEADD(day, -35, @Hoje)),
        ('30000000-0000-0000-0000-000000000009', '20000000-0000-0000-0000-000000000006', '40000000-0000-0000-0000-000000000009', '50000000-0000-0000-0000-000000000009', @ContaEstudio, N'Lia', N'Tatuagem', DATEADD(day, -120, @Hoje)),
        ('30000000-0000-0000-0000-000000000010', '20000000-0000-0000-0000-000000000007', '40000000-0000-0000-0000-000000000010', '50000000-0000-0000-0000-000000000010', @ContaEstudio, N'Thais', N'Piercing', DATEADD(year, -1, DATEADD(day, -20, @Hoje))),
        ('30000000-0000-0000-0000-000000000011', '20000000-0000-0000-0000-000000000008', '40000000-0000-0000-0000-000000000011', '50000000-0000-0000-0000-000000000011', @ContaEstudio, N'Taty', N'Tatuagem', DATEADD(day, -2, @Hoje)),
        ('30000000-0000-0000-0000-000000000012', '20000000-0000-0000-0000-000000000009', '40000000-0000-0000-0000-000000000012', '50000000-0000-0000-0000-000000000012', @ContaEstudio, N'Thais', N'Piercing', DATEADD(day, -5, @Hoje)),
        ('30000000-0000-0000-0000-000000000013', '20000000-0000-0000-0000-000000000010', '40000000-0000-0000-0000-000000000013', '50000000-0000-0000-0000-000000000013', @ContaEstudio, N'Lia', N'Tatuagem', DATEADD(day, -240, @Hoje)),
        ('30000000-0000-0000-0000-000000000014', '20000000-0000-0000-0000-000000000010', '40000000-0000-0000-0000-000000000014', '50000000-0000-0000-0000-000000000014', @ContaEstudio, N'Thais', N'Piercing', @Hoje),
        ('30000000-0000-0000-0000-000000000015', '20000000-0000-0000-0000-000000000011', '40000000-0000-0000-0000-000000000015', '50000000-0000-0000-0000-000000000015', @ContaEstudio, N'Lia', N'Tatuagem', @Hoje),
        ('30000000-0000-0000-0000-000000000016', '20000000-0000-0000-0000-000000000012', '40000000-0000-0000-0000-000000000016', '50000000-0000-0000-0000-000000000016', @ContaEstudio, N'Taty', N'Tatuagem', DATEADD(day, -15, @Hoje));

    INSERT INTO [Fichas] (
        [Id], [ClienteId], [ProfissionalResponsavelId],
        [ProfissionalResponsavelNome], [TipoProcedimento], [Status],
        [CriadaEmUtc])
    SELECT
        [FichaId], [ClienteId], [ProfissionalId], [ProfissionalNome],
        [TipoProcedimento], N'Concluida',
        DATEADD(day, -1, TODATETIMEOFFSET(CAST([DataConclusao] AS datetime2), '+00:00'))
    FROM @FichasDemo;

    INSERT INTO [DadosPessoaisFichas] (
        [FichaId], [NomeCompleto], [NomeSocial], [Pronomes],
        [DataNascimento], [Celular], [Email], [Instagram],
        [ContatoEmergenciaNome], [ContatoEmergenciaCelular],
        [ConfirmadosEmUtc])
    SELECT
        ficha.[FichaId], cliente.[NomeCompleto], cliente.[NomeSocial],
        cliente.[Pronomes], cliente.[DataNascimento], cliente.[Celular],
        cliente.[Email], cliente.[Instagram],
        cliente.[ContatoEmergenciaNome], cliente.[ContatoEmergenciaCelular],
        DATEADD(hour, 8, TODATETIMEOFFSET(CAST(ficha.[DataConclusao] AS datetime2), '+00:00'))
    FROM @FichasDemo ficha
    INNER JOIN [Clientes] cliente ON cliente.[Id] = ficha.[ClienteId];

    INSERT INTO [QuestionariosSaude] (
        [Id], [FichaId], [Versao], [TemDiabetes], [TipoDiabetes],
        [PossuiPressaoAlta], [TemAlergia], [DescricaoAlergia],
        [PossuiCondicaoCardiaca], [TemEpilepsia], [TemHemofilia],
        [UsaMarcaPasso], [EstaGravidaOuAmamentando], [RespondidoEmUtc])
    SELECT
        [QuestionarioId], [FichaId], 2,
        CASE WHEN [ClienteId] = '20000000-0000-0000-0000-000000000006' THEN 1 ELSE 0 END,
        CASE WHEN [ClienteId] = '20000000-0000-0000-0000-000000000006' THEN N'Tipo 2, controlada' ELSE NULL END,
        CASE WHEN [ClienteId] = '20000000-0000-0000-0000-000000000005' THEN 1 ELSE 0 END,
        CASE WHEN [ClienteId] = '20000000-0000-0000-0000-000000000003' THEN 1 ELSE 0 END,
        CASE WHEN [ClienteId] = '20000000-0000-0000-0000-000000000003' THEN N'Alergia fictícia a látex' ELSE NULL END,
        0, 0, 0, 0, 0,
        DATEADD(hour, 9, TODATETIMEOFFSET(CAST([DataConclusao] AS datetime2), '+00:00'))
    FROM @FichasDemo;

    INSERT INTO [AceitesTermoConsentimento] (
        [Id], [FichaId], [VersaoTermo], [ConteudoTermo], [ConteudoHash],
        [NomeAssinante], [ConfirmouLeituraEAutorizacao], [VersaoEvidencia],
        [EvidenciaJson], [EvidenciaHash], [AceitoEmUtc])
    SELECT
        ficha.[AceiteId], ficha.[FichaId], 1,
        N'Termo de consentimento fictício para demonstração.',
        REPLICATE(N'0', 64), cliente.[NomeCompleto],
        1, 1, N'{"registroDemonstracao":true}',
        N'0d2c9e656e43a3f29fa34ace539e8dff66f9e5711b28346799f602f88bf0fe6a',
        DATEADD(hour, 10, TODATETIMEOFFSET(CAST(ficha.[DataConclusao] AS datetime2), '+00:00'))
    FROM @FichasDemo ficha
    INNER JOIN [Clientes] cliente ON cliente.[Id] = ficha.[ClienteId];

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;

SELECT N'Clientes' AS [Tabela], COUNT_BIG(*) AS [Registros] FROM [Clientes]
UNION ALL SELECT N'Fichas', COUNT_BIG(*) FROM [Fichas]
UNION ALL SELECT N'DadosPessoaisFichas', COUNT_BIG(*) FROM [DadosPessoaisFichas]
UNION ALL SELECT N'QuestionariosSaude', COUNT_BIG(*) FROM [QuestionariosSaude]
UNION ALL SELECT N'AceitesTermoConsentimento', COUNT_BIG(*) FROM [AceitesTermoConsentimento]
UNION ALL SELECT N'Conta demo do estúdio', COUNT_BIG(*) FROM [AspNetUsers]
WHERE [Id] = @ContaEstudio;

SELECT DISTINCT [ProfissionalResponsavelNome] AS [Profissional],
    [TipoProcedimento]
FROM [Fichas]
ORDER BY [ProfissionalResponsavelNome], [TipoProcedimento];
