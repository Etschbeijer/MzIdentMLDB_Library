namespace MzIdentMLDataBase


module DataContext =

    open System
    open System.ComponentModel.DataAnnotations.Schema
    open Microsoft.EntityFrameworkCore
    open System.Collections.Generic


    module EntityTypes =
        open System.Data

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
                mutable MzIdentML : MzIdentML
                RowVersion        : DateTime
            }

        ///A single entry from an ontology or a controlled vocabulary.
        and CVParamBase =
            abstract member ID         : int
            abstract member Name       : string
            abstract member Value      : string
            abstract member Term       : Term
            abstract member Unit       : Term
            abstract member UnitName   : string
            abstract member RowVersion : DateTime

        ///A single entry from an ontology or a controlled vocabulary.
        and [<CLIMutable>] [<Table("CVParams")>]
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
        and [<CLIMutable>] [<Table("OrganizationParams")>]
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
        and [<CLIMutable>] [<Table("PersonParams")>]
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
        and [<CLIMutable>] [<Table("SampleParams")>]
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
        and [<CLIMutable>] [<Table("ModificationParams")>]
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
        and [<CLIMutable>] [<Table("PeptideParams")>]
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
        and [<CLIMutable>] [<Table("TranslationTableParams")>]
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
        and [<CLIMutable>] [<Table("MeasureParams")>]
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
        and [<CLIMutable>] [<Table("AmbiguousResidueParams")>]
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
        and [<CLIMutable>] [<Table("MassTableParams")>]
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
        and [<CLIMutable>] [<Table("IonTypeParams")>]
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
        and [<CLIMutable>] [<Table("SpecificityRuleParams")>]
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
        and [<CLIMutable>] [<Table("SearchModificationParams")>]
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
        and [<CLIMutable>] [<Table("EnzymeNameParams")>]
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
        and [<CLIMutable>] [<Table("IncludeParams")>]
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
        and [<CLIMutable>] [<Table("ExcludeParams")>]
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
        and [<CLIMutable>] [<Table("AdditionalSearchParams")>]
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
        and [<CLIMutable>] [<Table("FragmentToleranceParams")>]
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
        and [<CLIMutable>] [<Table("ParentToleranceParams")>]
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
        and [<CLIMutable>] [<Table("ThresholdParams")>]
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
        and [<CLIMutable>] [<Table("SearchDatabaseParams")>]
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
        and [<CLIMutable>] [<Table("DBSequenceParams")>]
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
        and [<CLIMutable>] [<Table("PeptideEvidenceParams")>]
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
        and [<CLIMutable>] [<Table("SpectrumIdentificationItemParams")>]
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
        and [<CLIMutable>] [<Table("SpectrumIdentificationResultParams")>]
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
        and [<CLIMutable>] [<Table("SpectrumIdentificationListParams")>]
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
        and [<CLIMutable>] [<Table("AnalysisParams")>]
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
        and [<CLIMutable>] [<Table("SourceFileParams")>]
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
        and [<CLIMutable>] [<Table("ProteinDetectionHypothesisParams")>]
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
        and [<CLIMutable>] [<Table("ProteinAmbiguityGroupParams")>]
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
        and [<CLIMutable>] [<Table("ProteinDetectionListParams")>]
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
        //and [<CLIMutable>] 
        //    UserParam =
        //    {
        //     [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
        //     ID         : int
        //     Name       : string
        //     Value      : string
        //     and       : string
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

        ///A person's name and contact details.
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
                TranslationTable =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID              : string
                mutable Name    : string
                mutable Details : List<TranslationTableParam>
                RowVersion      : DateTime
                }

        ///References to CV terms defining the measures about product ions to be reported in SpectrumIdentificationItem.
        and [<CLIMutable>] 
                Measure =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID           : string
                mutable Name : string
                Details      : List<MeasureParam>
                RowVersion   : DateTime
                }

        ///The specification of a single residue within the mass table.
        and [<CLIMutable>] 
                Residue =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Code       : string
                Mass       : float
                RowVersion : DateTime
                }

        ///Ambiguous residues e.g. X can be specified by the Code attribute and a set of parameters for example giving the different masses that will be used in the search.
        and [<CLIMutable>] 
                AmbiguousResidue =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Code       : string
                Details    : List<AmbiguousResidueParam>
                RowVersion : DateTime
                }

        ///The masses of residues used in the search.
        and [<CLIMutable>] 
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

        ///The values of this particular measure, corresponding to the index defined in ion and.
        and [<CLIMutable>] 
                Value =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Value      : float
                RowVersion : DateTime
                }

        ///An array of values for a given and of measure and for a particular ion and, in parallel to the index of ions identified.
        and [<CLIMutable>] 
                FragmentArray =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Measure    : Measure
                Values     : List<Value>
                RowVersion : DateTime
                }

        ///The index of ions identified as integers, following standard notation for a-c, x-z e.g. if b3 b5 and b6 have been identified, the index would store "3 5 6".
        and [<CLIMutable>] 
                Index =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Index      : int
                RowVersion : DateTime
                }

        ///Iontype defines the index of fragmentation ions being reported, importing a CV term for the and of ion e.g. b ion. Example: if b3 b7 b8 and b10 have been identified, the index attribute will contain 3 7 8 10.
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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
        and [<CLIMutable>]
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
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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

        ///Filters applied to the search database. The filter MUST include at least one of Include and Exclude.
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
                Frame =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID         : string
                Frame      : int
                RowVersion : DateTime
                }

        ///The parameters and settings of a SpectrumIdentification analysis.
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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


        ///An Analysis which tries to identify peptides in input spectra, referencing the database searched, the input spectra, the output results and the protocol that is run.
        and [<CLIMutable>] 
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

        ///The parameters and settings of a SpectrumIdentification analysis.
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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

        ///The inputs to the analyses including the databases searched, the spectral data and the source file converted to mzIdentML.
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
                PeptideHypothesis =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                          : string
                PeptideEvidence             : PeptideEvidence
                SpectrumIdentificationItems : List<SpectrumIdentificationItem>
                RowVersion                  : DateTime
                }

        ///A single result of the ProteinDetection analysis (i.e. a protein).
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
                ProteinDetectionList =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                             : string
                mutable Name                   : string
                mutable ProteinAmbiguityGroups : List<ProteinAmbiguityGroup>
                mutable Details                : List<ProteinDetectionListParam>
                RowVersion                     : DateTime
                }

        ///Data sets generated by the analyses, including peptide and protein lists.
        and [<CLIMutable>] 
                AnalysisData =
                {
                //[<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
                ID                           : string
                SpectrumIdentificationList   : List<SpectrumIdentificationList>
                mutable ProteinDetectionList : ProteinDetectionList
                RowVersion                   : DateTime
                }

        ///An Analysis which assembles a set of peptides (e.g. from a spectra search analysis) to proteins. 
        and [<CLIMutable>] 
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
        and [<CLIMutable>] 
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

        ///The Provider of the mzIdentML record in terms of the contact and software.
        and [<CLIMutable>] 
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

        and [<AllowNullLiteral>]
            MzIdentML(
                      id:int, version:string, ontologies:List<Ontology>, spectrumIdentification:List<SpectrumIdentification>, 
                      spectrumIdentificationProtocol:List<SpectrumIdentificationProtocol>, inputs:Inputs, analysisData:AnalysisData, rowVersion:DateTime,
                      name:string, analysisSoftwares:List<AnalysisSoftware>, provider:Provider, persons:List<Person>, 
                      organizations:List<Organization>, samples:List<Sample>, dbSequences:List<DBSequence>, peptides:List<Peptide>,
                      peptideEvidences:List<PeptideEvidence>, proteinDetection:ProteinDetection, proteinDetectionProtocol:ProteinDetectionProtocol,
                      biblioGraphicReferences:List<BiblioGraphicReference>) =
                let mutable id' = id
                let mutable name' = name
                let mutable version' = version
                let mutable ontologies' = ontologies
                let mutable analysisSoftwares' = analysisSoftwares
                let mutable provider' = provider
                let mutable persons' = persons
                let mutable organizations' = organizations
                let mutable samples' = samples
                let mutable dbSequences' = dbSequences
                let mutable peptides' = peptides
                let mutable peptideEvidences' = peptideEvidences
                let mutable spectrumIdentification' = spectrumIdentification
                let mutable proteinDetection' = proteinDetection
                let mutable spectrumIdentificationProtocol' = spectrumIdentificationProtocol
                let mutable proteinDetectionProtocol' = proteinDetectionProtocol
                let mutable inputs' = inputs
                let mutable analysisData' = analysisData
                let mutable biblioGraphicReferences' = biblioGraphicReferences
                let mutable rowVersion' = rowVersion

                new() = MzIdentML()

                member this.ID with get() = id' and set(value) = id' <- value
                member this.Name with get() = name' and set(value) = name' <- value
                member this.Version with get() = version' and set(value) = version' <- value
                //Formerly CVList
                member this.Ontologies with get() = ontologies' and set(value) = ontologies' <- value
                //
                //Formerly AnalysisSoftwareList
                member this. AnalysisSoftwares with get() = analysisSoftwares' and set(value) = analysisSoftwares' <- value
                //
                member this.Provider with get() = provider' and set(value) = provider' <- value
                //Formerly AuditCollection
                member this. Persons with get() = persons and set(value) = persons' <- value
                member this. Organizations with get() = organizations' and set(value) = organizations' <- value
                //
                //Formerly AnalysisSampleCollection
                member this. Samples with get() = samples' and set(value) = samples' <- value
                //
                //Formerly SequenceCollection
                member this. DBSequences with get() = dbSequences' and set(value) = dbSequences' <- value
                member this. Peptides with get() = peptides' and set(value) = peptides' <- value
                member this. PeptideEvidences with get() = peptideEvidences' and set(value) = peptideEvidences' <- value
                //
                //AnalysisCollection
                member this.SpectrumIdentification with get() = spectrumIdentification' and set(value) = spectrumIdentification' <- value
                member this.ProteinDetection with get() = proteinDetection' and set(value) = proteinDetection' <- value
                //
                //AnalysisProtocolCollection
                member this.SpectrumIdentificationProtocol with get() = spectrumIdentificationProtocol' and set(value) = spectrumIdentificationProtocol' <- value
                member this.ProteinDetectionProtocol with get() = proteinDetectionProtocol' and set(value) = proteinDetectionProtocol' <- value
                //
                //DataCollection
                member this.Inputs with get() = inputs' and set(value) = inputs' <- value
                member this.AnalysisData with get() = analysisData' and set(value) = analysisData' <- value
                //
                member this.BiblioGraphicReferences with get() = biblioGraphicReferences' and set(value) = biblioGraphicReferences' <- value
                member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

        /////The upper-most hierarchy level of mzIdentML with sub-containers for example describing software, protocols and search results (spectrum identifications or protein detection results). 
        //and [<CLIMutable>] 
        //        MzIdentML =
        //        {
        //        ID                               : int
        //        mutable Name                     : string
        //        Version                          : string
        //        //Formerly CVList
        //        Ontologies                       : List<Ontology>
        //        //
        //        //Formerly AnalysisSoftwareList
        //        mutable AnalysisSoftwares        : List<AnalysisSoftware>
        //        //
        //        mutable Provider                 : Provider
        //        //Formerly AuditCollection
        //        mutable Persons                  : List<Person>
        //        mutable Organizations            : List<Organization>
        //        //
        //        //Formerly AnalysisSampleCollection
        //        mutable Samples                  : List<Sample>
        //        //
        //        //Formerly SequenceCollection
        //        mutable DBSequences              : List<DBSequence>
        //        mutable Peptides                 : List<Peptide>
        //        mutable PeptideEvidences         : List<PeptideEvidence>
        //        //
        //        //AnalysisCollection
        //        SpectrumIdentification           : List<SpectrumIdentification>
        //        mutable ProteinDetection         : ProteinDetection
        //        //
        //        //AnalysisProtocolCollection
        //        SpectrumIdentificationProtocol   : List<SpectrumIdentificationProtocol>
        //        mutable ProteinDetectionProtocol : ProteinDetectionProtocol
        //        //
        //        //DataCollection
        //        Inputs                           : Inputs
        //        AnalysisData                     : AnalysisData
        //        //
        //        mutable BiblioGraphicReferences  : List<BiblioGraphicReference> 
        //        RowVersion                       : DateTime
        //        }

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

                [<DefaultValue>] 
                val mutable m_OrganizationParam : DbSet<OrganizationParam>
                member public this.OrganizationParam with get() = this.m_OrganizationParam
                                                                  and set value = this.m_OrganizationParam <- value

                [<DefaultValue>] 
                val mutable m_PersonParam : DbSet<PersonParam>
                member public this.PersonParam with get() = this.m_PersonParam
                                                            and set value = this.m_PersonParam <- value

                [<DefaultValue>] 
                val mutable m_SampleParam : DbSet<SampleParam>
                member public this.SampleParam with get() = this.m_SampleParam
                                                            and set value = this.m_SampleParam <- value

                [<DefaultValue>] 
                val mutable m_ModificationParam : DbSet<ModificationParam>
                member public this.ModificationParam with get() = this.m_ModificationParam
                                                                  and set value = this.m_ModificationParam <- value

                [<DefaultValue>] 
                val mutable m_PeptideParam : DbSet<PeptideParam>
                member public this.PeptideParam with get() = this.m_PeptideParam
                                                             and set value = this.m_PeptideParam <- value

                [<DefaultValue>] 
                val mutable m_TranslationTableParam : DbSet<TranslationTableParam>
                member public this.TranslationTableParam with get() = this.m_TranslationTableParam
                                                                      and set value = this.m_TranslationTableParam <- value

                [<DefaultValue>] 
                val mutable m_MeasureParam : DbSet<MeasureParam>
                member public this.MeasureParam with get() = this.m_MeasureParam
                                                             and set value = this.m_MeasureParam <- value

                [<DefaultValue>] 
                val mutable m_AmbiguousResidueParam : DbSet<AmbiguousResidueParam>
                member public this.AmbiguousResidueParam with get() = this.m_AmbiguousResidueParam
                                                                      and set value = this.m_AmbiguousResidueParam <- value

                [<DefaultValue>] 
                val mutable m_MassTableParam : DbSet<MassTableParam>
                member public this.MassTableParam with get() = this.m_MassTableParam
                                                               and set value = this.m_MassTableParam <- value

                [<DefaultValue>] 
                val mutable m_IonTypeParam : DbSet<IonTypeParam>
                member public this.IonTypeParam with get() = this.m_IonTypeParam
                                                             and set value = this.m_IonTypeParam <- value

                [<DefaultValue>] 
                val mutable m_SpecificityRuleParam : DbSet<SpecificityRuleParam>
                member public this.SpecificityRuleParam with get() = this.m_SpecificityRuleParam
                                                                     and set value = this.m_SpecificityRuleParam <- value

                [<DefaultValue>] 
                val mutable m_SearchModificationParam : DbSet<SearchModificationParam>
                member public this.SearchModificationParam with get() = this.m_SearchModificationParam
                                                                        and set value = this.m_SearchModificationParam <- value

                [<DefaultValue>] 
                val mutable m_EnzymeNameParam : DbSet<EnzymeNameParam>
                member public this.EnzymeNameParam with get() = this.m_EnzymeNameParam
                                                                and set value = this.m_EnzymeNameParam <- value

                [<DefaultValue>] 
                val mutable m_IncludeParam : DbSet<IncludeParam>
                member public this.IncludeParam with get() = this.m_IncludeParam
                                                             and set value = this.m_IncludeParam <- value

                [<DefaultValue>] 
                val mutable m_ExcludeParam : DbSet<ExcludeParam>
                member public this.ExcludeParam with get() = this.m_ExcludeParam
                                                             and set value = this.m_ExcludeParam <- value

                [<DefaultValue>] 
                val mutable m_AdditionalSearchParam : DbSet<AdditionalSearchParam>
                member public this.AdditionalSearchParam with get() = this.m_AdditionalSearchParam
                                                                      and set value = this.m_AdditionalSearchParam <- value

                [<DefaultValue>] 
                val mutable m_FragmentToleranceParam : DbSet<FragmentToleranceParam>
                member public this.FragmentToleranceParam with get() = this.m_FragmentToleranceParam
                                                                       and set value = this.m_FragmentToleranceParam <- value

                [<DefaultValue>] 
                val mutable m_ParentToleranceParam : DbSet<ParentToleranceParam>
                member public this.ParentToleranceParam with get() = this.m_ParentToleranceParam
                                                                     and set value = this.m_ParentToleranceParam <- value

                [<DefaultValue>] 
                val mutable m_ThresholdParam : DbSet<ThresholdParam>
                member public this.ThresholdParam with get() = this.m_ThresholdParam
                                                               and set value = this.m_ThresholdParam <- value

                [<DefaultValue>] 
                val mutable m_SearchDatabaseParam : DbSet<SearchDatabaseParam>
                member public this.SearchDatabaseParam with get() = this.m_SearchDatabaseParam
                                                                    and set value = this.m_SearchDatabaseParam <- value

                [<DefaultValue>] 
                val mutable m_DBSequenceParam : DbSet<DBSequenceParam>
                member public this.DBSequenceParam with get() = this.m_DBSequenceParam
                                                                and set value = this.m_DBSequenceParam <- value

                [<DefaultValue>] 
                val mutable m_PeptideEvidenceParam : DbSet<PeptideEvidenceParam>
                member public this.PeptideEvidenceParam with get() = this.m_PeptideEvidenceParam
                                                                     and set value = this.m_PeptideEvidenceParam <- value

                [<DefaultValue>] 
                val mutable m_SpectrumIdentificationItemParam : DbSet<SpectrumIdentificationItemParam>
                member public this.SpectrumIdentificationItemParam with get() = this.m_SpectrumIdentificationItemParam
                                                                                and set value = this.m_SpectrumIdentificationItemParam <- value

                [<DefaultValue>] 
                val mutable m_SpectrumIdentificationResultParam : DbSet<SpectrumIdentificationResultParam>
                member public this.SpectrumIdentificationResultParam with get() = this.m_SpectrumIdentificationResultParam
                                                                                  and set value = this.m_SpectrumIdentificationResultParam <- value

                [<DefaultValue>] 
                val mutable m_SpectrumIdentificationListParam : DbSet<SpectrumIdentificationListParam>
                member public this.SpectrumIdentificationListParam with get() = this.m_SpectrumIdentificationListParam
                                                                                and set value = this.m_SpectrumIdentificationListParam <- value

                [<DefaultValue>] 
                val mutable m_AnalysisParam : DbSet<AnalysisParam>
                member public this.AnalysisParam with get() = this.m_AnalysisParam
                                                              and set value = this.m_AnalysisParam <- value

                [<DefaultValue>] 
                val mutable m_SourceFileParam : DbSet<SourceFileParam>
                member public this.SourceFileParam with get() = this.m_SourceFileParam
                                                                and set value = this.m_SourceFileParam <- value

                [<DefaultValue>] 
                val mutable m_ProteinDetectionHypothesisParam : DbSet<ProteinDetectionHypothesisParam>
                member public this.ProteinDetectionHypothesisParam with get() = this.m_ProteinDetectionHypothesisParam
                                                                                and set value = this.m_ProteinDetectionHypothesisParam <- value

                [<DefaultValue>] 
                val mutable m_ProteinAmbiguityGroupParam : DbSet<ProteinAmbiguityGroupParam>
                member public this.ProteinAmbiguityGroupParam with get() = this.m_ProteinAmbiguityGroupParam
                                                                           and set value = this.m_ProteinAmbiguityGroupParam <- value

                [<DefaultValue>] 
                val mutable m_ProteinDetectionListParam : DbSet<ProteinDetectionListParam>
                member public this.ProteinDetectionListParam with get() = this.m_ProteinDetectionListParam
                                                                          and set value = this.m_ProteinDetectionListParam <- value

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

