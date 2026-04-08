

public interface IOpenFgaService
{
    Task<bool> CanReadFileAsync(string userId, string fileId, CancellationToken ct = default);
    Task<bool> CanWriteFileAsync(string userId, string fileId, CancellationToken ct = default);
    Task<bool> CanDeleteFileAsync(string userId, string fileId, CancellationToken ct = default);
    Task<bool> CanUploadToPatientAsync(string userId, string patientId, CancellationToken ct = default);

}