namespace SunSpec.Models.Generated.Server;

public interface IServerModelBuilder
{
    int Build(Memory<byte> buffer);
}

public interface IServerModelBuilder<T> : IServerModelBuilder
    where T : IServerModel<T>
{
    T Model { get; }
}