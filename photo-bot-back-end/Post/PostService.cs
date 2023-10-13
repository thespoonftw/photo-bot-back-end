using photo_bot_back_end;
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

        public async Task PostPhoto(PostPhoto photoPost)
        {
            var albumId = await sql.GetAlbumId(photoPost.channelId);
            var userId = await GetOrCreateUserId(photoPost.uploaderId);
            
            var existingPhoto = await sql.GetPhotoFromUrl(photoPost.url);
            if (existingPhoto == null)
            {
                var id = await sql.GetNextPhotoIdAsync();
                var photo = new Photo(id, photoPost.url, albumId, userId, 0, photoPost.uploadTime, photoPost.caption, photoPost.messageId);
                await sql.MergeItem(photo);
                thumbnails.SaveThumbnail(id, photoPost.url);
            }
            else
            {
                var photo = new Photo(existingPhoto.id, existingPhoto.url, albumId, userId, existingPhoto.score, photoPost.uploadTime, photoPost.caption, photoPost.messageId);
                await sql.MergeItem(photo);
            }
        }

        public async Task<ReplyAlbumUrl> PostAlbum(PostAlbum albumPost)
        {
            var album = await GetOrCreateAlbum(albumPost);
            await sql.MergeItem(album);
            var userIds = await DiscordIdsToUserIds(albumPost.members);
            await SetUsersInAlbum(album.id, userIds);
            var encryptedId = Encryptor.Encrypt(album.id.ToString());
            var url = $"http://www.brunch-projects.co.uk/album/{encryptedId}";
            return new ReplyAlbumUrl(url);
        }

        public async Task PostAlbumDate(PostAlbumDate datePost)
        {
            var album = await sql.GetAlbum(datePost.albumId);
            if (album == null) { return; }

            var newAlbum = new Album(album.id, album.channelId, album.name, datePost.year, datePost.month);
            await sql.MergeItem(newAlbum);
        }

        public async Task PostAlbumUsers(PostAlbumUsers usersPost)
        {
            await SetUsersInAlbum(usersPost.albumId, usersPost.users);
        }

        public async Task PostVote(Vote vote)
        {
            await sql.MergeItem(vote);
            await sql.UpdateScore(vote.photoId);
        }
        
        public async Task<int> GetOrCreateUserId(string discordId)
        {
            var userId = await sql.GetUserId(discordId);

            if (userId != null)
            {
                return (int)userId;
            }

            var newId = await sql.GetNextUserId();
            var user = new User(newId, discordId, "New User", 0, string.Empty);
            await sql.MergeItem(user);
            return newId;
        }

        public async Task SetUsersInAlbum(int albumId, List<int> userIds)
        {
            await sql.RemoveAllUsersForAlbum(albumId);

            foreach (var userId in userIds)
            {
                await sql.MergeItem(new UserInAlbum(userId, albumId));
            }
        }

        public async Task TrashPhotoById(int photoId)
        {
            await sql.MovePhotoToAlbum(photoId, 0);
        }

        public async Task<HttpResponseMessage> DeletePhotoById(DeletePhotoById photoDelete)
        {
            var getPhoto = sql.GetPhoto(photoDelete.photoId);
            var getUser = sql.GetUserFromDiscordId(photoDelete.requesterId);
            return await DeletePhoto(await getPhoto, await getUser);
            
        }

        public async Task<HttpResponseMessage> DeletePhotoByUrl(DeletePhotoByUrl photoDelete)
        {
            var getPhoto = sql.GetPhotoFromUrl(photoDelete.url);
            var getUser = sql.GetUserFromDiscordId(photoDelete.requesterId);
            return await DeletePhoto(await getPhoto, await getUser);
        }

        public async Task<HttpResponseMessage> DeletePhoto(DeletePhoto photoDelete)
        {
            var getPhoto = sql.GetPhoto(photoDelete.photoId);
            var getUser = sql.GetUser(photoDelete.userId);
            return await DeletePhoto(await getPhoto, await getUser);
        }

        public async Task<ReplyLogin> VerifyLogin(PostLogin post)
        {
            var user = await sql.GetUser(post.userId);
            if (user == null) { return new ReplyLogin(false); }
            return new ReplyLogin(user.username == post.password);
        }

        private async Task<HttpResponseMessage> DeletePhoto(Photo? photo, User? user)
        {
            if (photo == null || user == null)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            }

            if (photo.userId != user.id && user.level <= 1)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
            }

            await sql.DeletePhoto(photo.id);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }

        private async Task<Album> GetOrCreateAlbum(PostAlbum albumPost)
        {
            var existingAlbum = await sql.GetAlbumFromChannelId(albumPost.channelId);
            if (existingAlbum == null)
            {
                var id = await sql.GetNextAlbumId();
                return new Album(id, albumPost.channelId, albumPost.name, DateTime.Now.Year, DateTime.Now.Month);
            }
            else
            {
                var newName = albumPost.name != "" ? albumPost.name : existingAlbum.name;
                return new Album(existingAlbum.id, existingAlbum.channelId, newName, existingAlbum.year, existingAlbum.month);
            }
        }

        private async Task<List<int>> DiscordIdsToUserIds(List<string> discordIds)
        {
            var returner = new List<int>();
            foreach (var discordId in discordIds)
            {
                var userId = await GetOrCreateUserId(discordId);
                returner.Add(userId);
            }
            return returner;
        }
    }
}
