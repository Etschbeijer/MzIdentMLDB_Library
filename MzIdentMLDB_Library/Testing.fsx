////Apply functions
        
//#r "System.ComponentModel.DataAnnotations.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\netstandard.dll"
//#r @"C:\Users\PatrickB\Source\Repos\MzIdentML_Library\MzIdentML_Library\bin\Debug\netstandard2.0\BioFSharp.dll"
//#r @"C:\Users\PatrickB\Source\Repos\MzIdentML_Library\MzIdentML_Library\bin\Debug\netstandard2.0\BioFSharp.IO.dll"
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

open MzIdentMLDataBase.DataContext.DataContext
open MzIdentMLDataBase.InsertStatements.ObjectHandlers
open MzIdentMLDataBase.InsertStatements.ManipulateDataContextAndDB
open MzIdentMLDataBase.InsertStatements.InitializeStandardDB
open MzIdentMLDataBase.XMLParsing

let context = configureSQLiteContextMzIdentML standardDBPathSQLite

//Term and Ontology
let termI = TermHandler.init("I")
let termII = TermHandler.addName termI "Test"
let ontologyI = OntologyHandler.init("I")
let termIII = TermHandler.addOntology termI ontologyI
let ontologyII = OntologyHandler.addTerm ontologyI termI
//let addOntologyToContext = OntologyHandler.addToContext context (OntologyHandler.addTerm ontologyI termI)
let addTermToContext = TermHandler.addToContext context termI

let cvParam = CVParamHandler.init("Test", termI)
let addCVtoContext = CVParamHandler.addToContext context cvParam

let analysisSoftware = AnalysisSoftwareHandler.init(cvParam, 0)
let analysisSoftwareName = AnalysisSoftwareHandler.addName analysisSoftware "BioFsharp.MZ"
let analysisSoftwareURI = AnalysisSoftwareHandler.addURI analysisSoftwareName "www.TEST.de"
let analysisSoftwareVersion = AnalysisSoftwareHandler.addVersion analysisSoftwareURI "V 1.00"
let analyisisSofwareDeveloper = AnalysisSoftwareHandler.addAnalysisSoftwareDeveloper analysisSoftwareVersion (ContactRoleHandler.init(PersonHandler.init(0, "David"),(CVParamHandler.init("Testi",termI))))
let addAnalysisSoftwareToContext = AnalysisSoftwareHandler.addToContext context analyisisSofwareDeveloper

let person = PersonHandler.init(0)
let addpersonToContext = PersonHandler.addToContext context person

let organization = OrganizationHandler.init(0)
let addOrganizationToContext = OrganizationHandler.addToContext context organization

context.Database.ProviderName
context.Database.EnsureCreated()

insertWithExceptionCheck context

initStandardDB context

type Test =
    {
        ID : int
        Name : string
        Detail : string
    }

type TestHandler =
        static member init
            (
                id        : int,
                ?name     : string,
                ?detail   : string
            ) =
            let name' :string= defaultArg name null
            let detail'    = defaultArg detail null
            {
                ID         = id;
                Name       = name';
                Detail     = detail'
            }
    

let transformOption (item : 'a option) =
    match item with
    |Some x -> x
    |None -> Unchecked.defaultof<'a>

TestHandler.init(0,transformOption(Some("test")) ,"string")
TestHandler.init(0)
TestHandler.init(0,transformOption(None) ,"string")

//XMLParser

let xmlCVParams = SchemePeptideShaker.Load("..\MzIdentMLDB_Library\XML_Files\PeptideShaker_mzid_1_2_example.mzid")

let xmlTest = xmlCVParams.AnalysisProtocolCollection.ProteinDetectionProtocol.Value.Threshold.CvParams

let test =
    xmlTest
    |> Array.map (fun item -> convertToEntity_CVParam context item)
test.Length
test
let testTerm =
    xmlTest
    |> Array.map (fun item -> takeTermEntry context item.Accession)

let testi (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam) =
    let init = CVParamHandler.init(mzIdentMLXML.Name, (takeTermEntry dbContext mzIdentMLXML.Accession))
    init

let testii =
    xmlTest
    |> Array.map (fun item -> testi context item)

let testiii =
    xmlTest
    |> Array.map (fun item -> CVParamHandler.addValue testii.[0] item.Value.Value)