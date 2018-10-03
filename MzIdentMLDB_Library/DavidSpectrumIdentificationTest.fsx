    
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
let pathDB = fileDir + "\Databases/" 

let sqliteMzIdentMLDBName = "MzIdentML1.db"
//let sqliteMzIdentMLContext = 
//    MzIdentMLDataBase.InsertStatements.ObjectHandlers.ContextHandler.sqliteConnection (sqliteMzIdentMLDBName, pathDB) 
//sqliteMzIdentMLContext.ChangeTracker.AutoDetectChangesEnabled = false

let sqliteMzQuantMLDBName = "MzQuantML1.db"
//let sqliteMzQuantMLContext = 
//    MzQuantMLDataBase.InsertStatements.ObjectHandlers.ContextHandler.sqliteConnection (sqliteMzQuantMLDBName, pathDB)
//sqliteMzQuantMLContext.ChangeTracker.AutoDetectChangesEnabled = false



type MzIdentMLDBContext =
    | SQLServer
    | SQLite
    static member createDBContext (item:MzIdentMLDBContext) (dbName:string) (path:string) =
        match item with
        | SQLServer -> MzIdentMLDataBase.InsertStatements.ObjectHandlers.ContextHandler.sqlConnection dbName path
        | SQLite    -> MzIdentMLDataBase.InsertStatements.ObjectHandlers.ContextHandler.sqliteConnection dbName path

type MzQuantMLDBContext =
    | SQLServer
    | SQLite
    static member createDBContext (item:MzQuantMLDBContext) (dbName:string) (path:string) =
        match item with
        | SQLServer -> MzQuantMLDataBase.InsertStatements.ObjectHandlers.ContextHandler.sqlConnection dbName path
        | SQLite    -> MzQuantMLDataBase.InsertStatements.ObjectHandlers.ContextHandler.sqliteConnection dbName path

let createMzIdentMLContext (dbType:MzIdentMLDBContext) (dbName:string) (path:string) =
    let dbContext = MzIdentMLDBContext.createDBContext dbType dbName path
    dbContext
    //dbContext.Database.EnsureCreated()

let createMzQuantMLContext (dbType:MzQuantMLDBContext) (dbName:string) (path:string) =
    let dbContext = MzQuantMLDBContext.createDBContext dbType dbName path
    dbContext
    //dbContext.Database.EnsureCreated()

type DBContext =
    | MzIdentML
    | MzQuantML
    static member initDB (
                          contextType:DBContext, dbName:string, ?path:string, 
                          ?dbTypeMzIdentML:MzIdentMLDBContext, ?dbTypeMzQuantML:MzQuantMLDBContext
                         ) =
        let path'            = defaultArg path Unchecked.defaultof<string>
        let dbTypeMzIdentML' = defaultArg dbTypeMzIdentML Unchecked.defaultof<MzIdentMLDBContext>
        let dbTypeMzQuantML' = defaultArg dbTypeMzQuantML Unchecked.defaultof<MzQuantMLDBContext>
        match contextType with
        | MzIdentML -> let dbContext =
                        createMzIdentMLContext dbTypeMzIdentML' dbName path'
                       dbContext.Database.EnsureCreated()
        | MzQuantML -> let dbContext =
                        createMzQuantMLContext dbTypeMzQuantML' dbName path'
                       dbContext.Database.EnsureCreated()

    static member initMzIdentMLDBContext (
                                          dbName:string, dbTypeMzIdentML:MzIdentMLDBContext, ?path:string
                                         ) =
        let path' = defaultArg path Unchecked.defaultof<string>
        createMzIdentMLContext dbTypeMzIdentML dbName path'

    static member initMzQuantMLDBContext (
                                          dbName:string, dbTypeMzQuantML:MzQuantMLDBContext, ?path:string
                                         ) =
        let path' = defaultArg path Unchecked.defaultof<string>
        createMzQuantMLContext dbTypeMzQuantML dbName path'

    static member initDB (
                          contextType:DBContext, ?dbContextMzIdentML:MzIdentML, 
                          ?dbContextMzQuantML:MzQuantML
                         ) =
        let dbTypeMzIdentML' = defaultArg dbContextMzIdentML Unchecked.defaultof<MzIdentML>
        let dbTypeMzQuantML' = defaultArg dbContextMzQuantML Unchecked.defaultof<MzQuantML>
        match contextType with
        | MzIdentML -> dbTypeMzIdentML'.Database.EnsureCreated()
        | MzQuantML -> dbTypeMzQuantML'.Database.EnsureCreated()


///One (poly)peptide (a sequence with modifications).
type [<AllowNullLiteral>]
    Peptide (id:string, name:string, peptideSequence:string, modifications:List<Modification>, 
                substitutionModifications:List<SubstitutionModification>, fkMzIdentMLDocument:string,
                details:List<PeptideParam>, rowVersion:Nullable<DateTime>
            ) =
        let mutable id'                        = id
        let mutable name'                      = name
        let mutable peptideSequence'           = peptideSequence
        let mutable modifications'             = modifications
        let mutable substitutionModifications' = substitutionModifications
        let mutable details'                   = details
        let mutable fkMzIdentMLDocument'       = fkMzIdentMLDocument
        let mutable rowVersion'                = rowVersion

        new() = Peptide(null, null, null, null, null, null, null, Nullable())

        member this.ID with get() = id' and set(value) = id' <- value
        member this.Name with get() = name' and set(value) = name' <- value
        member this.PeptideSequence with get() = peptideSequence' and set(value) = peptideSequence' <- value
        [<ForeignKey("FKPeptide")>]
        member this.Modifications with get() = modifications' and set(value) = modifications' <- value
        [<ForeignKey("FKPeptide")>]
        member this.SubstitutionModifications with get() = substitutionModifications' and set(value) = substitutionModifications' <- value
        member this.FKMzIdentMLDocument with get() = fkMzIdentMLDocument' and set(value) = fkMzIdentMLDocument' <- value
        [<ForeignKey("FKPeptide")>]
        member this.Details with get() = details' and set(value) = details' <- value
        member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

///An identification of a single (poly)peptide, resulting from querying an input spectra, along with the set of confidence values for that identification.
type [<AllowNullLiteral>]
    SpectrumIdentificationItem (id:string, name:string, sample:Sample, fkSample:string, massTable:MassTable, 
                                fkMassTable:string, passThreshold:Nullable<bool>, rank:Nullable<int>, 
                                peptideEvidences:List<PeptideEvidence>, fragmentations:List<IonType>, 
                                peptide:Peptide, fkPeptide:string, chargeState:Nullable<int>, 
                                experimentalMassToCharge:Nullable<float>, 
                                calculatedMassToCharge:Nullable<float>, calculatedPI:Nullable<float>, 
                                spectrumIdentificationResult:SpectrumIdentificationResult,
                                fkSpectrumIdentificationResult:string,
                                fkPeptideHypothesis:string,
                                details:List<SpectrumIdentificationItemParam>, 
                                rowVersion:Nullable<DateTime>
                                ) =

        let mutable id'                             = id
        let mutable name'                           = name
        let mutable sample'                         = sample
        let mutable fkSample'                       = fkSample
        let mutable fkMassTable'                    = fkMassTable
        let mutable massTable'                      = massTable
        let mutable passThreshold'                  = passThreshold
        let mutable rank'                           = rank
        let mutable peptideEvidences'               = peptideEvidences
        let mutable fragmentations'                 = fragmentations
        let mutable peptide'                        = peptide
        let mutable fkPeptide'                      = fkPeptide
        let mutable chargeState'                    = chargeState
        let mutable experimentalMassToCharge'       = experimentalMassToCharge
        let mutable calculatedMassToCharge'         = calculatedMassToCharge
        let mutable calculatedPI'                   = calculatedPI
        let mutable spectrumIdentificationResult'   = spectrumIdentificationResult
        let mutable fkSpectrumIdentificationResult' = fkSpectrumIdentificationResult
        let mutable fkPeptideHypothesis'            = fkPeptideHypothesis
        let mutable details'                        = details
        let mutable rowVersion'                     = rowVersion

        new() = SpectrumIdentificationItem(null, null, null, null, null, null, Nullable(), Nullable(), null, null,
                                            null, null, Nullable(), Nullable(), Nullable(), Nullable(), null,
                                            null, null, null, Nullable()
                                            )

        member this.ID with get() = id' and set(value) = id' <- value
        member this.Name with get() = name' and set(value) = name' <- value
        member this.Sample with get() = sample' and set(value) = sample' <- value
        [<ForeignKey("Sample")>]
        member this.FKSample with get() = fkSample' and set(value) = fkSample' <- value
        member this.MassTable with get() = massTable' and set(value) = massTable' <- value
        [<ForeignKey("MassTable")>]
        member this.FKMassTable with get() = fkMassTable' and set(value) = fkMassTable' <- value
        member this.PassThreshold with get() = passThreshold' and set(value) = passThreshold' <- value
        member this.Rank with get() = rank' and set(value) = rank' <- value
        [<ForeignKey("FKSpectrumIdentificationItem")>]
        member this.PeptideEvidences with get() = peptideEvidences' and set(value) = peptideEvidences' <- value
        [<ForeignKey("FKSpectrumIdentificationItem")>]
        member this.Fragmentations with get() = fragmentations' and set(value) = fragmentations' <- value
        member this.Peptide with get() = peptide' and set(value) = peptide' <- value
        [<ForeignKey("Peptide")>]
        member this.FKPeptide with get() = fkPeptide' and set(value) = fkPeptide' <- value
        member this.ChargeState with get() = chargeState' and set(value) = chargeState' <- value
        member this.ExperimentalMassToCharge with get() = experimentalMassToCharge' and set(value) = experimentalMassToCharge' <- value
        member this.CalculatedMassToCharge with get() = calculatedMassToCharge' and set(value) = calculatedMassToCharge' <- value
        member this.CalculatedPI with get() = calculatedPI' and set(value) = calculatedPI' <- value
        member this.SpectrumIdentificationResult with get() = spectrumIdentificationResult' and set(value) = spectrumIdentificationResult' <- value
        [<ForeignKey("SpectrumIdentificationResult")>]
        member this.FKSpectrumIdentificationResult with get() = fkSpectrumIdentificationResult' and set(value) = fkSpectrumIdentificationResult' <- value
        member this.FKPeptideHypothesis with get() = fkPeptideHypothesis' and set(value) = fkPeptideHypothesis' <- value
        [<ForeignKey("FKSpectrumIdentificationItem")>]
        member this.Details with get() = details' and set(value) = details' <- value
        member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value


type DBContextNew =
    | MzIdentML
    | MzQuantML
    | SQLServer
    | SQLite
    static member initDB (
                          dbContextType:DBContextNew, dbType:DBContextNew, dbName:string, ?path:string
                         ) =
        let path' = defaultArg path Unchecked.defaultof<string>
        match dbContextType with
        | MzIdentML -> match dbType with
                       | SQLServer -> let dbContext =
                                        MzIdentMLDataBase.InsertStatements.ObjectHandlers.ContextHandler.sqlConnection dbName path'
                                      dbContext.Database.EnsureCreated()
                       | SQLite    -> let dbContext =
                                        MzIdentMLDataBase.InsertStatements.ObjectHandlers.ContextHandler.sqliteConnection dbName path'
                                      dbContext.Database.EnsureCreated()
                       | _         -> printfn "Use either SQLServer or SQLite as the dbType." 
                                      false
        | MzQuantML -> match dbType with
                       | SQLServer -> let dbContext =
                                        MzQuantMLDataBase.InsertStatements.ObjectHandlers.ContextHandler.sqlConnection dbName path'
                                      dbContext.Database.EnsureCreated()
                       | SQLite    -> let dbContext =
                                        MzQuantMLDataBase.InsertStatements.ObjectHandlers.ContextHandler.sqliteConnection dbName path'
                                      dbContext.Database.EnsureCreated()
                       | _         -> printfn "Use either SQLServer or SQLite as the dbType." 
                                      false
        | _ -> printfn "Use either MzIdentML or MzQuantML as the dbContextType." 
               false

//Functions

//DBContext.initDB(DBContext.MzIdentML, sqliteMzIdentMLDBName, pathDB, dbTypeMzIdentML=MzIdentMLDBContext.SQLite)
//DBContext.initDB(DBContext.MzQuantML, sqliteMzQuantMLDBName, pathDB, dbTypeMzQuantML=MzQuantMLDBContext.SQLite)

DBContextNew.initDB(DBContextNew.MzIdentML, DBContextNew.SQLite, sqliteMzIdentMLDBName, pathDB)
DBContextNew.initDB(DBContextNew.MzIdentML, DBContextNew.MzIdentML, sqliteMzIdentMLDBName, pathDB)

//Functions to create everything related to spectrumIdentificationItem

let peptide =
    PeptideHandler.init("Some Sequence")

let spectrumIdentificationItem =
    SpectrumIdentificationItemHandler.init("peptide 1", -1, -1., true, -1, "spectrumIDentificationItem 1")