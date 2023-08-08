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

        [HttpGet("album/{url}")]
        public async Task<AlbumData?> GetAlbumData(string url)
        {
            return await getService.GetAlbumForUrl(url);
        }

        [HttpGet("photosByUser/{userId}")]
        public async Task<PhotosData> GetPhotosByUser(int userId)
        {
            return await getService.GetPhotosByUser(userId);
        }

        [HttpGet("albumList")]
        public async Task<IEnumerable<AlbumListData>> GetAlbums()
        {
            return await getService.GetAlbums();
        }

        [HttpGet("voteLevel")]
        public async Task<int?> GetVoteLevel(int userId, int photoId)
        {
            return await getService.GetVoteLevel(userId, photoId);
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