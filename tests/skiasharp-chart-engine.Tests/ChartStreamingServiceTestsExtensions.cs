using System.Threading.Tasks;

namespace SkiasharpChartEngine.Tests
{
    /// <summary>
    /// Provides extension methods for <see cref="ChartStreamingServiceTests"/> to facilitate
    /// batch execution of test scenarios.
    /// </summary>
    public static class ChartStreamingServiceTestsExtensions
    {
        /// <summary>
        /// Executes all test methods within the test class, including asynchronous operations.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        public static async Task ExecuteAllTestsAsync(this ChartStreamingServiceTests tests)
        {
            tests.Register_NewChart_IsRegisteredSuccessfully();
            tests.Publish_UnregisteredChart_ThrowsInvalidOperationException();
            tests.Publish_ValidPoint_IsAppliedToSnapshot();
            tests.PublishBatch_MultiplePoints_AllApplied();
            tests.WindowSize_Enforced_OldestPointsDropped();
            tests.AutoCreateSeries_WhenSeriesDoesNotExist_SeriesCreated();
            tests.Unregister_PublishAfterwards_ThrowsInvalidOperationException();

            await tests.FlushAsync_AppliesBufferedPoints();
        }

        /// <summary>
        /// Executes test methods specifically related to data publishing and batch operations.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        public static void ExecutePublishingTests(this ChartStreamingServiceTests tests)
        {
            tests.Publish_UnregisteredChart_ThrowsInvalidOperationException();
            tests.Publish_ValidPoint_IsAppliedToSnapshot();
            tests.PublishBatch_MultiplePoints_AllApplied();
        }

        /// <summary>
        /// Executes test methods related to the lifecycle of the chart (registration and unregistration).
        /// </summary>
        /// <param name="tests">The test instance.</param>
        public static void ExecuteLifecycleTests(this ChartStreamingServiceTests tests)
        {
            tests.Register_NewChart_IsRegisteredSuccessfully();
            tests.Unregister_PublishAfterwards_ThrowsInvalidOperationException();
        }

        /// <summary>
        /// Executes test methods related to chart configuration, such as windowing and series creation.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        public static void ExecuteConfigurationTests(this ChartStreamingServiceTests tests)
        {
            tests.WindowSize_Enforced_OldestPointsDropped();
            tests.AutoCreateSeries_WhenSeriesDoesNotExist_SeriesCreated();
        }
    }
}
