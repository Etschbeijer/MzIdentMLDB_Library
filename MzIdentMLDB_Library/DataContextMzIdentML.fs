namespace MzIdentMLDataBase

open System
open System.ComponentModel.DataAnnotations.Schema
open Microsoft.EntityFrameworkCore
open System.Collections.Generic


module DataModel =

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
    and [<AllowNullLiteral>]
        CVParamBase =
        abstract member ID         : string
        abstract member Value      : string
        abstract member Term       : Term
        abstract member Unit       : Term
        abstract member RowVersion : Nullable<DateTime>

    ///A single entry from an ontology or a controlled vocabulary.
    and [<AllowNullLiteral>] [<Table("CVParams")>]
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
    and [<AllowNullLiteral>] [<Table("OrganizationParams")>]
        OrganizationParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
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

    ///A single entry from an ontology or a controlled vocabulary.
    and [<AllowNullLiteral>] [<Table("PersonParams")>]
        PersonParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
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

    ///A single entry from an ontology or a controlled vocabulary.
    and [<AllowNullLiteral>] [<Table("SampleParams")>]
        SampleParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = SampleParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("ModificationParams")>]
        ModificationParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ModificationParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("PeptideParams")>]
        PeptideParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = PeptideParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("TranslationTableParams")>]
        TranslationTableParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = TranslationTableParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("MeasureParams")>]
        MeasureParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = MeasureParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("AmbiguousResidueParams")>]
        AmbiguousResidueParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = AmbiguousResidueParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("MassTableParams")>]
        MassTableParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = MassTableParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("IonTypeParams")>]
        IonTypeParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = IonTypeParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("SpecificityRuleParams")>]
        SpecificityRuleParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = SpecificityRuleParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("SearchModificationParams")>]
        SearchModificationParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = SearchModificationParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("EnzymeNameParams")>]
        EnzymeNameParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = EnzymeNameParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("IncludeParams")>]
        IncludeParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = IncludeParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("ExcludeParams")>]
        ExcludeParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ExcludeParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("AdditionalSearchParams")>]
        AdditionalSearchParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = AdditionalSearchParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("FragmentToleranceParams")>]
        FragmentToleranceParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = FragmentToleranceParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("ParentToleranceParams")>]
        ParentToleranceParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ParentToleranceParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("ThresholdParams")>]
        ThresholdParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ThresholdParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("SearchDatabaseParams")>]
        SearchDatabaseParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
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
    and [<AllowNullLiteral>] [<Table("DBSequenceParams")>]
        DBSequenceParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = DBSequenceParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("PeptideEvidenceParams")>]
        PeptideEvidenceParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = PeptideEvidenceParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("SpectrumIdentificationItemParams")>]
        SpectrumIdentificationItemParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = SpectrumIdentificationItemParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("SpectrumIdentificationResultParams")>]
        SpectrumIdentificationResultParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = SpectrumIdentificationResultParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("SpectrumIdentificationListParams")>]
        SpectrumIdentificationListParam(id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = SpectrumIdentificationListParam(null, null, null, null, Nullable())

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

    /////A single entry from an ontology or a controlled vocabulary.
    //and [<AllowNullLiteral>] [<Table("SpectrumIdentificationProtocolParams")>]
    //    SpectrumIdentificationProtocolParam(id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
    //        //inherit CVParam(null, null, null, null, Nullable())
    //        let mutable id'         = id
    //        let mutable value'      = value
    //        let mutable term'       = term
    //        let mutable unit'       = unit
    //        let mutable rowVersion' = rowVersion

    //        new() = SpectrumIdentificationProtocolParam(null, null, null, null, Nullable())

    //        [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
    //        member this.ID with get() = id' and set(value) = id' <- value
    //        member this.Value with get() = value' and set(value) = value' <- value
    //        member this.Term with get() = term' and set(value) = term' <- value
    //        member this.Unit with get() = unit' and set(value) = unit' <- value
    //        member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
    //        interface CVParamBase with
    //            member x.ID         = x.ID
    //            member x.Value      = x.Value
    //            member x.Term       = x.Term
    //            member x.Unit       = x.Unit
    //            member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    and [<AllowNullLiteral>] [<Table("AnalysisParams")>]
        AnalysisParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = AnalysisParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("SourceFileParams")>]
        SourceFileParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = SourceFileParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("ProteinDetectionHypothesisParams")>]
        ProteinDetectionHypothesisParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ProteinDetectionHypothesisParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("ProteinAmbiguityGroupParams")>]
        ProteinAmbiguityGroupParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ProteinAmbiguityGroupParam(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("ProteinDetectionListParams")>]
        ProteinDetectionListParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =
            //inherit CVParam(null, null, null, null, Nullable())
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ProteinDetectionListParam(null, null, null, null, Nullable())

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


    ///Organizations are entities like companies, universities, government agencies.
    and [<AllowNullLiteral>]
        Organization (id:string, name:string, parent:string,
                      details:List<OrganizationParam>, mzIdentMLDocument:MzIdentMLDocument,  
                      rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'                = id
            let mutable name'              = name 
            let mutable parent'            = parent
            let mutable details'           = details
            let mutable mzIdentMLDocument' = mzIdentMLDocument
            let mutable rowVersion'        = rowVersion

            new() = Organization(null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Parent with get() = parent' and set(value) = parent' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.MzIdentMLDocument with get() = mzIdentMLDocument' and set(value) = mzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A person's name and contact details.
    and [<AllowNullLiteral>]
        Person (id:string, name:string, firstName:string, midInitials:string, 
                lastName:string, organizations:List<Organization>, 
                details:List<PersonParam>, mzIdentMLDocument:MzIdentMLDocument,
                rowVersion:Nullable<DateTime>
                ) =
            let mutable id'                = id
            let mutable name'              = name
            let mutable firstName'         = firstName
            let mutable midInitials'       = midInitials
            let mutable lastName'          = lastName
            let mutable organizations'     = organizations
            let mutable details'           = details
            let mutable mzIdentMLDocument' = mzIdentMLDocument
            let mutable rowVersion'        = rowVersion

            new() = Person(null, null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.FirstName with get() = firstName' and set(value) = firstName' <- value
            member this.MidInitials with get() = midInitials' and set(value) = midInitials' <- value
            member this.LastName with get() = lastName' and set(value) = lastName' <- value
            member this.Organizations with get() = organizations' and set(value) = organizations' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.MzIdentMLDocument with get() = mzIdentMLDocument' and set(value) = mzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The software used for performing the analyses.
    and [<AllowNullLiteral>]
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

    ///The software used for performing the analyses.
    and [<AllowNullLiteral>]
        AnalysisSoftware (id:string, name:string, uri:string, version:string, customizations:string, contactRole:ContactRole, 
                          softwareName:CVParam, mzIdentMLDocument:MzIdentMLDocument, rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'                = id
            let mutable name'              = name
            let mutable uri'               = uri
            let mutable version'           = version
            let mutable customization'     = customizations
            let mutable contactRole'       = contactRole
            let mutable softwareName'      = softwareName
            let mutable mzIdentMLDocument' = mzIdentMLDocument
            let mutable rowVersion'        = rowVersion

            new() = AnalysisSoftware(null, null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.URI with get() = uri' and set(value) = uri' <- value
            member this.Version with get() = version' and set(value) = version' <- value
            member this.Customizations with get() = customization' and set(value) = customization' <- value
            member this.ContactRole with get() = contactRole' and set(value) = contactRole' <- value
            member this.SoftwareName with get() = softwareName' and set(value) = softwareName' <- value
            member this.MzIdentMLDocument with get() = mzIdentMLDocument' and set(value) = mzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///References to the individual component samples within a mixed parent sample.
    and [<AllowNullLiteral>]
        SubSample (id:string, sample:Sample, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable sample'     = sample
            let mutable rowVersion' = rowVersion

            new() = SubSample(null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Sample with get() = sample' and set(value) = sample' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A description of the sample analysed by mass spectrometry using CVParams or UserParams.
    and [<AllowNullLiteral>]
        Sample (id:string, name:string, contactRoles:List<ContactRole>, subSamples:List<SubSample>, 
                details:List<SampleParam>, mzIdentMLDocument:MzIdentMLDocument, rowVersion:Nullable<DateTime>
                ) =
            let mutable id'                = id
            let mutable name'              = name
            let mutable contactRoles'      = contactRoles
            let mutable subSamples'        = subSamples
            let mutable details'           = details
            let mutable mzIdentMLDocument' = mzIdentMLDocument
            let mutable rowVersion'        = rowVersion

            new() = Sample(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.ContactRoles with get() = contactRoles' and set(value) = contactRoles' <- value
            member this.SubSamples with get() = subSamples' and set(value) = subSamples' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.MzIdentMLDocument with get() = mzIdentMLDocument' and set(value) = mzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A molecule modification specification.
    and [<AllowNullLiteral>]
        Modification (id:string, residues:string, location:Nullable<int>, monoIsotopicMassDelta:Nullable<float>, 
                      avgMassDelta:Nullable<float>, details:List<ModificationParam>, rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'                    = id
            let mutable residues'              = residues
            let mutable location'              = location
            let mutable monoIsotopicMassDelta' = monoIsotopicMassDelta
            let mutable avgMassDelta'          = avgMassDelta
            let mutable details'               = details
            let mutable rowVersion'            = rowVersion

            new() = Modification(null, null, Nullable(), Nullable(), Nullable(), null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Residues with get() = residues' and set(value) = residues' <- value
            member this.Location with get() = location' and set(value) = location' <- value
            member this.MonoIsotopicMassDelta with get() = monoIsotopicMassDelta' and set(value) = monoIsotopicMassDelta' <- value
            member this.AvgMassDelta with get() = avgMassDelta' and set(value) = avgMassDelta' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A modification where one residue is substituted by another (amino acid change).
    and [<AllowNullLiteral>]
        SubstitutionModification (id:string, originalResidue:string, replacementResidue:string, location:Nullable<int>, 
                                  monoIsotopicMassDelta:Nullable<float>, avgMassDelta:Nullable<float>, rowVersion:Nullable<DateTime>
                                 ) =
            let mutable id'                    = id
            let mutable originalResidue'       = originalResidue
            let mutable replacementResidue'    = replacementResidue
            let mutable location'              = location
            let mutable monoIsotopicMassDelta' = monoIsotopicMassDelta
            let mutable avgMassDelta'          = avgMassDelta
            let mutable rowVersion'            = rowVersion

            new() = SubstitutionModification(null, null, null, Nullable(), Nullable(), Nullable(), Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.OriginalResidue with get() = originalResidue' and set(value) = originalResidue' <- value
            member this.ReplacementResidue with get() = replacementResidue' and set(value) = replacementResidue' <- value
            member this.Location with get() = location' and set(value) = location' <- value
            member this.MonoIsotopicMassDelta with get() = monoIsotopicMassDelta' and set(value) = monoIsotopicMassDelta' <- value
            member this.AvgMassDelta with get() = avgMassDelta' and set(value) = avgMassDelta' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///One (poly)peptide (a sequence with modifications).
    and [<AllowNullLiteral>]
        Peptide (id:string, name:string, peptideSequence:string, modifications:List<Modification>, substitutionModifications:List<SubstitutionModification>, 
                 details:List<PeptideParam>, mzIdentMLDocument:MzIdentMLDocument, rowVersion:Nullable<DateTime>
                ) =
            let mutable id'                        = id
            let mutable name'                      = name
            let mutable peptideSequence'           = peptideSequence
            let mutable modifications'             = modifications
            let mutable substitutionModifications' = substitutionModifications
            let mutable details'                   = details
            let mutable mzIdentMLDocument'         = mzIdentMLDocument
            let mutable rowVersion'                = rowVersion

            new() = Peptide(null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.PeptideSequence with get() = peptideSequence' and set(value) = peptideSequence' <- value
            member this.Modifications with get() = modifications' and set(value) = modifications' <- value
            member this.SubstitutionModifications with get() = substitutionModifications' and set(value) = substitutionModifications' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.MzIdentMLDocument with get() = mzIdentMLDocument' and set(value) = mzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///PeptideEvidence links a specific Peptide element to a specific position in a DBSequence.
    and [<AllowNullLiteral>]
        TranslationTable (id:string, name:string, details:List<TranslationTableParam>, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable name'       = name
            let mutable details'    = details
            let mutable rowVersion' = rowVersion

            new() = TranslationTable(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///References to CV terms defining the measures about product ions to be reported in SpectrumIdentificationItem.
    and [<AllowNullLiteral>]
        Measure (id:string, name:string, details:List<MeasureParam>, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable name'       = name
            let mutable details'    = details
            let mutable rowVersion' = rowVersion

            new() = Measure(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The specification of a single residue within the mass table.
    and [<AllowNullLiteral>]
        Residue (id:string, code:string, mass:Nullable<float>, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable code'       = code
            let mutable mass'       = mass
            let mutable rowVersion' = rowVersion

            new() = Residue(null, null, Nullable(), Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Code with get() = code' and set(value) = code' <- value
            member this.Mass with get() = mass' and set(value) = mass' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Ambiguous residues e.g. X can be specified by the Code attribute and a set of parameters for example giving the different masses that will be used in the search.
    and [<AllowNullLiteral>]
        AmbiguousResidue (id:string, code:string, details:List<AmbiguousResidueParam>, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable code'       = code
            let mutable details'    = details
            let mutable rowVersion' = rowVersion

            new() = AmbiguousResidue(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Code with get() = code' and set(value) = code' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The masses of residues used in the search.
    and [<AllowNullLiteral>]
        MassTable (id:string, name:string, msLevel:string, residues:List<Residue>, 
                   ambiguousResidues:List<AmbiguousResidue>, details:List<MassTableParam>, rowVersion:Nullable<DateTime>
                  ) =
            let mutable id'                = id
            let mutable name'              = name
            let mutable msLevel'           = msLevel
            let mutable residues'          = residues
            let mutable ambiguousResidues' = ambiguousResidues
            let mutable details'           = details
            let mutable rowVersion'        = rowVersion

            new() = MassTable(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.MSLevel with get() = msLevel' and set(value) = msLevel' <- value
            member this.Residues with get() = residues' and set(value) = residues' <- value
            member this.AmbiguousResidues with get() = ambiguousResidues' and set(value) = ambiguousResidues' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The values of this particular measure, corresponding to the index defined in ion and.
    and [<AllowNullLiteral>]
        Value (id:string, value:Nullable<float>, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable value'      = value
            let mutable rowVersion' = rowVersion

            new() = Value(null, Nullable(), Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///An array of values for a given and of measure and for a particular ion and, in parallel to the index of ions identified.
    and [<AllowNullLiteral>]
        FragmentArray (id:string, measure:Measure, values:Nullable<float>, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable measure'    = measure
            let mutable values'     = values
            let mutable rowVersion' = rowVersion

            new() = FragmentArray(null, null, Nullable(), Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Measure with get() = measure' and set(value) = measure' <- value
            member this.Values with get() = values' and set(value) = values' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The index of ions identified as integers, following standard notation for a-c, x-z e.g. if b3 b5 and b6 have been identified, the index would store "3 5 6".
    and [<AllowNullLiteral>]
        Index (id:string, index:Nullable<int>, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable index'      = index
            let mutable rowVersion' = rowVersion

            new() = Index(null, Nullable(), Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Index with get() = index' and set(value) = index' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///IonType defines the index of fragmentation ions being reported, importing a CV term for the and of ion e.g. b ion. Example: if b3 b7 b8 and b10 have been identified, the index attribute will contain 3 7 8 10.
    and [<AllowNullLiteral>]
        IonType (id:string, index:List<Index>, fragmentArrays:List<FragmentArray>, 
                 details:List<IonTypeParam>, rowVersion:Nullable<DateTime>
                ) =
            let mutable id'             = id
            let mutable index'          = index
            let mutable fragmentArrays' = fragmentArrays
            let mutable details'        = details
            let mutable rowVersion'     = rowVersion

            new() = IonType(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Index with get() = index' and set(value) = index' <- value
            member this.FragmentArrays with get() = fragmentArrays' and set(value) = fragmentArrays' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A data set containing spectra data (consisting of one or more spectra).
    and [<AllowNullLiteral>]
        SpectraData (id:string, name:string, location:string, externalFormatDocumentation:string, 
                     fileFormat:CVParam, spectrumIDFormat:CVParam, rowVersion:Nullable<DateTime>
                    ) =
            let mutable id'                          = id
            let mutable name'                        = name
            let mutable location'                    = location
            let mutable externalFormatDocumentation' = externalFormatDocumentation
            let mutable fileFormat'                  = fileFormat
            let mutable spectrumIDFormat'            = spectrumIDFormat
            let mutable rowVersion'                  = rowVersion

            new() = SpectraData(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Location with get() = location' and set(value) = location' <- value
            member this.ExternalFormatDocumentation with get() = externalFormatDocumentation' and set(value) = externalFormatDocumentation' <- value
            member this.FileFormat with get() = fileFormat' and set(value) = fileFormat' <- value
            member this.SpectrumIDFormat with get() = spectrumIDFormat' and set(value) = spectrumIDFormat' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    /////The specificity rules of the searched modification including for example the probability of a modification's presence or peptide or protein termini.
    //and [<AllowNullLiteral>]
    //    //Formerly Specificityrules
    //    SpecificityRule (id:string, details:List<SpecificityRuleParam>, rowVersion:Nullable<DateTime>) =
    //        let mutable id'         = id
    //        let mutable details'    = details
    //        let mutable rowVersion' = rowVersion
    //    //
    //        new() = SpecificityRule(null, null, Nullable())

    //        member this.ID with get() = id' and set(value) = id' <- value
    //        member this.Details with get() = details' and set(value) = details' <- value
    //        member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Specification of a search modification as parameter for a spectra search.
    and [<AllowNullLiteral>]
        SearchModification (id:string, fixedMod:Nullable<bool>, massDelta:Nullable<float>, residues:string, specificityRules:List<SpecificityRuleParam>, 
                            searchModificationParams:List<SearchModificationParam>, rowVersion:Nullable<DateTime>
                           ) =
            let mutable id'                       = id
            let mutable fixedMod'                 = fixedMod
            let mutable massDelta'                = massDelta
            let mutable residues'                 = residues
            let mutable specificityRules'         = specificityRules
            let mutable searchModificationParams' = searchModificationParams
            let mutable rowVersion'               = rowVersion

            new() = SearchModification(null, Nullable(), Nullable(), null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.FixedMod with get() = fixedMod' and set(value) = fixedMod' <- value
            member this.MassDelta with get() = massDelta' and set(value) = massDelta' <- value
            member this.Residues with get() = residues' and set(value) = residues' <- value
            member this.SpecificityRules with get() = specificityRules' and set(value) = specificityRules' <- value
            member this.Details with get() = searchModificationParams' and set(value) = searchModificationParams' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The details of an individual cleavage enzyme should be provided by giving a regular expression or a CV term if a "standard" enzyme cleavage has been performed.
    and [<AllowNullLiteral>]
        Enzyme (id:string, name:string, cTermGain:string, nTermGain:string, minDistance:Nullable<int>, missedCleavages:Nullable<int>, 
                semiSpecific:Nullable<bool>, siteRegexc:string, enzymeName:List<EnzymeNameParam>, rowVersion:Nullable<DateTime>
               ) =
            let mutable id'              = id
            let mutable name'            = name
            let mutable cTermGain'       = cTermGain
            let mutable nTermGain'       = nTermGain
            let mutable minDistance'     = minDistance
            let mutable missedCleavages' = missedCleavages
            let mutable semiSpecific'    = semiSpecific
            let mutable siteRegexc'      = siteRegexc
            let mutable enzymeName'      = enzymeName
            let mutable rowVersion'      = rowVersion

            new() = Enzyme(null, null, null, null, Nullable(), Nullable(), Nullable(), null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.CTermGain with get() = cTermGain' and set(value) = cTermGain' <- value
            member this.NTermGain with get() = nTermGain' and set(value) = nTermGain' <- value
            member this.MinDistance with get() = minDistance' and set(value) = minDistance' <- value
            member this.MissedCleavages with get() = missedCleavages' and set(value) = missedCleavages' <- value
            member this.SemiSpecific with get() = semiSpecific' and set(value) = semiSpecific' <- value
            member this.SiteRegexc with get() = siteRegexc' and set(value) = siteRegexc' <- value
            member this.EnzymeName with get() = enzymeName' and set(value) = enzymeName' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Filters applied to the search database. The filter MUST include at least one of Include and Exclude.
    and [<AllowNullLiteral>]
        Filter (id:string, filterType:CVParam, includes:List<IncludeParam>, excludes:List<ExcludeParam>, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable filterType' = filterType
            let mutable includes'   = includes
            let mutable excludes'   = excludes
            let mutable rowVersion' = rowVersion

            new() = Filter(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.FilterType with get() = filterType' and set(value) = filterType' <- value
            member this.Includes with get() = includes' and set(value) = includes' <- value
            member this.Excludes with get() = excludes' and set(value) = excludes' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The frames in which the nucleic acid sequence has been translated as a space separated list.
    and [<AllowNullLiteral>] 
        Frame (id:string, frame:Nullable<int>, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable frame'      = frame
            let mutable rowVersion' = rowVersion

            new() = Frame(null, Nullable(), Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Frame with get() = frame' and set(value) = frame' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The parameters and settings of a SpectrumIdentification analysis.
    and [<AllowNullLiteral>] 
        SpectrumIdentificationProtocol (id:string, name:string, analysisSoftware:AnalysisSoftware, searchType:CVParam,
                                        additionalSearchParams:List<AdditionalSearchParam>, modificationParams:List<SearchModification>,
                                        enzymes:List<Enzyme>, independent_Enzymes:Nullable<bool>, massTables:List<MassTable>,
                                        fragmentTolerance:List<FragmentToleranceParam>, parentTolerance:List<ParentToleranceParam>,
                                        threshold:List<ThresholdParam>, databaseFilters:List<Filter>, frames:List<Frame>,
                                        translationTables:List<TranslationTable>, mzIdentMLDocument:MzIdentMLDocument, 
                                        rowVersion:Nullable<DateTime>
                                       ) =
            let mutable id'                     = id
            let mutable name'                   = name
            //Formerly AnalysisSoftware_Ref
            let mutable analysisSoftware'       = analysisSoftware
            //
            let mutable searchType'             = searchType
            let mutable additionalSearchParams' = additionalSearchParams
            let mutable modificationParams'     = modificationParams
            //Formerly Enzymes
            let mutable enzymes'                = enzymes
            let mutable independent_Enzymes'    = independent_Enzymes
            //
            let mutable massTables'             = massTables
            let mutable fragmentTolerance'      = fragmentTolerance
            let mutable parentTolerance'        = parentTolerance
            let mutable threshold'              = threshold
            let mutable databaseFilters'        = databaseFilters
            //DatabaseTranlation
            let mutable frames'                 = frames
            let mutable translationTables'      = translationTables
            //
            let mutable mzIdentMLDocument'      = mzIdentMLDocument
            let mutable rowVersion'             = rowVersion

            new() = SpectrumIdentificationProtocol(null, null, null, null, null, null, null, Nullable(), null, null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.AnalysisSoftware with get() = analysisSoftware' and set(value) = analysisSoftware' <- value
            member this.SearchType with get() = searchType' and set(value) = searchType' <- value
            member this.AdditionalSearchParams with get() = additionalSearchParams' and set(value) = additionalSearchParams' <- value
            member this.ModificationParams with get() = modificationParams' and set(value) = modificationParams' <- value
            member this.Enzymes with get() = enzymes' and set(value) = enzymes' <- value
            member this.Independent_Enzymes with get() = independent_Enzymes' and set(value) = independent_Enzymes' <- value
            member this.MassTables with get() = massTables' and set(value) = massTables' <- value
            member this.FragmentTolerance with get() = fragmentTolerance' and set(value) = fragmentTolerance' <- value
            member this.ParentTolerance with get() = parentTolerance' and set(value) = parentTolerance' <- value
            member this.Threshold with get() = threshold' and set(value) = threshold' <- value
            member this.DatabaseFilters with get() = databaseFilters' and set(value) = databaseFilters' <- value
            member this.Frames with get() = frames' and set(value) = frames' <- value
            member this.TranslationTables with get() = translationTables' and set(value) = translationTables' <- value
            member this.MzIdentMLDocument with get() = mzIdentMLDocument' and set(value) = mzIdentMLDocument' <- value  
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A database for searching mass spectra.
    and [<AllowNullLiteral>]
        SearchDatabase (id:string, name:string, location:string, numDatabaseSequences:Nullable<int64>, numResidues:Nullable<int64>,
                        releaseDate:Nullable<DateTime>, version:string, externalFormatDocumentation:string, fileFormat:CVParam,
                        databaseName:CVParam, details:List<SearchDatabaseParam>, rowVersion:Nullable<DateTime>
                       ) =
            let mutable id'                          = id
            let mutable name'                        = name
            let mutable location'                    = location
            let mutable numDatabaseSequences'        = numDatabaseSequences
            let mutable numResidues'                 = numResidues
            let mutable releaseDate'                 = releaseDate
            let mutable version'                     = version
            let mutable externalFormatDocumentation' = externalFormatDocumentation
            let mutable fileFormat'                  = fileFormat
            let mutable databaseName'                = databaseName
            let mutable details'                     = details
            let mutable rowVersion'                  = rowVersion

            new() = SearchDatabase(null, null, null, Nullable(), Nullable(), Nullable(), null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Location with get() = location' and set(value) = location' <- value
            member this.NumDatabaseSequences with get() = numDatabaseSequences' and set(value) = numDatabaseSequences' <- value
            member this.NumResidues with get() = numResidues' and set(value) = numResidues' <- value
            member this.ReleaseDate with get() = releaseDate' and set(value) = releaseDate' <- value
            member this.Version with get() = version' and set(value) = version' <- value
            member this.ExternalFormatDocumentation with get() = externalFormatDocumentation' and set(value) = externalFormatDocumentation' <- value
            member this.FileFormat with get() = fileFormat' and set(value) = fileFormat' <- value
            member this.DatabaseName with get() = databaseName' and set(value) = databaseName' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A database sequence from the specified SearchDatabase (nucleic acid or amino acid).
    and [<AllowNullLiteral>]
        DBSequence (id:string, name:string, accession:string, searchDatabase:SearchDatabase, sequence:string, 
                    length:Nullable<int>, details:List<DBSequenceParam>, mzIdentMLDocument:MzIdentMLDocument, rowVersion:Nullable<DateTime>
                   ) =
            let mutable id'                = id
            let mutable name'              = name
            let mutable accession'         = accession
            let mutable searchDatabase'    = searchDatabase
            let mutable sequence'          = sequence
            let mutable length'            = length
            let mutable details'           = details
            let mutable mzIdentMLDocument' = mzIdentMLDocument
            let mutable rowVersion'        = rowVersion

            new() = DBSequence(null, null, null, null, null, Nullable(), null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Accession with get() = accession' and set(value) = accession' <- value
            member this.SearchDatabase with get() = searchDatabase' and set(value) = searchDatabase' <- value
            member this.Sequence with get() = sequence' and set(value) = sequence' <- value
            member this.Length with get() = length' and set(value) = length' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.MzIdentMLDocument with get() = mzIdentMLDocument' and set(value) = mzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///PeptideEvidence links a specific Peptide element to a specific position in a DBSequence.
    and [<AllowNullLiteral>]
        PeptideEvidence (id:string, name:string, dbSequence:DBSequence, peptide:Peptide, start:Nullable<int>, 
                         ends:Nullable<int>, pre:string, post:string, frame:Frame, isDecoy:Nullable<bool>, translationTable:TranslationTable, 
                         details:List<PeptideEvidenceParam>, mzIdentMLDocument:MzIdentMLDocument, rowVersion:Nullable<DateTime>
                        ) =
            let mutable id'                = id
            let mutable name'              = name
            //Formerly DBSequence_Ref
            let mutable dbSequence'        = dbSequence
            //
            //Formerly Peptide_Ref
            let mutable peptide'           = peptide
            //
            let mutable start'             = start
            let mutable ends'              = ends
            let mutable pre'               = pre
            let mutable post'              = post
            let mutable frame'             = frame
            let mutable isDecoy'           = isDecoy
            //Formerly TranslationTable_Ref
            let mutable translationTable'  = translationTable
            //
            let mutable details'           = details
            let mutable mzIdentMLDocument' = mzIdentMLDocument
            let mutable rowVersion'        = rowVersion

            new() = PeptideEvidence(null, null, null, null, Nullable(), Nullable(), null, null, null, Nullable(), null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.DBSequence with get() = dbSequence' and set(value) = dbSequence' <- value
            member this.Peptide with get() = peptide' and set(value) = peptide' <- value
            member this.Start with get() = start' and set(value) = start' <- value
            member this.End with get() = ends' and set(value) = ends' <- value
            member this.Pre with get() = pre' and set(value) = pre' <- value
            member this.Post with get() = post' and set(value) = post' <- value
            member this.Frame with get() = frame' and set(value) = frame' <- value
            member this.IsDecoy with get() = isDecoy' and set(value) = isDecoy' <- value
            member this.TranslationTable with get() = translationTable' and set(value) = translationTable' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.MzIdentMLDocument with get() = mzIdentMLDocument' and set(value) = mzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///An identification of a single (poly)peptide, resulting from querying an input spectra, along with the set of confidence values for that identification.
    and [<AllowNullLiteral>]
        SpectrumIdentificationItem (id:string, name:string, sample:Sample, massTable:MassTable, 
                                    passThreshold:Nullable<bool>, rank:Nullable<int>, peptideEvidences:List<PeptideEvidence>,
                                    fragmentations:List<IonType>, peptide:Peptide, chargeState:Nullable<int>, experimentalMassToCharge:Nullable<float>,
                                    calculatedMassToCharge:Nullable<float>, calculatedPI:Nullable<float>, details:List<SpectrumIdentificationItemParam>, 
                                    rowVersion:Nullable<DateTime>
                                   ) =

            let mutable id'                       = id
            let mutable name'                     = name
            let mutable sample'                   = sample
            let mutable massTable'                = massTable
            let mutable passThreshold'            = passThreshold
            let mutable rank'                     = rank
            let mutable peptideEvidences'         = peptideEvidences
            let mutable fragmentations'           = fragmentations
            let mutable peptide'                  = peptide
            let mutable chargeState'              = chargeState
            let mutable experimentalMassToCharge' = experimentalMassToCharge
            let mutable calculatedMassToCharge'   = calculatedMassToCharge
            let mutable calculatedPI'             = calculatedPI
            let mutable details'                  = details
            let mutable rowVersion'               = rowVersion

            new() = SpectrumIdentificationItem(null, null, null, null, Nullable(), Nullable(), null, null, null, Nullable(), Nullable(), Nullable(), Nullable(), null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Sample with get() = sample' and set(value) = sample' <- value
            member this.MassTable with get() = massTable' and set(value) = massTable' <- value
            member this.PassThreshold with get() = passThreshold' and set(value) = passThreshold' <- value
            member this.Rank with get() = rank' and set(value) = rank' <- value
            member this.PeptideEvidences with get() = peptideEvidences' and set(value) = peptideEvidences' <- value
            member this.Fragmentations with get() = fragmentations' and set(value) = fragmentations' <- value
            member this.Peptide with get() = peptide' and set(value) = peptide' <- value
            member this.ChargeState with get() = chargeState' and set(value) = chargeState' <- value
            member this.ExperimentalMassToCharge with get() = experimentalMassToCharge' and set(value) = experimentalMassToCharge' <- value
            member this.CalculatedMassToCharge with get() = calculatedMassToCharge' and set(value) = calculatedMassToCharge' <- value
            member this.CalculatedPI with get() = calculatedPI' and set(value) = calculatedPI' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///All identifications made from searching one spectrum.
    and [<AllowNullLiteral>]
        SpectrumIdentificationResult (id:string, name:string, spectraData:SpectraData, spectrumID:string, 
                                      spectrumIdentificationItem:List<SpectrumIdentificationItem>, details:List<SpectrumIdentificationResultParam>,
                                      rowVersion:Nullable<DateTime>
                                     ) =
            let mutable id'                         = id
            let mutable name'                       = name
            let mutable spectraData'                = spectraData
            let mutable spectrumID'                 = spectrumID
            let mutable spectrumIdentificationItem' = spectrumIdentificationItem
            let mutable details'                    = details
            let mutable rowVersion'                 = rowVersion

            new() = SpectrumIdentificationResult(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.SpectraData with get() = spectraData' and set(value) = spectraData' <- value
            member this.SpectrumID with get() = spectrumID' and set(value) = spectrumID' <- value
            member this.SpectrumIdentificationItem with get() = spectrumIdentificationItem' and set(value) = spectrumIdentificationItem' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Represents the set of all search results from SpectrumIdentification.
    and [<AllowNullLiteral>]
        SpectrumIdentificationList (id:string, name:string, numSequencesSearched:Nullable<int64>, fragmentationTables:List<Measure>,
                                    spectrumIdentificationResult:List<SpectrumIdentificationResult>, details:List<SpectrumIdentificationListParam>,
                                    rowVersion:Nullable<DateTime>
                                   ) =
            let mutable id'                           = id
            let mutable name'                         = name
            let mutable numSequencesSearched'         = numSequencesSearched
            let mutable fragmentationTables'          = fragmentationTables
            let mutable spectrumIdentificationResult' = spectrumIdentificationResult
            let mutable details'                      = details
            let mutable rowVersion'                   = rowVersion

            new() = SpectrumIdentificationList(null, null, Nullable(), null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.NumSequencesSearched with get() = numSequencesSearched' and set(value) = numSequencesSearched' <- value
            member this.FragmentationTables with get() = fragmentationTables' and set(value) = fragmentationTables' <- value
            member this.SpectrumIdentificationResult with get() = spectrumIdentificationResult' and set(value) = spectrumIdentificationResult' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///An Analysis which tries to identify peptides in input spectra, referencing the database searched, the input spectra, the output results and the protocol that is run.
    and [<AllowNullLiteral>]
        SpectrumIdentification (id:string, name:string, activityDate:Nullable<DateTime>, spectrumidentificationList:SpectrumIdentificationList,
                                spectrumIdentificationProtocol:SpectrumIdentificationProtocol, spectraData:List<SpectraData>,
                                searchDatabase:List<SearchDatabase>, mzIdentMLDocument:MzIdentMLDocument, rowVersion:Nullable<DateTime>
                               ) =
            let mutable id'                             = id
            let mutable name'                           = name
            let mutable activityDate'                   = activityDate
            let mutable spectrumidentificationList'     = spectrumidentificationList
            let mutable spectrumIdentificationProtocol' = spectrumIdentificationProtocol
            //SpectraData_Ref
            let mutable spectraData'                    = spectraData
            //
            //SearchDatabase_Ref
            let mutable searchDatabase'                 = searchDatabase
            //
            let mutable mzIdentMLDocument'              = mzIdentMLDocument
            let mutable rowVersion'                     = rowVersion

            new() = SpectrumIdentification(null, null, Nullable(), null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.ActivityDate with get() = activityDate' and set(value) = activityDate' <- value
            member this.SpectrumIdentificationList with get() = spectrumidentificationList' and set(value) = spectrumidentificationList' <- value
            member this.SpectrumIdentificationProtocol with get() = spectrumIdentificationProtocol' and set(value) = spectrumIdentificationProtocol' <- value
            member this.SpectraData with get() = spectraData' and set(value) = spectraData' <- value
            member this.SearchDatabase with get() = searchDatabase' and set(value) = searchDatabase' <- value
            member this.MzIdentMLDocument with get() = mzIdentMLDocument' and set(value) = mzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The parameters and settings of a SpectrumIdentification analysis.
    and [<AllowNullLiteral>] 
        ProteinDetectionProtocol (id:string, name:string, analysisSoftware:AnalysisSoftware, analysisParams:List<AnalysisParam>,
                                  threshold:List<ThresholdParam>, mzIdentMLDocument:MzIdentMLDocument, rowVersion:Nullable<DateTime>
                                 ) =
            let mutable id'                = id
            let mutable name'              = name
            let mutable analysisSoftware'  = analysisSoftware
            let mutable analysisParams'    = analysisParams
            let mutable threshold'         = threshold
            let mutable mzIdentMLDocument' = mzIdentMLDocument
            let mutable rowVersion'        = rowVersion

            new() = ProteinDetectionProtocol(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.AnalysisSoftware with get() = analysisSoftware' and set(value) = analysisSoftware' <- value
            member this.AnalysisParams with get() = analysisParams' and set(value) = analysisParams' <- value
            member this.Threshold with get() = threshold' and set(value) = threshold' <- value
            member this.MzIdentMLDocument with get() = mzIdentMLDocument' and set(value) = mzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A file from which this mzIdentML instance was created.
    and [<AllowNullLiteral>] 
        SourceFile (id:string, name:string, location:string, externalFormatDocumentation:string, fileFormat:CVParam,
                    details:List<SourceFileParam>, rowVersion:Nullable<DateTime>
                   ) =
            let mutable id'                          = id
            let mutable name'                        = name
            let mutable location'                    = location
            let mutable externalFormatDocumentation' = externalFormatDocumentation
            let mutable fileFormat'                  = fileFormat
            let mutable details'                     = details
            let mutable rowVersion'                  = rowVersion

            new() = SourceFile(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Location with get() = location' and set(value) = location' <- value
            member this.ExternalFormatDocumentation with get() = externalFormatDocumentation' and set(value) = externalFormatDocumentation' <- value
            member this.FileFormat with get() = fileFormat' and set(value) = fileFormat' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The inputs to the analyses including the databases searched, the spectral data and the source file converted to mzIdentML.
    and [<AllowNullLiteral>]
        Inputs (id:string, sourceFiles:List<SourceFile>, searchDatabases:List<SearchDatabase>,
                spectraData:List<SpectraData>, mzIdentMLDocument:MzIdentMLDocument, rowVersion:Nullable<DateTime>
               ) =
            let mutable id'                = id
            let mutable sourceFiles'       = sourceFiles
            let mutable searchDatabases'   = searchDatabases
            let mutable spectraData'       = spectraData
            let mutable mzIdentMLDocument' = mzIdentMLDocument
            let mutable rowVersion'        = rowVersion

            new() = Inputs(null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.SourceFiles with get() = sourceFiles' and set(value) = sourceFiles' <- value
            member this.SearchDatabases with get() = searchDatabases' and set(value) = searchDatabases' <- value
            member this.SpectraData with get() = spectraData' and set(value) = spectraData' <- value
            member this.MzIdentMLDocument with get() = mzIdentMLDocument' and set(value) = mzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Peptide evidence on which this ProteinHypothesis is based by reference to a PeptideEvidence element.
    and [<AllowNullLiteral>]
        PeptideHypothesis (id:string, peptideEvidence:PeptideEvidence, spectrumIdentificationItems:List<SpectrumIdentificationItem>,
                           rowVersion:Nullable<DateTime>
                          ) =
            let mutable id'                          = id
            let mutable peptideEvidence'             = peptideEvidence
            let mutable spectrumIdentificationItems' = spectrumIdentificationItems
            let mutable rowVersion'                  = rowVersion

            new() = PeptideHypothesis(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.PeptideEvidence with get() = peptideEvidence' and set(value) = peptideEvidence' <- value
            member this.SpectrumIdentificationItems with get() = spectrumIdentificationItems' and set(value) = spectrumIdentificationItems' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A single result of the ProteinDetection analysis (i.e. a protein).
    and [<AllowNullLiteral>] 
        ProteinDetectionHypothesis (id:string, name:string, passThreshold:Nullable<bool>, dbSequence:DBSequence,
                                    peptideHypothesis:List<PeptideHypothesis>, details:List<ProteinDetectionHypothesisParam>,
                                    mzIdentMLDocument:MzIdentMLDocument, rowVersion:Nullable<DateTime>
                                   ) =
            let mutable id'                = id
            let mutable name'              = name
            let mutable passThreshold'     = passThreshold
            let mutable dbSequence'        = dbSequence
            let mutable peptideHypothesis' = peptideHypothesis
            let mutable details'           = details
            let mutable mzIdentMLDocument' = mzIdentMLDocument
            let mutable rowVersion'        = rowVersion

            new() = ProteinDetectionHypothesis(null, null, Nullable(), null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.PassThreshold with get() = passThreshold' and set(value) = passThreshold' <- value
            member this.DBSequence with get() = dbSequence' and set(value) = dbSequence' <- value
            member this.PeptideHypothesis with get() = peptideHypothesis' and set(value) = peptideHypothesis' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.MzIdentMLDocument with get() = mzIdentMLDocument' and set(value) = mzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A set of logically related results from a protein detection, for example to represent conflicting assignments of peptides to proteins.
    and [<AllowNullLiteral>] 
        ProteinAmbiguityGroup (id:string, name:string, proteinDetecionHypothesis:List<ProteinDetectionHypothesis>,
                               details:List<ProteinAmbiguityGroupParam>, rowVersion:Nullable<DateTime>
                              ) =
            let mutable id'                        = id
            let mutable name'                      = name
            let mutable proteinDetecionHypothesis' = proteinDetecionHypothesis
            let mutable details'                   = details
            let mutable rowVersion'                = rowVersion

            new() = ProteinAmbiguityGroup(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.ProteinDetectionHypothesis with get() = proteinDetecionHypothesis' and set(value) = proteinDetecionHypothesis' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The protein list resulting from a protein detection process.
    and [<AllowNullLiteral>] 
        ProteinDetectionList (id:string, name:string, proteinAmbiguityGroups:List<ProteinAmbiguityGroup>,
                              details:List<ProteinDetectionListParam>, rowVersion:Nullable<DateTime>
                             ) =
            let mutable id'                     = id
            let mutable name'                   = name
            let mutable proteinAmbiguityGroups' = proteinAmbiguityGroups
            let mutable details'                = details
            let mutable rowVersion'             = rowVersion

            new() = ProteinDetectionList(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.ProteinAmbiguityGroups with get() = proteinAmbiguityGroups' and set(value) = proteinAmbiguityGroups' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Data sets generated by the analyses, including peptide and protein lists.
    and [<AllowNullLiteral>] 
        AnalysisData (id:string, spectrumIdentificationList:List<SpectrumIdentificationList>, proteinDetectionList:ProteinDetectionList, 
                      mzIdentMLDocument:MzIdentMLDocument, rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'                         = id
            let mutable spectrumIdentificationList' = spectrumIdentificationList
            let mutable proteinDetectionList'       = proteinDetectionList
            let mutable mzIdentMLDocument'          = mzIdentMLDocument
            let mutable rowVersion'                 = rowVersion

            new() = AnalysisData(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.SpectrumIdentificationList with get() = spectrumIdentificationList' and set(value) = spectrumIdentificationList' <- value
            member this.ProteinDetectionList with get() = proteinDetectionList' and set(value) = proteinDetectionList' <- value
            member this.MzIdentMLDocument with get() = mzIdentMLDocument' and set(value) = mzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///An Analysis which assembles a set of peptides (e.g. from a spectra search analysis) to proteins. 
    and [<AllowNullLiteral>] 
        ProteinDetection (id:string, name:string, activityDate:Nullable<DateTime>, proteinDetectionList:ProteinDetectionList,
                          proteinDetectionProtocol:ProteinDetectionProtocol, spectrumIdentificationLists:List<SpectrumIdentificationList>,
                          mzIdentMLDocument:MzIdentMLDocument, rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'                          = id
            let mutable name'                        = name
            let mutable activityDate'                = activityDate
            let mutable proteinDetectionList'        = proteinDetectionList
            let mutable proteinDetectionProtocol'    = proteinDetectionProtocol
            let mutable spectrumIdentificationLists' = spectrumIdentificationLists
            let mutable mzIdentMLDocument'           = mzIdentMLDocument
            let mutable rowVersion'                  = rowVersion

            new() = ProteinDetection(null, null, Nullable(), null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.ActivityDate with get() = activityDate' and set(value) = activityDate' <- value
            member this.ProteinDetectionList with get() = proteinDetectionList' and set(value) = proteinDetectionList' <- value
            member this.ProteinDetectionProtocol with get() = proteinDetectionProtocol' and set(value) = proteinDetectionProtocol' <- value
            member this.SpectrumIdentificationLists with get() = spectrumIdentificationLists' and set(value) = spectrumIdentificationLists' <- value
            member this.MzIdentMLDocument with get() = mzIdentMLDocument' and set(value) = mzIdentMLDocument' <- value
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

    ///The Provider of the mzIdentML record in terms of the contact and software.
    and [<AllowNullLiteral>] 
        Provider (id:string, name:string, analysisSoftware:AnalysisSoftware, contactRole:ContactRole, 
                  mzIdentML:MzIdentMLDocument, rowVersion:Nullable<DateTime>
                 ) =
            let mutable id'               = id
            let mutable name'             = name
            let mutable analysisSoftware' = analysisSoftware
            let mutable contactRole'      = contactRole
            let mutable mzIdentML'        = mzIdentML
            let mutable rowVersion'       = rowVersion

            new() = Provider(null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.AnalysisSoftware with get() = analysisSoftware' and set(value) = analysisSoftware' <- value
            member this.ContactRole with get() = contactRole' and set(value) = contactRole' <- value
            member this.MzIdentMLDocument with get() = mzIdentML' and set(value) = mzIdentML' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The upper-most hierarchy level of mzIdentML with sub-containers for example describing software, protocols and search results.
    and [<AllowNullLiteral>]
        MzIdentMLDocument(
                          id:string,
                          name:string, 
                          version:string,
                          analysisSoftwares:List<AnalysisSoftware>,
                          provider:List<Provider>,
                          persons:List<Person>, 
                          organizations:List<Organization>, 
                          samples:List<Sample>, 
                          dbSequences:List<DBSequence>, 
                          peptides:List<Peptide>,
                          peptideEvidences:List<PeptideEvidence>, 
                          spectrumIdentification:List<SpectrumIdentification>, 
                          proteinDetection:List<ProteinDetection>, 
                          spectrumIdentificationProtocol:List<SpectrumIdentificationProtocol>, 
                          proteinDetectionProtocol:List<ProteinDetectionProtocol>,
                          inputs:List<Inputs>,
                          analysisData:List<AnalysisData>,
                          biblioGraphicReferences:List<BiblioGraphicReference>,
                          rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'                             = id
            let mutable name'                           = name
            let mutable version'                        = version
            let mutable analysisSoftwares'              = analysisSoftwares
            let mutable provider'                       = provider
            let mutable persons'                        = persons
            let mutable organizations'                  = organizations
            let mutable samples'                        = samples
            let mutable dbSequences'                    = dbSequences
            let mutable peptides'                       = peptides
            let mutable peptideEvidences'               = peptideEvidences
            let mutable spectrumIdentification'         = spectrumIdentification
            let mutable proteinDetection'               = proteinDetection
            let mutable spectrumIdentificationProtocol' = spectrumIdentificationProtocol
            let mutable proteinDetectionProtocol'       = proteinDetectionProtocol
            let mutable inputs'                         = inputs
            let mutable analysisData'                   = analysisData
            let mutable biblioGraphicReferences'        = biblioGraphicReferences
            let mutable rowVersion'                     = rowVersion

            new() = MzIdentMLDocument(null, null, null, null, null, null, null, null, null, null, null, null, null, null,null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Version with get() = version' and set(value) = version' <- value
            //Formerly AnalysisSoftwareList
            member this. AnalysisSoftwares with get() = analysisSoftwares' and set(value) = analysisSoftwares' <- value
            //
            member this.Provider with get() = provider' and set(value) = provider' <- value
            //Formerly AuditCollection
            member this. Persons with get() = persons and set(value) = persons' <- value
            member this. Organizations with get() = organizations' and set(value) = organizations' <- value
            //
            //Formerly AnalysisSampleCollection
            member this. Samples with get() = samples' and set(value) = samples' <- value
            //
            //Formerly SequenceCollection
            member this. DBSequences with get() = dbSequences' and set(value) = dbSequences' <- value
            member this. Peptides with get() = peptides' and set(value) = peptides' <- value
            member this. PeptideEvidences with get() = peptideEvidences' and set(value) = peptideEvidences' <- value
            //
            //AnalysisCollection
            member this.SpectrumIdentification with get() = spectrumIdentification' and set(value) = spectrumIdentification' <- value
            member this.ProteinDetection with get() = proteinDetection' and set(value) = proteinDetection' <- value
            //
            //AnalysisProtocolCollection
            member this.SpectrumIdentificationProtocol with get() = spectrumIdentificationProtocol' and set(value) = spectrumIdentificationProtocol' <- value
            member this.ProteinDetectionProtocol with get() = proteinDetectionProtocol' and set(value) = proteinDetectionProtocol' <- value
            //
            //DataCollection
            member this.Inputs with get() = inputs' and set(value) = inputs' <- value
            member this.AnalysisData with get() = analysisData' and set(value) = analysisData' <- value
            //
            member this.BiblioGraphicReferences with get() = biblioGraphicReferences' and set(value) = biblioGraphicReferences' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
    

 

    type MzIdentML =
     
            inherit DbContext

            new(options : DbContextOptions<MzIdentML>) = {inherit DbContext(options)}

            [<DefaultValue>] 
            val mutable m_term : DbSet<Term>
            member public this.Term with get() = this.m_term
                                                    and set value = this.m_term <- value
  
            [<DefaultValue>] 
            val mutable m_Ontology : DbSet<Ontology>
            member public this.Ontology with get() = this.m_Ontology
                                                     and set value = this.m_Ontology <- value 

            [<DefaultValue>] 
            val mutable m_cvParam : DbSet<CVParam>
            member public this.CVParam with get() = this.m_cvParam
                                                    and set value = this.m_cvParam <- value

            [<DefaultValue>] 
            val mutable m_ContactRole : DbSet<ContactRole>
            member public this.ContactRole with get() = this.m_ContactRole
                                                        and set value = this.m_ContactRole <- value

            [<DefaultValue>] 
            val mutable m_AnalysisSoftware : DbSet<AnalysisSoftware>
            member public this.AnalysisSoftware with get() = this.m_AnalysisSoftware
                                                             and set value = this.m_AnalysisSoftware <- value

            [<DefaultValue>] 
            val mutable m_Person : DbSet<Person>
            member public this.Person with get() = this.m_Person
                                                    and set value = this.m_Person <- value

            [<DefaultValue>] 
            val mutable m_Organization : DbSet<Organization>
            member public this.Organization with get() = this.m_Organization
                                                         and set value = this.m_Organization <- value

            [<DefaultValue>] 
            val mutable m_DBSequence : DbSet<DBSequence>
            member public this.DBSequence with get() = this.m_DBSequence
                                                       and set value = this.m_DBSequence <- value     

            [<DefaultValue>] 
            val mutable m_SubSample : DbSet<SubSample>
            member public this.SubSample with get() = this.m_SubSample
                                                      and set value = this.m_SubSample <- value  

            [<DefaultValue>] 
            val mutable m_Sample : DbSet<Sample>
            member public this.Sample with get() = this.m_Sample
                                                   and set value = this.m_Sample <- value   

            [<DefaultValue>] 
            val mutable m_Modification : DbSet<Modification>
            member public this.Modification with get() = this.m_Modification
                                                         and set value = this.m_Modification <- value  

            [<DefaultValue>] 
            val mutable m_SubstitutionModification : DbSet<SubstitutionModification>
            member public this.SubstitutionModification with get() = this.m_SubstitutionModification
                                                                     and set value = this.m_SubstitutionModification <- value

            [<DefaultValue>] 
            val mutable m_Peptide : DbSet<Peptide>
            member public this.Peptide with get() = this.m_Peptide
                                                    and set value = this.m_Peptide <- value

            [<DefaultValue>] 
            val mutable m_TranslationTable : DbSet<TranslationTable>
            member public this.TranslationTable with get() = this.m_TranslationTable
                                                             and set value = this.m_TranslationTable <- value

            [<DefaultValue>] 
            val mutable m_PeptideEvidence : DbSet<PeptideEvidence>
            member public this.PeptideEvidence with get() = this.m_PeptideEvidence
                                                            and set value = this.m_PeptideEvidence <- value

            [<DefaultValue>] 
            val mutable m_Measure : DbSet<Measure>
            member public this.Measure with get() = this.m_Measure
                                                    and set value = this.m_Measure <- value

            [<DefaultValue>] 
            val mutable m_Residue : DbSet<Residue>
            member public this.Residue with get() = this.m_Residue
                                                    and set value = this.m_Residue <- value

            [<DefaultValue>] 
            val mutable m_AmbiguousResidue : DbSet<AmbiguousResidue>
            member public this.AmbiguousResidue with get() = this.m_AmbiguousResidue
                                                             and set value = this.m_AmbiguousResidue <- value

            [<DefaultValue>] 
            val mutable m_MassTable : DbSet<MassTable>
            member public this.MassTable with get() = this.m_MassTable
                                                      and set value = this.m_MassTable <- value

            [<DefaultValue>] 
            val mutable m_Value : DbSet<Value>
            member public this.Value with get() = this.m_Value
                                                  and set value = this.m_Value <- value

            [<DefaultValue>] 
            val mutable m_FragmentArray : DbSet<FragmentArray>
            member public this.FragmentArray with get() = this.m_FragmentArray
                                                          and set value = this.m_FragmentArray <- value

            [<DefaultValue>] 
            val mutable m_Index : DbSet<Index>
            member public this.Index with get() = this.m_Index
                                                  and set value = this.m_Index <- value

            [<DefaultValue>] 
            val mutable m_IonType : DbSet<IonType>
            member public this.IonType with get() = this.m_IonType
                                                    and set value = this.m_IonType <- value

            [<DefaultValue>] 
            val mutable m_SpectrumIdentificationItem : DbSet<SpectrumIdentificationItem>
            member public this.SpectrumIdentificationItem with get() = this.m_SpectrumIdentificationItem
                                                                       and set value = this.m_SpectrumIdentificationItem <- value

            [<DefaultValue>] 
            val mutable m_SpectraData : DbSet<SpectraData>
            member public this.SpectraData with get() = this.m_SpectraData
                                                        and set value = this.m_SpectraData <- value

            [<DefaultValue>] 
            val mutable m_SpectrumIdentificationResult : DbSet<SpectrumIdentificationResult>
            member public this.SpectrumIdentificationResult with get() = this.m_SpectrumIdentificationResult
                                                                         and set value = this.m_SpectrumIdentificationResult <- value

            [<DefaultValue>] 
            val mutable m_SpectrumIdentificationList : DbSet<SpectrumIdentificationList>
            member public this.SpectrumIdentificationList with get() = this.m_SpectrumIdentificationList
                                                                       and set value = this.m_SpectrumIdentificationList <- value

            //[<DefaultValue>] 
            //val mutable m_SpecificityRule : DbSet<SpecificityRule>
            //member public this.SpecificityRule with get() = this.m_SpecificityRule
            //                                                and set value = this.m_SpecificityRule <- value

            [<DefaultValue>] 
            val mutable m_SearchModification : DbSet<SearchModification>
            member public this.SearchModification with get() = this.m_SearchModification
                                                               and set value = this.m_SearchModification <- value

            [<DefaultValue>] 
            val mutable m_Enzyme : DbSet<Enzyme>
            member public this.Enzyme with get() = this.m_Enzyme
                                                   and set value = this.m_Enzyme <- value

            [<DefaultValue>] 
            val mutable m_Filter : DbSet<Filter>
            member public this.Filter with get() = this.m_Filter
                                                   and set value = this.m_Filter <- value

            [<DefaultValue>] 
            val mutable m_Frame : DbSet<Frame>
            member public this.Frame with get() = this.m_Frame
                                                  and set value = this.m_Frame <- value

            //[<DefaultValue>] 
            //val mutable m_SpectrumIdentificationProtocolParam : DbSet<SpectrumIdentificationProtocolParam>
            //member public this.SpectrumIdentificationProtocolParam with get() = this.m_SpectrumIdentificationProtocolParam
            //                                                                and set value = this.m_SpectrumIdentificationProtocolParam <- value
            
            [<DefaultValue>] 
            val mutable m_SpectrumIdentificationProtocol : DbSet<SpectrumIdentificationProtocol>
            member public this.SpectrumIdentificationProtocol with get() = this.m_SpectrumIdentificationProtocol
                                                                           and set value = this.m_SpectrumIdentificationProtocol <- value

            [<DefaultValue>] 
            val mutable m_SearchDatabase : DbSet<SearchDatabase>
            member public this.SearchDatabase with get() = this.m_SearchDatabase
                                                           and set value = this.m_SearchDatabase <- value

            [<DefaultValue>] 
            val mutable m_SpectrumIdentification : DbSet<SpectrumIdentification>
            member public this.SpectrumIdentification with get() = this.m_SpectrumIdentification
                                                                   and set value = this.m_SpectrumIdentification <- value

            [<DefaultValue>] 
            val mutable m_ProteinDetectionProtocol : DbSet<ProteinDetectionProtocol>
            member public this.ProteinDetectionProtocol with get() = this.m_ProteinDetectionProtocol
                                                                     and set value = this.m_ProteinDetectionProtocol <- value

            [<DefaultValue>] 
            val mutable m_SourceFile : DbSet<SourceFile>
            member public this.SourceFile with get() = this.m_SourceFile
                                                       and set value = this.m_SourceFile <- value

            [<DefaultValue>] 
            val mutable m_Inputs : DbSet<Inputs>
            member public this.Inputs with get() = this.m_Inputs
                                                   and set value = this.m_Inputs <- value

            [<DefaultValue>] 
            val mutable m_PeptideHypothesis : DbSet<PeptideHypothesis>
            member public this.PeptideHypothesis with get() = this.m_PeptideHypothesis
                                                              and set value = this.m_PeptideHypothesis <- value

            [<DefaultValue>]
            val mutable m_ProteinDetectionHypothesis : DbSet<ProteinDetectionHypothesis>
            member public this.ProteinDetectionHypothesis with get() = this.m_ProteinDetectionHypothesis
                                                                       and set value = this.m_ProteinDetectionHypothesis <- value

            [<DefaultValue>]
            val mutable m_ProteinAmbiguityGroup : DbSet<ProteinAmbiguityGroup>
            member public this.ProteinAmbiguityGroup with get() = this.m_ProteinAmbiguityGroup
                                                                  and set value = this.m_ProteinAmbiguityGroup <- value

            [<DefaultValue>]
            val mutable m_ProteinDetectionList : DbSet<ProteinDetectionList>
            member public this.ProteinDetectionList with get() = this.m_ProteinDetectionList
                                                                 and set value = this.m_ProteinDetectionList <- value

            [<DefaultValue>]
            val mutable m_AnalysisData : DbSet<AnalysisData>
            member public this.AnalysisData with get() = this.m_AnalysisData
                                                            and set value = this.m_AnalysisData <- value

            [<DefaultValue>]
            val mutable m_ProteinDetection : DbSet<ProteinDetection>
            member public this.ProteinDetection with get() = this.m_ProteinDetection
                                                             and set value = this.m_ProteinDetection <- value

            [<DefaultValue>]
            val mutable m_Provider : DbSet<Provider>
            member public this.Provider with get() = this.m_Provider
                                                     and set value = this.m_Provider <- value

            [<DefaultValue>]
            val mutable m_BiblioGraphicReference : DbSet<BiblioGraphicReference>
            member public this.BiblioGraphicReference with get() = this.m_BiblioGraphicReference
                                                                   and set value = this.m_BiblioGraphicReference <- value

            [<DefaultValue>]
            val mutable m_MzIdentMLDocument : DbSet<MzIdentMLDocument>
            member public this.MzIdentMLDocument with get() = this.m_MzIdentMLDocument
                                                              and set value = this.m_MzIdentMLDocument <- value

            [<DefaultValue>] 
            val mutable m_OrganizationParam : DbSet<OrganizationParam>
            member public this.OrganizationParam with get() = this.m_OrganizationParam
                                                              and set value = this.m_OrganizationParam <- value

            [<DefaultValue>] 
            val mutable m_PersonParam : DbSet<PersonParam>
            member public this.PersonParam with get() = this.m_PersonParam
                                                        and set value = this.m_PersonParam <- value

            [<DefaultValue>] 
            val mutable m_SampleParam : DbSet<SampleParam>
            member public this.SampleParam with get() = this.m_SampleParam
                                                        and set value = this.m_SampleParam <- value

            [<DefaultValue>] 
            val mutable m_ModificationParam : DbSet<ModificationParam>
            member public this.ModificationParam with get() = this.m_ModificationParam
                                                              and set value = this.m_ModificationParam <- value

            [<DefaultValue>] 
            val mutable m_PeptideParam : DbSet<PeptideParam>
            member public this.PeptideParam with get() = this.m_PeptideParam
                                                         and set value = this.m_PeptideParam <- value

            [<DefaultValue>] 
            val mutable m_TranslationTableParam : DbSet<TranslationTableParam>
            member public this.TranslationTableParam with get() = this.m_TranslationTableParam
                                                                  and set value = this.m_TranslationTableParam <- value

            [<DefaultValue>] 
            val mutable m_MeasureParam : DbSet<MeasureParam>
            member public this.MeasureParam with get() = this.m_MeasureParam
                                                         and set value = this.m_MeasureParam <- value

            [<DefaultValue>] 
            val mutable m_AmbiguousResidueParam : DbSet<AmbiguousResidueParam>
            member public this.AmbiguousResidueParam with get() = this.m_AmbiguousResidueParam
                                                                  and set value = this.m_AmbiguousResidueParam <- value

            [<DefaultValue>] 
            val mutable m_MassTableParam : DbSet<MassTableParam>
            member public this.MassTableParam with get() = this.m_MassTableParam
                                                           and set value = this.m_MassTableParam <- value

            [<DefaultValue>] 
            val mutable m_IonTypeParam : DbSet<IonTypeParam>
            member public this.IonTypeParam with get() = this.m_IonTypeParam
                                                         and set value = this.m_IonTypeParam <- value

            [<DefaultValue>] 
            val mutable m_SpecificityRuleParam : DbSet<SpecificityRuleParam>
            member public this.SpecificityRuleParam with get() = this.m_SpecificityRuleParam
                                                                 and set value = this.m_SpecificityRuleParam <- value

            [<DefaultValue>] 
            val mutable m_SearchModificationParam : DbSet<SearchModificationParam>
            member public this.SearchModificationParam with get() = this.m_SearchModificationParam
                                                                    and set value = this.m_SearchModificationParam <- value

            [<DefaultValue>] 
            val mutable m_EnzymeNameParam : DbSet<EnzymeNameParam>
            member public this.EnzymeNameParam with get() = this.m_EnzymeNameParam
                                                            and set value = this.m_EnzymeNameParam <- value

            [<DefaultValue>] 
            val mutable m_IncludeParam : DbSet<IncludeParam>
            member public this.IncludeParam with get() = this.m_IncludeParam
                                                         and set value = this.m_IncludeParam <- value

            [<DefaultValue>] 
            val mutable m_ExcludeParam : DbSet<ExcludeParam>
            member public this.ExcludeParam with get() = this.m_ExcludeParam
                                                         and set value = this.m_ExcludeParam <- value

            [<DefaultValue>] 
            val mutable m_AdditionalSearchParam : DbSet<AdditionalSearchParam>
            member public this.AdditionalSearchParam with get() = this.m_AdditionalSearchParam
                                                                  and set value = this.m_AdditionalSearchParam <- value

            [<DefaultValue>] 
            val mutable m_FragmentToleranceParam : DbSet<FragmentToleranceParam>
            member public this.FragmentToleranceParam with get() = this.m_FragmentToleranceParam
                                                                   and set value = this.m_FragmentToleranceParam <- value

            [<DefaultValue>] 
            val mutable m_ParentToleranceParam : DbSet<ParentToleranceParam>
            member public this.ParentToleranceParam with get() = this.m_ParentToleranceParam
                                                                 and set value = this.m_ParentToleranceParam <- value

            [<DefaultValue>] 
            val mutable m_ThresholdParam : DbSet<ThresholdParam>
            member public this.ThresholdParam with get() = this.m_ThresholdParam
                                                           and set value = this.m_ThresholdParam <- value

            [<DefaultValue>] 
            val mutable m_SearchDatabaseParam : DbSet<SearchDatabaseParam>
            member public this.SearchDatabaseParam with get() = this.m_SearchDatabaseParam
                                                                and set value = this.m_SearchDatabaseParam <- value

            [<DefaultValue>] 
            val mutable m_DBSequenceParam : DbSet<DBSequenceParam>
            member public this.DBSequenceParam with get() = this.m_DBSequenceParam
                                                            and set value = this.m_DBSequenceParam <- value

            [<DefaultValue>] 
            val mutable m_PeptideEvidenceParam : DbSet<PeptideEvidenceParam>
            member public this.PeptideEvidenceParam with get() = this.m_PeptideEvidenceParam
                                                                 and set value = this.m_PeptideEvidenceParam <- value

            [<DefaultValue>] 
            val mutable m_SpectrumIdentificationItemParam : DbSet<SpectrumIdentificationItemParam>
            member public this.SpectrumIdentificationItemParam with get() = this.m_SpectrumIdentificationItemParam
                                                                            and set value = this.m_SpectrumIdentificationItemParam <- value

            [<DefaultValue>] 
            val mutable m_SpectrumIdentificationResultParam : DbSet<SpectrumIdentificationResultParam>
            member public this.SpectrumIdentificationResultParam with get() = this.m_SpectrumIdentificationResultParam
                                                                              and set value = this.m_SpectrumIdentificationResultParam <- value

            [<DefaultValue>] 
            val mutable m_SpectrumIdentificationListParam : DbSet<SpectrumIdentificationListParam>
            member public this.SpectrumIdentificationListParam with get() = this.m_SpectrumIdentificationListParam
                                                                            and set value = this.m_SpectrumIdentificationListParam <- value

            [<DefaultValue>] 
            val mutable m_AnalysisParam : DbSet<AnalysisParam>
            member public this.AnalysisParam with get() = this.m_AnalysisParam
                                                          and set value = this.m_AnalysisParam <- value

            [<DefaultValue>] 
            val mutable m_SourceFileParam : DbSet<SourceFileParam>
            member public this.SourceFileParam with get() = this.m_SourceFileParam
                                                            and set value = this.m_SourceFileParam <- value

            [<DefaultValue>] 
            val mutable m_ProteinDetectionHypothesisParam : DbSet<ProteinDetectionHypothesisParam>
            member public this.ProteinDetectionHypothesisParam with get() = this.m_ProteinDetectionHypothesisParam
                                                                            and set value = this.m_ProteinDetectionHypothesisParam <- value

            [<DefaultValue>] 
            val mutable m_ProteinAmbiguityGroupParam : DbSet<ProteinAmbiguityGroupParam>
            member public this.ProteinAmbiguityGroupParam with get() = this.m_ProteinAmbiguityGroupParam
                                                                       and set value = this.m_ProteinAmbiguityGroupParam <- value

            [<DefaultValue>] 
            val mutable m_ProteinDetectionListParam : DbSet<ProteinDetectionListParam>
            member public this.ProteinDetectionListParam with get() = this.m_ProteinDetectionListParam
                                                                      and set value = this.m_ProteinDetectionListParam <- value

            //override this.OnModelCreating (modelBuilder :  ModelBuilder) =
            //         modelBuilder.Entity<MzIdentMLDocument>()
            //            .HasOne(fun item -> item.Inputs)
            //            .WithOne("MzIdentMLDocument")
            //            .HasColumnName("MzIdentMLDocument") |> ignore
                        //.OnDelete(DeleteBehavior.Cascade)|> ignore
            //         modelBuilder.Entity<CVParam>()
            //            .HasOne("PersonID")
            //            .WithMany("Details")
            //            .HasForeignKey("FK")
            //            .OnDelete(DeleteBehavior.Cascade)|> ignore
            //         modelBuilder.Entity<CVParam>()
            //            .HasOne("AmbiguousResidueID")
            //            .WithMany("Details")
            //            .HasForeignKey("FK")
            //            .OnDelete(DeleteBehavior.Cascade)|> ignore
            //         modelBuilder.Entity<CVParam>()
            //            .HasOne("SampleID")
            //            .WithMany("Details")
            //            .HasForeignKey("FK")
            //            .OnDelete(DeleteBehavior.Cascade)|> ignore
