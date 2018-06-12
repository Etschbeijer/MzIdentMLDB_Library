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
//#r @"C:\Users\PatrickB\Source\Repos\MzIdentML_Library\MzIdentML_Library\bin\Debug\netstandard2.0\FSharp.Care.dll"
//#r @"C:\Users\PatrickB\Source\Repos\MzIdentML_Library\MzIdentML_Library\bin\Debug\netstandard2.0\FSharp.Care.IO.dll"
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
open MzIdentMLDataBase.DataContext.DataContext
open MzIdentMLDataBase.DataContext.EntityTypes
open MzIdentMLDataBase.InsertStatements.ManipulateDataContextAndDB
open MzIdentMLDataBase.InsertStatements.InitializeStandardDB
open MzIdentMLDataBase.InsertStatements.ObjectHandlers
open MzIdentMLDataBase.XMLParsing

let context = configureSQLiteContextMzIdentML standardDBPathSQLite
//initStandardDB context

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
    TermHandler.tryFindByID context "Test"

termTryFindTestI

let CVParamInit =
    CVParamHandler.init("Test", null,unit=termTryFindTestI)

testQueryable context "MS:0000000"
testQueryable context "I"


takeTermEntry context "MS:0000000"
|> (fun item -> context.Term.Add(item))
context.SaveChanges()

context.Term.Find("MS:0000000")
|> (fun item -> context.Term.Add(item))
context.SaveChanges()

let termTestII =
    let termBasic =
        TermHandler.init("II")
    let termWithName =
        TermHandler.addName "TestName" termBasic
    TermHandler.addToContext context termWithName

let ontologyTest =
    let ontologyBasic =
        OntologyHandler.init("Test")
    let ontologyWithTerm =
        OntologyHandler.addTerm (TermHandler.tryFindByID context "I") ontologyBasic
    let ontologyTermWithOntology =
        TermHandler.addOntology ontologyBasic ontologyWithTerm.Terms.[0]
    OntologyHandler.addToContext context ontologyBasic

let cvParamTest =
    let cvParamBasic =
        CVParamHandler.init("Name", TermHandler.init("I",null,(OntologyHandler.init(""))))
    //let cvParamWithUnit =
    //    CVParamHandler.addUnit cvParamBasic (CVParamHandler.findTermByID context "II")
    context.CVParam.Add(cvParamBasic)

let cvParamTestI =
    let cvParamBasic =
        CVParamHandler.init("Name", TermHandler.init("I",null,(OntologyHandler.init(""))))
    //let cvParamWithUnit =
    //    CVParamHandler.addUnit cvParamBasic (CVParamHandler.findTermByID context "II")
    CVParamHandler.addToContext context cvParamBasic

//let analysisSoftwareTest =
//    let I = AnalysisSoftwareHandler.init(cvParamTest)
//    AnalysisSoftwareHandler.addToContext context I

context.SaveChanges()

let organizationTest =
    let organizationBasic =
        OrganizationHandler.init("I","Test")
    let organizationDetail =
        CVParamHandler.tryFindByID context "I"
    //let organizationDetailWithUnit =
    //    CVParamHandler.addUnit organizationDetail (TermHandler.init("", null, (OntologyHandler.init(""))))
    //let organizationWithDetail = 
    //    OrganizationHandler.addDetail organizationBasic organizationDetailWithUnit
    OrganizationHandler.addToContext context organizationBasic

context.Organization.Find("I")
context.SaveChanges()

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
    let psiMS = takeOntologyEntry context "Psi-MS"
    psiMS

  
let replacePsiMS =
    let tmpI = testPsiMS
    context.Ontology.Remove(tmpI) |> ignore
    context.SaveChanges() |> ignore
    let tmpOnto =
        let tmpII = testPsiMS 
        tmpII.Terms <- null
        tmpII
    let termsWithoutOntologyAdded = 
        testPsiMS.Terms
        |> Seq.map (fun item -> TermHandler.addOntology tmpOnto item) |> ignore
        testPsiMS
    termsWithoutOntologyAdded
    //context.Ontology.Add(testPsiMS) |> ignore
    //context.SaveChanges()



//Test of XML-PArser//////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////

type SchemePeptideShaker = XmlProvider<Schema = "..\MzIdentMLDB_Library\XML_Files\MzIdentMLScheme1_2.xsd">
let xmlFile = SchemePeptideShaker.Load("..\MzIdentMLDB_Library\XML_Files\PeptideShaker_mzid_1_2_example.mzid")


let organizations = 
    (match xmlFile.AuditCollection with
        |Some x -> x.Organizations 
                   |> Array.map (fun organization -> convertToEntity_Organization context organization)
        |None -> Unchecked.defaultof<array<bool>>
    )
                                                          
let persons =
    (match xmlFile.AuditCollection with
        |Some x -> x.Persons 
                   |> Array.map (fun person -> convertToEntity_Person context person)
        |None -> Unchecked.defaultof<array<bool>>
    )  

let analysisSoftwares = 
    (match xmlFile.AnalysisSoftwareList with
        | Some x -> x.AnalysisSoftwares 
                    |> Array.map (fun analysisSoftware -> convertToEntity_AnalysisSoftware context analysisSoftware)
        |None -> Unchecked.defaultof<array<bool>>
    )

context.SaveChanges()


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
    |> Array.map (fun item -> OntologyHandler.tryFindByID context item)

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


//Test parser for whole xml-file

let parseWholeXMLFile (dbContext:MzIdentML) (xmlContext:SchemePeptideShaker.MzIdentMl) =
    
    let parsedOrganizations =
        match xmlContext.AuditCollection with
        |Some x -> x.Organizations
                   |> Array.map (fun organization -> convertToEntity_Organization dbContext organization)
        |None -> Unchecked.defaultof<array<bool>>
  
    let parsedPersons =
        match xmlContext.AuditCollection with
        |Some x -> x.Persons
                   |> Array.map (fun person -> convertToEntity_Person dbContext person)
        |None -> Unchecked.defaultof<array<bool>> 
        
    let parsedAnalysisSoftwares =
        match xmlContext.AnalysisSoftwareList with
        |Some x -> x.AnalysisSoftwares
                   |> Array.map (fun analysisSoftware -> convertToEntity_AnalysisSoftware dbContext analysisSoftware)
        |None -> Unchecked.defaultof<array<bool>>

    let parsedSamples =
        match xmlContext.AnalysisSampleCollection with
        |Some x -> x.Samples
                   |> Array.map (fun sample -> convertToEntity_Sample dbContext sample)
        |None -> Unchecked.defaultof<array<bool>>

    let parsedSubSamples =
        match xmlContext.AnalysisSampleCollection with
        |Some x -> x.Samples
                   |> Array.map (fun sample -> sample.SubSamples
                                               |> Array.map (fun subSample -> convertToEntity_SubSample dbContext subSample)
                                )
        |None -> Unchecked.defaultof<array<array<bool>>>

    let parsedPeptides =
        match xmlContext.SequenceCollection with
        |Some x -> x.Peptides
                   |> Array.map (fun peptide -> convertToEntity_Peptide dbContext peptide)
        |None -> Unchecked.defaultof<array<bool>>

    let parsedDBSequences =
        match xmlContext.SequenceCollection with
        |Some x -> x.DbSequences
                   |> Array.map (fun dbSequence -> convertToEntity_DBSequence dbContext dbSequence)
        |None -> Unchecked.defaultof<array<bool>>

    let parsedPeptideEvidences =
        match xmlContext.SequenceCollection with
        |Some x -> x.PeptideEvidences
                   |> Array.map (fun peptideEvidences -> convertToEntity_PeptideEvidence dbContext peptideEvidences)
        |None -> Unchecked.defaultof<array<bool>>

    let parsedMassTables =
        xmlContext.AnalysisProtocolCollection.SpectrumIdentificationProtocols
        |> Array.map (fun spectrumIdentificationProtocol -> spectrumIdentificationProtocol.MassTables
                                                            |> Array.map (fun massTable -> convertToEntity_MassTable dbContext massTable)
                     )

    let parsedEnzymes =
        xmlContext.AnalysisProtocolCollection.SpectrumIdentificationProtocols
        |> Array.map (fun spectrumIdentificationProtocol -> match spectrumIdentificationProtocol.Enzymes with
                                                            |Some x -> x.Enzymes
                                                                       |> Array.map (fun enzyme -> convertToEntity_Enzyme dbContext enzyme)
                                                            |None -> Unchecked.defaultof<array<bool>>
                                                            
                     )

    let parsedSpectrumIdentificationProtocols =
        xmlContext.AnalysisProtocolCollection.SpectrumIdentificationProtocols
        |> Array.map (fun spectrumIdentificationProtocol -> convertToEntity_SpectrumIdentificationProtocol dbContext spectrumIdentificationProtocol)

    let parsedProteinDetectionProtocol =
        match xmlContext.AnalysisProtocolCollection.ProteinDetectionProtocol with
        |Some x -> convertToEntity_ProteinDetectionProtocol dbContext x
        |None -> Unchecked.defaultof<bool>

    let parsedMzIdentMl =
        convertToEntity_MzIdentML dbContext xmlContext
    context.SaveChanges()

#time
parseWholeXMLFile context xmlFile

let x =
    xmlFile.DataCollection.AnalysisData.SpectrumIdentificationList
    |> Array.map (fun item -> item.SpectrumIdentificationResults
                              |> Array.map (fun item -> item.SpectrumIdentificationItems
                                                        |> Array.map (fun item -> convertToEntity_SpectrumIdentificationItem context item)
                                           )
                 )
    |> Array.concat
    |> Array.concat

let y =
    match xmlFile.SequenceCollection with
            |Some x -> x.DbSequences
                       |> Array.map (fun dbSequence -> convertToEntity_DBSequence context dbSequence)
            |None -> Unchecked.defaultof<array<bool>>

let z =
    convertToEntity_MzIdentML context xmlFile

context.SaveChanges()