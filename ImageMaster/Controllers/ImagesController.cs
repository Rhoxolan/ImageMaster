using ImageMaster.Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
	}
}
