using System;

namespace ClassLibrary4
{
    public class HandlerEntry
    {
        private readonly Type _t;
        private readonly Action<object> _handler;

        public HandlerEntry(Type t, Action<object> handler)
        {
            _t = t;
            _handler = handler;
        }

        public void Execute(object message)
        {
            _handler(message);
        }

        public bool ShouldExecuteFor(Type type)
        {
            return matches(type, _t);
        }

        private static bool matches(Type msg, Type target)
        {
            if (target.IsAssignableFrom(msg)) return true;

            if (target.IsGenericType == false) return false;
            if (msg.IsGenericType == false) return false;

            if (target.GetGenericTypeDefinition().IsAssignableFrom(msg.GetGenericTypeDefinition()) == false) return false;

            var targetTypeParams = target.GetGenericParameterConstraints();
            var msgTypeParams = msg.GetGenericParameterConstraints();

            if (targetTypeParams.Length != msgTypeParams.Length) return false;

            for (int i = 0; i < targetTypeParams.Length; i++)
            {
                if (!matches(targetTypeParams[i], msgTypeParams[i])) return false;
            }

            return true;
        }
    }
}