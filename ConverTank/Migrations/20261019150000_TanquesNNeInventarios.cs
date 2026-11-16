using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConverTank.Migrations
{
    /// <summary>
    /// 1) Tanque passa a ser MODELO de tanque; nova tabela PostosTanques (N:N Posto × Tanque,
    ///    com chave própria) guarda cada tanque instalado, com seu combustível.
    /// 2) Medições passam a apontar para PostosTanques.
    /// 3) Coluna Diametro vira Raio (o código já usava Raio).
    /// 4) Tabelas de Inventário.
    ///
    /// Os dados existentes são preservados: cada tanque antigo vira uma instalação no seu posto,
    /// as medições são religadas a essa instalação e tanques ativos com as mesmas especificações
    /// (volume, raio, comprimento e fabricante) são unificados num único modelo.
    /// </summary>
    public partial class TanquesNNeInventarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---- Solta as FKs antigas ----
            migrationBuilder.DropForeignKey(name: "FK_Medicoes_Tanques_TanqueId", table: "Medicoes");
            migrationBuilder.DropForeignKey(name: "FK_Tanques_Combustiveis_CombustivelId", table: "Tanques");
            migrationBuilder.DropForeignKey(name: "FK_Tanques_Postos_PostoId", table: "Tanques");

            // ---- Diametro → Raio ----
            migrationBuilder.RenameColumn(name: "Diametro", table: "Tanques", newName: "Raio");
            migrationBuilder.Sql("UPDATE [Tanques] SET [Raio] = [Raio] / 2.0;");

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "Tanques",
                type: "nvarchar(max)",
                nullable: true);

            // ---- Tabela N:N ----
            migrationBuilder.CreateTable(
                name: "PostosTanques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostoId = table.Column<int>(type: "int", nullable: false),
                    TanqueId = table.Column<int>(type: "int", nullable: false),
                    CombustivelId = table.Column<int>(type: "int", nullable: false),
                    Identificacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostosTanques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostosTanques_Combustiveis_CombustivelId",
                        column: x => x.CombustivelId,
                        principalTable: "Combustiveis",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PostosTanques_Postos_PostoId",
                        column: x => x.PostoId,
                        principalTable: "Postos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PostosTanques_Tanques_TanqueId",
                        column: x => x.TanqueId,
                        principalTable: "Tanques",
                        principalColumn: "Id");
                });

            // Cada tanque existente vira uma instalação no seu posto (mantém a ordem por posto)
            migrationBuilder.Sql(@"
INSERT INTO [PostosTanques] ([PostoId], [TanqueId], [CombustivelId], [Identificacao], [Status])
SELECT [PostoId], [Id], [CombustivelId], NULL, [Status]
FROM [Tanques]
ORDER BY [PostoId], [Id];");

            // ---- Medições: TanqueId → PostoTanqueId ----
            migrationBuilder.AddColumn<int>(
                name: "PostoTanqueId",
                table: "Medicoes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
UPDATE m SET m.[PostoTanqueId] = pt.[Id]
FROM [Medicoes] m
INNER JOIN [PostosTanques] pt ON pt.[TanqueId] = m.[TanqueId];");

            migrationBuilder.DropIndex(name: "IX_Medicoes_TanqueId", table: "Medicoes");
            migrationBuilder.DropColumn(name: "TanqueId", table: "Medicoes");

            // ---- Unifica modelos idênticos (somente tanques ativos) ----
            migrationBuilder.Sql(@"
;WITH c AS (
    SELECT [Id], MIN([Id]) OVER (PARTITION BY [Volume], [Raio], [Comprimento], [FabricanteId]) AS [Canonico]
    FROM [Tanques]
    WHERE [Status] = 1
)
UPDATE pt SET pt.[TanqueId] = c.[Canonico]
FROM [PostosTanques] pt
INNER JOIN c ON c.[Id] = pt.[TanqueId];

UPDATE t SET t.[Status] = 0
FROM [Tanques] t
WHERE t.[Status] = 1 AND EXISTS (
    SELECT 1 FROM [Tanques] t2
    WHERE t2.[Status] = 1 AND t2.[Id] < t.[Id]
      AND t2.[Volume] = t.[Volume] AND t2.[Raio] = t.[Raio]
      AND t2.[Comprimento] = t.[Comprimento] AND t2.[FabricanteId] = t.[FabricanteId]);");

            // ---- Remove posto e combustível do modelo ----
            migrationBuilder.DropIndex(name: "IX_Tanques_CombustivelId", table: "Tanques");
            migrationBuilder.DropIndex(name: "IX_Tanques_PostoId", table: "Tanques");
            migrationBuilder.DropColumn(name: "CombustivelId", table: "Tanques");
            migrationBuilder.DropColumn(name: "PostoId", table: "Tanques");

            migrationBuilder.CreateIndex(name: "IX_Medicoes_PostoTanqueId", table: "Medicoes", column: "PostoTanqueId");
            migrationBuilder.CreateIndex(name: "IX_PostosTanques_CombustivelId", table: "PostosTanques", column: "CombustivelId");
            migrationBuilder.CreateIndex(name: "IX_PostosTanques_PostoId", table: "PostosTanques", column: "PostoId");
            migrationBuilder.CreateIndex(name: "IX_PostosTanques_TanqueId", table: "PostosTanques", column: "TanqueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medicoes_PostosTanques_PostoTanqueId",
                table: "Medicoes",
                column: "PostoTanqueId",
                principalTable: "PostosTanques",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // ---- Inventários ----
            migrationBuilder.CreateTable(
                name: "Inventarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ano = table.Column<int>(type: "int", nullable: false),
                    Trimestre = table.Column<int>(type: "int", nullable: false),
                    DataReferencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataImportacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItensInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventarioId = table.Column<int>(type: "int", nullable: false),
                    PostoTanqueId = table.Column<int>(type: "int", nullable: false),
                    MedicaoId = table.Column<int>(type: "int", nullable: true),
                    PostoNome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroNoPosto = table.Column<int>(type: "int", nullable: false),
                    Identificacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Combustivel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FornecedorId = table.Column<int>(type: "int", nullable: false),
                    FornecedorNome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CapacidadeM3 = table.Column<double>(type: "float", nullable: false),
                    AlturaCm = table.Column<double>(type: "float", nullable: true),
                    VolumeLitros = table.Column<double>(type: "float", nullable: true),
                    DataLeitura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ppl = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensInventario_Inventarios_InventarioId",
                        column: x => x.InventarioId,
                        principalTable: "Inventarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItensInventario_Medicoes_MedicaoId",
                        column: x => x.MedicaoId,
                        principalTable: "Medicoes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItensInventario_PostosTanques_PostoTanqueId",
                        column: x => x.PostoTanqueId,
                        principalTable: "PostosTanques",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(name: "IX_Inventarios_Ano_Trimestre", table: "Inventarios", columns: new[] { "Ano", "Trimestre" });
            migrationBuilder.CreateIndex(name: "IX_ItensInventario_InventarioId", table: "ItensInventario", column: "InventarioId");
            migrationBuilder.CreateIndex(name: "IX_ItensInventario_MedicaoId", table: "ItensInventario", column: "MedicaoId");
            migrationBuilder.CreateIndex(name: "IX_ItensInventario_PostoTanqueId", table: "ItensInventario", column: "PostoTanqueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Volta para 1 tanque = 1 posto. Instalações do mesmo modelo além da primeira
            // não têm como voltar ao formato antigo e são descartadas junto com suas medições.
            migrationBuilder.DropTable(name: "ItensInventario");
            migrationBuilder.DropTable(name: "Inventarios");

            migrationBuilder.DropForeignKey(name: "FK_Medicoes_PostosTanques_PostoTanqueId", table: "Medicoes");

            migrationBuilder.AddColumn<int>(name: "PostoId", table: "Tanques", type: "int", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<int>(name: "CombustivelId", table: "Tanques", type: "int", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<int>(name: "TanqueId", table: "Medicoes", type: "int", nullable: false, defaultValue: 0);

            migrationBuilder.Sql(@"
;WITH primeira AS (
    SELECT pt.*, ROW_NUMBER() OVER (PARTITION BY pt.[TanqueId] ORDER BY pt.[Id]) AS rn
    FROM [PostosTanques] pt
)
UPDATE t SET t.[PostoId] = p.[PostoId], t.[CombustivelId] = p.[CombustivelId]
FROM [Tanques] t INNER JOIN primeira p ON p.[TanqueId] = t.[Id] AND p.rn = 1;

UPDATE m SET m.[TanqueId] = pt.[TanqueId]
FROM [Medicoes] m INNER JOIN [PostosTanques] pt ON pt.[Id] = m.[PostoTanqueId];

DELETE m FROM [Medicoes] m
WHERE NOT EXISTS (
    SELECT 1 FROM [PostosTanques] pt
    WHERE pt.[Id] = m.[PostoTanqueId]
      AND pt.[Id] = (SELECT MIN(p2.[Id]) FROM [PostosTanques] p2 WHERE p2.[TanqueId] = pt.[TanqueId]));

DELETE FROM [Tanques] WHERE [Id] NOT IN (SELECT [TanqueId] FROM [PostosTanques]);");

            migrationBuilder.DropIndex(name: "IX_Medicoes_PostoTanqueId", table: "Medicoes");
            migrationBuilder.DropColumn(name: "PostoTanqueId", table: "Medicoes");
            migrationBuilder.DropTable(name: "PostosTanques");

            migrationBuilder.DropColumn(name: "Nome", table: "Tanques");
            migrationBuilder.Sql("UPDATE [Tanques] SET [Raio] = [Raio] * 2.0;");
            migrationBuilder.RenameColumn(name: "Raio", table: "Tanques", newName: "Diametro");

            migrationBuilder.CreateIndex(name: "IX_Medicoes_TanqueId", table: "Medicoes", column: "TanqueId");
            migrationBuilder.CreateIndex(name: "IX_Tanques_CombustivelId", table: "Tanques", column: "CombustivelId");
            migrationBuilder.CreateIndex(name: "IX_Tanques_PostoId", table: "Tanques", column: "PostoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medicoes_Tanques_TanqueId", table: "Medicoes", column: "TanqueId",
                principalTable: "Tanques", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Tanques_Combustiveis_CombustivelId", table: "Tanques", column: "CombustivelId",
                principalTable: "Combustiveis", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Tanques_Postos_PostoId", table: "Tanques", column: "PostoId",
                principalTable: "Postos", principalColumn: "Id");
        }
    }
}
