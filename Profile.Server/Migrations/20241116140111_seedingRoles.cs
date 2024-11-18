using Microsoft.EntityFrameworkCore.Migrations;
using Profile.Server.Settings;

#nullable disable

namespace Profile.Server.Migrations
{
    /// <inheritdoc />
    public partial class seedingRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), RoleNames.Admin, "ADMIN" },
                    { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), RoleNames.User, "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
            => migrationBuilder.Sql("DELETE FROM [Roles]");
    }
}
