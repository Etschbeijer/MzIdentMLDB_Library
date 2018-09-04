    
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
                        select (cv.Term.ID(*,cv.Term.Name*))
                        distinct
          }
    |> Seq.iter (fun termID -> dict.Add(termID,""))
    dict

let findAllSearchDatabaseTerms (dbContext:MzQuantML) (id:string) (dict:System.Collections.Generic.Dictionary<string,string>) =    

    query {
           for mzQdoc in dbContext.MzQuantMLDocument
                            .Include(fun item -> item.ProteinList)
                            .ThenInclude(fun item -> item.Proteins :> IEnumerable<_>)
                            .ThenInclude(fun (item:Protein) -> item.SearchDatabase.Details :> IEnumerable<_>) 
                            .ThenInclude(fun (item:SearchDatabaseParam) -> item.Term)
                            .Where(fun x -> x.ID=id)
                            .ToList()
                            do
                for prot in mzQdoc.ProteinList.Proteins do                    
                    for cv in prot.SearchDatabase.Details do 
                        select (cv.Term.ID(*,cv.Term.Name*))
                        distinct
          }
    |> Seq.iter (fun termID -> dict.Add(termID,""))
    dict

let findAllDBSequenceTerms (dbContext:MzIdentML) (id:string) (dict:System.Collections.Generic.Dictionary<string,string>) =    

    query {
           for mzIdent in dbContext.MzIdentMLDocument
                            .Include(fun item -> item.DBSequences :> IEnumerable<_>) 
                            .ThenInclude(fun (item:DBSequence) -> item.Details :> IEnumerable<_>) 
                            .ThenInclude(fun (item:DBSequenceParam) -> item.Term)
                            .Where(fun x -> x.ID=id)
                            .ToList()
                            do
                for dbSeq in mzIdent.DBSequences do  
                    for cv in dbSeq.Details do  
                    select (cv.Term.ID(*,cv.Term.Name*))
                    distinct
          }
    |> Seq.iter (fun termID -> dict.Add(termID,""))
    dict

let testFindAllProteinTerms =
    findAllProteinTerms sqliteMzQuantMLContext "Test1" (Dictionary())
    
let testFindAllSearchDBTerms =
    findAllSearchDatabaseTerms sqliteMzQuantMLContext "Test1" testFindAllProteinTerms

let testFindAllDBSeqTerms =
    findAllDBSequenceTerms sqliteMzIdentMLContext "Test1" testFindAllSearchDBTerms

//let standardCSVPath = fileDir + "\Databases\TSVTest1.csv"

//let test =
//    testFindAllDBSeqTerms
//    |> Seq.toCSV "\t" true
//    |> Seq.write standardCSVPath

let listOfColumnNames =
    [
     "accession", "MS:1000885"
     "description", "MS:1001088"
     "taxid", "MS:1001467"
     "species", "MS:1001469"
     "database", "MS:1001013"
     "database_version", "MS:1001016"
     "search_engine", "MS:1002337"
     "best_search_engine_score[1]", "User:0000079"
     "search_engine_score[1-n]_ms_run[1]", "MS:1002338"
     //"reliability", ""
     //"num_psms_ms_run[1-n]", ""
     "num_peptides_distinct_ms_run[1]", "MS:1001097"
     "num_peptides_unique_ms_run[1]", "MS:1001897"
     "ambiguity_members", "PRIDE:0000418"
     "modifications", "MS:1000933"
     //"uri", ""
     "go_terms", "MS:1000934"
     "protein_coverage", "MS:1001093"
     //"protein_abundance_assay[1-n]", ""
     //"protein_abundance_study_variable[1-n]", ""
     //"protein_abundance_stdev_study_variable[1-n]", ""
     //"protein_abundance_std_error_study_variable [1-n]", ""
     "opt_{identifier}_*", ""
    ]

let createDictionaryofColumNames (collectionOfCollumnNames:seq<string*string>) =
    let startDictionary = Dictionary()
    collectionOfCollumnNames
    |> Seq.iter (fun (columnName, termID) -> startDictionary.Add(termID, columnName)) |> ignore
    startDictionary

type TryTest =
    | Accession of string
    | Description
        static member toName (item:TryTest) =
            match item with
            | Accession s -> s
            | Description -> "TestI"

let Accession = TryTest.Accession "Test"
let y = TryTest.Accession "TestII"
TryTest.Accession "Test"
TryTest.toName Description
TryTest.toName y
TryTest.toName Accession
let x = testFindAllDBSeqTerms.Keys


let dictionaryOfColumnAndIDs = createDictionaryofColumNames listOfColumnNames

let testMatch =
    dictionaryOfColumnAndIDs.Values
    |> Seq.map (fun item -> testFindAllDBSeqTerms.Item item)

let completeDictionary (collectionOfColumnNames:Dictionary<string, string>) (collectionOfTerms:Dictionary<string, string>) =
    let tmp1 = Dictionary()
    let tmp2 = Dictionary()
    collectionOfTerms.Keys
    |> Seq.iter (fun item ->
        try
            tmp1.Add(collectionOfColumnNames.Item item, item)
        with
            :? System.Collections.Generic.KeyNotFoundException ->
            tmp2.Add("opt_{identifier}_" + (tmp2.Count.ToString()), item)
                )
    tmp2 
    |> Seq.iter (fun item -> tmp1.Add(item.Key, item.Value)) |> ignore
    tmp1

let finalTest =
    completeDictionary dictionaryOfColumnAndIDs testFindAllDBSeqTerms

let tmp = new Dictionary<string,string>(finalTest)
let test = tmp.Item "description" <- "Test"
tmp
finalTest