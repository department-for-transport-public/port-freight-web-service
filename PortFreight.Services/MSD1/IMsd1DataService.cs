using PortFreight.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PortFreight.Services.MSD1
{
   public interface IMsd1DataService
    {
        void Add(Msd1Data msd1);
        void DeleteAllPreviousMsd1Data(int fileRefId);
    }
}
