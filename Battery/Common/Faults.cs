using System.Runtime.Serialization;


namespace Common
{
    [DataContract]
    public class ValidationFault
    {
        [DataMember] public string Message { get; set; }
    }

    [DataContract]
    public class DataFormatFault
    {
        [DataMember] public string Message { get; set; }
    }
}
