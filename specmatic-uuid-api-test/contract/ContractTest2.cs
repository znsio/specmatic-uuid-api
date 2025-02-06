using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using DotNet.Testcontainers.Builders;

namespace specmatic_uuid_api_test.contract
{
    public class ContractTest2 : IClassFixture<PostgresTestContainer>
    {
        private DotNet.Testcontainers.Containers.IContainer? _testContainer;
        private readonly HttpClient _client;
        private const string ProjectName = "specmatic-uuid-api";
        private const string TestContainerDirectory = "/usr/src/app";
        private readonly string apiDirectory;
        private readonly string testDirectory;

        private string _baseUrl = "";

        public ContractTest2(PostgresTestContainer postgresContainer)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var testDirectoryInfo = Directory.GetParent(currentDirectory);
            while (!File.Exists(Path.Combine(testDirectoryInfo.FullName, "specmatic.yaml")))
            {
                testDirectoryInfo = testDirectoryInfo.Parent;
            }
            var projectDir = testDirectoryInfo.Parent ?? throw new InvalidOperationException("Unable to get project directory");
            testDirectory = testDirectoryInfo.FullName;
            apiDirectory = Path.Combine(projectDir.FullName, ProjectName);
            // Start API with test database
            var factory = new ServiceFactory<Program>(postgresContainer);
            // _client = factory.CreateClient();
            _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("http://localhost:8080")
            });
        }

        [Fact]
        public async Task Get_HealthCheck_ShouldReturnSuccess()
        {
            var response = await _client.GetAsync("/uuid");
            response.EnsureSuccessStatusCode();

            Console.WriteLine("[Test] Waiting for 3 minutes to ensure the app has fully started...");
            await Task.Delay(TimeSpan.FromMinutes(1));

            Assert.Equal("", response.StatusCode.ToString());

            // var clientUri = _client.BaseAddress;
            // _baseUrl = clientUri?.ToString() ?? throw new Exception("Failed to determine app base URL.");

            // // Assert.Equal("", _baseUrl");

            await RunContractTests();
            Assert.NotNull(_testContainer);
            var logs = await _testContainer.GetLogsAsync();
            if (!logs.Stdout.Contains("Failures: 0"))
            {
                Assert.Fail("There are failing tests, please refer to build/reports/specmatic/html/index.html for more details");
            }
        }

        private async Task RunContractTests()
        {
            var localReportDirectory = Path.Combine(apiDirectory, "build", "reports");
            Directory.CreateDirectory(localReportDirectory);

            _testContainer = new ContainerBuilder()
                .WithImage("znsio/specmatic")
                .WithCommand("test").WithCommand("--port=8080").WithCommand("--host=host.docker.internal")
                .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
                .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Tests run:"))
                .WithBindMount(localReportDirectory, $"{TestContainerDirectory}/build/reports")
                .WithBindMount($"{testDirectory}/specmatic.yaml", $"{TestContainerDirectory}/specmatic.yaml")
                .WithBindMount($"{testDirectory}/uuid.openapi.yaml", $"{TestContainerDirectory}/uuid.openapi.yaml")
                .Build();

            await _testContainer.StartAsync();
        }
    }
}