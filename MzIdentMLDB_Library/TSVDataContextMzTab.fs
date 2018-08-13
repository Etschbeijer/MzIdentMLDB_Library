namespace MzTabDataBase

open System
open System.ComponentModel.DataAnnotations.Schema
open Microsoft.EntityFrameworkCore
open System.Collections.Generic
open MzIdentMLDataBase.DataModel
open MzIdentMLDataBase.InsertStatements
open MzQuantMLDataBase.DataModel
open MzQuantMLDataBase.InsertStatements
open Microsoft.EntityFrameworkCore.ValueGeneration.Internal

open BioFSharp
open BioFSharp.IO
open BioFSharp.BioSeq
open FSharp.Care
open BioFSharp.BioArray
open BioFSharp.BioList
open BioFSharp.AminoProperties
open FSharp.Care.IO.SchemaReader
open FSharp.Care.IO.SchemaReader.Attribute


module TabSeperatedValueScheme =

    ///The results included in an mzTab file can be reported in 2 ways: 
    ///‘Complete’ (when results for each assay/replicate are included) and 
    ///‘Summary’, when only the most representative results are reported. 
    ///1 stand for the value summary and 0 for complete.
    type MzTabMode =
        | Summary  = 0 
        | Complete = 1

    ///The results included in an mzTab file MUST be flagged as ‘Identification’ or ‘Quantification’ 
    ///- the latter encompassing approaches that are quantification only or quantification and identification.
    ///1 stand for the value quantification and 0 for identification.
    type MzType =
        | Identification = 0 
        | Quantification = 1

    type MetaData =

            {
            [<FieldAttribute(1)>]
            mzTabVersion                     : string
            [<FieldAttribute(2)>]
            mzTabMode                        : MzTabMode
            [<FieldAttribute(3)>]
            mzType                           : MzType
            [<FieldAttribute(4)>]
            mzID                             : string
            [<FieldAttribute(5)>]
            title                            : string
            [<FieldAttribute(6)>]
            description                      : string
            [<FieldAttribute(7)>]
            sampleProcessings                : array<array<string>>
            [<FieldAttribute(8)>]
            instrumentNames                  : array<string>
            [<FieldAttribute(9)>]
            instrumentSources                : array<string>
            [<FieldAttribute(10)>]
            instrumentAnalyzers              : array<string>
            [<FieldAttribute(11)>]
            instrumentDetectors              : array<string>
            [<FieldAttribute(12)>]
            softwares                        : array<string>
            [<FieldAttribute(13)>]
            softwaresSettings                : array<string>
            [<FieldAttribute(14)>]
            proteinSearchEngineScores        : array<string>
            [<FieldAttribute(15)>]
            peptideSearchEngineScores        : array<string>
            [<FieldAttribute(16)>]
            psmSearchEngineScores            : array<string>
            [<FieldAttribute(17)>]
            smallMoleculeSearchEngineScores  : array<string>
            [<FieldAttribute(18)>]
            falseDiscoveryRates              : array<string>
            [<FieldAttribute(19)>]
            publications                     : array<string>
            [<FieldAttribute(20)>]
            contactNames                     : array<string>
            [<FieldAttribute(21)>]
            contactAffiliations              : array<string>
            [<FieldAttribute(22)>]
            contactEMails                    : array<string>
            [<FieldAttribute(23)>]
            uris                             : array<string>
            [<FieldAttribute(24)>]
            fixedMods                        : array<string>
            [<FieldAttribute(25)>]
            fixedModSites                    : array<string>
            [<FieldAttribute(26)>]
            fixedModPositions                : array<string>
            [<FieldAttribute(27)>]
            variableMods                     : array<string>
            [<FieldAttribute(28)>]
            variableModSites                 : array<string>
            [<FieldAttribute(29)>]
            variableModPositions             : array<string>
            [<FieldAttribute(30)>]
            quantificationMethod             : array<string>
            [<FieldAttribute(31)>]
            proteinQuantificationUnit        : array<string>
            [<FieldAttribute(32)>]
            peptideQuantificationUnit        : array<string>
            [<FieldAttribute(33)>]
            smallMoleculeQuantificationUnit  : array<string>
            [<FieldAttribute(34)>]
            msRunsFormats                    : array<string>
            [<FieldAttribute(35)>]
            msRunLocation                    : array<string>
            [<FieldAttribute(36)>]
            msRunsIDForamts                  : array<string>
            [<FieldAttribute(37)>]
            msRunsFragmentationMethod        : array<string>
            [<FieldAttribute(38)>]
            msRunsHash                       : array<string>
            [<FieldAttribute(39)>]
            msRunsHashMethods                : array<string>
            [<FieldAttribute(40)>]
            customs                          : array<string>
            [<FieldAttribute(41)>]
            samplesSpecies                   : array<string>
            [<FieldAttribute(42)>]
            sampleTissues                    : array<string>
            [<FieldAttribute(43)>]
            samplesCellTypes                 : array<string>
            [<FieldAttribute(44)>]
            samplesDiseases                  : array<string>
            [<FieldAttribute(45)>]
            sampleDescriptions               : array<string>
            [<FieldAttribute(46)>]
            samplesCustoms                   : array<string>
            [<FieldAttribute(47)>]
            assaysQuantificationReagent      : array<string>
            [<FieldAttribute(48)>]
            assaysQuantificationMods         : array<string>
            [<FieldAttribute(49)>]
            assaysQuantificationModsSite     : array<string>
            [<FieldAttribute(50)>]
            assaysQuantificationModsPosition : array<string>
            [<FieldAttribute(51)>]
            assaysSampleRef                  : array<string>
            [<FieldAttribute(52)>]
            assaysMSRunRef                   : array<string>
            [<FieldAttribute(53)>]
            studyVariablesAssayRefs          : array<string>
            [<FieldAttribute(54)>]
            studyVariablesSampleRefs         : array<string>
            [<FieldAttribute(55)>]
            studyVariablesDescription        : array<string>
            [<FieldAttribute(56)>]
            cvsLabel                         : array<string>
            [<FieldAttribute(57)>]
            cvsVersion                       : array<string>
            [<FieldAttribute(58)>]
            cvsURI                           : array<string>
            [<FieldAttribute(59)>]
            colUnitProtein                   : array<string>
            [<FieldAttribute(60)>]
            colUnitPeptide                   : array<string>
            [<FieldAttribute(61)>]
            colUnitPSM                       : array<string>
            [<FieldAttribute(62)>]
            colUnitSmallMolecule             : array<string>
            [<FieldAttribute(63)>]
            rowVersion                       : DateTime
            }

