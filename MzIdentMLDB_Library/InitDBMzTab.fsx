﻿    
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
open MzTabDataBase.DataModel
open MzTabDataBase.InsertStatements.ObjectHandlers
//open MzIdentMLDataBase.XMLParsing


let fileDir = __SOURCE_DIRECTORY__
let standardDBPathSQLiteMzIdentML = fileDir + "\Databases\MzTab1.db"

let sqliteMzIdentMLContext = ContextHandler.sqliteConnection standardDBPathSQLiteMzIdentML


let fromPsiMS =
    ContextHandler.fromFileObo (fileDir + "\Ontologies\Psi-MS.txt")

let fromPride =
    ContextHandler.fromFileObo (fileDir + "\Ontologies\Pride.txt")

let fromUniMod =
    ContextHandler.fromFileObo (fileDir + "\Ontologies\Unimod.txt")

let fromUnit_Ontology =
    ContextHandler.fromFileObo (fileDir + "\Ontologies\Unit_Ontology.txt")
        
 
let initStandardDB (dbContext : MzTab) =

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


let sqliteMzIdentMLDB =
    initStandardDB sqliteMzIdentMLContext

