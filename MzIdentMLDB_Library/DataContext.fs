﻿namespace MzIdentMLDataBase


module DataContext =

    open System
    open System.ComponentModel.DataAnnotations.Schema
    open Microsoft.EntityFrameworkCore
    open System.Collections.Generic


    module EntityTypes =
        open System.Data

        //type [<CLIMutable>] 
        //    Term =
        //    {
        //        ID               : string
        //        mutable Name     : string
        //        mutable Ontology : Ontology
        //        RowVersion       : DateTime 
        //    }

        type [<AllowNullLiteral>]
            Term (id:string, name:string, ontology:Ontology, rowVersion:DateTime) =
                let mutable id'         = id
                let mutable name'       = name
                let mutable ontology'   = ontology
                let mutable rowVersion' = rowVersion
                new() = Term()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Ontology with get() = ontology' and set(value) = ontology' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
        
    
        ///Standarized vocabulary for MS-Database.
        and [<AllowNullLiteral>]
            Ontology (id:string, terms:List<Term>, rowVersion:DateTime) =
                let mutable id'         = id
                let mutable terms'      = terms
                let mutable rowVersion' = rowVersion
                new() = Ontology()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Terms with get() = terms' and set(value) = terms' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("CVParams")>]
            CVParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = CVParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("OrganizationParams")>]
            OrganizationParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = OrganizationParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("PersonParams")>]
            PersonParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = PersonParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("SampleParams")>]
            SampleParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = SampleParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("ModificationParams")>]
            ModificationParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = ModificationParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("PeptideParams")>]
            PeptideParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = PeptideParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("TranslationTableParams")>]
            TranslationTableParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = TranslationTableParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("MeasureParams")>]
            MeasureParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = MeasureParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("AmbiguousResidueParams")>]
            AmbiguousResidueParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = AmbiguousResidueParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("MassTableParams")>]
            MassTableParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = MassTableParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("IonTypeParams")>]
            IonTypeParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = IonTypeParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("SpecificityRuleParams")>]
            SpecificityRuleParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = SpecificityRuleParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("SearchModificationParams")>]
            SearchModificationParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = SearchModificationParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("EnzymeNameParams")>]
            EnzymeNameParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = EnzymeNameParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("IncludeParams")>]
            IncludeParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = IncludeParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("ExcludeParams")>]
            ExcludeParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = ExcludeParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("AdditionalSearchParams")>]
            AdditionalSearchParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = AdditionalSearchParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("FragmentToleranceParams")>]
            FragmentToleranceParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = FragmentToleranceParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("ParentToleranceParams")>]
            ParentToleranceParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = ParentToleranceParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("ThresholdParams")>]
            ThresholdParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = ThresholdParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("SearchDatabaseParams")>]
            SearchDatabaseParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = SearchDatabaseParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("DBSequenceParams")>]
            DBSequenceParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = DBSequenceParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("PeptideEvidenceParams")>]
            PeptideEvidenceParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = PeptideEvidenceParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("SpectrumIdentificationItemParams")>]
            SpectrumIdentificationItemParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = SpectrumIdentificationItemParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("SpectrumIdentificationResultParams")>]
            SpectrumIdentificationResultParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = SpectrumIdentificationResultParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("SpectrumIdentificationListParams")>]
            SpectrumIdentificationListParam(id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = SpectrumIdentificationListParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("AnalysisParams")>]
            AnalysisParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = AnalysisParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("SourceFileParams")>]
            SourceFileParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = SourceFileParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("ProteinDetectionHypothesisParams")>]
            ProteinDetectionHypothesisParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = ProteinDetectionHypothesisParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("ProteinAmbiguityGroupParams")>]
            ProteinAmbiguityGroupParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = ProteinAmbiguityGroupParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A single entry from an ontology or a controlled vocabulary.
        and [<AllowNullLiteral>] [<Table("ProteinDetectionListParams")>]
            ProteinDetectionListParam (id:int, name:string, value:string, term:Term, unit:Term, unitName:string, rowVersion:DateTime) =
                inherit CVParam()
                let mutable id'         = id
                let mutable name'       = name
                let mutable value'      = value
                let mutable term'       = term
                let mutable unit'       = unit
                let mutable unitName'   = unitName
                let mutable rowVersion' = rowVersion
                new() = ProteinDetectionListParam()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.Term with get() = term' and set(value) = term' <- value
                member this.Unit with get() = unit' and set(value) = unit' <- value
                member this.UnitName with get() = unitName' and set(value) = unitName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        /////A single entry from an ontology or a controlled vocabulary.
        //and [<CLIMutable>] 
        //    UserParam =
        //    {
        //     [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
        //     ID         : int
        //     Name       : string
        //     Value      : string
        //     and       : string
        //     Unit       : Term
        //     UnitName   : string
        //     RowVersion : DateTime 
        //    }

        ///Organizations are entities like companies, universities, government agencies.
        and [<AllowNullLiteral>]
            Organization (id:string, name:string, details:List<OrganizationParam>, parent:string, rowVersion:DateTime) =
                let mutable id' = id
                let mutable name' = name
                let mutable details' = details
                let mutable parent' = parent
                let mutable rowVersion' = rowVersion
                new() = Organization()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Parent with get() = parent' and set(value) = parent' <- value
                member this.Details with get() = details' and set(value) = details' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A person's name and contact details.
        and [<AllowNullLiteral>]
            Person (id:string, name:string, firstName:string, midInitials:string, 
                    lastName:string, organizations:List<Organization>, 
                    details:List<PersonParam>, rowVersion:DateTime) =
                let mutable id' = id
                let mutable name' = name
                let mutable firstName' = firstName
                let mutable midInitials' = midInitials
                let mutable lastName' = lastName
                let mutable organizations' = organizations
                let mutable details' = details
                let mutable rowVersion' = rowVersion
                new() = Person()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.FisrstName with get() = firstName' and set(value) = firstName' <- value
                member this.MidInitials with get() = midInitials' and set(value) = midInitials' <- value
                member this.LastName with get() = lastName' and set(value) = lastName' <- value
                member this.Organizations with get() = organizations' and set(value) = organizations' <- value
                member this.Details with get() = details' and set(value) = details' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///The software used for performing the analyses.
        and [<AllowNullLiteral>]
            ContactRole (id:string, name:string, role:CVParam, rowVersion:DateTime) =
                let mutable id' = id
                let mutable name' = name
                let mutable role' = role
                let mutable rowVersion' = rowVersion
                new() = ContactRole()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Role with get() = role' and set(value) = role' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
        ///The software used for performing the analyses.
        and [<AllowNullLiteral>]
            AnalysisSoftware (id:string, name:string, uri:string, version:string, customizations:string, 
                              contactRole:ContactRole, softwareName:CVParam, rowVersion:DateTime) =
                let mutable id' = id
                let mutable name' = name
                let mutable uri' = uri
                let mutable version' = version
                let mutable customization' = contactRole
                let mutable softwareName' = softwareName
                let mutable rowVersion' = rowVersion
                new() = AnalysisSoftware()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.URI with get() = uri' and set(value) = uri' <- value
                member this.Version with get() = version' and set(value) = version' <- value
                member this.Customization with get() = customization' and set(value) = customization' <- value
                member this.SoftwareName with get() = softwareName' and set(value) = softwareName' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///References to the individual component samples within a mixed parent sample.
        and [<AllowNullLiteral>]
            SubSample (id:string, sample:Sample, rowVersion:DateTime) =
                let mutable id' = id
                let mutable sample' = sample
                let mutable rowVersion' = rowVersion
                new() = SubSample()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Sample with get() = sample' and set(value) = sample' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A description of the sample analysed by mass spectrometry using CVParams or UserParams.
        and [<AllowNullLiteral>]
            Sample (id:string, name:string, contactRoles:List<CVParam>, subSamples:List<SubSample>, 
                    details:List<SampleParam>, rowVersion:DateTime) =
                let mutable id' = id
                let mutable name' = name
                let mutable contactRoles' = contactRoles
                let mutable subSamples' = subSamples
                let mutable details' = details
                let mutable rowVersion' = rowVersion
                new() = Sample()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.ContactRoles with get() = contactRoles' and set(value) = contactRoles' <- value
                member this.SubSamples with get() = subSamples' and set(value) = subSamples' <- value
                member this.Details with get() = details' and set(value) = details' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A molecule modification specification.
        and [<AllowNullLiteral>]
            Modification (id:string, residues:string, location:int, monoIsotopicMassDelta:float, 
                          avgMassDelta:float, details:List<ModificationParam>, rowVersion:DateTime) =
                let mutable id' = id
                let mutable residues' = residues
                let mutable location' = location
                let mutable monoIsotopicMassDelta' = monoIsotopicMassDelta
                let mutable avgMassDelta' = avgMassDelta
                let mutable details' = details
                let mutable rowVersion' = rowVersion
                new() = Modification()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Resiudes with get() = residues' and set(value) = residues' <- value
                member this.Location with get() = location' and set(value) = location' <- value
                member this.MonoIsotopicMassDelta with get() = monoIsotopicMassDelta' and set(value) = monoIsotopicMassDelta' <- value
                member this.AvgMassDelta with get() = avgMassDelta' and set(value) = avgMassDelta' <- value
                member this.Details with get() = details' and set(value) = details' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A modification where one residue is substituted by another (amino acid change).
        and [<AllowNullLiteral>]
            SubstitutionModification (id:string, originalResidue:string, replacementResidue:string, location:int, 
                                      monoIsotopicMassDelta:float, avgMassDelta:float, rowVersion:DateTime) =
                let mutable id' = id
                let mutable originalResidue' = originalResidue
                let mutable replacementResidue' = replacementResidue
                let mutable location' = location
                let mutable monoIsotopicMassDelta' = monoIsotopicMassDelta
                let mutable avgMassDelta' = avgMassDelta
                let mutable rowVersion' = rowVersion
                new() = SubstitutionModification()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.OriginalResidue with get() = originalResidue' and set(value) = originalResidue' <- value
                member this.ReplacementResidue with get() = replacementResidue' and set(value) = replacementResidue' <- value
                member this.Location with get() = location' and set(value) = location' <- value
                member this.MonoIsotopicMassDelta with get() = monoIsotopicMassDelta' and set(value) = monoIsotopicMassDelta' <- value
                member this.AvgMassDelta with get() = avgMassDelta' and set(value) = avgMassDelta' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///One (poly)peptide (a sequence with modifications).
        and [<AllowNullLiteral>]
            Peptide (id:string, name:string, peptideSequence:string, modifications:List<Modification>, 
                     substitutionModifications:List<SubstitutionModification>, details:List<PeptideParam>, rowVersion:DateTime) =
                let mutable id' = id
                let mutable name' = name
                let mutable peptideSequence' = peptideSequence
                let mutable modifications' = modifications
                let mutable substitutionModifications' = substitutionModifications
                let mutable details' = details
                let mutable rowVersion' = rowVersion
                new() = Peptide()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.PeptideSequence with get() = peptideSequence' and set(value) = peptideSequence' <- value
                member this.Modifications with get() = modifications' and set(value) = modifications' <- value
                member this.SubstitutionModifications with get() = substitutionModifications' and set(value) = substitutionModifications' <- value
                member this.Details with get() = details' and set(value) = details' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///PeptideEvidence links a specific Peptide element to a specific position in a DBSequence.
        and [<AllowNullLiteral>]
            TranslationTable (id:string, name:string, details:List<TranslationTableParam>, rowVersion:DateTime) =
                let mutable id' = id
                let mutable name' = name
                let mutable details' = details
                let mutable rowVersion' = rowVersion
                new() = TranslationTable()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Details with get() = details' and set(value) = details' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///References to CV terms defining the measures about product ions to be reported in SpectrumIdentificationItem.
        and [<AllowNullLiteral>]
            Measure (id:string, name:string, details:List<MeasureParam>, rowVersion:DateTime) =
                let mutable id' = id
                let mutable name' = name
                let mutable details' = details
                let mutable rowVersion' = rowVersion
                new() = Measure()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Details with get() = details' and set(value) = details' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///The specification of a single residue within the mass table.
        and [<AllowNullLiteral>]
            Residue (id:string, code:string, mass:float, rowVersion:DateTime) =
                let mutable id' = id
                let mutable code' = code
                let mutable mass' = mass
                let mutable rowVersion' = rowVersion
                new() = Residue()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Code with get() = code' and set(value) = code' <- value
                member this.Mass with get() = mass' and set(value) = mass' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///Ambiguous residues e.g. X can be specified by the Code attribute and a set of parameters for example giving the different masses that will be used in the search.
        and [<AllowNullLiteral>]
            AmbiguousResidue (id:string, code:string, details:List<AmbiguousResidueParam>, rowVersion:DateTime) =
                let mutable id' = id
                let mutable code' = code
                let mutable details' = details
                let mutable rowVersion' = rowVersion
                new() = AmbiguousResidue()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Code with get() = code' and set(value) = code' <- value
                member this.Details with get() = details' and set(value) = details' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///The masses of residues used in the search.
        and [<AllowNullLiteral>]
            MassTable (id:string, name:string, msLevel:string, residues:List<Residue>, 
                       ambiguousResidues:List<AmbiguousResidue>, details:List<MassTableParam>, rowVersion:DateTime) =
                let mutable id' = id
                let mutable name' = name
                let mutable msLevel' = msLevel
                let mutable residues' = residues
                let mutable ambiguousResidues' = ambiguousResidues
                let mutable details' = details
                let mutable rowVersion' = rowVersion
                new() = MassTable()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.MSLevel with get() = msLevel' and set(value) = msLevel' <- value
                member this.Residues with get() = residues' and set(value) = residues' <- value
                member this.AmbiguousResidues with get() = ambiguousResidues' and set(value) = ambiguousResidues' <- value
                member this.Details with get() = details' and set(value) = details' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///The values of this particular measure, corresponding to the index defined in ion and.
        and [<AllowNullLiteral>]
            Value (id:string, value:Nullable<float>, rowVersion:DateTime) =
                let mutable id' = id
                let mutable value' = value
                let mutable rowVersion' = rowVersion
                new() = Value()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Value with get() = value' and set(value) = value' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///An array of values for a given and of measure and for a particular ion and, in parallel to the index of ions identified.
        and [<AllowNullLiteral>]
            FragmentArray (id:string, measure:Measure, values:List<Value>, rowVersion:DateTime) =
                let mutable id' = id
                let mutable measure' = measure
                let mutable values' = values
                let mutable rowVersion' = rowVersion
                new() = FragmentArray()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Measure with get() = measure' and set(value) = measure' <- value
                member this.Values with get() = values' and set(value) = values' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///The index of ions identified as integers, following standard notation for a-c, x-z e.g. if b3 b5 and b6 have been identified, the index would store "3 5 6".
        and [<AllowNullLiteral>]
            Index (id:string, index:Nullable<int>, rowVersion:DateTime) =
                let mutable id' = id
                let mutable index' = index
                let mutable rowVersion' = rowVersion
                new() = Index()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Index with get() = index' and set(value) = index' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///Iontype defines the index of fragmentation ions being reported, importing a CV term for the and of ion e.g. b ion. Example: if b3 b7 b8 and b10 have been identified, the index attribute will contain 3 7 8 10.
        and [<AllowNullLiteral>]
            IonType (id:string, index:List<Index>, fragmentArray:List<FragmentArray>, details:List<IonTypeParam>, rowVersion:DateTime) =
                let mutable id' = id
                let mutable index' = index
                let mutable fragmentArray' = fragmentArray
                let mutable details' = details
                let mutable rowVersion' = rowVersion
                new() = IonType()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Index with get() = index' and set(value) = index' <- value
                member this.FragmentArray with get() = fragmentArray' and set(value) = fragmentArray' <- value
                member this.Details with get() = details' and set(value) = details' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///A data set containing spectra data (consisting of one or more spectra).
        and [<AllowNullLiteral>]
            SpectraData (id:string, name:string, location:string, externalFormatDocumentation:string, 
                         fileFormat:CVParam, spectrumIDFormat:CVParam, rowVersion:DateTime) =
                let mutable id' = id
                let mutable name' = name
                let mutable location' = location
                let mutable externalFormatDocumentation' = externalFormatDocumentation
                let mutable fileFormat' = fileFormat
                let mutable spectrumIDFormat' = spectrumIDFormat
                let mutable rowVersion' = rowVersion
                new() = SpectraData()
                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Location with get() = location' and set(value) = location' <- value
                member this.ExternalFormatDocumentation with get() = externalFormatDocumentation' and set(value) = externalFormatDocumentation' <- value
                member this.FileFormat with get() = fileFormat' and set(value) = fileFormat' <- value
                member this.SpectrumIDFormat with get() = spectrumIDFormat' and set(value) = spectrumIDFormat' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        ///The specificity rules of the searched modification including for example the probability of a modification's presence or peptide or protein termini.
        and [<CLIMutable>]
                //Formerly Specificityrules
                SpecificityRule =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Details    : List<SpecificityRuleParam>
                RowVersion : DateTime
                }  
                //

        ///Specification of a search modification as parameter for a spectra search.
        and [<CLIMutable>] 
                SearchModification =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                       : string
                FixedMod                 : bool
                MassDelta                : float
                Residues                 : string
                mutable SpecificityRules : List<SpecificityRule>
                Details                  : List<SearchModificationParam>
                RowVersion               : DateTime
                }

        ///The details of an individual cleavage enzyme should be provided by giving a regular expression or a CV term if a "standard" enzyme cleavage has been performed.
        and [<CLIMutable>] 
                Enzyme =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                      : string
                mutable Name            : string
                mutable CTermGain       : string
                mutable NTermGain       : string
                mutable MinDistance     : int
                mutable MissedCleavages : int
                mutable SemiSpecific    : bool
                mutable SiteRegexc      : string
                mutable EnzymeName      : List<EnzymeNameParam>
                RowVersion              : DateTime
                }

        ///Filters applied to the search database. The filter MUST include at least one of Include and Exclude.
        and [<CLIMutable>] 
                Filter =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID               : string
                FilterType       : CVParam
                mutable Includes : List<IncludeParam>
                mutable Excludes : List<ExcludeParam>
                RowVersion       : DateTime
                }

        ///The frames in which the nucleic acid sequence has been translated as a space separated list.
        and [<CLIMutable>] 
                Frame =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Frame      : int
                RowVersion : DateTime
                }

        ///The parameters and settings of a SpectrumIdentification analysis.
        and [<CLIMutable>] 
                SpectrumIdentificationProtocol =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                             : string
                mutable Name                   : string
                //Formerly AnalysisSoftware_Ref
                AnalysisSoftware               : AnalysisSoftware
                //
                SearchType                     : CVParam
                mutable AdditionalSearchParams : List<AdditionalSearchParam>
                mutable ModificationParams     : List<SearchModification>
                //Formerly Enzymes
                mutable Enzymes                : List<Enzyme>
                mutable Independent_Enzymes    : bool
                //
                mutable MassTables             : List<MassTable>
                mutable FragmentTolerance      : List<FragmentToleranceParam>
                mutable ParentTolerance        : List<ParentToleranceParam>
                Threshold                      : List<ThresholdParam>
                mutable DatabaseFilters        : List<Filter>
                //DatabaseTranlation
                mutable Frames                 : List<Frame>
                mutable TranslationTables      : List<TranslationTable>
                //
                RowVersion                     : DateTime
                }

        ///A database for searching mass spectra.
        and [<CLIMutable>] 
                SearchDatabase =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                  : string
                mutable Name                        : string
                Location                            : string
                mutable NumDatabaseSequences        : int64
                mutable NumResidues                 : int64
                mutable ReleaseDate                 : DateTime
                mutable Version                     : string
                mutable ExternalFormatDocumentation : string
                FileFormat                          : CVParam
                DatabaseName                        : CVParam
                mutable Details                     : List<SearchDatabaseParam>
                RowVersion                          : DateTime
                }

        ///A database sequence from the specified SearchDatabase (nucleic acid or amino acid).
        and [<CLIMutable>] 
                DBSequence =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                : string
                mutable Name      : string
                Accession         : string
                SearchDatabase    : SearchDatabase
                mutable Sequence  : string
                mutable Length    : int
                mutable Details   : List<DBSequenceParam>
                RowVersion        : DateTime
                }

        ///PeptideEvidence links a specific Peptide element to a specific position in a DBSequence.
        and [<CLIMutable>] 
                PeptideEvidence =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                 : string
                mutable Name                       : string
                //Formerly DBSequence_Ref
                DBSequence                         : DBSequence
                //
                //Formerly Peptide_Ref
                Peptide                            : Peptide
                //
                mutable Start                      : int
                mutable End                        : int
                mutable Pre                        : string
                mutable Post                       : string
                mutable Frame                      : Frame
                mutable IsDecoy                    : bool
                //Formerly TranslationTable_Ref
                mutable TranslationTable           : TranslationTable
                //
                mutable Details                    : List<PeptideEvidenceParam>
                RowVersion                         : DateTime
                }

        ///An identification of a single (poly)peptide, resulting from querying an input spectra, along with the set of confidence values for that identification.
        and [<CLIMutable>] 
                SpectrumIdentificationItem =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                   : string
                mutable Name                         : string
                mutable Sample                       : Sample
                mutable MassTable                    : MassTable
                PassThreshold                        : bool
                Rank                                 : int
                mutable PeptideEvidences             : List<PeptideEvidence>
                mutable Fragmentations               : List<IonType>
                Peptide                              : Peptide
                ChargeState                          : int
                ExperimentalMassToCharge             : float
                mutable CalculatedMassToCharge       : float
                mutable CalculatedPI                 : float
                mutable Details                      : List<SpectrumIdentificationItemParam>
                RowVersion                           : DateTime
                }

        ///All identifications made from searching one spectrum.
        and [<CLIMutable>] 
                SpectrumIdentificationResult =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                 : string
                mutable Name                       : string
                SpectraData                        : SpectraData
                SpectrumID                         : string
                SpectrumIdentificationItem         : List<SpectrumIdentificationItem>
                mutable Details                    : List<SpectrumIdentificationResultParam>
                RowVersion                         : DateTime
                }

        ///Represents the set of all search results from SpectrumIdentification.
        and [<CLIMutable>] 
                SpectrumIdentificationList =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                           : string
                mutable Name                 : string
                mutable NumSequencesSearched : int64
                mutable FragmentationTables  : List<Measure>
                SpectrumIdentificationResult : List<SpectrumIdentificationResult>
                mutable Details              : List<SpectrumIdentificationListParam>
                RowVersion                   : DateTime
                }


        ///An Analysis which tries to identify peptides in input spectra, referencing the database searched, the input spectra, the output results and the protocol that is run.
        and [<CLIMutable>] 
                SpectrumIdentification =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                             : string
                mutable Name                   : string
                mutable ActivityDate           : DateTime
                SpectrumIdentificationList     : SpectrumIdentificationList
                SpectrumIdentificationProtocol : SpectrumIdentificationProtocol
                //SpectraData_Ref
                SpectraData                    : List<SpectraData>
                //
                //SearchDatabase_Ref
                SearchDatabase                 : List<SearchDatabase>
                //
                RowVersion                     : DateTime
                }

        ///The parameters and settings of a SpectrumIdentification analysis.
        and [<CLIMutable>] 
                ProteinDetectionProtocol =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                     : string
                mutable Name           : string
                AnalysisSoftware       : AnalysisSoftware
                mutable AnalysisParams : List<AnalysisParam>
                Threshold              : List<ThresholdParam>
                RowVersion             : DateTime
                }

        ///A file from which this mzIdentML instance was created.
        and [<CLIMutable>] 
                SourceFile =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                  : string
                mutable Name                        : string
                Location                            : string
                mutable ExternalFormatDocumentation : string
                FileFormat                          : CVParam
                mutable Details                     : List<SourceFileParam>
                RowVersion                          : DateTime
                }

        ///The inputs to the analyses including the databases searched, the spectral data and the source file converted to mzIdentML.
        and [<CLIMutable>] 
                Inputs =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                      : string
                mutable SourceFiles     : List<SourceFile>
                mutable SearchDatabases : List<SearchDatabase>
                SpectraData             : List<SpectraData>
                RowVersion              : DateTime
                }

        ///Peptide evidence on which this ProteinHypothesis is based by reference to a PeptideEvidence element.
        and [<CLIMutable>] 
                PeptideHypothesis =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                          : string
                PeptideEvidence             : PeptideEvidence
                SpectrumIdentificationItems : List<SpectrumIdentificationItem>
                RowVersion                  : DateTime
                }

        ///A single result of the ProteinDetection analysis (i.e. a protein).
        and [<CLIMutable>] 
                ProteinDetectionHypothesis =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                : string
                mutable Name      : string
                PassThreshold     : bool
                DBSequence        : DBSequence
                PeptideHypothesis : List<PeptideHypothesis>
                mutable Details   : List<ProteinDetectionHypothesisParam>
                RowVersion        : DateTime
                }

        ///A set of logically related results from a protein detection, for example to represent conflicting assignments of peptides to proteins.
        and [<CLIMutable>] 
                ProteinAmbiguityGroup =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                        : string
                mutable Name              : string
                ProteinDetecionHypothesis : List<ProteinDetectionHypothesis>
                mutable Details           : List<ProteinAmbiguityGroupParam>
                RowVersion                : DateTime
                }

        ///The protein list resulting from a protein detection process.
        and [<CLIMutable>] 
                ProteinDetectionList =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                             : string
                mutable Name                   : string
                mutable ProteinAmbiguityGroups : List<ProteinAmbiguityGroup>
                mutable Details                : List<ProteinDetectionListParam>
                RowVersion                     : DateTime
                }

        ///Data sets generated by the analyses, including peptide and protein lists.
        and [<CLIMutable>] 
                AnalysisData =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                           : string
                SpectrumIdentificationList   : List<SpectrumIdentificationList>
                mutable ProteinDetectionList : ProteinDetectionList
                RowVersion                   : DateTime
                }

        ///An Analysis which assembles a set of peptides (e.g. from a spectra search analysis) to proteins. 
        and [<CLIMutable>] 
                ProteinDetection =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                           : string
                mutable Name                 : string
                mutable ActivityDate         : DateTime
                ProteinDetectionList         : ProteinDetectionList
                ProteinDetectionProtocol     : ProteinDetectionProtocol
                SpectrumIdentificationLists  : List<SpectrumIdentificationList>
                RowVersion                   : DateTime
                }

        ///Any bibliographic references associated with the file.
        and [<CLIMutable>] 
                BiblioGraphicReference =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                  : string
                mutable Name        : string
                mutable Authors     : string
                mutable DOI         : string
                mutable Editor      : string
                mutable Issue       : string
                mutable Pages       : string
                mutable Publication : string
                mutable Publisher   : string
                mutable Title       : string
                mutable Volume      : string
                mutable Year        : int
                RowVersion          : DateTime
                }

        ///The Provider of the mzIdentML record in terms of the contact and software.
        and [<CLIMutable>] 
                Provider =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                       : string
                mutable Name             : string
                //Formerly AnalysisSoftware_Ref
                mutable AnalysisSoftware : AnalysisSoftware
                //
                mutable ContactRole      : ContactRole
                RowVersion               : DateTime
                }

        and [<AllowNullLiteral>]
            MzIdentML(
                      id:int, version:string, spectrumIdentification:List<SpectrumIdentification>, 
                      spectrumIdentificationProtocol:List<SpectrumIdentificationProtocol>, inputs:Inputs, analysisData:AnalysisData, rowVersion:DateTime,
                      name:string, analysisSoftwares:List<AnalysisSoftware>, provider:Provider, persons:List<Person>, 
                      organizations:List<Organization>, samples:List<Sample>, dbSequences:List<DBSequence>, peptides:List<Peptide>,
                      peptideEvidences:List<PeptideEvidence>, proteinDetection:ProteinDetection, proteinDetectionProtocol:ProteinDetectionProtocol,
                      biblioGraphicReferences:List<BiblioGraphicReference>) =
                let mutable id' = id
                let mutable name' = name
                let mutable version' = version
                let mutable analysisSoftwares' = analysisSoftwares
                let mutable provider' = provider
                let mutable persons' = persons
                let mutable organizations' = organizations
                let mutable samples' = samples
                let mutable dbSequences' = dbSequences
                let mutable peptides' = peptides
                let mutable peptideEvidences' = peptideEvidences
                let mutable spectrumIdentification' = spectrumIdentification
                let mutable proteinDetection' = proteinDetection
                let mutable spectrumIdentificationProtocol' = spectrumIdentificationProtocol
                let mutable proteinDetectionProtocol' = proteinDetectionProtocol
                let mutable inputs' = inputs
                let mutable analysisData' = analysisData
                let mutable biblioGraphicReferences' = biblioGraphicReferences
                let mutable rowVersion' = rowVersion

                new() = MzIdentML()

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
    

    module DataContext =

        open EntityTypes

        type MzIdentMLContext =
     
                inherit DbContext

                new(options : DbContextOptions<MzIdentMLContext>) = {inherit DbContext(options)}

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
                member public this.Iontype with get() = this.m_IonType
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

                [<DefaultValue>] 
                val mutable m_SpecificityRule : DbSet<SpecificityRule>
                member public this.SpecificityRule with get() = this.m_SpecificityRule
                                                                and set value = this.m_SpecificityRule <- value

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
                val mutable m_MzIdentML : DbSet<MzIdentML>
                member public this.MzIdentML with get() = this.m_MzIdentML
                                                          and set value = this.m_MzIdentML <- value

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
                //         modelBuilder.Entity<CVParam>()
                //            .HasOne("OrganizationID")
                //            .WithMany("Details")
                //            .HasForeignKey("FK")
                //            .OnDelete(DeleteBehavior.Cascade)|> ignore
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
        let fileDir = __SOURCE_DIRECTORY__
        let standardDBPathSQLite = fileDir + "\Databases\Test.db"

        let configureSQLiteContextMzIdentML path = 
            let optionsBuilder = new DbContextOptionsBuilder<MzIdentMLContext>()
            optionsBuilder.UseSqlite(@"Data Source=" + path) |> ignore
            new MzIdentMLContext(optionsBuilder.Options)

        type OntologyContext =
     
                inherit DbContext

                new(options : DbContextOptions<OntologyContext>) = {inherit DbContext(options)}

                [<DefaultValue>] 
                val mutable m_term : DbSet<Term>
                member public this.Term with get() = this.m_term
                                                     and set value = this.m_term <- value
  
                [<DefaultValue>] 
                val mutable m_Ontology : DbSet<Ontology>
                member public this.Ontology with get() = this.m_Ontology
                                                         and set value = this.m_Ontology <- value 


        type AnalysisSoftwareContext =
     
                inherit DbContext

                new(options : DbContextOptions<AnalysisSoftwareContext>) = {inherit DbContext(options)}

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
                val mutable m_AnalysisSoftware : DbSet<AnalysisSoftware>
                member public this.AnalysisSoftware with get() = this.m_AnalysisSoftware
                                                                and set value = this.m_AnalysisSoftware <- value

