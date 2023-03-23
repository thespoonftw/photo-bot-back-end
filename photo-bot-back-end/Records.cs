
namespace photo_bot_back_end
{
    public record PhotoPost(string url, string channelId, string uploaderId, string uploadTime, string caption);

    public record AlbumPost(string channelId, string name, List<string> members);

    public record PhotoDeleteById(int photoId, string requesterId);

    public record PhotoDeleteByUrl(string url, string requesterId);



    public record Photo(int id, string url, int albumId, int userId, string uploadTime, string caption);

    public record Album(int id, string channelId, string name, int year);

    public record User(int id, string discordId, string name);

    public record UserInAlbum(int userId, int albumId);
}
