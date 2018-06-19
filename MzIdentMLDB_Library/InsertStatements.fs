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

        type ExampleTerm =
            |  A 
            |  B 
            |  C of string 
            static member ToString (x:ExampleTerm) =
                match x with
                | A  -> "A"
                | B  -> "B"
                | C v -> sprintf "C %s" v

        type ContextHandler =

            ///Creates connection for SQLite-context and database.
            static member sqliteConnection (path:string) =
                let optionsBuilder = 
                    new DbContextOptionsBuilder<MzIdentML>()
                optionsBuilder.UseSqlite(@"Data Source=" + path) |> ignore
                new MzIdentML(optionsBuilder.Options)       

            ///Creats connection for SQL-context and DataBase.
            static member sqlConnection() =
                let optionsBuilder = 
                    new DbContextOptionsBuilder<MzIdentML>()
                optionsBuilder.UseSqlServer("Server=(localdb)\mssqllocaldb;Database=MyDatabase;Trusted_Connection=True;") |> ignore
                new MzIdentML(optionsBuilder.Options) 

            ///Reads Obo-file and creates sequence of Obo.Terms.
            static member fromFileObo (filePath:string) =
                FileIO.readFile filePath
                |> Obo.parseOboTerms

            ///Tries to add the object to the database-context.
            static member tryAddToContext (context:MzIdentML) (item:'b) =
                context.Add(item)

            ///Tries to add the Object to the DataBase-Context and insert it in the database in the next step.
            static member tryAddToContextAndInsert (context:MzIdentML) (item:'b) =
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

            ///Adds a name to an existing term-object.
            static member addName
                (name:string) (term:Term) =
                term.Name <- name
                term
                    
            ///Adds an ontology to an existing term-object.
            static member addOntology
                (ontology:Ontology) (term:Term) =
                term.Ontology <- ontology
                term

            ///Tries to find a term-object in the context and database, based on its ID.
            static member tryFindByID
                (context:MzIdentML) (termID:string) =
                tryFind (context.Term.Find(termID))

            static member private matchAndAddTerms (dbContext:MzIdentML) (item1:Term) (item2:Term) =
                if item1.Ontology=item2.Ontology
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- System.Guid.NewGuid().ToString()
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Term) =
                query {
                        for i in dbContext.Term.Local do
                            if i.Name = item.Name
                               then select (i, i.Ontology)
                      }
                |> Seq.map (fun (term, _) -> term)
                |> (fun term -> 
                    if Seq.length term < 1 
                        then 
                            query {
                                   for i in dbContext.Term do
                                       if i.Name = item.Name
                                          then select (i, i.Ontology)
                                  }
                            |> Seq.map (fun (term, _) -> term)
                            |> (fun term' -> if (term'.Count()) < 1
                                                then let tmp = dbContext.Term.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- System.Guid.NewGuid().ToString()
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else term'
                                                     |> Seq.iter (fun termItem -> TermHandler.matchAndAddTerms dbContext termItem item)
                               )
                        else term
                             |> Seq.iter (fun termItem -> TermHandler.matchAndAddTerms dbContext termItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Term) =
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

            ///Adds a collection of terms to an existing ontology-object
            static member addTerms
                (terms:seq<Term>) (ontology:Ontology) =
                let result = ontology.Terms <- addCollectionToList ontology.Terms terms
                ontology

            ///Tries to find a ontology-object in the context and database, based on its ID.
            static member tryFindByID
                (context:MzIdentML) (ontologyID:string) =
                tryFind (context.Ontology.Find(ontologyID))
            
            //static member private matchAndAddOntologies (dbContext:MzIdentML) (item1:Ontology) (item2:Ontology) =
            //    if item1.ID=item2.ID && item1.Terms=item2.Terms
            //       then ()
            //       else 
            //            if item1.ID = item2.ID
            //               then item2.ID <- System.Guid.NewGuid().ToString()
            //                    dbContext.Add item2 |> ignore
            //               else dbContext.Add item2 |> ignore

            //static member addToContext (dbContext : MzIdentML) (item:Ontology) =
            //    query {
            //        for i in dbContext.Ontology.Local do
            //            if item.Terms = item.Terms
            //               then select i
            //          }
            //    |> (fun term -> 
            //        if Seq.length term < 1 
            //            then 
            //                query {
            //                       for i in dbContext.Term do
            //                           if item.Terms = item.Terms
            //                              then select i
            //                      }
            //                |> (fun term' -> if (term'.Count()) < 1
            //                                    then let tmp = dbContext.Ontology.Find(item.ID)
            //                                         if tmp.ID = item.ID 
            //                                            then item.ID <- System.Guid.NewGuid().ToString()
            //                                                 dbContext.Add item |> ignore
            //                                            else dbContext.Add item |> ignore
            //                                    else term'
            //                                         |> Seq.iter (fun termItem -> OntologyHandler.matchAndAddOntologies dbContext termItem item)
            //                   )
            //            else term
            //                 |> Seq.iter (fun termItem -> OntologyHandler.matchAndAddOntologies dbContext termItem item)
            //       )

            //static member addToContextAndInsert (dbContext : MzIdentML) (item:Term) =
            //    TermHandler.addToContext dbContext item |> ignore
            //    dbContext.SaveChanges()

        type CVParamHandler =
            ///Initializes a cvparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>

                new CVParam(
                            Nullable(id'), 
                            value', 
                            term, 
                            unit', 
                            Nullable(DateTime.Now)
                           )

            static member addValue
                (value:string) (cvParam:CVParam) =
                cvParam.Value <- value
                cvParam

            static member addUnit
                (unit:Term) (cvParam:CVParam) =
                cvParam.Unit <- unit
                cvParam

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.CVParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:CVParam) (item2:CVParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:CVParam) =
                query {
                       for i in dbContext.CVParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.CVParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.CVParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> CVParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> CVParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:CVParam) =
                CVParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type OrganizationParamHandler =
            ///Initializes a organizationparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>

                new OrganizationParam(
                                      Nullable(id'), 
                                      value', 
                                      term, 
                                      unit', 
                                      Nullable(DateTime.Now)
                                     )

            static member addValue
                (value:string) (param:OrganizationParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:OrganizationParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.OrganizationParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:OrganizationParam) (item2:OrganizationParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:OrganizationParam) =
                query {
                       for i in dbContext.OrganizationParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.OrganizationParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.OrganizationParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> OrganizationParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> OrganizationParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:OrganizationParam) =
                OrganizationParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type PersonParamHandler =
            ///Initializes a personparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new PersonParam(
                                Nullable(id'), 
                                value', 
                                term, 
                                unit', 
                                Nullable(DateTime.Now)
                               )

            static member addValue
                (value:string) (param:PersonParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:PersonParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.PersonParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:PersonParam) (item2:PersonParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:PersonParam) =
                query {
                       for i in dbContext.PersonParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.PersonParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.PersonParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> PersonParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> PersonParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:PersonParam) =
                PersonParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type SampleParamHandler =
            ///Initializes a sampleparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term,
                    ?unitName : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new SampleParam(
                                Nullable(id'), 
                                value', 
                                term, 
                                unit', 
                                Nullable(DateTime.Now)
                               )

            static member addValue
                (value:string) (param:SampleParam)  =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:SampleParam)  =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.SampleParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:SampleParam) (item2:SampleParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SampleParam) =
                query {
                       for i in dbContext.SampleParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.SampleParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.SampleParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> SampleParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> SampleParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SampleParam) =
                SampleParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type ModificationParamHandler =
            ///Initializes a modificationparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new ModificationParam(
                                      Nullable(id'), 
                                      value', 
                                      term, 
                                      unit', 
                                      Nullable(DateTime.Now)
                                     )

            static member addValue
                (value:string) (param:ModificationParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:ModificationParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.ModificationParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:ModificationParam) (item2:ModificationParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:ModificationParam) =
                query {
                       for i in dbContext.ModificationParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.ModificationParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.ModificationParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> ModificationParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> ModificationParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:ModificationParam) =
                ModificationParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type PeptideParamHandler =
            ///Initializes a peptideparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new PeptideParam(
                                 Nullable(id'), 
                                 value', 
                                 term, 
                                 unit', 
                                 Nullable(DateTime.Now)
                                )

            static member addValue
                (value:string) (param:PeptideParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:PeptideParam)  =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.PeptideParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:PeptideParam) (item2:PeptideParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:PeptideParam) =
                query {
                       for i in dbContext.PeptideParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.PeptideParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.ModificationParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> PeptideParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> PeptideParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:PeptideParam) =
                PeptideParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type TranslationTableParamHandler =
            ///Initializes a translationtableparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new TranslationTableParam(
                                          Nullable(id'), 
                                          value', 
                                          term, 
                                          unit', 
                                          Nullable(DateTime.Now)
                                         )

            static member addValue
                (value:string) (param:TranslationTableParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:TranslationTableParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.TranslationTableParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:TranslationTableParam) (item2:TranslationTableParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:TranslationTableParam) =
                query {
                       for i in dbContext.TranslationTableParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.TranslationTableParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.TranslationTableParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> TranslationTableParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> TranslationTableParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:TranslationTableParam) =
                TranslationTableParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type MeasureParamHandler =
            ///Initializes a measureparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new MeasureParam(
                                 Nullable(id'), 
                                 value', 
                                 term, 
                                 unit', 
                                 Nullable(DateTime.Now)
                                )

            static member addValue
                (value:string) (param:MeasureParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:MeasureParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.MeasureParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:MeasureParam) (item2:MeasureParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:MeasureParam) =
                query {
                       for i in dbContext.MeasureParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.MeasureParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.MeasureParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> MeasureParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> MeasureParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:MeasureParam) =
                MeasureParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type AmbiguousResidueParamHandler =
            ///Initializes a ambiguousresidueparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>

                new AmbiguousResidueParam(
                                          Nullable(id'), 
                                          value', 
                                          term, 
                                          unit', 
                                          Nullable(DateTime.Now)
                                         )

            static member addValue
                (value:string) (param:AmbiguousResidueParam)  =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:AmbiguousResidueParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.AmbiguousResidueParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:AmbiguousResidueParam) (item2:AmbiguousResidueParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:AmbiguousResidueParam) =
                query {
                       for i in dbContext.AmbiguousResidueParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.AmbiguousResidueParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.AmbiguousResidueParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> AmbiguousResidueParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> AmbiguousResidueParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:AmbiguousResidueParam) =
                AmbiguousResidueParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type MassTableParamHandler =
            ///Initializes a masstableparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new MassTableParam(
                                   Nullable(id'), 
                                   value', 
                                   term, 
                                   unit', 
                                   Nullable(DateTime.Now)
                                  )

            static member addValue
                (value:string) (param:MassTableParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:MassTableParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.MassTableParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:MassTableParam) (item2:MassTableParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:MassTableParam) =
                query {
                       for i in dbContext.MassTableParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.MassTableParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.MassTableParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> MassTableParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> MassTableParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:MassTableParam) =
                MassTableParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type IonTypeParamHandler =
            ///Initializes a iontypeparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new IonTypeParam(
                                 Nullable(id'), 
                                 value', 
                                 term, 
                                 unit', 
                                 Nullable(DateTime.Now)
                                )

            static member addValue
                (value:string) (param:IonTypeParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:IonTypeParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.IonTypeParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:IonTypeParam) (item2:IonTypeParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:IonTypeParam) =
                query {
                       for i in dbContext.IonTypeParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.IonTypeParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.IonTypeParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> IonTypeParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> IonTypeParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:IonTypeParam) =
                IonTypeParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type SpecificityRuleParamHandler =
            ///Initializes a specificityruleparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>

                new SpecificityRuleParam(
                                         Nullable(id'), 
                                         value', 
                                         term, 
                                         unit', 
                                         Nullable(DateTime.Now)
                                        )

            static member addValue
                (value:string) (param:SpecificityRuleParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:SpecificityRuleParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.SpecificityRuleParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:SpecificityRuleParam) (item2:SpecificityRuleParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SpecificityRuleParam) =
                query {
                       for i in dbContext.SpecificityRuleParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.SpecificityRuleParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.SpecificityRuleParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> SpecificityRuleParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> SpecificityRuleParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SpecificityRuleParam) =
                SpecificityRuleParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type SearchModificationParamHandler =
            ///Initializes a searchmodificationparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new SearchModificationParam(
                                            Nullable(id'), 
                                            value', 
                                            term, 
                                            unit',  
                                            Nullable(DateTime.Now)
                                           )

            static member addValue
                (value:string) (param:SearchModificationParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:SearchModificationParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.SearchModificationParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:SearchModificationParam) (item2:SearchModificationParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SearchModificationParam) =
                query {
                       for i in dbContext.SearchModificationParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.SearchModificationParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.SearchModificationParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> SearchModificationParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> SearchModificationParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SearchModificationParam) =
                SearchModificationParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type EnzymeNameParamHandler =
            ///Initializes a enzymenameparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new EnzymeNameParam(
                                    Nullable(id'),  
                                    value', 
                                    term, 
                                    unit', 
                                    Nullable(DateTime.Now)
                                   )

            static member addValue
                (value:string) (param:EnzymeNameParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:EnzymeNameParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.EnzymeNameParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:EnzymeNameParam) (item2:EnzymeNameParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:EnzymeNameParam) =
                query {
                       for i in dbContext.EnzymeNameParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.EnzymeNameParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.EnzymeNameParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> EnzymeNameParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> EnzymeNameParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:EnzymeNameParam) =
                EnzymeNameParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type IncludeParamHandler =
            ///Initializes a includeparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new IncludeParam(
                                 Nullable(id'), 
                                 value', 
                                 term, 
                                 unit',  
                                 Nullable(DateTime.Now)
                                )

            static member addValue
                (value:string) (param:IncludeParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:IncludeParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.EnzymeNameParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:IncludeParam) (item2:IncludeParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:IncludeParam) =
                query {
                       for i in dbContext.IncludeParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.IncludeParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.IncludeParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> IncludeParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> IncludeParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:IncludeParam) =
                IncludeParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type ExcludeParamHandler =
            ///Initializes a excludeparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new ExcludeParam(
                                 Nullable(id'), 
                                 value', 
                                 term, 
                                 unit',  
                                 Nullable(DateTime.Now)
                                )

            static member addValue
                (value:string) (param:ExcludeParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:ExcludeParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.ExcludeParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:ExcludeParam) (item2:ExcludeParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:ExcludeParam) =
                query {
                       for i in dbContext.ExcludeParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.ExcludeParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.ExcludeParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> ExcludeParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> ExcludeParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:ExcludeParam) =
                ExcludeParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type AdditionalSearchParamHandler =
            ///Initializes a additionalssearchparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>

                new AdditionalSearchParam(
                                          Nullable(id'), 
                                          value', 
                                          term, 
                                          unit',  
                                          Nullable(DateTime.Now)
                                         )

            static member addValue
                (value:string) (param:AdditionalSearchParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:AdditionalSearchParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.AdditionalSearchParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:AdditionalSearchParam) (item2:AdditionalSearchParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:AdditionalSearchParam) =
                query {
                       for i in dbContext.AdditionalSearchParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.AdditionalSearchParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.AdditionalSearchParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> AdditionalSearchParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> AdditionalSearchParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:AdditionalSearchParam) =
                AdditionalSearchParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type FragmentToleranceParamHandler =
            ///Initializes a fragmenttoleranceparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new FragmentToleranceParam(
                                           Nullable(id'),  
                                           value', 
                                           term, 
                                           unit', 
                                           Nullable(DateTime.Now)
                                          )

            static member addValue
                (value:string) (param:FragmentToleranceParam)  =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:FragmentToleranceParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.FragmentToleranceParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:FragmentToleranceParam) (item2:FragmentToleranceParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:FragmentToleranceParam) =
                query {
                       for i in dbContext.FragmentToleranceParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.FragmentToleranceParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.FragmentToleranceParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> FragmentToleranceParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> FragmentToleranceParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:FragmentToleranceParam) =
                FragmentToleranceParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type ParentToleranceParamHandler =
            ///Initializes a parenttoleranceparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new ParentToleranceParam(
                                         Nullable(id'), 
                                         value', 
                                         term, 
                                         unit', 
                                         Nullable(DateTime.Now)
                                        )

            static member addValue
                (value:string) (param:ParentToleranceParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:ParentToleranceParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.ParentToleranceParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:ParentToleranceParam) (item2:ParentToleranceParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:ParentToleranceParam) =
                query {
                       for i in dbContext.ParentToleranceParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.ParentToleranceParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.ParentToleranceParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> ParentToleranceParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> ParentToleranceParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:ParentToleranceParam) =
                ParentToleranceParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type ThresholdParamHandler =
            ///Initializes a thresholdparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new ThresholdParam(
                                   Nullable(id'), 
                                   value', 
                                   term, 
                                   unit', 
                                   Nullable(DateTime.Now)
                                  )

            static member addValue
                (value:string) (param:ThresholdParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:ThresholdParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.ThresholdParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:ThresholdParam) (item2:ThresholdParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:ThresholdParam) =
                query {
                       for i in dbContext.ThresholdParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.ThresholdParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.ThresholdParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> ThresholdParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> ThresholdParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:ThresholdParam) =
                ThresholdParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type SearchDatabaseParamHandler =
            ///Initializes a searchdatabaseparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new SearchDatabaseParam(
                                        Nullable(id'), 
                                        value', 
                                        term, 
                                        unit', 
                                        Nullable(DateTime.Now)
                                       )

            static member addValue
                (value:string) (param:SearchDatabaseParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:SearchDatabaseParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.SearchDatabaseParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:SearchDatabaseParam) (item2:SearchDatabaseParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SearchDatabaseParam) =
                query {
                       for i in dbContext.SearchDatabaseParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.SearchDatabaseParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.SearchDatabaseParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> SearchDatabaseParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> SearchDatabaseParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SearchDatabaseParam) =
                SearchDatabaseParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type DBSequenceParamHandler =
            ///Initializes a dbsequenceparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new DBSequenceParam(
                                    Nullable(id'), 
                                    value', 
                                    term, 
                                    unit', 
                                    Nullable(DateTime.Now)
                                   )

            static member addValue
                (value:string) (param:DBSequenceParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:DBSequenceParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.DBSequenceParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:DBSequenceParam) (item2:DBSequenceParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:DBSequenceParam) =
                query {
                       for i in dbContext.DBSequenceParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.DBSequenceParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.DBSequenceParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> DBSequenceParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> DBSequenceParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:DBSequenceParam) =
                DBSequenceParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type PeptideEvidenceParamHandler =
            ///Initializes a peptideevidenceparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term,
                    ?unitName : string
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new PeptideEvidenceParam(
                                         Nullable(id'),
                                         value', 
                                         term, 
                                         unit', 
                                         Nullable(DateTime.Now)
                                        )

            static member addValue
                (value:string) (param:PeptideEvidenceParam)  =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:PeptideEvidenceParam)  =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.PeptideEvidenceParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:PeptideEvidenceParam) (item2:PeptideEvidenceParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:PeptideEvidenceParam) =
                query {
                       for i in dbContext.PeptideEvidenceParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.PeptideEvidenceParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.PeptideEvidenceParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> PeptideEvidenceParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> PeptideEvidenceParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:PeptideEvidenceParam) =
                PeptideEvidenceParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationItemParamHandler =
            ///Initializes a spectrumidentificationparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>

                new SpectrumIdentificationItemParam(
                                                    Nullable(id'), 
                                                    value', 
                                                    term, 
                                                    unit',  
                                                    Nullable(DateTime.Now)
                                                   )

            static member addValue
                (value:string) (param:SpectrumIdentificationItemParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:SpectrumIdentificationItemParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.SpectrumIdentificationItemParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:SpectrumIdentificationItemParam) (item2:SpectrumIdentificationItemParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SpectrumIdentificationItemParam) =
                query {
                       for i in dbContext.SpectrumIdentificationItemParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationItemParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.SpectrumIdentificationItemParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> SpectrumIdentificationItemParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> SpectrumIdentificationItemParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SpectrumIdentificationItemParam) =
                SpectrumIdentificationItemParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationResultParamHandler =
            ///Initializes a spectrumidentificationresultparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>

                new SpectrumIdentificationResultParam(
                                                      Nullable(id'), 
                                                      value', 
                                                      term, 
                                                      unit', 
                                                      Nullable(DateTime.Now)
                                                     )

            static member addValue
                (value:string) (param:SpectrumIdentificationResultParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:SpectrumIdentificationResultParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.SpectrumIdentificationResultParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:SpectrumIdentificationResultParam) (item2:SpectrumIdentificationResultParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SpectrumIdentificationResultParam) =
                query {
                       for i in dbContext.SpectrumIdentificationResultParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationResultParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.SpectrumIdentificationResultParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> SpectrumIdentificationResultParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> SpectrumIdentificationResultParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SpectrumIdentificationResultParam) =
                SpectrumIdentificationResultParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationListParamHandler =
            ///Initializes a spectrumidentificationlistparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>

                new SpectrumIdentificationListParam(
                                                    Nullable(id'), 
                                                    value', 
                                                    term, 
                                                    unit',  
                                                    Nullable(DateTime.Now)
                                                   )

            static member addValue
                (value:string) (param:SpectrumIdentificationListParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:SpectrumIdentificationListParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.SpectrumIdentificationListParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:SpectrumIdentificationListParam) (item2:SpectrumIdentificationListParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SpectrumIdentificationListParam) =
                query {
                       for i in dbContext.SpectrumIdentificationListParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationListParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.SpectrumIdentificationListParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> SpectrumIdentificationListParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> SpectrumIdentificationListParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SpectrumIdentificationListParam) =
                SpectrumIdentificationListParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type AnalysisParamHandler =
            ///Initializes a analysisparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new AnalysisParam(
                                  Nullable(id'),  
                                  value', 
                                  term, 
                                  unit',  
                                  Nullable(DateTime.Now)
                                 )

            static member addValue
                (value:string) (param:AnalysisParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:AnalysisParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.AnalysisParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:AnalysisParam) (item2:AnalysisParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:AnalysisParam) =
                query {
                       for i in dbContext.AnalysisParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.AnalysisParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.AnalysisParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> AnalysisParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> AnalysisParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:AnalysisParam) =
                AnalysisParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type SourceFileParamHandler =
            ///Initializes a sourcefileparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new SourceFileParam(
                                    Nullable(id'),  
                                    value', 
                                    term, 
                                    unit', 
                                    Nullable(DateTime.Now)
                                   )

            static member addValue
                (value:string) (param:SourceFileParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:SourceFileParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.SourceFileParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:SourceFileParam) (item2:SourceFileParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SourceFileParam) =
                query {
                       for i in dbContext.SourceFileParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.SourceFileParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.SourceFileParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> SourceFileParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> SourceFileParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SourceFileParam) =
                SourceFileParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinDetectionHypothesisParamHandler =
            ///Initializes a proteindetectionhypothesisparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    
                new ProteinDetectionHypothesisParam(
                                                    Nullable(id'), 
                                                    value', 
                                                    term, 
                                                    unit',  
                                                    Nullable(DateTime.Now)
                                                   )

            static member addValue
                (value:string) (param:ProteinDetectionHypothesisParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:ProteinDetectionHypothesisParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.ProteinDetectionHypothesisParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:ProteinDetectionHypothesisParam) (item2:ProteinDetectionHypothesisParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:ProteinDetectionHypothesisParam) =
                query {
                       for i in dbContext.ProteinDetectionHypothesisParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionHypothesisParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.ProteinDetectionHypothesisParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> ProteinDetectionHypothesisParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> ProteinDetectionHypothesisParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:ProteinDetectionHypothesisParam) =
                ProteinDetectionHypothesisParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinAmbiguityGroupParamHandler =
            ///Initializes a proteinambiguitygroupparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>

                new  ProteinAmbiguityGroupParam(
                                                Nullable(id'), 
                                                value', 
                                                term, 
                                                unit', 
                                                Nullable(DateTime.Now)
                                               )

            static member addValue
                (value:string) (param:ProteinAmbiguityGroupParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:ProteinAmbiguityGroupParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.ProteinAmbiguityGroupParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:ProteinAmbiguityGroupParam) (item2:ProteinAmbiguityGroupParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:ProteinAmbiguityGroupParam) =
                query {
                       for i in dbContext.ProteinAmbiguityGroupParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.ProteinAmbiguityGroupParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.ProteinAmbiguityGroupParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> ProteinAmbiguityGroupParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> ProteinAmbiguityGroupParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:ProteinAmbiguityGroupParam) =
                ProteinAmbiguityGroupParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinDetectionListParamHandler =
            ///Initializes a proteindetectionlistparam-object with at least all necessary parameters.
            static member init
                (
                    term      : Term,
                    ?id       : Guid,
                    ?value    : string,
                    ?unit     : Term
                ) =
                let id'       = defaultArg id (System.Guid.NewGuid())
                let value'    = defaultArg value Unchecked.defaultof<string>
                let unit'     = defaultArg unit Unchecked.defaultof<Term>

                new ProteinDetectionListParam(
                                              Nullable(id'),
                                              value', 
                                              term, 
                                              unit', 
                                              Nullable(DateTime.Now)
                                             )

            static member addValue
                (value:string) (param:ProteinDetectionListParam) =
                param.Value <- value
                param

            static member addUnit
                (unit:Term) (param:ProteinDetectionListParam) =
                param.Unit <- unit
                param

            static member tryFindByID
                (context:MzIdentML) (paramID:string) =
                tryFind (context.ProteinDetectionListParam.Find(paramID))

            static member private matchAndAddParams (dbContext:MzIdentML) (item1:ProteinDetectionListParam) (item2:ProteinDetectionListParam) =
                if item1.Value=item2.Value && item1.Unit=item2.Unit
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:ProteinDetectionListParam) =
                query {
                       for i in dbContext.ProteinDetectionListParam.Local do
                           if i.Term = item.Term
                              then select (i, i.Term, i.Unit)
                      }
                |> Seq.map (fun (param,_ ,_) -> param)
                |> (fun param -> 
                    if Seq.length param < 1 
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionListParam do
                                       if i.Term = item.Term
                                          then select (i, i.Term, i.Unit)
                                  }
                            |> Seq.map (fun (param,_ ,_) -> param)
                            |> (fun param' -> if (param'.Count()) < 1
                                                then let tmp = dbContext.ProteinDetectionListParam.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else param'
                                                     |> Seq.iter (fun paramItem -> ProteinDetectionListParamHandler.matchAndAddParams dbContext paramItem item)
                               )
                        else param
                             |> Seq.iter (fun paramItem -> ProteinDetectionListParamHandler.matchAndAddParams dbContext paramItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:ProteinDetectionListParam) =
                ProteinDetectionListParamHandler.matchAndAddParams dbContext item |> ignore
                dbContext.SaveChanges()

        type OrganizationHandler =
            ///Initializes a organization-object with at least all necessary parameters.
            static member init
                (
                    ?id      : Guid,
                    ?name    : string,
                    ?details : seq<OrganizationParam>,
                    ?parent  : string
                ) =
                let id'      = defaultArg id (System.Guid.NewGuid())
                let name'    = defaultArg name Unchecked.defaultof<string>
                let details' = convertOptionToList details
                let parent'  = defaultArg parent Unchecked.defaultof<string>
                    
                new Organization(
                                 Nullable(id'), 
                                 name', 
                                 details',  
                                 parent', 
                                 Nullable(DateTime.Now)
                                )

            static member addName
                (name:string) (organization:Organization) =
                organization.Name <- name
                organization

            static member addParent
                (parent:string) (organization:Organization) =
                organization.Parent <- parent
                organization

            static member addDetail
                (detail:OrganizationParam) (organization:Organization) =
                let result = organization.Details <- addToList organization.Details detail
                organization

            static member addDetails
                (details:seq<OrganizationParam>) (organization:Organization) =
                let result = organization.Details <- addCollectionToList organization.Details details
                organization

            static member tryFindByID
                (context:MzIdentML) (organizationID:string) =
                tryFind (context.Organization.Find(organizationID))

            static member private matchAndAddOrganizations (dbContext:MzIdentML) (item1:Organization) (item2:Organization) =
                if item1.Details=item2.Details && item1.Parent=item2.Parent
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Organization) =
                query {
                       for i in dbContext.Organization.Local do
                           if i.Name = item.Name
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (organization, _ ) -> organization)
                |> (fun organization -> 
                    if Seq.length organization < 1 
                        then 
                            query {
                                   for i in dbContext.Organization do
                                       if i.Name = item.Name
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (organization, _ ) -> organization)
                            |> (fun organization' -> if (organization'.Count()) < 1
                                                     then let tmp = dbContext.Organization.Find(item.ID)
                                                          if tmp.ID = item.ID 
                                                             then item.ID <- Nullable(System.Guid.NewGuid())
                                                                  dbContext.Add item |> ignore
                                                             else dbContext.Add item |> ignore
                                                     else organization'
                                                          |> Seq.iter (fun organizationItem -> OrganizationHandler.matchAndAddOrganizations dbContext organizationItem item)
                               )
                        else organization
                             |> Seq.iter (fun organizationItem -> OrganizationHandler.matchAndAddOrganizations dbContext organizationItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Organization) =
                OrganizationHandler.matchAndAddOrganizations dbContext item |> ignore
                dbContext.SaveChanges()

        type PersonHandler =
        ///Initializes a person-object with at least all necessary parameters.
            static member init
                (
                    ?id             : Guid,
                    ?name           : string,
                    ?firstName      : string,
                    ?midInitials    : string,
                    ?lastName       : string,
                    ?contactDetails : seq<PersonParam>,
                    ?organizations  : seq<Organization> 
                ) =
                let id'          = defaultArg id (System.Guid.NewGuid())
                let name'        = defaultArg name Unchecked.defaultof<string>
                let firstName'   = defaultArg firstName Unchecked.defaultof<string>
                let midInitials' = defaultArg midInitials Unchecked.defaultof<string>
                let lastName'    = defaultArg lastName Unchecked.defaultof<string>
                    
                new Person(
                           Nullable(id'), 
                           name', 
                           firstName',  
                           midInitials', 
                           lastName', 
                           convertOptionToList organizations,convertOptionToList contactDetails, 
                           Nullable(DateTime.Now)
                          )

            static member addName
                (name:string) (person:Person) =
                person.Name <- name
                person

            static member addFirstName
                (firstName:string) (person:Person) =
                person.FirstName <- firstName
                person

            static member addMidInitials
                (midInitials:string) (person:Person) =
                person.MidInitials <- midInitials
                person

            static member addLastName
                (lastName:string) (person:Person) =
                person.LastName <- lastName
                person

            static member addDetail (detail:PersonParam) (person:Person) =
                let result = person.Details <- addToList person.Details detail
                person

            static member addDetails
                (details:seq<PersonParam>) (person:Person) =
                let result = person.Details <- addCollectionToList person.Details details
                person

            static member addOrganization
                (organization:Organization) (person:Person) =
                let result = person.Organizations <- addToList person.Organizations organization
                person

            static member addOrganizations
                (organizations:seq<Organization>) (person:Person) =
                let result = person.Organizations <- addCollectionToList person.Organizations organizations
                person

            static member tryFindByID
                (context:MzIdentML) (personID:string) =
                tryFind (context.Person.Find(personID))

            static member private matchAndAddPersons (dbContext:MzIdentML) (item1:Person) (item2:Person) =
                if item1.FirstName=item2.FirstName && item1.FirstName=item2.FirstName && 
                   item1.MidInitials=item2.MidInitials && item1.LastName=item2.LastName && 
                   item1.Organizations=item2.Organizations && item1.Details=item2.Details
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Person) =
                query {
                       for i in dbContext.Person.Local do
                           if i.Name = item.Name
                              then select (i, i.Details, i.Organizations)
                      }
                |> Seq.map (fun (person, _, _) -> person)
                |> (fun person -> 
                    if Seq.length person < 1 
                        then 
                            query {
                                   for i in dbContext.Person do
                                       if i.Name = item.Name
                                          then select (i, i.Details, i.Organizations)
                                  }
                            |> Seq.map (fun (person, _, _) -> person)
                            |> (fun person' -> if (person'.Count()) < 1
                                                then let tmp = dbContext.Person.Find(item.ID)
                                                     if tmp.ID = item.ID 
                                                        then item.ID <- Nullable(System.Guid.NewGuid())
                                                             dbContext.Add item |> ignore
                                                        else dbContext.Add item |> ignore
                                                else person'
                                                     |> Seq.iter (fun personItem -> PersonHandler.matchAndAddPersons dbContext personItem item)
                               )
                        else person
                             |> Seq.iter (fun personItem -> PersonHandler.matchAndAddPersons dbContext personItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Person) =
                PersonHandler.matchAndAddPersons dbContext item |> ignore
                dbContext.SaveChanges()

        type ContactRoleHandler =
            ///Initializes a contactrole-object with at least all necessary parameters.
            static member init
                (   
                    person : Person, 
                    role   : CVParam,
                    ?id    : Guid
                ) =
                let id' = defaultArg id (System.Guid.NewGuid())
                    
                new ContactRole(
                                Nullable(id'), 
                                person, 
                                role, 
                                Nullable(DateTime.Now)
                               )

            static member tryFindByID
                (context:MzIdentML) (contactRoleID:string) =
                tryFind (context.ContactRole.Find(contactRoleID))

            static member private matchAndAddContactRoles (dbContext:MzIdentML) (item1:ContactRole) (item2:ContactRole) =
                if item1.Role=item2.Role
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:ContactRole) =
                query {
                       for i in dbContext.ContactRole.Local do
                           if i.Person = item.Person
                              then select (i, i.Role)
                      }
                |> Seq.map (fun (contactRole, _) -> contactRole)
                |> (fun contactRole -> 
                    if Seq.length contactRole < 1 
                        then 
                            query {
                                   for i in dbContext.ContactRole do
                                       if i.Person = item.Person
                                          then select (i, i.Role)
                                  }
                            |> Seq.map (fun (contactRole, _) -> contactRole)
                            |> (fun contactRole' -> if (contactRole'.Count()) < 1
                                                    then let tmp = dbContext.ContactRole.Find(item.ID)
                                                         if tmp.ID = item.ID 
                                                            then item.ID <- Nullable(System.Guid.NewGuid())
                                                                 dbContext.Add item |> ignore
                                                            else dbContext.Add item |> ignore
                                                    else contactRole'
                                                         |> Seq.iter (fun contactRoleItem -> ContactRoleHandler.matchAndAddContactRoles dbContext contactRoleItem item)
                               )
                        else contactRole
                             |> Seq.iter (fun contactRoleItem -> ContactRoleHandler.matchAndAddContactRoles dbContext contactRoleItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:ContactRole) =
                ContactRoleHandler.matchAndAddContactRoles dbContext item |> ignore
                dbContext.SaveChanges()

        type AnalysisSoftwareHandler =
            ///Initializes a analysissoftware-object with at least all necessary parameters.
            static member init
                (
                    softwareName       : CVParam,
                    ?id                : Guid,
                    ?name              : string,
                    ?uri               : string,
                    ?version           : string,
                    ?customizations    : string,
                    ?softwareDeveloper : ContactRole,
                    ?mzIdentML         : MzIdentMLDocument
                ) =
                let id'             = defaultArg id (System.Guid.NewGuid())
                let name'           = defaultArg name Unchecked.defaultof<string>
                let uri'            = defaultArg uri Unchecked.defaultof<string>
                let version'        = defaultArg version Unchecked.defaultof<string>
                let customizations' = defaultArg customizations Unchecked.defaultof<string>
                let contactRole'    = defaultArg softwareDeveloper Unchecked.defaultof<ContactRole>
                let mzIdentML'      = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                new AnalysisSoftware(
                                     Nullable(id'), 
                                     name', 
                                     uri', 
                                     version', 
                                     customizations', 
                                     contactRole', 
                                     softwareName, 
                                     mzIdentML', 
                                     Nullable(DateTime.Now)
                                    )

            static member addName
                (name:string) (analysisSoftware:AnalysisSoftware) =
                analysisSoftware.Name <- name
                analysisSoftware

            static member addURI
                (uri:string) (analysisSoftware:AnalysisSoftware) =
                analysisSoftware.URI <- uri
                analysisSoftware

            static member addVersion
                (version:string) (analysisSoftware:AnalysisSoftware) =
                analysisSoftware.Version <- version
                analysisSoftware

            static member addCustomization
                (customizations:string) (analysisSoftware:AnalysisSoftware) =
                analysisSoftware.Customizations <- customizations
                analysisSoftware

            static member addAnalysisSoftwareDeveloper
                (analysisSoftwareDeveloper:ContactRole) (analysisSoftware:AnalysisSoftware)=
                analysisSoftware.ContactRole <- analysisSoftwareDeveloper
                analysisSoftware

            static member addMzIdentML
                (mzIdentMLDocument:MzIdentMLDocument) (analysisSoftware:AnalysisSoftware)=
                let result = analysisSoftware.MzIdentMLDocument <- mzIdentMLDocument
                analysisSoftware

            static member tryFindByID
                (context:MzIdentML) (analysisSoftwareID:string) =
                tryFind (context.AnalysisSoftware.Find(analysisSoftwareID))

            static member private matchAndAddAnalysisSoftwares (dbContext:MzIdentML) (item1:AnalysisSoftware) (item2:AnalysisSoftware) =
                if item1.Name=item2.Name && item1.URI=item2.URI && item1.Version=item2.Version && 
                   item1.Customizations=item2.Customizations && item1.ContactRole=item2.ContactRole && 
                   item1.MzIdentMLDocument=item2.MzIdentMLDocument
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:AnalysisSoftware) =
                query {
                       for i in dbContext.AnalysisSoftware.Local do
                           if i.SoftwareName=item.SoftwareName
                              then select (i, i.ContactRole, i.MzIdentMLDocument)
                      }
                |> Seq.map (fun (analysisSoftware, _, _) -> analysisSoftware)
                |> (fun analysisSoftware -> 
                    if Seq.length analysisSoftware < 1 
                        then 
                            query {
                                   for i in dbContext.AnalysisSoftware do
                                       if i.SoftwareName=item.SoftwareName
                                          then select (i, i.ContactRole, i.MzIdentMLDocument)
                                  }
                            |> Seq.map (fun (analysisSoftware, _, _) -> analysisSoftware)
                            |> (fun analysisSoftware' -> if (analysisSoftware'.Count()) < 1
                                                         then let tmp = dbContext.AnalysisSoftware.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else analysisSoftware'
                                                              |> Seq.iter (fun analysisSoftwareItem -> AnalysisSoftwareHandler.matchAndAddAnalysisSoftwares dbContext analysisSoftwareItem item)
                               )
                        else analysisSoftware
                             |> Seq.iter (fun analysisSoftwareItem -> AnalysisSoftwareHandler.matchAndAddAnalysisSoftwares dbContext analysisSoftwareItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:AnalysisSoftware) =
                AnalysisSoftwareHandler.matchAndAddAnalysisSoftwares dbContext item |> ignore
                dbContext.SaveChanges()

        type SubSampleHandler =
            ///Initializes a subsample-object with at least all necessary parameters.
            static member init
                (
                    ?id          : Guid,
                    ?sample      : Sample
                ) =
                let id'          = defaultArg id (System.Guid.NewGuid())
                let Sample'      = defaultArg sample Unchecked.defaultof<Sample>
                    
                new SubSample(
                              Nullable(id'), 
                              Sample', 
                              Nullable(DateTime.Now)
                             )

            static member addSample
                (sampleID:Sample) (subSample:SubSample) =
                subSample.Sample <- sampleID
                subSample

            static member tryFindByID
                (context:MzIdentML) (subSampleID:string) =
                tryFind (context.SubSample.Find(subSampleID))

            static member private matchAndAddSubSamples (dbContext:MzIdentML) (item1:SubSample) (item2:SubSample) =
                if item1.ID = item2.ID
                   then item2.ID <- Nullable(System.Guid.NewGuid())
                        dbContext.Add item2 |> ignore
                   else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SubSample) =
                query {
                       for i in dbContext.SubSample.Local do
                           if i.Sample=item.Sample
                              then select (i, i.Sample)
                      }
                |> Seq.map (fun (subSample, _) -> subSample)
                |> (fun subSample -> 
                    if Seq.length subSample < 1 
                        then 
                            query {
                                   for i in dbContext.SubSample do
                                       if i.Sample=item.Sample
                                          then select (i, i.Sample)
                                  }
                            |> Seq.map (fun (subSample, _) -> subSample)
                            |> (fun subSample' -> if (subSample'.Count()) < 1
                                                         then let tmp = dbContext.SubSample.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else subSample'
                                                              |> Seq.iter (fun subSampleItem -> SubSampleHandler.matchAndAddSubSamples dbContext subSampleItem item)
                               )
                        else subSample
                             |> Seq.iter (fun subSampleItem -> SubSampleHandler.matchAndAddSubSamples dbContext subSampleItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SubSample) =
                SubSampleHandler.matchAndAddSubSamples dbContext item |> ignore
                dbContext.SaveChanges()

        type SampleHandler =
            ///Initializes a sample-object with at least all necessary parameters.
            static member init
                (
                    ?id           : Guid,
                    ?name         : string,
                    ?contactRoles : seq<ContactRole>,
                    ?subSamples   : seq<SubSample>,
                    ?details      : seq<SampleParam>,
                    ?mzIdentML    : MzIdentMLDocument
                ) =
                let id'           = defaultArg id (System.Guid.NewGuid())
                let name'         = defaultArg name Unchecked.defaultof<string>
                let contactRoles' = convertOptionToList contactRoles
                let subSamples'   = convertOptionToList subSamples
                let details'      = convertOptionToList details
                let mzIdentML'    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                new Sample(
                           Nullable(id'), 
                           name', 
                           contactRoles', 
                           subSamples', 
                           details', 
                           mzIdentML', 
                           Nullable(DateTime.Now)
                          )

            static member addName
                (name:string) (sample:Sample) =
                sample.Name <- name
                sample

            static member addContactRole
                (contactRole:ContactRole) (sample:Sample) =
                let result = sample.ContactRoles <- addToList sample.ContactRoles contactRole
                sample

            static member addContactRoles
                (contactRoles:seq<ContactRole>) (sample:Sample) =
                let result = sample.ContactRoles <- addCollectionToList sample.ContactRoles contactRoles
                sample

            static member addSubSample
                (subSample:SubSample) (sample:Sample) =
                let result = sample.SubSamples <- addToList sample.SubSamples subSample
                sample

            static member addSubSamples
                (subSamples:seq<SubSample>) (sample:Sample) =
                let result = sample.SubSamples <- addCollectionToList sample.SubSamples subSamples
                sample

            static member addDetail
                (detail:SampleParam) (sample:Sample) =
                let result = sample.Details <- addToList sample.Details detail
                sample

            static member addDetails
                (details:seq<SampleParam>) (sample:Sample) =
                let result = sample.Details <- addCollectionToList sample.Details details
                sample

            static member addMzIdentML
                (mzIdentMLDocument:MzIdentMLDocument) (sample:Sample) =
                let result = sample.MzIdentMLDocument <- mzIdentMLDocument
                sample

            static member tryFindByID
                (context:MzIdentML) (contactRolesID:string) =
                tryFind (context.Sample.Find(contactRolesID))

            static member private matchAndAddSamples (dbContext:MzIdentML) (item1:Sample) (item2:Sample) =
                if item1.ContactRoles=item2.ContactRoles && item1.SubSamples=item2.SubSamples &&
                   item1.Details=item2.Details 
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Sample) =
                query {
                       for i in dbContext.Sample.Local do
                           if i.Name=item.Name
                              then select (i, i.ContactRoles, i.SubSamples, i.Details)
                      }
                |> Seq.map (fun (sample, _, _, _) -> sample)
                |> (fun sample -> 
                    if Seq.length sample < 1 
                        then 
                            query {
                                   for i in dbContext.Sample do
                                       if i.Name=item.Name
                                          then select (i, i.ContactRoles, i.SubSamples, i.Details)
                                  }
                            |> Seq.map (fun (sample, _, _, _) -> sample)
                            |> (fun sample' -> if (sample'.Count()) < 1
                                                         then let tmp = dbContext.Sample.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else sample'
                                                              |> Seq.iter (fun sampleItem -> SampleHandler.matchAndAddSamples dbContext sampleItem item)
                               )
                        else sample
                             |> Seq.iter (fun sampleItem -> SampleHandler.matchAndAddSamples dbContext sampleItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Sample) =
                SampleHandler.matchAndAddSamples dbContext item |> ignore
                dbContext.SaveChanges()

        type ModificationHandler =
            ///Initializes a modification-object with at least all necessary parameters.
            static member init
                (
                    details                : seq<ModificationParam>,
                    ?id                    : Guid,
                    ?residues              : string,
                    ?location              : int,
                    ?monoIsotopicMassDelta : float,
                    ?avgMassDelta          : float
                ) =
                let id'                    = defaultArg id (System.Guid.NewGuid())
                let residues'              = defaultArg residues Unchecked.defaultof<string>
                let location'              = defaultArg location Unchecked.defaultof<int>
                let monoIsotopicMassDelta' = defaultArg monoIsotopicMassDelta Unchecked.defaultof<float>
                let avgMassDelta'          = defaultArg avgMassDelta Unchecked.defaultof<float>
                    
                new Modification(
                                 Nullable(id'), 
                                 residues', 
                                 Nullable(location'), 
                                 Nullable(monoIsotopicMassDelta'), 
                                 Nullable(avgMassDelta'), 
                                 details |> List, Nullable(DateTime.Now)
                                )

            static member addResidues
                (residues:string) (modification:Modification) =
                modification.Residues <- residues
                modification

            static member addLocation
                (location:int) (modification:Modification) =
                modification.Location <- Nullable(location)
                modification

            static member addMonoIsotopicMassDelta
                (monoIsotopicMassDelta:float) (modification:Modification) =
                modification.MonoIsotopicMassDelta <- Nullable(monoIsotopicMassDelta)
                modification

            static member addAvgMassDelta
                (avgMassDelta:float) (modification:Modification) =
                modification.AvgMassDelta <- Nullable(avgMassDelta)
                modification

            static member tryFindByID
                (context:MzIdentML) (modificationID:string) =
                tryFind (context.Modification.Find(modificationID))

            static member private matchAndModifications (dbContext:MzIdentML) (item1:Modification) (item2:Modification) =
                if item1.Residues=item2.Residues && item1.Location=item2.Location &&
                   item1.AvgMassDelta=item2.AvgMassDelta && item1.Details=item2.Details 
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Modification) =
                query {
                       for i in dbContext.Modification.Local do
                           if i.MonoIsotopicMassDelta=item.MonoIsotopicMassDelta
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (modification, _) -> modification)
                |> (fun modification -> 
                    if Seq.length modification < 1 
                        then 
                            query {
                                   for i in dbContext.Modification do
                                       if i.MonoIsotopicMassDelta=item.MonoIsotopicMassDelta
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (modification, _) -> modification)
                            |> (fun modification' -> if (modification'.Count()) < 1
                                                         then let tmp = dbContext.Modification.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else modification'
                                                              |> Seq.iter (fun modificationItem -> ModificationHandler.matchAndModifications dbContext modificationItem item)
                               )
                        else modification
                             |> Seq.iter (fun modificationItem -> ModificationHandler.matchAndModifications dbContext modificationItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Modification) =
                ModificationHandler.matchAndModifications dbContext item |> ignore
                dbContext.SaveChanges()

        type SubstitutionModificationHandler =
            ///Initializes a substitutionmodification-object with at least all necessary parameters.
            static member init
                (
                    originalResidue        : string,
                    replacementResidue     : string,
                    ?id                    : Guid,
                    ?location              : int,
                    ?monoIsotopicMassDelta : float,
                    ?avgMassDelta          : float
                ) =
                let id'                    = defaultArg id (System.Guid.NewGuid())
                let location'              = defaultArg location Unchecked.defaultof<int>
                let monoIsotopicMassDelta' = defaultArg monoIsotopicMassDelta Unchecked.defaultof<float>
                let avgMassDelta'          = defaultArg avgMassDelta Unchecked.defaultof<float>

                new SubstitutionModification(
                                             Nullable(id'), 
                                             originalResidue, 
                                             replacementResidue, 
                                             Nullable(location'), 
                                             Nullable(monoIsotopicMassDelta'), 
                                             Nullable(avgMassDelta'), 
                                             Nullable(DateTime.Now)
                                            )

            static member addLocation
                (location:int) (substitutionModification:SubstitutionModification) =
                substitutionModification.Location <- Nullable(location)
                substitutionModification

            static member addMonoIsotopicMassDelta
                (monoIsotopicMassDelta:float) (substitutionModification:SubstitutionModification) =
                substitutionModification.MonoIsotopicMassDelta <- Nullable(monoIsotopicMassDelta)
                substitutionModification

            static member addAvgMassDelta
                (avgMassDelta:float) (substitutionModification:SubstitutionModification) =
                substitutionModification.AvgMassDelta <- Nullable(avgMassDelta)
                substitutionModification

            static member tryFindByID
                (context:MzIdentML) (substitutionModificationID:string) =
                tryFind (context.SubstitutionModification.Find(substitutionModificationID))

            static member private matchAndAddSubstitutionModifications (dbContext:MzIdentML) (item1:SubstitutionModification) (item2:SubstitutionModification) =
                if item1.OriginalResidue=item2.OriginalResidue && item1.ReplacementResidue=item2.ReplacementResidue &&
                   item1.AvgMassDelta=item2.AvgMassDelta && item1.Location=item2.Location 
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SubstitutionModification) =
                query {
                       for i in dbContext.SubstitutionModification.Local do
                           if i.MonoIsotopicMassDelta=item.MonoIsotopicMassDelta
                              then select i
                      }
                |> (fun substitutionModification -> 
                    if Seq.length substitutionModification < 1 
                        then 
                            query {
                                   for i in dbContext.SubstitutionModification do
                                       if i.MonoIsotopicMassDelta=item.MonoIsotopicMassDelta
                                          then select i
                                  }
                            |> (fun substitutionModification' -> if (substitutionModification'.Count()) < 1
                                                                 then let tmp = dbContext.SubstitutionModification.Find(item.ID)
                                                                      if tmp.ID = item.ID 
                                                                         then item.ID <- Nullable(System.Guid.NewGuid())
                                                                              dbContext.Add item |> ignore
                                                                         else dbContext.Add item |> ignore
                                                                 else substitutionModification'
                                                                      |> Seq.iter (fun substitutionModificationItem -> SubstitutionModificationHandler.matchAndAddSubstitutionModifications dbContext substitutionModificationItem item)
                               )
                        else substitutionModification
                             |> Seq.iter (fun substitutionModificationItem -> SubstitutionModificationHandler.matchAndAddSubstitutionModifications dbContext substitutionModificationItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SubstitutionModification) =
                SubstitutionModificationHandler.matchAndAddSubstitutionModifications dbContext item |> ignore
                dbContext.SaveChanges()

        type PeptideHandler =
            ///Initializes a peptide-object with at least all necessary parameters.
            static member init
                (
                    peptideSequence            : string,
                    ?id                        : Guid,
                    ?name                      : string,                    
                    ?modifications             : seq<Modification>,
                    ?substitutionModifications : seq<SubstitutionModification>,
                    ?details                   : seq<PeptideParam>,
                    ?mzIdentML                 : MzIdentMLDocument
                ) =
                let id'                        = defaultArg id (System.Guid.NewGuid())
                let name'                      = defaultArg name Unchecked.defaultof<string>
                let modifications'             = convertOptionToList modifications
                let substitutionModifications' = convertOptionToList substitutionModifications
                let details'                   = convertOptionToList details
                let mzIdentML'                 = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new Peptide(
                            Nullable(id'), 
                            name', 
                            peptideSequence, 
                            modifications', 
                            substitutionModifications', 
                            details', 
                            mzIdentML', 
                            Nullable(DateTime.Now)
                           )

            static member addName
                (name:string) (peptide:Peptide) =
                peptide.Name <- name
                peptide

            static member addModification
                (modification:Modification) (peptide:Peptide) =
                let result = peptide.Modifications <- addToList peptide.Modifications modification
                peptide

            static member addModifications
                (modifications:seq<Modification>) (peptide:Peptide) =
                let result = peptide.Modifications <- addCollectionToList peptide.Modifications modifications
                peptide

            static member addSubstitutionModification
                (substitutionModification:SubstitutionModification) (peptide:Peptide) =
                let result = peptide.SubstitutionModifications <- addToList peptide.SubstitutionModifications substitutionModification
                peptide

            static member addSubstitutionModifications
                (substitutionModifications:seq<SubstitutionModification>) (peptide:Peptide) =
                let result = peptide.SubstitutionModifications <- addCollectionToList peptide.SubstitutionModifications substitutionModifications
                peptide

            static member addDetail
                (detail:PeptideParam) (peptide:Peptide) =
                let result = peptide.Details <- addToList peptide.Details detail
                peptide

            static member addDetails
                (details:seq<PeptideParam>) (peptide:Peptide) =
                let result = peptide.Details <- addCollectionToList peptide.Details details
                peptide

            static member addMzIdentML
                (mzIdentMLDocument:MzIdentMLDocument) (peptide:Peptide) =
                let result = peptide.MzIdentMLDocument <- mzIdentMLDocument
                peptide

            static member tryFindByID
                (context:MzIdentML) (peptideID:string) =
                tryFind (context.Peptide.Find(peptideID))

            static member private matchAndAddPeptides (dbContext:MzIdentML) (item1:Peptide) (item2:Peptide) =
                if item1.Modifications=item2.Modifications && item1.SubstitutionModifications=item2.SubstitutionModifications && 
                   item1.MzIdentMLDocument=item2.MzIdentMLDocument && item1.Details=item2.Details 
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Peptide) =
                query {
                       for i in dbContext.Peptide.Local do
                           if i.PeptideSequence=item.PeptideSequence
                              then select (i, i.Modifications, i.SubstitutionModifications, i.Details)
                      }
                |> Seq.map (fun (peptide, _, _, _) -> peptide)
                |> (fun peptide -> 
                    if Seq.length peptide < 1 
                        then 
                            query {
                                   for i in dbContext.Peptide do
                                       if i.PeptideSequence=item.PeptideSequence
                                          then select (i, i.Modifications, i.SubstitutionModifications, i.Details)
                                  }
                            |> Seq.map (fun (peptide, _, _, _) -> peptide)
                            |> (fun peptide' -> if (peptide'.Count()) < 1
                                                         then let tmp = dbContext.Peptide.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else peptide'
                                                              |> Seq.iter (fun peptidepeptideItem -> PeptideHandler.matchAndAddPeptides dbContext peptidepeptideItem item)
                               )
                        else peptide
                             |> Seq.iter (fun peptidepeptideItem -> PeptideHandler.matchAndAddPeptides dbContext peptidepeptideItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Peptide) =
                PeptideHandler.matchAndAddPeptides dbContext item |> ignore
                dbContext.SaveChanges()

        type TranslationTableHandler =
            ///Initializes a translationtable-object with at least all necessary parameters.
            static member init
                (
                    ?id      : Guid,
                    ?name    : string,
                    ?details : seq<TranslationTableParam>
                ) =
                let id'      = defaultArg id (System.Guid.NewGuid())
                let name'    = defaultArg name Unchecked.defaultof<string>
                let details' = convertOptionToList details
                    
                new TranslationTable(
                                     Nullable(id'), 
                                     name', 
                                     details', 
                                     Nullable(DateTime.Now)
                                    )

            static member addName
                (name:string) (translationTable:TranslationTable) =
                translationTable.Name <- name
                translationTable

            static member addDetail
                (detail:TranslationTableParam) (translationTable:TranslationTable) =
                let result = translationTable.Details <- addToList translationTable.Details detail
                translationTable

            static member addDetails
                (details:seq<TranslationTableParam>) (translationTable:TranslationTable) =
                let result = translationTable.Details <- addCollectionToList translationTable.Details details
                translationTable

            static member tryFindByID
                (context:MzIdentML) (translationTableID:string) =
                tryFind (context.TranslationTable.Find(translationTableID))

            static member private matchAndAddTranslationTables (dbContext:MzIdentML) (item1:TranslationTable) (item2:TranslationTable) =
                if item1.Details=item2.Details
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:TranslationTable) =
                query {
                       for i in dbContext.TranslationTable.Local do
                           if i.Name=item.Name
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (translationTable, _) -> translationTable)
                |> (fun translationTable -> 
                    if Seq.length translationTable < 1 
                        then 
                            query {
                                   for i in dbContext.TranslationTable do
                                       if i.Name=item.Name
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (translationTable, _) -> translationTable)
                            |> (fun translationTable' -> if (translationTable'.Count()) < 1
                                                         then let tmp = dbContext.TranslationTable.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else translationTable'
                                                              |> Seq.iter (fun translationTableItem -> TranslationTableHandler.matchAndAddTranslationTables dbContext translationTableItem item)
                               )
                        else translationTable
                             |> Seq.iter (fun translationTableItem -> TranslationTableHandler.matchAndAddTranslationTables dbContext translationTableItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:TranslationTable) =
                TranslationTableHandler.matchAndAddTranslationTables dbContext item |> ignore
                dbContext.SaveChanges()

        type MeasureHandler =
            ///Initializes a measure-object with at least all necessary parameters.
            static member init
                (
                    details  : seq<MeasureParam>,
                    ?id      : Guid,
                    ?name    : string 
                ) =
                let id'   = defaultArg id (System.Guid.NewGuid())
                let name' = defaultArg name Unchecked.defaultof<string>
                    
                new Measure(
                            Nullable(id'), 
                            name', 
                            details |> List, Nullable(DateTime.Now)
                           )

            static member addName
                (name:string) (measure:Measure) =
                measure.Name <- name
                measure

            static member tryFindByID
                (context:MzIdentML) (measureID:string) =
                tryFind (context.Measure.Find(measureID))

            static member private matchAndAddMeasures (dbContext:MzIdentML) (item1:Measure) (item2:Measure) =
                if item1.Details=item2.Details
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Measure) =
                query {
                       for i in dbContext.Measure.Local do
                           if i.Name=item.Name
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (measure, _) -> measure)
                |> (fun measure -> 
                    if Seq.length measure < 1 
                        then 
                            query {
                                   for i in dbContext.Measure do
                                       if i.Name=item.Name
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (measure, _) -> measure)
                            |> (fun measure' -> if (measure'.Count()) < 1
                                                         then let tmp = dbContext.Measure.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else measure'
                                                              |> Seq.iter (fun measureItem -> MeasureHandler.matchAndAddMeasures dbContext measureItem item)
                               )
                        else measure
                             |> Seq.iter (fun measureItem -> MeasureHandler.matchAndAddMeasures dbContext measureItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Measure) =
                MeasureHandler.matchAndAddMeasures dbContext item |> ignore
                dbContext.SaveChanges()

        type ResidueHandler =
            ///Initializes a residue-object with at least all necessary parameters.
            static member init
                (
                    code    : string,
                    mass    : float,
                    ?id     : Guid
                ) =
                let id' = defaultArg id (System.Guid.NewGuid())
                new Residue(
                            Nullable(id'), 
                            code, 
                            Nullable(mass), 
                            Nullable(DateTime.Now)
                           )

            static member findResidueByID
                (context:MzIdentML) (residueID:string) =
                tryFind (context.Residue.Find(residueID))

            static member private matchAndAddResidues (dbContext:MzIdentML) (item1:Residue) (item2:Residue) =
                if item1.Mass=item2.Mass
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Residue) =
                query {
                       for i in dbContext.Residue.Local do
                           if i.Code=item.Code
                              then select i
                      }
                |> (fun residue -> 
                    if Seq.length residue < 1 
                        then 
                            query {
                                   for i in dbContext.Residue do
                                       if i.Code=item.Code
                                          then select i
                                  }
                            |> (fun residue' -> if (residue'.Count()) < 1
                                                         then let tmp = dbContext.Residue.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else residue'
                                                              |> Seq.iter (fun residueItem -> ResidueHandler.matchAndAddResidues dbContext residueItem item)
                               )
                        else residue
                             |> Seq.iter (fun residueItem -> ResidueHandler.matchAndAddResidues dbContext residueItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Residue) =
                ResidueHandler.matchAndAddResidues dbContext item |> ignore
                dbContext.SaveChanges()

        type AmbiguousResidueHandler =
            static member init
                (
                    code    : string,
                    details : seq<AmbiguousResidueParam>,
                    ?id     : Guid
                ) =
                let id' = defaultArg id (System.Guid.NewGuid())
                    
                new AmbiguousResidue(
                                     Nullable(id'), 
                                     code, 
                                     details |> List, 
                                     Nullable(DateTime.Now)
                                    )

            static member tryFindByID
                (context:MzIdentML) (ambiguousResidueID:string) =
                tryFind (context.AmbiguousResidue.Find(ambiguousResidueID))

            static member private matchAndAddAmbiguousResidues (dbContext:MzIdentML) (item1:AmbiguousResidue) (item2:AmbiguousResidue) =
                if item1.Details=item2.Details
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:AmbiguousResidue) =
                query {
                       for i in dbContext.AmbiguousResidue.Local do
                           if i.Code=item.Code
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (ambiguousResidue, _) -> ambiguousResidue)
                |> (fun ambiguousResidue -> 
                    if Seq.length ambiguousResidue < 1 
                        then 
                            query {
                                   for i in dbContext.AmbiguousResidue do
                                       if i.Code=item.Code
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (ambiguousResidue, _) -> ambiguousResidue)
                            |> (fun ambiguousResidue' -> if (ambiguousResidue'.Count()) < 1
                                                         then let tmp = dbContext.AmbiguousResidue.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else ambiguousResidue'
                                                              |> Seq.iter (fun ambiguousResidueItem -> AmbiguousResidueHandler.matchAndAddAmbiguousResidues dbContext ambiguousResidueItem item)
                               )
                        else ambiguousResidue
                             |> Seq.iter (fun ambiguousResidueItem -> AmbiguousResidueHandler.matchAndAddAmbiguousResidues dbContext ambiguousResidueItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:AmbiguousResidue) =
                AmbiguousResidueHandler.matchAndAddAmbiguousResidues dbContext item |> ignore
                dbContext.SaveChanges()

        type MassTableHandler =
            ///Initializes a masstable-object with at least all necessary parameters.
            static member init
                (
                    msLevel           : string,
                    ?id               : Guid,
                    ?name             : string,
                    ?residue          : seq<Residue>,
                    ?ambiguousResidue : seq<AmbiguousResidue>,
                    ?details          : seq<MassTableParam>
                ) =
                let id'               = defaultArg id (System.Guid.NewGuid())
                let name'             = defaultArg name Unchecked.defaultof<string>
                let residue'          = convertOptionToList residue
                let ambiguousResidue' = convertOptionToList ambiguousResidue
                let details'          = convertOptionToList details
                    
                new MassTable(
                              Nullable(id'), 
                              name', 
                              msLevel, 
                              residue', 
                              ambiguousResidue', 
                              details', 
                              Nullable(DateTime.Now)
                             )

            static member addName
                (name:string) (massTable:MassTable) =
                massTable.Name <- name
                massTable

            static member addResidue
                (residue:Residue) (massTable:MassTable) =
                let result = massTable.Residues <- addToList massTable.Residues residue
                massTable

            static member addResidues
                (residues:seq<Residue>) (massTable:MassTable) =
                let result = massTable.Residues <- addCollectionToList massTable.Residues residues
                massTable

            static member addAmbiguousResidue
                (ambiguousResidues:AmbiguousResidue) (massTable:MassTable) =
                let result = massTable.AmbiguousResidues <- addToList massTable.AmbiguousResidues ambiguousResidues
                massTable

            static member addAmbiguousResidues
                (ambiguousResidues:seq<AmbiguousResidue>) (massTable:MassTable) =
                let result = massTable.AmbiguousResidues <- addCollectionToList massTable.AmbiguousResidues ambiguousResidues
                massTable

            static member addDetail
                (detail:MassTableParam) (massTable:MassTable) =
                let result = massTable.Details <- addToList massTable.Details detail
                massTable

            static member addDetails
                (details:seq<MassTableParam>) (massTable:MassTable) =
                let result = massTable.Details <- addCollectionToList massTable.Details details
                massTable

            static member tryFindByID
                (context:MzIdentML) (massTableID:string) =
                tryFind (context.MassTable.Find(massTableID))

            static member private matchAndAddMassTables (dbContext:MzIdentML) (item1:MassTable) (item2:MassTable) =
                if item1.MSLevel=item2.MSLevel && item1.Residues=item2.Residues && 
                   item1.AmbiguousResidues=item2.AmbiguousResidues && item1.Details=item2.Details
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:MassTable) =
                query {
                       for i in dbContext.MassTable.Local do
                           if i.Name=item.Name
                              then select (i, i.Residues, i.AmbiguousResidues, i.Details)
                      }
                |> Seq.map (fun (massTable, _, _, _) -> massTable)
                |> (fun massTable -> 
                    if Seq.length massTable < 1 
                        then 
                            query {
                                   for i in dbContext.MassTable do
                                       if i.Name=item.Name
                                          then select (i, i.Residues, i.AmbiguousResidues, i.Details)
                                  }
                            |> Seq.map (fun (massTable, _, _, _) -> massTable)
                            |> (fun massTable' -> if (massTable'.Count()) < 1
                                                         then let tmp = dbContext.MassTable.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else massTable'
                                                              |> Seq.iter (fun massTableItem -> MassTableHandler.matchAndAddMassTables dbContext massTableItem item)
                               )
                        else massTable
                             |> Seq.iter (fun massTableItem -> MassTableHandler.matchAndAddMassTables dbContext massTableItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:MassTable) =
                MassTableHandler.matchAndAddMassTables dbContext item |> ignore
                dbContext.SaveChanges()

        type ValueHandler =
            ///Initializes a value-object with at least all necessary parameters.
            static member init
                (
                    value   : float,
                    ?id     : Guid
                ) =
                let id' = defaultArg id (System.Guid.NewGuid())
                    
                new Value(
                          Nullable(id'), 
                          Nullable(value), 
                          Nullable(DateTime.Now)
                         )

            static member findValueByID
                (context:MzIdentML) (valueID:string) =
                tryFind (context.Value.Find(valueID))

            static member private matchAndAddValues (dbContext:MzIdentML) (item1:Value) (item2:Value) =
                if item1.ID = item2.ID
                   then item2.ID <- Nullable(System.Guid.NewGuid())
                        dbContext.Add item2 |> ignore
                   else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Value) =
                query {
                       for i in dbContext.Value.Local do
                           if i.Value=item.Value
                              then select i
                      }
                |> (fun value -> 
                    if Seq.length value < 1 
                        then 
                            query {
                                   for i in dbContext.Value do
                                       if i.Value=item.Value
                                          then select i
                                  }
                            |> (fun value' -> if (value'.Count()) < 1
                                                         then let tmp = dbContext.Value.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else value'
                                                              |> Seq.iter (fun valueItem -> ValueHandler.matchAndAddValues dbContext valueItem item)
                               )
                        else value
                             |> Seq.iter (fun valueItem -> ValueHandler.matchAndAddValues dbContext valueItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Value) =
                ValueHandler.matchAndAddValues dbContext item |> ignore
                dbContext.SaveChanges()

        type FragmentArrayHandler =
            ///Initializes a fragmentarray-object with at least all necessary parameters.
            static member init
                (
                    measure : Measure,
                    values  : seq<Value>,
                    ?id     : Guid
                ) =
                let id' = defaultArg id (System.Guid.NewGuid())
                    
                new FragmentArray(
                                  Nullable(id'), 
                                  measure, 
                                  values |> List, 
                                  Nullable(DateTime.Now)
                                 )

            static member tryFindByID
                (context:MzIdentML) (fragmentArrayID:string) =
                tryFind (context.FragmentArray.Find(fragmentArrayID))

            static member private matchAndAddFragmentArrays (dbContext:MzIdentML) (item1:FragmentArray) (item2:FragmentArray) =
                if item1.Values=item2.Values
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:FragmentArray) =
                query {
                       for i in dbContext.FragmentArray.Local do
                           if i.Measure=item.Measure
                              then select (i, i.Values)
                      }
                |> Seq.map (fun (fragmentArray, _) -> fragmentArray)
                |> (fun fragmentArray -> 
                    if Seq.length fragmentArray < 1 
                        then 
                            query {
                                   for i in dbContext.FragmentArray do
                                       if i.Measure=item.Measure
                                          then select (i, i.Values)
                                  }
                            |> Seq.map (fun (fragmentArray, _) -> fragmentArray)
                            |> (fun fragmentArray' -> if (fragmentArray'.Count()) < 1
                                                         then let tmp = dbContext.FragmentArray.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else fragmentArray'
                                                              |> Seq.iter (fun fragmentArrayItem -> FragmentArrayHandler.matchAndAddFragmentArrays dbContext fragmentArrayItem item)
                               )
                        else fragmentArray
                             |> Seq.iter (fun fragmentArrayItem -> FragmentArrayHandler.matchAndAddFragmentArrays dbContext fragmentArrayItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:FragmentArray) =
                FragmentArrayHandler.matchAndAddFragmentArrays dbContext item |> ignore
                dbContext.SaveChanges()

        type IndexHandler =
            ///Initializes a index-object with at least all necessary parameters.
            static member init
                (
                    index : int,
                    ?id   : Guid
                ) =
                let id' = defaultArg id (System.Guid.NewGuid())

                new Index(
                          Nullable(id'), 
                          Nullable(index), 
                          Nullable(DateTime.Now)
                         )

            static member tryFindByID
                (context:MzIdentML) (indexID:string) =
                tryFind (context.Index.Find(indexID))

            static member private matchAndAddIndeces (dbContext:MzIdentML) (item1:Index) (item2:Index) =
                if item1.ID = item2.ID
                   then item2.ID <- Nullable(System.Guid.NewGuid())
                        dbContext.Add item2 |> ignore
                   else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Index) =
                query {
                       for i in dbContext.Index.Local do
                           if i.Index=item.Index
                              then select i
                      }
                |> (fun fragmentArray -> 
                    if Seq.length fragmentArray < 1 
                        then 
                            query {
                                   for i in dbContext.Index do
                                       if i.Index=item.Index
                                          then select i
                                  }
                            |> (fun fragmentArray' -> if (fragmentArray'.Count()) < 1
                                                         then let tmp = dbContext.Index.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else fragmentArray'
                                                              |> Seq.iter (fun fragmentArrayItem -> IndexHandler.matchAndAddIndeces dbContext fragmentArrayItem item)
                               )
                        else fragmentArray
                             |> Seq.iter (fun fragmentArrayItem -> IndexHandler.matchAndAddIndeces dbContext fragmentArrayItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Index) =
                IndexHandler.matchAndAddIndeces dbContext item |> ignore
                dbContext.SaveChanges()

        type IonTypeHandler =
            ///Initializes a iontype-object with at least all necessary parameters.
            static member init
                (
                    details        : seq<IonTypeParam>,
                    ?id            : Guid,
                    ?index         : seq<Index>,
                    ?fragmentArray : seq<FragmentArray>
                ) =
                let id'            = defaultArg id (System.Guid.NewGuid())
                let index'         = convertOptionToList index
                let fragmentArray' = convertOptionToList fragmentArray
                    
                new IonType(
                            Nullable(id'), 
                            index', 
                            fragmentArray', 
                            details |> List, Nullable(DateTime.Now)
                           )

            static member addIndex
                (index:Index) (ionType:IonType) =
                let result = ionType.Index <- addToList ionType.Index index
                ionType

            static member addIndexes
                (index:seq<Index>) (ionType:IonType) =
                let result = ionType.Index <- addCollectionToList ionType.Index index
                ionType

            static member addFragmentArray
                (fragmentArray:FragmentArray) (ionType:IonType) =
                let result = ionType.FragmentArrays <- addToList ionType.FragmentArrays fragmentArray
                ionType

            static member addFragmentArrays
                (fragmentArrays:seq<FragmentArray>) (ionType:IonType) =
                let result = ionType.FragmentArrays <- addCollectionToList ionType.FragmentArrays fragmentArrays
                ionType

            static member tryFindByID
                (context:MzIdentML) (ionTypeID:string) =
                tryFind (context.IonType.Find(ionTypeID))

            static member private matchAndAddIonTypes (dbContext:MzIdentML) (item1:IonType) (item2:IonType) =
                if item1.FragmentArrays=item2.FragmentArrays && item1.Details=item2.Details
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:IonType) =
                query {
                       for i in dbContext.IonType.Local do
                           if i.Index=item.Index
                              then select (i, i.FragmentArrays, i.Details)
                      }
                |> Seq.map (fun (ionType, _, _) -> ionType)
                |> (fun fragmentArray -> 
                    if Seq.length fragmentArray < 1 
                        then 
                            query {
                                   for i in dbContext.IonType do
                                       if i.Index=item.Index
                                          then select (i, i.FragmentArrays, i.Details)
                                  }
                            |> Seq.map (fun (ionType, _, _) -> ionType)
                            |> (fun fragmentArray' -> if (fragmentArray'.Count()) < 1
                                                         then let tmp = dbContext.IonType.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else fragmentArray'
                                                              |> Seq.iter (fun fragmentArrayItem -> IonTypeHandler.matchAndAddIonTypes dbContext fragmentArrayItem item)
                               )
                        else fragmentArray
                             |> Seq.iter (fun fragmentArrayItem -> IonTypeHandler.matchAndAddIonTypes dbContext fragmentArrayItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:IonType) =
                IonTypeHandler.matchAndAddIonTypes dbContext item |> ignore
                dbContext.SaveChanges()

        type SpectraDataHandler =
            ///Initializes a spectradata-object with at least all necessary parameters.
            static member init
                ( 
                    location                     : string,
                    fileFormat                   : CVParam,
                    spectrumIDFormat             : CVParam,
                    ?id                          : Guid,
                    ?name                        : string,
                    ?externalFormatDocumentation : string
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>

                new SpectraData(
                                Nullable(id'), 
                                name', 
                                location, 
                                externalFormatDocumentation', 
                                fileFormat, spectrumIDFormat, 
                                Nullable(DateTime.Now)
                               )

            static member addName
                (name:string) (spectraData:SpectraData) =
                spectraData.Name <- name
                spectraData

            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (spectraData:SpectraData) =
                spectraData.ExternalFormatDocumentation <- externalFormatDocumentation
                spectraData

            static member tryFindByID
                (context:MzIdentML) (spectraDataID:string) =
                tryFind (context.SpectraData.Find(spectraDataID))

            static member private matchAndAddSpectraDatas (dbContext:MzIdentML) (item1:SpectraData) (item2:SpectraData) =
                if item1.Name=item2.Name && item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation &&
                   item1.Location=item2.Location && item1.FileFormat=item2.FileFormat
                
                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SpectraData) =
                query {
                       for i in dbContext.SpectraData.Local do
                           if i.SpectrumIDFormat=item.SpectrumIDFormat
                              then select (i, i.FileFormat, i.SpectrumIDFormat)
                      }
                |> Seq.map (fun (spectraData, _, _) -> spectraData)
                |> (fun spectraData -> 
                    if Seq.length spectraData < 1 
                        then 
                            query {
                                   for i in dbContext.SpectraData do
                                       if i.SpectrumIDFormat=item.SpectrumIDFormat
                                          then select (i, i.FileFormat, i.SpectrumIDFormat)
                                  }
                            |> Seq.map (fun (spectraData, _, _) -> spectraData)
                            |> (fun spectraData' -> if (spectraData'.Count()) < 1
                                                         then let tmp = dbContext.SpectraData.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else spectraData'
                                                              |> Seq.iter (fun spectraDataItem -> SpectraDataHandler.matchAndAddSpectraDatas dbContext spectraDataItem item)
                               )
                        else spectraData
                             |> Seq.iter (fun spectraDataItem -> SpectraDataHandler.matchAndAddSpectraDatas dbContext spectraDataItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SpectraData) =
                SpectraDataHandler.matchAndAddSpectraDatas dbContext item |> ignore
                dbContext.SaveChanges()

        type SpecificityRulesHandler =
            ///Initializes a specificityrules-object with at least all necessary parameters.
            static member init
                ( 
                    details    : seq<SpecificityRuleParam>,
                    ?id        : Guid
                ) =
                let id' = defaultArg id (System.Guid.NewGuid())
                    
                new SpecificityRule(
                                    Nullable(id'), 
                                    details |> List, 
                                    Nullable(DateTime.Now)
                                   )

            static member tryFindByID
                (context:MzIdentML) (specificityRuleID:string) =
                tryFind (context.SpecificityRule.Find(specificityRuleID))

            static member private matchAndAddSpecificityRules (dbContext:MzIdentML) (item1:SpecificityRule) (item2:SpecificityRule) =
                if item1.ID = item2.ID
                   then item2.ID <- Nullable(System.Guid.NewGuid())
                        dbContext.Add item2 |> ignore
                   else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SpecificityRule) =
                query {
                       for i in dbContext.SpecificityRule.Local do
                           if i.Details=item.Details
                              then select (i, i.Details)
                      }
                |> Seq.map (fun (specificityRule, _) -> specificityRule)
                |> (fun specificityRule -> 
                    if Seq.length specificityRule < 1 
                        then 
                            query {
                                   for i in dbContext.SpecificityRule do
                                       if i.Details=item.Details
                                          then select (i, i.Details)
                                  }
                            |> Seq.map (fun (specificityRule, _) -> specificityRule)
                            |> (fun specificityRule' -> if (specificityRule'.Count()) < 1
                                                         then let tmp = dbContext.SpecificityRule.Find(item.ID)
                                                              if tmp.ID = item.ID 
                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                      dbContext.Add item |> ignore
                                                                 else dbContext.Add item |> ignore
                                                         else specificityRule'
                                                              |> Seq.iter (fun specificityRuleItem -> SpecificityRulesHandler.matchAndAddSpecificityRules dbContext specificityRuleItem item)
                               )
                        else specificityRule
                             |> Seq.iter (fun specificityRuleItem -> SpecificityRulesHandler.matchAndAddSpecificityRules dbContext specificityRuleItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SpecificityRule) =
                SpecificityRulesHandler.matchAndAddSpecificityRules dbContext item |> ignore
                dbContext.SaveChanges()

        type SearchModificationHandler =
            ///Initializes a searchmodification-object with at least all necessary parameters.
            static member init
                ( 
                    fixedMod          : bool,
                    massDelta         : float,
                    residues          : string,
                    details           : seq<SearchModificationParam>,
                    ?id               : Guid,
                    ?specificityRules : seq<SpecificityRule>
                ) =
                let id'               = defaultArg id (System.Guid.NewGuid())
                let specificityRules' = convertOptionToList specificityRules
                    
                new SearchModification(
                                       Nullable(id'), 
                                       Nullable(fixedMod), 
                                       Nullable(massDelta), 
                                       residues, 
                                       specificityRules', 
                                       details |> List, Nullable(DateTime.Now)
                                      )

            static member addSpecificityRule
                (specificityRule:SpecificityRule) (searchModification:SearchModification) =
                let result = searchModification.SpecificityRules <- addToList searchModification.SpecificityRules specificityRule
                searchModification

            static member addSpecificityRules
                (specificityRules:seq<SpecificityRule>) (searchModification:SearchModification) =
                let result = searchModification.SpecificityRules <- addCollectionToList searchModification.SpecificityRules specificityRules
                searchModification

            static member tryFindByID
                (context:MzIdentML) (searchModificationID:string) =
                tryFind (context.SearchModification.Find(searchModificationID))

            static member private matchAndAddSearchModifications (dbContext:MzIdentML) (item1:SearchModification) (item2:SearchModification) =
                if item1.FixedMod=item2.FixedMod && item1.Residues=item2.Residues && item1.Residues=item2.Residues &&
                   item1.SpecificityRules=item2.SpecificityRules && item1.Details=item2.Details

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SearchModification) =
                query {
                       for i in dbContext.SearchModification.Local do
                           if i.MassDelta=item.MassDelta
                              then select (i, i.SpecificityRules, i.Details)
                      }
                |> Seq.map (fun (searchModification, _, _) -> searchModification)
                |> (fun searchModification -> 
                    if Seq.length searchModification < 1 
                        then 
                            query {
                                   for i in dbContext.SearchModification do
                                       if i.MassDelta=item.MassDelta
                                          then select (i, i.SpecificityRules, i.Details)
                                  }
                            |> Seq.map (fun (searchModification, _, _) -> searchModification)
                            |> (fun searchModification' -> if (searchModification'.Count()) < 1
                                                            then let tmp = dbContext.SearchModification.Find(item.ID)
                                                                 if tmp.ID = item.ID 
                                                                    then item.ID <- Nullable(System.Guid.NewGuid())
                                                                         dbContext.Add item |> ignore
                                                                    else dbContext.Add item |> ignore
                                                            else searchModification'
                                                                 |> Seq.iter (fun searchModificationItem -> SearchModificationHandler.matchAndAddSearchModifications dbContext searchModificationItem item)
                               )
                        else searchModification
                             |> Seq.iter (fun searchModificationItem -> SearchModificationHandler.matchAndAddSearchModifications dbContext searchModificationItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SearchModification) =
                SearchModificationHandler.matchAndAddSearchModifications dbContext item |> ignore
                dbContext.SaveChanges()

        type EnzymeHandler =
            ///Initializes a enzyme-object with at least all necessary parameters.
            static member init
                (
                    ?id              : Guid,
                    ?name            : string,
                    ?cTermGain       : string,
                    ?nTermGain       : string,
                    ?minDistance     : int,
                    ?missedCleavages : int,
                    ?semiSpecific    : bool,
                    ?siteRegexc      : string,
                    ?enzymeName      : seq<EnzymeNameParam>
                ) =
                let id'              = defaultArg id (System.Guid.NewGuid())
                let name'            = defaultArg name Unchecked.defaultof<string>
                let cTermGain'       = defaultArg cTermGain Unchecked.defaultof<string>
                let nTermGain'       = defaultArg nTermGain Unchecked.defaultof<string>
                let minDistance'     = defaultArg minDistance Unchecked.defaultof<int>
                let missedCleavages' = defaultArg missedCleavages Unchecked.defaultof<int>
                let semiSpecific'    = defaultArg semiSpecific Unchecked.defaultof<bool>
                let siteRegexc'      = defaultArg siteRegexc Unchecked.defaultof<string>
                let enzymeName'      = convertOptionToList enzymeName
                    
                new Enzyme(
                           Nullable(id'), 
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

            static member addName
                (name:string) (enzyme:Enzyme) =
                enzyme.Name <- name
                enzyme

            static member addCTermGain
                (cTermGain:string) (enzyme:Enzyme) =
                enzyme.CTermGain <- cTermGain
                enzyme

            static member addNTermGain
                (nTermGain:string) (enzyme:Enzyme) =
                enzyme.NTermGain <- nTermGain
                enzyme

            static member addMinDistance
                (minDistance:int) (enzyme:Enzyme) =
                enzyme.MinDistance <- Nullable(minDistance)
                enzyme

            static member addMissedCleavages
                (missedCleavages:int) (enzyme:Enzyme) =
                enzyme.MissedCleavages <-Nullable( missedCleavages)
                enzyme

            static member addSemiSpecific
                (semiSpecific:bool) (enzyme:Enzyme) =
                enzyme.SemiSpecific <- Nullable(semiSpecific)
                enzyme

            static member addSiteRegexc
                (siteRegexc:string) (enzyme:Enzyme) =
                enzyme.SiteRegexc <- siteRegexc
                enzyme

            static member addEnzymeName
                (enzymeName:EnzymeNameParam) (enzyme:Enzyme) =
                let result = enzyme.EnzymeName <- addToList enzyme.EnzymeName enzymeName
                enzyme

            static member addEnzymeNames
                (enzymeNames:seq<EnzymeNameParam>) (enzyme:Enzyme) =
                let result = enzyme.EnzymeName <- addCollectionToList enzyme.EnzymeName enzymeNames
                enzyme

            static member tryFindByID
                (context:MzIdentML) (enzymeID:string) =
                tryFind (context.Enzyme.Find(enzymeID))

            static member private matchAndAddEnzymes (dbContext:MzIdentML) (item1:Enzyme) (item2:Enzyme) =
                if item1.CTermGain=item2.CTermGain && item1.NTermGain=item2.NTermGain && item1.MinDistance=item2.MinDistance &&
                   item1.MissedCleavages=item2.MissedCleavages && item1.SemiSpecific=item2.SemiSpecific &&
                   item1.SiteRegexc=item2.SiteRegexc && item1.EnzymeName=item2.EnzymeName

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Enzyme) =
                query {
                       for i in dbContext.Enzyme.Local do
                           if i.Name=item.Name
                              then select (i, i.EnzymeName)
                      }
                |> Seq.map (fun (enzyme, _) -> enzyme)
                |> (fun enzyme -> 
                    if Seq.length enzyme < 1 
                        then 
                            query {
                                   for i in dbContext.Enzyme do
                                       if i.Name=item.Name
                                          then select (i, i.EnzymeName)
                                  }
                            |> Seq.map (fun (enzyme, _) -> enzyme)
                            |> (fun enzyme' -> if (enzyme'.Count()) < 1
                                                            then let tmp = dbContext.Enzyme.Find(item.ID)
                                                                 if tmp.ID = item.ID 
                                                                    then item.ID <- Nullable(System.Guid.NewGuid())
                                                                         dbContext.Add item |> ignore
                                                                    else dbContext.Add item |> ignore
                                                            else enzyme'
                                                                 |> Seq.iter (fun enzymeItem -> EnzymeHandler.matchAndAddEnzymes dbContext enzymeItem item)
                               )
                        else enzyme
                             |> Seq.iter (fun enzymeItem -> EnzymeHandler.matchAndAddEnzymes dbContext enzymeItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Enzyme) =
                EnzymeHandler.matchAndAddEnzymes dbContext item |> ignore
                dbContext.SaveChanges()

        type FilterHandler =
            ///Initializes a filter-object with at least all necessary parameters.
            static member init
                (
                    filterType : CVParam,
                    ?id        : Guid,
                    ?includes  : seq<IncludeParam>,
                    ?excludes  : seq<ExcludeParam>
                ) =
                let id'         = defaultArg id (System.Guid.NewGuid())
                let includes'   = convertOptionToList includes
                let excludes'   = convertOptionToList excludes
                    
                new Filter(
                           Nullable(id'), 
                           filterType, 
                           includes', 
                           excludes', 
                           Nullable(DateTime.Now)
                          )

            static member addInclude
                (include':IncludeParam) (filter:Filter) =
                let result = filter.Includes <- addToList filter.Includes include'
                filter

            static member addIncludes
                (includes:seq<IncludeParam>) (filter:Filter) =
                let result = filter.Includes <- addCollectionToList filter.Includes includes
                filter

            static member addExclude
                (exclude':ExcludeParam) (filter:Filter) =
                let result = filter.Excludes <- addToList filter.Excludes exclude'
                filter

            static member addExcludes
                (excludes:seq<ExcludeParam>) (filter:Filter) =
                let result = filter.Excludes <- addCollectionToList filter.Excludes excludes
                filter

            static member tryFindByID
                (context:MzIdentML) (filterID:string) =
                tryFind (context.Filter.Find(filterID))

            static member private matchAndAddFilters (dbContext:MzIdentML) (item1:Filter) (item2:Filter) =
                if item1.Includes=item2.Includes && item1.Excludes=item2.Excludes

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Filter) =
                query {
                       for i in dbContext.Filter.Local do
                           if i.FilterType=item.FilterType
                              then select (i, i.FilterType, i.Includes, i.Excludes)
                      }
                |> Seq.map (fun (filter, _, _, _) -> filter)
                |> (fun filter -> 
                    if Seq.length filter < 1 
                        then 
                            query {
                                   for i in dbContext.Filter do
                                       if i.FilterType=item.FilterType
                                          then select (i, i.FilterType, i.Includes, i.Excludes)
                                  }
                            |> Seq.map (fun (filter, _, _, _) -> filter)
                            |> (fun filter' -> if (filter'.Count()) < 1
                                                            then let tmp = dbContext.Filter.Find(item.ID)
                                                                 if tmp.ID = item.ID 
                                                                    then item.ID <- Nullable(System.Guid.NewGuid())
                                                                         dbContext.Add item |> ignore
                                                                    else dbContext.Add item |> ignore
                                                            else filter'
                                                                 |> Seq.iter (fun filterItem -> FilterHandler.matchAndAddFilters dbContext filterItem item)
                               )
                        else filter
                             |> Seq.iter (fun filterItem -> FilterHandler.matchAndAddFilters dbContext filterItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Filter) =
                FilterHandler.matchAndAddFilters dbContext item |> ignore
                dbContext.SaveChanges()

        type FrameHandler =
            ///Initializes a frame-object with at least all necessary parameters.
            static member init
                ( 
                    frame : int,
                    ?id   : Guid
                ) =
                let id'   = defaultArg id (System.Guid.NewGuid())
                    
                new Frame(
                          Nullable(id'), 
                          Nullable(frame), 
                          Nullable(DateTime.Now)
                         )

            static member findFrameByID
                (context:MzIdentML) (frameID:string) =
                tryFind (context.Frame.Find(frameID))

            static member private matchAndAddFrames (dbContext:MzIdentML) (item1:Frame) (item2:Frame) =
                if item1.ID = item2.ID
                   then item2.ID <- Nullable(System.Guid.NewGuid())
                        dbContext.Add item2 |> ignore
                   else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Frame) =
                query {
                       for i in dbContext.Frame.Local do
                           if i.Frame=item.Frame
                              then select i
                      }
                |> (fun frame -> 
                    if Seq.length frame < 1 
                        then 
                            query {
                                   for i in dbContext.Frame do
                                       if i.Frame=item.Frame
                                          then select i
                                  }
                            |> (fun frame' -> if (frame'.Count()) < 1
                                                            then let tmp = dbContext.Frame.Find(item.ID)
                                                                 if tmp.ID = item.ID 
                                                                    then item.ID <- Nullable(System.Guid.NewGuid())
                                                                         dbContext.Add item |> ignore
                                                                    else dbContext.Add item |> ignore
                                                            else frame'
                                                                 |> Seq.iter (fun frameItem -> FrameHandler.matchAndAddFrames dbContext frameItem item)
                               )
                        else frame
                             |> Seq.iter (fun frameItem -> FrameHandler.matchAndAddFrames dbContext frameItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Frame) =
                FrameHandler.matchAndAddFrames dbContext item |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationProtocolHandler =
            ///Initializes a spectrumidentificationprotocol-object with at least all necessary parameters.
            static member init
                (
                    analysisSoftware        : AnalysisSoftware,
                    searchType              : CVParam ,
                    threshold               : seq<ThresholdParam>,
                    ?id                     : Guid,
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
                    ?mzIdentML              : MzIdentMLDocument
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid())
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
                let mzIdentML'              = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                new SpectrumIdentificationProtocol(
                                                   Nullable(id'), 
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
                                                   mzIdentML', 
                                                   Nullable(DateTime.Now)
                                                  )

            static member addName
                (name:string) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                spectrumIdentificationProtocol.Name <- name
                spectrumIdentificationProtocol

            static member addEnzymeAdditionalSearchParam
                (additionalSearchParam:AdditionalSearchParam) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.AdditionalSearchParams <- addToList spectrumIdentificationProtocol.AdditionalSearchParams additionalSearchParam
                spectrumIdentificationProtocol

            static member addEnzymeAdditionalSearchParams
                (additionalSearchParams:seq<AdditionalSearchParam>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.AdditionalSearchParams <- addCollectionToList spectrumIdentificationProtocol.AdditionalSearchParams additionalSearchParams
                spectrumIdentificationProtocol

            static member addModificationParam
                (modificationParam:SearchModification) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.ModificationParams <- addToList spectrumIdentificationProtocol.ModificationParams modificationParam
                spectrumIdentificationProtocol

            static member addModificationParams
                (modificationParams:seq<SearchModification>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.ModificationParams <- addCollectionToList spectrumIdentificationProtocol.ModificationParams modificationParams
                spectrumIdentificationProtocol

            static member addEnzyme
                (enzyme:Enzyme) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.Enzymes <- addToList spectrumIdentificationProtocol.Enzymes enzyme
                spectrumIdentificationProtocol

            static member addEnzymes
                (enzymes:seq<Enzyme>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.Enzymes <- addCollectionToList spectrumIdentificationProtocol.Enzymes enzymes
                spectrumIdentificationProtocol

            static member addIndependent_Enzymes
                (independent_Enzymes:bool) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                spectrumIdentificationProtocol.Independent_Enzymes <- Nullable(independent_Enzymes)
                spectrumIdentificationProtocol

            static member addMassTable
                (massTable:MassTable) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.MassTables <- addToList spectrumIdentificationProtocol.MassTables massTable
                spectrumIdentificationProtocol

            static member addMassTables
                (massTables:seq<MassTable>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.MassTables <- addCollectionToList spectrumIdentificationProtocol.MassTables massTables
                spectrumIdentificationProtocol

            static member addFragmentTolerance
                (fragmentTolerance:FragmentToleranceParam) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.FragmentTolerance <- addToList spectrumIdentificationProtocol.FragmentTolerance fragmentTolerance
                spectrumIdentificationProtocol

            static member addFragmentTolerances
                (fragmentTolerances:seq<FragmentToleranceParam>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.FragmentTolerance <- addCollectionToList spectrumIdentificationProtocol.FragmentTolerance fragmentTolerances
                spectrumIdentificationProtocol

            static member addParentTolerance
                (parentTolerance:ParentToleranceParam) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.ParentTolerance <- addToList spectrumIdentificationProtocol.ParentTolerance parentTolerance
                spectrumIdentificationProtocol

            static member addParentTolerances
                (parentTolerances:seq<ParentToleranceParam>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.ParentTolerance <- addCollectionToList spectrumIdentificationProtocol.ParentTolerance parentTolerances
                spectrumIdentificationProtocol

            static member addDatabaseFilter
                (databaseFilter:Filter) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.DatabaseFilters <- addToList spectrumIdentificationProtocol.DatabaseFilters databaseFilter
                spectrumIdentificationProtocol

            static member addDatabaseFilters
                (databaseFilters:seq<Filter>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.DatabaseFilters <- addCollectionToList spectrumIdentificationProtocol.DatabaseFilters databaseFilters
                spectrumIdentificationProtocol

            static member addFrame
                (frame:Frame) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.Frames <- addToList spectrumIdentificationProtocol.Frames frame
                spectrumIdentificationProtocol

            static member addFrames
                (frames:seq<Frame>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.Frames <- addCollectionToList spectrumIdentificationProtocol.Frames frames
                spectrumIdentificationProtocol

            static member addTranslationTable
                (translationTable:TranslationTable) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.TranslationTables <- addToList spectrumIdentificationProtocol.TranslationTables translationTable
                spectrumIdentificationProtocol

            static member addTranslationTables
                (translationTables:seq<TranslationTable>) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.TranslationTables <- addCollectionToList spectrumIdentificationProtocol.TranslationTables translationTables
                spectrumIdentificationProtocol

            static member addMzIdentML
                (mzIdentMLDocument:MzIdentMLDocument) (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) =
                let result = spectrumIdentificationProtocol.MzIdentMLDocument <- mzIdentMLDocument
                spectrumIdentificationProtocol

            static member tryFindByID
                (context:MzIdentML) (spectrumIdentificationProtocolID:string) =
                tryFind (context.SpectrumIdentificationProtocol.Find(spectrumIdentificationProtocolID))

            static member private matchAndAddSpectrumIdentificationProtocols (dbContext:MzIdentML) (item1:SpectrumIdentificationProtocol) (item2:SpectrumIdentificationProtocol) =
                if item1.AnalysisSoftware=item2.AnalysisSoftware && item1.Threshold=item2.Threshold && 
                   item1.AdditionalSearchParams=item2.AdditionalSearchParams && item1.ModificationParams=item2.ModificationParams && 
                   item1.Enzymes=item2.Enzymes && item1.MassTables=item2.MassTables && item1.FragmentTolerance=item2.FragmentTolerance &&
                   item1.ParentTolerance=item2.ParentTolerance && item1.DatabaseFilters=item2.DatabaseFilters && item1.Frames=item2.Frames &&
                   item1.TranslationTables=item2.TranslationTables && item1.MzIdentMLDocument=item2.MzIdentMLDocument

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SpectrumIdentificationProtocol) =
                query {
                       for i in dbContext.SpectrumIdentificationProtocol.Local do
                           if i.SearchType=item.SearchType
                              then select (i, i.AnalysisSoftware, i.SearchType, i.Threshold, i.AdditionalSearchParams, 
                                           i.ModificationParams, i.Enzymes, i.MassTables, i.FragmentTolerance, 
                                           i.ParentTolerance, i.DatabaseFilters, i.Frames, i.TranslationTables, 
                                           i.MzIdentMLDocument
                                          )
                      }
                |> Seq.map (fun (spectrumIdentificationProtocol, _, _, _, _, _, _, _, _, _, _, _, _, _) -> spectrumIdentificationProtocol)
                |> (fun spectrumIdentificationProtocol -> 
                    if Seq.length spectrumIdentificationProtocol < 1 
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationProtocol do
                                       if i.SearchType=item.SearchType
                                          then select (i, i.AnalysisSoftware, i.SearchType, i.Threshold, i.AdditionalSearchParams, 
                                                       i.ModificationParams, i.Enzymes, i.MassTables, i.FragmentTolerance, 
                                                       i.ParentTolerance, i.DatabaseFilters, i.Frames, i.TranslationTables, 
                                                       i.MzIdentMLDocument
                                                      )
                                  }
                            |> Seq.map (fun (spectrumIdentificationProtocol, _, _, _, _, _, _, _, _, _, _, _, _, _) -> spectrumIdentificationProtocol)
                            |> (fun spectrumIdentificationProtocol' -> if (spectrumIdentificationProtocol'.Count()) < 1
                                                                           then let tmp = dbContext.SpectrumIdentificationProtocol.Find(item.ID)
                                                                                if tmp.ID = item.ID 
                                                                                then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                     dbContext.Add item |> ignore
                                                                                else dbContext.Add item |> ignore
                                                                           else spectrumIdentificationProtocol'
                                                                                |> Seq.iter (fun spectrumIdentificationProtocolItem -> SpectrumIdentificationProtocolHandler.matchAndAddSpectrumIdentificationProtocols dbContext spectrumIdentificationProtocolItem item)
                               )
                        else spectrumIdentificationProtocol
                             |> Seq.iter (fun spectrumIdentificationProtocolItem -> SpectrumIdentificationProtocolHandler.matchAndAddSpectrumIdentificationProtocols dbContext spectrumIdentificationProtocolItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SpectrumIdentificationProtocol) =
                SpectrumIdentificationProtocolHandler.matchAndAddSpectrumIdentificationProtocols dbContext item |> ignore
                dbContext.SaveChanges()

        type SearchDatabaseHandler =
            ///Initializes a searchdatabase-object with at least all necessary parameters.
            static member init
                (
                    location                     : string,
                    fileFormat                   : CVParam,
                    databaseName                 : CVParam,
                    ?id                          : Guid,
                    ?name                        : string,                    
                    ?numDatabaseSequences        : int64,
                    ?numResidues                 : int64,
                    ?releaseDate                 : DateTime,
                    ?version                     : string,
                    ?externalFormatDocumentation : string,
                    ?details                     : seq<SearchDatabaseParam>
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let numDatabaseSequences'        = defaultArg numDatabaseSequences Unchecked.defaultof<int64>
                let numResidues'                 = defaultArg numResidues Unchecked.defaultof<int64>
                let releaseDate'                 = defaultArg releaseDate Unchecked.defaultof<DateTime>
                let version'                     = defaultArg version Unchecked.defaultof<string>
                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let details'                     = convertOptionToList details
                    
                new SearchDatabase(
                                   Nullable(id'), 
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

            static member addName
                (name:string) (searchDatabase:SearchDatabase) =
                searchDatabase.Name <- name
                searchDatabase

            static member addNumDatabaseSequences
                (numDatabaseSequences:int64) (searchDatabase:SearchDatabase) =
                searchDatabase.NumDatabaseSequences <- Nullable(numDatabaseSequences)
                searchDatabase

            static member addNumResidues
                (numResidues:int64) (searchDatabase:SearchDatabase) =
                searchDatabase.NumResidues <- Nullable(numResidues)
                searchDatabase

            static member addReleaseDate
                (releaseDate:DateTime) (searchDatabase:SearchDatabase) =
                searchDatabase.ReleaseDate <- Nullable(releaseDate)
                searchDatabase

            static member addVersion
                (version:string) (searchDatabase:SearchDatabase) =
                searchDatabase.Version <- version
                searchDatabase

            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (searchDatabase:SearchDatabase) =
                searchDatabase.Version <- externalFormatDocumentation
                searchDatabase

            static member addDetail
                (detail:SearchDatabaseParam) (searchDatabase:SearchDatabase) =
                let result = searchDatabase.Details <- addToList searchDatabase.Details detail
                searchDatabase

            static member addDetails
                (details:seq<SearchDatabaseParam>) (searchDatabase:SearchDatabase) =
                let result = searchDatabase.Details <- addCollectionToList searchDatabase.Details details
                searchDatabase

            static member tryFindByID
                (context:MzIdentML) (searchDatabaseID:string) =
                tryFind (context.SearchDatabase.Find(searchDatabaseID))

            static member private matchAndAddSearchDatabases (dbContext:MzIdentML) (item1:SearchDatabase) (item2:SearchDatabase) =
                if item1.Name=item2.Name && item1.NumDatabaseSequences=item2.NumDatabaseSequences && 
                   item1.NumResidues=item2.NumResidues && item1.ReleaseDate=item2.ReleaseDate &&
                   item1.Version=item2.Version && item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation && 
                   item1.Details=item2.Details && item1.Location=item2.Location && item1.FileFormat=item2.FileFormat

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SearchDatabase) =
                query {
                       for i in dbContext.SearchDatabase.Local do
                           if i.DatabaseName=item.DatabaseName
                              then select (i, i.DatabaseName, i.FileFormat, i.Details)
                      }
                |> Seq.map (fun (searchDatabase, _, _, _) -> searchDatabase)
                |> (fun searchDatabase -> 
                    if Seq.length searchDatabase < 1 
                        then 
                            query {
                                   for i in dbContext.SearchDatabase do
                                       if i.DatabaseName=item.DatabaseName
                                          then select (i, i.DatabaseName, i.FileFormat, i.Details)
                                  }
                            |> Seq.map (fun (searchDatabase, _, _, _) -> searchDatabase)
                            |> (fun searchDatabase' -> if (searchDatabase'.Count()) < 1
                                                                           then let tmp = dbContext.SearchDatabase.Find(item.ID)
                                                                                if tmp.ID = item.ID 
                                                                                then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                     dbContext.Add item |> ignore
                                                                                else dbContext.Add item |> ignore
                                                                           else searchDatabase'
                                                                                |> Seq.iter (fun searchDatabaseItem -> SearchDatabaseHandler.matchAndAddSearchDatabases dbContext searchDatabaseItem item)
                               )
                        else searchDatabase
                             |> Seq.iter (fun searchDatabaseItem -> SearchDatabaseHandler.matchAndAddSearchDatabases dbContext searchDatabaseItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SearchDatabase) =
                SearchDatabaseHandler.matchAndAddSearchDatabases dbContext item |> ignore
                dbContext.SaveChanges()

        type DBSequenceHandler =
            ///Initializes a dbsequence-object with at least all necessary parameters.
            static member init
                (
                    accession      : string,
                    searchDatabase : SearchDatabase,
                    ?id            : Guid,
                    ?name          : string,
                    ?sequence      : string,
                    ?length        : int,
                    ?details       : seq<DBSequenceParam>,
                    ?mzIdentML     : MzIdentMLDocument
                ) =
                let id'        = defaultArg id (System.Guid.NewGuid())
                let name'      = defaultArg name Unchecked.defaultof<string>
                let sequence'  = defaultArg sequence Unchecked.defaultof<string>
                let length'    = defaultArg length Unchecked.defaultof<int>
                let details'   = convertOptionToList details
                let mzIdentML' = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                new DBSequence(
                               Nullable(id'), 
                               name', 
                               accession, 
                               searchDatabase, 
                               sequence', 
                               Nullable(length'), 
                               details', 
                               mzIdentML', 
                               Nullable(DateTime.Now)
                              )

            static member addName
                (name:string) (dbSequence:DBSequence) =
                dbSequence.Name <- name
                dbSequence

            static member addSequence
                (sequence:string) (dbSequence:DBSequence) =
                dbSequence.Sequence <- sequence
                dbSequence

            static member addLength
                (length:int) (dbSequence:DBSequence) =
                dbSequence.Length <- Nullable(length)
                dbSequence

            static member addDetail
                (detail:DBSequenceParam) (dbSequence:DBSequence) =
                let result = dbSequence.Details <- addToList dbSequence.Details detail
                dbSequence

            static member addDetails
                (details:seq<DBSequenceParam>) (dbSequence:DBSequence) =
                let result = dbSequence.Details <- addCollectionToList dbSequence.Details details
                dbSequence

            static member addMzIdentML
                (mzIdentMLDocument:MzIdentMLDocument) (dbSequence:DBSequence) =
                let result = dbSequence.MzIdentMLDocument <- mzIdentMLDocument
                dbSequence

            static member tryFindByID
                (context:MzIdentML) (dbSequenceID:string) =
                tryFind (context.DBSequence.Find(dbSequenceID))

            static member private matchAndAddDBSequence (dbContext:MzIdentML) (item1:DBSequence) (item2:DBSequence) =
                if item1.Name=item2.Name && item1.Sequence=item2.Sequence && item1.Length=item2.Length && 
                   item1.Details=item2.Details && item1.MzIdentMLDocument=item2.MzIdentMLDocument &&
                   item1.SearchDatabase=item2.SearchDatabase

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:DBSequence) =
                query {
                       for i in dbContext.DBSequence.Local do
                           if i.Accession=item.Accession
                              then select (i, i.SearchDatabase, i.MzIdentMLDocument, i.Details)
                      }
                |> Seq.map (fun (dbSequence, _, _, _) -> dbSequence)
                |> (fun dbSequence -> 
                    if Seq.length dbSequence < 1 
                        then 
                            query {
                                   for i in dbContext.DBSequence do
                                       if i.Accession=item.Accession
                                          then select (i, i.SearchDatabase, i.MzIdentMLDocument, i.Details)
                                  }
                            |> Seq.map (fun (dbSequence, _, _, _) -> dbSequence)
                            |> (fun dbSequence' -> if (dbSequence'.Count()) < 1
                                                                           then let tmp = dbContext.DBSequence.Find(item.ID)
                                                                                if tmp.ID = item.ID 
                                                                                then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                     dbContext.Add item |> ignore
                                                                                else dbContext.Add item |> ignore
                                                                           else dbSequence'
                                                                                |> Seq.iter (fun dbSequenceItem -> DBSequenceHandler.matchAndAddDBSequence dbContext dbSequenceItem item)
                               )
                        else dbSequence
                             |> Seq.iter (fun dbSequenceItem -> DBSequenceHandler.matchAndAddDBSequence dbContext dbSequenceItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:DBSequence) =
                DBSequenceHandler.matchAndAddDBSequence dbContext item |> ignore
                dbContext.SaveChanges()

        type PeptideEvidenceHandler =
            ///Initializes a peptideevidence-object with at least all necessary parameters.
            static member init
                (
                    dbSequence                  : DBSequence,
                    peptide                     : Peptide,
                    ?id                         : Guid,
                    ?name                       : string,
                    ?start                      : int,
                    ?end'                       : int,
                    ?pre                        : string,
                    ?post                       : string,
                    ?frame                      : Frame,
                    ?isDecoy                    : bool,
                    ?translationTable           : TranslationTable,
                    ?details                    : seq<PeptideEvidenceParam>,
                    ?mzIdentML                  : MzIdentMLDocument
                ) =
                let id'                         = defaultArg id (System.Guid.NewGuid())
                let name'                       = defaultArg name Unchecked.defaultof<string>
                let start'                      = defaultArg start Unchecked.defaultof<int>
                let end''                       = defaultArg end' Unchecked.defaultof<int>
                let pre'                        = defaultArg pre Unchecked.defaultof<string>
                let post'                       = defaultArg post Unchecked.defaultof<string>
                let frame'                      = defaultArg frame Unchecked.defaultof<Frame>
                let isDecoy'                    = defaultArg isDecoy Unchecked.defaultof<bool>
                let translationTable'           = defaultArg translationTable Unchecked.defaultof<TranslationTable>
                let details'                    = convertOptionToList details
                let mzIdentML'                  = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                new PeptideEvidence(
                                    Nullable(id'), 
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
                                    mzIdentML', 
                                    Nullable(DateTime.Now)
                                   )

            static member addName
                (name:string) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.Name <- name
                peptideEvidence

            static member addStart
                (start:int) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.Start <- Nullable(start)
                peptideEvidence

            static member addEnd 
                (end':int) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.End  <- Nullable(end')
                peptideEvidence

            static member addPre
                (pre:string) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.Pre <- pre
                peptideEvidence

            static member addPost
                (post:string) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.Post <- post
                peptideEvidence

            static member addFrame
                (frame:Frame) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.Frame <- frame
                peptideEvidence

            static member addIsDecoy
                (isDecoy:bool) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.IsDecoy <- Nullable(isDecoy)
                peptideEvidence

            static member addTranslationTable
                (translationTable:TranslationTable) (peptideEvidence:PeptideEvidence) =
                peptideEvidence.TranslationTable <- translationTable
                peptideEvidence

            static member addDetail
                (detail:PeptideEvidenceParam) (peptideEvidence:PeptideEvidence) =
                let result = peptideEvidence.Details <- addToList peptideEvidence.Details detail
                peptideEvidence

            static member addDetails
                (details:seq<PeptideEvidenceParam>) (peptideEvidence:PeptideEvidence) =
                let result = peptideEvidence.Details <- addCollectionToList peptideEvidence.Details details
                peptideEvidence

            static member addMzIdentML
                (mzIdentMLDocument:MzIdentMLDocument) (peptideEvidence:PeptideEvidence) =
                let result = peptideEvidence.MzIdentMLDocument <- mzIdentMLDocument
                peptideEvidence

            static member tryFindByID
                (context:MzIdentML) (peptideEvidenceID:string) =
                tryFind (context.PeptideEvidence.Find(peptideEvidenceID))

            static member private matchAndAddPeptideEvidence (dbContext:MzIdentML) (item1:PeptideEvidence) (item2:PeptideEvidence) =
                if item1.Name=item2.Name && item1.Start=item2.Start && item1.End=item2.End &&
                   item1.Pre=item2.Pre && item1.Post=item2.Post && item1.Frame=item2.Frame &&
                   item1.IsDecoy=item2.IsDecoy && item1.TranslationTable=item2.TranslationTable && 
                   item1.Details=item2.Details && item1.MzIdentMLDocument=item2.MzIdentMLDocument &&
                   item1.DBSequence=item2.DBSequence

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:PeptideEvidence) =
                query {
                       for i in dbContext.PeptideEvidence.Local do
                           if i.Peptide=item.Peptide
                              then select (i, i.Peptide, i.TranslationTable, i.DBSequence,  i.MzIdentMLDocument, i.Details)
                      }
                |> Seq.map (fun (peptideEvidence, _, _, _, _, _) -> peptideEvidence)
                |> (fun peptideEvidence -> 
                    if Seq.length peptideEvidence < 1 
                        then 
                            query {
                                   for i in dbContext.PeptideEvidence do
                                       if i.Peptide=item.Peptide
                                          then select (i, i.Peptide, i.TranslationTable, i.DBSequence,  i.MzIdentMLDocument, i.Details)
                                  }
                            |> Seq.map (fun (peptideEvidence, _, _, _, _, _) -> peptideEvidence)
                            |> (fun peptideEvidence' -> if (peptideEvidence'.Count()) < 1
                                                                             then let tmp = dbContext.PeptideEvidence.Find(item.ID)
                                                                                  if tmp.ID = item.ID 
                                                                                  then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                       dbContext.Add item |> ignore
                                                                                  else dbContext.Add item |> ignore
                                                                             else peptideEvidence'
                                                                                  |> Seq.iter (fun peptideEvidenceItem -> PeptideEvidenceHandler.matchAndAddPeptideEvidence dbContext peptideEvidenceItem item)
                               )
                        else peptideEvidence
                             |> Seq.iter (fun peptideEvidenceItem -> PeptideEvidenceHandler.matchAndAddPeptideEvidence dbContext peptideEvidenceItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:PeptideEvidence) =
                PeptideEvidenceHandler.matchAndAddPeptideEvidence dbContext item |> ignore
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
                    ?id                           : Guid,
                    ?name                         : string,
                    ?sample                       : Sample,
                    ?massTable                    : MassTable,
                    ?peptideEvidences             : seq<PeptideEvidence>,
                    ?fragmentations               : seq<IonType>,
                    ?calculatedMassToCharge       : float,
                    ?calculatedPI                 : float,
                    ?details                      : seq<SpectrumIdentificationItemParam>
                ) =
                let id'                           = defaultArg id (System.Guid.NewGuid())
                let name'                         = defaultArg name Unchecked.defaultof<string>
                let sample'                       = defaultArg sample Unchecked.defaultof<Sample>
                let massTable'                    = defaultArg massTable Unchecked.defaultof<MassTable>
                let peptideEvidences'             = convertOptionToList peptideEvidences
                let fragmentations'               = convertOptionToList fragmentations
                let calculatedMassToCharge'       = defaultArg calculatedMassToCharge Unchecked.defaultof<float>
                let calculatedPI'                 = defaultArg calculatedPI Unchecked.defaultof<float>
                let details'                      = convertOptionToList details
                    
                new SpectrumIdentificationItem(
                                               Nullable(id'), 
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

            static member addName
                (name:string) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                spectrumIdentificationItem.Name <- name
                spectrumIdentificationItem

            static member addSample
                (sample:Sample) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                spectrumIdentificationItem.Sample <- sample 
                spectrumIdentificationItem

            static member addMassTable
                (massTable:MassTable) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                spectrumIdentificationItem.MassTable <- massTable
                spectrumIdentificationItem

            static member addPeptideEvidence
                (peptideEvidence:PeptideEvidence) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                let result = spectrumIdentificationItem.PeptideEvidences <- addToList spectrumIdentificationItem.PeptideEvidences peptideEvidence
                spectrumIdentificationItem

            static member addPeptideEvidences
                (peptideEvidences:seq<PeptideEvidence>) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                let result = spectrumIdentificationItem.PeptideEvidences <- addCollectionToList spectrumIdentificationItem.PeptideEvidences peptideEvidences
                spectrumIdentificationItem   

            static member addFragmentation
                (ionType:IonType) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                let result = spectrumIdentificationItem.Fragmentations <- addToList spectrumIdentificationItem.Fragmentations ionType
                spectrumIdentificationItem

            static member addFragmentations
                (ionTypes:seq<IonType>) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                let result = spectrumIdentificationItem.Fragmentations <- addCollectionToList spectrumIdentificationItem.Fragmentations ionTypes
                spectrumIdentificationItem 

            static member addCalculatedMassToCharge
                (calculatedMassToCharge:float) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                spectrumIdentificationItem.CalculatedMassToCharge <- Nullable(calculatedMassToCharge)
                //printfn "%A" calculatedMassToCharge
                spectrumIdentificationItem

            static member addCalculatedPI
                (calculatedPI:float) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                spectrumIdentificationItem.CalculatedPI <- Nullable(calculatedPI)
                spectrumIdentificationItem

            static member addDetail
                (detail:SpectrumIdentificationItemParam) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                let result = spectrumIdentificationItem.Details <- addToList spectrumIdentificationItem.Details detail
                spectrumIdentificationItem

            static member addDetails
                (details:seq<SpectrumIdentificationItemParam>) (spectrumIdentificationItem:SpectrumIdentificationItem) =
                let result = spectrumIdentificationItem.Details <- addCollectionToList spectrumIdentificationItem.Details details
                spectrumIdentificationItem

            //static member addSpectrumIdentificationResult
            //     (spectrumIdentificationItem:SpectrumIdentificationItem) (spectrumIdentificationResult:SpectrumIdentificationResult) =
            //     spectrumIdentificationItem.SpectrumIdentificationResult <- spectrumIdentificationResult

            static member tryFindByID
                (context:MzIdentML) (spectrumIdentificationItemID:string) =
                tryFind (context.SpectrumIdentificationItem.Find(spectrumIdentificationItemID))

            static member private matchAndAddSpectrumIdentificationItem (dbContext:MzIdentML) (item1:SpectrumIdentificationItem) (item2:SpectrumIdentificationItem) =
                if item1.Name=item2.Name && item1.Sample=item2.Sample && item1.MassTable=item2.MassTable && 
                   item1.PassThreshold=item2.PassThreshold && item1.Rank=item2.Rank && item1.PeptideEvidences=item2.PeptideEvidences && 
                   item1.Fragmentations=item2.Fragmentations && item1.CalculatedMassToCharge=item2.CalculatedMassToCharge && 
                   item1.CalculatedPI=item2.CalculatedPI && item1.Details=item2.Details && item1.ChargeState=item2.ChargeState && 
                   item1.ExperimentalMassToCharge=item2.ExperimentalMassToCharge

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SpectrumIdentificationItem) =
                query {
                       for i in dbContext.SpectrumIdentificationItem.Local do
                           if i.Peptide=item.Peptide
                              then select (i, i.Peptide, i.Sample, i.MassTable, 
                                        i.PeptideEvidences, i.Fragmentations, i.Details
                                       )
                      }
                |> Seq.map (fun (spectrumIdentificationItem, _, _, _, _, _, _) -> spectrumIdentificationItem)
                |> (fun spectrumIdentificationItem -> 
                    if Seq.length spectrumIdentificationItem < 1 
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationItem do
                                       if i.Peptide=item.Peptide
                                          then select (i, i.Peptide, i.Sample, i.MassTable, 
                                                       i.PeptideEvidences, i.Fragmentations, i.Details
                                                      )
                                  }
                            |> Seq.map (fun (spectrumIdentificationItem, _, _, _, _, _, _) -> spectrumIdentificationItem)
                            |> (fun spectrumIdentificationItem' -> if (spectrumIdentificationItem'.Count()) < 1
                                                                                                   then let tmp = dbContext.SpectrumIdentificationItem.Find(item.ID)
                                                                                                        if tmp.ID = item.ID 
                                                                                                           then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                                                dbContext.Add item |> ignore
                                                                                                        else dbContext.Add item |> ignore
                                                                                                   else spectrumIdentificationItem'
                                                                                                        |> Seq.iter (fun spectrumIdentificationItemsItem -> SpectrumIdentificationItemHandler.matchAndAddSpectrumIdentificationItem dbContext spectrumIdentificationItemsItem item)
                               )
                        else spectrumIdentificationItem
                             |> Seq.iter (fun spectrumIdentificationItemsItem -> SpectrumIdentificationItemHandler.matchAndAddSpectrumIdentificationItem dbContext spectrumIdentificationItemsItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SpectrumIdentificationItem) =
                SpectrumIdentificationItemHandler.matchAndAddSpectrumIdentificationItem dbContext item |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationResultHandler =
            ///Initializes a spectrumidentificationresult-object with at least all necessary parameters.
            static member init
                (
                    spectraData                 : SpectraData,
                    spectrumID                  : string,
                    spectrumIdentificationItem  : seq<SpectrumIdentificationItem>,
                    ?id                         : Guid,
                    ?name                       : string,
                    ?details                    : seq<SpectrumIdentificationResultParam>
                ) =
                let id'                         = defaultArg id (System.Guid.NewGuid())
                let name'                       = defaultArg name Unchecked.defaultof<string>
                let details'                    = convertOptionToList details
                    
                new SpectrumIdentificationResult(
                                                 Nullable(id'), 
                                                 name', 
                                                 spectraData, 
                                                 spectrumID, 
                                                 spectrumIdentificationItem |> List,
                                                 details', 
                                                 Nullable(DateTime.Now)
                                                )

            static member addName
                (name:string) (spectrumIdentificationResult:SpectrumIdentificationResult)  =
                spectrumIdentificationResult.Name <- name
                spectrumIdentificationResult

            static member addDetail
                (detail:SpectrumIdentificationResultParam) (spectrumIdentificationResult:SpectrumIdentificationResult) =
                let result = spectrumIdentificationResult.Details <- addToList spectrumIdentificationResult.Details detail
                spectrumIdentificationResult

            static member addDetails
                (details:seq<SpectrumIdentificationResultParam>) (spectrumIdentificationResult:SpectrumIdentificationResult) =
                let result = spectrumIdentificationResult.Details <- addCollectionToList spectrumIdentificationResult.Details details
                spectrumIdentificationResult

            //static member addSpectrumIdentificationList
            //     (spectrumIdentificationResult:SpectrumIdentificationResult) (spectrumIdentificationList:SpectrumIdentificationList) =
            //     spectrumIdentificationResult.SpectrumIdentificationList <- spectrumIdentificationList

            static member tryFindByID
                (context:MzIdentML) (spectrumIdentificationResultID:string) =
                tryFind (context.SpectrumIdentificationResult.Find(spectrumIdentificationResultID))

            static member private matchAndAddSpectrumIdentificationResult (dbContext:MzIdentML) (item1:SpectrumIdentificationResult) (item2:SpectrumIdentificationResult) =
                if item1.Name=item2.Name

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SpectrumIdentificationResult) =
                query {
                       for i in dbContext.SpectrumIdentificationResult.Local do
                           if i.SpectraData=item.SpectraData
                              then select (i, i.SpectrumIdentificationItem, i.Details)
                      }
                |> Seq.map (fun (spectrumIdentificationResult, _, _) -> spectrumIdentificationResult)
                |> (fun spectrumIdentificationResult -> 
                    if Seq.length spectrumIdentificationResult < 1 
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationResult do
                                       if i.SpectraData=item.SpectraData
                                          then select (i, i.SpectrumIdentificationItem, i.Details)
                                  }
                            |> Seq.map (fun (spectrumIdentificationResult, _, _) -> spectrumIdentificationResult)
                            |> (fun spectrumIdentificationResult' -> if (spectrumIdentificationResult'.Count()) < 1
                                                                                                       then let tmp = dbContext.SpectrumIdentificationResult.Find(item.ID)
                                                                                                            if tmp.ID = item.ID 
                                                                                                               then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                                                    dbContext.Add item |> ignore
                                                                                                               else dbContext.Add item |> ignore
                                                                                                       else spectrumIdentificationResult'
                                                                                                            |> Seq.iter (fun spectrumIdentificationResultItem -> SpectrumIdentificationResultHandler.matchAndAddSpectrumIdentificationResult dbContext spectrumIdentificationResultItem item)
                               )
                        else spectrumIdentificationResult
                             |> Seq.iter (fun spectrumIdentificationResultItem -> SpectrumIdentificationResultHandler.matchAndAddSpectrumIdentificationResult dbContext spectrumIdentificationResultItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SpectrumIdentificationResult) =
                SpectrumIdentificationResultHandler.matchAndAddSpectrumIdentificationResult dbContext item |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationListHandler =
            ///Initializes a spectrumidentificationlist-object with at least all necessary parameters.
            static member init
                (
                    spectrumIdentificationResult : seq<SpectrumIdentificationResult>,
                    ?id                          : Guid,
                    ?name                        : string,
                    ?numSequencesSearched        : int64,
                    ?fragmentationTable          : seq<Measure>,
                    ?details                     : seq<SpectrumIdentificationListParam>          
                ) =
                let id'                   = defaultArg id (System.Guid.NewGuid())
                let name'                 = defaultArg name Unchecked.defaultof<string>
                let numSequencesSearched' = defaultArg numSequencesSearched Unchecked.defaultof<int64>
                let fragmentationTable'   = convertOptionToList fragmentationTable
                let details'              = convertOptionToList details
                    
                new SpectrumIdentificationList(
                                               Nullable(id'), 
                                               name', 
                                               Nullable(numSequencesSearched'), 
                                               fragmentationTable', 
                                               spectrumIdentificationResult |> List,
                                               details', 
                                               Nullable(DateTime.Now)
                                              )

            static member addName
                (name:string) (spectrumIdentificationList:SpectrumIdentificationList) =
                spectrumIdentificationList.Name <- name
                spectrumIdentificationList

            static member addNumSequencesSearched
                (numSequencesSearched:int64) (spectrumIdentificationList:SpectrumIdentificationList) =
                spectrumIdentificationList.NumSequencesSearched <- Nullable(numSequencesSearched)
                spectrumIdentificationList

            static member addFragmentationTable
                (fragmentationTable:Measure) (spectrumIdentificationList:SpectrumIdentificationList) =
                let result = spectrumIdentificationList.FragmentationTables <- addToList spectrumIdentificationList.FragmentationTables fragmentationTable
                spectrumIdentificationList

            static member addFragmentationTables
                (fragmentationTables:seq<Measure>) (spectrumIdentificationList:SpectrumIdentificationList) =
                let result = spectrumIdentificationList.FragmentationTables <- addCollectionToList spectrumIdentificationList.FragmentationTables fragmentationTables
                spectrumIdentificationList

            static member addDetail
                (detail:SpectrumIdentificationListParam) (spectrumIdentificationList:SpectrumIdentificationList) =
                let result = spectrumIdentificationList.Details <- addToList spectrumIdentificationList.Details detail
                spectrumIdentificationList

            static member addDetails
                (details:seq<SpectrumIdentificationListParam>) (spectrumIdentificationList:SpectrumIdentificationList) =
                let result = spectrumIdentificationList.Details <- addCollectionToList spectrumIdentificationList.Details details
                spectrumIdentificationList

            static member tryFindByID
                (context:MzIdentML) (spectrumIdentificationListID:string) =
                tryFind (context.SpectrumIdentificationList.Find(spectrumIdentificationListID))

            static member private matchAndAddSpectrumIdentificationList (dbContext:MzIdentML) (item1:SpectrumIdentificationList) (item2:SpectrumIdentificationList) =
                if item1.NumSequencesSearched=item2.NumSequencesSearched && item1.FragmentationTables=item2.FragmentationTables &&
                   item1.SpectrumIdentificationResult=item2.SpectrumIdentificationResult && item1.Details=item2.Details

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SpectrumIdentificationList) =
                query {
                       for i in dbContext.SpectrumIdentificationList.Local do
                           if i.Name=item.Name
                              then select (i, i.FragmentationTables, i.SpectrumIdentificationResult, i.Details)
                      }
                |> Seq.map (fun (spectrumIdentificationList, _, _, _) -> spectrumIdentificationList)
                |> (fun spectrumIdentificationList -> 
                    if Seq.length spectrumIdentificationList < 1 
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentificationList do
                                       if i.Name=item.Name
                                          then select (i, i.FragmentationTables, i.SpectrumIdentificationResult, i.Details)
                                  }
                            |> Seq.map (fun (spectrumIdentificationList, _, _, _) -> spectrumIdentificationList)
                            |> (fun spectrumIdentificationList' -> if (spectrumIdentificationList'.Count()) < 1
                                                                                                   then let tmp = dbContext.SpectrumIdentificationList.Find(item.ID)
                                                                                                        if tmp.ID = item.ID 
                                                                                                           then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                                                dbContext.Add item |> ignore
                                                                                                            else dbContext.Add item |> ignore
                                                                                                   else spectrumIdentificationList'
                                                                                                        |> Seq.iter (fun spectrumIdentificationListItem -> SpectrumIdentificationListHandler.matchAndAddSpectrumIdentificationList dbContext spectrumIdentificationListItem item)
                               )
                        else spectrumIdentificationList
                             |> Seq.iter (fun spectrumIdentificationListItem -> SpectrumIdentificationListHandler.matchAndAddSpectrumIdentificationList dbContext spectrumIdentificationListItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SpectrumIdentificationList) =
                SpectrumIdentificationListHandler.matchAndAddSpectrumIdentificationList dbContext item |> ignore
                dbContext.SaveChanges()

        type SpectrumIdentificationHandler =
            ///Initializes a spectrumidentification-object with at least all necessary parameters.
            static member init
                (
                    spectrumIdentificationList     : SpectrumIdentificationList,
                    spectrumIdentificationProtocol : SpectrumIdentificationProtocol,
                    spectraData                    : seq<SpectraData>,
                    searchDatabase                 : seq<SearchDatabase>,
                    ?id                            : Guid,
                    ?name                          : string,
                    ?activityDate                  : DateTime,
                    ?mzIdentML                     : MzIdentMLDocument
                ) =
                let id'               = defaultArg id (System.Guid.NewGuid())
                let name'             = defaultArg name Unchecked.defaultof<string>
                let activityDate'     = defaultArg activityDate Unchecked.defaultof<DateTime>
                let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                new SpectrumIdentification(
                                           Nullable(id'), 
                                           name',
                                           Nullable(activityDate'), 
                                           spectrumIdentificationList, 
                                           spectrumIdentificationProtocol,
                                           spectraData |> List, 
                                           searchDatabase |> List, 
                                           mzIdentML', 
                                           Nullable(DateTime.Now)
                                          )

            static member addName
                (name:string) (spectrumIdentification:SpectrumIdentification) =
                spectrumIdentification.Name <- name
                spectrumIdentification

            static member addActivityDate
                (activityDate:DateTime) (spectrumIdentification:SpectrumIdentification) =
                spectrumIdentification.ActivityDate <- Nullable(activityDate)
                spectrumIdentification

            static member addMzIdentML
                (mzIdentMLDocument:MzIdentMLDocument) (spectrumIdentification:SpectrumIdentification) =
                let result = spectrumIdentification.MzIdentMLDocument <- mzIdentMLDocument
                spectrumIdentification

            static member tryFindByID
                (context:MzIdentML) (spectrumIdentificationID:string) =
                tryFind (context.SpectrumIdentification.Find(spectrumIdentificationID))

            static member private matchAndAddSpectrumIdentification (dbContext:MzIdentML) (item1:SpectrumIdentification) (item2:SpectrumIdentification) =
                if item1.SpectrumIdentificationList=item2.SpectrumIdentificationList && 
                   item1.MzIdentMLDocument=item2.MzIdentMLDocument && item1.Name=item2.Name &&
                   item1.SpectraData=item2.SpectraData && item1.SearchDatabase=item2.SearchDatabase &&
                   item1.ActivityDate=item2.ActivityDate

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SpectrumIdentification) =
                query {
                       for i in dbContext.SpectrumIdentification.Local do
                           if i.SpectrumIdentificationProtocol=item.SpectrumIdentificationProtocol
                              then select (i, i.SpectrumIdentificationProtocol, i.SpectrumIdentificationList, 
                                           i.SpectraData, i.SearchDatabase, i.MzIdentMLDocument
                                          )
                      }
                |> Seq.map (fun (spectrumIdentification, _, _, _, _, _) -> spectrumIdentification)
                |> (fun spectrumIdentification -> 
                    if Seq.length spectrumIdentification < 1 
                        then 
                            query {
                                   for i in dbContext.SpectrumIdentification do
                                       if i.SpectrumIdentificationProtocol=item.SpectrumIdentificationProtocol
                                          then select (i, i.SpectrumIdentificationProtocol, i.SpectrumIdentificationList, 
                                                       i.SpectraData, i.SearchDatabase, i.MzIdentMLDocument
                                                      )
                                  }
                            |> Seq.map (fun (spectrumIdentification, _, _, _, _, _) -> spectrumIdentification)
                            |> (fun spectrumIdentification' -> if (spectrumIdentification'.Count()) < 1
                                                                                           then let tmp = dbContext.SpectrumIdentification.Find(item.ID)
                                                                                                if tmp.ID = item.ID 
                                                                                                   then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                                        dbContext.Add item |> ignore
                                                                                                   else dbContext.Add item |> ignore
                                                                                           else spectrumIdentification'
                                                                                                |> Seq.iter (fun spectrumIdentificationItem -> SpectrumIdentificationHandler.matchAndAddSpectrumIdentification dbContext spectrumIdentificationItem item)
                               )
                        else spectrumIdentification
                             |> Seq.iter (fun spectrumIdentificationItem -> SpectrumIdentificationHandler.matchAndAddSpectrumIdentification dbContext spectrumIdentificationItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SpectrumIdentification) =
                SpectrumIdentificationHandler.matchAndAddSpectrumIdentification dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinDetectionProtocolHandler =
            ///Initializes a proteindetectionprotocol-object with at least all necessary parameters.
            static member init
                (
                    analysisSoftware : AnalysisSoftware,
                    threshold        : seq<ThresholdParam>,
                    ?id              : Guid,
                    ?name            : string,
                    ?analysisParams  : seq<AnalysisParam>,
                    ?mzIdentML       : MzIdentMLDocument
                ) =
                let id'             = defaultArg id (System.Guid.NewGuid())
                let name'           = defaultArg name Unchecked.defaultof<string>
                let analysisParams' = convertOptionToList analysisParams
                let mzIdentML'      = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                new ProteinDetectionProtocol(
                                             Nullable(id'), 
                                             name', 
                                             analysisSoftware, 
                                             analysisParams', 
                                             threshold |> List,
                                             mzIdentML', 
                                             Nullable(DateTime.Now)
                                            )

            static member addName
                (name:string) (proteinDetectionProtocol:ProteinDetectionProtocol) =
                proteinDetectionProtocol.Name <- name
                proteinDetectionProtocol

            static member addAnalysisParam
                (analysisParam:AnalysisParam) (proteinDetectionProtocol:ProteinDetectionProtocol) =
                let result = proteinDetectionProtocol.AnalysisParams <- addToList proteinDetectionProtocol.AnalysisParams analysisParam
                proteinDetectionProtocol

            static member addAnalysisParams
                (analysisParams:seq<AnalysisParam>) (proteinDetectionProtocol:ProteinDetectionProtocol) =
                let result = proteinDetectionProtocol.AnalysisParams <- addCollectionToList proteinDetectionProtocol.AnalysisParams analysisParams
                proteinDetectionProtocol

            static member addMzIdentML
                (mzIdentMLDocument:MzIdentMLDocument) (proteinDetectionProtocol:ProteinDetectionProtocol) =
                let result = proteinDetectionProtocol.MzIdentMLDocument <- mzIdentMLDocument
                proteinDetectionProtocol

            static member tryFindByID
                (context:MzIdentML) (proteinDetectionProtocolID:string) =
                tryFind (context.ProteinDetectionProtocol.Find(proteinDetectionProtocolID))

            static member private matchAndAddProteinDetectionProtocol (dbContext:MzIdentML) (item1:ProteinDetectionProtocol) (item2:ProteinDetectionProtocol) =
                if item1.Threshold=item2.Threshold && item1.Name=item2.Name &&
                   item1.AnalysisParams=item2.AnalysisParams && item1.MzIdentMLDocument=item2.MzIdentMLDocument

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:ProteinDetectionProtocol) =
                query {
                       for i in dbContext.ProteinDetectionProtocol.Local do
                           if i.AnalysisSoftware=item.AnalysisSoftware
                              then select (i, i.Threshold, i.AnalysisParams, i.MzIdentMLDocument)
                      }
                |> Seq.map (fun (proteinDetectionProtocol, _, _, _) -> proteinDetectionProtocol)
                |> (fun proteinDetectionProtocol -> 
                    if Seq.length proteinDetectionProtocol < 1 
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionProtocol do
                                       if i.AnalysisSoftware=item.AnalysisSoftware
                                          then select (i, i.Threshold, i.AnalysisParams, i.MzIdentMLDocument)
                                  }
                            |> Seq.map (fun (proteinDetectionProtocol, _, _, _) -> proteinDetectionProtocol)
                            |> (fun proteinDetectionProtocol' -> if (proteinDetectionProtocol'.Count()) < 1
                                                                                               then let tmp = dbContext.ProteinDetectionProtocol.Find(item.ID)
                                                                                                    if tmp.ID = item.ID 
                                                                                                       then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                                            dbContext.Add item |> ignore
                                                                                                       else dbContext.Add item |> ignore
                                                                                               else proteinDetectionProtocol'
                                                                                                    |> Seq.iter (fun proteinDetectionProtocolItem -> ProteinDetectionProtocolHandler.matchAndAddProteinDetectionProtocol dbContext proteinDetectionProtocolItem item)
                               )
                        else proteinDetectionProtocol
                             |> Seq.iter (fun proteinDetectionProtocolItem -> ProteinDetectionProtocolHandler.matchAndAddProteinDetectionProtocol dbContext proteinDetectionProtocolItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:ProteinDetectionProtocol) =
                ProteinDetectionProtocolHandler.matchAndAddProteinDetectionProtocol dbContext item |> ignore
                dbContext.SaveChanges()

        type SourceFileHandler =
            ///Initializes a sourcefile-object with at least all necessary parameters.
            static member init
                (             
                    location                     : string,
                    fileFormat                   : CVParam,
                    ?id                          : Guid,
                    ?name                        : string,
                    ?externalFormatDocumentation : string,
                    ?details                     : seq<SourceFileParam>
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let externalFormatDocumentation' = defaultArg externalFormatDocumentation Unchecked.defaultof<string>
                let details'                     = convertOptionToList details
                    
                new SourceFile(
                               Nullable(id'), 
                               name', 
                               location, 
                               externalFormatDocumentation', 
                               fileFormat, 
                               details', 
                               Nullable(DateTime.Now)
                              )

            static member addName
                (name:string) (sourceFile:SourceFile) =
                sourceFile.Name <- name
                sourceFile

            static member addExternalFormatDocumentation
                (externalFormatDocumentation:string) (sourceFile:SourceFile) =
                sourceFile.ExternalFormatDocumentation <- externalFormatDocumentation
                sourceFile

            static member addDetail
                (detail:SourceFileParam) (sourceFile:SourceFile) =
                let result = sourceFile.Details <- addToList sourceFile.Details detail
                sourceFile

            static member addDetails
                (details:seq<SourceFileParam>) (sourceFile:SourceFile) =
                let result = sourceFile.Details <- addCollectionToList sourceFile.Details details
                sourceFile

            //static member addInputs
            //     (sourceFile:SourceFile) (inputs:Inputs) =
            //     sourceFile.Inputs <- inputs

            static member tryFindByID
                (context:MzIdentML) (sourceFileID:string) =
                tryFind (context.SourceFile.Find(sourceFileID))

            static member private matchAndAddSourceFiles (dbContext:MzIdentML) (item1:SourceFile) (item2:SourceFile) =
                if item1.Location=item2.Location && item1.Name=item2.Name && item1.Details=item2.Details &&
                   item1.ExternalFormatDocumentation=item2.ExternalFormatDocumentation

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:SourceFile) =
                query {
                       for i in dbContext.SourceFile.Local do
                           if i.FileFormat=item.FileFormat
                              then select (i, i.FileFormat, i.Details)
                      }
                |> Seq.map (fun (sourceFile, _, _) -> sourceFile)
                |> (fun sourceFile -> 
                    if Seq.length sourceFile < 1 
                        then 
                            query {
                                   for i in dbContext.SourceFile do
                                       if i.FileFormat=item.FileFormat
                                          then select (i, i.FileFormat, i.Details)
                                  }
                            |> Seq.map (fun (sourceFile, _, _) -> sourceFile)
                            |> (fun sourceFile' -> if (sourceFile'.Count()) < 1
                                                                   then let tmp = dbContext.SourceFile.Find(item.ID)
                                                                        if tmp.ID = item.ID 
                                                                           then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                dbContext.Add item |> ignore
                                                                           else dbContext.Add item |> ignore
                                                                   else sourceFile'
                                                                        |> Seq.iter (fun sourceFileItem -> SourceFileHandler.matchAndAddSourceFiles dbContext sourceFileItem item)
                               )
                        else sourceFile
                             |> Seq.iter (fun sourceFileItem -> SourceFileHandler.matchAndAddSourceFiles dbContext sourceFileItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:SourceFile) =
                SourceFileHandler.matchAndAddSourceFiles dbContext item |> ignore
                dbContext.SaveChanges()

        type InputsHandler =
            ///Initializes an inputs-object with at least all necessary parameters.
            static member init
                (              
                    spectraData     : seq<SpectraData>,
                    ?id             : Guid,
                    ?sourceFile     : seq<SourceFile>,
                    ?searchDatabase : seq<SearchDatabase>,
                    ?mzIdentML      : MzIdentMLDocument
                ) =
                let id'             = defaultArg id (System.Guid.NewGuid())
                let sourceFile'     = convertOptionToList sourceFile
                let searchDatabase' = convertOptionToList searchDatabase
                let mzIdentML'      = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                new Inputs(
                           Nullable(id'), 
                           sourceFile', 
                           searchDatabase', 
                           spectraData |> List, 
                           mzIdentML', 
                           Nullable(DateTime.Now)
                          )

            static member addSourceFile
                (sourceFile:SourceFile) (inputs:Inputs) =
                let result = inputs.SourceFiles <- addToList inputs.SourceFiles sourceFile
                inputs

            static member addSourceFiles
                (sourceFiles:seq<SourceFile>) (inputs:Inputs) =
                let result = inputs.SourceFiles <- addCollectionToList inputs.SourceFiles sourceFiles
                inputs

            static member addSearchDatabase
                (searchDatabase:SearchDatabase) (inputs:Inputs) =
                let result = inputs.SearchDatabases <- addToList inputs.SearchDatabases searchDatabase
                inputs

            static member addSearchDatabases
                (searchDatabases:seq<SearchDatabase>) (inputs:Inputs) =
                let result = inputs.SearchDatabases <- addCollectionToList inputs.SearchDatabases searchDatabases
                inputs

            static member addMzIdentML
                (mzIdentMLDocument:MzIdentMLDocument) (inputs:Inputs) =
                let result = inputs.MzIdentMLDocument <- mzIdentMLDocument
                inputs

            static member tryFindByID
                (context:MzIdentML) (inputsID:string) =
                tryFind (context.Inputs.Find(inputsID))

            static member private matchAndAddInputs (dbContext:MzIdentML) (item1:Inputs) (item2:Inputs) =
                if item1.SourceFiles=item2.SourceFiles && item1.SearchDatabases=item2.SearchDatabases &&
                   item1.MzIdentMLDocument=item2.MzIdentMLDocument

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Inputs) =
                query {
                       for i in dbContext.Inputs.Local do
                           if i.SpectraData=item.SpectraData
                              then select (i, i.SourceFiles, i.SpectraData, i.SearchDatabases, i.MzIdentMLDocument)
                      }
                |> Seq.map (fun (inputs, _, _, _, _) -> inputs)
                |> (fun inputs -> 
                    if Seq.length inputs < 1 
                        then 
                            query {
                                   for i in dbContext.Inputs do
                                       if i.SpectraData=item.SpectraData
                                          then select (i, i.SourceFiles, i.SpectraData, i.SearchDatabases, i.MzIdentMLDocument)
                                  }
                            |> Seq.map (fun (inputs, _, _, _, _) -> inputs)
                            |> (fun inputs' -> if (inputs'.Count()) < 1
                                                                   then let tmp = dbContext.Inputs.Find(item.ID)
                                                                        if tmp.ID = item.ID 
                                                                           then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                dbContext.Add item |> ignore
                                                                           else dbContext.Add item |> ignore
                                                                   else inputs'
                                                                        |> Seq.iter (fun inputItem -> InputsHandler.matchAndAddInputs dbContext inputItem item)
                               )
                        else inputs
                             |> Seq.iter (fun inputItem -> InputsHandler.matchAndAddInputs dbContext inputItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Inputs) =
                InputsHandler.matchAndAddInputs dbContext item |> ignore
                dbContext.SaveChanges()

        type PeptideHypothesisHandler =
            ///Initializes a peptidehypothesis-object with at least all necessary parameters.
            static member init
                (              
                    peptideEvidence             : PeptideEvidence,
                    spectrumIdentificationItems : seq<SpectrumIdentificationItem>,
                    ?id                         : Guid
                ) =
                let id' = defaultArg id (System.Guid.NewGuid())
                    
                new PeptideHypothesis(
                                      Nullable(id'), 
                                      peptideEvidence, 
                                      spectrumIdentificationItems |> List, 
                                      Nullable(DateTime.Now)
                                     )

            static member tryFindByID
                (context:MzIdentML) (peptideHypothesisID:string) =
                tryFind (context.PeptideHypothesis.Find(peptideHypothesisID))

            static member private matchAndAddPeptideHypothesis (dbContext:MzIdentML) (item1:PeptideHypothesis) (item2:PeptideHypothesis) =
                if item1.SpectrumIdentificationItems=item2.SpectrumIdentificationItems

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:PeptideHypothesis) =
                query {
                       for i in dbContext.PeptideHypothesis.Local do
                           if i.PeptideEvidence=item.PeptideEvidence
                              then select (i, i.PeptideEvidence, i.SpectrumIdentificationItems)
                      }
                |> Seq.map (fun (peptideHypothesis, _, _) -> peptideHypothesis)
                |> (fun peptideHypothesis -> 
                    if Seq.length peptideHypothesis < 1 
                        then 
                            query {
                                   for i in dbContext.PeptideHypothesis do
                                       if i.PeptideEvidence=item.PeptideEvidence
                                          then select (i, i.PeptideEvidence, i.SpectrumIdentificationItems)
                                  }
                            |> Seq.map (fun (peptideHypothesis, _, _) -> peptideHypothesis)
                            |> (fun peptideHypothesis' -> if (peptideHypothesis'.Count()) < 1
                                                                   then let tmp = dbContext.PeptideHypothesis.Find(item.ID)
                                                                        if tmp.ID = item.ID 
                                                                           then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                dbContext.Add item |> ignore
                                                                           else dbContext.Add item |> ignore
                                                                   else peptideHypothesis'
                                                                        |> Seq.iter (fun peptideHypothesisItem -> PeptideHypothesisHandler.matchAndAddPeptideHypothesis dbContext peptideHypothesisItem item)
                               )
                        else peptideHypothesis
                             |> Seq.iter (fun peptideHypothesisItem -> PeptideHypothesisHandler.matchAndAddPeptideHypothesis dbContext peptideHypothesisItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:PeptideHypothesis) =
                PeptideHypothesisHandler.matchAndAddPeptideHypothesis dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinDetectionHypothesisHandler =
            ///Initializes a proteindetectionhypothesis-object with at least all necessary parameters.
            static member init
                (             
                    passThreshold     : bool,
                    dbSequence        : DBSequence,
                    peptideHypothesis : seq<PeptideHypothesis>,
                    ?id               : Guid,
                    ?name             : string,
                    ?details          : seq<ProteinDetectionHypothesisParam>,
                    ?mzIdentML        : MzIdentMLDocument
                ) =
                let id'        = defaultArg id (System.Guid.NewGuid())
                let name'      = defaultArg name Unchecked.defaultof<string>
                let details'   = convertOptionToList details
                let mzIdentML' = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                new ProteinDetectionHypothesis(
                                               Nullable(id'), 
                                               name', 
                                               Nullable(passThreshold), 
                                               dbSequence, 
                                               peptideHypothesis |> List,
                                               details', 
                                               mzIdentML', 
                                               Nullable(DateTime.Now)
                                              )

            static member addName
                (name:string) (proteinDetectionHypothesis:ProteinDetectionHypothesis) =
                proteinDetectionHypothesis.Name <- name
                proteinDetectionHypothesis

            static member addDetail
                (detail:ProteinDetectionHypothesisParam) (proteinDetectionHypothesis:ProteinDetectionHypothesis) =
                let result = proteinDetectionHypothesis.Details <- addToList proteinDetectionHypothesis.Details detail
                proteinDetectionHypothesis

            static member addDetails
                (details:seq<ProteinDetectionHypothesisParam>) (proteinDetectionHypothesis:ProteinDetectionHypothesis) =
                let result = proteinDetectionHypothesis.Details <- addCollectionToList proteinDetectionHypothesis.Details details
                proteinDetectionHypothesis

            static member addMzIdentML
                (mzIdentMLDocument:MzIdentMLDocument) (proteinDetectionHypothesis:ProteinDetectionHypothesis) =
                let result = proteinDetectionHypothesis.MzIdentMLDocument <- mzIdentMLDocument
                proteinDetectionHypothesis

            static member tryFindByID
                (context:MzIdentML) (proteinDetectionHypothesisID:string) =
                tryFind (context.ProteinDetectionHypothesis.Find(proteinDetectionHypothesisID))

            static member private matchAndAddProteinDetectionHypothesis (dbContext:MzIdentML) (item1:ProteinDetectionHypothesis) (item2:ProteinDetectionHypothesis) =
                if item1.PassThreshold=item2.PassThreshold && item1.PeptideHypothesis=item2.PeptideHypothesis &&
                   item1.Name=item2.Name && item1.Details=item2.Details && item1.MzIdentMLDocument=item2.MzIdentMLDocument

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:ProteinDetectionHypothesis) =
                query {
                       for i in dbContext.ProteinDetectionHypothesis.Local do
                           if i.DBSequence=item.DBSequence
                              then select (i, i.PeptideHypothesis, i.Details, i. MzIdentMLDocument)
                      }
                |> Seq.map (fun (proteinDetectionHypothesis, _, _, _) -> proteinDetectionHypothesis)
                |> (fun proteinDetectionHypothesis -> 
                    if Seq.length proteinDetectionHypothesis < 1 
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionHypothesis do
                                       if i.DBSequence=item.DBSequence
                                          then select (i, i.PeptideHypothesis, i.Details, i. MzIdentMLDocument)
                                  }
                            |> Seq.map (fun (proteinDetectionHypothesis, _, _, _) -> proteinDetectionHypothesis)
                            |> (fun proteinDetectionHypothesis' -> if (proteinDetectionHypothesis'.Count()) < 1
                                                                                                   then let tmp = dbContext.ProteinDetectionHypothesis.Find(item.ID)
                                                                                                        if tmp.ID = item.ID 
                                                                                                           then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                                                dbContext.Add item |> ignore
                                                                                                           else dbContext.Add item |> ignore
                                                                                                   else proteinDetectionHypothesis'
                                                                                                        |> Seq.iter (fun proteinDetectionHypothesisItem -> ProteinDetectionHypothesisHandler.matchAndAddProteinDetectionHypothesis dbContext proteinDetectionHypothesisItem item)
                               )
                        else proteinDetectionHypothesis
                             |> Seq.iter (fun proteinDetectionHypothesisItem -> ProteinDetectionHypothesisHandler.matchAndAddProteinDetectionHypothesis dbContext proteinDetectionHypothesisItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:ProteinDetectionHypothesis) =
                ProteinDetectionHypothesisHandler.matchAndAddProteinDetectionHypothesis dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinAmbiguityGroupHandler =
            ///Initializes a proteinambiguitygroup-object with at least all necessary parameters.
            static member init
                (             
                    proteinDetecionHypothesis : seq<ProteinDetectionHypothesis>,
                    ?id                       : Guid,
                    ?name                     : string,
                    ?details                  : seq<ProteinAmbiguityGroupParam>
                ) =
                let id'                          = defaultArg id (System.Guid.NewGuid())
                let name'                        = defaultArg name Unchecked.defaultof<string>
                let details'                     = convertOptionToList details
                    
                new ProteinAmbiguityGroup(
                                          Nullable(id'), 
                                          name', 
                                          proteinDetecionHypothesis |> List, 
                                          details', 
                                          Nullable(DateTime.Now)
                                         )

            static member addName
                (name:string) (proteinAmbiguityGroup:ProteinAmbiguityGroup) =
                proteinAmbiguityGroup.Name <- name
                proteinAmbiguityGroup

            static member addDetail
                (detail:ProteinAmbiguityGroupParam) (proteinAmbiguityGroup:ProteinAmbiguityGroup) =
                let result = proteinAmbiguityGroup.Details <- addToList proteinAmbiguityGroup.Details detail
                proteinAmbiguityGroup

            static member addDetails
                (details:seq<ProteinAmbiguityGroupParam>) (proteinAmbiguityGroup:ProteinAmbiguityGroup) =
                let result = proteinAmbiguityGroup.Details <- addCollectionToList proteinAmbiguityGroup.Details details
                proteinAmbiguityGroup

            static member tryFindByID
                (context:MzIdentML) (proteinAmbiguityGroupID:string) =
                tryFind (context.ProteinAmbiguityGroup.Find(proteinAmbiguityGroupID))

            static member private matchAndAddProteinAmbiguityGroups (dbContext:MzIdentML) (item1:ProteinAmbiguityGroup) (item2:ProteinAmbiguityGroup) =
                if item1.Name=item2.Name && item1.Details=item2.Details

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:ProteinAmbiguityGroup) =
                query {
                       for i in dbContext.ProteinAmbiguityGroup.Local do
                           if i.ProteinDetecionHypothesis=item.ProteinDetecionHypothesis
                              then select (i, i.ProteinDetecionHypothesis, i.Details)
                      }
                |> Seq.map (fun (proteinAmbiguityGroup, _, _) -> proteinAmbiguityGroup)
                |> (fun proteinAmbiguityGroup -> 
                    if Seq.length proteinAmbiguityGroup < 1 
                        then 
                            query {
                                   for i in dbContext.ProteinAmbiguityGroup do
                                       if i.ProteinDetecionHypothesis=item.ProteinDetecionHypothesis
                                          then select (i, i.ProteinDetecionHypothesis, i.Details)
                                  }
                            |> Seq.map (fun (proteinAmbiguityGroup, _, _) -> proteinAmbiguityGroup)
                            |> (fun proteinAmbiguityGroup' -> if (proteinAmbiguityGroup'.Count()) < 1
                                                                                         then let tmp = dbContext.ProteinAmbiguityGroup.Find(item.ID)
                                                                                              if tmp.ID = item.ID 
                                                                                                 then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                                      dbContext.Add item |> ignore
                                                                                                 else dbContext.Add item |> ignore
                                                                                         else proteinAmbiguityGroup'
                                                                                              |> Seq.iter (fun proteinAmbiguityGroupItem -> ProteinAmbiguityGroupHandler.matchAndAddProteinAmbiguityGroups dbContext proteinAmbiguityGroupItem item)
                               )
                        else proteinAmbiguityGroup
                             |> Seq.iter (fun proteinAmbiguityGroupItem -> ProteinAmbiguityGroupHandler.matchAndAddProteinAmbiguityGroups dbContext proteinAmbiguityGroupItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:ProteinAmbiguityGroup) =
                ProteinAmbiguityGroupHandler.matchAndAddProteinAmbiguityGroups dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinDetectionListHandler =
            ///Initializes a proteindetectionlist-object with at least all necessary parameters.
            static member init
                (             
                    ?id                     : Guid,
                    ?name                   : string,
                    ?proteinAmbiguityGroups : seq<ProteinAmbiguityGroup>,
                    ?details                : seq<ProteinDetectionListParam>
                ) =
                let id'                     = defaultArg id (System.Guid.NewGuid())
                let name'                   = defaultArg name Unchecked.defaultof<string>
                let proteinAmbiguityGroups' = convertOptionToList proteinAmbiguityGroups
                let details'                = convertOptionToList details
                    
                new ProteinDetectionList(
                                         Nullable(id'), 
                                         name', 
                                         proteinAmbiguityGroups', 
                                         details', 
                                         Nullable(DateTime.Now)
                                        )

            static member addName
                (name:string) (proteinDetectionList:ProteinDetectionList) =
                proteinDetectionList.Name <- name
                proteinDetectionList

            static member addProteinAmbiguityGroup
                (proteinAmbiguityGroup:ProteinAmbiguityGroup) (proteinDetectionList:ProteinDetectionList) =
                let result = proteinDetectionList.ProteinAmbiguityGroups <- addToList proteinDetectionList.ProteinAmbiguityGroups proteinAmbiguityGroup
                proteinDetectionList

            static member addProteinAmbiguityGroups
                (proteinAmbiguityGroups:seq<ProteinAmbiguityGroup>) (proteinDetectionList:ProteinDetectionList) =
                let result = proteinDetectionList.ProteinAmbiguityGroups <- addCollectionToList proteinDetectionList.ProteinAmbiguityGroups proteinAmbiguityGroups
                proteinDetectionList

            static member addDetail
                (detail:ProteinDetectionListParam) (proteinDetectionList:ProteinDetectionList) =
                let result = proteinDetectionList.Details <- addToList proteinDetectionList.Details detail
                proteinDetectionList

            static member addDetails
                (details:seq<ProteinDetectionListParam>) (proteinDetectionList:ProteinDetectionList) =
                let result = proteinDetectionList.Details <- addCollectionToList proteinDetectionList.Details details
                proteinDetectionList

            static member tryFindByID
                (context:MzIdentML) (proteinDetectionListID:string) =
                tryFind (context.ProteinDetectionList.Find(proteinDetectionListID))

            static member private matchAndAddProteinDetectionList (dbContext:MzIdentML) (item1:ProteinDetectionList) (item2:ProteinDetectionList) =
                if item1.ProteinAmbiguityGroups=item2.ProteinAmbiguityGroups && item1.Details=item2.Details

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:ProteinDetectionList) =
                query {
                       for i in dbContext.ProteinDetectionList.Local do
                           if i.Name=item.Name
                              then select (i, i.ProteinAmbiguityGroups, i.Details)
                      }
                |> Seq.map (fun (proteinAmbiguityGroup, _, _) -> proteinAmbiguityGroup)
                |> (fun proteinDetectionList -> 
                    if Seq.length proteinDetectionList < 1 
                        then 
                            query {
                                   for i in dbContext.ProteinDetectionList do
                                       if i.Name=item.Name
                                          then select (i, i.ProteinAmbiguityGroups, i.Details)
                                  }
                            |> Seq.map (fun (proteinDetectionList, _, _) -> proteinDetectionList)
                            |> (fun proteinDetectionList' -> if (proteinDetectionList'.Count()) < 1
                                                                                       then let tmp = dbContext.ProteinDetectionList.Find(item.ID)
                                                                                            if tmp.ID = item.ID 
                                                                                               then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                                    dbContext.Add item |> ignore
                                                                                               else dbContext.Add item |> ignore
                                                                                       else proteinDetectionList'
                                                                                            |> Seq.iter (fun proteinDetectionListItem -> ProteinDetectionListHandler.matchAndAddProteinDetectionList dbContext proteinDetectionListItem item)
                               )
                        else proteinDetectionList
                             |> Seq.iter (fun proteinDetectionListItem -> ProteinDetectionListHandler.matchAndAddProteinDetectionList dbContext proteinDetectionListItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:ProteinDetectionList) =
                ProteinDetectionListHandler.matchAndAddProteinDetectionList dbContext item |> ignore
                dbContext.SaveChanges()

        type AnalysisDataHandler =
            ///Initializes a analysisdata-object with at least all necessary parameters.
            static member init
                (             
                    spectrumIdentificationList : seq<SpectrumIdentificationList>,
                    ?id                        : Guid,
                    ?proteinDetectionList      : ProteinDetectionList,
                    ?mzIdentML                 : MzIdentMLDocument
                ) =
                let id'                   = defaultArg id (System.Guid.NewGuid())
                let proteinDetectionList' = defaultArg proteinDetectionList Unchecked.defaultof<ProteinDetectionList>
                let mzIdentML'            = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                new AnalysisData(
                                 Nullable(id'), 
                                 spectrumIdentificationList |> List, 
                                 proteinDetectionList', 
                                 mzIdentML', 
                                 Nullable(DateTime.Now)
                                )

            static member addProteinDetectionList
                (proteinDetectionList:ProteinDetectionList) (analysisData:AnalysisData) =
                analysisData.ProteinDetectionList <- proteinDetectionList
                analysisData

            static member addMzIdentML
                (mzIdentMLDocument:MzIdentMLDocument) (analysisData:AnalysisData) =
                let result = analysisData.MzIdentMLDocument <- mzIdentMLDocument
                analysisData

            static member tryFindByID
                (context:MzIdentML) (analysisDataID:string) =
                tryFind (context.AnalysisData.Find(analysisDataID))

            static member private matchAndAddAnalysisData (dbContext:MzIdentML) (item1:AnalysisData) (item2:AnalysisData) =
                if item1.ProteinDetectionList=item2.ProteinDetectionList && item1.MzIdentMLDocument=item2.MzIdentMLDocument

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:AnalysisData) =
                query {
                       for i in dbContext.AnalysisData.Local do
                           if i.SpectrumIdentificationList=item.SpectrumIdentificationList
                              then select (i, i.SpectrumIdentificationList, i.ProteinDetectionList, i.MzIdentMLDocument)
                      }
                |> Seq.map (fun (analysisData, _, _, _) -> analysisData)
                |> (fun analysisData -> 
                    if Seq.length analysisData < 1 
                        then 
                            query {
                                   for i in dbContext.AnalysisData do
                                       if i.SpectrumIdentificationList=item.SpectrumIdentificationList
                                          then select (i, i.SpectrumIdentificationList, i.ProteinDetectionList, i.MzIdentMLDocument)
                                  }
                            |> Seq.map (fun (analysisData, _, _, _) -> analysisData)
                            |> (fun analysisData' -> if (analysisData'.Count()) < 1
                                                                                                   then let tmp = dbContext.AnalysisData.Find(item.ID)
                                                                                                        if tmp.ID = item.ID 
                                                                                                           then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                                                dbContext.Add item |> ignore
                                                                                                           else dbContext.Add item |> ignore
                                                                                                   else analysisData'
                                                                                                        |> Seq.iter (fun analysisDataItem -> AnalysisDataHandler.matchAndAddAnalysisData dbContext analysisDataItem item)
                               )
                        else analysisData
                             |> Seq.iter (fun analysisDataItem -> AnalysisDataHandler.matchAndAddAnalysisData dbContext analysisDataItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:AnalysisData) =
                AnalysisDataHandler.matchAndAddAnalysisData dbContext item |> ignore
                dbContext.SaveChanges()

        type ProteinDetectionHandler =
            ///Initializes a proteindetection-object with at least all necessary parameters.
            static member init
                (             
                    proteinDetectionList        : ProteinDetectionList,
                    proteinDetectionProtocol    : ProteinDetectionProtocol,
                    spectrumIdentificationLists : seq<SpectrumIdentificationList>,
                    ?id                         : Guid,
                    ?name                       : string,
                    ?activityDate               : DateTime
                ) =
                let id'           = defaultArg id (System.Guid.NewGuid())
                let name'         = defaultArg name Unchecked.defaultof<string>
                let activityDate' = defaultArg activityDate Unchecked.defaultof<DateTime>
                    
                new ProteinDetection(
                                     Nullable(id'), 
                                     name', 
                                     Nullable(activityDate'), 
                                     proteinDetectionList, 
                                     proteinDetectionProtocol,
                                     spectrumIdentificationLists |> List, 
                                     Nullable(DateTime.Now)
                                    )

            static member addName
                (name:string) (proteinDetection:ProteinDetection) =
                proteinDetection.Name <- name
                proteinDetection

            static member addActivityDate
                (activityDate:DateTime) (proteinDetection:ProteinDetection) =
                proteinDetection.ActivityDate <- Nullable(activityDate)
                proteinDetection

            static member tryFindByID
                (context:MzIdentML) (proteinDetectionID:string) =
                tryFind (context.ProteinDetection.Find(proteinDetectionID))

            static member private matchAndAddProteinDetection (dbContext:MzIdentML) (item1:ProteinDetection) (item2:ProteinDetection) =
                if item1.ProteinDetectionProtocol=item2.ProteinDetectionProtocol && item1.SpectrumIdentificationLists=item2.SpectrumIdentificationLists &&
                   item1.Name=item2.Name && item1.ActivityDate=item2.ActivityDate

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:ProteinDetection) =
                query {
                       for i in dbContext.ProteinDetection.Local do
                           if i.ProteinDetectionList=item.ProteinDetectionList
                              then select (i, i.ProteinDetectionProtocol, i.ProteinDetectionList, i.SpectrumIdentificationLists)
                      }
                |> Seq.map (fun (proteinDetection, _, _, _) -> proteinDetection)
                |> (fun proteinDetection -> 
                    if Seq.length proteinDetection < 1 
                        then 
                            query {
                                   for i in dbContext.ProteinDetection do
                                       if i.ProteinDetectionList=item.ProteinDetectionList
                                          then select (i, i.ProteinDetectionProtocol, i.ProteinDetectionList, i.SpectrumIdentificationLists)
                                  }
                            |> Seq.map (fun (proteinDetection, _, _, _) -> proteinDetection)
                            |> (fun proteinDetection' -> if (proteinDetection'.Count()) < 1
                                                                               then let tmp = dbContext.ProteinDetection.Find(item.ID)
                                                                                    if tmp.ID = item.ID 
                                                                                       then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                            dbContext.Add item |> ignore
                                                                                       else dbContext.Add item |> ignore
                                                                               else proteinDetection'
                                                                                    |> Seq.iter (fun proteinDetectionItem -> ProteinDetectionHandler.matchAndAddProteinDetection dbContext proteinDetectionItem item)
                               )
                        else proteinDetection
                             |> Seq.iter (fun proteinDetectionItem -> ProteinDetectionHandler.matchAndAddProteinDetection dbContext proteinDetectionItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:ProteinDetection) =
                ProteinDetectionHandler.matchAndAddProteinDetection dbContext item |> ignore
                dbContext.SaveChanges()

        type BiblioGraphicReferenceHandler =
            ///Initializes a bibliographicreference-object with at least all necessary parameters.
            static member init
                (             
                    ?id          : Guid,
                    ?name        : string,
                    ?authors     : string,
                    ?doi         : string,
                    ?editor      : string,
                    ?issue       : string,
                    ?pages       : string,
                    ?publication : string,
                    ?publisher   : string,
                    ?title       : string,
                    ?volume      : string,
                    ?year        : int,
                    ?mzIdentML   : MzIdentMLDocument
                ) =
                let id'          = defaultArg id (System.Guid.NewGuid())
                let name'        = defaultArg name Unchecked.defaultof<string>
                let authors'     = defaultArg authors Unchecked.defaultof<string>
                let doi'         = defaultArg doi Unchecked.defaultof<string>
                let editor'      = defaultArg editor Unchecked.defaultof<string>
                let issue'       = defaultArg issue Unchecked.defaultof<string>
                let pages'       = defaultArg pages Unchecked.defaultof<string>
                let publication' = defaultArg publication Unchecked.defaultof<string>
                let publisher'   = defaultArg publisher Unchecked.defaultof<string>
                let title'       = defaultArg title Unchecked.defaultof<string>
                let volume'      = defaultArg volume Unchecked.defaultof<string>
                let year'        = defaultArg year Unchecked.defaultof<int>
                let mzIdentML'   = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                new BiblioGraphicReference(
                                           Nullable(id'), 
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
                                           mzIdentML', 
                                           Nullable(DateTime.Now)
                                          )

            static member addName
                (name:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Name <- name
                biblioGraphicReference

            static member addAuthors
                (authors:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Authors <- authors
                biblioGraphicReference

            static member addDOI
                (doi:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.DOI <- doi
                biblioGraphicReference

            static member addEditor
                (editor:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Editor <- editor
                biblioGraphicReference

            static member addIssue
                (issue:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Issue <- issue
                biblioGraphicReference

            static member addPages
                (pages:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Pages <- pages
                biblioGraphicReference

            static member addPublication
                (publication:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Publication <- publication
                biblioGraphicReference

            static member addPublisher
                (publisher:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Publisher <- publisher
                biblioGraphicReference

            static member addTitle
                (title:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Title <- title
                biblioGraphicReference

            static member addVolume
                (volume:string) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Volume <- volume
                biblioGraphicReference

            static member addYear
                (year:int) (biblioGraphicReference:BiblioGraphicReference) =
                biblioGraphicReference.Year <- Nullable(year)
                biblioGraphicReference

            static member addMzIdentML
                (mzIdentMLDocument:MzIdentMLDocument) (biblioGraphicReference:BiblioGraphicReference) =
                let result = biblioGraphicReference.MzIdentMLDocument <- mzIdentMLDocument
                biblioGraphicReference

            static member tryFindByID
                (context:MzIdentML) (biblioGraphicReferenceID:string) =
                tryFind (context.BiblioGraphicReference.Find(biblioGraphicReferenceID))

            static member private matchAndAddBiblioGraphicReference (dbContext:MzIdentML) (item1:BiblioGraphicReference) (item2:BiblioGraphicReference) =
                if item1.Authors=item2.Authors && item1.DOI=item2.DOI && item1.Editor=item2.Editor && item1.Issue=item2.Issue &&
                   item1.Pages=item2.Pages && item1.Publication=item2.Publication && item1.Publisher=item2.Publisher && item1.Title=item2.Title &&
                   item1.Volume=item2.Volume && item1.Year=item2.Year && item1.MzIdentMLDocument=item2.MzIdentMLDocument

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:BiblioGraphicReference) =
                query {
                       for i in dbContext.BiblioGraphicReference.Local do
                           if i.Name=item.Name
                              then select (i, i.MzIdentMLDocument)
                      }
                |> Seq.map (fun (biblioGraphicReference, _) -> biblioGraphicReference)
                |> (fun biblioGraphicReference -> 
                    if Seq.length biblioGraphicReference < 1 
                        then 
                            query {
                                   for i in dbContext.BiblioGraphicReference do
                                       if i.Name=item.Name
                                          then select (i, i.MzIdentMLDocument)
                                  }
                            |> Seq.map (fun (biblioGraphicReference, _) -> biblioGraphicReference)
                            |> (fun biblioGraphicReference' -> if (biblioGraphicReference'.Count()) < 1
                                                                               then let tmp = dbContext.BiblioGraphicReference.Find(item.ID)
                                                                                    if tmp.ID = item.ID 
                                                                                       then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                            dbContext.Add item |> ignore
                                                                                       else dbContext.Add item |> ignore
                                                                               else biblioGraphicReference'
                                                                                    |> Seq.iter (fun biblioGraphicReferenceItem -> BiblioGraphicReferenceHandler.matchAndAddBiblioGraphicReference dbContext biblioGraphicReferenceItem item)
                               )
                        else biblioGraphicReference
                             |> Seq.iter (fun biblioGraphicReferenceItem -> BiblioGraphicReferenceHandler.matchAndAddBiblioGraphicReference dbContext biblioGraphicReferenceItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:BiblioGraphicReference) =
                BiblioGraphicReferenceHandler.matchAndAddBiblioGraphicReference dbContext item |> ignore
                dbContext.SaveChanges()

        type ProviderHandler =
            ///Initializes a provider-object with at least all necessary parameters.
            static member init
                (             
                    ?id               : Guid,
                    ?name             : string,
                    ?analysisSoftware : AnalysisSoftware,
                    ?contactRole      : ContactRole,
                    ?mzIdentML        : MzIdentMLDocument
                ) =
                let id'               = defaultArg id (System.Guid.NewGuid())
                let name'             = defaultArg name Unchecked.defaultof<string>
                let analysisSoftware' = defaultArg analysisSoftware Unchecked.defaultof<AnalysisSoftware>
                let contactRole'      = defaultArg contactRole Unchecked.defaultof<ContactRole>
                let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                new Provider(
                             Nullable(id'), 
                             name', 
                             analysisSoftware', 
                             contactRole', 
                             mzIdentML', 
                             Nullable(DateTime.Now)
                            )

            static member addName
                (name:string) (provider:Provider) =
                provider.Name <- name
                provider

            static member addAnalysisSoftware
                (analysisSoftware:AnalysisSoftware) (provider:Provider) =
                provider.AnalysisSoftware <- analysisSoftware
                provider

            static member addContactRole
                (contactRole:ContactRole) (provider:Provider) =
                provider.ContactRole <- contactRole
                provider

            static member addMzIdentML
                (mzIdentMLDocument:MzIdentMLDocument) (provider:Provider) =
                let result = provider.MzIdentMLDocument <- mzIdentMLDocument
                provider

            static member tryFindByID
                (context:MzIdentML) (providerID:string) =
                tryFind (context.Provider.Find(providerID))

            static member private matchAndAddProvider (dbContext:MzIdentML) (item1:Provider) (item2:Provider) =
                if item1.AnalysisSoftware=item2.AnalysisSoftware && item1.ContactRole=item2.ContactRole &&
                   item1.MzIdentMLDocument=item2.MzIdentMLDocument 

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:Provider) =
                query {
                       for i in dbContext.Provider.Local do
                           if i.Name=item.Name
                              then select (i, i.AnalysisSoftware, i.ContactRole, i.MzIdentMLDocument)
                      }
                |> Seq.map (fun (provider, _, _, _) -> provider)
                |> (fun provider -> 
                    if Seq.length provider < 1 
                        then 
                            query {
                                   for i in dbContext.Provider do
                                       if i.Name=item.Name
                                          then select (i, i.AnalysisSoftware, i.ContactRole, i.MzIdentMLDocument)
                                  }
                            |> Seq.map (fun (provider, _, _, _) -> provider)
                            |> (fun provider' -> if (provider'.Count()) < 1
                                                               then let tmp = dbContext.Provider.Find(item.ID)
                                                                    if tmp.ID = item.ID 
                                                                       then item.ID <- Nullable(System.Guid.NewGuid())
                                                                            dbContext.Add item |> ignore
                                                                       else dbContext.Add item |> ignore
                                                               else provider'
                                                                    |> Seq.iter (fun providerItem -> ProviderHandler.matchAndAddProvider dbContext providerItem item)
                               )
                        else provider
                             |> Seq.iter (fun providerItem -> ProviderHandler.matchAndAddProvider dbContext providerItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:Provider) =
                ProviderHandler.matchAndAddProvider dbContext item |> ignore
                dbContext.SaveChanges()

        type MzIdentMLHandler =
            ///Initializes a mzidentml-object with at least all necessary parameters.
            static member init
                (             
                    ?inputs                         : Inputs,
                    ?version                        : string,
                    ?spectrumIdentification         : seq<SpectrumIdentification>,
                    ?spectrumIdentificationProtocol : seq<SpectrumIdentificationProtocol>,
                    ?analysisData                   : AnalysisData,
                    ?id                             : Guid,
                    ?name                           : string,
                    ?analysisSoftwares              : seq<AnalysisSoftware>,
                    ?provider                       : Provider,
                    ?persons                        : seq<Person>,
                    ?organizations                  : seq<Organization>,
                    ?samples                        : seq<Sample>,
                    ?dbSequences                    : seq<DBSequence>,
                    ?peptides                       : seq<Peptide>,
                    ?peptideEvidences               : seq<PeptideEvidence>,
                    ?proteinDetection               : ProteinDetection,
                    ?proteinDetectionProtocol       : ProteinDetectionProtocol,
                    ?biblioGraphicReferences        : seq<BiblioGraphicReference>
                ) =
                let id'                             = defaultArg id (System.Guid.NewGuid())
                let name'                           = defaultArg name Unchecked.defaultof<string>
                let version'                        = defaultArg version Unchecked.defaultof<string>
                let analysisSoftwares'              = convertOptionToList analysisSoftwares
                let provider'                       = defaultArg provider Unchecked.defaultof<Provider>
                let persons'                        = convertOptionToList persons
                let organizations'                  = convertOptionToList organizations
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
                                      Nullable(id'), 
                                      name',
                                      version',
                                      analysisSoftwares', 
                                      provider', 
                                      persons', 
                                      organizations', 
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
                    

            static member addName
                (name:string) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.Name <- name
                mzIdentML

            static member addVersion
                (version:string) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.Version <- version
                mzIdentML

            static member addAnalysisSoftware
                (analysisSoftware:AnalysisSoftware) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.AnalysisSoftwares <- addToList mzIdentML.AnalysisSoftwares analysisSoftware
                mzIdentML

            static member addAnalysisSoftwares
                (analysisSoftwares:seq<AnalysisSoftware>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.AnalysisSoftwares <- addCollectionToList mzIdentML.AnalysisSoftwares analysisSoftwares
                mzIdentML

            static member addProvider
                (provider:Provider) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.Provider <- provider
                mzIdentML

            static member addPerson
                (person:Person) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.Persons <- addToList mzIdentML.Persons person
                mzIdentML

            static member addPersons
                (persons:seq<Person>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Persons <- addCollectionToList mzIdentML.Persons persons
                mzIdentML

            static member addOrganization
                (organization:Organization) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.Organizations <- addToList mzIdentML.Organizations organization
                mzIdentML

            static member addOrganizations
                (organizations:seq<Organization>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Organizations <- addCollectionToList mzIdentML.Organizations organizations
                mzIdentML

            static member addSample
                (sample:Sample) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Samples <- addToList mzIdentML.Samples sample
                mzIdentML

            static member addSamples
                (samples:seq<Sample>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Samples <- addCollectionToList mzIdentML.Samples samples
                mzIdentML

            static member addDBSequence
                (dbSequence:DBSequence) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.DBSequences <- addToList mzIdentML.DBSequences dbSequence
                mzIdentML

            static member addDBSequences
                (dbSequences:seq<DBSequence>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.DBSequences <- addCollectionToList mzIdentML.DBSequences dbSequences
                printfn "%A" dbSequences
                mzIdentML

            static member addPeptide
                (peptide:Peptide) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Peptides <- addToList mzIdentML.Peptides peptide
                mzIdentML

            static member addPeptides
                (peptides:seq<Peptide>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.Peptides <- addCollectionToList mzIdentML.Peptides peptides
                mzIdentML

            static member addPeptideEvidence
                (peptideEvidence:PeptideEvidence) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.PeptideEvidences <- addToList mzIdentML.PeptideEvidences peptideEvidence
                mzIdentML

            static member addPeptideEvidences
                (peptideEvidences:seq<PeptideEvidence>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.PeptideEvidences <- addCollectionToList mzIdentML.PeptideEvidences peptideEvidences
                mzIdentML

            static member addSpectrumIdentification
                (spectrumIdentification:SpectrumIdentification) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.SpectrumIdentification <- addToList mzIdentML.SpectrumIdentification spectrumIdentification
                mzIdentML

            static member addSpectrumIdentifications
                (spectrumIdentification:seq<SpectrumIdentification>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.SpectrumIdentification <- addCollectionToList mzIdentML.SpectrumIdentification spectrumIdentification
                mzIdentML

            static member addProteinDetection
                (proteinDetection:ProteinDetection) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.ProteinDetection <- proteinDetection
                mzIdentML

            static member addSpectrumIdentificationProtocol
                (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.SpectrumIdentificationProtocol <- addToList mzIdentML.SpectrumIdentificationProtocol spectrumIdentificationProtocol
                mzIdentML

            static member addSpectrumIdentificationProtocols
                (spectrumIdentificationProtocol:seq<SpectrumIdentificationProtocol>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.SpectrumIdentificationProtocol <- addCollectionToList mzIdentML.SpectrumIdentificationProtocol spectrumIdentificationProtocol
                mzIdentML

            static member addProteinDetectionProtocol
                (proteinDetectionProtocol:ProteinDetectionProtocol) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.ProteinDetectionProtocol <- proteinDetectionProtocol
                mzIdentML

            static member addInputs
                (inputs:Inputs) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.Inputs <- inputs
                mzIdentML

            static member addAnalysisData
                (analysisData:AnalysisData) (mzIdentML:MzIdentMLDocument) =
                mzIdentML.AnalysisData <- analysisData
                mzIdentML

            static member addBiblioGraphicReference
                (biblioGraphicReference:BiblioGraphicReference) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.BiblioGraphicReferences <- addToList mzIdentML.BiblioGraphicReferences biblioGraphicReference
                mzIdentML

            static member addBiblioGraphicReferences
                (biblioGraphicReferences:seq<BiblioGraphicReference>) (mzIdentML:MzIdentMLDocument) =
                let result = mzIdentML.BiblioGraphicReferences <- addCollectionToList mzIdentML.BiblioGraphicReferences biblioGraphicReferences
                mzIdentML

            static member tryFindByID
                (context:MzIdentML) (mzIdentMLID:string) =
                tryFind (context.MzIdentMLDocument.Find(mzIdentMLID))

            static member private matchAndAddMzIdentMLDocument (dbContext:MzIdentML) (item1:MzIdentMLDocument) (item2:MzIdentMLDocument) =
                if item1.Name=item2.Name && item1.Version=item2.Version && item1.AnalysisSoftwares=item2.AnalysisSoftwares && 
                   item1.Provider=item2.Provider && item1.Persons=item2.Persons && item1.Organizations=item2.Organizations && 
                   item1.Samples=item2.Samples && item1.DBSequences=item2.DBSequences && item1.Peptides=item2.Peptides && 
                   item1.PeptideEvidences=item2.PeptideEvidences && item1.SpectrumIdentification=item2.SpectrumIdentification &&
                   item1.ProteinDetection=item2.ProteinDetection && item1.SpectrumIdentificationProtocol=item2.SpectrumIdentificationProtocol &&
                   item1.ProteinDetectionProtocol=item2.ProteinDetectionProtocol && item1.AnalysisData=item2.AnalysisData &&
                   item1.BiblioGraphicReferences=item2.BiblioGraphicReferences

                   then ()
                   else 
                        if item1.ID = item2.ID
                           then item2.ID <- Nullable(System.Guid.NewGuid())
                                dbContext.Add item2 |> ignore
                           else dbContext.Add item2 |> ignore

            static member addToContext (dbContext : MzIdentML) (item:MzIdentMLDocument) =
                query {
                       for i in dbContext.MzIdentMLDocument.Local do
                           if i.Inputs=item.Inputs
                              then select (i, i.Inputs, i.AnalysisSoftwares, i.Provider, i.Persons, i.Organizations, i.Samples,
                                           i.DBSequences, i.Peptides, i.PeptideEvidences, i.SpectrumIdentification, i.ProteinDetection,
                                           i.SpectrumIdentificationProtocol, i.ProteinDetectionProtocol, i.AnalysisData, i.BiblioGraphicReferences
                                          )
                      }
                |> Seq.map (fun (mzIdentMLDocument, _, _, _, _, _, _, _, _, __, _, _, _, _, _, _) -> mzIdentMLDocument)
                |> (fun mzIdentMLDocument -> 
                    if Seq.length mzIdentMLDocument < 1 
                        then 
                            query {
                                   for i in dbContext.MzIdentMLDocument do
                                       if i.Inputs=item.Inputs
                                          then select (i, i.Inputs, i.AnalysisSoftwares, i.Provider, i.Persons, i.Organizations, i.Samples,
                                                       i.DBSequences, i.Peptides, i.PeptideEvidences, i.SpectrumIdentification, i.ProteinDetection,
                                                       i.SpectrumIdentificationProtocol, i.ProteinDetectionProtocol, i.AnalysisData, i.BiblioGraphicReferences
                                                      )
                                  }
                            |> Seq.map (fun (mzIdentMLDocument, _, _, _, _, _, _, _, _, __, _, _, _, _, _, _) -> mzIdentMLDocument)
                            |> (fun mzIdentMLDocument' -> if (mzIdentMLDocument'.Count()) < 1
                                                                                 then let tmp = dbContext.MzIdentMLDocument.Find(item.ID)
                                                                                      if tmp.ID = item.ID 
                                                                                         then item.ID <- Nullable(System.Guid.NewGuid())
                                                                                              dbContext.Add item |> ignore
                                                                                         else dbContext.Add item |> ignore
                                                                                 else mzIdentMLDocument'
                                                                                      |> Seq.iter (fun mzIdentMLDocumentItem -> MzIdentMLHandler.matchAndAddMzIdentMLDocument dbContext mzIdentMLDocumentItem item)
                               )
                        else mzIdentMLDocument
                             |> Seq.iter (fun mzIdentMLDocumentItem -> MzIdentMLHandler.matchAndAddMzIdentMLDocument dbContext mzIdentMLDocumentItem item)
                   )

            static member addToContextAndInsert (dbContext : MzIdentML) (item:MzIdentMLDocument) =
                MzIdentMLHandler.matchAndAddMzIdentMLDocument dbContext item |> ignore
                dbContext.SaveChanges()