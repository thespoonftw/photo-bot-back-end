using photo_bot_back_end.Sql;

namespace photo_bot_back_end.Post
{
    public class PostService
    {
        private readonly ILogger<SqlService> logger;
        private readonly SqlService sql;

        public PostService(ILogger<SqlService> logger, SqlService sql)
        {
            this.logger = logger;
            this.sql = sql;
        }

        public async Task PostPhoto(PostPhoto photoPost)
        {
            var album = await sql.GetAlbumFromChannelId(photoPost.channelId);
            if (album == null)
            {
                throw new Exception("No album found for photo. " + photoPost.channelId);
            }

            var getUserId = GetOrCreateUserId(photoPost.uploaderId);
            var getId = sql.GetNextPhotoId();
            //var postUpload = imgur.UploadPhoto(photoPost.url, album.imgurId);
            //var postThumbnail = imgur.UploadThumbnail(photoPost.url);
            //var upload = await postUpload;
            //var thumbnail = await postThumbnail;

            var photo = new Photo(
                await getId,
                string.Empty, 
                album.id, 
                await getUserId, 
                0, 
                photoPost.uploadTime, 
                photoPost.caption, 
                photoPost.messageId, 
                photoPost.messageIndex,
                string.Empty, 
                string.Empty,
                string.Empty,
                string.Empty
                );

            await sql.MergeItem(photo);
        }

        public async Task<ReplyAlbumUrl> PostAlbum(PostAlbum albumPost)
        {
            var album = await GetOrCreateAlbum(albumPost);
            var userIds = await DiscordIdsToUserIds(albumPost.members);
            await SetUsersInAlbum(album.id, userIds);
            var url = $"http://www.brunch-projects.co.uk/album/{album.imgurId}";
            return new ReplyAlbumUrl(url);
        }

        public async Task PostAlbumDate(PostAlbumDate datePost)
        {
            var album = await sql.GetAlbum(datePost.albumId);
            if (album == null) { return; }

            var newAlbum = new Album(album.id, album.channelId, album.imgurId, album.name, datePost.year, datePost.month);
            await sql.MergeItem(newAlbum);
        }

        public async Task PostAlbumUsers(PostAlbumUsers usersPost)
        {
            await SetUsersInAlbum(usersPost.albumId, usersPost.users);
        }

        public async Task PostReact(PostReact reactPost)
        {
            if (reactPost.level != null)
            {
                var react = new React(reactPost.userId, reactPost.photoId, (int)reactPost.level);
                await sql.MergeItem(react);
            }
            else
            {
                await sql.DeleteReact(reactPost.userId, reactPost.photoId);
            }

            await sql.UpdateScore(reactPost.photoId);
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
                var nextId = await sql.GetNextAlbumId();
                // TODO remove imgur album id
                var re = new Album(nextId, string.Empty, albumPost.channelId, albumPost.name, DateTime.Now.Year, DateTime.Now.Month);
                await sql.MergeItem(re);
                return re;
            }
            else
            {
                var newName = albumPost.name != "" ? albumPost.name : existingAlbum.name;
                var re = new Album(existingAlbum.id, existingAlbum.imgurId, existingAlbum.channelId, newName, existingAlbum.year, existingAlbum.month);
                await sql.MergeItem(re);
                return re;
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
