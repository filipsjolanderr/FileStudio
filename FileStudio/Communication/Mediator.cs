using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace FileStudio.Communication
{
    /// <summary>
    /// Concrete implementation of the IMediator interface.
    /// </summary>
    public class Mediator(IServiceProvider serviceProvider) : IMediator
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly Dictionary<Type, Func<object>> _requestHandlers = new();
        private readonly Dictionary<Type, List<Func<object>>> _notificationHandlers = new();
        public Task<TResponse> SendAsync<TResponse>(object request)
        {
            var requestType = request.GetType();

            if (!_requestHandlers.TryGetValue(requestType, out var handlerFactory))
            {
                throw new InvalidOperationException($"No handler registered for request type {requestType.Name}");
            }

            var handler = handlerFactory();

            // Use reflection to find the HandleAsync method with the correct request type
            var handleMethod = handler.GetType().GetMethod("HandleAsync", new[] { requestType });
            if (handleMethod != null)
            {
                var result = handleMethod.Invoke(handler, new[] { request });
                return (Task<TResponse>)result!;
            }
            else
            {
                throw new InvalidOperationException($"Handler for {requestType.Name} does not have a HandleAsync method accepting {requestType.Name}");
            }
        }

        public async Task PublishAsync(INotification notification)
        {
            var notificationType = notification.GetType();
            if (_notificationHandlers.TryGetValue(notificationType, out var handlerFactories))
            {
                var handlers = handlerFactories.Select(factory => factory()).ToList();
                foreach (var handler in handlers)
                {
                    // Dynamically invoke the HandleAsync method
                    var handleMethod = handler.GetType().GetMethod("HandleAsync", new[] { notificationType });
                    if (handleMethod != null)
                    {
                        await (Task)handleMethod.Invoke(handler, new object[] { notification });
                    }
                    else
                    {
                        // Fallback or error handling if method not found (shouldn't happen with correct registration)
                        throw new InvalidOperationException($"Could not find HandleAsync method on handler {handler.GetType().Name} for notification {notificationType.Name}");
                    }
                }
            }
            // It's okay if no handlers are registered for a notification
        }

        public void RegisterHandler<TRequest, TResponse>(Func<IRequestHandler<TRequest, TResponse>> handlerFactory)
        {
            _requestHandlers[typeof(TRequest)] = () => handlerFactory();
        }

        public void RegisterNotificationHandler<TNotification>(Func<INotificationHandler<TNotification>> handlerFactory)
            where TNotification : INotification
        {
            var notificationType = typeof(TNotification);
            if (!_notificationHandlers.ContainsKey(notificationType))
            {
                _notificationHandlers[notificationType] = new List<Func<object>>();
            }
            _notificationHandlers[notificationType].Add(() => handlerFactory());
        }
    }
}
