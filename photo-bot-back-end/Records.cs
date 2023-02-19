
namespace photo_bot_back_end
{
    public record PhotoPost(string url, string channelId, string uploaderId);

    public record AlbumPost(string channelId, string name, int? year);


    //public record Photo(int id, string url, int albumId, int uploaderId, DateTime uploadTime, string caption);

    public record Photo(int id, string url, int albumId);

    public record Album(int id, string channelId, string name, int year);

    public record User(int id, string discordId, string name);
}
