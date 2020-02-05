using System.Collections.Generic;
using PortFreight.ViewModel;

namespace PortFreight.Services.Interface
{
    public interface ICsvExtract
    {
        byte[] GenerateMsd1CsvExtract(List<Msd1SummaryViewModel> msd1SummaryViewModel);
        byte[] GenerateMsd2CsvExtract(List<Msd2SummaryViewModel> msd2SummaryViewModel);
        byte[] GenerateMsd3CsvExtract(List<Msd3SummaryViewModel> msd3SummaryViewModel);
    
    }
}
