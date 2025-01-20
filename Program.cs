using SchoolSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SchoolSystem.Services;
using SchoolSystem.Models;
using Microsoft.Extensions.DependencyInjection;
using WebOptimizer;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(option =>
option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // อ่าน Token จาก Cookie
                if (context.Request.Cookies.ContainsKey("AuthToken"))
                {
                    context.Token = context.Request.Cookies["AuthToken"];
                }
                return Task.CompletedTask;
            }
        };
    });

// Add services to the container.
builder.Services.AddAuthorization(Options =>
{
    Options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    Options.AddPolicy("StudentPolicy", policy => policy.RequireRole("Student"));
    Options.AddPolicy("TeacherPolicy", policy => policy.RequireRole("Teacher"));
    Options.AddPolicy("AcademicPolicy", policy => policy.RequireRole("Academic"));
    Options.AddPolicy("DirectorPolicy", policy => policy.RequireRole("Director"));

});

builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/bundle.css", "wwwroot/css/tailwind.min.css")
        .UseContentRoot()
        .MinifyCss(); 

    pipeline.AddJavaScriptBundle("/js/bundle.js", "wwwroot/js/*.js")
        .UseContentRoot()
        .MinifyJavaScript(); 
});

var app = builder.Build();
await SeedService.SeedDatabase(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseWebOptimizer(); 

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
