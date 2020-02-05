
namespace PortFreight.Services.Common
{
    public static class Enums
    {
        public enum ShipListUploadOutcomes
        {
            FileTypeNotCsv,
            EmptyFile,
            IncorrectHeader,
            ErrorWhileProcessingFile,
            SavedSuccessfully,
            PotentialMissingRecords
        }

        public enum MethodResultOutcome
        {
            Success,
            Failure
        }
        public enum CrudOperation
        {
            Create,
            Read,
            Update,
            Delete
        }
    }
}
