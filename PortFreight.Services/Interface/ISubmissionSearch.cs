using System.Collections.Generic;
using PortFreight.ViewModel;

namespace PortFreight.Services.Interface
{
    public interface ISubmissionSearch
    {
         List<Msd1SummaryViewModel>  GenerateMsd1ViewModelFilteredOnSearchCriteria(SubmissionSearchInputModel searchModel, bool resultToBeExported);
         List<Msd2SummaryViewModel> GenerateMsd2ViewModelFilteredOnSearchCriteria(Msd2SearchInputModel searchInputModel, bool resultToBeExported);

         List<Msd3SummaryViewModel> GenerateMsd3ViewModelFilteredOnSearchCriteria(Msd3SearchInputModel searchInputModel, bool resultToBeExported);
    }

   
}
