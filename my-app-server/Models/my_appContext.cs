using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace my_app_server.Models
{
    public partial class my_appContext : DbContext
    {
        public virtual DbSet<ActionToken> ActionToken { get; set; }
        public virtual DbSet<Heros> Heros { get; set; }
        public virtual DbSet<Tokens> Tokens { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<UsersHeros> UsersHeros { get; set; }
        public virtual DbSet<UserToken> UserToken { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActionToken>(entity =>
            {
                entity.HasKey(e => e.HeroId);

                entity.HasIndex(e => e.ExpireDate)
                    .HasName("IX_ActionToken_1");

                entity.HasIndex(e => e.HeroId)
                    .HasName("IX_ActionToken")
                    .IsUnique();

                entity.Property(e => e.HeroId)
                    .HasColumnName("HeroID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ExpireDate).HasColumnType("datetime");

                entity.Property(e => e.HashedToken)
                    .IsRequired()
                    .HasColumnType("nchar(64)");

                entity.HasOne(d => d.Hero)
                    .WithOne(p => p.ActionToken)
                    .HasForeignKey<ActionToken>(d => d.HeroId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ActionToken_Heros");
            });

            modelBuilder.Entity<Heros>(entity =>
            {
                entity.HasKey(e => e.HeroId);

                entity.HasIndex(e => e.Name)
                    .HasName("IX_Heros")
                    .IsUnique();

                entity.Property(e => e.HeroId)
                    .HasColumnName("HeroID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Hp).HasColumnName("HP");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Nickname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Sl).HasColumnName("SL");
            });

            modelBuilder.Entity<Tokens>(entity =>
            {
                entity.HasKey(e => e.TokenName);

                entity.HasIndex(e => e.UserName)
                    .HasName("IX_Tokens")
                    .IsUnique();

                entity.Property(e => e.TokenName)
                    .HasColumnType("nchar(64)")
                    .ValueGeneratedNever();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.ExpireDate).HasColumnType("datetime");

                entity.Property(e => e.HashedToken)
                    .IsRequired()
                    .HasColumnType("nchar(64)");

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.UserNameNavigation)
                    .WithOne(p => p.Tokens)
                    .HasForeignKey<Tokens>(d => d.UserName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tokens_Users1");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.Name);

                entity.HasIndex(e => e.Email)
                    .HasName("IX_Email")
                    .IsUnique();

                entity.HasIndex(e => e.LastLogin)
                    .HasName("IX_LastLogin");

                entity.HasIndex(e => e.RegistryDate)
                    .HasName("IX_RegistryDate");

                entity.Property(e => e.Name)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastLogin).HasColumnType("datetime");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnType("nchar(64)");

                entity.Property(e => e.RegistryDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<UsersHeros>(entity =>
            {
                entity.HasKey(e => e.HeroId);

                entity.HasIndex(e => e.HeroId)
                    .HasName("IX_UsersHeros_2");

                entity.HasIndex(e => e.UserName)
                    .HasName("IX_UsersHeros_1");

                entity.HasIndex(e => new { e.UserName, e.HeroId })
                    .HasName("IX_UsersHeros")
                    .IsUnique();

                entity.Property(e => e.HeroId)
                    .HasColumnName("HeroID")
                    .ValueGeneratedNever();

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.Hero)
                    .WithOne(p => p.UsersHeros)
                    .HasForeignKey<UsersHeros>(d => d.HeroId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UsersHeros_Heros");

                entity.HasOne(d => d.UserNameNavigation)
                    .WithMany(p => p.UsersHeros)
                    .HasForeignKey(d => d.UserName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UsersHeros_Users");
            });

            modelBuilder.Entity<UserToken>(entity =>
            {
                entity.HasKey(e => e.UserName);

                entity.HasIndex(e => e.ExpireDate)
                    .HasName("IX_UserToken_1");

                entity.HasIndex(e => e.UserName)
                    .HasName("IX_UserToken")
                    .IsUnique();

                entity.Property(e => e.UserName)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.ExpireDate).HasColumnType("datetime");

                entity.Property(e => e.HashedToken)
                    .IsRequired()
                    .HasColumnType("nchar(64)");

                entity.HasOne(d => d.UserNameNavigation)
                    .WithOne(p => p.UserToken)
                    .HasForeignKey<UserToken>(d => d.UserName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserToken_Users");
            });
        }
    }
}
