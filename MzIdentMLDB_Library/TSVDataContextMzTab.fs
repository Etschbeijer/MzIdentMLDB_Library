namespace TSVMzTabDataBase

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


module DataModel =

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
    type MzTabType =
        | Identification = 0 
        | Quantification = 1

    ///The metadata section can provide additional information about the dataset(s) reported in the mzTab file. 
    type MetaDataSection =

            {
            [<FieldAttribute("mzTab-version")>]
            MzTabVersion                             : string
            [<FieldAttribute("mzTab-mode")>]
            MzTabMode                                : MzTabMode
            [<FieldAttribute("mzTab-type")>]
            MzType                                   : MzTabType
            [<FieldAttribute("mzTab-ID")>]
            mutable MzID                             : string
            [<FieldAttribute("title")>]
            mutable Title                            : string
            [<FieldAttribute("description")>]
            Description                              : string
            [<FieldAttribute("sample_processing[1-n]")>]
            mutable SampleProcessings                : array<(string*string*string*string)>
            [<FieldAttribute("instrument[1-n]-name")>]
            mutable InstrumentNames                  : array<string*string*string*string>
            [<FieldAttribute("instrument[1-n]-source")>]
            mutable InstrumentSources                : array<string*string*string*string>
            [<FieldAttribute("instrument[1-n]-analyzer[1-n]")>]
            mutable InstrumentAnalyzers              : array<string*string*string*string>
            [<FieldAttribute("instrument[1-n]-detector")>]
            mutable InstrumentDetectors              : array<string*string*string*string>
            [<FieldAttribute("software[1-n]")>]
            mutable Softwares                        : array<string*string*string*string>
            [<FieldAttribute("software[1-n]-setting[1-n]")>]
            mutable SoftwaresSettings                : array<string*string*string*string>
            [<FieldAttribute("protein_search_engine_score[1-n]")>]
            ProteinSearchEngineScores                : array<string*string*string*string>
            [<FieldAttribute("peptide_search_engine_score[1-n]")>]
            PeptideSearchEngineScores                : array<string*string*string*string>
            [<FieldAttribute("psm_search_engine_score[1-n]")>]
            PSMSearchEngineScores                    : array<string*string*string*string>
            [<FieldAttribute("smallmolecule_search_engine_score[1-n]")>]
            SmallMoleculeSearchEngineScores          : array<string*string*string*string>
            [<FieldAttribute("false_discovery_rate")>]
            mutable FalseDiscoveryRates              : array<string*string*string*string>
            [<FieldAttribute("publication[1-n]")>]
            mutable Publications                     : array<string>
            [<FieldAttribute("contact[1-n]-name")>]
            mutable ContactNames                     : array<string>
            [<FieldAttribute("contact[1-n]-affiliation")>]
            mutable ContactAffiliations              : array<string>
            [<FieldAttribute("contact[1-n]-email")>]
            mutable ContactEMails                    : array<string>
            [<FieldAttribute("uri[1-n]")>]
            mutable URIs                             : array<string>
            [<FieldAttribute("fixed_mod[1-n]")>]
            FixedMods                                : array<string*string*string*string>
            [<FieldAttribute("fixed_mod[1-n]-site")>]
            mutable FixedModSites                    : array<string>
            [<FieldAttribute("fixed_mod[1-n]-position")>]
            mutable FixedModPositions                : array<string>
            [<FieldAttribute("variable_mod[1-n]")>]
            VariableMods                             : array<string*string*string*string>
            [<FieldAttribute("variable_mod[1-n]-site")>]
            mutable VariableModSites                 : array<string>
            [<FieldAttribute("variable_mod[1-n]-position")>]
            mutable VariableModPositions             : array<string>
            [<FieldAttribute("quantification_method")>]
            mutable QuantificationMethod             : string*string*string*string
            [<FieldAttribute("protein-quantification_unit")>]
            mutable ProteinQuantificationUnit        : string*string*string*string
            [<FieldAttribute("peptide-quantification_unit")>]
            mutable PeptideQuantificationUnit        : string*string*string*string
            [<FieldAttribute("small_molecule-quantification_unit")>]
            mutable SmallMoleculeQuantificationUnit  : string*string*string*string
            [<FieldAttribute("ms_run[1-n]-format")>]
            mutable MSRunsFormat                     : string*string*string*string
            [<FieldAttribute("ms_run[1-n]-location")>]
            MSRunLocation                            : string
            [<FieldAttribute("ms_run[1-n]-id_format")>]
            mutable MSRunsIDFormat                   : string*string*string*string
            [<FieldAttribute("ms_run[1-n]-fragmentation_method")>]
            mutable MSRunsFragmentationMethods       : array<string*string*string*string>
            [<FieldAttribute("ms_run[1-n]-hash")>]
            mutable MSRunsHash                       : string
            [<FieldAttribute("ms_run[1-n]-hash_method")>]
            mutable MSRunsHashMethod                 : string*string*string*string
            [<FieldAttribute("custom[1-n]")>]
            mutable Customs                          : array<string*string*string*string>
            [<FieldAttribute("sample[1-n]-species[1-n]")>]
            mutable SamplesSpecies                   : array<string*string*string*string>
            [<FieldAttribute("sample[1-n]-tissue[1-n]")>]
            mutable SampleTissues                    : array<string*string*string*string>
            [<FieldAttribute("sample[1-n]-cell_type[1-n]")>]
            mutable SamplesCellTypes                 : array<string*string*string*string>
            [<FieldAttribute("sample[1-n]-disease[1-n]")>]
            mutable SamplesDiseases                  : array<string*string*string*string>
            [<FieldAttribute("sample[1-n]-description")>]
            mutable SampleDescriptions               : array<string>
            [<FieldAttribute("sample[1-n]-custom[1-n]")>]
            mutable SamplesCustoms                   : array<string>
            [<FieldAttribute("assay[1-n]-quantification_reagent")>]
            mutable AssaysQuantificationReagent      : string*string*string*string
            [<FieldAttribute("assay[1-n]-quantification_mod[1-n]")>]
            mutable AssaysQuantificationMods         : array<string*string*string*string>
            [<FieldAttribute("assay[1-n]-quantification_mod[1-n]-site")>]
            mutable AssaysQuantificationModsSite     : string
            [<FieldAttribute("assay[1-n]-quantification_mod[1-n]-position")>]
            mutable AssaysQuantificationModsPosition : string
            [<FieldAttribute("assay[1-n]-sample_ref ")>]
            mutable AssaysSampleRef                  : string
            [<FieldAttribute("assay[1-n]-ms_run_ref")>]
            mutable AssaysMSRunRef                   : string
            [<FieldAttribute("study_variable[1-n]-assay_refs")>]
            mutable StudyVariablesAssayRefs          : array<string>
            [<FieldAttribute("study_variable[1-n]-sample_refs")>]
            mutable StudyVariablesSampleRefs         : array<string>
            [<FieldAttribute("study_variable[1-n]-description")>]
            mutable StudyVariablesDescription        : array<string>
            [<FieldAttribute("cv[1-n]-label")>]
            mutable CvsLabel                         : string
            [<FieldAttribute("cv[1-n]-full_name")>]
            mutable CvsFullName                      : string
            [<FieldAttribute("cv[1-n]-version")>]
            mutable CvsVersion                       : string
            [<FieldAttribute("cv[1-n]-url")>]
            mutable CvsURI                           : string
            [<FieldAttribute("colunit-protein")>]
            mutable ColUnitProtein                   : string*string*string*string
            [<FieldAttribute("colunit-peptide")>]
            mutable ColUnitPeptide                   : string*string*string*string
            [<FieldAttribute("colunit-psm")>]
            mutable ColUnitPSM                       : string*string*string*string
            [<FieldAttribute("colunit-small_molecule")>]
            mutable ColUnitSmallMolecule             : string*string*string*string
            }

    //The protein section can provide additional information about the reported proteins in the mzTab file.
    type [<AllowNullLiteral>]
         ProteinSection (
                         accession:string, description:string, taxid:int, species:string, database:string, databaseVersion:string, 
                         searchEngines:array<string*string*string*string>, bestSearchEngineScore:float, 
                         searchEngineScoresMSRuns:float, reliability:int, numPSMsMSRuns:int, numPeptidesDistinctMSRuns:int,
                         numPeptidesUniqueMSRuns:int, ambiguityMembers:array<string>, modifications:array<string>, uri:string, goTerms:array<string>, 
                         proteinCoverage:float, proteinAbundanceAssays:array<float>, proteinAbundanceStudyVariables:array<float>, 
                         proteinAbundanceStandardDeviationStudyVariables:array<float>, proteinAbundanceStandardErrorStudyVariables:array<float>, 
                         optionalInformation:array<string>
                        ) =

                        [<FieldAttribute("accession")>]
                        let mutable accession'                                       = accession
                        [<FieldAttribute("description")>]
                        let mutable description'                                     = description
                        [<FieldAttribute("taxid")>]
                        let mutable taxid'                                           = taxid
                        [<FieldAttribute("species")>]
                        let mutable species'                                         = species
                        [<FieldAttribute("database")>]
                        let mutable database'                                        = database
                        [<FieldAttribute("database_version")>]
                        let mutable databaseVersion'                                 = databaseVersion
                        [<FieldAttribute("search_engine")>]
                        let mutable searchEngines'                                   = searchEngines
                        [<FieldAttribute("best_search_engine_score[1-n]")>]
                        let mutable bestSearchEngineScores'                          = bestSearchEngineScore
                        [<FieldAttribute("search_engine_score[1-n]_ms_run[1-n]")>]
                        let mutable searchEngineScoresMSRuns'                        = searchEngineScoresMSRuns
                        [<FieldAttribute("reliability")>]
                        let mutable reliability'                                     = reliability
                        [<FieldAttribute("num_psms_ms_run[1-n]")>]
                        let mutable numPSMsMSRuns'                                   = numPSMsMSRuns
                        [<FieldAttribute("num_peptides_distinct_ms_run[1-n]")>]
                        let mutable numPeptidesDistinctMSRuns'                       = numPeptidesDistinctMSRuns
                        [<FieldAttribute("num_peptides_unique_ms_run[1-n]")>]
                        let mutable numPeptidesUniqueMSRuns'                         = numPeptidesUniqueMSRuns
                        [<FieldAttribute("ambiguity_members")>]
                        let mutable ambiguityMembers'                                = ambiguityMembers
                        [<FieldAttribute("modifications")>]
                        let mutable modifications'                                   = modifications
                        [<FieldAttribute("uri")>]
                        let mutable uri'                                             = uri
                        [<FieldAttribute("go_terms")>]
                        let mutable goTerms'                                         = goTerms
                        [<FieldAttribute("protein_coverage")>]
                        let mutable proteinCoverage'                                 = proteinCoverage
                        [<FieldAttribute("protein_abundance_assay[1-n]")>]
                        let mutable proteinAbundanceAssays'                          = proteinAbundanceAssays
                        [<FieldAttribute("protein_abundance_study_variable[1-n]")>]
                        let mutable proteinAbundanceStudyVariables'                  = proteinAbundanceStudyVariables
                        [<FieldAttribute("protein_abundance_stdev_study_variable[1-n]")>]
                        let mutable proteinAbundanceStandardDeviationStudyVariables' = proteinAbundanceStandardDeviationStudyVariables
                        [<FieldAttribute("protein_abundance_std_error_study_variable[1-n]")>]
                        let mutable proteinAbundanceStandardErrorStudyVariables'      = proteinAbundanceStandardErrorStudyVariables
                        [<FieldAttribute("opt_{identifier}_*")>]
                        let mutable optionalInformation'                             = optionalInformation

                        member this.Accession with get() = accession' and set(value) = accession' <- value
                        member this.Description with get() = description' and set(value) = description' <- value
                        member this.Taxid with get() = taxid' and set(value) = taxid' <- value
                        member this.Species with get() = species' and set(value) = species' <- value
                        member this.Database with get() = database' and set(value) = database' <- value
                        member this.DatabaseVersion with get() = databaseVersion' and set(value) = databaseVersion' <- value
                        member this.SearchEngines with get() = searchEngines' and set(value) = searchEngines' <- value
                        member this.BestSearchEngineScore with get() = bestSearchEngineScores' and set(value) = bestSearchEngineScores' <- value
                        member this.SearchEngineScoresMSRuns with get() = searchEngineScoresMSRuns' and set(value) = searchEngineScoresMSRuns' <- value
                        member this.Reliability with get() = reliability' and set(value) = reliability' <- value
                        member this.NumPSMsMSRuns with get() = numPSMsMSRuns' and set(value) = numPSMsMSRuns' <- value
                        member this.NumPeptidesDistinctMSRuns with get() = numPeptidesDistinctMSRuns' and set(value) = numPeptidesDistinctMSRuns' <- value
                        member this.NumPeptidesUniqueMSRuns with get() = numPeptidesUniqueMSRuns' and set(value) = numPeptidesUniqueMSRuns' <- value
                        member this.AmbiguityMembers with get() = ambiguityMembers' and set(value) = ambiguityMembers' <- value
                        member this.Modifications with get() = modifications' and set(value) = modifications' <- value
                        member this.URI with get() = uri' and set(value) = uri' <- value
                        member this.GoTerms with get() = goTerms' and set(value) = goTerms' <- value
                        member this.ProteinCoverage with get() = proteinCoverage' and set(value) = proteinCoverage' <- value
                        member this.ProteinAbundanceAssays with get() = proteinAbundanceAssays' and set(value) = proteinAbundanceAssays' <- value
                        member this.ProteinAbundanceStudyVariables with get() = proteinAbundanceStudyVariables' and set(value) = proteinAbundanceStudyVariables' <- value
                        member this.ProteinAbundanceStandardDeviationStudyVariables with get() = proteinAbundanceStandardDeviationStudyVariables' and set(value) = proteinAbundanceStandardDeviationStudyVariables' <- value
                        member this.ProteinAbundanceStandardErrorStudyVariables with get() = proteinAbundanceStandardErrorStudyVariables' and set(value) = proteinAbundanceStandardErrorStudyVariables' <- value
                        member this.OptionalInformations with get() = optionalInformation' and set(value) = optionalInformation' <- value

        //The protein section can provide additional information about the reported proteins in the mzTab file.
    type [<AllowNullLiteral>]
         PeptideSection (
                         peptideSequence:string, accession:string, unique:bool, database:string, databaseVersion:string, searchEngines:array<string*string*string*string>,
                         bestSearchEngineScore:float, searchEngineScoresMSRuns:float, reliability:int, modifications:array<string>, retentionTime:array<float>, 
                         retentionTimeWindow:array<float>, charge:int, massToCharge:float, uri:string, spectraRefs:array<string>, peptideAbundanceAssay:array<float>, 
                         peptideAbundanceStudyVariable:array<float>, peptideAbundanceStandardDeviationStudyVariable:array<float>, peptideAbundanceStandardErrorStudyVariables:array<float>, 
                         optionalInformation:array<string>
                        ) =

                        [<FieldAttribute("sequence")>]
                        let mutable peptideSequence'                                 = peptideSequence
                        [<FieldAttribute("accession")>]
                        let mutable accession'                                       = accession
                        [<FieldAttribute("unique")>]
                        let mutable unique'                                          = unique
                        [<FieldAttribute("database")>]
                        let mutable database'                                        = database
                        [<FieldAttribute("database_version")>]
                        let mutable databaseVersion'                                 = databaseVersion
                        [<FieldAttribute("search_engine")>]
                        let mutable searchEngines'                                   = searchEngines
                        [<FieldAttribute("best_search_engine_score[1-n]")>]
                        let mutable bestSearchEngineScores'                          = bestSearchEngineScore
                        [<FieldAttribute("search_engine_score[1-n]_ms_run[1-n]")>]
                        let mutable searchEngineScoresMSRuns'                        = searchEngineScoresMSRuns
                        [<FieldAttribute("reliability")>]
                        let mutable reliability'                                     = reliability
                        [<FieldAttribute("modifications")>]
                        let mutable modifications'                                   = modifications
                        [<FieldAttribute("retention_time")>]
                        let mutable retentionTime'                                   = retentionTime
                        [<FieldAttribute("retention_time_window")>]
                        let mutable retentionTimeWindow'                             = retentionTimeWindow
                        [<FieldAttribute("charge")>]
                        let mutable charge'                                          = charge
                        [<FieldAttribute("mass_to_charge")>]
                        let mutable massToCharge'                                    = massToCharge
                        [<FieldAttribute("uri")>]
                        let mutable uri'                                             = uri
                        [<FieldAttribute("spectra_ref")>]
                        let mutable spectraRefs'                                     = spectraRefs
                        [<FieldAttribute("peptide_abundance_assay[1-n]")>]
                        let mutable peptideAbundanceAssay'                           = peptideAbundanceAssay
                        [<FieldAttribute("peptide_abundance_study_variable[1-n]")>]
                        let mutable peptideAbundanceStudyVariable'                   = peptideAbundanceStudyVariable
                        [<FieldAttribute("peptide_abundance_stdev_study_variable[1-n]")>]
                        let mutable peptideAbundanceStandardDeviationStudyVariable'  = peptideAbundanceStandardDeviationStudyVariable
                        [<FieldAttribute("peptide_abundance_std_error_study_variable[1-n]")>]
                        let mutable peptideAbundanceStandardErrorStudyVariables'     = peptideAbundanceStandardErrorStudyVariables
                        [<FieldAttribute("opt_{identifier}_*")>]
                        let mutable optionalInformation'                             = optionalInformation

                        member this.PeptideSequence with get() = peptideSequence' and set(value) = peptideSequence' <- value
                        member this.Accession with get() = accession' and set(value) = accession' <- value
                        member this.Unique with get() = unique' and set(value) = unique' <- value
                        member this.Database with get() = database' and set(value) = database' <- value
                        member this.DatabaseVersion with get() = databaseVersion' and set(value) = databaseVersion' <- value
                        member this.SearchEngines with get() = searchEngines' and set(value) = searchEngines' <- value
                        member this.BestSearchEngineScore with get() = bestSearchEngineScores' and set(value) = bestSearchEngineScores' <- value
                        member this.SearchEngineScoresMSRuns with get() = searchEngineScoresMSRuns' and set(value) = searchEngineScoresMSRuns' <- value
                        member this.Reliability with get() = reliability' and set(value) = reliability' <- value
                        member this.Modifications with get() = modifications' and set(value) = modifications' <- value
                        member this.RetentionTime with get() = retentionTime' and set(value) = retentionTime' <- value
                        member this.RetentionTimeWindow with get() = retentionTimeWindow' and set(value) = retentionTimeWindow' <- value
                        member this.charge with get() = charge' and set(value) = charge' <- value
                        member this.MassToCharge with get() = massToCharge' and set(value) = massToCharge' <- value
                        member this.URI with get() = uri' and set(value) = uri' <- value
                        member this.SpectraRefs with get() = spectraRefs' and set(value) = spectraRefs' <- value
                        member this.PeptideAbundanceAssay with get() = peptideAbundanceAssay' and set(value) = peptideAbundanceAssay' <- value
                        member this.PeptideAbundanceStudyVariable with get() = peptideAbundanceStudyVariable' and set(value) = peptideAbundanceStudyVariable' <- value
                        member this.PeptideAbundanceStandardDeviationStudyVariable with get() = peptideAbundanceStandardDeviationStudyVariable' and set(value) = peptideAbundanceStandardDeviationStudyVariable' <- value
                        member this.PeptideAbundanceStandardErrorStudyVariables with get() = peptideAbundanceStandardErrorStudyVariables' and set(value) = peptideAbundanceStandardErrorStudyVariables' <- value
                        member this.OptionalInformations with get() = optionalInformation' and set(value) = optionalInformation' <- value
