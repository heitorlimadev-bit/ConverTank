/* Consulta SOMENTE LEITURA do estado atual do cadastro (não altera nada).
   sqlcmd -S ".\MSSQLSERVER01" -d ConvertTankDb -f 65001 -C -s "|" -W -i Scripts\ler_banco.sql -o Scripts\estado_banco.txt */
SET NOCOUNT ON;

PRINT '### POSTOS';
SELECT e.Id, e.Nome, e.Status FROM Postos p JOIN Entidades e ON e.Id = p.Id ORDER BY e.Nome;

PRINT '### FORNECEDORES';
SELECT e.Id, e.Nome, e.Status FROM Fornecedores f JOIN Entidades e ON e.Id = f.Id ORDER BY e.Nome;

PRINT '### FABRICANTES';
SELECT e.Id, e.Nome, e.Status FROM Fabricantes f JOIN Entidades e ON e.Id = f.Id ORDER BY e.Nome;

PRINT '### COMBUSTIVEIS';
SELECT c.Id, c.Nome, c.FornecedorId, e.Nome AS Fornecedor, c.Status
FROM Combustiveis c JOIN Entidades e ON e.Id = c.FornecedorId ORDER BY e.Nome, c.Nome;

PRINT '### MODELOS DE TANQUE';
SELECT t.Id, t.Nome, t.Volume, t.Raio, t.Comprimento, t.FabricanteId, t.Status FROM Tanques t ORDER BY t.Volume, t.Id;

PRINT '### TANQUES INSTALADOS NOS POSTOS';
SELECT pt.Id, pt.PostoId, ep.Nome AS Posto, pt.TanqueId, t.Volume, pt.CombustivelId, c.Nome AS Combustivel,
       ef.Nome AS Fornecedor, pt.Identificacao, pt.Status
FROM PostosTanques pt
JOIN Entidades ep ON ep.Id = pt.PostoId
JOIN Tanques t ON t.Id = pt.TanqueId
JOIN Combustiveis c ON c.Id = pt.CombustivelId
JOIN Entidades ef ON ef.Id = c.FornecedorId
ORDER BY ep.Nome, pt.Id;

PRINT '### MEDICOES POR TANQUE INSTALADO';
SELECT PostoTanqueId, COUNT(*) AS Medicoes FROM Medicoes GROUP BY PostoTanqueId;
