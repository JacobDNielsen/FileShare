namespace Storage.Interfaces;

public interface IOpenFgaTupleWriter
{
    Task WriteOwnerTupleAsync(string userId, string fileId, CancellationToken cancellationToken = default);
}