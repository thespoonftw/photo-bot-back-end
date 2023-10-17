using Microsoft.AspNetCore.Mvc;
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
            var postPhoto = JsonSerializer.Deserialize<PostPhoto>(body);
            if (postPhoto == null) { return; }

            await postService.PostPhoto(postPhoto);
        }

        [HttpPost("album")]
        public async Task<ReplyAlbumUrl?> PostAlbum()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Post album: {Body}", body);
            var postAlbum = JsonSerializer.Deserialize<PostAlbum>(body);
            if (postAlbum == null) { return null; }

            return await postService.PostAlbum(postAlbum);
        }

        [HttpPost("react")]
        public async Task React()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var postVote = JsonSerializer.Deserialize<PostReact>(body);
            if (postVote == null) { return; }

            await postService.PostReact(postVote);
        }

        [HttpPost("delete_photo_by_id")]
        public async Task<HttpResponseMessage> DeletePhotoByDiscordId()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Delete photo: {Body}", body);
            var deletePhoto = JsonSerializer.Deserialize<DeletePhotoById>(body);
            if (deletePhoto == null) { return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest); }

            return await postService.DeletePhotoById(deletePhoto);
        }

        [HttpPost("delete_photo_by_url")]
        public async Task<HttpResponseMessage> DeletePhotoByUrl()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Delete photo: {Body}", body);
            var deletePhoto = JsonSerializer.Deserialize<DeletePhotoByUrl>(body);
            if (deletePhoto == null) { return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest); }

            return await postService.DeletePhotoByUrl(deletePhoto);
        }

        [HttpPost("delete_photo")]
        public async Task<HttpResponseMessage> DeletePhoto()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Delete photo: {Body}", body);
            var deletePhoto = JsonSerializer.Deserialize<DeletePhoto>(body);
            if (deletePhoto == null) { return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest); }

            return await postService.DeletePhoto(deletePhoto);
        }

        [HttpPost("login")]
        public async Task<ReplyLogin> Login()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Login Attempt: {Body}", body);
            var post = JsonSerializer.Deserialize<PostLogin>(body);
            if (post == null) { return new ReplyLogin(false); }

            return await postService.VerifyLogin(post);
        }

        [HttpPost("trash/{photoId}")]
        public async Task TrashPhoto(string photoId)
        {
            var success = int.TryParse(photoId, out int id);
            if (success == false) { return; }
            await postService.TrashPhotoById(id);
        }

        [HttpPost("album_date")]
        public async Task PostAlbumDate()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Album Date: {Body}", body);
            var post = JsonSerializer.Deserialize<PostAlbumDate>(body);
            if (post == null) { return; }

            await postService.PostAlbumDate(post);
        }

        [HttpPost("album_users")]
        public async Task PostAlbumUsers()
        {
            var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            logger.LogInformation("Album Users: {Body}", body);
            var post = JsonSerializer.Deserialize<PostAlbumUsers>(body);
            if (post == null) { return; }

            await postService.PostAlbumUsers(post);
        }
    }
}