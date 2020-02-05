using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using PortFreight.Services.Interface;
using PortFreight.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PortFreight.Services.MSD3
{
    public class Msd3AgentDataService : IMsd3AgentDataService
    {
        private PortFreightContext _context;
        private IEntityToViewModelMapper _entityToViewModelMapper;

        public UserManager<PortFreightUser> _userManager { get; }

        public Msd3AgentDataService(PortFreightContext context,
                                    IEntityToViewModelMapper entityToViewModelMapper,
                                    UserManager<PortFreightUser> userManager)
        {
            _context = context;
            _entityToViewModelMapper = entityToViewModelMapper;
            _userManager = userManager;
        }


        public MethodResult Add(Msd3AgentViewModel msd3AgentVM)
        {
            var methodResult = new MethodResult();
            try
            {
                if (_context.Msd3agents.Where(x => x.Msd3Id == msd3AgentVM.Msd3Id && x.SenderId == msd3AgentVM.SenderId).Any())
                {
                    methodResult.SuccessFaliure = Enums.MethodResultOutcome.Failure;
                    methodResult.Message = string.Format("Sender Name {0} already exists, please enter non-duplicate Sender and retry", msd3AgentVM.SenderId);
                    return methodResult;
                }
                _context.Msd3agents.Add(_entityToViewModelMapper.MapMsd3AgentViewModelToEntity(msd3AgentVM));

                _context.SaveChanges();
                methodResult.SuccessFaliure = Enums.MethodResultOutcome.Success;
                methodResult.Message = "Sender added successfully";
            }
            catch (Exception e)
            {
                methodResult.SuccessFaliure = Enums.MethodResultOutcome.Failure;
                methodResult.Message = "Error ocurred while adding Sender, please retry";
            }

            return methodResult;
        }
        
        public Msd3AgentViewModel GetMsd3AgentDetail(int msd3PKId)
        {
            var msd3Agent = _context.Msd3agents.FirstOrDefault(x => x.Id == msd3PKId);
            return _entityToViewModelMapper.MapToMsd3AgentViewModel(msd3Agent);           
        }

        public MethodResult Update(Msd3AgentViewModel msd3AgentVM, string lastUpdatedByUser)
        {
            var methodResult = new MethodResult();
            try
            {
                if (_context.Msd3agents.Where(x => x.Id != msd3AgentVM.Id && x.SenderId == msd3AgentVM.SenderId).Any())
                {
                    methodResult.SuccessFaliure = Enums.MethodResultOutcome.Failure;
                    methodResult.Message = string.Format("Sender Name {0} already exists, please enter non-duplicate Sender and retry", msd3AgentVM.SenderId);
                    return methodResult;
                }

                _context.Msd3agents.Attach(_entityToViewModelMapper.MapMsd3AgentViewModelToEntity(msd3AgentVM)).State = EntityState.Modified;

                
                _context.Attach(UpdateMsd3AuditFields(msd3AgentVM, lastUpdatedByUser)).State = EntityState.Modified;
                _context.SaveChanges();
                methodResult.SuccessFaliure = Enums.MethodResultOutcome.Success;
                methodResult.Message = "Sender updated successfully";
            }
            catch (Exception e)
            {
                methodResult.SuccessFaliure = Enums.MethodResultOutcome.Failure;
                methodResult.Message = "Error ocurred while updating Sender, please retry";
            }

            return methodResult;
        }              

        public MethodResult UpdateLastUpdatedBy(Msd3AgentViewModel msd3AgentVM, string lastUpdatedByUser)
        {
           var methodResult = new MethodResult();
            try
            {
                if (_context.Msd3agents.Where(x => x.Msd3Id == msd3AgentVM.Msd3Id && x.SenderId == msd3AgentVM.SenderId).Any())
                {
                    methodResult.SuccessFaliure = Enums.MethodResultOutcome.Failure;
                    methodResult.Message = string.Format("Sender Name {0} already exists, please enter non-duplicate Sender and retry", msd3AgentVM.SenderId);
                    return methodResult;
                }
                _context.Msd3agents.Add(_entityToViewModelMapper.MapMsd3AgentViewModelToEntity(msd3AgentVM));
                _context.Attach(UpdateMsd3AuditFields(msd3AgentVM, lastUpdatedByUser)).State = EntityState.Modified;

                  _context.SaveChanges();
                methodResult.SuccessFaliure = Enums.MethodResultOutcome.Success;
                methodResult.Message = "Sender added successfully";
            }
             catch (Exception e)
            {
                methodResult.SuccessFaliure = Enums.MethodResultOutcome.Failure;
                methodResult.Message = "Error ocurred while updating Sender, please retry";
            }

            return methodResult;
            
        }

        public MethodResult Delete(Msd3AgentViewModel msd3AgentVM, string lastUpdatedByUser)
        {
            var methodResult = new MethodResult();
            try
            {
                var msd3Agent = _context.Msd3agents.Find(msd3AgentVM.Id);
                if (msd3Agent != null)
                {
                     _context.Attach(UpdateMsd3AuditFields(msd3AgentVM, lastUpdatedByUser)).State = EntityState.Modified;
                    _context.Msd3agents.Remove(msd3Agent);
                    _context.SaveChanges();
                }
                methodResult.SuccessFaliure = Enums.MethodResultOutcome.Success;
                methodResult.Message = "Agent deleted successfully";
            }
            catch (Exception e)
            {
                methodResult.SuccessFaliure = Enums.MethodResultOutcome.Failure;
                methodResult.Message = "Error ocurred while deleting Agent, please retry";
            }
            return methodResult;
        }

        public List<Msd3AgentViewModel> GetAgentListFilteredByMsd3Id(string msd3Id)
        {
            return _context.Msd3agents.Where(x => x.Msd3Id == msd3Id).Select(x=>  new Msd3AgentViewModel { Id = x.Id, Msd3Id = x.Msd3Id, SenderId = x.SenderId }).ToList();
            
        }

        public List<string> GetShippingLineOrAgentList()
        {
            return _context.OrgList.Where(x=> x.IsAgent  || x.IsLine).Select(x=> x.OrgId).ToList();
        }

        /// <param name="msd3AgentVM"></param>
        /// <param name="lastUpdatedByUser"></param>
        private Msd3 UpdateMsd3AuditFields(Msd3AgentViewModel msd3AgentVM, string lastUpdatedByUser)
        {            
            var msd3Record = _context.Msd3.FirstOrDefault(m => m.Id == msd3AgentVM.Msd3Id);
            msd3Record.ModifiedDate = DateTime.Now;
            msd3Record.LastUpdatedBy = lastUpdatedByUser;
            return msd3Record;
        }

    }
}
