﻿namespace SimpleEventBus;

public interface IEventHandler<in TEvent>  where TEvent : class
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken=default);
}