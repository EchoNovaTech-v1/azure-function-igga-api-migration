using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using AzureFunctionIgga.Api.Models.Entities;
using System.Text.Json;

namespace AzureFunctionIgga.Api.Data;

/// <summary>
/// Contexto de base de datos migrado desde Azure Function
/// Implementa Entity Framework Core 8 con mejores prácticas
/// </summary>
public class IggaDbContext : DbContext
{
    public IggaDbContext(DbContextOptions<IggaDbContext> options) : base(options)
    {
    }

    // DbSets - Tablas migradas desde Azure Function
    public DbSet<User> Users { get; set; }
    public DbSet<DataRecord> DataRecords { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ProcessingJob> ProcessingJobs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Report> Reports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar conversores JSON para campos complejos
        ConfigureJsonConverters(modelBuilder);

        // Configurar entidades
        ConfigureUser(modelBuilder);
        ConfigureDataRecord(modelBuilder);
        ConfigureAuditLog(modelBuilder);
        ConfigureProcessingJob(modelBuilder);
        ConfigureNotification(modelBuilder);
        ConfigureReport(modelBuilder);

        // Configurar índices para optimización
        ConfigureIndexes(modelBuilder);

        // Datos semilla
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Configura conversores JSON para campos que almacenan datos complejos
    /// </summary>
    private static void ConfigureJsonConverters(ModelBuilder modelBuilder)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var jsonConverter = new ValueConverter<Dictionary<string, object>?, string>(
            v => v == null ? null : JsonSerializer.Serialize(v, jsonOptions),
            v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, jsonOptions));

        // Aplicar conversores a propiedades específicas
        modelBuilder.Entity<DataRecord>()
            .Property(e => e.Metadata)
            .HasConversion(jsonConverter);

        modelBuilder.Entity<ProcessingJob>()
            .Property(e => e.Parameters)
            .HasConversion(jsonConverter);

        modelBuilder.Entity<ProcessingJob>()
            .Property(e => e.Result)
            .HasConversion(jsonConverter);

        modelBuilder.Entity<Report>()
            .Property(e => e.Parameters)
            .HasConversion(jsonConverter);
    }

    /// <summary>
    /// Configuración de la entidad User
    /// </summary>
    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("User");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Configurar relaciones
            entity.HasMany(e => e.DataRecords)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.AuditLogs)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Configuración de la entidad DataRecord
    /// </summary>
    private static void ConfigureDataRecord(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataRecord>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.Property(e => e.Value)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        });
    }

    /// <summary>
    /// Configuración de la entidad AuditLog
    /// </summary>
    private static void ConfigureAuditLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Timestamp)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        });
    }

    /// <summary>
    /// Configuración de la entidad ProcessingJob
    /// </summary>
    private static void ConfigureProcessingJob(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessingJob>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.JobName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Queued");

            entity.Property(e => e.Progress)
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configuración de la entidad Notification
    /// </summary>
    private static void ConfigureNotification(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Info");

            entity.Property(e => e.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configuración de la entidad Report
    /// </summary>
    private static void ConfigureReport(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Generating");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configuración de índices para optimización de consultas
    /// </summary>
    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Índices para User
        modelBuilder.Entity<User>()
            .HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        modelBuilder.Entity<User>()
            .HasIndex(e => new { e.IsActive, e.Role })
            .HasDatabaseName("IX_Users_IsActive_Role");

        // Índices para DataRecord
        modelBuilder.Entity<DataRecord>()
            .HasIndex(e => new { e.UserId, e.Category, e.Status })
            .HasDatabaseName("IX_DataRecords_UserId_Category_Status");

        modelBuilder.Entity<DataRecord>()
            .HasIndex(e => e.RecordDate)
            .HasDatabaseName("IX_DataRecords_RecordDate");

        // Índices para AuditLog
        modelBuilder.Entity<AuditLog>()
            .HasIndex(e => new { e.EntityType, e.EntityId, e.Timestamp })
            .HasDatabaseName("IX_AuditLogs_EntityType_EntityId_Timestamp");

        // Índices para ProcessingJob
        modelBuilder.Entity<ProcessingJob>()
            .HasIndex(e => new { e.Status, e.CreatedAt })
            .HasDatabaseName("IX_ProcessingJobs_Status_CreatedAt");

        // Índices para Notification
        modelBuilder.Entity<Notification>()
            .HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt })
            .HasDatabaseName("IX_Notifications_UserId_IsRead_CreatedAt");

        // Índices para Report
        modelBuilder.Entity<Report>()
            .HasIndex(e => new { e.UserId, e.Status, e.Type })
            .HasDatabaseName("IX_Reports_UserId_Status_Type");
    }

    /// <summary>
    /// Datos semilla para la base de datos
    /// </summary>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Usuario administrador por defecto
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FirstName = "Administrador",
                LastName = "Sistema",
                Email = "admin@igga.com",
                Role = "Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }

    /// <summary>
    /// Override de SaveChanges para auditoría automática
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Implementar auditoría automática antes de guardar
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries);
        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || 
                entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                TableName = entry.Entity.GetType().Name,
                Action = entry.State.ToString(),
                Timestamp = DateTime.UtcNow
            };

            auditEntries.Add(auditEntry);
        }

        return auditEntries.Where(e => !e.HasTemporaryProperties).ToList();
    }

    private async Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return;

        foreach (var auditEntry in auditEntries)
        {
            AuditLogs.Add(auditEntry.ToAuditLog());
        }

        await base.SaveChangesAsync();
    }
}

/// <summary>
/// Clase auxiliar para manejar entradas de auditoría
/// </summary>
public class AuditEntry
{
    public AuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        Entry = entry;
    }

    public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; }
    public string UserId { get; set; } = "1"; // Usuario por defecto
    public string TableName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object?> KeyValues { get; } = new();
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();
    public List<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry> TemporaryProperties { get; } = new();

    public bool HasTemporaryProperties => TemporaryProperties.Any();

    public AuditLog ToAuditLog()
    {
        return new AuditLog
        {
            Action = Action,
            EntityType = TableName,
            EntityId = int.TryParse(KeyValues.FirstOrDefault().Value?.ToString(), out var id) ? id : 0,
            Timestamp = Timestamp,
            UserId = int.TryParse(UserId, out var uid) ? uid : 1,
            OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
            NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues)
        };
    }
}