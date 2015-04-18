namespace ClassLibrary4
{
    public class MessageEnvelope<T> : Envelope<T>
    {
        public T Message { get; private set; }
        public object UntypedMessage {get {return Message;} }

        public MessageEnvelope(T message)
        {
            Message = message;
        }
    }
}