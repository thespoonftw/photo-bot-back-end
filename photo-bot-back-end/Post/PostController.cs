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

        [HttpPost("vote")]
        public async Task Vote()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var postVote = JsonSerializer.Deserialize<Vote>(body);
            if (postVote == null) { return; }

            await postService.PostVote(postVote);
        }

        [HttpPost("delete_photo_by_id")]
        public async Task<HttpResponseMessage> DeletePhotoById()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Delete photo: {Body}", body);
            var deletePhoto = JsonSerializer.Deserialize<PhotoDeleteById>(body);
            if (deletePhoto == null) { return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest); }

            return await postService.DeletePhotoById(deletePhoto);
        }

        [HttpPost("delete_photo_by_url")]
        public async Task<HttpResponseMessage> DeletePhotoByUrl()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Delete photo: {Body}", body);
            var deletePhoto = JsonSerializer.Deserialize<PhotoDeleteByUrl>(body);
            if (deletePhoto == null) { return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest); }

            return await postService.DeletePhotoByUrl(deletePhoto);
        }
    }
}