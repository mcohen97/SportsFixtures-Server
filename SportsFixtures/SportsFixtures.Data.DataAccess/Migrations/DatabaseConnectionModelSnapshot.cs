﻿// <auto-generated />
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ObligatorioDA2.Data.DataAccess;

namespace ObligatorioDA2.Data.DataAccess.Migrations
{
    [ExcludeFromCodeCoverage]
    [DbContext(typeof(DatabaseConnection))]
    partial class DatabaseConnectionModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ObligatorioDA2.Data.Entities.CommentEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("EncounterEntityId");

                    b.Property<string>("MakerUserName");

                    b.Property<string>("Text");

                    b.HasKey("Id");

                    b.HasIndex("EncounterEntityId");

                    b.HasIndex("MakerUserName");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("ObligatorioDA2.Data.Entities.EncounterEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Date");

                    b.Property<bool>("HasResult");

                    b.Property<string>("SportEntityName");

                    b.HasKey("Id");

                    b.HasIndex("SportEntityName");

                    b.ToTable("Encounters");
                });

            modelBuilder.Entity("ObligatorioDA2.Data.Entities.EncounterTeam", b =>
                {
                    b.Property<int>("EncounterId");

                    b.Property<int>("TeamNumber");

                    b.Property<int>("Position");

                    b.HasKey("EncounterId", "TeamNumber");

                    b.HasIndex("TeamNumber");

                    b.ToTable("EncounterTeams");
                });

            modelBuilder.Entity("ObligatorioDA2.Data.Entities.SportEntity", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsTwoTeams");

                    b.HasKey("Name");

                    b.ToTable("Sports");
                });

            modelBuilder.Entity("ObligatorioDA2.Data.Entities.TeamEntity", b =>
                {
                    b.Property<int>("TeamNumber")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Photo");

                    b.Property<string>("SportEntityName")
                        .IsRequired();

                    b.HasKey("TeamNumber");

                    b.HasAlternateKey("SportEntityName", "Name");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("ObligatorioDA2.Data.Entities.UserEntity", b =>
                {
                    b.Property<string>("UserName")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<bool>("IsAdmin");

                    b.Property<string>("Name");

                    b.Property<string>("Password");

                    b.Property<string>("Surname");

                    b.HasKey("UserName");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ObligatorioDA2.Data.Entities.UserTeam", b =>
                {
                    b.Property<string>("TeamEntityName");

                    b.Property<string>("TeamEntitySportEntityName");

                    b.Property<string>("UserEntityUserName");

                    b.Property<int?>("TeamNumber");

                    b.HasKey("TeamEntityName", "TeamEntitySportEntityName", "UserEntityUserName");

                    b.HasIndex("TeamNumber");

                    b.HasIndex("UserEntityUserName");

                    b.ToTable("UserTeams");
                });

            modelBuilder.Entity("ObligatorioDA2.Data.Entities.CommentEntity", b =>
                {
                    b.HasOne("ObligatorioDA2.Data.Entities.EncounterEntity")
                        .WithMany("Commentaries")
                        .HasForeignKey("EncounterEntityId");

                    b.HasOne("ObligatorioDA2.Data.Entities.UserEntity", "Maker")
                        .WithMany()
                        .HasForeignKey("MakerUserName");
                });

            modelBuilder.Entity("ObligatorioDA2.Data.Entities.EncounterEntity", b =>
                {
                    b.HasOne("ObligatorioDA2.Data.Entities.SportEntity", "SportEntity")
                        .WithMany()
                        .HasForeignKey("SportEntityName");
                });

            modelBuilder.Entity("ObligatorioDA2.Data.Entities.EncounterTeam", b =>
                {
                    b.HasOne("ObligatorioDA2.Data.Entities.EncounterEntity", "Encounter")
                        .WithMany()
                        .HasForeignKey("EncounterId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ObligatorioDA2.Data.Entities.TeamEntity", "Team")
                        .WithMany()
                        .HasForeignKey("TeamNumber")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ObligatorioDA2.Data.Entities.TeamEntity", b =>
                {
                    b.HasOne("ObligatorioDA2.Data.Entities.SportEntity", "Sport")
                        .WithMany()
                        .HasForeignKey("SportEntityName")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ObligatorioDA2.Data.Entities.UserTeam", b =>
                {
                    b.HasOne("ObligatorioDA2.Data.Entities.TeamEntity", "Team")
                        .WithMany()
                        .HasForeignKey("TeamNumber");

                    b.HasOne("ObligatorioDA2.Data.Entities.UserEntity", "Follower")
                        .WithMany()
                        .HasForeignKey("UserEntityUserName")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
