    
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
open MzBasis.Basetypes

let fileDir = __SOURCE_DIRECTORY__
let standardTSVPath = fileDir + "\Databases"


let testString = "Test"
let testStringTimes4 = (testString, testString, testString, testString)
let testStringCollection = [testString]
let testStringTimes4Collection = [testStringTimes4]

//let metaDataSectionMinimum =
//    MetaDataSectionHandler.initBaseObject(
//        testString, 0, 0, testString, testStringTimes4Collection, testStringTimes4Collection, testStringTimes4Collection, testStringTimes4Collection, testStringTimes4Collection, 
//        testStringTimes4Collection, testString, testString
//                                        )

//let proteinSectionFull =
//    ProteinSectionHandler.initBaseObject(
//        testString, testString, -1, testString, testString, testString, testStringTimes4Collection, [-2.], testStringCollection, testStringCollection, 
//        [-2.], -2, [-2], [-2], [-2], testString, testStringCollection, -1., [-1.; -2.], [-1.], [-1.], [-1.], testStringCollection
//                                        )

//let proteinSectionMinimum =
//    ProteinSectionHandler.initBaseObject(
//        testString, testString, -1, testString, testString, testString, 
//        testStringTimes4Collection, [-2.], testStringCollection, testStringCollection
//                                        )
//    |> ProteinSectionHandler.addSearchEngineScoresMSRun -1.
//    |> ProteinSectionHandler.addSearchEngineScoresMSRuns [-2.; -2.]
//    |> ProteinSectionHandler.addSearchEngineScoresMSRun -1.

//let testCSVFile =
//    [proteinSectionMinimum]
//    |> Seq.ofList 
//    |> Seq.toCSV "," true
//    |> Seq.write (standardTSVPath + "\TSV_TestFile_1.csv")

//let testCSVCompleteSections =
//    TSVFileHandler.init(metaDataSectionMinimum, [proteinSectionFull])
//    |> TSVFileHandler.saveTSVFile (standardTSVPath + "\TSV_TestFile_1.csv")

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

let createProteinSection
    accession description taxid species database databaseVersion searchEngines bestSearchEngineScore
    searchEngineScoresMSRuns reliability numPSMsMSRuns numPeptidesDistinctMSRuns numPeptidesUniqueMSRuns
    ambiguityMembers modifications uri goTerms proteinCoverage proteinAbundanceAssays proteinAbundanceStudyVariables
    proteinAbundanceStandardDeviationStudyVariables proteinAbundanceStandardErrorStudyVariables 
    optionalInformation
    =
    {
     ProteinSection.Accession                                       = accession 
     ProteinSection.Description                                     = description 
     ProteinSection.Taxid                                           = taxid 
     ProteinSection.Species                                         = species 
     ProteinSection.Database                                        = database 
     ProteinSection.DatabaseVersion                                 = databaseVersion 
     ProteinSection.SearchEngines                                   = searchEngines 
     ProteinSection.BestSearchEngineScore                           = bestSearchEngineScore
     ProteinSection.SearchEngineScoresMSRuns                        = searchEngineScoresMSRuns 
     ProteinSection.Reliability                                     = reliability 
     ProteinSection.NumPSMsMSRuns                                   = numPSMsMSRuns 
     ProteinSection.NumPeptidesDistinctMSRuns                       = numPeptidesDistinctMSRuns 
     ProteinSection.NumPeptidesUniqueMSRuns                         = numPeptidesUniqueMSRuns
     ProteinSection.AmbiguityMembers                                = ambiguityMembers 
     ProteinSection.Modifications                                   = modifications 
     ProteinSection.URI                                             = uri 
     ProteinSection.GoTerms                                         = goTerms 
     ProteinSection.ProteinCoverage                                 = proteinCoverage 
     ProteinSection.ProteinAbundanceAssays                          = proteinAbundanceAssays 
     ProteinSection.ProteinAbundanceStudyVariables                  = proteinAbundanceStudyVariables
     ProteinSection.ProteinAbundanceStandardDeviationStudyVariables = proteinAbundanceStandardDeviationStudyVariables 
     ProteinSection.ProteinAbundanceStandardErrorStudyVariables     = proteinAbundanceStandardErrorStudyVariables 
     ProteinSection.OptionalInformations                            = optionalInformation
    }

type Converter =
    {
     ColumnName     : string
     ColumnValue    : string
     Optional       : bool
    }

let createConverter columnName columnValue optional =
    {
     Converter.ColumnName   = columnName
     Converter.ColumnValue  = columnValue
     Converter.Optional     = optional
    }

let createProteinSectionArray 
    accession description taxid species database databaseVersion searchEngines bestSearchEngineScore
    searchEngineScoresMSRuns reliability numPSMsMSRuns numPeptidesDistinctMSRuns numPeptidesUniqueMSRuns
    ambiguityMembers modifications uri goTerms proteinCoverage proteinAbundanceAssays proteinAbundanceStudyVariables
    proteinAbundanceStandardDeviationStudyVariables proteinAbundanceStandardErrorStudyVariables 
    optionalInformation
    =
    [|
      createConverter "accession"                                       accession                                       true;
      createConverter "description"                                     description                                     true;
      createConverter "taxid"                                           taxid                                           true;
      createConverter "species"                                         species                                         true;
      createConverter "database"                                        database                                        true;
      createConverter "database_version"                                databaseVersion                                 true;
      createConverter "search_engine"                                   searchEngines                                   true;
      createConverter "best_search_engine_score[1-n]"                   bestSearchEngineScore                           true;
      createConverter "search_engine_score[1-n]_ms_run[1-n]"            searchEngineScoresMSRuns                        false;
      createConverter "reliability"                                     reliability                                     false;
      createConverter "num_psms_ms_run[1-n]"                            numPSMsMSRuns                                   false;
      createConverter "num_peptides_distinct_ms_run[1-n]"               numPeptidesDistinctMSRuns                       false;
      createConverter "num_peptides_unique_ms_run[1-n]"                 numPeptidesUniqueMSRuns                         false;
      createConverter "ambiguity_members"                               ambiguityMembers                                true;
      createConverter "modifications"                                   modifications                                   true;
      createConverter "uri"                                             uri                                             false;
      createConverter "go_terms"                                        goTerms                                         false;
      createConverter "protein_coverage"                                proteinCoverage                                 false;
      createConverter "protein_abundance_assay[1-n]"                    proteinAbundanceAssays                          false;
      createConverter "protein_abundance_study_variable[1-n]"           proteinAbundanceStudyVariables                  false;
      createConverter "protein_abundance_stdev_study_variable[1-n]"     proteinAbundanceStandardDeviationStudyVariables false;
      createConverter "protein_abundance_std_error_study_variable[1-n]" proteinAbundanceStandardErrorStudyVariables     false;
      createConverter "opt_{identifier}_*"                              optionalInformation                             false;
    |]

let standardDBPathSQLiteMzIdentML = fileDir + "\Databases\MzIdentML1.db"

let sqliteMzIdentMLContext = 
    MzIdentMLDataBase.InsertStatements.ObjectHandlers.ContextHandler.sqliteConnection standardDBPathSQLiteMzIdentML
sqliteMzIdentMLContext.ChangeTracker.AutoDetectChangesEnabled = false


let standardDBPathSQLiteMzQuantML = fileDir + "\Databases\MzQuantML1.db"

let sqliteMzQuantMLContext = 
    ContextHandler.sqliteConnection standardDBPathSQLiteMzQuantML
sqliteMzQuantMLContext.ChangeTracker.AutoDetectChangesEnabled = false



let convert4timesStringToSingleString (item:string*string*string*string) =
                let a,b,c,d = item
                sprintf "%s| %s| %s| %s|" a b c d
                //let tmp =
                //    (fun (a,b,c,d) -> a + "| " + b + "| "+ c + "| " + d + "| ") item
                //tmp

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////


#time

let findAllProteinTerms (dbContext:MzQuantML) (id:string) (dict:System.Collections.Generic.Dictionary<string,string>) =    
    //dbContext.MzQuantMLDocument
    //    .Where(fun item -> item.ID=id)
    //    .Select(fun item -> item.ProteinList.Proteins)
    //    .Include(fun protein -> protein :> IEnumerable<_>)
    //    .ThenInclude(fun (item:Protein) -> item.ID) //.ThenInclude(fun item -> item)
    ////                                                                             |> Seq.collect (fun protein -> protein.Details 
    ////                                                                                                            |> Seq.map (fun proteinParam -> proteinParam.Term)))
    ////|> Seq.concat
    query {
            
            
            for mzQdoc in dbContext.MzQuantMLDocument
                            .Include(fun item -> item.ProteinList)
                            .ThenInclude(fun item -> item.Proteins :> IEnumerable<_>)
                            .ThenInclude(fun (item:Protein) -> item.Details :> IEnumerable<_>) 
                            .ThenInclude(fun (item:ProteinParam) -> item.Term)
                            .Where(fun x -> x.ID=id)
                            .ToList()
                            do
                for prot in mzQdoc.ProteinList.Proteins do                    
                    for cv in prot.Details do 
            //where (mzQdoc.ID=id)
                        select (cv.Term.ID,cv.Term.Name)
                        distinct
            }
    //|> Seq.iter (fun term -> dict.Add(term,""))

    //dict

let testFindAllProteinTerms =
    findAllProteinTerms sqliteMzQuantMLContext "Test1" (Dictionary())
    |> Seq.toArray
    
let test = testFindAllProteinTerms
