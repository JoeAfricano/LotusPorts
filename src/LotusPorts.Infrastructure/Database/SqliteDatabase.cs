using Microsoft.Data.Sqlite;

namespace LotusPorts.Infrastructure.Database;

/// <summary>Opens connections to the local database. All access goes through the repositories in this folder.</summary>
public sealed class SqliteDatabase(string path)
{
    public string Path { get; } = path;

    public async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken = default)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = Path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,
            DefaultTimeout = 5,
        };
        var connection = new SqliteConnection(builder.ToString());
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }
}
