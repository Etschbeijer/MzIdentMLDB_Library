    
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
open MzQuantMLDataBase.InsertStatements.ObjectHandlers
open MzBasis.Basetypes

let fileDir = __SOURCE_DIRECTORY__
let standardTSVPath = fileDir + "\Databases"


let testString = "Test"
let testStringTimes4 = (testString, testString, testString, testString)
let testStringCollection = [testString]
let testStringTimes4Collection = [testStringTimes4]


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
                sprintf "[%s ,%s ,%s ,%s ]" a b c d


let getAllProteinTerms (dbContext:MzQuantML) (mzQuantID:string) =

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
    //|> Seq.iter (fun termID -> dict.Add(termID,""))
    //dict

let getAllSearchDatabaseTerms (dbContext:MzQuantML) (mzQuantID:string) =    

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
    //|> Seq.iter (fun termID -> dict.Add(termID,""))
    //dict

let getAllDBSequenceTerms (dbContext:MzIdentML) (mzIdentID:string) =    

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
    //|> Seq.iter (fun termID -> dict.Add(termID,""))
    //dict

let getAllProteinSectionTerms 
    (mzQuantContext:MzQuantML) (mzQuantID:string) 
    (mzIDentContext:MzIdentML) (mzIdentID:string) 
    (dict:System.Collections.Generic.Dictionary<string,string>) =    
    [
     getAllProteinTerms mzQuantContext mzQuantID
     getAllSearchDatabaseTerms mzQuantContext mzQuantID
     getAllDBSequenceTerms mzIDentContext mzIdentID
    ]
    |> Seq.concat
    |> Seq.distinct
    |> Seq.iter (fun termID -> dict.Add(termID,""))
    dict

//let allTerms =
//    getAllProteinSectionTerms sqliteMzQuantMLContext "Test1" sqliteMzIdentMLContext "Test1" (Dictionary())

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

//let dictionaryOfColumnAndIDs = 
//    createDictionaryofColumnNames columnNames

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
    (mzQuantContext:MzQuantML) (mzQuantID:string) (mzIdentContext:MzIdentML) (mzIdentID:string) =

    findAllProteins mzQuantContext mzQuantID
    |> Seq.map (fun protein ->
        protein, findAllDBSequencesOfProteins mzIdentContext mzIdentID protein.Accession
               )
    |> Array.ofSeq

//let proteinsAndDBSeqs =
//    getProteinsAndDBSeqs sqliteMzQuantMLContext "Test1" sqliteMzIdentMLContext "Test1"

let addColumnNamesOfProteinSection (prot:Protein) (columnNames:Dictionary<string, string>) (values:Dictionary<string, string>) =
        let tmp1 = 
            match values.Keys.Contains "MS:1000885" with
            | true -> 
                let tmp =
                    let tmp2=
                        Dictionary()
                    tmp2.Add("PRH", "PRT")
                    tmp2
                tmp
            | false -> 
                let tmp =
                    let tmp2=
                        Dictionary()
                    tmp2.Add("PRH", "PRT")
                    tmp2.Add("accession", prot.Accession)
                    tmp2.Add("uri", prot.SearchDatabase.Location)
                    tmp2
                tmp

        let tmp2 = Dictionary()

        values.Keys
        |> Seq.iter (fun item ->
            try
                tmp1.Add(columnNames.Item item, values.Item item)
            with
                :? System.Collections.Generic.KeyNotFoundException ->
                tmp2.Add("opt_{" + item + "}_", values.Item item)
                    )
        tmp2 
        |> Seq.iter (fun item -> tmp1.Add(item.Key, item.Value)) |> ignore
        tmp1

let createDictionaryWithValuesAndColumnNames
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

    let dictionaryWithValues =
        CvParamsBase
        |> Array.iter (fun cvParam -> 
            tmp.Item cvParam.Term.ID <- cvParam.Value (*convert4timesStringToSingleString("", cvParam.Term.ID, cvParam.Term.Name, cvParam.Value)*))
        tmp

    addColumnNamesOfProteinSection protein columnNames dictionaryWithValues

//let valuesAndTermIDs =
//    proteinsAndDBSeqs
//    |> Array.map (fun item -> createDictionaryWithValuesAndColumnNames dictionaryOfColumnAndIDs allTerms item)

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

let writeTSVFileAsTable (path:string) (termsAndValues:Dictionary<string, string>[]) =
    let columnNames = 
        termsAndValues.[0].Keys |> Array.ofSeq
    let tmp = 
        termsAndValues |> Array.map (fun item -> item.ToArray())
        |> Array.map (fun outer -> outer |> Array.map (fun inner -> inner.Value))
    [|yield columnNames; yield! tmp|]
    |> Array.map (fun item -> item |> String.concat "\t")
    |> Seq.write path

//let finalStep =
//    writeTSVFileAsTable standardCSVPath valuesAndTermIDs

let createProteinSection (mzQuantContext:MzQuantML) (mzQuantID:string) (mzIdentContext:MzIdentML) (mzIdentID:string) (columnNames:seq<string*string>) =
    
    let tmpDict = Dictionary<string, string>()
    
    let allTerms =
        getAllProteinSectionTerms mzQuantContext mzQuantID mzIdentContext mzIdentID tmpDict

    let namesOfColumns =
        createDictionaryofColumnNames columnNames

    let protsAndDBSeqs = 
        getProteinsAndDBSeqs mzQuantContext mzQuantID mzIdentContext mzIdentID

    protsAndDBSeqs
    |> Array.map (fun item -> createDictionaryWithValuesAndColumnNames namesOfColumns allTerms item)

#time
let proteinSection =
    createProteinSection  sqliteMzQuantMLContext "Test1" sqliteMzIdentMLContext "Test1" columnNames
    |> writeTSVFileAsTable standardCSVPath
