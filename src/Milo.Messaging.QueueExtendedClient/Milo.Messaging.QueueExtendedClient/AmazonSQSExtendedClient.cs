using Amazon.SQS.Model;
using Amazon.SQS;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;

namespace Milo.Messaging.QueueExtendedClient
{
  public class AmazonSQSExtendedClient : IAmazonSQSExtendedClient
  {
    public const int MaxLengthAllowed = 200000;
    public const string S3ObjectKey = "__S3ObjectKey__";
    public const string S3BucketName = "__S3BucketName__";

    public IAmazonSQS SQSClient { get; }
    public IAmazonS3 S3Client { get; }

    public AmazonSQSExtendedClient(IAmazonSQS sqsClient, IAmazonS3 s3Client)
    {
      SQSClient = sqsClient;
      S3Client = s3Client;
    }

    public async Task<SendMessageResponse> SendMessageAsync(string queueUrl, string messageBody, string bucketName, string prefix = "")
    {
      var sendMessageRequest = new SendMessageRequest
      {
        QueueUrl = queueUrl
      };


      if (messageBody.Length < MaxLengthAllowed)
      {
        sendMessageRequest.MessageBody = messageBody;
      }
      else
      {
        var s3ObjectKey = await SaveToS3Async(bucketName, prefix, messageBody);

        var messageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    S3ObjectKey, new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = s3ObjectKey
                    }
                },
                {
                    S3BucketName, new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = bucketName
                    }
                }
            };
        sendMessageRequest.MessageAttributes = messageAttributes;
        sendMessageRequest.MessageBody = "The message content is saved to S3 bucket.";
      }




      return await SQSClient.SendMessageAsync(sendMessageRequest);
    }

    public async Task<ReceiveMessageResponse> ReceiveMessageAsync(ReceiveMessageRequest receiveMessageRequest)
    {
      receiveMessageRequest.MessageAttributeNames.Add(S3ObjectKey);
      receiveMessageRequest.MessageAttributeNames.Add(S3BucketName);
      ReceiveMessageResponse receiveMessageResponse = await SQSClient.ReceiveMessageAsync(receiveMessageRequest);

      foreach (var message in receiveMessageResponse.Messages)
      {
        var extendedMessage = await FromMessageAsync(message);
        message.Body = extendedMessage.Body;
      }
      return receiveMessageResponse;
    }

    public async Task DeleteMessageAsync(string queueUrl, Message message)
    {
      var deleteMessageRequest = new DeleteMessageRequest
      {
        QueueUrl = queueUrl,
        ReceiptHandle = message.ReceiptHandle
      };
      await SQSClient.DeleteMessageAsync(deleteMessageRequest);

      if (message.MessageAttributes.ContainsKey(S3ObjectKey) && message.MessageAttributes.ContainsKey(S3BucketName))
      {
        var objectKey = message.MessageAttributes[S3ObjectKey].StringValue;
        var bucketName = message.MessageAttributes[S3BucketName].StringValue;
        var deleteObjectRequest = new DeleteObjectRequest
        {
          BucketName = bucketName,
          Key = objectKey
        };
        await S3Client.DeleteObjectAsync(deleteObjectRequest);
      }
    }

    public async Task<Message> FromMessageAsync(Message message)
    {
      if (message.MessageAttributes.ContainsKey(S3ObjectKey) && message.MessageAttributes.ContainsKey(S3BucketName))
      {
        var objectKey = message.MessageAttributes[S3ObjectKey].StringValue;
        var bucketName = message.MessageAttributes[S3BucketName].StringValue;
        var content = await GetFromS3Async(bucketName, objectKey);
        message.Body = content;
      }
      return message;
    }

    #region Helper methods
    private async Task<string> SaveToS3Async(string bucketName, string prefix, string stringData)
    {
      var objectKey = $"{prefix}{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid().ToString()}.json";
      var stringStream = new MemoryStream(Encoding.UTF8.GetBytes(stringData));

      var putRequest = new PutObjectRequest
      {
        BucketName = bucketName,
        Key = objectKey,
        InputStream = stringStream,
        ContentType = "application/json"
      };

      await S3Client.PutObjectAsync(putRequest);
      return objectKey;
    }

    private async Task<string> GetFromS3Async(string bucketName, string objectKey)
    {
      var getRequest = new GetObjectRequest
      {
        BucketName = bucketName,
        Key = objectKey
      };

      using (var getResponse = await S3Client.GetObjectAsync(getRequest))
      using (var responseStream = getResponse.ResponseStream)
      using (var reader = new StreamReader(responseStream))
      {
        return await reader.ReadToEndAsync();
      }
    }
    #endregion


  }
}
