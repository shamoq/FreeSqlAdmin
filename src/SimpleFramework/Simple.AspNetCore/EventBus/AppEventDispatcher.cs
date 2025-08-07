using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Simple.Utils.Models.Events;

namespace Simple.AspNetCore
{
    /// <summary>app 领域应用内的事件分发</summary>
    public class AppDomainEventDispatcher
    {
        // /// <summary>发送事件</summary>
        // /// <typeparam name="TEvent"></typeparam>
        // /// <param name="event"></param>
        // /// <returns></returns>
        // public static async Task PublishEvent<TEvent>(TEvent @event) where TEvent : INotification
        // {
        //     await _mediator.Publish(@event);
        // }
        
        /// <summary>发送事件</summary>
        /// <returns></returns>
        public static async Task PublishEvent<TArg>(TArg arg) where TArg : IEventArg
        {
            // 尝试从当前 HTTP 请求作用域获取 IMediator
            var requestMediator = HostServiceExtension.CurrentHttpContext?.RequestServices.GetService<IMediator>();
    
            if (requestMediator != null)
            {
                await requestMediator.Publish(new AppDomainEvent<TArg>(arg));
            }
            else
            {
                // 回退方案：创建新作用域
                using var scope = HostServiceExtension.ServiceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Publish(new AppDomainEvent<TArg>(arg));
            }
        }
    }

    /// <summary>App领域事件 用于传递 Vaule</summary>
    /// <typeparam name="T"></typeparam>
    public class AppDomainEvent<T> : INotification
    {
        /// <summary>App领域事件 用于传递 Vaule</summary>
        /// <param name="name">事件名称</param>
        /// <param name="value">事件值</param>
        public AppDomainEvent(T value, string name = "")
        {
            Name = name;
            Value = value;
        }

        /// <summary>事件参数</summary>
        public T Value { get; set; }

        /// <summary>事件名称</summary>
        public string Name { get; set; }

        /// <summary>事件时间</summary>
        public DateTime EventTime { get; } = DateTime.Now;
    }
}