
namespace photo_bot_back_end
{

    public record PostPhoto(string url, string channelId, string uploaderId, string uploadTime, string caption, string messageId, int messageIndex);

    public record PostAlbum(string channelId, string name, List<string> members);

    public record PostLogin(int userId, string password);

    public record PostReact(int userId, int photoId, int? level);


    public record PostAlbumDate(int albumId, int year, int month);

    public record PostAlbumUsers(int albumId, List<int> users);


    public record DeletePhotoById(int photoId, string requesterId);

    public record DeletePhotoByUrl(string url, string requesterId);

    public record DeletePhoto(int photoId, int userId);


    public record ReplyAlbumUrl(string albumUrl);

    public record ReplyAlbumDirectory(int id, string imgurId, string name, int year, int month, int numberOfPhotos);

    public record ReplyAlbum(int id, string name, int year, int month, List<Photo> photos, List<int> usersInAlbum);

    public record ReplyPhotos(string shareUrl, List<Photo> photos);

    public record ReplyLogin(bool isSuccessful);

    public record ReplyReactLevel(int? level);


    public record Photo(
        int id, 
        string url, 
        int albumId, 
        int userId, 
        int score, 
        string uploadTime, 
        string caption, 
        string messageId, 
        int messageIndex, 
        string imgurId, 
        string deleteHash,
        string thumbnailId,
        string thumbnailDeleteHash
        );

    public record Album(int id, string imgurId, string channelId, string name, int year, int month);

    public record User(int id, string discordId, string name, int level, string username);

    public record UserInAlbum(int userId, int albumId);

    public record React(int userId, int photoId, int level);

    public record UserInPhoto(int userId, int photoId);


    public record ImgurAlbum(string title, string description, string[] ids);

    public record ImgurAlbumReply(string id, string deletehash);

    public record ImgurUpload(string image, string type, string album);

    public record ImgurUploadReply(string id, string link, string deletehash);
}
