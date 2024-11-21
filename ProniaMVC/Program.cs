using Microsoft.EntityFrameworkCore;
using ProniaMVC.DAL;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContex>(opt =>
    opt.UseSqlServer("server=HP\\SQLEXPRESS;database=ProniaDB;trusted_connection=true;integrated security=true;TrustServerCertificate=true;")
);

var app = builder.Build();
app.UseStaticFiles();
app.MapControllerRoute
    (
    "default",
    "{controller=home}/{action=index}/{id?}"
    );



app.Run();
