﻿// <auto-generated />
using System;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DataAccess.Migrations
{
    [DbContext(typeof(DatabaseConnection))]
    [Migration("20180929215702_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.3-rtm-32065")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ObligatorioDA2.DataAccess.Entities.CommentEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("MatchEntityId");

                    b.Property<string>("Text");

                    b.HasKey("Id");

                    b.HasIndex("MatchEntityId");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("ObligatorioDA2.DataAccess.Entities.MatchEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AwayTeamName");

                    b.Property<string>("AwayTeamSportEntityName");

                    b.Property<DateTime>("Date");

                    b.Property<string>("HomeTeamName");

                    b.Property<string>("HomeTeamSportEntityName");

                    b.HasKey("Id");

                    b.HasIndex("AwayTeamSportEntityName", "AwayTeamName");

                    b.HasIndex("HomeTeamSportEntityName", "HomeTeamName");

                    b.ToTable("Matches");
                });

            modelBuilder.Entity("ObligatorioDA2.DataAccess.Entities.SportEntity", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Id");

                    b.HasKey("Name");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("Sports");
                });

            modelBuilder.Entity("ObligatorioDA2.DataAccess.Entities.TeamEntity", b =>
                {
                    b.Property<string>("SportEntityName");

                    b.Property<string>("Name");

                    b.Property<int>("Identity")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Photo");

                    b.HasKey("SportEntityName", "Name");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("ObligatorioDA2.DataAccess.Entities.UserEntity", b =>
                {
                    b.Property<string>("UserName")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<int>("Id");

                    b.Property<bool>("IsAdmin");

                    b.Property<string>("Name");

                    b.Property<string>("Password");

                    b.Property<string>("Surname");

                    b.HasKey("UserName");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ObligatorioDA2.DataAccess.Entities.CommentEntity", b =>
                {
                    b.HasOne("ObligatorioDA2.DataAccess.Entities.MatchEntity")
                        .WithMany("Commentaries")
                        .HasForeignKey("MatchEntityId");
                });

            modelBuilder.Entity("ObligatorioDA2.DataAccess.Entities.MatchEntity", b =>
                {
                    b.HasOne("ObligatorioDA2.DataAccess.Entities.TeamEntity", "AwayTeam")
                        .WithMany()
                        .HasForeignKey("AwayTeamSportEntityName", "AwayTeamName");

                    b.HasOne("ObligatorioDA2.DataAccess.Entities.TeamEntity", "HomeTeam")
                        .WithMany()
                        .HasForeignKey("HomeTeamSportEntityName", "HomeTeamName");
                });

            modelBuilder.Entity("ObligatorioDA2.DataAccess.Entities.SportEntity", b =>
                {
                    b.HasOne("ObligatorioDA2.DataAccess.Entities.MatchEntity")
                        .WithOne("SportEntity")
                        .HasForeignKey("ObligatorioDA2.DataAccess.Entities.SportEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ObligatorioDA2.DataAccess.Entities.TeamEntity", b =>
                {
                    b.HasOne("ObligatorioDA2.DataAccess.Entities.SportEntity")
                        .WithMany("Teams")
                        .HasForeignKey("SportEntityName")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ObligatorioDA2.DataAccess.Entities.UserEntity", b =>
                {
                    b.HasOne("ObligatorioDA2.DataAccess.Entities.CommentEntity")
                        .WithOne("Maker")
                        .HasForeignKey("ObligatorioDA2.DataAccess.Entities.UserEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}