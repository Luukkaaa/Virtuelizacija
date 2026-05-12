# Virtuelizacija
![image alt](https://github.com/Luukkaaa/Virtuelizacija/blob/603780b91317fd216bb217dac4e052be5805b54b/e7af9af4-2ab0-44bb-8cd3-d3775a9b1bd9.png)

### A. Komponente sistema

1. **Klijent:** Zadužen je da pronađe sve CSV fajlove u odgovarajućoj arhivi (dataset-u). Klijent čita fajl red po red (implementira se `IDisposable` za pravilno rukovanje fajlovima pomoću `StreamReader`/`FileStream`).
2. **WCF Servis (Server):** Prihvata podatke od klijenta u realnom vremenu. Njegov zadatak je da proveri ispravnost podataka (validacija), izvrši analitiku i smesti podatke na disk.
3. **Delegati i događaji:** Servis obaveštava o toku rada kroz događaje: `OnTransferStarted`, `OnSampleReceived`, `OnTransferCompleted`, `OnWarningRaised` i specifične događaje za greške (`TemperatureSpike`, `ResistanceOutOfBounds`, `RangeMismatch`).

---

### B. Pravila protokola (Komunikacija)

Slanje podataka se odvija u jasno definisanim sesijama:

1. **Inicijalizacija (StartSession):** 
   Klijent šalje operaciju `StartSession(EisMeta)` koja sadrži meta-zaglavlje: `BatteryId` (npr. B01), `TestId` (npr. Test_1), `SoC%` iz naziva fajla, `FileName` i `TotalRows`. Server otvara ili kreira putanju na disku: `Data/<BatteryId>/<TestId>/<SoC%>/session.csv`. Server vraća `ACK` (priznanje) ili `NACK` (odbijanje).

2. **Sekvencijalno slanje (PushSample):** 
   Klijent kroz `for`/`while` petlju prolazi kroz CSV i šalje po jedan red zovući `PushSample(EisSample)`. Objekat `EisSample` sadrži: `RowIndex`, `FrequencyHz`, `R_ohm`, `X_ohm`, `T_degC`, `Range_ohm`, `TimestampLocal`. Redosled slanja je strogo prirodan (kako stoji u CSV-u). Na svaki uzorak server vraća status `IN_PROGRESS` (ili `NACK` ako dođe do greške tipa `DataFormatFault` ili `ValidationFault`).

3. **Analitika i Validacija na Serveru:**
   * **Validacija:** Server proverava da li `RowIndex` raste, da li je `FrequencyHz > 0` i da li su vrednosti realne.
   * **Temperaturni skok:** Računa se razlika temperature `ΔT = T(t) - T(t - Δt)`. Ako je `|ΔT| > T_threshold` (prag iz konfiguracije), podiže se događaj za potencijalno pregrevanje.
   * **Senzor (Otpor i Opseg):** Proverava se da li je otpor van granica (`R_min` / `R_max`) i da li se opseg (`Range`) poklapa sa dozvoljenim granicama. Neispravni podaci idu u `rejects.csv`.

4. **Završetak (EndSession):** 
   Kada se fajl iscrpi, klijent zove `EndSession()`. Server zatvara fajlove na disku i klijentu vraća status `COMPLETED`.
