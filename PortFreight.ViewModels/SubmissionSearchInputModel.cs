using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PortFreight.ViewModels
{
    public class SubmissionSearchInputModel
    {
        public string SenderId { get; set; }
        //senders ref in flat_file table
        public string RecordRef { get; set; }
        public string SubmissionRef { get; set; }
        public string Agent { get; set; }
        public string Line { get; set; }
        public string ReportingPort { get; set; }
        public string Port { get; set; }
        public string StatisticalPort { get; set; }
        public string IMO { get; set; }
        public string ShipName { get; set; }
        public string Year { get; set; }
        public string Quarter { get; set; }
        public string DateEntered { get; set; }
        public string Source { get; set; }
        public string SelectedCargoType { get; set; }
        public List<SelectListItem> CargoType { get; set; }

        public SubmissionSearchInputModel()
        {
        }
    }
}
