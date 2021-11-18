using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Amazon.CloudFormation;
using Amazon.CloudWatchLogs;
using Amazon.Lambda;
using Amazon.S3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestServerlessApp.IntegrationTests.Helpers;
using Xunit;

namespace TestServerlessApp.IntegrationTests
{
    public class IntegrationTestContextFixture : IDisposable
    {
        private readonly string _stackName;
        private readonly string _bucketName;
        private readonly CloudFormationHelper _cloudFormationHelper;
        private readonly S3Helper _s3Helper;

        public readonly LambdaHelper LambdaHelper;
        public readonly CloudWatchHelper CloudWatchHelper;
        public readonly string RestApiUrlPrefix;
        public readonly string HttpApiUrlPrefix;
        public readonly List<LambdaFunction> LambdaFunctions;
        public readonly HttpClient HttpClient;

        public IntegrationTestContextFixture()
        {
            DeployTestServerlessApp().GetAwaiter().GetResult();

            _stackName = GetStackName();
            _bucketName = GetBucketName();
            Assert.False(string.IsNullOrEmpty(_stackName));
            Assert.False(string.IsNullOrEmpty(_bucketName));

            _cloudFormationHelper = new CloudFormationHelper(new AmazonCloudFormationClient());
            _s3Helper = new S3Helper(new AmazonS3Client());
            LambdaHelper = new LambdaHelper(new AmazonLambdaClient());
            CloudWatchHelper = new CloudWatchHelper(new AmazonCloudWatchLogsClient());
            RestApiUrlPrefix = _cloudFormationHelper.GetOutputValueAsync(_stackName, "RestApiURL").GetAwaiter().GetResult();
            HttpApiUrlPrefix = _cloudFormationHelper.GetOutputValueAsync(_stackName, "HttpApiURL").GetAwaiter().GetResult();
            LambdaFunctions = LambdaHelper.FilterByCloudFormationStackAsync(_stackName).GetAwaiter().GetResult();
            HttpClient = new HttpClient();

            Assert.Equal(StackStatus.CREATE_COMPLETE, _cloudFormationHelper.GetStackStatusAsync(_stackName).GetAwaiter().GetResult());
            Assert.True(_s3Helper.BucketExistsAsync(_bucketName).GetAwaiter().GetResult());
            Assert.Equal(11, LambdaFunctions.Count);
            Assert.False(string.IsNullOrEmpty(RestApiUrlPrefix));
            Assert.False(string.IsNullOrEmpty(RestApiUrlPrefix));
        }

        private async Task DeployTestServerlessApp()
        {
            var scriptPath = Path.Combine("..", "..", "..", "DeploymentScript.ps1");
            var command = $"pwsh {scriptPath}";
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "pwsh",
                Arguments = scriptPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var process = Process.Start(processStartInfo);
            if (null == process)
                throw new Exception("Process.Start failed to return a non-null process");

            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            while (true)
            {
                if (process.HasExited)
                {
                    // In some cases, process might have exited but OutputDataReceived or ErrorDataReceived could still be writing
                    // asynchronously, adding a delay should cover most of the cases.
                    await Task.Delay(TimeSpan.FromSeconds(1), default);
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50), default);
            }
        }

        private string GetStackName()
        {
            var filePath = Path.Combine("..", "..", "..", "..", "TestServerlessApp", "aws-lambda-tools-defaults.json");
            var token = JObject.Parse(File.ReadAllText(filePath))["stack-name"];
            return token.ToObject<string>();
        }

        private string GetBucketName()
        {
            var filePath = Path.Combine("..", "..", "..", "..", "TestServerlessApp", "aws-lambda-tools-defaults.json");
            var token = JObject.Parse(File.ReadAllText(filePath))["s3-bucket"];
            return token.ToObject<string>();
        }

        private async Task CleanUpAsync()
        {
            await _cloudFormationHelper.DeleteStackAsync(_stackName);
            Assert.True(await _cloudFormationHelper.IsDeletedAsync(_stackName), $"The stack '{_stackName}' still exists and will have to be manually deleted from the AWS console.");

            await _s3Helper.DeleteBucketAsync(_bucketName);
            Assert.False(await _s3Helper.BucketExistsAsync(_bucketName), $"The bucket '{_bucketName}' still exists and will have to be manually deleted from the AWS console.");

            var filePath = Path.Combine("..", "..", "..", "..", "TestServerlessApp", "aws-lambda-tools-defaults.json");
            var token = JObject.Parse(await File.ReadAllTextAsync(filePath));
            token["s3-bucket"] = "test-serverless-app";
            token["stack-name"] = "test-serverless-app";
            await File.WriteAllTextAsync(filePath, token.ToString(Formatting.Indented));
        }

        public void Dispose()
        {
            CleanUpAsync().GetAwaiter().GetResult();
        }
    }
}