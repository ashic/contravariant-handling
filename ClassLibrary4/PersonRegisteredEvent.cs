namespace ClassLibrary4
{
    public class PersonRegisteredEvent : RegisteredEvent
    {
        public string Id { get; private set; }

        public PersonRegisteredEvent(string id)
        {
            Id = id;
        }
    }
}