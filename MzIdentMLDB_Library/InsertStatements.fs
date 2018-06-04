﻿namespace MzIdentMLDataBase

module InsertStatements =

    open DataContext
    open DataContext.EntityTypes
    open DataContext.DataContext
    open System
    //open System.ComponentModel.DataAnnotations
    //open System.ComponentModel.DataAnnotations.Schema
    open Microsoft.EntityFrameworkCore
    open System.Collections.Generic
    //open FSharp.Care.IO
    //open BioFSharp.IO



    module SubFunctions =

        //ObjectHandlers

        let convertOptionToList (optionOfType : seq<'a> option) =
            match optionOfType with
            | Some x -> x |> List
            | None -> null

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
  
    module ManipulateDataContextAndDB =

        let addToContextWithExceptionCheck (context : MzIdentMLContext) (item : 'a) =
            try
                context.Add(item) |> ignore
                true
            with
                | :? System.InvalidOperationException as text ->
                     printfn "%s" text.Message
                     false
                | :? System .NullReferenceException as text ->
                     printfn "%s" text.Message
                     false
                | _ -> false

        let insertWithExceptionCheck (context : MzIdentMLContext) =
            try
                context.SaveChanges()
                |> (fun i -> printfn "Added %i elements to the DB" i)
                true
            with
                | :? Microsoft.EntityFrameworkCore.DbUpdateException as text ->
                     printfn "%s" text.Message
                     false
                |_ ->
                   false

        let fileDir = __SOURCE_DIRECTORY__
        let standardDBPathSQLite = fileDir + "\Databases\Test.db"

        let configureSQLiteContextMzIdentML path = 
            let optionsBuilder = new DbContextOptionsBuilder<MzIdentMLContext>()
            optionsBuilder.UseSqlite(@"Data Source=" + path) |> ignore
            new MzIdentMLContext(optionsBuilder.Options)

    module ObjectHandlers =

        open ManipulateDataContextAndDB
        open SubFunctions

        let setOption (addFunction:'a->'b->'a) (object:'a) (item:'b option) =
            match item with
            |Some x -> addFunction object x
            |None -> object

        type TermHandler =
               static member init
                    (
                        id        : string,
                        ?name     : string,
                        ?ontology : Ontology  
                    ) =
                    let name'      = defaultArg name null
                    let ontology'  = defaultArg ontology Unchecked.defaultof<Ontology>
                    {
                        ID         = id;
                        Name       = name';
                        Ontology   = ontology';
                        RowVersion = DateTime.Now
                    }

               static member addName
                    (term:Term) (name:string) =
                    term.Name <- name
                    term
                    
               static member addOntology
                    (term:Term) (ontology:Ontology) =
                    term.Ontology <- ontology
                    term

               static member findTermByID
                    (context:MzIdentMLContext) (termID:string) =
                    context.Term.Find(termID)

               static member addToContext (context:MzIdentMLContext) (item:Term) =
                        addToContextWithExceptionCheck context item

               static member addAndInsert (context:MzIdentMLContext) (item:Term) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context
        
        type OntologyHandler =
               static member init
                    (
                        id         : string,
                        ?terms     : seq<Term>
                        //?mzIdentML : MzIdentML
                    ) =
                    let terms'     = convertOptionToList terms
                    //let mzIdentML' = defaultArg mzIdentML Unchecked.defaultof<MzIdentML>
                    {
                        Ontology.ID         = id;
                        Ontology.Terms      = terms';
                        Ontology.RowVersion = DateTime.Now
                        //Ontology.MzIdentML  = mzIdentML'
                    }

                static member addTerm
                    (ontology:Ontology) (term:Term) =
                    let result = ontology.Terms <- addToList ontology.Terms term
                    ontology

                static member addTerms
                    (ontology:Ontology) (terms:seq<Term>) =
                    let result = ontology.Terms <- addCollectionToList ontology.Terms terms
                    ontology

                //static member addMzIdentML
                //    (ontology:Ontology) (mzIdentML:MzIdentML) =
                //    let result = ontology.MzIdentML <- mzIdentML
                //    ontology

               static member findOntologyByID
                    (context:MzIdentMLContext) (ontologyID:string) =
                    context.Ontology.Find(ontologyID)

                static member findTermByID
                    (context:MzIdentMLContext) (termID:string) =
                    context.Term.Find(termID)

                static member addToContext (context:MzIdentMLContext) (item:Ontology) =
                    addToContextWithExceptionCheck context item

                static member insertIntoDB (context:MzIdentMLContext) (item:Ontology) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context
            
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
                    let value'    = defaultArg value null
                    let unit'     = defaultArg unit Unchecked.defaultof<Term>
                    let unitName' = defaultArg unitName null
                    {
                        ID         = id';
                        Name       = name;
                        Value      = value';
                        Term       = term;
                        Unit       = unit';
                        UnitName   = unitName';
                        RowVersion = DateTime.Now
                    }
               static member addValue
                    (cvParam:CVParam) (value:string) =
                    cvParam.Value <- value
                    cvParam

               static member addUnit
                    (cvParam:CVParam) (unit:Term) =
                    cvParam.Unit <- unit
                    cvParam

               static member addUnitName
                    (cvParam:CVParam) (unitName:string) =
                    cvParam.UnitName <- unitName
                    cvParam

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member findTermByID
                    (context:MzIdentMLContext) (termID:string) =
                    context.Term.Find(termID)

               static member addToContext (context:MzIdentMLContext) (item:CVParam) =
                        (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:CVParam) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type OrganizationHandler =
               static member init
                    (
                        ?id      : string,
                        ?name    : string,
                        ?details : seq<CVParam>,
                        ?parent  : string
                    ) =
                    let id'      = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'    = defaultArg name null
                    let details' = convertOptionToList details
                    let parent'  = defaultArg parent null
                    {
                        Organization.ID         = id';
                        Organization.Name       = name';
                        Organization.Details    = details';
                        Organization.Parent     = parent';
                        Organization.RowVersion = DateTime.Now
                    }

               static member addName
                    (organization:Organization) (name:string) =
                    organization.Name <- name
                    organization

               static member addParent
                    (organization:Organization) (parent:string) =
                    organization.Parent <- parent
                    organization

               static member addDetail
                    (organization:Organization) (detail:CVParam) =
                    let result = organization.Details <- addToList organization.Details detail
                    organization

               static member addDetails
                    (organization:Organization) (details:seq<CVParam>) =
                    let result = organization.Details <- addCollectionToList organization.Details details
                    organization

               static member findOrganizationByID
                    (context:MzIdentMLContext) (organizationID:string) =
                    context.Organization.Find(organizationID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:Organization) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:Organization) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type PersonHandler =
               static member init
                    (
                        ?id             : string,
                        ?name           : string,
                        ?firstName      : string,
                        ?midInitials    : string,
                        ?lastName       : string,
                        ?contactDetails : seq<CVParam>,
                        ?organizations  : seq<Organization> 
                    ) =
                    let id'          = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'        = defaultArg name null
                    let firstName'   = defaultArg firstName null
                    let midInitials' = defaultArg midInitials null
                    let lastName'    = defaultArg lastName null
                    {
                        Person.ID            = id';
                        Person.Name          = name';
                        Person.FirstName     = firstName';
                        Person.MidInitials   = midInitials';
                        Person.LastName      = lastName';
                        Person.Details       = convertOptionToList contactDetails;
                        Person.Organizations = convertOptionToList organizations;
                        Person.RowVersion    = DateTime.Now
                    }

               static member addName
                    (person:Person) (name:string) =
                    person.Name <- name
                    person

               static member addFirstName
                    (person:Person) (firstName:string) =
                    person.FirstName <- firstName
                    person

               static member addMidInitials
                    (person:Person) (midInitials:string) =
                    person.MidInitials <- midInitials
                    person

               static member addLastName
                    (person:Person) (lastName:string) =
                    person.LastName <- lastName
                    person

               static member addDetail (person:Person) (detail:CVParam) =
                    let result = person.Details <- addToList person.Details detail
                    person

               static member addDetails
                    (person:Person) (details:seq<CVParam>) =
                    let result = person.Details <- addCollectionToList person.Details details
                    person

               static member addOrganization
                    (person:Person) (organization:Organization) =
                    let result = person.Organizations <- addToList person.Organizations organization
                    person

               static member addOrganizations
                    (person:Person) (organizations:seq<Organization>) =
                    let result = person.Organizations <- addCollectionToList person.Organizations organizations
                    person

               static member findPersonByID
                    (context:MzIdentMLContext) (personID:string) =
                    context.Person.Find(personID)

               static member findOrganizationByID
                    (context:MzIdentMLContext) (organizationID:string) =
                    context.Organization.Find(organizationID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:Person) =
                        (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:Person) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type ContactRoleHandler =
               static member init
                    (   
                        person : Person, 
                        role   : CVParam,
                        ?id    : string
                    ) =
                    let id' = defaultArg id (System.Guid.NewGuid().ToString())
                    {
                         ContactRole.ID         = id'
                         ContactRole.Person     = person
                         ContactRole.Role       = role
                         ContactRole.RowVersion = DateTime.Now.Date
                    }

               static member findContactRoleByID
                    (context:MzIdentMLContext) (contactRoleID:string) =
                    context.ContactRole.Find(contactRoleID)

               static member findPersonByID
                    (context:MzIdentMLContext) (personID:string) =
                    context.Person.Find(personID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:ContactRole) =
                        (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:ContactRole) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type AnalysisSoftwareHandler =
               static member init
                    (
                        softwareName       : CVParam,
                        ?id                : string,
                        ?name              : string,
                        ?uri               : string,
                        ?version           : string,
                        ?customizations    : string,
                        ?softwareDeveloper : ContactRole
                        ////?mzIdentML         : MzIdentML
                    ) =
                    let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'           = defaultArg name null
                    let uri'            = defaultArg uri null
                    let version'        = defaultArg version null
                    let customizations' = defaultArg customizations null
                    let contactRole'    = defaultArg softwareDeveloper Unchecked.defaultof<ContactRole>
                    ////let mzIdentML'      = defaultArg mzIdentML Unchecked.defaultof<MzIdentML>
                    {
                        AnalysisSoftware.ID             = id';
                        AnalysisSoftware.Name           = name';
                        AnalysisSoftware.URI            = uri';
                        AnalysisSoftware.Version        = version';
                        AnalysisSoftware.Customizations = customizations';
                        AnalysisSoftware.ContactRole    = contactRole';
                        AnalysisSoftware.SoftwareName   = softwareName;
                        AnalysisSoftware.RowVersion     = DateTime.Now
                    }
               static member addName
                    (analysisSoftware:AnalysisSoftware) (name:string) =
                    analysisSoftware.Name <- name
                    analysisSoftware

               static member addURI
                    (analysisSoftware:AnalysisSoftware) (uri:string) =
                    analysisSoftware.URI <- uri
                    analysisSoftware

               static member addVersion
                    (analysisSoftware:AnalysisSoftware) (version:string) =
                    analysisSoftware.Version <- version
                    analysisSoftware

               static member addCustomization
                    (analysisSoftware:AnalysisSoftware) (customizations:string) =
                    analysisSoftware.Customizations <- customizations
                    analysisSoftware

               static member addAnalysisSoftwareDeveloper
                    (analysisSoftware:AnalysisSoftware) (analysisSoftwareDeveloper:ContactRole) =
                    analysisSoftware.ContactRole <- analysisSoftwareDeveloper
                    analysisSoftware

               //static member addMzIdentML
               //     (analysisSoftware:AnalysisSoftware) (mzIdentML:MzIdentML) =
               //     analysisSoftware.MzIdentML <- mzIdentML
               //     analysisSoftware

               static member findAnalysisSoftwareByID
                    (context:MzIdentMLContext) (analysisSoftwareID:string) =
                    context.AnalysisSoftware.Find(analysisSoftwareID)

               static member findContactRoleByID
                    (context:MzIdentMLContext) (contactRoleID:string) =
                    context.ContactRole.Find(contactRoleID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:AnalysisSoftware) =
                        (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:AnalysisSoftware) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type SubSampleHandler =
               static member init
                    (
                        ?id          : string,
                        ?sample      : Sample
                    ) =
                    let id'          = defaultArg id (System.Guid.NewGuid().ToString())
                    let Sample'      = defaultArg sample Unchecked.defaultof<Sample>
                    {
                        SubSample.ID          = id';
                        SubSample.Sample      = Sample';
                        SubSample.RowVersion  = DateTime.Now    
                    }

               static member addSample
                    (subSample:SubSample) (sampleID:Sample) =
                    subSample.Sample <- sampleID
                    subSample

               static member findSubSampleByID
                    (context:MzIdentMLContext) (subSampleID:string) =
                    context.SubSample.Find(subSampleID)

               static member findSampleByID
                    (context:MzIdentMLContext) (sampleID:string) =
                    context.Sample.Find(sampleID)

               static member addToContext (context:MzIdentMLContext) (item:SubSample) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:SubSample) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type SampleHandler =
               static member init
                    (
                        ?id           : string,
                        ?name         : string,
                        ?contactRoles : seq<ContactRole>,
                        ?subSamples   : seq<SubSample>,
                        ?details      : seq<CVParam>
                        //?mzIdentML    : MzIdentML
                    ) =
                    let id'           = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'         = defaultArg name null
                    let contactRoles' = convertOptionToList contactRoles
                    let subSamples'   = convertOptionToList subSamples
                    let details'      = convertOptionToList details
                    //let mzIdentML'    = defaultArg mzIdentML Unchecked.defaultof<MzIdentML>
                    {
                        Sample.ID           = id'
                        Sample.Name         = name'
                        Sample.ContactRoles = contactRoles'
                        Sample.SubSamples   = subSamples'
                        Sample.Details      = details'
                        Sample.RowVersion   = DateTime.Now
                    }

               static member addName
                    (sample:Sample) (name:string) =
                    sample.Name <- name
                    sample

               static member addContactRole
                    (sample:Sample) (contactRole:ContactRole) =
                    let result = sample.ContactRoles <- addToList sample.ContactRoles contactRole
                    sample

               static member addContactRoles
                    (sample:Sample) (contactRoles:seq<ContactRole>) =
                    let result = sample.ContactRoles <- addCollectionToList sample.ContactRoles contactRoles
                    sample

               static member addSubSample
                    (sample:Sample) (subSample:SubSample) =
                    let result = sample.SubSamples <- addToList sample.SubSamples subSample
                    sample

               static member addSubSamples
                    (sample:Sample) (subSamples:seq<SubSample>) =
                    let result = sample.SubSamples <- addCollectionToList sample.SubSamples subSamples
                    sample

               static member addDetail
                    (sample:Sample) (detail:CVParam) =
                    let result = sample.Details <- addToList sample.Details detail
                    sample

               static member addDetails
                    (sample:Sample) (details:seq<CVParam>) =
                    let result = sample.Details <- addCollectionToList sample.Details details
                    sample

               static member findContactRolesByID
                    (context:MzIdentMLContext) (contactRolesID:string) =
                    context.ContactRole.Find(contactRolesID)

               static member findSubSampleID
                    (context:MzIdentMLContext) (subSampleID:string) =
                    context.SubSample.Find(subSampleID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:Sample) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:Sample) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type ModificationHandler =
               static member init
                    (
                        details                : seq<CVParam>,
                        ?id                    : string,
                        ?residues              : string,
                        ?location              : int,
                        ?monoIsotopicMassDelta : float,
                        ?avgMassDelta          : float
                    ) =
                    let id'               = defaultArg id (System.Guid.NewGuid().ToString())
                    let residues'         = defaultArg residues null
                    let location'         = defaultArg location Unchecked.defaultof<int>
                    let monoIsotopicMassDelta' = defaultArg monoIsotopicMassDelta Unchecked.defaultof<float>
                    let avgMassDelta' = defaultArg avgMassDelta Unchecked.defaultof<float>
                    {
                        Modification.ID                    = id'
                        Modification.Details               = details |> List
                        Modification.Residues              = residues'
                        Modification.Location              = location'
                        Modification.MonoIsotopicMassDelta = monoIsotopicMassDelta'
                        Modification.AvgMassDelta          = avgMassDelta'
                        Modification.RowVersion            = DateTime.Now
                    }

               static member addResidues
                    (modification:Modification) (residues:string) =
                    modification.Residues <- residues
                    modification

               static member addLocation
                    (modification:Modification) (location:int) =
                    modification.Location <- location
                    modification

               static member addMonoIsotopicMassDelta
                    (modification:Modification) (monoIsotopicMassDelta:float) =
                    modification.MonoIsotopicMassDelta <- monoIsotopicMassDelta
                    modification

               static member addAvgMassDelta
                    (modification:Modification) (avgMassDelta:float) =
                    modification.AvgMassDelta <- avgMassDelta
                    modification

               static member findModificationByID
                    (context:MzIdentMLContext) (modificationID:string) =
                    context.Modification.Find(modificationID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:Modification) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:Modification) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

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
                    {
                        SubstitutionModification.ID                    = id'
                        SubstitutionModification.OriginalResidue       = originalResidue
                        SubstitutionModification.ReplacementResidue    = replacementResidue
                        SubstitutionModification.Location              = location'
                        SubstitutionModification.MonoIsotopicMassDelta = monoIsotopicMassDelta'
                        SubstitutionModification.AvgMassDelta          = avgMassDelta'
                        SubstitutionModification.RowVersion            = DateTime.Now
                    }

               static member addLocation
                    (substitutionModification:SubstitutionModification) (location:int) =
                    substitutionModification.Location <- location
                    substitutionModification

               static member addMonoIsotopicMassDelta
                    (substitutionModification:SubstitutionModification) (monoIsotopicMassDelta:float) =
                    substitutionModification.MonoIsotopicMassDelta <- monoIsotopicMassDelta
                    substitutionModification

               static member addAvgMassDelta
                    (substitutionModification:SubstitutionModification) (avgMassDelta:float) =
                    substitutionModification.AvgMassDelta <- avgMassDelta
                    substitutionModification

               static member findSubstitutionModificationByID
                    (context:MzIdentMLContext) (substitutionModificationID:string) =
                    context.SubstitutionModification.Find(substitutionModificationID)

               static member addToContext (context:MzIdentMLContext) (item:SubstitutionModification) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:SubstitutionModification) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type PeptideHandler =
               static member init
                    (
                        peptideSequence            : string,
                        ?id                        : string,
                        ?name                      : string,                    
                        ?modifications             : seq<Modification>,
                        ?substitutionModifications : seq<SubstitutionModification>,
                        ?details                   : seq<CVParam>
                        //?mzIdentML                 : MzIdentML
                    ) =
                    let id'                        = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'                      = defaultArg name null
                    let modifications'             = convertOptionToList modifications
                    let substitutionModifications' = convertOptionToList substitutionModifications
                    let details'                   = convertOptionToList details
                    //let mzIdentML'                 = defaultArg mzIdentML Unchecked.defaultof<MzIdentML>
                    {
                        Peptide.ID                        = id'
                        Peptide.Name                      = name'
                        Peptide.PeptideSequence           = peptideSequence
                        Peptide.Modifications             = modifications'
                        Peptide.SubstitutionModifications = substitutionModifications'
                        Peptide.Details                   = details'
                        Peptide.RowVersion                = DateTime.Now
                    }

               static member addName
                    (peptide:Peptide) (name:string) =
                    peptide.Name <- name
                    peptide

               static member addModification
                    (peptide:Peptide) (modification:Modification) =
                    let result = peptide.Modifications <- addToList peptide.Modifications modification
                    peptide

               static member addModifications
                    (peptide:Peptide) (modifications:seq<Modification>) =
                    let result = peptide.Modifications <- addCollectionToList peptide.Modifications modifications
                    peptide

               static member addSubstitutionModification
                    (peptide:Peptide) (substitutionModification:SubstitutionModification) =
                    let result = peptide.SubstitutionModifications <- addToList peptide.SubstitutionModifications substitutionModification
                    peptide

               static member addSubstitutionModifications
                    (peptide:Peptide) (substitutionModifications:seq<SubstitutionModification>) =
                    let result = peptide.SubstitutionModifications <- addCollectionToList peptide.SubstitutionModifications substitutionModifications
                    peptide

               static member addDetail
                    (peptide:Peptide) (detail:CVParam) =
                    let result = peptide.Details <- addToList peptide.Details detail
                    peptide

               static member addDetails
                    (peptide:Peptide) (details:seq<CVParam>) =
                    let result = peptide.Details <- addCollectionToList peptide.Details details
                    peptide

               //static member addMzIdentML
               //     (peptide:Peptide) (mzIdentML:MzIdentML) =
               //     peptide.MzIdentML <- mzIdentML
               //     peptide

               static member findPeptideByID
                    (context:MzIdentMLContext) (peptideID:string) =
                    context.Peptide.Find(peptideID)

               static member findModificationByID
                    (context:MzIdentMLContext) (modificationID:string) =
                    context.Modification.Find(modificationID)

               static member findSubstitutionModificationByID
                    (context:MzIdentMLContext) (substitutionModificationID:string) =
                    context.SubstitutionModification.Find(substitutionModificationID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:Peptide) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:Peptide) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type TranslationTableHandler =
               static member init
                    (
                        ?id      : string,
                        ?name    : string,
                        ?details : seq<CVParam>
                    ) =
                    let id'                        = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'                      = defaultArg name null
                    let details'                   = convertOptionToList details
                    {
                        TranslationTable.ID          = id'
                        TranslationTable.Name        = name'
                        TranslationTable.Details     = details'
                        TranslationTable.RowVersion  = DateTime.Now
                    }

               static member addName
                    (translationTable:TranslationTable) (name:string) =
                    translationTable.Name <- name
                    translationTable

               static member addDetail
                    (translationTable:TranslationTable) (detail:CVParam) =
                    let result = translationTable.Details <- addToList translationTable.Details detail
                    translationTable

               static member addDetails
                    (translationTable:TranslationTable) (details:seq<CVParam>) =
                    let result = translationTable.Details <- addCollectionToList translationTable.Details details
                    translationTable

               static member findTranslationTableID
                    (context:MzIdentMLContext) (translationTableID:string) =
                    context.TranslationTable.Find(translationTableID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:TranslationTable) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:TranslationTable) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type MeasureHandler =
               static member init
                    (
                        details  : seq<CVParam>,
                        ?id      : string,
                        ?name    : string 
                    ) =
                    let id'   = defaultArg id (System.Guid.NewGuid().ToString())
                    let name' = defaultArg name null
                    {
                        Measure.ID          = id'
                        Measure.Name        = name'
                        Measure.Details     = details |> List
                        Measure.RowVersion  = DateTime.Now
                    }

               static member addName
                    (measure:Measure) (name:string) =
                    measure.Name <- name
                    measure

               static member findMeasureByID
                    (context:MzIdentMLContext) (measureID:string) =
                    context.Measure.Find(measureID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:Measure) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:Measure) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type ResidueHandler =
               static member init
                    (
                        code    : string,
                        mass    : float,
                        ?id     : string
                    ) =
                    let id'   = defaultArg id (System.Guid.NewGuid().ToString())
                    {
                        Residue.ID          = id'
                        Residue.Code        = code
                        Residue.Mass        = mass
                        Residue.RowVersion  = DateTime.Now
                    }

               static member findResidueByID
                    (context:MzIdentMLContext) (residueID:string) =
                    context.Residue.Find(residueID)

               static member addToContext (context:MzIdentMLContext) (item:Residue) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:Residue) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type AmbiguousResidueHandler =
               static member init
                    (
                        code    : string,
                        details : seq<CVParam>,
                        ?id     : string
                    ) =
                    let id'   = defaultArg id (System.Guid.NewGuid().ToString())
                    {
                        AmbiguousResidue.ID          = id'
                        AmbiguousResidue.Code        = code
                        AmbiguousResidue.Details     = details |> List
                        AmbiguousResidue.RowVersion  = DateTime.Now
                    }

               static member findAmbiguousResidueByID
                    (context:MzIdentMLContext) (ambiguousResidueID:string) =
                    context.AmbiguousResidue.Find(ambiguousResidueID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:AmbiguousResidue) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:AmbiguousResidue) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type MassTableHandler =
               static member init
                    (
                        msLevel           : string,
                        ?id               : string,
                        ?name             : string,
                        ?residue          : seq<Residue>,
                        ?ambiguousResidue : seq<AmbiguousResidue>,
                        ?details          : seq<CVParam>
                    ) =
                    let id'               = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'             = defaultArg name null
                    let residue'          = convertOptionToList residue
                    let ambiguousResidue' = convertOptionToList ambiguousResidue
                    let details'          = convertOptionToList details
                    {
                        MassTable.ID                = id'
                        MassTable.Name              = name'
                        MassTable.MSLevel           = msLevel
                        MassTable.Residues          = residue'
                        MassTable.AmbiguousResidues = ambiguousResidue'
                        MassTable.Details           = details'
                        MassTable.RowVersion        = DateTime.Now
                    }

               static member addName
                    (massTable:MassTable) (name:string) =
                    massTable.Name <- name
                    massTable

               static member addResidue
                    (massTable:MassTable) (residue:Residue) =
                    let result = massTable.Residues <- addToList massTable.Residues residue
                    massTable

               static member addResidues
                    (massTable:MassTable) (residues:seq<Residue>) =
                    let result = massTable.Residues <- addCollectionToList massTable.Residues residues
                    massTable

               static member addAmbiguousResidue
                    (massTable:MassTable) (ambiguousResidues:AmbiguousResidue) =
                    let result = massTable.AmbiguousResidues <- addToList massTable.AmbiguousResidues ambiguousResidues
                    massTable

               static member addAmbiguousResidues
                    (massTable:MassTable) (ambiguousResidues:seq<AmbiguousResidue>) =
                    let result = massTable.AmbiguousResidues <- addCollectionToList massTable.AmbiguousResidues ambiguousResidues
                    massTable

               static member addDetail
                    (massTable:MassTable) (detail:CVParam) =
                    let result = massTable.Details <- addToList massTable.Details detail
                    massTable

               static member addDetails
                    (massTable:MassTable) (details:seq<CVParam>) =
                    let result = massTable.Details <- addCollectionToList massTable.Details details
                    massTable

               static member findMassTableByID
                    (context:MzIdentMLContext) (massTableID:string) =
                    context.MassTable.Find(massTableID)

               static member findResidueByID
                    (context:MzIdentMLContext) (residueID:string) =
                    context.Residue.Find(residueID)

               static member findAmbiguousResidueByID
                    (context:MzIdentMLContext) (ambiguousResidueID:string) =
                    context.AmbiguousResidue.Find(ambiguousResidueID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:MassTable) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:MassTable) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type ValueHandler =
               static member init
                    (
                        value   : float,
                        ?id     : string
                    ) =
                    let id'   = defaultArg id (System.Guid.NewGuid().ToString())
                    {
                        Value.ID          = id'
                        Value.Value       = value
                        Value.RowVersion  = DateTime.Now
                    }

               static member findValueByID
                    (context:MzIdentMLContext) (valueID:string) =
                    context.Value.Find(valueID)

               static member addToContext (context:MzIdentMLContext) (item:Value) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:Value) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type FragmentArrayHandler =
               static member init
                    (
                        measure : Measure,
                        values  : seq<Value>,
                        ?id     : string
                    ) =
                    let id'   = defaultArg id (System.Guid.NewGuid().ToString())
                    {
                        FragmentArray.ID          = id'
                        FragmentArray.Measure     = measure
                        FragmentArray.Values      = values |> List
                        FragmentArray.RowVersion  = DateTime.Now
                    }

               static member findFragmentArrayByID
                    (context:MzIdentMLContext) (fragmentArrayID:string) =
                    context.FragmentArray.Find(fragmentArrayID)

               static member findMeasureByID
                    (context:MzIdentMLContext) (measureID:string) =
                    context.Measure.Find(measureID)

               static member findValueByID
                    (context:MzIdentMLContext) (valueID:string) =
                    context.Value.Find(valueID)

               static member addToContext (context:MzIdentMLContext) (item:FragmentArray) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:FragmentArray) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type IndexHandler =
               static member init
                    (
                        index : int,
                        ?id   : string
                    ) =
                    let id'   = defaultArg id (System.Guid.NewGuid().ToString())
                    {
                        Index.ID          = id'
                        Index.Index       = index
                        Index.RowVersion  = DateTime.Now
                    }

               static member findIndexByID
                    (context:MzIdentMLContext) (indexID:string) =
                    context.Index.Find(indexID)

               static member addToContext (context:MzIdentMLContext) (item:Index) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:Index) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type IonTypeHandler =
               static member init
                    (
                        details        : seq<CVParam>,
                        ?id            : string,
                        ?index         : seq<Index>,
                        ?fragmentArray : seq<FragmentArray>
                    ) =
                    let id'            = defaultArg id (System.Guid.NewGuid().ToString())
                    let index'         = convertOptionToList index
                    let fragmentArray' = convertOptionToList fragmentArray
                    {
                        IonType.ID             = id'
                        IonType.Index          = index'
                        IonType.FragmentArrays = fragmentArray'
                        IonType.Details        = details |> List
                        IonType.RowVersion     = DateTime.Now
                    }

               static member addIndex
                    (ionType:IonType) (index:Index) =
                    let result = ionType.Index <- addToList ionType.Index index
                    ionType

               static member addIndexes
                    (ionType:IonType) (index:seq<Index>) =
                    let result = ionType.Index <- addCollectionToList ionType.Index index
                    ionType

               static member addFragmentArray
                    (ionType:IonType) (fragmentArray:FragmentArray) =
                    let result = ionType.FragmentArrays <- addToList ionType.FragmentArrays fragmentArray
                    ionType

               static member addFragmentArrays
                    (ionType:IonType) (fragmentArrays:seq<FragmentArray>) =
                    let result = ionType.FragmentArrays <- addCollectionToList ionType.FragmentArrays fragmentArrays
                    ionType

               static member findIonTypeByID
                    (context:MzIdentMLContext) (ionTypeID:string) =
                    context.Iontype.Find(ionTypeID)

               static member findIndexByID
                    (context:MzIdentMLContext) (indexID:string) =
                    context.Index.Find(indexID)

               static member findFragmentArrayByID
                    (context:MzIdentMLContext) (fragmentArrayID:string) =
                    context.FragmentArray.Find(fragmentArrayID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:IonType) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:IonType) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

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
                    let name'                        = defaultArg name null
                    let externalFormatDocumentation' = defaultArg externalFormatDocumentation null
                    {
                        SpectraData.ID                          = id'
                        SpectraData.Name                        = name'
                        SpectraData.Location                    = location
                        SpectraData.ExternalFormatDocumentation = externalFormatDocumentation'
                        SpectraData.FileFormat                  = fileFormat
                        SpectraData.SpectrumIDFormat            = spectrumIDFormat
                        SpectraData.RowVersion                  = DateTime.Now
                    }

               static member addName
                    (spectraData:SpectraData) (name:string) =
                    spectraData.Name <- name
                    spectraData

               static member addExternalFormatDocumentation
                    (spectraData:SpectraData) (externalFormatDocumentation:string) =
                    spectraData.ExternalFormatDocumentation <- externalFormatDocumentation
                    spectraData

               //static member addInputs
               //     (spectraData:SpectraData) (inputs:Inputs) =
               //     spectraData.Inputs <- inputs
               //     spectraData

               //static member addSpectrumIdentification
               //     (spectraData:SpectraData) (spectrumIdentification:SpectrumIdentification) =
               //     spectraData.SpectrumIdentification <- spectrumIdentification
               //     spectraData

               static member findSpectraDataByID
                    (context:MzIdentMLContext) (spectraDataID:string) =
                    context.SpectraData.Find(spectraDataID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:SpectraData) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:SpectraData) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type SpecificityRulesHandler =
               static member init
                    ( 
                        details    : seq<CVParam>,
                        ?id        : string
                    ) =
                    let id'   = defaultArg id (System.Guid.NewGuid().ToString())
                    {
                        SpecificityRule.ID         = id'
                        SpecificityRule.Details    = details |> List
                        SpecificityRule.RowVersion = DateTime.Now
                    }

               static member findSpecificityRuleByID
                    (context:MzIdentMLContext) (specificityRuleID:string) =
                    context.SpecificityRule.Find(specificityRuleID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:SpecificityRule) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:SpecificityRule) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type SearchModificationHandler =
               static member init
                    ( 
                        fixedMod          : bool,
                        massDelta         : float,
                        residues          : string,
                        details           : seq<CVParam>,
                        ?id               : string,
                        ?specificityRules : seq<SpecificityRule>
                    ) =
                    let id'               = defaultArg id (System.Guid.NewGuid().ToString())
                    let specificityRules' = convertOptionToList specificityRules
                    {
                        SearchModification.ID               = id'
                        SearchModification.FixedMod         = fixedMod
                        SearchModification.MassDelta        = massDelta
                        SearchModification.Residues         = residues
                        SearchModification.SpecificityRules = specificityRules'
                        SearchModification.Details          = details |> List
                        SearchModification.RowVersion       = DateTime.Now
                    }

               static member addSpecificityRule
                    (searchModification:SearchModification) (specificityRule:SpecificityRule) =
                    let result = searchModification.SpecificityRules <- addToList searchModification.SpecificityRules specificityRule
                    searchModification

               static member addSpecificityRules
                    (searchModification:SearchModification) (specificityRules:seq<SpecificityRule>) =
                    let result = searchModification.SpecificityRules <- addCollectionToList searchModification.SpecificityRules specificityRules
                    searchModification

               static member findSearchModificationByID
                    (context:MzIdentMLContext) (searchModificationID:string) =
                    context.SearchModification.Find(searchModificationID)

               static member findSpecificityRuleByID
                    (context:MzIdentMLContext) (specificityRuleID:string) =
                    context.SpecificityRule.Find(specificityRuleID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:SearchModification) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:SearchModification) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

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
                        ?enzymeName      : seq<CVParam>
                    ) =
                    let id'              = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'            = defaultArg name null
                    let cTermGain'       = defaultArg cTermGain null
                    let nTermGain'       = defaultArg nTermGain null
                    let minDistance'     = defaultArg minDistance Unchecked.defaultof<int>
                    let missedCleavages' = defaultArg missedCleavages Unchecked.defaultof<int>
                    let semiSpecific'    = defaultArg semiSpecific Unchecked.defaultof<bool>
                    let siteRegexc'      = defaultArg siteRegexc null
                    let enzymeName'      = convertOptionToList enzymeName
                    {
                        Enzyme.ID              = id'
                        Enzyme.Name            = name'
                        Enzyme.CTermGain       = cTermGain'
                        Enzyme.NTermGain       = nTermGain'
                        Enzyme.MinDistance     = minDistance'
                        Enzyme.MissedCleavages = missedCleavages'
                        Enzyme.SemiSpecific    = semiSpecific'
                        Enzyme.SiteRegexc      = siteRegexc'
                        Enzyme.EnzymeName      = enzymeName'
                        Enzyme.RowVersion      = DateTime.Now
                    }

               static member addName
                    (enzyme:Enzyme) (name:string) =
                    enzyme.Name <- name
                    enzyme

               static member addCTermGain
                    (enzyme:Enzyme) (cTermGain:string) =
                    enzyme.CTermGain <- cTermGain
                    enzyme

               static member addNTermGain
                    (enzyme:Enzyme) (nTermGain:string) =
                    enzyme.NTermGain <- nTermGain
                    enzyme

               static member addMinDistance
                    (enzyme:Enzyme) (minDistance:int) =
                    enzyme.MinDistance <- minDistance
                    enzyme

               static member addMissedCleavages
                    (enzyme:Enzyme) (missedCleavages:int) =
                    enzyme.MissedCleavages <- missedCleavages
                    enzyme

               static member addSemiSpecific
                    (enzyme:Enzyme) (semiSpecific:bool) =
                    enzyme.SemiSpecific <- semiSpecific
                    enzyme

               static member addSiteRegexc
                    (enzyme:Enzyme) (siteRegexc:string) =
                    enzyme.SiteRegexc <- siteRegexc
                    enzyme

               static member addEnzymeName
                    (enzyme:Enzyme) (enzymeName:CVParam) =
                    let result = enzyme.EnzymeName <- addToList enzyme.EnzymeName enzymeName
                    enzyme

               static member addEnzymeNames
                    (enzyme:Enzyme) (enzymeNames:seq<CVParam>) =
                    let result = enzyme.EnzymeName <- addCollectionToList enzyme.EnzymeName enzymeNames
                    enzyme

               static member findEnzymeByID
                    (context:MzIdentMLContext) (enzymeID:string) =
                    context.Enzyme.Find(enzymeID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:Enzyme) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:Enzyme) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type FilterHandler =
               static member init
                    (
                        filterType : CVParam,
                        ?id        : string,
                        ?includes  : seq<CVParam>,
                        ?excludes  : seq<CVParam>
                    ) =
                    let id'         = defaultArg id (System.Guid.NewGuid().ToString())
                    let includes'   = convertOptionToList includes
                    let excludes'   = convertOptionToList excludes
                    {
                        Filter.ID         = id'
                        Filter.FilterType = filterType
                        Filter.Includes   = includes'
                        Filter.Excludes   = excludes'
                        Filter.RowVersion = DateTime.Now
                    }

               static member addInclude
                    (filter:Filter) (include':CVParam) =
                    let result = filter.Includes <- addToList filter.Includes include'
                    filter

               static member addIncludes
                    (filter:Filter) (includes:seq<CVParam>) =
                    let result = filter.Includes <- addCollectionToList filter.Includes includes
                    filter

               static member addExclude
                    (filter:Filter) (exclude':CVParam) =
                    let result = filter.Excludes <- addToList filter.Excludes exclude'
                    filter

               static member addExcludes
                    (filter:Filter) (excludes:seq<CVParam>) =
                    let result = filter.Excludes <- addCollectionToList filter.Excludes excludes
                    filter

               static member findFilterByID
                    (context:MzIdentMLContext) (filterID:string) =
                    context.Filter.Find(filterID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:Filter) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:Filter) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type FrameHandler =
               static member init
                    ( 
                        frame : int,
                        ?id   : string
                    ) =
                    let id'   = defaultArg id (System.Guid.NewGuid().ToString())
                    {
                        Frame.ID         = id'
                        Frame.Frame      = frame
                        Frame.RowVersion = DateTime.Now
                    }

               static member findFrameByID
                    (context:MzIdentMLContext) (frameID:string) =
                    context.Frame.Find(frameID)

               static member addToContext (context:MzIdentMLContext) (item:Frame) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:Frame) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

        type SpectrumIdentificationProtocolHandler =
               static member init
                    (
                        analysisSoftware        : AnalysisSoftware,
                        searchType              : CVParam ,
                        threshold               : seq<CVParam>,
                        ?id                     : string,
                        ?name                   : string,
                        ?additionalSearchParams : seq<CVParam>,
                        ?modificationParams     : seq<SearchModification>,
                        ?enzymes                : seq<Enzyme>,
                        ?independent_Enzymes    : bool,
                        ?massTables             : seq<MassTable>,
                        ?fragmentTolerance      : seq<CVParam>,
                        ?parentTolerance        : seq<CVParam>,
                        ?databaseFilters        : seq<Filter>,
                        ?frames                 : seq<Frame>,
                        ?translationTable       : seq<TranslationTable>
                        //?mzIdentML              : MzIdentML
                    ) =
                    let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'                   = defaultArg name null
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
                    //let mzIdentML'              = defaultArg mzIdentML Unchecked.defaultof<MzIdentML>
                    {
                        SpectrumIdentificationProtocol.ID                     = id'
                        SpectrumIdentificationProtocol.Name                   = name'
                        SpectrumIdentificationProtocol.AnalysisSoftware       = analysisSoftware
                        SpectrumIdentificationProtocol.SearchType             = searchType
                        SpectrumIdentificationProtocol.AdditionalSearchParams = additionalSearchParams'
                        SpectrumIdentificationProtocol.ModificationParams     = modificationParams'
                        SpectrumIdentificationProtocol.Enzymes                = enzymes'
                        SpectrumIdentificationProtocol.Independent_Enzymes    = independent_Enzymes'
                        SpectrumIdentificationProtocol.MassTables             = massTables'
                        SpectrumIdentificationProtocol.FragmentTolerance      = fragmentTolerance'
                        SpectrumIdentificationProtocol.ParentTolerance        = parentTolerance'
                        SpectrumIdentificationProtocol.Threshold              = threshold |> List
                        SpectrumIdentificationProtocol.DatabaseFilters        = databaseFilters'
                        SpectrumIdentificationProtocol.Frames                 = frames'
                        SpectrumIdentificationProtocol.TranslationTables      = translationTable'
                        SpectrumIdentificationProtocol.RowVersion             = DateTime.Now
                    }

               static member addName
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (name:string) =
                    spectrumIdentificationProtocol.Name <- name
                    spectrumIdentificationProtocol

               static member addEnzymeAdditionalSearchParam
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (additionalSearchParam:CVParam) =
                    let result = spectrumIdentificationProtocol.AdditionalSearchParams <- addToList spectrumIdentificationProtocol.AdditionalSearchParams additionalSearchParam
                    spectrumIdentificationProtocol

               static member addEnzymeAdditionalSearchParams
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (additionalSearchParams:seq<CVParam>) =
                    let result = spectrumIdentificationProtocol.AdditionalSearchParams <- addCollectionToList spectrumIdentificationProtocol.AdditionalSearchParams additionalSearchParams
                    spectrumIdentificationProtocol

               static member addModificationParam
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (modificationParam:SearchModification) =
                    let result = spectrumIdentificationProtocol.ModificationParams <- addToList spectrumIdentificationProtocol.ModificationParams modificationParam
                    spectrumIdentificationProtocol

               static member addModificationParams
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (modificationParams:seq<SearchModification>) =
                    let result = spectrumIdentificationProtocol.ModificationParams <- addCollectionToList spectrumIdentificationProtocol.ModificationParams modificationParams
                    spectrumIdentificationProtocol

               static member addEnzyme
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (enzyme:Enzyme) =
                    let result = spectrumIdentificationProtocol.Enzymes <- addToList spectrumIdentificationProtocol.Enzymes enzyme
                    spectrumIdentificationProtocol

               static member addEnzymes
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (enzymes:seq<Enzyme>) =
                    let result = spectrumIdentificationProtocol.Enzymes <- addCollectionToList spectrumIdentificationProtocol.Enzymes enzymes
                    spectrumIdentificationProtocol

               static member addIndependent_Enzymes
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (independent_Enzymes:bool) =
                    spectrumIdentificationProtocol.Independent_Enzymes <- independent_Enzymes
                    spectrumIdentificationProtocol

               static member addMassTable
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (massTable:MassTable) =
                    let result = spectrumIdentificationProtocol.MassTables <- addToList spectrumIdentificationProtocol.MassTables massTable
                    spectrumIdentificationProtocol

               static member addMassTables
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (massTables:seq<MassTable>) =
                    let result = spectrumIdentificationProtocol.MassTables <- addCollectionToList spectrumIdentificationProtocol.MassTables massTables
                    spectrumIdentificationProtocol

               static member addFragmentTolerance
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (fragmentTolerance:CVParam) =
                    let result = spectrumIdentificationProtocol.FragmentTolerance <- addToList spectrumIdentificationProtocol.FragmentTolerance fragmentTolerance
                    spectrumIdentificationProtocol

               static member addFragmentTolerances
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (fragmentTolerances:seq<CVParam>) =
                    let result = spectrumIdentificationProtocol.FragmentTolerance <- addCollectionToList spectrumIdentificationProtocol.FragmentTolerance fragmentTolerances
                    spectrumIdentificationProtocol

               static member addParentTolerance
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (parentTolerance:CVParam) =
                    let result = spectrumIdentificationProtocol.ParentTolerance <- addToList spectrumIdentificationProtocol.ParentTolerance parentTolerance
                    spectrumIdentificationProtocol

               static member addParentTolerances
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (parentTolerances:seq<CVParam>) =
                    let result = spectrumIdentificationProtocol.ParentTolerance <- addCollectionToList spectrumIdentificationProtocol.ParentTolerance parentTolerances
                    spectrumIdentificationProtocol

               static member addDatabaseFilter
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (databaseFilter:Filter) =
                    let result = spectrumIdentificationProtocol.DatabaseFilters <- addToList spectrumIdentificationProtocol.DatabaseFilters databaseFilter
                    spectrumIdentificationProtocol

               static member addDatabaseFilters
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (databaseFilters:seq<Filter>) =
                    let result = spectrumIdentificationProtocol.DatabaseFilters <- addCollectionToList spectrumIdentificationProtocol.DatabaseFilters databaseFilters
                    spectrumIdentificationProtocol

               static member addFrame
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (frame:Frame) =
                    let result = spectrumIdentificationProtocol.Frames <- addToList spectrumIdentificationProtocol.Frames frame
                    spectrumIdentificationProtocol

               static member addFrames
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (frames:seq<Frame>) =
                    let result = spectrumIdentificationProtocol.Frames <- addCollectionToList spectrumIdentificationProtocol.Frames frames
                    spectrumIdentificationProtocol

               static member addTranslationTable
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (translationTable:TranslationTable) =
                    let result = spectrumIdentificationProtocol.TranslationTables <- addToList spectrumIdentificationProtocol.TranslationTables translationTable
                    spectrumIdentificationProtocol

               static member addTranslationTables
                    (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (translationTables:seq<TranslationTable>) =
                    let result = spectrumIdentificationProtocol.TranslationTables <- addCollectionToList spectrumIdentificationProtocol.TranslationTables translationTables
                    spectrumIdentificationProtocol

               //static member addMzIdentML
               //     (spectrumIdentificationProtocol:SpectrumIdentificationProtocol) (mzIdentML:MzIdentML) =
               //     spectrumIdentificationProtocol.MzIdentML <- mzIdentML
               //     spectrumIdentificationProtocol

               static member findSpectrumIdentificationProtocolByID
                    (context:MzIdentMLContext) (spectrumIdentificationProtocolID:string) =
                    context.SpectrumIdentificationProtocol.Find(spectrumIdentificationProtocolID)

               static member findAnalysisSoftwareByID
                    (context:MzIdentMLContext) (analysisSoftwareID:string) =
                    context.AnalysisSoftware.Find(analysisSoftwareID)

               static member findSearchModificationByID
                    (context:MzIdentMLContext) (searchModificationID:string) =
                    context.SearchModification.Find(searchModificationID)

               static member findEnzymeByID
                    (context:MzIdentMLContext) (enzymeID:string) =
                    context.Enzyme.Find(enzymeID)

               static member findMassTableByID
                    (context:MzIdentMLContext) (massTableID:string) =
                    context.MassTable.Find(massTableID)

               static member findFilterByID
                    (context:MzIdentMLContext) (filterID:string) =
                    context.Filter.Find(filterID)

               static member findTranslationTableByID
                    (context:MzIdentMLContext) (translationTableID:string) =
                    context.TranslationTable.Find(translationTableID)

               static member findFrameByID
                    (context:MzIdentMLContext) (frameID:string) =
                    context.Frame.Find(frameID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

               static member addToContext (context:MzIdentMLContext) (item:SpectrumIdentificationProtocol) =
                    (addToContextWithExceptionCheck context item)

               static member addToContextAndInsert (context:MzIdentMLContext) (item:SpectrumIdentificationProtocol) =
                    (addToContextWithExceptionCheck context item) |> ignore
                    insertWithExceptionCheck context

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
                        ?details                     : seq<CVParam>
                    ) =
                    let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'                        = defaultArg name null
                    let numDatabaseSequences'        = defaultArg numDatabaseSequences Unchecked.defaultof<int64>
                    let numResidues'                 = defaultArg numResidues Unchecked.defaultof<int64>
                    let releaseDate'                 = defaultArg releaseDate Unchecked.defaultof<DateTime>
                    let version'                     = defaultArg version null
                    let externalFormatDocumentation' = defaultArg externalFormatDocumentation null
                    let details'                     = convertOptionToList details
                    {
                        SearchDatabase.ID                          = id';
                        SearchDatabase.Name                        = name';
                        SearchDatabase.Location                    = location;
                        SearchDatabase.NumDatabaseSequences        = numDatabaseSequences';
                        SearchDatabase.NumResidues                 = numResidues';
                        SearchDatabase.ReleaseDate                 = releaseDate';
                        SearchDatabase.Version                     = version';
                        SearchDatabase.ExternalFormatDocumentation = externalFormatDocumentation';
                        SearchDatabase.FileFormat                  = fileFormat;
                        SearchDatabase.DatabaseName                = databaseName;
                        SearchDatabase.Details                     =  details';
                        SearchDatabase.RowVersion                  = DateTime.Now.Date

                    }

               static member addName
                    (searchDatabase:SearchDatabase) (name:string) =
                    searchDatabase.Name <- name
                    searchDatabase

               static member addNumDatabaseSequences
                    (searchDatabase:SearchDatabase) (numDatabaseSequences:int64) =
                    searchDatabase.NumDatabaseSequences <- numDatabaseSequences
                    searchDatabase

               static member addNumResidues
                    (searchDatabase:SearchDatabase) (numResidues:int64) =
                    searchDatabase.NumResidues <- numResidues
                    searchDatabase

               static member addReleaseDate
                    (searchDatabase:SearchDatabase) (releaseDate:DateTime) =
                    searchDatabase.ReleaseDate <- releaseDate
                    searchDatabase

               static member addVersion
                    (searchDatabase:SearchDatabase) (version:string) =
                    searchDatabase.Version <- version
                    searchDatabase

               static member addExternalFormatDocumentation
                    (searchDatabase:SearchDatabase) (externalFormatDocumentation:string) =
                    searchDatabase.Version <- externalFormatDocumentation
                    searchDatabase

               static member addDetails
                    (searchDatabase:SearchDatabase) (details:seq<CVParam>) =
                    let result = searchDatabase.Details <- addCollectionToList searchDatabase.Details details
                    searchDatabase

               //static member addSpectrumIdentification
               //     (searchDatabase:SearchDatabase) (spectrumIdentification:SpectrumIdentification) =
               //     searchDatabase.SpectrumIdentification <- spectrumIdentification
               //     searchDatabase

               //static member addInputs
               //     (searchDatabase:SearchDatabase) (inputs:Inputs) =
               //     searchDatabase.Inputs <- inputs
               //     searchDatabase

               static member findSearchDatabaseByID
                    (context:MzIdentMLContext) (searchDatabaseID:string) =
                    context.SearchDatabase.Find(searchDatabaseID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

                static member addToContext (context:MzIdentMLContext) (item:SearchDatabase) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:SearchDatabase) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type DBSequenceHandler =
               static member init
                    (
                        accession      : string,
                        searchDatabase : SearchDatabase,
                        ?id            : string,
                        ?name          : string,
                        ?sequence      : string,
                        ?length        : int,
                        ?details       : seq<CVParam>
                        //?mzIdentML     : MzIdentML
                    ) =
                    let id'       = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'     = defaultArg name null
                    let sequence' = defaultArg sequence null
                    let length'   = defaultArg length Unchecked.defaultof<int>
                    let details'  = convertOptionToList details
                    //let mzIdentML' = defaultArg mzIdentML Unchecked.defaultof<MzIdentML>
                    {
                        DBSequence.ID             = id';
                        DBSequence.Name           = name';
                        DBSequence.Accession      = accession;
                        DBSequence.SearchDatabase = searchDatabase;
                        DBSequence.Sequence       = sequence';
                        DBSequence.Length         = length';
                        DBSequence.Details        = details';
                        DBSequence.RowVersion     = DateTime.Now
                    }

               static member addName
                    (dbSequence:DBSequence) (name:string) =
                    dbSequence.Name <- name
                    dbSequence

               static member addSequence
                    (dbSequence:DBSequence) (sequence:string) =
                    dbSequence.Sequence <- sequence
                    dbSequence

               static member addLength
                    (dbSequence:DBSequence) (length:int) =
                    dbSequence.Length <- length
                    dbSequence

               static member addDetail
                    (dbSequence:DBSequence) (detail:CVParam) =
                    let result = dbSequence.Details <- addToList dbSequence.Details detail
                    dbSequence

               static member addDetails
                    (dbSequence:DBSequence) (details:seq<CVParam>) =
                    let result = dbSequence.Details <- addCollectionToList dbSequence.Details details
                    dbSequence

               //static member addMzIdentML
               //     (dbSequence:DBSequence) (mzIdentML:MzIdentML) =
               //     dbSequence.MzIdentML <- mzIdentML

               static member findDBSequenceByID
                    (context:MzIdentMLContext) (dbSequenceID:string) =
                    context.DBSequence.Find(dbSequenceID)

               static member findSearchDatabaseByID
                    (context:MzIdentMLContext) (searchDatabaseID:string) =
                    context.SearchDatabase.Find(searchDatabaseID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

                static member addToContext (context:MzIdentMLContext) (item:DBSequence) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:DBSequence) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context


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
                        ?details                    : seq<CVParam>
                        //?mzIdentML                  : MzIdentML
                    ) =
                    let id'                         = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'                       = defaultArg name null
                    let start'                      = defaultArg start Unchecked.defaultof<int>
                    let end''                       = defaultArg end' Unchecked.defaultof<int>
                    let pre'                        = defaultArg pre null
                    let post'                       = defaultArg post null
                    let frame'                      = defaultArg frame Unchecked.defaultof<Frame>
                    let isDecoy'                    = defaultArg isDecoy Unchecked.defaultof<bool>
                    let translationTable'           = defaultArg translationTable Unchecked.defaultof<TranslationTable>
                    let details'                    = convertOptionToList details
                    {
                        PeptideEvidence.ID                         = id'
                        PeptideEvidence.Name                       = name'
                        PeptideEvidence.DBSequence                 = dbSequence
                        PeptideEvidence.Peptide                    = peptide
                        PeptideEvidence.Start                      = start'
                        PeptideEvidence.End                        = end''
                        PeptideEvidence.Pre                        = pre'
                        PeptideEvidence.Post                       = post'
                        PeptideEvidence.Frame                      = frame'
                        PeptideEvidence.IsDecoy                    = isDecoy'
                        PeptideEvidence.TranslationTable           = translationTable'
                        PeptideEvidence.Details                    = details'
                        PeptideEvidence.RowVersion                 = DateTime.Now
                    }

               static member addName
                    (peptideEvidence:PeptideEvidence) (name:string) =
                    peptideEvidence.Name <- name
                    peptideEvidence

               static member addStart
                    (peptideEvidence:PeptideEvidence) (start:int) =
                    peptideEvidence.Start <- start
                    peptideEvidence

               static member addEnd 
                    (peptideEvidence:PeptideEvidence) (end':int) =
                    peptideEvidence.End  <- end'
                    peptideEvidence

               static member addPre
                    (peptideEvidence:PeptideEvidence) (pre:string) =
                    peptideEvidence.Pre <- pre
                    peptideEvidence

               static member addPost
                    (peptideEvidence:PeptideEvidence) (post:string) =
                    peptideEvidence.Post <- post
                    peptideEvidence

               static member addFrame
                    (peptideEvidence:PeptideEvidence) (frame:Frame) =
                    peptideEvidence.Frame <- frame
                    peptideEvidence

               static member addIsDecoy
                    (peptideEvidence:PeptideEvidence) (isDecoy:bool) =
                    peptideEvidence.IsDecoy <- isDecoy
                    peptideEvidence

               static member addTranslationTable
                    (peptideEvidence:PeptideEvidence) (translationTable:TranslationTable) =
                    peptideEvidence.TranslationTable <- translationTable
                    peptideEvidence

               static member addDetail
                    (peptideEvidence:PeptideEvidence) (detail:CVParam) =
                    let result = peptideEvidence.Details <- addToList peptideEvidence.Details detail
                    peptideEvidence

               static member addDetails
                    (peptideEvidence:PeptideEvidence) (details:seq<CVParam>) =
                    let result = peptideEvidence.Details <- addCollectionToList peptideEvidence.Details details
                    peptideEvidence

               //static member addSpectrumIdentificationItem
               //     (peptideEvidence:PeptideEvidence) (spectrumIdentificationItem:SpectrumIdentificationItem) =
               //     peptideEvidence.SpectrumIdentificationItem <- spectrumIdentificationItem
               //     peptideEvidence

               //static member addMzIdentML
               //     (peptideEvidence:PeptideEvidence) (mzIdentML:MzIdentML) =
               //     peptideEvidence.MzIdentML <- mzIdentML
               //     peptideEvidence

               static member findPeptideEvidenceByID
                    (context:MzIdentMLContext) (peptideEvidenceID:string) =
                    context.PeptideEvidence.Find(peptideEvidenceID)

               static member findDBSequenceByID
                    (context:MzIdentMLContext) (dbSequenceID:string) =
                    context.DBSequence.Find(dbSequenceID)

               static member findPeptideByID
                    (context:MzIdentMLContext) (peptideID:string) =
                    context.Peptide.Find(peptideID)

               static member findFrameID
                    (context:MzIdentMLContext) (frameID:string) =
                    context.Frame.Find(frameID)

               static member findTranslationTableByID
                    (context:MzIdentMLContext) (translationTableID:string) =
                    context.TranslationTable.Find(translationTableID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

                static member addToContext (context:MzIdentMLContext) (item:PeptideEvidence) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:PeptideEvidence) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

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
                        ?details                      : seq<CVParam>
                    ) =
                    let id'                           = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'                         = defaultArg name null
                    let sample'                       = defaultArg sample Unchecked.defaultof<Sample>
                    let massTable'                    = defaultArg massTable Unchecked.defaultof<MassTable>
                    let peptideEvidences'             = convertOptionToList peptideEvidences
                    let fragmentations'               = convertOptionToList fragmentations
                    let calculatedMassToCharge'       = defaultArg calculatedMassToCharge Unchecked.defaultof<float>
                    let calculatedPI'                 = defaultArg calculatedPI Unchecked.defaultof<float>
                    let details'                      = convertOptionToList details
                    {
                        SpectrumIdentificationItem.ID                           = id'
                        SpectrumIdentificationItem.Name                         = name'
                        SpectrumIdentificationItem.Sample                       = sample'
                        SpectrumIdentificationItem.MassTable                    = massTable'
                        SpectrumIdentificationItem.PassThreshold                = passThreshold
                        SpectrumIdentificationItem.Rank                         = rank
                        SpectrumIdentificationItem.PeptideEvidences             = peptideEvidences'
                        SpectrumIdentificationItem.Fragmentations               = fragmentations'
                        SpectrumIdentificationItem.Peptide                      = peptide
                        SpectrumIdentificationItem.ChargeState                  = chargeState
                        SpectrumIdentificationItem.ExperimentalMassToCharge     = experimentalMassToCharge
                        SpectrumIdentificationItem.CalculatedMassToCharge       = calculatedMassToCharge'
                        SpectrumIdentificationItem.CalculatedPI                 = calculatedPI'
                        SpectrumIdentificationItem.Details                      = details'
                        SpectrumIdentificationItem.RowVersion                   = DateTime.Now
                    }

               static member addName
                    (spectrumIdentificationItem:SpectrumIdentificationItem) (name:string) =
                    spectrumIdentificationItem.Name <- name
                    spectrumIdentificationItem

               static member addSample
                    (spectrumIdentificationItem:SpectrumIdentificationItem) (sample:Sample) =
                    spectrumIdentificationItem.Sample <- sample 
                    spectrumIdentificationItem

               static member addMassTable
                    (spectrumIdentificationItem:SpectrumIdentificationItem) (massTable:MassTable) =
                    spectrumIdentificationItem.MassTable <- massTable
                    spectrumIdentificationItem

               static member addPeptideEvidence
                    (spectrumIdentificationItem:SpectrumIdentificationItem) (peptideEvidence:PeptideEvidence) =
                    let result = spectrumIdentificationItem.PeptideEvidences <- addToList spectrumIdentificationItem.PeptideEvidences peptideEvidence
                    spectrumIdentificationItem

               static member addPeptideEvidences
                    (spectrumIdentificationItem:SpectrumIdentificationItem) (peptideEvidences:seq<PeptideEvidence>) =
                    let result = spectrumIdentificationItem.PeptideEvidences <- addCollectionToList spectrumIdentificationItem.PeptideEvidences peptideEvidences
                    spectrumIdentificationItem   

               static member addFragmentation
                    (spectrumIdentificationItem:SpectrumIdentificationItem) (ionType:IonType) =
                    let result = spectrumIdentificationItem.Fragmentations <- addToList spectrumIdentificationItem.Fragmentations ionType
                    spectrumIdentificationItem

               static member addFragmentations
                    (spectrumIdentificationItem:SpectrumIdentificationItem) (ionTypes:seq<IonType>) =
                    let result = spectrumIdentificationItem.Fragmentations <- addCollectionToList spectrumIdentificationItem.Fragmentations ionTypes
                    spectrumIdentificationItem 

               static member addCalculatedMassToCharge
                    (spectrumIdentificationItem:SpectrumIdentificationItem) (calculatedMassToCharge:float) =
                    spectrumIdentificationItem.CalculatedMassToCharge <- calculatedMassToCharge
                    spectrumIdentificationItem

               static member addCalculatedPI
                    (spectrumIdentificationItem:SpectrumIdentificationItem) (calculatedPI:float) =
                    spectrumIdentificationItem.CalculatedPI <- calculatedPI
                    spectrumIdentificationItem

               static member addDetail
                    (spectrumIdentificationItem:SpectrumIdentificationItem) (detail:CVParam) =
                    let result = spectrumIdentificationItem.Details <- addToList spectrumIdentificationItem.Details detail
                    spectrumIdentificationItem

               static member addDetails
                    (spectrumIdentificationItem:SpectrumIdentificationItem) (details:seq<CVParam>) =
                    let result = spectrumIdentificationItem.Details <- addCollectionToList spectrumIdentificationItem.Details details
                    spectrumIdentificationItem

               //static member addSpectrumIdentificationResult
               //     (spectrumIdentificationItem:SpectrumIdentificationItem) (spectrumIdentificationResult:SpectrumIdentificationResult) =
               //     spectrumIdentificationItem.SpectrumIdentificationResult <- spectrumIdentificationResult

               static member findSpectrumIdentificationItemByID
                    (context:MzIdentMLContext) (spectrumIdentificationItemID:string) =
                    context.SpectrumIdentificationItem.Find(spectrumIdentificationItemID)

               static member findMassTableByID
                    (context:MzIdentMLContext) (massTableID:string) =
                    context.MassTable.Find(massTableID)

               static member findSampleByID
                    (context:MzIdentMLContext) (sampleID:string) =
                    context.Sample.Find(sampleID)

               static member findIontypeByID
                    (context:MzIdentMLContext) (ionTypeID:string) =
                    context.Iontype.Find(ionTypeID)

               static member findPeptideByID
                    (context:MzIdentMLContext) (peptideID:string) =
                    context.Peptide.Find(peptideID)

               static member findPeptideEvidenceByID
                    (context:MzIdentMLContext) (peptideEvidenceID:string) =
                    context.PeptideEvidence.Find(peptideEvidenceID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

                static member addToContext (context:MzIdentMLContext) (item:SpectrumIdentificationItem) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:SpectrumIdentificationItem) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type SpectrumIdentificationResultHandler =
               static member init
                    (
                        spectraData                 : SpectraData,
                        spectrumID                  : string,
                        spectrumIdentificationItem  : seq<SpectrumIdentificationItem>,
                        ?id                         : string,
                        ?name                       : string,
                        ?details                    : seq<CVParam>
                    ) =
                    let id'                         = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'                       = defaultArg name null
                    let details'                    = convertOptionToList details
                    {
                        SpectrumIdentificationResult.ID                         = id'
                        SpectrumIdentificationResult.Name                       = name'
                        SpectrumIdentificationResult.SpectraData                = spectraData
                        SpectrumIdentificationResult.SpectrumID                 = spectrumID
                        SpectrumIdentificationResult.SpectrumIdentificationItem = spectrumIdentificationItem |> List
                        SpectrumIdentificationResult.Details                    = details'
                        SpectrumIdentificationResult.RowVersion                 = DateTime.Now
                    }

               static member addName
                    (spectrumIdentificationResult:SpectrumIdentificationResult) (name:string) =
                    spectrumIdentificationResult.Name <- name
                    spectrumIdentificationResult

               static member addDetail
                    (spectrumIdentificationResult:SpectrumIdentificationResult) (detail:CVParam) =
                    let result = spectrumIdentificationResult.Details <- addToList spectrumIdentificationResult.Details detail
                    spectrumIdentificationResult

               static member addDetails
                    (spectrumIdentificationResult:SpectrumIdentificationResult) (details:seq<CVParam>) =
                    let result = spectrumIdentificationResult.Details <- addCollectionToList spectrumIdentificationResult.Details details
                    spectrumIdentificationResult

               //static member addSpectrumIdentificationList
               //     (spectrumIdentificationResult:SpectrumIdentificationResult) (spectrumIdentificationList:SpectrumIdentificationList) =
               //     spectrumIdentificationResult.SpectrumIdentificationList <- spectrumIdentificationList

               static member findSpectrumIdentificationResultByID
                    (context:MzIdentMLContext) (spectrumIdentificationResultID:string) =
                    context.SpectrumIdentificationResult.Find(spectrumIdentificationResultID)

               static member findSpectraDataByID
                    (context:MzIdentMLContext) (spectraDataID:string) =
                    context.SpectraData.Find(spectraDataID)

               static member findSpectrumIdentificationItemByID
                    (context:MzIdentMLContext) (spectrumIdentificationItemID:string) =
                    context.SpectrumIdentificationItem.Find(spectrumIdentificationItemID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

                static member addToContext (context:MzIdentMLContext) (item:SpectrumIdentificationResult) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:SpectrumIdentificationResult) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type SpectrumIdentificationListHandler =
               static member init
                    (
                        spectrumIdentificationResult : seq<SpectrumIdentificationResult>,
                        ?id                          : string,
                        ?name                        : string,
                        ?numSequencesSearched        : int64,
                        ?fragmentationTable          : seq<Measure>,
                        ?details                     : seq<CVParam>          
                    ) =
                    let id'                   = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'                 = defaultArg name null
                    let numSequencesSearched' = defaultArg numSequencesSearched Unchecked.defaultof<int64>
                    let fragmentationTable'   = convertOptionToList fragmentationTable
                    let details'              = convertOptionToList details
                    {
                        SpectrumIdentificationList.ID                           = id'
                        SpectrumIdentificationList.Name                         = name'
                        SpectrumIdentificationList.NumSequencesSearched         = numSequencesSearched'
                        SpectrumIdentificationList.FragmentationTables          = fragmentationTable'
                        SpectrumIdentificationList.SpectrumIdentificationResult = spectrumIdentificationResult |> List
                        SpectrumIdentificationList.Details                      = details'
                        SpectrumIdentificationList.RowVersion                   = DateTime.Now
                    }

               static member addName
                    (spectrumIdentificationList:SpectrumIdentificationList) (name:string) =
                    spectrumIdentificationList.Name <- name
                    spectrumIdentificationList

               static member addNumSequencesSearched
                    (spectrumIdentificationList:SpectrumIdentificationList) (numSequencesSearched:int64) =
                    spectrumIdentificationList.NumSequencesSearched <- numSequencesSearched
                    spectrumIdentificationList

               static member addFragmentationTable
                    (spectrumIdentificationList:SpectrumIdentificationList) (fragmentationTable:Measure) =
                    let result = spectrumIdentificationList.FragmentationTables <- addToList spectrumIdentificationList.FragmentationTables fragmentationTable
                    spectrumIdentificationList

               static member addFragmentationTables
                    (spectrumIdentificationList:SpectrumIdentificationList) (fragmentationTables:seq<Measure>) =
                    let result = spectrumIdentificationList.FragmentationTables <- addCollectionToList spectrumIdentificationList.FragmentationTables fragmentationTables
                    spectrumIdentificationList

               static member addDetail
                    (spectrumIdentificationList:SpectrumIdentificationList) (detail:CVParam) =
                    let result = spectrumIdentificationList.Details <- addToList spectrumIdentificationList.Details detail
                    spectrumIdentificationList

               static member addDetails
                    (spectrumIdentificationList:SpectrumIdentificationList) (details:seq<CVParam>) =
                    let result = spectrumIdentificationList.Details <- addCollectionToList spectrumIdentificationList.Details details
                    spectrumIdentificationList

               static member findSpectrumIdentificationListByID
                    (context:MzIdentMLContext) (spectrumIdentificationListID:string) =
                    context.SpectrumIdentificationList.Find(spectrumIdentificationListID)

               static member findSpectrumIdentificationResultByID
                    (context:MzIdentMLContext) (spectrumIdentificationResultID:string) =
                    context.SpectrumIdentificationResult.Find(spectrumIdentificationResultID)

               static member findMeasureByID
                    (context:MzIdentMLContext) (measureID:string) =
                    context.Measure.Find(measureID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

                static member addToContext (context:MzIdentMLContext) (item:SpectrumIdentificationList) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:SpectrumIdentificationList) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type SpectrumIdentificationHandler =
               static member init
                    (
                        spectrumIdentificationList     : SpectrumIdentificationList,
                        spectrumIdentificationProtocol : SpectrumIdentificationProtocol,
                        spectraData                    : seq<SpectraData>,
                        searchDatabase                 : seq<SearchDatabase>,
                        ?id                            : string,
                        ?name                          : string,
                        ?activityDate                  : DateTime
                    ) =
                    let id'               = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'             = defaultArg name null
                    let activityDate'     = defaultArg activityDate Unchecked.defaultof<DateTime>
                    {
                        SpectrumIdentification.ID                             = id'
                        SpectrumIdentification.Name                           = name'
                        SpectrumIdentification.ActivityDate                   = activityDate'
                        SpectrumIdentification.SpectrumIdentificationList     = spectrumIdentificationList
                        SpectrumIdentification.SpectrumIdentificationProtocol = spectrumIdentificationProtocol
                        SpectrumIdentification.SpectraData                    = spectraData |> List
                        SpectrumIdentification.SearchDatabase                 = searchDatabase |> List
                        SpectrumIdentification.RowVersion                     = DateTime.Now
                    }

               static member addName
                    (spectrumIdentificationList:SpectrumIdentification) (name:string) =
                    spectrumIdentificationList.Name <- name
                    spectrumIdentificationList

               static member addActivityDate
                    (spectrumIdentificationList:SpectrumIdentification) (activityDate:DateTime) =
                    spectrumIdentificationList.ActivityDate <- activityDate
                    spectrumIdentificationList

               //static member addProteinDetection
               //     (spectrumIdentificationList:SpectrumIdentification) (proteinDetection:ProteinDetection) =
               //     spectrumIdentificationList.ProteinDetection <- proteinDetection
               //     spectrumIdentificationList

               //static member addMzIdentML
               //     (spectrumIdentificationList:SpectrumIdentification) (mzIdentML:MzIdentML) =
               //     spectrumIdentificationList.MzIdentML <- mzIdentML

               static member findSpectrumIdentificationByID
                    (context:MzIdentMLContext) (spectrumIdentificationID:string) =
                    context.SpectrumIdentification.Find(spectrumIdentificationID)

               static member findSpectrumIdentificationListByID
                    (context:MzIdentMLContext) (spectrumIdentificationListID:string) =
                    context.SpectrumIdentificationList.Find(spectrumIdentificationListID)

               static member findSpectrumIdentificationProtocolByID
                    (context:MzIdentMLContext) (spectrumIdentificationProtocolID:string) =
                    context.SpectrumIdentificationProtocol.Find(spectrumIdentificationProtocolID)

               static member findSpectraDataByID
                    (context:MzIdentMLContext) (spectraDataID:string) =
                    context.SpectraData.Find(spectraDataID)

               static member findSearchDatabaseByID
                    (context:MzIdentMLContext) (searchDatabaseID:string) =
                    context.SearchDatabase.Find(searchDatabaseID)

                static member addToContext (context:MzIdentMLContext) (item:SpectrumIdentification) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:SpectrumIdentification) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type ProteinDetectionProtocolHandler =
               static member init
                    (
                        analysisSoftware : AnalysisSoftware,
                        threshold        : seq<CVParam>,
                        ?id              : string,
                        ?name            : string,
                        ?analysisParams  : seq<CVParam>
                    ) =
                    let id'             = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'           = defaultArg name null
                    let analysisParams' = convertOptionToList analysisParams
                    {
                        ProteinDetectionProtocol.ID               = id'
                        ProteinDetectionProtocol.Name             = name'
                        ProteinDetectionProtocol.AnalysisSoftware = analysisSoftware
                        ProteinDetectionProtocol.AnalysisParams   = analysisParams'
                        ProteinDetectionProtocol.Threshold        = threshold |> List
                        ProteinDetectionProtocol.RowVersion       = DateTime.Now
                    }

               static member addName
                    (proteinDetectionProtocol:ProteinDetectionProtocol) (name:string) =
                    proteinDetectionProtocol.Name <- name
                    proteinDetectionProtocol

               static member addAnalysisParam
                    (proteinDetectionProtocol:ProteinDetectionProtocol) (analysisParam:CVParam) =
                    let result = proteinDetectionProtocol.AnalysisParams <- addToList proteinDetectionProtocol.AnalysisParams analysisParam
                    proteinDetectionProtocol

               static member addAnalysisParams
                    (proteinDetectionProtocol:ProteinDetectionProtocol) (analysisParams:seq<CVParam>) =
                    let result = proteinDetectionProtocol.AnalysisParams <- addCollectionToList proteinDetectionProtocol.AnalysisParams analysisParams
                    proteinDetectionProtocol

               static member findProteinDetectionProtocolByID
                    (context:MzIdentMLContext) (proteinDetectionProtocolID:string) =
                    context.ProteinDetectionProtocol.Find(proteinDetectionProtocolID)

               static member findAnalysisSoftwareByID
                    (context:MzIdentMLContext) (analysisSoftwareID:string) =
                    context.AnalysisSoftware.Find(analysisSoftwareID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

                static member addToContext (context:MzIdentMLContext) (item:ProteinDetectionProtocol) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:ProteinDetectionProtocol) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type SourceFileHandler =
               static member init
                    (             
                        location                     : string,
                        fileFormat                   : CVParam,
                        ?id                          : string,
                        ?name                        : string,
                        ?externalFormatDocumentation : string,
                        ?details                     : seq<CVParam>
                    ) =
                    let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'                        = defaultArg name null
                    let externalFormatDocumentation' = defaultArg externalFormatDocumentation null
                    let details'                     = convertOptionToList details
                    {
                        SourceFile.ID                          = id'
                        SourceFile.Name                        = name'
                        SourceFile.Location                    = location
                        SourceFile.ExternalFormatDocumentation = externalFormatDocumentation'
                        SourceFile.FileFormat                  = fileFormat
                        SourceFile.Details                     = details'
                        SourceFile.RowVersion                  = DateTime.Now
                    }

               static member addName
                    (sourceFile:SourceFile) (name:string) =
                    sourceFile.Name <- name
                    sourceFile

               static member addExternalFormatDocumentation
                    (sourceFile:SourceFile) (externalFormatDocumentation:string) =
                    sourceFile.ExternalFormatDocumentation <- externalFormatDocumentation
                    sourceFile

               static member addDetail
                    (sourceFile:SourceFile) (detail:CVParam) =
                    let result = sourceFile.Details <- addToList sourceFile.Details detail
                    sourceFile

               static member addDetails
                    (sourceFile:SourceFile) (details:seq<CVParam>) =
                    let result = sourceFile.Details <- addCollectionToList sourceFile.Details details
                    sourceFile

               //static member addInputs
               //     (sourceFile:SourceFile) (inputs:Inputs) =
               //     sourceFile.Inputs <- inputs

               static member findSourceFileByID
                    (context:MzIdentMLContext) (sourceFileID:string) =
                    context.SourceFile.Find(sourceFileID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

                static member addToContext (context:MzIdentMLContext) (item:SourceFile) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:SourceFile) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type InputsHandler =
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
                    {
                        Inputs.ID              = id'
                        Inputs.SourceFiles     = sourceFile'
                        Inputs.SearchDatabases = searchDatabase'
                        Inputs.SpectraData     = spectraData |> List
                        Inputs.RowVersion      = DateTime.Now
                    }

               static member addSourceFile
                    (inputs:Inputs) (sourceFile:SourceFile) =
                    let result = inputs.SourceFiles <- addToList inputs.SourceFiles sourceFile
                    inputs

               static member addSourceFiles
                    (inputs:Inputs) (sourceFiles:seq<SourceFile>) =
                    let result = inputs.SourceFiles <- addCollectionToList inputs.SourceFiles sourceFiles
                    inputs

               static member addSearchDatabase
                    (inputs:Inputs) (searchDatabase:SearchDatabase) =
                    let result = inputs.SearchDatabases <- addToList inputs.SearchDatabases searchDatabase
                    inputs

               static member addSearchDatabases
                    (inputs:Inputs) (searchDatabases:seq<SearchDatabase>) =
                    let result = inputs.SearchDatabases <- addCollectionToList inputs.SearchDatabases searchDatabases
                    inputs

               static member findInputsID
                    (context:MzIdentMLContext) (inputsID:string) =
                    context.Inputs.Find(inputsID)

               static member findSpectraDataByID
                    (context:MzIdentMLContext) (spectraDataID:string) =
                    context.SpectraData.Find(spectraDataID)

               static member findSourceFileByID
                    (context:MzIdentMLContext) (sourceFileID:string) =
                    context.SourceFile.Find(sourceFileID)

               static member findSearchDatabaseByID
                    (context:MzIdentMLContext) (searchDatabaseID:string) =
                    context.SearchDatabase.Find(searchDatabaseID)

                static member addToContext (context:MzIdentMLContext) (item:Inputs) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:Inputs) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type PeptideHypothesisHandler =
               static member init
                    (              
                        peptideEvidence             : PeptideEvidence,
                        spectrumIdentificationItems : seq<SpectrumIdentificationItem>,
                        ?id                         : string
                    ) =
                    let id' = defaultArg id (System.Guid.NewGuid().ToString())
                    {
                        PeptideHypothesis.ID                          = id'
                        PeptideHypothesis.PeptideEvidence             = peptideEvidence
                        PeptideHypothesis.SpectrumIdentificationItems = spectrumIdentificationItems |> List
                        PeptideHypothesis.RowVersion                  = DateTime.Now
                    }

               static member findPeptideHypothesisByID
                    (context:MzIdentMLContext) (peptideHypothesisID:string) =
                    context.PeptideHypothesis.Find(peptideHypothesisID)

               static member findPeptideEvidenceByID
                    (context:MzIdentMLContext) (peptideEvidenceID:string) =
                    context.PeptideEvidence.Find(peptideEvidenceID)

               static member findSpectrumIdentificationItemByID
                    (context:MzIdentMLContext) (spectrumIdentificationItemID:string) =
                    context.SpectrumIdentificationItem.Find(spectrumIdentificationItemID)

                static member addToContext (context:MzIdentMLContext) (item:PeptideHypothesis) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:PeptideHypothesis) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type ProteinDetectionHypothesisHandler =
               static member init
                    (             
                        passThreshold     : bool,
                        dbSequence        : DBSequence,
                        peptideHypothesis : seq<PeptideHypothesis>,
                        ?id               : string,
                        ?name             : string,
                        ?details          : seq<CVParam>
                    ) =
                    let id'      = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'    = defaultArg name null
                    let details' = convertOptionToList details
                    {
                        ProteinDetectionHypothesis.ID                = id'
                        ProteinDetectionHypothesis.Name              = name'
                        ProteinDetectionHypothesis.PassThreshold     = passThreshold
                        ProteinDetectionHypothesis.DBSequence        = dbSequence
                        ProteinDetectionHypothesis.PeptideHypothesis = peptideHypothesis |> List
                        ProteinDetectionHypothesis.Details           = details'
                        ProteinDetectionHypothesis.RowVersion        = DateTime.Now
                    }

               static member addName
                    (proteinDetectionHypothesis:ProteinDetectionHypothesis) (name:string) =
                    proteinDetectionHypothesis.Name <- name
                    proteinDetectionHypothesis

               static member addDetail
                    (proteinDetectionHypothesis:ProteinDetectionHypothesis) (detail:CVParam) =
                    let result = proteinDetectionHypothesis.Details <- addToList proteinDetectionHypothesis.Details detail
                    proteinDetectionHypothesis

               static member addDetails
                    (proteinDetectionHypothesis:ProteinDetectionHypothesis) (details:seq<CVParam>) =
                    let result = proteinDetectionHypothesis.Details <- addCollectionToList proteinDetectionHypothesis.Details details
                    proteinDetectionHypothesis

               static member findProteinDetectionHypothesisByID
                    (context:MzIdentMLContext) (proteinDetectionHypothesisID:string) =
                    context.ProteinDetectionHypothesis.Find(proteinDetectionHypothesisID)

               static member findDBSequenceByID
                    (context:MzIdentMLContext) (dbSequenceID:string) =
                    context.DBSequence.Find(dbSequenceID)

               static member findPeptideHypothesisByID
                    (context:MzIdentMLContext) (peptideHypothesisID:string) =
                    context.PeptideHypothesis.Find(peptideHypothesisID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

                static member addToContext (context:MzIdentMLContext) (item:ProteinDetectionHypothesis) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:ProteinDetectionHypothesis) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type ProteinAmbiguityGroupHandler =
               static member init
                    (             
                        proteinDetecionHypothesis : seq<ProteinDetectionHypothesis>,
                        ?id                       : string,
                        ?name                     : string,
                        ?details                  : seq<CVParam>
                    ) =
                    let id'                          = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'                        = defaultArg name null
                    let details'                     = convertOptionToList details
                    {
                        ProteinAmbiguityGroup.ID                        = id'
                        ProteinAmbiguityGroup.Name                      = name'
                        ProteinAmbiguityGroup.ProteinDetecionHypothesis = proteinDetecionHypothesis |> List
                        ProteinAmbiguityGroup.Details                   = details'
                        ProteinAmbiguityGroup.RowVersion                = DateTime.Now
                    }

               static member addName
                    (proteinAmbiguityGroup:ProteinAmbiguityGroup) (name:string) =
                    proteinAmbiguityGroup.Name <- name
                    proteinAmbiguityGroup

               static member addDetail
                    (proteinAmbiguityGroup:ProteinAmbiguityGroup) (detail:CVParam) =
                    let result = proteinAmbiguityGroup.Details <- addToList proteinAmbiguityGroup.Details detail
                    proteinAmbiguityGroup

               static member addDetails
                    (proteinAmbiguityGroup:ProteinAmbiguityGroup) (details:seq<CVParam>) =
                    let result = proteinAmbiguityGroup.Details <- addCollectionToList proteinAmbiguityGroup.Details details
                    proteinAmbiguityGroup

               static member findProteinAmbiguityGroupByID
                    (context:MzIdentMLContext) (proteinAmbiguityGroupID:string) =
                    context.ProteinAmbiguityGroup.Find(proteinAmbiguityGroupID)

               static member findProteinDetectionHypothesisByID
                    (context:MzIdentMLContext) (proteinDetectionHypothesisID:string) =
                    context.ProteinDetectionHypothesis.Find(proteinDetectionHypothesisID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

                static member addToContext (context:MzIdentMLContext) (item:ProteinAmbiguityGroup) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:ProteinAmbiguityGroup) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type ProteinDetectionListHandler =
               static member init
                    (             
                        ?id                     : string,
                        ?name                   : string,
                        ?proteinAmbiguityGroups : seq<ProteinAmbiguityGroup>,
                        ?details                : seq<CVParam>
                    ) =
                    let id'                     = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'                   = defaultArg name null
                    let proteinAmbiguityGroups' = convertOptionToList proteinAmbiguityGroups
                    let details'                = convertOptionToList details
                    {
                        ProteinDetectionList.ID                     = id'
                        ProteinDetectionList.Name                   = name'
                        ProteinDetectionList.ProteinAmbiguityGroups = proteinAmbiguityGroups'
                        ProteinDetectionList.Details                = details'
                        ProteinDetectionList.RowVersion             = DateTime.Now
                    }

               static member addName
                    (proteinDetectionList:ProteinDetectionList) (name:string) =
                    proteinDetectionList.Name <- name
                    proteinDetectionList

               static member addProteinAmbiguityGroup
                    (proteinDetectionList:ProteinDetectionList) (proteinAmbiguityGroup:ProteinAmbiguityGroup) =
                    let result = proteinDetectionList.ProteinAmbiguityGroups <- addToList proteinDetectionList.ProteinAmbiguityGroups proteinAmbiguityGroup
                    proteinDetectionList

               static member addProteinAmbiguityGroups
                    (proteinDetectionList:ProteinDetectionList) (proteinAmbiguityGroups:seq<ProteinAmbiguityGroup>) =
                    let result = proteinDetectionList.ProteinAmbiguityGroups <- addCollectionToList proteinDetectionList.ProteinAmbiguityGroups proteinAmbiguityGroups
                    proteinDetectionList

               static member addDetail
                    (proteinDetectionList:ProteinDetectionList) (detail:CVParam) =
                    let result = proteinDetectionList.Details <- addToList proteinDetectionList.Details detail
                    proteinDetectionList

               static member addDetails
                    (proteinDetectionList:ProteinDetectionList) (details:seq<CVParam>) =
                    let result = proteinDetectionList.Details <- addCollectionToList proteinDetectionList.Details details
                    proteinDetectionList

               static member findProteinDetectionListByID
                    (context:MzIdentMLContext) (proteinDetectionListID:string) =
                    context.ProteinDetectionList.Find(proteinDetectionListID)

               static member findProteinAmbiguityGroupByID
                    (context:MzIdentMLContext) (proteinAmbiguityGroupID:string) =
                    context.ProteinAmbiguityGroup.Find(proteinAmbiguityGroupID)

               static member findCVParamByID
                    (context:MzIdentMLContext) (cvParamID:string) =
                    context.CVParam.Find(cvParamID)

                static member addToContext (context:MzIdentMLContext) (item:ProteinDetectionList) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:ProteinDetectionList) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type AnalysisDataHandler =
               static member init
                    (             
                        spectrumIdentificationList : seq<SpectrumIdentificationList>,
                        ?id                        : string,
                        ?proteinDetectionList      : ProteinDetectionList
                    ) =
                    let id'                   = defaultArg id (System.Guid.NewGuid().ToString())
                    let proteinDetectionList' = defaultArg proteinDetectionList Unchecked.defaultof<ProteinDetectionList>
                    {
                        AnalysisData.ID                         = id'
                        AnalysisData.SpectrumIdentificationList = spectrumIdentificationList |> List
                        AnalysisData.ProteinDetectionList       = proteinDetectionList'
                        AnalysisData.RowVersion                 = DateTime.Now
                    }

               static member addProteinDetectionList
                    (analysisData:AnalysisData) (proteinDetectionList:ProteinDetectionList) =
                    analysisData.ProteinDetectionList <- proteinDetectionList
                    analysisData

               static member findAnalysisDataByID
                    (context:MzIdentMLContext) (analysisDataID:string) =
                    context.AnalysisData.Find(analysisDataID)

               static member findSpectrumIdentificationListByID
                    (context:MzIdentMLContext) (spectrumIdentificationListID:string) =
                    context.SpectrumIdentificationList.Find(spectrumIdentificationListID)

               static member findProteinDetectionListByID
                    (context:MzIdentMLContext) (proteinDetectionListID:string) =
                    context.ProteinDetectionList.Find(proteinDetectionListID)

                static member addToContext (context:MzIdentMLContext) (item:AnalysisData) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:AnalysisData) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type ProteinDetectionHandler =
               static member init
                    (             
                        proteinDetectionList     : ProteinDetectionList,
                        proteinDetectionProtocol : ProteinDetectionProtocol,
                        spectrumIdentifications  : seq<SpectrumIdentification>,
                        ?id                      : string,
                        ?name                    : string,
                        ?activityDate            : DateTime
                    ) =
                    let id'           = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'         = defaultArg name null
                    let activityDate' = defaultArg activityDate Unchecked.defaultof<DateTime>
                    {
                        ProteinDetection.ID                       = id'
                        ProteinDetection.Name                     = name'
                        ProteinDetection.ActivityDate             = activityDate'
                        ProteinDetection.ProteinDetectionList     = proteinDetectionList
                        ProteinDetection.ProteinDetectionProtocol = proteinDetectionProtocol
                        ProteinDetection.SpectrumIdentifications  = spectrumIdentifications |> List
                        ProteinDetection.RowVersion               = DateTime.Now
                    }

               static member addName
                    (proteinDetection:ProteinDetection) (name:string) =
                    proteinDetection.Name <- name
                    proteinDetection

               static member addActivityDate
                    (proteinDetection:ProteinDetection) (activityDate:DateTime) =
                    proteinDetection.ActivityDate <- activityDate
                    proteinDetection

               static member findProteinDetectionByID
                    (context:MzIdentMLContext) (proteinDetectionID:string) =
                    context.ProteinDetection.Find(proteinDetectionID)

               static member findProteinDetectionListByID
                    (context:MzIdentMLContext) (proteinDetectionListID:string) =
                    context.ProteinDetectionList.Find(proteinDetectionListID)

               static member findProteinDetectionProtocolByID
                    (context:MzIdentMLContext) (proteinDetectionProtocolID:string) =
                    context.ProteinDetectionProtocol.Find(proteinDetectionProtocolID)

               static member findSpectrumIdentificationByID
                    (context:MzIdentMLContext) (spectrumIdentificationID:string) =
                    context.SpectrumIdentification.Find(spectrumIdentificationID)

                static member addToContext (context:MzIdentMLContext) (item:ProteinDetection) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:ProteinDetection) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

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
                        ?year        : int
                    ) =
                    let id'          = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'        = defaultArg name null
                    let authors'     = defaultArg authors null
                    let doi'         = defaultArg doi null
                    let editor'      = defaultArg editor null
                    let issue'       = defaultArg issue null
                    let pages'       = defaultArg pages null
                    let publication' = defaultArg publication null
                    let publisher'   = defaultArg publisher null
                    let title'       = defaultArg title null
                    let volume'      = defaultArg volume null
                    let year'        = defaultArg year Unchecked.defaultof<int>
                    {
                        BiblioGraphicReference.ID          = id'
                        BiblioGraphicReference.Name        = name'
                        BiblioGraphicReference.Authors     = authors'
                        BiblioGraphicReference.DOI         = doi'
                        BiblioGraphicReference.Editor      = editor'
                        BiblioGraphicReference.Issue       = issue'
                        BiblioGraphicReference.Pages       = pages'
                        BiblioGraphicReference.Publication = publication'
                        BiblioGraphicReference.Publisher   = publisher'
                        BiblioGraphicReference.Title       = title'
                        BiblioGraphicReference.Volume      = volume'
                        BiblioGraphicReference.Year        = year'
                        BiblioGraphicReference.RowVersion  = DateTime.Now
                    }

               static member addName
                    (biblioGraphicReference:BiblioGraphicReference) (name:string) =
                    biblioGraphicReference.Name <- name
                    biblioGraphicReference

               static member addAuthors
                    (biblioGraphicReference:BiblioGraphicReference) (authors:string) =
                    biblioGraphicReference.Authors <- authors
                    biblioGraphicReference

               static member addDOI
                    (biblioGraphicReference:BiblioGraphicReference) (doi:string) =
                    biblioGraphicReference.DOI <- doi
                    biblioGraphicReference

               static member addEditor
                    (biblioGraphicReference:BiblioGraphicReference) (editor:string) =
                    biblioGraphicReference.Editor <- editor
                    biblioGraphicReference

               static member addIssue
                    (biblioGraphicReference:BiblioGraphicReference) (issue:string) =
                    biblioGraphicReference.Issue <- issue
                    biblioGraphicReference

               static member addPages
                    (biblioGraphicReference:BiblioGraphicReference) (pages:string) =
                    biblioGraphicReference.Pages <- pages
                    biblioGraphicReference

               static member addPublication
                    (biblioGraphicReference:BiblioGraphicReference) (publication:string) =
                    biblioGraphicReference.Publication <- publication
                    biblioGraphicReference

               static member addPublisher
                    (biblioGraphicReference:BiblioGraphicReference) (publisher:string) =
                    biblioGraphicReference.Publisher <- publisher
                    biblioGraphicReference

               static member addTitle
                    (biblioGraphicReference:BiblioGraphicReference) (title:string) =
                    biblioGraphicReference.Title <- title
                    biblioGraphicReference

               static member addVolume
                    (biblioGraphicReference:BiblioGraphicReference) (volume:string) =
                    biblioGraphicReference.Volume <- volume
                    biblioGraphicReference

               static member addYear
                    (biblioGraphicReference:BiblioGraphicReference) (year:int) =
                    biblioGraphicReference.Year <- year
                    biblioGraphicReference

               //static member addMzIdentML
               //     (biblioGraphicReference:BiblioGraphicReference) (mzIdentML:MzIdentML) =
               //     biblioGraphicReference.MzIdentML <- mzIdentML
               //     biblioGraphicReference

               static member findBiblioGraphicReferenceByID
                    (context:MzIdentMLContext) (biblioGraphicReferenceID:string) =
                    context.BiblioGraphicReference.Find(biblioGraphicReferenceID)

                static member addToContext (context:MzIdentMLContext) (item:BiblioGraphicReference) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:BiblioGraphicReference) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type ProviderHandler =
               static member init
                    (             
                        ?id               : string,
                        ?name             : string,
                        ?analysisSoftware : AnalysisSoftware,
                        ?contactRole      : ContactRole
                    ) =
                    let id'               = defaultArg id (System.Guid.NewGuid().ToString())
                    let name'             = defaultArg name null
                    let analysisSoftware' = defaultArg analysisSoftware Unchecked.defaultof<AnalysisSoftware>
                    let contactRole'      = defaultArg contactRole Unchecked.defaultof<ContactRole>
                    {
                        Provider.ID               = id'
                        Provider.Name             = name'
                        Provider.AnalysisSoftware = analysisSoftware'
                        Provider.ContactRole      = contactRole'
                        Provider.RowVersion       = DateTime.Now
                    }

               static member addName
                    (provider:Provider) (name:string) =
                    provider.Name <- name
                    provider

               static member addAnalysisSoftware
                    (provider:Provider) (analysisSoftware:AnalysisSoftware) =
                    provider.AnalysisSoftware <- analysisSoftware
                    provider

               static member addContactRole
                    (provider:Provider) (contactRole:ContactRole) =
                    provider.ContactRole <- contactRole
                    provider

               static member findProviderByID
                    (context:MzIdentMLContext) (providerID:string) =
                    context.Provider.Find(providerID)

               static member findAnalysisSoftwareRoleByID
                    (context:MzIdentMLContext) (analysisSoftwareID:string) =
                    context.AnalysisSoftware.Find(analysisSoftwareID)

               static member findContactRoleByID
                    (context:MzIdentMLContext) (contactRoleID:string) =
                    context.ContactRole.Find(contactRoleID)

                static member addToContext (context:MzIdentMLContext) (item:Provider) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:Provider) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

        type MzIdentMLHandler =
               static member init
                    (             
                    
                        version                        : string,
                        ontologies                     : seq<Ontology>,
                        spectrumIdentification         : seq<SpectrumIdentification>,
                        spectrumIdentificationProtocol : seq<SpectrumIdentificationProtocol>,
                        inputs                         : Inputs,
                        analysisData                   : AnalysisData,
                        ?id                            : int,
                        ?name                          : string,
                        ?analysisSoftwares             : seq<AnalysisSoftware>,
                        ?provider                      : Provider,
                        ?person                        : Person,
                        ?organization                  : Organization,
                        ?samples                       : seq<Sample>,
                        ?dbSequences                   : seq<DBSequence>,
                        ?peptides                      : seq<Peptide>,
                        ?peptideEvidences              : seq<PeptideEvidence>,
                        ?proteinDetection              : ProteinDetection,
                        ?proteinDetectionProtocol      : ProteinDetectionProtocol,
                        ?biblioGraphicReferences       : seq<BiblioGraphicReference>
                    ) =
                    let id'                       = defaultArg id 0
                    let name'                     = defaultArg name null
                    let analysisSoftwares'        = convertOptionToList analysisSoftwares
                    let provider'                 = defaultArg provider Unchecked.defaultof<Provider>
                    let person'                   = defaultArg person Unchecked.defaultof<Person>
                    let organization'             = defaultArg organization Unchecked.defaultof<Organization>
                    let samples'                  = convertOptionToList samples
                    let dbSequences'              = convertOptionToList dbSequences
                    let peptides'                 = convertOptionToList peptides
                    let peptideEvidences'         = convertOptionToList peptideEvidences
                    let proteinDetection'         = defaultArg proteinDetection Unchecked.defaultof<ProteinDetection>
                    let proteinDetectionProtocol' = defaultArg proteinDetectionProtocol Unchecked.defaultof<ProteinDetectionProtocol>
                    let biblioGraphicReferences'  = convertOptionToList biblioGraphicReferences
                    {
                        MzIdentML.ID                             = id'
                        MzIdentML.Name                           = name'
                        MzIdentML.Version                        = version
                        MzIdentML.Ontologies                     = ontologies |> List
                        MzIdentML.AnalysisSoftwares              = analysisSoftwares'
                        MzIdentML.Provider                       = provider'
                        MzIdentML.Person                         = person'
                        MzIdentML.Organization                   = organization'
                        MzIdentML.Samples                        = samples'
                        MzIdentML.DBSequences                    = dbSequences'
                        MzIdentML.Peptides                       = peptides'
                        MzIdentML.PeptideEvidences               = peptideEvidences'
                        MzIdentML.SpectrumIdentification         = spectrumIdentification |> List
                        MzIdentML.ProteinDetection               = proteinDetection'
                        MzIdentML.SpectrumIdentificationProtocol = spectrumIdentificationProtocol |> List
                        MzIdentML.ProteinDetectionProtocol       = proteinDetectionProtocol'
                        MzIdentML.Inputs                         = inputs
                        MzIdentML.AnalysisData                   = analysisData
                        MzIdentML.BiblioGraphicReferences        = biblioGraphicReferences' |> List
                        MzIdentML.RowVersion                     = DateTime.Now
                    }

               static member addName
                    (mzIdentML:MzIdentML) (name:string) =
                    mzIdentML.Name <- name

               static member addAnalysisSoftware
                    (mzIdentML:MzIdentML) (analysisSoftware:AnalysisSoftware) =
                    let result = mzIdentML.AnalysisSoftwares <- addToList mzIdentML.AnalysisSoftwares analysisSoftware
                    mzIdentML

               static member addAnalysisSoftwares
                    (mzIdentML:MzIdentML) (analysisSoftwares:seq<AnalysisSoftware>) =
                    let result = mzIdentML.AnalysisSoftwares <- addCollectionToList mzIdentML.AnalysisSoftwares analysisSoftwares
                    mzIdentML

               static member addProvider
                    (mzIdentML:MzIdentML) (provider:Provider) =
                    mzIdentML.Provider <- provider

               static member addPerson
                    (mzIdentML:MzIdentML) (person:Person) =
                    mzIdentML.Person <- person

               static member addOrganization
                    (mzIdentML:MzIdentML) (organization:Organization) =
                    mzIdentML.Organization <- organization

               static member addSample
                    (mzIdentML:MzIdentML) (sample:Sample) =
                    let result = mzIdentML.Samples <- addToList mzIdentML.Samples sample
                    mzIdentML

               static member addSamples
                    (mzIdentML:MzIdentML) (samples:seq<Sample>) =
                    let result = mzIdentML.Samples <- addCollectionToList mzIdentML.Samples samples
                    mzIdentML

               static member addDBSequence
                    (mzIdentML:MzIdentML) (dbSequence:DBSequence) =
                    let result = mzIdentML.DBSequences <- addToList mzIdentML.DBSequences dbSequence
                    mzIdentML

               static member addDBSequences
                    (mzIdentML:MzIdentML) (dbSequences:seq<DBSequence>) =
                    let result = mzIdentML.DBSequences <- addCollectionToList mzIdentML.DBSequences dbSequences
                    mzIdentML

               static member addPeptide
                    (mzIdentML:MzIdentML) (peptide:Peptide) =
                    let result = mzIdentML.Peptides <- addToList mzIdentML.Peptides peptide
                    mzIdentML

               static member addPeptides
                    (mzIdentML:MzIdentML) (peptides:seq<Peptide>) =
                    let result = mzIdentML.Peptides <- addCollectionToList mzIdentML.Peptides peptides
                    mzIdentML

               static member addPeptideEvidence
                    (mzIdentML:MzIdentML) (peptideEvidence:PeptideEvidence) =
                    let result = mzIdentML.PeptideEvidences <- addToList mzIdentML.PeptideEvidences peptideEvidence
                    mzIdentML

               static member addPeptideEvidences
                    (mzIdentML:MzIdentML) (peptideEvidences:seq<PeptideEvidence>) =
                    let result = mzIdentML.PeptideEvidences <- addCollectionToList mzIdentML.PeptideEvidences peptideEvidences
                    mzIdentML

               static member addProteinDetection
                    (mzIdentML:MzIdentML) (proteinDetection:ProteinDetection) =
                    mzIdentML.ProteinDetection <- proteinDetection

               static member addProteinDetectionProtocol
                    (mzIdentML:MzIdentML) (proteinDetectionProtocol:ProteinDetectionProtocol) =
                    mzIdentML.ProteinDetectionProtocol <- proteinDetectionProtocol

               static member addBiblioGraphicReference
                    (mzIdentML:MzIdentML) (biblioGraphicReference:BiblioGraphicReference) =
                    let result = mzIdentML.BiblioGraphicReferences <- addToList mzIdentML.BiblioGraphicReferences biblioGraphicReference
                    mzIdentML

               static member addBiblioGraphicReferences
                    (mzIdentML:MzIdentML) (biblioGraphicReferences:seq<BiblioGraphicReference>) =
                    let result = mzIdentML.BiblioGraphicReferences <- addCollectionToList mzIdentML.BiblioGraphicReferences biblioGraphicReferences
                    mzIdentML

                static member addToContext (context:MzIdentMLContext) (item:MzIdentML) =
                        (addToContextWithExceptionCheck context item)

                static member addToContextAndInsert (context:MzIdentMLContext) (item:MzIdentML) =
                        (addToContextWithExceptionCheck context item) |> ignore
                        insertWithExceptionCheck context

    module InitializeStandardDB =
        open BioFSharp.IO
        open FSharp.Care.IO
        open ObjectHandlers
        open ManipulateDataContextAndDB

        ///Define reader for OboFile
        let fromFileObo (filePath : string) =
            FileIO.readFile filePath
            |> Obo.parseOboTerms

        let fromPsiMS =
            fromFileObo (fileDir + "\Ontologies\Psi-MS.txt")

        let fromPride =
            fromFileObo (fileDir + "\Ontologies\Pride.txt")

        let fromUniMod =
            fromFileObo (fileDir + "\Ontologies\Unimod.txt")

        let fromUnit_Ontology =
            fromFileObo (fileDir + "\Ontologies\Unit_Ontology.txt")

        let initStandardDB (context : MzIdentMLContext) =
            let terms_PsiMS =
                let psims = OntologyHandler.init ("Psi-MS")
                OntologyHandler.addToContext context psims |> ignore 
                fromPsiMS
                |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, (context.Ontology.Find(psims.ID))))
                |> Seq.toArray
            terms_PsiMS
            |> Array.map (fun term -> TermHandler.addToContext context term) |> ignore

            let terms_Pride =
                let pride = OntologyHandler.init ("Pride")
                OntologyHandler.addToContext context pride |> ignore 
                fromPride
                |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, (context.Ontology.Find(pride.ID))))
                |> Seq.toArray
            terms_Pride
            |> Array.map (fun term -> TermHandler.addToContext context term) |> ignore

            let terms_Unimod =
                let unimod = OntologyHandler.init ("Unimod")
                OntologyHandler.addToContext context unimod |> ignore 
                fromUniMod
                |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, (context.Ontology.Find(unimod.ID))))
                |> Seq.toArray
            terms_Unimod
            |> Array.map (fun term -> TermHandler.addToContext context term) |> ignore

            let terms_Unit_Ontology =
                let unit_ontology = OntologyHandler.init ("Unit_Ontology")
                OntologyHandler.addToContext context unit_ontology |> ignore 
                fromUnit_Ontology
                |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, (context.Ontology.Find(unit_ontology.ID))))
                |> Seq.toArray
            terms_Unit_Ontology
            |> Array.map (fun term -> TermHandler.addToContext context term) |> ignore

            let userOntology =
                    OntologyHandler.init("UserParam")
            OntologyHandler.addToContext context userOntology |> ignore

            context.Database.EnsureCreated() |> ignore
            context.SaveChanges()

    //module test =
    //    ////Apply functions
        
    //    //#r "System.ComponentModel.DataAnnotations.dll"
    //    #r @"C:\Users\PatrickB\Source\Repos\MzIdentMLDB_Library\MzIdentMLDB_Library\bin\Debug\netstandard.dll"
    //    //#r @"C:\Users\PatrickB\Source\Repos\MzIdentML_Library\MzIdentML_Library\bin\Debug\netstandard2.0\BioFSharp.dll"
    //    //#r @"C:\Users\PatrickB\Source\Repos\MzIdentML_Library\MzIdentML_Library\bin\Debug\netstandard2.0\BioFSharp.IO.dll"
    //    #r @"C:\Users\PatrickB\Source\Repos\MzIdentMLDB_Library\MzIdentMLDB_Library\bin\Debug\Microsoft.EntityFrameworkCore.dll"
    //    #r @"C:\Users\PatrickB\Source\Repos\MzIdentMLDB_Library\MzIdentMLDB_Library\bin\Debug\Microsoft.EntityFrameworkCore.Relational.dll"
    //    #r @"C:\Users\PatrickB\Source\Repos\MzIdentMLDB_Library\MzIdentMLDB_Library\bin\Debug\Microsoft.EntityFrameworkCore.Sqlite.dll"
    //    #r @"C:\Users\PatrickB\Source\Repos\MzIdentMLDB_Library\MzIdentMLDB_Library\bin\Debug\SQLitePCLRaw.batteries_v2.dll"
    //    #r @"C:\Users\PatrickB\Source\Repos\MzIdentMLDB_Library\MzIdentMLDB_Library\bin\Debug\Remotion.Linq.dll"
    //    #r @"C:\Users\PatrickB\Source\Repos\MzIdentMLDB_Library\MzIdentMLDB_Library\bin\Debug\SQLitePCLRaw.core.dll"
    //    //#r @"C:\Users\PatrickB\Source\Repos\MzIdentML_Library\MzIdentML_Library\bin\Debug\netstandard2.0\FSharp.Care.dll"
    //    //#r @"C:\Users\PatrickB\Source\Repos\MzIdentML_Library\MzIdentML_Library\bin\Debug\netstandard2.0\FSharp.Care.IO.dll"
    //    #r @"C:\Users\PatrickB\Source\Repos\MZIdentMLDB\packages\FSharp.Data.2.4.6\lib\net45\FSharp.Data.dll"
    //    #r @"C:\Users\PatrickB\Source\Repos\MZIdentMLDB\packages\FSharp.Data.Xsd.1.0.2\lib\net45\FSharp.Data.Xsd.dll"
    //    #r "System.Xml.Linq.dll"
    //    #r @"C:\Users\PatrickB\Source\Repos\MzIdentMLDB_Library\MzIdentMLDB_Library\bin\Debug\MzIdentMLDB_Library.dll"

    //    //    open ObjectHandlers
    //    //    open ManipulateDataContextAndDB
    //    //    open InitializeStandardDB

    //    let context = configureSQLiteContextMzIdentML "C:\Users\PatrickB\Source\Repos\Test.db"

    //    //Term and Ontology
    //    let termI = TermHandler.init("I")
    //    let termII = TermHandler.addName termI "Test"
    //    let ontologyI = OntologyHandler.init("I")
    //    let termIII = TermHandler.addOntology termI ontologyI
    //    let ontologyII = OntologyHandler.addTerm ontologyI termI
    //    //let addOntologyToContext = OntologyHandler.addToContext context (OntologyHandler.addTerm ontologyI termI)
    //    let addTermToContext = TermHandler.addToContext context termI

    //    let cvParam = CVParamHandler.init("Test", termI)
    //    let addCVtoContext = CVParamHandler.addToContext context cvParam

    //    let analysisSoftware = AnalysisSoftwareHandler.init(cvParam, 0)
    //    let analysisSoftwareName = AnalysisSoftwareHandler.addName analysisSoftware "BioFsharp.MZ"
    //    let analysisSoftwareURI = AnalysisSoftwareHandler.addURI analysisSoftwareName "www.TEST.de"
    //    let analysisSoftwareVersion = AnalysisSoftwareHandler.addVersion analysisSoftwareURI "V 1.00"
    //    let analyisisSofwareDeveloper = AnalysisSoftwareHandler.addAnalysisSoftwareDeveloper analysisSoftwareVersion (ContactRoleHandler.init(PersonHandler.init(0, "David"),(CVParamHandler.init("Testi",termI))))
    //    let addAnalysisSoftwareToContext = AnalysisSoftwareHandler.addToContext context analyisisSofwareDeveloper

    //    let person = PersonHandler.init(0)
    //    let addpersonToContext = PersonHandler.addToContext context person

    //    let organization = OrganizationHandler.init(0)
    //    let addOrganizationToContext = OrganizationHandler.addToContext context organization

    //    context.Database.EnsureCreated()

    //    insertWithExceptionCheck context

    //    initStandardDB context

    //    type Test =
    //        {
    //         ID : int
    //         Name : string
    //         Detail : string
    //        }

    //    type TestHandler =
    //            static member init
    //                (
    //                    id        : int,
    //                    ?name     : string,
    //                    ?detail   : string
    //                ) =
    //                let name' :string= defaultArg name null
    //                let detail'    = defaultArg detail null
    //                {
    //                    ID         = id;
    //                    Name       = name';
    //                    Detail     = detail'
    //                }
    

    //    let transformOption (item : 'a option) =
    //        match item with
    //        |Some x -> x
    //        |None -> Unchecked.defaultof<'a>

    //    TestHandler.init(0,transformOption(Some("test")) ,"string")
    //    TestHandler.init(0)
    //    TestHandler.init(0,transformOption(None) ,"string")

    //    let xmlCVParams = SchemePeptideShaker.Load("..\ExampleFile\PeptideShaker_mzid_1_2_example.mzid")
