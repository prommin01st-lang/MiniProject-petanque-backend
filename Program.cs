
using System.Text;
using JWTdemo.Data;
using JWTdemo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization; // 👈 1. [เพิ่ม] Import นี้
// for Image Uploader
using Microsoft.Extensions.FileProviders; // 👈 1. [เพิ่ม] Import นี้
using System.IO; // 👈 2. [เพิ่ม] Import นี้

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers().AddJsonOptions(options =>
    {
        // 4. [เพิ่ม] สั่งให้มันเพิกเฉยต่อการอ้างอิงวนลูป
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserDatabase")));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JWTdemo API", Version = "v1" });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["AppSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["AppSettings:Audience"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)
        ),
        ValidateIssuerSigningKey = true
    };  
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();

// เพิ่ม CORS Service
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextApp", // ตั้งชื่อ Policy
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") // **ใส่ Origin ของ Next.js app ของคุณ**
                  .AllowAnyHeader()   // อนุญาตทุก Header
                  .AllowAnyMethod();  // อนุญาตทุก Method (GET, POST, PUT, DELETE, etc.)
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {   
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "JWTdemo API v1");
        c.RoutePrefix = string.Empty;
    }
    );
}

// for Upload Image
app.UseStaticFiles(); // 👈 (อันนี้สำหรับ wwwroot ทั่วไป)

app.UseStaticFiles(new StaticFileOptions
{
    // Path ที่ไฟล์จะถูกเก็บ (เช่น F:/.../wwwroot/uploads)
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads")),
    // Path ที่ Browser จะเรียก (เช่น http://localhost:5139/uploads)
    RequestPath = "/uploads"
});


//app.UseHttpsRedirection();

app.UseCors("AllowNextApp"); // ใช้ Policy ที่เราตั้งชื่อไว้

app.UseAuthorization();

app.MapControllers();

app.Run();
