using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;

namespace PortFreight.Services.MSD1
{
    public class Msd1DataService : IMsd1DataService
    {      
        private readonly PortFreightContext _context;
        public Msd1DataService(PortFreightContext context)
        {
            _context = context;
        }
        public void Add(Msd1Data msd1)
        {
            if (msd1 != null)
            {                
                _context.Msd1Data.Add(msd1);               
                _context.SaveChanges();
                _context.Entry(msd1).State = EntityState.Detached;
            }            
        }
        public void DeleteAllPreviousMsd1Data(int fileRefId)
        {
            var file = _context.FlatFile.FirstOrDefault(x => x.FileRefId ==  fileRefId);

            var flatFiles = _context.FlatFile.Where(x => x.SenderId == file.SenderId
                                      && x.TableRef == file.TableRef
                                      && x.SendersRef == file.SendersRef
                                      );

            foreach (var fileItem in flatFiles)
            {
                var msd1Data = _context.Msd1Data.Include("Msd1CargoSummary").Where(x => x.FileRefId == fileItem.FileRefId).ToList();
                _context.Msd1Data.RemoveRange(msd1Data);
                _context.SaveChanges();

            }            
        }
    }
}

