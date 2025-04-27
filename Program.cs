using SchoolSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SchoolSystem.Services;
using Microsoft.Extensions.DependencyInjection;
using WebOptimizer;
using SchoolSystem.Models.UserManagement;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddLogging();
builder.Services.AddControllers();
builder.Services.AddControllersWithViews()
    .AddViewLocalization() // Enable view localization
    .AddDataAnnotationsLocalization();
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

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("th-TH") };
    options.DefaultRequestCulture = new RequestCulture("th-TH");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

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
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/NotAuthorized");
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
    Options.AddPolicy("StudentCouncil", policy => policy.RequireRole("StudentCouncil"));
    Options.AddPolicy("DirectorPolicy", policy => policy.RequireRole("Director"));
    Options.AddPolicy("AcademicPolicyOrAdminPolicy", policy => policy.RequireRole("Academic", "Admin"));
    Options.AddPolicy("TeacherPolicyOrStudentCouncilPolicy", policy => policy.RequireRole("Teacher", "StudentCouncil"));
    Options.AddPolicy("AcademicPolicyOrAdminPolicyOrStudentPolicy", policy => policy.RequireRole("Academic", "Admin", "Student"));

});

builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/bundle.css", "css/*.css");
    pipeline.AddJavaScriptBundle("/js/bundle.js", "js/*.js");
});

var app = builder.Build();
await SeedService.SeedDatabase(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("Home/Error");
    app.UseHsts();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = "/static"
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebOptimizer();
app.UseRequestLocalization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllers();

app.Run();
