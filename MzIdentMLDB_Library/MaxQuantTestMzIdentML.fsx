    
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
open MzIdentMLDataBase.DataModel
open MzIdentMLDataBase.InsertStatements.ObjectHandlers

//open MzIdentMLDataBase.XMLParsing


let fileDir = __SOURCE_DIRECTORY__
let standardDBPathSQLiteMzIdentML = fileDir + "\Databases\MzIdentML1.db"

let sqliteMzIdentMLContext = ContextHandler.sqliteConnection standardDBPathSQLiteMzIdentML


//Using peptideID = 119; Modification-specific peptides IDs=125 & 126; 
//Oxidation (M)Sites for Modification-specific peptides ID=97; ProteinGroups ID=173;
//AllPeptides line 227574 (MOxidized) & line 616423 (unmodified); MS/MS scans MOxLine=10847, UnModID=41328,
//Ms/MS MOxID=568, UnModID=576

let user0 =
    TermHandler.init("User:0000000")
    |> TermHandler.addName "N-term cleavage window"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user1 =
    TermHandler.init("User:0000001")
    |> TermHandler.addName "C-term cleavage window"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user2 =
    TermHandler.init("User:0000002")
    |> TermHandler.addName "Leading razor protein"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user3 =
    TermHandler.init("User:0000003")
    |> TermHandler.addName "MaxQuant: Unique Protein Group"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user4 =
    TermHandler.init("User:0000004")
    |> TermHandler.addName "MaxQuant: Unique Protein"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user5 =
    TermHandler.init("User:0000005")
    |> TermHandler.addName "MaxQuant: PEP"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user6 =
    TermHandler.init("User:0000006")
    |> TermHandler.addName "IdentificationType"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user7 =
    TermHandler.init("User:0000007")
    |> TermHandler.addName "AminoAcid modification"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user8 =
    TermHandler.init("User:0000008")
    |> TermHandler.addName "Charge corrected mass of the precursor ion"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user9 =
    TermHandler.init("User:0000009")
    |> TermHandler.addName "Calibrated retention time"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user10 =
    TermHandler.init("User:0000010")
    |> TermHandler.addName "Mass error"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user11 =
    TermHandler.init("User:0000011")
    |> TermHandler.addName "The intensity values of the isotopes."
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user12 =
    TermHandler.init("User:0000012")
    |> TermHandler.addName "MaxQuant: Major protein IDs"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user13 =
    TermHandler.init("User:0000013")
    |> TermHandler.addName "MaxQuant: Number of proteins"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user14 =
    TermHandler.init("User:0000014")
    |> TermHandler.addName "MaxQuant: Peptides"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user15 =
    TermHandler.init("User:0000015")
    |> TermHandler.addName "MaxQuant: Razor + unique peptides"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user16 =
    TermHandler.init("User:0000016")
    |> TermHandler.addName "MaxQuant: Unique peptides"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user17 =
    TermHandler.init("User:0000017")
    |> TermHandler.addName "MaxQuant: Unique + razor sequence coverage"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user18 =
    TermHandler.init("User:0000018")
    |> TermHandler.addName "MaxQuant: Unique sequence coverage"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user19 =
    TermHandler.init("User:0000019")
    |> TermHandler.addName "MaxQuant: SequenceLength(s)"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user20 =
    TermHandler.init("User:0000020")
    |> TermHandler.addName "Metabolic labeling N14/N15"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user21 =
    TermHandler.init("User:0000021")
    |> TermHandler.addName "DetectionType"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user22 =
    TermHandler.init("User:0000022")
    |> TermHandler.addName "MaxQuant: Uncalibrated m/z"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user23 =
    TermHandler.init("User:0000023")
    |> TermHandler.addName "MaxQuant: Number of data points"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user24 =
    TermHandler.init("User:0000024")
    |> TermHandler.addName "MaxQuant: Number of isotopic peaks"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user25 =
    TermHandler.init("User:0000025")
    |> TermHandler.addName "MaxQuant: Parent ion fraction"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user26 =
    TermHandler.init("User:0000026")
    |> TermHandler.addName "MaxQuant: Mass precision"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user27 =
    TermHandler.init("User:0000027")
    |> TermHandler.addName "Retention length (FWHM)"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user28 =
    TermHandler.init("User:0000028")
    |> TermHandler.addName "MaxQuant: Min scan number"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user29 =
    TermHandler.init("User:0000029")
    |> TermHandler.addName "MaxQuant: Max scan number"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user30 =
    TermHandler.init("User:0000030")
    |> TermHandler.addName "MaxQuant: MSMS scan numbers"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user31 =
    TermHandler.init("User:0000031")
    |> TermHandler.addName "MaxQuant: MSMS isotope indices"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user32 =
    TermHandler.init("User:0000032")
    |> TermHandler.addName "MaxQuant: Filtered peaks"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user33 =
    TermHandler.init("User:0000033")
    |> TermHandler.addName "FragmentationType"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user34 =
    TermHandler.init("User:0000034")
    |> TermHandler.addName "Parent intensity fraction"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user35 =
    TermHandler.init("User:0000035")
    |> TermHandler.addName "Fraction of total spectrum"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user36 =
    TermHandler.init("User:0000036")
    |> TermHandler.addName "Base peak fraction"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user37 =
    TermHandler.init("User:0000037")
    |> TermHandler.addName "Precursor full scan number"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user38 =
    TermHandler.init("User:0000038")
    |> TermHandler.addName "Precursor intensity"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user39 =
    TermHandler.init("User:0000039")
    |> TermHandler.addName "Precursor apex fraction"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user40 =
    TermHandler.init("User:0000040")
    |> TermHandler.addName "Precursor apex offset"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user41 =
    TermHandler.init("User:0000041")
    |> TermHandler.addName "Precursor apex offset time"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user42 =
    TermHandler.init("User:0000042")
    |> TermHandler.addName "Scan event number"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user43 =
    TermHandler.init("User:0000043")
    |> TermHandler.addName "MaxQuant: Score difference"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user44 =
    TermHandler.init("User:0000044")
    |> TermHandler.addName "MaxQuant: Combinatorics"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user45 =
    TermHandler.init("User:0000045")
    |> TermHandler.addName "MaxQuant: Matches"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user46 =
    TermHandler.init("User:0000046")
    |> TermHandler.addName "MaxQuant: Match between runs"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user47 =
    TermHandler.init("User:0000047")
    |> TermHandler.addName "MaxQuant: Number of matches"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user48 =
    TermHandler.init("User:0000048")
    |> TermHandler.addName "MaxQuant: Intensity coverage"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user49 =
    TermHandler.init("User:0000049")
    |> TermHandler.addName "MaxQuant: Peak coverage"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user50 =
    TermHandler.init("User:0000050")
    |> TermHandler.addName "MaxQuant: ETD identification type"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user51 =
    TermHandler.init("User:0000051")
    |> TermHandler.addName "Min. score unmodified peptides"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext
  
let user52 =
    TermHandler.init("User:0000052")
    |> TermHandler.addName "Min. score modified peptides"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user53 =
    TermHandler.init("User:0000053")
    |> TermHandler.addName "Min. delta score of unmodified peptides"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext 

let user54 =
    TermHandler.init("User:0000054")
    |> TermHandler.addName "Min. delta score of modified peptides"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user55 =
    TermHandler.init("User:0000055")
    |> TermHandler.addName "Min. amount unique peptide"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user56 =
    TermHandler.init("User:0000056")
    |> TermHandler.addName "Min. amount razor peptide"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user57 =
    TermHandler.init("User:0000057")
    |> TermHandler.addName "Min. amount peptide"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user58 =
    TermHandler.init("User:0000058")
    |> TermHandler.addName "MaxQuant: Decoy mode"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user59 =
    TermHandler.init("User:0000059")
    |> TermHandler.addName "MaxQuant: Special AAs"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user60 =
    TermHandler.init("User:0000060")
    |> TermHandler.addName "MaxQuant: Include contaminants"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user61 =
    TermHandler.init("User:0000061")
    |> TermHandler.addName "MaxQuant: iBAQ"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user62 =
    TermHandler.init("User:0000062")
    |> TermHandler.addName "MaxQuant: Top MS/MS peaks per 100 Dalton"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user63 =
    TermHandler.init("User:0000063")
    |> TermHandler.addName "MaxQuant: IBAQ log fit"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user64 =
    TermHandler.init("User:0000064")
    |> TermHandler.addName "MaxQuant: Protein FDR"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user65 =
    TermHandler.init("User:0000065")
    |> TermHandler.addName "MaxQuant: SiteFDR"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user66 =
    TermHandler.init("User:0000066")
    |> TermHandler.addName "MaxQuant: Use Normalized Ratios For Occupancy"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user67 =
    TermHandler.init("User:0000067")
    |> TermHandler.addName "MaxQuant: Peptides used for protein quantification"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user68 =
    TermHandler.init("User:0000068")
    |> TermHandler.addName "MaxQuant: Discard unmodified counterpart peptides"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user69 =
    TermHandler.init("User:0000069")
    |> TermHandler.addName "MaxQuant: Min. ratio count"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user70 =
    TermHandler.init("User:0000070")
    |> TermHandler.addName "MaxQuant: Use delta score"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user71 =
    TermHandler.init("User:0000071")
    |> TermHandler.addName "Data-dependt acquisition"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user72 =
    TermHandler.init("User:0000072")
    |> TermHandler.addName "razor-protein"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user73 =
    TermHandler.init("User:0000073")
    |> TermHandler.addName "razor-peptide"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user74 =
    TermHandler.init("User:0000074")
    |> TermHandler.addName "Mass-deviation"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user75 =
    TermHandler.init("User:0000075")
    |> TermHandler.addName "leading-peptide"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user76 =
    TermHandler.init("User:0000076")
    |> TermHandler.addName "unique-protein"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user77 =
    TermHandler.init("User:0000077")
    |> TermHandler.addName "unique-peptide"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user78 =
    TermHandler.init("User:0000078")
    |> TermHandler.addName "MaxQuant: Delta score"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

let user79 =
    TermHandler.init("User:0000079")
    |> TermHandler.addName "MaxQuant: Best andromeda score"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteMzIdentMLContext "UserParam").Value
    |> TermHandler.addToContext sqliteMzIdentMLContext

type TermIDByName =
    //Else
    | RawFile
    ///Length of sequence (MaxQuant).
    | SequenceLength
    ///The length of all sequences of the proteins contained in the group.
    | SequenceLengths
    ///Posterior Error Probability of the identification. Smaller is more significant.
    | PosteriorErrorProbability
    ///Percentage of the sequence that is covered by the identified
    ///peptides of the best protein sequence contained in the group.
    | SequenceCoverage
    ///Percentage of the sequence that is covered by the identified unique and razor peptides 
    ///of the best protein sequence contained in the group.
    | UniqueAndRazorSequenceCoverage
    ///Percentage of the sequence that is covered by the identified unique peptides 
    ///of the best protein sequence contained in the group.
    | UniqueSequenceCoverage
    ///Molecular weight of the leading protein sequence contained in the protein group.
    | MolecularWeight
    ///It is a quantitative proteomics software package designed for analyzing large-scale mass-spectrometric data sets.
    | MaxQuant
    ///The fraction of intensity in the MS/MS spectrum that is annotated.
    | IntensityCoverage
    ///The fraction of peaks in the MS/MS spectrum that are annotated.
    | PeakCoverage
    ///False discovery rate for peptide spectrum machtes.
    | PSMFDR
    ///False discovery rate for protein.
    | ProteinFDR
    ///False discovery rate for site.
    | SideFDR
    ///
    | UseNormRatios
    ///
    |UseDeltaScores
    ///Popular MS1 method, where a protein's total intensity is divided by the number of tryptic peptides 
    ///between 6 and 30 amino acids in length
    | IBAQ
    ///
    | IBAQLogFit
    ///Heavy N15 is induced to the growth media of one culture in order to distinguish 
    ///between heavy and light labeled peptides/proteins.
    | MetabolicLabelingN14N15
    ///The weight difference between light and heavy labeled peptides is used for quantification.
    | MetabolicLabelingN14N15Quantification
    ///
    | DataDependentAcquisition
    ///
    | DataIndependentAcquisition
    ///
    | PeptideRatio
    ///
    | MassDeviation
    ///The NCBI taxonomy id for the species.
    | TaxidNCBI
    ///Describes the version of the database.
    | DatabaseVersion

    //AminoAcids
    ///Sequence of amino acids.
    | AminoAcidSequence
    ///Mutation of an AminoAcid.
    | AmnoAcidModification
    ///
    | SpecialAAs

    //Proteins
    ///The identifier of the best scoring protein, from the
    ///ProteinGroups file this, this peptide is associated to.
    | LeadingRazorProtein
    ///The identifiers of the proteins in the proteinGroups file, with this
    ///Protein as best match, this particular peptide is associated with.
    | LeadingProtein
    ///Description of the protein.
    | ProteinDescription
    ///Number of proteins contained within the group.
    | NumberOfProteins
    ///This is the ratio of reverse to forward protein groups.
    | QValue
    ///True or FALSe of contaminants are included or not
    | Contaminants
    ///Determines which kind of peptide is used to identify the protein.
    | PeptidesForProteinQuantification
    ///
    | RazorProtein
    ///
    | UniqueProtein

    //Peptides
    ///Peptide is unique to a single protein group in the proteinGroups file.
    | UniqueToGroups
    ///Peptide is unique to a single protein sequence in the fasta file(s).
    | UniqueToProtein
    ///Sequence window from -8 to 8 around the N-terminal cleavagesite of this peptide.
    | NTermCleavageWindow
    ///Sequence window from -8 to 8 around the C-terminal cleavagesite of this peptide.
    | CTermCleavageWindow
    ///IDs of those proteins that have at least half of the peptides that the leading protein has
    | MajorProteinIDs
    ///Number of peptides associated with each protein in proteingroup.
    | PeptideCountsAll
    ///Number of razor and unique peptides associated with each protein in proteingroup.
    | PeptideCountsRazorAndUnique
    ///Number of unique peptides associated with each protein in proteingroup.
    | PeptideCountUnique
    ///The total number of peptide sequences associated with the protein group (i.e. for all the proteins in the group).
    | Peptides
    ///The total number of razor + unique peptides associated with the protein group (i.e. these peptides are shared with another protein group).
    | RazorAndUniquePeptides
    ///The total number of unique peptides associated with the protein group (i.e. these peptides are shared with another protein group).
    | UniquePeptides
    ///The mass of the neutral peptide ((m/z-proton) * charge).
    | Mass
    ///The score of the identification (higher is better).
    | PeptideScore
    ///The percentage the parent ion intensity makes up of the total
    ///intensity of the whole MS spectrum.
    | FractionOfTotalSpectrum
    ///Number of possible distributions of the modifications over the peptide sequence.
    | Combinatorics
    ///
    | DiscardUnmodifiedPeptide
    ///
    | MinRatioCount
    ///
    | NumberPeptideSeqsMatchedEachSpec
    ///
    | DistinctPeptideSequences
    ///Peptide can be found in several proteinsequences
    | PeptideSharedWithinMultipleProteins
    ///
    | RazorPeptide
    ///
    | UniquePeptide
    ///
    | LeadingPeptide

    //Nucleic Acids
    ///Seuqence of nucleic acids.
    | NucleicAcidSequence
    ///Mutation of a nucleic acid base.
    | NucleicAcidModification

    //MassSpectometry
    ///MonoIsotopicMass of the peptide.
    | MonoIsotopicMass
    ///Indicates the way this experiment was identified.
    | IdentificationType
    ///Area under curve for one XIC.
    | XICArea
    ///Normalized area under curve  for one XIC.
    | NormalizedXICArea
    ///Intensity values of the isotopes.
    | XICAreas
    ///Total sum of XIC areas associated with the AA-sequence.
    | TotalXIC
    ///The number of MS/MS spectra recorded for the peptide.
    | MSMSCount
    ///Charge corrected mass of the precursor ion.
    | MassPrecursorIon
    ///Retention time of the peptide.
    | RetentionTime
    ///Calibrated retention time.
    | CalibratedRetentionTime
    ///The RAW-file derived scan number of the MS/MS with the highest peptide identification score.
    | MSMSScanNumber
    ///Mass error of the recalibrated mass-over-charge value of the precursor ion 
    ///in comparison to the predicted monoisotopic mass of the identified peptide sequence.
    | MassError
    ///Protein score which is derived from peptide posterior error probabilities.
    | ProteinScore
    ///The type of detection for the peptide. 
    ///MULTI – A labeling multiplet was detected.
    ///ISO – An isotope pattern was detected
    | DetectionType
    ///m/z before recalibrations have been applied.
    | UncalibratedMZ
    ///The resolution of the peak detected for the peptide measured in Full Width at Half Maximum (FWHM).
    | FWHM
    ///The number of data points (peak centroids) collected for this peptide feature.
    | NumberOfDataPoints
    ///The number of MS scans that the 3d peaks of this peptide feature are overlapping with.
    | ScanNumber
    ///The number of isotopic peaks contained in this peptide feature.
    | NumberOfIsotopicPeaks
    ///Indicates the fraction the target peak makes up of the total intensity in the inclusion window (Parent Ion Fraction)
    | PIF
    ///Empirically derived deviation measure to the next nearest integer scaled to center around 0. 
    ///Can be used to visually detect contaminants in a plot setting Mass against this value.
    ///m*a+b – round(m*a+b)
    ///m: the peptide mass
    ///a: 0.999555
    ///b: -0.10
    | MassDefect
    ///The precision of the mass detection of the peptide in parts-per-million.
    | MassPrecision
    ///The total retention time width of the peak (last timepoint – first timepoint) in seconds
    | RetentionLength
    ///The full width at half maximum value retention time width of the peak in seconds.
    | RetentionTimeFWHM
    ///The first scan-number at which the peak was encountered.
    | MinScanNumber
    ///The last scan-number at which the peak was encountered.
    | MaxScanNumber
    ///The intensity values of the isotopes.
    | Intensities
    ///The scan numbers where the MS/MS spectra were recorded.
    | MSMSScanNumbers
    ///Indices of the isotopic peaks that the MS/MS spectra reside on.
    ///A value of 0 corresponds to the monoisotopic peak.
    | MSMSIsotopIndices
    ///Multiple peak list nativeID format.
    | MultipleIDs
    ///The ion inject time for the MS/MS scan.
    | IonInjectionTime
    ///The total ion current of the MS/MS scan.
    | TotalIonCurrent
    ///Tandem mass spectrometry involves several steps in identifing the target peptides.
    | MSMSSearch
    ///The collision energy used for the fragmentation that resulted in this MS/MS scan.
    | CollisionEnergy
    ///The intensity of the most intense ion in the spectrum.
    | BasePeakIntension
    ///The time the MS/MS scan took to complete.
    | ElapsedTime
    ///Number of peaks after the 'top X per 100 Da' filtering.
    | FilteredPeaks
    ///The type of fragmentation used to create the MS/MS spectrum.
    ///CID – Collision Induced Dissociation.
    ///HCD – High energy Collision induced Dissociation.
    ///ETD – Electron Transfer Dissociation.
    | FragmentationType
    ///The mass analyzer used to record the MS/MS spectrum. ITMS
    ///     Ion trap.
    ///FTMS Fourier transform ICR or orbitrap cell.
    ///TOF  Time of flight.
    | MassAnalyzerType
    ///The percentage the parent ion intensity makes up of the total
    ///intensity in the selection window
    | ParentIntensityFraction
    ///The percentage the parent ion intensity in comparison to the
    ///highest peak in he MS spectrum
    | BasePeakFraction
    ///The full scan number where the precursor ion was selected for fragmentation.
    | PrecursorFullScanNumbers
    ///The intensity of the precursor ion at the scannumber it was selected.
    | PrecursorIntensity
    ///The fraction the intensity of the precursor ion makes up of the peak (apex) intensity.
    | PrecursorApexFraction
    ///How many full scans the precursor ion is offset from the peak (apex) position
    | PrecursorApexOffset
    ///How much time the precursor ion is offset from the peak (apex) position.
    | PrecursorApexOffsetTime
    ///This number indicates which MS/MS scan this one is in the
    ///consecutive order of the MS/MS scans that are acquired after an MS scan.
    | ScanEventNumber
    ///Score difference to the second best positioning of modifications
    ///identified peptide with the same amino acid sequence.
    | ScoreDifference
    ///The species of the peaks in the fragmentation spectrum after TopN filtering
    | Matches
    ///The species of the peaks in the fragmentation spectrum after TopN filtering.
    | ProductIonIntensity
    ///The number of peaks matching to the predicted fragmentation spectrum.
    | NumberOfMatches
    ///How many neutral losses were applied to each fragment in the Andromeda scoring.
    | NeutralIonLoss
    ///For ETD spectra several different combinations of ion series
    ///are scored. Here the highest scoring combination is indicated.
    | ETDIdentificationType
    ///Min score for unmodified peptides to be identified.
    | MinScoreUnmodifiedPeptides
    ///Min score for modified peptides to be identified.
    | MinScoreModifiedPeptides
    ///Minimal length of peptide to be identified.
    | MinPeptideLength
    ///Minimal DeltaScore of unmodified peptide to be identified.
    | MinDeltaScoreUnmod
    ///Minimal DeltaScore of modified peptide to be identified.
    | MinDeltaScoreMod
    ///Minimal amount of unique peptides to be identified.
    | MinPepUnique
    ///Minimal amount of razor peptides to be identified.
    | MinPepRazor
    ///Minimal amount of peptides to be identified.
    | MinPep
    ///
    | DecoyMode
    ///The error window on experimental MS/MS fragment ion mass values.
    | MSMSTolerance
    ///Number of peaks after the 'top X per 100 Da' filtering.
    | TopPeakPer100Da
    ///
    | MSMSDeisotoping
    ///Mass error of the recalibrated mass-over-charge value of the
    ///precursor ion in comparison to the predicted monoisotopic
    ///mass of the identified peptide sequence in parts per million.
    | MassErrorPpm
    ///Mass error of the recalibrated mass-over-charge value of the
    ///precursor ion in comparison to the predicted monoisotopic
    ///mass of the identified peptide sequence in percent.
    | MassErrorPercent
    ///Mass error of the recalibrated mass-over-charge value of the
    ///precursor ion in comparison to the predicted monoisotopic
    ///mass of the identified peptide sequence in dalton.
    | MassErrorDa
    ///Upper limit of the search tolerance.
    | SearchToleranceUpperLimit
    ///Lower limit of the search tolerance.
    | SearchToleranceLowerLimit
    ///
    | ProductIonMZ
    ///
    | ProductIonErrorMZ
    ///How many neutral losses were applied to each fragment in the Andromeda scoring.
    | NeutralFragmentLoss
    ///
    | MatchBetweenRuns
    ///
    | MS1LabelBasedAnalysis
    ///
    | MS1LabelBasedPeptideAnalysis
    ///
    | MS1LabelBasedProteinAnalysis
    ///
    | SpectralCountQuantification
    ///
    | MzDeltaScore

    //Ion fragment types
    ///Kind of ion after fragmentation of peptide.
    | Fragment
    | ProductIonSeriesOrdinal
    | ProductIonMZDelta
    | ProductInterpretationRank
    | ATypeIon
    | YIon
    | YIonWithoutWater
    | YIonWithoutAmmoniumGroup
    | YIonWihtout3H3PO4
    | YIonWihtout2H3PO4
    | YIonWihtoutH3PO4
    | BIon
    | BIonWithoutWater
    | BIonWithoutAmmoniumGroup
    | BIonWihtout3H3PO4
    | BIonWihtout2H3PO4
    | BIonWihtoutH3PO4
    | XIon
    | XIonWithoutWater
    | XIonWithoutAmmoniumGroup

    //Units
    | Minute
    | Second
    | Ppm
    | Percentage
    | Dalton
    | KiloDalton
    | NumberOfDetections
    | MZ

    //File
    ///Information coded in FASTA format.
    | FASTAFormat
    ///Values are seperated by tab(s).
    | TSV
    ///Unspecific kind of database.
    | DataStoredInDataBase
    ///Uknown file type.
    | UnknownFileType
        
    //DataBases
    ///UniProt.
    | UniProt
    | DataBaseName

    //Taxonomie
    ///ScientificName of the species.
    | ScientificName

    //EnzymeNames
    ///A serineProtease which cuts digests after K, R and modified C.
    | Trypsin
    ///No enzyme was used.
    | NoEnzyme

    //Type of modification of NA or AA.
    ///An oxidaton.
    | Oxidation
        
    //Type of scoring by searchEnginges
    ///Andromeda score for the associated MS/MS spectra.
    | AndromedaScore
    ///Best Andromeda score for the associated MS/MS spectra.
    | BestAndromedaScore
    ///
    | ProteomicDiscovererDeltaScore
    ///Score difference to the second best identified peptide.
    | DeltaScore

    static member toID (item:TermIDByName) =
        match item with
        | RawFile -> "MS:1000577"
        | AminoAcidSequence -> "MS:1001344"
        | NucleicAcidSequence -> "MS:1001343"
        | SequenceLength -> "MS:1001900"
        | NTermCleavageWindow -> "User:0000000"
        | CTermCleavageWindow -> "User:0000001"
        | MonoIsotopicMass -> "MS:1000225"
        | LeadingRazorProtein -> "User:0000002"
        | UniqueToGroups -> "User:0000003"
        | UniqueToProtein -> "MS:1001363"
        | PosteriorErrorProbability -> "User:0000005"
        | AndromedaScore -> "MS:1002338"
        | IdentificationType -> "User:0000006"
        | XICArea -> "MS:1001858"
        | NormalizedXICArea -> "MS:1001859"
        | TotalXIC -> "MS:1002412"
        | MSMSCount -> "MS:1001904"
        | NucleicAcidModification -> "MS:1002028"
        | AmnoAcidModification -> "User:0000007"
        | MassPrecursorIon -> "User:0000008"
        | RetentionTime -> "MS:1000894"
        | Minute -> "MS:1000038"
        | Second -> "MS:1000039"
        | CalibratedRetentionTime -> "User:0000009"
        | MSMSScanNumber -> "MS:1001115"
        | ProteomicDiscovererDeltaScore -> "MS:1002834"
        | LeadingProtein -> "MS:1002401"
        | ProteinDescription -> "MS:1001088"
        | MassError -> "User:0000010"
        | Ppm -> "UO:0000169"
        | MajorProteinIDs -> "User:0000012"
        | PeptideCountsAll -> "MS:1001898"
        | PeptideCountsRazorAndUnique -> "MS:1001899"
        | PeptideCountUnique -> "MS:1001897"
        | NumberOfProteins -> "User:0000013"
        | Peptides -> "User:0000014"
        | RazorAndUniquePeptides -> "User:0000015"
        | UniquePeptides -> "User:0000016"
        | SequenceCoverage -> "MS:1001093"
        | Percentage -> "UO:0000187"
        | UniqueAndRazorSequenceCoverage -> "User:0000017"
        | UniqueSequenceCoverage -> "User:0000018"
        | MolecularWeight -> "PRIDE:0000057"
        | SequenceLengths -> "User:0000019"
        | QValue -> "MS:1001491"
        | ProteinScore -> "MS:1002394"
        | FASTAFormat -> "MS:1001348"
        | DetectionType -> "User:0000021"
        | Mass -> "MS:1000224"
        | UncalibratedMZ -> "User:0000022"
        | FWHM -> "MS:1000086"
        | NumberOfDataPoints -> "User:0000023"
        | ScanNumber -> "MS:1001115"
        | NumberOfIsotopicPeaks -> "User:0000024"
        | PIF -> "User:0000025"
        | MassDefect -> "MS:1000222"
        | MassPrecision -> "User:0000026"
        | RetentionLength -> "MS:1000826"
        | RetentionTimeFWHM -> "User:0000027"
        | MaxScanNumber -> "User:0000028"
        | MinScanNumber -> "User:0000029"
        | PeptideScore -> "MS:1002221"
        | Intensities -> "MS:1001846"
        | MSMSScanNumbers -> "User:0000030"
        | MSMSIsotopIndices -> "User:0000031"
        | MultipleIDs -> "MS:1000774"
        | IonInjectionTime -> "MS:1000927"
        | TotalIonCurrent -> "MS:1000285"
        | MaxQuant -> "MS:1001583"
        | MSMSSearch -> "MS:1001083"
        | CollisionEnergy -> "MS:1000045"
        | BasePeakIntension -> "MS:1000505"
        | ElapsedTime -> "MS:1000747"
        | FilteredPeaks -> "User:0000032"
        | FragmentationType -> "User:0000033"
        | MassAnalyzerType -> "MS:1000443"
        | ParentIntensityFraction -> "User:0000034"
        | FractionOfTotalSpectrum -> "User:0000035"
        | BasePeakFraction -> "User:0000036"
        | PrecursorFullScanNumbers -> "User:0000037"
        | PrecursorIntensity -> "User:0000038"
        | PrecursorApexFraction -> "User:0000039"
        | PrecursorApexOffset -> "User:0000040"
        | PrecursorApexOffsetTime -> "User:0000041"
        | ScanEventNumber -> "User:0000042"
        | ScoreDifference -> "User:0000043"
        | Combinatorics -> "User:0000044"
        | Matches -> "User:0000045"
        | Fragment -> "MS:1002695"
        | ProductIonIntensity -> "MS:1001226"
        | Dalton -> "MS:1000212"
        | KiloDalton -> "UO:0000222"
        | NumberOfMatches -> "User:0000047"
        | IntensityCoverage -> "User:0000048"
        | PeakCoverage -> "User:0000049"
        | NeutralIonLoss -> "MS:1001061"
        | ETDIdentificationType -> "User:0000050"
        | MinScoreUnmodifiedPeptides -> "User:0000051"
        | MinScoreModifiedPeptides -> "User:0000052"
        | MinPeptideLength -> "MS:1002322"
        | MinDeltaScoreUnmod -> "User:0000053"
        | MinDeltaScoreMod -> "User:0000054"
        | MinPepUnique -> "User:0000055"
        | MinPepRazor -> "User:0000056"
        | MinPep -> "User:0000057"
        | DecoyMode -> "User:0000058"
        | SpecialAAs -> "User:0000059"
        | Contaminants -> "User:0000060"
        | MSMSTolerance -> "MS:1001655"
        | TopPeakPer100Da -> "User:0000062"
        | MSMSDeisotoping -> "MS:1000033"
        | PSMFDR -> "MS:1002351"
        | ProteinFDR -> "User:0000064"
        | SideFDR -> "User:0000065"
        | UseNormRatios -> "User:0000066"
        | PeptidesForProteinQuantification -> "User:0000067"
        | DiscardUnmodifiedPeptide -> "User:0000068"
        | MinRatioCount -> "User:0000069"
        | UseDeltaScores -> "User:0000070"
        | MassErrorPpm -> "PRIDE:0000083"
        | MassErrorPercent -> "PRIDE:0000085"
        | MassErrorDa -> "PRIDE:0000086"
        | NumberOfDetections -> "MS:1000131"
        | SearchToleranceUpperLimit -> "MS:1001412"
        | SearchToleranceLowerLimit -> "MS:1001413"
        | ProductIonMZ -> "MS:1001225"
        | ProductIonErrorMZ -> "MS:1001227"
        | MZ -> "MS:1000040"
        | TSV -> "MS:1000914"
        | DataStoredInDataBase -> "MS:1001107"
        | UnknownFileType -> "PRIDE:0000059"
        | UniProt -> "MS:1001254"
        | ScientificName -> "MS:1001469"
        | Trypsin -> "MS:1001251"
        | NoEnzyme -> "MS:1001091"
        | ATypeIon -> "UNIMOD:140"
        | BIonWihtoutH3PO4 -> "PRIDE:0000419"
        | BIonWihtout2H3PO4 -> "PRIDE:0000420"
        | BIonWihtout3H3PO4 -> "PRIDE:0000421"
        | YIonWihtoutH3PO4 -> "PRIDE:0000422"
        | YIonWihtout2H3PO4 -> "PRIDE:0000423"
        | YIonWihtout3H3PO4 -> "PRIDE:0000424"
        | ProductIonSeriesOrdinal -> "MS:1000903"
        | ProductIonMZDelta -> "MS:1000904"
        | ProductInterpretationRank -> "MS:1000926"
        | YIon -> "MS:1001220"
        | YIonWithoutWater -> "MS:1001223"
        | YIonWithoutAmmoniumGroup -> "MS:1001233"
        | BIon -> "MS:1001224"
        | BIonWithoutWater -> "MS:1001222"
        | BIonWithoutAmmoniumGroup -> "MS:1001232"
        | XIon ->"MS:1001228"
        | XIonWithoutWater -> "MS:1001519"
        | XIonWithoutAmmoniumGroup -> "MS:1001520"
        | NumberPeptideSeqsMatchedEachSpec -> "MS:1001030"
        | Oxidation -> "UNIMOD:35"
        | DistinctPeptideSequences -> "MS:1001097"
        | PeptideSharedWithinMultipleProteins -> "MS:1001175"
        | NeutralFragmentLoss -> "MS:1001524"
        | MatchBetweenRuns -> "User:0000046"
        | IBAQ ->"User:0000061"
        | IBAQLogFit -> "User:0000063"
        | MS1LabelBasedAnalysis -> "MS:1002018"
        | MS1LabelBasedPeptideAnalysis -> "MS:1002002"
        | MS1LabelBasedProteinAnalysis -> "MS:1002003"
        | SpectralCountQuantification -> "MS:1001836"
        | MetabolicLabelingN14N15Quantification -> "MS:1001839"
        | MetabolicLabelingN14N15 -> "User:0000020"
        | DataDependentAcquisition -> "User:0000071"
        | DataIndependentAcquisition -> "PRIDE:0000450"
        | XICAreas -> "User:0000011"
        | RazorProtein -> "User:0000072"
        | UniqueProtein -> "User:0000076"
        | RazorPeptide -> "User:0000073"
        | UniquePeptide -> "User:0000077"
        | LeadingPeptide -> "User:0000075"
        | PeptideRatio -> "MS:1001132"
        | MzDeltaScore -> "MS:1001975"
        | MassDeviation -> "User:0000074"
        | TaxidNCBI -> "MS:1001467"
        | DatabaseVersion -> "MS:1001016"
        | DeltaScore -> "User:0000078"
        | BestAndromedaScore -> "User:0000079"
        | DataBaseName -> "MS:1001013"

//Peptides ID=119; Modification-specific peptides IDs=123 & 124

let mzIdentMLDocument =
    MzIdentMLDocumentHandler.init(version="1.0", id="Test1")

let measureErrors =
    [
    MeasureParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MassErrorPpm)).Value
                            )
    |> MeasureParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Ppm)).Value;
    MeasureParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MassErrorPercent)).Value
                            )
    |> MeasureParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Percentage)).Value;
    MeasureParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MassErrorDa)).Value
                            )
    |> MeasureParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Dalton)).Value;
    MeasureParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID SearchToleranceUpperLimit)).Value,
            "Mass Deviations upper limit [ppm]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Ppm)).Value;
    MeasureParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID SearchToleranceLowerLimit)).Value,
            "Mass Deviations lower limit [ppm]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Ppm)).Value;
    MeasureParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MassDeviation)).Value,
            "Mass Deviation [ppm]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Ppm)).Value;
    MeasureParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID SearchToleranceUpperLimit)).Value,
            "Mass Deviations upper limit [Da]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Dalton)).Value;
    MeasureParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID SearchToleranceLowerLimit)).Value,
            "Mass Deviations lower limit [Da]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Dalton)).Value;
    MeasureParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MassDeviation)).Value,
            "Mass Deviation [Da]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Dalton)).Value;
    MeasureParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ProductIonErrorMZ)).Value,
            "Product-ion error [M/Z]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MZ)).Value; 
    ]

let measureTypes =
    [
    MeasureParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ProductIonMZ)).Value,
            "Product-ion [M/Z]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MZ)).Value;                       
    MeasureParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ProductIonIntensity)).Value,
            "Intensity"
                            )
    |> MeasureParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID NumberOfDetections)).Value;
    ]

MeasureHandler.init(measureTypes, "FragmentationTable measure-type")
|> MeasureHandler.addToContext sqliteMzIdentMLContext

MeasureHandler.init(measureErrors, "FragmentationTable measure-error")
|> MeasureHandler.addToContext sqliteMzIdentMLContext

MeasureHandler.tryFindByID sqliteMzIdentMLContext "FragmentationTable measure-type"
MeasureHandler.tryFindByID sqliteMzIdentMLContext "FragmentationTable measure-error"

let fileFormat1 =
    CVParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID DataStoredInDataBase)).Value
                        )

let fileFormat2 =
    CVParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID UnknownFileType)).Value
                        )

let fileFormat3 =
    CVParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID FASTAFormat)).Value
                        )

let databaseName =
    CVParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID DataBaseName)).Value
                        )
    |> CVParamHandler.addValue "Unknown to me at the moment"

let searchDataBaseParam =
    SearchDatabaseParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID DatabaseVersion)).Value
                                   )
    |> SearchDatabaseParamHandler.addValue "1.0"

let searchDatabase =
    SearchDatabaseHandler.init(
        "local", fileFormat3, databaseName
                              )
    |> SearchDatabaseHandler.addDetail searchDataBaseParam

let dbSequenceParams =
    [
    DBSequenceParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID TaxidNCBI)).Value
                               )
    |> DBSequenceParamHandler.addValue "906914";
    DBSequenceParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ScientificName)).Value
                               )
    |> DBSequenceParamHandler.addValue "C. reinhardtii";
    ]

let dbSequence =
    DBSequenceHandler.init("Cre02.g096150.t1.2", searchDatabase)
    |> DBSequenceHandler.addSequence "AAIEASFGSVDEMK"
    |> DBSequenceHandler.addLength 14
    |> DBSequenceHandler.addDetails dbSequenceParams
    |> DBSequenceHandler.addMzIdentMLDocument mzIdentMLDocument

let peptideParamUnmodified =
    [
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID AminoAcidSequence)).Value
                            )
    |> PeptideParamHandler.addValue "AAIEASFGSVDEMK";
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID SequenceLength)).Value
                            )
    |> PeptideParamHandler.addValue "14";
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID NTermCleavageWindow)).Value
                            )
    |> PeptideParamHandler.addValue "NGPNGDVKAAIEASFG";
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID CTermCleavageWindow)).Value
                            )
    |> PeptideParamHandler.addValue "FGSVDEMKAKFNAAAA";
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID UniqueToGroups)).Value
                            )
    |> PeptideParamHandler.addValue "yes";
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID AmnoAcidModification)).Value
                            )
    |> PeptideParamHandler.addValue "yes";
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Mass)).Value
                            )
    |> PeptideParamHandler.addValue "1453.6797"
    |> PeptideParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID KiloDalton)).Value;
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MonoIsotopicMass)).Value
                            )
    |> PeptideParamHandler.addValue "1453.6759";
    ]

let peptideUnmodified =
    PeptideHandler.init("AAIEASFGSVDEMK", "119")
    |> PeptideHandler.addDetails peptideParamUnmodified
    |> PeptideHandler.addMzIdentMLDocument mzIdentMLDocument

let enzymeName =
    EnzymeNameParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Trypsin)).Value;
                               )
let enzyme =
    EnzymeHandler.init()
    |> EnzymeHandler.addSiteRegexc "KRC"
    |> EnzymeHandler.addEnzymeName enzymeName

let spectrumidentificationItemParamPeptideUnmodified =
    [
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PosteriorErrorProbability)).Value
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "2.95E-39";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID AndromedaScore)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "122.97";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID IdentificationType)).Value
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "By MS/MS";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID TotalXIC)).Value
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "290760";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSCount)).Value
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "14";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSScanNumber)).Value
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "15279";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID RetentionTime)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "25.752"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Minute)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID CalibratedRetentionTime)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "25.664"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Minute)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID DeltaScore)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "110.81";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID DetectionType)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "MULTI";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID UncalibratedMZ)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "727.8481";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID FWHM)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "NAN";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID NumberOfDataPoints)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "27";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ScanNumber)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "11";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID NumberOfIsotopicPeaks)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "3";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PIF)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "0.8484042";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MassDefect)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "-0.028955966";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MassPrecision)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "0.771146"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Ppm)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MzDeltaScore)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "727.8458225";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID RetentionLength)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "22.688"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Minute)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID RetentionTimeFWHM)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "11.349"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Minute)).Value;
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

let fragmentArrayUnModified1 =
    [
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Intensity").Value]
                            ),
                            177.                
                             );
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Mass Deviation [Da]").Value]
                            ),
                            0.00357427
                             );
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Mass Deviation [ppm]").Value]
                            ),
                            6.844385
                             );
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Product-ion [M/Z]").Value]
                            ),
                            522.219250635495
                             );
    ]
  
let ionTypeParamUnModified1 =
    [
    IonTypeParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Fragment)).Value
                            );
    IonTypeParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID YIon)).Value
                            )
    |> IonTypeParamHandler.addValue "y4";
    ]

let fragmentArrayUnModified2 =
    [
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Intensity").Value]
                            ),
                            129.                
                                );
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Mass Deviation [Da]").Value]
                            ),
                            0.00393288
                                );
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Mass Deviation [ppm]").Value]
                            ),
                            6.330212
                                );
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Product-ion [M/Z]").Value]
                            ),
                            621.287305940905
                             );
    ]
  
let ionTypeParamUnModified2 =
    [
    IonTypeParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Fragment)).Value
                            );
    IonTypeParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID YIon)).Value
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
        peptideUnmodified, 2, 727.84714, true, 0
                                            )
    |> SpectrumIdentificationItemHandler.addFragmentations fragmentationsUnModified
    |> SpectrumIdentificationItemHandler.addDetails spectrumidentificationItemParamPeptideUnmodified

let modificationParamsMOxidized =
    [
    ModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Oxidation)).Value
                                    )
    |> ModificationParamHandler.addValue "Methionin";
    ModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Combinatorics)).Value
                                    )
    |> ModificationParamHandler.addValue "1";
    ModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID NeutralIonLoss)).Value
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
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID AminoAcidSequence)).Value
                            )
    |> PeptideParamHandler.addValue "AAIEASFGSVDEMK";
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID SequenceLength)).Value
                            )
    |> PeptideParamHandler.addValue "14";
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID NTermCleavageWindow)).Value
                            )
    |> PeptideParamHandler.addValue "NGPNGDVKAAIEASFG";
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID CTermCleavageWindow)).Value
                            )
    |> PeptideParamHandler.addValue "FGSVDEMKAKFNAAAA";
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID UniqueToGroups)).Value
                            )
    |> PeptideParamHandler.addValue "yes";
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID AmnoAcidModification)).Value
                            )
    |> PeptideParamHandler.addValue "yes"; 
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Mass)).Value
                            )
    |> PeptideParamHandler.addValue "1469.6761";
    PeptideParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MonoIsotopicMass)).Value
                                                )
    |> PeptideParamHandler.addValue "1469.6761";
    ]

let peptideMOxidized =
        PeptideHandler.init("AAIEASFGSVDEM(1)K", "119MOxidized")
        |> PeptideHandler.addModification modificationMOxidized
        |> PeptideHandler.addDetails peptideParamMOxidized
        |> PeptideHandler.addMzIdentMLDocument mzIdentMLDocument

let spectrumidentificationItemParamPeptideMOxidized =
    [
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PosteriorErrorProbability)).Value
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "6,36E-25";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID AndromedaScore)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "111.12";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID IdentificationType)).Value
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "By MS/MS";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID TotalXIC)).Value
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "167790";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSCount)).Value
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "14";
    //SpectrumIdentificationItemParamHandler.init(
    //    (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSScanNumber)).Value
    //                                            )
    //|> SpectrumIdentificationItemParamHandler.addValue "12427";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID RetentionTime)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "21.489"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Minute)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID CalibratedRetentionTime)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "21.372"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Minute)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID DeltaScore)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "100.69";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID DetectionType)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "MULTI";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID UncalibratedMZ)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "735.8439"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MZ)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID FWHM)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "NAN";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID NumberOfDataPoints)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "24";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ScanNumber)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "10";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID NumberOfIsotopicPeaks)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "4";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PIF)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "0.8281062";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MassDefect)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "-0.039976308";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MassPrecision)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "1.815383"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Ppm)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MzDeltaScore)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "735.8475942";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID RetentionLength)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "20.649"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Minute)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID RetentionTimeFWHM)).Value
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "7.509"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Minute)).Value;
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
    |> Seq.map (fun value -> ValueHandler.init value)

let fragmentArrayMOXidized1 =
    [
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Intensity").Value]
                            ),
                            68.                
                                );
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Mass Deviation [Da]").Value]
                            ),
                            0.005498344
                                );
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Mass Deviation [ppm]").Value]
                            ),
                            12.99276
                                );
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Product-ion [M/Z]").Value]
                            ),
                            423.185298151502
                                );
    ]
  
let ionTypeParamMOxidized1 =
    [
    IonTypeParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Fragment)).Value
                            );
    IonTypeParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID YIon)).Value
                            )
    |> IonTypeParamHandler.addValue "y3";
    ]

let fragmentArrayMOXidized2 =
    [
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Intensity").Value]
                            ),
                            150.                
                                );
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Mass Deviation [Da]").Value]
                            ),
                            -0.003835145
                                );
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Mass Deviation [ppm]").Value]
                            ),
                            -7.125587
                                );
    FragmentArrayHandler.init(
        MeasureHandler.init(
            [(MeasureParamHandler.tryFindByID sqliteMzIdentMLContext "Product-ion [M/Z]").Value]
                            ),
                            423.185298151502
                                );
    ]
  
let ionTypeParamMOxidized2 =
    [
    IonTypeParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Fragment)).Value
                            );
    IonTypeParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID YIon)).Value
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
        peptideMOxidized, 2, 735.84531, true, -1
                                            )
    |> SpectrumIdentificationItemHandler.addFragmentations fragmentationsMOxidized
    |> SpectrumIdentificationItemHandler.addDetails spectrumidentificationItemParamPeptideMOxidized
    
let peptideEvidences =
    [
    PeptideEvidenceHandler.init(
        dbSequence, peptideUnmodified
                                )
    |> PeptideEvidenceHandler.addStart 106
    |> PeptideEvidenceHandler.addEnd 119
    |> PeptideEvidenceHandler.addMzIdentMLDocument mzIdentMLDocument;
    PeptideEvidenceHandler.init(
        dbSequence, peptideMOxidized
                                )
    |> PeptideEvidenceHandler.addStart 106
    |> PeptideEvidenceHandler.addEnd 119
    |> PeptideEvidenceHandler.addMzIdentMLDocument mzIdentMLDocument
    ]

let peptideHypothesisUnModified =
    PeptideHypothesisHandler.init(
        peptideEvidences.[0], 
        [spectrumidentificationItemPeptideUnmodified]
                                    )

let peptideHypothesisMOxidized =
    PeptideHypothesisHandler.init(
        peptideEvidences.[1], 
        [spectrumidentificationItemPeptideMOxidized]
                                    )

let proteinDetectionHypothesisParams =
    [
    ProteinDetectionHypothesisParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ProteinDescription)).Value
                                                )
    |> ProteinDetectionHypothesisParamHandler.addValue 
        ">Cre02.g096150.t1.2 Mn superoxide dismutase ALS=MSD1 DBV=JGI5.5 GN=MSD1 
        OS=Chlamydomonas reinhardtii SV=2 TOU=Cre"
    ProteinDetectionHypothesisParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID DistinctPeptideSequences)).Value
                                                )
    |> ProteinDetectionHypothesisParamHandler.addValue "TRUE";
    ]
    
let proteinDetectionHypothesis =
    ProteinDetectionHypothesisHandler.init(
        true, dbSequence, [peptideHypothesisUnModified; peptideHypothesisMOxidized], "Cre02.g096150.t1.2", "Cre02.g096150.t1.2"
                                            )
    |> ProteinDetectionHypothesisHandler.addDetails proteinDetectionHypothesisParams
    |> ProteinDetectionHypothesisHandler.addMzIdentMLDocument mzIdentMLDocument

let proteinAmbiguousGroupsParams =
    [
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MajorProteinIDs)).Value
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "Cre02.g096150.t1.2";  
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID NumberOfProteins)).Value
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "1";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Peptides)).Value
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "6";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID RazorAndUniquePeptides)).Value
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "6";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID UniquePeptides)).Value
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "6";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID SequenceCoverage)).Value
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "31.7"
    |> ProteinAmbiguityGroupParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Percentage)).Value;
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID UniqueAndRazorSequenceCoverage)).Value
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "31.7"
    |> ProteinAmbiguityGroupParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Percentage)).Value;
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID UniqueSequenceCoverage)).Value
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "31.7"
    |> ProteinAmbiguityGroupParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Percentage)).Value;
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MolecularWeight)).Value
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "23.9"
    |> ProteinAmbiguityGroupParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Percentage)).Value;
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID SequenceLength)).Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue "218";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID SequenceLengths)).Value
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "218";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID QValue)).Value
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "0";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID TotalXIC)).Value
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "1335100";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID LeadingProtein)).Value
                                               )
    |> ProteinAmbiguityGroupParamHandler.addValue "Cre02.g096150.t1.2";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID LeadingRazorProtein)).Value
                                               )
    |> ProteinAmbiguityGroupParamHandler.addValue "Cre02.g096150.t1.2"
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PSMFDR)).Value
                                        )
    |> ProteinAmbiguityGroupParamHandler.addValue "0.01";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ProteinFDR)).Value
                                        )
    |> ProteinAmbiguityGroupParamHandler.addValue "0.01";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID SideFDR)).Value
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
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MultipleIDs)).Value
                        )

let spectradata =
    [
    SpectraDataHandler.init(
        "local", fileFormat1, spectrumIDFormat
                            )
    |> SpectraDataHandler.addName "20170518 TM FSconc3001";
    SpectraDataHandler.init(
        "local", fileFormat1, spectrumIDFormat
                            )
    |> SpectraDataHandler.addName "20170518 TM FSconc3002";
    ]

let organizations =
    [
    OrganizationHandler.init(name="TuKL")
    |> OrganizationHandler.addMzIdentMLDocument mzIdentMLDocument;
    OrganizationHandler.init(name="BioTech")
    |> OrganizationHandler.addMzIdentMLDocument mzIdentMLDocument;
    OrganizationHandler.init(name="CSB")
    |> OrganizationHandler.addMzIdentMLDocument mzIdentMLDocument;
    ]

let person =
    PersonHandler.init()
    |> PersonHandler.addFirstName "Patrick"
    |> PersonHandler.addLastName "Blume"
    |> PersonHandler.addOrganizations organizations
    |> PersonHandler.addMzIdentMLDocument mzIdentMLDocument

let role =
    CVParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext "MS:1001267").Value
                       )

let contactRole =
    ContactRoleHandler.init(
        person, role
                           )

let softwareName =
    CVParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MaxQuant)).Value
                        )

let analysisSoftware =
    AnalysisSoftwareHandler.init(softwareName)
    |> AnalysisSoftwareHandler.addAnalysisSoftwareDeveloper contactRole

let searchType =
    CVParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSSearch)).Value
                        )

let searchModificationParam =
    [
    SearchModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID NeutralFragmentLoss)).Value
                            )
    |> SearchModificationParamHandler.addValue "None";
    SearchModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Oxidation)).Value
                            )
    |> SearchModificationParamHandler.addValue "Methionine";
    SearchModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSTolerance)).Value
                                        )
    |> SearchModificationParamHandler.addValue "20 for FTMS"
    |> SearchModificationParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Ppm)).Value;
    SearchModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID TopPeakPer100Da)).Value
                                        )
    |> SearchModificationParamHandler.addValue "12 for FTMS";
    SearchModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSDeisotoping)).Value
                                        )
    |> SearchModificationParamHandler.addValue "TRUE for FTMS";
    SearchModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSTolerance)).Value
                                        )
    |> SearchModificationParamHandler.addValue "0.5 for ITMS"
    |> SearchModificationParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Dalton)).Value;
    SearchModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID TopPeakPer100Da)).Value
                                        )
    |> SearchModificationParamHandler.addValue "8 for ITMS";
    SearchModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSDeisotoping)).Value
                                        )
    |> SearchModificationParamHandler.addValue "FALSE for ITMS";
    SearchModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSTolerance)).Value
                                        )
    |> SearchModificationParamHandler.addValue "40 for TOF"
    |> SearchModificationParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Ppm)).Value;
    SearchModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID TopPeakPer100Da)).Value
                                        )
    |> SearchModificationParamHandler.addValue "10 for TOF";
    SearchModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSDeisotoping)).Value
                                        )
    |> SearchModificationParamHandler.addValue "TRUE for TOF";
    SearchModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSTolerance)).Value
                                        )
    |> SearchModificationParamHandler.addValue "0.5 for Unknown"
    |> SearchModificationParamHandler.addUnit
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Dalton)).Value;
    SearchModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID TopPeakPer100Da)).Value
                                        )
    |> SearchModificationParamHandler.addValue "8 for Unknown";
    SearchModificationParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSDeisotoping)).Value
                                        )
    |> SearchModificationParamHandler.addValue "False for Unknown";
    ]

let searchModificationParams =
    [
    SearchModificationHandler.init(
        false, 15.9949, "M", searchModificationParam
                                    );
    SearchModificationHandler.init(
        false, 43.0, "N-Term", searchModificationParam
                                    )
    ]

let threshold =
    [
    ThresholdParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MinScoreUnmodifiedPeptides)).Value
                                )
    |> ThresholdParamHandler.addValue "0";
    ThresholdParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MinScoreModifiedPeptides)).Value
                                )
    |> ThresholdParamHandler.addValue "40";
    ThresholdParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MinPeptideLength)).Value
                                )
    |> ThresholdParamHandler.addValue "6";
    ThresholdParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MinDeltaScoreUnmod)).Value
                                )
    |> ThresholdParamHandler.addValue "0";
    ThresholdParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MinDeltaScoreMod)).Value
                                )
    |> ThresholdParamHandler.addValue "6";
    ThresholdParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MinPepUnique)).Value
                                )
    |> ThresholdParamHandler.addValue "0";
    ]

let additionalSearchParams =
    [
    AdditionalSearchParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID DecoyMode)).Value
                                        )
    |> AdditionalSearchParamHandler.addValue "revert";
    AdditionalSearchParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Contaminants)).Value
                                        )
    |> AdditionalSearchParamHandler.addValue "TRUE";
    ]

let fragmentTolerance =
    [
    FragmentToleranceParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID SearchToleranceUpperLimit)).Value,
            "Mass Deviations upper limit [ppm]"
                            )
    |> FragmentToleranceParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Ppm)).Value;
    FragmentToleranceParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID SearchToleranceLowerLimit)).Value,
            "Mass Deviations lower limit [ppm]"
                            )
    |> FragmentToleranceParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Ppm)).Value;
    FragmentToleranceParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID SearchToleranceUpperLimit)).Value,
            "Mass Deviations upper limit [Da]"
                            )
    |> FragmentToleranceParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Dalton)).Value;
    FragmentToleranceParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID SearchToleranceLowerLimit)).Value,
            "Mass Deviations lower limit [Da]"
                            )
    |> FragmentToleranceParamHandler.addUnit 
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID Dalton)).Value;
    ]

let analysisParams =
    [
    AnalysisParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PeptidesForProteinQuantification)).Value
                                                    )
    |> AnalysisParamHandler.addValue "Razor";
    AnalysisParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID DiscardUnmodifiedPeptide)).Value
                                                    )
    |> AnalysisParamHandler.addValue "TRUE";
    AnalysisParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MinRatioCount)).Value
                                                    )
    |> AnalysisParamHandler.addValue "2";
    AnalysisParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID UseDeltaScores)).Value
                                                    )
    |> AnalysisParamHandler.addValue "FALSE";
    AnalysisParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MaxQuant)).Value
                                                    );
    ]

let proteinDetectionProtocol =
    ProteinDetectionProtocolHandler.init(
        analysisSoftware, threshold
                                        )
    |> ProteinDetectionProtocolHandler.addAnalysisParams analysisParams
    |> ProteinDetectionProtocolHandler.addMzIdentMLDocument mzIdentMLDocument

let spectrumIdentificationProtocol =
    SpectrumIdentificationProtocolHandler.init(
        analysisSoftware, searchType, threshold
                                                )
    |> SpectrumIdentificationProtocolHandler.addEnzyme enzyme
    |> SpectrumIdentificationProtocolHandler.addModificationParams searchModificationParams
    |> SpectrumIdentificationProtocolHandler.addAdditionalSearchParams additionalSearchParams
    |> SpectrumIdentificationProtocolHandler.addFragmentTolerances fragmentTolerance
    |> SpectrumIdentificationProtocolHandler.addMzIdentMLDocument mzIdentMLDocument

let spectrumIdentificationResultParamUnModified =
    [
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID BasePeakIntension)).Value
                                                    )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ElapsedTime)).Value
                                                    )
    |> SpectrumIdentificationResultParamHandler.addValue "1";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MinScanNumber)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "15077";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MaxScanNumber)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "15329";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PeptideScore)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "105.0287018";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID TotalXIC)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "25278";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID XICAreas)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0;3563;15589;27011;28600;27211;19809;14169;10127;6641;1399;1375;0";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSCount)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "1";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSScanNumbers)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "15159";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSIsotopIndices)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ScanNumber)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "15159";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID IonInjectionTime)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID TotalIonCurrent)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "92047";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID FilteredPeaks)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "135";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ParentIntensityFraction)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.8484042";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID FractionOfTotalSpectrum)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.001671316";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID BasePeakFraction)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.07051102";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PrecursorFullScanNumbers)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "15140";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PrecursorIntensity)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "10131";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PrecursorApexFraction)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.905362";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PrecursorApexOffset)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "-2";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PrecursorApexOffsetTime)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.06315041";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ScanEventNumber)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "19";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ScoreDifference)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "NaN";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID NumberOfMatches)).Value
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "26";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID IntensityCoverage)).Value
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "0.5504808";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PeakCoverage)).Value
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "0.1851852";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ETDIdentificationType)).Value
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "Unknown";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID UniqueToProtein)).Value
                                               )
    ]

let spectrumIdentificationResultParamMOxidized =
    [
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID BasePeakIntension)).Value
                                                    )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ElapsedTime)).Value
                                                    )
    |> SpectrumIdentificationResultParamHandler.addValue "1";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MinScanNumber)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "12116";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MaxScanNumber)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "12347";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PeptideScore)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "69.04515839";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID TotalXIC)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "25120";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID XICAreas)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0;3731;6755;24524;24360;18177;11825;4543;4102.5;3066;2497;0";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSCount)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "1";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSScanNumbers)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "12193";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID MSMSIsotopIndices)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ScanNumber)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "12193";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID IonInjectionTime)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID TotalIonCurrent)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "71250";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID FilteredPeaks)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "157";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ParentIntensityFraction)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.8028493";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID FractionOfTotalSpectrum)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.002043774";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID BasePeakFraction)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0.04564729";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PrecursorFullScanNumbers)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "12179";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PrecursorIntensity)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "10056";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PrecursorApexFraction)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "1";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PrecursorApexOffset)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PrecursorApexOffsetTime)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "0";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ScanEventNumber)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "14";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ScoreDifference)).Value
                                                )
    |> SpectrumIdentificationResultParamHandler.addValue "69.045";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID NumberOfMatches)).Value
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "27";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID IntensityCoverage)).Value
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "0.4059861";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID PeakCoverage)).Value
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "0.1719745";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID ETDIdentificationType)).Value
                            )
    |> SpectrumIdentificationResultParamHandler.addValue "Unknown";
    SpectrumIdentificationResultParamHandler.init(
        (TermHandler.tryFindByID sqliteMzIdentMLContext (TermIDByName.toID UniqueToProtein)).Value
                                               )
    ]

let spectrumIdentificationResult =
    [
    SpectrumIdentificationResultHandler.init(
        spectradata.[0], "568", [spectrumidentificationItemPeptideMOxidized]
                                            )
    |> SpectrumIdentificationResultHandler.addDetails spectrumIdentificationResultParamMOxidized;
    SpectrumIdentificationResultHandler.init(
        spectradata.[1], "576", [spectrumidentificationItemPeptideUnmodified]
                                            )
    |> SpectrumIdentificationResultHandler.addDetails spectrumIdentificationResultParamUnModified
    ]
        
let spectrumIdentificationList =
    SpectrumIdentificationListHandler.init(spectrumIdentificationResult)
    |> SpectrumIdentificationListHandler.addFragmentationTables
        [
        (MeasureHandler.tryFindByID sqliteMzIdentMLContext "FragmentationTable measure-type").Value;
        (MeasureHandler.tryFindByID sqliteMzIdentMLContext "FragmentationTable measure-error").Value;
        ]

let spectrumIdentification = 
    SpectrumIdentificationHandler.init(
        spectrumIdentificationList, spectrumIdentificationProtocol, spectradata, [searchDatabase]
                                        )
    |>SpectrumIdentificationHandler.addMzIdentMLDocument mzIdentMLDocument

let proteinDetectionList =
    ProteinDetectionListHandler.init()
    |> ProteinDetectionListHandler.addProteinAmbiguityGroup proteinAmbiguousGroups

let proteinDetection =
    ProteinDetectionHandler.init(
        proteinDetectionList,proteinDetectionProtocol, [spectrumIdentificationList]
                                )
    |> ProteinDetectionHandler.addMzIdentMLDocument mzIdentMLDocument
            
let analysisData =
    AnalysisDataHandler.init([spectrumIdentificationList])
    |> AnalysisDataHandler.addProteinDetectionList proteinDetectionList
    |> AnalysisDataHandler.addMzIdentMLDocument mzIdentMLDocument

let sourceFiles =
    [SourceFileHandler.init("local", fileFormat2, name="20170518 TM FSconc3009"); SourceFileHandler.init("local", fileFormat3, name="D:\Fred\FASTA\sequence\Chlamy\Chlamy_JGI5_5(Cp_Mp)TM.fasta")]

let inputs =
    InputsHandler.init(spectradata)
    |> InputsHandler.addSourceFiles sourceFiles 
    |> InputsHandler.addSearchDatabase searchDatabase

let provider =
    ProviderHandler.init()
    |> ProviderHandler.addContactRole contactRole
    |> ProviderHandler.addAnalysisSoftware analysisSoftware
    |> ProviderHandler.addMzIdentMLDocument mzIdentMLDocument

let finalMzIdentMLDocument =
    mzIdentMLDocument
    |> MzIdentMLDocumentHandler.addName "Test MzIdentMLDatabase"
    |> MzIdentMLDocumentHandler.addAnalysisSoftware analysisSoftware
    |> MzIdentMLDocumentHandler.addDBSequence dbSequence
    |> MzIdentMLDocumentHandler.addPeptides [peptideUnmodified; peptideMOxidized]
    |> MzIdentMLDocumentHandler.addPeptideEvidences peptideEvidences
    |> MzIdentMLDocumentHandler.addProteinDetectionProtocol proteinDetectionProtocol
    |> MzIdentMLDocumentHandler.addSpectrumIdentificationProtocol spectrumIdentificationProtocol
    |> MzIdentMLDocumentHandler.addSpectrumIdentification spectrumIdentification
    |> MzIdentMLDocumentHandler.addProteinDetection proteinDetection
    |> MzIdentMLDocumentHandler.addAnalysisData analysisData
    |> MzIdentMLDocumentHandler.addInputs inputs
    |> MzIdentMLDocumentHandler.addProvider provider
    |> MzIdentMLDocumentHandler.addOrganizations organizations
    |> MzIdentMLDocumentHandler.addPerson person
    |> MzIdentMLDocumentHandler.addToContext sqliteMzIdentMLContext

sqliteMzIdentMLContext.SaveChanges()