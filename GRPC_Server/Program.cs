using System;
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;

namespace GRPC_Server
{
    class GreeterImpl : Greeter.GreeterBase
    {
        // Server side handler of the SayHello RPC
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            Console.WriteLine("Received Message: " + request.Name);
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name,
                Date = DateTime.UtcNow.ToString()
            });
        }
    }

    class Program
    {
        const int Port = 50051;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the AWS SQS Demo!");
            Server server = new Server
            {
                Services = { Greeter.BindService(new GreeterImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
