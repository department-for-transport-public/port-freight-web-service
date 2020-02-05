using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace PortFreight.Services.Common
{
    public class HelperService : IHelperService
    {
        private readonly PortFreightContext _context;
        public HelperService(PortFreightContext context)
        {
            _context = context;
        }

        public string GetFlagCodeFromIMO(uint imo)
        {
            return _context.WorldFleet.AsNoTracking()
                .Where(x => x.Imo == imo)
                .Select(x => x.FlagCode)
                .SingleOrDefault();
        }

        public string GetCompanyNameBySenderID(string senderID)
        {
            return _context.OrgList.AsNoTracking()
                .Where(x => x.OrgId == senderID)
                .Select(x => x.OrgName).SingleOrDefault();
        }

        public string GetShipNameByIMO(uint IMO)
        {
            return _context.WorldFleet.AsNoTracking()
                .Where(x => x.Imo == IMO)
                .Select(x => x.ShipName).SingleOrDefault();
        }
        public uint GetDeadweightByIMO(uint IMO)
        {
            return _context.WorldFleet.AsNoTracking()
                .Where(x => x.Imo == IMO)
                .Select(x => x.Deadweight).SingleOrDefault();
        }

        public string GetCategoryDescriptionByCode(byte code)
        {
            return _context.CargoCategory.AsNoTracking()
                .Where(x => x.CategoryCode == code)
                .Select(x => x.Description).SingleOrDefault();

        }

        public byte GetCategoryCodeByDescription(string category)
        {
            return _context.CargoCategory.AsNoTracking()
                              .Where(x => x.Description == category)
                              .Select(x => x.CategoryCode).SingleOrDefault();
        }

        public string GetPortCodeByPort(string portName)
        {
            return _context.GlobalPort.AsNoTracking()
                  .Where(x => x.PortName == portName)
                  .Select(x => x.Locode).SingleOrDefault();
        }

        public string GetPortNameByCode(string code)
        {
            return _context.GlobalPort.AsNoTracking()
                .Where(x => x.Locode == code)
                .Select(x => x.PortName).SingleOrDefault();
        }

        public string GetCountryCodeByPort(string portName)
        {
            return _context.GlobalPort.AsQueryable().AsNoTracking()
                  .Where(x => x.PortName == portName)
                  .Select(x => x.CountryCode).SingleOrDefault();
        }

        public string GenerateRandomAlphaNumericString()
        {
            string strAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            int keyLength = 6;
            var outOfRange = byte.MaxValue + 1 - (byte.MaxValue + 1) % strAlphabet.Length;
            return string.Concat(
                            Enumerable
                                .Repeat(0, int.MaxValue)
                                .Select(e => RandomByte())
                                .Where(randomByte => randomByte < outOfRange)
                                .Take(keyLength)
                                .Select(randomByte => strAlphabet[randomByte % strAlphabet.Length])
                        );
        }

        public string GetUniqueKey()
        {
            var uniqueKey = GenerateRandomAlphaNumericString();
            while (_context.Msd1Data.Any(x => x.Msd1Id == uniqueKey))
            {
                uniqueKey = GenerateRandomAlphaNumericString();
            };
            return uniqueKey;
        }

        public byte RandomByte()
        {
            using (var randomizationProvider = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[1];
                randomizationProvider.GetBytes(randomBytes);
                return randomBytes.Single();
            }
        }

        public SenderType GetSenderType(string senderId)
        {
            return _context.SenderType
                .AsNoTracking()
                .Where(x => x.SenderId == senderId)
                .Select(x => new SenderType
                {
                    IsAgent =  x.IsAgent,
                    IsLine =  x.IsLine,
                    IsPort = x.IsPort
                })
                .SingleOrDefault();
        }

        public List<SelectListItem> GetPortsByCountryCode(string countryCode)
        {
            return  _context.GlobalPort.AsNoTracking().Where(x => x.CountryCode.ToUpper() == countryCode)
                        .OrderBy(n => n.PortName)
                        .Select(n =>
                            new SelectListItem
                            {
                                Value = n.Locode,
                                Text = n.PortName
                            })
                            .ToList();
        }

        public List<string> GetRespondentPorts()
        {
            return _context.GlobalPort.AsNoTracking()
                        .Where(x => x.ForMsd2 || x.ForMsd3 || x.ForMsd4 || x.ForMsd5)
                        .OrderBy(n => n.PortName)
                        .Select(n => n.PortName)
                            .ToList();
        }

        public List<string> GetReportingPorts()
        {
            return _context.GlobalPort.AsNoTracking()
                        .Where(x => x.ForMsd1ReportingPort)
                        .Select(x => x.PortName)
                        .ToList();
        }

        public List<string> GetPortsOfLoadUnload()
        {
            return _context.GlobalPort.AsNoTracking()
                        .Where(x => x.ForMsd1LoadUnload)
                        .Select(x => x.PortName)
                        .ToList();
        }

        public bool IsValidImo(uint imo)
        {
            return _context.WorldFleet.Any(x => x.Imo == imo);
        }

        public bool IsValidPort(string port)
        {
            return _context.GlobalPort.Any(x => x.Locode == port);
        }
        public bool IsValidMsd2Port(string port)
        {
            return _context.GlobalPort.Any(x => x.Locode == port && x.ForMsd2);
        }
        public bool IsValidMsd3Port(string port)
        {
            return _context.GlobalPort.Any(x => x.Locode == port && x.ForMsd3);
        }
        public bool IsValidReportingPort(string port)
        {
            return _context.GlobalPort.Any(x => x.Locode == port && x.ForMsd1ReportingPort);
        }

    }
}
