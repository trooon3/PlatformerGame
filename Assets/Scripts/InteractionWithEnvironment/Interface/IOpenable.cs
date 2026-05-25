public interface IOpenable
{
    bool IsClosed { get; }
    void Open();
}