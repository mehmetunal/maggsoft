using Maggsoft.Data.Events;
using Maggsoft.ExampleTest.Entity;
using Maggsoft.Services.Events;
using System.Threading.Tasks;

namespace Maggsoft.ExampleTest.Services.Event
{
    public class EventUser : IConsumer<EntityInsertedEvent<User>>,
        IConsumer<EntityUpdatedEvent<User>>,
        IConsumer<EntityDeletedEvent<User>>
    {
        #region Fields

        protected readonly IUserService _userService;

        #endregion

        #region Ctor

        public EventUser(IUserService userService)
        {
            _userService = userService;
        }

        #endregion

        #region Methods

        public void HandleEvent(EntityInsertedEvent<User> eventMessage)
        {
            if (eventMessage.Entity is null)
                return;

            //database işlemi

            throw new System.NotImplementedException();
        }

        public async Task HandleEventAsync(EntityInsertedEvent<User> eventMessage)
        {
            if (eventMessage.Entity is null)
                return;

            //database işlemi

            throw new System.NotImplementedException();
        }


        public void HandleEvent(EntityUpdatedEvent<User> eventMessage)
        {
            if (eventMessage.Entity is null)
                return;

            //database işlemi

            throw new System.NotImplementedException();
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<User> eventMessage)
        {
            if (eventMessage.Entity is null)
                return;

            //database işlemi

            throw new System.NotImplementedException();
        }


        public void HandleEvent(EntityDeletedEvent<User> eventMessage)
        {
            if (eventMessage.Entity is null)
                return;

            //database işlemi

            throw new System.NotImplementedException();
        }

        public async Task HandleEventAsync(EntityDeletedEvent<User> eventMessage)
        {
            if (eventMessage.Entity is null)
                return;

            //database işlemi

            throw new System.NotImplementedException();
        }
        
        #endregion
    }
}