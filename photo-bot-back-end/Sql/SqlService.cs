
namespace photo_bot_back_end.Sql
{
    public class SqlService
    {
        private readonly ILogger<SqlService> logger;

        public SqlService(ILogger<SqlService> logger)
        {
            this.logger = logger;
        }

        public async Task<int> GetNextPhotoIdAsync()
        {
            using var sql = await SqlConnection.Query("SELECT MAX(id) FROM photo");
            return sql.ReadId() + 1;
        }

        public async Task<int> GetNextAlbumId()
        {
            using var sql = await SqlConnection.Query("SELECT MAX(id) FROM album");
            return sql.ReadId() + 1;
        }

        public async Task<int> GetNextUserId()
        {
            using var sql = await SqlConnection.Query("SELECT MAX(id) FROM user");
            return sql.ReadId() + 1;
        }

        public async Task<int> GetAlbumId(string channelId)
        {
            using var sql = await SqlConnection.Query($"SELECT id FROM album WHERE channelId={channelId}");
            sql.Next();
            return sql.ReadId();
        }

        public async Task<int?> GetUserId(string discordId)
        {
            using var sql = await SqlConnection.Query($"SELECT id FROM user WHERE discordId={discordId}");
            if (sql.Next())
            {
                return sql.ReadId();
            }
            return null;
        }

        public async Task AddPhoto(Photo photo)
        {
            await SqlConnection.NonQuery($"INSERT INTO photo (id, url, albumId, userId, uploadTime, caption) VALUES ('{photo.id}', '{photo.url}', '{photo.albumId}', '{photo.userId}', '{photo.uploadTime}', '{photo.caption}')");
        }

        public async Task AddAlbum(Album album)
        {
            await SqlConnection.NonQuery($"INSERT INTO album (id, name, channelId, year) VALUES ('{album.id}', '{album.name}', '{album.channelId}', '{album.year}')");
        }

        public async Task AddUser(User user)
        {
            await SqlConnection.NonQuery($"$INSERT INTO user (id, name, discordId) VALUES ('{user.id}', '{user.name}', '{user.discordId}')");
        }

        public async Task UpdateAlbum(Album album)
        {
            await SqlConnection.NonQuery($"UPDATE album SET channelId = '{album.channelId}', name = '{album.name}', year = '{album.year}' WHERE id='{album.id}'");
        }

        public async Task UpdatePhoto(Photo photo)
        {
            await SqlConnection.NonQuery($"UPDATE photo SET albumId='{photo.albumId}', url='{photo.url}', userId='{photo.userId}, uploadTime='{photo.uploadTime}', caption='{photo.caption}' WHERE id='{photo.id}'");
        }

        public async Task<Album?> GetAlbum(int id)
        {
            using var sql = await SqlConnection.Query($"SELECT * FROM album WHERE id={id}");
            if (!sql.Next())
            {
                return null;
            }
            return sql.ReadAlbum();
        }

        public async Task<Album?> GetAlbumFromChannelId(string channelId)
        {
            using var sql = await SqlConnection.Query($"SELECT * FROM album WHERE channelId={channelId}");
            if (!sql.Next())
            {
                return null;
            }
            return sql.ReadAlbum();
        }

        public async Task<Photo?> GetPhotoFromUrl(string url)
        {
            using var sql = await SqlConnection.Query($"SELECT * FROM photo WHERE url={url}");
            if (!sql.Next())
            {
                return null;
            }
            return sql.ReadPhoto();
        }

        public async Task<List<Album>> GetAllAlbums()
        {
            var returner = new List<Album>();
            using var sql = await SqlConnection.Query($"SELECT * FROM album");
            while (sql.Next())
            {
                returner.Add(sql.ReadAlbum());
            }
            return returner;
        }

        public async Task<List<Photo>> GetPhotos(int albumId)
        {
            var returner = new List<Photo>();
            using var sql = await SqlConnection.Query($"SELECT * FROM photo WHERE albumId={albumId}");
            while (sql.Next())
            {
                returner.Add(sql.ReadPhoto());
            }
            return returner;
        }
    }

}
