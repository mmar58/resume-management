using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCVJoins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CVAttributeValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CVId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateAttributeValueId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CVAttributeValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CVAttributeValues_CVs_CVId",
                        column: x => x.CVId,
                        principalTable: "CVs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CVAttributeValues_CandidateAttributeValues_CandidateAttribu~",
                        column: x => x.CandidateAttributeValueId,
                        principalTable: "CandidateAttributeValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CVProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CVId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CVProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CVProjects_CVs_CVId",
                        column: x => x.CVId,
                        principalTable: "CVs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CVProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CVAttributeValues_CandidateAttributeValueId",
                table: "CVAttributeValues",
                column: "CandidateAttributeValueId");

            migrationBuilder.CreateIndex(
                name: "IX_CVAttributeValues_CVId_CandidateAttributeValueId",
                table: "CVAttributeValues",
                columns: new[] { "CVId", "CandidateAttributeValueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CVProjects_CVId_ProjectId",
                table: "CVProjects",
                columns: new[] { "CVId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CVProjects_ProjectId",
                table: "CVProjects",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CVAttributeValues");

            migrationBuilder.DropTable(
                name: "CVProjects");
        }
    }
}
