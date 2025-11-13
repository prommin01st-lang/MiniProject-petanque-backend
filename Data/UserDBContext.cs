using JWTdemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace JWTdemo.Data
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotificationStatus> UserNotificationStatus { get; set; }

        // 👇 1. [แก้ไข/เพิ่ม] ตารางใหม่
        public DbSet<TodoListCategory> TodoListCategories { get; set; }

        // (ถ้าคุณลบ Migration เก่า, DbSet<TodoItem> เก่าจะหายไป)
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleLike> ArticleLikes { get; set; }

        public DbSet<ArticleComment> ArticleComments { get; set; }

        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- (โค้ดเดิมที่คุณอาจจะมี สำหรับ ArticleLike) ---
            modelBuilder.Entity<ArticleLike>()
                .HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.NoAction); // 👈 (อันเดิม)

            // --- (โค้ดเดิมที่คุณอาจจะมี สำหรับ UserNotificationStatus) ---
            modelBuilder.Entity<UserNotificationStatus>()
                .HasOne(uns => uns.User)
                .WithMany()
                .HasForeignKey(uns => uns.UserId)
                .OnDelete(DeleteBehavior.NoAction); // 👈 (อันเดิม)

            // --- 👇 3. [เพิ่ม] Logic ใหม่สำหรับ Comment ---
            // (ป้องกันการสับสนระหว่าง User -> Comment และ Article -> Comment)
            modelBuilder.Entity<ArticleComment>()
                .HasOne(ac => ac.User) // (Comment มี 1 User)
                .WithMany() // (User มีหลาย Comments)
                .HasForeignKey(ac => ac.UserId)
                .OnDelete(DeleteBehavior.NoAction); // 👈 [สำคัญ] ห้าม Cascade

            modelBuilder.Entity<Conversation>()
            .HasOne(c => c.User1)
            .WithMany()
            .HasForeignKey(c => c.User1Id)
            .OnDelete(DeleteBehavior.NoAction);

            // ป้องกัน User2 -> Conversation
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.User2)
                .WithMany()
                .HasForeignKey(c => c.User2Id)
                .OnDelete(DeleteBehavior.NoAction);

            // ป้องกัน Sender -> Message
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }

    }
}