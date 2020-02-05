using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using PortFreight.Web.Models;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Pages.QuarterlyReturn
{
    public class ChecklistModel : BaseMsd1PageModel
    {
        private readonly PortFreightContext _context;
        public PortFreightUser LoggedInUser { get; set; }
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly IHelperService _helperService;
        public MSD23 MSD23 { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        [BindProperty]
        public List<AgentQuarterViewModel> AgentQuarters { get; set; }
        [BindProperty]
        public List<CargoQuarterViewModel> CargoQuarters { get; set; }

        public List<SelectListItem> ReportingPorts { get; set; }

        public ChecklistModel(PortFreightContext context, UserManager<PortFreightUser> userManager, IHelperService helperService)
        {
            _context = context;
            _userManager = userManager;
            _helperService = helperService;
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Select a reporting port")]
            [Display(Name = "Reporting Port")]
            public string ReportingPort { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string input = null)
        {
            LoggedInUser = await _userManager.GetUserAsync(HttpContext.User);
            InitialisePage(LoggedInUser, input);

            return Page();
        }

        public async Task<IActionResult> OnPostAgentReturn(string id)
        {
            string portname = await _context.GlobalPort.AsNoTracking().Where(x => x.Locode == Input.ReportingPort).Select(x => x.PortName).FirstOrDefaultAsync();
            MSD23 = CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD23Key] != null ? new MSD23(TempData.GetKeep<MSD23>(MSD23Key)) : new MSD23();
            MSD23.Port = Input.ReportingPort;
            MSD23.Year = ushort.Parse(id.Substring(1, 4));
            MSD23.Quarter = ushort.Parse(id.Substring(0, 1));
            MSD23.PortName = portname;
            TempData.Put(MSD23Key, MSD23);

            return RedirectToPage("./AgentReturn");
        }

        public async Task<IActionResult> OnPostCargoReturn(string id)
        {
            string portname = await _context.GlobalPort.AsNoTracking().Where(x => x.Locode == Input.ReportingPort).Select(x => x.PortName).FirstOrDefaultAsync();
            MSD23 = CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD23Key] != null ? new MSD23(TempData.GetKeep<MSD23>(MSD23Key)) : new MSD23();
            MSD23.Port = Input.ReportingPort;
            MSD23.Year = ushort.Parse(id.Substring(1, 4));
            MSD23.Quarter = ushort.Parse(id.Substring(0, 1));
            MSD23.PortName = portname;
            TempData.Put(MSD23Key, MSD23);

            return RedirectToPage("./CargoReturn");
        }

        private void InitialisePage(PortFreightUser user, string port)
        {
            if (port == null)
            {
                var locodes = (from p in _context.SenderIdPort
                               join t in _context.SenderType on p.SenderId equals t.SenderId
                               where (t.IsPort == true) && p.SenderId == user.SenderId
                               select p.Locode);
                IOrderedQueryable<GlobalPort> ports = _context.GlobalPort.AsNoTracking().Where(x => locodes.Contains(x.Locode)).OrderByDescending(o => o.Locode);
                ReportingPorts = ports.Select(n => new SelectListItem
                {
                    Value = n.Locode,
                    Text = n.PortName,
                    Selected = true
                }).ToList();
            }
            port = port ?? ReportingPorts[ReportingPorts.Count - 1].Value;
            GetSubmittedQuarters(port, user);
        }

        private void GetSubmittedQuarters(string port, PortFreightUser user)
        {
            port = port ?? ReportingPorts[ReportingPorts.Count - 1].Value;
            if (port.Any())
            {
                AgentQuarters = _context.Msd3
                   .Where(p =>
                   p.ReportingPort == port &&
                   p.SenderId == user.SenderId &&
                   (p.Year == DateTime.Now.Year || p.Year == DateTime.Now.AddYears(-1).Year))
                   .Select(x => new AgentQuarterViewModel
                   {
                       ReportingPort = x.ReportingPort,
                       Year = x.Year,
                       Quarter = x.Quarter
                   })
                   .ToList();

                CargoQuarters = _context.Msd2
                                .Where(p => p.ReportingPort == port && p.SenderId == user.SenderId &&
                                (p.Year == DateTime.Now.Year || p.Year == DateTime.Now.AddYears(-1).Year))
                                .Select(x => new CargoQuarterViewModel
                                {
                                    ReportingPort = x.ReportingPort,
                                    Year = x.Year,
                                    Quarter = x.Quarter
                                })
                               .ToList();
            }
            var locodes = (from p in _context.SenderIdPort
                           join t in _context.SenderType on p.SenderId equals t.SenderId
                           where (t.IsPort == true) && p.SenderId == user.SenderId
                           select p.Locode);
            IOrderedQueryable<GlobalPort> ports = _context.GlobalPort.AsNoTracking().Where(x => locodes.Contains(x.Locode)).OrderByDescending(o => o.Locode);
            ReportingPorts = ports.Select(n => new SelectListItem
            {
                Value = n.Locode,
                Text = n.PortName,
            }).ToList();
            ReportingPorts.ForEach(x =>
            {
                x.Selected = x.Value == port;
            });
        }
    }
}
