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
            [<FieldAttribute("mzTab-version")>]
            mzTabVersion                     : string
            [<FieldAttribute("mzTab-mode")>]
            mzTabMode                        : MzTabMode
            [<FieldAttribute("mzTab-type")>]
            mzType                           : MzType
            [<FieldAttribute("mzTab-ID")>]
            mzID                             : string
            [<FieldAttribute("title")>]
            title                            : string
            [<FieldAttribute("description")>]
            description                      : string
            [<FieldAttribute("sample_processing[1-n]")>]
            sampleProcessings                : array<(string*string*string*string)>
            [<FieldAttribute("instrument[1-n]-name")>]
            instrumentNames                  : array<string*string*string*string>
            [<FieldAttribute("instrument[1-n]-source")>]
            instrumentSources                : array<string*string*string*string>
            [<FieldAttribute("instrument[1-n]-analyzer[1-n]")>]
            instrumentAnalyzers              : array<string*string*string*string>
            [<FieldAttribute("instrument[1-n]-detector")>]
            instrumentDetectors              : array<string*string*string*string>
            [<FieldAttribute("software[1-n]")>]
            softwares                        : array<string*string*string*string>
            [<FieldAttribute("software[1-n]-setting[1-n]")>]
            softwaresSettings                : array<string*string*string*string>
            [<FieldAttribute("protein_search_engine_score[1-n]")>]
            proteinSearchEngineScores        : array<string*string*string*string>
            [<FieldAttribute("peptide_search_engine_score[1-n]")>]
            peptideSearchEngineScores        : array<string*string*string*string>
            [<FieldAttribute("psm_search_engine_score[1-n]")>]
            psmSearchEngineScores            : array<string*string*string*string>
            [<FieldAttribute("smallmolecule_search_engine_score[1-n]")>]
            smallMoleculeSearchEngineScores  : array<string*string*string*string>
            [<FieldAttribute("false_discovery_rate")>]
            falseDiscoveryRates              : array<string*string*string*string>
            [<FieldAttribute("publication[1-n]")>]
            publications                     : array<string>
            [<FieldAttribute("contact[1-n]-name")>]
            contactNames                     : array<string>
            [<FieldAttribute("contact[1-n]-affiliation")>]
            contactAffiliations              : array<string>
            [<FieldAttribute("contact[1-n]-email")>]
            contactEMails                    : array<string>
            [<FieldAttribute("uri[1-n]")>]
            uris                             : array<string>
            [<FieldAttribute("fixed_mod[1-n]")>]
            fixedMods                        : array<string*string*string*string>
            [<FieldAttribute("fixed_mod[1-n]-site")>]
            fixedModSites                    : array<string*string*string*string>
            [<FieldAttribute("fixed_mod[1-n]-position")>]
            fixedModPositions                : array<string*string*string*string>
            [<FieldAttribute("variable_mod[1-n]")>]
            variableMods                     : array<string*string*string*string>
            [<FieldAttribute("variable_mod[1-n]-site")>]
            variableModSites                 : array<string*string*string*string>
            [<FieldAttribute("variable_mod[1-n]-position")>]
            variableModPositions             : array<string*string*string*string>
            [<FieldAttribute("quantification_method")>]
            quantificationMethod             : string*string*string*string
            [<FieldAttribute("protein-quantification_unit")>]
            proteinQuantificationUnit        : string*string*string*string
            [<FieldAttribute("peptide-quantification_unit")>]
            peptideQuantificationUnit        : string*string*string*string
            [<FieldAttribute("small_molecule-quantification_unit")>]
            smallMoleculeQuantificationUnit  : string*string*string*string
            [<FieldAttribute("ms_run[1-n]-format")>]
            msRunsFormat                     : string*string*string*string
            [<FieldAttribute("ms_run[1-n]-location")>]
            msRunLocation                    : string
            [<FieldAttribute("ms_run[1-n]-id_format")>]
            msRunsIDForamts                  : string*string*string*string
            [<FieldAttribute("ms_run[1-n]-fragmentation_method")>]
            msRunsFragmentationMethod        : array<string*string*string*string>
            [<FieldAttribute("ms_run[1-n]-hash")>]
            msRunsHash                       : string
            [<FieldAttribute("ms_run[1-n]-hash_method")>]
            msRunsHashMethods                : string
            [<FieldAttribute("custom[1-n]")>]
            customs                          : array<string*string*string*string>
            [<FieldAttribute("sample[1-n]-species[1-n]")>]
            samplesSpecies                   : array<string*string*string*string>
            [<FieldAttribute("sample[1-n]-tissue[1-n]")>]
            sampleTissues                    : array<string*string*string*string>
            [<FieldAttribute("sample[1-n]-cell_type[1-n]")>]
            samplesCellTypes                 : array<string*string*string*string>
            [<FieldAttribute("sample[1-n]-disease[1-n]")>]
            samplesDiseases                  : array<string*string*string*string>
            [<FieldAttribute("sample[1-n]-description")>]
            sampleDescriptions               : string
            [<FieldAttribute("sample[1-n]-custom[1-n]")>]
            samplesCustoms                   : array<string>
            [<FieldAttribute("assay[1-n]-quantification_reagent")>]
            assaysQuantificationReagent      : array<string*string*string*string>
            [<FieldAttribute("assay[1-n]-quantification_mod[1-n]")>]
            assaysQuantificationMods         : array<string*string*string*string>
            [<FieldAttribute("assay[1-n]-quantification_mod[1-n]-site")>]
            assaysQuantificationModsSite     : array<string>
            [<FieldAttribute("assay[1-n]-quantification_mod[1-n]-position")>]
            assaysQuantificationModsPosition : array<string>
            [<FieldAttribute("assay[1-n]-sample_ref ")>]
            assaysSampleRef                  : array<string>
            [<FieldAttribute("assay[1-n]-ms_run_ref")>]
            assaysMSRunRef                   : array<string>
            [<FieldAttribute("study_variable[1-n]-assay_refs")>]
            studyVariablesAssayRefs          : array<string>
            [<FieldAttribute("study_variable[1-n]-sample_refs")>]
            studyVariablesSampleRefs         : array<string>
            [<FieldAttribute("study_variable[1-n]-description")>]
            studyVariablesDescription        : array<string>
            [<FieldAttribute("cv[1-n]-label")>]
            cvsLabel                         : array<string>
            [<FieldAttribute("cv[1-n]-full_name")>]
            cvsVersion                       : array<string>
            [<FieldAttribute("cv[1-n]-version")>]
            cvsURI                           : array<string>
            [<FieldAttribute("cv[1-n]-url")>]
            colUnitProtein                   : array<string>
            [<FieldAttribute("colunit-protein")>]
            colUnitPeptide                   : array<string>
            [<FieldAttribute("colunit-peptide")>]
            colUnitPSM                       : array<string>
            [<FieldAttribute("colunit-psm")>]
            colUnitSmallMolecule             : array<string>
            [<FieldAttribute("colunit-small_molecule")>]
            rowVersion                       : DateTime
            }

