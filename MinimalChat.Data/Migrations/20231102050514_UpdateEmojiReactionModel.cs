using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinimalChat.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmojiReactionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EmojiReactions_MessageId",
                table: "EmojiReactions",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmojiReactions_Messages_MessageId",
                table: "EmojiReactions",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmojiReactions_Messages_MessageId",
                table: "EmojiReactions");

            migrationBuilder.DropIndex(
                name: "IX_EmojiReactions_MessageId",
                table: "EmojiReactions");
        }
    }
}
