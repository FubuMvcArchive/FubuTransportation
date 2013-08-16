namespace FubuTransportation.Sagas
{
    public interface ISagaRepository<TState, TMessage>
    {
        void Save(TState state);
        TState Find(TMessage message);
        void Delete(TState state);
    }
}