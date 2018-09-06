namespace MzIdentMLDataBase

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
                            if item1.Term=item2.Term && item1.TermID=item2.TermID && item2.Unit=item2.Unit && 
                               item2.UnitID=item2.UnitID && item1.Value=item2.Value
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
            static member sqliteConnection (path:string) =
                let optionsBuilder = 
                    new DbContextOptionsBuilder<MzIdentML>()
                optionsBuilder.UseSqlite(@"Data Source=" + path) |> ignore
                new MzIdentML(optionsBuilder.Options)       

            ///Creats connection for SQL-context and database.
            static member sqlConnection() (dbName:string)=
                let optionsBuilder = 
                    new DbContextOptionsBuilder<MzIdentML>()
                optionsBuilder.UseSqlServer("Server=(localdb)\mssqllocaldb;Database=" + dbName + ";Trusted_Connection=True;") |> ignore
                new MzIdentML(optionsBuilder.Options) 

            ///Reads obo-file and creates sequence of Obo.Terms.
            static member fromFileObo (filePath:string) =
                FileIO.readFile filePath
                |> Obo.parseOboTerms

            ///Tries to add the object to the database-context.
            static member tryAddToContext (context:MzIdentML) (item:'b) =
                context.Add(item)

            ///Tries to add the object to the database-context and insert it in the database.
            static member tryAddToContextAndInsert (context:MzIdentML) (item:'b) =
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
                table.OntologyID <- ontologyID
                table

            ///Tries to find a term-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (ontologyID:string) =
                query {
                        for i in dbContext.Term.Local do
                            if i.OntologyID=ontologyID
                                then select (i, i.Ontology)
                        }
                |> Seq.map (fun (term,_) -> term)
                |> (fun term -> 
                    if (Seq.exists (fun term' -> term' <> null) term) = false
                        then 
                            query {
                                    for i in dbContext.Term do
                                        if i.OntologyID=ontologyID
                                            then select (i, i.Ontology)
                                    }
                            |> Seq.map (fun (term,_) -> term)
                            |> (fun term -> if (Seq.exists (fun term' -> term' <> null) term) = false
                                                then None
                                                else Some (term.Single())
                                )
                        else Some (term.Single())
                    )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                        for i in dbContext.Term.Local do
                            if i.Name=name
                                then select (i, i.Ontology)
                        }
                |> Seq.map (fun (term,_) -> term)
                |> (fun term -> 
                    if (Seq.exists (fun term' -> term' <> null) term) = false
                        then 
                            query {
                                    for i in dbContext.Term do
                                        if i.Name=name
                                            then select (i, i.Ontology)
                                    }
                            |> Seq.map (fun (term,_) -> term)
                            |> (fun term -> if (Seq.exists (fun term' -> term' <> null) term) = false
                                                then None
                                                else Some term
                                )
                        else Some term
                    )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Term) (item2:Term) =
                item1.Ontology.ID=item2.Ontology.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Term) =
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Term) =
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
                (term:Term) (ontology:Ontology) =
                let result = ontology.Terms <- addToList ontology.Terms term
                ontology

            ///Adds a collection of terms to an existing ontology-object.
            static member addTerms
                (terms:seq<Term>) (ontology:Ontology) =
                let result = ontology.Terms <- addCollectionToList ontology.Terms terms
                ontology

            ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
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
            static member addUnit
                (fkUnit:string) (table:CVParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a cvparam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
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
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:CVParam) =
                    CVParamHandler.tryFindByTermName dbContext item.Term.Name
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:CVParam) =
                CVParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type OrganizationParamHandler =
            ///Initializes a organizationparam-object with at least all necessary parameters.
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

                new OrganizationParam(
                                      id', 
                                      value',
                                      null,
                                      fkTerm,
                                      null,
                                      fkUnit', 
                                      Nullable(DateTime.Now)
                                     )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:OrganizationParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:OrganizationParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a organizationParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
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
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:OrganizationParam) =
                    OrganizationParamHandler.tryFindByTermName dbContext item.Term.Name
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:OrganizationParam) =
                OrganizationParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type PersonParamHandler =
            ///Initializes a personparam-object with at least all necessary parameters.
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
                    
                new PersonParam(
                                id', 
                                value',
                                null,
                                fkTerm,
                                null,
                                fkUnit', 
                                Nullable(DateTime.Now)
                               )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:PersonParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:PersonParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a personParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
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
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:PersonParam) =
                    PersonParamHandler.tryFindByTermName dbContext item.Term.Name
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:PersonParam) =
                PersonParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type SampleParamHandler =
            ///Initializes a sampleparam-object with at least all necessary parameters.
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
                    
                new SampleParam(
                                id', 
                                value',
                                null,
                                fkTerm,
                                null,
                                fkUnit', 
                                Nullable(DateTime.Now)
                               )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SampleParam)  =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:SampleParam)  =
                table.UnitID <- fkUnit
                table

            ///Tries to find an sampleParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SampleParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SampleParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
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
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SampleParam) =
                    SampleParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SampleParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SampleParam) =
                SampleParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type ModificationParamHandler =
            ///Initializes a modificationparam-object with at least all necessary parameters.
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
                    
                new ModificationParam(
                                      id', 
                                      value',
                                      null,
                                      fkTerm,
                                      null,
                                      fkUnit', 
                                      Nullable(DateTime.Now)
                                     )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:ModificationParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:ModificationParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a modificationParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.ModificationParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ModificationParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.ModificationParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ModificationParam do
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
            static member private hasEqualFieldValues (item1:ModificationParam) (item2:ModificationParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ModificationParam) =
                    ModificationParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ModificationParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:ModificationParam) =
                ModificationParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type PeptideParamHandler =
            ///Initializes a peptideparam-object with at least all necessary parameters.
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
                    
                new PeptideParam(
                                 id', 
                                 value',
                                 null,
                                 fkTerm,
                                 null,
                                 fkUnit', 
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:PeptideParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:PeptideParam)  =
                table.UnitID <- fkUnit
                table

            ///Tries to find a peptideParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.PeptideParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PeptideParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.PeptideParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PeptideParam do
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
            static member private hasEqualFieldValues (item1:PeptideParam) (item2:PeptideParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:PeptideParam) =
                    PeptideParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PeptideParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:PeptideParam) =
                PeptideParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type TranslationTableParamHandler =
            ///Initializes a translationtableparam-object with at least all necessary parameters.
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
                    
                new TranslationTableParam(
                                          id', 
                                          value',
                                          null,
                                          fkTerm,
                                          null,
                                          fkUnit', 
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:TranslationTableParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:TranslationTableParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a translationTableParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.TranslationTableParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.TranslationTableParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.TranslationTableParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.TranslationTableParam do
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
            static member private hasEqualFieldValues (item1:TranslationTableParam) (item2:TranslationTableParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:TranslationTableParam) =
                    TranslationTableParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match TranslationTableParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:TranslationTableParam) =
                TranslationTableParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type MeasureParamHandler =
            ///Initializes a measureparam-object with at least all necessary parameters.
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
                    
                new MeasureParam(
                                 id', 
                                 value',
                                 null,
                                 fkTerm,
                                 null,
                                 fkUnit', 
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:MeasureParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:MeasureParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a measureParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.MeasureParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.MeasureParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.MeasureParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.MeasureParam do
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
            static member private hasEqualFieldValues (item1:MeasureParam) (item2:MeasureParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:MeasureParam) =
                    MeasureParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match MeasureParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:MeasureParam) =
                MeasureParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type AmbiguousResidueParamHandler =
            ///Initializes a ambiguousresidueparam-object with at least all necessary parameters.
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

                new AmbiguousResidueParam(
                                          id', 
                                          value',
                                          null,
                                          fkTerm,
                                          null,
                                          fkUnit', 
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:AmbiguousResidueParam)  =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:AmbiguousResidueParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a ambiguousResidueParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.AmbiguousResidueParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.AmbiguousResidueParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.AmbiguousResidueParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.AmbiguousResidueParam do
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
            static member private hasEqualFieldValues (item1:AmbiguousResidueParam) (item2:AmbiguousResidueParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:AmbiguousResidueParam) =
                    AmbiguousResidueParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match AmbiguousResidueParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:AmbiguousResidueParam) =
                AmbiguousResidueParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type MassTableParamHandler =
            ///Initializes a masstableparam-object with at least all necessary parameters.
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
                    
                new MassTableParam(
                                   id', 
                                   value',
                                   null,
                                   fkTerm,
                                   null,
                                   fkUnit', 
                                   Nullable(DateTime.Now)
                                  )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:MassTableParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:MassTableParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a massTableParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.MassTableParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.MassTableParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.MassTableParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.MassTableParam do
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
            static member private hasEqualFieldValues (item1:MassTableParam) (item2:MassTableParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:MassTableParam) =
                    MassTableParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match MassTableParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:MassTableParam) =
                MassTableParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type IonTypeParamHandler =
            ///Initializes a iontypeparam-object with at least all necessary parameters.
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
                    
                new IonTypeParam(
                                 id', 
                                 value',
                                 null,
                                 fkTerm,
                                 null,
                                 fkUnit', 
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:IonTypeParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:IonTypeParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a ionTypeParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.IonTypeParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.IonTypeParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.IonTypeParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.IonTypeParam do
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
            static member private hasEqualFieldValues (item1:IonTypeParam) (item2:IonTypeParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:IonTypeParam) =
                    IonTypeParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match IonTypeParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:IonTypeParam) =
                IonTypeParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type SpecificityRuleParamHandler =
            ///Initializes a specificityruleparam-object with at least all necessary parameters.
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

                new SpecificityRuleParam(
                                         id', 
                                         value',
                                         null,
                                         fkTerm,
                                         null,
                                         fkUnit', 
                                         Nullable(DateTime.Now)
                                        )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SpecificityRuleParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:SpecificityRuleParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a specificityRuleParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SpecificityRuleParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SpecificityRuleParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.SpecificityRuleParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SpecificityRuleParam do
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
            static member private hasEqualFieldValues (item1:SpecificityRuleParam) (item2:SpecificityRuleParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SpecificityRuleParam) =
                    SpecificityRuleParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SpecificityRuleParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SpecificityRuleParam) =
                SpecificityRuleParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type SearchModificationParamHandler =
            ///Initializes a searchmodificationparam-object with at least all necessary parameters.
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
                    
                new SearchModificationParam(
                                            id', 
                                            value',
                                            null,
                                            fkTerm,
                                            null,
                                            fkUnit', 
                                            Nullable(DateTime.Now)
                                           )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SearchModificationParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:SearchModificationParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a searchModificationParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SearchModificationParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SearchModificationParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.SearchModificationParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SearchModificationParam do
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
            static member private hasEqualFieldValues (item1:SearchModificationParam) (item2:SearchModificationParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SearchModificationParam) =
                    SearchModificationParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SearchModificationParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SearchModificationParam) =
                SearchModificationParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type EnzymeNameParamHandler =
            ///Initializes a enzymenameparam-object with at least all necessary parameters.
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
                    
                new EnzymeNameParam(
                                    id', 
                                    value',
                                    null,
                                    fkTerm,
                                    null,
                                    fkUnit', 
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:EnzymeNameParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:EnzymeNameParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a enzymeNameParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.EnzymeNameParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.EnzymeNameParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.EnzymeNameParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.EnzymeNameParam do
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
            static member private hasEqualFieldValues (item1:EnzymeNameParam) (item2:EnzymeNameParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:EnzymeNameParam) =
                    EnzymeNameParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match EnzymeNameParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:EnzymeNameParam) =
                EnzymeNameParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type IncludeParamHandler =
            ///Initializes a includeparam-object with at least all necessary parameters.
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
                    
                new IncludeParam(
                                 id', 
                                 value',
                                 null,
                                 fkTerm,
                                 null,
                                 fkUnit', 
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:IncludeParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:IncludeParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a includeParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.IncludeParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.IncludeParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.IncludeParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.IncludeParam do
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
            static member private hasEqualFieldValues (item1:IncludeParam) (item2:IncludeParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:IncludeParam) =
                    IncludeParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match IncludeParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:IncludeParam) =
                IncludeParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type ExcludeParamHandler =
            ///Initializes a excludeparam-object with at least all necessary parameters.
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
                    
                new ExcludeParam(
                                 id', 
                                 value',
                                 null,
                                 fkTerm,
                                 null,
                                 fkUnit', 
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:ExcludeParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:ExcludeParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a excludeParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.ExcludeParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ExcludeParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.ExcludeParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ExcludeParam do
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
            static member private hasEqualFieldValues (item1:ExcludeParam) (item2:ExcludeParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ExcludeParam) =
                    ExcludeParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ExcludeParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:ExcludeParam) =
                ExcludeParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type AdditionalSearchParamHandler =
            ///Initializes a additionalssearchparam-object with at least all necessary parameters.
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

                new AdditionalSearchParam(
                                          id', 
                                          value',
                                          null,
                                          fkTerm,
                                          null,
                                          fkUnit', 
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:AdditionalSearchParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:AdditionalSearchParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a additionalSearchParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.AdditionalSearchParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.AdditionalSearchParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.AdditionalSearchParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.AdditionalSearchParam do
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
            static member private hasEqualFieldValues (item1:AdditionalSearchParam) (item2:AdditionalSearchParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:AdditionalSearchParam) =
                    AdditionalSearchParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match AdditionalSearchParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:AdditionalSearchParam) =
                AdditionalSearchParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type FragmentToleranceParamHandler =
            ///Initializes a fragmenttoleranceparam-object with at least all necessary parameters.
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
                    
                new FragmentToleranceParam(
                                           id', 
                                           value',
                                           null,
                                           fkTerm,
                                           null,
                                           fkUnit', 
                                           Nullable(DateTime.Now)
                                          )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:FragmentToleranceParam)  =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:FragmentToleranceParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a fragmentToleranceParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.FragmentToleranceParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.FragmentToleranceParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.FragmentToleranceParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.FragmentToleranceParam do
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
            static member private hasEqualFieldValues (item1:FragmentToleranceParam) (item2:FragmentToleranceParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:FragmentToleranceParam) =
                    FragmentToleranceParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match FragmentToleranceParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:FragmentToleranceParam) =
                FragmentToleranceParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type ParentToleranceParamHandler =
            ///Initializes a parenttoleranceparam-object with at least all necessary parameters.
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
                    
                new ParentToleranceParam(
                                         id', 
                                         value',
                                         null,
                                         fkTerm,
                                         null,
                                         fkUnit', 
                                         Nullable(DateTime.Now)
                                        )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:ParentToleranceParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:ParentToleranceParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a parentToleranceParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.ParentToleranceParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ParentToleranceParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.ParentToleranceParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ParentToleranceParam do
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
            static member private hasEqualFieldValues (item1:ParentToleranceParam) (item2:ParentToleranceParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ParentToleranceParam) =
                    ParentToleranceParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ParentToleranceParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:ParentToleranceParam) =
                ParentToleranceParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type ThresholdParamHandler =
            ///Initializes a thresholdparam-object with at least all necessary parameters.
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
                    
                new ThresholdParam(
                                   id', 
                                   value',
                                   null,
                                   fkTerm,
                                   null,
                                   fkUnit', 
                                   Nullable(DateTime.Now)
                                  )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:ThresholdParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFkUnit
                (fkUnit:string) (table:ThresholdParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a thresholdParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.ThresholdParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ThresholdParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.ThresholdParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ThresholdParam do
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
            static member private hasEqualFieldValues (item1:ThresholdParam) (item2:ThresholdParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ThresholdParam) =
                    ThresholdParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ThresholdParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:ThresholdParam) =
                ThresholdParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type SearchDatabaseParamHandler =
            ///Initializes a searchdatabaseparam-object with at least all necessary parameters.
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
                    
                new SearchDatabaseParam(
                                        id', 
                                        value',
                                        null,
                                        fkTerm,
                                        null,
                                        fkUnit', 
                                        Nullable(DateTime.Now)
                                       )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SearchDatabaseParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:SearchDatabaseParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a searchDatabaseParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
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
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SearchDatabaseParam) =
                    SearchDatabaseParamHandler.tryFindByTermName dbContext item.Term.Name
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SearchDatabaseParam) =
                SearchDatabaseParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type DBSequenceParamHandler =
            ///Initializes a dbsequenceparam-object with at least all necessary parameters.
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
                    
                new DBSequenceParam(
                                    id', 
                                    value',
                                    null,
                                    fkTerm,
                                    null,
                                    fkUnit', 
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:DBSequenceParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:DBSequenceParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a dbSequenceParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.DBSequenceParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.DBSequenceParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.DBSequenceParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.DBSequenceParam do
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
            static member private hasEqualFieldValues (item1:DBSequenceParam) (item2:DBSequenceParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:DBSequenceParam) =
                    DBSequenceParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match DBSequenceParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:DBSequenceParam) =
                DBSequenceParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type PeptideEvidenceParamHandler =
            ///Initializes a peptideevidenceparam-object with at least all necessary parameters.
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
                    
                new PeptideEvidenceParam(
                                         id', 
                                         value',
                                         null,
                                         fkTerm,
                                         null,
                                         fkUnit', 
                                         Nullable(DateTime.Now)
                                        )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:PeptideEvidenceParam)  =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:PeptideEvidenceParam)  =
                table.UnitID <- fkUnit
                table

            ///Tries to find a peptideEvidenceParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.PeptideEvidenceParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PeptideEvidenceParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.PeptideEvidenceParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PeptideEvidenceParam do
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
            static member private hasEqualFieldValues (item1:PeptideEvidenceParam) (item2:PeptideEvidenceParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:PeptideEvidenceParam) =
                    PeptideEvidenceParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PeptideEvidenceParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:PeptideEvidenceParam) =
                PeptideEvidenceParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationItemParamHandler =
            ///Initializes a spectrumidentificationparam-object with at least all necessary parameters.
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

                new SpectrumIdentificationItemParam(
                                                    id', 
                                                    value',
                                                    null,
                                                    fkTerm,
                                                    null,
                                                    fkUnit', 
                                                    Nullable(DateTime.Now)
                                                   )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SpectrumIdentificationItemParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:SpectrumIdentificationItemParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a spectrumIdentificationItemParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SpectrumIdentificationItemParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationItemParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.SpectrumIdentificationItemParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationItemParam do
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
            static member private hasEqualFieldValues (item1:SpectrumIdentificationItemParam) (item2:SpectrumIdentificationItemParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SpectrumIdentificationItemParam) =
                    SpectrumIdentificationItemParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SpectrumIdentificationItemParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SpectrumIdentificationItemParam) =
                SpectrumIdentificationItemParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationResultParamHandler =
            ///Initializes a spectrumidentificationresultparam-object with at least all necessary parameters.
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

                new SpectrumIdentificationResultParam(
                                                      id', 
                                                      value',
                                                      null,
                                                      fkTerm,
                                                      null,
                                                      fkUnit', 
                                                      Nullable(DateTime.Now)
                                                     )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SpectrumIdentificationResultParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:SpectrumIdentificationResultParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a spectrumIdentificationResultParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SpectrumIdentificationResultParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationResultParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.SpectrumIdentificationResultParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationResultParam do
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
            static member private hasEqualFieldValues (item1:SpectrumIdentificationResultParam) (item2:SpectrumIdentificationResultParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SpectrumIdentificationResultParam) =
                    SpectrumIdentificationResultParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SpectrumIdentificationResultParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SpectrumIdentificationResultParam) =
                SpectrumIdentificationResultParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationListParamHandler =
            ///Initializes a spectrumidentificationlistparam-object with at least all necessary parameters.
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

                new SpectrumIdentificationListParam(
                                                    id', 
                                                    value',
                                                    null,
                                                    fkTerm,
                                                    null,
                                                    fkUnit', 
                                                    Nullable(DateTime.Now)
                                                   )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SpectrumIdentificationListParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:SpectrumIdentificationListParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a spectrumIdentificationListParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SpectrumIdentificationListParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationListParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.SpectrumIdentificationListParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationListParam do
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
            static member private hasEqualFieldValues (item1:SpectrumIdentificationListParam) (item2:SpectrumIdentificationListParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SpectrumIdentificationListParam) =
                    SpectrumIdentificationListParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SpectrumIdentificationListParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SpectrumIdentificationListParam) =
                SpectrumIdentificationListParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type AnalysisParamHandler =
            ///Initializes a analysisparam-object with at least all necessary parameters.
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
                    
                new AnalysisParam(
                                  id', 
                                  value',
                                  null,
                                  fkTerm,
                                  null,
                                  fkUnit', 
                                  Nullable(DateTime.Now)
                                 )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:AnalysisParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:AnalysisParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a analysisParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.AnalysisParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.AnalysisParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisParam do
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
            static member private hasEqualFieldValues (item1:AnalysisParam) (item2:AnalysisParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:AnalysisParam) =
                    AnalysisParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match AnalysisParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:AnalysisParam) =
                AnalysisParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type SourceFileParamHandler =
            ///Initializes a sourcefileparam-object with at least all necessary parameters.
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
                    
                new SourceFileParam(
                                    id', 
                                    value',
                                    null,
                                    fkTerm,
                                    null,
                                    fkUnit', 
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SourceFileParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:SourceFileParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a sourceFileParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SourceFileParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SourceFileParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.SourceFileParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SourceFileParam do
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
            static member private hasEqualFieldValues (item1:SourceFileParam) (item2:SourceFileParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SourceFileParam) =
                    SourceFileParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SourceFileParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SourceFileParam) =
                SourceFileParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type ProteinDetectionHypothesisParamHandler =
            ///Initializes a proteindetectionhypothesisparam-object with at least all necessary parameters.
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
                    
                new ProteinDetectionHypothesisParam(
                                                    id', 
                                                    value',
                                                    null,
                                                    fkTerm,
                                                    null,
                                                    fkUnit', 
                                                    Nullable(DateTime.Now)
                                                   )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:ProteinDetectionHypothesisParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:ProteinDetectionHypothesisParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a proteinDetectionHypothesisParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.ProteinDetectionHypothesisParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionHypothesisParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.ProteinDetectionHypothesisParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionHypothesisParam do
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
            static member private hasEqualFieldValues (item1:ProteinDetectionHypothesisParam) (item2:ProteinDetectionHypothesisParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ProteinDetectionHypothesisParam) =
                    ProteinDetectionHypothesisParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinDetectionHypothesisParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:ProteinDetectionHypothesisParam) =
                ProteinDetectionHypothesisParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type ProteinAmbiguityGroupParamHandler =
            ///Initializes a proteinambiguitygroupparam-object with at least all necessary parameters.
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

                new  ProteinAmbiguityGroupParam(
                                                id', 
                                                value',
                                                null,
                                                fkTerm,
                                                null,
                                                fkUnit', 
                                                Nullable(DateTime.Now)
                                               )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:ProteinAmbiguityGroupParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:ProteinAmbiguityGroupParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a proteinAmbiguityGroupParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.ProteinAmbiguityGroupParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinAmbiguityGroupParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.ProteinAmbiguityGroupParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinAmbiguityGroupParam do
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
            static member private hasEqualFieldValues (item1:ProteinAmbiguityGroupParam) (item2:ProteinAmbiguityGroupParam) =
                 item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ProteinAmbiguityGroupParam) =
                    ProteinAmbiguityGroupParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinAmbiguityGroupParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:ProteinAmbiguityGroupParam) =
                ProteinAmbiguityGroupParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type ProteinDetectionListParamHandler =
            ///Initializes a proteindetectionlistparam-object with at least all necessary parameters.
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

                new ProteinDetectionListParam(
                                              id', 
                                              value',
                                              null,
                                              fkTerm,
                                              null,
                                              fkUnit', 
                                              Nullable(DateTime.Now)
                                             )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:ProteinDetectionListParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addUnit
                (fkUnit:string) (table:ProteinDetectionListParam) =
                table.UnitID <- fkUnit
                table

            ///Tries to find a proteinDetectionListParam-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.ProteinDetectionListParam.Local do
                           if i.ID=id
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param, _ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionListParam do
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
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.ProteinDetectionListParam.Local do
                           if i.Term.Name=name
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionListParam do
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
            static member private hasEqualFieldValues (item1:ProteinDetectionListParam) (item2:ProteinDetectionListParam) =
                item1.Value=item2.Value && item1.Unit=item2.Unit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ProteinDetectionListParam) =
                    ProteinDetectionListParamHandler.tryFindByTermName dbContext item.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinDetectionListParamHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:ProteinDetectionListParam) =
                ProteinDetectionListParamHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type OrganizationHandler =
            ///Initializes a organization-object with at least all necessary parameters.
            static member init
                (
                    ?id                  : string,
                    ?name                : string,
                    ?parent              : string,
                    ?details             : seq<OrganizationParam>,
                    ?fkMzIdentMLDocument : string
                ) =
                let id'                = defaultArg id (System.Guid.NewGuid().ToString())
                let name'              = defaultArg name Unchecked.defaultof<string>
                let parent'            = defaultArg parent Unchecked.defaultof<string>
                let details'           = convertOptionToList details
                let fkMzIdentMLDocument' = defaultArg fkMzIdentMLDocument Unchecked.defaultof<string>
                    
                new Organization(
                                 id', 
                                 name', 
                                 parent',
                                 details',
                                 fkMzIdentMLDocument',
                                 Nullable(DateTime.Now)
                                )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (organization:Organization) =
                organization.Name <- name
                organization

            ///Replaces parent of existing object with new one.
            static member addParent
                (parent:string) (organization:Organization) =
                organization.Parent <- parent
                organization

            ///Adds a organizationparam to an existing organization-object.
            static member addDetail
                (detail:OrganizationParam) (organization:Organization) =
                let result = organization.Details <- addToList organization.Details detail
                organization

            ///Adds a collection of organizationparams to an existing organization-object.
            static member addDetails
                (details:seq<OrganizationParam>) (organization:Organization) =
                let result = organization.Details <- addCollectionToList organization.Details details
                organization

            ///Replaces fkMzIdentML of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentMLDocument:string) (table:Organization)=
                let result = table.MzIdentMLDocumentID <- fkMzIdentMLDocument
                table

            ///Tries to find a organization-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.Organization.Local do
                           if i.ID=id
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (organization, _ ) -> organization)
                |> (fun organization -> 
                    if (Seq.exists (fun organization' -> organization' <> null) organization) = false
                        then 
                            query {
                                   for i in dbContext.Organization do
                                       if i.ID=id
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (organization, _ ) -> organization)
                            |> (fun organization -> if (Seq.exists (fun organization' -> organization' <> null) organization) = false
                                                    then None
                                                    else Some (organization.Single())
                               )
                        else Some (organization.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
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
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) && item1.Parent=item2.Parent &&
                item1.MzIdentMLDocumentID=item2.MzIdentMLDocumentID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Organization) =
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Organization) =
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
                    ?organizations       : seq<Organization>,
                    ?contactDetails      : seq<PersonParam>,
                    ?fkMzIdentMLDocument : string
                ) =
                let id'                  = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                = defaultArg name Unchecked.defaultof<string>
                let firstName'           = defaultArg firstName Unchecked.defaultof<string>
                let midInitials'         = defaultArg midInitials Unchecked.defaultof<string>
                let lastName'            = defaultArg lastName Unchecked.defaultof<string>    
                let organizations'       = convertOptionToList organizations
                let contactDetails'      = convertOptionToList contactDetails
                let fkMzIdentMLDocument' = defaultArg fkMzIdentMLDocument Unchecked.defaultof<string>

                    
                new Person(
                           id', 
                           name', 
                           firstName',  
                           midInitials', 
                           lastName', 
                           organizations',
                           contactDetails',
                           fkMzIdentMLDocument',
                           Nullable(DateTime.Now)
                          )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (person:Person) =
                person.Name <- name
                person

            ///Replaces firstname of existing object with new one.
            static member addFirstName
                (firstName:string) (person:Person) =
                person.FirstName <- firstName
                person

            ///Replaces midinitials of existing object with new one.
            static member addMidInitials
                (midInitials:string) (person:Person) =
                person.MidInitials <- midInitials
                person

            ///Replaces lastname of existing object with new one.
            static member addLastName
                (lastName:string) (person:Person) =
                person.LastName <- lastName
                person

            ///Adds a personparam to an existing person-object.
            static member addDetail (detail:PersonParam) (person:Person) =
                let result = person.Details <- addToList person.Details detail
                person

            ///Adds a collection of personparams to an existing person-object.
            static member addDetails
                (details:seq<PersonParam>) (person:Person) =
                let result = person.Details <- addCollectionToList person.Details details
                person

            ///Adds a organization to an existing person-object.
            static member addOrganization
                (organization:Organization) (person:Person) =
                let result = person.Organizations <- addToList person.Organizations organization
                person

            ///Adds a collection of organizations to an existing person-object.
            static member addOrganizations
                (organizations:seq<Organization>) (person:Person) =
                let result = person.Organizations <- addCollectionToList person.Organizations organizations
                person

            ///Replaces fkMzIdentMLDocument of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentMLDocument:string) (table:Person)=
                let result = table.MzIdentMLDocumentID <- fkMzIdentMLDocument
                table

            ///Tries to find a person-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.Person.Local do
                           if i.ID=id
                              then select (i, i.Details, i.Organizations, i.MzIdentMLDocumentID)
                      }
                |> Seq.map (fun (person, _, _, _) -> person)
                |> (fun person -> 
                    if (Seq.exists (fun person' -> person' <> null) person) = false
                        then 
                            query {
                                   for i in dbContext.Person do
                                       if i.ID=id
                                          then select (i, i.Details, i.Organizations, i.MzIdentMLDocumentID)
                                  }
                            |> Seq.map (fun (person, _, _, _) -> person)
                            |> (fun person -> if (Seq.exists (fun person' -> person' <> null) person) = false
                                                then None
                                                else Some (person.Single())
                               )
                        else Some (person.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.Person.Local do
                           if i.Name = name
                              then select (i, i.Details, i.Organizations, i.MzIdentMLDocumentID)
                      }
                |> Seq.map (fun (person, _, _, _) -> person)
                |> (fun person -> 
                    if (Seq.exists (fun person' -> person' <> null) person) = false
                        then 
                            query {
                                   for i in dbContext.Person do
                                       if i.Name = name
                                          then select (i, i.Details, i.Organizations, i.MzIdentMLDocumentID)
                                  }
                            |> Seq.map (fun (person, _, _, _) -> person)
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
                item1.Organizations=item2.Organizations && item1.Organizations=item2.Organizations &&
                item1.MzIdentMLDocumentID=item2.MzIdentMLDocumentID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Person) =
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Person) =
                PersonHandler.addToContext dbContext item |> ignore |> ignore
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

            ///Tries to find a contactRole-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
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

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByPersonName (dbContext:MzIdentML) (name:string) =
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
            static member addToContext (dbContext:MzIdentML) (item:ContactRole) =
                    ContactRoleHandler.tryFindByPersonName dbContext item.Person.Name
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:ContactRole) =
                ContactRoleHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type AnalysisSoftwareHandler =
            ///Initializes a analysissoftware-object with at least all necessary parameters.
            static member init
                (
                    softwareName         : CVParam,
                    ?id                  : string,
                    ?name                : string,
                    ?uri                 : string,
                    ?version             : string,
                    ?customizations      : string,
                    ?softwareDeveloper   : ContactRole,
                    ?fkMzIdentMLDocument : string
                ) =
                let id'                  = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                = defaultArg name Unchecked.defaultof<string>
                let uri'                 = defaultArg uri Unchecked.defaultof<string>
                let version'             = defaultArg version Unchecked.defaultof<string>
                let customizations'      = defaultArg customizations Unchecked.defaultof<string>
                let contactRole'         = defaultArg softwareDeveloper Unchecked.defaultof<ContactRole>
                let fkMzIdentMLDocument' = defaultArg fkMzIdentMLDocument Unchecked.defaultof<string>
                    
                new AnalysisSoftware(
                                     id', 
                                     name', 
                                     uri', 
                                     version', 
                                     customizations', 
                                     contactRole', 
                                     softwareName, 
                                     fkMzIdentMLDocument', 
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (analysisSoftware:AnalysisSoftware) =
                analysisSoftware.Name <- name
                analysisSoftware

            ///Replaces uri of existing object with new uri.
            static member addURI
                (uri:string) (analysisSoftware:AnalysisSoftware) =
                analysisSoftware.URI <- uri
                analysisSoftware

            ///Replaces version of existing object with new version.
            static member addVersion
                (version:string) (analysisSoftware:AnalysisSoftware) =
                analysisSoftware.Version <- version
                analysisSoftware

            ///Replaces customization of existing object with new customization.
            static member addCustomization
                (customizations:string) (analysisSoftware:AnalysisSoftware) =
                analysisSoftware.Customizations <- customizations
                analysisSoftware

            ///Replaces contactrole of existing object with new contactrole.
            static member addAnalysisSoftwareDeveloper
                (analysisSoftwareDeveloper:ContactRole) (analysisSoftware:AnalysisSoftware)=
                analysisSoftware.ContactRole <- analysisSoftwareDeveloper
                analysisSoftware

            ///Replaces fkMzIdentMLDocument of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentMLDocument:string) (analysisSoftware:AnalysisSoftware)=
                let result = analysisSoftware.MzIdentMLDocumentID <- fkMzIdentMLDocument
                analysisSoftware

            ///Tries to find a analysisSoftware-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.AnalysisSoftware.Local do
                           if i.ID=id
                              then select (i, i.SoftwareName, i.ContactRole, i.MzIdentMLDocumentID)
                      }
                |> Seq.map (fun (analysisSoftware, _, _, _) -> analysisSoftware)
                |> (fun analysisSoftware -> 
                    if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisSoftware do
                                       if i.ID=id
                                          then select (i, i.SoftwareName, i.ContactRole, i.MzIdentMLDocumentID)
                                  }
                            |> Seq.map (fun (analysisSoftware, _, _, _) -> analysisSoftware)
                            |> (fun analysisSoftware -> if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
                                                        then None
                                                        else Some (analysisSoftware.Single())
                               )
                        else Some (analysisSoftware.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindBySoftwareNameValue (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.AnalysisSoftware.Local do
                           if i.SoftwareName.Value = name
                              then select (i, i.SoftwareName, i.ContactRole, i.MzIdentMLDocumentID)
                      }
                |> Seq.map (fun (analysisSoftware, _, _, _) -> analysisSoftware)
                |> (fun analysisSoftware -> 
                    if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisSoftware do
                                       if i.SoftwareName.Value = name
                                          then select (i, i.SoftwareName, i.ContactRole, i.MzIdentMLDocumentID)
                                  }
                            |> Seq.map (fun (analysisSoftware, _, _, _) -> analysisSoftware)
                            |> (fun analysisSoftware -> if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
                                                            then None
                                                            else Some analysisSoftware
                               )
                        else Some analysisSoftware
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:AnalysisSoftware) (item2:AnalysisSoftware) =
                item1.Name=item2.Name && item1.URI=item2.URI && item1.Version=item2.Version && 
                item1.Customizations=item2.Customizations && item1.ContactRole=item2.ContactRole && 
                item1.MzIdentMLDocumentID=item2.MzIdentMLDocumentID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:AnalysisSoftware) =
                    AnalysisSoftwareHandler.tryFindBySoftwareNameValue dbContext item.SoftwareName.Value
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match AnalysisSoftwareHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:AnalysisSoftware) =
                AnalysisSoftwareHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SubSampleHandler =
            ///Initializes a subsample-object with at least all necessary parameters.
            static member init
                (
                    ?id          : string,
                    ?fkSample    : string
                ) =
                let id'          = defaultArg id (System.Guid.NewGuid().ToString())
                let fkSample'    = defaultArg fkSample Unchecked.defaultof<string>
                    
                new SubSample(
                              id', 
                              fkSample', 
                              Nullable(DateTime.Now)
                             )

            ///Replaces SampleID of existing object with new one.
            static member addSample
                (fkSample:string) (subSample:SubSample) =
                subSample.SampleID <- fkSample
                subSample

            ///Tries to find a subSample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SubSample.Local do
                           if i.ID=id
                              then select (i, i.SampleID)
                      }
                |> Seq.map (fun (subSample, _) -> subSample)
                |> (fun subSample -> 
                    if (Seq.exists (fun subSample' -> subSample' <> null) subSample) = false
                        then 
                            query {
                                   for i in dbContext.SubSample do
                                       if i.ID=id
                                          then select (i, i.SampleID)
                                  }
                            |> Seq.map (fun (subSample, _) -> subSample)
                            |> (fun subSample -> if (Seq.exists (fun subSample' -> subSample' <> null) subSample) = false
                                                    then None
                                                    else Some (subSample.Single())
                               )
                        else Some (subSample.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindBySampleName (dbContext:MzIdentML) (sampleID:string) =
                query {
                       for i in dbContext.SubSample.Local do
                           if i.SampleID=sampleID
                              then select (i, i.SampleID)
                      }
                |> Seq.map (fun (subSample, _) -> subSample)
                |> (fun subSample -> 
                    if (Seq.exists (fun subSample' -> subSample' <> null) subSample) = false
                        then 
                            query {
                                   for i in dbContext.SubSample do
                                       if i.SampleID=sampleID
                                          then select (i, i.SampleID)
                                  }
                            |> Seq.map (fun (subSample, _) -> subSample)
                            |> (fun subSample -> if (Seq.exists (fun subSample' -> subSample' <> null) subSample) = false
                                                            then None
                                                            else Some subSample
                               )
                        else Some subSample
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SubSample) (item2:SubSample) =
                item1.ID = item2.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SubSample) =
                    SubSampleHandler.tryFindBySampleName dbContext item.SampleID
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SubSampleHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SubSample) =
                SubSampleHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SampleHandler =
            ///Initializes a sample-object with at least all necessary parameters.
            static member init
                (
                    ?id           : string,
                    ?name         : string,
                    ?contactRoles : seq<ContactRole>,
                    ?subSamples   : seq<SubSample>,
                    ?details      : seq<SampleParam>,
                    ?fkMzIdentML  : string
                ) =
                let id'           = defaultArg id (System.Guid.NewGuid().ToString())
                let name'         = defaultArg name Unchecked.defaultof<string>
                let contactRoles' = convertOptionToList contactRoles
                let subSamples'   = convertOptionToList subSamples
                let details'      = convertOptionToList details
                let fkMzIdentML'  = defaultArg fkMzIdentML Unchecked.defaultof<string>
                    
                new Sample(
                           id', 
                           name', 
                           contactRoles', 
                           subSamples', 
                           details', 
                           fkMzIdentML', 
                           Nullable(DateTime.Now)
                          )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (sample:Sample) =
                sample.Name <- name
                sample

            ///Adds a contactrole to an existing sample-object.
            static member addContactRole
                (contactRole:ContactRole) (sample:Sample) =
                let result = sample.ContactRoles <- addToList sample.ContactRoles contactRole
                sample

            ///Adds a collection of contactroles to an existing sample-object.
            static member addContactRoles
                (contactRoles:seq<ContactRole>) (sample:Sample) =
                let result = sample.ContactRoles <- addCollectionToList sample.ContactRoles contactRoles
                sample

            ///Adds a subsample to an existing sample-object.
            static member addSubSample
                (subSample:SubSample) (sample:Sample) =
                let result = sample.SubSamples <- addToList sample.SubSamples subSample
                sample

            ///Adds a collection of subsamples to an existing sample-object.
            static member addSubSamples
                (subSamples:seq<SubSample>) (sample:Sample) =
                let result = sample.SubSamples <- addCollectionToList sample.SubSamples subSamples
                sample

            ///Adds a sampleparam to an existing sample-object.
            static member addDetail
                (detail:SampleParam) (sample:Sample) =
                let result = sample.Details <- addToList sample.Details detail
                sample

            ///Adds a collection of sampleparams to an existing sample-object.
            static member addDetails
                (details:seq<SampleParam>) (sample:Sample) =
                let result = sample.Details <- addCollectionToList sample.Details details
                sample

            ///Replaces MzIdentMLDocumentID of existing object with new mzIdentML.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (sample:Sample) =
                let result = sample.MzIdentMLDocumentID <- fkMzIdentML
                sample

            ///Tries to find a sample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.Sample.Local do
                           if i.ID=id
                              then select (i, i.ContactRoles, i.SubSamples, i.Details)
                      }
                |> Seq.map (fun (sample, _, _, _) -> sample)
                |> (fun sample -> 
                    if (Seq.exists (fun sample' -> sample' <> null) sample) = false
                        then 
                            query {
                                   for i in dbContext.Sample do
                                       if i.ID=id
                                          then select (i, i.ContactRoles, i.SubSamples, i.Details)
                                  }
                            |> Seq.map (fun (sample, _, _, _) -> sample)
                            |> (fun sample -> if (Seq.exists (fun sample' -> sample' <> null) sample) = false
                                                then None
                                                else Some (sample.Single())
                               )
                        else Some (sample.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.Sample.Local do
                           if i.Name=name
                              then select (i, i.ContactRoles, i.SubSamples, i.Details)
                      }
                |> Seq.map (fun (sample, _, _, _) -> sample)
                |> (fun subSample -> 
                    if (Seq.exists (fun subSample' -> subSample' <> null) subSample) = false
                        then 
                            query {
                                   for i in dbContext.Sample do
                                       if i.Name=name
                                          then select (i, i.ContactRoles, i.SubSamples, i.Details)
                                  }
                            |> Seq.map (fun (sample, _, _, _) -> sample)
                            |> (fun subSample -> if (Seq.exists (fun subSample' -> subSample' <> null) subSample) = false
                                                            then None
                                                            else Some subSample
                               )
                        else Some subSample
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Sample) (item2:Sample) =
                item1.ContactRoles=item2.ContactRoles && item1.SubSamples=item2.SubSamples &&
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) && item1.MzIdentMLDocumentID=item2.MzIdentMLDocumentID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Sample) =
                    SampleHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SampleHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Sample) =
                SampleHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ModificationHandler =
            ///Initializes a modification-object with at least all necessary parameters.
            static member init
                (
                    details                : seq<ModificationParam>,
                    ?id                    : string,
                    ?residues              : string,
                    ?location              : int,
                    ?monoIsotopicMassDelta : float,
                    ?avgMassDelta          : float
                ) =
                let id'                    = defaultArg id (System.Guid.NewGuid().ToString())
                let residues'              = defaultArg residues Unchecked.defaultof<string>
                let location'              = defaultArg location Unchecked.defaultof<int>
                let monoIsotopicMassDelta' = defaultArg monoIsotopicMassDelta Unchecked.defaultof<float>
                let avgMassDelta'          = defaultArg avgMassDelta Unchecked.defaultof<float>
                    
                new Modification(
                                 id', 
                                 residues', 
                                 Nullable(location'), 
                                 Nullable(monoIsotopicMassDelta'), 
                                 Nullable(avgMassDelta'), 
                                 details |> List, Nullable(DateTime.Now)
                                )

            ///Replaces residues of existing object with new residues.
            static member addResidues
                (residues:string) (modification:Modification) =
                modification.Residues <- residues
                modification

            ///Replaces location of existing object with new location.
            static member addLocation
                (location:int) (modification:Modification) =
                modification.Location <- Nullable(location)
                modification

            ///Replaces monoisotopicmassdelta of existing object with new monoisotopicmassdelta.
            static member addMonoIsotopicMassDelta
                (monoIsotopicMassDelta:float) (modification:Modification) =
                modification.MonoIsotopicMassDelta <- Nullable(monoIsotopicMassDelta)
                modification

            ///Replaces avgmassdelta of existing object with new avgmassdelta.
            static member addAvgMassDelta
                (avgMassDelta:float) (modification:Modification) =
                modification.AvgMassDelta <- Nullable(avgMassDelta)
                modification

            ///Tries to find a modification-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.Modification.Local do
                           if i.ID=id
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (modification, _) -> modification)
                |> (fun modification -> 
                    if (Seq.exists (fun modification' -> modification' <> null) modification) = false
                        then 
                            query {
                                   for i in dbContext.Modification do
                                       if i.ID=id
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (modification, _) -> modification)
                            |> (fun modification -> if (Seq.exists (fun modification' -> modification' <> null) modification) = false
                                                    then None
                                                    else Some (modification.Single())
                               )
                        else Some (modification.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByMonoIsotopicMassDelta (dbContext:MzIdentML) (monoIsotopicMassDelta:Nullable<float>) =
                query {
                       for i in dbContext.Modification.Local do
                           if i.MonoIsotopicMassDelta=monoIsotopicMassDelta
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (modification, _) -> modification)
                |> (fun modification -> 
                    if (Seq.exists (fun modification' -> modification' <> null) modification) = false
                        then 
                            query {
                                   for i in dbContext.Modification do
                                       if i.MonoIsotopicMassDelta=monoIsotopicMassDelta
                                          then select (i, i.Details)
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
                item1.Residues=item2.Residues && item1.Location=item2.Location &&
                item1.AvgMassDelta=item2.AvgMassDelta && matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Modification) =
                    ModificationHandler.tryFindByMonoIsotopicMassDelta dbContext item.MonoIsotopicMassDelta
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Modification) =
                ModificationHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SubstitutionModificationHandler =
            ///Initializes a substitutionmodification-object with at least all necessary parameters.
            static member init
                (
                    originalResidue        : string,
                    replacementResidue     : string,
                    ?id                    : string,
                    ?location              : int,
                    ?monoIsotopicMassDelta : float,
                    ?avgMassDelta          : float
                ) =
                let id'                    = defaultArg id (System.Guid.NewGuid().ToString())
                let location'              = defaultArg location Unchecked.defaultof<int>
                let monoIsotopicMassDelta' = defaultArg monoIsotopicMassDelta Unchecked.defaultof<float>
                let avgMassDelta'          = defaultArg avgMassDelta Unchecked.defaultof<float>

                new SubstitutionModification(
                                             id', 
                                             originalResidue, 
                                             replacementResidue, 
                                             Nullable(location'), 
                                             Nullable(monoIsotopicMassDelta'), 
                                             Nullable(avgMassDelta'), 
                                             Nullable(DateTime.Now)
                                            )

            ///Replaces location of existing object with new location.
            static member addLocation
                (location:int) (substitutionModification:SubstitutionModification) =
                substitutionModification.Location <- Nullable(location)
                substitutionModification

            ///Replaces monoisotopicmassdelta of existing object with new monoisotopicmassdelta.
            static member addMonoIsotopicMassDelta
                (monoIsotopicMassDelta:float) (substitutionModification:SubstitutionModification) =
                substitutionModification.MonoIsotopicMassDelta <- Nullable(monoIsotopicMassDelta)
                substitutionModification

            ///Replaces avgmassdelta of existing object with new avgmassdelta.
            static member addAvgMassDelta
                (avgMassDelta:float) (substitutionModification:SubstitutionModification) =
                substitutionModification.AvgMassDelta <- Nullable(avgMassDelta)
                substitutionModification

            ///Tries to find a substitutionModification-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SubstitutionModification.Local do
                           if i.ID=id
                              then select i
                      }
                |> (fun substitutionModification -> 
                    if (Seq.exists (fun substitutionModification' -> substitutionModification' <> null) substitutionModification) = false
                        then 
                            query {
                                   for i in dbContext.SubstitutionModification do
                                       if i.ID=id
                                          then select i
                                  }
                            |> (fun substitutionModification -> if (Seq.exists (fun substitutionModification' -> substitutionModification' <> null) substitutionModification) = false
                                                                then None
                                                                else Some (substitutionModification.Single())
                               )
                        else Some (substitutionModification.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByMonoIsotopicMassDelta (dbContext:MzIdentML) (monoIsotopicMassDelta:Nullable<float>) =
                query {
                       for i in dbContext.SubstitutionModification.Local do
                           if i.MonoIsotopicMassDelta=monoIsotopicMassDelta
                              then select i
                      }
                |> (fun substitutionModification -> 
                    if (Seq.exists (fun substitutionModification' -> substitutionModification' <> null) substitutionModification) = false
                        then 
                            query {
                                   for i in dbContext.SubstitutionModification do
                                       if i.MonoIsotopicMassDelta=monoIsotopicMassDelta
                                          then select i
                                  }
                            |> (fun substitutionModification -> if (Seq.exists (fun substitutionModification' -> substitutionModification' <> null) substitutionModification) = false
                                                                then None
                                                                else Some (seq(substitutionModification))
                               )
                        else Some substitutionModification
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SubstitutionModification) (item2:SubstitutionModification) =
                item1.OriginalResidue=item2.OriginalResidue && item1.ReplacementResidue=item2.ReplacementResidue &&
                item1.AvgMassDelta=item2.AvgMassDelta && item1.Location=item2.Location 

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SubstitutionModification) =
                    SubstitutionModificationHandler.tryFindByMonoIsotopicMassDelta dbContext item.MonoIsotopicMassDelta
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SubstitutionModificationHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Modification) =
                ModificationHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type PeptideHandler =
            ///Initializes a peptide-object with at least all necessary parameters.
            static member init
                (
                    peptideSequence            : string,
                    ?id                        : string,
                    ?name                      : string,                    
                    ?modifications             : seq<Modification>,
                    ?substitutionModifications : seq<SubstitutionModification>,
                    ?details                   : seq<PeptideParam>,
                    ?fkMzIdentML               : string
                ) =
                let id'                        = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                      = defaultArg name Unchecked.defaultof<string>
                let modifications'             = convertOptionToList modifications
                let substitutionModifications' = convertOptionToList substitutionModifications
                let details'                   = convertOptionToList details
                let fkMzIdentML'               = defaultArg fkMzIdentML Unchecked.defaultof<string>

                new Peptide(
                            id', 
                            name', 
                            peptideSequence, 
                            modifications', 
                            substitutionModifications', 
                            details', 
                            fkMzIdentML', 
                            Nullable(DateTime.Now)
                           )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (peptide:Peptide) =
                peptide.Name <- name
                peptide

            ///Adds a modification to an existing peptide-object.
            static member addModification
                (modification:Modification) (peptide:Peptide) =
                let result = peptide.Modifications <- addToList peptide.Modifications modification
                peptide

            ///Adds a collection of modifications to an existing peptide-object.
            static member addModifications
                (modifications:seq<Modification>) (peptide:Peptide) =
                let result = peptide.Modifications <- addCollectionToList peptide.Modifications modifications
                peptide

            ///Adds a substitutionmodification to an existing peptide-object.
            static member addSubstitutionModification
                (substitutionModification:SubstitutionModification) (peptide:Peptide) =
                let result = peptide.SubstitutionModifications <- addToList peptide.SubstitutionModifications substitutionModification
                peptide

            ///Adds a collection of substitutionmodifications to an existing peptide-object.
            static member addSubstitutionModifications
                (substitutionModifications:seq<SubstitutionModification>) (peptide:Peptide) =
                let result = peptide.SubstitutionModifications <- addCollectionToList peptide.SubstitutionModifications substitutionModifications
                peptide

            ///Adds a peptideparam to an existing peptide-object.
            static member addDetail
                (detail:PeptideParam) (peptide:Peptide) =
                let result = peptide.Details <- addToList peptide.Details detail
                peptide

            ///Adds a collection of peptideparams to an existing peptide-object.
            static member addDetails
                (details:seq<PeptideParam>) (peptide:Peptide) =
                let result = peptide.Details <- addCollectionToList peptide.Details details
                peptide

            ///Replaces MzIdentMLDocumentID of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (peptide:Peptide) =
                let result = peptide.MzIdentMLDocumentID <- fkMzIdentML
                peptide

            ///Tries to find a peptide-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.Peptide.Local do
                           if i.ID=id
                              then select (i, i.Modifications, i.SubstitutionModifications, i.Details)
                      }
                |> Seq.map (fun (peptide, _, _, _) -> peptide)
                |> (fun peptide -> 
                    if (Seq.exists (fun peptide' -> peptide' <> null) peptide) = false
                        then 
                            query {
                                   for i in dbContext.Peptide do
                                       if i.ID=id
                                          then select (i, i.Modifications, i.SubstitutionModifications, i.Details)
                                  }
                            |> Seq.map (fun (peptide, _, _, _) -> peptide)
                            |> (fun peptide -> if (Seq.exists (fun peptide' -> peptide' <> null) peptide) = false
                                                then None
                                                else Some (peptide.Single())
                               )
                        else Some (peptide.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByPeptideSequence (dbContext:MzIdentML) (peptideSeq:string) =
                query {
                       for i in dbContext.Peptide.Local do
                           if i.PeptideSequence=peptideSeq
                              then select (i, i.Modifications, i.SubstitutionModifications, i.Details)
                      }
                |> Seq.map (fun (peptide, _, _, _) -> peptide)
                |> (fun peptide -> 
                    if (Seq.exists (fun peptide' -> peptide' <> null) peptide) = false
                        then 
                            query {
                                   for i in dbContext.Peptide do
                                       if i.PeptideSequence=peptideSeq
                                          then select (i, i.Modifications, i.SubstitutionModifications, i.Details)
                                  }
                            |> Seq.map (fun (peptide, _, _, _) -> peptide)
                            |> (fun peptide -> if (Seq.exists (fun peptide' -> peptide' <> null) peptide) = false
                                               then None
                                               else Some peptide
                               )
                        else Some peptide
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Peptide) (item2:Peptide) =
                item1.Name=item2.Name && item1.Modifications=item2.Modifications &&
                item1.MzIdentMLDocumentID=item2.MzIdentMLDocumentID && matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) &&
                item1.SubstitutionModifications=item2.SubstitutionModifications

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Peptide) =
                    PeptideHandler.tryFindByPeptideSequence dbContext item.PeptideSequence
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PeptideHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Peptide) =
                PeptideHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type TranslationTableHandler =
            ///Initializes a translationtable-object with at least all necessary parameters.
            static member init
                (
                    ?id      : string,
                    ?name    : string,
                    ?details : seq<TranslationTableParam>
                ) =
                let id'      = defaultArg id (System.Guid.NewGuid().ToString())
                let name'    = defaultArg name Unchecked.defaultof<string>
                let details' = convertOptionToList details
                    
                new TranslationTable(
                                     id', 
                                     name', 
                                     details', 
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (translationTable:TranslationTable) =
                translationTable.Name <- name
                translationTable

            ///Adds a translationtableparam to an existing translationtable-object.
            static member addDetail
                (detail:TranslationTableParam) (translationTable:TranslationTable) =
                let result = translationTable.Details <- addToList translationTable.Details detail
                translationTable

            ///Adds a collection of translationtableparams to an existing translationtable-object.
            static member addDetails
                (details:seq<TranslationTableParam>) (translationTable:TranslationTable) =
                let result = translationTable.Details <- addCollectionToList translationTable.Details details
                translationTable

            ///Tries to find a translationTable-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.TranslationTable.Local do
                           if i.ID=id
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (translationTable, _) -> translationTable)
                |> (fun translationTable -> 
                    if (Seq.exists (fun translationTable' -> translationTable' <> null) translationTable) = false
                        then 
                            query {
                                   for i in dbContext.TranslationTable do
                                       if i.ID=id
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (translationTable, _) -> translationTable)
                            |> (fun translationTable -> if (Seq.exists (fun translationTable' -> translationTable' <> null) translationTable) = false
                                                        then None
                                                        else Some (translationTable.Single())
                               )
                        else Some (translationTable.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.TranslationTable.Local do
                           if i.Name=name
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (translationTable, _) -> translationTable)
                |> (fun translationTable -> 
                    if (Seq.exists (fun translationTable' -> translationTable' <> null) translationTable) = false
                        then 
                            query {
                                   for i in dbContext.TranslationTable do
                                       if i.Name=name
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (translationTable, _) -> translationTable)
                            |> (fun translationTable -> if (Seq.exists (fun translationTable' -> translationTable' <> null) translationTable) = false
                                                        then None
                                                        else Some translationTable
                               )
                        else Some translationTable
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:TranslationTable) (item2:TranslationTable) =
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:TranslationTable) =
                    TranslationTableHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match TranslationTableHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:TranslationTable) =
                TranslationTableHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()

        type MeasureHandler =
            ///Initializes a measure-object with at least all necessary parameters.
            static member init
                (
                    details  : seq<MeasureParam>,
                    ?id      : string,
                    ?name    : string 
                ) =
                let id'   = defaultArg id (System.Guid.NewGuid().ToString())
                let name' = defaultArg name Unchecked.defaultof<string>
                    
                new Measure(
                            id', 
                            name', 
                            details |> List, Nullable(DateTime.Now)
                           )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (measure:Measure) =
                measure.Name <- name
                measure

            ///Tries to find a measure-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.Measure.Local do
                           if i.ID=id
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (measure, _) -> measure)
                |> (fun measure -> 
                    if (Seq.exists (fun measure' -> measure' <> null) measure) = false
                        then 
                            query {
                                   for i in dbContext.Measure do
                                       if i.ID=id
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (measure, _) -> measure)
                            |> (fun measure -> if (Seq.exists (fun measure' -> measure' <> null) measure) = false
                                                then None
                                                else Some (measure.Single())
                               )
                        else Some (measure.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.Measure.Local do
                           if i.Name=name
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (measure, _) -> measure)
                |> (fun measure -> 
                    if (Seq.exists (fun measure' -> measure' <> null) measure) = false
                        then 
                            query {
                                   for i in dbContext.Measure.Local do
                                       if i.Name=name
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (measure, _) -> measure)
                            |> (fun measure -> if (Seq.exists (fun measure' -> measure' <> null) measure) = false
                                                   then None
                                                   else Some measure
                               )
                        else Some measure
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member hasEqualFieldValues (item1:Measure) (item2:Measure) =
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Measure) =
                    MeasureHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match MeasureHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Measure) =
                MeasureHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ResidueHandler =
            ///Initializes a residue-object with at least all necessary parameters.
            static member init
                (
                    code    : string,
                    mass    : float,
                    ?id     : string
                ) =
                let id' = defaultArg id (System.Guid.NewGuid().ToString())
                new Residue(
                            id', 
                            code, 
                            Nullable(mass), 
                            Nullable(DateTime.Now)
                           )

            ///Tries to find a residue-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.Residue.Local do
                           if i.ID=id
                              then select i
                      }        
                |> (fun residue -> 
                    if (Seq.exists (fun residue' -> residue' <> null) residue) = false
                        then 
                            query {
                                   for i in dbContext.Residue do
                                       if i.ID=id
                                          then select i
                                  }
                            |> (fun residue -> if (Seq.exists (fun residue' -> residue' <> null) residue) = false
                                                then None
                                                else Some (residue.Single())
                               )
                        else Some (residue.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByCode (dbContext:MzIdentML) (code:string) =
                query {
                       for i in dbContext.Residue.Local do
                           if i.Code=code
                              then select i
                      }
                |> (fun residue -> 
                    if (Seq.exists (fun residue' -> residue' <> null) residue) = false
                        then 
                            query {
                                   for i in dbContext.Residue do
                                       if i.Code=code
                                          then select i
                                  }
                            |> Seq.map (fun (residue) -> residue)
                            |> (fun residue -> if (Seq.exists (fun residue' -> residue' <> null) residue) = false
                                                   then None
                                                   else Some residue
                               )
                        else Some residue
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Residue) (item2:Residue) =
                item1.Mass=item2.Mass

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Residue) =
                    ResidueHandler.tryFindByCode dbContext item.Code
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ResidueHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Residue) =
                ResidueHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type AmbiguousResidueHandler =
            static member init
                (
                    code    : string,
                    details : seq<AmbiguousResidueParam>,
                    ?id     : string
                ) =
                let id' = defaultArg id (System.Guid.NewGuid().ToString())
                    
                new AmbiguousResidue(
                                     id', 
                                     code, 
                                     details |> List, 
                                     Nullable(DateTime.Now)
                                    )

            ///Tries to find a ambiguousResidue-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.AmbiguousResidue.Local do
                           if i.ID=id
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (ambiguousResidue, _) -> ambiguousResidue)
                |> (fun ambiguousResidue -> 
                    if (Seq.exists (fun ambiguousResidue' -> ambiguousResidue' <> null) ambiguousResidue) = false
                        then 
                            query {
                                   for i in dbContext.AmbiguousResidue do
                                       if i.ID=id
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (ambiguousResidue, _) -> ambiguousResidue)
                            |> (fun ambiguousResidue -> if (Seq.exists (fun ambiguousResidue' -> ambiguousResidue' <> null) ambiguousResidue) = false
                                                        then None
                                                        else Some (ambiguousResidue.Single())
                               )
                        else Some (ambiguousResidue.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByCode (dbContext:MzIdentML) (code:string) =
                query {
                       for i in dbContext.AmbiguousResidue.Local do
                           if i.Code=code
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (ambiguousResidue, _) -> ambiguousResidue)
                |> (fun ambiguousResidue -> 
                    if (Seq.exists (fun ambiguousResidue' -> ambiguousResidue' <> null) ambiguousResidue) = false
                        then 
                            query {
                                   for i in dbContext.AmbiguousResidue do
                                       if i.Code=code
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (ambiguousResidue, _) -> ambiguousResidue)
                            |> (fun ambiguousResidue -> if (Seq.exists (fun ambiguousResidue' -> ambiguousResidue' <> null) ambiguousResidue) = false
                                                            then None
                                                            else Some ambiguousResidue
                               )
                        else Some ambiguousResidue
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:AmbiguousResidue) (item2:AmbiguousResidue) =
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:AmbiguousResidue) =
                    AmbiguousResidueHandler.tryFindByCode dbContext item.Code
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match AmbiguousResidueHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:AmbiguousResidue) =
                AmbiguousResidueHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type MassTableHandler =
            ///Initializes a masstable-object with at least all necessary parameters.
            static member init
                (
                    msLevel           : string,
                    ?id               : string,
                    ?name             : string,
                    ?residue          : seq<Residue>,
                    ?ambiguousResidue : seq<AmbiguousResidue>,
                    ?details          : seq<MassTableParam>
                ) =
                let id'               = defaultArg id (System.Guid.NewGuid().ToString())
                let name'             = defaultArg name Unchecked.defaultof<string>
                let residue'          = convertOptionToList residue
                let ambiguousResidue' = convertOptionToList ambiguousResidue
                let details'          = convertOptionToList details
                    
                new MassTable(
                              id', 
                              name', 
                              msLevel, 
                              residue', 
                              ambiguousResidue', 
                              details', 
                              Nullable(DateTime.Now)
                             )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (massTable:MassTable) =
                massTable.Name <- name
                massTable

            ///Adds a residue to an existing masstable-object.
            static member addResidue
                (residue:Residue) (massTable:MassTable) =
                let result = massTable.Residues <- addToList massTable.Residues residue
                massTable

            ///Adds a collection of residues to an existing masstable-object.
            static member addResidues
                (residues:seq<Residue>) (massTable:MassTable) =
                let result = massTable.Residues <- addCollectionToList massTable.Residues residues
                massTable

            ///Adds a ambiguousresidue to an existing masstable-object.
            static member addAmbiguousResidue
                (ambiguousResidues:AmbiguousResidue) (massTable:MassTable) =
                let result = massTable.AmbiguousResidues <- addToList massTable.AmbiguousResidues ambiguousResidues
                massTable

            ///Adds a collection of ambiguousresidues to an existing masstable-object.
            static member addAmbiguousResidues
                (ambiguousResidues:seq<AmbiguousResidue>) (massTable:MassTable) =
                let result = massTable.AmbiguousResidues <- addCollectionToList massTable.AmbiguousResidues ambiguousResidues
                massTable

            ///Adds a masstableparam to an existing masstable-object.
            static member addDetail
                (detail:MassTableParam) (massTable:MassTable) =
                let result = massTable.Details <- addToList massTable.Details detail
                massTable

            ///Adds a collection of masstableparams to an existing masstable-object.
            static member addDetails
                (details:seq<MassTableParam>) (massTable:MassTable) =
                let result = massTable.Details <- addCollectionToList massTable.Details details
                massTable

            ///Tries to find a massTable-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.MassTable.Local do
                           if i.ID=id
                              then select (i, i.Residues, i.AmbiguousResidues, i.Details)
                      }
                |> Seq.map (fun (massTable, _, _, _) -> massTable)
                |> (fun massTable -> 
                    if (Seq.exists (fun massTable' -> massTable' <> null) massTable) = false
                        then 
                            query {
                                   for i in dbContext.MassTable do
                                       if i.ID=id
                                          then select (i, i.Residues, i.AmbiguousResidues, i.Details)
                                  }
                            |> Seq.map (fun (massTable, _, _, _) -> massTable)
                            |> (fun massTable -> if (Seq.exists (fun massTable' -> massTable' <> null) massTable) = false
                                                    then None
                                                    else Some (massTable.Single())
                               )
                        else Some (massTable.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.MassTable.Local do
                           if i.Name=name
                              then select (i, i.Residues, i.AmbiguousResidues, i.Details)
                      }
                |> Seq.map (fun (massTable, _, _, _) -> massTable)
                |> (fun massTable -> 
                    if (Seq.exists (fun massTable' -> massTable' <> null) massTable) = false
                        then 
                            query {
                                   for i in dbContext.MassTable do
                                       if i.Name=name
                                          then select (i, i.Residues, i.AmbiguousResidues, i.Details)
                                  }
                            |> Seq.map (fun (massTable, _, _, _) -> massTable)
                            |> (fun massTable -> if (Seq.exists (fun massTable' -> massTable' <> null) massTable) = false
                                                            then None
                                                            else Some massTable
                               )
                        else Some massTable
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:MassTable) (item2:MassTable) =
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:MassTable) =
                    MassTableHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match MassTableHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:MassTable) =
                MassTableHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ValueHandler =
            ///Initializes a value-object with at least all necessary parameters.
            static member init
                (
                    value   : float,
                    ?id     : string
                ) =
                let id' = defaultArg id (System.Guid.NewGuid().ToString())
                    
                new Value(
                          id', 
                          Nullable(value), 
                          Nullable(DateTime.Now)
                         )

            //Tries to find a value-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.Value.Local do
                           if i.ID=id
                              then select i
                      }
                |> (fun value -> 
                    if (Seq.exists (fun value' -> value' <> null) value) = false
                        then 
                            query {
                                   for i in dbContext.Value do
                                       if i.ID=id
                                          then select i
                                  }
                            |> (fun value -> if (Seq.exists (fun value' -> value' <> null) value) = false
                                                then None
                                                else Some (value.Single())
                               )
                        else Some (value.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByValue (dbContext:MzIdentML) (item:Nullable<float>) =
                query {
                       for i in dbContext.Value.Local do
                           if i.Value=item
                              then select i
                      }
                |> (fun value -> 
                    if (Seq.exists (fun value' -> value' <> null) value) = false
                        then 
                            query {
                                   for i in dbContext.Value do
                                       if i.Value=item
                                          then select i
                                  }
                            |> Seq.map (fun (value) -> value)
                            |> (fun value -> if (Seq.exists (fun value' -> value' <> null) value) = false
                                                            then None
                                                            else Some value
                               )
                        else Some value
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Value) (item2:Value) =
                item1.ID = item2.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Value) =
                    ValueHandler.tryFindByValue dbContext item.Value
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ValueHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Value) =
                ValueHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type FragmentArrayHandler =
            ///Initializes a fragmentarray-object with at least all necessary parameters.
            static member init
                (
                    measure  : Measure,
                    value    : float,
                    ?id      : string
                ) =
                let id' = defaultArg id (System.Guid.NewGuid().ToString())
                    
                new FragmentArray(
                                  id', 
                                  measure, 
                                  Nullable(value), 
                                  Nullable(DateTime.Now)
                                 )

            ///Tries to find a fragmentArray-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.FragmentArray.Local do
                           if i.ID=id
                              then select (i, i.Measure)
                      }
                |> Seq.map (fun (fragmentArray, _) -> fragmentArray)
                |> (fun fragmentArray -> 
                    if (Seq.exists (fun fragmentArray' -> fragmentArray' <> null) fragmentArray) = false
                        then 
                            query {
                                   for i in dbContext.FragmentArray do
                                       if i.ID=id
                                          then select (i, i.Measure)
                                  }
                            |> Seq.map (fun (fragmentArray, _) -> fragmentArray)
                            |> (fun fragmentArray -> if (Seq.exists (fun fragmentArray' -> fragmentArray' <> null) fragmentArray) = false
                                                        then None
                                                        else Some (fragmentArray.Single())
                               )
                        else Some (fragmentArray.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByMeasureName (dbContext:MzIdentML) (value:Nullable<float>) =
                query {
                       for i in dbContext.FragmentArray.Local do
                           if i.Values=value
                              then select (i, i.Measure)
                      }
                |> Seq.map (fun (fragmentArray, _) -> fragmentArray)
                |> (fun fragmentArray -> 
                    if (Seq.exists (fun fragmentArray' -> fragmentArray' <> null) fragmentArray) = false
                        then 
                            query {
                                   for i in dbContext.FragmentArray do
                                       if i.Values=value
                                          then select (i, i.Measure)
                                  }
                            |> Seq.map (fun (fragmentArray, _) -> fragmentArray)
                            |> (fun fragmentArray -> if (Seq.exists (fun fragmentArray' -> fragmentArray' <> null) fragmentArray) = false
                                                            then None
                                                            else Some fragmentArray
                               )
                        else Some fragmentArray
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:FragmentArray) (item2:FragmentArray) =
                item1.Measure=item2.Measure

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:FragmentArray) =
                    FragmentArrayHandler.tryFindByMeasureName dbContext item.Values
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match FragmentArrayHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:FragmentArray) =
                FragmentArrayHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type IndexHandler =
            ///Initializes a index-object with at least all necessary parameters.
            static member init
                (
                    index : int,
                    ?id   : string
                ) =
                let id' = defaultArg id (System.Guid.NewGuid().ToString())

                new Index(
                          id', 
                          Nullable(index), 
                          Nullable(DateTime.Now)
                         )

            ///Tries to find a index-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.Index.Local do
                           if i.ID=id
                              then select i
                      }
                |> (fun index -> 
                    if (Seq.exists (fun index' -> index' <> null) index) = false
                        then 
                            query {
                                   for i in dbContext.Index do
                                       if i.ID=id
                                          then select i
                                  }
                            |> (fun index -> if (Seq.exists (fun index' -> index' <> null) index) = false
                                                then None
                                                else Some (index.Single())
                               )
                        else Some (index.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByIndexItem (dbContext:MzIdentML) (item:Nullable<int>) =
                query {
                       for i in dbContext.Index.Local do
                           if i.Index=item
                              then select i
                      }
                |> (fun index -> 
                    if (Seq.exists (fun index' -> index' <> null) index) = false
                        then 
                            query {
                                   for i in dbContext.Index do
                                       if i.Index=item
                                          then select i
                                  }
                            |> Seq.map (fun (index) -> index)
                            |> (fun index -> if (Seq.exists (fun index' -> index' <> null) index) = false
                                                            then None
                                                            else Some index
                               )
                        else Some index
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Index) (item2:Index) =
                item1.ID = item2.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Index) =
                    IndexHandler.tryFindByIndexItem dbContext item.Index
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match IndexHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Index) =
                IndexHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type IonTypeHandler =
            ///Initializes a iontype-object with at least all necessary parameters.
            static member init
                (
                    details        : seq<IonTypeParam>,
                    ?id            : string,
                    ?index         : seq<Index>,
                    ?fragmentArray : seq<FragmentArray>
                ) =
                let id'            = defaultArg id (System.Guid.NewGuid().ToString())
                let index'         = convertOptionToList index
                let fragmentArray' = convertOptionToList fragmentArray
                    
                new IonType(
                            id', 
                            index', 
                            fragmentArray', 
                            details |> List, Nullable(DateTime.Now)
                           )

            ///Adds a index to an existing iontype-object.
            static member addIndex
                (index:Index) (ionType:IonType) =
                let result = ionType.Index <- addToList ionType.Index index
                ionType

            ///Adds a collection of indexes to an existing iontype-object.
            static member addIndexes
                (index:seq<Index>) (ionType:IonType) =
                let result = ionType.Index <- addCollectionToList ionType.Index index
                ionType

            ///Adds a fragmentarray to an existing iontype-object.
            static member addFragmentArray
                (fragmentArray:FragmentArray) (ionType:IonType) =
                let result = ionType.FragmentArrays <- addToList ionType.FragmentArrays fragmentArray
                ionType

            ///Adds a collection of fragmentarrays to an existing iontype-object.
            static member addFragmentArrays
                (fragmentArrays:seq<FragmentArray>) (ionType:IonType) =
                let result = ionType.FragmentArrays <- addCollectionToList ionType.FragmentArrays fragmentArrays
                ionType

            ///Tries to find a ionType-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.IonType.Local do
                           if i.ID=id
                              then select (i, i.FragmentArrays, i.Details)
                      }
                |> Seq.map (fun (ionType, _, _) -> ionType)
                |> (fun ionType -> 
                    if (Seq.exists (fun ionType' -> ionType' <> null) ionType) = false
                        then 
                            query {
                                   for i in dbContext.IonType do
                                       if i.ID=id
                                          then select (i, i.FragmentArrays, i.Details)
                                  }
                            |> Seq.map (fun (ionType, _, _) -> ionType)
                            |> (fun ionType -> if (Seq.exists (fun ionType' -> ionType' <> null) ionType) = false
                                                then None
                                                else Some (ionType.Single())
                               )
                        else Some (ionType.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByDetails (dbContext:MzIdentML) (details:seq<IonTypeParam>) =
                query {
                       for i in dbContext.IonType.Local do
                           if i.Details=(details |> List)
                              then select (i, i.FragmentArrays, i.Details)
                      }
                |> Seq.map (fun (ionType, _, _) -> ionType)
                |> (fun ionType -> 
                    if (Seq.exists (fun ionType' -> ionType' <> null) ionType) = false
                        then 
                            query {
                                   for i in dbContext.IonType do
                                       if i.Details=(details |> List)
                                          then select (i, i.FragmentArrays, i.Details)
                                  }
                            |> Seq.map (fun (ionType, _, _) -> ionType)
                            |> (fun ionType -> if (Seq.exists (fun ionType' -> ionType' <> null) ionType) = false
                                                            then None
                                                            else Some ionType
                               )
                        else Some ionType
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:IonType) (item2:IonType) =
                item1.ID = item2.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:IonType) =
                    IonTypeHandler.tryFindByDetails dbContext item.Details
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match IonTypeHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:IonType) =
                IonTypeHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SpectraDataHandler =
            ///Initializes a spectradata-object with at least all necessary parameters.
            static member init
                ( 
                    location                     : string,
                    fileFormat                   : CVParam,
                    spectrumIDFormat             : CVParam,
                    ?id                          : string,
                    ?name                        : string,
                    ?externalFormatDocumentation : string
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>

                new SpectraData(
                                id', 
                                name', 
                                location, 
                                externalFormatDocumentation', 
                                fileFormat, spectrumIDFormat, 
                                Nullable(DateTime.Now)
                               )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (spectraData:SpectraData) =
                spectraData.Name <- name
                spectraData

            ///Replaces name of externalformatdocumentation object with new externalformatdocumentation.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (spectraData:SpectraData) =
                spectraData.ExternalFormatDocumentation <- externalFormatDocumentation
                spectraData

            ///Tries to find a spectraData-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SpectraData.Local do
                           if i.ID=id
                              then select (i, i.FileFormat, i.SpectrumIDFormat)
                      }
                |> Seq.map (fun (spectraData, _, _) -> spectraData)
                |> (fun spectraData -> 
                    if (Seq.exists (fun spectraData' -> spectraData' <> null) spectraData) = false
                        then 
                            query {
                                   for i in dbContext.SpectraData do
                                       if i.ID=id
                                          then select (i, i.FileFormat, i.SpectrumIDFormat)
                                  }
                            |> Seq.map (fun (spectraData, _, _) -> spectraData)
                            |> (fun spectraData -> if (Seq.exists (fun spectraData' -> spectraData' <> null) spectraData) = false
                                                    then None
                                                    else Some (spectraData.Single())
                               )
                        else Some (spectraData.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByLocation (dbContext:MzIdentML) (location:string) =
                query {
                       for i in dbContext.SpectraData.Local do
                           if i.Location=location
                              then select (i, i.FileFormat, i.SpectrumIDFormat)
                      }
                |> Seq.map (fun (spectraData, _, _) -> spectraData)
                |> (fun spectraData -> 
                    if (Seq.exists (fun spectraData' -> spectraData' <> null) spectraData) = false
                        then 
                            query {
                                   for i in dbContext.SpectraData do
                                       if i.Location=location
                                          then select (i, i.FileFormat, i.SpectrumIDFormat)
                                  }
                            |> Seq.map (fun (spectraData, _, _) -> spectraData)
                            |> (fun spectraData -> if (Seq.exists (fun spectraData' -> spectraData' <> null) spectraData) = false
                                                            then None
                                                            else Some spectraData
                               )
                        else Some spectraData
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SpectraData) (item2:SpectraData) =
                item1.ID = item2.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SpectraData) =
                    SpectraDataHandler.tryFindByLocation dbContext item.Location
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SpectraDataHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SpectraData) =
                SpectraDataHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        //type SpecificityRulesHandler =
        //    ///Initializes a specificityrules-object with at least all necessary parameters.
        //    static member init
        //        ( 
        //            details    : seq<SpecificityRuleParam>,
        //            ?id        : string
        //        ) =
        //        let id' = defaultArg id (System.Guid.NewGuid().ToString())
                    
        //        new SpecificityRule(
        //                            id', 
        //                            details |> List, 
        //                            Nullable(DateTime.Now)
        //                           )

        //    ///Tries to find a ontology-object in the context and database, based on its primary-key(ID).
        //    static member tryFindByID
        //        (context:MzIdentML) (specificityRuleID:string) =
        //        tryFind (context.SpecificityRule.Find(specificityRuleID))

        //    ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
        //    static member tryFindByDetails (dbContext:MzIdentML) (details:seq<SpecificityRuleParam>) =
        //        query {
        //               for i in dbContext.SpecificityRule.Local do
        //                   if i.Details=(details |> List)
        //                      then select (i, i.Details)
        //              }
        //        |> Seq.map (fun (specificityRule, _) -> specificityRule)
        //        |> (fun specificityRule -> 
        //            if (Seq.exists (fun specificityRule' -> specificityRule' <> null) specificityRule) = false
        //                then 
        //                    query {
        //                           for i in dbContext.SpecificityRule do
        //                               if i.Details=(details |> List)
        //                                  then select (i, i.Details)
        //                          }
        //                    |> Seq.map (fun (specificityRule, _) -> specificityRule)
        //                    |> (fun specificityRule -> if (Seq.exists (fun specificityRule' -> specificityRule' <> null) specificityRule) = false
        //                                                    then None
        //                                                    else Some specificityRule
        //                       )
        //                else Some specificityRule
        //           )

        //    ///Checks whether all other fields of the current object and context object have the same values or not.
        //    static member private hasEqualFieldValues (item1:SpecificityRule) (item2:SpecificityRule) =
        //        item1.ID = item2.ID

        //    ///First checks if any object with same field-values (except primary key) exists within the context or database. 
              ///If no entry exists, a new object is added to the context and otherwise does nothing.
        //    static member addToContext (dbContext:MzIdentML) (item:SpecificityRule) =
        //            SpecificityRulesHandler.tryFindByDetails dbContext item.Details
        //            |> (fun organizationCollection -> match organizationCollection with
        //                                              |Some x -> x
        //                                                         |> Seq.map (fun organization -> match SpecificityRulesHandler.hasEqualFieldValues organization item with
        //                                                                                         |true -> null
        //                                                                                         |false -> dbContext.Add item
        //                                                                    ) |> ignore
        //                                              |None -> dbContext.Add item |> ignore
        //               )

        //    ///First checks if any object with same field-values (except primary key) exists within the context or database. 
              ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
        //    static member addToContextAndInsert (dbContext:MzIdentML) (item:SpecificityRule) =
        //        SpecificityRulesHandler.addToContext dbContext item |> ignore
        //        dbContext.SaveChanges()

        type SearchModificationHandler =
            ///Initializes a searchmodification-object with at least all necessary parameters.
            static member init
                ( 
                    fixedMod          : bool,
                    massDelta         : float,
                    residues          : string,
                    details           : seq<SearchModificationParam>,
                    ?id               : string,
                    ?specificityRules : seq<SpecificityRuleParam>
                ) =
                let id'               = defaultArg id (System.Guid.NewGuid().ToString())
                let specificityRules' = convertOptionToList specificityRules
                    
                new SearchModification(
                                       id', 
                                       Nullable(fixedMod), 
                                       Nullable(massDelta), 
                                       residues, 
                                       specificityRules', 
                                       details |> List, Nullable(DateTime.Now)
                                      )

            ///Adds a specificityruleparam to an existing searchmodification-object.
            static member addSpecificityRule
                (specificityRule:SpecificityRuleParam) (searchModification:SearchModification) =
                let result = searchModification.SpecificityRules <- addToList searchModification.SpecificityRules specificityRule
                searchModification

            ///Adds a collection of specificityruleparams to an existing searchmodification-object.
            static member addSpecificityRules
                (specificityRules:seq<SpecificityRuleParam>) (searchModification:SearchModification) =
                let result = searchModification.SpecificityRules <- addCollectionToList searchModification.SpecificityRules specificityRules
                searchModification

            ///Tries to find a searchModification-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SearchModification.Local do
                           if i.ID=id
                              then select (i, i.SpecificityRules, i.Details)
                      }
                |> Seq.map (fun (searchModification, _, _) -> searchModification)
                |> (fun searchModification -> 
                    if (Seq.exists (fun searchModification' -> searchModification' <> null) searchModification) = false
                        then 
                            query {
                                   for i in dbContext.SearchModification do
                                       if i.ID=id
                                          then select (i, i.SpecificityRules, i.Details)
                                  }
                            |> Seq.map (fun (searchModification, _, _) -> searchModification)
                            |> (fun searchModification -> if (Seq.exists (fun searchModification' -> searchModification' <> null) searchModification) = false
                                                            then None
                                                            else Some (searchModification.Single())
                               )
                        else Some (searchModification.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByMassDelta (dbContext:MzIdentML) (massDelta:Nullable<float>) =
                query {
                       for i in dbContext.SearchModification.Local do
                           if i.MassDelta=massDelta
                              then select (i, i.SpecificityRules, i.Details)
                      }
                |> Seq.map (fun (searchModification, _, _) -> searchModification)
                |> (fun searchModification -> 
                    if (Seq.exists (fun searchModification' -> searchModification' <> null) searchModification) = false
                        then 
                            query {
                                   for i in dbContext.SearchModification do
                                       if i.MassDelta=massDelta
                                          then select (i, i.SpecificityRules, i.Details)
                                  }
                            |> Seq.map (fun (searchModification, _, _) -> searchModification)
                            |> (fun searchModification -> if (Seq.exists (fun searchModification' -> searchModification' <> null) searchModification) = false
                                                            then None
                                                            else Some searchModification
                               )
                        else Some searchModification
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SearchModification) (item2:SearchModification) =
                item1.FixedMod=item2.FixedMod && item1.Residues=item2.Residues && item1.Residues=item2.Residues &&
                item1.SpecificityRules=item2.SpecificityRules && matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SearchModification) =
                    SearchModificationHandler.tryFindByMassDelta dbContext item.MassDelta
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SearchModificationHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SearchModification) =
                SearchModificationHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type EnzymeHandler =
            ///Initializes a enzyme-object with at least all necessary parameters.
            static member init
                (
                    ?id              : string,
                    ?name            : string,
                    ?cTermGain       : string,
                    ?nTermGain       : string,
                    ?minDistance     : int,
                    ?missedCleavages : int,
                    ?semiSpecific    : bool,
                    ?siteRegexc      : string,
                    ?enzymeName      : seq<EnzymeNameParam>
                ) =
                let id'              = defaultArg id (System.Guid.NewGuid().ToString())
                let name'            = defaultArg name Unchecked.defaultof<string>
                let cTermGain'       = defaultArg cTermGain Unchecked.defaultof<string>
                let nTermGain'       = defaultArg nTermGain Unchecked.defaultof<string>
                let minDistance'     = defaultArg minDistance Unchecked.defaultof<int>
                let missedCleavages' = defaultArg missedCleavages Unchecked.defaultof<int>
                let semiSpecific'    = defaultArg semiSpecific Unchecked.defaultof<bool>
                let siteRegexc'      = defaultArg siteRegexc Unchecked.defaultof<string>
                let enzymeName'      = convertOptionToList enzymeName
                    
                new Enzyme(
                           id', 
                           name', 
                           cTermGain', 
                           nTermGain', 
                           Nullable(minDistance'), 
                           Nullable(missedCleavages'), 
                           Nullable(semiSpecific'), 
                           siteRegexc', 
                           enzymeName', 
                           Nullable(DateTime.Now)
                          )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (enzyme:Enzyme) =
                enzyme.Name <- name
                enzyme

            ///Replaces ctermgain of existing object with new ctermgain.
            static member addCTermGain
                (cTermGain:string) (enzyme:Enzyme) =
                enzyme.CTermGain <- cTermGain
                enzyme

            ///Replaces ntermgain of existing object with new ntermgain.
            static member addNTermGain
                (nTermGain:string) (enzyme:Enzyme) =
                enzyme.NTermGain <- nTermGain
                enzyme

            ///Replaces mindistance of existing object with new mindistance.
            static member addMinDistance
                (minDistance:int) (enzyme:Enzyme) =
                enzyme.MinDistance <- Nullable(minDistance)
                enzyme

            ///Replaces missedcleavages of existing object with new missedcleavages.
            static member addMissedCleavages
                (missedCleavages:int) (enzyme:Enzyme) =
                enzyme.MissedCleavages <-Nullable( missedCleavages)
                enzyme

            ///Replaces semispecific of existing object with new semispecific.
            static member addSemiSpecific
                (semiSpecific:bool) (enzyme:Enzyme) =
                enzyme.SemiSpecific <- Nullable(semiSpecific)
                enzyme

            ///Replaces siteregexc of existing object with new siteregexc.
            static member addSiteRegexc
                (siteRegexc:string) (enzyme:Enzyme) =
                enzyme.SiteRegexc <- siteRegexc
                enzyme

            ///Adds new enzymename to collection of enzymenames.
            static member addEnzymeName
                (enzymeName:EnzymeNameParam) (enzyme:Enzyme) =
                let result = enzyme.EnzymeName <- addToList enzyme.EnzymeName enzymeName
                enzyme

            ///Add new collection of enzymenames to collection of enzymenames.
            static member addEnzymeNames
                (enzymeNames:seq<EnzymeNameParam>) (enzyme:Enzyme) =
                let result = enzyme.EnzymeName <- addCollectionToList enzyme.EnzymeName enzymeNames
                enzyme

            ///Tries to find a enzyme-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.Enzyme.Local do
                           if i.ID=id
                              then select (i, i.EnzymeName)
                      }
                |> Seq.map (fun (enzyme, _) -> enzyme)
                |> (fun enzyme -> 
                    if (Seq.exists (fun enzyme' -> enzyme' <> null) enzyme) = false
                        then 
                            query {
                                   for i in dbContext.Enzyme do
                                       if i.ID=id
                                          then select (i, i.EnzymeName)
                                  }
                            |> Seq.map (fun (enzyme, _) -> enzyme)
                            |> (fun enzyme -> if (Seq.exists (fun enzyme' -> enzyme' <> null) enzyme) = false
                                                then None
                                                else Some (enzyme.Single())
                               )
                        else Some (enzyme.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.Enzyme.Local do
                           if i.Name=name
                              then select (i, i.EnzymeName)
                      }
                |> Seq.map (fun (enzyme, _) -> enzyme)
                |> (fun enzyme -> 
                    if (Seq.exists (fun enzyme' -> enzyme' <> null) enzyme) = false
                        then 
                            query {
                                   for i in dbContext.Enzyme do
                                       if i.Name=name
                                          then select (i, i.EnzymeName)
                                  }
                            |> Seq.map (fun (enzyme, _) -> enzyme)
                            |> (fun enzyme -> if (Seq.exists (fun enzyme' -> enzyme' <> null) enzyme) = false
                                                            then None
                                                            else Some enzyme
                               )
                        else Some enzyme
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Enzyme) (item2:Enzyme) =
                item1.CTermGain=item2.CTermGain && item1.NTermGain=item2.NTermGain && item1.MinDistance=item2.MinDistance &&
                item1.MissedCleavages=item2.MissedCleavages && item1.SemiSpecific=item2.SemiSpecific &&
                item1.SiteRegexc=item2.SiteRegexc && item1.EnzymeName=item2.EnzymeName

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Enzyme) =
                    EnzymeHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match EnzymeHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Enzyme) =
                EnzymeHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type FilterHandler =
            ///Initializes a filter-object with at least all necessary parameters.
            static member init
                (
                    filterType : CVParam,
                    ?id        : string,
                    ?includes  : seq<IncludeParam>,
                    ?excludes  : seq<ExcludeParam>
                ) =
                let id'         = defaultArg id (System.Guid.NewGuid().ToString())
                let includes'   = convertOptionToList includes
                let excludes'   = convertOptionToList excludes
                    
                new Filter(
                           id', 
                           filterType, 
                           includes', 
                           excludes', 
                           Nullable(DateTime.Now)
                          )

            ///Adds a includeparam to an existing filter-object.
            static member addInclude
                (include':IncludeParam) (filter:Filter) =
                let result = filter.Includes <- addToList filter.Includes include'
                filter

            ///Adds a collection of includeparams to an existing filter-object.
            static member addIncludes
                (includes:seq<IncludeParam>) (filter:Filter) =
                let result = filter.Includes <- addCollectionToList filter.Includes includes
                filter

            ///Adds a excludeparam to an existing filter-object.
            static member addExclude
                (exclude':ExcludeParam) (filter:Filter) =
                let result = filter.Excludes <- addToList filter.Excludes exclude'
                filter

            ///Adds a collection of excludeparams to an existing filter-object.
            static member addExcludes
                (excludes:seq<ExcludeParam>) (filter:Filter) =
                let result = filter.Excludes <- addCollectionToList filter.Excludes excludes
                filter

            ///Tries to find a filter-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.Filter.Local do
                           if i.ID=id
                              then select (i, i.FilterType, i.Includes, i.Excludes)
                      }
                |> Seq.map (fun (filter, _, _, _) -> filter)
                |> (fun filter -> 
                    if (Seq.exists (fun filter' -> filter' <> null) filter) = false
                        then 
                            query {
                                   for i in dbContext.Filter do
                                       if i.ID=id
                                          then select (i, i.FilterType, i.Includes, i.Excludes)
                                  }
                            |> Seq.map (fun (filter, _, _, _) -> filter)
                            |> (fun filter -> if (Seq.exists (fun filter' -> filter' <> null) filter) = false
                                                then None
                                                else Some (filter.Single())
                               )
                        else Some (filter.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByTermName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.Filter.Local do
                           if i.FilterType.Term.Name=name
                              then select (i, i.FilterType, i.Includes, i.Excludes)
                      }
                |> Seq.map (fun (filter, _, _, _) -> filter)
                |> (fun enzyme -> 
                    if (Seq.exists (fun enzyme' -> enzyme' <> null) enzyme) = false
                        then 
                            query {
                                   for i in dbContext.Filter do
                                       if i.FilterType.Term.Name=name
                                          then select (i, i.FilterType, i.Includes, i.Excludes)
                                  }
                            |> Seq.map (fun (filter, _, _, _) -> filter)
                            |> (fun enzyme -> if (Seq.exists (fun enzyme' -> enzyme' <> null) enzyme) = false
                                                            then None
                                                            else Some enzyme
                               )
                        else Some enzyme
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Filter) (item2:Filter) =
                item1.Includes=item2.Includes && item1.Excludes=item2.Excludes

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Filter) =
                    FilterHandler.tryFindByTermName dbContext item.FilterType.Term.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match FilterHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Filter) =
                FilterHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type FrameHandler =
            ///Initializes a frame-object with at least all necessary parameters.
            static member init
                ( 
                    frame : int,
                    ?id   : string
                ) =
                let id'   = defaultArg id (System.Guid.NewGuid().ToString())
                    
                new Frame(
                          id', 
                          Nullable(frame), 
                          Nullable(DateTime.Now)
                         )

            ///Tries to find a frame-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.Frame.Local do
                           if i.ID=id
                              then select i
                      }
                |> (fun frame -> 
                    if (Seq.exists (fun frame' -> frame' <> null) frame) = false
                        then 
                            query {
                                   for i in dbContext.Frame do
                                       if i.ID=id
                                          then select i
                                  }
                            |> (fun frame -> if (Seq.exists (fun frame' -> frame' <> null) frame) = false
                                                then None
                                                else Some (frame.Single())
                               )
                        else Some (frame.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFrameItem (dbContext:MzIdentML) (item:Nullable<int>) =
                query {
                       for i in dbContext.Frame.Local do
                           if i.Frame=item
                              then select i
                      }
                |> (fun frame -> 
                    if (Seq.exists (fun frame' -> frame' <> null) frame) = false
                        then 
                            query {
                                   for i in dbContext.Frame do
                                       if i.Frame=item
                                          then select i
                                  }
                            |> Seq.map (fun (frame) -> frame)
                            |> (fun frame -> if (Seq.exists (fun frame' -> frame' <> null) frame) = false
                                                            then None
                                                            else Some frame
                               )
                        else Some frame
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Frame) (item2:Frame) =
                item1.ID = item2.ID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Frame) =
                    FrameHandler.tryFindByFrameItem dbContext item.Frame
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match FrameHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Frame) =
                FrameHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationProtocolHandler =
            ///Initializes a spectrumidentificationprotocol-object with at least all necessary parameters.
            static member init
                (
                    analysisSoftware        : AnalysisSoftware,
                    searchType              : CVParam ,
                    threshold               : seq<ThresholdParam>,
                    ?id                     : string,
                    ?name                   : string,
                    ?additionalSearchParams : seq<AdditionalSearchParam>,
                    ?modificationParams     : seq<SearchModification>,
                    ?enzymes                : seq<Enzyme>,
                    ?independent_Enzymes    : bool,
                    ?massTables             : seq<MassTable>,
                    ?fragmentTolerance      : seq<FragmentToleranceParam>,
                    ?parentTolerance        : seq<ParentToleranceParam>,
                    ?databaseFilters        : seq<Filter>,
                    ?frames                 : seq<Frame>,
                    ?translationTable       : seq<TranslationTable>,
                    ?fkMzIdentML            : string
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                   = defaultArg name Unchecked.defaultof<string>
                let additionalSearchParams' = convertOptionToList additionalSearchParams
                let modificationParams'     = convertOptionToList modificationParams
                let enzymes'                = convertOptionToList enzymes
                let independent_Enzymes'    = defaultArg independent_Enzymes Unchecked.defaultof<bool>
                let massTables'             = convertOptionToList massTables
                let fragmentTolerance'      = convertOptionToList fragmentTolerance
                let parentTolerance'        = convertOptionToList parentTolerance
                let databaseFilters'        = convertOptionToList databaseFilters
                let frames'                 = convertOptionToList frames
                let translationTable'       = convertOptionToList translationTable
                let fkMzIdentML'            = defaultArg fkMzIdentML Unchecked.defaultof<string>
                    
                new SpectrumIdentificationProtocol(
                                                   id', 
                                                   name', 
                                                   analysisSoftware, 
                                                   searchType, 
                                                   additionalSearchParams',
                                                   modificationParams', 
                                                   enzymes',
                                                   Nullable(independent_Enzymes'), 
                                                   massTables',
                                                   fragmentTolerance', 
                                                   parentTolerance', 
                                                   threshold |> List, 
                                                   databaseFilters',
                                                   frames', 
                                                   translationTable', 
                                                   fkMzIdentML', 
                                                   Nullable(DateTime.Now)
                                                  )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                spectrumIdentificationProtocol.Name <- name
                spectrumIdentificationProtocol

            ///Adds a additionalsearchparam to an existing spectrumIdentificationprotocol-object.
            static member addAdditionalSearchParam
                (additionalSearchParam:AdditionalSearchParam) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.AdditionalSearchParams <- addToList spectrumIdentificationProtocol.AdditionalSearchParams additionalSearchParam
                spectrumIdentificationProtocol

            ///Adds a collection of additionalsearchparams to an existing spectrumIdentificationprotocol-object.
            static member addAdditionalSearchParams
                (additionalSearchParams:seq<AdditionalSearchParam>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.AdditionalSearchParams <- addCollectionToList spectrumIdentificationProtocol.AdditionalSearchParams additionalSearchParams
                spectrumIdentificationProtocol

            ///Adds a modificationparam to an existing spectrumIdentificationprotocol-object.
            static member addModificationParam
                (modificationParam:SearchModification) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.ModificationParams <- addToList spectrumIdentificationProtocol.ModificationParams modificationParam
                spectrumIdentificationProtocol

            ///Adds a collection of modificationparams to an existing spectrumIdentificationprotocol-object.
            static member addModificationParams
                (modificationParams:seq<SearchModification>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.ModificationParams <- addCollectionToList spectrumIdentificationProtocol.ModificationParams modificationParams
                spectrumIdentificationProtocol

            ///Adds a enzyme to an existing spectrumIdentificationprotocol-object.
            static member addEnzyme
                (enzyme:Enzyme) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.Enzymes <- addToList spectrumIdentificationProtocol.Enzymes enzyme
                spectrumIdentificationProtocol

            ///Adds a collection of enzymes to an existing spectrumIdentificationprotocol-object.
            static member addEnzymes
                (enzymes:seq<Enzyme>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.Enzymes <- addCollectionToList spectrumIdentificationProtocol.Enzymes enzymes
                spectrumIdentificationProtocol

            ///Replaces independent_enzymes of existing object with new independent_enzymes.
            static member addIndependent_Enzymes
                (independent_Enzymes:bool) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                spectrumIdentificationProtocol.Independent_Enzymes <- Nullable(independent_Enzymes)
                spectrumIdentificationProtocol

            ///Adds a masstable to an existing spectrumIdentificationprotocol-object.
            static member addMassTable
                (massTable:MassTable) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.MassTables <- addToList spectrumIdentificationProtocol.MassTables massTable
                spectrumIdentificationProtocol

            ///Adds a collection of masstables to an existing spectrumIdentificationprotocol-object.
            static member addMassTables
                (massTables:seq<MassTable>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.MassTables <- addCollectionToList spectrumIdentificationProtocol.MassTables massTables
                spectrumIdentificationProtocol

            ///Adds a fragmenttolerance to an existing spectrumIdentificationprotocol-object.
            static member addFragmentTolerance
                (fragmentTolerance:FragmentToleranceParam) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.FragmentTolerance <- addToList spectrumIdentificationProtocol.FragmentTolerance fragmentTolerance
                spectrumIdentificationProtocol

            ///Adds a collection of fragmenttolerances to an existing spectrumIdentificationprotocol-object.
            static member addFragmentTolerances
                (fragmentTolerances:seq<FragmentToleranceParam>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.FragmentTolerance <- addCollectionToList spectrumIdentificationProtocol.FragmentTolerance fragmentTolerances
                spectrumIdentificationProtocol

            ///Adds a parenttolerance to an existing spectrumIdentificationprotocol-object.
            static member addParentTolerance
                (parentTolerance:ParentToleranceParam) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.ParentTolerance <- addToList spectrumIdentificationProtocol.ParentTolerance parentTolerance
                spectrumIdentificationProtocol

            ///Adds a collection of parenttolerances to an existing spectrumIdentificationprotocol-object.
            static member addParentTolerances
                (parentTolerances:seq<ParentToleranceParam>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.ParentTolerance <- addCollectionToList spectrumIdentificationProtocol.ParentTolerance parentTolerances
                spectrumIdentificationProtocol

            ///Adds a databasefilter to an existing spectrumIdentificationprotocol-object.
            static member addDatabaseFilter
                (databaseFilter:Filter) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.DatabaseFilters <- addToList spectrumIdentificationProtocol.DatabaseFilters databaseFilter
                spectrumIdentificationProtocol

            ///Adds a collection of databasefilters to an existing spectrumIdentificationprotocol-object.
            static member addDatabaseFilters
                (databaseFilters:seq<Filter>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.DatabaseFilters <- addCollectionToList spectrumIdentificationProtocol.DatabaseFilters databaseFilters
                spectrumIdentificationProtocol

            ///Adds a frame to an existing spectrumIdentificationprotocol-object.
            static member addFrame
                (frame:Frame) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.Frames <- addToList spectrumIdentificationProtocol.Frames frame
                spectrumIdentificationProtocol

            ///Adds a collection of frames to an existing spectrumIdentificationprotocol-object.
            static member addFrames
                (frames:seq<Frame>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.Frames <- addCollectionToList spectrumIdentificationProtocol.Frames frames
                spectrumIdentificationProtocol

            ///Adds a translationtable to an existing spectrumIdentificationprotocol-object.
            static member addTranslationTable
                (translationTable:TranslationTable) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.TranslationTables <- addToList spectrumIdentificationProtocol.TranslationTables translationTable
                spectrumIdentificationProtocol

            ///Adds a collection of translationtables to an existing spectrumIdentificationprotocol-object.
            static member addTranslationTables
                (translationTables:seq<TranslationTable>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.TranslationTables <- addCollectionToList spectrumIdentificationProtocol.TranslationTables translationTables
                spectrumIdentificationProtocol

            ///Replaces MzIdentMLDocumentID of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.MzIdentMLDocumentID <- fkMzIdentML
                spectrumIdentificationProtocol

            ///Tries to find a spectrumIdentificationProtocol-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SpectrumIdentificationProtocol.Local do
                           if i.ID=id
                              then select (i, i.AnalysisSoftware, i.SearchType, i.Threshold, i.AdditionalSearchParams, 
                                           i.ModificationParams, i.Enzymes, i.MassTables, i.FragmentTolerance, 
                                           i.ParentTolerance, i.DatabaseFilters, i.Frames, i.TranslationTables, 
                                           i.MzIdentMLDocumentID
                                          )
                      }
                |> Seq.map (fun (spectrumIdentificationProtocol, _, _, _, _, _, _, _, _, _, _, _, _, _) -> spectrumIdentificationProtocol)
                |> (fun spectrumIdentificationProtocol -> 
                    if (Seq.exists (fun spectrumIdentificationProtocol' -> spectrumIdentificationProtocol' <> null) spectrumIdentificationProtocol) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationProtocol do
                                       if i.ID=id
                                          then select (i, i.AnalysisSoftware, i.SearchType, i.Threshold, i.AdditionalSearchParams, 
                                                       i.ModificationParams, i.Enzymes, i.MassTables, i.FragmentTolerance, 
                                                       i.ParentTolerance, i.DatabaseFilters, i.Frames, i.TranslationTables, 
                                                       i.MzIdentMLDocumentID
                                                      )
                                  }
                            |> Seq.map (fun (spectrumIdentificationProtocol, _, _, _, _, _, _, _, _, _, _, _, _, _) -> spectrumIdentificationProtocol)
                            |> (fun spectrumIdentificationProtocol -> if (Seq.exists (fun spectrumIdentificationProtocol' -> spectrumIdentificationProtocol' <> null) spectrumIdentificationProtocol) = false
                                                                        then None
                                                                        else Some (spectrumIdentificationProtocol.Single())
                               )
                        else Some (spectrumIdentificationProtocol.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.SpectrumIdentificationProtocol.Local do
                           if i.Name=name
                              then select (i, i.AnalysisSoftware, i.SearchType, i.Threshold, i.AdditionalSearchParams, 
                                           i.ModificationParams, i.Enzymes, i.MassTables, i.FragmentTolerance, 
                                           i.ParentTolerance, i.DatabaseFilters, i.Frames, i.TranslationTables, 
                                           i.MzIdentMLDocumentID
                                          )
                      }
                |> Seq.map (fun (spectrumIdentificationProtocol, _, _, _, _, _, _, _, _, _, _, _, _, _) -> spectrumIdentificationProtocol)
                |> (fun spectrumIdentificationProtocol -> 
                    if (Seq.exists (fun spectrumIdentificationProtocol' -> spectrumIdentificationProtocol' <> null) spectrumIdentificationProtocol) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationProtocol do
                                       if i.Name=name
                                          then select (i, i.AnalysisSoftware, i.SearchType, i.Threshold, i.AdditionalSearchParams, 
                                                       i.ModificationParams, i.Enzymes, i.MassTables, i.FragmentTolerance, 
                                                       i.ParentTolerance, i.DatabaseFilters, i.Frames, i.TranslationTables, 
                                                       i.MzIdentMLDocumentID
                                                      )
                                  }
                            |> Seq.map (fun (spectrumIdentificationProtocol, _, _, _, _, _, _, _, _, _, _, _, _, _) -> spectrumIdentificationProtocol)
                            |> (fun spectrumIdentificationProtocol -> if (Seq.exists (fun spectrumIdentificationProtocol' -> spectrumIdentificationProtocol' <> null) spectrumIdentificationProtocol) = false
                                                                        then None
                                                                        else Some spectrumIdentificationProtocol
                               )
                        else Some spectrumIdentificationProtocol
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SpectrumIdentificationProtocol) (item2:SpectrumIdentificationProtocol) =
                item1.SearchType=item2.SearchType && item1.Threshold=item2.Threshold && item1.AdditionalSearchParams=item2.AdditionalSearchParams && 
                item1.ModificationParams=item2.ModificationParams && item1.Enzymes=item2.Enzymes && item1.MassTables=item2.MassTables && 
                item1.FragmentTolerance=item2.FragmentTolerance && item1.ParentTolerance=item2.ParentTolerance && 
                item1.DatabaseFilters=item2.DatabaseFilters && item1.Frames=item2.Frames && item1.TranslationTables=item2.TranslationTables && 
                item1.MzIdentMLDocumentID=item2.MzIdentMLDocumentID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SpectrumIdentificationProtocol) =
                    SpectrumIdentificationProtocolHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SpectrumIdentificationProtocolHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SpectrumIdentificationProtocol) =
                SpectrumIdentificationProtocolHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SearchDatabaseHandler =
            ///Initializes a searchdatabase-object with at least all necessary parameters.
            static member init
                (
                    location                     : string,
                    fileFormat                   : CVParam,
                    databaseName                 : CVParam,
                    ?id                          : string,
                    ?name                        : string,                    
                    ?numDatabaseSequences        : int64,
                    ?numResidues                 : int64,
                    ?releaseDate                 : DateTime,
                    ?version                     : string,
                    ?externalFormatDocumentation : string,
                    ?details                     : seq<SearchDatabaseParam>
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let numDatabaseSequences'        = defaultArg numDatabaseSequences Unchecked.defaultof<int64>
                let numResidues'                 = defaultArg numResidues Unchecked.defaultof<int64>
                let releaseDate'                 = defaultArg releaseDate Unchecked.defaultof<DateTime>
                let version'                     = defaultArg version Unchecked.defaultof<string>
                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let details'                     = convertOptionToList details
                    
                new SearchDatabase(
                                   id', 
                                   name', 
                                   location, 
                                   Nullable(numDatabaseSequences'), 
                                   Nullable(numResidues'), 
                                   Nullable(releaseDate'), 
                                   version',
                                   externalFormatDocumentation', 
                                   fileFormat, 
                                   databaseName, 
                                   details', 
                                   Nullable(DateTime.Now)
                                  )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (searchDatabase:SearchDatabase) =
                searchDatabase.Name <- name
                searchDatabase

            ///Replaces numdatabasesequences of existing object with new numdatabasesequences.
            static member addNumDatabaseSequences
                (numDatabaseSequences:int64) (searchDatabase:SearchDatabase) =
                searchDatabase.NumDatabaseSequences <- Nullable(numDatabaseSequences)
                searchDatabase

            ///Replaces numresidues of existing object with new numresidues.
            static member addNumResidues
                (numResidues:int64) (searchDatabase:SearchDatabase) =
                searchDatabase.NumResidues <- Nullable(numResidues)
                searchDatabase

            ///Replaces releasedate of existing object with new releasedate.
            static member addReleaseDate
                (releaseDate:DateTime) (searchDatabase:SearchDatabase) =
                searchDatabase.ReleaseDate <- Nullable(releaseDate)
                searchDatabase

            ///Replaces version of existing object with new version.
            static member addVersion
                (version:string) (searchDatabase:SearchDatabase) =
                searchDatabase.Version <- version
                searchDatabase

            ///Replaces externalformatdocumentation of existing object with new externalformatdocumentation.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (searchDatabase:SearchDatabase) =
                searchDatabase.Version <- externalFormatDocumentation
                searchDatabase

            ///Adds a searchdatabaseparam to an existing searchdatabase-object.
            static member addDetail
                (detail:SearchDatabaseParam) (searchDatabase:SearchDatabase) =
                let result = searchDatabase.Details <- addToList searchDatabase.Details detail
                searchDatabase

            ///Adds a collection of searchdatabaseparams to an existing searchdatabase-object.
            static member addDetails
                (details:seq<SearchDatabaseParam>) (searchDatabase:SearchDatabase) =
                let result = searchDatabase.Details <- addCollectionToList searchDatabase.Details details
                searchDatabase

            ///Tries to find a searchDatabase-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SearchDatabase.Local do
                           if i.ID=id
                              then select (i, i.DatabaseName, i.FileFormat, i.Details)
                      }
                |> Seq.map (fun (searchDatabase, _, _, _) -> searchDatabase)
                |> (fun searchDatabase -> 
                    if (Seq.exists (fun searchDatabase' -> searchDatabase' <> null) searchDatabase) = false
                        then 
                            query {
                                   for i in dbContext.SearchDatabase do
                                       if i.ID=id
                                          then select (i, i.DatabaseName, i.FileFormat, i.Details)
                                  }
                            |> Seq.map (fun (searchDatabase, _, _, _) -> searchDatabase)
                            |> (fun searchDatabase -> if (Seq.exists (fun searchDatabase' -> searchDatabase' <> null) searchDatabase) = false
                                                        then None
                                                        else Some (searchDatabase.Single())
                               )
                        else Some (searchDatabase.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByLocation (dbContext:MzIdentML) (location:string) =
                query {
                       for i in dbContext.SearchDatabase.Local do
                           if i.Location=location
                              then select (i, i.DatabaseName, i.FileFormat, i.Details)
                      }
                |> Seq.map (fun (searchDatabase, _, _, _) -> searchDatabase)
                |> (fun searchDatabase -> 
                    if (Seq.exists (fun searchDatabase' -> searchDatabase' <> null) searchDatabase) = false
                        then 
                            query {
                                   for i in dbContext.SearchDatabase do
                                       if i.Location=location
                                          then select (i, i.DatabaseName, i.FileFormat, i.Details)
                                  }
                            |> Seq.map (fun (searchDatabase, _, _, _) -> searchDatabase)
                            |> (fun searchDatabase -> if (Seq.exists (fun searchDatabase' -> searchDatabase' <> null) searchDatabase) = false
                                                          then None
                                                          else Some searchDatabase
                               )
                        else Some searchDatabase
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SearchDatabase) (item2:SearchDatabase) =
               item1.Name=item2.Name && item1.NumDatabaseSequences=item2.NumDatabaseSequences && 
               item1.NumResidues=item2.NumResidues && item1.ReleaseDate=item2.ReleaseDate &&
               item1.Version=item2.Version && item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation && 
               matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) && item1.Location=item2.Location && item1.FileFormat=item2.FileFormat

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SearchDatabase) =
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SearchDatabase) =
                SearchDatabaseHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type DBSequenceHandler =
            ///Initializes a dbsequence-object with at least all necessary parameters.
            static member init
                (
                    accession      : string,
                    searchDatabase : SearchDatabase,
                    ?id            : string,
                    ?name          : string,
                    ?sequence      : string,
                    ?length        : int,
                    ?details       : seq<DBSequenceParam>,
                    ?fkMzIdentML   : string
                ) =
                let id'          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'        = defaultArg name Unchecked.defaultof<string>
                let sequence'    = defaultArg sequence Unchecked.defaultof<string>
                let length'      = defaultArg length Unchecked.defaultof<int>
                let details'     = convertOptionToList details
                let fkMzIdentML' = defaultArg fkMzIdentML Unchecked.defaultof<string>
                    
                new DBSequence(
                               id', 
                               name', 
                               accession, 
                               searchDatabase, 
                               sequence', 
                               Nullable(length'), 
                               details', 
                               fkMzIdentML', 
                               Nullable(DateTime.Now)
                              )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (dbSequence:DBSequence) =
                dbSequence.Name <- name
                dbSequence

            ///Replaces sequence of existing object with new sequence.
            static member addSequence
                (sequence:string) (dbSequence:DBSequence) =
                dbSequence.Sequence <- sequence
                dbSequence

            ///Replaces length of existing object with new length.
            static member addLength
                (length:int) (dbSequence:DBSequence) =
                dbSequence.Length <- Nullable(length)
                dbSequence

            ///Adds a dbsequenceparam to an existing dbsequence-object.
            static member addDetail
                (detail:DBSequenceParam) (dbSequence:DBSequence) =
                let result = dbSequence.Details <- addToList dbSequence.Details detail
                dbSequence

            ///Adds a collection of dbsequenceparams to an existing dbsequence-object.
            static member addDetails
                (details:seq<DBSequenceParam>) (dbSequence:DBSequence) =
                let result = dbSequence.Details <- addCollectionToList dbSequence.Details details
                dbSequence

            ///Replaces MzIdentMLDocumentID of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (dbSequence:DBSequence) =
                let result = dbSequence.MzIdentMLDocumentID <- fkMzIdentML
                dbSequence

            ///Tries to find a dbSequence-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.DBSequence.Local do
                           if i.ID=id
                              then select (i, i.SearchDatabase, i.MzIdentMLDocumentID, i.Details)
                      }
                |> Seq.map (fun (dbSequence, _, _, _) -> dbSequence)
                |> (fun dbSequence -> 
                    if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
                        then 
                            query {
                                   for i in dbContext.DBSequence do
                                       if i.ID=id
                                          then select (i, i.SearchDatabase, i.MzIdentMLDocumentID, i.Details)
                                  }
                            |> Seq.map (fun (dbSequence, _, _, _) -> dbSequence)
                            |> (fun dbSequence -> if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
                                                    then None
                                                    else Some (dbSequence.Single())
                               )
                        else Some (dbSequence.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByAccession (dbContext:MzIdentML) (accession:string) =
                query {
                       for i in dbContext.DBSequence.Local do
                           if i.Accession=accession
                              then select (i, i.SearchDatabase, i.MzIdentMLDocumentID, i.Details)
                      }
                |> Seq.map (fun (dbSequence, _, _, _) -> dbSequence)
                |> (fun dbSequence -> 
                    if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
                        then 
                            query {
                                   for i in dbContext.DBSequence do
                                       if i.Accession=accession
                                          then select (i, i.SearchDatabase, i.MzIdentMLDocumentID, i.Details)
                                  }
                            |> Seq.map (fun (dbSequence, _, _, _) -> dbSequence)
                            |> (fun dbSequence -> if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
                                                          then None
                                                          else Some dbSequence
                               )
                        else Some dbSequence
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:DBSequence) (item2:DBSequence) =
               item1.Name=item2.Name && item1.Sequence=item2.Sequence && item1.Length=item2.Length && 
               matchCVParamBases (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) && item1.MzIdentMLDocumentID=item2.MzIdentMLDocumentID &&
               item1.SearchDatabase=item2.SearchDatabase

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:DBSequence) =
                    DBSequenceHandler.tryFindByAccession dbContext item.Accession
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match DBSequenceHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:DBSequence) =
                DBSequenceHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type PeptideEvidenceHandler =
            ///Initializes a peptideevidence-object with at least all necessary parameters.
            static member init
                (
                    dbSequence                  : DBSequence,
                    peptide                     : Peptide,
                    ?id                         : string,
                    ?name                       : string,
                    ?start                      : int,
                    ?end'                       : int,
                    ?pre                        : string,
                    ?post                       : string,
                    ?frame                      : Frame,
                    ?isDecoy                    : bool,
                    ?translationTable           : TranslationTable,
                    ?details                    : seq<PeptideEvidenceParam>,
                    ?fkMzIdentML                : string
                ) =
                let id'               = defaultArg id (System.Guid.NewGuid().ToString())
                let name'             = defaultArg name Unchecked.defaultof<string>
                let start'            = defaultArg start Unchecked.defaultof<int>
                let end''             = defaultArg end' Unchecked.defaultof<int>
                let pre'              = defaultArg pre Unchecked.defaultof<string>
                let post'             = defaultArg post Unchecked.defaultof<string>
                let frame'            = defaultArg frame Unchecked.defaultof<Frame>
                let isDecoy'          = defaultArg isDecoy Unchecked.defaultof<bool>
                let translationTable' = defaultArg translationTable Unchecked.defaultof<TranslationTable>
                let details'          = convertOptionToList details
                let fkMzIdentML'      = defaultArg fkMzIdentML Unchecked.defaultof<string>
                    
                new PeptideEvidence(
                                    id', 
                                    name', 
                                    dbSequence, 
                                    peptide, 
                                    Nullable(start'), 
                                    Nullable(end''), 
                                    pre', 
                                    post', 
                                    frame', 
                                    Nullable(isDecoy'), 
                                    translationTable', 
                                    details', 
                                    fkMzIdentML', 
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.Name <- name
                peptideEvidence

            ///Replaces start of existing object with new start.
            static member addStart
                (start:int) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.Start <- Nullable(start)
                peptideEvidence

            ///Replaces end of existing object with new end.
            static member addEnd 
                (end':int) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.End  <- Nullable(end')
                peptideEvidence

            ///Replaces pre of existing object with new pre.
            static member addPre
                (pre:string) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.Pre <- pre
                peptideEvidence

            ///Replaces post of existing object with new post.
            static member addPost
                (post:string) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.Post <- post
                peptideEvidence

            ///Replaces frame of existing object with new frame.
            static member addFrame
                (frame:Frame) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.Frame <- frame
                peptideEvidence

            ///Replaces isdecoy of existing object with new isdecoy.
            static member addIsDecoy
                (isDecoy:bool) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.IsDecoy <- Nullable(isDecoy)
                peptideEvidence

            ///Replaces translationtable of existing object with new translationtable.
            static member addTranslationTable
                (translationTable:TranslationTable) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.TranslationTable <- translationTable
                peptideEvidence

            ///Adds a peptideevidenceparam to an existing peptideevidence-object.
            static member addDetail
                (detail:PeptideEvidenceParam) (peptideEvidence:PeptideEvidence) =
                let result = peptideEvidence.Details <- addToList peptideEvidence.Details detail
                peptideEvidence

            ///Adds a collection of peptideevidenceparam to an existing peptideevidence-object.
            static member addDetails
                (details:seq<PeptideEvidenceParam>) (peptideEvidence:PeptideEvidence) =
                let result = peptideEvidence.Details <- addCollectionToList peptideEvidence.Details details
                peptideEvidence

            ///Replaces MzIdentMLDocumentID of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (peptideEvidence:PeptideEvidence) =
                let result = peptideEvidence.MzIdentMLDocumentID <- fkMzIdentML
                peptideEvidence

            ///Tries to find a peptideEvidence-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.PeptideEvidence.Local do
                           if i.ID=id
                              then select (i, i.Peptide, i.TranslationTable, i.DBSequence,  i.MzIdentMLDocumentID, i.Details)
                      }
                |> Seq.map (fun (peptideEvidence, _, _, _, _, _) -> peptideEvidence)
                |> (fun peptideEvidence -> 
                    if (Seq.exists (fun peptideEvidence' -> peptideEvidence' <> null) peptideEvidence) = false
                        then 
                            query {
                                   for i in dbContext.PeptideEvidence do
                                       if i.ID=id
                                          then select (i, i.Peptide, i.TranslationTable, i.DBSequence,  i.MzIdentMLDocumentID, i.Details)
                                  }
                            |> Seq.map (fun (peptideEvidence, _, _, _, _, _) -> peptideEvidence)
                            |> (fun peptideEvidence -> if (Seq.exists (fun peptideEvidence' -> peptideEvidence' <> null) peptideEvidence) = false
                                                        then None
                                                        else Some (peptideEvidence.Single())
                               )
                        else Some (peptideEvidence.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.PeptideEvidence.Local do
                           if i.Name=name
                              then select (i, i.Peptide, i.TranslationTable, i.DBSequence,  i.MzIdentMLDocumentID, i.Details)
                      }
                |> Seq.map (fun (peptideEvidence, _, _, _, _, _) -> peptideEvidence)
                |> (fun peptideEvidence -> 
                    if (Seq.exists (fun peptideEvidence' -> peptideEvidence' <> null) peptideEvidence) = false
                        then 
                            query {
                                   for i in dbContext.PeptideEvidence do
                                       if i.Name=name
                                          then select (i, i.Peptide, i.TranslationTable, i.DBSequence,  i.MzIdentMLDocumentID, i.Details)
                                  }
                            |> Seq.map (fun (peptideEvidence, _, _, _, _, _) -> peptideEvidence)
                            |> (fun peptideEvidence -> if (Seq.exists (fun peptideEvidence' -> peptideEvidence' <> null) peptideEvidence) = false
                                                          then None
                                                          else Some peptideEvidence
                               )
                        else Some peptideEvidence
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:PeptideEvidence) (item2:PeptideEvidence) =
               item1.Name=item2.Name && item1.Start=item2.Start && item1.End=item2.End &&
               item1.Pre=item2.Pre && item1.Post=item2.Post && item1.Frame=item2.Frame &&
               item1.IsDecoy=item2.IsDecoy && item1.TranslationTable=item2.TranslationTable && 
               matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) && 
               item1.MzIdentMLDocumentID=item2.MzIdentMLDocumentID && item1.DBSequence=item2.DBSequence

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:PeptideEvidence) =
                    PeptideEvidenceHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PeptideEvidenceHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:PeptideEvidence) =
                PeptideEvidenceHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationItemHandler =
            ///Initializes a spectrumidentificationitem-object with at least all necessary parameters.
            static member init
                (
                    peptide                       : Peptide,
                    chargeState                   : int,
                    experimentalMassToCharge      : float,
                    passThreshold                 : bool,
                    rank                          : int,
                    ?id                           : string,
                    ?name                         : string,
                    ?sample                       : Sample,
                    ?massTable                    : MassTable,
                    ?peptideEvidences             : seq<PeptideEvidence>,
                    ?fragmentations               : seq<IonType>,
                    ?calculatedMassToCharge       : float,
                    ?calculatedPI                 : float,
                    ?details                      : seq<SpectrumIdentificationItemParam>
                ) =
                let id'                           = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                         = defaultArg name Unchecked.defaultof<string>
                let sample'                       = defaultArg sample Unchecked.defaultof<Sample>
                let massTable'                    = defaultArg massTable Unchecked.defaultof<MassTable>
                let peptideEvidences'             = convertOptionToList peptideEvidences
                let fragmentations'               = convertOptionToList fragmentations
                let calculatedMassToCharge'       = defaultArg calculatedMassToCharge Unchecked.defaultof<float>
                let calculatedPI'                 = defaultArg calculatedPI Unchecked.defaultof<float>
                let details'                      = convertOptionToList details
                    
                new SpectrumIdentificationItem(
                                               id', 
                                               name', 
                                               sample', 
                                               massTable', 
                                               Nullable(passThreshold), 
                                               Nullable(rank), 
                                               peptideEvidences',
                                               fragmentations', 
                                               peptide, 
                                               Nullable(chargeState), 
                                               Nullable(experimentalMassToCharge), 
                                               Nullable(calculatedMassToCharge'),
                                               Nullable(calculatedPI'), 
                                               details', 
                                               Nullable(DateTime.Now)
                                              )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                spectrumIdentificationItem.Name <- name
                spectrumIdentificationItem

            ///Replaces sample of existing object with new sample.
            static member addSample
                (sample:Sample) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                spectrumIdentificationItem.Sample <- sample 
                spectrumIdentificationItem

            ///Replaces masstable of existing object with new masstable.
            static member addMassTable
                (massTable:MassTable) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                spectrumIdentificationItem.MassTable <- massTable
                spectrumIdentificationItem

            ///Adds a peptideevidence to an existing spectrumidentification-object.
            static member addPeptideEvidence
                (peptideEvidence:PeptideEvidence) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                let result = spectrumIdentificationItem.PeptideEvidences <- addToList spectrumIdentificationItem.PeptideEvidences peptideEvidence
                spectrumIdentificationItem

            ///Adds a collection of peptideevidences to an existing spectrumidentification-object.
            static member addPeptideEvidences
                (peptideEvidences:seq<PeptideEvidence>) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                let result = spectrumIdentificationItem.PeptideEvidences <- addCollectionToList spectrumIdentificationItem.PeptideEvidences peptideEvidences
                spectrumIdentificationItem   

            ///Adds a fragmentation to an existing spectrumidentification-object.
            static member addFragmentation
                (ionType:IonType) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                let result = spectrumIdentificationItem.Fragmentations <- addToList spectrumIdentificationItem.Fragmentations ionType
                spectrumIdentificationItem

            ///Adds a collection of fragmentations to an existing spectrumidentification-object.
            static member addFragmentations
                (ionTypes:seq<IonType>) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                let result = spectrumIdentificationItem.Fragmentations <- addCollectionToList spectrumIdentificationItem.Fragmentations ionTypes
                spectrumIdentificationItem 

           ///Replaces calculatedmasstocharge of existing object with new calculatedmasstocharge.
            static member addCalculatedMassToCharge
                (calculatedMassToCharge:float) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                spectrumIdentificationItem.CalculatedMassToCharge <- Nullable(calculatedMassToCharge)
                spectrumIdentificationItem

            ///Replaces calculatedpi of existing object with new calculatedpi.
            static member addCalculatedPI
                (calculatedPI:float) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                spectrumIdentificationItem.CalculatedPI <- Nullable(calculatedPI)
                spectrumIdentificationItem

            ///Adds a spectrumidentificationparam to an existing spectrumidentification-object.
            static member addDetail
                (detail:SpectrumIdentificationItemParam) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                let result = spectrumIdentificationItem.Details <- addToList spectrumIdentificationItem.Details detail
                spectrumIdentificationItem

            ///Adds a collection of spectrumidentificationparams to an existing spectrumidentification-object.
            static member addDetails
                (details:seq<SpectrumIdentificationItemParam>) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                let result = spectrumIdentificationItem.Details <- addCollectionToList spectrumIdentificationItem.Details details
                spectrumIdentificationItem

            //static member addSpectrumIdentificationResult
            //     (spectrumIdentificationItem:SpectrumIdentificationItem) (spectrumIdentificationResult:SpectrumIdentificationResult) =
            //     spectrumIdentificationItem.SpectrumIdentificationResult <- spectrumIdentificationResult

            ///Tries to find a spectrumIdentificationItem-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SpectrumIdentificationItem.Local do
                           if i.ID=id
                              then select (i, i.Peptide, i.Sample, i.MassTable, 
                                           i.PeptideEvidences, i.Fragmentations, i.Details
                                          )
                      }
                |> Seq.map (fun (spectrumIdentificationItem, _, _, _, _, _, _) -> spectrumIdentificationItem)
                |> (fun spectrumIdentificationItem -> 
                    if (Seq.exists (fun spectrumIdentificationItem' -> spectrumIdentificationItem' <> null) spectrumIdentificationItem) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationItem do
                                       if i.ID=id
                                          then select (i, i.Peptide, i.Sample, i.MassTable, 
                                                       i.PeptideEvidences, i.Fragmentations, i.Details
                                                      )
                                  }
                            |> Seq.map (fun (spectrumIdentificationItem, _, _, _, _, _, _) -> spectrumIdentificationItem)
                            |> (fun spectrumIdentificationItem -> if (Seq.exists (fun spectrumIdentificationItem' -> spectrumIdentificationItem' <> null) spectrumIdentificationItem) = false
                                                                    then None
                                                                    else Some (spectrumIdentificationItem.Single())
                               )
                        else Some (spectrumIdentificationItem.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByExperimentalMassToCharge (dbContext:MzIdentML) (experimentalMassToCharge:Nullable<float>) =
                query {
                       for i in dbContext.SpectrumIdentificationItem.Local do
                           if i.ExperimentalMassToCharge=experimentalMassToCharge
                              then select (i, i.Peptide, i.Sample, i.MassTable, 
                                           i.PeptideEvidences, i.Fragmentations, i.Details
                                          )
                      }
                |> Seq.map (fun (spectrumIdentificationItem, _, _, _, _, _, _) -> spectrumIdentificationItem)
                |> (fun spectrumIdentificationItem -> 
                    if (Seq.exists (fun spectrumIdentificationItem' -> spectrumIdentificationItem' <> null) spectrumIdentificationItem) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationItem do
                                       if i.ExperimentalMassToCharge=experimentalMassToCharge
                                          then select (i, i.Peptide, i.Sample, i.MassTable, 
                                                       i.PeptideEvidences, i.Fragmentations, i.Details
                                                      )
                                  }
                            |> Seq.map (fun (spectrumIdentificationItem, _, _, _, _, _, _) -> spectrumIdentificationItem)
                            |> (fun peptideEvidence -> if (Seq.exists (fun spectrumIdentificationItem' -> spectrumIdentificationItem' <> null) spectrumIdentificationItem) = false
                                                          then None
                                                          else Some spectrumIdentificationItem
                               )
                        else Some spectrumIdentificationItem
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SpectrumIdentificationItem) (item2:SpectrumIdentificationItem) =
               item1.Name=item2.Name && item1.Sample=item2.Sample && item1.MassTable=item2.MassTable && 
               item1.PassThreshold=item2.PassThreshold && item1.Rank=item2.Rank && item1.PeptideEvidences=item2.PeptideEvidences && 
               item1.Fragmentations=item2.Fragmentations && item1.CalculatedMassToCharge=item2.CalculatedMassToCharge && 
               item1.CalculatedPI=item2.CalculatedPI && matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) && item1.ChargeState=item2.ChargeState && 
               item1.Peptide.PeptideSequence=item2.Peptide.PeptideSequence

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SpectrumIdentificationItem) =
                    SpectrumIdentificationItemHandler.tryFindByExperimentalMassToCharge dbContext item.ExperimentalMassToCharge
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SpectrumIdentificationItemHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SpectrumIdentificationItem) =
                SpectrumIdentificationItemHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationResultHandler =
            ///Initializes a spectrumidentificationresult-object with at least all necessary parameters.
            static member init
                (
                    spectraData                 : SpectraData,
                    spectrumID                  : string,
                    spectrumIdentificationItem  : seq<SpectrumIdentificationItem>,
                    ?id                         : string,
                    ?name                       : string,
                    ?details                    : seq<SpectrumIdentificationResultParam>
                ) =
                let id'                         = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                       = defaultArg name Unchecked.defaultof<string>
                let details'                    = convertOptionToList details
                    
                new SpectrumIdentificationResult(
                                                 id', 
                                                 name', 
                                                 spectraData, 
                                                 spectrumID, 
                                                 spectrumIdentificationItem |> List,
                                                 details', 
                                                 Nullable(DateTime.Now)
                                                )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (spectrumIdentificationResult:SpectrumIdentificationResult)  =
                spectrumIdentificationResult.Name <- name
                spectrumIdentificationResult

            ///Adds a spectrumidentificationresultparam to an existing spectrumidentificationresult-object.
            static member addDetail
                (detail:SpectrumIdentificationResultParam) (spectrumIdentificationResult:SpectrumIdentificationResult) =
                let result = spectrumIdentificationResult.Details <- addToList spectrumIdentificationResult.Details detail
                spectrumIdentificationResult

            ///Adds a collection of spectrumidentificationresultparams to an existing spectrumidentificationresult-object.
            static member addDetails
                (details:seq<SpectrumIdentificationResultParam>) (spectrumIdentificationResult:SpectrumIdentificationResult) =
                let result = spectrumIdentificationResult.Details <- addCollectionToList spectrumIdentificationResult.Details details
                spectrumIdentificationResult

            //static member addSpectrumIdentificationList
            //     (spectrumIdentificationResult:SpectrumIdentificationResult) (spectrumIdentificationList:SpectrumIdentificationList) =
            //     spectrumIdentificationResult.SpectrumIdentificationList <- spectrumIdentificationList

            ///Tries to find a spectrumIdentificationResult-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SpectrumIdentificationResult.Local do
                           if i.ID=id
                              then select (i, i.SpectraData, i.SpectrumIdentificationItem, i.Details)
                      }
                |> Seq.map (fun (spectrumIdentificationResult, _, _, _) -> spectrumIdentificationResult)
                |> (fun spectrumIdentificationResult -> 
                    if (Seq.exists (fun spectrumIdentificationResult' -> spectrumIdentificationResult' <> null) spectrumIdentificationResult) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationResult do
                                       if i.ID=id
                                          then select (i, i.SpectraData, i.SpectrumIdentificationItem, i.Details)
                                  }
                            |> Seq.map (fun (spectrumIdentificationResult, _, _, _) -> spectrumIdentificationResult)
                            |> (fun spectrumIdentificationResult -> if (Seq.exists (fun spectrumIdentificationResult' -> spectrumIdentificationResult' <> null) spectrumIdentificationResult) = false
                                                                    then None
                                                                    else Some (spectrumIdentificationResult.Single())
                               )
                        else Some (spectrumIdentificationResult.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindBySpectrumID (dbContext:MzIdentML) (spectrumID:string) =
                query {
                       for i in dbContext.SpectrumIdentificationResult.Local do
                           if i.SpectrumID=spectrumID
                              then select (i, i.SpectraData, i.SpectrumIdentificationItem, i.Details)
                      }
                |> Seq.map (fun (spectrumIdentificationResult, _, _, _) -> spectrumIdentificationResult)
                |> (fun spectrumIdentificationResult -> 
                    if (Seq.exists (fun spectrumIdentificationResult' -> spectrumIdentificationResult' <> null) spectrumIdentificationResult) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationResult do
                                       if i.SpectrumID=spectrumID
                                          then select (i, i.SpectraData, i.SpectrumIdentificationItem, i.Details)
                                  }
                            |> Seq.map (fun (spectrumIdentificationResult, _, _, _) -> spectrumIdentificationResult)
                            |> (fun spectrumIdentificationResult -> if (Seq.exists (fun spectrumIdentificationResult' -> spectrumIdentificationResult' <> null) spectrumIdentificationResult) = false
                                                                      then None
                                                                      else Some spectrumIdentificationResult
                               )
                        else Some spectrumIdentificationResult
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SpectrumIdentificationResult) (item2:SpectrumIdentificationResult) =
               item1.Name=item2.Name && item1.SpectraData=item2.SpectraData && matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) && 
               item1.SpectrumIdentificationItem=item2.SpectrumIdentificationItem

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SpectrumIdentificationResult) =
                    SpectrumIdentificationResultHandler.tryFindBySpectrumID dbContext item.SpectrumID
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SpectrumIdentificationResultHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SpectrumIdentificationResult) =
                SpectrumIdentificationResultHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationListHandler =
            ///Initializes a spectrumidentificationlist-object with at least all necessary parameters.
            static member init
                (
                    spectrumIdentificationResult : seq<SpectrumIdentificationResult>,
                    ?id                          : string,
                    ?name                        : string,
                    ?numSequencesSearched        : int64,
                    ?fragmentationTable          : seq<Measure>,
                    ?details                     : seq<SpectrumIdentificationListParam>          
                ) =
                let id'                   = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                 = defaultArg name Unchecked.defaultof<string>
                let numSequencesSearched' = defaultArg numSequencesSearched Unchecked.defaultof<int64>
                let fragmentationTable'   = convertOptionToList fragmentationTable
                let details'              = convertOptionToList details
                    
                new SpectrumIdentificationList(
                                               id', 
                                               name', 
                                               Nullable(numSequencesSearched'), 
                                               fragmentationTable', 
                                               spectrumIdentificationResult |> List,
                                               details', 
                                               Nullable(DateTime.Now)
                                              )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (spectrumIdentificationList:SpectrumIdentificationList) =
                spectrumIdentificationList.Name <- name
                spectrumIdentificationList

            ///Replaces numsequencessearched of existing object with new numsequencessearched.
            static member addNumSequencesSearched
                (numSequencesSearched:int64) (spectrumIdentificationList:SpectrumIdentificationList) =
                spectrumIdentificationList.NumSequencesSearched <- Nullable(numSequencesSearched)
                spectrumIdentificationList

            ///Adds a fragmentationtable to an existing spectrumidentificationlist-object.
            static member addFragmentationTable
                (fragmentationTable:Measure) (spectrumIdentificationList:SpectrumIdentificationList) =
                let result = spectrumIdentificationList.FragmentationTables <- addToList spectrumIdentificationList.FragmentationTables fragmentationTable
                spectrumIdentificationList

            ///Adds a collection of fragmentationtables to an existing spectrumidentificationlist-object.
            static member addFragmentationTables
                (fragmentationTables:seq<Measure>) (spectrumIdentificationList:SpectrumIdentificationList) =
                let result = spectrumIdentificationList.FragmentationTables <- addCollectionToList spectrumIdentificationList.FragmentationTables fragmentationTables
                spectrumIdentificationList

            ///Adds a spectrumidentificationlistparam to an existing spectrumidentificationlist-object.
            static member addDetail
                (detail:SpectrumIdentificationListParam) (spectrumIdentificationList:SpectrumIdentificationList) =
                let result = spectrumIdentificationList.Details <- addToList spectrumIdentificationList.Details detail
                spectrumIdentificationList

            ///Adds a collection of spectrumidentificationlistparams to an existing spectrumidentificationlist-object.
            static member addDetails
                (details:seq<SpectrumIdentificationListParam>) (spectrumIdentificationList:SpectrumIdentificationList) =
                let result = spectrumIdentificationList.Details <- addCollectionToList spectrumIdentificationList.Details details
                spectrumIdentificationList

            ///Tries to find a spectrumIdentificationList-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SpectrumIdentificationList.Local do
                           if i.ID=id
                              then select (i, i.FragmentationTables, i.SpectrumIdentificationResult, i.Details)
                      }
                |> Seq.map (fun (spectrumIdentificationList, _, _, _) -> spectrumIdentificationList)
                |> (fun spectrumIdentificationList -> 
                    if (Seq.exists (fun spectrumIdentificationList' -> spectrumIdentificationList' <> null) spectrumIdentificationList) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationList do
                                       if i.ID=id
                                          then select (i, i.FragmentationTables, i.SpectrumIdentificationResult, i.Details)
                                  }
                            |> Seq.map (fun (spectrumIdentificationList, _, _, _) -> spectrumIdentificationList)
                            |> (fun spectrumIdentificationList -> if (Seq.exists (fun spectrumIdentificationList' -> spectrumIdentificationList' <> null) spectrumIdentificationList) = false
                                                                    then None
                                                                    else Some (spectrumIdentificationList.Single())
                               )
                        else Some (spectrumIdentificationList.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.SpectrumIdentificationList.Local do
                           if i.Name=name
                              then select (i, i.FragmentationTables, i.SpectrumIdentificationResult, i.Details)
                      }
                |> Seq.map (fun (spectrumIdentificationList, _, _, _) -> spectrumIdentificationList)
                |> (fun spectrumIdentificationList -> 
                    if (Seq.exists (fun spectrumIdentificationList' -> spectrumIdentificationList' <> null) spectrumIdentificationList) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationList do
                                       if i.Name=name
                                          then select (i, i.FragmentationTables, i.SpectrumIdentificationResult, i.Details)
                                  }
                            |> Seq.map (fun (spectrumIdentificationList, _, _, _) -> spectrumIdentificationList)
                            |> (fun spectrumIdentificationList -> if (Seq.exists (fun spectrumIdentificationList' -> spectrumIdentificationList' <> null) spectrumIdentificationList) = false
                                                                      then None
                                                                      else Some spectrumIdentificationList
                               )
                        else Some spectrumIdentificationList
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SpectrumIdentificationList) (item2:SpectrumIdentificationList) =
               item1.NumSequencesSearched=item2.NumSequencesSearched && item1.FragmentationTables=item2.FragmentationTables &&
               item1.SpectrumIdentificationResult=item2.SpectrumIdentificationResult && matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SpectrumIdentificationList) =
                    SpectrumIdentificationListHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SpectrumIdentificationListHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SpectrumIdentificationList) =
                SpectrumIdentificationListHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationHandler =
            ///Initializes a spectrumidentification-object with at least all necessary parameters.
            static member init
                (
                    spectrumIdentificationList     : SpectrumIdentificationList,
                    spectrumIdentificationProtocol : SpectrumIdentificationProtocol,
                    spectraData                    : seq<SpectraData>,
                    searchDatabase                 : seq<SearchDatabase>,
                    ?id                            : string,
                    ?name                          : string,
                    ?activityDate                  : DateTime,
                    ?fkMzIdentML                   : string
                ) =
                let id'               = defaultArg id (System.Guid.NewGuid().ToString())
                let name'             = defaultArg name Unchecked.defaultof<string>
                let activityDate'     = defaultArg activityDate Unchecked.defaultof<DateTime>
                let fkMzIdentML'      = defaultArg fkMzIdentML Unchecked.defaultof<string>
                    
                new SpectrumIdentification(
                                           id', 
                                           name',
                                           Nullable(activityDate'), 
                                           spectrumIdentificationList, 
                                           spectrumIdentificationProtocol,
                                           spectraData |> List, 
                                           searchDatabase |> List, 
                                           fkMzIdentML', 
                                           Nullable(DateTime.Now)
                                          )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (spectrumIdentification:SpectrumIdentification) =
                spectrumIdentification.Name <- name
                spectrumIdentification

            ///Replaces activitydate of existing object with new name.
            static member addActivityDate
                (activityDate:DateTime) (spectrumIdentification:SpectrumIdentification) =
                spectrumIdentification.ActivityDate <- Nullable(activityDate)
                spectrumIdentification

            ///Replaces MzIdentMLDocumentID of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (spectrumIdentification:SpectrumIdentification) =
                let result = spectrumIdentification.MzIdentMLDocumentID <- fkMzIdentML
                spectrumIdentification

            ///Tries to find a spectrumIdentification-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SpectrumIdentification.Local do
                           if i.ID=id
                              then select (i, i.SpectrumIdentificationProtocol, i.SpectrumIdentificationList, 
                                           i.SpectraData, i.SearchDatabase, i.MzIdentMLDocumentID
                                          )
                      }
                |> Seq.map (fun (spectrumIdentification, _, _, _, _, _) -> spectrumIdentification)
                |> (fun spectrumIdentification -> 
                    if (Seq.exists (fun spectrumIdentification' -> spectrumIdentification' <> null) spectrumIdentification) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentification do
                                       if i.ID=id
                                          then select (i, i.SpectrumIdentificationProtocol, i.SpectrumIdentificationList, 
                                                       i.SpectraData, i.SearchDatabase, i.MzIdentMLDocumentID
                                                      )
                                  }
                            |> Seq.map (fun (spectrumIdentification, _, _, _, _, _) -> spectrumIdentification)
                            |> (fun spectrumIdentification -> if (Seq.exists (fun spectrumIdentification' -> spectrumIdentification' <> null) spectrumIdentification) = false
                                                                then None
                                                                else Some (spectrumIdentification.Single())
                               )
                        else Some (spectrumIdentification.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.SpectrumIdentification.Local do
                           if i.Name=name
                              then select (i, i.SpectrumIdentificationProtocol, i.SpectrumIdentificationList, 
                                           i.SpectraData, i.SearchDatabase, i.MzIdentMLDocumentID
                                          )
                      }
                |> Seq.map (fun (spectrumIdentification, _, _, _, _, _) -> spectrumIdentification)
                |> (fun spectrumIdentificationList -> 
                    if (Seq.exists (fun spectrumIdentificationList' -> spectrumIdentificationList' <> null) spectrumIdentificationList) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentification do
                                       if i.Name=name
                                          then select (i, i.SpectrumIdentificationProtocol, i.SpectrumIdentificationList, 
                                                       i.SpectraData, i.SearchDatabase, i.MzIdentMLDocumentID
                                                      )
                                  }
                            |> Seq.map (fun (spectrumIdentification, _, _, _, _, _) -> spectrumIdentification)
                            |> (fun spectrumIdentificationList -> if (Seq.exists (fun spectrumIdentificationList' -> spectrumIdentificationList' <> null) spectrumIdentificationList) = false
                                                                      then None
                                                                      else Some spectrumIdentificationList
                               )
                        else Some spectrumIdentificationList
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SpectrumIdentification) (item2:SpectrumIdentification) =
               item1.SpectrumIdentificationList=item2.SpectrumIdentificationList && 
               item1.MzIdentMLDocumentID=item2.MzIdentMLDocumentID && item1.Name=item2.Name &&
               item1.SpectraData=item2.SpectraData && item1.SearchDatabase=item2.SearchDatabase &&
               item1.ActivityDate=item2.ActivityDate

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SpectrumIdentification) =
                    SpectrumIdentificationHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match SpectrumIdentificationHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SpectrumIdentification) =
                SpectrumIdentificationHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinDetectionProtocolHandler =
            ///Initializes a proteindetectionprotocol-object with at least all necessary parameters.
            static member init
                (
                    analysisSoftware : AnalysisSoftware,
                    threshold        : seq<ThresholdParam>,
                    ?id              : string,
                    ?name            : string,
                    ?analysisParams  : seq<AnalysisParam>,
                    ?fkMzIdentML     : string
                ) =
                let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'           = defaultArg name Unchecked.defaultof<string>
                let analysisParams' = convertOptionToList analysisParams
                let fkMzIdentML'    = defaultArg fkMzIdentML Unchecked.defaultof<string>
                    
                new ProteinDetectionProtocol(
                                             id', 
                                             name', 
                                             analysisSoftware, 
                                             analysisParams', 
                                             threshold |> List,
                                             fkMzIdentML', 
                                             Nullable(DateTime.Now)
                                            )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (proteinDetectionProtocol:ProteinDetectionProtocol) =
                proteinDetectionProtocol.Name <- name
                proteinDetectionProtocol

            ///Adds a analysisparam to an existing proteindetectionprotocol-object.
            static member addAnalysisParam
                (analysisParam:AnalysisParam) (proteinDetectionProtocol:ProteinDetectionProtocol) =
                let result = proteinDetectionProtocol.AnalysisParams <- addToList proteinDetectionProtocol.AnalysisParams analysisParam
                proteinDetectionProtocol

            ///Adds a collection of analysisparams to an existing proteindetectionprotocol-object.
            static member addAnalysisParams
                (analysisParams:seq<AnalysisParam>) (proteinDetectionProtocol:ProteinDetectionProtocol) =
                let result = proteinDetectionProtocol.AnalysisParams <- addCollectionToList proteinDetectionProtocol.AnalysisParams analysisParams
                proteinDetectionProtocol

            ///Replaces MzIdentMLDocumentID of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (proteinDetectionProtocol:ProteinDetectionProtocol) =
                let result = proteinDetectionProtocol.MzIdentMLDocumentID <- fkMzIdentML
                proteinDetectionProtocol

            ///Tries to find a proteinDetectionProtocol-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.ProteinDetectionProtocol.Local do
                           if i.ID=id
                              then select (i, i.Threshold, i.AnalysisParams, i.MzIdentMLDocumentID)
                      }
                |> Seq.map (fun (proteinDetectionProtocol, _, _, _) -> proteinDetectionProtocol)
                |> (fun proteinDetectionProtocol -> 
                    if (Seq.exists (fun proteinDetectionProtocol' -> proteinDetectionProtocol' <> null) proteinDetectionProtocol) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionProtocol do
                                       if i.ID=id
                                          then select (i, i.Threshold, i.AnalysisParams, i.MzIdentMLDocumentID)
                                  }
                            |> Seq.map (fun (proteinDetectionProtocol, _, _, _) -> proteinDetectionProtocol)
                            |> (fun proteinDetectionProtocol -> if (Seq.exists (fun proteinDetectionProtocol' -> proteinDetectionProtocol' <> null) proteinDetectionProtocol) = false
                                                                then None
                                                                else Some (proteinDetectionProtocol.Single())
                               )
                        else Some (proteinDetectionProtocol.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.ProteinDetectionProtocol.Local do
                           if i.Name=name
                              then select (i, i.Threshold, i.AnalysisParams, i.MzIdentMLDocumentID)
                      }
                |> Seq.map (fun (proteinDetectionProtocol, _, _, _) -> proteinDetectionProtocol)
                |> (fun proteinDetectionProtocol -> 
                    if (Seq.exists (fun proteinDetectionProtocol' -> proteinDetectionProtocol' <> null) proteinDetectionProtocol) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionProtocol do
                                       if i.Name=name
                                          then select (i, i.Threshold, i.AnalysisParams, i.MzIdentMLDocumentID)
                                  }
                            |> Seq.map (fun (proteinDetectionProtocol, _, _, _) -> proteinDetectionProtocol)
                            |> (fun proteinDetectionProtocol -> if (Seq.exists (fun proteinDetectionProtocol' -> proteinDetectionProtocol' <> null) proteinDetectionProtocol) = false
                                                                      then None
                                                                      else Some proteinDetectionProtocol
                               )
                        else Some proteinDetectionProtocol
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinDetectionProtocol) (item2:ProteinDetectionProtocol) =
               item1.Threshold=item2.Threshold && item1.Name=item2.Name && item1.AnalysisSoftware=item2.AnalysisSoftware &&
               item1.AnalysisParams=item2.AnalysisParams && item1.MzIdentMLDocumentID=item2.MzIdentMLDocumentID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ProteinDetectionProtocol) =
                    ProteinDetectionProtocolHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinDetectionProtocolHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:ProteinDetectionProtocol) =
                ProteinDetectionProtocolHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type SourceFileHandler =
            ///Initializes a sourcefile-object with at least all necessary parameters.
            static member init
                (             
                    location                     : string,
                    fileFormat                   : CVParam,
                    ?id                          : string,
                    ?name                        : string,
                    ?externalFormatDocumentation : string,
                    ?details                     : seq<SourceFileParam>
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let details'                     = convertOptionToList details
                    
                new SourceFile(
                               id', 
                               name', 
                               location, 
                               externalFormatDocumentation', 
                               fileFormat, 
                               details', 
                               Nullable(DateTime.Now)
                              )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (sourceFile:SourceFile) =
                sourceFile.Name <- name
                sourceFile

            ///Replaces externalformatdocumentation of existing object with new name.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (sourceFile:SourceFile) =
                sourceFile.ExternalFormatDocumentation <- externalFormatDocumentation
                sourceFile

            ///Adds a sourcefileparam to an existing sourcefile-object.
            static member addDetail
                (detail:SourceFileParam) (sourceFile:SourceFile) =
                let result = sourceFile.Details <- addToList sourceFile.Details detail
                sourceFile

            ///Adds a collection of sourcefileparams to an existing sourcefile-object.
            static member addDetails
                (details:seq<SourceFileParam>) (sourceFile:SourceFile) =
                let result = sourceFile.Details <- addCollectionToList sourceFile.Details details
                sourceFile

            //static member addInputs
            //     (sourceFile:SourceFile) (inputs:Inputs) =
            //     sourceFile.Inputs <- inputs

            ///Tries to find a sourceFile-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SourceFile.Local do
                           if i.ID=id
                              then select (i, i.FileFormat, i.Details)
                      }
                |> Seq.map (fun (sourceFile, _, _) -> sourceFile)
                |> (fun sourceFile -> 
                    if (Seq.exists (fun sourceFile' -> sourceFile' <> null) sourceFile) = false
                        then 
                            query {
                                   for i in dbContext.SourceFile do
                                       if i.ID=id
                                          then select (i, i.FileFormat, i.Details)
                                  }
                            |> Seq.map (fun (sourceFile, _, _) -> sourceFile)
                            |> (fun sourceFile -> if (Seq.exists (fun sourceFile' -> sourceFile' <> null) sourceFile) = false
                                                    then None
                                                    else Some (sourceFile.Single())
                               )
                        else Some (sourceFile.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByLocation (dbContext:MzIdentML) (location:string) =
                query {
                       for i in dbContext.SourceFile.Local do
                           if i.Location=location
                              then select (i, i.FileFormat, i.Details)
                      }
                |> Seq.map (fun (sourceFile, _, _) -> sourceFile)
                |> (fun sourceFile -> 
                    if (Seq.exists (fun sourceFile' -> sourceFile' <> null) sourceFile) = false
                        then 
                            query {
                                   for i in dbContext.SourceFile do
                                       if i.Location=location
                                          then select (i, i.FileFormat, i.Details)
                                  }
                            |> Seq.map (fun (sourceFile, _, _) -> sourceFile)
                            |> (fun sourceFile -> if (Seq.exists (fun sourceFile' -> sourceFile' <> null) sourceFile) = false
                                                      then None
                                                      else Some sourceFile
                               )
                        else Some sourceFile
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:SourceFile) (item2:SourceFile) =
               item1.FileFormat=item2.FileFormat && item1.Name=item2.Name && matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) &&
               item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SourceFile) =
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:SourceFile) =
                SourceFileHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type InputsHandler =
            ///Initializes an inputs-object with at least all necessary parameters.
            static member init
                (              
                    spectraData     : seq<SpectraData>,
                    ?id             : string,
                    ?sourceFile     : seq<SourceFile>,
                    ?searchDatabase : seq<SearchDatabase>
                ) =
                let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                let sourceFile'     = convertOptionToList sourceFile
                let searchDatabase' = convertOptionToList searchDatabase
                    
                new Inputs(
                           id', 
                           sourceFile', 
                           searchDatabase', 
                           spectraData |> List,  
                           Nullable(DateTime.Now)
                          )

            ///Adds a sourcefile to an existing inputs-object.
            static member addSourceFile
                (sourceFile:SourceFile) (inputs:Inputs) =
                let result = inputs.SourceFiles <- addToList inputs.SourceFiles sourceFile
                inputs

            ///Adds a collection of sourcefiles to an existing inputs-object.
            static member addSourceFiles
                (sourceFiles:seq<SourceFile>) (inputs:Inputs) =
                let result = inputs.SourceFiles <- addCollectionToList inputs.SourceFiles sourceFiles
                inputs

            ///Adds a searchdatabase to an existing inputs-object.
            static member addSearchDatabase
                (searchDatabase:SearchDatabase) (inputs:Inputs) =
                let result = inputs.SearchDatabases <- addToList inputs.SearchDatabases searchDatabase
                inputs

            ///Adds a collection of searchdatabases to an existing inputs-object.
            static member addSearchDatabases
                (searchDatabases:seq<SearchDatabase>) (inputs:Inputs) =
                let result = inputs.SearchDatabases <- addCollectionToList inputs.SearchDatabases searchDatabases
                inputs

            ///Tries to find a inputs-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.Inputs.Local do
                           if i.ID=id
                              then select (i, i.SourceFiles, i.SpectraData, i.SearchDatabases)
                      }
                |> Seq.map (fun (inputs, _, _, _) -> inputs)
                |> (fun inputs -> 
                    if (Seq.exists (fun inputs' -> inputs' <> null) inputs) = false
                        then 
                            query {
                                   for i in dbContext.Inputs do
                                       if i.ID=id
                                          then select (i, i.SourceFiles, i.SpectraData, i.SearchDatabases)
                                  }
                            |> Seq.map (fun (inputs, _, _, _) -> inputs)
                            |> (fun inputs -> if (Seq.exists (fun inputs' -> inputs' <> null) inputs) = false
                                                then None
                                                else Some (inputs.Single())
                               )
                        else Some (inputs.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindBySpectraData (dbContext:MzIdentML) (spectraData:seq<SpectraData>) =
                query {
                       for i in dbContext.Inputs.Local do
                           if i.SpectraData=(spectraData |> List)
                              then select (i, i.SourceFiles, i.SpectraData, i.SearchDatabases)
                      }
                |> Seq.map (fun (inputs, _, _, _) -> inputs)
                |> (fun inputs -> 
                    if (Seq.exists (fun inputs' -> inputs' <> null) inputs) = false
                        then 
                            query {
                                   for i in dbContext.Inputs do
                                       if i.SpectraData=(spectraData |> List)
                                          then select (i, i.SourceFiles, i.SpectraData, i.SearchDatabases)
                                  }
                            |> Seq.map (fun (inputs, _, _, _) -> inputs)
                            |> (fun inputs -> if (Seq.exists (fun inputs' -> inputs' <> null) inputs) = false
                                                  then None
                                                  else Some inputs
                               )
                        else Some inputs
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:Inputs) (item2:Inputs) =
               item1.SourceFiles=item2.SourceFiles && item1.SearchDatabases=item2.SearchDatabases

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Inputs) =
                    InputsHandler.tryFindBySpectraData dbContext item.SpectraData
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match InputsHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Inputs) =
                InputsHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type PeptideHypothesisHandler =
            ///Initializes a peptidehypothesis-object with at least all necessary parameters.
            static member init
                (              
                    peptideEvidence             : PeptideEvidence,
                    spectrumIdentificationItems : seq<SpectrumIdentificationItem>,
                    ?id                         : string
                ) =
                let id' = defaultArg id (System.Guid.NewGuid().ToString())
                    
                new PeptideHypothesis(
                                      id', 
                                      peptideEvidence, 
                                      spectrumIdentificationItems |> List, 
                                      Nullable(DateTime.Now)
                                     )

            ///Tries to find a peptideHypothesis-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.PeptideHypothesis.Local do
                           if i.ID=id
                              then select (i, i.PeptideEvidence, i.SpectrumIdentificationItems)
                      }
                |> Seq.map (fun (peptideHypothesis, _, _) -> peptideHypothesis)
                |> (fun peptideHypothesis -> 
                    if (Seq.exists (fun peptideHypothesis' -> peptideHypothesis' <> null) peptideHypothesis) = false
                        then 
                            query {
                                   for i in dbContext.PeptideHypothesis do
                                       if i.ID=id
                                          then select (i, i.PeptideEvidence, i.SpectrumIdentificationItems)
                                  }
                            |> Seq.map (fun (peptideHypothesis, _, _) -> peptideHypothesis)
                            |> (fun peptideHypothesis -> if (Seq.exists (fun peptideHypothesis' -> peptideHypothesis' <> null) peptideHypothesis) = false
                                                            then None
                                                            else Some (peptideHypothesis.Single())
                               )
                        else Some (peptideHypothesis.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByPeptideEvidencePeptideSequence (dbContext:MzIdentML) (peptideSequence:string) =
                query {
                       for i in dbContext.PeptideHypothesis.Local do
                           if i.PeptideEvidence.Peptide.PeptideSequence=peptideSequence
                              then select (i, i.PeptideEvidence, i.SpectrumIdentificationItems)
                      }
                |> Seq.map (fun (peptideHypothesis, _, _) -> peptideHypothesis)
                |> (fun peptideHypothesis -> 
                    if (Seq.exists (fun peptideHypothesis' -> peptideHypothesis' <> null) peptideHypothesis) = false
                        then 
                            query {
                                   for i in dbContext.PeptideHypothesis do
                                       if i.PeptideEvidence.Peptide.PeptideSequence=peptideSequence
                                          then select (i, i.PeptideEvidence, i.SpectrumIdentificationItems)
                                  }
                            |> Seq.map (fun (peptideHypothesis, _, _) -> peptideHypothesis)
                            |> (fun peptideHypothesis -> if (Seq.exists (fun peptideHypothesis' -> peptideHypothesis' <> null) peptideHypothesis) = false
                                                             then None
                                                             else Some peptideHypothesis
                               )
                        else Some peptideHypothesis
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:PeptideHypothesis) (item2:PeptideHypothesis) =
               item1.SpectrumIdentificationItems=item2.SpectrumIdentificationItems

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:PeptideHypothesis) =
                    PeptideHypothesisHandler.tryFindByPeptideEvidencePeptideSequence dbContext item.PeptideEvidence.Peptide.PeptideSequence
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match PeptideHypothesisHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:PeptideHypothesis) =
                PeptideHypothesisHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinDetectionHypothesisHandler =
            ///Initializes a proteindetectionhypothesis-object with at least all necessary parameters.
            static member init
                (             
                    passThreshold     : bool,
                    dbSequence        : DBSequence,
                    peptideHypothesis : seq<PeptideHypothesis>,
                    ?id               : string,
                    ?name             : string,
                    ?details          : seq<ProteinDetectionHypothesisParam>
                ) =
                let id'        = defaultArg id (System.Guid.NewGuid().ToString())
                let name'        = defaultArg name Unchecked.defaultof<string>
                let details'     = convertOptionToList details
                    
                new ProteinDetectionHypothesis(
                                               id', 
                                               name', 
                                               Nullable(passThreshold), 
                                               dbSequence, 
                                               peptideHypothesis |> List,
                                               details', 
                                               Nullable(DateTime.Now)
                                              )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (proteinDetectionHypothesis:ProteinDetectionHypothesis) =
                proteinDetectionHypothesis.Name <- name
                proteinDetectionHypothesis

            ///Adds a proteindetectionhypothesisparam to an existing proteindetectionhypothesis-object.
            static member addDetail
                (detail:ProteinDetectionHypothesisParam) (proteinDetectionHypothesis:ProteinDetectionHypothesis) =
                let result = proteinDetectionHypothesis.Details <- addToList proteinDetectionHypothesis.Details detail
                proteinDetectionHypothesis

            ///Adds a collection of proteindetectionhypothesisparams to an existing proteindetectionhypothesis-object.
            static member addDetails
                (details:seq<ProteinDetectionHypothesisParam>) (proteinDetectionHypothesis:ProteinDetectionHypothesis) =
                let result = proteinDetectionHypothesis.Details <- addCollectionToList proteinDetectionHypothesis.Details details
                proteinDetectionHypothesis

            ///Tries to find a proteinDetectionHypothesis-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.ProteinDetectionHypothesis.Local do
                           if i.ID=id
                              then select (i, i.PeptideHypothesis, i.Details)
                      }
                |> Seq.map (fun (proteinDetectionHypothesis, _, _) -> proteinDetectionHypothesis)
                |> (fun proteinDetectionHypothesis -> 
                    if (Seq.exists (fun proteinDetectionHypothesis' -> proteinDetectionHypothesis' <> null) proteinDetectionHypothesis) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionHypothesis do
                                       if i.ID=id
                                          then select (i, i.PeptideHypothesis, i.Details)
                                  }
                            |> Seq.map (fun (proteinDetectionHypothesis, _, _) -> proteinDetectionHypothesis)
                            |> (fun proteinDetectionHypothesis -> if (Seq.exists (fun proteinDetectionHypothesis' -> proteinDetectionHypothesis' <> null) proteinDetectionHypothesis) = false
                                                                    then None
                                                                    else Some (proteinDetectionHypothesis.Single())
                               )
                        else Some (proteinDetectionHypothesis.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByPassThreshold (dbContext:MzIdentML) (passThreshold:Nullable<bool>) =
                query {
                       for i in dbContext.ProteinDetectionHypothesis.Local do
                           if i.PassThreshold=passThreshold
                              then select (i, i.PeptideHypothesis, i.Details)
                      }
                |> Seq.map (fun (proteinDetectionHypothesis, _, _) -> proteinDetectionHypothesis)
                |> (fun proteinDetectionHypothesis -> 
                    if (Seq.exists (fun proteinDetectionHypothesis' -> proteinDetectionHypothesis' <> null) proteinDetectionHypothesis) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionHypothesis do
                                       if i.PassThreshold=passThreshold
                                          then select (i, i.PeptideHypothesis, i.Details)
                                  }
                            |> Seq.map (fun (proteinDetectionHypothesis, _, _) -> proteinDetectionHypothesis)
                            |> (fun proteinDetectionHypothesis -> if (Seq.exists (fun proteinDetectionHypothesis' -> proteinDetectionHypothesis' <> null) proteinDetectionHypothesis) = false
                                                                      then None
                                                                      else Some proteinDetectionHypothesis
                               )
                        else Some proteinDetectionHypothesis
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinDetectionHypothesis) (item2:ProteinDetectionHypothesis) =
               item1.PassThreshold=item2.PassThreshold && item1.PeptideHypothesis=item2.PeptideHypothesis &&
               item1.Name=item2.Name && matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ProteinDetectionHypothesis) =
                    ProteinDetectionHypothesisHandler.tryFindByPassThreshold dbContext item.PassThreshold
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinDetectionHypothesisHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:ProteinDetectionHypothesis) =
                ProteinDetectionHypothesisHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinAmbiguityGroupHandler =
            ///Initializes a proteinambiguitygroup-object with at least all necessary parameters.
            static member init
                (             
                    proteinDetecionHypothesis : seq<ProteinDetectionHypothesis>,
                    ?id                       : string,
                    ?name                     : string,
                    ?details                  : seq<ProteinAmbiguityGroupParam>
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let details'                     = convertOptionToList details
                    
                new ProteinAmbiguityGroup(
                                          id', 
                                          name', 
                                          proteinDetecionHypothesis |> List, 
                                          details', 
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (proteinAmbiguityGroup:ProteinAmbiguityGroup) =
                proteinAmbiguityGroup.Name <- name
                proteinAmbiguityGroup

            ///Adds a proteinambiguitygroupparam to an existing proteinambiguitygroup-object.
            static member addDetail
                (detail:ProteinAmbiguityGroupParam) (proteinAmbiguityGroup:ProteinAmbiguityGroup) =
                let result = proteinAmbiguityGroup.Details <- addToList proteinAmbiguityGroup.Details detail
                proteinAmbiguityGroup

            ///Adds a collection of proteinambiguitygroupparam to an existing proteinambiguitygroupparam-object.
            static member addDetails
                (details:seq<ProteinAmbiguityGroupParam>) (proteinAmbiguityGroup:ProteinAmbiguityGroup) =
                let result = proteinAmbiguityGroup.Details <- addCollectionToList proteinAmbiguityGroup.Details details
                proteinAmbiguityGroup

            ///Tries to find a proteinAmbiguityGroup-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.ProteinAmbiguityGroup.Local do
                           if i.ID=id
                              then select (i, i.ProteinDetectionHypothesis, i.Details)
                      }
                |> Seq.map (fun (proteinAmbiguityGroup, _, _) -> proteinAmbiguityGroup)
                |> (fun proteinAmbiguityGroup -> 
                    if (Seq.exists (fun proteinAmbiguityGroup' -> proteinAmbiguityGroup' <> null) proteinAmbiguityGroup) = false
                        then 
                            query {
                                   for i in dbContext.ProteinAmbiguityGroup do
                                       if i.ID=id
                                          then select (i, i.ProteinDetectionHypothesis, i.Details)
                                  }
                            |> Seq.map (fun (proteinAmbiguityGroup, _, _) -> proteinAmbiguityGroup)
                            |> (fun proteinAmbiguityGroup -> if (Seq.exists (fun proteinAmbiguityGroup' -> proteinAmbiguityGroup' <> null) proteinAmbiguityGroup) = false
                                                                then None
                                                                else Some (proteinAmbiguityGroup.Single())
                               )
                        else Some (proteinAmbiguityGroup.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.ProteinAmbiguityGroup.Local do
                           if i.Name=name
                              then select (i, i.ProteinDetectionHypothesis, i.Details)
                      }
                |> Seq.map (fun (proteinAmbiguityGroup, _, _) -> proteinAmbiguityGroup)
                |> (fun proteinAmbiguityGroup -> 
                    if (Seq.exists (fun proteinAmbiguityGroup' -> proteinAmbiguityGroup' <> null) proteinAmbiguityGroup) = false
                        then 
                            query {
                                   for i in dbContext.ProteinAmbiguityGroup do
                                       if i.Name=name
                                          then select (i, i.ProteinDetectionHypothesis, i.Details)
                                  }
                            |> Seq.map (fun (proteinAmbiguityGroup, _, _) -> proteinAmbiguityGroup)
                            |> (fun proteinAmbiguityGroup -> if (Seq.exists (fun proteinAmbiguityGroup' -> proteinAmbiguityGroup' <> null) proteinAmbiguityGroup) = false
                                                                      then None
                                                                      else Some proteinAmbiguityGroup
                               )
                        else Some proteinAmbiguityGroup
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinAmbiguityGroup) (item2:ProteinAmbiguityGroup) =
               item1.ProteinDetectionHypothesis=item2.ProteinDetectionHypothesis && matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ProteinAmbiguityGroup) =
                    ProteinAmbiguityGroupHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinAmbiguityGroupHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:ProteinAmbiguityGroup) =
                ProteinAmbiguityGroupHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinDetectionListHandler =
            ///Initializes a proteindetectionlist-object with at least all necessary parameters.
            static member init
                (             
                    ?id                     : string,
                    ?name                   : string,
                    ?proteinAmbiguityGroups : seq<ProteinAmbiguityGroup>,
                    ?details                : seq<ProteinDetectionListParam>
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                   = defaultArg name Unchecked.defaultof<string>
                let proteinAmbiguityGroups' = convertOptionToList proteinAmbiguityGroups
                let details'                = convertOptionToList details
                    
                new ProteinDetectionList(
                                         id', 
                                         name', 
                                         proteinAmbiguityGroups', 
                                         details', 
                                         Nullable(DateTime.Now)
                                        )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (proteinDetectionList:ProteinDetectionList) =
                proteinDetectionList.Name <- name
                proteinDetectionList

            ///Adds a proteinambiguitygroup to an existing proteindetectionlist-object.
            static member addProteinAmbiguityGroup
                (proteinAmbiguityGroup:ProteinAmbiguityGroup) (proteinDetectionList:ProteinDetectionList) =
                let result = proteinDetectionList.ProteinAmbiguityGroups <- addToList proteinDetectionList.ProteinAmbiguityGroups proteinAmbiguityGroup
                proteinDetectionList

            ///Adds a collection of proteinambiguitygroups to an existing proteindetectionlist-object.
            static member addProteinAmbiguityGroups
                (proteinAmbiguityGroups:seq<ProteinAmbiguityGroup>) (proteinDetectionList:ProteinDetectionList) =
                let result = proteinDetectionList.ProteinAmbiguityGroups <- addCollectionToList proteinDetectionList.ProteinAmbiguityGroups proteinAmbiguityGroups
                proteinDetectionList

            ///Adds a proteindetectionlistparam to an existing proteindetectionlist-object.
            static member addDetail
                (detail:ProteinDetectionListParam) (proteinDetectionList:ProteinDetectionList) =
                let result = proteinDetectionList.Details <- addToList proteinDetectionList.Details detail
                proteinDetectionList

            ///Adds a collection of proteindetectionlistparams to an existing proteindetectionlist-object.
            static member addDetails
                (details:seq<ProteinDetectionListParam>) (proteinDetectionList:ProteinDetectionList) =
                let result = proteinDetectionList.Details <- addCollectionToList proteinDetectionList.Details details
                proteinDetectionList

            ///Tries to find a proteinDetectionList-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.ProteinDetectionList.Local do
                           if i.ID=id
                              then select (i, i.ProteinAmbiguityGroups, i.Details)
                      }
                |> Seq.map (fun (proteinDetectionList, _, _) -> proteinDetectionList)
                |> (fun proteinDetectionList -> 
                    if (Seq.exists (fun proteinDetectionList' -> proteinDetectionList' <> null) proteinDetectionList) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionList do
                                       if i.ID=id
                                          then select (i, i.ProteinAmbiguityGroups, i.Details)
                                  }
                            |> Seq.map (fun (proteinDetectionList, _, _) -> proteinDetectionList)
                            |> (fun proteinDetectionList -> if (Seq.exists (fun proteinDetectionList' -> proteinDetectionList' <> null) proteinDetectionList) = false
                                                            then None
                                                            else Some (proteinDetectionList.Single())
                               )
                        else Some (proteinDetectionList.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.ProteinDetectionList.Local do
                           if i.Name=name
                              then select (i, i.ProteinAmbiguityGroups, i.Details)
                      }
                |> Seq.map (fun (proteinDetectionList, _, _) -> proteinDetectionList)
                |> (fun proteinDetectionList -> 
                    if (Seq.exists (fun proteinDetectionList' -> proteinDetectionList' <> null) proteinDetectionList) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionList do
                                       if i.Name=name
                                          then select (i, i.ProteinAmbiguityGroups, i.Details)
                                  }
                            |> Seq.map (fun (proteinDetectionList, _, _) -> proteinDetectionList)
                            |> (fun proteinDetectionList -> if (Seq.exists (fun proteinDetectionList' -> proteinDetectionList' <> null) proteinDetectionList) = false
                                                                      then None
                                                                      else Some proteinDetectionList
                               )
                        else Some proteinDetectionList
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinDetectionList) (item2:ProteinDetectionList) =
               item1.ProteinAmbiguityGroups=item2.ProteinAmbiguityGroups && matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ProteinDetectionList) =
                    ProteinDetectionListHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinDetectionListHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:ProteinDetectionList) =
                ProteinDetectionListHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type AnalysisDataHandler =
            ///Initializes a analysisdata-object with at least all necessary parameters.
            static member init
                (             
                    spectrumIdentificationList : seq<SpectrumIdentificationList>,
                    ?id                        : string,
                    ?proteinDetectionList      : ProteinDetectionList
                ) =
                let id'                   = defaultArg id (System.Guid.NewGuid().ToString())
                let proteinDetectionList' = defaultArg proteinDetectionList Unchecked.defaultof<ProteinDetectionList>
                    
                new AnalysisData(
                                 id', 
                                 spectrumIdentificationList |> List, 
                                 proteinDetectionList',  
                                 Nullable(DateTime.Now)
                                )

            ///Replaces proteindetectionlist of existing object with new one.
            static member addProteinDetectionList
                (proteinDetectionList:ProteinDetectionList) (analysisData:AnalysisData) =
                analysisData.ProteinDetectionList <- proteinDetectionList
                analysisData

            ///Tries to find a analysisData-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.AnalysisData.Local do
                           if i.ID=id
                              then select (i, i.SpectrumIdentificationList, i.ProteinDetectionList)
                      }
                |> Seq.map (fun (analysisData, _, _) -> analysisData)
                |> (fun analysisData -> 
                    if (Seq.exists (fun analysisData' -> analysisData' <> null) analysisData) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisData do
                                       if i.ID=id
                                          then select (i, i.SpectrumIdentificationList, i.ProteinDetectionList)
                                  }
                            |> Seq.map (fun (analysisData, _, _) -> analysisData)
                            |> (fun analysisData -> if (Seq.exists (fun analysisData' -> analysisData' <> null) analysisData) = false
                                                    then None
                                                    else Some (analysisData.Single())
                               )
                        else Some (analysisData.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByMzIdentMLDocument (dbContext:MzIdentML) (proteinDetectionListID:string) =
                query {
                       for i in dbContext.AnalysisData.Local do
                           if i.ProteinDetectionList.ID=proteinDetectionListID
                              then select (i, i.SpectrumIdentificationList, i.ProteinDetectionList)
                      }
                |> Seq.map (fun (analysisData, _, _) -> analysisData)
                |> (fun analysisData -> 
                    if (Seq.exists (fun analysisData' -> analysisData' <> null) analysisData) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisData do
                                       if i.ProteinDetectionList.ID=proteinDetectionListID
                                          then select (i, i.SpectrumIdentificationList, i.ProteinDetectionList)
                                  }
                            |> Seq.map (fun (analysisData, _, _) -> analysisData)
                            |> (fun analysisData -> if (Seq.exists (fun analysisData' -> analysisData' <> null) analysisData) = false
                                                        then None
                                                        else Some analysisData
                               )
                        else Some analysisData
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:AnalysisData) (item2:AnalysisData) =
               item1.SpectrumIdentificationList=item2.SpectrumIdentificationList && 
               item1.ProteinDetectionList=item2.ProteinDetectionList

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:AnalysisData) =
                    AnalysisDataHandler.tryFindByMzIdentMLDocument dbContext item.ProteinDetectionList.ID
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match AnalysisDataHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:AnalysisData) =
                AnalysisDataHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinDetectionHandler =
            ///Initializes a proteindetection-object with at least all necessary parameters.
            static member init
                (             
                    proteinDetectionList        : ProteinDetectionList,
                    proteinDetectionProtocol    : ProteinDetectionProtocol,
                    spectrumIdentificationLists : seq<SpectrumIdentificationList>,
                    ?id                         : string,
                    ?name                       : string,
                    ?activityDate               : DateTime
                ) =
                let id'                = defaultArg id (System.Guid.NewGuid().ToString())
                let name'              = defaultArg name Unchecked.defaultof<string>
                let activityDate'      = defaultArg activityDate Unchecked.defaultof<DateTime>
                    
                new ProteinDetection(
                                     id', 
                                     name', 
                                     Nullable(activityDate'), 
                                     proteinDetectionList, 
                                     proteinDetectionProtocol,
                                     spectrumIdentificationLists |> List,
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (proteinDetection:ProteinDetection) =
                proteinDetection.Name <- name
                proteinDetection

            ///Replaces activitydate of existing object with new name.
            static member addActivityDate
                (activityDate:DateTime) (proteinDetection:ProteinDetection) =
                proteinDetection.ActivityDate <- Nullable(activityDate)
                proteinDetection

            ///Tries to find a proteinDetection-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.ProteinDetection.Local do
                           if i.ID=id
                              then select (i, i.ProteinDetectionProtocol, i.ProteinDetectionList, i.SpectrumIdentificationLists)
                      }
                |> Seq.map (fun (proteinDetection, _, _, _) -> proteinDetection)
                |> (fun proteinDetection -> 
                    if (Seq.exists (fun proteinDetection' -> proteinDetection' <> null) proteinDetection) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetection do
                                       if i.ID=id
                                          then select (i, i.ProteinDetectionProtocol, i.ProteinDetectionList, i.SpectrumIdentificationLists)
                                  }
                            |> Seq.map (fun (proteinDetection, _, _, _) -> proteinDetection)
                            |> (fun proteinDetection -> if (Seq.exists (fun proteinDetection' -> proteinDetection' <> null) proteinDetection) = false
                                                        then None
                                                        else Some (proteinDetection.Single())
                               )
                        else Some (proteinDetection.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.ProteinDetection.Local do
                           if i.Name=name
                              then select (i, i.ProteinDetectionProtocol, i.ProteinDetectionList, i.SpectrumIdentificationLists)
                      }
                |> Seq.map (fun (proteinDetection, _, _, _) -> proteinDetection)
                |> (fun proteinDetection -> 
                    if (Seq.exists (fun proteinDetection' -> proteinDetection' <> null) proteinDetection) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetection do
                                       if i.Name=name
                                          then select (i, i.ProteinDetectionProtocol, i.ProteinDetectionList, i.SpectrumIdentificationLists)
                                  }
                            |> Seq.map (fun (proteinDetection, _, _, _) -> proteinDetection)
                            |> (fun proteinDetection -> if (Seq.exists (fun proteinDetection' -> proteinDetection' <> null) proteinDetection) = false
                                                        then None
                                                        else Some proteinDetection
                               )
                        else Some proteinDetection
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ProteinDetection) (item2:ProteinDetection) =
               item1.ProteinDetectionProtocol=item2.ProteinDetectionProtocol && item1.SpectrumIdentificationLists=item2.SpectrumIdentificationLists &&
               item1.Name=item2.Name && item1.ActivityDate=item2.ActivityDate

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ProteinDetection) =
                    ProteinDetectionHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match ProteinDetectionHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:ProteinDetection) =
                ProteinDetectionHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type BiblioGraphicReferenceHandler =
            ///Initializes a bibliographicreference-object with at least all necessary parameters.
            static member init
                (             
                    ?id                  : string,
                    ?name                : string,
                    ?authors             : string,
                    ?doi                 : string,
                    ?editor              : string,
                    ?issue               : string,
                    ?pages               : string,
                    ?publication         : string,
                    ?publisher           : string,
                    ?title               : string,
                    ?volume              : string,
                    ?year                : int,
                    ?fkMzIdentMLDocument : string
                ) =
                let id'                  = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                = defaultArg name Unchecked.defaultof<string>
                let authors'             = defaultArg authors Unchecked.defaultof<string>
                let doi'                 = defaultArg doi Unchecked.defaultof<string>
                let editor'              = defaultArg editor Unchecked.defaultof<string>
                let issue'               = defaultArg issue Unchecked.defaultof<string>
                let pages'               = defaultArg pages Unchecked.defaultof<string>
                let publication'         = defaultArg publication Unchecked.defaultof<string>
                let publisher'           = defaultArg publisher Unchecked.defaultof<string>
                let title'               = defaultArg title Unchecked.defaultof<string>
                let volume'              = defaultArg volume Unchecked.defaultof<string>
                let year'                = defaultArg year Unchecked.defaultof<int>
                let fkMzIdentMLDocument' = defaultArg fkMzIdentMLDocument Unchecked.defaultof<string>
                    
                new BiblioGraphicReference(
                                           id', 
                                           name', 
                                           authors', 
                                           doi', editor', 
                                           issue', 
                                           pages', 
                                           publication',
                                           publisher', 
                                           title', 
                                           volume', 
                                           Nullable(year'), 
                                           fkMzIdentMLDocument', 
                                           Nullable(DateTime.Now)
                                          )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Name <- name
                biblioGraphicReference

            ///Replaces authors of existing object with new authors.
            static member addAuthors
                (authors:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Authors <- authors
                biblioGraphicReference

            ///Replaces doi of existing object with new doi.
            static member addDOI
                (doi:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.DOI <- doi
                biblioGraphicReference

            ///Replaces editor of existing object with new editor.
            static member addEditor
                (editor:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Editor <- editor
                biblioGraphicReference

            ///Replaces issue of existing object with new issue.
            static member addIssue
                (issue:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Issue <- issue
                biblioGraphicReference

            ///Replaces pages of existing object with new pages.
            static member addPages
                (pages:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Pages <- pages
                biblioGraphicReference

            ///Replaces publication of existing object with new publication.
            static member addPublication
                (publication:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Publication <- publication
                biblioGraphicReference

            ///Replaces publisher of existing object with new publisher.
            static member addPublisher
                (publisher:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Publisher <- publisher
                biblioGraphicReference

            ///Replaces title of existing object with new title.
            static member addTitle
                (title:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Title <- title
                biblioGraphicReference

            ///Replaces volume of existing object with new volume.
            static member addVolume
                (volume:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Volume <- volume
                biblioGraphicReference

            ///Replaces year of existing object with new year.
            static member addYear
                (year:int) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Year <- Nullable(year)
                biblioGraphicReference

            ///Replaces MzIdentMLDocumentID of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (biblioGraphicReference:BiblioGraphicReference) =
                let result = biblioGraphicReference.MzIdentMLDocumentID <- fkMzIdentML
                biblioGraphicReference

            ///Tries to find a biblioGraphicReference-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.BiblioGraphicReference.Local do
                           if i.ID=id
                              then select (i, i.MzIdentMLDocumentID)
                      }
                |> Seq.map (fun (biblioGraphicReference, _) -> biblioGraphicReference)
                |> (fun biblioGraphicReference -> 
                    if (Seq.exists (fun biblioGraphicReference' -> biblioGraphicReference' <> null) biblioGraphicReference) = false
                        then 
                            query {
                                   for i in dbContext.BiblioGraphicReference do
                                       if i.ID=id
                                          then select (i, i.MzIdentMLDocumentID)
                                  }
                            |> Seq.map (fun (biblioGraphicReference, _) -> biblioGraphicReference)
                            |> (fun biblioGraphicReference -> if (Seq.exists (fun biblioGraphicReference' -> biblioGraphicReference' <> null) biblioGraphicReference) = false
                                                                then None
                                                                else Some (biblioGraphicReference.Single())
                               )
                        else Some (biblioGraphicReference.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.BiblioGraphicReference.Local do
                           if i.Name=name
                              then select (i, i.MzIdentMLDocumentID)
                      }
                |> Seq.map (fun (biblioGraphicReference, _) -> biblioGraphicReference)
                |> (fun biblioGraphicReference -> 
                    if (Seq.exists (fun biblioGraphicReference' -> biblioGraphicReference' <> null) biblioGraphicReference) = false
                        then 
                            query {
                                   for i in dbContext.BiblioGraphicReference do
                                       if i.Name=name
                                          then select (i, i.MzIdentMLDocumentID)
                                  }
                            |> Seq.map (fun (biblioGraphicReference, _) -> biblioGraphicReference)
                            |> (fun biblioGraphicReference -> if (Seq.exists (fun biblioGraphicReference' -> biblioGraphicReference' <> null) biblioGraphicReference) = false
                                                                  then None
                                                                  else Some biblioGraphicReference
                               )
                        else Some biblioGraphicReference
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:BiblioGraphicReference) (item2:BiblioGraphicReference) =
               item1.Authors=item2.Authors && item1.DOI=item2.DOI && item1.Editor=item2.Editor && item1.Issue=item2.Issue &&
               item1.Pages=item2.Pages && item1.Publication=item2.Publication && item1.Publisher=item2.Publisher && item1.Title=item2.Title &&
               item1.Volume=item2.Volume && item1.Year=item2.Year && item1.MzIdentMLDocumentID=item2.MzIdentMLDocumentID

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:BiblioGraphicReference) =
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:BiblioGraphicReference) =
                BiblioGraphicReferenceHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type ProviderHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    ?id               : string,
                    ?name             : string,
                    ?analysisSoftware : AnalysisSoftware,
                    ?contactRole      : ContactRole
                ) =
                let id'               = defaultArg id (System.Guid.NewGuid().ToString())
                let name'             = defaultArg name Unchecked.defaultof<string>
                let analysisSoftware' = defaultArg analysisSoftware Unchecked.defaultof<AnalysisSoftware>
                let contactRole'      = defaultArg contactRole Unchecked.defaultof<ContactRole>

                new Provider(
                             id', 
                             name', 
                             analysisSoftware', 
                             contactRole', 
                             Nullable(DateTime.Now)
                            )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (provider:Provider) =
                provider.Name <- name
                provider

            ///Replaces analysissoftware of existing object with new analysissoftware.
            static member addAnalysisSoftware
                (analysisSoftware:AnalysisSoftware) (provider:Provider) =
                provider.AnalysisSoftware <- analysisSoftware
                provider

            ///Replaces contactrole of existing object with new contactrole.
            static member addContactRole
                (contactRole:ContactRole) (provider:Provider) =
                provider.ContactRole <- contactRole
                provider

            ///Tries to find a provider-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.Provider.Local do
                           if i.ID=id
                              then select (i, i.AnalysisSoftware, i.ContactRole)
                      }
                |> Seq.map (fun (provider, _, _) -> provider)
                |> (fun provider -> 
                    if (Seq.exists (fun provider' -> provider' <> null) provider) = false
                        then 
                            query {
                                   for i in dbContext.Provider do
                                       if i.ID=id
                                          then select (i, i.AnalysisSoftware, i.ContactRole)
                                  }
                            |> Seq.map (fun (provider, _, _) -> provider)
                            |> (fun provider -> if (Seq.exists (fun provider' -> provider' <> null) provider) = false
                                                then None
                                                else Some (provider.Single())
                               )
                        else Some (provider.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.Provider.Local do
                           if i.Name=name
                              then select (i, i.AnalysisSoftware, i.ContactRole)
                      }
                |> Seq.map (fun (provider, _, _) -> provider)
                |> (fun provider -> 
                    if (Seq.exists (fun provider' -> provider' <> null) provider) = false
                        then 
                            query {
                                   for i in dbContext.Provider do
                                       if i.Name=name
                                          then select (i, i.AnalysisSoftware, i.ContactRole)
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

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Provider) =
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:Provider) =
                ProviderHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()

        type AuditCollectionHandler =
        ///Initializes a term-object with at least all necessary parameters.
            static member init
                (
                    persons              : seq<Person>,
                    organisations        : seq<Organization>,
                    ?id                  : string,
                    ?fkMzIdentMLDocument : string
                ) =
                let id'                     = defaultArg id Unchecked.defaultof<string>
                let fkMzIdentMLDocument'    = defaultArg fkMzIdentMLDocument Unchecked.defaultof<string>

                new AuditCollection(
                                    id', 
                                    persons |> List,
                                    organisations |> List,
                                    fkMzIdentMLDocument', 
                                    (Nullable(DateTime.Now))
                                   )

            ///Adds new person to collection of persons.
            static member addPerson
                (person:Person) (table:AuditCollection) =
                table.Persons <- addToList table.Persons person
                table

            ///Add new collection of persons to collection of persons.
            static member addPersons
                (persons:seq<Person>) (table:AuditCollection) =
                table.Persons <- addCollectionToList table.Persons persons
                table

            ///Adds new organization to collection of organizations.
            static member addOrganization
                (organization:Organization) (table:AuditCollection) =
                table.Organizations <- addToList table.Organizations organization
                table

            ///Add new collection of organizations to collection of organizations.
            static member addOrganizations
                (organizations:seq<Organization>) (table:AuditCollection) =
                table.Organizations <- addCollectionToList table.Organizations organizations
                table
    
            ///Replaces fkMzIdentMLDocument of existing object with new one.
            static member addMzIdentMLDocumentID
                (fkMzIdentMLDocument:string) (table:AuditCollection) =
                table.MzIdentMLDocumentID <- fkMzIdentMLDocument
                table

            ///Tries to find a auditCollection-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                        for i in dbContext.AuditCollection.Local do
                            if i.ID=id
                                then select (i, i.Persons, i.Organizations)
                        }
                |> Seq.map (fun (auditCollection,_, _) -> auditCollection)
                |> (fun auditCollection -> 
                    if (Seq.exists (fun auditCollection' -> auditCollection' <> null) auditCollection) = false
                        then 
                            query {
                                    for i in dbContext.AuditCollection do
                                        if i.ID=id
                                            then select (i, i.Persons, i.Organizations)
                                    }
                            |> Seq.map (fun (auditCollection,_, _) -> auditCollection)
                            |> (fun auditCollection -> if (Seq.exists (fun auditCollection' -> auditCollection' <> null) auditCollection) = false
                                                        then None
                                                        else Some auditCollection
                                )
                        else Some auditCollection
                    )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:AuditCollection) (item2:AuditCollection) =
                item1.Persons=item2.Persons && item1.Organizations=item2.Organizations

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:AuditCollection) =
                    AuditCollectionHandler.tryFindByID dbContext item.ID
                    |> (fun organizationCollection -> match organizationCollection with
                                                        |Some x -> x
                                                                    |> Seq.map (fun organization -> match AuditCollectionHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:AuditCollection) =
                AuditCollectionHandler.addToContext dbContext item |> ignore |> ignore
                dbContext.SaveChanges()  
        
        type MzIdentMLDocumentHandler =
            ///Initializes a mzIdentML-object with at least all necessary parameters.
            static member init
                (             
                    ?inputs                         : Inputs,
                    ?version                        : string,
                    ?spectrumIdentification         : seq<SpectrumIdentification>,
                    ?spectrumIdentificationProtocol : seq<SpectrumIdentificationProtocol>,
                    ?analysisData                   : AnalysisData,
                    ?id                             : string,
                    ?name                           : string,
                    ?analysisSoftwares              : seq<AnalysisSoftware>,
                    ?provider                       : Provider,
                    ?auditCollection                : AuditCollection, 
                    ?samples                        : seq<Sample>,
                    ?dbSequences                    : seq<DBSequence>,
                    ?peptides                       : seq<Peptide>,
                    ?peptideEvidences               : seq<PeptideEvidence>,
                    ?proteinDetection               : ProteinDetection,
                    ?proteinDetectionProtocol       : ProteinDetectionProtocol,
                    ?biblioGraphicReferences        : seq<BiblioGraphicReference>
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                           = defaultArg name Unchecked.defaultof<string>
                let version'                        = defaultArg version Unchecked.defaultof<string>
                let analysisSoftwares'              = convertOptionToList analysisSoftwares
                let provider'                       = defaultArg provider Unchecked.defaultof<Provider>
                let auditCollection'                = defaultArg auditCollection Unchecked.defaultof<AuditCollection>
                let samples'                        = convertOptionToList samples
                let dbSequences'                    = convertOptionToList dbSequences
                let peptides'                       = convertOptionToList peptides
                let peptideEvidences'               = convertOptionToList peptideEvidences
                let spectrumIdentification'         = convertOptionToList spectrumIdentification
                let proteinDetection'               = defaultArg proteinDetection Unchecked.defaultof<ProteinDetection>
                let spectrumIdentificationProtocol' = convertOptionToList spectrumIdentificationProtocol
                let proteinDetectionProtocol'       = defaultArg proteinDetectionProtocol Unchecked.defaultof<ProteinDetectionProtocol>
                let inputs'                         = defaultArg inputs Unchecked.defaultof<Inputs>
                let analysisData'                   = defaultArg analysisData Unchecked.defaultof<AnalysisData>
                let biblioGraphicReferences'        = convertOptionToList biblioGraphicReferences
                new MzIdentMLDocument(
                                      id', 
                                      name',
                                      version',
                                      analysisSoftwares', 
                                      provider', 
                                      auditCollection', 
                                      samples', 
                                      dbSequences', 
                                      peptides', 
                                      peptideEvidences', 
                                      spectrumIdentification',
                                      proteinDetection',
                                      spectrumIdentificationProtocol',
                                      proteinDetectionProtocol', 
                                      inputs', 
                                      analysisData',
                                      biblioGraphicReferences',
                                      Nullable(DateTime.Now)
                                     )
                    
            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.Name <- name
                mzIdentML

            ///Replaces version of existing object with new one.
            static member addVersion
                (version:string) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.Version <- version
                mzIdentML

            ///Adds a analysissoftware to an existing mzidentmldocument-object.
            static member addAnalysisSoftware
                (analysisSoftware:AnalysisSoftware) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.AnalysisSoftwares <- addToList mzIdentML.AnalysisSoftwares analysisSoftware
                mzIdentML

            ///Adds a collection of analysissoftwares to an existing mzidentmldocument-object.
            static member addAnalysisSoftwares
                (analysisSoftwares:seq<AnalysisSoftware>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.AnalysisSoftwares <- addCollectionToList mzIdentML.AnalysisSoftwares analysisSoftwares
                mzIdentML

            ///Replaces provider of existing object with new one.
            static member addProvider
                (provider:Provider) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.Provider <- provider
                mzIdentML

            ///Replaces auditCollection of existing object with new one.
            static member addAuditCollection
                (auditCollection:AuditCollection) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.AuditCollection <- auditCollection
                mzIdentML

            ///Adds a sample to an existing mzidentmldocument-object.
            static member addSample
                (sample:Sample) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Samples <- addToList mzIdentML.Samples sample
                mzIdentML

            ///Adds a collection of samples to an existing mzidentmldocument-object.
            static member addSamples
                (samples:seq<Sample>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Samples <- addCollectionToList mzIdentML.Samples samples
                mzIdentML

            ///Adds a dbsequence to an existing mzidentmldocument-object.
            static member addDBSequence
                (dbSequence:DBSequence) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.DBSequences <- addToList mzIdentML.DBSequences dbSequence
                mzIdentML

            ///Adds a collection of dbsequences to an existing mzidentmldocument-object.
            static member addDBSequences
                (dbSequences:seq<DBSequence>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.DBSequences <- addCollectionToList mzIdentML.DBSequences dbSequences
                mzIdentML

            ///Adds a peptide to an existing mzidentmldocument-object.
            static member addPeptide
                (peptide:Peptide) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Peptides <- addToList mzIdentML.Peptides peptide
                mzIdentML

            ///Adds a collection of peptides to an existing mzidentmldocument-object.
            static member addPeptides
                (peptides:seq<Peptide>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Peptides <- addCollectionToList mzIdentML.Peptides peptides
                mzIdentML

            ///Adds a peptideevidence to an existing mzidentmldocument-object.
            static member addPeptideEvidence
                (peptideEvidence:PeptideEvidence) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.PeptideEvidences <- addToList mzIdentML.PeptideEvidences peptideEvidence
                mzIdentML

            ///Adds a collection of peptideevidences to an existing mzidentmldocument-object.
            static member addPeptideEvidences
                (peptideEvidences:seq<PeptideEvidence>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.PeptideEvidences <- addCollectionToList mzIdentML.PeptideEvidences peptideEvidences
                mzIdentML

            ///Adds a spectrumidentification to an existing mzidentmldocument-object.
            static member addSpectrumIdentification
                (spectrumIdentification:SpectrumIdentification) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.SpectrumIdentifications <- addToList mzIdentML.SpectrumIdentifications spectrumIdentification
                mzIdentML

            ///Adds a collection of spectrumidentifications to an existing mzidentmldocument-object.
            static member addSpectrumIdentifications
                (spectrumIdentification:seq<SpectrumIdentification>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.SpectrumIdentifications <- addCollectionToList mzIdentML.SpectrumIdentifications spectrumIdentification
                mzIdentML

            ///Replaces proteinDetection of existing object with new one.
            static member addProteinDetection
                (proteinDetection:ProteinDetection) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.ProteinDetection <- proteinDetection
                mzIdentML

            ///Adds a spectrumidentificationprotocol to an existing mzidentmldocument-object.
            static member addSpectrumIdentificationProtocol
                (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.SpectrumIdentificationProtocols <- addToList mzIdentML.SpectrumIdentificationProtocols spectrumIdentificationProtocol
                mzIdentML

            ///Adds a collection of spectrumidentificationprotocols to an existing mzidentmldocument-object.
            static member addSpectrumIdentificationProtocols
                (spectrumIdentificationProtocol:seq<SpectrumIdentificationProtocol>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.SpectrumIdentificationProtocols <- addCollectionToList mzIdentML.SpectrumIdentificationProtocols spectrumIdentificationProtocol
                mzIdentML

            ///Replaces proteinDetectionProtocol of existing object with new one.
            static member addProteinDetectionProtocol
                (proteinDetectionProtocol:ProteinDetectionProtocol) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.ProteinDetectionProtocol <- proteinDetectionProtocol
                mzIdentML

            ///Replaces inputs of existing object with new one.
            static member addInputs
                (inputs:Inputs) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.Inputs <- inputs
                mzIdentML

            ///Replaces analysisData of existing object with new one.
            static member addAnalysisData
                (analysisData:AnalysisData) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.AnalysisData <- analysisData
                mzIdentML

            ///Adds a bibliographicreference to an existing mzidentmldocument-object.
            static member addBiblioGraphicReference
                (biblioGraphicReference:BiblioGraphicReference) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.BiblioGraphicReferences <- addToList mzIdentML.BiblioGraphicReferences biblioGraphicReference
                mzIdentML

            ///Adds a collection of bibliographicreferences to an existing mzidentmldocument-object.
            static member addBiblioGraphicReferences
                (biblioGraphicReferences:seq<BiblioGraphicReference>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.BiblioGraphicReferences <- addCollectionToList mzIdentML.BiblioGraphicReferences biblioGraphicReferences
                mzIdentML

            ///Tries to find a mzIdentMLDocument-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.MzIdentMLDocument.Local do
                           if i.ID=id
                              then select (i, i.Inputs, i.AnalysisSoftwares, i.Provider, i.AuditCollection, i.Samples,
                                           i.DBSequences, i.Peptides, i.PeptideEvidences, i.SpectrumIdentifications, i.ProteinDetection,
                                           i.SpectrumIdentificationProtocols, i.ProteinDetectionProtocol, i.AnalysisData, i.BiblioGraphicReferences
                                          )
                      }
                |> Seq.map (fun (mzIdentMLDocument, _, _, _, _, _, _, _, _, _, _, _, _, _, _) -> mzIdentMLDocument)
                |> (fun mzIdentMLDocument -> 
                    if (Seq.exists (fun mzIdentMLDocument' -> mzIdentMLDocument' <> null) mzIdentMLDocument) = false
                        then 
                            query {
                                   for i in dbContext.MzIdentMLDocument do
                                       if i.ID=id
                                          then select (i, i.Inputs, i.AnalysisSoftwares, i.Provider, i.AuditCollection, i.Samples,
                                                       i.DBSequences, i.Peptides, i.PeptideEvidences, i.SpectrumIdentifications, i.ProteinDetection,
                                                       i.SpectrumIdentificationProtocols, i.ProteinDetectionProtocol, i.AnalysisData, i.BiblioGraphicReferences
                                                      )
                                  }
                            |> Seq.map (fun (mzIdentMLDocument, _, _, _, _, _, _, _, _, _, _, _, _, _, _) -> mzIdentMLDocument)
                            |> (fun mzIdentMLDocument -> if (Seq.exists (fun mzIdentMLDocument' -> mzIdentMLDocument' <> null) mzIdentMLDocument) = false
                                                            then None
                                                            else Some (mzIdentMLDocument.Single())
                               )
                        else Some (mzIdentMLDocument.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
                query {
                       for i in dbContext.MzIdentMLDocument.Local do
                           if i.Name=name
                              then select (i, i.Inputs, i.AnalysisSoftwares, i.Provider, i.AuditCollection, i.Samples,
                                           i.DBSequences, i.Peptides, i.PeptideEvidences, i.SpectrumIdentifications, i.ProteinDetection,
                                           i.SpectrumIdentificationProtocols, i.ProteinDetectionProtocol, i.AnalysisData, i.BiblioGraphicReferences
                                          )
                      }
                |> Seq.map (fun (mzIdentMLDocument, _, _, _, _, _, _, _, _, _, _, _, _, _, _) -> mzIdentMLDocument)
                |> (fun mzIdentMLDocument -> 
                    if (Seq.exists (fun mzIdentMLDocument' -> mzIdentMLDocument' <> null) mzIdentMLDocument) = false
                        then 
                            query {
                                   for i in dbContext.MzIdentMLDocument do
                                       if i.Name=name
                                          then select (i, i.Inputs, i.AnalysisSoftwares, i.Provider, i.AuditCollection, i.Samples,
                                                       i.DBSequences, i.Peptides, i.PeptideEvidences, i.SpectrumIdentifications, i.ProteinDetection,
                                                       i.SpectrumIdentificationProtocols, i.ProteinDetectionProtocol, i.AnalysisData, i.BiblioGraphicReferences
                                                      )
                                  }
                            |> Seq.map (fun (mzIdentMLDocument, _, _, _, _, _, _, _, _, _, _, _, _, _, _) -> mzIdentMLDocument)
                            |> (fun mzIdentMLDocument -> if (Seq.exists (fun mzIdentMLDocument' -> mzIdentMLDocument' <> null) mzIdentMLDocument) = false
                                                             then None
                                                             else Some mzIdentMLDocument
                               )
                        else Some mzIdentMLDocument
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:MzIdentMLDocument) (item2:MzIdentMLDocument) =
               item1.Name=item2.Name && item1.Version=item2.Version && item1.AnalysisSoftwares=item2.AnalysisSoftwares && 
               item1.Provider=item2.Provider && item1.AuditCollection=item2.AuditCollection &&
               item1.Samples=item2.Samples && item1.DBSequences=item2.DBSequences && item1.Peptides=item2.Peptides && 
               item1.PeptideEvidences=item2.PeptideEvidences && item1.SpectrumIdentifications=item2.SpectrumIdentifications &&
               item1.ProteinDetection=item2.ProteinDetection && item1.SpectrumIdentificationProtocols=item2.SpectrumIdentificationProtocols &&
               item1.ProteinDetectionProtocol=item2.ProteinDetectionProtocol && item1.Inputs=item2.Inputs && item1.AnalysisData=item2.AnalysisData &&
               item1.BiblioGraphicReferences=item2.BiblioGraphicReferences

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:MzIdentMLDocument) =
                    MzIdentMLDocumentHandler.tryFindByName dbContext item.Name
                    |> (fun organizationCollection -> match organizationCollection with
                                                      |Some x -> x
                                                                 |> Seq.map (fun organization -> match MzIdentMLDocumentHandler.hasEqualFieldValues organization item with
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
            static member addToContextAndInsert (dbContext:MzIdentML) (item:MzIdentMLDocument) =
                MzIdentMLDocumentHandler.addToContext dbContext item |> ignore
                dbContext.SaveChanges()
