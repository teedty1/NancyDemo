using System;
using System.Collections.Generic;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS.Model;

namespace AWS_SQS_Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new App();
            app.Run();
        }
    }

    public class App
    {
        public async void Run()
        {
            Console.WriteLine("Welcome to the AWS SQS Demo!");

            var queueURL = "https://sqs.us-east-1.amazonaws.com/066432702053/demo";

            //Credentials should not really be using profile.  This is for development purposes.
            //In Production inside AWS an Instance profile should be used
            using (var sqs = new Amazon.SQS.AmazonSQSClient(new StoredProfileAWSCredentials("GovBrands"), RegionEndpoint.USEast1))
            {
                while (true)
                {
                    //The ReceiveMessageAsync call blocks until it receives a message or 20 seconds has expired
                    var response = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                    {
                        QueueUrl = queueURL,
                        MessageAttributeNames = new List<string> { "*" }
                    });

                    foreach (var message in response.Messages)
                    {
                        Console.WriteLine("{0}: {1} {2}", message.MessageId, message.MessageAttributes["Source"].StringValue, message.Body);
                        await sqs.DeleteMessageAsync(queueURL, message.ReceiptHandle);
                    }
                }
            }
        }

    }
}
