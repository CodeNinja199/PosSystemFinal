using Xunit;

// The test classes share one database and one in-memory server each; running them one at a time keeps the first run
// from migrating and seeding the same new database three times at once.
[assembly: CollectionBehavior(DisableTestParallelization = true)]