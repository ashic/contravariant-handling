using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace ClassLibrary4
{
    public class HandlerResolverTests

    {
        private readonly HandlerResolver _resolver;

        public HandlerResolverTests()
        {
            _resolver = new HandlerResolver();
        }

        [Fact]
        public void should_handle_exact_type()
        {
            var handled = false;
            _resolver.Register<PersonRegisteredEvent>(x => handled = true);
            execute(new PersonRegisteredEvent("foo"));

            Assert.True(handled);
        }

        [Fact]
        public void should_handle_for_interface_type()
        {
            var handled = false;
            _resolver.Register<RegisteredEvent>(x => handled = true);
            execute(new PersonRegisteredEvent("foo"));

            Assert.True(handled);
        }

        [Fact]
        public void should_handle_for_exact_envelope_type()
        {
            var handled = false;
            _resolver.Register<MessageEnvelope<PersonRegisteredEvent>>(x => handled = true);
            execute(new MessageEnvelope<PersonRegisteredEvent>(new PersonRegisteredEvent("foo")));

            Assert.True(handled);
        }

        [Fact]
        public void should_handle_for_envelope_interface_on_exact_type()
        {
            var handled = false;
            _resolver.Register<Envelope<PersonRegisteredEvent>>(x => handled = true);
            execute(new MessageEnvelope<PersonRegisteredEvent>(new PersonRegisteredEvent("foo")));

            Assert.True(handled);
        }

        [Fact]
        public void should_handle_for_envelope_interface_on_interface_type()
        {
            var handled = false;
            _resolver.Register<Envelope<RegisteredEvent>>(x => handled = true);
            execute(new MessageEnvelope<PersonRegisteredEvent>(new PersonRegisteredEvent("foo")));

            Assert.True(handled);
        }


        [Fact]
        public void should_not_handle_for_envelope_interface_on_different_message_type()
        {
            var handled = false;
            _resolver.Register<Envelope<CancelledEvent>>(x => handled = true);
            execute(new MessageEnvelope<PersonRegisteredEvent>(new PersonRegisteredEvent("foo")));

            Assert.False(handled);
        }

        [Fact]
        public void handlers_shouldnt_freak_each_other_out()
        {
            var handledCancelled = false;
            var handledRegistered = false;
            _resolver.Register<Envelope<CancelledEvent>>(x => handledCancelled = true);
            _resolver.Register<Envelope<RegisteredEvent>>(x => handledRegistered = true);
            execute(new MessageEnvelope<PersonRegisteredEvent>(new PersonRegisteredEvent("foo")));

            Assert.False(handledCancelled);
            Assert.True(handledRegistered);
        }


        [Fact]
        public void all_handlers_for_message_should_trigger()
        {
            var handledInterface = false;
            var handledClass = false;
            _resolver.Register<Envelope<RegisteredEvent>>(x => handledInterface = true);
            _resolver.Register<Envelope<PersonRegisteredEvent>>(x => handledClass = true);

            execute(new MessageEnvelope<PersonRegisteredEvent>(new PersonRegisteredEvent("foo")));

            Assert.True(handledInterface);
            Assert.True(handledClass);
        }

        [Fact]
        public void envelopes_of_derived_messages_should_get_handled()
        {
            var handledInterface = false;
            var handledClass = false;
            _resolver.Register<Envelope<RegisteredEvent>>(x => handledInterface = true);
            _resolver.Register<Envelope<PersonRegisteredEvent>>(x => handledClass = true);

            execute(new MessageEnvelope<CriminalRegisteredEvent>(new CriminalRegisteredEvent("foo")));

            Assert.True(handledInterface);
            Assert.True(handledClass);
        }


        [Fact]
        public void should_work_with_value_types()
        {
            var handled = false;
            _resolver.Register<int>(x => handled = true);

            execute(42);

            Assert.True(handled);
        }

        [Fact]
        public void should_work_with_strings()
        {
            var handled = false;
            _resolver.Register<string>(x => handled = true);

            execute("42");

            Assert.True(handled);
        }

        [Fact]
        public void should_work_with_envelope_of_value_types()
        {
            var handled = false;
            _resolver.Register<Envelope<int>>(x => handled = true);

            execute(new MessageEnvelope<int>(42));

            Assert.True(handled);
        }


        [Fact]
        public void should_work_with_envelope_of_strings()
        {
            var handled = false;
            _resolver.Register<Envelope<string>>(x => handled = true);

            execute(new MessageEnvelope<string>("42"));

            Assert.True(handled);
        }

        [Fact]
        public void should_work_with_enumerables_of_strings()
        {
            var handled = false;
            _resolver.Register<Envelope<IEnumerable<string>>>(x => handled = true);

            execute(new MessageEnvelope<IEnumerable<string>>(new[] { "42" }));

            Assert.True(handled);
        }

        [Fact]
        public void should_work_with_enumerables_of_objects()
        {
            var handled = false;
            _resolver.Register<Envelope<IEnumerable<RegisteredEvent>>>(x => handled = true);

            execute(new MessageEnvelope<IEnumerable<PersonRegisteredEvent>>(new[] { new PersonRegisteredEvent("42") }));

            Assert.True(handled);
        }

        [Fact]
        public void should_maintain_order_with_message_first()
        {
            var queue = new ConcurrentQueue<int>();
            _resolver.Register<Envelope<PersonRegisteredEvent>>(x => queue.Enqueue(1));
            _resolver.Register<Envelope<RegisteredEvent>>(x => queue.Enqueue(2));

            execute(new MessageEnvelope<CriminalRegisteredEvent>(new CriminalRegisteredEvent("foo")));


            Assert.Equal(1, queue.ElementAtOrDefault(0));
            Assert.Equal(2, queue.ElementAtOrDefault(1));
        }

        [Fact]
        public void should_maintain_order_with_message_interface_first()
        {
            var queue = new ConcurrentQueue<int>();
            _resolver.Register<Envelope<RegisteredEvent>>(x => queue.Enqueue(2));
            _resolver.Register<Envelope<PersonRegisteredEvent>>(x => queue.Enqueue(1));

            execute(new MessageEnvelope<CriminalRegisteredEvent>(new CriminalRegisteredEvent("foo")));


            Assert.Equal(2, queue.ElementAtOrDefault(0));
            Assert.Equal(1, queue.ElementAtOrDefault(1));
        }

        [Fact]
        public void should_handle_for_untyped_envelope_interface()
        {
            var handled = false;
            _resolver.Register<Envelope>(x => handled = true);

            execute(new MessageEnvelope<CriminalRegisteredEvent>(new CriminalRegisteredEvent("foo")));

            Assert.True(handled);
        }

        private void execute(object message)
        {
            var handlers = _resolver.GetHandlersFor(message);
            foreach (var handlerEntry in handlers)
            {
                handlerEntry.Execute(message);
            }
        }
    }
}
