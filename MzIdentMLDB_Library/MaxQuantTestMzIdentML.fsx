    
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

//open MzIdentMLDataBase.XMLParsing


let fileDir = __SOURCE_DIRECTORY__
let standardDBPathSQLiteMzIdentML = fileDir + "\Databases\MzIdentML1.db"

let sqliteMzIdentMLContext = ContextHandler.sqliteConnection standardDBPathSQLiteMzIdentML
sqliteMzIdentMLContext.ChangeTracker.AutoDetectChangesEnabled=false

//Using peptideID = 119; Modification-specific peptides IDs=125 & 126; 
//Oxidation (M)Sites for Modification-specific peptides ID=97; ProteinGroups ID=173;
//AllPeptides line 227574 (MOxidized) & line 616423 (unmodified); MS/MS scans MOxLine=10847, UnModID=41328,
//Ms/MS MOxID=568, UnModID=576

let user0 =
    TermHandler.init("User:0000000")
    |> TermHandler.addName "N-term cleavage window"
    |> TermHandler.addOntologyID
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user1 =
    TermHandler.init("User:0000001")
    |> TermHandler.addName "C-term cleavage window"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user2 =
    TermHandler.init("User:0000002")
    |> TermHandler.addName "Leading razor protein"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user3 =
    TermHandler.init("User:0000003")
    |> TermHandler.addName "MaxQuant: Unique Protein Group"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user4 =
    TermHandler.init("User:0000004")
    |> TermHandler.addName "MaxQuant: Unique Protein"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user5 =
    TermHandler.init("User:0000005")
    |> TermHandler.addName "MaxQuant: PEP"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user6 =
    TermHandler.init("User:0000006")
    |> TermHandler.addName "IdentificationType"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user7 =
    TermHandler.init("User:0000007")
    |> TermHandler.addName "AminoAcid modification"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user8 =
    TermHandler.init("User:0000008")
    |> TermHandler.addName "Charge corrected mass of the precursor ion"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user9 =
    TermHandler.init("User:0000009")
    |> TermHandler.addName "Calibrated retention time"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user10 =
    TermHandler.init("User:0000010")
    |> TermHandler.addName "Mass error"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user11 =
    TermHandler.init("User:0000011")
    |> TermHandler.addName "The intensity values of the isotopes."
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user12 =
    TermHandler.init("User:0000012")
    |> TermHandler.addName "MaxQuant: Major protein IDs"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user13 =
    TermHandler.init("User:0000013")
    |> TermHandler.addName "MaxQuant: Number of proteins"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user14 =
    TermHandler.init("User:0000014")
    |> TermHandler.addName "MaxQuant: Peptides"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user15 =
    TermHandler.init("User:0000015")
    |> TermHandler.addName "MaxQuant: Razor + unique peptides"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user16 =
    TermHandler.init("User:0000016")
    |> TermHandler.addName "MaxQuant: Unique peptides"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user17 =
    TermHandler.init("User:0000017")
    |> TermHandler.addName "MaxQuant: Unique + razor sequence coverage"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user18 =
    TermHandler.init("User:0000018")
    |> TermHandler.addName "MaxQuant: Unique sequence coverage"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user19 =
    TermHandler.init("User:0000019")
    |> TermHandler.addName "MaxQuant: SequenceLength(s)"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user20 =
    TermHandler.init("User:0000020")
    |> TermHandler.addName "Metabolic labeling N14/N15"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user21 =
    TermHandler.init("User:0000021")
    |> TermHandler.addName "DetectionType"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user22 =
    TermHandler.init("User:0000022")
    |> TermHandler.addName "MaxQuant: Uncalibrated m/z"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user23 =
    TermHandler.init("User:0000023")
    |> TermHandler.addName "MaxQuant: Number of data points"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user24 =
    TermHandler.init("User:0000024")
    |> TermHandler.addName "MaxQuant: Number of isotopic peaks"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user25 =
    TermHandler.init("User:0000025")
    |> TermHandler.addName "MaxQuant: Parent ion fraction"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user26 =
    TermHandler.init("User:0000026")
    |> TermHandler.addName "MaxQuant: Mass precision"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user27 =
    TermHandler.init("User:0000027")
    |> TermHandler.addName "Retention length (FWHM)"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user28 =
    TermHandler.init("User:0000028")
    |> TermHandler.addName "MaxQuant: Min scan number"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user29 =
    TermHandler.init("User:0000029")
    |> TermHandler.addName "MaxQuant: Max scan number"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user30 =
    TermHandler.init("User:0000030")
    |> TermHandler.addName "MaxQuant: MSMS scan numbers"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user31 =
    TermHandler.init("User:0000031")
    |> TermHandler.addName "MaxQuant: MSMS isotope indices"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user32 =
    TermHandler.init("User:0000032")
    |> TermHandler.addName "MaxQuant: Filtered peaks"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user33 =
    TermHandler.init("User:0000033")
    |> TermHandler.addName "FragmentationType"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user34 =
    TermHandler.init("User:0000034")
    |> TermHandler.addName "Parent intensity fraction"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user35 =
    TermHandler.init("User:0000035")
    |> TermHandler.addName "Fraction of total spectrum"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user36 =
    TermHandler.init("User:0000036")
    |> TermHandler.addName "Base peak fraction"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user37 =
    TermHandler.init("User:0000037")
    |> TermHandler.addName "Precursor full scan number"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user38 =
    TermHandler.init("User:0000038")
    |> TermHandler.addName "Precursor intensity"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user39 =
    TermHandler.init("User:0000039")
    |> TermHandler.addName "Precursor apex fraction"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user40 =
    TermHandler.init("User:0000040")
    |> TermHandler.addName "Precursor apex offset"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user41 =
    TermHandler.init("User:0000041")
    |> TermHandler.addName "Precursor apex offset time"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user42 =
    TermHandler.init("User:0000042")
    |> TermHandler.addName "Scan event number"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user43 =
    TermHandler.init("User:0000043")
    |> TermHandler.addName "MaxQuant: Score difference"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user44 =
    TermHandler.init("User:0000044")
    |> TermHandler.addName "MaxQuant: Combinatorics"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user45 =
    TermHandler.init("User:0000045")
    |> TermHandler.addName "MaxQuant: Matches"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user46 =
    TermHandler.init("User:0000046")
    |> TermHandler.addName "MaxQuant: Match between runs"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user47 =
    TermHandler.init("User:0000047")
    |> TermHandler.addName "MaxQuant: Number of matches"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user48 =
    TermHandler.init("User:0000048")
    |> TermHandler.addName "MaxQuant: Intensity coverage"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user49 =
    TermHandler.init("User:0000049")
    |> TermHandler.addName "MaxQuant: Peak coverage"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user50 =
    TermHandler.init("User:0000050")
    |> TermHandler.addName "MaxQuant: ETD identification type"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user51 =
    TermHandler.init("User:0000051")
    |> TermHandler.addName "Min. score unmodified peptides"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext
  
let user52 =
    TermHandler.init("User:0000052")
    |> TermHandler.addName "Min. score modified peptides"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user53 =
    TermHandler.init("User:0000053")
    |> TermHandler.addName "Min. delta score of unmodified peptides"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext 

let user54 =
    TermHandler.init("User:0000054")
    |> TermHandler.addName "Min. delta score of modified peptides"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user55 =
    TermHandler.init("User:0000055")
    |> TermHandler.addName "Min. amount unique peptide"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user56 =
    TermHandler.init("User:0000056")
    |> TermHandler.addName "Min. amount razor peptide"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user57 =
    TermHandler.init("User:0000057")
    |> TermHandler.addName "Min. amount peptide"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user58 =
    TermHandler.init("User:0000058")
    |> TermHandler.addName "MaxQuant: Decoy mode"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user59 =
    TermHandler.init("User:0000059")
    |> TermHandler.addName "MaxQuant: Special AAs"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user60 =
    TermHandler.init("User:0000060")
    |> TermHandler.addName "MaxQuant: Include contaminants"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user61 =
    TermHandler.init("User:0000061")
    |> TermHandler.addName "MaxQuant: iBAQ"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user62 =
    TermHandler.init("User:0000062")
    |> TermHandler.addName "MaxQuant: Top MS/MS peaks per 100 Dalton"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user63 =
    TermHandler.init("User:0000063")
    |> TermHandler.addName "MaxQuant: IBAQ log fit"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user64 =
    TermHandler.init("User:0000064")
    |> TermHandler.addName "MaxQuant: Protein FDR"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user65 =
    TermHandler.init("User:0000065")
    |> TermHandler.addName "MaxQuant: SiteFDR"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user66 =
    TermHandler.init("User:0000066")
    |> TermHandler.addName "MaxQuant: Use Normalized Ratios For Occupancy"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user67 =
    TermHandler.init("User:0000067")
    |> TermHandler.addName "MaxQuant: Peptides used for protein quantification"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user68 =
    TermHandler.init("User:0000068")
    |> TermHandler.addName "MaxQuant: Discard unmodified counterpart peptides"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user69 =
    TermHandler.init("User:0000069")
    |> TermHandler.addName "MaxQuant: Min. ratio count"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user70 =
    TermHandler.init("User:0000070")
    |> TermHandler.addName "MaxQuant: Use delta score"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user71 =
    TermHandler.init("User:0000071")
    |> TermHandler.addName "Data-dependt acquisition"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user72 =
    TermHandler.init("User:0000072")
    |> TermHandler.addName "razor-protein"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user73 =
    TermHandler.init("User:0000073")
    |> TermHandler.addName "razor-peptide"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user74 =
    TermHandler.init("User:0000074")
    |> TermHandler.addName "Mass-deviation"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user75 =
    TermHandler.init("User:0000075")
    |> TermHandler.addName "leading-peptide"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user76 =
    TermHandler.init("User:0000076")
    |> TermHandler.addName "unique-protein"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user77 =
    TermHandler.init("User:0000077")
    |> TermHandler.addName "unique-peptide"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user78 =
    TermHandler.init("User:0000078")
    |> TermHandler.addName "MaxQuant: Delta score"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user79 =
    TermHandler.init("User:0000079")
    |> TermHandler.addName "MaxQuant: Best andromeda score"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContext sqliteMzIdentMLContext

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
    |> DBSequenceHandler.addFkMzIdentMLDocument mzIdentMLDocument.ID

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
    |> DBSequenceHandler.addFkMzIdentMLDocument mzIdentMLDocument.ID

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
    |> PeptideHandler.addFkMzIdentMLDocument mzIdentMLDocument.ID

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
        |> PeptideHandler.addFkMzIdentMLDocument mzIdentMLDocument.ID

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
    |> PeptideEvidenceHandler.addFkMzIdentMLDocument mzIdentMLDocument.ID;
    PeptideEvidenceHandler.init(
        "DBSeq 1", "119MOxidized"
                                )
    |> PeptideEvidenceHandler.addStart 106
    |> PeptideEvidenceHandler.addEnd 119
    |> PeptideEvidenceHandler.addFkMzIdentMLDocument mzIdentMLDocument.ID
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
    OrganizationHandler.init(name="TuKL");
    OrganizationHandler.init(name="BioTech");
    OrganizationHandler.init(name="CSB");
    ]

let person =
    PersonHandler.init("MasterStudent 2")
    |> PersonHandler.addFirstName "Patrick"
    |> PersonHandler.addLastName "Blume"
    |> PersonHandler.addOrganizations organizations

let role =
    CVParamHandler.init("MS:1001267", "Developer of DB")
    |> CVParamHandler.addToContext sqliteMzIdentMLContext

let contactRole =
    ContactRoleHandler.init("MasterStudent 2", "Developer of DB")

let softwareName =
    CVParamHandler.init((TermSymbol.toID MaxQuant), "SoftwareName 1")
    |> CVParamHandler.addToContext sqliteMzIdentMLContext

let analysisSoftware =
    AnalysisSoftwareHandler.init("SoftwareName 1", "Software 1")
    |> AnalysisSoftwareHandler.addAnalysisSoftwareDeveloper contactRole

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
    |> ProteinDetectionProtocolHandler.addFkMzIdentMLDocument mzIdentMLDocument.ID

let spectrumIdentificationProtocol =
    SpectrumIdentificationProtocolHandler.init(
        "Software 1", "SearchType 1", threshold, "SpectrumIdentificationProtocol 1"
                                                )
    |> SpectrumIdentificationProtocolHandler.addEnzyme enzyme
    |> SpectrumIdentificationProtocolHandler.addModificationParams searchModificationParams
    |> SpectrumIdentificationProtocolHandler.addAdditionalSearchParams additionalSearchParams
    |> SpectrumIdentificationProtocolHandler.addFragmentTolerances fragmentTolerance
    |> SpectrumIdentificationProtocolHandler.addFkMzIdentMLDocument mzIdentMLDocument.ID

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
        "SpectrumIdentificationList 1", "SpectrumIdentificationProtocol 1", spectradata, [searchDatabase], "SpectrumIdentification 1"
                                        )
    |>SpectrumIdentificationHandler.addFkMzIdentMLDocument mzIdentMLDocument.ID

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
    |> InputsHandler.addSearchDatabase searchDatabase

let provider =
    ProviderHandler.init()
    |> ProviderHandler.addContactRole contactRole
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