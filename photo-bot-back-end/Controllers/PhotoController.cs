using Microsoft.AspNetCore.Mvc;
using photo_bot_back_end;
using photo_bot_back_end.Services;
using System.Text.Json;

namespace photo_bot_backend.Controllers
{
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private record PhotoPost(string url, string channelId);

        private record AlbumPost(string channelId, string name);

        public record AlbumData(Album album, List<Photo> photos);

        private readonly ILogger<PhotoController> logger;
        private readonly SqlService sql;
        private readonly ThumbnailService thumbnails;

        public PhotoController(ILogger<PhotoController> logger, ThumbnailService thumbnails, SqlService sql)
        {
            this.logger = logger;
            this.thumbnails = thumbnails;
            this.sql = sql;
        }

        [HttpGet("album/{id}")]
        public AlbumData GetAlbumData(int id)
        {
            var album = sql.GetAlbum(id);
            var photos = sql.GetPhotos(id);
            return new AlbumData(album, photos);
        }

        [HttpGet("album")]
        public IEnumerable<Album> GetAllAlbums()
        {
            return sql.GetAllAlbums();
        }

        [HttpPost("photo")]
        public async void PostPhoto()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Post photo: {Body}", body);
            var postPhoto = JsonSerializer.Deserialize<PhotoPost>(body);
            if (postPhoto == null) { return; }

            var id = sql.GetNextPhotoId();
            var albumId = sql.GetAlbumId(postPhoto.channelId);
            var photo = new Photo(id, postPhoto.url, albumId);
            sql.AddPhoto(photo);
            thumbnails.SaveThumbnail(id, postPhoto.url);
        }

        [HttpPost("album")]
        public async void PostAlbum()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Post album: {Body}", body);
            var postAlbum = JsonSerializer.Deserialize<AlbumPost>(body);
            if (postAlbum == null) { return; }

            var id = sql.GetNextAlbumId();
            var album = new Album(id, postAlbum.channelId, postAlbum.name, DateTime.Now.Year);
            sql.AddAlbum(album);
        }

        [HttpPost("generatethumbnails/{id}")]
        public void GenerateThumbnails(int id)
        {
            var photos = sql.GetPhotos(id);

            foreach (var photo in photos)
            {
                var isThumbail = thumbnails.IsThumbnailExisting(photo.id);
                if (isThumbail) { continue; }
                thumbnails.SaveThumbnail(photo.id, photo.url);
            }
        }
    }
}