using API.Middlewares;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http1);
//});
// Add services to the container.

builder.Services.AddControllers();
//builder.Services.AddDbContext<StoreContext>(opt =>{
//    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
//});

builder.Services.AddDbContext<StoreContext>();
builder.Services.AddScoped<IProductRepository,ProductRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));
builder.Services.AddCors();
builder.Services.AddSingleton<IConnectionMultiplexer>(config =>
{
    var conn = builder.Configuration.GetConnectionString("Redis")??throw new Exception("Can't get redis connection !");
    var connectionString = ConfigurationOptions.Parse(conn);
    return ConnectionMultiplexer.Connect(connectionString) ;
});
builder.Services.AddSingleton<ICartService, CartService>();
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<StoreContext>();


var app = builder.Build();

 // Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(x=>x.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
.WithOrigins("http://localhost:4200", "https://localhost:4200"));


app.MapControllers();

app.MapGroup("api").MapIdentityApi<AppUser>();

try
{
    using var scope = app.Services.CreateScope();

    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<StoreContext>();
    await context.Database.MigrateAsync();
    await StoreContextSeed.SeedAsync(context);

}
catch (Exception ex)
{
    Console.WriteLine(ex);
    throw;

   
}

 app.Run();

