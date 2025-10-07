using Data;
using Microsoft.EntityFrameworkCore;
using shared.Model;

namespace Service;

public class DataService
{
    private PostContext db { get; }

    public DataService(PostContext db)
    {
        this.db = db;
    }
    
    public void SeedData() 
    {
        if (!db.Users.Any())
        {
            var user1 = new User { Username = "Kristian" };
            var user2 = new User { Username = "Søren" };
            var user3 = new User { Username = "Mette" };

            db.Users.AddRange(user1, user2, user3);
            db.SaveChanges();
        }

        if (!db.Post.Any())
        {
            var user = db.Users.First();

            var post = new Post
            {
                Title = "Første tråd",
                Content = "Velkommen til den første tråd!",
                User = user,
                Comments = new List<Comment>
                {
                    new Comment { Content = "Første kommentar", Upvotes = 0, Downvotes = 0, User = user },
                    new Comment { Content = "Anden kommentar", Upvotes = 0, Downvotes = 0, User = user },
                }
            };

            db.Post.Add(post);
            db.SaveChanges();
        }
    }

    public List<Post> GetPosts()
    {
        return db.Post
            .Include(p => p.User)
            .Include(p => p.Comments)
            .ThenInclude(c => c.User)
            .ToList();
    }

    public Post GetPost(int id)
    {
        return db.Post
            .Include(p => p.User)
            .Include(p => p.Comments)
            .ThenInclude(c => c.User)
            .FirstOrDefault(p => p.Id == id);
    }

    public Post UpVotePost(int id)
    {
        var post = db.Post.FirstOrDefault(p => p.Id == id);
        if (post == null) return null!;

        post.Upvotes += 1;
        db.SaveChanges();
        return post;
    }

    public Post DownVotePost(int id)
    {
        var post = db.Post.FirstOrDefault(p => p.Id == id);
        if (post == null) return null!;

        post.Downvotes += 1;
        db.SaveChanges();
        return post;
    }

    public Comment UpVoteComment(int postId, int commentId)
    {
        var post = db.Post.Include(p => p.Comments)
            .ThenInclude(c => c.User)
            .FirstOrDefault(p => p.Id == postId);

        if (post == null) return null!;

        var comment = post.Comments.FirstOrDefault(c => c.Id == commentId);
        if (comment == null) return null!;

        comment.Upvotes += 1;
        db.SaveChanges();
        return comment;
    }

    public Comment DownVoteComment(int postId, int commentId)
    {
        var post = db.Post.Include(p => p.Comments)
            .ThenInclude(c => c.User)
            .FirstOrDefault(p => p.Id == postId);

        if (post == null) return null!;

        var comment = post.Comments.FirstOrDefault(c => c.Id == commentId);
        if (comment == null) return null!;

        comment.Downvotes += 1;
        db.SaveChanges();
        return comment;
    }

    public string CreatePost(string title, string content, int userId)
    {
        var user = db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return "User not found";

        var post = new Post
        {
            Title = title,
            Content = content,
            User = user,
            Upvotes = 0,
            Downvotes = 0,
            Comments = new List<Comment>()
        };

        db.Post.Add(post);
        db.SaveChanges();
        return "Post created";
    }

    public string CreateComment(string content, int userId, int postId)
    {
        var user = db.Users.FirstOrDefault(u => u.Id == userId);
        var post = db.Post.Include(p => p.Comments).FirstOrDefault(p => p.Id == postId);

        if (user == null || post == null) return "User or post not found";

        var comment = new Comment
        {
            Content = content,
            User = user,
            Upvotes = 0,
            Downvotes = 0
        };

        post.Comments.Add(comment);
        db.SaveChanges();

        return "Comment created";
    }
}
