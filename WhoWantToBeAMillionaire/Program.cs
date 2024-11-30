using Microsoft.EntityFrameworkCore;
using WhoWantToBeAMillionaire.Data;

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
