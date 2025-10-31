using BlackjackV3.Services;
using BlackjackV3.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add session support for game state
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register game services
builder.Services.AddSingleton<IRandomProvider, CryptoRandomProvider>();
builder.Services.AddSingleton<IGameStateStore, InMemoryGameStateStore>();

// Register multiplayer services
builder.Services.AddSingleton<ITableManager, InMemoryTableManager>();

// Add SignalR for multiplayer support
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

// Map SignalR hub for multiplayer
app.MapHub<GameHub>("/hub/game");

app.Run();
