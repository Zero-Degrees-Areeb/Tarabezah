using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using Tarabezah.Domain.Common;
using Tarabezah.Domain.Entities;
using Tarabezah.Domain.Enums;

namespace Tarabezah.Data.Context;

/// <summary>
/// DbContext implementation for Tarabezah
/// </summary>
public class TarabezahDbContext : DbContext
{
    public TarabezahDbContext(DbContextOptions<TarabezahDbContext> options)
        : base(options)
    {
    }

    public DbSet<Restaurant> Restaurants { get; set; } = null!;
    public DbSet<Floorplan> Floorplans { get; set; } = null!;
    public DbSet<Element> Elements { get; set; } = null!;
    public DbSet<FloorplanElementInstance> FloorplanElements { get; set; } = null!;
    public DbSet<Shift> Shifts { get; set; } = null!;
    public DbSet<RestaurantShift> RestaurantShifts { get; set; } = null!;
    public DbSet<CombinedTable> CombinedTables { get; set; } = null!;
    public DbSet<CombinedTableMember> CombinedTableMembers => Set<CombinedTableMember>();
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;
    public DbSet<BlackList> BlackLists { get; set; } = null!;
    public DbSet<BlockTable> BlockTables { get; set; } = null!;

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedDate = DateTime.UtcNow; // Always store in UTC
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedDate = DateTime.UtcNow; // Always store in UTC
                }
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Common configuration for all BaseEntity entities - apply directly to each entity
        // Instead of using the problematic ApplyConfigurationToAllEntities method
        ConfigureBaseEntity<Restaurant>(modelBuilder);
        ConfigureBaseEntity<Floorplan>(modelBuilder);
        ConfigureBaseEntity<Element>(modelBuilder);
        ConfigureBaseEntity<FloorplanElementInstance>(modelBuilder);
        ConfigureBaseEntity<Shift>(modelBuilder);
        ConfigureBaseEntity<RestaurantShift>(modelBuilder);
        ConfigureBaseEntity<CombinedTable>(modelBuilder);
        ConfigureBaseEntity<CombinedTableMember>(modelBuilder);
        ConfigureBaseEntity<Client>(modelBuilder);
        ConfigureBaseEntity<Reservation>(modelBuilder);
        ConfigureBaseEntity<BlackList>(modelBuilder);
        ConfigureBaseEntity<BlockTable>(modelBuilder);

        // Restaurant configuration
        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.ToTable("Restaurants");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        // Floorplan configuration
        modelBuilder.Entity<Floorplan>(entity =>
        {
            entity.ToTable("Floorplans");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Restaurant)
                  .WithMany(r => r.Floorplans)
                  .HasForeignKey(e => e.RestaurantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Element configuration
        modelBuilder.Entity<Element>(entity =>
        {
            entity.ToTable("Elements");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);

            entity.Property(e => e.TableType)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(e => e.Purpose)
                  .HasConversion<string>()
                  .HasMaxLength(20);
        });

        // FloorplanElementInstance configuration
        modelBuilder.Entity<FloorplanElementInstance>(entity =>
        {
            entity.ToTable("FloorplanElements");
            entity.Property(e => e.TableId).HasMaxLength(50);

            entity.HasOne(e => e.Floorplan)
                  .WithMany(f => f.Elements)
                  .HasForeignKey(e => e.FloorplanId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Element)
                  .WithMany(e => e.UsedIn)
                  .HasForeignKey(e => e.ElementId)
                  .OnDelete(DeleteBehavior.Restrict); // Don't delete element templates when instances are deleted

            // Remove any existing index on FloorplanId and TableId
            entity.HasIndex(e => e.FloorplanId);
        });

        // Shift configuration
        modelBuilder.Entity<Shift>(entity =>
        {
            entity.ToTable("Shifts");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        });

        // RestaurantShift configuration
        modelBuilder.Entity<RestaurantShift>(entity =>
        {
            entity.ToTable("RestaurantShifts");

            // Configure relationships
            entity.HasOne(rs => rs.Restaurant)
                  .WithMany(r => r.RestaurantShifts)
                  .HasForeignKey(rs => rs.RestaurantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rs => rs.Shift)
                  .WithMany(s => s.RestaurantShifts)
                  .HasForeignKey(rs => rs.ShiftId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Create a unique constraint for Restaurant-Shift combinations
            entity.HasIndex(rs => new { rs.RestaurantId, rs.ShiftId }).IsUnique();
        });

        // BlockingClient configuration
        modelBuilder.Entity<BlackList>(entity =>
        {
            entity.ToTable("BlackList");
            entity.Property(e => e.Reason).HasMaxLength(500);

            // Configure relationships
            entity.HasOne(bc => bc.Client)
                  .WithMany(c => c.BlockedByRestaurants)
                  .HasForeignKey(bc => bc.ClientId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(bc => bc.Restaurant)
                  .WithMany(r => r.BlockedClients)
                  .HasForeignKey(bc => bc.RestaurantId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Create a unique constraint for Client-Restaurant combinations
            entity.HasIndex(bc => new { bc.ClientId, bc.RestaurantId }).IsUnique();
        });

        // CombinedTable configuration
        modelBuilder.Entity<CombinedTable>(entity =>
        {
            entity.ToTable("CombinedTables");
            entity.Property(e => e.GroupName).HasMaxLength(100);

            // Configure relationship with Floorplan
            entity.HasOne(e => e.Floorplan)
                  .WithMany(f => f.CombinedTables)
                  .HasForeignKey(e => e.FloorplanId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // CombinedTableMember configuration
        modelBuilder.Entity<CombinedTableMember>(entity =>
        {
            entity.ToTable("CombinedTableMembers");

            // Configure relationship with CombinedTable
            entity.HasOne(e => e.CombinedTable)
                  .WithMany(ct => ct.Members)
                  .HasForeignKey(e => e.CombinedTableId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship with FloorplanElementInstance
            entity.HasOne(e => e.FloorplanElementInstance)
                  .WithMany(fei => fei.CombinedTableMemberships)
                  .HasForeignKey(e => e.FloorplanElementInstanceId)
                  .OnDelete(DeleteBehavior.Restrict); // Don't delete instances when removed from combinations

            // Configure relationship with Reservations
            entity.HasMany(e => e.Reservations)
                  .WithOne(r => r.CombinedTableMember)
                  .HasForeignKey(r => r.CombinedTableMemberId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Client configuration
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);

            // Convert ClientSource enum to string
            entity.Property(e => e.Source)
                  .HasConversion<string>()
                  .HasMaxLength(50);

            entity.Property(e => e.Notes).HasMaxLength(500);

            // Storing tags as a comma-separated string with value comparer
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .HasMaxLength(500);

            // Add a value comparer for the Tags property
            var valueComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );

            entity.Property(e => e.Tags).Metadata.SetValueComparer(valueComparer);
        });

        // Reservation configuration
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("Reservations");
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Time).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);

            // Configure relationships
            entity.HasOne(r => r.Client)
                  .WithMany(c => c.Reservations)
                  .HasForeignKey(r => r.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Shift)
                  .WithMany(s => s.Reservations)
                  .HasForeignKey(r => r.ShiftId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Storing tags as a comma-separated string
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .HasMaxLength(500);

            // Add a value comparer for the Tags property
            var valueComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );

            entity.Property(e => e.Tags).Metadata.SetValueComparer(valueComparer);

            // Create an index for efficient querying by date
            entity.HasIndex(r => r.Date);
        });

        // BlockTable configuration
        modelBuilder.Entity<BlockTable>(entity =>
        {
            entity.ToTable("BlockTables");
            entity.Property(e => e.Notes).HasMaxLength(500);

            // Configure relationship with FloorplanElementInstance
            entity.HasOne(e => e.FloorplanElementInstance)
                  .WithMany()  // No navigation property on FloorplanElementInstance side
                  .HasForeignKey(e => e.FloorplanElementInstanceId)
                  .OnDelete(DeleteBehavior.Cascade);  // If a table is deleted, just set the reference to null
        });

        // Seed data
        SeedElements(modelBuilder);
        SeedRestaurants(modelBuilder);
        SeedFloorplans(modelBuilder);
        SeedFloorplanElements(modelBuilder);
        SeedShifts(modelBuilder);
        SeedRestaurantShifts(modelBuilder);
        SeedClients(modelBuilder);
        SeedReservations(modelBuilder);
    }

    // New helper method to configure base entities
    private void ConfigureBaseEntity<T>(ModelBuilder modelBuilder) where T : BaseEntity
    {
        modelBuilder.Entity<T>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Guid).IsRequired();
            builder.HasIndex(e => e.Guid).IsUnique();
            builder.Property(e => e.CreatedDate).IsRequired();
            builder.Property(e => e.ModifiedDate).IsRequired();
        });
    }

    private void SeedElements(ModelBuilder modelBuilder)
    {
        // Use fixed dates for seeding to avoid model changes
        var createdDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Element>().HasData(
            new Element
            {
                Id = 1,
                Guid = Guid.Parse("f1e32900-5e22-4824-8adb-e1c50e976a23"),
                Name = "Round Table",
                ImageUrl = "/images/elements/round-table.png",
                TableType = TableType.Round,
                Purpose = ElementPurpose.Reservable,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Element
            {
                Id = 2,
                Guid = Guid.Parse("28be68e2-a4d1-4a33-b0d6-f2d603fa0b41"),
                Name = "Square Table",
                ImageUrl = "/images/elements/square-table.png",
                TableType = TableType.Square,
                Purpose = ElementPurpose.Reservable,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Element
            {
                Id = 3,
                Guid = Guid.Parse("a45c7f08-8b04-4d3e-8d23-f9e274a7c546"),
                Name = "Chair",
                ImageUrl = "/images/elements/chair.png",
                TableType = TableType.Custom,
                Purpose = ElementPurpose.Decorative,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Element
            {
                Id = 4,
                Guid = Guid.Parse("e28b7af4-6f77-4a42-b4f6-29e7fd6ad0a9"),
                Name = "Wall",
                ImageUrl = "/images/elements/wall.png",
                TableType = TableType.Custom,
                Purpose = ElementPurpose.Decorative,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            }
        );
    }

    private void SeedRestaurants(ModelBuilder modelBuilder)
    {
        // Use fixed dates for seeding
        var createdDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Restaurant>().HasData(
            new Restaurant
            {
                Id = 1,
                Guid = Guid.Parse("a7fa1095-d8c5-4d00-8a44-7ba684eae835"),
                Name = "Italian Bistro",
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Restaurant
            {
                Id = 2,
                Guid = Guid.Parse("b2e7c6f0-d98c-4e5d-9a83-bc9429ab4187"),
                Name = "Seaside Grill",
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Restaurant
            {
                Id = 3,
                Guid = Guid.Parse("c3d8e9f2-a04b-4c1e-8a75-1d0e7f35b281"),
                Name = "Downtown Cafe",
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            }
        );
    }

    private void SeedFloorplans(ModelBuilder modelBuilder)
    {
        // Use fixed dates for seeding
        var createdDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Floorplan>().HasData(
            new Floorplan
            {
                Id = 1,
                Guid = Guid.Parse("d4f9a1b2-c03e-4f5a-8b67-9a0e7f2c3d45"),
                Name = "Main Floor",
                RestaurantId = 1,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Floorplan
            {
                Id = 2,
                Guid = Guid.Parse("e5b0c1d2-a03f-4e5b-9c78-0b1f2d3e4a56"),
                Name = "Patio",
                RestaurantId = 1,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Floorplan
            {
                Id = 3,
                Guid = Guid.Parse("f6c1d2e3-b04f-5e6c-0d89-1c2f3e4d5a67"),
                Name = "Dining Room",
                RestaurantId = 2,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Floorplan
            {
                Id = 4,
                Guid = Guid.Parse("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"),
                Name = "Bar Area",
                RestaurantId = 3,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            }
        );
    }

    private void SeedFloorplanElements(ModelBuilder modelBuilder)
    {
        // Use fixed dates for seeding
        var createdDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<FloorplanElementInstance>().HasData(
            // Tables for Main Floor (Floorplan 1)
            new FloorplanElementInstance
            {
                Id = 1,
                Guid = Guid.Parse("b7c8d9e0-f1a2-3b4c-5d6e-7f8a9b0c1d2e"),
                TableId = "T1",
                FloorplanId = 1,
                ElementId = 1, // Round Table
                MinCapacity = 2,
                MaxCapacity = 4,
                X = 100,
                Y = 150,
                Rotation = 0,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new FloorplanElementInstance
            {
                Id = 2,
                Guid = Guid.Parse("c8d9e0f1-a2b3-4c5d-6e7f-8a9b0c1d2e3f"),
                TableId = "T2",
                FloorplanId = 1,
                ElementId = 2, // Square Table
                MinCapacity = 4,
                MaxCapacity = 6,
                X = 250,
                Y = 150,
                Rotation = 0,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            // Decorative elements for Main Floor
            new FloorplanElementInstance
            {
                Id = 3,
                Guid = Guid.Parse("d9e0f1a2-b3c4-5d6e-7f8a-9b0c1d2e3f4a"),
                TableId = "W1",
                FloorplanId = 1,
                ElementId = 4, // Wall
                MinCapacity = 0,
                MaxCapacity = 0,
                X = 50,
                Y = 50,
                Rotation = 90,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },

            // Tables for Patio (Floorplan 2)
            new FloorplanElementInstance
            {
                Id = 4,
                Guid = Guid.Parse("e0f1a2b3-c4d5-6e7f-8a9b-0c1d2e3f4a5b"),
                TableId = "P1",
                FloorplanId = 2,
                ElementId = 1, // Round Table
                MinCapacity = 2,
                MaxCapacity = 4,
                X = 100,
                Y = 100,
                Rotation = 0,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },

            // Tables for Dining Room (Floorplan 3)
            new FloorplanElementInstance
            {
                Id = 5,
                Guid = Guid.Parse("f1a2b3c4-d5e6-7f8a-9b0c-1d2e3f4a5b6c"),
                TableId = "D1",
                FloorplanId = 3,
                ElementId = 2, // Square Table
                MinCapacity = 4,
                MaxCapacity = 8,
                X = 150,
                Y = 200,
                Rotation = 45,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },

            // Tables for Bar Area (Floorplan 4)
            new FloorplanElementInstance
            {
                Id = 6,
                Guid = Guid.Parse("a2b3c4d5-e6f7-8a9b-0c1d-2e3f4a5b6c7d"),
                TableId = "B1",
                FloorplanId = 4,
                ElementId = 1, // Round Table
                MinCapacity = 2,
                MaxCapacity = 2,
                X = 75,
                Y = 125,
                Rotation = 0,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            }
        );
    }

    private void SeedShifts(ModelBuilder modelBuilder)
    {
        // Use fixed dates for seeding
        var createdDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Shift>().HasData(
            new Shift
            {
                Id = 1,
                Guid = Guid.Parse("d1e23f45-6789-4a0b-b1c2-d3e4f5a6b7c8"),
                Name = "Breakfast",
                StartTime = new TimeSpan(7, 0, 0),  // 7:00 AM
                EndTime = new TimeSpan(11, 0, 0),   // 11:00 AM
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Shift
            {
                Id = 2,
                Guid = Guid.Parse("e2f34a56-789b-4c0d-e1f2-a3b4c5d6e7f8"),
                Name = "Lunch",
                StartTime = new TimeSpan(11, 30, 0), // 11:30 AM
                EndTime = new TimeSpan(15, 0, 0),    // 3:00 PM
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Shift
            {
                Id = 3,
                Guid = Guid.Parse("f3a45b67-8c9d-4e0f-a1b2-c3d4e5f6a7b8"),
                Name = "Dinner",
                StartTime = new TimeSpan(17, 0, 0), // 5:00 PM
                EndTime = new TimeSpan(23, 0, 0),   // 11:00 PM
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            }
        );
    }

    private void SeedRestaurantShifts(ModelBuilder modelBuilder)
    {
        // Use fixed dates for seeding
        var createdDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<RestaurantShift>().HasData(
            // Restaurant 1 (Italian Bistro) shifts
            new RestaurantShift
            {
                Id = 1,
                Guid = Guid.Parse("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"),
                RestaurantId = 1,
                ShiftId = 1, // Breakfast
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new RestaurantShift
            {
                Id = 2,
                Guid = Guid.Parse("b2c3d4e5-f6a7-8b9c-0d1e-2f3a4b5c6d7e"),
                RestaurantId = 1,
                ShiftId = 2, // Lunch
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new RestaurantShift
            {
                Id = 3,
                Guid = Guid.Parse("c3d4e5f6-a7b8-9c0d-1e2f-3a4b5c6d7e8f"),
                RestaurantId = 1,
                ShiftId = 3, // Dinner
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },

            // Restaurant 2 (Seaside Grill) shifts
            new RestaurantShift
            {
                Id = 4,
                Guid = Guid.Parse("d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f9a"),
                RestaurantId = 2,
                ShiftId = 2, // Lunch
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new RestaurantShift
            {
                Id = 5,
                Guid = Guid.Parse("e5f6a7b8-c9d0-1e2f-3a4b-5c6d7e8f9a0b"),
                RestaurantId = 2,
                ShiftId = 3, // Dinner
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            }
        );
    }

    private void SeedClients(ModelBuilder modelBuilder)
    {
        // Use fixed dates for seeding
        var createdDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Client>().HasData(
            new Client
            {
                Id = 1,
                Guid = Guid.Parse("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"),
                Name = "John Smith",
                PhoneNumber = "+1-555-123-4567",
                Email = "john.smith@example.com",
                Birthday = new DateTime(1985, 6, 15),
                Source = ClientSource.Website,
                Tags = new List<string> { "vip", "wine lover" },
                Notes = "Prefers window seating",
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Client
            {
                Id = 2,
                Guid = Guid.Parse("b2c3d4e5-f6a7-8b9c-0d1e-2f3a4b5c6d7e"),
                Name = "Sara Johnson",
                PhoneNumber = "+1-555-987-6543",
                Email = "sara.johnson@example.com",
                Birthday = new DateTime(1990, 3, 25),
                Source = ClientSource.Instagram,
                Tags = new List<string> { "vegetarian", "birthday" },
                Notes = "Allergic to nuts",
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Client
            {
                Id = 3,
                Guid = Guid.Parse("c3d4e5f6-a7b8-9c0d-1e2f-3a4b5c6d7e8f"),
                Name = "Michael Chen",
                PhoneNumber = "+1-555-456-7890",
                Email = "michael.chen@example.com",
                Birthday = new DateTime(1978, 9, 12),
                Source = ClientSource.Facebook,
                Tags = new List<string> { "bbq lover", "regular" },
                Notes = "Celebrates anniversary on September 22",
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Client
            {
                Id = 4,
                Guid = Guid.Parse("d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f9a"),
                Name = "Jennifer Garcia",
                PhoneNumber = "+1-555-789-0123",
                Email = "jennifer.garcia@example.com",
                Birthday = new DateTime(1992, 12, 8),
                Source = ClientSource.Referral,
                Tags = new List<string> { "pescatarian", "quiet table" },
                Notes = "Prefers quiet corner tables",
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Client
            {
                Id = 5,
                Guid = Guid.Parse("e5f6a7b8-c9d0-1e2f-3a4b-5c6d7e8f9a0b"),
                Name = "Robert Williams",
                PhoneNumber = "+1-555-234-5678",
                Email = "robert.williams@example.com",
                Birthday = null,
                Source = ClientSource.WalkIn,
                Tags = new List<string> { "business", "wine lover" },
                Notes = "Frequently books for business meetings",
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            }
        );
    }

    private void SeedReservations(ModelBuilder modelBuilder)
    {
        // Use fixed dates for seeding
        var createdDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var modifiedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Create future reservation dates (relative to seed date)
        var tomorrow = new DateTime(2023, 1, 2);
        var nextWeek = new DateTime(2023, 1, 8);
        var twoWeeksLater = new DateTime(2023, 1, 15);

        modelBuilder.Entity<Reservation>().HasData(
            new Reservation
            {
                Id = 1,
                Guid = Guid.Parse("f6a7b8c9-d0e1-2f3a-4b5c-6d7e8f9a0b1c"),
                ClientId = 1, // John Smith
                ShiftId = 3,  // Dinner
                Date = tomorrow,
                Time = new TimeSpan(19, 0, 0), // 7:00 PM
                PartySize = 2,
                Tags = new List<string> { "anniversary", "window seat" },
                Notes = "Celebrating wedding anniversary",
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Reservation
            {
                Id = 2,
                Guid = Guid.Parse("a7b8c9d0-e1f2-3a4b-5c6d-7e8f9a0b1c2d"),
                ClientId = 2, // Sara Johnson
                ShiftId = 2,  // Lunch
                Date = nextWeek,
                Time = new TimeSpan(12, 30, 0), // 12:30 PM
                PartySize = 4,
                Tags = new List<string> { "birthday", "cake" },
                Notes = "Birthday celebration - bringing own cake",
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Reservation
            {
                Id = 3,
                Guid = Guid.Parse("b8c9d0e1-f2a3-4b5c-6d7e-8f9a0b1c2d3e"),
                ClientId = 3, // Michael Chen
                ShiftId = 3,  // Dinner
                Date = tomorrow,
                Time = new TimeSpan(20, 0, 0), // 8:00 PM
                PartySize = 6,
                Tags = new List<string> { "family", "bbq" },
                Notes = "Family gathering, requests BBQ specials",
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Reservation
            {
                Id = 4,
                Guid = Guid.Parse("c9d0e1f2-a3b4-5c6d-7e8f-9a0b1c2d3e4f"),
                ClientId = 4, // Jennifer Garcia
                ShiftId = 2,  // Lunch
                Date = twoWeeksLater,
                Time = new TimeSpan(13, 0, 0), // 1:00 PM
                PartySize = 2,
                Tags = new List<string> { "quiet corner", "pescatarian" },
                Notes = "Pescatarian menu options requested",
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Reservation
            {
                Id = 5,
                Guid = Guid.Parse("d0e1f2a3-b4c5-6d7e-8f9a-0b1c2d3e4f5a"),
                ClientId = 5, // Robert Williams
                ShiftId = 3,  // Dinner
                Date = nextWeek,
                Time = new TimeSpan(18, 30, 0), // 6:30 PM
                PartySize = 8,
                Tags = new List<string> { "business", "wine pairing" },
                Notes = "Business dinner, wine pairing recommended",
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            },
            new Reservation
            {
                Id = 6,
                Guid = Guid.Parse("e1f2a3b4-c5d6-7e8f-9a0b-1c2d3e4f5a6b"),
                ClientId = 1, // John Smith
                ShiftId = 2,  // Lunch
                Date = twoWeeksLater,
                Time = new TimeSpan(12, 0, 0), // 12:00 PM
                PartySize = 4,
                Tags = new List<string> { "business", "quick service" },
                Notes = "Business lunch, time-constrained",
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            }
        );
    }
}

// Extension method to apply configuration to all entities of a specific type
public static class ModelBuilderExtensions
{
    public static void ApplyConfigurationToAllEntities<TEntity>(
        this ModelBuilder modelBuilder,
        Action<EntityTypeBuilder<TEntity>> configuration) where TEntity : class
    {
        if (modelBuilder == null || configuration == null)
        {
            return;
        }

        // Get all entity types that inherit from TEntity
        var entities = modelBuilder.Model.GetEntityTypes()
            .Where(t => t != null && t.ClrType != null && t.ClrType.IsAssignableTo(typeof(TEntity)) && t.ClrType != typeof(TEntity))
            .Select(t => t.ClrType)
            .ToList();

        foreach (var entity in entities)
        {
            if (entity == null)
            {
                continue;
            }

            try
            {
                // Use reflection to call the Entity<T> method and apply configuration
                var entityMethodInfo = typeof(ModelBuilder).GetMethod("Entity", Type.EmptyTypes)!
                    .MakeGenericMethod(entity);
                var entityBuilder = entityMethodInfo.Invoke(modelBuilder, null);

                if (entityBuilder != null)
                {
                    configuration(entityBuilder as EntityTypeBuilder<TEntity>);
                }
            }
            catch (Exception ex)
            {
                // Log the exception but continue with other entities
                Console.WriteLine($"Error configuring entity {entity.Name}: {ex.Message}");
            }
        }
    }
}