using fitnessCenter.web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// DbContext (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity + Roles
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        // Şifre kurallarını yumuşatmak istersen:
        // options.Password.RequireNonAlphanumeric = false;
        // options.Password.RequireUppercase = false;
        // options.Password.RequireDigit = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC + Razor Pages (Identity UI için gerekli)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// =======================
//   ROL + İLK TRAINER SEED
// =======================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    // Sadece Trainer rolü gerekli
    string[] roles = new[] { "Trainer" };

    foreach (var role in roles)
    {
        var roleExists = await roleManager.RoleExistsAsync(role);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // İlk TRAINER kullanıcısı
    string trainerEmail = "kolesmanurr@gmail.com";   // admin hesabın
    string trainerPassword = "Esmanur123.";          // ilk giriş için şifre

    var trainerUser = await userManager.FindByEmailAsync(trainerEmail);
    if (trainerUser == null)
    {
        trainerUser = new IdentityUser
        {
            UserName = trainerEmail,
            Email = trainerEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(trainerUser, trainerPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(trainerUser, "Trainer");
        }
    }
}

// =======================
//    HTTP PIPELINE
// =======================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Identity Razor Pages (Login / Register)
app.MapRazorPages();

app.Run();
