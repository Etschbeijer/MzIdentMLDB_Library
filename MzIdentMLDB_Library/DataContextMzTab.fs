namespace MzTabDataBase

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

    ///The file’s false discovery rate(s) reported at the PSM, peptide, and/or protein level. 
    ///False Localization Rate (FLD) for the reporting of modifications can also be reported here.
    and [<AllowNullLiteral>] [<Table("false_discovery_rate")>]
        FalseDiscoveryRate (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = FalseDiscoveryRate(null, null, null, null, Nullable())

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
    and [<AllowNullLiteral>] [<Table("software[1-n]-setting[1-n]")>]
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

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("OrganizationParams")>]
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

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("PersonParams")>]
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

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("MetaDataParams")>]
        MetaDataParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = MetaDataParam(null, null, null, null, Nullable())

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

    ///Parameters describing the sample’s additional properties.
    type [<AllowNullLiteral>] [<Table("sample[1-n]-custom[1-n]")>]
        SampleParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
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

    ///The respective species of the samples analysed.
    type [<AllowNullLiteral>] [<Table("sample[1-n]-species[1-n]")>]
        Species (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = Species(null, null, null, null, Nullable())

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

    ///The respective tissue(s) of the sample.
    type [<AllowNullLiteral>] [<Table("sample[1-n]-tissue[1-n]")>]
        Tissue (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = Tissue(null, null, null, null, Nullable())

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

    ///The respective cellType(s) of the sample.
    type [<AllowNullLiteral>] [<Table("sample[1-n]-cell_type[1-n]")>]
        CellType (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = CellType(null, null, null, null, Nullable())

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

    ///The respective disease(s) of the sample.
    type [<AllowNullLiteral>] [<Table("sample[1-n]-disease[1-n]")>]
        Disease (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = Disease(null, null, null, null, Nullable())

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
    type [<AllowNullLiteral>] [<Table("fixed_mod[1-n]")>]
        FixedMod (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = FixedMod(null, null, null, null, Nullable())

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
    type [<AllowNullLiteral>] [<Table("fixed_mod[1-n]-site")>]
        FixedModSite (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = FixedModSite(null, null, null, null, Nullable())

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
    type [<AllowNullLiteral>] [<Table("fixed_mod[1-n]-position")>]
        FixedModPosition (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = FixedModPosition(null, null, null, null, Nullable())

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
    type [<AllowNullLiteral>] [<Table("variable_mod[1-n]")>]
        VariableMod (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = VariableMod(null, null, null, null, Nullable())

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
    type [<AllowNullLiteral>] [<Table("variable_mod[1-n]-site")>]
        VariableModSite (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = VariableModSite(null, null, null, null, Nullable())

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
    type [<AllowNullLiteral>] [<Table("variable_mod[1-n]-position")>]
        VariableModPosition (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = VariableModPosition(null, null, null, null, Nullable())

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

    ///The order of the search engine scores SHOULD reflect their importance 
    ///for the identification and be used to determine the identification’s rank.
    type [<AllowNullLiteral>] [<Table("protein_search_engine_score[1-n]")>]
        ProteinSearchEngineScore (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ProteinSearchEngineScore(null, null, null, null, Nullable())

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

    ///The order of the search engine scores SHOULD reflect their importance 
    ///for the identification and be used to determine the identification’s rank.
    type [<AllowNullLiteral>] [<Table("peptide_search_engine_score[1-n]")>]
        PeptideSearchEngineScore (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = PeptideSearchEngineScore(null, null, null, null, Nullable())

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

    ///The order of the search engine scores SHOULD reflect their importance 
    ///for the identification and be used to determine the identification’s rank.
    type [<AllowNullLiteral>] [<Table("psm_search_engine_score[1-n]")>]
        PSMSearchEngineScore (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = PSMSearchEngineScore(null, null, null, null, Nullable())

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

    ///The order of the search engine scores SHOULD reflect their importance 
    ///for the identification and be used to determine the identification’s rank.
    type [<AllowNullLiteral>] [<Table("smallmolecule_search_engine_score[1-n]")>]
        SmallMoleculeSearchEngineScore (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = SmallMoleculeSearchEngineScore(null, null, null, null, Nullable())

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

    ///A parameter specifying the data format of the external MS data file.
    type [<AllowNullLiteral>] [<Table("ms_run[1-n]-format")>]
        MSRunFormat (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = MSRunFormat(null, null, null, null, Nullable())

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
                
    ///Location of the external data file. 
    ///If the actual location of the MS run is unknown, a “null” MUST be used as a place holder value.
    type [<AllowNullLiteral>] [<Table("ms_run[1-n]-location")>]
        MSRunLocation (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = MSRunLocation(null, null, null, null, Nullable())

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

    ///Parameter specifying the id format used in the external data file.
    type [<AllowNullLiteral>] [<Table("ms_run[1-n]-id_format")>]
        MSRunIDFormat (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = MSRunIDFormat(null, null, null, null, Nullable())

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

    ///A list of parameters describing all the types of fragmentation used in a given ms run.
    type [<AllowNullLiteral>] [<Table("ms_run[1-n]-fragmentation_method")>]
        MSRunFragmentationMethod (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = MSRunFragmentationMethod(null, null, null, null, Nullable())

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

    ///Hash value of the corresponding external MS data file defined in ms_run[1-n]-mzTab Specification location.
    type [<AllowNullLiteral>] [<Table("ms_run[1-n]-hash")>]
        MSRunHash (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = MSRunHash(null, null, null, null, Nullable())

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

    ///A parameter specifying the hash methods used to generate the String in ms_run[1-n]-hash. 
    ///Specifics of the hash method used MAY follow the definitions of the mzML format.
    type [<AllowNullLiteral>] [<Table("ms_run[1-n]-hash_method")>]
        MSRunHashMethod (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = MSRunHashMethod(null, null, null, null, Nullable())

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

    ///The reagent used to label the sample in the assay. 
    ///For label-free analyses the “unlabeled sample” CV term SHOULD be used. 
    ///For the “light” channel in label-based experiments the appropriate CV term 
    ///specifying the labelling channel should be used.
    type [<AllowNullLiteral>] [<Table("assay[1-n]-quantification_reagent")>]
        QuantificationReagent (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = QuantificationReagent(null, null, null, null, Nullable())

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

    ///Table describing the modifications site. 
    ///Following the unimod convention, modification site is a residue (e.g. “M”), 
    ///terminus (“N-term” or “C-term”) or both (e.g. “N-term Q” or “C-term K”)
    type [<AllowNullLiteral>] [<Table("assay[1-n]-quantification_mod[1-n]-site")>]
        QuantificationModSite (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = QuantificationModSite(null, null, null, null, Nullable())

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

    ///Table describing the term specifity of the modification. 
    ///Following the unimod convention, term specifity is denoted by the strings “Anywhere”, 
    ///“Any N-term”, “Any C-term”, “Protein N-term”, “Protein C-term”.
    type [<AllowNullLiteral>] [<Table("assay[1-n]-quantification_mod[1-n]-position")>]
        QuantificationModPosition (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = QuantificationModPosition(null, null, null, null, Nullable())

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
               
    ///The name of the instrument used in the experiment. Multiple instruments are numbered 1..n.
    type [<AllowNullLiteral>] [<Table("instrument[1-n]-name")>]
        InstrumentName (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = InstrumentName(null, null, null, null, Nullable())

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

    ///The instrument's source used in the experiment. Multiple instruments are numbered 1..n.
    type [<AllowNullLiteral>] [<Table("instrument[1-n]-source")>]
        InstrumentSource (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = InstrumentSource(null, null, null, null, Nullable())

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

    ///The instrument’s analyzer type used in the experiment. Multiple instruments are enumerated 1..n.
    type [<AllowNullLiteral>] [<Table("instrument[1-n]-analyzer[1-n]")>]
        InstrumentAnalyzer (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = InstrumentAnalyzer(null, null, null, null, Nullable())

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

    ///The instrument's detector type used in the experiment. Multiple instruments are numbered 1..n.
    type [<AllowNullLiteral>] [<Table("instrument[1-n]-detector")>]
        InstrumentDetector (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = InstrumentDetector(null, null, null, null, Nullable())

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

    ///A list of parameters describing a sample processing step. 
    ///The order of the data_processing items should reflect the order these processing steps were performed in.
    type [<AllowNullLiteral>] [<Table("sample_processing[1-n]")>]
        SampleProcessing (id:string, value:string, term:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable rowVersion' = rowVersion

            new() = SampleProcessing(null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Software used to analyze the data and obtain the reported results. 
    ///The parameter’s value SHOULD contain the software’s version. 
    ///The order (numbering) should reflect the order in which the tools were used.
    type [<AllowNullLiteral>] [<Table("Software")>]
        AnalysisSoftware (id:string, version:string, settings:List<AnalysisSoftwareParam>, rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'         = id
            let mutable version'    = version
            [<Column("software[1-n]-setting[1-n]")>]
            let mutable settings'    = settings
            let mutable rowVersion' = rowVersion

            new() = AnalysisSoftware(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Version with get() = version' and set(value) = version' <- value
            member this.Details with get() = settings' and set(value) = settings' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A publication associated with this file. Several publications can be given by indicating the number 
    ///in the square brackets after “publication”. PubMed ids must be prefixed by “pubmed:”, DOIs by “doi:”.
    type [<AllowNullLiteral>] [<Table("publication[1-n]")>]
        BiblioGraphicReference (id:string, name:string, authors:string, doi:string, editor:string, 
                                issue:string, pages:string, publication:string, publisher:string, title:string,
                                volume:string, year:Nullable<int>, rowVersion:Nullable<DateTime>
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
            let mutable rowVersion'  = rowVersion

            new() = BiblioGraphicReference(null, null, null, null, null, null, null, null, null, null, null, Nullable(), (*null,*) Nullable())

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
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Organizations are entities like companies, universities, government agencies.
    type [<AllowNullLiteral>]
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
    type [<AllowNullLiteral>]
        Person (id:string, name:string, firstName:string, midInitials:string, 
                lastName:string, organizations:List<Organization>, 
                details:List<PersonParam>, rowVersion:Nullable<DateTime>
                ) =
            let mutable id'            = id
            let mutable name'          = name
            let mutable firstName'     = firstName
            let mutable midInitials'   = midInitials
            let mutable lastName'      = lastName
            [<Column("contact[1-n]-affiliation")>]
            let mutable organizations' = organizations
            [<Column("contact[1-n]-email")>]
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

    ///A URI pointing to the file's source data (e.g., a PRIDE experiment or a PeptideAtlas build).
    type [<AllowNullLiteral>] [<Table("uri[1-n]")>]
        URI (id:string, uri:string, detail:CVParam, rowVersion:Nullable<DateTime>
            ) =
            let mutable id'            = id
            let mutable uri'          = uri
            let mutable detail'       = detail
            let mutable rowVersion'    = rowVersion

            new() = URI(null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.URI with get() = uri' and set(value) = uri' <- value
            member this.Detail with get() = detail' and set(value) = detail' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A parameter describing a fixed modifications searched for. 
    ///Multiple fixed modifications are numbered 1..n.
    type [<AllowNullLiteral>]
        FixedModification (id:string, fixedMods:List<FixedMod>, fixedModSites:List<FixedModSite>, 
                           fixedModPositions:List<FixedModPosition>, rowVersion:Nullable<DateTime>
                          ) =
            let mutable id'               = id
            [<Column("fixed_mod[1-n]")>]
            let mutable fixedMods'         = fixedMods
            [<Column("fixed_mod[1-n]-site")>]
            let mutable fixedModSites'     = fixedModSites
            [<Column("fixed_mod[1-n]-position")>]
            let mutable fixedModPositions' = fixedModPositions
            let mutable rowVersion'       = rowVersion

            new() = FixedModification(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.FixedMods with get() = fixedMods' and set(value) = fixedMods' <- value
            member this.FixedModSites with get() = fixedModSites' and set(value) = fixedModSites' <- value
            member this.FixedModPositions with get() = fixedModPositions' and set(value) = fixedModPositions' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A parameter describing a variable modifications searched for. 
    ///Multiple fixed modifications are numbered 1..n.
    type [<AllowNullLiteral>]
        VariableModification (id:string, variableMods:List<VariableMod>, variableModSites:List<VariableModSite>, 
                              variableModPositions:List<VariableModPosition>, rowVersion:Nullable<DateTime>
                             ) =
            let mutable id'               = id
            [<Column("variable_mod[1-n]")>]
            let mutable variableMods'         = variableMods
            [<Column("variable_mod[1-n]-sites")>]
            let mutable variableModSites'     = variableModSites
            [<Column("variable_mod[1-n]-position")>]
            let mutable variableModPositions' = variableModPositions
            let mutable rowVersion'       = rowVersion

            new() = VariableModification(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.VariableMods with get() = variableMods' and set(value) = variableMods' <- value
            member this.VariableModSites with get() = variableModSites' and set(value) = variableModSites' <- value
            member this.VariableModPositions with get() = variableModPositions' and set(value) = variableModPositions' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A table describing the instruments of the experiment.
    type [<AllowNullLiteral>]
        Instrument (id:string, instrumentNames:List<InstrumentName>, 
                    instrumentSources:List<InstrumentSource>, 
                    instrumentAnalyzers:List<InstrumentAnalyzer>, 
                    instrumentDetectors:List<InstrumentDetector>, 
                    rowVersion:Nullable<DateTime>
                   ) =
            let mutable id'                  = id
            [<Column("instrument[1-n]-name")>]
            let mutable instrumentNames'     = instrumentNames
            [<Column("instrument[1-n]-source")>]
            let mutable instrumentSources'   = instrumentSources
            [<Column("instrument[1-n]-analyzer[1-n]")>]
            let mutable instrumentAnalyzers' = instrumentAnalyzers
            [<Column("instrument[1-n]-detector")>]
            let mutable instrumentDetectors' = instrumentDetectors
            let mutable rowVersion'          = rowVersion

            new() = Instrument(null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.InstrumentNames with get() = instrumentNames' and set(value) = instrumentNames' <- value
            member this.InstrumentSources with get() = instrumentSources' and set(value) = instrumentSources' <- value
            member this.InstrumentAnalyzers with get() = instrumentAnalyzers' and set(value) = instrumentAnalyzers' <- value
            member this.InstrumentDetectors with get() = instrumentDetectors' and set(value) = instrumentDetectors' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
    
    ///A table describing the instruments of the experiment.
    type [<AllowNullLiteral>]
        SearchEngineScore (id:string, proteinSearchEngineScores:List<ProteinSearchEngineScore>, 
                           peptideSearchEngineScores:List<PeptideSearchEngineScore>, 
                           psmSearchEngineScores:List<PSMSearchEngineScore>, 
                           smallMoleculeSearchEngineScores:List<SmallMoleculeSearchEngineScore>, 
                           rowVersion:Nullable<DateTime>
                          ) =
            let mutable id'                              = id
            [<Column("protein_search_engine_score[1-n]")>]
            let mutable proteinSearchEngineScores'       = proteinSearchEngineScores
            [<Column("peptide_search_engine_score[1-n]")>]
            let mutable peptideSearchEngineScores'       = peptideSearchEngineScores
            [<Column("psm_search_engine_score[1-n]")>]
            let mutable psmSearchEngineScores'           = psmSearchEngineScores
            [<Column("smallmolecule_search_engine_score[1-n]")>]
            let mutable smallMoleculeSearchEngineScores' = smallMoleculeSearchEngineScores
            let mutable rowVersion'                      = rowVersion

            new() = SearchEngineScore(null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.ProteinSearchEngineScores with get() = proteinSearchEngineScores' and set(value) = proteinSearchEngineScores' <- value
            member this.PeptideSearchEngineScores with get() = peptideSearchEngineScores' and set(value) = peptideSearchEngineScores' <- value
            member this.PSMSearchEngineScores with get() = psmSearchEngineScores' and set(value) = psmSearchEngineScores' <- value
            member this.SmallMoleculeSearchEngineScores with get() = smallMoleculeSearchEngineScores' and set(value) = smallMoleculeSearchEngineScores' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A table describing the quantifications of the experiment.
    type [<AllowNullLiteral>]
        Quantification (id:string, method:CVParam, proteinUnit:CVParam, peptideUnit:CVParam,
                        smallMoleculeUnit:CVParam, rowVersion:Nullable<DateTime>
                       ) =
            let mutable id'         = id
            [<Column("quantification_method")>]
            let mutable method' = method
            [<Column("protein-quantification_unit")>]
            let mutable proteinUnit'     = proteinUnit
            [<Column("peptide-quantification_unit")>]
            let mutable peptideUnit'      = peptideUnit
            [<Column("small_molecule-quantification_unit")>]
            let mutable smallMoleculeUnit'    = smallMoleculeUnit
            let mutable rowVersion'  = rowVersion

            new() = Quantification(null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Method with get() = method' and set(value) = method' <- value
            member this.ProteinUnit with get() = proteinUnit' and set(value) = proteinUnit' <- value
            member this.PeptideUnit with get() = peptideUnit' and set(value) = peptideUnit' <- value
            member this.SmallMoleculeUnit with get() = smallMoleculeUnit' and set(value) = smallMoleculeUnit' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A table describing the parameters of a ms_run.
    type [<AllowNullLiteral>]
        MSRun (id:string, msRunFormat:List<MSRunFormat>, msRunLocation:List<MSRunLocation>, 
               msRunIDFormat:List<MSRunIDFormat>, msRunFragmentationMethod:List<MSRunFragmentationMethod>, 
               msRunHash:List<MSRunHash>, msRunHashMethod:List<MSRunHashMethod>,
               rowVersion:Nullable<DateTime>
              ) =
            let mutable id'                       = id
            [<Column("ms_run[1-n]-format")>]
            let mutable msRunFormat'              = msRunFormat
            [<Column("ms_run[1-n]-location")>]
            let mutable msRunLocation'            = msRunLocation
            [<Column("ms_run[1-n]-id_format")>]
            let mutable msRunIDFormat'            = msRunIDFormat
            [<Column("ms_run[1-n]-fragmentation_method")>]
            let mutable msRunFragmentationMethod' = msRunFragmentationMethod
            [<Column("ms_run[1-n]-hash")>]
            let mutable msRunHash'                = msRunHash
            [<Column("ms_run[1-n]-hash_method")>]
            let mutable msRunHashMethod'          = msRunHashMethod
            let mutable rowVersion'               = rowVersion

            new() = MSRun(null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.MSRunFormat with get() = msRunFormat' and set(value) = msRunFormat' <- value
            member this.MSRunLocation with get() = msRunLocation' and set(value) = msRunLocation' <- value
            member this.MSRunIDFormat with get() = msRunIDFormat' and set(value) = msRunIDFormat' <- value
            member this.MSRunFragmentationMethod with get() = msRunFragmentationMethod' and set(value) = msRunFragmentationMethod' <- value
            member this.MSRunHash with get() = msRunHash' and set(value) = msRunHash' <- value
            member this.MSRunHashMethod with get() = msRunHashMethod' and set(value) = msRunHashMethod' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A table describing the samples of the experiment.
    type [<AllowNullLiteral>]
        Sample (id:string, description:string, species:List<Species>, tissues:List<Tissue>, 
                cellTypes:List<CellType>, diseases:List<Disease>, 
                details:List<SampleParam>, rowVersion:Nullable<DateTime>
               ) =
            let mutable id'         = id
            [<Column("sample[1-n]-description")>]
            let mutable description' = description
            [<Column("sample[1-n]-species[1-n]")>]
            let mutable species'     = species
            [<Column("sample[1-n]-tissue[1-n]")>]
            let mutable tissues'      = tissues
            [<Column("sample[1-n]-cell_type[1-n]")>]
            let mutable cellTypes'    = cellTypes
            [<Column("sample[1-n]-disease[1-n]")>]
            let mutable diseases'     = diseases
            [<Column("sample[1-n]-custom[1-n]")>]
            let mutable details'     = details
            let mutable rowVersion'  = rowVersion

            new() = Sample(null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Description with get() = description' and set(value) = description' <- value
            member this.Species with get() = species' and set(value) = species' <- value
            member this.Tissues with get() = tissues' and set(value) = tissues' <- value
            member this.CellTypes with get() = cellTypes' and set(value) = cellTypes' <- value
            member this.Diseases with get() = diseases' and set(value) = diseases' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A table describing the assays of the experiment.
    type [<AllowNullLiteral>]
        Assay (id:string, quantificationReagents:List<QuantificationReagent>, 
               quantificationModSites:List<QuantificationModSite>, 
               quantificationModPositions:List<QuantificationModPosition>, 
               samples:List<Sample>, msRuns:List<MSRun>, rowVersion:Nullable<DateTime>
              ) =
            let mutable id'                         = id
            [<Column("assay[1-n]-quantification_reagent")>]
            let mutable quantificationReagents'     = quantificationReagents
            [<Column("assay[1-n]-quantification_mod[1-n]-site")>]
            let mutable quantificationModSites'     = quantificationModSites
            [<Column("assay[1-n]-quantification_mod[1-n]-position")>]
            let mutable quantificationModPositions' = quantificationModPositions
            [<Column("assay[1-n]-sample_ref")>]
            let mutable samples'                    = samples
            [<Column("assay[1-n]-ms_run_ref")>]
            let mutable msRuns'                     = msRuns
            let mutable rowVersion'                 = rowVersion

            new() = Assay(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.QuantificationReagents with get() = quantificationReagents' and set(value) = quantificationReagents' <- value
            member this.QuantificationModSites with get() = quantificationModSites' and set(value) = quantificationModSites' <- value
            member this.QuantificationModPositions with get() = quantificationModPositions' and set(value) = quantificationModPositions' <- value
            member this.Samples with get() = samples' and set(value) = samples' <- value
            member this.MSRuns with get() = msRuns' and set(value) = msRuns' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Defines the unit used unit for a column in the corresponding section. 
    ///The format of the value has to be {column name}={Parameter defining the unit}
    ///This field MUST NOT be used to define a unit for quantification columns.
    type [<AllowNullLiteral>]
        ColUnit (id:string, protein:CVParam, peptide:CVParam, psm:CVParam, 
                 smallMolecule:CVParam, rowVersion:Nullable<DateTime>
                ) =
            let mutable id'                         = id
            [<Column("colunit-protein")>]
            let mutable protein'     = protein
            [<Column("colunit-peptide")>]
            let mutable peptide' = peptide
            [<Column("colunit-psm")>]
            let mutable psm'                    = psm
            [<Column("colunit-small_molecule")>]
            let mutable smallMolecule'                     = smallMolecule
            let mutable rowVersion'                 = rowVersion

            new() = ColUnit(null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Protein with get() = protein' and set(value) = protein' <- value
            member this.Peptide with get() = peptide' and set(value) = peptide' <- value
            member this.PSM with get() = psm' and set(value) = psm' <- value
            member this.SmallMolecule with get() = smallMolecule' and set(value) = smallMolecule' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A table describing the studyVariables of the experiment.
    type [<AllowNullLiteral>]
        StudyVariable (id:string, description:string, assays:List<Assay>, 
                       samples:List<Sample>, rowVersion:Nullable<DateTime>
                      ) =
            let mutable id'          = id
            [<Column("study_variable[1-n]-assay_refs")>]
            let mutable description' = description
            [<Column("assay[1-n]-quantification_mod[1-n]-site")>]
            let mutable assays'      = assays
            [<Column("study_variable[1-n]-sample_refs")>]
            let mutable samples'     = samples
            let mutable rowVersion'  = rowVersion

            new() = StudyVariable(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Description with get() = description' and set(value) = description' <- value
            member this.Assays with get() = assays' and set(value) = assays' <- value
            member this.Samples with get() = samples' and set(value) = samples' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The results included in an mzTab file can be reported in 2 ways: 
    ///‘Complete’ (when results for each assay/replicate are included) and 
    ///‘Summary’, when only the most representative results are reported.
    type MzTabMode =
        | Summary  = 0 
        | Complete = 1

    ///The results included in an mzTab file MUST be flagged as ‘Identification’ or ‘Quantification’ 
    ///- the latter encompassing approaches that are quantification only or quantification and identification.
    type MzType =
        | Identification = 0 
        | Quantification = 1

    ///The metadata section can provide additional information about the dataset(s) 
    ///reported in the mzTab file.
    type [<AllowNullLiteral>]
         MetaData (id:string, title:string, description:string, version:string, mode:Nullable<MzTabMode>, 
                   mzType:Nullable<MzType>, sampleProcessings:List<SampleProcessing>, instruments:List<Instrument>,
                   analysisSoftwares:List<AnalysisSoftware>, searchEngineScores:List<SearchEngineScore>,
                   falseDiscoveryRates:List<FalseDiscoveryRate>, publications:List<BiblioGraphicReference>, 
                   persons:List<Person>, uri:List<URI>, fixedModifications:List<FixedModification>, 
                   variableModifications:List<VariableModification>, quantification:Quantification, 
                   msRuns:List<MSRun>, samples:List<Sample>, assays:List<Assay>, 
                   studyVariables:List<StudyVariable>, colUnit:ColUnit, details:List<MetaDataParam>, 
                   rowVersion:Nullable<DateTime>
                  ) =  
            let mutable id'                    = id
            let mutable title'                 = title
            let mutable description'           = description
            let mutable version'               = version
            let mutable mode'                  = mode
            let mutable type'                  = mzType
            let mutable sampleProcessings'     = sampleProcessings
            let mutable instruments'           = instruments
            let mutable analysisSoftwares'     = analysisSoftwares 
            let mutable searchEngineScores'    = searchEngineScores
            let mutable falseDiscoveryRates'   = falseDiscoveryRates
            [<Column("publication[1-n]")>]
            let mutable publications'          = publications
            [<Column("contact[1-n]-name")>]
            let mutable persons'               = persons
            [<Column("uri[1-n]")>]
            let mutable uri'                   = uri
            let mutable fixedModifications'    = fixedModifications
            let mutable variableModifications' = variableModifications
            let mutable quantification'        = quantification
            let mutable msRuns'                = msRuns
            let mutable samples'               = samples
            let mutable assays'                = assays
            let mutable studyVariables'        = studyVariables
            let mutable colUnit'               = colUnit
            [<Column("custom[1-n]")>]
            let mutable details'               = details
            let mutable rowVersion'            = rowVersion

            new() = MetaData(null, null, null, null,  Nullable(),  Nullable(), null, null, 
                             null, null, null, null, null, null, null, null, null, null, null, null, null,
                             null, null, Nullable()
                            )

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Title with get() = title' and set(value) = title' <- value
            member this.Description with get() = description' and set(value) = description' <- value
            member this.Version with get() = version' and set(value) = version' <- value
            member this.Mode with get() = mode' and set(value) = mode' <- value
            member this.Type with get() = type' and set(value) = type' <- value
            member this.SampleProcessings with get() = sampleProcessings' and set(value) = sampleProcessings' <- value
            member this.Instruments with get() = instruments' and set(value) = instruments' <- value
            member this.AnalysisSoftwares with get() = analysisSoftwares' and set(value) = analysisSoftwares' <- value
            member this.SearchEngineScores with get() = searchEngineScores' and set(value) = searchEngineScores' <- value
            member this.FalseDiscoveryRates with get() = falseDiscoveryRates' and set(value) = falseDiscoveryRates' <- value
            member this.Publications with get() = publications' and set(value) = publications' <- value
            member this.Persons with get() = persons' and set(value) = persons' <- value
            member this.URI with get() = uri' and set(value) = uri' <- value
            member this.FixedModifications with get() = fixedModifications' and set(value) = fixedModifications' <- value
            member this.VariableModifications with get() = variableModifications' and set(value) = variableModifications' <- value
            member this.Quantification with get() = quantification' and set(value) = quantification' <- value
            member this.MSRuns with get() = msRuns' and set(value) = msRuns' <- value
            member this.Samples with get() = samples' and set(value) = samples' <- value
            member this.Assays with get() = assays' and set(value) = assays' <- value
            member this.StudyVariables with get() = studyVariables' and set(value) = studyVariables' <- value
            member this.ColUnit with get() = colUnit' and set(value) = colUnit' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The peptide's sequence.
    type [<AllowNullLiteral>]
        PeptideSequence (id:string, rowVersion:Nullable<DateTime>
                         ) =
            [<Column("sequence")>]
            let mutable id'                       = id
            let mutable rowVersion'               = rowVersion

            new() = PeptideSequence(null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The accession of the protein in the source database.
    type [<AllowNullLiteral>]
        Accession (id:string, rowVersion:Nullable<DateTime>
                  ) =
            let mutable id'              = id
            let mutable rowVersion'      = rowVersion

            new() = Accession(null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A table describing the additional information for the proteins of the experiment.
    type [<AllowNullLiteral>]
        AccessionParamaeter (id:string, peptideSequence:PeptideSequence, accession:Accession, taxid:string, species:string,
                             dataBase:string, dataBaseVersion:string, rowVersion:Nullable<DateTime>
                            ) =
            let mutable id'              = id
            let mutable peptideSequence' = peptideSequence
            let mutable accession'       = accession
            let mutable taxid'           = taxid
            let mutable species'         = species
            let mutable dataBase'        = dataBase
            [<Column("database_version")>]
            let mutable dataBaseVersion' = dataBaseVersion
            let mutable rowVersion'      = rowVersion

            new() = AccessionParamaeter(null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.PeptideSequence with get() = peptideSequence' and set(value) = peptideSequence' <- value
            member this.Accession with get() = accession' and set(value) = accession' <- value
            member this.Taxid with get() = taxid' and set(value) = taxid' <- value
            member this.Species with get() = species' and set(value) = species' <- value
            member this.DataBase with get() = dataBase' and set(value) = dataBase' <- value
            member this.DataBaseVersion with get() = dataBaseVersion' and set(value) = dataBaseVersion' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A delimited list of search engine(s) that identified this protein.
    type [<AllowNullLiteral>] [<Table("search_engine")>]
        SearchEgnineName (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = SearchEgnineName(null, null, null, null, Nullable())

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

    ///A table describing the additional information about the searchengines of the experiment.
    type [<AllowNullLiteral>]
        SearchEngine (id:string, peptideSequence:PeptideSequence, accession:Accession, searchEngineNames:List<SearchEgnineName>, 
                      bestSearchEngineScore:Nullable<float>, searchEngineScoreMSRun:Nullable<float>, 
                      rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'                     = id
            [<Column("sequence")>]
            let mutable peptideSequence'        = peptideSequence
            let mutable accession'              = accession
            [<Column("search_engine")>]
            let mutable searchEngineNames'      = searchEngineNames
            [<Column("best_search_engine_score[1-n]")>]
            let mutable bestSearchEngineScore'  = bestSearchEngineScore
            [<Column("search_engine_score[1-n]_ms_run[1-n]")>]
            let mutable searchEngineScoreMSRun' = searchEngineScoreMSRun
            let mutable rowVersion'             = rowVersion

            new() = SearchEngine(null, null, null, null, Nullable(), Nullable(), Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.PeptideSequence with get() = peptideSequence' and set(value) = peptideSequence' <- value
            member this.Accession with get() = accession' and set(value) = accession' <- value
            member this.SearchEngineNames with get() = searchEngineNames' and set(value) = searchEngineNames' <- value
            member this.BestSearchEngineScore with get() = bestSearchEngineScore' and set(value) = bestSearchEngineScore' <- value
            member this.SearchEngineScoreMSRun with get() = searchEngineScoreMSRun' and set(value) = searchEngineScoreMSRun' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A table describing the additional information about the peptides of the experiment.
    type [<AllowNullLiteral>]
        PeptideInfo (id:string, accession:Accession, numPeptidesDistinctMSRun:string, 
                      numPeptidesUniqueMSRun:string, ambigutityMembers:List<Accession>, 
                      rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'                       = id
            let mutable accession'                = accession
            [<Column("num_peptides_distinct_ms_run[1-n]")>]
            let mutable numPeptidesDistinctMSRun' = numPeptidesDistinctMSRun
            [<Column("num_peptides_unique_ms_run[1-n]")>]
            let mutable numPeptidesUniqueMSRun'   = numPeptidesUniqueMSRun
            [<Column("ambiguity_members")>]
            let mutable ambigutityMembers'        = ambigutityMembers
            let mutable rowVersion'               = rowVersion

            new() = PeptideInfo(null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Accession with get() = accession' and set(value) = accession' <- value
            member this.NumPeptidesDistinctMSRun with get() = numPeptidesDistinctMSRun' and set(value) = numPeptidesDistinctMSRun' <- value
            member this.NumPeptidesUniqueMSRun with get() = numPeptidesUniqueMSRun' and set(value) = numPeptidesUniqueMSRun' <- value
            member this.AmbigutityMembers with get() = ambigutityMembers' and set(value) = ambigutityMembers' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///In contrast to the PSM section, fixed modifications or modifications caused by the 
    ///quantification reagent (i.e. the SILAC/iTRAQ label) SHOULD NOT be reported in this column.
    type [<AllowNullLiteral>] [<Table("search_engine")>]
        Modification (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = Modification(null, null, null, null, Nullable())

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

    ///A table describing the additional information about the proteins of the experiment.
    type [<AllowNullLiteral>]
        ProteinAbundance (id:string, accession:Accession, abundanceAssay:string, 
                          abundanceStudyVariable:string, abundanceSEDEVStudyVariable:string, 
                          abundanceSTDErrorStudyVariable:string, rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'                       = id
            let mutable accession'                = accession
            [<Column("protein_abundance_assay[1-n]")>]
            let mutable abundanceAssay' = abundanceAssay
            [<Column("protein_abundance_study_variable[1-n]")>]
            let mutable abundanceStudyVariable'   = abundanceStudyVariable
            [<Column("protein_abundance_stdev_study_variable[1-n]")>]
            let mutable abundanceSEDEVStudyVariable'        = abundanceSEDEVStudyVariable
            [<Column("protein_abundance_std_error_study_variable [1-n]")>]
            let mutable abundanceSTDErrorStudyVariable'        = abundanceSTDErrorStudyVariable
            let mutable rowVersion'               = rowVersion

            new() = ProteinAbundance(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Accession with get() = accession' and set(value) = accession' <- value
            member this.AbundanceAssay with get() = abundanceAssay' and set(value) = abundanceAssay' <- value
            member this.AbundanceStudyVariable with get() = abundanceStudyVariable' and set(value) = abundanceStudyVariable' <- value
            member this.AbundanceSEDEVStudyVariable with get() = abundanceSEDEVStudyVariable' and set(value) = abundanceSEDEVStudyVariable' <- value
            member this.AbundanceSTDErrorStudyVariable with get() = abundanceSTDErrorStudyVariable' and set(value) = abundanceSTDErrorStudyVariable' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///AdditionalInforamtion for proteinSection..
    type [<AllowNullLiteral>] [<Table("ProteinSectionParams")>]
        ProteinSectionParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = ProteinSectionParam(null, null, null, null, Nullable())

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

    ///The protein section can provide additional information about the reported proteins in the mzTab file.
    type [<AllowNullLiteral>]
         ProteinSection (id:string, accession:Accession, description:string, accessionParameter:List<AccessionParamaeter>,
                         searchEngine:List<SearchEngine>, reliability:Nullable<int>, numPSMSMSRun:string,
                         peptideInfo:PeptideInfo, modifications:List<Modification>, uri:string, goTerms:string, 
                         proteinCoverage:Nullable<float>, proteinAbundance:List<ProteinAbundance>, 
                         details:List<ProteinSectionParam>, rowVersion:Nullable<DateTime>
                        ) =  
            let mutable id'                 = id
            let mutable accession'          = accession
            let mutable description'        = description
            let mutable accessionParameter' = accessionParameter
            let mutable searchEngine'       = searchEngine
            let mutable reliability'        = reliability
            [<Column("num_psms_ms_run[1-n]")>]
            let mutable numPSMSMSRun'       = numPSMSMSRun
            let mutable peptideInfo'        = peptideInfo 
            let mutable modifications'      = modifications
            let mutable uri'                = uri
            [<Column("go_terms")>]
            let mutable goTerms'            = goTerms
            [<Column("protein_coverage")>]
            let mutable proteinCoverage'    = proteinCoverage
            [<Column("protein_abundance")>]
            let mutable proteinAbundance'   = proteinAbundance
            let mutable details'            = details
            let mutable rowVersion'         = rowVersion

            new() = ProteinSection(null, null, null, null, null, Nullable(), null, null, null, null, null, 
                                   Nullable(), null, null, Nullable()
                                  )

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Accession with get() = accession' and set(value) = accession' <- value
            member this.Description with get() = description' and set(value) = description' <- value
            member this.AccessionParameter with get() = accessionParameter' and set(value) = accessionParameter' <- value
            member this.SearchEngine with get() = searchEngine' and set(value) = searchEngine' <- value
            member this.Reliability with get() = reliability' and set(value) = reliability' <- value
            member this.NumPSMSMSRun with get() = numPSMSMSRun' and set(value) = numPSMSMSRun' <- value
            member this.PeptideInfo with get() = peptideInfo' and set(value) = peptideInfo' <- value
            member this.Modifications with get() = modifications' and set(value) = modifications' <- value
            member this.URI with get() = uri' and set(value) = uri' <- value
            member this.GoTerms with get() = goTerms' and set(value) = goTerms' <- value
            member this.ProteinCoverage with get() = proteinCoverage' and set(value) = proteinCoverage' <- value
            member this.ProteinAbundance with get() = proteinAbundance' and set(value) = proteinAbundance' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
  
    ///AdditionalInforamtion for proteinSection..
    type [<AllowNullLiteral>] [<Table("retention_time_window")>]
        RetentionTimeWindow (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = RetentionTimeWindow(null, null, null, null, Nullable())

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
    
    ///A table describing the additional information about the retentionTime 
    ///of the protein of the parent-protein.
    type [<AllowNullLiteral>]
        RetentionTime (id:string, peptideSequence:PeptideSequence, retentionTime:Nullable<float>, 
                       retentionTimeWindow:List<RetentionTimeWindow>, rowVersion:Nullable<DateTime>
                      ) =
            let mutable id'                  = id
            [<Column("Sequence")>]
            let mutable peptideSequence'     = peptideSequence
            [<Column("retention_time")>]
            let mutable retentionTime'       = retentionTime
            [<Column("retention_time_window")>]
            let mutable retentionTimeWindow' = retentionTimeWindow
            let mutable rowVersion'          = rowVersion

            new() = RetentionTime(null, null, Nullable(), null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.PeptideSequence with get() = peptideSequence' and set(value) = peptideSequence' <- value
            member this.RetentionTime with get() = retentionTime' and set(value) = retentionTime' <- value
            member this.RetentionTimeWindow with get() = retentionTimeWindow' and set(value) = retentionTimeWindow' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A table describing the additional information about the peptides of the experiment.
    type [<AllowNullLiteral>]
        PeptideAbundance (id:string, peptideSequence:PeptideSequence, abundanceAssay:string, 
                          abundanceStudyVariable:string, abundanceSEDEVStudyVariable:string, 
                          abundanceSTDErrorStudyVariable:string, rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'                             = id
            [<Column("sequence")>]
            let mutable peptideSequence'                = peptideSequence
            [<Column("peptide_abundance_assay[1-n]")>]
            let mutable abundanceAssay'                 = abundanceAssay
            [<Column("peptide_abundance_study_variable[1-n]")>]
            let mutable abundanceStudyVariable'         = abundanceStudyVariable
            [<Column("peptide_abundance_stdev_study_variable[1-n]")>]
            let mutable abundanceSEDEVStudyVariable'    = abundanceSEDEVStudyVariable
            [<Column("peptide_abundance_std_error_study_variable[1-n]")>]
            let mutable abundanceSTDErrorStudyVariable' = abundanceSTDErrorStudyVariable
            let mutable rowVersion'                     = rowVersion

            new() = PeptideAbundance(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.PeptideSequence with get() = peptideSequence' and set(value) = peptideSequence' <- value
            member this.AbundanceAssay with get() = abundanceAssay' and set(value) = abundanceAssay' <- value
            member this.AbundanceStudyVariable with get() = abundanceStudyVariable' and set(value) = abundanceStudyVariable' <- value
            member this.AbundanceSEDEVStudyVariable with get() = abundanceSEDEVStudyVariable' and set(value) = abundanceSEDEVStudyVariable' <- value
            member this.AbundanceSTDErrorStudyVariable with get() = abundanceSTDErrorStudyVariable' and set(value) = abundanceSTDErrorStudyVariable' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
    
    ///AdditionalInforamtion for proteinSection.
    type [<AllowNullLiteral>] [<Table("PeptideSectionParams")>]
        PeptideSectionParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = PeptideSectionParam(null, null, null, null, Nullable())

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
    
    ///The protein section can provide additional information about the reported peptides in the mzTab file.
    type [<AllowNullLiteral>]
         PeptideSection (id:string, peptideSequence:PeptideSequence, accession:Accession, unique:Nullable<bool>, 
                         accessionParameter:List<AccessionParamaeter>, searchEngine:List<SearchEngine>, 
                         reliability:Nullable<int>, modifications:List<Modification>, retentionTime:RetentionTime,
                         charge:Nullable<float>, massToCharge:Nullable<float>, uri:string, 
                         spectraRef:string, peptideAbundance:List<PeptideAbundance>, 
                         details:List<PeptideSectionParam>, rowVersion:Nullable<DateTime>
                        ) =  
            let mutable id'                 = id
            [<Column("sequence")>]
            let mutable peptideSequence'    = peptideSequence
            let mutable accession'          = accession
            let mutable unique'             = unique
            let mutable accessionParameter' = accessionParameter
            let mutable searchEngine'       = searchEngine
            let mutable reliability'        = reliability
            let mutable modifications'      = modifications
            let mutable retentionTime'      = retentionTime
            let mutable charge'             = charge
            [<Column("mass_to_charge")>]
            let mutable massToCharge'       = massToCharge
            let mutable uri'                = uri
            [<Column("spectra_ref")>]
            let mutable spectraRef'         = spectraRef
            [<Column("peptide_coverage")>]
            let mutable peptideAbundance'   = peptideAbundance
            let mutable details'            = details
            let mutable rowVersion'         = rowVersion

            new() = PeptideSection(null, null, null, Nullable(), null, null, Nullable(), null, null, Nullable(), 
                                   Nullable(), null,  null, null, null, Nullable()
                                  )

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Sequence with get() = peptideSequence' and set(value) = peptideSequence' <- value
            member this.Accession with get() = accession' and set(value) = accession' <- value
            member this.Unique with get() = unique' and set(value) = unique' <- value
            member this.AccessionParameter with get() = accessionParameter' and set(value) = accessionParameter' <- value
            member this.SearchEngine with get() = searchEngine' and set(value) = searchEngine' <- value
            member this.Reliability with get() = reliability' and set(value) = reliability' <- value
            member this.Modifications with get() = modifications' and set(value) = modifications' <- value
            member this.RetentionTime with get() = retentionTime' and set(value) = retentionTime' <- value
            member this.Charge with get() = charge' and set(value) = charge' <- value
            member this.MassToCharge with get() = massToCharge' and set(value) = massToCharge' <- value
            member this.URI with get() = uri' and set(value) = uri' <- value
            member this.SpectraRef with get() = spectraRef' and set(value) = spectraRef' <- value
            member this.PeptideAbundance with get() = peptideAbundance' and set(value) = peptideAbundance' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A unique identifier for a PSM within the file. 
    ///If a PSM can be matched to multiple proteins, the same PSM should be represented on multiple rows 
    ///with different accessions and the same PSM_ID.
    type [<AllowNullLiteral>]  [<Table("PSM_ID")>]
        PSMID (id:string, rowVersion:Nullable<DateTime>) =
            [<Column("PSM_ID")>]
            let mutable id'         = id
            let mutable rowVersion' = rowVersion

            new() = PSMID(null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
    
    ///A table describing the additional information about the peptides and protein based on the psm.
    type [<AllowNullLiteral>]
        PSMInformation (id:string, peptideSequence:string, pre:string, post:string, 
                        start:Nullable<int>, ende:Nullable<int>, rowVersion:Nullable<DateTime>
                       ) =
            let mutable id'              = id
            [<Column("sequence")>]
            let mutable peptideSequence' = peptideSequence
            let mutable pre'             = pre
            let mutable post'            = post
            let mutable start'           = start
            [<Column("end")>]
            let mutable end'             = ende
            let mutable rowVersion'      = rowVersion

            new() = PSMInformation(null, null, null, null, Nullable(), Nullable(), Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.PeptideSequence with get() = peptideSequence' and set(value) = peptideSequence' <- value
            member this.Pre with get() = pre' and set(value) = pre' <- value
            member this.Post with get() = post' and set(value) = post' <- value
            member this.Start with get() = start' and set(value) = start' <- value
            member this.End with get() = end' and set(value) = end' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
    
    ///AdditionalInforamtion for psmSection.
    type [<AllowNullLiteral>] [<Table("PSMSectionParams")>]
        PSMSectionParam (id:string, value:string, term:Term, unit:Term, rowVersion:Nullable<DateTime>) =  
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable unit'       = unit
            let mutable rowVersion' = rowVersion

            new() = PSMSectionParam(null, null, null, null, Nullable())

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
    
    ///The protein section can provide additional information about the reported peptides in the mzTab file.
    type [<AllowNullLiteral>]
         PSMSection (id:string, peptideSequence:PeptideSequence, psmID:PSMID, accession:Accession, 
                     unique:Nullable<bool>,  accessionParameter:List<AccessionParamaeter>, 
                     searchEngine:List<SearchEngine>,  reliability:Nullable<int>, 
                     modifications:List<Modification>, retentionTime:Nullable<float>,
                     charge:Nullable<float>, experimentalMassToCharge:Nullable<float>, 
                     calculatedMassToCharge:Nullable<float>, uri:string, 
                     spectraRef:string, psmInformation:PSMInformation, 
                     details:List<PSMSectionParam>, rowVersion:Nullable<DateTime>
                    ) =  
            let mutable id'                       = id
            [<Column("sequence")>]
            let mutable peptideSequence'          = peptideSequence
            [<Column("PSM_ID")>]
            let mutable psmID'                    = psmID
            let mutable accession'                = accession
            let mutable unique'                   = unique
            let mutable accessionParameter'       = accessionParameter
            let mutable searchEngine'             = searchEngine
            let mutable reliability'              = reliability
            let mutable modifications'            = modifications
            let mutable retentionTime'            = retentionTime
            let mutable charge'                   = charge
            [<Column("exp_mass_to_charge")>]
            let mutable experimentalMassToCharge' = experimentalMassToCharge
            [<Column("calc_mass_to_charge")>]
            let mutable calculatedMassToCharge'   = calculatedMassToCharge
            let mutable uri'                      = uri
            [<Column("spectra_ref")>]
            let mutable spectraRef'               = spectraRef
            [<Column("peptide_coverage")>]
            let mutable psmInformation'         = psmInformation
            let mutable details'                  = details
            let mutable rowVersion'               = rowVersion

            new() = PSMSection(null, null, null, null, Nullable(), null, null, Nullable(), null, Nullable(), 
                               Nullable(), Nullable(), Nullable(), null,  null, null, null, Nullable()
                              )

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Sequence with get() = peptideSequence' and set(value) = peptideSequence' <- value
            member this.PSMID with get() = psmID' and set(value) = psmID' <- value
            member this.Accession with get() = accession' and set(value) = accession' <- value
            member this.Unique with get() = unique' and set(value) = unique' <- value
            member this.AccessionParameter with get() = accessionParameter' and set(value) = accessionParameter' <- value
            member this.SearchEngine with get() = searchEngine' and set(value) = searchEngine' <- value
            member this.Reliability with get() = reliability' and set(value) = reliability' <- value
            member this.Modifications with get() = modifications' and set(value) = modifications' <- value
            member this.RetentionTime with get() = retentionTime' and set(value) = retentionTime' <- value
            member this.Charge with get() = charge' and set(value) = charge' <- value
            member this.ExperimentalMassToCharge with get() = experimentalMassToCharge' and set(value) = experimentalMassToCharge' <- value
            member this.CalculatedMassToCharge with get() = calculatedMassToCharge' and set(value) = calculatedMassToCharge' <- value
            member this.URI with get() = uri' and set(value) = uri' <- value
            member this.SpectraRef with get() = spectraRef' and set(value) = spectraRef' <- value
            member this.PSMInformation with get() = psmInformation' and set(value) = psmInformation' <- value
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    //Go on with small molecule section.
    
    type MzTab =
     
            inherit DbContext

            new(options : DbContextOptions<MzTab>) = {inherit DbContext(options)}

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
            val mutable m_MetaData : DbSet<MetaData>
            member public this.MetaData with get() = this.m_MetaData
                                                     and set value = this.m_MetaData <- value

            [<DefaultValue>] 
            val mutable m_ProteinSection : DbSet<ProteinSection>
            member public this.ProteinSection with get() = this.m_ProteinSection
                                                           and set value = this.m_ProteinSection <- value

            [<DefaultValue>] 
            val mutable m_PeptideSection : DbSet<PeptideSection>
            member public this.PeptideSection with get() = this.m_PeptideSection
                                                           and set value = this.m_PeptideSection <- value
