using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FleetManager.Domain.Models;

namespace FleetManager.Infrastructure;

public partial class FleetContext : DbContext
{
    public FleetContext() { }

    public FleetContext(DbContextOptions<FleetContext> options) : base(options) { }

    public virtual DbSet<Firmware> Firmwares { get; set; }
    public virtual DbSet<HardwareLog> HardwareLogs { get; set; }
    public virtual DbSet<LogSeverity> LogSeverities { get; set; }
    public virtual DbSet<Policy> Policies { get; set; }
    public virtual DbSet<Robot> Robots { get; set; }
    public virtual DbSet<RobotStatus> RobotStatuses { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<User> Users { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            if (entityEntry.State == EntityState.Added)
            {
                if (entityEntry.Metadata.FindProperty("CreatedAt") != null)
                {
                    entityEntry.Property("CreatedAt").CurrentValue = now;
                }
            }

            if (entityEntry.Metadata.FindProperty("UpdatedAt") != null)
            {
                entityEntry.Property("UpdatedAt").CurrentValue = now;
                entityEntry.Property("UpdatedAt").IsModified = true;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Firmware>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("firmware_pkey");
            entity.ToTable("firmware");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Version).HasMaxLength(50).HasColumnName("version");
            entity.Property(e => e.ReleaseDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("release_date");
        });

        modelBuilder.Entity<LogSeverity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("log_severity_pkey");
            entity.ToTable("log_severity");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(50).HasColumnName("name");
        });

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("policies_pkey");
            entity.ToTable("policies");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<RobotStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("robot_statuses_pkey");
            entity.ToTable("robot_statuses");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(50).HasColumnName("name");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");
            entity.ToTable("roles");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(50).HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");
            entity.ToTable("users");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Username).HasMaxLength(100).HasColumnName("username");
            entity.Property(e => e.FullName).HasMaxLength(255).HasColumnName("full_name");
            entity.Property(e => e.PasswordHash).HasMaxLength(255).HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at")
                .ValueGeneratedOnAdd();

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId).HasConstraintName("users_role_id_fkey");
        });

        modelBuilder.Entity<Robot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("robots_pkey");
            entity.ToTable("robots");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.SerialNumber).HasMaxLength(100).HasColumnName("serial_number");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.IpAddress).HasMaxLength(45).HasColumnName("ip_address");
            entity.Property(e => e.PolicyId).HasColumnName("policy_id");
            entity.Property(e => e.FirmwareId).HasColumnName("firmware_id");

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Firmware).WithMany(p => p.Robots)
                .HasForeignKey(d => d.FirmwareId).HasConstraintName("robots_firmware_id_fkey");

            entity.HasOne(d => d.Policy).WithMany(p => p.Robots)
                .HasForeignKey(d => d.PolicyId).HasConstraintName("robots_policy_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Robots)
                .HasForeignKey(d => d.StatusId).HasConstraintName("robots_status_id_fkey");
        });

        modelBuilder.Entity<HardwareLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hardware_logs_pkey");
            entity.ToTable("hardware_logs");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RobotId).HasColumnName("robot_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SeverityId).HasColumnName("severity_id");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Robot).WithMany(p => p.HardwareLogs)
                .HasForeignKey(d => d.RobotId).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("hardware_logs_robot_id_fkey");

            entity.HasOne(d => d.Severity).WithMany(p => p.HardwareLogs)
                .HasForeignKey(d => d.SeverityId).HasConstraintName("hardware_logs_severity_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.HardwareLogs)
                .HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("hardware_logs_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}