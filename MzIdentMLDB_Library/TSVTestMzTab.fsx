    
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
open TSVMzTabDataBase.DataModel
open TSVMzTabDataBase.InsertStatements.ObjectHandlers


let fileDir = __SOURCE_DIRECTORY__
let standardDBPathSQLiteMzQuantML = fileDir + "\Databases\MzQuantML1.db"


let testString = "Test"
let testStringTimes4 = (testString, testString, testString, testString)
let testStringCollection = [testString]
let testStringTimes4Collection = [testStringTimes4]

let metaDataSectionMinimum =
    MetaDataSectionHandler.init(
        testString, 0, 0, testString, testStringTimes4Collection, testStringTimes4Collection, testStringTimes4Collection, testStringTimes4Collection, testStringTimes4Collection, 
        testStringTimes4Collection, testString, testString
                               )

let proteinSectionFull =
    ProteinSectionHandler.init(
        testString, testString, -1, testString, testString, testString, testStringTimes4Collection, -1., testStringCollection, testStringCollection, 
        testStringTimes4Collection, -1, -1, -1, -1, testString, testStringCollection, -1., [-1.], [-1.], [-1.], [-1.], testStringCollection
                              )

let proteinSectionMinimum =
    ProteinSectionHandler.init(
        testString, testString, -1, testString, testString, testString, 
        testStringTimes4Collection, -1., testStringCollection, testStringCollection
                              )
    |> ProteinSectionHandler.addSearchEngineScoresMSRun testStringTimes4
    |> ProteinSectionHandler.addSearchEngineScoresMSRuns testStringTimes4Collection
    |> ProteinSectionHandler.addSearchEngineScoresMSRun testStringTimes4


