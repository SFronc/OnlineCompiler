
using System.Security.Cryptography;
using System.Text;
using OnlineCompiler.Data;
using OnlineCompiler.Models;

namespace OnlineCompiler.Services
{
    public static class DbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider, 
                                      string adminUsername, 
                                      string adminPassword)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataBaseContext>();
        
        await context.Database.EnsureCreatedAsync();

        if (!context.User.Any())
        {
            var admin = new User
            {
                Username = adminUsername,
                Role = "Admin",
                PasswordHash = HashPassword(adminUsername, MD5.Create()),
                RegisterDate = DateTime.UtcNow,
                Email = "Placeholder"
            };
        
            
            context.User.Add(admin);
            await context.SaveChangesAsync();
        }
    }

    public static string HashPassword(string input, HashAlgorithm hasher)
        {
            Encoding enc = Encoding.UTF8;
            var hashBuilder = new StringBuilder();
            byte[] result = hasher.ComputeHash(enc.GetBytes(input));
            foreach (var b in result)
                hashBuilder.Append(b.ToString("x2"));
            return hashBuilder.ToString();
        }
}
}