using Microsoft.EntityFrameworkCore;
using PredictionService.Models;

namespace PredictionService.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Prediction> Predictions { get; set; }
    }
}
