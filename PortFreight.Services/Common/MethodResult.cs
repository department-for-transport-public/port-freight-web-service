

namespace PortFreight.Services.Common
{
    public class MethodResult
    {
        public MethodResult() {

            SuccessFaliure = Enums.MethodResultOutcome.Failure;
        }   

        public Enums.MethodResultOutcome SuccessFaliure;

        public string Message;
                
    }
}
