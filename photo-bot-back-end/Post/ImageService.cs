using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace photo_bot_back_end.Post
{
    public class ImageService
    {
        private const int THUMBNAIL_SIZE_PIXELS = 300;

        private readonly ILogger<ImageService> logger;

        private readonly string thumbnailDir;
        private readonly string originalDir;

        public ImageService(ILogger<ImageService> logger)
        {
            this.logger = logger;

            originalDir = Path.Combine(AppContext.BaseDirectory, "originals");
            thumbnailDir = Path.Combine(AppContext.BaseDirectory, "thumbnails");
            Directory.CreateDirectory(thumbnailDir);
        }

        public async Task<string> SaveImage(string imageUrlDiscord, int id)
        {
            using var client = new HttpClient();
            using var stream = await client.GetStreamAsync(imageUrlDiscord);
            using var image = Image.Load(stream);

            var newWidth = THUMBNAIL_SIZE_PIXELS;
            var newHeight = THUMBNAIL_SIZE_PIXELS;

            if (image.Width > image.Height)
            {
                newHeight = image.Height * THUMBNAIL_SIZE_PIXELS / image.Width;
            }
            else
            {
                newWidth = image.Width * THUMBNAIL_SIZE_PIXELS / image.Height;
            }

            var format = await Image.DetectFormatAsync(stream);
            var extension = format?.FileExtensions.FirstOrDefault() ?? "jpg";
            var imagePath = id.ToString() + "." + extension;
            var originalPath = Path.Combine(originalDir, imagePath);
            await SaveImage(originalPath, image, format ?? JpegFormat.Instance);

            image.Mutate(x => x.Resize(newWidth, newHeight));
            var thumbnailPath = Path.Combine(thumbnailDir, id.ToString() + ".jpg");
            await SaveImage(thumbnailPath, image, JpegFormat.Instance);

            return imagePath;
        }

        private async Task SaveImage(string filePath, Image image, IImageFormat format)
        {
            await using (var fileStream = File.Create(filePath))
            {
                await image.SaveAsync(fileStream, format);
            }
        }
    }
}
