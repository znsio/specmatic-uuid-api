using System.Diagnostics;
using System.Text;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using specmatic_uuid_api;
using specmatic_uuid_api.Models;
using specmatic_uuid_api.Models.Entity;

namespace specmatic_uuid_api_test.contract
{
    public class ContractTest : IAsyncLifetime, IClassFixture<PostgresTestContainer>
    {
        private IContainer? _specmaticTestContainer;
        private readonly Process _uuidServiceProcess;
        private const string ProjectName = "specmatic-uuid-api";
        private const string TestContainerDirectory = "/usr/src/app";
        private readonly string apiDirectory;
        private static IHost _host;
        private readonly string testDirectory;

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

        public async Task InitializeAsync()
        {
            _host = Program.BuildApp();
            _ = _host.RunAsync();
            await Task.Delay(50000); 
            Console.WriteLine("UUID service started on port 8080");
        }


        public async Task DisposeAsync()
        {
            await _host.StopAsync();
            if (_specmaticTestContainer != null) await _specmaticTestContainer.DisposeAsync();
            _host?.Dispose();
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
        }

        private async Task RunContractTests()
        {
            var localReportDirectory = Path.Combine(testDirectory, "build", "reports");
            Directory.CreateDirectory(localReportDirectory);

            _specmaticTestContainer = new ContainerBuilder()
                .WithImage("znsio/specmatic")
                .WithCommand("test").WithCommand("--port=8080").WithCommand("--host=host.docker.internal")
                .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
                .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Tests run:"))
                .WithBindMount(localReportDirectory, $"{TestContainerDirectory}/build/reports")
                .WithBindMount($"{testDirectory}/specmatic.yaml", $"{TestContainerDirectory}/specmatic.yaml")
                .WithBindMount($"{testDirectory}/uuid.openapi.yaml", $"{TestContainerDirectory}/uuid.openapi.yaml")
                .Build();

            await _specmaticTestContainer.StartAsync();
        }
    }
}
