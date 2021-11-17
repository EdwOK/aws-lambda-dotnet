using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

namespace TestServerlessApp
{
    public class ComplexCalculator_Add_Generated
    {
        private readonly ComplexCalculator complexCalculator;

        public ComplexCalculator_Add_Generated()
        {
            SetExecutionEnvironment();
            complexCalculator = new ComplexCalculator();
        }

        public Amazon.Lambda.APIGatewayEvents.APIGatewayHttpApiV2ProxyResponse Add(Amazon.Lambda.APIGatewayEvents.APIGatewayHttpApiV2ProxyRequest request, Amazon.Lambda.Core.ILambdaContext context)
        {
            var complexNumbers = request.Body;

            var response = complexCalculator.Add(complexNumbers, context, request);

            var body = System.Text.Json.JsonSerializer.Serialize(response);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = body,
                Headers = new Dictionary<string, string>
                {
                    {"Content-Type", "application/json"}
                },
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