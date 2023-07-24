using Microsoft.AspNetCore.Mvc;
using photo_bot_back_end.Post;
using photo_bot_back_end.Sql;

namespace photo_bot_back_end.Misc
{
    [ApiController]
    public class GetController : ControllerBase
    {
        public record AlbumData(Album album, List<Photo> photos, List<User> usersInAlbum);

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