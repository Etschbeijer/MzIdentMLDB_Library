//Apply functions
        
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
open System.Collections.ObjectModel
open MzIdentMLDataBase.XMLParsing


let fileDir = __SOURCE_DIRECTORY__
let standardDBPathSQLite = fileDir + "\Databases\Test1.db"

let sqliteContext = ContextHandler.sqliteConnection standardDBPathSQLite


let fromPsiMS =
    ContextHandler.fromFileObo (fileDir + "\Ontologies\Psi-MS.txt")

let fromPride =
    ContextHandler.fromFileObo (fileDir + "\Ontologies\Pride.txt")

let fromUniMod =
    ContextHandler.fromFileObo (fileDir + "\Ontologies\Unimod.txt")

let fromUnit_Ontology =
    ContextHandler.fromFileObo (fileDir + "\Ontologies\Unit_Ontology.txt")
        
 
let initStandardDB (dbContext : MzIdentML) =

    let termsPSIMS =
        let ontology =  OntologyHandler.init ("PSI-MS")
        fromPsiMS
        |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, ontology))
        |> Seq.iter (fun term -> ContextHandler.tryAddToContext dbContext term |> ignore) 

    let termsPride =
        let ontology =  OntologyHandler.init ("PRIDE")
        fromPride
        |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, ontology))
        |> Seq.iter (fun term -> ContextHandler.tryAddToContext dbContext term |> ignore)

    let termsUnimod =
        let ontology =  OntologyHandler.init ("UNIMOD")
        fromUniMod
        |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, ontology))
        |> Seq.iter (fun term -> ContextHandler.tryAddToContext dbContext term |> ignore)

    let termsUnit_Ontology =
        let ontology =  OntologyHandler.init ("UNIT-ONTOLOGY") 
        fromUnit_Ontology
        |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, ontology))
        |> Seq.iter (fun term -> ContextHandler.tryAddToContext dbContext term |> ignore)

    let userOntology =
        OntologyHandler.init("UserParam")
        |> ContextHandler.tryAddToContext dbContext |> ignore

    dbContext.Database.EnsureCreated() |> ignore
    dbContext.SaveChanges()

//initStandardDB sqliteContext


TermHandler.tryFindByID sqliteContext "Teest"
TermHandler.tryFindByID sqliteContext "MS:0000000"

//Rewrite function, so that it takes all correspending object out of context and then compare them because otherwise the checking lasts to long!

let private matchTerms (dbContext:MzIdentML) (item1:Term) (item2:Term) =
    if item1.Name=item2.Name && item1.Ontology=item2.Ontology
       then ()
       else 
            if item1.ID = item2.ID
               then item2.ID <- System.Guid.NewGuid().ToString()
                    dbContext.Add item2 |> ignore
               else dbContext.Add item2 |> ignore

let addTermToContext (dbContext : MzIdentML) (item:Term) =
    query {
        for i in dbContext.Term.Local do
            if i.Name = item.Name
               then select i
          }
    |> (fun term -> 
        if Seq.length term < 1 
            then 
                query {
                       for i in dbContext.Term do
                           if i.Name = item.Name
                              then select i
                      }
                |> (fun term' -> if (term'.Count()) < 1
                                    then let tmp = dbContext.Term.Find(item.ID)
                                         if tmp.ID = item.ID 
                                            then item.ID <- System.Guid.NewGuid().ToString()
                                                 dbContext.Add item |> ignore
                                            else dbContext.Add item |> ignore
                                    else term'
                                         |> Seq.iter (fun termItem -> matchTerms dbContext termItem item)
                   )
            else term
                 |> Seq.iter (fun termItem -> matchTerms dbContext termItem item)
       )

let addTerm2 (dbContext : MzIdentML) (item:Term) =
    query {
        for i in dbContext.Term.Local do
            if i.Name = item.Name && i.Ontology=item.Ontology 
               then select (i)
          }
    |> (fun item -> if (Seq.length item < 1)
                       then dbContext.Add item |> ignore
                            ()
                       else ()
       )


addTermToContext sqliteContext (TermHandler.init("TEEEEEEEEEEEST"))
sqliteContext.SaveChanges()

let x = addTermToContext sqliteContext (TermHandler.init("TEEEEEEEEEEEST", Ontology=(OntologyHandler.tryFindByID sqliteContext "PSI-MS").Value))
let y = addTermToContext sqliteContext (TermHandler.init("MS:0000000", "Proteomics Standards Initiative Mass Spectrometry Vocabularies", (OntologyHandler.tryFindByID sqliteContext "PSI-MS").Value))
let z = addTerm2 sqliteContext (TermHandler.init("TEEEEEEEEEEEST"))

let termTest = TermHandler.init("I","III")

let CVParamTest = CVParamHandler.init(termTest)

let addedtermTest =
    sqliteContext.Add termTest

for i = 0 to 10000 do
    TermHandler.addToContext sqliteContext termTest

for i = 0 to 10000 do
    CVParamHandler.addToContext sqliteContext CVParamTest

sqliteContext.SaveChanges()

let testQuery (dbContext:MzIdentML) (item:CVParam) =
    query {
           for i in dbContext.CVParam do
               if i.Term.ID = item.Term.ID && i.Term.Name = item.Term.Name && i.Term.Ontology = item.Term.Ontology
                  then select (i, i.Term, i.Unit)
          }
    |> Seq.map (fun (param,_ ,_) -> param)
    |> Seq.map (fun cvParam -> ContextHandler.tryAddToContext dbContext cvParam)


testQuery sqliteContext CVParamTest

Seq.item 0 (testQuery sqliteContext CVParamTest)

type TermIDByName =
    | ProteinGroupLevelFDR
    | ParentMassMono
    | FragmentMassMono
    | ConsensusScoring
    | PeptideLevelScoring
    | GroupPSMsBySequenceWithModifications
    | ModificationLocalizationScoring
    | Trypsin
    | SearchTolerancePlusValue
    | SearchToleranceMinusValue
    | ModificationSpecificityProteinNterm
    | ModificationSpecificityPeptideNterm
    | PeptideSequenceLevelFDR
    | PSMLevelFDR
    | PhosphoRSScoreThreshold
    | DScoreThreshold
    | SoftwareVendor
    | PeptideShaker
    | ContactName
    | ContactAddress
    | ContactURL
    | ContactEmail
    | CountOfIdentifiedProteins
    | PeptideShakerProteinGroupScore
    | PeptideShakerProteinGroupConfidence
    | PeptideShakerProteinConfidenceType
    | ProteinGroupPassesThreshold
    | ProductIonMasstoChargeRatio
    | ProductIonIntensity
    | ProductIonMasstoChargeRatioError
    | SpectrumTitle
    | RetentionTime
    | ProteinDescription
    | Carbamidomethyl
    | Oxidation
    | NTerminalGlutamateToPyroglutamateConversion
    | NTerminalGlutamicAcidToPyroglutamateConversion
    | Acetyl
    | AmmoniaLoss
    static member toString (item:TermIDByName) =
        match item with
        | ProteinGroupLevelFDR -> "MS:1002369"
        | ParentMassMono -> "MS:1001211"
        | FragmentMassMono -> "MS:1001256"
        | ConsensusScoring -> "MS:1002492"
        | PeptideLevelScoring -> "MS:1002490"
        | GroupPSMsBySequenceWithModifications -> "MS:1002497"
        | ModificationLocalizationScoring -> "MS:1002491"
        | Trypsin -> "MS:1001251"
        | SearchTolerancePlusValue -> "MS:1001412"
        | SearchToleranceMinusValue -> "MS:1001413"
        | ModificationSpecificityProteinNterm -> "MS:1002057"
        | ModificationSpecificityPeptideNterm -> "MS:1001189"
        | PeptideSequenceLevelFDR -> "MS:1001364"
        | PSMLevelFDR -> "MS:1002350"
        | PhosphoRSScoreThreshold -> "MS:1002567"
        | DScoreThreshold -> "MS:1002557"
        | SoftwareVendor -> "MS:1001267"
        | PeptideShaker -> "MS:1002458"
        | ContactName -> "MS:1000586"
        | ContactAddress -> "MS:1000587"
        | ContactURL -> "MS:1000588"
        | ContactEmail -> "MS:1000589"
        | CountOfIdentifiedProteins -> "MS:1002404"
        | PeptideShakerProteinGroupScore -> "MS:1002470"
        | PeptideShakerProteinGroupConfidence -> "MS:1002471"
        | PeptideShakerProteinConfidenceType -> "MS:1002542"
        | ProteinGroupPassesThreshold -> "MS:1002415"
        | ProductIonMasstoChargeRatio -> "MS:1001225"
        | ProductIonIntensity -> "MS:1001226"
        | ProductIonMasstoChargeRatioError -> "MS:1001227"
        | SpectrumTitle -> "MS:1000796"
        | RetentionTime -> "MS:1000894"
        | ProteinDescription -> "MS:1001088"
        | Carbamidomethyl -> "UNIMOD:4"
        | Oxidation -> "UNIMOD:35"
        | NTerminalGlutamateToPyroglutamateConversion -> "UNIMOD:28"
        | NTerminalGlutamicAcidToPyroglutamateConversion -> "UNIMOD:27"
        | Acetyl -> "UNIMOD:1"
        | AmmoniaLoss -> "UNIMOD:385"

#time
type SchemePeptideShaker = XmlProvider<Schema = "..\MzIdentMLDB_Library\XML_Files\MzIdentMLScheme1_2.xsd">
let  samplePeptideShaker = SchemePeptideShaker.Load("..\MzIdentMLDB_Library\XML_Files\PeptideShaker_mzid_1_2_example.txt") 


samplePeptideShaker.AuditCollection.Value.Organizations
|> Array.iter (fun organization -> convertToEntity_Organization sqliteContext organization)

samplePeptideShaker.AuditCollection.Value.Persons
|> Array.iter (fun person -> convertToEntity_Person  sqliteContext person)

samplePeptideShaker.AnalysisSoftwareList.Value.AnalysisSoftwares
|> Array.iter (fun analysisSoftware -> convertToEntity_AnalysisSoftware sqliteContext analysisSoftware)

samplePeptideShaker.SequenceCollection.Value.Peptides
|> Array.iter (fun peptide -> convertToEntity_Peptide sqliteContext peptide)

samplePeptideShaker.AnalysisProtocolCollection.SpectrumIdentificationProtocols
|> Array.iter (fun item -> convertToEntity_SpectrumIdentificationProtocol sqliteContext item)

samplePeptideShaker.SequenceCollection.Value.DbSequences
|> Array.iter (fun item -> convertToEntity_DBSequence sqliteContext item)

samplePeptideShaker.SequenceCollection.Value.PeptideEvidences
|> Array.iter (fun item -> convertToEntity_PeptideEvidence sqliteContext item)

samplePeptideShaker.DataCollection.Inputs.SpectraDatas
|> Array.iter (fun item -> convertToEntity_SpectraData sqliteContext item)

samplePeptideShaker.DataCollection.Inputs.SourceFiles
|> Array.iter (fun item -> convertToEntity_SourceFile sqliteContext item)

samplePeptideShaker.DataCollection.Inputs.SearchDatabases
|> Array.iter (fun item -> convertToEntity_SearchDatabase sqliteContext item)

convertToEntity_MzIdentML sqliteContext samplePeptideShaker

convertToEntity_AnalysisData sqliteContext samplePeptideShaker.DataCollection.AnalysisData

samplePeptideShaker.DataCollection.Inputs
|> (fun item -> convertToEntity_Inputs sqliteContext item)

convertToEntity_Provider sqliteContext samplePeptideShaker.Provider.Value

convertToEntity_ProteinDetectionProtocol sqliteContext samplePeptideShaker.AnalysisProtocolCollection.ProteinDetectionProtocol.Value

convertToEntity_ProteinDetection sqliteContext samplePeptideShaker.AnalysisCollection.ProteinDetection.Value


//sqliteContext.SaveChanges()


let convertToEntity_AnalysisData1 (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.AnalysisData) =
    let analysisDataBasic = 
        AnalysisDataHandler.init(
                                    mzIdentMLXML.SpectrumIdentificationList
                                    |> Array.map (fun spectrumIdentificationList -> convertToEntity_SpectrumIdentificationList dbContext spectrumIdentificationList)
                                )
    let analysisDataWithProteinDetectionList = 
        (match mzIdentMLXML.ProteinDetectionList with
            |Some x -> AnalysisDataHandler.addProteinDetectionList
                        (convertToEntity_ProteinDetectionList dbContext x)
                        analysisDataBasic
            |None -> analysisDataBasic
        )
    let analysisDataWithMzIdentMLDocument =
        AnalysisDataHandler.addMzIdentMLDocument (MzIdentMLDocumentHandler.tryFindByID dbContext "PeptideShaker v1.12.2"|> (fun item -> match item with
                                                                                                                                        | Some x -> x
                                                                                                                                        | None -> null
                                                                                                                            )) analysisDataWithProteinDetectionList
    analysisDataWithMzIdentMLDocument



let t =
    samplePeptideShaker.DataCollection.AnalysisData
    |> (fun item -> convertToEntity_AnalysisData1 sqliteContext item) 

t
|> AnalysisDataHandler.addToContext sqliteContext

1+1



let convertToEntity_ProteinDetection1 (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.ProteinDetection) =
        let proteinDetectionBasic = 
            ProteinDetectionHandler.init(
                                         (ProteinDetectionListHandler.tryFindByID dbContext mzIdentMLXML.ProteinDetectionListRef|> (fun item -> match item with
                                                                                                                                                | Some x -> x
                                                                                                                                                | None -> null
                                                                                                                                    )),
                                         (ProteinDetectionProtocolHandler.tryFindByID dbContext mzIdentMLXML.ProteinDetectionProtocolRef|> (fun item -> match item with
                                                                                                                                                        | Some x -> x
                                                                                                                                                        | None -> null
                                                                                                                                            )),
                                         mzIdentMLXML.InputSpectrumIdentifications
                                         |> Array.map (fun spectrumIdentificationList -> (SpectrumIdentificationListHandler.tryFindByID dbContext spectrumIdentificationList.SpectrumIdentificationListRef|> (fun item -> match item with
                                                                                                                                                                                                                            | Some x -> x
                                                                                                                                                                                                                            | None -> null
                                                                                                                                                                                                               ))),
                                         mzIdentMLXML.Id
                                        )
        let proteinDetectionWithName = 
            setOption ProteinDetectionHandler.addName mzIdentMLXML.Name proteinDetectionBasic
        let proteinDetectionWithActivityDate = 
            setOption ProteinDetectionHandler.addActivityDate mzIdentMLXML.ActivityDate proteinDetectionWithName
        let proteinDetectionWithMzIdentML =
            ProteinDetectionHandler.addMzIdentMLDocument (MzIdentMLDocumentHandler.tryFindByID dbContext "PeptideShaker v1.12.2"|> (fun item -> match item with
                                                                                                                                                | Some x -> x
                                                                                                                                                | None -> null
                                                                                                                                   )) proteinDetectionWithActivityDate
        proteinDetectionBasic

let m =
    convertToEntity_ProteinDetection1 
        sqliteContext 
        samplePeptideShaker.AnalysisCollection.ProteinDetection.Value

m.MzIdentMLDocument