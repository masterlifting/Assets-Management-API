using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IM.Service.Company.Data.Migrations
{
    public partial class CompanyServiceDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanySourceTypes");

            migrationBuilder.DropTable(
                name: "SourceTypes");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "IndustryId",
                table: "Companies",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "Sectors",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sectors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sources",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Industries",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    SectorId = table.Column<byte>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Industries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Industries_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanySources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CompanyId = table.Column<string>(type: "character varying(10)", nullable: false),
                    SourceId = table.Column<byte>(type: "smallint", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanySources_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanySources_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Sectors",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { (byte)1, null, "Сырье" },
                    { (byte)2, null, "Средства производства" },
                    { (byte)3, null, "Технологии" },
                    { (byte)4, null, "Коммунальные услуги" },
                    { (byte)5, null, "Энергетика" },
                    { (byte)6, null, "Цикличные компании" },
                    { (byte)7, null, "Финансы" },
                    { (byte)8, null, "Нецикличные компании" },
                    { (byte)9, null, "Здравоохранение" },
                    { (byte)10, null, "Услуги" },
                    { (byte)11, null, "Транспорт" }
                });

            migrationBuilder.InsertData(
                table: "Sources",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { (byte)1, "For reports from company official site", "official" },
                    { (byte)2, "For prices from Moscow Exchange", "moex" },
                    { (byte)3, "For prices from Nasdaq Exchange", "tdameritrade" },
                    { (byte)4, "For reports from Investing.com", "investing" }
                });

            migrationBuilder.InsertData(
                table: "Industries",
                columns: new[] { "Id", "Description", "Name", "SectorId" },
                values: new object[,]
                {
                    { (byte)1, null, "Пищевая промышленность", (byte)8 },
                    { (byte)2, null, "Отдых", (byte)10 },
                    { (byte)3, null, "Полупроводники", (byte)3 },
                    { (byte)4, null, "Интегрированная нефтегазовая промышленность", (byte)5 },
                    { (byte)5, null, "Производство и поставки медицинского оборудования", (byte)9 },
                    { (byte)6, null, "Потребительские финансовые услуги", (byte)7 },
                    { (byte)7, null, "Золото и серебро", (byte)1 },
                    { (byte)8, null, "Аэрокосмическая и оборонная промышленность", (byte)2 },
                    { (byte)9, null, "Эфирное и кабельное телевидение", (byte)10 },
                    { (byte)10, null, "Биотехнологии и лекарства", (byte)9 },
                    { (byte)11, null, "Компьютерные услуги", (byte)3 },
                    { (byte)12, null, "Строительство-снабжение", (byte)2 },
                    { (byte)13, null, "Напитки", (byte)8 },
                    { (byte)14, null, "Научно-техническое приборостроение", (byte)3 },
                    { (byte)15, null, "Газоснабжение", (byte)4 },
                    { (byte)16, null, "Электроэнергетика", (byte)4 },
                    { (byte)17, null, "Региональные банки", (byte)7 },
                    { (byte)18, null, "Химическое производство", (byte)1 },
                    { (byte)19, null, "Коммуникационное оборудование", (byte)3 },
                    { (byte)20, null, "Разные промышленные товары", (byte)1 },
                    { (byte)21, null, "Программное обеспечение и программирование", (byte)3 },
                    { (byte)22, null, "Различные средства производства", (byte)2 },
                    { (byte)23, null, "Автомобильная промышленность", (byte)6 },
                    { (byte)24, null, "Деловые услуги", (byte)10 },
                    { (byte)25, null, "Услуги связи", (byte)10 },
                    { (byte)26, null, "Розничная торговля", (byte)10 },
                    { (byte)27, null, "Нерудная промышленность", (byte)1 },
                    { (byte)28, null, "Воздушные перевозки", (byte)11 },
                    { (byte)29, null, "Металлодобывающая промышленность", (byte)1 },
                    { (byte)30, null, "Нефтегазовая промышленность", (byte)5 }
                });

            migrationBuilder.UpdateData(
                table:"Companies",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    "KHC"
                    ,"MDLZ"
                    ,"NKHP"
                    ,"GIS"
                    ,"MCD"
                    ,"CCL"
                    ,"LYV"
                    ,"NCLH"
                    ,"RCL"
                    ,"NVDA"
                    ,"MU"
                    ,"INTC"
                    ,"AMAT"
                    ,"LKOH"
                    ,"NVTK"
                    ,"CVX"
                    ,"COP"
                    ,"ROSN"
                    ,"ABT"
                    ,"BDX"
                    ,"BSX"
                    ,"ALGN"
                    ,"MDT"
                    ,"GPN"
                    ,"AXP"
                    ,"TCSG"
                    ,"PYPL"
                    ,"MA"
                    ,"FIS"
                    ,"LNZL"
                    ,"NEM"
                    ,"POLY"
                    ,"PLZL"
                    ,"SELG"
                    ,"BA"
                    ,"RTX"
                    ,"LMT"
                    ,"RKKE"
                    ,"UNAC"
                    ,"IRKT"
                    ,"HON"
                    ,"DIS"
                    ,"DISCK"
                    ,"NFLX"
                    ,"SIRI"
                    ,"BIIB"
                    ,"JNJ"
                    ,"PFE"
                    ,"MRK"
                    ,"MRNA"
                    ,"ISKJ"
                    ,"VRTX"
                    ,"TWTR"
                    ,"GOOG"
                    ,"YY"
                    ,"FB"
                    ,"IBM"
                    ,"HPQ"
                    ,"YNDX"
                    ,"MMM"
                    ,"MAS"
                    ,"TRMK"
                    ,"RPM"
                    ,"KO"
                    ,"PEP"
                    ,"KDP"
                    ,"DHR"
                    ,"EMR"
                    ,"LVHK"
                    ,"ILMN"
                    ,"GAZP"
                    ,"KMI"
                    ,"NEE"
                    ,"ENRU"
                    ,"MRKV"
                    ,"NRG"
                    ,"SO"
                    ,"D"
                    ,"HYDR"
                    ,"MSNG"
                    ,"ROSB"
                    ,"WFC"
                    ,"VTBR"
                    ,"BAC"
                    ,"SBER"
                    ,"CBOM"
                    ,"WM"
                    ,"PHOR"
                    ,"SHW"
                    ,"KAZT"
                    ,"APD"
                    ,"AKRN"
                    ,"AAPL"
                    ,"NOC"
                    ,"QCOM"
                    ,"CSCO"
                    ,"CHMK"
                    ,"PH"
                    ,"DOV"
                    ,"MAGN"
                    ,"RS"
                    ,"ADBE"
                    ,"PANW"
                    ,"ORCL"
                    ,"MSFT"
                    ,"CRM"
                    ,"LRCX"
                    ,"CMI"
                    ,"TUZA"
                    ,"KMAZ"
                    ,"GE"
                    ,"OMZZP"
                    ,"SVAV"
                    ,"F"
                    ,"TSLA"
                    ,"ZILL"
                    ,"V"
                    ,"RTKM"
                    ,"VZ"
                    ,"MTSS"
                    ,"CMCSA"
                    ,"T"
                    ,"CHTR"
                    ,"LNTA"
                    ,"AMZN"
                    ,"BABA"
                    ,"BBY"
                    ,"DSKY"
                    ,"FIVE"
                    ,"ALRS"
                    ,"LUV"
                    ,"MTLR"
                    ,"SCCO"
                    ,"CHMF"
                    ,"GMKN"
                    ,"SNGS"
                },
                column:"IndustryId",
                values:new object[]
                {
                    (byte)1
                    ,(byte)1
                    ,(byte)1
                    ,(byte)1
                    ,(byte)1
                    ,(byte)2
                    ,(byte)2
                    ,(byte)2
                    ,(byte)2
                    ,(byte)3
                    ,(byte)3
                    ,(byte)3
                    ,(byte)3
                    ,(byte)4
                    ,(byte)4
                    ,(byte)4
                    ,(byte)4
                    ,(byte)4
                    ,(byte)5
                    ,(byte)5
                    ,(byte)5
                    ,(byte)5
                    ,(byte)5
                    ,(byte)6
                    ,(byte)6
                    ,(byte)6
                    ,(byte)6
                    ,(byte)6
                    ,(byte)6
                    ,(byte)7
                    ,(byte)7
                    ,(byte)7
                    ,(byte)7
                    ,(byte)7
                    ,(byte)8
                    ,(byte)8
                    ,(byte)8
                    ,(byte)8
                    ,(byte)8
                    ,(byte)8
                    ,(byte)8
                    ,(byte)9
                    ,(byte)9
                    ,(byte)9
                    ,(byte)9
                    ,(byte)10
                    ,(byte)10
                    ,(byte)10
                    ,(byte)10
                    ,(byte)10
                    ,(byte)10
                    ,(byte)10
                    ,(byte)11
                    ,(byte)11
                    ,(byte)11
                    ,(byte)11
                    ,(byte)11
                    ,(byte)11
                    ,(byte)11
                    ,(byte)12
                    ,(byte)12
                    ,(byte)12
                    ,(byte)12
                    ,(byte)13
                    ,(byte)13
                    ,(byte)13
                    ,(byte)14
                    ,(byte)14
                    ,(byte)14
                    ,(byte)14
                    ,(byte)15
                    ,(byte)15
                    ,(byte)16
                    ,(byte)16
                    ,(byte)16
                    ,(byte)16
                    ,(byte)16
                    ,(byte)16
                    ,(byte)16
                    ,(byte)16
                    ,(byte)17
                    ,(byte)17
                    ,(byte)17
                    ,(byte)17
                    ,(byte)17
                    ,(byte)17
                    ,(byte)18
                    ,(byte)18
                    ,(byte)18
                    ,(byte)18
                    ,(byte)18
                    ,(byte)18
                    ,(byte)19
                    ,(byte)19
                    ,(byte)19
                    ,(byte)19
                    ,(byte)20
                    ,(byte)20
                    ,(byte)20
                    ,(byte)20
                    ,(byte)20
                    ,(byte)21
                    ,(byte)21
                    ,(byte)21
                    ,(byte)21
                    ,(byte)21
                    ,(byte)22
                    ,(byte)22
                    ,(byte)22
                    ,(byte)22
                    ,(byte)22
                    ,(byte)22
                    ,(byte)23
                    ,(byte)23
                    ,(byte)23
                    ,(byte)23
                    ,(byte)24
                    ,(byte)25
                    ,(byte)25
                    ,(byte)25
                    ,(byte)25
                    ,(byte)25
                    ,(byte)25
                    ,(byte)26
                    ,(byte)26
                    ,(byte)26
                    ,(byte)26
                    ,(byte)26
                    ,(byte)26
                    ,(byte)27
                    ,(byte)28
                    ,(byte)29
                    ,(byte)29
                    ,(byte)29
                    ,(byte)29
                    ,(byte)30
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_IndustryId",
                table: "Companies",
                column: "IndustryId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySources_CompanyId",
                table: "CompanySources",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySources_SourceId",
                table: "CompanySources",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Industries_Name",
                table: "Industries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Industries_SectorId",
                table: "Industries",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Sectors_Name",
                table: "Sectors",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sources_Name",
                table: "Sources",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Industries_IndustryId",
                table: "Companies",
                column: "IndustryId",
                principalTable: "Industries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Industries_IndustryId",
                table: "Companies");

            migrationBuilder.DropTable(
                name: "CompanySources");

            migrationBuilder.DropTable(
                name: "Industries");

            migrationBuilder.DropTable(
                name: "Sources");

            migrationBuilder.DropTable(
                name: "Sectors");

            migrationBuilder.DropIndex(
                name: "IX_Companies_IndustryId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Name",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "IndustryId",
                table: "Companies");

            migrationBuilder.CreateTable(
                name: "SourceTypes",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanySourceTypes",
                columns: table => new
                {
                    CompanyId = table.Column<string>(type: "character varying(10)", nullable: false),
                    SourceTypeId = table.Column<byte>(type: "smallint", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySourceTypes", x => new { x.CompanyId, x.SourceTypeId });
                    table.ForeignKey(
                        name: "FK_CompanySourceTypes_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanySourceTypes_SourceTypes_SourceTypeId",
                        column: x => x.SourceTypeId,
                        principalTable: "SourceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SourceTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { (byte)1, "For reports from company official site", "official" },
                    { (byte)2, "For prices from Moscow Exchange", "moex" },
                    { (byte)3, "For prices from Nasdaq Exchange", "tdameritrade" },
                    { (byte)4, "For reports from Investing.com", "investing" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanySourceTypes_SourceTypeId",
                table: "CompanySourceTypes",
                column: "SourceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SourceTypes_Name",
                table: "SourceTypes",
                column: "Name",
                unique: true);
        }
    }
}
