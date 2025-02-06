using System;
using System.Diagnostics;
using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;

namespace specmatic_uuid_api_test.contract
{
    public class ContractTest : IAsyncLifetime
    {
        private DotNet.Testcontainers.Containers.IContainer? _testContainer;
        private PostgreSqlContainer _pgContainer;
        private readonly Process _uuidServiceProcess;

        private const string ProjectName = "specmatic-uuid-api";
        private const string TestContainerDirectory = "/usr/src/app";
        private readonly string apiDirectory;
        private readonly string testDirectory;


        public async Task InitializeAsync()
        {
            await _pgContainer.StartAsync();
            Console.WriteLine($"Database started on port 5432");
            _uuidServiceProcess.Start();
            Console.WriteLine("UUID service started on port 8080");
        }

        public async Task DisposeAsync()
        {
            await _pgContainer.DisposeAsync();
            _uuidServiceProcess.Kill();
            if (_testContainer != null) await _testContainer.DisposeAsync();
        }

        public ContractTest()
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

            _pgContainer = new PostgreSqlBuilder()
                .WithImage("postgres:17")
                .WithDatabase("specmatic_uuid_db")
                .WithPortBinding(5432)
                .WithExposedPort(5432)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .WithUsername("dotnet")
                .WithPassword("dotNet1234")
                .Build();

            _uuidServiceProcess = StartUuidService();
        }

        [Fact]
        public async Task ContractTestsAsync()
        {
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

        private Process StartUuidService()
        {
            var appProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project {apiDirectory}/{ProjectName}.csproj",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            return appProcess;
        }
    }
}
