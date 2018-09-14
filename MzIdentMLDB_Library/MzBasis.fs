namespace MzBasis

open System
open System.Collections.Generic
open System.ComponentModel.DataAnnotations.Schema

module Basetypes =

    type [<AllowNullLiteral>]
        Term (id:string, name:string, ontology:Ontology, fkOntology:string, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable name'       = name
            let mutable ontology'   = ontology
            let mutable fkOntology' = fkOntology
            let mutable rowVersion' = rowVersion

            new() = Term(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Ontology with get() = ontology' and set(value) = ontology' <- value
            member this.OntologyID with get() = fkOntology' and set(value) = fkOntology' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
        
    ///Standarized vocabulary for MS-Database.
    and [<AllowNullLiteral>]
        Ontology (id:string, terms:List<Term>, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            [<ForeignKey("OntologyID")>]
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
        abstract member FKTerm     : string
        abstract member Unit       : Term
        abstract member FKUnit     : string
        abstract member RowVersion : Nullable<DateTime>
