using System.Text;
using GrueneisR.RestClientGenerator;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using SecureVeloMobilWebsite.Model;
using SecureVeloMobilWebsite.Services;
using VeloMobilDb;

string corsKey = "_myCorsKey";
string swaggerVersion = "v1";
string swaggerTitle = "MinApiDemo";
string restClientFolder = Environment.CurrentDirectory;
string restClientFilename = "_requests.http";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

#region -------------------------------------------- ConfigureServices

builder.Services.AddControllers();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services
    .AddEndpointsApiExplorer()
    .AddAuthorization()
    .AddSwaggerGen(x => x.SwaggerDoc(
        swaggerVersion,
        new OpenApiInfo { Title = swaggerTitle, Version = swaggerVersion }
    ))
    .AddCors(options => options.AddPolicy(
        corsKey,
        x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
    ))
    .AddRestClientGenerator(options => options
            .SetFolder(restClientFolder)
            .SetFilename(restClientFilename)
            .SetAction($"swagger/{swaggerVersion}/swagger.json")
        //.EnableLogging()
    );

//builder.Services.AddScoped<ICategoriesService, CategoriesService>();

//builder.Services.AddLogging(x => x.AddCustomFormatter());
string? connectionStringMariaDb = builder.Configuration.GetConnectionString("VeloMobilMariaDb");

string? connectionString = builder.Configuration.GetConnectionString("VeloMobilDb");
string location = System.Reflection.Assembly.GetEntryAssembly()!.Location;
string dataDirectory = Path.GetDirectoryName(location)!;
Console.WriteLine("Path: " + dataDirectory);
connectionString = connectionString?.Replace("|DataDirectory|", dataDirectory + Path.DirectorySeparatorChar);
Console.WriteLine($"******** ConnectionString: {connectionStringMariaDb}");
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine($"******** Don't forget to comment out NorthwindContext.OnConfiguring !");
Console.ResetColor();

builder.Services.AddDbContext<VeloMobilContext>(options => options
    .UseMySql(connectionStringMariaDb,
        ServerVersion.Create(new Version(11, 1, 2), ServerType.MariaDb)));
builder.Services.AddLogging();
builder.Services.AddHostedService<StartupBackgroundService>();
builder.Services.AddScoped<VeloMobilService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromHours(10); });

//Authentication
var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings = appSettingsSection.Get<AppSettings>() ?? new();
string secret = appSettings.Secret;

byte[]? key = Encoding.ASCII.GetBytes(secret);
builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

#endregion

var app = builder.Build();

#region -------------------------------------------- Middleware pipeline

app.UseHttpLogging();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    Console.WriteLine("++++ Swagger enabled: http://localhost:5000 (to set as default route: see launchsettings.json)");
    app.UseSwagger();
    Console.WriteLine($@"++++ RestClient generating (after first request) to {restClientFolder}\{restClientFilename}");
    app.UseRestClientGenerator(); //Note: must be used after UseSwagger
    app.UseSwaggerUI(x => x.SwaggerEndpoint($"/swagger/{swaggerVersion}/swagger.json", swaggerTitle));
}

app.UseCors(corsKey);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//app.UseExceptionHandler(config => config.Run(async context =>
//{
//  context.Response.StatusCode = StatusCodes.Status500InternalServerError;
//  context.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Json; //"application/json"
//  var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
//  if (error != null)
//  {
//    await context.Response.WriteAsync(
//      $"Exception: {error.Error?.Message} {error.Error?.InnerException?.Message}");
//  }
//}));

#endregion

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();
app.MapRazorPages();
app.MapControllers();

app.Run();