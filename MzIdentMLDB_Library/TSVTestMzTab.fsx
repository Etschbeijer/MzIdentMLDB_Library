    
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

let findAllProteinTerms (dbContext:MzQuantML) (mzQuantID:string) (dict:System.Collections.Generic.Dictionary<string,string>) =

    query {
           for mzQdoc in dbContext.MzQuantMLDocument
                            .Include(fun item -> item.ProteinList)
                            .ThenInclude(fun item -> item.Proteins :> IEnumerable<_>)
                            .ThenInclude(fun (item:Protein) -> item.Details :> IEnumerable<_>) 
                            .ThenInclude(fun (item:ProteinParam) -> item.Term)
                            .Where(fun x -> x.ID=mzQuantID)
                            .ToList()
                            do
                for prot in mzQdoc.ProteinList.Proteins do                    
                    for cv in prot.Details do 
                        select (cv.Term.ID(*,cv.Term.Name*))
                        distinct
          }
    |> Seq.iter (fun termID -> dict.Add(termID,""))
    dict

let findAllSearchDatabaseTerms (dbContext:MzQuantML) (mzQuantID:string) (dict:System.Collections.Generic.Dictionary<string,string>) =    

    query {
           for mzQdoc in dbContext.MzQuantMLDocument
                            .Include(fun item -> item.ProteinList)
                            .ThenInclude(fun item -> item.Proteins :> IEnumerable<_>)
                            .ThenInclude(fun (item:Protein) -> item.SearchDatabase.Details :> IEnumerable<_>) 
                            .ThenInclude(fun (item:SearchDatabaseParam) -> item.Term)
                            .Where(fun x -> x.ID=mzQuantID)
                            .ToList()
                            do
                for prot in mzQdoc.ProteinList.Proteins do                    
                    for cv in prot.SearchDatabase.Details do 
                        select (cv.Term.ID(*,cv.Term.Name*))
                        distinct
          }
    |> Seq.iter (fun termID -> dict.Add(termID,""))
    dict

let findAllDBSequenceTerms (dbContext:MzIdentML) (mzIdentID:string) (dict:System.Collections.Generic.Dictionary<string,string>) =    

    query {
           for mzIdent in dbContext.MzIdentMLDocument
                            .Include(fun item -> item.DBSequences :> IEnumerable<_>) 
                            .ThenInclude(fun (item:DBSequence) -> item.Details :> IEnumerable<_>) 
                            .ThenInclude(fun (item:DBSequenceParam) -> item.Term)
                            .Where(fun x -> x.ID=mzIdentID)
                            .ToList()
                            do
                for dbSeq in mzIdent.DBSequences do  
                    for cv in dbSeq.Details do  
                    select (cv.Term.ID(*,cv.Term.Name*))
                    distinct
          }
    |> Seq.iter (fun termID -> dict.Add(termID,""))
    dict

let allTerms =
    findAllProteinTerms sqliteMzQuantMLContext "Test1" (Dictionary())
    |> findAllSearchDatabaseTerms sqliteMzQuantMLContext "Test1"
    |> findAllDBSequenceTerms sqliteMzIdentMLContext "Test1"

//replace with Map!!!
let columnNames =
    [
     "MS:1000885", "accession"
     "MS:1001088", "description"
     "MS:1001467", "taxid"
     "MS:1001469", "species"
     "MS:1001013", "database"
     "MS:1001016", "database_version"
     "MS:1002337", "search_engine"
     "User:0000079", "best_search_engine_score[1]"
     "MS:1002338", "search_engine_score[1-n]_ms_run[1]"
     //"reliability"
     //"num_psms_ms_run[1-n]"
     "MS:1001097", "num_peptides_distinct_ms_run[1]"
     "MS:1001897", "num_peptides_unique_ms_run[1]"
     "PRIDE:0000418", "ambiguity_members"
     "MS:1000933", "modifications"
     //"uri"
     "MS:1000934", "go_terms"
     "MS:1001093", "protein_coverage"
     //"protein_abundance_assay[1-n]"
     //"protein_abundance_study_variable[1-n]"
     //"protein_abundance_stdev_study_variable[1-n]"
     //"protein_abundance_std_error_study_variable [1-n]"
     "", "opt_{identifier}_*"
    ]

let createDictionaryofColumnNames (columnNames:seq<string*string>) =
    let startDictionary = Dictionary()
    columnNames
    |> Seq.iter (fun (termID, columnName) -> startDictionary.Add(termID, columnName)) |> ignore
    startDictionary

let dictionaryOfColumnAndIDs = createDictionaryofColumnNames columnNames

let findAllProteins (dbContext:MzQuantML) (mzQuantID:string) =
    
    query {
           for mzQdoc in dbContext.MzQuantMLDocument
                            .Include(fun item -> item.ProteinList)
                            .ThenInclude(fun item -> item.Proteins :> IEnumerable<_>)
                            .ThenInclude(fun (item:Protein) -> item.Details :> IEnumerable<_>) 
                            .ThenInclude(fun (item:ProteinParam) -> item.Term)
                            .Include(fun item -> item.ProteinList)
                            .ThenInclude(fun item -> item.Proteins :> IEnumerable<_>)
                            .ThenInclude(fun (item:Protein) -> item.SearchDatabase.Details :> IEnumerable<_>) 
                            .ThenInclude(fun (item:SearchDatabaseParam) -> item.Term)
                            .Where(fun x -> x.ID=mzQuantID)
                            .ToList()
                            do
                for prot in mzQdoc.ProteinList.Proteins do                    
                    select prot    
          }

let findAllDBSequencesOfProteins (dbContext:MzIdentML) (mzIdentID:string) (protAccession:string)=    
    
    query {
           for mzIdent in dbContext.MzIdentMLDocument
                            .Include(fun item -> item.DBSequences :> IEnumerable<_>) 
                            .ThenInclude(fun (item:DBSequence) -> item.Details :> IEnumerable<_>)
                            .ThenInclude(fun (item:DBSequenceParam) -> item.Term)
                            .Where(fun x -> x.ID=mzIdentID)
                            .ToList()
                            do
                for dbSeq in mzIdent.DBSequences do
                    where (mzIdent.ID=mzIdentID && dbSeq.Accession=protAccession)
                    select dbSeq
          }
    |> Array.ofSeq

let getProteinsAndDBSeqs
    (mzIdentContext:MzIdentML) (mzIdentID:string) (mzQuantContext:MzQuantML) (mzQuantID:string) =

    findAllProteins mzQuantContext mzQuantID
    |> Seq.map (fun protein ->
        protein, findAllDBSequencesOfProteins mzIdentContext mzIdentID protein.Accession
               )
    |> Array.ofSeq

let proteinsAndDBSeqs =
    getProteinsAndDBSeqs sqliteMzIdentMLContext "Test1" sqliteMzQuantMLContext "Test1"

let findValuesOfProteins
    (columnNames:Dictionary<string, string>) (terms:Dictionary<string,string>) ((protein, dbSequences):Protein*(DBSequence[])) =
    
    let tmp = new Dictionary<string, string>(terms)
        

    let CvParamsBase =
        [protein.Details |> Seq.map (fun protParam -> protParam :> CVParamBase); 
         protein.SearchDatabase.Details |> Seq.map (fun searchDBParam -> searchDBParam :> CVParamBase); 
         dbSequences 
         |> Seq.collect (fun dbSequence -> dbSequence.Details 
                                           |> Seq.map (fun dbSeqParam -> dbSeqParam :> CVParamBase))
        ]
        |> Seq.concat
        |> Array.ofSeq

    let valuesAndTermIDs =
        CvParamsBase
        |> Array.iter (fun cvParam -> 
            tmp.Item cvParam.Term.ID <- convert4timesStringToSingleString("", cvParam.Term.ID, cvParam.Term.Name, cvParam.Value))
        tmp

    let addColumnNamesOfProteinSection (terms:Dictionary<string, string>) =
        let tmp1 = 
            match terms.Keys.Contains "MS:1000885" with
            | true -> 
                let tmp1 =
                    let tmp2=
                        Dictionary()
                    tmp2.Add("PRH", "PRT")
                    terms
                    |> Seq.iter (fun item -> tmp2.Add(item.Key, item.Value))
                    tmp2
                tmp1
            | false -> 
                let tmp1 =
                    let tmp2=
                        Dictionary()
                    tmp2.Add("PRH", "PRT")
                    tmp2.Add("accession", protein.Accession)
                    tmp2.Add("uri", protein.SearchDatabase.Location)
                    terms
                    |> Seq.iter (fun item -> tmp2.Add(item.Key, item.Value))
                    tmp2
                tmp1
        let tmp2 = Dictionary()
        terms.Keys
        |> Seq.iter (fun item ->
            try
                tmp1.Add(columnNames.Item item, terms.Item item)
            with
                :? System.Collections.Generic.KeyNotFoundException ->
                tmp2.Add("opt_{identifier}_" + (tmp2.Count.ToString()), terms.Item item)
                    )
        tmp2 
        |> Seq.iter (fun item -> tmp1.Add(item.Key, item.Value)) |> ignore
        tmp1 
    addColumnNamesOfProteinSection tmp

let valuesAndTermIDs =
    proteinsAndDBSeqs
    |> Array.map (fun item -> findValuesOfProteins dictionaryOfColumnAndIDs allTerms item)

//let addColumnNames (columnNames:Dictionary<string, string>) (terms:Dictionary<string, string>) =
//    let tmp1 = Dictionary()
//    let tmp2 = Dictionary()
//    terms.Keys
//    |> Seq.iter (fun item ->
//        try
//            tmp1.Add(columnNames.Item item, terms.Item item)
//        with
//            :? System.Collections.Generic.KeyNotFoundException ->
//            tmp2.Add("opt_{identifier}_" + (tmp2.Count.ToString()), terms.Item item)
//                )
//    tmp2 
//    |> Seq.iter (fun item -> tmp1.Add(item.Key, item.Value)) |> ignore
//    tmp1

//let columnNamedDictionary = 
//    valuesAndTermIDs
//    |> Array.map (fun item -> addColumnNames dictionaryOfColumnAndIDs item)

let standardCSVPath = fileDir + "\Databases\TSVTest1.tab"

let finalDictionary (path:string) (termsAndValues:Dictionary<string, string>[]) =
    let columnNames = termsAndValues.[0].Keys |> Array.ofSeq
    let tmp = 
        termsAndValues |> Array.map (fun item -> item.ToArray())
        |> Array.map (fun outer -> outer |> Array.map (fun inner -> inner.Value))
    [|yield columnNames; yield! tmp|]
    |> Array.map (fun item -> item |> String.concat "\t")
    |> Seq.write path

let finalStep =
    finalDictionary standardCSVPath valuesAndTermIDs
