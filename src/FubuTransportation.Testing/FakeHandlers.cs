namespace FubuTransportation.Testing
{

    public interface ITargetHandler
    {
        Output OneInOneOut(Input input);
        void OneInZeroOut(Input input);
        object OneInManyOut(Input input);
    }

    public class Input
    {
    }

    public class SpecialInput : Input
    {

    }

    public class Output
    {
    }
}