using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using WhoWantToBeAMillionaire.Areas.Admin.Data;
using WhoWantToBeAMillionaire.Data;
using WhoWantToBeAMillionaire.Hubs;
using WhoWantToBeAMillionaire.Models;

var builder = WebApplication.CreateBuilder(args);

// Thêm SignalR
builder.Services.AddSignalR();
//ConnectionDb
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DbConnection"]);
});

//add email sender
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Thêm dịch vụ Session vào container
builder.Services.AddDistributedMemoryCache(); // Sử dụng bộ nhớ trong để lưu Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn của Session
    options.Cookie.HttpOnly = true;                 // Chỉ cho phép truy cập Session qua HTTP
    options.Cookie.IsEssential = true;              // Cookie cần thiết cho Session hoạt động
});
// Add services to the container.
builder.Services.AddControllersWithViews();

//Identity
builder.Services.AddIdentity<AppUserModel, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders(); 
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;

    options.User.RequireUniqueEmail = true;
});

//configure login google
builder.Services.AddAuthentication(options =>
{
    //options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie().AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/AccountAdmin/Login"; // Đường dẫn đến trang đăng nhập của bạn

    options.AccessDeniedPath = "/AccessDenied";
});

var app = builder.Build();
app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");
app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();//Dang nhap
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Định tuyến cho SignalR
app.MapHub<OnlineCountHub>("/onlinecount");


//Seeding Data

var scope = app.Services.CreateScope();

var service = scope.ServiceProvider;
await UserRoleInitializer.InitializeAsync(service);

app.Run();
