using Microsoft.EntityFrameworkCore;
using API_trip_link.Models;

namespace API_trip_link.Data
{
    //מחלקה האחראית על העברת הנתונים ושליחתם למסד נתונים 
    //מחלקה זו בעצם המרגמת בין c# ל sql
    public class TripContext : DbContext
    {
        public TripContext(DbContextOptions<TripContext> options) : base(options) { }
        // ─── DbSets ───────────────────────────────────────────────────────────────
        //טבלאות המסד הנתונים
        public DbSet<Destination>             Destinations             { get; set; }
        public DbSet<DifficultyLevel>         DifficultyLevels         { get; set; }
        public DbSet<TypeTraveler>            TypeTravelers            { get; set; }
        public DbSet<DestinationFeature>      DestinationFeatures      { get; set; }
        public DbSet<CategoriesOfDestination> CategoriesOfDestinations { get; set; }
        public DbSet<Category>                Categories               { get; set; }
        public DbSet<FeatureType>             FeatureTypes             { get; set; }
        public DbSet<Station>                 Stations                 { get; set; }
        public DbSet<StationToDestination>    StationToDestinations    { get; set; }
        public DbSet<Bus>                     Buses                    { get; set; }
        public DbSet<BusStation>              BusStations              { get; set; }
        public DbSet<Trip>                    Trips                    { get; set; }
        public DbSet<DesOfTrip>               DesOfTrips               { get; set; }
        public DbSet<CategoriesToTrip>        CategoriesToTrips        { get; set; }
        public DbSet<FeatureToTrip>           FeatureToTrips           { get; set; }
        public DbSet<NatureTrip>              NatureTrips              { get; set; }
        public DbSet<User>                    Users                    { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ─── Composite Keys ───────────────────────────────────────────────────

            modelBuilder.Entity<CategoriesOfDestination>()
                .HasKey(c => new { c.CategoriesId, c.DesId });

            modelBuilder.Entity<BusStation>()
                .HasKey(b => new { b.BusId, b.StationId });

            modelBuilder.Entity<DesOfTrip>()
                .HasKey(d => new { d.TripId, d.DesId });

            modelBuilder.Entity<CategoriesToTrip>()
                .HasKey(c => new { c.CategoriesId, c.TripId });

            modelBuilder.Entity<StationToDestination>()
                .HasKey(s => new { s.DesId, s.StationNum });

            // ─── Relationships ────────────────────────────────────────────────────

            // Destination → DifficultyLevel
            modelBuilder.Entity<Destination>()
                .HasOne(d => d.DifficultyLevel)
                .WithMany(dl => dl.Destinations)
                .HasForeignKey(d => d.LevelId);

            // Destination → TypeTraveler
            modelBuilder.Entity<Destination>()
                .HasOne(d => d.TypeTraveler)
                .WithMany(t => t.Destinations)
                .HasForeignKey(d => d.TravelerId);

            // CategoriesOfDestination → Destination
            modelBuilder.Entity<CategoriesOfDestination>()
                .HasOne(c => c.Destination)
                .WithMany(d => d.CategoriesOfDestinations)
                .HasForeignKey(c => c.DesId);

            // CategoriesOfDestination → Category
            modelBuilder.Entity<CategoriesOfDestination>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.CategoriesOfDestinations)
                .HasForeignKey(c => c.CategoriesId);

            // StationToDestination → Destination
            modelBuilder.Entity<StationToDestination>()
                .HasOne(s => s.Destination)
                .WithMany(d => d.StationToDestinations)
                .HasForeignKey(s => s.DesId);

            // StationToDestination → Station
            modelBuilder.Entity<StationToDestination>()
                .HasOne(s => s.Station)
                .WithMany(st => st.StationToDestinations)
                .HasForeignKey(s => s.StationNum);

            // BusStation → Bus
            modelBuilder.Entity<BusStation>()
                .HasOne(bs => bs.Bus)
                .WithMany(b => b.BusStations)
                .HasForeignKey(bs => bs.BusId);

            // BusStation → Station
            modelBuilder.Entity<BusStation>()
                .HasOne(bs => bs.Station)
                .WithMany(s => s.BusStations)
                .HasForeignKey(bs => bs.StationId);

            // Trip → User
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.User)
                .WithMany(u => u.Trips)
                .HasForeignKey(t => t.UserId);

            // DesOfTrip → Trip
            modelBuilder.Entity<DesOfTrip>()
                .HasOne(d => d.Trip)
                .WithMany(t => t.DesOfTrips)
                .HasForeignKey(d => d.TripId);

            // DesOfTrip → Destination
            modelBuilder.Entity<DesOfTrip>()
                .HasOne(d => d.Destination)
                .WithMany(dest => dest.DesOfTrips)
                .HasForeignKey(d => d.DesId);

            // CategoriesToTrip → Category
            modelBuilder.Entity<CategoriesToTrip>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.CategoriesToTrips)
                .HasForeignKey(c => c.CategoriesId);

            // CategoriesToTrip → Trip
            modelBuilder.Entity<CategoriesToTrip>()
                .HasOne(c => c.Trip)
                .WithMany(t => t.CategoriesToTrips)
                .HasForeignKey(c => c.TripId);

            // FeatureToTrip → FeatureType
            modelBuilder.Entity<FeatureToTrip>()
                .HasOne(f => f.FeatureType)
                .WithMany(ft => ft.FeatureToTrips)
                .HasForeignKey(f => f.FeatureId);

            // FeatureToTrip → Trip
            modelBuilder.Entity<FeatureToTrip>()
                .HasOne(f => f.Trip)
                .WithMany(t => t.FeatureToTrips)
                .HasForeignKey(f => f.TripId);

            // NatureTrip → Trip
            modelBuilder.Entity<NatureTrip>()
                .HasOne(n => n.Trip)
                .WithMany(t => t.NatureTrips)
                .HasForeignKey(n => n.TripId);

            // NatureTrip → DifficultyLevel
            modelBuilder.Entity<NatureTrip>()
                .HasOne(n => n.DifficultyLevel)
                .WithMany(dl => dl.NatureTrips)
                .HasForeignKey(n => n.LevelId);

            modelBuilder.Entity<Destination>(entity =>
            {
                entity.Property(d => d.Lat).HasPrecision(9, 6);
                entity.Property(d => d.Lon).HasPrecision(9, 6);
            });

            modelBuilder.Entity<Station>(entity =>
            {
                entity.Property(s => s.Lat).HasPrecision(9, 6);
                entity.Property(s => s.Lon).HasPrecision(9, 6);
            });

            modelBuilder.Entity<Trip>(entity =>
            {
                entity.Property(t => t.TripCost).HasPrecision(18, 2);
            });
        }
    }
}
