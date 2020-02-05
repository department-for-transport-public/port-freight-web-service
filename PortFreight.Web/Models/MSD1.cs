using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace PortFreight.Web.Models
{
    public class MSD1
    {
        public MSD1() { }

        public MSD1(MSD1 _msd1)
        {
            Imo = _msd1.Imo;
            ShipName = _msd1.ShipName;
            Deadweight = _msd1.Deadweight;
            Year = _msd1.Year;
            Quarter = _msd1.Quarter;
            AssociatedPort = _msd1.AssociatedPort;
            ReportingPort = _msd1.ReportingPort;
            NumVoyages = _msd1.NumVoyages;
            IsInbound = _msd1.IsInbound;
            Msd1Id = _msd1.Msd1Id;
            AgentSenderID = _msd1.AgentSenderID;
            AgentCompanyName = _msd1.AgentCompanyName;
            LineSenderID = _msd1.LineSenderID;
            LineCompanyName = _msd1.LineCompanyName;
            FlagCode = _msd1.FlagCode;
            RecordRef = _msd1.RecordRef;
            VoyageDate = _msd1.VoyageDate;

            CargoSummary = new List<CargoItem>(_msd1.CargoSummary);
        }

        public uint Imo { get; set; }
        public string ShipName { get; set; }
        public uint Deadweight { get; set; }
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
        public DateTime? VoyageDate { get; set; }
        
    

        public List<CargoItem> CargoSummary { get; set; } = new List<CargoItem>();
    }
}
