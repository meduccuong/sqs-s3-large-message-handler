using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Milo.Messaging.QueueExtendedClient;

var services = new ServiceCollection();
services.AddExtendedQueueClient();

var serviceProvider = services.BuildServiceProvider();

var sqsClient = serviceProvider.GetRequiredService<IAmazonSQSExtendedClient>();


const string BucketName = "my-bucket-6b4584ad-9476-4ad8-30cb-7ebdecd63a44";
const string QueueUrl = "https://sqs.ap-southeast-2.amazonaws.com/221912726255/my-queue-bed24642-87bb-15ae-a65c-a110671fad86";

var message = new string('*', 500000);

await sqsClient.SendMessageAsync(QueueUrl, message, BucketName, "_prefix");
var response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
{
  QueueUrl = QueueUrl,
  MaxNumberOfMessages = 10,
  VisibilityTimeout = 10
});

foreach (var msg in response.Messages)
{
  Console.WriteLine(msg.Body);
  await sqsClient.DeleteMessageAsync(QueueUrl, msg);
}