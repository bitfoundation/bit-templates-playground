﻿// <auto-generated />
using Bit.TemplatePlayground.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Bit.TemplatePlayground.Server.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("Bit.TemplatePlayground.Server.Models.Categories.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Color")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Categories");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Color = "#FFCD56",
                            Name = "Ford"
                        },
                        new
                        {
                            Id = 2,
                            Color = "#FF6384",
                            Name = "Nissan"
                        },
                        new
                        {
                            Id = 3,
                            Color = "#4BC0C0",
                            Name = "Benz"
                        },
                        new
                        {
                            Id = 4,
                            Color = "#FF9124",
                            Name = "BMW"
                        },
                        new
                        {
                            Id = 5,
                            Color = "#2B88D8",
                            Name = "Tesla"
                        });
                });

            modelBuilder.Entity("Bit.TemplatePlayground.Server.Models.Identity.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("Roles", (string)null);
                });

            modelBuilder.Entity("Bit.TemplatePlayground.Server.Models.Identity.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("BirthDate")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<long?>("ConfirmationEmailRequestedOn")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FullName")
                        .HasColumnType("TEXT");

                    b.Property<int?>("Gender")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("LockoutEnd")
                        .HasColumnType("INTEGER");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("TEXT");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ProfileImageName")
                        .HasColumnType("TEXT");

                    b.Property<long?>("ResetPasswordEmailRequestedOn")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("TEXT");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("Users", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            AccessFailedCount = 0,
                            BirthDate = 1306790461440000060L,
                            ConcurrencyStamp = "315e1a26-5b3a-4544-8e91-2760cd28e231",
                            Email = "test@bitplatform.dev",
                            EmailConfirmed = true,
                            FullName = "Bit.TemplatePlayground test account",
                            Gender = 2,
                            LockoutEnabled = true,
                            NormalizedEmail = "TEST@BITPLATFORM.DEV",
                            NormalizedUserName = "TEST@BITPLATFORM.DEV",
                            PasswordHash = "AQAAAAIAAYagAAAAEP0v3wxkdWtMkHA3Pp5/JfS+42/Qto9G05p2mta6dncSK37hPxEHa3PGE4aqN30Aag==",
                            PhoneNumberConfirmed = false,
                            SecurityStamp = "959ff4a9-4b07-4cc1-8141-c5fc033daf83",
                            TwoFactorEnabled = false,
                            UserName = "test@bitplatform.dev"
                        });
                });

            modelBuilder.Entity("Bit.TemplatePlayground.Server.Models.Products.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("CreatedOn")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasMaxLength(512)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Price")
                        .HasColumnType("money");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Products");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CategoryId = 1,
                            CreatedOn = 1307416854528000060L,
                            Description = "The Ford Mustang is ranked #1 in Sports Cars",
                            Name = "Mustang",
                            Price = 27155m
                        },
                        new
                        {
                            Id = 2,
                            CategoryId = 1,
                            CreatedOn = 1307408007168000060L,
                            Description = "The Ford GT is a mid-engine two-seater sports car manufactured and marketed by American automobile manufacturer",
                            Name = "GT",
                            Price = 500000m
                        },
                        new
                        {
                            Id = 3,
                            CategoryId = 1,
                            CreatedOn = 1307390312448000060L,
                            Description = "Ford Ranger is a nameplate that has been used on multiple model lines of pickup trucks sold by Ford worldwide.",
                            Name = "Ranger",
                            Price = 25000m
                        },
                        new
                        {
                            Id = 4,
                            CategoryId = 1,
                            CreatedOn = 1307381465088000060L,
                            Description = "Raptor is a SCORE off-road trophy truck living in a asphalt world",
                            Name = "Raptor",
                            Price = 53205m
                        },
                        new
                        {
                            Id = 5,
                            CategoryId = 1,
                            CreatedOn = 1307372617728000060L,
                            Description = "The Ford Maverick is a compact pickup truck produced by Ford Motor Company.",
                            Name = "Maverick",
                            Price = 22470m
                        },
                        new
                        {
                            Id = 6,
                            CategoryId = 2,
                            CreatedOn = 1307416854528000060L,
                            Description = "A powerful convertible sports car",
                            Name = "Roadster",
                            Price = 42800m
                        },
                        new
                        {
                            Id = 7,
                            CategoryId = 2,
                            CreatedOn = 1307408007168000060L,
                            Description = "A perfectly adequate family sedan with sharp looks",
                            Name = "Altima",
                            Price = 24550m
                        },
                        new
                        {
                            Id = 8,
                            CategoryId = 2,
                            CreatedOn = 1307390312448000060L,
                            Description = "Legendary supercar with AWD, 4 seats, a powerful V6 engine and the latest tech",
                            Name = "GT-R",
                            Price = 113540m
                        },
                        new
                        {
                            Id = 9,
                            CategoryId = 2,
                            CreatedOn = 1307372617728000060L,
                            Description = "A new smart SUV",
                            Name = "Juke",
                            Price = 28100m
                        },
                        new
                        {
                            Id = 10,
                            CategoryId = 3,
                            CreatedOn = 1307416854528000060L,
                            Description = "",
                            Name = "H247",
                            Price = 54950m
                        },
                        new
                        {
                            Id = 11,
                            CategoryId = 3,
                            CreatedOn = 1307408007168000060L,
                            Description = "",
                            Name = "V297",
                            Price = 103360m
                        },
                        new
                        {
                            Id = 12,
                            CategoryId = 3,
                            CreatedOn = 1307372617728000060L,
                            Description = "",
                            Name = "R50",
                            Price = 2000000m
                        },
                        new
                        {
                            Id = 13,
                            CategoryId = 4,
                            CreatedOn = 1307416854528000060L,
                            Description = "",
                            Name = "M550i",
                            Price = 77790m
                        },
                        new
                        {
                            Id = 14,
                            CategoryId = 4,
                            CreatedOn = 1307408007168000060L,
                            Description = "",
                            Name = "540i",
                            Price = 60945m
                        },
                        new
                        {
                            Id = 15,
                            CategoryId = 4,
                            CreatedOn = 1307399159808000060L,
                            Description = "",
                            Name = "530e",
                            Price = 56545m
                        },
                        new
                        {
                            Id = 16,
                            CategoryId = 4,
                            CreatedOn = 1307390312448000060L,
                            Description = "",
                            Name = "530i",
                            Price = 55195m
                        },
                        new
                        {
                            Id = 17,
                            CategoryId = 4,
                            CreatedOn = 1307381465088000060L,
                            Description = "",
                            Name = "M850i",
                            Price = 100045m
                        },
                        new
                        {
                            Id = 18,
                            CategoryId = 4,
                            CreatedOn = 1307372617728000060L,
                            Description = "",
                            Name = "X7",
                            Price = 77980m
                        },
                        new
                        {
                            Id = 19,
                            CategoryId = 4,
                            CreatedOn = 1307363770368000060L,
                            Description = "",
                            Name = "IX",
                            Price = 87000m
                        },
                        new
                        {
                            Id = 20,
                            CategoryId = 5,
                            CreatedOn = 1307416854528000060L,
                            Description = "rapid acceleration and dynamic handling",
                            Name = "Model 3",
                            Price = 61990m
                        },
                        new
                        {
                            Id = 21,
                            CategoryId = 5,
                            CreatedOn = 1307408007168000060L,
                            Description = "finishes near the top of our luxury electric car rankings.",
                            Name = "Model S",
                            Price = 135000m
                        },
                        new
                        {
                            Id = 22,
                            CategoryId = 5,
                            CreatedOn = 1307399159808000060L,
                            Description = "Heart-pumping acceleration, long drive range",
                            Name = "Model X",
                            Price = 138890m
                        },
                        new
                        {
                            Id = 23,
                            CategoryId = 5,
                            CreatedOn = 1307372617728000060L,
                            Description = "extensive driving range, lots of standard safety features",
                            Name = "Model Y",
                            Price = 67790m
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FriendlyName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Xml")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("DataProtectionKeys");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<int>("RoleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("RoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("UserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("UserTokens", (string)null);
                });

            modelBuilder.Entity("Bit.TemplatePlayground.Server.Models.Products.Product", b =>
                {
                    b.HasOne("Bit.TemplatePlayground.Server.Models.Categories.Category", "Category")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.HasOne("Bit.TemplatePlayground.Server.Models.Identity.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.HasOne("Bit.TemplatePlayground.Server.Models.Identity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.HasOne("Bit.TemplatePlayground.Server.Models.Identity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.HasOne("Bit.TemplatePlayground.Server.Models.Identity.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Bit.TemplatePlayground.Server.Models.Identity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.HasOne("Bit.TemplatePlayground.Server.Models.Identity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Bit.TemplatePlayground.Server.Models.Categories.Category", b =>
                {
                    b.Navigation("Products");
                });
#pragma warning restore 612, 618
        }
    }
}