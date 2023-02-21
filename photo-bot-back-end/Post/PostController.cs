using Microsoft.AspNetCore.Mvc;
using photo_bot_back_end;
using photo_bot_back_end.Misc;
using photo_bot_back_end.Sql;
using System.Text.Json;

namespace photo_bot_back_end.Post
{
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly ILogger<PostController> logger;
        private readonly PostService postService;

        public PostController(ILogger<PostController> logger, PostService postService)
        {
            this.logger = logger;
            this.postService = postService;
        }

        [HttpPost("photo")]
        public async Task PostPhoto()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Post photo: {Body}", body);
            var postPhoto = JsonSerializer.Deserialize<PhotoPost>(body);
            if (postPhoto == null) { return; }

            await postService.PostPhoto(postPhoto);
        }

        [HttpPost("album")]
        public async Task PostAlbum()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Post album: {Body}", body);
            var postAlbum = JsonSerializer.Deserialize<AlbumPost>(body);
            if (postAlbum == null) { return; }

            await postService.PostAlbum(postAlbum);
        }
    }
}