using Microsoft.AspNetCore.Mvc;
using photo_bot_back_end.Post;
using photo_bot_back_end.Sql;

namespace photo_bot_back_end.Misc
{
    [ApiController]
    public class GetController : ControllerBase
    {
        public record AlbumListData(string name, int year, int month, int numberOfPhotos);

        public record AlbumData(Album album, List<Photo> photos, List<int> usersInAlbum);

        private readonly ILogger<PostController> logger;
        private readonly SqlService sql;

        public GetController(ILogger<PostController> logger, SqlService sql)
        {
            this.logger = logger;
            this.sql = sql;
        }

        [HttpGet("album/{urlName}")]
        public async Task<AlbumData?> GetAlbumData(string urlName)
        {
            var underscoreLastIndex = urlName.LastIndexOf("_");
            var name = urlName[0..underscoreLastIndex].Replace("_", " ");
            var year = int.Parse(urlName[(underscoreLastIndex+1)..]);
            var album = await sql.GetAlbum(name, year);
            if (album == null) return null;
            var photosAsync = sql.GetPhotosInAlbum(album.id);
            var usersAsync = sql.GetUsersForAlbum(album.id);            
            return new AlbumData(album, await photosAsync, await usersAsync);
        }

        [HttpGet("photosByUser/{userId}")]
        public async Task<List<Photo>> GetPhotosByUser(int userId)
        {
            return await sql.GetPhotosByUser(userId);
        }

        [HttpGet("albumList")]
        public async Task<IEnumerable<AlbumListData>> GetAlbumList()
        {
            var albums_async = sql.GetAllAlbums();
            var counts = await sql.GetAlbumCounts();
            var albums = await albums_async;
            return albums.Select(a =>
                new AlbumListData(a.name, a.year, a.month, counts[a.id])
            );
        }

        [HttpGet("album")]
        public async Task<IEnumerable<Album>> GetAllAlbums()
        {
            return await sql.GetAllAlbums();
        }

        [HttpGet("user")]
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await sql.GetAllUsers();
        }
    }
}