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
        public DbSet<DiseasesHasCauses> DiseasesHasCauses { get; set; }
        public DbSet<Cause> Causes { get; set; }
        public DbSet<DiseasesHasSymptoms> DiseasesHasSymptoms { get; set; }
        public DbSet<Symptom> Symptoms { get; set; }
        public DbSet<DiseasesNeedsMeasures> DiseasesNeedsMeasures { get; set; }
        public DbSet<PreventativeMeasure> PreventativeMeasures { get; set; }
        public DbSet<DiseasesNeedsPesticides> DiseasesNeedsPesticides { get; set; }
        public DbSet<Pesticide> Pesticides { get; set; }        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Disease>().HasData(
                new Disease
                {
                    Id = 1,
                    Name = "Powdery Mildew",
                    Description = "Powdery mildew is one of the major strawberry diseases that attacks all parts of the plant, " +
                        "but is usually first seen on the older leaves. " +
                        "As this fungal disease can overwinter on dead tissue from previous seasons, " +
                        "sanitation is an important part of pre-season management. " +
                        "The common name \"powdery mildew\" is also found in other crop groups, " +
                        "but those are different fungi that cannot infect strawberries " +
                        "(e.g. powdery mildew in strawberries is different from the powdery mildew in raspberries). " +
                        "Day neutral strawberry varieties can be harder hit than June bearing varieties, as they have a longer season.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Disease
                {
                    Id = 2,
                    Name = "Leaf Spot",
                    Description = "Common leaf spot of strawberry (also known as Mycosphaerella leaf spot, Ramularia leaf spot, strawberry leaf spot, bird’s-eye spot, gray spotness, and white spot) is a common fungal leaf disease that affects both wild and cultivated strawberries throughout the world.  " +
                        "Common leaf spot was once the most economically important strawberry disease, but the use of resistant strawberry varieties/cultivars and improvements in methods for growing strawberries have been effective in managing the disease and reducing its impact.  " +
                        "Today, the disease is often a cosmetic problem and typically has little impact on yield or fruit quality.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<Symptom>().HasData(
                new Symptom
                {
                    Id = 1,
                    Title = "White powdery colonies",
                    Description = "Leaves infected with powdery mildew initially have small, " +
                        "white powdery colonies on the undersides of leaves. " +
                        "These colonies enlarge to cover the entire lower leaf surface, " +
                        "causing the edges of the leaves to roll up",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Symptom
                {
                    Id = 2,
                    Title = "Purple reddish blotches",
                    Description = "Purple reddish blotches appear on the upper and lower surface of leaves.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Symptom
                {
                    Id = 3,
                    Title = "Infected flowers",
                    Description = "Infected flowers produce deformed fruit or no fruit at all. " +
                        "Severely infected flowers may be completely covered by mycelium and killed.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Symptom
                {
                    Id = 4,
                    Title = "Infected immature fruits",
                    Description = "Infected immature fruits become hardened and desiccated",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Symptom
                {
                    Id = 5,
                    Title = "Infected mature fruits",
                    Description = "Infected mature fruits become seedy in appearance and " +
                        "support spore-producing colonies that look powdery and white.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Symptom
                {
                    Id = 6,
                    Title = "Leaves",
                    Description = "Small and circular leaf spots with light to tan centers and purplish borders." +
                        "Lesions usually start on the upper leaf surface as small, deep purple, round to irregularly shaped necrotic spots, " +
                        "which can grow to 1–2 mm in diameter",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Symptom
                {
                    Id = 7,
                    Title = "Young leaves",
                    Description = "Spots remain light brown, " +
                        "whereas on older leaves the center changes from tan or brown to gray or white, " +
                        "and the necrotic center is surrounded by reddish-purple to rusty-brown borders. " +
                        "Lesions occur less frequently on the underside of the leaves, and the color is not as vivid. " +
                        "Intense spotting may lead to death of the infected leaves.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
                );

            modelBuilder.Entity<DiseasesHasSymptoms>().HasData(
                new DiseasesHasSymptoms
                {
                    DiseaseId = 1,
                    SymptomId = 1
                },
                new DiseasesHasSymptoms
                {
                    DiseaseId = 1,
                    SymptomId = 2
                },
                new DiseasesHasSymptoms
                {
                    DiseaseId = 1,
                    SymptomId = 3
                },
                new DiseasesHasSymptoms
                {
                    DiseaseId = 1,
                    SymptomId = 4
                },
                new DiseasesHasSymptoms
                {
                    DiseaseId = 1,
                    SymptomId = 5
                },
                new DiseasesHasSymptoms
                {
                    DiseaseId = 2,
                    SymptomId = 6
                },
                new DiseasesHasSymptoms
                {
                    DiseaseId = 2,
                    SymptomId = 7
                }
                );

            modelBuilder.Entity<DiseasesHasSymptoms>()
                .HasKey(ds => new { ds.DiseaseId, ds.SymptomId });
            modelBuilder.Entity<DiseasesNeedsMeasures>()
                .HasKey(dm => new { dm.DiseaseId, dm.PreventativeMeasureId });
            modelBuilder.Entity<DiseasesNeedsPesticides>()
                .HasKey(dp => new { dp.DiseaseId, dp.PesticideId });
            modelBuilder.Entity<DiseasesHasCauses>()
                .HasKey(dc => new { dc.DiseaseId, dc.CauseId });
        }
    }
}
