    
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

//Peptides ID=119; Modification-specific peptides IDs=123 & 124

let mzIdentMLDocument =
    MzIdentMLDocumentHandler.init(version="1.0", id="Test1")

let measureErrors =
    [
    MeasureParamHandler.init(
        (TermSymbol.toID MassErrorPpm)
                            )
    |> MeasureParamHandler.addUnit 
        (TermSymbol.toID Ppm);
    MeasureParamHandler.init(
        (TermSymbol.toID MassErrorPercent)
                            )
    |> MeasureParamHandler.addUnit 
        (TermSymbol.toID Percentage);
    MeasureParamHandler.init(
        (TermSymbol.toID MassErrorDa)
                            )
    |> MeasureParamHandler.addUnit 
        (TermSymbol.toID Dalton);
    MeasureParamHandler.init(
        (TermSymbol.toID SearchToleranceUpperLimit),
            "Mass Deviations upper limit [ppm]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermSymbol.toID Ppm);
    MeasureParamHandler.init(
        (TermSymbol.toID SearchToleranceLowerLimit),
            "Mass Deviations lower limit [ppm]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermSymbol.toID Ppm);
    MeasureParamHandler.init(
        (TermSymbol.toID MassDeviation),
            "Mass Deviation [ppm]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermSymbol.toID Ppm);
    MeasureParamHandler.init(
        (TermSymbol.toID SearchToleranceUpperLimit),
            "Mass Deviations upper limit [Da]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermSymbol.toID Dalton);
    MeasureParamHandler.init(
        (TermSymbol.toID SearchToleranceLowerLimit),
            "Mass Deviations lower limit [Da]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermSymbol.toID Dalton);
    MeasureParamHandler.init(
        (TermSymbol.toID MassDeviation),
            "Mass Deviation [Da]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermSymbol.toID Dalton);
    MeasureParamHandler.init(
        (TermSymbol.toID ProductIonErrorMZ),
            "Product-ion error [M/Z]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermSymbol.toID MZ); 
    ]

let measureTypes =
    [
    MeasureParamHandler.init(
        (TermSymbol.toID ProductIonMZ),
            "Product-ion [M/Z]"
                            )
    |> MeasureParamHandler.addUnit 
        (TermSymbol.toID MZ);                       
    MeasureParamHandler.init(
        (TermSymbol.toID ProductIonIntensity),
            "Intensity"
                            )
    |> MeasureParamHandler.addUnit 
        (TermSymbol.toID NumberOfDetections);
    ]

MeasureHandler.init(measureTypes, "FragmentationTable measure-type")
|> MeasureHandler.addToContext sqliteMzIdentMLContext

MeasureHandler.init(measureErrors, "FragmentationTable measure-error")
|> MeasureHandler.addToContext sqliteMzIdentMLContext

let fileFormat1 =
    CVParamHandler.init(
        (TermSymbol.toID DataStoredInDataBase)
                        )

let fileFormat2 =
    CVParamHandler.init(
        (TermSymbol.toID UnknownFileType)
                        )

let fileFormat3 =
    CVParamHandler.init(
        (TermSymbol.toID FASTAFormat)
                        )

let databaseName =
    CVParamHandler.init(
        (TermSymbol.toID DataBaseName)
                        )
    |> CVParamHandler.addValue "Unknown to me at the moment"

let searchDataBaseParam =
    SearchDatabaseParamHandler.init(
        (TermSymbol.toID DatabaseVersion)
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
        (TermSymbol.toID TaxidNCBI)
                               )
    |> DBSequenceParamHandler.addValue "906914";
    DBSequenceParamHandler.init(
        (TermSymbol.toID ScientificName)
                               )
    |> DBSequenceParamHandler.addValue "C. reinhardtii";
    ]

let dbSequence =
    DBSequenceHandler.init("Test", searchDatabase)
    |> DBSequenceHandler.addSequence "AAIEASFGSVDEMK"
    |> DBSequenceHandler.addLength 14
    |> DBSequenceHandler.addDetails dbSequenceParams
    |> DBSequenceHandler.addFkMzIdentMLDocument mzIdentMLDocument.ID

let searchDatabase1 =
    SearchDatabaseHandler.init(
        "local", fileFormat3, databaseName
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
    DBSequenceHandler.init("Cre02.g096150.t1.2", searchDatabase1)
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
    |> PeptideParamHandler.addUnit
        (TermSymbol.toID KiloDalton);
    PeptideParamHandler.init(
        (TermSymbol.toID MonoIsotopicMass)
                            )
    |> PeptideParamHandler.addValue "1453.6759";
    ]

let peptideUnmodified =
    PeptideHandler.init("AAIEASFGSVDEMK", "119")
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
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermSymbol.toID Minute);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID CalibratedRetentionTime)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "25.664"
    |> SpectrumIdentificationItemParamHandler.addUnit
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
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermSymbol.toID Ppm);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID MzDeltaScore)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "727.8458225";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID RetentionLength)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "22.688"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermSymbol.toID Minute);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID RetentionTimeFWHM)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "11.349"
    |> SpectrumIdentificationItemParamHandler.addUnit
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
        (TermSymbol.toID Fragment)
                            );
    IonTypeParamHandler.init(
        (TermSymbol.toID YIon)
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
        peptideUnmodified, 2, 727.84714, true, 0
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
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermSymbol.toID Minute);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID CalibratedRetentionTime)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "21.372"
    |> SpectrumIdentificationItemParamHandler.addUnit
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
    |> SpectrumIdentificationItemParamHandler.addUnit
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
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermSymbol.toID Ppm);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID MzDeltaScore)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "735.8475942";
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID RetentionLength)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "20.649"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermSymbol.toID Minute);
    SpectrumIdentificationItemParamHandler.init(
        (TermSymbol.toID RetentionTimeFWHM)
                                                )
    |> SpectrumIdentificationItemParamHandler.addValue "7.509"
    |> SpectrumIdentificationItemParamHandler.addUnit
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
        (TermSymbol.toID Fragment)
                            );
    IonTypeParamHandler.init(
        (TermSymbol.toID YIon)
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
    |> PeptideEvidenceHandler.addFkMzIdentMLDocument mzIdentMLDocument.ID;
    PeptideEvidenceHandler.init(
        dbSequence, peptideMOxidized
                                )
    |> PeptideEvidenceHandler.addStart 106
    |> PeptideEvidenceHandler.addEnd 119
    |> PeptideEvidenceHandler.addFkMzIdentMLDocument mzIdentMLDocument.ID
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
        true, dbSequence, [peptideHypothesisUnModified; peptideHypothesisMOxidized], "Cre02.g096150.t1.2", "Cre02.g096150.t1.2"
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
    |> ProteinAmbiguityGroupParamHandler.addUnit
        (TermSymbol.toID Percentage);
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID UniqueAndRazorSequenceCoverage)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "31.7"
    |> ProteinAmbiguityGroupParamHandler.addUnit
        (TermSymbol.toID Percentage);
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID UniqueSequenceCoverage)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "31.7"
    |> ProteinAmbiguityGroupParamHandler.addUnit
        (TermSymbol.toID Percentage);
    ProteinAmbiguityGroupParamHandler.init(
        (TermSymbol.toID MolecularWeight)
                                            )
    |> ProteinAmbiguityGroupParamHandler.addValue "23.9"
    |> ProteinAmbiguityGroupParamHandler.addUnit
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
        (TermSymbol.toID PSMFDR)
                                        )
    |> ProteinAmbiguityGroupParamHandler.addValue "0.01";
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
        (TermSymbol.toID MultipleIDs)
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
    OrganizationHandler.init(name="TuKL");
    OrganizationHandler.init(name="BioTech");
    OrganizationHandler.init(name="CSB");
    ]

let person =
    PersonHandler.init()
    |> PersonHandler.addFirstName "Patrick"
    |> PersonHandler.addLastName "Blume"
    |> PersonHandler.addOrganizations organizations

let role =
    CVParamHandler.init("MS:1001267")

let contactRole =
    ContactRoleHandler.init(person, role)

let softwareName =
    CVParamHandler.init((TermSymbol.toID MaxQuant))

let analysisSoftware =
    AnalysisSoftwareHandler.init(softwareName)
    |> AnalysisSoftwareHandler.addAnalysisSoftwareDeveloper contactRole

let searchType =
    CVParamHandler.init(
        (TermSymbol.toID MSMSSearch)
                        )

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
    |> SearchModificationParamHandler.addUnit
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
    |> SearchModificationParamHandler.addUnit
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
    |> SearchModificationParamHandler.addUnit
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
    |> SearchModificationParamHandler.addUnit
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
        false, 15.9949, "M", searchModificationParam
                                    );
    SearchModificationHandler.init(
        false, 43.0, "N-Term", searchModificationParam
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
    |> FragmentToleranceParamHandler.addUnit 
        (TermSymbol.toID Ppm);
    FragmentToleranceParamHandler.init(
        (TermSymbol.toID SearchToleranceLowerLimit),
            "Mass Deviations lower limit [ppm]"
                            )
    |> FragmentToleranceParamHandler.addUnit 
        (TermSymbol.toID Ppm);
    FragmentToleranceParamHandler.init(
        (TermSymbol.toID SearchToleranceUpperLimit),
            "Mass Deviations upper limit [Da]"
                            )
    |> FragmentToleranceParamHandler.addUnit 
        (TermSymbol.toID Dalton);
    FragmentToleranceParamHandler.init(
        (TermSymbol.toID SearchToleranceLowerLimit),
            "Mass Deviations lower limit [Da]"
                            )
    |> FragmentToleranceParamHandler.addUnit 
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
        analysisSoftware, threshold
                                        )
    |> ProteinDetectionProtocolHandler.addAnalysisParams analysisParams
    |> ProteinDetectionProtocolHandler.addFkMzIdentMLDocument mzIdentMLDocument.ID

let spectrumIdentificationProtocol =
    SpectrumIdentificationProtocolHandler.init(
        analysisSoftware, searchType, threshold
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
    |>SpectrumIdentificationHandler.addFkMzIdentMLDocument mzIdentMLDocument.ID

let proteinDetectionList =
    ProteinDetectionListHandler.init()
    |> ProteinDetectionListHandler.addProteinAmbiguityGroup proteinAmbiguousGroups

let proteinDetection =
    ProteinDetectionHandler.init(
        proteinDetectionList,proteinDetectionProtocol, [spectrumIdentificationList]
                                )
            
let analysisData =
    AnalysisDataHandler.init([spectrumIdentificationList])
    |> AnalysisDataHandler.addProteinDetectionList proteinDetectionList

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