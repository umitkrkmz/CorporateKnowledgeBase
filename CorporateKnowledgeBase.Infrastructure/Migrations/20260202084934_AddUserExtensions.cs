using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CorporateKnowledgeBase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnNewAnnouncement",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnNewBlogPost",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnNewComment",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnNewDocument",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastLoginDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NotifyOnNewAnnouncement",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NotifyOnNewBlogPost",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NotifyOnNewComment",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NotifyOnNewDocument",
                table: "AspNetUsers");
        }
    }
}
