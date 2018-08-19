    
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
#r @"..\MzIdentMLDB_Library\bin\Debug\FSharp.Care.IO.dll"

open System
open System.Data
open System.Linq
open System.Collections.Generic
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open Microsoft.EntityFrameworkCore
open BioFSharp
open BioFSharp.IO
open FSharp.Care.IO
open FSharp.Data

open MzIdentMLDataBase.DataModel
open MzIdentMLDataBase.InsertStatements.ObjectHandlers
open MzQuantMLDataBase.DataModel
open MzQuantMLDataBase.InsertStatements.ObjectHandlers
open TSVMzTabDataBase.DataModel
open TSVMzTabDataBase.InsertStatements.ObjectHandlers
open MzQuantMLDataBase.InsertStatements.ObjectHandlers



let fileDir = __SOURCE_DIRECTORY__
let standardTSVPath = fileDir + "\Databases"


let testString = "Test"
let testStringTimes4 = (testString, testString, testString, testString)
let testStringCollection = [testString]
let testStringTimes4Collection = [testStringTimes4]

let metaDataSectionMinimum =
    MetaDataSectionHandler.initBaseObject(
        testString, 0, 0, testString, testStringTimes4Collection, testStringTimes4Collection, testStringTimes4Collection, testStringTimes4Collection, testStringTimes4Collection, 
        testStringTimes4Collection, testString, testString
                                        )

let proteinSectionFull =
    ProteinSectionHandler.initBaseObject(
        testString, testString, -1, testString, testString, testString, testStringTimes4Collection, [-2.], testStringCollection, testStringCollection, 
        [-2.], -2, [-2], [-2], [-2], testString, testStringCollection, -1., [-1.; -2.], [-1.], [-1.], [-1.], testStringCollection
                                        )

let proteinSectionMinimum =
    ProteinSectionHandler.initBaseObject(
        testString, testString, -1, testString, testString, testString, 
        testStringTimes4Collection, [-2.], testStringCollection, testStringCollection
                                        )
    |> ProteinSectionHandler.addSearchEngineScoresMSRun -1.
    |> ProteinSectionHandler.addSearchEngineScoresMSRuns [-2.; -2.]
    |> ProteinSectionHandler.addSearchEngineScoresMSRun -1.

let testCSVFile =
    [proteinSectionMinimum]
    |> Seq.ofList 
    |> Seq.toCSV "," true
    |> Seq.write (standardTSVPath + "\TSV_TestFile_1.csv")

let testCSVCompleteSections =
    TSVFileHandler.init(metaDataSectionMinimum, [proteinSectionFull])
    |> TSVFileHandler.saveTSVFile (standardTSVPath + "\TSV_TestFile_1.csv")

type TestUnionCase =
        |Test1
        |Test2
        static member toFunction (item:TestUnionCase) =
            match item with
            |Test1 -> Test2
            |Test2 -> TestUnionCase.toFunction Test1
            
type Test =
    {
     ID         : TestUnionCase
     ToFunction : TestUnionCase
    }

let createTest id =
    {
     Test.ID         = id
     Test.ToFunction = TestUnionCase.toFunction id
    }

createTest Test1
createTest Test2

////////////////////////////////////////////////////
//New Test//////////
////////////////////////////////////////////////////

let standardDBPathSQLiteMzIdentML = fileDir + "\Databases\MzIdentML1.db"

let sqliteMzIdentMLContext = 
    MzIdentMLDataBase.InsertStatements.ObjectHandlers.ContextHandler.sqliteConnection standardDBPathSQLiteMzIdentML

let standardDBPathSQLiteMzQuantML = fileDir + "\Databases\MzQuantML1.db"

let sqliteMzQuantMLContext = 
    ContextHandler.sqliteConnection standardDBPathSQLiteMzQuantML


let findProteinsofProfteinList (dbContext:MzQuantML) (id:string) =
    query {
            for i in dbContext.ProteinList.Local do
                if i.ID=id
                    then select i.Proteins
            }
    |> (fun proteinList -> 
        if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
            then 
                query {
                        for i in dbContext.ProteinList do
                            if i.ID=id
                                then select i.Proteins
                        }
                |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
                                        then None
                                        else Some (proteinList |> seq)
                    )
            else Some (proteinList |> seq)
        )
    |> (fun item -> 
        match item with
        | Some x ->  x |> Seq.collect (fun item1 -> item1)
        | None ->  [] |> seq
       )

let createProteinSection (path:string) (mzIdentMLContext:MzIdentML) (mzQuantMLContext:MzQuantML) (mzIdentMLDocumentID:string) (mzQuantMLDocumentID:string) =
    
    let mzIdentML = MzIdentMLDocumentHandler.tryFindByID mzIdentMLContext mzIdentMLDocumentID
    let mzQuantML = MzQuantMLDocumentHandler.tryFindByID mzQuantMLContext mzQuantMLDocumentID

    let proteins = 
        match mzQuantML with
        | Some x -> x.ProteinList
                    |> Seq.collect (fun proteinListItem -> findProteinsofProfteinList mzQuantMLContext proteinListItem.ID)  
        | None -> [] |> seq

    let accessions =
        proteins 
        |> Seq.map (fun protein -> protein.Accession)
    accessions

    let dbSequence =
        

let test = 
    createProteinSection "Lalala" sqliteMzIdentMLContext sqliteMzQuantMLContext "Test1" "Test1"

