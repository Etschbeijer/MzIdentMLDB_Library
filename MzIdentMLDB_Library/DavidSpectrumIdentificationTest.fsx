    
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
open MzIdentMLDataBase.InsertStatements.ObjectHandlers.UserParams
open MzQuantMLDataBase.DataModel
open MzQuantMLDataBase.InsertStatements.ObjectHandlers
open MzQuantMLDataBase.InsertStatements.ObjectHandlers
open MzQuantMLDataBase.InsertStatements.ObjectHandlers.DBFunctions
open MzBasis.Basetypes


///One (poly)peptide (a sequence with modifications).
type [<AllowNullLiteral>]
    Peptide (
             id:string, name:string, peptideSequence:string, modifications:List<Modification>, 
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
    SpectrumIdentificationItem (
                                id:string, name:string, sample:Sample, fkSample:string, massTable:MassTable, 
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

//Functions

let fileDir = __SOURCE_DIRECTORY__
let pathDB = fileDir + "\Databases/" 
let sqliteMzIdentMLDBName = "MzIdentML1.db"
let sqliteMzQuantMLDBName = "MzQuantML1.db"


let mzIdentMLContext =
    createMzIdentMLContext DBType.SQLite sqliteMzIdentMLDBName pathDB

let mzQuantMLDBContext =
    createMzQuantMLContext DBType.SQLite sqliteMzQuantMLDBName pathDB

//createMzIdentMLDB mzIdentMLContext

//createMzQuantMLDB mzQuantMLDBContext

//mzIdentMLContext.ChangeTracker.AutoDetectChangesEnabled = false

//mzQuantMLDBContext.ChangeTracker.AutoDetectChangesEnabled = false

type OntologyWithPath =
    {
     Name   :   string
     Path   :   string
    }

let createOntologyWithPath (name:string) (path:string) =
    {
     Name   =   name
     Path   =   path
    }

let ontologyNames =
    [
     "Psi-MS"; "Pride"; "Unimod"; "Unit_Ontology"; "UserParam"
    ]

let ontologyPaths =
    [
     (fileDir + "\Ontologies\Psi-MS.txt");
     (fileDir + "\Ontologies\Pride.txt");
     (fileDir + "\Ontologies\Unimod.txt");
     (fileDir + "\Ontologies\Unit_Ontology.txt");
     (fileDir + "\Ontologies\UserParam.txt");
    ]

let standardOntologyWithPaths =
    List.map2 createOntologyWithPath ontologyNames ontologyPaths

//let userParams =
//    [
//     user0; user1; user2; user3; user4; user5; user6; user7; user8; user9; user10; user11; user12; user13; user14; user15; user16; user17; 
//     user18; user19; user20; user21; user22; user23; user24; user25; user26; user27; user28; user29; user30; user31; user32; user33; user34;
//     user35; user36; user37; user38; user39; user40; user41; user42; user43; user44; user45; user46; user47; user48; user49; user50; user51;
//     user52; user53; user54; user55; user56; user57; user58; user59; user60; user61; user62; user63; user64; user65; user66; user67; user68;
//     user69; user70; user71; user72; user73; user74; user75; user76; user77; user78; user79;
//    ]
//    |> List.map (fun term -> createOboString term)
//    |> List.concat

//let userParamOboFile = createOboFile (fileDir + "\Ontologies\UserParam.txt") userParams

//let ontologiesAndTerms =
//    standardOntologyWithPaths
//    |> List.map (fun item -> createOntologyAndTerms item.Name item.Path)

//let addedOntoigiesAndTermsToMzIdentMLDB =
//    ontologiesAndTerms
//    |> List.map (fun item -> addOntologyAndTermsToMzIdentMLDB mzIdentMLContext item)

//let addedOntoigiesAndTermsToMzQuantMLDB =
//    ontologiesAndTerms
//    |> List.map (fun item -> addOntologyAndTermsToMzQuantMLDB mzQuantMLDBContext item)

let mzIdentMLDB =
    initMzIdentMLDB DBType.SQLite sqliteMzIdentMLDBName pathDB ontologyNames ontologyPaths

let mzQuantMLDB =
    initMzQuantMLDB DBType.SQLite sqliteMzQuantMLDBName pathDB ontologyNames ontologyPaths


//Functions to create everything related to spectrumIdentificationItem

let sequestScore =
    TermSymbol.Accession "PRIDE:0000053"

let xTandem =
    TermSymbol.Accession "MS:1001476"

let modificationParams =
    [
     MzIdentMLDataBase.InsertStatements.ObjectHandlers.ModificationParamHandler.init(TermSymbol.toID Oxidation, "Oxidation")
    ]

let modifications =
    MzIdentMLDataBase.InsertStatements.ObjectHandlers.ModificationHandler.init(modificationParams, "Oxidation")
    |> MzIdentMLDataBase.InsertStatements.ObjectHandlers.ModificationHandler.addResidues "M"
    |> MzIdentMLDataBase.InsertStatements.ObjectHandlers.ModificationHandler.addFKPeptide "Peptide 2"

let peptide1 =
    PeptideHandler.init("Some Sequence", "Peptide 1")
    |> PeptideHandler.addToContext mzIdentMLContext

let peptide2 =
    PeptideHandler.init("Some Sequence", "Peptide 2")
    |> PeptideHandler.addModification modifications
    |> PeptideHandler.addToContext mzIdentMLContext

let spectrumIdentificationItemParams =
    [
     SpectrumIdentificationItemParamHandler.init(TermSymbol.toID sequestScore)
     |> SpectrumIdentificationItemParamHandler.addValue "Test Number"
     |> SpectrumIdentificationItemParamHandler.addFKSpectrumIdentificationItem "SpectrumIDentificationItem 1";
     SpectrumIdentificationItemParamHandler.init(TermSymbol.toID xTandem)
     |> SpectrumIdentificationItemParamHandler.addValue "Test Number"
     |> SpectrumIdentificationItemParamHandler.addFKSpectrumIdentificationItem "SpectrumIDentificationItem 1";
     SpectrumIdentificationItemParamHandler.init(TermSymbol.toID AndromedaScore)
     |> SpectrumIdentificationItemParamHandler.addValue "Test Number"
     |> SpectrumIdentificationItemParamHandler.addFKSpectrumIdentificationItem "SpectrumIDentificationItem 1";
     SpectrumIdentificationItemParamHandler.init(TermSymbol.toID sequestScore)
     |> SpectrumIdentificationItemParamHandler.addValue "Test Number"
     |> SpectrumIdentificationItemParamHandler.addFKSpectrumIdentificationItem "SpectrumIDentificationItem 2";
     SpectrumIdentificationItemParamHandler.init(TermSymbol.toID xTandem)
     |> SpectrumIdentificationItemParamHandler.addValue "Test Number"
     |> SpectrumIdentificationItemParamHandler.addFKSpectrumIdentificationItem "SpectrumIDentificationItem 2";
     SpectrumIdentificationItemParamHandler.init(TermSymbol.toID AndromedaScore)
     |> SpectrumIdentificationItemParamHandler.addValue "Test Number"
     |> SpectrumIdentificationItemParamHandler.addFKSpectrumIdentificationItem "SpectrumIDentificationItem 2";
    ]
    |> (fun item -> mzIdentMLContext.AddRange (item.Cast()))

let spectrumIdentificationItem1 =
    SpectrumIdentificationItemHandler.init("Peptide 1", -1, -1., true, 0, "SpectrumIDentificationItem 1")
    |> SpectrumIdentificationItemHandler.addToContext mzIdentMLContext

let spectrumIdentificationItem2 =
    SpectrumIdentificationItemHandler.init("Peptide 2", -1, -1., true, 0, "SpectrumIDentificationItem 2")
    |> SpectrumIdentificationItemHandler.addToContext mzIdentMLContext

mzIdentMLContext.SaveChanges()