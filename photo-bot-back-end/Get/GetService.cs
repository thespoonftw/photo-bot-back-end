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

        public async Task<AlbumData?> GetAlbum(string albumName)
        {
            var album = await sql.GetAlbum(albumName);
            if (album == null) { return null; }
            var photosAsync = sql.GetPhotosForAlbum(album.id);
            var usersAsync = sql.GetUsersForAlbum(album.id);
            return new AlbumData(album.name, album.year, album.month, await photosAsync, await usersAsync);
        }

        public async Task<IEnumerable<AlbumListData>> GetAlbums()
        {
            var albums_async = sql.GetAllAlbums();
            var counts = await sql.GetAlbumCounts();
            var albums = await albums_async;
            return albums.Select(a =>
                new AlbumListData(a.name, a.year, a.month, counts[a.id])
            );
        }

        public async Task<int?> GetVoteLevel(int userId, int photoId)
        {
            var vote = await sql.GetVote(userId, photoId);
            if (vote == null) { return 0; }
            return vote.level;
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await sql.GetAllUsers();
        }

        public async Task<IEnumerable<Album>> GetAllAlbums()
        {
            return await sql.GetAllAlbums();
        }

        public async Task<IEnumerable<PhotoData>> GetPhotosByUser(int userId)
        {
            return await sql.GetPhotosByUser(userId);
        }


    }
}
