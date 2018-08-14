namespace TSVMzTabDataBase

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
            
            let convertOptionToArray (optionOfType : seq<'a> option) =
                match optionOfType with
                | Some x -> x.ToArray()
                | None -> Unchecked.defaultof<array<'a>>

            let addToList (typeCollection : array<'a>) (optionOfType : 'a) =
                match typeCollection with
                | null -> 
                    Array.create 1 optionOfType
                | _ -> 
                    Array.append typeCollection [|optionOfType|]

            let addCollectionToList (typeCollection : array<'a>) (optionOfType : seq<'a>) =
                match typeCollection with
                | null -> 
                    Array.append Array.empty<'a> (optionOfType.ToArray())
                | _ -> 
                    Array.append typeCollection (optionOfType.ToArray())

        // Defining the objectHandlers of the entities of the dbContext.

        //open HelperFunctions

        //type ContextHandler =

        //    ///Creates connection for SQLite-context and database.
        //    static member sqliteConnection (path:string) =
        //        let optionsBuilder = 
        //            new DbContextOptionsBuilder<MzTab>()
        //        optionsBuilder.UseSqlite(@"Data Source=" + path) |> ignore
        //        new MzTab(optionsBuilder.Options)       

        //    ///Creats connection for SQL-context and database.
        //    static member sqlConnection() =
        //        let optionsBuilder = 
        //            new DbContextOptionsBuilder<MzTab>()
        //        optionsBuilder.UseSqlServer("Server=(localdb)\mssqllocaldb;Database=MyDatabase;Trusted_Connection=True;") |> ignore
        //        new MzTab(optionsBuilder.Options) 

        //    ///Reads obo-file and creates sequence of Obo.Terms.
        //    static member fromFileObo (filePath:string) =
        //        FileIO.readFile filePath
        //        |> Obo.parseOboTerms

        //    ///Tries to add the object to the database-context.
        //    static member tryAddToContext (context:MzTab) (item:'b) =
        //        context.Add(item)

        //    ///Tries to add the object to the database-context and insert it in the database.
        //    static member tryAddToContextAndInsert (context:MzTab) (item:'b) =
        //        context.Add(item) |> ignore
        //        context.SaveChanges()

        open HelperFunctions

        

        type MetaDataSectionHandler =
            ///Initializes a sample-object with at least all necessary parameters.
            static member init
                (             
                    mzTabVersion                      : string,
                    mzTabMode                         : int,
                    mzType                            : int,     
                    description                       : string,
                    proteinSearchEngineScores         : seq<string*string*string*string>,
                    peptideSearchEngineScores         : seq<string*string*string*string>,
                    psmSearchEngineScores             : seq<string*string*string*string>,
                    smallMoleculeSearchEngineScores   : seq<string*string*string*string>,
                    fixedMods                         : seq<string*string*string*string>,
                    variableMods                      : seq<string*string*string*string>,
                    msRunLocation                     : string,
                    ?mzID                             : string,
                    ?title                            : string,
                    ?sampleProcessings                : seq<string*string*string*string>,
                    ?instrumentNames                  : seq<string*string*string*string>,
                    ?instrumentSources                : seq<string*string*string*string>,
                    ?instrumentAnalyzers              : seq<string*string*string*string>,
                    ?instrumentDetectors              : seq<string*string*string*string>,
                    ?softwares                        : seq<string*string*string*string>,
                    ?softwaresSettings                : seq<string*string*string*string>,
                    ?falseDiscoveryRates              : seq<string*string*string*string>,
                    ?publications                     : seq<string>,
                    ?contactNames                     : seq<string>,
                    ?contactAffiliations              : seq<string>,
                    ?contactEMails                    : seq<string>,
                    ?uris                             : seq<string>,
                    ?fixedModSites                    : seq<string>,
                    ?fixedModPositions                : seq<string>,
                    ?variableModSites                 : seq<string>,
                    ?variableModPositions             : seq<string>,
                    ?quantificationMethod             : string*string*string*string,
                    ?proteinQuantificationUnit        : string*string*string*string,
                    ?peptideQuantificationUnit        : string*string*string*string,
                    ?smallMoleculeQuantificationUnit  : string*string*string*string,
                    ?msRunsFormat                     : string*string*string*string,
                    ?msRunsIDFormats                  : string*string*string*string,
                    ?msRunsFragmentationMethods       : seq<string*string*string*string>,
                    ?msRunsHash                       : string,
                    ?msRunsHashMethod                 : string*string*string*string,
                    ?customs                          : seq<string*string*string*string>,
                    ?samplesSpecies                   : seq<string*string*string*string>,
                    ?sampleTissues                    : seq<string*string*string*string>,
                    ?samplesCellTypes                 : seq<string*string*string*string>,
                    ?samplesDiseases                  : seq<string*string*string*string>,
                    ?sampleDescriptions               : seq<string>,
                    ?samplesCustoms                   : seq<string>,
                    ?assaysQuantificationReagent      : string*string*string*string,
                    ?assaysQuantificationMods         : seq<string*string*string*string>,
                    ?assaysQuantificationModsSite     : string,
                    ?assaysQuantificationModsPosition : string,
                    ?assaysSampleRef                  : string,
                    ?assaysMSRunRef                   : string,
                    ?studyVariablesAssayRefs          : seq<string>,
                    ?studyVariablesSampleRefs         : seq<string>,
                    ?studyVariablesDescription        : seq<string>,
                    ?cvsLabel                         : string,
                    ?cvsFullName                      : string,
                    ?cvsVersion                       : string,
                    ?cvsURI                           : string,
                    ?colUnitProtein                   : string*string*string*string,
                    ?colUnitPeptide                   : string*string*string*string,
                    ?colUnitPSM                       : string*string*string*string,
                    ?colUnitSmallMolecule             : string*string*string*string
                    
                ) =

                let mzID'                             = defaultArg mzID Unchecked.defaultof<string>
                let title'                            = defaultArg title Unchecked.defaultof<string>
                let sampleProcessings'                = convertOptionToArray sampleProcessings 
                let instrumentNames'                  = convertOptionToArray instrumentNames 
                let instrumentSources'                = convertOptionToArray instrumentSources
                let instrumentAnalyzers'              = convertOptionToArray instrumentAnalyzers 
                let instrumentDetectors'              = convertOptionToArray instrumentDetectors
                let softwares'                        = convertOptionToArray softwares
                let softwaresSettings'                = convertOptionToArray softwaresSettings
                let falseDiscoveryRates'              = convertOptionToArray falseDiscoveryRates
                let publications'                     = convertOptionToArray publications 
                let contactNames'                     = convertOptionToArray contactNames
                let contactAffiliations'              = convertOptionToArray contactAffiliations
                let contactEMails'                    = convertOptionToArray contactEMails
                let uris'                             = convertOptionToArray uris
                let fixedModSites'                    = convertOptionToArray fixedModSites
                let fixedModPositions'                = convertOptionToArray fixedModPositions
                let variableModSites'                 = convertOptionToArray variableModSites
                let variableModPositions'             = convertOptionToArray variableModPositions
                let quantificationMethod'             = defaultArg quantificationMethod Unchecked.defaultof<string*string*string*string>
                let proteinQuantificationUnit'        = defaultArg proteinQuantificationUnit Unchecked.defaultof<string*string*string*string>
                let peptideQuantificationUnit'        = defaultArg peptideQuantificationUnit Unchecked.defaultof<string*string*string*string>
                let smallMoleculeQuantificationUnit'  = defaultArg smallMoleculeQuantificationUnit Unchecked.defaultof<string*string*string*string>
                let msRunsFormat'                     = defaultArg msRunsFormat Unchecked.defaultof<string*string*string*string>
                let msRunsIDFormats'                  = defaultArg msRunsIDFormats Unchecked.defaultof<string*string*string*string>
                let msRunsFragmentationMethods'       = convertOptionToArray msRunsFragmentationMethods
                let msRunsHash'                       = defaultArg msRunsHash Unchecked.defaultof<string>
                let msRunsHashMethod'                 = defaultArg msRunsHashMethod Unchecked.defaultof<string*string*string*string>
                let customs'                          = convertOptionToArray customs
                let samplesSpecies'                   = convertOptionToArray samplesSpecies
                let sampleTissues'                    = convertOptionToArray sampleTissues
                let samplesCellTypes'                 = convertOptionToArray samplesCellTypes
                let samplesDiseases'                  = convertOptionToArray samplesDiseases
                let sampleDescriptions'               = convertOptionToArray sampleDescriptions
                let samplesCustoms'                   = convertOptionToArray samplesCustoms
                let assaysQuantificationReagent'      = defaultArg assaysQuantificationReagent Unchecked.defaultof<string*string*string*string> 
                let assaysQuantificationMods'         = convertOptionToArray assaysQuantificationMods
                let assaysQuantificationModsSite'     = defaultArg assaysQuantificationModsSite Unchecked.defaultof<string>
                let assaysQuantificationModsPosition' = defaultArg assaysQuantificationModsPosition Unchecked.defaultof<string>
                let assaysSampleRef'                  = defaultArg assaysSampleRef Unchecked.defaultof<string>
                let assaysMSRunRef'                   = defaultArg assaysMSRunRef Unchecked.defaultof<string>
                let studyVariablesAssayRefs'          = convertOptionToArray studyVariablesAssayRefs
                let studyVariablesSampleRefs'         = convertOptionToArray studyVariablesSampleRefs
                let studyVariablesDescription'        = convertOptionToArray studyVariablesDescription
                let cvsLabel'                         = defaultArg cvsLabel Unchecked.defaultof<string>
                let cvsFullName'                      = defaultArg cvsFullName Unchecked.defaultof<string>
                let cvsVersion'                       = defaultArg cvsVersion Unchecked.defaultof<string>
                let cvsURI'                           = defaultArg cvsURI Unchecked.defaultof<string>
                let colUnitProtein'                   = defaultArg colUnitProtein Unchecked.defaultof<string*string*string*string>
                let colUnitPeptide'                   = defaultArg colUnitPeptide Unchecked.defaultof<string*string*string*string>
                let colUnitPSM'                       = defaultArg colUnitPSM Unchecked.defaultof<string*string*string*string>
                let colUnitSmallMolecule'             = defaultArg colUnitSmallMolecule Unchecked.defaultof<string*string*string*string>

                let createMetadatasection 
                        mzTabVersion mzTabMode mzType mzID title description sampleProcessings instrumentNames instrumentSources instrumentAnalyzers
                        instrumentDetectors softwares softwaresSettings proteinSearchEngineScores peptideSearchEngineScores psmSearchEngineScores smallMoleculeSearchEngineScores
                        falseDiscoveryRates publications contactNames contactAffiliations contactEMails uris fixedMods fixedModSites fixedModPositions variableMods
                        variableModSites variableModPositions quantificationMethod proteinQuantificationUnit peptideQuantificationUnit smallMoleculeQuantificationUnit
                        msRunsFormat msRunLocation msRunsIDFormats msRunsFragmentationMethods msRunsHash msRunsHashMethod customs samplesSpecies sampleTissues samplesCellTypes
                        samplesDiseases sampleDescriptions samplesCustoms assaysQuantificationReagent assaysQuantificationMods assaysQuantificationModsSite 
                        assaysQuantificationModsPosition assaysSampleRef assaysMSRunRef studyVariablesAssayRefs studyVariablesSampleRefs studyVariablesDescription cvsLabel 
                        cvsFullName cvsVersion cvsURI colUnitProtein colUnitPeptide colUnitPSM colUnitSmallMolecule
                        =
                    {
                     MetaDataSection.MzTabVersion                     = mzTabVersion
                     MetaDataSection.MzTabMode                        = enum<MzTabMode>(mzTabMode)
                     MetaDataSection.MzType                           = enum<MzTabType>(mzType)
                     MetaDataSection.MzID                             = mzID
                     MetaDataSection.Title                            = title
                     MetaDataSection.Description                      = description
                     MetaDataSection.SampleProcessings                = sampleProcessings
                     MetaDataSection.InstrumentNames                  = instrumentNames
                     MetaDataSection.InstrumentSources                = instrumentSources
                     MetaDataSection.InstrumentAnalyzers              = instrumentAnalyzers
                     MetaDataSection.InstrumentDetectors              = instrumentDetectors
                     MetaDataSection.Softwares                        = softwares
                     MetaDataSection.SoftwaresSettings                = softwaresSettings
                     MetaDataSection.ProteinSearchEngineScores        = proteinSearchEngineScores
                     MetaDataSection.PeptideSearchEngineScores        = peptideSearchEngineScores
                     MetaDataSection.PSMSearchEngineScores            = psmSearchEngineScores
                     MetaDataSection.SmallMoleculeSearchEngineScores  = smallMoleculeSearchEngineScores
                     MetaDataSection.FalseDiscoveryRates              = falseDiscoveryRates
                     MetaDataSection.Publications                     = publications
                     MetaDataSection.ContactNames                     = contactNames
                     MetaDataSection.ContactAffiliations              = contactAffiliations
                     MetaDataSection.ContactEMails                    = contactEMails
                     MetaDataSection.URIs                             = uris
                     MetaDataSection.FixedMods                        = fixedMods
                     MetaDataSection.FixedModSites                    = fixedModSites
                     MetaDataSection.FixedModPositions                = fixedModPositions
                     MetaDataSection.VariableMods                     = variableMods
                     MetaDataSection.VariableModSites                 = variableModSites
                     MetaDataSection.VariableModPositions             = variableModPositions
                     MetaDataSection.QuantificationMethod             = quantificationMethod
                     MetaDataSection.ProteinQuantificationUnit        = proteinQuantificationUnit
                     MetaDataSection.PeptideQuantificationUnit        = peptideQuantificationUnit
                     MetaDataSection.SmallMoleculeQuantificationUnit  = smallMoleculeQuantificationUnit
                     MetaDataSection.MSRunsFormat                     = msRunsFormat
                     MetaDataSection.MSRunLocation                    = msRunLocation
                     MetaDataSection.MSRunsIDFormat                   = msRunsIDFormats
                     MetaDataSection.MSRunsFragmentationMethods       = msRunsFragmentationMethods
                     MetaDataSection.MSRunsHash                       = msRunsHash
                     MetaDataSection.MSRunsHashMethod                 = msRunsHashMethod
                     MetaDataSection.Customs                          = customs
                     MetaDataSection.SamplesSpecies                   = samplesSpecies
                     MetaDataSection.SampleTissues                    = sampleTissues
                     MetaDataSection.SamplesCellTypes                 = samplesCellTypes
                     MetaDataSection.SamplesDiseases                  = samplesDiseases
                     MetaDataSection.SampleDescriptions               = sampleDescriptions
                     MetaDataSection.SamplesCustoms                   = samplesCustoms
                     MetaDataSection.AssaysQuantificationReagent      = assaysQuantificationReagent
                     MetaDataSection.AssaysQuantificationMods         = assaysQuantificationMods
                     MetaDataSection.AssaysQuantificationModsSite     = assaysQuantificationModsSite
                     MetaDataSection.AssaysQuantificationModsPosition = assaysQuantificationModsPosition
                     MetaDataSection.AssaysSampleRef                  = assaysSampleRef
                     MetaDataSection.AssaysMSRunRef                   = assaysMSRunRef
                     MetaDataSection.StudyVariablesAssayRefs          = studyVariablesAssayRefs
                     MetaDataSection.StudyVariablesSampleRefs         = studyVariablesSampleRefs
                     MetaDataSection.StudyVariablesDescription        = studyVariablesDescription
                     MetaDataSection.CvsLabel                         = cvsLabel
                     MetaDataSection.CvsFullName                      = cvsFullName
                     MetaDataSection.CvsVersion                       = cvsVersion
                     MetaDataSection.CvsURI                           = cvsURI
                     MetaDataSection.ColUnitProtein                   = colUnitProtein
                     MetaDataSection.ColUnitPeptide                   = colUnitPeptide
                     MetaDataSection.ColUnitPSM                       = colUnitPSM
                     MetaDataSection.ColUnitSmallMolecule             = colUnitSmallMolecule
                    }
                createMetadatasection
                        mzTabVersion mzTabMode mzType mzID' title' description sampleProcessings' instrumentNames' instrumentSources' instrumentAnalyzers' instrumentDetectors' 
                        softwares' softwaresSettings' (proteinSearchEngineScores.ToArray()) (peptideSearchEngineScores.ToArray()) (psmSearchEngineScores.ToArray()) 
                        (smallMoleculeSearchEngineScores.ToArray()) falseDiscoveryRates' publications' contactNames' contactAffiliations' contactEMails' uris' (fixedMods.ToArray()) 
                        fixedModSites' fixedModPositions' (variableMods.ToArray()) variableModSites' variableModPositions' quantificationMethod' proteinQuantificationUnit' 
                        peptideQuantificationUnit' smallMoleculeQuantificationUnit' msRunsFormat' msRunLocation msRunsIDFormats' msRunsFragmentationMethods' msRunsHash' 
                        msRunsHashMethod' customs' samplesSpecies' sampleTissues' samplesCellTypes' samplesDiseases' sampleDescriptions' samplesCustoms' assaysQuantificationReagent' 
                        assaysQuantificationMods' assaysQuantificationModsSite' assaysQuantificationModsPosition' assaysSampleRef' assaysMSRunRef' studyVariablesAssayRefs' 
                        studyVariablesSampleRefs' studyVariablesDescription' cvsLabel' cvsFullName' cvsVersion' cvsURI' colUnitProtein' colUnitPeptide'  colUnitPSM' 
                        colUnitSmallMolecule'
                                   
            ///Replaces mzID of existing object with new one.
            static member addReliability
                (mzID:string) (section:MetaDataSection) =
                section.MzID <- mzID
                section

            ///Replaces title of existing object with new one.
            static member addTitle
                (title:string) (section:MetaDataSection) =
                section.Title <- title
                section
            
            ///Adds a sampleProcessing to an existing object.
            static member addSampleProcessing (sampleProcessing:string*string*string*string) (section:MetaDataSection) =
                section.SampleProcessings <- addToList section.SampleProcessings sampleProcessing
                section

            ///Adds a collection of sampleProcessings to an existing object.
            static member addSampleProcessings (sampleProcessings:seq<string*string*string*string>) (section:MetaDataSection) =
                section.SampleProcessings <- addCollectionToList section.SampleProcessings sampleProcessings
                section

            ///Adds a instrumentName to an existing object.
            static member addInstrumentName (instrumentName:string*string*string*string) (section:MetaDataSection) =
                section.InstrumentNames <- addToList section.InstrumentNames instrumentName
                section

            ///Adds a collection of instrumentNames to an existing object.
            static member addInstrumentNames (instrumentNames:seq<string*string*string*string>) (section:MetaDataSection) =
                section.InstrumentNames <- addCollectionToList section.InstrumentNames instrumentNames
                section

            ///Adds a instrumentSource to an existing object.
            static member addInstrumentSource (instrumentSource:string*string*string*string) (section:MetaDataSection) =
                section.InstrumentSources <- addToList section.InstrumentSources instrumentSource
                section

            ///Adds a collection of instrumentSources to an existing object.
            static member addInstrumentSources (instrumentAnalyzer:seq<string*string*string*string>) (section:MetaDataSection) =
                section.InstrumentSources <- addCollectionToList section.InstrumentSources instrumentAnalyzer
                section

            ///Adds a instrumentAnalyzer to an existing object.
            static member addInstrumentAnalyzer (instrumentAnalyzer:string*string*string*string) (section:MetaDataSection) =
                section.InstrumentAnalyzers <- addToList section.InstrumentAnalyzers instrumentAnalyzer
                section

            ///Adds a collection of instrumentAnalyzers to an existing object.
            static member addInstrumentAnalyzers (instrumentAnalyzers:seq<string*string*string*string>) (section:MetaDataSection) =
                section.InstrumentAnalyzers <- addCollectionToList section.InstrumentAnalyzers instrumentAnalyzers
                section

            ///Adds a instrumentDetector to an existing object.
            static member addInstrumentDetector (instrumentDetector:string*string*string*string) (section:MetaDataSection) =
                section.InstrumentDetectors <- addToList section.InstrumentDetectors instrumentDetector
                section

            ///Adds a collection of instrumentDetectors to an existing object.
            static member addInstrumentDetectors (instrumentDetectors:seq<string*string*string*string>) (section:MetaDataSection) =
                section.InstrumentDetectors <- addCollectionToList section.InstrumentDetectors instrumentDetectors
                section

            ///Adds a software to an existing object.
            static member addSoftware (software:string*string*string*string) (section:MetaDataSection) =
                section.Softwares <- addToList section.Softwares software
                section

            ///Adds a collection of softwares to an existing object.
            static member addSoftwares (softwares:seq<string*string*string*string>) (section:MetaDataSection) =
                section.Softwares <- addCollectionToList section.Softwares softwares
                section

            ///Adds a softwaresSetting to an existing object.
            static member addSoftwaresSetting (softwaresSetting:string*string*string*string) (section:MetaDataSection) =
                section.SoftwaresSettings <- addToList section.SoftwaresSettings softwaresSetting
                section

            ///Adds a collection of softwaresSettings to an existing object.
            static member addSoftwaresSettings (softwaresSettings:seq<string*string*string*string>) (section:MetaDataSection) =
                section.SoftwaresSettings <- addCollectionToList section.SoftwaresSettings softwaresSettings
                section

            ///Adds a falseDiscoveryRate to an existing object.
            static member addFalseDiscoveryRate (falseDiscoveryRate:string*string*string*string) (section:MetaDataSection) =
                section.FalseDiscoveryRates <- addToList section.FalseDiscoveryRates falseDiscoveryRate
                section

            ///Adds a collection of falseDiscoveryRates to an existing object.
            static member addFalseDiscoveryRates (falseDiscoveryRates:seq<string*string*string*string>) (section:MetaDataSection) =
                section.FalseDiscoveryRates <- addCollectionToList section.FalseDiscoveryRates falseDiscoveryRates
                section

            ///Adds a publication to an existing object.
            static member addPublication (publication:string) (section:MetaDataSection) =
                section.Publications <- addToList section.Publications publication
                section

            ///Adds a collection of publications to an existing object.
            static member addPublications (publications:seq<string>) (section:MetaDataSection) =
                section.Publications <- addCollectionToList section.Publications publications
                section

            ///Adds a contactName to an existing object.
            static member addContactName (contactName:string) (section:MetaDataSection) =
                section.ContactNames <- addToList section.ContactNames contactName
                section

            ///Adds a collection of contactNames to an existing object.
            static member addContactNames (contactNames:seq<string>) (section:MetaDataSection) =
                section.ContactNames <- addCollectionToList section.ContactNames contactNames
                section

            ///Adds a contactAffiliation to an existing object.
            static member addContactAffiliation (contactAffiliation:string) (section:MetaDataSection) =
                section.ContactAffiliations <- addToList section.ContactAffiliations contactAffiliation
                section

            ///Adds a collection of contactAffiliations to an existing object.
            static member addContactAffiliations (contactAffiliations:seq<string>) (section:MetaDataSection) =
                section.ContactAffiliations <- addCollectionToList section.ContactAffiliations contactAffiliations
                section

            ///Adds a contactEMail to an existing object.
            static member addContactEMail (contactEMail:string) (section:MetaDataSection) =
                section.ContactEMails <- addToList section.ContactEMails contactEMail
                section

            ///Adds a collection of contactEMails to an existing object.
            static member addContactEMails (contactEMails:seq<string>) (section:MetaDataSection) =
                section.ContactEMails <- addCollectionToList section.ContactEMails contactEMails
                section

            ///Adds a uri to an existing object.
            static member addURI (uri:string) (section:MetaDataSection) =
                section.URIs <- addToList section.URIs uri
                section

            ///Adds a collection of uris to an existing object.
            static member addURIs (uris:seq<string>) (section:MetaDataSection) =
                section.URIs <- addCollectionToList section.URIs uris
                section

            ///Adds a fixedModSite to an existing object.
            static member addFixedModSite (fixedModSite:string) (section:MetaDataSection) =
                section.FixedModSites <- addToList section.FixedModSites fixedModSite
                section

            ///Adds a collection of fixedModSites to an existing object.
            static member addFixedModSites (fixedModSites:seq<string>) (section:MetaDataSection) =
                section.FixedModSites <- addCollectionToList section.FixedModSites fixedModSites
                section

            ///Adds a fixedModSite to an existing object.
            static member addFixedModPosition (fixedModPosition:string) (section:MetaDataSection) =
                section.FixedModPositions <- addToList section.FixedModPositions fixedModPosition
                section

            ///Adds a collection of fixedModSites to an existing object.
            static member addFixedModPositions (fixedModPositions:seq<string>) (section:MetaDataSection) =
                section.FixedModPositions <- addCollectionToList section.FixedModPositions fixedModPositions
                section

            ///Adds a variableModSite to an existing object.
            static member addVariableModSite (variableModSite:string) (section:MetaDataSection) =
                section.VariableModSites <- addToList section.VariableModSites variableModSite
                section

            ///Adds a collection of variableModSites to an existing object.
            static member addVariableModSites (variableModSites:seq<string>) (section:MetaDataSection) =
                section.VariableModSites <- addCollectionToList section.VariableModSites variableModSites
                section

            ///Adds a variableModSite to an existing object.
            static member addVariableModPosition (variableModPosition:string) (section:MetaDataSection) =
                section.VariableModPositions <- addToList section.VariableModPositions variableModPosition
                section

            ///Adds a collection of variableModSites to an existing object.
            static member addVariableModPositions (variableModPositions:seq<string>) (section:MetaDataSection) =
                section.VariableModPositions <- addCollectionToList section.VariableModPositions variableModPositions
                section

            ///Replaces quantificationMethod of existing object with new one.
            static member addQuantificationMethod
                (quantificationMethod:string*string*string*string) (section:MetaDataSection) =
                section.QuantificationMethod <- quantificationMethod
                section

            ///Replaces proteinQuantificationUnit of existing object with new one.
            static member addProteinQuantificationUnit
                (proteinQuantificationUnit:string*string*string*string) (section:MetaDataSection) =
                section.ProteinQuantificationUnit <- proteinQuantificationUnit
                section

            ///Replaces peptideQuantificationUnit of existing object with new one.
            static member addPeptideQuantificationUnit
                (peptideQuantificationUnit:string*string*string*string) (section:MetaDataSection) =
                section.PeptideQuantificationUnit <- peptideQuantificationUnit
                section

            ///Replaces smallMoleculeQuantificationUnit of existing object with new one.
            static member addSmallMoleculeQuantificationUnit
                (smallMoleculeQuantificationUnit:string*string*string*string) (section:MetaDataSection) =
                section.SmallMoleculeQuantificationUnit <- smallMoleculeQuantificationUnit
                section

            ///Replaces msRunsFormat of existing object with new one.
            static member addMSRunsFormat
                (msRunsFormat:string*string*string*string) (section:MetaDataSection) =
                section.MSRunsFormat <- msRunsFormat
                section

            ///Replaces msRunsIDFormats of existing object with new one.
            static member addMSRunsIDFormats
                (msRunsIDFormats:string*string*string*string) (section:MetaDataSection) =
                section.MSRunsIDFormat <- msRunsIDFormats
                section

            ///Adds a msRunsFragmentationMethod to an existing object.
            static member addMSRunsFragmentationMethod (msRunsFragmentationMethod:string*string*string*string) (section:MetaDataSection) =
                section.MSRunsFragmentationMethods <- addToList section.MSRunsFragmentationMethods msRunsFragmentationMethod
                section

            ///Adds a collection of msRunsFragmentationMethods to an existing object.
            static member addMSRunsFragmentationMethods (msRunsFragmentationMethods:seq<string*string*string*string>) (section:MetaDataSection) =
                section.MSRunsFragmentationMethods <- addCollectionToList section.MSRunsFragmentationMethods msRunsFragmentationMethods
                section

            ///Replaces msRunsHash of existing object with new one.
            static member addMSRunsHash
                (msRunsHash:string) (section:MetaDataSection) =
                section.MSRunsHash <- msRunsHash
                section

            ///Replaces msRunsHashMethod of existing object with new one.
            static member addMSRunsHashMethod
                (msRunsHashMethod:string*string*string*string) (section:MetaDataSection) =
                section.MSRunsHashMethod <- msRunsHashMethod
                section

            ///Adds a custom to an existing object.
            static member addCustom (custom:string*string*string*string) (section:MetaDataSection) =
                section.Customs <- addToList section.MSRunsFragmentationMethods custom
                section

            ///Adds a collection of customs to an existing object.
            static member addCustoms (customs:seq<string*string*string*string>) (section:MetaDataSection) =
                section.Customs <- addCollectionToList section.MSRunsFragmentationMethods customs
                section

            ///Adds a sampleSpecies to an existing object.
            static member addSampleSpecies (sampleSpecies:string*string*string*string) (section:MetaDataSection) =
                section.SamplesSpecies <- addToList section.SamplesSpecies sampleSpecies
                section

            ///Adds a collection of sampleSpecies to an existing object.
            static member addMoreSampleSpecies (sampleSpecies:seq<string*string*string*string>) (section:MetaDataSection) =
                section.SamplesSpecies <- addCollectionToList section.SamplesSpecies sampleSpecies
                section

            ///Adds a sampleTissue to an existing object.
            static member addSampleTissue (sampleTissue:string*string*string*string) (section:MetaDataSection) =
                section.SampleTissues <- addToList section.SampleTissues sampleTissue
                section

            ///Adds a collection of sampleTissues to an existing object.
            static member addSampleTissues (sampleTissues:seq<string*string*string*string>) (section:MetaDataSection) =
                section.SampleTissues <- addCollectionToList section.SampleTissues sampleTissues
                section

            ///Adds a samplesCellType to an existing object.
            static member addSamplesCellType (samplesCellType:string*string*string*string) (section:MetaDataSection) =
                section.SamplesCellTypes <- addToList section.SamplesCellTypes samplesCellType
                section

            ///Adds a collection of samplesCellTypes to an existing object.
            static member addSamplesCellTypes (samplesCellTypes:seq<string*string*string*string>) (section:MetaDataSection) =
                section.SamplesCellTypes <- addCollectionToList section.SamplesCellTypes samplesCellTypes
                section

            ///Adds a samplesDisease to an existing object.
            static member addSamplesDisease (samplesDisease:string*string*string*string) (section:MetaDataSection) =
                section.SamplesDiseases <- addToList section.SamplesDiseases samplesDisease
                section

            ///Adds a collection of samplesDiseases to an existing object.
            static member addSamplesDiseases (samplesDiseases:seq<string*string*string*string>) (section:MetaDataSection) =
                section.SamplesDiseases <- addCollectionToList section.SamplesDiseases samplesDiseases
                section

            ///Adds a samplesDisease to an existing object.
            static member addSampleDescription (sampleDescription:string) (section:MetaDataSection) =
                section.SampleDescriptions <- addToList section.SampleDescriptions sampleDescription
                section

            ///Adds a collection of samplesDiseases to an existing object.
            static member addSampleDescriptions (sampleDescriptions:seq<string>) (section:MetaDataSection) =
                section.SampleDescriptions <- addCollectionToList section.SampleDescriptions sampleDescriptions
                section

            ///Adds a samplesCustom to an existing object.
            static member addSamplesCustom (samplesCustom:string) (section:MetaDataSection) =
                section.SamplesCustoms <- addToList section.SampleDescriptions samplesCustom
                section

            ///Adds a collection of samplesCustoms to an existing object.
            static member addSamplesCustoms (samplesCustoms:seq<string>) (section:MetaDataSection) =
                section.SamplesCustoms <- addCollectionToList section.SampleDescriptions samplesCustoms
                section

            ///Replaces assaysQuantificationReagent of existing object with new one.
            static member addAssaysQuantificationReagent
                (assaysQuantificationReagent:string*string*string*string) (section:MetaDataSection) =
                section.AssaysQuantificationReagent <- assaysQuantificationReagent
                section

            ///Adds a assaysQuantificationMod to an existing object.
            static member addAssaysQuantificationMod (assaysQuantificationMod:string*string*string*string) (section:MetaDataSection) =
                section.AssaysQuantificationMods <- addToList section.AssaysQuantificationMods assaysQuantificationMod
                section

            ///Adds a collection of assaysQuantificationMods to an existing object.
            static member addAssaysQuantificationMods (assaysQuantificationMods:seq<string*string*string*string>) (section:MetaDataSection) =
                section.AssaysQuantificationMods <- addCollectionToList section.AssaysQuantificationMods assaysQuantificationMods
                section

            ///Replaces assaysQuantificationModsSite of existing object with new one.
            static member addAssaysQuantificationModsSite
                (assaysQuantificationModsSite:string) (section:MetaDataSection) =
                section.AssaysQuantificationModsSite <- assaysQuantificationModsSite
                section

            ///Replaces assaysQuantificationModsPosition of existing object with new one.
            static member addAssaysQuantificationModsPosition
                (assaysQuantificationModsPosition:string) (section:MetaDataSection) =
                section.AssaysQuantificationModsPosition <- assaysQuantificationModsPosition
                section

            ///Replaces assaysSampleRef of existing object with new one.
            static member addAssaysSampleRef
                (assaysSampleRef:string) (section:MetaDataSection) =
                section.AssaysSampleRef <- assaysSampleRef
                section

            ///Replaces assaysMSRunRef of existing object with new one.
            static member addAssaysMSRunRef
                (assaysMSRunRef:string) (section:MetaDataSection) =
                section.AssaysMSRunRef <- assaysMSRunRef
                section

            ///Adds a studyVariablesAssayRef to an existing object.
            static member addStudyVariablesAssayRef (studyVariablesAssayRef:string) (section:MetaDataSection) =
                section.StudyVariablesAssayRefs <- addToList section.StudyVariablesAssayRefs studyVariablesAssayRef
                section

            ///Adds a collection of studyVariablesAssayRefs to an existing object.
            static member addStudyVariablesAssayRefs (studyVariablesAssayRefs:seq<string>) (section:MetaDataSection) =
                section.StudyVariablesAssayRefs <- addCollectionToList section.StudyVariablesAssayRefs studyVariablesAssayRefs
                section

            ///Adds a studyVariablesSampleRef to an existing object.
            static member addStudyVariablesSampleRef (studyVariablesSampleRef:string) (section:MetaDataSection) =
                section.StudyVariablesSampleRefs <- addToList section.StudyVariablesSampleRefs studyVariablesSampleRef
                section

            ///Adds a collection of studyVariablesSampleRefs to an existing object.
            static member addStudyVariablesSampleRefs (studyVariablesSampleRefs:seq<string>) (section:MetaDataSection) =
                section.StudyVariablesSampleRefs <- addCollectionToList section.StudyVariablesSampleRefs studyVariablesSampleRefs
                section

            ///Adds a studyVariablesDescription to an existing object.
            static member addStudyVariablesDescription (studyVariablesDescription:string) (section:MetaDataSection) =
                section.StudyVariablesDescription <- addToList section.StudyVariablesDescription studyVariablesDescription
                section

            ///Adds a collection of studyVariablesDescriptions to an existing object.
            static member addStudyVariablesDescriptions (studyVariablesDescriptions:seq<string>) (section:MetaDataSection) =
                section.StudyVariablesDescription <- addCollectionToList section.StudyVariablesDescription studyVariablesDescriptions
                section

            ///Replaces cvsLabel of existing object with new one.
            static member addCvsLabel
                (cvsLabel:string) (section:MetaDataSection) =
                section.CvsLabel <- cvsLabel
                section

            ///Replaces cvsFullName of existing object with new one.
            static member addCvsFullName
                (cvsFullName:string) (section:MetaDataSection) =
                section.CvsFullName <- cvsFullName
                section

            ///Replaces cvsVersion of existing object with new one.
            static member addCvsVersion
                (cvsVersion:string) (section:MetaDataSection) =
                section.CvsVersion <- cvsVersion
                section

            ///Replaces cvsURI of existing object with new one.
            static member addCvsURI
                (cvsURI:string) (section:MetaDataSection) =
                section.CvsURI <- cvsURI
                section

            ///Replaces colUnitProtein of existing object with new one.
            static member addColUnitProtein
                (colUnitProtein:string*string*string*string) (section:MetaDataSection) =
                section.ColUnitProtein <- colUnitProtein
                section

            ///Replaces colUnitPeptide of existing object with new one.
            static member addColUnitPeptide
                (colUnitPeptide:string*string*string*string) (section:MetaDataSection) =
                section.ColUnitPeptide <- colUnitPeptide
                section

            ///Replaces colUnitPSM of existing object with new one.
            static member addColUnitPSM
                (colUnitPSM:string*string*string*string) (section:MetaDataSection) =
                section.ColUnitPSM <- colUnitPSM
                section

            ///Replaces colUnitSmallMolecule of existing object with new one.
            static member addColUnitSmallMolecule
                (colUnitSmallMolecule:string*string*string*string) (section:MetaDataSection) =
                section.ColUnitSmallMolecule <- colUnitSmallMolecule
                section

        type ProteinSectionHandler =
            ///Initializes a sample-object with at least all necessary parameters.
            static member init
                (             
                    accession                                        : string,
                    description                                      : string,
                    taxid                                            : int,
                    species                                          : string,
                    database                                         : string,
                    databaseVersion                                  : string,
                    searchEngines                                    : seq<string*string*string*string>,
                    bestSearchEngineScore                            : float,
                    ambiguityMembers                                 : seq<string>,
                    modifications                                    : seq<string>,
                    ?searchEngineScoresMSRuns                        : float,
                    ?reliability                                     : int,
                    ?numPSMsMSRuns                                   : int,
                    ?numPeptidesDistinctMSRuns                       : int,
                    ?numPeptidesUniqueMSRuns                         : int,
                    ?uri                                             : string,
                    ?goTerms                                         : seq<string>, 
                    ?proteinCoverage                                 : float,
                    ?proteinAbundanceAssays                          : seq<float>,
                    ?proteinAbundanceStudyVariables                  : seq<float>,
                    ?proteinAbundanceStandardDeviationStudyVariables : seq<float>,
                    ?proteinAbundanceStandardErrorStudyVariables     : seq<float>,
                    ?optionalInformation                             : seq<string>
                    
                ) =

                let searchEngineScoresMSRuns'                        = defaultArg searchEngineScoresMSRuns Unchecked.defaultof<float>
                let reliability'                                     = defaultArg reliability Unchecked.defaultof<int>
                let numPSMsMSRuns'                                   = defaultArg numPSMsMSRuns Unchecked.defaultof<int>
                let numPeptidesDistinctMSRuns'                       = defaultArg numPeptidesDistinctMSRuns Unchecked.defaultof<int>
                let numPeptidesUniqueMSRuns'                         = defaultArg numPeptidesUniqueMSRuns Unchecked.defaultof<int>
                let uri'                                             = defaultArg uri Unchecked.defaultof<string>
                let goTerms'                                         = convertOptionToArray goTerms
                let proteinCoverage'                                 = defaultArg proteinCoverage Unchecked.defaultof<float>
                let proteinAbundanceAssays'                          = convertOptionToArray proteinAbundanceAssays
                let proteinAbundanceStudyVariables'                  = convertOptionToArray proteinAbundanceStudyVariables
                let proteinAbundanceStandardDeviationStudyVariables' = convertOptionToArray proteinAbundanceStandardDeviationStudyVariables
                let proteinAbundanceStandardErrorStudyVariables'     = convertOptionToArray proteinAbundanceStandardErrorStudyVariables
                let optionalInformation'                             = convertOptionToArray optionalInformation

                new ProteinSection(
                                   accession,
                                   description,
                                   taxid,
                                   species,
                                   database,
                                   databaseVersion,
                                   searchEngines.ToArray(),  
                                   bestSearchEngineScore,
                                   searchEngineScoresMSRuns',
                                   reliability',
                                   numPSMsMSRuns',
                                   numPeptidesDistinctMSRuns',
                                   numPeptidesUniqueMSRuns',
                                   ambiguityMembers.ToArray(), 
                                   modifications.ToArray(),
                                   uri',
                                   goTerms',
                                   proteinCoverage',
                                   proteinAbundanceAssays',
                                   proteinAbundanceStudyVariables',
                                   proteinAbundanceStandardDeviationStudyVariables', 
                                   proteinAbundanceStandardErrorStudyVariables',
                                   optionalInformation'
                                  )

            ///Replaces searchEngineScoresMSRuns of existing object with new one.
            static member addSearchEngineScoresMSRuns
                (searchEngineScoresMSRuns:float) (section:ProteinSection) =
                section.SearchEngineScoresMSRuns <- searchEngineScoresMSRuns
                section

            ///Replaces reliability of existing object with new one.
            static member addReliability
                (reliability:int) (section:ProteinSection) =
                section.Reliability <- reliability
                section

            ///Replaces numPSMsMSRuns of existing object with new one.
            static member addNumPSMsMSRuns
                (numPSMsMSRuns:int) (section:ProteinSection) =
                section.NumPSMsMSRuns <- numPSMsMSRuns
                section

            ///Replaces numPeptidesDistinctMSRuns of existing object with new one.
            static member addNumPeptidesDistinctMSRuns
                (numPeptidesDistinctMSRuns:int) (section:ProteinSection) =
                section.NumPeptidesDistinctMSRuns <- numPeptidesDistinctMSRuns
                section

            ///Replaces numPeptidesUniqueMSRuns of existing object with new one.
            static member addNumPeptidesUniqueMSRuns
                (numPeptidesUniqueMSRuns:int) (section:ProteinSection) =
                section.NumPeptidesUniqueMSRuns <- numPeptidesUniqueMSRuns
                section

            ///Replaces uri of existing object with new one.
            static member addURI
                (uri:string) (section:ProteinSection) =
                section.URI <- uri
                section

            ///Adds a goTerm to an existing object.
            static member addGoTerm (goTerm:string) (section:ProteinSection) =
                section.GoTerms <- addToList section.GoTerms goTerm
                section

            ///Adds a collection of goTerms to an existing object.
            static member addGoTerms (goTerms:seq<string>) (section:ProteinSection) =
                section.GoTerms <- addCollectionToList section.GoTerms goTerms
                section

            ///Replaces proteinCoverage of existing object with new one.
            static member addProteinCoverage
                (proteinCoverage:float) (section:ProteinSection) =
                section.ProteinCoverage <- proteinCoverage
                section
   
            ///Adds a proteinAbundanceAssay to an existing object.
            static member addProteinAbundanceAssay (proteinAbundanceAssay:float) (section:ProteinSection) =
                section.ProteinAbundanceAssays <- addToList section.ProteinAbundanceAssays proteinAbundanceAssay
                section

            ///Adds a collection of proteinAbundanceAssays to an existing object.
            static member addProteinAbundanceAssays (proteinAbundanceAssays:seq<float>) (section:ProteinSection) =
                section.ProteinAbundanceAssays <- addCollectionToList section.ProteinAbundanceAssays proteinAbundanceAssays
                section

            ///Adds a proteinAbundanceStudyVariable to an existing object.
            static member addProteinAbundanceStudyVariable (proteinAbundanceStudyVariable:float) (section:ProteinSection) =
                section.ProteinAbundanceStudyVariables <- addToList section.ProteinAbundanceStudyVariables proteinAbundanceStudyVariable
                section

            ///Adds a collection of proteinAbundanceStudyVariables to an existing object.
            static member addProteinAbundanceStudyVariables (proteinAbundanceStudyVariables:seq<float>) (section:ProteinSection) =
                section.ProteinAbundanceStudyVariables <- addCollectionToList section.ProteinAbundanceStudyVariables proteinAbundanceStudyVariables
                section

            ///Adds a proteinAbundanceStandardDeviationStudyVariable to an existing object.
            static member addProteinAbundanceStandardDeviationStudyVariable (proteinAbundanceStandardDeviationStudyVariable:float) (section:ProteinSection) =
                section.ProteinAbundanceStandardDeviationStudyVariables <- addToList section.ProteinAbundanceStandardDeviationStudyVariables proteinAbundanceStandardDeviationStudyVariable
                section

            ///Adds a collection of proteinAbundanceStandardDeviationStudyVariables to an existing object.
            static member addProteinAbundanceStandardDeviationStudyVariables (proteinAbundanceStandardDeviationStudyVariables:seq<float>) (section:ProteinSection) =
                section.ProteinAbundanceStandardDeviationStudyVariables <- addCollectionToList section.ProteinAbundanceStandardDeviationStudyVariables proteinAbundanceStandardDeviationStudyVariables
                section

            ///Adds a proteinAbundanceStandardErrorStudyVariable to an existing object.
            static member addProteinAbundanceStandardErrorStudyVariable (proteinAbundanceStandardErrorStudyVariable:float) (section:ProteinSection) =
                section.ProteinAbundanceStandardErrorStudyVariables <- addToList section.ProteinAbundanceStandardErrorStudyVariables proteinAbundanceStandardErrorStudyVariable
                section

            ///Adds a collection of proteinAbundanceStandardErrorStudyVariables to an existing object.
            static member addProteinAbundanceStandardErrorStudyVariables (proteinAbundanceStandardErrorStudyVariables:seq<float>) (section:ProteinSection) =
                section.ProteinAbundanceStandardErrorStudyVariables <- addCollectionToList section.ProteinAbundanceStandardErrorStudyVariables proteinAbundanceStandardErrorStudyVariables
                section

            ///Adds a optionalInformation to an existing object.
            static member addOptionalInformation (optionalInformation:string) (section:ProteinSection) =
                section.OptionalInformations <- addToList section.OptionalInformations optionalInformation
                section

            ///Adds a collection of optionalInformations to an existing object.
            static member addOptionalInformations (optionalInformations:seq<string>) (section:ProteinSection) =
                section.OptionalInformations <- addCollectionToList section.OptionalInformations optionalInformations
                section

        type PeptideSectionHandler =
            ///Initializes a sample-object with at least all necessary parameters.
            static member init
                (             
                    ?peptideSequence                                 : string,
                    ?accession                                       : string,
                    ?unique                                          : bool,
                    ?database                                        : string,
                    ?databaseVersion                                 : string,
                    ?searchEngines                                   : seq<string*string*string*string>,
                    ?bestSearchEngineScores                          : float,
                    ?searchEngineScoresMSRuns                        : float,
                    ?reliability                                     : int,
                    ?modifications                                   : seq<string>,
                    ?retentionTime                                   : seq<float>,
                    ?retentionTimeWindow                             : seq<float>,
                    ?charge                                          : int,
                    ?massToCharge                                    : float,
                    ?uri                                             : string,
                    ?spectraRefs                                     : seq<string>,
                    ?peptideAbundanceAssay                           : seq<float>,
                    ?peptideAbundanceStudyVariable                   : seq<float>,
                    ?peptideAbundanceStandardDeviationStudyVariable  : seq<float>,
                    ?peptideAbundanceStandardErrorStudyVariables     : seq<float>,
                    ?optionalInformation                             : seq<string>
                    
                ) =

                let searchEngineScoresMSRuns'                        = defaultArg searchEngineScoresMSRuns Unchecked.defaultof<float>
                let reliability'                                     = defaultArg reliability Unchecked.defaultof<int>
                let numPSMsMSRuns'                                   = defaultArg numPSMsMSRuns Unchecked.defaultof<int>
                let numPeptidesDistinctMSRuns'                       = defaultArg numPeptidesDistinctMSRuns Unchecked.defaultof<int>
                let numPeptidesUniqueMSRuns'                         = defaultArg numPeptidesUniqueMSRuns Unchecked.defaultof<int>
                let uri'                                             = defaultArg uri Unchecked.defaultof<string>
                let goTerms'                                         = convertOptionToArray goTerms
                let proteinCoverage'                                 = defaultArg proteinCoverage Unchecked.defaultof<float>
                let proteinAbundanceAssays'                          = convertOptionToArray proteinAbundanceAssays
                let proteinAbundanceStudyVariables'                  = convertOptionToArray proteinAbundanceStudyVariables
                let proteinAbundanceStandardDeviationStudyVariables' = convertOptionToArray proteinAbundanceStandardDeviationStudyVariables
                let proteinAbundanceStandardErrorStudyVariables'     = convertOptionToArray proteinAbundanceStandardErrorStudyVariables
                let optionalInformation'                             = convertOptionToArray optionalInformation

                new PeptideSection(
                                   accession,
                                   description,
                                   taxid,
                                   species,
                                   database,
                                   databaseVersion,
                                   searchEngines.ToArray(),  
                                   bestSearchEngineScore,
                                   searchEngineScoresMSRuns',
                                   reliability',
                                   numPSMsMSRuns',
                                   numPeptidesDistinctMSRuns',
                                   numPeptidesUniqueMSRuns',
                                   ambiguityMembers.ToArray(), 
                                   modifications.ToArray(),
                                   uri',
                                   goTerms',
                                   proteinCoverage',
                                   proteinAbundanceAssays',
                                   proteinAbundanceStudyVariables',
                                   proteinAbundanceStandardDeviationStudyVariables', 
                                   proteinAbundanceStandardErrorStudyVariables',
                                   optionalInformation'
                                  )

            ///Replaces searchEngineScoresMSRuns of existing object with new one.
            static member addSearchEngineScoresMSRuns
                (searchEngineScoresMSRuns:float) (section:ProteinSection) =
                section.SearchEngineScoresMSRuns <- searchEngineScoresMSRuns
                section