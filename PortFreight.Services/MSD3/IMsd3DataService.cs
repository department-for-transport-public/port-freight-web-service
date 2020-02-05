using PortFreight.Data.Entities;

namespace PortFreight.Services.MSD2
{
    public interface IMsd3DataService
    {
        void Add(Msd3 msd3);
        void DeleteAllPreviousMsd3Data(int fileRefId);
    }
}
