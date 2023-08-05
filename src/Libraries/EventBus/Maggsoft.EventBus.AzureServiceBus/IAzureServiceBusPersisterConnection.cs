using System;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace Maggsoft.EventBus.AzureServiceBus
{
    public interface IAzureServiceBusPersisterConnection : IDisposable
    {
        ServiceBusClient TopicClient { get; }
        ServiceBusAdministrationClient AdministrationClient { get; }
    }
}
