using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace my_app_server.Models
{
    public partial class my_appContext : DbContext
    {
        public virtual DbSet<ActionToken> ActionToken { get; set; }
        public virtual DbSet<Backpack> Backpack { get; set; }
        public virtual DbSet<Equipment> Equipment { get; set; }
        public virtual DbSet<Heros> Heros { get; set; }
        public virtual DbSet<HerosLocations> HerosLocations { get; set; }
        public virtual DbSet<Items> Items { get; set; }
        public virtual DbSet<LocationsDb> LocationsDb { get; set; }
        public virtual DbSet<Tokens> Tokens { get; set; }
        public virtual DbSet<Traveling> Traveling { get; set; }
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

            modelBuilder.Entity<Backpack>(entity =>
            {
                entity.HasKey(e => new { e.HeroId, e.SlotNr });

                entity.Property(e => e.HeroId).HasColumnName("HeroID");

                entity.Property(e => e.SlotNr).HasColumnName("SlotNR");

                entity.Property(e => e.ItemId).HasColumnName("ItemID");

                entity.HasOne(d => d.Hero)
                    .WithMany(p => p.Backpack)
                    .HasForeignKey(d => d.HeroId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Backpack_Heros");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.Backpack)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Backpack_Items");
            });

            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasKey(e => e.HeroId);

                entity.Property(e => e.HeroId)
                    .HasColumnName("HeroID")
                    .ValueGeneratedNever();

                entity.HasOne(d => d.ArmourNavigation)
                    .WithMany(p => p.EquipmentArmourNavigation)
                    .HasForeignKey(d => d.Armour)
                    .HasConstraintName("FK_Equipment_Items2");

                entity.HasOne(d => d.BraceletNavigation)
                    .WithMany(p => p.EquipmentBraceletNavigation)
                    .HasForeignKey(d => d.Bracelet)
                    .HasConstraintName("FK_Equipment_Items9");

                entity.HasOne(d => d.FirstHandNavigation)
                    .WithMany(p => p.EquipmentFirstHandNavigation)
                    .HasForeignKey(d => d.FirstHand)
                    .HasConstraintName("FK_Equipment_Items");

                entity.HasOne(d => d.GlovesNavigation)
                    .WithMany(p => p.EquipmentGlovesNavigation)
                    .HasForeignKey(d => d.Gloves)
                    .HasConstraintName("FK_Equipment_Items5");

                entity.HasOne(d => d.Hero)
                    .WithOne(p => p.Equipment)
                    .HasForeignKey<Equipment>(d => d.HeroId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Equipment_Heros");

                entity.HasOne(d => d.NecklesNavigation)
                    .WithMany(p => p.EquipmentNecklesNavigation)
                    .HasForeignKey(d => d.Neckles)
                    .HasConstraintName("FK_Equipment_Items8");

                entity.HasOne(d => d.Ring1Navigation)
                    .WithMany(p => p.EquipmentRing1Navigation)
                    .HasForeignKey(d => d.Ring1)
                    .HasConstraintName("FK_Equipment_Items6");

                entity.HasOne(d => d.Ring2Navigation)
                    .WithMany(p => p.EquipmentRing2Navigation)
                    .HasForeignKey(d => d.Ring2)
                    .HasConstraintName("FK_Equipment_Items7");

                entity.HasOne(d => d.SecondHandNavigation)
                    .WithMany(p => p.EquipmentSecondHandNavigation)
                    .HasForeignKey(d => d.SecondHand)
                    .HasConstraintName("FK_Equipment_Items1");

                entity.HasOne(d => d.ShoesNavigation)
                    .WithMany(p => p.EquipmentShoesNavigation)
                    .HasForeignKey(d => d.Shoes)
                    .HasConstraintName("FK_Equipment_Items4");

                entity.HasOne(d => d.TrousersNavigation)
                    .WithMany(p => p.EquipmentTrousersNavigation)
                    .HasForeignKey(d => d.Trousers)
                    .HasConstraintName("FK_Equipment_Items3");
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

                entity.Property(e => e.Slbase).HasColumnName("SLbase");
            });

            modelBuilder.Entity<HerosLocations>(entity =>
            {
                entity.HasKey(e => e.LocationId);

                entity.HasIndex(e => e.HeroId)
                    .HasName("IX_HerosLocations");

                entity.HasIndex(e => e.LocationIdentifier)
                    .HasName("IX_HerosLocations_1");

                entity.HasIndex(e => new { e.HeroId, e.LocationIdentifier })
                    .HasName("IX_HerosLocations_2")
                    .IsUnique();

                entity.Property(e => e.LocationId)
                    .HasColumnName("LocationID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.HeroId).HasColumnName("HeroID");

                entity.HasOne(d => d.Hero)
                    .WithMany(p => p.HerosLocations)
                    .HasForeignKey(d => d.HeroId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HerosLocations_Heros");

                entity.HasOne(d => d.LocationIdentifierNavigation)
                    .WithMany(p => p.HerosLocations)
                    .HasForeignKey(d => d.LocationIdentifier)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HerosLocations_LocationsDB");
            });

            modelBuilder.Entity<Items>(entity =>
            {
                entity.HasKey(e => e.ItemId);

                entity.Property(e => e.ItemId)
                    .HasColumnName("ItemID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Modifier).IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<LocationsDb>(entity =>
            {
                entity.HasKey(e => e.LocationIdentifier);

                entity.ToTable("LocationsDB");

                entity.HasIndex(e => e.LocationIdentifier)
                    .HasName("IX_LocationsDB");

                entity.Property(e => e.LocationIdentifier).ValueGeneratedNever();

                entity.Property(e => e.Sketch)
                    .IsRequired()
                    .IsUnicode(false);
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

            modelBuilder.Entity<Traveling>(entity =>
            {
                entity.HasKey(e => e.HeroId);

                entity.HasIndex(e => e.EndTime)
                    .HasName("IX_Traveling");

                entity.Property(e => e.HeroId)
                    .HasColumnName("HeroID")
                    .ValueGeneratedNever();

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.ReverseTime).HasColumnType("datetime");

                entity.Property(e => e.StartName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.Property(e => e.TargetName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Hero)
                    .WithOne(p => p.Traveling)
                    .HasForeignKey<Traveling>(d => d.HeroId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Traveling_Heros");
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
