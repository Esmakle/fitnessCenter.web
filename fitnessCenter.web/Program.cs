using fitnessCenter.web.Data;
using fitnessCenter.web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// -----------------------
// 1. BUILDER TANIMLAMA (SADECE BİR KEZ OLMALI)
// -----------------------
var builder = WebApplication.CreateBuilder(args);

// -----------------------
// 2. TEMEL SERVİSLER VE VERİTABANI
// -----------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// -----------------------
// 3. AI SETTINGS + SERVICE (İKİNCİ TANIMLAMA KALDIRILDI)
// -----------------------
// builder değişkeni yukarıda zaten tanımlandığı için tekrar 'var builder = ...' yazmıyoruz.

builder.Services.AddScoped<IAiService, GeminiAiService>();

// HttpClient'ı IHttpClientFactory yerine doğrudan DI ile ekliyoruz
builder.Services.AddHttpClient();

// -----------------------
// 4. MİMARİ SERVİSLER
// -----------------------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// -----------------------
// 5. UYGULAMAYI OLUŞTUR
// -----------------------
var app = builder.Build();

// -----------------------
// 6. ROLE SEED + ADMIN SEED (ASYNC İŞLEMİ app.Run()'dan önce yapılmalı)
// -----------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    // Roller
    string[] roles = { "Admin", "Member" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Admin kullanıcı
    string adminEmail = "kolesmanurr@gmail.com";
    string adminPassword = "Esmanur123.";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createResult.Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
    else
    {
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// -----------------------
// 7. HTTP PIPELINE
// -----------------------
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();