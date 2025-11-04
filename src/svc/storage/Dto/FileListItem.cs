namespace Storage.Dto;

public sealed record FileListItem(
    string FileId,
    string BaseFileName,
    long Size,
    DateTimeOffset LastModifiedAt
);
