using Microsoft.EntityFrameworkCore;
using rWCMS.Models;

namespace rWCMS.Data
{
    public class rWCMSDbContext : DbContext
    {
        public rWCMSDbContext(DbContextOptions<rWCMSDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<AppGroup> AppGroups { get; set; }
        public DbSet<ADEntity> ADEntities { get; set; }
        public DbSet<AppGroupMember> AppGroupMembers { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<FileVersion> FileVersions { get; set; }
        public DbSet<BasePermission> BasePermissions { get; set; }
        public DbSet<PermissionLevel> PermissionLevels { get; set; }
        public DbSet<PermissionLevelAssignment> PermissionLevelAssignments { get; set; }
        public DbSet<PathPermission> PathPermissions { get; set; }
        public DbSet<PathPermissionAssignment> PathPermissionAssignments { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<PublishWorkflow> PublishWorkflows { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<WorkflowApprover> WorkflowApprovers { get; set; }
        public DbSet<WorkflowFile> WorkflowFiles { get; set; }
        public DbSet<AssociationBundle> AssociationBundles { get; set; }
        public DbSet<BundleGroup> BundleGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure primary keys
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<AppGroup>()
                .HasKey(a => a.AppGroupId);

            modelBuilder.Entity<ADEntity>()
                .HasKey(a => a.ADEntityId);

            modelBuilder.Entity<Item>()
                .HasKey(i => i.ItemId);

            modelBuilder.Entity<FileVersion>()
                .HasKey(f => f.VersionId);

            modelBuilder.Entity<BasePermission>()
                .HasKey(b => b.BasePermissionId);

            modelBuilder.Entity<PermissionLevel>()
                .HasKey(p => p.PermissionLevelId);

            modelBuilder.Entity<PathPermission>()
                .HasKey(p => p.PathPermissionId);

            modelBuilder.Entity<PathPermissionAssignment>()
                .HasKey(ppa => new { ppa.PathPermissionId, ppa.AppGroupId, ppa.PermissionLevelId });

            modelBuilder.Entity<SystemSetting>()
                .HasKey(s => s.SettingId);

            modelBuilder.Entity<PublishWorkflow>()
                .HasKey(w => w.WorkflowId);

            modelBuilder.Entity<AuditLog>()
                .HasKey(a => a.AuditId);

            modelBuilder.Entity<AssociationBundle>()
                .HasKey(b => b.BundleId);

            // Configure composite keys
            modelBuilder.Entity<AppGroupMember>()
                .HasKey(agm => new { agm.AppGroupId, agm.ADEntityId });

            modelBuilder.Entity<PermissionLevelAssignment>()
                .HasKey(pla => new { pla.PermissionLevelId, pla.BasePermissionId });

            modelBuilder.Entity<WorkflowApprover>()
                .HasKey(wa => new { wa.WorkflowId, wa.UserId });

            modelBuilder.Entity<WorkflowFile>()
                .HasKey(wf => new { wf.WorkflowId, wf.ItemId });

            modelBuilder.Entity<BundleGroup>()
                .HasKey(bg => new { bg.BundleId, bg.AppGroupId });

            // Configure relationships
            modelBuilder.Entity<AppGroup>()
                .HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AppGroup>()
                .HasOne(a => a.ModifiedBy)
                .WithMany()
                .HasForeignKey(a => a.ModifiedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.CreatedBy)
                .WithMany()
                .HasForeignKey(i => i.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.ModifiedBy)
                .WithMany()
                .HasForeignKey(i => i.ModifiedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FileVersion>()
                .HasOne(f => f.Item)
                .WithMany(i => i.FileVersions)
                .HasForeignKey(f => f.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FileVersion>()
                .HasOne(f => f.CreatedBy)
                .WithMany()
                .HasForeignKey(f => f.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PermissionLevel>()
                .HasOne(p => p.CreatedBy)
                .WithMany()
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PermissionLevel>()
                .HasOne(p => p.ModifiedBy)
                .WithMany()
                .HasForeignKey(p => p.ModifiedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PathPermission>()
                .HasOne(p => p.CreatedBy)
                .WithMany()
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PathPermissionAssignment>()
                .HasOne(ppa => ppa.PathPermission)
                .WithMany(p => p.PathPermissionAssignments)
                .HasForeignKey(ppa => ppa.PathPermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PathPermissionAssignment>()
                .HasOne(ppa => ppa.AppGroup)
                .WithMany(a => a.PathPermissionAssignments)
                .HasForeignKey(ppa => ppa.AppGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PathPermissionAssignment>()
                .HasOne(ppa => ppa.PermissionLevel)
                .WithMany(pl => pl.PathPermissionAssignments)
                .HasForeignKey(ppa => ppa.PermissionLevelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PathPermissionAssignment>()
                .HasOne(ppa => ppa.CreatedBy)
                .WithMany()
                .HasForeignKey(ppa => ppa.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PathPermissionAssignment>()
                .HasOne(ppa => ppa.ModifiedBy)
                .WithMany()
                .HasForeignKey(ppa => ppa.ModifiedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SystemSetting>()
                .HasOne(s => s.LastUpdatedBy)
                .WithMany()
                .HasForeignKey(s => s.LastUpdatedById)
                .HasConstraintName("FK_SystemSettings_Users_LastUpdatedById")
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PublishWorkflow>()
                .HasOne(w => w.CreatedBy)
                .WithMany()
                .HasForeignKey(w => w.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PublishWorkflow>()
                .HasOne(w => w.ModifiedBy)
                .WithMany()
                .HasForeignKey(w => w.ModifiedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.Item)
                .WithMany()
                .HasForeignKey(a => a.ItemId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.Workflow)
                .WithMany()
                .HasForeignKey(a => a.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WorkflowApprover>()
                .HasOne(wa => wa.Workflow)
                .WithMany(w => w.WorkflowApprovers)
                .HasForeignKey(wa => wa.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkflowApprover>()
                .HasOne(wa => wa.User)
                .WithMany()
                .HasForeignKey(wa => wa.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkflowFile>()
                .HasOne(wf => wf.Workflow)
                .WithMany(w => w.WorkflowFiles)
                .HasForeignKey(wf => wf.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkflowFile>()
                .HasOne(wf => wf.Item)
                .WithMany(i => i.WorkflowFiles)
                .HasForeignKey(wf => wf.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssociationBundle>()
                .HasOne(b => b.CreatedBy)
                .WithMany()
                .HasForeignKey(b => b.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BundleGroup>()
                .HasOne(bg => bg.Bundle)
                .WithMany(b => b.BundleGroups)
                .HasForeignKey(bg => bg.BundleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BundleGroup>()
                .HasOne(bg => bg.AppGroup)
                .WithMany(a => a.BundleGroups)
                .HasForeignKey(bg => bg.AppGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure enum conversions
            modelBuilder.Entity<ADEntity>()
                .Property(a => a.EntityType)
                .HasConversion<string>();

            modelBuilder.Entity<PublishWorkflow>()
                .Property(w => w.Status)
                .HasConversion<string>();

            // Configure indexes
            modelBuilder.Entity<PathPermission>()
                .HasIndex(p => p.Path)
                .IsUnique();

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.Path)
                .IsUnique();

            modelBuilder.Entity<Item>()
                .HasIndex(i => new { i.LockedByType, i.LockedById });

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.PendingDeletion);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.PendingRename);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.PendingMove);

            modelBuilder.Entity<FileVersion>()
                .HasIndex(fv => fv.ItemId);

            modelBuilder.Entity<AppGroupMember>()
                .HasIndex(agm => agm.AppGroupId);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.ItemId);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.WorkflowId);

            modelBuilder.Entity<PublishWorkflow>()
                .HasIndex(w => w.ScheduleTime);

            modelBuilder.Entity<WorkflowApprover>()
                .HasIndex(wa => wa.WorkflowId);

            modelBuilder.Entity<WorkflowFile>()
                .HasIndex(wf => wf.WorkflowId);

            modelBuilder.Entity<AssociationBundle>()
                .HasIndex(b => b.Name)
                .IsUnique();

            // Configure unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.SID)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.AdLoginId)
                .IsUnique();

            modelBuilder.Entity<AppGroup>()
                .HasIndex(a => a.Name)
                .IsUnique();

            modelBuilder.Entity<ADEntity>()
                .HasIndex(a => a.SID)
                .IsUnique();

            modelBuilder.Entity<FileVersion>()
                .HasIndex(f => new { f.ItemId, f.MajorVersion, f.MinorVersion })
                .IsUnique();

            modelBuilder.Entity<BasePermission>()
                .HasIndex(b => b.Name)
                .IsUnique();

            modelBuilder.Entity<PermissionLevel>()
                .HasIndex(p => p.Name)
                .IsUnique();

            // Configure new Item properties
            modelBuilder.Entity<Item>()
                .Property(i => i.PendingName)
                .HasMaxLength(255);

            modelBuilder.Entity<Item>()
                .Property(i => i.StagingPath)
                .HasMaxLength(850);

            modelBuilder.Entity<Item>()
                .Property(i => i.ProductionPath)
                .HasMaxLength(850);

            modelBuilder.Entity<Item>()
                .Property(i => i.PendingMove)
                .HasDefaultValue(false);

            modelBuilder.Entity<Item>()
                .Property(i => i.PendingPath)
                .HasMaxLength(850);

            // Configure new User property
            modelBuilder.Entity<User>()
                .Property(u => u.IsSiteAdmin)
                .HasDefaultValue(false);

            // Configure PublishWorkflow properties
            modelBuilder.Entity<PublishWorkflow>()
                .Property(w => w.Title)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<PublishWorkflow>()
                .Property(w => w.Description)
                .HasMaxLength(850);
        }
    }
}