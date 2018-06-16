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

            static member addTermToContext (dbContext:MzIdentML) (item:Term) =
                query {
                    for i in dbContext.Term.Local do
                        if i.ID=item.ID || i.Name=item.Name
                           then select i
                      }
                |> (fun term -> 
                    if Seq.length term < 1 
                        then 
                            query {
                                   for i in dbContext.Term do
                                       if i.ID=item.ID || i.Name=item.Name
                                          then select i
                                  }
                            |> (fun term' -> if (term'.Count()) < 1
                                                then dbContext.Add item |> ignore
                                                else ()
                                )
                        else ()
                   )
        
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
            
            static member addOntologyToContext (dbContext:MzIdentML) (item:Ontology) =
                query {
                    for i in dbContext.Ontology.Local do
                        if i.ID=item.ID
                           then select i
                      }
                |> (fun term -> 
                    if Seq.length term < 1 
                        then 
                            query {
                                   for i in dbContext.Ontology do
                                       if i.ID=item.ID
                                          then select i
                                  }
                            |> (fun term' -> if (term'.Count()) < 1
                                                then dbContext.Add item |> ignore
                                                else ()
                                )
                        else ()
                   )

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
                tryFind (context.Iontype.Find(ionTypeID))

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
