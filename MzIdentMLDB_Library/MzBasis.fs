namespace MzBasis

open System
open System.Collections.Generic
open System.ComponentModel.DataAnnotations.Schema
open System.Reflection


module Basetypes =
    
    type DBType =
    | MSSQL
    | SQLite

    type [<AllowNullLiteral>]
        Term (id:string, name:string, ontology:Ontology, fkOntology:string, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id
            let mutable name'       = name
            let mutable ontology'   = ontology
            let mutable fkOntology' = fkOntology
            let mutable rowVersion' = rowVersion

            new() = Term(null, null, null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            member this.Name with get() = name' and set(value) = name' <- value
            member this.Ontology with get() = ontology' and set(value) = ontology' <- value
            member this.FKOntology with get() = fkOntology' and set(value) = fkOntology' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value
        
    ///Standarized vocabulary for MS-Database.
    and [<AllowNullLiteral>]
        Ontology (id:string, terms:List<Term>, rowVersion:Nullable<DateTime>) =
            let mutable id'         = id    
            let mutable terms'      = terms
            let mutable rowVersion' = rowVersion

            new() = Ontology(null, null, Nullable())

            member this.ID with get() = id' and set(value) = id' <- value
            [<ForeignKey("FKOntology")>]
            member this.Terms with get() = terms' and set(value) = terms' <- value
            member this.RowVersion with get() = rowVersion' and set(value) = rowVersion' <- value

    ///Abstract member for params, in order to enable working without additional functions.
    type [<AllowNullLiteral>]
        CVParamBase =
        abstract member ID         : string
        abstract member Value      : string
        abstract member Term       : Term
        abstract member FKTerm     : string
        abstract member Unit       : Term
        abstract member FKUnit     : string
        abstract member RowVersion : Nullable<DateTime>

    type TermSymbol =
        | Accession of string

        //Else
        | RawFile
        ///Length of sequence (MaxQuant).
        | SequenceLength
        ///The length of all sequences of the proteins contained in the group.
        | SequenceLengths
        ///Posterior Error Probability of the identification. Smaller is more significant.
        | PosteriorErrorProbability
        ///Percentage of the sequence that is covered by the identified
        ///peptides of the best protein sequence contained in the group.
        | SequenceCoverage
        ///Percentage of the sequence that is covered by the identified unique and razor peptides 
        ///of the best protein sequence contained in the group.
        | UniqueAndRazorSequenceCoverage
        ///Percentage of the sequence that is covered by the identified unique peptides 
        ///of the best protein sequence contained in the group.
        | UniqueSequenceCoverage
        ///Molecular weight of the leading protein sequence contained in the protein group.
        | MolecularWeight
        ///It is a quantitative proteomics software package designed for analyzing large-scale mass-spectrometric data sets.
        | MaxQuant
        ///The fraction of intensity in the MS/MS spectrum that is annotated.
        | IntensityCoverage
        ///The fraction of peaks in the MS/MS spectrum that are annotated.
        | PeakCoverage
        ///Local false discovery rate for peptide spectrum machtes.
        | PSMFDR
        ///False discovery rate for protein.
        | ProteinFDR
        ///False discovery rate for site.
        | SideFDR
        ///
        | UseNormRatios
        ///
        |UseDeltaScores
        ///Popular MS1 method, where a protein's total intensity is divided by the number of tryptic peptides 
        ///between 6 and 30 amino acids in length
        | IBAQ
        ///
        | IBAQLogFit
        ///Heavy N15 is induced to the growth media of one culture in order to distinguish 
        ///between heavy and light labeled peptides/proteins.
        | MetabolicLabelingN14N15
        ///The weight difference between light and heavy labeled peptides is used for quantification.
        | MetabolicLabelingN14N15Quantification
        ///
        | DataDependentAcquisition
        ///
        | DataIndependentAcquisition
        ///
        | PeptideRatio
        ///
        | MassDeviation
        ///The NCBI taxonomy id for the species.
        | TaxidNCBI
        ///Describes the version of the database.
        | DatabaseVersion

        //AminoAcids
        ///Sequence of amino acids.
        | AminoAcidSequence
        ///Mutation of an AminoAcid.
        | AmnoAcidModification
        ///
        | SpecialAAs

        //Proteins
        ///The identifier of the best scoring protein, from the
        ///ProteinGroups file this, this peptide is associated to.
        | LeadingRazorProtein
        ///The identifiers of the proteins in the proteinGroups file, with this
        ///Protein as best match, this particular peptide is associated with.
        | LeadingProtein
        ///Description of the protein.
        | ProteinDescription
        ///Number of proteins contained within the group.
        | NumberOfProteins
        ///This is the ratio of reverse to forward protein groups.
        | QValue
        ///True or FALSe of contaminants are included or not
        | Contaminants
        ///Determines which kind of peptide is used to identify the protein.
        | PeptidesForProteinQuantification
        ///
        | RazorProtein
        ///
        | UniqueProtein
        ///Modifications of the Protein.
        | ProteinModifications
        ///Other members of the protein-ambiguity-group
        | ProteinAmbiguityGroupMembers

        //Peptides
        ///Peptide is unique to a single protein group in the proteinGroups file.
        | UniqueToGroups
        ///Peptide is unique to a single protein sequence in the fasta file(s).
        | UniqueToProtein
        ///Sequence window from -8 to 8 around the N-terminal cleavagesite of this peptide.
        | NTermCleavageWindow
        ///Sequence window from -8 to 8 around the C-terminal cleavagesite of this peptide.
        | CTermCleavageWindow
        ///IDs of those proteins that have at least half of the peptides that the leading protein has
        | MajorProteinIDs
        ///Number of peptides associated with each protein in proteingroup.
        | PeptideCountsAll
        ///Number of razor and unique peptides associated with each protein in proteingroup.
        | PeptideCountsRazorAndUnique
        ///Number of unique peptides associated with each protein in proteingroup.
        | PeptideCountUnique
        ///The total number of peptide sequences associated with the protein group (i.e. for all the proteins in the group).
        | Peptides
        ///The total number of razor + unique peptides associated with the protein group (i.e. these peptides are shared with another protein group).
        | RazorAndUniquePeptides
        ///The total number of unique peptides associated with the protein group (i.e. these peptides are shared with another protein group).
        | UniquePeptides
        ///The mass of the neutral peptide ((m/z-proton) * charge).
        | Mass
        ///The score of the identification (higher is better).
        | PeptideScore
        ///The percentage the parent ion intensity makes up of the total
        ///intensity of the whole MS spectrum.
        | FractionOfTotalSpectrum
        ///Number of possible distributions of the modifications over the peptide sequence.
        | Combinatorics
        ///
        | DiscardUnmodifiedPeptide
        ///
        | MinRatioCount
        ///
        | NumberPeptideSeqsMatchedEachSpec
        ///Number of different peptide-sequences.
        | DistinctPeptideSequences
        ///Peptide can be found in several proteinsequences
        | PeptideSharedWithinMultipleProteins
        ///
        | RazorPeptide
        ///
        | UniquePeptide
        ///
        | LeadingPeptide

        //Nucleic Acids
        ///Seuqence of nucleic acids.
        | NucleicAcidSequence
        ///Mutation of a nucleic acid base.
        | NucleicAcidModification

        //MassSpectometry
        ///MonoIsotopicMass of the peptide.
        | MonoIsotopicMass
        ///Indicates the way this experiment was identified.
        | IdentificationType
        ///Area under curve for one XIC.
        | XICArea
        ///Normalized area under curve  for one XIC.
        | NormalizedXICArea
        ///Intensity values of the isotopes.
        | XICAreas
        ///Total sum of XIC areas associated with the AA-sequence.
        | TotalXIC
        ///The number of MS/MS spectra recorded for the peptide.
        | MSMSCount
        ///Charge corrected mass of the precursor ion.
        | MassPrecursorIon
        ///Retention time of the peptide.
        | RetentionTime
        ///Calibrated retention time.
        | CalibratedRetentionTime
        ///The RAW-file derived scan number of the MS/MS with the highest peptide identification score.
        | MSMSScanNumber
        ///Mass error of the recalibrated mass-over-charge value of the precursor ion 
        ///in comparison to the predicted monoisotopic mass of the identified peptide sequence.
        | MassError
        ///Protein score which is derived from peptide posterior error probabilities.
        | ProteinScore
        ///The type of detection for the peptide. 
        ///MULTI – A labeling multiplet was detected.
        ///ISO – An isotope pattern was detected
        | DetectionType
        ///m/z before recalibrations have been applied.
        | UncalibratedMZ
        ///The resolution of the peak detected for the peptide measured in Full Width at Half Maximum (FWHM).
        | FWHM
        ///The number of data points (peak centroids) collected for this peptide feature.
        | NumberOfDataPoints
        ///The number of MS scans that the 3d peaks of this peptide feature are overlapping with.
        | ScanNumber
        ///The number of isotopic peaks contained in this peptide feature.
        | NumberOfIsotopicPeaks
        ///Indicates the fraction the target peak makes up of the total intensity in the inclusion window (Parent Ion Fraction)
        | PIF
        ///Empirically derived deviation measure to the next nearest integer scaled to center around 0. 
        ///Can be used to visually detect contaminants in a plot setting Mass against this value.
        ///m*a+b – round(m*a+b)
        ///m: the peptide mass
        ///a: 0.999555
        ///b: -0.10
        | MassDefect
        ///The precision of the mass detection of the peptide in parts-per-million.
        | MassPrecision
        ///The total retention time width of the peak (last timepoint – first timepoint) in seconds
        | RetentionLength
        ///The full width at half maximum value retention time width of the peak in seconds.
        | RetentionTimeFWHM
        ///The first scan-number at which the peak was encountered.
        | MinScanNumber
        ///The last scan-number at which the peak was encountered.
        | MaxScanNumber
        ///The intensity values of the isotopes.
        | Intensities
        ///The scan numbers where the MS/MS spectra were recorded.
        | MSMSScanNumbers
        ///Indices of the isotopic peaks that the MS/MS spectra reside on.
        ///A value of 0 corresponds to the monoisotopic peak.
        | MSMSIsotopIndices
        ///Multiple peak list nativeID format.
        | MultipleIDs
        ///The ion inject time for the MS/MS scan.
        | IonInjectionTime
        ///The total ion current of the MS/MS scan.
        | TotalIonCurrent
        ///Tandem mass spectrometry involves several steps in identifing the target peptides.
        | MSMSSearch
        ///The collision energy used for the fragmentation that resulted in this MS/MS scan.
        | CollisionEnergy
        ///The intensity of the most intense ion in the spectrum.
        | BasePeakIntension
        ///The time the MS/MS scan took to complete.
        | ElapsedTime
        ///Number of peaks after the 'top X per 100 Da' filtering.
        | FilteredPeaks
        ///The type of fragmentation used to create the MS/MS spectrum.
        ///CID – Collision Induced Dissociation.
        ///HCD – High energy Collision induced Dissociation.
        ///ETD – Electron Transfer Dissociation.
        | FragmentationType
        ///The mass analyzer used to record the MS/MS spectrum. ITMS
        ///     Ion trap.
        ///FTMS Fourier transform ICR or orbitrap cell.
        ///TOF  Time of flight.
        | MassAnalyzerType
        ///The percentage the parent ion intensity makes up of the total
        ///intensity in the selection window
        | ParentIntensityFraction
        ///The percentage the parent ion intensity in comparison to the
        ///highest peak in he MS spectrum
        | BasePeakFraction
        ///The full scan number where the precursor ion was selected for fragmentation.
        | PrecursorFullScanNumbers
        ///The intensity of the precursor ion at the scannumber it was selected.
        | PrecursorIntensity
        ///The fraction the intensity of the precursor ion makes up of the peak (apex) intensity.
        | PrecursorApexFraction
        ///How many full scans the precursor ion is offset from the peak (apex) position
        | PrecursorApexOffset
        ///How much time the precursor ion is offset from the peak (apex) position.
        | PrecursorApexOffsetTime
        ///This number indicates which MS/MS scan this one is in the
        ///consecutive order of the MS/MS scans that are acquired after an MS scan.
        | ScanEventNumber
        ///Score difference to the second best positioning of modifications
        ///identified peptide with the same amino acid sequence.
        | ScoreDifference
        ///The species of the peaks in the fragmentation spectrum after TopN filtering
        | Matches
        ///The species of the peaks in the fragmentation spectrum after TopN filtering.
        | ProductIonIntensity
        ///The number of peaks matching to the predicted fragmentation spectrum.
        | NumberOfMatches
        ///How many neutral losses were applied to each fragment in the Andromeda scoring.
        | NeutralIonLoss
        ///For ETD spectra several different combinations of ion series
        ///are scored. Here the highest scoring combination is indicated.
        | ETDIdentificationType
        ///Min score for unmodified peptides to be identified.
        | MinScoreUnmodifiedPeptides
        ///Min score for modified peptides to be identified.
        | MinScoreModifiedPeptides
        ///Minimal length of peptide to be identified.
        | MinPeptideLength
        ///Minimal DeltaScore of unmodified peptide to be identified.
        | MinDeltaScoreUnmod
        ///Minimal DeltaScore of modified peptide to be identified.
        | MinDeltaScoreMod
        ///Minimal amount of unique peptides to be identified.
        | MinPepUnique
        ///Minimal amount of razor peptides to be identified.
        | MinPepRazor
        ///Minimal amount of peptides to be identified.
        | MinPep
        ///
        | DecoyMode
        ///The error window on experimental MS/MS fragment ion mass values.
        | MSMSTolerance
        ///Number of peaks after the 'top X per 100 Da' filtering.
        | TopPeakPer100Da
        ///
        | MSMSDeisotoping
        ///Mass error of the recalibrated mass-over-charge value of the
        ///precursor ion in comparison to the predicted monoisotopic
        ///mass of the identified peptide sequence in parts per million.
        | MassErrorPpm
        ///Mass error of the recalibrated mass-over-charge value of the
        ///precursor ion in comparison to the predicted monoisotopic
        ///mass of the identified peptide sequence in percent.
        | MassErrorPercent
        ///Mass error of the recalibrated mass-over-charge value of the
        ///precursor ion in comparison to the predicted monoisotopic
        ///mass of the identified peptide sequence in dalton.
        | MassErrorDa
        ///Upper limit of the search tolerance.
        | SearchToleranceUpperLimit
        ///Lower limit of the search tolerance.
        | SearchToleranceLowerLimit
        ///
        | ProductIonMZ
        ///
        | ProductIonErrorMZ
        ///How many neutral losses were applied to each fragment in the Andromeda scoring.
        | NeutralFragmentLoss
        ///
        | MatchBetweenRuns
        ///
        | MS1LabelBasedAnalysis
        ///
        | MS1LabelBasedPeptideAnalysis
        ///
        | MS1LabelBasedProteinAnalysis
        ///
        | SpectralCountQuantification
        ///
        | MzDeltaScore

        //Ion fragment types
        ///Kind of ion after fragmentation of peptide.
        | Fragment
        | ProductIonSeriesOrdinal
        | ProductIonMZDelta
        | ProductInterpretationRank
        | ATypeIon
        | YIon
        | YIonWithoutWater
        | YIonWithoutAmmoniumGroup
        | YIonWihtout3H3PO4
        | YIonWihtout2H3PO4
        | YIonWihtoutH3PO4
        | BIon
        | BIonWithoutWater
        | BIonWithoutAmmoniumGroup
        | BIonWihtout3H3PO4
        | BIonWihtout2H3PO4
        | BIonWihtoutH3PO4
        | XIon
        | XIonWithoutWater
        | XIonWithoutAmmoniumGroup

        //Units
        | Minute
        | Second
        | Ppm
        | Percentage
        | Dalton
        | KiloDalton
        | NumberOfDetections
        | MZ

        //File
        ///Information coded in FASTA format.
        | FASTAFormat
        ///Values are seperated by tab(s).
        | TSV
        ///Unspecific kind of database.
        | DataStoredInDataBase
        ///Uknown file type.
        | UnknownFileType
        
        //DataBases
        ///UniProt.
        | UniProt
        | DataBaseName

        //Taxonomie
        ///ScientificName of the species.
        | ScientificName

        //EnzymeNames
        ///A serineProtease which cuts digests after K, R and modified C.
        | Trypsin
        ///No enzyme was used.
        | NoEnzyme

        //Type of modification of NA or AA.
        ///An oxidaton.
        | Oxidation
        
        //Type of scoring by searchEnginges
        ///Andromeda score for the associated MS/MS spectra.
        | AndromedaScore
        ///Best Andromeda score for the associated MS/MS spectra.
        | BestAndromedaScore
        ///
        | ProteomicDiscovererDeltaScore
        ///Score difference to the second best identified peptide.
        | DeltaScore

        static member toID (item:TermSymbol) =
            match item with
            | Accession s -> s
            | RawFile -> "MS:1000577"
            | AminoAcidSequence -> "MS:1001344"
            | NucleicAcidSequence -> "MS:1001343"
            | SequenceLength -> "MS:1001900"
            | NTermCleavageWindow -> "User:0000000"
            | CTermCleavageWindow -> "User:0000001"
            | MonoIsotopicMass -> "MS:1000225"
            | LeadingRazorProtein -> "User:0000002"
            | UniqueToGroups -> "User:0000003"
            | UniqueToProtein -> "MS:1001363"
            | PosteriorErrorProbability -> "User:0000005"
            | AndromedaScore -> "MS:1002338"
            | IdentificationType -> "User:0000006"
            | XICArea -> "MS:1001858"
            | NormalizedXICArea -> "MS:1001859"
            | TotalXIC -> "MS:1002412"
            | MSMSCount -> "MS:1001904"
            | NucleicAcidModification -> "MS:1002028"
            | AmnoAcidModification -> "User:0000007"
            | MassPrecursorIon -> "User:0000008"
            | RetentionTime -> "MS:1000894"
            | Minute -> "MS:1000038"
            | Second -> "MS:1000039"
            | CalibratedRetentionTime -> "User:0000009"
            | MSMSScanNumber -> "MS:1001115"
            | ProteomicDiscovererDeltaScore -> "MS:1002834"
            | LeadingProtein -> "MS:1002401"
            | ProteinDescription -> "MS:1001088"
            | MassError -> "User:0000010"
            | Ppm -> "UO:0000169"
            | MajorProteinIDs -> "User:0000012"
            | PeptideCountsAll -> "MS:1001898"
            | PeptideCountsRazorAndUnique -> "MS:1001899"
            | PeptideCountUnique -> "MS:1001897"
            | NumberOfProteins -> "User:0000013"
            | Peptides -> "User:0000014"
            | RazorAndUniquePeptides -> "User:0000015"
            | UniquePeptides -> "User:0000016"
            | SequenceCoverage -> "MS:1001093"
            | Percentage -> "UO:0000187"
            | UniqueAndRazorSequenceCoverage -> "User:0000017"
            | UniqueSequenceCoverage -> "User:0000018"
            | MolecularWeight -> "PRIDE:0000057"
            | SequenceLengths -> "User:0000019"
            | QValue -> "MS:1001491"
            | ProteinScore -> "MS:1002394"
            | FASTAFormat -> "MS:1001348"
            | DetectionType -> "User:0000021"
            | Mass -> "MS:1000224"
            | UncalibratedMZ -> "User:0000022"
            | FWHM -> "MS:1000086"
            | NumberOfDataPoints -> "User:0000023"
            | ScanNumber -> "MS:1001115"
            | NumberOfIsotopicPeaks -> "User:0000024"
            | PIF -> "User:0000025"
            | MassDefect -> "MS:1000222"
            | MassPrecision -> "User:0000026"
            | RetentionLength -> "MS:1000826"
            | RetentionTimeFWHM -> "User:0000027"
            | MaxScanNumber -> "User:0000028"
            | MinScanNumber -> "User:0000029"
            | PeptideScore -> "MS:1002221"
            | Intensities -> "MS:1001846"
            | MSMSScanNumbers -> "User:0000030"
            | MSMSIsotopIndices -> "User:0000031"
            | MultipleIDs -> "MS:1000774"
            | IonInjectionTime -> "MS:1000927"
            | TotalIonCurrent -> "MS:1000285"
            | MaxQuant -> "MS:1001583"
            | MSMSSearch -> "MS:1001083"
            | CollisionEnergy -> "MS:1000045"
            | BasePeakIntension -> "MS:1000505"
            | ElapsedTime -> "MS:1000747"
            | FilteredPeaks -> "User:0000032"
            | FragmentationType -> "User:0000033"
            | MassAnalyzerType -> "MS:1000443"
            | ParentIntensityFraction -> "User:0000034"
            | FractionOfTotalSpectrum -> "User:0000035"
            | BasePeakFraction -> "User:0000036"
            | PrecursorFullScanNumbers -> "User:0000037"
            | PrecursorIntensity -> "User:0000038"
            | PrecursorApexFraction -> "User:0000039"
            | PrecursorApexOffset -> "User:0000040"
            | PrecursorApexOffsetTime -> "User:0000041"
            | ScanEventNumber -> "User:0000042"
            | ScoreDifference -> "User:0000043"
            | Combinatorics -> "User:0000044"
            | Matches -> "User:0000045"
            | Fragment -> "MS:1002695"
            | ProductIonIntensity -> "MS:1001226"
            | Dalton -> "MS:1000212"
            | KiloDalton -> "UO:0000222"
            | NumberOfMatches -> "User:0000047"
            | IntensityCoverage -> "User:0000048"
            | PeakCoverage -> "User:0000049"
            | NeutralIonLoss -> "MS:1001061"
            | ETDIdentificationType -> "User:0000050"
            | MinScoreUnmodifiedPeptides -> "User:0000051"
            | MinScoreModifiedPeptides -> "User:0000052"
            | MinPeptideLength -> "MS:1002322"
            | MinDeltaScoreUnmod -> "User:0000053"
            | MinDeltaScoreMod -> "User:0000054"
            | MinPepUnique -> "User:0000055"
            | MinPepRazor -> "User:0000056"
            | MinPep -> "User:0000057"
            | DecoyMode -> "User:0000058"
            | SpecialAAs -> "User:0000059"
            | Contaminants -> "User:0000060"
            | MSMSTolerance -> "MS:1001655"
            | TopPeakPer100Da -> "User:0000062"
            | MSMSDeisotoping -> "MS:1000033"
            | PSMFDR -> "MS:1002351"
            | ProteinFDR -> "User:0000064"
            | SideFDR -> "User:0000065"
            | UseNormRatios -> "User:0000066"
            | PeptidesForProteinQuantification -> "User:0000067"
            | DiscardUnmodifiedPeptide -> "User:0000068"
            | MinRatioCount -> "User:0000069"
            | UseDeltaScores -> "User:0000070"
            | MassErrorPpm -> "PRIDE:0000083"
            | MassErrorPercent -> "PRIDE:0000085"
            | MassErrorDa -> "PRIDE:0000086"
            | NumberOfDetections -> "MS:1000131"
            | SearchToleranceUpperLimit -> "MS:1001412"
            | SearchToleranceLowerLimit -> "MS:1001413"
            | ProductIonMZ -> "MS:1001225"
            | ProductIonErrorMZ -> "MS:1001227"
            | MZ -> "MS:1000040"
            | TSV -> "MS:1000914"
            | DataStoredInDataBase -> "MS:1001107"
            | UnknownFileType -> "PRIDE:0000059"
            | UniProt -> "MS:1001254"
            | ScientificName -> "MS:1001469"
            | Trypsin -> "MS:1001251"
            | NoEnzyme -> "MS:1001091"
            | ATypeIon -> "UNIMOD:140"
            | BIonWihtoutH3PO4 -> "PRIDE:0000419"
            | BIonWihtout2H3PO4 -> "PRIDE:0000420"
            | BIonWihtout3H3PO4 -> "PRIDE:0000421"
            | YIonWihtoutH3PO4 -> "PRIDE:0000422"
            | YIonWihtout2H3PO4 -> "PRIDE:0000423"
            | YIonWihtout3H3PO4 -> "PRIDE:0000424"
            | ProductIonSeriesOrdinal -> "MS:1000903"
            | ProductIonMZDelta -> "MS:1000904"
            | ProductInterpretationRank -> "MS:1000926"
            | YIon -> "MS:1001220"
            | YIonWithoutWater -> "MS:1001223"
            | YIonWithoutAmmoniumGroup -> "MS:1001233"
            | BIon -> "MS:1001224"
            | BIonWithoutWater -> "MS:1001222"
            | BIonWithoutAmmoniumGroup -> "MS:1001232"
            | XIon ->"MS:1001228"
            | XIonWithoutWater -> "MS:1001519"
            | XIonWithoutAmmoniumGroup -> "MS:1001520"
            | NumberPeptideSeqsMatchedEachSpec -> "MS:1001030"
            | Oxidation -> "UNIMOD:35"
            | DistinctPeptideSequences -> "MS:1001097"
            | PeptideSharedWithinMultipleProteins -> "MS:1001175"
            | NeutralFragmentLoss -> "MS:1001524"
            | MatchBetweenRuns -> "User:0000046"
            | IBAQ ->"User:0000061"
            | IBAQLogFit -> "User:0000063"
            | MS1LabelBasedAnalysis -> "MS:1002018"
            | MS1LabelBasedPeptideAnalysis -> "MS:1002002"
            | MS1LabelBasedProteinAnalysis -> "MS:1002003"
            | SpectralCountQuantification -> "MS:1001836"
            | MetabolicLabelingN14N15Quantification -> "MS:1001839"
            | MetabolicLabelingN14N15 -> "User:0000020"
            | DataDependentAcquisition -> "User:0000071"
            | DataIndependentAcquisition -> "PRIDE:0000450"
            | XICAreas -> "User:0000011"
            | RazorProtein -> "User:0000072"
            | UniqueProtein -> "User:0000076"
            | RazorPeptide -> "User:0000073"
            | UniquePeptide -> "User:0000077"
            | LeadingPeptide -> "User:0000075"
            | PeptideRatio -> "MS:1001132"
            | MzDeltaScore -> "MS:1001975"
            | MassDeviation -> "User:0000074"
            | TaxidNCBI -> "MS:1001467"
            | DatabaseVersion -> "MS:1001016"
            | DeltaScore -> "User:0000078"
            | BestAndromedaScore -> "User:0000079"
            | DataBaseName -> "MS:1001013"
            | ProteinModifications -> "MS:1000933"
            | ProteinAmbiguityGroupMembers -> "PRIDE:0000418"

