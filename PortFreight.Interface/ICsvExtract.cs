using System.Collections.Generic;
using PortFreight.ViewModels;

namespace PortFreight.Interface
{
    public interface ICsvExtract
    {
        byte[] GenerateMsd1CsvExtract(List<Msd1SummaryViewModel> msd1SummaryViewModel);
    }
}
