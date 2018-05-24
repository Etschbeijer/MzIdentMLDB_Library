namespace MzIdentMLDataBase


module DataContext =

    open System
    open System.ComponentModel.DataAnnotations.Schema
    open Microsoft.EntityFrameworkCore
    open System.Collections.Generic


    module EntityTypes =

        //Type definitions

        type [<CLIMutable>] 
            Term =
            {
                ID               : string
                mutable Name     : string
                mutable Ontology : Ontology
                RowVersion       : DateTime 
            }

        ///Standarized vocabulary for MS-Database.
        and [<CLIMutable>] 
            Ontology = 
            {
                ID             : string
                mutable Terms  : List<Term>
                RowVersion     : DateTime
            }

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            CVParam =
            {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                 : int
                Name               : string
                mutable Value      : string
                Term               : Term
                mutable Unit       : Term
                mutable UnitName   : string
                RowVersion         : DateTime 
            }

        /////A single entry from an ontology or a controlled vocabulary.
        //type [<CLIMutable>] 
        //    UserParam =
        //    {
        //     [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
        //     ID         : int
        //     Name       : string
        //     Value      : string
        //     Type       : string
        //     Unit       : Term
        //     UnitName   : string
        //     RowVersion : DateTime 
        //    }

        ///Organizations are entities like companies, universities, government agencies.
        type [<CLIMutable>] 
                Organization =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID              : int
                mutable Name    : string
                mutable Details : List<CVParam>
                //Formerly Parent/Organization_Ref
                mutable Parent  : string
                //
                RowVersion      : DateTime
                }

        ///A person's name and contact details.
        type [<CLIMutable>] 
                Person =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                     : int
                mutable Name           : string
                mutable FirstName      : string
                mutable MidInitials    : string
                mutable LastName       : string
                //Formerly Organization_Ref
                mutable Organizations  : List<Organization>
                //
                mutable Details        : List<CVParam>
                RowVersion             : DateTime
                //CVParams_Organization   : List<CVParam>
                //UserParams_Organization : List<UserParam>
                }

        ///The software used for performing the analyses.
        type [<CLIMutable>] 
            ContactRole =
            {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : int
                //Formerly Contact_Ref
                Person     : Person
                //
                Role       : CVParam
                RowVersion : DateTime 
            }

        ///The software used for performing the analyses.
        type [<CLIMutable>] 
            AnalysisSoftware =
            {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                     : int
                mutable Name           : string
                mutable URI            : string
                mutable Version        : string
                mutable Customizations : string
                mutable ContactRole    : ContactRole
                SoftwareName           : CVParam
                RowVersion             : DateTime 
            }

        ///References to the individual component samples within a mixed parent sample.
        type [<CLIMutable>] 
                SubSample =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                  : int
                //Formerly Sample_Ref
                mutable SubSampleID : string
                //
                RowVersion          : DateTime
                }

        ///A description of the sample analysed by mass spectrometry using CVParams or UserParams.
        type [<CLIMutable>] 
                Sample =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                   : int
                mutable Name         : string
                mutable ContactRoles : List<ContactRole>
                mutable SubSamples   : List<SubSample>
                mutable Details      : List<CVParam>
                RowVersion          : DateTime
                }

        ///A molecule modification specification.
        type [<CLIMutable>] 
                Modification =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                            : int
                mutable Residues              : string
                mutable Location              : int
                mutable MonoIsotopicMassDelta : float
                mutable AvgMassDelta          : float
                Details                       : List<CVParam>
                RowVersion                    : DateTime
                }

        ///A modification where one residue is substituted by another (amino acid change).
        type [<CLIMutable>] 
                SubstitutionModification =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                            : int
                OriginalResidue               : string
                ReplacementResidue            : string
                mutable Location              : int
                mutable MonoIsotopicMassDelta : float
                mutable AvgMassDelta          : float
                RowVersion                    : DateTime
                }

        ///One (poly)peptide (a sequence with modifications).
        type [<CLIMutable>] 
                Peptide =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                : int
                mutable Name                      : string
                PeptideSequence                   : string
                mutable Modifications             : List<Modification>
                mutable SubstitutionModifications : List<SubstitutionModification>
                mutable Details                   : List<CVParam>
                RowVersion                        : DateTime
                }

        ///PeptideEvidence links a specific Peptide element to a specific position in a DBSequence.
        type [<CLIMutable>] 
                TranslationTable =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID              : int
                mutable Name    : string
                mutable Details : List<CVParam>
                RowVersion      : DateTime
                }

        ///References to CV terms defining the measures about product ions to be reported in SpectrumIdentificationItem.
        type [<CLIMutable>] 
                Measure =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID           : int
                mutable Name : string
                Details      : List<CVParam>
                RowVersion   : DateTime
                }

        ///The specification of a single residue within the mass table.
        type [<CLIMutable>] 
                Residue =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : int
                Code       : string
                Mass       : float
                RowVersion : DateTime
                }

        ///Ambiguous residues e.g. X can be specified by the Code attribute and a set of parameters for example giving the different masses that will be used in the search.
        type [<CLIMutable>] 
                AmbiguousResidue =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : int
                Code       : string
                Details    : List<CVParam>
                RowVersion : DateTime
                }

        ///The masses of residues used in the search.
        type [<CLIMutable>] 
                MassTable =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                       : int
                mutable Name             : string
                MSLevel                  : string
                mutable Residue          : List<Residue>
                mutable AmbiguousResidue : List<AmbiguousResidue>
                mutable Details          : List<CVParam>
                RowVersion               : DateTime
                }

        ///The values of this particular measure, corresponding to the index defined in ion type.
        type [<CLIMutable>] 
                Value =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : int
                Value      : float
                RowVersion : DateTime
                }

        ///An array of values for a given type of measure and for a particular ion type, in parallel to the index of ions identified.
        type [<CLIMutable>] 
                FragmentArray =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : int
                Measure    : Measure
                Values     : List<Value>
                RowVersion : DateTime
                }

        ///The index of ions identified as integers, following standard notation for a-c, x-z e.g. if b3 b5 and b6 have been identified, the index would store "3 5 6".
        type [<CLIMutable>] 
                Index =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : int
                Index      : int
                RowVersion : DateTime
                }

        ///IonType defines the index of fragmentation ions being reported, importing a CV term for the Type of ion e.g. b ion. Example: if b3 b7 b8 and b10 have been identified, the index attribute will contain 3 7 8 10.
        type [<CLIMutable>] 
                IonType =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                    : int
                mutable Index         : List<Index>
                mutable FragmentArray : List<FragmentArray>
                Details               : List<CVParam>
                RowVersion            : DateTime
                }

        ///A data set containing spectra data (consisting of one or more spectra).
        type [<CLIMutable>] 
                SpectraData =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                  : int
                mutable Name                        : string
                Location                            : string
                mutable ExternalFormatDocumentation : string
                FileFormat                          : CVParam
                SpectrumIDFormat                    : CVParam
                RowVersion                          : DateTime
                }

        ///The specificity rules of the searched modification including for example the probability of a modification's presence or peptide or protein termini.
        type [<CLIMutable>] 
                SpecificityRules =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : int
                Details    : List<CVParam>
                RowVersion : DateTime
                }    

        ///Specification of a search modification as parameter for a spectra search.
        type [<CLIMutable>] 
                SearchModification =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                       : int
                FixedMod                 : bool
                MassDelta                : float
                Residues                 : string
                mutable SpecificityRules : List<SpecificityRules>
                Details                  : List<CVParam>
                RowVersion               : DateTime
                }

        ///The details of an individual cleavage enzyme should be provided by giving a regular expression or a CV term if a "standard" enzyme cleavage has been performed.
        type [<CLIMutable>] 
                Enzyme =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                      : int
                mutable Name            : string
                mutable CTermGain       : string
                mutable NTermGain       : string
                mutable MinDistance     : int
                mutable MissedCleavages : int
                mutable SemiSpecific    : bool
                mutable SiteRegexc      : string
                mutable EnzymeName      : List<CVParam>
                RowVersion              : DateTime
                }

        ///Filters applied to the search database. The filter MUST include at least one of Include and Exclude.
        type [<CLIMutable>] 
                Filter =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID               : int
                FilterType       : CVParam
                mutable Includes : List<CVParam>
                mutable Excludes : List<CVParam>
                RowVersion       : DateTime
                }

        ///The frames in which the nucleic acid sequence has been translated as a space separated list.
        type [<CLIMutable>] 
                Frame =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : int
                Frame      : string
                RowVersion : DateTime
                }

        ///The parameters and settings of a SpectrumIdentification analysis.
        type [<CLIMutable>] 
                SpectrumIdentificationProtocol =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                             : int
                mutable Name                   : string
                //Formerly AnalysisSoftware_Ref
                AnalysisSoftware               : AnalysisSoftware
                //
                SearchType                     : CVParam
                mutable AdditionalSearchParams : List<CVParam>
                mutable ModificationParams     : List<SearchModification>
                //Formerly Enzymes
                mutable Enzymes                : List<Enzyme>
                mutable Independent_Enzymes    : bool
                //
                mutable MassTables             : List<MassTable>
                mutable FragmentTolerance      : List<CVParam>
                mutable ParentTolerance        : List<CVParam>
                Threshold                      : List<CVParam>
                mutable DatabaseFilters        : List<Filter>
                //DatabaseTranlation
                mutable Frames                 : List<Frame>
                mutable TranslationTables      : List<TranslationTable>
                //
                RowVersion                     : DateTime
                }

        ///A database for searching mass spectra.
        type [<CLIMutable>] 
                SearchDatabase =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                  : int
                mutable Name                        : string
                Location                            : string
                mutable NumDatabaseSequences        : string
                mutable NumResidues                 : string
                mutable ReleaseDate                 : DateTime
                mutable Version                     : string
                mutable ExternalFormatDocumentation : string
                FileFormat                          : CVParam
                DatabaseName                        : CVParam
                mutable Details                     : List<CVParam>
                RowVersion                          : DateTime
                }


        ///A database sequence from the specified SearchDatabase (nucleic acid or amino acid).
        type [<CLIMutable>] 
                DBSequence =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID               : int
                mutable Name     : string
                Accession        : string
                SearchDatabase   : SearchDatabase
                mutable Sequence : string
                mutable Length   : int
                mutable Details  : List<CVParam>
                RowVersion       : DateTime
                }

        ///PeptideEvidence links a specific Peptide element to a specific position in a DBSequence.
        type [<CLIMutable>] 
                PeptideEvidence =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                       : int
                mutable Name             : string
                //Formerly DBSequence_Ref
                DBSequence               : DBSequence
                //
                //Formerly Peptide_Ref
                Peptide                  : Peptide
                //
                mutable Start            : int
                mutable End              : int
                mutable Pre              : string
                mutable Post             : string
                mutable Frame            : Frame
                mutable IsDecoy          : bool
                //Formerly TranslationTable_Ref
                mutable TranslationTable : TranslationTable
                //
                mutable Details          : List<CVParam>
                RowVersion               : DateTime
                }

        ///An identification of a single (poly)peptide, resulting from querying an input spectra, along with the set of confidence values for that identification.
        type [<CLIMutable>] 
                SpectrumIdentificationItem =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                             : int
                mutable Name                   : string
                mutable Sample                 : Sample
                mutable MassTable              : MassTable
                PassThreshold                  : bool
                Rank                           : int
                mutable PeptideEvidences       : List<PeptideEvidence>
                mutable Fragmentations         : List<IonType>
                Peptide                        : Peptide
                ChargeState                    : int
                ExperimentalMassToCharge       : float
                mutable CalculatedMassToCharge : float
                mutable CalculatedPI           : float
                mutable Details                : List<CVParam>
                RowVersion                     : DateTime
                }

        ///All identifications made from searching one spectrum.
        type [<CLIMutable>] 
                SpectrumIdentificationResult =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                         : int
                mutable Name               : string
                SpectraData                : SpectraData
                SpectrumID                 : string
                SpectrumIdentificationItem : List<SpectrumIdentificationItem>
                mutable Details            : List<CVParam>
                RowVersion                 : DateTime
                }

        ///Represents the set of all search results from SpectrumIdentification.
        type [<CLIMutable>] 
                SpectrumIdentificationList =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                           : int
                mutable Name                 : string
                mutable NumSequencesSearched : int
                mutable FragmentationTables  : List<Measure>
                SpectrumIdentificationResult : List<SpectrumIdentificationResult>
                mutable Details              : List<CVParam>
                RowVersion                   : DateTime
                }


        ///An Analysis which tries to identify peptides in input spectra, referencing the database searched, the input spectra, the output results and the protocol that is run.
        type [<CLIMutable>] 
                SpectrumIdentification =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                             : int
                mutable Name                   : string
                mutable ActivityDate           : DateTime
                SpectrumIdentificationList     : SpectrumIdentificationList
                SpectrumIdentificationProtocol : SpectrumIdentificationProtocol
                //SpectraData_Ref
                SpectraData                    : List<SpectraData>
                //
                //SearchDatabase_Ref
                SearchDatabase                 : List<SearchDatabase>
                //
                RowVersion                     : DateTime
                }

        ///The parameters and settings of a SpectrumIdentification analysis.
        type [<CLIMutable>] 
                ProteinDetectionProtocol =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID : int
                mutable Name           : string
                AnalysisSoftware       : AnalysisSoftware
                mutable AnalysisParams : List<CVParam>
                Threshold              : List<CVParam>
                RowVersion             : DateTime
                }

        ///A file from which this mzIdentML instance was created.
        type [<CLIMutable>] 
                SourceFile =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                  : int
                mutable Name                        : string
                Location                            : string
                mutable ExternalFormatDocumentation : string
                FileFormat                          : CVParam
                mutable Details                     : List<CVParam>
                RowVersion                          : DateTime
                }

        ///The inputs to the analyses including the databases searched, the spectral data and the source file converted to mzIdentML.
        type [<CLIMutable>] 
                Inputs =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                      : int
                mutable SourceFiles     : List<SourceFile>
                mutable SearchDatabases : List<SearchDatabase>
                SpectraData             : List<SpectraData>
                RowVersion              : DateTime
                }

        ///Peptide evidence on which this ProteinHypothesis is based by reference to a PeptideEvidence element.
        type [<CLIMutable>] 
                PeptideHypothesis =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                          : int
                PeptideEvidence             : PeptideEvidence
                SpectrumIdentificationItems : List<SpectrumIdentificationItem>
                RowVersion                  : DateTime
                }

        ///A single result of the ProteinDetection analysis (i.e. a protein).
        type [<CLIMutable>] 
                ProteinDetectionHypothesis =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                : int
                mutable Name      : string
                PassThreshold     : bool
                DBSequence        : DBSequence
                PeptideHypothesis : List<PeptideHypothesis>
                mutable Details   : List<CVParam>
                RowVersion        : DateTime
                }

        ///A set of logically related results from a protein detection, for example to represent conflicting assignments of peptides to proteins.
        type [<CLIMutable>] 
                ProteinAmbiguityGroup =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                        : int
                mutable Name              : string
                ProteinDetecionHypothesis : List<ProteinDetectionHypothesis>
                mutable Details           : List<CVParam>
                RowVersion                : DateTime
                }

        ///The protein list resulting from a protein detection process.
        type [<CLIMutable>] 
                ProteinDetectionList =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                             : int
                mutable Name                   : string
                mutable ProteinAmbiguityGroups : List<ProteinAmbiguityGroup>
                mutable Details                : List<CVParam>
                RowVersion                     : DateTime
                }

        ///Data sets generated by the analyses, including peptide and protein lists.
        type [<CLIMutable>] 
                AnalysisData =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                           : int
                SpectrumIdentificationList   : List<SpectrumIdentificationList>
                mutable ProteinDetectionList : ProteinDetectionList
                RowVersion                   : DateTime
                }

        ///An Analysis which assembles a set of peptides (e.g. from a spectra search analysis) to proteins. 
        type [<CLIMutable>] 
                ProteinDetection =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                       : int
                mutable Name             : string
                mutable ActivityDate     : DateTime
                ProteinDetectionList     : ProteinDetectionList
                ProteinDetectionProtocol : ProteinDetectionProtocol
                SpectrumIdentifications  : List<SpectrumIdentification>
                RowVersion               : DateTime
                }

        ///Any bibliographic references associated with the file.
        type [<CLIMutable>] 
                BiblioGraphicReference =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                  : int
                mutable Name        : string
                mutable Authors     : string
                mutable DOI         : string
                mutable Editor      : string
                mutable Issue       : string
                mutable Pages       : string
                mutable Publication : string
                mutable Publisher   : string
                mutable Title       : string
                mutable Volume      : string
                mutable Year        : int
                RowVersion          : DateTime
                }

        ///The Provider of the mzIdentML record in terms of the contact and software.
        type [<CLIMutable>] 
                Provider =
                {
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                       : int
                mutable Name             : string
                //Formerly AnalysisSoftware_Ref
                mutable AnalysisSoftware : AnalysisSoftware
                //
                mutable ContactRole      : ContactRole
                RowVersion               : DateTime
                }

        ///The upper-most hierarchy level of mzIdentML with sub-containers for example describing software, protocols and search results (spectrum identifications or protein detection results). 
        type [<CLIMutable>] 
                MzIdentML =
                {
                ID                               : int
                mutable Name                     : string
                Version                          : string
                //Formerly CVList
                Ontologies                       : List<Ontology>
                //
                //Formerly AnalysisSoftwareList
                mutable AnalysisSoftwares        : List<AnalysisSoftware>
                //
                mutable Provider                 : Provider
                //Formerly AuditCollection
                mutable Person                   : Person
                mutable Organization             : Organization
                //
                //Formerly AnalysisSampleCollection
                mutable Samples                  : List<Sample>
                //
                //Formerly SequenceCollection
                mutable DBSequences              : List<DBSequence>
                mutable Peptides                 : List<Peptide>
                mutable PeptideEvidences         : List<PeptideEvidence>
                //
                //AnalysisCollection
                SpectrumIdentification           : List<SpectrumIdentification>
                mutable ProteinDetection         : ProteinDetection
                //
                //AnalysisProtocolCollection
                SpectrumIdentificationProtocol   : List<SpectrumIdentificationProtocol>
                mutable ProteinDetectionProtocol : ProteinDetectionProtocol
                //
                //DataCollection
                Inputs                           : Inputs
                AnalysisData                     : AnalysisData
                //
                mutable BiblioGraphicReferences  : List<BiblioGraphicReference> 
                RowVersion                       : DateTime
                }

    module DataContext =

        open EntityTypes

        type MzIdentMLContext =
     
                inherit DbContext

                new(options : DbContextOptions<MzIdentMLContext>) = {inherit DbContext(options)}

                [<DefaultValue>] 
                val mutable m_term : DbSet<Term>
                member public this.Term with get() = this.m_term
                                                    and set value = this.m_term <- value
  
                [<DefaultValue>] 
                val mutable m_Ontology : DbSet<Ontology>
                member public this.Ontology with get() = this.m_Ontology
                                                        and set value = this.m_Ontology <- value 

                [<DefaultValue>] 
                val mutable m_cvParam : DbSet<CVParam>
                member public this.CVParam with get() = this.m_cvParam
                                                        and set value = this.m_cvParam <- value

                [<DefaultValue>] 
                val mutable m_AnalysisSoftware : DbSet<AnalysisSoftware>
                member public this.AnalysisSoftware with get() = this.m_AnalysisSoftware
                                                                and set value = this.m_AnalysisSoftware <- value

                [<DefaultValue>] 
                val mutable m_Person : DbSet<Person>
                member public this.Person with get() = this.m_Person
                                                    and set value = this.m_Person <- value

                [<DefaultValue>] 
                val mutable m_Organization : DbSet<Organization>
                member public this.Organization with get() = this.m_Organization
                                                            and set value = this.m_Organization <- value

                [<DefaultValue>] 
                val mutable m_DBSequence : DbSet<DBSequence>
                member public this.DBSequence with get() = this.m_DBSequence
                                                        and set value = this.m_DBSequence <- value     

                [<DefaultValue>] 
                val mutable m_Sample : DbSet<Sample>
                member public this.Sample with get() = this.m_Sample
                                                    and set value = this.m_Sample <- value   

                [<DefaultValue>] 
                val mutable m_Modification : DbSet<Modification>
                member public this.Modification with get() = this.m_Modification
                                                            and set value = this.m_Modification <- value  

                [<DefaultValue>] 
                val mutable m_SubstitutionModification : DbSet<SubstitutionModification>
                member public this.SubstitutionModification with get() = this.m_SubstitutionModification
                                                                        and set value = this.m_SubstitutionModification <- value

                [<DefaultValue>] 
                val mutable m_Peptide : DbSet<Peptide>
                member public this.Peptide with get() = this.m_Peptide
                                                        and set value = this.m_Peptide <- value

                [<DefaultValue>] 
                val mutable m_TranslationTable : DbSet<TranslationTable>
                member public this.TranslationTable with get() = this.m_TranslationTable
                                                                and set value = this.m_TranslationTable <- value

                [<DefaultValue>] 
                val mutable m_PeptideEvidence : DbSet<PeptideEvidence>
                member public this.PeptideEvidence with get() = this.m_PeptideEvidence
                                                                and set value = this.m_PeptideEvidence <- value

                [<DefaultValue>] 
                val mutable m_Measure : DbSet<Measure>
                member public this.Measure with get() = this.m_Measure
                                                        and set value = this.m_Measure <- value

                [<DefaultValue>] 
                val mutable m_Residue : DbSet<Residue>
                member public this.Residue with get() = this.m_Residue
                                                        and set value = this.m_Residue <- value

                [<DefaultValue>] 
                val mutable m_AmbiguousResidue : DbSet<AmbiguousResidue>
                member public this.AmbiguousResidue with get() = this.m_AmbiguousResidue
                                                                and set value = this.m_AmbiguousResidue <- value

                [<DefaultValue>] 
                val mutable m_MassTable : DbSet<MassTable>
                member public this.MassTable with get() = this.m_MassTable
                                                        and set value = this.m_MassTable <- value

                [<DefaultValue>] 
                val mutable m_FragmentArray : DbSet<FragmentArray>
                member public this.FragmentArray with get() = this.m_FragmentArray
                                                            and set value = this.m_FragmentArray <- value

                [<DefaultValue>] 
                val mutable m_IonType : DbSet<IonType>
                member public this.IonType with get() = this.m_IonType
                                                        and set value = this.m_IonType <- value

                [<DefaultValue>] 
                val mutable m_SpectrumIdentificationItem : DbSet<SpectrumIdentificationItem>
                member public this.SpectrumIdentificationItem with get() = this.m_SpectrumIdentificationItem
                                                                        and set value = this.m_SpectrumIdentificationItem <- value

                [<DefaultValue>] 
                val mutable m_SpectraData : DbSet<SpectraData>
                member public this.SpectraData with get() = this.m_SpectraData
                                                            and set value = this.m_SpectraData <- value

                [<DefaultValue>] 
                val mutable m_SpectrumIdentificationResult : DbSet<SpectrumIdentificationResult>
                member public this.SpectrumIdentificationResult with get() = this.m_SpectrumIdentificationResult
                                                                            and set value = this.m_SpectrumIdentificationResult <- value

                [<DefaultValue>] 
                val mutable m_SpectrumIdentificationList : DbSet<SpectrumIdentificationList>
                member public this.SpectrumIdentificationList with get() = this.m_SpectrumIdentificationList
                                                                        and set value = this.m_SpectrumIdentificationList <- value

                [<DefaultValue>] 
                val mutable m_SpecificityRules : DbSet<SpecificityRules>
                member public this.SpecificityRules with get() = this.m_SpecificityRules
                                                                and set value = this.m_SpecificityRules <- value

                [<DefaultValue>] 
                val mutable m_SearchModification : DbSet<SearchModification>
                member public this.SearchModification with get() = this.m_SearchModification
                                                                and set value = this.m_SearchModification <- value

                [<DefaultValue>] 
                val mutable m_Enzyme : DbSet<Enzyme>
                member public this.Enzyme with get() = this.m_Enzyme
                                                    and set value = this.m_Enzyme <- value

                [<DefaultValue>] 
                val mutable m_Filter : DbSet<Filter>
                member public this.Filter with get() = this.m_Filter
                                                    and set value = this.m_Filter <- value

                [<DefaultValue>] 
                val mutable m_SpectrumIdentificationProtocol : DbSet<SpectrumIdentificationProtocol>
                member public this.SpectrumIdentificationProtocol with get() = this.m_SpectrumIdentificationProtocol
                                                                            and set value = this.m_SpectrumIdentificationProtocol <- value

                [<DefaultValue>] 
                val mutable m_SearchDatabase : DbSet<SearchDatabase>
                member public this.SearchDatabase with get() = this.m_SearchDatabase
                                                            and set value = this.m_SearchDatabase <- value

                [<DefaultValue>] 
                val mutable m_SpectrumIdentification : DbSet<SpectrumIdentification>
                member public this.SpectrumIdentification with get() = this.m_SpectrumIdentification
                                                                    and set value = this.m_SpectrumIdentification <- value

                [<DefaultValue>] 
                val mutable m_ProteinDetectionProtocol : DbSet<ProteinDetectionProtocol>
                member public this.ProteinDetectionProtocol with get() = this.m_ProteinDetectionProtocol
                                                                        and set value = this.m_ProteinDetectionProtocol <- value

                [<DefaultValue>] 
                val mutable m_SourceFile : DbSet<SourceFile>
                member public this.SourceFile with get() = this.m_SourceFile
                                                        and set value = this.m_SourceFile <- value

                [<DefaultValue>] 
                val mutable m_Inputs : DbSet<Inputs>
                member public this.Inputs with get() = this.m_Inputs
                                                    and set value = this.m_Inputs <- value

                [<DefaultValue>] 
                val mutable m_PeptideHypothesis : DbSet<PeptideHypothesis>
                member public this.PeptideHypothesis with get() = this.m_PeptideHypothesis
                                                                and set value = this.m_PeptideHypothesis <- value

                [<DefaultValue>]
                val mutable m_ProteinDetectionHypothesis : DbSet<ProteinDetectionHypothesis>
                member public this.ProteinDetectionHypothesis with get() = this.m_ProteinDetectionHypothesis
                                                                        and set value = this.m_ProteinDetectionHypothesis <- value

                [<DefaultValue>]
                val mutable m_ProteinAmbiguityGroup : DbSet<ProteinAmbiguityGroup>
                member public this.ProteinAmbiguityGroup with get() = this.m_ProteinAmbiguityGroup
                                                                    and set value = this.m_ProteinAmbiguityGroup <- value

                [<DefaultValue>]
                val mutable m_ProteinDetectionList : DbSet<ProteinDetectionList>
                member public this.ProteinDetectionList with get() = this.m_ProteinDetectionList
                                                                    and set value = this.m_ProteinDetectionList <- value

                [<DefaultValue>]
                val mutable m_AnalysisData : DbSet<AnalysisData>
                member public this.AnalysisData with get() = this.m_AnalysisData
                                                            and set value = this.m_AnalysisData <- value

                [<DefaultValue>]
                val mutable m_BiblioGraphicReference : DbSet<BiblioGraphicReference>
                member public this.BiblioGraphicReference with get() = this.m_BiblioGraphicReference
                                                                    and set value = this.m_BiblioGraphicReference <- value


        type OntologyContext =
     
                inherit DbContext

                new(options : DbContextOptions<OntologyContext>) = {inherit DbContext(options)}

                [<DefaultValue>] 
                val mutable m_term : DbSet<Term>
                member public this.Term with get() = this.m_term
                                                    and set value = this.m_term <- value
  
                [<DefaultValue>] 
                val mutable m_Ontology : DbSet<Ontology>
                member public this.Ontology with get() = this.m_Ontology
                                                        and set value = this.m_Ontology <- value 


        type AnalysisSoftwareContext =
     
                inherit DbContext

                new(options : DbContextOptions<AnalysisSoftwareContext>) = {inherit DbContext(options)}

                [<DefaultValue>] 
                val mutable m_term : DbSet<Term>
                member public this.Term with get() = this.m_term
                                                    and set value = this.m_term <- value
  
                [<DefaultValue>] 
                val mutable m_Ontology : DbSet<Ontology>
                member public this.Ontology with get() = this.m_Ontology
                                                        and set value = this.m_Ontology <- value 

                [<DefaultValue>] 
                val mutable m_cvParam : DbSet<CVParam>
                member public this.CVParam with get() = this.m_cvParam
                                                        and set value = this.m_cvParam <- value

                [<DefaultValue>] 
                val mutable m_AnalysisSoftware : DbSet<AnalysisSoftware>
                member public this.AnalysisSoftware with get() = this.m_AnalysisSoftware
                                                                and set value = this.m_AnalysisSoftware <- value

