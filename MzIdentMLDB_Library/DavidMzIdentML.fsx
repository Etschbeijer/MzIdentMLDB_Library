    
#r "System.ComponentModel.DataAnnotations.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\netstandard.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\\BioFSharp.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\\BioFSharp.IO.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\Microsoft.EntityFrameworkCore.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\Microsoft.EntityFrameworkCore.Relational.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\Microsoft.EntityFrameworkCore.Sqlite.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\Remotion.Linq.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\SQLitePCLRaw.core.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\SQLitePCLRaw.batteries_v2.dll"
#r @"..\packages\FSharp.Data.2.4.6\lib\net45\FSharp.Data.dll"
#r @"..\packages\FSharp.Data.Xsd.1.0.2\lib\net45\FSharp.Data.Xsd.dll"
#r "System.Xml.Linq.dll"
#r @"..\MzIdentMLDB_Library\bin\Debug\MzIdentMLDB_Library.dll"


open System
open System.Data
open System.Linq
open System.Collections.Generic
open FSharp.Data
open Microsoft.EntityFrameworkCore
open MzIdentMLDataBase.DataModel
open MzIdentMLDataBase.InsertStatements.ObjectHandlers

let fileDir = __SOURCE_DIRECTORY__
let standardDBPathSQLite = fileDir + "\Databases\Test3.db"

let sqliteContext = ContextHandler.sqliteConnection standardDBPathSQLite


//let fromPsiMS =
//    ContextHandler.fromFileObo (fileDir + "\Ontologies\Psi-MS.txt")

//let fromPride =
//    ContextHandler.fromFileObo (fileDir + "\Ontologies\Pride.txt")

//let fromUniMod =
//    ContextHandler.fromFileObo (fileDir + "\Ontologies\Unimod.txt")

//let fromUnit_Ontology =
//    ContextHandler.fromFileObo (fileDir + "\Ontologies\Unit_Ontology.txt")
        
 
//let initStandardDB (dbContext : MzIdentML) =

//    let termsPSIMS =
//        let ontology =  OntologyHandler.init ("PSI-MS")
//        fromPsiMS
//        |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, ontology))
//        |> Seq.iter (fun term -> ContextHandler.tryAddToContext dbContext term |> ignore) 

//    let termsPride =
//        let ontology =  OntologyHandler.init ("PRIDE")
//        fromPride
//        |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, ontology))
//        |> Seq.iter (fun term -> ContextHandler.tryAddToContext dbContext term |> ignore)

//    let termsUnimod =
//        let ontology =  OntologyHandler.init ("UNIMOD")
//        fromUniMod
//        |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, ontology))
//        |> Seq.iter (fun term -> ContextHandler.tryAddToContext dbContext term |> ignore)

//    let termsUnit_Ontology =
//        let ontology =  OntologyHandler.init ("UNIT-ONTOLOGY") 
//        fromUnit_Ontology
//        |> Seq.map (fun termItem -> TermHandler.init(termItem.Id, termItem.Name, ontology))
//        |> Seq.iter (fun term -> ContextHandler.tryAddToContext dbContext term |> ignore)

//    let userOntology =
//        OntologyHandler.init("UserParam")
//        |> ContextHandler.tryAddToContext dbContext |> ignore

//    dbContext.Database.EnsureCreated() |> ignore
//    dbContext.SaveChanges()

//initStandardDB sqliteContext



//Create new dbContent based on xml-file


let createDBEntry (dbContext:MzIdentML) =
    let mzIdentMLDocument =
        MzIdentMLDocumentHandler.init(
                                      "1.2.0",
                                      id="PeptideShaker v1.12.2"
                                     )
        |> MzIdentMLDocumentHandler.addToContext dbContext

    let organization =
        OrganizationHandler.init("ORG_DOC_OWNER")
        |> OrganizationHandler.addName("Test")
        |> OrganizationHandler.addDetail
            (OrganizationParamHandler.init(
                                           (TermHandler.tryFindByID dbContext "MS:1000586").Value
                                          )
            )
        |> OrganizationHandler.addToContext dbContext

    let person = 
        PersonHandler.init("PROVIDER")
        |> PersonHandler.addFirstName "test"
        |> PersonHandler.addLastName "test"
        |> PersonHandler.addDetail 
            (PersonParamHandler.init(
                                     (TermHandler.tryFindByID dbContext "MS:1000587").Value
                                    )
            )
        |> PersonHandler.addOrganization
            (OrganizationHandler.tryFindByID 
                dbContext
                "ORG_DOC_OWNER"
            ).Value
        |> PersonHandler.addToContext dbContext
    
    let contactRole =
        ContactRoleHandler.init(
                                (PersonHandler.tryFindByID dbContext "PROVIDER").Value,
                                CVParamHandler.init(
                                                    (TermHandler.tryFindByID dbContext "MS:1001271").Value
                                                   ),
                                "PROVIDER"
                               )
        |> ContactRoleHandler.addToContext dbContext
    
    let analysisSoftware =
        AnalysisSoftwareHandler.init(
            CVParamHandler.init(
                                (TermHandler.tryFindByID dbContext "MS:1002458").Value
                            ),
            "ID_software"
                                    )
        |> AnalysisSoftwareHandler.addName "PeptideShaker"
        |> AnalysisSoftwareHandler.addURI "http://compomics.github.io/projects/peptide-shaker.html"
        |> AnalysisSoftwareHandler.addAnalysisSoftwareDeveloper
            (ContactRoleHandler.tryFindByID dbContext "PROVIDER").Value
        |> AnalysisSoftwareHandler.addMzIdentMLDocument
            (MzIdentMLDocumentHandler.tryFindByID dbContext "PeptideShaker v1.12.2").Value
        |> AnalysisSoftwareHandler.addToContext dbContext
    
    let provider =
        ProviderHandler.init("PROVIDER")
        |> ProviderHandler.addContactRole
            (ContactRoleHandler.tryFindByID dbContext "PROVIDER").Value
        |> ProviderHandler.addMzIdentMLDocument 
            (MzIdentMLDocumentHandler.tryFindByID dbContext "PeptideShaker v1.12.2").Value
        |> ProviderHandler.addToContext dbContext

    let peptide =
        PeptideHandler.init("SIQFVDWCPTGFK", "SIQFVDWCPTGFK")
        |> PeptideHandler.addModification
            (ModificationHandler.init(
                                      [ModificationParamHandler.init(
                                                                     (TermHandler.tryFindByID 
                                                                        dbContext 
                                                                        "UNIMOD:4"
                                                                     ).Value
                                                                    )
                                      ]
                                     )
             |> ModificationHandler.addMonoIsotopicMassDelta 57.021464
             |> ModificationHandler.addLocation 8
             |> ModificationHandler.addResidues "C"
            )
        |> PeptideHandler.addToContext dbContext

    //let spectrumIdentification =
    //    SpectrumIdentificationHandler.init(
    //                                       (SpectrumIdentificationListHandler.tryFindByID dbContext "SIL_1").Value,
    //                                       (SpectrumIdentificationProtocolHandler.tryFindByID dbContext "SearchProtocol_1").Value,
    //                                       (SpectraDataHandler.tryFindByID dbContext "qExactive01819.mgf").Value,
    //                                       (SearchDatabaseHandler.tryFindByID dbContext "SearchDB_1").Value,
    //                                       "SpecIdent_1"
    //                                      )
    //    |> SpectrumIdentificationHandler.addToContext dbContext
    
    //let proteinDetection =
    //    ProteinDetectionHandler.init(
    //                                 (ProteinDetectionListHandler.tryFindByID dbContext "Protein_groups").Value,
    //                                 (ProteinDetectionProtocolHandler.tryFindByID dbContext "PeptideShaker_1").Value,
    //                                 (SpectrumIdentificationListHandler.tryFindByID dbContext "SIL_1").Value,
    //                                 "PD_1"
    //                                )
    //    |> ProteinDetectionHandler.addToContext dbContext

    let spectrumIdentificationProtocol =
        SpectrumIdentificationProtocolHandler.init(
                                                   (AnalysisSoftwareHandler.tryFindByID 
                                                        dbContext 
                                                        "ID_software"
                                                   ).Value,
                                                   (CVParamHandler.init(
                                                                        (TermHandler.tryFindByID 
                                                                            dbContext 
                                                                            "MS:1001083"
                                                                        ).Value
                                                                       )
                                                   ),
                                                   [(ThresholdParamHandler.init(
                                                                                (TermHandler.tryFindByID 
                                                                                    dbContext 
                                                                                    "MS:1001364"
                                                                                ).Value
                                                                               )
                                                   )],
                                                   "SearchProtocol_1"
                                                  )
        |> SpectrumIdentificationProtocolHandler.addAdditionalSearchParam
                (AdditionalSearchParamHandler.init(
                                                   (TermHandler.tryFindByID dbContext "MS:1001211").Value
                                                  )
                )
        |> SpectrumIdentificationProtocolHandler.addModificationParams
            [(SearchModificationHandler.init(
                                             true,
                                             57.021464,
                                             "C",
                                             [SearchModificationParamHandler.init(
                                                                                  (TermHandler.tryFindByID
                                                                                    dbContext
                                                                                    "UNIMOD:4"
                                                                                   ).Value
                                                                                  )
                                             ]
                                            )
            )]
        |> SpectrumIdentificationProtocolHandler.addEnzyme
            (EnzymeHandler.init(
                                "Enz1",
                                "Trypsin",
                                semiSpecific=false,
                                missedCleavages=2
                               )
            |> EnzymeHandler.addEnzymeName
                (EnzymeNameParamHandler.init(
                                             (TermHandler.tryFindByID dbContext "MS:1001251").Value
                                            )
                )
            )
        |> SpectrumIdentificationProtocolHandler.addIndependent_Enzymes false
        |> SpectrumIdentificationProtocolHandler.addFragmentTolerance
            (FragmentToleranceParamHandler.init(
                                                (TermHandler.tryFindByID dbContext "MS:1001412").Value
                                               )
            )
        |> SpectrumIdentificationProtocolHandler.addParentTolerance
            (ParentToleranceParamHandler.init(
                                              (TermHandler.tryFindByID dbContext "MS:1001412").Value
                                             )
            )
        |> SpectrumIdentificationProtocolHandler.addToContext dbContext

    let proteinDetectionProtocol =
        ProteinDetectionProtocolHandler.init(
            (AnalysisSoftwareHandler.tryFindByID 
            dbContext "ID_software"
            ).Value,
            [(ThresholdParamHandler.init(
                                        (TermHandler.tryFindByID
                                            dbContext "MS:1002369"
                                        ).Value
                                        )
            |> ThresholdParamHandler.addValue "0.01"
            )],
            "PeptideShaker_1"
                                            )
        |> ProteinDetectionProtocolHandler.addToContext dbContext
    
    let inputs =
        InputsHandler.init(
            [(SpectraDataHandler.init(
                                        "file:/C:/Users/hba041/My_Git_Applications/peptide-shaker.wiki/data/2016_04_05/qExactive01819.mgf",
                                        CVParamHandler.init(
                                                            (TermHandler.tryFindByID 
                                                            dbContext "MS:1001062"
                                                            ).Value
                                                        ),
                                        CVParamHandler.init(
                                                            (TermHandler.tryFindByID
                                                            dbContext "MS:1000774"
                                                            ).Value
                                                        ),
                                        "qExactive01819.mgf",
                                        "qExactive01819.mgf"
                                    )
            )]
                          )
        |> InputsHandler.addSourceFile
            (SourceFileHandler.init(
                "file:/C:/Users/hba041/My_Git_Applications/peptide-shaker.wiki/data/2016_04_05/.PeptideShaker_unzip_temp/searchgui_out_PeptideShaker_temp/qExactive01819.omx",
                CVParamHandler.init(
                                    (TermHandler.tryFindByID dbContext "MS:1001400").Value
                                    ),
                "SourceFile_1"
                                   )
            )
        |> InputsHandler.addSearchDatabase
            (SearchDatabaseHandler.init(
                "file:/C:/Users/hba041/My_Git_Applications/peptide-shaker.wiki/data/2016_04_05/uniprot-human-reviewed-trypsin-april-2016_concatenated_target_decoy.fasta",
                CVParamHandler.init(
                                    (TermHandler.tryFindByID dbContext "MS:1001348").Value
                                    ),
                CVParamHandler.init(
                                    (TermHandler.init("uniprot-human-reviewed-trypsin-april-2016_concatenated_target_decoy.fasta")
                                        |> TermHandler.addOntology
                                            (OntologyHandler.tryFindByID dbContext "UserParam").Value
                                    )
                                    ),
                "SearchDB_1"
                                       )
             |> SearchDatabaseHandler.addNumDatabaseSequences 40400L
            )
        |> InputsHandler.addMzIdentMLDocument
            (MzIdentMLDocumentHandler.tryFindByID dbContext "PeptideShaker v1.12.2").Value
        |> InputsHandler.addToContext dbContext
    
    let dbSequence =
        DBSequenceHandler.init(
                               "P27348",
                               (SearchDatabaseHandler.tryFindByID dbContext "SearchDB_1").Value,
                               "P27348"
                              )
        |> DBSequenceHandler.addDetail
            (DBSequenceParamHandler.init(
                                         (TermHandler.tryFindByID dbContext "MS:1001088").Value
                                        )
            |> DBSequenceParamHandler.addValue "1433T_HUMAN 14-3-3 protein theta OS=Homo sapiens GN=YWHAQ PE=1 SV=1"
            )
        |> DBSequenceHandler.addToContext dbContext

    let peptideEvidence =
        PeptideEvidenceHandler.init(
                                    (DBSequenceHandler.tryFindByID dbContext "P27348").Value,
                                    (PeptideHandler.tryFindByID dbContext "SIQFVDWCPTGFK").Value,
                                    "PepEv_1"
                                   )
        |> PeptideEvidenceHandler.addIsDecoy false
        |> PeptideEvidenceHandler.addPre "R"
        |> PeptideEvidenceHandler.addPost "V"
        |> PeptideEvidenceHandler.addStart 340
        |> PeptideEvidenceHandler.addEnd 352
        |> PeptideEvidenceHandler.addToContext dbContext

    let fragmentationTable =
        MeasureHandler.init(
                    [MeasureParamHandler.init(
                                              (TermHandler.tryFindByID
                                                 dbContext
                                                 "MS:1001225"
                                              ).Value
                                             )
                     |> MeasureParamHandler.addUnit
                            (TermHandler.tryFindByID
                                                 dbContext
                                                 "MS:1000040"
                                              ).Value
                    ],
                    "Measure_MZ"
                           )
        |> MeasureHandler.addToContext dbContext

    let spectrumIdentificationList =
        SpectrumIdentificationListHandler.init(
            [SpectrumIdentificationResultHandler.init(
                (SpectraDataHandler.tryFindByID dbContext "qExactive01819.mgf").Value,
                "index=11104",
                [SpectrumIdentificationItemHandler.init(
                                                        (PeptideHandler.tryFindByID
                                                            dbContext
                                                            "SIQFVDWCPTGFK"
                                                        ).Value,
                                                        3,
                                                        372.228240966797,
                                                        false,
                                                        1,
                                                        "SII_1_1"
                                                        )
                |> SpectrumIdentificationItemHandler.addPeptideEvidence
                    (PeptideEvidenceHandler.tryFindByID
                        dbContext
                        "PepEv_1"
                    ).Value
                |> SpectrumIdentificationItemHandler.addFragmentation
                    (IonTypeHandler.init(
                                            [IonTypeParamHandler.init(
                                                                    (TermHandler.tryFindByID
                                                                        dbContext
                                                                        "MS:1001224"
                                                                    ).Value
                                                                    )
                                            ]
                                        )
                    |> IonTypeHandler.addIndex (IndexHandler.init(1))
                    |> IonTypeHandler.addFragmentArray
                        (FragmentArrayHandler.init(
                                                    (MeasureHandler.tryFindByID 
                                                        dbContext
                                                        "Measure_MZ"
                                                    ).Value,
                                                    [ValueHandler.init(335.2040405)]
                                                    )
                        )
                    )
                |> SpectrumIdentificationItemHandler.addDetail
                    (SpectrumIdentificationItemParamHandler.init(
                                                                    (TermHandler.tryFindByID
                                                                    dbContext "MS:1002466"
                                                                    ).Value
                                                                )
                        |> SpectrumIdentificationItemParamHandler.addValue "0.0"
                    )

                ],
                "SIR_1"
                                                     )
            |> SpectrumIdentificationResultHandler.addDetail
                (SpectrumIdentificationResultParamHandler.init(
                                                               (TermHandler.tryFindByID
                                                                    dbContext
                                                                    "MS:1002466"
                                                               ).Value
                                                              )
                )
            ],
            "SIL_1"                                      
                                                    )
        |> SpectrumIdentificationListHandler.addFragmentationTable
                (MeasureHandler.tryFindByID dbContext "Measure_MZ").Value
        |> SpectrumIdentificationListHandler.addToContext dbContext

    let proteinDetectionList =
        ProteinDetectionListHandler.init("Protein_groups")
        |> ProteinDetectionListHandler.addProteinAmbiguityGroup
            (ProteinAmbiguityGroupHandler.init(
                [ProteinDetectionHypothesisHandler.init(
                                                        true,
                                                        (DBSequenceHandler.tryFindByID
                                                            dbContext 
                                                            "P27348"
                                                        ).Value,
                                                        [PeptideHypothesisHandler.init(
                                                            (PeptideEvidenceHandler.tryFindByID
                                                                dbContext
                                                                "PepEv_1"
                                                            ).Value,
                                                            [(SpectrumIdentificationItemHandler.tryFindByID
                                                                dbContext
                                                                "SII_1_1"
                                                             ).Value
                                                            ]
                                                                                      )
                                                        ],
                                                        "PAG_0"
                                                       )
                 |> ProteinDetectionHypothesisHandler.addDetail
                        (ProteinDetectionHypothesisParamHandler.init(
                                                                     (TermHandler.tryFindByID
                                                                        dbContext "MS:1002403"
                                                                     ).Value
                                                                    )
                        )
                ]   
                                                       )
        |> ProteinAmbiguityGroupHandler.addDetail
                        (ProteinAmbiguityGroupParamHandler.init(
                                                                (TermHandler.tryFindByID
                                                                dbContext "MS:1002470"
                                                                ).Value
                                                               )
                         |> ProteinAmbiguityGroupParamHandler.addValue "100.0"
                        )
            )
        |> ProteinDetectionListHandler.addToContext dbContext

    let analysisData =
        AnalysisDataHandler.init(
            [(SpectrumIdentificationListHandler.tryFindByID dbContext "SIL_1").Value]
                                )
        |> AnalysisDataHandler.addProteinDetectionList
            (ProteinDetectionListHandler.tryFindByID dbContext "Protein_groups").Value
        |> AnalysisDataHandler.addMzIdentMLDocument
            (MzIdentMLDocumentHandler.tryFindByID dbContext "PeptideShaker v1.12.2").Value
        |> AnalysisDataHandler.addToContext dbContext
    analysisData

let testDBContext =
    createDBEntry sqliteContext


type TermIDByName =
    | ProteinGroupLevelFDR
    | ParentMassMono
    | FragmentMassMono
    | ConsensusScoring
    | PeptideLevelScoring
    | GroupPSMsBySequenceWithModifications
    | ModificationLocalizationScoring
    | Trypsin
    | SearchTolerancePlusValue
    | SearchToleranceMinusValue
    | ModificationSpecificityProteinNterm
    | ModificationSpecificityPeptideNterm
    | PeptideSequenceLevelFDR
    | PSMLevelFDR
    | PhosphoRSScoreThreshold
    | DScoreThreshold
    | SoftwareVendor
    | PeptideShaker
    | ContactName
    | ContactAddress
    | ContactURL
    | ContactEmail
    | CountOfIdentifiedProteins
    | PeptideShakerProteinGroupScore
    | PeptideShakerProteinGroupConfidence
    | PeptideShakerProteinConfidenceType
    | ProteinGroupPassesThreshold
    | ProductIonMasstoChargeRatio
    | ProductIonIntensity
    | ProductIonMasstoChargeRatioError
    | SpectrumTitle
    | RetentionTime
    | ProteinDescription
    | Carbamidomethyl
    | Oxidation
    | NTerminalGlutamateToPyroglutamateConversion
    | NTerminalGlutamicAcidToPyroglutamateConversion
    | Acetyl
    | AmmoniaLoss
    | Reasearcher
    | MsMsSearch
    | MascotMGFformat
    | MultiplePeakListNativeIDFormat
    | OMSSAXMLFormat
    | FASTAFormat
    | Frag
    | PeptideShakerPSMScore
    | GroupRepresentative
    | MZRatio
    static member toID (item:TermIDByName) =
        match item with
        | ProteinGroupLevelFDR -> "MS:1002369"
        | ParentMassMono -> "MS:1001211"
        | FragmentMassMono -> "MS:1001256"
        | ConsensusScoring -> "MS:1002492"
        | PeptideLevelScoring -> "MS:1002490"
        | GroupPSMsBySequenceWithModifications -> "MS:1002497"
        | ModificationLocalizationScoring -> "MS:1002491"
        | Trypsin -> "MS:1001251"
        | SearchTolerancePlusValue -> "MS:1001412"
        | SearchToleranceMinusValue -> "MS:1001413"
        | ModificationSpecificityProteinNterm -> "MS:1002057"
        | ModificationSpecificityPeptideNterm -> "MS:1001189"
        | PeptideSequenceLevelFDR -> "MS:1001364"
        | PSMLevelFDR -> "MS:1002350"
        | PhosphoRSScoreThreshold -> "MS:1002567"
        | DScoreThreshold -> "MS:1002557"
        | SoftwareVendor -> "MS:1001267"
        | PeptideShaker -> "MS:1002458"
        | ContactName -> "MS:1000586"
        | ContactAddress -> "MS:1000587"
        | ContactURL -> "MS:1000588"
        | ContactEmail -> "MS:1000589"
        | CountOfIdentifiedProteins -> "MS:1002404"
        | PeptideShakerProteinGroupScore -> "MS:1002470"
        | PeptideShakerProteinGroupConfidence -> "MS:1002471"
        | PeptideShakerProteinConfidenceType -> "MS:1002542"
        | ProteinGroupPassesThreshold -> "MS:1002415"
        | ProductIonMasstoChargeRatio -> "MS:1001225"
        | ProductIonIntensity -> "MS:1001226"
        | ProductIonMasstoChargeRatioError -> "MS:1001227"
        | SpectrumTitle -> "MS:1000796"
        | RetentionTime -> "MS:1000894"
        | ProteinDescription -> "MS:1001088"
        | Carbamidomethyl -> "UNIMOD:4"
        | Oxidation -> "UNIMOD:35"
        | NTerminalGlutamateToPyroglutamateConversion -> "UNIMOD:28"
        | NTerminalGlutamicAcidToPyroglutamateConversion -> "UNIMOD:27"
        | Acetyl -> "UNIMOD:1"
        | AmmoniaLoss -> "UNIMOD:385"
        | Reasearcher -> "1001271"
        | MsMsSearch -> "1001083"
        | MascotMGFformat -> "1001062"
        | MultiplePeakListNativeIDFormat -> "1000774"
        | OMSSAXMLFormat -> "MS:1001400"
        | FASTAFormat -> "MS:1001348"
        | Frag -> "1001224"
        | PeptideShakerPSMScore -> "1002466"
        | GroupRepresentative -> "1002403"
        | MZRatio -> "MS:1000040"

let createDBEntry2 (dbContext:MzIdentML) =
    let mzIdentMLDocument =
        MzIdentMLDocumentHandler.init(
            "1.2.0",
            id="PeptideShaker v1.12.2"
                                     )

    let organization =
        OrganizationHandler.init("ORG_DOC_OWNER")
        |> OrganizationHandler.addName("Test")
        |> OrganizationHandler.addDetail
            (OrganizationParamHandler.init(
                (TermHandler.tryFindByID 
                    dbContext 
                    (TermIDByName.toID ContactName)
                ).Value
                                          )
            )
        |> OrganizationHandler.addToContext dbContext

    let person = 
        PersonHandler.init("PROVIDER")
        |> PersonHandler.addFirstName "test"
        |> PersonHandler.addLastName "test"
        |> PersonHandler.addDetail 
            (PersonParamHandler.init(
                (TermHandler.tryFindByID 
                dbContext 
                (TermIDByName.toID ContactAddress)
                ).Value
                                    )
            )
        |> PersonHandler.addOrganization
            (OrganizationHandler.tryFindByID 
                dbContext
                "ORG_DOC_OWNER"
            ).Value
    
    let contactRole =
        ContactRoleHandler.init(
            person,
            CVParamHandler.init(
                                (TermHandler.tryFindByID 
                                    dbContext 
                                    (TermIDByName.toID Reasearcher)
                                ).Value
                                ),
            "PROVIDER"
                               )
    
    let analysisSoftware =
        AnalysisSoftwareHandler.init(
            CVParamHandler.init(
                (TermHandler.tryFindByID 
                    dbContext 
                    (TermIDByName.toID PeptideShaker)
                ).Value
                               ),
            "ID_software"
                                    )
        |> AnalysisSoftwareHandler.addName "PeptideShaker"
        |> AnalysisSoftwareHandler.addURI "http://compomics.github.io/projects/peptide-shaker.html"
        |> AnalysisSoftwareHandler.addAnalysisSoftwareDeveloper contactRole
        |> AnalysisSoftwareHandler.addMzIdentMLDocument mzIdentMLDocument
    
    let provider =
        ProviderHandler.init("PROVIDER")
        |> ProviderHandler.addContactRole contactRole
        |> ProviderHandler.addMzIdentMLDocument mzIdentMLDocument
        |> ProviderHandler.addToContext dbContext

    let peptide =
        PeptideHandler.init("SIQFVDWCPTGFK", "SIQFVDWCPTGFK")
        |> PeptideHandler.addModification
            (ModificationHandler.init(
                [ModificationParamHandler.init(
                                                (TermHandler.tryFindByID 
                                                    dbContext 
                                                    (TermIDByName.toID Carbamidomethyl)
                                                ).Value
                                            )
                ]
                                     )
             |> ModificationHandler.addMonoIsotopicMassDelta 57.021464
             |> ModificationHandler.addLocation 8
             |> ModificationHandler.addResidues "C"
            )

    //let spectrumIdentification =
    //    SpectrumIdentificationHandler.init(
    //                                       (SpectrumIdentificationListHandler.tryFindByID dbContext "SIL_1").Value,
    //                                       (SpectrumIdentificationProtocolHandler.tryFindByID dbContext "SearchProtocol_1").Value,
    //                                       (SpectraDataHandler.tryFindByID dbContext "qExactive01819.mgf").Value,
    //                                       (SearchDatabaseHandler.tryFindByID dbContext "SearchDB_1").Value,
    //                                       "SpecIdent_1"
    //                                      )
    //    |> SpectrumIdentificationHandler.addToContext dbContext
    
    //let proteinDetection =
    //    ProteinDetectionHandler.init(
    //                                 (ProteinDetectionListHandler.tryFindByID dbContext "Protein_groups").Value,
    //                                 (ProteinDetectionProtocolHandler.tryFindByID dbContext "PeptideShaker_1").Value,
    //                                 (SpectrumIdentificationListHandler.tryFindByID dbContext "SIL_1").Value,
    //                                 "PD_1"
    //                                )
    //    |> ProteinDetectionHandler.addToContext dbContext

    let spectrumIdentificationProtocol =
        SpectrumIdentificationProtocolHandler.init(
            analysisSoftware,
            (CVParamHandler.init(
                                (TermHandler.tryFindByID 
                                    dbContext 
                                    (TermIDByName.toID MsMsSearch)
                                ).Value
                                )
            ),
            [(ThresholdParamHandler.init(
                                        (TermHandler.tryFindByID 
                                            dbContext 
                                            (TermIDByName.toID PeptideSequenceLevelFDR)
                                        ).Value
                                        )
            )],
            "SearchProtocol_1"
                                                  )
        |> SpectrumIdentificationProtocolHandler.addAdditionalSearchParam
                (AdditionalSearchParamHandler.init(
                                                   (TermHandler.tryFindByID dbContext (TermIDByName.toID ParentMassMono)).Value
                                                  )
                )
        |> SpectrumIdentificationProtocolHandler.addModificationParams
            [(SearchModificationHandler.init(
                true,
                57.021464,
                "C",
                [SearchModificationParamHandler.init(
                                                    (TermHandler.tryFindByID
                                                        dbContext
                                                        (TermIDByName.toID Carbamidomethyl)
                                                    ).Value
                                                    )
                ]
                                            )
            )]
        |> SpectrumIdentificationProtocolHandler.addEnzyme
            (EnzymeHandler.init(
                                "Enz1",
                                "Trypsin",
                                semiSpecific=false,
                                missedCleavages=2
                               )
            |> EnzymeHandler.addEnzymeName
                (EnzymeNameParamHandler.init(
                    (TermHandler.tryFindByID 
                        dbContext 
                        (TermIDByName.toID Trypsin)
                    ).Value
                                            )
                )
            )
        |> SpectrumIdentificationProtocolHandler.addIndependent_Enzymes false
        |> SpectrumIdentificationProtocolHandler.addFragmentTolerance
            (FragmentToleranceParamHandler.init(
                (TermHandler.tryFindByID 
                    dbContext 
                    (TermIDByName.toID SearchTolerancePlusValue)
                ).Value
                                               )
            )
        |> SpectrumIdentificationProtocolHandler.addParentTolerance
            (ParentToleranceParamHandler.init(
                (TermHandler.tryFindByID 
                dbContext 
                (TermIDByName.toID SearchTolerancePlusValue)
                ).Value
                                             )
            )
        |> SpectrumIdentificationProtocolHandler.addToContext dbContext

    let proteinDetectionProtocol =
        ProteinDetectionProtocolHandler.init(
            analysisSoftware,
            [(ThresholdParamHandler.init(
                (TermHandler.tryFindByID
                    dbContext 
                    (TermIDByName.toID ProteinGroupLevelFDR)
                ).Value
                                        )
            |> ThresholdParamHandler.addValue "0.01"
            )],
            "PeptideShaker_1"
                                            )
        |> ProteinDetectionProtocolHandler.addToContext dbContext
    
    let inputs =
        InputsHandler.init(
            [(SpectraDataHandler.init(
                "file:/C:/Users/hba041/My_Git_Applications/peptide-shaker.wiki/data/2016_04_05/qExactive01819.mgf",
                CVParamHandler.init(
                                    (TermHandler.tryFindByID 
                                        dbContext 
                                        (TermIDByName.toID MascotMGFformat)
                                    ).Value
                                ),
                CVParamHandler.init(
                                    (TermHandler.tryFindByID
                                        dbContext 
                                        (TermIDByName.toID MultiplePeakListNativeIDFormat)
                                    ).Value
                                ),
                "qExactive01819.mgf",
                "qExactive01819.mgf"
                                    )
            )]
                          )
        |> InputsHandler.addSourceFile
            (SourceFileHandler.init(
                "file:/C:/Users/hba041/My_Git_Applications/peptide-shaker.wiki/data/2016_04_05/.PeptideShaker_unzip_temp/searchgui_out_PeptideShaker_temp/qExactive01819.omx",
                CVParamHandler.init(
                    (TermHandler.tryFindByID 
                        dbContext 
                        (TermIDByName.toID OMSSAXMLFormat)
                    ).Value
                                    ),
                "SourceFile_1"
                                   )
            )
        |> InputsHandler.addSearchDatabase
            (SearchDatabaseHandler.init(
                "file:/C:/Users/hba041/My_Git_Applications/peptide-shaker.wiki/data/2016_04_05/uniprot-human-reviewed-trypsin-april-2016_concatenated_target_decoy.fasta",
                CVParamHandler.init(
                    (TermHandler.tryFindByID 
                        dbContext 
                        (TermIDByName.toID FASTAFormat)
                    ).Value
                                    ),
                CVParamHandler.init(
                    (TermHandler.init("uniprot-human-reviewed-trypsin-april-2016_concatenated_target_decoy.fasta")
                        |> TermHandler.addOntology
                            (OntologyHandler.tryFindByID 
                                dbContext 
                                "UserParam"
                            ).Value
                    )
                                    ),
                "SearchDB_1"
                                       )
             |> SearchDatabaseHandler.addNumDatabaseSequences 40400L
            )
        |> InputsHandler.addMzIdentMLDocument mzIdentMLDocument
        |> InputsHandler.addToContext dbContext
    
    let dbSequence =
        DBSequenceHandler.init(
            "P27348",
            (SearchDatabaseHandler.tryFindByID 
                dbContext 
                "SearchDB_1"
            ).Value,
            "P27348"
                              )
        |> DBSequenceHandler.addDetail
            (DBSequenceParamHandler.init(
                (TermHandler.tryFindByID 
                    dbContext
                    (TermIDByName.toID ProteinDescription)
                ).Value
                                        )
            |> DBSequenceParamHandler.addValue "1433T_HUMAN 14-3-3 protein theta OS=Homo sapiens GN=YWHAQ PE=1 SV=1"
            )

    let peptideEvidence =
        PeptideEvidenceHandler.init(
            dbSequence,
            peptide,
            "PepEv_1"
                                   )
        |> PeptideEvidenceHandler.addIsDecoy false
        |> PeptideEvidenceHandler.addPre "R"
        |> PeptideEvidenceHandler.addPost "V"
        |> PeptideEvidenceHandler.addStart 340
        |> PeptideEvidenceHandler.addEnd 352
        |> PeptideEvidenceHandler.addToContext dbContext

    let fragmentationTable =
        MeasureHandler.init(
            [MeasureParamHandler.init(
                (TermHandler.tryFindByID
                    dbContext
                    (TermIDByName.toID ProductIonMasstoChargeRatio)
                ).Value
                                        )
                |> MeasureParamHandler.addUnit
                    (TermHandler.tryFindByID
                        dbContext
                        (TermIDByName.toID MZRatio)
                    ).Value
            ],
            "Measure_MZ"
                           )

    let spectrumIdentificationList =
        SpectrumIdentificationListHandler.init(
            [SpectrumIdentificationResultHandler.init(
                (SpectraDataHandler.tryFindByID dbContext "qExactive01819.mgf").Value,
                "index=11104",
                [SpectrumIdentificationItemHandler.init(
                    peptide,
                    3,
                    372.228240966797,
                    false,
                    1,
                    "SII_1_1"
                                                        )
                |> SpectrumIdentificationItemHandler.addPeptideEvidence
                    (PeptideEvidenceHandler.tryFindByID
                        dbContext
                        "PepEv_1"
                    ).Value
                |> SpectrumIdentificationItemHandler.addFragmentation
                    (IonTypeHandler.init(
                        [IonTypeParamHandler.init(
                                                (TermHandler.tryFindByID
                                                    dbContext
                                                    (TermIDByName.toID Frag)
                                                ).Value
                                                )
                        ]
                                        )
                    |> IonTypeHandler.addIndex (IndexHandler.init(1))
                    |> IonTypeHandler.addFragmentArray
                        (FragmentArrayHandler.init(
                            fragmentationTable,
                            [ValueHandler.init(335.2040405)]
                                                  )
                        )
                    )
                |> SpectrumIdentificationItemHandler.addDetail
                    (SpectrumIdentificationItemParamHandler.init(
                        (TermHandler.tryFindByID
                            dbContext 
                            (TermIDByName.toID PeptideShakerPSMScore)
                        ).Value
                                                                )
                        |> SpectrumIdentificationItemParamHandler.addValue "0.0"
                    )

                ],
                "SIR_1"
                                                     )
            |> SpectrumIdentificationResultHandler.addDetail
                (SpectrumIdentificationResultParamHandler.init(
                    (TermHandler.tryFindByID
                        dbContext
                        (TermIDByName.toID PeptideShakerPSMScore)
                    ).Value
                    )
                )
            ],
            "SIL_1"                                      
                                                    )
        |> SpectrumIdentificationListHandler.addFragmentationTable fragmentationTable

    let proteinDetectionList =
        ProteinDetectionListHandler.init("Protein_groups")
        |> ProteinDetectionListHandler.addProteinAmbiguityGroup
            (ProteinAmbiguityGroupHandler.init(
                [ProteinDetectionHypothesisHandler.init(
                    true,
                    dbSequence,
                    [PeptideHypothesisHandler.init(
                        (PeptideEvidenceHandler.tryFindByID
                            dbContext
                            "PepEv_1"
                        ).Value,
                        spectrumIdentificationList.
                            SpectrumIdentificationResult.[0].SpectrumIdentificationItem
                                                    )
                    ],
                    "PAG_0"
                                                       )
                 |> ProteinDetectionHypothesisHandler.addDetail
                        (ProteinDetectionHypothesisParamHandler.init(
                            (TermHandler.tryFindByID
                            dbContext 
                            (TermIDByName.toID GroupRepresentative)
                            ).Value
                                                                    )
                        )
                ]   
                                            )
        |> ProteinAmbiguityGroupHandler.addDetail
            (ProteinAmbiguityGroupParamHandler.init(
                                                    (TermHandler.tryFindByID
                                                        dbContext 
                                                        (TermIDByName.toID PeptideShakerProteinGroupScore)
                                                    ).Value
                                                    )
                |> ProteinAmbiguityGroupParamHandler.addValue "100.0"
            )
            )

    let analysisData =
        AnalysisDataHandler.init([spectrumIdentificationList])
        |> AnalysisDataHandler.addProteinDetectionList proteinDetectionList
        |> AnalysisDataHandler.addMzIdentMLDocument mzIdentMLDocument
        |> AnalysisDataHandler.addToContext dbContext
    analysisData