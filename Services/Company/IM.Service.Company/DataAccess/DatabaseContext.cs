using IM.Service.Company.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

namespace IM.Service.Company.DataAccess;

public sealed class DatabaseContext : DbContext
{
    public DbSet<Entities.Company> Companies { get; set; } = null!;
    public DbSet<Industry> Industries { get; set; } = null!;
    public DbSet<Sector> Sectors { get; set; } = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        //Database.EnsureDeleted();
        //Database.EnsureCreated();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();
        modelBuilder.Entity<Entities.Company>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<Sector>().HasData(
             new() { Id = 1, Name = "Сырье" }
            , new() { Id = 2, Name = "Средства производства" }
            , new() { Id = 3, Name = "Технологии" }
            , new() { Id = 4, Name = "Коммунальные услуги" }
            , new() { Id = 5, Name = "Энергетика" }
            , new() { Id = 6, Name = "Цикличные компании" }
            , new() { Id = 7, Name = "Финансы" }
            , new() { Id = 8, Name = "Нецикличные компании" }
            , new() { Id = 9, Name = "Здравоохранение" }
            , new() { Id = 10, Name = "Услуги" }
            , new() { Id = 11, Name = "Транспорт" }
        );
        modelBuilder.Entity<Industry>().HasData(
             new() { Id = 1, SectorId = 1, Name = "Разные промышленные товары" }
            , new() { Id = 2, SectorId = 1, Name = "Химическое производство" }
            , new() { Id = 3, SectorId = 1, Name = "Золото и серебро" }
            , new() { Id = 4, SectorId = 1, Name = "Металлодобывающая промышленность" }
            , new() { Id = 5, SectorId = 1, Name = "Нерудная промышленность" }

            , new() { Id = 6, SectorId = 2, Name = "Строительство-снабжение" }
            , new() { Id = 7, SectorId = 2, Name = "Аэрокосмическая и оборонная промышленность" }
            , new() { Id = 8, SectorId = 2, Name = "Различные средства производства" }

            , new() { Id = 9, SectorId = 3, Name = "Научно-техническое приборостроение" }
            , new() { Id = 10, SectorId = 3, Name = "Компьютерные услуги" }
            , new() { Id = 11, SectorId = 3, Name = "Программное обеспечение и программирование" }
            , new() { Id = 12, SectorId = 3, Name = "Коммуникационное оборудование" }
            , new() { Id = 13, SectorId = 3, Name = "Полупроводники" }

            , new() { Id = 14, SectorId = 4, Name = "Электроэнергетика" }
            , new() { Id = 15, SectorId = 4, Name = "Газоснабжение" }

            , new() { Id = 16, SectorId = 5, Name = "Нефтегазовая промышленность" }
            , new() { Id = 17, SectorId = 5, Name = "Интегрированная нефтегазовая промышленность" }

            , new() { Id = 18, SectorId = 6, Name = "Автомобильная промышленность" }

            , new() { Id = 19, SectorId = 7, Name = "Региональные банки" }
            , new() { Id = 20, SectorId = 7, Name = "Потребительские финансовые услуги" }

            , new() { Id = 21, SectorId = 8, Name = "Пищевая промышленность" }
            , new() { Id = 22, SectorId = 8, Name = "Напитки" }

            , new() { Id = 23, SectorId = 9, Name = "Производство и поставки медицинского оборудования" }
            , new() { Id = 24, SectorId = 9, Name = "Биотехнологии и лекарства" }

            , new() { Id = 25, SectorId = 10, Name = "Услуги связи" }
            , new() { Id = 26, SectorId = 10, Name = "Розничная торговля" }
            , new() { Id = 27, SectorId = 10, Name = "Эфирное и кабельное телевидение" }
            , new() { Id = 28, SectorId = 10, Name = "Деловые услуги" }
            , new() { Id = 29, SectorId = 10, Name = "Отдых" }

            , new() { Id = 30, SectorId = 11, Name = "Воздушные перевозки" }
        );
        base.OnModelCreating(modelBuilder);
    }
}