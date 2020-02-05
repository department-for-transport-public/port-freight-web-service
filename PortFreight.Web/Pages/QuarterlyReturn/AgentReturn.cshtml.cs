using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using PortFreight.Web.Models;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Pages.QuarterlyReturn
{
    public class AgentReturnModel : BaseMsd1PageModel
    {
        private readonly PortFreightContext _context;
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        public List<SelectListItem> ReportingPorts { get; set; }
        public IList<string> Senders { get; set; }

        public Agent AgentItem { get; set; }
        public MSD3ViewModel MSD3VM { get; set; }
        public Msd3 MSD3Data;
        public MSD23 MSD23 { get; set; }
        protected readonly string SubmittedKey = "SubmittedKey";

        public DateTime currentDateTime = DateTime.Now;
        private static string msd3UniqueKey;
        public string NameSort { get; set; }

        public PortFreightUser LoggedInUser { get; set; }
        private readonly UserManager<PortFreightUser> _userManager;
        private readonly IHelperService _helperService;

        public AgentReturnModel(PortFreightContext context, UserManager<PortFreightUser> userManager, IHelperService helperService)
        {
            _context = context;
            _userManager = userManager;
            _helperService = helperService;
        }

        public class InputModel
        {
            [Display(Name = "Shipping agent")]
            public string SenderId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string sortOrder)
        {
            LoggedInUser = await _userManager.GetUserAsync(HttpContext.User);
            InitialisePage(LoggedInUser);
            MSD3VM = CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD3Key] != null ? new MSD3ViewModel(TempData.GetKeep<MSD3ViewModel>(MSD3Key)) : new MSD3ViewModel();

            string submitted = CustomExtensions.NotNullOrEmpty(TempData) && TempData[SubmittedKey] != null ? TempData.GetKeep<string>(SubmittedKey) : "";
            var existingParent = FindSubmission();
            if (existingParent != null)
            {
                TempData.Put("SubmittedKey", "true");
                TempData.Remove(MSD3Key);
                AssignMSD3ToInput(existingParent);
            }
            else
            {
                string slectedYearQuarter = MSD23.Year.ToString() + MSD23.Quarter.ToString();
                if (submitted.ToString() == "true")
                {
                    MSD3VM.AgentSummary.Clear();
                    TempData.Remove(MSD3Key);
                }
                else if (submitted.ToString() != "false" + slectedYearQuarter)
                {
                    MSD3VM.AgentSummary.Clear();
                    TempData.Remove(MSD3Key);
                }
                TempData.Put("SubmittedKey", "false" + slectedYearQuarter);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            LoggedInUser = await _userManager.GetUserAsync(HttpContext.User);
            MSD3VM = CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD3Key] != null ? new MSD3ViewModel(TempData.GetKeep<MSD3ViewModel>(MSD3Key)) : new MSD3ViewModel();
            MSD23 = CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD23Key] != null ? new MSD23(TempData.GetKeep<MSD23>(MSD23Key)) : new MSD23();
            if (MSD3VM.AgentSummary.Count < 1) { ModelState.AddModelError("Input.SenderId", "No shipping Line or Agents entered"); }

            msd3UniqueKey = _helperService.GetUniqueKey();
            if (msd3UniqueKey == null) { ModelState.AddModelError("UniqueKeyError", "Failed to generate a unique Key"); }

            if (!ModelState.IsValid)
            {
                InitialisePage(LoggedInUser);
                return Page();
            }

            MSD3Data = new Msd3
            {
                Id = msd3UniqueKey,
                SenderId = LoggedInUser.SenderId,
                ReportingPort = MSD23.Port,
                Year = MSD23.Year,
                Quarter = MSD23.Quarter,
                DataSourceId = (uint)DataSource.WEB,
                CreatedDate = DateTime.SpecifyKind(currentDateTime, DateTimeKind.Utc),
                ModifiedDate = DateTime.SpecifyKind(currentDateTime, DateTimeKind.Utc),
                CreatedBy = LoggedInUser.Email.ToString(),
                LastUpdatedBy = LoggedInUser.Email.ToString()
            };
 
            var existingParent = FindSubmission();
            if (existingParent != null)
            {
                msd3UniqueKey = existingParent.Id;
                MSD3Data.Id = existingParent.Id;
                MSD3Data.CreatedDate = existingParent.CreatedDate;
                MSD3Data.CreatedBy = existingParent.CreatedBy;
                _context.Entry(existingParent).CurrentValues.SetValues(MSD3Data);

                _context.Msd3agents.RemoveRange(existingParent.Msd3agents);

                foreach (Agent item in MSD3VM.AgentSummary)
                {
                    Msd3agents agentSummary = new Msd3agents
                    {
                        Msd3Id = msd3UniqueKey,
                        SenderId = item.ShippingAgent.Split('-').FirstOrDefault().TrimEnd()
                    };
                    existingParent.Msd3agents.Add(agentSummary);
                };
            }
            else
            {
                foreach (Agent item in MSD3VM.AgentSummary)
                {
                    Msd3agents agentSummary = new Msd3agents
                    {
                        Msd3Id = msd3UniqueKey,
                        SenderId = item.ShippingAgent.Split('-').FirstOrDefault().TrimEnd()
                    };
                    MSD3Data.Msd3agents.Add(agentSummary);
                };
                await _context.AddAsync(MSD3Data);
            }

            await _context.SaveChangesAsync();
            TempData.Remove(MSD3Key);

            return RedirectToPage("./Success");
        }

        public async Task OnPostAddAgent()
        {
            ModelState.Clear();
            LoggedInUser = await _userManager.GetUserAsync(HttpContext.User);

            if (Input.SenderId == null) { ModelState.AddModelError("Input.SenderId", "Enter a valid shipping agent"); }
            var localMSD3 = CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD3Key] != null ? new MSD3ViewModel(TempData.GetKeep<MSD3ViewModel>(MSD3Key)) : new MSD3ViewModel();
            if (localMSD3.AgentSummary.Count > 0)
            {
                bool isAlreadyAdded = localMSD3.AgentSummary.Any(x => x.ShippingAgent == Input.SenderId);
                if (isAlreadyAdded) { ModelState.AddModelError("Input.SenderId", "Agent or line is already added"); }
            }

            if (!ModelState.IsValid)
            {
                InitialisePage(LoggedInUser);
            }
            else
            {
                AgentItem = new Agent
                {
                    Id = Guid.NewGuid(),
                    ShippingAgent = Input.SenderId
                };
                localMSD3.AgentSummary.Add(AgentItem);
                TempData.Put(MSD3Key, localMSD3);
                Input.SenderId = "";
                InitialisePage(LoggedInUser);
            }
            MSD3VM = CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD3Key] != null ? new MSD3ViewModel(TempData.GetKeep<MSD3ViewModel>(MSD3Key)) : new MSD3ViewModel();
        }

        public async Task OnPostRemoveAgent(Guid? Id)
        {
            ModelState.Clear();
            LoggedInUser = await _userManager.GetUserAsync(HttpContext.User);
            if (Id != null && Id != Guid.Empty)
            {
                var localMSD3 = CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD3Key] != null ? new MSD3ViewModel(TempData.GetKeep<MSD3ViewModel>(MSD3Key)) : new MSD3ViewModel(); ;
                var agentItem = localMSD3.AgentSummary.Where(x => x.Id == Id).SingleOrDefault();
                localMSD3.AgentSummary.Remove(agentItem);
                TempData.Put(MSD3Key, localMSD3);
            }
            InitialisePage(LoggedInUser);
            MSD3VM = CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD3Key] != null ? new MSD3ViewModel(TempData.GetKeep<MSD3ViewModel>(MSD3Key)) : new MSD3ViewModel();
        }

        private void InitialisePage(PortFreightUser user)
        {
            MSD23 = CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD23Key] != null ? new MSD23(TempData.GetKeep<MSD23>(MSD23Key)) : new MSD23();
            Senders = _context.OrgList
                       .Where(s => s.IsAgent || s.IsLine)
                       .Select(s =>
                            s.OrgId.ToString() + " - " + s.OrgName.ToString()
                            ).ToList();
        }

        private Msd3 FindSubmission()
        {
            return _context.Msd3.Include(p => p.Msd3agents)
                .Where(p => p.ReportingPort == MSD23.Port && p.Year == MSD23.Year
                && p.Quarter == MSD23.Quarter && p.SenderId == LoggedInUser.SenderId)
                .Include(p => p.Msd3agents)
                .SingleOrDefault();
        }

        private void AssignMSD3ToInput(Msd3 msd3)
        {
            var localMSD3 = CustomExtensions.NotNullOrEmpty(TempData) && TempData[MSD3Key] != null ? new MSD3ViewModel(TempData.GetKeep<MSD3ViewModel>(MSD3Key)) : new MSD3ViewModel(); ;
            MSD3VM.AgentSummary.Clear();
            MSD23 = new MSD23(TempData.GetKeep<MSD23>(MSD23Key));
            MSD23.Year = msd3.Year;
            MSD23.Quarter = msd3.Quarter;
            MSD23.Port = msd3.ReportingPort;
            MSD23.PortName = _helperService.GetPortNameByCode(msd3.ReportingPort);
            foreach (Msd3agents msd3Agent in msd3.Msd3agents)
            {
                Agent agent = new Agent
                {
                    Id = Guid.NewGuid(),
                    ShippingAgent = _context.OrgList.Where(s => s.OrgId == msd3Agent.SenderId)
                    .Select(s => s.OrgId.ToString() + " - " + s.OrgName.ToString()).FirstOrDefault()
                };
                MSD3VM.AgentSummary.Add(agent);
                localMSD3.AgentSummary.Add(agent);
            };
            TempData.Put(MSD3Key, localMSD3);
        }
    }
}