namespace MzIdentMLDataBase

open System
open System.ComponentModel.DataAnnotations.Schema
open Microsoft.EntityFrameworkCore
open System.Collections.Generic
open MzBasis.Basetypes

module DataModel =

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("CVParams")>]
        CVParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable termID'     = termID
            let mutable unit'       = unit
            let mutable unitID'     = unitID
            let mutable rowVersion' = rowVersion

            new() = CVParam(null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("OrganizationParams")>]
        OrganizationParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                           fkOrganization:string, rowVersion:Nullable<DateTime>
                          ) =
            let mutable id'             = id
            let mutable value'          = value
            let mutable term'           = term
            let mutable termID'         = termID
            let mutable unit'           = unit
            let mutable unitID'         = unitID
            let mutable fkOrganization' = fkOrganization
            let mutable rowVersion'     = rowVersion

            new() = OrganizationParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
             member this.FKOrganization with get() = fkOrganization' and set(value) = fkOrganization' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("PersonParams")>]
        PersonParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                     fkPerson:string, rowVersion:Nullable<DateTime>
                    ) =
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable termID'     = termID
            let mutable unit'       = unit
            let mutable unitID'     = unitID
            let mutable fkPerson'   = fkPerson
            let mutable rowVersion' = rowVersion

            new() = PersonParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKPerson with get() = fkPerson' and set(value) = fkPerson' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("SampleParams")>]
        SampleParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                     fkSample:string, rowVersion:Nullable<DateTime>
                    ) =
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable termID'     = termID
            let mutable unit'       = unit
            let mutable unitID'     = unitID
            let mutable fkSample'   = fkSample
            let mutable rowVersion' = rowVersion

            new() = SampleParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKSample with get() = fkSample' and set(value) = fkSample' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("ModificationParams")>]
        ModificationParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                           fkModification:string, rowVersion:Nullable<DateTime>
                          ) =
            let mutable id'             = id
            let mutable value'          = value
            let mutable term'           = term
            let mutable termID'         = termID
            let mutable unit'           = unit
            let mutable unitID'         = unitID
            let mutable fkModification' = fkModification
            let mutable rowVersion'     = rowVersion

            new() = ModificationParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKModification with get() = fkModification' and set(value) = fkModification' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("PeptideParams")>]
        PeptideParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                      fkPeptide:string, rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable termID'     = termID
            let mutable unit'       = unit
            let mutable unitID'     = unitID
            let mutable fkPeptide'  = fkPeptide
            let mutable rowVersion' = rowVersion

            new() = PeptideParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKPeptide with get() = fkPeptide' and set(value) = fkPeptide' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("TranslationTableParams")>]
        TranslationTableParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                               fkTranslationTable:string, rowVersion:Nullable<DateTime>
                              ) =
            let mutable id'                 = id
            let mutable value'              = value
            let mutable term'               = term
            let mutable termID'             = termID
            let mutable unit'               = unit
            let mutable unitID'             = unitID
            let mutable fkTranslationTable' = fkTranslationTable
            let mutable rowVersion'         = rowVersion

            new() = TranslationTableParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKTranslationTable with get() = fkTranslationTable' and set(value) = fkTranslationTable' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("MeasureParams")>]
        MeasureParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                      fkMeasure:string, rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable termID'     = termID
            let mutable unit'       = unit
            let mutable unitID'     = unitID
            let mutable fkMeasure'  = fkMeasure
            let mutable rowVersion' = rowVersion

            new() = MeasureParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKMeasure with get() = fkMeasure' and set(value) = fkMeasure' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("AmbiguousResidueParams")>]
        AmbiguousResidueParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                               fkAmbiguousResidue:string, rowVersion:Nullable<DateTime>
                              ) =
            let mutable id'                 = id
            let mutable value'              = value
            let mutable term'               = term
            let mutable termID'             = termID
            let mutable unit'               = unit
            let mutable unitID'             = unitID
            let mutable fkAmbiguousResidue' = fkAmbiguousResidue
            let mutable rowVersion'         = rowVersion

            new() = AmbiguousResidueParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKAmbiguousResidue with get() = fkAmbiguousResidue' and set(value) = fkAmbiguousResidue' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("MassTableParams")>]
        MassTableParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                        fkMassTable:string, rowVersion:Nullable<DateTime>
                       ) =
            let mutable id'             = id
            let mutable value'          = value
            let mutable term'           = term
            let mutable termID'         = termID
            let mutable unit'           = unit
            let mutable unitID'         = unitID
            let mutable fkMassTable'    = fkMassTable
            let mutable rowVersion'     = rowVersion

            new() = MassTableParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKMassTable with get() = fkMassTable' and set(value) = fkMassTable' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("IonTypeParams")>]
        IonTypeParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                      fkIonType:string, rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable termID'     = termID
            let mutable unit'       = unit
            let mutable unitID'     = unitID
            let mutable fkIonType'  = fkIonType
            let mutable rowVersion' = rowVersion

            new() = IonTypeParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKIonType with get() = fkIonType' and set(value) = fkIonType' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("SpecificityRuleParams")>]
        SpecificityRuleParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                              fkSearchModification:string, rowVersion:Nullable<DateTime>
                             ) =
            let mutable id'                     = id
            let mutable value'                  = value
            let mutable term'                   = term
            let mutable termID'                 = termID
            let mutable unit'                   = unit
            let mutable unitID'                 = unitID
            let mutable fkSearchModification'   = fkSearchModification
            let mutable rowVersion'             = rowVersion

            new() = SpecificityRuleParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKSearchModification with get() = fkSearchModification' and set(value) = fkSearchModification' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("SearchModificationParams")>]
        SearchModificationParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                                 fkSearchModification:string, rowVersion:Nullable<DateTime>
                                ) =
            let mutable id'                     = id
            let mutable value'                  = value
            let mutable term'                   = term
            let mutable termID'                 = termID
            let mutable unit'                   = unit
            let mutable unitID'                 = unitID
            let mutable fkSearchModification'   = fkSearchModification
            let mutable rowVersion'             = rowVersion

            new() = SearchModificationParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKSearchModification with get() = fkSearchModification' and set(value) = fkSearchModification' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("EnzymeNameParams")>]
        EnzymeNameParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                         fkEnzyme:string, rowVersion:Nullable<DateTime>
                        ) =
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable termID'     = termID
            let mutable unit'       = unit
            let mutable unitID'     = unitID
            let mutable fkEnzyme'   = fkEnzyme
            let mutable rowVersion' = rowVersion

            new() = EnzymeNameParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKEnzyme with get() = fkEnzyme' and set(value) = fkEnzyme' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("IncludeParams")>]
        IncludeParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                      fkFilter:string, rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable termID'     = termID
            let mutable unit'       = unit
            let mutable unitID'     = unitID
            let mutable fkFilter'   = fkFilter
            let mutable rowVersion' = rowVersion

            new() = IncludeParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKFilter with get() = fkFilter' and set(value) = fkFilter' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("ExcludeParams")>]
        ExcludeParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                      fkFilter:string, rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'         = id
            let mutable value'      = value
            let mutable term'       = term
            let mutable termID'     = termID
            let mutable unit'       = unit
            let mutable unitID'     = unitID
            let mutable fkFilter'   = fkFilter
            let mutable rowVersion' = rowVersion

            new() = ExcludeParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKFilter with get() = fkFilter' and set(value) = fkFilter' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("AdditionalSearchParams")>]
        AdditionalSearchParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                               fkSpectrumIdentificationProtocol:string, rowVersion:Nullable<DateTime>) =
            let mutable id'                                 = id
            let mutable value'                              = value
            let mutable term'                               = term
            let mutable termID'                             = termID
            let mutable unit'                               = unit
            let mutable unitID'                             = unitID
            let mutable fkSpectrumIdentificationProtocol'   = fkSpectrumIdentificationProtocol
            let mutable rowVersion'                         = rowVersion

            new() = AdditionalSearchParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKSpectrumIdentificationProtocol with get() = fkSpectrumIdentificationProtocol' and set(value) = fkSpectrumIdentificationProtocol' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("FragmentToleranceParams")>]
        FragmentToleranceParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                                fkSpectrumIdentificationProtocol:string, rowVersion:Nullable<DateTime>
                               ) =
            let mutable id'                                 = id
            let mutable value'                              = value
            let mutable term'                               = term
            let mutable termID'                             = termID
            let mutable unit'                               = unit
            let mutable unitID'                             = unitID
            let mutable fkSpectrumIdentificationProtocol' = fkSpectrumIdentificationProtocol
            let mutable rowVersion'                         = rowVersion

            new() = FragmentToleranceParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKSpectrumIdentificationProtocol with get() = fkSpectrumIdentificationProtocol' and set(value) = fkSpectrumIdentificationProtocol' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("ParentToleranceParams")>]
        ParentToleranceParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                              fkSpectrumIdentificationProtocol:string, rowVersion:Nullable<DateTime>
                             ) =
            let mutable id'                                 = id
            let mutable value'                              = value
            let mutable term'                               = term
            let mutable termID'                             = termID
            let mutable unit'                               = unit
            let mutable unitID'                             = unitID
            let mutable fkSpectrumIdentificationProtocol' = fkSpectrumIdentificationProtocol
            let mutable rowVersion'                         = rowVersion

            new() = ParentToleranceParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKSpectrumIdentificationProtocol with get() = fkSpectrumIdentificationProtocol' and set(value) = fkSpectrumIdentificationProtocol' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("ThresholdParams")>]
        ThresholdParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                        fkSpectrumIdentificationProtocol:string, fkProteinDetectionProtocol:string,
                        rowVersion:Nullable<DateTime>
                       ) =
            let mutable id'                                 = id
            let mutable value'                              = value
            let mutable term'                               = term
            let mutable termID'                             = termID
            let mutable unit'                               = unit
            let mutable unitID'                             = unitID
            let mutable fkSpectrumIdentificationProtocol'   = fkSpectrumIdentificationProtocol
            let mutable fkProteinDetectionProtocol'         = fkProteinDetectionProtocol
            let mutable rowVersion'                         = rowVersion

            new() = ThresholdParam(null, null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKSpectrumIdentificationProtocol with get() = fkSpectrumIdentificationProtocol' and set(value) = fkSpectrumIdentificationProtocol' <- value
            member this.FKProteinDetectionProtocol with get() = fkProteinDetectionProtocol' and set(value) = fkProteinDetectionProtocol' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("SearchDatabaseParams")>]
        SearchDatabaseParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                             fkSearchDatabase:string, rowVersion:Nullable<DateTime>
                            ) =
            let mutable id'                 = id
            let mutable value'              = value
            let mutable term'               = term
            let mutable termID'             = termID
            let mutable unit'               = unit
            let mutable unitID'             = unitID
            let mutable fkSearchDatabase'   = fkSearchDatabase
            let mutable rowVersion'         = rowVersion

            new() = SearchDatabaseParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
             member this.FKSearchDatabase with get() = fkSearchDatabase' and set(value) = fkSearchDatabase' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("DBSequenceParams")>]
        DBSequenceParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                         fkDBSequence:string, rowVersion:Nullable<DateTime>) =
            let mutable id'             = id
            let mutable value'          = value
            let mutable term'           = term
            let mutable termID'         = termID
            let mutable unit'           = unit
            let mutable unitID'         = unitID
            let mutable fkDBSequence'   = fkDBSequence
            let mutable rowVersion'     = rowVersion

            new() = DBSequenceParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKDBSequence with get() = fkDBSequence' and set(value) = fkDBSequence' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("PeptideEvidenceParams")>]
        PeptideEvidenceParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                              fkPeptideEvidence:string, rowVersion:Nullable<DateTime>
                             ) =
            let mutable id'                 = id
            let mutable value'              = value
            let mutable term'               = term
            let mutable termID'             = termID
            let mutable unit'               = unit
            let mutable unitID'             = unitID
            let mutable fkPeptideEvidence'  = fkPeptideEvidence
            let mutable rowVersion'         = rowVersion

            new() = PeptideEvidenceParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKPeptideEvidence with get() = fkPeptideEvidence' and set(value) = fkPeptideEvidence' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("SpectrumIdentificationItemParams")>]
        SpectrumIdentificationItemParam (id:string, value:string, term:Term, termID:string, unit:Term,
                                         unitID:string, fkSpectrumIdentificationItem:string, 
                                         rowVersion:Nullable<DateTime>
                                        ) =
            let mutable id'                             = id
            let mutable value'                          = value
            let mutable term'                           = term
            let mutable termID'                         = termID
            let mutable unit'                           = unit
            let mutable unitID'                         = unitID
            let mutable fkSpectrumIdentificationItem'   = fkSpectrumIdentificationItem
            let mutable rowVersion'                     = rowVersion

            new() = SpectrumIdentificationItemParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKSpectrumIdentificationItem with get() = fkSpectrumIdentificationItem' and set(value) = fkSpectrumIdentificationItem' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("SpectrumIdentificationResultParams")>]
        SpectrumIdentificationResultParam (id:string, value:string, term:Term, termID:string, unit:Term, 
                                           unitID:string, fkSpectrumIdentificationResult:string,
                                           rowVersion:Nullable<DateTime>) =
            let mutable id'                             = id
            let mutable value'                          = value
            let mutable term'                           = term
            let mutable termID'                         = termID
            let mutable unit'                           = unit
            let mutable unitID'                         = unitID
            let mutable fkSpectrumIdentificationResult' = fkSpectrumIdentificationResult
            let mutable rowVersion'                     = rowVersion

            new() = SpectrumIdentificationResultParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKSpectrumIdentificationResult with get() = fkSpectrumIdentificationResult' and set(value) = fkSpectrumIdentificationResult' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("SpectrumIdentificationListParams")>]
        SpectrumIdentificationListParam(id:string, value:string, term:Term, termID:string, unit:Term, 
                                        unitID:string, fkSpectrumIdentificationList:string, 
                                        rowVersion:Nullable<DateTime>
                                       ) =
            let mutable id'                             = id
            let mutable value'                          = value
            let mutable term'                           = term
            let mutable termID'                         = termID
            let mutable unit'                           = unit
            let mutable unitID'                         = unitID
            let mutable fkSpectrumIdentificationList'   = fkSpectrumIdentificationList
            let mutable rowVersion'                     = rowVersion

            new() = SpectrumIdentificationListParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKSpectrumIdentificationList with get() = fkSpectrumIdentificationList' and set(value) = fkSpectrumIdentificationList' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("AnalysisParams")>]
        AnalysisParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                       fkProteinDetectionProtocol:string, rowVersion:Nullable<DateTime>
                      ) =
            let mutable id'                         = id
            let mutable value'                      = value
            let mutable term'                       = term
            let mutable termID'                     = termID
            let mutable unit'                       = unit
            let mutable unitID'                     = unitID
            let mutable fkProteinDetectionProtocol' = fkProteinDetectionProtocol
            let mutable rowVersion'                 = rowVersion

            new() = AnalysisParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKProteinDetectionProtocol with get() = fkProteinDetectionProtocol' and set(value) = fkProteinDetectionProtocol' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("SourceFileParams")>]
        SourceFileParam (id:string, value:string, term:Term, termID:string, unit:Term, unitID:string, 
                         fkSourceFile:string, rowVersion:Nullable<DateTime>
                        ) =
            let mutable id'             = id
            let mutable value'          = value
            let mutable term'           = term
            let mutable termID'         = termID
            let mutable unit'           = unit
            let mutable unitID'         = unitID
            let mutable fkSourceFile'   = fkSourceFile
            let mutable rowVersion'     = rowVersion

            new() = SourceFileParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKSourceFile with get() = fkSourceFile' and set(value) = fkSourceFile' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("ProteinDetectionHypothesisParams")>]
        ProteinDetectionHypothesisParam (id:string, value:string, term:Term, termID:string, unit:Term, 
                                         unitID:string, fkProteinDetectionHypothesis:string,
                                         rowVersion:Nullable<DateTime>
                                        ) =
            let mutable id'                             = id
            let mutable value'                          = value
            let mutable term'                           = term
            let mutable termID'                         = termID
            let mutable unit'                           = unit
            let mutable unitID'                         = unitID
            let mutable fkProteinDetectionHypothesis'   = fkProteinDetectionHypothesis
            let mutable rowVersion'                     = rowVersion

            new() = ProteinDetectionHypothesisParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKProteinDetectionHypothesis with get() = fkProteinDetectionHypothesis' and set(value) = fkProteinDetectionHypothesis' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("ProteinAmbiguityGroupParams")>]
        ProteinAmbiguityGroupParam (id:string, value:string, term:Term, termID:string, unit:Term, 
                                    unitID:string, fkProteinAmbiguityGroup:string, 
                                    rowVersion:Nullable<DateTime>
                                   ) =
            let mutable id'                         = id
            let mutable value'                      = value
            let mutable term'                       = term
            let mutable termID'                     = termID
            let mutable unit'                       = unit
            let mutable unitID'                     = unitID
            let mutable fkProteinAmbiguityGroup'    = fkProteinAmbiguityGroup
            let mutable rowVersion'                 = rowVersion

            new() = ProteinAmbiguityGroupParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKProteinAmbiguityGroup with get() = fkProteinAmbiguityGroup' and set(value) = fkProteinAmbiguityGroup' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion

    ///A single entry from an ontology or a controlled vocabulary.
    type [<AllowNullLiteral>] [<Table("ProteinDetectionListParams")>]
        ProteinDetectionListParam (id:string, value:string, term:Term, termID:string, unit:Term, 
                                   unitID:string, fkProteinDetectionList:string,
                                   rowVersion:Nullable<DateTime>
                                  ) =
            let mutable id'                     = id
            let mutable value'                  = value
            let mutable term'                   = term
            let mutable termID'                 = termID
            let mutable unit'                   = unit
            let mutable unitID'                 = unitID
            let mutable fkProteinDetectionList' = fkProteinDetectionList
            let mutable rowVersion'             = rowVersion

            new() = ProteinDetectionListParam(null, null, null, null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Value with get() = value' and set(value) = value' <- value
            member this.Term with get() = term' and set(value) = term' <- value
            [<ForeignKey("Term")>]
            member this.FKTerm with get() = termID' and set(value) = termID' <- value
            member this.Unit with get() = unit' and set(value) = unit' <- value
            [<ForeignKey("Unit")>]
            member this.FKUnit with get() = unitID' and set(value) = unitID' <- value
            member this.FKProteinDetectionList with get() = fkProteinDetectionList' and set(value) = fkProteinDetectionList' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.FKTerm     = x.FKTerm
                member x.Unit       = x.Unit
                member x.FKUnit     = x.FKUnit
                member x.RowVersion = x.RowVersion


        ////////////////////////////////////////////////////////////////////////////////////////////
        //End of params///////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////


    ///Organizations are entities like companies, universities, government agencies.
    type [<AllowNullLiteral>]
        Organization (id:string, name:string, parent:Organization, fkParent:string, fkPerson:string,
                      fkMzIdentMLDocument:string, details:List<OrganizationParam>, 
                      rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'                  = id
            let mutable name'                = name 
            let mutable parent'              = parent
            let mutable fkParent'            = fkParent
            let mutable fkPerson'            = fkPerson
            let mutable fkMzIdentMLDocument' = fkMzIdentMLDocument
            let mutable details'             = details
            let mutable rowVersion'          = rowVersion

            new() = Organization(null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Parent with get() = parent' and set(value) = parent' <- value
            [<ForeignKey("Parent")>]
            member this.FKParent with get() = fkParent' and set(value) = fkParent' <- value
            member this.FKPerson with get() = fkPerson' and set(value) = fkPerson' <- value 
            member this.FKMzIdentMLDocument with get() = fkMzIdentMLDocument' and set(value) = fkMzIdentMLDocument' <- value
            [<ForeignKey("FKOrganization")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A person's name and contact details.
    type [<AllowNullLiteral>]
        Person (id:string, name:string, firstName:string, midInitials:string, 
                lastName:string, organizations:List<Organization>, fkMzIdentMLDocument:string, 
                details:List<PersonParam>, rowVersion:Nullable<DateTime>
               ) =
            let mutable id'                  = id
            let mutable name'                = name
            let mutable firstName'           = firstName
            let mutable midInitials'         = midInitials
            let mutable lastName'            = lastName
            let mutable organizations'       = organizations
            let mutable details'             = details
            let mutable fkMzIdentMLDocument' = fkMzIdentMLDocument
            let mutable rowVersion'          = rowVersion

            new() = Person(null, null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.FirstName with get() = firstName' and set(value) = firstName' <- value
            member this.MidInitials with get() = midInitials' and set(value) = midInitials' <- value
            member this.LastName with get() = lastName' and set(value) = lastName' <- value
            [<ForeignKey("FKPerson")>]
            member this.Organizations with get() = organizations' and set(value) = organizations' <- value
            member this.FKMzIdentMLDocument with get() = fkMzIdentMLDocument' and set(value) = fkMzIdentMLDocument' <- value
            [<ForeignKey("FKPerson")>]
            member this.Details with get() = details' and set(value) = details' <- value            
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The software used for performing the analyses.
    type [<AllowNullLiteral>]
        ContactRole (id:string, person:Person, fkPerson:string, role:CVParam, fkRole:string, 
                     fkSample:string, rowVersion:Nullable<DateTime>
                    ) =
            let mutable id'             = id
            let mutable person'         = person
            let mutable fkPerson'       = fkPerson
            let mutable role'           = role
            let mutable fkRole'         = fkRole
            let mutable fkSample'       = fkSample
            let mutable rowVersion'     = rowVersion

            new() = ContactRole(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Person with get() = person' and set(value) = person' <- value
            [<ForeignKey("Person")>]
            member this.FKPerson with get() = fkPerson' and set(value) = fkPerson' <- value
            member this.Role with get() = role' and set(value) = role' <- value
            [<ForeignKey("Role")>]
            member this.FKRole with get() = fkRole' and set(value) = fkRole' <- value
            member this.FKSample with get() = fkSample' and set(value) = fkSample' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The software used for performing the analyses.
    type [<AllowNullLiteral>]
        AnalysisSoftware (id:string, name:string, uri:string, version:string, customizations:string, 
                          contactRole:ContactRole, fkContactRole:string, softwareName:CVParam, 
                          fkSoftwareName:string, rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'                = id
            let mutable name'              = name
            let mutable uri'               = uri
            let mutable version'           = version
            let mutable customization'     = customizations
            let mutable contactRole'       = contactRole
            let mutable fkContactRole'     = fkContactRole
            let mutable softwareName'      = softwareName
            let mutable fkSoftwareName'    = fkSoftwareName
            let mutable rowVersion'        = rowVersion

            new() = AnalysisSoftware(null, null, null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.URI with get() = uri' and set(value) = uri' <- value
            member this.Version with get() = version' and set(value) = version' <- value
            member this.Customizations with get() = customization' and set(value) = customization' <- value
            member this.ContactRole with get() = contactRole' and set(value) = contactRole' <- value
            [<ForeignKey("ContactRole")>]
            member this.FKContactRole with get() = fkContactRole' and set(value) = fkContactRole' <- value
            member this.SoftwareName with get() = softwareName' and set(value) = softwareName' <- value
            [<ForeignKey("SoftwareName")>]
            member this.FKSoftwareName with get() = fkSoftwareName' and set(value) = fkSoftwareName' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///References to the individual component samples within a mixed parent sample.
    type [<AllowNullLiteral>]
        SubSample (id:string, fkSample:string, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable fkSample'   = fkSample
            let mutable rowVersion' = rowVersion

            new() = SubSample(null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.FKSample with get() = fkSample' and set(value) = fkSample' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A description of the sample analysed by mass spectrometry using CVParams or UserParams.
    type [<AllowNullLiteral>]
        Sample (id:string, name:string, contactRoles:List<ContactRole>, subSamples:List<SubSample>, 
                fkMzIdentMLDocument:string, details:List<SampleParam>, rowVersion:Nullable<DateTime>
                ) =
            let mutable id'                  = id
            let mutable name'                = name
            let mutable contactRoles'        = contactRoles
            let mutable subSamples'          = subSamples 
            let mutable fkMzIdentMLDocument' = fkMzIdentMLDocument
            let mutable details'             = details
            let mutable rowVersion'          = rowVersion

            new() = Sample(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            [<ForeignKey("FKSample")>]
            member this.ContactRoles with get() = contactRoles' and set(value) = contactRoles' <- value
            [<ForeignKey("FKSample")>]
            member this.SubSamples with get() = subSamples' and set(value) = subSamples' <- value
            member this.FKMzIdentMLDocument with get() = fkMzIdentMLDocument' and set(value) = fkMzIdentMLDocument' <- value
            [<ForeignKey("FKSample")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A molecule modification specification.
    type [<AllowNullLiteral>]
        Modification (id:string, residues:string, location:Nullable<int>, monoIsotopicMassDelta:Nullable<float>, 
                      avgMassDelta:Nullable<float>, fkPeptide:string, details:List<ModificationParam>, 
                      rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'                    = id
            let mutable residues'              = residues
            let mutable location'              = location
            let mutable monoIsotopicMassDelta' = monoIsotopicMassDelta
            let mutable avgMassDelta'          = avgMassDelta
            let mutable fkPeptide'             = fkPeptide
            let mutable details'               = details
            let mutable rowVersion'            = rowVersion

            new() = Modification(null, null, Nullable(), Nullable(), Nullable(), null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Residues with get() = residues' and set(value) = residues' <- value
            member this.Location with get() = location' and set(value) = location' <- value
            member this.MonoIsotopicMassDelta with get() = monoIsotopicMassDelta' and set(value) = monoIsotopicMassDelta' <- value
            member this.AvgMassDelta with get() = avgMassDelta' and set(value) = avgMassDelta' <- value
            member this.FKPeptide with get() = fkPeptide' and set(value) = fkPeptide' <- value
            [<ForeignKey("FKModification")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A modification where one residue is substituted by another (amino acid change).
    type [<AllowNullLiteral>]
        SubstitutionModification (id:string, originalResidue:string, replacementResidue:string, 
                                  location:Nullable<int>, monoIsotopicMassDelta:Nullable<float>, 
                                  avgMassDelta:Nullable<float>, fkPeptide:string, rowVersion:Nullable<DateTime>
                                 ) =
            let mutable id'                    = id
            let mutable originalResidue'       = originalResidue
            let mutable replacementResidue'    = replacementResidue
            let mutable location'              = location
            let mutable monoIsotopicMassDelta' = monoIsotopicMassDelta
            let mutable avgMassDelta'          = avgMassDelta
            let mutable fkPeptide'             = fkPeptide
            let mutable rowVersion'            = rowVersion

            new() = SubstitutionModification(null, null, null, Nullable(), Nullable(), Nullable(), null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.OriginalResidue with get() = originalResidue' and set(value) = originalResidue' <- value
            member this.ReplacementResidue with get() = replacementResidue' and set(value) = replacementResidue' <- value
            member this.Location with get() = location' and set(value) = location' <- value
            member this.MonoIsotopicMassDelta with get() = monoIsotopicMassDelta' and set(value) = monoIsotopicMassDelta' <- value
            member this.AvgMassDelta with get() = avgMassDelta' and set(value) = avgMassDelta' <- value
            member this.FKPeptide with get() = fkPeptide' and set(value) = fkPeptide' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

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

    ///PeptideEvidence links a specific Peptide element to a specific position in a DBSequence.
    type [<AllowNullLiteral>]
        TranslationTable (id:string, name:string, fkSpectrumIdentificationProtocol:String,
                          details:List<TranslationTableParam>, rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'                                 = id
            let mutable name'                               = name
            let mutable fkSpectrumIdentificationProtocol'   = fkSpectrumIdentificationProtocol
            let mutable details'                            = details
            let mutable rowVersion'                         = rowVersion

            new() = TranslationTable(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.FKSpectrumIdentificationProtocol with get() = fkSpectrumIdentificationProtocol' and set(value) = fkSpectrumIdentificationProtocol' <- value
            [<ForeignKey("FKTranslationTable")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///References to CV terms defining the measures about product ions to be reported in SpectrumIdentificationItem.
    type [<AllowNullLiteral>]
        Measure (id:string, name:string, fkSpectrumIdentificationList:string, details:List<MeasureParam>, 
                 rowVersion:Nullable<DateTime>
                ) =
            let mutable id'                             = id
            let mutable name'                           = name
            let mutable fkSpectrumIdentificationList'   = fkSpectrumIdentificationList
            let mutable details'                        = details
            let mutable rowVersion'                     = rowVersion

            new() = Measure(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.FKSpectrumIdentificationList with get() = fkSpectrumIdentificationList' and set(value) = fkSpectrumIdentificationList' <- value
            [<ForeignKey("FKMeasure")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The specification of a single residue within the mass table.
    type [<AllowNullLiteral>]
        Residue (id:string, code:string, mass:Nullable<float>, fkMassTable:string, 
                 rowVersion:Nullable<DateTime>
                ) =
            let mutable id'             = id
            let mutable code'           = code
            let mutable mass'           = mass
            let mutable fkMassTable'    = fkMassTable
            let mutable rowVersion'     = rowVersion

            new() = Residue(null, null, Nullable(), null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Code with get() = code' and set(value) = code' <- value
            member this.Mass with get() = mass' and set(value) = mass' <- value
            member this.FKMassTable with get() = fkMassTable' and set(value) = fkMassTable' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Ambiguous residues e.g. X can be specified by the Code attribute and a set of parameters for example giving the different masses that will be used in the search.
    type [<AllowNullLiteral>]
        AmbiguousResidue (id:string, code:string, fkMassTable:string, details:List<AmbiguousResidueParam>,
                          rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'             = id
            let mutable code'           = code
            let mutable fkMassTable'    = fkMassTable
            let mutable details'        = details
            let mutable rowVersion'     = rowVersion

            new() = AmbiguousResidue(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Code with get() = code' and set(value) = code' <- value
            member this.FKMassTable with get() = fkMassTable' and set(value) = fkMassTable' <- value
            [<ForeignKey("FKAmbiguousResidue")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The masses of residues used in the search.
    type [<AllowNullLiteral>]
        MassTable (id:string, name:string, msLevel:string, residues:List<Residue>, 
                   ambiguousResidues:List<AmbiguousResidue>, details:List<MassTableParam>, 
                   rowVersion:Nullable<DateTime>
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
            [<ForeignKey("FKMassTable")>]
            member this.Residues with get() = residues' and set(value) = residues' <- value
            [<ForeignKey("FKMassTable")>]
            member this.AmbiguousResidues with get() = ambiguousResidues' and set(value) = ambiguousResidues' <- value
            [<ForeignKey("FKMassTable")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    /////The values of this particular measure, corresponding to the index defined in ion and.
    //type [<AllowNullLiteral>]
    //    Value (id:string, value:Nullable<float>, rowVersion:Nullable<DateTime>) =
    //        let mutable id'         = id
    //        let mutable value'      = value
    //        let mutable rowVersion' = rowVersion

    //        new() = Value(null, Nullable(), Nullable())

    //        member this.ID with get() = id' and set(value) = id' <- value
    //        member this.Value with get() = value' and set(value) = value' <- value
    //        member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///An array of values for a given and of measure and for a particular ion and, in parallel to the index of ions identified.
    type [<AllowNullLiteral>]
        FragmentArray (id:string, measure:Measure, fkMeasure:string, values:Nullable<float>, 
                       fkIonType:string, rowVersion:Nullable<DateTime>
                      ) =
            let mutable id'         = id
            let mutable measure'    = measure
            let mutable fkMeasure'  = fkMeasure
            let mutable values'     = values
            let mutable fkIonType'  = fkIonType
            let mutable rowVersion' = rowVersion

            new() = FragmentArray(null, null, null, Nullable(), null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Measure with get() = measure' and set(value) = measure' <- value
            [<ForeignKey("Measure")>]
            member this.FKMeasure with get() = fkMeasure' and set(value) = fkMeasure' <- value
            member this.Values with get() = values' and set(value) = values' <- value
            member this.FKIonType with get() = fkIonType' and set(value) = fkIonType' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The index of ions identified as integers, following standard notation for a-c, x-z e.g. if b3 b5 and b6 have been identified, the index would store "3 5 6".
    type [<AllowNullLiteral>]
        Index (id:string, index:Nullable<int>, fkIonType:string, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable index'      = index
            let mutable fkIonType'  = fkIonType
            let mutable rowVersion' = rowVersion

            new() = Index(null, Nullable(), null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Index with get() = index' and set(value) = index' <- value
            member this.FKIonType with get() = fkIonType' and set(value) = fkIonType' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///IonType defines the index of fragmentation ions being reported, importing a CV term for the and of ion e.g. b ion. Example: if b3 b7 b8 and b10 have been identified, the index attribute will contain 3 7 8 10.
    type [<AllowNullLiteral>]
        IonType (id:string, index:List<Index>, fragmentArrays:List<FragmentArray>, 
                 fkSpectrumIdentificationItem:string, details:List<IonTypeParam>, 
                 rowVersion:Nullable<DateTime>
                ) =
            let mutable id'                             = id
            let mutable index'                          = index
            let mutable fragmentArrays'                 = fragmentArrays
            let mutable fkSpectrumIdentificationItem'   = fkSpectrumIdentificationItem
            let mutable details'                        = details
            let mutable rowVersion'                     = rowVersion

            new() = IonType(null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            [<ForeignKey("FKIonType")>]
            member this.Index with get() = index' and set(value) = index' <- value
            [<ForeignKey("FKIonType")>]
            member this.FragmentArrays with get() = fragmentArrays' and set(value) = fragmentArrays' <- value
            member this.FKSpectrumIdentificationItem with get() = fkSpectrumIdentificationItem' and set(value) = fkSpectrumIdentificationItem' <- value
            [<ForeignKey("FKIonType")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A data set containing spectra data (consisting of one or more spectra).
    type [<AllowNullLiteral>]
        SpectraData (id:string, name:string, location:string, externalFormatDocumentation:string, 
                     fileFormat:CVParam, fkFileFormat:string, spectrumIDFormat:CVParam, 
                     fkSpectrumIDFormat:string, fkInputs:string, rowVersion:Nullable<DateTime>
                    ) =
            let mutable id'                          = id
            let mutable name'                        = name
            let mutable location'                    = location
            let mutable externalFormatDocumentation' = externalFormatDocumentation
            let mutable fileFormat'                  = fileFormat
            let mutable fkFileFormat'                = fkFileFormat
            let mutable spectrumIDFormat'            = spectrumIDFormat
            let mutable fkSpectrumIDFormat'          = fkSpectrumIDFormat
            let mutable fkInputs'                    = fkInputs
            let mutable rowVersion'                  = rowVersion

            new() = SpectraData(null, null, null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Location with get() = location' and set(value) = location' <- value
            member this.ExternalFormatDocumentation with get() = externalFormatDocumentation' and set(value) = externalFormatDocumentation' <- value
            member this.FileFormat with get() = fileFormat' and set(value) = fileFormat' <- value
            [<ForeignKey("FileFormat")>]
            member this.FKFileFormat with get() = fkFileFormat' and set(value) = fkFileFormat' <- value
            member this.SpectrumIDFormat with get() = spectrumIDFormat' and set(value) = spectrumIDFormat' <- value
            member this.FKInputs with get() = fkInputs' and set(value) = fkInputs' <- value
            [<ForeignKey("SpectrumIDFormat")>]
            member this.FKSpectrumIDFormat with get() = fkSpectrumIDFormat' and set(value) = fkSpectrumIDFormat' <- value
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
    type [<AllowNullLiteral>]
        SearchModification (id:string, fixedMod:Nullable<bool>, massDelta:Nullable<float>, residues:string, 
                            specificityRules:List<SpecificityRuleParam>, 
                            fkSpectrumIdentificationProtocol:string,
                            details:List<SearchModificationParam>,
                            rowVersion:Nullable<DateTime>
                           ) =
            let mutable id'                                 = id
            let mutable fixedMod'                           = fixedMod
            let mutable massDelta'                          = massDelta
            let mutable residues'                           = residues
            let mutable specificityRules'                   = specificityRules
            let mutable fkSpectrumIdentificationProtocol'   = fkSpectrumIdentificationProtocol
            let mutable details'                            = details
            let mutable rowVersion'                         = rowVersion

            new() = SearchModification(null, Nullable(), Nullable(), null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.FixedMod with get() = fixedMod' and set(value) = fixedMod' <- value
            member this.MassDelta with get() = massDelta' and set(value) = massDelta' <- value
            member this.Residues with get() = residues' and set(value) = residues' <- value
            [<ForeignKey("FKSearchModification")>]
            member this.SpecificityRules with get() = specificityRules' and set(value) = specificityRules' <- value
            member this.FKSpectrumIdentificationProtocol with get() = fkSpectrumIdentificationProtocol' and set(value) = fkSpectrumIdentificationProtocol' <- value
            [<ForeignKey("FKSearchModification")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The details of an individual cleavage enzyme should be provided by giving a regular expression or a CV term if a "standard" enzyme cleavage has been performed.
    type [<AllowNullLiteral>]
        Enzyme (id:string, name:string, cTermGain:string, nTermGain:string, minDistance:Nullable<int>, 
                missedCleavages:Nullable<int>, semiSpecific:Nullable<bool>, siteRegexc:string, 
                enzymeName:List<EnzymeNameParam>, fkSpectrumIdentificationProtocol:string,
                rowVersion:Nullable<DateTime>
               ) =
            let mutable id'                                 = id
            let mutable name'                               = name
            let mutable cTermGain'                          = cTermGain
            let mutable nTermGain'                          = nTermGain
            let mutable minDistance'                        = minDistance
            let mutable missedCleavages'                    = missedCleavages
            let mutable semiSpecific'                       = semiSpecific
            let mutable siteRegexc'                         = siteRegexc
            let mutable enzymeName'                         = enzymeName
            let mutable fkSpectrumIdentificationProtocol' = fkSpectrumIdentificationProtocol
            let mutable rowVersion'                         = rowVersion

            new() = Enzyme(null, null, null, null, Nullable(), Nullable(), Nullable(), null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.CTermGain with get() = cTermGain' and set(value) = cTermGain' <- value
            member this.NTermGain with get() = nTermGain' and set(value) = nTermGain' <- value
            member this.MinDistance with get() = minDistance' and set(value) = minDistance' <- value
            member this.MissedCleavages with get() = missedCleavages' and set(value) = missedCleavages' <- value
            member this.SemiSpecific with get() = semiSpecific' and set(value) = semiSpecific' <- value
            member this.SiteRegexc with get() = siteRegexc' and set(value) = siteRegexc' <- value
            [<ForeignKey("FKEnzyme")>]
            member this.EnzymeName with get() = enzymeName' and set(value) = enzymeName' <- value
            member this.FKSpectrumIdentificationProtocol with get() = fkSpectrumIdentificationProtocol' and set(value) = fkSpectrumIdentificationProtocol' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Filters applied to the search database. The filter MUST include at least one of Include and Exclude.
    type [<AllowNullLiteral>]
        Filter (id:string, filterType:CVParam, fkFilterType:string, includes:List<IncludeParam>, 
                excludes:List<ExcludeParam>, fkSpectrumIdentificationProtocol:string, 
                rowVersion:Nullable<DateTime>
               ) =
            let mutable id'                                 = id
            let mutable filterType'                         = filterType
            let mutable fkFilterType'                       = fkFilterType
            let mutable includes'                           = includes
            let mutable excludes'                           = excludes
            let mutable fkSpectrumIdentificationProtocol'   = fkSpectrumIdentificationProtocol
            let mutable rowVersion'                         = rowVersion

            new() = Filter(null, null, null, null, null,null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.FilterType with get() = filterType' and set(value) = filterType' <- value
            [<ForeignKey("FilterType")>]
            member this.FKFilterType with get() = fkFilterType' and set(value) = fkFilterType' <- value
            [<ForeignKey("FKFilter")>]
            member this.Includes with get() = includes' and set(value) = includes' <- value
            [<ForeignKey("FKFilter")>]
            member this.Excludes with get() = excludes' and set(value) = excludes' <- value
            member this.FKSpectrumIdentificationProtocol with get() = fkSpectrumIdentificationProtocol' and set(value) = fkSpectrumIdentificationProtocol' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The frames in which the nucleic acid sequence has been translated as a space separated list.
    type [<AllowNullLiteral>] 
        Frame (id:string, frame:Nullable<int>, fkSpectrumIdentificationProtocol:string, 
               rowVersion:Nullable<DateTime>
              ) =
            let mutable id'                                 = id
            let mutable frame'                              = frame
            let mutable fkSpectrumIdentificationProtocol'   = fkSpectrumIdentificationProtocol
            let mutable rowVersion'                         = rowVersion

            new() = Frame(null, Nullable(), null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Frame with get() = frame' and set(value) = frame' <- value
            member this.FKSpectrumIdentificationProtocol with get() = fkSpectrumIdentificationProtocol' and set(value) = fkSpectrumIdentificationProtocol' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The parameters and settings of a SpectrumIdentification analysis.
    type [<AllowNullLiteral>] 
        SpectrumIdentificationProtocol (id:string, name:string, analysisSoftware:AnalysisSoftware, 
                                        fkAnalysisSoftware:string, searchType:CVParam, fkSearchType:string,
                                        additionalSearchParams:List<AdditionalSearchParam>, 
                                        modificationParams:List<SearchModification>,
                                        enzymes:List<Enzyme>, independent_Enzymes:Nullable<bool>, 
                                        massTables:List<MassTable>,
                                        fragmentTolerance:List<FragmentToleranceParam>, 
                                        parentTolerance:List<ParentToleranceParam>,
                                        threshold:List<ThresholdParam>, databaseFilters:List<Filter>, 
                                        frames:List<Frame>, translationTables:List<TranslationTable>, 
                                        fkMzIdentMLDocument:string, rowVersion:Nullable<DateTime>
                                       ) =
            let mutable id'                     = id
            let mutable name'                   = name
            //Formerly AnalysisSoftware_Ref
            let mutable analysisSoftware'       = analysisSoftware
            let mutable fkAnalysisSoftware'     = fkAnalysisSoftware
            //
            let mutable searchType'             = searchType
            let mutable fkSearchType'           = fkSearchType
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
            let mutable fkMzIdentMLDocument'    = fkMzIdentMLDocument
            let mutable rowVersion'             = rowVersion

            new() = SpectrumIdentificationProtocol(null, null, null, null, null, null, null, null, null, 
                                                   Nullable(), null, null, null, null, null, null, null, 
                                                   null, Nullable()
                                                  )

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.AnalysisSoftware with get() = analysisSoftware' and set(value) = analysisSoftware' <- value
            [<ForeignKey("AnalysisSoftware")>]
            member this.FKAnalysisSoftware with get() = fkAnalysisSoftware' and set(value) = fkAnalysisSoftware' <- value
            member this.SearchType with get() = searchType' and set(value) = searchType' <- value
            [<ForeignKey("SearchType")>]
            member this.FKSearchType with get() = fkSearchType' and set(value) = fkSearchType' <- value
            [<ForeignKey("FKSpectrumIdentificationProtocol")>]
            member this.AdditionalSearchParams with get() = additionalSearchParams' and set(value) = additionalSearchParams' <- value
            [<ForeignKey("FKSpectrumIdentificationProtocol")>]
            member this.ModificationParams with get() = modificationParams' and set(value) = modificationParams' <- value
            [<ForeignKey("FKSpectrumIdentificationProtocol")>]
            member this.Enzymes with get() = enzymes' and set(value) = enzymes' <- value
            member this.Independent_Enzymes with get() = independent_Enzymes' and set(value) = independent_Enzymes' <- value
            [<ForeignKey("FKSpectrumIdentificationProtocol")>]
            member this.MassTables with get() = massTables' and set(value) = massTables' <- value
            [<ForeignKey("FKSpectrumIdentificationProtocol")>]
            member this.FragmentTolerance with get() = fragmentTolerance' and set(value) = fragmentTolerance' <- value
            [<ForeignKey("FKSpectrumIdentificationProtocol")>]
            member this.ParentTolerance with get() = parentTolerance' and set(value) = parentTolerance' <- value
            [<ForeignKey("FKSpectrumIdentificationProtocol")>]
            member this.Threshold with get() = threshold' and set(value) = threshold' <- value
            [<ForeignKey("FKSpectrumIdentificationProtocol")>]
            member this.DatabaseFilters with get() = databaseFilters' and set(value) = databaseFilters' <- value
            [<ForeignKey("FKSpectrumIdentificationProtocol")>]
            member this.Frames with get() = frames' and set(value) = frames' <- value
            [<ForeignKey("FKSpectrumIdentificationProtocol")>]
            member this.TranslationTables with get() = translationTables' and set(value) = translationTables' <- value
            member this.FKMzIdentMLDocument with get() = fkMzIdentMLDocument' and set(value) = fkMzIdentMLDocument' <- value  
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A database for searching mass spectra.
    type [<AllowNullLiteral>]
        SearchDatabase (id:string, name:string, location:string, numDatabaseSequences:Nullable<int64>, 
                        numResidues:Nullable<int64>, releaseDate:Nullable<DateTime>, version:string, 
                        externalFormatDocumentation:string, fileFormat:CVParam, fkFileFormat:string,
                        databaseName:CVParam, fkDatabaseName:string, fkInputs:string, 
                        details:List<SearchDatabaseParam>, rowVersion:Nullable<DateTime>
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
            let mutable fkFileFormat'                = fkFileFormat
            let mutable databaseName'                = databaseName
            let mutable fkDatabaseName'              = fkDatabaseName
            let mutable fkInputs'                    = fkInputs
            let mutable details'                     = details
            let mutable rowVersion'                  = rowVersion

            new() = SearchDatabase(null, null, null, Nullable(), Nullable(), Nullable(), null, null, null, null,
                                   null, null, null, null, Nullable()
                                  )

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Location with get() = location' and set(value) = location' <- value
            member this.NumDatabaseSequences with get() = numDatabaseSequences' and set(value) = numDatabaseSequences' <- value
            member this.NumResidues with get() = numResidues' and set(value) = numResidues' <- value
            member this.ReleaseDate with get() = releaseDate' and set(value) = releaseDate' <- value
            member this.Version with get() = version' and set(value) = version' <- value
            member this.ExternalFormatDocumentation with get() = externalFormatDocumentation' and set(value) = externalFormatDocumentation' <- value
            member this.FileFormat with get() = fileFormat' and set(value) = fileFormat' <- value
            [<ForeignKey("FileFormat")>]
            member this.FKFileFormat with get() = fkFileFormat' and set(value) = fkFileFormat' <- value
            member this.DatabaseName with get() = databaseName' and set(value) = databaseName' <- value
            [<ForeignKey("DatabaseName")>]
            member this.FKDatabaseName with get() = fkDatabaseName' and set(value) = fkDatabaseName' <- value
            member this.FKInputs with get() = fkInputs' and set(value) = fkInputs' <- value
            [<ForeignKey("FKSearchDatabase")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A database sequence from the specified SearchDatabase (nucleic acid or amino acid).
    type [<AllowNullLiteral>]
        DBSequence (id:string, name:string, accession:string, searchDatabase:SearchDatabase, 
                    fkSearchDatabase:string, sequence:string, length:Nullable<int>, 
                    fkMzIdentMLDocument:string, details:List<DBSequenceParam>, 
                    rowVersion:Nullable<DateTime>
                   ) =
            let mutable id'                     = id
            let mutable name'                   = name
            let mutable accession'              = accession
            let mutable searchDatabase'         = searchDatabase
            let mutable fkSearchDatabase'       = fkSearchDatabase
            let mutable sequence'               = sequence
            let mutable length'                 = length
            let mutable fkMzIdentMLDocument'    = fkMzIdentMLDocument
            let mutable details'                = details
            let mutable rowVersion'             = rowVersion

            new() = DBSequence(null, null, null, null, null, null, Nullable(), null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Accession with get() = accession' and set(value) = accession' <- value
            member this.SearchDatabase with get() = searchDatabase' and set(value) = searchDatabase' <- value
            [<ForeignKey("SearchDatabase")>]
            member this.FKSearchDatabase with get() = fkSearchDatabase' and set(value) = fkSearchDatabase' <- value
            member this.Sequence with get() = sequence' and set(value) = sequence' <- value
            member this.Length with get() = length' and set(value) = length' <- value
            member this.FKMzIdentMLDocument with get() = fkMzIdentMLDocument' and set(value) = fkMzIdentMLDocument' <- value
            [<ForeignKey("FKDBSequence")>]
            member this.Details with get() = details' and set(value) = details' <- value 
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///PeptideEvidence links a specific Peptide element to a specific position in a DBSequence.
    type [<AllowNullLiteral>]
        PeptideEvidence (id:string, name:string, dbSequence:DBSequence, fkDBSequence:string, 
                         peptide:Peptide, fkPeptide:string, start:Nullable<int>, ends:Nullable<int>, 
                         pre:string, post:string, frame:Frame, isDecoy:Nullable<bool>, 
                         translationTable:TranslationTable, fkTranslationTable:string, 
                         fkSpectrumIdentificationItem:string, fkMzIdentMLDocument:string, 
                         details:List<PeptideEvidenceParam>, rowVersion:Nullable<DateTime>
                        ) =
            let mutable id'                             = id
            let mutable name'                           = name
            //Formerly DBSequence_Ref
            let mutable dbSequence'                     = dbSequence
            let mutable fkDBSequence'                   = fkDBSequence
            //
            //Formerly Peptide_Ref
            let mutable peptide'                        = peptide
            let mutable fkPeptide'                      = fkPeptide
            //
            let mutable start'                          = start
            let mutable ends'                           = ends
            let mutable pre'                            = pre
            let mutable post'                           = post
            let mutable frame'                          = frame
            let mutable isDecoy'                        = isDecoy
            //Formerly TranslationTable_Ref
            let mutable translationTable'               = translationTable
            let mutable fkTranslationTable'             = fkTranslationTable
            //
            let mutable fkSpectrumIdentificationItem'   = fkSpectrumIdentificationItem
            let mutable fkMzIdentMLDocument'            = fkMzIdentMLDocument
            let mutable details'                        = details
            let mutable rowVersion'                     = rowVersion

            new() = PeptideEvidence(null, null, null, null, null, null, Nullable(), Nullable(), null, null, null,
                                    Nullable(), null, null, null, null, null, Nullable()
                                   )

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.DBSequence with get() = dbSequence' and set(value) = dbSequence' <- value
            [<ForeignKey("DBSequence")>]
            member this.FKDBSequence with get() = fkDBSequence' and set(value) = fkDBSequence' <- value
            member this.Peptide with get() = peptide' and set(value) = peptide' <- value
            [<ForeignKey("Peptide")>]
            member this.FKPeptide with get() = fkPeptide' and set(value) = fkPeptide' <- value
            member this.Start with get() = start' and set(value) = start' <- value
            member this.End with get() = ends' and set(value) = ends' <- value
            member this.Pre with get() = pre' and set(value) = pre' <- value
            member this.Post with get() = post' and set(value) = post' <- value
            member this.Frame with get() = frame' and set(value) = frame' <- value
            member this.IsDecoy with get() = isDecoy' and set(value) = isDecoy' <- value
            member this.TranslationTable with get() = translationTable' and set(value) = translationTable' <- value
            [<ForeignKey("TranslationTable")>]
            member this.FKTranslationTable with get() = fkTranslationTable' and set(value) = fkTranslationTable' <- value
            member this.FKSpectrumIdentificationItem with get() = fkSpectrumIdentificationItem' and set(value) = fkSpectrumIdentificationItem' <- value
            member this.FKMzIdentMLDocument with get() = fkMzIdentMLDocument' and set(value) = fkMzIdentMLDocument' <- value
            [<ForeignKey("FKPeptideEvidence")>]
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

    ///All identifications made from searching one spectrum.
    and [<AllowNullLiteral>]
        SpectrumIdentificationResult (id:string, name:string, spectraData:SpectraData, fkSpectraData:string, 
                                      spectrumID:string, spectrumIdentificationItem:List<SpectrumIdentificationItem>, 
                                      fkSpectrumIdentificationList:string,
                                      details:List<SpectrumIdentificationResultParam>,
                                      rowVersion:Nullable<DateTime>
                                     ) =
            let mutable id'                             = id
            let mutable name'                           = name
            let mutable spectraData'                    = spectraData
            let mutable fkSpectraData'                  = fkSpectraData
            let mutable spectrumID'                     = spectrumID
            let mutable spectrumIdentificationItem'     = spectrumIdentificationItem
            let mutable fkSpectrumIdentificationList'   = fkSpectrumIdentificationList
            let mutable details'                        = details
            let mutable rowVersion'                     = rowVersion

            new() = SpectrumIdentificationResult(null, null, null, null,  null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.SpectraData with get() = spectraData' and set(value) = spectraData' <- value
            [<ForeignKey("SpectraData")>]
            member this.FKSpectraData with get() = fkSpectraData' and set(value) = fkSpectraData' <- value
            member this.SpectrumID with get() = spectrumID' and set(value) = spectrumID' <- value
            [<ForeignKey("FKSpectrumIdentificationResult")>]
            member this.SpectrumIdentificationItem with get() = spectrumIdentificationItem' and set(value) = spectrumIdentificationItem' <- value
            member this.FKSpectrumIdentificationList with get() = fkSpectrumIdentificationList' and set(value) = fkSpectrumIdentificationList' <- value
            [<ForeignKey("FKSpectrumIdentificationResult")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Represents the set of all search results from SpectrumIdentification.
    type [<AllowNullLiteral>]
        SpectrumIdentificationList (id:string, name:string, numSequencesSearched:Nullable<int64>, 
                                    fragmentationTables:List<Measure>,
                                    spectrumIdentificationResult:List<SpectrumIdentificationResult>, 
                                    fkAnalysisData:string, fkProteinDetection:string, 
                                    details:List<SpectrumIdentificationListParam>, rowVersion:Nullable<DateTime>
                                   ) =
            let mutable id'                           = id
            let mutable name'                         = name
            let mutable numSequencesSearched'         = numSequencesSearched
            let mutable fragmentationTables'          = fragmentationTables
            let mutable spectrumIdentificationResult' = spectrumIdentificationResult
            let mutable fkAnalysisData'               = fkAnalysisData
            let mutable fkProteinDetection'           = fkProteinDetection
            let mutable details'                      = details
            let mutable rowVersion'                   = rowVersion

            new() = SpectrumIdentificationList(null, null, Nullable(), null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.NumSequencesSearched with get() = numSequencesSearched' and set(value) = numSequencesSearched' <- value
            [<ForeignKey("FKSpectrumIdentificationList")>]
            member this.FragmentationTables with get() = fragmentationTables' and set(value) = fragmentationTables' <- value
            [<ForeignKey("FKSpectrumIdentificationList")>]
            member this.SpectrumIdentificationResult with get() = spectrumIdentificationResult' and set(value) = spectrumIdentificationResult' <- value
            member this.FKAnalysisData with get() = fkAnalysisData' and set(value) = fkAnalysisData' <- value
            member this.FKProteinDetection with get() = fkProteinDetection' and set(value) = fkProteinDetection' <- value
            [<ForeignKey("FKSpectrumIdentificationList")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///An Analysis which tries to identify peptides in input spectra, referencing the database searched, the input spectra, the output results and the protocol that is run.
    type [<AllowNullLiteral>]
        SpectrumIdentification (id:string, name:string, activityDate:Nullable<DateTime>, 
                                spectrumidentificationList:SpectrumIdentificationList,
                                fkSpectrumIdentificationList:string,
                                spectrumIdentificationProtocol:SpectrumIdentificationProtocol,
                                fkSpectrumIdentificationProtocol:string,
                                spectraData:List<SpectraData>, searchDatabases:List<SearchDatabase>, 
                                fkMzIdentMLDocument:string, rowVersion:Nullable<DateTime>
                               ) =
            let mutable id'                                 = id
            let mutable name'                               = name
            let mutable activityDate'                       = activityDate
            let mutable spectrumidentificationList'         = spectrumidentificationList
            let mutable fkSpectrumIdentificationList'       = fkSpectrumIdentificationList
            let mutable spectrumIdentificationProtocol'     = spectrumIdentificationProtocol
            let mutable fkSpectrumIdentificationProtocol'   = fkSpectrumIdentificationProtocol
            //SpectraData_Ref
            let mutable spectraData'                        = spectraData
            //
            //SearchDatabase_Ref
            let mutable searchDatabases'                    = searchDatabases
            //
            let mutable fkMzIdentMLDocument'                = fkMzIdentMLDocument
            let mutable rowVersion'                         = rowVersion

            new() = SpectrumIdentification(null, null, Nullable(), null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.ActivityDate with get() = activityDate' and set(value) = activityDate' <- value
            member this.SpectrumIdentificationList with get() = spectrumidentificationList' and set(value) = spectrumidentificationList' <- value
            [<ForeignKey("SpectrumIdentificationList")>]
            member this.FKSpectrumIdentificationList with get() = fkSpectrumIdentificationList' and set(value) = fkSpectrumIdentificationList' <- value
            member this.SpectrumIdentificationProtocol with get() = spectrumIdentificationProtocol' and set(value) = spectrumIdentificationProtocol' <- value
            [<ForeignKey("SpectrumIdentificationProtocol")>]
            member this.FKSpectrumIdentificationProtocol with get() = fkSpectrumIdentificationProtocol' and set(value) = fkSpectrumIdentificationProtocol' <- value
            [<ForeignKey("FKSpectrumIdentification")>]
            member this.SpectraData with get() = spectraData' and set(value) = spectraData' <- value
            [<ForeignKey("FKSpectrumIdentification")>]
            member this.SearchDatabases with get() = searchDatabases' and set(value) = searchDatabases' <- value
            member this.FKMzIdentMLDocument with get() = fkMzIdentMLDocument' and set(value) = fkMzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The parameters and settings of a SpectrumIdentification analysis.
    type [<AllowNullLiteral>] 
        ProteinDetectionProtocol (id:string, name:string, analysisSoftware:List<AnalysisSoftware>, 
                                  analysisParams:List<AnalysisParam>, threshold:List<ThresholdParam>, 
                                  fkMzIdentMLDocument:string, rowVersion:Nullable<DateTime>
                                 ) =
            let mutable id'                     = id
            let mutable name'                   = name
            let mutable analysisSoftware'       = analysisSoftware
            let mutable analysisParams'         = analysisParams
            let mutable threshold'              = threshold
            let mutable fkMzIdentMLDocument'    = fkMzIdentMLDocument
            let mutable rowVersion'             = rowVersion

            new() = ProteinDetectionProtocol(null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.AnalysisSoftware with get() = analysisSoftware' and set(value) = analysisSoftware' <- value
            [<ForeignKey("FKProteinDetectionProtocol")>]
            member this.AnalysisParams with get() = analysisParams' and set(value) = analysisParams' <- value
            [<ForeignKey("FKProteinDetectionProtocol")>]
            member this.Threshold with get() = threshold' and set(value) = threshold' <- value
            member this.FKMzIdentMLDocument with get() = fkMzIdentMLDocument' and set(value) = fkMzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A file from which this mzIdentML instance was created.
    type [<AllowNullLiteral>] 
        SourceFile (id:string, name:string, location:string, externalFormatDocumentation:string, 
                    fileFormat:CVParam, fkFileFormat:string, fkInputs:string, details:List<SourceFileParam>, 
                    rowVersion:Nullable<DateTime>
                   ) =
            let mutable id'                          = id
            let mutable name'                        = name
            let mutable location'                    = location
            let mutable externalFormatDocumentation' = externalFormatDocumentation
            let mutable fileFormat'                  = fileFormat
            let mutable fkFileFormat'                = fkFileFormat
            let mutable fkInputs'                    = fkInputs
            let mutable details'                     = details
            let mutable rowVersion'                  = rowVersion

            new() = SourceFile(null, null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Location with get() = location' and set(value) = location' <- value
            member this.ExternalFormatDocumentation with get() = externalFormatDocumentation' and set(value) = externalFormatDocumentation' <- value
            member this.FileFormat with get() = fileFormat' and set(value) = fileFormat' <- value
            [<ForeignKey("FileFormat")>]
            member this.FKFileFormat with get() = fkFileFormat' and set(value) = fkFileFormat' <- value
            member this.FKInputs with get() = fkInputs' and set(value) = fkInputs' <- value
            [<ForeignKey("FKSourceFile")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The inputs to the analyses including the databases searched, the spectral data and the source file converted to mzIdentML.
    type [<AllowNullLiteral>]
        Inputs (id:string, sourceFiles:List<SourceFile>, searchDatabases:List<SearchDatabase>,
                spectraData:List<SpectraData>, rowVersion:Nullable<DateTime>
               ) =
            let mutable id'                = id
            let mutable sourceFiles'       = sourceFiles
            let mutable searchDatabases'   = searchDatabases
            let mutable spectraData'       = spectraData
            let mutable rowVersion'        = rowVersion

            new() = Inputs(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            [<ForeignKey("FKInputs")>]
            member this.SourceFiles with get() = sourceFiles' and set(value) = sourceFiles' <- value
            [<ForeignKey("FKInputs")>]
            member this.SearchDatabases with get() = searchDatabases' and set(value) = searchDatabases' <- value
            [<ForeignKey("FKInputs")>]
            member this.SpectraData with get() = spectraData' and set(value) = spectraData' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Peptide evidence on which this ProteinHypothesis is based by reference to a PeptideEvidence element.
    type [<AllowNullLiteral>]
        PeptideHypothesis (id:string, peptideEvidence:PeptideEvidence, fkPeptideEvidence:string,
                           spectrumIdentificationItems:List<SpectrumIdentificationItem>,
                           fkProteinDetectionHypothesis:string, rowVersion:Nullable<DateTime>
                          ) =
            let mutable id'                             = id
            let mutable peptideEvidence'                = peptideEvidence
            let mutable fkPeptideEvidence'              = fkPeptideEvidence
            let mutable spectrumIdentificationItems'    = spectrumIdentificationItems
            let mutable fkProteinDetectionHypothesis'   = fkProteinDetectionHypothesis
            let mutable rowVersion'                     = rowVersion

            new() = PeptideHypothesis(null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.PeptideEvidence with get() = peptideEvidence' and set(value) = peptideEvidence' <- value
            [<ForeignKey("PeptideEvidence")>]
            member this.FKPeptideEvidence with get() = fkPeptideEvidence' and set(value) = fkPeptideEvidence' <- value
            [<ForeignKey("FKPeptideHypothesis")>]
            member this.SpectrumIdentificationItems with get() = spectrumIdentificationItems' and set(value) = spectrumIdentificationItems' <- value
            member this.FKProteinDetectionHypothesis with get() = fkProteinDetectionHypothesis' and set(value) = fkProteinDetectionHypothesis' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A single result of the ProteinDetection analysis (i.e. a protein).
    type [<AllowNullLiteral>] 
        ProteinDetectionHypothesis (id:string, name:string, passThreshold:Nullable<bool>, dbSequence:DBSequence,
                                    fkDBSequence:string, peptideHypothesis:List<PeptideHypothesis>, 
                                    fkProteinAmbiguityGroup:string, details:List<ProteinDetectionHypothesisParam>,
                                    rowVersion:Nullable<DateTime>
                                   ) =
            let mutable id'                         = id
            let mutable name'                       = name
            let mutable passThreshold'              = passThreshold
            let mutable dbSequence'                 = dbSequence
            let mutable fkDBSequence'               = fkDBSequence
            let mutable peptideHypothesis'          = peptideHypothesis
            let mutable fkProteinAmbiguityGroup'    = fkProteinAmbiguityGroup
            let mutable details'                    = details
            let mutable rowVersion'                 = rowVersion

            new() = ProteinDetectionHypothesis(null, null, Nullable(), null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.PassThreshold with get() = passThreshold' and set(value) = passThreshold' <- value
            member this.DBSequence with get() = dbSequence' and set(value) = dbSequence' <- value
            [<ForeignKey("DBSequence")>]
            member this.FKDBSequence with get() = fkDBSequence' and set(value) = fkDBSequence' <- value
            [<ForeignKey("FKProteinDetectionHypothesis")>]
            member this.PeptideHypothesis with get() = peptideHypothesis' and set(value) = peptideHypothesis' <- value
            member this.FKProteinAmbiguityGroup with get() = fkProteinAmbiguityGroup' and set(value) = fkProteinAmbiguityGroup' <- value
            [<ForeignKey("FKProteinDetectionHypothesis")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///A set of logically related results from a protein detection, for example to represent conflicting assignments of peptides to proteins.
    type [<AllowNullLiteral>] 
        ProteinAmbiguityGroup (id:string, name:string, proteinDetecionHypothesis:List<ProteinDetectionHypothesis>,
                               fkProteinDetectionList:string, details:List<ProteinAmbiguityGroupParam>,
                               rowVersion:Nullable<DateTime>
                              ) =
            let mutable id'                         = id
            let mutable name'                        = name
            let mutable proteinDetecionHypothesis'  = proteinDetecionHypothesis
            let mutable fkProteinDetectionList'     = fkProteinDetectionList
            let mutable details'                    = details
            let mutable rowVersion'                 = rowVersion

            new() = ProteinAmbiguityGroup(null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            [<ForeignKey("FKProteinAmbiguityGroup")>]
            member this.ProteinDetectionHypothesis with get() = proteinDetecionHypothesis' and set(value) = proteinDetecionHypothesis' <- value
            member this.FKProteinDetectionList with get() = fkProteinDetectionList' and set(value) = fkProteinDetectionList' <- value
            [<ForeignKey("FKProteinAmbiguityGroup")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The protein list resulting from a protein detection process.
    type [<AllowNullLiteral>] 
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
            [<ForeignKey("FKProteinDetectionList")>]
            member this.ProteinAmbiguityGroups with get() = proteinAmbiguityGroups' and set(value) = proteinAmbiguityGroups' <- value
            [<ForeignKey("FKProteinDetectionList")>]
            member this.Details with get() = details' and set(value) = details' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Data sets generated by the analyses, including peptide and protein lists.
    type [<AllowNullLiteral>] 
        AnalysisData (id:string, spectrumIdentificationList:List<SpectrumIdentificationList>, 
                      proteinDetectionList:ProteinDetectionList, fkProteinDetectionList:string,
                      rowVersion:Nullable<DateTime>
                     ) =
            let mutable id'                         = id
            let mutable spectrumIdentificationList' = spectrumIdentificationList
            let mutable proteinDetectionList'       = proteinDetectionList
            let mutable fkProteinDetectionList'     = fkProteinDetectionList
            let mutable rowVersion'                 = rowVersion

            new() = AnalysisData(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            [<ForeignKey("FKAnalysisData")>]
            member this.SpectrumIdentificationList with get() = spectrumIdentificationList' and set(value) = spectrumIdentificationList' <- value
            member this.ProteinDetectionList with get() = proteinDetectionList' and set(value) = proteinDetectionList' <- value
            [<ForeignKey("ProteinDetectionList")>]
            member this.FKProteinDetectionList with get() = fkProteinDetectionList' and set(value) = fkProteinDetectionList' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///An Analysis which assembles a set of peptides (e.g. from a spectra search analysis) to proteins. 
    type [<AllowNullLiteral>] 
        ProteinDetection (id:string, name:string, activityDate:Nullable<DateTime>, 
                          proteinDetectionList:ProteinDetectionList, fkProteinDetectionList:string,
                          proteinDetectionProtocol:ProteinDetectionProtocol, fkProteinDetectionProtocol:string,
                          spectrumIdentificationLists:List<SpectrumIdentificationList>,
                          rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'                          = id
            let mutable name'                        = name
            let mutable activityDate'                = activityDate
            let mutable proteinDetectionList'        = proteinDetectionList
            let mutable fkProteinDetectionList'      = fkProteinDetectionList
            let mutable proteinDetectionProtocol'    = proteinDetectionProtocol
            let mutable fkProteinDetectionProtocol'  = fkProteinDetectionProtocol
            let mutable spectrumIdentificationLists' = spectrumIdentificationLists
            let mutable rowVersion'                  = rowVersion

            new() = ProteinDetection(null, null, Nullable(), null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.ActivityDate with get() = activityDate' and set(value) = activityDate' <- value
            member this.ProteinDetectionList with get() = proteinDetectionList' and set(value) = proteinDetectionList' <- value
            [<ForeignKey("ProteinDetectionList")>]
            member this.FKProteinDetectionList with get() = fkProteinDetectionList' and set(value) = fkProteinDetectionList' <- value
            member this.ProteinDetectionProtocol with get() = proteinDetectionProtocol' and set(value) = proteinDetectionProtocol' <- value
            [<ForeignKey("ProteinDetectionProtocol")>]
            member this.FKProteinDetectionProtocol with get() = fkProteinDetectionProtocol' and set(value) = fkProteinDetectionProtocol' <- value
            [<ForeignKey("FKProteinDetection")>]
            member this.SpectrumIdentificationLists with get() = spectrumIdentificationLists' and set(value) = spectrumIdentificationLists' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Any bibliographic references associated with the file.
    type [<AllowNullLiteral>] 
        BiblioGraphicReference (id:string, name:string, authors:string, doi:string, editor:string, 
                                issue:string, pages:string, publication:string, publisher:string, title:string,
                                volume:string, year:Nullable<int>, fkMzIdentMLDocument:string, 
                                rowVersion:Nullable<DateTime>
                               ) =
            let mutable id'                  = id
            let mutable name'                = name
            let mutable authors'             = authors
            let mutable doi'                 = doi
            let mutable editor'              = editor
            let mutable issue'               = issue
            let mutable pages'               = pages
            let mutable publication'         = publication
            let mutable publisher'           = publisher
            let mutable title'               = title
            let mutable volume'              = volume
            let mutable year'                = year
            let mutable fkMzIdentMLDocument' = fkMzIdentMLDocument
            let mutable rowVersion'          = rowVersion

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
            member this.FKMzIdentMLDocument with get() = fkMzIdentMLDocument' and set(value) = fkMzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The Provider of the mzIdentML record in terms of the contact and software.
    type [<AllowNullLiteral>] 
        Provider (id:string, name:string, analysisSoftware:AnalysisSoftware, fkAnalysisSoftware:string,
                  contactRole:ContactRole, fkContactRole:string, fkMzIdentMLDocument:string,
                  rowVersion:Nullable<DateTime>
                 ) =
            let mutable id'                     = id
            let mutable name'                   = name
            let mutable analysisSoftware'       = analysisSoftware
            let mutable fkAnalysisSoftware'     = fkAnalysisSoftware
            let mutable contactRole'            = contactRole
            let mutable fkContactRole'          = fkContactRole
            let mutable fkMzIdentMLDocument'    = fkMzIdentMLDocument
            let mutable rowVersion'             = rowVersion

            new() = Provider(null, null, null, null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.AnalysisSoftware with get() = analysisSoftware' and set(value) = analysisSoftware' <- value
            [<ForeignKey("AnalysisSoftware")>]
            member this.FKAnalysisSoftware with get() = fkAnalysisSoftware' and set(value) = fkAnalysisSoftware' <- value
            member this.ContactRole with get() = contactRole' and set(value) = contactRole' <- value
            [<ForeignKey("ContactRole")>]
            member this.FKContactRole with get() = fkContactRole' and set(value) = fkContactRole' <- value
            member this.FKMzIdentMLDocument with get() = fkMzIdentMLDocument' and set(value) = fkMzIdentMLDocument' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///The upper-most hierarchy level of mzIdentML with sub-containers for example describing software, protocols and search results.
    type [<AllowNullLiteral>]
        MzIdentMLDocument(
                          id:string,
                          name:string, 
                          version:string,
                          analysisSoftwares:List<AnalysisSoftware>,
                          providers:List<Provider>,
                          organizations:List<Organization>,
                          persons:List<Person>,
                          samples:List<Sample>, 
                          dbSequences:List<DBSequence>, 
                          peptides:List<Peptide>,
                          peptideEvidences:List<PeptideEvidence>, 
                          spectrumIdentifications:List<SpectrumIdentification>, 
                          proteinDetections:List<ProteinDetection>, 
                          spectrumIdentificationProtocols:List<SpectrumIdentificationProtocol>, 
                          proteinDetectionProtocol:ProteinDetectionProtocol,
                          fkProteinDetectionProtocol:string,
                          inputs:Inputs,
                          fkInputs:string,
                          analysisData:AnalysisData,
                          fkAnalysisData:string,
                          biblioGraphicReferences:List<BiblioGraphicReference>,
                          rowVersion:Nullable<DateTime>
                         ) =
            let mutable id'                              = id
            let mutable name'                            = name
            let mutable version'                         = version
            let mutable analysisSoftwares'               = analysisSoftwares
            let mutable providers'                       = providers
            let mutable organizations'                   = organizations
            let mutable persons'                         = persons
            let mutable samples'                         = samples
            let mutable dbSequences'                     = dbSequences
            let mutable peptides'                        = peptides
            let mutable peptideEvidences'                = peptideEvidences
            let mutable spectrumIdentifications'         = spectrumIdentifications
            let mutable proteinDetections'               = proteinDetections
            let mutable spectrumIdentificationProtocols' = spectrumIdentificationProtocols
            let mutable proteinDetectionProtocol'        = proteinDetectionProtocol
            let mutable fkProteinDetectionProtocol'      = fkProteinDetectionProtocol
            let mutable inputs'                          = inputs
            let mutable fkInputs'                        = fkInputs
            let mutable analysisData'                    = analysisData
            let mutable fkAnalysisData'                  = fkAnalysisData
            let mutable biblioGraphicReferences'         = biblioGraphicReferences
            let mutable rowVersion'                      = rowVersion

            new() = MzIdentMLDocument(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,null, null, null, null, Nullable())

            [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Version with get() = version' and set(value) = version' <- value
            [<ForeignKey("FKMzIdentMLDocument")>]
            member this.AnalysisSoftwareList with get() = analysisSoftwares' and set(value) = analysisSoftwares' <- value
            [<ForeignKey("FKMzIdentMLDocument")>]
            member this.Providers with get() = providers' and set(value) = providers' <- value
            //Formerly AuditCollection
            [<ForeignKey("FKMzIdentMLDocument")>]
            member this.Organizations with get() = organizations and set(value) = organizations' <- value
            [<ForeignKey("FKMzIdentMLDocument")>]
            member this.Persons with get() = persons and set(value) = persons' <- value
            //
            //Formerly AnalysisSampleCollection
            [<ForeignKey("FKMzIdentMLDocument")>]
            member this. Samples with get() = samples' and set(value) = samples' <- value
            //
            //Formerly SequenceCollection
            [<ForeignKey("FKMzIdentMLDocument")>]
            member this. DBSequences with get() = dbSequences' and set(value) = dbSequences' <- value
            [<ForeignKey("FKMzIdentMLDocument")>]
            member this. Peptides with get() = peptides' and set(value) = peptides' <- value
            [<ForeignKey("FKMzIdentMLDocument")>]
            member this. PeptideEvidences with get() = peptideEvidences' and set(value) = peptideEvidences' <- value
            //
            //AnalysisCollection
            [<ForeignKey("FKMzIdentMLDocument")>]
            member this.SpectrumIdentifications with get() = spectrumIdentifications' and set(value) = spectrumIdentifications' <- value
            [<ForeignKey("FKMzIdentMLDocument")>]
            member this.ProteinDetections with get() = proteinDetections' and set(value) = proteinDetections' <- value
            //
            //AnalysisProtocolCollection
            [<ForeignKey("FKMzIdentMLDocument")>]
            member this.SpectrumIdentificationProtocols with get() = spectrumIdentificationProtocols' and set(value) = spectrumIdentificationProtocols' <- value
            member this.ProteinDetectionProtocol with get() = proteinDetectionProtocol' and set(value) = proteinDetectionProtocol' <- value
            [<ForeignKey("ProteinDetectionProtocol")>]
            member this.FKProteinDetectionProtocol with get() = fkProteinDetectionProtocol' and set(value) = fkProteinDetectionProtocol' <- value
            //
            //DataCollection
            member this.Inputs with get() = inputs' and set(value) = inputs' <- value
            [<ForeignKey("Inputs")>]
            member this.FKInputs with get() = fkInputs' and set(value) = fkInputs' <- value
            [<ForeignKey("FKMzIdentMLDocument")>]
            member this.AnalysisData with get() = analysisData' and set(value) = analysisData' <- value
            [<ForeignKey("FKMzIdentMLDocument")>]
            member this.FKAnalysisData with get() = fkAnalysisData' and set(value) = fkAnalysisData' <- value
            //
            [<ForeignKey("FKMzIdentMLDocument")>]
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

            //[<DefaultValue>] 
            //val mutable m_Value : DbSet<Value>
            //member public this.Value with get() = this.m_Value
            //                                      and set value = this.m_Value <- value

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

            override this.OnModelCreating (modelBuilder:ModelBuilder) =
                    modelBuilder.Entity<MzIdentMLDocument>()
                        .HasIndex("ID")
                        .IsUnique() |> ignore
                    //modelBuilder.Entity<MzIdentMLDocument>()
                    //    .HasIndex("AnalysisDataID") |> ignore
                    //modelBuilder.Entity<DBSequence>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore
                    //modelBuilder.Entity<DBSequence>()
                    //    .HasIndex("FKMzIdentMLDocument") |> ignore
                    //modelBuilder.Entity<DBSequenceParam>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore
                    //modelBuilder.Entity<DBSequenceParam>()
                    //    .HasIndex("DBSequenceID") |> ignore
                    //modelBuilder.Entity<DBSequenceParam>()
                    //    .HasIndex("FKTerm") |> ignore
                    //modelBuilder.Entity<AnalysisData>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore
                    //modelBuilder.Entity<AnalysisData>()
                    //    .HasIndex("ProteinDetectionListID") |> ignore
                    //modelBuilder.Entity<ProteinDetectionList>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore
                    //modelBuilder.Entity<ProteinAmbiguityGroup>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore
                    //modelBuilder.Entity<ProteinAmbiguityGroup>()
                    //    .HasIndex("ProteinDetectionListID") |> ignore
                    //modelBuilder.Entity<ProteinDetectionHypothesis>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore
                    //modelBuilder.Entity<ProteinDetectionHypothesis>()
                    //    .HasIndex("ProteinAmbiguityGroupID") |> ignore
                    //modelBuilder.Entity<PeptideHypothesis>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore
                    //modelBuilder.Entity<PeptideHypothesis>()
                    //    .HasIndex("ProteinDetectionHypothesisID") |> ignore
                    modelBuilder.Entity<SpectrumIdentificationItem>()
                        .HasIndex("ID")
                        .IsUnique() |> ignore
                    //modelBuilder.Entity<SpectrumIdentificationItem>()
                    //    .HasIndex("PeptideHypothesisID") |> ignore
                    //modelBuilder.Entity<SpectrumIdentificationItemParam>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore
                    //modelBuilder.Entity<SpectrumIdentificationItemParam>()
                    //    .HasIndex("SpectrumIdentificationItemID") |> ignore
                    //modelBuilder.Entity<SpectrumIdentificationItemParam>()
                    //    .HasIndex("FKTerm") |> ignore
                    //modelBuilder.Entity<SpectrumIdentificationItem>()
                    //    .HasIndex("SpectrumIdentificationResultID") |> ignore
                    //modelBuilder.Entity<SpectrumIdentificationResultParam>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore
                    //modelBuilder.Entity<SpectrumIdentificationResultParam>()
                    //    .HasIndex("SpectrumIdentificationResultID") |> ignore
                    //modelBuilder.Entity<SpectrumIdentificationResultParam>()
                    //    .HasIndex("FKTerm") |> ignore
                    //modelBuilder.Entity<PeptideEvidence>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore
                    //modelBuilder.Entity<PeptideEvidence>()
                    //    .HasIndex("SpectrumIdentificationItemID") |> ignore
                    //modelBuilder.Entity<PeptideEvidenceParam>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore
                    //modelBuilder.Entity<PeptideEvidenceParam>()
                    //    .HasIndex("PeptideEvidenceID") |> ignore
                    //modelBuilder.Entity<PeptideEvidenceParam>()
                    //    .HasIndex("FKTerm") |> ignore
                    //modelBuilder.Entity<SpectrumIdentificationItem>()
                    //    .HasIndex("PeptideID") |> ignore
                    //modelBuilder.Entity<Peptide>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore
                    //modelBuilder.Entity<PeptideParam>()
                    //    .HasIndex("PeptideID") |> ignore
                    //modelBuilder.Entity<PeptideParam>()
                    //    .HasIndex("FKTerm") |> ignore
                    //modelBuilder.Entity<Modification>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore
                    //modelBuilder.Entity<Modification>()
                    //    .HasIndex("PeptideID") |> ignore
                    //modelBuilder.Entity<ModificationParam>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore
                    //modelBuilder.Entity<ModificationParam>()
                    //    .HasIndex("ModificationID") |> ignore
                    //modelBuilder.Entity<ModificationParam>()
                    //    .HasIndex("FKTerm") |> ignore
                    //modelBuilder.Entity<Term>()
                    //    .HasIndex("ID")
                    //    .IsUnique() |> ignore

