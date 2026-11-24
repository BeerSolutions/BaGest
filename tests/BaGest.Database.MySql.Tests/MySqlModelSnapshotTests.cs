using BaGest.Database.MySql.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaGest.Database.MySql.Tests;

public class MySqlModelSnapshotTests
{
    [Fact]
    public void ModelSnapshot_MatchesCurrentModel()
    {
        // The comparison runs against the model only, so no server is needed when the version is given.
        var options = new DbContextOptionsBuilder<MySqlContext>()
            .UseMySql("Server=localhost", ServerVersion.Parse("8.4.0-mysql"))
            .Options;

        using var context = new MySqlContext(options, StaticOptionsSnapshot.ForConnectionString("Server=localhost"));

        Assert.False(context.Database.HasPendingModelChanges());
    }
}
