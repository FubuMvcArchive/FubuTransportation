namespace FubuTransportation.Runtime.Invocation
{
    /// <summary>
    /// Executes the handler chains for a message in context
    /// with the current message
    /// </summary>
    public interface IMessageExecutor
    {
        /// <summary>
        /// Consumes and executes the message within the same
        /// request and transaction context as the currently
        /// executing handler
        /// </summary>
        /// <param name="message"></param>
        void Execute(object message);
    }
}