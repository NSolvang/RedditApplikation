using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Json;

using Data;
using Service;
using shared.Model;

var builder = WebApplication.CreateBuilder(args);

// Sætter CORS så API'en kan bruges fra andre domæner
var AllowSomeStuff = "_AllowSomeStuff";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSomeStuff, builder => {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Tilføj DbContext factory som service.
builder.Services.AddDbContext<PostContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ContextSQLite")));

// Tilføj DataService så den kan bruges i endpoints
builder.Services.AddScoped<DataService>();

// Dette kode kan bruges til at fjerne "cykler" i JSON objekterne.
/*
builder.Services.Configure<JsonOptions>(options =>
{
    // Her kan man fjerne fejl der opstår, når man returnerer JSON med objekter,
    // der refererer til hinanden i en cykel.
    // (altså dobbelrettede associeringer)
    options.SerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
*/

var app = builder.Build();

// Seed data hvis nødvendigt.
using (var scope = app.Services.CreateScope())
{
    var dataService = scope.ServiceProvider.GetRequiredService<DataService>();
    dataService.SeedData(); // Fylder data på, hvis databasen er tom. Ellers ikke.
}

app.UseHttpsRedirection();
app.UseCors(AllowSomeStuff);

// Middlware der kører før hver request. Sætter ContentType for alle responses til "JSON".
app.Use(async (context, next) =>
{
    context.Response.ContentType = "application/json; charset=utf-8";
    await next(context);
});

app.MapGet("/api/posts", (DataService service) =>
{
    return service.GetPosts().Select(p => new
    {
        id = p.Id,
        title = p.Title,
        content = p.Content,
        user = p.User == null ? null : new
        {
            id = p.User.Id, 
            username = p.User.Username
        },
        upvotes = p.Upvotes,
        downvotes = p.Downvotes,
        comments = p.Comments.Select(c => new
        {
            id = c.Id,
            content = c.Content,
            upvotes = c.Upvotes,
            downvotes = c.Downvotes,
            user = c.User == null ? null : new
            {
                id = c.User.Id,  
                username = c.User.Username
            }
        })
    });
});


app.MapGet("/api/posts/{id}", (DataService service, int id) =>
{
    return service.GetPost(id);
});

app.MapPut("/api/posts/{id}/upvote", (DataService service, int id) =>
{
    return service.UpVotePost(id);
});

app.MapPut("/api/posts/{id}/downvote", (DataService service, int id) =>
{
    return service.DownVotePost(id);
});


app.MapPut("/api/posts/{postid}/comments/{commentid}/upvote", (DataService service, int postId, int commentId) =>
{
    return service.UpVoteComment(postId, commentId);
});

app.MapPut("/api/posts/{postid}/comments/{commentid}/downvote", (DataService service, int postId, int commentId) =>
{
    return service.DownVoteComment(postId, commentId);
});

app.MapPost("/api/posts", (DataService service, NewPostData data) =>
{
    Post createdPost = service.CreatePost(data.Title, data.Content, data.UserId);
    return Results.Ok(createdPost); 
});


app.MapPost("/api/posts/{id}/comments", (DataService service, NewCommentData data) =>
{
    service.CreateComment(data.Content, data.UserId, data.PostId);
    var post = service.GetPost(data.PostId);
    var newComment = post.Comments.Last(); // hent den senest tilføjede
    return newComment;
});

app.Run();

record NewPostData(string Title, string Content, int UserId);
record NewCommentData(string Content, int UserId, int PostId);



