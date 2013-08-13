namespace FubuTransportation.Runtime
{
    public interface ISagaRepository<TMessage> where TMessage : class
    {
        void Save<TState>(TState state) where TState : class;
        TState Load<TState>() where TState : class;
        void Delete<TState>(TState state) where TState : class;
    }
}