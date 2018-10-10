namespace MzQuantMLDataBase

module InsertStatements =

    open DataModel
    open System
    open System.Linq
    open System.ComponentModel.DataAnnotations
    open System.ComponentModel.DataAnnotations.Schema
    open Microsoft.EntityFrameworkCore
    open System.Collections.Generic
    open FSharp.Care.IO
    open BioFSharp.IO
    open MzQuantMLDataBase
    open MzBasis.Basetypes

    module ObjectHandlers =
        
        module private HelperFunctions =
            
            let convertOptionToList (optionOfType : seq<'a> option) =
                match optionOfType with
                | Some x -> x |> List
                | None -> Unchecked.defaultof<List<'a>>

            let addToList (typeCollection : List<'a>) (optionOfType : 'a) =
                match typeCollection with
                | null -> 
                    let tmp = new List<'a>()
                    tmp.Add optionOfType
                    tmp
                | _ -> 
                    typeCollection.Add optionOfType
                    typeCollection

            let addCollectionToList (typeCollection : List<'a>) (optionOfType : seq<'a>) =
                match typeCollection with
                | null -> 
                    let tmp = new List<'a>()
                    tmp.AddRange optionOfType
                    tmp
                | _ -> 
                    typeCollection.AddRange optionOfType
                    typeCollection

            //let addOptionToList (typeCollection : List<'a>) (optionOfType : 'a option) =
            //    match optionOfType with
            //    |Some x -> addToList typeCollection x
            //    |None -> typeCollection

            //let addOptionCollectionToList (inputCollection : List<'a>) (input : seq<'a> option) =
            //    match input with
            //    |Some x -> addCollectionToList inputCollection x
            //    |None -> inputCollection  

            let tryFind (item:'a) =
                match item with
                |null -> None
                |_ -> Some item

            let matchCVParamBases (collection1:List<CVParamBase>) (collection2:List<CVParamBase>) =
                let rec loop i n =
                    let item1=collection1.[i]
                    let item2=collection2.[n]
                    if i=(collection1.Count)-1
                        then
                            if n=(collection2.Count)-1
                                then 
                                    false
                                else
                                    loop 0 (n+1)
                        else
                            if item1.Term=item2.Term && item1.FKTerm=item2.FKTerm && item2.Unit=item2.Unit && 
                               item2.FKUnit=item2.FKUnit && item1.Value=item2.Value
                                then true
                                else loop (i+1) n
                loop 0 0

        // Defining the objectHandlers of the entities of the dbContext.

        open HelperFunctions

        type OntologyTerm =
            |  PsiMS 
            |  PRIDE 
            |  UNIMOD 
            static member ToString (x:OntologyTerm) =
                match x with
                | PsiMS  -> "PSI-MS"
                | PRIDE  -> "PRIDE"
                | UNIMOD -> "UNIMOD"

        type ContextHandler =

            ///Creates connection for SQLite-context and database.
            static member sqliteConnection (name:string) (path:string) =
                let path' = 
                    match path with
                    | null -> __SOURCE_DIRECTORY__
                    | _ -> path 
                let optionsBuilder = 
                    new DbContextOptionsBuilder<MzQuantML>()
                optionsBuilder.UseSqlite(@"Data Source=" + path' + "/" + name) |> ignore
                new MzQuantML(optionsBuilder.Options)       

            ///Creats connection for SQL-context and database.
            static member sqlConnection (name:string) (path:string) =
                let path' = 
                    match path with
                    | null -> "localdb"
                    | _ -> path
                let optionsBuilder = 
                    new DbContextOptionsBuilder<MzQuantML>()
                optionsBuilder.UseSqlServer("Server=(" + path' + ")\mssqllocaldb;Database=" + name + ";Trusted_Connection=True;") |> ignore
                new MzQuantML(optionsBuilder.Options) 

            ///Tries to add the object to the database-context.
            static member tryAddToContext (context:MzQuantML) (item:'b) =
                context.Add(item)

            ///Tries to add the object to the database-context and insert it in the database.
            static member tryAddToContextAndInsert (context:MzQuantML) (item:'b) =
                context.Add(item) |> ignore
                context.SaveChanges()
   
        type TermHandler =
        ///Initializes a term-object with at least all necessary parameters.
            static member init
                (
                    id          : string,
                    ?name       : string,
                    ?ontologyID : string  
                ) =
                let name'        = defaultArg name Unchecked.defaultof<string>
                let ontologyID'  = defaultArg ontologyID Unchecked.defaultof<string>

                new Term(
                            id, 
                            name',
                            null,
                            ontologyID', 
                            (Nullable(DateTime.Now))
                        )

            ///Replaces a name of an existing object with new name.
            static member addName
                (name:string) (table:Term) =
                table.Name <- name
                table
                    
            ///Replaces an ontologyID of an existing term-object with new ontology.
            static member addOntologyID
                (ontologyID:string) (table:Term) =
                table.FKOntology <- ontologyID
                table

            ///Tries to find a term-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (ontologyID:string) =
                query {
                        for i in dbContext.Term.Local do
                            if i.FKOntology=ontologyID
                                then select i
                        }
                |> (fun term -> 
                    if (Seq.exists (fun term' -> term' <> null) term) = false
                        then 
                            query {
                                    for i in dbContext.Term do
                                        if i.FKOntology=ontologyID
                                            then select i
                                    }
                            |> Seq.map (fun term -> term)
                            |> (fun term -> if (Seq.exists (fun term' -> term' <> null) term) = false
                                                then None
                                                else Some (term.Single())
                                )
                        else Some (term.Single())
                    )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (name:string) =
                query {
                        for i in dbContext.Term.Local do
                            if i.Name=name
                                then select i
                        }
                |> (fun term -> 
                    if (Seq.exists (fun term' -> term' <> null) term) = false
                        then 
                            query {
                                    for i in dbContext.Term do
                                        if i.Name=name
                                            then select i
                                    }
                            |> Seq.map (fun term -> term)
                            |> (fun term -> if (Seq.exists (fun term' -> term' <> null) term) = false
                                                then None
                                                else Some term
                                )
                        else Some term
                    )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Term) (item2:Term) =
                item1.FKOntology=item2.FKOntology

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Term) =
                    TermHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                        |Some x -> x
                                                                    |> Seq.map (fun organization -> match TermHandler.hasEqualFieldValues organization item with
                                                                                                    |true -> true
                                                                                                    |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                    if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                                )
                                                        |None -> Some(dbContext.Add item)
                        )

            
            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Term) =
                TermHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()  

        type OntologyHandler =
            ///Initializes a ontology-object with at least all necessary parameters.
            static member init
                (
                    id         : string,
                    ?terms     : seq<Term>
                ) =
                let terms'     = convertOptionToList terms

                new Ontology(
                             id,
                             terms',
                             Nullable(DateTime.Now)
                            )

            ///Adds a term to an existing ontology-object.
            static member addTerm
                (term:Term) (table:Ontology) =
                let result = table.Terms <- addToList table.Terms term
                table

            ///Adds a collection of terms to an existing ontology-object.
            static member addTerms
                (terms:seq<Term>) (table:Ontology) =
                let result = table.Terms <- addCollectionToList table.Terms terms
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.Ontology.Local do
                           if i.ID=id
                              then select (i, i.Terms)
                      }
                |> Seq.map (fun (ontology, _) -> ontology)
                |> (fun ontology -> 
                    if (Seq.exists (fun ontology' -> ontology' <> null) ontology) = false
                        then 
                            query {
                                   for i in dbContext.Ontology do
                                       if i.ID=id
                                          then select (i, i.Terms)
                                  }
                            |> Seq.map (fun (ontology, _) -> ontology)
                            |> (fun ontology -> if (Seq.exists (fun ontology' -> ontology' <> null) ontology) = false
                                                then None
                                                else Some (ontology.Single())
                               )
                        else Some (ontology.Single())
                   )

        type CVParamHandler =
            ///Initializes a cvparam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>

                new CVParam(
                            id', 
                            value', 
                            null,
                            fkTerm,
                            null,
                            fkUnit', 
                            Nullable(DateTime.Now)
                           )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (table:CVParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:CVParam) =
                table.FKUnit <- fkUnit
                table

            ///Tries to find a cvparam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.CVParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.CVParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.CVParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.CVParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:CVParam) (item2:CVParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:CVParam) =
                    CVParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match CVParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:CVParam) =
                CVParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type OrganizationParamHandler =
            ///Initializes a organizationparam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm                  : string,
                    ?id                     : string,
                    ?value                  : string,
                    ?fkUnit                 : string,
                    ?fkMzQuantMLDocument    : string
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let value'                  = defaultArg value Unchecked.defaultof<string>
                let fkUnit'                 = defaultArg fkUnit Unchecked.defaultof<string>
                let fkMzQuantMLDocument'    = defaultArg fkMzQuantMLDocument Unchecked.defaultof<string>

                new OrganizationParam(
                                      id', 
                                      value', 
                                      null,
                                      fkTerm,
                                      null,
                                      fkUnit',
                                      fkMzQuantMLDocument',
                                      Nullable(DateTime.Now)
                                     )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (table:OrganizationParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:OrganizationParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkOrganization of existing object with new one.
            static member addFKOrganization
                (fkOrganization:string) (table:OrganizationParam) =
                table.FKOrganization <- fkOrganization
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.OrganizationParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.OrganizationParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.OrganizationParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.OrganizationParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:OrganizationParam) (item2:OrganizationParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:OrganizationParam) =
                    OrganizationParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match OrganizationParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:OrganizationParam) =
                OrganizationParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type PersonParamHandler =
            ///Initializes a personparam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new PersonParam(
                                id', 
                                value', 
                                null,
                                fkTerm,
                                null,
                                fkUnit',
                                fk',
                                Nullable(DateTime.Now)
                               )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (table:PersonParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:PersonParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkPerson of existing object with new one.
            static member addFKPerson
                (fkPerson:string) (table:PersonParam) =
                table.FKPerson <- fkPerson
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.PersonParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PersonParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.PersonParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PersonParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:PersonParam) (item2:PersonParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:PersonParam) =
                    PersonParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PersonParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:PersonParam) =
                PersonParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type SoftwareParamHandler =
            ///Initializes a softwareParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
  
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new SoftwareParam(
                                  id', 
                                  value', 
                                  null,
                                  fkTerm,
                                  null,
                                  fkUnit',
                                  fk',
                                  Nullable(DateTime.Now)
                                 )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:SoftwareParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:SoftwareParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSoftware of existing object with new one.
            static member addFKSoftware
                (fkSoftware:string) (table:SoftwareParam) =
                table.FKSoftware <- fkSoftware
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.SoftwareParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SoftwareParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.SoftwareParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SoftwareParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SoftwareParam) (item2:SoftwareParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:SoftwareParam) =
                    SoftwareParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SoftwareParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:SoftwareParam) =
                SoftwareParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type SearchDatabaseParamHandler =
            ///Initializes a searchDataBaseParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new SearchDatabaseParam(
                                        id', 
                                        value', 
                                        null,
                                        fkTerm,
                                        null,
                                        fkUnit', 
                                        fk',
                                        Nullable(DateTime.Now)
                                       )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:SearchDatabaseParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:SearchDatabaseParam) =
                table.FKUnit <- fkUnit
                table
        
            ///Replaces fkSearchDatabase of existing object with new one.
            static member addFKSearchDatabase
                (fkSearchDatabase:string) (table:SearchDatabaseParam) =
                table.FKSearchDatabase <- fkSearchDatabase
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.SearchDatabaseParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SearchDatabaseParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.SearchDatabaseParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SearchDatabaseParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SearchDatabaseParam) (item2:SearchDatabaseParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:SearchDatabaseParam) =
                    SearchDatabaseParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SearchDatabaseParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:SearchDatabaseParam) =
                SearchDatabaseParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type RawFileParamHandler =
            ///Initializes a rawFilePAram-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new RawFileParam(
                                 id', 
                                 value', 
                                 null,
                                 fkTerm,
                                 null,
                                 fkUnit', 
                                 fk',
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:RawFileParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:RawFileParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkRawFile of existing object with new one.
            static member addFKRawFile
                (fkRawFile:string) (table:RawFileParam) =
                table.FKRawFile <- fkRawFile
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.RawFileParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.RawFileParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.RawFileParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.RawFileParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:RawFileParam) (item2:RawFileParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:RawFileParam) =
                    RawFileParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match RawFileParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:RawFileParam) =
                RawFileParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type AssayParamHandler =
            ///Initializes a assayParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new AssayParam(
                               id', 
                               value', 
                               null,
                               fkTerm,
                               null,
                               fkUnit', 
                               fk',
                               Nullable(DateTime.Now)
                              )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:AssayParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:AssayParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkAssay of existing object with new one.
            static member addFKAssay
                (fkAssay:string) (table:AssayParam) =
                table.FKAssay <- fkAssay
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.AssayParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.AssayParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.AssayParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.AssayParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:AssayParam) (item2:AssayParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:AssayParam) =
                    AssayParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match AssayParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:AssayParam) =
                AssayParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type StudyVariableParamHandler =
            ///Initializes a studyVariableParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new StudyVariableParam(
                                       id', 
                                       value', 
                                       null,
                                       fkTerm,
                                       null,
                                       fkUnit', 
                                       fk',
                                       Nullable(DateTime.Now)
                                      )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:AssayParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:StudyVariableParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkStudyVariable of existing object with new one.
            static member addFKStudyVariable
                (fkStudyVariable:string) (table:StudyVariableParam) =
                table.FKStudyVariable <- fkStudyVariable
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.StudyVariableParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.StudyVariableParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.StudyVariableParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.StudyVariableParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:StudyVariableParam) (item2:StudyVariableParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:StudyVariableParam) =
                    StudyVariableParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match StudyVariableParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:StudyVariableParam) =
                StudyVariableParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type RatioCalculationParamHandler =
            ///Initializes a ratioCalculationParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new RatioCalculationParam(
                                          id', 
                                          value', 
                                          null,
                                          fkTerm,
                                          null,
                                          fkUnit',
                                          fk',
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:RatioCalculationParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:RatioCalculationParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkRatioCalculation of existing object with new one.
            static member addFKRatioCalculation
                (fkRatio:string) (table:RatioCalculationParam) =
                table.FKRatio <- fkRatio
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.RatioCalculationParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.RatioCalculationParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.RatioCalculationParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.RatioCalculationParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:RatioCalculationParam) (item2:RatioCalculationParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:RatioCalculationParam) =
                    RatioCalculationParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match RatioCalculationParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:RatioCalculationParam) =
                RatioCalculationParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type FeatureParamHandler =
            ///Initializes a featureParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new FeatureParam(
                                 id', 
                                 value', 
                                 null,
                                 fkTerm,
                                 null,
                                 fkUnit',
                                 fk',
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:FeatureParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:FeatureParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkFeature of existing object with new one.
            static member addFKFeature
                (fkFeature:string) (table:FeatureParam) =
                table.FKFeature <- fkFeature
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.FeatureParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.FeatureParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.FeatureParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.FeatureParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:FeatureParam) (item2:FeatureParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:FeatureParam) =
                    FeatureParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match FeatureParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:FeatureParam) =
                FeatureParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type SmallMoleculeParamHandler =
            ///Initializes a smallMoleculeParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new SmallMoleculeParam(
                                       id', 
                                       value', 
                                       null,
                                       fkTerm,
                                       null,
                                       fkUnit',
                                       fk',
                                       Nullable(DateTime.Now)
                                      )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:SmallMoleculeParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:SmallMoleculeParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSmallMolecule of existing object with new one.
            static member addFKSmallMolecule
                (fkSmallMolecule:string) (table:SmallMoleculeParam) =
                table.FKSmallMolecule <- fkSmallMolecule
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.SmallMoleculeParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SmallMoleculeParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.SmallMoleculeParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SmallMoleculeParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SmallMoleculeParam) (item2:SmallMoleculeParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:SmallMoleculeParam) =
                    SmallMoleculeParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SmallMoleculeParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:SmallMoleculeParam) =
                SmallMoleculeParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type SmallMoleculeListParamHandler =
            ///Initializes a smallMoleculeListParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new SmallMoleculeListParam(
                                           id', 
                                           value', 
                                           null,
                                           fkTerm,
                                           null,
                                           fkUnit',
                                           fk',
                                           Nullable(DateTime.Now)
                                          )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:SmallMoleculeListParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:SmallMoleculeListParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSmallMoleculeList of existing object with new one.
            static member addFKSmallMoleculeList
                (fkSmallMoleculeList:string) (table:SmallMoleculeListParam) =
                table.FKSmallMoleculeList <- fkSmallMoleculeList
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.SmallMoleculeListParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SmallMoleculeListParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.SmallMoleculeListParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SmallMoleculeListParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SmallMoleculeListParam) (item2:SmallMoleculeListParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:SmallMoleculeListParam) =
                    SmallMoleculeListParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SmallMoleculeListParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:SmallMoleculeListParam) =
                SmallMoleculeListParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type PeptideConsensusParamHandler =
            ///Initializes a peptideConsensusParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new PeptideConsensusParam(
                                          id', 
                                          value', 
                                          null,
                                          fkTerm,
                                          null,
                                          fkUnit',
                                          fk',
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:PeptideConsensusParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:PeptideConsensusParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkPeptideConsensus of existing object with new one.
            static member addFKPeptideConsensus
                (fkPeptideConsensus:string) (table:PeptideConsensusParam) =
                table.FKPeptideConsensus <- fkPeptideConsensus
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.PeptideConsensusParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PeptideConsensusParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.PeptideConsensusParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PeptideConsensusParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:PeptideConsensusParam) (item2:PeptideConsensusParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:PeptideConsensusParam) =
                    PeptideConsensusParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PeptideConsensusParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:PeptideConsensusParam) =
                PeptideConsensusParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type ProteinParamHandler =
            ///Initializes a proteinParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new ProteinParam(
                                 id', 
                                 value', 
                                 null,
                                 fkTerm,
                                 null,
                                 fkUnit',
                                 fk',
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:ProteinParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:ProteinParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkProtein of existing object with new one.
            static member addFKProtein
                (fkProtein:string) (table:ProteinParam) =
                table.FKProtein <- fkProtein
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.ProteinParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.ProteinParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinParam) (item2:ProteinParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:ProteinParam) =
                    ProteinParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinParam) =
                ProteinParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type ProteinListParamHandler =
            ///Initializes a proteinListParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new ProteinListParam(
                                     id', 
                                     value', 
                                     null,
                                     fkTerm,
                                     null,
                                     fkUnit',
                                     fk',
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:ProteinListParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:ProteinListParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkProteinList of existing object with new one.
            static member addFKProteinList
                (fkProteinList:string) (table:ProteinListParam) =
                table.FKProteinList <- fkProteinList
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.ProteinListParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinListParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.ProteinListParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinListParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinListParam) (item2:ProteinListParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:ProteinListParam) =
                    ProteinListParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinListParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinListParam) =
                ProteinListParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type ProteinGroupParamHandler =
            ///Initializes a proteinGroupParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new ProteinGroupParam(
                                      id', 
                                      value', 
                                      null,
                                      fkTerm,
                                      null,
                                      fkUnit', 
                                      fk',
                                      Nullable(DateTime.Now)
                                     )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:ProteinGroupParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:ProteinGroupParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkProteinGroup of existing object with new one.
            static member addFKProteinGroup
                (fkProteinGroup:string) (table:ProteinGroupParam) =
                table.FKProteinGroup <- fkProteinGroup
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.ProteinGroupParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinGroupParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.ProteinGroupParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinGroupParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinGroupParam) (item2:ProteinGroupParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:ProteinGroupParam) =
                    ProteinGroupParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinGroupParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinGroupParam) =
                ProteinGroupParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type ProteinGroupListParamHandler =
            ///Initializes a proteinGroupListParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new ProteinGroupListParam(
                                          id', 
                                          value', 
                                          null,
                                          fkTerm,
                                          null,
                                          fkUnit',
                                          fk',
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:ProteinGroupListParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:ProteinGroupListParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkProteinGroupList of existing object with new one.
            static member addFKProteinGroupList
                (fkProteinGroupList:string) (table:ProteinGroupListParam) =
                table.FKProteinGroupList <- fkProteinGroupList
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.ProteinGroupListParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinGroupListParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.ProteinGroupListParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinGroupListParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinGroupListParam) (item2:ProteinGroupListParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:ProteinGroupListParam) =
                    ProteinGroupListParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinGroupListParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinGroupListParam) =
                ProteinGroupListParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type PeptideConsensusListParamHandler =
            ///Initializes a peptideConsensusListParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm    : string,
                    ?id       : string,
                    ?value    : string,
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new PeptideConsensusListParam(
                                              id', 
                                              value', 
                                              null,
                                              fkTerm,
                                              null,
                                              fkUnit',
                                              fk',
                                              Nullable(DateTime.Now)
                                             )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:PeptideConsensusListParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:PeptideConsensusListParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkPeptideConsensusList of existing object with new one.
            static member addFKPeptideConsensusList
                (fkPeptideConsensusList:string) (table:PeptideConsensusListParam) =
                table.FKPeptideConsensusList <- fkPeptideConsensusList
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.PeptideConsensusListParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PeptideConsensusListParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.PeptideConsensusListParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PeptideConsensusListParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:PeptideConsensusListParam) (item2:PeptideConsensusListParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:PeptideConsensusListParam) =
                    PeptideConsensusListParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PeptideConsensusListParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:PeptideConsensusListParam) =
                PeptideConsensusListParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type AnalysisSummaryHandler =
            ///Initializes a peptideConsensusListParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm             : string,
                    ?id                : string,
                    ?value             : string,
                    ?fkUnit            : string,
                    ?fk                : string
                ) =
                let id'                = defaultArg id (System.Guid.NewGuid().ToString())
                let value'             = defaultArg value Unchecked.defaultof<string>
                let fkUnit'            = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'                = defaultArg fk Unchecked.defaultof<string>

                new AnalysisSummary(
                                    id', 
                                    value', 
                                    null,
                                    fkTerm,
                                    null,
                                    fkUnit',
                                    fk',
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:AnalysisSummary) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:AnalysisSummary) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkMzQuantMLDocument of existing object with new one.
            static member addFKMzQuantMLDocument
                (fkMzQuantMLDocument:string) (table:AnalysisSummary) =
                table.FKMzQuantMLDocument <- fkMzQuantMLDocument
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.AnalysisSummary.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (analysisSummary, _, _) -> analysisSummary)
                |> (fun analysisSummary -> 
                    if (Seq.exists (fun analysisSummary' -> analysisSummary' <> null) analysisSummary) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisSummary do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (analysisSummary, _, _) -> analysisSummary)
                            |> (fun analysisSummary -> if (Seq.exists (fun analysisSummary' -> analysisSummary' <> null) analysisSummary) = false
                                                        then None
                                                        else Some (analysisSummary.Single())
                               )
                        else Some (analysisSummary.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.AnalysisSummary.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (term, _, _) -> term)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisSummary do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (term, _, _) -> term)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:AnalysisSummary) (item2:AnalysisSummary) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:AnalysisSummary) =
                    AnalysisSummaryHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match AnalysisSummaryHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:AnalysisSummary) =
                AnalysisSummaryHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type RawFilesGroupParamHandler =
            ///Initializes a organizationparam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm                  : string,
                    ?id                     : string,
                    ?value                  : string,
                    ?fkUnit                 : string,
                    ?fkMzQuantMLDocument    : string
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let value'                  = defaultArg value Unchecked.defaultof<string>
                let fkUnit'                 = defaultArg fkUnit Unchecked.defaultof<string>
                let fkMzQuantMLDocument'    = defaultArg fkMzQuantMLDocument Unchecked.defaultof<string>

                new RawFilesGroupParam(
                                       id', 
                                       value', 
                                       null,
                                       fkTerm,
                                       null,
                                       fkUnit',
                                       fkMzQuantMLDocument',
                                       Nullable(DateTime.Now)
                                      )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (table:RawFilesGroupParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:RawFilesGroupParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkOrganization of existing object with new one.
            static member addFKRawFilesGroup
                (fkRawFilesGroup:string) (table:RawFilesGroupParam) =
                table.FKRawFilesGroup <- fkRawFilesGroup
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.RawFilesGroupParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.RawFilesGroupParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.RawFilesGroupParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.RawFilesGroupParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:RawFilesGroupParam) (item2:RawFilesGroupParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:RawFilesGroupParam) =
                    RawFilesGroupParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match RawFilesGroupParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:RawFilesGroupParam) =
                RawFilesGroupParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type ProteinRefParamHandler =
            ///Initializes a peptideConsensusListParam-object with at least all necessary parameters.
            static member init
                (
                    fkTerm             : string,
                    ?id                : string,
                    ?value             : string,
                    ?fkUnit            : string,
                    ?fk                : string
                ) =
                let id'                = defaultArg id (System.Guid.NewGuid().ToString())
                let value'             = defaultArg value Unchecked.defaultof<string>
                let fkUnit'            = defaultArg fkUnit Unchecked.defaultof<string> 
                let fk'                = defaultArg fk Unchecked.defaultof<string>
                
                new ProteinRefParam(
                                    id', 
                                    value', 
                                    null,
                                    fkTerm,
                                    null,
                                    fkUnit',
                                    fk',
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:ProteinRefParam) =
                param.Value <- value
                param

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:ProteinRefParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkProteinRef of existing object with new one.
            static member addFKProteinRef
                (fkProteinRef:string) (table:ProteinRefParam) =
                table.FKProteinRef <- fkProteinRef
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.ProteinRefParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinRefParam do
                                       if i.ID=id
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some (param.Single())
                               )
                        else Some (param.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.ProteinRefParam.Local do
                           if i.FKTerm=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinRefParam do
                                       if i.FKTerm=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param, _ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinRefParam) (item2:ProteinRefParam) =
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:ProteinRefParam) =
                    ProteinRefParamHandler.tryFindByTermName dbContext item.FKTerm
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinRefParamHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinRefParam) =
                ProteinRefParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

//////////////////////////
//End of paramHandlers////////////////////////////
//////////////////////////

        type SoftwareHandler =
            ///Initializes a analysisSoftware-object with at least all necessary parameters.
            static member init
                (
                    
                    version              : string,
                    ?id                  : string,
                    ?details             : seq<SoftwareParam>,
                    ?fkMzQuantMLDocument : string

                ) =
                let id'                  = defaultArg id (System.Guid.NewGuid().ToString())
                let details'             = convertOptionToList details
                let fkMzQuantMLDocument' = defaultArg fkMzQuantMLDocument Unchecked.defaultof<string>

                new Software(
                             id', 
                             version, 
                             details',
                             fkMzQuantMLDocument',
                             Nullable(DateTime.Now)
                            )

            ///Adds new softwareParam to collection of enzymenames.
            static member addDetail
                (softwareParam:SoftwareParam) (table:Software) =
                let result = table.Details <- addToList table.Details softwareParam
                table

            ///Add new collection of analysisSoftwareParams to collection of enzymenames.
            static member addDetails
                (analysisSoftwareParams:seq<SoftwareParam>) (table:Software) =
                let result = table.Details <- addCollectionToList table.Details analysisSoftwareParams
                table

            ///Replaces fkMzQuantMLDocument of existing object with new one.
            static member addFKMzQuantMLDocument
                (fkMzQuantMLDocument:string) (table:Software) =
                table.FKMzQuantMLDocument <- fkMzQuantMLDocument
                table

            ///Tries to find a analysisSoftware-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.Software.Local do
                           if i.ID=id
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (analysisSoftware, _) -> analysisSoftware)
                |> (fun analysisSoftware -> 
                    if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
                        then 
                            query {
                                   for i in dbContext.Software do
                                       if i.ID=id
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (analysisSoftware, _) -> analysisSoftware)
                            |> (fun analysisSoftware -> if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
                                                        then None
                                                        else Some (analysisSoftware.Single())
                               )
                        else Some (analysisSoftware.Single())
                   )

            ///Tries to find a analysisSoftware-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByVersion (dbContext:MzQuantML) (version:string) =
                query {
                       for i in dbContext.Software.Local do
                           if i.Version=version
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (analysisSoftware, _) -> analysisSoftware)
                |> (fun analysisSoftware -> 
                    if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
                        then 
                            query {
                                   for i in dbContext.Software do
                                       if i.Version=version
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (analysisSoftware, _) -> analysisSoftware)
                            |> (fun analysisSoftware -> if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
                                                            then None
                                                            else Some analysisSoftware
                               )
                        else Some analysisSoftware
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Software) (item2:Software) =
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Software) =
                    SoftwareHandler.tryFindByVersion dbContext item.Version
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SoftwareHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Software) =
                SoftwareHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SourceFileHandler =
            ///Initializes a sourceFile-object with at least all necessary parameters.
            static member init
                (   
                    location                     : string,
                    ?id                          : string,
                    ?name                        : string,
                    ?externalFormatDocumentation : string,
                    ?fkInputFiles                : string,
                    ?fileFormat                  : CVParam,
                    ?fkFileFormat                : string
                    
                ) =
                let id'                           = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                         = defaultArg name Unchecked.defaultof<string>
                let externalFormatDocumentation'  = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let fkInputFiles'                 = defaultArg fkInputFiles Unchecked.defaultof<string>
                let fileFormat'                   = defaultArg fileFormat Unchecked.defaultof<CVParam>
                let fkFileFormat'                 = defaultArg fkFileFormat Unchecked.defaultof<string>                
                    
                new SourceFile(
                               id',
                               name',
                               location, 
                               externalFormatDocumentation',
                               fkInputFiles',
                               fileFormat',
                               fkFileFormat',
                               Nullable(DateTime.Now)
                              )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:SourceFile) =
                table.Name <- name
                table

            ///Replaces externalFormatDocumentation of existing object with new one.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (table:SourceFile) =
                table.ExternalFormatDocumentation <- externalFormatDocumentation
                table

            ///Replaces fkInputFiles of existing object with new one.
            static member addFKInputFiles
                (fkInputFiles:string) (table:SourceFile) =
                table.FKInputFiles <- fkInputFiles
                table

            ///Replaces fileFormat of existing object with new one.
            static member addFileFormat
                (fileFormat:CVParam) (table:SourceFile) =
                table.FileFormat <- fileFormat
                table

            ///Replaces fkFileFormat of existing object with new one.
            static member addFKFileFormat
                (fkFileFormat:string) (table:SourceFile) =
                table.FKFileFormat <- fkFileFormat
                table

            ///Tries to find a sourceFile-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.SourceFile.Local do
                           if i.ID=id
                              then select (i, i.FileFormat)
                      }
                |> Seq.map (fun (sourceFile, _) -> sourceFile)
                |> (fun sourceFile -> 
                    if (Seq.exists (fun sourceFile' -> sourceFile' <> null) sourceFile) = false
                        then 
                            query {
                                   for i in dbContext.SourceFile do
                                       if i.ID=id
                                          then select (i, i.FileFormat)
                                  }
                            |> Seq.map (fun (sourceFile, _) -> sourceFile)
                            |> (fun sourceFile -> if (Seq.exists (fun sourceFile' -> sourceFile' <> null) sourceFile) = false
                                                    then None
                                                    else Some (sourceFile.Single())
                               )
                        else Some (sourceFile.Single())
                   )

            ///Tries to find a sourceFile-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByLocation (dbContext:MzQuantML) (location:string) =
                query {
                       for i in dbContext.SourceFile.Local do
                           if i.Location=location
                              then select (i, i.FileFormat)
                      }
                |> Seq.map (fun (sourceFile, _) -> sourceFile)
                |> (fun sourceFile -> 
                    if (Seq.exists (fun sourceFile' -> sourceFile' <> null) sourceFile) = false
                        then 
                            query {
                                   for i in dbContext.SourceFile do
                                       if i.Location=location
                                          then select (i, i.FileFormat)
                                  }
                            |> Seq.map (fun (sourceFile, _) -> sourceFile)
                            |> (fun sourceFile -> if (Seq.exists (fun sourceFile' -> sourceFile' <> null) sourceFile) = false
                                                            then None
                                                            else Some sourceFile
                               )
                        else Some sourceFile
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SourceFile) (item2:SourceFile) =
                item1.FKFileFormat=item2.FKFileFormat && item1.Name=item2.Name && 
                item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:SourceFile) =
                    SourceFileHandler.tryFindByLocation dbContext item.Location
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SourceFileHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:SourceFile) =
                SourceFileHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type OrganizationHandler =
            ///Initializes a organization-object with at least all necessary parameters.
            static member init
                (
                    ?id                  : string,
                    ?name                : string,
                    ?parent              : string,
                    ?fkPerson            : string,
                    ?details             : seq<OrganizationParam>,
                    ?fkMzQuantMLDocument : string
                ) =
                let id'                  = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                = defaultArg name Unchecked.defaultof<string>
                let parent'              = defaultArg parent Unchecked.defaultof<string>
                let fkPerson'            = defaultArg fkPerson Unchecked.defaultof<string>
                let details'             = convertOptionToList details
                let fkMzQuantMLDocument' = defaultArg fkMzQuantMLDocument Unchecked.defaultof<string>
                    
                new Organization(
                                 id', 
                                 name', 
                                 parent',
                                 fkPerson',
                                 details', 
                                 fkMzQuantMLDocument',
                                 Nullable(DateTime.Now)
                                )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:Organization) =
                table.Name <- name
                table

            ///Replaces parent of existing object with new one.
            static member addParent
                (parent:string) (table:Organization) =
                table.Parent <- parent
                table

            ///Replaces fkPerson of existing object with new one.
            static member addFKPerson
                (fkPerson:string) (table:Organization) =
                table.FKPerson <- fkPerson
                table

            ///Adds a organizationparam to an existing organization-object.
            static member addDetail
                (detail:OrganizationParam) (table:Organization) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of organizationparams to an existing organization-object.
            static member addDetails
                (details:seq<OrganizationParam>) (table:Organization) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Replaces fkMzQuantMLDocument of existing object with new one.
            static member addFKMzQuantMLDocument
                (fkMzQuantMLDocument:string) (table:Organization) =
                table.FKMzQuantMLDocument <- fkMzQuantMLDocument
                table

            ///Tries to find a organization-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.Organization.Local do
                           if i.ID=id
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (organization, _) -> organization)
                |> (fun organization -> 
                    if (Seq.exists (fun organization' -> organization' <> null) organization) = false
                        then 
                            query {
                                   for i in dbContext.Organization do
                                       if i.ID=id
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (organization, _) -> organization)
                            |> (fun organization -> if (Seq.exists (fun organization' -> organization' <> null) organization) = false
                                                    then None
                                                    else Some (organization.Single())
                               )
                        else Some (organization.Single())
                   )

            ///Tries to find an organization-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.Organization.Local do
                           if i.Name = name
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (organization, _) -> organization)
                |> (fun organization -> 
                    if (Seq.exists (fun organization' -> organization' <> null) organization) = false
                        then 
                            query {
                                   for i in dbContext.Organization do
                                       if i.Name = name
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (organization, _) -> organization)
                            |> (fun param -> if (Seq.exists (fun organization' -> organization' <> null) organization) = false
                                                then None
                                                else Some param
                               )
                        else Some organization
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Organization) (item2:Organization) =
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) && 
                item1.Parent=item2.Parent

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Organization) =
                    OrganizationHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match OrganizationHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Organization) =
                OrganizationHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type PersonHandler =
        ///Initializes a person-object with at least all necessary parameters.
            static member init
                (
                    ?id                  : string,
                    ?name                : string,
                    ?firstName           : string,
                    ?midInitials         : string,
                    ?lastName            : string,
                    ?contactDetails      : seq<PersonParam>,
                    ?organizations       : seq<Organization>,
                    ?fkMzQuantMLDocument : string
                ) =
                let id'                  = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                = defaultArg name Unchecked.defaultof<string>
                let firstName'           = defaultArg firstName Unchecked.defaultof<string>
                let midInitials'         = defaultArg midInitials Unchecked.defaultof<string>
                let lastName'            = defaultArg lastName Unchecked.defaultof<string>
                let contactDetails'      = convertOptionToList contactDetails
                let organizations'       = convertOptionToList organizations
                let fkMzQuantMLDocument' = defaultArg fkMzQuantMLDocument Unchecked.defaultof<string>
                    
                new Person(
                           id', 
                           name', 
                           firstName',  
                           midInitials', 
                           lastName', 
                           organizations',
                           contactDetails',
                           fkMzQuantMLDocument',
                           Nullable(DateTime.Now)
                          )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:Person) =
                table.Name <- name
                table

            ///Replaces firstname of existing object with new one.
            static member addFirstName
                (firstName:string) (table:Person) =
                table.FirstName <- firstName
                table

            ///Replaces midinitials of existing object with new one.
            static member addMidInitials
                (midInitials:string) (table:Person) =
                table.MidInitials <- midInitials
                table

            ///Replaces lastname of existing object with new one.
            static member addLastName
                (lastName:string) (table:Person) =
                table.LastName <- lastName
                table

            ///Adds a personparam to an existing person-object.
            static member addDetail (detail:PersonParam) (table:Person) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of personparams to an existing person-object.
            static member addDetails
                (details:seq<PersonParam>) (table:Person) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Adds a organization to an existing person-object.
            static member addOrganization
                (organization:Organization) (table:Person) =
                let result = table.Organizations <- addToList table.Organizations organization
                table

            ///Adds a collection of organizations to an existing person-object.
            static member addOrganizations
                (organizations:seq<Organization>) (table:Person) =
                let result = table.Organizations <- addCollectionToList table.Organizations organizations
                table

            ///Replaces fkMzQuantMLDocument of existing object with new one.
            static member addFKMzQuantMLDocument
                (fkMzQuantMLDocument:string) (table:Person) =
                table.FKMzQuantMLDocument <- fkMzQuantMLDocument
                table

            ///Tries to find a person-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.Person.Local do
                           if i.ID=id
                              then select (i, i.Details, i.Organizations)
                      }
                |> Seq.map (fun (person, _, _) -> person)
                |> (fun person -> 
                    if (Seq.exists (fun person' -> person' <> null) person) = false
                        then 
                            query {
                                   for i in dbContext.Person do
                                       if i.ID=id
                                          then select (i, i.Details, i.Organizations)
                                  }
                            |> Seq.map (fun (person, _, _) -> person)
                            |> (fun person -> if (Seq.exists (fun person' -> person' <> null) person) = false
                                                then None
                                                else Some (person.Single())
                               )
                        else Some (person.Single())
                   )

            ///Tries to find a person-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.Person.Local do
                           if i.Name = name
                              then select (i, i.Details, i.Organizations)
                      }
                |> Seq.map (fun (person, _, _) -> person)
                |> (fun person -> 
                    if (Seq.exists (fun person' -> person' <> null) person) = false
                        then 
                            query {
                                   for i in dbContext.Person do
                                       if i.Name = name
                                          then select (i, i.Details, i.Organizations)
                                  }
                            |> Seq.map (fun (person, _, _) -> person)
                            |> (fun person -> if (Seq.exists (fun person' -> person' <> null) person) = false
                                                then None
                                                else Some person
                               )
                        else Some person
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Person) (item2:Person) =
                item1.FirstName=item2.FirstName && item1.FirstName=item2.FirstName && 
                item1.MidInitials=item2.MidInitials && item1.LastName=item2.LastName && 
                item1.Organizations=item2.Organizations && 
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Person) =
                    PersonHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PersonHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Person) =
                PersonHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type ContactRoleHandler =
            ///Initializes a contactrole-object with at least all necessary parameters.
            static member init
                (   
                    fkPerson : string, 
                    fkRole   : string,
                    ?id      : string,
                    ?person  : Person,
                    ?role    : CVParam
                ) =
                let id'                  = defaultArg id (System.Guid.NewGuid().ToString())
                let person'              = defaultArg person Unchecked.defaultof<Person>
                let role'                = defaultArg role Unchecked.defaultof<CVParam>
                    
                new ContactRole(
                                id', 
                                person', 
                                fkPerson,
                                role', 
                                fkRole,
                                Nullable(DateTime.Now)
                               )

            ///Replaces person of existing object with new one.
            static member addPerson
                (person:Person) (table:ContactRole) =
                table.Person <- person
                table

            ///Replaces role of existing object with new one.
            static member addRole
                (role:CVParam) (table:ContactRole) =
                table.Role <- role
                table

            ///Tries to find a contactRole-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.ContactRole.Local do
                           if i.ID=id
                              then select (i, i.Role)
                      }
                |> Seq.map (fun (contactRole, _) -> contactRole)
                |> (fun contactRole -> 
                    if (Seq.exists (fun contactRole' -> contactRole' <> null) contactRole) = false
                        then 
                            query {
                                   for i in dbContext.ContactRole do
                                       if i.ID=id
                                          then select (i, i.Role)
                                  }
                            |> Seq.map (fun (contactRole, _) -> contactRole)
                            |> (fun contactRole -> if (Seq.exists (fun contactRole' -> contactRole' <> null) contactRole) = false
                                                    then None
                                                    else Some (contactRole.Single())
                               )
                        else Some (contactRole.Single())
                   )

            ///Tries to find a contactRole-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByPersonName (dbContext:MzQuantML) (fkPerson:string) =
                query {
                       for i in dbContext.ContactRole.Local do
                           if i.FKPerson=fkPerson
                              then select (i, i.Role)
                      }
                |> Seq.map (fun (contactRole, _) -> contactRole)
                |> (fun contactRole -> 
                    if (Seq.exists (fun contactRole' -> contactRole' <> null) contactRole) = false
                        then 
                            query {
                                   for i in dbContext.ContactRole do
                                       if i.FKPerson=fkPerson
                                          then select (i, i.Role)
                                  }
                            |> Seq.map (fun (contactRole, _) -> contactRole)
                            |> (fun contactRole -> if (Seq.exists (fun contactRole' -> contactRole' <> null) contactRole) = false
                                                       then None
                                                       else Some contactRole
                               )
                        else Some contactRole
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ContactRole) (item2:ContactRole) =
                item1.Role=item2.Role

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:ContactRole) =
                    ContactRoleHandler.tryFindByPersonName dbContext item.FKPerson
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ContactRoleHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:ContactRole) =
                ContactRoleHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type ProviderHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    ?id                  : string,
                    ?name                : string,
                    ?software            : Software,
                    ?fkSoftware          : string,
                    ?contactRole         : ContactRole,
                    ?fkContactRole       : string,
                    ?fkMzQuantMLDocument : string

                ) =
                let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'           = defaultArg name Unchecked.defaultof<string>
                let software'       = defaultArg software Unchecked.defaultof<Software>
                let fkSoftware'     = defaultArg fkSoftware Unchecked.defaultof<string>
                let contactRole'    = defaultArg contactRole Unchecked.defaultof<ContactRole>
                let fkContactRole'  = defaultArg fkContactRole Unchecked.defaultof<string>
                let fkMzQuantMLDocument' = defaultArg fkMzQuantMLDocument Unchecked.defaultof<string>

                new Provider(
                             id', 
                             name', 
                             software', 
                             fkSoftware', 
                             contactRole', 
                             fkContactRole',
                             fkMzQuantMLDocument',
                             Nullable(DateTime.Now)
                            )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:Provider) =
                table.Name <- name
                table

            ///Replaces software of existing object with new one.
            static member addSoftware
                (analysisSoftware:Software) (table:Provider) =
                table.Software <- analysisSoftware
                table

            ///Replaces fkSoftware of existing object with new one.
            static member addFKSoftware
                (fkSoftware:string) (table:Provider) =
                table.FKSoftware <- fkSoftware
                table

            ///Replaces contactrole of existing object with new one.
            static member addContactRole
                (contactRole:ContactRole) (table:Provider) =
                table.ContactRole <- contactRole
                table

            ///Replaces fkContactrole of existing object with new one.
            static member addFKContactRole
                (fkContactrole:string) (table:Provider) =
                table.FKContactRole <- fkContactrole
                table

            ///Replaces fkMzQuantMLDocument of existing object with new one.
            static member addFKMzQuantMLDocument
                (fkMzQuantMLDocument:string) (table:Provider) =
                table.FKMzQuantMLDocument <- fkMzQuantMLDocument
                table

            ///Tries to find a provider-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.Provider.Local do
                           if i.ID=id
                              then select (i, i.Software, i.ContactRole)
                      }
                |> Seq.map (fun (provider, _, _) -> provider)
                |> (fun provider -> 
                    if (Seq.exists (fun provider' -> provider' <> null) provider) = false
                        then 
                            query {
                                   for i in dbContext.Provider do
                                       if i.ID=id
                                          then select (i, i.Software, i.ContactRole)
                                  }
                            |> Seq.map (fun (provider, _, _) -> provider)
                            |> (fun provider -> if (Seq.exists (fun provider' -> provider' <> null) provider) = false
                                                then None
                                                else Some (provider.Single())
                               )
                        else Some (provider.Single())
                   )

            ///Tries to find a provider-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.Provider.Local do
                           if i.Name=name
                              then select (i, i.Software, i.ContactRole)
                      }
                |> Seq.map (fun (provider, _, _) -> provider)
                |> (fun provider -> 
                    if (Seq.exists (fun provider' -> provider' <> null) provider) = false
                        then 
                            query {
                                   for i in dbContext.Provider do
                                       if i.Name=name
                                          then select (i, i.Software, i.ContactRole)
                                  }
                            |> Seq.map (fun (provider, _, _) -> provider)
                            |> (fun provider -> if (Seq.exists (fun provider' -> provider' <> null) provider) = false
                                                    then None
                                                    else Some provider
                               )
                        else Some provider
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Provider) (item2:Provider) =
               item1.FKSoftware=item2.FKSoftware && item1.FKContactRole=item2.FKContactRole

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Provider) =
                    ProviderHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProviderHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Provider) =
                ProviderHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SearchDatabaseHandler =
            ///Initializes a searrchDatabase-object with at least all necessary parameters.
            static member init
                (             
                    location                     : string,
                    fkDatabaseName               : string,
                    ?id                          : string,
                    ?name                        : string,
                    ?numDatabaseEntries          : int,
                    ?releaseDate                 : DateTime,
                    ?version                     : string,
                    ?externalFormatDocumentation : string,
                    ?fileFormat                  : CVParam,
                    ?fkFileFormat                : string,
                    ?databaseName                : CVParam,
                    ?fkInputFiles                : string,
                    ?details                     : seq<SearchDatabaseParam>
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let numDatabaseEntries'          = defaultArg numDatabaseEntries Unchecked.defaultof<int>
                let releaseDate'                 = defaultArg releaseDate Unchecked.defaultof<DateTime>
                let version'                     = defaultArg version Unchecked.defaultof<string>
                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let fileFormat'                  = defaultArg fileFormat Unchecked.defaultof<CVParam>
                let fkFileFormat'                = defaultArg fkFileFormat Unchecked.defaultof<string>
                let databaseName'                = defaultArg databaseName Unchecked.defaultof<CVParam>
                let fkInputFiles'                = defaultArg fkInputFiles Unchecked.defaultof<string>
                let details'                     = convertOptionToList details

                new SearchDatabase(
                                   id', 
                                   name', 
                                   location, 
                                   Nullable(numDatabaseEntries'),
                                   Nullable(releaseDate'),
                                   version',
                                   externalFormatDocumentation',
                                   fileFormat',
                                   fkFileFormat',
                                   databaseName',
                                   fkDatabaseName,
                                   fkInputFiles',
                                   details',
                                   Nullable(DateTime.Now)
                                  )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:SearchDatabase) =
                table.Name <- name
                table

            ///Replaces numDatabaseEntries of existing object with new one.
            static member addNumDatabaseEntries
                (numDatabaseEntries:int) (table:SearchDatabase) =
                table.NumDatabaseEntries <- Nullable(numDatabaseEntries)
                table

            ///Replaces releaseDate of existing object with new one.
            static member addReleaseDate
                (releaseDate:DateTime) (table:SearchDatabase) =
                table.ReleaseDate <- Nullable(releaseDate)
                table

            ///Replaces version of existing object with new one.
            static member addVersion
                (version:string) (table:SearchDatabase) =
                table.Version <- version
                table

            ///Replaces version of existing object with new one.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (table:SearchDatabase) =
                table.Version <- externalFormatDocumentation
                table

            ///Replaces fileFormat of existing object with new one.
            static member addFileFormat
                (fileFormat:CVParam) (table:SearchDatabase) =
                table.FileFormat <- fileFormat
                table
            
            ///Replaces fkFileFormat of existing object with new one.
            static member addFKFileFormat
                (fkFileFormat:string) (table:SearchDatabase) =
                table.FKFileFormat <- fkFileFormat
                table

            ///Replaces databaseName of existing object with new one.
            static member addDatabaseName
                (databaseName:CVParam) (table:SearchDatabase) =
                table.DatabaseName <- databaseName
                table

            ///Replaces fkDatabaseName of existing object with new one.
            static member addFKDatabaseName
                (fkDatabaseName:string) (table:SearchDatabase) =
                table.FKDatabaseName <- fkDatabaseName
                table

            ///Replaces fkInputFiles of existing object with new one.
            static member addFKInputFiles
                (fkInputFiles:string) (table:SearchDatabase) =
                table.FKInputFiles <- fkInputFiles
                table

            ///Adds a searchDatabaseParam to an existing object.
            static member addDetail (detail:SearchDatabaseParam) (table:SearchDatabase) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of searchDatabaseParams to an existing object.
            static member addDetails
                (details:seq<SearchDatabaseParam>) (table:SearchDatabase) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a searchDatabase-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.SearchDatabase.Local do
                           if i.ID=id
                              then select (i, i.FileFormat, i.DatabaseName, i.ReleaseDate, i.Details)
                      }
                |> Seq.map (fun (searchDatabase, _, _, _, _) -> searchDatabase)
                |> (fun searchDatabase -> 
                    if (Seq.exists (fun searchDatabase' -> searchDatabase' <> null) searchDatabase) = false
                        then 
                            query {
                                   for i in dbContext.SearchDatabase do
                                       if i.ID=id
                                          then select (i, i.FileFormat, i.DatabaseName, i.ReleaseDate, i.Details)
                                  }
                            |> Seq.map (fun (searchDatabase, _, _, _, _) -> searchDatabase)
                            |> (fun searchDatabase -> if (Seq.exists (fun searchDatabase' -> searchDatabase' <> null) searchDatabase) = false
                                                        then None
                                                        else Some (searchDatabase.Single())
                               )
                        else Some (searchDatabase.Single())
                   )

            ///Tries to find a searchDatabase-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByLocation (dbContext:MzQuantML) (location:string) =
                query {
                       for i in dbContext.SearchDatabase.Local do
                           if i.Location=location
                              then select (i, i.FileFormat, i.DatabaseName, i.ReleaseDate, i.Details)
                      }
                |> Seq.map (fun (searchDatabase, _, _, _, _) -> searchDatabase)
                |> (fun searchDatabase -> 
                    if (Seq.exists (fun searchDatabase' -> searchDatabase' <> null) searchDatabase) = false
                        then 
                            query {
                                   for i in dbContext.SearchDatabase do
                                       if i.Location=location
                                          then select (i, i.FileFormat, i.DatabaseName, i.ReleaseDate, i.Details)
                                  }
                            |> Seq.map (fun (searchDatabase, _, _, _, _) -> searchDatabase)
                            |> (fun searchDatabase -> if (Seq.exists (fun searchDatabase' -> searchDatabase' <> null) searchDatabase) = false
                                                        then None
                                                        else Some searchDatabase
                               )
                        else Some searchDatabase
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SearchDatabase) (item2:SearchDatabase) =
               item1.Name=item2.Name && item1.NumDatabaseEntries=item2.NumDatabaseEntries &&
               item1.ReleaseDate=item2.ReleaseDate && item1.Version=item2.Version &&
               item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation &&
               item1.FKFileFormat=item2.FKFileFormat && item1.FKDatabaseName=item2.FKDatabaseName &&
               matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:SearchDatabase) =
                    SearchDatabaseHandler.tryFindByLocation dbContext item.Location
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SearchDatabaseHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:SearchDatabase) =
                SearchDatabaseHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type IdentificationFileHandler =
            ///Initializes a identificationFile-object with at least all necessary parameters.
            static member init
                (             
                    location                     : string,
                    ?id                          : string,
                    ?name                        : string,
                    ?searchDatabase              : SearchDatabase,
                    ?fkSearchDatabase            : string,
                    ?externalFormatDocumentation : string,
                    ?fileFormat                  : CVParam,
                    ?fkFileFormat                : string,
                    ?fkInputFiles                : string,
                    ?details                     : seq<IdentificationFileParam>
                    
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let searchDatabase'              = defaultArg searchDatabase Unchecked.defaultof<SearchDatabase>
                let fkSearchDatabase'            = defaultArg fkSearchDatabase Unchecked.defaultof<string>
                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let fileFormat'                  = defaultArg fileFormat Unchecked.defaultof<CVParam>
                let fkFileFormat'                = defaultArg fkFileFormat Unchecked.defaultof<string>
                let fkInputFiles'                = defaultArg fkInputFiles Unchecked.defaultof<string>
                let details'                     = convertOptionToList details
                        

                new IdentificationFile(
                                       id', 
                                       name', 
                                       location, 
                                       searchDatabase',
                                       fkSearchDatabase',
                                       externalFormatDocumentation',
                                       fileFormat',
                                       fkFileFormat',
                                       fkInputFiles',
                                       details',
                                       Nullable(DateTime.Now)
                                      )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:IdentificationFile) =
                table.Name <- name
                table

            ///Replaces searchDatabase of existing object with new one.
            static member addSearchDatabase
                (searchDatabase:SearchDatabase) (table:IdentificationFile) =
                table.SearchDatabase <- searchDatabase
                table

            ///Replaces fkSearchDatabase of existing object with new one.
            static member addFKSearchDatabase
                (fkSearchDatabase:string) (table:IdentificationFile) =
                table.FKSearchDatabase <- fkSearchDatabase
                table

            ///Replaces externalFormatDocumentation of existing object with new one.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (table:IdentificationFile) =
                table.ExternalFormatDocumentation <- externalFormatDocumentation
                table

            ///Replaces fileFormat of existing object with new one.
            static member addFileFormat
                (fileFormat:CVParam) (table:IdentificationFile) =
                table.FileFormat <- fileFormat
                table

            ///Replaces fkFileFormat of existing object with new one.
            static member addFKFileFormat
                (fkFileFormat:string) (table:IdentificationFile) =
                table.FKFileFormat <- fkFileFormat
                table

            ///Replaces fkInputFiles of existing object with new one.
            static member addFKInputFiles
                (fkInputFiles:string) (table:IdentificationFile) =
                table.FKInputFiles <- fkInputFiles
                table

            ///Adds a identificationFileParam to an existing object.
            static member addDetail (detail:IdentificationFileParam) (table:IdentificationFile) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of identificationFileParams to an existing object.
            static member addDetails
                (details:seq<IdentificationFileParam>) (table:IdentificationFile) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a identificationFile-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.IdentificationFile.Local do
                           if i.ID=id
                              then select (i, i.FileFormat, i.SearchDatabase, i.Details)
                      }
                |> Seq.map (fun (identificationFile, _, _, _) -> identificationFile)
                |> (fun identificationFile -> 
                    if (Seq.exists (fun identificationFile' -> identificationFile' <> null) identificationFile) = false
                        then 
                            query {
                                   for i in dbContext.IdentificationFile do
                                       if i.ID=id
                                          then select (i, i.FileFormat, i.SearchDatabase, i.Details)
                                  }
                            |> Seq.map (fun (identificationFile, _, _, _) -> identificationFile)
                            |> (fun identificationFile -> if (Seq.exists (fun identificationFile' -> identificationFile' <> null) identificationFile) = false
                                                            then None
                                                            else Some (identificationFile.Single())
                               )
                        else Some (identificationFile.Single())
                   )

            ///Tries to find a identificationFile-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByLocation (dbContext:MzQuantML) (location:string) =
                query {
                       for i in dbContext.IdentificationFile.Local do
                           if i.Location=location
                              then select (i, i.FileFormat, i.SearchDatabase, i.Details)
                      }
                |> Seq.map (fun (identificationFile, _, _, _) -> identificationFile)
                |> (fun identificationFile -> 
                    if (Seq.exists (fun identificationFile' -> identificationFile' <> null) identificationFile) = false
                        then 
                            query {
                                   for i in dbContext.IdentificationFile do
                                       if i.Location=location
                                          then select (i, i.FileFormat, i.SearchDatabase, i.Details)
                                  }
                            |> Seq.map (fun (identificationFile, _, _, _) -> identificationFile)
                            |> (fun identificationFile -> if (Seq.exists (fun identificationFile' -> identificationFile' <> null) identificationFile) = false
                                                            then None
                                                            else Some identificationFile
                               )
                        else Some identificationFile
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:IdentificationFile) (item2:IdentificationFile) =
               item1.Name=item2.Name && item1.SearchDatabase=item2.SearchDatabase &&
               item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation &&
               item1.FKFileFormat=item2.FKFileFormat && 
               matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:IdentificationFile) =
                    IdentificationFileHandler.tryFindByLocation dbContext item.Location
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match IdentificationFileHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:IdentificationFile) =
                IdentificationFileHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type IdentificationRefHandler =
            ///Initializes a identificationRef-object with at least all necessary parameters.
            static member init
                (             
                    fkIdentificationFile : string,
                    ?id                  : string,
                    ?identificationFile  : IdentificationFile,
                    ?fkProtein           : string,
                    ?fkProteinGroup      : string
                    
                ) =
                let id'                 = defaultArg id (System.Guid.NewGuid().ToString())
                let identificationFile' = defaultArg identificationFile Unchecked.defaultof<IdentificationFile>
                let fkProtein'          = defaultArg fkProtein Unchecked.defaultof<string>
                let fkProteinGroup'     = defaultArg fkProteinGroup Unchecked.defaultof<string>

                new IdentificationRef(
                                      id', 
                                      identificationFile',
                                      fkIdentificationFile,
                                      fkProtein',
                                      fkProteinGroup',
                                      Nullable(DateTime.Now)
                                     )

            ///Replaces identificationFile of existing object with new one.
            static member addIdentificationFile
                (identificationFile:IdentificationFile) (table:IdentificationRef) =
                table.IdentificationFile <- identificationFile
                table

            ///Replaces fkProtein of existing object with new one.
            static member addFKProtein
                (fkProtein:string) (table:IdentificationRef) =
                table.FKProtein <- fkProtein
                table

            ///Replaces fkProteinGroup of existing object with new one.
            static member addFKProteinGroup
                (fkProteinGroup:string) (table:IdentificationRef) =
                table.FKProteinGroup <- fkProteinGroup
                table

            ///Tries to find a identificationRef-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.IdentificationRef.Local do
                           if i.ID=id
                              then select (i, i.IdentificationFile)
                      }
                |> Seq.map (fun (identificationRef, _) -> identificationRef)
                |> (fun identificationRef -> 
                    if (Seq.exists (fun identificationRef' -> identificationRef' <> null) identificationRef) = false
                        then 
                            query {
                                   for i in dbContext.IdentificationRef do
                                       if i.ID=id
                                          then select (i, i.IdentificationFile)
                                  }
                            |> Seq.map (fun (identificationRef, _) -> identificationRef)
                            |> (fun identificationRef -> if (Seq.exists (fun identificationRef' -> identificationRef' <> null) identificationRef) = false
                                                            then None
                                                            else Some (identificationRef.Single())
                               )
                        else Some (identificationRef.Single())
                   )

            ///Tries to find a identificationRef-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFKIdentificationFile (dbContext:MzQuantML) (fkIdentificationFile:string) =
                query {
                       for i in dbContext.IdentificationRef.Local do
                           if i.FKIdentificationFile=fkIdentificationFile
                              then select (i, i.IdentificationFile)
                      }
                |> Seq.map (fun (identificationRef, _) -> identificationRef)
                |> (fun identificationRef -> 
                    if (Seq.exists (fun identificationRef' -> identificationRef' <> null) identificationRef) = false
                        then 
                            query {
                                   for i in dbContext.IdentificationRef do
                                       if i.FKIdentificationFile=fkIdentificationFile
                                          then select (i, i.IdentificationFile)
                                  }
                            |> Seq.map (fun (identificationRef, _) -> identificationRef)
                            |> (fun identificationRef -> if (Seq.exists (fun identificationRef' -> identificationRef' <> null) identificationRef) = false
                                                            then None
                                                            else Some identificationRef
                               )
                        else Some identificationRef
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:IdentificationRef) (item2:IdentificationRef) =
               item1.FKIdentificationFile=item2.FKIdentificationFile && item1.FKProtein=item2.FKProtein &&
               item1.FKProteinGroup=item2.FKProteinGroup

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:IdentificationRef) =
                    IdentificationRefHandler.tryFindByFKIdentificationFile dbContext item.FKIdentificationFile
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match IdentificationRefHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:IdentificationRef) =
                IdentificationRefHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type MethodFileHandler =
            ///Initializes a methodFile-object with at least all necessary parameters.
            static member init
                (             
                    location                     : string,
                    ?id                          : string,
                    ?name                        : string,
                    ?externalFormatDocumentation : string,
                    ?fileFormat                  : CVParam,
                    ?fkFileFormat                : string,
                    ?fkInputFiles                : string
                    
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let fileFormat'                  = defaultArg fileFormat Unchecked.defaultof<CVParam>
                let fkFileFormat'                = defaultArg fkFileFormat Unchecked.defaultof<string>
                let fkInputFiles'                = defaultArg fkInputFiles Unchecked.defaultof<string>

                new MethodFile(
                               id', 
                               name', 
                               location, 
                               externalFormatDocumentation',
                               fileFormat',
                               fkFileFormat',
                               fkInputFiles',
                               Nullable(DateTime.Now)
                              )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:MethodFile) =
                table.Name <- name
                table

            ///Replaces externalFormatDocumentation of existing object with new one.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (table:MethodFile) =
                table.ExternalFormatDocumentation <- externalFormatDocumentation
                table

            ///Replaces fileFormat of existing object with new one.
            static member addFileFormat
                (fileFormat:CVParam) (table:MethodFile) =
                table.FileFormat <- fileFormat
                table

            ///Replaces fkFileFormat of existing object with new one.
            static member addFKFileFormat
                (fkFileFormat:string) (table:MethodFile) =
                table.FKFileFormat <- fkFileFormat
                table

            ///Replaces fkInputFiles of existing object with new one.
            static member addFKInputFiles
                (fkInputFiles:string) (table:MethodFile) =
                table.FKInputFiles <- fkInputFiles
                table

            ///Tries to find a methodFile-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.MethodFile.Local do
                           if i.ID=id
                              then select (i, i.FileFormat)
                      }
                |> Seq.map (fun (methodFile, _) -> methodFile)
                |> (fun methodFile -> 
                    if (Seq.exists (fun methodFile' -> methodFile' <> null) methodFile) = false
                        then 
                            query {
                                   for i in dbContext.MethodFile do
                                       if i.ID=id
                                          then select (i, i.FileFormat)
                                  }
                            |> Seq.map (fun (methodFile, _) -> methodFile)
                            |> (fun methodFile -> if (Seq.exists (fun methodFile' -> methodFile' <> null) methodFile) = false
                                                    then None
                                                    else Some (methodFile.Single())
                               )
                        else Some (methodFile.Single())
                   )

            ///Tries to find a methodFile-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByLocation (dbContext:MzQuantML) (location:string) =
                query {
                       for i in dbContext.MethodFile.Local do
                           if i.Location=location
                              then select (i, i.FileFormat)
                      }
                |> Seq.map (fun (methodFile, _) -> methodFile)
                |> (fun methodFile -> 
                    if (Seq.exists (fun methodFile' -> methodFile' <> null) methodFile) = false
                        then 
                            query {
                                   for i in dbContext.MethodFile do
                                       if i.Location=location
                                          then select (i, i.FileFormat)
                                  }
                            |> Seq.map (fun (methodFile, _) -> methodFile)
                            |> (fun methodFile -> if (Seq.exists (fun methodFile' -> methodFile' <> null) methodFile) = false
                                                            then None
                                                            else Some methodFile
                               )
                        else Some methodFile
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:MethodFile) (item2:MethodFile) =
               item1.Name=item2.Name && item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation
               && item1.FKFileFormat=item2.FKFileFormat 

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:MethodFile) =
                    MethodFileHandler.tryFindByLocation dbContext item.Location
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match MethodFileHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:MethodFile) =
                MethodFileHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type RawFileHandler =
            ///Initializes a rawFile-object with at least all necessary parameters.
            static member init
                (             
                    location                     : string,
                    fkRawFilesGroup              : string,
                    ?id                          : string,
                    ?name                        : string,
                    ?externalFormatDocumentation : string,
                    ?methodFile                  : MethodFile,
                    ?fkMethodFile                : string,
                    ?fileFormat                  : CVParam, 
                    ?fkFileFormat                : string,
                    ?details                     : seq<RawFileParam>
                    
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let methodFile'                  = defaultArg methodFile Unchecked.defaultof<MethodFile>
                let fkMethodFile'                = defaultArg fkMethodFile Unchecked.defaultof<string>
                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let fileFormat'                  = defaultArg fileFormat Unchecked.defaultof<CVParam>
                let fkFileFormat'                = defaultArg fkFileFormat Unchecked.defaultof<string>
                let details'                     = convertOptionToList details
                        

                new RawFile(
                            id', 
                            name', 
                            location, 
                            methodFile',
                            fkMethodFile',
                            externalFormatDocumentation',
                            fileFormat',
                            fkFileFormat',
                            fkRawFilesGroup,
                            details', 
                            Nullable(DateTime.Now)
                           )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:RawFile) =
                table.Name <- name
                table

            ///Replaces methodFile of existing object with new one.
            static member addMethodFile
                (methodFile:MethodFile) (table:RawFile) =
                table.MethodFile <- methodFile
                table

            ///Replaces fkMethodFile of existing object with new one.
            static member addFKMethodFile
                (fkMethodFile:string) (table:RawFile) =
                table.FKMethodFile <- fkMethodFile
                table

            ///Replaces externalFormatDocumentation of existing object with new one.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (table:RawFile) =
                table.ExternalFormatDocumentation <- externalFormatDocumentation
                table

            ///Replaces fileFormat of existing object with new one.
            static member addFileFormat
                (fileFormat:CVParam) (table:RawFile) =
                table.FileFormat <- fileFormat
                table

            ///Replaces fkFileFormat of existing object with new one.
            static member addFKFileFormat
                (fkFileFormat:string) (table:RawFile) =
                table.FKFileFormat <- fkFileFormat
                table

            /////Replaces fkRawFilesGroup of existing object with new one.
            //static member addFKRawFilesGroup
            //    (fkRawFilesGroup:string) (table:RawFile) =
            //    table.FKRawFilesGroup <- fkRawFilesGroup
            //    table

            ///Adds a identificationFileParam to an existing object.
            static member addDetail (detail:RawFileParam) (table:RawFile) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of identificationFileParams to an existing object.
            static member addDetails (details:seq<RawFileParam>) (table:RawFile) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a rawFile-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.RawFile.Local do
                           if i.ID=id
                              then select (i, i.FileFormat, i.MethodFile, i.Details)
                      }
                |> Seq.map (fun (rawFile, _, _, _) -> rawFile)
                |> (fun rawFile -> 
                    if (Seq.exists (fun rawFile' -> rawFile' <> null) rawFile) = false
                        then 
                            query {
                                   for i in dbContext.RawFile do
                                       if i.ID=id
                                          then select (i, i.FileFormat, i.MethodFile, i.Details)
                                  }
                            |> Seq.map (fun (rawFile, _, _, _) -> rawFile)
                            |> (fun rawFile -> if (Seq.exists (fun rawFile' -> rawFile' <> null) rawFile) = false
                                                then None
                                                else Some (rawFile.Single())
                               )
                        else Some (rawFile.Single())
                   )

            ///Tries to find a rawFile-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByLocation (dbContext:MzQuantML) (location:string) =
                query {
                       for i in dbContext.RawFile.Local do
                           if i.Location=location
                              then select (i, i.FileFormat, i.MethodFile, i.Details)
                      }
                |> Seq.map (fun (rawFile, _, _, _) -> rawFile)
                |> (fun rawFile -> 
                    if (Seq.exists (fun rawFile' -> rawFile' <> null) rawFile) = false
                        then 
                            query {
                                   for i in dbContext.RawFile do
                                       if i.Location=location
                                          then select  (i, i.FileFormat, i.MethodFile, i.Details)
                                  }
                            |> Seq.map (fun (rawFile, _, _, _) -> rawFile)
                            |> (fun rawFile -> if (Seq.exists (fun rawFile' -> rawFile' <> null) rawFile) = false
                                                            then None
                                                            else Some rawFile
                               )
                        else Some rawFile
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:RawFile) (item2:RawFile) =
               item1.Name=item2.Name && item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation && 
               item1.FKFileFormat=item2.FKFileFormat && item1.MethodFile=item2.MethodFile && 
               matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:RawFile) =
                    RawFileHandler.tryFindByLocation dbContext item.Location
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match RawFileHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:RawFile) =
                RawFileHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type RawFilesGroupHandler =
            ///Initializes a rawFilesGroup-object with at least all necessary parameters.
            static member init
                (             
                    
                    ?id                          : string,
                    ?rawFiles                    : seq<RawFile>,
                    ?fkInputFiles                : string,
                    ?details                     : seq<RawFilesGroupParam>
                    
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let rawFiles'                    = convertOptionToList rawFiles
                let fkInputFiles'                = defaultArg fkInputFiles Unchecked.defaultof<string>
                let details'                     = convertOptionToList details
                        

                new RawFilesGroup(
                                  id', 
                                  rawFiles', 
                                  fkInputFiles', 
                                  details',
                                  Nullable(DateTime.Now)
                                 )

            ///Adds a rawFile to an existing object.
            static member addRawFile (detail:RawFile) (table:RawFilesGroup) =
                let result = table.RawFiles <- addToList table.RawFiles detail
                table

            ///Adds a collection of rawFile to an existing object.
            static member addRawFiles (details:seq<RawFile>) (table:RawFilesGroup) =
                let result = table.RawFiles <- addCollectionToList table.RawFiles details
                table
            
            ///Replaces fkInputFiles of existing object with new one.
            static member addFKInputFiles
                (fkInputFiles:string) (table:RawFilesGroup) =
                table.FKInputFiles <- fkInputFiles
                table

            ///Adds a rawFilesGroupParam to an existing object.
            static member addDetail (detail:RawFilesGroupParam) (table:RawFilesGroup) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of rawFilesGroupParams to an existing object.
            static member addDetails (details:seq<RawFilesGroupParam>) (table:RawFilesGroup) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a rawFilesGroup-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.RawFilesGroup.Local do
                           if i.ID=id
                              then select (i, i.RawFiles, i.Details)
                      }
                |> Seq.map (fun (rawFilesGroup, _, _) -> rawFilesGroup)
                |> (fun rawFilesGroup -> 
                    if (Seq.exists (fun rawFilesGroup' -> rawFilesGroup' <> null) rawFilesGroup) = false
                        then 
                            query {
                                   for i in dbContext.RawFilesGroup do
                                       if i.ID=id
                                          then select (i, i.RawFiles, i.Details)
                                  }
                            |> Seq.map (fun (rawFilesGroup, _, _) -> rawFilesGroup)
                            |> (fun rawFilesGroup -> if (Seq.exists (fun rawFilesGroup' -> rawFilesGroup' <> null) rawFilesGroup) = false
                                                        then None
                                                        else Some (rawFilesGroup.Single())
                               )
                        else Some (rawFilesGroup.Single())
                   )

            ///Tries to find a rawFilesGroup-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFKInputFiles (dbContext:MzQuantML) (fkInputFiles:string) =
                query {
                       for i in dbContext.RawFilesGroup.Local do
                           if i.FKInputFiles=fkInputFiles
                              then select (i, i.RawFiles, i.Details)
                      }
                |> Seq.map (fun (rawFilesGroup, _, _) -> rawFilesGroup)
                |> (fun rawFilesGroup -> 
                    if (Seq.exists (fun rawFilesGroup' -> rawFilesGroup' <> null) rawFilesGroup) = false
                        then 
                            query {
                                   for i in dbContext.RawFilesGroup do
                                       if i.FKInputFiles=fkInputFiles
                                          then select  (i, i.RawFiles, i.Details)
                                  }
                            |> Seq.map (fun (rawFilesGroup, _, _) -> rawFilesGroup)
                            |> (fun rawFilesGroup -> if (Seq.exists (fun rawFilesGroup' -> rawFilesGroup' <> null) rawFilesGroup) = false
                                                            then None
                                                            else Some rawFilesGroup
                               )
                        else Some rawFilesGroup
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:RawFilesGroup) (item2:RawFilesGroup) =
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:RawFilesGroup) =
                    RawFilesGroupHandler.tryFindByFKInputFiles dbContext item.FKInputFiles
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match RawFilesGroupHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:RawFilesGroup) =
                RawFilesGroupHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type InputFilesHandler =
            ///Initializes a inputFiles-object with at least all necessary parameters.
            static member init
                (             
                    ?id                          : string,
                    ?rawFilesGroups              : seq<RawFilesGroup>,
                    ?methodFiles                 : seq<MethodFile>,
                    ?identificationFiles         : seq<IdentificationFile>,
                    ?searchDatabases             : seq<SearchDatabase>,
                    ?sourceFiles                 : seq<SourceFile>
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let rawFilesGroups'              = convertOptionToList rawFilesGroups
                let methodFiles'                 = convertOptionToList methodFiles
                let identificationFiles'         = convertOptionToList identificationFiles
                let searchDatabases'             = convertOptionToList searchDatabases
                let sourceFiles'                 = convertOptionToList sourceFiles

                new InputFiles(
                               id', 
                               rawFilesGroups', 
                               methodFiles',
                               identificationFiles',
                               searchDatabases',
                               sourceFiles',
                               Nullable(DateTime.Now)
                              )

            ///Adds a rawFilesGroup to an existing object.
            static member addRawFilesGroup (detail:RawFilesGroup) (table:InputFiles) =
                let result = table.RawFilesGroups <- addToList table.RawFilesGroups detail
                table

            ///Adds a collection of rawFilesGroups to an existing object.
            static member addRawFilesGroups (details:seq<RawFilesGroup>) (table:InputFiles) =
                let result = table.RawFilesGroups <- addCollectionToList table.RawFilesGroups details
                table
            
            ///Adds a methodFile to an existing object.
            static member addMethodFile (detail:MethodFile) (table:InputFiles) =
                let result = table.MethodFiles <- addToList table.MethodFiles detail
                table

            ///Adds a collection of methodFiles to an existing object.
            static member addMethodFiles (details:seq<MethodFile>) (table:InputFiles) =
                let result = table.MethodFiles <- addCollectionToList table.MethodFiles details
                table
            
            ///Adds a identificationFile to an existing object.
            static member addIdentificationFile (detail:IdentificationFile) (table:InputFiles) =
                let result = table.IdentificationFiles <- addToList table.IdentificationFiles detail
                table

            ///Adds a collection of identificationFiles to an existing object.
            static member addIdentificationFiles (details:seq<IdentificationFile>) (table:InputFiles) =
                let result = table.IdentificationFiles <- addCollectionToList table.IdentificationFiles details
                table
            
            ///Adds a searchDatabase to an existing object.
            static member addSearchDatabase (detail:SearchDatabase) (table:InputFiles) =
                let result = table.SearchDatabases <- addToList table.SearchDatabases detail
                table

            ///Adds a collection of searchDatabases to an existing object.
            static member addSearchDatabases (details:seq<SearchDatabase>) (table:InputFiles) =
                let result = table.SearchDatabases <- addCollectionToList table.SearchDatabases details
                table
            
            ///Adds a sourceFile to an existing object.
            static member addSourceFile (detail:SourceFile) (table:InputFiles) =
                let result = table.SourceFiles <- addToList table.SourceFiles detail
                table

            ///Adds a collection of sourceFiles to an existing object.
            static member addSourceFiles (details:seq<SourceFile>) (table:InputFiles) =
                let result = table.SourceFiles <- addCollectionToList table.SourceFiles details
                table

            ///Tries to find a inputFiles-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.InputFiles.Local do
                           if i.ID=id
                              then select (i, i.MethodFiles, i.RawFilesGroups, i.SearchDatabases, i.SourceFiles, i.IdentificationFiles)
                      }
                |> Seq.map (fun (inputFiles, _, _, _, _, _) -> inputFiles)
                |> (fun inputFiles -> 
                    if (Seq.exists (fun inputFiles' -> inputFiles' <> null) inputFiles) = false
                        then 
                            query {
                                   for i in dbContext.InputFiles do
                                       if i.ID=id
                                          then select (i, i.MethodFiles, i.RawFilesGroups, i.SearchDatabases, i.SourceFiles, i.IdentificationFiles)
                                  }
                            |> Seq.map (fun (inputFiles, _, _, _, _, _) -> inputFiles)
                            |> (fun inputFiles -> if (Seq.exists (fun inputFiles' -> inputFiles' <> null) inputFiles) = false
                                                    then None
                                                    else Some inputFiles
                               )
                        else Some inputFiles
                   )

            ///Tries to find a inputFiles-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByRawFilesGroups (dbContext:MzQuantML) (rawFilesGroups:seq<RawFilesGroup>) =
                query {
                       for i in dbContext.InputFiles.Local do
                           if i.RawFilesGroups=(rawFilesGroups |> List)
                              then select (i, i.MethodFiles, i.RawFilesGroups, i.SearchDatabases, i.SourceFiles, i.IdentificationFiles)
                      }
                |> Seq.map (fun (inputFiles, _, _, _, _, _) -> inputFiles)
                |> (fun inputFiles -> 
                    if (Seq.exists (fun inputFiles' -> inputFiles' <> null) inputFiles) = false
                        then 
                            query {
                                   for i in dbContext.InputFiles do
                                       if i.RawFilesGroups=(rawFilesGroups |> List)
                                          then select  (i, i.MethodFiles, i.RawFilesGroups, i.SearchDatabases, i.SourceFiles, i.IdentificationFiles)
                                  }
                            |> Seq.map (fun (inputFiles, _, _, _, _, _) -> inputFiles)
                            |> (fun inputFiles -> if (Seq.exists (fun inputFiles' -> inputFiles' <> null) inputFiles) = false
                                                            then None
                                                            else Some inputFiles
                               )
                        else Some inputFiles
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:InputFiles) (item2:InputFiles) =
               item1.MethodFiles=item2.MethodFiles && item1.SearchDatabases=item2.SearchDatabases
               && item1.SourceFiles=item2.SourceFiles && item1.IdentificationFiles=item2.IdentificationFiles

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:InputFiles) =
                    InputFilesHandler.tryFindByID dbContext item.ID
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match InputFilesHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:InputFiles) =
                InputFilesHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ModificationHandler =
            ///Initializes a modification-object with at least all necessary parameters.
            static member init
                (             
                    fkDetail                   : string,
                    ?id                        : string,
                    ?massDelta                 : float,
                    ?residues                  : string,
                    ?fkAssay                   : string,
                    ?fkPeptideConsensus        : string,
                    ?fkSmallMolecule           : string,
                    ?detail                    : CVParam
                    
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let massDelta'                   = defaultArg massDelta Unchecked.defaultof<float>
                let residues'                    = defaultArg residues Unchecked.defaultof<string>
                let fkAssay'                     = defaultArg fkAssay Unchecked.defaultof<string>
                let fkPeptideConsensus'          = defaultArg fkPeptideConsensus Unchecked.defaultof<string>
                let fkSmallMolecule'             = defaultArg fkSmallMolecule Unchecked.defaultof<string>
                let detail'                      = defaultArg detail Unchecked.defaultof<CVParam>
                        

                new Modification(
                                 id', 
                                 Nullable(massDelta'),
                                 residues',
                                 fkAssay',
                                 fkPeptideConsensus',
                                 fkSmallMolecule',
                                 detail',
                                 fkDetail, 
                                 Nullable(DateTime.Now)
                                )

            ///Replaces massDelta of existing object with new one.
            static member addMassDelta
                (massDelta:float) (table:Modification) =
                table.MassDelta <- Nullable(massDelta)
                table

            ///Replaces residues of existing object with new one.
            static member addResidues
                (residues:string) (table:Modification) =
                table.Residues <- residues
                table

            ///Replaces fkAssay of existing object with new one.
            static member addFKAssay
                (fkAssay:string) (table:Modification) =
                table.FKAssay <- fkAssay
                table

            ///Replaces fkPeptideConsensus of existing object with new one.
            static member addFKPeptideConsensus
                (fkPeptideConsensus:string) (table:Modification) =
                table.FKPeptideConsensus <- fkPeptideConsensus
                table

            ///Replaces fkSmallMolecule of existing object with new one.
            static member addFKSmallMolecule
                (fkSmallMolecule:string) (table:Modification) =
                table.FKSmallMolecule <- fkSmallMolecule
                table

            ///Replaces detail of existing object with new one.
            static member addDetail
                (detail:CVParam) (table:Modification) =
                table.Detail <- detail
                table

            ///Tries to find a modification-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.Modification.Local do
                           if i.ID=id
                              then select (i, i.Detail)
                      }
                |> Seq.map (fun (modification, _) -> modification)
                |> (fun modification -> 
                    if (Seq.exists (fun modification' -> modification' <> null) modification) = false
                        then 
                            query {
                                   for i in dbContext.Modification do
                                       if i.ID=id
                                          then select (i, i.Detail)
                                  }
                            |> Seq.map (fun (modification, _) -> modification)
                            |> (fun modification -> if (Seq.exists (fun modification' -> modification' <> null) modification) = false
                                                        then None
                                                        else Some (modification.Single())
                               )
                        else Some (modification.Single())
                   )

            ///Tries to find a modification-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFKAssay (dbContext:MzQuantML) (fkAssay:string) =
                query {
                       for i in dbContext.Modification.Local do
                           if i.FKAssay=fkAssay
                              then select (i, i.Detail)
                      }
                |> Seq.map (fun (modification, _) -> modification)
                |> (fun modification -> 
                    if (Seq.exists (fun modification' -> modification' <> null) modification) = false
                        then 
                            query {
                                   for i in dbContext.Modification do
                                       if i.FKAssay=fkAssay
                                          then select (i, i.Detail)
                                  }
                            |> Seq.map (fun (modification, _) -> modification)
                            |> (fun modification -> if (Seq.exists (fun modification' -> modification' <> null) modification) = false
                                                            then None
                                                            else Some modification
                               )
                        else Some modification
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Modification) (item2:Modification) =
                item1.MassDelta=item2.MassDelta && item1.Residues=item2.Residues &&
                item1.FKSmallMolecule=item2.FKSmallMolecule

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Modification) =
                    ModificationHandler.tryFindByFKAssay dbContext item.FKAssay
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ModificationHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Modification) =
                ModificationHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type AssayHandler =
            ///Initializes an assay-object with at least all necessary parameters.
            static member init
                (             
                    fkEvidenceRef                : string,
                    ?id                          : string,
                    ?name                        : string,
                    ?rawFilesGroup               : RawFilesGroup,
                    ?fkRawFilesGroup             : string,
                    ?label                       : seq<Modification>,
                    ?identificationFile          : IdentificationFile,
                    ?fkIdentificationFile        : string,
                    ?fkStudyVariable             : string,
                    ?fkMzQuantMLDocument         : string,
                    ?details                     : seq<AssayParam>

                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let rawFilesGroup'               = defaultArg rawFilesGroup Unchecked.defaultof<RawFilesGroup>
                let fkRawFilesGroup'             = defaultArg fkRawFilesGroup Unchecked.defaultof<string>
                let label'                       = convertOptionToList label
                let identificationFile'          = defaultArg identificationFile Unchecked.defaultof<IdentificationFile>
                let fkIdentificationFile'        = defaultArg fkIdentificationFile Unchecked.defaultof<string>
                let fkStudyVariable'             = defaultArg fkStudyVariable Unchecked.defaultof<string>
                let fkMzQuantMLDocument'         = defaultArg fkMzQuantMLDocument Unchecked.defaultof<string>
                let details'                     = convertOptionToList details

                new Assay(
                          id', 
                          name', 
                          rawFilesGroup',
                          fkRawFilesGroup',
                          label', 
                          identificationFile',
                          fkIdentificationFile',
                          fkStudyVariable',
                          fkEvidenceRef,
                          fkMzQuantMLDocument',
                          details',
                          Nullable(DateTime.Now)
                         )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:Assay) =
                table.Name <- name
                table
            
            ///Replaces rawFilesGroup of existing object with new one.
            static member addRawFilesGroup
                (rawFilesGroup:RawFilesGroup) (table:Assay) =
                table.RawFilesGroup <- rawFilesGroup
                table

            ///Replaces fkRawFilesGroup of existing object with new one.
            static member addFKRawFilesGroup
                (fkRawFilesGroup:string) (table:Assay) =
                table.FKRawFilesGroup <- fkRawFilesGroup
                table

            ///Replaces identificationFile of existing object with new one.
            static member addIdentificationFile
                (identificationFile:IdentificationFile) (table:Assay) =
                table.IdentificationFile <- identificationFile
                table

            ///Replaces fkIdentificationFile of existing object with new one.
            static member addFKIdentificationFile
                (fkIdentificationFile:string) (table:Assay) =
                table.FKIdentificationFile <- fkIdentificationFile
                table

            ///Replaces fkStudyVariable of existing object with new one.
            static member addFKStudyVariable
                (fkStudyVariable:string) (table:Assay) =
                table.FKStudyVariable <- fkStudyVariable
                table

            ///Adds a modification to an existing object.
            static member addLabel (modification:Modification) (table:Assay) =
                let result = table.Label <- addToList table.Label modification
                table

            ///Adds a collection of modifications to an existing object.
            static member addLabels (modifications:seq<Modification>) (table:Assay) =
                let result = table.Label <- addCollectionToList table.Label modifications
                table

            /////Replaces fkEvidenceRef of existing object with new one.
            //static member addFKEvidenceRef
            //    (fkEvidenceRef:string) (table:Assay) =
            //    table.FKEvidenceRef <- fkEvidenceRef
            //    table

            ///Replaces fkMzQuantMLDocument of existing object with new one.
            static member addFkMzQuantMLDocument
                (fkMzQuantMLDocument:string) (table:Assay) =
                let result = table.FKMzQuantMLDocument <- fkMzQuantMLDocument
                table

            ///Adds a assayParam to an existing object.
            static member addDetail (detail:AssayParam) (table:Assay) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of assayParams to an existing object.
            static member addDetails (details:seq<AssayParam>) (table:Assay) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a assay-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.Assay.Local do
                           if i.ID=id
                              then select (i, i.RawFilesGroup, i.IdentificationFile, i.Label, i.Details)
                      }
                |> Seq.map (fun (assay, _, _, _, _) -> assay)
                |> (fun assay -> 
                    if (Seq.exists (fun assay' -> assay' <> null) assay) = false
                        then 
                            query {
                                   for i in dbContext.Assay do
                                       if i.ID=id
                                          then select (i, i.RawFilesGroup, i.IdentificationFile, i.Label, i.Details)
                                  }
                            |> Seq.map (fun (assay, _, _, _, _) -> assay)
                            |> (fun assay -> if (Seq.exists (fun assay' -> assay' <> null) assay) = false
                                                then None
                                                else Some (assay.Single())
                               )
                        else Some (assay.Single())
                   )

            ///Tries to find an assay-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.Assay.Local do
                           if i.Name=name
                              then select (i, i.RawFilesGroup, i.IdentificationFile, i.Label, i.Details)
                      }
                |> Seq.map (fun (assay, _, _, _, _) -> assay)
                |> (fun assay -> 
                    if (Seq.exists (fun assay' -> assay' <> null) assay) = false
                        then 
                            query {
                                   for i in dbContext.Assay do
                                       if i.Name=name
                                          then select (i, i.RawFilesGroup, i.IdentificationFile, i.Label, i.Details)
                                  }
                            |> Seq.map (fun (assay, _, _, _, _) -> assay)
                            |> (fun assay -> if (Seq.exists (fun assay' -> assay' <> null) assay) = false
                                                            then None
                                                            else Some assay
                               )
                        else Some assay
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Assay) (item2:Assay) =
                item1.RawFilesGroup=item2.RawFilesGroup && item1.IdentificationFile=item2.IdentificationFile && 
                item1.Label=item2.Label && 
                    matchCVParamBases 
                        (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                        (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Assay) =
                    AssayHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match AssayHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Assay) =
                AssayHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type StudyVariableHandler =
            ///Initializes a studyVariable-object with at least all necessary parameters.
            static member init
                (             
                    assays                  : seq<Assay>,
                    ?id                     : string,
                    ?name                   : string,
                    ?details                : seq<StudyVariableParam>,
                    ?fkMzQuantMLDocument    : string
                    
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                   = defaultArg name Unchecked.defaultof<string>
                let details'                = convertOptionToList details
                let fkMzQuantMLDocument'    = defaultArg fkMzQuantMLDocument Unchecked.defaultof<string>       

                new StudyVariable(
                                  id', 
                                  name',
                                  assays |> List,
                                  fkMzQuantMLDocument',
                                  details',
                                  Nullable(DateTime.Now)
                                 )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:StudyVariable) =
                table.Name <- name
                table

            ///Replaces fkMzQuantMLDocument of existing object with new one.
            static member addFKMzQuantMLDocument
                (fkMzQuantMLDocument:string) (table:Provider) =
                table.FKMzQuantMLDocument <- fkMzQuantMLDocument
                table

            ///Adds a studyVariableParam to an existing object.
            static member addDetail (detail:StudyVariableParam) (table:StudyVariable) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of studyVariableParams to an existing object.
            static member addDetails (details:seq<StudyVariableParam>) (table:StudyVariable) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a studyVariable-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.StudyVariable.Local do
                           if i.ID=id
                              then select (i, i.Assays, i.Details)
                      }
                |> Seq.map (fun (studyVariable, _, _) -> studyVariable)
                |> (fun studyVariable -> 
                    if (Seq.exists (fun studyVariable' -> studyVariable' <> null) studyVariable) = false
                        then 
                            query {
                                   for i in dbContext.StudyVariable do
                                       if i.ID=id
                                          then select (i, i.Assays, i.Details)
                                  }
                            |> Seq.map (fun (studyVariable, _, _) -> studyVariable)
                            |> (fun studyVariable -> if (Seq.exists (fun studyVariable' -> studyVariable' <> null) studyVariable) = false
                                                        then None
                                                        else Some (studyVariable.Single())
                               )
                        else Some (studyVariable.Single())
                   )

            ///Tries to find a studyVariable-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.StudyVariable.Local do
                           if i.Name=name
                              then select (i, i.Assays, i.Details)
                      }
                |> Seq.map (fun (studyVariable, _, _) -> studyVariable)
                |> (fun studyVariable -> 
                    if (Seq.exists (fun studyVariable' -> studyVariable' <> null) studyVariable) = false
                        then 
                            query {
                                   for i in dbContext.StudyVariable do
                                       if i.Name=name
                                          then select (i, i.Assays, i.Details)
                                  }
                            |> Seq.map (fun (studyVariable, _, _) -> studyVariable)
                            |> (fun studyVariable -> if (Seq.exists (fun studyVariable' -> studyVariable' <> null) studyVariable) = false
                                                            then None
                                                            else Some studyVariable
                               )
                        else Some studyVariable
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:StudyVariable) (item2:StudyVariable) =
                item1.Assays=item2.Assays &&
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:StudyVariable) =
                    StudyVariableHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match StudyVariableHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:StudyVariable) =
                StudyVariableHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type RatioHandler =
            ///Initializes a ratio-object with at least all necessary parameters.
            static member init
                (             
                    
                    fkDenominatorDatatype        : string,
                    fkNumeratorDatatype          : string,
                    ?id                          : string,
                    ?name                        : string,
                    ?denominatorDatatype         : CVParam,
                    ?denominatorSV               : StudyVariable,
                    ?fkDenominatorSV             : string,
                    ?denominatorAS               : Assay,
                    ?fkDenominatorAS             : string,
                    ?numeratorDatatype           : CVParam,
                    ?numeratorSV                 : StudyVariable,
                    ?fkNumeratorSV               : string,
                    ?numeratorAS                 : Assay,
                    ?fkNumeratorAS               : string,
                    ?ratioCalculation            : seq<RatioCalculationParam>,
                    ?fkMzQuantMLDocument         : string
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let denominatorDatatype'         = defaultArg denominatorDatatype Unchecked.defaultof<CVParam>
                let denominatorSV'               = defaultArg denominatorSV Unchecked.defaultof<StudyVariable>
                let fkDenominatorSV'             = defaultArg fkDenominatorSV Unchecked.defaultof<string>
                let denominatorAS'               = defaultArg denominatorAS Unchecked.defaultof<Assay>
                let fkDenominatorAS'             = defaultArg fkDenominatorAS Unchecked.defaultof<string>
                let numeratorDatatype'           = defaultArg numeratorDatatype Unchecked.defaultof<CVParam>
                let numeratorSV'                 = defaultArg numeratorSV Unchecked.defaultof<StudyVariable>
                let fkNumeratorSV'               = defaultArg fkNumeratorSV Unchecked.defaultof<string>
                let numeratorAS'                 = defaultArg numeratorAS Unchecked.defaultof<Assay>
                let fkNumeratorAS'               = defaultArg fkNumeratorAS Unchecked.defaultof<string>
                let ratioCalculation'            = convertOptionToList ratioCalculation
                let fkMzQuantMLDocument'         = defaultArg fkMzQuantMLDocument Unchecked.defaultof<string>

                new Ratio(
                          id', 
                          name',
                          denominatorSV',
                          fkDenominatorSV',
                          denominatorAS',
                          fkDenominatorAS',
                          denominatorDatatype',
                          fkDenominatorDatatype,
                          numeratorSV',
                          fkNumeratorSV',
                          numeratorAS',
                          fkNumeratorAS',
                          numeratorDatatype',
                          fkNumeratorDatatype,
                          ratioCalculation',
                          fkMzQuantMLDocument',
                          Nullable(DateTime.Now)
                         )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:Ratio) =
                table.Name <- name
                table

            ///Replaces numerator of existing object with new one.
            static member addNumeratorSV
                (numerator:StudyVariable) (table:Ratio) =
                table.NumeratorSV <- numerator
                table

            ///Replaces denominator of existing object with new one.
            static member addDenomiantorSV
                (denominator:StudyVariable) (table:Ratio) =
                table.DenominatorSV <- denominator
                table

            ///Replaces numerator of existing object with new one.
            static member addNumeratorAS
                (numerator:Assay) (table:Ratio) =
                table.NumeratorAS <- numerator
                table

            ///Replaces denominator of existing object with new one.
            static member addDenomiantorAS
                (denominator:Assay) (table:Ratio) =
                table.DenominatorAS <- denominator
                table

            ///Adds a ratioCalculationParam to an existing object.
            static member addRatioCalculationParam (ratioCalculationParam:RatioCalculationParam) (table:Ratio) =
                let result = table.RatioCalculation <- addToList table.RatioCalculation ratioCalculationParam
                table

            ///Adds a collection of ratioCalculationParams to an existing object.
            static member addRatioCalculationParams (ratioCalculationParams:seq<RatioCalculationParam>) (table:Ratio) =
                let result = table.RatioCalculation <- addCollectionToList table.RatioCalculation ratioCalculationParams
                table

            ///Replaces fkMzQuantMLDocument of existing object with new one.
            static member addFkMzQuantMLDocument
                (fkMzQuantMLDocument:string) (table:Ratio) =
                let result = table.FKMzQuantMLDocument <- fkMzQuantMLDocument
                table

            ///Tries to find a ratio-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.Ratio.Local do
                           if i.ID=id
                              then select (i, i.NumeratorAS, i.DenominatorAS, i.NumeratorSV, i.DenominatorSV, i.NumeratorDataType, i.DenominatorDataType, i.RatioCalculation)
                      }
                |> Seq.map (fun (ratio, _, _ ,_ , _, _,_ ,_) -> ratio)
                |> (fun ratio -> 
                    if (Seq.exists (fun ratio' -> ratio' <> null) ratio) = false
                        then 
                            query {
                                   for i in dbContext.Ratio do
                                       if i.ID=id
                                          then select (i, i.NumeratorAS, i.DenominatorAS, i.NumeratorSV, i.DenominatorSV, i.NumeratorDataType, i.DenominatorDataType, i.RatioCalculation)
                                  }
                            |> Seq.map (fun (ratio, _, _ ,_ , _, _,_ ,_) -> ratio)
                            |> (fun ratio -> if (Seq.exists (fun ratio' -> ratio' <> null) ratio) = false
                                                then None
                                                else Some (ratio.Single())
                               )
                        else Some (ratio.Single())
                   )

            ///Tries to find a ratio-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.Ratio.Local do
                           if i.Name=name
                              then select (i, i.NumeratorAS, i.DenominatorAS, i.NumeratorSV, i.DenominatorSV, i.NumeratorDataType, i.DenominatorDataType, i.RatioCalculation)
                      }
                |> Seq.map (fun (ratio, _, _ ,_ , _, _,_ ,_) -> ratio)
                |> (fun ratio -> 
                    if (Seq.exists (fun ratio' -> ratio' <> null) ratio) = false
                        then 
                            query {
                                   for i in dbContext.Ratio do
                                       if i.Name=name
                                          then select (i, i.NumeratorAS, i.DenominatorAS, i.NumeratorSV, i.DenominatorSV, i.NumeratorDataType, i.DenominatorDataType, i.RatioCalculation)
                                  }
                            |> Seq.map (fun (ratio, _, _ ,_ , _, _, _, _) -> ratio)
                            |> (fun ratio -> if (Seq.exists (fun ratio' -> ratio' <> null) ratio) = false
                                                            then None
                                                            else Some ratio
                               )
                        else Some ratio
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Ratio) (item2:Ratio) =
                item1.NumeratorAS=item2.NumeratorAS && item1.DenominatorAS=item2.DenominatorAS && 
                item1.NumeratorSV=item2.NumeratorSV && item1.DenominatorSV=item2.DenominatorSV && 
                item1.NumeratorDataType=item2.NumeratorDataType && item1.RatioCalculation=item2.RatioCalculation && 
                item1.DenominatorDataType=item2.DenominatorDataType              

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Ratio) =
                    RatioHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match RatioHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Ratio) =
                RatioHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ColumnHandler =
            ///Initializes a column-object with at least all necessary parameters.
            static member init
                (             
                    index       : int,
                    fkdatatype  : string,
                    ?id         : string,
                    ?datatype   : CVParam
                    
                ) =
                let id'         = defaultArg id (System.Guid.NewGuid().ToString())
                let datatype'   = defaultArg datatype Unchecked.defaultof<CVParam>
                        

                new Column(
                           id',
                           Nullable(index),
                           datatype',
                           fkdatatype,
                           Nullable(DateTime.Now)
                          )

            ///Replaces datatype of existing object with new one.
            static member addDataType
                (datatype:CVParam) (table:Column) =
                table.DataType <- datatype
                table

            ///Tries to find a column-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.Column.Local do
                           if i.ID=id
                              then select (i, i.DataType)
                      }
                |> Seq.map (fun (column, _) -> column)
                |> (fun column -> 
                    if (Seq.exists (fun column' -> column' <> null) column) = false
                        then 
                            query {
                                   for i in dbContext.Column do
                                       if i.ID=id
                                          then select (i, i.DataType)
                                  }
                            |> Seq.map (fun (column, _) -> column)
                            |> (fun column -> if (Seq.exists (fun column' -> column' <> null) column) = false
                                                then None
                                                else Some (column.Single())
                               )
                        else Some (column.Single())
                   )

            ///Tries to find a column-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByIndex (dbContext:MzQuantML) (index:Nullable<int>) =
                query {
                       for i in dbContext.Column.Local do
                           if i.Index=index
                              then select (i, i.DataType)
                      }
                |> Seq.map (fun (column, _) -> column)
                |> (fun column -> 
                    if (Seq.exists (fun column' -> column' <> null) column) = false
                        then 
                            query {
                                   for i in dbContext.Column do
                                       if i.Index=index
                                          then select (i, i.DataType)
                                  }
                            |> Seq.map (fun (column, _) -> column)
                            |> (fun column -> if (Seq.exists (fun column' -> column' <> null) column) = false
                                                            then None
                                                            else Some column
                               )
                        else Some column
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Column) (item2:Column) =
                item1.FKDataType=item2.FKDataType

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Column) =
                    ColumnHandler.tryFindByIndex dbContext item.Index
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ColumnHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Column) =
                ColumnHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type DataMatrixHandler =
            ///Initializes a dataMatrix-object with at least all necessary parameters.
            static member init
                (             
                    row     : string,
                    ?id     : string
                    
                ) =
                let id' = defaultArg id (System.Guid.NewGuid().ToString())
                        

                new DataMatrix(
                               id',
                               row,
                               Nullable(DateTime.Now)
                              )

            ///Tries to find a dataMatrix-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.DataMatrix.Local do
                           if i.ID=id
                              then select i
                      }
                |> Seq.map (fun (dataMatrix) -> dataMatrix)
                |> (fun dataMatrix -> 
                    if (Seq.exists (fun dataMatrix' -> dataMatrix' <> null) dataMatrix) = false
                        then 
                            query {
                                   for i in dbContext.DataMatrix do
                                       if i.ID=id
                                          then select i
                                  }
                            |> Seq.map (fun (dataMatrix) -> dataMatrix)
                            |> (fun dataMatrix -> if (Seq.exists (fun dataMatrix' -> dataMatrix' <> null) dataMatrix) = false
                                                    then None
                                                    else Some (dataMatrix.Single())
                               )
                        else Some (dataMatrix.Single())
                   )

            ///Tries to find a dataMatrix-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByRow (dbContext:MzQuantML) (row:string) =
                query {
                       for i in dbContext.DataMatrix.Local do
                           if i.Row=row
                              then select i
                      }
                |> Seq.map (fun (dataMatrix) -> dataMatrix)
                |> (fun dataMatrix -> 
                    if (Seq.exists (fun dataMatrix' -> dataMatrix' <> null) dataMatrix) = false
                        then 
                            query {
                                   for i in dbContext.DataMatrix do
                                       if i.Row=row
                                          then select i
                                  }
                            |> Seq.map (fun (dataMatrix) -> dataMatrix)
                            |> (fun dataMatrix -> if (Seq.exists (fun dataMatrix' -> dataMatrix' <> null) dataMatrix) = false
                                                            then None
                                                            else Some dataMatrix
                               )
                        else Some dataMatrix
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:DataMatrix) (item2:DataMatrix) =
                item1.ID=item2.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:DataMatrix) =
                    DataMatrixHandler.tryFindByRow dbContext item.Row
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match DataMatrixHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:DataMatrix) =
                DataMatrixHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type AssayQuantLayerHandler =
            ///Initializes a assayQuantLayer-object with at least all necessary parameters.
            static member init
                (             
                    fkdataType                   : string,
                    columnIndex                  : string,
                    fkDataMatrix                 : string,
                    ?id                          : string,
                    ?dataType                    : CVParam,
                    ?dataMatrix                  : DataMatrix,
                    ?fkSmallMoleculeList         : string,
                    ?fkProteinList               : string,
                    ?fkProteinGroupList          : string,
                    ?fkPeptideConsensusList      : string
                    
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let datatype'               = defaultArg dataType Unchecked.defaultof<CVParam>
                let dataMatrix'             = defaultArg dataMatrix Unchecked.defaultof<DataMatrix> 
                let fkSmallMoleculeList'    = defaultArg fkSmallMoleculeList Unchecked.defaultof<string> 
                let fkProteinList'          = defaultArg fkProteinList Unchecked.defaultof<string> 
                let fkProteinGroupList'     = defaultArg fkProteinGroupList Unchecked.defaultof<string> 
                let fkPeptideConsensusList' = defaultArg fkPeptideConsensusList Unchecked.defaultof<string> 

                new AssayQuantLayer(
                                    id',
                                    datatype',
                                    fkdataType,
                                    columnIndex,
                                    dataMatrix',
                                    fkDataMatrix,
                                    fkSmallMoleculeList',
                                    fkProteinList',
                                    fkProteinGroupList',
                                    fkPeptideConsensusList',
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces datatype of existing object with new one.
            static member addDataType
                (datatype:CVParam) (table:AssayQuantLayer) =
                table.DataType <- datatype
                table

            ///Replaces dataMatrix of existing object with new one.
            static member addDataMatrix
                (dataMatrix:DataMatrix) (table:AssayQuantLayer) =
                table.DataMatrix <- dataMatrix
                table

            ///Replaces fkSmallMoleculeList of existing object with new one.
            static member addFKSmallMoleculeList
                (fkSmallMoleculeList:string) (table:AssayQuantLayer) =
                table.FKSmallMoleculeList <- fkSmallMoleculeList
                table

            ///Replaces fkProteinList of existing object with new one.
            static member addFKProteinList
                (fkProteinList:string) (table:AssayQuantLayer) =
                table.FKProteinList <- fkProteinList
                table

            ///Replaces fkProteinGroupList of existing object with new one.
            static member addFKProteinGroupList
                (fkProteinGroupList:string) (table:AssayQuantLayer) =
                table.FKProteinGroupList <- fkProteinGroupList
                table

            ///Replaces fkPeptideConsensusList of existing object with new one.
            static member addFKPeptideConsensusList
                (fkPeptideConsensusList:string) (table:AssayQuantLayer) =
                table.FKPeptideConsensusList <- fkPeptideConsensusList
                table

            ///Tries to find a assayQuantLayer-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.AssayQuantLayer.Local do
                           if i.ID=id
                              then select (i, i.FKDataType, i.FKDataMatrix, i.FKSmallMoleculeList, i.FKProteinList, i.FKProteinGroupList, i.FKPeptideConsensusList)
                      }
                |> Seq.map (fun (assayQuantLayer, _, _, _, _, _, _) -> assayQuantLayer)
                |> (fun assayQuantLayer -> 
                    if (Seq.exists (fun assayQuantLayer' -> assayQuantLayer' <> null) assayQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.AssayQuantLayer do
                                       if i.ID=id
                                          then select (i, i.FKDataType, i.FKDataMatrix, i.FKSmallMoleculeList, i.FKProteinList, i.FKProteinGroupList, i.FKPeptideConsensusList)
                                  }
                            |> Seq.map (fun (assayQuantLayer, _, _, _, _, _, _) -> assayQuantLayer)
                            |> (fun assayQuantLayer -> if (Seq.exists (fun assayQuantLayer' -> assayQuantLayer' <> null) assayQuantLayer) = false
                                                        then None
                                                        else Some (assayQuantLayer.Single())
                               )
                        else Some (assayQuantLayer.Single())
                   )

            ///Tries to find a assayQuantLayer-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
                query {
                       for i in dbContext.AssayQuantLayer.Local do
                           if i.ColumnIndex=columnIndex
                              then select (i, i.FKDataType, i.FKDataMatrix, i.FKSmallMoleculeList, i.FKProteinList, i.FKProteinGroupList, i.FKPeptideConsensusList)
                      }
                |> Seq.map (fun (assayQuantLayer, _, _, _, _, _, _) -> assayQuantLayer)
                |> (fun assayQuantLayer -> 
                    if (Seq.exists (fun assayQuantLayer' -> assayQuantLayer' <> null) assayQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.AssayQuantLayer do
                                       if i.ColumnIndex=columnIndex
                                          then select (i, i.FKDataType, i.FKDataMatrix, i.FKSmallMoleculeList, i.FKProteinList, i.FKProteinGroupList, i.FKPeptideConsensusList)
                                  }
                            |> Seq.map (fun (assayQuantLayer, _, _, _, _, _, _) -> assayQuantLayer)
                            |> (fun assayQuantLayer -> if (Seq.exists (fun assayQuantLayer' -> assayQuantLayer' <> null) assayQuantLayer) = false
                                                            then None
                                                            else Some assayQuantLayer
                               )
                        else Some assayQuantLayer
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:AssayQuantLayer) (item2:AssayQuantLayer) =
                item1.FKDataType=item2.FKDataType && item1.DataMatrix=item2.DataMatrix

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:AssayQuantLayer) =
                    AssayQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match AssayQuantLayerHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:AssayQuantLayer) =
                AssayQuantLayerHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type GlobalQuantLayerHandler =
            ///Initializes a globalQuantLayer-object with at least all necessary parameters.
            static member init
                (             
                    columns                      : seq<Column>,
                    fkDataMatrix                 : string,
                    ?id                          : string,
                    ?dataType                    : CVParam,
                    ?dataMatrix                  : DataMatrix,
                    ?fkSmallMoleculeList         : string,
                    ?fkProteinList               : string,
                    ?fkProteinGroupList          : string,
                    ?fkPeptideConsensusList      : string

                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let datatype'               = defaultArg dataType Unchecked.defaultof<CVParam>
                let dataMatrix'             = defaultArg dataMatrix Unchecked.defaultof<DataMatrix> 
                let fkSmallMoleculeList'    = defaultArg fkSmallMoleculeList Unchecked.defaultof<string> 
                let fkProteinList'          = defaultArg fkProteinList Unchecked.defaultof<string> 
                let fkProteinGroupList'     = defaultArg fkProteinGroupList Unchecked.defaultof<string> 
                let fkPeptideConsensusList' = defaultArg fkPeptideConsensusList Unchecked.defaultof<string> 

                new GlobalQuantLayer(
                                     id',
                                     columns |> List,
                                     dataMatrix',
                                     fkDataMatrix,
                                     fkSmallMoleculeList',
                                     fkProteinList',
                                     fkProteinGroupList',
                                     fkPeptideConsensusList',
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces dataMatrix of existing object with new one.
            static member addDataMatrix
                (dataMatrix:DataMatrix) (table:GlobalQuantLayer) =
                table.DataMatrix <- dataMatrix
                table

            ///Replaces fkSmallMoleculeList of existing object with new one.
            static member addFKSmallMoleculeList
                (fkSmallMoleculeList:string) (table:GlobalQuantLayer) =
                table.FKSmallMoleculeList <- fkSmallMoleculeList
                table

            ///Replaces fkProteinList of existing object with new one.
            static member addFKProteinList
                (fkProteinList:string) (table:GlobalQuantLayer) =
                table.FKProteinList <- fkProteinList
                table

            ///Replaces fkProteinGroupList of existing object with new one.
            static member addFKProteinGroupList
                (fkProteinGroupList:string) (table:GlobalQuantLayer) =
                table.FKProteinGroupList <- fkProteinGroupList
                table

            ///Replaces fkPeptideConsensusList of existing object with new one.
            static member addFKPeptideConsensusList
                (fkPeptideConsensusList:string) (table:GlobalQuantLayer) =
                table.FKPeptideConsensusList <- fkPeptideConsensusList
                table

            ///Tries to find a globalQuantLayer-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.GlobalQuantLayer.Local do
                           if i.ID=id
                              then select (i, i.Columns, i.DataMatrix, i.FKSmallMoleculeList, i.FKProteinList, i.FKProteinGroupList, i.FKPeptideConsensusList)
                      }
                |> Seq.map (fun (globalQuantLayer, _, _, _, _, _, _) -> globalQuantLayer)
                |> (fun globalQuantLayer -> 
                    if (Seq.exists (fun globalQuantLayer' -> globalQuantLayer' <> null) globalQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.GlobalQuantLayer do
                                       if i.ID=id
                                          then select (i, i.Columns, i.DataMatrix, i.FKSmallMoleculeList, i.FKProteinList, i.FKProteinGroupList, i.FKPeptideConsensusList)
                                  }
                            |> Seq.map (fun (globalQuantLayer, _, _, _, _, _, _) -> globalQuantLayer)
                            |> (fun globalQuantLayer -> if (Seq.exists (fun globalQuantLayer' -> globalQuantLayer' <> null) globalQuantLayer) = false
                                                        then None
                                                        else Some (globalQuantLayer.Single())
                               )
                        else Some (globalQuantLayer.Single())
                   )

            ///Tries to find a globalQuantLayer-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByDataMatrix (dbContext:MzQuantML) (dataMatrix:DataMatrix) =
                query {
                       for i in dbContext.GlobalQuantLayer.Local do
                           if i.DataMatrix=dataMatrix
                              then select (i, i.Columns, i.DataMatrix, i.FKSmallMoleculeList, i.FKProteinList, i.FKProteinGroupList, i.FKPeptideConsensusList)
                      }
                |> Seq.map (fun (globalQuantLayer, _, _, _, _, _, _) -> globalQuantLayer)
                |> (fun globalQuantLayer -> 
                    if (Seq.exists (fun globalQuantLayer' -> globalQuantLayer' <> null) globalQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.GlobalQuantLayer do
                                       if i.DataMatrix=dataMatrix
                                          then select (i, i.Columns, i.DataMatrix, i.FKSmallMoleculeList, i.FKProteinList, i.FKProteinGroupList, i.FKPeptideConsensusList)
                                  }
                            |> Seq.map (fun (globalQuantLayer, _, _, _, _, _, _) -> globalQuantLayer)
                            |> (fun globalQuantLayer -> if (Seq.exists (fun globalQuantLayer' -> globalQuantLayer' <> null) globalQuantLayer) = false
                                                            then None
                                                            else Some globalQuantLayer
                               )
                        else Some globalQuantLayer
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:GlobalQuantLayer) (item2:GlobalQuantLayer) =
                item1.FKDataMatrix=item2.FKDataMatrix &&
                item1.FKSmallMoleculeList=item2.FKSmallMoleculeList && item1.FKProteinList=item2.FKProteinList &&
                item1.FKProteinGroupList=item2.FKProteinGroupList && item1.FKPeptideConsensusList=item2.FKPeptideConsensusList

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:GlobalQuantLayer) =
                    GlobalQuantLayerHandler.tryFindByDataMatrix dbContext item.DataMatrix
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match GlobalQuantLayerHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:GlobalQuantLayer) =
                GlobalQuantLayerHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type MS2AssayQuantLayerHandler =
            ///Initializes a ms2AssayQuantLayer-object with at least all necessary parameters.
            static member init
                (             
                    fkdataType                   : string,
                    columnIndex                  : string,
                    fkDataMatrix                 : string,
                    ?id                          : string,
                    ?dataType                    : CVParam,
                    ?dataMatrix                  : DataMatrix,
                    ?fkFeatureList               : string
                    
                ) =
                let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                let datatype'       = defaultArg dataType Unchecked.defaultof<CVParam>
                let dataMatrix'     = defaultArg dataMatrix Unchecked.defaultof<DataMatrix>
                let fkFeatureList'  = defaultArg fkFeatureList Unchecked.defaultof<string>

                new MS2AssayQuantLayer(
                                       id',
                                       datatype',
                                       fkdataType,
                                       columnIndex,
                                       dataMatrix',
                                       fkDataMatrix,
                                       fkFeatureList',
                                       Nullable(DateTime.Now)
                                      )

            ///Replaces datatype of existing object with new one.
            static member addDataType
                (datatype:CVParam) (table:MS2AssayQuantLayer) =
                table.DataType <- datatype
                table

            ///Replaces dataMatrix of existing object with new one.
            static member addDataMatrix
                (dataMatrix:DataMatrix) (table:MS2AssayQuantLayer) =
                table.DataMatrix <- dataMatrix
                table

            ///Replaces fkFeatureList of existing object with new one.
            static member addFKFeatureList
                (fkFeatureList:string) (table:MS2AssayQuantLayer) =
                table.FKFeatureList <- fkFeatureList
                table

            ///Tries to find a ms2AssayQuantLayer-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.MS2AssayQuantLayer.Local do
                           if i.ID=id
                              then select (i, i.FKDataType, i.FKDataMatrix, i.FKFeatureList)
                      }
                |> Seq.map (fun (ms2AssayQuantLayer, _, _, _) -> ms2AssayQuantLayer)
                |> (fun ms2AssayQuantLayer -> 
                    if (Seq.exists (fun ms2AssayQuantLayer' -> ms2AssayQuantLayer' <> null) ms2AssayQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.MS2AssayQuantLayer do
                                       if i.ID=id
                                          then select (i, i.FKDataType, i.FKDataMatrix, i.FKFeatureList)
                                  }
                            |> Seq.map (fun (ms2AssayQuantLayer, _, _, _) -> ms2AssayQuantLayer)
                            |> (fun ms2AssayQuantLayer -> if (Seq.exists (fun ms2AssayQuantLayer' -> ms2AssayQuantLayer' <> null) ms2AssayQuantLayer) = false
                                                            then None
                                                            else Some (ms2AssayQuantLayer.Single())
                               )
                        else Some (ms2AssayQuantLayer.Single())
                   )

            ///Tries to find a ms2AssayQuantLayer-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
                query {
                       for i in dbContext.MS2AssayQuantLayer.Local do
                           if i.ColumnIndex=columnIndex
                              then select (i, i.FKDataType, i.FKDataMatrix, i.FKFeatureList)
                      }
                |> Seq.map (fun (ms2AssayQuantLayer, _, _, _) -> ms2AssayQuantLayer)
                |> (fun ms2AssayQuantLayer -> 
                    if (Seq.exists (fun ms2AssayQuantLayer' -> ms2AssayQuantLayer' <> null) ms2AssayQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.MS2AssayQuantLayer do
                                       if i.ColumnIndex=columnIndex
                                          then select (i, i.FKDataType, i.FKDataMatrix, i.FKFeatureList)
                                  }
                            |> Seq.map (fun (ms2AssayQuantLayer, _, _, _) -> ms2AssayQuantLayer)
                            |> (fun ms2AssayQuantLayer -> if (Seq.exists (fun ms2AssayQuantLayer' -> ms2AssayQuantLayer' <> null) ms2AssayQuantLayer) = false
                                                            then None
                                                            else Some ms2AssayQuantLayer
                               )
                        else Some ms2AssayQuantLayer
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:MS2AssayQuantLayer) (item2:MS2AssayQuantLayer) =
                item1.FKDataType=item2.FKDataType && item1.FKDataMatrix=item2.FKDataMatrix &&
                item1.FKFeatureList=item2.FKFeatureList

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:MS2AssayQuantLayer) =
                    MS2AssayQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match MS2AssayQuantLayerHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:MS2AssayQuantLayer) =
                MS2AssayQuantLayerHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type StudyVariableQuantLayerHandler =
            ///Initializes a studyVariableQuantLayer-object with at least all necessary parameters.
            static member init
                (             
                    fkdataType                   : string,
                    columnIndex                  : string,
                    fkDataMatrix                 : string,
                    ?id                          : string,
                    ?dataType                    : CVParam,
                    ?dataMatrix                  : DataMatrix,
                    ?fkSmallMoleculeList         : string,
                    ?fkProteinList               : string,
                    ?fkProteinGroupList          : string,
                    ?fkPeptideConsensusList      : string
                    
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let datatype'               = defaultArg dataType Unchecked.defaultof<CVParam>
                let dataMatrix'             = defaultArg dataMatrix Unchecked.defaultof<DataMatrix> 
                let fkSmallMoleculeList'    = defaultArg fkSmallMoleculeList Unchecked.defaultof<string> 
                let fkProteinList'          = defaultArg fkProteinList Unchecked.defaultof<string> 
                let fkProteinGroupList'     = defaultArg fkProteinGroupList Unchecked.defaultof<string> 
                let fkPeptideConsensusList' = defaultArg fkPeptideConsensusList Unchecked.defaultof<string> 
                        

                new StudyVariableQuantLayer(
                                            id',
                                            datatype',
                                            fkdataType,
                                            columnIndex,
                                            dataMatrix',
                                            fkDataMatrix,
                                            fkSmallMoleculeList',
                                            fkProteinList',
                                            fkProteinGroupList',
                                            fkPeptideConsensusList',
                                            Nullable(DateTime.Now)
                                           )

            ///Replaces datatype of existing object with new one.
            static member addDataType
                (datatype:CVParam) (table:StudyVariableQuantLayer) =
                table.DataType <- datatype
                table

            ///Replaces dataMatrix of existing object with new one.
            static member addDataMatrix
                (dataMatrix:DataMatrix) (table:StudyVariableQuantLayer) =
                table.DataMatrix <- dataMatrix
                table

            ///Replaces fkSmallMoleculeList of existing object with new one.
            static member addFKSmallMoleculeList
                (fkSmallMoleculeList:string) (table:StudyVariableQuantLayer) =
                table.FKSmallMoleculeList <- fkSmallMoleculeList
                table

            ///Replaces fkProteinList of existing object with new one.
            static member addFKProteinList
                (fkProteinList:string) (table:StudyVariableQuantLayer) =
                table.FKProteinList <- fkProteinList
                table

            ///Replaces fkProteinGroupList of existing object with new one.
            static member addFKProteinGroupList
                (fkProteinGroupList:string) (table:StudyVariableQuantLayer) =
                table.FKProteinGroupList <- fkProteinGroupList
                table

            ///Replaces fkPeptideConsensusList of existing object with new one.
            static member addFKPeptideConsensusList
                (fkPeptideConsensusList:string) (table:StudyVariableQuantLayer) =
                table.FKPeptideConsensusList <- fkPeptideConsensusList
                table

            ///Tries to find a studyVariableQuantLayer-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.StudyVariableQuantLayer.Local do
                           if i.ID=id
                              then select (i, i.FKDataType, i.FKDataMatrix, i.FKSmallMoleculeList, i.FKProteinList, i.FKProteinGroupList, i.FKPeptideConsensusList)
                      }
                |> Seq.map (fun (studyVariableQuantLayer, _, _, _, _, _, _) -> studyVariableQuantLayer)
                |> (fun studyVariableQuantLayer -> 
                    if (Seq.exists (fun studyVariableQuantLayer' -> studyVariableQuantLayer' <> null) studyVariableQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.StudyVariableQuantLayer do
                                       if i.ID=id
                                          then select (i, i.FKDataType, i.FKDataMatrix, i.FKSmallMoleculeList, i.FKProteinList, i.FKProteinGroupList, i.FKPeptideConsensusList)
                                  }
                            |> Seq.map (fun (studyVariableQuantLayer, _, _, _, _, _, _) -> studyVariableQuantLayer)
                            |> (fun studyVariableQuantLayer -> if (Seq.exists (fun studyVariableQuantLayer' -> studyVariableQuantLayer' <> null) studyVariableQuantLayer) = false
                                                                then None
                                                                else Some (studyVariableQuantLayer.Single())
                               )
                        else Some (studyVariableQuantLayer.Single())
                   )

            ///Tries to find a studyVariableQuantLayer-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
                query {
                       for i in dbContext.StudyVariableQuantLayer.Local do
                           if i.ColumnIndex=columnIndex
                              then select (i, i.FKDataType, i.FKDataMatrix, i.FKSmallMoleculeList, i.FKProteinList, i.FKProteinGroupList, i.FKPeptideConsensusList)
                      }
                |> Seq.map (fun (studyVariableQuantLayer, _, _, _, _, _, _) -> studyVariableQuantLayer)
                |> (fun studyVariableQuantLayer -> 
                    if (Seq.exists (fun studyVariableQuantLayer' -> studyVariableQuantLayer' <> null) studyVariableQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.StudyVariableQuantLayer do
                                       if i.ColumnIndex=columnIndex
                                          then select (i, i.FKDataType, i.FKDataMatrix, i.FKSmallMoleculeList, i.FKProteinList, i.FKProteinGroupList, i.FKPeptideConsensusList)
                                  }
                            |> Seq.map (fun (studyVariableQuantLayer, _, _, _, _, _, _) -> studyVariableQuantLayer)
                            |> (fun studyVariableQuantLayer -> if (Seq.exists (fun studyVariableQuantLayer' -> studyVariableQuantLayer' <> null) studyVariableQuantLayer) = false
                                                                    then None
                                                                    else Some studyVariableQuantLayer
                               )
                        else Some studyVariableQuantLayer
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:StudyVariableQuantLayer) (item2:StudyVariableQuantLayer) =
                item1.FKDataType=item2.FKDataType && item1.FKDataMatrix=item2.FKDataMatrix &&
                item1.FKSmallMoleculeList=item2.FKSmallMoleculeList && item1.FKProteinList=item2.FKProteinList &&
                item1.FKProteinGroupList=item2.FKProteinGroupList && item1.FKPeptideConsensusList=item2.FKPeptideConsensusList

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:StudyVariableQuantLayer) =
                    StudyVariableQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match StudyVariableQuantLayerHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:StudyVariableQuantLayer) =
                StudyVariableQuantLayerHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type RatioQuantLayerHandler =
            ///Initializes a ratioQuantLayer-object with at least all necessary parameters.
            static member init
                (             
                    columnIndex                  : string,
                    fkDataMatrix                 : string,
                    ?id                          : string,
                    ?dataMatrix                  : DataMatrix
                    
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let dataMatrix'             = defaultArg dataMatrix Unchecked.defaultof<DataMatrix>
                        

                new RatioQuantLayer(
                                    id',
                                    columnIndex,
                                    dataMatrix',
                                    fkDataMatrix,
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces dataMatrix of existing object with new one.
            static member addDataMatrix
                (dataMatrix:DataMatrix) (table:RatioQuantLayer) =
                table.DataMatrix <- dataMatrix
                table

            ///Tries to find a ratioQuantLayer-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.RatioQuantLayer.Local do
                           if i.ID=id
                              then select (i, i.DataMatrix)
                      }
                |> Seq.map (fun (studyVariableQuantLayer, _) -> studyVariableQuantLayer)
                |> (fun studyVariableQuantLayer -> 
                    if (Seq.exists (fun studyVariableQuantLayer' -> studyVariableQuantLayer' <> null) studyVariableQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.RatioQuantLayer do
                                       if i.ID=id
                                          then select (i, i.DataMatrix)
                                  }
                            |> Seq.map (fun (studyVariableQuantLayer, _) -> studyVariableQuantLayer)
                            |> (fun studyVariableQuantLayer -> if (Seq.exists (fun studyVariableQuantLayer' -> studyVariableQuantLayer' <> null) studyVariableQuantLayer) = false
                                                                then None
                                                                else Some (studyVariableQuantLayer.Single())
                               )
                        else Some (studyVariableQuantLayer.Single())
                   )

            ///Tries to find a ratioQuantLayer-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
                query {
                       for i in dbContext.RatioQuantLayer.Local do
                           if i.ColumnIndex=columnIndex
                              then select (i, i.DataMatrix)
                      }
                |> Seq.map (fun (studyVariableQuantLayer, _) -> studyVariableQuantLayer)
                |> (fun studyVariableQuantLayer -> 
                    if (Seq.exists (fun studyVariableQuantLayer' -> studyVariableQuantLayer' <> null) studyVariableQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.RatioQuantLayer do
                                       if i.ColumnIndex=columnIndex
                                          then select (i, i.DataMatrix)
                                  }
                            |> Seq.map (fun (studyVariableQuantLayer, _) -> studyVariableQuantLayer)
                            |> (fun studyVariableQuantLayer -> if (Seq.exists (fun studyVariableQuantLayer' -> studyVariableQuantLayer' <> null) studyVariableQuantLayer) = false
                                                                    then None
                                                                    else Some studyVariableQuantLayer
                               )
                        else Some studyVariableQuantLayer
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:RatioQuantLayer) (item2:RatioQuantLayer) =
                item1.FKDataMatrix=item2.FKDataMatrix

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:RatioQuantLayer) =
                    RatioQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match RatioQuantLayerHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:RatioQuantLayer) =
                RatioQuantLayerHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProcessingMethodHandler =
            ///Initializes a processingMethod-object with at least all necessary parameters.
            static member init
                (             
                    order                        : int,
                    ?id                          : string,
                    ?fkDataProcessing            : string,
                    ?details                     : seq<ProcessingMethodParam>
                    
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let fkDataProcessing'            = defaultArg fkDataProcessing Unchecked.defaultof<string>
                let details'                     = convertOptionToList details
                        

                new ProcessingMethod(
                                     id', 
                                     Nullable(order),
                                     fkDataProcessing',
                                     details',
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces fkDataProcessing of existing object with new one.
            static member addFKDataProcessing
                (fkDataProcessing:string) (table:ProcessingMethod) =
                table.FKDataProcessing <- fkDataProcessing
                table

            ///Adds a processingMethodParam to an existing object.
            static member addDetail (processingMethodParam:ProcessingMethodParam) (table:ProcessingMethod) =
                let result = table.Details <- addToList table.Details processingMethodParam
                table

            ///Adds a collection of processingMethodParams to an existing object.
            static member addDetails (processingMethodParams:seq<ProcessingMethodParam>) (table:ProcessingMethod) =
                let result = table.Details <- addCollectionToList table.Details processingMethodParams
                table

            ///Tries to find a processingMethod-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.ProcessingMethod.Local do
                           if i.ID=id
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (processingMethod, _) -> processingMethod)
                |> (fun processingMethod -> 
                    if (Seq.exists (fun processingMethod' -> processingMethod' <> null) processingMethod) = false
                        then 
                            query {
                                   for i in dbContext.ProcessingMethod do
                                       if i.ID=id
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (processingMethod, _) -> processingMethod)
                            |> (fun processingMethod -> if (Seq.exists (fun processingMethod' -> processingMethod' <> null) processingMethod) = false
                                                        then None
                                                        else Some (processingMethod.Single())
                               )
                        else Some (processingMethod.Single())
                   )

            ///Tries to find a processingMethod-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByOrder (dbContext:MzQuantML) (order:Nullable<int>) =
                query {
                       for i in dbContext.ProcessingMethod.Local do
                           if i.Order=order
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (processingMethod, _) -> processingMethod)
                |> (fun processingMethod -> 
                    if (Seq.exists (fun processingMethod' -> processingMethod' <> null) processingMethod) = false
                        then 
                            query {
                                   for i in dbContext.ProcessingMethod do
                                       if i.Order=order
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (processingMethod, _) -> processingMethod)
                            |> (fun processingMethod -> if (Seq.exists (fun processingMethod' -> processingMethod' <> null) processingMethod) = false
                                                            then None
                                                            else Some processingMethod
                               )
                        else Some processingMethod
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProcessingMethod) (item2:ProcessingMethod) =
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)            

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:ProcessingMethod) =
                    ProcessingMethodHandler.tryFindByOrder dbContext item.Order
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProcessingMethodHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProcessingMethod) =
                ProcessingMethodHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type DataProcessingHandler =
            ///Initializes a dataProcessing-object with at least all necessary parameters.
            static member init
                (             
                    order                        : int,
                    fkSoftware                   : string,
                    processingMethods            : seq<ProcessingMethod>,
                    ?id                          : string,
                    ?software                    : Software,
                    ?inputObjects                : string,
                    ?outputObjects               : string,
                    ?fkMzQuantMLDocument         : string                   

                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let software'                    = defaultArg software Unchecked.defaultof<Software>
                let inputObjects'                = defaultArg inputObjects Unchecked.defaultof<string>
                let outputObjects'               = defaultArg outputObjects Unchecked.defaultof<string>
                let fkMzQuantMLDocument'         = defaultArg fkMzQuantMLDocument Unchecked.defaultof<string>

                new DataProcessing(
                                   id', 
                                   Nullable(order),
                                   software',
                                   fkSoftware,
                                   inputObjects',
                                   outputObjects',
                                   processingMethods |> List,
                                   fkMzQuantMLDocument',
                                   Nullable(DateTime.Now)
                                  )

            ///Replaces software of existing object with new one.
            static member addSoftware
                (software:Software) (table:DataProcessing) =
                table.Software <- software
                table
            
            ///Replaces inputObjects of existing object with new one.
            static member addInputObjects
                (inputObjects:string) (table:DataProcessing) =
                table.InputObjects <- inputObjects
                table

            ///Replaces outputObjects of existing object with new one.
            static member addOutputObjects
                (outputObjects:string) (table:DataProcessing) =
                table.OutputObjects <- outputObjects
                table

            ///Replaces fkMzQuantMLDocument of existing object with new one.
            static member addFkMzQuantMLDocument
                (fkMzQuantMLDocument:string) (table:DataProcessing) =
                let result = table.FKMzQuantMLDocument <- fkMzQuantMLDocument
                table

            ///Tries to find a dataProcessing-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.DataProcessing.Local do
                           if i.ID=id
                              then select (i, i.Software, i.ProcessingMethods)
                      }
                |> Seq.map (fun (dataProcessing, _, _) -> dataProcessing)
                |> (fun dataProcessing -> 
                    if (Seq.exists (fun dataProcessing' -> dataProcessing' <> null) dataProcessing) = false
                        then 
                            query {
                                   for i in dbContext.DataProcessing do
                                       if i.ID=id
                                          then select (i, i.Software, i.ProcessingMethods)
                                  }
                            |> Seq.map (fun (dataProcessing, _, _) -> dataProcessing)
                            |> (fun dataProcessing -> if (Seq.exists (fun dataProcessing' -> dataProcessing' <> null) dataProcessing) = false
                                                        then None
                                                        else Some (dataProcessing.Single())
                               )
                        else Some (dataProcessing.Single())
                   )

            ///Tries to find a dataProcessing-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByOrder (dbContext:MzQuantML) (order:Nullable<int>) =
                query {
                       for i in dbContext.DataProcessing.Local do
                           if i.Order=order
                              then select (i, i.Software, i.ProcessingMethods)
                      }
                |> Seq.map (fun (dataProcessing, _, _) -> dataProcessing)
                |> (fun dataProcessing -> 
                    if (Seq.exists (fun dataProcessing' -> dataProcessing' <> null) dataProcessing) = false
                        then 
                            query {
                                   for i in dbContext.DataProcessing do
                                       if i.Order=order
                                          then select (i, i.Software, i.ProcessingMethods)
                                  }
                            |> Seq.map (fun (dataProcessing, _, _) -> dataProcessing)
                            |> (fun dataProcessing -> if (Seq.exists (fun dataProcessing' -> dataProcessing' <> null) dataProcessing) = false
                                                            then None
                                                            else Some dataProcessing
                               )
                        else Some dataProcessing
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:DataProcessing) (item2:DataProcessing) =
                item1.FKSoftware=item2.FKSoftware && item1.InputObjects=item2.InputObjects &&
                item1.OutputObjects=item2.OutputObjects && item1.ProcessingMethods=item2.ProcessingMethods

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:DataProcessing) =
                    DataProcessingHandler.tryFindByOrder dbContext item.Order
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match DataProcessingHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:DataProcessing) =
                DataProcessingHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type DBIdentificationRefHandler =
            ///Initializes a dbIdentificationRef-object with at least all necessary parameters.
            static member init
                (             
                    fkExternalFile               : string,
                    fkSearchDatabase             : string,
                    ?id                          : string,
                    ?searchDatabase              : SearchDatabase,
                    ?fkSmallMolecule             : string
                    
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let searchDatabase'              = defaultArg searchDatabase Unchecked.defaultof<SearchDatabase>
                let fkSmallMolecule'             = defaultArg fkSmallMolecule Unchecked.defaultof<string>

                new DBIdentificationRef(
                                        id', 
                                        fkExternalFile,
                                        searchDatabase',
                                        fkSearchDatabase,
                                        fkSmallMolecule',
                                        Nullable(DateTime.Now)
                                       )

            ///Replaces searchDatabase of existing object with new one.
            static member addSearchDatabase
                (searchDatabase:string) (table:DBIdentificationRef) =
                table.FKSearchDatabase <- searchDatabase
                table

            ///Replaces fkSmallMolecule of existing object with new one.
            static member addFKSmallMolecule
                (fkSmallMolecule:string) (table:DBIdentificationRef) =
                table.FKSmallMolecule <- fkSmallMolecule
                table

            ///Tries to find a dbIdentificationRef-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.DBIdentificationRef.Local do
                           if i.ID=id
                              then select (i, i.SearchDatabase)
                      }
                |> Seq.map (fun (dbIdentificationRef, _) -> dbIdentificationRef)
                |> (fun dbIdentificationRef -> 
                    if (Seq.exists (fun dbIdentificationRef' -> dbIdentificationRef' <> null) dbIdentificationRef) = false
                        then 
                            query {
                                   for i in dbContext.DBIdentificationRef do
                                       if i.ID=id
                                          then select (i, i.SearchDatabase)
                                  }
                            |> Seq.map (fun (dbIdentificationRef, _) -> dbIdentificationRef)
                            |> (fun dbIdentificationRef -> if (Seq.exists (fun dbIdentificationRef' -> dbIdentificationRef' <> null) dbIdentificationRef) = false
                                                            then None
                                                            else Some (dbIdentificationRef.Single())
                               )
                        else Some (dbIdentificationRef.Single())
                   )

            ///Tries to find a dbIdentificationRef-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFKExternalFile (dbContext:MzQuantML) (fkExternalFile:string) =
                query {
                       for i in dbContext.DBIdentificationRef.Local do
                           if i.FKExternalFile=fkExternalFile
                              then select (i, i.SearchDatabase)
                      }
                |> Seq.map (fun (dbIdentificationRef, _) -> dbIdentificationRef)
                |> (fun dbIdentificationRef -> 
                    if (Seq.exists (fun dbIdentificationRef' -> dbIdentificationRef' <> null) dbIdentificationRef) = false
                        then 
                            query {
                                   for i in dbContext.DBIdentificationRef do
                                       if i.FKExternalFile=fkExternalFile
                                          then select (i, i.SearchDatabase)
                                  }
                            |> Seq.map (fun (dbIdentificationRef, _) -> dbIdentificationRef)
                            |> (fun dbIdentificationRef -> if (Seq.exists (fun dbIdentificationRef' -> dbIdentificationRef' <> null) dbIdentificationRef) = false
                                                            then None
                                                            else Some dbIdentificationRef
                               )
                        else Some dbIdentificationRef
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:DBIdentificationRef) (item2:DBIdentificationRef) =
                item1.FKSearchDatabase=item2.FKSearchDatabase && item1.FKSmallMolecule=item2.FKSmallMolecule

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:DBIdentificationRef) =
                    DBIdentificationRefHandler.tryFindByFKExternalFile dbContext item.FKExternalFile
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match DBIdentificationRefHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:DBIdentificationRef) =
                DBIdentificationRefHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type FeatureHandler =
            ///Initializes a feature-object with at least all necessary parameters.
            static member init
                (             
                    charge                       : int,
                    mz                           : float,
                    retentionTime                : float,
                    ?id                          : string,
                    ?fkChromatogram              : string,
                    ?rawFile                     : RawFile,
                    ?fkRawFile                   : string,
                    ?fkSpectrum                  : string,
                    ?massTraces                  : seq<MassTraceParam>,
                    ?fkSmallMolecule             : string,
                    ?fkFeatureList               : string,
                    ?details                     : seq<FeatureParam>
                    
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let fkChromatogram'              = defaultArg fkChromatogram Unchecked.defaultof<string>
                let rawFile'                     = defaultArg rawFile Unchecked.defaultof<RawFile>
                let fkRawFile'                   = defaultArg fkRawFile Unchecked.defaultof<string>
                let fkSpectrum'                  = defaultArg fkSpectrum Unchecked.defaultof<string>
                let massTraces'                  = convertOptionToList massTraces
                let fkSmallMolecule'             = defaultArg fkSmallMolecule Unchecked.defaultof<string>
                let fkFeatureList'               = defaultArg fkFeatureList Unchecked.defaultof<string>
                let details'                     = convertOptionToList details
                

                new Feature(
                            id', 
                            Nullable(charge),
                            fkChromatogram',
                            Nullable(mz),
                            rawFile',
                            fkRawFile',
                            Nullable(retentionTime),
                            fkSpectrum',
                            massTraces',
                            fkSmallMolecule',
                            fkFeatureList',
                            details',
                            Nullable(DateTime.Now)
                           )

            ///Replaces fkChromatogram of existing object with new one.
            static member addFKChromatogram
                (fkChromatogram:string) (table:Feature) =
                table.FKChromatogram <- fkChromatogram
                table

            ///Replaces rawFile of existing object with new one.
            static member addRawFile
                (rawFile:RawFile) (table:Feature) =
                table.RawFile <- rawFile
                table

            ///Replaces fkRawFile of existing object with new one.
            static member addFKRawFile
                (fkRawFile:string) (table:Feature) =
                table.FKRawFile <- fkRawFile
                table

            ///Replaces fkSpectrum of existing object with new one.
            static member addFKSpectrum
                (fkSpectrum:string) (table:Feature) =
                table.FKSpectrum <- fkSpectrum
                table
            
            ///Adds a massTraceParam to an existing object.
            static member addMassTrace (massTraceParam:MassTraceParam) (table:Feature) =
                let result = table.MassTraces <- addToList table.MassTraces massTraceParam
                table

            ///Adds a collection of massTraceParams to an existing object.
            static member addMassTraces (massTraceParams:seq<MassTraceParam>) (table:Feature) =
                let result = table.MassTraces <- addCollectionToList table.MassTraces massTraceParams
                table

            ///Replaces fkSmallMolecule of existing object with new one.
            static member addFKSmallMolecule
                (fkSmallMolecule:string) (table:Feature) =
                table.FKSmallMolecule <- fkSmallMolecule
                table

            ///Replaces fkFeatureList of existing object with new one.
            static member addFKFeatureList
                (fkFeatureList:string) (table:Feature) =
                table.FKFeatureList <- fkFeatureList
                table

            ///Adds a processingMethodParam to an existing object.
            static member addDetail (processingMethodParam:FeatureParam) (table:Feature) =
                let result = table.Details <- addToList table.Details processingMethodParam
                table

            ///Adds a collection of processingMethodParams to an existing object.
            static member addDetails (processingMethodParams:seq<FeatureParam>) (table:Feature) =
                let result = table.Details <- addCollectionToList table.Details processingMethodParams
                table

            ///Tries to find a feature-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.Feature.Local do
                           if i.ID=id
                              then select (i, i.MassTraces, i.RawFile, i.Details)
                      }
                |> Seq.map (fun (feature, _, _, _) -> feature)
                |> (fun feature -> 
                    if (Seq.exists (fun feature' -> feature' <> null) feature) = false
                        then 
                            query {
                                   for i in dbContext.Feature do
                                       if i.ID=id
                                          then select (i, i.MassTraces, i.RawFile, i.Details)
                                  }
                            |> Seq.map (fun (feature, _, _, _) -> feature)
                            |> (fun feature -> if (Seq.exists (fun feature' -> feature' <> null) feature) = false
                                                then None
                                                else Some (feature.Single())
                               )
                        else Some (feature.Single())
                   )

            ///Tries to find a feature-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByMz (dbContext:MzQuantML) (mz:Nullable<float>) =
                query {
                       for i in dbContext.Feature.Local do
                           if i.MZ=mz
                              then select (i, i.MassTraces, i.RawFile, i.Details)
                      }
                |> Seq.map (fun (feature, _, _, _) -> feature)
                |> (fun feature -> 
                    if (Seq.exists (fun feature' -> feature' <> null) feature) = false
                        then 
                            query {
                                   for i in dbContext.Feature do
                                       if i.MZ=mz
                                          then select (i, i.MassTraces, i.RawFile, i.Details)
                                  }
                            |> Seq.map (fun (feature, _, _, _) -> feature)
                            |> (fun feature -> if (Seq.exists (fun feature' -> feature' <> null) feature) = false
                                                            then None
                                                            else Some feature
                               )
                        else Some feature
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Feature) (item2:Feature) =
                item1.Charge=item2.Charge && item1.RetentionTime=item2.RetentionTime &&
                item1.FKChromatogram=item2.FKChromatogram && item1.FKRawFile=item2.FKRawFile &&
                item1.FKSpectrum=item2.FKSpectrum && item1.MassTraces=item2.MassTraces &&
                item1.FKSpectrum=item2.FKSpectrum && item1.MassTraces=item2.MassTraces

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Feature) =
                    FeatureHandler.tryFindByMz dbContext item.MZ
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match FeatureHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Feature) =
                FeatureHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SmallMoleculeHandler =
            ///Initializes a smallMolecule-object with at least all necessary parameters.
            static member init
                (             
                    ?id                          : string,
                    ?modifications               : seq<Modification>,
                    ?dbIdentificationRefs        : seq<DBIdentificationRef>,
                    ?features                    : seq<Feature>,
                    ?fkSmallMoleculeList         : string,
                    ?details                     : seq<SmallMoleculeParam>
                    
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let modifications'               = convertOptionToList modifications
                let dbIdentificationRefs'        = convertOptionToList dbIdentificationRefs
                let features'                    = convertOptionToList features
                let fkSmallMoleculeList'         = defaultArg fkSmallMoleculeList Unchecked.defaultof<string>
                let details'                     = convertOptionToList details
                

                new SmallMolecule(
                                  id', 
                                  modifications',
                                  dbIdentificationRefs',
                                  features',
                                  fkSmallMoleculeList',
                                  details',
                                  Nullable(DateTime.Now)
                                 )
            
            ///Replaces fkSmallMoleculeList of existing object with new one.
            static member addFKSmallMoleculeList
                (fkSmallMoleculeList:string) (table:SmallMolecule) =
                table.FKSmallMoleculeList <- fkSmallMoleculeList
                table

            ///Adds a modification to an existing object.
            static member addModification (modification:Modification) (table:SmallMolecule) =
                let result = table.Modifications <- addToList table.Modifications modification
                table

            ///Adds a collection of modifications to an existing object.
            static member addModifications (modifications:seq<Modification>) (table:SmallMolecule) =
                let result = table.Modifications <- addCollectionToList table.Modifications modifications
                table

            ///Adds a dbIdentificationRef to an existing object.
            static member addDBIdentificationRef (dbIdentificationRef:DBIdentificationRef) (table:SmallMolecule) =
                let result = table.DBIdentificationRefs <- addToList table.DBIdentificationRefs dbIdentificationRef
                table

            ///Adds a collection of dbIdentificationRefs to an existing object.
            static member addDBIdentificationRefs (dbIdentificationRefs:seq<DBIdentificationRef>) (table:SmallMolecule) =
                let result = table.DBIdentificationRefs <- addCollectionToList table.DBIdentificationRefs dbIdentificationRefs
                table

            ///Adds a feature to an existing object.
            static member addFeature (feature:Feature) (table:SmallMolecule) =
                let result = table.Features <- addToList table.Features feature
                table

            ///Adds a collection of features to an existing object.
            static member addFeatures (features:seq<Feature>) (table:SmallMolecule) =
                let result = table.Features <- addCollectionToList table.Features features
                table

            ///Adds a smallMoleculeParam to an existing object.
            static member addDetail (smallMoleculeParam:SmallMoleculeParam) (table:SmallMolecule) =
                let result = table.Details <- addToList table.Details smallMoleculeParam
                table

            ///Adds a collection of smallMoleculeParams to an existing object.
            static member addDetails (smallMoleculeParam:seq<SmallMoleculeParam>) (table:SmallMolecule) =
                let result = table.Details <- addCollectionToList table.Details smallMoleculeParam
                table

            ///Tries to find a smallMolecule-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.SmallMolecule.Local do
                           if i.ID=id
                              then select (i, i.Modifications, i.DBIdentificationRefs, i.Features, i.Details)
                      }
                |> Seq.map (fun (smallMolecule, _, _, _, _) -> smallMolecule)
                |> (fun smallMolecule -> 
                    if (Seq.exists (fun smallMolecule' -> smallMolecule' <> null) smallMolecule) = false
                        then 
                            query {
                                   for i in dbContext.SmallMolecule do
                                       if i.ID=id
                                          then select (i, i.Modifications, i.DBIdentificationRefs, i.Features, i.Details)
                                  }
                            |> Seq.map (fun (smallMolecule, _, _, _, _) -> smallMolecule)
                            |> (fun smallMolecule -> if (Seq.exists (fun smallMolecule' -> smallMolecule' <> null) smallMolecule) = false
                                                        then None
                                                        else Some (smallMolecule.Single())
                               )
                        else Some (smallMolecule.Single())
                   )

            ///Tries to find a smallMolecule-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFKSmallMoleculeList (dbContext:MzQuantML) (fkSmallMoleculeList:string) =
                query {
                       for i in dbContext.SmallMolecule.Local do
                           if i.FKSmallMoleculeList=fkSmallMoleculeList
                              then select (i, i.Modifications, i.DBIdentificationRefs, i.Features, i.Details)
                      }
                |> Seq.map (fun (smallMolecule, _, _, _, _) -> smallMolecule)
                |> (fun smallMolecule -> 
                    if (Seq.exists (fun smallMolecule' -> smallMolecule' <> null) smallMolecule) = false
                        then 
                            query {
                                   for i in dbContext.SmallMolecule do
                                       if i.FKSmallMoleculeList=fkSmallMoleculeList
                                          then select (i, i.Modifications, i.DBIdentificationRefs, i.Features, i.Details)
                                  }
                            |> Seq.map (fun (smallMolecule, _, _, _, _) -> smallMolecule)
                            |> (fun smallMolecule -> if (Seq.exists (fun smallMolecule' -> smallMolecule' <> null) smallMolecule) = false
                                                            then None
                                                            else Some smallMolecule
                               )
                        else Some smallMolecule
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SmallMolecule) (item2:SmallMolecule) =
                item1.DBIdentificationRefs=item2.DBIdentificationRefs && item1.Features=item2.Features &&
                item1.Modifications=item2.Modifications &&
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:SmallMolecule) =
                    SmallMoleculeHandler.tryFindByFKSmallMoleculeList dbContext item.FKSmallMoleculeList
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SmallMoleculeHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:SmallMolecule) =
                SmallMoleculeHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SmallMoleculeListHandler =
            ///Initializes a smallMoleculeList-object with at least all necessary parameters.
            static member init
                (             
                    smallMolecules               : seq<SmallMolecule>,
                    ?id                          : string,
                    ?globalQuantLayer            : seq<GlobalQuantLayer>,
                    ?assayQuantLayer             : seq<AssayQuantLayer>,
                    ?studyVariableQuantLayer     : seq<StudyVariableQuantLayer>,
                    ?ratioQuantLayer             : RatioQuantLayer,
                    ?fkRatioQuantLayer           : string,
                    ?details                     : seq<SmallMoleculeListParam>
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let globalQuantLayer'               = convertOptionToList globalQuantLayer
                let assayQuantLayer'                = convertOptionToList assayQuantLayer
                let studyVariableQuantLayer'        = convertOptionToList studyVariableQuantLayer
                let ratioQuantLayer'                = defaultArg ratioQuantLayer Unchecked.defaultof<RatioQuantLayer>
                let fkRatioQuantLayer'              = defaultArg fkRatioQuantLayer Unchecked.defaultof<string>
                let details'                        = convertOptionToList details

                new SmallMoleculeList(
                                      id',
                                      smallMolecules |> List,
                                      globalQuantLayer',
                                      assayQuantLayer',
                                      studyVariableQuantLayer',
                                      ratioQuantLayer',
                                      fkRatioQuantLayer',
                                      details',
                                      Nullable(DateTime.Now)
                                     )
            
            ///Adds a globalQuantLayer to an existing object.
            static member addGlobalQuantLayer (globalQuantLayer:GlobalQuantLayer) (table:SmallMoleculeList) =
                let result = table.GlobalQuantLayers <- addToList table.GlobalQuantLayers globalQuantLayer
                table

            ///Adds a collection of globalQuantLayers to an existing object.
            static member addGlobalQuantLayers (globalQuantLayers:seq<GlobalQuantLayer>) (table:SmallMoleculeList) =
                let result = table.GlobalQuantLayers <- addCollectionToList table.GlobalQuantLayers globalQuantLayers
                table

            ///Adds a assayQuantLayer to an existing object.
            static member addAssayQuantLayer (assayQuantLayer:AssayQuantLayer) (table:SmallMoleculeList) =
                let result = table.AssayQuantLayers <- addToList table.AssayQuantLayers assayQuantLayer
                table

            ///Adds a collection of assayQuantLayers to an existing object.
            static member addAssayQuantLayers (assayQuantLayers:seq<AssayQuantLayer>) (table:SmallMoleculeList) =
                let result = table.AssayQuantLayers <- addCollectionToList table.AssayQuantLayers assayQuantLayers
                table

            ///Adds a studyVariableQuantLayer to an existing object.
            static member addStudyVariableQuantLayer (studyVariableQuantLayer:StudyVariableQuantLayer) (table:SmallMoleculeList) =
                let result = table.StudyVariableQuantLayers <- addToList table.StudyVariableQuantLayers studyVariableQuantLayer
                table

            ///Adds a collection of studyVariableQuantLayers to an existing object.
            static member addStudyVariableQuantLayers (studyVariableQuantLayers:seq<StudyVariableQuantLayer>) (table:SmallMoleculeList) =
                let result = table.StudyVariableQuantLayers <- addCollectionToList table.StudyVariableQuantLayers studyVariableQuantLayers
                table

            ///Replaces ratioQuantLayer of existing object with new one.
            static member addRatioQuantLayer (ratioQuantLayer:RatioQuantLayer) (table:SmallMoleculeList) =
                table.RatioQuantLayer <- ratioQuantLayer
                table

            ///Replaces fkRatioQuantLayer of existing object with new one.
            static member addFKRatioQuantLayer (fkRatioQuantLayer:string) (table:SmallMoleculeList) =
                table.FKRatioQuantLayer <- fkRatioQuantLayer
                table

            ///Adds a smallMoleculeListParam to an existing object.
            static member addDetail (smallMoleculeListParam:SmallMoleculeListParam) (table:SmallMoleculeList) =
                let result = table.Details <- addToList table.Details smallMoleculeListParam
                table

            ///Adds a collection of smallMoleculeListParams to an existing object.
            static member addDetails (smallMoleculeListParams:seq<SmallMoleculeListParam>) (table:SmallMoleculeList) =
                let result = table.Details <- addCollectionToList table.Details smallMoleculeListParams
                table

            ///Tries to find a smallMoleculeList-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.SmallMoleculeList.Local do
                           if i.ID=id
                              then select (i, i.SmallMolecules, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                      }
                |> Seq.map (fun (smallMoleculeList, _, _, _, _, _, _) -> smallMoleculeList)
                |> (fun smallMoleculeList -> 
                    if (Seq.exists (fun smallMoleculeList' -> smallMoleculeList' <> null) smallMoleculeList) = false
                        then 
                            query {
                                   for i in dbContext.SmallMoleculeList do
                                       if i.ID=id
                                          then select (i, i.SmallMolecules, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                                  }
                            |> Seq.map (fun (smallMoleculeList, _, _, _, _, _, _) -> smallMoleculeList)
                            |> (fun smallMoleculeList -> if (Seq.exists (fun smallMoleculeList' -> smallMoleculeList' <> null) smallMoleculeList) = false
                                                            then None
                                                            else Some (smallMoleculeList.Single())
                               )
                        else Some (smallMoleculeList.Single())
                   )

            ///Tries to find a smallMoleculeList-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFKRatioQuantLayer (dbContext:MzQuantML) (fkRatioQuantLayer:string) =
                query {
                       for i in dbContext.SmallMoleculeList.Local do
                           if i.FKRatioQuantLayer=fkRatioQuantLayer
                              then select (i, i.SmallMolecules, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                      }
                |> Seq.map (fun (smallMoleculeList, _, _, _, _, _, _) -> smallMoleculeList)
                |> (fun smallMoleculeList -> 
                    if (Seq.exists (fun smallMoleculeList' -> smallMoleculeList' <> null) smallMoleculeList) = false
                        then 
                            query {
                                   for i in dbContext.SmallMoleculeList do
                                       if i.FKRatioQuantLayer=fkRatioQuantLayer
                                          then select (i, i.SmallMolecules, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                                  }
                            |> Seq.map (fun (smallMoleculeList, _, _, _, _, _, _) -> smallMoleculeList)
                            |> (fun smallMoleculeList -> if (Seq.exists (fun smallMoleculeList' -> smallMoleculeList' <> null) smallMoleculeList) = false
                                                            then None
                                                            else Some smallMoleculeList
                               )
                        else Some smallMoleculeList
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SmallMoleculeList) (item2:SmallMoleculeList) =
                item1.GlobalQuantLayers=item2.GlobalQuantLayers && item1.AssayQuantLayers=item2.AssayQuantLayers && 
                item1.StudyVariableQuantLayers=item2.StudyVariableQuantLayers && 
                item1.FKRatioQuantLayer=item2.FKRatioQuantLayer &&
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:SmallMoleculeList) =
                    SmallMoleculeListHandler.tryFindByFKRatioQuantLayer dbContext item.FKRatioQuantLayer
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SmallMoleculeListHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:SmallMoleculeList) =
                SmallMoleculeListHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type FeatureQuantLayerHandler =
            ///Initializes a featureQuantLayer-object with at least all necessary parameters.
            static member init
                (             
                    columns         : seq<Column>,
                    fkDataMatrix    : string,
                    ?id             : string,
                    ?dataMatrix     : DataMatrix,
                    ?fkFeatureList  : string
                    
                ) =
                let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                let dataMatrix'     = defaultArg dataMatrix Unchecked.defaultof<DataMatrix>
                let fkFeatureList'  = defaultArg fkFeatureList Unchecked.defaultof<string>

                new FeatureQuantLayer(
                                      id',
                                      columns |> List,
                                      dataMatrix',
                                      fkDataMatrix,
                                      fkFeatureList',
                                      Nullable(DateTime.Now)
                                     )

            ///Replaces dataMatrix of existing object with new one.
            static member addDataMatrix (dataMatrix:DataMatrix) (table:FeatureQuantLayer) =
                table.DataMatrix <- dataMatrix
                table

            ///Replaces fkFeatureList of existing object with new one.
            static member addFKFeatureList (fkFeatureList:string) (table:FeatureQuantLayer) =
                table.FKFeatureList <- fkFeatureList
                table

            ///Tries to find a featureQuantLayer-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.FeatureQuantLayer.Local do
                           if i.ID=id
                              then select (i, i.Columns, i.DataMatrix)
                      }
                |> Seq.map (fun (featureQuantLayer, _, _) -> featureQuantLayer)
                |> (fun featureQuantLayer -> 
                    if (Seq.exists (fun featureQuantLayer' -> featureQuantLayer' <> null) featureQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.FeatureQuantLayer do
                                       if i.ID=id
                                          then select (i, i.Columns, i.DataMatrix)
                                  }
                            |> Seq.map (fun (featureQuantLayer, _, _) -> featureQuantLayer)
                            |> (fun featureQuantLayer -> if (Seq.exists (fun featureQuantLayer' -> featureQuantLayer' <> null) featureQuantLayer) = false
                                                            then None
                                                            else Some (featureQuantLayer.Single())
                               )
                        else Some (featureQuantLayer.Single())
                   )

            ///Tries to find a featureQuantLayer-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFKDataMatrix (dbContext:MzQuantML) (fkDataMatrix:string) =
                query {
                       for i in dbContext.FeatureQuantLayer.Local do
                           if i.FKDataMatrix=fkDataMatrix
                              then select (i, i.Columns, i.DataMatrix)
                      }
                |> Seq.map (fun (featureQuantLayer, _, _) -> featureQuantLayer)
                |> (fun featureQuantLayer -> 
                    if (Seq.exists (fun featureQuantLayer' -> featureQuantLayer' <> null) featureQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.FeatureQuantLayer do
                                       if i.FKDataMatrix=fkDataMatrix
                                          then select (i, i.Columns, i.DataMatrix)
                                  }
                            |> Seq.map (fun (featureQuantLayer, _, _) -> featureQuantLayer)
                            |> (fun featureQuantLayer -> if (Seq.exists (fun featureQuantLayer' -> featureQuantLayer' <> null) featureQuantLayer) = false
                                                            then None
                                                            else Some featureQuantLayer
                               )
                        else Some featureQuantLayer
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:FeatureQuantLayer) (item2:FeatureQuantLayer) =
                item1.FKFeatureList=item2.FKFeatureList

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:FeatureQuantLayer) =
                    FeatureQuantLayerHandler.tryFindByFKDataMatrix dbContext item.FKDataMatrix
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match FeatureQuantLayerHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:FeatureQuantLayer) =
                FeatureQuantLayerHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type MS2RatioQuantLayerHandler =
            ///Initializes a ms2ratioQuantLayer-object with at least all necessary parameters.
            static member init
                (             
                    fkdataType                   : string,
                    columnIndex                  : string,
                    fkDataMatrix                 : string,
                    ?id                          : string,
                    ?dataType                    : CVParam,
                    ?dataMatrix                  : DataMatrix,
                    ?fkFeatureList               : string
                    
                ) =
                let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                let datatype'       = defaultArg dataType Unchecked.defaultof<CVParam>
                let dataMatrix'     = defaultArg dataMatrix Unchecked.defaultof<DataMatrix>
                let fkFeatureList'  = defaultArg fkFeatureList Unchecked.defaultof<string>

                new MS2RatioQuantLayer(
                                       id',
                                       datatype',
                                       fkdataType,
                                       columnIndex,
                                       dataMatrix',
                                       fkDataMatrix,
                                       fkFeatureList',
                                       Nullable(DateTime.Now)
                                      )
            
            ///Replaces datatype of existing object with new one.
            static member addDataType
                (datatype:CVParam) (table:MS2RatioQuantLayer) =
                table.DataType <- datatype
                table

            ///Replaces dataMatrix of existing object with new one.
            static member addDataMatrix
                (dataMatrix:DataMatrix) (table:MS2RatioQuantLayer) =
                table.DataMatrix <- dataMatrix
                table

            ///Replaces fkFeatureList of existing object with new one.
            static member addFKFeatureList
                (fkFeatureList:string) (table:MS2RatioQuantLayer) =
                table.FKFeatureList <- fkFeatureList
                table

            ///Tries to find a ms2RatioQuantLayer-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.MS2RatioQuantLayer.Local do
                           if i.ID=id
                              then select (i, i.FKDataType, i.FKDataMatrix)
                      }
                |> Seq.map (fun (ms2RatioQuantLayer, _, _) -> ms2RatioQuantLayer)
                |> (fun ms2RatioQuantLayer -> 
                    if (Seq.exists (fun ms2RatioQuantLayer' -> ms2RatioQuantLayer' <> null) ms2RatioQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.MS2RatioQuantLayer do
                                       if i.ID=id
                                          then select (i, i.FKDataType, i.FKDataMatrix)
                                  }
                            |> Seq.map (fun (ms2RatioQuantLayer, _, _) -> ms2RatioQuantLayer)
                            |> (fun ms2RatioQuantLayer -> if (Seq.exists (fun ms2RatioQuantLayer' -> ms2RatioQuantLayer' <> null) ms2RatioQuantLayer) = false
                                                            then None
                                                            else Some (ms2RatioQuantLayer.Single())
                               )
                        else Some (ms2RatioQuantLayer.Single())
                   )

            ///Tries to find a ms2RatioQuantLayer-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
                query {
                       for i in dbContext.MS2RatioQuantLayer.Local do
                           if i.ColumnIndex=columnIndex
                              then select (i, i.FKDataType, i.FKDataMatrix)
                      }
                |> Seq.map (fun (ms2RatioQuantLayer, _, _) -> ms2RatioQuantLayer)
                |> (fun ms2RatioQuantLayer -> 
                    if (Seq.exists (fun ms2RatioQuantLayer' -> ms2RatioQuantLayer' <> null) ms2RatioQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.MS2RatioQuantLayer do
                                       if i.ColumnIndex=columnIndex
                                          then select (i, i.FKDataType, i.FKDataMatrix)
                                  }
                            |> Seq.map (fun (ms2RatioQuantLayer, _, _) -> ms2RatioQuantLayer)
                            |> (fun ms2RatioQuantLayer -> if (Seq.exists (fun ms2RatioQuantLayer' -> ms2RatioQuantLayer' <> null) ms2RatioQuantLayer) = false
                                                            then None
                                                            else Some ms2RatioQuantLayer
                               )
                        else Some ms2RatioQuantLayer
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:MS2RatioQuantLayer) (item2:MS2RatioQuantLayer) =
                item1.FKDataType=item2.FKDataType && item1.FKDataMatrix=item2.FKDataMatrix &&
                item1.FKFeatureList=item2.FKFeatureList

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:MS2RatioQuantLayer) =
                    MS2RatioQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match MS2RatioQuantLayerHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:MS2RatioQuantLayer) =
                MS2RatioQuantLayerHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type MS2StudyVariableQuantLayerHandler =
            ///Initializes a ms2StudyVariableQuantLayer-object with at least all necessary parameters.
            static member init
                (             
                    fkdataType                   : string,
                    columnIndex                  : string,
                    fkDataMatrix                 : string,
                    ?id                          : string,
                    ?dataType                    : CVParam,
                    ?dataMatrix                  : DataMatrix,
                    ?fkFeatureList               : string
                    
                ) =
                let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                let datatype'       = defaultArg dataType Unchecked.defaultof<CVParam>
                let dataMatrix'     = defaultArg dataMatrix Unchecked.defaultof<DataMatrix>
                let fkFeatureList'  = defaultArg fkFeatureList Unchecked.defaultof<string>
                        

                new MS2StudyVariableQuantLayer(
                                               id',
                                               datatype',
                                               fkdataType,
                                               columnIndex,
                                               dataMatrix',
                                               fkDataMatrix,
                                               fkFeatureList',
                                               Nullable(DateTime.Now)
                                              )

            ///Tries to find a ms2StudyVariableQuantLayer-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.MS2StudyVariableQuantLayer.Local do
                           if i.ID=id
                              then select (i, i.FKDataType, i.FKDataMatrix)
                      }
                |> Seq.map (fun (ms2RatioQuantLayer, _, _) -> ms2RatioQuantLayer)
                |> (fun ms2RatioQuantLayer -> 
                    if (Seq.exists (fun ms2RatioQuantLayer' -> ms2RatioQuantLayer' <> null) ms2RatioQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.MS2StudyVariableQuantLayer do
                                       if i.ID=id
                                          then select (i, i.FKDataType, i.FKDataMatrix)
                                  }
                            |> Seq.map (fun (ms2RatioQuantLayer, _, _) -> ms2RatioQuantLayer)
                            |> (fun ms2RatioQuantLayer -> if (Seq.exists (fun ms2RatioQuantLayer' -> ms2RatioQuantLayer' <> null) ms2RatioQuantLayer) = false
                                                            then None
                                                            else Some (ms2RatioQuantLayer.Single())
                               )
                        else Some (ms2RatioQuantLayer.Single())
                   )

            ///Tries to find a ms2StudyVariableQuantLayer-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
                query {
                       for i in dbContext.MS2StudyVariableQuantLayer.Local do
                           if i.ColumnIndex=columnIndex
                              then select (i, i.FKDataType, i.FKDataMatrix)
                      }
                |> Seq.map (fun (ms2RatioQuantLayer, _, _) -> ms2RatioQuantLayer)
                |> (fun ms2StudyVariableQuantLayer -> 
                    if (Seq.exists (fun ms2StudyVariableQuantLayer' -> ms2StudyVariableQuantLayer' <> null) ms2StudyVariableQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.MS2StudyVariableQuantLayer do
                                       if i.ColumnIndex=columnIndex
                                          then select (i, i.FKDataType, i.FKDataMatrix)
                                  }
                            |> Seq.map (fun (ms2StudyVariableQuantLayer, _, _) -> ms2StudyVariableQuantLayer)
                            |> (fun ms2StudyVariableQuantLayer -> if (Seq.exists (fun ms2StudyVariableQuantLayer' -> ms2StudyVariableQuantLayer' <> null) ms2StudyVariableQuantLayer) = false
                                                                    then None
                                                                    else Some ms2StudyVariableQuantLayer
                               )
                        else Some ms2StudyVariableQuantLayer
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:MS2StudyVariableQuantLayer) (item2:MS2StudyVariableQuantLayer) =
                item1.FKDataType=item2.FKDataType && item1.FKDataMatrix=item2.FKDataMatrix &&
                item1.FKFeatureList=item2.FKFeatureList

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:MS2StudyVariableQuantLayer) =
                    MS2StudyVariableQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match MS2StudyVariableQuantLayerHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:MS2StudyVariableQuantLayer) =
                MS2StudyVariableQuantLayerHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type FeatureListHandler =
            ///Initializes a featureList-object with at least all necessary parameters.
            static member init
                (             
                    fkRawFilesGroup              : string,
                    features                     : seq<Feature>,
                    ?id                          : string,
                    ?rawFilesGroup               : RawFilesGroup,
                    ?featureQuantLayers          : seq<FeatureQuantLayer>,
                    ?ms2AssayQuantLayers         : seq<MS2AssayQuantLayer>,
                    ?ms2StudyVariableQuantLayer  : seq<MS2StudyVariableQuantLayer>,
                    ?ms2RatioQuantLayer          : seq<MS2RatioQuantLayer>,
                    ?details                     : seq<FeatureListParam>,
                    ?fkMzQuantMLDocument         : string
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let rawFilesGroup'               = defaultArg rawFilesGroup Unchecked.defaultof<RawFilesGroup>
                let featureQuantLayers'          = convertOptionToList featureQuantLayers
                let ms2AssayQuantLayers'         = convertOptionToList ms2AssayQuantLayers
                let ms2StudyVariableQuantLayer'  = convertOptionToList ms2StudyVariableQuantLayer
                let ms2RatioQuantLayer'          = convertOptionToList ms2RatioQuantLayer
                let details'                     = convertOptionToList details
                let fkMzQuantMLDocument'         = defaultArg fkMzQuantMLDocument Unchecked.defaultof<string>

                new FeatureList(
                                id', 
                                rawFilesGroup',
                                fkRawFilesGroup,
                                features |> List,
                                featureQuantLayers', 
                                ms2AssayQuantLayers',
                                ms2StudyVariableQuantLayer',
                                ms2RatioQuantLayer',
                                details',
                                fkMzQuantMLDocument',
                                Nullable(DateTime.Now)
                               )
            
            ///Adds a rawFilesGroup to an existing object.
            static member addRawFilesGroup (rawFilesGroup:RawFilesGroup) (table:FeatureList) =
                table.RawFilesGroup <- rawFilesGroup
                table
            
            ///Adds a featureQuantLayer to an existing object.
            static member addModification (featureQuantLayer:FeatureQuantLayer) (table:FeatureList) =
                let result = table.FeatureQuantLayers <- addToList table.FeatureQuantLayers featureQuantLayer
                table

            ///Adds a collection of featureQuantLayers to an existing object.
            static member addModifications (featureQuantLayers:seq<FeatureQuantLayer>) (table:FeatureList) =
                let result = table.FeatureQuantLayers <- addCollectionToList table.FeatureQuantLayers featureQuantLayers
                table

            ///Adds a ms2AssayQuantLayer to an existing object.
            static member addMS2AssayQuantLayer (ms2AssayQuantLayer:MS2AssayQuantLayer) (table:FeatureList) =
                let result = table.MS2AssayQuantLayers <- addToList table.MS2AssayQuantLayers ms2AssayQuantLayer
                table

            ///Adds a collection of ms2AssayQuantLayers to an existing object.
            static member addMS2AssayQuantLayers (ms2AssayQuantLayers:seq<MS2AssayQuantLayer>) (table:FeatureList) =
                let result = table.MS2AssayQuantLayers <- addCollectionToList table.MS2AssayQuantLayers ms2AssayQuantLayers
                table

            ///Adds a ms2StudyVariableQuantLayer to an existing object.
            static member addMS2StudyVariableQuantLayer (ms2StudyVariableQuantLayer:MS2StudyVariableQuantLayer) (table:FeatureList) =
                let result = table.MS2StudyVariableQuantLayers <- addToList table.MS2StudyVariableQuantLayers ms2StudyVariableQuantLayer
                table

            ///Adds a collection of ms2StudyVariableQuantLayers to an existing object.
            static member addMS2StudyVariableQuantLayers (ms2StudyVariableQuantLayers:seq<MS2StudyVariableQuantLayer>) (table:FeatureList) =
                let result = table.MS2StudyVariableQuantLayers <- addCollectionToList table.MS2StudyVariableQuantLayers ms2StudyVariableQuantLayers
                table

            ///Adds a ms2RatioQuantLayer to an existing object.
            static member addMS2RatioQuantLayer (ms2RatioQuantLayer:MS2RatioQuantLayer) (table:FeatureList) =
                let result = table.MS2RatioQuantLayers <- addToList table.MS2RatioQuantLayers ms2RatioQuantLayer
                table

            ///Adds a collection of ms2RatioQuantLayers to an existing object.
            static member addMS2RatioQuantLayers (ms2RatioQuantLayers:seq<MS2RatioQuantLayer>) (table:FeatureList) =
                let result = table.MS2RatioQuantLayers <- addCollectionToList table.MS2RatioQuantLayers ms2RatioQuantLayers
                table

            ///Adds a featureListParam to an existing object.
            static member addDetail (featureListParam:FeatureListParam) (table:FeatureList) =
                let result = table.Details <- addToList table.Details featureListParam
                table

            ///Adds a collection of featureListParams to an existing object.
            static member addDetails (featureListParams:seq<FeatureListParam>) (table:FeatureList) =
                let result = table.Details <- addCollectionToList table.Details featureListParams
                table

            ///Replaces fkMzQuantMLDocument of existing object with new one.
            static member addFkMzQuantMLDocument
                (fkMzQuantMLDocument:string) (table:FeatureList) =
                let result = table.FKMzQuantMLDocument <- fkMzQuantMLDocument
                table

            ///Tries to find a featureList-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.FeatureList.Local do
                           if i.ID=id
                              then select (i, i.FeatureQuantLayers, i.MS2AssayQuantLayers, i.MS2StudyVariableQuantLayers, 
                                           i.MS2RatioQuantLayers, i.RawFilesGroup, i.Features, i.Details
                                          )
                      }
                |> Seq.map (fun (featureList, _, _, _, _, _, _, _) -> featureList)
                |> (fun featureList -> 
                    if (Seq.exists (fun featureList' -> featureList' <> null) featureList) = false
                        then 
                            query {
                                   for i in dbContext.FeatureList do
                                       if i.ID=id
                                          then select (i, i.FeatureQuantLayers, i.MS2AssayQuantLayers, i.MS2StudyVariableQuantLayers, 
                                                       i.MS2RatioQuantLayers, i.RawFilesGroup, i.Features, i.Details
                                                      )
                                  }
                            |> Seq.map (fun (featureList, _, _, _, _, _, _, _) -> featureList)
                            |> (fun featureList -> if (Seq.exists (fun featureList' -> featureList' <> null) featureList) = false
                                                    then None
                                                    else Some (featureList.Single())
                               )
                        else Some (featureList.Single())
                   )

            ///Tries to find a featureList-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByRawFilesGroup (dbContext:MzQuantML) (fkRawFilesGroup:string) =
                query {
                       for i in dbContext.FeatureList.Local do
                           if i.FKRawFilesGroup=fkRawFilesGroup
                              then select (i, i.FeatureQuantLayers, i.MS2AssayQuantLayers, i.MS2StudyVariableQuantLayers, 
                                           i.MS2RatioQuantLayers, i.RawFilesGroup, i.Features, i.Details
                                          )
                      }
                |> Seq.map (fun (featureList, _, _, _, _, _, _, _) -> featureList)
                |> (fun featureList -> 
                    if (Seq.exists (fun featureList' -> featureList' <> null) featureList) = false
                        then 
                            query {
                                   for i in dbContext.FeatureList do
                                       if i.FKRawFilesGroup=fkRawFilesGroup
                                          then select (i, i.FeatureQuantLayers, i.MS2AssayQuantLayers, i.MS2StudyVariableQuantLayers, 
                                                       i.MS2RatioQuantLayers, i.RawFilesGroup, i.Features, i.Details
                                                      )
                                  }
                            |> Seq.map (fun (featureList, _, _, _, _, _, _, _) -> featureList)
                            |> (fun featureList -> if (Seq.exists (fun featureList' -> featureList' <> null) featureList) = false
                                                            then None
                                                            else Some featureList
                               )
                        else Some featureList
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:FeatureList) (item2:FeatureList) =
                item1.MS2AssayQuantLayers=item2.MS2AssayQuantLayers && item1.MS2StudyVariableQuantLayers=item2.MS2StudyVariableQuantLayers && 
                item1.MS2RatioQuantLayers=item2.MS2RatioQuantLayers && item1.Features=item2.Features &&
                item1.FeatureQuantLayers=item2.FeatureQuantLayers && 
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:FeatureList) =
                    FeatureListHandler.tryFindByRawFilesGroup dbContext item.FKRawFilesGroup
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match FeatureListHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:FeatureList) =
                FeatureListHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type EvidenceRefHandler =
            ///Initializes a evidenceRef-object with at least all necessary parameters.
            static member init
                (             
                    fkFeature                    : string,
                    ?id                          : string,
                    ?assays                      : seq<Assay>,
                    ?feature                     : Feature,
                    ?fkExternalFileRef           : string,
                    ?identificationFile          : IdentificationFile,
                    ?fkIdentificationFile        : string,
                    ?fkPeptideConsensus          : string
                    
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let assays'                      = convertOptionToList assays
                let feature'                     = defaultArg feature Unchecked.defaultof<Feature>
                let fkExternalFileRef'           = defaultArg fkExternalFileRef Unchecked.defaultof<string>
                let identificationFile'          = defaultArg identificationFile Unchecked.defaultof<IdentificationFile>
                let fkIdentificationFile'        = defaultArg fkIdentificationFile Unchecked.defaultof<string>
                let fkPeptideConsensus'          = defaultArg fkPeptideConsensus Unchecked.defaultof<string>

                new EvidenceRef(
                                id', 
                                assays',
                                feature',
                                fkFeature,
                                fkExternalFileRef',
                                identificationFile',
                                fkIdentificationFile',
                                fkPeptideConsensus',
                                Nullable(DateTime.Now)
                               )

            ///Adds a assay to an existing object.
            static member addAssay (assay:Assay) (table:EvidenceRef) =
                let result = table.Assays <- addToList table.Assays assay
                table

            ///Adds a collection of assays to an existing object.
            static member addAssays (assays:seq<Assay>) (table:EvidenceRef) =
                let result = table.Assays <- addCollectionToList table.Assays assays
                table

            ///Replaces feature of existing object with new one.
            static member addFeature
                (feature:Feature) (table:EvidenceRef) =
                table.Feature <- feature
                table

            ///Replaces fkExternalFileRef of existing object with new one.
            static member addFKExternalFileRef
                (fkExternalFileRef:string) (table:EvidenceRef) =
                table.FKExternalFileRef <- fkExternalFileRef
                table

            ///Replaces identificationFile of existing object with new one.
            static member addIdentificationFile
                (identificationFile:IdentificationFile) (table:EvidenceRef) =
                table.IdentificationFile <- identificationFile
                table

            ///Replaces fkIdentificationFile of existing object with new one.
            static member addFKIdentificationFile
                (fkIdentificationFile:string) (table:EvidenceRef) =
                table.FKIdentificationFile <- fkIdentificationFile
                table

            ///Replaces fkPeptideConsensus of existing object with new one.
            static member addFKPeptideConsensus
                (fkPeptideConsensus:string) (table:EvidenceRef) =
                table.FKPeptideConsensus <- fkPeptideConsensus
                table

            ///Tries to find a evidenceRef-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.EvidenceRef.Local do
                           if i.ID=id
                              then select (i, i.Feature, i.Assays, i.IdentificationFile)
                      }
                |> Seq.map (fun (evidenceRef, _, _, _) -> evidenceRef)
                |> (fun evidenceRef -> 
                    if (Seq.exists (fun evidenceRef' -> evidenceRef' <> null) evidenceRef) = false
                        then 
                            query {
                                   for i in dbContext.EvidenceRef do
                                       if i.ID=id
                                          then select (i, i.Feature, i.Assays, i.IdentificationFile)
                                  }
                            |> Seq.map (fun (evidenceRef, _, _, _) -> evidenceRef)
                            |> (fun evidenceRef -> if (Seq.exists (fun evidenceRef' -> evidenceRef' <> null) evidenceRef) = false
                                                    then None
                                                    else Some (evidenceRef.Single())
                               )
                        else Some (evidenceRef.Single())
                   )

            ///Tries to find a evidenceRef-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFKFeature (dbContext:MzQuantML) (fkFeature:string) =
                query {
                       for i in dbContext.EvidenceRef.Local do
                           if i.FKFeature=fkFeature
                              then select (i, i.Feature, i.Assays, i.IdentificationFile)
                      }
                |> Seq.map (fun (evidenceRef, _, _, _) -> evidenceRef)
                |> (fun evidenceRef -> 
                    if (Seq.exists (fun evidenceRef' -> evidenceRef' <> null) evidenceRef) = false
                        then 
                            query {
                                   for i in dbContext.EvidenceRef do
                                       if i.FKFeature=fkFeature
                                          then select (i, i.Feature, i.Assays, i.IdentificationFile)
                                  }
                            |> Seq.map (fun (evidenceRef, _, _, _) -> evidenceRef)
                            |> (fun evidenceRef -> if (Seq.exists (fun evidenceRef' -> evidenceRef' <> null) evidenceRef) = false
                                                            then None
                                                            else Some evidenceRef
                               )
                        else Some evidenceRef
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:EvidenceRef) (item2:EvidenceRef) =
                item1.Assays=item2.Assays && item1.FKIdentificationFile=item2.FKIdentificationFile &&
                item1.FKExternalFileRef=item2.FKExternalFileRef

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:EvidenceRef) =
                    EvidenceRefHandler.tryFindByFKFeature dbContext item.FKFeature
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match EvidenceRefHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:EvidenceRef) =
                EvidenceRefHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type PeptideConsensusHandler =
            ///Initializes a peptideConsensus-object with at least all necessary parameters.
            static member init
                (             
                    charge                       : int,
                    fkPeptideConsensusList       : string,
                    ?id                          : string,
                    ?searchDatabase              : SearchDatabase,
                    ?fkSearchDatabase            : string,
                    ?dataMatrix                  : DataMatrix,
                    ?fkDataMatrix                : string,
                    ?peptideSequence             : string,
                    ?modifications               : seq<Modification>,
                    ?evidenceRefs                : seq<EvidenceRef>,
                    ?fkProtein                   : string,
                    ?details                     : seq<PeptideConsensusParam>
                    
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let searchDatabase'              = defaultArg searchDatabase Unchecked.defaultof<SearchDatabase>
                let fkSearchDatabase'            = defaultArg fkSearchDatabase Unchecked.defaultof<string>
                let dataMatrix'                  = defaultArg dataMatrix Unchecked.defaultof<DataMatrix>
                let fkDataMatrix'                = defaultArg fkDataMatrix Unchecked.defaultof<string>
                let peptideSequence'             = defaultArg peptideSequence Unchecked.defaultof<string>
                let modifications'               = convertOptionToList modifications
                let evidenceRefs'                = convertOptionToList evidenceRefs
                let fkProtein'                   = defaultArg fkProtein Unchecked.defaultof<string>
                let details'                     = convertOptionToList details                

                new PeptideConsensus(
                                     id',
                                     Nullable(charge),
                                     searchDatabase',
                                     fkSearchDatabase',
                                     dataMatrix',
                                     fkDataMatrix',
                                     peptideSequence',
                                     modifications',
                                     evidenceRefs',
                                     fkProtein',
                                     fkPeptideConsensusList,
                                     details',
                                     Nullable(DateTime.Now)
                                    )

            ///Adds a evidenceRef to an existing object.
            static member addEvidenceRef (evidenceRef:EvidenceRef) (table:PeptideConsensus) =
                let result = table.EvidenceRefs <- addToList table.EvidenceRefs evidenceRef
                table

            ///Adds a collection of evidenceRefs to an existing object.
            static member addEvidenceRefs (evidenceRefs:seq<EvidenceRef>) (table:PeptideConsensus) =
                let result = table.EvidenceRefs <- addCollectionToList table.EvidenceRefs evidenceRefs
                table

            ///Replaces searchDatabase of existing object with new one.
            static member addSearchDatabase
                (searchDatabase:SearchDatabase) (table:PeptideConsensus) =
                table.SearchDatabase <- searchDatabase
                table

            ///Replaces fkSearchDatabase of existing object with new one.
            static member addFKSearchDatabase
                (fkSearchDatabase:string) (table:PeptideConsensus) =
                table.FKSearchDatabase <- fkSearchDatabase
                table

            ///Replaces dataMatrix of existing object with new one.
            static member addDataMatrix
                (dataMatrix:DataMatrix) (table:PeptideConsensus) =
                table.DataMatrix <- dataMatrix
                table

            ///Replaces fkDataMatrix of existing object with new one.
            static member addFKDataMatrix
                (fkDataMatrix:string) (table:PeptideConsensus) =
                table.FKDataMatrix <- fkDataMatrix
                table

            ///Replaces peptideSequence of existing object with new one.
            static member addPeptideSequence
                (peptideSequence:string) (table:PeptideConsensus) =
                table.PeptideSequence <- peptideSequence
                table

            ///Adds a modification to an existing object.
            static member addModification (modification:Modification) (table:PeptideConsensus) =
                let result = table.Modifications <- addToList table.Modifications modification
                table

            ///Adds a collection of modifications to an existing object.
            static member addModifications (modifications:seq<Modification>) (table:PeptideConsensus) =
                let result = table.Modifications <- addCollectionToList table.Modifications modifications
                table

            ///Replaces fkProtein of existing object with new one.
            static member addFKProtein
                (fkProtein:string) (table:PeptideConsensus) =
                table.FKProtein <- fkProtein
                table

            /////Replaces fkPeptideConsensusList of existing object with new one.
            //static member addFKPeptideConsensusList
            //    (fkPeptideConsensusList:string) (table:PeptideConsensus) =
            //    table.FKPeptideConsensusList <- fkPeptideConsensusList
            //    table

            ///Adds a peptideConsensusParam to an existing object.
            static member addDetail (detail:PeptideConsensusParam) (table:PeptideConsensus) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of peptideConsensusParams to an existing object.
            static member addDetails (details:seq<PeptideConsensusParam>) (table:PeptideConsensus) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a peptideConsensus-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.PeptideConsensus.Local do
                           if i.ID=id
                              then select (i, i.EvidenceRefs, i.SearchDatabase, i.DataMatrix, i.Modifications, i.Details)
                      }
                |> Seq.map (fun (peptideConsensus, _, _, _, _, _) -> peptideConsensus)
                |> (fun peptideConsensus -> 
                    if (Seq.exists (fun peptideConsensus' -> peptideConsensus' <> null) peptideConsensus) = false
                        then 
                            query {
                                   for i in dbContext.PeptideConsensus do
                                       if i.ID=id
                                          then select (i, i.EvidenceRefs, i.SearchDatabase, i.DataMatrix, i.Modifications, i.Details)
                                  }
                            |> Seq.map (fun (peptideConsensus, _, _, _, _, _) -> peptideConsensus)
                            |> (fun peptideConsensus -> if (Seq.exists (fun peptideConsensus' -> peptideConsensus' <> null) peptideConsensus) = false
                                                        then None
                                                        else Some (peptideConsensus.Single())
                               )
                        else Some (peptideConsensus.Single())
                   )

            ///Tries to find a peptideConsensus-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByCharge (dbContext:MzQuantML) (charge:Nullable<int>) =
                query {
                       for i in dbContext.PeptideConsensus.Local do
                           if i.Charge=charge
                              then select (i, i.EvidenceRefs, i.SearchDatabase, i.DataMatrix, i.Modifications, i.Details)
                      }
                |> Seq.map (fun (peptideConsensus, _, _, _, _, _) -> peptideConsensus)
                |> (fun peptideConsensus -> 
                    if (Seq.exists (fun peptideConsensus' -> peptideConsensus' <> null) peptideConsensus) = false
                        then 
                            query {
                                   for i in dbContext.PeptideConsensus do
                                       if i.Charge=charge
                                          then select (i, i.EvidenceRefs, i.SearchDatabase, i.DataMatrix, i.Modifications, i.Details)
                                  }
                            |> Seq.map (fun (peptideConsensus, _, _, _, _, _) -> peptideConsensus)
                            |> (fun peptideConsensus -> if (Seq.exists (fun peptideConsensus' -> peptideConsensus' <> null) peptideConsensus) = false
                                                            then None
                                                            else Some peptideConsensus
                               )
                        else Some peptideConsensus
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:PeptideConsensus) (item2:PeptideConsensus) =
                item1.EvidenceRefs=item2.EvidenceRefs && item1.FKSearchDatabase=item2.FKSearchDatabase &&
                item1.FKDataMatrix=item2.FKDataMatrix && item1.Modifications=item2.Modifications &&
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:PeptideConsensus) =
                    PeptideConsensusHandler.tryFindByCharge dbContext item.Charge
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PeptideConsensusHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:PeptideConsensus) =
                PeptideConsensusHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinHandler =
            ///Initializes a protein-object with at least all necessary parameters.
            static member init
                (             
                    accession                    : string,
                    fkSearchDatabase             : string,
                    fkProteinList                : string,
                    ?id                          : string,
                    ?searchDatabase              : SearchDatabase,
                    ?identificationRefs          : seq<IdentificationRef>,
                    ?peptideConsensi             : seq<PeptideConsensus>,
                    ?details                     : seq<ProteinParam>
                    
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let searchDatabase'              = defaultArg searchDatabase Unchecked.defaultof<SearchDatabase>
                let identificationRefs'          = convertOptionToList identificationRefs
                let peptideConsensi'             = convertOptionToList peptideConsensi
                let details'                     = convertOptionToList details
                

                new Protein(
                            id',
                            accession,
                            searchDatabase',
                            fkSearchDatabase,
                            identificationRefs',
                            peptideConsensi',
                            fkProteinList,
                            details',
                            Nullable(DateTime.Now)
                           )

            ///Replaces searchDatabase of existing object with new one.
            static member addSearchDatabase
                (searchDatabase:SearchDatabase) (table:Protein) =
                table.SearchDatabase <- searchDatabase
                table

            ///Adds a identificationRef to an existing object.
            static member addIdentificationRef (identificationRef:IdentificationRef) (table:Protein) =
                let result = table.IdentificationRefs <- addToList table.IdentificationRefs identificationRef
                table

            ///Adds a collection of identificationRefs to an existing object.
            static member addIdentificationRefs (identificationRefs:seq<IdentificationRef>) (table:Protein) =
                let result = table.IdentificationRefs <- addCollectionToList table.IdentificationRefs identificationRefs
                table

            ///Adds a peptideConsensus to an existing object.
            static member addPeptideConsensus (peptideConsensus:PeptideConsensus) (table:Protein) =
                let result = table.PeptideConsensi <- addToList table.PeptideConsensi peptideConsensus
                table

            ///Adds a collection of peptideConsensi to an existing object.
            static member addPeptideConsensi (peptideConsensi:seq<PeptideConsensus>) (table:Protein) =
                let result = table.PeptideConsensi <- addCollectionToList table.PeptideConsensi peptideConsensi
                table

            /////Replaces fkProteinList of existing object with new one.
            //static member addFKProteinList
            //    (fkProteinList:string) (table:Protein) =
            //    table.FKProteinList <- fkProteinList
            //    table

            ///Adds a proteinParam to an existing object.
            static member addDetail (detail:ProteinParam) (table:Protein) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of proteinParams to an existing object.
            static member addDetails (details:seq<ProteinParam>) (table:Protein) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a protein-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.Protein.Local do
                           if i.ID=id
                              then select (i, i.IdentificationRefs, i.SearchDatabase, i.PeptideConsensi, i.Details)
                      }
                |> Seq.map (fun (protein, _, _, _, _) -> protein)
                |> (fun protein -> 
                    if (Seq.exists (fun protein' -> protein' <> null) protein) = false
                        then 
                            query {
                                   for i in dbContext.Protein do
                                       if i.ID=id
                                          then select (i, i.IdentificationRefs, i.SearchDatabase, i.PeptideConsensi, i.Details)
                                  }
                            |> Seq.map (fun (protein, _, _, _, _) -> protein)
                            |> (fun protein -> if (Seq.exists (fun protein' -> protein' <> null) protein) = false
                                                then None
                                                else Some (protein.Single())
                               )
                        else Some (protein.Single())
                   )

            ///Tries to find a protein-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByAccession (dbContext:MzQuantML) (accession:string) =
                query {
                       for i in dbContext.Protein.Local do
                           if i.Accession=accession
                              then select (i, i.IdentificationRefs, i.SearchDatabase, i.PeptideConsensi, i.Details)
                      }
                |> Seq.map (fun (protein, _, _, _, _) -> protein)
                |> (fun protein -> 
                    if (Seq.exists (fun protein' -> protein' <> null) protein) = false
                        then 
                            query {
                                   for i in dbContext.Protein do
                                       if i.Accession=accession
                                          then select (i, i.IdentificationRefs, i.SearchDatabase, i.PeptideConsensi, i.Details)
                                  }
                            |> Seq.map (fun (protein, _, _, _, _) -> protein)
                            |> (fun protein -> if (Seq.exists (fun protein' -> protein' <> null) protein) = false
                                                            then None
                                                            else Some protein
                               )
                        else Some protein
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Protein) (item2:Protein) =
                item1.IdentificationRefs=item2.IdentificationRefs && 
                item1.FKSearchDatabase=item2.FKSearchDatabase && item1.FKProteinList=item2.FKProteinList &&
                item1.PeptideConsensi=item2.PeptideConsensi && 
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Protein) =
                    ProteinHandler.tryFindByAccession dbContext item.Accession
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Protein) =
                ProteinHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinListHandler =
            ///Initializes a proteinList-object with at least all necessary parameters.
            static member init
                (             
                    ?id                          : string,
                    ?proteins                    : seq<Protein>,
                    ?globalQuantLayer            : seq<GlobalQuantLayer>,
                    ?assayQuantLayer             : seq<AssayQuantLayer>,
                    ?studyVariableQuantLayer     : seq<StudyVariableQuantLayer>,
                    ?ratioQuantLayer             : RatioQuantLayer,
                    ?details                     : seq<ProteinListParam>

                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let proteins'                       = convertOptionToList proteins
                let globalQuantLayer'               = convertOptionToList globalQuantLayer
                let assayQuantLayer'                = convertOptionToList assayQuantLayer
                let studyVariableQuantLayer'        = convertOptionToList studyVariableQuantLayer
                let ratioQuantLayer'                = defaultArg ratioQuantLayer Unchecked.defaultof<RatioQuantLayer>
                let details'                        = convertOptionToList details

                new ProteinList(
                                id',
                                proteins',
                                globalQuantLayer',
                                assayQuantLayer',
                                studyVariableQuantLayer',
                                ratioQuantLayer',
                                details',
                                Nullable(DateTime.Now)
                               )
            
            ///Adds a protein to an existing object.
            static member addProtein (protein:Protein) (table:ProteinList) =
                let result = table.Proteins <- addToList table.Proteins protein
                table

            ///Adds a collection of proteins to an existing object.
            static member addProteins (proteins:seq<Protein>) (table:ProteinList) =
                let result = table.Proteins <- addCollectionToList table.Proteins proteins
                table

            ///Adds a globalQuantLayer to an existing object.
            static member addGlobalQuantLayer (globalQuantLayer:GlobalQuantLayer) (table:ProteinList) =
                let result = table.GlobalQuantLayers <- addToList table.GlobalQuantLayers globalQuantLayer
                table

            ///Adds a collection of globalQuantLayers to an existing object.
            static member addGlobalQuantLayers (globalQuantLayers:seq<GlobalQuantLayer>) (table:ProteinList) =
                let result = table.GlobalQuantLayers <- addCollectionToList table.GlobalQuantLayers globalQuantLayers
                table

            ///Adds a assayQuantLayer to an existing object.
            static member addAssayQuantLayer (assayQuantLayer:AssayQuantLayer) (table:ProteinList) =
                let result = table.AssayQuantLayers <- addToList table.AssayQuantLayers assayQuantLayer
                table

            ///Adds a collection of assayQuantLayers to an existing object.
            static member addAssayQuantLayers (assayQuantLayers:seq<AssayQuantLayer>) (table:ProteinList) =
                let result = table.AssayQuantLayers <- addCollectionToList table.AssayQuantLayers assayQuantLayers
                table

            ///Adds a studyVariableQuantLayer to an existing object.
            static member addStudyVariableQuantLayer (studyVariableQuantLayer:StudyVariableQuantLayer) (table:ProteinList) =
                let result = table.StudyVariableQuantLayers <- addToList table.StudyVariableQuantLayers studyVariableQuantLayer
                table

            ///Adds a collection of studyVariableQuantLayers to an existing object.
            static member addStudyVariableQuantLayers (studyVariableQuantLayers:seq<StudyVariableQuantLayer>) (table:ProteinList) =
                let result = table.StudyVariableQuantLayers <- addCollectionToList table.StudyVariableQuantLayers studyVariableQuantLayers
                table

            ///Replaces ratioQuantLayer of existing object with new one.
            static member addRatioQuantLayer (ratioQuantLayer:RatioQuantLayer) (table:ProteinList) =
                table.RatioQuantLayer <- ratioQuantLayer
                table

            ///Adds a proteinListParam to an existing object.
            static member addDetail (detail:ProteinListParam) (table:ProteinList) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of proteinListParams to an existing object.
            static member addDetails (details:seq<ProteinListParam>) (table:ProteinList) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a proteinList-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.ProteinList.Local do
                           if i.ID=id
                              then select (i, i.Proteins, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                      }
                |> Seq.map (fun (proteinList, _, _, _, _, _, _) -> proteinList)
                |> (fun proteinList -> 
                    if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
                        then 
                            query {
                                   for i in dbContext.ProteinList do
                                       if i.ID=id
                                          then select (i, i.Proteins, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                                  }
                            |> Seq.map (fun (proteinList, _, _, _, _, _, _) -> proteinList)
                            |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
                                                    then None
                                                    else Some proteinList
                               )
                        else Some proteinList
                   )

            ///Tries to find a proteinList-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByProteins (dbContext:MzQuantML) (proteins:seq<Protein>) =
                query {
                       for i in dbContext.ProteinList.Local do
                           if i.Proteins=(proteins |> List)
                              then select (i, i.Proteins, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                      }
                |> Seq.map (fun (proteinList, _, _, _, _, _, _) -> proteinList)
                |> (fun proteinList -> 
                    if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
                        then 
                            query {
                                   for i in dbContext.ProteinList do
                                       if i.Proteins=(proteins |> List)
                                          then select (i, i.Proteins, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                                  }
                            |> Seq.map (fun (proteinList, _, _, _, _, _, _) -> proteinList)
                            |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
                                                            then None
                                                            else Some proteinList
                               )
                        else Some proteinList
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinList) (item2:ProteinList) =
                item1.GlobalQuantLayers=item2.GlobalQuantLayers && item1.AssayQuantLayers=item2.AssayQuantLayers && 
                item1.StudyVariableQuantLayers=item2.StudyVariableQuantLayers && item1.RatioQuantLayer=item2.RatioQuantLayer &&
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:ProteinList) =
                    ProteinListHandler.tryFindByID dbContext item.ID
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinListHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinList) =
                ProteinListHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinRefHandler =
            ///Initializes a proteinRef-object with at least all necessary parameters.
            static member init
                (             
                    fkProtein                    : string,
                    fkProteinGroup               : string,
                    ?id                          : string,
                    ?protein                     : Protein,
                    ?details                     : seq<ProteinRefParam>
                    
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let protein'                = defaultArg protein Unchecked.defaultof<Protein>
                let details'                = convertOptionToList details
                

                new ProteinRef(
                               id',
                               protein',
                               fkProtein,
                               fkProteinGroup,
                               details',
                               Nullable(DateTime.Now)
                              )  

            ///Replaces protein of existing object with new one.
            static member addProtein (protein:Protein) (table:ProteinRef) =
                table.Protein <- protein
                table

            /////Replaces fkProteinGroup of existing object with new one.
            //static member addFKProteinGroup (fkProteinGroup:string) (table:ProteinRef) =
            //    table.FKProteinGroup <- fkProteinGroup
            //    table

            ///Adds a proteinRefParam to an existing object.
            static member addDetail (detail:ProteinRefParam) (table:ProteinRef) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of proteinRefParams to an existing object.
            static member addDetails (details:seq<ProteinRefParam>) (table:ProteinRef) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a proteinRef-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.ProteinRef.Local do
                           if i.ID=id
                              then select (i, i.Protein, i.Details)
                      }
                |> Seq.map (fun (proteinRef, _, _) -> proteinRef)
                |> (fun proteinRef -> 
                    if (Seq.exists (fun proteinRef' -> proteinRef' <> null) proteinRef) = false
                        then 
                            query {
                                   for i in dbContext.ProteinRef do
                                       if i.ID=id
                                          then select (i, i.Protein, i.Details)
                                  }
                            |> Seq.map (fun (proteinRef, _, _) -> proteinRef)
                            |> (fun proteinRef -> if (Seq.exists (fun proteinRef' -> proteinRef' <> null) proteinRef) = false
                                                    then None
                                                    else Some (proteinRef.Single())
                               )
                        else Some (proteinRef.Single())
                   )

            ///Tries to find a proteinRef-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFKProtein (dbContext:MzQuantML) (fkProtein:string) =
                query {
                       for i in dbContext.ProteinRef.Local do
                           if i.FKProtein=fkProtein
                              then select (i, i.Protein, i.Details)
                      }
                |> Seq.map (fun (proteinRef, _, _) -> proteinRef)
                |> (fun proteinRef -> 
                    if (Seq.exists (fun proteinRef' -> proteinRef' <> null) proteinRef) = false
                        then 
                            query {
                                   for i in dbContext.ProteinRef do
                                       if i.FKProtein=fkProtein
                                          then select (i, i.Protein, i.Details)
                                  }
                            |> Seq.map (fun (proteinRef, _, _) -> proteinRef)
                            |> (fun proteinRef -> if (Seq.exists (fun proteinRef' -> proteinRef' <> null) proteinRef) = false
                                                            then None
                                                            else Some proteinRef
                               )
                        else Some proteinRef
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinRef) (item2:ProteinRef) =
                item1.FKProteinGroup=item2.FKProteinGroup &&
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:ProteinRef) =
                    ProteinRefHandler.tryFindByFKProtein dbContext item.FKProtein
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinRefHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinRef) =
                ProteinRefHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinGroupHandler =
            ///Initializes a proteinGroup-object with at least all necessary parameters.
            static member init
                (             
                    fkSearchDatabase             : string,
                    fkProteinGroupList           : string,
                    ?id                          : string,
                    ?searchDatabase              : SearchDatabase,
                    ?identificationRefs          : seq<IdentificationRef>,
                    ?proteinRefs                 : seq<ProteinRef>,
                    ?details                     : seq<ProteinGroupParam>
                    
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let searchDatabase'                 = defaultArg searchDatabase Unchecked.defaultof<SearchDatabase>
                let identificationRefs'             = convertOptionToList identificationRefs
                let proteinRefs'                    = convertOptionToList proteinRefs
                let details'                        = convertOptionToList details
                

                new ProteinGroup(
                                 id',
                                 searchDatabase',
                                 fkSearchDatabase,
                                 identificationRefs',
                                 proteinRefs',
                                 fkProteinGroupList,
                                 details',
                                 Nullable(DateTime.Now)
                                )  

            ///Replaces searchDatabase of existing object with new one.
            static member addSearchDatabase (searchDatabase:SearchDatabase) (table:ProteinGroup) =
                table.SearchDatabase <- searchDatabase
                table
            
            /////Replaces fkProteinGroupList of existing object with new one.
            //static member addFKProteinGroupList (fkProteinGroupList:string) (table:ProteinGroup) =
            //    table.FKProteinGroupList <- fkProteinGroupList
            //    table

            ///Adds a identificationRef to an existing object.
            static member addIdentificationRef (identificationRef:IdentificationRef) (table:ProteinGroup) =
                let result = table.IdentificationRefs <- addToList table.IdentificationRefs identificationRef
                table

            ///Adds a collection of identificationRefs to an existing object.
            static member addIdentificationRefs (identificationRefs:seq<IdentificationRef>) (table:ProteinGroup) =
                let result = table.IdentificationRefs <- addCollectionToList table.IdentificationRefs identificationRefs
                table

            ///Adds a proteinRef to an existing object.
            static member addProteinRef (proteinRef:ProteinRef) (table:ProteinGroup) =
                let result = table.ProteinRefs <- addToList table.ProteinRefs proteinRef
                table

            ///Adds a collection of proteinRefs to an existing object.
            static member addProteinRefs (proteinRefs:seq<ProteinRef>) (table:ProteinGroup) =
                let result = table.ProteinRefs <- addCollectionToList table.ProteinRefs proteinRefs
                table

            ///Adds a proteinGroupParam to an existing object.
            static member addDetail (detail:ProteinGroupParam) (table:ProteinGroup) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of proteinGroupParams to an existing object.
            static member addDetails (details:seq<ProteinGroupParam>) (table:ProteinGroup) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a proteinGroup-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.ProteinGroup.Local do
                           if i.ID=id
                              then select (i, i.ProteinRefs, i.IdentificationRefs, i.SearchDatabase, i.Details)
                      }
                |> Seq.map (fun (proteinGroup, _, _, _, _) -> proteinGroup)
                |> (fun proteinGroup -> 
                    if (Seq.exists (fun proteinGroup' -> proteinGroup' <> null) proteinGroup) = false
                        then 
                            query {
                                   for i in dbContext.ProteinGroup do
                                       if i.ID=id
                                          then select (i, i.ProteinRefs, i.IdentificationRefs, i.SearchDatabase, i.Details)
                                  }
                            |> Seq.map (fun (proteinGroup, _, _, _, _) -> proteinGroup)
                            |> (fun proteinGroup -> if (Seq.exists (fun proteinGroup' -> proteinGroup' <> null) proteinGroup) = false
                                                    then None
                                                    else Some (proteinGroup.Single())
                               )
                        else Some (proteinGroup.Single())
                   )

            ///Tries to find a proteinGroup-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFKSearchDatabase (dbContext:MzQuantML) (fkSearchDatabase:string) =
                query {
                       for i in dbContext.ProteinGroup.Local do
                           if i.FKSearchDatabase=fkSearchDatabase
                              then select (i, i.ProteinRefs, i.IdentificationRefs, i.SearchDatabase, i.Details)
                      }
                |> Seq.map (fun (proteinGroup, _, _, _, _) -> proteinGroup)
                |> (fun proteinGroup -> 
                    if (Seq.exists (fun proteinGroup' -> proteinGroup' <> null) proteinGroup) = false
                        then 
                            query {
                                   for i in dbContext.ProteinGroup do
                                       if i.FKSearchDatabase=fkSearchDatabase
                                          then select (i, i.ProteinRefs, i.IdentificationRefs, i.SearchDatabase, i.Details)
                                  }
                            |> Seq.map (fun (proteinGroup, _, _, _, _) -> proteinGroup)
                            |> (fun proteinGroup -> if (Seq.exists (fun proteinGroup' -> proteinGroup' <> null) proteinGroup) = false
                                                            then None
                                                            else Some proteinGroup
                               )
                        else Some proteinGroup
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinGroup) (item2:ProteinGroup) =
                item1.ProteinRefs=item2.ProteinRefs && item1.IdentificationRefs=item2.IdentificationRefs && 
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:ProteinGroup) =
                    ProteinGroupHandler.tryFindByFKSearchDatabase dbContext item.FKSearchDatabase
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinGroupHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinGroup) =
                ProteinGroupHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinGroupListHandler =
            ///Initializes a proteinGroupList-object with at least all necessary parameters.
            static member init
                (             
                    ?id                          : string,
                    ?proteinGroups               : seq<ProteinGroup>,
                    ?globalQuantLayer            : seq<GlobalQuantLayer>,
                    ?assayQuantLayer             : seq<AssayQuantLayer>,
                    ?studyVariableQuantLayer     : seq<StudyVariableQuantLayer>,
                    ?ratioQuantLayer             : RatioQuantLayer,
                    ?fkRatioQuantLayer           : string,
                    ?details                     : seq<ProteinGroupListParam>
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let proteinGroups'                  = convertOptionToList proteinGroups
                let globalQuantLayer'               = convertOptionToList globalQuantLayer
                let assayQuantLayer'                = convertOptionToList assayQuantLayer
                let studyVariableQuantLayer'        = convertOptionToList studyVariableQuantLayer
                let ratioQuantLayer'                = defaultArg ratioQuantLayer Unchecked.defaultof<RatioQuantLayer>
                let fkRatioQuantLayer'              = defaultArg fkRatioQuantLayer Unchecked.defaultof<string>
                let details'                        = convertOptionToList details

                new ProteinGroupList(
                                     id',
                                     proteinGroups',
                                     globalQuantLayer',
                                     assayQuantLayer',
                                     studyVariableQuantLayer',
                                     ratioQuantLayer',
                                     fkRatioQuantLayer',
                                     details',
                                     Nullable(DateTime.Now)
                                    )
            
            ///Adds a proteinGroup to an existing object.
            static member addProteinGroup (proteinGroup:ProteinGroup) (table:ProteinGroupList) =
                let result = table.ProteinGroups <- addToList table.ProteinGroups proteinGroup
                table

            ///Adds a collection of proteinGroups to an existing object.
            static member addProteinGroups (proteinGroups:seq<ProteinGroup>) (table:ProteinGroupList) =
                let result = table.ProteinGroups <- addCollectionToList table.ProteinGroups proteinGroups
                table
            
            ///Adds a globalQuantLayer to an existing object.
            static member addGlobalQuantLayer (globalQuantLayer:GlobalQuantLayer) (table:ProteinGroupList) =
                let result = table.GlobalQuantLayers <- addToList table.GlobalQuantLayers globalQuantLayer
                table

            ///Adds a collection of globalQuantLayers to an existing object.
            static member addGlobalQuantLayers (globalQuantLayers:seq<GlobalQuantLayer>) (table:ProteinGroupList) =
                let result = table.GlobalQuantLayers <- addCollectionToList table.GlobalQuantLayers globalQuantLayers
                table

            ///Adds a assayQuantLayer to an existing object.
            static member addAssayQuantLayer (assayQuantLayer:AssayQuantLayer) (table:ProteinGroupList) =
                let result = table.AssayQuantLayers <- addToList table.AssayQuantLayers assayQuantLayer
                table

            ///Adds a collection of assayQuantLayers to an existing object.
            static member addAssayQuantLayers (assayQuantLayers:seq<AssayQuantLayer>) (table:ProteinGroupList) =
                let result = table.AssayQuantLayers <- addCollectionToList table.AssayQuantLayers assayQuantLayers
                table

            ///Adds a studyVariableQuantLayer to an existing object.
            static member addStudyVariableQuantLayer (studyVariableQuantLayer:StudyVariableQuantLayer) (table:ProteinGroupList) =
                let result = table.StudyVariableQuantLayers <- addToList table.StudyVariableQuantLayers studyVariableQuantLayer
                table

            ///Adds a collection of studyVariableQuantLayers to an existing object.
            static member addStudyVariableQuantLayers (studyVariableQuantLayers:seq<StudyVariableQuantLayer>) (table:ProteinGroupList) =
                let result = table.StudyVariableQuantLayers <- addCollectionToList table.StudyVariableQuantLayers studyVariableQuantLayers
                table

            ///Replaces ratioQuantLayer of existing object with new one.
            static member addRatioQuantLayer (ratioQuantLayer:RatioQuantLayer) (table:ProteinGroupList) =
                table.RatioQuantLayer <- ratioQuantLayer
                table

            ///Replaces fkRatioQuantLayer of existing object with new one.
            static member addFKRatioQuantLayer (fkRatioQuantLayer:string) (table:ProteinGroupList) =
                table.FKRatioQuantLayer <- fkRatioQuantLayer
                table

            ///Adds a smallMoleculeParam to an existing object.
            static member addDetail (smallMoleculeParam:ProteinGroupListParam) (table:ProteinGroupList) =
                let result = table.Details <- addToList table.Details smallMoleculeParam
                table

            ///Adds a collection of smallMoleculeParams to an existing object.
            static member addDetails (smallMoleculeParams:seq<ProteinGroupListParam>) (table:ProteinGroupList) =
                let result = table.Details <- addCollectionToList table.Details smallMoleculeParams
                table

            ///Tries to find a proteinGroupList-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.ProteinGroupList.Local do
                           if i.ID=id
                              then select (i, i.ProteinGroups, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                      }
                |> Seq.map (fun (smallMoleculeList, _, _, _, _, _, _) -> smallMoleculeList)
                |> (fun smallMoleculeList -> 
                    if (Seq.exists (fun smallMoleculeList' -> smallMoleculeList' <> null) smallMoleculeList) = false
                        then 
                            query {
                                   for i in dbContext.ProteinGroupList do
                                       if i.ID=id
                                          then select (i, i.ProteinGroups, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                                  }
                            |> Seq.map (fun (smallMoleculeList, _, _, _, _, _, _) -> smallMoleculeList)
                            |> (fun smallMoleculeList -> if (Seq.exists (fun smallMoleculeList' -> smallMoleculeList' <> null) smallMoleculeList) = false
                                                            then None
                                                            else Some (smallMoleculeList.Single())
                               )
                        else Some (smallMoleculeList.Single())
                   )

            ///Tries to find a proteinGroupList-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFKRatioQuantLayer (dbContext:MzQuantML) (fkRatioQuantLayer:string) =
                query {
                       for i in dbContext.ProteinGroupList.Local do
                           if i.FKRatioQuantLayer=fkRatioQuantLayer
                              then select (i, i.ProteinGroups, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                      }
                |> Seq.map (fun (smallMoleculeList, _, _, _, _, _, _) -> smallMoleculeList)
                |> (fun smallMoleculeList -> 
                    if (Seq.exists (fun smallMoleculeList' -> smallMoleculeList' <> null) smallMoleculeList) = false
                        then 
                            query {
                                   for i in dbContext.ProteinGroupList do
                                       if i.FKRatioQuantLayer=fkRatioQuantLayer
                                          then select (i, i.ProteinGroups, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                                  }
                            |> Seq.map (fun (smallMoleculeList, _, _, _, _, _, _) -> smallMoleculeList)
                            |> (fun smallMoleculeList -> if (Seq.exists (fun smallMoleculeList' -> smallMoleculeList' <> null) smallMoleculeList) = false
                                                            then None
                                                            else Some smallMoleculeList
                               )
                        else Some smallMoleculeList
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinGroupList) (item2:ProteinGroupList) =
                item1.GlobalQuantLayers=item2.GlobalQuantLayers && item1.AssayQuantLayers=item2.AssayQuantLayers && 
                item1.StudyVariableQuantLayers=item2.StudyVariableQuantLayers &&
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:ProteinGroupList) =
                    ProteinGroupListHandler.tryFindByFKRatioQuantLayer dbContext item.FKRatioQuantLayer
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinGroupListHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinGroupList) =
                ProteinGroupListHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type PeptideConsensusListHandler =
            ///Initializes a peptideConsensusList-object with at least all necessary parameters.
            static member init
                (             
                    finalResult                  : bool,
                    ?id                          : string,
                    ?peptideConsensi             : seq<PeptideConsensus>,
                    ?globalQuantLayer            : seq<GlobalQuantLayer>,
                    ?assayQuantLayer             : seq<AssayQuantLayer>,
                    ?studyVariableQuantLayer     : seq<StudyVariableQuantLayer>,
                    ?ratioQuantLayer             : RatioQuantLayer,
                    ?fkRatioQuantLayer           : string,
                    ?fkMzQuantMLDocument         : string,
                    ?details                     : seq<PeptideConsensusListParam>
                    
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let peptideConsensi'                = convertOptionToList peptideConsensi
                let globalQuantLayer'               = convertOptionToList globalQuantLayer
                let assayQuantLayer'                = convertOptionToList assayQuantLayer
                let studyVariableQuantLayer'        = convertOptionToList studyVariableQuantLayer
                let ratioQuantLayer'                = defaultArg ratioQuantLayer Unchecked.defaultof<RatioQuantLayer>
                let fkRatioQuantLayer'              = defaultArg fkRatioQuantLayer Unchecked.defaultof<string>
                let fkMzQuantMLDocument'            = defaultArg fkMzQuantMLDocument Unchecked.defaultof<string>
                let details'                        = convertOptionToList details

                new PeptideConsensusList(
                                         id',
                                         Nullable(finalResult),
                                         peptideConsensi',
                                         globalQuantLayer',
                                         assayQuantLayer',
                                         studyVariableQuantLayer',
                                         ratioQuantLayer',
                                         fkRatioQuantLayer',
                                         details',
                                         fkMzQuantMLDocument',
                                         Nullable(DateTime.Now)
                                        )
            
            ///Adds a peptideConsensus to an existing object.
            static member addPeptideConsensus (peptideConsensus:PeptideConsensus) (table:PeptideConsensusList) =
                let result = table.PeptideConsensi <- addToList table.PeptideConsensi peptideConsensus
                table

            ///Adds a collection of peptideConsensi to an existing object.
            static member addPeptideConsensi (peptideConsensi:seq<PeptideConsensus>) (table:PeptideConsensusList) =
                let result = table.PeptideConsensi <- addCollectionToList table.PeptideConsensi peptideConsensi
                table
            
            ///Adds a globalQuantLayer to an existing object.
            static member addGlobalQuantLayer (globalQuantLayer:GlobalQuantLayer) (table:PeptideConsensusList) =
                let result = table.GlobalQuantLayers <- addToList table.GlobalQuantLayers globalQuantLayer
                table

            ///Adds a collection of globalQuantLayers to an existing object.
            static member addGlobalQuantLayers (globalQuantLayers:seq<GlobalQuantLayer>) (table:PeptideConsensusList) =
                let result = table.GlobalQuantLayers <- addCollectionToList table.GlobalQuantLayers globalQuantLayers
                table

            ///Adds a assayQuantLayer to an existing object.
            static member addAssayQuantLayer (assayQuantLayer:AssayQuantLayer) (table:PeptideConsensusList) =
                let result = table.AssayQuantLayers <- addToList table.AssayQuantLayers assayQuantLayer
                table

            ///Adds a collection of assayQuantLayers to an existing object.
            static member addAssayQuantLayers (assayQuantLayers:seq<AssayQuantLayer>) (table:PeptideConsensusList) =
                let result = table.AssayQuantLayers <- addCollectionToList table.AssayQuantLayers assayQuantLayers
                table

            ///Adds a studyVariableQuantLayer to an existing object.
            static member addStudyVariableQuantLayer (studyVariableQuantLayer:StudyVariableQuantLayer) (table:PeptideConsensusList) =
                let result = table.StudyVariableQuantLayers <- addToList table.StudyVariableQuantLayers studyVariableQuantLayer
                table

            ///Adds a collection of studyVariableQuantLayers to an existing object.
            static member addStudyVariableQuantLayers (studyVariableQuantLayers:seq<StudyVariableQuantLayer>) (table:PeptideConsensusList) =
                let result = table.StudyVariableQuantLayers <- addCollectionToList table.StudyVariableQuantLayers studyVariableQuantLayers
                table

            ///Replaces ratioQuantLayer of existing object with new one.
            static member addRatioQuantLayer (ratioQuantLayer:RatioQuantLayer) (table:PeptideConsensusList) =
                table.RatioQuantLayer <- ratioQuantLayer
                table

            ///Replaces fkRatioQuantLayer of existing object with new one.
            static member addFKRatioQuantLayer (fkRatioQuantLayer:string) (table:PeptideConsensusList) =
                table.FKRatioQuantLayer <- fkRatioQuantLayer
                table

            ///Adds a peptideConsensusListParam to an existing object.
            static member addDetail (peptideConsensusListParam:PeptideConsensusListParam) (table:PeptideConsensusList) =
                let result = table.Details <- addToList table.Details peptideConsensusListParam
                table

            ///Adds a collection of peptideConsensusListParams to an existing object.
            static member addDetails (peptideConsensusListParams:seq<PeptideConsensusListParam>) (table:PeptideConsensusList) =
                let result = table.Details <- addCollectionToList table.Details peptideConsensusListParams
                table

            ///Replaces fkMzQuantMLDocument of existing object with new one.
            static member addFkMzQuantMLDocument
                (fkMzQuantMLDocument:string) (table:PeptideConsensusList) =
                let result = table.FKMzQuantMLDocument <- fkMzQuantMLDocument
                table

            ///Tries to find a peptideConsensusList-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.PeptideConsensusList.Local do
                           if i.ID=id
                              then select (i, i.PeptideConsensi, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                      }
                |> Seq.map (fun (peptideConsensi, _, _, _, _, _, _) -> peptideConsensi)
                |> (fun peptideConsensi -> 
                    if (Seq.exists (fun peptideConsensi' -> peptideConsensi' <> null) peptideConsensi) = false
                        then 
                            query {
                                   for i in dbContext.PeptideConsensusList do
                                       if i.ID=id
                                          then select (i, i.PeptideConsensi, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                                  }
                            |> Seq.map (fun (peptideConsensi, _, _, _, _, _, _) -> peptideConsensi)
                            |> (fun peptideConsensi -> if (Seq.exists (fun peptideConsensi' -> peptideConsensi' <> null) peptideConsensi) = false
                                                        then None
                                                        else Some (peptideConsensi.Single())
                               )
                        else Some (peptideConsensi.Single())
                   )

            ///Tries to find a peptideConsensusList-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFinalResult (dbContext:MzQuantML) (finalResult:Nullable<bool>) =
                query {
                       for i in dbContext.PeptideConsensusList.Local do
                           if i.FinalResult=finalResult
                              then select (i, i.PeptideConsensi, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                      }
                |> Seq.map (fun (peptideConsensi, _, _, _, _, _, _) -> peptideConsensi)
                |> (fun peptideConsensi -> 
                    if (Seq.exists (fun peptideConsensi' -> peptideConsensi' <> null) peptideConsensi) = false
                        then 
                            query {
                                   for i in dbContext.PeptideConsensusList do
                                       if i.FinalResult=finalResult
                                          then select (i, i.PeptideConsensi, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
                                  }
                            |> Seq.map (fun (peptideConsensi, _, _, _, _, _, _) -> peptideConsensi)
                            |> (fun peptideConsensi -> if (Seq.exists (fun peptideConsensi' -> peptideConsensi' <> null) peptideConsensi) = false
                                                            then None
                                                            else Some peptideConsensi
                               )
                        else Some peptideConsensi
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:PeptideConsensusList) (item2:PeptideConsensusList) =
                item1.GlobalQuantLayers=item2.GlobalQuantLayers && item1.AssayQuantLayers=item2.AssayQuantLayers && 
                item1.StudyVariableQuantLayers=item2.StudyVariableQuantLayers && item1.RatioQuantLayer=item2.RatioQuantLayer &&
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) && 
                item1.PeptideConsensi=item2.PeptideConsensi

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:PeptideConsensusList) =
                    PeptideConsensusListHandler.tryFindByFinalResult dbContext item.FinalResult
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PeptideConsensusListHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:PeptideConsensusList) =
                PeptideConsensusListHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type BiblioGraphicReferenceHandler =
            ///Initializes a biblioGraphicReference-object with at least all necessary parameters.
            static member init
                (             
                    ?id                          : string,
                    ?name                        : string,
                    ?authors                     : string,
                    ?doi                         : string,
                    ?editor                      : string,
                    ?issue                       : string,
                    ?pages                       : string,
                    ?publication                 : string,
                    ?publisher                   : string,
                    ?title                       : string,
                    ?volume                      : string,
                    ?year                        : int,
                    ?fkMzQuantMLDocument         : string
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                           = defaultArg name Unchecked.defaultof<string>
                let authors'                        = defaultArg authors Unchecked.defaultof<string>
                let doi'                            = defaultArg doi Unchecked.defaultof<string>
                let editor'                         = defaultArg editor Unchecked.defaultof<string>
                let issue'                          = defaultArg issue Unchecked.defaultof<string>
                let pages'                          = defaultArg pages Unchecked.defaultof<string>
                let publication'                    = defaultArg publication Unchecked.defaultof<string>
                let publisher'                      = defaultArg publisher Unchecked.defaultof<string>
                let title'                          = defaultArg title Unchecked.defaultof<string>
                let volume'                         = defaultArg volume Unchecked.defaultof<string>
                let year'                           = defaultArg year Unchecked.defaultof<int>
                let fkMzQuantMLDocument'            = defaultArg fkMzQuantMLDocument Unchecked.defaultof<string>

                new BiblioGraphicReference(
                                           id',
                                           name',
                                           authors',
                                           doi',
                                           editor',
                                           issue',
                                           pages',
                                           publication',
                                           publisher',
                                           title',
                                           volume',
                                           Nullable(year'),
                                           fkMzQuantMLDocument',
                                           Nullable(DateTime.Now)
                                          ) 

            ///Replaces name of existing object with new one.
            static member addName (name:string) (table:BiblioGraphicReference) =
                table.Name <- name
                table

            ///Replaces authors of existing object with new one.
            static member addAuthors (authors:string) (table:BiblioGraphicReference) =
                table.Authors <- authors
                table

            ///Replaces doi of existing object with new one.
            static member addDOI (doi:string) (table:BiblioGraphicReference) =
                table.DOI <- doi
                table

            ///Replaces editor of existing object with new one.
            static member addEditor (editor:string) (table:BiblioGraphicReference) =
                table.Editor <- editor
                table

            ///Replaces issue of existing object with new one.
            static member addIssue (issue:string) (table:BiblioGraphicReference) =
                table.Issue <- issue
                table

            ///Replaces pages of existing object with new one.
            static member addPages (pages:string) (table:BiblioGraphicReference) =
                table.Pages <- pages
                table

            ///Replaces publication of existing object with new one.
            static member addPublication (publication:string) (table:BiblioGraphicReference) =
                table.Publication <- publication
                table

            ///Replaces publisher of existing object with new one.
            static member addPublisher (publisher:string) (table:BiblioGraphicReference) =
                table.Publisher <- publisher
                table

            ///Replaces title of existing object with new one.
            static member addTitle (title:string) (table:BiblioGraphicReference) =
                table.Title <- title
                table

            ///Replaces volume of existing object with new one.
            static member addVolume (volume:string) (table:BiblioGraphicReference) =
                table.Volume <- volume
                table

            ///Replaces year of existing object with new one.
            static member addYear (year:int) (table:BiblioGraphicReference) =
                table.Year <- Nullable(year)
                table

            ///Replaces fkMzQuantMLDocument of existing object with new one.
            static member addFkMzQuantMLDocument
                (fkMzQuantMLDocument:string) (table:BiblioGraphicReference) =
                let result = table.FKMzQuantMLDocument <- fkMzQuantMLDocument
                table

            ///Tries to find a biblioGraphicReference-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.BiblioGraphicReference.Local do
                           if i.ID=id
                              then select i
                      }
                |> (fun biblioGraphicReference -> 
                    if (Seq.exists (fun biblioGraphicReference' -> biblioGraphicReference' <> null) biblioGraphicReference) = false
                        then 
                            query {
                                   for i in dbContext.BiblioGraphicReference do
                                       if i.ID=id
                                          then select i
                                  }
                            |> Seq.map (fun (biblioGraphicReference) -> biblioGraphicReference)
                            |> (fun biblioGraphicReference -> if (Seq.exists (fun biblioGraphicReference' -> biblioGraphicReference' <> null) biblioGraphicReference) = false
                                                                then None
                                                                else Some (biblioGraphicReference.Single())
                               )
                        else Some (biblioGraphicReference.Single())
                   )

            ///Tries to find a biblioGraphicReference-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.BiblioGraphicReference.Local do
                           if i.Name=name
                              then select i
                      }
                |> Seq.map (fun (biblioGraphicReference) -> biblioGraphicReference)
                |> (fun biblioGraphicReference -> 
                    if (Seq.exists (fun biblioGraphicReference' -> biblioGraphicReference' <> null) biblioGraphicReference) = false
                        then 
                            query {
                                   for i in dbContext.BiblioGraphicReference do
                                       if i.Name=name
                                          then select i
                                  }
                            |> Seq.map (fun (biblioGraphicReference) -> biblioGraphicReference)
                            |> (fun biblioGraphicReference -> if (Seq.exists (fun biblioGraphicReference' -> biblioGraphicReference' <> null) biblioGraphicReference) = false
                                                                then None
                                                                else Some biblioGraphicReference
                               )
                        else Some biblioGraphicReference
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:BiblioGraphicReference) (item2:BiblioGraphicReference) =
                item1.Authors=item2.Authors && item1.DOI=item2.DOI && item1.Editor=item2.Editor && item1.Issue=item2.Issue &&
                item1.Pages=item2.Pages && item1.Publication=item2.Publication && item1.Publisher=item2.Publisher && 
                item1.Title=item2.Title && item1.Volume=item2.Volume && item1.Year=item2.Year

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:BiblioGraphicReference) =
                    BiblioGraphicReferenceHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match BiblioGraphicReferenceHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:BiblioGraphicReference) =
                BiblioGraphicReferenceHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type MzQuantMLDocumentHandler =
            ///Initializes a mzQuantMLDocument-object with at least all necessary parameters.
            static member init
                (             
                    
                    ?id                          : string,
                    ?name                        : string,
                    ?creationDate                : DateTime,
                    ?version                     : string,
                    ?providers                   : seq<Provider>,
                    ?organizations               : seq<Organization>,
                    ?persons                     : seq<Person>,
                    ?analysisSummaries           : seq<AnalysisSummary>,
                    ?inputFiles                  : InputFiles,
                    ?fkInputFiles                : string,
                    ?softwares                   : seq<Software>,
                    ?dataProcessings             : seq<DataProcessing>,
                    ?assays                      : seq<Assay>,
                    ?biblioGraphicReferences     : seq<BiblioGraphicReference>,
                    ?studyVariables              : seq<StudyVariable>,
                    ?ratios                      : seq<Ratio>,
                    ?proteinGroupList            : ProteinGroupList,
                    ?fkProteinGroupList          : string,
                    ?proteinList                 : ProteinList,
                    ?fkProteinList               : string,
                    ?peptideConsensusList        : seq<PeptideConsensusList>,
                    ?smallMoleculeList           : SmallMoleculeList,
                    ?fkSmallMoleculeList         : string,
                    ?featureList                 : seq<FeatureList>
                    
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                           = defaultArg name Unchecked.defaultof<string>
                let creationDate'                   = defaultArg creationDate Unchecked.defaultof<DateTime>
                let version'                        = defaultArg version Unchecked.defaultof<string>
                let providers'                      = convertOptionToList providers
                let organizations'                  = convertOptionToList organizations
                let persons'                        = convertOptionToList persons
                let analysisSummaries'              = convertOptionToList analysisSummaries
                let inputFiles'                     = defaultArg inputFiles Unchecked.defaultof<InputFiles>
                let fkInputFiles'                   = defaultArg fkInputFiles Unchecked.defaultof<string>
                let softwares'                      = convertOptionToList softwares
                let dataProcessings'                = convertOptionToList dataProcessings
                let assays'                         = convertOptionToList assays
                let biblioGraphicReferences'        = convertOptionToList biblioGraphicReferences
                let studyVariables'                 = convertOptionToList studyVariables
                let ratios'                         = convertOptionToList ratios
                let proteinGroupList'               = defaultArg proteinGroupList Unchecked.defaultof<ProteinGroupList>
                let fkProteinGroupList'             = defaultArg fkProteinGroupList Unchecked.defaultof<string>
                let proteinList'                    = defaultArg proteinList Unchecked.defaultof<ProteinList>
                let fkProteinList'                  = defaultArg fkProteinList Unchecked.defaultof<string>
                let peptideConsensusList'           = convertOptionToList peptideConsensusList
                let smallMoleculeList'              = defaultArg smallMoleculeList Unchecked.defaultof<SmallMoleculeList>
                let fkSmallMoleculeList'            = defaultArg fkSmallMoleculeList Unchecked.defaultof<string>
                let featureList'                    = convertOptionToList featureList

                new MzQuantMLDocument(id', 
                                      name', 
                                      Nullable(creationDate'), 
                                      version', 
                                      providers',
                                      organizations', 
                                      persons', 
                                      analysisSummaries', 
                                      inputFiles',
                                      fkInputFiles',
                                      softwares', 
                                      dataProcessings', 
                                      assays', 
                                      biblioGraphicReferences', 
                                      studyVariables', 
                                      ratios', 
                                      proteinGroupList',
                                      fkProteinGroupList',
                                      proteinList', 
                                      fkProteinList',
                                      peptideConsensusList', 
                                      smallMoleculeList',
                                      fkSmallMoleculeList',
                                      featureList', 
                                      Nullable(DateTime.Now)
                                     )

            ///Replaces name of existing object with new one.
            static member addName (name:string) (table:MzQuantMLDocument) =
                table.Name <- name
                table

            ///Replaces creationDate of existing object with new one.
            static member addCreationDate (creationDate:DateTime) (table:MzQuantMLDocument) =
                table.CreationDate <- Nullable(creationDate)
                table

            ///Replaces version of existing object with new one.
            static member addVersion (version:string) (table:MzQuantMLDocument) =
                table.Version <- version
                table

            ///Adds new provider to collection of enzymenames.
            static member addProvider
                (provider:Provider) (table:MzQuantMLDocument) =
                table.Providers <- addToList table.Providers provider
                table

            ///Add new collection of providers to collection of enzymenames.
            static member addProviders
                (providers:seq<Provider>) (table:MzQuantMLDocument) =
                table.Providers <- addCollectionToList table.Providers providers
                table

             ///Replaces proteinGroupList of existing object with new one.
            static member addProteinGroupList
                (proteinGroupList:ProteinGroupList) (table:MzQuantMLDocument) =
                table.ProteinGroupList <- proteinGroupList
                table 

            ///Replaces fkProteinGroupList of existing object with new one.
            static member addFKProteinGroupList
                (fkProteinGroupList:string) (table:MzQuantMLDocument) =
                table.FKProteinGroupList <- fkProteinGroupList
                table

            ///Replaces proteinList of existing object with new one.
            static member addProteinList (proteinList:ProteinList) (table:MzQuantMLDocument) =
                table.ProteinList <- proteinList
                table

            ///Replaces fkProteinList of existing object with new one.
            static member addFKProteinList
                (fkProteinList:string) (table:MzQuantMLDocument) =
                table.FKProteinList <- fkProteinList
                table

            ///Adds new person to collection of persons.
            static member addPerson
                (person:Person) (table:MzQuantMLDocument) =
                table.Persons <- addToList table.Persons person
                table

            ///Add new collection of persons to collection of persons.
            static member addPersons
                (persons:seq<Person>) (table:MzQuantMLDocument) =
                table.Persons <- addCollectionToList table.Persons persons
                table

            ///Adds new organization to collection of organizations.
            static member addOrganization
                (organization:Organization) (table:MzQuantMLDocument) =
                table.Organizations <- addToList table.Organizations organization
                table

            ///Add new collection of organizations to collection of organizations.
            static member addOrganizations
                (organizations:seq<Organization>) (table:MzQuantMLDocument) =
                table.Organizations <- addCollectionToList table.Organizations organizations
                table

            ///Adds new peptideConsensusList to collection of peptideConsensusLists.
            static member addPeptideConsensusList
                (peptideConsensusList:PeptideConsensusList) (table:MzQuantMLDocument) =
                table.PeptideConsensusList <- addToList table.PeptideConsensusList peptideConsensusList
                table

            ///Add new collection of peptideConsensusLists to collection of peptideConsensusLists.
            static member addPeptideConsensusLists
                (peptideConsensusLists:seq<PeptideConsensusList>) (table:MzQuantMLDocument) =
                table.PeptideConsensusList <- addCollectionToList table.PeptideConsensusList peptideConsensusLists
                table

            ///Replaces smallMoleculeList of existing object with new one.
            static member addSmallMoleculeList
                (smallMoleculeList:SmallMoleculeList) (table:MzQuantMLDocument) =
                table.SmallMoleculeList <- smallMoleculeList
                table

            ///Adds new featureList to collection of enzymenames.
            static member addFeatureList
                (featureList:FeatureList) (table:MzQuantMLDocument) =
                table.FeatureList <- addToList table.FeatureList featureList
                table

            ///Adds new analysisSummary to collection of enzymenames.
            static member addAnalysisSummary
                (analysisSummary:AnalysisSummary) (table:MzQuantMLDocument) =
                table.AnalysisSummaries <- addToList table.AnalysisSummaries analysisSummary
                table

            ///Add new collection of analysisSummaries to collection of enzymenames.
            static member addAnalysisSummaries
                (analysisSummaries:seq<AnalysisSummary>) (table:MzQuantMLDocument) =
                table.AnalysisSummaries <- addCollectionToList table.AnalysisSummaries analysisSummaries
                table

            ///Replaces inputFiles of existing object with new one.
            static member addInputFiles
                (inputFiles:InputFiles) (table:MzQuantMLDocument) =
                table.InputFiles <- inputFiles
                table

            ///Replaces fkInputFiles of existing object with new one.
            static member addFKInputFiles
                (fkInputFiles:string) (table:MzQuantMLDocument) =
                table.FKInputFiles <- fkInputFiles
                table

            ///Adds new software to collection of softwares.
            static member addSoftware
                (software:Software) (table:MzQuantMLDocument) =
                table.SoftwareList <- addToList table.SoftwareList software
                table

            ///Add new collection of softwares to collection of softwares.
            static member addSoftwares
                (softwares:seq<Software>) (table:MzQuantMLDocument) =
                table.SoftwareList <- addCollectionToList table.SoftwareList softwares
                table

            ///Adds new biblioGraphicReference to collection of enzymenames.
            static member addBiblioGraphicReference
                (biblioGraphicReference:BiblioGraphicReference) (table:MzQuantMLDocument) =
                table.BiblioGraphicReferences <- addToList table.BiblioGraphicReferences biblioGraphicReference
                table

            ///Add new collection of biblioGraphicReferences to collection of enzymenames.
            static member addBiblioGraphicReferences
                (biblioGraphicReferences:seq<BiblioGraphicReference>) (table:MzQuantMLDocument) =
                table.BiblioGraphicReferences <- addCollectionToList table.BiblioGraphicReferences biblioGraphicReferences
                table

            ///Adds new studyVariable to collection of enzymenames.
            static member addStudyVariable
                (studyVariable:StudyVariable) (table:MzQuantMLDocument) =
                table.StudyVariables <- addToList table.StudyVariables studyVariable
                table

            ///Add new collection of studyVariables to collection of enzymenames.
            static member addStudyVariables
                (studyVariables:seq<StudyVariable>) (table:MzQuantMLDocument) =
                table.StudyVariables <- addCollectionToList table.StudyVariables studyVariables
                table

            ///Adds new person to collection of enzymenames.
            static member addRatio
                (ratio:Ratio) (table:MzQuantMLDocument) =
                table.Ratios <- addToList table.Ratios ratio
                table

            ///Add new collection of persons to collection of enzymenames.
            static member addRatios
                (ratios:seq<Ratio>) (table:MzQuantMLDocument) =
                table.Ratios <- addCollectionToList table.Ratios ratios
                table

            ///Tries to find a mzQuantMLDocument-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzQuantML) (id:string) =
                query {
                       for i in dbContext.MzQuantMLDocument.Local do
                           if i.ID=id
                              then select (i, i.AnalysisSummaries, i.SoftwareList, i.InputFiles, i.FeatureList, i.Assays, 
                                           i.DataProcessings, i.Providers, i.Organizations, i.Persons, i.BiblioGraphicReferences, 
                                           i.StudyVariables, i.Ratios, i.ProteinList, i.ProteinGroupList, i.PeptideConsensusList,
                                           i.SmallMoleculeList
                                          )
                      }
                |> Seq.map (fun (mzQuantML, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _) -> mzQuantML)
                |> (fun mzQuantML -> 
                    if (Seq.exists (fun mzQuantML' -> mzQuantML' <> null) mzQuantML) = false
                        then 
                            query {
                                   for i in dbContext.MzQuantMLDocument do
                                       if i.ID=id
                                          then select (i, i.AnalysisSummaries, i.SoftwareList, i.InputFiles, i.FeatureList, i.Assays, 
                                                       i.DataProcessings, i.Providers, i.Organizations, i.Persons, i.BiblioGraphicReferences, 
                                                       i.StudyVariables, i.Ratios, i.ProteinList, i.ProteinGroupList, i.PeptideConsensusList,
                                                       i.SmallMoleculeList
                                                      )
                                  }
                            |> Seq.map (fun (mzQuantML, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _) -> mzQuantML)
                            |> (fun mzQuantML -> if (Seq.exists (fun mzQuantML' -> mzQuantML' <> null) mzQuantML) = false
                                                    then None
                                                    else Some (mzQuantML.Single())
                               )
                        else Some (mzQuantML.Single())
                   )

            ///Tries to find a mzQuantMLDocument-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.MzQuantMLDocument.Local do
                           if i.Name=name
                              then select (i, i.AnalysisSummaries, i.SoftwareList, i.InputFiles, i.FeatureList, i.Assays, 
                                           i.DataProcessings, i.Providers, i.Organizations, i.Persons, i.BiblioGraphicReferences, 
                                           i.StudyVariables, i.Ratios, i.ProteinList, i.ProteinGroupList, i.PeptideConsensusList,
                                           i.SmallMoleculeList
                                          )
                      }
                |> Seq.map (fun (mzQuantML, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _) -> mzQuantML)
                |> (fun mzQuantML -> 
                    if (Seq.exists (fun mzQuantML' -> mzQuantML' <> null) mzQuantML) = false
                        then 
                            query {
                                   for i in dbContext.MzQuantMLDocument do
                                       if i.Name=name
                                          then select (i, i.AnalysisSummaries, i.SoftwareList, i.InputFiles, i.FeatureList, i.Assays, 
                                                       i.DataProcessings, i.Providers, i.Organizations, i.Persons, i.BiblioGraphicReferences, 
                                                       i.StudyVariables, i.Ratios, i.ProteinList, i.ProteinGroupList, i.PeptideConsensusList,
                                                       i.SmallMoleculeList
                                                      )
                                  }
                            |> Seq.map (fun (mzQuantML, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _) -> mzQuantML)
                            |> (fun mzQuantML -> if (Seq.exists (fun mzQuantML' -> mzQuantML' <> null) mzQuantML) = false
                                                                then None
                                                                else Some mzQuantML
                               )
                        else Some mzQuantML
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:MzQuantMLDocument) (item2:MzQuantMLDocument) =
                item1.AnalysisSummaries=item2.AnalysisSummaries && 
                item1.SoftwareList=item2.SoftwareList && 
                item1.InputFiles=item2.InputFiles && 
                item1.FeatureList=item2.FeatureList && 
                item1.Assays=item2.Assays && 
                item1.DataProcessings=item2.DataProcessings && 
                item1.Providers=item2.Providers && 
                item1.Organizations=item2.Organizations &&
                item1.Persons=item2.Persons && 
                item1.BiblioGraphicReferences=item2.BiblioGraphicReferences &&
                item1.StudyVariables=item2.StudyVariables && 
                item1.Ratios=item2.Ratios && 
                item1.ProteinList=item2.ProteinList &&
                item1.ProteinGroupList=item2.ProteinGroupList && 
                item1.PeptideConsensusList=item2.PeptideConsensusList && 
                item1.SmallMoleculeList=item2.SmallMoleculeList

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:MzQuantMLDocument) =
                    MzQuantMLDocumentHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match MzQuantMLDocumentHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> true
                                                                                                 |false -> false
                                                                            )
                                                                            |> (fun collection -> 
                                                                                 if Seq.contains true collection=true
                                                                                    then None
                                                                                    else Some(dbContext.Add item)
                                                                               )
                                                      |None -> Some(dbContext.Add item)
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:MzQuantMLDocument) =
                MzQuantMLDocumentHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        module DBFunctions =

            ///Reads obo-file and creates sequence of Obo.Terms.
            let fromFileObo (filePath:string) =
                FileIO.readFile filePath
                |> Obo.parseOboTerms

            let createOntologyAndTerms (ontologyName:string) (path:string) =
                let ontology =
                    OntologyHandler.init (ontologyName)
                let terms =
                    fromFileObo path
                    |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, ontologyName))
                ontology, terms

            let addOntologyAndTermsToMzQuantMLDB (mzQuantMLContext:MzQuantML) ((ontology, terms): Ontology*IEnumerable<Term>) =
                mzQuantMLContext.Ontology.Add ontology    |> ignore
                mzQuantMLContext.Term.AddRange (terms)    |> ignore
                mzQuantMLContext.SaveChanges()

            let createMzQuantMLContext (dbType:DBType) (dbName:string) (path:string) =
                match dbType with
                | MSSQL     -> ContextHandler.sqlConnection dbName path
                | SQLite    -> ContextHandler.sqliteConnection dbName path

            let createMzQuantMLDB (dbContext:MzQuantML) =
                dbContext.Database.EnsureCreated()

            let initMzQuantMLDB (dbType:DBType) (dbName:string) (dbPath:string) (ontologyNames:list<string>) (ontologyPaths:list<string>) =
                let dbContext =
                    match dbType with
                    | MSSQL     -> ContextHandler.sqlConnection dbName dbPath
                    | SQLite    -> ContextHandler.sqliteConnection dbName dbPath
                dbContext.Database.EnsureCreated() |> ignore
                List.map2 createOntologyAndTerms ontologyNames ontologyPaths                
                |> List.map (fun item -> addOntologyAndTermsToMzQuantMLDB dbContext item)   |> ignore
                dbContext

        module UserParams =
    
            let user0 =
                TermHandler.init("User:0000000")
                |> TermHandler.addName "N-term cleavage window"
                |> TermHandler.addOntologyID "UserParam"

            let user1 =
                TermHandler.init("User:0000001")
                |> TermHandler.addName "C-term cleavage window"
                |> TermHandler.addOntologyID "UserParam"

            let user2 =
                TermHandler.init("User:0000002")
                |> TermHandler.addName "Leading razor protein"
                |> TermHandler.addOntologyID "UserParam"

            let user3 =
                TermHandler.init("User:0000003")
                |> TermHandler.addName "MaxQuant:Unique Protein Group"
                |> TermHandler.addOntologyID  "UserParam"

            let user4 =
                TermHandler.init("User:0000004")
                |> TermHandler.addName "MaxQuant:Unique Protein"
                |> TermHandler.addOntologyID "UserParam"

            let user5 =
                TermHandler.init("User:0000005")
                |> TermHandler.addName "MaxQuant:PEP"
                |> TermHandler.addOntologyID "UserParam"

            let user6 =
                TermHandler.init("User:0000006")
                |> TermHandler.addName "IdentificationType"
                |> TermHandler.addOntologyID "UserParam"

            let user7 =
                TermHandler.init("User:0000007")
                |> TermHandler.addName "AminoAcid modification"
                |> TermHandler.addOntologyID "UserParam"

            let user8 =
                TermHandler.init("User:0000008")
                |> TermHandler.addName "Charge corrected mass of the precursor ion"
                |> TermHandler.addOntologyID "UserParam"

            let user9 =
                TermHandler.init("User:0000009")
                |> TermHandler.addName "Calibrated retention time"
                |> TermHandler.addOntologyID "UserParam"

            let user10 =
                TermHandler.init("User:0000010")
                |> TermHandler.addName "Mass error"
                |> TermHandler.addOntologyID "UserParam"

            let user11 =
                TermHandler.init("User:0000011")
                |> TermHandler.addName "The intensity values of the isotopes."
                |> TermHandler.addOntologyID "UserParam"

            let user12 =
                TermHandler.init("User:0000012")
                |> TermHandler.addName "MaxQuant:Major protein IDs"
                |> TermHandler.addOntologyID "UserParam"

            let user13 =
                TermHandler.init("User:0000013")
                |> TermHandler.addName "MaxQuant:Number of proteins"
                |> TermHandler.addOntologyID "UserParam"

            let user14 =
                TermHandler.init("User:0000014")
                |> TermHandler.addName "MaxQuant:Peptides"
                |> TermHandler.addOntologyID "UserParam"

            let user15 =
                TermHandler.init("User:0000015")
                |> TermHandler.addName "MaxQuant:Razor + unique peptides"
                |> TermHandler.addOntologyID "UserParam"

            let user16 =
                TermHandler.init("User:0000016")
                |> TermHandler.addName "MaxQuant:Unique peptides"
                |> TermHandler.addOntologyID "UserParam"

            let user17 =
                TermHandler.init("User:0000017")
                |> TermHandler.addName "MaxQuant:Unique + razor sequence coverage"
                |> TermHandler.addOntologyID "UserParam"

            let user18 =
                TermHandler.init("User:0000018")
                |> TermHandler.addName "MaxQuant:Unique sequence coverage"
                |> TermHandler.addOntologyID "UserParam"

            let user19 =
                TermHandler.init("User:0000019")
                |> TermHandler.addName "MaxQuant:SequenceLength(s)"
                |> TermHandler.addOntologyID "UserParam"

            let user20 =
                TermHandler.init("User:0000020")
                |> TermHandler.addName "Metabolic labeling N14/N15"
                |> TermHandler.addOntologyID "UserParam"

            let user21 =
                TermHandler.init("User:0000021")
                |> TermHandler.addName "DetectionType"
                |> TermHandler.addOntologyID "UserParam"

            let user22 =
                TermHandler.init("User:0000022")
                |> TermHandler.addName "MaxQuant:Uncalibrated m/z"
                |> TermHandler.addOntologyID "UserParam"

            let user23 =
                TermHandler.init("User:0000023")
                |> TermHandler.addName "MaxQuant:Number of data points"
                |> TermHandler.addOntologyID "UserParam"

            let user24 =
                TermHandler.init("User:0000024")
                |> TermHandler.addName "MaxQuant:Number of isotopic peaks"
                |> TermHandler.addOntologyID "UserParam"

            let user25 =
                TermHandler.init("User:0000025")
                |> TermHandler.addName "MaxQuant:Parent ion fraction"
                |> TermHandler.addOntologyID "UserParam"

            let user26 =
                TermHandler.init("User:0000026")
                |> TermHandler.addName "MaxQuant:Mass precision"
                |> TermHandler.addOntologyID "UserParam"

            let user27 =
                TermHandler.init("User:0000027")
                |> TermHandler.addName "Retention length (FWHM)"
                |> TermHandler.addOntologyID "UserParam"

            let user28 =
                TermHandler.init("User:0000028")
                |> TermHandler.addName "MaxQuant:Min scan number"
                |> TermHandler.addOntologyID "UserParam"

            let user29 =
                TermHandler.init("User:0000029")
                |> TermHandler.addName "MaxQuant:Max scan number"
                |> TermHandler.addOntologyID "UserParam"

            let user30 =
                TermHandler.init("User:0000030")
                |> TermHandler.addName "MaxQuant:MSMS scan numbers"
                |> TermHandler.addOntologyID "UserParam"

            let user31 =
                TermHandler.init("User:0000031")
                |> TermHandler.addName "MaxQuant:MSMS isotope indices"
                |> TermHandler.addOntologyID "UserParam"

            let user32 =
                TermHandler.init("User:0000032")
                |> TermHandler.addName "MaxQuant:Filtered peaks"
                |> TermHandler.addOntologyID "UserParam"

            let user33 =
                TermHandler.init("User:0000033")
                |> TermHandler.addName "FragmentationType"
                |> TermHandler.addOntologyID "UserParam"

            let user34 =
                TermHandler.init("User:0000034")
                |> TermHandler.addName "Parent intensity fraction"
                |> TermHandler.addOntologyID "UserParam"

            let user35 =
                TermHandler.init("User:0000035")
                |> TermHandler.addName "Fraction of total spectrum"
                |> TermHandler.addOntologyID "UserParam"

            let user36 =
                TermHandler.init("User:0000036")
                |> TermHandler.addName "Base peak fraction"
                |> TermHandler.addOntologyID "UserParam"

            let user37 =
                TermHandler.init("User:0000037")
                |> TermHandler.addName "Precursor full scan number"
                |> TermHandler.addOntologyID "UserParam"

            let user38 =
                TermHandler.init("User:0000038")
                |> TermHandler.addName "Precursor intensity"
                |> TermHandler.addOntologyID "UserParam"

            let user39 =
                TermHandler.init("User:0000039")
                |> TermHandler.addName "Precursor apex fraction"
                |> TermHandler.addOntologyID "UserParam"

            let user40 =
                TermHandler.init("User:0000040")
                |> TermHandler.addName "Precursor apex offset"
                |> TermHandler.addOntologyID "UserParam"

            let user41 =
                TermHandler.init("User:0000041")
                |> TermHandler.addName "Precursor apex offset time"
                |> TermHandler.addOntologyID "UserParam"

            let user42 =
                TermHandler.init("User:0000042")
                |> TermHandler.addName "Scan event number"
                |> TermHandler.addOntologyID "UserParam"

            let user43 =
                TermHandler.init("User:0000043")
                |> TermHandler.addName "MaxQuant:Score difference"
                |> TermHandler.addOntologyID "UserParam"

            let user44 =
                TermHandler.init("User:0000044")
                |> TermHandler.addName "MaxQuant:Combinatorics"
                |> TermHandler.addOntologyID "UserParam"

            let user45 =
                TermHandler.init("User:0000045")
                |> TermHandler.addName "MaxQuant:Matches"
                |> TermHandler.addOntologyID "UserParam"

            let user46 =
                TermHandler.init("User:0000046")
                |> TermHandler.addName "MaxQuant:Match between runs"
                |> TermHandler.addOntologyID "UserParam"

            let user47 =
                TermHandler.init("User:0000047")
                |> TermHandler.addName "MaxQuant:Number of matches"
                |> TermHandler.addOntologyID "UserParam"

            let user48 =
                TermHandler.init("User:0000048")
                |> TermHandler.addName "MaxQuant:Intensity coverage"
                |> TermHandler.addOntologyID "UserParam"

            let user49 =
                TermHandler.init("User:0000049")
                |> TermHandler.addName "MaxQuant:Peak coverage"
                |> TermHandler.addOntologyID "UserParam"

            let user50 =
                TermHandler.init("User:0000050")
                |> TermHandler.addName "MaxQuant:ETD identification type"
                |> TermHandler.addOntologyID "UserParam"

            let user51 =
                TermHandler.init("User:0000051")
                |> TermHandler.addName "Min. score unmodified peptides"
                |> TermHandler.addOntologyID "UserParam"
  
            let user52 =
                TermHandler.init("User:0000052")
                |> TermHandler.addName "Min. score modified peptides"
                |> TermHandler.addOntologyID "UserParam"

            let user53 =
                TermHandler.init("User:0000053")
                |> TermHandler.addName "Min. delta score of unmodified peptides"
                |> TermHandler.addOntologyID "UserParam" 

            let user54 =
                TermHandler.init("User:0000054")
                |> TermHandler.addName "Min. delta score of modified peptides"
                |> TermHandler.addOntologyID "UserParam"

            let user55 =
                TermHandler.init("User:0000055")
                |> TermHandler.addName "Min. amount unique peptide"
                |> TermHandler.addOntologyID "UserParam"

            let user56 =
                TermHandler.init("User:0000056")
                |> TermHandler.addName "Min. amount razor peptide"
                |> TermHandler.addOntologyID "UserParam"

            let user57 =
                TermHandler.init("User:0000057")
                |> TermHandler.addName "Min. amount peptide"
                |> TermHandler.addOntologyID "UserParam"

            let user58 =
                TermHandler.init("User:0000058")
                |> TermHandler.addName "MaxQuant:Decoy mode"
                |> TermHandler.addOntologyID "UserParam"

            let user59 =
                TermHandler.init("User:0000059")
                |> TermHandler.addName "MaxQuant:Special AAs"
                |> TermHandler.addOntologyID "UserParam"

            let user60 =
                TermHandler.init("User:0000060")
                |> TermHandler.addName "MaxQuant:Include contaminants"
                |> TermHandler.addOntologyID "UserParam"

            let user61 =
                TermHandler.init("User:0000061")
                |> TermHandler.addName "MaxQuant:iBAQ"
                |> TermHandler.addOntologyID "UserParam"

            let user62 =
                TermHandler.init("User:0000062")
                |> TermHandler.addName "MaxQuant:Top MS/MS peaks per 100 Dalton"
                |> TermHandler.addOntologyID "UserParam"

            let user63 =
                TermHandler.init("User:0000063")
                |> TermHandler.addName "MaxQuant:IBAQ log fit"
                |> TermHandler.addOntologyID "UserParam"

            let user64 =
                TermHandler.init("User:0000064")
                |> TermHandler.addName "MaxQuant:Protein FDR"
                |> TermHandler.addOntologyID "UserParam"

            let user65 =
                TermHandler.init("User:0000065")
                |> TermHandler.addName "MaxQuant:SiteFDR"
                |> TermHandler.addOntologyID "UserParam"

            let user66 =
                TermHandler.init("User:0000066")
                |> TermHandler.addName "MaxQuant:Use Normalized Ratios For Occupancy"
                |> TermHandler.addOntologyID "UserParam"

            let user67 =
                TermHandler.init("User:0000067")
                |> TermHandler.addName "MaxQuant:Peptides used for protein quantification"
                |> TermHandler.addOntologyID "UserParam"

            let user68 =
                TermHandler.init("User:0000068")
                |> TermHandler.addName "MaxQuant:Discard unmodified counterpart peptides"
                |> TermHandler.addOntologyID "UserParam"

            let user69 =
                TermHandler.init("User:0000069")
                |> TermHandler.addName "MaxQuant:Min. ratio count"
                |> TermHandler.addOntologyID "UserParam"

            let user70 =
                TermHandler.init("User:0000070")
                |> TermHandler.addName "MaxQuant:Use delta score"
                |> TermHandler.addOntologyID "UserParam"

            let user71 =
                TermHandler.init("User:0000071")
                |> TermHandler.addName "Data-dependt acquisition"
                |> TermHandler.addOntologyID "UserParam"

            let user72 =
                TermHandler.init("User:0000072")
                |> TermHandler.addName "razor-protein"
                |> TermHandler.addOntologyID "UserParam"

            let user73 =
                TermHandler.init("User:0000073")
                |> TermHandler.addName "razor-peptide"
                |> TermHandler.addOntologyID "UserParam"

            let user74 =
                TermHandler.init("User:0000074")
                |> TermHandler.addName "Mass-deviation"
                |> TermHandler.addOntologyID "UserParam"

            let user75 =
                TermHandler.init("User:0000075")
                |> TermHandler.addName "leading-peptide"
                |> TermHandler.addOntologyID "UserParam"

            let user76 =
                TermHandler.init("User:0000076")
                |> TermHandler.addName "unique-protein"
                |> TermHandler.addOntologyID "UserParam"

            let user77 =
                TermHandler.init("User:0000077")
                |> TermHandler.addName "unique-peptide"
                |> TermHandler.addOntologyID "UserParam"

            let user78 =
                TermHandler.init("User:0000078")
                |> TermHandler.addName "MaxQuant:Delta score"
                |> TermHandler.addOntologyID "UserParam"

            let user79 =
                TermHandler.init("User:0000079")
                |> TermHandler.addName "MaxQuant:Best andromeda score"
                |> TermHandler.addOntologyID "UserParam"

            let userParams =
                [
                 user0; user1; user2; user3; user4; user5; user6; user7; user8; user9; user10; user11; user12; user13; user14; user15; user16; user17; 
                 user18; user19; user20; user21; user22; user23; user24; user25; user26; user27; user28; user29; user30; user31; user32; user33; user34;
                 user35; user36; user37; user38; user39; user40; user41; user42; user43; user44; user45; user46; user47; user48; user49; user50; user51;
                 user52; user53; user54; user55; user56; user57; user58; user59; user60; user61; user62; user63; user64; user65; user66; user67; user68;
                 user69; user70; user71; user72; user73; user74; user75; user76; user77; user78; user79;
                ]

            let createOboString (term:Term) =
                [["[Term]"; "id: " + term.ID; "name: " + term.Name]; [""]]
                |> List.concat

            let createOboFile (path:string) (oboTerm:seq<string>) =
                Seq.write path oboTerm

