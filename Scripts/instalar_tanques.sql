/* =====================================================================
   ConverTank — instala os tanques nos postos conforme a planilha de volumes (m³)

   Executar:
     sqlcmd -S ".\MSSQLSERVER01" -d ConvertTankDb -f 65001 -C -i Scripts\instalar_tanques.sql

   - Usa os IDs que JÁ existem no banco (postos, combustíveis Vibra/ALE, modelos 7,5/10/15/20/30 m³).
   - Insere só o que falta: para cada posto, compara (combustível + modelo) com o que já
     está ligado e lança apenas a diferença. Pode rodar de novo sem duplicar.
   - Abundância usa os combustíveis da ALE; os demais, Vibra. GA = Gasolina Grid (Vibra) /
     Gasolina Energy (ALE). ARLA: cria "Arla 32" na Vibra se não existir.
   - FICAM DE FORA, para conferência: Anahata, Prakash e Conexão (ver final do arquivo).

   @Aplicar = 0 -> só mostra o que seria inserido (nada é gravado).
   @Aplicar = 1 -> grava.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Aplicar BIT = 1;

BEGIN TRANSACTION;

/* Conferência de segurança: os IDs precisam ter os nomes esperados */
IF NOT EXISTS (SELECT 1 FROM Entidades WHERE Id = 21 AND Nome LIKE N'Vibra%')
   OR NOT EXISTS (SELECT 1 FROM Entidades WHERE Id = 20 AND Nome LIKE N'ALE%')
   OR NOT EXISTS (SELECT 1 FROM Tanques WHERE Id = 1 AND Volume = 30000)
   OR NOT EXISTS (SELECT 1 FROM Tanques WHERE Id = 2 AND Volume = 15000)
   OR NOT EXISTS (SELECT 1 FROM Tanques WHERE Id = 3 AND Volume = 20000)
   OR NOT EXISTS (SELECT 1 FROM Tanques WHERE Id = 4 AND Volume = 10000)
   OR NOT EXISTS (SELECT 1 FROM Tanques WHERE Id = 5 AND Volume = 7500)
BEGIN
    RAISERROR(N'O banco não está como esperado (fornecedores/modelos mudaram). Nada foi alterado.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END

/* ARLA 32 (Vibra) */
DECLARE @Arla INT = (SELECT TOP 1 Id FROM Combustiveis WHERE FornecedorId = 21 AND Nome LIKE N'Arla%');
IF @Arla IS NULL
BEGIN
    INSERT INTO Combustiveis (Nome, FornecedorId, Status) VALUES (N'Arla 32', 21, 1);
    SET @Arla = SCOPE_IDENTITY();
END

/* Planilha: um registro por tanque, na ordem de cada posto */
DECLARE @Plano TABLE (Ordem INT, PostoId INT, Posto NVARCHAR(100), TanqueId INT, CombustivelId INT, Descricao NVARCHAR(50));
INSERT INTO @Plano VALUES
 ( 1,  6, N'Abundancia', 3, 8, N'20 m³ E'),
 ( 2,  6, N'Abundancia', 2, 7, N'15 m³ GC'),
 ( 3,  6, N'Abundancia', 2, 9, N'15 m³ GA'),
 ( 4,  6, N'Abundancia', 4, 11, N'10 m³ S500'),
 ( 5,  1, N'Ananda', 2, 2, N'15 m³ E'),
 ( 6,  1, N'Ananda', 4, 1, N'10 m³ GC'),
 ( 7,  1, N'Ananda', 4, 3, N'10 m³ GA'),
 ( 8,  1, N'Ananda', 2, 5, N'15 m³ S500'),
 ( 9,  1, N'Ananda', 4, 4, N'10 m³ S10'),
 (10, 14, N'Itamarati Dez', 3, 2, N'20 m³ E'),
 (11, 14, N'Itamarati Dez', 2, 1, N'15 m³ GC'),
 (12, 14, N'Itamarati Dez', 2, 3, N'15 m³ GA'),
 (13, 14, N'Itamarati Dez', 4, 4, N'10 m³ S10'),
 (14, 13, N'Mitocondria', 1, 2, N'30 m³ E'),
 (15, 13, N'Mitocondria', 2, 1, N'15 m³ GC'),
 (16, 13, N'Mitocondria', 2, 3, N'15 m³ GA'),
 (17, 13, N'Mitocondria', 1, 5, N'30 m³ S500'),
 (18, 13, N'Mitocondria', 2, 4, N'15 m³ S10'),
 (19,  7, N'Jaya', 3, 2, N'20 m³ E'),
 (20,  7, N'Jaya', 4, 1, N'10 m³ GC'),
 (21,  7, N'Jaya', 4, 3, N'10 m³ GA'),
 (22,  7, N'Jaya', 4, 5, N'10 m³ S500'),
 (23,  7, N'Jaya', 4, 4, N'10 m³ S10'),
 (24,  3, N'Liberty', 2, 2, N'15 m³ E'),
 (25,  3, N'Liberty', 2, 1, N'15 m³ GC'),
 (26,  3, N'Liberty', 4, 3, N'10 m³ GA'),
 (27,  3, N'Liberty', 4, 6, N'10 m³ GP'),
 (28,  3, N'Liberty', 4, 4, N'10 m³ S10'),
 (29, 10, N'São Domingos', 3, 2, N'20 m³ E'),
 (30, 10, N'São Domingos', 2, 1, N'15 m³ GC'),
 (31, 10, N'São Domingos', 2, 3, N'15 m³ GA'),
 (32, 10, N'São Domingos', 4, 5, N'10 m³ S500'),
 (33, 12, N'Star Alliance', 1, 2, N'30 m³ E'),
 (34, 12, N'Star Alliance', 2, 1, N'15 m³ GC'),
 (35, 12, N'Star Alliance', 2, 3, N'15 m³ GA'),
 (36, 12, N'Star Alliance', 1, 4, N'30 m³ S10'),
 (37, 17, N'Marcopolo', 2, 2, N'15 m³ E'),
 (38, 17, N'Marcopolo', 2, 1, N'15 m³ GC'),
 (39, 17, N'Marcopolo', 2, 3, N'15 m³ GA'),
 (40, 17, N'Marcopolo', 2, 5, N'15 m³ S500'),
 (41, 17, N'Marcopolo', 2, 4, N'15 m³ S10'),
 (42, 11, N'Star One', 1, 2, N'30 m³ E'),
 (43, 11, N'Star One', 2, 1, N'15 m³ GC'),
 (44, 11, N'Star One', 2, 3, N'15 m³ GA'),
 (45, 11, N'Star One', 1, 5, N'30 m³ S500'),
 (46,  5, N'Eldorado', 3, 2, N'20 m³ E'),
 (47,  5, N'Eldorado', 2, 1, N'15 m³ GC'),
 (48,  5, N'Eldorado', 2, 3, N'15 m³ GA'),
 (49,  5, N'Eldorado', 4, 5, N'10 m³ S500'),
 (50,  8, N'Matec', 1, 2, N'30 m³ E'),
 (51,  8, N'Matec', 1, 1, N'30 m³ GC'),
 (52,  8, N'Matec', 2, 3, N'15 m³ GA'),
 (53,  8, N'Matec', 2, 5, N'15 m³ S500'),
 (54,  9, N'Visao Futuro', 3, 2, N'20 m³ E'),
 (55,  9, N'Visao Futuro', 2, 1, N'15 m³ GC'),
 (56,  9, N'Visao Futuro', 2, 3, N'15 m³ GA'),
 (57,  9, N'Visao Futuro', 4, 5, N'10 m³ S500'),
 (58, 18, N'Genesis', 3, 2, N'20 m³ E'),
 (59, 18, N'Genesis', 2, 1, N'15 m³ GC'),
 (60, 18, N'Genesis', 2, 3, N'15 m³ GA'),
 (61, 18, N'Genesis', 4, 4, N'10 m³ S10'),
 (62,  4, N'Vanguard', 1, 2, N'30 m³ E'),
 (63,  4, N'Vanguard', 2, 1, N'15 m³ GC'),
 (64,  4, N'Vanguard', 2, 3, N'15 m³ GA'),
 (65,  4, N'Vanguard', 1, 5, N'30 m³ S500'),
 (66,  4, N'Vanguard', 1, 4, N'30 m³ S10'),
 (67,  4, N'Vanguard', 4, @Arla, N'10 m³ ARLA'),
 (68,  2, N'Yantra', 1, 2, N'30 m³ E'),
 (69,  2, N'Yantra', 2, 1, N'15 m³ GC'),
 (70,  2, N'Yantra', 2, 3, N'15 m³ GA'),
 (71,  2, N'Yantra', 2, 5, N'15 m³ S500'),
 (72,  2, N'Yantra', 2, 4, N'15 m³ S10');

/* Confere se os postos do plano batem com os nomes do banco */
IF EXISTS (SELECT 1 FROM @Plano p LEFT JOIN Entidades e ON e.Id = p.PostoId
           WHERE e.Id IS NULL OR e.Nome <> p.Posto)
BEGIN
    SELECT DISTINCT p.PostoId, p.Posto AS EsperadoNoScript, e.Nome AS NoBanco
    FROM @Plano p LEFT JOIN Entidades e ON e.Id = p.PostoId
    WHERE e.Id IS NULL OR e.Nome <> p.Posto;
    RAISERROR(N'Nome de posto diferente do esperado. Nada foi alterado.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END

/* Só o que falta: numera cada (posto, combustível, modelo) do plano e compara com o que já existe */
DECLARE @Faltando TABLE (Ordem INT, PostoId INT, Posto NVARCHAR(100), TanqueId INT, CombustivelId INT, Descricao NVARCHAR(50));
INSERT INTO @Faltando
SELECT x.Ordem, x.PostoId, x.Posto, x.TanqueId, x.CombustivelId, x.Descricao
FROM (
    SELECT p.*, ROW_NUMBER() OVER (PARTITION BY p.PostoId, p.CombustivelId, p.TanqueId ORDER BY p.Ordem) AS n
    FROM @Plano p
) x
WHERE x.n > (SELECT COUNT(*) FROM PostosTanques pt
             WHERE pt.Status = 1 AND pt.PostoId = x.PostoId
               AND pt.CombustivelId = x.CombustivelId AND pt.TanqueId = x.TanqueId);

SELECT Posto, Descricao, CombustivelId, TanqueId AS ModeloId FROM @Faltando ORDER BY Ordem;
DECLARE @Qtd INT = (SELECT COUNT(*) FROM @Faltando);

IF @Aplicar = 1
BEGIN
    INSERT INTO PostosTanques (PostoId, TanqueId, CombustivelId, Identificacao, Status)
    SELECT PostoId, TanqueId, CombustivelId, NULL, 1 FROM @Faltando ORDER BY Ordem;
    COMMIT TRANSACTION;
    PRINT CONCAT(N'GRAVADO: ', @Qtd, N' tanque(s) instalados.');
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT CONCAT(N'SIMULAÇÃO: ', @Qtd, N' tanque(s) seriam instalados. Nada foi gravado. Mude @Aplicar para 1 para gravar.');
END

/* =====================================================================
   PENDENTES DE CONFERÊNCIA (não alterados por este script)
   - Anahata: planilha = 30 Etanol, 15 GC, 15 GA, 30 S500.
     Banco (Id 14) tem um tanque 15 m³ "Diesel S500" onde a planilha indica 15 m³ GA (Gasolina Grid).
     Se for engano:  UPDATE PostosTanques SET CombustivelId = 3 WHERE Id = 14;
   - Prakash: planilha = 22 Etanol, 15 GC, 15 GA, 7 S10. No banco já está como
     15 + 7,5 m³ de Etanol e 7,5 m³ de S10 — mantido como você cadastrou.
   - Conexão: planilha = 10 Etanol, 20 GC, 45 S500, 15 S10. Não existe modelo de 45 m³;
     falta saber se são tanques separados (ex.: 30 + 15) ou um modelo novo.
   ===================================================================== */
