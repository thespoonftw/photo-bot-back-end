using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;

namespace photo_bot_back_end.Sql
{
    public static class SqlConnectionExtensions
    {
        public static string SafeGetString(this DbDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            return string.Empty;
        }
    }

    public class SqlConnection : IDisposable
    {
        private const string CONNECTION_STRING = @"server=localhost;userid=mike;password=Ph0t0B0t;database=photo-bot";

        private readonly DbDataReader reader;
        private readonly MySqlConnection connection;

        private SqlConnection(MySqlConnection connection, DbDataReader reader)
        {
            this.connection = connection;
            this.reader = reader;
        }

        public static async Task<SqlConnection> Query(string query)
        {
            var connection = new MySqlConnection(CONNECTION_STRING);
            connection.Open();
            var command = new MySqlCommand(query, connection);
            var reader = await command.ExecuteReaderAsync();
            var sql = new SqlConnection(connection, reader);
            return sql;
        }

        public static async Task NonQuery(string query)
        {
            var connection = new MySqlConnection(CONNECTION_STRING);
            connection.Open();
            var command = new MySqlCommand(query, connection);
            await command.ExecuteNonQueryAsync();
            connection.Close();
        }

        public void Dispose()
        {
            reader.Close();
            connection.Close();
        }

        public bool Next()
        {
            return reader.Read();
        }

        public int ReadId()
        {
            return reader.GetInt32(0);
        }

        public int ReadInt(int index)
        {
            return reader.GetInt32(index);
        }

        public Album ReadAlbum()
        {
            var id = reader.GetInt32(0);
            var channelId = reader.GetString(1);
            var name = reader.GetString(2);
            var year = reader.GetInt32(3);
            var month = reader.GetInt32(4);
            return new Album(id, channelId, name, year, month);
        }

        public Photo ReadPhoto()
        {
            var id = reader.GetInt32(0);
            var url = reader.GetString(1);
            var albumId = reader.GetInt32(2);
            var userId = reader.GetInt32(3);
            var score = reader.GetInt32(4);
            var uploadTime = reader.SafeGetString(5);
            var caption = reader.SafeGetString(6);
            var messageId = reader.SafeGetString(7);
            return new Photo(id, url, albumId, userId, score, uploadTime, caption, messageId);
        }

        public User ReadUser()
        {
            var id = reader.GetInt32(0);
            var discordId = reader.SafeGetString(1);
            var name = reader.GetString(2);
            var level = reader.GetInt32(3);
            var username = reader.SafeGetString(4);
            return new User(id, discordId, name, level, username);
        }

        public React ReadReact()
        {
            var userId = reader.GetInt32(0);
            var photoId = reader.GetInt32(1);
            var level = reader.GetInt32(2);
            return new React(userId, photoId, level);
        }

        public UserInAlbum ReadUserInAlbum()
        {
            var userId = reader.GetInt32(0);
            var albumId = reader.GetInt32(1);
            return new UserInAlbum(userId, albumId);
        }
    }
}
