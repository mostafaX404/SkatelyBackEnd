using API.Middlewares;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ================= Controllers =================
builder.Services.AddControllers();

// ================= DbContext =================
builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ================= Repositories =================
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// ================= CORS =================
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ================= Redis =================
builder.Services.AddSingleton<IConnectionMultiplexer>(config =>
{
    var conn = builder.Configuration.GetConnectionString("Redis")
        ?? throw new Exception("Can't get redis connection !");

    var connectionString = ConfigurationOptions.Parse(conn);
    return ConnectionMultiplexer.Connect(connectionString);
});

// ================= Services =================
builder.Services.AddSingleton<ICartService, CartService>();
builder.Services.AddScoped<IPaymentService,PaymentService>();

// ================= Identity =================
builder.Services.AddAuthorization();

builder.Services
    .AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<StoreContext>();

var app = builder.Build();

// ================= Middleware Pipeline =================

// مهم جدًا: CORS لازم قبل auth
app.UseCors("CorsPolicy");

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Identity endpoints
app.MapGroup("api").MapIdentityApi<AppUser>();

// ================= Database Migration =================
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