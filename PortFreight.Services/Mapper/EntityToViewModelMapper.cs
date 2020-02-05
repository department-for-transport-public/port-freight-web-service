using System.Collections.Generic;
using PortFreight.Data.Entities;
using PortFreight.Services.Interface;
using PortFreight.ViewModel;

namespace PortFreight.Services.Mapper
{
    public class EntityToViewModelMapper : IEntityToViewModelMapper
    {
       
        #region publicMethods

        public List<OrganisationViewModel> MapToListOfOrganisationViewModel(List<OrgList> organisationList)
        {
            List<OrganisationViewModel> orgList = new List<OrganisationViewModel>();

            foreach (var item in organisationList)
            {
                orgList.Add(MappedOrganisation(item));
            }

            return orgList;

        }

        public OrganisationViewModel MapToOrganisationViewModel(OrgList organisation)
        {
            return MappedOrganisation(organisation);
        }
       
        public OrgList MapOrganisationViewModelToEntity(OrganisationViewModel organisationVM)
        {
            return new OrgList()
            {
                OrgPkId = organisationVM.OrgPkId,
                OrgId = organisationVM.OrgId,
                OrgName = organisationVM.OrgName,
                IsAgent = organisationVM.IsAgent,
                IsLine = organisationVM.IsLine,
                IsPort = organisationVM.IsPort,
                SubmitsMsd1 = organisationVM.submits_msd1 ,
                SubmitsMsd2 = organisationVM.submits_msd2 ,
                SubmitsMsd3 = organisationVM.submits_msd3, 
                SubmitsMsd4 = organisationVM.submits_msd4,
                SubmitsMsd5 = organisationVM.submits_msd5,
            };
        }

        public Msd3AgentViewModel MapToMsd3AgentViewModel(Msd3agents msd3Agent)
        {
            return new Msd3AgentViewModel()
            {
                Id = msd3Agent.Id,
                Msd3Id = msd3Agent.Msd3Id,
                SenderId = msd3Agent.SenderId
            };
        }

        public Msd3agents MapMsd3AgentViewModelToEntity(Msd3AgentViewModel msd3AgentVM)
        {
            return new Msd3agents()
            {
                Id = msd3AgentVM.Id,
                Msd3Id = msd3AgentVM.Msd3Id,
                SenderId = msd3AgentVM.SenderId
            };
        }
        #endregion

        #region privateMethods

        private OrganisationViewModel MappedOrganisation(OrgList organisation)
        {
            return new OrganisationViewModel()
            {
                OrgPkId = organisation.OrgPkId,
                OrgId = organisation.OrgId,
                OrgName = organisation.OrgName,
                IsAgent = organisation.IsAgent,
                IsLine = organisation.IsLine,
                IsPort = organisation.IsPort,
                submits_msd1 = organisation.SubmitsMsd1,
                submits_msd2 = organisation.SubmitsMsd2,
                submits_msd3 = organisation.SubmitsMsd3
            };
        }

       
        #endregion
    }
}
