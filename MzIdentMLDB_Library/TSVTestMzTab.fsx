    
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


//let findProteinParams (dbContext:MzQuantML) =
//    query {
//            for i in dbContext.ProteinParam do
//                select i
//            }
//    |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
//                            then None
//                            else Some proteinList
//        )
//    |> (fun item -> 
//        match item with
//        | Some x ->  Some (x.ToList())
//        | None ->  None
//       )

//let findSearchDataBaseParams (dbContext:MzQuantML) =
//    query {
//            for i in dbContext.SearchDatabaseParam do
//                select i
//            }
//    |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
//                            then None
//                            else Some proteinList
//        )
//    |> (fun item -> 
//        match item with
//        | Some x ->  Some (x.ToList())
//        | None ->  None
//       )

//let findDatabaseName (dbContext:MzQuantML) =
//    query {
//           for i in dbContext.SearchDatabase do
//               select (i.DatabaseName, i.DatabaseName.Term)
//          }
//    |> Seq.map (fun (i, _) -> i)
//    |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
//                            then None
//                            else Some proteinList
//        )
    
//let findSearchDataBase (dbContext:MzQuantML) =
//    query {
//            for i in dbContext.SearchDatabase do
//                select i
//            }
//    |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
//                            then None
//                            else Some proteinList
//        )

let findMatchingTermIDCVParamBase (id:string) (paramCollection:seq<CVParamBase>) =
    paramCollection
    |> Seq.map (fun param -> 
        if id=param.Term.ID then
            param
        else null
               )
    |> Seq.filter (fun param -> param <> null)

let checkForNullOfCVParamBase (paramCollection:seq<CVParamBase>) =
    if Seq.length paramCollection < 1 then
        CVParamHandler.init(TermHandler.init(""), value="Null") :> CVParamBase
    else
        Seq.item 0 paramCollection

let findProteinParamTerms (dbContext:MzQuantML) =
    query {
            for i in dbContext.ProteinParam do
                select i.Term
            }
    |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
                            then None
                            else Some proteinList
        )
    |> (fun item -> 
        match item with
        | Some x ->  Some (x.ToList())
        | None ->  None
       )

let findSearchDataBaseParamTerms (dbContext:MzQuantML) =
    query {
            for i in dbContext.SearchDatabaseParam do
                select i.Term
            }
    |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
                            then None
                            else Some proteinList
        )
    |> (fun item -> 
        match item with
        | Some x ->  Some (x.ToList())
        | None ->  None
       )

let findProteins (dbContext:MzQuantML) =
    query {
            for i in dbContext.Protein do
                select i
            }
    |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
                            then None
                            else Some proteinList
        )
    |> (fun item -> 
        match item with
        | Some x ->  Some (x.ToArray())
        | None ->  None
       )

let findProteinList (dbContext:MzQuantML) (id:string) =
    findProteins dbContext |> ignore
    query {
            for i in dbContext.MzQuantMLDocument do
                if i.ID=id
                    then select i.ProteinList
            }
    |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
                            then None
                            else Some (proteinList.Single())
        )
    |> (fun item -> 
        match item with
        | Some x ->  Some (x.ToArray())
        | None ->  None
       )

let findCompleteProteins (dbContext:MzQuantML) (id:string) =
    query {
            for i in dbContext.Protein do
                if i.ID=id
                    then select (i, i.Details, i.SearchDatabase, i.SearchDatabase.DatabaseName, i.SearchDatabase.DatabaseName.Term, i.SearchDatabase.Details)
            }
    //|> Array.ofSeq
    |> Seq.map (fun (protein, proteinParams, search, _, _, searchParams) -> (protein, proteinParams, search, searchParams))
    //|> (fun proteinList -> if (Array.isEmpty proteinList) = true
    //                        then None
    //                        else Some (proteinList)
    //   )

let findDBSequenceParamTerms (dbContext:MzIdentML) =
    query {
            for i in dbContext.DBSequenceParam do
                select i.Term
            }
    |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
                            then None
                            else Some proteinList
        )
    |> (fun item -> 
        match item with
        | Some x ->  Some (x.ToList())
        | None ->  None
       )

let findDBSequenceParams (dbContext:MzIdentML) =
    query {
            for i in dbContext.DBSequenceParam do
                select i
            }
    |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
                            then None
                            else Some proteinList
        )
    |> (fun item -> 
        match item with
        | Some x ->  Some (x.ToList())
        | None ->  None
       )

let findDBSequences (dbContext:MzIdentML) (id:string) =
    query {
            for i in dbContext.MzIdentMLDocument do
                if i.ID=id
                    then select i.DBSequences
            }
    |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
                            then None
                            else Some proteinList
        )
    |> (fun item -> 
        match item with
        | Some x ->  x |> Seq.collect (fun item1 -> item1)
        | None ->  [] |> seq
       )
    |> (fun item -> item.ToArray())


let matchDBSequenceAccessionWithProteinAccesion (dbSequences:seq<DBSequence>) (proteinComplete:(Protein*List<ProteinParam>*SearchDatabase*List<SearchDatabaseParam>)) =
    let protein =
        proteinComplete
        |> (fun (protein, _, _, _) -> protein)

    let tmp =
        dbSequences
        |> Seq.map (fun dbSequence ->
            match dbSequence.Accession=protein.Accession with
            | true -> proteinComplete, dbSequence.Details
            | false -> proteinComplete, null
                    )
    Seq.filter (fun (_, i) -> i <> null) tmp

    //let rec loop collection (n:int) =
    //    if n = proteinComplete.Length then
    //           (List.tail collection).ToArray()
    //    else
    //        if dbSequence.Accession <> proteins.[n].Accession then
    //           loop collection (n+1)
    //        else
    //            loop (List.append collection [(proteinComplete.[n], dbSequence.Details)]) (n+1)
    //loop [((null, null, null, null), [] |> List)] 0

let createDictionaryOfConverterList (collection:(int*Converter)list) =
    let tmp = 
        List.sortBy (fun (key, _) -> key) collection
        |> List.map (fun (_, value) -> value)
        |> List.map (fun value -> value.ColumnName, value.ColumnValue)
    dict tmp

let createProteinSectionDictionary 
        (protein:Protein) (proteinParams:seq<ProteinParam>) (searchDatabase:SearchDatabase) (searchDatabaseParams:seq<SearchDatabaseParam>) (dbSequenceParams:seq<DBSequenceParam>)
        (searchEngine:string) (proteinAmbiguityGroup:string) (modifications:string) (terms:Term[]) =

        let cvParamBases =
            let proteinParams =
                proteinParams
                |> Seq.map (fun item -> item :> CVParamBase)
            let searchDatabaseParams =
                searchDatabaseParams
                |> Seq.map (fun item -> item :> CVParamBase)
            let dbSequenceParams =
                dbSequenceParams
                |> Seq.map (fun item -> item :> CVParamBase)
            let cvParams =
                searchDatabase.DatabaseName

            Seq.append (Seq.append (Seq.append proteinParams searchDatabaseParams) dbSequenceParams) [cvParams]

        let getMatchingCVParamBase (id:string) (paramCollection:seq<CVParamBase>) =
            (checkForNullOfCVParamBase (findMatchingTermIDCVParamBase id paramCollection))
        
        //let getProteinParam (id:string) (paramCollection:seq<ProteinParam>) =
        //    (checkForNullOfProteinParams (findMatchingTermIDProteinParams id paramCollection))

        //let getDBSequenceParam (id:string) (paramCollection:seq<DBSequenceParam>) =
        //    (checkForNullOfDBSequenceParams (findMatchingTermIDDBSequenceParams id paramCollection))

        //let getSearchDatabaseParam (id:string) (paramCollection:seq<SearchDatabaseParam>) =
        //    (checkForNullOfSearchDatabaseParams (findMatchingTermIDSearchDatabaseParam id paramCollection))

        let startDictionary =
            [
                1, createConverter "accession" protein.Accession false; 
                //5, createConverter "database" searchDatabase.DatabaseName.Value false;
                7, createConverter "search_engine" searchEngine false;
                14, createConverter "ambiguity_members" proteinAmbiguityGroup false;
                15, createConverter "modifications" modifications false;
                16, createConverter "uri" searchDatabase.Location true;
            ]
        let rec loop collection (n:int) (i:int) =
            if n = terms.Length then 
                //let tmp = List.sortBy (fun (key, _) -> key) collection
                //let tmpDictionary = dict tmp
                //tmpDictionary.Values
                collection |> (fun item -> createDictionaryOfConverterList item)

            else
                match terms.[n].ID with
                | "MS:1001088" -> loop (List.append collection [2, createConverter "description" (getMatchingCVParamBase terms.[n].ID cvParamBases).Value false]) (n+1) i
                | "MS:1001467" -> loop (List.append collection [3, createConverter "taxid" (getMatchingCVParamBase terms.[n].ID cvParamBases).Value false]) (n+1) i
                | "MS:1001469" -> loop (List.append collection [4, createConverter "species" (getMatchingCVParamBase terms.[n].ID cvParamBases).Value false]) (n+1) i
                | "MS:1001013" -> loop (List.append collection [5, createConverter "database" (getMatchingCVParamBase terms.[n].ID cvParamBases).Value false]) (n+1) i
                | "MS:1001016" -> loop (List.append collection [6, createConverter "database_version" (getMatchingCVParamBase terms.[n].ID cvParamBases).Value false]) (n+1) i
                | "MS:1002394" -> loop (List.append collection [8, createConverter "best_search_engine_score[1-n]" (getMatchingCVParamBase terms.[n].ID cvParamBases).Value false]) (n+1) i
                //search_engine_score[1-n]_ms_run[1-n]
                //reliability
                //num_psms_ms_run[1-n]
                //num_psms_ms_run[1-n]
                | "MS:1001898" -> loop (List.append collection [12, createConverter "num_peptides_distinct_ms_run[1-n]" (getMatchingCVParamBase terms.[n].ID cvParamBases).Value true]) (n+1) i
                | "MS:1001897" -> loop (List.append collection [13, createConverter "num_peptides_unique_ms_run[1-n]" (getMatchingCVParamBase terms.[n].ID cvParamBases).Value true]) (n+1) i
                //ambiguity_members
                | "MS:1000933" -> loop (List.append collection [15, createConverter "modifications" (getMatchingCVParamBase terms.[n].ID cvParamBases).Value true]) (n+1) i
                | "MS:1000934" -> loop (List.append collection [17, createConverter "go_terms" (getMatchingCVParamBase terms.[n].ID cvParamBases).Value true]) (n+1) i
                | "MS:1001093" -> loop (List.append collection [18, createConverter "protein_coverage" (getMatchingCVParamBase terms.[n].ID cvParamBases).Value true]) (n+1) i
                //protein_abundance_assay[1-n]
                //protein_abundance_study_variable[1-n]
                //protein_abundance_stdev_study_variable[1-n]
                //protein_abundance_std_error_study_variable [1-n]
                | _ -> loop (List.append collection  [n+1000, createConverter
                                                                ("option" + ([i].ToString()))
                                                                (convert4timesStringToSingleString
                                                                    (
                                                                    "",
                                                                    (getMatchingCVParamBase terms.[n].ID cvParamBases).Term.ID,
                                                                    (getMatchingCVParamBase terms.[n].ID cvParamBases).Term.Name,
                                                                    (getMatchingCVParamBase terms.[n].ID cvParamBases).Value
                                                                    )
                                                                )
                                                                true
                                                    ]
                            )
                                                            (n+1) (i+1)
                       
        loop startDictionary 0 1
createProteinSectionDictionary

let createProteinSection2 (path:string) (mzIdentMLContext:MzIdentML) (mzQuantMLContext:MzQuantML) (mzIdentMLDocumentID:string) (mzQuantMLDocumentID:string) =

    let terms =
        let proteinParamTerms = 
            match (findProteinParamTerms mzQuantMLContext) with
            | Some x -> x.ToArray()
            | None -> [||]
        let searchDatabaseParamTerms = 
            match (findSearchDataBaseParamTerms mzQuantMLContext) with
            | Some x -> x.ToArray()
            | None -> [||]
        let dbSequenceParamTerms =
            match (findDBSequenceParamTerms mzIdentMLContext) with
            | Some x -> x.ToArray()
            | None -> [||]
        
        Array.append (Array.append proteinParamTerms searchDatabaseParamTerms) dbSequenceParamTerms
        |> Array.distinct

    findDBSequenceParams mzIdentMLContext |> ignore

    let proteinIDList =
        findProteinList mzQuantMLContext mzQuantMLDocumentID
        |> (fun item -> 
            match item with
            | Some x -> x
                        |> Seq.collect (fun item1 -> item1.Proteins)
                        |> Seq.map (fun protein -> protein.ID)
            | None -> null
        )
        |> List.ofSeq
    
    let proteinsComplete =
        proteinIDList
        |> Seq.map (fun id -> findCompleteProteins mzQuantMLContext id)
        |> Seq.collect (fun item -> item)
        //|> Seq.collect (fun item -> match item with
        //                              | Some x -> x
        //                              | None-> [||]
        //                 )

    let dbSequences =
        findDBSequences mzIdentMLContext mzIdentMLDocumentID

    let rightDBSequenceParams =
        proteinsComplete
        |> Seq.collect (fun proteinComplete -> matchDBSequenceAccessionWithProteinAccesion dbSequences proteinComplete)
     
        
    rightDBSequenceParams, terms
    
#time
let x = createProteinSection2 "" sqliteMzIdentMLContext sqliteMzQuantMLContext "Test1" "Test1"
        //|> (fun (i, ii) -> i.ToArray(), ii)

let y =
    x
    |> (fun (item1, item2) -> item1
                              |> Seq.map(fun (proteinComplete, dbSequenceParams) -> proteinComplete
                                                                                      |> (fun (protein, proteinParams, searchDatabase, searchdatabaseparam) -> createProteinSectionDictionary protein proteinParams searchDatabase searchdatabaseparam dbSequenceParams "MaxQuant" "1 | 2 | 3" "Heavy labeling" item2)))
    |> Seq.collect (fun item -> item)
    |> Array.ofSeq



let testCSVFile =
    y
    |> (fun i -> Seq.toCSV ";" true i)
    |> Seq.write (standardTSVPath + "\TSV_TestFile_1.csv")
testCSVFile


//let findProtein (dbContext:MzQuantML) (id:string) =
//    query {
//           for i in dbContext.Protein do
//                if i.ID = id then
//                    select i
//          }
//    |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
//                            then None
//                            else Some proteinList
//        )

//let createAndAddProtein =
//    ProteinHandler.init("PatricksTest", null, "III")
//    |> ProteinHandler.addToContext sqliteMzQuantMLContext

//findProtein sqliteMzQuantMLContext "III"

//ProteinHandler.tryFindByID sqliteMzQuantMLContext "III"

//let proteinIDList =
//    findProteinList sqliteMzQuantMLContext "Test1"
//    |> (fun item -> 
//        match item with
//        | Some x -> x
//                    |> Seq.collect (fun item1 -> item1.Proteins)
//                    |> Seq.map (fun protein -> protein.ID)
//        | None -> null
//    )
//    |> List.ofSeq

//let testi = findProteinList sqliteMzQuantMLContext "Test1"
