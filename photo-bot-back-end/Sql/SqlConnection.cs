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

        public Album ReadAlbum()
        {
            var id = reader.GetInt32(0);
            var channelId = reader.GetString(1);
            var name = reader.GetString(2);
            var year = reader.GetInt32(3);
            return new Album(id, channelId, name, year);
        }

        public Photo ReadPhoto()
        {
            var id = reader.GetInt32(0);
            var url = reader.GetString(1);
            var albumId = reader.GetInt32(2);
            var userId = reader.GetInt32(3);
            var uploadTime = reader.SafeGetString(4);
            var caption = reader.SafeGetString(5);
            return new Photo(id, url, albumId, userId, uploadTime, caption);
        }

        public User ReadUser()
        {
            var id = reader.GetInt32(0);
            var discordId = reader.GetString(1);
            var name = reader.GetString(2);
            return new User(id, discordId, name);
        }
    }
}
