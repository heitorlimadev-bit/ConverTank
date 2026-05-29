using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConverTank.Migrations
{
    /// <inheritdoc />
    public partial class LoginAdicionado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cnpj = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Login = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Administrador = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fabricantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fabricantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fabricantes_Entidades_Id",
                        column: x => x.Id,
                        principalTable: "Entidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fornecedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornecedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fornecedores_Entidades_Id",
                        column: x => x.Id,
                        principalTable: "Entidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Postos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Postos_Entidades_Id",
                        column: x => x.Id,
                        principalTable: "Entidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Combustiveis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FornecedorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combustiveis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Combustiveis_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosPostos",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    PostoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioPostoPostoId = table.Column<int>(type: "int", nullable: true),
                    UsuarioPostoUsuarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosPostos", x => new { x.UsuarioId, x.PostoId });
                    table.ForeignKey(
                        name: "FK_UsuariosPostos_Postos_PostoId",
                        column: x => x.PostoId,
                        principalTable: "Postos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuariosPostos_UsuariosPostos_UsuarioPostoUsuarioId_UsuarioPostoPostoId",
                        columns: x => new { x.UsuarioPostoUsuarioId, x.UsuarioPostoPostoId },
                        principalTable: "UsuariosPostos",
                        principalColumns: new[] { "UsuarioId", "PostoId" });
                    table.ForeignKey(
                        name: "FK_UsuariosPostos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tanques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Volume = table.Column<int>(type: "int", nullable: false),
                    Raio = table.Column<double>(type: "float", nullable: false),
                    Comprimento = table.Column<double>(type: "float", nullable: false),
                    PostoId = table.Column<int>(type: "int", nullable: false),
                    CombustivelId = table.Column<int>(type: "int", nullable: false),
                    FabricanteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tanques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tanques_Combustiveis_CombustivelId",
                        column: x => x.CombustivelId,
                        principalTable: "Combustiveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tanques_Fabricantes_FabricanteId",
                        column: x => x.FabricanteId,
                        principalTable: "Fabricantes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tanques_Postos_PostoId",
                        column: x => x.PostoId,
                        principalTable: "Postos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Medicoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Altura = table.Column<int>(type: "int", nullable: false),
                    Volume = table.Column<double>(type: "float", nullable: false),
                    TanqueId = table.Column<int>(type: "int", nullable: false),
                    DataMedicao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medicoes_Tanques_TanqueId",
                        column: x => x.TanqueId,
                        principalTable: "Tanques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Combustiveis_FornecedorId",
                table: "Combustiveis",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicoes_TanqueId",
                table: "Medicoes",
                column: "TanqueId");

            migrationBuilder.CreateIndex(
                name: "IX_Tanques_CombustivelId",
                table: "Tanques",
                column: "CombustivelId");

            migrationBuilder.CreateIndex(
                name: "IX_Tanques_FabricanteId",
                table: "Tanques",
                column: "FabricanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tanques_PostoId",
                table: "Tanques",
                column: "PostoId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPostos_PostoId",
                table: "UsuariosPostos",
                column: "PostoId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPostos_UsuarioPostoUsuarioId_UsuarioPostoPostoId",
                table: "UsuariosPostos",
                columns: new[] { "UsuarioPostoUsuarioId", "UsuarioPostoPostoId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medicoes");

            migrationBuilder.DropTable(
                name: "UsuariosPostos");

            migrationBuilder.DropTable(
                name: "Tanques");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Combustiveis");

            migrationBuilder.DropTable(
                name: "Fabricantes");

            migrationBuilder.DropTable(
                name: "Postos");

            migrationBuilder.DropTable(
                name: "Fornecedores");

            migrationBuilder.DropTable(
                name: "Entidades");
        }
    }
}
