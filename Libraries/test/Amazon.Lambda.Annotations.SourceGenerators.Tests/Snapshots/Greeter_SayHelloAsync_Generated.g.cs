using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Amazon.Lambda.Annotations;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

namespace TestServerlessApp
{
    public class Greeter_SayHelloAsync_Generated
    {
        private readonly Greeter greeter;

        public Greeter_SayHelloAsync_Generated()
        {
            SetExecutionEnvironment();
            greeter = new Greeter();
        }

        public async Task<Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse> SayHelloAsync(Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest request, Amazon.Lambda.Core.ILambdaContext context)
        {
            var firstNames = default(System.Collections.Generic.IEnumerable<string>);
            if (request.MultiValueHeaders?.ContainsKey("names") == true)
            {
                firstNames = request.MultiValueHeaders["names"].Select(q => (string)Convert.ChangeType(q, typeof(string))).ToList();
            }

            await greeter.SayHelloAsync(firstNames);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200
            };
        }

        private static void SetExecutionEnvironment()
        {
            const string envName = "AWS_EXECUTION_ENV";
            const string amazonLambdaAnnotations = "amazon-lambda-annotations";

            var assemblyVersion = typeof(LambdaFunctionAttribute).Assembly
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .FirstOrDefault()
                as AssemblyInformationalVersionAttribute;

            var envValue = new StringBuilder();

            // If there is an existing execution environment variable add the annotations package as a suffix.
            if(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(envName)))
            {
                envValue.Append($"{Environment.GetEnvironmentVariable(envName)}_");
            }

            envValue.Append($"{amazonLambdaAnnotations}_{assemblyVersion?.InformationalVersion}");

            Environment.SetEnvironmentVariable(envName, envValue.ToString());
        }
    }
}