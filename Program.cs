using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using OrleansServiceBus.Providers;
using Orleans.Streams;
using Orleans.Providers.Streams.Common;
using System.Diagnostics;
using System.Collections.Generic;

namespace Orleans2GettingStarted
{
    class Program
    {
        public static long num_of_operators = 3;
        static async Task Main(string[] args)
        {

            var siloBuilder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .AddSimpleMessageStreamProvider("Test")
                .AddMemoryGrainStorage("PubSubStore")
                .UseDashboard(options => { options.Port = 8086; })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "Orleans2GettingStarted";
                })
                .Configure<EndpointOptions>(options =>
                    options.AdvertisedIPAddress = IPAddress.Loopback)
                .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.None).AddConsole());

            using (var host = siloBuilder.Build())
            {
                await host.StartAsync();

                var clientBuilder = new ClientBuilder()
                    .UseLocalhostClustering()
                    //.AddSqsStreams("Test", option => option.ConnectionString = "Service=123;AccessKey=AKIAI7DQ33Z2GGKGF6CQ;SecretKey=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;")
                    .AddSimpleMessageStreamProvider("Test")
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "dev";
                        options.ServiceId = "Orleans2GettingStarted";
                    })
                    .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.None).AddConsole());

                using (var client = clientBuilder.Build())
                {
                    await client.Connect();
                       
                    //declearation of the workflow:

                    //1. operator grains
                    var operator_a = client.GetGrain<IOperator>(1);
                    var operator_b = client.GetGrain<IOperator>(2);
                    var operator_c = client.GetGrain<IOperator>(3);

                    //2. layers
                    var layer_a = client.GetGrain<ILayer>(1);
                    var layer_b = client.GetGrain<ILayer>(2);
                    var layer_c = client.GetGrain<ILayer>(3);

                    //3. controllers
                    var controller_a = client.GetGrain<IController>(1);
                    var controller_b = client.GetGrain<IController>(2);
                    var controller_c = client.GetGrain<IController>(3);

                    //initialization of the workflow:

                    //1. initialize operator grains
                    await operator_a.Init(new List<IOperator> { operator_b });
                    await operator_b.Init(new List<IOperator> { operator_c });

                    //2. initialize layers
                    await layer_a.Init(null, new List<IOperator> { operator_a }, controller_a);
                    await layer_b.Init(layer_a, new List<IOperator> { operator_b }, controller_b);
                    await layer_c.Init(layer_b, new List<IOperator> { operator_c }, controller_c);

                    //3. initialize controllers
                    await controller_a.Init(new List<ILayer> { layer_a });
                    await controller_b.Init(new List<ILayer> { layer_b }, controller_a);
                    await controller_c.Init(new List<ILayer> { layer_c }, controller_c);

                    //dynamic node allocation in practice:

                    //1. sending 2 messages to the workflow
                    for (int i = 1; i < 3; ++i)
                        operator_a.DoWork(i);

                    //2. now, adding one more node to layer_b and wait for completion
                    await layer_b.AllocateNode();

                    //3. sending 2 messages to the workflow
                    for (int i = 3; i < 5; ++i)
                        operator_a.DoWork(i);

                    Console.ReadLine();
                }
            }
        }
    }
}