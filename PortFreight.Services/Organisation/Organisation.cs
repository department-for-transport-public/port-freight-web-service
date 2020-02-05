
using System;
using System.Linq;
using PortFreight.Data;
using PortFreight.Services.Common;
using PortFreight.Services.Interface;
using PortFreight.ViewModel;
namespace PortFreight.Services.Organisation
{
    public class Organisation : IOrganisation
    {
        private PortFreightContext _context;
        private IEntityToViewModelMapper _entityToViewModelMapper;
        private IOrganisationSearch _organisationSearch;

        public Organisation(PortFreightContext context, 
                            IEntityToViewModelMapper entityToViewModelMapper,
                            IOrganisationSearch organisationSearch)
        {
            _context = context;
            _entityToViewModelMapper = entityToViewModelMapper;
            _organisationSearch = organisationSearch;
        }

        public MethodResult Create(OrganisationViewModel organisationVM)
        {
            var methodResult = new MethodResult();
            try
            {
                if (_organisationSearch.OrganisationNameDuplicate(organisationVM))
                {
                    methodResult.SuccessFaliure = Enums.MethodResultOutcome.Failure;
                    methodResult.Message = string.Format("Organisation {0} already exists, please enter non-duplicate organisation name and retry", organisationVM.OrgName);
                    return methodResult;
                }

                if (_organisationSearch.OrganisationIdDuplicate(organisationVM))
                {
                    methodResult.SuccessFaliure = Enums.MethodResultOutcome.Failure;
                    methodResult.Message = string.Format("Organisation Id {0} already exists, please enter non-duplicate organisation id and retry", organisationVM.OrgId);
                    return methodResult;
                }
                _context.OrgList.Add(_entityToViewModelMapper.MapOrganisationViewModelToEntity(organisationVM));
                _context.SaveChanges();
                methodResult.SuccessFaliure = Enums.MethodResultOutcome.Success;
            }
            catch (Exception e)
            {
                methodResult.SuccessFaliure = Enums.MethodResultOutcome.Failure;
                string errorMessage = "Error ocurred while creating organisation, please retry";
                methodResult.Message = errorMessage;
            }
            return methodResult;
        }

        public MethodResult Delete(int organisationPKId)
        {
            var methodResult = new MethodResult();
            try
            {
                var organisation =  _context.OrgList.Find(organisationPKId);
                if (organisation != null)
                {
                    _context.OrgList.Remove(organisation);
                     _context.SaveChanges();
                }                
                methodResult.SuccessFaliure = Enums.MethodResultOutcome.Success;
                methodResult.Message = "Organisation deleted successfully";
            }
            catch (Exception e)
            {
                methodResult.SuccessFaliure = Enums.MethodResultOutcome.Failure;
                methodResult.Message = "Error ocurred while deleting organisation, please retry";
            }
            return methodResult;
        }

        public MethodResult Update(OrganisationViewModel organisationVM)
        {
            var methodResult = new MethodResult();
            try
            {                
                if (_organisationSearch.OrganisationNameDuplicate(organisationVM))
                {
                    methodResult.SuccessFaliure = Enums.MethodResultOutcome.Failure;
                    methodResult.Message = string.Format("Organisation Name {0} already exists, please enter non-duplicate organisation name and retry", organisationVM.OrgName);
                    return methodResult;
                }

                if (_organisationSearch.OrganisationIdDuplicate(organisationVM))
                {
                    methodResult.SuccessFaliure = Enums.MethodResultOutcome.Failure;
                    methodResult.Message = string.Format("Organisation id {0} already exists, please enter non-duplicate organisation id and retry", organisationVM.OrgId);
                    return methodResult;
                }

                _context.OrgList.Attach(_entityToViewModelMapper.MapOrganisationViewModelToEntity(organisationVM)).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _context.SaveChanges();
                methodResult.SuccessFaliure = Enums.MethodResultOutcome.Success;
                methodResult.Message = "Organisation updated successfully";
            }
            catch (Exception e)
            {
                methodResult.SuccessFaliure = Enums.MethodResultOutcome.Failure;
                methodResult.Message = "Error ocurred while updating organisation, please retry";
            }

            return methodResult;            
        }               

        public OrganisationViewModel GetOrganisationDetail(int organisationPKId)
        {
            var organistion = _context.OrgList.FirstOrDefault(x => x.OrgPkId == organisationPKId);
            return _entityToViewModelMapper.MapToOrganisationViewModel(organistion);
        }       

    }
}
