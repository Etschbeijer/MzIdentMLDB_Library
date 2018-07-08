    
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
open BioFSharp

//open MzIdentMLDataBase.XMLParsing


let fileDir = __SOURCE_DIRECTORY__
let standardDBPathSQLite = fileDir + "\Databases\Test.db"

let sqliteContext = ContextHandler.sqliteConnection standardDBPathSQLite

type TermIDByName =
    //AllPeptides
    | RawFile
    | MZRatio
    | RetentionTime
    | RetentionLength
    | PeptideScore
    | HigherScoreBetter
    | LowerScoreBetter
    | MSMSCount
    ///Unmodified peptide sequence.
    | Sequence
    ///Modified peptide sequence.
    | ModifiedSequence
    | SequenceLength
    | FullWithAtHalfMaximum
    | Intensity
    // Evidence
    | LeadingProteins
    | PrecursorIon
    | MassErrorByppm
    | MassErrorByPercent
    | MassErrorBymmu
    | MassErrorByDaltons
    ///Proteins the peptide is associated with.
    | Protein
    | MonoisotopicMass
    | MassErrorPPM
    | MassErrorDaltons
    | ElutionTime
    | BasePeakFraction
    | AndromedaScore
    | DeltaScore
    | MaxModificationsPerPeptide
    | Reverse
    | LeadingProtein
    | ChargeState
    | PosteriorErrorProbaility
    //ProteinGroups
    | PeptideCounts
    | PeptideCountsWithRazorAndUnique
    | PeptideCountsWithUnique
    | SequenceCoverage
    | MolecularWeight
    | KiloDalton
    | QValue
    | ProteinDescription //FASTA Headers
    | ProteinScore
    | Decoy //For Reverse
    | Calibrated //Internal calibration
    //User
    | RetentionTimeWindowWith
    | SequestScore
    | IsotopicPatternArea
    | XICArea
    | Oxidation
    | Probability // Probability for modification
    | Fraction
    | PValue
    | DatabaseName
    | Spectrum
    | BasePeak
    | MSLevel
    | ScanNumber
    static member toID (item:TermIDByName) =
        match item with
        | RawFile -> "MS:1000577"
        | MZRatio -> "MS:1000040"
        | RetentionTime -> "MS:1000894"
        | PeptideScore -> "MS:1001950"
        | HigherScoreBetter -> "MS:1002108"
        | LowerScoreBetter -> "MS:1002109"
        | MSMSCount -> "MS:1001904"
        | ModifiedSequence -> "MS:1000889"
        | Sequence -> "MS:1000888"
        | SequenceLength -> "PEFF:0001006"
        | LeadingProteins -> "MS:1002401"
        | PrecursorIon -> "PRIDE:0000263"
        | MonoisotopicMass -> "MS:1000225"
        | MassErrorPPM -> "PRIDE:0000083"
        | MassErrorDaltons -> "PRIDE:0000086"
        | ElutionTime -> "MS:1000826"
        | BasePeakFraction -> "MS:1000132"
        | PValue -> "MS:1001191"
        | AndromedaScore -> "MS:1002338"
        | DeltaScore -> "MS:1002263"
        | MaxModificationsPerPeptide -> "MS:1001673"
        | Reverse -> "MS:1001195"
        | PeptideCounts -> "MS:1001898"
        | PeptideCountsWithRazorAndUnique -> "MS:1001899"
        | PeptideCountsWithUnique -> "MS:1001897"
        | SequenceCoverage -> "MS:1001093"
        | MolecularWeight -> "PRIDE:0000057"
        | KiloDalton -> "UO:0000222"
        | QValue -> "MS:1001491"
        | FullWithAtHalfMaximum -> "MS:1000086"
        | Intensity -> "MS:1002412"
        | RetentionLength -> "MS:1000826"
        | RetentionTimeWindowWith -> "MS:1001907"
        | SequestScore -> "PRIDE:0000053"
        | IsotopicPatternArea -> "MS:1001846"
        | XICArea -> "MS:1001858"
        | Oxidation -> "UNIMOD:35"
        | Probability -> "MS:1001876"
        | LeadingProtein -> "MS:1002401"
        | ChargeState -> "MS:1000041"
        | Fraction -> "UO:0000191"
        | ProteinDescription -> "MS:1001088"
        | ProteinScore -> "MS:1001951"
        | Decoy -> "PEFF:0000011"
        | Calibrated -> "MS:1000759"
        | PosteriorErrorProbaility -> "MS:1002192"
        | Protein -> "MS:1000882"
        | DatabaseName -> "MS:1001013"
        | MassErrorByppm -> "PRIDE:0000083"
        | MassErrorByPercent -> "PRIDE:0000085"
        | MassErrorBymmu -> "PRIDE:0000084"
        | MassErrorByDaltons -> "PRIDE:0000086"
        | Spectrum -> "MS:1000442"
        | BasePeak -> "MS:1000210"
        | MSLevel -> "MS:1000511"
        | ScanNumber -> "MS:1001115"

//Terms of userParams
let user1 =
    TermHandler.init("User:0000001")
    |> TermHandler.addName "Leading razor protein"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user2 = 
    TermHandler.init("User:0000002")
    |> TermHandler.addName "SQLite"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user3 (dbContext:MzIdentML) =
    TermHandler.init("User:0000003")
    |> TermHandler.addName "parent ion"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user4 =
    TermHandler.init("User:0000004")
    |> TermHandler.addName "MS/MS"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user5 =
    TermHandler.init("User:0000005")
    |> TermHandler.addName "Count"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user6 =
    TermHandler.init("User:0000006")
    |> TermHandler.addName "Isotopic cluster"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

//Evidence of line 36

let modification (dbContext:MzIdentML) =
    ModificationHandler.init(
        [
        ModificationParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID Sequence)).Value
                                     )
        |> ModificationParamHandler.addValue "AAAASSEVPDMNK";
        ModificationParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID ModifiedSequence)).Value
                                     )
        |> ModificationParamHandler.addValue "_AAAASSEVPDM(ox)NK_";
        ModificationParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID SequenceLength)).Value
                                     )
        |> ModificationParamHandler.addValue "13";
        ModificationParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID Oxidation)).Value
                                     )
        |> ModificationParamHandler.addValue "1"
        ]
                            )
    |> ModificationHandler.addResidues "M"
    |> ModificationHandler.addMonoIsotopicMassDelta 1305.58708

let searchDatabase (dbContext:MzIdentML) =
    SearchDatabaseHandler.init(
        "local",
        CVParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID RawFile)).Value
                           )
        |> CVParamHandler.addUnit
            (TermHandler.tryFindByID dbContext "User:0000002").Value,
        CVParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID DatabaseName)).Value
                           )    
        |> CVParamHandler.addValue "Test.db"
                               )

let dbSequence (dbContext:MzIdentML) =
    DBSequenceHandler.init("AAAASSEVPDMNK",searchDatabase dbContext)
    |> DBSequenceHandler.addSequence "AAAASSEVPDMNK"
    |> DBSequenceHandler.addLength 13

let peptide (dbContext:MzIdentML) =
    PeptideHandler.init("AAAASSEVPDMNK")
    |> PeptideHandler.addModification (modification dbContext)
    |> PeptideHandler.addDetails 
        [
         PeptideParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID Protein)).Value
                                 )
         |> PeptideParamHandler.addValue "Cre11.g467689.t1.1";
         PeptideParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID LeadingProteins)).Value
                                 )
         |> PeptideParamHandler.addValue "Cre11.g467689.t1.1";
         PeptideParamHandler.init(
            (TermHandler.tryFindByID dbContext "User:0000002").Value
                                 )
         |> PeptideParamHandler.addValue "Cre11.g467689.t1.1"
        ]

let massTable (dbContext:MzIdentML) =
    MassTableHandler.init("isotope cluster itentified by MS/MS")
    |> MassTableHandler.addDetails
        [
         (MassTableParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID MassErrorByDaltons)).Value
                                    )
          |> MassTableParamHandler.addValue "0.00055673"
         )
        ]

let peptideEvidence (dbContext:MzIdentML) =
    PeptideEvidenceHandler.init(dbSequence dbContext, peptide dbContext)
    |> PeptideEvidenceHandler.addDetails
        [
         PeptideEvidenceParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID RetentionTime)).Value
                                         )
         |> PeptideEvidenceParamHandler.addValue "80.184";
         PeptideEvidenceParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID RetentionLength)).Value
                                         )
         |> PeptideEvidenceParamHandler.addValue "0.31173"                           
        ]

let spectrumIdentificationItem (dbContext:MzIdentML) =
    SpectrumIdentificationItemHandler.init(peptide dbContext, 2, 653.800817, true, 0, "36")
    |> SpectrumIdentificationItemHandler.addMassTable (massTable dbContext)
    |> SpectrumIdentificationItemHandler.addDetails
        [
         SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID dbContext "User:0000003").Value
                                                    )
         |> SpectrumIdentificationItemParamHandler.addUnit
                (TermHandler.tryFindByID dbContext (TermIDByName.toID Fraction)).Value
         |> SpectrumIdentificationItemParamHandler.addValue "0.251157581806183";
         SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID Spectrum)).Value
                                                    )
         |> SpectrumIdentificationItemParamHandler.addUnit
                (TermHandler.tryFindByID dbContext (TermIDByName.toID Fraction)).Value
         |> SpectrumIdentificationItemParamHandler.addValue "0.000860746193211526";
         SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID BasePeak)).Value
                                                    )
         |> SpectrumIdentificationItemParamHandler.addUnit
                (TermHandler.tryFindByID dbContext (TermIDByName.toID Fraction)).Value
         |> SpectrumIdentificationItemParamHandler.addValue "0.0242771115154028";
         SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID PosteriorErrorProbaility)).Value
                                                    )
         |> SpectrumIdentificationItemParamHandler.addValue "0.00021505";
         SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID dbContext "User:0000004").Value
                                                    )
         |> SpectrumIdentificationItemParamHandler.addUnit
            (TermHandler.tryFindByID dbContext "User:0000005").Value
         |> SpectrumIdentificationItemParamHandler.addValue "1";
         SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID dbContext "User:0000001").Value
                                                    )
         |> SpectrumIdentificationItemParamHandler.addUnit
                (TermHandler.tryFindByID dbContext (TermIDByName.toID ScanNumber)).Value
         |> SpectrumIdentificationItemParamHandler.addValue "3207";
         SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID AndromedaScore)).Value
                                                    )
         |> SpectrumIdentificationItemParamHandler.addValue "55.896";
         SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID dbContext (TermIDByName.toID DeltaScore)).Value
                                                    )
         |> SpectrumIdentificationItemParamHandler.addValue "49.28";
         SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID dbContext "User:0000006").Value
                                                    )
         |> SpectrumIdentificationItemParamHandler.addUnit
                (TermHandler.tryFindByID dbContext (TermIDByName.toID XICArea)).Value
         |> SpectrumIdentificationItemParamHandler.addValue "3666";
        ]