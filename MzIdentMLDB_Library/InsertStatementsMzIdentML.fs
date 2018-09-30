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
                table.FKOntology <- ontologyID
                table

            ///Tries to find a term-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (ontologyID:string) =
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
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
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
            static member addFKUnit
                (fkUnit:string) (table:CVParam) =
                table.FKUnit <- fkUnit
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.CVParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.CVParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:CVParam) =
                    CVParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>

                new OrganizationParam(
                                      id', 
                                      value',
                                      null,
                                      fkTerm,
                                      null,
                                      fkUnit', 
                                      fk',
                                      Nullable(DateTime.Now)
                                     )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:OrganizationParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:OrganizationParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkOrganization of existing object with new one.
            static member addFKOrganization
                (fkOrganization:string) (table:OrganizationParam) =
                table.FKOrganization <- fkOrganization
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.OrganizationParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.OrganizationParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:OrganizationParam) =
                    OrganizationParamHandler.tryFindByFKTerm dbContext item.Term.Name
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

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:PersonParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:PersonParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkPerson of existing object with new one.
            static member addFKPerson
                (fkPerson:string) (table:PersonParam) =
                table.FKPerson <- fkPerson
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.PersonParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PersonParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:PersonParam) =
                    PersonParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new SampleParam(
                                id', 
                                value',
                                null,
                                fkTerm,
                                null,
                                fkUnit', 
                                fk',
                                Nullable(DateTime.Now)
                               )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SampleParam)  =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:SampleParam)  =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSample of existing object with new one.
            static member addFKSample
                (fkSample:string) (table:SampleParam)  =
                table.FKSample <- fkSample
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.SampleParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SampleParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SampleParam) =
                    SampleParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new ModificationParam(
                                      id', 
                                      value',
                                      null,
                                      fkTerm,
                                      null,
                                      fkUnit', 
                                      fk',
                                      Nullable(DateTime.Now)
                                     )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:ModificationParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:ModificationParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkModification of existing object with new one.
            static member addFKModification
                (fkModification:string) (table:ModificationParam) =
                table.FKModification <- fkModification
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.ModificationParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ModificationParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ModificationParam) =
                    ModificationParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new PeptideParam(
                                 id', 
                                 value',
                                 null,
                                 fkTerm,
                                 null,
                                 fkUnit', 
                                 fk',
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:PeptideParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:PeptideParam)  =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkPeptide of existing object with new one.
            static member addFKPeptide
                (fkPeptide:string) (table:PeptideParam)  =
                table.FKPeptide <- fkPeptide
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.PeptideParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PeptideParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:PeptideParam) =
                    PeptideParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new TranslationTableParam(
                                          id', 
                                          value',
                                          null,
                                          fkTerm,
                                          null,
                                          fkUnit', 
                                          fk',
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:TranslationTableParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:TranslationTableParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkTranslationTable of existing object with new one.
            static member addFKTranslationTable
                (fkTranslationTable:string) (table:TranslationTableParam) =
                table.FKTranslationTable <- fkTranslationTable
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.TranslationTableParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.TranslationTableParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:TranslationTableParam) =
                    TranslationTableParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new MeasureParam(
                                 id', 
                                 value',
                                 null,
                                 fkTerm,
                                 null,
                                 fkUnit', 
                                 fk',
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:MeasureParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:MeasureParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkMeasure of existing object with new one.
            static member addFKMeasure
                (fkMeasure:string) (table:MeasureParam) =
                table.FKMeasure <- fkMeasure
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.MeasureParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.MeasureParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:MeasureParam) =
                    MeasureParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>

                new AmbiguousResidueParam(
                                          id', 
                                          value',
                                          null,
                                          fkTerm,
                                          null,
                                          fkUnit', 
                                          fk',
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:AmbiguousResidueParam)  =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:AmbiguousResidueParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkAmbiguousResidue of existing object with new one.
            static member addFKAmbiguousResidue
                (fkAmbiguousResidue:string) (table:AmbiguousResidueParam) =
                table.FKAmbiguousResidue <- fkAmbiguousResidue
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.AmbiguousResidueParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.AmbiguousResidueParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:AmbiguousResidueParam) =
                    AmbiguousResidueParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new MassTableParam(
                                   id', 
                                   value',
                                   null,
                                   fkTerm,
                                   null,
                                   fkUnit', 
                                   fk',
                                   Nullable(DateTime.Now)
                                  )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:MassTableParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:MassTableParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkMassTable of existing object with new one.
            static member addFKMassTable
                (fkMassTable:string) (table:MassTableParam) =
                table.FKMassTable <- fkMassTable
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.MassTableParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.MassTableParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:MassTableParam) =
                    MassTableParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new IonTypeParam(
                                 id', 
                                 value',
                                 null,
                                 fkTerm,
                                 null,
                                 fkUnit', 
                                 fk',
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:IonTypeParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:IonTypeParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkIonType of existing object with new one.
            static member addFKIonType
                (fkIonType:string) (table:IonTypeParam) =
                table.FKIonType <- fkIonType
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.IonTypeParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.IonTypeParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:IonTypeParam) =
                    IonTypeParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>

                new SpecificityRuleParam(
                                         id', 
                                         value',
                                         null,
                                         fkTerm,
                                         null,
                                         fkUnit', 
                                         fk',
                                         Nullable(DateTime.Now)
                                        )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SpecificityRuleParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:SpecificityRuleParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSearchModification of existing object with new one.
            static member addFKSearchModification
                (fkSearchModification:string) (table:SpecificityRuleParam) =
                table.FKSearchModification <- fkSearchModification
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.SpecificityRuleParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SpecificityRuleParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SpecificityRuleParam) =
                    SpecificityRuleParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new SearchModificationParam(
                                            id', 
                                            value',
                                            null,
                                            fkTerm,
                                            null,
                                            fkUnit', 
                                            fk',
                                            Nullable(DateTime.Now)
                                           )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SearchModificationParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:SearchModificationParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSearchModification of existing object with new one.
            static member addFKSearchModification
                (fkSearchModification:string) (table:SearchModificationParam) =
                table.FKSearchModification <- fkSearchModification
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.SearchModificationParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SearchModificationParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SearchModificationParam) =
                    SearchModificationParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new EnzymeNameParam(
                                    id', 
                                    value',
                                    null,
                                    fkTerm,
                                    null,
                                    fkUnit', 
                                    fk',
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:EnzymeNameParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:EnzymeNameParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkEnzyme of existing object with new one.
            static member addFKEnzyme
                (fkEnzyme:string) (table:EnzymeNameParam) =
                table.FKEnzyme <- fkEnzyme
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.EnzymeNameParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.EnzymeNameParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:EnzymeNameParam) =
                    EnzymeNameParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new IncludeParam(
                                 id', 
                                 value',
                                 null,
                                 fkTerm,
                                 null,
                                 fkUnit', 
                                 fk',
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:IncludeParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:IncludeParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkFilter of existing object with new one.
            static member addFKFilter
                (fkFilter:string) (table:IncludeParam) =
                table.FKFilter <- fkFilter
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.IncludeParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.IncludeParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:IncludeParam) =
                    IncludeParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new ExcludeParam(
                                 id', 
                                 value',
                                 null,
                                 fkTerm,
                                 null,
                                 fkUnit', 
                                 fk',
                                 Nullable(DateTime.Now)
                                )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:ExcludeParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:ExcludeParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkFilter of existing object with new one.
            static member addFKFilter
                (fkFilter:string) (table:ExcludeParam) =
                table.FKFilter <- fkFilter
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.ExcludeParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ExcludeParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ExcludeParam) =
                    ExcludeParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>

                new AdditionalSearchParam(
                                          id', 
                                          value',
                                          null,
                                          fkTerm,
                                          null,
                                          fkUnit', 
                                          fk',
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:AdditionalSearchParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:AdditionalSearchParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSpectrumIdentificationProtocol of existing object with new one.
            static member addFKSpectrumIdentificationProtocol
                (fkSpectrumIdentificationProtocol:string) (table:AdditionalSearchParam) =
                table.FKSpectrumIdentificationProtocol <- fkSpectrumIdentificationProtocol
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.AdditionalSearchParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.AdditionalSearchParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:AdditionalSearchParam) =
                    AdditionalSearchParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new FragmentToleranceParam(
                                           id', 
                                           value',
                                           null,
                                           fkTerm,
                                           null,
                                           fkUnit', 
                                           fk',
                                           Nullable(DateTime.Now)
                                          )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:FragmentToleranceParam)  =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:FragmentToleranceParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSpectrumIdentificationProtocol of existing object with new one.
            static member addFKSpectrumIdentificationProtocol
                (fkSpectrumIdentificationProtocol:string) (table:FragmentToleranceParam) =
                table.FKSpectrumIdentificationProtocol <- fkSpectrumIdentificationProtocol
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.FragmentToleranceParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.FragmentToleranceParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:FragmentToleranceParam) =
                    FragmentToleranceParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new ParentToleranceParam(
                                         id', 
                                         value',
                                         null,
                                         fkTerm,
                                         null,
                                         fkUnit', 
                                         fk',
                                         Nullable(DateTime.Now)
                                        )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:ParentToleranceParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:ParentToleranceParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSpectrumIdentificationProtocol of existing object with new one.
            static member addFKSpectrumIdentificationProtocol
                (fkSpectrumIdentificationProtocol:string) (table:ParentToleranceParam) =
                table.FKSpectrumIdentificationProtocol <- fkSpectrumIdentificationProtocol
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.ParentToleranceParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ParentToleranceParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ParentToleranceParam) =
                    ParentToleranceParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    fkTerm              : string,
                    ?id                 : string,
                    ?value              : string,
                    ?fkUnit             : string,
                    ?fkSpecIdentProt    : string,
                    ?fkProteinDetProt   : string
                ) =
                let id'                 = defaultArg id (System.Guid.NewGuid().ToString())
                let value'              = defaultArg value Unchecked.defaultof<string>
                let fkUnit'             = defaultArg fkUnit Unchecked.defaultof<string>
                let fkSpecIdentProt'    = defaultArg fkSpecIdentProt Unchecked.defaultof<string>
                let fkProteinDetProt'   = defaultArg fkProteinDetProt Unchecked.defaultof<string>
                    
                new ThresholdParam(
                                   id', 
                                   value',
                                   null,
                                   fkTerm,
                                   null,
                                   fkUnit', 
                                   fkSpecIdentProt',
                                   fkProteinDetProt',
                                   Nullable(DateTime.Now)
                                  )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:ThresholdParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:ThresholdParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSpecIdentProt of existing object with new one.
            static member addFKSpectrumIdentificationProtocol
                (fkSpecIdentProt:string) (table:ThresholdParam) =
                table.FKSpectrumIdentificationProtocol <- fkSpecIdentProt
                table

            ///Replaces fkProteinDetProt of existing object with new one.
            static member addFKProteinDetectionProtocol
                (fkProteinDetProt:string) (table:ThresholdParam) =
                table.FKProteinDetectionProtocol <- fkProteinDetProt
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.ThresholdParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ThresholdParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ThresholdParam) =
                    ThresholdParamHandler.tryFindByFKTerm dbContext item.Term.Name
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

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SearchDatabaseParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:SearchDatabaseParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSearchDatabase of existing object with new one.
            static member addFKSearchDatabase
                (fkSearchDatabase:string) (table:SearchDatabaseParam) =
                table.FKSearchDatabase <- fkSearchDatabase
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.SearchDatabaseParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SearchDatabaseParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SearchDatabaseParam) =
                    SearchDatabaseParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new DBSequenceParam(
                                    id', 
                                    value',
                                    null,
                                    fkTerm,
                                    null,
                                    fkUnit', 
                                    fk',
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:DBSequenceParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:DBSequenceParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkDBSequence of existing object with new one.
            static member addFKDBSequence
                (fkDBSequence:string) (table:DBSequenceParam) =
                table.FKDBSequence <- fkDBSequence
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.DBSequenceParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.DBSequenceParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:DBSequenceParam) =
                    DBSequenceParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new PeptideEvidenceParam(
                                         id', 
                                         value',
                                         null,
                                         fkTerm,
                                         null,
                                         fkUnit', 
                                         fk',
                                         Nullable(DateTime.Now)
                                        )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:PeptideEvidenceParam)  =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:PeptideEvidenceParam)  =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkPeptideEvidence of existing object with new one.
            static member addFKPeptideEvidence
                (fkPeptideEvidence:string) (table:PeptideEvidenceParam)  =
                table.FKPeptideEvidence <- fkPeptideEvidence
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.PeptideEvidenceParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.PeptideEvidenceParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:PeptideEvidenceParam) =
                    PeptideEvidenceParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>

                new SpectrumIdentificationItemParam(
                                                    id', 
                                                    value',
                                                    null,
                                                    fkTerm,
                                                    null,
                                                    fkUnit', 
                                                    fk',
                                                    Nullable(DateTime.Now)
                                                   )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SpectrumIdentificationItemParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:SpectrumIdentificationItemParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSpectrumIdentificationItem of existing object with new one.
            static member addFKSpectrumIdentificationItem
                (fkSpectrumIdentificationItem:string) (table:SpectrumIdentificationItemParam) =
                table.FKSpectrumIdentificationItem <- fkSpectrumIdentificationItem
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.SpectrumIdentificationItemParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationItemParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SpectrumIdentificationItemParam) =
                    SpectrumIdentificationItemParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>

                new SpectrumIdentificationResultParam(
                                                      id', 
                                                      value',
                                                      null,
                                                      fkTerm,
                                                      null,
                                                      fkUnit', 
                                                      fk',
                                                      Nullable(DateTime.Now)
                                                     )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SpectrumIdentificationResultParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:SpectrumIdentificationResultParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSpectrumIdentificationResult of existing object with new one.
            static member addFKSpectrumIdentificationResult
                (fkSpectrumIdentificationResult:string) (table:SpectrumIdentificationResultParam) =
                table.FKSpectrumIdentificationResult <- fkSpectrumIdentificationResult
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.SpectrumIdentificationResultParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationResultParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SpectrumIdentificationResultParam) =
                    SpectrumIdentificationResultParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>

                new SpectrumIdentificationListParam(
                                                    id', 
                                                    value',
                                                    null,
                                                    fkTerm,
                                                    null,
                                                    fkUnit', 
                                                    fk',
                                                    Nullable(DateTime.Now)
                                                   )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SpectrumIdentificationListParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:SpectrumIdentificationListParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSpectrumIdentificationList of existing object with new one.
            static member addFKSpectrumIdentificationList
                (fkSpectrumIdentificationList:string) (table:SpectrumIdentificationListParam) =
                table.FKSpectrumIdentificationList <- fkSpectrumIdentificationList
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.SpectrumIdentificationListParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationListParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SpectrumIdentificationListParam) =
                    SpectrumIdentificationListParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new AnalysisParam(
                                  id', 
                                  value',
                                  null,
                                  fkTerm,
                                  null,
                                  fkUnit', 
                                  fk',
                                  Nullable(DateTime.Now)
                                 )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:AnalysisParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:AnalysisParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkProteinDetectionProtocol of existing object with new one.
            static member addFKProteinDetectionProtocol
                (fkProteinDetectionProtocol:string) (table:AnalysisParam) =
                table.FKProteinDetectionProtocol <- fkProteinDetectionProtocol
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.AnalysisParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:AnalysisParam) =
                    AnalysisParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new SourceFileParam(
                                    id', 
                                    value',
                                    null,
                                    fkTerm,
                                    null,
                                    fkUnit', 
                                    fk',
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:SourceFileParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:SourceFileParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkSourceFile of existing object with new one.
            static member addFKSourceFile
                (fkSourceFile:string) (table:SourceFileParam) =
                table.FKSourceFile <- fkSourceFile
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.SourceFileParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.SourceFileParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:SourceFileParam) =
                    SourceFileParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>
                    
                new ProteinDetectionHypothesisParam(
                                                    id', 
                                                    value',
                                                    null,
                                                    fkTerm,
                                                    null,
                                                    fkUnit', 
                                                    fk',
                                                    Nullable(DateTime.Now)
                                                   )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:ProteinDetectionHypothesisParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:ProteinDetectionHypothesisParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkProteinDetectionHypothesis of existing object with new one.
            static member addFKProteinDetectionHypothesis
                (fkProteinDetectionHypothesis:string) (table:ProteinDetectionHypothesisParam) =
                table.FKProteinDetectionHypothesis <- fkProteinDetectionHypothesis
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.ProteinDetectionHypothesisParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionHypothesisParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ProteinDetectionHypothesisParam) =
                    ProteinDetectionHypothesisParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>

                new  ProteinAmbiguityGroupParam(
                                                id', 
                                                value',
                                                null,
                                                fkTerm,
                                                null,
                                                fkUnit', 
                                                fk',
                                                Nullable(DateTime.Now)
                                               )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:ProteinAmbiguityGroupParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:ProteinAmbiguityGroupParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkProteinAmbiguityGroup of existing object with new one.
            static member addFKProteinAmbiguityGroup
                (fkProteinAmbiguityGroup:string) (table:ProteinAmbiguityGroupParam) =
                table.FKProteinAmbiguityGroup <- fkProteinAmbiguityGroup
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.ProteinAmbiguityGroupParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinAmbiguityGroupParam do
                                       if i.FKTerm=fkTerm
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
                 item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ProteinAmbiguityGroupParam) =
                    ProteinAmbiguityGroupParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?fkUnit   : string,
                    ?fk       : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let fkUnit'   = defaultArg fkUnit Unchecked.defaultof<string>
                let fk'       = defaultArg fk Unchecked.defaultof<string>

                new ProteinDetectionListParam(
                                              id', 
                                              value',
                                              null,
                                              fkTerm,
                                              null,
                                              fkUnit', 
                                              fk',
                                              Nullable(DateTime.Now)
                                             )

            ///Replaces value of existing object with new one..
            static member addValue
                (value:string) (table:ProteinDetectionListParam) =
                table.Value <- value
                table

            ///Replaces fkUnit of existing object with new one.
            static member addFKUnit
                (fkUnit:string) (table:ProteinDetectionListParam) =
                table.FKUnit <- fkUnit
                table

            ///Replaces fkProteinDetectionList of existing object with new one.
            static member addFKProteinDetectionList
                (fkProteinDetectionList:string) (table:ProteinDetectionListParam) =
                table.FKProteinDetectionList <- fkProteinDetectionList
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
            static member tryFindByFKTerm (dbContext:MzIdentML) (fkTerm:string) =
                query {
                       for i in dbContext.ProteinDetectionListParam.Local do
                           if i.FKTerm=fkTerm
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if (Seq.exists (fun param' -> param' <> null) param) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionListParam do
                                       if i.FKTerm=fkTerm
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
                item1.Value=item2.Value && item1.FKUnit=item2.FKUnit

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ProteinDetectionListParam) =
                    ProteinDetectionListParamHandler.tryFindByFKTerm dbContext item.Term.Name
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
                    ?parent              : Organization,
                    ?fkParent            : string,
                    ?fkPerson            : string,
                    ?fkMzIdentMLDocument : string,
                    ?details             : seq<OrganizationParam>
                    
                ) =
                let id'                  = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                = defaultArg name Unchecked.defaultof<string>
                let parent'              = defaultArg parent Unchecked.defaultof<Organization>
                let fkParent'            = defaultArg fkParent Unchecked.defaultof<string>
                let fkPerson'            = defaultArg fkPerson Unchecked.defaultof<string>
                let fkMzIdentMLDocument' = defaultArg fkMzIdentMLDocument Unchecked.defaultof<string>
                let details'             = convertOptionToList details

                new Organization(
                                 id', 
                                 name', 
                                 parent',
                                 fkParent',
                                 fkPerson',
                                 fkMzIdentMLDocument',
                                 details',
                                 Nullable(DateTime.Now)
                                )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:Organization) =
                table.Name <- name
                table

            ///Replaces parent of existing object with new one.
            static member addParent
                (parent:Organization) (table:Organization) =
                table.Parent <- parent
                table

            ///Replaces fkParent of existing object with new one.
            static member addFKParent
                (fkParent:string) (table:Organization) =
                table.FKParent <- fkParent
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

            ///Replaces fkMzIDentMLDocument of existing object with new one.
            static member addMzIdentMLDocumentID
                (fkMzIDentMLDocument:string) (table:Organization) =
                table.FKMzIdentMLDocument <- fkMzIDentMLDocument
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
                item1.FKParent=item2.FKParent && item1.FKPerson=item2.FKPerson && 
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)  

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
                    ?fkMzIdentMLDocument : string,
                    ?contactDetails      : seq<PersonParam>
                ) =
                let id'                  = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                = defaultArg name Unchecked.defaultof<string>
                let firstName'           = defaultArg firstName Unchecked.defaultof<string>
                let midInitials'         = defaultArg midInitials Unchecked.defaultof<string>
                let lastName'            = defaultArg lastName Unchecked.defaultof<string>    
                let organizations'       = convertOptionToList organizations
                let fkMzIdentMLDocument' = defaultArg fkMzIdentMLDocument Unchecked.defaultof<string>
                let contactDetails'      = convertOptionToList contactDetails

                new Person(
                           id', 
                           name', 
                           firstName',  
                           midInitials', 
                           lastName', 
                           organizations',
                           fkMzIdentMLDocument',
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

            ///Adds a personparam to an existing person-object.
            static member addDetail (detail:PersonParam) (table:Person) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of personparams to an existing person-object.
            static member addDetails
                (details:seq<PersonParam>) (table:Person) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Replaces fkMzIDentMLDocument of existing object with new one.
            static member addMzIdentMLDocumentID
                (fkMzIDentMLDocument:string) (table:Person) =
                table.FKMzIdentMLDocument <- fkMzIDentMLDocument
                table

            ///Tries to find a person-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
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

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByName (dbContext:MzIdentML) (name:string) =
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
                item1.Organizations=item2.Organizations && item1.Organizations=item2.Organizations

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
                    fkPerson    : string, 
                    fkRole      : string,
                    ?id         : string,
                    ?person     : Person, 
                    ?role       : CVParam,
                    ?fkSample   : string
                ) =
                let id'         = defaultArg id (System.Guid.NewGuid().ToString())
                let person'     = defaultArg person Unchecked.defaultof<Person>
                let role'       = defaultArg role Unchecked.defaultof<CVParam>
                let fkSample'   = defaultArg fkSample Unchecked.defaultof<string>

                new ContactRole(
                                id', 
                                person',
                                fkPerson,
                                role',
                                fkRole,
                                fkSample',
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

            ///Replaces fkSample of existing object with new one.
            static member addFKSample
                (fkSample:string) (table:ContactRole) =
                table.FKSample <- fkSample
                table

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
            static member tryFindByFKPerson (dbContext:MzIdentML) (fkPerson:string) =
                query {
                       for i in dbContext.ContactRole.Local do
                           if i.FKPerson=fkPerson
                              then select (i, i.Role, i.Person)
                      }
                |> Seq.map (fun (contactRole, _, _) -> contactRole)
                |> (fun contactRole -> 
                    if (Seq.exists (fun contactRole' -> contactRole' <> null) contactRole) = false
                        then 
                            query {
                                   for i in dbContext.ContactRole do
                                       if i.FKPerson=fkPerson
                                          then select (i, i.Role, i.Person)
                                  }
                            |> Seq.map (fun (contactRole, _, _) -> contactRole)
                            |> (fun contactRole -> if (Seq.exists (fun contactRole' -> contactRole' <> null) contactRole) = false
                                                       then None
                                                       else Some contactRole
                               )
                        else Some contactRole
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:ContactRole) (item2:ContactRole) =
                item1.FKRole=item2.FKRole && item1.FKSample=item2.FKSample

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:ContactRole) =
                    ContactRoleHandler.tryFindByFKPerson dbContext item.FKPerson
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
                    fkSoftwareName          : string,
                    ?id                     : string,
                    ?name                   : string,
                    ?uri                    : string,
                    ?version                : string,
                    ?customizations         : string,
                    ?softwareDeveloper      : ContactRole,
                    ?fkSoftwareDeveloper    : string,
                    ?softwareName           : CVParam
                ) =
                let id'                 = defaultArg id (System.Guid.NewGuid().ToString())
                let name'               = defaultArg name Unchecked.defaultof<string>
                let uri'                = defaultArg uri Unchecked.defaultof<string>
                let version'            = defaultArg version Unchecked.defaultof<string>
                let customizations'     = defaultArg customizations Unchecked.defaultof<string>
                let contactRole'        = defaultArg softwareDeveloper Unchecked.defaultof<ContactRole>
                let fkContactRole'      = defaultArg fkSoftwareDeveloper Unchecked.defaultof<string>
                let softwareName'       = defaultArg softwareName Unchecked.defaultof<CVParam>
                    
                new AnalysisSoftware(
                                     id', 
                                     name', 
                                     uri', 
                                     version', 
                                     customizations', 
                                     contactRole',
                                     fkContactRole', 
                                     softwareName',
                                     fkSoftwareName,
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:AnalysisSoftware) =
                table.Name <- name
                table

            ///Replaces uri of existing object with new one.
            static member addURI
                (uri:string) (table:AnalysisSoftware) =
                table.URI <- uri
                table

            ///Replaces version of existing object with new one.
            static member addVersion
                (version:string) (table:AnalysisSoftware) =
                table.Version <- version
                table

            ///Replaces customization of existing object with new one.
            static member addCustomization
                (customizations:string) (table:AnalysisSoftware) =
                table.Customizations <- customizations
                table

            ///Replaces analysisSoftwareDeveloper of existing object with new one.
            static member addAnalysisSoftwareDeveloper
                (analysisSoftwareDeveloper:ContactRole) (table:AnalysisSoftware) =
                table.ContactRole <- analysisSoftwareDeveloper
                table

            ///Replaces fkAnalysisSoftwareDeveloper of existing object with new one.
            static member addFKAnalysisSoftwareDeveloper
                (fkAnalysisSoftwareDeveloper:string) (table:AnalysisSoftware) =
                table.FKContactRole <- fkAnalysisSoftwareDeveloper
                table

            ///Replaces softwareName of existing object with new one.
            static member addSoftwareName
                (softwareName:CVParam) (table:AnalysisSoftware) =
                table.SoftwareName <- softwareName
                table

            ///Tries to find a analysisSoftware-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.AnalysisSoftware.Local do
                           if i.ID=id
                              then select (i, i.SoftwareName, i.ContactRole)
                      }
                |> Seq.map (fun (analysisSoftware, _, _) -> analysisSoftware)
                |> (fun analysisSoftware -> 
                    if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisSoftware do
                                       if i.ID=id
                                          then select (i, i.SoftwareName, i.ContactRole)
                                  }
                            |> Seq.map (fun (analysisSoftware, _, _) -> analysisSoftware)
                            |> (fun analysisSoftware -> if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
                                                        then None
                                                        else Some (analysisSoftware.Single())
                               )
                        else Some (analysisSoftware.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFKSoftwareName (dbContext:MzIdentML) (fkSoftwareName:string) =
                query {
                       for i in dbContext.AnalysisSoftware.Local do
                           if i.FKSoftwareName=fkSoftwareName
                              then select (i, i.SoftwareName, i.ContactRole)
                      }
                |> Seq.map (fun (analysisSoftware, _, _) -> analysisSoftware)
                |> (fun analysisSoftware -> 
                    if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisSoftware do
                                       if i.FKSoftwareName=fkSoftwareName
                                          then select (i, i.SoftwareName, i.ContactRole)
                                  }
                            |> Seq.map (fun (analysisSoftware, _, _) -> analysisSoftware)
                            |> (fun analysisSoftware -> if (Seq.exists (fun analysisSoftware' -> analysisSoftware' <> null) analysisSoftware) = false
                                                            then None
                                                            else Some analysisSoftware
                               )
                        else Some analysisSoftware
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:AnalysisSoftware) (item2:AnalysisSoftware) =
                item1.Name=item2.Name && item1.URI=item2.URI && item1.Version=item2.Version && 
                item1.Customizations=item2.Customizations && item1.FKContactRole=item2.FKContactRole

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:AnalysisSoftware) =
                    AnalysisSoftwareHandler.tryFindByFKSoftwareName dbContext item.FKSoftwareName
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

            ///Replaces fkSample of existing object with new one.
            static member addFKSample
                (fkSample:string) (table:SubSample) =
                table.FKSample <- fkSample
                table

            ///Tries to find a subSample-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SubSample.Local do
                           if i.ID=id
                              then select i
                      }
                |> Seq.map (fun (subSample) -> subSample)
                |> (fun subSample -> 
                    if (Seq.exists (fun subSample' -> subSample' <> null) subSample) = false
                        then 
                            query {
                                   for i in dbContext.SubSample do
                                       if i.ID=id
                                          then select i
                                  }
                            |> Seq.map (fun (subSample) -> subSample)
                            |> (fun subSample -> if (Seq.exists (fun subSample' -> subSample' <> null) subSample) = false
                                                    then None
                                                    else Some (subSample.Single())
                               )
                        else Some (subSample.Single())
                   )

            ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
            static member tryFindByFKSample (dbContext:MzIdentML) (fkSample:string) =
                query {
                       for i in dbContext.SubSample.Local do
                           if i.FKSample=fkSample
                              then select i
                      }
                |> Seq.map (fun (subSample) -> subSample)
                |> (fun subSample -> 
                    if (Seq.exists (fun subSample' -> subSample' <> null) subSample) = false
                        then 
                            query {
                                   for i in dbContext.SubSample do
                                       if i.FKSample=fkSample
                                          then select i
                                  }
                            |> Seq.map (fun (subSample) -> subSample)
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
                    SubSampleHandler.tryFindByFKSample dbContext item.FKSample
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
                           fkMzIdentML', 
                           details', 
                           Nullable(DateTime.Now)
                          )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:Sample) =
                table.Name <- name
                table

            ///Adds a contactrole to an existing sample-object.
            static member addContactRole
                (contactRole:ContactRole) (table:Sample) =
                let result = table.ContactRoles <- addToList table.ContactRoles contactRole
                table

            ///Adds a collection of contactroles to an existing sample-object.
            static member addContactRoles
                (contactRoles:seq<ContactRole>) (table:Sample) =
                let result = table.ContactRoles <- addCollectionToList table.ContactRoles contactRoles
                table

            ///Adds a subsample to an existing sample-object.
            static member addSubSample
                (subSample:SubSample) (table:Sample) =
                let result = table.SubSamples <- addToList table.SubSamples subSample
                table

            ///Adds a collection of subsamples to an existing sample-object.
            static member addSubSamples
                (subSamples:seq<SubSample>) (table:Sample) =
                let result = table.SubSamples <- addCollectionToList table.SubSamples subSamples
                table

            ///Adds a sampleparam to an existing sample-object.
            static member addDetail
                (detail:SampleParam) (table:Sample) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of sampleparams to an existing sample-object.
            static member addDetails
                (details:seq<SampleParam>) (table:Sample) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Replaces fkMzIdentML of existing object with new mzIdentML.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (table:Sample) =
                let result = table.FKMzIdentMLDocument <- fkMzIdentML
                table

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
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) && item1.FKMzIdentMLDocument=item2.FKMzIdentMLDocument

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
                    details                 : seq<ModificationParam>,
                    ?id                     : string,
                    ?residues               : string,
                    ?location               : int,
                    ?monoIsotopicMassDelta  : float,
                    ?avgMassDelta           : float,
                    ?fkPeptide              : string
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let residues'               = defaultArg residues Unchecked.defaultof<string>
                let location'               = defaultArg location Unchecked.defaultof<int>
                let monoIsotopicMassDelta'  = defaultArg monoIsotopicMassDelta Unchecked.defaultof<float>
                let avgMassDelta'           = defaultArg avgMassDelta Unchecked.defaultof<float>
                let fkPeptide'              = defaultArg fkPeptide Unchecked.defaultof<string>
                    
                new Modification(
                                 id', 
                                 residues', 
                                 Nullable(location'), 
                                 Nullable(monoIsotopicMassDelta'), 
                                 Nullable(avgMassDelta'), 
                                 fkPeptide',
                                 details |> List, 
                                 Nullable(DateTime.Now)
                                )

            ///Replaces residues of existing object with new residues.
            static member addResidues
                (residues:string) (table:Modification) =
                table.Residues <- residues
                table

            ///Replaces location of existing object with new location.
            static member addLocation
                (location:int) (table:Modification) =
                table.Location <- Nullable(location)
                table

            ///Replaces monoisotopicmassdelta of existing object with new monoisotopicmassdelta.
            static member addMonoIsotopicMassDelta
                (monoIsotopicMassDelta:float) (table:Modification) =
                table.MonoIsotopicMassDelta <- Nullable(monoIsotopicMassDelta)
                table

            ///Replaces avgmassdelta of existing object with new avgmassdelta.
            static member addAvgMassDelta
                (avgMassDelta:float) (table:Modification) =
                table.AvgMassDelta <- Nullable(avgMassDelta)
                table

            ///Replaces fkPeptide of existing object with new avgmassdelta.
            static member addFKPeptide
                (fkPeptide:string) (table:Modification) =
                table.FKPeptide <- fkPeptide
                table

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
                item1.FKPeptide=item2.FKPeptide &&
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
                    originalResidue         : string,
                    replacementResidue      : string,
                    ?id                     : string,
                    ?location               : int,
                    ?monoIsotopicMassDelta  : float,
                    ?avgMassDelta           : float,
                    ?fkPeptide              : string
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let location'               = defaultArg location Unchecked.defaultof<int>
                let monoIsotopicMassDelta'  = defaultArg monoIsotopicMassDelta Unchecked.defaultof<float>
                let avgMassDelta'           = defaultArg avgMassDelta Unchecked.defaultof<float>
                let fkPeptide'              = defaultArg fkPeptide Unchecked.defaultof<string>

                new SubstitutionModification(
                                             id', 
                                             originalResidue, 
                                             replacementResidue, 
                                             Nullable(location'), 
                                             Nullable(monoIsotopicMassDelta'), 
                                             Nullable(avgMassDelta'),
                                             fkPeptide',
                                             Nullable(DateTime.Now)
                                            )

            ///Replaces location of existing object with new one.
            static member addLocation
                (location:int) (table:SubstitutionModification) =
                table.Location <- Nullable(location)
                table

            ///Replaces monoisotopicmassdelta of existing object with new one.
            static member addMonoIsotopicMassDelta
                (monoIsotopicMassDelta:float) (table:SubstitutionModification) =
                table.MonoIsotopicMassDelta <- Nullable(monoIsotopicMassDelta)
                table

            ///Replaces avgmassdelta of existing object with new one.
            static member addAvgMassDelta
                (avgMassDelta:float) (table:SubstitutionModification) =
                table.AvgMassDelta <- Nullable(avgMassDelta)
                table

            ///Replaces fkPeptide of existing object with new one.
            static member addFKPeptide
                (fkPeptide:string) (table:SubstitutionModification) =
                table.FKPeptide <- fkPeptide
                table

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
                item1.AvgMassDelta=item2.AvgMassDelta && item1.Location=item2.Location && 
                item1.FKPeptide=item2.FKPeptide 

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
                    ?fkMzIdentML               : string,
                    ?details                   : seq<PeptideParam>
                ) =
                let id'                        = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                      = defaultArg name Unchecked.defaultof<string>
                let modifications'             = convertOptionToList modifications
                let substitutionModifications' = convertOptionToList substitutionModifications
                let fkMzIdentML'               = defaultArg fkMzIdentML Unchecked.defaultof<string>
                let details'                   = convertOptionToList details

                new Peptide(
                            id', 
                            name', 
                            peptideSequence, 
                            modifications', 
                            substitutionModifications', 
                            fkMzIdentML', 
                            details', 
                            Nullable(DateTime.Now)
                           )

            ///Replaces name of existing object with new one.
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

            ///Replaces fkMzIdentML of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (peptide:Peptide) =
                let result = peptide.FKMzIdentMLDocument <- fkMzIdentML
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
                item1.FKMzIdentMLDocument=item2.FKMzIdentMLDocument && matchCVParamBases 
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
                    ?id                                 : string,
                    ?name                               : string,
                    ?fkSpectrumIdentificationProtocol   : string,
                    ?details                            : seq<TranslationTableParam>
                ) =
                let id'                                 = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                               = defaultArg name Unchecked.defaultof<string>
                let fkSpectrumIdentificationProtocol'   = defaultArg fkSpectrumIdentificationProtocol Unchecked.defaultof<string>
                let details'                            = convertOptionToList details
                    
                new TranslationTable(
                                     id', 
                                     name',
                                     fkSpectrumIdentificationProtocol',
                                     details', 
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:TranslationTable) =
                table.Name <- name
                table

            ///Replaces fkSpectrumIdentificationProtocol of existing object with new one.
            static member addFKSpectrumIdentificationProtocol
                (fkSpectrumIdentificationProtocol:string) (table:TranslationTable) =
                table.FKSpectrumIdentificationProtocol <- fkSpectrumIdentificationProtocol
                table

            ///Adds a translationtableparam to an existing translationtable-object.
            static member addDetail
                (detail:TranslationTableParam) (table:TranslationTable) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of translationtableparams to an existing translationtable-object.
            static member addDetails
                (details:seq<TranslationTableParam>) (table:TranslationTable) =
                let result = table.Details <- addCollectionToList table.Details details
                table

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
                item1.FKSpectrumIdentificationProtocol=item2.FKSpectrumIdentificationProtocol &&
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
                    details                         : seq<MeasureParam>,
                    ?id                             : string,
                    ?name                           : string,
                    ?fkSpectrumIdentificationList   : string
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                           = defaultArg name Unchecked.defaultof<string>
                let fkSpectrumIdentificationList'   = defaultArg fkSpectrumIdentificationList Unchecked.defaultof<string>
                    
                new Measure(
                            id', 
                            name',
                            fkSpectrumIdentificationList',
                            details |> List, Nullable(DateTime.Now)
                           )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:Measure) =
                table.Name <- name
                table

            ///Replaces fkSpectrumIdentificationList of existing object with new one.
            static member addFKSpectrumIdentificationList
                (fkSpectrumIdentificationList:string) (table:Measure) =
                table.FKSpectrumIdentificationList <- fkSpectrumIdentificationList
                table

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
                item1.FKSpectrumIdentificationList=item2.FKSpectrumIdentificationList &&
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
                    code            : string,
                    mass            : float,
                    ?id             : string,
                    ?fkMassTable    : string
                ) =
                let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                let fkMassTable'    = defaultArg fkMassTable Unchecked.defaultof<string>

                new Residue(
                            id', 
                            code, 
                            Nullable(mass),
                            fkMassTable',
                            Nullable(DateTime.Now)
                           )

            ///Replaces fkMassTable of existing object with new one.
            static member addFKMassTable
                (fkMassTable:string) (table:Residue) =
                table.FKMassTable <- fkMassTable
                table

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
                item1.Mass=item2.Mass && item1.FKMassTable=item2.FKMassTable

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
                    code            : string,
                    details         : seq<AmbiguousResidueParam>,
                    ?id             : string,
                    ?fkMassTable    : string
                ) =
                let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                let fkMassTable'    = defaultArg fkMassTable Unchecked.defaultof<string>
                    
                new AmbiguousResidue(
                                     id', 
                                     code,
                                     fkMassTable',
                                     details |> List, 
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces fkMassTable of existing object with new one.
            static member addFKMassTable
                (fkMassTable:string) (table:AmbiguousResidue) =
                table.FKMassTable <- fkMassTable
                table

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

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:MassTable) =
                table.Name <- name
                table

            ///Adds a residue to an existing masstable-object.
            static member addResidue
                (residue:Residue) (table:MassTable) =
                let result = table.Residues <- addToList table.Residues residue
                table

            ///Adds a collection of residues to an existing masstable-object.
            static member addResidues
                (residues:seq<Residue>) (table:MassTable) =
                let result = table.Residues <- addCollectionToList table.Residues residues
                table

            ///Adds a ambiguousresidue to an existing masstable-object.
            static member addAmbiguousResidue
                (ambiguousResidues:AmbiguousResidue) (table:MassTable) =
                let result = table.AmbiguousResidues <- addToList table.AmbiguousResidues ambiguousResidues
                table

            ///Adds a collection of ambiguousresidues to an existing masstable-object.
            static member addAmbiguousResidues
                (ambiguousResidues:seq<AmbiguousResidue>) (table:MassTable) =
                let result = table.AmbiguousResidues <- addCollectionToList table.AmbiguousResidues ambiguousResidues
                table

            ///Adds a masstableparam to an existing masstable-object.
            static member addDetail
                (detail:MassTableParam) (table:MassTable) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of masstableparams to an existing masstable-object.
            static member addDetails
                (details:seq<MassTableParam>) (table:MassTable) =
                let result = table.Details <- addCollectionToList table.Details details
                table

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
                item1.MSLevel=item2.MSLevel &&
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

        //type ValueHandler =
        //    ///Initializes a value-object with at least all necessary parameters.
        //    static member init
        //        (
        //            value   : float,
        //            ?id     : string
        //        ) =
        //        let id' = defaultArg id (System.Guid.NewGuid().ToString())
                    
        //        new Value(
        //                  id', 
        //                  Nullable(value), 
        //                  Nullable(DateTime.Now)
        //                 )

        //    //Tries to find a value-object in the context and database, based on its primary-key(ID).
        //    static member tryFindByID (dbContext:MzIdentML) (id:string) =
        //        query {
        //               for i in dbContext.Value.Local do
        //                   if i.ID=id
        //                      then select i
        //              }
        //        |> (fun value -> 
        //            if (Seq.exists (fun value' -> value' <> null) value) = false
        //                then 
        //                    query {
        //                           for i in dbContext.Value do
        //                               if i.ID=id
        //                                  then select i
        //                          }
        //                    |> (fun value -> if (Seq.exists (fun value' -> value' <> null) value) = false
        //                                        then None
        //                                        else Some (value.Single())
        //                       )
        //                else Some (value.Single())
        //           )

        //    ///Tries to find a cvparam-object in the context and database, based on its 2nd most unique identifier.
        //    static member tryFindByValue (dbContext:MzIdentML) (item:Nullable<float>) =
        //        query {
        //               for i in dbContext.Value.Local do
        //                   if i.Value=item
        //                      then select i
        //              }
        //        |> (fun value -> 
        //            if (Seq.exists (fun value' -> value' <> null) value) = false
        //                then 
        //                    query {
        //                           for i in dbContext.Value do
        //                               if i.Value=item
        //                                  then select i
        //                          }
        //                    |> Seq.map (fun (value) -> value)
        //                    |> (fun value -> if (Seq.exists (fun value' -> value' <> null) value) = false
        //                                                    then None
        //                                                    else Some value
        //                       )
        //                else Some value
        //           )

        //    ///Checks whether all other fields of the current object and context object have the same values or not.
        //    static member private hasEqualFieldValues (item1:Value) (item2:Value) =
        //        item1.ID = item2.ID

        //    ///First checks if any object with same field-values (except primary key) exists within the context or database. 
        //    ///If no entry exists, a new object is added to the context and otherwise does nothing.
        //    static member addToContext (dbContext:MzIdentML) (item:Value) =
        //            ValueHandler.tryFindByValue dbContext item.Value
        //            |> (fun organizationCollection -> match organizationCollection with
        //                                              |Some x -> x
        //                                                         |> Seq.map (fun organization -> match ValueHandler.hasEqualFieldValues organization item with
        //                                                                                         |true -> true
        //                                                                                         |false -> false
        //                                                                    )
        //                                                                    |> (fun collection -> 
        //                                                                         if Seq.contains true collection=true
        //                                                                            then None
        //                                                                            else Some(dbContext.Add item)
        //                                                                       )
        //                                              |None -> Some(dbContext.Add item)
        //               )

        //    ///First checks if any object with same field-values (except primary key) exists within the context or database. 
        //    ///If no entry exists, a new object is first added to the context and then to the database and otherwise does nothing.
        //    static member addToContextAndInsert (dbContext:MzIdentML) (item:Value) =
        //        ValueHandler.addToContext dbContext item |> ignore
        //        dbContext.SaveChanges()

        type FragmentArrayHandler =
            ///Initializes a fragmentarray-object with at least all necessary parameters.
            static member init
                (
                    fkMeasure   : string,
                    value       : float,
                    ?id         : string,
                    ?measure    : Measure,
                    ?fkIonType  : string
                ) =
                let id'         = defaultArg id (System.Guid.NewGuid().ToString())
                let measure'    = defaultArg measure Unchecked.defaultof<Measure>
                let fkIonType'  = defaultArg fkIonType Unchecked.defaultof<string>

                new FragmentArray(
                                  id', 
                                  measure',
                                  fkMeasure,
                                  Nullable(value),
                                  fkIonType',
                                  Nullable(DateTime.Now)
                                 )

            ///Replaces measure of existing object with new one.
            static member addMeasure
                (measure:Measure) (table:FragmentArray) =
                table.Measure <- measure
                table

            ///Replaces fkIonType of existing object with new one.
            static member addFKIonType
                (fkIonType:string) (table:FragmentArray) =
                table.FKIonType <- fkIonType
                table

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
                item1.FKMeasure=item2.FKMeasure && item1.FKIonType=item2.FKIonType

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
                    index       : int,
                    ?id         : string,
                    ?fkIonType  : string
                ) =
                let id'         = defaultArg id (System.Guid.NewGuid().ToString())
                let fkIonType'  = defaultArg fkIonType Unchecked.defaultof<string>

                new Index(
                          id', 
                          Nullable(index),
                          fkIonType',
                          Nullable(DateTime.Now)
                         )

            ///Replaces fkIonType of existing object with new one.
            static member addFKIonType
                (fkIonType:string) (table:Index) =
                table.FKIonType <- fkIonType
                table

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
            static member tryFindByFKIonType (dbContext:MzIdentML) (fkIonType:string) =
                query {
                       for i in dbContext.Index.Local do
                           if i.FKIonType=fkIonType
                              then select i
                      }
                |> (fun index -> 
                    if (Seq.exists (fun index' -> index' <> null) index) = false
                        then 
                            query {
                                   for i in dbContext.Index do
                                       if i.FKIonType=fkIonType
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
                item1.Index=item2.Index

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Index) =
                    IndexHandler.tryFindByFKIonType dbContext item.FKIonType
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
                    details                         : seq<IonTypeParam>,
                    ?id                             : string,
                    ?index                          : seq<Index>,
                    ?fragmentArray                  : seq<FragmentArray>,
                    ?fkSpectrumIdentificationItem   : string
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let index'                          = convertOptionToList index
                let fragmentArray'                  = convertOptionToList fragmentArray
                let fkSpectrumIdentificationItem'   = defaultArg fkSpectrumIdentificationItem Unchecked.defaultof<string>
                    
                new IonType(
                            id', 
                            index', 
                            fragmentArray',
                            fkSpectrumIdentificationItem',
                            details |> List, Nullable(DateTime.Now)
                           )

            ///Adds a index to an existing iontype-object.
            static member addIndex
                (index:Index) (table:IonType) =
                let result = table.Index <- addToList table.Index index
                table

            ///Adds a collection of indexes to an existing iontype-object.
            static member addIndexes
                (index:seq<Index>) (table:IonType) =
                let result = table.Index <- addCollectionToList table.Index index
                table

            ///Adds a fragmentarray to an existing iontype-object.
            static member addFragmentArray
                (fragmentArray:FragmentArray) (table:IonType) =
                let result = table.FragmentArrays <- addToList table.FragmentArrays fragmentArray
                table

            ///Adds a collection of fragmentarrays to an existing iontype-object.
            static member addFragmentArrays
                (fragmentArrays:seq<FragmentArray>) (table:IonType) =
                let result = table.FragmentArrays <- addCollectionToList table.FragmentArrays fragmentArrays
                table

            ///Replaces fkSpectrumIdentificationItem of existing object with new one.
            static member addFKSpectrumIdentificationItem
                (fkSpectrumIdentificationItem:string) (table:IonType) =
                table.FKSpectrumIdentificationItem <- fkSpectrumIdentificationItem
                table

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
            static member tryFindByFKSpectrumIdentificationItem (dbContext:MzIdentML) (fkSpectrumIdentificationItem:string) =
                query {
                       for i in dbContext.IonType.Local do
                           if i.FKSpectrumIdentificationItem=fkSpectrumIdentificationItem
                              then select (i, i.FragmentArrays, i.Details)
                      }
                |> Seq.map (fun (ionType, _, _) -> ionType)
                |> (fun ionType -> 
                    if (Seq.exists (fun ionType' -> ionType' <> null) ionType) = false
                        then 
                            query {
                                   for i in dbContext.IonType do
                                       if i.FKSpectrumIdentificationItem=fkSpectrumIdentificationItem
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
                matchCVParamBases 
                    (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                    (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List)

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:IonType) =
                    IonTypeHandler.tryFindByFKSpectrumIdentificationItem dbContext item.FKSpectrumIdentificationItem
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
                    location                        : string,
                    fkFileFormat                    : string,
                    fkSpectrumIDFormat              : string,
                    ?id                             : string,
                    ?name                           : string,
                    ?externalFormatDocumentation    : string,
                    ?fileFormat                     : CVParam,
                    ?spectrumIDFormat               : CVParam,
                    ?fkInputs                       : string
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                           = defaultArg name Unchecked.defaultof<string>
                let externalFormatDocumentation'    = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let fileFormat'                     = defaultArg fileFormat Unchecked.defaultof<CVParam>
                let spectrumIDFormat'               = defaultArg spectrumIDFormat Unchecked.defaultof<CVParam>
                let fkInputs'                       = defaultArg fkInputs Unchecked.defaultof<string>

                new SpectraData(
                                id', 
                                name', 
                                location, 
                                externalFormatDocumentation', 
                                fileFormat',
                                fkFileFormat,
                                spectrumIDFormat',
                                fkSpectrumIDFormat,
                                fkInputs',
                                Nullable(DateTime.Now)
                               )

            ///Replaces existing with new one.
            static member addName
                (name:string) (table:SpectraData) =
                table.Name <- name
                table

            ///Replaces externalformatdocumentation with new one.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (table:SpectraData) =
                table.ExternalFormatDocumentation <- externalFormatDocumentation
                table

            ///Replaces fileFormat object with new one.
            static member addFileFormat
                (fileFormat:CVParam) (table:SpectraData) =
                table.FileFormat <- fileFormat
                table

            ///Replaces fkSpectrumIDFormat with new one.
            static member addSpectrumIDFormat
                (fkSpectrumIDFormat:CVParam) (table:SpectraData) =
                table.SpectrumIDFormat <- fkSpectrumIDFormat
                table

            ///Replaces fkInputs with new one.
            static member addFKInputs
                (fkInputs:string) (table:SpectraData) =
                table.FKInputs <- fkInputs
                table

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
                item1.FKSpectrumIDFormat = item2.FKSpectrumIDFormat &&
                item1.FKFileFormat = item2.FKFileFormat && item1.FKInputs = item2.FKInputs

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
                    fixedMod                            : bool,
                    massDelta                           : float,
                    residues                            : string,
                    fkSpectrumIdentificationProtocol    : string,
                    details                             : seq<SearchModificationParam>,
                    ?id                                 : string,
                    ?specificityRules                   : seq<SpecificityRuleParam>
                ) =
                let id'                 = defaultArg id (System.Guid.NewGuid().ToString())
                let specificityRules'   = convertOptionToList specificityRules
                    
                new SearchModification(
                                       id', 
                                       Nullable(fixedMod), 
                                       Nullable(massDelta), 
                                       residues, 
                                       specificityRules', 
                                       fkSpectrumIdentificationProtocol,
                                       details |> List, 
                                       Nullable(DateTime.Now)
                                      )

            ///Adds a specificityruleparam to an existing searchmodification-object.
            static member addSpecificityRule
                (specificityRule:SpecificityRuleParam) (table:SearchModification) =
                let result = table.SpecificityRules <- addToList table.SpecificityRules specificityRule
                table

            ///Adds a collection of specificityruleparams to an existing searchmodification-object.
            static member addSpecificityRules
                (specificityRules:seq<SpecificityRuleParam>) (table:SearchModification) =
                let result = table.SpecificityRules <- addCollectionToList table.SpecificityRules specificityRules
                table

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
                item1.SpecificityRules=item2.SpecificityRules && 
                item1.FKSpectrumIdentificationProtocol=item2.FKSpectrumIdentificationProtocol &&
                matchCVParamBases 
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
                    ?id                                 : string,
                    ?name                               : string,
                    ?cTermGain                          : string,
                    ?nTermGain                          : string,
                    ?minDistance                        : int,
                    ?missedCleavages                    : int,
                    ?semiSpecific                       : bool,
                    ?siteRegexc                         : string,
                    ?enzymeName                         : seq<EnzymeNameParam>,
                    ?fkSpectrumIdentificationProtocol   : string
                ) =
                let id'                                 = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                               = defaultArg name Unchecked.defaultof<string>
                let cTermGain'                          = defaultArg cTermGain Unchecked.defaultof<string>
                let nTermGain'                          = defaultArg nTermGain Unchecked.defaultof<string>
                let minDistance'                        = defaultArg minDistance Unchecked.defaultof<int>
                let missedCleavages'                    = defaultArg missedCleavages Unchecked.defaultof<int>
                let semiSpecific'                       = defaultArg semiSpecific Unchecked.defaultof<bool>
                let siteRegexc'                         = defaultArg siteRegexc Unchecked.defaultof<string>
                let enzymeName'                         = convertOptionToList enzymeName
                let fkSpectrumIdentificationProtocol'   = defaultArg fkSpectrumIdentificationProtocol Unchecked.defaultof<string>
                    
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
                           fkSpectrumIdentificationProtocol',
                           Nullable(DateTime.Now)
                          )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:Enzyme) =
                table.Name <- name
                table

            ///Replaces ctermgain of existing object with new one.
            static member addCTermGain
                (cTermGain:string) (table:Enzyme) =
                table.CTermGain <- cTermGain
                table

            ///Replaces ntermgain of existing object with new one.
            static member addNTermGain
                (nTermGain:string) (table:Enzyme) =
                table.NTermGain <- nTermGain
                table

            ///Replaces mindistance of existing object with new one.
            static member addMinDistance
                (minDistance:int) (table:Enzyme) =
                table.MinDistance <- Nullable(minDistance)
                table

            ///Replaces missedcleavages of existing object with new one.
            static member addMissedCleavages
                (missedCleavages:int) (table:Enzyme) =
                table.MissedCleavages <-Nullable( missedCleavages)
                table

            ///Replaces semispecific of existing object with new one.
            static member addSemiSpecific
                (semiSpecific:bool) (table:Enzyme) =
                table.SemiSpecific <- Nullable(semiSpecific)
                table

            ///Replaces siteregexc of existing object with new one.
            static member addSiteRegexc
                (siteRegexc:string) (table:Enzyme) =
                table.SiteRegexc <- siteRegexc
                table

            ///Adds new enzymename to collection of enzymenames.
            static member addEnzymeName
                (enzymeName:EnzymeNameParam) (table:Enzyme) =
                let result = table.EnzymeName <- addToList table.EnzymeName enzymeName
                table

            ///Add new collection of enzymenames to collection of enzymenames.
            static member addEnzymeNames
                (enzymeNames:seq<EnzymeNameParam>) (table:Enzyme) =
                let result = table.EnzymeName <- addCollectionToList table.EnzymeName enzymeNames
                table

            ///Replaces fkSpectrumIdentificationProtocol of existing object with new one.
            static member addFKSpectrumIdentificationProtocol
                (fkSpectrumIdentificationProtocol:string) (table:Enzyme) =
                table.FKSpectrumIdentificationProtocol <- fkSpectrumIdentificationProtocol
                table

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
                item1.SiteRegexc=item2.SiteRegexc && item1.EnzymeName=item2.EnzymeName &&
                item1.FKSpectrumIdentificationProtocol=item2.FKSpectrumIdentificationProtocol

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
                    fkFilterType                        : string,
                    ?id                                 : string,
                    ?filterType                         : CVParam,
                    ?includes                           : seq<IncludeParam>,
                    ?excludes                           : seq<ExcludeParam>,
                    ?fkSpectrumIdentificationProtocol   : string
                ) =
                let id'                                 = defaultArg id (System.Guid.NewGuid().ToString())
                let filterType'                         = defaultArg filterType Unchecked.defaultof<CVParam>
                let includes'                           = convertOptionToList includes
                let excludes'                           = convertOptionToList excludes
                let fkSpectrumIdentificationProtocol'   = defaultArg fkSpectrumIdentificationProtocol Unchecked.defaultof<string>
                    
                new Filter(
                           id', 
                           filterType',
                           fkFilterType,
                           includes', 
                           excludes',
                           fkSpectrumIdentificationProtocol',
                           Nullable(DateTime.Now)
                          )

            ///Adds a includeparam to an existing filter-object.
            static member addInclude
                (include':IncludeParam) (table:Filter) =
                let result = table.Includes <- addToList table.Includes include'
                table

            ///Adds a collection of includeparams to an existing filter-object.
            static member addIncludes
                (includes:seq<IncludeParam>) (table:Filter) =
                let result = table.Includes <- addCollectionToList table.Includes includes
                table

            ///Adds a excludeparam to an existing filter-object.
            static member addExclude
                (exclude':ExcludeParam) (table:Filter) =
                let result = table.Excludes <- addToList table.Excludes exclude'
                table

            ///Adds a collection of excludeparams to an existing filter-object.
            static member addExcludes
                (excludes:seq<ExcludeParam>) (table:Filter) =
                let result = table.Excludes <- addCollectionToList table.Excludes excludes
                table

            ///Replaces filterType of existing object with new one.
            static member addFilterType
                (filterType:CVParam) (table:Filter) =
                table.FilterType <- filterType
                table

            ///Replaces fkSpectrumIdentificationProtocol of existing object with new one.
            static member addFKSpectrumIdentificationProtocol
                (fkSpectrumIdentificationProtocol:string) (table:Filter) =
                table.FKSpectrumIdentificationProtocol <- fkSpectrumIdentificationProtocol
                table

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
            static member tryFindByFKSpectrumIdentificationProtocol (dbContext:MzIdentML) (fkFilterType:string) =
                query {
                       for i in dbContext.Filter.Local do
                           if i.FKFilterType=fkFilterType
                              then select (i, i.FilterType, i.Includes, i.Excludes)
                      }
                |> Seq.map (fun (filter, _, _, _) -> filter)
                |> (fun enzyme -> 
                    if (Seq.exists (fun enzyme' -> enzyme' <> null) enzyme) = false
                        then 
                            query {
                                   for i in dbContext.Filter do
                                       if i.FKFilterType=fkFilterType
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
                item1.Includes=item2.Includes && item1.Excludes=item2.Excludes &&
                item1.FKSpectrumIdentificationProtocol=item2.FKSpectrumIdentificationProtocol

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:Filter) =
                    FilterHandler.tryFindByFKSpectrumIdentificationProtocol dbContext item.FilterType.Term.Name
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
                    frame                               : int,
                    ?id                                 : string,
                    ?fkSpectrumIdentificationProtocol   : string
                ) =
                let id'                                 = defaultArg id (System.Guid.NewGuid().ToString())
                let fkSpectrumIdentificationProtocol'   = defaultArg fkSpectrumIdentificationProtocol Unchecked.defaultof<string>
                    
                new Frame(
                          id', 
                          Nullable(frame),
                          fkSpectrumIdentificationProtocol',
                          Nullable(DateTime.Now)
                         )

            ///Replaces fkSpectrumIdentificationProtocol of existing object with new one.
            static member addFKSpectrumIdentificationProtocol
                (fkSpectrumIdentificationProtocol:string) (table:Frame) =
                table.FKSpectrumIdentificationProtocol <- fkSpectrumIdentificationProtocol
                table

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
                item1.FKSpectrumIdentificationProtocol = item2.FKSpectrumIdentificationProtocol

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
                    fkAnalysisSoftware      : string,
                    fkSearchType            : string,
                    threshold               : seq<ThresholdParam>,
                    ?id                     : string,
                    ?name                   : string,
                    ?analysisSoftware       : AnalysisSoftware,
                    ?searchType             : CVParam,
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
                let analysisSoftware'       = defaultArg analysisSoftware Unchecked.defaultof<AnalysisSoftware>
                let searchType'             = defaultArg searchType Unchecked.defaultof<CVParam>
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
                                                   analysisSoftware',
                                                   fkAnalysisSoftware,
                                                   searchType', 
                                                   fkSearchType,
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

            ///Replaces fkAnalysisSoftware of existing object with new one.
            static member addFKAnalysisSoftware
                (fkAnalysisSoftware:string) (table:SpectrumIdentificationProtocol) =
                table.FKAnalysisSoftware <- fkAnalysisSoftware
                table

            ///Replaces fkSearchType of existing object with new one.
            static member addFKSearchType
                (fkSearchType:string) (table:SpectrumIdentificationProtocol) =
                table.FKSearchType <- fkSearchType
                table

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (table:SpectrumIdentificationProtocol) =
                table.Name <- name
                table

            ///Adds a additionalsearchparam to an existing spectrumIdentificationprotocol-object.
            static member addAdditionalSearchParam
                (additionalSearchParam:AdditionalSearchParam) (table:SpectrumIdentificationProtocol) =
                let result = table.AdditionalSearchParams <- addToList table.AdditionalSearchParams additionalSearchParam
                table

            ///Adds a collection of additionalsearchparams to an existing spectrumIdentificationprotocol-object.
            static member addAdditionalSearchParams
                (additionalSearchParams:seq<AdditionalSearchParam>) (table:SpectrumIdentificationProtocol) =
                let result = table.AdditionalSearchParams <- addCollectionToList table.AdditionalSearchParams additionalSearchParams
                table

            ///Adds a modificationparam to an existing spectrumIdentificationprotocol-object.
            static member addModificationParam
                (modificationParam:SearchModification) (table:SpectrumIdentificationProtocol) =
                let result = table.ModificationParams <- addToList table.ModificationParams modificationParam
                table

            ///Adds a collection of modificationparams to an existing spectrumIdentificationprotocol-object.
            static member addModificationParams
                (modificationParams:seq<SearchModification>) (table:SpectrumIdentificationProtocol) =
                let result = table.ModificationParams <- addCollectionToList table.ModificationParams modificationParams
                table

            ///Adds a enzyme to an existing spectrumIdentificationprotocol-object.
            static member addEnzyme
                (enzyme:Enzyme) (table:SpectrumIdentificationProtocol) =
                let result = table.Enzymes <- addToList table.Enzymes enzyme
                table

            ///Adds a collection of enzymes to an existing spectrumIdentificationprotocol-object.
            static member addEnzymes
                (enzymes:seq<Enzyme>) (table:SpectrumIdentificationProtocol) =
                let result = table.Enzymes <- addCollectionToList table.Enzymes enzymes
                table

            ///Replaces independent_enzymes of existing object with new independent_enzymes.
            static member addIndependent_Enzymes
                (independent_Enzymes:bool) (table:SpectrumIdentificationProtocol) =
                table.Independent_Enzymes <- Nullable(independent_Enzymes)
                table

            ///Adds a masstable to an existing spectrumIdentificationprotocol-object.
            static member addMassTable
                (massTable:MassTable) (table:SpectrumIdentificationProtocol) =
                let result = table.MassTables <- addToList table.MassTables massTable
                table

            ///Adds a collection of masstables to an existing spectrumIdentificationprotocol-object.
            static member addMassTables
                (massTables:seq<MassTable>) (table:SpectrumIdentificationProtocol) =
                let result = table.MassTables <- addCollectionToList table.MassTables massTables
                table

            ///Adds a fragmenttolerance to an existing spectrumIdentificationprotocol-object.
            static member addFragmentTolerance
                (fragmentTolerance:FragmentToleranceParam) (table:SpectrumIdentificationProtocol) =
                let result = table.FragmentTolerance <- addToList table.FragmentTolerance fragmentTolerance
                table

            ///Adds a collection of fragmenttolerances to an existing spectrumIdentificationprotocol-object.
            static member addFragmentTolerances
                (fragmentTolerances:seq<FragmentToleranceParam>) (table:SpectrumIdentificationProtocol) =
                let result = table.FragmentTolerance <- addCollectionToList table.FragmentTolerance fragmentTolerances
                table

            ///Adds a parenttolerance to an existing spectrumIdentificationprotocol-object.
            static member addParentTolerance
                (parentTolerance:ParentToleranceParam) (table:SpectrumIdentificationProtocol) =
                let result = table.ParentTolerance <- addToList table.ParentTolerance parentTolerance
                table

            ///Adds a collection of parenttolerances to an existing spectrumIdentificationprotocol-object.
            static member addParentTolerances
                (parentTolerances:seq<ParentToleranceParam>) (table:SpectrumIdentificationProtocol) =
                let result = table.ParentTolerance <- addCollectionToList table.ParentTolerance parentTolerances
                table

            ///Adds a databasefilter to an existing spectrumIdentificationprotocol-object.
            static member addDatabaseFilter
                (databaseFilter:Filter) (table:SpectrumIdentificationProtocol) =
                let result = table.DatabaseFilters <- addToList table.DatabaseFilters databaseFilter
                table

            ///Adds a collection of databasefilters to an existing spectrumIdentificationprotocol-object.
            static member addDatabaseFilters
                (databaseFilters:seq<Filter>) (table:SpectrumIdentificationProtocol) =
                let result = table.DatabaseFilters <- addCollectionToList table.DatabaseFilters databaseFilters
                table

            ///Adds a frame to an existing spectrumIdentificationprotocol-object.
            static member addFrame
                (frame:Frame) (table:SpectrumIdentificationProtocol) =
                let result = table.Frames <- addToList table.Frames frame
                table

            ///Adds a collection of frames to an existing spectrumIdentificationprotocol-object.
            static member addFrames
                (frames:seq<Frame>) (table:SpectrumIdentificationProtocol) =
                let result = table.Frames <- addCollectionToList table.Frames frames
                table

            ///Adds a translationtable to an existing spectrumIdentificationprotocol-object.
            static member addTranslationTable
                (translationTable:TranslationTable) (table:SpectrumIdentificationProtocol) =
                let result = table.TranslationTables <- addToList table.TranslationTables translationTable
                table

            ///Adds a collection of translationtables to an existing spectrumIdentificationprotocol-object.
            static member addTranslationTables
                (translationTables:seq<TranslationTable>) (table:SpectrumIdentificationProtocol) =
                let result = table.TranslationTables <- addCollectionToList table.TranslationTables translationTables
                table

            ///Replaces fkMzIdentML of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (table:SpectrumIdentificationProtocol) =
                let result = table.FKMzIdentMLDocument <- fkMzIdentML
                table

            ///Tries to find a spectrumIdentificationProtocol-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SpectrumIdentificationProtocol.Local do
                           if i.ID=id
                              then select (i, i.AnalysisSoftware, i.SearchType, i.Threshold, i.AdditionalSearchParams, 
                                           i.ModificationParams, i.Enzymes, i.MassTables, i.FragmentTolerance, 
                                           i.ParentTolerance, i.DatabaseFilters, i.Frames, i.TranslationTables, 
                                           i.FKMzIdentMLDocument
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
                                                       i.FKMzIdentMLDocument
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
                                           i.FKMzIdentMLDocument
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
                                                       i.FKMzIdentMLDocument
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
                item1.FKSearchType=item2.FKSearchType && item1.Threshold=item2.Threshold && item1.AdditionalSearchParams=item2.AdditionalSearchParams && 
                item1.ModificationParams=item2.ModificationParams && item1.Enzymes=item2.Enzymes && item1.MassTables=item2.MassTables && 
                item1.FragmentTolerance=item2.FragmentTolerance && item1.ParentTolerance=item2.ParentTolerance && 
                item1.DatabaseFilters=item2.DatabaseFilters && item1.Frames=item2.Frames && item1.TranslationTables=item2.TranslationTables && 
                item1.FKAnalysisSoftware=item2.FKAnalysisSoftware && item1.FKMzIdentMLDocument=item2.FKMzIdentMLDocument

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
                    location                        : string,
                    fkFileFormat                    : string,
                    fkDatabaseName                  : string,
                    ?id                             : string,
                    ?name                           : string,                    
                    ?numDatabaseSequences           : int64,
                    ?numResidues                    : int64,
                    ?releaseDate                    : DateTime,
                    ?fileFormat                     : CVParam,
                    ?databaseName                   : CVParam,
                    ?version                        : string,
                    ?externalFormatDocumentation    : string,
                    ?fkInputs                       : string,
                    ?details                        : seq<SearchDatabaseParam>
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                           = defaultArg name Unchecked.defaultof<string>
                let numDatabaseSequences'           = defaultArg numDatabaseSequences Unchecked.defaultof<int64>
                let numResidues'                    = defaultArg numResidues Unchecked.defaultof<int64>
                let releaseDate'                    = defaultArg releaseDate Unchecked.defaultof<DateTime>
                let fileFormat'                     = defaultArg fileFormat Unchecked.defaultof<CVParam>
                let databaseName'                   = defaultArg databaseName Unchecked.defaultof<CVParam>
                let version'                        = defaultArg version Unchecked.defaultof<string>
                let externalFormatDocumentation'    = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let fkInputs'                       = defaultArg fkInputs Unchecked.defaultof<string>
                let details'                        = convertOptionToList details
                    
                new SearchDatabase(
                                   id', 
                                   name', 
                                   location, 
                                   Nullable(numDatabaseSequences'), 
                                   Nullable(numResidues'), 
                                   Nullable(releaseDate'), 
                                   version',
                                   externalFormatDocumentation', 
                                   fileFormat', 
                                   fkFileFormat, 
                                   databaseName', 
                                   fkDatabaseName,
                                   fkInputs',
                                   details', 
                                   Nullable(DateTime.Now)
                                  )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:SearchDatabase) =
                table.Name <- name
                table

            ///Replaces numdatabasesequences of existing object with new one.
            static member addNumDatabaseSequences
                (numDatabaseSequences:int64) (table:SearchDatabase) =
                table.NumDatabaseSequences <- Nullable(numDatabaseSequences)
                table

            ///Replaces numresidues of existing object with new one.
            static member addNumResidues
                (numResidues:int64) (table:SearchDatabase) =
                table.NumResidues <- Nullable(numResidues)
                table

            ///Replaces releasedate of existing object with new one.
            static member addReleaseDate
                (releaseDate:DateTime) (table:SearchDatabase) =
                table.ReleaseDate <- Nullable(releaseDate)
                table

            ///Replaces databaseName of existing object with new one.
            static member addDatabaseName
                (databaseName:CVParam) (table:SearchDatabase) =
                table.DatabaseName <- databaseName
                table

            ///Replaces fileFormat of existing object with new one.
            static member addFileFormat
                (fileFormat:CVParam) (table:SearchDatabase) =
                table.FileFormat <- fileFormat
                table

            ///Replaces version of existing object with new one.
            static member addVersion
                (version:string) (table:SearchDatabase) =
                table.Version <- version
                table

            ///Replaces externalformatdocumentation of existing object with new one.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (table:SearchDatabase) =
                table.ExternalFormatDocumentation <- externalFormatDocumentation
                table

            ///Replaces inputs of existing object with new one.
            static member addInputs
                (fkInputs:string) (table:SearchDatabase) =
                table.FKInputs <- fkInputs
                table

            ///Adds a searchdatabaseparam to an existing searchdatabase-object.
            static member addDetail
                (detail:SearchDatabaseParam) (table:SearchDatabase) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of searchdatabaseparams to an existing searchdatabase-object.
            static member addDetails
                (details:seq<SearchDatabaseParam>) (table:SearchDatabase) =
                let result = table.Details <- addCollectionToList table.Details details
                table

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
               item1.FKInputs=item2.FKInputs && item1.FKFileFormat=item2.FKFileFormat &&
               item1.FKDatabaseName=item2.FKDatabaseName &&
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
                               fkMzIdentML',
                               details', 
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

            ///Replaces fkMzIdentML of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (dbSequence:DBSequence) =
                let result = dbSequence.FKMzIdentMLDocument <- fkMzIdentML
                dbSequence

            ///Tries to find a dbSequence-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.DBSequence.Local do
                           if i.ID=id
                              then select (i, i.SearchDatabase, i.FKMzIdentMLDocument, i.Details)
                      }
                |> Seq.map (fun (dbSequence, _, _, _) -> dbSequence)
                |> (fun dbSequence -> 
                    if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
                        then 
                            query {
                                   for i in dbContext.DBSequence do
                                       if i.ID=id
                                          then select (i, i.SearchDatabase, i.FKMzIdentMLDocument, i.Details)
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
                              then select (i, i.SearchDatabase, i.FKMzIdentMLDocument, i.Details)
                      }
                |> Seq.map (fun (dbSequence, _, _, _) -> dbSequence)
                |> (fun dbSequence -> 
                    if (Seq.exists (fun dbSequence' -> dbSequence' <> null) dbSequence) = false
                        then 
                            query {
                                   for i in dbContext.DBSequence do
                                       if i.Accession=accession
                                          then select (i, i.SearchDatabase, i.FKMzIdentMLDocument, i.Details)
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
               matchCVParamBases (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) && item1.FKMzIdentMLDocument=item2.FKMzIdentMLDocument &&
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
                    fkDBSequence                    : string,
                    fkPeptide                       : string,
                    ?id                             : string,
                    ?name                           : string,
                    ?dbSequence                     : DBSequence,
                    ?peptide                        : Peptide,
                    ?start                          : int,
                    ?end'                           : int,
                    ?pre                            : string,
                    ?post                           : string,
                    ?frame                          : Frame,
                    ?isDecoy                        : bool,
                    ?translationTable               : TranslationTable,
                    ?fkTranslationTable             : string,
                    ?fkSpectrumIdentificationItem   : string,
                    ?fkMzIdentML                    : string,
                    ?details                        : seq<PeptideEvidenceParam>
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                           = defaultArg name Unchecked.defaultof<string>
                let dbSequence'                     = defaultArg dbSequence Unchecked.defaultof<DBSequence>
                let peptide'                        = defaultArg peptide Unchecked.defaultof<Peptide>
                let start'                          = defaultArg start Unchecked.defaultof<int>
                let end''                           = defaultArg end' Unchecked.defaultof<int>
                let pre'                            = defaultArg pre Unchecked.defaultof<string>
                let post'                           = defaultArg post Unchecked.defaultof<string>
                let frame'                          = defaultArg frame Unchecked.defaultof<Frame>
                let isDecoy'                        = defaultArg isDecoy Unchecked.defaultof<bool>
                let translationTable'               = defaultArg translationTable Unchecked.defaultof<TranslationTable>
                let fkTranslationTable'             = defaultArg fkTranslationTable Unchecked.defaultof<string>
                let fkSpectrumIdentificationItem'   = defaultArg fkSpectrumIdentificationItem Unchecked.defaultof<string>
                let details'                        = convertOptionToList details
                let fkMzIdentML'                    = defaultArg fkMzIdentML Unchecked.defaultof<string>
                    
                new PeptideEvidence(
                                    id', 
                                    name', 
                                    dbSequence', 
                                    fkDBSequence,
                                    peptide', 
                                    fkPeptide,
                                    Nullable(start'), 
                                    Nullable(end''), 
                                    pre', 
                                    post', 
                                    frame', 
                                    Nullable(isDecoy'), 
                                    translationTable', 
                                    fkTranslationTable', 
                                    fkSpectrumIdentificationItem',
                                    fkMzIdentML', 
                                    details', 
                                    Nullable(DateTime.Now)
                                   )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (table:PeptideEvidence) =
                table.Name <- name
                table

            ///Replaces dbSequence of existing object with new one.
            static member addDBSequence
                (dbSequence:DBSequence) (table:PeptideEvidence) =
                table.DBSequence <- dbSequence
                table

            ///Replaces peptide of existing object with new one.
            static member addPeptide
                (peptide:Peptide) (table:PeptideEvidence) =
                table.Peptide <- peptide
                table

            ///Replaces start of existing object with new start.
            static member addStart
                (start:int) (table:PeptideEvidence) =
                table.Start <- Nullable(start)
                table

            ///Replaces end of existing object with new end.
            static member addEnd 
                (end':int) (table:PeptideEvidence) =
                table.End  <- Nullable(end')
                table

            ///Replaces pre of existing object with new pre.
            static member addPre
                (pre:string) (table:PeptideEvidence) =
                table.Pre <- pre
                table

            ///Replaces post of existing object with new post.
            static member addPost
                (post:string) (table:PeptideEvidence) =
                table.Post <- post
                table

            ///Replaces frame of existing object with new frame.
            static member addFrame
                (frame:Frame) (table:PeptideEvidence) =
                table.Frame <- frame
                table

            ///Replaces isdecoy of existing object with new isdecoy.
            static member addIsDecoy
                (isDecoy:bool) (table:PeptideEvidence) =
                table.IsDecoy <- Nullable(isDecoy)
                table

            ///Replaces translationtable of existing object with new translationtable.
            static member addTranslationTable
                (translationTable:TranslationTable) (table:PeptideEvidence) =
                table.TranslationTable <- translationTable
                table

            ///Replaces fkSpectrumIdentificationItem of existing object with new one.
            static member addFKSpectrumIdentificationItem
                (fkSpectrumIdentificationItem:string) (table:PeptideEvidence) =
                table.FKSpectrumIdentificationItem <- fkSpectrumIdentificationItem
                table

            ///Adds a peptideevidenceparam to an existing peptideevidence-object.
            static member addDetail
                (detail:PeptideEvidenceParam) (table:PeptideEvidence) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of peptideevidenceparam to an existing peptideevidence-object.
            static member addDetails
                (details:seq<PeptideEvidenceParam>) (table:PeptideEvidence) =
                let result = table.Details <- addCollectionToList table.Details details
                table

            ///Replaces fkMzIdentML of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (table:PeptideEvidence) =
                let result = table.FKMzIdentMLDocument <- fkMzIdentML
                table

            ///Tries to find a peptideEvidence-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.PeptideEvidence.Local do
                           if i.ID=id
                              then select (i, i.Peptide, i.TranslationTable, i.DBSequence,  i.FKMzIdentMLDocument, i.Details)
                      }
                |> Seq.map (fun (peptideEvidence, _, _, _, _, _) -> peptideEvidence)
                |> (fun peptideEvidence -> 
                    if (Seq.exists (fun peptideEvidence' -> peptideEvidence' <> null) peptideEvidence) = false
                        then 
                            query {
                                   for i in dbContext.PeptideEvidence do
                                       if i.ID=id
                                          then select (i, i.Peptide, i.TranslationTable, i.DBSequence,  i.FKMzIdentMLDocument, i.Details)
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
                              then select (i, i.Peptide, i.TranslationTable, i.DBSequence,  i.FKMzIdentMLDocument, i.Details)
                      }
                |> Seq.map (fun (peptideEvidence, _, _, _, _, _) -> peptideEvidence)
                |> (fun peptideEvidence -> 
                    if (Seq.exists (fun peptideEvidence' -> peptideEvidence' <> null) peptideEvidence) = false
                        then 
                            query {
                                   for i in dbContext.PeptideEvidence do
                                       if i.Name=name
                                          then select (i, i.Peptide, i.TranslationTable, i.DBSequence,  i.FKMzIdentMLDocument, i.Details)
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
               item1.FKPeptide=item2.FKPeptide && item1.FKDBSequence=item2.FKDBSequence &&
               item1.Name=item2.Name && item1.Start=item2.Start && item1.End=item2.End &&
               item1.Pre=item2.Pre && item1.Post=item2.Post && item1.Frame=item2.Frame &&
               item1.IsDecoy=item2.IsDecoy && item1.TranslationTable=item2.TranslationTable && 
               matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) && 
               item1.FKMzIdentMLDocument=item2.FKMzIdentMLDocument

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
                    fkPeptide                       : string,
                    chargeState                     : int,
                    experimentalMassToCharge        : float,
                    passThreshold                   : bool,
                    rank                            : int,
                    ?id                             : string,
                    ?name                           : string,
                    ?sample                         : Sample,
                    ?fkSample                       : string,
                    ?massTable                      : MassTable,
                    ?fkMassTable                    : string,
                    ?peptideEvidences               : seq<PeptideEvidence>,
                    ?fragmentations                 : seq<IonType>,
                    ?peptide                        : Peptide,
                    ?calculatedMassToCharge         : float,
                    ?calculatedPI                   : float,
                    ?spectrumIdentificationResult   : SpectrumIdentificationResult,
                    ?fkSpectrumIdentificationResult : string,
                    ?fkPeptideHypothesis            : string,
                    ?details                        : seq<SpectrumIdentificationItemParam>
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                           = defaultArg name Unchecked.defaultof<string>
                let sample'                         = defaultArg sample Unchecked.defaultof<Sample>
                let fkSample'                       = defaultArg fkSample Unchecked.defaultof<string>
                let massTable'                      = defaultArg massTable Unchecked.defaultof<MassTable>
                let fkMassTable'                    = defaultArg fkMassTable Unchecked.defaultof<string>
                let peptideEvidences'               = convertOptionToList peptideEvidences
                let fragmentations'                 = convertOptionToList fragmentations
                let peptide'                        = defaultArg peptide Unchecked.defaultof<Peptide>
                let calculatedMassToCharge'         = defaultArg calculatedMassToCharge Unchecked.defaultof<float>
                let calculatedPI'                   = defaultArg calculatedPI Unchecked.defaultof<float>
                let spectrumIdentificationResult'   = defaultArg spectrumIdentificationResult Unchecked.defaultof<SpectrumIdentificationResult>
                let fkSpectrumIdentificationResult' = defaultArg fkSpectrumIdentificationResult Unchecked.defaultof<string>
                let fkPeptideHypothesis'            = defaultArg fkPeptideHypothesis Unchecked.defaultof<string>
                let details'                        = convertOptionToList details
                    
                new SpectrumIdentificationItem(
                                               id', 
                                               name', 
                                               sample', 
                                               fkSample',
                                               massTable', 
                                               fkMassTable',
                                               Nullable(passThreshold), 
                                               Nullable(rank), 
                                               peptideEvidences',
                                               fragmentations', 
                                               peptide',
                                               fkPeptide,
                                               Nullable(chargeState), 
                                               Nullable(experimentalMassToCharge), 
                                               Nullable(calculatedMassToCharge'),
                                               Nullable(calculatedPI'),
                                               spectrumIdentificationResult',
                                               fkSpectrumIdentificationResult',
                                               fkPeptideHypothesis',
                                               details', 
                                               Nullable(DateTime.Now)
                                              )

            ///Replaces name of existing object with new name.
            static member addName
                (name:string) (table:SpectrumIdentificationItem) =
                table.Name <- name
                table

            ///Replaces sample of existing object with new one.
            static member addSample
                (sample:Sample) (table:SpectrumIdentificationItem) =
                table.Sample <- sample 
                table

            ///Replaces masstable of existing object with new one.
            static member addMassTable
                (massTable:MassTable) (table:SpectrumIdentificationItem) =
                table.MassTable <- massTable
                table

            ///Replaces fkMassTable of existing object with new one.
            static member addFKMassTable
                (fkMassTable:string) (table:SpectrumIdentificationItem) =
                table.FKMassTable <- fkMassTable
                table

            ///Replaces peptide of existing object with new one.
            static member addPeptide
                (peptide:Peptide) (table:SpectrumIdentificationItem) =
                table.Peptide <- peptide
                table

            ///Adds a peptideevidence to an existing spectrumidentification-object.
            static member addPeptideEvidence
                (peptideEvidence:PeptideEvidence) (table:SpectrumIdentificationItem) =
                let result = table.PeptideEvidences <- addToList table.PeptideEvidences peptideEvidence
                table

            ///Adds a collection of peptideevidences to an existing spectrumidentification-object.
            static member addPeptideEvidences
                (peptideEvidences:seq<PeptideEvidence>) (table:SpectrumIdentificationItem) =
                let result = table.PeptideEvidences <- addCollectionToList table.PeptideEvidences peptideEvidences
                table   

            ///Adds a fragmentation to an existing spectrumidentification-object.
            static member addFragmentation
                (ionType:IonType) (table:SpectrumIdentificationItem) =
                let result = table.Fragmentations <- addToList table.Fragmentations ionType
                table

            ///Adds a collection of fragmentations to an existing spectrumidentification-object.
            static member addFragmentations
                (ionTypes:seq<IonType>) (table:SpectrumIdentificationItem) =
                let result = table.Fragmentations <- addCollectionToList table.Fragmentations ionTypes
                table 

           ///Replaces calculatedmasstocharge of existing object with new one.
            static member addCalculatedMassToCharge
                (calculatedMassToCharge:float) (table:SpectrumIdentificationItem) =
                table.CalculatedMassToCharge <- Nullable(calculatedMassToCharge)
                table

            ///Replaces calculatedpi of existing object with new one.
            static member addCalculatedPI
                (calculatedPI:float) (table:SpectrumIdentificationItem) =
                table.CalculatedPI <- Nullable(calculatedPI)
                table

            ///Replaces spectrumIdentificationResult of existing object with new one.
            static member addSpectrumIdentificationResult
                (spectrumIdentificationResult:SpectrumIdentificationResult) (table:SpectrumIdentificationItem) =
                table.SpectrumIdentificationResult <- spectrumIdentificationResult
                table

            ///Replaces fkSpectrumIdentificationResult of existing object with new one.
            static member addFKSpectrumIdentificationResult
                (fkSpectrumIdentificationResult:string) (table:SpectrumIdentificationItem) =
                table.FKSpectrumIdentificationResult <- fkSpectrumIdentificationResult
                table

            ///Replaces fkPeptideHypothesis of existing object with new one.
            static member addFKPeptideHypothesis
                (fkPeptideHypothesis:string) (table:SpectrumIdentificationItem) =
                table.FKPeptideHypothesis <- fkPeptideHypothesis
                table

            ///Adds a spectrumidentificationparam to an existing spectrumidentification-object.
            static member addDetail
                (detail:SpectrumIdentificationItemParam) (table:SpectrumIdentificationItem) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of spectrumidentificationparams to an existing spectrumidentification-object.
            static member addDetails
                (details:seq<SpectrumIdentificationItemParam>) (table:SpectrumIdentificationItem) =
                let result = table.Details <- addCollectionToList table.Details details
                table

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
               item1.Name=item2.Name && item1.FKSample=item2.FKSample && item1.FKMassTable=item2.FKMassTable && 
               item1.PassThreshold=item2.PassThreshold && item1.Rank=item2.Rank && item1.PeptideEvidences=item2.PeptideEvidences && 
               item1.Fragmentations=item2.Fragmentations && item1.CalculatedMassToCharge=item2.CalculatedMassToCharge && 
               item1.CalculatedPI=item2.CalculatedPI && matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) && 
               item1.ChargeState=item2.ChargeState && item1.FKPeptide=item2.FKPeptide &&
               item1.FKMassTable=item2.FKMassTable && item1.Peptide.PeptideSequence=item2.Peptide.PeptideSequence
               
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
                    fkSpectraData                   : string,
                    spectrumID                      : string,
                    spectrumIdentificationItem      : seq<SpectrumIdentificationItem>,
                    fkSpectrumIdentificationList    : string,
                    ?id                             : string,
                    ?name                           : string,
                    ?spectraData                    : SpectraData,
                    ?details                        : seq<SpectrumIdentificationResultParam>
                ) =
                let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'           = defaultArg name Unchecked.defaultof<string>
                let spectraData'    = defaultArg spectraData Unchecked.defaultof<SpectraData>
                let details'        = convertOptionToList details
                    
                new SpectrumIdentificationResult(
                                                 id', 
                                                 name', 
                                                 spectraData', 
                                                 fkSpectraData,
                                                 spectrumID, 
                                                 spectrumIdentificationItem |> List,
                                                 fkSpectrumIdentificationList,
                                                 details', 
                                                 Nullable(DateTime.Now)
                                                )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:SpectrumIdentificationResult)  =
                table.Name <- name
                table

            ///Replaces spectraData of existing object with new one.
            static member addSpectraData
                (spectraData:SpectraData) (table:SpectrumIdentificationResult)  =
                table.SpectraData <- spectraData
                table

            ///Replaces fkSpectrumIdentificationList of existing object with new one.
            static member addFKSpectrumIdentificationList
                (fkSpectrumIdentificationList:string) (table:SpectrumIdentificationResult)  =
                table.FKSpectrumIdentificationList <- fkSpectrumIdentificationList
                table

            ///Adds a spectrumidentificationresultparam to an existing spectrumidentificationresult-object.
            static member addDetail
                (detail:SpectrumIdentificationResultParam) (table:SpectrumIdentificationResult) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of spectrumidentificationresultparams to an existing spectrumidentificationresult-object.
            static member addDetails
                (details:seq<SpectrumIdentificationResultParam>) (table:SpectrumIdentificationResult) =
                let result = table.Details <- addCollectionToList table.Details details
                table

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
               item1.Name=item2.Name && item1.FKSpectraData=item2.FKSpectraData && matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) && 
               item1.SpectrumIdentificationItem=item2.SpectrumIdentificationItem &&
               item1.FKSpectrumIdentificationList=item2.FKSpectrumIdentificationList

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
                    spectrumIdentificationResult    : seq<SpectrumIdentificationResult>,
                    fkProteinDetection              : string,
                    fkAnalysisData                  : string,
                    ?id                             : string,
                    ?name                           : string,
                    ?numSequencesSearched           : int64,
                    ?fragmentationTable             : seq<Measure>,
                    ?details                        : seq<SpectrumIdentificationListParam>          
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                   = defaultArg name Unchecked.defaultof<string>
                let numSequencesSearched'   = defaultArg numSequencesSearched Unchecked.defaultof<int64>
                let fragmentationTable'     = convertOptionToList fragmentationTable
                let details'                = convertOptionToList details
                    
                new SpectrumIdentificationList(
                                               id', 
                                               name', 
                                               Nullable(numSequencesSearched'), 
                                               fragmentationTable', 
                                               spectrumIdentificationResult |> List,
                                               fkAnalysisData,
                                               fkProteinDetection,
                                               details',
                                               Nullable(DateTime.Now)
                                              )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:SpectrumIdentificationList) =
                table.Name <- name
                table

            ///Replaces numsequencessearched of existing object with new numsequencessearched.
            static member addNumSequencesSearched
                (numSequencesSearched:int64) (table:SpectrumIdentificationList) =
                table.NumSequencesSearched <- Nullable(numSequencesSearched)
                table

            ///Adds a fragmentationtable to an existing spectrumidentificationlist-object.
            static member addFragmentationTable
                (fragmentationTable:Measure) (table:SpectrumIdentificationList) =
                let result = table.FragmentationTables <- addToList table.FragmentationTables fragmentationTable
                table

            ///Adds a collection of fragmentationtables to an existing spectrumidentificationlist-object.
            static member addFragmentationTables
                (fragmentationTables:seq<Measure>) (table:SpectrumIdentificationList) =
                let result = table.FragmentationTables <- addCollectionToList table.FragmentationTables fragmentationTables
                table

            ///Adds a spectrumidentificationlistparam to an existing spectrumidentificationlist-object.
            static member addDetail
                (detail:SpectrumIdentificationListParam) (table:SpectrumIdentificationList) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of spectrumidentificationlistparams to an existing spectrumidentificationlist-object.
            static member addDetails
                (details:seq<SpectrumIdentificationListParam>) (table:SpectrumIdentificationList) =
                let result = table.Details <- addCollectionToList table.Details details
                table

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
               item1.FKAnalysisData=item2.FKAnalysisData && item1.FKProteinDetection=item2.FKProteinDetection &&
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
                    fkSpectrumIdentificationList        : string,
                    fkSpectrumIdentificationProtocol    : string,
                    spectraData                         : seq<SpectraData>,
                    searchDatabase                      : seq<SearchDatabase>,
                    ?id                                 : string,
                    ?name                               : string,
                    ?spectrumIdentificationList         : SpectrumIdentificationList,
                    ?spectrumIdentificationProtocol     : SpectrumIdentificationProtocol,
                    ?activityDate                       : DateTime,
                    ?fkMzIdentML                        : string
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                           = defaultArg name Unchecked.defaultof<string>
                let activityDate'                   = defaultArg activityDate Unchecked.defaultof<DateTime>
                let spectrumIdentificationList'     = defaultArg spectrumIdentificationList Unchecked.defaultof<SpectrumIdentificationList>
                let spectrumIdentificationProtocol' = defaultArg spectrumIdentificationProtocol Unchecked.defaultof<SpectrumIdentificationProtocol>
                let fkMzIdentML'                    = defaultArg fkMzIdentML Unchecked.defaultof<string>
                    
                new SpectrumIdentification(
                                           id', 
                                           name',
                                           Nullable(activityDate'), 
                                           spectrumIdentificationList', 
                                           fkSpectrumIdentificationList, 
                                           spectrumIdentificationProtocol',
                                           fkSpectrumIdentificationProtocol,
                                           spectraData |> List, 
                                           searchDatabase |> List, 
                                           fkMzIdentML', 
                                           Nullable(DateTime.Now)
                                          )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:SpectrumIdentification) =
                table.Name <- name
                table

            ///Replaces activitydate of existing object with new name.
            static member addActivityDate
                (activityDate:DateTime) (table:SpectrumIdentification) =
                table.ActivityDate <- Nullable(activityDate)
                table

            ///Replaces spectrumIdentificationList of existing object with new one.
            static member addSpectrumIdentificationList
                (spectrumIdentificationList:SpectrumIdentificationList) (table:SpectrumIdentification) =
                table.SpectrumIdentificationList <- spectrumIdentificationList
                table

            ///Replaces spectrumIdentificationProtocol of existing object with new one.
            static member addSpectrumIdentificationList
                (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (table:SpectrumIdentification) =
                table.SpectrumIdentificationProtocol <- spectrumIdentificationProtocol
                table

            ///Replaces fkMzIdentML of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (table:SpectrumIdentification) =
                let result = table.FKMzIdentMLDocument <- fkMzIdentML
                table

            ///Tries to find a spectrumIdentification-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.SpectrumIdentification.Local do
                           if i.ID=id
                              then select (i, i.SpectrumIdentificationProtocol, i.SpectrumIdentificationList, 
                                           i.SpectraData, i.SearchDatabases, i.FKMzIdentMLDocument
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
                                                       i.SpectraData, i.SearchDatabases, i.FKMzIdentMLDocument
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
                                           i.SpectraData, i.SearchDatabases, i.FKMzIdentMLDocument
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
                                                       i.SpectraData, i.SearchDatabases, i.FKMzIdentMLDocument
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
               item1.FKSpectrumIdentificationList=item2.FKSpectrumIdentificationList && 
               item1.FKSpectrumIdentificationProtocol=item2.FKSpectrumIdentificationProtocol && 
               item1.FKMzIdentMLDocument=item2.FKMzIdentMLDocument && item1.Name=item2.Name &&
               item1.SpectraData=item2.SpectraData && item1.SearchDatabases=item2.SearchDatabases &&
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
                    fkAnalysisSoftware  : string,
                    threshold           : seq<ThresholdParam>,
                    ?id                 : string,
                    ?name               : string,
                    ?analysisSoftware   : AnalysisSoftware,
                    ?analysisParams     : seq<AnalysisParam>,
                    ?fkMzIdentML        : string
                ) =
                let id'                 = defaultArg id (System.Guid.NewGuid().ToString())
                let name'               = defaultArg name Unchecked.defaultof<string>
                let analysisSoftware'   = defaultArg analysisSoftware Unchecked.defaultof<AnalysisSoftware>
                let analysisParams'     = convertOptionToList analysisParams
                let fkMzIdentML'        = defaultArg fkMzIdentML Unchecked.defaultof<string>
                    
                new ProteinDetectionProtocol(
                                             id', 
                                             name', 
                                             analysisSoftware', 
                                             fkAnalysisSoftware,
                                             analysisParams', 
                                             threshold |> List,
                                             fkMzIdentML', 
                                             Nullable(DateTime.Now)
                                            )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:ProteinDetectionProtocol) =
                table.Name <- name
                table

            ///Replaces analysisSoftware of existing object with new one.
            static member addAnalysisSoftware
                (analysisSoftware:AnalysisSoftware) (table:ProteinDetectionProtocol) =
                table.AnalysisSoftware <- analysisSoftware
                table

            ///Adds a analysisparam to an existing proteindetectionprotocol-object.
            static member addAnalysisParam
                (analysisParam:AnalysisParam) (table:ProteinDetectionProtocol) =
                let result = table.AnalysisParams <- addToList table.AnalysisParams analysisParam
                table

            ///Adds a collection of analysisparams to an existing proteindetectionprotocol-object.
            static member addAnalysisParams
                (analysisParams:seq<AnalysisParam>) (table:ProteinDetectionProtocol) =
                let result = table.AnalysisParams <- addCollectionToList table.AnalysisParams analysisParams
                table

            ///Replaces fkMzIdentML of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (table:ProteinDetectionProtocol) =
                table.FKMzIdentMLDocument <- fkMzIdentML
                table

            ///Tries to find a proteinDetectionProtocol-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.ProteinDetectionProtocol.Local do
                           if i.ID=id
                              then select (i, i.Threshold, i.AnalysisParams, i.FKMzIdentMLDocument)
                      }
                |> Seq.map (fun (proteinDetectionProtocol, _, _, _) -> proteinDetectionProtocol)
                |> (fun proteinDetectionProtocol -> 
                    if (Seq.exists (fun proteinDetectionProtocol' -> proteinDetectionProtocol' <> null) proteinDetectionProtocol) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionProtocol do
                                       if i.ID=id
                                          then select (i, i.Threshold, i.AnalysisParams, i.FKMzIdentMLDocument)
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
                              then select (i, i.Threshold, i.AnalysisParams, i.FKMzIdentMLDocument)
                      }
                |> Seq.map (fun (proteinDetectionProtocol, _, _, _) -> proteinDetectionProtocol)
                |> (fun proteinDetectionProtocol -> 
                    if (Seq.exists (fun proteinDetectionProtocol' -> proteinDetectionProtocol' <> null) proteinDetectionProtocol) = false
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionProtocol do
                                       if i.Name=name
                                          then select (i, i.Threshold, i.AnalysisParams, i.FKMzIdentMLDocument)
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
               item1.Threshold=item2.Threshold && item1.Name=item2.Name && 
               item1.FKAnalysisSoftware=item2.FKAnalysisSoftware &&
               item1.AnalysisParams=item2.AnalysisParams && item1.FKMzIdentMLDocument=item2.FKMzIdentMLDocument

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
                    location                        : string,
                    fkFileFormat                    : string,
                    ?id                             : string,
                    ?name                           : string,
                    ?externalFormatDocumentation    : string,
                    ?fileFormat                     : CVParam,
                    ?fkInputs                       : string,
                    ?details                        : seq<SourceFileParam>
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                           = defaultArg name Unchecked.defaultof<string>
                let externalFormatDocumentation'    = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let fileFormat'                     = defaultArg fileFormat Unchecked.defaultof<CVParam>
                let fkInputs'                       = defaultArg fkInputs Unchecked.defaultof<string>
                let details'                        = convertOptionToList details
                    
                new SourceFile(
                               id', 
                               name', 
                               location, 
                               externalFormatDocumentation', 
                               fileFormat', 
                               fkFileFormat,
                               fkInputs',
                               details', 
                               Nullable(DateTime.Now)
                              )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:SourceFile) =
                table.Name <- name
                table

            ///Replaces externalformatdocumentation of existing object with new one.
            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (table:SourceFile) =
                table.ExternalFormatDocumentation <- externalFormatDocumentation
                table

            ///Replaces fileFormat of existing object with new one.
            static member addFileFormat
                (fileFormat:CVParam) (table:SourceFile) =
                table.FileFormat <- fileFormat
                table

            ///Replaces fkInputs of existing object with new one.
            static member addFKInputs
                (fkInputs:string) (table:SourceFile) =
                table.FKInputs <- fkInputs
                table

            ///Adds a sourcefileparam to an existing sourcefile-object.
            static member addDetail
                (detail:SourceFileParam) (table:SourceFile) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of sourcefileparams to an existing sourcefile-object.
            static member addDetails
                (details:seq<SourceFileParam>) (table:SourceFile) =
                let result = table.Details <- addCollectionToList table.Details details
                table

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
               item1.FKFileFormat=item2.FKFileFormat && item1.Name=item2.Name && 
               matchCVParamBases 
                (item1.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) 
                (item2.Details |> Seq.map (fun item -> item :> CVParamBase) |> List) &&
               item1.FKInputs=item2.FKInputs && 
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
                    fkPeptideEvidence               : string,
                    spectrumIdentificationItems     : seq<SpectrumIdentificationItem>,
                    ?id                             : string,
                    ?peptideEvidence                : PeptideEvidence,
                    ?fkProteinDetectionHypothesis   : string
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let peptideEvidence'                = defaultArg peptideEvidence Unchecked.defaultof<PeptideEvidence>
                let fkProteinDetectionHypothesis'   = defaultArg fkProteinDetectionHypothesis Unchecked.defaultof<string>

                new PeptideHypothesis(
                                      id', 
                                      peptideEvidence',
                                      fkPeptideEvidence,
                                      spectrumIdentificationItems |> List,
                                      fkProteinDetectionHypothesis',
                                      Nullable(DateTime.Now)
                                     )

            ///Replaces peptideEvidence of existing object with new one.
            static member addPeptideEvidence
                (peptideEvidence:PeptideEvidence) (table:PeptideHypothesis) =
                table.PeptideEvidence <- peptideEvidence
                table

            ///Replaces fkProteinDetectionHypothesis of existing object with new one.
            static member addFKProteinDetectionHypothesis
                (fkProteinDetectionHypothesis:string) (table:PeptideHypothesis) =
                table.FKProteinDetectionHypothesis <- fkProteinDetectionHypothesis
                table

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
            static member tryFindByFKPeptideEvidence (dbContext:MzIdentML) (fkPeptideEvidence:string) =
                query {
                       for i in dbContext.PeptideHypothesis.Local do
                           if i.FKPeptideEvidence=fkPeptideEvidence
                              then select (i, i.PeptideEvidence, i.SpectrumIdentificationItems)
                      }
                |> Seq.map (fun (peptideHypothesis, _, _) -> peptideHypothesis)
                |> (fun peptideHypothesis -> 
                    if (Seq.exists (fun peptideHypothesis' -> peptideHypothesis' <> null) peptideHypothesis) = false
                        then 
                            query {
                                   for i in dbContext.PeptideHypothesis do
                                       if i.FKPeptideEvidence=fkPeptideEvidence
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
               item1.SpectrumIdentificationItems=item2.SpectrumIdentificationItems &&
               item1.FKProteinDetectionHypothesis=item2.FKProteinDetectionHypothesis

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:PeptideHypothesis) =
                    PeptideHypothesisHandler.tryFindByFKPeptideEvidence dbContext item.FKPeptideEvidence
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
                    passThreshold               : bool,
                    fkDBSequence                : string,
                    peptideHypothesis           : seq<PeptideHypothesis>,
                    fkProteinAmbiguityGroup     : string,
                    ?id                         : string,
                    ?name                       : string,
                    ?dbSequence                 : DBSequence,
                    ?details                    : seq<ProteinDetectionHypothesisParam>
                ) =
                let id'                         = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                       = defaultArg name Unchecked.defaultof<string>
                let dbSequence'                 = defaultArg dbSequence Unchecked.defaultof<DBSequence>
                let details'                    = convertOptionToList details
                    
                new ProteinDetectionHypothesis(
                                               id', 
                                               name', 
                                               Nullable(passThreshold), 
                                               dbSequence', 
                                               fkDBSequence,
                                               peptideHypothesis |> List,
                                               fkProteinAmbiguityGroup,
                                               details', 
                                               Nullable(DateTime.Now)
                                              )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:ProteinDetectionHypothesis) =
                table.Name <- name
                table

            ///Replaces dbSequence of existing object with new one.
            static member addDBSequence
                (dbSequence:DBSequence) (table:ProteinDetectionHypothesis) =
                table.DBSequence <- dbSequence
                table

            ///Replaces fkProteinAmbiguityGroup of existing object with new one.
            static member addDBSequence
                (fkProteinAmbiguityGroup:string) (table:ProteinDetectionHypothesis) =
                table.FKProteinAmbiguityGroup <- fkProteinAmbiguityGroup
                table

            ///Adds a proteindetectionhypothesisparam to an existing proteindetectionhypothesis-object.
            static member addDetail
                (detail:ProteinDetectionHypothesisParam) (table:ProteinDetectionHypothesis) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of proteindetectionhypothesisparams to an existing proteindetectionhypothesis-object.
            static member addDetails
                (details:seq<ProteinDetectionHypothesisParam>) (table:ProteinDetectionHypothesis) =
                let result = table.Details <- addCollectionToList table.Details details
                table

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
               item1.Name=item2.Name && item1.FKProteinAmbiguityGroup=item2.FKProteinAmbiguityGroup &&
               matchCVParamBases 
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
                    proteinDetecionHypothesis   : seq<ProteinDetectionHypothesis>,
                    ?id                         : string,
                    ?name                       : string,
                    ?fkProteinDetectionList     : string,
                    ?details                    : seq<ProteinAmbiguityGroupParam>
                ) =
                let id'                         = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                       = defaultArg name Unchecked.defaultof<string>
                let fkProteinDetectionList'     = defaultArg fkProteinDetectionList Unchecked.defaultof<string>
                let details'                    = convertOptionToList details
                    
                new ProteinAmbiguityGroup(
                                          id', 
                                          name', 
                                          proteinDetecionHypothesis |> List,
                                          fkProteinDetectionList',
                                          details', 
                                          Nullable(DateTime.Now)
                                         )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:ProteinAmbiguityGroup) =
                table.Name <- name
                table

            ///Replaces fkProteinDetectionList of existing object with new one.
            static member addFKProteinDetectionList
                (fkProteinDetectionList:string) (table:ProteinAmbiguityGroup) =
                table.FKProteinDetectionList <- fkProteinDetectionList
                table

            ///Adds a proteinambiguitygroupparam to an existing proteinambiguitygroup-object.
            static member addDetail
                (detail:ProteinAmbiguityGroupParam) (table:ProteinAmbiguityGroup) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of proteinambiguitygroupparam to an existing proteinambiguitygroupparam-object.
            static member addDetails
                (details:seq<ProteinAmbiguityGroupParam>) (table:ProteinAmbiguityGroup) =
                let result = table.Details <- addCollectionToList table.Details details
                table

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
               item1.ProteinDetectionHypothesis=item2.ProteinDetectionHypothesis &&
               item1.FKProteinDetectionList=item2.FKProteinDetectionList &&
               matchCVParamBases 
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

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:ProteinDetectionList) =
                table.Name <- name
                table

            ///Adds a proteinambiguitygroup to an existing proteindetectionlist-object.
            static member addProteinAmbiguityGroup
                (proteinAmbiguityGroup:ProteinAmbiguityGroup) (table:ProteinDetectionList) =
                let result = table.ProteinAmbiguityGroups <- addToList table.ProteinAmbiguityGroups proteinAmbiguityGroup
                table

            ///Adds a collection of proteinambiguitygroups to an existing proteindetectionlist-object.
            static member addProteinAmbiguityGroups
                (proteinAmbiguityGroups:seq<ProteinAmbiguityGroup>) (table:ProteinDetectionList) =
                let result = table.ProteinAmbiguityGroups <- addCollectionToList table.ProteinAmbiguityGroups proteinAmbiguityGroups
                table

            ///Adds a proteindetectionlistparam to an existing proteindetectionlist-object.
            static member addDetail
                (detail:ProteinDetectionListParam) (table:ProteinDetectionList) =
                let result = table.Details <- addToList table.Details detail
                table

            ///Adds a collection of proteindetectionlistparams to an existing proteindetectionlist-object.
            static member addDetails
                (details:seq<ProteinDetectionListParam>) (table:ProteinDetectionList) =
                let result = table.Details <- addCollectionToList table.Details details
                table

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
                    spectrumIdentificationList  : seq<SpectrumIdentificationList>,
                    ?id                         : string,
                    ?proteinDetectionList       : ProteinDetectionList,
                    ?fkProteinDetectionList     : string
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let proteinDetectionList'   = defaultArg proteinDetectionList Unchecked.defaultof<ProteinDetectionList>
                let fkProteinDetectionList' = defaultArg fkProteinDetectionList Unchecked.defaultof<string>
                    
                new AnalysisData(
                                 id', 
                                 spectrumIdentificationList |> List, 
                                 proteinDetectionList',
                                 fkProteinDetectionList',
                                 Nullable(DateTime.Now)
                                )

            ///Replaces proteindetectionlist of existing object with new one.
            static member addProteinDetectionList
                (proteinDetectionList:ProteinDetectionList) (table:AnalysisData) =
                table.ProteinDetectionList <- proteinDetectionList
                table

            ///Replaces fkProteinDetectionList of existing object with new one.
            static member addFKProteinDetectionList
                (fkProteinDetectionList:string) (table:AnalysisData) =
                table.FKProteinDetectionList <- fkProteinDetectionList
                table

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
            static member tryFindByFKProteinDetectionList (dbContext:MzIdentML) (fkProteinDetectionList:string) =
                query {
                       for i in dbContext.AnalysisData.Local do
                           if i.FKProteinDetectionList=fkProteinDetectionList
                              then select (i, i.SpectrumIdentificationList, i.ProteinDetectionList)
                      }
                |> Seq.map (fun (analysisData, _, _) -> analysisData)
                |> (fun analysisData -> 
                    if (Seq.exists (fun analysisData' -> analysisData' <> null) analysisData) = false
                        then 
                            query {
                                   for i in dbContext.AnalysisData do
                                       if i.FKProteinDetectionList=fkProteinDetectionList
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
               item1.FKProteinDetectionList=item2.FKProteinDetectionList

            ///First checks if any object with same field-values (except primary key) exists within the context or database. 
            ///If no entry exists, a new object is added to the context and otherwise does nothing.
            static member addToContext (dbContext:MzIdentML) (item:AnalysisData) =
                    AnalysisDataHandler.tryFindByFKProteinDetectionList dbContext item.FKProteinDetectionList
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
                    fkProteinDetectionList      : string,
                    fkProteinDetectionProtocol  : string,
                    spectrumIdentificationLists : seq<SpectrumIdentificationList>,
                    ?id                         : string,
                    ?name                       : string,
                    ?activityDate               : DateTime,
                    ?proteinDetectionList       : ProteinDetectionList,
                    ?proteinDetectionProtocol   : ProteinDetectionProtocol
                ) =
                let id'                         = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                       = defaultArg name Unchecked.defaultof<string>
                let activityDate'               = defaultArg activityDate Unchecked.defaultof<DateTime>
                let proteinDetectionList'       = defaultArg proteinDetectionList Unchecked.defaultof<ProteinDetectionList>
                let proteinDetectionProtocol'   = defaultArg proteinDetectionProtocol Unchecked.defaultof<ProteinDetectionProtocol>
                    
                new ProteinDetection(
                                     id', 
                                     name', 
                                     Nullable(activityDate'), 
                                     proteinDetectionList', 
                                     fkProteinDetectionList,
                                     proteinDetectionProtocol',
                                     fkProteinDetectionProtocol,
                                     spectrumIdentificationLists |> List,
                                     Nullable(DateTime.Now)
                                    )

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:ProteinDetection) =
                table.Name <- name
                table

            ///Replaces activitydate of existing object with new one.
            static member addActivityDate
                (activityDate:DateTime) (table:ProteinDetection) =
                table.ActivityDate <- Nullable(activityDate)
                table

            ///Replaces proteinDetectionList of existing object with new one.
            static member addProteinDetectionList
                (proteinDetectionList:ProteinDetectionList) (table:ProteinDetection) =
                table.ProteinDetectionList <- proteinDetectionList
                table

            ///Replaces proteinDetectionProtocol of existing object with new one.
            static member addProteinDetectionProtocol
                (proteinDetectionProtocol:ProteinDetectionProtocol) (table:ProteinDetection) =
                table.ProteinDetectionProtocol <- proteinDetectionProtocol
                table

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
               item1.FKProteinDetectionProtocol=item2.FKProteinDetectionProtocol && 
               item1.FKProteinDetectionList=item2.FKProteinDetectionList &&
               item1.SpectrumIdentificationLists=item2.SpectrumIdentificationLists &&
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

            ///Replaces name of existing object with new one.
            static member addName
                (name:string) (table:BiblioGraphicReference) =
                table.Name <- name
                table

            ///Replaces authors of existing object with new authors.
            static member addAuthors
                (authors:string) (table:BiblioGraphicReference) =
                table.Authors <- authors
                table

            ///Replaces doi of existing object with new doi.
            static member addDOI
                (doi:string) (table:BiblioGraphicReference) =
                table.DOI <- doi
                table

            ///Replaces editor of existing object with new editor.
            static member addEditor
                (editor:string) (table:BiblioGraphicReference) =
                table.Editor <- editor
                table

            ///Replaces issue of existing object with new issue.
            static member addIssue
                (issue:string) (table:BiblioGraphicReference) =
                table.Issue <- issue
                table

            ///Replaces pages of existing object with new pages.
            static member addPages
                (pages:string) (table:BiblioGraphicReference) =
                table.Pages <- pages
                table

            ///Replaces publication of existing object with new publication.
            static member addPublication
                (publication:string) (table:BiblioGraphicReference) =
                table.Publication <- publication
                table

            ///Replaces publisher of existing object with new publisher.
            static member addPublisher
                (publisher:string) (table:BiblioGraphicReference) =
                table.Publisher <- publisher
                table

            ///Replaces title of existing object with new title.
            static member addTitle
                (title:string) (table:BiblioGraphicReference) =
                table.Title <- title
                table

            ///Replaces volume of existing object with new volume.
            static member addVolume
                (volume:string) (table:BiblioGraphicReference) =
                table.Volume <- volume
                table

            ///Replaces year of existing object with new year.
            static member addYear
                (year:int) (table:BiblioGraphicReference) =
                table.Year <- Nullable(year)
                table

            ///Replaces fkMzIdentML of existing object with new one.
            static member addFkMzIdentMLDocument
                (fkMzIdentML:string) (table:BiblioGraphicReference) =
                table.FKMzIdentMLDocument <- fkMzIdentML
                table

            ///Tries to find a biblioGraphicReference-object in the context and database, based on its primary-key(ID).
            static member tryFindByID (dbContext:MzIdentML) (id:string) =
                query {
                       for i in dbContext.BiblioGraphicReference.Local do
                           if i.ID=id
                              then select (i, i.FKMzIdentMLDocument)
                      }
                |> Seq.map (fun (biblioGraphicReference, _) -> biblioGraphicReference)
                |> (fun biblioGraphicReference -> 
                    if (Seq.exists (fun biblioGraphicReference' -> biblioGraphicReference' <> null) biblioGraphicReference) = false
                        then 
                            query {
                                   for i in dbContext.BiblioGraphicReference do
                                       if i.ID=id
                                          then select (i, i.FKMzIdentMLDocument)
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
                              then select (i, i.FKMzIdentMLDocument)
                      }
                |> Seq.map (fun (biblioGraphicReference, _) -> biblioGraphicReference)
                |> (fun biblioGraphicReference -> 
                    if (Seq.exists (fun biblioGraphicReference' -> biblioGraphicReference' <> null) biblioGraphicReference) = false
                        then 
                            query {
                                   for i in dbContext.BiblioGraphicReference do
                                       if i.Name=name
                                          then select (i, i.FKMzIdentMLDocument)
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
               item1.Volume=item2.Volume && item1.Year=item2.Year && item1.FKMzIdentMLDocument=item2.FKMzIdentMLDocument

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
                    ?id                     : string,
                    ?name                   : string,
                    ?analysisSoftware       : AnalysisSoftware,
                    ?fkAnalysisSoftware     : string,
                    ?contactRole            : ContactRole,
                    ?fkContactRole          : string,
                    ?fkMzQuantMLDocument    : string
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                   = defaultArg name Unchecked.defaultof<string>
                let analysisSoftware'       = defaultArg analysisSoftware Unchecked.defaultof<AnalysisSoftware>
                let fkAnalysisSoftware'     = defaultArg fkAnalysisSoftware Unchecked.defaultof<string>
                let contactRole'            = defaultArg contactRole Unchecked.defaultof<ContactRole>
                let fkContactRole'          = defaultArg fkContactRole Unchecked.defaultof<string>
                let fkMzQuantMLDocument'    = defaultArg fkMzQuantMLDocument Unchecked.defaultof<string>

                new Provider(
                             id', 
                             name', 
                             analysisSoftware', 
                             fkAnalysisSoftware', 
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

            ///Replaces analysissoftware of existing object with new one.
            static member addAnalysisSoftware
                (analysisSoftware:AnalysisSoftware) (table:Provider) =
                table.AnalysisSoftware <- analysisSoftware
                table

            ///Replaces fkAnalysisSoftware of existing object with new one.
            static member addFKAnalysisSoftware
                (fkAnalysisSoftware:string) (table:Provider) =
                table.FKAnalysisSoftware <- fkAnalysisSoftware
                table

            ///Replaces contactrole of existing object with new one.
            static member addContactRole
                (contactRole:ContactRole) (table:Provider) =
                table.ContactRole <- contactRole
                table

            ///Replaces fkContactRole of existing object with new one.
            static member addFKContactRole
                (fkContactRole:string) (table:Provider) =
                table.FKContactRole <- fkContactRole
                table

            ///Replaces fkMzIdentMLDocument of existing object with new one.
            static member addFKMzIdentMLDocument
                (fkMzIdentMLDocument:string) (table:Provider) =
                table.FKMzIdentMLDocument <- fkMzIdentMLDocument
                table

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
               item1.FKAnalysisSoftware=item2.FKAnalysisSoftware && item1.FKContactRole=item2.FKContactRole &&
               item1.FKMzIdentMLDocument=item2.FKMzIdentMLDocument

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
                    ?analysisSoftwareList           : seq<AnalysisSoftware>,
                    ?providers                      : seq<Provider>,
                    ?organizations                  : seq<Organization>,
                    ?persons                        : seq<Person>, 
                    ?samples                        : seq<Sample>,
                    ?dbSequences                    : seq<DBSequence>,
                    ?peptides                       : seq<Peptide>,
                    ?peptideEvidences               : seq<PeptideEvidence>,
                    ?proteinDetections              : seq<ProteinDetection>,
                    ?proteinDetectionProtocol       : ProteinDetectionProtocol,
                    ?biblioGraphicReferences        : seq<BiblioGraphicReference>
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
                let name'                           = defaultArg name Unchecked.defaultof<string>
                let version'                        = defaultArg version Unchecked.defaultof<string>
                let analysisSoftwareList'           = convertOptionToList analysisSoftwareList
                let providers'                      = convertOptionToList providers
                let organizations'                  = convertOptionToList organizations
                let persons'                        = convertOptionToList persons
                let samples'                        = convertOptionToList samples
                let dbSequences'                    = convertOptionToList dbSequences
                let peptides'                       = convertOptionToList peptides
                let peptideEvidences'               = convertOptionToList peptideEvidences
                let spectrumIdentification'         = convertOptionToList spectrumIdentification
                let proteinDetections'              = convertOptionToList proteinDetections
                let spectrumIdentificationProtocol' = convertOptionToList spectrumIdentificationProtocol
                let proteinDetectionProtocol'       = defaultArg proteinDetectionProtocol Unchecked.defaultof<ProteinDetectionProtocol>
                let inputs'                         = defaultArg inputs Unchecked.defaultof<Inputs>
                let analysisData'                   = defaultArg analysisData Unchecked.defaultof<AnalysisData>
                let biblioGraphicReferences'        = convertOptionToList biblioGraphicReferences
                new MzIdentMLDocument(
                                      id', 
                                      name',
                                      version',
                                      analysisSoftwareList', 
                                      providers', 
                                      organizations', 
                                      persons', 
                                      samples', 
                                      dbSequences', 
                                      peptides', 
                                      peptideEvidences', 
                                      spectrumIdentification',
                                      proteinDetections',
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

            ///Adds a analysisSoftware to an existing mzidentmldocument-object.
            static member addAnalysisSoftware
                (table:AnalysisSoftware) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.AnalysisSoftwareList <- addToList mzIdentML.AnalysisSoftwareList analysisSoftware
                mzIdentML

            ///Adds a collection of analysisSoftwareList to an existing mzidentmldocument-object.
            static member addAnalysisSoftwareList
                (analysisSoftwareList:seq<AnalysisSoftware>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.AnalysisSoftwareList <- addCollectionToList mzIdentML.AnalysisSoftwareList analysisSoftwareList
                mzIdentML

            ///Adds a provider to an existing mzidentmldocument-object.
            static member addProvider
                (provider:Provider) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Providers <- addToList mzIdentML.Providers provider
                mzIdentML

            ///Adds a collection of providers to an existing mzidentmldocument-object.
            static member addProviders
                (providers:seq<Provider>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Providers <- addCollectionToList mzIdentML.Providers providers
                mzIdentML

            ///Adds a organization to an existing mzidentmldocument-object.
            static member addOrganization
                (organization:Organization) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Organizations <- addToList mzIdentML.Organizations organization
                mzIdentML

            ///Adds a collection of organizations to an existing mzidentmldocument-object.
            static member addOrganizations
                (organizations:seq<Organization>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Organizations <- addCollectionToList mzIdentML.Organizations organizations
                mzIdentML

            ///Adds a person to an existing mzidentmldocument-object.
            static member addPerson
                (table:Person) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Persons <- addToList mzIdentML.Persons person
                mzIdentML

            ///Adds a collection of persons to an existing mzidentmldocument-object.
            static member addPersons
                (persons:seq<Person>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Persons <- addCollectionToList mzIdentML.Persons persons
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

            ///Adds a proteinDetection to an existing mzidentmldocument-object.
            static member addProteinDetection
                (proteinDetection:ProteinDetection) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.ProteinDetections <- addToList mzIdentML.ProteinDetections proteinDetection
                mzIdentML

            ///Adds a collection of proteinDetections to an existing mzidentmldocument-object.
            static member addProteinDetections
                (proteinDetections:seq<ProteinDetection>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.ProteinDetections <- addCollectionToList mzIdentML.ProteinDetections proteinDetections
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
                              then select (i, i.Inputs, i.AnalysisSoftwareList, i.Providers, i.Persons, i.Organizations, i.Samples,
                                           i.DBSequences, i.Peptides, i.PeptideEvidences, i.SpectrumIdentifications, i.ProteinDetections,
                                           i.SpectrumIdentificationProtocols, i.ProteinDetectionProtocol, i.AnalysisData, i.BiblioGraphicReferences
                                          )
                      }
                |> Seq.map (fun (mzIdentMLDocument, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _) -> mzIdentMLDocument)
                |> (fun mzIdentMLDocument -> 
                    if (Seq.exists (fun mzIdentMLDocument' -> mzIdentMLDocument' <> null) mzIdentMLDocument) = false
                        then 
                            query {
                                   for i in dbContext.MzIdentMLDocument do
                                       if i.ID=id
                                          then select (i, i.Inputs, i.AnalysisSoftwareList, i.Providers, i.Persons, i.Organizations, i.Samples,
                                                       i.DBSequences, i.Peptides, i.PeptideEvidences, i.SpectrumIdentifications, i.ProteinDetections,
                                                       i.SpectrumIdentificationProtocols, i.ProteinDetectionProtocol, i.AnalysisData, i.BiblioGraphicReferences
                                                      )
                                  }
                            |> Seq.map (fun (mzIdentMLDocument, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _) -> mzIdentMLDocument)
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
                              then select (i, i.Inputs, i.AnalysisSoftwareList, i.Providers, i.Persons, i.Organizations, i.Samples,
                                           i.DBSequences, i.Peptides, i.PeptideEvidences, i.SpectrumIdentifications, i.ProteinDetections,
                                           i.SpectrumIdentificationProtocols, i.ProteinDetectionProtocol, i.AnalysisData, i.BiblioGraphicReferences
                                          )
                      }
                |> Seq.map (fun (mzIdentMLDocument, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _) -> mzIdentMLDocument)
                |> (fun mzIdentMLDocument -> 
                    if (Seq.exists (fun mzIdentMLDocument' -> mzIdentMLDocument' <> null) mzIdentMLDocument) = false
                        then 
                            query {
                                   for i in dbContext.MzIdentMLDocument do
                                       if i.Name=name
                                          then select (i, i.Inputs, i.AnalysisSoftwareList, i.Providers, i.Persons, i.Organizations, i.Samples,
                                                       i.DBSequences, i.Peptides, i.PeptideEvidences, i.SpectrumIdentifications, i.ProteinDetections,
                                                       i.SpectrumIdentificationProtocols, i.ProteinDetectionProtocol, i.AnalysisData, i.BiblioGraphicReferences
                                                      )
                                  }
                            |> Seq.map (fun (mzIdentMLDocument, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _) -> mzIdentMLDocument)
                            |> (fun mzIdentMLDocument -> if (Seq.exists (fun mzIdentMLDocument' -> mzIdentMLDocument' <> null) mzIdentMLDocument) = false
                                                             then None
                                                             else Some mzIdentMLDocument
                               )
                        else Some mzIdentMLDocument
                   )

            ///Checks whether all other fields of the current object and context object have the same values or not.
            static member private hasEqualFieldValues (item1:MzIdentMLDocument) (item2:MzIdentMLDocument) =
               item1.Name=item2.Name && item1.Version=item2.Version && item1.AnalysisSoftwareList=item2.AnalysisSoftwareList && 
               item1.Providers=item2.Providers && item1.Organizations=item2.Organizations && item1.Persons=item2.Persons &&
               item1.Samples=item2.Samples && item1.DBSequences=item2.DBSequences && item1.Peptides=item2.Peptides && 
               item1.PeptideEvidences=item2.PeptideEvidences && item1.SpectrumIdentifications=item2.SpectrumIdentifications &&
               item1.ProteinDetections=item2.ProteinDetections && item1.SpectrumIdentificationProtocols=item2.SpectrumIdentificationProtocols &&
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
