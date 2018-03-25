namespace CardWar.Common.Messaging
{
    public interface IMessageHandler
    {
        object HandleMessage(object message);
    }
}
