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
open MzIdentMLDataBase.InsertStatements.ObjectHandlers
open MzIdentMLDataBase.InsertStatements.ManipulateDataContextAndDB
open MzIdentMLDataBase.InsertStatements.InitializeStandardDB
//open MzIdentMLDataBase.XMLParsing

let context = configureSQLiteContextMzIdentML standardDBPathSQLite
//initStandardDB context

////Term and Ontology
//let termI = TermHandler.init("I")
//let termII = TermHandler.addName termI "Test"
//let ontologyI = OntologyHandler.init("I")
//let termIII = TermHandler.addOntology termI ontologyI
//let ontologyII = OntologyHandler.addTerm ontologyI termI
////let addOntologyToContext = OntologyHandler.addToContext context (OntologyHandler.addTerm ontologyI termI)
//let addTermToContext = TermHandler.addToContext context termI

//let cvParam = CVParamHandler.init("Test", termI)
//let addCVtoContext = CVParamHandler.addToContext context cvParam

//let analysisSoftware = AnalysisSoftwareHandler.init(cvParam, 0)
//let analysisSoftwareName = AnalysisSoftwareHandler.addName analysisSoftware "BioFsharp.MZ"
//let analysisSoftwareURI = AnalysisSoftwareHandler.addURI analysisSoftwareName "www.TEST.de"
//let analysisSoftwareVersion = AnalysisSoftwareHandler.addVersion analysisSoftwareURI "V 1.00"
//let analyisisSofwareDeveloper = AnalysisSoftwareHandler.addAnalysisSoftwareDeveloper analysisSoftwareVersion (ContactRoleHandler.init(PersonHandler.init(0, "David"),(CVParamHandler.init("Testi",termI))))
//let addAnalysisSoftwareToContext = AnalysisSoftwareHandler.addToContext context analyisisSofwareDeveloper

//let person = PersonHandler.init(0)
//let addpersonToContext = PersonHandler.addToContext context person

//let organization = OrganizationHandler.init(0)
//let addOrganizationToContext = OrganizationHandler.addToContext context organization

//context.Database.ProviderName
//context.Database.EnsureCreated()

//insertWithExceptionCheck context
//context.Database.EnsureCreated()

//let testII (dbContext:MzIdentMLContext) =
//    let terms_PsiMS =
//        fromPsiMS
//        |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name))
//        |> Seq.toArray
//    //terms_PsiMS|> Array.map (fun item -> TermHandler.addToContext dbContext item)
//    let psiMS = OntologyHandler.init("Psi-MS")
//    let add   = OntologyHandler.addTerms psiMS terms_PsiMS
//    add
//    //OntologyHandler.addToContext dbContext tmp

//let finish = testII context
//finish.Terms
//context.Add finish
//OntologyHandler.init("i")
//OntologyHandler.addToContext context (OntologyHandler.init("i"))
//TermHandler.init("i")
//TermHandler.addToContext context (TermHandler.init("iii","" ,(OntologyHandler.init("i"))))

//context.SaveChanges()


let termTest =
    let initTerm =
        OntologyHandler.init("Test",[TermHandler.init("I")])
    OntologyHandler.addToContext context initTerm

let cvParamTest =
    let initCVParam =
        CVParamHandler.init("", TermHandler.init(""))
    initCVParam

let organizationTest =
    let initOrg =
        OrganizationHandler.init(0,"Test")
    let orgWithDetail = 
        OrganizationHandler.addDetails initOrg [cvParamTest]
    OrganizationHandler.addToContext context orgWithDetail

let testPerson =
    let initPerson =
        PersonHandler.init()
    let personWithDetails = 
        PersonHandler.addDetail initPerson cvParamTest
    let personWithOrgan =
        PersonHandler.addOrganization initPerson (OrganizationHandler.init(0,"Test"))
    PersonHandler.addToContext context personWithOrgan

//type Test =
//    {
//        ID : int
//        Name : string
//        Detail : string
//    }

//type TestHandler =
//        static member init
//            (
//                id        : int,
//                ?name     : string,
//                ?detail   : string
//            ) =
//            let name' :string= defaultArg name null
//            let detail'    = defaultArg detail null
//            {
//                ID         = id;
//                Name       = name';
//                Detail     = detail'
//            }
    

//let transformOption (item : 'a option) =
//    match item with
//    |Some x -> x
//    |None -> Unchecked.defaultof<'a>

//TestHandler.init(0,transformOption(Some("test")) ,"string")
//TestHandler.init(0)
//TestHandler.init(0,transformOption(None) ,"string")

//XMLParser

//let xmlCVParams = SchemePeptideShaker.Load("..\MzIdentMLDB_Library\XML_Files\PeptideShaker_mzid_1_2_example.mzid")

//let xmlTest = xmlCVParams.AnalysisProtocolCollection.ProteinDetectionProtocol.Value.Threshold.CvParams

//let test =
//    xmlTest
//    |> Array.map (fun item -> convertToEntity_CVParam context item)
//test.Length
//test
//let testTerm =
//    xmlTest
//    |> Array.map (fun item -> takeTermEntry context item.Accession)

//let testi (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam) =
//    let init = CVParamHandler.init(mzIdentMLXML.Name, (takeTermEntry dbContext mzIdentMLXML.Accession))
//    init

//let testii =
//    xmlTest
//    |> Array.map (fun item -> testi context item)

//let testiii =
//    xmlTest
//    |> Array.map (fun item -> CVParamHandler.addValue testii.[0] item.Value.Value)


let personCollection = [PersonHandler.init(); PersonHandler.init(); PersonHandler.init(1)]
let termCollection = [TermHandler.init("i"); TermHandler.init("ii"); TermHandler.init("iii")]

let testContext =
    personCollection
    |> List.map (fun item -> context.Person.Add item)

let testContextI =
    termCollection
    |> List.map (fun item -> context.Term.Add item)


context.Person.Find(1)
context.Term.Find("i")

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