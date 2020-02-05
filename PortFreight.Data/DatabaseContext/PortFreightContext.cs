using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using PortFreight.Data.Entities;

namespace PortFreight.Data
{
    public class PortFreightContext : DbContext
    {
        public PortFreightContext()
        {
        }

        public PortFreightContext(DbContextOptions<PortFreightContext> options)
            : base(options)
        {
        }

        public static readonly LoggerFactory LoggerFactory =
            new LoggerFactory(new[]
            {
                new ConsoleLoggerProvider((category, level)
                    => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information, true)
            });

        public virtual DbSet<SenderType> SenderType { get; set; }
        public virtual DbSet<FlatFile> FlatFile { get; set; }
        public virtual DbSet<LogFileData> LogfileData { get; set; }
        public virtual DbSet<SenderIdPort> SenderIdPort { get; set; }
        public virtual DbSet<ContactDetails> ContactDetails { get; set; }
        public virtual DbSet<CargoGroup> CargoGroup { get; set; }
        public virtual DbSet<CargoCategory> CargoCategory { get; set; }
        public virtual DbSet<GlobalPort> GlobalPort { get; set; }
        public virtual DbSet<WorldFleet> WorldFleet { get; set; }
        public virtual DbSet<PortCargoOil> PortCargoOil { get; set; }
        public virtual DbSet<Msd1CargoSummary> Msd1CargoSummary { get; set; }
        public virtual DbSet<Msd1Data> Msd1Data { get; set; }
        public virtual DbSet<Msd1DataSource> Msd1DataSource { get; set; }
        public virtual DbSet<ShipCargoCategory> ShipCargoCategory { get; set; }
        public virtual DbSet<OrgList> OrgList { get; set; }
        public virtual DbSet<Msd2> Msd2 { get; set; }
        public virtual DbSet<Msd3> Msd3 { get; set; }
        public virtual DbSet<Msd3agents> Msd3agents { get; set; }
        public virtual DbSet<FileUploadInfo> FileUploadInfo { get; set; }
        public virtual DbSet<Msd2Threshold> Msd2Threshold { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                throw new Exception("Not configured");
            }
            optionsBuilder.UseLoggerFactory(LoggerFactory).EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SenderIdPort>()
                 .HasKey(sp => new { sp.SenderId, sp.Locode });

            modelBuilder.Entity<SenderIdPort>()
                .HasOne(sp => sp.SenderType)
                .WithMany(s => s.SenderIdPort)
                .HasForeignKey(sp => sp.SenderId);

            modelBuilder.Entity<SenderIdPort>()
                .HasOne(sp => sp.GlobalPort)
                .WithMany(c => c.SenderIdPorts)
                .HasForeignKey(sp => sp.Locode);

            modelBuilder.Entity<LogFileData>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.ToTable("logfile_data");

                entity.HasIndex(e => e.FileRefId)
                    .HasName("flat_file_FileRefId_fk_idx");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.Property(e => e.Description).HasColumnType("longtext");

                entity.Property(e => e.FileRefId).HasColumnType("int(11)");

                entity.Property(e => e.IsEmailed)
                 .HasColumnType("bit(1)");
            });

            modelBuilder.Entity<FlatFile>(entity =>
            {
                entity.HasKey(e => e.FileRefId);

                entity.ToTable("flat_file");

                entity.Property(e => e.FileRefId).HasColumnType("int(11)");

                entity.Property(e => e.CreationDate).HasColumnType("date");

                entity.Property(e => e.FileName).HasColumnType("varchar(12)");

                entity.Property(e => e.IsAmendment)
                    .HasColumnType("tinyint(4)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.IsTest)
                    .HasColumnType("tinyint(4)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.RegistrationNumber).HasColumnType("int(11)");

                entity.Property(e => e.SenderId)
                    .IsRequired()
                    .HasColumnType("varchar(6)");

                entity.Property(e => e.SendersRef).HasColumnType("varchar(12)");

                entity.Property(e => e.TableRef).HasColumnType("varchar(12)");

                entity.Property(e => e.TotalRecords).HasColumnType("int(11)");
            });

            modelBuilder.Entity<GlobalPort>(entity =>
            {
                entity.HasKey(e => e.Locode);

                entity.ToTable("global_port");

                entity.HasIndex(e => e.StatisticalPort)
                    .HasName("statistical_port_fk");

                entity.Property(e => e.Locode)
                    .HasColumnName("locode")
                    .HasColumnType("varchar(5)");

                entity.Property(e => e.CountryCode)
                    .IsRequired()
                    .HasColumnName("country_code")
                    .HasColumnType("varchar(2)");

                entity.Property(e => e.PortName)
                    .IsRequired()
                    .HasColumnName("port_name")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.StatisticalPort)
                    .HasColumnName("statistical_port")
                    .HasColumnType("varchar(5)");

                entity.Property(e => e.ForMsd1LoadUnload)
                    .HasColumnName("for_msd1_load_unload")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.ForMsd1ReportingPort)
                    .HasColumnName("for_msd1_reporting_port")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.ForMsd2)
                    .HasColumnName("for_msd2")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.ForMsd3)
                    .HasColumnName("for_msd3")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.ForMsd4)
                    .HasColumnName("for_msd4")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.ForMsd5)
                    .HasColumnName("for_msd5")
                    .HasColumnType("bit(1)");
            });

            modelBuilder.Entity<WorldFleet>(entity =>
            {
                entity.HasKey(e => e.Imo);

                entity.ToTable("world_fleet");

                entity.Property(e => e.Imo)
                    .HasColumnName("imo")
                    .HasColumnType("mediumint unsigned");

                entity.Property(e => e.CallSign)
                    .HasColumnName("call_sign")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.Deadweight)
                    .IsRequired()
                    .HasColumnName("deadweight")
                    .HasColumnType("mediumint unsigned");

                entity.Property(e => e.FlagCode)
                    .IsRequired()
                    .HasColumnName("flag_code")
                    .HasColumnType("varchar(3)");

                entity.Property(e => e.ShipName)
                    .IsRequired()
                    .HasColumnName("ship_name")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.ShipStatus)
                    .IsRequired()
                    .HasColumnName("ship_status")
                    .HasColumnType("varchar(1)");

                entity.Property(e => e.ShipTypeCode)
                    .IsRequired()
                    .HasColumnName("ship_type_code")
                    .HasColumnType("tinyint(3)");
            });

            modelBuilder.Entity<CargoCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryCode);

                entity.ToTable("cargo_category");

                entity.HasIndex(e => e.GroupCode)
                    .HasName("group_code_fk_idx");

                entity.Property(e => e.CategoryCode)
                    .HasColumnName("category_code");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.GroupCode).HasColumnName("group_code");

                entity.Property(e => e.HasWeight)
                    .HasColumnName("has_weight")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.HelpText)
                    .HasColumnName("help_text")
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.MaxWeight).HasColumnName("max_weight");

                entity.Property(e => e.TakesCargo)
                    .HasColumnName("takes_cargo")
                    .HasColumnType("bit(1)");

                entity.HasOne(d => d.GroupCodeNavigation)
                    .WithMany(p => p.CargoCategory)
                    .HasForeignKey(d => d.GroupCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_group_code");
            });

            modelBuilder.Entity<CargoGroup>(entity =>
            {
                entity.HasKey(e => e.GroupCode);

                entity.ToTable("cargo_group");

                entity.Property(e => e.GroupCode).HasColumnName("group_code");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.IsUnitised)
                    .HasColumnName("is_unitised")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.HelpText)
                   .HasColumnName("help_text")
                   .HasColumnType("varchar(500)");
            });

            modelBuilder.Entity<PortCargoOil>(entity =>
            {
                entity.HasKey(e => e.Locode);

                entity.ToTable("port_cargo_oil");

                entity.Property(e => e.Locode)
                    .HasColumnName("locode")
                    .HasColumnType("varchar(5)");

                entity.Property(e => e.AllowCategory12)
                    .HasColumnName("allow_category_12")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.AllowCategory13Outward)
                    .HasColumnName("allow_category_13_outward")
                    .HasColumnType("bit(1)");
            });

            modelBuilder.Entity<Msd1CargoSummary>(entity =>
            {
                entity.ToTable("MSD1CargoSummary");

                entity.HasIndex(e => e.Msd1Id)
                    .HasName("MSD1_fk_idx");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Description)
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.GrossWeight).HasColumnType("decimal(9,3) unsigned");

                entity.Property(e => e.Msd1Id)
                    .IsRequired()
                    .HasColumnName("Msd1ID")
                    .HasColumnType("char(6)");

                entity.Property(e => e.TotalUnits).HasColumnType("mediumint unsigned");

                entity.Property(e => e.UnitsWithCargo).HasColumnType("mediumint unsigned");

                entity.Property(e => e.UnitsWithoutCargo).HasColumnType("mediumint unsigned");
            });

            modelBuilder.Entity<Msd1Data>(entity =>
            {
                entity.HasKey(e => e.Msd1Id);

                entity.ToTable("MSD1_Data");

                entity.HasIndex(e => e.AssociatedPort)
                    .HasName("Associated_port_fk_idx");

                entity.HasIndex(e => e.DataSourceId)
                    .HasName("Datasource_fk_idx");

                entity.HasIndex(e => e.FileRefId)
                    .HasName("FileRefId_fk_idx");

                entity.HasIndex(e => e.ReportingPort)
                    .HasName("Reporting_port_fk_idx");

                entity.Property(e => e.Msd1Id)
                    .HasColumnName("Msd1ID")
                    .HasColumnType("char(6)");

                entity.Property(e => e.AgentSenderId)
                    .IsRequired()
                    .HasColumnType("varchar(6)");

                entity.Property(e => e.AssociatedPort)
                    .IsRequired()
                    .HasColumnType("varchar(5)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.FileRefId).HasColumnType("int(11)");

                entity.Property(e => e.Imo).HasColumnType("mediumint unsigned");

                entity.Property(e => e.IsInbound).HasColumnType("bit(1)");

                entity.Property(e => e.LineSenderId)
                   .IsRequired()
                   .HasColumnType("varchar(6)");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime");

                entity.Property(e => e.NumVoyages)
                    .HasColumnType("mediumint unsigned")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.VoyageDate)
                    .HasColumnType("datetime");

                entity.Property(e => e.ReportingPort)
                    .IsRequired()
                    .HasColumnType("varchar(5)");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasColumnType("varchar(256)");

                entity.Property(e => e.Year)
                    .HasColumnType("mediumint unsigned");

                entity.Property(e => e.RecordRef)
                   .HasColumnType("varchar(20)");

                entity.Property(e => e.LastUpdatedBy)
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.ShipName)
                   .HasColumnType("varchar(45)");

                entity.Property(e => e.Callsign)
                   .HasColumnType("varchar(10)");

                entity.Property(e => e.FlagCode)
                   .HasColumnType("varchar(3)");

                entity.HasOne(d => d.DataSource)
                    .WithMany(p => p.Msd1Data)
                    .HasForeignKey(d => d.DataSourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Datasource_fk");
            });

            modelBuilder.Entity<Msd1DataSource>(entity =>
            {
                entity.HasKey(e => e.DataSourceId);

                entity.ToTable("MSD1_DataSource");

                entity.Property(e => e.DataSourceId).HasColumnName("DataSourceID");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.Description).HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<SenderType>(entity =>
            {
                entity.HasKey(e => e.SenderId);

                entity.Property(e => e.IsAgent)
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsLine)
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsPort)
                    .HasColumnType("bit(1)");
            });

            modelBuilder.Entity<ShipCargoCategory>(entity =>
            {
                entity.HasKey(e => new { e.ShipTypeCode, e.CargoCategoryCode });

                entity.ToTable("ship_cargo_category");

                entity.Property(e => e.ShipTypeCode)
                    .HasColumnName("ship_type_code")
                    .HasColumnType("tinyint(3)");

                entity.Property(e => e.CargoCategoryCode)
                    .HasColumnName("cargo_Category_code")
                    .HasColumnType("tinyint(2)");
            });

            modelBuilder.Entity<OrgList>(entity =>
            {
                entity.HasKey(e => e.OrgPkId);

                entity.ToTable("org_list");

                entity.HasIndex(e => e.OrgId)
                    .HasName("org_pk_id_uq_idx")
                    .IsUnique();

                entity.Property(e => e.OrgPkId)
                    .HasColumnName("org_pk_id")
                    .HasColumnType("int(11)").HasAnnotation("MySql:ValueGeneratedOnAdd", true); ;

                entity.Property(e => e.OrgId)
                    .HasColumnName("org_id")
                    .HasColumnType("varchar(6)");

                entity.Property(e => e.IsAgent)
                    .HasColumnName("is_agent")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsLine)
                    .HasColumnName("is_line")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.IsPort)
                    .HasColumnName("is_port")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.OrgName)
                    .HasColumnName("org_name")
                    .HasColumnType("varchar(80)");

                entity.Property(e => e.SubmitsMsd1)
                    .HasColumnName("submits_msd1")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.SubmitsMsd2)
                    .HasColumnName("submits_msd2")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.SubmitsMsd3)
                    .HasColumnName("submits_msd3")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.SubmitsMsd4)
                    .HasColumnName("submits_msd4")
                    .HasColumnType("bit(1)");

                entity.Property(e => e.SubmitsMsd5)
                    .HasColumnName("submits_msd5")
                    .HasColumnType("bit(1)");
            });                

            modelBuilder.Entity<Msd2>(entity =>
            {
                entity.ToTable("MSD2");

                entity.HasIndex(e => e.DataSourceId)
                    .HasName("Datasource_fk_idx2");

                entity.HasIndex(e => e.ReportingPort)
                    .HasName("Reporting_port_fk_idx2");

                entity.HasIndex(e => new { e.ReportingPort, e.Year, e.Quarter })
                    .HasName("PortQuarterYearIdx");

                entity.Property(e => e.Id).HasColumnType("int(11)").HasAnnotation("MySql:ValueGeneratedOnAdd", true);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.GrossWeightInward).HasColumnType("decimal(15,3) unsigned");

                entity.Property(e => e.GrossWeightOutward).HasColumnType("decimal(15,3) unsigned");

                entity.Property(e => e.InwardGrossWeightDescription).HasColumnType("varchar(500)");

                entity.Property(e => e.InwardUnitDescription).HasColumnType("varchar(500)");

                entity.Property(e => e.OutwardGrossWeightDescription).HasColumnType("varchar(500)");

                entity.Property(e => e.OutwardUnitDescription).HasColumnType("varchar(500)");

                entity.Property(e => e.LastUpdatedBy).HasColumnType("varchar(100)");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.PassengerVehiclesInward).HasColumnType("mediumint unsigned");

                entity.Property(e => e.PassengerVehiclesOutward).HasColumnType("mediumint unsigned");

                entity.Property(e => e.ReportingPort)
                    .IsRequired()
                    .HasColumnType("varchar(5)");

                entity.Property(e => e.SenderId)
                    .IsRequired()
                    .HasColumnType("varchar(6)");

                entity.Property(e => e.TotalUnitsInward).HasColumnType("mediumint unsigned");

                entity.Property(e => e.TotalUnitsOutward).HasColumnType("mediumint unsigned");

                entity.Property(e => e.Year).HasColumnType("mediumint unsigned");
            });

            modelBuilder.Entity<Msd3>(entity =>
            {
                entity.ToTable("MSD3");

                entity.HasIndex(e => e.DataSourceId)
                    .HasName("Datasource_fk_idx3");

                entity.HasIndex(e => e.ReportingPort)
                    .HasName("Reporting_port_fk_idx3");

                entity.HasIndex(e => new { e.ReportingPort, e.Year, e.Quarter })
                    .HasName("PortQuarterYearIdx3");

                entity.Property(e => e.Id).HasColumnType("char(6)");

                entity.Property(e => e.SenderId)
                   .IsRequired()
                   .HasColumnType("varchar(255)");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy).HasColumnType("varchar(100)");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.ReportingPort)
                    .IsRequired()
                    .HasColumnType("varchar(5)");

                entity.Property(e => e.Year).HasColumnType("mediumint unsigned");
            });

            modelBuilder.Entity<Msd3agents>(entity =>
            {
                entity.ToTable("MSD3Agents");

                entity.HasIndex(e => e.Msd3Id)
                    .HasName("MSD3_fk_idx");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Msd3Id).HasColumnType("char(6)"); ;

                entity.Property(e => e.SenderId)
                    .IsRequired()
                    .HasColumnType("varchar(6)");

                entity.HasOne(d => d.Msd3)
                    .WithMany(p => p.Msd3agents)
                    .HasForeignKey(d => d.Msd3Id)
                    .HasConstraintName("MSD3_fk");
            });

            modelBuilder.Entity<FileUploadInfo>(entity =>
            {
                entity.ToTable("FileUploadInfo");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.UploadBy)
                    .IsRequired()
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.UploadDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Msd2Threshold>(entity =>
            {
                entity.ToTable("MSD2_Threshold");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.RangeFrom).HasColumnType("int(9)");

                entity.Property(e => e.RangeTo).HasColumnType("int(9)");

                entity.Property(e => e.ThresholdCategory)
                    .IsRequired()
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.Tolerance).HasColumnType("smallint(3)");
            });

            modelBuilder.Entity<WorldFleetArchive>(entity =>
            {
                entity.HasKey(e => e.Imo);

                entity.ToTable("world_fleet_archive");

                entity.Property(e => e.Imo)
                    .HasColumnName("imo")
                    .HasColumnType("mediumint unsigned");

                entity.Property(e => e.CallSign)
                    .HasColumnName("call_sign")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.Deadweight)
                    .IsRequired()
                    .HasColumnName("deadweight")
                    .HasColumnType("mediumint unsigned");

                entity.Property(e => e.FlagCode)
                    .IsRequired()
                    .HasColumnName("flag_code")
                    .HasColumnType("varchar(3)");

                entity.Property(e => e.ShipName)
                    .IsRequired()
                    .HasColumnName("ship_name")
                    .HasColumnType("varchar(45)");

                entity.Property(e => e.ShipStatus)
                    .IsRequired()
                    .HasColumnName("ship_status")
                    .HasColumnType("varchar(1)");

                entity.Property(e => e.ShipTypeCode)
                    .IsRequired()
                    .HasColumnName("ship_type_code")
                    .HasColumnType("tinyint(3)");

                entity.Property(e => e.ArchivedDate)
                    .IsRequired()
                    .HasColumnName("archive_date")
                    .HasColumnType("datetime");
            });

        }
    }
}
