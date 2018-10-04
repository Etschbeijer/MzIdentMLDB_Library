    
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
open MzQuantMLDataBase.DataModel
open MzQuantMLDataBase.InsertStatements.ObjectHandlers
open BioFSharp



let fileDir = __SOURCE_DIRECTORY__
let standardDBPathSQLiteMzQuantML = fileDir + "\Databases\MzQuantML1.db"

let sqliteMzQuantMLContext = ContextHandler.sqliteConnection standardDBPathSQLiteMzQuantML
sqliteMzQuantMLContext.ChangeTracker.AutoDetectChangesEnabled=false

sqliteMzQuantMLContext.Database.OpenConnection()

//Using peptideID = 119; Modification-specific peptides IDs=125 & 126; 
//Oxidation (M)Sites for Modification-specific peptides ID=97; ProteinGroups ID=173;
//AllPeptides line 227574 (MOxidized) & line 616423 (unmodified); MS/MS scans MOxLine=10847, UnModID=41328,
//Ms/MS MOxID=568, UnModID=576

let user0 =
    TermHandler.init("User:0000000")
    |> TermHandler.addName "N-term cleavage window"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user1 =
    TermHandler.init("User:0000001")
    |> TermHandler.addName "C-term cleavage window"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user2 =
    TermHandler.init("User:0000002")
    |> TermHandler.addName "Leading razor protein"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user3 =
    TermHandler.init("User:0000003")
    |> TermHandler.addName "MaxQuant: Unique Protein Group"
    |> TermHandler.addOntologyID  "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user4 =
    TermHandler.init("User:0000004")
    |> TermHandler.addName "MaxQuant: Unique Protein"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user5 =
    TermHandler.init("User:0000005")
    |> TermHandler.addName "MaxQuant: PEP"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user6 =
    TermHandler.init("User:0000006")
    |> TermHandler.addName "IdentificationType"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user7 =
    TermHandler.init("User:0000007")
    |> TermHandler.addName "AminoAcid modification"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user8 =
    TermHandler.init("User:0000008")
    |> TermHandler.addName "Charge corrected mass of the precursor ion"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user9 =
    TermHandler.init("User:0000009")
    |> TermHandler.addName "Calibrated retention time"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user10 =
    TermHandler.init("User:0000010")
    |> TermHandler.addName "Mass error"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user11 =
    TermHandler.init("User:0000011")
    |> TermHandler.addName "The intensity values of the isotopes."
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user12 =
    TermHandler.init("User:0000012")
    |> TermHandler.addName "MaxQuant: Major protein IDs"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user13 =
    TermHandler.init("User:0000013")
    |> TermHandler.addName "MaxQuant: Number of proteins"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user14 =
    TermHandler.init("User:0000014")
    |> TermHandler.addName "MaxQuant: Peptides"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user15 =
    TermHandler.init("User:0000015")
    |> TermHandler.addName "MaxQuant: Razor + unique peptides"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user16 =
    TermHandler.init("User:0000016")
    |> TermHandler.addName "MaxQuant: Unique peptides"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user17 =
    TermHandler.init("User:0000017")
    |> TermHandler.addName "MaxQuant: Unique + razor sequence coverage"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user18 =
    TermHandler.init("User:0000018")
    |> TermHandler.addName "MaxQuant: Unique sequence coverage"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user19 =
    TermHandler.init("User:0000019")
    |> TermHandler.addName "MaxQuant: SequenceLength(s)"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user20 =
    TermHandler.init("User:0000020")
    |> TermHandler.addName "Metabolic labeling N14/N15"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user21 =
    TermHandler.init("User:0000021")
    |> TermHandler.addName "DetectionType"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user22 =
    TermHandler.init("User:0000022")
    |> TermHandler.addName "MaxQuant: Uncalibrated m/z"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user23 =
    TermHandler.init("User:0000023")
    |> TermHandler.addName "MaxQuant: Number of data points"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user24 =
    TermHandler.init("User:0000024")
    |> TermHandler.addName "MaxQuant: Number of isotopic peaks"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user25 =
    TermHandler.init("User:0000025")
    |> TermHandler.addName "MaxQuant: Parent ion fraction"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user26 =
    TermHandler.init("User:0000026")
    |> TermHandler.addName "MaxQuant: Mass precision"
    |> TermHandler.addOntologyID "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user27 =
    TermHandler.init("User:0000027")
    |> TermHandler.addName "Retention length (FWHM)"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user28 =
    TermHandler.init("User:0000028")
    |> TermHandler.addName "MaxQuant: Min scan number"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user29 =
    TermHandler.init("User:0000029")
    |> TermHandler.addName "MaxQuant: Max scan number"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user30 =
    TermHandler.init("User:0000030")
    |> TermHandler.addName "MaxQuant: MSMS scan numbers"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user31 =
    TermHandler.init("User:0000031")
    |> TermHandler.addName "MaxQuant: MSMS isotope indices"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user32 =
    TermHandler.init("User:0000032")
    |> TermHandler.addName "MaxQuant: Filtered peaks"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user33 =
    TermHandler.init("User:0000033")
    |> TermHandler.addName "FragmentationType"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user34 =
    TermHandler.init("User:0000034")
    |> TermHandler.addName "Parent intensity fraction"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user35 =
    TermHandler.init("User:0000035")
    |> TermHandler.addName "Fraction of total spectrum"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user36 =
    TermHandler.init("User:0000036")
    |> TermHandler.addName "Base peak fraction"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user37 =
    TermHandler.init("User:0000037")
    |> TermHandler.addName "Precursor full scan number"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user38 =
    TermHandler.init("User:0000038")
    |> TermHandler.addName "Precursor intensity"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user39 =
    TermHandler.init("User:0000039")
    |> TermHandler.addName "Precursor apex fraction"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user40 =
    TermHandler.init("User:0000040")
    |> TermHandler.addName "Precursor apex offset"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user41 =
    TermHandler.init("User:0000041")
    |> TermHandler.addName "Precursor apex offset time"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user42 =
    TermHandler.init("User:0000042")
    |> TermHandler.addName "Scan event number"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user43 =
    TermHandler.init("User:0000043")
    |> TermHandler.addName "MaxQuant: Score difference"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user44 =
    TermHandler.init("User:0000044")
    |> TermHandler.addName "MaxQuant: Combinatorics"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user45 =
    TermHandler.init("User:0000045")
    |> TermHandler.addName "MaxQuant: Matches"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user46 =
    TermHandler.init("User:0000046")
    |> TermHandler.addName "MaxQuant: Match between runs"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user47 =
    TermHandler.init("User:0000047")
    |> TermHandler.addName "MaxQuant: Number of matches"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user48 =
    TermHandler.init("User:0000048")
    |> TermHandler.addName "MaxQuant: Intensity coverage"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user49 =
    TermHandler.init("User:0000049")
    |> TermHandler.addName "MaxQuant: Peak coverage"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user50 =
    TermHandler.init("User:0000050")
    |> TermHandler.addName "MaxQuant: ETD identification type"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user51 =
    TermHandler.init("User:0000051")
    |> TermHandler.addName "Min. score unmodified peptides"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext
  
let user52 =
    TermHandler.init("User:0000052")
    |> TermHandler.addName "Min. score modified peptides"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user53 =
    TermHandler.init("User:0000053")
    |> TermHandler.addName "Min. delta score of unmodified peptides"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext 

let user54 =
    TermHandler.init("User:0000054")
    |> TermHandler.addName "Min. delta score of modified peptides"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user55 =
    TermHandler.init("User:0000055")
    |> TermHandler.addName "Min. amount unique peptide"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user56 =
    TermHandler.init("User:0000056")
    |> TermHandler.addName "Min. amount razor peptide"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user57 =
    TermHandler.init("User:0000057")
    |> TermHandler.addName "Min. amount peptide"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user58 =
    TermHandler.init("User:0000058")
    |> TermHandler.addName "MaxQuant: Decoy mode"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user59 =
    TermHandler.init("User:0000059")
    |> TermHandler.addName "MaxQuant: Special AAs"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user60 =
    TermHandler.init("User:0000060")
    |> TermHandler.addName "MaxQuant: Include contaminants"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user61 =
    TermHandler.init("User:0000061")
    |> TermHandler.addName "MaxQuant: iBAQ"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user62 =
    TermHandler.init("User:0000062")
    |> TermHandler.addName "MaxQuant: Top MS/MS peaks per 100 Dalton"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user63 =
    TermHandler.init("User:0000063")
    |> TermHandler.addName "MaxQuant: IBAQ log fit"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user64 =
    TermHandler.init("User:0000064")
    |> TermHandler.addName "MaxQuant: Protein FDR"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user65 =
    TermHandler.init("User:0000065")
    |> TermHandler.addName "MaxQuant: SiteFDR"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user66 =
    TermHandler.init("User:0000066")
    |> TermHandler.addName "MaxQuant: Use Normalized Ratios For Occupancy"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user67 =
    TermHandler.init("User:0000067")
    |> TermHandler.addName "MaxQuant: Peptides used for protein quantification"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user68 =
    TermHandler.init("User:0000068")
    |> TermHandler.addName "MaxQuant: Discard unmodified counterpart peptides"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user69 =
    TermHandler.init("User:0000069")
    |> TermHandler.addName "MaxQuant: Min. ratio count"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user70 =
    TermHandler.init("User:0000070")
    |> TermHandler.addName "MaxQuant: Use delta score"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user71 =
    TermHandler.init("User:0000071")
    |> TermHandler.addName "Data-dependt acquisition"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user72 =
    TermHandler.init("User:0000072")
    |> TermHandler.addName "razor-protein"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user73 =
    TermHandler.init("User:0000073")
    |> TermHandler.addName "razor-peptide"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user74 =
    TermHandler.init("User:0000074")
    |> TermHandler.addName "Mass-deviation"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user75 =
    TermHandler.init("User:0000075")
    |> TermHandler.addName "leading-peptide"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user76 =
    TermHandler.init("User:0000076")
    |> TermHandler.addName "unique-protein"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user77 =
    TermHandler.init("User:0000077")
    |> TermHandler.addName "unique-peptide"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user78 =
    TermHandler.init("User:0000078")
    |> TermHandler.addName "MaxQuant: Delta score"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

let user79 =
    TermHandler.init("User:0000079")
    |> TermHandler.addName "MaxQuant: Best andromeda score"
    |> TermHandler.addOntologyID 
        "UserParam"
    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

#time

let analysisSoftwareParams =
    [
    SoftwareParamHandler.init(
        (TermSymbol.toID MaxQuant);
                             )
    |> SoftwareParamHandler.addFKSoftware "AnalysisSoftware 1"
    SoftwareParamHandler.init(
        (TermSymbol.toID IBAQ);
                             )
    |> SoftwareParamHandler.addFKSoftware "AnalysisSoftware 1"
    SoftwareParamHandler.init(
        (TermSymbol.toID IBAQLogFit);
                             )
    |> SoftwareParamHandler.addFKSoftware "AnalysisSoftware 1"
    SoftwareParamHandler.init(
        (TermSymbol.toID MatchBetweenRuns);
                             )
    |> SoftwareParamHandler.addFKSoftware "AnalysisSoftware 1"
    SoftwareParamHandler.init(
        (TermSymbol.toID PeptidesForProteinQuantification)
                             )
    |> SoftwareParamHandler.addValue "Razor"
    |> SoftwareParamHandler.addFKSoftware "AnalysisSoftware 1";
    SoftwareParamHandler.init(
        (TermSymbol.toID DiscardUnmodifiedPeptide)
                             )
    |> SoftwareParamHandler.addValue "TRUE"
    |> SoftwareParamHandler.addFKSoftware "AnalysisSoftware 1";
    SoftwareParamHandler.init(
        (TermSymbol.toID MinRatioCount)
                             )
    |> SoftwareParamHandler.addValue "2"
    |> SoftwareParamHandler.addFKSoftware "AnalysisSoftware 1";
    SoftwareParamHandler.init(
        (TermSymbol.toID UseDeltaScores)
                             )
    |> SoftwareParamHandler.addValue "FALSE"
    |> SoftwareParamHandler.addFKSoftware "AnalysisSoftware 1";
    SoftwareParamHandler.init(
        (TermSymbol.toID UseDeltaScores)
                             )
    |> SoftwareParamHandler.addValue "TRUE"
    |> SoftwareParamHandler.addFKSoftware "AnalysisSoftware 1";
    ]
    |> (fun item -> sqliteMzQuantMLContext.AddRange(item.Cast()))

let software =
    SoftwareHandler.init("1.0", "AnalysisSoftware 1")
    |> SoftwareHandler.addFKMzQuantMLDocument "Test1"
    |> SoftwareHandler.addToContext sqliteMzQuantMLContext

let analysisSummaries =
    [
    AnalysisSummaryHandler.init(
        (TermSymbol.toID MetabolicLabelingN14N15Quantification)
                               )
    |> AnalysisSummaryHandler.addValue "Razor"
    |> AnalysisSummaryHandler.addFKMzQuantMLDocument "Test1";
    AnalysisSummaryHandler.init(
        (TermSymbol.toID MS1LabelBasedAnalysis)
                               )
    |> AnalysisSummaryHandler.addFKMzQuantMLDocument "Test1";
    AnalysisSummaryHandler.init(
        (TermSymbol.toID MS1LabelBasedPeptideAnalysis)
                               )
    |> AnalysisSummaryHandler.addFKMzQuantMLDocument "Test1";
    AnalysisSummaryHandler.init(
        (TermSymbol.toID SpectralCountQuantification)
                               )
    |> AnalysisSummaryHandler.addFKMzQuantMLDocument "Test1";
    ]
    |> (fun item -> sqliteMzQuantMLContext.AddRange(item.Cast()))

let TSV =
    CVParamHandler.init(
        (TermSymbol.toID TSV), "TSV"
                        )
    |> CVParamHandler.addToContext sqliteMzQuantMLContext

let FASTA =
    CVParamHandler.init(
        (TermSymbol.toID FASTAFormat), "FASTA"
                        )
    |> CVParamHandler.addToContext sqliteMzQuantMLContext

let DataStoredInDataBase =
    CVParamHandler.init(
        (TermSymbol.toID DataStoredInDataBase), "DataStoredInDataBase"
                        )
    |> CVParamHandler.addToContext sqliteMzQuantMLContext

let UnknownFileType =
    CVParamHandler.init(
        (TermSymbol.toID UnknownFileType), "UnknownFileType"
                        )
    |> CVParamHandler.addToContext sqliteMzQuantMLContext

let databaseName =
    CVParamHandler.init(
        (TermSymbol.toID DataBaseName), "Name 1"
                        )
    |> CVParamHandler.addValue "Unknown to me at the moment"
    |> CVParamHandler.addToContext sqliteMzQuantMLContext

let searchDatabaseParam =
    SearchDatabaseParamHandler.init(
        (TermSymbol.toID DatabaseVersion), "DatabaseVersion"
                                   )
    |> SearchDatabaseParamHandler.addValue "1.0"
    |> SearchDatabaseParamHandler.addFKSearchDatabase "SearchDataBase 1"
    |> SearchDatabaseParamHandler.addToContext sqliteMzQuantMLContext

let searchDatabase =
    SearchDatabaseHandler.init(
        "local", "Name 1", "SearchDataBase 1"
                                )
    |> SearchDatabaseHandler.addFKFileFormat "FASTA"
    |> SearchDatabaseHandler.addFKInputFiles "InputFiles 1"
    |> SearchDatabaseHandler.addToContext sqliteMzQuantMLContext

let methodFile =
    MethodFileHandler.init("local")
    |> MethodFileHandler.addFileFormat
        (CVParamHandler.init(
            (TermSymbol.toID DataDependentAcquisition)
                            )
        )
    |> MethodFileHandler.addFKInputFiles "InputFiles 1"
    |> MethodFileHandler.addToContext sqliteMzQuantMLContext
  
let sourceFiles =
    [
    SourceFileHandler.init("local")
    |> SourceFileHandler.addName "peptides.txt"
    |> SourceFileHandler.addFKFileFormat "TSV"
    |> SourceFileHandler.addFKInputFiles "InputFiles 1";
    SourceFileHandler.init("local")
    |> SourceFileHandler.addName "proteinGroups.txt"
    |> SourceFileHandler.addFKFileFormat "TSV"
    |> SourceFileHandler.addFKInputFiles "InputFiles 1";
    SourceFileHandler.init("local")
    |> SourceFileHandler.addName "msmsScans.txt"
    |> SourceFileHandler.addFKFileFormat "TSV"
    |> SourceFileHandler.addFKInputFiles "InputFiles 1";
    ]
    |> (fun item -> sqliteMzQuantMLContext.AddRange(item.Cast()))

let rawFiles =
    [
    RawFileHandler.init("local", "RawFilesGroup 1")
    |> RawFileHandler.addName "20170518 TM FSconc3001"
    |> RawFileHandler.addFKFileFormat "UnknownFileType"
    |> RawFileHandler.addToContext sqliteMzQuantMLContext;
    RawFileHandler.init("local", "RawFilesGroup 1")
    |> RawFileHandler.addName "20170518 TM FSconc3002"
    |> RawFileHandler.addFKFileFormat "UnknownFileType"
    |> RawFileHandler.addToContext sqliteMzQuantMLContext;
    RawFileHandler.init("local", "RawFilesGroup 1")
    |> RawFileHandler.addName "D:\Fred\FASTA\sequence\Chlamy\Chlamy_JGI5_5(Cp_Mp)TM.fasta"
    |> RawFileHandler.addFKFileFormat "FASTA"
    |> RawFileHandler.addToContext sqliteMzQuantMLContext;
    ]

let identificationFile1 =
    IdentificationFileHandler.init("local", "IdentificationFile 1")
    |> IdentificationFileHandler.addName "20170518 TM FSconc3009"
    |> IdentificationFileHandler.addFKSearchDatabase "SearchDataBase 1"
    |> IdentificationFileHandler.addFKInputFiles "InputFiles 1"
    |> IdentificationFileHandler.addToContext sqliteMzQuantMLContext

let rawFilesGroup =
    RawFilesGroupHandler.init("RawFilesGroup 1")
    |> RawFilesGroupHandler.addFKInputFiles "InputFiles 1"
    |> RawFilesGroupHandler.addToContext sqliteMzQuantMLContext

let inputFiles =
    InputFilesHandler.init("InputFiles 1")
    |> InputFilesHandler.addToContext sqliteMzQuantMLContext

let cvParams =
    [
     CVParamHandler.init(
            (TermSymbol.toID Oxidation), "Oxidation"
                        );
     CVParamHandler.init(
            (TermSymbol.toID NeutralIonLoss), "Neutral-Ion-Loss"
                        )
     CVParamHandler.init(
            (TermSymbol.toID MetabolicLabelingN14N15), "MetabolicLabeling-N14N15"
                        )
    ]
    |> (fun item -> sqliteMzQuantMLContext.AddRange(item.Cast()))

let modifications =
    [
    ModificationHandler.init("Oxidation")
    |> ModificationHandler.addResidues "M"
    |> ModificationHandler.addMassDelta 16000.
    |> ModificationHandler.addFKPeptideConsensus "PeptideConsensusMOxidized 1";
    ModificationHandler.init("Neutral-Ion-Loss")     
    |> ModificationHandler.addFKPeptideConsensus "PeptideConsensusMOxidized 1";
    ]
    |> (fun item -> sqliteMzQuantMLContext.AddRange(item.Cast()))

let label =
    ModificationHandler.init("MetabolicLabeling-N14N15", "Label 1")
    |> ModificationHandler.addMassDelta 1.01
    |> ModificationHandler.addFKAssay "Assay 1"
    |> ModificationHandler.addToContext sqliteMzQuantMLContext

let peptideConsensusUnmodifiedParams =
    [
    PeptideConsensusParamHandler.init(
        (TermSymbol.toID MonoIsotopicMass)
                            )
    |> PeptideConsensusParamHandler.addValue "1453.6759"
    |> PeptideConsensusParamHandler.addFKPeptideConsensus "PeptideConsensusUnmodified 1";
    PeptideConsensusParamHandler.init(
        (TermSymbol.toID BestAndromedaScore)
                                                )
    |> PeptideConsensusParamHandler.addValue "122.97"
    |> PeptideConsensusParamHandler.addFKPeptideConsensus "PeptideConsensusUnmodified 1";
    PeptideConsensusParamHandler.init(
        (TermSymbol.toID FractionOfTotalSpectrum)
                                                )
    |> PeptideConsensusParamHandler.addValue "0.001671316"
    |> PeptideConsensusParamHandler.addFKPeptideConsensus "PeptideConsensusUnmodified 1";
    PeptideConsensusParamHandler.init(
        (TermSymbol.toID UniqueToProtein)
                                     )
    |> PeptideConsensusParamHandler.addValue "true"
    |> PeptideConsensusParamHandler.addFKPeptideConsensus "PeptideConsensusUnmodified 1";
    ]
    |> (fun item -> sqliteMzQuantMLContext.AddRange(item.Cast()))

let assay1 =
    AssayHandler.init("EvidenceRefUnmodified 1", "Assay 1")
    |> AssayHandler.addFKRawFilesGroup "RawFilesGroup 1"
    |> AssayHandler.addFKIdentificationFile "IdentificationFile 1"
    |> AssayHandler.addFkMzQuantMLDocument "Test1" 
    |> AssayHandler.addFKIdentificationFile "IdentificationFile 1"
    |> AssayHandler.addToContext sqliteMzQuantMLContext

let featureUnmodified =
    FeatureHandler.init(2, 727.84714, 25.752, "FeatureUnmodified 1")
    |> FeatureHandler.addFKSpectrum "576"
    |> FeatureHandler.addToContext sqliteMzQuantMLContext

let evidenceRefUnmodified =
    EvidenceRefHandler.init("FeatureUnmodified 1", "EvidenceRefUnmodified 1")
    |> EvidenceRefHandler.addFKIdentificationFile "IdentificationFile 1"
    |> EvidenceRefHandler.addFKPeptideConsensus "PeptideConsensusUnmodified 1"
    |> EvidenceRefHandler.addToContext sqliteMzQuantMLContext

let peptideConsensusUnmodified =
    PeptideConsensusHandler.init(2, "PeptideConsensusList 1", "PeptideConsensusUnmodified 1")
    |> PeptideConsensusHandler.addFKSearchDatabase "SearchDataBase 1"
    |> PeptideConsensusHandler.addPeptideSequence "AAIEASFGSVDEMK" 
    |> PeptideConsensusHandler.addFKProtein "ProteinUnmodified"
    |> PeptideConsensusHandler.addToContext sqliteMzQuantMLContext

let peptideConsensusMOxidizedParams =
    [
    PeptideConsensusParamHandler.init(
        (TermSymbol.toID MonoIsotopicMass)
                                                )
    |> PeptideConsensusParamHandler.addValue "1469.6761"
    |> PeptideConsensusParamHandler.addFKPeptideConsensus "PeptideConsensusMOxidized 1";
    PeptideConsensusParamHandler.init(
        (TermSymbol.toID BestAndromedaScore)
                                                )
    |> PeptideConsensusParamHandler.addValue "111.12"
    |> PeptideConsensusParamHandler.addFKPeptideConsensus "PeptideConsensusMOxidized 1";
    PeptideConsensusParamHandler.init(
        (TermSymbol.toID FractionOfTotalSpectrum)
                                                )
    |> PeptideConsensusParamHandler.addValue "0.002043774"
    |> PeptideConsensusParamHandler.addFKPeptideConsensus "PeptideConsensusMOxidized 1";
    PeptideConsensusParamHandler.init(
        (TermSymbol.toID UniqueToProtein)
                                               )
    ]
    |> (fun item -> sqliteMzQuantMLContext.AddRange(item.Cast()))

let assay2 =
    AssayHandler.init("FeatureMOxidized 1", "Assay 2")
    |> AssayHandler.addFKRawFilesGroup "RawFilesGroup 1"
    |> AssayHandler.addFKIdentificationFile "IdentificationFile 1"
    |> AssayHandler.addFkMzQuantMLDocument "Test1"
    |> AssayHandler.addFKIdentificationFile "IdentificationFile 1"
    |> AssayHandler.addToContext sqliteMzQuantMLContext

let featureMOxidized =
    FeatureHandler.init(2, 735.84531, 21.489, "FeatureMOxidized 1")
    |> FeatureHandler.addFKSpectrum "576"
    |> FeatureHandler.addToContext sqliteMzQuantMLContext

let evidenceRefMOxidized =
    EvidenceRefHandler.init("FeatureMOxidized 1")
    |> EvidenceRefHandler.addFKIdentificationFile "IdentificationFile 1"
    |> EvidenceRefHandler.addFKPeptideConsensus "PeptideConsensusMOxidized 1"
    |> EvidenceRefHandler.addToContext sqliteMzQuantMLContext

let peptideConsensusMOxidized =
    PeptideConsensusHandler.init(2, "PeptideConsensusList 1", "PeptideConsensusMOxidized 1")
    |> PeptideConsensusHandler.addPeptideSequence "AAIEASFGSVDEMK"
    |> PeptideConsensusHandler.addFKSearchDatabase "SearchDataBase 1"
    |> PeptideConsensusHandler.addFKProtein "ProteinMOxidized"
    |> PeptideConsensusHandler.addToContext sqliteMzQuantMLContext

let peptideConsensusList =
    PeptideConsensusListHandler.init(true, "PeptideConsensusList 1")
    |> PeptideConsensusListHandler.addFkMzQuantMLDocument "Test1"
    |> PeptideConsensusListHandler.addToContext sqliteMzQuantMLContext

let proteinParams1 =
    [
    ProteinParamHandler.init(
        (TermSymbol.toID SequenceCoverage)
                            )
    |> ProteinParamHandler.addValue "31.7"
    |> ProteinParamHandler.addFkUnit
        (TermSymbol.toID Percentage)
    |> ProteinParamHandler.addFKProtein "ProteinUnmodified";
    ProteinParamHandler.init(
        (TermSymbol.toID ProteinScore)
                                          )
    |> ProteinParamHandler.addValue "105.09"
    |> ProteinParamHandler.addFKProtein "ProteinUnmodified";
    ProteinParamHandler.init(
        (TermSymbol.toID PeptideCountsAll)
                                            )
    |> ProteinParamHandler.addValue "6"
    |> ProteinParamHandler.addFKProtein "ProteinUnmodified";
    ProteinParamHandler.init(
        (TermSymbol.toID PeptideCountsRazorAndUnique)
                                            )
    |> ProteinParamHandler.addValue "6"
    |> ProteinParamHandler.addFKProtein "ProteinUnmodified";
    ProteinParamHandler.init(
        (TermSymbol.toID PeptideCountsRazorAndUnique)
                                            )
    |> ProteinParamHandler.addValue "6"
    |> ProteinParamHandler.addFKProtein "ProteinUnmodified";
    ProteinParamHandler.init(
        (TermSymbol.toID PeptideCountUnique)
                                            )
    |> ProteinParamHandler.addValue "6"
    |> ProteinParamHandler.addFKProtein "ProteinUnmodified";
    ProteinParamHandler.init(
        (TermSymbol.toID ProteinDescription)
                                            )
    |> ProteinParamHandler.addValue ">Cre02.g096150.t1.2 Mn superoxide dismutase ALS=MSD1 DBV=JGI5.5 GN=MSD1 OS=Chlamydomonas reinhardtii SV=2 TOU=Cre";
    ProteinParamHandler.init(
            (TermSymbol.toID MetabolicLabelingN14N15)
                            )
    |> ProteinParamHandler.addFKProtein "ProteinUnmodified";
    ProteinParamHandler.init(
            (TermSymbol.toID ProteinModifications)
                            )
    |> ProteinParamHandler.addValue "Methionine oxidation";
    ProteinParamHandler.init(
            (TermSymbol.toID ProteinAmbiguityGroupMembers)
                            )
    |> ProteinParamHandler.addFKProtein "ProteinUnmodified";
    ]
    |> (fun item -> sqliteMzQuantMLContext.AddRange(item.Cast()))

let proteinParams2 =
    [
    ProteinParamHandler.init(
        (TermSymbol.toID SequenceCoverage)
                            )
    |> ProteinParamHandler.addValue "31.7"
    |> ProteinParamHandler.addFkUnit
        (TermSymbol.toID Percentage)
    |> ProteinParamHandler.addFKProtein "ProteinMOxidized";
    ProteinParamHandler.init(
        (TermSymbol.toID ProteinScore)
                                          )
    |> ProteinParamHandler.addValue "105.09"
    |> ProteinParamHandler.addFKProtein "ProteinMOxidized";
    ProteinParamHandler.init(
        (TermSymbol.toID DistinctPeptideSequences)
                                            )
    |> ProteinParamHandler.addValue "6"
    |> ProteinParamHandler.addFKProtein "ProteinMOxidized";
    ProteinParamHandler.init(
        (TermSymbol.toID PeptideCountUnique)
                                            )
    |> ProteinParamHandler.addValue "6"
    |> ProteinParamHandler.addFKProtein "ProteinMOxidized";
    ProteinParamHandler.init(
        (TermSymbol.toID ProteinDescription)
                                            )
    |> ProteinParamHandler.addValue ">Cre02.g096150.t1.2 Mn superoxide dismutase ALS=MSD1 DBV=JGI5.5 GN=MSD1 OS=Chlamydomonas reinhardtii SV=2 TOU=Cre";
    ProteinParamHandler.init(
            (TermSymbol.toID MetabolicLabelingN14N15)
                            )
    |> ProteinParamHandler.addFKProtein "ProteinMOxidized";
    ]
    |> (fun item -> sqliteMzQuantMLContext.AddRange(item.Cast()))

let proteins =
    [
    ProteinHandler.init(
        "Cre02.g096150.t1.2", "SearchDataBase 1", "ProteinList", "ProteinUnmodified"
                       )
    ProteinHandler.init(
        "Cre02.g096150.t1.2", "SearchDataBase 1", "ProteinList", "ProteinMOxidized"
                       )
    ]
    |> (fun item -> sqliteMzQuantMLContext.AddRange(item.Cast()))

let proteinUnmodifiedRefParams =
    ProteinRefParamHandler.init(
        (TermSymbol.toID LeadingRazorProtein), "proteinUnmodifiedRefParam"
                               )
    |> ProteinRefParamHandler.addFKProteinRef "ProteinRefUnmodified"
    |> ProteinRefParamHandler.addToContext sqliteMzQuantMLContext

let proteinMOxidizedRefParams =
    ProteinRefParamHandler.init(
        (TermSymbol.toID LeadingRazorProtein), "proteinMOxidizedRefParam"
                               )
    |> ProteinRefParamHandler.addFKProteinRef "ProteinRefMOxidized"
    |> ProteinRefParamHandler.addToContext sqliteMzQuantMLContext

let proteinRef1 =
    ProteinRefHandler.init("ProteinUnmodified", "ProteinGroup 1", "ProteinRefUnmodified") 
    |> ProteinRefHandler.addToContext sqliteMzQuantMLContext

let proteinRef2 =
    ProteinRefHandler.init("ProteinMOxidized", "ProteinGroup 1", "ProteinRefMOxidized")
    |> ProteinRefHandler.addToContext sqliteMzQuantMLContext

let proteinGroupParam =
    ProteinGroupParamHandler.init(
        (TermSymbol.toID SequenceCoverage)
                                 )
    |> ProteinGroupParamHandler.addValue "31.7"
    |> ProteinGroupParamHandler.addFkUnit
        (TermSymbol.toID Percentage)
    |> ProteinGroupParamHandler.addFKProteinGroup "ProteinGroup 1"
    |> ProteinGroupParamHandler.addToContext sqliteMzQuantMLContext

let terms =
    [|
        (TermSymbol.toID Dalton)
        (TermSymbol.toID KiloDalton)
        (TermSymbol.toID Ppm)
        (TermSymbol.toID XICArea)
        (TermSymbol.toID NormalizedXICArea)
        (TermSymbol.toID TotalXIC)
        (TermSymbol.toID XICAreas)
        (TermSymbol.toID MinPep)
        (TermSymbol.toID MinPepRazor)
        (TermSymbol.toID NumberPeptideSeqsMatchedEachSpec)
        (TermSymbol.toID MinScoreModifiedPeptides)
        (TermSymbol.toID MinPeptideLength)
        (TermSymbol.toID LeadingPeptide)
        (TermSymbol.toID DistinctPeptideSequences)
        (TermSymbol.toID DiscardUnmodifiedPeptide)
        (TermSymbol.toID MinPepUnique)
        (TermSymbol.toID MS1LabelBasedPeptideAnalysis)
        (TermSymbol.toID BIon)
        (TermSymbol.toID MaxQuant)
        (TermSymbol.toID XIon)
    |]

let createTestProteinParams (n:int) =
    [
    ProteinParamHandler.init(terms.[0])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[1])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[2])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[3])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[4])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[5])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[6])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[7])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[8])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[9])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
     ProteinParamHandler.init(terms.[10])
     |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[11])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[12])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[13])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[14])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[15])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[16])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[17])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[18])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ProteinParamHandler.init(terms.[19])
    |> ProteinParamHandler.addFKProtein (string n)
    |> ProteinParamHandler.addValue (System.Guid.NewGuid().ToString());
    ]

let testSearchDatabaseParam =
    SearchDatabaseParamHandler.init(
        (TermSymbol.toID DatabaseVersion), "testSearchDBParam 1"
                                   )
    |> SearchDatabaseParamHandler.addValue "1.0"
    |> SearchDatabaseParamHandler.addFKSearchDatabase "TestSearchDB 1"
    |> SearchDatabaseParamHandler.addToContext sqliteMzQuantMLContext

let testSearchDatabase =
    SearchDatabaseHandler.init("local", "Name 1", "TestSearchDB 1")
    |> SearchDatabaseHandler.addToContext sqliteMzQuantMLContext

let createTestPeptideParams (n:int) =
    [
    PeptideConsensusParamHandler.init(
        terms.[0]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[1]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[2]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[3]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[4]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[5]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[6]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[7]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[8]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[9]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
     PeptideConsensusParamHandler.init(
        terms.[10]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[11]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[12]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[13]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[14]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[15]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[16]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[17]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[18]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    PeptideConsensusParamHandler.init(
        terms.[19]
                            )
    |> PeptideConsensusParamHandler.addValue (System.Guid.NewGuid().ToString())
    |> PeptideConsensusParamHandler.addFKPeptideConsensus (string n);
    ]
    
let testEvidenceRefMOxidized =
    EvidenceRefHandler.init("FeatureMOxidized 1")
    |> EvidenceRefHandler.addFKIdentificationFile "IdentificationFile 1"
    |> EvidenceRefHandler.addFKPeptideConsensus "PeptideConsensusMOxidized 1"
    |> EvidenceRefHandler.addToContext sqliteMzQuantMLContext

let createPeptideConsensus (n:int) (i:int) =
    PeptideConsensusHandler.init(3, "PeptideConsensusList 1", string n)
    |> PeptideConsensusHandler.addPeptideSequence "AAIEASFGSVDEMK" 
    |> PeptideConsensusHandler.addFKProtein (string i)
    //|> PeptideConsensusHandler.addDetails (createTestPeptideParams n)

//let createMultiplePeptides (collection:Map<string, PeptideConsensus>) =
//    let rec loop (someThings:Map<string, PeptideConsensus>) n i =
//        if n < 100 then
//            if i>10 then
//                loop someThings (n+1) 0
//            else
//                loop (someThings.Add (System.Guid.NewGuid().ToString(), testPeptideConsensis n)) n (i+1)
//        else someThings
//    loop collection 0 0
    
let createProtein n =
    ProteinHandler.init("Test", "TestSearchDB 1", "ProteinList", string n)
    |> ProteinHandler.addDetails (createTestProteinParams n)

let createMultiplePeptideParams (collection:Map<string, PeptideConsensusParam list>) =
    let rec loop (someThings:Map<string, PeptideConsensusParam list>) n =
        if n < 100000 then
            loop (someThings.Add (System.Guid.NewGuid().ToString(), createTestPeptideParams n)) (n+1)
        else someThings
    loop collection 0

let createMultiplePeptides (collection:Map<string, PeptideConsensus>) =
    let rec loop (someThings:Map<string, PeptideConsensus>) n i =
        if n < 100000 then
            if i>=10000 then
                loop someThings n 0
            else
                loop (someThings.Add (System.Guid.NewGuid().ToString(), createPeptideConsensus n i)) (n+1) (i+1)
        else someThings
    loop collection 0 0

let rec createMultipleProteins (collection:Map<string, Protein>) =
    let rec loop (someThings:Map<string, Protein>) n =
        if n < 10000 then 
            loop (someThings.Add (System.Guid.NewGuid().ToString(), createProtein n)) (n+1)
        else someThings
    loop collection 0
createMultipleProteins

let manyThousandPeptideParams = 
    createMultiplePeptideParams Map.empty
    |> Seq.map (fun item -> item.Value)
    |> Seq.concat
    |> Array.ofSeq
    |> (fun item -> sqliteMzQuantMLContext.PeptideConsensusParam.AddRange(item.Cast()))

let manyThousandPeptides = 
    createMultiplePeptides Map.empty
    |> Seq.map (fun item -> item.Value)
    |> Array.ofSeq
    |> (fun item -> sqliteMzQuantMLContext.PeptideConsensus.AddRange(item.Cast()))
    
let manyThousandProteins = 
    createMultipleProteins Map.empty
    |> Seq.map (fun item -> item.Value)
    |> Array.ofSeq
    |> (fun item -> sqliteMzQuantMLContext.Protein.AddRange(item.Cast()))

//for i in manyThousandPeptideParams do
//    printfn "%A" i.FKPeptideConsensus

let proteinList =
    ProteinListHandler.init("ProteinList")
    |> ProteinListHandler.addToContext sqliteMzQuantMLContext

let proteinGroup =
    ProteinGroupHandler.init(
        "SearchDataBase 1", "ProteinGroupList", "ProteinGroup 1"
                            )
    |> ProteinGroupHandler.addToContext sqliteMzQuantMLContext

let proteinGroupList =
    ProteinGroupListHandler.init("ProteinGroupList")
    |> ProteinGroupListHandler.addToContext sqliteMzQuantMLContext

let organizations =
    [
    OrganizationHandler.init("TuKL", name="TuKL")
    |> OrganizationHandler.addFKPerson "PatrickB"
    |> OrganizationHandler.addFKMzQuantMLDocument "Test1";
    OrganizationHandler.init("BioTech", name="BioTech")
    |> OrganizationHandler.addFKPerson "PatrickB"
    |> OrganizationHandler.addFKMzQuantMLDocument "Test1";
    OrganizationHandler.init("CSB", name="CSB")
    |> OrganizationHandler.addFKPerson "PatrickB"
    |> OrganizationHandler.addFKMzQuantMLDocument "Test1";
    ]
    |> (fun item -> sqliteMzQuantMLContext.AddRange(item.Cast()))

let person =
    PersonHandler.init("PatrickB")
    |> PersonHandler.addFirstName "Patrick"
    |> PersonHandler.addLastName "Blume"
    |> PersonHandler.addFKMzQuantMLDocument "Test1"
    |> PersonHandler.addToContext sqliteMzQuantMLContext

let role =
    CVParamHandler.init("MS:1001267", "Role 1")
    |> CVParamHandler.addToContext sqliteMzQuantMLContext

let contactRole =
    ContactRoleHandler.init("PatrickB", "Role 1", "ContactRole 1")
    |> ContactRoleHandler.addToContext sqliteMzQuantMLContext

let provider =
    ProviderHandler.init()
    |> ProviderHandler.addFKContactRole "ContactRole 1"
    |> ProviderHandler.addFKSoftware "AnalysisSoftware 1"
    |> ProviderHandler.addFKMzQuantMLDocument "Test1"
    |> ProviderHandler.addToContext sqliteMzQuantMLContext

let mzQuantMLDocument =
    MzQuantMLDocumentHandler.init("Test1")
    |> MzQuantMLDocumentHandler.addVersion "1.0"
    |> MzQuantMLDocumentHandler.addName "TestForMaxQuantData"
    |> MzQuantMLDocumentHandler.addCreationDate DateTime.Today
    |> MzQuantMLDocumentHandler.addFKInputFiles "InputFiles 1"
    |> MzQuantMLDocumentHandler.addFKProteinGroupList "ProteinGroupList"
    |> MzQuantMLDocumentHandler.addFKProteinList "ProteinList"
    |> MzQuantMLDocumentHandler.addToContext sqliteMzQuantMLContext


//sqliteMzQuantMLContext.SaveChanges()

//sqliteMzQuantMLContext.Database.CloseConnection()