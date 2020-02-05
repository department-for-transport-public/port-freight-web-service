using System;

namespace PortFreight.Data.Entities
{
    public partial class WorldFleetArchive
    {
        public uint Imo { get; set; }
        public string ShipName { get; set; }
        public string CallSign { get; set; }
        public string FlagCode { get; set; }
        public uint Deadweight { get; set; }
        public string ShipStatus { get; set; }
        public sbyte ShipTypeCode { get; set; }
        public DateTime ArchivedDate { get; set; }
    }
}
