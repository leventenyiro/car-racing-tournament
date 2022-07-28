﻿using formula1_tournament_api.Models;
using Microsoft.EntityFrameworkCore;

namespace formula1_tournament_api.Data
{
    public class FormulaDbContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<Season> Season { get; set; }
        public DbSet<Team> Team { get; set; }
        public DbSet<Racer> Racer { get; set; }
        public DbSet<Race> Race { get; set; }

        public FormulaDbContext(DbContextOptions<FormulaDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                //entity.ToTable("user");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username)
                    .IsRequired();
                entity.Property(e => e.Password)
                    .IsRequired();
            });

            modelBuilder.Entity<Season>(entity =>
            {
                //entity.ToTable("season");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired();
                entity.Property(e => e.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<Team>(entity =>
            {
                //entity.ToTable("team");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired();
                entity.Property(e => e.Color)
                    .IsRequired();
                entity.Property(e => e.SeasonId)
                    .IsRequired();
            });

            modelBuilder.Entity<Racer>(entity =>
            {
                //entity.ToTable("racer");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired();
                entity.Property(e => e.TeamId)
                    .IsRequired();
                entity.Property(e => e.SeasonId)
                    .IsRequired();
            });

            modelBuilder.Entity<Race>(entity =>
            {
                //entity.ToTable("race");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Position)
                    .IsRequired();
                entity.Property(e => e.Points)
                    .IsRequired();
                entity.Property(e => e.RacerId)
                    .IsRequired();
                entity.Property(e => e.RacerId)
                    .IsRequired();
            });
        }
    }
}
