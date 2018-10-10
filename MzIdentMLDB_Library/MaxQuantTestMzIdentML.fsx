    
#r "System.ComponentModel.DataAnnotations.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\netstandard.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\\BioFSharp.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\\BioFSharp.IO.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\Microsoft.EntityFrameworkCore.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\Microsoft.EntityFrameworkCore.Relational.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\Microsoft.EntityFrameworkCore.Sqlite.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\Remotion.Linq.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\SQLitePCLRaw.core.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\SQLitePCLRaw.batteries_v2.dll"
#r @"..\packages\FSharp.Data.2.4.6\lib\net45\FSharp.Data.dll"
#r @"..\packages\FSharp.Data.Xsd.1.0.2\lib\net45\FSharp.Data.Xsd.dll"
#r "System.Xml.Linq.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\MzIdentMLDB_Library.dll"

open System
open System.Data
open System.Linq
open System.Collections.Generic
open FSharp.Data
open Microsoft.EntityFrameworkCore
open MzBasis.Basetypes
open MzIdentMLDataBase.DataModel
open MzIdentMLDataBase.InsertStatements.ObjectHandlers
open MzIdentMLDataBase.InsertStatements.ObjectHandlers.DBFunctions

//open MzIdentMLDataBase.XMLParsing


let fileDir = __SOURCE_DIRECTORY__
let pathDB = fileDir + "\Databases/" 
let sqliteMzIdentMLDBName = "MzIdentML1.db"

let sqliteMzIdentMLContext = 
        createMzIdentMLContext DBType.SQLite sqliteMzIdentMLDBName pathDB
sqliteMzIdentMLContext.ChangeTracker.AutoDetectChangesEnabled=false

//Using peptideID = 119; Modification-specific peptides IDs=125 & 126; 
//Oxidation (M)Sites for Modification-specific peptides ID=97; ProteinGroups ID=173;
//AllPeptides line 227574 (MOxidized) & line 616423 (unmodified); MS/MS scans MOxLine=10847, UnModID=41328,
//Ms/MS MOxID=568, UnModID=576

//Peptides ID=119; Modification-specific peptides IDs=123 & 124

let mzIdentMLDocument =
    MzIdentMLDocumentHandler.init(version="1.0", id="Test1")

let measureErrors =
    [
    MeasureParamHandler.init(
        (TermSymbol.toID MassErrorPpm)
                            )
    |> MeasureParamHandler.addFKUnit 
        (TermSymbol.toID Ppm);
    MeasureParamHandler.init(
        (TermSymbol.toID MassErrorPercent)
                            )
    |> MeasureParamHandler.addFKUnit 
        (TermSymbol.toID Percentage);
    MeasureParamHandler.init(
        (TermSymbol.toID MassErrorDa)
                            )
    |> MeasureParamHandler.addFKUnit 
        (TermSymbol.toID Dalton);
    MeasureParamHandler.init(
        (TermSymbol.toID SearchToleranceUpperLimit),
            "Mass Deviations upper limit [ppm]"
                            )
    |> MeasureParamHandler.addFKUnit 
        (TermSymbol.toID Ppm);
    MeasureParamHandler.init(
        (TermSymbol.toID SearchToleranceLowerLimit),
            "Mass Deviations lower limit [ppm]"
                            )
    |> MeasureParamHandler.addFKUnit 
        (TermSymbol.toID Ppm);
    MeasureParamHandler.init(
        (TermSymbol.toID MassDeviation),
            "Mass Deviation [ppm]"
                            )
    |> MeasureParamHandler.addFKUnit 
        (TermSymbol.toID Ppm);
    MeasureParamHandler.init(
        (TermSymbol.toID SearchToleranceUpperLimit),
            "Mass Deviations upper limit [Da]"
                            )
    |> MeasureParamHandler.addFKUnit 
        (TermSymbol.toID Dalton);
    MeasureParamHandler.init(
        (TermSymbol.toID SearchToleranceLowerLimit),
            "Mass Deviations lower limit [Da]"
                            )
    |> MeasureParamHandler.addFKUnit 
        (TermSymbol.toID Dalton);
    MeasureParamHandler.init(
        (TermSymbol.toID MassDeviation),
            "Mass Deviation [Da]"
                            )
    |> MeasureParamHandler.addFKUnit 
        (TermSymbol.toID Dalton);
    MeasureParamHandler.init(
        (TermSymbol.toID ProductIonErrorMZ),
            "Product-ion error [M/Z]"
                            )
    |> MeasureParamHandler.addFKUnit 
        (TermSymbol.toID MZ); 
    ]

let measureTypes =
    [
    MeasureParamHandler.init(
        (TermSymbol.toID ProductIonMZ),
            "Product-ion [M/Z]"
                            )
    |> MeasureParamHandler.addFKUnit 
        (TermSymbol.toID MZ);                       
    MeasureParamHandler.init(
        (TermSymbol.toID ProductIonIntensity),
            "Intensity"
                            )
    |> MeasureParamHandler.addFKUnit 
        (TermSymbol.toID NumberOfDetections);
    ]

MeasureHandler.init(measureTypes, "FragmentationTable measure-type")
|> MeasureHandler.addToContext sqliteMzIdentMLContext

MeasureHandler.init(measureErrors, "FragmentationTable measure-error")
|> MeasureHandler.addToContext sqliteMzIdentMLContext

let fileFormat1 =
    CVParamHandler.init(
        (TermSymbol.toID DataStoredInDataBase), "Data saved in DB"
                       )
    |> CVParamHandler.addToContext sqliteMzIdentMLContext

let fileFormat2 =
    CVParamHandler.init(
        (TermSymbol.toID UnknownFileType), "Unknown"
                        )
    |> CVParamHandler.addToContext sqliteMzIdentMLContext

let fileFormat3 =
    CVParamHandler.init(
        (TermSymbol.toID FASTAFormat), "FASTA"
                        )
    |> CVParamHandler.addToContext sqliteMzIdentMLContext

let databaseName =
    CVParamHandler.init(
        (TermSymbol.toID DataBaseName), "DB 1"
                        )
    |> CVParamHandler.addValue "Unknown to me at the moment"
    |> CVParamHandler.addToContext sqliteMzIdentMLContext

let searchDataBaseParam =
    SearchDatabaseParamHandler.init(
        (TermSymbol.toID DatabaseVersion)
                                   )
    |> SearchDatabaseParamHandler.addValue "1.0"

let searchDatabase =
    SearchDatabaseHandler.init(
        "local", "FASTA", "DB 1", "SearchDB"
                              )
    |> SearchDatabaseHandler.addDetail searchDataBaseParam

let dbSequenceParams =
    [
    DBSequenceParamHandler.init(
        (TermSymbol.toID TaxidNCBI)
                               )
    |> DBSequenceParamHandler.addValue "906914";
    DBSequenceParamHandler.init(
        (TermSymbol.toID ScientificName)
                               )
    |> DBSequenceParamHandler.addValue "C. reinhardtii";
    ]

let dbSequence =
    DBSequenceHandler.init("Test", "SearchDB", "DBSeq 1")
    |> DBSequenceHandler.addSequence "AAIEASFGSVDEMK"
    |> DBSequenceHandler.addLength 14
    |> DBSequenceHandler.addDetails dbSequenceParams
    |> DBSequenceHandler.addFkMzIdentMLDocument "Test1"

let searchDatabase1 =
    SearchDatabaseHandler.init(
        "local", "FASTA", "DB 1", "SearchDB 1"
                              )
    |> SearchDatabaseHandler.addDetail searchDataBaseParam

let dbSequenceParams1 =
    [
    DBSequenceParamHandler.init(
        (TermSymbol.toID TaxidNCBI)
                               )
    |> DBSequenceParamHandler.addValue "906914";
    DBSequenceParamHandler.init(
        (TermSymbol.toID ScientificName)
                               )
    |> DBSequenceParamHandler.addValue "C. reinhardtii";
    ]

let dbSequence1 =
    DBSequenceHandler.init("Cre02.g096150.t1.2", "SearchDB 1")
    |> DBSequenceHandler.addSequence "AAIEASFGSVDEMK"
    |> DBSequenceHandler.addLength 14
    |> DBSequenceHandler.addDetails dbSequenceParams1
    |> DBSequenceHandler.addFkMzIdentMLDocument "Test1"

let peptideParamUnmodified =
    [
    PeptideParamHandler.init(
        (TermSymbol.toID AminoAcidSequence)
                            )
    |> PeptideParamHandler.addValue "AAIEASFGSVDEMK";
    PeptideParamHandler.init(
        (TermSymbol.toID SequenceLength)
                            )
    |> PeptideParamHandler.addValue "14";
    PeptideParamHandler.init(
        (TermSymbol.toID NTermCleavageWindow)
                            )
    |> PeptideParamHandler.addValue "NGPNGDVKAAIEASFG";
    PeptideParamHandler.init(
        (TermSymbol.toID CTermCleavageWindow)
                            )
    |> PeptideParamHandler.addValue "FGSVDEMKAKFNAAAA";
    PeptideParamHandler.init(
        (TermSymbol.toID UniqueToGroups)
                            )
    |> PeptideParamHandler.addValue "yes";
    PeptideParamHandler.init(
        (TermSymbol.toID AmnoAcidModification)
                            )
    |> PeptideParamHandler.addValue "yes";
    PeptideParamHandler.init(
        (TermSymbol.toID Mass)
                            )
    |> PeptideParamHandler.addValue "1453.6797"
    |> PeptideParamHandler.addFKUnit
        (TermSymbol.toID KiloDalton);
    PeptideParamHandler.init(
        (TermSymbol.toID MonoIsotopicMass)
                            )
    |> PeptideParamHandler.addValue "1453.6759";
    PeptideParamHandler.init(
        (TermSymbol.toID PSMFDR)
                                        )
    |> PeptideParamHandler.addValue "0.01";
    ]

let peptideUnmodified =
    PeptideHandler.init("AAIEASFGSVDEMK", "PeptideUnmod 1")
    |> PeptideHandler.addDetails peptideParamUnmodified
    |> PeptideHandler.addFkMzIdentMLDocument "Test1"

let enzymeName =
    EnzymeNameParamHandler.init(
        (TermSymbol.toID Trypsin);
                               )
let enzyme =
    EnzymeHandler.init()
    |> EnzymeHandler.addSiteRegexc "KRC"
    |> EnzymeHandler.addEnzymeName enzymeName

let spectrumidentificationItemParamPeptideUnmodified =
    [
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermSymbol.toID PosteriorErrorProbability)
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "2.95E-39";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID AndromedaScore)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "122.97";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermSymbol.toID IdentificationType)
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "By MS/MS";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermSymbol.toID TotalXIC)
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "290760";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermSymbol.toID MSMSCount)
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "14";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermSymbol.toID MSMSScanNumber)
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "15279";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID RetentionTime)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "25.752"
    |> SpectrumIdentificationItemParamHandler.addFKUnit
        (TermSymbol.toID Minute);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID CalibratedRetentionTime)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "25.664"
    |> SpectrumIdentificationItemParamHandler.addFKUnit
        (TermSymbol.toID Minute);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID DeltaScore)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "110.81";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID DetectionType)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "MULTI";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID UncalibratedMZ)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "727.8481";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID FWHM)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "NAN";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID NumberOfDataPoints)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "27";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID ScanNumber)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "11";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID NumberOfIsotopicPeaks)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "3";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID PIF)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "0.8484042";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID MassDefect)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "-0.028955966";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID MassPrecision)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "0.771146"
    |> SpectrumIdentificationItemParamHandler.addFKUnit
        (TermSymbol.toID Ppm);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID MzDeltaScore)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "727.8458225";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID RetentionLength)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "22.688"
    |> SpectrumIdentificationItemParamHandler.addFKUnit
        (TermSymbol.toID Minute);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID RetentionTimeFWHM)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "11.349"
    |> SpectrumIdentificationItemParamHandler.addFKUnit
        (TermSymbol.toID Minute);
    ]


let valueIntensitiesUnModified =
    [177;129;154;811;571;803;959;640;34;51;68;51;85;68;135;34;51;68;434;34;164;68;68;34;34;51]

let valueMassDeviationsDaltonUnModified =
    [
    0.00357427;0.00393288;0.008461593;-0.002829286;0.004859461;-0.01884633;-0.01274995;-0.003771845;
    -0.01300172;-0.02979815;0.001000052; -0.002043101;-0.003109885;0.01681768;0.006003371;0.0058705;
    0.0009094998;-0.001203112;0.0005502771;-0.01015722;0.007309517;0.01232744; -0.003247674;-0.001948987;
    0.01491439;0.003216534
    ]

let measureFragmentsUnModified =
        "y4;y5;y6;y7;y8;y9;y10;y11;y5-NH3;y7-H2O;y7-NH3;y8-H2O;y9-H2O;y10-H2O;y11-H2O;y11-NH3;y11(2+);y12(2+);b4;b4-H2O;b5;b6;b6-H2O;b7;b7-H2O;b8"

let valueMassDeviationsPPMUnModified =
    [
    6.844385;6.330212;11.94609;-3.696733;5.325973;-18.85643;-11.91033;-3.144439;-21.51614;-39.87101;
    1.336402;-2.284314;-3.168703;15.97948; 5.081108;4.964514;1.515163;-1.831752;1.428521;-27.6607;
    16.02128;22.6914;-6.182867;-2.823197;22.18347;4.303839
    ]

let valueMassesUnModified =
    [
    522.219250635495;621.287305940905;708.314805638216;765.3475602406;912.408285409533;999.464019606072;
    1070.49503701499;1199.52865200952; 604.277691442015;747.363964421907;748.317181800882;894.404623285356;
    981.43771847911;1052.45490470087;1181.50831210729;1182.49246056335; 600.265168815957;656.809313417653;
    385.207610841753;367.207753655009;456.237965389838;543.264975874201;525.269986304516;690.347666220192;
    672.320238153757;747.363964421907
    ]

let measures =
    [
     MeasureHandler.init(
                         [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Intensity").Value], "Intensity"
                        );
     MeasureHandler.init(
                         [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Mass Deviation [Da]").Value], "Mass Deviation [Da]"
                        );
     MeasureHandler.init(
                         [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Mass Deviation [ppm]").Value], "Mass Deviation [ppm]"
                        );
     MeasureHandler.init(
                         [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Product-ion [M/Z]").Value], "Product-ion [M/Z]"
                        );
    ]
    |> (fun item -> sqliteMzIdentMLContext.AddRange (item.Cast()))

let fragmentArrayUnModified1 =
    [
     FragmentArrayHandler.init("Intensity", 177.);
     FragmentArrayHandler.init("Mass Deviation [Da]", 0.00357427);
     FragmentArrayHandler.init("Mass Deviation [ppm]", 6.844385);
     FragmentArrayHandler.init("Product-ion [M/Z]", 522.219250635495);
    ]

  
let ionTypeParamUnModified1 =
    [
    IonTypeParamHandler.init(
        (TermSymbol.toID Fragment)
                            );
    IonTypeParamHandler.init(
        (TermSymbol.toID YIon)
                            )
    |> IonTypeParamHandler.addValue "y4";
    ]

let fragmentArrayUnModified2 =
    [
    FragmentArrayHandler.init("Intensity", 129.);
    FragmentArrayHandler.init("Mass Deviation [Da]", 0.00393288);
    FragmentArrayHandler.init("Mass Deviation [ppm]", 6.330212);
    FragmentArrayHandler.init("Product-ion [M/Z]", 621.287305940905);
    ]
  
let ionTypeParamUnModified2 =
    [
    IonTypeParamHandler.init(
        (TermSymbol.toID Fragment)
                            );
    IonTypeParamHandler.init(
        (TermSymbol.toID YIon)
                            )
    |> IonTypeParamHandler.addValue "y5";
    ]

let fragmentationsUnModified =
    [
    IonTypeHandler.init(
        ionTypeParamUnModified1
                        )
    |> IonTypeHandler.addIndex (IndexHandler.init 1)
    |> IonTypeHandler.addFragmentArrays fragmentArrayUnModified1;
    IonTypeHandler.init(
        ionTypeParamUnModified2
                       )
    |> IonTypeHandler.addIndex (IndexHandler.init 2)
    |> IonTypeHandler.addFragmentArrays fragmentArrayUnModified2
    ]

let spectrumidentificationItemPeptideUnmodified =
    SpectrumIdentificationItemHandler.init(
        "PeptideUnmod 1", 2, 727.84714, true, 0
                                            )
    |> SpectrumIdentificationItemHandler.addFragmentations fragmentationsUnModified
    |> SpectrumIdentificationItemHandler.addDetails spectrumidentificationItemParamPeptideUnmodified

let modificationParamsMOxidized =
    [
    ModificationParamHandler.init(
        (TermSymbol.toID Oxidation)
                                    )
    |> ModificationParamHandler.addValue "Methionin";
    ModificationParamHandler.init(
        (TermSymbol.toID Combinatorics)
                                    )
    |> ModificationParamHandler.addValue "1";
    ModificationParamHandler.init(
        (TermSymbol.toID NeutralIonLoss)
                                    )
    |> ModificationParamHandler.addValue "None";
    ]

let modificationMOxidized =
    ModificationHandler.init(modificationParamsMOxidized, "123")
    |> ModificationHandler.addResidues "M"
    |> ModificationHandler.addLocation 13

let peptideParamMOxidized =
    [
    PeptideParamHandler.init(
        (TermSymbol.toID AminoAcidSequence)
                            )
    |> PeptideParamHandler.addValue "AAIEASFGSVDEMK";
    PeptideParamHandler.init(
        (TermSymbol.toID SequenceLength)
                            )
    |> PeptideParamHandler.addValue "14";
    PeptideParamHandler.init(
        (TermSymbol.toID NTermCleavageWindow)
                            )
    |> PeptideParamHandler.addValue "NGPNGDVKAAIEASFG";
    PeptideParamHandler.init(
        (TermSymbol.toID CTermCleavageWindow)
                            )
    |> PeptideParamHandler.addValue "FGSVDEMKAKFNAAAA";
    PeptideParamHandler.init(
        (TermSymbol.toID UniqueToGroups)
                            )
    |> PeptideParamHandler.addValue "yes";
    PeptideParamHandler.init(
        (TermSymbol.toID AmnoAcidModification)
                            )
    |> PeptideParamHandler.addValue "yes"; 
    PeptideParamHandler.init(
        (TermSymbol.toID Mass)
                            )
    |> PeptideParamHandler.addValue "1469.6761";
    PeptideParamHandler.init(
        (TermSymbol.toID MonoIsotopicMass)
                            )
    |> PeptideParamHandler.addValue "1469.6761";
    PeptideParamHandler.init(
        (TermSymbol.toID PSMFDR)
                            )
    |> PeptideParamHandler.addValue "0.01";
    ]

let peptideMOxidized =
        PeptideHandler.init("AAIEASFGSVDEM(1)K", "119MOxidized")
        |> PeptideHandler.addModification modificationMOxidized
        |> PeptideHandler.addDetails peptideParamMOxidized
        |> PeptideHandler.addFkMzIdentMLDocument "Test1"

let spectrumidentificationItemParamPeptideMOxidized =
    [
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermSymbol.toID PosteriorErrorProbability)
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "6,36E-25";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID AndromedaScore)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "111.12";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermSymbol.toID IdentificationType)
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "By MS/MS";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermSymbol.toID TotalXIC)
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "167790";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermSymbol.toID MSMSCount)
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "14";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermSymbol.toID MSMSScanNumber)
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "12427";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID RetentionTime)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "21.489"
    |> SpectrumIdentificationItemParamHandler.addFKUnit
        (TermSymbol.toID Minute);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID CalibratedRetentionTime)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "21.372"
    |> SpectrumIdentificationItemParamHandler.addFKUnit
        (TermSymbol.toID Minute);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID DeltaScore)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "100.69";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID DetectionType)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "MULTI";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID UncalibratedMZ)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "735.8439"
    |> SpectrumIdentificationItemParamHandler.addFKUnit
        (TermSymbol.toID MZ);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID FWHM)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "NAN";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID NumberOfDataPoints)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "24";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID ScanNumber)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "10";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID NumberOfIsotopicPeaks)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "4";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID PIF)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "0.8281062";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID MassDefect)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "-0.039976308";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID MassPrecision)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "1.815383"
    |> SpectrumIdentificationItemParamHandler.addFKUnit
        (TermSymbol.toID Ppm);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID MzDeltaScore)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "735.8475942";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID RetentionLength)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "20.649"
    |> SpectrumIdentificationItemParamHandler.addFKUnit
        (TermSymbol.toID Minute);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID RetentionTimeFWHM)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "7.509"
    |> SpectrumIdentificationItemParamHandler.addFKUnit
        (TermSymbol.toID Minute);
    ]

let valueIntensitiesMOxized =
    [68;150;85;204;622;418;683;548;34;102;34;34;34;17;68;204;85;85;85;34;51;17;34;17;34;34;17]

let valueMassDeviationsDaltonMOxized =
    [
    0.005498344;-0.003835145;0.005161485;0.01738624;-0.005117801;-0.002557511;0.01119273;0.0122177;
    -0.006379636;-2.509175E-05; 0.02011697;0.0001084094;0.001737067;0.003105442;0.003946549;-0.007037913;
    -0.004313142;0.007871579;-0.002417978;-0.00719715; 0.01309856;0.008905692;0.002088337;0.005445614;
    -0.00520534;-0.03854265;0.006262494
    ]

let valueMassDeviationsPPMMOxized =
    [
    12.99276;-7.125587;8.099229;24.00417;-6.54999;-2.75472;11.02267;11.24537;-15.74493;-0.03287147;
    18.82824;0.2133104;3.194662;5.105448; 5.936431;-18.27008;-9.453469;14.48927;-4.603313;-9.629913;
    17.95939;10.67329;2.558025;5.833781;-5.686024;-36.75864;5.318326
    ]

let measureFragmentsMOxidized =
    "y3;y4;y5;y6;y7;y8;y9;y10;y3-H2O;y7-H2O;y10-H2O;y9(2+);y10(2+);y11(2+);y12(2+);b4;b5;
    b6;b6-H2O;b8;b8-H2O;b9;b9-H2O;b10;b10-H2O;b11;b12"

let valueMassDeviationsMassesMOxized =
    [
    423.185298151502;538.221574671891;637.280991958579;724.30079561233;781.344763377517;928.410617004186;
    1015.42889516821;1086.46498399296; 405.186611445428;763.329105982246;1068.44652003759;508.22357377536;
    543.740502011698;608.260430184701;664.801621067737;385.215199032386; 456.249588048393;543.269431737229;
    525.269156608527;747.374378106757;729.343517707074;834.390303674119;816.386556342786;933.462177668826;
    915.462263936226;1048.53310896602;1177.53089691696
    ]

let fragmentArrayMOXidized1 =
    [
    FragmentArrayHandler.init("Intensity", 68.);
    FragmentArrayHandler.init("Mass Deviation [Da]",0.005498344);
    FragmentArrayHandler.init("Mass Deviation [ppm]", 12.99276);
    FragmentArrayHandler.init("Product-ion [M/Z]", 423.185298151502);
    ]
  
let ionTypeParamMOxidized1 =
    [
    IonTypeParamHandler.init(
        (TermSymbol.toID Fragment)
                            );
    IonTypeParamHandler.init(
        (TermSymbol.toID YIon)
                            )
    |> IonTypeParamHandler.addValue "y3";
    ]

let fragmentArrayMOXidized2 =
    [
    FragmentArrayHandler.init("Intensity", 150.);
    FragmentArrayHandler.init("Mass Deviation [Da]", -0.003835145);
    FragmentArrayHandler.init("Mass Deviation [ppm]", -7.125587);
    FragmentArrayHandler.init("Product-ion [M/Z]", 423.185298151502);
    ]
  
let ionTypeParamMOxidized2 =
    [
    IonTypeParamHandler.init(
        (TermSymbol.toID Fragment)
                            );
    IonTypeParamHandler.init(
        (TermSymbol.toID YIon)
                            )
    |> IonTypeParamHandler.addValue "y4";
    ]

let fragmentationsMOxidized =
    [
    IonTypeHandler.init(
        ionTypeParamMOxidized1
                        )
    |> IonTypeHandler.addIndex (IndexHandler.init 0)
    |> IonTypeHandler.addFragmentArrays fragmentArrayMOXidized1;
    IonTypeHandler.init(
        ionTypeParamMOxidized2
                        )
    |> IonTypeHandler.addIndex (IndexHandler.init 0)
    |> IonTypeHandler.addFragmentArrays fragmentArrayMOXidized2;
    ]

let spectrumidentificationItemPeptideMOxidized =
    SpectrumIdentificationItemHandler.init(
        "119MOxidized", 2, 735.84531, true, -1
                                            )
    |> SpectrumIdentificationItemHandler.addFragmentations fragmentationsMOxidized
    |> SpectrumIdentificationItemHandler.addDetails spectrumidentificationItemParamPeptideMOxidized
    
let peptideEvidences =
    [
    PeptideEvidenceHandler.init(
        "DBSeq 1", "PeptideUnmod 1"
                                )
    |> PeptideEvidenceHandler.addStart 106
    |> PeptideEvidenceHandler.addEnd 119
    |> PeptideEvidenceHandler.addFkMzIdentMLDocument "Test1";
    PeptideEvidenceHandler.init(
        "DBSeq 1", "119MOxidized"
                                )
    |> PeptideEvidenceHandler.addStart 106
    |> PeptideEvidenceHandler.addEnd 119
    |> PeptideEvidenceHandler.addFkMzIdentMLDocument "Test1"
    ]

let peptideHypothesisUnModified =
    PeptideHypothesisHandler.init(
        peptideEvidences.[0].ID, 
        [spectrumidentificationItemPeptideUnmodified]
                                    )

let peptideHypothesisMOxidized =
    PeptideHypothesisHandler.init(
        peptideEvidences.[1].ID, 
        [spectrumidentificationItemPeptideMOxidized]
                                    )

let proteinDetectionHypothesisParams =
    [
    ProteinDetectionHypothesisParamHandler.init(
        (TermSymbol.toID ProteinDescription)
                                                )
    |> ProteinDetectionHypothesisParamHandler.addValue 
        ">Cre02.g096150.t1.2 Mn superoxide dismutase ALS=MSD1 DBV=JGI5.5 GN=MSD1 
        OS=Chlamydomonas reinhardtii SV=2 TOU=Cre"
    ProteinDetectionHypothesisParamHandler.init(
        (TermSymbol.toID DistinctPeptideSequences)
                                                )
    |> ProteinDetectionHypothesisParamHandler.addValue "TRUE";
    ]
    
let proteinDetectionHypothesis =
    ProteinDetectionHypothesisHandler.init(
        true, "DBSeq 1", [peptideHypothesisUnModified; peptideHypothesisMOxidized], "173","Cre02.g096150.t1.2", "Cre02.g096150.t1.2"
                                            )
    |> ProteinDetectionHypothesisHandler.addDetails proteinDetectionHypothesisParams

let proteinAmbiguousGroupsParams =
    [
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID MajorProteinIDs)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "Cre02.g096150.t1.2";  
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID NumberOfProteins)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "1";
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID Peptides)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "6";
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID RazorAndUniquePeptides)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "6";
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID UniquePeptides)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "6";
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID SequenceCoverage)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "31.7"
    |> ProteinAmbiguityGroupParamHandler.addFKUnit
        (TermSymbol.toID Percentage);
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID UniqueAndRazorSequenceCoverage)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "31.7"
    |> ProteinAmbiguityGroupParamHandler.addFKUnit
        (TermSymbol.toID Percentage);
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID UniqueSequenceCoverage)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "31.7"
    |> ProteinAmbiguityGroupParamHandler.addFKUnit
        (TermSymbol.toID Percentage);
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID MolecularWeight)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "23.9"
    |> ProteinAmbiguityGroupParamHandler.addFKUnit
        (TermSymbol.toID Percentage);
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID SequenceLength)
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue "218";
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID SequenceLengths)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "218";
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID QValue)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "0";
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID TotalXIC)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "1335100";
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID LeadingProtein)
                                               )
    |> ProteinAmbiguityGroupParamHandler.addValue "Cre02.g096150.t1.2";
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID LeadingRazorProtein)
                                               )
    |> ProteinAmbiguityGroupParamHandler.addValue "Cre02.g096150.t1.2"
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID ProteinFDR)
                                        )
    |> ProteinAmbiguityGroupParamHandler.addValue "0.01";
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID SideFDR)
                                        )
    |> ProteinAmbiguityGroupParamHandler.addValue "0.01";
    ]

let proteinAmbiguousGroups =
    ProteinAmbiguityGroupHandler.init(
        [proteinDetectionHypothesis], "173"
                                        )
    |> ProteinAmbiguityGroupHandler.addDetails proteinAmbiguousGroupsParams

let spectrumIDFormat =
    CVParamHandler.init(
        (TermSymbol.toID MultipleIDs), "MultipleIDs"
                        )
    |> CVParamHandler.addToContext sqliteMzIdentMLContext

let spectradata =
    [
    SpectraDataHandler.init(
        "local", "Data saved in DB", "MultipleIDs"
                            )
    |> SpectraDataHandler.addName "20170518 TM FSconc3001";
    SpectraDataHandler.init(
        "local", "Data saved in DB", "MultipleIDs"
                            )
    |> SpectraDataHandler.addName "20170518 TM FSconc3002";
    ]

let organizations =
    [
    OrganizationHandler.init(name="TuKL")
    |> OrganizationHandler.addFKMzIdentMLDocument "Test1";
    OrganizationHandler.init(name="BioTech")
    |> OrganizationHandler.addFKMzIdentMLDocument "Test1";
    OrganizationHandler.init(name="CSB")
    |> OrganizationHandler.addFKMzIdentMLDocument "Test1";
    ]

let person =
    PersonHandler.init("MasterStudent 2")
    |> PersonHandler.addFirstName "Patrick"
    |> PersonHandler.addLastName "Blume"
    |> PersonHandler.addOrganizations organizations
    |> PersonHandler.addFKMzIdentMLDocument "Test1"

let role =
    CVParamHandler.init("MS:1001267", "Developer of DB")
    |> CVParamHandler.addToContext sqliteMzIdentMLContext

let contactRole =
    ContactRoleHandler.init("MasterStudent 2", "Developer of DB", "contactRole1" )

let softwareName =
    CVParamHandler.init((TermSymbol.toID MaxQuant), "SoftwareName 1")
    |> CVParamHandler.addToContext sqliteMzIdentMLContext

let analysisSoftware =
    AnalysisSoftwareHandler.init("SoftwareName 1", "Software 1")
    //|> AnalysisSoftwareHandler.addAnalysisSoftwareDeveloper contactRole
    //|> AnalysisSoftwareHandler.addFKAnalysisSoftwareDeveloper contactRole.ID
    |> AnalysisSoftwareHandler.addFKMzIdentMLDocument "Test1"

let searchType =
    CVParamHandler.init(
        (TermSymbol.toID MSMSSearch), "SearchType 1"
                        )
    |> CVParamHandler.addToContext sqliteMzIdentMLContext

let searchModificationParam =
    [
    SearchModificationParamHandler.init(
        (TermSymbol.toID NeutralFragmentLoss)
                            )
    |> SearchModificationParamHandler.addValue "None";
    SearchModificationParamHandler.init(
        (TermSymbol.toID Oxidation)
                            )
    |> SearchModificationParamHandler.addValue "Methionine";
    SearchModificationParamHandler.init(
        (TermSymbol.toID MSMSTolerance)
                                        )
    |> SearchModificationParamHandler.addValue "20 for FTMS"
    |> SearchModificationParamHandler.addFKUnit
        (TermSymbol.toID Ppm);
    SearchModificationParamHandler.init(
        (TermSymbol.toID TopPeakPer100Da)
                                        )
    |> SearchModificationParamHandler.addValue "12 for FTMS";
    SearchModificationParamHandler.init(
        (TermSymbol.toID MSMSDeisotoping)
                                        )
    |> SearchModificationParamHandler.addValue "TRUE for FTMS";
    SearchModificationParamHandler.init(
        (TermSymbol.toID MSMSTolerance)
                                        )
    |> SearchModificationParamHandler.addValue "0.5 for ITMS"
    |> SearchModificationParamHandler.addFKUnit
        (TermSymbol.toID Dalton);
    SearchModificationParamHandler.init(
        (TermSymbol.toID TopPeakPer100Da)
                                        )
    |> SearchModificationParamHandler.addValue "8 for ITMS";
    SearchModificationParamHandler.init(
        (TermSymbol.toID MSMSDeisotoping)
                                        )
    |> SearchModificationParamHandler.addValue "FALSE for ITMS";
    SearchModificationParamHandler.init(
        (TermSymbol.toID MSMSTolerance)
                                        )
    |> SearchModificationParamHandler.addValue "40 for TOF"
    |> SearchModificationParamHandler.addFKUnit
        (TermSymbol.toID Ppm);
    SearchModificationParamHandler.init(
        (TermSymbol.toID TopPeakPer100Da)
                                        )
    |> SearchModificationParamHandler.addValue "10 for TOF";
    SearchModificationParamHandler.init(
        (TermSymbol.toID MSMSDeisotoping)
                                        )
    |> SearchModificationParamHandler.addValue "TRUE for TOF";
    SearchModificationParamHandler.init(
        (TermSymbol.toID MSMSTolerance)
                                        )
    |> SearchModificationParamHandler.addValue "0.5 for Unknown"
    |> SearchModificationParamHandler.addFKUnit
        (TermSymbol.toID Dalton);
    SearchModificationParamHandler.init(
        (TermSymbol.toID TopPeakPer100Da)
                                        )
    |> SearchModificationParamHandler.addValue "8 for Unknown";
    SearchModificationParamHandler.init(
        (TermSymbol.toID MSMSDeisotoping)
                                        )
    |> SearchModificationParamHandler.addValue "False for Unknown";
    ]

let searchModificationParams =
    [
    SearchModificationHandler.init(
        false, 15.9949, "M", "SpectrumIdentificationProtocol 1", searchModificationParam
                                    );
    SearchModificationHandler.init(
        false, 43.0, "N-Term", "SpectrumIdentificationProtocol 1", searchModificationParam
                                    )
    ]

let threshold =
    [
    ThresholdParamHandler.init(
        (TermSymbol.toID MinScoreUnmodifiedPeptides)
                                )
    |> ThresholdParamHandler.addValue "0";
    ThresholdParamHandler.init(
        (TermSymbol.toID MinScoreModifiedPeptides)
                                )
    |> ThresholdParamHandler.addValue "40";
    ThresholdParamHandler.init(
        (TermSymbol.toID MinPeptideLength)
                                )
    |> ThresholdParamHandler.addValue "6";
    ThresholdParamHandler.init(
        (TermSymbol.toID MinDeltaScoreUnmod)
                                )
    |> ThresholdParamHandler.addValue "0";
    ThresholdParamHandler.init(
        (TermSymbol.toID MinDeltaScoreMod)
                                )
    |> ThresholdParamHandler.addValue "6";
    ThresholdParamHandler.init(
        (TermSymbol.toID MinPepUnique)
                                )
    |> ThresholdParamHandler.addValue "0";
    ]

let additionalSearchParams =
    [
    AdditionalSearchParamHandler.init(
        (TermSymbol.toID DecoyMode)
                                        )
    |> AdditionalSearchParamHandler.addValue "revert";
    AdditionalSearchParamHandler.init(
        (TermSymbol.toID Contaminants)
                                        )
    |> AdditionalSearchParamHandler.addValue "TRUE";
    ]

let fragmentTolerance =
    [
    FragmentToleranceParamHandler.init(
        (TermSymbol.toID SearchToleranceUpperLimit),
            "Mass Deviations upper limit [ppm]"
                            )
    |> FragmentToleranceParamHandler.addFKUnit 
        (TermSymbol.toID Ppm);
    FragmentToleranceParamHandler.init(
        (TermSymbol.toID SearchToleranceLowerLimit),
            "Mass Deviations lower limit [ppm]"
                            )
    |> FragmentToleranceParamHandler.addFKUnit 
        (TermSymbol.toID Ppm);
    FragmentToleranceParamHandler.init(
        (TermSymbol.toID SearchToleranceUpperLimit),
            "Mass Deviations upper limit [Da]"
                            )
    |> FragmentToleranceParamHandler.addFKUnit 
        (TermSymbol.toID Dalton);
    FragmentToleranceParamHandler.init(
        (TermSymbol.toID SearchToleranceLowerLimit),
            "Mass Deviations lower limit [Da]"
                            )
    |> FragmentToleranceParamHandler.addFKUnit 
        (TermSymbol.toID Dalton);
    ]

let analysisParams =
    [
    AnalysisParamHandler.init(
        (TermSymbol.toID PeptidesForProteinQuantification)
                                                    )
    |> AnalysisParamHandler.addValue "Razor";
    AnalysisParamHandler.init(
        (TermSymbol.toID DiscardUnmodifiedPeptide)
                                                    )
    |> AnalysisParamHandler.addValue "TRUE";
    AnalysisParamHandler.init(
        (TermSymbol.toID MinRatioCount)
                                                    )
    |> AnalysisParamHandler.addValue "2";
    AnalysisParamHandler.init(
        (TermSymbol.toID UseDeltaScores)
                                                    )
    |> AnalysisParamHandler.addValue "FALSE";
    AnalysisParamHandler.init(
        (TermSymbol.toID MaxQuant)
                                                    );
    ]

let proteinDetectionProtocol =
    ProteinDetectionProtocolHandler.init(
        [analysisSoftware], threshold, "ProteinDetectionProtocol 1"
                                        )
    |> ProteinDetectionProtocolHandler.addAnalysisParams analysisParams
    //|> ProteinDetectionProtocolHandler.addFkMzIdentMLDocument "Test1"

let spectrumIdentificationProtocol =
    SpectrumIdentificationProtocolHandler.init(
        "Software 1", "SearchType 1", threshold, "SpectrumIdentificationProtocol 1"
                                                )
    |> SpectrumIdentificationProtocolHandler.addEnzyme enzyme
    |> SpectrumIdentificationProtocolHandler.addModificationParams searchModificationParams
    |> SpectrumIdentificationProtocolHandler.addAdditionalSearchParams additionalSearchParams
    |> SpectrumIdentificationProtocolHandler.addFragmentTolerances fragmentTolerance
    |> SpectrumIdentificationProtocolHandler.addFkMzIdentMLDocument "Test1"

let spectrumIdentificationResultParamUnModified =
    [
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID BasePeakIntension)
                                                    )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID ElapsedTime)
                                                    )
    |> SpectrumIdentificationResultParamHandler.addValue "1";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID MinScanNumber)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "15077";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID MaxScanNumber)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "15329";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID PeptideScore)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "105.0287018";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID TotalXIC)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "25278";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID XICAreas)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0;3563;15589;27011;28600;27211;19809;14169;10127;6641;1399;1375;0";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID MSMSCount)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "1";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID MSMSScanNumbers)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "15159";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID MSMSIsotopIndices)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID ScanNumber)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "15159";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID IonInjectionTime)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID TotalIonCurrent)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "92047";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID FilteredPeaks)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "135";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID ParentIntensityFraction)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.8484042";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID FractionOfTotalSpectrum)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.001671316";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID BasePeakFraction)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.07051102";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID PrecursorFullScanNumbers)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "15140";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID PrecursorIntensity)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "10131";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID PrecursorApexFraction)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.905362";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID PrecursorApexOffset)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "-2";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID PrecursorApexOffsetTime)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.06315041";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID ScanEventNumber)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "19";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID ScoreDifference)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "NaN";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID NumberOfMatches)
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "26";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID IntensityCoverage)
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "0.5504808";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID PeakCoverage)
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "0.1851852";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID ETDIdentificationType)
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "Unknown";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID UniqueToProtein)
                                               )
    ]

let spectrumIdentificationResultParamMOxidized =
    [
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID BasePeakIntension)
                                                    )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID ElapsedTime)
                                                    )
    |> SpectrumIdentificationResultParamHandler.addValue "1";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID MinScanNumber)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "12116";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID MaxScanNumber)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "12347";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID PeptideScore)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "69.04515839";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID TotalXIC)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "25120";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID XICAreas)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0;3731;6755;24524;24360;18177;11825;4543;4102.5;3066;2497;0";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID MSMSCount)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "1";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID MSMSScanNumbers)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "12193";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID MSMSIsotopIndices)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID ScanNumber)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "12193";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID IonInjectionTime)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID TotalIonCurrent)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "71250";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID FilteredPeaks)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "157";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID ParentIntensityFraction)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.8028493";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID FractionOfTotalSpectrum)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.002043774";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID BasePeakFraction)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.04564729";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID PrecursorFullScanNumbers)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "12179";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID PrecursorIntensity)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "10056";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID PrecursorApexFraction)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "1";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID PrecursorApexOffset)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID PrecursorApexOffsetTime)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID ScanEventNumber)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "14";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID ScoreDifference)
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "69.045";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID NumberOfMatches)
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "27";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID IntensityCoverage)
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "0.4059861";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID PeakCoverage)
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "0.1719745";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID ETDIdentificationType)
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "Unknown";
    SpectrumIdentificationResultParamHandler.init(
        (TermSymbol.toID UniqueToProtein)
                                               )
    ]

let spectrumIdentificationResult =
    [
    SpectrumIdentificationResultHandler.init(
        spectradata.[0].ID, "568", [spectrumidentificationItemPeptideMOxidized], "SpectrumIdentificationList 1"
                                            )
    |> SpectrumIdentificationResultHandler.addDetails spectrumIdentificationResultParamMOxidized;
    SpectrumIdentificationResultHandler.init(
        spectradata.[1].ID, "576", [spectrumidentificationItemPeptideUnmodified], "SpectrumIdentificationList 1"
                                            )
    |> SpectrumIdentificationResultHandler.addDetails spectrumIdentificationResultParamUnModified
    ]
        
let spectrumIdentificationList =
    SpectrumIdentificationListHandler.init(spectrumIdentificationResult, "ProteinDetection 1", "AnalysisData 1", "SpectrumIdentificationList 1") 
    |> SpectrumIdentificationListHandler.addFragmentationTables
        [
        (MeasureHandler.tryFindByID sqliteMzIdentMLContext "FragmentationTable measure-type").Value;
        (MeasureHandler.tryFindByID sqliteMzIdentMLContext "FragmentationTable measure-error").Value;
        ]

let spectrumIdentification = 
    SpectrumIdentificationHandler.init(
        "SpectrumIdentificationList 1", "SpectrumIdentificationProtocol 1", spectradata, [searchDatabase; searchDatabase1], "SpectrumIdentification 1"
                                        )
    |>SpectrumIdentificationHandler.addFkMzIdentMLDocument "Test1"

let proteinDetectionList =
    ProteinDetectionListHandler.init("ProteinDetectlionList 1")
    |> ProteinDetectionListHandler.addProteinAmbiguityGroup proteinAmbiguousGroups

let proteinDetection =
    ProteinDetectionHandler.init(
        "ProteinDetectlionList 1", "ProteinDetectionProtocol 1", [spectrumIdentificationList], "ProteinDetection 1"
                                )
            
let analysisData =
    AnalysisDataHandler.init([spectrumIdentificationList], "AnalysisData 1")
    |> AnalysisDataHandler.addProteinDetectionList proteinDetectionList

let sourceFiles =
    [SourceFileHandler.init("local", "Unknown", name="20170518 TM FSconc3009"); SourceFileHandler.init("local", "FASTA", name="D:\Fred\FASTA\sequence\Chlamy\Chlamy_JGI5_5(Cp_Mp)TM.fasta")]

let inputs =
    InputsHandler.init(spectradata)
    |> InputsHandler.addSourceFiles sourceFiles 
    |> InputsHandler.addSearchDatabases [searchDatabase; searchDatabase1]

let provider =
    ProviderHandler.init()
    |> ProviderHandler.addContactRole contactRole
    |> ProviderHandler.addFKContactRole contactRole.ID
    |> ProviderHandler.addAnalysisSoftware analysisSoftware

let finalMzIdentMLDocument =
    mzIdentMLDocument
    |> MzIdentMLDocumentHandler.addName "Test MzIdentMLDatabase"
    |> MzIdentMLDocumentHandler.addAnalysisSoftware analysisSoftware
    |> MzIdentMLDocumentHandler.addDBSequences [dbSequence; dbSequence1]
    |> MzIdentMLDocumentHandler.addPeptides [peptideUnmodified; peptideMOxidized]
    |> MzIdentMLDocumentHandler.addPeptideEvidences peptideEvidences
    |> MzIdentMLDocumentHandler.addProteinDetectionProtocol proteinDetectionProtocol
    |> MzIdentMLDocumentHandler.addSpectrumIdentificationProtocol spectrumIdentificationProtocol
    |> MzIdentMLDocumentHandler.addSpectrumIdentification spectrumIdentification
    |> MzIdentMLDocumentHandler.addProteinDetection proteinDetection
    |> MzIdentMLDocumentHandler.addAnalysisData analysisData
    |> MzIdentMLDocumentHandler.addInputs inputs
    |> MzIdentMLDocumentHandler.addProvider provider
    |> MzIdentMLDocumentHandler.addPerson person
    |> MzIdentMLDocumentHandler.addOrganizations organizations
    |> MzIdentMLDocumentHandler.addToContext sqliteMzIdentMLContext

sqliteMzIdentMLContext.SaveChanges()
