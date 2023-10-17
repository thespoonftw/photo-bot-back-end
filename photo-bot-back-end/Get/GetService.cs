using photo_bot_back_end.Misc;
using photo_bot_back_end.Sql;

namespace photo_bot_back_end.Post
{
    public class GetService
    {
        private readonly ILogger<SqlService> logger;
        private readonly SqlService sql;

        public GetService(ILogger<SqlService> logger, SqlService sql)
        {
            this.logger = logger;
            this.sql = sql;
        }

        public async Task<ReplyAlbum?> GetAlbumForUrl(string url)
        {
            var decrypt = Encryptor.Decrypt(url);
            var isSuccess = int.TryParse(decrypt, out int id);
            if (!isSuccess) { return null; }

            var album = await sql.GetAlbum(id);
            if (album == null) { return null; }

            var photosAsync = sql.GetPhotosInAlbum(album.id);
            var usersAsync = sql.GetUsersForAlbum(album.id);
            return new ReplyAlbum(album.id, album.name, album.year, album.month, await photosAsync, await usersAsync);
        }

        public async Task<IEnumerable<ReplyAlbumDirectory>> GetAlbums()
        {
            var albums_async = sql.GetAllAlbums();
            var counts = await sql.GetAlbumCounts();
            var albums = await albums_async;
            return albums.Select(a =>
                new ReplyAlbumDirectory(Encryptor.Encrypt(a.id.ToString()), a.name, a.year, a.month, counts[a.id])
            );
        }

        public async Task<ReplyReactLevel> GetReactLevel(int userId, int photoId)
        {
            var react = await sql.GetReact(userId, photoId);
            return new ReplyReactLevel(react?.level);
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await sql.GetAllUsers();
        }

        public async Task<IEnumerable<Album>> GetAllAlbums()
        {
            return await sql.GetAllAlbums();
        }

        public async Task<ReplyPhotos> GetPhotosByUser(int userId)
        {
            var photos = sql.GetPhotosByUser(userId);
            var url = Encryptor.Encrypt($"u={userId}");
            return new ReplyPhotos(url, await photos);
        }

        public async Task<ReplyPhotos> GetTrashPhotos()
        {
            // TODO combine with search functionality
            var photos = await sql.GetPhotosInAlbum(0);
            return new ReplyPhotos("", photos);
        }


    }
}
