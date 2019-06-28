using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;
using Nancy;
using Nancy.Extensions;

namespace www.Modules
{
    public class GRPCModule : NancyModule
    {
        public GRPCModule(IGRPCClient grpc) : base("/grpc")
        {
            Post("/", async (x, ct) => await grpc.SayHelloAsync(Request.Body.AsString()));
        }
    }

    //Create an interface so Nancy instantiates this as a singleton
    public interface IGRPCClient
    {
        Task<HelloReply> SayHelloAsync(string name);
    }

    public class GRPCClient : IGRPCClient
    {
        private readonly Greeter.GreeterClient client;
        public GRPCClient()
        {
            var channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
            client = new Greeter.GreeterClient(channel);
        }

        public async Task<HelloReply> SayHelloAsync(string name)
        {
            var resp = await client.SayHelloAsync(new HelloRequest { Name = name });
            return resp;
        }
    }
}
