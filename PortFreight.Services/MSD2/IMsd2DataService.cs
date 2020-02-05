using PortFreight.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PortFreight.Services.MSD2
{
    public interface IMsd2DataService
    {
        void Add(Msd2 msd2);
        void DeleteAllPreviousMsd2Data(int fileRefId);
        (decimal?, decimal?, bool) ValidateGrossWeightInwards(decimal grossweight, uint currentYear, ushort CurrentQuarter, string port, string senderId);
        (decimal?, decimal?, bool) ValidateGrossWeightOutwards(decimal grossweight, uint currentYear, ushort CurrentQuarter, string port, string senderId);
        (uint, uint, bool) ValidateUnitsInwards(uint? totalUnits, uint currentYear, ushort CurrentQuarter, string port, string senderId);
        (uint, uint, bool) ValidateUnitsOutwards(uint? totalUnits, uint currentYear, ushort CurrentQuarter, string port, string senderId);
    }
}
