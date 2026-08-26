using Xunit;

// Disable parallelism — E2E tests hit a real API and a single web host.
// Running them in parallel would saturate the API DEV instance.
[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = 1)]
