public interface IGuidService
{
    Guid GetGuid();
}

public class ScopedGuidService : IGuidService
{
    private readonly Guid _guid;
    public ScopedGuidService()
    {
        _guid = Guid.NewGuid();
    }

    public Guid GetGuid() => _guid;
}

public class TransientGuidService : IGuidService
{
    private readonly Guid _guid;
    public TransientGuidService()
    {
        _guid = Guid.NewGuid();
    }

    public Guid GetGuid() => _guid;
}

public class SingletonGuidService : IGuidService
{
    private readonly Guid _guid;
    public SingletonGuidService()
    {
        _guid = Guid.NewGuid();
    }

    public Guid GetGuid() => _guid;
}
public interface ITestService
{
    Guid GetScopedGuid();
    Guid GetTransientGuid();
    Guid GetSingletonGuid();
}
public class FirstTestService : ITestService
{
    private readonly ScopedGuidService _scoped1;
    private readonly TransientGuidService _transient1;
    private readonly SingletonGuidService _singleton1;

    public FirstTestService(ScopedGuidService scoped1, TransientGuidService transient1, SingletonGuidService singleton1)
    {
        _scoped1 = scoped1;
        _transient1 = transient1;
        _singleton1 = singleton1;
    }
    public Guid GetScopedGuid()
    {
        return _scoped1.GetGuid();
    }
    public Guid GetTransientGuid()
    {
        return _transient1.GetGuid();
    }
    public Guid GetSingletonGuid()
    {
        return _singleton1.GetGuid();
    }

}

public class SecondTestService : ITestService
{
    private readonly ScopedGuidService _scoped1;
    private readonly TransientGuidService _transient1;
    private readonly SingletonGuidService _singleton1;

    public SecondTestService(ScopedGuidService scoped1, TransientGuidService transient1, SingletonGuidService singleton1)
    {
        _scoped1 = scoped1;
        _transient1 = transient1;
        _singleton1 = singleton1;
    }
    public Guid GetScopedGuid()
    {
        return _scoped1.GetGuid();
    }
    public Guid GetTransientGuid()
    {
        return _transient1.GetGuid();
    }
    public Guid GetSingletonGuid()
    {
        return _singleton1.GetGuid();
    }
    
}