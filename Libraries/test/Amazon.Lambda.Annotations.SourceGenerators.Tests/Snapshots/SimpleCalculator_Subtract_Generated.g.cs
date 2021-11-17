﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

namespace TestServerlessApp
{
    public class SimpleCalculator_Subtract_Generated
    {
        private readonly ServiceProvider serviceProvider;

        public SimpleCalculator_Subtract_Generated()
        {
            SetExecutionEnvironment();
            var services = new ServiceCollection();

            // By default, Lambda function class is added to the service container using the singleton lifetime
            // To use a different lifetime, specify the lifetime in Startup.ConfigureServices(IServiceCollection) method.
            services.AddSingleton<SimpleCalculator>();

            var startup = new TestServerlessApp.Startup();
            startup.ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        public Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse Subtract(Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest request, Amazon.Lambda.Core.ILambdaContext context)
        {
            // Create a scope for every request,
            // this allows creating scoped dependencies without creating a scope manually.
            using var scope = serviceProvider.CreateScope();
            var simpleCalculator = scope.ServiceProvider.GetRequiredService<SimpleCalculator>();

            var x = default(int);
            if (request.Headers?.ContainsKey("x") == true)
            {
                x = (int)Convert.ChangeType(request.Headers["x"], typeof(int));
            }

            var y = default(int);
            if (request.Headers?.ContainsKey("y") == true)
            {
                y = (int)Convert.ChangeType(request.Headers["y"], typeof(int));
            }

            var simpleCalculatorService = scope.ServiceProvider.GetRequiredService<TestServerlessApp.Services.ISimpleCalculatorService>();
            var response = simpleCalculator.Subtract(x, y, simpleCalculatorService);
            return response;
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