using sendbol_videoshop.Server.Models;
using sendbol_videoshop.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.StackExchangeRedis;


var builder = WebApplication.CreateBuilder(args);





builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.Configure<MongoVideoshopDatabaseSettings>(
    builder.Configuration.GetSection("VideoshopDatabase"));

builder.Services.Configure<RedisVideoshopDatabaseSettings>(
    builder.Configuration.GetSection("RedisConnection"));

builder.Services.AddSingleton<UsuariosService>();
builder.Services.AddSingleton<ProductosService>();
builder.Services.AddSingleton<CategoriasService>();
builder.Services.AddSingleton<ChiptagsService>();
builder.Services.AddSingleton<PlataformasService>();


builder.Services.AddSingleton<CountriesService>();



// Configurar Redis como almacenamiento de sesiones
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("RedisConnection:ConnectionString");
});

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Sendbol.Session";
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ...existing code...







var app = builder.Build();

#region Config. CORS 


app.UseCors(options =>
options.WithOrigins("https://localhost:54993","http://localhost:54993", "http://0.0.0.0:54993", "https://26.211.167.41:54993")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials() // <-- Esto es necesario
);


#endregion



app.UseDefaultFiles();
app.UseStaticFiles();






// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}




app.UseHttpsRedirection(); 
app.UseSession(); // <-- Agrega esto aquí


app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
