using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JWTdemo.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleLikeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotificationStatus_Users_UserId",
                table: "UserNotificationStatus");

            migrationBuilder.CreateTable(
                name: "ArticleLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleLikes_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleLikes_ArticleId",
                table: "ArticleLikes",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleLikes_UserId",
                table: "ArticleLikes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotificationStatus_Users_UserId",
                table: "UserNotificationStatus",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserNotificationStatus_Users_UserId",
                table: "UserNotificationStatus");

            migrationBuilder.DropTable(
                name: "ArticleLikes");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotificationStatus_Users_UserId",
                table: "UserNotificationStatus",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
