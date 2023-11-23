﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Ticketing.Data;

#nullable disable

namespace Ticketing.Migrations
{
    [DbContext(typeof(TicketingDbContext))]
    partial class TicketingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Ticketing.Data.Entities.Cart", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("Carts");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.CartItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CartId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SeatPriceId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("CartId");

                    b.HasIndex("SeatPriceId");

                    b.ToTable("CartItems");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Customer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Event", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Payment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<Guid>("CartId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("PaymentDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("CartId");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Price", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.ToTable("Prices");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Row", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("SectionId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("SectionId");

                    b.ToTable("Rows");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Seat", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("RowId")
                        .HasColumnType("uuid");

                    b.Property<string>("SeatNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RowId");

                    b.ToTable("Seats");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.SeatPrice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("PriceId")
                        .HasColumnType("uuid");

                    b.Property<string>("PriceType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("SeatId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PriceId");

                    b.HasIndex("SeatId");

                    b.ToTable("SeatPrices");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Section", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("VenueId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("VenueId");

                    b.ToTable("Sections");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Venue", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uuid");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.ToTable("Venues");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Cart", b =>
                {
                    b.HasOne("Ticketing.Data.Entities.Customer", "Customer")
                        .WithMany("Carts")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.CartItem", b =>
                {
                    b.HasOne("Ticketing.Data.Entities.Cart", "Cart")
                        .WithMany("CartItems")
                        .HasForeignKey("CartId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Ticketing.Data.Entities.SeatPrice", "SeatPrice")
                        .WithMany("CartItems")
                        .HasForeignKey("SeatPriceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cart");

                    b.Navigation("SeatPrice");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Payment", b =>
                {
                    b.HasOne("Ticketing.Data.Entities.Cart", "Cart")
                        .WithMany()
                        .HasForeignKey("CartId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cart");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Row", b =>
                {
                    b.HasOne("Ticketing.Data.Entities.Section", "Section")
                        .WithMany("Rows")
                        .HasForeignKey("SectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Section");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Seat", b =>
                {
                    b.HasOne("Ticketing.Data.Entities.Row", "Row")
                        .WithMany("Seats")
                        .HasForeignKey("RowId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Row");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.SeatPrice", b =>
                {
                    b.HasOne("Ticketing.Data.Entities.Price", "Price")
                        .WithMany("SeatPrices")
                        .HasForeignKey("PriceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Ticketing.Data.Entities.Seat", "Seat")
                        .WithMany("SeatPrices")
                        .HasForeignKey("SeatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Price");

                    b.Navigation("Seat");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Section", b =>
                {
                    b.HasOne("Ticketing.Data.Entities.Venue", "Venue")
                        .WithMany("Sections")
                        .HasForeignKey("VenueId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Venue");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Venue", b =>
                {
                    b.HasOne("Ticketing.Data.Entities.Event", "Event")
                        .WithMany("Venues")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Cart", b =>
                {
                    b.Navigation("CartItems");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Customer", b =>
                {
                    b.Navigation("Carts");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Event", b =>
                {
                    b.Navigation("Venues");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Price", b =>
                {
                    b.Navigation("SeatPrices");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Row", b =>
                {
                    b.Navigation("Seats");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Seat", b =>
                {
                    b.Navigation("SeatPrices");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.SeatPrice", b =>
                {
                    b.Navigation("CartItems");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Section", b =>
                {
                    b.Navigation("Rows");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Venue", b =>
                {
                    b.Navigation("Sections");
                });
#pragma warning restore 612, 618
        }
    }
}
