using Amazon.SQS.Model;

namespace Milo.Messaging.QueueExtendedClient
{
  public interface IAmazonSQSExtendedClient
  {
    public Task<SendMessageResponse> SendMessageAsync(string queueUrl, string messageBody, string bucketName, string prefix = "");
    public Task<ReceiveMessageResponse> ReceiveMessageAsync(ReceiveMessageRequest receiveMessageRequest);
    public Task DeleteMessageAsync(string queueUrl, Message message);
  }
}
