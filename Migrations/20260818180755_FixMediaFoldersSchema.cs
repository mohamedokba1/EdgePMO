using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgePMO.API.Migrations
{
    /// <inheritdoc />
    public partial class FixMediaFoldersSchema : Migration
    {
        // Requirements 1.1/1.2 — the original AddMediafilesStructure migration defined
        // MediaFolders' self-referencing FK (and MediaFiles.FolderId's FK) pointing at
        // principalColumn "CertificateId", a column that doesn't exist on MediaFolders.
        // A Postgres CREATE TABLE with a foreign key targeting a nonexistent column
        // cannot succeed, so depending on how that migration was actually applied to
        // this database, MediaFolders/MediaFiles.FolderId may be entirely missing, or
        // may have been hand-patched already. This migration is written to self-heal
        // either state without server/DB access to confirm which one we're in: every
        // statement is guarded so it's a no-op if the schema is already correct.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""MediaFolders"" (
                    ""Id"" uuid NOT NULL DEFAULT gen_random_uuid(),
                    ""Name"" character varying(255) NOT NULL,
                    ""RelativePath"" character varying(1000) NOT NULL,
                    ""ParentFolderId"" uuid NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT ""PK_MediaFolders"" PRIMARY KEY (""Id"")
                );
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""MediaFiles"" ADD COLUMN IF NOT EXISTS ""FolderId"" uuid NULL;
            ");

            // Drop whichever FKs exist under either the broken or the correct
            // definition, then recreate them pointed at the real primary key.
            migrationBuilder.Sql(@"
                ALTER TABLE ""MediaFolders"" DROP CONSTRAINT IF EXISTS ""FK_MediaFolders_MediaFolders_ParentFolderId"";
                ALTER TABLE ""MediaFiles"" DROP CONSTRAINT IF EXISTS ""FK_MediaFiles_MediaFolders_FolderId"";

                ALTER TABLE ""MediaFolders""
                    ADD CONSTRAINT ""FK_MediaFolders_MediaFolders_ParentFolderId""
                    FOREIGN KEY (""ParentFolderId"") REFERENCES ""MediaFolders"" (""Id"") ON DELETE CASCADE;

                ALTER TABLE ""MediaFiles""
                    ADD CONSTRAINT ""FK_MediaFiles_MediaFolders_FolderId""
                    FOREIGN KEY (""FolderId"") REFERENCES ""MediaFolders"" (""Id"") ON DELETE SET NULL;
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_MediaFiles_FolderId"" ON ""MediaFiles"" (""FolderId"");
                CREATE INDEX IF NOT EXISTS ""IX_MediaFolders_ParentFolderId"" ON ""MediaFolders"" (""ParentFolderId"");
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_MediaFolders_Name_ParentFolderId"" ON ""MediaFolders"" (""Name"", ""ParentFolderId"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Deliberately a no-op — this migration only repairs a broken FK
            // definition and fills in schema that should already exist per the
            // original (now also corrected) AddMediafilesStructure migration.
            // Rolling it back would risk dropping MediaFolders/FolderId data that
            // predates this fix.
        }
    }
}
