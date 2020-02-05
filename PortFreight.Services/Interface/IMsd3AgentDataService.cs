using PortFreight.Services.Common;
using PortFreight.ViewModel;
using System.Collections.Generic;

namespace PortFreight.Services.Interface
{
    public interface IMsd3AgentDataService
    {
        MethodResult Add(Msd3AgentViewModel organisation);

        MethodResult Update(Msd3AgentViewModel organisation, string lastUpdatedByUser);
        MethodResult UpdateLastUpdatedBy(Msd3AgentViewModel organisation, string lastUpdatedByUser);

        MethodResult Delete(Msd3AgentViewModel organisation, string lastUpdatedByUser);

        Msd3AgentViewModel GetMsd3AgentDetail(int msd3PKId);

        List<Msd3AgentViewModel> GetAgentListFilteredByMsd3Id(string msd3Id);

        List<string> GetShippingLineOrAgentList();
    }
}
