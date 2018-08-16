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


        open HelperFunctions

        
        type MetaDataSectionHandler =
            ///Initializes a metaDataSection-object with at least all necessary parameters for the section, 
            ///in order to be used as a basis for all combinations of mzTabType and mzTabMode. 
            ///In order to choose the mzTabMode "Summary" put 1 in and 2 for "Complete".
            ///In order to choose the mzTabType "Identification" put 1 in and 2 for "Quantification". If both was done use "Quantification".
            static member initBaseObject
                (             
                    mzTabVersion                        : string,
                    mzTabMode                           : int,
                    mzType                              : int,     
                    description                         : string,
                    proteinSearchEngineScores           : seq<string*string*string*string>,
                    peptideSearchEngineScores           : seq<string*string*string*string>,
                    psmSearchEngineScores               : seq<string*string*string*string>,
                    smallMoleculeSearchEngineScores     : seq<string*string*string*string>,
                    fixedMods                           : seq<string*string*string*string>,
                    variableMods                        : seq<string*string*string*string>,
                    msRunLocation                       : string,
                    ?mzID                               : string,
                    ?title                              : string,
                    ?sampleProcessings                  : seq<string*string*string*string>,
                    ?instrumentNames                    : seq<string*string*string*string>,
                    ?instrumentSources                  : seq<string*string*string*string>,
                    ?instrumentAnalyzers                : seq<string*string*string*string>,
                    ?instrumentDetectors                : seq<string*string*string*string>,
                    ?softwares                          : seq<string*string*string*string>,
                    ?softwaresSettings                  : seq<string*string*string*string>,
                    ?falseDiscoveryRates                : seq<string*string*string*string>,
                    ?publications                       : seq<string>,
                    ?contactNames                       : seq<string>,
                    ?contactAffiliations                : seq<string>,
                    ?contactEMails                      : seq<string>,
                    ?uri                                : string,
                    ?fixedModSites                      : seq<string>,
                    ?fixedModPositions                  : seq<string>,
                    ?variableModSites                   : seq<string>,
                    ?variableModPositions               : seq<string>,
                    ?quantificationMethod               : string*string*string*string,
                    ?proteinQuantificationUnit          : string*string*string*string,
                    ?peptideQuantificationUnit          : string*string*string*string,
                    ?smallMoleculeQuantificationUnit    : string*string*string*string,
                    ?msRunsFormat                       : string*string*string*string,
                    ?msRunsIDFormats                    : string*string*string*string,
                    ?msRunsFragmentationMethods         : seq<string*string*string*string>,
                    ?msRunsHash                         : string,
                    ?msRunsHashMethod                   : string*string*string*string,
                    ?customs                            : seq<string*string*string*string>,
                    ?samplesSpecies                     : seq<string*string*string*string>,
                    ?sampleTissues                      : seq<string*string*string*string>,
                    ?samplesCellTypes                   : seq<string*string*string*string>,
                    ?samplesDiseases                    : seq<string*string*string*string>,
                    ?sampleDescriptions                 : seq<string>,
                    ?samplesCustoms                     : seq<string>,
                    ?assaysQuantificationReagent        : string*string*string*string,
                    ?assaysQuantificationMods           : seq<string*string*string*string>,
                    ?assaysQuantificationModsSite       : string,
                    ?assaysQuantificationModsPosition   : string,
                    ?assaysSampleRef                    : string,
                    ?assaysMSRunRef                     : string,
                    ?studyVariablesAssayRefs            : seq<string>,
                    ?studyVariablesSampleRefs           : seq<string>,
                    ?studyVariablesDescription          : seq<string>,
                    ?cvsLabel                           : seq<string>,
                    ?cvsFullName                        : seq<string>,
                    ?cvsVersion                         : seq<string>,
                    ?cvsURI                             : seq<string>,
                    ?colUnitProtein                     : string*string*string*string,
                    ?colUnitPeptide                     : string*string*string*string,
                    ?colUnitPSM                         : string*string*string*string,
                    ?colUnitSmallMolecule               : string*string*string*string
                    
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
                let uri'                              = defaultArg uri Unchecked.defaultof<string>
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
                let cvsLabel'                         = convertOptionToArray cvsLabel
                let cvsFullName'                      = convertOptionToArray cvsFullName
                let cvsVersion'                       = convertOptionToArray cvsVersion 
                let cvsURI'                           = convertOptionToArray cvsURI 
                let colUnitProtein'                   = defaultArg colUnitProtein Unchecked.defaultof<string*string*string*string>
                let colUnitPeptide'                   = defaultArg colUnitPeptide Unchecked.defaultof<string*string*string*string>
                let colUnitPSM'                       = defaultArg colUnitPSM Unchecked.defaultof<string*string*string*string>
                let colUnitSmallMolecule'             = defaultArg colUnitSmallMolecule Unchecked.defaultof<string*string*string*string>

                let createMetadatasection 
                        mzTabVersion mzTabMode mzType mzID title description sampleProcessings instrumentNames instrumentSources instrumentAnalyzers
                        instrumentDetectors softwares softwaresSettings proteinSearchEngineScores peptideSearchEngineScores psmSearchEngineScores smallMoleculeSearchEngineScores
                        falseDiscoveryRates publications contactNames contactAffiliations contactEMails uri fixedMods fixedModSites fixedModPositions variableMods
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
                     MetaDataSection.URI                              = uri
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
                        (smallMoleculeSearchEngineScores.ToArray()) falseDiscoveryRates' publications' contactNames' contactAffiliations' contactEMails' uri' (fixedMods.ToArray()) 
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

            ///Replaces uri of existing object with new one.
            static member addURI
                (uri:string) (section:MetaDataSection) =
                section.URI <- uri
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

            ///Adds a cvsLabel to an existing object.
            static member addCvsLabel (cvsLabel:string) (section:MetaDataSection) =
                section.CvsLabel <- addToList section.CvsLabel cvsLabel
                section

            ///Adds a collection of cvsLabels to an existing object.
            static member addCvsLabels (cvsLabels:seq<string>) (section:MetaDataSection) =
                section.CvsLabel <- addCollectionToList section.CvsLabel cvsLabels
                section

            ///Adds a cvsFullName to an existing object.
            static member addCvsFullName (cvsFullName:string) (section:MetaDataSection) =
                section.CvsFullName <- addToList section.CvsFullName cvsFullName
                section

            ///Adds a collection of cvsFullNames to an existing object.
            static member addCvsFullNames (cvsFullNames:seq<string>) (section:MetaDataSection) =
                section.CvsFullName <- addCollectionToList section.CvsFullName cvsFullNames
                section

            ///Adds a cvsVersion to an existing object.
            static member addCvsVersion (cvsVersion:string) (section:MetaDataSection) =
                section.CvsVersion <- addToList section.CvsVersion cvsVersion
                section

            ///Adds a collection of cvsVersions to an existing object.
            static member addCvsVersions (cvsVersions:seq<string>) (section:MetaDataSection) =
                section.CvsVersion <- addCollectionToList section.CvsVersion cvsVersions
                section

            ///Adds a cvsURI to an existing object.
            static member addCvsURI (cvsURI:string) (section:MetaDataSection) =
                section.CvsURI <- addToList section.CvsVersion cvsURI
                section

            ///Adds a collection of cvsURIs to an existing object.
            static member addCvsURIs (cvsURIs:seq<string>) (section:MetaDataSection) =
                section.CvsURI <- addCollectionToList section.CvsVersion cvsURIs
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

            static member initMetaDataSectionWithSummaryModeAndIdentificationType
                (             
                    mzTabVersion                        : string,    
                    description                         : string,
                    proteinSearchEngineScores           : seq<string*string*string*string>,
                    peptideSearchEngineScores           : seq<string*string*string*string>,
                    psmSearchEngineScores               : seq<string*string*string*string>,
                    smallMoleculeSearchEngineScores     : seq<string*string*string*string>,
                    fixedMods                           : seq<string*string*string*string>,
                    variableMods                        : seq<string*string*string*string>,
                    msRunLocation                       : string,
                    ?mzID                               : string,
                    ?title                              : string,
                    ?sampleProcessings                  : seq<string*string*string*string>,
                    ?instrumentNames                    : seq<string*string*string*string>,
                    ?instrumentSources                  : seq<string*string*string*string>,
                    ?instrumentAnalyzers                : seq<string*string*string*string>,
                    ?instrumentDetectors                : seq<string*string*string*string>,
                    ?softwares                          : seq<string*string*string*string>,
                    ?softwaresSettings                  : seq<string*string*string*string>,
                    ?falseDiscoveryRates                : seq<string*string*string*string>,
                    ?publications                       : seq<string>,
                    ?contactNames                       : seq<string>,
                    ?contactAffiliations                : seq<string>,
                    ?contactEMails                      : seq<string>,
                    ?uri                                : string,
                    ?fixedModSites                      : seq<string>,
                    ?fixedModPositions                  : seq<string>,
                    ?variableModSites                   : seq<string>,
                    ?variableModPositions               : seq<string>,
                    ?quantificationMethod               : string*string*string*string,
                    ?proteinQuantificationUnit          : string*string*string*string,
                    ?peptideQuantificationUnit          : string*string*string*string,
                    ?smallMoleculeQuantificationUnit    : string*string*string*string,
                    ?msRunsFormat                       : string*string*string*string,
                    ?msRunsIDFormats                    : string*string*string*string,
                    ?msRunsFragmentationMethods         : seq<string*string*string*string>,
                    ?msRunsHash                         : string,
                    ?msRunsHashMethod                   : string*string*string*string,
                    ?customs                            : seq<string*string*string*string>,
                    ?samplesSpecies                     : seq<string*string*string*string>,
                    ?sampleTissues                      : seq<string*string*string*string>,
                    ?samplesCellTypes                   : seq<string*string*string*string>,
                    ?samplesDiseases                    : seq<string*string*string*string>,
                    ?sampleDescriptions                 : seq<string>,
                    ?samplesCustoms                     : seq<string>,
                    ?assaysSampleRef                    : string,
                    ?assaysMSRunRef                     : string,
                    ?studyVariablesAssayRefs            : seq<string>,
                    ?studyVariablesSampleRefs           : seq<string>,
                    ?studyVariablesDescription          : seq<string>,
                    ?cvsLabel                           : seq<string>,
                    ?cvsFullName                        : seq<string>,
                    ?cvsVersion                         : seq<string>,
                    ?cvsURI                             : seq<string>,
                    ?colUnitProtein                     : string*string*string*string,
                    ?colUnitPeptide                     : string*string*string*string,
                    ?colUnitPSM                         : string*string*string*string,
                    ?colUnitSmallMolecule               : string*string*string*string
                    
                ) =

                let mzID'                            = defaultArg mzID Unchecked.defaultof<string>
                let title'                           = defaultArg title Unchecked.defaultof<string>
                let sampleProcessings'               = convertOptionToArray sampleProcessings 
                let instrumentNames'                 = convertOptionToArray instrumentNames 
                let instrumentSources'               = convertOptionToArray instrumentSources
                let instrumentAnalyzers'             = convertOptionToArray instrumentAnalyzers 
                let instrumentDetectors'             = convertOptionToArray instrumentDetectors
                let softwares'                       = convertOptionToArray softwares
                let softwaresSettings'               = convertOptionToArray softwaresSettings
                let falseDiscoveryRates'             = convertOptionToArray falseDiscoveryRates
                let publications'                    = convertOptionToArray publications 
                let contactNames'                    = convertOptionToArray contactNames
                let contactAffiliations'             = convertOptionToArray contactAffiliations
                let contactEMails'                   = convertOptionToArray contactEMails
                let uri'                             = defaultArg uri Unchecked.defaultof<string>
                let fixedModSites'                   = convertOptionToArray fixedModSites
                let fixedModPositions'               = convertOptionToArray fixedModPositions
                let variableModSites'                = convertOptionToArray variableModSites
                let variableModPositions'            = convertOptionToArray variableModPositions
                let quantificationMethod'            = defaultArg quantificationMethod Unchecked.defaultof<string*string*string*string>
                let proteinQuantificationUnit'       = defaultArg proteinQuantificationUnit Unchecked.defaultof<string*string*string*string>
                let peptideQuantificationUnit'       = defaultArg peptideQuantificationUnit Unchecked.defaultof<string*string*string*string>
                let smallMoleculeQuantificationUnit' = defaultArg smallMoleculeQuantificationUnit Unchecked.defaultof<string*string*string*string>
                let msRunsFormat'                    = defaultArg msRunsFormat Unchecked.defaultof<string*string*string*string>
                let msRunsIDFormats'                 = defaultArg msRunsIDFormats Unchecked.defaultof<string*string*string*string>
                let msRunsFragmentationMethods'      = convertOptionToArray msRunsFragmentationMethods
                let msRunsHash'                      = defaultArg msRunsHash Unchecked.defaultof<string>
                let msRunsHashMethod'                = defaultArg msRunsHashMethod Unchecked.defaultof<string*string*string*string>
                let customs'                         = convertOptionToArray customs
                let samplesSpecies'                  = convertOptionToArray samplesSpecies
                let sampleTissues'                   = convertOptionToArray sampleTissues
                let samplesCellTypes'                = convertOptionToArray samplesCellTypes
                let samplesDiseases'                 = convertOptionToArray samplesDiseases
                let sampleDescriptions'              = convertOptionToArray sampleDescriptions
                let samplesCustoms'                  = convertOptionToArray samplesCustoms
                let assaysSampleRef'                 = defaultArg assaysSampleRef Unchecked.defaultof<string>
                let assaysMSRunRef'                  = defaultArg assaysMSRunRef Unchecked.defaultof<string>
                let studyVariablesAssayRefs'         = convertOptionToArray studyVariablesAssayRefs
                let studyVariablesSampleRefs'        = convertOptionToArray studyVariablesSampleRefs
                let studyVariablesDescription'       = convertOptionToArray studyVariablesDescription
                let cvsLabel'                        = convertOptionToArray cvsLabel
                let cvsFullName'                     = convertOptionToArray cvsFullName
                let cvsVersion'                      = convertOptionToArray cvsVersion 
                let cvsURI'                          = convertOptionToArray cvsURI 
                let colUnitProtein'                  = defaultArg colUnitProtein Unchecked.defaultof<string*string*string*string>
                let colUnitPeptide'                  = defaultArg colUnitPeptide Unchecked.defaultof<string*string*string*string>
                let colUnitPSM'                      = defaultArg colUnitPSM Unchecked.defaultof<string*string*string*string>
                let colUnitSmallMolecule'            = defaultArg colUnitSmallMolecule Unchecked.defaultof<string*string*string*string>

                let createMetadatasectionWithSummaryModeAndIdentificationType
                        mzTabVersion mzID title description sampleProcessings instrumentNames instrumentSources instrumentAnalyzers
                        instrumentDetectors softwares softwaresSettings proteinSearchEngineScores peptideSearchEngineScores psmSearchEngineScores smallMoleculeSearchEngineScores
                        falseDiscoveryRates publications contactNames contactAffiliations contactEMails uri fixedMods fixedModSites fixedModPositions variableMods
                        variableModSites variableModPositions quantificationMethod proteinQuantificationUnit peptideQuantificationUnit smallMoleculeQuantificationUnit
                        msRunsFormat msRunLocation msRunsIDFormats msRunsFragmentationMethods msRunsHash msRunsHashMethod customs samplesSpecies sampleTissues samplesCellTypes
                        samplesDiseases sampleDescriptions samplesCustoms assaysSampleRef assaysMSRunRef studyVariablesAssayRefs studyVariablesSampleRefs studyVariablesDescription cvsLabel 
                        cvsFullName cvsVersion cvsURI colUnitProtein colUnitPeptide colUnitPSM colUnitSmallMolecule
                        =
                    {
                     MetaDataSection.MzTabVersion                     = mzTabVersion
                     MetaDataSection.MzTabMode                        = MzTabMode.Summary
                     MetaDataSection.MzType                           = MzTabType.Identification
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
                     MetaDataSection.URI                              = uri
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
                     MetaDataSection.AssaysQuantificationReagent      = Unchecked.defaultof<string*string*string*string>
                     MetaDataSection.AssaysQuantificationMods         = Unchecked.defaultof<array<string*string*string*string>>
                     MetaDataSection.AssaysQuantificationModsSite     = null
                     MetaDataSection.AssaysQuantificationModsPosition = null
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

                createMetadatasectionWithSummaryModeAndIdentificationType
                        mzTabVersion mzID' title' description sampleProcessings' instrumentNames' instrumentSources' instrumentAnalyzers' instrumentDetectors' 
                        softwares' softwaresSettings' (proteinSearchEngineScores.ToArray()) (peptideSearchEngineScores.ToArray()) (psmSearchEngineScores.ToArray()) 
                        (smallMoleculeSearchEngineScores.ToArray()) falseDiscoveryRates' publications' contactNames' contactAffiliations' contactEMails' uri' (fixedMods.ToArray()) 
                        fixedModSites' fixedModPositions' (variableMods.ToArray()) variableModSites' variableModPositions' quantificationMethod' proteinQuantificationUnit' 
                        peptideQuantificationUnit' smallMoleculeQuantificationUnit' msRunsFormat' msRunLocation msRunsIDFormats' msRunsFragmentationMethods' msRunsHash' 
                        msRunsHashMethod' customs' samplesSpecies' sampleTissues' samplesCellTypes' samplesDiseases' sampleDescriptions' samplesCustoms'
                        assaysSampleRef' assaysMSRunRef' studyVariablesAssayRefs' studyVariablesSampleRefs' studyVariablesDescription' cvsLabel' 
                        cvsFullName' cvsVersion' cvsURI' colUnitProtein' colUnitPeptide'  colUnitPSM' colUnitSmallMolecule'

        type ProteinSectionHandler =
            ///Initializes a proteinSection-object with at least all necessary parameters for the section, 
            ///in order to be used as a basis for all combinations of mzTabType and mzTabMode.
            static member initBaseObject
                (             
                    accession                                           : string,
                    description                                         : string,
                    taxid                                               : int,
                    species                                             : string,
                    database                                            : string,
                    databaseVersion                                     : string,
                    searchEngines                                       : seq<string*string*string*string>,
                    bestSearchEngineScore                               : seq<float>,
                    ambiguityMembers                                    : seq<string>,
                    modifications                                       : seq<string>,
                    ?searchEngineScoresMSRuns                           : seq<float>,
                    ?reliability                                        : int,
                    ?numPSMsMSRuns                                      : seq<int>,
                    ?numPeptidesDistinctMSRuns                          : seq<int>,
                    ?numPeptidesUniqueMSRuns                            : seq<int>,
                    ?uri                                                : string,
                    ?goTerms                                            : seq<string>, 
                    ?proteinCoverage                                    : float,
                    ?proteinAbundanceAssays                             : seq<float>,
                    ?proteinAbundanceStudyVariables                     : seq<float>,
                    ?proteinAbundanceStandardDeviationStudyVariables    : seq<float>,
                    ?proteinAbundanceStandardErrorStudyVariables        : seq<float>,
                    ?optionalInformation                                : seq<string>
                    
                ) =

                let searchEngineScoresMSRuns'                        = convertOptionToArray searchEngineScoresMSRuns
                let reliability'                                     = defaultArg reliability Unchecked.defaultof<int>
                let numPSMsMSRuns'                                   = convertOptionToArray numPSMsMSRuns
                let numPeptidesDistinctMSRuns'                       = convertOptionToArray numPeptidesDistinctMSRuns
                let numPeptidesUniqueMSRuns'                         = convertOptionToArray numPeptidesUniqueMSRuns
                let uri'                                             = defaultArg uri Unchecked.defaultof<string>
                let goTerms'                                         = convertOptionToArray goTerms
                let proteinCoverage'                                 = defaultArg proteinCoverage Unchecked.defaultof<float>
                let proteinAbundanceAssays'                          = convertOptionToArray proteinAbundanceAssays
                let proteinAbundanceStudyVariables'                  = convertOptionToArray proteinAbundanceStudyVariables
                let proteinAbundanceStandardDeviationStudyVariables' = convertOptionToArray proteinAbundanceStandardDeviationStudyVariables
                let proteinAbundanceStandardErrorStudyVariables'     = convertOptionToArray proteinAbundanceStandardErrorStudyVariables
                let optionalInformation'                             = convertOptionToArray optionalInformation

                let createProteinSection
                        accession description taxid species database databaseVersion searchEngines bestSearchEngineScore
                        searchEngineScoresMSRuns reliability numPSMsMSRuns numPeptidesDistinctMSRuns numPeptidesUniqueMSRuns
                        ambiguityMembers modifications uri goTerms proteinCoverage proteinAbundanceAssays proteinAbundanceStudyVariables
                        proteinAbundanceStandardDeviationStudyVariables proteinAbundanceStandardErrorStudyVariables 
                        optionalInformation
                        =
                    {
                     ProteinSection.Accession                                       = accession 
                     ProteinSection.Description                                     = description 
                     ProteinSection.Taxid                                           = taxid 
                     ProteinSection.Species                                         = species 
                     ProteinSection.Database                                        = database 
                     ProteinSection.DatabaseVersion                                 = databaseVersion 
                     ProteinSection.SearchEngines                                   = searchEngines 
                     ProteinSection.BestSearchEngineScore                           = bestSearchEngineScore
                     ProteinSection.SearchEngineScoresMSRuns                        = searchEngineScoresMSRuns 
                     ProteinSection.Reliability                                     = reliability 
                     ProteinSection.NumPSMsMSRuns                                   = numPSMsMSRuns 
                     ProteinSection.NumPeptidesDistinctMSRuns                       = numPeptidesDistinctMSRuns 
                     ProteinSection.NumPeptidesUniqueMSRuns                         = numPeptidesUniqueMSRuns
                     ProteinSection.AmbiguityMembers                                = ambiguityMembers 
                     ProteinSection.Modifications                                   = modifications 
                     ProteinSection.URI                                             = uri 
                     ProteinSection.GoTerms                                         = goTerms 
                     ProteinSection.ProteinCoverage                                 = proteinCoverage 
                     ProteinSection.ProteinAbundanceAssays                          = proteinAbundanceAssays 
                     ProteinSection.ProteinAbundanceStudyVariables                  = proteinAbundanceStudyVariables
                     ProteinSection.ProteinAbundanceStandardDeviationStudyVariables = proteinAbundanceStandardDeviationStudyVariables 
                     ProteinSection.ProteinAbundanceStandardErrorStudyVariables     = proteinAbundanceStandardErrorStudyVariables 
                     ProteinSection.OptionalInformations                            = optionalInformation
                    }
                createProteinSection 
                    accession description taxid species database databaseVersion (searchEngines.ToArray()) (bestSearchEngineScore.ToArray())
                    searchEngineScoresMSRuns' reliability' numPSMsMSRuns' numPeptidesDistinctMSRuns' numPeptidesUniqueMSRuns'
                    (ambiguityMembers.ToArray()) (modifications.ToArray()) uri' goTerms' proteinCoverage' proteinAbundanceAssays'
                    proteinAbundanceStudyVariables' proteinAbundanceStandardDeviationStudyVariables' proteinAbundanceStandardErrorStudyVariables'
                    optionalInformation'

            ///Adds a searchEngineScoresMSRun to an existing object.
            static member addSearchEngineScoresMSRun (searchEngineScoresMSRun:float) (section:ProteinSection) =
                section.SearchEngineScoresMSRuns <- addToList section.SearchEngineScoresMSRuns searchEngineScoresMSRun
                section

            ///Adds a collection of searchEngineScoresMSRuns to an existing object.
            static member addSearchEngineScoresMSRuns (searchEngineScoresMSRuns:seq<float>) (section:ProteinSection) =
                section.SearchEngineScoresMSRuns <- addCollectionToList section.SearchEngineScoresMSRuns searchEngineScoresMSRuns
                section

            ///Replaces reliability of existing object with new one.
            static member addReliability
                (reliability:int) (section:ProteinSection) =
                section.Reliability <- reliability
                section

            ///Adds a numPSMsMSRun to an existing object.
            static member addNumPSMsMSRun (numPSMsMSRun:int) (section:ProteinSection) =
                section.NumPSMsMSRuns <- addToList section.NumPSMsMSRuns numPSMsMSRun
                section

            ///Adds a collection of numPSMsMSRuns to an existing object.
            static member addNumPSMsMSRuns (numPSMsMSRuns:seq<int>) (section:ProteinSection) =
                section.NumPSMsMSRuns <- addCollectionToList section.NumPSMsMSRuns numPSMsMSRuns
                section

            ///Adds a numPeptidesDistinctMSRun to an existing object.
            static member addNumPeptidesDistinctMSRun (numPeptidesDistinctMSRun:int) (section:ProteinSection) =
                section.NumPeptidesDistinctMSRuns <- addToList section.NumPeptidesDistinctMSRuns numPeptidesDistinctMSRun
                section

            ///Adds a collection of numPeptidesDistinctMSRuns to an existing object.
            static member addNumPeptidesDistinctMSRuns (numPeptidesDistinctMSRuns:seq<int>) (section:ProteinSection) =
                section.NumPeptidesDistinctMSRuns <- addCollectionToList section.NumPeptidesDistinctMSRuns numPeptidesDistinctMSRuns
                section

            ///Adds a numPeptidesUniqueMSRun to an existing object.
            static member numPeptidesUniqueMSRun (numPeptidesUniqueMSRun:int) (section:ProteinSection) =
                section.NumPeptidesUniqueMSRuns <- addToList section.NumPeptidesUniqueMSRuns numPeptidesUniqueMSRun
                section

            ///Adds a collection of numPeptidesUniqueMSRuns to an existing object.
            static member numPeptidesUniqueMSRuns (numPeptidesUniqueMSRuns:seq<int>) (section:ProteinSection) =
                section.NumPeptidesUniqueMSRuns <- addCollectionToList section.NumPeptidesUniqueMSRuns numPeptidesUniqueMSRuns
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
            ///Initializes a peptideSection-object with at least all necessary parameters for the section, 
            ///in order to be used as a basis for all combinations of mzTabType and mzTabMode.
            static member initBaseObject
                (             
                    ?peptideSequence                                    : string,
                    ?accession                                          : string,
                    ?unique                                             : bool,
                    ?database                                           : string,
                    ?databaseVersion                                    : string,
                    ?searchEngines                                      : seq<string*string*string*string>,
                    ?bestSearchEngineScores                             : seq<float>,
                    ?searchEngineScoresMSRuns                           : seq<float>,
                    ?reliability                                        : int,
                    ?modifications                                      : seq<string>,
                    ?retentionTime                                      : seq<float>,
                    ?retentionTimeWindow                                : seq<float>,
                    ?charge                                             : int,
                    ?massToCharge                                       : float,
                    ?uri                                                : string,
                    ?spectraRefs                                        : seq<string>,
                    ?peptideAbundanceAssays                             : seq<float>,
                    ?peptideAbundanceStudyVariables                     : seq<float>,
                    ?peptideAbundanceStandardDeviationStudyVariables    : seq<float>,
                    ?peptideAbundanceStandardErrorStudyVariables        : seq<float>,
                    ?optionalInformation                                : seq<string>
                    
                ) =

                let peptideSequence'                                 = defaultArg peptideSequence Unchecked.defaultof<string>
                let accession'                                       = defaultArg accession Unchecked.defaultof<string>
                let unique'                                          = defaultArg unique Unchecked.defaultof<bool>
                let database'                                        = defaultArg database Unchecked.defaultof<string>
                let databaseVersion'                                 = defaultArg databaseVersion Unchecked.defaultof<string>
                let searchEngines'                                   = convertOptionToArray searchEngines
                let bestSearchEngineScores'                          = convertOptionToArray bestSearchEngineScores
                let searchEngineScoresMSRuns'                        = convertOptionToArray searchEngineScoresMSRuns
                let reliability'                                     = defaultArg reliability Unchecked.defaultof<int>
                let modifications'                                   = convertOptionToArray modifications
                let retentionTime'                                   = convertOptionToArray retentionTime
                let retentionTimeWindow'                             = convertOptionToArray retentionTimeWindow
                let charge'                                          = defaultArg charge Unchecked.defaultof<int>
                let massToCharge'                                    = defaultArg massToCharge Unchecked.defaultof<float>
                let uri'                                             = defaultArg uri Unchecked.defaultof<string>
                let spectraRefs'                                     = convertOptionToArray spectraRefs 
                let peptideAbundanceAssays'                          = convertOptionToArray peptideAbundanceAssays
                let peptideAbundanceStudyVariables'                  = convertOptionToArray peptideAbundanceStudyVariables
                let peptideAbundanceStandardDeviationStudyVariables' = convertOptionToArray peptideAbundanceStandardDeviationStudyVariables 
                let peptideAbundanceStandardErrorStudyVariables'     = convertOptionToArray peptideAbundanceStandardErrorStudyVariables
                let optionalInformation'                             = convertOptionToArray optionalInformation

                let createPeptideSection
                        peptideSequence accession unique database databaseVersion searchEngines bestSearchEngineScores 
                        searchEngineScoresMSRuns reliability  modifications retentionTime retentionTimeWindow charge massToCharge 
                        uri spectraRefs peptideAbundanceAssays peptideAbundanceStudyVariables peptideAbundanceStandardDeviationStudyVariables
                        peptideAbundanceStandardErrorStudyVariables optionalInformation
                        =

                    { 
                     PeptideSection.PeptideSequence                                 = peptideSequence
                     PeptideSection.Accession                                       = accession
                     PeptideSection.Unique                                          = unique
                     PeptideSection.Database                                        = database
                     PeptideSection.DatabaseVersion                                 = databaseVersion
                     PeptideSection.SearchEngines                                   = searchEngines
                     PeptideSection.BestSearchEngineScores                          = bestSearchEngineScores
                     PeptideSection.SearchEngineScoresMSRuns                        = searchEngineScoresMSRuns
                     PeptideSection.Reliability                                     = reliability
                     PeptideSection.Modifications                                   = modifications
                     PeptideSection.RetentionTime                                   = retentionTime
                     PeptideSection.RetentionTimeWindow                             = retentionTimeWindow
                     PeptideSection.Charge                                          = charge
                     PeptideSection.MassToCharge                                    = massToCharge
                     PeptideSection.URI                                             = uri
                     PeptideSection.SpectraRefs                                     = spectraRefs
                     PeptideSection.PeptideAbundanceAssays                          = peptideAbundanceAssays
                     PeptideSection.PeptideAbundanceStudyVariables                  = peptideAbundanceStudyVariables
                     PeptideSection.PeptideAbundanceStandardDeviationStudyVariables = peptideAbundanceStandardDeviationStudyVariables
                     PeptideSection.PeptideAbundanceStandardErrorStudyVariables     = peptideAbundanceStandardErrorStudyVariables
                     PeptideSection.OptionalInformations                            = optionalInformation
                    }

                createPeptideSection
                    peptideSequence' accession' unique' database' databaseVersion' searchEngines' bestSearchEngineScores' 
                    searchEngineScoresMSRuns' reliability'  modifications' retentionTime' retentionTimeWindow' charge' massToCharge' 
                    uri' spectraRefs' peptideAbundanceAssays' peptideAbundanceStudyVariables' peptideAbundanceStandardDeviationStudyVariables'
                    peptideAbundanceStandardErrorStudyVariables' optionalInformation'

            ///Replaces peptideSequence of existing object with new one.
            static member addPeptideSequence
                (peptideSequence:string) (section:PeptideSection) =
                section.PeptideSequence <- peptideSequence
                section

            ///Replaces accession of existing object with new one.
            static member addAccession
                (accession:string) (section:PeptideSection) =
                section.Accession <- accession
                section
            
            ///Replaces unique of existing object with new one.
            static member addUnique
                (unique:bool) (section:PeptideSection) =
                section.Unique <- unique
                section

            ///Replaces database of existing object with new one.
            static member addDatabase
                (database:string) (section:PeptideSection) =
                section.Database <- database
                section

            ///Replaces databaseVersion of existing object with new one.
            static member addDatabaseVersion
                (databaseVersion:string) (section:PeptideSection) =
                section.DatabaseVersion <- databaseVersion
                section

            ///Adds a searchEngine to an existing object.
            static member addSearchEngine (searchEngine:string*string*string*string) (section:PeptideSection) =
                section.SearchEngines <- addToList section.SearchEngines searchEngine
                section

            ///Adds a collection of searchEngines to an existing object.
            static member addSearchEngines (searchEngines:seq<string*string*string*string>) (section:PeptideSection) =
                section.SearchEngines <- addCollectionToList section.SearchEngines searchEngines
                section
            
            ///Adds a bestSearchEngineScore to an existing object.
            static member addBestSearchEngineScore (bestSearchEngineScore:float) (section:PeptideSection) =
                section.BestSearchEngineScores <- addToList section.BestSearchEngineScores bestSearchEngineScore
                section

            ///Adds a collection of bestSearchEngineScores to an existing object.
            static member addBestSearchEngineScores (bestSearchEngineScores:seq<float>) (section:PeptideSection) =
                section.BestSearchEngineScores <- addCollectionToList section.BestSearchEngineScores bestSearchEngineScores
                section
            
            ///Adds a searchEngineScoresMSRun to an existing object.
            static member addSearchEngineScoresMSRun (searchEngineScoresMSRun:float) (section:PeptideSection) =
                section.SearchEngineScoresMSRuns <- addToList section.SearchEngineScoresMSRuns searchEngineScoresMSRun
                section

            ///Adds a collection of searchEngineScoresMSRuns to an existing object.
            static member addSearchEngineScoresMSRuns (searchEngineScoresMSRuns:seq<float>) (section:PeptideSection) =
                section.SearchEngineScoresMSRuns <- addCollectionToList section.SearchEngineScoresMSRuns searchEngineScoresMSRuns
                section

            ///Replaces reliability of existing object with new one.
            static member addReliability
                (reliability:int) (section:PeptideSection) =
                section.Reliability <- reliability

            ///Adds a modification to an existing object.
            static member addModification (modification:string) (section:PeptideSection) =
                section.Modifications <- addToList section.Modifications modification
                section

            ///Adds a collection of modifications to an existing object.
            static member addModifications (modifications:seq<string>) (section:PeptideSection) =
                section.Modifications <- addCollectionToList section.Modifications modifications
                section

            ///Adds a retentionTime to an existing object.
            static member addRetentionTime (retentionTime:float) (section:PeptideSection) =
                section.RetentionTime <- addToList section.RetentionTime retentionTime
                section

            ///Adds a collection of retentionTimes to an existing object.
            static member addRetentionTimes (retentionTimes:seq<float>) (section:PeptideSection) =
                section.RetentionTime <- addCollectionToList section.RetentionTime retentionTimes
                section

            ///Adds a retentionTimeWindow to an existing object.
            static member addRetentionTimeWindow (retentionTimeWindow:seq<float>) (section:PeptideSection) =
                section.RetentionTimeWindow <- addCollectionToList section.RetentionTimeWindow retentionTimeWindow
                section

            ///Replaces charge of existing object with new one.
            static member addCharge
                (charge:int) (section:PeptideSection) =
                section.Charge <- charge
            
            ///Replaces massToCharge of existing object with new one.
            static member addMassToCharge
                (massToCharge:float) (section:PeptideSection) =
                section.MassToCharge <- massToCharge

            ///Replaces uri of existing object with new one.
            static member addURI
                (uri:string) (section:PeptideSection) =
                section.URI <- uri

            ///Adds a spectraRef to an existing object.
            static member addSpectraRef (spectraRef:string) (section:PeptideSection) =
                section.SpectraRefs <- addToList section.SpectraRefs spectraRef
                section

            ///Adds a collection of spectraRefs to an existing object.
            static member addSpectraRefs (spectraRefs:seq<string>) (section:PeptideSection) =
                section.SpectraRefs <- addCollectionToList section.SpectraRefs spectraRefs
                section

            ///Adds a peptideAbundanceAssay to an existing object.
            static member addPeptideAbundanceAssay (peptideAbundanceAssay:float) (section:PeptideSection) =
                section.PeptideAbundanceAssays <- addToList section.PeptideAbundanceAssays peptideAbundanceAssay
                section

            ///Adds a collection of peptideAbundanceAssays to an existing object.
            static member addPeptideAbundanceAssays (peptideAbundanceAssays:seq<float>) (section:PeptideSection) =
                section.PeptideAbundanceAssays <- addCollectionToList section.PeptideAbundanceAssays peptideAbundanceAssays
                section

            ///Adds a peptideAbundanceStudyVariable to an existing object.
            static member addPeptideAbundanceStudyVariable (peptideAbundanceStudyVariable:float) (section:PeptideSection) =
                section.PeptideAbundanceStudyVariables <- addToList section.PeptideAbundanceStudyVariables peptideAbundanceStudyVariable
                section

            ///Adds a collection of peptideAbundanceStudyVariables to an existing object.
            static member addPeptideAbundanceStudyVariables (peptideAbundanceStudyVariables:seq<float>) (section:PeptideSection) =
                section.PeptideAbundanceStudyVariables <- addCollectionToList section.PeptideAbundanceStudyVariables peptideAbundanceStudyVariables
                section

            ///Adds a peptideAbundanceStandardDeviationStudyVariable to an existing object.
            static member addPeptideAbundanceStandardDeviationStudyVariable (peptideAbundanceStandardDeviationStudyVariable:float) (section:PeptideSection) =
                section.PeptideAbundanceStandardDeviationStudyVariables <- addToList section.PeptideAbundanceStandardDeviationStudyVariables peptideAbundanceStandardDeviationStudyVariable
                section

            ///Adds a collection of peptideAbundanceStandardDeviationStudyVariables to an existing object.
            static member addPeptideAbundanceStandardDeviationStudyVariables (peptideAbundanceStandardDeviationStudyVariables:seq<float>) (section:PeptideSection) =
                section.PeptideAbundanceStandardDeviationStudyVariables <- addCollectionToList section.PeptideAbundanceStandardDeviationStudyVariables peptideAbundanceStandardDeviationStudyVariables
                section

            ///Adds a peptideAbundanceStandardErrorStudyVariable to an existing object.
            static member addPeptideAbundanceStandardErrorStudyVariable (peptideAbundanceStandardErrorStudyVariable:float) (section:PeptideSection) =
                section.PeptideAbundanceStandardErrorStudyVariables <- addToList section.PeptideAbundanceStandardErrorStudyVariables peptideAbundanceStandardErrorStudyVariable
                section

            ///Adds a collection of peptideAbundanceStandardErrorStudyVariables to an existing object.
            static member addPeptideAbundanceStandardErrorStudyVariables (peptideAbundanceStandardErrorStudyVariables:seq<float>) (section:PeptideSection) =
                section.PeptideAbundanceStandardErrorStudyVariables <- addCollectionToList section.PeptideAbundanceStandardErrorStudyVariables peptideAbundanceStandardErrorStudyVariables
                section

            ///Adds a optionalInformation to an existing object.
            static member addOptionalInformation (optionalInformation:string) (section:PeptideSection) =
                section.OptionalInformations <- addToList section.OptionalInformations optionalInformation
                section

            ///Adds a collection of optionalInformations to an existing object.
            static member addOptionalInformations (optionalInformations:seq<string>) (section:PeptideSection) =
                section.OptionalInformations <- addCollectionToList section.OptionalInformations optionalInformations
                section

        type PSMSectionHandler =
            ///Initializes a psmSection-object with at least all necessary parameters for the section, 
            ///in order to be used as a basis for all combinations of mzTabType and mzTabMode.
            static member initBaseObject
                (             
                    peptideSequence         : string,
                    psmID                   : string,
                    accession               : string,
                    unique                  : bool,
                    database                : string,
                    databaseVersion         : string,
                    searchEngines           : seq<string*string*string*string>,
                    searchEngineScores      : seq<float>,
                    modifications           : seq<string>,
                    retentionTimes          : seq<float>,
                    charge                  : int,
                    expMassToCharge         : float,
                    calcMassToCharge        : float,
                    spectraRefs             : seq<string>,
                    pre                     : string,
                    post                    : string,
                    start                   : int,
                    ende                    : int,
                    ?reliability            : int,
                    ?uri                    : string,
                    ?optionalInformation    : seq<string>
                    
                ) =

                let reliability'         = defaultArg reliability Unchecked.defaultof<int>
                let uri'                 = defaultArg uri Unchecked.defaultof<string>
                let optionalInformation' = convertOptionToArray optionalInformation

                let createPSMSection
                        peptideSequence  psmID accession  unique database databaseVersion searchEngines searchEngineScores
                        reliability modifications retentionTimes charge expMassToCharge calcMassToCharge uri spectraRefs
                        pre post start ende optionalInformation
                        =
                    {
                     PSMSection.PeptideSequence      = peptideSequence
                     PSMSection.PSMID                = psmID
                     PSMSection.Accession            = accession
                     PSMSection.Unique               = unique
                     PSMSection.Database             = database
                     PSMSection.DatabaseVersion      = databaseVersion
                     PSMSection.SearchEngines        = searchEngines
                     PSMSection.SearchEngineScores   = searchEngineScores
                     PSMSection.Reliability          = reliability
                     PSMSection.Modifications        = modifications
                     PSMSection.RetentionTimes       = retentionTimes
                     PSMSection.Charge               = charge
                     PSMSection.ExpMassToCharge      = expMassToCharge
                     PSMSection.CalcMassToCharge     = calcMassToCharge
                     PSMSection.URI                  = uri
                     PSMSection.SpectraRefs          = spectraRefs
                     PSMSection.Pre                  = pre
                     PSMSection.Post                 = post
                     PSMSection.Start                = start
                     PSMSection.End                  = ende
                     PSMSection.OptionalInformations = optionalInformation
                    }
                        
                createPSMSection     
                    peptideSequence psmID accession unique database databaseVersion (searchEngines.ToArray()) 
                    (searchEngineScores.ToArray()) reliability' (modifications.ToArray()) (retentionTimes.ToArray()) charge 
                    expMassToCharge calcMassToCharge uri' (spectraRefs.ToArray()) pre post start ende optionalInformation'

            ///Replaces reliability of existing object with new one.
            static member addReliability
                (reliability:int) (section:PSMSection) =
                section.Reliability <- reliability

            ///Replaces uri of existing object with new one.
            static member addURI
                (uri:string) (section:PSMSection) =
                section.URI <- uri
           
            ///Adds a optionalInformation to an existing object.
            static member addOptionalInformation (optionalInformation:string) (section:PSMSection) =
                    section.OptionalInformations <- addToList section.OptionalInformations optionalInformation
                    section

            ///Adds a collection of optionalInformations to an existing object.
            static member addOptionalInformations (optionalInformations:seq<string>) (section:PSMSection) =
                    section.OptionalInformations <- addCollectionToList section.OptionalInformations optionalInformations
                    section

        type SmallMoleculeSectionHandler =
            ///Initializes a smallMoleculeSection-object with at least all necessary parameters for the section, 
            ///in order to be used as a basis for all combinations of mzTabType and mzTabMode.
            static member initBaseObject
                (             
                    identifiers                                             : seq<string>,
                    chemicalFormula                                         : string,
                    smiles                                                  : seq<string>,
                    inchiKey                                                : seq<string>,
                    description                                             : seq<string>,
                    expMassToCharge                                         : float,
                    calcMassToCharge                                        : float,
                    charge                                                  : int,
                    retentionTimes                                          : seq<float>,
                    taxid                                                   : int,
                    species                                                 : string,
                    database                                                : string,
                    databaseVersion                                         : string,                             
                    spectraRefs                                             : seq<string>,
                    searchEngines                                           : seq<string*string*string*string>,
                    bestSearchEngineScores                                  : seq<float>,                    
                    modifications                                           : array<string>,                   
                    ?reliability                                            : int,
                    ?uri                                                    : string,
                    ?searchEngineScoresMSRuns                               : seq<float>,
                    ?smallMoleculeAbundanceAssays                           : seq<float>,
                    ?smallMoleculeAbundanceStudyVariables                   : seq<float>,
                    ?smallMoleculeAbundanceStandardDeviationStudyVariables  : seq<float>,
                    ?smallMoleculeAbundanceStandardErrorStudyVariables      : seq<float>,
                    ?optionalInformations                                   : seq<string>
                ) =

                let reliability'                                           = defaultArg reliability Unchecked.defaultof<int>
                let uri'                                                   = defaultArg uri Unchecked.defaultof<string>
                let searchEngineScoresMSRuns'                              = convertOptionToArray searchEngineScoresMSRuns
                let smallMoleculeAbundanceAssays'                          = convertOptionToArray smallMoleculeAbundanceAssays
                let smallMoleculeAbundanceStudyVariables'                  = convertOptionToArray smallMoleculeAbundanceStudyVariables
                let smallMoleculeAbundanceStandardDeviationStudyVariables' = convertOptionToArray smallMoleculeAbundanceStandardDeviationStudyVariables
                let smallMoleculeAbundanceStandardErrorStudyVariables'     = convertOptionToArray smallMoleculeAbundanceStandardErrorStudyVariables
                let optionalInformations'                                  = convertOptionToArray optionalInformations

                let createSmallMoleculeSection
                        identifiers chemicalFormula smiles inchiKey description expMassToCharge calcMassToCharge charge
                        retentionTimes taxid species database databaseVersion reliability uri spectraRefs searchEngines
                        bestSearchEngineScores searchEngineScoresMSRuns modifications smallMoleculeAbundanceAssays
                        smallMoleculeAbundanceStudyVariables smallMoleculeAbundanceStandardDeviationStudyVariables
                        smallMoleculeAbundanceStandardErrorStudyVariables optionalInformations
                        =
                    {
                     SmallMoleculeSection.Identifiers                                           = identifiers 
                     SmallMoleculeSection.ChemicalFormula                                       = chemicalFormula 
                     SmallMoleculeSection.Smiles                                                = smiles 
                     SmallMoleculeSection.InchiKey                                              = inchiKey 
                     SmallMoleculeSection.Description                                           = description 
                     SmallMoleculeSection.ExpMassToCharge                                       = expMassToCharge 
                     SmallMoleculeSection.CalcMassToCharge                                      = calcMassToCharge
                     SmallMoleculeSection.Charge                                                = charge
                     SmallMoleculeSection.RetentionTimes                                        = retentionTimes 
                     SmallMoleculeSection.Taxid                                                 = taxid 
                     SmallMoleculeSection.Species                                               = species 
                     SmallMoleculeSection.Database                                              = database 
                     SmallMoleculeSection.DatabaseVersion                                       = databaseVersion 
                     SmallMoleculeSection.Reliability                                           = reliability 
                     SmallMoleculeSection.URI                                                   = uri 
                     SmallMoleculeSection.SpectraRefs                                           = spectraRefs 
                     SmallMoleculeSection.SearchEngines                                         = searchEngines
                     SmallMoleculeSection.BestSearchEngineScores                                = bestSearchEngineScores 
                     SmallMoleculeSection.SearchEngineScoresMSRuns                              = searchEngineScoresMSRuns 
                     SmallMoleculeSection.Modifications                                         = modifications 
                     SmallMoleculeSection.SmallMoleculeAbundanceAssays                          = smallMoleculeAbundanceAssays
                     SmallMoleculeSection.SmallMoleculeAbundanceStudyVariables                  = smallMoleculeAbundanceStudyVariables 
                     SmallMoleculeSection.SmallMoleculeAbundanceStandardDeviationStudyVariables = smallMoleculeAbundanceStandardDeviationStudyVariables
                     SmallMoleculeSection.SmallMoleculeAbundanceStandardErrorStudyVariables     = smallMoleculeAbundanceStandardErrorStudyVariables 
                     SmallMoleculeSection.OptionalInformations                                  = optionalInformations
                    }

                createSmallMoleculeSection
                    (identifiers.ToArray()) chemicalFormula (smiles.ToArray()) (inchiKey.ToArray()) (description.ToArray()) 
                    expMassToCharge calcMassToCharge charge (retentionTimes.ToArray()) taxid species database databaseVersion
                    reliability' uri' (spectraRefs.ToArray()) (searchEngines.ToArray()) (bestSearchEngineScores.ToArray()) 
                    searchEngineScoresMSRuns' modifications smallMoleculeAbundanceAssays' smallMoleculeAbundanceStudyVariables' 
                    smallMoleculeAbundanceStandardDeviationStudyVariables' smallMoleculeAbundanceStandardErrorStudyVariables' 
                    optionalInformations'

            ///Replaces reliability of existing object with new one.
            static member addReliability
                (reliability:int) (section:SmallMoleculeSection) =
                section.Reliability <- reliability
                section

            ///Replaces uri of existing object with new one.
            static member addURI
                (uri:string) (section:SmallMoleculeSection) =
                section.URI <- uri
                section

            ///Adds a searchEngineScoresMSRun to an existing object.
            static member addSearchEngineScoresMSRun (searchEngineScoresMSRun:float) (section:SmallMoleculeSection) =
                section.SearchEngineScoresMSRuns <- addToList section.SearchEngineScoresMSRuns searchEngineScoresMSRun
                section

            ///Adds a collection of searchEngineScoresMSRuns to an existing object.
            static member addSearchEngineScoresMSRuns (searchEngineScoresMSRuns:seq<float>) (section:SmallMoleculeSection) =
                section.SearchEngineScoresMSRuns <- addCollectionToList section.SearchEngineScoresMSRuns searchEngineScoresMSRuns
                section

            ///Adds a smallMoleculeAbundanceAssay to an existing object.
            static member addSmallMoleculeAbundanceAssay (smallMoleculeAbundanceAssay:float) (section:SmallMoleculeSection) =
                section.SearchEngineScoresMSRuns <- addToList section.SearchEngineScoresMSRuns smallMoleculeAbundanceAssay
                section

            ///Adds a collection of smallMoleculeAbundanceAssays to an existing object.
            static member addSmallMoleculeAbundanceAssays (smallMoleculeAbundanceAssays:seq<float>) (section:SmallMoleculeSection) =
                section.SmallMoleculeAbundanceAssays <- addCollectionToList section.SearchEngineScoresMSRuns smallMoleculeAbundanceAssays
                section

            ///Adds a smallMoleculeAbundanceStudyVariable to an existing object.
            static member addSmallMoleculeAbundanceStudyVariable (smallMoleculeAbundanceStudyVariable:float) (section:SmallMoleculeSection) =
                section.SmallMoleculeAbundanceStudyVariables <- addToList section.SmallMoleculeAbundanceStudyVariables smallMoleculeAbundanceStudyVariable
                section

            ///Adds a collection of smallMoleculeAbundanceStudyVariables to an existing object.
            static member addSmallMoleculeAbundanceStudyVariables (smallMoleculeAbundanceStudyVariables:seq<float>) (section:SmallMoleculeSection) =
                section.SmallMoleculeAbundanceStudyVariables <- addCollectionToList section.SmallMoleculeAbundanceStudyVariables smallMoleculeAbundanceStudyVariables
                section

            ///Adds a smallMoleculeAbundanceStandardDeviationStudyVariable to an existing object.
            static member addSmallMoleculeAbundanceStandardDeviationStudyVariable (smallMoleculeAbundanceStandardDeviationStudyVariable:float) (section:SmallMoleculeSection) =
                section.SmallMoleculeAbundanceStandardDeviationStudyVariables <- addToList section.SmallMoleculeAbundanceStandardDeviationStudyVariables smallMoleculeAbundanceStandardDeviationStudyVariable
                section

            ///Adds a collection of smallMoleculeAbundanceStandardDeviationStudyVariables to an existing object.
            static member addSmallMoleculeAbundanceStandardDeviationStudyVariables (smallMoleculeAbundanceStandardDeviationStudyVariables:seq<float>) (section:SmallMoleculeSection) =
                section.SmallMoleculeAbundanceStandardDeviationStudyVariables <- addCollectionToList section.SmallMoleculeAbundanceStandardDeviationStudyVariables smallMoleculeAbundanceStandardDeviationStudyVariables
                section

            ///Adds a smallMoleculeAbundanceStandardErrorStudyVariable to an existing object.
            static member addSmallMoleculeAbundanceStandardErrorStudyVariable (smallMoleculeAbundanceStandardErrorStudyVariable:float) (section:SmallMoleculeSection) =
                section.SmallMoleculeAbundanceStandardErrorStudyVariables <- addToList section.SmallMoleculeAbundanceStandardErrorStudyVariables smallMoleculeAbundanceStandardErrorStudyVariable
                section

            ///Adds a collection of smallMoleculeAbundanceStandardErrorStudyVariables to an existing object.
            static member addSmallMoleculeAbundanceStandardErrorStudyVariables (smallMoleculeAbundanceStandardErrorStudyVariables:seq<float>) (section:SmallMoleculeSection) =
                section.SmallMoleculeAbundanceStandardErrorStudyVariables <- addCollectionToList section.SmallMoleculeAbundanceStandardErrorStudyVariables smallMoleculeAbundanceStandardErrorStudyVariables
                section

            ///Adds a optionalInformation to an existing object.
            static member addOptionalInformation (optionalInformation:string) (section:SmallMoleculeSection) =
                section.OptionalInformations <- addToList section.OptionalInformations optionalInformation
                section

            ///Adds a collection of optionalInformations to an existing object.
            static member addOptionalInformations (optionalInformations:seq<string>) (section:SmallMoleculeSection) =
                section.OptionalInformations <- addCollectionToList section.OptionalInformations optionalInformations
                section

        type TSVFileHandler =
            ///Initializes a tsvFile-object with at least all necessary parameters for the file.
            static member init
                (             
                    metaDataSections          : MetaDataSection,
                    ?proteinSections          : seq<ProteinSection>,
                    ?peptideSections          : seq<PeptideSection>,
                    ?psmSections              : seq<PSMSection>,
                    ?smallMoleculeSections    : seq<SmallMoleculeSection>
                ) =

                let proteinSections'       = convertOptionToArray proteinSections
                let peptideSections'       = convertOptionToArray peptideSections
                let psmSections'           = convertOptionToArray psmSections
                let smallMoleculeSections' = convertOptionToArray smallMoleculeSections

                let createTSVFile metaDataSections proteinSections peptideSections psmSections smallMoleculeSections =
                    {
                     TSVFile.MetaDataSection       = metaDataSections
                     TSVFile.ProteinSections       = proteinSections
                     TSVFile.PeptideSections       = peptideSections
                     TSVFile.PSMSections           = psmSections
                     TSVFile.SmallMoleculeSections = smallMoleculeSections
                    }

                createTSVFile metaDataSections proteinSections' peptideSections' psmSections' smallMoleculeSections'

            ///Writes a tsv-file, based on the TSVFile-Object given.
            static member saveTSVFile (path:string) (tsvFileObject:TSVFile) =

                let metaData =
                    [tsvFileObject.MetaDataSection]
                    |> Seq.ofList 
                    |> Seq.toCSV "," true

                let proteinSections =
                    match tsvFileObject.ProteinSections with
                    |null -> Seq.ofArray [|""|]
                    |_ -> tsvFileObject.ProteinSections
                            |> Seq.ofArray
                            |> Seq.toCSV "," true

                let peptideSections =
                    match tsvFileObject.PeptideSections with
                    |null -> Seq.ofArray [|""|]
                    |_ -> tsvFileObject.PeptideSections
                            |> Seq.ofArray
                            |> Seq.toCSV "," true

                let psmSections =
                    match tsvFileObject.PSMSections with
                    |null -> Seq.ofArray [|""|]
                    |_ -> tsvFileObject.PSMSections
                            |> Seq.ofArray
                            |> Seq.toCSV ";" true

                let smallMoleculeSections =
                    match tsvFileObject.SmallMoleculeSections with
                    |null -> Seq.ofArray [|""|]
                    |_ -> tsvFileObject.SmallMoleculeSections
                            |> Seq.ofArray
                            |> Seq.toCSV "," true

                let tmp = Seq.concat [metaData; proteinSections; peptideSections; psmSections; smallMoleculeSections]
                tmp
                |> Seq.write path