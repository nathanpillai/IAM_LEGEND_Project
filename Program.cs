//using AutoMapper;
//using IAMLegend.Data;
//using IAMLegend.Mapping;
//using IAMLegend.Repositories;
//using IAMLegend.Services;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc.Authorization;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Identity.Web;
//using Microsoft.Identity.Web.UI;
//using Swashbuckle.AspNetCore;

//var builder = WebApplication.CreateBuilder(args);

//// Configuration: Use PostgreSQL by default; switch provider via connection string & package
//var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=onboard;Username=postgres;Password=postgres";

////// Add services to the container.
////builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
////    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

////builder.Services.AddControllersWithViews(options =>
////{
////    var policy = new AuthorizationPolicyBuilder()
////        .RequireAuthenticatedUser()
////        .Build();
////    options.Filters.Add(new AuthorizeFilter(policy));
////});
////builder.Services.AddRazorPages()
////    .AddMicrosoftIdentityUI();

////var app = builder.Build();

////// Configure the HTTP request pipeline.
////if (!app.Environment.IsDevelopment())
////{
////    app.UseExceptionHandler("/Home/Error");
////    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
////    app.UseHsts();
////}

////app.UseHttpsRedirection();
////app.UseStaticFiles();

////app.UseRouting();

////app.UseAuthentication();
////app.UseAuthorization();

////app.MapControllerRoute(
////    name: "default",
////    pattern: "{controller=Home}/{action=Index}/{id?}");
////app.MapRazorPages();

////app.Run();

//// Register DbContext
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseNpgsql(conn));

//// Repositories & Services
//builder.Services.AddScoped<IQueryUserRepository, EfUserRepository>();
//builder.Services.AddScoped<ICommandUserRepository, EfUserRepository>();
//builder.Services.AddScoped<IUserService, UserService>();

//// Automapper
//builder.Services.AddAutoMapper(cfg =>
//{
//    cfg.AddProfile<AutoMapperProfile>();
//});

//// Controllers & swagger
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseAuthorization();
//app.MapControllers();
//app.Run();

using AutoMapper;
using IAMLegend.Data;
using IAMLegend.Mapping;
using IAMLegend.Repositories;
using IAMLegend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration: PostgreSQL connection string
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? "Host=localhost;Database=iamkitty;Username=postgres;Password=test123";

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(conn));

// Repositories & Services
builder.Services.AddScoped<IQueryUserRepository, EfUserRepository>();
builder.Services.AddScoped<ICommandUserRepository, EfUserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoMapperProfile>();
});

// MVC Controllers with views
builder.Services.AddControllersWithViews();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Development environment setup
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Routing & Authorization
app.UseRouting();
app.UseAuthorization();

// MVC routing for views + default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Index}/{id?}");

// Run the app
app.Run();
