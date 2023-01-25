using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using MySql.Data.MySqlClient;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Net;

namespace photo_bot_backend.Controllers
{
    public record Photo(int Id, string Url);

    [ApiController]
    [Route("[controller]")]
    public class PhotoController : ControllerBase
    {
        private readonly ILogger<PhotoController> _logger;
        private readonly IWebHostEnvironment webHostEnvironment;

        private const string CONNECTION_STRING = @"server=localhost;userid=mike;password=Ph0t0B0t;database=photo-bot";

        public PhotoController(ILogger<PhotoController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IEnumerable<Photo> Get()
        {
            return GetAllPhotos();
        }

        [HttpPost]
        public async void Post()
        {
            var imageUrl = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var id = GetNewId();
            var photo = new Photo(id, imageUrl);
            SaveThumbnail(photo);
            AddPhotoToDb(photo);
        }

        [HttpPost("generatethumbnails")]
        public void GenerateThumbnails()
        {
            var photos = GetAllPhotos();

            foreach (var photo in photos)
            {
                var isExisting = System.IO.File.Exists(GetThumbnailPath(photo.Id));
                if (isExisting) { continue; }
                SaveThumbnail(photo);
            }
        }

        private async void SaveThumbnail(Photo photo)
        {
            using var client = new HttpClient();
            using var stream = await client.GetStreamAsync(photo.Url);
            using var image = Image.Load(stream);

            var newWidth = 300;
            var newHeight = 300;

            if (image.Width > image.Height)
            {
                newHeight = (image.Height * 300) / image.Width;
            }
            else
            {
                newWidth = (image.Width * 300) / image.Height;
            }

            image.Mutate(x => x.Resize(newWidth, newHeight));
            image.Save(GetThumbnailPath(photo.Id));
        }

        private int GetNewId()
        {
            using var con = new MySqlConnection(CONNECTION_STRING);
            con.Open();

            string sql = "select MAX(id) FROM photos";
            using var cmd = new MySqlCommand(sql, con);
            using MySqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            var maxId = rdr.GetInt32(0);
            rdr.Close();
            return maxId + 1;
        }

        private List<Photo> GetAllPhotos()
        {
            var returner = new List<Photo>();
            using var con = new MySqlConnection(CONNECTION_STRING);
            con.Open();
            string sql = "SELECT * FROM photos";
            using var cmd = new MySqlCommand(sql, con);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                var id = rdr.GetInt32(0);
                var url = rdr.GetString(1);
                returner.Add(new Photo(id, url));
            }
            return returner;
        }

        private void AddPhotoToDb(Photo photo)
        {
            using var con = new MySqlConnection(CONNECTION_STRING);
            con.Open();
            string sql = $"INSERT INTO `photo-bot`.`photos` (`id`, `url`) VALUES('{photo.Id}', '{photo.Url}')";
            using var cmd = new MySqlCommand(sql, con);
            cmd.ExecuteNonQuery();
        }

        private string GetThumbnailPath(int id)
        {
            return $"{webHostEnvironment.WebRootPath}/thumbnails/{id}.jpg";
        }

    }
}