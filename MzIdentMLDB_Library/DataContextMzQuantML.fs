namespace MzQuantMLDataBase

open System
open System.ComponentModel.DataAnnotations.Schema
open Microsoft.EntityFrameworkCore
open System.Collections.Generic


module DataModel =
    open MzIdentMLDataBase.DataModel

    type [<AllowNullLiteral>]
        Term (id:string, name:string, ontology:Ontology, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable name'       = name
            let mutable ontology'   = ontology
            let mutable rowVersion' = rowVersion

            new() = Term(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Ontology with get() = ontology' and set(value) = ontology' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
        
    ///Standarized vocabulary for MS-Database.
    and [<AllowNullLiteral>]
        Ontology (id:string, terms:List<Term>, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable terms'      = terms
            let mutable rowVersion' = rowVersion

            new() = Ontology(null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Terms with get() = terms' and set(value) = terms' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Abstract member for params, in order to enable working without additional functions.
    type [<AllowNullLiteral>]
        CVParamBase =
        abstract member ID         : string
        abstract member Value      : string
        abstract member Term       : Term
        abstract member Unit       : Term
        abstract member RowVersion : Nullable<DateTime>

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("CVParams")>]
        CVParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = CVParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("AnalysisSummaryParams")>]
        AnalysisSummaryParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = AnalysisSummaryParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("FeatureParams")>]
        FeatureParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = FeatureParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("FeatureListParams")>]
        FeatureListParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = FeatureListParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("IdentificationFileParams")>]
        IdentificationFileParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = IdentificationFileParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("AssayParams")>]
        AssayParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = AssayParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("PeptideConsensusParams")>]
        PeptideConsensusParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = PeptideConsensusParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("PeptideConsensusParams")>]
        PeptideConsensusListParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = PeptideConsensusListParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("ProcessingmethodParams")>]
        ProcessingmethodParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ProcessingmethodParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("ProteinParams")>]
        ProteinParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ProteinParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("ProteinParams")>]
        ProteinListParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ProteinListParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion
            
    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("ProteinGroupParams")>]
        ProteinGroupParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ProteinGroupParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("ProteinGroupParams")>]
        ProteinGroupListParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ProteinGroupListParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("RatioCalculationParams")>]
        RatioCalculationParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = RatioCalculationParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("RawFileParams")>]
        RawFileParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = RawFileParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("RawFileParams")>]
        RawFileGroupParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = RawFileGroupParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("RawFileParams")>]
        SearchDatabaseParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = SearchDatabaseParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("RawFileParams")>]
        SmallMoleculeParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = SmallMoleculeParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    type [<AllowNullLiteral>] [<Table("RawFileParams")>]
        SmallMoleculeListParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = SmallMoleculeListParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    type [<AllowNullLiteral>] [<Table("RawFileParams")>]
        AnalysisSoftwareParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = AnalysisSoftwareParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    type [<AllowNullLiteral>] [<Table("RawFileParams")>]
        StudyVariableParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = StudyVariableParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    type [<AllowNullLiteral>] [<Table("RawFileParams")>]
        ProcessingMethodParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ProcessingMethodParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    type [<AllowNullLiteral>] [<Table("RawFileParams")>]
        OrganizationParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = OrganizationParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    type [<AllowNullLiteral>] [<Table("RawFileParams")>]
        PersonParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = PersonParam(null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.RowVersion = x.RowVersion

    ////////////////////////////////////////////////////////////////////////////////////////////
    //End of params///////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////

    ///A software package used in the analysis.
    type [<AllowNullLiteral>] [<Table("Software")>]
        AnalysisSoftware (id:string, version:string, details:List<AnalysisSoftwareParam>, rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'         = id
            let mutable version'    = version
            let mutable details'    = details
            let mutable rowVersion' = rowVersion

            new() = AnalysisSoftware(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Version with get() = version' and set(value) = version' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A file from which this MzQuantML instance was created, including potentially MzQuantML files for earlier stages in a workflow.
    type [<AllowNullLiteral>]
        SourceFile (id:string, name:string, location:string, externalFormatDocumentation:string, fileFormat:CVParam, rowVersion:Nullable<DateTime>) =
            let mutable id'                          = id
            let mutable name'                        = name
            let mutable location'                    = location
            let mutable externalFormatDocumentation' = externalFormatDocumentation
            let mutable fileFormat'                  = fileFormat
            let mutable rowVersion'                  = rowVersion

            new() = SourceFile(null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Location with get() = location' and set(value) = location' <- value
            member this.ExternalFormatDocumentation with get() = externalFormatDocumentation' and set(value) = externalFormatDocumentation' <- value
            member this.FileFormat with get() = fileFormat' and set(value) = fileFormat' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Organizations are entities like companies, universities, government agencies.
    and [<AllowNullLiteral>]
        Organization (id:string, name:string, details:List<OrganizationParam>, parent:string, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable name'       = name
            let mutable details'    = details
            let mutable parent'     = parent
            let mutable rowVersion' = rowVersion

            new() = Organization(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Parent with get() = parent' and set(value) = parent' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A person's name and contact details.
    and [<AllowNullLiteral>]
        Person (id:string, name:string, firstName:string, midInitials:string, 
                lastName:string, organizations:List<Organization>, 
                details:List<PersonParam>, rowVersion:Nullable<DateTime>
                ) =
            let mutable id'            = id
            let mutable name'          = name
            let mutable firstName'     = firstName
            let mutable midInitials'   = midInitials
            let mutable lastName'      = lastName
            let mutable organizations' = organizations
            let mutable details'       = details
            let mutable rowVersion'    = rowVersion

            new() = Person(null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.FirstName with get() = firstName' and set(value) = firstName' <- value
            member this.MidInitials with get() = midInitials' and set(value) = midInitials' <- value
            member this.LastName with get() = lastName' and set(value) = lastName' <- value
            member this.Organizations with get() = organizations' and set(value) = organizations' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The software used for performing the analyses.
    type [<AllowNullLiteral>]
        ContactRole (id:string, person:Person, role:CVParam, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable person'     = person
            let mutable role'       = role
            let mutable rowVersion' = rowVersion

            new() = ContactRole(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Person with get() = person' and set(value) = person' <- value
            member this.Role with get() = role' and set(value) = role' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The provider of the document in terms of the Contact and the software the produced the document instance. 
    and [<AllowNullLiteral>] 
        Provider (id:string, name:string, analysisSoftware:AnalysisSoftware, contactRole:ContactRole, rowVersion:Nullable<DateTime>
                 ) =
            let mutable id'               = id
            let mutable name'             = name
            let mutable analysisSoftware' = analysisSoftware
            let mutable contactRole'      = contactRole
            let mutable rowVersion'       = rowVersion

            new() = Provider(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.AnalysisSoftware with get() = analysisSoftware' and set(value) = analysisSoftware' <- value
            member this.ContactRole with get() = contactRole' and set(value) = contactRole' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A database used for searching mass spectra. Examples include a set of amino acid sequence entries, or annotated spectra libraries.
    and [<AllowNullLiteral>] 
        SearchDatabase (id:string, name:string, location:string, numDatabaseEntries:Nullable<int>, releaseDate:Nullable<DateTime>, 
                        version:string, externalFormatDocumentation:string, fileFormat:CVParam, databaseName:CVParam,
                        details:List<SearchDatabaseParam>, rowVersion:Nullable<DateTime>
                       ) =
            let mutable id'                          = id
            let mutable name'                        = name
            let mutable location'                    = location
            let mutable numDatabaseEntries'          = numDatabaseEntries
            let mutable releaseDate'                 = releaseDate
            let mutable version'                     = version
            let mutable externalFormatDocumentation' = externalFormatDocumentation
            let mutable fileFormat'                  = fileFormat
            let mutable databaseName'                = databaseName
            let mutable details'                     = details
            let mutable rowVersion'                  = rowVersion

            new() = SearchDatabase(null, null, null, Nullable(), Nullable(), null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.location with get() = location' and set(value) = location' <- value
            member this.numDatabaseEntries with get() = numDatabaseEntries' and set(value) = numDatabaseEntries' <- value
            member this.releaseDate with get() = releaseDate' and set(value) = releaseDate' <- value
            member this.version with get() = version' and set(value) = version' <- value
            member this.externalFormatDocumentation with get() = externalFormatDocumentation' and set(value) = externalFormatDocumentation' <- value
            member this.fileFormat with get() = fileFormat' and set(value) = fileFormat' <- value
            member this.databaseName with get() = databaseName' and set(value) = databaseName' <- value
            member this.details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A single identification file associated with this analysis.
    type [<AllowNullLiteral>]
        IdentificationFile (id:string, name:string, location:string, searchDatabase:SearchDatabase,
                            externalFormatDocumentation:string, fileFormat:CVParam,
                            details:List<IdentificationFileParam>, rowVersion:Nullable<DateTime>
                           ) =
            let mutable id'                          = id
            let mutable name'                        = name
            let mutable location'                    = location
            let mutable searchDatabase'              = searchDatabase
            let mutable externalFormatDocumentation' = externalFormatDocumentation
            let mutable fileFormat'                  = fileFormat
            let mutable details'                     = details
            let mutable rowVersion'                  = rowVersion

            new() = IdentificationFile(null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Location with get() = location' and set(value) = location' <- value
            member this.SearchDatabase with get() = searchDatabase' and set(value) = searchDatabase' <- value
            member this.ExternalFormatDocumentation with get() = externalFormatDocumentation' and set(value) = externalFormatDocumentation' <- value
            member this.FileFormat with get() = fileFormat' and set(value) = fileFormat' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Depending on context:
    ///1: Reference for the identification evidence for peptides from the referenced external file and 
    ///unique identifier e.g. a link to an mzIdentML file and ID for the ProteinAmbiguityGroup.
    ///2: Reference for the identification evidence for peptides from the referenced external file and 
    ///unique identifier e.g. a link to an mzIdentML file and ID for the ProteinDetectionHypothesis.

    type [<AllowNullLiteral>]
        IdentificationRef (id:string, fkIdentificationFile:string, identificationFile:IdentificationFile,
                           rowVersion:Nullable<DateTime>
                          ) =
            let mutable id'                   = id
            let mutable fkIdentificationFile' = fkIdentificationFile
            let mutable identificationFile'   = identificationFile
            let mutable rowVersion'           = rowVersion

            new() = IdentificationRef(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.FKIdentificationFile with get() = fkIdentificationFile' and set(value) = fkIdentificationFile' <- value
            member this.IdentificationFile with get() = identificationFile' and set(value) = identificationFile' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Depending on context:
    ///1: Reference for the identification evidence for peptides from the referenced external file 
    ///and unique identifier e.g. a link to an mzIdentML file and ID for the ProteinAmbiguityGroup.
    ///2: Reference for the identification evidence for peptides from the referenced external file 
    ///and unique identifier e.g. a link to an mzIdentML file and ID for the ProteinDetectionHypothesis.
    type [<AllowNullLiteral>]
        MethodFile (id:string, name:string, location:string, externalFormatDocumentation:string, 
                    fileFormat:CVParam, rowVersion:Nullable<DateTime>
                   ) =
            let mutable id'                          = id
            let mutable name'                        = name
            let mutable location'                    = location
            let mutable externalFormatDocumentation' = externalFormatDocumentation
            let mutable fileFormat'                  = fileFormat
            let mutable rowVersion'                  = rowVersion

            new() = MethodFile(null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Location with get() = location' and set(value) = location' <- value
            member this.ExternalFormatDocumentation with get() = externalFormatDocumentation' and set(value) = externalFormatDocumentation' <- value
            member this.FileFormat with get() = fileFormat' and set(value) = fileFormat' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A raw mass spectrometry output file that has been analysed e.g. in mzML format. 
    ///The same raw file can be referenced in multiple assays, for example if it contains multiple samples differentially labelled or tagged. 
    ///Note, the name raw file does not necessarily imply that the file has not been processed, since in some quant methods, 
    ///processed peak list formats such as MGF or DTA can be used, which could be referenced here.
    type [<AllowNullLiteral>]
        RawFile (id:string, name:string, location:string, methodFiles:MethodFile, externalFormatDocumentation:string, 
                 fileFormat:CVParam, details:List<RawFileParam>, rowVersion:Nullable<DateTime>
                ) =
            let mutable id'                          = id
            let mutable name'                        = name
            let mutable location'                    = location
            let mutable methodFiles'                 = methodFiles
            let mutable externalFormatDocumentation' = externalFormatDocumentation
            let mutable fileFormat'                  = fileFormat
            let mutable details'                     = details
            let mutable rowVersion'                  = rowVersion

            new() = RawFile(null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Location with get() = location' and set(value) = location' <- value
            member this.MethodFiles with get() = methodFiles' and set(value) = methodFiles' <- value
            member this.ExternalFormatDocumentation with get() = externalFormatDocumentation' and set(value) = externalFormatDocumentation' <- value
            member this.FileFormat with get() = fileFormat' and set(value) = fileFormat' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The raw file or collection of raw files that together form one unit of analysis. 
    ///This is mandatory unless raw files were not used for quantitation e.g. spectral counting. 
    ///Multiple raw files should only be provided within a group if they have been used for sample pre-fractionation which are later summed together.
    type [<AllowNullLiteral>]
        RawFilesGroup (id:string, rawFiles:List<RawFile>, details:List<RawFileGroupParam> ,rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable rawFiles'   = rawFiles
            let mutable details'    = details
            let mutable rowVersion' = rowVersion

            new() = RawFilesGroup(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.RawFiles with get() = rawFiles' and set(value) = rawFiles' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A single methods file associated with this analysis e.g. a TraML file used for SRM analysis.
    type [<AllowNullLiteral>]
        InputFile (id:string, rawFilesGroups:List<RawFilesGroup>, methodFiles:List<MethodFile>,
                   identificationFile:IdentificationFile, searchDatabases:List<SearchDatabase>,
                   sourceFiles:List<SourceFile>, rowVersion:Nullable<DateTime>
                  ) =
            let mutable id'                 = id
            let mutable rawFilesGroups'     = rawFilesGroups
            let mutable methodFiles'        = methodFiles
            let mutable identificationFile' = identificationFile
            let mutable searchDatabases'    = searchDatabases
            let mutable sourceFiles'        = sourceFiles
            let mutable rowVersion'         = rowVersion

            new() = InputFile(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.RawFilesGroups with get() = rawFilesGroups' and set(value) = rawFilesGroups' <- value
            member this.MethodFiles with get() = methodFiles' and set(value) = methodFiles' <- value
            member this.IdentificationFile with get() = identificationFile' and set(value) = identificationFile' <- value
            member this.SearchDatabases with get() = searchDatabases' and set(value) = searchDatabases' <- value
            member this.SourceFiles with get() = sourceFiles' and set(value) = sourceFiles' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The modification searched for or used to define the label or tag for quantification.
    type [<AllowNullLiteral>]
        Modification (id:string, massDelta:Nullable<float>, residues:string, detail:CVParam, 
                      rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'         = id
            let mutable massDelta'  = massDelta
            let mutable residues'   = residues
            let mutable detail'     = detail
            let mutable rowVersion' = rowVersion

            new() = Modification(null, Nullable(), null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.MassDelta with get() = massDelta' and set(value) = massDelta' <- value
            member this.Residues with get() = residues' and set(value) = residues' <- value
            member this.Detail with get() = detail' and set(value) = detail' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    /////A specification of labels or tags used to define the assay within the raw file, 
    /////such as heavy labelling or iTRAQ tag mass. The Label and Modification is mandatory 
    /////so a specific term is provided under Modification for unlabeled sample for label-free and, 
    /////for example, so-called light samples in a labelling experiment.
    //type [<AllowNullLiteral>]
    //    Label (id:string, modifications:List<Modification>, rowVersion:Nullable<DateTime>) =
    //        let mutable id'            = id
    //        let mutable modifications' = modifications
    //        let mutable rowVersion'    = rowVersion

    //        new() = Label(null, null, Nullable())

    //        member this.ID with get() = id' and set(value) = id' <- value
    //        member this.Modifications with get() = modifications' and set(value) = modifications' <- value
    //        member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Describes a single analysis of a sample (e.g. with the channel mapping in iTRAQ), 
    ///which could constitute multiple raw files e.g. if pre-separation steps have occurred. 
    type [<AllowNullLiteral>]
        Assay (id:string, name:string, rawFilesGroup:RawFilesGroup, label:List<Modification>,
               identificationFile:IdentificationFile, details:List<AssayParam>, 
               rowVersion:Nullable<DateTime>
              ) =
            let mutable id'                 = id
            let mutable name'               = name
            let mutable rawFilesGroup'      = rawFilesGroup
            let mutable label'              = label
            let mutable identificationFile' = identificationFile
            let mutable details'            = details
            let mutable rowVersion'         = rowVersion

            new() = Assay(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.RawFilesGroup with get() = rawFilesGroup' and set(value) = rawFilesGroup' <- value
            member this.Label with get() = label' and set(value) = label' <- value
            member this.IdentificationFile with get() = identificationFile' and set(value) = identificationFile' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A logical grouping of assays into conditions or user-defined study variables such as wild-type versus disease or time points in a time course.
    type [<AllowNullLiteral>]
        StudyVariable (id:string, name:string, assays:List<Assay>, details:List<StudyVariableParam>, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable name'       = name
            let mutable assays'     = assays
            let mutable details'    = details
            let mutable rowVersion' = rowVersion

            new() = StudyVariable(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Assays with get() = assays' and set(value) = assays' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The setup of a ratio of study variables or assays that is referenced elsewhere in the file. 
    ///It is expected that the numerator and denominator MUST both be Assays or MUST both be StudyVariables. 
    ///However, StudyVariables MAY contain 1 to many Assays, thus allowing more complex ratios to be constructed 
    ///if needed via use of StudyVariables with unbalanced numbers of Assays.
    type [<AllowNullLiteral>]
        Ratio (id:string, name:string, denominator:StudyVariable, numerator:StudyVariable, ratioCalculation:List<RatioCalculationParam>,
               numeratorDataType:CVParam, denominatorDataType:CVParam, rowVersion:Nullable<DateTime>
              ) =
            let mutable id'                  = id
            let mutable name'                = name
            let mutable numerator'           = numerator
            let mutable denominator'         = denominator
            let mutable ratioCalculation'    = ratioCalculation
            let mutable numeratorDataType'   = numeratorDataType
            let mutable denominatorDataType' = denominatorDataType
            let mutable rowVersion'          = rowVersion

            new() = Ratio(null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Numerator with get() = numerator' and set(value) = numerator' <- value
            member this.Denominator with get() = denominator' and set(value) = denominator' <- value
            member this.RatioCalculation with get() = ratioCalculation' and set(value) = ratioCalculation' <- value
            member this.NumeratorDataType with get() = numeratorDataType' and set(value) = numeratorDataType' <- value
            member this.DenominatorDataType with get() = denominatorDataType' and set(value) = denominatorDataType' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The coordinates defining the feature in RT and MZ space, given as boundary points or 
    ///a series of rectangles, as encoded by the MassTraceEncoding cvParam on the FeatureList. 
    type [<AllowNullLiteral>]
        MassTrace (id:string, value:Nullable<float>, detail:CVParam, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable value'      = value
            let mutable detail'     = detail
            let mutable rowVersion' = rowVersion

            new() = MassTrace(null, Nullable(), null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Detail with get() = detail' and set(value) = detail' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The datatype and index position of one column of data in the DataMatrix.
    type [<AllowNullLiteral>]
        Column (id:string, index:Nullable<int>, dataType:CVParam, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable index'      = index
            let mutable dataType'   = dataType
            let mutable rowVersion' = rowVersion

            new() = Column(null, Nullable(), null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Index with get() = index' and set(value) = index' <- value
            member this.DataType with get() = dataType' and set(value) = dataType' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    /////Depending on context:
    /////1: Space separated unique identifiers for each column of data, MUST refer to an object in the file i.e. StudyVariable or Assay, depending on the context where the QuantLayer resides.
    /////2: Space separated unique identifiers for each column of data, MUST refer to an object in the file i.e. Ratio elements.
    //type [<AllowNullLiteral>]
    //    ColumnIndex (id:string, rawFileRef:string, rowVersion:Nullable<DateTime>) =
    //        let mutable id'         = id
    //        let mutable rawFileRef' = rawFileRef
    //        let mutable rowVersion' = rowVersion

    //        new() = ColumnIndex(null, null, Nullable())

    //        member this.ID with get() = id' and set(value) = id' <- value
    //        member this.RawFileRef with get() = rawFileRef' and set(value) = rawFileRef' <- value            
    //        member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A matrix of data stored in rows and columns, as defined in the parent QuantLayer.
    type [<AllowNullLiteral>]
        DataMatrix (id:string, row:string, rowVersion:Nullable<DateTime>
                        ) =
            let mutable id'         = id
            let mutable row'        = row
            let mutable rowVersion' = rowVersion

            new() = DataMatrix(null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Row with get() = row' and set(value) = row' <- value            
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Depending on context:
    ///1: Quant layer for reporting data values about protein groups related to different assays i.e. the column index MUST refer to Assays defined in the file. 
    ///2: Quant layer for reporting data values about proteins related to different assays i.e. the column index MUST refer to Assays defined in the file. 
    ///3: Quant layer for reporting data values about peptides related to different assays i.e. the column index MUST refer to Assays defined in the file. 
    ///4: Quant layer for reporting data values about small molecules related to different assays i.e. the column index MUST refer to Assays defined in the file.
    type [<AllowNullLiteral>]
        AssayQuantLayer (id:string, dataType:CVParam, columnIndex:string, dataMatrix:DataMatrix, rowVersion:Nullable<DateTime>
                        ) =
            let mutable id'          = id
            let mutable dataType'    = dataType
            let mutable columnIndex' = columnIndex
            let mutable dataMatrix'  = dataMatrix
            let mutable rowVersion'  = rowVersion

            new() = AssayQuantLayer(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.DataType with get() = dataType' and set(value) = dataType' <- value
            member this.ColumnIndex with get() = columnIndex' and set(value) = columnIndex' <- value
            member this.DataMatrix with get() = dataMatrix' and set(value) = dataMatrix' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Depending on context:
    ///1: Global values corresponding to the ProteinGroup such as the total intensity of the protein group in all assays, Anova etc. 
    ///2: Global values corresponding to the Protein such as the total intensity of the protein in all assays, Anova etc. 
    ///3: Global values corresponding to the Peptide such as the total intensity of peptide in all assays, Anova in a quantitative peptidome experiment etc. 
    ///4: Global values corresponding to the small molecule such as the total intensity of the molecule in all assays, Anova etc. 
    type [<AllowNullLiteral>]
        GlobalQuantLayer (id:string, columns:List<Column>, dataMatrix:DataMatrix, rowVersion:Nullable<DateTime>) =
            let mutable id'           = id
            //Former column name was ColumnDefinition.
            let mutable columns' = columns
            //
            let mutable dataMatrix' = dataMatrix
            let mutable rowVersion'   = rowVersion

            new() = GlobalQuantLayer(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Columns with get() = columns' and set(value) = columns' <- value            
            member this.DataMatrix with get() = dataMatrix' and set(value) = dataMatrix' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Quant layer for reporting data values about MS2 features (e.g. iTRAQ) 
    ///related to different assays i.e. the column index MUST refer to Assays defined in the file.
    type [<AllowNullLiteral>]
        MS2AssayQuantLayer (id:string, dataType:CVParam, columnIndex:string, dataMatrix:DataMatrix,
                            rowVersion:Nullable<DateTime>
                           ) =
            let mutable id'          = id
            let mutable dataType'    = dataType
            let mutable columnIndex' = columnIndex
            let mutable dataMatrix'  = dataMatrix
            let mutable rowVersion'  = rowVersion

            new() = MS2AssayQuantLayer(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.DataType with get() = dataType' and set(value) = dataType' <- value
            member this.ColumnIndex with get() = columnIndex' and set(value) = columnIndex' <- value      
            member this.DataMatrix with get() = dataMatrix' and set(value) = dataMatrix' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Depending on context:
    ///1: Quant layer for reporting data values about protein groups related to different study variables i.e. the column index MUST refer to StudyVariables defined in the file. 
    ///2: Quant layer for reporting data values about proteins related to different study variables i.e. the column index MUST refer to StudyVariables defined in the file. 
    ///3: Quant layer for reporting data values about peptides related to different study variables i.e. the column index MUST refer to StudyVariables defined in the file. 
    ///4: Quant layer for reporting data values about small molecules related to different study variables i.e. the column index MUST refer to StudyVariables defined in the file. 
    type [<AllowNullLiteral>]
        StudyVariableQuantLayer (id:string, dataType:CVParam, columnIndex:string, dataMatrix:DataMatrix, rowVersion:Nullable<DateTime>
                        ) =
            let mutable id'          = id
            let mutable dataType'    = dataType
            let mutable columnIndex' = columnIndex
            let mutable dataMatrix'  = dataMatrix
            let mutable rowVersion'  = rowVersion

            new() = StudyVariableQuantLayer(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.DataType with get() = dataType' and set(value) = dataType' <- value
            member this.ColumnIndex with get() = columnIndex' and set(value) = columnIndex' <- value
            member this.DataMatrix with get() = dataMatrix' and set(value) = dataMatrix' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Depending on context:
    ///1: Quant layer for reporting data values about protein groups related to different ratios i.e. the column index MUST refer to Ratio elements defined in the file.
    ///2: Quant layer for reporting data values about proteins related to different ratios i.e. the column index MUST refer to Ratio elements defined in the file.
    ///3: Quant layer for reporting data values about peptides related to different ratios i.e. the column index MUST refer to Ratio elements defined in the file. 
    ///4: Quant layer for reporting data values about small molecules related to different ratios i.e. the column index MUST refer to Ratio elements defined in the file.

    type [<AllowNullLiteral>]
        RatioQuantLayer (id:string, columnIndex:string, dataMatrix:DataMatrix, rowVersion:Nullable<DateTime>
                        ) =
            let mutable id'          = id
            let mutable columnIndex' = columnIndex
            let mutable dataMatrix'  = dataMatrix
            let mutable rowVersion'  = rowVersion

            new() = RatioQuantLayer(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.ColumnIndex with get() = columnIndex' and set(value) = columnIndex' <- value
            member this.DataMatrix with get() = dataMatrix' and set(value) = dataMatrix' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Description of one step within the data processing pipeline.
    type [<AllowNullLiteral>]
        ProcessingMethod (id:string, order:Nullable<int>, details:List<ProcessingMethodParam>, 
                          rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'         = id
            let mutable order'      = order
            let mutable details'    = details
            let mutable rowVersion' = rowVersion

            new() = ProcessingMethod(null, Nullable(), null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Order with get() = order' and set(value) = order' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Description of the way in which a particular software package was used to analyse data and 
    ///for example produce different quant layers or lists in the file. 
    type [<AllowNullLiteral>]
        DataProcessing (id:string, order:Nullable<int>, analysisSoftware:AnalysisSoftware, 
                        inputObjects:string, outputObjects:string, processingMethods:List<ProcessingMethod>,
                        rowVersion:Nullable<DateTime>
                       ) =
            let mutable id'                    = id
            let mutable order'                 = order
            let mutable analysisSoftware'      = analysisSoftware
            let mutable inputObjects'          = inputObjects
            let mutable outputObjects'         = outputObjects
            let mutable processingMethods' = processingMethods
            let mutable rowVersion'            = rowVersion

            new() = DataProcessing(null,Nullable(), null,  null,  null,  null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Order with get() = order' and set(value) = order' <- value            
            member this.AnalysisSoftware with get() = analysisSoftware' and set(value) = analysisSoftware' <- value
            member this.InputObjects with get() = inputObjects' and set(value) = inputObjects' <- value
            member this.OutputObjects with get() = outputObjects' and set(value) = outputObjects' <- value            
            member this.DataprocessingMethods with get() = processingMethods' and set(value) = processingMethods' <- value
            member this.rowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///External database references for the small molecule identification.
    type [<AllowNullLiteral>]
        DBIdentificationRef (id:string, fkExternalFile:string, searchDatabase:SearchDatabase, rowVersion:Nullable<DateTime>
                            ) =
            let mutable id'             = id
            let mutable fkExternalFile' = fkExternalFile
            let mutable searchDatabase' = searchDatabase
            let mutable rowVersion'     = rowVersion

            new() = DBIdentificationRef(null, null,  null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.FKExternalFile with get() = fkExternalFile' and set(value) = fkExternalFile' <- value            
            member this.SearchDatabase with get() = searchDatabase' and set(value) = searchDatabase' <- value
            member this.rowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///FeatureA region on a (potentially) two-dimensional map of LC-MS (MS1) scans, defined by the 
    ///retention time, mass over charge and optionally a mass trace. Quantitative values about features 
    ///can be added in the associated QuantLayers. For techniques that analyse data from single 
    ///scans e.g. MS2 tagging approaches, a Feature corresponds with the mz of the parent ions only.
    type [<AllowNullLiteral>]
        Feature (id:string, charge:Nullable<int>, fkChromatochram:string, mz:Nullable<float>, rawFile:string, 
                 retentionTime:Nullable<float>, fkSpectrum:string, massTrace:MassTrace,
                 details:List<FeatureParam>, rowVersion:Nullable<DateTime>
                ) =
            let mutable id'              = id
            let mutable charge'          = charge
            let mutable fkChromatochram' = fkChromatochram
            let mutable rawFile'         = rawFile
            [<Column("RT")>]
            let mutable retentionTime'   = retentionTime
            let mutable fkSpectrum'      = fkSpectrum
            let mutable massTrace'       = massTrace
            let mutable details'         = details
            let mutable rowVersion'      = rowVersion

            new() = Feature(null, Nullable(), null, Nullable(), null, Nullable(), null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Charge with get() = charge' and set(value) = charge' <- value            
            member this.FKChromatochram with get() = fkChromatochram' and set(value) = fkChromatochram' <- value
            member this.RawFile with get() = rawFile' and set(value) = rawFile' <- value
            member this.RetentionTime with get() = retentionTime' and set(value) = retentionTime' <- value
            member this.FKSpectrum with get() = fkSpectrum' and set(value) = fkSpectrum' <- value
            member this.MassTrace with get() = massTrace' and set(value) = massTrace' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///An element to represent a unique identifier of a small molecule for which quantitative values are reported.
    type [<AllowNullLiteral>]
        SmallMolecule (id:string, modifications:List<Modification>, dbIdentificationRefs:List<DBIdentificationRef>, feature:Feature, 
                       details:List<SmallMoleculeParam>, rowVersion:Nullable<DateTime>
                      ) =
            let mutable id'                   = id
            let mutable modifications'        = modifications
            let mutable dbIdentificationRefs' = dbIdentificationRefs
            let mutable feature'              = feature
            let mutable details'              = details
            let mutable rowVersion'           = rowVersion

            new() = SmallMolecule(null, null,  null, null,  null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Modifications with get() = modifications' and set(value) = modifications' <- value            
            member this.DBIdentificationRefs with get() = dbIdentificationRefs' and set(value) = dbIdentificationRefs' <- value
            member this.Feature with get() = feature' and set(value) = feature' <- value            
            member this.Details with get() = details' and set(value) = details' <- value
            member this.rowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///TThe list of all individual proteins (i.e. ungrouped) for which quantitation values are being reported. 
    ///If quantitation is done on protein groups, the constituent proteins should be listed here with no QuantLayers.
    type [<AllowNullLiteral>]
        SmallMoleculeList (id:string, proteins:List<SmallMolecule>, globalQuantLayer:List<GlobalQuantLayer>,assayQuantLayer:List<AssayQuantLayer>, 
                           studyVariableQuantLayer:List<StudyVariableQuantLayer>, ratioQuantLayer:List<RatioQuantLayer>,
                           details:List<SmallMoleculeListParam>, rowVersion:Nullable<DateTime>
                          ) =
            let mutable id'                      = id
            let mutable proteins'                = proteins
            let mutable globalQuantLayer'        = globalQuantLayer
            let mutable assayQuantLayer'         = assayQuantLayer
            let mutable studyVariableQuantLayer' = studyVariableQuantLayer
            let mutable ratioQuantLayer'         = ratioQuantLayer
            let mutable details'                 = details
            let mutable rowVersion'              = rowVersion

            new() = SmallMoleculeList(null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Proteins with get() = proteins' and set(value) = proteins' <- value
            member this.GlobalQuantLayer with get() = globalQuantLayer' and set(value) = globalQuantLayer' <- value
            member this.AssayQuantLayer with get() = assayQuantLayer' and set(value) = assayQuantLayer' <- value
            member this.StudyVariableQuantLayer with get() = studyVariableQuantLayer' and set(value) = studyVariableQuantLayer' <- value
            member this.RatioQuantLayer with get() = ratioQuantLayer' and set(value) = ratioQuantLayer' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///All the data values about features in one raw file or raw file group, 
    ///such as feature raw intensity, feature RT window size etc. 
    type [<AllowNullLiteral>]
        FeatureQuantLayer (id:string, columns:List<Column>, dataMatrix:DataMatrix, rowVersion:Nullable<DateTime>) =
            let mutable id'           = id
            //Former column name was ColumnDefinition.
            let mutable columns' = columns
            //
            let mutable dataMatrix' = dataMatrix
            let mutable rowVersion'   = rowVersion

            new() = FeatureQuantLayer(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Columns with get() = columns' and set(value) = columns' <- value            
            member this.DataMatrix with get() = dataMatrix' and set(value) = dataMatrix' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Quant layer for reporting data values about MS2 features (e.g. iTRAQ) 
    ///related to different ratios i.e. the column index MUST refer to Ratio elements defined in the file. 
    type [<AllowNullLiteral>]
        MS2RatioQuantLayer (id:string, dataType:CVParam, columnIndex:string, dataMatrix:DataMatrix,
                            rowVersion:Nullable<DateTime>
                           ) =
            let mutable id'          = id
            let mutable dataType'    = dataType
            let mutable columnIndex' = columnIndex
            let mutable dataMatrix'  = dataMatrix
            let mutable rowVersion'  = rowVersion

            new() = MS2RatioQuantLayer(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.DataType with get() = dataType' and set(value) = dataType' <- value
            member this.ColumnIndex with get() = columnIndex' and set(value) = columnIndex' <- value      
            member this.DataMatrix with get() = dataMatrix' and set(value) = dataMatrix' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Quant layer for reporting data values about MS2 features (e.g. iTRAQ) 
    ///related to different study variables i.e. the column index MUST refer to StudyVariables 
    ///defined in the file. 
    type [<AllowNullLiteral>]
        MS2StudyVariableQuantLayer (id:string, dataType:CVParam, columnIndex:string, 
                                    dataMatrix:DataMatrix, rowVersion:Nullable<DateTime>
                                   ) =
            let mutable id'          = id
            let mutable dataType'    = dataType
            let mutable columnIndex' = columnIndex
            let mutable dataMatrix'  = dataMatrix
            let mutable rowVersion'  = rowVersion

            new() = MS2StudyVariableQuantLayer(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.DataType with get() = dataType' and set(value) = dataType' <- value
            member this.ColumnIndex with get() = columnIndex' and set(value) = columnIndex' <- value      
            member this.DataMatrix with get() = dataMatrix' and set(value) = dataMatrix' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///FeatureA region on a (potentially) two-dimensional map of LC-MS (MS1) scans, defined by the 
    ///retention time, mass over charge and optionally a mass trace. Quantitative values about features 
    ///can be added in the associated QuantLayers. For techniques that analyse data from single 
    ///scans e.g. MS2 tagging approaches, a Feature corresponds with the mz of the parent ions only.
    type [<AllowNullLiteral>]
        FeatureList (id:string, rawFilesGroup:RawFilesGroup, features:List<Feature>, 
                     featureQuantLayers:List<FeatureQuantLayer>, 
                     ms2AssayQuantLayers:List<MS2AssayQuantLayer>, 
                     ms2StudyVariableQuantLayer:List<MS2StudyVariableQuantLayer>, 
                     ms2RatioQuantLayer:List<MS2RatioQuantLayer>, 
                     details:List<FeatureListParam>,rowVersion:Nullable<DateTime>
                    ) =
            let mutable id'                         = id
            let mutable rawFilesGroup'              = rawFilesGroup
            let mutable features'                   = features
            let mutable featureQuantLayers'         = featureQuantLayers
            let mutable ms2AssayQuantLayers'        = ms2AssayQuantLayers
            let mutable ms2StudyVariableQuantLayer' = ms2StudyVariableQuantLayer
            let mutable ms2RatioQuantLayer'         = ms2RatioQuantLayer
            let mutable details'                    = details
            let mutable rowVersion'                 = rowVersion

            new() = FeatureList(null, null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.RawFilesGroup with get() = rawFilesGroup' and set(value) = rawFilesGroup' <- value            
            member this.Features with get() = features' and set(value) = features' <- value
            member this.FeatureQuantLayers with get() = featureQuantLayers' and set(value) = featureQuantLayers' <- value
            member this.MS2AssayQuantLayers with get() = ms2AssayQuantLayers' and set(value) = ms2AssayQuantLayers' <- value
            member this.MS2StudyVariableQuantLayer with get() = ms2StudyVariableQuantLayer' and set(value) = ms2StudyVariableQuantLayer' <- value
            member this.MS2RatioQuantLayer with get() = ms2RatioQuantLayer' and set(value) = ms2RatioQuantLayer' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Evidence associated with the PeptideConsensus, including mandatory associations to features and 
    ///optional references to identifications that have been assigned to the feature.
    type [<AllowNullLiteral>]
        EvidenceRef (id:string, externalFileRef:string, feature:Feature, fkExternalFileRef:string, 
                     identificationFile:IdentificationFile, rowVersion:Nullable<DateTime>
                    ) =
            let mutable id'             = id
            let mutable externalFileRef' = externalFileRef
            let mutable feature' = feature
            let mutable fkExternalFileRef' = fkExternalFileRef
            let mutable identificationFile' = identificationFile
            let mutable rowVersion'     = rowVersion

            new() = EvidenceRef(null, null,  null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.ExternalFileRef with get() = externalFileRef' and set(value) = externalFileRef' <- value            
            member this.Feature with get() = feature' and set(value) = feature' <- value
            member this.FKExternalFileRef with get() = fkExternalFileRef' and set(value) = fkExternalFileRef' <- value
            member this.IdentificationFile with get() = identificationFile' and set(value) = identificationFile' <- value
            member this.rowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///An element representing a peptide in different assays that may or may not have been identified. 
    ///If it has been identified, the sequence and modification(s) SHOULD be reported. 
    ///Within the parent list, it is allowed for there to be multiple instances of the 
    ///same peptide sequence, for example capturing different charge states or different modifications, 
    ///if they are differentially quantified. If peptides with different charge states are aggregated, 
    ///they should be represented by a single PeptideConsensus element.
    type [<AllowNullLiteral>]
        PeptideConsensus (id:string, charge:Nullable<float>, searchDatabase:SearchDatabase, 
                          dataMatrix:DataMatrix, peptideSequence:string, modifications:List<Modification>,
                          evidence:List<EvidenceRef>, details:List<PeptideConsensusParam>,
                          rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'              = id
            let mutable charge'          = charge
            let mutable searchDatabase'  = searchDatabase
            let mutable dataMatrix'      = dataMatrix
            let mutable peptideSequence' = peptideSequence
            let mutable modifications'   = modifications
            let mutable evidence'        = evidence
            let mutable details'         = details
            let mutable rowVersion'      = rowVersion

            new() = PeptideConsensus(null, Nullable(), null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Charge with get() = charge' and set(value) = charge' <- value
            member this.SearchDatabase with get() = searchDatabase' and set(value) = searchDatabase' <- value
            member this.DataMatrix with get() = dataMatrix' and set(value) = dataMatrix' <- value
            member this.PeptideSequence with get() = peptideSequence' and set(value) = peptideSequence' <- value
            member this.Modifications with get() = modifications' and set(value) = modifications' <- value
            member this.Evidence with get() = evidence' and set(value) = evidence' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///One protein that has been quantified in the file, including references to peptides 
    ///on which the quantification is based. 
    type [<AllowNullLiteral>]
        Protein (id:string, accession:string, searchDatabase:SearchDatabase, identificationRef:List<IdentificationRef>, 
                 peptideConsensus:PeptideConsensus, details:List<ProteinParam>, rowVersion:Nullable<DateTime>
                ) =
            let mutable id'                = id
            let mutable accession'         = accession
            let mutable searchDatabase'    = searchDatabase
            let mutable identificationRef' = identificationRef
            let mutable peptideConsensus'  = peptideConsensus
            let mutable details'           = details
            let mutable rowVersion'        = rowVersion

            new() = Protein(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Accession with get() = accession' and set(value) = accession' <- value
            member this.SearchDatabase with get() = searchDatabase' and set(value) = searchDatabase' <- value
            member this.IdentificationRef with get() = identificationRef' and set(value) = identificationRef' <- value
            member this.PeptideConsensus with get() = peptideConsensus' and set(value) = peptideConsensus' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///TThe list of all individual proteins (i.e. ungrouped) for which quantitation values are being reported. 
    ///If quantitation is done on protein groups, the constituent proteins should be listed here with no QuantLayers.
    type [<AllowNullLiteral>]
        ProteinList (id:string, proteins:List<Protein>, globalQuantLayer:List<GlobalQuantLayer>,assayQuantLayer:List<AssayQuantLayer>, 
                     studyVariableQuantLayer:List<StudyVariableQuantLayer>, ratioQuantLayer:List<RatioQuantLayer>,
                     details:List<ProteinListParam>, rowVersion:Nullable<DateTime>
                    ) =
            let mutable id'                      = id
            let mutable proteins'                = proteins
            let mutable globalQuantLayer'        = globalQuantLayer
            let mutable assayQuantLayer'         = assayQuantLayer
            let mutable studyVariableQuantLayer' = studyVariableQuantLayer
            let mutable ratioQuantLayer'         = ratioQuantLayer
            let mutable details'                 = details
            let mutable rowVersion'              = rowVersion

            new() = ProteinList(null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Proteins with get() = proteins' and set(value) = proteins' <- value
            member this.GlobalQuantLayer with get() = globalQuantLayer' and set(value) = globalQuantLayer' <- value
            member this.AssayQuantLayer with get() = assayQuantLayer' and set(value) = assayQuantLayer' <- value
            member this.StudyVariableQuantLayer with get() = studyVariableQuantLayer' and set(value) = studyVariableQuantLayer' <- value
            member this.RatioQuantLayer with get() = ratioQuantLayer' and set(value) = ratioQuantLayer' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A reference to one of the Proteins contained within this group, 
    ///along with CV terms describing the role it plays within the group, 
    ///such as representative or anchor protein, same set or sub-set.
    type [<AllowNullLiteral>]
        ProteinRef (id:string, protein:Protein, details:List<ProteinParam>, rowVersion:Nullable<DateTime>
                   ) =
            let mutable id'         = id
            let mutable protein'    = protein
            let mutable details'    = details
            let mutable rowVersion' = rowVersion

            new() = ProteinRef(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Protein with get() = protein' and set(value) = protein' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A grouping of quantified proteins based on ambiguous assignment of peptide evidence to protein identification. 
    ///The semantics of elements within the group, such as a leading protein or those sharing equal evidence can be reported using cvParams.
    type [<AllowNullLiteral>]
        ProteinGroup (id:string, searchDatabase:SearchDatabase, identificationRef:List<IdentificationRef>, 
                      proteinRefs:List<ProteinRef>, details:List<ProteinGroupParam>, rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'                = id
            let mutable searchDatabase'    = searchDatabase
            let mutable identificationRef' = identificationRef
            let mutable proteinRefs'       = proteinRefs
            let mutable details'           = details
            let mutable rowVersion'        = rowVersion

            new() = ProteinGroup(null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.SearchDatabase with get() = searchDatabase' and set(value) = searchDatabase' <- value
            member this.IdentificationRef with get() = identificationRef' and set(value) = identificationRef' <- value
            member this.Protein with get() = proteinRefs' and set(value) = proteinRefs' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
    
    ///The list of all groups of proteins with conflicting evidence for which quantitation values are being reported 
    ///along with quantitative values about those protein groups.
    type [<AllowNullLiteral>]
        ProteinGroupList (id:string, proteinGroups:List<ProteinGroup>, globalQuantLayer:List<GlobalQuantLayer>,assayQuantLayer:List<AssayQuantLayer>, 
                          studyVariableQuantLayer:List<StudyVariableQuantLayer>, ratioQuantLayer:List<RatioQuantLayer>,
                          details:List<ProteinGroupListParam>, rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'                      = id
            let mutable proteinGroups'           = proteinGroups
            let mutable globalQuantLayer'        = globalQuantLayer
            let mutable assayQuantLayer'         = assayQuantLayer
            let mutable studyVariableQuantLayer' = studyVariableQuantLayer
            let mutable ratioQuantLayer'         = ratioQuantLayer
            let mutable details'                 = details
            let mutable rowVersion'              = rowVersion

            new() = ProteinGroupList(null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.ProteinGroup with get() = proteinGroups' and set(value) = proteinGroups' <- value
            member this.GlobalQuantLayer with get() = globalQuantLayer' and set(value) = globalQuantLayer' <- value
            member this.AssayQuantLayer with get() = assayQuantLayer' and set(value) = assayQuantLayer' <- value
            member this.StudyVariableQuantLayer with get() = studyVariableQuantLayer' and set(value) = studyVariableQuantLayer' <- value
            member this.RatioQuantLayer with get() = ratioQuantLayer' and set(value) = ratioQuantLayer' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The list of all peptides for which quantitation values are reported. 
    type [<AllowNullLiteral>]
        PeptideConsensusList (id:string, finalResult:Nullable<bool>, peptideConsensi:List<PeptideConsensus>, 
                              globalQuantLayer:List<GlobalQuantLayer>, assayQuantLayer:List<AssayQuantLayer>, 
                              studyVariableQuantLayer:List<StudyVariableQuantLayer>, 
                              ratioQuantLayer:RatioQuantLayer, details:List<PeptideConsensusListParam>, 
                              rowVersion:Nullable<DateTime>
                             ) =
            let mutable id'                      = id
            let mutable finalResult'             = finalResult
            let mutable peptideConsensi'         = peptideConsensi
            let mutable globalQuantLayer'        = globalQuantLayer
            let mutable assayQuantLayer'         = assayQuantLayer
            let mutable studyVariableQuantLayer' = studyVariableQuantLayer
            let mutable ratioQuantLayer'         = ratioQuantLayer
            let mutable details'                 = details
            let mutable rowVersion'              = rowVersion

            new() = PeptideConsensusList(null, Nullable(), null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.FinalResult with get() = finalResult' and set(value) = finalResult' <- value
            member this.PeptideConsensi with get() = peptideConsensi' and set(value) = peptideConsensi' <- value
            member this.GlobalQuantLayer with get() = globalQuantLayer' and set(value) = globalQuantLayer' <- value
            member this.AssayQuantLayer with get() = assayQuantLayer' and set(value) = assayQuantLayer' <- value
            member this.StudyVariableQuantLayer with get() = studyVariableQuantLayer' and set(value) = studyVariableQuantLayer' <- value
            member this.RatioQuantLayer with get() = ratioQuantLayer' and set(value) = ratioQuantLayer' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Any bibliographic references associated with the file.
    and [<AllowNullLiteral>] 
        BiblioGraphicReference (id:string, name:string, authors:string, doi:string, editor:string, 
                                issue:string, pages:string, publication:string, publisher:string, title:string,
                                volume:string, year:Nullable<int>, mzIdentML:MzIdentMLDocument, rowVersion:Nullable<DateTime>
                               ) =
            let mutable id'          = id
            let mutable name'        = name
            let mutable authors'     = authors
            let mutable doi'         = doi
            let mutable editor'      = editor
            let mutable issue'       = issue
            let mutable pages'       = pages
            let mutable publication' = publication
            let mutable publisher'   = publisher
            let mutable title'       = title
            let mutable volume'      = volume
            let mutable year'        = year
            let mutable mzIdentML'   = mzIdentML
            let mutable rowVersion'  = rowVersion

            new() = BiblioGraphicReference(null, null, null, null, null, null, null, null, null, null, null, Nullable(), null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Authors with get() = authors' and set(value) = authors' <- value
            member this.DOI with get() = doi' and set(value) = doi' <- value
            member this.Editor with get() = editor' and set(value) = editor' <- value
            member this.Issue with get() = issue' and set(value) = issue' <- value
            member this.Pages with get() = pages' and set(value) = pages' <- value
            member this.Publication with get() = publication' and set(value) = publication' <- value
            member this.Publisher with get() = publisher' and set(value) = publisher' <- value
            member this.Title with get() = title' and set(value) = title' <- value
            member this.Volume with get() = volume' and set(value) = volume' <- value
            member this.Year with get() = year' and set(value) = year' <- value
            member this.MzIdentMLDocument with get() = mzIdentML' and set(value) = mzIdentML' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
    
    ///Root element of the instance document. 
    type [<AllowNullLiteral>]
        MzQuantMLDocument (id:string, name:string, creationDate:Nullable<DateTime>, version:string, 
                           provider:Provider, persons:List<Person>, organzations:List<Organization>, analysisSummary:List<AnalysisSummaryParam>, 
                           inputFiles:List<InputFile>, analysisSoftwares:List<AnalysisSoftware>, dataProcessings:List<DataProcessing>, assays:List<Assay>,
                           bibliographicReferences:List<BiblioGraphicReference>, studyVariables:List<StudyVariable>, 
                           ratios:List<Ratio>, proteinGroups:List<ProteinGroup>, proteins:List<Protein>, 
                           peptideConsensi:List<PeptideConsensus>, smallMolecules:List<SmallMolecule>, 
                           features:List<Feature>, rowVersion:Nullable<DateTime>
                          ) =
            let mutable id'                      = id
            let mutable name'                    = name
            let mutable creationDate'            = creationDate
            let mutable version'                 = version
            let mutable provider'                = provider
            //Formerly Audicollection
            let mutable persons'                 = persons
            let mutable organzations'            = organzations
            //
            let mutable analysisSummary'         = analysisSummary
            //Formerly SoftwareList
            let mutable analysisSoftwares'       = analysisSoftwares
            //
            let mutable inputFiles'              = inputFiles
            //Formerly DataProcessingList
            let mutable dataProcessings'         = dataProcessings
            //
            let mutable bibliographicReferences' = bibliographicReferences
            //Formerly AssayList
            let mutable assays'                  = assays
            //
            //Formerly StudyVariableList
            let mutable studyVariables'          = studyVariables
            //
            //Formerly RatioList
            let mutable ratios'                  = ratios
            //
            //Formerly ProteinGroupList
            let mutable proteinGroups'           = proteinGroups
            //
            //Formerly ProteinList
            let mutable proteins'                = proteins
            //
            //Formerly PeptideConsensusList
            let mutable peptideConsensi'         = peptideConsensi
            //
            //Formerly SmallMoceluleList
            let mutable smallMolecules'          = smallMolecules
            //
            //Formerly FeatureList
            let mutable features'                = features
            //
            let mutable rowVersion'              = rowVersion

            new() = MzQuantMLDocument(null, null, Nullable(), null, null, null, null, null, null, null, null, null,
                                      null, null, null, null, null, null, null, null, Nullable()
                                     )

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.CreationDate with get() = creationDate' and set(value) = creationDate' <- value
            member this.Version with get() = version' and set(value) = version' <- value
            member this.Provider with get() = provider' and set(value) = provider' <- value
            member this.Persons with get() = persons' and set(value) = persons' <- value
            member this.Organzations with get() = organzations' and set(value) = organzations' <- value
            member this.AnalysisSummary with get() = analysisSummary' and set(value) = analysisSummary' <- value
            member this.AnalysisSoftwares with get() = analysisSoftwares' and set(value) = analysisSoftwares' <- value
            member this.InputFiles with get() = inputFiles' and set(value) = inputFiles' <- value
            member this.DataProcessings with get() = dataProcessings' and set(value) = dataProcessings' <- value
            member this.BibliographicReferences with get() = bibliographicReferences' and set(value) = bibliographicReferences' <- value
            member this.Assays with get() = assays' and set(value) = assays' <- value
            member this.StudyVariables with get() = studyVariables' and set(value) = studyVariables' <- value
            member this.Ratios with get() = ratios' and set(value) = ratios' <- value
            member this.ProteinGroups with get() = proteinGroups' and set(value) = proteinGroups' <- value
            member this.Proteins with get() = proteins' and set(value) = proteins' <- value
            member this.PeptideConsensi with get() = peptideConsensi' and set(value) = peptideConsensi' <- value
            member this.SmallMolecules with get() = smallMolecules' and set(value) = smallMolecules' <- value
            member this.Features with get() = features' and set(value) = features' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value


    type MzQuantML =
     
            inherit DbContext

            new(options : DbContextOptions<MzQuantML>) = {inherit DbContext(options)}

            [<DefaultValue>] 
            val mutable m_term : DbSet<Term>
            member public this.Term with get() = this.m_term
                                                 and set value = this.m_term <- value

            [<DefaultValue>] 
            val mutable m_ontology : DbSet<Term>
            member public this.Ontology with get() = this.m_ontology
                                                     and set value = this.m_ontology <- value

            [<DefaultValue>] 
            val mutable m_AnalysisSoftware : DbSet<Term>
            member public this.AnalysisSoftware with get() = this.m_AnalysisSoftware
                                                             and set value = this.m_AnalysisSoftware <- value

            [<DefaultValue>] 
            val mutable m_SourceFile : DbSet<Term>
            member public this.SourceFile with get() = this.m_SourceFile
                                                       and set value = this.m_SourceFile <- value

            [<DefaultValue>] 
            val mutable m_Organization : DbSet<Term>
            member public this.Organization with get() = this.m_Organization
                                                         and set value = this.m_Organization <- value

            [<DefaultValue>] 
            val mutable m_Person : DbSet<Term>
            member public this.Person with get() = this.m_Person
                                                   and set value = this.m_Person <- value

            [<DefaultValue>] 
            val mutable m_ContactRole : DbSet<Term>
            member public this.ContactRole with get() = this.m_ContactRole
                                                        and set value = this.m_ContactRole <- value

            [<DefaultValue>] 
            val mutable m_Provider : DbSet<Term>
            member public this.Provider with get() = this.m_Provider
                                                     and set value = this.m_Provider <- value

            [<DefaultValue>] 
            val mutable m_SearchDatabase : DbSet<Term>
            member public this.SearchDatabase with get() = this.m_SearchDatabase
                                                           and set value = this.m_SearchDatabase <- value

            [<DefaultValue>] 
            val mutable m_IdentificationFile : DbSet<Term>
            member public this.IdentificationFile with get() = this.m_IdentificationFile
                                                               and set value = this.m_IdentificationFile <- value

            [<DefaultValue>] 
            val mutable m_IdentificationRef : DbSet<Term>
            member public this.IdentificationRef with get() = this.m_IdentificationRef
                                                              and set value = this.m_IdentificationRef <- value

            [<DefaultValue>] 
            val mutable m_MethodFile : DbSet<Term>
            member public this.MethodFile with get() = this.m_MethodFile
                                                       and set value = this.m_MethodFile <- value

            [<DefaultValue>] 
            val mutable m_RawFile : DbSet<Term>
            member public this.RawFile with get() = this.m_RawFile
                                                    and set value = this.m_RawFile <- value

            [<DefaultValue>] 
            val mutable m_RawFilesGroup : DbSet<Term>
            member public this.RawFilesGroup with get() = this.m_RawFilesGroup
                                                          and set value = this.m_RawFilesGroup <- value

            [<DefaultValue>] 
            val mutable m_InputFile : DbSet<Term>
            member public this.InputFile with get() = this.m_InputFile
                                                      and set value = this.m_InputFile <- value

            [<DefaultValue>] 
            val mutable m_Modification : DbSet<Term>
            member public this.Modification with get() = this.m_Modification
                                                         and set value = this.m_Modification <- value

            [<DefaultValue>] 
            val mutable m_Assay : DbSet<Term>
            member public this.Assay with get() = this.m_Assay
                                                  and set value = this.m_Assay <- value

            [<DefaultValue>] 
            val mutable m_StudyVariable : DbSet<Term>
            member public this.StudyVariable with get() = this.m_StudyVariable
                                                          and set value = this.m_StudyVariable <- value

            [<DefaultValue>] 
            val mutable m_Ratio : DbSet<Term>
            member public this.Ratio with get() = this.m_Ratio
                                                    and set value = this.m_Ratio <- value

            [<DefaultValue>] 
            val mutable m_MassTrace : DbSet<Term>
            member public this.MassTrace with get() = this.m_MassTrace
                                                      and set value = this.m_MassTrace <- value

            [<DefaultValue>] 
            val mutable m_Column : DbSet<Term>
            member public this.Column with get() = this.m_Column
                                                   and set value = this.m_Column <- value

            [<DefaultValue>] 
            val mutable m_DataMatrix : DbSet<Term>
            member public this.DataMatrix with get() = this.m_DataMatrix
                                                       and set value = this.m_DataMatrix <- value

            [<DefaultValue>] 
            val mutable m_AssayQuantLayer : DbSet<Term>
            member public this.AssayQuantLayer with get() = this.m_AssayQuantLayer
                                                            and set value = this.m_AssayQuantLayer <- value

            [<DefaultValue>] 
            val mutable m_GlobalQuantLayer : DbSet<Term>
            member public this.GlobalQuantLayer with get() = this.m_GlobalQuantLayer
                                                             and set value = this.m_GlobalQuantLayer <- value

            [<DefaultValue>] 
            val mutable m_MS2AssayQuantLayer : DbSet<Term>
            member public this.MS2AssayQuantLayer with get() = this.m_MS2AssayQuantLayer
                                                               and set value = this.m_MS2AssayQuantLayer <- value

            [<DefaultValue>] 
            val mutable m_StudyVariableQuantLayer : DbSet<Term>
            member public this.StudyVariableQuantLayer with get() = this.m_StudyVariableQuantLayer
                                                                    and set value = this.m_StudyVariableQuantLayer <- value

            [<DefaultValue>] 
            val mutable m_RatioQuantLayer : DbSet<Term>
            member public this.RatioQuantLayer with get() = this.m_RatioQuantLayer
                                                            and set value = this.m_RatioQuantLayer <- value

            [<DefaultValue>] 
            val mutable m_ProcessingMethod : DbSet<Term>
            member public this.ProcessingMethod with get() = this.m_ProcessingMethod
                                                             and set value = this.m_ProcessingMethod <- value

            [<DefaultValue>] 
            val mutable m_DataProcessing : DbSet<Term>
            member public this.DataProcessing with get() = this.m_DataProcessing
                                                           and set value = this.m_DataProcessing <- value

            [<DefaultValue>] 
            val mutable m_DBIdentificationRef : DbSet<Term>
            member public this.DBIdentificationRef with get() = this.m_DBIdentificationRef
                                                                and set value = this.m_DBIdentificationRef <- value

            [<DefaultValue>] 
            val mutable m_Feature : DbSet<Term>
            member public this.Feature with get() = this.m_Feature
                                                    and set value = this.m_Feature <- value

            [<DefaultValue>] 
            val mutable m_SmallMolecule : DbSet<Term>
            member public this.SmallMolecule with get() = this.m_SmallMolecule
                                                          and set value = this.m_SmallMolecule <- value

            [<DefaultValue>] 
            val mutable m_SmallMoleculeList : DbSet<Term>
            member public this.SmallMoleculeList with get() = this.m_SmallMoleculeList
                                                              and set value = this.m_SmallMoleculeList <- value

            [<DefaultValue>] 
            val mutable m_FeatureQuantLayer : DbSet<Term>
            member public this.FeatureQuantLayer with get() = this.m_FeatureQuantLayer
                                                              and set value = this.m_FeatureQuantLayer <- value

            [<DefaultValue>] 
            val mutable m_MS2RatioQuantLayer : DbSet<Term>
            member public this.MS2RatioQuantLayer with get() = this.m_MS2RatioQuantLayer
                                                               and set value = this.m_MS2RatioQuantLayer <- value

            [<DefaultValue>] 
            val mutable m_MS2StudyVariableQuantLayer : DbSet<Term>
            member public this.MS2StudyVariableQuantLayer with get() = this.m_MS2StudyVariableQuantLayer
                                                                       and set value = this.m_MS2StudyVariableQuantLayer <- value

            [<DefaultValue>] 
            val mutable m_FeatureList : DbSet<Term>
            member public this.FeatureList with get() = this.m_FeatureList
                                                        and set value = this.m_FeatureList <- value

            [<DefaultValue>] 
            val mutable m_EvidenceRef : DbSet<Term>
            member public this.EvidenceRef with get() = this.m_EvidenceRef
                                                        and set value = this.m_EvidenceRef <- value

            [<DefaultValue>] 
            val mutable m_PeptideConsensus : DbSet<Term>
            member public this.PeptideConsensus with get() = this.m_PeptideConsensus
                                                             and set value = this.m_PeptideConsensus <- value

            [<DefaultValue>] 
            val mutable m_Protein : DbSet<Term>
            member public this.Protein with get() = this.m_Protein
                                                    and set value = this.m_Protein <- value

            [<DefaultValue>] 
            val mutable m_ProteinList : DbSet<Term>
            member public this.ProteinList with get() = this.m_ProteinList
                                                        and set value = this.m_ProteinList <- value

            [<DefaultValue>] 
            val mutable m_ProteinRef : DbSet<Term>
            member public this.ProteinRef with get() = this.m_ProteinRef
                                                       and set value = this.m_ProteinRef <- value

            [<DefaultValue>] 
            val mutable m_ProteinGroup : DbSet<Term>
            member public this.ProteinGroup with get() = this.m_ProteinGroup
                                                         and set value = this.m_ProteinGroup <- value

            [<DefaultValue>] 
            val mutable m_ProteinGroupList : DbSet<Term>
            member public this.ProteinGroupList with get() = this.m_ProteinGroupList
                                                             and set value = this.m_ProteinGroupList <- value

            [<DefaultValue>] 
            val mutable m_PeptideConsensusList : DbSet<Term>
            member public this.PeptideConsensusList with get() = this.m_PeptideConsensusList
                                                                 and set value = this.m_PeptideConsensusList <- value

            [<DefaultValue>] 
            val mutable m_BiblioGraphicReference : DbSet<Term>
            member public this.BiblioGraphicReference with get() = this.m_BiblioGraphicReference
                                                                   and set value = this.m_BiblioGraphicReference <- value

            [<DefaultValue>] 
            val mutable m_MzQuantMLDocument : DbSet<Term>
            member public this.MzQuantMLDocument with get() = this.m_MzQuantMLDocument
                                                              and set value = this.m_MzQuantMLDocument <- value

            [<DefaultValue>] 
            val mutable m_CVParam : DbSet<Term>
            member public this.CVParam with get() = this.m_CVParam
                                                    and set value = this.m_CVParam <- value

            [<DefaultValue>] 
            val mutable m_AnalysisSummaryParam : DbSet<Term>
            member public this.AnalysisSummaryParam with get() = this.m_AnalysisSummaryParam
                                                                 and set value = this.m_AnalysisSummaryParam <- value

            [<DefaultValue>] 
            val mutable m_FeatureParam : DbSet<Term>
            member public this.FeatureParam with get() = this.m_FeatureParam
                                                         and set value = this.m_FeatureParam <- value

            [<DefaultValue>] 
            val mutable m_FeatureListParam : DbSet<Term>
            member public this.FeatureListParam with get() = this.m_FeatureListParam
                                                             and set value = this.m_FeatureListParam <- value

            [<DefaultValue>] 
            val mutable m_IdentificationFileParam : DbSet<Term>
            member public this.IdentificationFileParam with get() = this.m_IdentificationFileParam
                                                                    and set value = this.m_IdentificationFileParam <- value

            [<DefaultValue>] 
            val mutable m_AssayParam : DbSet<Term>
            member public this.AssayParam with get() = this.m_AssayParam
                                                       and set value = this.m_AssayParam <- value

            [<DefaultValue>] 
            val mutable m_PeptideConsensusParam : DbSet<Term>
            member public this.PeptideConsensusParam with get() = this.m_PeptideConsensusParam
                                                                  and set value = this.m_PeptideConsensusParam <- value

            [<DefaultValue>] 
            val mutable m_PeptideConsensusListParam : DbSet<Term>
            member public this.PeptideConsensusListParam with get() = this.m_PeptideConsensusListParam
                                                                      and set value = this.m_PeptideConsensusListParam <- value

            [<DefaultValue>] 
            val mutable m_ProcessingmethodParam : DbSet<Term>
            member public this.ProcessingmethodParam with get() = this.m_ProcessingmethodParam
                                                                  and set value = this.m_ProcessingmethodParam <- value

            [<DefaultValue>] 
            val mutable m_ProteinParam : DbSet<Term>
            member public this.ProteinParam with get() = this.m_ProteinParam
                                                         and set value = this.m_ProteinParam <- value

            [<DefaultValue>] 
            val mutable m_ProteinListParam : DbSet<Term>
            member public this.ProteinListParam with get() = this.m_ProteinListParam
                                                             and set value = this.m_ProteinListParam <- value

            [<DefaultValue>] 
            val mutable m_ProteinGroupParam : DbSet<Term>
            member public this.ProteinGroupParam with get() = this.m_ProteinGroupParam
                                                              and set value = this.m_ProteinGroupParam <- value

            [<DefaultValue>] 
            val mutable m_ProteinGroupListParam : DbSet<Term>
            member public this.ProteinGroupListParam with get() = this.m_ProteinGroupListParam
                                                                  and set value = this.m_ProteinGroupListParam <- value

            [<DefaultValue>] 
            val mutable m_RatioCalculationParam : DbSet<Term>
            member public this.RatioCalculationParam with get() = this.m_RatioCalculationParam
                                                                  and set value = this.m_RatioCalculationParam <- value

            [<DefaultValue>] 
            val mutable m_RawFileParam : DbSet<Term>
            member public this.RawFileParam with get() = this.m_RawFileParam
                                                         and set value = this.m_RawFileParam <- value

            [<DefaultValue>] 
            val mutable m_RawFileGroupParam : DbSet<Term>
            member public this.RawFileGroupParam with get() = this.m_RawFileGroupParam
                                                              and set value = this.m_RawFileGroupParam <- value

            [<DefaultValue>] 
            val mutable m_SearchDatabaseParam : DbSet<Term>
            member public this.SearchDatabaseParam with get() = this.m_SearchDatabaseParam
                                                                and set value = this.m_SearchDatabaseParam <- value

            [<DefaultValue>] 
            val mutable m_SmallMoleculeParam : DbSet<Term>
            member public this.SmallMoleculeParam with get() = this.m_SmallMoleculeParam
                                                               and set value = this.m_SmallMoleculeParam <- value

            [<DefaultValue>] 
            val mutable m_SmallMoleculeListParam : DbSet<Term>
            member public this.SmallMoleculeListParam with get() = this.m_SmallMoleculeListParam
                                                                   and set value = this.m_SmallMoleculeListParam <- value

            [<DefaultValue>] 
            val mutable m_AnalysisSoftwareParam : DbSet<Term>
            member public this.AnalysisSoftwareParam with get() = this.m_AnalysisSoftwareParam
                                                                  and set value = this.m_AnalysisSoftwareParam <- value

            [<DefaultValue>] 
            val mutable m_StudyVariableParam : DbSet<Term>
            member public this.StudyVariableParam with get() = this.m_StudyVariableParam
                                                               and set value = this.m_StudyVariableParam <- value

            [<DefaultValue>] 
            val mutable m_ProcessingMethodParam : DbSet<Term>
            member public this.ProcessingMethodParam with get() = this.m_ProcessingMethodParam
                                                                  and set value = this.m_ProcessingMethodParam <- value

            [<DefaultValue>] 
            val mutable m_OrganizationParam : DbSet<Term>
            member public this.OrganizationParam with get() = this.m_OrganizationParam
                                                              and set value = this.m_OrganizationParam <- value

            [<DefaultValue>] 
            val mutable m_PersonParam : DbSet<Term>
            member public this.PersonParam with get() = this.m_PersonParam
                                                        and set value = this.m_PersonParam <- value



