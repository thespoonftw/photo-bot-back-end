using Microsoft.AspNetCore.Mvc;
using photo_bot_back_end.Post;
using photo_bot_back_end.Sql;

namespace photo_bot_back_end.Misc
{
    [ApiController]
    public class MiscController : ControllerBase
    {

        private readonly ILogger<PostController> logger;
        private readonly ThumbnailService thumbnailService;
        private readonly SqlService sqlService;

        public MiscController(ILogger<PostController> logger, ThumbnailService thumbnailService, SqlService sqlService)
        {
            this.logger = logger;
            this.thumbnailService = thumbnailService;
            this.sqlService = sqlService;
        }

        [HttpPost("generatethumbnails/{id}")]
        public async Task GenerateThumbnails(int id)
        {
            var photos = await sqlService.GetPhotosInAlbum(id);

            foreach (var photo in photos)
            {
                var isThumbail = thumbnailService.IsThumbnailExisting(photo.id);
                if (isThumbail) { continue; }
                thumbnailService.SaveThumbnail(photo.id, photo.url);
            }
        }
    }
}