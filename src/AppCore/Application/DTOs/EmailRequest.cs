namespace OrionSoft.AppCore.Application.DTOs;

public sealed class EmailRequest {
    public required string To { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public required string From { get; set; }
    public List<AttachmentFile> AttachmentFiles { get; set; } = [];
}

public sealed class AttachmentFile {
    public required string FileBase64 { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
}
