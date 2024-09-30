using Amazon.S3;
using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using Milo.Messaging.QueueExtendedClient;

public static class ServiceCollectionExtension
{
  public static ServiceCollection AddExtendedQueueClient(this ServiceCollection services)
  {
    services.AddSingleton<IAmazonS3, AmazonS3Client>();
    services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
    services.AddSingleton<IAmazonSQSExtendedClient, AmazonSQSExtendedClient>();
    return services;
  }
}