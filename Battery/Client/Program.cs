using System;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using Common;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Pritisni ENTER da pokreneš klijenta...");
            Console.ReadLine();

            ChannelFactory<IEisService> factory = new ChannelFactory<IEisService>("EisTcpEndpoint");
            IEisService proxy = factory.CreateChannel();

            // Podesite putanju do vašeg testnog dataset foldera
            string datasetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dataset");

            if (!Directory.Exists(datasetPath))
            {
                Console.WriteLine("Putanja ne postoji. Proverite datasetPath varijablu.");
                return;
            }

            string[] csvFiles = Directory.GetFiles(datasetPath, "*.csv", SearchOption.AllDirectories);

            foreach (string filePath in csvFiles)
            {
                // Ekstrakcija iz putanje (Pojednostavljeno - prilagodite vašoj tačnoj strukturi Regexom ili splitovanjem)
                string fileName = Path.GetFileName(filePath);
                string batteryId = "B01"; // Izvucite pravu vrednost iz putanje
                string testId = "Test_1"; // Izvucite pravu vrednost iz putanje
                string soc = fileName.Replace(".csv", "");

                EisMeta meta = new EisMeta
                {
                    BatteryId = batteryId,
                    TestId = testId,
                    SoC = soc,
                    FileName = fileName,
                    TotalRows = 28
                };

                Console.WriteLine($"Pokrećem sesiju za: {fileName}");
                var status = proxy.StartSession(meta);
                if (status != ServerStatus.ACK) continue;

                // DISPOSE PATTERN: Koristimo using blokove koji garantuju zatvaranje fajlova i memorije!
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader reader = new StreamReader(fs))
                {
                    reader.ReadLine();

                    string line;
                    int rowCount = 0;

                    // Preskakanje zaglavlja ako ga ima u CSV-u
                    // reader.ReadLine(); 

                    while ((line = reader.ReadLine()) != null && rowCount < 28)
                    {
                        try
                        {
                            string[] parts = line.Split(',');

                            EisSample sample = new EisSample
                            {
                                RowIndex = rowCount,
                                FrequencyHz = double.Parse(parts[0], CultureInfo.InvariantCulture),
                                R_ohm = double.Parse(parts[1], CultureInfo.InvariantCulture),
                                X_ohm = double.Parse(parts[2], CultureInfo.InvariantCulture),
                                T_degC = double.Parse(parts[3], CultureInfo.InvariantCulture),
                                Range_ohm = double.Parse(parts[4], CultureInfo.InvariantCulture),
                                TimestampLocal = DateTime.Now // ili parsirano iz fajla
                            };

                            var resp = proxy.PushSample(sample);
                            rowCount++;
                        }
                        catch (FaultException<ValidationFault> vf)
                        {
                            Console.WriteLine($"[Server Odbio Validacija] Red: {rowCount}. Razlog: {vf.Detail.Message}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Lokalna Greška Parsiranja] Red: {rowCount}. Razlog: {ex.Message}");
                            // Opciono: Upis u poseban nevalidni log
                        }
                    }
                }

                proxy.EndSession();
                Console.WriteLine($"Završeno slanje za {fileName}\n");
            }

            // Pravilno zatvaranje WCF kanala (Deo Dispose pattern-a za komunikaciju)
            ((IClientChannel)proxy).Close();
            factory.Close();

            Console.WriteLine("Klijent je završio rad.");
            Console.ReadLine();
        }
    }
}