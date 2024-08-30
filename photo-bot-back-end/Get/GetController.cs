using Microsoft.AspNetCore.Mvc;
using photo_bot_back_end.Post;

namespace photo_bot_back_end.Misc
{
    [ApiController]
    public class GetController : ControllerBase
    {
        private readonly ILogger<PostController> logger;
        private readonly GetService getService;

        public GetController(ILogger<PostController> logger, GetService getService)
        {
            this.logger = logger;
            this.getService = getService;
        }

        [HttpGet("album/{imgurId}")]
        public async Task<ReplyAlbum?> GetAlbumData(string imgurId)
        {
            return await getService.GetAlbumForImgurId(imgurId);
        }

        [HttpGet("photosByUser/{userId}")]
        public async Task<ReplyPhotos> GetPhotosByUser(int userId)
        {
            return await getService.GetPhotosByUser(userId);
        }

        [HttpGet("trash")]
        public async Task<ReplyPhotos> GetTrash()
        {
            return await getService.GetTrashPhotos();
        }

        [HttpGet("albumList")]
        public async Task<IEnumerable<ReplyAlbumDirectory>> GetAlbums()
        {
            return await getService.GetAlbums();
        }

        [HttpGet("react_level")]
        public async Task<ReplyReactLevel> GetReactLevel(int userId, int photoId)
        {
            return await getService.GetReactLevel(userId, photoId);
        }

        [HttpGet("album")]
        public async Task<IEnumerable<Album>> GetAllAlbums()
        {
            return await getService.GetAllAlbums();
        }

        [HttpGet("user")]
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await getService.GetAllUsers();
        }
    }
}