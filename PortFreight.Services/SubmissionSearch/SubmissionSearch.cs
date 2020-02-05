using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Interface;
using PortFreight.Services.User;
using PortFreight.ViewModel;

namespace PortFreight.Services.SubmissionSearch
{
    public class SubmissionSearch :ISubmissionSearch 
    {
        public SubmissionSearch(PortFreightContext context, UserDbContext userContext, IUserService userService)
        {
            _context = context;
            _userContext = userContext;
            _userService = userService;
        }

        #region privateVariables 
        private readonly PortFreightContext _context;
        private readonly UserDbContext _userContext;
        private IUserService _userService;
        private IQueryable<Msd1Data> _msd1Queryable;
        private IQueryable<Msd2> _msd2Queryable;
        private IQueryable<Msd3> _msd3Queryable;
        private List<Msd1SummaryViewModel> _msd1SummarySearchResultList = new List<Msd1SummaryViewModel>();
        private List<Msd2SummaryViewModel> _msd2SummarySearchResultList = new List<Msd2SummaryViewModel>();
        private List<Msd3SummaryViewModel> _msd3SummarySearchResultList = new List<Msd3SummaryViewModel>();
        private Dictionary<string, string> _searchFilterDictionary = new Dictionary<string, string>();
        private enum MSDData
        {
            MSD1 = 1,
            MSD2 = 2,
            MSD3 = 3
        }
        #endregion

        public List<Msd1SummaryViewModel> GenerateMsd1ViewModelFilteredOnSearchCriteria(SubmissionSearchInputModel searchModel, bool resultToBeExported)
        {
            _searchFilterDictionary.Clear();
           CreateSearchDictionary(searchModel);
           if (_searchFilterDictionary.Count == 0)  return new List<Msd1SummaryViewModel>();

           CreateMsd1List(_searchFilterDictionary, MSDData.MSD1);
           if (_msd1Queryable == null) return new List<Msd1SummaryViewModel>();

           AddToSummaryViewModel();
           PopulateShipAndSenderDetails();
            if (resultToBeExported)
            {
               PopulateAdditionalDetailsForCsvExport();
            }
            return _msd1SummarySearchResultList;
        }

        public List<Msd2SummaryViewModel> GenerateMsd2ViewModelFilteredOnSearchCriteria(Msd2SearchInputModel searchInputModel, bool resultToBeExported)
        {
            _searchFilterDictionary.Clear();
           CreateMsd2SearchDictionary(searchInputModel);
           if (_searchFilterDictionary.Count == 0)  return new List<Msd2SummaryViewModel>();

           CreateMsd1List(_searchFilterDictionary, MSDData.MSD2);
           if (_msd2Queryable == null) return new List<Msd2SummaryViewModel>();

            AddToMsd2SummaryViewModel();
           
            return _msd2SummarySearchResultList;
        }
        
        public List<Msd3SummaryViewModel> GenerateMsd3ViewModelFilteredOnSearchCriteria(Msd3SearchInputModel searchInputModel, bool resultToBeExported)
        {
           _searchFilterDictionary.Clear();
           CreateMsd3SearchDictionary(searchInputModel);
           
           if (_searchFilterDictionary.Count == 0)  return new List<Msd3SummaryViewModel>();

           CreateMsd1List(_searchFilterDictionary, MSDData.MSD3);
           if (_msd3Queryable == null) return new List<Msd3SummaryViewModel>();

            AddToMsd3SummaryViewModel();
            return _msd3SummarySearchResultList;
        }

        #region privateMethods
        private void PopulateAdditionalDetailsForCsvExport()
        {            
            var flatFileDetail = _context.FlatFile.AsNoTracking().Where(x => _msd1SummarySearchResultList.Select(m => m.FileRefId).Contains(x.FileRefId)).Select(c => new { c.FileRefId, c.SendersRef }).ToList();
            List<Msd1SummaryViewModel> msd1AdditionalRecordsCargoSummary = new List<Msd1SummaryViewModel>();
            var cargoCategoryList = _context.CargoCategory.AsNoTracking().Select(x => x).ToList();
            var cargoData = _context.Msd1CargoSummary.AsNoTracking().Where(x => _msd1SummarySearchResultList.Any(l=> l.OurRef == x.Msd1Id)).ToList();
            
            foreach (var searchData in _msd1SummarySearchResultList)
            {               
                var senderData = flatFileDetail.FirstOrDefault(x => x.FileRefId == searchData.FileRefId);
                searchData.SendersSubmissionRef = senderData?.SendersRef;                              
                msd1AdditionalRecordsCargoSummary.AddRange( PopulateCargoDetails(searchData, cargoCategoryList, cargoData));
            }
            _msd1SummarySearchResultList.AddRange(msd1AdditionalRecordsCargoSummary);
        }

        private List<Msd1SummaryViewModel> PopulateCargoDetails(Msd1SummaryViewModel searchData, List<CargoCategory> cargoCategoryList, List<Msd1CargoSummary> cargoData)
        {
            List<Msd1SummaryViewModel> msd1CargoSummaryVM = new List<Msd1SummaryViewModel>();

            int loopCounter = 0;
            foreach (var cargoRecord in cargoData.Where(x=>x.Msd1Id==searchData.OurRef).ToList())
            {
                if (loopCounter == 0)
                {
                    searchData.CargoCategory = cargoRecord.CategoryCode;
                    searchData.UnitsWithCargo = cargoRecord.UnitsWithCargo;
                    searchData.UnitsWithoutCargo = cargoRecord.UnitsWithoutCargo;
                    searchData.TotalUnits = cargoRecord.TotalUnits;
                    searchData.GrossWeight = cargoRecord.GrossWeight;
                    searchData.Description = cargoRecord.Description;
                    searchData.CargoCategoryDescription = PopulateCargoDescription(searchData.CargoCategory, cargoCategoryList);
                }

                if (loopCounter > 0)
                {
                    var additionListItem = new Msd1SummaryViewModel();
                    additionListItem.SenderId = searchData.SenderId;
                    additionListItem.Agent = searchData.Agent;
                    additionListItem.Line = searchData.Line;
                    additionListItem.IMO = searchData.IMO;
                    additionListItem.ShipName = searchData.ShipName;
                    additionListItem.ShipType = searchData.ShipType;
                    additionListItem.Year = searchData.Year;
                    additionListItem.ReportingPort = searchData.ReportingPort;
                    additionListItem.LoadUnloadPort = searchData.LoadUnloadPort;
                    additionListItem.Direction = searchData.Direction;
                    additionListItem.NumVoyages = searchData.NumVoyages;
                    additionListItem.VoyageDate = searchData.VoyageDate;
                    additionListItem.DataSource = searchData.DataSource;
                    additionListItem.SendersRecordRef = searchData.SendersRecordRef;
                    additionListItem.SendersSubmissionRef = searchData.SendersSubmissionRef;
                    additionListItem.OurRef = searchData.OurRef;
                    additionListItem.UserName = searchData.UserName;
                    additionListItem.CreatedDate = searchData.CreatedDate;
                    additionListItem.ModifiedDate = searchData.ModifiedDate;
                    additionListItem.LastUpdatedBy = searchData.LastUpdatedBy;
                    additionListItem.DataSourceId = searchData.DataSourceId;
                    additionListItem.FileRefId = searchData.FileRefId;
                    additionListItem.Quarter = searchData.Quarter;
                    additionListItem.CallSign = searchData.CallSign;
                    additionListItem.FlagCode = searchData.FlagCode;

                    additionListItem.CargoCategory = cargoRecord.CategoryCode;
                    additionListItem.UnitsWithCargo = cargoRecord.UnitsWithCargo;
                    additionListItem.UnitsWithoutCargo = cargoRecord.UnitsWithoutCargo;
                    additionListItem.TotalUnits = cargoRecord.TotalUnits;
                    additionListItem.GrossWeight = cargoRecord.GrossWeight;
                    additionListItem.Description = cargoRecord.Description;
                    additionListItem.CargoCategoryDescription = PopulateCargoDescription(cargoRecord.CategoryCode, cargoCategoryList);
                    
                    msd1CargoSummaryVM.Add(additionListItem);
                }                                                          
                loopCounter++;
            }
            return msd1CargoSummaryVM;
        }

        private string PopulateCargoDescription(byte? cargoCategory, List<CargoCategory> cargoCategoryList)
        {
            if (cargoCategory == null) return "";

            var cargoCategotyrecord = cargoCategoryList.FirstOrDefault(c => c.CategoryCode == cargoCategory);

            if (cargoCategotyrecord == null) return "";
            else
                return cargoCategotyrecord.Description;
        }

        private void  PopulateShipAndSenderDetails()
        {
            var ships = _context.WorldFleet.AsNoTracking().Where(w => _msd1SummarySearchResultList.Select(m => m.IMO).Contains(w.Imo))
                .Select(s => new { s.ShipName, s.Imo, s.ShipTypeCode }).ToList();

            _msd1SummarySearchResultList.ForEach(m => { m.ShipName = m.ShipName ?? ships.FirstOrDefault(s => s.Imo == m.IMO)?.ShipName; m.ShipType = ships.FirstOrDefault(s => s.Imo == m.IMO)?.ShipTypeCode.ToString(); });

            var senders = _userContext.Users.AsNoTracking().Where(w => _msd1SummarySearchResultList.Select(u => u.UserName).Contains(w.Email))
                .Select(s => new { s.Email, s.SenderId }).ToList();

            _msd1SummarySearchResultList.ForEach(m => m.SenderId = senders.FirstOrDefault(s => s.Email == m.UserName)?.SenderId);
            _msd1SummarySearchResultList = _msd1SummarySearchResultList.OrderByDescending(x => x.CreatedDate).ToList();
        }

        private void CreateMsd1List(Dictionary<string, string> filterDictionary, MSDData msdDataFor)
        {
            if (_msd1Queryable == null)
            { _msd1Queryable = _context.Msd1Data.AsQueryable(); }

            if (_msd3Queryable == null)
            {
                _msd3Queryable = _context.Msd3.Include(m => m.Msd3agents).AsQueryable();
            }

            foreach (var (key, value) in filterDictionary)
            {   
                switch(msdDataFor)
                {
                    case MSDData.MSD1:
                       FilterMsd1Data(key, value);
                    break;
                    case MSDData.MSD2:
                        FilterMsd2Data(key, value);
                    break;
                    case MSDData.MSD3:                            
                        FilterMsd3Data(key, value);
                    break;
                }
            }
        }
        
        private void CreateSearchDictionary(SubmissionSearchInputModel searchModel)
        {
            if (!string.IsNullOrWhiteSpace(searchModel.RecordRef))
            {
                _searchFilterDictionary.Add("RecordRef", searchModel.RecordRef);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ShipName))
            {
                _searchFilterDictionary.Add("ShipName", searchModel.ShipName);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.SelectedCargoType))
            {
                _searchFilterDictionary.Add("CargoType", searchModel.SelectedCargoType);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.StatisticalPort))
            {
                _searchFilterDictionary.Add("StatisticalPort", searchModel.StatisticalPort);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.SenderId))
            {
                _searchFilterDictionary.Add("SenderID", searchModel.SenderId);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.SubmissionRef))
            {
                _searchFilterDictionary.Add("SubmissionRef", searchModel.SubmissionRef);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Agent))
            {
                _searchFilterDictionary.Add("Agent", searchModel.Agent);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Line))
            {
                _searchFilterDictionary.Add("Line", searchModel.Line);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ReportingPort))
            {
                _searchFilterDictionary.Add("ReportingPort", searchModel.ReportingPort);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Port))
            {
                _searchFilterDictionary.Add("Port", searchModel.Port);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.IMO))
            {
                _searchFilterDictionary.Add("IMO", searchModel.IMO);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Year))
            {
                _searchFilterDictionary.Add("Year", searchModel.Year);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Quarter))
            {
                _searchFilterDictionary.Add("Quarter", searchModel.Quarter);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.DateEntered))
            {
                _searchFilterDictionary.Add("DateEntered", searchModel.DateEntered);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Source))
            {
                _searchFilterDictionary.Add("Source", searchModel.Source);
            }
        }

        private void CreateMsd2SearchDictionary(Msd2SearchInputModel searchModel)
        {
            if (!string.IsNullOrWhiteSpace(searchModel.SenderId))
            {
                _searchFilterDictionary.Add("SenderId", searchModel.SenderId);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.ReportingPort))
            {
                _searchFilterDictionary.Add("ReportingPort", searchModel.ReportingPort);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Year))
            {
                _searchFilterDictionary.Add("Year", searchModel.Year);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Quarter))
            {
                _searchFilterDictionary.Add("Quarter", searchModel.Quarter);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.GrossWeightInward))
            {
                _searchFilterDictionary.Add("GrossWeightIn", searchModel.GrossWeightInward);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.TotalUnitInward))
            {
                _searchFilterDictionary.Add("TotalUnitIn", searchModel.TotalUnitInward);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PassengerVehiclesInward))
            {
                _searchFilterDictionary.Add("PassengerVehiclesIn", searchModel.PassengerVehiclesInward);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.GrossWeightOutward))
            {
                _searchFilterDictionary.Add("GrossWeightOut", searchModel.GrossWeightOutward);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.TotalUnitOutward))
            {
                _searchFilterDictionary.Add("TotalUnitOut", searchModel.TotalUnitOutward);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PassengerVehiclesOutward))
            {
                _searchFilterDictionary.Add("PassengerVehiclesOut", searchModel.PassengerVehiclesOutward);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.DateEntered))
            {
                _searchFilterDictionary.Add("DateEntered", searchModel.DateEntered);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.LastUpdatedBy))
            {
                _searchFilterDictionary.Add("LastUpdatedBy", searchModel.LastUpdatedBy);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.FileRefId))
            {
                _searchFilterDictionary.Add("FileRefId", searchModel.FileRefId);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Source))
            {
                _searchFilterDictionary.Add("Source", searchModel.Source);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.StatisticalPort))
            {
                _searchFilterDictionary.Add("StatisticalPort", searchModel.StatisticalPort);
            }

        }

        private void CreateMsd3SearchDictionary(Msd3SearchInputModel searchModel)
        {
            if (!string.IsNullOrWhiteSpace(searchModel.SenderId))
            {
                _searchFilterDictionary.Add("SenderId", searchModel.SenderId);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.UniqueRef))
            {
                _searchFilterDictionary.Add("UniqueRef", searchModel.UniqueRef);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.ReportingPort))
            {
                _searchFilterDictionary.Add("ReportingPort", searchModel.ReportingPort);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Year))
            {
                _searchFilterDictionary.Add("Year", searchModel.Year);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Quarter))
            {
                _searchFilterDictionary.Add("Quarter", searchModel.Quarter);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.DateEntered))
            {
                _searchFilterDictionary.Add("DateEntered", searchModel.DateEntered);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Agent))
            {
                _searchFilterDictionary.Add("Agent", searchModel.Agent);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.LastUpdatedBy))
            {
                _searchFilterDictionary.Add("LastUpdatedBy", searchModel.LastUpdatedBy);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Source))
            {
                _searchFilterDictionary.Add("Source", searchModel.Source);
            }
        }

        private void FilterMsd1Data(string key, string val)
        {
            switch (key)
            {
                case "SenderID":
                      FilterBySenderId(val);
                    break;
                case "RecordRef":
                     _msd1Queryable =  _msd1Queryable.Where(x => x.RecordRef == val).AsNoTracking();

                    break;
                case "Agent":
                    _msd1Queryable = val.Length <= 3 ? _msd1Queryable.Where(x => x.AgentSenderId.StartsWith(val)).AsNoTracking() : _msd1Queryable.Where(x => x.AgentSenderId == val).AsNoTracking();
                    break;
                case "Line":
                    _msd1Queryable = _msd1Queryable.Where(x => x.LineSenderId == val).AsNoTracking();
                    break;
                case "ReportingPort":
                    _msd1Queryable = _msd1Queryable.Where(x => x.ReportingPort == val).AsNoTracking();
                    break;
                case "Port":
                    _msd1Queryable = _msd1Queryable.Where(x => x.AssociatedPort == val).AsNoTracking();
                    break;
                case "IMO":
                    _msd1Queryable = _msd1Queryable.Where(x => x.Imo == uint.Parse(val)).AsNoTracking();
                    break;
                case "Year":
                    _msd1Queryable = _msd1Queryable.Where(x => x.Year == uint.Parse(val)).AsNoTracking();
                    break;
                case "Quarter":
                    _msd1Queryable = _msd1Queryable.Where(x => x.Quarter == ushort.Parse(val)).AsNoTracking();
                    break;
                case "DateEntered":
                 DateTime formattedDate;
                   if (!DateTime.TryParse(val, out formattedDate))
                   {
                        val = FormatDate(val);
                   }
                 _msd1Queryable = _msd1Queryable.Where(x => x.CreatedDate.Date == Convert.ToDateTime(val)).AsNoTracking();
                 break;
                case "Source":
                    _msd1Queryable = _msd1Queryable.Where(x => x.DataSourceId == uint.Parse(val)).AsNoTracking();
                    break;
                case "StatisticalPort":
                    FilterByStatisticalPort(val, MSDData.MSD1);
                    break;
                case "CargoType":
                    FilterByCargoType(val);
                    break;
                case "ShipName":
                    FilterByShipName(val);
                    break;
                case "SubmissionRef":
                    _msd1Queryable = _msd1Queryable.Where(x => x.Msd1Id == val).AsNoTracking();
                    break;
            }
        }
        
        private void FilterMsd2Data(string key, string val)
        {
            if (_msd2Queryable == null)
            { _msd2Queryable = _context.Msd2.Select(x => x); }
            switch (key)
            {
                case "FileRefId":
                    _msd2Queryable = _msd2Queryable.Where(x => x.FileRefId == int.Parse(val));
                    break;
                case "Quarter":
                    _msd2Queryable = _msd2Queryable.Where(x => x.Quarter == int.Parse(val));
                    break;
                case "Year":
                    _msd2Queryable = _msd2Queryable.Where(x => x.Year == int.Parse(val));
                    break;
                case "SenderId":
                    _msd2Queryable = _msd2Queryable.Where(x => x.SenderId.ToUpper().StartsWith(val.ToUpper()));
                    break;
                case "ReportingPort":
                    _msd2Queryable = _msd2Queryable.Where(x => x.ReportingPort.ToUpper().StartsWith(val.ToUpper()));
                    break;
                case "GrossWeightIn":
                    _msd2Queryable = _msd2Queryable.Where(x => x.GrossWeightInward == decimal.Parse(val));
                    break;
                case "TotalUnitIn":
                    _msd2Queryable = _msd2Queryable.Where(x => x.TotalUnitsInward == int.Parse(val));
                        break;
                case "PassengerVehiclesIn":
                    _msd2Queryable = _msd2Queryable.Where(x => x.PassengerVehiclesInward == int.Parse(val));
                    break;
                case "GrossWeightOut":
                    _msd2Queryable = _msd2Queryable.Where(x => x.GrossWeightOutward == decimal.Parse(val));
                    break;
                case "TotalUnitOut":
                    _msd2Queryable = _msd2Queryable.Where(x => x.TotalUnitsOutward == int.Parse(val));
                    break;
                case "PassengerVehiclesOut":
                    _msd2Queryable = _msd2Queryable.Where(x => x.PassengerVehiclesOutward == int.Parse(val));
                    break;
                case "DateEntered":
                   DateTime formattedDate;
                   if (!DateTime.TryParse(val, out formattedDate))
                   {
                        val = FormatDate(val);
                   }
                   _msd2Queryable = _msd2Queryable.Where(x => x.CreatedDate.Date == Convert.ToDateTime(val));
                   break;
                case "LastUpdatedBy":
                    _msd2Queryable = _msd2Queryable.Where(x => x.LastUpdatedBy.ToUpper().StartsWith(val.ToUpper()));
                    break;
                case "Source":
                    _msd2Queryable = _msd2Queryable.Where(x => x.DataSourceId == int.Parse(val));
                    break;
                case "StatisticalPort":
                     FilterByStatisticalPort(val, MSDData.MSD2);
                    break;
            }
        }

        private void FilterMsd3Data(string key, string val)
        {
            switch (key)
            {
                case "SenderId":
                    _msd3Queryable = _msd3Queryable.Where(x => x.SenderId == val);
                    break;
                case "ReportingPort":
                    _msd3Queryable = _msd3Queryable.Where(x => x.ReportingPort.ToUpper().StartsWith(val.ToUpper()));
                    break;
                case "Quarter":
                    _msd3Queryable = _msd3Queryable.Where(x => x.Quarter == int.Parse(val));
                    break;
                case "Year":
                    _msd3Queryable = _msd3Queryable.Where(x => x.Year == int.Parse(val));
                    break;
                case "DateEntered":
                    DateTime formattedDate;
                    if (!DateTime.TryParse(val, out formattedDate))
                    {
                            val = FormatDate(val);
                    }
                    _msd3Queryable = _msd3Queryable.Where(x => x.CreatedDate.Date == Convert.ToDateTime(val));
                    break;        
                case "LastUpdatedBy":
                    _msd3Queryable = _msd3Queryable.Where(x => x.LastUpdatedBy.ToUpper().StartsWith(val.ToUpper()));
                    break;
                case "Source":
                    _msd3Queryable = _msd3Queryable.Where(x => x.DataSourceId == int.Parse(val));
                    break;
                case "UniqueRef":
                    _msd3Queryable = _msd3Queryable.Where(x => x.Id.StartsWith(val.ToUpper()));
                    break;
                case "Agent":
                    _msd3Queryable = _msd3Queryable.Where(x => x.Msd3agents.Any(a => a.SenderId.Trim() == val.Trim()));
                    break;
            }
        }
                        

        private string FormatDate(string unformattedDate){
            var day = unformattedDate.Split("/")[0];
            var month = unformattedDate.Split("/")[1];
            var year = unformattedDate.Split("/")[2];
            var formattedDate = year + "/" + month + "/" + day;
                return formattedDate;
        }

        private void FilterByShipName(string val)
        {
            var shipDetails = _context.WorldFleet.Where(x => x.ShipName == val).Select(x => x.Imo);
            _msd1Queryable = _msd1Queryable == null ? _context.Msd1Data.Where(x => shipDetails.Contains(x.Imo))
                : _msd1Queryable.Where(x => shipDetails.Contains(x.Imo));
        }

        private void FilterByStatisticalPort(string statisticalPort, MSDData msdDataFor)
        {
            var portList = _context.GlobalPort.Where(x => x.StatisticalPort == statisticalPort).Select(p => p.Locode);

            switch(msdDataFor)
            {
                case MSDData.MSD1 :
                    _msd1Queryable = _msd1Queryable.Where(x => portList.Contains(x.ReportingPort));
                break;
                case MSDData.MSD2:
                    _msd2Queryable = _msd2Queryable.Where(x => portList.Contains(x.ReportingPort));
                break;
                case MSDData.MSD3:
                    _msd3Queryable = _msd3Queryable.Where(x => portList.Contains(x.ReportingPort));
                break;


            }
        }

        private void FilterByCargoType(string cargoType)
        {
            var cargoCategory = _context.CargoCategory.Where(x => x.GroupCode == int.Parse(cargoType)).Select(x => x.CategoryCode);
            var msd1CargoSummary = _context.Msd1CargoSummary.Where(x => cargoCategory.Contains(x.CategoryCode)).Select(x => x.Msd1Id);
            _msd1Queryable = _msd1Queryable == null ? _context.Msd1Data.Where(x => msd1CargoSummary.Contains(x.Msd1Id))
                : _msd1Queryable.Where(x => msd1CargoSummary.Contains(x.Msd1Id));
        }
        
        private void AddToSummaryViewModel()
        {
            foreach (var msd1Data in _msd1Queryable)
            {
                if (_msd1SummarySearchResultList.All(x => x.OurRef != msd1Data.Msd1Id))
                    _msd1SummarySearchResultList.Add(new Msd1SummaryViewModel
                    {
                        IMO = msd1Data.Imo,
                        Agent = msd1Data.AgentSenderId,
                        DataSourceId = msd1Data.DataSourceId,
                        CreatedDate = msd1Data.CreatedDate,
                        LastUpdatedBy = msd1Data.LastUpdatedBy,
                        ModifiedDate = msd1Data.ModifiedDate,
                        SenderId = msd1Data.AgentSenderId,
                        OurRef = msd1Data.Msd1Id,
                        UserName = msd1Data.UserName,
                        Direction = msd1Data.IsInbound ? "Inbound" : "Outbound",
                        VoyageDate = msd1Data.VoyageDate,
                        NumVoyages = msd1Data.NumVoyages,
                        ShipName = msd1Data.ShipName,
                        ShipType = msd1Data.ShipType,
                        Line = msd1Data.LineSenderId,
                        Year = msd1Data.Year,
                        Quarter = msd1Data.Quarter,
                        SendersRecordRef = msd1Data.RecordRef,
                        LoadUnloadPort = msd1Data.AssociatedPort,
                        ReportingPort = msd1Data.ReportingPort,
                        FileRefId = msd1Data.FileRefId,
                        CallSign = msd1Data.Callsign,
                        FlagCode = msd1Data.FlagCode
                    }
                );
            }

        }

        private void AddToMsd2SummaryViewModel()
        {
            foreach (var msd2Data in _msd2Queryable)
            {
                if (_msd2SummarySearchResultList.All(x => x.Id != msd2Data.FileRefId))
                    _msd2SummarySearchResultList.Add(new Msd2SummaryViewModel
                        {
                            Id = msd2Data.Id,
                            LastUpdatedBy = msd2Data.LastUpdatedBy,
                            Year = msd2Data.Year,
                            Quarter = msd2Data.Quarter,
                            ReportingPort = msd2Data.ReportingPort,
                            SenderId = msd2Data.SenderId,
                            GrossWeightInward = msd2Data.GrossWeightInward,
                            PassengerVehiclesInward = msd2Data.PassengerVehiclesInward,
                            TotalUnitInward = msd2Data.TotalUnitsInward,
                            GrossWeightOutward = msd2Data.GrossWeightOutward,
                            PassengerVehiclesOutward = msd2Data.PassengerVehiclesOutward,
                            TotalUnitOutward = msd2Data.TotalUnitsOutward,
                            CreatedDate = msd2Data.CreatedDate == DateTime.MinValue ? null : (DateTime?)msd2Data.CreatedDate,
                            Source = msd2Data.DataSourceId.ToString()

                    }
                    );
            }
        }
        
        private void AddToMsd3SummaryViewModel()
        {
            foreach (var msd3Data in _msd3Queryable)
            {
                if (_msd3SummarySearchResultList.All(x => x.Id != msd3Data.Id))
                    _msd3SummarySearchResultList.Add(new Msd3SummaryViewModel
                        {
                            SenderId = msd3Data.SenderId,
                            Id = msd3Data.Id,
                            ReportingPort = msd3Data.ReportingPort,
                            Year = msd3Data.Year,
                            Quarter = msd3Data.Quarter,
                            DataSourceId = msd3Data.DataSourceId,
                            LastUpdatedBy = msd3Data.LastUpdatedBy,
                            Agents = msd3Data.Msd3agents.Select(x=>x.SenderId).ToList(),
                            Source = msd3Data.DataSourceId,
                            CreatedDate = msd3Data.CreatedDate == DateTime.MinValue ? null : (DateTime?)msd3Data.CreatedDate

                        }
                    );
            }
        }

        /// <param name="inputSenderId"></param>
        private void FilterBySenderId(string inputSenderId)
        {
            var userNames = _userService.GetUsersBySenderID(inputSenderId);
            _msd1Queryable = _msd1Queryable.Where(x => userNames.Contains(x.UserName));
        }

       
        #endregion
    }

}
