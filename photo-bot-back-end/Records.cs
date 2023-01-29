
namespace photo_bot_back_end
{
    public record Photo(int id, string url, int albumId);

    public record Album(int id, string channelId, string name, int year);
}
