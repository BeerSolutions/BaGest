using BaGest.Core;
using Microsoft.Extensions.Options;

namespace BaGest.Database.Sqlite.Tests.Support;

internal sealed class StaticOptionsSnapshot(BaGestOptions value) : IOptionsSnapshot<BaGestOptions>
{
    public BaGestOptions Value => value;

    public BaGestOptions Get(string name) => value;

    public static StaticOptionsSnapshot ForConnectionString(string connectionString) =>
        new(new BaGestOptions { Database = new DatabaseOptions { ConnectionString = connectionString } });
}
