namespace BusinessObject.Dtos;

public class AvatarPresignResponse
{
    public string Url { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string CloudName { get; set; } = string.Empty;
    public required string Timestamp { get; set; }
    public string Signature { get; set; } = string.Empty;
    public string Folder { get; set; } = string.Empty;
}