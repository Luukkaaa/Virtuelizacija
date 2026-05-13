using System;
using System.ServiceModel;
using Common;

namespace WCFServer
{
    // InstanceContextMode.PerSession garantuje da svaki klijent ima svoju instancu servisa (svoju sesiju)
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class EisService : IEisService
    {
        private int _lastRowIndex = -1;
        private EisMeta _currentMeta;

        public ServerStatus StartSession(EisMeta meta)
        {
            _currentMeta = meta;
            _lastRowIndex = -1; // Reset za novu sesiju
            Console.WriteLine($"[Sesija Započeta] Baterija: {meta.BatteryId}, Test: {meta.TestId}, SoC: {meta.SoC}");
            return ServerStatus.ACK;
        }

        public ServerStatus PushSample(EisSample sample)
        {
            // Validacija 1: Monotoni rast
            if (sample.RowIndex <= _lastRowIndex)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Message = "RowIndex ne raste monotono." },
                    new FaultReason("Greška u validaciji indeksa."));
            }

            // Validacija 2: Frekvencija
            if (sample.FrequencyHz <= 0)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault { Message = "Frekvencija mora biti veća od 0." },
                    new FaultReason("Greška u validaciji frekvencije."));
            }

            _lastRowIndex = sample.RowIndex;
            // Simulacija upisa na disk za KT1 (biće urađeno u KT2)
            // Console.WriteLine($"Primljen uzorak: {sample.RowIndex}, Freq: {sample.FrequencyHz}");

            return ServerStatus.IN_PROGRESS;
        }

        public ServerStatus EndSession()
        {
            Console.WriteLine($"[Sesija Završena] Fajl: {_currentMeta?.FileName}");
            return ServerStatus.COMPLETED;
        }
    }
}