using PortFreight.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace PortFreight.Services.Interface
{
    public interface IOrganisationSearch
    {
        List<OrganisationViewModel> Search(string searchInput);
        bool OrganisationNameDuplicate(OrganisationViewModel organisationVM);
        bool OrganisationIdDuplicate(OrganisationViewModel organisationVM);
    }
}
