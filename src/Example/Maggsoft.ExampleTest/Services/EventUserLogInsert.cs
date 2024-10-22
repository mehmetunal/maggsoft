using System.Threading.Tasks;
using Maggsoft.Data.Events;
using Maggsoft.ExampleTest.Entity;
using Maggsoft.Services.Events;

namespace Maggsoft.ExampleTest.Services
{
    public class EventUserLogInsert : IConsumer<EntityInsertedEvent<UserLog>>
    {
        public void HandleEvent(EntityInsertedEvent<UserLog> eventMessage)
        {
            throw new System.NotImplementedException();
        }

        public Task HandleEventAsync(EntityInsertedEvent<UserLog> eventMessage)
        {
            throw new System.NotImplementedException();
        }
    }
}
