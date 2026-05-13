using System.ServiceModel;

namespace Common
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IEisService
    {
        [OperationContract]
        ServerStatus StartSession(EisMeta meta);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        [FaultContract(typeof(DataFormatFault))]
        ServerStatus PushSample(EisSample sample);

        [OperationContract]
        ServerStatus EndSession();
    }
}
