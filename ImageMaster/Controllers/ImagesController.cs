using Azure.Core;
using ImageMaster.Data.Contexts;
using ImageMaster.Data.Entities;
using ImageMaster.DTOs.ImagesDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using System;

namespace ImageMaster.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ImagesController : ControllerBase
	{
		private readonly ImagesContext _context;
		private static readonly object _lock = new();

		public ImagesController(ImagesContext context)
		{
			_context = context;
		}

		[HttpPost("upload-by-url")]
		public async Task<IActionResult> UploadByUrl(ImageDTO imageDTO)
		{
			try
			{
				if (!Uri.IsWellFormedUriString(imageDTO.Url, UriKind.Absolute))
				{
					return BadRequest("URI");
				}
				var imageUri = new Uri(imageDTO.Url);
				using var httpClient = new HttpClient(); //Узнать
				byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUri);
				if (imageBytes.Length > (5 * 1024 * 1024))
				{
					return BadRequest("Length");
				}
				var format = Image.DetectFormat(imageBytes);
				using var image = Image.Load(imageBytes);
				string imageDirectory = GetNewImageDirectoryName();
				string imageName = $"{Guid.NewGuid()}.{format.Name}";
				string imagePath = Path.Combine(imageDirectory, imageName);
				lock (_lock)
				{
					Directory.CreateDirectory(imageDirectory);
					image.Save(imagePath);
				}
				ImageEntity imageEntity = new ImageEntity
				{
					Path = imagePath
				};
				_context.ImageEntities.Add(imageEntity);
				await _context.SaveChangesAsync();
				return Ok(new { url = GetImageUrl(imageEntity.Id) });
			}
			catch (UnknownImageFormatException)
			{
				return BadRequest("The image format error");
			}
			catch
			{
				return BadRequest("Other");
			}
		}

		[HttpGet("get-url/{id}")]
		public async Task<IActionResult> GetUrl(int id)
		{
			var imageEntity = await _context.ImageEntities.FindAsync(id);

			if (imageEntity == null)
			{
				return NotFound();
			}

			return Ok(new { url = GetImageUrl(imageEntity.Id) });
		}

		private string GetImageUrl(int id)
		{
			return $"{Request.Scheme}://{Request.Host}/api/images/get-url/{id}";
		}

		private string GetNewImageDirectoryName()
		{
			return Path.Combine(Directory.GetCurrentDirectory(), "Images",
				$"{GetRandomLetter()}{GetRandomLetter()}", $"{GetRandomLetter()}{GetRandomLetter()}");
		}

		private string GetRandomLetter()
		{
			const string letters = "qwertyuiopasdfghjklzxcvbnm";
			var rand = new Random();
			return letters[rand.Next(letters.Length)].ToString();
		}
	}
}
