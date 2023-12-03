using System.Threading.Tasks;

namespace Maggsoft.EventBus.Abstractions;

public interface IDynamicIntegrationEventHandler
{
    Task Handle(dynamic eventData);
}
