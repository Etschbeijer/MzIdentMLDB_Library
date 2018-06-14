    
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

let fileDir = __SOURCE_DIRECTORY__
let standardDBPathSQLite = fileDir + "\Databases\Test.db"

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

initStandardDB sqliteContext

type TestType =
    | Software
    | TuKL
    | CSB
    | BioTech
    | SoftwareVendor
    | David
    | Timo
    | Michael
    static member toString (x:TestType) =
        match x with
        | Software  -> "MS:1000531"
        | TuKL -> "TechnischeUniversität KaisersLautern"
        | CSB -> "Computational Systems Biology"
        | BioTech -> "BioTech"
        | SoftwareVendor -> "MS:1001267"
        | David -> "2"
        | Timo -> "1"
        | Michael -> "3"

TestType.toString Software

let organizations =
    [
     OrganizationHandler.init(TestType.toString TuKL);
     OrganizationHandler.init(TestType.toString CSB)
        |> OrganizationHandler.addParent(TestType.toString TuKL);
    OrganizationHandler.init(TestType.toString BioTech)
        |> OrganizationHandler.addParent(TestType.toString TuKL)
    ]
    |> List.map (fun organization -> ContextHandler.addToContext sqliteContext organization)


let persons =
    [
     PersonHandler.init("1")
     |> PersonHandler.addFirstName "Timo"
     |> PersonHandler.addLastName "Mühlhaus"
     |> PersonHandler.addOrganization 
        (OrganizationHandler.tryFindByID 
            sqliteContext 
            (TestType.toString CSB)
        );
     PersonHandler.init("2")
     |> PersonHandler.addFirstName "David"
     |> PersonHandler.addLastName "Zimmer"
     |> PersonHandler.addOrganization 
            (OrganizationHandler.tryFindByID 
                sqliteContext 
                (TestType.toString CSB)
            );
     PersonHandler.init("3")
     |> PersonHandler.addFirstName "Michael"
     |> PersonHandler.addLastName "Schroda"
     |> PersonHandler.addOrganization 
            (OrganizationHandler.tryFindByID 
                sqliteContext 
                (TestType.toString BioTech)
            )
    ]
    |> List.map (fun person -> ContextHandler.addToContext sqliteContext person)

let analysisSoftware =
    AnalysisSoftwareHandler.init(
                                 CVParamHandler.init(
                                                     "MzIdentMLTest", 
                                                     TermHandler.tryFindByID 
                                                        sqliteContext 
                                                        (TestType.toString Software)
                                                    )
                                )
    |> AnalysisSoftwareHandler.addName "MzIdentMLDatanBank"
    |> AnalysisSoftwareHandler.addVersion "0.8"
    |> AnalysisSoftwareHandler.addAnalysisSoftwareDeveloper (ContactRoleHandler.init(
                                                                                     PersonHandler.tryFindByID 
                                                                                        sqliteContext 
                                                                                        (TestType.toString David),
                                                                                     CVParamHandler.init
                                                                                        ("MasterStudent", 
                                                                                         TermHandler.tryFindByID 
                                                                                            sqliteContext 
                                                                                            (TestType.toString SoftwareVendor)
                                                                                        )
                                                                                    )
                                                            )
    |> ContextHandler.addToContext sqliteContext

sqliteContext.SaveChanges()