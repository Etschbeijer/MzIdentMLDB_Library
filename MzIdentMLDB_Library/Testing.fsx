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
        |> Seq.iter (fun term -> ContextHandler.addToContext dbContext term |> ignore) 

    let termsPride =
        let ontology =  OntologyHandler.init ("PRIDE")
        fromPride
        |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, ontology))
        |> Seq.iter (fun term -> ContextHandler.addToContext dbContext term |> ignore)

    let termsUnimod =
        let ontology =  OntologyHandler.init ("UNIMOD")
        fromUniMod
        |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, ontology))
        |> Seq.iter (fun term -> ContextHandler.addToContext dbContext term |> ignore)

    let termsUnit_Ontology =
        let ontology =  OntologyHandler.init ("UNIT-ONTOLOGY") 
        fromUnit_Ontology
        |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, ontology))
        |> Seq.iter (fun term -> ContextHandler.addToContext dbContext term |> ignore)

    let userOntology =
        OntologyHandler.init("UserParam")
        |> ContextHandler.addToContext dbContext |> ignore

    dbContext.Database.EnsureCreated() |> ignore
    dbContext.SaveChanges()


let takeTermEntry (dbContext : MzIdentML) (termID : string) =
    query {
        for i in dbContext.Term do
            if i.ID = termID then
               select (i)
          }
    |> Queryable.FirstOrDefault

let takeTermEntryLocal (dbContext : MzIdentML) (termID : string) =
    query {
        for i in dbContext.Term.Local do
            if i.ID = termID then
               select (i)
          }
    |> (fun item -> if (Seq.length item > 0)
                       then Seq.item 0 item
                       else Unchecked.defaultof<Term>)

let testQueryable (dbContext : MzIdentML) (id : string) =
    try
        (takeTermEntry dbContext id)
    with
        | :? System.InvalidOperationException -> takeTermEntryLocal dbContext id
        | _ -> Unchecked.defaultof<Term>

let termTryFindTestI =
    TermHandler.tryFindByID sqliteContext "Test"

termTryFindTestI

let CVParamInit =
    CVParamHandler.init("Test", null,unit=termTryFindTestI)

testQueryable sqliteContext "MS:0000000"
testQueryable sqliteContext "I"


takeTermEntry sqliteContext "MS:0000000"
|> (fun item -> sqliteContext.Term.Add(item))
sqliteContext.SaveChanges()

sqliteContext.Term.Find("MS:0000000")
|> (fun item -> sqliteContext.Term.Add(item))
sqliteContext.SaveChanges()

let termTestII =
    let termBasic =
        TermHandler.init("II")
    let termWithName =
        TermHandler.addName "TestName" termBasic
    ContextHandler.addToContext sqliteContext termWithName

let ontologyTest =
    let ontologyBasic =
        OntologyHandler.init("Test")
    let ontologyWithTerm =
        OntologyHandler.addTerm (TermHandler.tryFindByID sqliteContext "I") ontologyBasic
    let ontologyTermWithOntology =
        TermHandler.addOntology ontologyBasic ontologyWithTerm.Terms.[0]
    ContextHandler.addToContext sqliteContext ontologyBasic

let cvParamTest =
    let cvParamBasic =
        CVParamHandler.init("Name", TermHandler.init("I",null,(OntologyHandler.init(""))))
    //let cvParamWithUnit =
    //    CVParamHandler.addUnit cvParamBasic (CVParamHandler.findTermByID sqliteContext "II")
    sqliteContext.CVParam.Add(cvParamBasic)

let cvParamTestI =
    let cvParamBasic =
        CVParamHandler.init("Name", TermHandler.init("I",null,(OntologyHandler.init(""))))
    //let cvParamWithUnit =
    //    CVParamHandler.addUnit cvParamBasic (CVParamHandler.findTermByID sqliteContext "II")
    ContextHandler.addToContext sqliteContext cvParamBasic

//let analysisSoftwareTest =
//    let I = AnalysisSoftwareHandler.init(cvParamTest)
//    AnalysisSoftwareHandler.addToContext sqliteContext I

sqliteContext.SaveChanges()

let organizationTest =
    let organizationBasic =
        OrganizationHandler.init("I","Test")
    let organizationDetail =
        CVParamHandler.tryFindByID sqliteContext "I"
    //let organizationDetailWithUnit =
    //    CVParamHandler.addUnit organizationDetail (TermHandler.init("", null, (OntologyHandler.init(""))))
    //let organizationWithDetail = 
    //    OrganizationHandler.addDetail organizationBasic organizationDetailWithUnit
    ContextHandler.addToContext sqliteContext organizationBasic

sqliteContext.Organization.Find("I")
sqliteContext.SaveChanges()

let takeTermEntryI (dbContext : MzIdentML) (termID : string) =
    query {
        for i in dbContext.Term do
            if i.ID = termID then
                select (i, i.Ontology)
          }
    |> Seq.toArray

let takeOntologyEntry (dbContext : MzIdentML) (ontologyID : string) =
    query {
        for i in dbContext.Ontology do
            if i.ID = ontologyID then
                select (i, i.Terms)
          }
    |> Seq.toArray
    |> (fun item -> item.[0])
    |> (fun (a,_) ->  a)

let testPsiMS =
    let psiMS = takeOntologyEntry sqliteContext "Psi-MS"
    psiMS

  
let replacePsiMS =
    let tmpI = testPsiMS
    sqliteContext.Ontology.Remove(tmpI) |> ignore
    sqliteContext.SaveChanges() |> ignore
    let tmpOnto =
        let tmpII = testPsiMS 
        tmpII.Terms <- null
        tmpII
    let termsWithoutOntologyAdded = 
        testPsiMS.Terms
        |> Seq.map (fun item -> TermHandler.addOntology tmpOnto item) |> ignore
        testPsiMS
    termsWithoutOntologyAdded
    //sqliteContext.Ontology.Add(testPsiMS) |> ignore
    //sqliteContext.SaveChanges()



//Test of XML-PArser//////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////

type SchemePeptideShaker = XmlProvider<Schema = "..\MzIdentMLDB_Library\XML_Files\MzIdentMLScheme1_2.xsd">
let xmlFile = SchemePeptideShaker.Load("..\MzIdentMLDB_Library\XML_Files\PeptideShaker_mzid_1_2_example.mzid")


let xmlFragmentArray = 
    xmlFile.DataCollection.AnalysisData.SpectrumIdentificationList
    |> Array.map (fun spectrumIdentification -> spectrumIdentification.SpectrumIdentificationResults
                                                |> Array.map (fun item -> item.SpectrumIdentificationItems
                                                                          |> Array.map (fun item -> match item.Fragmentation with
                                                                                                    |Some x -> x.IonTypes
                                                                                                               |> Array.map (fun item -> item.Index.Value)
                                                                                                    | None -> null)

                                                             )
                 )

let xmlFrame =
    xmlFile.AnalysisProtocolCollection.SpectrumIdentificationProtocols
    |> Array.map (fun item -> match item.DatabaseTranslation with
                              |Some x -> match x.Frames with
                                         |Some x -> x
                                         |None -> null
                              |None -> null)

let xmlPI =
    xmlFile.DataCollection.AnalysisData.SpectrumIdentificationList
    |> Array.map (fun spectrumIdentification -> spectrumIdentification.SpectrumIdentificationResults
                                                |> Array.map (fun item -> item.SpectrumIdentificationItems
                                                                          |> Array.map (fun item -> match item.CalculatedPi with
                                                                                                    |Some x -> float x
                                                                                                    | None -> Unchecked.defaultof<float>)

                                                             )
                 )

let xmlOntology =
    xmlFile.CvList
    |> Array.map (fun item -> item.FullName)

let ontologyTestI =
    xmlOntology
    |> Array.map (fun item -> OntologyHandler.tryFindByID sqliteContext item)

//type PersonenVerzeichnis(personenVerzeichnisid : int, name : string, abteilungen : Abteilung, rollen : Rolle) =
//    let mutable personenVerzeichnisid = personenVerzeichnisid
//    let mutable name                  = name
//    let mutable abteilungen   = abteilungen
//    let mutable rollen        = rollen
//    //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
//    member this.PersonenVerzeichnisID with get() = personenVerzeichnisid and set(value) = personenVerzeichnisid <- value
//    member this.Name          with get()         = name          and set(value)         = name                  <- value
//    member this.Abteilungen   with get()         = abteilungen   and set(value)         = abteilungen           <- value
//    member this.Rollen        with get()         = rollen        and set(value)         = rollen                <- value

//type CVParam_Base (id : string, name : string, value : string, term : Term, unit : Term, unitName : string, rowVersion : DateTime) =
//    new() = CVParam_Base
//    let mutable id'        = id
//    let mutable name' = name
//    let mutable value'     = value
//    let mutable term'      = term
//    let mutable unit'      = unit
//    let mutable unitName'  = unitName

//    member this.ID         with get() = id' and set(value) = id' <- value
//    member this.Name       with get() = name' and set(value) = id' <- value
//    member this.Value      with get() = value' and set(value) = id' <- value
//    member this.Term       with get() = term' and set(value) = id' <- value
//    member this.Unit       with get() = unit' and set(value) = id' <- value
//    member this.UnitName   with get() = unitName' and set(value) = id' <- value
//    member this.RowVersion with get() = rowVersion and set(value) = id' <- value


////Test parser for whole xml-file

//let parseWholeXMLFile (dbContext:MzIdentML) (xmlContext:SchemePeptideShaker.MzIdentMl) =
    
//    let parsedOrganizations =
//        match xmlContext.AuditCollection with
//        |Some x -> x.Organizations
//                   |> Array.map (fun organization -> convertToEntity_Organization dbContext organization)
//        |None -> null
  
//    let parsedPersons =
//        match xmlContext.AuditCollection with
//        |Some x -> x.Persons
//                   |> Array.map (fun person -> convertToEntity_Person dbContext person)
//        |None -> null
        
//    let parsedAnalysisSoftwares =
//        match xmlContext.AnalysisSoftwareList with
//        |Some x -> x.AnalysisSoftwares
//                   |> Array.map (fun analysisSoftware -> convertToEntity_AnalysisSoftware dbContext analysisSoftware)
//        |None -> null

//    let parsedSamples =
//        match xmlContext.AnalysisSampleCollection with
//        |Some x -> x.Samples
//                   |> Array.map (fun sample -> convertToEntity_Sample dbContext sample)
//        |None -> null

//    let parsedSubSamples =
//        match xmlContext.AnalysisSampleCollection with
//        |Some x -> x.Samples
//                   |> Array.map (fun sample -> sample.SubSamples
//                                               |> Array.map (fun subSample -> convertToEntity_SubSample dbContext subSample)
//                                )
//        |None -> null

//    let parsedPeptides =
//        match xmlContext.SequenceCollection with
//        |Some x -> x.Peptides
//                   |> Array.map (fun peptide -> convertToEntity_Peptide dbContext peptide)
//        |None -> null

//    let parsedDBSequences =
//        match xmlContext.SequenceCollection with
//        |Some x -> x.DbSequences
//                   |> Array.map (fun dbSequence -> convertToEntity_DBSequence dbContext dbSequence)
//        |None -> null

//    let parsedPeptideEvidences =
//        match xmlContext.SequenceCollection with
//        |Some x -> x.PeptideEvidences
//                   |> Array.map (fun peptideEvidences -> convertToEntity_PeptideEvidence dbContext peptideEvidences)
//        |None -> null

//    let parsedMassTables =
//        xmlContext.AnalysisProtocolCollection.SpectrumIdentificationProtocols
//        |> Array.map (fun spectrumIdentificationProtocol -> spectrumIdentificationProtocol.MassTables
//                                                            |> Array.map (fun massTable -> convertToEntity_MassTable dbContext massTable)
//                     )

//    let parsedEnzymes =
//        xmlContext.AnalysisProtocolCollection.SpectrumIdentificationProtocols
//        |> Array.map (fun spectrumIdentificationProtocol -> match spectrumIdentificationProtocol.Enzymes with
//                                                            |Some x -> x.Enzymes
//                                                                       |> Array.map (fun enzyme -> convertToEntity_Enzyme dbContext enzyme)
//                                                            |None -> null
                                                            
//                     )

//    let parsedSpectrumIdentificationProtocols =
//        xmlContext.AnalysisProtocolCollection.SpectrumIdentificationProtocols
//        |> Array.map (fun spectrumIdentificationProtocol -> convertToEntity_SpectrumIdentificationProtocol dbContext spectrumIdentificationProtocol)

//    let parsedProteinDetectionProtocol =
//        match xmlContext.AnalysisProtocolCollection.ProteinDetectionProtocol with
//        |Some x -> convertToEntity_ProteinDetectionProtocol dbContext x
//        |None -> null

//    let parsedMzIdentMl =
//        convertToEntity_MzIdentML dbContext xmlContext

//    sqliteContext.SaveChanges()

//#time
//parseWholeXMLFile sqliteContext xmlFile

//let parsedOrganizations =
//    match xmlFile.AuditCollection with
//    |Some x -> x.Organizations
//                |> Array.map (fun organization -> convertToEntity_Organization sqliteContext organization)
//    |None -> null
  
//let parsedPersons =
//    match xmlFile.AuditCollection with
//    |Some x -> x.Persons
//                |> Array.map (fun person -> convertToEntity_Person sqliteContext person)
//    |None -> null
        
//let parsedAnalysisSoftwares =
//    match xmlFile.AnalysisSoftwareList with
//    |Some x -> x.AnalysisSoftwares
//                |> Array.map (fun analysisSoftware -> convertToEntity_AnalysisSoftware sqliteContext analysisSoftware)
//    |None -> null

//let parsedSamples =
//    match xmlFile.AnalysisSampleCollection with
//    |Some x -> x.Samples
//                |> Array.map (fun sample -> convertToEntity_Sample sqliteContext sample)
//    |None -> null

//let parsedSubSamples =
//    match xmlFile.AnalysisSampleCollection with
//    |Some x -> x.Samples
//                |> Array.map (fun sample -> sample.SubSamples
//                                            |> Array.map (fun subSample -> convertToEntity_SubSample sqliteContext subSample)
//                            )
//    |None -> null

//let parsedPeptides =
//    match xmlFile.SequenceCollection with
//    |Some x -> x.Peptides
//                |> Array.map (fun peptide -> convertToEntity_Peptide sqliteContext peptide)
//    |None -> null

//let parsedDBSequences =
//    match xmlFile.SequenceCollection with
//    |Some x -> x.DbSequences
//                |> Array.map (fun dbSequence -> convertToEntity_DBSequence sqliteContext dbSequence)
//    |None -> null

//let parsedPeptideEvidences =
//    match xmlFile.SequenceCollection with
//    |Some x -> x.PeptideEvidences
//                |> Array.map (fun peptideEvidences -> convertToEntity_PeptideEvidence sqliteContext peptideEvidences)
//    |None -> null

//let parsedMassTables =
//    xmlFile.AnalysisProtocolCollection.SpectrumIdentificationProtocols
//    |> Array.map (fun spectrumIdentificationProtocol -> spectrumIdentificationProtocol.MassTables
//                                                        |> Array.map (fun massTable -> convertToEntity_MassTable sqliteContext massTable)
//                    )

//let parsedEnzymes =
//    xmlFile.AnalysisProtocolCollection.SpectrumIdentificationProtocols
//    |> Array.map (fun spectrumIdentificationProtocol -> match spectrumIdentificationProtocol.Enzymes with
//                                                        |Some x -> x.Enzymes
//                                                                    |> Array.map (fun enzyme -> convertToEntity_Enzyme sqliteContext enzyme)
//                                                        |None -> null
                                                            
//                    )

//let parsedSpectrumIdentificationProtocols =
//    xmlFile.AnalysisProtocolCollection.SpectrumIdentificationProtocols
//    |> Array.map (fun spectrumIdentificationProtocol -> convertToEntity_SpectrumIdentificationProtocol sqliteContext spectrumIdentificationProtocol)

//let parsedProteinDetectionProtocol =
//    match xmlFile.AnalysisProtocolCollection.ProteinDetectionProtocol with
//    |Some x -> convertToEntity_ProteinDetectionProtocol sqliteContext x
//    |None -> null

//let parsedMzIdentMl =
//    convertToEntity_MzIdentML sqliteContext xmlFile



//let convertToEntity_MzIdentMLI (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.MzIdentMl) =
//        let mzIdentMLBasic =  
//            MzIdentMLHandler.init(
//                                  convertToEntity_Inputs dbContext mzIdentMLXML.DataCollection.Inputs,
//                                  mzIdentMLXML.Version,
//                                  mzIdentMLXML.AnalysisCollection.SpectrumIdentifications
//                                  |> Array.map (fun spectrumIdentification -> convertToEntity_SpectrumIdentification dbContext spectrumIdentification),
//                                  mzIdentMLXML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
//                                  |> Array.map (fun spectrumIdentificationProtocol -> SpectrumIdentificationProtocolHandler.tryFindByID dbContext spectrumIdentificationProtocol.Id),
//                                  convertToEntity_AnalysisData dbContext mzIdentMLXML.DataCollection.AnalysisData,
//                                  mzIdentMLXML.Id
//                                 )
//        //let mzIdentMLWithName = setOption MzIdentMLHandler.addName mzIdentMLXML.Name mzIdentMLBasic
//        //let mzIdentMLWithAnalysisSoftwares =
//        //        match mzIdentMLXML.AnalysisSoftwareList with
//        //         |Some x -> MzIdentMLHandler.addAnalysisSoftwares      
//        //                        (x.AnalysisSoftwares
//        //                         |> Array.map (fun analysisSoftware -> AnalysisSoftwareHandler.tryFindByID dbContext analysisSoftware.Id)
//        //                        )
//        //                        mzIdentMLWithName
//        //         |None -> mzIdentMLWithName
//        //let mzIDentMLWithProvider =
//        //        (match mzIdentMLXML.Provider with
//        //         |Some x -> MzIdentMLHandler.addProvider
//        //                        (convertToEntity_Provider dbContext x)
//        //                        mzIdentMLWithAnalysisSoftwares
//        //         |None -> mzIdentMLWithAnalysisSoftwares
//        //        )
//        //let mzIdentMLWithPersons =
//        //        match mzIdentMLXML.AuditCollection with
//        //         |Some x -> MzIdentMLHandler.addPersons
//        //                        (x.Persons
//        //                         |> Array.map (fun person -> PersonHandler.tryFindByID dbContext person.Id)
//        //                        )
//        //                        mzIDentMLWithProvider
//        //         |None -> mzIDentMLWithProvider
//        //let mzIdentMLWithOrganizations =
//        //        match mzIdentMLXML.AuditCollection with
//        //         |Some x -> MzIdentMLHandler.addOrganizations
//        //                        (x.Organizations
//        //                         |> Array.map (fun organization -> OrganizationHandler.tryFindByID dbContext organization.Id)
//        //                        )
//        //                        mzIdentMLWithPersons
//        //         |None -> mzIdentMLWithPersons
//        //let mzIdentMLWithSamples =
//        //        match mzIdentMLXML.AnalysisSampleCollection with
//        //         |Some x -> MzIdentMLHandler.addSamples
//        //                        (x.Samples
//        //                         |>Array.map (fun sample -> SampleHandler.tryFindByID dbContext sample.Id)
//        //                        )
//        //                        mzIdentMLWithOrganizations
//        //         |None -> mzIdentMLWithOrganizations
//        //let mzIdentMLWithDBSequences =
//        //        match mzIdentMLXML.SequenceCollection with
//        //         |Some x -> MzIdentMLHandler.addDBSequences
//        //                        (x.DbSequences
//        //                         |> Array.map (fun dbSequence -> DBSequenceHandler.tryFindByID dbContext dbSequence.Id)
//        //                        )
//        //                        mzIdentMLWithSamples
//        //         |None -> mzIdentMLWithSamples
//        //let mzIdentMLWithPeptides =
//        //        match mzIdentMLXML.SequenceCollection with
//        //         |Some x -> MzIdentMLHandler.addPeptides
//        //                        (x.Peptides
//        //                         |> Array.map (fun peptide -> PeptideHandler.tryFindByID dbContext peptide.Id)
//        //                        )
//        //                        mzIdentMLWithDBSequences
//        //         |None -> mzIdentMLWithDBSequences
//        //let mzIdentMLWithPeptideEvidences =
//        //        match mzIdentMLXML.SequenceCollection with
//        //         |Some x -> MzIdentMLHandler.addPeptideEvidences
//        //                        (x.PeptideEvidences
//        //                         |> Array.map (fun peptideEvidence -> PeptideEvidenceHandler.tryFindByID dbContext peptideEvidence.Id)
//        //                        )
//        //                        mzIdentMLWithPeptides
//        //         |None -> mzIdentMLWithPeptides
//        //let mzIdentMLWithProteinDetection =
//        //    (match mzIdentMLXML.AnalysisCollection.ProteinDetection with
//        //     |Some x -> MzIdentMLHandler.addProteinDetection
//        //                    (convertToEntity_ProteinDetection dbContext x)
//        //                    mzIdentMLWithPeptideEvidences
//        //     |None -> mzIdentMLWithPeptideEvidences
//        //    )
//        //let mzIdentMLWithProteinDetectionProtocol =
//        //    (match mzIdentMLXML.AnalysisProtocolCollection.ProteinDetectionProtocol with
//        //     |Some x -> MzIdentMLHandler.addProteinDetectionProtocol
//        //                    (ProteinDetectionProtocolHandler.tryFindByID dbContext x.Id)
//        //                    mzIdentMLWithProteinDetection
//        //     |None -> mzIdentMLWithProteinDetection
//            //) 
//        //let mzIdentMLWithBiblioGraphicReference =
//        //    MzIdentMLHandler.addBiblioGraphicReferences
//        //        (mzIdentMLXML.BibliographicReferences
//        //         |> Array.map (fun bibliographicReference -> convertToEntity_BiblioGraphicReference dbContext bibliographicReference)
//        //        )
//        //        mzIdentMLWithProteinDetectionProtocol
//        //MzIdentMLHandler.addToContext dbContext 
//        mzIdentMLBasic

//let testMzIdentML =
//    convertToEntity_MzIdentMLI sqliteContext xmlFile

//testMzIdentML
//    |> sqliteContext.Add

//initStandardDB sqliteContext

let tryAdd (dbContext:MzIdentML) (id:'a) (item:'b) =
    let x = dbContext.Find(item.GetType(),id)
    match x with
    |null -> dbContext.Add item |> ignore
             true
    |_ -> false

let tryAddAndInsert (dbContext:MzIdentML) (id:'a) (item:'b) =
    let x = dbContext.Find(item.GetType(),id)
    match x with
    |null -> dbContext.Add item      |> ignore
             dbContext.SaveChanges() |> ignore
             true
    |_ -> false

let tryAddAndInsert' (dbContext:MzIdentML) (item:'b) =
    try
        dbContext.Add item      |> ignore
        dbContext.SaveChanges() |> ignore
        true
    with
    | _ -> false 


