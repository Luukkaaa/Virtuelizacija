using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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
