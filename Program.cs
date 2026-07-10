using Microsoft.EntityFrameworkCore;
using WeightliftingApi.Data;
using WeightliftingApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Database (SQLite file). DB_PATH env var overrides location. ---
var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "app.db";
builder.Services.AddDbContext<AppDb>(o => o.UseSqlite($"Data Source={dbPath}"));

// --- CORS: set FRONTEND_ORIGIN env var to your deployed frontend URL(s), comma-separated ---
var origins = (Environment.GetEnvironmentVariable("FRONTEND_ORIGIN")
               ?? "http://localhost:5173")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

// Render/Railway inject a PORT env var
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    app.Urls.Add($"http://0.0.0.0:{port}");

// --- Create DB + seed default movements on first run ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.EnsureCreated();
    if (!db.Movements.Any())
    {
        string[] defaults = { "اسنچ", "کلین و جرک", "بک اسکوات", "فرانت اسکوات", "ددلیفت", "پرس سرشانه" };
        db.Movements.AddRange(defaults.Select(n => new Movement { Name = n }));
        db.SaveChanges();
    }
}

app.MapGet("/", () => "Weightlifting API is running ✅");

// ---------- Entries ----------
app.MapGet("/api/entries", async (AppDb db) =>
    await db.Entries
        .OrderByDescending(e => e.Date)
        .ThenByDescending(e => e.Id)
        .ToListAsync());

app.MapPost("/api/entries", async (AppDb db, Entry entry) =>
{
    if (entry.Weight <= 0 || entry.Reps <= 0 ||
        string.IsNullOrWhiteSpace(entry.MovementName) ||
        string.IsNullOrWhiteSpace(entry.Date))
        return Results.BadRequest(new { error = "داده نامعتبر است" });

    entry.Id = 0;
    db.Entries.Add(entry);
    await db.SaveChangesAsync();
    return Results.Created($"/api/entries/{entry.Id}", entry);
});

app.MapDelete("/api/entries/{id:int}", async (AppDb db, int id) =>
{
    var e = await db.Entries.FindAsync(id);
    if (e is null) return Results.NotFound();
    db.Entries.Remove(e);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ---------- Movements ----------
app.MapGet("/api/movements", async (AppDb db) =>
    await db.Movements.OrderBy(m => m.Id).ToListAsync());

app.MapPost("/api/movements", async (AppDb db, Movement m) =>
{
    var name = m.Name?.Trim();
    if (string.IsNullOrEmpty(name))
        return Results.BadRequest(new { error = "نام حرکت خالی است" });
    if (await db.Movements.AnyAsync(x => x.Name == name))
        return Results.Conflict(new { error = "این حرکت قبلاً وجود دارد" });

    var mv = new Movement { Name = name };
    db.Movements.Add(mv);
    await db.SaveChangesAsync();
    return Results.Created($"/api/movements/{mv.Id}", mv);
});

app.Run();
