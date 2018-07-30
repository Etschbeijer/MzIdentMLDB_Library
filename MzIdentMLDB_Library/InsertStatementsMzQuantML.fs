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
                (name:string) (table:Term) =
                table.Name <- name
                table
                    
            ///Replaces an ontology of an existing term-object with new ontology.
            static member addOntology
                (ontology:Ontology) (table:Term) =
                table.Ontology <- ontology
                table

            ///Tries to find a term-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.Term.Find(id))

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
                (term:Term) (table:Ontology) =
                let result = table.Terms <- addToList table.Terms term
                table

            ///Adds a collection of terms to an existing ontology-object.
            static member addTerms
                (terms:seq<Term>) (table:Ontology) =
                let result = table.Terms <- addCollectionToList table.Terms terms
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.Ontology.Find(id))

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
                (value:string) (table:CVParam) =
                table.Value <- value
                table

            ///Replaces unit of existing object with new unit.
            static member addUnit
                (unit:Term) (table:CVParam) =
                table.Unit <- unit
                table

            ///Tries to find a cvparam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.CVParam.Find(id))

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
                (value:string) (table:OrganizationParam) =
                table.Value <- value
                table

            ///Replaces unit of existing object with new unit.
            static member addUnit
                (unit:Term) (table:OrganizationParam) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.OrganizationParam.Find(id))

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
                (value:string) (table:PersonParam) =
                table.Value <- value
                table

            ///Replaces unit of existing object with new unit.
            static member addUnit
                (unit:Term) (param:PersonParam) =
                param.Unit <- unit
                param

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.PersonParam.Find(id))

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

        type AnalysisSoftwareParamHandler =
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
                    
                new AnalysisSoftwareParam(
                                          id', 
                                          value', 
                                          term, 
                                          unit', 
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces value of existing object with new value.
            static member addValue
                (value:string) (param:AnalysisSoftwareParam) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new unit.
            static member addUnit
                (unit:Term) (table:AnalysisSoftwareParam) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.AnalysisSoftwareParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.AnalysisSoftwareParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisSoftwareParam do
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
            static member private hasEqualFieldValues (item1:AnalysisSoftwareParam) (item2:AnalysisSoftwareParam) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:AnalysisSoftwareParam) =
                    AnalysisSoftwareParamHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match AnalysisSoftwareParamHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:AnalysisSoftwareParam) =
                AnalysisSoftwareParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SearchDatabaeParamHandler =
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
                    
                new SearchDatabaseParam(
                                        id', 
                                        value', 
                                        term, 
                                        unit', 
                                        Nullable(DateTime.Now)
                                       )

            ///Replaces value of existing object with new value.
            static member addValue
                (value:string) (param:SearchDatabaseParam) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new unit.
            static member addUnit
                (unit:Term) (table:SearchDatabaseParam) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.SearchDatabaseParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.SearchDatabaseParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SearchDatabaseParam do
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
            static member private hasEqualFieldValues (item1:SearchDatabaseParam) (item2:SearchDatabaseParam) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:SearchDatabaseParam) =
                    SearchDatabaeParamHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match SearchDatabaeParamHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:SearchDatabaseParam) =
                SearchDatabaeParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type RawFileParamHandler =
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
                    
                new RawFileParam(
                                 id', 
                                 value', 
                                 term, 
                                 unit', 
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new value.
            static member addValue
                (value:string) (param:RawFileParam) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new unit.
            static member addUnit
                (unit:Term) (table:RawFileParam) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.RawFileParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.RawFileParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.RawFileParam do
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
            static member private hasEqualFieldValues (item1:RawFileParam) (item2:RawFileParam) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:RawFileParam) =
                    RawFileParamHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match RawFileParamHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:RawFileParam) =
                RawFileParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type AssayParamHandler =
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
                    
                new AssayParam(
                               id', 
                               value', 
                               term, 
                               unit', 
                               Nullable(DateTime.Now)
                              )

            ///Replaces value of existing object with new value.
            static member addValue
                (value:string) (param:AssayParam) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new unit.
            static member addUnit
                (unit:Term) (table:AssayParam) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.AssayParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.AssayParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.AssayParam do
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
            static member private hasEqualFieldValues (item1:AssayParam) (item2:AssayParam) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:AssayParam) =
                    AssayParamHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match AssayParamHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:AssayParam) =
                AssayParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type StudyVariableParamHandler =
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
                    
                new StudyVariableParam(
                                       id', 
                                       value', 
                                       term, 
                                       unit', 
                                       Nullable(DateTime.Now)
                                      )

            ///Replaces value of existing object with new value.
            static member addValue
                (value:string) (param:AssayParam) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new unit.
            static member addUnit
                (unit:Term) (table:StudyVariableParam) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.StudyVariableParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.StudyVariableParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.StudyVariableParam do
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
            static member private hasEqualFieldValues (item1:StudyVariableParam) (item2:StudyVariableParam) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:StudyVariableParam) =
                    StudyVariableParamHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match StudyVariableParamHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:StudyVariableParam) =
                StudyVariableParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type RatioCalculationParamHandler =
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
                    
                new RatioCalculationParam(
                                          id', 
                                          value', 
                                          term, 
                                          unit', 
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces value of existing object with new value.
            static member addValue
                (value:string) (param:RatioCalculationParam) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new unit.
            static member addUnit
                (unit:Term) (table:RatioCalculationParam) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.RatioCalculationParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.RatioCalculationParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.RatioCalculationParam do
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
            static member private hasEqualFieldValues (item1:RatioCalculationParam) (item2:RatioCalculationParam) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:RatioCalculationParam) =
                    RatioCalculationParamHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match RatioCalculationParamHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:RatioCalculationParam) =
                RatioCalculationParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

//////////////////////////////////////////
//End of paramHandlers//////////////////////////////////////////////
//////////////////////////////////////////

        type AnalysisSoftwareHandler =
            ///Initializes a personparam-object with at least all necessary parameters.
            static member init
                (
                    id        : string,
                    version   : string,
                    ?details  : seq<AnalysisSoftwareParam>
                ) =

                let details'  = convertOptionToList details
                    
                new AnalysisSoftware(
                                     id, 
                                     version, 
                                     details', 
                                     Nullable(DateTime.Now)
                                    )

            ///Adds new enzymename to collection of enzymenames.
            static member addDetail
                (analysisSoftwareParam:AnalysisSoftwareParam) (table:AnalysisSoftware) =
                let result = table.Details <- addToList table.Details analysisSoftwareParam
                table

            ///Add new collection of enzymenames to collection of enzymenames.
            static member addDetails
                (analysisSoftwareParams:seq<AnalysisSoftwareParam>) (table:AnalysisSoftware) =
                let result = table.Details <- addCollectionToList table.Details analysisSoftwareParams
                table

            ///Tries to find a analysisSoftware-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.AnalysisSoftware.Find(id))

            ///Tries to find a analysisSoftware-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByVersion (dbContext:MzQuantML) (version:string) =
                query {
                       for i in dbContext.AnalysisSoftware.Local do
                           if i.Version=version
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (analysisSoftware, _) -> analysisSoftware)
                |> (fun analysisSoftware -> 
                    if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisSoftware do
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
            static member private hasEqualFieldValues (item1:AnalysisSoftware) (item2:AnalysisSoftware) =
                item1.Details=item2.Details

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:AnalysisSoftware) =
                    AnalysisSoftwareHandler.tryFindByVersion dbContext item.Version
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match AnalysisSoftwareHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:AnalysisSoftware) =
                AnalysisSoftwareHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type SourceFileHandler =
            ///Initializes a personparam-object with at least all necessary parameters.
            static member init
                (
                    id                           : string,
                    location                     : string,
                    ?name                        : string,
                    ?externalFormatDocumentation : string,
                    ?fileFormat                  : CVParam
                ) =

                let name'                         = defaultArg name Unchecked.defaultof<string>
                let externalFormatDocumentation'  = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let fileFormat'                   = defaultArg fileFormat Unchecked.defaultof<CVParam>
                    
                new SourceFile(
                               id,
                               name',
                               location, 
                               externalFormatDocumentation',
                               fileFormat',
                               Nullable(DateTime.Now)
                              )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (table:SourceFile) =
                table.Name <- name
                table

            ///Replaces externalFormatDocumentation of existing object with new externalFormatDocumentation.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (table:SourceFile) =
                table.ExternalFormatDocumentation <- externalFormatDocumentation
                table

            ///Replaces fileFormat of existing object with new fileFormat.
            static member addFileFormat
                (fileFormat:CVParam) (table:SourceFile) =
                table.FileFormat <- fileFormat
                table

            ///Tries to find a analysisSoftware-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.SourceFile.Find(id))

            ///Tries to find a analysisSoftware-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByVersion (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.SourceFile.Local do
                           if i.Name=name
                              then select (i, i.FileFormat)
                      }
                |> Seq.map (fun (sourceFile, _) -> sourceFile)
                |> (fun sourceFile -> 
                    if (Seq.exists (fun sourceFile' -> sourceFile' <> null) sourceFile) = false
                        then 
                            query {
                                   for i in dbContext.SourceFile do
                                       if i.Name=name
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
                item1.FileFormat=item2.FileFormat

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:SourceFile) =
                    SourceFileHandler.tryFindByVersion dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SourceFileHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:SourceFile) =
                SourceFileHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type OrganizationHandler =
            ///Initializes a organization-object with at least all necessary parameters.
            static member init
                (
                    ?id      : string,
                    ?name    : string,
                    ?details : seq<OrganizationParam>,
                    ?parent  : string
                ) =
                let id'      = defaultArg id (System.Guid.NewGuid().ToString())
                let name'    = defaultArg name Unchecked.defaultof<string>
                let details' = convertOptionToList details
                let parent'  = defaultArg parent Unchecked.defaultof<string>
                    
                new Organization(
                                 id', 
                                 name', 
                                 details',  
                                 parent', 
                                 Nullable(DateTime.Now)
                                )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (table:Organization) =
                table.Name <- name
                table

            ///Replaces parent of existing object with new parent.
            static member addParent
                (parent:string) (table:Organization) =
                table.Parent <- parent
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

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.Organization.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.Organization.Local do
                           if i.Name = name
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (organization, _ ) -> organization)
                |> (fun organization -> 
                    if (Seq.exists (fun organization' -> organization' <> null) organization) = false
                        then 
                            query {
                                   for i in dbContext.Organization do
                                       if i.Name = name
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (organization, _ ) -> organization)
                            |> (fun param -> if (Seq.exists (fun organization' -> organization' <> null) organization) = false
                                                then None
                                                else Some param
                               )
                        else Some organization
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Organization) (item2:Organization) =
                item1.Details=item2.Details && item1.Parent=item2.Parent

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Organization) =
                    OrganizationHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match OrganizationHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Organization) =
                OrganizationHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type PersonHandler =
        ///Initializes a person-object with at least all necessary parameters.
            static member init
                (
                    ?id             : string,
                    ?name           : string,
                    ?firstName      : string,
                    ?midInitials    : string,
                    ?lastName       : string,
                    ?contactDetails : seq<PersonParam>,
                    ?organizations  : seq<Organization> 
                ) =
                let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'           = defaultArg name Unchecked.defaultof<string>
                let firstName'      = defaultArg firstName Unchecked.defaultof<string>
                let midInitials'    = defaultArg midInitials Unchecked.defaultof<string>
                let lastName'       = defaultArg lastName Unchecked.defaultof<string>
                let contactDetails' = convertOptionToList contactDetails
                let organizations'  = convertOptionToList organizations
                    
                new Person(
                           id', 
                           name', 
                           firstName',  
                           midInitials', 
                           lastName', 
                           organizations',
                           contactDetails', 
                           Nullable(DateTime.Now)
                          )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (table:Person) =
                table.Name <- name
                table

            ///Replaces firstname of existing object with new firstname.
            static member addFirstName
                (firstName:string) (table:Person) =
                table.FirstName <- firstName
                table

            ///Replaces midinitials of existing object with new midinitials.
            static member addMidInitials
                (midInitials:string) (table:Person) =
                table.MidInitials <- midInitials
                table

            ///Replaces lastname of existing object with new lastname.
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

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.Person.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
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
                item1.Organizations=item2.Organizations && item1.Details=item2.Details

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Person) =
                    PersonHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PersonHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Person) =
                PersonHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ContactRoleHandler =
            ///Initializes a contactrole-object with at least all necessary parameters.
            static member init
                (   
                    person : Person, 
                    role   : CVParam,
                    ?id    : string
                ) =
                let id' = defaultArg id (System.Guid.NewGuid().ToString())
                    
                new ContactRole(
                                id', 
                                person, 
                                role, 
                                Nullable(DateTime.Now)
                               )

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.ContactRole.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByPersonName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.ContactRole.Local do
                           if i.Person.Name = name
                              then select (i, i.Role)
                      }
                |> Seq.map (fun (contactRole, _) -> contactRole)
                |> (fun contactRole -> 
                    if (Seq.exists (fun contactRole' -> contactRole' <> null) contactRole) = false
                        then 
                            query {
                                   for i in dbContext.ContactRole do
                                       if i.Person.Name = name
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
                    ContactRoleHandler.tryFindByPersonName dbContext item.Person.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ContactRoleHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:ContactRole) =
                ContactRoleHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProviderHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    ?id               : string,
                    ?name             : string,
                    ?analysisSoftware : AnalysisSoftware,
                    ?contactRole      : ContactRole
                    //?mzIdentML        : MzIdentMLDocument
                ) =
                let id'               = defaultArg id (System.Guid.NewGuid().ToString())
                let name'             = defaultArg name Unchecked.defaultof<string>
                let analysisSoftware' = defaultArg analysisSoftware Unchecked.defaultof<AnalysisSoftware>
                let contactRole'      = defaultArg contactRole Unchecked.defaultof<ContactRole>
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new Provider(
                             id', 
                             name', 
                             analysisSoftware', 
                             contactRole', 
                             //mzIdentML', 
                             Nullable(DateTime.Now)
                            )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (table:Provider) =
                table.Name <- name
                table

            ///Replaces analysissoftware of existing object with new analysissoftware.
            static member addAnalysisSoftware
                (analysisSoftware:AnalysisSoftware) (table:Provider) =
                table.AnalysisSoftware <- analysisSoftware
                table

            ///Replaces contactrole of existing object with new contactrole.
            static member addContactRole
                (contactRole:ContactRole) (table:Provider) =
                table.ContactRole <- contactRole
                table

            /////Replaces mzidentml of existing object with new mzidentml.
            //static member addMzIdentMLDocument
            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
            //    provider

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.Provider.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.Provider.Local do
                           if i.Name=name
                              then select (i, i.AnalysisSoftware, i.ContactRole(*, i.MzIdentMLDocument*))
                      }
                |> Seq.map (fun (provider, _, _) -> provider)
                |> (fun provider -> 
                    if (Seq.exists (fun provider' -> provider' <> null) provider) = false
                        then 
                            query {
                                   for i in dbContext.Provider do
                                       if i.Name=name
                                          then select (i, i.AnalysisSoftware, i.ContactRole(*, i.MzIdentMLDocument*))
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
               item1.AnalysisSoftware=item2.AnalysisSoftware && item1.ContactRole=item2.ContactRole 
               //&& item1.MzIdentMLDocument=item2.MzIdentMLDocument 

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Provider) =
                    ProviderHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProviderHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Provider) =
                ProviderHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type SearchDatabaseHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    location                     : string,
                    databaseName                 : CVParam,
                    ?id                          : string,
                    ?name                        : string,
                    ?numDatabaseEntries          : int,
                    ?releaseDate                 : DateTime,
                    ?version                     : string,
                    ?externalFormatDocumentation : string,
                    ?fileFormat                  : CVParam,
                    ?details                     : seq<SearchDatabaseParam>
                    //?mzIdentML        : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let numDatabaseEntries'          = defaultArg numDatabaseEntries Unchecked.defaultof<int>
                let releaseDate'                 = defaultArg releaseDate Unchecked.defaultof<DateTime>
                let version'                     = defaultArg version Unchecked.defaultof<string>
                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let fileFormat'                  = defaultArg fileFormat Unchecked.defaultof<CVParam>
                let details'                     = convertOptionToList details
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new SearchDatabase(
                                   id', 
                                   name', 
                                   location, 
                                   Nullable(numDatabaseEntries'),
                                   Nullable(releaseDate'),
                                   version',
                                   externalFormatDocumentation',
                                   fileFormat',
                                   databaseName,
                                   details',
                                   //mzIdentML', 
                                   Nullable(DateTime.Now)
                                  )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (table:SearchDatabase) =
                table.Name <- name
                table

            ///Replaces numDatabaseEntries of existing object with new numDatabaseEntries.
            static member addNumDatabaseEntries
                (numDatabaseEntries:int) (table:SearchDatabase) =
                table.NumDatabaseEntries <- Nullable(numDatabaseEntries)
                table

            ///Replaces releaseDate of existing object with new releaseDate.
            static member addReleaseDate
                (releaseDate:DateTime) (table:SearchDatabase) =
                table.ReleaseDate <- Nullable(releaseDate)
                table

            ///Replaces version of existing object with new version.
            static member addVersion
                (version:string) (table:SearchDatabase) =
                table.Version <- version
                table

            ///Replaces version of existing object with new version.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (table:SearchDatabase) =
                table.Version <- externalFormatDocumentation
                table

            ///Replaces fileFormat of existing object with new fileFormat.
            static member addFileFormat
                (fileFormat:CVParam) (table:SearchDatabase) =
                table.FileFormat <- fileFormat
                table
            
            ///Replaces fileFormat of existing object with new fileFormat.
            static member addDatabaseName
                (databaseName:CVParam) (table:SearchDatabase) =
                table.DatabaseName <- databaseName
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

            /////Replaces mzidentml of existing object with new mzidentml.
            //static member addMzIdentMLDocument
            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
            //    provider

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.SearchDatabase.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
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
               item1.FileFormat=item2.FileFormat && item1.DatabaseName=item2.DatabaseName &&
               item1.Details=item2.Details

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:SearchDatabase) =
                    SearchDatabaseHandler.tryFindByLocation dbContext item.Location
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SearchDatabaseHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:SearchDatabase) =
                SearchDatabaseHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type IdentificationFileHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    location                     : string,
                    ?id                          : string,
                    ?name                        : string,
                    ?searchDatabase              : SearchDatabase,
                    ?externalFormatDocumentation : string,
                    ?fileFormat                  : CVParam,
                    ?details                     : seq<IdentificationFileParam>
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let searchDatabase'              = defaultArg searchDatabase Unchecked.defaultof<SearchDatabase>
                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let fileFormat'                  = defaultArg fileFormat Unchecked.defaultof<CVParam>
                let details'                     = convertOptionToList details
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new IdentificationFile(
                                       id', 
                                       name', 
                                       location, 
                                       searchDatabase',
                                       externalFormatDocumentation',
                                       fileFormat',
                                       details',
                                       //mzIdentML', 
                                       Nullable(DateTime.Now)
                                      )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (table:IdentificationFile) =
                table.Name <- name
                table

            ///Replaces searchDatabase of existing object with new searchDatabase.
            static member addNumSearchDatabase
                (searchDatabase:SearchDatabase) (table:IdentificationFile) =
                table.SearchDatabase <- searchDatabase
                table

            ///Replaces externalFormatDocumentation of existing object with new externalFormatDocumentation.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (table:IdentificationFile) =
                table.ExternalFormatDocumentation <- externalFormatDocumentation
                table

            ///Replaces fileFormat of existing object with new fileFormat.
            static member addFileFormat
                (fileFormat:CVParam) (table:IdentificationFile) =
                table.FileFormat <- fileFormat
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

            /////Replaces mzidentml of existing object with new mzidentml.
            //static member addMzIdentMLDocument
            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
            //    provider

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.IdentificationFile.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
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
               item1.FileFormat=item2.FileFormat && item1.Details=item2.Details

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:IdentificationFile) =
                    IdentificationFileHandler.tryFindByLocation dbContext item.Location
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match IdentificationFileHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:IdentificationFile) =
                IdentificationFileHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type IdentificationRefHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    fkIdentificationFile : string,
                    identificationFile   : IdentificationFile,
                    ?id                  : string
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                 = defaultArg id (System.Guid.NewGuid().ToString())
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new IdentificationRef(
                                      id', 
                                      fkIdentificationFile,
                                      identificationFile,
                                      //mzIdentML', 
                                      Nullable(DateTime.Now)
                                     )

            /////Replaces mzidentml of existing object with new mzidentml.
            //static member addMzIdentMLDocument
            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
            //    provider

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.IdentificationRef.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
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
               item1.IdentificationFile=item2.IdentificationFile

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:IdentificationRef) =
                    IdentificationRefHandler.tryFindByFKIdentificationFile dbContext item.FKIdentificationFile
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match IdentificationRefHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:IdentificationRef) =
                IdentificationRefHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type MethodFileHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    location                     : string,
                    ?id                          : string,
                    ?name                        : string,
                    ?externalFormatDocumentation : string,
                    ?fileFormat                  : CVParam
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let fileFormat'                  = defaultArg fileFormat Unchecked.defaultof<CVParam>
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new MethodFile(
                               id', 
                               name', 
                               location, 
                               externalFormatDocumentation',
                               fileFormat',
                               //mzIdentML', 
                               Nullable(DateTime.Now)
                              )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (table:MethodFile) =
                table.Name <- name
                table

            ///Replaces externalFormatDocumentation of existing object with new externalFormatDocumentation.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (table:MethodFile) =
                table.ExternalFormatDocumentation <- externalFormatDocumentation
                table

            ///Replaces fileFormat of existing object with new fileFormat.
            static member addFileFormat
                (fileFormat:CVParam) (table:MethodFile) =
                table.FileFormat <- fileFormat
                table

            /////Replaces mzidentml of existing object with new mzidentml.
            //static member addMzIdentMLDocument
            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
            //    provider

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.MethodFile.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
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
               && item1.FileFormat=item2.FileFormat

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:MethodFile) =
                    MethodFileHandler.tryFindByLocation dbContext item.Location
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match MethodFileHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:MethodFile) =
                MethodFileHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type RawFileHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    location                     : string,
                    ?id                          : string,
                    ?name                        : string,
                    ?externalFormatDocumentation : string,
                    ?methodFile                  : MethodFile,
                    ?fileFormat                  : CVParam, 
                    ?details                     : seq<RawFileParam>
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let methodFile'                  = defaultArg methodFile Unchecked.defaultof<MethodFile>
                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let fileFormat'                  = defaultArg fileFormat Unchecked.defaultof<CVParam>
                let details'                     = convertOptionToList details
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new RawFile(
                            id', 
                            name', 
                            location, 
                            methodFile',
                            externalFormatDocumentation',
                            fileFormat',
                            details',
                            //mzIdentML', 
                            Nullable(DateTime.Now)
                           )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (table:RawFile) =
                table.Name <- name
                table

            ///Replaces name of existing object with new name.
            static member addMethodFile
                (methodFile:MethodFile) (table:RawFile) =
                table.MethodFile <- methodFile
                table

            ///Replaces externalFormatDocumentation of existing object with new externalFormatDocumentation.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (table:RawFile) =
                table.ExternalFormatDocumentation <- externalFormatDocumentation
                table

            ///Replaces fileFormat of existing object with new fileFormat.
            static member addFileFormat
                (fileFormat:CVParam) (table:RawFile) =
                table.FileFormat <- fileFormat
                table

            ///Adds a identificationFileParam to an existing object.
            static member addDetail (detail:RawFileParam) (table:RawFile) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of identificationFileParams to an existing object.
            static member addDetails (details:seq<RawFileParam>) (table:RawFile) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            /////Replaces mzidentml of existing object with new mzidentml.
            //static member addMzIdentMLDocument
            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
            //    provider

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.RawFile.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (location:string) =
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
               item1.Name=item2.Name && item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation
               && item1.FileFormat=item2.FileFormat && item1.MethodFile=item2.MethodFile
               && item1.Details=item2.Details

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:RawFile) =
                    RawFileHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match RawFileHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:RawFile) =
                RawFileHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type RawFilesGroupHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    rawFiles                     : seq<RawFile>,
                    ?id                          : string, 
                    ?details                     : seq<RawFilesGroupParam>
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let details'                     = convertOptionToList details
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new RawFilesGroup(
                                  id', 
                                  rawFiles |> List, 
                                  details',
                                  //mzIdentML', 
                                  Nullable(DateTime.Now)
                                 )

            ///Adds a identificationFileParam to an existing object.
            static member addRawFile (detail:RawFile) (table:RawFilesGroup) =
                let result = table.RawFiles <- addToList table.RawFiles detail
                table

            ///Adds a collection of identificationFileParams to an existing object.
            static member addRawFiles (details:seq<RawFile>) (table:RawFilesGroup) =
                let result = table.RawFiles <- addCollectionToList table.RawFiles details
                table
            
            ///Adds a identificationFileParam to an existing object.
            static member addDetail (detail:RawFilesGroupParam) (table:RawFilesGroup) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of identificationFileParams to an existing object.
            static member addDetails (details:seq<RawFilesGroupParam>) (table:RawFilesGroup) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            /////Replaces mzidentml of existing object with new mzidentml.
            //static member addMzIdentMLDocument
            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
            //    provider

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.RawFile.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByRawFiles (dbContext:MzQuantML) (rawFiles:seq<RawFile>) =
                query {
                       for i in dbContext.RawFilesGroup.Local do
                           if i.RawFiles=(rawFiles |> List)
                              then select (i, i.RawFiles, i.Details)
                      }
                |> Seq.map (fun (rawFilesGroup, _, _) -> rawFilesGroup)
                |> (fun rawFilesGroup -> 
                    if (Seq.exists (fun rawFilesGroup' -> rawFilesGroup' <> null) rawFilesGroup) = false
                        then 
                            query {
                                   for i in dbContext.RawFilesGroup do
                                       if i.RawFiles=(rawFiles |> List)
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
                item1.Details=item2.Details

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:RawFilesGroup) =
                    RawFilesGroupHandler.tryFindByRawFiles dbContext item.RawFiles
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match RawFilesGroupHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:RawFilesGroup) =
                RawFilesGroupHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type InputFilesHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    ?id                          : string,
                    ?rawFilesGroups              : seq<RawFilesGroup>,
                    ?methodFiles                 : seq<MethodFile>,
                    ?identificationFiles         : seq<IdentificationFile>,
                    ?searchDatabases             : seq<SearchDatabase>,
                    ?sourceFiles                 : seq<SourceFile>
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let rawFilesGroups'              = convertOptionToList rawFilesGroups
                let methodFiles'                 = convertOptionToList methodFiles
                let identificationFiles'         = convertOptionToList identificationFiles
                let searchDatabases'             = convertOptionToList searchDatabases
                let sourceFiles'                 = convertOptionToList sourceFiles
                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new InputFiles(
                               id', 
                               rawFilesGroups', 
                               methodFiles',
                               identificationFiles',
                               searchDatabases',
                               sourceFiles',
                               //mzIdentML', 
                               Nullable(DateTime.Now)
                              )

            ///Adds a identificationFileParam to an existing object.
            static member addRawFilesGroup (detail:RawFilesGroup) (table:InputFiles) =
                let result = table.RawFilesGroups <- addToList table.RawFilesGroups detail
                table

            ///Adds a collection of identificationFileParams to an existing object.
            static member addRawFilesGroups (details:seq<RawFilesGroup>) (table:InputFiles) =
                let result = table.RawFilesGroups <- addCollectionToList table.RawFilesGroups details
                table
            
            ///Adds a identificationFileParam to an existing object.
            static member addMethodFile (detail:MethodFile) (table:InputFiles) =
                let result = table.MethodFiles <- addToList table.MethodFiles detail
                table

            ///Adds a collection of identificationFileParams to an existing object.
            static member addMethodFiles (details:seq<MethodFile>) (table:InputFiles) =
                let result = table.MethodFiles <- addCollectionToList table.MethodFiles details
                table
            
            ///Adds a identificationFileParam to an existing object.
            static member addIdentificationFile (detail:IdentificationFile) (table:InputFiles) =
                let result = table.IdentificationFiles <- addToList table.IdentificationFiles detail
                table

            ///Adds a collection of identificationFileParams to an existing object.
            static member addIdentificationFiles (details:seq<IdentificationFile>) (table:InputFiles) =
                let result = table.IdentificationFiles <- addCollectionToList table.IdentificationFiles details
                table
            
            ///Adds a identificationFileParam to an existing object.
            static member addSearchDatabase (detail:SearchDatabase) (table:InputFiles) =
                let result = table.SearchDatabases <- addToList table.SearchDatabases detail
                table

            ///Adds a collection of identificationFileParams to an existing object.
            static member addDetails (details:seq<SearchDatabase>) (table:InputFiles) =
                let result = table.SearchDatabases <- addCollectionToList table.SearchDatabases details
                table
            
            ///Adds a identificationFileParam to an existing object.
            static member addSourceFile (detail:SourceFile) (table:InputFiles) =
                let result = table.SourceFiles <- addToList table.SourceFiles detail
                table

            ///Adds a collection of identificationFileParams to an existing object.
            static member addSourceFiles (details:seq<SourceFile>) (table:InputFiles) =
                let result = table.SourceFiles <- addCollectionToList table.SourceFiles details
                table

            /////Replaces mzidentml of existing object with new mzidentml.
            //static member addMzIdentMLDocument
            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
            //    provider

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.RawFile.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
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
                    InputFilesHandler.tryFindByRawFilesGroups dbContext item.RawFilesGroups
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match InputFilesHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:InputFiles) =
                InputFilesHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type ModificationHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    detail                     : CVParam,
                    ?id                        : string,
                    ?massDelta                 : float,
                    ?residues                  : string
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let massDelta'                   = defaultArg massDelta Unchecked.defaultof<float>
                let residues'                    = defaultArg residues Unchecked.defaultof<string>
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new Modification(
                                 id', 
                                 Nullable(massDelta'),
                                 residues',
                                 detail,
                                 //mzIdentML', 
                                 Nullable(DateTime.Now)
                                )

            ///Replaces name of existing object with new name.
            static member addMassDelta
                (massDelta:float) (table:Modification) =
                table.MassDelta <- Nullable(massDelta)
                table

            ///Replaces name of existing object with new name.
            static member addResidues
                (residues:string) (table:Modification) =
                table.Residues <- residues
                table

            /////Replaces mzidentml of existing object with new mzidentml.
            //static member addMzIdentMLDocument
            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
            //    provider

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.Modification.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (detail:CVParam) =
                query {
                       for i in dbContext.Modification.Local do
                           if i.Detail=detail
                              then select (i, i.Detail)
                      }
                |> Seq.map (fun (modification, _) -> modification)
                |> (fun modification -> 
                    if (Seq.exists (fun modification' -> modification' <> null) modification) = false
                        then 
                            query {
                                   for i in dbContext.Modification do
                                       if i.Detail=detail
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
                item1.MassDelta=item2.MassDelta && item1.Residues=item2.Residues

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Modification) =
                    ModificationHandler.tryFindByName dbContext item.Detail
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ModificationHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Modification) =
                ModificationHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type AssayHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    ?id                          : string,
                    ?name                        : string,
                    ?rawFilesGroup               : RawFilesGroup,
                    ?label                       : seq<Modification>,
                    ?identificationFile          : IdentificationFile,
                    ?details                     : seq<AssayParam>
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let rawFilesGroup'               = defaultArg rawFilesGroup Unchecked.defaultof<RawFilesGroup>
                let label'                       = convertOptionToList label
                let identificationFile'          = defaultArg identificationFile Unchecked.defaultof<IdentificationFile>
                let details'                     = convertOptionToList details
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new Assay(
                          id', 
                          name', 
                          rawFilesGroup', 
                          label', 
                          identificationFile', 
                          details',
                          Nullable(DateTime.Now)
                         )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (table:Assay) =
                table.Name <- name
                table
            
            ///Replaces rawFilesGroup of existing object with new one.
            static member addRawFilesGroup
                (rawFilesGroup:RawFilesGroup) (table:Assay) =
                table.RawFilesGroup <- rawFilesGroup
                table

            ///Replaces identificationFile of existing object with new one.
            static member addIdentificationFile
                (identificationFile:IdentificationFile) (table:Assay) =
                table.IdentificationFile <- identificationFile
                table

            ///Adds a modification to an existing object.
            static member addLabel (modification:Modification) (table:Assay) =
                let result = table.Label <- addToList table.Label modification
                table

            ///Adds a collection of modifications to an existing object.
            static member addLabels (modifications:seq<Modification>) (table:Assay) =
                let result = table.Label <- addCollectionToList table.Label modifications
                table

            ///Adds a identificationFileParam to an existing object.
            static member addDetail (detail:AssayParam) (table:Assay) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of identificationFileParams to an existing object.
            static member addDetails (details:seq<AssayParam>) (table:Assay) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            /////Replaces mzidentml of existing object with new mzidentml.
            //static member addMzIdentMLDocument
            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
            //    provider

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.Assay.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
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
                item1.RawFilesGroup=item2.RawFilesGroup && item1.IdentificationFile=item2.IdentificationFile 
                && item1.Label=item2.Label && item1.Details=item2.Details 

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Assay) =
                    AssayHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match AssayHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Assay) =
                AssayHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type StudyVariableHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    assays                       : seq<Assay>,
                    ?id                          : string,
                    ?name                        : string,
                    ?details                     : seq<StudyVariableParam>
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let details'                     = convertOptionToList details
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new StudyVariable(
                                  id', 
                                  name',
                                  assays |> List,
                                  details',
                                  Nullable(DateTime.Now)
                                 )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (table:StudyVariable) =
                table.Name <- name
                table

            ///Adds a identificationFileParam to an existing object.
            static member addDetail (detail:StudyVariableParam) (table:StudyVariable) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of identificationFileParams to an existing object.
            static member addDetails (details:seq<StudyVariableParam>) (table:StudyVariable) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            /////Replaces mzidentml of existing object with new mzidentml.
            //static member addMzIdentMLDocument
            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
            //    provider

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.StudyVariable.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
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
                item1.Assays=item2.Assays && item1.Details=item2.Details 

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:StudyVariable) =
                    StudyVariableHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match StudyVariableHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:StudyVariable) =
                StudyVariableHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type RatioHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    numerator                    : StudyVariable,
                    denominator                  : StudyVariable,
                    numeratorDatatype            : CVParam,
                    denominatorDatatype          : CVParam,
                    ?id                          : string,
                    ?name                        : string,
                    ?ratioCalculation            : seq<RatioCalculationParam>
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let ratioCalculation'            = convertOptionToList ratioCalculation
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new Ratio(
                          id', 
                          name',
                          numerator,
                          denominator,
                          ratioCalculation',
                          numeratorDatatype,
                          denominatorDatatype,
                          Nullable(DateTime.Now)
                         )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (table:Ratio) =
                table.Name <- name
                table

            ///Adds a identificationFileParam to an existing object.
            static member addRatioCalculationParam (ratioCalculationParam:RatioCalculationParam) (table:Ratio) =
                let result = table.RatioCalculation <- addToList table.RatioCalculation ratioCalculationParam
                table

            ///Adds a collection of identificationFileParams to an existing object.
            static member addRatioCalculationParams (ratioCalculationParams:seq<RatioCalculationParam>) (table:Ratio) =
                let result = table.RatioCalculation <- addCollectionToList table.RatioCalculation ratioCalculationParams
                table

            /////Replaces mzidentml of existing object with new mzidentml.
            //static member addMzIdentMLDocument
            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
            //    provider

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.Ratio.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzQuantML) (name:string) =
                query {
                       for i in dbContext.Ratio.Local do
                           if i.Name=name
                              then select (i, i.Numerator, i.Denominator, i.NumeratorDataType, i.DenominatorDataType, i.RatioCalculation)
                      }
                |> Seq.map (fun (ratio, _, _ ,_ , _, _) -> ratio)
                |> (fun ratio -> 
                    if (Seq.exists (fun ratio' -> ratio' <> null) ratio) = false
                        then 
                            query {
                                   for i in dbContext.Ratio do
                                       if i.Name=name
                                          then select (i, i.Numerator, i.Denominator, i.NumeratorDataType, i.DenominatorDataType, i.RatioCalculation)
                                  }
                            |> Seq.map (fun (ratio, _, _ ,_ , _, _) -> ratio)
                            |> (fun ratio -> if (Seq.exists (fun ratio' -> ratio' <> null) ratio) = false
                                                            then None
                                                            else Some ratio
                               )
                        else Some ratio
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Ratio) (item2:Ratio) =
                item1.Numerator=item2.Numerator && item1.Denominator=item2.Denominator && 
                item1.NumeratorDataType=item2.NumeratorDataType && item1.RatioCalculation=item2.RatioCalculation && 
                item1.DenominatorDataType=item2.DenominatorDataType              

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Ratio) =
                    RatioHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match RatioHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Ratio) =
                RatioHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type ColumnHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    index                        : int,
                    datatype                     : CVParam,
                    ?id                          : string
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new Column(
                           id',
                           Nullable(index),
                           datatype,
                           Nullable(DateTime.Now)
                          )

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.Column.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
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
                item1.DataType=item2.DataType

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:Column) =
                    ColumnHandler.tryFindByIndex dbContext item.Index
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ColumnHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:Column) =
                ColumnHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type DataMatrixHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    row                          : string,
                    ?id                          : string
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new DataMatrix(
                               id',
                               row,
                               Nullable(DateTime.Now)
                              )

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.DataMatrix.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
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
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:DataMatrix) =
                DataMatrixHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type AssayQuantLayerHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    dataType                     : CVParam,
                    columnIndex                  : string,
                    dataMatrix                   : DataMatrix,
                    ?id                          : string
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new AssayQuantLayer(
                                    id',
                                    dataType,
                                    columnIndex,
                                    dataMatrix,
                                    Nullable(DateTime.Now)
                                   )

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.AssayQuantLayer.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
                query {
                       for i in dbContext.AssayQuantLayer.Local do
                           if i.ColumnIndex=columnIndex
                              then select (i, i.DataType, i.DataMatrix)
                      }
                |> Seq.map (fun (assayQuantLayer, _, _) -> assayQuantLayer)
                |> (fun assayQuantLayer -> 
                    if (Seq.exists (fun assayQuantLayer' -> assayQuantLayer' <> null) assayQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.AssayQuantLayer do
                                       if i.ColumnIndex=columnIndex
                                          then select (i, i.DataType, i.DataMatrix)
                                  }
                            |> Seq.map (fun (assayQuantLayer, _, _) -> assayQuantLayer)
                            |> (fun assayQuantLayer -> if (Seq.exists (fun assayQuantLayer' -> assayQuantLayer' <> null) assayQuantLayer) = false
                                                            then None
                                                            else Some assayQuantLayer
                               )
                        else Some assayQuantLayer
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:AssayQuantLayer) (item2:AssayQuantLayer) =
                item1.DataType=item2.DataType && item1.DataMatrix=item2.DataMatrix

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:AssayQuantLayer) =
                    AssayQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match AssayQuantLayerHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:AssayQuantLayer) =
                AssayQuantLayerHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type GlobalQuantLayerHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    columns                      : seq<Column>,
                    dataMatrix                   : DataMatrix,
                    ?id                          : string
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new GlobalQuantLayer(
                                     id',
                                     columns |> List,
                                     dataMatrix,
                                     Nullable(DateTime.Now)
                                    )

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.GlobalQuantLayer.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByDataMatrix (dbContext:MzQuantML) (dataMatrix:DataMatrix) =
                query {
                       for i in dbContext.GlobalQuantLayer.Local do
                           if i.DataMatrix=dataMatrix
                              then select (i, i.Columns, i.DataMatrix)
                      }
                |> Seq.map (fun (globalQuantLayer, _, _) -> globalQuantLayer)
                |> (fun globalQuantLayer -> 
                    if (Seq.exists (fun globalQuantLayer' -> globalQuantLayer' <> null) globalQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.GlobalQuantLayer do
                                       if i.DataMatrix=dataMatrix
                                          then select (i, i.Columns, i.DataMatrix)
                                  }
                            |> Seq.map (fun (globalQuantLayer, _, _) -> globalQuantLayer)
                            |> (fun globalQuantLayer -> if (Seq.exists (fun globalQuantLayer' -> globalQuantLayer' <> null) globalQuantLayer) = false
                                                            then None
                                                            else Some globalQuantLayer
                               )
                        else Some globalQuantLayer
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:GlobalQuantLayer) (item2:GlobalQuantLayer) =
                item1.Columns=item2.Columns

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:GlobalQuantLayer) =
                    GlobalQuantLayerHandler.tryFindByDataMatrix dbContext item.DataMatrix
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match GlobalQuantLayerHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:GlobalQuantLayer) =
                GlobalQuantLayerHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type MS2AssayQuantLayerHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    dataType                     : CVParam,
                    columnIndex                  : string,
                    dataMatrix                   : DataMatrix,
                    ?id                          : string
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new MS2AssayQuantLayer(
                                       id',
                                       dataType,
                                       columnIndex,
                                       dataMatrix,
                                       Nullable(DateTime.Now)
                                      )

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.MS2AssayQuantLayer.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
                query {
                       for i in dbContext.MS2AssayQuantLayer.Local do
                           if i.ColumnIndex=columnIndex
                              then select (i, i.DataType, i.DataMatrix)
                      }
                |> Seq.map (fun (ms2AssayQuantLayer, _, _) -> ms2AssayQuantLayer)
                |> (fun ms2AssayQuantLayer -> 
                    if (Seq.exists (fun ms2AssayQuantLayer' -> ms2AssayQuantLayer' <> null) ms2AssayQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.MS2AssayQuantLayer do
                                       if i.ColumnIndex=columnIndex
                                          then select (i, i.DataType, i.DataMatrix)
                                  }
                            |> Seq.map (fun (ms2AssayQuantLayer, _, _) -> ms2AssayQuantLayer)
                            |> (fun ms2AssayQuantLayer -> if (Seq.exists (fun ms2AssayQuantLayer' -> ms2AssayQuantLayer' <> null) ms2AssayQuantLayer) = false
                                                            then None
                                                            else Some ms2AssayQuantLayer
                               )
                        else Some ms2AssayQuantLayer
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:MS2AssayQuantLayer) (item2:MS2AssayQuantLayer) =
                item1.DataType=item2.DataType && item1.DataMatrix=item2.DataMatrix

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:MS2AssayQuantLayer) =
                    MS2AssayQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match MS2AssayQuantLayerHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:MS2AssayQuantLayer) =
                MS2AssayQuantLayerHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type StudyVariableQuantLayerHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    dataType                     : CVParam,
                    columnIndex                  : string,
                    dataMatrix                   : DataMatrix,
                    ?id                          : string
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new StudyVariableQuantLayer(
                                            id',
                                            dataType,
                                            columnIndex,
                                            dataMatrix,
                                            Nullable(DateTime.Now)
                                           )

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.StudyVariableQuantLayer.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
                query {
                       for i in dbContext.StudyVariableQuantLayer.Local do
                           if i.ColumnIndex=columnIndex
                              then select (i, i.DataType, i.DataMatrix)
                      }
                |> Seq.map (fun (studyVariableQuantLayer, _, _) -> studyVariableQuantLayer)
                |> (fun studyVariableQuantLayer -> 
                    if (Seq.exists (fun studyVariableQuantLayer' -> studyVariableQuantLayer' <> null) studyVariableQuantLayer) = false
                        then 
                            query {
                                   for i in dbContext.StudyVariableQuantLayer do
                                       if i.ColumnIndex=columnIndex
                                          then select (i, i.DataType, i.DataMatrix)
                                  }
                            |> Seq.map (fun (studyVariableQuantLayer, _, _) -> studyVariableQuantLayer)
                            |> (fun studyVariableQuantLayer -> if (Seq.exists (fun studyVariableQuantLayer' -> studyVariableQuantLayer' <> null) studyVariableQuantLayer) = false
                                                                    then None
                                                                    else Some studyVariableQuantLayer
                               )
                        else Some studyVariableQuantLayer
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:StudyVariableQuantLayer) (item2:StudyVariableQuantLayer) =
                item1.DataType=item2.DataType && item1.DataMatrix=item2.DataMatrix

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:StudyVariableQuantLayer) =
                    StudyVariableQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match StudyVariableQuantLayerHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:StudyVariableQuantLayer) =
                StudyVariableQuantLayerHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type RatioQuantLayerHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    columnIndex                  : string,
                    dataMatrix                   : DataMatrix,
                    ?id                          : string
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new RatioQuantLayer(
                                    id',
                                    columnIndex,
                                    dataMatrix,
                                    Nullable(DateTime.Now)
                                   )

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.RatioQuantLayer.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
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
                item1.DataMatrix=item2.DataMatrix

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:RatioQuantLayer) =
                    RatioQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match RatioQuantLayerHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:RatioQuantLayer) =
                RatioQuantLayerHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type ProcessingMethodHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    order                        : int,
                    ?id                          : string,
                    ?details                     : seq<ProcessingMethodParam>
                    //?mzIdentML                   : MzIdentMLDocument
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let details'                     = convertOptionToList details
                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new ProcessingMethod(
                                     id', 
                                     Nullable(order),
                                     details',
                                     Nullable(DateTime.Now)
                                    )

            ///Adds a identificationFileParam to an existing object.
            static member addDetail (processingMethodParam:ProcessingMethodParam) (table:ProcessingMethod) =
                let result = table.Details <- addToList table.Details processingMethodParam
                table

            ///Adds a collection of identificationFileParams to an existing object.
            static member addDetails (processingMethodParams:seq<ProcessingMethodParam>) (table:ProcessingMethod) =
                let result = table.Details <- addCollectionToList table.Details processingMethodParams
                table

            /////Replaces mzidentml of existing object with new mzidentml.
            //static member addMzIdentMLDocument
            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
            //    provider

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzQuantML) (id:string) =
                tryFind (context.ProcessingMethod.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
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
                item1.Details=item2.Details            

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzQuantML) (item:ProcessingMethod) =
                    ProcessingMethodHandler.tryFindByOrder dbContext item.Order
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProcessingMethodHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProcessingMethod) =
                ProcessingMethodHandler.addToContext dbContext item
                dbContext.SaveChanges()

        //Go on with DataProcessing