using ArtShopApi.Models;

namespace ArtShopApi.Data
{
    public static class Seeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if(!context.Users.Any(u => u.Role == "Admin")){
                
                context.Users.Add(new User
                {
                    Name = "Admin",
                    Email = "thorstore24@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@lesa18"),
                    Role = "Admin"
                });
                await context.SaveChangesAsync();
            }

            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Stickers" },
                    new Category { Name = "Prints" },
                    new Category { Name = "Bookmarks" }
                    );
                await context.SaveChangesAsync();
            }
        }
    }
}
