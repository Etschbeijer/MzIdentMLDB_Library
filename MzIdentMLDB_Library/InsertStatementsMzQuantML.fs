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
            static member sqliteConnection (path:string) =
                let optionsBuilder = 
                    new DbContextOptionsBuilder<MzQuantML>()
                optionsBuilder.UseSqlite(@"Data Source=" + path) |> ignore
                new MzQuantML(optionsBuilder.Options)       

            ///Creats connection for SQL-context and database.
            static member sqlConnection() =
                let optionsBuilder = 
                    new DbContextOptionsBuilder<MzQuantML>()
                optionsBuilder.UseSqlServer("Server=(localdb)\mssqllocaldb;Database=MyDatabase;Trusted_Connection=True;") |> ignore
                new MzQuantML(optionsBuilder.Options) 

            ///Reads obo-file and creates sequence of Obo.Terms.
            static member fromFileObo (filePath:string) =
                FileIO.readFile filePath
                |> Obo.parseOboTerms

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
                    id        : string,
                    ?name     : string,
                    ?ontology : Ontology  
                ) =
                let name'      = defaultArg name Unchecked.defaultof<string>
                let ontology'  = defaultArg ontology Unchecked.defaultof<Ontology>

                new Term(
                         id, 
                         name', 
                         ontology', 
                         (Nullable(DateTime.Now))
                        )

            ///Replaces a name of an existing object with new name.
            static member addName
                (name:string) (term:Term) =
                term.Name <- name
                term
                    
            ///Replaces an ontology of an existing term-object with new ontology.
            static member addOntology
                (ontology:Ontology) (term:Term) =
                term.Ontology <- ontology
                term

            ///Tries to find a term-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (termID:string) =
                tryFind (context.Term.Find(termID))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.Term.Local do
                           if i.Name=name
                              then select (i, i.Ontology)
                      }
                |> Seq.map (fun (term, _) -> term)
                |> (fun term -> 
                    if (Seq.exists (fun term' -> term' <> null) term) = false
                        then 
                            query {
                                   for i in dbContext.Term do
                                       if i.Name=name
                                          then select (i, i.Ontology)
                                  }
                            |> Seq.map (fun (term, _) -> term)
                            |> (fun term -> if (Seq.exists (fun term' -> term' <> null) term) = false
                                                then None
                                                else Some term
                               )
                        else Some term
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Term) (item2:Term) =
                item1.Ontology=item2.Ontology

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Term) =
                    TermHandler.tryFindByName dbContext item.Name
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match TermHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            
            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Term) =
                TermHandler.addToContext dbContext item |> ignore
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
                (term:Term) (ontology:Ontology) =
                let result = ontology.Terms <- addToList ontology.Terms term
                ontology

            ///Adds a collection of terms to an existing ontology-object.
            static member addTerms
                (terms:seq<Term>) (ontology:Ontology) =
                let result = ontology.Terms <- addCollectionToList ontology.Terms terms
                ontology

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (ontologyID:string) =
                tryFind (context.Ontology.Find(ontologyID))

        type CVParamHandler =
            ///Initializes a cvparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : string,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>

                new CVParam(
                            id', 
                            value', 
                            term, 
                            unit', 
                            Nullable(DateTime.Now)
                           )

            ///Replaces value of existing object with new value.
            static member addValue
                (value:string) (cvParam:CVParam) =
                cvParam.Value <- value
                cvParam

            ///Replaces unit of existing object with new unit.
            static member addUnit
                (unit:Term) (cvParam:CVParam) =
                cvParam.Unit <- unit
                cvParam

            ///Tries to find a cvparam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (paramID:string) =
                tryFind (context.CVParam.Find(paramID))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.CVParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.CVParam do
                                       if i.Term.Name=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:CVParam) (item2:CVParam) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:CVParam) =
                    CVParamHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match CVParamHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:CVParam) =
                CVParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type OrganizationParamHandler =
            ///Initializes a organizationparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : string,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>

                new OrganizationParam(
                                      id', 
                                      value', 
                                      term, 
                                      unit', 
                                      Nullable(DateTime.Now)
                                     )

            ///Replaces value of existing object with new value.
            static member addValue
                (value:string) (param:OrganizationParam) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new unit.
            static member addUnit
                (unit:Term) (param:OrganizationParam) =
                param.Unit <- unit
                param

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (paramID:string) =
                tryFind (context.OrganizationParam.Find(paramID))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.OrganizationParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.OrganizationParam do
                                       if i.Term.Name=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:OrganizationParam) (item2:OrganizationParam) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:OrganizationParam) =
                    OrganizationParamHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> 
                                                                match OrganizationParamHandler.hasEqualFieldValues cvParam item with
                                                                |true -> null
                                                                |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:OrganizationParam) =
                OrganizationParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type PersonParamHandler =
            ///Initializes a personparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : string,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new PersonParam(
                                id', 
                                value', 
                                term, 
                                unit', 
                                Nullable(DateTime.Now)
                               )

            ///Replaces value of existing object with new value.
            static member addValue
                (value:string) (param:PersonParam) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new unit.
            static member addUnit
                (unit:Term) (param:PersonParam) =
                param.Unit <- unit
                param

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (paramID:string) =
                tryFind (context.PersonParam.Find(paramID))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.PersonParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PersonParam do
                                       if i.Term.Name=name
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:PersonParam) (item2:PersonParam) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:PersonParam) =
                    PersonParamHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match PersonParamHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:PersonParam) =
                PersonParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()