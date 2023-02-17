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
        public async Task<AlbumData?> GetAlbumData(int id)
        {
            var album = await sql.GetAlbum(id);
            if (album == null) return null;
            var photos = await sql.GetPhotos(id);
            return new AlbumData(album, photos);
        }

        [HttpGet("album")]
        public async Task<IEnumerable<Album>> GetAllAlbums()
        {
            return await sql.GetAllAlbums();
        }

        [HttpPost("photo")]
        public async Task PostPhoto()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Post photo: {Body}", body);
            var postPhoto = JsonSerializer.Deserialize<PhotoPost>(body);
            if (postPhoto == null) { return; }

            var id = await sql.GetNextPhotoIdAsync();
            var albumId = await sql.GetAlbumId(postPhoto.channelId);
            var photo = new Photo(id, postPhoto.url, albumId);
            await sql.AddPhoto(photo);
            thumbnails.SaveThumbnail(id, postPhoto.url);
        }

        [HttpPost("album")]
        public async Task PostAlbum()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Post album: {Body}", body);
            var postAlbum = JsonSerializer.Deserialize<AlbumPost>(body);
            if (postAlbum == null) { return; }

            var existingAlbum = await sql.GetAlbumFromChannelId(postAlbum.channelId);
            if (existingAlbum == null)
            {
                var id = await sql.GetNextAlbumId();
                var album = new Album(id, postAlbum.channelId, postAlbum.name, DateTime.Now.Year);
                await sql.AddAlbum(album);
            }
            else
            {
                var album = new Album(existingAlbum.id, postAlbum.channelId, postAlbum.name, existingAlbum.year);
                await sql.UpdateAlbum(album);
            }

            
        }

        [HttpPost("generatethumbnails/{id}")]
        public async Task GenerateThumbnails(int id)
        {
            var photos = await sql.GetPhotos(id);

            foreach (var photo in photos)
            {
                var isThumbail = thumbnails.IsThumbnailExisting(photo.id);
                if (isThumbail) { continue; }
                thumbnails.SaveThumbnail(photo.id, photo.url);
            }
        }
    }
}