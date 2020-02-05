using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Web.Utilities;
using PortFreight.Services.Common;
using PortFreight.Web.Models;

namespace PortFreight.Web.Pages.Summary
{
    public class SubmissionSummaryModel : PageModel
    {
        private readonly UserDbContext _UserContext;
        private readonly PortFreightContext _context;
        private readonly ILogger<SubmissionSummaryModel> _logger;
        private readonly IHelperService _helperService;
        private int numberOfRecords { get; set; } = 0;
        public bool IsEdited { get; set; }
        public bool FromSummary { get; set; }
    

        public List<Msd1Data> Msd1s = new List<Msd1Data>();

        public SubmissionSummaryModel(
            PortFreightContext context,
            UserDbContext userContext,
            IHelperService helperService,
            ILogger<SubmissionSummaryModel> logger)
        {
            _logger = logger;
            _context = context;
            _UserContext = userContext;
            _helperService = helperService;
        }

        public IActionResult OnGet()
        {
            if (TempData.ContainsKey("Search"))
            {
                numberOfRecords = (TempData.Peek("Search").ToString() == "") ?
                25 : numberOfRecords = int.Parse(TempData.Peek("Search").ToString());
            }
            else
            {
                numberOfRecords = 25;
            }

            IsEdited = Helpers.ReturnBoolFromQueryString(HttpContext, "IsEdited");

            GetMSD1s();



            return Page();
        }

        public IActionResult OnPostViewSummary(string Id)
        {
            try
            {
                TempData.Put("MSD1Key", mapMSD1DataToMSD1(Id));
                return RedirectToPage("../Msd1/SubmitReturn", new { FromSummary = "true" });
            }
            catch
            {
                return Page();
            }
        }

        public IActionResult OnPostGetNumberOfResults(string number)
        {
            numberOfRecords = Convert.ToInt16(number);
            GetMSD1s();
            TempData["Search"] = number;
            return Page();
        }

        private void GetMSD1s()
        {
            try
            {
                foreach (PortFreightUser user in GetEmailAddressesForSenderID())
                {
                    foreach (Msd1Data msd1Data in _context.Msd1Data.Where(x => x.UserName == user.Email))
                    {
                        Msd1s.Add(msd1Data);
                    }
                }
                Msd1s = Msd1s.OrderByDescending(x => x.CreatedDate).ToList();
                Msd1s = Msd1s.GetRange(0, numberOfRecords == 0 ? Msd1s.Count() : numberOfRecords);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        private IQueryable<PortFreightUser> GetEmailAddressesForSenderID()
        {
            string SenderID = _UserContext.Users
                .FirstOrDefault(x => x.UserName == HttpContext.User.Identity.Name)
                .SenderId
                .ToUpper();

            return _UserContext.Users.Where(x => x.SenderId == SenderID);
        }

        private MSD1 mapMSD1DataToMSD1(string Id)
        {
            Msd1Data msd1Data = _context.Msd1Data.First(x => x.Msd1Id == Id) ?? new Msd1Data();

            MSD1 msd1 = new MSD1()
            {
                Imo = msd1Data.Imo,
                ShipName = msd1Data.ShipName ??_helperService.GetShipNameByIMO(msd1Data.Imo),
                Deadweight = _helperService.GetDeadweightByIMO(msd1Data.Imo),
                Year = msd1Data.Year,
                Quarter = msd1Data.Quarter,
                AssociatedPort = _helperService.GetPortNameByCode(msd1Data.AssociatedPort)
                    ?? msd1Data.AssociatedPort,
                ReportingPort = _helperService.GetPortNameByCode(msd1Data.ReportingPort)
                    ?? msd1Data.ReportingPort,
                NumVoyages = msd1Data.NumVoyages,
                IsInbound = msd1Data.IsInbound,
                Msd1Id = msd1Data.Msd1Id,
                AgentSenderID = msd1Data.AgentSenderId,
                AgentCompanyName = _helperService.GetCompanyNameBySenderID(msd1Data.AgentSenderId) ?? string.Empty,
                LineSenderID = msd1Data.LineSenderId,
                LineCompanyName = _helperService.GetCompanyNameBySenderID(msd1Data.LineSenderId) ?? string.Empty,
                FlagCode = _helperService.GetFlagCodeFromIMO(msd1Data.Imo) ?? string.Empty,
                RecordRef = msd1Data.RecordRef,
                CargoSummary = MapMsd1CargoSummaryToCargoItem(msd1Data.Msd1Id)
            };

            return msd1;
        }

        private List<CargoItem> MapMsd1CargoSummaryToCargoItem(string id)
        {
            List<CargoItem> cargoItems = new List<CargoItem>();

            try
            {
                IQueryable<Msd1CargoSummary> cargoSummary = _context.Msd1CargoSummary.Where(x => x.Msd1Id == id)
                .AsQueryable();

                foreach (Msd1CargoSummary msd1CargoSummary in cargoSummary)
                {
                    CargoItem cargoItem = new CargoItem()
                    {
                        Id = Guid.NewGuid(),
                        Description = msd1CargoSummary.Description,
                        Category = _helperService.GetCategoryDescriptionByCode(msd1CargoSummary.CategoryCode),
                        GrossWeight = (double?)msd1CargoSummary.GrossWeight,
                        TotalUnits = msd1CargoSummary.TotalUnits,
                        UnitsWithCargo = msd1CargoSummary.UnitsWithCargo,
                        UnitsWithoutCargo = msd1CargoSummary.UnitsWithoutCargo,
                    };

                    cargoItems.Add(cargoItem);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            return cargoItems;
        }
    }
}