using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WhoWantToBeAMillionaire.Data;
using WhoWantToBeAMillionaire.Models;

var builder = WebApplication.CreateBuilder(args);


//ConnectionDb
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DbConnection"]);
});

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

var app = builder.Build();
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


//Seeding Data

var scope = app.Services.CreateScope();

var service = scope.ServiceProvider;
await UserRoleInitializer.InitializeAsync(service);

app.Run();
