
namespace photo_bot_back_end
{

    public record PhotoPost(string url, string channelId, string uploaderId, string uploadTime, string caption, string messageId);

    public record AlbumPost(string channelId, string name, List<string> members);

    public record AlbumResponse(string albumUrl);

    public record PhotoDeleteById(int photoId, string requesterId);

    public record PhotoDeleteByUrl(string url, string requesterId);



    public record AlbumListData(string id, string name, int year, int month, int numberOfPhotos);

    public record AlbumData(string name, int year, int month, List<Photo> photos, List<int> usersInAlbum);

    public record PhotosData(string shareUrl, List<Photo> photos);


    public record Photo(int id, string url, int albumId, int userId, int score, string uploadTime, string caption, string messageId);

    public record Album(int id, string channelId, string name, int year, int month);

    public record User(int id, string discordId, string name, int level);

    public record UserInAlbum(int userId, int albumId);

    public record Vote(int userId, int photoId, int level);

    public record UserInPhoto(int userId, int photoId);
}
