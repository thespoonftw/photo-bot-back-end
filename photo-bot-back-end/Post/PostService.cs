using photo_bot_back_end.Misc;
using photo_bot_back_end.Sql;

namespace photo_bot_back_end.Post
{
    public class PostService
    {
        private readonly ILogger<SqlService> logger;
        private readonly SqlService sql;
        private readonly ThumbnailService thumbnails;

        public PostService(ILogger<SqlService> logger, SqlService sql, ThumbnailService thumbnails)
        {
            this.logger = logger;
            this.sql = sql;
            this.thumbnails = thumbnails;
        }

        public async Task PostPhoto(PhotoPost photoPost)
        {
            var albumId = await sql.GetAlbumId(photoPost.channelId);
            var userId = await GetOrCreateUserId(photoPost.uploaderId);
            
            var existingPhoto = await sql.GetPhotoFromUrl(photoPost.url);
            if (existingPhoto == null)
            {
                var id = await sql.GetNextPhotoIdAsync();
                var photo = new Photo(id, photoPost.url, albumId, userId, photoPost.uploadTime, photoPost.caption);
                await sql.MergeItem(photo);
                thumbnails.SaveThumbnail(id, photoPost.url);
            }
            else
            {
                var photo = new Photo(existingPhoto.id, existingPhoto.url, albumId, userId, photoPost.uploadTime, photoPost.caption);
                await sql.MergeItem(photo);
            }
        }

        public async Task PostAlbum(AlbumPost albumPost)
        {
            var existingAlbum = await sql.GetAlbumFromChannelId(albumPost.channelId);
            if (existingAlbum == null)
            {
                var id = await sql.GetNextAlbumId();
                var album = new Album(id, albumPost.channelId, albumPost.name, DateTime.Now.Year);
                await sql.MergeItem(album);
                await CreateUsersInAlbum(id, albumPost.members);
            }
            else
            {
                var newName = albumPost.name != "" ? albumPost.name : existingAlbum.name;
                var album = new Album(existingAlbum.id, existingAlbum.channelId, newName, existingAlbum.year);
                await sql.MergeItem(album);
                await CreateUsersInAlbum(existingAlbum.id, albumPost.members);
            }
        }
        
        public async Task<int> GetOrCreateUserId(string discordId)
        {
            var userId = await sql.GetUserId(discordId);

            if (userId != null)
            {
                return (int)userId;
            }

            var newId = await sql.GetNextUserId();
            var user = new User(newId, discordId, "New User");
            await sql.MergeItem(user);
            return newId;
        }

        public async Task CreateUsersInAlbum(int albumId, List<string> participantIds)
        {
            await sql.RemoveAllUsersForAlbum(albumId);

            foreach (var discordId in participantIds)
            {
                var userId = await GetOrCreateUserId(discordId);
                await sql.MergeItem(new UserInAlbum(userId, albumId));
            }
        }

        public async Task<HttpResponseMessage> DeletePhotoById(PhotoDeleteById photoDelete)
        {
            var getPhoto = sql.GetPhoto(photoDelete.photoId);
            var getUser = sql.GetUserFromDiscordId(photoDelete.requesterId);
            return await DeletePhoto(await getPhoto, await getUser);
            
        }

        public async Task<HttpResponseMessage> DeletePhotoByUrl(PhotoDeleteByUrl photoDelete)
        {
            var getPhoto = sql.GetPhotoFromUrl(photoDelete.url);
            var getUser = sql.GetUserFromDiscordId(photoDelete.requesterId);
            return await DeletePhoto(await getPhoto, await getUser);
        }

        private async Task<HttpResponseMessage> DeletePhoto(Photo? photo, User? user)
        {
            if (photo == null)
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);

            if (user == null || photo.userId != user.id)
                return new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);

            await sql.DeletePhoto(photo.id);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
    }
}
