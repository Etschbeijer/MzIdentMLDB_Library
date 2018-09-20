﻿    
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

//let user0 =
//    TermHandler.init("User:0000000")
//    |> TermHandler.addName "N-term cleavage window"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user1 =
//    TermHandler.init("User:0000001")
//    |> TermHandler.addName "C-term cleavage window"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user2 =
//    TermHandler.init("User:0000002")
//    |> TermHandler.addName "Leading razor protein"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user3 =
//    TermHandler.init("User:0000003")
//    |> TermHandler.addName "MaxQuant: Unique Protein Group"
//    |> TermHandler.addOntologyID  "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user4 =
//    TermHandler.init("User:0000004")
//    |> TermHandler.addName "MaxQuant: Unique Protein"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user5 =
//    TermHandler.init("User:0000005")
//    |> TermHandler.addName "MaxQuant: PEP"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user6 =
//    TermHandler.init("User:0000006")
//    |> TermHandler.addName "IdentificationType"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user7 =
//    TermHandler.init("User:0000007")
//    |> TermHandler.addName "AminoAcid modification"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user8 =
//    TermHandler.init("User:0000008")
//    |> TermHandler.addName "Charge corrected mass of the precursor ion"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user9 =
//    TermHandler.init("User:0000009")
//    |> TermHandler.addName "Calibrated retention time"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user10 =
//    TermHandler.init("User:0000010")
//    |> TermHandler.addName "Mass error"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user11 =
//    TermHandler.init("User:0000011")
//    |> TermHandler.addName "The intensity values of the isotopes."
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user12 =
//    TermHandler.init("User:0000012")
//    |> TermHandler.addName "MaxQuant: Major protein IDs"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user13 =
//    TermHandler.init("User:0000013")
//    |> TermHandler.addName "MaxQuant: Number of proteins"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user14 =
//    TermHandler.init("User:0000014")
//    |> TermHandler.addName "MaxQuant: Peptides"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user15 =
//    TermHandler.init("User:0000015")
//    |> TermHandler.addName "MaxQuant: Razor + unique peptides"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user16 =
//    TermHandler.init("User:0000016")
//    |> TermHandler.addName "MaxQuant: Unique peptides"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user17 =
//    TermHandler.init("User:0000017")
//    |> TermHandler.addName "MaxQuant: Unique + razor sequence coverage"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user18 =
//    TermHandler.init("User:0000018")
//    |> TermHandler.addName "MaxQuant: Unique sequence coverage"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user19 =
//    TermHandler.init("User:0000019")
//    |> TermHandler.addName "MaxQuant: SequenceLength(s)"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user20 =
//    TermHandler.init("User:0000020")
//    |> TermHandler.addName "Metabolic labeling N14/N15"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user21 =
//    TermHandler.init("User:0000021")
//    |> TermHandler.addName "DetectionType"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user22 =
//    TermHandler.init("User:0000022")
//    |> TermHandler.addName "MaxQuant: Uncalibrated m/z"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user23 =
//    TermHandler.init("User:0000023")
//    |> TermHandler.addName "MaxQuant: Number of data points"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user24 =
//    TermHandler.init("User:0000024")
//    |> TermHandler.addName "MaxQuant: Number of isotopic peaks"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user25 =
//    TermHandler.init("User:0000025")
//    |> TermHandler.addName "MaxQuant: Parent ion fraction"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user26 =
//    TermHandler.init("User:0000026")
//    |> TermHandler.addName "MaxQuant: Mass precision"
//    |> TermHandler.addOntologyID "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user27 =
//    TermHandler.init("User:0000027")
//    |> TermHandler.addName "Retention length (FWHM)"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user28 =
//    TermHandler.init("User:0000028")
//    |> TermHandler.addName "MaxQuant: Min scan number"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user29 =
//    TermHandler.init("User:0000029")
//    |> TermHandler.addName "MaxQuant: Max scan number"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user30 =
//    TermHandler.init("User:0000030")
//    |> TermHandler.addName "MaxQuant: MSMS scan numbers"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user31 =
//    TermHandler.init("User:0000031")
//    |> TermHandler.addName "MaxQuant: MSMS isotope indices"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user32 =
//    TermHandler.init("User:0000032")
//    |> TermHandler.addName "MaxQuant: Filtered peaks"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user33 =
//    TermHandler.init("User:0000033")
//    |> TermHandler.addName "FragmentationType"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user34 =
//    TermHandler.init("User:0000034")
//    |> TermHandler.addName "Parent intensity fraction"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user35 =
//    TermHandler.init("User:0000035")
//    |> TermHandler.addName "Fraction of total spectrum"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user36 =
//    TermHandler.init("User:0000036")
//    |> TermHandler.addName "Base peak fraction"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user37 =
//    TermHandler.init("User:0000037")
//    |> TermHandler.addName "Precursor full scan number"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user38 =
//    TermHandler.init("User:0000038")
//    |> TermHandler.addName "Precursor intensity"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user39 =
//    TermHandler.init("User:0000039")
//    |> TermHandler.addName "Precursor apex fraction"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user40 =
//    TermHandler.init("User:0000040")
//    |> TermHandler.addName "Precursor apex offset"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user41 =
//    TermHandler.init("User:0000041")
//    |> TermHandler.addName "Precursor apex offset time"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user42 =
//    TermHandler.init("User:0000042")
//    |> TermHandler.addName "Scan event number"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user43 =
//    TermHandler.init("User:0000043")
//    |> TermHandler.addName "MaxQuant: Score difference"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user44 =
//    TermHandler.init("User:0000044")
//    |> TermHandler.addName "MaxQuant: Combinatorics"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user45 =
//    TermHandler.init("User:0000045")
//    |> TermHandler.addName "MaxQuant: Matches"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user46 =
//    TermHandler.init("User:0000046")
//    |> TermHandler.addName "MaxQuant: Match between runs"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user47 =
//    TermHandler.init("User:0000047")
//    |> TermHandler.addName "MaxQuant: Number of matches"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user48 =
//    TermHandler.init("User:0000048")
//    |> TermHandler.addName "MaxQuant: Intensity coverage"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user49 =
//    TermHandler.init("User:0000049")
//    |> TermHandler.addName "MaxQuant: Peak coverage"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user50 =
//    TermHandler.init("User:0000050")
//    |> TermHandler.addName "MaxQuant: ETD identification type"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user51 =
//    TermHandler.init("User:0000051")
//    |> TermHandler.addName "Min. score unmodified peptides"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext
  
//let user52 =
//    TermHandler.init("User:0000052")
//    |> TermHandler.addName "Min. score modified peptides"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user53 =
//    TermHandler.init("User:0000053")
//    |> TermHandler.addName "Min. delta score of unmodified peptides"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext 

//let user54 =
//    TermHandler.init("User:0000054")
//    |> TermHandler.addName "Min. delta score of modified peptides"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user55 =
//    TermHandler.init("User:0000055")
//    |> TermHandler.addName "Min. amount unique peptide"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user56 =
//    TermHandler.init("User:0000056")
//    |> TermHandler.addName "Min. amount razor peptide"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user57 =
//    TermHandler.init("User:0000057")
//    |> TermHandler.addName "Min. amount peptide"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user58 =
//    TermHandler.init("User:0000058")
//    |> TermHandler.addName "MaxQuant: Decoy mode"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user59 =
//    TermHandler.init("User:0000059")
//    |> TermHandler.addName "MaxQuant: Special AAs"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user60 =
//    TermHandler.init("User:0000060")
//    |> TermHandler.addName "MaxQuant: Include contaminants"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user61 =
//    TermHandler.init("User:0000061")
//    |> TermHandler.addName "MaxQuant: iBAQ"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user62 =
//    TermHandler.init("User:0000062")
//    |> TermHandler.addName "MaxQuant: Top MS/MS peaks per 100 Dalton"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user63 =
//    TermHandler.init("User:0000063")
//    |> TermHandler.addName "MaxQuant: IBAQ log fit"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user64 =
//    TermHandler.init("User:0000064")
//    |> TermHandler.addName "MaxQuant: Protein FDR"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user65 =
//    TermHandler.init("User:0000065")
//    |> TermHandler.addName "MaxQuant: SiteFDR"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user66 =
//    TermHandler.init("User:0000066")
//    |> TermHandler.addName "MaxQuant: Use Normalized Ratios For Occupancy"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user67 =
//    TermHandler.init("User:0000067")
//    |> TermHandler.addName "MaxQuant: Peptides used for protein quantification"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user68 =
//    TermHandler.init("User:0000068")
//    |> TermHandler.addName "MaxQuant: Discard unmodified counterpart peptides"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user69 =
//    TermHandler.init("User:0000069")
//    |> TermHandler.addName "MaxQuant: Min. ratio count"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user70 =
//    TermHandler.init("User:0000070")
//    |> TermHandler.addName "MaxQuant: Use delta score"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user71 =
//    TermHandler.init("User:0000071")
//    |> TermHandler.addName "Data-dependt acquisition"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user72 =
//    TermHandler.init("User:0000072")
//    |> TermHandler.addName "razor-protein"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user73 =
//    TermHandler.init("User:0000073")
//    |> TermHandler.addName "razor-peptide"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user74 =
//    TermHandler.init("User:0000074")
//    |> TermHandler.addName "Mass-deviation"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user75 =
//    TermHandler.init("User:0000075")
//    |> TermHandler.addName "leading-peptide"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user76 =
//    TermHandler.init("User:0000076")
//    |> TermHandler.addName "unique-protein"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user77 =
//    TermHandler.init("User:0000077")
//    |> TermHandler.addName "unique-peptide"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user78 =
//    TermHandler.init("User:0000078")
//    |> TermHandler.addName "MaxQuant: Delta score"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

//let user79 =
//    TermHandler.init("User:0000079")
//    |> TermHandler.addName "MaxQuant: Best andromeda score"
//    |> TermHandler.addOntologyID 
//        "UserParam"
//    |> TermHandler.addToContextAndInsert sqliteMzQuantMLContext

type TermSymbol =
    | Accession of string

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
    ///Modifications of the Protein.
    | ProteinModifications
    ///Other members of the protein-ambiguity-group
    | ProteinAmbiguityGroupMembers

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
    ///Number of different peptide-sequences.
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

    static member toID (item:TermSymbol) =
        match item with
        | Accession s -> s
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
        | ProteinModifications -> "MS:1000933"
        | ProteinAmbiguityGroupMembers -> "PRIDE:0000418"

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
    RawFileHandler.init("local")
    |> RawFileHandler.addName "20170518 TM FSconc3001"
    |> RawFileHandler.addFKFileFormat "UnknownFileType"
    |> RawFileHandler.addFKRawFilesGroup "RawFilesGroup 1"
    |> RawFileHandler.addToContext sqliteMzQuantMLContext;
    RawFileHandler.init("local")
    |> RawFileHandler.addName "20170518 TM FSconc3002"
    |> RawFileHandler.addFKFileFormat "UnknownFileType"
    |> RawFileHandler.addFKRawFilesGroup "RawFilesGroup 1"
    |> RawFileHandler.addToContext sqliteMzQuantMLContext;
    RawFileHandler.init("local")
    |> RawFileHandler.addName "D:\Fred\FASTA\sequence\Chlamy\Chlamy_JGI5_5(Cp_Mp)TM.fasta"
    |> RawFileHandler.addFKFileFormat "FASTA"
    |> RawFileHandler.addFKRawFilesGroup "RawFilesGroup 1"
    |> RawFileHandler.addToContext sqliteMzQuantMLContext;
    ]

let identificationFile1 =
    IdentificationFileHandler.init("local", "IdentificationFile 1")
    |> IdentificationFileHandler.addName "20170518 TM FSconc3009"
    |> IdentificationFileHandler.addFKSearchDatabase "SearchDataBase 1"
    |> IdentificationFileHandler.addFKInputFiles "InputFiles 1"
    |> IdentificationFileHandler.addToContext sqliteMzQuantMLContext

let rawFilesGroup =
    RawFilesGroupHandler.init([null], "RawFilesGroup 1")
    |> RawFilesGroupHandler.addFKInputFiles "InputFiles 1"
    |> RawFilesGroupHandler.addToContext sqliteMzQuantMLContext

let inputFiles =
    InputFilesHandler.init("InputFiles 1")
    |> InputFilesHandler.addToContext sqliteMzQuantMLContext

let modifications =
    [
    ModificationHandler.init(
        CVParamHandler.init(
            (TermSymbol.toID Oxidation)
                            ).ID
                             )
    |> ModificationHandler.addResidues "M"
    |> ModificationHandler.addMassDelta 16000.
    |> ModificationHandler.addFKPeptideConsensus "PeptideConsensusMOxidized 1";
    ModificationHandler.init(
        CVParamHandler.init(
            (TermSymbol.toID NeutralIonLoss)
                           ).ID
                            )
    |> ModificationHandler.addFKPeptideConsensus "PeptideConsensusMOxidized 1";
    ]
    |> (fun item -> sqliteMzQuantMLContext.AddRange(item.Cast()))

let label =
    ModificationHandler.init(
        CVParamHandler.init(
            (TermSymbol.toID MetabolicLabelingN14N15)
                           ).ID
                             , "Label 1")
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
    AssayHandler.init("Assay 1")
    |> AssayHandler.addFKRawFilesGroup "RawFilesGroup 1"
    |> AssayHandler.addFKIdentificationFile "IdentificationFile 1"
    |> AssayHandler.addFkMzQuantMLDocument "Test1"
    |> AssayHandler.addFKEvidenceRef "EvidenceRefUnmodified 1"
    |> AssayHandler.addFKIdentificationFile "IdentificationFile 1"
    |> AssayHandler.addToContext sqliteMzQuantMLContext

let featureUnmodified =
    FeatureHandler.init(2, 727.84714, 25.752, "FeatureUnmodified 1")
    |> FeatureHandler.addFKSpectrum "576"
    |> FeatureHandler.addToContext sqliteMzQuantMLContext

let evidenceRefUnmodified =
    EvidenceRefHandler.init([null], "FeatureUnmodified 1", "EvidenceRefUnmodified 1")
    |> EvidenceRefHandler.addFKIdentificationFile "IdentificationFile 1"
    |> EvidenceRefHandler.addFKPeptideConsensus "PeptideConsensusUnmodified 1"
    |> EvidenceRefHandler.addToContext sqliteMzQuantMLContext

let peptideConsensusUnmodified =
    PeptideConsensusHandler.init(2, [null], "PeptideConsensusUnmodified 1")
    |> PeptideConsensusHandler.addFKSearchDatabase "SearchDataBase 1"
    |> PeptideConsensusHandler.addPeptideSequence "AAIEASFGSVDEMK"
    |> PeptideConsensusHandler.addFKPeptideConsensusList "List1"
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
    AssayHandler.init("Assay 2")
    |> AssayHandler.addFKRawFilesGroup "RawFilesGroup 1"
    |> AssayHandler.addFKIdentificationFile "IdentificationFile 1"
    |> AssayHandler.addFkMzQuantMLDocument "Test1"
    |> AssayHandler.addFKEvidenceRef "FeatureMOxidized 1"
    |> AssayHandler.addFKIdentificationFile "IdentificationFile 1"
    |> AssayHandler.addToContext sqliteMzQuantMLContext

let featureMOxidized =
    FeatureHandler.init(2, 735.84531, 21.489, "FeatureMOxidized 1")
    |> FeatureHandler.addFKSpectrum "576"
    |> FeatureHandler.addToContext sqliteMzQuantMLContext

let evidenceRefMOxidized =
    EvidenceRefHandler.init([null], "FeatureMOxidized 1")
    |> EvidenceRefHandler.addFKIdentificationFile "IdentificationFile 1"
    |> EvidenceRefHandler.addFKPeptideConsensus "PeptideConsensusMOxidized 1"
    |> EvidenceRefHandler.addToContext sqliteMzQuantMLContext

let peptideConsensusMOxidized =
    PeptideConsensusHandler.init(2, [null], "PeptideConsensusMOxidized 1")
    |> PeptideConsensusHandler.addPeptideSequence "AAIEASFGSVDEMK"
    |> PeptideConsensusHandler.addFKSearchDatabase "SearchDataBase 1"
    |> PeptideConsensusHandler.addFKPeptideConsensusList "List1"
    |> PeptideConsensusHandler.addFKProtein "ProteinMOxidized"
    |> PeptideConsensusHandler.addToContext sqliteMzQuantMLContext

let peptideConsensusList =
    PeptideConsensusListHandler.init(true, [null], "List1")
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
        "Cre02.g096150.t1.2", "SearchDataBase 1", "ProteinUnmodified"
                       )
    |> ProteinHandler.addFKProteinList "ProteinList"
    ProteinHandler.init(
        "Cre02.g096150.t1.2", "SearchDataBase 1", "ProteinMOxidized"
                       )

    |> ProteinHandler.addFKProteinList "ProteinList"
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
    ProteinRefHandler.init("ProteinRefUnmodified")
    |> ProteinRefHandler.addFKProteinGroup "ProteinGroup 1"
    |> ProteinRefHandler.addToContext sqliteMzQuantMLContext

let proteinRef2 =
    ProteinRefHandler.init("ProteinRefMOxidized")
    |> ProteinRefHandler.addFKProteinGroup "ProteinGroup 1"
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

let createTestPeptideParam (n:int) =
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
    EvidenceRefHandler.init([null], "FeatureMOxidized 1")
    |> EvidenceRefHandler.addFKIdentificationFile "IdentificationFile 1"
    |> EvidenceRefHandler.addFKPeptideConsensus "PeptideConsensusMOxidized 1"
    |> EvidenceRefHandler.addToContext sqliteMzQuantMLContext

let testPeptideConsensis (n:int) =
    PeptideConsensusHandler.init(3, [null])
    |> PeptideConsensusHandler.addPeptideSequence "AAIEASFGSVDEMK"
    |> PeptideConsensusHandler.addFKPeptideConsensusList "List1"
    |> PeptideConsensusHandler.addFKProtein (string n)
    |> PeptideConsensusHandler.addDetails (createTestPeptideParam n)

let createMultiplePeptides collection =
    let rec loop someThings n i =
        if n < 1000 then
            if i>10 then
                loop someThings (n+1) 0
            else
                loop (List.append someThings [(testPeptideConsensis n)]) n (i+1)
        else someThings
    loop collection 0 0
   
let tenThousandPeptides = 
    createMultiplePeptides []
    |> (fun item -> sqliteMzQuantMLContext.AddRange(item.Cast()))

let testProtein n =
    ProteinHandler.init("Test", "TestSearchDB 1", (string n))
    |> ProteinHandler.addFKProteinList "ProteinList"
    |> ProteinHandler.addDetails (createTestProteinParams n)

let rec createMultipleProteins collection n =
    if n < 1000 then 
        createMultipleProteins (List.append collection [testProtein n]) (n+1)
    else collection
createMultipleProteins

let tenThousandProteins = 
    createMultipleProteins [] 0
    |> (fun item -> sqliteMzQuantMLContext.AddRange(item.Cast()))

let proteinList =
    ProteinListHandler.init([null], "ProteinList")
    |> ProteinListHandler.addToContext sqliteMzQuantMLContext

let proteinGroup =
    ProteinGroupHandler.init(
        "SearchDataBase 1", [null], "ProteinGroup 1"
                            )
    |> ProteinGroupHandler.addFKProteinGroupList "ProteinGroupList"
    |> ProteinGroupHandler.addToContext sqliteMzQuantMLContext

let proteinGroupList =
    ProteinGroupListHandler.init([null], "ProteinGroupList")
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
    |> MzQuantMLDocumentHandler.addToContextAndInsert sqliteMzQuantMLContext


//sqliteMzQuantMLContext.SaveChanges()

sqliteMzQuantMLContext.Database.CloseConnection()