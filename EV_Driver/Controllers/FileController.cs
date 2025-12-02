using BusinessObject.Dtos;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace EV_Driver.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileController(IFileService fileService): ControllerBase
{
    [HttpPost("upload/avatar")]
    public async Task<ActionResult<ResponseObject<AvatarPresignResponse>>> AvatarUpload([FromQuery] string fileName)
    {
        var result = await fileService.UploadAvatarAsync(fileName);
        return Ok(new ResponseObject<AvatarPresignResponse>
        {
            Content = result,
            Message = "Create presign url successfully",
            Code = "200",
            Success = true
        });
    }
}