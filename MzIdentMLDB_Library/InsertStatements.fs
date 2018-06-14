namespace MzIdentMLDataBase

module InsertStatements =

    open DataModel
    open System
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

            let addOptionToList (typeCollection : List<'a>) (optionOfType : 'a option) =
                match optionOfType with
                |Some x -> addToList typeCollection x
                |None -> typeCollection

            let addOptionCollectionToList (inputCollection : List<'a>) (input : seq<'a> option) =
                match input with
                |Some x -> addCollectionToList inputCollection x
                |None -> inputCollection  

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

             static member sqliteConnection (path:string) =
                 let optionsBuilder = 
                    new DbContextOptionsBuilder<MzIdentML>()
                 optionsBuilder.UseSqlite(@"Data Source=" + path) |> ignore
                 new MzIdentML(optionsBuilder.Options)       

             static member sqlConnection() =
                 let optionsBuilder = 
                    new DbContextOptionsBuilder<MzIdentML>()
                 optionsBuilder.UseSqlServer("Server=(localdb)\mssqllocaldb;Database=MyDatabase;Trusted_Connection=True;") |> ignore
                 new MzIdentML(optionsBuilder.Options) 

             static member fromFileObo (filePath:string) =
                 FileIO.readFile filePath
                 |> Obo.parseOboTerms

             static member addToContext (context:MzIdentML) (item:'b) =
                    context.Add(item)

             static member addToContextAndInsert (context:MzIdentML) (item:'b) =
                    context.Add(item) |> ignore
                    context.SaveChanges()

        type TermHandler =
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

                static member addName
                    (name:string) (term:Term) =
                    term.Name <- name
                    term
                    
                static member addOntology
                    (ontology:Ontology) (term:Term) =
                    term.Ontology <- ontology
                    term

                static member tryFindByID
                    (context:MzIdentML) (termID:string) =
                    context.Term.Find(termID)
        
        type OntologyHandler =
                static member init
                    (
                        id         : string,
                        ?terms     : seq<Term>
                        //?mzIdentML : MzIdentML
                    ) =
                    let terms'     = convertOptionToList terms
                    //let mzIdentML' = defaultArg mzIdentML Unchecked.defaultof<MzIdentML>

                    new Ontology(
                                 id,
                                 terms',
                                 Nullable(DateTime.Now)
                                )

                static member addTerm
                    (term:Term) (ontology:Ontology) =
                    let result = ontology.Terms <- addToList ontology.Terms term
                    ontology

                static member addTerms
                    (terms:seq<Term>) (ontology:Ontology) =
                    let result = ontology.Terms <- addCollectionToList ontology.Terms terms
                    ontology

                //static member addMzIdentML
                //    (ontology:Ontology) (mzIdentML:MzIdentML) =
                //    let result = ontology.MzIdentML <- mzIdentML
                //    ontology

                static member tryFindByID
                    (context:MzIdentML) (ontologyID:string) =
                    context.Ontology.Find(ontologyID)


                static member tryFind
                    (context:MzIdentML) (ontologyTerm:OntologyTerm) =
                    OntologyHandler.tryFindByID context (OntologyTerm.ToString  ontologyTerm)
            
        type CVParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>

                    new CVParam(
                                id', 
                                name, 
                                value', 
                                term, 
                                unit', 
                                unitName', 
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

                static member addUnitName
                    (unitName:string) (cvParam:CVParam) =
                    cvParam.UnitName <- unitName
                    cvParam

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.CVParam.Find(paramID)

        type OrganizationParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>

                    new OrganizationParam(
                                          id', 
                                          name, 
                                          value', 
                                          term, 
                                          unit', 
                                          unitName', 
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

                static member addUnitName
                    (unitName:string) (param:OrganizationParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.OrganizationParam.Find(paramID)

        type PersonParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new PersonParam(
                                    id', 
                                    name, 
                                    value', 
                                    term, 
                                    unit', 
                                    unitName', 
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

                static member addUnitName
                    (unitName:string) (param:PersonParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.PersonParam.Find(paramID)

        type SampleParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new SampleParam(
                                    id', 
                                    name, 
                                    value', 
                                    term, 
                                    unit', 
                                    unitName', 
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

                static member addUnitName
                    (unitName:string) (param:SampleParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.SampleParam.Find(paramID)

        type ModificationParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new ModificationParam(
                                          id', 
                                          name, 
                                          value', 
                                          term, 
                                          unit', 
                                          unitName', 
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

                static member addUnitName
                    (unitName:string) (param:ModificationParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.ModificationParam.Find(paramID)

        type PeptideParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new PeptideParam(
                                     id', 
                                     name, 
                                     value', 
                                     term, 
                                     unit', 
                                     unitName', 
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

                static member addUnitName
                    (unitName:string) (param:PeptideParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.PeptideParam.Find(paramID)

        type TranslationTableParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new TranslationTableParam(
                                              id', 
                                              name, 
                                              value', 
                                              term, 
                                              unit', 
                                              unitName', 
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

                static member addUnitName
                    (unitName:string) (param:TranslationTableParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.TranslationTableParam.Find(paramID)

        type MeasureParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new MeasureParam(
                                     id', 
                                     name, 
                                     value', 
                                     term, 
                                     unit', 
                                     unitName', 
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

                static member addUnitName
                    (unitName:string) (param:MeasureParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.MeasureParam.Find(paramID)

        type AmbiguousResidueParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>

                    new AmbiguousResidueParam(
                                              id', 
                                              name, 
                                              value', 
                                              term, 
                                              unit', 
                                              unitName', 
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

                static member addUnitName
                    (unitName:string) (param:AmbiguousResidueParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.AmbiguousResidueParam.Find(paramID)

        type MassTableParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new MassTableParam(
                                       id', 
                                       name,
                                       value', 
                                       term, 
                                       unit', 
                                       unitName', 
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

                static member addUnitName
                    (unitName:string) (param:MassTableParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.MassTableParam.Find(paramID)

        type IonTypeParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new IonTypeParam(
                                     id', 
                                     name, 
                                     value', 
                                     term, 
                                     unit', 
                                     unitName', 
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

                static member addUnitName
                    (unitName:string) (param:IonTypeParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.IonTypeParam.Find(paramID)

        type SpecificityRuleParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>

                    new SpecificityRuleParam(
                                             id', 
                                             name, 
                                             value', 
                                             term, 
                                             unit', 
                                             unitName', 
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

                static member addUnitName
                    (unitName:string) (param:SpecificityRuleParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.SpecificityRuleParam.Find(paramID)

        type SearchModificationParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new SearchModificationParam(
                                                id', 
                                                name, 
                                                value', 
                                                term, 
                                                unit', 
                                                unitName', 
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

                static member addUnitName
                    (unitName:string) (param:SearchModificationParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.SearchModificationParam.Find(paramID)

        type EnzymeNameParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new EnzymeNameParam(
                                        id', 
                                        name, 
                                        value', 
                                        term, 
                                        unit', 
                                        unitName', 
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

                static member addUnitName
                    (unitName:string) (param:EnzymeNameParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.EnzymeNameParam.Find(paramID)

        type IncludeParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new IncludeParam(
                                     id', 
                                     name, 
                                     value', 
                                     term, 
                                     unit', 
                                     unitName', 
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

                static member addUnitName
                    (unitName:string) (param:IncludeParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.EnzymeNameParam.Find(paramID)

        type ExcludeParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new ExcludeParam(
                                     id', 
                                     name, 
                                     value', 
                                     term, 
                                     unit', 
                                     unitName', 
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

                static member addUnitName
                    (unitName:string) (param:ExcludeParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.ExcludeParam.Find(paramID)

        type AdditionalSearchParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>

                    new AdditionalSearchParam(
                                              id', 
                                              name, 
                                              value', 
                                              term, 
                                              unit', 
                                              unitName', 
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

                static member addUnitName
                    (unitName:string) (param:AdditionalSearchParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.AdditionalSearchParam.Find(paramID)

        type FragmentToleranceParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new FragmentToleranceParam(
                                               id', 
                                               name, 
                                               value', 
                                               term, 
                                               unit', 
                                               unitName',
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

                static member addUnitName
                    (unitName:string) (param:FragmentToleranceParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.FragmentToleranceParam.Find(paramID)

        type ParentToleranceParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new ParentToleranceParam(
                                             id', 
                                             name, 
                                             value', 
                                             term, 
                                             unit', 
                                             unitName', 
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

                static member addUnitName
                    (unitName:string) (param:ParentToleranceParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.ParentToleranceParam.Find(paramID)

        type ThresholdParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new ThresholdParam(
                                       id', 
                                       name, 
                                       value', 
                                       term, 
                                       unit', 
                                       unitName', 
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

                static member addUnitName
                    (unitName:string) (param:ThresholdParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.ThresholdParam.Find(paramID)

        type SearchDatabaseParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new SearchDatabaseParam(
                                            id', 
                                            name, 
                                            value', 
                                            term, 
                                            unit', 
                                            unitName', 
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

                static member addUnitName
                    (unitName:string) (param:SearchDatabaseParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.SearchDatabaseParam.Find(paramID)

        type DBSequenceParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new DBSequenceParam(
                                        id',
                                        name, 
                                        value', 
                                        term, 
                                        unit', 
                                        unitName', 
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

                static member addUnitName
                    (unitName:string) (param:DBSequenceParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.DBSequenceParam.Find(paramID)

        type PeptideEvidenceParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new PeptideEvidenceParam(
                                             id',
                                             name, 
                                             value', 
                                             term, 
                                             unit', 
                                             unitName', 
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

                static member addUnitName
                    (unitName:string) (param:PeptideEvidenceParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.PeptideEvidenceParam.Find(paramID)

        type SpectrumIdentificationItemParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>

                    new SpectrumIdentificationItemParam(
                                                        id', 
                                                        name, 
                                                        value', 
                                                        term, 
                                                        unit', 
                                                        unitName', 
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

                static member addUnitName
                    (unitName:string) (param:SpectrumIdentificationItemParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.SpectrumIdentificationItemParam.Find(paramID)

        type SpectrumIdentificationResultParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>

                    new SpectrumIdentificationResultParam(
                                                          id', 
                                                          name, 
                                                          value', 
                                                          term, 
                                                          unit', 
                                                          unitName', 
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

                static member addUnitName
                    (unitName:string) (param:SpectrumIdentificationResultParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.SpectrumIdentificationResultParam.Find(paramID)

        type SpectrumIdentificationListParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>

                    new SpectrumIdentificationListParam(
                                                        id', 
                                                        name, 
                                                        value', 
                                                        term, 
                                                        unit', 
                                                        unitName', 
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

                static member addUnitName
                    (unitName:string) (param:SpectrumIdentificationListParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.SpectrumIdentificationListParam.Find(paramID)

        type AnalysisParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new AnalysisParam(
                                      id', 
                                      name, 
                                      value', 
                                      term, 
                                      unit', 
                                      unitName', 
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

                static member addUnitName
                    (unitName:string) (param:AnalysisParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.AnalysisParam.Find(paramID)

        type SourceFileParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new SourceFileParam(
                                        id', 
                                        name, 
                                        value', 
                                        term, 
                                        unit', 
                                        unitName',
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

                static member addUnitName
                    (unitName:string) (param:SourceFileParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.SourceFileParam.Find(paramID)

        type ProteinDetectionHypothesisParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>
                    
                    new ProteinDetectionHypothesisParam(
                                                        id', 
                                                        name, 
                                                        value', 
                                                        term, 
                                                        unit', 
                                                        unitName', 
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

                static member addUnitName
                    (unitName:string) (param:ProteinDetectionHypothesisParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.ProteinDetectionHypothesisParam.Find(paramID)

        type ProteinAmbiguityGroupParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>

                    new  ProteinAmbiguityGroupParam(
                                                    id', 
                                                    name, 
                                                    value', 
                                                    term, 
                                                    unit', 
                                                    unitName', 
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

                static member addUnitName
                    (unitName:string) (param:ProteinAmbiguityGroupParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.ProteinAmbiguityGroupParam.Find(paramID)

        type ProteinDetectionListParamHandler =

                static member init
                    (
                        name      : string,
                        term      : Term,
                        ?id       : string,
                        ?value    : string,
                        ?unit     : Term,
                        ?unitName : string
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let value'    = defaultArg value Unchecked.defaultof<string>
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName Unchecked.defaultof<string>

                    new ProteinDetectionListParam(
                                                  id',
                                                  name, 
                                                  value', 
                                                  term, 
                                                  unit', 
                                                  unitName', 
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

                static member addUnitName
                    (unitName:string) (param:ProteinDetectionListParam) =
                    param.UnitName <- unitName
                    param

                static member tryFindByID
                    (context:MzIdentML) (paramID:string) =
                    context.ProteinDetectionListParam.Find(paramID)

        type OrganizationHandler =
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
                    context.Organization.Find(organizationID)

        type PersonHandler =
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
                    let id'          = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'        = defaultArg name Unchecked.defaultof<string>
                    let firstName'   = defaultArg firstName Unchecked.defaultof<string>
                    let midInitials' = defaultArg midInitials Unchecked.defaultof<string>
                    let lastName'    = defaultArg lastName Unchecked.defaultof<string>
                    
                    new Person(
                               id', 
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
                    context.Person.Find(personID)

        type ContactRoleHandler =
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

                static member tryFindByID
                    (context:MzIdentML) (contactRoleID:string) =
                    context.ContactRole.Find(contactRoleID)

        type AnalysisSoftwareHandler =
                static member init
                    (
                        softwareName       : CVParam,
                        ?id                : string,
                        ?name              : string,
                        ?uri               : string,
                        ?version           : string,
                        ?customizations    : string,
                        ?softwareDeveloper : ContactRole,
                        ?mzIdentML         : MzIdentMLDocument
                    ) =
                    let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'           = defaultArg name Unchecked.defaultof<string>
                    let uri'            = defaultArg uri Unchecked.defaultof<string>
                    let version'        = defaultArg version Unchecked.defaultof<string>
                    let customizations' = defaultArg customizations Unchecked.defaultof<string>
                    let contactRole'    = defaultArg softwareDeveloper Unchecked.defaultof<ContactRole>
                    let mzIdentML'      = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                    new AnalysisSoftware(
                                         id', 
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
                    context.AnalysisSoftware.Find(analysisSoftwareID)

        type SubSampleHandler =
                static member init
                    (
                        ?id          : string,
                        ?sample      : Sample
                    ) =
                    let id'          = defaultArg id (System.Guid.NewGuid().ToString())
                    let Sample'      = defaultArg sample Unchecked.defaultof<Sample>
                    
                    new SubSample(
                                  id', 
                                  Sample', 
                                  Nullable(DateTime.Now)
                                 )

                static member addSample
                    (sampleID:Sample) (subSample:SubSample) =
                    subSample.Sample <- sampleID
                    subSample

                static member tryFindByID
                    (context:MzIdentML) (subSampleID:string) =
                    context.SubSample.Find(subSampleID)

        type SampleHandler =
                static member init
                    (
                        ?id           : string,
                        ?name         : string,
                        ?contactRoles : seq<ContactRole>,
                        ?subSamples   : seq<SubSample>,
                        ?details      : seq<SampleParam>,
                        ?mzIdentML    : MzIdentMLDocument
                    ) =
                    let id'           = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'         = defaultArg name Unchecked.defaultof<string>
                    let contactRoles' = convertOptionToList contactRoles
                    let subSamples'   = convertOptionToList subSamples
                    let details'      = convertOptionToList details
                    let mzIdentML'    = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                    new Sample(
                               id', 
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
                    context.Sample.Find(contactRolesID)

        type ModificationHandler =
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
                    context.Modification.Find(modificationID)

        type SubstitutionModificationHandler =
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
                    context.SubstitutionModification.Find(substitutionModificationID)

        type PeptideHandler =
                static member init
                    (
                        peptideSequence            : string,
                        ?id                        : string,
                        ?name                      : string,                    
                        ?modifications             : seq<Modification>,
                        ?substitutionModifications : seq<SubstitutionModification>,
                        ?details                   : seq<PeptideParam>,
                        ?mzIdentML                 : MzIdentMLDocument
                    ) =
                    let id'                        = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'                      = defaultArg name Unchecked.defaultof<string>
                    let modifications'             = convertOptionToList modifications
                    let substitutionModifications' = convertOptionToList substitutionModifications
                    let details'                   = convertOptionToList details
                    let mzIdentML'                 = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                    new Peptide(
                                id', 
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
                    context.Peptide.Find(peptideID)

        type TranslationTableHandler =
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
                    context.TranslationTable.Find(translationTableID)

        type MeasureHandler =
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

                static member addName
                    (name:string) (measure:Measure) =
                    measure.Name <- name
                    measure

                static member tryFindByID
                    (context:MzIdentML) (measureID:string) =
                    context.Measure.Find(measureID)

        type ResidueHandler =
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

                static member findResidueByID
                    (context:MzIdentML) (residueID:string) =
                    context.Residue.Find(residueID)

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

                static member tryFindByID
                    (context:MzIdentML) (ambiguousResidueID:string) =
                    context.AmbiguousResidue.Find(ambiguousResidueID)

        type MassTableHandler =
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
                    context.MassTable.Find(massTableID)

        type ValueHandler =
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

                static member findValueByID
                    (context:MzIdentML) (valueID:string) =
                    context.Value.Find(valueID)

        type FragmentArrayHandler =
                static member init
                    (
                        measure : Measure,
                        values  : seq<Value>,
                        ?id     : string
                    ) =
                    let id' = defaultArg id (System.Guid.NewGuid().ToString())
                    
                    new FragmentArray(
                                      id', 
                                      measure, 
                                      values |> List, 
                                      Nullable(DateTime.Now)
                                     )

                static member tryFindByID
                    (context:MzIdentML) (fragmentArrayID:string) =
                    context.FragmentArray.Find(fragmentArrayID)

        type IndexHandler =
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

                static member tryFindByID
                    (context:MzIdentML) (indexID:string) =
                    context.Index.Find(indexID)

        type IonTypeHandler =
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
                    context.Iontype.Find(ionTypeID)

        type SpectraDataHandler =
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
                    context.SpectraData.Find(spectraDataID)

        type SpecificityRulesHandler =
                static member init
                    ( 
                        details    : seq<SpecificityRuleParam>,
                        ?id        : string
                    ) =
                    let id' = defaultArg id (System.Guid.NewGuid().ToString())
                    
                    new SpecificityRule(
                                        id', 
                                        details |> List, 
                                        Nullable(DateTime.Now)
                                       )

                static member tryFindByID
                    (context:MzIdentML) (specificityRuleID:string) =
                    context.SpecificityRule.Find(specificityRuleID)

        type SearchModificationHandler =
                static member init
                    ( 
                        fixedMod          : bool,
                        massDelta         : float,
                        residues          : string,
                        details           : seq<SearchModificationParam>,
                        ?id               : string,
                        ?specificityRules : seq<SpecificityRule>
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
                    context.SearchModification.Find(searchModificationID)

        type EnzymeHandler =
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
                    context.Enzyme.Find(enzymeID)

        type FilterHandler =
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
                    context.Filter.Find(filterID)

        type FrameHandler =
                static member init
                    ( 
                        frame : int,
                        ?id   : string
                    ) =
                    let id'   = defaultArg id (System.Guid.NewGuid().ToString())
                    
                    new Frame(id', Nullable(frame), Nullable(DateTime.Now))

                static member findFrameByID
                    (context:MzIdentML) (frameID:string) =
                    context.Frame.Find(frameID)

        type SpectrumIdentificationProtocolHandler =
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
                        ?mzIdentML              : MzIdentMLDocument
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
                    let mzIdentML'              = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
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
                    context.SpectrumIdentificationProtocol.Find(spectrumIdentificationProtocolID)

        type SearchDatabaseHandler =
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
                    context.SearchDatabase.Find(searchDatabaseID)

        type DBSequenceHandler =
                static member init
                    (
                        accession      : string,
                        searchDatabase : SearchDatabase,
                        ?id            : string,
                        ?name          : string,
                        ?sequence      : string,
                        ?length        : int,
                        ?details       : seq<DBSequenceParam>,
                        ?mzIdentML     : MzIdentMLDocument
                    ) =
                    let id'        = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'      = defaultArg name Unchecked.defaultof<string>
                    let sequence'  = defaultArg sequence Unchecked.defaultof<string>
                    let length'    = defaultArg length Unchecked.defaultof<int>
                    let details'   = convertOptionToList details
                    let mzIdentML' = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                    new DBSequence(
                                   id', 
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
                    context.DBSequence.Find(dbSequenceID)

        type PeptideEvidenceHandler =
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
                        ?mzIdentML                  : MzIdentMLDocument
                    ) =
                    let id'                         = defaultArg id (System.Guid.NewGuid().ToString())
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
                    context.PeptideEvidence.Find(peptideEvidenceID)

        type SpectrumIdentificationItemHandler =
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
                    context.SpectrumIdentificationItem.Find(spectrumIdentificationItemID)

        type SpectrumIdentificationResultHandler =
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
                    context.SpectrumIdentificationResult.Find(spectrumIdentificationResultID)

        type SpectrumIdentificationListHandler =
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
                    context.SpectrumIdentificationList.Find(spectrumIdentificationListID)

        type SpectrumIdentificationHandler =
                static member init
                    (
                        spectrumIdentificationList     : SpectrumIdentificationList,
                        spectrumIdentificationProtocol : SpectrumIdentificationProtocol,
                        spectraData                    : seq<SpectraData>,
                        searchDatabase                 : seq<SearchDatabase>,
                        ?id                            : string,
                        ?name                          : string,
                        ?activityDate                  : DateTime,
                        ?mzIdentML                     : MzIdentMLDocument
                    ) =
                    let id'               = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'             = defaultArg name Unchecked.defaultof<string>
                    let activityDate'     = defaultArg activityDate Unchecked.defaultof<DateTime>
                    let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                    new SpectrumIdentification(
                                               id', 
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
                    context.SpectrumIdentification.Find(spectrumIdentificationID)

        type ProteinDetectionProtocolHandler =
                static member init
                    (
                        analysisSoftware : AnalysisSoftware,
                        threshold        : seq<ThresholdParam>,
                        ?id              : string,
                        ?name            : string,
                        ?analysisParams  : seq<AnalysisParam>,
                        ?mzIdentML       : MzIdentMLDocument
                    ) =
                    let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'           = defaultArg name Unchecked.defaultof<string>
                    let analysisParams' = convertOptionToList analysisParams
                    let mzIdentML'      = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                    new ProteinDetectionProtocol(
                                                 id', 
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
                    context.ProteinDetectionProtocol.Find(proteinDetectionProtocolID)

        type SourceFileHandler =
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
                    context.SourceFile.Find(sourceFileID)

        type InputsHandler =
                static member init
                    (              
                        spectraData     : seq<SpectraData>,
                        ?id             : string,
                        ?sourceFile     : seq<SourceFile>,
                        ?searchDatabase : seq<SearchDatabase>,
                        ?mzIdentML      : MzIdentMLDocument
                    ) =
                    let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                    let sourceFile'     = convertOptionToList sourceFile
                    let searchDatabase' = convertOptionToList searchDatabase
                    let mzIdentML'      = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                    new Inputs(
                               id', 
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
                    context.Inputs.Find(inputsID)

        type PeptideHypothesisHandler =
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

                static member tryFindByID
                    (context:MzIdentML) (peptideHypothesisID:string) =
                    context.PeptideHypothesis.Find(peptideHypothesisID)

        type ProteinDetectionHypothesisHandler =
                static member init
                    (             
                        passThreshold     : bool,
                        dbSequence        : DBSequence,
                        peptideHypothesis : seq<PeptideHypothesis>,
                        ?id               : string,
                        ?name             : string,
                        ?details          : seq<ProteinDetectionHypothesisParam>,
                        ?mzIdentML        : MzIdentMLDocument
                    ) =
                    let id'        = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'      = defaultArg name Unchecked.defaultof<string>
                    let details'   = convertOptionToList details
                    let mzIdentML' = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                    new ProteinDetectionHypothesis(
                                                   id', 
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
                    context.ProteinDetectionHypothesis.Find(proteinDetectionHypothesisID)

        type ProteinAmbiguityGroupHandler =
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
                    context.ProteinAmbiguityGroup.Find(proteinAmbiguityGroupID)

        type ProteinDetectionListHandler =
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
                    context.ProteinDetectionList.Find(proteinDetectionListID)

        type AnalysisDataHandler =
                static member init
                    (             
                        spectrumIdentificationList : seq<SpectrumIdentificationList>,
                        ?id                        : string,
                        ?proteinDetectionList      : ProteinDetectionList,
                        ?mzIdentML                 : MzIdentMLDocument
                    ) =
                    let id'                   = defaultArg id (System.Guid.NewGuid().ToString())
                    let proteinDetectionList' = defaultArg proteinDetectionList Unchecked.defaultof<ProteinDetectionList>
                    let mzIdentML'            = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>
                    
                    new AnalysisData(
                                     id', 
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
                    context.AnalysisData.Find(analysisDataID)

        type ProteinDetectionHandler =
                static member init
                    (             
                        proteinDetectionList        : ProteinDetectionList,
                        proteinDetectionProtocol    : ProteinDetectionProtocol,
                        spectrumIdentificationLists : seq<SpectrumIdentificationList>,
                        ?id                         : string,
                        ?name                       : string,
                        ?activityDate               : DateTime
                    ) =
                    let id'           = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'         = defaultArg name Unchecked.defaultof<string>
                    let activityDate' = defaultArg activityDate Unchecked.defaultof<DateTime>
                    
                    new ProteinDetection(
                                         id', 
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
                    context.ProteinDetection.Find(proteinDetectionID)

        type BiblioGraphicReferenceHandler =
                static member init
                    (             
                        ?id          : string,
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
                    let id'          = defaultArg id (System.Guid.NewGuid().ToString())
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
                    context.BiblioGraphicReference.Find(biblioGraphicReferenceID)

        type ProviderHandler =
                static member init
                    (             
                        ?id               : string,
                        ?name             : string,
                        ?analysisSoftware : AnalysisSoftware,
                        ?contactRole      : ContactRole,
                        ?mzIdentML        : MzIdentMLDocument
                    ) =
                    let id'               = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'             = defaultArg name Unchecked.defaultof<string>
                    let analysisSoftware' = defaultArg analysisSoftware Unchecked.defaultof<AnalysisSoftware>
                    let contactRole'      = defaultArg contactRole Unchecked.defaultof<ContactRole>
                    let mzIdentML'        = defaultArg mzIdentML Unchecked.defaultof<MzIdentMLDocument>

                    new Provider(
                                 id', 
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
                    context.Provider.Find(providerID)

        type MzIdentMLHandler =
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
                    let id'                             = defaultArg id (System.Guid.NewGuid().ToString())
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
                                          id', 
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
                    context.MzIdentMLDocument.Find(mzIdentMLID)
