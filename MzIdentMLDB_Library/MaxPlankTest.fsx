    
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

type PSIMSTermIDByName =
    //AllPeptides
    | RawFile
    | MZRatio
    | RetentionTime
    | RetentionLength
    | PeptideScore
    | HigherScoreBetter
    | LowerScoreBetter
    | MSMSCount
    | Sequence
    | ModifiedSequence
    | SequenceLength
    | FullWithAtHalfMaximum
    | Intensity
    // Evidence
    | LeadingProteins
    | PrecursorIon
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
    static member toString (item:PSIMSTermIDByName) =
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
        | DeltaScore -> "MS:1002536"
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
