using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary4
{
    public class HandlerResolver
    {
        readonly List<HandlerEntry> _handlers = new List<HandlerEntry>(); 

        public HandlerResolver Register<T>(Action<T> handler)
        {
            _handlers.Add(new HandlerEntry(typeof(T), x => handler((T)x)));
            return this;
        }

        public IEnumerable<HandlerEntry> GetHandlersFor(object message)
        {
            var type = message.GetType();
            var handlers = _handlers.Where(x => x.ShouldExecuteFor(type));
            return handlers;
        }
    }
}