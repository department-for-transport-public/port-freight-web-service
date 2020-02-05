using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PortFreight.Services.MSD2
{
    public class Msd3DataService : IMsd3DataService
    {
        private readonly PortFreightContext _context;

        public Msd3DataService(PortFreightContext context)
        {
            _context = context;
        }

        public void Add(Msd3 msd3)
        {
            if (msd3 == null) return;
            _context.Msd3.Add(msd3);
            _context.SaveChanges();
            _context.Entry(msd3).State = EntityState.Detached;
        }

        public void DeleteAllPreviousMsd3Data(int fileRefId)
        {
            var file = _context.FlatFile.FirstOrDefault(x => x.FileRefId == fileRefId);
            var flatFiles = _context.FlatFile.Where(x => x.SenderId == file.SenderId && x.TableRef == file.TableRef && x.SendersRef == file.SendersRef);

            foreach (var fileItem in flatFiles)
            {
                var msd3Data = _context.Msd3.SingleOrDefault(x => x.FileRefId == fileItem.FileRefId);
                if (msd3Data == null) continue;
                {
                    var msd3Agents = _context.Msd3agents.Where(x => x.Msd3Id == msd3Data.Id).ToList();
                    _context.Msd3agents.RemoveRange(msd3Agents);
                    _context.Msd3.RemoveRange(msd3Data);
                    _context.SaveChanges();
                }
            }
        }
    }
}
