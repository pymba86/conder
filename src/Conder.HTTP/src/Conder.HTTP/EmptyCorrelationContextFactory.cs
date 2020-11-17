namespace Conder.HTTP
{
    internal class EmptyCorrelationContextFactory : ICorrelationContextFactory
    {
        public string Create() => default;
    }
}