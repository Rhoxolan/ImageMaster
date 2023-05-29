using ImageMaster.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImageMaster.Data.Contexts
{
	public class ImagesContext : DbContext
	{
		public ImagesContext(DbContextOptions<ImagesContext> options) : base(options)
		{
			Database.EnsureCreated();
		}

		public DbSet<ImageEntity> ImageEntities { get; set; }
	}
}
