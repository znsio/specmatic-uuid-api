﻿using System.Diagnostics;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace specmatic_uuid_api_test.contract
{
    public class ContractTest : IAsyncLifetime, IClassFixture<PostgresTestContainer>
    {
        private IContainer? _specmaticTestContainer;
        private readonly Process _uuidServiceProcess;
        private const string ProjectName = "specmatic-uuid-api";
        private const string TestContainerDirectory = "/usr/src/app";
        private readonly string apiDirectory;
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
            _uuidServiceProcess.Start();
            Console.WriteLine("UUID service started on port 8080");
        }

        public async Task DisposeAsync()
        {
            var pid = GetProcessIdByPort(8080);
            if (pid > 0)
            {
                var process =  Process.GetProcessById(_uuidServiceProcess.Id);
                process.Kill();
                process.Dispose();
            }
            _uuidServiceProcess.Kill();
            _uuidServiceProcess.Dispose();
            if (_specmaticTestContainer != null)
            { 
                await _specmaticTestContainer.StopAsync(); 
                await _specmaticTestContainer.DisposeAsync();
            }
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

            _uuidServiceProcess = UuidServiceProcess();
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

        private Process UuidServiceProcess()
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

        private static int GetProcessIdByPort(int port)
        {
            var fileName = OperatingSystem.IsWindows() ? "netstat" : "lsof";
            var arguments = OperatingSystem.IsWindows() 
                ? $"-ano | findstr :{port}" 
                : $"-i :{port} -t";

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            var output = process?.StandardOutput.ReadToEnd().Trim();
            process?.WaitForExit();

            return int.TryParse(output, out int pid) ? pid : -1;
        }
    }
}
