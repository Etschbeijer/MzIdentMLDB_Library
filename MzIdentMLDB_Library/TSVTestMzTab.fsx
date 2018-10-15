    
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
open MzIdentMLDataBase.InsertStatements.ObjectHandlers.DBFunctions
open MzQuantMLDataBase.DataModel
open MzQuantMLDataBase.InsertStatements.ObjectHandlers
open MzQuantMLDataBase.InsertStatements.ObjectHandlers.DBFunctions
open MzBasis.Basetypes


let fileDir = __SOURCE_DIRECTORY__
let pathDB = fileDir + "\Databases/"
let sqliteMzIdentMLDBName = "MzIdentML1.db"
let sqliteMzQuantMLDBName = "MzQuantML1.db"

let sqliteMzIdentMLContext = 
    createMzIdentMLContext DBType.SQLite sqliteMzIdentMLDBName pathDB
sqliteMzIdentMLContext.ChangeTracker.AutoDetectChangesEnabled=false

let sqliteMzQuantMLContext = 
    createMzQuantMLContext DBType.SQLite sqliteMzQuantMLDBName pathDB
sqliteMzQuantMLContext.ChangeTracker.AutoDetectChangesEnabled=false



let convert4timesStringToSingleString ((a,b,c,d):string*string*string*string) =

    sprintf "[%s ,%s ,%s ,%s ]" a b c d

let createSeperatedStringOfCollection (collection:seq<'a>) =

    let tmp = [|""|]

    for i in collection do
        tmp.[0] <- tmp.[0] + i.ToString() + " | "
        //tmp.[0] <- tmp.[0] + "; "

    tmp.[0]

let createDictionaryofColumnNames (columnNames:seq<string*string>) =
    let startDictionary = Dictionary()
    columnNames
    |> Seq.iter (fun (termID, columnName) -> startDictionary.Add(termID, columnName)) |> ignore
    startDictionary

let writeTSVFileAsTable (path:string) (termsAndValues:Dictionary<string, string>[]) =
    let columnNames = 
        termsAndValues.[0].Keys |> Array.ofSeq
    let tmp = 
        termsAndValues |> Array.map (fun item -> item.ToArray())
        |> Array.map (fun outer -> outer |> Array.map (fun inner -> inner.Value))
    [|yield columnNames; yield! tmp|]
    |> Array.map (fun item -> item |> String.concat "\t")
    |> Seq.write path

let createColumnNames (fixedColumnNames:seq<string*string>) (terms:Dictionary<string, string>) =

    let tmp = Dictionary()

    let fixedColumnNames =
        createDictionaryofColumnNames fixedColumnNames 

    let allColumnNames =

        fixedColumnNames.Keys
        |> Seq.iter (fun key ->
            try
                (terms.Item key, fixedColumnNames.Item key) |> ignore
                tmp.Add(key, fixedColumnNames.Item key)
            with
                :? System.Collections.Generic.KeyNotFoundException -> ()
                    )
                     
        terms.Keys
        |> Seq.iter (fun key ->
            try
                (fixedColumnNames.Item key, terms.Item key) |> ignore
            with
                :? System.Collections.Generic.KeyNotFoundException ->
                tmp.Add(key, "opt_{" + (terms.Item key) + "}_")
                    )
    tmp

let filterOfWrongMzdentAndMzQuantIDs (mzIdentID:string) (mzQuantID:string) (idsAndTerms:seq<string*Term>) =
    idsAndTerms
    |> Seq.map (fun (id, term) ->
        match id=mzIdentID || id=mzQuantID with
        | true -> term
        | false -> null
               )
    |> Seq.filter (fun item -> item <> null)

let filterIDs (itemID:string) (idsAndTerms:(string*Term)[]) =
    idsAndTerms
    |> Array.map (fun (id, term) ->
        match id=itemID with
        | true -> term
        | false -> null
               )
    |> Array.filter (fun item -> item <> null)

let filterIDsByMapping (itemID:string) (idsAndTerms:Map<string, Term>) =
    idsAndTerms.Item itemID

    //let rec loop n =
    //    if n=idsAndTerms.Length then 
    //        null
    //    else
    //        if fst idsAndTerms.[n] = itemID then
    //            snd idsAndTerms.[n]
    //        else loop (n+1)
    //loop 0
    

////////////////////////////////////////////////////////|\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
            ////////////PROTEIN-SECTION||||||||||PROTEIN-SECTION||||||||||PROTEIN-SECTION\\\\\\\\\\            
////////////////////////////////////////////////////////|\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

let getAllProteinTerms (dbContext:MzQuantML) =

    query {
           for mzQdoc in dbContext.MzQuantMLDocument
                            .Include(fun item -> item.ProteinList)
                            .ThenInclude(fun item -> item.Proteins :> IEnumerable<_>)
                            .ThenInclude(fun (item:Protein) -> item.Details :> IEnumerable<_>) 
                            .ThenInclude(fun (item:ProteinParam) -> item.Term)
                            .ToList()
                            do
                for prot in mzQdoc.ProteinList.Proteins do                    
                    for cvParam in prot.Details do 
                        select (mzQdoc.ID, cvParam.Term)
                        distinct
          }

let getAllSearchDatabaseTerms (dbContext:MzQuantML) =    

    query {
           for mzQdoc in dbContext.MzQuantMLDocument
                            .Include(fun item -> item.ProteinList)
                            .ThenInclude(fun item -> item.Proteins :> IEnumerable<_>)
                            .ThenInclude(fun (item:Protein) -> item.SearchDatabase.Details :> IEnumerable<_>) 
                            .ThenInclude(fun (item:SearchDatabaseParam) -> item.Term)
                            .ToList()
                            do
                for prot in mzQdoc.ProteinList.Proteins do                    
                    for cvParam in prot.SearchDatabase.Details do 
                        select (mzQdoc.ID, cvParam.Term)
                        distinct
          }

let getAllDBSequenceTerms (dbContext:MzIdentML) =    

    query {
           for mzIdent in dbContext.MzIdentMLDocument
                            .Include(fun item -> item.DBSequences :> IEnumerable<_>) 
                            .ThenInclude(fun (item:DBSequence) -> item.Details :> IEnumerable<_>) 
                            .ThenInclude(fun (item:DBSequenceParam) -> item.Term)
                            .ToList()
                            do
                for dbSeq in mzIdent.DBSequences do  
                    for cvParam in dbSeq.Details do  
                    select (mzIdent.ID, cvParam.Term)
                    distinct
          }

let getAllProteinSectionTerms 
    (mzQuantContext:MzQuantML) (mzQuantID:string) 
    (mzIDentContext:MzIdentML) (mzIdentID:string) 
    (dict:Dictionary<string,string>) =    
    [
     getAllProteinTerms mzQuantContext
     getAllSearchDatabaseTerms mzQuantContext
     getAllDBSequenceTerms mzIDentContext
    ]
    |> Array.ofSeq
    |> Seq.collect (fun term -> filterOfWrongMzdentAndMzQuantIDs mzIdentID mzQuantID term)
    |> Seq.distinct
    |> Seq.iter (fun term -> dict.Add(term.ID, term.Name))
    dict

//replace with Map!!!
let proteinSectionColumnNames =
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

let findAllProteins (dbContext:MzQuantML) (mzQuantID:string) =
    
    query {
           for mzQdoc in dbContext.MzQuantMLDocument
                            .Include(fun item -> item.ProteinList)
                            .ThenInclude(fun item -> item.Proteins :> IEnumerable<_>) 
                            .ToList()
                            do
                for prot in mzQdoc.ProteinList.Proteins do 
                    where (mzQdoc.ID=mzQuantID)
                    select prot    
          }

let findAllDBSequencesOfProteins (dbContext:MzIdentML) (mzIdentID:string) (protAccession:string)=    
    
    query {
           for mzIdent in dbContext.MzIdentMLDocument
                            .Include(fun item -> item.DBSequences :> IEnumerable<_>) 
                            .ToList()
                            do
                for dbSeq in mzIdent.DBSequences do
                    where (mzIdent.ID=mzIdentID && dbSeq.Accession=protAccession)
                    select (dbSeq.Details.ToArray())
          }
    |> Array.ofSeq
    |> Array.concat

let getProteinsAndDBSeqs
    (mzQuantContext:MzQuantML) (mzQuantID:string) (mzIdentContext:MzIdentML) (mzIdentID:string) =

    findAllProteins mzQuantContext mzQuantID
    |> Seq.map (fun protein ->
        protein, findAllDBSequencesOfProteins mzIdentContext mzIdentID protein.Accession
               )
    |> Array.ofSeq

let addColumnNamesOfProteinSection (prot:Protein) (columnNames:Dictionary<string, string>) (values:Dictionary<string, string>) =
    let tmp1 = 
        let tmp =
            let tmp2=
                Dictionary()
            tmp2.Add("PRH", "PRT")
            tmp2.Add("accession", prot.Accession)
            tmp2.Add("uri", prot.SearchDatabase.Location)
            tmp2
        tmp

    columnNames.Keys
    |> Seq.iter (fun item -> tmp1.Add(columnNames.Item item, (values.Item item)))
    tmp1

let createDictionaryWithValuesAndColumnNamesForProteinSection
    (columnNames:Dictionary<string, string>) (terms:Dictionary<string,string>) ((protein, dbSequenceParams):Protein*(DBSequenceParam[])) =
    
    let tmp =
        let tmp1 =
            Dictionary<string, string>()
        terms
        |> Seq.iter (fun item -> tmp1.Add(item.Key, ""))
        tmp1
        
    let CvParamsBase =
        [
         protein.Details 
         |> Seq.map (fun protParam -> protParam :> CVParamBase); 
         protein.SearchDatabase.Details 
         |> Seq.map (fun searchDBParam -> searchDBParam :> CVParamBase); 
         dbSequenceParams 
         |> Seq.map (fun dbSeqParam -> dbSeqParam :> CVParamBase)
        ]
        |> Seq.concat
        |> Array.ofSeq

    let values =
        CvParamsBase
        |> Array.iter (fun cvParam -> 
            tmp.Item cvParam.Term.ID <- cvParam.Value + " | "
                      )
        tmp
        
    addColumnNamesOfProteinSection protein columnNames values

let createProteinSection (mzQuantContext:MzQuantML) (mzQuantID:string) (mzIdentContext:MzIdentML) (mzIdentID:string) (columnNames:seq<string*string>) =
    
    let tmpDict = Dictionary<string, string>()
    
    let allTerms =
        getAllProteinSectionTerms mzQuantContext mzQuantID mzIdentContext mzIdentID tmpDict
    
    let namesOfColumns =
        createColumnNames columnNames allTerms
    
    let protsAndDBSeqs = 
        getProteinsAndDBSeqs mzQuantContext mzQuantID mzIdentContext mzIdentID
    
    protsAndDBSeqs
    |> Array.map (fun item -> createDictionaryWithValuesAndColumnNamesForProteinSection namesOfColumns allTerms item)


////////////////////////////////////////////////////////|\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
            ////////////PEPTIDE-SECTION||||||||||PEPTIDE-SECTION||||||||||PEPTIDE-SECTION\\\\\\\\\\            
////////////////////////////////////////////////////////|\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

let getAllPeptideConsensusIDs (dbContext:MzQuantML) (mzQuantID:string) =
    
    query {
           for mzQdoc in dbContext.MzQuantMLDocument
                            .Include(fun item -> item.ProteinList)
                            .ThenInclude(fun item -> item.Proteins :> IEnumerable<_>)
                            .ThenInclude(fun (item:Protein) -> item.PeptideConsensi :> IEnumerable<_>) 
                            .ToList()
                            do
                for prot in mzQdoc.ProteinList.Proteins do
                    for pept in prot.PeptideConsensi do
                    where (mzQdoc.ID=mzQuantID)
                    select pept.ID
                    distinct
          }

let getAllPeptideTerms (dbContext:MzQuantML) =
    
    query {
           for cvParam in dbContext.PeptideConsensusParam  do
           select cvParam.Term
           distinct
          } 

let getAllPeptideModificationTerms (dbContext:MzQuantML) =
    
    query {
           for pept in dbContext.PeptideConsensus 
                            .Include(fun item -> item.Modifications :> IEnumerable<_>)
                            .ThenInclude(fun (item:Modification) -> item.Detail)
                            .ThenInclude(fun item -> item.Term)
                            .ToList()
                            do
                for modi in pept.Modifications do
                select (pept.ID, modi.Detail.Term) 
                distinct
          }

let getAllPeptideFeatureTerms (dbContext:MzQuantML) =
    
    query {
           for pept in dbContext.PeptideConsensus
                            .Include(fun item -> item.EvidenceRefs :> IEnumerable<_>)
                            .ThenInclude(fun (item:EvidenceRef) -> item.Feature.Details :> IEnumerable<_>)
                            .ThenInclude(fun (item:FeatureParam) -> item.Term)
                            .ToList()
                            do                    
                for eviRefs in pept.EvidenceRefs do
                    for cvParam in eviRefs.Feature.Details do
                    select (pept.ID, cvParam.Term) 
                    distinct
          }

let getAllPeptideSectionTerms 
    (mzQuantContext:MzQuantML) (mzQuantID:string) 
    (mzIdentContext:MzIdentML) (mzIdentID:string) 
    (dict:Dictionary<string,string>) =
    
    let peptideIDs =
        getAllPeptideConsensusIDs mzQuantContext mzQuantID
        |> Array.ofSeq

    let dbSeqTerms =
        getAllDBSequenceTerms mzIdentContext
        |> Array.ofSeq
        |> filterIDs mzIdentID
        |> Array.filter (fun term -> term<>null)

    let allSearchDBTerms =
        getAllSearchDatabaseTerms mzQuantContext
        |> Array.ofSeq
        |> filterIDs mzQuantID
        |> Array.filter (fun term -> term<>null)

    let peptideTerms = 
        getAllPeptideTerms mzQuantContext
        |> Array.ofSeq
        |> Array.distinct
    let terms =
        [  
         getAllPeptideModificationTerms mzQuantContext;
         getAllPeptideFeatureTerms mzQuantContext;
        ]
        |> Seq.concat
        |> Array.ofSeq

    peptideIDs
    |> Array.collect (fun peptideID -> filterIDs peptideID terms)
    |> Array.append peptideTerms
    |> Array.distinct
    |> Array.iter (fun term -> dict.Add(term.ID, term.Name))
    dbSeqTerms
    |> Array.iter (fun term -> dict.Add(term.ID, term.Name))
    allSearchDBTerms
    |> Array.iter (fun term -> dict.Add(term.ID, term.Name))
    dict
    
let peptideSectionColumnNames =
    [
     "MS:1000888","sequence"
     "MS:1000885", "accession"
     "MS:1001363", "unique"
     "MS:1001013", "database"
     "MS:1001016", "database_version"
     "MS:1002337", "search_engine"
     "User:0000079", "best_search_engine_score[1]"
     "MS:1002338", "search_engine_score[1-n]_ms_run[1]"
     //"", "reliability"
     "MS:1001471","modifications"
     "MS:1001114", "retention_time"
     "MS:1000916", "retentime_window" //lower offset
     "MS:1000917", "retentime_window" //upper offset
     "MS:1000041", "charge" //maybe better use object-field"
     "MS:1000040", "mass_to_charge" //maybe better use object-field"
     //"", "uri" //maybe better use object-field"
     //"", "spectra_ref" //maybe better use object-field"
     // "", "peptide_abundance_assay[1-n]"
     // "", "peptide_abundance_study_variable[1-n]"
     //"", "peptide_abundance_stdev_study_variable[1-n]"
     //"", "peptide_abundance_std_error_study_variable[1-n]"
     "", "opt_{identifier}_*"
    ]

let findAllPeptidesConsensusParams (dbContext:MzQuantML) =
    
    query {
           for cvParam in dbContext.PeptideConsensusParam do
           select cvParam
          }

let findAllPeptidesWithProtAccs (dbContext:MzQuantML) (mzQuantID:string) =
    //Needed to retrive cvParams fast!
    let allPeptideParams = 
        findAllPeptidesConsensusParams dbContext
        |> Seq.toArray

    query {
           for mzQdoc in dbContext.MzQuantMLDocument
                            .Include(fun item -> item.ProteinList)
                            .ThenInclude(fun (item:ProteinList) -> item.Proteins :> IEnumerable<_>)
                            .ThenInclude(fun (item:Protein) -> item.PeptideConsensi :> IEnumerable<_>)
                            .ToList()
                            do
                for prot in mzQdoc.ProteinList.Proteins do
                    where (mzQdoc.ID=mzQuantID)
                    select (prot, prot.PeptideConsensi.ToArray())   
          }
    |> Array.ofSeq

let allProtAccsWithPeptidesWithDBSeqParams
    (mzQuantContext:MzQuantML) (mzQuantID:string) (mzIdentContext:MzIdentML) (mzIdentID:string) =

    findAllPeptidesWithProtAccs mzQuantContext mzQuantID
    |> Seq.map (fun (prot, peptides) ->
        prot, peptides, findAllDBSequencesOfProteins mzIdentContext mzIdentID prot.Accession
             )
    |> Array.ofSeq
    |> Array.collect (fun (protAcc, peptides, dbSeqs) -> 
        peptides |> Array.map (fun peptide -> protAcc, peptide, dbSeqs))

let addColumnNamesOfPeptideSection (prot:Protein) (peptide:PeptideConsensus) (columnNames:Dictionary<string, string>) (values:Dictionary<string, string>) =
    let tmp1 =  
        let tmp =
            let tmp2=
                Dictionary()
            tmp2.Add("PEH", "PEP")
            tmp2.Add("sequence", peptide.PeptideSequence)
            tmp2.Add("accession", prot.Accession)
            tmp2.Add("uri", prot.SearchDatabase.Location)
            tmp2.Add("modifications", createSeperatedStringOfCollection (peptide.Modifications |> Seq.map (fun modification -> modification.Residues + "-" +  modification.Detail.Term.Name)))
            tmp2.Add("retention_time", createSeperatedStringOfCollection (peptide.EvidenceRefs |> Seq.map (fun evidenceRef -> evidenceRef.Feature.RetentionTime)))
            tmp2.Add("charge", string peptide.Charge)
            tmp2.Add("mass_to_charge", createSeperatedStringOfCollection (peptide.EvidenceRefs |> Seq.map (fun evidenceRef -> evidenceRef.Feature.MZ)))
            //tmp2.Add("spectra_ref", createSeperatedStringOfCollection (peptide.EvidenceRefs |> Seq.map (fun evidenceRef -> evidenceRef)))
            tmp2
        tmp

    let tmp2 = Dictionary()

    columnNames.Keys
    |> Seq.iter (fun item -> tmp1.Add(columnNames.Item item, (values.Item item)))
    tmp1

let createDictionaryWithValuesAndColumnNamesForPeptideSection
    (columnNames:Dictionary<string, string>) (terms:Dictionary<string,string>) ((prot, peptide, dbSequenceParams):Protein*PeptideConsensus*(DBSequenceParam[])) =
    
    let tmp =
        let tmp1 =
            Dictionary<string, string>()
        terms
        |> Seq.iter (fun item -> tmp1.Add(item.Key, item.Value))
        tmp1
        
    let CvParamsBase =
        [
         prot.SearchDatabase.Details 
         |> Seq.map (fun searchDBParam -> searchDBParam :> CVParamBase);
         peptide.Details 
         |> Seq.map (fun protParam -> protParam :> CVParamBase); 
         peptide.Modifications 
         |> Seq.map (fun modification -> modification.Detail :> CVParamBase); 
         peptide.EvidenceRefs 
         |> Seq.collect (fun evidenceRef -> evidenceRef.Feature.Details |> Seq.map (fun featureParam -> featureParam :> CVParamBase));
         dbSequenceParams 
         |> Seq.map (fun dbSeqParam -> dbSeqParam :> CVParamBase)
        ]
        |> Seq.concat
        |> Array.ofSeq

    let values =
        CvParamsBase
        |> Array.iter (fun cvParam -> 
            tmp.Item cvParam.Term.ID <- cvParam.Value + " | "
                      )
        tmp
    
    addColumnNamesOfPeptideSection prot peptide columnNames values


let createPeptideSection (mzQuantContext:MzQuantML) (mzQuantID:string) (mzIdentContext:MzIdentML) (mzIdentID:string) (columnNames:seq<string*string>) =
    
    let tmpDict = Dictionary<string, string>()

    let allTerms =
        getAllPeptideSectionTerms mzQuantContext mzQuantID mzIdentContext mzIdentID tmpDict
     
    let namesOfColumns =
        createColumnNames columnNames allTerms
    
    let allProtAccsWithPeptidesWithDBSeqParams = 
        allProtAccsWithPeptidesWithDBSeqParams mzQuantContext mzQuantID mzIdentContext mzIdentID
    
    allProtAccsWithPeptidesWithDBSeqParams
    |> Array.map (fun item -> createDictionaryWithValuesAndColumnNamesForPeptideSection namesOfColumns allTerms item)



////////////////////////////////////////////////////////|\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
        ////////////////////////PSM-SECTION||||||||||PSM-SECTION||||||||||PSM-SECTION\\\\\\\\\\            
////////////////////////////////////////////////////////|\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

let getSpectrumIdentificationItemID (dbContext:MzIdentML) =

    query {
           for mzIdent in dbContext.MzIdentMLDocument
                            
                            .Include(fun item -> item.AnalysisData.ProteinDetectionList.ProteinAmbiguityGroups :> IEnumerable<_>) 
                            .ThenInclude(fun (item:ProteinAmbiguityGroup) -> item.ProteinDetectionHypothesis :> IEnumerable<_>)
                            .ThenInclude(fun (item:ProteinDetectionHypothesis) -> item.DBSequence)
            
                            .Include(fun item -> item.AnalysisData.ProteinDetectionList.ProteinAmbiguityGroups :> IEnumerable<_>) 
                            .ThenInclude(fun (item:ProteinAmbiguityGroup) -> item.ProteinDetectionHypothesis :> IEnumerable<_>)
                            .ThenInclude(fun (item:ProteinDetectionHypothesis) -> item.PeptideHypothesis :> IEnumerable<_>)
                            .ThenInclude(fun (item:PeptideHypothesis) -> item.SpectrumIdentificationItems :> IEnumerable<_>)

                            .ToList()
                            do
                for protAmbGroup in mzIdent.AnalysisData.ProteinDetectionList.ProteinAmbiguityGroups do
                    for protDetHyp in protAmbGroup.ProteinDetectionHypothesis do
                        for pepHyp in protDetHyp.PeptideHypothesis do
                            for specItem in pepHyp.SpectrumIdentificationItems do 
                            select (mzIdent.ID, protDetHyp.DBSequence.Accession, specItem.ID)
                            distinct
          }

let getSpectrumIdentificationItemTerms (dbContext:MzIdentML) =

    query {
           for specItem in dbContext.SpectrumIdentificationItem    
                            .Include(fun item -> item.Details :> IEnumerable<_>)
                            .ThenInclude(fun (item:SpectrumIdentificationItemParam) -> item.Term)

                            .ToList()
                            do

            for cvParam in specItem.Details do
            select (specItem.ID, cvParam.Term)
            distinct
          }

let getSpectrumIdentificationResultTerms (dbContext:MzIdentML) =

    query {
           for specItem in dbContext.SpectrumIdentificationItem    
                            
                            .Include(fun item -> item.SpectrumIdentificationResult.Details :> IEnumerable<_>)
                            .ThenInclude(fun (item:SpectrumIdentificationResultParam) -> item.Term)

                            .ToList()
                            do

                for cvParam in specItem.SpectrumIdentificationResult.Details do
                select (specItem.ID, cvParam.Term)
                distinct
          }

let getPeptideEvidenceTerms (dbContext:MzIdentML) =

    query {
           for specItem in dbContext.SpectrumIdentificationItem   
                            
                            .Include(fun item -> item.PeptideEvidences:> IEnumerable<_>)
                            .ThenInclude(fun (item:PeptideEvidence) -> item.Details:> IEnumerable<_>)
                            .ThenInclude(fun (item:PeptideEvidenceParam) -> item.Term)

                            .ToList()
                            do

                for pepEvidence in specItem.PeptideEvidences do
                    for cvParam in pepEvidence.Details do
                    select (specItem.ID, cvParam.Term)
                    distinct
          }

let getPeptideTerms (dbContext:MzIdentML) =

    query {
           for specItem in dbContext.SpectrumIdentificationItem  
                            
                            .Include(fun item -> item.Peptide.Details :> IEnumerable<_>)
                            .ThenInclude(fun (item:PeptideParam) -> item.Term)

                            .ToList()
                            do

                for cvParam in specItem.Peptide.Details do
                select (specItem.ID, cvParam.Term)
                distinct
          }

let getPeptideModificationTerms (dbContext:MzIdentML) =

    query {
           for specItem in dbContext.SpectrumIdentificationItem  
                            
                            .Include(fun (item:SpectrumIdentificationItem) -> item.Peptide.Modifications :> IEnumerable<_>)
                            .ThenInclude(fun (item:MzIdentMLDataBase.DataModel.Modification) -> item.Details :> IEnumerable<_>)
                            .ThenInclude(fun (item:MzIdentMLDataBase.DataModel.ModificationParam) -> item.Term)

                            .ToList()
                            do

                for mods in specItem.Peptide.Modifications do
                    for cvParam in mods.Details do
                    select (specItem.ID, cvParam.Term)
                    distinct
          }

let getAllPSMSectionTerms 
    (mzQuantContext:MzQuantML) (mzQuantID:string) 
    (mzIdentContext:MzIdentML) (mzIdentID:string) (specItemIDs:seq<string>)
    (dict:Dictionary<string,string>) =

    let dbSeqTerms =
        getAllSearchDatabaseTerms mzQuantContext
        |> filterOfWrongMzdentAndMzQuantIDs mzIdentID mzQuantID 
        |> Array.ofSeq
    let terms =
        [
         getSpectrumIdentificationItemTerms mzIdentContext;
         getSpectrumIdentificationResultTerms mzIdentContext;
         getPeptideEvidenceTerms mzIdentContext;
         getPeptideTerms mzIdentContext;
         getPeptideModificationTerms mzIdentContext
        ]
        |> Seq.concat
        |> Array.ofSeq
    specItemIDs
    |> Seq.collect (fun specItemID -> filterIDs specItemID terms)
    |> Seq.distinct
    |> Seq.iter (fun term -> dict.Add(term.ID, term.Name))
    dbSeqTerms
    |> Array.iter (fun term -> dict.Add(term.ID, term.Name))
    dict    

let psmSectionColumnNames =
    [
     //"1000888","sequence" //maybe better use object-field"
     //"1000885", "PSM_ID" //maybe better use object-field"
     //"1000885", "accession" //maybe better use object-field"
     //"1001363", "unique" //maybe better use object-field"
     "1001013", "database"
     "1001016", "database_version"
     "1002337", "search_engine"
     "1002338", "search_engine_score[1-n]"
     //"", "reliability" //maybe better use object-field"
     //"1001471","modifications" //maybe better use object-field"
     "MS:1000894", "retention_time"
     //"1000041", "charge" //maybe better use object-field"
     //"1000040", "exp_mass_to_charge" //maybe better use object-field"
     //"1000040", "calc_mass_to_charge" //maybe better use object-field"
     //"", "spectra_ref" //maybe better use object-field"
     //"", "uri" //maybe better use object-field"
     //"", "pre" //maybe better use object-field"
     // "", "post" //maybe better use object-field"
     // "", "start" //maybe better use object-field"
     //"", "end" //maybe better use object-field"
     "", "opt_{identifier}_*"
    ]

let findAllPSMsOfProteins (dbContext:MzIdentML) (mzIdentID:string) (protAccession:string) =
    
    query {
           for mzIdent in dbContext.MzIdentMLDocument
                            .Include(fun item -> item.AnalysisData.ProteinDetectionList.ProteinAmbiguityGroups :> IEnumerable<_>) 
                            .ThenInclude(fun (item:ProteinAmbiguityGroup) -> item.ProteinDetectionHypothesis :> IEnumerable<_>)
                            .ThenInclude(fun (item:ProteinDetectionHypothesis) -> item.DBSequence)

                            .Include(fun item -> item.AnalysisData.ProteinDetectionList.ProteinAmbiguityGroups :> IEnumerable<_>) 
                            .ThenInclude(fun (item:ProteinAmbiguityGroup) -> item.ProteinDetectionHypothesis :> IEnumerable<_>)
                            .ThenInclude(fun (item:ProteinDetectionHypothesis) -> item.PeptideHypothesis :> IEnumerable<_>)
                            .ThenInclude(fun (item:PeptideHypothesis) -> item.PeptideEvidence)

                            .ToList()
                            do
                for protAmbGroup in mzIdent.AnalysisData.ProteinDetectionList.ProteinAmbiguityGroups do
                    for protDetHyp in protAmbGroup.ProteinDetectionHypothesis do
                        for pepHyp in protDetHyp.PeptideHypothesis do
                            for specItem in pepHyp.SpectrumIdentificationItems do
                            where (mzIdent.ID=mzIdentID && protDetHyp.DBSequence.Accession=protAccession)
                            select (pepHyp, specItem.SpectrumIdentificationResult.SpectrumID, specItem)
          }

let getAllPSMItems 
    (mzIdentContext:MzIdentML) (mzIdentID:string) (prot:Protein) =

    findAllPSMsOfProteins mzIdentContext mzIdentID prot.Accession
    |> Seq.map (fun (pepHyp, spectrumID, spectrumItem) -> pepHyp, prot, spectrumID, spectrumItem)
    |> Array.ofSeq

let addColumnNamesOfPSMSection (pepHyp:PeptideHypothesis) (prot:Protein) (spectrumID:string) (spectrumItem:SpectrumIdentificationItem) (columnNames:Dictionary<string, string>) (values:Dictionary<string, string>) =
    let tmp1 =  
        let tmp =
            let tmp2=
                Dictionary()
            tmp2.Add("PSH", "PSM")
            tmp2.Add("sequence", spectrumItem.Peptide.PeptideSequence)
            tmp2.Add("PSM_ID", spectrumItem.SpectrumIdentificationResult.SpectrumID)
            tmp2.Add("accession", prot.Accession)
            tmp2.Add("modifications", createSeperatedStringOfCollection (spectrumItem.Peptide.Modifications |> Seq.collect (fun modification -> modification.Details |> Seq.map (fun modParam -> modParam.Term.ID + ", " + modParam.Term.Name + ", " + modParam.Value))))
            tmp2.Add("charge", string spectrumItem.ChargeState)
            tmp2.Add("exp_mass_to_charge", string spectrumItem.CalculatedMassToCharge)
            tmp2.Add("calc_mass_to_charge", string spectrumItem.CalculatedMassToCharge)
            tmp2.Add("pre", pepHyp.PeptideEvidence.Pre)
            tmp2.Add("post", pepHyp.PeptideEvidence.Post)
            tmp2.Add("start", string pepHyp.PeptideEvidence.Start)
            tmp2.Add("end", string pepHyp.PeptideEvidence.End)
            tmp2
        tmp

    columnNames.Keys
    |> Seq.iter (fun item -> tmp1.Add(columnNames.Item item, (values.Item item)))
    tmp1

let createDictionaryWithValuesAndColumnNamesForPSMSection
    (columnNames:Dictionary<string, string>) (terms:Dictionary<string,string>) ((pepHyp, prot, spectrumID, spectrumItem):PeptideHypothesis*Protein*string*SpectrumIdentificationItem) =
    
    let tmp =
        let tmp1 =
            Dictionary<string, string>()
        terms
        |> Seq.iter (fun item -> tmp1.Add(item.Key, item.Value))
        tmp1
        
    let CvParamsBase =
        [
         prot.SearchDatabase.Details 
         |> Seq.map (fun searchDBParam -> searchDBParam :> CVParamBase); 
         spectrumItem.Details 
         |> Seq.map (fun specItemDBParam -> specItemDBParam :> CVParamBase);
         spectrumItem.SpectrumIdentificationResult.Details 
         |> Seq.map (fun specRes -> specRes :> CVParamBase)
         spectrumItem.PeptideEvidences 
         |> Seq.collect (fun pepEvi -> pepEvi.Details |> Seq.map (fun pepEviParam -> pepEviParam :> CVParamBase))
         spectrumItem.Peptide.Details 
         |> Seq.map (fun peptideParam -> peptideParam :> CVParamBase);
         spectrumItem.Peptide.Modifications 
         |> Seq.collect (fun modification -> modification.Details |> Seq.map (fun modParam -> modParam :> CVParamBase));
        ]
        |> Seq.concat
        |> Array.ofSeq

    let values =
        CvParamsBase
        |> Array.iter (fun cvParam -> 
            tmp.Item cvParam.Term.ID <- cvParam.Value + " | "
                      )
        tmp

    addColumnNamesOfPSMSection pepHyp prot spectrumID spectrumItem columnNames values

let findRightMzIdentIandProtAcc (mzIdentMLID:string) (proteinAcc:string) (items:seq<string*string*string>) =
    items
    |> Seq.map (fun (mzID, protAcc, spectrumID) -> 
        match mzID=mzIdentMLID && protAcc=proteinAcc with
        | true -> spectrumID
        | false -> null)
    |> Seq.filter (fun item -> item <> null)

let createPSMSection (mzQuantContext:MzQuantML) (mzQuantID:string) (mzIdentContext:MzIdentML) (mzIdentID:string) (columnNames:seq<string*string>) =
    
    let tmpDict = Dictionary<string, string>()

    let proteins = 
        findAllProteins mzQuantContext mzQuantID
        |> Array.ofSeq
       
    let specItemIDs =     
        let items = 
            getSpectrumIdentificationItemID mzIdentContext
        proteins 
        |> Seq.collect (fun protein -> findRightMzIdentIandProtAcc mzIdentID protein.Accession items) 
        
    let allTerms =
        getAllPSMSectionTerms mzQuantContext mzQuantID mzIdentContext mzIdentID specItemIDs tmpDict

    let namesOfColumns =
        createColumnNames columnNames allTerms
    
    let allPSMItems =
        proteins
        |> Array.collect (fun prot -> getAllPSMItems mzIdentContext mzIdentID prot)
    
    allPSMItems
    |> Array.map (fun item -> createDictionaryWithValuesAndColumnNamesForPSMSection namesOfColumns allTerms item)
    

////////////////////////////////////////////////////////|\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
          ////////////METADATA-SECTION||||||||||METADATA-SECTION||||||||||METADATA-SECTION\\\\\\\\\\            
////////////////////////////////////////////////////////|\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

let metaDataSectionColumnNames =
    [
     "1000888", "mzTab-version"                                 //extern
     "1000885", "mzTab-mode"                                    //extern
     "1000888", "mzTab-type"                                    //extern
     "1000885", "mzTab-ID"                                      //extern
     "1000888", "title"                                         //exertn
     "1000885", "description"                                   //extern
     "1000888", "sample_processing[1-n]"                        //Some table?
     "1000885", "instrument[1-n]-name"                          //extern
     "1000888", "instrument[1-n]-source"                        //extern
     "1000885", "instrument[1-n]-analyzer[1-n]"                 //extern
     "1000888", "instrument[1-n]-detector"                      //extern
     "1000885", "software[1-n]"                                 //Table-Software/ AnalysisSoftware
     "1000888", "software[1-n]-setting[1-n]"                    //Table-SoftwareParam/AnalysisSoftwareParam
     "1000885", "protein_search_engine_score[1-n]"              //Software/ AnalysisSoftware
     "1000888", "peptide_search_engine_score[1-n]"              //Table-Software/ AnalysisSoftware
     "1000885", "psm_search_engine_score[1-n]"                  //Table-Software/ AnalysisSoftware
     "1000888", "smallmolecule_search_engine_score[1-n]"        //Table-Software/ AnalysisSoftware
     "1000885", "false_discovery_rate"                          //PeptideParam
     "1000888", "publication[1-n]"                              //BiblioGraphic
     "1000885", "contact[1-n]-name"                             //Persons
     "1000888", "contact[1-n]-affiliation"                      //Person.Organizations
     "1000885", "contact[1-n]-email"                            //Person.PersonParam
     "1000888", "uri[1-n]"                                      //Extern
     "1000885", "fixed_mod[1-n]"                                //Some table?
     "1000888", "fixed_mod[1-n]-site"                           //Some table?
     "1000885", "fixed_mod[1-n]-position"                       //Some table?
     "1000888", "variable_mod[1-n]"                             //Some table?
     "1000885", "variable_mod[1-n]-site"                        //Some table?
     "1000888", "variable_mod[1-n]-position"                    //Some table?
     "1000885", "quantification_method"                         //Extern
     "1000888", "protein-quantification_unit"                   //ProteinParams
     "1000885", "peptide-quantification_unit"                   //PeptideParams
     "1000888", "small_molecule-quantification_unit"            //Some table?
     "1000885", "ms_run[1-n]-format"                            //SpectraData
     "1000888", "ms_run[1-n]-location"                          //SpectraData
     "1000885", "ms_run[1-n]-id_format"                         //Some table?
     "1000888", "ms_run[1-n]-fragmentation_method"              //Some table?
     "1000885", "ms_run[1-n]-hash"                              //Extern
     "1000888", "ms_run[1-n]-hash_method"                       //Extern
     "1000885", "custom[1-n]"                                   //Extern
     "1000888", "sample[1-n]-species[1-n]"                      //DBSeqs
     "1000885", "sample[1-n]-tissue[1-n]"                       //DBSeqs
     "1000888", "sample[1-n]-cell_type[1-n]"                    //DBSeqs
     "1000885", "sample[1-n]-disease[1-n]"                      //DBSeqs
     "1000888", "sample[1-n]-description"                       //Extern
     "1000885", "sample[1-n]-custom[1-n]"                       //Extern
     "1000888", "assay[1-n]-quantification_reagent"             //Extern/Some table?
     "1000885", "assay[1-n]-quantification_mod[1-n]"            //Extern/Some table?
     "1000888", "assay[1-n]-quantification_mod[1-n]-site"       //Extern/Some table?
     "1000885", "assay[1-n]-quantification_mod[1-n]-position"   //Extern/Some table?
     "1000888", "assay[1-n]-sample_ref"                         //Extern/Some table?
     "1000885", "assay[1-n]-ms_run_ref"                         //Extern/Some table?
     "1000888", "study_variable[1-n]-assay_refs"                //Extern/Some table?
     "1000885", "study_variable[1-n]-sample_refs"               //Extern/Some table?
     "1000888", "study_variable[1-n]-description"               //Extern/Some table?
     "1000885", "cvParam[1-n]-label"                            //Extern
     "1000888", "cvParam[1-n]-full_name"                        //Extern
     "1000885", "cvParam[1-n]-version"                          //Extern
     "1000888", "cvParam[1-n]-url"                              //Extern
     "1000885", "colunit-protein"                               //Protein-Params
     "1000888", "colunit-peptide"                               //Peptide-Params
     "1000885", "colunit-psm"                                   //Protein-, peptide- and spectrumIdentification-Params
     "1000888", "colunit-small_molecule"                        //Some table?
    ]

///The results included in an mzTab file can be reported in 2 ways: 
///‘Complete’ (when results for each assay/replicate are included) and 
///‘Summary’, when only the most representative results are reported. 
///1 stand for the value summary and 0 for complete.
type MzTabMode =
    | Summary  = 0 
    | Complete = 1

///The results included in an mzTab file MUST be flagged as ‘Identification’ or ‘Quantification’ 
///- the latter encompassing approaches that are quantification only or quantification and identification.
///1 stand for the value quantification and 0 for identification.
type MzTabType =
    | Identification = 0 
    | Quantification = 1


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////FUNCTION-APPLICATION/////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#time
//let proteinSection =
//    createProteinSection  sqliteMzQuantMLContext "Test1" sqliteMzIdentMLContext "Test1" proteinSectionColumnNames
////    |> writeTSVFileAsTable (fileDir + "\Databases\TSVTest1.tab")

//let peptideSection =
//    createPeptideSection sqliteMzQuantMLContext "Test1" sqliteMzIdentMLContext "Test1" peptideSectionColumnNames
    //|> writeTSVFileAsTable (fileDir + "\Databases\TSVTest2.tab")

//let psmSection =
//    createPSMSection sqliteMzQuantMLContext "Test1" sqliteMzIdentMLContext "Test1" psmSectionColumnNames
////    |> writeTSVFileAsTable (fileDir + "\Databases\TSVTest3.tab")


//let writeWholeTSVFile (path:string) (proteinSection:Dictionary<string, string>[]) (peptideSection:Dictionary<string, string>[]) (psmSection:Dictionary<string, string>[]) =
//    let columnNamesOfProteinSection = 
//        proteinSection.[0].Keys |> Array.ofSeq
//    let valuesOfProteinSection = 
//        proteinSection |> Array.map (fun item -> item.ToArray())
//        |> Array.map (fun outer -> outer |> Array.map (fun inner -> inner.Value))
//    let columnNamesOfPeptideSection = 
//        peptideSection.[0].Keys |> Array.ofSeq
//    let valuesOfPeptideSection = 
//        peptideSection |> Array.map (fun item -> item.ToArray())
//        |> Array.map (fun outer -> outer |> Array.map (fun inner -> inner.Value))
//    let columnNamesOfPSMSection = 
//        psmSection.[0].Keys |> Array.ofSeq
//    let valuesOfPSMSection = 
//        psmSection |> Array.map (fun item -> item.ToArray())
//        |> Array.map (fun outer -> outer |> Array.map (fun inner -> inner.Value))
//    [|
//      yield [|""|]; 
//      yield columnNamesOfProteinSection; yield! valuesOfProteinSection; 
//      yield [|""|]; 
//      yield columnNamesOfPeptideSection; yield! valuesOfPeptideSection;
//      yield [|""|];
//      yield columnNamesOfPSMSection; yield! valuesOfPSMSection;
//    |]
//    |> Array.map (fun item -> item |> String.concat "\t")
//    |> Seq.write path


//let standardCSVPath = fileDir + "\Databases\TSVTest0.tab"

//let wholeTSVFile = 
//    writeWholeTSVFile standardCSVPath proteinSection peptideSection psmSection



//sqliteMzIdentMLContext.Database.CloseConnection()
//sqliteMzQuantMLContext.Database.CloseConnection()


let test =
    findAllPeptidesWithProtAccs sqliteMzQuantMLContext "Test1"