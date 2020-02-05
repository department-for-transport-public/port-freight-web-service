using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PortFreight.Services.MSD2
{
    public class Msd2DataService : IMsd2DataService
    {
        private readonly PortFreightContext _context;
        public Msd2DataService(PortFreightContext context)
        {
            _context = context;
        }

        public void Add(Msd2 msd2)
        {
            if (msd2 != null)
            {
                msd2.Id = _context.Msd2.Select(x => x.Id).Max() + 1;
                _context.Msd2.Add(msd2);
                _context.SaveChanges();

            }
        }

        public void DeleteAllPreviousMsd2Data(int fileRefId)
        {
            var file = _context.FlatFile.FirstOrDefault(x => x.FileRefId == fileRefId);

            var flatFiles = _context.FlatFile.Where(x => x.SenderId == file.SenderId
                                      && x.TableRef == file.TableRef
                                      && x.SendersRef == file.SendersRef
                                      );

            foreach (var fileItem in flatFiles)
            {
                var msd2Data = _context.Msd2.Where(x => x.FileRefId == fileItem.FileRefId).ToList();
                _context.Msd2.RemoveRange(msd2Data);
                _context.SaveChanges();
            }
        }

        public (decimal?, decimal?, bool) ValidateGrossWeightInwards(decimal grossweight, uint currentYear, ushort CurrentQuarter, string port, string senderId)
        {
            ushort previousQuarter = CurrentQuarter;
            uint previousYear = currentYear - 1;
            bool inwardBoxWarning = false;

            decimal previousYearQtrGrossWeight = _context.Msd2.
                Where(p => p.ReportingPort == port && p.Year == previousYear && p.Quarter == previousQuarter && p.SenderId == senderId)
                .Select(g => g.GrossWeightInward).SingleOrDefault();

            if (previousYearQtrGrossWeight == 0)
            {
                return (0, 0, false);
            }

            short tolerance = _context.Msd2Threshold
                .Where(t => previousYearQtrGrossWeight >= t.RangeFrom && previousYearQtrGrossWeight <= t.RangeTo && t.ThresholdCategory.Equals("grossweight"))
                .Select(x => x.Tolerance).SingleOrDefault();

            decimal threshold = previousYearQtrGrossWeight * tolerance / 100;
            inwardBoxWarning = grossweight >= (previousYearQtrGrossWeight - threshold) && grossweight <= (previousYearQtrGrossWeight + threshold) ? false : true;

            return (previousYearQtrGrossWeight, threshold, inwardBoxWarning);
        }

        public (decimal?, decimal?, bool) ValidateGrossWeightOutwards(decimal grossweight, uint currentYear, ushort CurrentQuarter, string port, string senderId)
        {
            ushort previousQuarter = CurrentQuarter;
            uint previousYear = currentYear - 1;
            bool outwardBoxWarning = false;

            decimal previousYearQtrGrossWeight = _context.Msd2.
                Where(p => p.Year == previousYear && p.Quarter == previousQuarter && p.ReportingPort == port && p.SenderId == senderId)
                .Select(g => g.GrossWeightOutward).SingleOrDefault();

            if (previousYearQtrGrossWeight == 0)
            {
                return (0, 0, false);
            }

            short tolerance = _context.Msd2Threshold
               .Where(t => previousYearQtrGrossWeight >= t.RangeFrom && previousYearQtrGrossWeight <= t.RangeTo && t.ThresholdCategory.Equals("grossweight"))
               .Select(x => x.Tolerance).SingleOrDefault();

            decimal threshold = previousYearQtrGrossWeight * tolerance / 100;
            outwardBoxWarning = grossweight >= (previousYearQtrGrossWeight - threshold) && grossweight <= (previousYearQtrGrossWeight + threshold) ? false : true;

            return (previousYearQtrGrossWeight, threshold, outwardBoxWarning);
        }

        public (uint, uint, bool) ValidateUnitsInwards(uint? totalUnits, uint currentYear, ushort CurrentQuarter, string port, string senderId)
        {
            ushort previousQuarter = CurrentQuarter;
            uint previousYear = currentYear - 1;
            bool inwardUnitBoxWarning = false;

            uint previousYearQtrUnits = _context.Msd2.
              Where(p => p.Year == previousYear && p.Quarter == previousQuarter && p.ReportingPort == port && p.SenderId == senderId)
              .Select(g => g.TotalUnitsInward).SingleOrDefault();

            if (previousYearQtrUnits == 0)
            {
                return (0, 0, false);
            }

            short tolerance = _context.Msd2Threshold
             .Where(t => previousYearQtrUnits >= t.RangeFrom && previousYearQtrUnits <= t.RangeTo && t.ThresholdCategory.Equals("units"))
             .Select(x => x.Tolerance).SingleOrDefault();

            uint threshold = (uint)(previousYearQtrUnits * tolerance / 100);
            inwardUnitBoxWarning = totalUnits >= (previousYearQtrUnits - threshold) && totalUnits <= (previousYearQtrUnits + threshold) ? false : true;

            return (previousYearQtrUnits, threshold, inwardUnitBoxWarning);
        }
        public (uint, uint, bool) ValidateUnitsOutwards(uint? totalUnits, uint currentYear, ushort CurrentQuarter, string port, string senderId)
        {
            ushort previousQuarter = CurrentQuarter;
            uint previousYear = currentYear - 1;
            bool outwardUnitBoxWarning = false;

            uint previousYearQtrUnits = _context.Msd2.
               Where(p => p.Year == previousYear && p.Quarter == previousQuarter && p.ReportingPort == port && p.SenderId == senderId)
               .Select(g => g.TotalUnitsOutward).SingleOrDefault();

            if (previousYearQtrUnits == 0)
            {
                return (0, 0, false);
            }

            short tolerance = _context.Msd2Threshold
            .Where(t => previousYearQtrUnits >= t.RangeFrom && previousYearQtrUnits <= t.RangeTo && t.ThresholdCategory.Equals("units"))
            .Select(x => x.Tolerance).SingleOrDefault();

            uint threshold = (uint)(previousYearQtrUnits * tolerance / 100);
            outwardUnitBoxWarning = totalUnits >= (previousYearQtrUnits - threshold) && totalUnits <= (previousYearQtrUnits + threshold) ? false : true;

            return (previousYearQtrUnits, threshold, outwardUnitBoxWarning);
        }
    }
}
