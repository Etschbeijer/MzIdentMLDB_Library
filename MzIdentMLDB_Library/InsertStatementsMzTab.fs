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

        type FalseDiscoveryRateHandler =
            ///Initializes a cvparam-object with at least all necessary parameters.
            static member init
                (
                    ?id       : string,
                    ?term     : Term,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let term'     = defaultArg term Unchecked.defaultof<Term>
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>

                new FalseDiscoveryRate(
                                       id', 
                                       value', 
                                       term', 
                                       unit', 
                                       Nullable(DateTime.Now)
                                      )

            ///Replaces term of existing object with new one.
            static member addTerm
                (term:Term) (table:FalseDiscoveryRate) =
                table.Term <- term
                table
            
            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (table:FalseDiscoveryRate) =
                table.Value <- value
                table

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:FalseDiscoveryRate) =
                table.Unit <- unit
                table

            ///Tries to find a cvparam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.FalseDiscoveryRate.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.FalseDiscoveryRate.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.FalseDiscoveryRate do
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
            static member private hasEqualFieldValues (item1:FalseDiscoveryRate) (item2:FalseDiscoveryRate) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:FalseDiscoveryRate) =
                    FalseDiscoveryRateHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match FalseDiscoveryRateHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:FalseDiscoveryRate) =
                FalseDiscoveryRateHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type AnalysisSoftwareParamHandler =
            ///Initializes a analysisSoftwareParam-object with at least all necessary parameters.
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

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:AnalysisSoftwareParam) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:AnalysisSoftwareParam) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.AnalysisSoftwareParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
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
            static member addToContext (dbContext:MzTab) (item:AnalysisSoftwareParam) =
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
            static member addToContextAndInsert (dbContext:MzTab) (item:AnalysisSoftwareParam) =
                AnalysisSoftwareParamHandler.addToContext dbContext item |> ignore
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

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (table:OrganizationParam) =
                table.Value <- value
                table

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:OrganizationParam) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.OrganizationParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
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
            static member addToContext (dbContext:MzTab) (item:OrganizationParam) =
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
            static member addToContextAndInsert (dbContext:MzTab) (item:OrganizationParam) =
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

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (table:PersonParam) =
                table.Value <- value
                table

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (param:PersonParam) =
                param.Unit <- unit
                param

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.PersonParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
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
            static member addToContext (dbContext:MzTab) (item:PersonParam) =
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
            static member addToContextAndInsert (dbContext:MzTab) (item:PersonParam) =
                PersonParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type MetaDataSectionParamHandler =
            ///Initializes a metaDataSectionParam-object with at least all necessary parameters.
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
                    
                new MetaDataSectionParam(
                                         id', 
                                         value', 
                                         term, 
                                         unit', 
                                         Nullable(DateTime.Now)
                                        )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:MetaDataSectionParam) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:MetaDataSectionParam) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.MetaDataSectionParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.MetaDataSectionParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.MetaDataSectionParam do
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
            static member private hasEqualFieldValues (item1:MetaDataSectionParam) (item2:MetaDataSectionParam) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:MetaDataSectionParam) =
                    MetaDataSectionParamHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match MetaDataSectionParamHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:MetaDataSectionParam) =
                MetaDataSectionParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SampleParamHandler =
            ///Initializes a sampleParam-object with at least all necessary parameters.
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
                    
                new SampleParam(
                                id', 
                                value', 
                                term, 
                                unit', 
                                Nullable(DateTime.Now)
                               )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:SampleParam) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:SampleParam) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.SampleParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.SampleParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SampleParam do
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
            static member private hasEqualFieldValues (item1:SampleParam) (item2:SampleParam) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:SampleParam) =
                    SampleParamHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match SampleParamHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:SampleParam) =
                SampleParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SpeciesHandler =
            ///Initializes a species-object with at least all necessary parameters.
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
                    
                new Species(
                            id', 
                            value', 
                            term, 
                            unit', 
                            Nullable(DateTime.Now)
                           )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:Species) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:Species) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.Species.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.Species.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.Species do
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
            static member private hasEqualFieldValues (item1:Species) (item2:Species) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:Species) =
                    SpeciesHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match SpeciesHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:Species) =
                SpeciesHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type TissueHandler =
            ///Initializes a tissue-object with at least all necessary parameters.
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
                    
                new Tissue(
                           id', 
                           value', 
                           term, 
                           unit', 
                           Nullable(DateTime.Now)
                          )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:Tissue) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:Tissue) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.Tissue.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.Tissue.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.Tissue do
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
            static member private hasEqualFieldValues (item1:Tissue) (item2:Tissue) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:Tissue) =
                    TissueHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match TissueHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:Tissue) =
                TissueHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type CellTypeHandler =
            ///Initializes a cellType-object with at least all necessary parameters.
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
                    
                new CellType(
                             id', 
                             value', 
                             term, 
                             unit', 
                             Nullable(DateTime.Now)
                            )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:CellType) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:CellType) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.CellType.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.CellType.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.CellType do
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
            static member private hasEqualFieldValues (item1:CellType) (item2:CellType) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:CellType) =
                    CellTypeHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match CellTypeHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:CellType) =
                CellTypeHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type DiseaseHandler =
            ///Initializes a disease-object with at least all necessary parameters.
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
                    
                new Disease(
                            id', 
                            value', 
                            term, 
                            unit', 
                            Nullable(DateTime.Now)
                           )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:Disease) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:Disease) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.Disease.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.Disease.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.Disease do
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
            static member private hasEqualFieldValues (item1:Disease) (item2:Disease) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:Disease) =
                    DiseaseHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match DiseaseHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:Disease) =
                DiseaseHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type FixedModHandler =
            ///Initializes a fixedMod-object with at least all necessary parameters.
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
                    
                new FixedMod(
                             id', 
                             value', 
                             term, 
                             unit', 
                             Nullable(DateTime.Now)
                            )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:FixedMod) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:FixedMod) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.FixedMod.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.FixedMod.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.FixedMod do
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
            static member private hasEqualFieldValues (item1:FixedMod) (item2:FixedMod) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:FixedMod) =
                    FixedModHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match FixedModHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:FixedMod) =
                FixedModHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type FixedModSiteHandler =
            ///Initializes a fixedModSite-object with at least all necessary parameters.
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
                    
                new FixedModSite(
                                 id', 
                                 value', 
                                 term, 
                                 unit', 
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:FixedModSite) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:FixedModSite) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.FixedModSite.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.FixedModSite.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.FixedModSite do
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
            static member private hasEqualFieldValues (item1:FixedModSite) (item2:FixedModSite) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:FixedModSite) =
                    FixedModSiteHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match FixedModSiteHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:FixedModSite) =
                FixedModSiteHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type FixedModPositionHandler =
            ///Initializes a fixedModPosition-object with at least all necessary parameters.
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
                    
                new FixedModPosition(
                                     id', 
                                     value', 
                                     term, 
                                     unit', 
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:FixedModPosition) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:FixedModPosition) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.FixedModPosition.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.FixedModPosition.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.FixedModPosition do
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
            static member private hasEqualFieldValues (item1:FixedModPosition) (item2:FixedModPosition) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:FixedModPosition) =
                    FixedModPositionHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match FixedModPositionHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:FixedModPosition) =
                FixedModPositionHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type VariableModHandler =
            ///Initializes a variableMod-object with at least all necessary parameters.
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
                    
                new VariableMod(
                                id', 
                                value', 
                                term, 
                                unit', 
                                Nullable(DateTime.Now)
                               )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:VariableMod) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:VariableMod) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.VariableMod.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.VariableMod.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.VariableMod do
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
            static member private hasEqualFieldValues (item1:VariableMod) (item2:VariableMod) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:VariableMod) =
                    VariableModHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match VariableModHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:VariableMod) =
                VariableModHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type VariableModSiteHandler =
            ///Initializes a variableModSite-object with at least all necessary parameters.
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
                    
                new VariableModSite(
                                    id', 
                                    value', 
                                    term, 
                                    unit', 
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:VariableModSite) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:VariableModSite) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.VariableModSite.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.VariableModSite.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.VariableModSite do
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
            static member private hasEqualFieldValues (item1:VariableModSite) (item2:VariableModSite) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:VariableModSite) =
                    VariableModSiteHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match VariableModSiteHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:VariableModSite) =
                VariableModSiteHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type VariableModPositionHandler =
            ///Initializes a variableModPosition-object with at least all necessary parameters.
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
                    
                new VariableModPosition(
                                        id', 
                                        value', 
                                        term, 
                                        unit', 
                                        Nullable(DateTime.Now)
                                       )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:VariableModPosition) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:VariableModPosition) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.VariableModPosition.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.VariableModPosition.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.VariableModPosition do
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
            static member private hasEqualFieldValues (item1:VariableModPosition) (item2:VariableModPosition) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:VariableModPosition) =
                    VariableModPositionHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match VariableModPositionHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:VariableModPosition) =
                VariableModPositionHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinSearchEngineScoreHandler =
            ///Initializes a proteinSearchEngineScore-object with at least all necessary parameters.
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
                    
                new ProteinSearchEngineScore(
                                             id', 
                                             value', 
                                             term, 
                                             unit', 
                                             Nullable(DateTime.Now)
                                            )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:ProteinSearchEngineScore) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:ProteinSearchEngineScore) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.ProteinSearchEngineScore.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.ProteinSearchEngineScore.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinSearchEngineScore do
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
            static member private hasEqualFieldValues (item1:ProteinSearchEngineScore) (item2:ProteinSearchEngineScore) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:ProteinSearchEngineScore) =
                    ProteinSearchEngineScoreHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match ProteinSearchEngineScoreHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:ProteinSearchEngineScore) =
                ProteinSearchEngineScoreHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type PeptideSearchEngineScoreHandler =
            ///Initializes a peptideSearchEngineScore-object with at least all necessary parameters.
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
                    
                new PeptideSearchEngineScore(
                                             id', 
                                             value', 
                                             term, 
                                             unit', 
                                             Nullable(DateTime.Now)
                                            )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:PeptideSearchEngineScore) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:PeptideSearchEngineScore) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.PeptideSearchEngineScore.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.PeptideSearchEngineScore.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PeptideSearchEngineScore do
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
            static member private hasEqualFieldValues (item1:PeptideSearchEngineScore) (item2:PeptideSearchEngineScore) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:PeptideSearchEngineScore) =
                    PeptideSearchEngineScoreHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match PeptideSearchEngineScoreHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:PeptideSearchEngineScore) =
                PeptideSearchEngineScoreHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type PSMSearchEngineScoreHandler =
            ///Initializes a psmSearchEngineScore-object with at least all necessary parameters.
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
                    
                new PSMSearchEngineScore(
                                         id', 
                                         value', 
                                         term, 
                                         unit', 
                                         Nullable(DateTime.Now)
                                        )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:PSMSearchEngineScore) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:PSMSearchEngineScore) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.PSMSearchEngineScore.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.PSMSearchEngineScore.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PSMSearchEngineScore do
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
            static member private hasEqualFieldValues (item1:PSMSearchEngineScore) (item2:PSMSearchEngineScore) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:PSMSearchEngineScore) =
                    PSMSearchEngineScoreHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match PSMSearchEngineScoreHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:PSMSearchEngineScore) =
                PSMSearchEngineScoreHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SmallMoleculeSearchEngineScoreHandler =
            ///Initializes a smallMoleculeSearchEngineScore-object with at least all necessary parameters.
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
                    
                new SmallMoleculeSearchEngineScore(
                                                   id', 
                                                   value', 
                                                   term, 
                                                   unit', 
                                                   Nullable(DateTime.Now)
                                                  )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:SmallMoleculeSearchEngineScore) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:SmallMoleculeSearchEngineScore) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.SmallMoleculeSearchEngineScore.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.SmallMoleculeSearchEngineScore.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SmallMoleculeSearchEngineScore do
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
            static member private hasEqualFieldValues (item1:SmallMoleculeSearchEngineScore) (item2:SmallMoleculeSearchEngineScore) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:SmallMoleculeSearchEngineScore) =
                    SmallMoleculeSearchEngineScoreHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match SmallMoleculeSearchEngineScoreHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:SmallMoleculeSearchEngineScore) =
                SmallMoleculeSearchEngineScoreHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type MSRunFormatHandler =
            ///Initializes a msRunFormat-object with at least all necessary parameters.
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
                    
                new MSRunFormat(
                                id', 
                                value', 
                                term, 
                                unit', 
                                Nullable(DateTime.Now)
                               )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:MSRunFormat) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:MSRunFormat) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.MSRunFormat.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.MSRunFormat.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.MSRunFormat do
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
            static member private hasEqualFieldValues (item1:MSRunFormat) (item2:MSRunFormat) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:MSRunFormat) =
                    MSRunFormatHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match MSRunFormatHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:MSRunFormat) =
                MSRunFormatHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type MSRunLocationHandler =
            ///Initializes a msRunLocation-object with at least all necessary parameters.
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
                    
                new MSRunLocation(
                                  id', 
                                  value', 
                                  term, 
                                  unit', 
                                  Nullable(DateTime.Now)
                                 )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:MSRunLocation) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:MSRunLocation) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.MSRunLocation.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.MSRunLocation.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.MSRunLocation do
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
            static member private hasEqualFieldValues (item1:MSRunLocation) (item2:MSRunLocation) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:MSRunLocation) =
                    MSRunLocationHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match MSRunLocationHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:MSRunLocation) =
                MSRunLocationHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type MSRunIDFormatHandler =
            ///Initializes a msRunIDFormat-object with at least all necessary parameters.
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
                    
                new MSRunIDFormat(
                                  id', 
                                  value', 
                                  term, 
                                  unit', 
                                  Nullable(DateTime.Now)
                                 )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:MSRunIDFormat) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:MSRunIDFormat) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.MSRunIDFormat.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.MSRunIDFormat.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.MSRunIDFormat do
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
            static member private hasEqualFieldValues (item1:MSRunIDFormat) (item2:MSRunIDFormat) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:MSRunIDFormat) =
                    MSRunIDFormatHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match MSRunIDFormatHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:MSRunIDFormat) =
                MSRunIDFormatHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type MSRunFragmentationMethodHandler =
            ///Initializes a msRunFragmentationMethod-object with at least all necessary parameters.
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
                    
                new MSRunFragmentationMethod(
                                             id', 
                                             value', 
                                             term, 
                                             unit', 
                                             Nullable(DateTime.Now)
                                            )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:MSRunFragmentationMethod) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:MSRunFragmentationMethod) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.MSRunFragmentationMethod.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.MSRunFragmentationMethod.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.MSRunFragmentationMethod do
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
            static member private hasEqualFieldValues (item1:MSRunFragmentationMethod) (item2:MSRunFragmentationMethod) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:MSRunFragmentationMethod) =
                    MSRunFragmentationMethodHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match MSRunFragmentationMethodHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:MSRunFragmentationMethod) =
                MSRunFragmentationMethodHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type MSRunHashHandler =
            ///Initializes a msRunHash-object with at least all necessary parameters.
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
                    
                new MSRunHash(
                              id', 
                              value', 
                              term, 
                              unit', 
                              Nullable(DateTime.Now)
                             )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:MSRunHash) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:MSRunHash) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.MSRunHash.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.MSRunHash.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.MSRunHash do
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
            static member private hasEqualFieldValues (item1:MSRunHash) (item2:MSRunHash) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:MSRunHash) =
                    MSRunHashHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match MSRunHashHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:MSRunHash) =
                MSRunHashHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type MSRunHashMethodHandler =
            ///Initializes a msRunMethod-object with at least all necessary parameters.
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
                    
                new MSRunHashMethod(
                                    id', 
                                    value', 
                                    term, 
                                    unit', 
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:MSRunHashMethod) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:MSRunHashMethod) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.MSRunHashMethod.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.MSRunHashMethod.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.MSRunHashMethod do
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
            static member private hasEqualFieldValues (item1:MSRunHashMethod) (item2:MSRunHashMethod) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:MSRunHashMethod) =
                    MSRunHashMethodHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match MSRunHashMethodHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:MSRunHashMethod) =
                MSRunHashMethodHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type QuantificationReagentHandler =
            ///Initializes a quantificationReagent-object with at least all necessary parameters.
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
                    
                new QuantificationReagent(
                                          id', 
                                          value', 
                                          term, 
                                          unit', 
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:QuantificationReagent) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:QuantificationReagent) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.QuantificationReagent.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.QuantificationReagent.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.QuantificationReagent do
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
            static member private hasEqualFieldValues (item1:QuantificationReagent) (item2:QuantificationReagent) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:QuantificationReagent) =
                    QuantificationReagentHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match QuantificationReagentHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:QuantificationReagent) =
                QuantificationReagentHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type QuantificationModSiteHandler =
            ///Initializes a quantificationModSite-object with at least all necessary parameters.
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
                    
                new QuantificationModSite(
                                          id', 
                                          value', 
                                          term, 
                                          unit', 
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:QuantificationModSite) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:QuantificationModSite) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.QuantificationModSite.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.QuantificationModSite.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.QuantificationModSite do
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
            static member private hasEqualFieldValues (item1:QuantificationModSite) (item2:QuantificationModSite) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:QuantificationModSite) =
                    QuantificationModSiteHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match QuantificationModSiteHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:QuantificationModSite) =
                QuantificationModSiteHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type QuantificationModPositionHandler =
            ///Initializes a quantificationModPosition-object with at least all necessary parameters.
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
                    
                new QuantificationModPosition(
                                              id', 
                                              value', 
                                              term, 
                                              unit', 
                                              Nullable(DateTime.Now)
                                             )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:QuantificationModPosition) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:QuantificationModPosition) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.QuantificationModPosition.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.QuantificationModPosition.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.QuantificationModPosition do
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
            static member private hasEqualFieldValues (item1:QuantificationModPosition) (item2:QuantificationModPosition) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:QuantificationModPosition) =
                    QuantificationModPositionHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match QuantificationModPositionHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:QuantificationModPosition) =
                QuantificationModPositionHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type InstrumentNameHandler =
            ///Initializes a instrumentName-object with at least all necessary parameters.
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
                    
                new InstrumentName(
                                   id', 
                                   value', 
                                   term, 
                                   unit', 
                                   Nullable(DateTime.Now)
                                  )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:InstrumentName) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:InstrumentName) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.InstrumentName.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.InstrumentName.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.InstrumentName do
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
            static member private hasEqualFieldValues (item1:InstrumentName) (item2:InstrumentName) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:InstrumentName) =
                    InstrumentNameHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match InstrumentNameHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:InstrumentName) =
                InstrumentNameHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type InstrumentSourceHandler =
            ///Initializes a instrumentSource-object with at least all necessary parameters.
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
                    
                new InstrumentSource(
                                     id', 
                                     value', 
                                     term, 
                                     unit', 
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:InstrumentSource) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:InstrumentSource) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.InstrumentSource.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.InstrumentSource.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.InstrumentSource do
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
            static member private hasEqualFieldValues (item1:InstrumentSource) (item2:InstrumentSource) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:InstrumentSource) =
                    InstrumentSourceHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match InstrumentSourceHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:InstrumentSource) =
                InstrumentSourceHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type InstrumentAnalyzerHandler =
            ///Initializes a instrumentAnalyzer-object with at least all necessary parameters.
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
                    
                new InstrumentAnalyzer(
                                       id', 
                                       value', 
                                       term, 
                                       unit', 
                                       Nullable(DateTime.Now)
                                      )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:InstrumentAnalyzer) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:InstrumentAnalyzer) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.InstrumentAnalyzer.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.InstrumentAnalyzer.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.InstrumentAnalyzer do
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
            static member private hasEqualFieldValues (item1:InstrumentAnalyzer) (item2:InstrumentAnalyzer) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:InstrumentAnalyzer) =
                    InstrumentAnalyzerHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match InstrumentAnalyzerHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:InstrumentAnalyzer) =
                InstrumentAnalyzerHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type InstrumentDetectorHandler =
            ///Initializes a instrumentDetector-object with at least all necessary parameters.
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
                    
                new InstrumentDetector(
                                       id', 
                                       value', 
                                       term, 
                                       unit', 
                                       Nullable(DateTime.Now)
                                      )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:InstrumentDetector) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:InstrumentDetector) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.InstrumentDetector.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.InstrumentDetector.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.InstrumentDetector do
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
            static member private hasEqualFieldValues (item1:InstrumentDetector) (item2:InstrumentDetector) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:InstrumentDetector) =
                    InstrumentDetectorHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match InstrumentDetectorHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:InstrumentDetector) =
                InstrumentDetectorHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SearchEngineNameHandler =
            ///Initializes a searchEngineName-object with at least all necessary parameters.
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
                    
                new SearchEngineName(
                                     id', 
                                     value', 
                                     term, 
                                     unit', 
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:SearchEngineName) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:SearchEngineName) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.SearchEngineName.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.SearchEngineName.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SearchEngineName do
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
            static member private hasEqualFieldValues (item1:SearchEngineName) (item2:SearchEngineName) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:SearchEngineName) =
                    SearchEngineNameHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match SearchEngineNameHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:SearchEngineName) =
                SearchEngineNameHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ModificationHandler =
            ///Initializes a modification-object with at least all necessary parameters.
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
                    
                new Modification(
                                 id', 
                                 value', 
                                 term, 
                                 unit', 
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:Modification) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:Modification) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.Modification.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.Modification.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.Modification do
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
            static member private hasEqualFieldValues (item1:Modification) (item2:Modification) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:Modification) =
                    ModificationHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match ModificationHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:Modification) =
                ModificationHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinSectionParamHandler =
            ///Initializes a proteinSectionParam-object with at least all necessary parameters.
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
                    
                new ProteinSectionParam(
                                        id', 
                                        value', 
                                        term, 
                                        unit', 
                                        Nullable(DateTime.Now)
                                       )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:ProteinSectionParam) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:ProteinSectionParam) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.ProteinSectionParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.ProteinSectionParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinSectionParam do
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
            static member private hasEqualFieldValues (item1:ProteinSectionParam) (item2:ProteinSectionParam) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:ProteinSectionParam) =
                    ProteinSectionParamHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match ProteinSectionParamHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:ProteinSectionParam) =
                ProteinSectionParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type RetentionTimeWindowHandler =
            ///Initializes a retentionTimeWindow-object with at least all necessary parameters.
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
                    
                new RetentionTimeWindow(
                                        id', 
                                        value', 
                                        term, 
                                        unit', 
                                        Nullable(DateTime.Now)
                                       )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:RetentionTimeWindow) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:RetentionTimeWindow) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.RetentionTimeWindow.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.RetentionTimeWindow.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.RetentionTimeWindow do
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
            static member private hasEqualFieldValues (item1:RetentionTimeWindow) (item2:RetentionTimeWindow) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:RetentionTimeWindow) =
                    RetentionTimeWindowHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match RetentionTimeWindowHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:RetentionTimeWindow) =
                RetentionTimeWindowHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type PSMSectionParamHandler =
            ///Initializes a psmSectionParam-object with at least all necessary parameters.
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
                    
                new PSMSectionParam(
                                    id', 
                                    value', 
                                    term, 
                                    unit', 
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:PSMSectionParam) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:PSMSectionParam) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.PSMSectionParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.PSMSectionParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PSMSectionParam do
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
            static member private hasEqualFieldValues (item1:PSMSectionParam) (item2:PSMSectionParam) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:PSMSectionParam) =
                    PSMSectionParamHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match PSMSectionParamHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:PSMSectionParam) =
                PSMSectionParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SmallMoleculeSectionParamHandler =
            ///Initializes a smallMoleculeSectionParam-object with at least all necessary parameters.
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
                    
                new SmallMoleculeSectionParam(
                                              id', 
                                              value', 
                                              term, 
                                              unit', 
                                              Nullable(DateTime.Now)
                                             )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:SmallMoleculeSectionParam) =
                param.Value <- value
                param

            ///Replaces unit of existing object with new one.
            static member addUnit
                (unit:Term) (table:SmallMoleculeSectionParam) =
                table.Unit <- unit
                table

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.SmallMoleculeSectionParam.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.SmallMoleculeSectionParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SmallMoleculeSectionParam do
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
            static member private hasEqualFieldValues (item1:SmallMoleculeSectionParam) (item2:SmallMoleculeSectionParam) =
                item1.Value=item2.Value && item1.Unit.ID=item2.Unit.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:SmallMoleculeSectionParam) =
                    SmallMoleculeSectionParamHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match SmallMoleculeSectionParamHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:SmallMoleculeSectionParam) =
                SmallMoleculeSectionParamHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

////////////////////////////////////////////
////End of paramHandlers//////////////////////////////////////////////
////////////////////////////////////////////

        type SampleProcessingHandler =
            ///Initializes a sampleProcessing-object with at least all necessary parameters.
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
                    
                new SampleProcessing(
                                     id',
                                     value',
                                     term,
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces value of existing object with new one.
            static member addValue
                (value:string) (param:SampleProcessing) =
                param.Value <- value
                param

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.SampleProcessing.Find(id))

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzTab) (name:string) =
                query {
                       for i in dbContext.SampleProcessing.Local do
                           if i.Term.Name=name
                              then select (i, i.Term)
                      }
                |> Seq.map (fun (param,_ ) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SampleProcessing do
                                       if i.Term.Name=name
                                          then select (i, i.Term)
                                  }
                            |> Seq.map (fun (param,_ ) -> param)
                            |> (fun param -> if (Seq.exists (fun param' -> param' <> null) param) = false
                                                then None
                                                else Some param
                               )
                        else Some param
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SampleProcessing) (item2:SampleProcessing) =
                item1.Value=item2.Value

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:SampleProcessing) =
                    SampleProcessingHandler.tryFindByTermName dbContext item.Term.ID
                    |> (fun cvParamCollection -> match cvParamCollection with
                                                 |Some x -> x
                                                            |> Seq.map (fun cvParam -> match SampleProcessingHandler.hasEqualFieldValues cvParam item with
                                                                                       |true -> null
                                                                                       |false -> dbContext.Add item
                                                                       ) |> ignore
                                                 |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:SampleProcessing) =
                SampleProcessingHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type AnalysisSoftwareHandler =
            ///Initializes a analysisSoftware-object with at least all necessary parameters.
            static member init
                (
                    id        : string,
                    version   : string,
                    ?settings : seq<AnalysisSoftwareParam>
                ) =

                let settings'  = convertOptionToList settings
                    
                new AnalysisSoftware(
                                     id, 
                                     version, 
                                     settings', 
                                     Nullable(DateTime.Now)
                                    )

            ///Adds new setting to collection of enzymenames.
            static member addSetting
                (setting:AnalysisSoftwareParam) (table:AnalysisSoftware) =
                let result = table.Settings <- addToList table.Settings setting
                table

            ///Add new collection of settings to collection of enzymenames.
            static member addSettings
                (settings:seq<AnalysisSoftwareParam>) (table:AnalysisSoftware) =
                let result = table.Settings <- addCollectionToList table.Settings settings
                table

            ///Tries to find a analysisSoftware-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.AnalysisSoftware.Find(id))

            ///Tries to find a analysisSoftware-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByVersion (dbContext:MzTab) (version:string) =
                query {
                       for i in dbContext.AnalysisSoftware.Local do
                           if i.Version=version
                              then select (i, i.Settings)
                      }
                |> Seq.map (fun (analysisSoftware, _) -> analysisSoftware)
                |> (fun analysisSoftware -> 
                    if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisSoftware do
                                       if i.Version=version
                                          then select (i, i.Settings)
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
                item1.Settings=item2.Settings

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:AnalysisSoftware) =
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
            static member addToContextAndInsert (dbContext:MzTab) (item:AnalysisSoftware) =
                AnalysisSoftwareHandler.addToContext dbContext item
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

            ///Tries to find a organization-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.Organization.Find(id))

            ///Tries to find an organization-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzTab) (name:string) =
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
            static member addToContext (dbContext:MzTab) (item:Organization) =
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
            static member addToContextAndInsert (dbContext:MzTab) (item:Organization) =
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

            ///Tries to find a person-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.Person.Find(id))

            ///Tries to find a person-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzTab) (name:string) =
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
            static member addToContext (dbContext:MzTab) (item:Person) =
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
            static member addToContextAndInsert (dbContext:MzTab) (item:Person) =
                PersonHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type FixedModificationHandler =
            ///Initializes a fixedModification-object with at least all necessary parameters.
            static member init
                (             
                    fixedMods          : seq<FixedMod>,
                    ?id                : string,
                    ?fixedModSites     : seq<FixedModSite>,
                    ?fixedModPositions : seq<FixedModPosition>
                ) =
                let id'                = defaultArg id (System.Guid.NewGuid().ToString())
                let fixedModSites'     = convertOptionToList fixedModSites
                let fixedModPositions' = convertOptionToList fixedModPositions

                new FixedModification(
                                      id', 
                                      fixedMods |> List, 
                                      fixedModSites', 
                                      fixedModPositions',  
                                      Nullable(DateTime.Now)
                                     )

            ///Adds a fixedModSite to an existing object.
            static member addFixedModSite (fixedModSite:FixedModSite) (table:FixedModification) =
                table.FixedModSites <- addToList table.FixedModSites fixedModSite
                table

            ///Adds a collection of fixedModSites to an existing object.
            static member addFixedModSites (fixedModSites:seq<FixedModSite>) (table:FixedModification) =
                table.FixedModSites <- addCollectionToList table.FixedModSites fixedModSites
                table

            ///Adds a fixedModPosition to an existing object.
            static member addFixedModPosition (fixedModPosition:FixedModPosition) (table:FixedModification) =
                table.FixedModPositions <- addToList table.FixedModPositions fixedModPosition
                table

            ///Adds a collection of fixedModPositions to an existing object.
            static member addFixedModPositions (fixedModPositions:seq<FixedModPosition>) (table:FixedModification) =
                table.FixedModPositions <- addCollectionToList table.FixedModPositions fixedModPositions
                table

            ///Tries to find a fixedModification-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.FixedModification.Find(id))

            ///Tries to find a fixedModification-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFixedMods (dbContext:MzTab) (fixedMods:seq<FixedMod>) =
                query {
                       for i in dbContext.FixedModification.Local do
                           if i.FixedMods=(fixedMods |> List)
                              then select (i, i.FixedModSites, i.FixedModPositions)
                      }
                |> Seq.map (fun (fixedModification, _, _) -> fixedModification)
                |> (fun fixedModification -> 
                    if (Seq.exists (fun fixedModification' -> fixedModification' <> null) fixedModification) = false
                        then 
                            query {
                                   for i in dbContext.FixedModification do
                                       if i.FixedMods=(fixedMods |> List)
                                          then select (i, i.FixedModSites, i.FixedModPositions)
                                  }
                            |> Seq.map (fun (fixedModification, _, _) -> fixedModification)
                            |> (fun fixedModification -> if (Seq.exists (fun fixedModification' -> fixedModification' <> null) fixedModification) = false
                                                            then None
                                                            else Some fixedModification
                               )
                        else Some fixedModification
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:FixedModification) (item2:FixedModification) =
               item1.FixedModSites=item2.FixedModSites && item1.FixedModPositions=item2.FixedModPositions 
               //&& item1.MzIdentMLDocument=item2.MzIdentMLDocument 

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:FixedModification) =
                    FixedModificationHandler.tryFindByFixedMods dbContext item.FixedMods
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match FixedModificationHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:FixedModification) =
                FixedModificationHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type VariableModificationHandler =
            ///Initializes a variableModification-object with at least all necessary parameters.
            static member init
                (             
                    variableMods          : seq<VariableMod>,
                    ?id                   : string,
                    ?variableModSites     : seq<VariableModSite>,
                    ?variableModPositions : seq<VariableModPosition>
                ) =
                let id'                   = defaultArg id (System.Guid.NewGuid().ToString())
                let variableModSites'     = convertOptionToList variableModSites
                let variableModPositions' = convertOptionToList variableModPositions

                new VariableModification(
                                         id', 
                                         variableMods |> List, 
                                         variableModSites', 
                                         variableModPositions',  
                                         Nullable(DateTime.Now)
                                        )

            ///Adds a variableModSite to an existing object.
            static member addVariableModSite (variableModSite:VariableModSite) (table:VariableModification) =
                table.VariableModSites <- addToList table.VariableModSites variableModSite
                table

            ///Adds a collection of variableModSites to an existing object.
            static member addVariableModSites (variableModSites:seq<VariableModSite>) (table:VariableModification) =
                table.VariableModSites <- addCollectionToList table.VariableModSites variableModSites
                table

            ///Adds a variablePosition to an existing object.
            static member addVariableModPosition (variablePosition:VariableModPosition) (table:VariableModification) =
                table.VariableModPositions <- addToList table.VariableModPositions variablePosition
                table

            ///Adds a collection of variablePositions to an existing object.
            static member addVariableModPositions (variablePositions:seq<VariableModPosition>) (table:VariableModification) =
                table.VariableModPositions <- addCollectionToList table.VariableModPositions variablePositions
                table

            ///Tries to find a variableModification-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.VariableModification.Find(id))

            ///Tries to find a variableModification-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByVariableMods (dbContext:MzTab) (variabledMods:seq<VariableMod>) =
                query {
                       for i in dbContext.VariableModification.Local do
                           if i.VariableMods=(variabledMods |> List)
                              then select (i, i.VariableMods, i.VariableModSites, i.VariableModPositions)
                      }
                |> Seq.map (fun (variableModification, _, _, _) -> variableModification)
                |> (fun variableModification -> 
                    if (Seq.exists (fun variableModification' -> variableModification' <> null) variableModification) = false
                        then 
                            query {
                                   for i in dbContext.VariableModification do
                                       if i.VariableMods=(variabledMods |> List)
                                          then select (i, i.VariableMods, i.VariableModSites, i.VariableModPositions)
                                  }
                            |> Seq.map (fun (variableModification, _, _, _) -> variableModification)
                            |> (fun variableModification -> if (Seq.exists (fun variableModification' -> variableModification' <> null) variableModification) = false
                                                            then None
                                                            else Some variableModification
                               )
                        else Some variableModification
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:VariableModification) (item2:VariableModification) =
               item1.VariableModSites=item2.VariableModSites && item1.VariableModPositions=item2.VariableModPositions 
               //&& item1.MzIdentMLDocument=item2.MzIdentMLDocument 

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:VariableModification) =
                    VariableModificationHandler.tryFindByVariableMods dbContext item.VariableMods
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match VariableModificationHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:VariableModification) =
                VariableModificationHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type InstrumentHandler =
            ///Initializes a instrument-object with at least all necessary parameters.
            static member init
                (             
                    ?id                   : string,
                    ?instrumentNames      : seq<InstrumentName>,
                    ?instrumentSources    : seq<InstrumentSource>,
                    ?instrumentAnalyzers  : seq<InstrumentAnalyzer>,
                    ?instrumentDetectors  : seq<InstrumentDetector>
                ) =
                let id'                   = defaultArg id (System.Guid.NewGuid().ToString())
                let instrumentNames'      = convertOptionToList instrumentNames
                let instrumentSources'    = convertOptionToList instrumentSources
                let instrumentAnalyzers'  = convertOptionToList instrumentAnalyzers
                let instrumentDetectors'  = convertOptionToList instrumentDetectors

                new Instrument(
                               id', 
                               instrumentNames', 
                               instrumentSources',
                               instrumentAnalyzers',
                               instrumentDetectors',
                               Nullable(DateTime.Now)
                              )

            ///Adds a instrumentName to an existing object.
            static member addInstrumentName (instrumentName:InstrumentName) (table:Instrument) =
                table.InstrumentNames <- addToList table.InstrumentNames instrumentName
                table

            ///Adds a collection of instrumentNames to an existing object.
            static member addInstrumentNames (instrumentNames:seq<InstrumentName>) (table:Instrument) =
                table.InstrumentNames <- addCollectionToList table.InstrumentNames instrumentNames
                table

            ///Adds a instrumentSource to an existing object.
            static member addInstrumentSource (instrumentSource:InstrumentSource) (table:Instrument) =
                table.InstrumentSources <- addToList table.InstrumentSources instrumentSource
                table

            ///Adds a collection of instrumentSources to an existing object.
            static member addInstrumentSources (instrumentSources:seq<InstrumentSource>) (table:Instrument) =
                table.InstrumentSources <- addCollectionToList table.InstrumentSources instrumentSources
                table

            ///Adds a instrumentAnalyzer to an existing object.
            static member addInstrumentAnalyzer (instrumentAnalyzer:InstrumentAnalyzer) (table:Instrument) =
                table.InstrumentAnalyzers <- addToList table.InstrumentAnalyzers instrumentAnalyzer
                table

            ///Adds a collection of instrumentAnalyzers to an existing object.
            static member addInstrumentAnalyzers (instrumentAnalyzers:seq<InstrumentAnalyzer>) (table:Instrument) =
                table.InstrumentAnalyzers <- addCollectionToList table.InstrumentAnalyzers instrumentAnalyzers
                table

            ///Adds a instrumentDetector to an existing object.
            static member addaddInstrumentDetector (instrumentDetector:InstrumentDetector) (table:Instrument) =
                table.InstrumentDetectors <- addToList table.InstrumentDetectors instrumentDetector
                table

            ///Adds a collection of instrumentDetectors to an existing object.
            static member addInstrumentDetectors (instrumentDetectors:seq<InstrumentDetector>) (table:Instrument) =
                table.InstrumentDetectors <- addCollectionToList table.InstrumentDetectors instrumentDetectors
                table

            ///Tries to find a instrument-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.Instrument.Find(id))

            ///Tries to find a instrument-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByInstrumentNames (dbContext:MzTab) (instrumentNames:seq<InstrumentName>) =
                query {
                       for i in dbContext.Instrument.Local do
                           if i.InstrumentNames=(instrumentNames |> List)
                              then select (i, i.InstrumentNames, i.InstrumentSources , i.InstrumentAnalyzers, i.InstrumentDetectors)
                      }
                |> Seq.map (fun (instrument, _, _, _, _) -> instrument)
                |> (fun instrument -> 
                    if (Seq.exists (fun instrument' -> instrument' <> null) instrument) = false
                        then 
                            query {
                                   for i in dbContext.Instrument do
                                       if i.InstrumentNames=(instrumentNames |> List)
                                          then select (i, i.InstrumentNames, i.InstrumentSources , i.InstrumentAnalyzers, i.InstrumentDetectors)
                                  }
                            |> Seq.map (fun (instrument, _, _, _, _) -> instrument)
                            |> (fun instrument -> if (Seq.exists (fun instrument' -> instrument' <> null) instrument) = false
                                                    then None
                                                    else Some instrument
                               )
                        else Some instrument
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Instrument) (item2:Instrument) =
               item1.InstrumentSources=item2.InstrumentSources && 
               item1.InstrumentAnalyzers=item2.InstrumentAnalyzers && 
               item1.InstrumentDetectors=item2.InstrumentDetectors  

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:Instrument) =
                    InstrumentHandler.tryFindByInstrumentNames dbContext item.InstrumentNames
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match InstrumentHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:Instrument) =
                InstrumentHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type SearchEngineScoreHandler =
            ///Initializes a searchEngineScore-object with at least all necessary parameters.
            static member init
                (             
                    proteinSearchEngineScores       : seq<ProteinSearchEngineScore>,
                    peptideSearchEngineScores       : seq<PeptideSearchEngineScore>,
                    psmSearchEngineScores           : seq<PSMSearchEngineScore>,
                    smallMoleculeSearchEngineScores : seq<SmallMoleculeSearchEngineScore>,
                    ?id                             : string
                ) =

                let id'                   = defaultArg id (System.Guid.NewGuid().ToString())

                new SearchEngineScore(
                                      id', 
                                      proteinSearchEngineScores |> List,
                                      peptideSearchEngineScores |> List,
                                      psmSearchEngineScores |> List,
                                      smallMoleculeSearchEngineScores |> List,
                                      Nullable(DateTime.Now)
                                     )

            ///Tries to find a searchEngineScore-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.SearchEngineScore.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByProteinSearchEngineScores (dbContext:MzTab) (proteinSearchEngineScore:seq<ProteinSearchEngineScore>) =
                query {
                       for i in dbContext.SearchEngineScore.Local do
                           if i.ProteinSearchEngineScores=(proteinSearchEngineScore |> List)
                              then select (i, i.ProteinSearchEngineScores, i.PeptideSearchEngineScores , i.PSMSearchEngineScores, i.SmallMoleculeSearchEngineScores)
                      }
                |> Seq.map (fun (searchEngineScore, _, _, _, _) -> searchEngineScore)
                |> (fun searchEngineScore -> 
                    if (Seq.exists (fun searchEngineScore' -> searchEngineScore' <> null) searchEngineScore) = false
                        then 
                            query {
                                   for i in dbContext.SearchEngineScore do
                                       if i.ProteinSearchEngineScores=(proteinSearchEngineScore |> List)
                                          then select (i, i.ProteinSearchEngineScores, i.PeptideSearchEngineScores , i.PSMSearchEngineScores, i.SmallMoleculeSearchEngineScores)
                                  }
                            |> Seq.map (fun (searchEngineScore, _, _, _, _) -> searchEngineScore)
                            |> (fun searchEngineScore -> if (Seq.exists (fun searchEngineScore' -> searchEngineScore' <> null) searchEngineScore) = false
                                                            then None
                                                            else Some searchEngineScore
                               )
                        else Some searchEngineScore
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SearchEngineScore) (item2:SearchEngineScore) =
               item1.PeptideSearchEngineScores=item2.PeptideSearchEngineScores && 
               item1.PSMSearchEngineScores=item2.PSMSearchEngineScores && 
               item1.SmallMoleculeSearchEngineScores=item2.SmallMoleculeSearchEngineScores  

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:SearchEngineScore) =
                    SearchEngineScoreHandler.tryFindByProteinSearchEngineScores dbContext item.ProteinSearchEngineScores
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SearchEngineScoreHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:SearchEngineScore) =
                SearchEngineScoreHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type QuantificationHandler =
            ///Initializes a quantification-object with at least all necessary parameters.
            static member init
                (             
                    ?id                 : string,
                    ?method             : CVParam,
                    ?proteinUnit        : CVParam,
                    ?peptideUnit        : CVParam,
                    ?smallMoleculeUnit  : CVParam
                ) =
                let id'                = defaultArg id (System.Guid.NewGuid().ToString())
                let method'            = defaultArg method Unchecked.defaultof<CVParam>
                let proteinUnit'       = defaultArg proteinUnit Unchecked.defaultof<CVParam>
                let peptideUnit'       = defaultArg peptideUnit Unchecked.defaultof<CVParam>
                let smallMoleculeUnit' = defaultArg smallMoleculeUnit Unchecked.defaultof<CVParam>

                new Quantification(
                                   id', 
                                   method', 
                                   proteinUnit',
                                   peptideUnit',
                                   smallMoleculeUnit',
                                   Nullable(DateTime.Now)
                                 )

            ///Replaces method of existing object with new one.
            static member addMethod
                (method:CVParam) (table:Quantification) =
                table.Method <- method
                table

            ///Replaces proteinUnit of existing object with new one.
            static member addProteinUnit
                (proteinUnit:CVParam) (table:Quantification) =
                table.ProteinUnit <- proteinUnit
                table

            ///Replaces peptideUnit of existing object with new one.
            static member addPeptideUnit
                (peptideUnit:CVParam) (table:Quantification) =
                table.PeptideUnit <- peptideUnit
                table

            ///Replaces method of existing object with new one.
            static member addSmallMoleculeUnit
                (smallMoleculeUnit:CVParam) (table:Quantification) =
                table.SmallMoleculeUnit <- smallMoleculeUnit
                table

            ///Tries to find a quantification-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.Quantification.Find(id))

            ///Tries to find a instrument-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByMethod (dbContext:MzTab) (method:CVParam) =
                query {
                       for i in dbContext.Quantification.Local do
                           if i.Method=method
                              then select (i, i.Method, i.ProteinUnit , i.PeptideUnit, i.SmallMoleculeUnit)
                      }
                |> Seq.map (fun (quantification, _, _, _, _) -> quantification)
                |> (fun quantification -> 
                    if (Seq.exists (fun quantification' -> quantification' <> null) quantification) = false
                        then 
                            query {
                                   for i in dbContext.Quantification do
                                       if i.Method=method
                                          then select (i, i.Method, i.ProteinUnit , i.PeptideUnit, i.SmallMoleculeUnit)
                                  }
                            |> Seq.map (fun (quantification, _, _, _, _) -> quantification)
                            |> (fun quantification -> if (Seq.exists (fun quantification' -> quantification' <> null) quantification) = false
                                                        then None
                                                        else Some quantification
                               )
                        else Some quantification
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Quantification) (item2:Quantification) =
               item1.ProteinUnit=item2.ProteinUnit && 
               item1.PeptideUnit=item2.PeptideUnit && 
               item1.SmallMoleculeUnit=item2.SmallMoleculeUnit  

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:Quantification) =
                    QuantificationHandler.tryFindByMethod dbContext item.Method
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match QuantificationHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:Quantification) =
                QuantificationHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type MSRunHandler =
            ///Initializes a msRun-object with at least all necessary parameters.
            static member init
                (             
                    msRunLocations                   : seq<MSRunLocation>,
                    ?id                              : string,
                    ?msRunFormats                    : seq<MSRunFormat>,
                    ?msRunIDFormats                  : seq<MSRunIDFormat>,
                    ?msRunFragmentationMethods       : seq<MSRunFragmentationMethod>,
                    ?msRunHashs                      : seq<MSRunHash>,
                    ?msRunHashMethods                : seq<MSRunHashMethod>
                ) =

                let id'                        = defaultArg id (System.Guid.NewGuid().ToString())
                let msRunFormats'              = convertOptionToList msRunFormats
                let msRunIDFormats'            = convertOptionToList msRunIDFormats
                let msRunFragmentationMethods' = convertOptionToList msRunFragmentationMethods
                let msRunHashs'                = convertOptionToList msRunHashs
                let msRunHashMethods'          = convertOptionToList msRunHashMethods

                new MSRun(
                          id',
                          msRunFormats',
                          msRunLocations |> List,
                          msRunIDFormats',
                          msRunFragmentationMethods',
                          msRunHashs',
                          msRunHashMethods',                          
                          Nullable(DateTime.Now)
                         )

            ///Adds a msRunFormat to an existing object.
            static member addMSRunFormat (msRunFormat:MSRunFormat) (table:MSRun) =
                table.MSRunFormats <- addToList table.MSRunFormats msRunFormat
                table

            ///Adds a collection of msRunFormats to an existing object.
            static member addMSRunFormats (msRunFormats:seq<MSRunFormat>) (table:MSRun) =
                table.MSRunFormats <- addCollectionToList table.MSRunFormats msRunFormats
                table

            ///Adds a msRunIDFormat to an existing object.
            static member addMSRunIDFormat (msRunIDFormat:MSRunIDFormat) (table:MSRun) =
                table.MSRunIDFormats <- addToList table.MSRunIDFormats msRunIDFormat
                table

            ///Adds a collection of msRunIDFormats to an existing object.
            static member addMSRunIDFormats (msRunIDFormats:seq<MSRunIDFormat>) (table:MSRun) =
                table.MSRunIDFormats <- addCollectionToList table.MSRunIDFormats msRunIDFormats
                table

            ///Adds a msRunFragmentationMethod to an existing object.
            static member addMSRunFragmentationMethod (msRunFragmentationMethod:MSRunFragmentationMethod) (table:MSRun) =
                table.MSRunFragmentationMethods <- addToList table.MSRunFragmentationMethods msRunFragmentationMethod
                table

            ///Adds a collection of msRunFragmentationMethods to an existing object.
            static member addMSRunFragmentationMethods (msRunFragmentationMethods:seq<MSRunFragmentationMethod>) (table:MSRun) =
                table.MSRunFragmentationMethods <- addCollectionToList table.MSRunFragmentationMethods msRunFragmentationMethods
                table

            ///Adds a msRunHash to an existing object.
            static member addMSRunHash (msRunHash:MSRunHash) (table:MSRun) =
                table.MSRunHashs <- addToList table.MSRunHashs msRunHash
                table

            ///Adds a collection of msRunHashs to an existing object.
            static member addMSRunHashs (msRunHashs:seq<MSRunHash>) (table:MSRun) =
                table.MSRunHashs <- addCollectionToList table.MSRunHashs msRunHashs
                table

            ///Adds a msRunHashMethod to an existing object.
            static member addMSRunHashMethod (msRunHashMethod:MSRunHashMethod) (table:MSRun) =
                table.MSRunHashMethods <- addToList table.MSRunHashMethods msRunHashMethod
                table

            ///Adds a collection of msRunHashMethods to an existing object.
            static member addMSRunHashMethods (msRunHashMethods:seq<MSRunHashMethod>) (table:MSRun) =
                table.MSRunHashMethods <- addCollectionToList table.MSRunHashMethods msRunHashMethods
                table

            ///Tries to find a searchEngineScore-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.MSRun.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByMSRunLocations (dbContext:MzTab) (msRunLocations:seq<MSRunLocation>) =
                query {
                       for i in dbContext.MSRun.Local do
                           if i.MSRunLocations=(msRunLocations |> List)
                              then select (i, i.MSRunLocations, i.MSRunFormats, i.MSRunIDFormats , i.MSRunFragmentationMethods, i.MSRunHashs, i.MSRunHashMethods)
                      }
                |> Seq.map (fun (msRun, _, _, _, _, _, _) -> msRun)
                |> (fun msRun -> 
                    if (Seq.exists (fun msRun' -> msRun' <> null) msRun) = false
                        then 
                            query {
                                   for i in dbContext.MSRun do
                                       if i.MSRunLocations=(msRunLocations |> List)
                                          then select (i, i.MSRunLocations, i.MSRunFormats, i.MSRunIDFormats , i.MSRunFragmentationMethods, i.MSRunHashs, i.MSRunHashMethods)
                                  }
                            |> Seq.map (fun (msRun, _, _, _, _, _, _) -> msRun)
                            |> (fun msRun -> if (Seq.exists (fun msRun' -> msRun' <> null) msRun) = false
                                                then None
                                                else Some msRun
                               )
                        else Some msRun
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:MSRun) (item2:MSRun) =
               item1.MSRunFormats=item2.MSRunFormats && 
               item1.MSRunIDFormats=item2.MSRunIDFormats && 
               item1.MSRunFragmentationMethods=item2.MSRunFragmentationMethods && 
               item1.MSRunHashs=item2.MSRunHashs &&
               item1.MSRunHashMethods=item2.MSRunHashMethods  

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:MSRun) =
                    MSRunHandler.tryFindByMSRunLocations dbContext item.MSRunLocations
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match MSRunHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:MSRun) =
                MSRunHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type SampleHandler =
            ///Initializes a sample-object with at least all necessary parameters.
            static member init
                (             
                    ?id                              : string,
                    ?description                     : string,
                    ?species                         : seq<Species>,
                    ?tissues                         : seq<Tissue>,
                    ?cellTypes                       : seq<CellType>,
                    ?diseases                        : seq<Disease>,
                    ?details                         : seq<SampleParam>
                ) =

                let id'          = defaultArg id (System.Guid.NewGuid().ToString())
                let description' = defaultArg description Unchecked.defaultof<string>
                let species'     = convertOptionToList species
                let tissues'     = convertOptionToList tissues
                let cellTypes'   = convertOptionToList cellTypes
                let diseases'    = convertOptionToList diseases
                let details'     = convertOptionToList details

                new Sample(
                           id',
                           description',
                           species',
                           tissues',
                           cellTypes',
                           diseases',
                           details',                          
                           Nullable(DateTime.Now)
                          )

            ///Replaces description of existing object with new one.
            static member addDescription
                (description:string) (table:Sample) =
                table.Description <- description
                table

            ///Adds a species to an existing object.
            static member addSpecies (species:Species) (table:Sample) =
                table.Species <- addToList table.Species species
                table

            ///Adds a collection of species to an existing object.
            static member addSpecieses (species:seq<Species>) (table:Sample) =
                table.Species <- addCollectionToList table.Species species
                table

            ///Adds a tissue to an existing object.
            static member addTissue (tissue:Tissue) (table:Sample) =
                table.Tissues <- addToList table.Tissues tissue
                table

            ///Adds a collection of tissues to an existing object.
            static member addTissues (tissues:seq<Tissue>) (table:Sample) =
                table.Tissues <- addCollectionToList table.Tissues tissues
                table

            ///Adds a cellType to an existing object.
            static member addCellType (cellType:CellType) (table:Sample) =
                table.CellTypes <- addToList table.CellTypes cellType
                table

            ///Adds a collection of cellTypes to an existing object.
            static member addCellTypes (cellTypes:seq<CellType>) (table:Sample) =
                table.CellTypes <- addCollectionToList table.CellTypes cellTypes
                table

            ///Adds a disease to an existing object.
            static member addDisease (disease:Disease) (table:Sample) =
                table.Diseases <- addToList table.Diseases disease
                table

            ///Adds a collection of diseases to an existing object.
            static member addDiseases (diseases:seq<Disease>) (table:Sample) =
                table.Diseases <- addCollectionToList table.Diseases diseases
                table

            ///Adds a detail to an existing object.
            static member addDetail (detail:SampleParam) (table:Sample) =
                table.Details <- addToList table.Details detail
                table

            ///Adds a collection of details to an existing object.
            static member addDetails (details:seq<SampleParam>) (table:Sample) =
                table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a sample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.Sample.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByDescription (dbContext:MzTab) (description:string) =
                query {
                       for i in dbContext.Sample.Local do
                           if i.Description=description
                              then select (i, i.Species, i.Tissues, i.CellTypes , i.Diseases, i.Details)
                      }
                |> Seq.map (fun (sample, _, _, _, _, _) -> sample)
                |> (fun sample -> 
                    if (Seq.exists (fun sample' -> sample' <> null) sample) = false
                        then 
                            query {
                                   for i in dbContext.Sample do
                                       if i.Description=description
                                          then select (i, i.Species, i.Tissues, i.CellTypes , i.Diseases, i.Details)
                                  }
                            |> Seq.map (fun (sample, _, _, _, _, _) -> sample)
                            |> (fun sample -> if (Seq.exists (fun sample' -> sample' <> null) sample) = false
                                                then None
                                                else Some sample
                               )
                        else Some sample
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Sample) (item2:Sample) =
               item1.Species=item2.Species && 
               item1.Tissues=item2.Tissues && 
               item1.CellTypes=item2.CellTypes && 
               item1.Diseases=item2.Diseases &&
               item1.Details=item2.Details  

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:Sample) =
                    SampleHandler.tryFindByDescription dbContext item.Description
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SampleHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:Sample) =
                SampleHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type AssayHandler =
            ///Initializes a assay-object with at least all necessary parameters.
            static member init
                (             
                    ?id                              : string,
                    ?quantificationReagents          : seq<QuantificationReagent>,
                    ?quantificationModSites          : seq<QuantificationModSite>,
                    ?quantificationModPositions      : seq<QuantificationModPosition>,
                    ?samples                         : seq<Sample>,
                    ?msRuns                          : seq<MSRun>
                ) =

                let id'                         = defaultArg id (System.Guid.NewGuid().ToString())
                let quantificationReagents'     = convertOptionToList quantificationReagents
                let quantificationModSites'     = convertOptionToList quantificationModSites
                let quantificationModPositions' = convertOptionToList quantificationModPositions
                let samples'                    = convertOptionToList samples
                let msRuns'                     = convertOptionToList msRuns

                new Assay(
                           id',
                           quantificationReagents',
                           quantificationModSites',
                           quantificationModPositions',
                           samples',
                           msRuns',                          
                           Nullable(DateTime.Now)
                          )

            ///Adds a quantificationReagent to an existing object.
            static member addQuantificationReagent (quantificationReagent:QuantificationReagent) (table:Assay) =
                table.QuantificationReagents <- addToList table.QuantificationReagents quantificationReagent

            ///Adds a collection of quantificationReagents to an existing object.
            static member addQuantificationReagents (quantificationReagents:seq<QuantificationReagent>) (table:Assay) =
                table.QuantificationReagents <- addCollectionToList table.QuantificationReagents quantificationReagents

            ///Adds a quantificationModSite to an existing object.
            static member addQuantificationModSite (quantificationModSite:QuantificationModSite) (table:Assay) =
                table.QuantificationModSites <- addToList table.QuantificationModSites quantificationModSite

            ///Adds a collection of quantificationModSites to an existing object.
            static member addQuantificationModSites (quantificationModSites:seq<QuantificationModSite>) (table:Assay) =
                table.QuantificationModSites <- addCollectionToList table.QuantificationModSites quantificationModSites

            ///Adds a quantificationModPosition to an existing object.
            static member addQuantificationModPosition (quantificationModPosition:QuantificationModPosition) (table:Assay) =
                table.QuantificationModPositions <- addToList table.QuantificationModPositions quantificationModPosition

            ///Adds a collection of quantificationModPositions to an existing object.
            static member addQuantificationModPositions (quantificationModPositions:seq<QuantificationModPosition>) (table:Assay) =
                table.QuantificationModPositions <- addCollectionToList table.QuantificationModPositions quantificationModPositions

            ///Adds a sample to an existing object.
            static member addSample (sample:Sample) (table:Assay) =
                table.Samples <- addToList table.Samples sample

            ///Adds a collection of samples to an existing object.
            static member addSamples (samples:seq<Sample>) (table:Assay) =
                table.Samples <- addCollectionToList table.Samples samples

            ///Adds a msRun to an existing object.
            static member addMSRun (msRun:MSRun) (table:Assay) =
                table.MSRuns <- addToList table.MSRuns msRun

            ///Adds a collection of msRuns to an existing object.
            static member addMSRuns (msRuns:seq<MSRun>) (table:Assay) =
                table.MSRuns <- addCollectionToList table.MSRuns msRuns

            ///Tries to find a sample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.Assay.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByQuantificationReagents (dbContext:MzTab) (quantificationReagent:seq<QuantificationReagent>) =
                query {
                       for i in dbContext.Assay.Local do
                           if i.QuantificationReagents=(quantificationReagent |> List)
                              then select (i, i.QuantificationReagents, i.QuantificationModSites, i.QuantificationModPositions , i.Samples, i.MSRuns)
                      }
                |> Seq.map (fun (sample, _, _, _, _, _) -> sample)
                |> (fun assay -> 
                    if (Seq.exists (fun assay' -> assay' <> null) assay) = false
                        then 
                            query {
                                   for i in dbContext.Assay do
                                       if i.QuantificationReagents=(quantificationReagent |> List)
                                          then select (i, i.QuantificationReagents, i.QuantificationModSites, i.QuantificationModPositions , i.Samples, i.MSRuns)
                                  }
                            |> Seq.map (fun (assay, _, _, _, _, _) -> assay)
                            |> (fun assay -> if (Seq.exists (fun assay' -> assay' <> null) assay) = false
                                                then None
                                                else Some assay
                               )
                        else Some assay
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Assay) (item2:Assay) =
               item1.QuantificationModSites=item2.QuantificationModSites && 
               item1.QuantificationModPositions=item2.QuantificationModPositions && 
               item1.Samples=item2.Samples && 
               item1.MSRuns=item2.MSRuns

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:Assay) =
                    AssayHandler.tryFindByQuantificationReagents dbContext item.QuantificationReagents
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
            static member addToContextAndInsert (dbContext:MzTab) (item:Assay) =
                AssayHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type ColUnitHandler =
            ///Initializes a colUnit-object with at least all necessary parameters.
            static member init
                (             
                    ?id                              : string,
                    ?protein                         : CVParam,
                    ?peptide                         : CVParam,
                    ?psm                             : CVParam,
                    ?smallMolecule                   : CVParam
                ) =

                let id'            = defaultArg id (System.Guid.NewGuid().ToString())
                let protein'       = defaultArg protein Unchecked.defaultof<CVParam>
                let peptide'       = defaultArg peptide Unchecked.defaultof<CVParam>
                let psm'           = defaultArg psm Unchecked.defaultof<CVParam>
                let smallMolecule' = defaultArg smallMolecule Unchecked.defaultof<CVParam>

                new ColUnit(
                            id',
                            protein',
                            peptide',
                            psm',
                            smallMolecule',
                            Nullable(DateTime.Now)
                           )

            ///Replaces protein of existing object with new one.
            static member addProtein
                (protein:CVParam) (table:ColUnit) =
                table.Protein <- protein
                table

            ///Replaces peptide of existing object with new one.
            static member addPeptide
                (peptide:CVParam) (table:ColUnit) =
                table.Peptide <- peptide
                table

            ///Replaces psm of existing object with new one.
            static member addPSM
                (psm:CVParam) (table:ColUnit) =
                table.PSM <- psm
                table

            ///Replaces smallMolecule of existing object with new one.
            static member addSmallMolecule
                (smallMolecule:CVParam) (table:ColUnit) =
                table.SmallMolecule <- smallMolecule
                table

            ///Tries to find a colUnit-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.ColUnit.Find(id))

            ///Tries to find a colUnit-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByProtein (dbContext:MzTab) (protein:CVParam) =
                query {
                       for i in dbContext.ColUnit.Local do
                           if i.Protein=protein
                              then select (i, i.Protein, i.Peptide, i.PSM , i.SmallMolecule)
                      }
                |> Seq.map (fun (colUnit, _, _, _, _) -> colUnit)
                |> (fun colUnit -> 
                    if (Seq.exists (fun colUnit' -> colUnit' <> null) colUnit) = false
                        then 
                            query {
                                   for i in dbContext.ColUnit do
                                       if i.Protein=protein
                                          then select (i, i.Protein, i.Peptide, i.PSM , i.SmallMolecule)
                                  }
                            |> Seq.map (fun (colUnit, _, _, _, _) -> colUnit)
                            |> (fun colUnit -> if (Seq.exists (fun colUnit' -> colUnit' <> null) colUnit) = false
                                                then None
                                                else Some colUnit
                               )
                        else Some colUnit
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ColUnit) (item2:ColUnit) =
               item1.Peptide=item2.Peptide && 
               item1.PSM=item2.PSM && 
               item1.SmallMolecule=item2.SmallMolecule

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:ColUnit) =
                    ColUnitHandler.tryFindByProtein dbContext item.Protein
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ColUnitHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:ColUnit) =
                ColUnitHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type StudyVariableHandler =
            ///Initializes a studyVariable-object with at least all necessary parameters.
            static member init
                (             
                    ?id                              : string,
                    ?description                     : string,
                    ?assays                          : string,
                    ?samples                         : string
                ) =

                let id'          = defaultArg id (System.Guid.NewGuid().ToString())
                let description' = defaultArg description Unchecked.defaultof<string>
                let assays'      = defaultArg assays Unchecked.defaultof<string>
                let samples'     = defaultArg samples Unchecked.defaultof<string>

                new StudyVariable(
                                  id',
                                  description',
                                  assays',
                                  samples',                          
                                  Nullable(DateTime.Now)
                                  )

            ///Replaces description of existing object with new one.
            static member addDescription
                (description:string) (table:StudyVariable) =
                table.Description <- description
                table

            ///Replaces assayRefs of existing object with new one.
            static member addAssayRefs
                (assayRefs:string) (table:StudyVariable) =
                table.AssayRefs <- assayRefs
                table
                
            ///Replaces sampleRefs of existing object with new one.
            static member addSampleRefs
                (sampleRefs:string) (table:StudyVariable) =
                table.SampleRefs <- sampleRefs
                table

            ///Tries to find a sample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.StudyVariable.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByDescription (dbContext:MzTab) (description:string) =
                query {
                       for i in dbContext.StudyVariable.Local do
                           if i.Description=description
                              then select i
                      }
                |> Seq.map (fun (studyVariable) -> studyVariable)
                |> (fun studyVariable -> 
                    if (Seq.exists (fun studyVariable' -> studyVariable' <> null) studyVariable) = false
                        then 
                            query {
                                   for i in dbContext.StudyVariable do
                                       if i.Description=description
                                          then select i
                                  }
                            |> Seq.map (fun (studyVariable) -> studyVariable)
                            |> (fun studyVariable -> if (Seq.exists (fun studyVariable' -> studyVariable' <> null) studyVariable) = false
                                                        then None
                                                        else Some studyVariable
                               )
                        else Some studyVariable
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:StudyVariable) (item2:StudyVariable) =
               item1.AssayRefs=item2.AssayRefs && 
               item1.SampleRefs=item2.SampleRefs 

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:StudyVariable) =
                    StudyVariableHandler.tryFindByDescription dbContext item.Description
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
            static member addToContextAndInsert (dbContext:MzTab) (item:StudyVariable) =
                StudyVariableHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type IdentifierHandler =
            ///Initializes a identifier-object with at least all necessary parameters.
            static member init
                (id : string) =

                new Identifier(id, Nullable(DateTime.Now))

            ///Tries to find a sample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.Identifier.Find(id))

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:Identifier) =
                    IdentifierHandler.tryFindByID dbContext item.ID
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> null    
                                                      |None -> dbContext.Add item
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:Identifier) =
                IdentifierHandler.addToContext dbContext item
                dbContext.SaveChanges()    

        type PeptideSequenceHandler =
            ///Initializes a identifier-object with at least all necessary parameters.
            static member init

                (id : string) =

                new PeptideSequence(id, Nullable(DateTime.Now))

            ///Tries to find a peptideSequence-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.PeptideSequence.Find(id))

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:PeptideSequence) =
                    PeptideSequenceHandler.tryFindByID dbContext item.ID
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> null    
                                                      |None -> dbContext.Add item
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:PeptideSequence) =
                PeptideSequenceHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type AccessionHandler =
            ///Initializes a identifier-object with at least all necessary parameters.
            static member init

                (id : string) =

                new Accession(id, Nullable(DateTime.Now))

            ///Tries to find a accession-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.Accession.Find(id))

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:Accession) =
                    AccessionHandler.tryFindByID dbContext item.ID
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> null    
                                                      |None -> dbContext.Add item
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:Accession) =
                AccessionHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type PSMIDHandler =
            ///Initializes a identifier-object with at least all necessary parameters.
            static member init

                (id : string) =

                new PSMID(id, Nullable(DateTime.Now))

            ///Tries to find a accession-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.PSMID.Find(id))

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:PSMID) =
                    PSMIDHandler.tryFindByID dbContext item.ID
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> null    
                                                      |None -> dbContext.Add item
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:PSMID) =
                PSMIDHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type AccessionParameterHandler =
            ///Initializes a accessionParameter-object with at least all necessary parameters.
            static member init
                (             
                    identifiers                      : seq<Identifier>,
                    peptideSequence                  : PeptideSequence,
                    accession                        : Accession,
                    taxid                            : string,
                    species                          : string,
                    dataBase                         : string,
                    dataBaseVersion                  : string,
                    ?id                              : string
                ) =

                let id'          = defaultArg id (System.Guid.NewGuid().ToString())

                new AccessionParameter(
                                       id',
                                       identifiers |> List,
                                       peptideSequence,
                                       accession,
                                       taxid,
                                       species,
                                       dataBase,
                                       dataBaseVersion,
                                       Nullable(DateTime.Now)
                                      )

            ///Tries to find a accessionParameter-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.AccessionParameter.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTaxid (dbContext:MzTab) (taxid:string) =
                query {
                       for i in dbContext.AccessionParameter.Local do
                           if i.Taxid=taxid
                              then select (i, i.PeptideSequence, i.Identifiers, i.Accession )
                      }
                |> Seq.map (fun (accessionParameter, _, _, _) -> accessionParameter)
                |> (fun accessionParameter -> 
                    if (Seq.exists (fun accessionParameter' -> accessionParameter' <> null) accessionParameter) = false
                        then 
                            query {
                                   for i in dbContext.AccessionParameter do
                                       if i.Taxid=taxid
                                          then select (i, i.PeptideSequence, i.Identifiers, i.Accession )
                                  }
                            |> Seq.map (fun (accessionParameter, _, _, _) -> accessionParameter)
                            |> (fun accessionParameter -> if (Seq.exists (fun accessionParameter' -> accessionParameter' <> null) accessionParameter) = false
                                                            then None
                                                            else Some accessionParameter
                               )
                        else Some accessionParameter
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:AccessionParameter) (item2:AccessionParameter) =
               item1.Identifiers=item2.Identifiers && item1.PeptideSequence=item2.PeptideSequence && 
               item1.Accession=item2.Accession && item1.Species=item2.Species && 
               item1.DataBase=item2.DataBase && item1.DataBaseVersion=item2.DataBaseVersion

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:AccessionParameter) =
                    AccessionParameterHandler.tryFindByTaxid dbContext item.Taxid
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match AccessionParameterHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:AccessionParameter) =
                AccessionParameterHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type SearchEngineHandler =
            ///Initializes a searchEngine-object with at least all necessary parameters.
            static member init
                (             
                    identifiers                      : seq<Identifier>,
                    peptideSequence                  : PeptideSequence,
                    accession                        : Accession,
                    searchEngineNames                : seq<SearchEngineName>,
                    bestSearchEngineScore            : float,
                    ?id                              : string,
                    ?searchEngineScoreMSRun          : float
                ) =

                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let searchEngineScoreMSRun' = defaultArg searchEngineScoreMSRun Unchecked.defaultof<float>

                new SearchEngine(
                                 id',
                                 identifiers |> List,
                                 peptideSequence,
                                 accession,
                                 searchEngineNames |> List,
                                 Nullable(bestSearchEngineScore),
                                 Nullable(searchEngineScoreMSRun'),                          
                                 Nullable(DateTime.Now)
                                )

            ///Replaces searchEngineScoreMSRun of existing object with new one.
            static member addSearchEngineScoreMSRun
                (searchEngineScoreMSRun:float) (table:SearchEngine) =
                table.SearchEngineScoreMSRun <- Nullable(searchEngineScoreMSRun)
                table

            ///Tries to find a sample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.SearchEngine.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByBestSearchEngineScore (dbContext:MzTab) (bestSearchEngineScore:Nullable<float>) =
                query {
                       for i in dbContext.SearchEngine.Local do
                           if i.BestSearchEngineScore=bestSearchEngineScore
                              then select (i, i.Identifiers, i.PeptideSequence, i.Accession, i.SearchEngineNames)
                      }
                |> Seq.map (fun (searchEngine,_ , _ , _ ,_) -> searchEngine)
                |> (fun searchEngine -> 
                    if (Seq.exists (fun searchEngine' -> searchEngine' <> null) searchEngine) = false
                        then 
                            query {
                                   for i in dbContext.SearchEngine do
                                       if i.BestSearchEngineScore=bestSearchEngineScore
                                          then select (i, i.Identifiers, i.PeptideSequence, i.Accession, i.SearchEngineNames)
                                  }
                            |> Seq.map (fun (searchEngine,_ , _ , _ ,_) -> searchEngine)
                            |> (fun searchEngine -> if (Seq.exists (fun searchEngine' -> searchEngine' <> null) searchEngine) = false
                                                        then None
                                                        else Some searchEngine
                               )
                        else Some searchEngine
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SearchEngine) (item2:SearchEngine) =
               item1.Identifiers=item2.Identifiers && item1.PeptideSequence=item2.PeptideSequence && 
               item1.Accession=item2.Accession && item1.SearchEngineNames=item2.SearchEngineNames && 
               item1.SearchEngineScoreMSRun=item2.SearchEngineScoreMSRun

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:SearchEngine) =
                    SearchEngineHandler.tryFindByBestSearchEngineScore dbContext item.BestSearchEngineScore
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SearchEngineHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:SearchEngine) =
                SearchEngineHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type PeptideInfoHandler =
            ///Initializes a peptideInfo-object with at least all necessary parameters.
            static member init
                (             
                    accession                        : Accession,
                    ambigutityMembers                : seq<Accession>,
                    ?id                              : string,
                    ?numPeptidesDistinctMSRun        : string,
                    ?numPeptidesUniqueMSRun          : string
                ) =

                let id'                       = defaultArg id (System.Guid.NewGuid().ToString())
                let numPeptidesDistinctMSRun' = defaultArg numPeptidesDistinctMSRun Unchecked.defaultof<string>
                let numPeptidesUniqueMSRun'   = defaultArg numPeptidesUniqueMSRun Unchecked.defaultof<string>

                new PeptideInfo(
                                id',
                                accession,
                                numPeptidesDistinctMSRun',
                                numPeptidesUniqueMSRun',
                                ambigutityMembers |> List,
                                Nullable(DateTime.Now)
                               )

            ///Replaces numPeptidesDistinctMSRun of existing object with new one.
            static member addNumPeptidesDistinctMSRun
                (numPeptidesDistinctMSRun:string) (table:PeptideInfo) =
                table.NumPeptidesDistinctMSRun <- numPeptidesDistinctMSRun
                table

            ///Replaces numPeptidesUniqueMSRun of existing object with new one.
            static member addNumPeptidesUniqueMSRun
                (numPeptidesUniqueMSRun:string) (table:PeptideInfo) =
                table.NumPeptidesUniqueMSRun <- numPeptidesUniqueMSRun
                table

            ///Tries to find a sample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.PeptideInfo.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByAccession (dbContext:MzTab) (accession:Accession) =
                query {
                       for i in dbContext.PeptideInfo.Local do
                           if i.Accession=accession
                              then select (i, i.Accession, i.AmbigutityMembers)
                      }
                |> Seq.map (fun (peptideInfo,_ , _) -> peptideInfo)
                |> (fun peptideInfo -> 
                    if (Seq.exists (fun peptideInfo' -> peptideInfo' <> null) peptideInfo) = false
                        then 
                            query {
                                   for i in dbContext.PeptideInfo do
                                       if i.Accession=accession
                                          then select (i, i.Accession, i.AmbigutityMembers)
                                  }
                            |> Seq.map (fun (peptideInfo, _ ,_) -> peptideInfo)
                            |> (fun peptideInfo -> if (Seq.exists (fun peptideInfo' -> peptideInfo' <> null) peptideInfo) = false
                                                        then None
                                                        else Some peptideInfo
                               )
                        else Some peptideInfo
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:PeptideInfo) (item2:PeptideInfo) =
               item1.AmbigutityMembers=item2.AmbigutityMembers && 
               item1.NumPeptidesDistinctMSRun=item2.NumPeptidesDistinctMSRun && 
               item1.NumPeptidesUniqueMSRun=item2.NumPeptidesUniqueMSRun

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:PeptideInfo) =
                    PeptideInfoHandler.tryFindByAccession dbContext item.Accession
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PeptideInfoHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:PeptideInfo) =
                PeptideInfoHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type ProteinAbundanceHandler =
            ///Initializes a proteinAbundance-object with at least all necessary parameters.
            static member init
                (             
                    accession                        : Accession,
                    ?id                              : string,
                    ?abundanceAssay                  : string,
                    ?abundanceStudyVariable          : string,
                    ?abundanceSEDEVStudyVariable     : string,
                    ?abundanceSTDErrorStudyVariable  : string
                ) =

                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let abundanceAssay'                 = defaultArg abundanceAssay Unchecked.defaultof<string>
                let abundanceStudyVariable'         = defaultArg abundanceStudyVariable Unchecked.defaultof<string>
                let abundanceSEDEVStudyVariable'    = defaultArg abundanceSEDEVStudyVariable Unchecked.defaultof<string>
                let abundanceSTDErrorStudyVariable' = defaultArg abundanceSTDErrorStudyVariable Unchecked.defaultof<string>

                new ProteinAbundance(
                                     id',
                                     accession,
                                     abundanceAssay',
                                     abundanceStudyVariable',
                                     abundanceSEDEVStudyVariable',
                                     abundanceSTDErrorStudyVariable',
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces abundanceAssay of existing object with new one.
            static member addAbundanceAssay
                (abundanceAssay:string) (table:ProteinAbundance) =
                table.AbundanceAssay <- abundanceAssay
                table

            ///Replaces abundanceStudyVariable of existing object with new one.
            static member addAbundanceStudyVariable
                (abundanceStudyVariable:string) (table:ProteinAbundance) =
                table.AbundanceStudyVariable <- abundanceStudyVariable
                table

            ///Replaces abundanceSEDEVStudyVariable of existing object with new one.
            static member addAbundanceSEDEVStudyVariable
                (abundanceSEDEVStudyVariable:string) (table:ProteinAbundance) =
                table.AbundanceSEDEVStudyVariable <- abundanceSEDEVStudyVariable
                table

            ///Replaces abundanceSTDErrorStudyVariable of existing object with new one.
            static member addAbundanceSTDErrorStudyVariable
                (abundanceSTDErrorStudyVariable:string) (table:ProteinAbundance) =
                table.AbundanceSTDErrorStudyVariable <- abundanceSTDErrorStudyVariable
                table

            ///Tries to find a sample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.ProteinAbundance.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByAccession (dbContext:MzTab) (accession:Accession) =
                query {
                       for i in dbContext.ProteinAbundance.Local do
                           if i.Accession=accession
                              then select (i, i.Accession)
                      }
                |> Seq.map (fun (proteinAbundance,_) -> proteinAbundance)
                |> (fun proteinAbundance -> 
                    if (Seq.exists (fun proteinAbundance' -> proteinAbundance' <> null) proteinAbundance) = false
                        then 
                            query {
                                   for i in dbContext.ProteinAbundance do
                                       if i.Accession=accession
                                          then select (i, i.Accession)
                                  }
                            |> Seq.map (fun (proteinAbundance, _) -> proteinAbundance)
                            |> (fun proteinAbundance -> if (Seq.exists (fun proteinAbundance' -> proteinAbundance' <> null) proteinAbundance) = false
                                                        then None
                                                        else Some proteinAbundance
                               )
                        else Some proteinAbundance
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinAbundance) (item2:ProteinAbundance) =
               item1.AbundanceAssay=item2.AbundanceAssay && 
               item1.AbundanceStudyVariable=item2.AbundanceStudyVariable &&
               item1.AbundanceSEDEVStudyVariable=item2.AbundanceSEDEVStudyVariable &&
               item1.AbundanceSTDErrorStudyVariable=item2.AbundanceSTDErrorStudyVariable

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:ProteinAbundance) =
                    ProteinAbundanceHandler.tryFindByAccession dbContext item.Accession
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinAbundanceHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:ProteinAbundance) =
                ProteinAbundanceHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type RetentionTimeHandler =
            ///Initializes a retentionTime-object with at least all necessary parameters.
            static member init
                (             
                    peptideSequence                  : PeptideSequence,
                    ?id                              : string, 
                    ?retentionTime                   : float, 
                    ?retentionTimeWindow             : seq<RetentionTimeWindow>
                ) =

                let id'                  = defaultArg id (System.Guid.NewGuid().ToString())
                let retentionTime'       = defaultArg retentionTime Unchecked.defaultof<float>
                let retentionTimeWindow' = convertOptionToList retentionTimeWindow

                new RetentionTime(
                                   id',
                                   peptideSequence,
                                   Nullable(retentionTime'),
                                   retentionTimeWindow',
                                   Nullable(DateTime.Now)
                                  )

            ///Replaces retentionTime of existing object with new one.
            static member addRetentionTime
                (retentionTime:float) (table:RetentionTime) =
                table.RetentionTime <- Nullable(retentionTime)
                table

            ///Adds a retentionTimeWindow to an existing object.
            static member addRetentionTimeWindow (retentionTimeWindow:RetentionTimeWindow) (table:RetentionTime) =
                table.RetentionTimeWindow <- addToList table.RetentionTimeWindow retentionTimeWindow
                table

            ///Adds a collection of retentionTimeWindows to an existing object.
            static member addRetentionTimeWindows (retentionTimeWindow:seq<RetentionTimeWindow>) (table:RetentionTime) =
                table.RetentionTimeWindow <- addCollectionToList table.RetentionTimeWindow retentionTimeWindow
                table

            ///Tries to find a sample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.RetentionTime.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByRetentionTime (dbContext:MzTab) (retentionTimes:Nullable<float>) =
                query {
                       for i in dbContext.RetentionTime.Local do
                           if i.RetentionTime=retentionTimes
                              then select (i, i.PeptideSequence, i.RetentionTimeWindow)
                      }
                |> Seq.map (fun (retentionTime,_, _) -> retentionTime)
                |> (fun retentionTime -> 
                    if (Seq.exists (fun retentionTime' -> retentionTime' <> null) retentionTime) = false
                        then 
                            query {
                                   for i in dbContext.RetentionTime do
                                       if i.RetentionTime=retentionTimes
                                          then select (i, i.PeptideSequence, i.RetentionTimeWindow)
                                  }
                            |> Seq.map (fun (retentionTime,_, _) -> retentionTime)
                            |> (fun retentionTime -> if (Seq.exists (fun retentionTime' -> retentionTime' <> null) retentionTime) = false
                                                        then None
                                                        else Some retentionTime
                               )
                        else Some retentionTime
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:RetentionTime) (item2:RetentionTime) =
               item1.PeptideSequence=item2.PeptideSequence && item1.RetentionTimeWindow=item2.RetentionTimeWindow

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:RetentionTime) =
                    RetentionTimeHandler.tryFindByRetentionTime dbContext item.RetentionTime
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match RetentionTimeHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:RetentionTime) =
                RetentionTimeHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type PeptideAbundanceHandler =
            ///Initializes a peptideAbundance-object with at least all necessary parameters.
            static member init
                (             
                    peptideSequence                  : PeptideSequence,
                    ?id                              : string,
                    ?abundanceAssay                  : string,
                    ?abundanceStudyVariable          : string,
                    ?abundanceSEDEVStudyVariable     : string,
                    ?abundanceSTDErrorStudyVariable  : string
                ) =

                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let abundanceAssay'                 = defaultArg abundanceAssay Unchecked.defaultof<string>
                let abundanceStudyVariable'         = defaultArg abundanceStudyVariable Unchecked.defaultof<string>
                let abundanceSEDEVStudyVariable'    = defaultArg abundanceSEDEVStudyVariable Unchecked.defaultof<string>
                let abundanceSTDErrorStudyVariable' = defaultArg abundanceSTDErrorStudyVariable Unchecked.defaultof<string>

                new PeptideAbundance(
                                     id',
                                     peptideSequence,
                                     abundanceAssay',
                                     abundanceStudyVariable',
                                     abundanceSEDEVStudyVariable',
                                     abundanceSTDErrorStudyVariable',
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces abundanceAssay of existing object with new one.
            static member addAbundanceAssay
                (abundanceAssay:string) (table:PeptideAbundance) =
                table.AbundanceAssay <- abundanceAssay
                table

            ///Replaces abundanceStudyVariable of existing object with new one.
            static member addAbundanceStudyVariable
                (abundanceStudyVariable:string) (table:PeptideAbundance) =
                table.AbundanceStudyVariable <- abundanceStudyVariable
                table

            ///Replaces abundanceSEDEVStudyVariable of existing object with new one.
            static member addAbundanceSEDEVStudyVariable
                (abundanceSEDEVStudyVariable:string) (table:PeptideAbundance) =
                table.AbundanceSEDEVStudyVariable <- abundanceSEDEVStudyVariable
                table

            ///Replaces abundanceSTDErrorStudyVariable of existing object with new one.
            static member addAbundanceSTDErrorStudyVariable
                (abundanceSTDErrorStudyVariable:string) (table:PeptideAbundance) =
                table.AbundanceSTDErrorStudyVariable <- abundanceSTDErrorStudyVariable
                table

            ///Tries to find a sample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.PeptideAbundance.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByPeptideSequence (dbContext:MzTab) (peptideSequence:PeptideSequence) =
                query {
                       for i in dbContext.PeptideAbundance.Local do
                           if i.PeptideSequence=peptideSequence
                              then select (i, i.PeptideSequence)
                      }
                |> Seq.map (fun (peptideAbundance,_) -> peptideAbundance)
                |> (fun peptideAbundance -> 
                    if (Seq.exists (fun peptideAbundance' -> peptideAbundance' <> null) peptideAbundance) = false
                        then 
                            query {
                                   for i in dbContext.PeptideAbundance do
                                       if i.PeptideSequence=peptideSequence
                                          then select (i, i.PeptideSequence)
                                  }
                            |> Seq.map (fun (peptideAbundance, _) -> peptideAbundance)
                            |> (fun peptideAbundance -> if (Seq.exists (fun peptideAbundance' -> peptideAbundance' <> null) peptideAbundance) = false
                                                        then None
                                                        else Some peptideAbundance
                               )
                        else Some peptideAbundance
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:PeptideAbundance) (item2:PeptideAbundance) =
               item1.AbundanceAssay=item2.AbundanceAssay && 
               item1.AbundanceStudyVariable=item2.AbundanceStudyVariable &&
               item1.AbundanceSEDEVStudyVariable=item2.AbundanceSEDEVStudyVariable &&
               item1.AbundanceSTDErrorStudyVariable=item2.AbundanceSTDErrorStudyVariable

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:PeptideAbundance) =
                    PeptideAbundanceHandler.tryFindByPeptideSequence dbContext item.PeptideSequence
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PeptideAbundanceHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:PeptideAbundance) =
                PeptideAbundanceHandler.addToContext dbContext item
                dbContext.SaveChanges()
        
        type PSMInformationHandler =
            ///Initializes a psmInformation-object with at least all necessary parameters.
            static member init
                (             
                    peptideSequence                  : PeptideSequence,
                    pre                              : string,
                    post                             : string,
                    start                            : int,
                    ende                             : int,
                    ?id                              : string
                ) =

                let id'                = defaultArg id (System.Guid.NewGuid().ToString())

                new PSMInformation(
                                   id',
                                   peptideSequence,
                                   pre,
                                   post,
                                   Nullable(start),
                                   Nullable(ende),
                                   Nullable(DateTime.Now)
                                  )

            ///Tries to find a sample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.PSMInformation.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByPre (dbContext:MzTab) (pre:string) =
                query {
                       for i in dbContext.PSMInformation.Local do
                           if i.Pre=pre
                              then select (i, i.PeptideSequence)
                      }
                |> Seq.map (fun (psmInformation,_) -> psmInformation)
                |> (fun psmInformation -> 
                    if (Seq.exists (fun psmInformation' -> psmInformation' <> null) psmInformation) = false
                        then 
                            query {
                                   for i in dbContext.PSMInformation do
                                       if i.Pre=pre
                                          then select (i, i.PeptideSequence)
                                  }
                            |> Seq.map (fun (psmInformation,_) -> psmInformation)
                            |> (fun psmInformation -> if (Seq.exists (fun psmInformation' -> psmInformation' <> null) psmInformation) = false
                                                        then None
                                                        else Some psmInformation
                               )
                        else Some psmInformation
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:PSMInformation) (item2:PSMInformation) =
               item1.PeptideSequence=item2.PeptideSequence && item1.Post=item2.Post &&
               item1.Start=item2.Start && item1.End=item2.End

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:PSMInformation) =
                    PSMInformationHandler.tryFindByPre dbContext item.Pre
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PSMInformationHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:PSMInformation) =
                PSMInformationHandler.addToContext dbContext item
                dbContext.SaveChanges()
        
        type ChemicalHandler =
            ///Initializes a chemical-object with at least all necessary parameters.
            static member init
                (             
                    identifiers                      : seq<Identifier>,
                    chemicalFormula                  : string,
                    smiles                           : string,
                    inchiKey                         : string,
                    ?id                              : string
                ) =

                let id'                = defaultArg id (System.Guid.NewGuid().ToString())

                new Chemical(
                             id',
                             identifiers |> List,
                             chemicalFormula,
                             smiles,
                             inchiKey,
                             Nullable(DateTime.Now)
                            )

            ///Tries to find a chemical-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.Chemical.Find(id))

            ///Tries to find a chemical-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByChemicalFormula (dbContext:MzTab) (chemicalFormula:string) =
                query {
                       for i in dbContext.Chemical.Local do
                           if i.ChemicalFormula=chemicalFormula
                              then select (i, i.Identifiers)
                      }
                |> Seq.map (fun (chemical,_) -> chemical)
                |> (fun chemical -> 
                    if (Seq.exists (fun chemical' -> chemical' <> null) chemical) = false
                        then 
                            query {
                                   for i in dbContext.Chemical do
                                       if i.ChemicalFormula=chemicalFormula
                                          then select (i, i.Identifiers)
                                  }
                            |> Seq.map (fun (chemical,_) -> chemical)
                            |> (fun chemical -> if (Seq.exists (fun chemical' -> chemical' <> null) chemical) = false
                                                        then None
                                                        else Some chemical
                               )
                        else Some chemical
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Chemical) (item2:Chemical) =
               item1.ChemicalFormula=item2.ChemicalFormula && item1.Smiles=item2.Smiles && 
               item1.InchiKey=item2.InchiKey

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:Chemical) =
                    ChemicalHandler.tryFindByChemicalFormula dbContext item.ChemicalFormula
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ChemicalHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:Chemical) =
                ChemicalHandler.addToContext dbContext item
                dbContext.SaveChanges()
 
        type SmallMoleculeAbundanceHandler =
            ///Initializes a smallMoleculeAbundance-object with at least all necessary parameters.
            static member init
                (             
                    identifiers                      : List<Identifier>,
                    ?id                              : string,
                    ?abundanceAssay                  : string,
                    ?abundanceStudyVariable          : string,
                    ?abundanceSEDEVStudyVariable     : string,
                    ?abundanceSTDErrorStudyVariable  : string
                ) =

                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let abundanceAssay'                 = defaultArg abundanceAssay Unchecked.defaultof<string>
                let abundanceStudyVariable'         = defaultArg abundanceStudyVariable Unchecked.defaultof<string>
                let abundanceSEDEVStudyVariable'    = defaultArg abundanceSEDEVStudyVariable Unchecked.defaultof<string>
                let abundanceSTDErrorStudyVariable' = defaultArg abundanceSTDErrorStudyVariable Unchecked.defaultof<string>

                new SmallMoleculeAbundance(
                                           id',
                                           identifiers |> List,
                                           abundanceAssay',
                                           abundanceStudyVariable',
                                           abundanceSEDEVStudyVariable',
                                           abundanceSTDErrorStudyVariable',
                                           Nullable(DateTime.Now)
                                          )

            ///Replaces abundanceAssay of existing object with new one.
            static member addAbundanceAssay
                (abundanceAssay:string) (table:SmallMoleculeAbundance) =
                table.AbundanceAssay <- abundanceAssay
                table

            ///Replaces abundanceStudyVariable of existing object with new one.
            static member addAbundanceStudyVariable
                (abundanceStudyVariable:string) (table:SmallMoleculeAbundance) =
                table.AbundanceStudyVariable <- abundanceStudyVariable
                table

            ///Replaces abundanceSEDEVStudyVariable of existing object with new one.
            static member addAbundanceSEDEVStudyVariable
                (abundanceSEDEVStudyVariable:string) (table:SmallMoleculeAbundance) =
                table.AbundanceSEDEVStudyVariable <- abundanceSEDEVStudyVariable
                table

            ///Replaces abundanceSTDErrorStudyVariable of existing object with new one.
            static member addAbundanceSTDErrorStudyVariable
                (abundanceSTDErrorStudyVariable:string) (table:SmallMoleculeAbundance) =
                table.AbundanceSTDErrorStudyVariable <- abundanceSTDErrorStudyVariable
                table

            ///Tries to find a smallMoleculeAbundance-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.PeptideAbundance.Find(id))

            ///Tries to find a smallMoleculeAbundance-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByIdentifiers (dbContext:MzTab) (identifiers:List<Identifier>) =
                query {
                       for i in dbContext.SmallMoleculeAbundance.Local do
                           if i.Identifiers=identifiers
                              then select (i, i.Identifiers)
                      }
                |> Seq.map (fun (peptideAbundance,_) -> peptideAbundance)
                |> (fun peptideAbundance -> 
                    if (Seq.exists (fun peptideAbundance' -> peptideAbundance' <> null) peptideAbundance) = false
                        then 
                            query {
                                   for i in dbContext.SmallMoleculeAbundance do
                                       if i.Identifiers=identifiers
                                          then select (i, i.Identifiers)
                                  }
                            |> Seq.map (fun (peptideAbundance, _) -> peptideAbundance)
                            |> (fun peptideAbundance -> if (Seq.exists (fun peptideAbundance' -> peptideAbundance' <> null) peptideAbundance) = false
                                                        then None
                                                        else Some peptideAbundance
                               )
                        else Some peptideAbundance
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SmallMoleculeAbundance) (item2:SmallMoleculeAbundance) =
               item1.AbundanceAssay=item2.AbundanceAssay && 
               item1.AbundanceStudyVariable=item2.AbundanceStudyVariable &&
               item1.AbundanceSEDEVStudyVariable=item2.AbundanceSEDEVStudyVariable &&
               item1.AbundanceSTDErrorStudyVariable=item2.AbundanceSTDErrorStudyVariable

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:SmallMoleculeAbundance) =
                    SmallMoleculeAbundanceHandler.tryFindByIdentifiers dbContext item.Identifiers
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SmallMoleculeAbundanceHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:SmallMoleculeAbundance) =
                SmallMoleculeAbundanceHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type ProteinSectionHandler =
            ///Initializes a proteinSection-object with at least all necessary parameters.
            static member init
                (             
                    accession                        : Accession,
                    description                      : string,
                    accessionParameters              : seq<AccessionParameter>,
                    searchEngines                    : seq<SearchEngine>,
                    modifications                    : seq<Modification>,
                    ?id                              : string,
                    ?reliability                     : int,
                    ?numPSMSMSRun                    : string,
                    ?peptideInfo                     : seq<PeptideInfo>,
                    ?uri                             : string,
                    ?goTerms                         : string, 
                    ?proteinCoverage                 : float, 
                    ?proteinAbundances               : seq<ProteinAbundance>,
                    ?details                         : seq<ProteinSectionParam>
                ) =

                let id'                = defaultArg id (System.Guid.NewGuid().ToString())
                let reliability'       = defaultArg reliability Unchecked.defaultof<int>
                let numPSMSMSRun'      = defaultArg numPSMSMSRun Unchecked.defaultof<string>
                let peptideInfo'       = convertOptionToList peptideInfo
                let uri'               = defaultArg uri Unchecked.defaultof<string>
                let goTerms'           = defaultArg goTerms Unchecked.defaultof<string>
                let proteinCoverage'   = defaultArg proteinCoverage Unchecked.defaultof<float>
                let proteinAbundances' = convertOptionToList proteinAbundances
                let details'           = convertOptionToList details

                new ProteinSection(
                                   id',
                                   accession,
                                   description,
                                   accessionParameters |> List,
                                   searchEngines |> List,
                                   Nullable(reliability'),
                                   numPSMSMSRun',
                                   peptideInfo',
                                   modifications |> List,
                                   uri',
                                   goTerms',
                                   Nullable(proteinCoverage'),
                                   proteinAbundances' |> List,
                                   details',
                                   Nullable(DateTime.Now)
                                  )

            ///Replaces reliability of existing object with new one.
            static member addReliability
                (reliability:int) (table:ProteinSection) =
                table.Reliability <- Nullable(reliability)
                table

            ///Replaces numPSMSMSRun of existing object with new one.
            static member addNumPSMSMSRun
                (numPSMSMSRun:string) (table:ProteinSection) =
                table.NumPSMSMSRun <- numPSMSMSRun
                table

            ///Adds a peptideInfo to an existing object.
            static member addPeptideInfo (peptideInfo:PeptideInfo) (table:ProteinSection) =
                table.PeptideInfos <- addToList table.PeptideInfos peptideInfo
                table

            ///Adds a collection of peptideInfos to an existing object.
            static member addPeptideInfos (peptideInfos:seq<PeptideInfo>) (table:ProteinSection) =
                table.PeptideInfos <- addCollectionToList table.PeptideInfos peptideInfos
                table

            ///Replaces uri of existing object with new one.
            static member addURI
                (uri:string) (table:ProteinSection) =
                table.URI <- uri
                table

            ///Replaces goTerms of existing object with new one.
            static member addGoTerms
                (goTerms:string) (table:ProteinSection) =
                table.GoTerms <- goTerms
                table
    
            ///Replaces proteinCoverage of existing object with new one.
            static member addProteinCoverage
                (proteinCoverage:float) (table:ProteinSection) =
                table.ProteinCoverage <- Nullable(proteinCoverage)
                table

            ///Adds a proteinAbundance to an existing object.
            static member addProteinAbundance (proteinAbundance:ProteinAbundance) (table:ProteinSection) =
                table.ProteinAbundances <- addToList table.ProteinAbundances proteinAbundance
                table

            ///Adds a collection of proteinAbundances to an existing object.
            static member addProteinAbundances (proteinAbundances:seq<ProteinAbundance>) (table:ProteinSection) =
                table.ProteinAbundances <- addCollectionToList table.ProteinAbundances proteinAbundances
                table

            ///Adds a detail to an existing object.
            static member addDetail (detail:ProteinSectionParam) (table:ProteinSection) =
                table.Details <- addToList table.Details detail
                table

            ///Adds a collection of details to an existing object.
            static member addDetails (details:seq<ProteinSectionParam>) (table:ProteinSection) =
                table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a sample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.ProteinSection.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByDescription (dbContext:MzTab) (description:string) =
                query {
                       for i in dbContext.ProteinSection.Local do
                           if i.Description=description
                              then select (i, i.AccessionParameters, i.SearchEngines, 
                                           i.Modifications, i.PeptideInfos, i.ProteinAbundances, i.Details
                                          )
                      }
                |> Seq.map (fun (proteinSection,_, _, _, _, _, _) -> proteinSection)
                |> (fun proteinSection -> 
                    if (Seq.exists (fun proteinSection' -> proteinSection' <> null) proteinSection) = false
                        then 
                            query {
                                   for i in dbContext.ProteinSection do
                                       if i.Description=description
                                          then select (i, i.AccessionParameters, i.SearchEngines, 
                                                       i.Modifications, i.PeptideInfos, i.ProteinAbundances, i.Details
                                                      )
                                  }
                            |> Seq.map (fun (proteinSection,_, _, _, _, _, _) -> proteinSection)
                            |> (fun proteinSection -> if (Seq.exists (fun proteinSection' -> proteinSection' <> null) proteinSection) = false
                                                        then None
                                                        else Some proteinSection
                               )
                        else Some proteinSection
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinSection) (item2:ProteinSection) =
               item1.Accession=item2.Accession && item1.AccessionParameters=item2.AccessionParameters &&
               item1.SearchEngines=item2.SearchEngines && item1.Modifications=item2.Modifications &&
               item1.Reliability=item2.Reliability && item1.NumPSMSMSRun=item2.NumPSMSMSRun &&
               item1.PeptideInfos=item2.PeptideInfos && item1.URI=item2.URI && item1.GoTerms=item2.GoTerms && 
               item1.ProteinCoverage=item2.ProteinCoverage && item1.Details=item2.Details &&
               item1.ProteinAbundances=item2.ProteinAbundances

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:ProteinSection) =
                    ProteinSectionHandler.tryFindByDescription dbContext item.Description
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinSectionHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:ProteinSection) =
                ProteinSectionHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type PeptideSectionHandler =
            ///Initializes a peptideSection-object with at least all necessary parameters.
            static member init
                (                
                    ?id                              : string,
                    ?peptideSequence                 : PeptideSequence,
                    ?accession                       : Accession,
                    ?unique                          : bool,
                    ?accessionParameters             : seq<AccessionParameter>,
                    ?searchEngines                   : seq<SearchEngine>,
                    ?reliability                     : int,
                    ?modifications                   : seq<Modification>,
                    ?retentionTime                   : RetentionTime,
                    ?charge                          : float,
                    ?massToCharge                    : float,
                    ?uri                             : string,
                    ?spectraRef                      : string, 
                    ?details                         : seq<PeptideSectionParam>
                ) =

                let id'                  = defaultArg id (System.Guid.NewGuid().ToString())
                let peptideSequence'     = defaultArg peptideSequence Unchecked.defaultof<PeptideSequence>
                let accession'           = defaultArg accession Unchecked.defaultof<Accession>
                let unique'              = defaultArg unique Unchecked.defaultof<bool>
                let accessionParameters' = convertOptionToList accessionParameters
                let searchEngines'       = convertOptionToList searchEngines
                let reliability'         = defaultArg reliability Unchecked.defaultof<int>
                let modifications'       = convertOptionToList modifications
                let retentionTime'       = defaultArg retentionTime Unchecked.defaultof<RetentionTime>
                let charge'              = defaultArg charge Unchecked.defaultof<float>
                let massToCharge'        = defaultArg massToCharge Unchecked.defaultof<float>
                let uri'                 = defaultArg uri Unchecked.defaultof<string>
                let spectraRef'          = defaultArg spectraRef Unchecked.defaultof<string>
                let details'             = convertOptionToList details

                new PeptideSection(
                                   id',
                                   peptideSequence',
                                   accession',
                                   Nullable(unique'),
                                   accessionParameters',
                                   searchEngines',
                                   Nullable(reliability'),
                                   modifications',
                                   retentionTime',
                                   Nullable(charge'),
                                   Nullable(massToCharge'),
                                   uri',
                                   spectraRef',
                                   details',
                                   Nullable(DateTime.Now)
                                  )

            ///Replaces peptideSequence of existing object with new one.
            static member addPeptideSequence
                (peptideSequence:PeptideSequence) (table:PeptideSection) =
                table.Sequence <- peptideSequence
                table
            
            ///Replaces accession of existing object with new one.
            static member addAccession
                (accession:Accession) (table:PeptideSection) =
                table.Accession <- accession
                table

            ///Replaces unique of existing object with new one.
            static member addUnique
                (unique:bool) (table:PeptideSection) =
                table.Unique <- Nullable(unique)
                table

            ///Adds a accessionParameter to an existing object.
            static member addAccessionParameter (accessionParameter:AccessionParameter) (table:PeptideSection) =
                table.AccessionParameters <- addToList table.AccessionParameters accessionParameter
                table

            ///Adds a collection of accessionParameters to an existing object.
            static member addAccessionParameters (accessionParameters:seq<AccessionParameter>) (table:PeptideSection) =
                table.AccessionParameters <- addCollectionToList table.AccessionParameters accessionParameters
                table

            ///Adds a searchEngine to an existing object.
            static member addSearchEngine (searchEngine:SearchEngine) (table:PeptideSection) =
                table.SearchEngines <- addToList table.SearchEngines searchEngine
                table

            ///Adds a collection of searchEngines to an existing object.
            static member addSearchEngines (searchEngines:seq<SearchEngine>) (table:PeptideSection) =
                table.SearchEngines <- addCollectionToList table.SearchEngines searchEngines
                table

            ///Replaces reliability of existing object with new one.
            static member addReliability
                (reliability:int) (table:PeptideSection) =
                table.Reliability <- Nullable(reliability)
                table

            ///Adds a modification to an existing object.
            static member addModification (modification:Modification) (table:PeptideSection) =
                table.Modifications <- addToList table.Modifications modification
                table

            ///Adds a modifications of searchEngines to an existing object.
            static member addModifications (modifications:seq<Modification>) (table:PeptideSection) =
                table.Modifications <- addCollectionToList table.Modifications modifications
                table

            ///Replaces retentionTime of existing object with new one.
            static member addRetentionTime
                (retentionTime:RetentionTime) (table:PeptideSection) =
                table.RetentionTime <- retentionTime
                table

            ///Replaces charge of existing object with new one.
            static member addCharge
                (charge:float) (table:PeptideSection) =
                table.Charge <- Nullable(charge)
                table

            ///Replaces charge of existing object with new one.
            static member addMassCharge
                (massToCharge:float) (table:PeptideSection) =
                table.MassToCharge <- Nullable(massToCharge)
                table

            ///Replaces uri of existing object with new one.
            static member addURI
                (uri:string) (table:PeptideSection) =
                table.URI <- uri
                table

            ///Replaces spectraRef of existing object with new one.
            static member addSpectraRef
                (spectraRef:string) (table:PeptideSection) =
                table.SpectraRef <- spectraRef
                table

            ///Adds a detail to an existing object.
            static member addDetail (detail:PeptideSectionParam) (table:PeptideSection) =
                table.Details <- addToList table.Details detail
                table

            ///Adds a collection of details to an existing object.
            static member addDetails (details:seq<PeptideSectionParam>) (table:PeptideSection) =
                table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a sample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.ProteinSection.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByMassToCharge (dbContext:MzTab) (massToCharge:Nullable<float>) =
                query {
                       for i in dbContext.PeptideSection.Local do
                           if i.MassToCharge=massToCharge
                              then select (i, i.Sequence, i.AccessionParameters, i.SearchEngines, 
                                           i.Modifications, i.RetentionTime, i.Details
                                          )
                      }
                |> Seq.map (fun (proteinSection,_, _, _, _, _, _) -> proteinSection)
                |> (fun peptideSection -> 
                    if (Seq.exists (fun peptideSection' -> peptideSection' <> null) peptideSection) = false
                        then 
                            query {
                                   for i in dbContext.PeptideSection do
                                       if i.MassToCharge=massToCharge
                                          then select (i, i.Sequence, i.AccessionParameters, i.SearchEngines, 
                                                       i.Modifications, i.RetentionTime, i.Details
                                                      )
                                  }
                            |> Seq.map (fun (peptideSection,_, _, _, _, _, _) -> peptideSection)
                            |> (fun peptideSection -> if (Seq.exists (fun peptideSection' -> peptideSection' <> null) peptideSection) = false
                                                        then None
                                                        else Some peptideSection
                               )
                        else Some peptideSection
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:PeptideSection) (item2:PeptideSection) =
               item1.Sequence=item2.Sequence && item1.Accession=item2.Accession &&
               item1.Unique=item2.Unique && item1.AccessionParameters=item2.AccessionParameters &&
               item1.SearchEngines=item2.SearchEngines && item1.Reliability=item2.Reliability &&
               item1.Modifications=item2.Modifications && item1.RetentionTime=item2.RetentionTime &&
               item1.Charge=item2.Charge && item1.URI=item2.URI && item1.SpectraRef=item2.SpectraRef && 
               item1.Details=item2.Details

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:PeptideSection) =
                    PeptideSectionHandler.tryFindByMassToCharge dbContext item.MassToCharge
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PeptideSectionHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:PeptideSection) =
                PeptideSectionHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type PSMSectionHandler =
            ///Initializes a psmSection-object with at least all necessary parameters.
            static member init
                (             
                    peptideSequence                  : PeptideSequence, 
                    psmID                            : PSMID,
                    accession                        : Accession,
                    unique                           : bool,
                    accessionParameters              : seq<AccessionParameter>,
                    searchEngines                    : seq<SearchEngine>,
                    modifications                    : seq<Modification>,
                    retentionTime                    : float,
                    charge                           : float,
                    expMassToCharge                  : float,
                    calcMassToCharge                 : float,
                    psmInformation                   : PSMInformation,
                    ?id                              : string,
                    ?reliability                     : int,
                    ?uri                             : string,
                    ?spectraRef                      : string, 
                    ?details                         : seq<PSMSectionParam>
                ) =

                let id'                = defaultArg id (System.Guid.NewGuid().ToString())
                let reliability'       = defaultArg reliability Unchecked.defaultof<int>
                let uri'               = defaultArg uri Unchecked.defaultof<string>
                let spectraRef'        = defaultArg spectraRef Unchecked.defaultof<string>
                let details'           = convertOptionToList details

                new PSMSection(
                               id',
                               peptideSequence,
                               psmID,
                               accession,
                               Nullable(unique),
                               accessionParameters |> List,
                               searchEngines |> List,
                               Nullable(reliability'),
                               modifications |> List,
                               Nullable(retentionTime),
                               Nullable(charge),
                               Nullable(expMassToCharge),
                               Nullable(calcMassToCharge),
                               uri',
                               spectraRef',
                               psmInformation,
                               details',
                               Nullable(DateTime.Now)
                              )

            ///Replaces reliability of existing object with new one.
            static member addReliability
                (reliability:int) (table:PSMSection) =
                table.Reliability <- Nullable(reliability)
                table

            ///Replaces uri of existing object with new one.
            static member addURI
                (uri:string) (table:PSMSection) =
                table.URI <- uri
                table

            ///Replaces spectraRef of existing object with new one.
            static member addSpectraRef
                (spectraRef:string) (table:PSMSection) =
                table.SpectraRef <- spectraRef
                table 

            ///Adds a detail to an existing object.
            static member addDetail (detail:PSMSectionParam) (table:PSMSection) =
                table.Details <- addToList table.Details detail
                table

            ///Adds a collection of details to an existing object.
            static member addDetails (details:seq<PSMSectionParam>) (table:PSMSection) =
                table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a psmSection-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.PSMSection.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByRetentionTime (dbContext:MzTab) (retentionTime:Nullable<float>) =
                query {
                       for i in dbContext.PSMSection.Local do
                           if i.RetentionTime=retentionTime
                              then select (i, i.Sequence, i.PSMID, i.AccessionParameters, i.SearchEngines, 
                                           i.Modifications, i.PSMInformation, i.Details
                                          )
                      }
                |> Seq.map (fun (psmSection,_, _, _, _, _, _, _) -> psmSection)
                |> (fun psmSection -> 
                    if (Seq.exists (fun psmSection' -> psmSection' <> null) psmSection) = false
                        then 
                            query {
                                   for i in dbContext.PSMSection do
                                       if i.RetentionTime=retentionTime
                                          then select (i, i.Sequence, i.PSMID, i.AccessionParameters, i.SearchEngines, 
                                                       i.Modifications, i.PSMInformation, i.Details
                                                      )
                                  }
                            |> Seq.map (fun (psmSection,_, _, _, _, _, _, _) -> psmSection)
                            |> (fun psmSection -> if (Seq.exists (fun psmSection' -> psmSection' <> null) psmSection) = false
                                                        then None
                                                        else Some psmSection
                               )
                        else Some psmSection
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:PSMSection) (item2:PSMSection) =
               item1.Sequence=item2.Sequence && item1.PSMID=item2.PSMID && item1.Accession=item2.Accession && 
               item1.Unique=item2.Unique && item1.AccessionParameters=item2.AccessionParameters &&
               item1.SearchEngines=item2.SearchEngines && item1.Reliability=item2.Reliability &&
               item1.Modifications=item2.Modifications && item1.Charge=item2.Charge 
               && item1.ExperimentalMassToCharge=item2.ExperimentalMassToCharge &&
               item1.CalculatedMassToCharge=item2.CalculatedMassToCharge && item1.URI=item2.URI && 
               item1.SpectraRef=item2.SpectraRef && item1.Details=item2.Details &&
               item1.PSMInformation=item2.PSMInformation

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:PSMSection) =
                    PSMSectionHandler.tryFindByRetentionTime dbContext item.RetentionTime
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PSMSectionHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:PSMSection) =
                PSMSectionHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type SmallMoleculeSectionHandler =
            ///Initializes a smallMoleculeSection-object with at least all necessary parameters.
            static member init
                (             
                    identifiers                      : seq<Identifier>, 
                    chemical                         : Chemical,
                    description                      : string,
                    accessionParameters              : seq<AccessionParameter>,
                    searchEngines                    : seq<SearchEngine>,
                    modifications                    : seq<Modification>,
                    retentionTime                    : float,
                    charge                           : float,
                    expMassToCharge                  : float,
                    calcMassToCharge                 : float,
                    spectraRef                       : string, 
                    ?id                              : string,
                    ?reliability                     : int,
                    ?uri                             : string,
                    ?smallMoleculeAbundances         : seq<SmallMoleculeAbundance>,
                    ?details                         : seq<SmallMoleculeSectionParam>
                ) =

                let id'                      = defaultArg id (System.Guid.NewGuid().ToString())
                let reliability'             = defaultArg reliability Unchecked.defaultof<int>
                let uri'                     = defaultArg uri Unchecked.defaultof<string>
                let smallMoleculeAbundances' = convertOptionToList smallMoleculeAbundances
                let details'                 = convertOptionToList details

                new SmallMoleculeSection(
                                         id',
                                         identifiers |> List,
                                         chemical,
                                         description,
                                         accessionParameters |> List,
                                         searchEngines |> List,
                                         Nullable(reliability'),
                                         modifications |> List,
                                         Nullable(retentionTime),
                                         Nullable(charge),
                                         Nullable(expMassToCharge),
                                         Nullable(calcMassToCharge),
                                         uri',
                                         spectraRef,
                                         smallMoleculeAbundances',
                                         details',
                                         Nullable(DateTime.Now)
                                       )

            ///Replaces reliability of existing object with new one.
            static member addReliability
                (reliability:int) (table:SmallMoleculeSection) =
                table.Reliability <- Nullable(reliability)
                table

            ///Replaces uri of existing object with new one.
            static member addURI
                (uri:string) (table:SmallMoleculeSection) =
                table.URI <- uri
                table

            ///Adds a smallMoleculeAbundance to an existing object.
            static member addSmallMoleculeAbundance (smallMoleculeAbundance:SmallMoleculeAbundance) (table:SmallMoleculeSection) =
                table.SmallMoleculeAbundances <- addToList table.SmallMoleculeAbundances smallMoleculeAbundance
                table

            ///Adds a collection of smallMoleculeAbundances to an existing object.
            static member addSmallMoleculeAbundances (smallMoleculeAbundances:seq<SmallMoleculeAbundance>) (table:SmallMoleculeSection) =
                table.SmallMoleculeAbundances <- addCollectionToList table.SmallMoleculeAbundances smallMoleculeAbundances
                table

            ///Adds a detail to an existing object.
            static member addDetail (detail:SmallMoleculeSectionParam) (table:SmallMoleculeSection) =
                table.Details <- addToList table.Details detail
                table

            ///Adds a collection of details to an existing object.
            static member addDetails (details:seq<SmallMoleculeSectionParam>) (table:SmallMoleculeSection) =
                table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a smallMoleculeSection-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.SmallMoleculeSection.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByDescription (dbContext:MzTab) (description:string) =
                query {
                       for i in dbContext.SmallMoleculeSection.Local do
                           if i.Description=description
                              then select (i, i.Identifiers, i.Chemical, i.AccessionParameters, i.SearchEngines, 
                                           i.Modifications, i.SmallMoleculeAbundances, i.Details
                                          )
                      }
                |> Seq.map (fun (smallMoleculeSection,_, _, _, _, _, _, _) -> smallMoleculeSection)
                |> (fun smallMoleculeSection -> 
                    if (Seq.exists (fun smallMoleculeSection' -> smallMoleculeSection' <> null) smallMoleculeSection) = false
                        then 
                            query {
                                   for i in dbContext.SmallMoleculeSection do
                                       if i.Description=description
                                          then select (i, i.Identifiers, i.Chemical, i.AccessionParameters, i.SearchEngines, 
                                                       i.Modifications, i.SmallMoleculeAbundances, i.Details
                                                      )
                                  }
                            |> Seq.map (fun (smallMoleculeSection,_, _, _, _, _, _, _) -> smallMoleculeSection)
                            |> (fun smallMoleculeSection -> if (Seq.exists (fun smallMoleculeSection' -> smallMoleculeSection' <> null) smallMoleculeSection) = false
                                                            then None
                                                            else Some smallMoleculeSection
                               )
                        else Some smallMoleculeSection
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SmallMoleculeSection) (item2:SmallMoleculeSection) =
               item1.Identifiers=item2.Identifiers && item1.Description=item2.Description && 
               item1.Chemical=item2.Chemical && item1.AccessionParameters=item2.AccessionParameters &&
               item1.SearchEngines=item2.SearchEngines && item1.Reliability=item2.Reliability &&
               item1.Modifications=item2.Modifications && item1.Charge=item2.Charge 
               && item1.ExperimentalMassToCharge=item2.ExperimentalMassToCharge &&
               item1.CalculatedMassToCharge=item2.CalculatedMassToCharge && item1.URI=item2.URI && 
               item1.SpectraRef=item2.SpectraRef && item1.Details=item2.Details &&
               item1.SmallMoleculeAbundances=item2.SmallMoleculeAbundances

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:SmallMoleculeSection) =
                    SmallMoleculeSectionHandler.tryFindByDescription dbContext item.Description
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SmallMoleculeSectionHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:SmallMoleculeSection) =
                SmallMoleculeSectionHandler.addToContext dbContext item
                dbContext.SaveChanges()

        type MetaDataSectionHandler =
            ///Initializes a metaDataSection-object with at least all necessary parameters.
            static member init
                (             
                    description                      : string,
                    version                          : string,
                    mode                             : MzTabMode,
                    mzType                           : MzType,
                    searchEngineScores               : seq<SearchEngineScore>,
                    fixedModifications               : seq<FixedModification>, 
                    variableModifications            : seq<VariableModification>, 
                    ?id                              : string,
                    ?title                           : string,
                    ?sampleProcessings               : seq<SampleProcessing>,
                    ?instruments                     : seq<Instrument>,
                    ?analysisSoftwares               : seq<AnalysisSoftware>,
                    ?falseDiscoveryRates             : seq<FalseDiscoveryRate>,
                    ?publications                    : string,
                    ?persons                         : seq<Person>, 
                    ?uri                             : string,
                    ?quantification                  : Quantification,
                    ?msRuns                          : seq<MSRun>,
                    ?samples                         : seq<Sample>, 
                    ?assays                          : seq<Assay>,
                    ?studyVariables                  : seq<StudyVariable>, 
                    ?colUnit                         : ColUnit,
                    ?proteinSections                 : seq<ProteinSection>, 
                    ?peptideSections                 : seq<PeptideSection>, 
                    ?psmSections                     : seq<PSMSection>, 
                    ?smallMoleculeSections           : seq<SmallMoleculeSection>,
                    ?details                         : seq<MetaDataSectionParam>
                ) =

                let id'                    = defaultArg id (System.Guid.NewGuid().ToString())
                let title'                 = defaultArg title Unchecked.defaultof<string>
                let sampleProcessings'     = convertOptionToList sampleProcessings
                let instruments'           = convertOptionToList instruments
                let analysisSoftwares'     = convertOptionToList analysisSoftwares
                let falseDiscoveryRates'   = convertOptionToList falseDiscoveryRates
                let publications'          = defaultArg publications Unchecked.defaultof<string>
                let persons'               = convertOptionToList persons
                let uri'                   = defaultArg uri Unchecked.defaultof<string>
                let quantification'        = defaultArg quantification Unchecked.defaultof<Quantification>
                let msRuns'                = convertOptionToList msRuns
                let samples'               = convertOptionToList samples
                let assays'                = convertOptionToList assays
                let studyVariables'        = convertOptionToList studyVariables
                let colUnit'               = defaultArg colUnit Unchecked.defaultof<ColUnit>
                let proteinSections'       = convertOptionToList proteinSections
                let peptideSections'       = convertOptionToList peptideSections
                let psmSections'           = convertOptionToList psmSections
                let smallMoleculeSections' = convertOptionToList smallMoleculeSections
                let details'               = convertOptionToList details

                new MetaDataSection(
                                    id',
                                    title',
                                    description,
                                    version,
                                    Nullable(mode),
                                    Nullable(mzType),
                                    sampleProcessings' |> List,
                                    instruments',
                                    analysisSoftwares',
                                    searchEngineScores |> List,
                                    falseDiscoveryRates',
                                    publications',
                                    persons',
                                    uri',
                                    fixedModifications |> List,
                                    variableModifications |> List,
                                    quantification',
                                    msRuns',
                                    samples',
                                    assays',
                                    studyVariables',
                                    colUnit',
                                    proteinSections',
                                    peptideSections',
                                    psmSections',
                                    smallMoleculeSections',
                                    details',
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces title of existing object with new one.
            static member addTitle
                (title:string) (table:MetaDataSection) =
                table.Title <- title
                table

            ///Adds a sampleProcessing to an existing object.
            static member addSampleProcessing (sampleProcessing:SampleProcessing) (table:MetaDataSection) =
                table.SampleProcessings <- addToList table.SampleProcessings sampleProcessing
                table

            ///Adds a collection of sampleProcessings to an existing object.
            static member addSampleProcessings (sampleProcessings:seq<SampleProcessing>) (table:MetaDataSection) =
                table.SampleProcessings <- addCollectionToList table.SampleProcessings sampleProcessings
                table

            ///Adds a instrument to an existing object.
            static member addInstrument (instrument:Instrument) (table:MetaDataSection) =
                table.Instruments <- addToList table.Instruments instrument
                table

            ///Adds a collection of instruments to an existing object.
            static member addInstruments (instruments:seq<Instrument>) (table:MetaDataSection) =
                table.Instruments <- addCollectionToList table.Instruments instruments
                table
                
            ///Adds a analysisSoftware to an existing object.
            static member addAnalysisSoftware (analysisSoftware:AnalysisSoftware) (table:MetaDataSection) =
                table.AnalysisSoftwares <- addToList table.AnalysisSoftwares analysisSoftware
                table

            ///Adds a collection of analysisSoftwares to an existing object.
            static member addAnalysisSoftwares (analysisSoftwares:seq<AnalysisSoftware>) (table:MetaDataSection) =
                table.AnalysisSoftwares <- addCollectionToList table.AnalysisSoftwares analysisSoftwares
                table

            ///Adds a falseDiscoveryRate to an existing object.
            static member addFalseDiscoveryRates (falseDiscoveryRate:FalseDiscoveryRate) (table:MetaDataSection) =
                table.FalseDiscoveryRates <- addToList table.FalseDiscoveryRates falseDiscoveryRate
                table

            ///Adds a collection of falseDiscoveryRates to an existing object.
            static member addFalseDiscoveryRate (falseDiscoveryRates:seq<FalseDiscoveryRate>) (table:MetaDataSection) =
                table.FalseDiscoveryRates <- addCollectionToList table.FalseDiscoveryRates falseDiscoveryRates
                table

            ///Replaces publication of existing object with new one.
            static member addPublication
                (publication:string) (table:MetaDataSection) =
                table.Publications <- publication
                table

            ///Adds a person to an existing object.
            static member addPerson (person:Person) (table:MetaDataSection) =
                table.Persons <- addToList table.Persons person
                table

            ///Adds a collection of persons to an existing object.
            static member addPersons (persons:seq<Person>) (table:MetaDataSection) =
                table.Persons <- addCollectionToList table.Persons persons
                table

            ///Replaces uri of existing object with new one.
            static member addURI
                (uri:string) (table:MetaDataSection) =
                table.URI <- uri
                table

            ///Replaces quantification of existing object with new one.
            static member addQuantification
                (quantification:Quantification) (table:MetaDataSection) =
                table.Quantification <- quantification
                table

            ///Adds a msRun to an existing object.
            static member addMSRun (msRun:MSRun) (table:MetaDataSection) =
                table.MSRuns <- addToList table.MSRuns msRun
                table

            ///Adds a collection of msRuns to an existing object.
            static member addMSRuns (msRuns:seq<MSRun>) (table:MetaDataSection) =
                table.MSRuns <- addCollectionToList table.MSRuns msRuns
                table

            ///Adds a sample to an existing object.
            static member addSample (sample:Sample) (table:MetaDataSection) =
                table.Samples <- addToList table.Samples sample
                table

            ///Adds a collection of samples to an existing object.
            static member addSamples (samples:seq<Sample>) (table:MetaDataSection) =
                table.Samples <- addCollectionToList table.Samples samples
                table

            ///Adds a assay to an existing object.
            static member addAssay (assay:Assay) (table:MetaDataSection) =
                table.Assays <- addToList table.Assays assay
                table

            ///Adds a collection of assays to an existing object.
            static member addAssays (assays:seq<Assay>) (table:MetaDataSection) =
                table.Assays <- addCollectionToList table.Assays assays
                table

            ///Adds a studyVariable to an existing object.
            static member addStudyVariable (studyVariable:StudyVariable) (table:MetaDataSection) =
                table.StudyVariables <- addToList table.StudyVariables studyVariable
                table

            ///Adds a collection of studyVariables to an existing object.
            static member addStudyVariables (studyVariables:seq<StudyVariable>) (table:MetaDataSection) =
                table.StudyVariables <- addCollectionToList table.StudyVariables studyVariables
                table

            ///Replaces colUnit of existing object with new one.
            static member addColUnit
                (colUnit:ColUnit) (table:MetaDataSection) =
                table.ColUnit <- colUnit
                table

            ///Adds a proteinSection to an existing object.
            static member addProteinSection (proteinSection:ProteinSection) (table:MetaDataSection) =
                table.ProteinSections <- addToList table.ProteinSections proteinSection
                table

            ///Adds a collection of proteinSections to an existing object.
            static member addProteinSections (proteinSections:seq<ProteinSection>) (table:MetaDataSection) =
                table.ProteinSections <- addCollectionToList table.ProteinSections proteinSections
                table
            
            ///Adds a peptideSection to an existing object.
            static member addPeptideSection (peptideSection:PeptideSection) (table:MetaDataSection) =
                table.PeptideSections <- addToList table.PeptideSections peptideSection
                table

            ///Adds a collection of peptideSections to an existing object.
            static member addPeptideSections (peptideSections:seq<PeptideSection>) (table:MetaDataSection) =
                table.PeptideSections <- addCollectionToList table.PeptideSections peptideSections
                table

            ///Adds a psmSection to an existing object.
            static member addPSMSection (psmSection:PSMSection) (table:MetaDataSection) =
                table.PSMSections <- addToList table.PSMSections psmSection
                table

            ///Adds a collection of psmSections to an existing object.
            static member addPSMSections (psmSections:seq<PSMSection>) (table:MetaDataSection) =
                table.PSMSections <- addCollectionToList table.PSMSections psmSections
                table

            ///Adds a smallMoleculeSection to an existing object.
            static member addSmallMoleculeSection (smallMoleculeSection:SmallMoleculeSection) (table:MetaDataSection) =
                table.SmallMoleculeSections <- addToList table.SmallMoleculeSections smallMoleculeSection
                table

            ///Adds a collection of smallMoleculeSections to an existing object.
            static member addSmallMoleculeSections (smallMoleculeSections:seq<SmallMoleculeSection>) (table:MetaDataSection) =
                table.SmallMoleculeSections <- addCollectionToList table.SmallMoleculeSections smallMoleculeSections
                table

            ///Adds a detail to an existing object.
            static member addDetail (detail:MetaDataSectionParam) (table:MetaDataSection) =
                table.Details <- addToList table.Details detail
                table

            ///Adds a collection of details to an existing object.
            static member addDetails (details:seq<MetaDataSectionParam>) (table:MetaDataSection) =
                table.Details <- addCollectionToList table.Details details
                table

            ///Tries to find a metaDataSection-object in the context and database, based on its primary-key(ID).
            static member tryFindByID
                (context:MzTab) (id:string) =
                tryFind (context.MetaDataSection.Find(id))

            ///Tries to find a searchEngineScore-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByDescription (dbContext:MzTab) (description:string) =
                query {
                       for i in dbContext.MetaDataSection.Local do
                           if i.Description=description
                              then select (i, i.Mode, i.Type, i.SampleProcessings, i.Instruments, 
                                           i.AnalysisSoftwares, i.SearchEngineScores, i.FalseDiscoveryRates,
                                           i.Persons, i.FixedModifications, i.VariableModifications, 
                                           i.Quantification, i.MSRuns, i.Samples, i.Assays, i.Details,
                                           i.StudyVariables, i.ColUnit, i.ProteinSections, i.PSMSections, 
                                           i.PeptideSections, i.SmallMoleculeSections
                                          )
                      }
                |> Seq.map (fun (metaDataSection,_, _, _, _, _, _, _, _,_, _, _, _, _, _, _, _, _, _, _, _, _) -> metaDataSection)
                |> (fun metaDataSection -> 
                    if (Seq.exists (fun metaDataSection' -> metaDataSection' <> null) metaDataSection) = false
                        then 
                            query {
                                   for i in dbContext.MetaDataSection do
                                       if i.Description=description
                                          then select (i, i.Mode, i.Type, i.SampleProcessings, i.Instruments, 
                                                       i.AnalysisSoftwares, i.SearchEngineScores, i.FalseDiscoveryRates,
                                                       i.Persons, i.FixedModifications, i.VariableModifications, 
                                                       i.Quantification, i.MSRuns, i.Samples, i.Assays, i.Details,
                                                       i.StudyVariables, i.ColUnit, i.ProteinSections, i.PSMSections, 
                                                       i.PeptideSections, i.SmallMoleculeSections
                                                      )
                                  }
                            |> Seq.map (fun (metaDataSection,_, _, _, _, _, _, _, _,_, _, _, _, _, _, _, _, _, _, _, _, _) -> metaDataSection)
                            |> (fun metaDataSection -> if (Seq.exists (fun metaDataSection' -> metaDataSection' <> null) metaDataSection) = false
                                                            then None
                                                            else Some metaDataSection
                               )
                        else Some metaDataSection
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:MetaDataSection) (item2:MetaDataSection) =
               item1.Title=item2.Title && item1.Version=item2.Version && item1.Mode=item2.Mode && 
               item1.Type=item2.Type && item1.SampleProcessings=item2.SampleProcessings && 
               item1.Instruments=item2.Instruments && item1.AnalysisSoftwares=item2.AnalysisSoftwares && 
               item1.SearchEngineScores=item2.SearchEngineScores && item1.Publications=item2.Publications &&
               item1.FalseDiscoveryRates=item2.FalseDiscoveryRates && item1.Persons=item2.Persons && 
               item1.URI=item2.URI && item1.FixedModifications=item2.FixedModifications && 
               item1.VariableModifications=item2.VariableModifications && item1.MSRuns=item2.MSRuns &&
               item1.Quantification=item2.Quantification && item1.Samples=item2.Samples && 
               item1.Assays=item2.Assays && item1.StudyVariables=item2.StudyVariables &&
               item1.ColUnit=item2.ColUnit && item1.ProteinSections=item2.ProteinSections &&
               item1.PeptideSections=item2.PeptideSections && item1.PSMSections=item2.PSMSections &&
               item1.SmallMoleculeSections=item2.SmallMoleculeSections && item1.Details=item2.Details

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzTab) (item:MetaDataSection) =
                    MetaDataSectionHandler.tryFindByDescription dbContext item.Description
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match MetaDataSectionHandler.hasEqualFieldValues organization item with
                                                                                                 |true -> null
                                                                                                 |false -> dbContext.Add item
                                                                            ) |> ignore
                                                      |None -> dbContext.Add item |> ignore
                       )

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
            static member addToContextAndInsert (dbContext:MzTab) (item:MetaDataSection) =
                MetaDataSectionHandler.addToContext dbContext item
                dbContext.SaveChanges()