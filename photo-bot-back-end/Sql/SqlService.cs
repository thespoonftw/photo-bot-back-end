
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
            sql.Next();
            return sql.ReadId() + 1;
        }

        public async Task<int> GetNextAlbumId()
        {
            using var sql = await SqlConnection.Query("SELECT MAX(id) FROM album");
            sql.Next();
            return sql.ReadId() + 1;
        }

        public async Task<int> GetNextUserId()
        {
            using var sql = await SqlConnection.Query("SELECT MAX(id) FROM user");
            sql.Next();
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

        public async Task MergeItem<T>(T item)
        {
            var name = ToCamel(typeof(T).Name);
            var props = typeof(T).GetProperties();
            var columns = string.Join(",", props.Select(p => p.Name));
            var values = string.Join(",", props.Select(p => $"'{p.GetValue(item)!.ToString()}'"));
            var pairs = string.Join(",", props.Select(p => $"{p.Name}='{p.GetValue(item!)}'"));
            var query = $"INSERT INTO {name} ({columns}) VALUES ({values}) ON DUPLICATE KEY UPDATE {pairs}";
            await SqlConnection.NonQuery(query);
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
            using var sql = await SqlConnection.Query($"SELECT * FROM photo WHERE url='{url}'");
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

        public async Task<List<Photo>> GetPhotosInAlbum(int albumId)
        {
            var returner = new List<Photo>();
            using var sql = await SqlConnection.Query($"SELECT * FROM photo WHERE albumId={albumId}");
            while (sql.Next())
            {
                returner.Add(sql.ReadPhoto());
            }
            return returner;
        }

        public async Task<List<Album>> GetAlbumsForUser(int userId)
        {
            var returner = new List<Album>();
            using var sql = await SqlConnection.Query($"SELECT * FROM album JOIN userInAlbum ON album.id = userInAlbum.albumId WHERE userInAlbum.userId={userId}");
            while (sql.Next())
            {
                returner.Add(sql.ReadAlbum());
            }
            return returner;
        }

        public async Task<List<User>> GetUsersForAlbum(int albumId)
        {
            var returner = new List<User>();
            using var sql = await SqlConnection.Query($"SELECT * FROM user JOIN userInAlbum ON user.id = userInAlbum.userId WHERE userInAlbum.albumId={albumId}");
            while (sql.Next())
            {
                returner.Add(sql.ReadUser());
            }
            return returner;
        }

        public async Task RemoveAllUsersForAlbum(int albumId)
        {
            await SqlConnection.NonQuery($"DELETE FROM userinalbum WHERE albumId = {albumId}");
        }

        private string ToCamel(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            char[] charArray = input.ToCharArray();
            charArray[0] = char.ToLower(charArray[0]);
            return new string(charArray);
        }
    }

}
