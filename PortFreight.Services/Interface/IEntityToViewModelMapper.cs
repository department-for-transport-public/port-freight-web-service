using PortFreight.Data.Entities;
using PortFreight.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace PortFreight.Services.Interface
{
    public interface IEntityToViewModelMapper
    {
        List<OrganisationViewModel>  MapToListOfOrganisationViewModel(List<OrgList> organisationList);

        OrganisationViewModel  MapToOrganisationViewModel(OrgList organisationList);

        OrgList MapOrganisationViewModelToEntity(OrganisationViewModel organisationList);

        Msd3AgentViewModel MapToMsd3AgentViewModel(Msd3agents organisationList);

        Msd3agents MapMsd3AgentViewModelToEntity(Msd3AgentViewModel organisationList);
    }
}
