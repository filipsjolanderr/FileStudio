using System;
using System.Threading.Tasks;

namespace FileStudio.Communication
{
    /// <summary>
    /// Defines the contract for a mediator component.
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Sends a request to be handled by a single handler.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request object.</param>
        /// <returns>A task representing the asynchronous operation, containing the response.</returns>
        Task<TResponse> SendAsync<TResponse>(object request);

        /// <summary>
        /// Sends a notification to be handled by multiple handlers.
        /// </summary>
        /// <param name="notification">The notification object.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task PublishAsync(INotification notification);

        /// <summary>
        /// Registers a handler for a specific request type.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="handlerFactory">A factory function to create the handler instance.</param>
        void RegisterHandler<TRequest, TResponse>(Func<IRequestHandler<TRequest, TResponse>> handlerFactory);

        /// <summary>
        /// Registers a handler for a specific notification type.
        /// </summary>
        /// <typeparam name="TNotification">The type of the notification.</typeparam>
        /// <param name="handlerFactory">A factory function to create the handler instance.</param>
        void RegisterNotificationHandler<TNotification>(Func<INotificationHandler<TNotification>> handlerFactory) where TNotification : INotification;
    }

    /// <summary>
    /// Marker interface for requests that expect a response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface IRequest<out TResponse> { }

    /// <summary>
    /// Marker interface for notifications (fire-and-forget).
    /// </summary>
    public interface INotification { }

    /// <summary>
    /// Defines the contract for a request handler.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface IRequestHandler<in TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request);
    }

    /// <summary>
    /// Defines the contract for a notification handler.
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    public interface INotificationHandler<in TNotification> where TNotification : INotification
    {
        Task HandleAsync(TNotification notification);
    }
}
