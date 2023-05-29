using Azure.Core;
using ImageMaster.Data.Contexts;
using ImageMaster.Data.Entities;
using ImageMaster.DTOs.ImagesDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

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

									// Создание директорий
									string imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Photos",
										imageName.Substring(0, 2), imageName.Substring(2, 2));
									if (!Directory.Exists(imageDirectory))
									{
										Directory.CreateDirectory(imageDirectory);
									}

									// Сохранение изображения в файл
									string imagePath = Path.Combine(imageDirectory, imageName + ".jpg");
									await System.IO.File.WriteAllBytesAsync(imagePath, imageBytes);

									// Сохранение пути к изображению в базу данных
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
	}
}
