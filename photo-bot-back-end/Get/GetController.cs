using Microsoft.AspNetCore.Mvc;
using photo_bot_back_end.Post;
using photo_bot_back_end.Sql;

namespace photo_bot_back_end.Misc
{
    [ApiController]
    public class GetController : ControllerBase
    {
        public record AlbumData(Album album, List<Photo> photos);

        private readonly ILogger<PostController> logger;
        private readonly SqlService sql;

        public GetController(ILogger<PostController> logger, SqlService sql)
        {
            this.logger = logger;
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
    }
}