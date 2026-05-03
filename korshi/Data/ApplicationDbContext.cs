using korshi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace korshi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Complex> Complexes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollOption> PollOptions { get; set; }
        public DbSet<PollVote> PollVotes { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<DirectMessage> DirectMessages { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<FinancialReport> FinancialReports { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ── Post ────────────────────────────────────────
            builder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Post>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Comment ─────────────────────────────────────
            builder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Like ────────────────────────────────────────
            builder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.PostId })
                .IsUnique();

            builder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Poll ────────────────────────────────────────
            builder.Entity<Poll>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PollOption>()
                .HasOne(o => o.Poll)
                .WithMany(p => p.Options)
                .HasForeignKey(o => o.PollId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── PollVote ─────────────────────────────────────
            builder.Entity<PollVote>()
                .HasIndex(v => new { v.UserId, v.PollId })
                .IsUnique();

            builder.Entity<PollVote>()
                .HasOne(v => v.Poll)
                .WithMany(p => p.Votes)
                .HasForeignKey(v => v.PollId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PollVote>()
                .HasOne(v => v.Option)
                .WithMany(o => o.Votes)
                .HasForeignKey(v => v.PollOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PollVote>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── ServiceRequest ───────────────────────────────
            builder.Entity<ServiceRequest>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ServiceRequest>()
                .HasOne(r => r.Service)
                .WithMany(s => s.Requests)
                .HasForeignKey(r => r.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Banner ───────────────────────────────────────
            builder.Entity<Banner>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Conversation ─────────────────────────────────
            builder.Entity<Conversation>()
                .HasOne(c => c.User1)
                .WithMany()
                .HasForeignKey(c => c.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Conversation>()
                .HasOne(c => c.User2)
                .WithMany()
                .HasForeignKey(c => c.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            // ── DirectMessage ────────────────────────────────
            builder.Entity<DirectMessage>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DirectMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Friendship ───────────────────────────────────
            builder.Entity<Friendship>()
                .HasIndex(f => new { f.RequesterId, f.AddresseeId })
                .IsUnique();

            builder.Entity<Friendship>()
                .HasOne(f => f.Requester)
                .WithMany()
                .HasForeignKey(f => f.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Friendship>()
                .HasOne(f => f.Addressee)
                .WithMany()
                .HasForeignKey(f => f.AddresseeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Notification ─────────────────────────────────
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Event ─────────────────────────────────────────
            builder.Entity<Event>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Announcement ──────────────────────────────────
            builder.Entity<Announcement>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── FinancialReport ───────────────────────────────
            builder.Entity<FinancialReport>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Seed: ЖК ─────────────────────────────────────
            builder.Entity<Complex>().HasData(new Complex
            {
                Id = 1,
                Name = "Nurly Tau",
                Address = "ул. Аль-Фараби, 77",
                City = "Алматы"
            });

            // ── Seed: Категории ──────────────────────────────
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Стена", Slug = "wall", Color = "#6B7280", Icon = "bi-house-door", SortOrder = 1 },
                new Category { Id = 2, Name = "Барахолка", Slug = "market", Color = "#F59E0B", Icon = "bi-bag", SortOrder = 2 },
                new Category { Id = 3, Name = "Взаимовыручка", Slug = "aid", Color = "#10B981", Icon = "bi-heart", SortOrder = 3 },
                new Category { Id = 4, Name = "Голосования", Slug = "poll", Color = "#8B5CF6", Icon = "bi-bar-chart-line", SortOrder = 4 }
            );

            // ── Seed: Услуги ─────────────────────────────────
            builder.Entity<Service>().HasData(
                new Service { Id = 1, Name = "Сантехника", Description = "Вызов сантехника", Icon = "bi-droplet", Color = "#3B82F6", SortOrder = 1 },
                new Service { Id = 2, Name = "Электрика", Description = "Вызов электрика", Icon = "bi-lightning-charge", Color = "#F59E0B", SortOrder = 2 },
                new Service { Id = 3, Name = "Уборка", Description = "Уборка территории", Icon = "bi-stars", Color = "#10B981", SortOrder = 3 },
                new Service { Id = 4, Name = "Охрана", Description = "Вопросы безопасности", Icon = "bi-shield-check", Color = "#8B5CF6", SortOrder = 4 },
                new Service { Id = 5, Name = "Другое", Description = "Прочие обращения", Icon = "bi-three-dots", Color = "#6B7280", SortOrder = 5 }
            );
        }
    }
}