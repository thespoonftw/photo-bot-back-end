using photo_bot_back_end.Misc;
using photo_bot_back_end.Post;
using photo_bot_back_end.Sql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSingleton<SqlService>();
builder.Services.AddSingleton<PostService>();
builder.Services.AddSingleton<GetService>();
builder.Services.AddSingleton<ImageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseStaticFiles(); // enables hosting images

app.Run();
