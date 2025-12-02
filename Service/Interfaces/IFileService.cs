using BusinessObject.Dtos;

namespace Service.Interfaces;

public interface IFileService
{
    Task<AvatarPresignResponse> UploadAvatarAsync(string fileName);
}