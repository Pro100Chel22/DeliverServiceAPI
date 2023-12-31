﻿using Microsoft.EntityFrameworkCore;

namespace ASPDotNetWebAPI.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<DishInCart> DishInCarts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<House> Houses { get; set; }
        public DbSet<Hierarchy> Hierarchys { get; set; }
        public DbSet<AddressElement> AddressElements { get; set; }
        public DbSet<RegionTimeZone> RegionTimeZones { get; set; }
        public DbSet<RefreshTokens> RefreshTokens { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            //Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Adding restrictions for the RefreshTokens link
            modelBuilder.Entity<RefreshTokens>()
                .HasOne(refreshTokens => refreshTokens.User)
                .WithMany()
                .HasForeignKey(refreshTokens => refreshTokens.UserId)
                .IsRequired();
            modelBuilder.Entity<RefreshTokens>()
                .HasKey(refreshTokens => refreshTokens.Id);

            // Adding restrictions for the Rating link
            modelBuilder.Entity<Rating>()
                .HasOne(rating => rating.Dish)
                .WithMany(/*dish => dish.Ratings*/)
                .HasForeignKey(rating => rating.DishId)
                .IsRequired();
            modelBuilder.Entity<Rating>()
                .HasOne(rating => rating.User)
                .WithMany(/*user => user.Ratings*/)
                .HasForeignKey(rating => rating.UserId)
                .IsRequired();
            modelBuilder.Entity<Rating>()
                .HasKey(rating => new { rating.DishId, rating.UserId });

            // Adding restrictions for the DishInCart link
            modelBuilder.Entity<DishInCart>()
                .HasOne(dishInCart => dishInCart.Dish)
                .WithMany()
                .HasForeignKey(dishInCart => dishInCart.DishId)
                .IsRequired();
            modelBuilder.Entity<DishInCart>()
                .HasOne(dishInCart => dishInCart.User)
                .WithMany()
                .HasForeignKey(dishInCart => dishInCart.UserId)
                .IsRequired();
            modelBuilder.Entity<DishInCart>()
                .HasKey(dishInCart => dishInCart.Id);

            // Adding restrictions for the Order link
            modelBuilder.Entity<Order>()
                .HasOne(order => order.User)
                .WithMany(/*user => user.Orders*/)
                .HasForeignKey(order => order.UserId)
                .IsRequired();
        }
    }
}
