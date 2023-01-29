using MySql.Data.MySqlClient;
using System.Threading.Channels;

namespace photo_bot_back_end.Services
{
    public class SqlService
    {
        private readonly ILogger<SqlService> logger;

        private const string CONNECTION_STRING = @"server=localhost;userid=mike;password=Ph0t0B0t;database=photo-bot";

        private readonly MySqlConnection connection;

        public SqlService(ILogger<SqlService> logger)
        {
            this.logger = logger;
            connection = new MySqlConnection(CONNECTION_STRING);
        }

        public int GetNextPhotoId()
        {
            using var rdr = Sql("SELECT MAX(id) FROM photo").ExecuteReader();
            rdr.Read();
            var maxId = rdr.GetInt32(0);
            rdr.Close();
            return maxId + 1;
        }

        public int GetNextAlbumId()
        {
            using var rdr = Sql("SELECT MAX(id) FROM album").ExecuteReader();
            rdr.Read();
            var maxId = rdr.GetInt32(0);
            rdr.Close();
            return maxId + 1;
        }

        public int GetAlbumId(string channelId)
        {
            using var rdr = Sql($"SELECT id FROM album WHERE channelId={channelId}").ExecuteReader();
            rdr.Read();
            var albumId = rdr.GetInt32(0);
            rdr.Close();
            return albumId;
        }

        public void AddPhoto(Photo photo)
        {
            Sql($"INSERT INTO photo (id, url, albumId) VALUES ('{photo.id}', '{photo.url}', '{photo.albumId}')").ExecuteNonQuery();
        }

        public void AddAlbum(Album album)
        {
            Sql($"INSERT INTO album (id, name, channelId, year) VALUES ('{album.id}', '{album.name}', '{album.channelId}', '{album.year}')").ExecuteNonQuery();
        }

        public Album GetAlbum(int id)
        {
            using var rdr = Sql($"SELECT * FROM album WHERE id={id}").ExecuteReader();
            rdr.Read();
            var channelId = rdr.GetString(1);
            var name = rdr.GetString(2);
            var year = rdr.GetInt32(3);
            rdr.Close();
            return new Album(id, channelId, name, year);
        }

        public List<Album> GetAllAlbums()
        {
            var returner = new List<Album>();
            using var rdr = Sql($"SELECT * FROM album").ExecuteReader();

            while (rdr.Read())
            {
                var id = rdr.GetInt32(0);
                var channelId = rdr.GetString(1);
                var name = rdr.GetString(2);
                var year = rdr.GetInt32(3);
                returner.Add(new Album(id, channelId, name, year));
            }
            return returner;
        }

        public List<Photo> GetPhotos(int albumId)
        {
            var returner = new List<Photo>();
            using var rdr = Sql($"SELECT * FROM photo WHERE albumId={albumId}").ExecuteReader();

            while (rdr.Read())
            {
                var id = rdr.GetInt32(0);
                var url = rdr.GetString(1);
                var album = rdr.GetInt32(2);
                returner.Add(new Photo(id, url, album));
            }
            return returner;
        }

        private MySqlCommand Sql(string query)
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
            return new MySqlCommand(query, connection);
        }

    }

}
