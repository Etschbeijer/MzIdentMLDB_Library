namespace MzIdentMLDataBase


module DataContext =

    open System
    open System.ComponentModel.DataAnnotations.Schema
    open Microsoft.EntityFrameworkCore
    open System.Collections.Generic


    module EntityTypes =

        //type definitions

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
                [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
                ID                : string
                mutable Terms     : List<Term>
                RowVersion        : DateTime
            }

        ///A single entry from an ontology or a controlled vocabulary.
        type CVParamBase =
            abstract member ID         : int
            abstract member Name       : string
            abstract member Value      : string
            abstract member Term       : Term
            abstract member Unit       : Term
            abstract member UnitName   : string
            abstract member RowVersion : DateTime

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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            OrganizationParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            PersonParam =
            {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                 : int
                Name               : string
                mutable Value      : string
                Term               : Term
                mutable Unit       : Term
                mutable UnitName   : string
                RowVersion         : DateTime 
            }
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        and [<CLIMutable>] 
            SampleParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            ModificationParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            PeptideParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            TranslationTableParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            MeasureParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            AmbiguousResidueParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            MassTableParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            IonTypeParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            SpecificityRuleParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            SearchModificationParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            EnzymeNameParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            IncludeParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            ExcludeParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            AdditionalSearchParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            FragmentToleranceParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            ParentToleranceParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            ThresholdParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            SearchDatabaseParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            DBSequenceParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            PeptideEvidenceParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            SpectrumIdentificationItemParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            SpectrumIdentificationResultParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            SpectrumIdentificationListParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            AnalysisParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            SourceFileParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            ProteinDetectionHypothesisParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            ProteinAmbiguityGroupParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion


        ///A single entry from an ontology or a controlled vocabulary.
        type [<CLIMutable>] 
            ProteinDetectionListParam =
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
            interface CVParamBase with
                member x.ID         = x.ID
                member x.Name       = x.Name
                member x.Value      = x.Value
                member x.Term       = x.Term
                member x.Unit       = x.Unit
                member x.UnitName   = x.UnitName
                member x.RowVersion = x.RowVersion

        /////A single entry from an ontology or a controlled vocabulary.
        //type [<CLIMutable>] 
        //    UserParam =
        //    {
        //     [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
        //     ID         : int
        //     Name       : string
        //     Value      : string
        //     type       : string
        //     Unit       : Term
        //     UnitName   : string
        //     RowVersion : DateTime 
        //    }

        ///Organizations are entities like companies, universities, government agencies.
        and [<CLIMutable>] 
                Organization =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID              : string
                mutable Name    : string
                mutable Details : List<OrganizationParam>
                //Formerly Parent/Organization_Ref
                mutable Parent  : string
                //
                RowVersion      : DateTime
                }

        ///A person's name type contact details.
        type [<CLIMutable>] 
                Person =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                     : string
                mutable Name           : string
                mutable FirstName      : string
                mutable MidInitials    : string
                mutable LastName       : string
                //Formerly Organization_Ref
                mutable Organizations  : List<Organization>
                //
                mutable Details        : List<PersonParam>
                RowVersion             : DateTime
                //CVParams_Organization   : List<CVParam>
                //UserParams_Organization : List<UserParam>
                }

        ///The software used for performing the analyses.
        type [<CLIMutable>] 
            ContactRole =
            {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
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
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                     : string
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
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                  : string
                //Formerly Sample_Ref
                mutable Sample      : Sample
                //
                RowVersion          : DateTime
                }

        ///A description of the sample analysed by mass spectrometry using CVParams or UserParams.
        and [<CLIMutable>] 
                Sample =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                   : string
                mutable Name         : string
                mutable ContactRoles : List<ContactRole>
                mutable SubSamples   : List<SubSample>
                mutable Details      : List<SampleParam>
                RowVersion           : DateTime
                }

        ///A molecule modification specification.
        type [<CLIMutable>] 
                Modification =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                            : string
                mutable Residues              : string
                mutable Location              : int
                mutable MonoIsotopicMassDelta : float
                mutable AvgMassDelta          : float
                Details                       : List<ModificationParam>
                RowVersion                    : DateTime
                }

        ///A modification where one residue is substituted by another (amino acid change).
        type [<CLIMutable>] 
                SubstitutionModification =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                            : string
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
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                : string
                mutable Name                      : string
                PeptideSequence                   : string
                mutable Modifications             : List<Modification>
                mutable SubstitutionModifications : List<SubstitutionModification>
                mutable Details                   : List<PeptideParam>
                RowVersion                        : DateTime
                }

        ///PeptideEvidence links a specific Peptide element to a specific position in a DBSequence.
        type [<CLIMutable>] 
                TranslationTable =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID              : string
                mutable Name    : string
                mutable Details : List<TranslationTableParam>
                RowVersion      : DateTime
                }

        ///References to CV terms defining the measures about product ions to be reported in SpectrumIdentificationItem.
        type [<CLIMutable>] 
                Measure =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID           : string
                mutable Name : string
                Details      : List<MeasureParam>
                RowVersion   : DateTime
                }

        ///The specification of a single residue within the mass table.
        type [<CLIMutable>] 
                Residue =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Code       : string
                Mass       : float
                RowVersion : DateTime
                }

        ///Ambiguous residues e.g. X can be specified by the Code attribute type a set of parameters for example giving the different masses that will be used in the search.
        type [<CLIMutable>] 
                AmbiguousResidue =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Code       : string
                Details    : List<AmbiguousResidueParam>
                RowVersion : DateTime
                }

        ///The masses of residues used in the search.
        type [<CLIMutable>] 
                MassTable =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                        : string
                mutable Name              : string
                MSLevel                   : string
                mutable Residues          : List<Residue>
                mutable AmbiguousResidues : List<AmbiguousResidue>
                mutable Details           : List<MassTableParam>
                RowVersion                : DateTime
                }

        ///The values of this particular measure, corresponding to the index defined in ion type.
        type [<CLIMutable>] 
                Value =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Value      : float
                RowVersion : DateTime
                }

        ///An array of values for a given type of measure type for a particular ion type, in parallel to the index of ions identified.
        type [<CLIMutable>] 
                FragmentArray =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Measure    : Measure
                Values     : List<Value>
                RowVersion : DateTime
                }

        ///The index of ions identified as integers, following standard notation for a-c, x-z e.g. if b3 b5 type b6 have been identified, the index would store "3 5 6".
        type [<CLIMutable>] 
                Index =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Index      : int
                RowVersion : DateTime
                }

        ///Iontype defines the index of fragmentation ions being reported, importing a CV term for the type of ion e.g. b ion. Example: if b3 b7 b8 type b10 have been identified, the index attribute will contain 3 7 8 10.
        type [<CLIMutable>] 
                IonType =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                     : string
                mutable Index          : List<Index>
                mutable FragmentArrays : List<FragmentArray>
                Details                : List<IonTypeParam>
                RowVersion             : DateTime
                }

        ///A data set containing spectra data (consisting of one or more spectra).
        type [<CLIMutable>] 
                SpectraData =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                  : string
                mutable Name                        : string
                Location                            : string
                mutable ExternalFormatDocumentation : string
                FileFormat                          : CVParam
                SpectrumIDFormat                    : CVParam
                RowVersion                          : DateTime
                }

        ///The specificity rules of the searched modification including for example the probability of a modification's presence or peptide or protein termini.
        type [<CLIMutable>]
                //Formerly Specificityrules
                SpecificityRule =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Details    : List<SpecificityRuleParam>
                RowVersion : DateTime
                }  
                //

        ///Specification of a search modification as parameter for a spectra search.
        type [<CLIMutable>] 
                SearchModification =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                       : string
                FixedMod                 : bool
                MassDelta                : float
                Residues                 : string
                mutable SpecificityRules : List<SpecificityRule>
                Details                  : List<SearchModificationParam>
                RowVersion               : DateTime
                }

        ///The details of an individual cleavage enzyme should be provided by giving a regular expression or a CV term if a "standard" enzyme cleavage has been performed.
        type [<CLIMutable>] 
                Enzyme =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                      : string
                mutable Name            : string
                mutable CTermGain       : string
                mutable NTermGain       : string
                mutable MinDistance     : int
                mutable MissedCleavages : int
                mutable SemiSpecific    : bool
                mutable SiteRegexc      : string
                mutable EnzymeName      : List<EnzymeNameParam>
                RowVersion              : DateTime
                }

        ///Filters applied to the search database. The filter MUST include at least one of Include type Exclude.
        type [<CLIMutable>] 
                Filter =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID               : string
                FilterType       : CVParam
                mutable Includes : List<IncludeParam>
                mutable Excludes : List<ExcludeParam>
                RowVersion       : DateTime
                }

        ///The frames in which the nucleic acid sequence has been translated as a space separated list.
        type [<CLIMutable>] 
                Frame =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Frame      : int
                RowVersion : DateTime
                }

        ///The parameters type settings of a SpectrumIdentification analysis.
        type [<CLIMutable>] 
                SpectrumIdentificationProtocol =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                             : string
                mutable Name                   : string
                //Formerly AnalysisSoftware_Ref
                AnalysisSoftware               : AnalysisSoftware
                //
                SearchType                     : CVParam
                mutable AdditionalSearchParams : List<AdditionalSearchParam>
                mutable ModificationParams     : List<SearchModification>
                //Formerly Enzymes
                mutable Enzymes                : List<Enzyme>
                mutable Independent_Enzymes    : bool
                //
                mutable MassTables             : List<MassTable>
                mutable FragmentTolerance      : List<FragmentToleranceParam>
                mutable ParentTolerance        : List<ParentToleranceParam>
                Threshold                      : List<ThresholdParam>
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
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                  : string
                mutable Name                        : string
                Location                            : string
                mutable NumDatabaseSequences        : int64
                mutable NumResidues                 : int64
                mutable ReleaseDate                 : DateTime
                mutable Version                     : string
                mutable ExternalFormatDocumentation : string
                FileFormat                          : CVParam
                DatabaseName                        : CVParam
                mutable Details                     : List<SearchDatabaseParam>
                RowVersion                          : DateTime
                }

        ///A database sequence from the specified SearchDatabase (nucleic acid or amino acid).
        type [<CLIMutable>] 
                DBSequence =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                : string
                mutable Name      : string
                Accession         : string
                SearchDatabase    : SearchDatabase
                mutable Sequence  : string
                mutable Length    : int
                mutable Details   : List<DBSequenceParam>
                RowVersion        : DateTime
                }

        ///PeptideEvidence links a specific Peptide element to a specific position in a DBSequence.
        type [<CLIMutable>] 
                PeptideEvidence =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                 : string
                mutable Name                       : string
                //Formerly DBSequence_Ref
                DBSequence                         : DBSequence
                //
                //Formerly Peptide_Ref
                Peptide                            : Peptide
                //
                mutable Start                      : int
                mutable End                        : int
                mutable Pre                        : string
                mutable Post                       : string
                mutable Frame                      : Frame
                mutable IsDecoy                    : bool
                //Formerly TranslationTable_Ref
                mutable TranslationTable           : TranslationTable
                //
                mutable Details                    : List<PeptideEvidenceParam>
                RowVersion                         : DateTime
                }

        ///An identification of a single (poly)peptide, resulting from querying an input spectra, along with the set of confidence values for that identification.
        type [<CLIMutable>] 
                SpectrumIdentificationItem =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                   : string
                mutable Name                         : string
                mutable Sample                       : Sample
                mutable MassTable                    : MassTable
                PassThreshold                        : bool
                Rank                                 : int
                mutable PeptideEvidences             : List<PeptideEvidence>
                mutable Fragmentations               : List<IonType>
                Peptide                              : Peptide
                ChargeState                          : int
                ExperimentalMassToCharge             : float
                mutable CalculatedMassToCharge       : float
                mutable CalculatedPI                 : float
                mutable Details                      : List<SpectrumIdentificationItemParam>
                RowVersion                           : DateTime
                }

        ///All identifications made from searching one spectrum.
        type [<CLIMutable>] 
                SpectrumIdentificationResult =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                 : string
                mutable Name                       : string
                SpectraData                        : SpectraData
                SpectrumID                         : string
                SpectrumIdentificationItem         : List<SpectrumIdentificationItem>
                mutable Details                    : List<SpectrumIdentificationResultParam>
                RowVersion                         : DateTime
                }

        ///Represents the set of all search results from SpectrumIdentification.
        type [<CLIMutable>] 
                SpectrumIdentificationList =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                           : string
                mutable Name                 : string
                mutable NumSequencesSearched : int64
                mutable FragmentationTables  : List<Measure>
                SpectrumIdentificationResult : List<SpectrumIdentificationResult>
                mutable Details              : List<SpectrumIdentificationListParam>
                RowVersion                   : DateTime
                }


        ///An Analysis which tries to identify peptides in input spectra, referencing the database searched, the input spectra, the output results type the protocol that is run.
        type [<CLIMutable>] 
                SpectrumIdentification =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                             : string
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

        ///The parameters type settings of a SpectrumIdentification analysis.
        type [<CLIMutable>] 
                ProteinDetectionProtocol =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                     : string
                mutable Name           : string
                AnalysisSoftware       : AnalysisSoftware
                mutable AnalysisParams : List<AnalysisParam>
                Threshold              : List<ThresholdParam>
                RowVersion             : DateTime
                }

        ///A file from which this mzIdentML instance was created.
        type [<CLIMutable>] 
                SourceFile =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                                  : string
                mutable Name                        : string
                Location                            : string
                mutable ExternalFormatDocumentation : string
                FileFormat                          : CVParam
                mutable Details                     : List<SourceFileParam>
                RowVersion                          : DateTime
                }

        ///The inputs to the analyses including the databases searched, the spectral data type the source file converted to mzIdentML.
        type [<CLIMutable>] 
                Inputs =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                      : string
                mutable SourceFiles     : List<SourceFile>
                mutable SearchDatabases : List<SearchDatabase>
                SpectraData             : List<SpectraData>
                RowVersion              : DateTime
                }

        ///Peptide evidence on which this ProteinHypothesis is based by reference to a PeptideEvidence element.
        type [<CLIMutable>] 
                PeptideHypothesis =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                          : string
                PeptideEvidence             : PeptideEvidence
                SpectrumIdentificationItems : List<SpectrumIdentificationItem>
                RowVersion                  : DateTime
                }

        ///A single result of the ProteinDetection analysis (i.e. a protein).
        type [<CLIMutable>] 
                ProteinDetectionHypothesis =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                : string
                mutable Name      : string
                PassThreshold     : bool
                DBSequence        : DBSequence
                PeptideHypothesis : List<PeptideHypothesis>
                mutable Details   : List<ProteinDetectionHypothesisParam>
                RowVersion        : DateTime
                }

        ///A set of logically related results from a protein detection, for example to represent conflicting assignments of peptides to proteins.
        type [<CLIMutable>] 
                ProteinAmbiguityGroup =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                        : string
                mutable Name              : string
                ProteinDetecionHypothesis : List<ProteinDetectionHypothesis>
                mutable Details           : List<ProteinAmbiguityGroupParam>
                RowVersion                : DateTime
                }

        ///The protein list resulting from a protein detection process.
        type [<CLIMutable>] 
                ProteinDetectionList =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                             : string
                mutable Name                   : string
                mutable ProteinAmbiguityGroups : List<ProteinAmbiguityGroup>
                mutable Details                : List<ProteinDetectionListParam>
                RowVersion                     : DateTime
                }

        ///Data sets generated by the analyses, including peptide type protein lists.
        type [<CLIMutable>] 
                AnalysisData =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                           : string
                SpectrumIdentificationList   : List<SpectrumIdentificationList>
                mutable ProteinDetectionList : ProteinDetectionList
                RowVersion                   : DateTime
                }

        ///An Analysis which assembles a set of peptides (e.g. from a spectra search analysis) to proteins. 
        type [<CLIMutable>] 
                ProteinDetection =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                           : string
                mutable Name                 : string
                mutable ActivityDate         : DateTime
                ProteinDetectionList         : ProteinDetectionList
                ProteinDetectionProtocol     : ProteinDetectionProtocol
                SpectrumIdentificationLists  : List<SpectrumIdentificationList>
                RowVersion                   : DateTime
                }

        ///Any bibliographic references associated with the file.
        type [<CLIMutable>] 
                BiblioGraphicReference =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                  : string
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

        ///The Provider of the mzIdentML record in terms of the contact type software.
        type [<CLIMutable>] 
                Provider =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                       : string
                mutable Name             : string
                //Formerly AnalysisSoftware_Ref
                mutable AnalysisSoftware : AnalysisSoftware
                //
                mutable ContactRole      : ContactRole
                RowVersion               : DateTime
                }

        ///The upper-most hierarchy level of mzIdentML with sub-containers for example describing software, protocols type search results (spectrum identifications or protein detection results). 
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
                mutable Persons                  : List<Person>
                mutable Organizations            : List<Organization>
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
                val mutable m_ContactRole : DbSet<ContactRole>
                member public this.ContactRole with get() = this.m_ContactRole
                                                            and set value = this.m_ContactRole <- value

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
                val mutable m_SubSample : DbSet<SubSample>
                member public this.SubSample with get() = this.m_SubSample
                                                          and set value = this.m_SubSample <- value  

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
                val mutable m_Value : DbSet<Value>
                member public this.Value with get() = this.m_Value
                                                      and set value = this.m_Value <- value

                [<DefaultValue>] 
                val mutable m_FragmentArray : DbSet<FragmentArray>
                member public this.FragmentArray with get() = this.m_FragmentArray
                                                              and set value = this.m_FragmentArray <- value

                [<DefaultValue>] 
                val mutable m_Index : DbSet<Index>
                member public this.Index with get() = this.m_Index
                                                      and set value = this.m_Index <- value

                [<DefaultValue>] 
                val mutable m_IonType : DbSet<IonType>
                member public this.Iontype with get() = this.m_IonType
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
                val mutable m_SpecificityRule : DbSet<SpecificityRule>
                member public this.SpecificityRule with get() = this.m_SpecificityRule
                                                                and set value = this.m_SpecificityRule <- value

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
                val mutable m_Frame : DbSet<Frame>
                member public this.Frame with get() = this.m_Frame
                                                      and set value = this.m_Frame <- value

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
                val mutable m_ProteinDetection : DbSet<ProteinDetection>
                member public this.ProteinDetection with get() = this.m_ProteinDetection
                                                                 and set value = this.m_ProteinDetection <- value

                [<DefaultValue>]
                val mutable m_Provider : DbSet<Provider>
                member public this.Provider with get() = this.m_Provider
                                                         and set value = this.m_Provider <- value

                [<DefaultValue>]
                val mutable m_BiblioGraphicReference : DbSet<BiblioGraphicReference>
                member public this.BiblioGraphicReference with get() = this.m_BiblioGraphicReference
                                                                       and set value = this.m_BiblioGraphicReference <- value

                [<DefaultValue>]
                val mutable m_MzIdentML : DbSet<MzIdentML>
                member public this.MzIdentML with get() = this.m_MzIdentML
                                                          and set value = this.m_MzIdentML <- value

                //override this.OnModelCreating (modelBuilder :  ModelBuilder) =
                //         modelBuilder.Entity<CVParam>()
                //            .HasOne("OrganizationID")
                //            .WithMany("Details")
                //            .HasForeignKey("FK")
                //            .OnDelete(DeleteBehavior.Cascade)|> ignore
                //         modelBuilder.Entity<CVParam>()
                //            .HasOne("PersonID")
                //            .WithMany("Details")
                //            .HasForeignKey("FK")
                //            .OnDelete(DeleteBehavior.Cascade)|> ignore
                //         modelBuilder.Entity<CVParam>()
                //            .HasOne("AmbiguousResidueID")
                //            .WithMany("Details")
                //            .HasForeignKey("FK")
                //            .OnDelete(DeleteBehavior.Cascade)|> ignore
                //         modelBuilder.Entity<CVParam>()
                //            .HasOne("SampleID")
                //            .WithMany("Details")
                //            .HasForeignKey("FK")
                //            .OnDelete(DeleteBehavior.Cascade)|> ignore

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

