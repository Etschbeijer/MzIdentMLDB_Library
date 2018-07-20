    
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

module test =

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


module MaxPlankFileTest =

//Using peptide ID = 119 for test, line 121 in peptides; Modification-specific peptides IDs=125 & 126; 
//Oxidation (M)Sites for Modification-specific peptides ID=125 -> 97; ProteinGroups ID=173, Line 175;
//AllPeptides Line 227574 (MOxidized) & line 616423 (unmodified); MS/MS scans

    let user0 =
        TermHandler.init("User:0000000")
        |> TermHandler.addName "N-term cleavage window"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user1 =
        TermHandler.init("User:0000001")
        |> TermHandler.addName "C-term cleavage window"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user2 =
        TermHandler.init("User:0000002")
        |> TermHandler.addName "Leading razor protein"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user3 =
        TermHandler.init("User:0000003")
        |> TermHandler.addName "MaxQuant: Unique Groups"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user4 =
        TermHandler.init("User:0000004")
        |> TermHandler.addName "MaxQuant: Unique Proteins"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user5 =
        TermHandler.init("User:0000005")
        |> TermHandler.addName "MaxQuant: PEP"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user6 =
        TermHandler.init("User:0000006")
        |> TermHandler.addName "IdentificationType"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user7 =
        TermHandler.init("User:0000007")
        |> TermHandler.addName "AminoAcid modification"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user8 =
        TermHandler.init("User:0000008")
        |> TermHandler.addName "Charge corrected mass of the precursor ion"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user9 =
        TermHandler.init("User:0000009")
        |> TermHandler.addName "Calibrated retention time"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user10 =
        TermHandler.init("User:0000010")
        |> TermHandler.addName "Mass error"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user11 =
        TermHandler.init("User:0000011")
        |> TermHandler.addName "ppm"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user12 =
        TermHandler.init("User:0000012")
        |> TermHandler.addName "MaxQuant: Major protein IDs"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user13 =
        TermHandler.init("User:0000013")
        |> TermHandler.addName "MaxQuant: Number of proteins"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user14 =
        TermHandler.init("User:0000014")
        |> TermHandler.addName "MaxQuant: Peptides"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user15 =
        TermHandler.init("User:0000015")
        |> TermHandler.addName "MaxQuant: Razor + unique peptides"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user16 =
        TermHandler.init("User:0000016")
        |> TermHandler.addName "MaxQuant: Unique peptides"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user17 =
        TermHandler.init("User:0000017")
        |> TermHandler.addName "MaxQuant: Unique + razor sequence coverage"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user18 =
        TermHandler.init("User:0000018")
        |> TermHandler.addName "MaxQuant: Unique sequence coverage"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user19 =
        TermHandler.init("User:0000019")
        |> TermHandler.addName "MaxQuant: SequenceLength(s)"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user20 =
        TermHandler.init("User:0000020")
        |> TermHandler.addName "Tab seperated"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user21 =
        TermHandler.init("User:0000021")
        |> TermHandler.addName "DetectionType"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user22 =
        TermHandler.init("User:0000022")
        |> TermHandler.addName "MaxQuant: Uncalibrated m/z"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user23 =
        TermHandler.init("User:0000023")
        |> TermHandler.addName "MaxQuant: Number of data points"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user24 =
        TermHandler.init("User:0000024")
        |> TermHandler.addName "MaxQuant: Number of isotopic peaks"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user25 =
        TermHandler.init("User:0000025")
        |> TermHandler.addName "MaxQuant: Parent ion fraction"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user26 =
        TermHandler.init("User:0000026")
        |> TermHandler.addName "MaxQuant: Mass precision"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user27 =
        TermHandler.init("User:0000027")
        |> TermHandler.addName "Retention length (FWHM)"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user28 =
        TermHandler.init("User:0000028")
        |> TermHandler.addName "MaxQuant: Min scan number"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user29 =
        TermHandler.init("User:0000029")
        |> TermHandler.addName "MaxQuant: Max scan number"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user30 =
        TermHandler.init("User:0000030")
        |> TermHandler.addName "MaxQuant: MSMS scan numbers"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user31 =
        TermHandler.init("User:0000031")
        |> TermHandler.addName "MaxQuant: MSMS isotope indices"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user32 =
        TermHandler.init("User:0000032")
        |> TermHandler.addName "MaxQuant: Filtered peaks"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user33 =
        TermHandler.init("User:0000033")
        |> TermHandler.addName "FragmentationType"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user34 =
        TermHandler.init("User:0000034")
        |> TermHandler.addName "Parent intensity fraction"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user35 =
        TermHandler.init("User:0000035")
        |> TermHandler.addName "Fraction of total spectrum"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user36 =
        TermHandler.init("User:0000036")
        |> TermHandler.addName "Base peak fraction"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user37 =
        TermHandler.init("User:0000037")
        |> TermHandler.addName "Precursor full scan number"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user38 =
        TermHandler.init("User:0000038")
        |> TermHandler.addName "Precursor intensity"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user39 =
        TermHandler.init("User:0000039")
        |> TermHandler.addName "Precursor apex fraction"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user40 =
        TermHandler.init("User:0000040")
        |> TermHandler.addName "Precursor apex offset"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user41 =
        TermHandler.init("User:0000041")
        |> TermHandler.addName "Precursor apex offset time"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user42 =
        TermHandler.init("User:0000042")
        |> TermHandler.addName "Scan event number"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user43 =
        TermHandler.init("User:0000043")
        |> TermHandler.addName "MaxQuant: Score difference"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user44 =
        TermHandler.init("User:0000044")
        |> TermHandler.addName "MaxQuant: Combinatorics"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user45 =
        TermHandler.init("User:0000045")
        |> TermHandler.addName "MaxQuant: Matches"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext

    let user46 =
        TermHandler.init("User:0000046")
        |> TermHandler.addName "MaxQuant: Intensities"
        |> TermHandler.addOntology 
            (OntologyHandler.tryFindByID sqliteContext "UserParam").Value
        |> TermHandler.addToContext sqliteContext
        
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

            //AminoAcids
            ///Sequence of amino acids.
            | AminoAcidSequence
            ///Mutation of an AminoAcid.
            | AmnoAcidModification

            //Proteins
            ///The identifier of the best scoring protein, from the
            ///ProteinGroups file this, this peptide is associated to.
            | LeadingRazorProtein
            ///The identifiers of the proteins in the proteinGroups file, with this
            ///Protein as best match, this particular peptide is associated with.
            | LeadingProteins
            ///Description of the protein.
            | ProteinDescription
            ///Number of proteins contained within the group.
            | NumberOfProteins
            ///This is the ratio of reverse to forward protein groups.
            | QValue

            //Peptides
            ///Peptide is unique to a single protein group in the proteinGroups file.
            | UniqueToGroups
            ///Peptide is unique to a single protein sequence in the fasta file(s).
            | UniqueToProteins
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

            //Nucleic Acids
            ///Seuqence of nucleic acids.
            | NucleicAcidSequence
            ///Mutation of a nucleic acid base.
            | NucleicAcidModification

            //MassSpectometry
            ///MonoIsotopicMass of the peptide.
            | MonoIsotopicMass
            ///Highest Andromeda score for the associated MS/MS spectra.
            | AndromedaScore
            ///Score difference to the second best identified peptide.
            | DeltaScore
            ///Indicates the way this experiment was identified.
            | IdentificationType
            ///Area under curve for one XIC.
            | XICArea
            ///Normalized area under curve  for one XIC.
            | NormalizedXICArea
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
            | MULTI
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
            ///Kind of ion after fragmentation of peptide.
            | Fragment
            ///The species of the peaks in the fragmentation spectrum after TopN filtering.
            | IntensitiesOfFragments

            //Units
            | Minute
            | Second
            | Ppm
            | Percent

            //File
            /// Values are seperated by tab(s).
            | FileFormat

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
                | UniqueToProteins -> "User:0000004"
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
                | DeltaScore -> "MS:1002834"
                | LeadingProteins -> "MS:1002401"
                | ProteinDescription -> "MS:1001088"
                | MassError -> "User:0000010"
                | Ppm -> "User:0000011"
                | MajorProteinIDs -> "User:0000012"
                | PeptideCountsAll -> "MS:1001898"
                | PeptideCountsRazorAndUnique -> "MS:1001899"
                | PeptideCountUnique -> "MS:1001897"
                | NumberOfProteins -> "User:0000013"
                | Peptides -> "User:0000014"
                | RazorAndUniquePeptides -> "User:0000015"
                | UniquePeptides -> "User:0000016"
                | SequenceCoverage -> "MS:1001093"
                | Percent -> "UO:0000187"
                | UniqueAndRazorSequenceCoverage -> "User:0000017"
                | UniqueSequenceCoverage -> "User:0000018"
                | MolecularWeight -> "PRIDE:0000057"
                | SequenceLengths -> "User:0000019"
                | QValue -> "MS:1001491"
                | ProteinScore -> "MS:1002394"
                | FileFormat -> "User:0000020"
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
                | MULTI -> "MS:1000774"
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
                | IntensitiesOfFragments -> "User:0000046"

//Peptides ID=119; Modification-specific peptides IDs=123 & 124


    let fileFormat =
        CVParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID FileFormat)).Value
                           )
        |> CVParamHandler.addValue "Tab seperated"


    let databaseName =
        CVParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID RawFile)).Value
                           )
        |> CVParamHandler.addValue "20170518 TM FSconc3009"


    let searchDatabase =
        SearchDatabaseHandler.init(
            "local", fileFormat, databaseName, "20170518 TM FSconc3009"
                                  )

    let dbSequence =
        DBSequenceHandler.init("AAIEASFGSVDEMK", searchDatabase)
        |> DBSequenceHandler.addSequence "AAIEASFGSVDEMK"
        |> DBSequenceHandler.addLength 14

    let peptideParamUnmodified =
        [
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID AminoAcidSequence)).Value
                                )
        |> PeptideParamHandler.addValue "AAIEASFGSVDEMK";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID SequenceLength)).Value
                                )
        |> PeptideParamHandler.addValue "14";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID NTermCleavageWindow)).Value
                                )
        |> PeptideParamHandler.addValue "NGPNGDVKAAIEASFG";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID CTermCleavageWindow)).Value
                                )
        |> PeptideParamHandler.addValue "FGSVDEMKAKFNAAAA";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID UniqueToGroups)).Value
                                )
        |> PeptideParamHandler.addValue "yes";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID AmnoAcidModification)).Value
                                )
        |> PeptideParamHandler.addValue "yes";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID LeadingProteins)).Value
                                )
        |> PeptideParamHandler.addValue "Cre02.g096150.t1.2";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID LeadingRazorProtein)).Value
                                )
        |> PeptideParamHandler.addValue "Cre02.g096150.t1.2";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Mass)).Value
                                )
        |> PeptideParamHandler.addValue "1453.6797";
        ]

    let peptideUnmodified =
        PeptideHandler.init("AAIEASFGSVDEMK", "119")
        |> PeptideHandler.addDetails peptideParamUnmodified

    let peptideEvidenceParams =
        PeptideEvidenceParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID DetectionType)).Value
                                        )
        |> PeptideEvidenceParamHandler.addValue "MULTI"
    
    let peptideEvidence =
        PeptideEvidenceHandler.init(
            dbSequence, peptideUnmodified
                                   )
        |> PeptideEvidenceHandler.addStart 106
        |> PeptideEvidenceHandler.addEnd 119
        |> PeptideEvidenceHandler.addDetail peptideEvidenceParams

    let enzyme =
        EnzymeHandler.init()
        |> EnzymeHandler.addMissedCleavages 0

    let spectrumidentificationItemParamPeptideUnmodified =
        [
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MonoIsotopicMass)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "1453.6759";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PosteriorErrorProbability)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "2.95E-39";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID AndromedaScore)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "122.97";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID IdentificationType)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "By MS/MS";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID TotalXIC)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "290760";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MSMSCount)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "14";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MSMSScanNumber)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "15279";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID RetentionTime)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "25.752"
        |> SpectrumIdentificationItemParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Minute)).Value;
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID CalibratedRetentionTime)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "25.664"
        |> SpectrumIdentificationItemParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Minute)).Value;
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID DeltaScore)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "110.81";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID DetectionType)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "MULTI";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID UncalibratedMZ)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "727.8481";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID FWHM)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "NAN";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID NumberOfDataPoints)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "27";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID ScanNumber)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "11";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID NumberOfIsotopicPeaks)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "3";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PIF)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0.8484042";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MassDefect)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "-0.028955966";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MassPrecision)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0.771146"
        |> SpectrumIdentificationItemParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Ppm)).Value;
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID TotalXIC)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "727.8458225";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID RetentionLength)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "22.688"
        |> SpectrumIdentificationItemParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Minute)).Value;
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID RetentionTimeFWHM)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "11.349"
        |> SpectrumIdentificationItemParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Minute)).Value;
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MinScanNumber)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "15077";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MaxScanNumber)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "15329";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PeptideScore)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "105.0287018";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID TotalXIC)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "25278";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID TotalXIC)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0;3563;15589;27011;28600;27211;19809;14169;10127;6641;1399;1375;0";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MSMSCount)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "1";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MSMSScanNumbers)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "15159";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MSMSIsotopIndices)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID ScanNumber)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "15159";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID IonInjectionTime)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID TotalIonCurrent)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "92047";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID FilteredPeaks)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "135";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID ParentIntensityFraction)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0.8484042";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID FractionOfTotalSpectrum)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0.001671316";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID BasePeakFraction)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0.07051102";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PrecursorFullScanNumbers)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "15140";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PrecursorIntensity)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "10131";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PrecursorApexFraction)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0.905362";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PrecursorApexOffset)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "-2";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PrecursorApexOffsetTime)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0.06315041";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID ScanEventNumber)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "19";
        ]

    let spectrumidentificationItemPeptideUnmodified =
        SpectrumIdentificationItemHandler.init(
            peptideUnmodified, 2, 727.84714, true, -1
                                              )
        |> SpectrumIdentificationItemHandler.addDetails spectrumidentificationItemParamPeptideUnmodified

    let modificationParams =
        [
        ModificationParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID UniqueToProteins)).Value
                                     )
        |> ModificationParamHandler.addValue "Oxidation";
        ModificationParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MassPrecursorIon)).Value
                                     )
        |> ModificationParamHandler.addValue "1469.6708";
        ModificationParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID RetentionTime)).Value
                                     )
        |> ModificationParamHandler.addValue "21.489"
        |> ModificationParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Minute)).Value;
        ModificationParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID RetentionTime)).Value
                                     )
        |> ModificationParamHandler.addValue "21.489"
        |> ModificationParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Minute)).Value;
        ModificationParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID CalibratedRetentionTime)).Value
                                     )
        |> ModificationParamHandler.addValue "21.372"
        |> ModificationParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Minute)).Value;
        ModificationParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Combinatorics)).Value
                                     )
        |> ModificationParamHandler.addValue "1";
        ]

    let modificationMOxidized =
        ModificationHandler.init(modificationParams, "123")
        |> ModificationHandler.addResidues "M"
        |> ModificationHandler.addLocation 13

    let peptideParamMOxidized =
        [
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID AminoAcidSequence)).Value
                                )
        |> PeptideParamHandler.addValue "AAIEASFGSVDEMK";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID SequenceLength)).Value
                                )
        |> PeptideParamHandler.addValue "14";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID NTermCleavageWindow)).Value
                                )
        |> PeptideParamHandler.addValue "NGPNGDVKAAIEASFG";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID CTermCleavageWindow)).Value
                                )
        |> PeptideParamHandler.addValue "FGSVDEMKAKFNAAAA";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID UniqueToGroups)).Value
                                )
        |> PeptideParamHandler.addValue "yes";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID AmnoAcidModification)).Value
                                )
        |> PeptideParamHandler.addValue "yes";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID LeadingProteins)).Value
                                )
        |> PeptideParamHandler.addValue "Cre02.g096150.t1.2";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID LeadingRazorProtein)).Value
                                )
        |> PeptideParamHandler.addValue "Cre02.g096150.t1.2";
        PeptideParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Mass)).Value
                                )
        |> PeptideParamHandler.addValue "1469.6761";
        ]

    let peptideMOxidized =
            PeptideHandler.init("AAIEASFGSVDEM(1)K", "119MOxidized")
            |> PeptideHandler.addModification modificationMOxidized
            |> PeptideHandler.addDetails peptideParamMOxidized

    let spectrumidentificationItemParamPeptideMOxidized =
        [
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MonoIsotopicMass)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "1453.6759";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PosteriorErrorProbability)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "6,36E-25";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID AndromedaScore)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "111.12";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID IdentificationType)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "By MS/MS";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID TotalXIC)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "167790";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MSMSCount)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "14";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MSMSScanNumber)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "12427";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID RetentionTime)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "21.489"
        |> SpectrumIdentificationItemParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Minute)).Value;
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID CalibratedRetentionTime)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "21.372"
        |> SpectrumIdentificationItemParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Minute)).Value;
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID DeltaScore)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "100.69";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID DetectionType)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "MULTI";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID UncalibratedMZ)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "735.8439";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID FWHM)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "NAN";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID NumberOfDataPoints)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "24";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID ScanNumber)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "10";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID NumberOfIsotopicPeaks)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "4";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PIF)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0.8281062";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MassDefect)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "-0.039976308";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MassPrecision)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "1.815383"
        |> SpectrumIdentificationItemParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Ppm)).Value;
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID TotalXIC)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "735.8475942";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID RetentionLength)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "20.649"
        |> SpectrumIdentificationItemParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Minute)).Value;
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID RetentionTimeFWHM)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "7.509"
        |> SpectrumIdentificationItemParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Minute)).Value;
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MinScanNumber)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "12116";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MaxScanNumber)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "12347";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PeptideScore)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "69.04515839";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID TotalXIC)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "25120";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID TotalXIC)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0;3731;6755;24524;24360;18177;11825;4543;4102.5;3066;2497;0";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MSMSCount)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "1";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MSMSScanNumbers)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "12193";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MSMSIsotopIndices)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID ScanNumber)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "12193";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID IonInjectionTime)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID TotalIonCurrent)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "71250";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID FilteredPeaks)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "157";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID ParentIntensityFraction)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0.8028493";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID FractionOfTotalSpectrum)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0.002043774";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID BasePeakFraction)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0.04564729";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PrecursorFullScanNumbers)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "12179";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PrecursorIntensity)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "10056";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PrecursorApexFraction)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "1";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PrecursorApexOffset)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PrecursorApexOffsetTime)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "0";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID ScanEventNumber)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "14";
        SpectrumIdentificationItemParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID ScoreDifference)).Value
                                                   )
        |> SpectrumIdentificationItemParamHandler.addValue "69.045";
        ]

    let measureParamsIntensietiesMOxidized =
        CVParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID IntensitiesOfFragments)).Value
                                )

    let valueIntensitiesMOxized =
        [68;150;85;204;622;418;683;548;34;102;34;34;34;17;68;204;85;85;85;34;51;17;34;17;34;34;17]
        |> Seq.map (fun value -> ValueHandler.init (float value))

    let fragmentArrayMOXidized =
        [
        FragmentArrayHandler.init(
            measureParamsIntensietiesMOxidized, valueIntensitiesMOxized
                                 );
        FragmentArrayHandler.init(
            measureParamsIntensietiesMOxidized, valueIntensitiesMOxized
                                 );

        ]
  
    let ionTypeParamMOxidized =
        [
        IonTypeParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Fragment)).Value
                                )
        |> IonTypeParamHandler.addValue 
            "y3;y4;y5;y6;y7;y8;y9;y10;y3-H2O;y7-H2O;y10-H2O;y9(2+);y10(2+);y11(2+);y12(2+);b4;b5;b6;b6-H2O;b8;b8-H2O;b9;b9-H2O;b10;b10-H2O;b11;b12"
        
        ]

    let fragmentationsMOxidized =
        [
        IonTypeHandler.init(
            ionTypeParamMOxidized
                           )
        |> IonTypeHandler.addIndex (IndexHandler.init 0)
        |> IonTypeHandler.addFragmentArrays fragmentArrayMOXidized
        ]

    let spectrumidentificationItemPeptideMOxidized =
        SpectrumIdentificationItemHandler.init(
            peptideMOxidized, 2, 735.84531, true, -1
                                              )
        |> SpectrumIdentificationItemHandler.addFragmentations fragmentationsMOxidized
        |> SpectrumIdentificationItemHandler.addDetails spectrumidentificationItemParamPeptideMOxidized

    let peptideHypothesis =
        PeptideHypothesisHandler.init(
            peptideEvidence, 
            [spectrumidentificationItemPeptideUnmodified; spectrumidentificationItemPeptideMOxidized]
                                     )

    let proteinDetectionHypothesisParams =
        [
        ProteinDetectionHypothesisParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID ProteinDescription)).Value
                                                   )
        |> ProteinDetectionHypothesisParamHandler.addValue 
            ">Cre02.g096150.t1.2 Mn superoxide dismutase ALS=MSD1 DBV=JGI5.5 GN=MSD1 
            OS=Chlamydomonas reinhardtii SV=2 TOU=Cre"
        ]
    
    let proteinDetectionHypothesis =
        ProteinDetectionHypothesisHandler.init(
            true, dbSequence, [peptideHypothesis], "Cre02.g096150.t1.2", "Cre02.g096150.t1.2"
                                              )
        |> ProteinDetectionHypothesisHandler.addDetails proteinDetectionHypothesisParams

    let proteinAmbiguousGroupsParams =
        [
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MajorProteinIDs)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "Cre02.g096150.t1.2";
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PeptideCountsAll)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "6";
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PeptideCountsRazorAndUnique)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "6";
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID PeptideCountsRazorAndUnique)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "6";
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID ProteinDescription)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue ">Cre02.g096150.t1.2 Mn superoxide dismutase ALS=MSD1 DBV=JGI5.5 GN=MSD1 OS=Chlamydomonas reinhardtii SV=2 TOU=Cre";
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID NumberOfProteins)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "1";
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Peptides)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "6";
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID RazorAndUniquePeptides)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "6";
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID UniquePeptides)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "6";
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID SequenceCoverage)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "31.7"
        |> ProteinAmbiguityGroupParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Percent)).Value;
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID UniqueAndRazorSequenceCoverage)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "31.7"
        |> ProteinAmbiguityGroupParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Percent)).Value;
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID UniqueSequenceCoverage)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "31.7"
        |> ProteinAmbiguityGroupParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Percent)).Value;
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MolecularWeight)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "23.9"
        |> ProteinAmbiguityGroupParamHandler.addUnit
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID Percent)).Value;
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID SequenceLength)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "218";
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID SequenceLengths)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "218";
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID QValue)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "0";
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID ProteinScore)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "105.09";
        ProteinAmbiguityGroupParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID TotalXIC)).Value
                                              )
        |> ProteinAmbiguityGroupParamHandler.addValue "1335100";
        ]

    let proteinAmbiguousGroups =
        ProteinAmbiguityGroupHandler.init(
            [proteinDetectionHypothesis], "173"
                                         )
        |> ProteinAmbiguityGroupHandler.addDetails proteinAmbiguousGroupsParams

    let spectrumIDFormat =
        CVParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MULTI)).Value
                           )

    let spectradata =
        [
        SpectraDataHandler.init(
            "local", fileFormat, spectrumIDFormat
                               )
        |> SpectraDataHandler.addName "20170518 TM FSconc3001";
        SpectraDataHandler.init(
            "local", fileFormat, spectrumIDFormat
                               )
        |> SpectraDataHandler.addName "20170518 TM FSconc3002";
        ]

    let softwareName =
        CVParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MaxQuant)).Value
                           )

    let analysisSoftware =
        AnalysisSoftwareHandler.init(softwareName)

    let searchType =
        CVParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MSMSSearch)).Value
                           )

    //let thresholdParam =
    //    ThresholdParamHandler.init(
    //        (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MaxQuant)).Value

    let searchModificationParam =
        [
        SearchModificationParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID CollisionEnergy)).Value
                                           )
        |> SearchModificationParamHandler.addValue "0";
        SearchModificationParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID FragmentationType)).Value
                                           )
        |> SearchModificationParamHandler.addValue "CID";
        SearchModificationParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID MassAnalyzerType)).Value
                                           )
        |> SearchModificationParamHandler.addValue "TOF";
        ]

    let searchModificationParams =
        SearchModificationHandler.init(
            false, 15.9949, "M", searchModificationParam
                                      )

    let spectrumIdentificationProtocol =
        SpectrumIdentificationProtocolHandler.init(
            analysisSoftware, searchType, null
                                                  )
        |> SpectrumIdentificationProtocolHandler.addEnzyme enzyme
        |> SpectrumIdentificationProtocolHandler.addModificationParam searchModificationParams

    let spectrumIdentificationResultParamMOxidized =
        [
        SpectrumIdentificationResultParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID BasePeakIntension)).Value
                                                     )
        |> SpectrumIdentificationResultParamHandler.addValue "0";
        SpectrumIdentificationResultParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID ElapsedTime)).Value
                                                     )
        |> SpectrumIdentificationResultParamHandler.addValue "1";
        ]

    let spectrumIdentificationResultParamUnModified =
        [
        SpectrumIdentificationResultParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID BasePeakIntension)).Value
                                                     )
        |> SpectrumIdentificationResultParamHandler.addValue "0";
        SpectrumIdentificationResultParamHandler.init(
            (TermHandler.tryFindByID sqliteContext (TermIDByName.toID ElapsedTime)).Value
                                                     )
        |> SpectrumIdentificationResultParamHandler.addValue "1";
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

    let spectrumIdentification = 
        SpectrumIdentificationHandler.init(
            spectrumIdentificationList, spectrumIdentificationProtocol, spectradata, [searchDatabase]
                                          )