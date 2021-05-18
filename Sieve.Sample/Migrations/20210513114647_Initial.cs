using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sieve.Sample.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(nullable: true),
                    LikeCount = table.Column<int>(nullable: false),
                    CommentCount = table.Column<int>(nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(nullable: false),
                    DateLastViewed = table.Column<DateTime>(type: "datetime", nullable: false),
                    CategoryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Posts");
        }
    }
}
