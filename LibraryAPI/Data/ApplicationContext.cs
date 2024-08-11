using System;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace LibraryAPI.Data
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public DbSet<Location>? Locations { get; set; }
        public DbSet<Language>? Languages { get; set; }
        public DbSet<Category>? Categories { get; set; }
        public DbSet<SubCategory>? SubCategories { get; set; }
        public DbSet<Publisher>? Publishers { get; set; }
        public DbSet<Author>? Authors { get; set; }
        public DbSet<Book>? Books { get; set; }
        public DbSet<AuthorBook>? AuthorBook { get; set; }
        public DbSet<Member>? Members { get; set; }
        public DbSet<Employee>? Employees { get; set; }
        public DbSet<BookLanguage>? BookLanguage { get; set; }
        public DbSet<BookSubCategory>? BookSubCategory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AuthorBook>().HasKey(a => new { a.AuthorsId, a.BooksId });

            modelBuilder.Entity<BookLanguage>().HasKey(d => new { d.LanguagesCode, d.BooksId });

            modelBuilder.Entity<BookSubCategory>().HasKey(c => new { c.SubCategoriesId, c.BooksId });

            modelBuilder.Entity<Borrow>()
                .HasOne(b => b.Member)
                .WithMany(x => x.Borrows)
                .HasForeignKey(b => b.MembersId)
                .OnDelete(DeleteBehavior.Restrict);




        }
        
        public DbSet<LibraryAPI.Models.Borrow> Borrow { get; set; } = default!;
        public DbSet<LibraryAPI.Models.BookCopy> BookCopy { get; set; } = default!;
    }
}
