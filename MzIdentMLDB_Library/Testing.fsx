////Apply functions
        
//#r "System.ComponentModel.DataAnnotations.dll"
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

open System.Collections.Generic
open MzIdentMLDataBase.DataContext.DataContext
open MzIdentMLDataBase.DataContext.EntityTypes
open MzIdentMLDataBase.InsertStatements.ObjectHandlers
open MzIdentMLDataBase.InsertStatements.ManipulateDataContextAndDB
open MzIdentMLDataBase.InsertStatements.InitializeStandardDB
open MzIdentMLDataBase.XMLParsing


let context = configureSQLiteContextMzIdentML standardDBPathSQLite
initStandardDB context

let termTestI =
    let termBasic =
        TermHandler.init("I")
    let termWithName =
        TermHandler.addName termBasic  "TestName"
    TermHandler.addToContext context termWithName

let termTestII =
    let termBasic =
        TermHandler.init("II")
    let termWithName =
        TermHandler.addName termBasic  "TestName"
    TermHandler.addToContext context termWithName

let ontologyTest =
    let ontologyBasic =
        OntologyHandler.init("Test")
    let ontologyWithTerm =
        OntologyHandler.addTerm ontologyBasic (OntologyHandler.findTermByID context "I")
    let ontologyTermWithOntology =
        TermHandler.addOntology ontologyWithTerm.Terms.[0] ontologyBasic
    OntologyHandler.addToContext context ontologyBasic

let cvParamTest =
    let cvParamBasic =
        CVParamHandler.init("Name", TermHandler.init("I",null,(OntologyHandler.init(""))), "I")
    //let cvParamWithUnit =
    //    CVParamHandler.addUnit cvParamBasic (CVParamHandler.findTermByID context "II")
    CVParamHandler.addToContext context cvParamBasic

let organizationTest =
    let organizationBasic =
        OrganizationHandler.init("I","Test")
    let organizationDetail =
        OrganizationHandler.findCVParamByID context "I"
    //let organizationDetailWithUnit =
    //    CVParamHandler.addUnit organizationDetail (TermHandler.init("", null, (OntologyHandler.init(""))))
    //let organizationWithDetail = 
    //    OrganizationHandler.addDetail organizationBasic organizationDetailWithUnit
    OrganizationHandler.addToContext context organizationBasic

context.Organization.Find("I")
context.SaveChanges()

let takeTermEntry (dbContext : MzIdentMLContext) (termID : string) =
    query {
        for i in dbContext.Term do
            if i.ID = termID then
                select (i, i.Ontology)
          }
    |> Seq.toArray

let takeOntologyEntry (dbContext : MzIdentMLContext) (ontologyID : string) =
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
        |> Seq.map (fun item -> TermHandler.addOntology item tmpOnto) |> ignore
        testPsiMS
    termsWithoutOntologyAdded
    //context.Ontology.Add(testPsiMS) |> ignore
    //context.SaveChanges()



//Test of XML-PArser//////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////
open FSharp.Data

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

//context.SaveChanges()


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
    |> Array.map (fun item -> OntologyHandler.findOntologyByID context item)