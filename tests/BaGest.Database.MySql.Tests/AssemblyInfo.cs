using Xunit;

// MySqlConnectionUsageTests reads the server-wide connection counter, which tests running in parallel would change.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
