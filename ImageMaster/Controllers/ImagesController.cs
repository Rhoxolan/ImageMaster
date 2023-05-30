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

		public ImagesController(ImagesContext context)
		{
			_context = context;
		}

		[HttpPost("upload-by-url")]
		public async Task<IActionResult> UploadByUrl(ImageDTO imageDTO)
		{
			try
			{
				var imageUrl = new Uri(imageDTO.Url);
				using var httpClient = new HttpClient(); //Узнать
				byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
				if (imageBytes.Length > (5 * 1024 * 1024))
				{
					return BadRequest("Length");
				}
				var format = Image.DetectFormat(imageBytes);
				if (format == null)
				{
					return BadRequest("Format");
				}
				using var image = Image.Load(imageBytes);
				string imageDirectory = GetNewImageDirectoryName();
				if (!Directory.Exists(imageDirectory))
				{
					Directory.CreateDirectory(imageDirectory);
				}
				string imageName = Guid.NewGuid().ToString() + "." + format.Name;
				string imagePath = Path.Combine(imageDirectory, imageName);
				image.Save(imagePath);
				ImageEntity imageEntity = new ImageEntity
				{
					Path = imagePath
				};
				_context.ImageEntities.Add(imageEntity);
				await _context.SaveChangesAsync();
				return Ok(new { url = GetImageUrl(imageEntity.Id) });
			}
			catch
			{
				return BadRequest("Other");
			}
		}

		[HttpPost("upload-by-url-old")]
		public async Task<IActionResult> UploadByUrlOld(ImageDTO imageDTO)
		{
			try
			{
				Uri imageUrl = new Uri(imageDTO.Url);

				// Загрузка изображения из URL
				using (var httpClient = new System.Net.Http.HttpClient())
				{
					using (var httpResponse = await httpClient.GetAsync(imageUrl))
					{
						if (httpResponse.IsSuccessStatusCode)
						{
							using (var stream = await httpResponse.Content.ReadAsStreamAsync())
							{
								// Чтение изображения в массив байтов
								using (var memoryStream = new MemoryStream())
								{
									await stream.CopyToAsync(memoryStream);
									byte[] imageBytes = memoryStream.ToArray();

									// Генерация уникального имени файла
									string imageName = Guid.NewGuid().ToString();


									string imageDirectory = GetNewImageDirectoryName();
									if (!Directory.Exists(imageDirectory))
									{
										Directory.CreateDirectory(imageDirectory);
									}

									// Сохранение изображения в файл
									string imagePath = Path.Combine(imageDirectory, imageName + ".jpg");
									await System.IO.File.WriteAllBytesAsync(imagePath, imageBytes);

									ImageEntity imageEntity = new ImageEntity
									{
										Path = imagePath
									};
									_context.ImageEntities.Add(imageEntity);
									await _context.SaveChangesAsync();

									return Ok(new { url = GetImageUrl(imageEntity.Id) });
								}
							}
						}
						else
						{
							return BadRequest("Failed to download image from the specified URL.");
						}
					}
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
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
