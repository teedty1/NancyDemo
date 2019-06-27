using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS.Model;
using Nancy;

namespace www.Modules
{
    public class SQSModule : NancyModule
    {
        public SQSModule() : base("/sqs")
        {
            Post("/", async (x, ct) =>
            {
                using (var sqs = new Amazon.SQS.AmazonSQSClient(new StoredProfileAWSCredentials("GovBrands"), RegionEndpoint.USEast1))
                {
                    var resp = await sqs.SendMessageAsync(new SendMessageRequest
                    {
                        QueueUrl = "https://sqs.us-east-1.amazonaws.com/066432702053/demo",
                        MessageAttributes = new Dictionary<string, MessageAttributeValue>
                        {
                            {"Source", new MessageAttributeValue {StringValue = "Nancy", DataType = "String"}}
                        },
                        MessageBody = "Message from the Web at " + DateTime.UtcNow
                    });

                    return resp.MessageId;
                }
            });
        }
    }
}
