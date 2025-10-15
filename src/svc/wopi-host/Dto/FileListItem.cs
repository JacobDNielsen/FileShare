namespace WopiHost.Dto;

public sealed record FileListItem(
    string FileId,
    string FileName,
    long Size,
    DateTimeOffset LastModifiedAt
);
