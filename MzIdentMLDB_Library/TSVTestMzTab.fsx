    
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

let standardDBPathSQLiteMzQuantML = fileDir + "\Databases\MzQuantML1.db"

let sqliteMzQuantMLContext = 
    ContextHandler.sqliteConnection standardDBPathSQLiteMzQuantML

let convert4timesStringToSingleString (item:string*string*string*string) =
                let tmp =
                    (fun (a,b,c,d) -> a + ", " + b + ", "+ c + ", " + d + ", ") item
                tmp

let findProteinsOfProteinList (dbContext:MzQuantML) (id:string) =
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

let findDBSequenceTerms (dbContext:MzIdentML) =
    query {
            for i in dbContext.DBSequenceParam.Local do
                select i.Term
            }
    |> Seq.toList
    |> (fun organization -> 
        if (List.exists (fun organization' -> organization' <> null) organization) = false
            then 
                query {
                        for i in dbContext.DBSequenceParam do
                            select i.Term
                        }
                |> Seq.toList
                |> (fun organization -> if (List.exists (fun organization' -> organization' <> null) organization) = false
                                        then None
                                        else Some (organization |> seq)
                    )
            else Some (organization |> seq)
        )

let findDetailsOfDBSequenceByAccession (dbContext:MzIdentML) (accession:string) =
    findDBSequenceTerms dbContext |> ignore
    query {
            for i in dbContext.DBSequence.Local do
                if i.Accession=accession
                    then select i.Details
            }
    |> (fun dbSequence -> 
        if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
            then 
                query {
                        for i in dbContext.DBSequence do
                            if i.Accession=accession
                                then select i.Details
                        }
                |> (fun dbSequence -> if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
                                        then None
                                        else Some (dbSequence |> seq)
                    )
            else Some (dbSequence |> seq)
        )
    |> (fun item -> 
        match item with
        | Some x ->  x |> Seq.collect (fun item1 -> item1)
        | None ->  [] |> seq
       )

let findTaxid (dbSequenceParam:DBSequenceParam) =
    match dbSequenceParam.Term.ID with
    | "MS:1001467" -> dbSequenceParam.Value
    | _ -> null

let findSpeciesName (dbSequenceParam:DBSequenceParam) =
    match dbSequenceParam.Term.ID with
    | "MS:1001467" -> dbSequenceParam.Value
    | _ -> null

let findSearchDatabaseNames (dbContext:MzIdentML) =
    findDBSequenceTerms dbContext |> ignore
    query {
            for i in dbContext.SearchDatabase.Local do
                select i.DatabaseName
            }
    |> (fun dbSequence -> 
        if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
            then 
                query {
                        for i in dbContext.SearchDatabase do
                            select i.DatabaseName
                        }
                |> (fun dbSequence -> if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
                                        then None
                                        else Some (dbSequence.Single())
                    )
            else Some (dbSequence.Single())
        )

let findCVParamsTerms (dbContext:MzIdentML) =
    query {
            for i in dbContext.CVParam.Local do
                select i.Term
            }
    |> Seq.toList
    |> (fun organization -> 
        if (List.exists (fun organization' -> organization' <> null) organization) = false
            then 
                query {
                        for i in dbContext.CVParam do
                            select i.Term
                        }
                |> Seq.toList
                |> (fun organization -> if (List.exists (fun organization' -> organization' <> null) organization) = false
                                        then None
                                        else Some (organization |> seq)
                    )
            else Some (organization |> seq)
        )

let findDataBaseNameByAccession (dbContext:MzIdentML) (accession:string) =
    findSearchDatabaseNames dbContext |> ignore
    findCVParamsTerms dbContext |> ignore
    
    query {
            for i in dbContext.DBSequence.Local do
                if i.Accession=accession
                    then select i.SearchDatabase
            }
    |> (fun dbSequence -> 
        if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
            then 
                query {
                        for i in dbContext.DBSequence do
                            if i.Accession=accession
                                then select i.SearchDatabase
                        }
                |> (fun dbSequence -> if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
                                                then None
                                                else Some (dbSequence.Single())
                    )
            else Some (dbSequence.Single())
        )
    |> (fun item ->
        match item with
        | Some x -> x.DatabaseName.Term.Name
        | None -> null
       )

let findSearchDatabaseParams (dbContext:MzIdentML) =
    query {
            for i in dbContext.SearchDatabase.Local do
                select i.Details
            }
    |> (fun dbSequence -> 
        if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
            then 
                query {
                        for i in dbContext.SearchDatabase do
                            select i.Details
                        }
                |> (fun dbSequence -> if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
                                        then None
                                        else Some (dbSequence |> seq)
                    )
            else Some (dbSequence |> seq)
        )

let findSearchDatabaseParamsTerms (dbContext:MzIdentML) =
    query {
            for i in dbContext.SearchDatabaseParam.Local do
                select i.Term
            }
    |> Seq.toList
    |> (fun organization -> 
        if (List.exists (fun organization' -> organization' <> null) organization) = false
            then 
                query {
                        for i in dbContext.SearchDatabaseParam do
                            select i.Term
                        }
                |> Seq.toList
                |> (fun organization -> if (List.exists (fun organization' -> organization' <> null) organization) = false
                                        then None
                                        else Some (organization |> seq)
                    )
            else Some (organization |> seq)
        )

let findDataBaseVersionByAccession (dbContext:MzIdentML) (accession:string) =
    findSearchDatabaseParams dbContext |> ignore
    findSearchDatabaseParamsTerms dbContext |> ignore
    
    query {
            for i in dbContext.DBSequence.Local do
                if i.Accession=accession
                    then select i.SearchDatabase
          }
    |> (fun dbSequence -> 
        if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
            then 
                query {
                        for i in dbContext.DBSequence do
                            if i.Accession=accession
                                then select i.SearchDatabase
                        }
                |> (fun dbSequence -> if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
                                                then None
                                                else Some (dbSequence.Single())
                    )
            else Some (dbSequence.Single())
        )
    |> (fun item -> 
        match item with
        | Some x ->  x.Details |> Seq.map (fun detail -> detail, detail.Term.ID)
        | None ->  null
       )

let findDatabaseVersion (searchDatabaseAndTermID:MzIdentMLDataBase.DataModel.SearchDatabaseParam*string) =
    searchDatabaseAndTermID
    |> (fun (detail, termID) ->
        match termID with
        | "MS:1001016" -> detail.Value
        | _ -> null
       )

let findAnalysisParamsTerms (dbContext:MzIdentML) =
    query {
            for i in dbContext.AnalysisParam.Local do
                select i.Term
            }
    |> Seq.toList
    |> (fun organization -> 
        if (List.exists (fun organization' -> organization' <> null) organization) = false
            then 
                query {
                        for i in dbContext.AnalysisParam do
                            select i.Term
                        }
                |> Seq.toList
                |> (fun organization -> if (List.exists (fun organization' -> organization' <> null) organization) = false
                                        then None
                                        else Some (organization |> seq)
                    )
            else Some (organization |> seq)
        )

let findAnalysisParamsOfProfteinList (dbContext:MzIdentML) (id:string) =
    findAnalysisParamsTerms dbContext |> ignore
    query {
            for i in dbContext.ProteinDetectionProtocol.Local do
                if i.ID=id
                    then select i.AnalysisParams
            }
    |> (fun proteinList -> 
        if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
            then 
                query {
                        for i in dbContext.ProteinDetectionProtocol do
                            if i.ID=id
                                then select i.AnalysisParams
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

let findSearchEgnines (analysisParams:seq<AnalysisParam>) =
    analysisParams
    |> Seq.map (fun analysisParam ->
        match analysisParam.Term.ID with
        | "MS:1001583" -> ("PSM", analysisParam.Term.ID, analysisParam.Term.Name, analysisParam.Value) |> (fun item -> convert4timesStringToSingleString item)
        | _ -> null
               )
let createTSVProteinSection (path:string) (mzIdentMLContext:MzIdentML) (mzQuantMLContext:MzQuantML) (mzIdentMLDocumentID:string) (mzQuantMLDocumentID:string) =
    
    let mzIdentML = MzIdentMLDocumentHandler.tryFindByID mzIdentMLContext mzIdentMLDocumentID
    let mzQuantML = MzQuantMLDocumentHandler.tryFindByID mzQuantMLContext mzQuantMLDocumentID

    let proteins = 
        match mzQuantML with
        | Some x -> x.ProteinList
                    |> Seq.collect (fun proteinListItem -> findProteinsOfProteinList mzQuantMLContext proteinListItem.ID)  
        | None -> [] |> seq

    let accessions =
        proteins 
        |> Seq.map (fun protein -> protein.Accession)

    let dbSequenceDetails =
        accessions
        |> Seq.collect 
            (fun accession -> findDetailsOfDBSequenceByAccession mzIdentMLContext accession)

    let taxids =
        dbSequenceDetails
        |> Seq.map (fun dbSequenceDetail -> findTaxid dbSequenceDetail)
        |> Seq.filter (fun species -> species<>null)

    let speciesNames =
        dbSequenceDetails
        |> Seq.map (fun dbSequenceDetail -> findSpeciesName dbSequenceDetail)
        |> Seq.filter (fun species -> species<>null)
    
    let searchDatabaseNames =
        accessions
        |> Seq.map
            (fun accession -> findDataBaseNameByAccession mzIdentMLContext accession)

    let searchDatabaseDetails =
        accessions
        |> Seq.collect
            (fun accession -> findDataBaseVersionByAccession mzIdentMLContext accession)

    let databaseVersions =
        searchDatabaseDetails
        |> Seq.map (fun databaseVersion -> findDatabaseVersion databaseVersion)
        |> Seq.filter (fun databaseVersion -> databaseVersion<>null)

    let searchEgnines =
        match mzIdentML with
        | Some x -> x.ProteinDetectionProtocol
                    |> Seq.map (fun item -> findAnalysisParamsOfProfteinList mzIdentMLContext item.ID)
        | None -> null
        |> Seq.map (fun item -> findSearchEgnines item)
        |> Seq.map (fun searchEngine1 -> searchEngine1 |> Seq.filter (fun searchEngine -> searchEngine<>null))
    searchEgnines
    

let test = 
    createTSVProteinSection "Unknown" sqliteMzIdentMLContext sqliteMzQuantMLContext "Test1" "Test1"
