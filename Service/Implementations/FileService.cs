using System.Net;
using BusinessObject.Dtos;
using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Service.Exceptions;
using Service.Interfaces;

namespace Service.Implementations;

public class FileService: IFileService
{
    private readonly Cloudinary _cloudinary;
    private readonly string _cloudName;
    private readonly string _apiKey;

    public FileService(IConfiguration configuration)
    {
        _cloudName = configuration["CloudinarySettings:CloudName"] ?? string.Empty;
        _apiKey = configuration["CloudinarySettings:ApiKey"] ?? string.Empty;
        var apiSecret = configuration["CloudinarySettings:ApiSecret"] ?? string.Empty;;
        var account = new Account(_cloudName, _apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public Task<AvatarPresignResponse> UploadAvatarAsync(string fileName)
    {
        if (fileName is null or { Length: 0 })
        {
            throw new ValidationException
            {
                ErrorMessage = "File name is required",
                StatusCode = HttpStatusCode.BadRequest,
                Code = "400"
            };
        }

        var parameters = new SortedDictionary<string, object>
        {
            { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { "folder", "profiles" },
            { "tags", "avatar,profile" }
        };

        var signature = _cloudinary.Api.SignParameters(parameters);
        return Task.FromResult(new AvatarPresignResponse
        {
            Url = $"https://api.cloudinary.com/v1_1/{_cloudName}/image/upload",
            Signature = signature,
            Timestamp = parameters["timestamp"].ToString()!,
            Folder = parameters["folder"].ToString()!,
            CloudName = _cloudName,
            Key = _apiKey
        });
    }
}