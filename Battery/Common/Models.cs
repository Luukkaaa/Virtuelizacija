using System;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;

namespace Common
{
    public enum ServerStatus
    {
        ACK, NACK, IN_PROGRESS, COMPLETED
    }

    [DataContract]
    public class EisMeta
    {
        [DataMember] public string BatteryId { get; set; }
        [DataMember] public string TestId { get; set; }
        [DataMember] public string SoC { get; set; }
        [DataMember] public string FileName { get; set; }
        [DataMember] public int TotalRows { get; set; }
    }

    [DataContract]
    public class EisSample
    {
        [DataMember] public int RowIndex { get; set; }
        [DataMember] public double FrequencyHz { get; set; }
        [DataMember] public double R_ohm { get; set; }
        [DataMember] public double X_ohm { get; set; }
        [DataMember] public double T_degC { get; set; }
        [DataMember] public double Range_ohm { get; set; }
        [DataMember] public DateTime TimestampLocal { get; set; }
    }

}
