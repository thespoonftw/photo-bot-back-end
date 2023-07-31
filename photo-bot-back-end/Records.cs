
namespace photo_bot_back_end
{

    public record PhotoPost(string url, string channelId, string uploaderId, string uploadTime, string caption);

    public record AlbumPost(string channelId, string name, List<string> members);

    public record PhotoDeleteById(int photoId, string requesterId);

    public record PhotoDeleteByUrl(string url, string requesterId);



    public record AlbumListData(string name, int year, int month, int numberOfPhotos);

    public record AlbumData(string name, int year, int month, List<PhotoData> photos, List<int> usersInAlbum);

    public record PhotoData(int id, string url, int albumId, int userId, string uploadTime, string caption, int score);



    public record Photo(int id, string url, int albumId, int userId, string uploadTime, string caption);

    public record Album(int id, string channelId, string name, int year, int month);

    public record User(int id, string discordId, string name, int level);

    public record UserInAlbum(int userId, int albumId);

    public record Vote(int userId, int photoId, int level);

    public record UserInPhoto(int userId, int photoId);
}
