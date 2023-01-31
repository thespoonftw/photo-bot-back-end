using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace photo_bot_back_end.Services
{
    public class ThumbnailService
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ILogger<ThumbnailService> logger;

        public ThumbnailService(ILogger<ThumbnailService> logger, IWebHostEnvironment webHostEnvironment)
        {
            this.logger = logger;
            this.webHostEnvironment = webHostEnvironment;
        }

        public async void SaveThumbnail(int id, string url)
        {
            using var client = new HttpClient();
            using var stream = await client.GetStreamAsync(url);
            using var image = Image.Load(stream);

            var newWidth = 300;
            var newHeight = 300;

            if (image.Width > image.Height)
            {
                newHeight = (image.Height * 300) / image.Width;
            }
            else
            {
                newWidth = (image.Width * 300) / image.Height;
            }

            image.Mutate(x => x.Resize(newWidth, newHeight));
            image.Save(GetThumbnailPath(id));
        }

        public bool IsThumbnailExisting(int id)
        {
            using var client = new HttpClient();
            try
            {
                var image = Image.Load(GetThumbnailPath(id));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetThumbnailPath(int id)
        {
            return $"{webHostEnvironment.WebRootPath}/thumbnails/{id}.jpg";
        }
    }
}
