using IM.Service.Market.DataAccess.Entities;
using IM.Service.Market.DataAccess.Entities.ManyToMany;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.DataAccess;

public class DatabaseContext : DbContext
{
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<Sources> Sources { get; set; } = null!;

    public DbSet<Industry> Industries { get; set; } = null!;
    public DbSet<Sector> Sectors { get; set; } = null!;

    public DbSet<CompanySource> CompanySources { get; set; } = null!;

    public DbSet<Price> Prices { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<StockSplit> StockSplits { get; set; } = null!;
    public DbSet<StockVolume> StockVolumes { get; set; } = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();
        modelBuilder.Entity<Company>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<Industry>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<Sector>().HasIndex(x => x.Name).IsUnique();

        modelBuilder.Entity<CompanySource>().HasKey(x => new {x.CompanyId, x.SourceId});
        modelBuilder.Entity<CompanySource>()
            .HasOne(x => x.Company)
            .WithMany(x => x.CompanySources)
            .HasForeignKey(x => x.CompanyId);
        modelBuilder.Entity<CompanySource>()
            .HasOne(x => x.Source)
            .WithMany(x => x.CompanySources)
            .HasForeignKey(x => x.SourceId);

        modelBuilder.Entity<Price>().HasKey(x => new {x.CompanyId, x.Date });
        modelBuilder.Entity<StockSplit>().HasKey(x => new {x.CompanyId, x.Date });
        modelBuilder.Entity<StockVolume>().HasKey(x => new {x.CompanyId, x.Date });
        modelBuilder.Entity<Report>().HasKey(x => new {x.CompanyId, x.Year, x.Quarter });

        modelBuilder.Entity<Price>().HasOne(x => x.Company).WithMany(x => x.Prices).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<StockSplit>().HasOne(x => x.Company).WithMany(x => x.StockSplits).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<StockVolume>().HasOne(x => x.Company).WithMany(x => x.StockVolumes).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Report>().HasOne(x => x.Company).WithMany(x => x.Reports).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Sources>().HasData(
            new() { Id = (byte)Enums.Sources.Official, Name = nameof(Enums.Sources.Official).ToLowerInvariant(), Description = "For reports from company official site" },
            new() { Id = (byte)Enums.Sources.Moex, Name = nameof(Enums.Sources.Moex).ToLowerInvariant(), Description = "For prices from Moscow Exchange" },
            new() { Id = (byte)Enums.Sources.Tdameritrade, Name = nameof(Enums.Sources.Tdameritrade).ToLowerInvariant(), Description = "For prices from Nasdaq Exchange" },
            new() { Id = (byte)Enums.Sources.Investing, Name = nameof(Enums.Sources.Investing).ToLowerInvariant(), Description = "For reports from Investing.com" }
        );

        modelBuilder.Entity<Sector>().HasData(
             new() { Id = 1, Name = "�����" }
            , new() { Id = 2, Name = "�������� ������������" }
            , new() { Id = 3, Name = "����������" }
            , new() { Id = 4, Name = "������������ ������" }
            , new() { Id = 5, Name = "����������" }
            , new() { Id = 6, Name = "��������� ��������" }
            , new() { Id = 7, Name = "�������" }
            , new() { Id = 8, Name = "����������� ��������" }
            , new() { Id = 9, Name = "���������������" }
            , new() { Id = 10, Name = "������" }
            , new() { Id = 11, Name = "���������" }
        );
        modelBuilder.Entity<Industry>().HasData(
             new() { Id = 20, SectorId = 1, Name = "������ ������������ ������" }
            , new() { Id = 18, SectorId = 1, Name = "���������� ������������" }
            , new() { Id = 7, SectorId = 1, Name = "������ � �������" }
            , new() { Id = 29, SectorId = 1, Name = "����������������� ��������������" }
            , new() { Id = 27, SectorId = 1, Name = "�������� ��������������" }

            , new() { Id = 12, SectorId = 2, Name = "�������������-���������" }
            , new() { Id = 8, SectorId = 2, Name = "��������������� � ��������� ��������������" }
            , new() { Id = 22, SectorId = 2, Name = "��������� �������� ������������" }

            , new() { Id = 14, SectorId = 3, Name = "������-����������� ���������������" }
            , new() { Id = 11, SectorId = 3, Name = "������������ ������" }
            , new() { Id = 21, SectorId = 3, Name = "����������� ����������� � ����������������" }
            , new() { Id = 19, SectorId = 3, Name = "���������������� ������������" }
            , new() { Id = 3, SectorId = 3, Name = "��������������" }

            , new() { Id = 16, SectorId = 4, Name = "�����������������" }
            , new() { Id = 15, SectorId = 4, Name = "�������������" }

            , new() { Id = 30, SectorId = 5, Name = "������������ ��������������" }
            , new() { Id = 4, SectorId = 5, Name = "��������������� ������������ ��������������" }

            , new() { Id = 23, SectorId = 6, Name = "������������� ��������������" }

            , new() { Id = 17, SectorId = 7, Name = "������������ �����" }
            , new() { Id = 6, SectorId = 7, Name = "��������������� ���������� ������" }

            , new() { Id = 1, SectorId = 8, Name = "������� ��������������" }
            , new() { Id = 13, SectorId = 8, Name = "�������" }

            , new() { Id = 5, SectorId = 9, Name = "������������ � �������� ������������ ������������" }
            , new() { Id = 10, SectorId = 9, Name = "������������� � ���������" }

            , new() { Id = 25, SectorId = 10, Name = "������ �����" }
            , new() { Id = 26, SectorId = 10, Name = "��������� ��������" }
            , new() { Id = 9, SectorId = 10, Name = "������� � ��������� �����������" }
            , new() { Id = 24, SectorId = 10, Name = "������� ������" }
            , new() { Id = 2, SectorId = 10, Name = "�����" }

            , new() { Id = 28, SectorId = 11, Name = "��������� ���������" }
        );

        base.OnModelCreating(modelBuilder);
    }
}