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

//open MzIdentMLDataBase.XMLParsing


let fileDir = __SOURCE_DIRECTORY__
let standardDBPathSQLite = fileDir + "\Databases\Test.db"

let sqliteContext = ContextHandler.sqliteConnection standardDBPathSQLite

//let sqlConntection = ContextHandler.sqlConnection()
//sqlConntection.Database.EnsureCreated()

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

let termTryFindTestI =
    TermHandler.tryFindByID sqliteContext "Test"

termTryFindTestI

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

#time
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

type SchemePeptideShaker = XmlProvider<Schema = "..\MzIdentMLDB_Library\XML_Files\MzIdentMLScheme1_2.xsd">
let  samplePeptideShaker = SchemePeptideShaker.Load("..\MzIdentMLDB_Library\XML_Files\PeptideShaker_mzid_1_2_example.txt") 

let removeDoubleTerms (xmlMzIdentML : SchemePeptideShaker.MzIdentMl) =
    let cvParamAccessionListOfXMLFile =
        [|
        (if xmlMzIdentML.AnalysisProtocolCollection.ProteinDetectionProtocol.IsSome 
            then xmlMzIdentML.AnalysisProtocolCollection.ProteinDetectionProtocol.Value.Threshold.CvParams
                |> Array.map (fun cvParamItem -> cvParamItem.Accession)
            else [|null|])

        (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
            |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.AdditionalSearchParams.IsSome 
                                                                        then spectrumIdentificationProtocolItem.AdditionalSearchParams. Value.CvParams
                                                                            |> Array.map (fun cvParamItem -> cvParamItem.Accession)

                                                                        else [|null|]))
                                    |> Array.concat

        (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
            |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.Enzymes.IsSome 
                                                                        then 
                                                                            if spectrumIdentificationProtocolItem.Enzymes.IsSome 
                                                                                then spectrumIdentificationProtocolItem.Enzymes.Value.Enzymes
                                                                                     |> Array.map (fun enzymeItem -> enzymeItem.EnzymeName.Value.CvParams
                                                                                                                     |> Array.map (fun cvParamitem -> cvParamitem.Accession))
                                                                                else [|[|null|]|]
                                                                        else [|[|null|]|]))
                                    |> Array.concat
                                    |> Array.concat

        (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
            |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.FragmentTolerance.IsSome
                                                                        then spectrumIdentificationProtocolItem.FragmentTolerance.Value.CvParams
                                                                             |> Array.map (fun cvParamItem -> cvParamItem.Accession)
                                                                        else [|null|]))
                                    |> Array.concat

        (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
            |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.ModificationParams.IsSome 
                                                                        then spectrumIdentificationProtocolItem.ModificationParams.Value.SearchModifications
                                                                             |> Array.map (fun searchModificationItem -> searchModificationItem.SpecificityRules
                                                                                                                            |> Array.map (fun specificityRuleItem -> specificityRuleItem.CvParams
                                                                                                                                                                    |> Array.map (fun cvParamitem -> cvParamitem.Accession)))
                                                                        else [|[|[|null|]|]|]))
                                    |> Array.concat
                                    |> Array.concat
                                    |> Array.concat

        (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
            |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.ParentTolerance.IsSome 
                                                                        then spectrumIdentificationProtocolItem.ParentTolerance.Value.CvParams
                                                                            |> Array.map (fun cvParamItem -> cvParamItem.Accession)
                                                                        else [|null|]))
                                    |> Array.concat

        (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
        |> Array.map (fun spectrumIdentificationProtocolItem -> spectrumIdentificationProtocolItem.Threshold.CvParams
                                                                |> Array.map (fun cvParamItem -> cvParamItem.Accession)))
                                    |> Array.concat

        (if xmlMzIdentML.AnalysisSoftwareList.IsSome 
            then xmlMzIdentML.AnalysisSoftwareList.Value.AnalysisSoftwares
                |> Array.map (fun analysisSoftwareItem -> if analysisSoftwareItem.ContactRole.IsSome 
                                                             then analysisSoftwareItem.ContactRole.Value.Role.CvParam.Accession
                                                             else null)
            else [|null|])

        (if xmlMzIdentML.AnalysisSoftwareList.IsSome 
        then xmlMzIdentML.AnalysisSoftwareList.Value.AnalysisSoftwares
          |> Array.map (fun analysisSoftwareItem -> if analysisSoftwareItem.SoftwareName.CvParam.IsSome 
                                                        then analysisSoftwareItem.SoftwareName.CvParam.Value.Accession
                                                        else null)
        else [|null|])

        (if xmlMzIdentML.AuditCollection.IsSome 
        then xmlMzIdentML.AuditCollection.Value.Organizations
          |> Array.map (fun orgItem -> orgItem.CvParams
                                        |> Array.map (fun cvParamItem -> cvParamItem.Accession))
                            |> Array.concat
        else [|null|])

        (if xmlMzIdentML.AuditCollection.IsSome 
            then xmlMzIdentML.AuditCollection.Value.Persons
                |> Array.map (fun personItem -> personItem.CvParams
                                                |> Array.map (fun cvParamItem -> cvParamItem.Accession))
                                                |> Array.concat
        else [|null|])

        (if xmlMzIdentML.DataCollection.AnalysisData.ProteinDetectionList.IsSome 
            then xmlMzIdentML.DataCollection.AnalysisData.ProteinDetectionList.Value.CvParams
                |> Array.map (fun cvParamItem -> cvParamItem.Accession)
            else [|null|])

        (if xmlMzIdentML.DataCollection.AnalysisData.ProteinDetectionList.IsSome 
            then xmlMzIdentML.DataCollection.AnalysisData.ProteinDetectionList.Value.ProteinAmbiguityGroups
                |> Array.map (fun proteinAmbiguityGroupsItem -> proteinAmbiguityGroupsItem.CvParams
                                                                |> Array.map (fun cvParamItem -> cvParamItem.Accession))
                                                                |> Array.concat
            else [|null|])

        (xmlMzIdentML.DataCollection.AnalysisData.SpectrumIdentificationList
            |> Array.map (fun spectrumIdentificationitem -> if spectrumIdentificationitem.FragmentationTable.IsSome
                                                                then spectrumIdentificationitem.FragmentationTable.Value.Measures
                                                                    |> Array.map (fun measureItem -> measureItem.CvParams
                                                                                                     |> Array.map (fun cvParamItem -> cvParamItem.Accession))
                                                                else [|[|null|]|])
                                                 |> Array.concat
                                                 |> Array.concat)

        (xmlMzIdentML.DataCollection.AnalysisData.SpectrumIdentificationList
            |> Array.map (fun spectrumIdentificationItem -> spectrumIdentificationItem.SpectrumIdentificationResults
                                                            |> Array.map (fun spectrumIdentificationResultItem -> spectrumIdentificationResultItem.CvParams
                                                                                                                    |> Array.map (fun cvParamItem -> cvParamItem.Accession)))
                                                              |> Array.concat
                                                              |> Array.concat)

        (if xmlMzIdentML.SequenceCollection.IsSome
            then xmlMzIdentML.SequenceCollection.Value.DbSequences
                 |> Array.map (fun dbSeqItem -> dbSeqItem.CvParams
                                                |> Array.map (fun cvParamitem -> cvParamitem.Accession))
                                                |> Array.concat
            else [|null|])

        (if xmlMzIdentML.SequenceCollection.IsSome 
            then xmlMzIdentML.SequenceCollection.Value.Peptides
                |> Array.map (fun peptideItem -> peptideItem.Modifications
                                                    |> Array.map (fun modificationItem -> modificationItem.CvParams
                                                                                            |> Array.map (fun cvParamItem -> cvParamItem.Accession)))
                                                |> Array.concat
                                                |> Array.concat
        else [|null|])
    |]
    cvParamAccessionListOfXMLFile
    |> Array.concat

let keepDuplicates collection =
    let d = System.Collections.Generic.Dictionary()
    [| for i in collection do match d.TryGetValue i with
                              | (false,_) -> d.[i] <- (); yield i
                              | _ -> () |]

let test1 = removeDoubleTerms samplePeptideShaker
let test2 = keepDuplicates test1

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
