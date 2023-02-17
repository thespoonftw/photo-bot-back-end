using MySql.Data.MySqlClient;
using System.Threading.Channels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace photo_bot_back_end.Services
{
    public class SqlService
    {
        private readonly ILogger<SqlService> logger;

        private const string CONNECTION_STRING = @"server=localhost;userid=mike;password=Ph0t0B0t;database=photo-bot";

        public SqlService(ILogger<SqlService> logger)
        {
            this.logger = logger;
        }

        public async Task<int> GetNextPhotoIdAsync()
        {
            using var sql = await Sql.ReadAsync("SELECT MAX(id) FROM photo");
            var maxId = sql.Reader.GetInt32(0);
            return maxId + 1;
        }

        public async Task<int> GetNextAlbumId()
        {
            using var sql = await Sql.ReadAsync("SELECT MAX(id) FROM album");
            sql.Reader.Read();
            var maxId = sql.Reader.GetInt32(0);
            return maxId + 1;
        }

        public async Task<int> GetAlbumId(string channelId)
        {
            using var sql = await Sql.ReadAsync($"SELECT id FROM album WHERE channelId={channelId}");
            sql.Reader.Read();
            var albumId = sql.Reader.GetInt32(0);
            return albumId;
        }

        public async Task AddPhoto(Photo photo)
        {
            await Sql.RunAsync($"INSERT INTO photo (id, url, albumId) VALUES ('{photo.id}', '{photo.url}', '{photo.albumId}')");
        }

        public async Task AddAlbum(Album album)
        {
            await Sql.RunAsync($"INSERT INTO album (id, name, channelId, year) VALUES ('{album.id}', '{album.name}', '{album.channelId}', '{album.year}')");
        }

        public async Task UpdateAlbum(Album album)
        {
            await Sql.RunAsync($"UPDATE album SET channelId = '{album.channelId}', name = '{album.name}', year = '{album.year}' WHERE id='{album.id}'");
        }

        public async Task<Album?> GetAlbum(int id)
        {
            using var sql = await Sql.ReadAsync($"SELECT * FROM album WHERE id={id}");
            if (!sql.Reader.HasRows) return null;
            var channelId = sql.Reader.GetString(1);
            var name = sql.Reader.GetString(2);
            var year = sql.Reader.GetInt32(3);
            return new Album(id, channelId, name, year);
        }

        public async Task<Album?> GetAlbumFromChannelId(string channelId)
        {
            using var sql = await Sql.ReadAsync($"SELECT * FROM album WHERE channelId={channelId}");
            if (!sql.Reader.HasRows) return null;
            var id = sql.Reader.GetInt32(0);
            var name = sql.Reader.GetString(2);
            var year = sql.Reader.GetInt32(3);
            return new Album(id, channelId, name, year);
        }

        public async Task<List<Album>> GetAllAlbums()
        {
            var returner = new List<Album>();
            using var sql = await Sql.ReadAsync($"SELECT * FROM album");

            while (sql.Reader.Read())
            {
                var id = sql.Reader.GetInt32(0);
                var channelId = sql.Reader.GetString(1);
                var name = sql.Reader.GetString(2);
                var year = sql.Reader.GetInt32(3);
                returner.Add(new Album(id, channelId, name, year));
            }
            return returner;
        }

        public async Task<List<Photo>> GetPhotos(int albumId)
        {
            var returner = new List<Photo>();
            using var sql = await Sql.ReadAsync($"SELECT * FROM photo WHERE albumId={albumId}");

            while (sql.Reader.Read())
            {
                var id = sql.Reader.GetInt32(0);
                var url = sql.Reader.GetString(1);
                var album = sql.Reader.GetInt32(2);
                returner.Add(new Photo(id, url, album));
            }
            return returner;
        }

        private class Sql : IDisposable
        {
            public MySqlDataReader Reader { get; init; }

            private readonly MySqlConnection connection;

            private Sql(MySqlConnection connection, MySqlDataReader reader)
            {
                this.connection = connection;
                Reader = reader;                
            }

            public static async Task<Sql> ReadAsync(string query)
            {
                var connection = new MySqlConnection(CONNECTION_STRING);
                connection.Open();
                var command = new MySqlCommand(query, connection);
                var reader = command.ExecuteReader();
                var sql = new Sql(connection, reader);
                await sql.Reader.ReadAsync();
                return sql;
            }

            public static async Task RunAsync(string query)
            {
                var connection = new MySqlConnection(CONNECTION_STRING);
                connection.Open();
                var command = new MySqlCommand(query, connection);
                await command.ExecuteNonQueryAsync();
                connection.Close();
            }

            public void Dispose()
            {
                Reader.Close();
                connection.Close();
            }
        }

    }

}
