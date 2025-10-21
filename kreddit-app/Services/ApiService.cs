using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

using shared.Model;

namespace kreddit_app.Data;

public class ApiService
{
    private readonly HttpClient http;
    private readonly IConfiguration configuration;
    private readonly string baseAPI = "";

    public ApiService(HttpClient http, IConfiguration configuration)
    {
        this.http = http;
        this.configuration = configuration;
        this.baseAPI = configuration["base_api"];
    }

    public async Task<Post[]> GetPosts()
    {
        string url = $"{baseAPI}posts/";
        HttpResponseMessage msg = await http.GetAsync(url);
        string json = await msg.Content.ReadAsStringAsync();
    
        Console.WriteLine("RAW JSON FROM API:");
        Console.WriteLine(json);  // PRINT DEN RÅ JSON
    
        Post[]? posts = JsonSerializer.Deserialize<Post[]>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        });
    
        return posts;
    }

    public async Task<Post> GetPost(int id)
    {
        string url = $"{baseAPI}posts/{id}/";
        HttpResponseMessage msg = await http.GetAsync(url);
        string json = await msg.Content.ReadAsStringAsync();
    
        Post? post = JsonSerializer.Deserialize<Post>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        });
    
        return post;
    }

    public async Task<Comment> CreateComment(string content, int postId, int userId)
    {
        string url = $"{baseAPI}posts/{postId}/comments";
     
        // Post JSON to API, save the HttpResponseMessage
        HttpResponseMessage msg = await http.PostAsJsonAsync(url, new { content, userId, postId});

        // Get the JSON string from the response
        string json = await msg.Content.ReadAsStringAsync();

        // Deserialize the JSON string to a Comment object
        Comment? newComment = JsonSerializer.Deserialize<Comment>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true // Ignore case when matching JSON properties to C# properties 
        });

        // Return the new comment 
        return newComment;
    }

    public async Task<Post> CreatePost(string title, string content, int userId)
    {
        string url = $"{baseAPI}posts";

        // Post JSON til API
        var msg = await http.PostAsJsonAsync(url, new { Title = title, Content = content, UserId = userId });
        msg.EnsureSuccessStatusCode();

        string json = await msg.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(json))
        {
            // Hvis API ikke returnerer JSON, lav Post-objekt lokalt
            var user = new User { Id = userId, Username = "Mads" }; // hårdkodet til test
            return new Post(user, title, content);
        }

        var post = JsonSerializer.Deserialize<Post>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return post!;
    }




    public async Task<Post> UpvotePost(int id)
    {
        string url = $"{baseAPI}posts/{id}/upvote/";

        // Post JSON to API, save the HttpResponseMessage
        HttpResponseMessage msg = await http.PutAsJsonAsync(url, "");

        // Get the JSON string from the response
        string json = msg.Content.ReadAsStringAsync().Result;

        // Deserialize the JSON string to a Post object
        Post? updatedPost = JsonSerializer.Deserialize<Post>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true // Ignore case when matching JSON properties to C# properties
        });

        // Return the updated post (vote increased)
        return updatedPost;
    }
    
    public async Task<Post> DownvotePost(int id)
    {
        string url = $"{baseAPI}posts/{id}/downvote/";

        // Post JSON to API, save the HttpResponseMessage
        HttpResponseMessage msg = await http.PutAsJsonAsync(url, "");

        // Get the JSON string from the response
        string json = msg.Content.ReadAsStringAsync().Result;

        // Deserialize the JSON string to a Post object
        Post? updatedPost = JsonSerializer.Deserialize<Post>(json, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true // Ignore case when matching JSON properties to C# properties
        });

        // Return the updated post (vote increased)
        return updatedPost;
    }
    
    public async Task<Comment> DownvoteComment(int postid, int commentid)
    {
        string url = $"{baseAPI}posts/{postid}/comments/{commentid}/downvote";

        HttpResponseMessage msg = await http.PutAsJsonAsync(url, "");
        string json = await msg.Content.ReadAsStringAsync();

        Comment? updatedComment = JsonSerializer.Deserialize<Comment>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return updatedComment!;
    }

public async Task<Comment> UpvoteComment(int postid, int commentid)
{
    string url = $"{baseAPI}posts/{postid}/comments/{commentid}/upvote";

    HttpResponseMessage msg = await http.PutAsJsonAsync(url, "");
    string json = await msg.Content.ReadAsStringAsync();

    Comment? updatedComment = JsonSerializer.Deserialize<Comment>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    return updatedComment!;
}
}
