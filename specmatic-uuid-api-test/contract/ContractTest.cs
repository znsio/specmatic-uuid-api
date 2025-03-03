using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace specmatic_uuid_api_test.contract
{
    public class ContractTest : IAsyncLifetime, IClassFixture<PostgresTestContainer>,IClassFixture<WebApplicationFactoryFixture<Program>>
    {
        private IContainer? _specmaticTestContainer;
        private readonly string testDirectory;
        private readonly WebApplicationFactoryFixture<Program> _factory;

        public ContractTest()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var testDirectoryInfo = Directory.GetParent(currentDirectory);
            _factory = new WebApplicationFactoryFixture<Program>();
            while (!File.Exists(Path.Combine(testDirectoryInfo.FullName, "specmatic.yaml")))
            {
                testDirectoryInfo = testDirectoryInfo.Parent;
            }
            testDirectory = testDirectoryInfo.FullName;
        }

        [Fact]
        public async Task ContractTestsAsync()
        {
            await RunContractTests();
            Assert.NotNull(_specmaticTestContainer);
            var logs = await _specmaticTestContainer.GetLogsAsync();
            if (!logs.Stdout.Contains("Failures: 0"))
            {
                Assert.Fail("There are failing tests, please refer to build/reports/specmatic/html/index.html for more details");
            }
        }

        public Task InitializeAsync()
        {
            _factory.CreateDefaultClient();
            Console.WriteLine($"UUID service started");
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            if (_specmaticTestContainer != null)
                await _specmaticTestContainer.DisposeAsync();
        }

        private async Task RunContractTests()
        {
            var localReportDirectory = Path.Combine(testDirectory, "build", "reports");
            Directory.CreateDirectory(localReportDirectory);

            _specmaticTestContainer = new ContainerBuilder()
                .WithImage("znsio/specmatic")
                .WithCommand("test")
                .WithCommand($"--port=8080")
                .WithCommand($"--host=host.docker.internal") // Use the test server address
                .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
                .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Tests run:"))
                .WithBindMount(localReportDirectory, "/usr/src/app/build/reports")
                .WithBindMount($"{testDirectory}/specmatic.yaml", "/usr/src/app/specmatic.yaml")
                .WithBindMount($"{testDirectory}/uuid.openapi.yaml", "/usr/src/app/uuid.openapi.yaml")
                .Build();

            await _specmaticTestContainer.StartAsync();
        }
    }
}
