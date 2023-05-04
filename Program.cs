using Microsoft.EntityFrameworkCore;
using ToDoAPI.Data;
using ToDoAPI.Models;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));


// Allow CORS and content types
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins("http://localhost:5173");
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("content-disposition")
              .WithHeaders("content-type");
    });
});

var app = builder.Build();

//app.UseHttpsRedirection();

// All entries
app.MapGet("api/todo", async (AppDbContext context) =>
{
    var items = await context.ToDos.ToListAsync();

    return Results.Ok(items);
});

// Add entry
app.MapPost("api/todo", async (AppDbContext context, ToDo toDo) =>
{
    await context.ToDos.AddAsync(toDo);

    await context.SaveChangesAsync();

    return Results.Created($"api/todo/{toDo.Id}", toDo);
});

// Update entry
app.MapPut("api/todo/{id}", async (AppDbContext context, int id, ToDo toDo) =>
{
    var toDoModel = await context.ToDos.FirstOrDefaultAsync(t => t.Id == id);

    if (toDoModel == null)
        return Results.NotFound();

    toDoModel.ToDoName = toDo.ToDoName;

    await context.SaveChangesAsync();

    return Results.NoContent();
});

// Delete entry
app.MapDelete("api/todo/{id}", async (AppDbContext context, int id) =>
{
    var toDoModel = await context.ToDos.FirstOrDefaultAsync(t => t.Id == id);

    if (toDoModel == null)
        return Results.NotFound();

    context.ToDos.Remove(toDoModel);

    await context.SaveChangesAsync();

    return Results.NoContent();
});

app.UseCors(MyAllowSpecificOrigins);

app.Run();
