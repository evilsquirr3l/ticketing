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

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

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

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<Guid>("OfferId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("CartId");

                    b.HasIndex("OfferId");

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

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

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

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Manifest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Map")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Manifests");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Offer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("OfferType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("PaymentId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("PriceId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("SeatId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("SectionId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.HasIndex("PaymentId")
                        .IsUnique();

                    b.HasIndex("PriceId");

                    b.HasIndex("SeatId");

                    b.HasIndex("SectionId");

                    b.ToTable("Offers", t =>
                        {
                            t.HasCheckConstraint("offer_section_seat_check", "(\"SectionId\" IS NOT NULL AND \"SeatId\" IS NULL) OR (\"SectionId\" IS NULL AND \"SeatId\" IS NOT NULL)");
                        });
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Payment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("PaymentDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Price", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("Prices");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Row", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

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

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsReserved")
                        .HasColumnType("boolean");

                    b.Property<Guid>("RowId")
                        .HasColumnType("uuid");

                    b.Property<string>("SeatNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RowId");

                    b.ToTable("Seats");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Section", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<Guid>("ManifestId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ManifestId");

                    b.ToTable("Sections");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Venue", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ManifestId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("EventId")
                        .IsUnique();

                    b.HasIndex("ManifestId")
                        .IsUnique();

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

                    b.HasOne("Ticketing.Data.Entities.Offer", "Offer")
                        .WithMany("CartItems")
                        .HasForeignKey("OfferId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cart");

                    b.Navigation("Offer");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Offer", b =>
                {
                    b.HasOne("Ticketing.Data.Entities.Event", "Event")
                        .WithMany("Offers")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Ticketing.Data.Entities.Payment", "Payment")
                        .WithOne("Offer")
                        .HasForeignKey("Ticketing.Data.Entities.Offer", "PaymentId");

                    b.HasOne("Ticketing.Data.Entities.Price", "Price")
                        .WithMany("Offers")
                        .HasForeignKey("PriceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Ticketing.Data.Entities.Seat", "Seat")
                        .WithMany("Offers")
                        .HasForeignKey("SeatId");

                    b.HasOne("Ticketing.Data.Entities.Section", "Section")
                        .WithMany("Offers")
                        .HasForeignKey("SectionId");

                    b.Navigation("Event");

                    b.Navigation("Payment");

                    b.Navigation("Price");

                    b.Navigation("Seat");

                    b.Navigation("Section");
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

            modelBuilder.Entity("Ticketing.Data.Entities.Section", b =>
                {
                    b.HasOne("Ticketing.Data.Entities.Manifest", "Manifest")
                        .WithMany("Sections")
                        .HasForeignKey("ManifestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Manifest");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Venue", b =>
                {
                    b.HasOne("Ticketing.Data.Entities.Event", "Event")
                        .WithOne("Venue")
                        .HasForeignKey("Ticketing.Data.Entities.Venue", "EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Ticketing.Data.Entities.Manifest", "Manifest")
                        .WithOne("Venue")
                        .HasForeignKey("Ticketing.Data.Entities.Venue", "ManifestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("Manifest");
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
                    b.Navigation("Offers");

                    b.Navigation("Venue")
                        .IsRequired();
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Manifest", b =>
                {
                    b.Navigation("Sections");

                    b.Navigation("Venue")
                        .IsRequired();
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Offer", b =>
                {
                    b.Navigation("CartItems");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Payment", b =>
                {
                    b.Navigation("Offer")
                        .IsRequired();
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Price", b =>
                {
                    b.Navigation("Offers");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Row", b =>
                {
                    b.Navigation("Seats");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Seat", b =>
                {
                    b.Navigation("Offers");
                });

            modelBuilder.Entity("Ticketing.Data.Entities.Section", b =>
                {
                    b.Navigation("Offers");

                    b.Navigation("Rows");
                });
#pragma warning restore 612, 618
        }
    }
}
