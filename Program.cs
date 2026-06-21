using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Data;
using SurveyApp.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var context = services.GetRequiredService<ApplicationDbContext>();
	context.Database.EnsureCreated();

	var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
	string[] roles = { "Ankieter", "Respondent" };

	foreach (var role in roles)
	{
		if (!await roleManager.RoleExistsAsync(role))
		{
			await roleManager.CreateAsync(new IdentityRole(role));
		}
	}

	var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
	string adminEmail = "admin@ankiety.pl";
	string adminPassword = "Admin123!";

	var adminUser = await userManager.FindByEmailAsync(adminEmail);
	if (adminUser == null)
	{
		var newAdmin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
		var createResult = await userManager.CreateAsync(newAdmin, adminPassword);
		if (createResult.Succeeded)
		{
			await userManager.AddToRoleAsync(newAdmin, "Ankieter");
		}
	}
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
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
    pattern: "{controller=Survey}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();