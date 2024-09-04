
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using System.Runtime.Serialization;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace photo_bot_back_end.Sql
{
    public class ImgurService
    {
        private const string UPLOAD_ACCESS_TOKEN = "80062daa63e17dcb2efa50948fab2f21d45ae12a";
        private const string THUMBNAIL_ACCESS_TOKEN = "c63057d51d1e22fa2ae10ce6b483bf60d4a71afa";
        private const string THUMBAILS_ALBUM_ID = "QWoRFt7";
        private const string IMGUR_ALBUM_PATH = "https://api.imgur.com/3/album";
        private const string IMGUR_PHOTO_PATH = "https://api.imgur.com/3/image";
        private const int THUMBNAIL_SIZE_PIXELS = 300;

        private Font font = SystemFonts.CreateFont("Arial", 24);

        private readonly ILogger<ImgurService> logger;

        public ImgurService(ILogger<ImgurService> logger)
        {
            this.logger = logger;
        }

        public async Task<ImgurAlbumReply> CreateAlbum(string name)
        {
            using var content = new MultipartFormDataContent();
            var byteArray = CreateAlbumImage(name);
            content.Add(new StringContent("file"), "type");
            content.Add(new ByteArrayContent(byteArray), "image", "thumbnail.jpg");
            var whiteBoxUpload = await PostContent<ImgurUploadReply>(content, IMGUR_PHOTO_PATH, UPLOAD_ACCESS_TOKEN);

            await Task.Delay(500);

            var obj = new ImgurAlbum(name, DateTime.Now.ToString(), new string[] { whiteBoxUpload.id });
            return await SerializeAndPostContent<ImgurAlbumReply>(obj, IMGUR_ALBUM_PATH, UPLOAD_ACCESS_TOKEN);
        }

        public async Task<ImgurUploadReply> UploadPhoto(string imageUrlDiscord, string imgurAlbumId)
        {
            var obj = new ImgurUpload(imageUrlDiscord, "url", imgurAlbumId);
            return await SerializeAndPostContent<ImgurUploadReply>(obj, IMGUR_PHOTO_PATH, UPLOAD_ACCESS_TOKEN);
        }

        public async Task<ImgurUploadReply> UploadThumbnail(string imageUrlDiscord)
        {
            var byteArray = await CreateThumbnail(imageUrlDiscord);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent("file"), "type");
            content.Add(new StringContent(THUMBAILS_ALBUM_ID), "album");
            content.Add(new ByteArrayContent(byteArray), "image", "thumbnail.jpg");

            return await PostContent<ImgurUploadReply>(content, IMGUR_PHOTO_PATH, THUMBNAIL_ACCESS_TOKEN);
        }

        private async Task<byte[]> CreateThumbnail(string imageUrlDiscord)
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

            image.Mutate(x => x.Resize(newWidth, newHeight));

            using var memoryStream = new MemoryStream();
            image.Save(memoryStream, new JpegEncoder());
            return memoryStream.ToArray();
        }

        private byte[] CreateAlbumImage(string text)
        {
            using var image = new Image<Rgba32>(THUMBNAIL_SIZE_PIXELS, THUMBNAIL_SIZE_PIXELS, Color.White);

            var options = new RichTextOptions(font) 
            {
                Origin = new PointF(10, 10),
            };

            image.Mutate(ctx =>
            {
                ctx.DrawText(options, text, Color.Black);
            });

            using var memoryStream = new MemoryStream();
            image.Save(memoryStream, new JpegEncoder());
            return memoryStream.ToArray();
        }

        private async Task<T> SerializeAndPostContent<T>(object obj, string path, string accessToken)
        {
            var json = JsonSerializer.Serialize(obj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await PostContent<T>(content, path, accessToken);
        }

        private async Task<T> PostContent<T>(HttpContent content, string path, string accessToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            var response = await client.PostAsync(path, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed " + typeof(T).ToString() + " " + response.StatusCode + " : " + errorContent);
                throw new HttpRequestException(errorContent, null, response.StatusCode);
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var jobject = JsonObject.Parse(responseBody);
            if (jobject == null) { throw new SerializationException(); }

            var re = jobject["data"].Deserialize<T>();
            if (re == null) { throw new SerializationException(); }

            return re;
        }
    }
}
