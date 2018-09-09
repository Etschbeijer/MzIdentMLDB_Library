namespace TSVMzTabDataBase

//open System
//open System.ComponentModel.DataAnnotations.Schema
//open Microsoft.EntityFrameworkCore
//open System.Collections.Generic
//open MzIdentMLDataBase.DataModel
//open MzIdentMLDataBase.InsertStatements
//open MzQuantMLDataBase.DataModel
//open MzQuantMLDataBase.InsertStatements
//open Microsoft.EntityFrameworkCore.ValueGeneration.Internal

//open BioFSharp
//open BioFSharp.IO
//open BioFSharp.BioSeq
//open FSharp.Care
//open BioFSharp.BioArray
//open BioFSharp.BioList
//open BioFSharp.AminoProperties
//open FSharp.Care.IO.SchemaReader
//open FSharp.Care.IO.SchemaReader.Attribute


//module DataModel =

//    ///The results included in an mzTab file can be reported in 2 ways: 
//    ///‘Complete’ (when results for each assay/replicate are included) and 
//    ///‘Summary’, when only the most representative results are reported. 
//    ///1 stand for the value summary and 0 for complete.
//    type MzTabMode =
//        | Summary  = 0 
//        | Complete = 1

//    ///The results included in an mzTab file MUST be flagged as ‘Identification’ or ‘Quantification’ 
//    ///- the latter encompassing approaches that are quantification only or quantification and identification.
//    ///1 stand for the value quantification and 0 for identification.
//    type MzTabType =
//        | Identification = 0 
//        | Quantification = 1

//    ///The metadata section can provide additional information about the dataset(s) reported in the mzTab file. 
//    type MetaDataSection =
//           {
//            [<FieldAttribute("mzTab-version")>]
//            MzTabVersion                             : string
//            [<FieldAttribute("mzTab-mode")>]
//            MzTabMode                                : MzTabMode
//            [<FieldAttribute("mzTab-type")>]
//            MzType                                   : MzTabType
//            [<FieldAttribute("mzTab-ID")>]
//            mutable MzID                             : string
//            [<FieldAttribute("title")>]
//            mutable Title                            : string
//            [<FieldAttribute("description")>]
//            Description                              : string
//            [<FieldAttribute("sample_processing[1-n]")>]
//            mutable SampleProcessings                : array<(string)>
//            [<FieldAttribute("instrument[1-n]-name")>]
//            mutable InstrumentNames                  : array<string>
//            [<FieldAttribute("instrument[1-n]-source")>]
//            mutable InstrumentSources                : array<string>
//            [<FieldAttribute("instrument[1-n]-analyzer[1-n]")>]
//            mutable InstrumentAnalyzers              : array<string>
//            [<FieldAttribute("instrument[1-n]-detector")>]
//            mutable InstrumentDetectors              : array<string>
//            [<FieldAttribute("software[1-n]")>]
//            mutable Softwares                        : array<string>
//            [<FieldAttribute("software[1-n]-setting[1-n]")>]
//            mutable SoftwaresSettings                : array<string>
//            [<FieldAttribute("protein_search_engine_score[1-n]")>]
//            ProteinSearchEngineScores                : array<string>
//            [<FieldAttribute("peptide_search_engine_score[1-n]")>]
//            PeptideSearchEngineScores                : array<string>
//            [<FieldAttribute("psm_search_engine_score[1-n]")>]
//            PSMSearchEngineScores                    : array<string>
//            [<FieldAttribute("smallmolecule_search_engine_score[1-n]")>]
//            SmallMoleculeSearchEngineScores          : array<string>
//            [<FieldAttribute("false_discovery_rate")>]
//            mutable FalseDiscoveryRates              : array<string>
//            [<FieldAttribute("publication[1-n]")>]
//            mutable Publications                     : array<string>
//            [<FieldAttribute("contact[1-n]-name")>]
//            mutable ContactNames                     : array<string>
//            [<FieldAttribute("contact[1-n]-affiliation")>]
//            mutable ContactAffiliations              : array<string>
//            [<FieldAttribute("contact[1-n]-email")>]
//            mutable ContactEMails                    : array<string>
//            [<FieldAttribute("uri[1-n]")>]
//            mutable URI                              : string
//            [<FieldAttribute("fixed_mod[1-n]")>]
//            FixedMods                                : array<string>
//            [<FieldAttribute("fixed_mod[1-n]-site")>]
//            mutable FixedModSites                    : array<string>
//            [<FieldAttribute("fixed_mod[1-n]-position")>]
//            mutable FixedModPositions                : array<string>
//            [<FieldAttribute("variable_mod[1-n]")>]
//            VariableMods                             : array<string>
//            [<FieldAttribute("variable_mod[1-n]-site")>]
//            mutable VariableModSites                 : array<string>
//            [<FieldAttribute("variable_mod[1-n]-position")>]
//            mutable VariableModPositions             : array<string>
//            [<FieldAttribute("quantification_method")>]
//            mutable QuantificationMethod             : string
//            [<FieldAttribute("protein-quantification_unit")>]
//            mutable ProteinQuantificationUnit        : string
//            [<FieldAttribute("peptide-quantification_unit")>]
//            mutable PeptideQuantificationUnit        : string
//            [<FieldAttribute("small_molecule-quantification_unit")>]
//            mutable SmallMoleculeQuantificationUnit  : string
//            [<FieldAttribute("ms_run[1-n]-format")>]
//            mutable MSRunsFormat                     : string
//            [<FieldAttribute("ms_run[1-n]-location")>]
//            MSRunLocation                            : string
//            [<FieldAttribute("ms_run[1-n]-id_format")>]
//            mutable MSRunsIDFormat                   : string
//            [<FieldAttribute("ms_run[1-n]-fragmentation_method")>]
//            mutable MSRunsFragmentationMethods       : array<string>
//            [<FieldAttribute("ms_run[1-n]-hash")>]
//            mutable MSRunsHash                       : string
//            [<FieldAttribute("ms_run[1-n]-hash_method")>]
//            mutable MSRunsHashMethod                 : string
//            [<FieldAttribute("custom[1-n]")>]
//            mutable Customs                          : array<string>
//            [<FieldAttribute("sample[1-n]-species[1-n]")>]
//            mutable SamplesSpecies                   : array<string>
//            [<FieldAttribute("sample[1-n]-tissue[1-n]")>]
//            mutable SampleTissues                    : array<string>
//            [<FieldAttribute("sample[1-n]-cell_type[1-n]")>]
//            mutable SamplesCellTypes                 : array<string>
//            [<FieldAttribute("sample[1-n]-disease[1-n]")>]
//            mutable SamplesDiseases                  : array<string>
//            [<FieldAttribute("sample[1-n]-description")>]
//            mutable SampleDescriptions               : array<string>
//            [<FieldAttribute("sample[1-n]-custom[1-n]")>]
//            mutable SamplesCustoms                   : array<string>
//            [<FieldAttribute("assay[1-n]-quantification_reagent")>]
//            mutable AssaysQuantificationReagent      : string
//            [<FieldAttribute("assay[1-n]-quantification_mod[1-n]")>]
//            mutable AssaysQuantificationMods         : array<string>
//            [<FieldAttribute("assay[1-n]-quantification_mod[1-n]-site")>]
//            mutable AssaysQuantificationModsSite     : string
//            [<FieldAttribute("assay[1-n]-quantification_mod[1-n]-position")>]
//            mutable AssaysQuantificationModsPosition : string
//            [<FieldAttribute("assay[1-n]-sample_ref")>]
//            mutable AssaysSampleRef                  : string
//            [<FieldAttribute("assay[1-n]-ms_run_ref")>]
//            mutable AssaysMSRunRef                   : string
//            [<FieldAttribute("study_variable[1-n]-assay_refs")>]
//            mutable StudyVariablesAssayRefs          : array<string>
//            [<FieldAttribute("study_variable[1-n]-sample_refs")>]
//            mutable StudyVariablesSampleRefs         : array<string>
//            [<FieldAttribute("study_variable[1-n]-description")>]
//            mutable StudyVariablesDescription        : array<string>
//            [<FieldAttribute("cv[1-n]-label")>]
//            mutable CvsLabel                         : array<string>
//            [<FieldAttribute("cv[1-n]-full_name")>]
//            mutable CvsFullName                      : array<string>
//            [<FieldAttribute("cv[1-n]-version")>]
//            mutable CvsVersion                       : array<string>
//            [<FieldAttribute("cv[1-n]-url")>]
//            mutable CvsURI                           : array<string>
//            [<FieldAttribute("colunit-protein")>]
//            mutable ColUnitProtein                   : string
//            [<FieldAttribute("colunit-peptide")>]
//            mutable ColUnitPeptide                   : string
//            [<FieldAttribute("colunit-psm")>]
//            mutable ColUnitPSM                       : string
//            [<FieldAttribute("colunit-small_molecule")>]
//            mutable ColUnitSmallMolecule             : string
//           }

//    ///The protein section can provide additional information about the reported proteins in the mzTab file.
//    type ProteinSection =
//           {
//            [<FieldAttribute("accession")>]
//            Accession:string
//            [<FieldAttribute("description")>]
//            Description:string
//            [<FieldAttribute("taxid")>]
//            Taxid:int
//            [<FieldAttribute("species")>]
//            Species:string
//            [<FieldAttribute("database")>]
//            Database:string
//            [<FieldAttribute("database_version")>]
//            DatabaseVersion:string
//            [<FieldAttribute("search_engine")>]
//            SearchEngines:array<string>
//            [<FieldAttribute("best_search_engine_score[1-n]")>]
//            BestSearchEngineScore:array<float>
//            [<FieldAttribute("search_engine_score[1-n]_ms_run[1-n]")>]
//            mutable SearchEngineScoresMSRuns:array<float>
//            [<FieldAttribute("reliability")>]
//            mutable Reliability:int
//            [<FieldAttribute("num_psms_ms_run[1-n]")>]
//            mutable NumPSMsMSRuns:array<int>
//            [<FieldAttribute("num_peptides_distinct_ms_run[1-n]")>]
//            mutable NumPeptidesDistinctMSRuns:array<int>
//            [<FieldAttribute("num_peptides_unique_ms_run[1-n]")>]
//            mutable NumPeptidesUniqueMSRuns:array<int>
//            [<FieldAttribute("ambiguity_members")>]
//            AmbiguityMembers:array<string>
//            [<FieldAttribute("modifications")>]
//            Modifications:array<string>
//            [<FieldAttribute("uri")>]
//            mutable URI:string
//            [<FieldAttribute("go_terms")>]
//            mutable GoTerms:array<string>
//            [<FieldAttribute("protein_coverage")>]
//            mutable ProteinCoverage:float
//            [<FieldAttribute("protein_abundance_assay[1-n]")>]
//            mutable ProteinAbundanceAssays:array<float>
//            [<FieldAttribute("protein_abundance_study_variable[1-n]")>]
//            mutable ProteinAbundanceStudyVariables:array<float>
//            [<FieldAttribute("protein_abundance_stdev_study_variable[1-n]")>]
//            mutable ProteinAbundanceStandardDeviationStudyVariables:array<float>
//            [<FieldAttribute("protein_abundance_std_error_study_variable[1-n]")>]
//            mutable ProteinAbundanceStandardErrorStudyVariables:array<float>
//            [<FieldAttribute("opt_{identifier}_*")>]
//            mutable OptionalInformations:array<string>
//           }

//    ///The peptide section can provide additional information about the reported peptides in the mzTab file.
//    type PeptideSection =
//            {
//             [<FieldAttribute("sequence")>]
//             mutable PeptideSequence:string
//             [<FieldAttribute("accession")>]
//             mutable Accession:string
//             [<FieldAttribute("unique")>]
//             mutable Unique:bool
//             [<FieldAttribute("database")>]
//             mutable Database:string
//             [<FieldAttribute("database_version")>]
//             mutable DatabaseVersion:string
//             [<FieldAttribute("search_engine")>]
//             mutable SearchEngines:array<string>
//             [<FieldAttribute("best_search_engine_score[1-n]")>]
//             mutable BestSearchEngineScores:array<float>
//             [<FieldAttribute("search_engine_score[1-n]_ms_run[1-n]")>]
//             mutable SearchEngineScoresMSRuns:array<float>
//             [<FieldAttribute("reliability")>]
//             mutable Reliability:int
//             [<FieldAttribute("modifications")>]
//             mutable Modifications:array<string>
//             [<FieldAttribute("retention_time")>]
//             mutable RetentionTime:array<float>
//             [<FieldAttribute("retention_time_window")>]
//             mutable RetentionTimeWindow:array<float>
//             [<FieldAttribute("charge")>]
//             mutable Charge:int
//             [<FieldAttribute("mass_to_charge")>]
//             mutable MassToCharge:float
//             [<FieldAttribute("uri")>]
//             mutable URI:string
//             [<FieldAttribute("spectra_ref")>]
//             mutable SpectraRefs:array<string>
//             [<FieldAttribute("peptide_abundance_assay[1-n]")>]
//             mutable PeptideAbundanceAssays:array<float>
//             [<FieldAttribute("peptide_abundance_study_variable[1-n]")>]
//             mutable PeptideAbundanceStudyVariables:array<float>
//             [<FieldAttribute("peptide_abundance_stdev_study_variable[1-n]")>]
//             mutable PeptideAbundanceStandardDeviationStudyVariables:array<float>
//             [<FieldAttribute("peptide_abundance_std_error_study_variable[1-n]")>]
//             mutable PeptideAbundanceStandardErrorStudyVariables:array<float>
//             [<FieldAttribute("opt_{identifier}_*")>]
//             mutable OptionalInformations:array<string>
//            } 

//    ///The psm section can provide additional information about the reported psms in the mzTab file.
//    type PSMSection =
//            {
//             [<FieldAttribute("sequence")>]
//             PeptideSequence:string
//             [<FieldAttribute("PSM_ID")>]
//             PSMID:string
//             [<FieldAttribute("accession")>]
//             Accession:string
//             [<FieldAttribute("unique")>]
//             Unique:bool
//             [<FieldAttribute("database")>]
//             Database:string
//             [<FieldAttribute("database_version")>]
//             DatabaseVersion:string
//             [<FieldAttribute("search_engine")>]
//             SearchEngines:array<string>
//             [<FieldAttribute("search_engine_score[1-n]")>]
//             SearchEngineScores:array<float>
//             [<FieldAttribute("reliability")>]
//             mutable Reliability:int
//             [<FieldAttribute("modifications")>]
//             Modifications:array<string>
//             [<FieldAttribute("retention_time")>]
//             RetentionTimes:array<float>
//             [<FieldAttribute("charge")>]
//             Charge:int
//             [<FieldAttribute("exp_mass_to_charge")>]
//             ExpMassToCharge:float
//             [<FieldAttribute("calc_mass_to_charge")>]
//             CalcMassToCharge:float
//             [<FieldAttribute("uri")>]
//             mutable URI:string
//             [<FieldAttribute("spectra_ref")>]
//             SpectraRefs:array<string>
//             [<FieldAttribute("pre")>]
//             Pre:string
//             [<FieldAttribute("post")>]
//             Post:string
//             [<FieldAttribute("start")>]
//             Start:int
//             [<FieldAttribute("end")>]
//             End:int
//             [<FieldAttribute("opt_{identifier}_*")>]
//             mutable OptionalInformations:array<string>
//            }

//    ///The psm section can provide additional information about the reported psms in the mzTab file.
//    type SmallMoleculeSection =
//            {
//             [<FieldAttribute("identifier")>]
//             Identifiers:array<string>
//             [<FieldAttribute("chemical_formula")>]
//             ChemicalFormula:string
//             [<FieldAttribute("smiles")>]
//             Smiles:array<string>
//             [<FieldAttribute("inchi_key")>]
//             InchiKey:array<string>
//             [<FieldAttribute("description")>]
//             Description:array<string>
//             [<FieldAttribute("exp_mass_to_charge")>]
//             ExpMassToCharge:float
//             [<FieldAttribute("calc_mass_to_charge")>]
//             CalcMassToCharge:float
//             [<FieldAttribute("charge")>]
//             Charge:int
//             [<FieldAttribute("retention_time")>]
//             RetentionTimes:array<float>
//             [<FieldAttribute("taxid")>]
//             Taxid:int
//             [<FieldAttribute("species")>]
//             Species:string
//             [<FieldAttribute("database")>]
//             Database:string
//             [<FieldAttribute("database_version")>]
//             DatabaseVersion:string
//             [<FieldAttribute("reliability")>]
//             mutable Reliability:int
//             [<FieldAttribute("uri")>]
//             mutable URI:string
//             [<FieldAttribute("spectra_ref")>]
//             SpectraRefs:array<string>
//             [<FieldAttribute("search_engine")>]
//             SearchEngines:array<string>
//             [<FieldAttribute("best_search_engine_score[1-n]")>]
//             BestSearchEngineScores:array<float>
//             [<FieldAttribute("search_engine_score[1-n]_ms_run[1-n]")>]
//             mutable SearchEngineScoresMSRuns:array<float>
//             [<FieldAttribute("modifications")>]
//             Modifications:array<string>
//             [<FieldAttribute("smallmolecule_abundance_assay[1-n]")>]
//             mutable SmallMoleculeAbundanceAssays:array<float>
//             [<FieldAttribute("smallmolecule_abundance_study_variable[1-n]")>]
//             mutable SmallMoleculeAbundanceStudyVariables:array<float>
//             [<FieldAttribute("smallmolecule_abundance_stdev_study_variable [1-n]")>]
//             mutable SmallMoleculeAbundanceStandardDeviationStudyVariables:array<float>
//             [<FieldAttribute("smallmolecule_abundance_std_error_study_variable[1-n]")>]
//             mutable SmallMoleculeAbundanceStandardErrorStudyVariables:array<float>
//             [<FieldAttribute("opt_{identifier}_*")>]
//             mutable OptionalInformations:array<string>
//            }

//    type TSVFile =
//        {
//         MetaDataSection       : MetaDataSection
//         ProteinSections       : array<ProteinSection>
//         PeptideSections       : array<PeptideSection>
//         PSMSections           : array<PSMSection>
//         SmallMoleculeSections : array<SmallMoleculeSection>
//        }