using System;
using System.Collections.Generic;
using System.Linq;
using PortFreight.Data.Entities;
using PortFreight.Web.Models;
using PortFreight.Web.Utilities;
using Microsoft.Extensions.Configuration;
using PortFreight.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PortFreight.Services.Common;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace PortFreight.Web.Pages.Msd1
{
    public class SubmitReturnModel : BaseMsd1PageModel
    {
        public MSD1 MSD1 { get; set; }
        public bool FromSummary { get; set; }
        public bool IsEdited { get; set; }
        public Msd1Data Msd1Data;
        public List<Msd1CargoSummary> cargoSummary;
        public DateTime currentDateTime = DateTime.Now;

        private static string uniqueKey;
        private readonly IConfiguration _configuration;
        private readonly IHelperService _helperService;
        private readonly PortFreightContext _context;
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly ILogger<SubmitReturnModel> _logger;

        public SubmitReturnModel(
            PortFreightContext context,
            IConfiguration configuration,
            UserManager<PortFreightUser> userManager,
            IHelperService helperService,
            ILogger<SubmitReturnModel> logger)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _helperService = helperService;
            _logger = logger;
        }
        public void OnGet()
        {
            GetMSD1();

            FromSummary = Helpers.ReturnBoolFromQueryString(HttpContext, "FromSummary");
            IsEdited = Helpers.ReturnBoolFromQueryString(HttpContext, "IsEdited");
        }

        public IActionResult OnPostAsync()
        {
            try
            {
                GetMSD1();
                MapMsd1ToMsd1Data();
                SaveMSD1Data();
            }

            catch (Exception ex) 

            {
                ModelState.AddModelError("MSD1Error", "There has been an error in saving MSD1 data");
                _logger.LogError(ex.Message);
                return Page();
            }

            if (ModelState.Keys.Contains("UniqueKeyError"))
            {
                return Page();
            }

            IsEdited = Helpers.ReturnBoolFromQueryString(HttpContext, "IsEdited");
            if (IsEdited)
            {
                return RedirectToPage("../Summary/SubmissionSummary", new {IsEdited = "true"});
            }

            return RedirectToPage("./Success");

        }

        private void SaveMSD1Data()
        {
            if (MSD1.Msd1Id == null)
            {
                _context.Msd1Data.Add(Msd1Data);

                MSD1.Msd1Id = Msd1Data.Msd1Id;

                TempData.Put(MSD1Key, MSD1);
            }

            _context.SaveChanges();
        }

        private void MapMsd1ToMsd1Data()
        {
            Msd1Data = _context.Msd1Data.FirstOrDefault(x => x.Msd1Id == MSD1.Msd1Id) ?? new Msd1Data();

            Msd1Data.Msd1Id = MSD1.Msd1Id ?? _helperService.GetUniqueKey();
            Msd1Data.Imo = MSD1.Imo;
            Msd1Data.ShipName = MSD1.ShipName; 
            Msd1Data.FlagCode = MSD1.FlagCode;
            Msd1Data.Year = MSD1.Year;
            Msd1Data.Quarter = MSD1.Quarter;
            Msd1Data.AssociatedPort = _helperService.GetPortCodeByPort(MSD1.AssociatedPort) ?? MSD1.AssociatedPort;
            Msd1Data.ReportingPort = _helperService.GetPortCodeByPort(MSD1.ReportingPort) ?? MSD1.ReportingPort;
            Msd1Data.NumVoyages = MSD1.NumVoyages;
            Msd1Data.IsInbound = MSD1.IsInbound == null ? false : MSD1.IsInbound.Value;
            Msd1Data.VoyageDate = MSD1.VoyageDate;
            Msd1Data.UserName = Msd1Data.UserName ?? _userManager.GetUserName(HttpContext.User);
            Msd1Data.DataSourceId = (int)DataSource.WEB;
            Msd1Data.CreatedDate = Msd1Data.CreatedDate != default(DateTime) && Msd1Data.CreatedDate < DateTime.Now ?
                    Msd1Data.CreatedDate : DateTime.SpecifyKind(currentDateTime, DateTimeKind.Utc);
            Msd1Data.LastUpdatedBy = _userManager.GetUserName(HttpContext.User);
            Msd1Data.ModifiedDate = DateTime.SpecifyKind(currentDateTime, DateTimeKind.Utc);
            Msd1Data.AgentSenderId = MSD1.AgentSenderID;
            Msd1Data.LineSenderId = MSD1.LineSenderID;
            Msd1Data.RecordRef = MSD1.RecordRef;

            Msd1Data.Msd1CargoSummary = _context.Msd1CargoSummary.Where(x => x.Msd1Id == Msd1Data.Msd1Id)
                .ToList();

            if (Msd1Data.Msd1CargoSummary.Count > 0)
            {
                Msd1Data.Msd1CargoSummary.Clear();
            }

            foreach (CargoItem item in MSD1.CargoSummary)
            {
                Msd1CargoSummary msd1CargoSummary = new Msd1CargoSummary
                {
                    Msd1Id = Msd1Data.Msd1Id,
                    CategoryCode = _helperService.GetCategoryCodeByDescription(item.Category),
                    UnitsWithCargo = item.UnitsWithCargo,
                    UnitsWithoutCargo = item.UnitsWithoutCargo,
                    TotalUnits = item.TotalUnits,
                    GrossWeight = (decimal?)item.GrossWeight,
                    Description = item.Description
                };

                Msd1Data.Msd1CargoSummary.Add(msd1CargoSummary);
            };
        }

        private void GetMSD1()
        {
            MSD1 = new MSD1(TempData.GetKeep<MSD1>(MSD1Key));
        }
    }
}