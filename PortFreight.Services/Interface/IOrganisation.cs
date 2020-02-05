using PortFreight.Services.Common;
using PortFreight.ViewModel;

namespace PortFreight.Services.Interface
{
    public interface IOrganisation
    {
        MethodResult Create(OrganisationViewModel organisation);

       MethodResult Update(OrganisationViewModel organisation);

        MethodResult Delete(int organisationPKId);

        OrganisationViewModel GetOrganisationDetail(int organisationPKId);
    }
}
