using System;
using System.Collections.Generic;
using System.Text;

namespace PortFreight.Services.Models
{
   public class MSD1_1
    {
        public MSD1_1() { }

        public MSD1_1(MSD1_1 _msd1_1)
        {
            Imo = _msd1_1.Imo;
            ShipName = _msd1_1.ShipName;
            DeadWeight = _msd1_1.DeadWeight;
            Year = _msd1_1.Year;
            Quarter = _msd1_1.Quarter;
            AssociatedPort = _msd1_1.AssociatedPort;
            ReportingPort = _msd1_1.ReportingPort;
            NumVoyages = _msd1_1.NumVoyages;
            IsInbound = _msd1_1.IsInbound;
            Msd1Id = _msd1_1.Msd1Id;
            AgentSenderID = _msd1_1.AgentSenderID;
            AgentCompanyName = _msd1_1.AgentCompanyName;
            LineSenderID = _msd1_1.LineSenderID;
            LineCompanyName = _msd1_1.LineCompanyName;
            FlagCode = _msd1_1.FlagCode;
            RecordRef = _msd1_1.RecordRef;

            CargoSummary = new List<CargoItem>(_msd1_1.CargoSummary);
        }

        public uint Imo { get; set; }
        public string ShipName { get; set; }
        public uint DeadWeight { get; set; }
        public uint Year { get; set; }
        public ushort Quarter { get; set; }
        public string AssociatedPort { get; set; }
        public string ReportingPort { get; set; }
        public uint NumVoyages { get; set; } = 1;
        public bool? IsInbound { get; set; }
        public string Msd1Id { get; set; }
        public string AgentSenderID { get; set; }
        public string AgentCompanyName { get; set; }
        public string LineSenderID { get; set; }
        public string LineCompanyName { get; set; }
        public string FlagCode { get; set; }
        public string RecordRef { get; set; }

        public List<CargoItem> CargoSummary { get; set; } = new List<CargoItem>();
    }
}
