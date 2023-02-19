using photo_bot_back_end.Misc;
using photo_bot_back_end.Sql;

namespace photo_bot_back_end.Post
{
    public class PostService
    {
        private readonly ILogger<SqlService> logger;
        private readonly SqlService sql;
        private readonly ThumbnailService thumbnails;

        public PostService(ILogger<SqlService> logger, SqlService sql, ThumbnailService thumbnails)
        {
            this.logger = logger;
            this.sql = sql;
            this.thumbnails = thumbnails;
        }

        public async Task PostPhoto(PhotoPost photoPost)
        {
            var albumId = await sql.GetAlbumId(photoPost.channelId);
            var existingPhoto = await sql.GetPhotoFromUrl(photoPost.url);
            if (existingPhoto == null)
            {
                var id = await sql.GetNextPhotoIdAsync();
                var photo = new Photo(id, photoPost.url, albumId);
                await sql.AddPhoto(photo);
                thumbnails.SaveThumbnail(id, photoPost.url);
            }
            else
            {
                var photo = new Photo(existingPhoto.id, existingPhoto.url, albumId);
                await sql.UpdatePhoto(photo);
            }
        }

        public async Task PostAlbum(AlbumPost albumPost)
        {
            var existingAlbum = await sql.GetAlbumFromChannelId(albumPost.channelId);
            if (existingAlbum == null)
            {
                var id = await sql.GetNextAlbumId();
                var album = new Album(id, albumPost.channelId, albumPost.name, DateTime.Now.Year);
                await sql.AddAlbum(album);
            }
            else
            {
                var album = new Album(existingAlbum.id, existingAlbum.channelId, albumPost.name, existingAlbum.year);
                await sql.UpdateAlbum(album);
            }
        }


    }

}
