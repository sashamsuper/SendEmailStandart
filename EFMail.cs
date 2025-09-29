using System.IO;
using System;
using Microsoft.EntityFrameworkCore;

namespace PostalService
{
    // <summary>
    // Сохранение полученных сообщений в базе
    // </summary>
    public class PostBase : DbContext
    {
        // <summary>
        // Gets or sets the path to the base.
        // </summary>
        public string PathToBase { set; get; } =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "base.sqlite3");

        //private string PathToBase { set; get; } = Path.Combine("base.sqlite3");

        // <summary>
        // Initializes a new instance of the PostBase class.
        // </summary>
        public DbSet<PostMessage> PostMessages { get; set; }

        // <summary>
        // Gets or sets the attachments.
        // </summary>
        public DbSet<MemoryStreamAttachment> Attachments { get; set; }

        // <summary>
        // Initializes a new instance of the PostBase class.
        // </summary>
        public PostBase()
        {
            if (!this.Database.CanConnect())
            {
                this.Database.Migrate();
            }
        }

        // <summary>
        // Configures the database connection options.
        // </summary>
        // <param name="optionsBuilder">The options builder to configure.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={PathToBase}");
        }

        // <summary>
        // Configures the model for the database context.
        // </summary>
        // <param name="modelBuilder">The model builder to configure.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<PostMessage>()
                .HasMany(p => p.AttachmentFiles)
                .WithOne()
                .HasForeignKey(p => p.PostMessagesId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
