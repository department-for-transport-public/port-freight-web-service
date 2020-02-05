using System.Linq;
using System.Text;
using PortFreight.ViewModel;
using System.Collections.Generic;
using PortFreight.Services.Interface;

namespace PortFreight.Services.Export
{
    public class  CsvExtract : ICsvExtract
    {
        public byte[] GenerateMsd1CsvExtract(List<Msd1SummaryViewModel> msd1SummaryViewModel)
        {
            var columnHeaders = new string[]
            {
                "SenderId",
                "Agent",
                "Line",
                "IMO ",
                "ShipName",
                "ShipType" ,
                "CallSign",
                "FlagCode",
                "Year",
                "Quarter",
                "ReportingPort",
                "Load/Unload Port",
                "Direction",
                "NumVoyages",
                "VoyageDate",
                "CargoCategory",
                "GrossWeight",
                "UnitsWithCargo",
                "UnitsWithoutCargo",
                "TotalUnits",
                "Description",
                "DataSource",
                "SendersRecordRef",
                "SendersSubmissionRef",
                "OurRef",
                "UserName",
                "CreatedDate",
                "ModifiedDate",
                "LastUpdatedBy"
            };

            var msd1Data = (from csvRecord in msd1SummaryViewModel
                                select new object[]
                                {
                                    csvRecord.SenderId,
                                    csvRecord.Agent,
                                    $"\"{csvRecord.Line}\"",
                                    csvRecord.IMO,
                                    $"\"{csvRecord.ShipName}\"",
                                    csvRecord.ShipType ,
                                    csvRecord.CallSign ,
                                    csvRecord.FlagCode ,
                                    csvRecord.Year,
                                    csvRecord.Quarter,
                                        $"\"{csvRecord.ReportingPort}\"",
                                    csvRecord.LoadUnloadPort,
                                    csvRecord.Direction,
                                    csvRecord.NumVoyages,
                                    csvRecord.VoyageDate,
                                     $"\"{csvRecord.CargoCategory}\"",
                                    csvRecord.GrossWeight,
                                    csvRecord.UnitsWithCargo,
                                    csvRecord.UnitsWithoutCargo,
                                    csvRecord.TotalUnits,
                                    $"\"{csvRecord.Description}\"",
                                    csvRecord.DataSourceId,
                                    $"\"{csvRecord.SendersRecordRef}\"",
                                    $"\"{csvRecord.SendersSubmissionRef}\"",
                                    csvRecord.OurRef,
                                    csvRecord.UserName,
                                    csvRecord.CreatedDate.ToString("dd/MM/yyyy"),
                                    csvRecord.ModifiedDate?.ToString("dd/MM/yyyy"),
                                    csvRecord.LastUpdatedBy
                                }).ToList();

            var msd1CsvData = new StringBuilder();
            msd1Data.ForEach(line =>
            {
                msd1CsvData.AppendLine(string.Join(",", line));
            });

            return Encoding.ASCII.GetBytes($"{string.Join(",", columnHeaders)}\r\n{msd1CsvData.ToString()}");
        }

         public byte[] GenerateMsd2CsvExtract(List<Msd2SummaryViewModel> msd2SummaryViewModel)
        {
            var columnHeaders = new string[]
            {
                "Id",
                "SenderId",
                "Year",
                "Quarter",
                "ReportingPort",
                "GrossWeightInward",
                "TotalUnitInward",
                "PassengerVehiclesInward",
                "GrossWeightOutward",
                "TotalUnitOutward",
                "PassengerVehiclesOutward",
                "Source",
                "LastUpdatedBy",
                "CreatedDate"

            };

            var msd2Data = (from csvRecord in msd2SummaryViewModel
                            select new object[]
                            {
                                       csvRecord.Id,
                                        csvRecord.SenderId,
                                        csvRecord.Year,
                                        csvRecord.Quarter,
                                         $"\"{csvRecord.ReportingPort}\"",
                                        csvRecord.GrossWeightInward,
                                        csvRecord.TotalUnitInward,
                                        csvRecord.PassengerVehiclesInward,
                                        csvRecord.GrossWeightOutward,
                                        csvRecord.TotalUnitOutward,
                                        csvRecord.PassengerVehiclesOutward,
                                        csvRecord.Source,
                                        csvRecord.LastUpdatedBy,
                                        csvRecord.CreatedDate?.ToString("dd/MM/yyyy")
                             }).ToList();

            var msd2CsvData = new StringBuilder();
            msd2Data.ForEach(line =>
            {
                msd2CsvData.AppendLine(string.Join(",", line));
            });

            return Encoding.ASCII.GetBytes($"{string.Join(",", columnHeaders)}\r\n{msd2CsvData.ToString()}");
        }

        public byte[] GenerateMsd3CsvExtract(List<Msd3SummaryViewModel> msd3SummaryViewModel)
        {
              var columnHeaders = new string[]
            {
                "Id",
                "SenderId",
                "Year",
                "Quarter",
                "ReportingPort",
                "Agents",
                "Source",
                "LastUpdatedBy",
                "CreatedDate"

            };

            var msd3Data = (from csvRecord in msd3SummaryViewModel
                            select new object[]
                            {
                                       csvRecord.Id,
                                        csvRecord.SenderId,
                                        csvRecord.Year,
                                        csvRecord.Quarter,
                                         $"\"{csvRecord.ReportingPort}\"",
                                         $"\"{string.Join(",",csvRecord.Agents)}\"",
                                        csvRecord.Source,
                                        csvRecord.LastUpdatedBy,
                                        csvRecord.CreatedDate?.ToString("dd/MM/yyyy"),
                             }).ToList();

            var msd3CsvData = new StringBuilder();
            msd3Data.ForEach(line =>
            {
                msd3CsvData.AppendLine(string.Join(",", line));
            });

            return Encoding.ASCII.GetBytes($"{string.Join(",", columnHeaders)}\r\n{msd3CsvData.ToString()}");
        }
        
    }
}
