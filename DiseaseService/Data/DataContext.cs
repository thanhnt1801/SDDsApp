using DiseaseService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiseaseService.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            foreach (var changedEntity in ChangeTracker.Entries())
            {
                if (changedEntity.Entity is DateBaseEntity entity)
                {
                    switch (changedEntity.State)
                    {
                        case EntityState.Added:
                            entity.CreatedAt = now;
                            entity.UpdatedAt = now;
                            break;

                        case EntityState.Modified:
                            Entry(entity).Property(x => x.CreatedAt).IsModified = false;
                            entity.UpdatedAt = now;
                            break;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<Disease> Diseases { get; set; }
        public DbSet<DiseaseImages> DiseaseImages { get; set; }
        public DbSet<DiseasesHasCauses> DiseasesHasCauses { get; set; }
        public DbSet<Cause> Causes { get; set; }
        public DbSet<CauseImages> CauseImages { get; set; }
        public DbSet<DiseasesHasSymptoms> DiseasesHasSymptoms { get; set; }
        public DbSet<Symptom> Symptoms { get; set; }
        public DbSet<SymptomImages> SymptomImages { get; set; }
        public DbSet<DiseasesNeedsMeasures> DiseasesNeedsMeasures { get; set; }
        public DbSet<PreventativeMeasure> PreventativeMeasures { get; set; }
        public DbSet<PreventativeMeasureImages> PreventativeMeasureImages { get; set; }
        public DbSet<DiseasesNeedsPesticides> DiseasesNeedsPesticides { get; set; }
        public DbSet<Pesticide> Pesticides { get; set; }
        public DbSet<PesticideImages> PesticideImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DiseasesHasSymptoms>()
                .HasKey(ds => new { ds.DiseaseId, ds.SymptomId });
            modelBuilder.Entity<DiseasesNeedsMeasures>()
                .HasKey(dm => new { dm.DiseaseId, dm.PreventativeMeasureId });
            modelBuilder.Entity<DiseasesNeedsPesticides>()
                .HasKey(dp => new { dp.DiseaseId, dp.PesticideId });
            modelBuilder.Entity<DiseasesHasCauses>()
                .HasKey(dc => new { dc.DiseaseId, dc.CauseId });
            modelBuilder.Entity<CauseImages>()
                .HasOne(cs => cs.Cause)
                .WithMany(cs => cs.CauseImages)
                .HasForeignKey(cs => cs.CauseId);
            modelBuilder.Entity<DiseaseImages>()
                .HasOne(cs => cs.Disease)
                .WithMany(cs => cs.DiseaseImages)
                .HasForeignKey(cs => cs.DiseaseId);
            modelBuilder.Entity<PesticideImages>()
                .HasOne(cs => cs.Pesticide)
                .WithMany(cs => cs.PesticideImages)
                .HasForeignKey(cs => cs.PesticideId);
            modelBuilder.Entity<PreventativeMeasureImages>()
                .HasOne(cs => cs.PreventativeMeasure)
                .WithMany(cs => cs.PreventativeMeasureImages)
                .HasForeignKey(cs => cs.PreventativeMeasureId);
            modelBuilder.Entity<SymptomImages>()
                .HasOne(cs => cs.Symptom)
                .WithMany(cs => cs.SymptomImages)
                .HasForeignKey(cs => cs.SymptomId);
        }
    }
}
