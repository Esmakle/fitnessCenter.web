using fitnessCenter.web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// appsettings.json içindeki ConnectionStrings:DefaultConnection'ý okuyoruz
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ApplicationDbContext'i PostgreSQL ile kaydediyoruz
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ?? Identity servisleri
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        // Ýstersen þifre kurallarýný yumuþatmak için burayý açabilirsin:
        // options.Password.RequireNonAlphanumeric = false;
        // options.Password.RequireUppercase = false;
        // options.Password.RequireDigit = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

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

// ?? Önce Authentication, sonra Authorization
app.UseAuthentication();
app.UseAuthorization();

// MVC için klasik route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Attribute route kullanan API controller'lar için
app.MapControllers();

app.Run();
