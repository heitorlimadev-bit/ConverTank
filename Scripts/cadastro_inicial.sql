/* =====================================================================
   ConverTank — cadastro inicial pelo terminal (sem interface gráfica)

   Executar (PowerShell, na pasta do projeto):
     sqlcmd -S "localhost,1433" -d ConvertTankDb -f 65001 -C -i Scripts\cadastro_inicial.sql

   -f 65001 = UTF-8 (necessário para os acentos: São Domingos, Conexão...).
   O banco precisa existir: rode o sistema uma vez (dotnet run) antes.
   O script pode ser executado mais de uma vez: não duplica o que já existe.

   ATENÇÃO — tabelas com herança (Postos, Fornecedores, Fabricantes):
   o registro é criado primeiro em Entidades (Nome, Cnpj, Status) e depois
   o mesmo Id é inserido na tabela filha.
   ===================================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

/* ---------------- 1. AJUSTE AQUI ---------------- */
DECLARE @AdminNome   NVARCHAR(100) = N'Administrador';
DECLARE @AdminLogin  NVARCHAR(100) = N'admin';
DECLARE @AdminSenha  NVARCHAR(100) = N'troque-esta-senha';
DECLARE @Fornecedor  NVARCHAR(100) = N'FORNECEDOR A DEFINIR';
DECLARE @Fabricante  NVARCHAR(100) = N'FABRICANTE A DEFINIR';
/* 1 = instala os tanques nos postos conforme a planilha LEITURAS,
       todos com o modelo provisório "A DEFINIR" (trocar depois — ver seção 7) */
DECLARE @CriarInstalacoes BIT = 1;
/* ------------------------------------------------ */

/* 2. Usuário administrador */
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Login = @AdminLogin)
    INSERT INTO Usuarios (Nome, Login, Senha, Administrador, Status)
    VALUES (@AdminNome, @AdminLogin, @AdminSenha, 1, 1);

/* 3. Fornecedor e fabricante (herança: Entidades + tabela filha) */
DECLARE @FornecedorId INT = (SELECT TOP 1 e.Id FROM Entidades e JOIN Fornecedores f ON f.Id = e.Id WHERE e.Nome = @Fornecedor);
IF @FornecedorId IS NULL
BEGIN
    INSERT INTO Entidades (Nome, Cnpj, Status) VALUES (@Fornecedor, N'', 1);
    SET @FornecedorId = SCOPE_IDENTITY();
    INSERT INTO Fornecedores (Id) VALUES (@FornecedorId);
END

DECLARE @FabricanteId INT = (SELECT TOP 1 e.Id FROM Entidades e JOIN Fabricantes f ON f.Id = e.Id WHERE e.Nome = @Fabricante);
IF @FabricanteId IS NULL
BEGIN
    INSERT INTO Entidades (Nome, Cnpj, Status) VALUES (@Fabricante, N'', 1);
    SET @FabricanteId = SCOPE_IDENTITY();
    INSERT INTO Fabricantes (Id) VALUES (@FabricanteId);
END

/* 4. Combustíveis (siglas usadas na planilha; a planilha de estoque converte para o nome completo) */
DECLARE @Combustiveis TABLE (Sigla NVARCHAR(10));
INSERT INTO @Combustiveis VALUES (N'GC'), (N'GA'), (N'GP'), (N'E'), (N'D'), (N'S10'), (N'ARLA');

INSERT INTO Combustiveis (Nome, FornecedorId, Status)
SELECT c.Sigla, @FornecedorId, 1
FROM @Combustiveis c
WHERE NOT EXISTS (SELECT 1 FROM Combustiveis x WHERE x.Nome = c.Sigla AND x.FornecedorId = @FornecedorId);

/* 5. Postos (planilha LEITURAS) */
DECLARE @Postos TABLE (Nome NVARCHAR(100));
INSERT INTO @Postos VALUES
 (N'São Domingos'), (N'Visão Futuro'), (N'Prakash C'), (N'Ananda'), (N'Abundância'), (N'Conexão3'), (N'Anahata'),
 (N'Eldorado'), (N'Jaya'), (N'Prakash B'), (N'Matec'), (N'Mitocôndria'), (N'Star One'), (N'Gênesis'),
 (N'Itamarati 10'), (N'Star Alliance'), (N'Yantra'), (N'Marcopolo'), (N'Vanguard'), (N'Liberty');

DECLARE @Nome NVARCHAR(100), @NovoId INT;
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT Nome FROM @Postos;
OPEN cur;
FETCH NEXT FROM cur INTO @Nome;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Entidades e JOIN Postos p ON p.Id = e.Id WHERE e.Nome = @Nome)
    BEGIN
        INSERT INTO Entidades (Nome, Cnpj, Status) VALUES (@Nome, N'', 1);
        SET @NovoId = SCOPE_IDENTITY();
        INSERT INTO Postos (Id) VALUES (@NovoId);
    END
    FETCH NEXT FROM cur INTO @Nome;
END
CLOSE cur; DEALLOCATE cur;

/* 6. Tanques instalados em cada posto (ordem = T1, T2... da planilha LEITURAS) */
IF @CriarInstalacoes = 1
BEGIN
    /* Modelo provisório — volume/raio/comprimento zerados até cadastrar os modelos reais */
    DECLARE @ModeloProvisorio INT = (SELECT TOP 1 Id FROM Tanques WHERE Nome = N'A DEFINIR');
    IF @ModeloProvisorio IS NULL
    BEGIN
        INSERT INTO Tanques (Nome, Volume, Raio, Comprimento, FabricanteId, Status)
        VALUES (N'A DEFINIR', 0, 0, 0, @FabricanteId, 1);
        SET @ModeloProvisorio = SCOPE_IDENTITY();
    END

    DECLARE @Instalacoes TABLE (Ordem INT IDENTITY(1,1), Posto NVARCHAR(100), Sigla NVARCHAR(10));
    INSERT INTO @Instalacoes (Posto, Sigla) VALUES
     (N'São Domingos', N'GC'), (N'São Domingos', N'GA'), (N'São Domingos', N'E'), (N'São Domingos', N'D'),
     (N'Visão Futuro', N'GC'), (N'Visão Futuro', N'GA'), (N'Visão Futuro', N'E'), (N'Visão Futuro', N'D'),
     (N'Prakash C', N'GC'), (N'Prakash C', N'GA'), (N'Prakash C', N'E'), (N'Prakash C', N'S10'),
     (N'Ananda', N'GC'), (N'Ananda', N'GA'), (N'Ananda', N'E'), (N'Ananda', N'D'), (N'Ananda', N'S10'),
     (N'Abundância', N'GC'), (N'Abundância', N'GA'), (N'Abundância', N'E'), (N'Abundância', N'D'),
     (N'Conexão3', N'GC'), (N'Conexão3', N'GC'), (N'Conexão3', N'E'), (N'Conexão3', N'D'), (N'Conexão3', N'D'), (N'Conexão3', N'S10'),
     (N'Anahata', N'GC'), (N'Anahata', N'GA'), (N'Anahata', N'E'), (N'Anahata', N'D'),
     (N'Eldorado', N'GC'), (N'Eldorado', N'GA'), (N'Eldorado', N'E'), (N'Eldorado', N'D'),
     (N'Jaya', N'GC'), (N'Jaya', N'GA'), (N'Jaya', N'E'), (N'Jaya', N'D'), (N'Jaya', N'S10'),
     (N'Prakash B', N'GC'), (N'Prakash B', N'GA'), (N'Prakash B', N'E'),
     (N'Matec', N'GC'), (N'Matec', N'GA'), (N'Matec', N'E'), (N'Matec', N'D'),
     (N'Mitocôndria', N'GC'), (N'Mitocôndria', N'GA'), (N'Mitocôndria', N'E'), (N'Mitocôndria', N'D'), (N'Mitocôndria', N'S10'),
     (N'Star One', N'GC'), (N'Star One', N'GA'), (N'Star One', N'E'), (N'Star One', N'D'),
     (N'Gênesis', N'GC'), (N'Gênesis', N'GA'), (N'Gênesis', N'E'), (N'Gênesis', N'S10'),
     (N'Itamarati 10', N'GC'), (N'Itamarati 10', N'GA'), (N'Itamarati 10', N'E'), (N'Itamarati 10', N'S10'),
     (N'Star Alliance', N'GC'), (N'Star Alliance', N'GA'), (N'Star Alliance', N'E'), (N'Star Alliance', N'S10'),
     (N'Yantra', N'GC'), (N'Yantra', N'GA'), (N'Yantra', N'E'), (N'Yantra', N'D'), (N'Yantra', N'S10'),
     (N'Marcopolo', N'GC'), (N'Marcopolo', N'GA'), (N'Marcopolo', N'E'), (N'Marcopolo', N'D'), (N'Marcopolo', N'S10'),
     (N'Vanguard', N'GC'), (N'Vanguard', N'GA'), (N'Vanguard', N'E'), (N'Vanguard', N'D'), (N'Vanguard', N'S10'), (N'Vanguard', N'ARLA'),
     (N'Liberty', N'GC'), (N'Liberty', N'GA'), (N'Liberty', N'GP'), (N'Liberty', N'E'), (N'Liberty', N'S10');

    /* Só instala nos postos que ainda não têm nenhum tanque (evita duplicar ao rodar de novo) */
    INSERT INTO PostosTanques (PostoId, TanqueId, CombustivelId, Identificacao, Status)
    SELECT p.Id, @ModeloProvisorio, c.Id, NULL, 1
    FROM @Instalacoes i
    JOIN Entidades e ON e.Nome = i.Posto
    JOIN Postos p ON p.Id = e.Id
    JOIN Combustiveis c ON c.Nome = i.Sigla AND c.FornecedorId = @FornecedorId
    WHERE NOT EXISTS (SELECT 1 FROM PostosTanques x WHERE x.PostoId = p.Id)
    ORDER BY i.Ordem;
END

COMMIT TRANSACTION;

/* Conferência */
SELECT e.Nome AS Posto, COUNT(pt.Id) AS Tanques,
       STRING_AGG(c.Nome, N', ') WITHIN GROUP (ORDER BY pt.Id) AS Combustiveis
FROM Postos p
JOIN Entidades e ON e.Id = p.Id
LEFT JOIN PostosTanques pt ON pt.PostoId = p.Id AND pt.Status = 1
LEFT JOIN Combustiveis c ON c.Id = pt.CombustivelId
GROUP BY e.Nome
ORDER BY e.Nome;

/* =====================================================================
   7. DEPOIS: cadastrar os modelos reais e trocar o provisório
   ---------------------------------------------------------------------
   -- novo modelo (Volume em LITROS; Raio e Comprimento em METROS; Raio = diâmetro / 2)
   INSERT INTO Tanques (Nome, Volume, Raio, Comprimento, FabricanteId, Status)
   VALUES (N'15 m³', 15000, 1.25, 3.10, <FabricanteId>, 1);

   -- ver os IDs dos tanques instalados
   SELECT pt.Id, e.Nome AS Posto, c.Nome AS Comb, t.Nome AS Modelo
   FROM PostosTanques pt JOIN Entidades e ON e.Id = pt.PostoId
   JOIN Combustiveis c ON c.Id = pt.CombustivelId JOIN Tanques t ON t.Id = pt.TanqueId
   ORDER BY e.Nome, pt.Id;

   -- trocar o modelo de um ou vários tanques instalados
   UPDATE PostosTanques SET TanqueId = <IdDoModelo> WHERE Id IN (<ids>);

   -- preencher CNPJ de um posto/fornecedor/fabricante
   UPDATE Entidades SET Cnpj = N'00.000.000/0000-00' WHERE Nome = N'São Domingos';
   ===================================================================== */
