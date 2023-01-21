using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace photo_bot_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PhotoController : ControllerBase
    {
        private readonly ILogger<PhotoController> _logger;

        public PhotoController(ILogger<PhotoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            string cs = @"server=localhost;userid=mike;password=Ph0t0B0t;database=photo-bot";
            using var con = new MySqlConnection(cs);
            con.Open();
            string sql = "SELECT * FROM photos";
            using var cmd = new MySqlCommand(sql, con);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            var results = new List<string>();
            while (rdr.Read())
            {
                results.Add(rdr.GetString(1));
            }

            return results;
        }

        [HttpPost]
        public async void Post()
        {
            var content = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            string cs = @"server=localhost;userid=mike;password=Ph0t0B0t;database=photo-bot";
            using var con = new MySqlConnection(cs);
            con.Open();

            string sql = "select MAX(id) FROM photos";
            using var cmd = new MySqlCommand(sql, con);
            using MySqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            var maxId = rdr.GetInt32(0);
            rdr.Close();

            string sql2 = $"INSERT INTO `photo-bot`.`photos` (`id`, `url`) VALUES('{maxId+1}', '{content}')";
            using var cmd2 = new MySqlCommand(sql2, con);
            cmd2.ExecuteNonQuery();
        }

    }
}