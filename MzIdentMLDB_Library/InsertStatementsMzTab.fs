namespace MzTabDataBase

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
                    new DbContextOptionsBuilder<MzTab>()
                optionsBuilder.UseSqlite(@"Data Source=" + path) |> ignore
                new MzTab(optionsBuilder.Options)       

            ///Creats connection for SQL-context and database.
            static member sqlConnection() =
                let optionsBuilder = 
                    new DbContextOptionsBuilder<MzTab>()
                optionsBuilder.UseSqlServer("Server=(localdb)\mssqllocaldb;Database=MyDatabase;Trusted_Connection=True;") |> ignore
                new MzTab(optionsBuilder.Options) 

            ///Reads obo-file and creates sequence of Obo.Terms.
            static member fromFileObo (filePath:string) =
                FileIO.readFile filePath
                |> Obo.parseOboTerms

            ///Tries to add the object to the database-context.
            static member tryAddToContext (context:MzTab) (item:'b) =
                context.Add(item)

            ///Tries to add the object to the database-context and insert it in the database.
            static member tryAddToContextAndInsert (context:MzTab) (item:'b) =
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
                (context:MzTab) (id:string) =
                tryFind (context.Term.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzTab) (name:string) =
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
            static member addToContext (dbContext:MzTab) (item:Term) =
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
            static member addToContextAndInsert (dbContext:MzTab) (item:Term) =
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
                (context:MzTab) (id:string) =
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

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (table:CVParam) =
                table.Value <- value
                table

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:CVParam) =
                table.Unit <- unit
                table

            ///Tries to find a cvparam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.CVParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
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
            static member addToContext (dbContext:MzTab) (item:CVParam) =
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
            static member addToContextAndInsert (dbContext:MzTab) (item:CVParam) =
                CVParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

//        type OrganizationParamHandler =
//            ///Initializes a organizationparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>

//                new OrganizationParam(
//                                      id', 
//                                      value', 
//                                      term, 
//                                      unit', 
//                                      Nullable(DateTime.Now)
//                                     )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (table:OrganizationParam) =
//                table.Value <- value
//                table

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:OrganizationParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzTab) (id:string) =
//                tryFind (context.OrganizationParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzTab) (name:string) =
//                query {
//                       for i in dbContext.OrganizationParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.OrganizationParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:OrganizationParam) (item2:OrganizationParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:OrganizationParam) =
//                    OrganizationParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> 
//                                                                match OrganizationParamHandler.hasEqualFieldValues cvParam item with
//                                                                |true -> null
//                                                                |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:OrganizationParam) =
//                OrganizationParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type PersonParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new PersonParam(
//                                id', 
//                                value', 
//                                term, 
//                                unit', 
//                                Nullable(DateTime.Now)
//                               )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (table:PersonParam) =
//                table.Value <- value
//                table

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (param:PersonParam) =
//                param.Unit <- unit
//                param

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.PersonParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.PersonParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.PersonParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:PersonParam) (item2:PersonParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:PersonParam) =
//                    PersonParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match PersonParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:PersonParam) =
//                PersonParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type AnalysisSoftwareParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new AnalysisSoftwareParam(
//                                          id', 
//                                          value', 
//                                          term, 
//                                          unit', 
//                                          Nullable(DateTime.Now)
//                                         )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:AnalysisSoftwareParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:AnalysisSoftwareParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.AnalysisSoftwareParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.AnalysisSoftwareParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.AnalysisSoftwareParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:AnalysisSoftwareParam) (item2:AnalysisSoftwareParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:AnalysisSoftwareParam) =
//                    AnalysisSoftwareParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match AnalysisSoftwareParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:AnalysisSoftwareParam) =
//                AnalysisSoftwareParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type SearchDatabaeParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new SearchDatabaseParam(
//                                        id', 
//                                        value', 
//                                        term, 
//                                        unit', 
//                                        Nullable(DateTime.Now)
//                                       )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:SearchDatabaseParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:SearchDatabaseParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.SearchDatabaseParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.SearchDatabaseParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.SearchDatabaseParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:SearchDatabaseParam) (item2:SearchDatabaseParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:SearchDatabaseParam) =
//                    SearchDatabaeParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match SearchDatabaeParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:SearchDatabaseParam) =
//                SearchDatabaeParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type RawFileParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new RawFileParam(
//                                 id', 
//                                 value', 
//                                 term, 
//                                 unit', 
//                                 Nullable(DateTime.Now)
//                                )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:RawFileParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:RawFileParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.RawFileParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.RawFileParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.RawFileParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:RawFileParam) (item2:RawFileParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:RawFileParam) =
//                    RawFileParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match RawFileParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:RawFileParam) =
//                RawFileParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type AssayParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new AssayParam(
//                               id', 
//                               value', 
//                               term, 
//                               unit', 
//                               Nullable(DateTime.Now)
//                              )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:AssayParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:AssayParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.AssayParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.AssayParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.AssayParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:AssayParam) (item2:AssayParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:AssayParam) =
//                    AssayParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match AssayParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:AssayParam) =
//                AssayParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type StudyVariableParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new StudyVariableParam(
//                                       id', 
//                                       value', 
//                                       term, 
//                                       unit', 
//                                       Nullable(DateTime.Now)
//                                      )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:AssayParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:StudyVariableParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.StudyVariableParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.StudyVariableParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.StudyVariableParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:StudyVariableParam) (item2:StudyVariableParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:StudyVariableParam) =
//                    StudyVariableParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match StudyVariableParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:StudyVariableParam) =
//                StudyVariableParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type RatioCalculationParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new RatioCalculationParam(
//                                          id', 
//                                          value', 
//                                          term, 
//                                          unit', 
//                                          Nullable(DateTime.Now)
//                                         )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:RatioCalculationParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:RatioCalculationParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.RatioCalculationParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.RatioCalculationParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.RatioCalculationParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:RatioCalculationParam) (item2:RatioCalculationParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:RatioCalculationParam) =
//                    RatioCalculationParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match RatioCalculationParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:RatioCalculationParam) =
//                RatioCalculationParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type FeatureParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new FeatureParam(
//                                 id', 
//                                 value', 
//                                 term, 
//                                 unit', 
//                                 Nullable(DateTime.Now)
//                                )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:FeatureParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:FeatureParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.FeatureParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.FeatureParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.FeatureParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:FeatureParam) (item2:FeatureParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:FeatureParam) =
//                    FeatureParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match FeatureParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:FeatureParam) =
//                FeatureParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type SmallMoleculeParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new SmallMoleculeParam(
//                                       id', 
//                                       value', 
//                                       term, 
//                                       unit', 
//                                       Nullable(DateTime.Now)
//                                      )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:SmallMoleculeParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:SmallMoleculeParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.SmallMoleculeParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.SmallMoleculeParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.SmallMoleculeParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:SmallMoleculeParam) (item2:SmallMoleculeParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:SmallMoleculeParam) =
//                    SmallMoleculeParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match SmallMoleculeParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:SmallMoleculeParam) =
//                SmallMoleculeParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type SmallMoleculeListParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new SmallMoleculeListParam(
//                                           id', 
//                                           value', 
//                                           term, 
//                                           unit', 
//                                           Nullable(DateTime.Now)
//                                          )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:SmallMoleculeListParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:SmallMoleculeListParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.SmallMoleculeListParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.SmallMoleculeListParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.SmallMoleculeListParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:SmallMoleculeListParam) (item2:SmallMoleculeListParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:SmallMoleculeListParam) =
//                    SmallMoleculeListParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match SmallMoleculeListParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:SmallMoleculeListParam) =
//                SmallMoleculeListParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type PeptideConsensusParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new PeptideConsensusParam(
//                                          id', 
//                                          value', 
//                                          term, 
//                                          unit', 
//                                          Nullable(DateTime.Now)
//                                         )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:PeptideConsensusParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:PeptideConsensusParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.PeptideConsensusParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.PeptideConsensusParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.PeptideConsensusParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:PeptideConsensusParam) (item2:PeptideConsensusParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:PeptideConsensusParam) =
//                    PeptideConsensusParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match PeptideConsensusParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:PeptideConsensusParam) =
//                PeptideConsensusParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type ProteinParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new ProteinParam(
//                                 id', 
//                                 value', 
//                                 term, 
//                                 unit', 
//                                 Nullable(DateTime.Now)
//                                )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:ProteinParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:ProteinParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.ProteinParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.ProteinParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.ProteinParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:ProteinParam) (item2:ProteinParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:ProteinParam) =
//                    ProteinParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match ProteinParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinParam) =
//                ProteinParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type ProteinListParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new ProteinListParam(
//                                     id', 
//                                     value', 
//                                     term, 
//                                     unit', 
//                                     Nullable(DateTime.Now)
//                                    )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:ProteinListParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:ProteinListParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.ProteinListParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.ProteinListParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.ProteinListParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:ProteinListParam) (item2:ProteinListParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:ProteinListParam) =
//                    ProteinListParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match ProteinListParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinListParam) =
//                ProteinListParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type ProteinGroupParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new ProteinGroupParam(
//                                      id', 
//                                      value', 
//                                      term, 
//                                      unit', 
//                                      Nullable(DateTime.Now)
//                                     )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:ProteinGroupParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:ProteinGroupParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.ProteinGroupParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.ProteinGroupParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.ProteinGroupParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:ProteinGroupParam) (item2:ProteinGroupParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:ProteinGroupParam) =
//                    ProteinGroupParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match ProteinGroupParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinGroupParam) =
//                ProteinGroupParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type ProteinGroupListParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new ProteinGroupListParam(
//                                          id', 
//                                          value', 
//                                          term, 
//                                          unit', 
//                                          Nullable(DateTime.Now)
//                                         )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:ProteinGroupListParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:ProteinGroupListParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.ProteinGroupListParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.ProteinGroupListParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.ProteinGroupListParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:ProteinGroupListParam) (item2:ProteinGroupListParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:ProteinGroupListParam) =
//                    ProteinGroupListParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match ProteinGroupListParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinGroupListParam) =
//                ProteinGroupListParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type PeptideConsensusListParamHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    term      : Term,
//                    ?id       : string,
//                    ?value    : string,
//                    ?unit     : Term
//                ) =
//                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
//                let value'    = defaultArg value Unchecked.defaultof<string>
//                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
//                new PeptideConsensusListParam(
//                                              id', 
//                                              value', 
//                                              term, 
//                                              unit', 
//                                              Nullable(DateTime.Now)
//                                             )

//            ///Replaces value of existing object with new one.
//            static member addValue
//                (value:string) (param:PeptideConsensusListParam) =
//                param.Value <- value
//                param

//            ///Replaces unit of existing object with new one.
//            static member addUnit
//                (unit:Term) (table:PeptideConsensusListParam) =
//                table.Unit <- unit
//                table

//            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.PeptideConsensusListParam.Find(id))

//            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByTermName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.PeptideConsensusListParam.Local do
//                           if i.Term.Name=name
//                              then select (i, i.Term, i.Unit)
//                      }
//                |> Seq.map (fun (param,_ ,_) -> param)
//                |> (fun param -> 
//                    if (Seq.exists (fun param' -> param' <> null) param) = false
//                        then 
//                            query {
//                                   for i in dbContext.PeptideConsensusListParam do
//                                       if i.Term.Name=name
//                                          then select (i, i.Term, i.Unit)
//                                  }
//                            |> Seq.map (fun (param,_ ,_) -> param)
//                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some param
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:PeptideConsensusListParam) (item2:PeptideConsensusListParam) =
//                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:PeptideConsensusListParam) =
//                    PeptideConsensusListParamHandler.tryFindByTermName dbContext item.Term.ID
//                    |> (fun cvParamCollection -> match cvParamCollection with
//                                                 |Some x -> x
//                                                            |> Seq.map (fun cvParam -> match PeptideConsensusListParamHandler.hasEqualFieldValues cvParam item with
//                                                                                       |true -> null
//                                                                                       |false -> dbContext.Add item
//                                                                       ) |> ignore
//                                                 |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:PeptideConsensusListParam) =
//                PeptideConsensusListParamHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

////////////////////////////////////////////
////End of paramHandlers//////////////////////////////////////////////
////////////////////////////////////////////

//        type AnalysisSoftwareHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    id        : string,
//                    version   : string,
//                    ?details  : seq<AnalysisSoftwareParam>
//                ) =

//                let details'  = convertOptionToList details
                    
//                new AnalysisSoftware(
//                                     id, 
//                                     version, 
//                                     details', 
//                                     Nullable(DateTime.Now)
//                                    )

//            ///Adds new enzymename to collection of enzymenames.
//            static member addDetail
//                (analysisSoftwareParam:AnalysisSoftwareParam) (table:AnalysisSoftware) =
//                let result = table.Details <- addToList table.Details analysisSoftwareParam
//                table

//            ///Add new collection of enzymenames to collection of enzymenames.
//            static member addDetails
//                (analysisSoftwareParams:seq<AnalysisSoftwareParam>) (table:AnalysisSoftware) =
//                let result = table.Details <- addCollectionToList table.Details analysisSoftwareParams
//                table

//            ///Tries to find a analysisSoftware-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.AnalysisSoftware.Find(id))

//            ///Tries to find a analysisSoftware-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByVersion (dbContext:MzQuantML) (version:string) =
//                query {
//                       for i in dbContext.AnalysisSoftware.Local do
//                           if i.Version=version
//                              then select (i, i.Details)
//                      }
//                |> Seq.map (fun (analysisSoftware, _) -> analysisSoftware)
//                |> (fun analysisSoftware -> 
//                    if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
//                        then 
//                            query {
//                                   for i in dbContext.AnalysisSoftware do
//                                       if i.Version=version
//                                          then select (i, i.Details)
//                                  }
//                            |> Seq.map (fun (analysisSoftware, _) -> analysisSoftware)
//                            |> (fun analysisSoftware -> if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
//                                                            then None
//                                                            else Some analysisSoftware
//                               )
//                        else Some analysisSoftware
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:AnalysisSoftware) (item2:AnalysisSoftware) =
//                item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:AnalysisSoftware) =
//                    AnalysisSoftwareHandler.tryFindByVersion dbContext item.Version
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match AnalysisSoftwareHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:AnalysisSoftware) =
//                AnalysisSoftwareHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type SourceFileHandler =
//            ///Initializes a personparam-object with at least all necessary parameters.
//            static member init
//                (
//                    id                           : string,
//                    location                     : string,
//                    ?name                        : string,
//                    ?externalFormatDocumentation : string,
//                    ?fileFormat                  : CVParam
//                ) =

//                let name'                         = defaultArg name Unchecked.defaultof<string>
//                let externalFormatDocumentation'  = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
//                let fileFormat'                   = defaultArg fileFormat Unchecked.defaultof<CVParam>
                    
//                new SourceFile(
//                               id,
//                               name',
//                               location, 
//                               externalFormatDocumentation',
//                               fileFormat',
//                               Nullable(DateTime.Now)
//                              )

//            ///Replaces name of existing object with new one.
//            static member addName
//                (name:string) (table:SourceFile) =
//                table.Name <- name
//                table

//            ///Replaces externalFormatDocumentation of existing object with new one.
//            static member addExternalFormatDocumentation
//                (externalFormatDocumentation:string) (table:SourceFile) =
//                table.ExternalFormatDocumentation <- externalFormatDocumentation
//                table

//            ///Replaces fileFormat of existing object with new one.
//            static member addFileFormat
//                (fileFormat:CVParam) (table:SourceFile) =
//                table.FileFormat <- fileFormat
//                table

//            ///Tries to find a analysisSoftware-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.SourceFile.Find(id))

//            ///Tries to find a analysisSoftware-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByVersion (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.SourceFile.Local do
//                           if i.Name=name
//                              then select (i, i.FileFormat)
//                      }
//                |> Seq.map (fun (sourceFile, _) -> sourceFile)
//                |> (fun sourceFile -> 
//                    if (Seq.exists (fun sourceFile' -> sourceFile' <> null) sourceFile) = false
//                        then 
//                            query {
//                                   for i in dbContext.SourceFile do
//                                       if i.Name=name
//                                          then select (i, i.FileFormat)
//                                  }
//                            |> Seq.map (fun (sourceFile, _) -> sourceFile)
//                            |> (fun sourceFile -> if (Seq.exists (fun sourceFile' -> sourceFile' <> null) sourceFile) = false
//                                                            then None
//                                                            else Some sourceFile
//                               )
//                        else Some sourceFile
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:SourceFile) (item2:SourceFile) =
//                item1.FileFormat=item2.FileFormat

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:SourceFile) =
//                    SourceFileHandler.tryFindByVersion dbContext item.Name
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match SourceFileHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:SourceFile) =
//                SourceFileHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type OrganizationHandler =
//            ///Initializes a organization-object with at least all necessary parameters.
//            static member init
//                (
//                    ?id      : string,
//                    ?name    : string,
//                    ?details : seq<OrganizationParam>,
//                    ?parent  : string
//                ) =
//                let id'      = defaultArg id (System.Guid.NewGuid().ToString())
//                let name'    = defaultArg name Unchecked.defaultof<string>
//                let details' = convertOptionToList details
//                let parent'  = defaultArg parent Unchecked.defaultof<string>
                    
//                new Organization(
//                                 id', 
//                                 name', 
//                                 details',  
//                                 parent', 
//                                 Nullable(DateTime.Now)
//                                )

//            ///Replaces name of existing object with new one.
//            static member addName
//                (name:string) (table:Organization) =
//                table.Name <- name
//                table

//            ///Replaces parent of existing object with new one.
//            static member addParent
//                (parent:string) (table:Organization) =
//                table.Parent <- parent
//                table

//            ///Adds a organizationparam to an existing organization-object.
//            static member addDetail
//                (detail:OrganizationParam) (table:Organization) =
//                let result = table.Details <- addToList table.Details detail
//                table

//            ///Adds a collection of organizationparams to an existing organization-object.
//            static member addDetails
//                (details:seq<OrganizationParam>) (table:Organization) =
//                let result = table.Details <- addCollectionToList table.Details details
//                table

//            ///Tries to find a organization-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.Organization.Find(id))

//            ///Tries to find an organization-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.Organization.Local do
//                           if i.Name = name
//                              then select (i, i.Details)
//                      }
//                |> Seq.map (fun (organization, _ ) -> organization)
//                |> (fun organization -> 
//                    if (Seq.exists (fun organization' -> organization' <> null) organization) = false
//                        then 
//                            query {
//                                   for i in dbContext.Organization do
//                                       if i.Name = name
//                                          then select (i, i.Details)
//                                  }
//                            |> Seq.map (fun (organization, _ ) -> organization)
//                            |> (fun param -> if (Seq.exists (fun organization' -> organization' <> null) organization) = false
//                                                then None
//                                                else Some param
//                               )
//                        else Some organization
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:Organization) (item2:Organization) =
//                item1.Details=item2.Details && item1.Parent=item2.Parent

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:Organization) =
//                    OrganizationHandler.tryFindByName dbContext item.Name
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match OrganizationHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:Organization) =
//                OrganizationHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type PersonHandler =
//        ///Initializes a person-object with at least all necessary parameters.
//            static member init
//                (
//                    ?id             : string,
//                    ?name           : string,
//                    ?firstName      : string,
//                    ?midInitials    : string,
//                    ?lastName       : string,
//                    ?contactDetails : seq<PersonParam>,
//                    ?organizations  : seq<Organization> 
//                ) =
//                let id'             = defaultArg id (System.Guid.NewGuid().ToString())
//                let name'           = defaultArg name Unchecked.defaultof<string>
//                let firstName'      = defaultArg firstName Unchecked.defaultof<string>
//                let midInitials'    = defaultArg midInitials Unchecked.defaultof<string>
//                let lastName'       = defaultArg lastName Unchecked.defaultof<string>
//                let contactDetails' = convertOptionToList contactDetails
//                let organizations'  = convertOptionToList organizations
                    
//                new Person(
//                           id', 
//                           name', 
//                           firstName',  
//                           midInitials', 
//                           lastName', 
//                           organizations',
//                           contactDetails', 
//                           Nullable(DateTime.Now)
//                          )

//            ///Replaces name of existing object with new one.
//            static member addName
//                (name:string) (table:Person) =
//                table.Name <- name
//                table

//            ///Replaces firstname of existing object with new one.
//            static member addFirstName
//                (firstName:string) (table:Person) =
//                table.FirstName <- firstName
//                table

//            ///Replaces midinitials of existing object with new one.
//            static member addMidInitials
//                (midInitials:string) (table:Person) =
//                table.MidInitials <- midInitials
//                table

//            ///Replaces lastname of existing object with new one.
//            static member addLastName
//                (lastName:string) (table:Person) =
//                table.LastName <- lastName
//                table

//            ///Adds a personparam to an existing person-object.
//            static member addDetail (detail:PersonParam) (table:Person) =
//                let result = table.Details <- addToList table.Details detail
//                table

//            ///Adds a collection of personparams to an existing person-object.
//            static member addDetails
//                (details:seq<PersonParam>) (table:Person) =
//                let result = table.Details <- addCollectionToList table.Details details
//                table

//            ///Adds a organization to an existing person-object.
//            static member addOrganization
//                (organization:Organization) (table:Person) =
//                let result = table.Organizations <- addToList table.Organizations organization
//                table

//            ///Adds a collection of organizations to an existing person-object.
//            static member addOrganizations
//                (organizations:seq<Organization>) (table:Person) =
//                let result = table.Organizations <- addCollectionToList table.Organizations organizations
//                table

//            ///Tries to find a person-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.Person.Find(id))

//            ///Tries to find a person-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.Person.Local do
//                           if i.Name = name
//                              then select (i, i.Details, i.Organizations)
//                      }
//                |> Seq.map (fun (person, _, _) -> person)
//                |> (fun person -> 
//                    if (Seq.exists (fun person' -> person' <> null) person) = false
//                        then 
//                            query {
//                                   for i in dbContext.Person do
//                                       if i.Name = name
//                                          then select (i, i.Details, i.Organizations)
//                                  }
//                            |> Seq.map (fun (person, _, _) -> person)
//                            |> (fun person -> if (Seq.exists (fun person' -> person' <> null) person) = false
//                                                then None
//                                                else Some person
//                               )
//                        else Some person
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:Person) (item2:Person) =
//                item1.FirstName=item2.FirstName && item1.FirstName=item2.FirstName && 
//                item1.MidInitials=item2.MidInitials && item1.LastName=item2.LastName && 
//                item1.Organizations=item2.Organizations && item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:Person) =
//                    PersonHandler.tryFindByName dbContext item.Name
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match PersonHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:Person) =
//                PersonHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type ContactRoleHandler =
//            ///Initializes a contactrole-object with at least all necessary parameters.
//            static member init
//                (   
//                    person : Person, 
//                    role   : CVParam,
//                    ?id    : string
//                ) =
//                let id' = defaultArg id (System.Guid.NewGuid().ToString())
                    
//                new ContactRole(
//                                id', 
//                                person, 
//                                role, 
//                                Nullable(DateTime.Now)
//                               )

//            ///Tries to find a contactRole-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.ContactRole.Find(id))

//            ///Tries to find a contactRole-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByPersonName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.ContactRole.Local do
//                           if i.Person.Name = name
//                              then select (i, i.Role)
//                      }
//                |> Seq.map (fun (contactRole, _) -> contactRole)
//                |> (fun contactRole -> 
//                    if (Seq.exists (fun contactRole' -> contactRole' <> null) contactRole) = false
//                        then 
//                            query {
//                                   for i in dbContext.ContactRole do
//                                       if i.Person.Name = name
//                                          then select (i, i.Role)
//                                  }
//                            |> Seq.map (fun (contactRole, _) -> contactRole)
//                            |> (fun contactRole -> if (Seq.exists (fun contactRole' -> contactRole' <> null) contactRole) = false
//                                                       then None
//                                                       else Some contactRole
//                               )
//                        else Some contactRole
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:ContactRole) (item2:ContactRole) =
//                item1.Role=item2.Role

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:ContactRole) =
//                    ContactRoleHandler.tryFindByPersonName dbContext item.Person.Name
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match ContactRoleHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:ContactRole) =
//                ContactRoleHandler.addToContext dbContext item |> ignore
//                dbContext.SaveChanges()

//        type ProviderHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    ?id               : string,
//                    ?name             : string,
//                    ?analysisSoftware : AnalysisSoftware,
//                    ?contactRole      : ContactRole
//                    //?mzIdentML        : MzIdentMLDocument
//                ) =
//                let id'               = defaultArg id (System.Guid.NewGuid().ToString())
//                let name'             = defaultArg name Unchecked.defaultof<string>
//                let analysisSoftware' = defaultArg analysisSoftware Unchecked.defaultof<AnalysisSoftware>
//                let contactRole'      = defaultArg contactRole Unchecked.defaultof<ContactRole>
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new Provider(
//                             id', 
//                             name', 
//                             analysisSoftware', 
//                             contactRole', 
//                             //mzIdentML', 
//                             Nullable(DateTime.Now)
//                            )

//            ///Replaces name of existing object with new one.
//            static member addName
//                (name:string) (table:Provider) =
//                table.Name <- name
//                table

//            ///Replaces analysissoftware of existing object with new one.
//            static member addAnalysisSoftware
//                (analysisSoftware:AnalysisSoftware) (table:Provider) =
//                table.AnalysisSoftware <- analysisSoftware
//                table

//            ///Replaces contactrole of existing object with new one.
//            static member addContactRole
//                (contactRole:ContactRole) (table:Provider) =
//                table.ContactRole <- contactRole
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a provider-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.Provider.Find(id))

//            ///Tries to find a provider-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.Provider.Local do
//                           if i.Name=name
//                              then select (i, i.AnalysisSoftware, i.ContactRole(*, i.MzIdentMLDocument*))
//                      }
//                |> Seq.map (fun (provider, _, _) -> provider)
//                |> (fun provider -> 
//                    if (Seq.exists (fun provider' -> provider' <> null) provider) = false
//                        then 
//                            query {
//                                   for i in dbContext.Provider do
//                                       if i.Name=name
//                                          then select (i, i.AnalysisSoftware, i.ContactRole(*, i.MzIdentMLDocument*))
//                                  }
//                            |> Seq.map (fun (provider, _, _) -> provider)
//                            |> (fun provider -> if (Seq.exists (fun provider' -> provider' <> null) provider) = false
//                                                    then None
//                                                    else Some provider
//                               )
//                        else Some provider
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:Provider) (item2:Provider) =
//               item1.AnalysisSoftware=item2.AnalysisSoftware && item1.ContactRole=item2.ContactRole 
//               //&& item1.MzIdentMLDocument=item2.MzIdentMLDocument 

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:Provider) =
//                    ProviderHandler.tryFindByName dbContext item.Name
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match ProviderHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:Provider) =
//                ProviderHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type SearchDatabaseHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    location                     : string,
//                    databaseName                 : CVParam,
//                    ?id                          : string,
//                    ?name                        : string,
//                    ?numDatabaseEntries          : int,
//                    ?releaseDate                 : DateTime,
//                    ?version                     : string,
//                    ?externalFormatDocumentation : string,
//                    ?fileFormat                  : CVParam,
//                    ?details                     : seq<SearchDatabaseParam>
//                    //?mzIdentML        : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let name'                        = defaultArg name Unchecked.defaultof<string>
//                let numDatabaseEntries'          = defaultArg numDatabaseEntries Unchecked.defaultof<int>
//                let releaseDate'                 = defaultArg releaseDate Unchecked.defaultof<DateTime>
//                let version'                     = defaultArg version Unchecked.defaultof<string>
//                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
//                let fileFormat'                  = defaultArg fileFormat Unchecked.defaultof<CVParam>
//                let details'                     = convertOptionToList details
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new SearchDatabase(
//                                   id', 
//                                   name', 
//                                   location, 
//                                   Nullable(numDatabaseEntries'),
//                                   Nullable(releaseDate'),
//                                   version',
//                                   externalFormatDocumentation',
//                                   fileFormat',
//                                   databaseName,
//                                   details',
//                                   //mzIdentML', 
//                                   Nullable(DateTime.Now)
//                                  )

//            ///Replaces name of existing object with new one.
//            static member addName
//                (name:string) (table:SearchDatabase) =
//                table.Name <- name
//                table

//            ///Replaces numDatabaseEntries of existing object with new one.
//            static member addNumDatabaseEntries
//                (numDatabaseEntries:int) (table:SearchDatabase) =
//                table.NumDatabaseEntries <- Nullable(numDatabaseEntries)
//                table

//            ///Replaces releaseDate of existing object with new one.
//            static member addReleaseDate
//                (releaseDate:DateTime) (table:SearchDatabase) =
//                table.ReleaseDate <- Nullable(releaseDate)
//                table

//            ///Replaces version of existing object with new one.
//            static member addVersion
//                (version:string) (table:SearchDatabase) =
//                table.Version <- version
//                table

//            ///Replaces version of existing object with new one.
//            static member addExternalFormatDocumentation
//                (externalFormatDocumentation:string) (table:SearchDatabase) =
//                table.Version <- externalFormatDocumentation
//                table

//            ///Replaces fileFormat of existing object with new one.
//            static member addFileFormat
//                (fileFormat:CVParam) (table:SearchDatabase) =
//                table.FileFormat <- fileFormat
//                table
            
//            ///Replaces databaseName of existing object with new one.
//            static member addDatabaseName
//                (databaseName:CVParam) (table:SearchDatabase) =
//                table.DatabaseName <- databaseName
//                table

//            ///Adds a searchDatabaseParam to an existing object.
//            static member addDetail (detail:SearchDatabaseParam) (table:SearchDatabase) =
//                let result = table.Details <- addToList table.Details detail
//                table

//            ///Adds a collection of searchDatabaseParams to an existing object.
//            static member addDetails
//                (details:seq<SearchDatabaseParam>) (table:SearchDatabase) =
//                let result = table.Details <- addCollectionToList table.Details details
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a searchDatabase-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.SearchDatabase.Find(id))

//            ///Tries to find a searchDatabase-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByLocation (dbContext:MzQuantML) (location:string) =
//                query {
//                       for i in dbContext.SearchDatabase.Local do
//                           if i.Location=location
//                              then select (i, i.FileFormat, i.DatabaseName, i.ReleaseDate, i.Details)
//                      }
//                |> Seq.map (fun (searchDatabase, _, _, _, _) -> searchDatabase)
//                |> (fun searchDatabase -> 
//                    if (Seq.exists (fun searchDatabase' -> searchDatabase' <> null) searchDatabase) = false
//                        then 
//                            query {
//                                   for i in dbContext.SearchDatabase do
//                                       if i.Location=location
//                                          then select (i, i.FileFormat, i.DatabaseName, i.ReleaseDate, i.Details)
//                                  }
//                            |> Seq.map (fun (searchDatabase, _, _, _, _) -> searchDatabase)
//                            |> (fun searchDatabase -> if (Seq.exists (fun searchDatabase' -> searchDatabase' <> null) searchDatabase) = false
//                                                        then None
//                                                        else Some searchDatabase
//                               )
//                        else Some searchDatabase
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:SearchDatabase) (item2:SearchDatabase) =
//               item1.Name=item2.Name && item1.NumDatabaseEntries=item2.NumDatabaseEntries &&
//               item1.ReleaseDate=item2.ReleaseDate && item1.Version=item2.Version &&
//               item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation &&
//               item1.FileFormat=item2.FileFormat && item1.DatabaseName=item2.DatabaseName &&
//               item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:SearchDatabase) =
//                    SearchDatabaseHandler.tryFindByLocation dbContext item.Location
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match SearchDatabaseHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:SearchDatabase) =
//                SearchDatabaseHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type IdentificationFileHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    location                     : string,
//                    ?id                          : string,
//                    ?name                        : string,
//                    ?searchDatabase              : SearchDatabase,
//                    ?externalFormatDocumentation : string,
//                    ?fileFormat                  : CVParam,
//                    ?details                     : seq<IdentificationFileParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let name'                        = defaultArg name Unchecked.defaultof<string>
//                let searchDatabase'              = defaultArg searchDatabase Unchecked.defaultof<SearchDatabase>
//                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
//                let fileFormat'                  = defaultArg fileFormat Unchecked.defaultof<CVParam>
//                let details'                     = convertOptionToList details
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new IdentificationFile(
//                                       id', 
//                                       name', 
//                                       location, 
//                                       searchDatabase',
//                                       externalFormatDocumentation',
//                                       fileFormat',
//                                       details',
//                                       //mzIdentML', 
//                                       Nullable(DateTime.Now)
//                                      )

//            ///Replaces name of existing object with new one.
//            static member addName
//                (name:string) (table:IdentificationFile) =
//                table.Name <- name
//                table

//            ///Replaces searchDatabase of existing object with new one.
//            static member addNumSearchDatabase
//                (searchDatabase:SearchDatabase) (table:IdentificationFile) =
//                table.SearchDatabase <- searchDatabase
//                table

//            ///Replaces externalFormatDocumentation of existing object with new one.
//            static member addExternalFormatDocumentation
//                (externalFormatDocumentation:string) (table:IdentificationFile) =
//                table.ExternalFormatDocumentation <- externalFormatDocumentation
//                table

//            ///Replaces fileFormat of existing object with new one.
//            static member addFileFormat
//                (fileFormat:CVParam) (table:IdentificationFile) =
//                table.FileFormat <- fileFormat
//                table

//            ///Adds a identificationFileParam to an existing object.
//            static member addDetail (detail:IdentificationFileParam) (table:IdentificationFile) =
//                let result = table.Details <- addToList table.Details detail
//                table

//            ///Adds a collection of identificationFileParams to an existing object.
//            static member addDetails
//                (details:seq<IdentificationFileParam>) (table:IdentificationFile) =
//                let result = table.Details <- addCollectionToList table.Details details
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a identificationFile-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.IdentificationFile.Find(id))

//            ///Tries to find a identificationFile-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByLocation (dbContext:MzQuantML) (location:string) =
//                query {
//                       for i in dbContext.IdentificationFile.Local do
//                           if i.Location=location
//                              then select (i, i.FileFormat, i.SearchDatabase, i.Details)
//                      }
//                |> Seq.map (fun (identificationFile, _, _, _) -> identificationFile)
//                |> (fun identificationFile -> 
//                    if (Seq.exists (fun identificationFile' -> identificationFile' <> null) identificationFile) = false
//                        then 
//                            query {
//                                   for i in dbContext.IdentificationFile do
//                                       if i.Location=location
//                                          then select (i, i.FileFormat, i.SearchDatabase, i.Details)
//                                  }
//                            |> Seq.map (fun (identificationFile, _, _, _) -> identificationFile)
//                            |> (fun identificationFile -> if (Seq.exists (fun identificationFile' -> identificationFile' <> null) identificationFile) = false
//                                                            then None
//                                                            else Some identificationFile
//                               )
//                        else Some identificationFile
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:IdentificationFile) (item2:IdentificationFile) =
//               item1.Name=item2.Name && item1.SearchDatabase=item2.SearchDatabase &&
//               item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation &&
//               item1.FileFormat=item2.FileFormat && item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:IdentificationFile) =
//                    IdentificationFileHandler.tryFindByLocation dbContext item.Location
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match IdentificationFileHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:IdentificationFile) =
//                IdentificationFileHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type IdentificationRefHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    fkIdentificationFile : string,
//                    identificationFile   : IdentificationFile,
//                    ?id                  : string
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                 = defaultArg id (System.Guid.NewGuid().ToString())
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new IdentificationRef(
//                                      id', 
//                                      fkIdentificationFile,
//                                      identificationFile,
//                                      //mzIdentML', 
//                                      Nullable(DateTime.Now)
//                                     )

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a identificationRef-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.IdentificationRef.Find(id))

//            ///Tries to find a identificationRef-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByFKIdentificationFile (dbContext:MzQuantML) (fkIdentificationFile:string) =
//                query {
//                       for i in dbContext.IdentificationRef.Local do
//                           if i.FKIdentificationFile=fkIdentificationFile
//                              then select (i, i.IdentificationFile)
//                      }
//                |> Seq.map (fun (identificationRef, _) -> identificationRef)
//                |> (fun identificationRef -> 
//                    if (Seq.exists (fun identificationRef' -> identificationRef' <> null) identificationRef) = false
//                        then 
//                            query {
//                                   for i in dbContext.IdentificationRef do
//                                       if i.FKIdentificationFile=fkIdentificationFile
//                                          then select (i, i.IdentificationFile)
//                                  }
//                            |> Seq.map (fun (identificationRef, _) -> identificationRef)
//                            |> (fun identificationRef -> if (Seq.exists (fun identificationRef' -> identificationRef' <> null) identificationRef) = false
//                                                            then None
//                                                            else Some identificationRef
//                               )
//                        else Some identificationRef
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:IdentificationRef) (item2:IdentificationRef) =
//               item1.IdentificationFile=item2.IdentificationFile

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:IdentificationRef) =
//                    IdentificationRefHandler.tryFindByFKIdentificationFile dbContext item.FKIdentificationFile
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match IdentificationRefHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:IdentificationRef) =
//                IdentificationRefHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type MethodFileHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    location                     : string,
//                    ?id                          : string,
//                    ?name                        : string,
//                    ?externalFormatDocumentation : string,
//                    ?fileFormat                  : CVParam
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let name'                        = defaultArg name Unchecked.defaultof<string>
//                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
//                let fileFormat'                  = defaultArg fileFormat Unchecked.defaultof<CVParam>
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new MethodFile(
//                               id', 
//                               name', 
//                               location, 
//                               externalFormatDocumentation',
//                               fileFormat',
//                               //mzIdentML', 
//                               Nullable(DateTime.Now)
//                              )

//            ///Replaces name of existing object with new one.
//            static member addName
//                (name:string) (table:MethodFile) =
//                table.Name <- name
//                table

//            ///Replaces externalFormatDocumentation of existing object with new one.
//            static member addExternalFormatDocumentation
//                (externalFormatDocumentation:string) (table:MethodFile) =
//                table.ExternalFormatDocumentation <- externalFormatDocumentation
//                table

//            ///Replaces fileFormat of existing object with new one.
//            static member addFileFormat
//                (fileFormat:CVParam) (table:MethodFile) =
//                table.FileFormat <- fileFormat
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a methodFile-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.MethodFile.Find(id))

//            ///Tries to find a methodFile-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByLocation (dbContext:MzQuantML) (location:string) =
//                query {
//                       for i in dbContext.MethodFile.Local do
//                           if i.Location=location
//                              then select (i, i.FileFormat)
//                      }
//                |> Seq.map (fun (methodFile, _) -> methodFile)
//                |> (fun methodFile -> 
//                    if (Seq.exists (fun methodFile' -> methodFile' <> null) methodFile) = false
//                        then 
//                            query {
//                                   for i in dbContext.MethodFile do
//                                       if i.Location=location
//                                          then select (i, i.FileFormat)
//                                  }
//                            |> Seq.map (fun (methodFile, _) -> methodFile)
//                            |> (fun methodFile -> if (Seq.exists (fun methodFile' -> methodFile' <> null) methodFile) = false
//                                                            then None
//                                                            else Some methodFile
//                               )
//                        else Some methodFile
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:MethodFile) (item2:MethodFile) =
//               item1.Name=item2.Name && item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation
//               && item1.FileFormat=item2.FileFormat

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:MethodFile) =
//                    MethodFileHandler.tryFindByLocation dbContext item.Location
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match MethodFileHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:MethodFile) =
//                MethodFileHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type RawFileHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    location                     : string,
//                    ?id                          : string,
//                    ?name                        : string,
//                    ?externalFormatDocumentation : string,
//                    ?methodFile                  : MethodFile,
//                    ?fileFormat                  : CVParam, 
//                    ?details                     : seq<RawFileParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let name'                        = defaultArg name Unchecked.defaultof<string>
//                let methodFile'                  = defaultArg methodFile Unchecked.defaultof<MethodFile>
//                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
//                let fileFormat'                  = defaultArg fileFormat Unchecked.defaultof<CVParam>
//                let details'                     = convertOptionToList details
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new RawFile(
//                            id', 
//                            name', 
//                            location, 
//                            methodFile',
//                            externalFormatDocumentation',
//                            fileFormat',
//                            details',
//                            //mzIdentML', 
//                            Nullable(DateTime.Now)
//                           )

//            ///Replaces name of existing object with new one.
//            static member addName
//                (name:string) (table:RawFile) =
//                table.Name <- name
//                table

//            ///Replaces methodFile of existing object with new one.
//            static member addMethodFile
//                (methodFile:MethodFile) (table:RawFile) =
//                table.MethodFile <- methodFile
//                table

//            ///Replaces externalFormatDocumentation of existing object with new one.
//            static member addExternalFormatDocumentation
//                (externalFormatDocumentation:string) (table:RawFile) =
//                table.ExternalFormatDocumentation <- externalFormatDocumentation
//                table

//            ///Replaces fileFormat of existing object with new one.
//            static member addFileFormat
//                (fileFormat:CVParam) (table:RawFile) =
//                table.FileFormat <- fileFormat
//                table

//            ///Adds a identificationFileParam to an existing object.
//            static member addDetail (detail:RawFileParam) (table:RawFile) =
//                let result = table.Details <- addToList table.Details detail
//                table

//            ///Adds a collection of identificationFileParams to an existing object.
//            static member addDetails (details:seq<RawFileParam>) (table:RawFile) =
//                let result = table.Details <- addCollectionToList table.Details details
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a rawFile-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.RawFile.Find(id))

//            ///Tries to find a rawFile-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByName (dbContext:MzQuantML) (location:string) =
//                query {
//                       for i in dbContext.RawFile.Local do
//                           if i.Location=location
//                              then select (i, i.FileFormat, i.MethodFile, i.Details)
//                      }
//                |> Seq.map (fun (rawFile, _, _, _) -> rawFile)
//                |> (fun rawFile -> 
//                    if (Seq.exists (fun rawFile' -> rawFile' <> null) rawFile) = false
//                        then 
//                            query {
//                                   for i in dbContext.RawFile do
//                                       if i.Location=location
//                                          then select  (i, i.FileFormat, i.MethodFile, i.Details)
//                                  }
//                            |> Seq.map (fun (rawFile, _, _, _) -> rawFile)
//                            |> (fun rawFile -> if (Seq.exists (fun rawFile' -> rawFile' <> null) rawFile) = false
//                                                            then None
//                                                            else Some rawFile
//                               )
//                        else Some rawFile
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:RawFile) (item2:RawFile) =
//               item1.Name=item2.Name && item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation
//               && item1.FileFormat=item2.FileFormat && item1.MethodFile=item2.MethodFile
//               && item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:RawFile) =
//                    RawFileHandler.tryFindByName dbContext item.Name
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match RawFileHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:RawFile) =
//                RawFileHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type RawFilesGroupHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    rawFiles                     : seq<RawFile>,
//                    ?id                          : string, 
//                    ?details                     : seq<RawFilesGroupParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let details'                     = convertOptionToList details
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new RawFilesGroup(
//                                  id', 
//                                  rawFiles |> List, 
//                                  details',
//                                  //mzIdentML', 
//                                  Nullable(DateTime.Now)
//                                 )

//            ///Adds a rawFile to an existing object.
//            static member addRawFile (detail:RawFile) (table:RawFilesGroup) =
//                let result = table.RawFiles <- addToList table.RawFiles detail
//                table

//            ///Adds a collection of rawFile to an existing object.
//            static member addRawFiles (details:seq<RawFile>) (table:RawFilesGroup) =
//                let result = table.RawFiles <- addCollectionToList table.RawFiles details
//                table
            
//            ///Adds a rawFilesGroupParam to an existing object.
//            static member addDetail (detail:RawFilesGroupParam) (table:RawFilesGroup) =
//                let result = table.Details <- addToList table.Details detail
//                table

//            ///Adds a collection of rawFilesGroupParams to an existing object.
//            static member addDetails (details:seq<RawFilesGroupParam>) (table:RawFilesGroup) =
//                let result = table.Details <- addCollectionToList table.Details details
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a rawFilesGroup-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.RawFilesGroup.Find(id))

//            ///Tries to find a rawFilesGroup-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByRawFiles (dbContext:MzQuantML) (rawFiles:seq<RawFile>) =
//                query {
//                       for i in dbContext.RawFilesGroup.Local do
//                           if i.RawFiles=(rawFiles |> List)
//                              then select (i, i.RawFiles, i.Details)
//                      }
//                |> Seq.map (fun (rawFilesGroup, _, _) -> rawFilesGroup)
//                |> (fun rawFilesGroup -> 
//                    if (Seq.exists (fun rawFilesGroup' -> rawFilesGroup' <> null) rawFilesGroup) = false
//                        then 
//                            query {
//                                   for i in dbContext.RawFilesGroup do
//                                       if i.RawFiles=(rawFiles |> List)
//                                          then select  (i, i.RawFiles, i.Details)
//                                  }
//                            |> Seq.map (fun (rawFilesGroup, _, _) -> rawFilesGroup)
//                            |> (fun rawFilesGroup -> if (Seq.exists (fun rawFilesGroup' -> rawFilesGroup' <> null) rawFilesGroup) = false
//                                                            then None
//                                                            else Some rawFilesGroup
//                               )
//                        else Some rawFilesGroup
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:RawFilesGroup) (item2:RawFilesGroup) =
//                item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:RawFilesGroup) =
//                    RawFilesGroupHandler.tryFindByRawFiles dbContext item.RawFiles
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match RawFilesGroupHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:RawFilesGroup) =
//                RawFilesGroupHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type InputFilesHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    ?id                          : string,
//                    ?rawFilesGroups              : seq<RawFilesGroup>,
//                    ?methodFiles                 : seq<MethodFile>,
//                    ?identificationFiles         : seq<IdentificationFile>,
//                    ?searchDatabases             : seq<SearchDatabase>,
//                    ?sourceFiles                 : seq<SourceFile>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let rawFilesGroups'              = convertOptionToList rawFilesGroups
//                let methodFiles'                 = convertOptionToList methodFiles
//                let identificationFiles'         = convertOptionToList identificationFiles
//                let searchDatabases'             = convertOptionToList searchDatabases
//                let sourceFiles'                 = convertOptionToList sourceFiles
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new InputFiles(
//                               id', 
//                               rawFilesGroups', 
//                               methodFiles',
//                               identificationFiles',
//                               searchDatabases',
//                               sourceFiles',
//                               //mzIdentML', 
//                               Nullable(DateTime.Now)
//                              )

//            ///Adds a rawFilesGroup to an existing object.
//            static member addRawFilesGroup (detail:RawFilesGroup) (table:InputFiles) =
//                let result = table.RawFilesGroups <- addToList table.RawFilesGroups detail
//                table

//            ///Adds a collection of rawFilesGroups to an existing object.
//            static member addRawFilesGroups (details:seq<RawFilesGroup>) (table:InputFiles) =
//                let result = table.RawFilesGroups <- addCollectionToList table.RawFilesGroups details
//                table
            
//            ///Adds a methodFile to an existing object.
//            static member addMethodFile (detail:MethodFile) (table:InputFiles) =
//                let result = table.MethodFiles <- addToList table.MethodFiles detail
//                table

//            ///Adds a collection of methodFiles to an existing object.
//            static member addMethodFiles (details:seq<MethodFile>) (table:InputFiles) =
//                let result = table.MethodFiles <- addCollectionToList table.MethodFiles details
//                table
            
//            ///Adds a identificationFile to an existing object.
//            static member addIdentificationFile (detail:IdentificationFile) (table:InputFiles) =
//                let result = table.IdentificationFiles <- addToList table.IdentificationFiles detail
//                table

//            ///Adds a collection of identificationFiles to an existing object.
//            static member addIdentificationFiles (details:seq<IdentificationFile>) (table:InputFiles) =
//                let result = table.IdentificationFiles <- addCollectionToList table.IdentificationFiles details
//                table
            
//            ///Adds a searchDatabase to an existing object.
//            static member addSearchDatabase (detail:SearchDatabase) (table:InputFiles) =
//                let result = table.SearchDatabases <- addToList table.SearchDatabases detail
//                table

//            ///Adds a collection of searchDatabases to an existing object.
//            static member addDetails (details:seq<SearchDatabase>) (table:InputFiles) =
//                let result = table.SearchDatabases <- addCollectionToList table.SearchDatabases details
//                table
            
//            ///Adds a sourceFile to an existing object.
//            static member addSourceFile (detail:SourceFile) (table:InputFiles) =
//                let result = table.SourceFiles <- addToList table.SourceFiles detail
//                table

//            ///Adds a collection of sourceFiles to an existing object.
//            static member addSourceFiles (details:seq<SourceFile>) (table:InputFiles) =
//                let result = table.SourceFiles <- addCollectionToList table.SourceFiles details
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a inputFiles-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.InputFiles.Find(id))

//            ///Tries to find a inputFiles-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByRawFilesGroups (dbContext:MzQuantML) (rawFilesGroups:seq<RawFilesGroup>) =
//                query {
//                       for i in dbContext.InputFiles.Local do
//                           if i.RawFilesGroups=(rawFilesGroups |> List)
//                              then select (i, i.MethodFiles, i.RawFilesGroups, i.SearchDatabases, i.SourceFiles, i.IdentificationFiles)
//                      }
//                |> Seq.map (fun (inputFiles, _, _, _, _, _) -> inputFiles)
//                |> (fun inputFiles -> 
//                    if (Seq.exists (fun inputFiles' -> inputFiles' <> null) inputFiles) = false
//                        then 
//                            query {
//                                   for i in dbContext.InputFiles do
//                                       if i.RawFilesGroups=(rawFilesGroups |> List)
//                                          then select  (i, i.MethodFiles, i.RawFilesGroups, i.SearchDatabases, i.SourceFiles, i.IdentificationFiles)
//                                  }
//                            |> Seq.map (fun (inputFiles, _, _, _, _, _) -> inputFiles)
//                            |> (fun inputFiles -> if (Seq.exists (fun inputFiles' -> inputFiles' <> null) inputFiles) = false
//                                                            then None
//                                                            else Some inputFiles
//                               )
//                        else Some inputFiles
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:InputFiles) (item2:InputFiles) =
//               item1.MethodFiles=item2.MethodFiles && item1.SearchDatabases=item2.SearchDatabases
//               && item1.SourceFiles=item2.SourceFiles && item1.IdentificationFiles=item2.IdentificationFiles

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:InputFiles) =
//                    InputFilesHandler.tryFindByRawFilesGroups dbContext item.RawFilesGroups
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match InputFilesHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:InputFiles) =
//                InputFilesHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type ModificationHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    detail                     : CVParam,
//                    ?id                        : string,
//                    ?massDelta                 : float,
//                    ?residues                  : string
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let massDelta'                   = defaultArg massDelta Unchecked.defaultof<float>
//                let residues'                    = defaultArg residues Unchecked.defaultof<string>
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new Modification(
//                                 id', 
//                                 Nullable(massDelta'),
//                                 residues',
//                                 detail,
//                                 //mzIdentML', 
//                                 Nullable(DateTime.Now)
//                                )

//            ///Replaces name of existing object with new one.
//            static member addMassDelta
//                (massDelta:float) (table:Modification) =
//                table.MassDelta <- Nullable(massDelta)
//                table

//            ///Replaces name of existing object with new one.
//            static member addResidues
//                (residues:string) (table:Modification) =
//                table.Residues <- residues
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a modification-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.Modification.Find(id))

//            ///Tries to find a modification-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByName (dbContext:MzQuantML) (detail:CVParam) =
//                query {
//                       for i in dbContext.Modification.Local do
//                           if i.Detail=detail
//                              then select (i, i.Detail)
//                      }
//                |> Seq.map (fun (modification, _) -> modification)
//                |> (fun modification -> 
//                    if (Seq.exists (fun modification' -> modification' <> null) modification) = false
//                        then 
//                            query {
//                                   for i in dbContext.Modification do
//                                       if i.Detail=detail
//                                          then select (i, i.Detail)
//                                  }
//                            |> Seq.map (fun (modification, _) -> modification)
//                            |> (fun modification -> if (Seq.exists (fun modification' -> modification' <> null) modification) = false
//                                                            then None
//                                                            else Some modification
//                               )
//                        else Some modification
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:Modification) (item2:Modification) =
//                item1.MassDelta=item2.MassDelta && item1.Residues=item2.Residues

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:Modification) =
//                    ModificationHandler.tryFindByName dbContext item.Detail
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match ModificationHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:Modification) =
//                ModificationHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type AssayHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    ?id                          : string,
//                    ?name                        : string,
//                    ?rawFilesGroup               : RawFilesGroup,
//                    ?label                       : seq<Modification>,
//                    ?identificationFile          : IdentificationFile,
//                    ?details                     : seq<AssayParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let name'                        = defaultArg name Unchecked.defaultof<string>
//                let rawFilesGroup'               = defaultArg rawFilesGroup Unchecked.defaultof<RawFilesGroup>
//                let label'                       = convertOptionToList label
//                let identificationFile'          = defaultArg identificationFile Unchecked.defaultof<IdentificationFile>
//                let details'                     = convertOptionToList details
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new Assay(
//                          id', 
//                          name', 
//                          rawFilesGroup', 
//                          label', 
//                          identificationFile', 
//                          details',
//                          Nullable(DateTime.Now)
//                         )

//            ///Replaces name of existing object with new one.
//            static member addName
//                (name:string) (table:Assay) =
//                table.Name <- name
//                table
            
//            ///Replaces rawFilesGroup of existing object with new one.
//            static member addRawFilesGroup
//                (rawFilesGroup:RawFilesGroup) (table:Assay) =
//                table.RawFilesGroup <- rawFilesGroup
//                table

//            ///Replaces identificationFile of existing object with new one.
//            static member addIdentificationFile
//                (identificationFile:IdentificationFile) (table:Assay) =
//                table.IdentificationFile <- identificationFile
//                table

//            ///Adds a modification to an existing object.
//            static member addLabel (modification:Modification) (table:Assay) =
//                let result = table.Label <- addToList table.Label modification
//                table

//            ///Adds a collection of modifications to an existing object.
//            static member addLabels (modifications:seq<Modification>) (table:Assay) =
//                let result = table.Label <- addCollectionToList table.Label modifications
//                table

//            ///Adds a assayParam to an existing object.
//            static member addDetail (detail:AssayParam) (table:Assay) =
//                let result = table.Details <- addToList table.Details detail
//                table

//            ///Adds a collection of assayParams to an existing object.
//            static member addDetails (details:seq<AssayParam>) (table:Assay) =
//                let result = table.Details <- addCollectionToList table.Details details
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a assay-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.Assay.Find(id))

//            ///Tries to find an assay-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.Assay.Local do
//                           if i.Name=name
//                              then select (i, i.RawFilesGroup, i.IdentificationFile, i.Label, i.Details)
//                      }
//                |> Seq.map (fun (assay, _, _, _, _) -> assay)
//                |> (fun assay -> 
//                    if (Seq.exists (fun assay' -> assay' <> null) assay) = false
//                        then 
//                            query {
//                                   for i in dbContext.Assay do
//                                       if i.Name=name
//                                          then select (i, i.RawFilesGroup, i.IdentificationFile, i.Label, i.Details)
//                                  }
//                            |> Seq.map (fun (assay, _, _, _, _) -> assay)
//                            |> (fun assay -> if (Seq.exists (fun assay' -> assay' <> null) assay) = false
//                                                            then None
//                                                            else Some assay
//                               )
//                        else Some assay
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:Assay) (item2:Assay) =
//                item1.RawFilesGroup=item2.RawFilesGroup && item1.IdentificationFile=item2.IdentificationFile 
//                && item1.Label=item2.Label && item1.Details=item2.Details 

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:Assay) =
//                    AssayHandler.tryFindByName dbContext item.Name
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match AssayHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:Assay) =
//                AssayHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type StudyVariableHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    assays                       : seq<Assay>,
//                    ?id                          : string,
//                    ?name                        : string,
//                    ?details                     : seq<StudyVariableParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let name'                        = defaultArg name Unchecked.defaultof<string>
//                let details'                     = convertOptionToList details
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new StudyVariable(
//                                  id', 
//                                  name',
//                                  assays |> List,
//                                  details',
//                                  Nullable(DateTime.Now)
//                                 )

//            ///Replaces name of existing object with new one.
//            static member addName
//                (name:string) (table:StudyVariable) =
//                table.Name <- name
//                table

//            ///Adds a studyVariableParam to an existing object.
//            static member addDetail (detail:StudyVariableParam) (table:StudyVariable) =
//                let result = table.Details <- addToList table.Details detail
//                table

//            ///Adds a collection of studyVariableParams to an existing object.
//            static member addDetails (details:seq<StudyVariableParam>) (table:StudyVariable) =
//                let result = table.Details <- addCollectionToList table.Details details
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a studyVariable-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.StudyVariable.Find(id))

//            ///Tries to find a studyVariable-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.StudyVariable.Local do
//                           if i.Name=name
//                              then select (i, i.Assays, i.Details)
//                      }
//                |> Seq.map (fun (studyVariable, _, _) -> studyVariable)
//                |> (fun studyVariable -> 
//                    if (Seq.exists (fun studyVariable' -> studyVariable' <> null) studyVariable) = false
//                        then 
//                            query {
//                                   for i in dbContext.StudyVariable do
//                                       if i.Name=name
//                                          then select (i, i.Assays, i.Details)
//                                  }
//                            |> Seq.map (fun (studyVariable, _, _) -> studyVariable)
//                            |> (fun studyVariable -> if (Seq.exists (fun studyVariable' -> studyVariable' <> null) studyVariable) = false
//                                                            then None
//                                                            else Some studyVariable
//                               )
//                        else Some studyVariable
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:StudyVariable) (item2:StudyVariable) =
//                item1.Assays=item2.Assays && item1.Details=item2.Details 

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:StudyVariable) =
//                    StudyVariableHandler.tryFindByName dbContext item.Name
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match StudyVariableHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:StudyVariable) =
//                StudyVariableHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type RatioHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    numerator                    : StudyVariable,
//                    denominator                  : StudyVariable,
//                    numeratorDatatype            : CVParam,
//                    denominatorDatatype          : CVParam,
//                    ?id                          : string,
//                    ?name                        : string,
//                    ?ratioCalculation            : seq<RatioCalculationParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let name'                        = defaultArg name Unchecked.defaultof<string>
//                let ratioCalculation'            = convertOptionToList ratioCalculation
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new Ratio(
//                          id', 
//                          name',
//                          numerator,
//                          denominator,
//                          ratioCalculation',
//                          numeratorDatatype,
//                          denominatorDatatype,
//                          Nullable(DateTime.Now)
//                         )

//            ///Replaces name of existing object with new one.
//            static member addName
//                (name:string) (table:Ratio) =
//                table.Name <- name
//                table

//            ///Adds a ratioCalculationParam to an existing object.
//            static member addRatioCalculationParam (ratioCalculationParam:RatioCalculationParam) (table:Ratio) =
//                let result = table.RatioCalculation <- addToList table.RatioCalculation ratioCalculationParam
//                table

//            ///Adds a collection of ratioCalculationParams to an existing object.
//            static member addRatioCalculationParams (ratioCalculationParams:seq<RatioCalculationParam>) (table:Ratio) =
//                let result = table.RatioCalculation <- addCollectionToList table.RatioCalculation ratioCalculationParams
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a ratio-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.Ratio.Find(id))

//            ///Tries to find a ratio-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.Ratio.Local do
//                           if i.Name=name
//                              then select (i, i.Numerator, i.Denominator, i.NumeratorDataType, i.DenominatorDataType, i.RatioCalculation)
//                      }
//                |> Seq.map (fun (ratio, _, _ ,_ , _, _) -> ratio)
//                |> (fun ratio -> 
//                    if (Seq.exists (fun ratio' -> ratio' <> null) ratio) = false
//                        then 
//                            query {
//                                   for i in dbContext.Ratio do
//                                       if i.Name=name
//                                          then select (i, i.Numerator, i.Denominator, i.NumeratorDataType, i.DenominatorDataType, i.RatioCalculation)
//                                  }
//                            |> Seq.map (fun (ratio, _, _ ,_ , _, _) -> ratio)
//                            |> (fun ratio -> if (Seq.exists (fun ratio' -> ratio' <> null) ratio) = false
//                                                            then None
//                                                            else Some ratio
//                               )
//                        else Some ratio
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:Ratio) (item2:Ratio) =
//                item1.Numerator=item2.Numerator && item1.Denominator=item2.Denominator && 
//                item1.NumeratorDataType=item2.NumeratorDataType && item1.RatioCalculation=item2.RatioCalculation && 
//                item1.DenominatorDataType=item2.DenominatorDataType              

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:Ratio) =
//                    RatioHandler.tryFindByName dbContext item.Name
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match RatioHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:Ratio) =
//                RatioHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type ColumnHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    index                        : int,
//                    datatype                     : CVParam,
//                    ?id                          : string
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new Column(
//                           id',
//                           Nullable(index),
//                           datatype,
//                           Nullable(DateTime.Now)
//                          )

//            ///Tries to find a column-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.Column.Find(id))

//            ///Tries to find a column-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByIndex (dbContext:MzQuantML) (index:Nullable<int>) =
//                query {
//                       for i in dbContext.Column.Local do
//                           if i.Index=index
//                              then select (i, i.DataType)
//                      }
//                |> Seq.map (fun (column, _) -> column)
//                |> (fun column -> 
//                    if (Seq.exists (fun column' -> column' <> null) column) = false
//                        then 
//                            query {
//                                   for i in dbContext.Column do
//                                       if i.Index=index
//                                          then select (i, i.DataType)
//                                  }
//                            |> Seq.map (fun (column, _) -> column)
//                            |> (fun column -> if (Seq.exists (fun column' -> column' <> null) column) = false
//                                                            then None
//                                                            else Some column
//                               )
//                        else Some column
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:Column) (item2:Column) =
//                item1.DataType=item2.DataType

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:Column) =
//                    ColumnHandler.tryFindByIndex dbContext item.Index
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match ColumnHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:Column) =
//                ColumnHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type DataMatrixHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    row                          : string,
//                    ?id                          : string
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new DataMatrix(
//                               id',
//                               row,
//                               Nullable(DateTime.Now)
//                              )

//            ///Tries to find a dataMatrix-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.DataMatrix.Find(id))

//            ///Tries to find a dataMatrix-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByRow (dbContext:MzQuantML) (row:string) =
//                query {
//                       for i in dbContext.DataMatrix.Local do
//                           if i.Row=row
//                              then select i
//                      }
//                |> Seq.map (fun (dataMatrix) -> dataMatrix)
//                |> (fun dataMatrix -> 
//                    if (Seq.exists (fun dataMatrix' -> dataMatrix' <> null) dataMatrix) = false
//                        then 
//                            query {
//                                   for i in dbContext.DataMatrix do
//                                       if i.Row=row
//                                          then select i
//                                  }
//                            |> Seq.map (fun (dataMatrix) -> dataMatrix)
//                            |> (fun dataMatrix -> if (Seq.exists (fun dataMatrix' -> dataMatrix' <> null) dataMatrix) = false
//                                                            then None
//                                                            else Some dataMatrix
//                               )
//                        else Some dataMatrix
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:DataMatrix) (item2:DataMatrix) =
//                item1.ID=item2.ID

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:DataMatrix) =
//                    DataMatrixHandler.tryFindByRow dbContext item.Row
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match DataMatrixHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:DataMatrix) =
//                DataMatrixHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type AssayQuantLayerHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    dataType                     : CVParam,
//                    columnIndex                  : string,
//                    dataMatrix                   : DataMatrix,
//                    ?id                          : string
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new AssayQuantLayer(
//                                    id',
//                                    dataType,
//                                    columnIndex,
//                                    dataMatrix,
//                                    Nullable(DateTime.Now)
//                                   )

//            ///Tries to find a assayQuantLayer-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.AssayQuantLayer.Find(id))

//            ///Tries to find a assayQuantLayer-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
//                query {
//                       for i in dbContext.AssayQuantLayer.Local do
//                           if i.ColumnIndex=columnIndex
//                              then select (i, i.DataType, i.DataMatrix)
//                      }
//                |> Seq.map (fun (assayQuantLayer, _, _) -> assayQuantLayer)
//                |> (fun assayQuantLayer -> 
//                    if (Seq.exists (fun assayQuantLayer' -> assayQuantLayer' <> null) assayQuantLayer) = false
//                        then 
//                            query {
//                                   for i in dbContext.AssayQuantLayer do
//                                       if i.ColumnIndex=columnIndex
//                                          then select (i, i.DataType, i.DataMatrix)
//                                  }
//                            |> Seq.map (fun (assayQuantLayer, _, _) -> assayQuantLayer)
//                            |> (fun assayQuantLayer -> if (Seq.exists (fun assayQuantLayer' -> assayQuantLayer' <> null) assayQuantLayer) = false
//                                                            then None
//                                                            else Some assayQuantLayer
//                               )
//                        else Some assayQuantLayer
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:AssayQuantLayer) (item2:AssayQuantLayer) =
//                item1.DataType=item2.DataType && item1.DataMatrix=item2.DataMatrix

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:AssayQuantLayer) =
//                    AssayQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match AssayQuantLayerHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:AssayQuantLayer) =
//                AssayQuantLayerHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type GlobalQuantLayerHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    columns                      : seq<Column>,
//                    dataMatrix                   : DataMatrix,
//                    ?id                          : string
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new GlobalQuantLayer(
//                                     id',
//                                     columns |> List,
//                                     dataMatrix,
//                                     Nullable(DateTime.Now)
//                                    )

//            ///Tries to find a globalQuantLayer-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.GlobalQuantLayer.Find(id))

//            ///Tries to find a globalQuantLayer-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByDataMatrix (dbContext:MzQuantML) (dataMatrix:DataMatrix) =
//                query {
//                       for i in dbContext.GlobalQuantLayer.Local do
//                           if i.DataMatrix=dataMatrix
//                              then select (i, i.Columns, i.DataMatrix)
//                      }
//                |> Seq.map (fun (globalQuantLayer, _, _) -> globalQuantLayer)
//                |> (fun globalQuantLayer -> 
//                    if (Seq.exists (fun globalQuantLayer' -> globalQuantLayer' <> null) globalQuantLayer) = false
//                        then 
//                            query {
//                                   for i in dbContext.GlobalQuantLayer do
//                                       if i.DataMatrix=dataMatrix
//                                          then select (i, i.Columns, i.DataMatrix)
//                                  }
//                            |> Seq.map (fun (globalQuantLayer, _, _) -> globalQuantLayer)
//                            |> (fun globalQuantLayer -> if (Seq.exists (fun globalQuantLayer' -> globalQuantLayer' <> null) globalQuantLayer) = false
//                                                            then None
//                                                            else Some globalQuantLayer
//                               )
//                        else Some globalQuantLayer
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:GlobalQuantLayer) (item2:GlobalQuantLayer) =
//                item1.Columns=item2.Columns

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:GlobalQuantLayer) =
//                    GlobalQuantLayerHandler.tryFindByDataMatrix dbContext item.DataMatrix
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match GlobalQuantLayerHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:GlobalQuantLayer) =
//                GlobalQuantLayerHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type MS2AssayQuantLayerHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    dataType                     : CVParam,
//                    columnIndex                  : string,
//                    dataMatrix                   : DataMatrix,
//                    ?id                          : string
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new MS2AssayQuantLayer(
//                                       id',
//                                       dataType,
//                                       columnIndex,
//                                       dataMatrix,
//                                       Nullable(DateTime.Now)
//                                      )

//            ///Tries to find a ms2AssayQuantLayer-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.MS2AssayQuantLayer.Find(id))

//            ///Tries to find a ms2AssayQuantLayer-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
//                query {
//                       for i in dbContext.MS2AssayQuantLayer.Local do
//                           if i.ColumnIndex=columnIndex
//                              then select (i, i.DataType, i.DataMatrix)
//                      }
//                |> Seq.map (fun (ms2AssayQuantLayer, _, _) -> ms2AssayQuantLayer)
//                |> (fun ms2AssayQuantLayer -> 
//                    if (Seq.exists (fun ms2AssayQuantLayer' -> ms2AssayQuantLayer' <> null) ms2AssayQuantLayer) = false
//                        then 
//                            query {
//                                   for i in dbContext.MS2AssayQuantLayer do
//                                       if i.ColumnIndex=columnIndex
//                                          then select (i, i.DataType, i.DataMatrix)
//                                  }
//                            |> Seq.map (fun (ms2AssayQuantLayer, _, _) -> ms2AssayQuantLayer)
//                            |> (fun ms2AssayQuantLayer -> if (Seq.exists (fun ms2AssayQuantLayer' -> ms2AssayQuantLayer' <> null) ms2AssayQuantLayer) = false
//                                                            then None
//                                                            else Some ms2AssayQuantLayer
//                               )
//                        else Some ms2AssayQuantLayer
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:MS2AssayQuantLayer) (item2:MS2AssayQuantLayer) =
//                item1.DataType=item2.DataType && item1.DataMatrix=item2.DataMatrix

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:MS2AssayQuantLayer) =
//                    MS2AssayQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match MS2AssayQuantLayerHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:MS2AssayQuantLayer) =
//                MS2AssayQuantLayerHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type StudyVariableQuantLayerHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    dataType                     : CVParam,
//                    columnIndex                  : string,
//                    dataMatrix                   : DataMatrix,
//                    ?id                          : string
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new StudyVariableQuantLayer(
//                                            id',
//                                            dataType,
//                                            columnIndex,
//                                            dataMatrix,
//                                            Nullable(DateTime.Now)
//                                           )

//            ///Tries to find a studyVariableQuantLayer-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.StudyVariableQuantLayer.Find(id))

//            ///Tries to find a studyVariableQuantLayer-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
//                query {
//                       for i in dbContext.StudyVariableQuantLayer.Local do
//                           if i.ColumnIndex=columnIndex
//                              then select (i, i.DataType, i.DataMatrix)
//                      }
//                |> Seq.map (fun (studyVariableQuantLayer, _, _) -> studyVariableQuantLayer)
//                |> (fun studyVariableQuantLayer -> 
//                    if (Seq.exists (fun studyVariableQuantLayer' -> studyVariableQuantLayer' <> null) studyVariableQuantLayer) = false
//                        then 
//                            query {
//                                   for i in dbContext.StudyVariableQuantLayer do
//                                       if i.ColumnIndex=columnIndex
//                                          then select (i, i.DataType, i.DataMatrix)
//                                  }
//                            |> Seq.map (fun (studyVariableQuantLayer, _, _) -> studyVariableQuantLayer)
//                            |> (fun studyVariableQuantLayer -> if (Seq.exists (fun studyVariableQuantLayer' -> studyVariableQuantLayer' <> null) studyVariableQuantLayer) = false
//                                                                    then None
//                                                                    else Some studyVariableQuantLayer
//                               )
//                        else Some studyVariableQuantLayer
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:StudyVariableQuantLayer) (item2:StudyVariableQuantLayer) =
//                item1.DataType=item2.DataType && item1.DataMatrix=item2.DataMatrix

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:StudyVariableQuantLayer) =
//                    StudyVariableQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match StudyVariableQuantLayerHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:StudyVariableQuantLayer) =
//                StudyVariableQuantLayerHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type RatioQuantLayerHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    columnIndex                  : string,
//                    dataMatrix                   : DataMatrix,
//                    ?id                          : string
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new RatioQuantLayer(
//                                    id',
//                                    columnIndex,
//                                    dataMatrix,
//                                    Nullable(DateTime.Now)
//                                   )

//            ///Tries to find a ratioQuantLayer-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.RatioQuantLayer.Find(id))

//            ///Tries to find a ratioQuantLayer-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
//                query {
//                       for i in dbContext.RatioQuantLayer.Local do
//                           if i.ColumnIndex=columnIndex
//                              then select (i, i.DataMatrix)
//                      }
//                |> Seq.map (fun (studyVariableQuantLayer, _) -> studyVariableQuantLayer)
//                |> (fun studyVariableQuantLayer -> 
//                    if (Seq.exists (fun studyVariableQuantLayer' -> studyVariableQuantLayer' <> null) studyVariableQuantLayer) = false
//                        then 
//                            query {
//                                   for i in dbContext.RatioQuantLayer do
//                                       if i.ColumnIndex=columnIndex
//                                          then select (i, i.DataMatrix)
//                                  }
//                            |> Seq.map (fun (studyVariableQuantLayer, _) -> studyVariableQuantLayer)
//                            |> (fun studyVariableQuantLayer -> if (Seq.exists (fun studyVariableQuantLayer' -> studyVariableQuantLayer' <> null) studyVariableQuantLayer) = false
//                                                                    then None
//                                                                    else Some studyVariableQuantLayer
//                               )
//                        else Some studyVariableQuantLayer
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:RatioQuantLayer) (item2:RatioQuantLayer) =
//                item1.DataMatrix=item2.DataMatrix

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:RatioQuantLayer) =
//                    RatioQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match RatioQuantLayerHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:RatioQuantLayer) =
//                RatioQuantLayerHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type ProcessingMethodHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    order                        : int,
//                    ?id                          : string,
//                    ?details                     : seq<ProcessingMethodParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let details'                     = convertOptionToList details
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new ProcessingMethod(
//                                     id', 
//                                     Nullable(order),
//                                     details',
//                                     Nullable(DateTime.Now)
//                                    )

//            ///Adds a processingMethodParam to an existing object.
//            static member addDetail (processingMethodParam:ProcessingMethodParam) (table:ProcessingMethod) =
//                let result = table.Details <- addToList table.Details processingMethodParam
//                table

//            ///Adds a collection of processingMethodParams to an existing object.
//            static member addDetails (processingMethodParams:seq<ProcessingMethodParam>) (table:ProcessingMethod) =
//                let result = table.Details <- addCollectionToList table.Details processingMethodParams
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a processingMethod-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.ProcessingMethod.Find(id))

//            ///Tries to find a processingMethod-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByOrder (dbContext:MzQuantML) (order:Nullable<int>) =
//                query {
//                       for i in dbContext.ProcessingMethod.Local do
//                           if i.Order=order
//                              then select (i, i.Details)
//                      }
//                |> Seq.map (fun (processingMethod, _) -> processingMethod)
//                |> (fun processingMethod -> 
//                    if (Seq.exists (fun processingMethod' -> processingMethod' <> null) processingMethod) = false
//                        then 
//                            query {
//                                   for i in dbContext.ProcessingMethod do
//                                       if i.Order=order
//                                          then select (i, i.Details)
//                                  }
//                            |> Seq.map (fun (processingMethod, _) -> processingMethod)
//                            |> (fun processingMethod -> if (Seq.exists (fun processingMethod' -> processingMethod' <> null) processingMethod) = false
//                                                            then None
//                                                            else Some processingMethod
//                               )
//                        else Some processingMethod
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:ProcessingMethod) (item2:ProcessingMethod) =
//                item1.Details=item2.Details            

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:ProcessingMethod) =
//                    ProcessingMethodHandler.tryFindByOrder dbContext item.Order
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match ProcessingMethodHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProcessingMethod) =
//                ProcessingMethodHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type DataProcessingHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    order                        : int,
//                    analysisSoftware             : AnalysisSoftware,
//                    processingMethods            : seq<ProcessingMethod>,
//                    ?id                          : string,
//                    ?inputObjects                : string,
//                    ?outputObjects               : string
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let inputObjects'                = defaultArg inputObjects Unchecked.defaultof<string>
//                let outputObjects'               = defaultArg outputObjects Unchecked.defaultof<string>
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new DataProcessing(
//                                   id', 
//                                   Nullable(order),
//                                   analysisSoftware,
//                                   inputObjects',
//                                   outputObjects',
//                                   processingMethods |> List,
//                                   Nullable(DateTime.Now)
//                                  )

//            ///Replaces inputObjects of existing object with new one.
//            static member addInputObjects
//                (inputObjects:string) (table:DataProcessing) =
//                table.InputObjects <- inputObjects
//                table

//            ///Replaces outputObjects of existing object with new one.
//            static member addOutputObjects
//                (outputObjects:string) (table:DataProcessing) =
//                table.OutputObjects <- outputObjects
//                table
            
//            /////Adds a identificationFileParam to an existing object.
//            //static member addDetail (processingMethodParam:ProcessingMethodParam) (table:ProcessingMethod) =
//            //    let result = table.Details <- addToList table.Details processingMethodParam
//            //    table

//            /////Adds a collection of identificationFileParams to an existing object.
//            //static member addDetails (processingMethodParams:seq<ProcessingMethodParam>) (table:ProcessingMethod) =
//            //    let result = table.Details <- addCollectionToList table.Details processingMethodParams
//            //    table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a dataProcessing-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.DataProcessing.Find(id))

//            ///Tries to find a dataProcessing-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByOrder (dbContext:MzQuantML) (order:Nullable<int>) =
//                query {
//                       for i in dbContext.DataProcessing.Local do
//                           if i.Order=order
//                              then select (i, i.AnalysisSoftware, i.ProcessingMethods)
//                      }
//                |> Seq.map (fun (dataProcessing, _, _) -> dataProcessing)
//                |> (fun dataProcessing -> 
//                    if (Seq.exists (fun dataProcessing' -> dataProcessing' <> null) dataProcessing) = false
//                        then 
//                            query {
//                                   for i in dbContext.DataProcessing do
//                                       if i.Order=order
//                                          then select (i, i.AnalysisSoftware, i.ProcessingMethods)
//                                  }
//                            |> Seq.map (fun (dataProcessing, _, _) -> dataProcessing)
//                            |> (fun dataProcessing -> if (Seq.exists (fun dataProcessing' -> dataProcessing' <> null) dataProcessing) = false
//                                                            then None
//                                                            else Some dataProcessing
//                               )
//                        else Some dataProcessing
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:DataProcessing) (item2:DataProcessing) =
//                item1.AnalysisSoftware=item2.AnalysisSoftware && item1.InputObjects=item2.InputObjects &&
//                item1.OutputObjects=item2.OutputObjects && item1.ProcessingMethods=item2.ProcessingMethods

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:DataProcessing) =
//                    DataProcessingHandler.tryFindByOrder dbContext item.Order
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match DataProcessingHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:DataProcessing) =
//                DataProcessingHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type DBIdentificationRefHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    fkExternalFile               : string,
//                    searchDatabase               : SearchDatabase,
//                    ?id                          : string
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new DBIdentificationRef(
//                                        id', 
//                                        fkExternalFile,
//                                        searchDatabase,
//                                        Nullable(DateTime.Now)
//                                       )

//            ///Tries to find a dbIdentificationRef-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.DBIdentificationRef.Find(id))

//            ///Tries to find a dbIdentificationRef-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByFKExternalFile (dbContext:MzQuantML) (fkExternalFile:string) =
//                query {
//                       for i in dbContext.DBIdentificationRef.Local do
//                           if i.FKExternalFile=fkExternalFile
//                              then select (i, i.SearchDatabase)
//                      }
//                |> Seq.map (fun (dbIdentificationRef, _) -> dbIdentificationRef)
//                |> (fun dbIdentificationRef -> 
//                    if (Seq.exists (fun dbIdentificationRef' -> dbIdentificationRef' <> null) dbIdentificationRef) = false
//                        then 
//                            query {
//                                   for i in dbContext.DBIdentificationRef do
//                                       if i.FKExternalFile=fkExternalFile
//                                          then select (i, i.SearchDatabase)
//                                  }
//                            |> Seq.map (fun (dbIdentificationRef, _) -> dbIdentificationRef)
//                            |> (fun dbIdentificationRef -> if (Seq.exists (fun dbIdentificationRef' -> dbIdentificationRef' <> null) dbIdentificationRef) = false
//                                                            then None
//                                                            else Some dbIdentificationRef
//                               )
//                        else Some dbIdentificationRef
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:DBIdentificationRef) (item2:DBIdentificationRef) =
//                item1.SearchDatabase=item2.SearchDatabase

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:DBIdentificationRef) =
//                    DBIdentificationRefHandler.tryFindByFKExternalFile dbContext item.FKExternalFile
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match DBIdentificationRefHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:DBIdentificationRef) =
//                DBIdentificationRefHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type FeatureHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    charge                       : int,
//                    mz                           : float,
//                    retentionTime                : float,
//                    ?id                          : string,
//                    ?fkChromatogram              : string,
//                    ?rawFile                     : RawFile,
//                    ?fkSpectrum                  : string,
//                    ?massTraces                  : seq<MassTraceParam>,
//                    ?details                     : seq<FeatureParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let fkChromatogram'              = defaultArg fkChromatogram Unchecked.defaultof<string>
//                let rawFile'                     = defaultArg rawFile Unchecked.defaultof<RawFile>
//                let fkSpectrum'                  = defaultArg fkSpectrum Unchecked.defaultof<string>
//                let massTraces'                  = convertOptionToList massTraces
//                let details'                     = convertOptionToList details
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new Feature(
//                            id', 
//                            Nullable(charge),
//                            fkChromatogram',
//                            Nullable(mz),
//                            rawFile',
//                            Nullable(retentionTime),
//                            fkSpectrum',
//                            massTraces',
//                            details',
//                            Nullable(DateTime.Now)
//                           )

//            ///Replaces fkChromatogram of existing object with new one.
//            static member addFKChromatogram
//                (fkChromatogram:string) (table:Feature) =
//                table.FKChromatogram <- fkChromatogram
//                table

//            ///Replaces rawFile of existing object with new one.
//            static member addRawFile
//                (rawFile:RawFile) (table:Feature) =
//                table.RawFile <- rawFile
//                table

//            ///Replaces fkSpectrum of existing object with new one.
//            static member addFKSpectrum
//                (fkSpectrum:string) (table:Feature) =
//                table.FKSpectrum <- fkSpectrum
//                table
            
//            ///Adds a massTraceParam to an existing object.
//            static member addMassTrace (massTraceParam:MassTraceParam) (table:Feature) =
//                let result = table.MassTraces <- addToList table.MassTraces massTraceParam
//                table

//            ///Adds a collection of massTraceParams to an existing object.
//            static member addMassTraces (massTraceParams:seq<MassTraceParam>) (table:Feature) =
//                let result = table.MassTraces <- addCollectionToList table.MassTraces massTraceParams
//                table

//            ///Adds a processingMethodParam to an existing object.
//            static member addDetail (processingMethodParam:FeatureParam) (table:Feature) =
//                let result = table.Details <- addToList table.Details processingMethodParam
//                table

//            ///Adds a collection of processingMethodParams to an existing object.
//            static member addDetails (processingMethodParams:seq<FeatureParam>) (table:Feature) =
//                let result = table.Details <- addCollectionToList table.Details processingMethodParams
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a feature-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.Feature.Find(id))

//            ///Tries to find a feature-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByOrder (dbContext:MzQuantML) (mz:Nullable<float>) =
//                query {
//                       for i in dbContext.Feature.Local do
//                           if i.MZ=mz
//                              then select (i, i.MassTraces, i.RawFile, i.Details)
//                      }
//                |> Seq.map (fun (feature, _, _, _) -> feature)
//                |> (fun feature -> 
//                    if (Seq.exists (fun feature' -> feature' <> null) feature) = false
//                        then 
//                            query {
//                                   for i in dbContext.Feature do
//                                       if i.MZ=mz
//                                          then select (i, i.MassTraces, i.RawFile, i.Details)
//                                  }
//                            |> Seq.map (fun (feature, _, _, _) -> feature)
//                            |> (fun feature -> if (Seq.exists (fun feature' -> feature' <> null) feature) = false
//                                                            then None
//                                                            else Some feature
//                               )
//                        else Some feature
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:Feature) (item2:Feature) =
//                item1.Charge=item2.Charge && item1.RetentionTime=item2.RetentionTime &&
//                item1.FKChromatogram=item2.FKChromatogram && item1.RawFile=item2.RawFile &&
//                item1.FKSpectrum=item2.FKSpectrum && item1.MassTraces=item2.MassTraces

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:Feature) =
//                    FeatureHandler.tryFindByOrder dbContext item.MZ
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match FeatureHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:Feature) =
//                FeatureHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type SmallMoleculeHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    ?id                          : string,
//                    ?modifications               : seq<Modification>,
//                    ?dbIdentificationRefs        : seq<DBIdentificationRef>,
//                    ?features                    : seq<Feature>,
//                    ?details                     : seq<SmallMoleculeParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let modifications'               = convertOptionToList modifications
//                let dbIdentificationRefs'        = convertOptionToList dbIdentificationRefs
//                let features'                    = convertOptionToList features
//                let details'                     = convertOptionToList details
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new SmallMolecule(
//                                  id', 
//                                  modifications',
//                                  dbIdentificationRefs',
//                                  features',                                  
//                                  details',
//                                  Nullable(DateTime.Now)
//                                 )
            
//            ///Adds a modification to an existing object.
//            static member addModification (modification:Modification) (table:SmallMolecule) =
//                let result = table.Modifications <- addToList table.Modifications modification
//                table

//            ///Adds a collection of modifications to an existing object.
//            static member addModifications (modifications:seq<Modification>) (table:SmallMolecule) =
//                let result = table.Modifications <- addCollectionToList table.Modifications modifications
//                table

//            ///Adds a dbIdentificationRef to an existing object.
//            static member addDBIdentificationRef (dbIdentificationRef:DBIdentificationRef) (table:SmallMolecule) =
//                let result = table.DBIdentificationRefs <- addToList table.DBIdentificationRefs dbIdentificationRef
//                table

//            ///Adds a collection of dbIdentificationRefs to an existing object.
//            static member addDBIdentificationRefs (dbIdentificationRefs:seq<DBIdentificationRef>) (table:SmallMolecule) =
//                let result = table.DBIdentificationRefs <- addCollectionToList table.DBIdentificationRefs dbIdentificationRefs
//                table

//            ///Adds a feature to an existing object.
//            static member addFeature (feature:Feature) (table:SmallMolecule) =
//                let result = table.Features <- addToList table.Features feature
//                table

//            ///Adds a collection of features to an existing object.
//            static member addFeatures (features:seq<Feature>) (table:SmallMolecule) =
//                let result = table.Features <- addCollectionToList table.Features features
//                table

//            ///Adds a smallMoleculeParam to an existing object.
//            static member addDetail (smallMoleculeParam:SmallMoleculeParam) (table:SmallMolecule) =
//                let result = table.Details <- addToList table.Details smallMoleculeParam
//                table

//            ///Adds a collection of smallMoleculeParams to an existing object.
//            static member addDetails (smallMoleculeParam:seq<SmallMoleculeParam>) (table:SmallMolecule) =
//                let result = table.Details <- addCollectionToList table.Details smallMoleculeParam
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a smallMolecule-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.SmallMolecule.Find(id))

//            ///Tries to find a smallMolecule-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByModifications (dbContext:MzQuantML) (modifications:seq<Modification>) =
//                query {
//                       for i in dbContext.SmallMolecule.Local do
//                           if i.Modifications=(modifications |> List)
//                              then select (i, i.Modifications, i.DBIdentificationRefs, i.Features, i.Details)
//                      }
//                |> Seq.map (fun (smallMolecule, _, _, _, _) -> smallMolecule)
//                |> (fun smallMolecule -> 
//                    if (Seq.exists (fun smallMolecule' -> smallMolecule' <> null) smallMolecule) = false
//                        then 
//                            query {
//                                   for i in dbContext.SmallMolecule do
//                                       if i.Modifications=(modifications |> List)
//                                          then select (i, i.Modifications, i.DBIdentificationRefs, i.Features, i.Details)
//                                  }
//                            |> Seq.map (fun (smallMolecule, _, _, _, _) -> smallMolecule)
//                            |> (fun smallMolecule -> if (Seq.exists (fun smallMolecule' -> smallMolecule' <> null) smallMolecule) = false
//                                                            then None
//                                                            else Some smallMolecule
//                               )
//                        else Some smallMolecule
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:SmallMolecule) (item2:SmallMolecule) =
//                item1.DBIdentificationRefs=item2.DBIdentificationRefs && item1.Features=item2.Features && item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:SmallMolecule) =
//                    SmallMoleculeHandler.tryFindByModifications dbContext item.Modifications
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match SmallMoleculeHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:SmallMolecule) =
//                SmallMoleculeHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type SmallMoleculeListHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    smallMolecules               : seq<SmallMolecule>,
//                    ?id                          : string,
//                    ?globalQuantLayer            : seq<GlobalQuantLayer>,
//                    ?assayQuantLayer             : seq<AssayQuantLayer>,
//                    ?studyVariableQuantLayer     : seq<StudyVariableQuantLayer>,
//                    ?ratioQuantLayer             : RatioQuantLayer,
//                    ?details                     : seq<SmallMoleculeListParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
//                let globalQuantLayer'               = convertOptionToList globalQuantLayer
//                let assayQuantLayer'                = convertOptionToList assayQuantLayer
//                let studyVariableQuantLayer'        = convertOptionToList studyVariableQuantLayer
//                let ratioQuantLayer'                = defaultArg ratioQuantLayer Unchecked.defaultof<RatioQuantLayer>
//                let details'                        = convertOptionToList details
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new SmallMoleculeList(
//                                      id',
//                                      smallMolecules |> List,
//                                      globalQuantLayer',
//                                      assayQuantLayer',
//                                      studyVariableQuantLayer',
//                                      ratioQuantLayer',
//                                      details',
//                                      Nullable(DateTime.Now)
//                                     )
            
//            ///Adds a globalQuantLayer to an existing object.
//            static member addGlobalQuantLayer (globalQuantLayer:GlobalQuantLayer) (table:SmallMoleculeList) =
//                let result = table.GlobalQuantLayers <- addToList table.GlobalQuantLayers globalQuantLayer
//                table

//            ///Adds a collection of globalQuantLayers to an existing object.
//            static member addGlobalQuantLayers (globalQuantLayers:seq<GlobalQuantLayer>) (table:SmallMoleculeList) =
//                let result = table.GlobalQuantLayers <- addCollectionToList table.GlobalQuantLayers globalQuantLayers
//                table

//            ///Adds a assayQuantLayer to an existing object.
//            static member addAssayQuantLayer (assayQuantLayer:AssayQuantLayer) (table:SmallMoleculeList) =
//                let result = table.AssayQuantLayers <- addToList table.AssayQuantLayers assayQuantLayer
//                table

//            ///Adds a collection of assayQuantLayers to an existing object.
//            static member addAssayQuantLayers (assayQuantLayers:seq<AssayQuantLayer>) (table:SmallMoleculeList) =
//                let result = table.AssayQuantLayers <- addCollectionToList table.AssayQuantLayers assayQuantLayers
//                table

//            ///Adds a studyVariableQuantLayer to an existing object.
//            static member addStudyVariableQuantLayer (studyVariableQuantLayer:StudyVariableQuantLayer) (table:SmallMoleculeList) =
//                let result = table.StudyVariableQuantLayers <- addToList table.StudyVariableQuantLayers studyVariableQuantLayer
//                table

//            ///Adds a collection of studyVariableQuantLayers to an existing object.
//            static member addStudyVariableQuantLayers (studyVariableQuantLayers:seq<StudyVariableQuantLayer>) (table:SmallMoleculeList) =
//                let result = table.StudyVariableQuantLayers <- addCollectionToList table.StudyVariableQuantLayers studyVariableQuantLayers
//                table

//            /////Replaces ratioQuantLayer of existing object with new one.
//            static member addRatioQuantLayer (ratioQuantLayer:RatioQuantLayer) (table:SmallMoleculeList) =
//                table.RatioQuantLayer <- ratioQuantLayer
//                table

//            ///Adds a smallMoleculeParam to an existing object.
//            static member addDetail (smallMoleculeParam:SmallMoleculeListParam) (table:SmallMoleculeList) =
//                let result = table.Details <- addToList table.Details smallMoleculeParam
//                table

//            ///Adds a collection of smallMoleculeParams to an existing object.
//            static member addDetails (smallMoleculeParams:seq<SmallMoleculeListParam>) (table:SmallMoleculeList) =
//                let result = table.Details <- addCollectionToList table.Details smallMoleculeParams
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a smallMoleculeList-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.SmallMoleculeList.Find(id))

//            ///Tries to find a smallMoleculeList-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindBySmallMolecules (dbContext:MzQuantML) (smallMolecules:seq<SmallMolecule>) =
//                query {
//                       for i in dbContext.SmallMoleculeList.Local do
//                           if i.SmallMolecules=(smallMolecules |> List)
//                              then select (i, i.SmallMolecules, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
//                      }
//                |> Seq.map (fun (smallMoleculeList, _, _, _, _, _, _) -> smallMoleculeList)
//                |> (fun smallMoleculeList -> 
//                    if (Seq.exists (fun smallMoleculeList' -> smallMoleculeList' <> null) smallMoleculeList) = false
//                        then 
//                            query {
//                                   for i in dbContext.SmallMoleculeList do
//                                       if i.SmallMolecules=(smallMolecules |> List)
//                                          then select (i, i.SmallMolecules, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
//                                  }
//                            |> Seq.map (fun (smallMoleculeList, _, _, _, _, _, _) -> smallMoleculeList)
//                            |> (fun smallMoleculeList -> if (Seq.exists (fun smallMoleculeList' -> smallMoleculeList' <> null) smallMoleculeList) = false
//                                                            then None
//                                                            else Some smallMoleculeList
//                               )
//                        else Some smallMoleculeList
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:SmallMoleculeList) (item2:SmallMoleculeList) =
//                item1.GlobalQuantLayers=item2.GlobalQuantLayers && item1.AssayQuantLayers=item2.AssayQuantLayers && 
//                item1.StudyVariableQuantLayers=item2.StudyVariableQuantLayers && item1.RatioQuantLayer=item2.RatioQuantLayer &&
//                item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:SmallMoleculeList) =
//                    SmallMoleculeListHandler.tryFindBySmallMolecules dbContext item.SmallMolecules
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match SmallMoleculeListHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:SmallMoleculeList) =
//                SmallMoleculeListHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type FeatureQuantLayerHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    columns                      : seq<Column>,
//                    dataMatrix                   : DataMatrix,
//                    ?id                          : string
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new FeatureQuantLayer(
//                                      id',
//                                      columns |> List,
//                                      dataMatrix,
//                                      Nullable(DateTime.Now)
//                                     )

//            ///Tries to find a featureQuantLayer-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.FeatureQuantLayer.Find(id))

//            ///Tries to find a featureQuantLayer-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByDataMatrix (dbContext:MzQuantML) (dataMatrix:DataMatrix) =
//                query {
//                       for i in dbContext.FeatureQuantLayer.Local do
//                           if i.DataMatrix=dataMatrix
//                              then select (i, i.Columns, i.DataMatrix)
//                      }
//                |> Seq.map (fun (featureQuantLayer, _, _) -> featureQuantLayer)
//                |> (fun featureQuantLayer -> 
//                    if (Seq.exists (fun featureQuantLayer' -> featureQuantLayer' <> null) featureQuantLayer) = false
//                        then 
//                            query {
//                                   for i in dbContext.FeatureQuantLayer do
//                                       if i.DataMatrix=dataMatrix
//                                          then select (i, i.Columns, i.DataMatrix)
//                                  }
//                            |> Seq.map (fun (featureQuantLayer, _, _) -> featureQuantLayer)
//                            |> (fun featureQuantLayer -> if (Seq.exists (fun featureQuantLayer' -> featureQuantLayer' <> null) featureQuantLayer) = false
//                                                            then None
//                                                            else Some featureQuantLayer
//                               )
//                        else Some featureQuantLayer
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:FeatureQuantLayer) (item2:FeatureQuantLayer) =
//                item1.Columns=item2.Columns

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:FeatureQuantLayer) =
//                    FeatureQuantLayerHandler.tryFindByDataMatrix dbContext item.DataMatrix
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match FeatureQuantLayerHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:FeatureQuantLayer) =
//                FeatureQuantLayerHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type MS2RatioQuantLayerHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    dataType                     : CVParam,
//                    columnIndex                  : string,
//                    dataMatrix                   : DataMatrix,
//                    ?id                          : string
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new MS2RatioQuantLayer(
//                                       id',
//                                       dataType,
//                                       columnIndex,
//                                       dataMatrix,
//                                       Nullable(DateTime.Now)
//                                      )

//            ///Tries to find a ms2RatioQuantLayer-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.MS2RatioQuantLayer.Find(id))

//            ///Tries to find a ms2RatioQuantLayer-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
//                query {
//                       for i in dbContext.MS2RatioQuantLayer.Local do
//                           if i.ColumnIndex=columnIndex
//                              then select (i, i.DataType, i.DataMatrix)
//                      }
//                |> Seq.map (fun (ms2RatioQuantLayer, _, _) -> ms2RatioQuantLayer)
//                |> (fun ms2RatioQuantLayer -> 
//                    if (Seq.exists (fun ms2RatioQuantLayer' -> ms2RatioQuantLayer' <> null) ms2RatioQuantLayer) = false
//                        then 
//                            query {
//                                   for i in dbContext.MS2RatioQuantLayer do
//                                       if i.ColumnIndex=columnIndex
//                                          then select (i, i.DataType, i.DataMatrix)
//                                  }
//                            |> Seq.map (fun (ms2RatioQuantLayer, _, _) -> ms2RatioQuantLayer)
//                            |> (fun ms2RatioQuantLayer -> if (Seq.exists (fun ms2RatioQuantLayer' -> ms2RatioQuantLayer' <> null) ms2RatioQuantLayer) = false
//                                                            then None
//                                                            else Some ms2RatioQuantLayer
//                               )
//                        else Some ms2RatioQuantLayer
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:MS2RatioQuantLayer) (item2:MS2RatioQuantLayer) =
//                item1.DataType=item2.DataType && item1.DataMatrix=item2.DataMatrix

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:MS2RatioQuantLayer) =
//                    MS2RatioQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match MS2RatioQuantLayerHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:MS2RatioQuantLayer) =
//                MS2RatioQuantLayerHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type MS2StudyVariableQuantLayerHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    dataType                     : CVParam,
//                    columnIndex                  : string,
//                    dataMatrix                   : DataMatrix,
//                    ?id                          : string
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                //let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new MS2StudyVariableQuantLayer(
//                                               id',
//                                               dataType,
//                                               columnIndex,
//                                               dataMatrix,
//                                               Nullable(DateTime.Now)
//                                              )

//            ///Tries to find a ms2StudyVariableQuantLayer-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.MS2StudyVariableQuantLayer.Find(id))

//            ///Tries to find a ms2StudyVariableQuantLayer-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByColumnIndex (dbContext:MzQuantML) (columnIndex:string) =
//                query {
//                       for i in dbContext.MS2StudyVariableQuantLayer.Local do
//                           if i.ColumnIndex=columnIndex
//                              then select (i, i.DataType, i.DataMatrix)
//                      }
//                |> Seq.map (fun (ms2RatioQuantLayer, _, _) -> ms2RatioQuantLayer)
//                |> (fun ms2StudyVariableQuantLayer -> 
//                    if (Seq.exists (fun ms2StudyVariableQuantLayer' -> ms2StudyVariableQuantLayer' <> null) ms2StudyVariableQuantLayer) = false
//                        then 
//                            query {
//                                   for i in dbContext.MS2StudyVariableQuantLayer do
//                                       if i.ColumnIndex=columnIndex
//                                          then select (i, i.DataType, i.DataMatrix)
//                                  }
//                            |> Seq.map (fun (ms2StudyVariableQuantLayer, _, _) -> ms2StudyVariableQuantLayer)
//                            |> (fun ms2StudyVariableQuantLayer -> if (Seq.exists (fun ms2StudyVariableQuantLayer' -> ms2StudyVariableQuantLayer' <> null) ms2StudyVariableQuantLayer) = false
//                                                                    then None
//                                                                    else Some ms2StudyVariableQuantLayer
//                               )
//                        else Some ms2StudyVariableQuantLayer
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:MS2StudyVariableQuantLayer) (item2:MS2StudyVariableQuantLayer) =
//                item1.DataType=item2.DataType && item1.DataMatrix=item2.DataMatrix

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:MS2StudyVariableQuantLayer) =
//                    MS2StudyVariableQuantLayerHandler.tryFindByColumnIndex dbContext item.ColumnIndex
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match MS2StudyVariableQuantLayerHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:MS2StudyVariableQuantLayer) =
//                MS2StudyVariableQuantLayerHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type FeatureListHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    rawFilesGroup                : RawFilesGroup,
//                    features                     : seq<Feature>,
//                    ?id                          : string,
//                    ?featureQuantLayers          : seq<FeatureQuantLayer>,
//                    ?ms2AssayQuantLayers         : seq<MS2AssayQuantLayer>,
//                    ?ms2StudyVariableQuantLayer  : seq<MS2StudyVariableQuantLayer>,
//                    ?ms2RatioQuantLayer          : seq<MS2RatioQuantLayer>,
//                    ?details                     : seq<FeatureListParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let featureQuantLayers'          = convertOptionToList featureQuantLayers
//                let ms2AssayQuantLayers'         = convertOptionToList ms2AssayQuantLayers
//                let ms2StudyVariableQuantLayer'  = convertOptionToList ms2StudyVariableQuantLayer
//                let ms2RatioQuantLayer'          = convertOptionToList ms2RatioQuantLayer
//                let details'                     = convertOptionToList details
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new FeatureList(
//                                id', 
//                                rawFilesGroup,
//                                features |> List,
//                                featureQuantLayers', 
//                                ms2AssayQuantLayers',
//                                ms2StudyVariableQuantLayer',
//                                ms2RatioQuantLayer',
//                                details',
//                                Nullable(DateTime.Now)
//                               )
            
//            ///Adds a featureQuantLayer to an existing object.
//            static member addModification (featureQuantLayer:FeatureQuantLayer) (table:FeatureList) =
//                let result = table.FeatureQuantLayers <- addToList table.FeatureQuantLayers featureQuantLayer
//                table

//            ///Adds a collection of featureQuantLayers to an existing object.
//            static member addModifications (featureQuantLayers:seq<FeatureQuantLayer>) (table:FeatureList) =
//                let result = table.FeatureQuantLayers <- addCollectionToList table.FeatureQuantLayers featureQuantLayers
//                table

//            ///Adds a ms2AssayQuantLayer to an existing object.
//            static member addMS2AssayQuantLayer (ms2AssayQuantLayer:MS2AssayQuantLayer) (table:FeatureList) =
//                let result = table.MS2AssayQuantLayers <- addToList table.MS2AssayQuantLayers ms2AssayQuantLayer
//                table

//            ///Adds a collection of ms2AssayQuantLayers to an existing object.
//            static member addMS2AssayQuantLayers (ms2AssayQuantLayers:seq<MS2AssayQuantLayer>) (table:FeatureList) =
//                let result = table.MS2AssayQuantLayers <- addCollectionToList table.MS2AssayQuantLayers ms2AssayQuantLayers
//                table

//            ///Adds a ms2StudyVariableQuantLayer to an existing object.
//            static member addMS2StudyVariableQuantLayer (ms2StudyVariableQuantLayer:MS2StudyVariableQuantLayer) (table:FeatureList) =
//                let result = table.MS2StudyVariableQuantLayers <- addToList table.MS2StudyVariableQuantLayers ms2StudyVariableQuantLayer
//                table

//            ///Adds a collection of ms2StudyVariableQuantLayers to an existing object.
//            static member addMS2StudyVariableQuantLayers (ms2StudyVariableQuantLayers:seq<MS2StudyVariableQuantLayer>) (table:FeatureList) =
//                let result = table.MS2StudyVariableQuantLayers <- addCollectionToList table.MS2StudyVariableQuantLayers ms2StudyVariableQuantLayers
//                table

//            ///Adds a ms2RatioQuantLayer to an existing object.
//            static member addMS2RatioQuantLayer (ms2RatioQuantLayer:MS2RatioQuantLayer) (table:FeatureList) =
//                let result = table.MS2RatioQuantLayers <- addToList table.MS2RatioQuantLayers ms2RatioQuantLayer
//                table

//            ///Adds a collection of ms2RatioQuantLayers to an existing object.
//            static member addMS2RatioQuantLayers (ms2RatioQuantLayers:seq<MS2RatioQuantLayer>) (table:FeatureList) =
//                let result = table.MS2RatioQuantLayers <- addCollectionToList table.MS2RatioQuantLayers ms2RatioQuantLayers
//                table

//            ///Adds a featureListParam to an existing object.
//            static member addDetail (featureListParam:FeatureListParam) (table:FeatureList) =
//                let result = table.Details <- addToList table.Details featureListParam
//                table

//            ///Adds a collection of featureListParams to an existing object.
//            static member addDetails (featureListParams:seq<FeatureListParam>) (table:FeatureList) =
//                let result = table.Details <- addCollectionToList table.Details featureListParams
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a featureList-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.FeatureList.Find(id))

//            ///Tries to find a featureList-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByRawFilesGroup (dbContext:MzQuantML) (rawFilesGroup:RawFilesGroup) =
//                query {
//                       for i in dbContext.FeatureList.Local do
//                           if i.RawFilesGroup=rawFilesGroup
//                              then select (i, i.FeatureQuantLayers, i.MS2AssayQuantLayers, i.MS2StudyVariableQuantLayers, 
//                                           i.MS2RatioQuantLayers, i.RawFilesGroup, i.Features, i.Details
//                                          )
//                      }
//                |> Seq.map (fun (featureList, _, _, _, _, _, _, _) -> featureList)
//                |> (fun featureList -> 
//                    if (Seq.exists (fun featureList' -> featureList' <> null) featureList) = false
//                        then 
//                            query {
//                                   for i in dbContext.FeatureList do
//                                       if i.RawFilesGroup=rawFilesGroup
//                                          then select (i, i.FeatureQuantLayers, i.MS2AssayQuantLayers, i.MS2StudyVariableQuantLayers, 
//                                                       i.MS2RatioQuantLayers, i.RawFilesGroup, i.Features, i.Details
//                                                      )
//                                  }
//                            |> Seq.map (fun (featureList, _, _, _, _, _, _, _) -> featureList)
//                            |> (fun featureList -> if (Seq.exists (fun featureList' -> featureList' <> null) featureList) = false
//                                                            then None
//                                                            else Some featureList
//                               )
//                        else Some featureList
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:FeatureList) (item2:FeatureList) =
//                item1.MS2AssayQuantLayers=item2.MS2AssayQuantLayers && item1.MS2StudyVariableQuantLayers=item2.MS2StudyVariableQuantLayers && 
//                item1.MS2RatioQuantLayers=item2.MS2RatioQuantLayers && item1.Features=item2.Features &&
//                item1.FeatureQuantLayers=item2.FeatureQuantLayers && item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:FeatureList) =
//                    FeatureListHandler.tryFindByRawFilesGroup dbContext item.RawFilesGroup
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match FeatureListHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:FeatureList) =
//                FeatureListHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type EvidenceRefHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    assays                       : seq<Assay>,
//                    feature                      : Feature,
//                    ?id                          : string,
//                    ?fkExternalFileRef           : string,
//                    ?identificationFile          : IdentificationFile
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let fkExternalFileRef'           = defaultArg fkExternalFileRef Unchecked.defaultof<string>
//                let identificationFile'          = defaultArg identificationFile Unchecked.defaultof<IdentificationFile>
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new EvidenceRef(
//                                id', 
//                                assays |> List,
//                                feature,
//                                fkExternalFileRef',
//                                identificationFile',
//                                Nullable(DateTime.Now)
//                               )

//            ///Replaces fkExternalFileRef of existing object with new one.
//            static member addFKExternalFileRef
//                (fkExternalFileRef:string) (table:EvidenceRef) =
//                table.FKExternalFileRef <- fkExternalFileRef
//                table

//            ///Replaces identificationFile of existing object with new one.
//            static member addOutputObjects
//                (identificationFile:IdentificationFile) (table:EvidenceRef) =
//                table.IdentificationFile <- identificationFile
//                table

//            ///Tries to find a evidenceRef-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.EvidenceRef.Find(id))

//            ///Tries to find a evidenceRef-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByFeature (dbContext:MzQuantML) (feature:Feature) =
//                query {
//                       for i in dbContext.EvidenceRef.Local do
//                           if i.Feature=feature
//                              then select (i, i.Feature, i.Assays, i.IdentificationFile)
//                      }
//                |> Seq.map (fun (evidenceRef, _, _, _) -> evidenceRef)
//                |> (fun evidenceRef -> 
//                    if (Seq.exists (fun evidenceRef' -> evidenceRef' <> null) evidenceRef) = false
//                        then 
//                            query {
//                                   for i in dbContext.EvidenceRef do
//                                       if i.Feature=feature
//                                          then select (i, i.Feature, i.Assays, i.IdentificationFile)
//                                  }
//                            |> Seq.map (fun (evidenceRef, _, _, _) -> evidenceRef)
//                            |> (fun evidenceRef -> if (Seq.exists (fun evidenceRef' -> evidenceRef' <> null) evidenceRef) = false
//                                                            then None
//                                                            else Some evidenceRef
//                               )
//                        else Some evidenceRef
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:EvidenceRef) (item2:EvidenceRef) =
//                item1.Assays=item2.Assays && item1.IdentificationFile=item2.IdentificationFile &&
//                item1.FKExternalFileRef=item2.FKExternalFileRef && item1.IdentificationFile=item2.IdentificationFile

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:EvidenceRef) =
//                    EvidenceRefHandler.tryFindByFeature dbContext item.Feature
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match EvidenceRefHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:EvidenceRef) =
//                EvidenceRefHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type PeptideConsensusHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    charge                       : int,
//                    evidenceRefs                 : seq<EvidenceRef>,
//                    ?id                          : string,
//                    ?searchDatabase              : SearchDatabase,
//                    ?dataMatrix                  : DataMatrix,
//                    ?peptideSequence             : string,
//                    ?modifications               : seq<Modification>,
//                    ?details                     : seq<PeptideConsensusParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let searchDatabase'              = defaultArg searchDatabase Unchecked.defaultof<SearchDatabase>
//                let dataMatrix'                  = defaultArg dataMatrix Unchecked.defaultof<DataMatrix>
//                let peptideSequence'             = defaultArg peptideSequence Unchecked.defaultof<string>
//                let modifications'               = convertOptionToList modifications
//                let details'                     = convertOptionToList details
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new PeptideConsensus(
//                                     id',
//                                     Nullable(charge),
//                                     searchDatabase',
//                                     dataMatrix',
//                                     peptideSequence',
//                                     modifications',
//                                     evidenceRefs |> List,
//                                     details',
//                                     Nullable(DateTime.Now)
//                                    )

//            ///Replaces searchDatabase of existing object with new one.
//            static member addSearchDatabase
//                (searchDatabase:SearchDatabase) (table:PeptideConsensus) =
//                table.SearchDatabase <- searchDatabase
//                table

//            ///Replaces dataMatrix of existing object with new one.
//            static member addDataMatrix
//                (dataMatrix:DataMatrix) (table:PeptideConsensus) =
//                table.DataMatrix <- dataMatrix
//                table

//            ///Replaces peptideSequence of existing object with new one.
//            static member addPeptideSequence
//                (peptideSequence:string) (table:PeptideConsensus) =
//                table.PeptideSequence <- peptideSequence
//                table

//            ///Adds a modification to an existing object.
//            static member addModification (modification:Modification) (table:PeptideConsensus) =
//                let result = table.Modifications <- addToList table.Modifications modification
//                table

//            ///Adds a collection of modifications to an existing object.
//            static member addModifications (modifications:seq<Modification>) (table:PeptideConsensus) =
//                let result = table.Modifications <- addCollectionToList table.Modifications modifications
//                table

//            ///Adds a peptideConsensusParam to an existing object.
//            static member addDetail (detail:PeptideConsensusParam) (table:PeptideConsensus) =
//                let result = table.Details <- addToList table.Details detail
//                table

//            ///Adds a collection of peptideConsensusParams to an existing object.
//            static member addDetails (details:seq<PeptideConsensusParam>) (table:PeptideConsensus) =
//                let result = table.Details <- addCollectionToList table.Details details
//                table

//            ///Tries to find a peptideConsensus-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.PeptideConsensus.Find(id))

//            ///Tries to find a peptideConsensus-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByCharge (dbContext:MzQuantML) (charge:Nullable<int>) =
//                query {
//                       for i in dbContext.PeptideConsensus.Local do
//                           if i.Charge=charge
//                              then select (i, i.EvidenceRefs, i.SearchDatabase, i.DataMatrix, i.Modifications, i.Details)
//                      }
//                |> Seq.map (fun (peptideConsensus, _, _, _, _, _) -> peptideConsensus)
//                |> (fun peptideConsensus -> 
//                    if (Seq.exists (fun peptideConsensus' -> peptideConsensus' <> null) peptideConsensus) = false
//                        then 
//                            query {
//                                   for i in dbContext.PeptideConsensus do
//                                       if i.Charge=charge
//                                          then select (i, i.EvidenceRefs, i.SearchDatabase, i.DataMatrix, i.Modifications, i.Details)
//                                  }
//                            |> Seq.map (fun (peptideConsensus, _, _, _, _, _) -> peptideConsensus)
//                            |> (fun peptideConsensus -> if (Seq.exists (fun peptideConsensus' -> peptideConsensus' <> null) peptideConsensus) = false
//                                                            then None
//                                                            else Some peptideConsensus
//                               )
//                        else Some peptideConsensus
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:PeptideConsensus) (item2:PeptideConsensus) =
//                item1.EvidenceRefs=item2.EvidenceRefs && item1.SearchDatabase=item2.SearchDatabase &&
//                item1.DataMatrix=item2.DataMatrix && item1.Modifications=item2.Modifications &&
//                item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:PeptideConsensus) =
//                    PeptideConsensusHandler.tryFindByCharge dbContext item.Charge
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match PeptideConsensusHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:PeptideConsensus) =
//                PeptideConsensusHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type ProteinHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    accession                    : string,
//                    searchDatabase               : SearchDatabase,
//                    ?id                          : string,
//                    ?identificationRefs          : seq<IdentificationRef>,
//                    ?peptideConsensi             : seq<PeptideConsensus>,
//                    ?details                     : seq<ProteinParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
//                let identificationRefs'          = convertOptionToList identificationRefs
//                let peptideConsensi'             = convertOptionToList peptideConsensi
//                let details'                     = convertOptionToList details
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new Protein(
//                            id',
//                            accession,
//                            searchDatabase,
//                            identificationRefs',
//                            peptideConsensi',
//                            details',
//                            Nullable(DateTime.Now)
//                           )

//            ///Adds a identificationRef to an existing object.
//            static member addIdentificationRef (identificationRef:IdentificationRef) (table:Protein) =
//                let result = table.IdentificationRefs <- addToList table.IdentificationRefs identificationRef
//                table

//            ///Adds a collection of identificationRefs to an existing object.
//            static member addIdentificationRefs (identificationRefs:seq<IdentificationRef>) (table:Protein) =
//                let result = table.IdentificationRefs <- addCollectionToList table.IdentificationRefs identificationRefs
//                table

//            ///Adds a peptideConsensus to an existing object.
//            static member addPeptideConsensus (peptideConsensus:PeptideConsensus) (table:Protein) =
//                let result = table.PeptideConsensi <- addToList table.PeptideConsensi peptideConsensus
//                table

//            ///Adds a collection of peptideConsensi to an existing object.
//            static member addPeptideConsensi (peptideConsensi:seq<PeptideConsensus>) (table:Protein) =
//                let result = table.PeptideConsensi <- addCollectionToList table.PeptideConsensi peptideConsensi
//                table

//            ///Adds a proteinParam to an existing object.
//            static member addDetail (detail:ProteinParam) (table:Protein) =
//                let result = table.Details <- addToList table.Details detail
//                table

//            ///Adds a collection of proteinParams to an existing object.
//            static member addDetails (details:seq<ProteinParam>) (table:Protein) =
//                let result = table.Details <- addCollectionToList table.Details details
//                table

//            ///Tries to find a protein-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.Protein.Find(id))

//            ///Tries to find a protein-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByAccession (dbContext:MzQuantML) (accession:string) =
//                query {
//                       for i in dbContext.Protein.Local do
//                           if i.Accession=accession
//                              then select (i, i.IdentificationRefs, i.SearchDatabase, i.PeptideConsensi, i.Details)
//                      }
//                |> Seq.map (fun (protein, _, _, _, _) -> protein)
//                |> (fun protein -> 
//                    if (Seq.exists (fun protein' -> protein' <> null) protein) = false
//                        then 
//                            query {
//                                   for i in dbContext.Protein do
//                                       if i.Accession=accession
//                                          then select (i, i.IdentificationRefs, i.SearchDatabase, i.PeptideConsensi, i.Details)
//                                  }
//                            |> Seq.map (fun (protein, _, _, _, _) -> protein)
//                            |> (fun protein -> if (Seq.exists (fun protein' -> protein' <> null) protein) = false
//                                                            then None
//                                                            else Some protein
//                               )
//                        else Some protein
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:Protein) (item2:Protein) =
//                item1.IdentificationRefs=item2.IdentificationRefs && item1.SearchDatabase=item2.SearchDatabase &&
//                item1.PeptideConsensi=item2.PeptideConsensi && item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:Protein) =
//                    ProteinHandler.tryFindByAccession dbContext item.Accession
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match ProteinHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:Protein) =
//                ProteinHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type ProteinListHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    proteins                     : seq<Protein>,
//                    ?id                          : string,
//                    ?globalQuantLayer            : seq<GlobalQuantLayer>,
//                    ?assayQuantLayer             : seq<AssayQuantLayer>,
//                    ?studyVariableQuantLayer     : seq<StudyVariableQuantLayer>,
//                    ?ratioQuantLayer             : RatioQuantLayer,
//                    ?details                     : seq<ProteinListParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
//                let globalQuantLayer'               = convertOptionToList globalQuantLayer
//                let assayQuantLayer'                = convertOptionToList assayQuantLayer
//                let studyVariableQuantLayer'        = convertOptionToList studyVariableQuantLayer
//                let ratioQuantLayer'                = defaultArg ratioQuantLayer Unchecked.defaultof<RatioQuantLayer>
//                let details'                        = convertOptionToList details
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new ProteinList(
//                                id',
//                                proteins |> List,
//                                globalQuantLayer',
//                                assayQuantLayer',
//                                studyVariableQuantLayer',
//                                ratioQuantLayer',
//                                details',
//                                Nullable(DateTime.Now)
//                               )
            
//            ///Adds a globalQuantLayer to an existing object.
//            static member addGlobalQuantLayer (globalQuantLayer:GlobalQuantLayer) (table:ProteinList) =
//                let result = table.GlobalQuantLayers <- addToList table.GlobalQuantLayers globalQuantLayer
//                table

//            ///Adds a collection of globalQuantLayers to an existing object.
//            static member addGlobalQuantLayers (globalQuantLayers:seq<GlobalQuantLayer>) (table:ProteinList) =
//                let result = table.GlobalQuantLayers <- addCollectionToList table.GlobalQuantLayers globalQuantLayers
//                table

//            ///Adds a assayQuantLayer to an existing object.
//            static member addAssayQuantLayer (assayQuantLayer:AssayQuantLayer) (table:ProteinList) =
//                let result = table.AssayQuantLayers <- addToList table.AssayQuantLayers assayQuantLayer
//                table

//            ///Adds a collection of assayQuantLayers to an existing object.
//            static member addAssayQuantLayers (assayQuantLayers:seq<AssayQuantLayer>) (table:ProteinList) =
//                let result = table.AssayQuantLayers <- addCollectionToList table.AssayQuantLayers assayQuantLayers
//                table

//            ///Adds a studyVariableQuantLayer to an existing object.
//            static member addStudyVariableQuantLayer (studyVariableQuantLayer:StudyVariableQuantLayer) (table:ProteinList) =
//                let result = table.StudyVariableQuantLayers <- addToList table.StudyVariableQuantLayers studyVariableQuantLayer
//                table

//            ///Adds a collection of studyVariableQuantLayers to an existing object.
//            static member addStudyVariableQuantLayers (studyVariableQuantLayers:seq<StudyVariableQuantLayer>) (table:ProteinList) =
//                let result = table.StudyVariableQuantLayers <- addCollectionToList table.StudyVariableQuantLayers studyVariableQuantLayers
//                table

//            /////Replaces ratioQuantLayer of existing object with new one.
//            static member addRatioQuantLayer (ratioQuantLayer:RatioQuantLayer) (table:ProteinList) =
//                table.RatioQuantLayer <- ratioQuantLayer
//                table

//            ///Adds a proteinListParam to an existing object.
//            static member addDetail (detail:ProteinListParam) (table:ProteinList) =
//                let result = table.Details <- addToList table.Details detail
//                table

//            ///Adds a collection of proteinListParams to an existing object.
//            static member addDetails (details:seq<ProteinListParam>) (table:ProteinList) =
//                let result = table.Details <- addCollectionToList table.Details details
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a proteinList-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.ProteinList.Find(id))

//            ///Tries to find a proteinList-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByProteins (dbContext:MzQuantML) (proteins:seq<Protein>) =
//                query {
//                       for i in dbContext.ProteinList.Local do
//                           if i.Proteins=(proteins |> List)
//                              then select (i, i.Proteins, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
//                      }
//                |> Seq.map (fun (proteinList, _, _, _, _, _, _) -> proteinList)
//                |> (fun proteinList -> 
//                    if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
//                        then 
//                            query {
//                                   for i in dbContext.ProteinList do
//                                       if i.Proteins=(proteins |> List)
//                                          then select (i, i.Proteins, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
//                                  }
//                            |> Seq.map (fun (proteinList, _, _, _, _, _, _) -> proteinList)
//                            |> (fun proteinList -> if (Seq.exists (fun proteinList' -> proteinList' <> null) proteinList) = false
//                                                            then None
//                                                            else Some proteinList
//                               )
//                        else Some proteinList
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:ProteinList) (item2:ProteinList) =
//                item1.GlobalQuantLayers=item2.GlobalQuantLayers && item1.AssayQuantLayers=item2.AssayQuantLayers && 
//                item1.StudyVariableQuantLayers=item2.StudyVariableQuantLayers && item1.RatioQuantLayer=item2.RatioQuantLayer &&
//                item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:ProteinList) =
//                    ProteinListHandler.tryFindByProteins dbContext item.Proteins
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match ProteinListHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinList) =
//                ProteinListHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type ProteinRefHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    protein                      : Protein,
//                    ?id                          : string,
//                    ?details                     : seq<ProteinRefParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
//                let details'                        = convertOptionToList details
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new ProteinRef(
//                               id',
//                               protein,
//                               details',
//                               Nullable(DateTime.Now)
//                              )  

//            ///Adds a proteinRefParam to an existing object.
//            static member addDetail (detail:ProteinRefParam) (table:ProteinRef) =
//                let result = table.Details <- addToList table.Details detail
//                table

//            ///Adds a collection of proteinRefParams to an existing object.
//            static member addDetails (details:seq<ProteinRefParam>) (table:ProteinRef) =
//                let result = table.Details <- addCollectionToList table.Details details
//                table

//            ///Tries to find a proteinRef-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.ProteinRef.Find(id))

//            ///Tries to find a proteinRef-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByProtein (dbContext:MzQuantML) (protein:Protein) =
//                query {
//                       for i in dbContext.ProteinRef.Local do
//                           if i.Protein=protein
//                              then select (i, i.Protein, i.Details)
//                      }
//                |> Seq.map (fun (proteinRef, _, _) -> proteinRef)
//                |> (fun proteinRef -> 
//                    if (Seq.exists (fun proteinRef' -> proteinRef' <> null) proteinRef) = false
//                        then 
//                            query {
//                                   for i in dbContext.ProteinRef do
//                                       if i.Protein=protein
//                                          then select (i, i.Protein, i.Details)
//                                  }
//                            |> Seq.map (fun (proteinRef, _, _) -> proteinRef)
//                            |> (fun proteinRef -> if (Seq.exists (fun proteinRef' -> proteinRef' <> null) proteinRef) = false
//                                                            then None
//                                                            else Some proteinRef
//                               )
//                        else Some proteinRef
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:ProteinRef) (item2:ProteinRef) =
//                item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:ProteinRef) =
//                    ProteinRefHandler.tryFindByProtein dbContext item.Protein
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match ProteinRefHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinRef) =
//                ProteinRefHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type ProteinGroupHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    searchDatabase               : SearchDatabase,
//                    proteinRefs                  : seq<ProteinRef>,
//                    ?id                          : string,
//                    ?identificationRefs          : seq<IdentificationRef>,
//                    ?details                     : seq<ProteinGroupParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
//                let identificationRefs'             = convertOptionToList identificationRefs
//                let details'                        = convertOptionToList details
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new ProteinGroup(
//                                 id',
//                                 searchDatabase,
//                                 identificationRefs',
//                                 proteinRefs |> List,
//                                 details',
//                                 Nullable(DateTime.Now)
//                                )  

//            ///Adds a proteinGroupParam to an existing object.
//            static member addDetail (detail:ProteinGroupParam) (table:ProteinGroup) =
//                let result = table.Details <- addToList table.Details detail
//                table

//            ///Adds a collection of proteinGroupParams to an existing object.
//            static member addDetails (details:seq<ProteinGroupParam>) (table:ProteinGroup) =
//                let result = table.Details <- addCollectionToList table.Details details
//                table

//            ///Tries to find a proteinGroup-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.ProteinGroup.Find(id))

//            ///Tries to find a proteinGroup-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindBySearchDatabase (dbContext:MzQuantML) (searchDatabase:SearchDatabase) =
//                query {
//                       for i in dbContext.ProteinGroup.Local do
//                           if i.SearchDatabase=searchDatabase
//                              then select (i, i.ProteinRefs, i.IdentificationRefs, i.SearchDatabase, i.Details)
//                      }
//                |> Seq.map (fun (proteinGroup, _, _, _, _) -> proteinGroup)
//                |> (fun proteinGroup -> 
//                    if (Seq.exists (fun proteinGroup' -> proteinGroup' <> null) proteinGroup) = false
//                        then 
//                            query {
//                                   for i in dbContext.ProteinGroup do
//                                       if i.SearchDatabase=searchDatabase
//                                          then select (i, i.ProteinRefs, i.IdentificationRefs, i.SearchDatabase, i.Details)
//                                  }
//                            |> Seq.map (fun (proteinGroup, _, _, _, _) -> proteinGroup)
//                            |> (fun proteinGroup -> if (Seq.exists (fun proteinGroup' -> proteinGroup' <> null) proteinGroup) = false
//                                                            then None
//                                                            else Some proteinGroup
//                               )
//                        else Some proteinGroup
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:ProteinGroup) (item2:ProteinGroup) =
//                item1.ProteinRefs=item2.ProteinRefs && item1.IdentificationRefs=item2.IdentificationRefs && 
//                item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:ProteinGroup) =
//                    ProteinGroupHandler.tryFindBySearchDatabase dbContext item.SearchDatabase
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match ProteinGroupHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinGroup) =
//                ProteinGroupHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type ProteinGroupListHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    proteinGroups                : seq<ProteinGroup>,
//                    ?id                          : string,
//                    ?globalQuantLayer            : seq<GlobalQuantLayer>,
//                    ?assayQuantLayer             : seq<AssayQuantLayer>,
//                    ?studyVariableQuantLayer     : seq<StudyVariableQuantLayer>,
//                    ?ratioQuantLayer             : RatioQuantLayer,
//                    ?details                     : seq<ProteinGroupListParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
//                let globalQuantLayer'               = convertOptionToList globalQuantLayer
//                let assayQuantLayer'                = convertOptionToList assayQuantLayer
//                let studyVariableQuantLayer'        = convertOptionToList studyVariableQuantLayer
//                let ratioQuantLayer'                = defaultArg ratioQuantLayer Unchecked.defaultof<RatioQuantLayer>
//                let details'                        = convertOptionToList details
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new ProteinGroupList(
//                                     id',
//                                     proteinGroups |> List,
//                                     globalQuantLayer',
//                                     assayQuantLayer',
//                                     studyVariableQuantLayer',
//                                     ratioQuantLayer',
//                                     details',
//                                     Nullable(DateTime.Now)
//                                    )
            
//            ///Adds a globalQuantLayer to an existing object.
//            static member addGlobalQuantLayer (globalQuantLayer:GlobalQuantLayer) (table:ProteinGroupList) =
//                let result = table.GlobalQuantLayers <- addToList table.GlobalQuantLayers globalQuantLayer
//                table

//            ///Adds a collection of globalQuantLayers to an existing object.
//            static member addGlobalQuantLayers (globalQuantLayers:seq<GlobalQuantLayer>) (table:ProteinGroupList) =
//                let result = table.GlobalQuantLayers <- addCollectionToList table.GlobalQuantLayers globalQuantLayers
//                table

//            ///Adds a assayQuantLayer to an existing object.
//            static member addAssayQuantLayer (assayQuantLayer:AssayQuantLayer) (table:ProteinGroupList) =
//                let result = table.AssayQuantLayers <- addToList table.AssayQuantLayers assayQuantLayer
//                table

//            ///Adds a collection of assayQuantLayers to an existing object.
//            static member addAssayQuantLayers (assayQuantLayers:seq<AssayQuantLayer>) (table:ProteinGroupList) =
//                let result = table.AssayQuantLayers <- addCollectionToList table.AssayQuantLayers assayQuantLayers
//                table

//            ///Adds a studyVariableQuantLayer to an existing object.
//            static member addStudyVariableQuantLayer (studyVariableQuantLayer:StudyVariableQuantLayer) (table:ProteinGroupList) =
//                let result = table.StudyVariableQuantLayers <- addToList table.StudyVariableQuantLayers studyVariableQuantLayer
//                table

//            ///Adds a collection of studyVariableQuantLayers to an existing object.
//            static member addStudyVariableQuantLayers (studyVariableQuantLayers:seq<StudyVariableQuantLayer>) (table:ProteinGroupList) =
//                let result = table.StudyVariableQuantLayers <- addCollectionToList table.StudyVariableQuantLayers studyVariableQuantLayers
//                table

//            /////Replaces ratioQuantLayer of existing object with new one.
//            static member addRatioQuantLayer (ratioQuantLayer:RatioQuantLayer) (table:ProteinGroupList) =
//                table.RatioQuantLayer <- ratioQuantLayer
//                table

//            ///Adds a smallMoleculeParam to an existing object.
//            static member addDetail (smallMoleculeParam:ProteinGroupListParam) (table:ProteinGroupList) =
//                let result = table.Details <- addToList table.Details smallMoleculeParam
//                table

//            ///Adds a collection of smallMoleculeParams to an existing object.
//            static member addDetails (smallMoleculeParams:seq<ProteinGroupListParam>) (table:ProteinGroupList) =
//                let result = table.Details <- addCollectionToList table.Details smallMoleculeParams
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a proteinGroupList-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.ProteinGroupList.Find(id))

//            ///Tries to find a proteinGroupList-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindBySmallMolecules (dbContext:MzQuantML) (proteinGroup:seq<ProteinGroup>) =
//                query {
//                       for i in dbContext.ProteinGroupList.Local do
//                           if i.ProteinGroups=(proteinGroup |> List)
//                              then select (i, i.ProteinGroups, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
//                      }
//                |> Seq.map (fun (smallMoleculeList, _, _, _, _, _, _) -> smallMoleculeList)
//                |> (fun smallMoleculeList -> 
//                    if (Seq.exists (fun smallMoleculeList' -> smallMoleculeList' <> null) smallMoleculeList) = false
//                        then 
//                            query {
//                                   for i in dbContext.ProteinGroupList do
//                                       if i.ProteinGroups=(proteinGroup |> List)
//                                          then select (i, i.ProteinGroups, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
//                                  }
//                            |> Seq.map (fun (smallMoleculeList, _, _, _, _, _, _) -> smallMoleculeList)
//                            |> (fun smallMoleculeList -> if (Seq.exists (fun smallMoleculeList' -> smallMoleculeList' <> null) smallMoleculeList) = false
//                                                            then None
//                                                            else Some smallMoleculeList
//                               )
//                        else Some smallMoleculeList
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:ProteinGroupList) (item2:ProteinGroupList) =
//                item1.GlobalQuantLayers=item2.GlobalQuantLayers && item1.AssayQuantLayers=item2.AssayQuantLayers && 
//                item1.StudyVariableQuantLayers=item2.StudyVariableQuantLayers && item1.RatioQuantLayer=item2.RatioQuantLayer &&
//                item1.Details=item2.Details

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:ProteinGroupList) =
//                    ProteinGroupListHandler.tryFindBySmallMolecules dbContext item.ProteinGroups
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match ProteinGroupListHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:ProteinGroupList) =
//                ProteinGroupListHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type PeptideConsensusListHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    finalResult                  : bool,
//                    peptideConsensi              : seq<PeptideConsensus>,
//                    ?id                          : string,
//                    ?globalQuantLayer            : seq<GlobalQuantLayer>,
//                    ?assayQuantLayer             : seq<AssayQuantLayer>,
//                    ?studyVariableQuantLayer     : seq<StudyVariableQuantLayer>,
//                    ?ratioQuantLayer             : RatioQuantLayer,
//                    ?details                     : seq<PeptideConsensusListParam>
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
//                let globalQuantLayer'               = convertOptionToList globalQuantLayer
//                let assayQuantLayer'                = convertOptionToList assayQuantLayer
//                let studyVariableQuantLayer'        = convertOptionToList studyVariableQuantLayer
//                let ratioQuantLayer'                = defaultArg ratioQuantLayer Unchecked.defaultof<RatioQuantLayer>
//                let details'                        = convertOptionToList details
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new PeptideConsensusList(
//                                         id',
//                                         Nullable(finalResult),
//                                         peptideConsensi |> List,
//                                         globalQuantLayer',
//                                         assayQuantLayer',
//                                         studyVariableQuantLayer',
//                                         ratioQuantLayer',
//                                         details',
//                                         Nullable(DateTime.Now)
//                                        )
            
//            ///Adds a globalQuantLayer to an existing object.
//            static member addGlobalQuantLayer (globalQuantLayer:GlobalQuantLayer) (table:PeptideConsensusList) =
//                let result = table.GlobalQuantLayers <- addToList table.GlobalQuantLayers globalQuantLayer
//                table

//            ///Adds a collection of globalQuantLayers to an existing object.
//            static member addGlobalQuantLayers (globalQuantLayers:seq<GlobalQuantLayer>) (table:PeptideConsensusList) =
//                let result = table.GlobalQuantLayers <- addCollectionToList table.GlobalQuantLayers globalQuantLayers
//                table

//            ///Adds a assayQuantLayer to an existing object.
//            static member addAssayQuantLayer (assayQuantLayer:AssayQuantLayer) (table:PeptideConsensusList) =
//                let result = table.AssayQuantLayers <- addToList table.AssayQuantLayers assayQuantLayer
//                table

//            ///Adds a collection of assayQuantLayers to an existing object.
//            static member addAssayQuantLayers (assayQuantLayers:seq<AssayQuantLayer>) (table:PeptideConsensusList) =
//                let result = table.AssayQuantLayers <- addCollectionToList table.AssayQuantLayers assayQuantLayers
//                table

//            ///Adds a studyVariableQuantLayer to an existing object.
//            static member addStudyVariableQuantLayer (studyVariableQuantLayer:StudyVariableQuantLayer) (table:PeptideConsensusList) =
//                let result = table.StudyVariableQuantLayers <- addToList table.StudyVariableQuantLayers studyVariableQuantLayer
//                table

//            ///Adds a collection of studyVariableQuantLayers to an existing object.
//            static member addStudyVariableQuantLayers (studyVariableQuantLayers:seq<StudyVariableQuantLayer>) (table:PeptideConsensusList) =
//                let result = table.StudyVariableQuantLayers <- addCollectionToList table.StudyVariableQuantLayers studyVariableQuantLayers
//                table

//            /////Replaces ratioQuantLayer of existing object with new one.
//            static member addRatioQuantLayer (ratioQuantLayer:RatioQuantLayer) (table:PeptideConsensusList) =
//                table.RatioQuantLayer <- ratioQuantLayer
//                table

//            ///Adds a peptideConsensusListParam to an existing object.
//            static member addDetail (peptideConsensusListParam:PeptideConsensusListParam) (table:PeptideConsensusList) =
//                let result = table.Details <- addToList table.Details peptideConsensusListParam
//                table

//            ///Adds a collection of peptideConsensusListParams to an existing object.
//            static member addDetails (peptideConsensusListParams:seq<PeptideConsensusListParam>) (table:PeptideConsensusList) =
//                let result = table.Details <- addCollectionToList table.Details peptideConsensusListParams
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a peptideConsensusList-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.PeptideConsensusList.Find(id))

//            ///Tries to find a peptideConsensusList-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindBySmallMolecules (dbContext:MzQuantML) (finalResult:Nullable<bool>) =
//                query {
//                       for i in dbContext.PeptideConsensusList.Local do
//                           if i.FinalResult=finalResult
//                              then select (i, i.PeptideConsensi, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
//                      }
//                |> Seq.map (fun (peptideConsensi, _, _, _, _, _, _) -> peptideConsensi)
//                |> (fun peptideConsensi -> 
//                    if (Seq.exists (fun peptideConsensi' -> peptideConsensi' <> null) peptideConsensi) = false
//                        then 
//                            query {
//                                   for i in dbContext.PeptideConsensusList do
//                                       if i.FinalResult=finalResult
//                                          then select (i, i.PeptideConsensi, i.GlobalQuantLayers, i.AssayQuantLayers, i.StudyVariableQuantLayers, i.RatioQuantLayer, i.Details)
//                                  }
//                            |> Seq.map (fun (peptideConsensi, _, _, _, _, _, _) -> peptideConsensi)
//                            |> (fun peptideConsensi -> if (Seq.exists (fun peptideConsensi' -> peptideConsensi' <> null) peptideConsensi) = false
//                                                            then None
//                                                            else Some peptideConsensi
//                               )
//                        else Some peptideConsensi
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:PeptideConsensusList) (item2:PeptideConsensusList) =
//                item1.GlobalQuantLayers=item2.GlobalQuantLayers && item1.AssayQuantLayers=item2.AssayQuantLayers && 
//                item1.StudyVariableQuantLayers=item2.StudyVariableQuantLayers && item1.RatioQuantLayer=item2.RatioQuantLayer &&
//                item1.Details=item2.Details && item1.PeptideConsensi=item2.PeptideConsensi

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:PeptideConsensusList) =
//                    PeptideConsensusListHandler.tryFindBySmallMolecules dbContext item.FinalResult
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match PeptideConsensusListHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:PeptideConsensusList) =
//                PeptideConsensusListHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type BiblioGraphicReferenceHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    ?id                          : string,
//                    ?name                        : string,
//                    ?authors                     : string,
//                    ?doi                         : string,
//                    ?editor                      : string,
//                    ?issue                       : string,
//                    ?pages                       : string,
//                    ?publication                 : string,
//                    ?publisher                   : string,
//                    ?title                       : string,
//                    ?volume                      : string,
//                    ?year                        : int
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
//                let name'                           = defaultArg name Unchecked.defaultof<string>
//                let authors'                        = defaultArg authors Unchecked.defaultof<string>
//                let doi'                            = defaultArg doi Unchecked.defaultof<string>
//                let editor'                         = defaultArg editor Unchecked.defaultof<string>
//                let issue'                          = defaultArg issue Unchecked.defaultof<string>
//                let pages'                          = defaultArg pages Unchecked.defaultof<string>
//                let publication'                    = defaultArg publication Unchecked.defaultof<string>
//                let publisher'                      = defaultArg publisher Unchecked.defaultof<string>
//                let title'                          = defaultArg title Unchecked.defaultof<string>
//                let volume'                         = defaultArg volume Unchecked.defaultof<string>
//                let year'                           = defaultArg year Unchecked.defaultof<int>
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new BiblioGraphicReference(
//                                           id',
//                                           name',
//                                           authors',
//                                           doi',
//                                           editor',
//                                           issue',
//                                           pages',
//                                           publication',
//                                           publisher',
//                                           title',
//                                           volume',
//                                           Nullable(year'),
//                                           Nullable(DateTime.Now)
//                                          ) 

//            /////Replaces name of existing object with new one.
//            static member addName (name:string) (table:BiblioGraphicReference) =
//                table.Name <- name
//                table

//            /////Replaces authors of existing object with new one.
//            static member addAuthors (authors:string) (table:BiblioGraphicReference) =
//                table.Authors <- authors
//                table

//            /////Replaces doi of existing object with new one.
//            static member addDOI (doi:string) (table:BiblioGraphicReference) =
//                table.DOI <- doi
//                table

//            /////Replaces editor of existing object with new one.
//            static member addEditor (editor:string) (table:BiblioGraphicReference) =
//                table.Editor <- editor
//                table

//            /////Replaces issue of existing object with new one.
//            static member addIssue (issue:string) (table:BiblioGraphicReference) =
//                table.Issue <- issue
//                table

//            /////Replaces pages of existing object with new one.
//            static member addPages (pages:string) (table:BiblioGraphicReference) =
//                table.Pages <- pages
//                table

//            /////Replaces publication of existing object with new one.
//            static member addPublication (publication:string) (table:BiblioGraphicReference) =
//                table.Publication <- publication
//                table

//            /////Replaces publisher of existing object with new one.
//            static member addPublisher (publisher:string) (table:BiblioGraphicReference) =
//                table.Publisher <- publisher
//                table

//            /////Replaces title of existing object with new one.
//            static member addTitle (title:string) (table:BiblioGraphicReference) =
//                table.Title <- title
//                table

//            /////Replaces volume of existing object with new one.
//            static member addVolume (volume:string) (table:BiblioGraphicReference) =
//                table.Volume <- volume
//                table

//            /////Replaces year of existing object with new one.
//            static member addYear (year:int) (table:BiblioGraphicReference) =
//                table.Year <- Nullable(year)
//                table

//            /////Replaces mzidentml of existing object with new mzidentml.
//            //static member addMzIdentMLDocument
//            //    (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
//            //    let result = provider.MzIdentMLDocument <- mzIdentMLDocument
//            //    provider

//            ///Tries to find a biblioGraphicReference-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.BiblioGraphicReference.Find(id))

//            ///Tries to find a biblioGraphicReference-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.BiblioGraphicReference.Local do
//                           if i.Name=name
//                              then select i
//                      }
//                |> Seq.map (fun (biblioGraphicReference) -> biblioGraphicReference)
//                |> (fun biblioGraphicReference -> 
//                    if (Seq.exists (fun biblioGraphicReference' -> biblioGraphicReference' <> null) biblioGraphicReference) = false
//                        then 
//                            query {
//                                   for i in dbContext.BiblioGraphicReference do
//                                       if i.Name=name
//                                          then select i
//                                  }
//                            |> Seq.map (fun (biblioGraphicReference) -> biblioGraphicReference)
//                            |> (fun biblioGraphicReference -> if (Seq.exists (fun biblioGraphicReference' -> biblioGraphicReference' <> null) biblioGraphicReference) = false
//                                                                then None
//                                                                else Some biblioGraphicReference
//                               )
//                        else Some biblioGraphicReference
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:BiblioGraphicReference) (item2:BiblioGraphicReference) =
//                item1.Authors=item2.Authors && item1.DOI=item2.DOI && item1.Editor=item2.Editor && item1.Issue=item2.Issue &&
//                item1.Pages=item2.Pages && item1.Publication=item2.Publication && item1.Publisher=item2.Publisher && 
//                item1.Title=item2.Title && item1.Volume=item2.Volume && item1.Year=item2.Year

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:BiblioGraphicReference) =
//                    BiblioGraphicReferenceHandler.tryFindByName dbContext item.Name
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match BiblioGraphicReferenceHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:BiblioGraphicReference) =
//                BiblioGraphicReferenceHandler.addToContext dbContext item
//                dbContext.SaveChanges()

//        type MzQuantMLDocumentHandler =
//            ///Initializes a provider-object with at least all necessary parameters.
//            static member init
//                (             
//                    analysisSummary              : seq<AnalysisSummaryParam>,
//                    version                      : string,
//                    inputFiles                   : seq<InputFiles>,
//                    analysisSoftwares            : seq<AnalysisSoftware>,
//                    dataProcessings              : seq<DataProcessing>,
//                    assays                       : seq<Assay>,
//                    ?id                          : string,
//                    ?name                        : string,
//                    ?creationDate                : DateTime,
//                    ?provider                    : Provider,
//                    ?persons                     : seq<Person>,
//                    ?organizations               : seq<Organization>,
//                    ?biblioGraphicReferences     : seq<BiblioGraphicReference>,
//                    ?studyVariables              : seq<StudyVariable>,
//                    ?ratios                      : seq<Ratio>,
//                    ?proteinGroupList            : ProteinGroupList,
//                    ?proteinList                 : ProteinList,
//                    ?peptideConsensusList        : PeptideConsensusList,
//                    ?smallMolecule               : SmallMoleculeList,
//                    ?featureList                 : FeatureList
//                    //?mzIdentML                   : MzIdentMLDocument
//                ) =
//                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
//                let name'                           = defaultArg name Unchecked.defaultof<string>
//                let creationDate'                   = defaultArg creationDate Unchecked.defaultof<DateTime>
//                let provider'                       = defaultArg provider Unchecked.defaultof<Provider>
//                let persons'                        = convertOptionToList persons
//                let organizations'                  = convertOptionToList organizations
//                let biblioGraphicReferences'        = convertOptionToList biblioGraphicReferences
//                let studyVariables'                 = convertOptionToList studyVariables
//                let ratios'                         = convertOptionToList ratios
//                let proteinGroupList'               = defaultArg proteinGroupList Unchecked.defaultof<ProteinGroupList>
//                let proteinList'                    = defaultArg proteinList Unchecked.defaultof<ProteinList>
//                let peptideConsensusList'           = defaultArg peptideConsensusList Unchecked.defaultof<PeptideConsensusList>
//                let smallMoleculeList'              = defaultArg smallMolecule Unchecked.defaultof<SmallMoleculeList>
//                let featureList'                    = defaultArg featureList Unchecked.defaultof<FeatureList>
//                //let mzIdentML'                    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

//                new MzQuantMLDocument(id', name', Nullable(creationDate'), 
//                                      version, provider',persons', organizations', analysisSummary |> List, 
//                                      inputFiles |> List, analysisSoftwares |> List, dataProcessings |> List, 
//                                      assays |> List, biblioGraphicReferences', studyVariables', ratios', proteinGroupList',
//                                      proteinList', peptideConsensusList', smallMoleculeList', featureList', Nullable(DateTime.Now)
//                                     )

//            ///Replaces name of existing object with new one.
//            static member addName (name:string) (table:MzQuantMLDocument) =
//                table.Name <- name
//                table

//            ///Replaces creationDate of existing object with new one.
//            static member addCreationDate (creationDate:DateTime) (table:MzQuantMLDocument) =
//                table.CreationDate <- Nullable(creationDate)
//                table

//            ///Replaces provider of existing object with new one.
//            static member addProvider (provider:Provider) (table:MzQuantMLDocument) =
//                table.Provider <- provider
//                table

//            ///Replaces proteinGroupList of existing object with new one.
//            static member addProteinGroupList (proteinGroupList:ProteinGroupList) (table:MzQuantMLDocument) =
//                table.ProteinGroupList <- proteinGroupList
//                table

//            ///Replaces proteinList of existing object with new one.
//            static member addProteinList (proteinList:ProteinList) (table:MzQuantMLDocument) =
//                table.ProteinList <- proteinList
//                table

//            ///Replaces peptideConsensusList of existing object with new one.
//            static member addPeptideConsensusList (peptideConsensusList:PeptideConsensusList) (table:MzQuantMLDocument) =
//                table.PeptideConsensusList <- peptideConsensusList
//                table

//            ///Replaces smallMoleculeList of existing object with new one.
//            static member addSmallMoleculeList (smallMoleculeList:SmallMoleculeList) (table:MzQuantMLDocument) =
//                table.SmallMoleculeList <- smallMoleculeList
//                table

//            ///Replaces featureList of existing object with new one.
//            static member addFeatureList (featureList:FeatureList) (table:MzQuantMLDocument) =
//                table.FeatureList <- featureList
//                table

//            ///Adds new person to collection of enzymenames.
//            static member addPerson
//                (person:Person) (table:MzQuantMLDocument) =
//                table.Persons <- addToList table.Persons person
//                table

//            ///Add new collection of persons to collection of enzymenames.
//            static member addPersons
//                (persons:seq<Person>) (table:MzQuantMLDocument) =
//                table.Persons <- addCollectionToList table.Persons persons
//                table

//            ///Adds new organization to collection of enzymenames.
//            static member addOrganization
//                (organization:Organization) (table:MzQuantMLDocument) =
//                table.Organizations <- addToList table.Organizations organization
//                table

//            ///Add new collection of organizations to collection of enzymenames.
//            static member addOrganizations
//                (organizations:seq<Organization>) (table:MzQuantMLDocument) =
//                table.Organizations <- addCollectionToList table.Organizations organizations
//                table

//            ///Adds new biblioGraphicReference to collection of enzymenames.
//            static member addBiblioGraphicReference
//                (biblioGraphicReference:BiblioGraphicReference) (table:MzQuantMLDocument) =
//                table.BiblioGraphicReferences <- addToList table.BiblioGraphicReferences biblioGraphicReference
//                table

//            ///Add new collection of biblioGraphicReferences to collection of enzymenames.
//            static member addBiblioGraphicReferences
//                (biblioGraphicReferences:seq<BiblioGraphicReference>) (table:MzQuantMLDocument) =
//                table.BiblioGraphicReferences <- addCollectionToList table.BiblioGraphicReferences biblioGraphicReferences
//                table

//            ///Adds new studyVariable to collection of enzymenames.
//            static member addStudyVariable
//                (studyVariable:StudyVariable) (table:MzQuantMLDocument) =
//                table.StudyVariables <- addToList table.StudyVariables studyVariable
//                table

//            ///Add new collection of studyVariables to collection of enzymenames.
//            static member addStudyVariables
//                (studyVariables:seq<StudyVariable>) (table:MzQuantMLDocument) =
//                table.StudyVariables <- addCollectionToList table.StudyVariables studyVariables
//                table

//            ///Adds new person to collection of enzymenames.
//            static member addRatio
//                (ratio:Ratio) (table:MzQuantMLDocument) =
//                table.Ratios <- addToList table.Ratios ratio
//                table

//            ///Add new collection of persons to collection of enzymenames.
//            static member addRatios
//                (ratios:seq<Ratio>) (table:MzQuantMLDocument) =
//                table.Ratios <- addCollectionToList table.Ratios ratios
//                table

//            ///Tries to find a mzQuantMLDocument-object in the context and database, based on its primary-key(ID).
//            static member tryFindByID
//                (context:MzQuantML) (id:string) =
//                tryFind (context.MzQuantMLDocument.Find(id))

//            ///Tries to find a mzQuantMLDocument-object in the context and database, based on its 2nd most unique identifier.
//            static member tryFindByName (dbContext:MzQuantML) (name:string) =
//                query {
//                       for i in dbContext.MzQuantMLDocument.Local do
//                           if i.Name=name
//                              then select (i, i.AnalysisSummary, i.AnalysisSoftwares, i.InputFiles, i.FeatureList, i.Assays, 
//                                           i.DataProcessings, i.Provider, i.Persons, i.Organizations, i.BiblioGraphicReferences, 
//                                           i.StudyVariables, i.Ratios, i.ProteinList, i.ProteinGroupList, i.PeptideConsensusList,
//                                           i.SmallMoleculeList
//                                          )
//                      }
//                |> Seq.map (fun (mzQuantML, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _) -> mzQuantML)
//                |> (fun mzQuantML -> 
//                    if (Seq.exists (fun mzQuantML' -> mzQuantML' <> null) mzQuantML) = false
//                        then 
//                            query {
//                                   for i in dbContext.MzQuantMLDocument do
//                                       if i.Name=name
//                                          then select (i, i.AnalysisSummary, i.AnalysisSoftwares, i.InputFiles, i.FeatureList, i.Assays, 
//                                                       i.DataProcessings, i.Provider, i.Persons, i.Organizations, i.BiblioGraphicReferences, 
//                                                       i.StudyVariables, i.Ratios, i.ProteinList, i.ProteinGroupList, i.PeptideConsensusList,
//                                                       i.SmallMoleculeList
//                                                      )
//                                  }
//                            |> Seq.map (fun (mzQuantML, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _) -> mzQuantML)
//                            |> (fun mzQuantML -> if (Seq.exists (fun mzQuantML' -> mzQuantML' <> null) mzQuantML) = false
//                                                                then None
//                                                                else Some mzQuantML
//                               )
//                        else Some mzQuantML
//                   )

//            ///Checks whether all other fields of the current object and context object have the same values or not.
//            static member private hasEqualFieldValues (item1:MzQuantMLDocument) (item2:MzQuantMLDocument) =
//                item1.AnalysisSummary=item2.AnalysisSummary && 
//                item1.AnalysisSoftwares=item2.AnalysisSoftwares && 
//                item1.InputFiles=item2.InputFiles && 
//                item1.FeatureList=item2.FeatureList && 
//                item1.Assays=item2.Assays && 
//                item1.DataProcessings=item2.DataProcessings && 
//                item1.Provider=item2.Provider && 
//                item1.Persons=item2.Persons && 
//                item1.Organizations=item2.Organizations && 
//                item1.BiblioGraphicReferences=item2.BiblioGraphicReferences &&
//                item1.StudyVariables=item2.StudyVariables && 
//                item1.Ratios=item2.Ratios && 
//                item1.ProteinList=item2.ProteinList &&
//                item1.ProteinGroupList=item2.ProteinGroupList && 
//                item1.PeptideConsensusList=item2.PeptideConsensusList && 
//                item1.SmallMoleculeList=item2.SmallMoleculeList

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is added to the context and otherwise does nothing.
//            static member addToContext (dbContext:MzQuantML) (item:MzQuantMLDocument) =
//                    MzQuantMLDocumentHandler.tryFindByName dbContext item.Name
//                    |> (fun organizationCollection -> match organizationCollection with
//                                                      |Some x -> x
//                                                                 |> Seq.map (fun organization -> match MzQuantMLDocumentHandler.hasEqualFieldValues organization item with
//                                                                                                 |true -> null
//                                                                                                 |false -> dbContext.Add item
//                                                                            ) |> ignore
//                                                      |None -> dbContext.Add item |> ignore
//                       )

//            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
//            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
//            static member addToContextAndInsert (dbContext:MzQuantML) (item:MzQuantMLDocument) =
//                MzQuantMLDocumentHandler.addToContext dbContext item
//                dbContext.SaveChanges()
