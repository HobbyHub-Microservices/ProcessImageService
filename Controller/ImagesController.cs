using ImageProcessingService.Models;
using ImageProcessingService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ImageProcessingService.Controller;

[ApiController]
[Route("[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IBlobsManagement _blobManagement;
    private readonly IQueuesManagement _queuesManagement;
    private readonly IConfiguration _config;

    public ImagesController(IBlobsManagement blobsManagement, IQueuesManagement queuesManagement, IConfiguration config)
    {
        _blobManagement = blobsManagement;
        _queuesManagement = queuesManagement;
        _config = config;
    }

    [HttpPost]
    [Route("ImageUpload")]
    public async Task<IActionResult> ImageUpload(IFormFile? img)
    {
        if (img == null)
        {
            Console.WriteLine("--> No image uploaded");
            return BadRequest();
        }

        Console.WriteLine("Uploading image");
        await UploadFile(img, 300, 300);
        return Ok();
    }

    [NonAction]
    private async Task UploadFile(IFormFile img, int width, int height)
    {

        if (img is not {Length: > 0 }) return;
        
        var connection = _config["StorageConfig:BlobConnection"];
        
        byte[]? fileBytes = null;
        MemoryStream? stream = null;
        await using (stream = new MemoryStream())
        {
            await img.CopyToAsync(stream);
            fileBytes = stream.ToArray();
        }

        if (fileBytes == null)
        {
            return;
        }

        var fileExtension = Path.GetExtension(img.FileName);
        
        var name = Path.GetRandomFileName() + "_" + DateTime.UtcNow.ToString("dd/MM/yyyy").Replace("/", "-")+fileExtension;
        
        var url = await _blobManagement.UploadFile("images", name, fileBytes, connection);
        
        await SendMessageToTheQueue(url, name, width, height, "images");
        Console.WriteLine("Image successfully uploaded");
    }

    [NonAction]
    private async Task SendMessageToTheQueue(string imageLocation, string imageName, int width, int height,
        string container)
    {
        var conn = _config["ServiceBus:QueueConnection"];

        ImageResizeDto imgResizeDto = new()
        {
            FileName = imageName,
            Width = width,
            Height = height,
            ImageContainer = container,
            Url = imageLocation
        };

        await _queuesManagement.SendMessage<ImageResizeDto>(imgResizeDto, "imageQueue", conn);

    }

}