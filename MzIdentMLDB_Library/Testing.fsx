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



//open MzIdentMLDataBase.XMLParsing

let context = configureSQLiteContextMzIdentML standardDBPathSQLite
//initStandardDB context


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

//replacePsiMS
//|> Seq.map (fun item -> context.Term.Add(item))

//Test Organization and Person
open FSharp.Data

type SchemePeptideShaker = XmlProvider<Schema = "..\MzIdentMLDB_Library\XML_Files\MzIdentMLScheme1_2.xsd">
let xmlCVParams = SchemePeptideShaker.Load("..\MzIdentMLDB_Library\XML_Files\PeptideShaker_mzid_1_2_example.mzid")

let findTerm =
    context.Term.Find("MS:000000")

let addDetails (dbContext:MzIdentMLContext) (xmlType:SchemePeptideShaker.Organization) =
        (xmlType.CvParams 
            |> Array.map (fun item -> CVParamHandler.init(item.Name, context.Term.Find(item.Accession)))
        )

xmlCVParams.AuditCollection.Value.Organizations
|> Array.map (fun item -> addDetails context item)

let convertOrganization (dbContext:MzIdentMLContext) (xmlType:SchemePeptideShaker.Organization) =
    let init = OrganizationHandler.init()
    let addName = OrganizationHandler.addName init xmlType.Name.Value
    let addDetails = 
        OrganizationHandler.addDetails 
            addName 
            (xmlType.CvParams 
                |> Array.map (fun item -> CVParamHandler.init(item.Name, context.Term.Find(item.Accession)))
            )
    let addParent = OrganizationHandler.addParent addName "CSB"
    addParent

let organizations =
    xmlCVParams.AuditCollection.Value.Organizations
    |> Array.map (fun organization -> convertOrganization context organization)
    //|> Array.map (fun item -> item.Details)
    |> Array.map (fun item -> OrganizationHandler.addToContext context item)

let testI =
    xmlCVParams.AuditCollection.Value.Organizations
    |> Array.collect (fun organization -> organization.CvParams
                                          |> Array.map (fun item -> CVParamHandler.init(item.Name, context.Term.Find(item.Accession))))

let transferTest =
    testI
    |> Array.map (fun item -> context.Add item)

transferTest.Length


let testTerm =
    let termInit = TermHandler.init("I")
    let termName = TermHandler.addName termInit "Test"
    TermHandler.addToContext context termInit


let convertPerson (dbContext:MzIdentMLContext) (xmlType:SchemePeptideShaker.Person) (organizationID:int) =
    let init = PersonHandler.init()
    let addFirstName = PersonHandler.addFirstName init xmlType.FirstName.Value
    let addLastname = PersonHandler.addLastName addFirstName xmlType.LastName.Value
    let addOrganization = PersonHandler.addOrganization addLastname (context.Organization.Find(organizationID))
    addOrganization

let persons =
    xmlCVParams.AuditCollection.Value.Persons
    |> Array.map (fun person -> convertPerson context person -2147482644)
    |> Array.map (fun item -> context.Add item)

persons
persons.[0]

module SubFunctions =
    //ObjectHandlers

    let convertOptionToList (optionOfType : seq<'a> option) =
        match optionOfType with
        | Some x -> x |> List
        | None -> null