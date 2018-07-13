    
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
    | ProteinAccession
    | PeptideCounts
    | PeptideCountsWithRazorAndUnique
    | PeptideCountsWithUnique
    | SequenceCoverage
    | MolecularWeight
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
    //Unit-types
    | Percent
    | KiloDalton
    | Minute
    | Second
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
        | MassErrorByPercent -> "PRIDE:0000085"
        | MassErrorBymmu -> "PRIDE:0000084"
        | MassErrorByDaltons -> "PRIDE:0000086"
        | Spectrum -> "MS:1000442"
        | BasePeak -> "MS:1000210"
        | MSLevel -> "MS:1000511"
        | ScanNumber -> "MS:1001115"
        | ProteinAccession -> "MS:1000885"
        | Percent -> "UO:0000187"
        | Minute -> "MS:1000038"
        | Second -> "MS:1000039"

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

let user7 =
    TermHandler.init("User:0000007")
    |> TermHandler.addName "MaxQuant:Major Protein IDs"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user8 =
    TermHandler.init("User:0000008")
    |> TermHandler.addName "Number of Proteins"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user9 =
    TermHandler.init("User:0000009")
    |> TermHandler.addName "Number of Peptides"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user10 =
    TermHandler.init("User:0000010")
    |> TermHandler.addName "MaxQuant:Razor + unique peptides"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user11 =
    TermHandler.init("User:0000011")
    |> TermHandler.addName "MaxQuant:Unique peptides"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user12 =
    TermHandler.init("User:0000012")
    |> TermHandler.addName "MaxQuant:Unique + razor sequence coverage"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user13 =
    TermHandler.init("User:0000013")
    |> TermHandler.addName "MaxQuant:Unique sequence coverage"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user14 =
    TermHandler.init("User:0000014")
    |> TermHandler.addName "IdentificationType"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user15 =
    TermHandler.init("User:0000015")
    |> TermHandler.addName "MaxQuant:Only identified by site"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user16 =
    TermHandler.init("User:0000016")
    |> TermHandler.addName "MaxQuant:Reverse"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user17 =
    TermHandler.init("User:0000017")
    |> TermHandler.addName "MaxQuant:Peptide is razor"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user18 =
    TermHandler.init("User:0000018")
    |> TermHandler.addName "MaxQuant:Uncalibrated m/z"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user19 =
    TermHandler.init("User:0000019")
    |> TermHandler.addName "Resolution"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user20 =
    TermHandler.init("User:0000020")
    |> TermHandler.addName "Number of Datapoints"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user21 =
    TermHandler.init("User:0000021")
    |> TermHandler.addName "Number of isotopic peaks"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user22 =
    TermHandler.init("User:0000022")
    |> TermHandler.addName "MaxQuant:PIF"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user23 =
    TermHandler.init("User:0000023")
    |> TermHandler.addName "MaxQuant:Mass fractional part"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user24 =
    TermHandler.init("User:0000024")
    |> TermHandler.addName "MaxQuant:Mass deficit"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user25 =
    TermHandler.init("User:0000025")
    |> TermHandler.addName "MaxQuant:Mass precision"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user26 =
    TermHandler.init("User:0000026")
    |> TermHandler.addName "[ppm]"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user27 =
    TermHandler.init("User:0000027")
    |> TermHandler.addName "Max intensity m/z 0"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user28 =
    TermHandler.init("User:0000028")
    |> TermHandler.addName "Min scan number(s)"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user29 =
    TermHandler.init("User:0000029")
    |> TermHandler.addName "Max scan number(s)"
    |> TermHandler.addOntology 
        (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
    |> TermHandler.addToContext sqliteContext

let user30 =
    TermHandler.init("User:0000030")
    |> TermHandler.addName "Identified"
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
         |> PeptideEvidenceParamHandler.addUnit
            (TermHandler.tryFindByID dbContext (TermIDByName.toID Second)).Value
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

//ProteinGroups line 2

let proteinAmbiguitigrpusParams (dbContext:MzIdentML) =
    [
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID ProteinAccession)).Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue "CON__P00766";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000007").Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue "CON__P00766";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID PeptideCounts)).Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue "2";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID PeptideCountsWithRazorAndUnique)).Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue "2";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID PeptideCountsWithUnique)).Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue "2";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID ProteinDescription)).Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue "2";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000008").Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue "1";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000009").Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue "2";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000010").Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue "2";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000011").Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue "2";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID SequenceCoverage)).Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addUnit
        (TermHandler.tryFindByID dbContext (TermIDByName.toID Percent)).Value
    |> ProteinAmbiguityGroupParamHandler.addValue "9.8";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000012").Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addUnit
        (TermHandler.tryFindByID dbContext (TermIDByName.toID Percent)).Value
    |> ProteinAmbiguityGroupParamHandler.addValue "9.8";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000013").Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addUnit
        (TermHandler.tryFindByID dbContext (TermIDByName.toID Percent)).Value
    |> ProteinAmbiguityGroupParamHandler.addValue "9.8";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID MolecularWeight)).Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addUnit
        (TermHandler.tryFindByID dbContext (TermIDByName.toID KiloDalton)).Value
    |> ProteinAmbiguityGroupParamHandler.addValue "25666";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID SequenceLength)).Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue "245";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID QValue)).Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue "0";
    ProteinAmbiguityGroupParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID Oxidation)).Value
                                          )
    |> ProteinAmbiguityGroupParamHandler.addValue ""
    ]

let proteinDetectionHypothesisParams (dbContext:MzIdentML) =
    [
    ProteinDetectionHypothesisParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID ProteinScore)).Value
                                               )
    |> ProteinDetectionHypothesisParamHandler.addValue "0";
    ProteinDetectionHypothesisParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000014").Value
                                               )
    |> ProteinDetectionHypothesisParamHandler.addValue "By Matching";
    ProteinDetectionHypothesisParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID XICArea)).Value
                                               )
    |> ProteinDetectionHypothesisParamHandler.addValue "26427";
    ProteinDetectionHypothesisParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID MSMSCount)).Value
                                               )
    |> ProteinDetectionHypothesisParamHandler.addValue "4";
    ProteinDetectionHypothesisParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000015").Value
                                               )
    |> ProteinDetectionHypothesisParamHandler.addValue "";
    ProteinDetectionHypothesisParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000016").Value
                                               )
    |> ProteinDetectionHypothesisParamHandler.addValue "";
    ]

let peptideParams (dbContext:MzIdentML) =
    [
    PeptideParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000017").Value
                                    )
    |> PeptideParamHandler.addValue "true";
    ]


let proteinAmbiguityGroup (dbContext:MzIdentML) =
    ProteinAmbiguityGroupHandler.init(
        [(ProteinDetectionHypothesisHandler.init(true, null, null))
        |> ProteinDetectionHypothesisHandler.addDetails (proteinDetectionHypothesisParams dbContext)
        ]
                                      )
    |> ProteinAmbiguityGroupHandler.addDetails (proteinAmbiguitigrpusParams dbContext)

//All peptides line 2

let spectrumidentificationItemParam2 (dbContext:MzIdentML) =
    [
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000018").Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "350.95113"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID dbContext (TermIDByName.toID KiloDalton)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000019").Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "NaN";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000020").Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "6";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID ScanNumber)).Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "3";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000021").Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "2";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000022").Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "0.1047619"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID dbContext (TermIDByName.toID Percent)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000023").Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "0.656214"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID dbContext (TermIDByName.toID Percent)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000024").Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "-0.18862800013585";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000025").Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "3.071728"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID dbContext "User:0000026").Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000027").Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "351.138012741353";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID RetentionTime)).Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "0.016"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID dbContext (TermIDByName.toID Minute)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID RetentionLength)).Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "1.174"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID dbContext (TermIDByName.toID Second)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext (TermIDByName.toID FullWithAtHalfMaximum)).Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "1.176"
    |> SpectrumIdentificationItemParamHandler.addUnit
        (TermHandler.tryFindByID dbContext (TermIDByName.toID Second)).Value;
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000028").Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "0";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000029").Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "4";
    SpectrumIdentificationItemParamHandler.init(
        (TermHandler.tryFindByID dbContext "User:0000030").Value
                                               )
    |> SpectrumIdentificationItemParamHandler.addValue "-";
    ]

let spectrumIdentificationItem2 (dbContext:MzIdentML) =
    SpectrumIdentificationItemHandler.init(
        null, 5, 350., true, 0
                                          )
    |> SpectrumIdentificationItemHandler.addDetails (spectrumidentificationItemParam2 dbContext)

let spectrumIdentificationResult (dbContext:MzIdentML) =
    SpectrumIdentificationResultHandler.init(
        null, "-1", [(spectrumIdentificationItem2 dbContext)]
                                            )
