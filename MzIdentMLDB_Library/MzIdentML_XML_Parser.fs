namespace MzIdentMLDataBase

open System
open DataModel
open InsertStatements.ObjectHandlers
open FSharp.Data


module XMLParsing =

    type SchemePeptideShaker = XmlProvider<Schema = "..\MzIdentMLDB_Library\XML_Files\MzIdentMLScheme1_2.xsd">
    //let  samplePeptideShaker = SchemePeptideShaker.Load("..\ExampleFile\PeptideShaker_mzid_1_2_example.txt") 

    let setOption (addFunction:'a->'b->'b) (item:'a option) (object:'b) =
        match item with
        |Some x -> addFunction x object
        |None -> object

    let takeTermEntry (dbContext : MzIdentML) (termID : string) =
        query {
            for i in dbContext.Term do
                if i.ID = termID then
                    select (i)
              }
        |> (fun item -> if (Seq.length item) > 0 
                            then Seq.item 0 item
                            else TermHandler.init(null))

    let takeTermEntryOption (dbContext : MzIdentML) (termID : string option) =
        match termID with
            |Some x -> 
                query {
                    for i in dbContext.Term do
                        if i.ID = x then
                            select (i)
                      }
                |> (fun item -> if (Seq.length item) > 0 
                                    then Seq.item 0 item
                                    else TermHandler.init(null))
            |None -> TermHandler.init(null)

    //Define insertStatements for standardDB////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    let convertOptionToEntity (converter:MzIdentML->'a->'b) (dbContext:MzIdentML) (mzIdentMLXML:'a option) =
        match mzIdentMLXML with
        |Some x -> converter dbContext x
        |None -> Unchecked.defaultof<'b>

    let convertToEntityCVParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = CVParamHandler.init((TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession)|> (fun item -> match item with
                                                                                                                            | Some x -> x
                                                                                                                            | None -> null
                                                                                                               )))
        let addedValue = setOption CVParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = (match mzIdentMLXML.UnitAccession with
                         |Some x -> CVParamHandler.addUnit
                                        (TermHandler.tryFindByID dbContext x
                                        |> (fun item -> match item with
                                                        | Some x -> x
                                                        | None -> null
                                           ))
                                        addedValue
                         |None -> addedValue
                        )
        addedUnit

    let convertToEntityModificationParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = ModificationParamHandler.init((TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession)|> (fun item -> match item with
                                                                                                                                    | Some x -> x
                                                                                                                                    | None -> null
                                                                                                                       )))
        let addedValue = setOption ModificationParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = (match mzIdentMLXML.UnitAccession with
                         |Some x -> ModificationParamHandler.addUnit
                                        (TermHandler.tryFindByID dbContext x|> (fun item -> match item with
                                                                                            | Some x -> x
                                                                                            | None -> null
                                                                               ))
                                        addedValue
                         |None -> addedValue
                        )
        addedUnit

    let convertToEntityTranslationTableParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = TranslationTableParamHandler.init((TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession)|> (fun item -> match item with
                                                                                                                                        | Some x -> x
                                                                                                                                        | None -> null
                                                                                                                            )))
        let addedValue = setOption TranslationTableParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = (match mzIdentMLXML.UnitAccession with
                         |Some x -> TranslationTableParamHandler.addUnit
                                        (TermHandler.tryFindByID dbContext x|> (fun item -> match item with
                                                                                            | Some x -> x
                                                                                            | None -> null
                                                                               ))
                                        addedValue
                         |None -> addedValue
                        )
        addedUnit

    let convertToEntityMeasureParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = MeasureParamHandler.init((TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))|> (fun item -> match item with
                                                                                                                                | Some x -> x
                                                                                                                                | None -> null
                                                                                                                           ))
        let addedValue = setOption MeasureParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = (match mzIdentMLXML.UnitAccession with
                         |Some x -> MeasureParamHandler.addUnit
                                        (TermHandler.tryFindByID dbContext x|> (fun item -> match item with
                                                                                            | Some x -> x
                                                                                            | None -> null
                                                                               ))
                                        addedValue
                         |None -> addedValue
                        )
        addedUnit

    let convertToEntitySearchModificationParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = SearchModificationParamHandler.init((TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))|> (fun item -> match item with
                                                                                                                                            | Some x -> x
                                                                                                                                            | None -> null
                                                                                                                            ))
        let addedValue = setOption SearchModificationParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = (match mzIdentMLXML.UnitAccession with
                         |Some x -> SearchModificationParamHandler.addUnit
                                        (TermHandler.tryFindByID dbContext x|> (fun item -> match item with
                                                                                            | Some x -> x
                                                                                            | None -> null
                                                                               ))
                                        addedValue
                         |None -> addedValue
                        )
        addedUnit

    let convertToEntitySpecificityRuleParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = SpecificityRuleParamHandler.init((TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))|> (fun item -> match item with
                                                                                                                                        | Some x -> x
                                                                                                                                        | None -> null
                                                                                                                           ))
        let addedValue = setOption SpecificityRuleParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = (match mzIdentMLXML.UnitAccession with
                         |Some x -> SpecificityRuleParamHandler.addUnit
                                        (TermHandler.tryFindByID dbContext x|> (fun item -> match item with
                                                                                            | Some x -> x
                                                                                            | None -> null
                                                                               ))
                                        addedValue
                         |None -> addedValue
                        ) 
        addedUnit

    let convertToEntityFragmentToleranceParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = FragmentToleranceParamHandler.init((TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))|> (fun item -> match item with
                                                                                                                                         | Some x -> x
                                                                                                                                         | None -> null
                                                                                                                           ))
        let addedValue = setOption FragmentToleranceParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = (match mzIdentMLXML.UnitAccession with
                         |Some x -> FragmentToleranceParamHandler.addUnit
                                        (TermHandler.tryFindByID dbContext x|> (fun item -> match item with
                                                                                            | Some x -> x
                                                                                            | None -> null
                                                                               ))
                                        addedValue
                         |None -> addedValue
                        ) 
        addedUnit

    let convertToEntityParentToleranceParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = ParentToleranceParamHandler.init((TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))|> (fun item -> match item with
                                                                                                                                        | Some x -> x
                                                                                                                                        | None -> null
                                                                                                                           ))
        let addedValue = setOption ParentToleranceParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = (match mzIdentMLXML.UnitAccession with
                         |Some x -> ParentToleranceParamHandler.addUnit
                                        (TermHandler.tryFindByID dbContext x|> (fun item -> match item with
                                                                                            | Some x -> x
                                                                                            | None -> null
                                                                               ))
                                        addedValue
                         |None -> addedValue
                        ) 
        addedUnit

    let convertToEntitySearchDatabaseParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = SearchDatabaseParamHandler.init((TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))|> (fun item -> match item with
                                                                                                                                        | Some x -> x
                                                                                                                                        | None -> null
                                                                                                                          ))
        let addedValue = setOption SearchDatabaseParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = (match mzIdentMLXML.UnitAccession with
                         |Some x -> SearchDatabaseParamHandler.addUnit
                                        (TermHandler.tryFindByID dbContext x|> (fun item -> match item with
                                                                                            | Some x -> x
                                                                                            | None -> null
                                                                               ))
                                        addedValue
                         |None -> addedValue
                        ) 
        addedUnit

    let convertToEntity_UserParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.UserParam) =
        let initialized = CVParamHandler.init(null)
        let addedValue = setOption CVParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = CVParamHandler.addUnit (takeTermEntryOption dbContext mzIdentMLXML.UnitAccession) addedValue
        addedUnit

    let chooseUserOrCVParamOption (dbContext:MzIdentML) (cvParam:SchemePeptideShaker.CvParam option) (userParam:SchemePeptideShaker.UserParam option) =
        match cvParam with
        |Some x -> convertToEntityCVParam dbContext x
        |None -> convertToEntity_UserParam dbContext userParam.Value

    let convertCVandUserParamCollections (dbContext:MzIdentML) (cvParams:SchemePeptideShaker.CvParam []) (userParams:SchemePeptideShaker.UserParam []) =
        (Array.append
                    (cvParams |> Array.map (fun item -> convertToEntityCVParam dbContext item))
                    (userParams |> Array.map (fun item -> convertToEntity_UserParam dbContext item))
        )
    
    let convertCVandUserParamCollectionOptions (dbContext:MzIdentML) (cvParams:SchemePeptideShaker.CvParam [] option) (userParams:SchemePeptideShaker.UserParam [] option) =
        let cvParamValues =
            match cvParams with
            |Some x -> x
            |None -> Unchecked.defaultof<array<SchemePeptideShaker.CvParam>>
        let userParamValues =
            match userParams with
            |Some x -> x
            |None -> Unchecked.defaultof<array<SchemePeptideShaker.UserParam>>

        (Array.append
                    (cvParamValues |> Array.map (fun item -> convertToEntityCVParam dbContext item))
                    (userParamValues |> Array.map (fun item -> convertToEntity_UserParam dbContext item))
        )

    let convertToEntityOption_CVParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam option) =
        convertOptionToEntity convertToEntityCVParam dbContext mzIdentMLXML

//Start real XML-Parser////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    let convertToEntity_Organization (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.Organization) =
        let organizationBasic = OrganizationHandler.init(mzIdentMLXML.Id)
        let organizationWithName = setOption OrganizationHandler.addName mzIdentMLXML.Name organizationBasic
        let organizationWithDetails = 
            OrganizationHandler.addDetails 
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                  |> Array.map (fun cvParamItem -> OrganizationParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                organizationWithName
        let organizationWithParent = 
                (match mzIdentMLXML.Parent with
                 |Some x -> OrganizationHandler.addParent 
                                x.OrganizationRef
                                organizationWithDetails
                 |None -> organizationWithDetails
                )
        OrganizationHandler.addToContext dbContext organizationWithParent

    let convertToEntity_Person (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.Person) =
        let personBasic = PersonHandler.init(mzIdentMLXML.Id)
        let personWithName = setOption PersonHandler.addName mzIdentMLXML.Name personBasic
        let personWithFirstName = setOption PersonHandler.addFirstName mzIdentMLXML.FirstName personWithName
        let personWithMidInitials = setOption PersonHandler.addMidInitials mzIdentMLXML.MidInitials personWithFirstName
        let personWithLastName = setOption PersonHandler.addLastName mzIdentMLXML.LastName personWithMidInitials
        let personWithDetails = 
            PersonHandler.addDetails 
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> PersonParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                personWithLastName
        let personWithOrganizations = 
            PersonHandler.addOrganizations 
                (mzIdentMLXML.Affiliations 
                    |> Array.map (fun item -> (OrganizationHandler.tryFindByID dbContext item.OrganizationRef|> (fun item -> match item with
                                                                                                                                | Some x -> x
                                                                                                                                | None -> null
                                                                                                                    )))
                )
                personWithDetails
        PersonHandler.addToContext dbContext personWithOrganizations

    let convertToEntity_ContactRole (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.ContactRole) =
        let contactRoleBasic = ContactRoleHandler.init((PersonHandler.tryFindByID dbContext mzIdentMLXML.ContactRef)|> (fun item -> match item with
                                                                                                                                    | Some x -> x
                                                                                                                                    | None -> null
                                                                                                                      ), 
                                                       convertToEntityCVParam dbContext mzIdentMLXML.Role.CvParam,
                                                       mzIdentMLXML.ContactRef
                                                      )
        contactRoleBasic

    let convertToEntity_AnalysisSoftware (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.AnalysisSoftware) =
        let analysisSoftwareBasic = 
            AnalysisSoftwareHandler.init((chooseUserOrCVParamOption dbContext mzIdentMLXML.SoftwareName.CvParam mzIdentMLXML.SoftwareName.UserParam), mzIdentMLXML.Id)
        let analysisSoftwareWithname = 
            setOption AnalysisSoftwareHandler.addName mzIdentMLXML.Name analysisSoftwareBasic
        let analysisSoftwareWithURI = 
            setOption AnalysisSoftwareHandler.addURI mzIdentMLXML.Uri analysisSoftwareWithname
        let analysisSoftwareWithVersion = 
            setOption AnalysisSoftwareHandler.addVersion mzIdentMLXML.Version analysisSoftwareWithURI
        let analysisSoftwareWithCustomizations = 
            setOption AnalysisSoftwareHandler.addCustomization mzIdentMLXML.Customizations analysisSoftwareWithVersion
        let analysisSoftwareWithSoftwareDeveloper = 
            AnalysisSoftwareHandler.addAnalysisSoftwareDeveloper (convertOptionToEntity convertToEntity_ContactRole dbContext mzIdentMLXML.ContactRole) analysisSoftwareWithCustomizations
        AnalysisSoftwareHandler.addToContext dbContext analysisSoftwareWithSoftwareDeveloper

    let convertToEntity_Sample (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.Sample) =
        let sampleBasic = SampleHandler.init(mzIdentMLXML.Id)
        let sampleWithName = setOption SampleHandler.addName mzIdentMLXML.Name sampleBasic
        let sampleWithContactRoles =
            SampleHandler.addContactRoles
                (mzIdentMLXML.ContactRoles
                 |> Array.map (fun contactRole -> convertToEntity_ContactRole dbContext contactRole)
                )
                sampleWithName
        let sampleWithDetails =
            SampleHandler.addDetails 
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> SampleParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                sampleWithName
        SampleHandler.addToContext dbContext sampleWithDetails

    let convertToEntity_SubSample (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SubSample) =
        let subSampleBasic = SubSampleHandler.init()
        let subSampleWithSample = SubSampleHandler.addSample (SampleHandler.tryFindByID dbContext mzIdentMLXML.SampleRef|> (fun item -> match item with
                                                                                                                                        | Some x -> x
                                                                                                                                        | None -> null
                                                                                                                            )) subSampleBasic
        SubSampleHandler.addToContext dbContext subSampleWithSample

    let convertToEntity_Modification (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.Modification) =
        let modificationBasic = 
            ModificationHandler.init(
                (mzIdentMLXML.CvParams
                 |> Array.map (fun cvParam -> convertToEntityModificationParam dbContext cvParam)
                ))
        let modificationWithLocation = setOption ModificationHandler.addLocation mzIdentMLXML.Location modificationBasic
        let modificationWithResidues = setOption ModificationHandler.addResidues mzIdentMLXML.Residues modificationWithLocation
        let modificationWithMonoMass = setOption ModificationHandler.addMonoIsotopicMassDelta mzIdentMLXML.MonoisotopicMassDelta modificationWithResidues
        let modificationWithAVGMass  = setOption ModificationHandler.addAvgMassDelta mzIdentMLXML.AvgMassDelta modificationWithMonoMass
        modificationWithAVGMass

    let convertToEntity_SubstitutionModification (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SubstitutionModification) =
        let subModificationBasic = SubstitutionModificationHandler.init(mzIdentMLXML.OriginalResidue, mzIdentMLXML.ReplacementResidue)
        let subModificationWithMonoMass = setOption SubstitutionModificationHandler.addMonoIsotopicMassDelta mzIdentMLXML.MonoisotopicMassDelta subModificationBasic
        let subModificationAVGMass = setOption SubstitutionModificationHandler.addAvgMassDelta mzIdentMLXML.AvgMassDelta subModificationWithMonoMass
        let subModificationWithLocation = setOption SubstitutionModificationHandler.addLocation mzIdentMLXML.Location subModificationAVGMass
        subModificationWithLocation

    let convertToEntity_Peptide (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.Peptide) =
        let peptideBasic = PeptideHandler.init(mzIdentMLXML.PeptideSequence, mzIdentMLXML.Id)
        let peptideWithName = setOption PeptideHandler.addName mzIdentMLXML.Name peptideBasic
        let peptideWithModifications =
            PeptideHandler.addModifications
                (mzIdentMLXML.Modifications
                 |> Array.map (fun modification -> convertToEntity_Modification dbContext modification)
                )
                peptideWithName
        let peptideWithSubMods =
            PeptideHandler.addSubstitutionModifications
                (mzIdentMLXML.SubstitutionModifications
                 |> Array.map (fun subMod -> convertToEntity_SubstitutionModification dbContext subMod)
                )
                peptideWithModifications
        let peptideWithDetails =
            PeptideHandler.addDetails
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem ->PeptideParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                peptideWithSubMods
        PeptideHandler.addToContext dbContext peptideWithDetails

    let convertToEntity_TranslationTable (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.TranslationTable) =
        let translationTableBasic = TranslationTableHandler.init(mzIdentMLXML.Id)
        let translationTableWithName = setOption TranslationTableHandler.addName mzIdentMLXML.Name translationTableBasic
        let translationTableWithDetails = 
            TranslationTableHandler.addDetails
                (mzIdentMLXML.CvParams
                 |> Array.map (fun cvParam -> convertToEntityTranslationTableParam dbContext cvParam)
                )
                translationTableWithName
        translationTableWithDetails

    let convertToEntity_Measure (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.Measure) =
        let measureBasic = MeasureHandler.init(
                                          mzIdentMLXML.CvParams
                                          |> Array.map (fun cvParam -> convertToEntityMeasureParam dbContext cvParam), 
                                          mzIdentMLXML.Id
                                              )
        let measureWithName = setOption MeasureHandler.addName mzIdentMLXML.Name measureBasic
        measureWithName

    let convertToEntity_Residue (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.Residue) =
        let residueBasic = ResidueHandler.init(mzIdentMLXML.Code, float mzIdentMLXML.Mass)
        residueBasic

    let convertToEntity_AmbiguousResidue (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.AmbiguousResidue) =
        let ambiguousResidueBasic = AmbiguousResidueHandler.init(
                                                            mzIdentMLXML.Code,
                                                            ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                                                             |> Array.map (fun cvParamItem -> AmbiguousResidueParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                                                            )
                                                                )
        ambiguousResidueBasic

    let convertToEntity_MassTable (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.MassTable) =
        let massTableBasic = MassTableHandler.init(mzIdentMLXML.MsLevel, mzIdentMLXML.Id)
        let massTableWithName = setOption MassTableHandler.addName mzIdentMLXML.Name massTableBasic
        let massTableWithResidues =  
            MassTableHandler.addResidues 
                (mzIdentMLXML.Residues
                 |> Array.map (fun residue -> convertToEntity_Residue dbContext residue)
                )
                massTableWithName
        let massTableWithAmbigousResidues = 
            MassTableHandler.addAmbiguousResidues
                (mzIdentMLXML.AmbiguousResidues
                    |> Array.map (fun ambResidue -> convertToEntity_AmbiguousResidue dbContext ambResidue)
                )
                massTableWithResidues
        let massTableWithDetails =
            MassTableHandler.addDetails
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> MassTableParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                massTableWithAmbigousResidues
        massTableWithDetails

//Stup because there is a problem with the xmlFile with the values of the FragmentArray/////////////////
    let convertToEntity_FragmentArray (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.FragmentArray) =
        let fragmentArrayBasic = FragmentArrayHandler.init(
                                                           (CVParamHandler.tryFindByID dbContext mzIdentMLXML.MeasureRef|> (fun item -> match item with
                                                                                                                                        | Some x -> x
                                                                                                                                        | None -> null
                                                                                                                           )), 
                                                           [ValueHandler.init (0.)]
                                                          )
        fragmentArrayBasic 

    let convertToEntity_IonType (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.IonType) =
        let ionTypeBasic = IonTypeHandler.init(
                                               (convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams
                                                |> Array.map (fun cvParamItem -> IonTypeParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                                               )
                                              )
        let ionTypeWithFragmentArray = 
            IonTypeHandler.addFragmentArrays 
                (mzIdentMLXML.FragmentArray
                 |> Array.map (fun fragmentArray -> convertToEntity_FragmentArray dbContext fragmentArray)
                )
                ionTypeBasic
        ionTypeWithFragmentArray

    let convertToEntity_SpectrumIdentificationItem (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SpectrumIdentificationItem) =
        let spectrumIdentificationItemBasic = 
            SpectrumIdentificationItemHandler.init(
                                                   (PeptideHandler.tryFindByID dbContext mzIdentMLXML.PeptideRef|> (fun item -> match item with
                                                                                                                                | Some x -> x
                                                                                                                                | None -> null
                                                                                                                   )),
                                                   mzIdentMLXML.ChargeState, 
                                                   mzIdentMLXML.ExperimentalMassToCharge, 
                                                   mzIdentMLXML.PassThreshold,
                                                   mzIdentMLXML.Rank,
                                                   mzIdentMLXML.Id
                                                  )
        let spectrumIdentificationItemWithName = setOption SpectrumIdentificationItemHandler.addName mzIdentMLXML.Name spectrumIdentificationItemBasic
        let spectrumIdentificationItemWithSample = 
                (match mzIdentMLXML.SampleRef with 
                 |Some x -> SpectrumIdentificationItemHandler.addSample  
                                (SampleHandler.tryFindByID dbContext x|> (fun item -> match item with
                                                                                        | Some x -> x
                                                                                        | None -> null
                                                                           ))
                                spectrumIdentificationItemWithName
                 |None -> spectrumIdentificationItemWithName
                )
        let spectrumIdentificationItemWithMassTable = 
                (match mzIdentMLXML.MassTableRef with
                 |Some x -> SpectrumIdentificationItemHandler.addMassTable 
                                (MassTableHandler.tryFindByID dbContext x|> (fun item -> match item with
                                                                                            | Some x -> x
                                                                                            | None -> null
                                                                               ))
                                spectrumIdentificationItemWithSample
                 |None -> spectrumIdentificationItemWithSample
                )
        let spectrumIdentificationItemWithPeptideEvidence =
            SpectrumIdentificationItemHandler.addPeptideEvidences
                (mzIdentMLXML.PeptideEvidenceRefs
                 |> Array.map (fun peptideEvidenceRef -> (PeptideEvidenceHandler.tryFindByID dbContext peptideEvidenceRef.PeptideEvidenceRef|> (fun item -> match item with
                                                                                                                                                            | Some x -> x
                                                                                                                                                            | None -> null
                                                                                                                                               )))
                )
                spectrumIdentificationItemWithMassTable
        let spectrumIdentificationItemWithFragmentation =
                match mzIdentMLXML.Fragmentation with
                 |Some x -> SpectrumIdentificationItemHandler.addFragmentations 
                                (x.IonTypes
                                 |> Array.map (fun ionType -> convertToEntity_IonType dbContext ionType)
                                )
                                spectrumIdentificationItemWithPeptideEvidence
                 |None -> spectrumIdentificationItemWithPeptideEvidence
        let spectrumIdentificationItemWithCalculatedMassToCharge =
            setOption SpectrumIdentificationItemHandler.addCalculatedMassToCharge mzIdentMLXML.CalculatedMassToCharge spectrumIdentificationItemWithFragmentation
        let spectrumIdentificationItemWithCalculatedPI =
            SpectrumIdentificationItemHandler.addCalculatedPI 
                    (match mzIdentMLXML.CalculatedPi with
                     |Some x -> float x
                     |None -> -1.
                    )
                    spectrumIdentificationItemWithCalculatedMassToCharge
        let spectrumIdentificationItemWithDetails =
            SpectrumIdentificationItemHandler.addDetails
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> SpectrumIdentificationItemParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                spectrumIdentificationItemWithCalculatedPI
        spectrumIdentificationItemWithDetails

    let convertToEntity_SpectraData (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SpectraData) =
        let spectraDataBasic = 
            SpectraDataHandler.init(
                                    mzIdentMLXML.Location, 
                                    convertToEntityCVParam dbContext mzIdentMLXML.FileFormat.CvParam, 
                                    convertToEntityCVParam dbContext mzIdentMLXML.SpectrumIdFormat.CvParam, 
                                    mzIdentMLXML.Id
                                   )
        let spectraDataWithName = setOption SpectraDataHandler.addName mzIdentMLXML.Name spectraDataBasic
        let spectraDataWithexternalDataFormat = setOption SpectraDataHandler.addExternalFormatDocumentation mzIdentMLXML.ExternalFormatDocumentation spectraDataWithName
        SpectraDataHandler.addToContext dbContext spectraDataWithexternalDataFormat

    let convertToEntity_SpectrumIdentificationResult (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SpectrumIdentificationResult) =
        let spectrumIdentificationResultBasic = 
            SpectrumIdentificationResultHandler.init(
                                                     (SpectraDataHandler.tryFindByID dbContext mzIdentMLXML.SpectraDataRef|> (fun item -> match item with
                                                                                                                                            | Some x -> x
                                                                                                                                            | None -> null
                                                                                                                               )),
                                                     mzIdentMLXML.SpectrumId,
                                                     (mzIdentMLXML.SpectrumIdentificationItems
                                                      |> Array.map (fun spectrumIdentificationItem -> convertToEntity_SpectrumIdentificationItem dbContext spectrumIdentificationItem)
                                                     ), 
                                                     mzIdentMLXML.Id
                                                    )
        let spectrumIdentificationResultWithName = setOption SpectrumIdentificationResultHandler.addName mzIdentMLXML.Name spectrumIdentificationResultBasic
        let spectrumIdentificationResultWithDetails =
            SpectrumIdentificationResultHandler.addDetails
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> SpectrumIdentificationResultParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                spectrumIdentificationResultWithName
        spectrumIdentificationResultWithDetails

    let convertToEntity_SpectrumIdentificationList (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SpectrumIdentificationList) =
        let spectrumIdentificationListBasic =
            SpectrumIdentificationListHandler.init(
                                                   mzIdentMLXML.SpectrumIdentificationResults
                                                   |> Array.map (fun spectrumIdentificationResult -> convertToEntity_SpectrumIdentificationResult dbContext spectrumIdentificationResult),
                                                   mzIdentMLXML.Id
                                                  )
        let spectrumIdentificationListWithName = setOption SpectrumIdentificationListHandler.addName mzIdentMLXML.Name spectrumIdentificationListBasic
        let spectrumIdentificationListWithNumSequencesSearched =
            setOption SpectrumIdentificationListHandler.addNumSequencesSearched mzIdentMLXML.NumSequencesSearched spectrumIdentificationListWithName
        let spectrumIdentificationListWithFragmentationTable =
                match mzIdentMLXML.FragmentationTable with
                 |Some x -> SpectrumIdentificationListHandler.addFragmentationTables
                                (x.Measures
                                 |> Array.map (fun measure -> convertToEntity_Measure dbContext measure)
                                )
                                spectrumIdentificationListWithNumSequencesSearched
                 |None -> spectrumIdentificationListWithNumSequencesSearched
        let spectrumIdentificationListWithDetails =
            SpectrumIdentificationListHandler.addDetails   
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> SpectrumIdentificationListParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                spectrumIdentificationListWithFragmentationTable
        spectrumIdentificationListWithDetails

    let convertToEntity_SpecificityRule (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SpecificityRules) =
        let specificityRuleBasic = (
                                    mzIdentMLXML.CvParams
                                    |> Array.map (fun cvParam -> convertToEntitySpecificityRuleParam dbContext cvParam)
                                   )
        specificityRuleBasic

    let convertToEntity_SearchModification (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SearchModification) =
        let searchModificationBasic = SearchModificationHandler.init(
                                                                     mzIdentMLXML.FixedMod, 
                                                                     float mzIdentMLXML.MassDelta, 
                                                                     mzIdentMLXML.Residues, 
                                                                     mzIdentMLXML.CvParams
                                                                     |> Array.map (fun cvParam -> convertToEntitySearchModificationParam dbContext cvParam)
                                                                    )
        let searchModificationWithSpecificityRules = 
            SearchModificationHandler.addSpecificityRules 
                (mzIdentMLXML.SpecificityRules 
                 |> Array.map (fun specificityRule -> convertToEntity_SpecificityRule dbContext specificityRule)
                 |> Array.concat
                )
                searchModificationBasic
        searchModificationWithSpecificityRules

    let convertToEntity_Enzyme (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.Enzyme) =
        let enzymeBasic = EnzymeHandler.init(mzIdentMLXML.Id)
        let enzymeWithName = setOption EnzymeHandler.addName mzIdentMLXML.Name enzymeBasic
        let enzymeWithCTermGain = setOption EnzymeHandler.addCTermGain mzIdentMLXML.CTermGain enzymeWithName
        let enzymeWithNTermGain = setOption EnzymeHandler.addNTermGain mzIdentMLXML.NTermGain enzymeWithCTermGain
        let enzymeWithMinDistance = setOption EnzymeHandler.addMinDistance mzIdentMLXML.MinDistance enzymeWithNTermGain
        let enzymeWithMissedCleavageSides = setOption EnzymeHandler.addMissedCleavages mzIdentMLXML.MissedCleavages enzymeWithMinDistance
        let enzymeWithSemiSpecific = setOption EnzymeHandler.addSemiSpecific mzIdentMLXML.SemiSpecific enzymeWithMissedCleavageSides
        let enzymeWithSiteRegxc = setOption EnzymeHandler.addSiteRegexc mzIdentMLXML.SiteRegexp enzymeWithSemiSpecific
        let enzymeWithEnzymeNames = 
                match mzIdentMLXML.EnzymeName with
                 |Some x -> EnzymeHandler.addEnzymeNames
                                (convertCVandUserParamCollections dbContext x.CvParams x.UserParams
                                 |> Array.map (fun cvParamItem -> EnzymeNameParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                                )
                                enzymeWithSiteRegxc
                 |None -> enzymeWithSiteRegxc
        enzymeWithEnzymeNames

    let convertToEntity_Filter (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.Filter) =
        let filterBasic = 
            FilterHandler.init(chooseUserOrCVParamOption dbContext mzIdentMLXML.FilterType.CvParam mzIdentMLXML.FilterType.UserParam)
        let filterWithIncludes = 
            FilterHandler.addIncludes 
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.Include.Value.CvParams mzIdentMLXML.Include.Value.UserParams)
                 |> Array.map (fun cvParamItem -> IncludeParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                filterBasic
        let filterWithExcludes = 
            FilterHandler.addExcludes
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.Exclude.Value.CvParams mzIdentMLXML.Exclude.Value.UserParams)
                 |> Array.map (fun cvParamItem -> ExcludeParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                filterWithIncludes
        filterWithExcludes

    let convertToEntity_SpectrumIdentificationProtocol (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SpectrumIdentificationProtocol) =
        let spectrumIdentificationProtocolBasic =
            SpectrumIdentificationProtocolHandler.init
                (
                 (AnalysisSoftwareHandler.tryFindByID dbContext mzIdentMLXML.AnalysisSoftwareRef|> (fun item -> match item with
                                                                                                                | Some x -> x
                                                                                                                | None -> null
                                                                                                   )), 
                 chooseUserOrCVParamOption dbContext mzIdentMLXML.SearchType.CvParam mzIdentMLXML.SearchType.UserParam, 
                 (convertCVandUserParamCollections dbContext mzIdentMLXML.Threshold.CvParams mzIdentMLXML.Threshold.UserParams
                  |> Array.map (fun cvParamItem -> ThresholdParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                 ),
                 mzIdentMLXML.Id
                )
        let spectrumIdentificationProtocolWithName = 
            setOption SpectrumIdentificationProtocolHandler.addName mzIdentMLXML.Name spectrumIdentificationProtocolBasic
        let spectrumIdentificationProtocolWithAdditionalSearchParams = 
                match mzIdentMLXML.AdditionalSearchParams with
                 |Some x -> SpectrumIdentificationProtocolHandler.addAdditionalSearchParams
                                (convertCVandUserParamCollections dbContext x.CvParams x.UserParams
                                 |> Array.map (fun cvParamItem -> AdditionalSearchParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                                )
                                spectrumIdentificationProtocolWithName
                 |None -> spectrumIdentificationProtocolWithName
        let spectrumIdentificationProtocolWithModificationParams = 
                match mzIdentMLXML.ModificationParams with
                 |Some x -> SpectrumIdentificationProtocolHandler.addModificationParams
                                (x.SearchModifications 
                                 |> Array.map (fun searchModification -> convertToEntity_SearchModification dbContext searchModification)
                                )
                                spectrumIdentificationProtocolWithAdditionalSearchParams
                 |None -> spectrumIdentificationProtocolWithAdditionalSearchParams
        let spectrumIdentificationProtocolWithEnzymes =
                match mzIdentMLXML.Enzymes with 
                 |Some x -> SpectrumIdentificationProtocolHandler.addEnzymes 
                                (x.Enzymes 
                                 |> Array.map (fun enzyme -> convertToEntity_Enzyme dbContext enzyme)
                                )
                                spectrumIdentificationProtocolWithModificationParams
                 |None -> spectrumIdentificationProtocolWithModificationParams
        let spectrumIdentificationProtocolWithEnzymesWithIndepent_Enzymes = 
                (match mzIdentMLXML.Enzymes with
                    |Some x -> match x.Independent with
                               |Some x -> SpectrumIdentificationProtocolHandler.addIndependent_Enzymes
                                            x
                                            spectrumIdentificationProtocolWithEnzymes
                               |None -> spectrumIdentificationProtocolWithEnzymes
                    |None -> spectrumIdentificationProtocolWithEnzymes
                )
        let spectrumIdentificationProtocolWithMassTables =
            SpectrumIdentificationProtocolHandler.addMassTables
                (mzIdentMLXML.MassTables
                 |> Array.map (fun massTable -> convertToEntity_MassTable dbContext massTable)
                )
                spectrumIdentificationProtocolWithEnzymesWithIndepent_Enzymes
        let spectrumIdentificationProtocolWithFragmentTolerances =
                match mzIdentMLXML.FragmentTolerance with
                 |Some x -> SpectrumIdentificationProtocolHandler.addFragmentTolerances
                                (x.CvParams 
                                 |> Array.map (fun cvParam -> convertToEntityFragmentToleranceParam dbContext cvParam)
                                )
                                spectrumIdentificationProtocolWithMassTables
                 |None -> spectrumIdentificationProtocolWithMassTables
        let spectrumIdentificationProtocolWithParentTolerances =
                match mzIdentMLXML.ParentTolerance with
                 |Some x -> SpectrumIdentificationProtocolHandler.addParentTolerances
                                (x.CvParams 
                                 |> Array.map (fun cvParam -> convertToEntityParentToleranceParam dbContext cvParam)
                                )
                                spectrumIdentificationProtocolWithFragmentTolerances
                 |None -> spectrumIdentificationProtocolWithFragmentTolerances
        let spectrumIdentificationProtocolWithDatabaseFilter = 
                match mzIdentMLXML.DatabaseFilters with
                 |Some x -> SpectrumIdentificationProtocolHandler.addDatabaseFilters
                                (x.Filters 
                                 |> Array.map (fun filter -> convertToEntity_Filter dbContext filter)
                                )
                                spectrumIdentificationProtocolWithParentTolerances
                 |None -> spectrumIdentificationProtocolWithParentTolerances
        //let spectrumIdentificationProtocolWithDatabaseFrames =
        //    SpectrumIdentificationProtocolHandler.addFrame
        //        spectrumIdentificationProtocolWithDatabaseFilter
        //        (match mzIdentMLXML.DatabaseTranslation with
        //         |Some x -> match x.Frames with
        //                    //Stub because frame isn`t saved correctly in the xml File
        //                    |Some x -> Unchecked.defaultof<Frame>
        //                    |None -> Unchecked.defaultof<Frame>
        //         |None -> Unchecked.defaultof<Frame>
        //        )
        let spectrumIdentificationProtocolWithTranslationTable =
                match mzIdentMLXML.DatabaseTranslation with
                 |Some x -> SpectrumIdentificationProtocolHandler.addTranslationTables
                                (x.TranslationTables 
                                 |> Array.map (fun translationTable -> convertToEntity_TranslationTable dbContext translationTable)
                                )
                                spectrumIdentificationProtocolWithDatabaseFilter
                 |None -> spectrumIdentificationProtocolWithDatabaseFilter
        SpectrumIdentificationProtocolHandler.addToContext dbContext spectrumIdentificationProtocolWithTranslationTable

    let convertToEntity_SearchDatabase (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SearchDatabase) =
        let searchDatabaseBasic = 
            SearchDatabaseHandler.init(
                                       mzIdentMLXML.Location, 
                                       convertToEntityCVParam dbContext mzIdentMLXML.FileFormat.CvParam,
                                       (*chooseUserOrCVParamOption dbContext mzIdentMLXML.DatabaseName.CvParam mzIdentMLXML.DatabaseName.UserParam*) 
                                       null,
                                       mzIdentMLXML.Id
                                      )
        let searchDatabaseWithName = setOption SearchDatabaseHandler.addName mzIdentMLXML.Name searchDatabaseBasic
        let searchDatabaseWithNumDatabaseSeq = setOption SearchDatabaseHandler.addNumDatabaseSequences mzIdentMLXML.NumDatabaseSequences searchDatabaseWithName
        let searchDatabaseWithNumResidues = setOption SearchDatabaseHandler.addNumResidues mzIdentMLXML.NumResidues searchDatabaseWithNumDatabaseSeq
        let searchDatabaseWithReleaseDate = setOption SearchDatabaseHandler.addReleaseDate mzIdentMLXML.ReleaseDate searchDatabaseWithNumResidues
        let searchDatabaseWithVersion = setOption SearchDatabaseHandler.addVersion mzIdentMLXML.Version searchDatabaseWithReleaseDate
        let searchDatabaseWithExternalFormatDocu = setOption SearchDatabaseHandler.addExternalFormatDocumentation mzIdentMLXML.ExternalFormatDocumentation searchDatabaseWithVersion
        let searchDatabaseWithDetails = 
            SearchDatabaseHandler.addDetails
                (mzIdentMLXML.CvParams
                 |> Array.map (fun cvParam -> convertToEntitySearchDatabaseParam dbContext cvParam)
                )
                searchDatabaseWithExternalFormatDocu
        SearchDatabaseHandler.addToContext dbContext searchDatabaseWithDetails


    let convertToEntity_DBSequence (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.DbSequence) =
        let dbSequenceBasic = 
            DBSequenceHandler.init(
                                   mzIdentMLXML.Accession, 
                                   (SearchDatabaseHandler.tryFindByID dbContext mzIdentMLXML.SearchDatabaseRef|> (fun item -> match item with
                                                                                                                                | Some x -> x
                                                                                                                                | None -> null
                                                                                                                    )),
                                   mzIdentMLXML.Id
                                  )
        let dbSequenceWithName = setOption DBSequenceHandler.addName mzIdentMLXML.Name dbSequenceBasic
        let dbSequenceWithSequence = setOption DBSequenceHandler.addSequence mzIdentMLXML.Seq dbSequenceWithName
        let dbSequenceWithLength = setOption DBSequenceHandler.addLength mzIdentMLXML.Length dbSequenceWithSequence
        let dbSequenceWithDetails = 
            DBSequenceHandler.addDetails 
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> DBSequenceParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                dbSequenceWithLength
        DBSequenceHandler.addToContext dbContext dbSequenceWithDetails

    let convertToEntity_PeptideEvidence (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.PeptideEvidence) =
        let peptideEvidenceBasic = 
            PeptideEvidenceHandler.init(
                                        (DBSequenceHandler.tryFindByID dbContext mzIdentMLXML.DBSequenceRef|> (fun item -> match item with
                                                                                                                            | Some x -> x
                                                                                                                            | None -> null
                                                                                                               )),
                                        (PeptideHandler.tryFindByID dbContext mzIdentMLXML.PeptideRef|> (fun item -> match item with
                                                                                                                        | Some x -> x
                                                                                                                        | None -> null
                                                                                                            )),
                                        mzIdentMLXML.Id
                                       )
        let peptideEvidenceWithName = setOption PeptideEvidenceHandler.addName mzIdentMLXML.Name peptideEvidenceBasic
        let peptideEvidenceWithStart = setOption PeptideEvidenceHandler.addStart mzIdentMLXML.Start peptideEvidenceWithName
        let peptideEvudenceWithEnde = setOption PeptideEvidenceHandler.addEnd mzIdentMLXML.End peptideEvidenceWithStart
        let peptideEvidenceWithPre = setOption PeptideEvidenceHandler.addPre mzIdentMLXML.Pre peptideEvudenceWithEnde
        let peptideEvidenceWithPost = setOption PeptideEvidenceHandler.addPost mzIdentMLXML.Post peptideEvidenceWithPre
        let peptideEvidenceWithFrame = 
            PeptideEvidenceHandler.addFrame 
                (FrameHandler.init(
                                   match mzIdentMLXML.Frame with
                                   |Some x -> x
                                   |None -> -1
                                  )
                )
                peptideEvidenceWithPost
        let peptideEvidenceWithIsDecoy = setOption PeptideEvidenceHandler.addIsDecoy mzIdentMLXML.IsDecoy peptideEvidenceWithFrame
        let peptideEvidenceWithTranslationTable = 
            
                (match mzIdentMLXML.TranslationTableRef with
                 |Some x -> PeptideEvidenceHandler.addTranslationTable 
                                (TranslationTableHandler.tryFindByID dbContext x|> (fun item -> match item with
                                                                                                | Some x -> x
                                                                                                | None -> null
                                                                                   ))
                                peptideEvidenceWithIsDecoy
                 |None -> peptideEvidenceWithIsDecoy
                )
        let peptideEvidenceWithDetails =
            PeptideEvidenceHandler.addDetails 
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> PeptideEvidenceParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                peptideEvidenceWithTranslationTable
        PeptideEvidenceHandler.addToContext dbContext peptideEvidenceWithDetails

    let convertToEntity_SourceFile (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SourceFile) =
        let sourceFileBasic = 
            SourceFileHandler.init(
                                   mzIdentMLXML.Location, 
                                   convertToEntityCVParam dbContext mzIdentMLXML.FileFormat.CvParam,
                                   mzIdentMLXML.Id
                                  )
        let sourceFileWithName = setOption SourceFileHandler.addName mzIdentMLXML.Name sourceFileBasic
        let sourceFileWithExternalFormatDocumentation = setOption SourceFileHandler.addExternalFormatDocumentation mzIdentMLXML.ExternalFormatDocumentation sourceFileWithName
        let sourceFileWithDetails = 
            SourceFileHandler.addDetails
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> SourceFileParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                sourceFileWithExternalFormatDocumentation
        SourceFileHandler.addToContext dbContext sourceFileWithDetails

    let convertToEntity_SpectrumIdentification (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SpectrumIdentification) =
        let spectrumIdentificationBasic = 
            SpectrumIdentificationHandler.init(
                                               (SpectrumIdentificationListHandler.tryFindByID dbContext mzIdentMLXML.SpectrumIdentificationListRef|> (fun item -> match item with
                                                                                                                                                                    | Some x -> x
                                                                                                                                                                    | None -> null
                                                                                                                                                       )),
                                               (SpectrumIdentificationProtocolHandler.tryFindByID dbContext mzIdentMLXML.SpectrumIdentificationProtocolRef|> (fun item -> match item with
                                                                                                                                                                            | Some x -> x
                                                                                                                                                                            | None -> null
                                                                                                                                                               )),
                                               mzIdentMLXML.InputSpectras
                                               |> Array.map (fun inputSpectra -> match inputSpectra.SpectraDataRef with
                                                                                 |Some x -> (SpectraDataHandler.tryFindByID dbContext x|> (fun item -> match item with
                                                                                                                                                        | Some x -> x
                                                                                                                                                        | None -> null
                                                                                                                                           ))
                                                                                 |None -> null
                                                            ),
                                               mzIdentMLXML.SearchDatabaseRefs
                                               |> Array.map (fun searchDatabaseRef -> match searchDatabaseRef.SearchDatabaseRef with
                                                                                      |Some x -> (SearchDatabaseHandler.tryFindByID dbContext x|> (fun item -> match item with
                                                                                                                                                                | Some x -> x
                                                                                                                                                                | None -> null
                                                                                                                                                   ))
                                                                                      |None -> null
                                                            ),
                                               mzIdentMLXML.Id
                                              )
        let spectrumIdentificationWithName = setOption SpectrumIdentificationHandler.addName mzIdentMLXML.Name spectrumIdentificationBasic
        let spectrumIdentificationWithActivityDate = setOption SpectrumIdentificationHandler.addActivityDate mzIdentMLXML.ActivityDate spectrumIdentificationWithName
        spectrumIdentificationWithActivityDate

    let convertToEntity_PeptideHypothesis (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.PeptideHypothesis) =
        let peptideHypothesisBasic = 
            PeptideHypothesisHandler.init(
                                          (PeptideEvidenceHandler.tryFindByID dbContext mzIdentMLXML.PeptideEvidenceRef|> (fun item -> match item with
                                                                                                                                        | Some x -> x
                                                                                                                                        | None -> null
                                                                                                                           )),
                                          mzIdentMLXML.SpectrumIdentificationItemRefs
                                          |> Array.map (fun spectrumIdentificationItemRef -> (SpectrumIdentificationItemHandler.tryFindByID dbContext spectrumIdentificationItemRef.SpectrumIdentificationItemRef|> (fun item -> match item with
                                                                                                                                                                                                                                    | Some x -> x
                                                                                                                                                                                                                                    | None -> null
                                                                                                                                                                                                                       )))
                                         )
        peptideHypothesisBasic

    let convertToEntity_ProteinDetectionHypothesis (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.ProteinDetectionHypothesis) =
        let proteinDetectionHypothesisBasic = 
            ProteinDetectionHypothesisHandler.init(
                                                   mzIdentMLXML.PassThreshold, 
                                                   (DBSequenceHandler.tryFindByID dbContext mzIdentMLXML.DBSequenceRef|> (fun item -> match item with
                                                                                                                                        | Some x -> x
                                                                                                                                        | None -> null
                                                                                                                            )),
                                                   mzIdentMLXML.PeptideHypotheses
                                                   |> Array.map (fun peptideHypothesis -> convertToEntity_PeptideHypothesis dbContext peptideHypothesis),
                                                   mzIdentMLXML.Id
                                                  )
        let proteinDetectionHypothesisWithName = setOption ProteinDetectionHypothesisHandler.addName mzIdentMLXML.Name proteinDetectionHypothesisBasic
        let proteinDetectionHypothesisWithDetails = 
            ProteinDetectionHypothesisHandler.addDetails 
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> ProteinDetectionHypothesisParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                proteinDetectionHypothesisWithName
        proteinDetectionHypothesisWithDetails

    let convertToEntity_ProteinAmbiguityGroup (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.ProteinAmbiguityGroup) =
        let proteinAmbiguityGroupBasic = 
            ProteinAmbiguityGroupHandler.init(
                                              mzIdentMLXML.ProteinDetectionHypotheses
                                              |> Array.map (fun proteinDetectionhypothesis -> convertToEntity_ProteinDetectionHypothesis dbContext proteinDetectionhypothesis),
                                              mzIdentMLXML.Id
                                             )
        let proteinAmbiguityGroupWithName = setOption ProteinAmbiguityGroupHandler.addName mzIdentMLXML.Name proteinAmbiguityGroupBasic
        let proteinAmbiguityGroupWithDetails = 
            ProteinAmbiguityGroupHandler.addDetails
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> ProteinAmbiguityGroupParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                proteinAmbiguityGroupWithName
        proteinAmbiguityGroupWithDetails

    let convertToEntity_ProteinDetectionList (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.ProteinDetectionList) =
        let poteinDetectionListBasic = ProteinDetectionListHandler.init(mzIdentMLXML.Id)
        let poteinDetectionListWithName = setOption ProteinDetectionListHandler.addName mzIdentMLXML.Name poteinDetectionListBasic
        let poteinDetectionListWithProteinAmbiguityGroups =
            ProteinDetectionListHandler.addProteinAmbiguityGroups
                (mzIdentMLXML.ProteinAmbiguityGroups
                 |> Array.map (fun proteinAmbiguityGroup -> convertToEntity_ProteinAmbiguityGroup dbContext proteinAmbiguityGroup)
                )
                poteinDetectionListWithName
        let poteinDetectionListWithDetails =
            ProteinDetectionListHandler.addDetails
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> ProteinDetectionListParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                )
                poteinDetectionListWithProteinAmbiguityGroups
        poteinDetectionListWithDetails

    let convertToEntity_BiblioGraphicReference (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.BibliographicReference) =
        let biblioGraphicReferenceBasic = BiblioGraphicReferenceHandler.init(mzIdentMLXML.Id)
        let biblioGraphicReferenceWithName = setOption BiblioGraphicReferenceHandler.addName mzIdentMLXML.Name biblioGraphicReferenceBasic
        let biblioGraphicReferenceWithAuthors = setOption BiblioGraphicReferenceHandler.addAuthors mzIdentMLXML.Authors biblioGraphicReferenceWithName
        let biblioGraphicReferenceWithDOI = setOption BiblioGraphicReferenceHandler.addDOI mzIdentMLXML.Doi biblioGraphicReferenceWithAuthors
        let biblioGraphicReferenceWithEditor = setOption BiblioGraphicReferenceHandler.addEditor mzIdentMLXML.Editor biblioGraphicReferenceWithDOI
        let biblioGraphicReferenceWithIssue = setOption BiblioGraphicReferenceHandler.addIssue mzIdentMLXML.Issue biblioGraphicReferenceWithEditor
        let biblioGraphicReferenceWithPages = setOption BiblioGraphicReferenceHandler.addPages mzIdentMLXML.Pages biblioGraphicReferenceWithIssue
        let biblioGraphicReferenceWithPublication = setOption BiblioGraphicReferenceHandler.addPublication mzIdentMLXML.Publication biblioGraphicReferenceWithPages
        let biblioGraphicReferenceWithPublisher = setOption BiblioGraphicReferenceHandler.addPublisher mzIdentMLXML.Publisher biblioGraphicReferenceWithPublication
        let biblioGraphicReferenceWithTitle = setOption BiblioGraphicReferenceHandler.addTitle mzIdentMLXML.Title biblioGraphicReferenceWithPublisher
        let biblioGraphicReferenceWithVolume = setOption BiblioGraphicReferenceHandler.addVolume mzIdentMLXML.Volume biblioGraphicReferenceWithTitle
        let biblioGraphicReferenceWithYear = setOption BiblioGraphicReferenceHandler.addYear mzIdentMLXML.Year biblioGraphicReferenceWithVolume
        biblioGraphicReferenceWithYear

    let convertToEntity_MzIdentML (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.MzIdentMl) =
        let mzIdentMLBasic =  
            MzIdentMLDocumentHandler.init(
                                          //convertToEntity_Inputs dbContext mzIdentMLXML.DataCollection.Inputs,
                                          mzIdentMLXML.Version,
                                          mzIdentMLXML.AnalysisCollection.SpectrumIdentifications
                                          |> Array.map (fun spectrumIdentification -> convertToEntity_SpectrumIdentification dbContext spectrumIdentification),
                                          mzIdentMLXML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
                                          |> Array.map (fun spectrumIdentificationProtocol -> (SpectrumIdentificationProtocolHandler.tryFindByID dbContext spectrumIdentificationProtocol.Id|> (fun item -> match item with
                                                                                                                                                                                                                | Some x -> x
                                                                                                                                                                                                                | None -> null
                                                                                                                                                                                                ))),
                                          mzIdentMLXML.Id
                                         )
        let mzIdentMLWithName = setOption MzIdentMLDocumentHandler.addName mzIdentMLXML.Name mzIdentMLBasic
        let mzIdentMLWithAnalysisSoftwares =
                match mzIdentMLXML.AnalysisSoftwareList with
                    |Some x -> MzIdentMLDocumentHandler.addAnalysisSoftwares      
                                ((x.AnalysisSoftwares
                                    |> Array.map (fun analysisSoftware -> (AnalysisSoftwareHandler.tryFindByID dbContext analysisSoftware.Id|> (fun item -> match item with
                                                                                                                                                            | Some x -> x
                                                                                                                                                            | None -> null
                                                                                                                                                )))
                                )
                                |> Seq.filter (fun item -> item<>null)
                                )
                                mzIdentMLWithName
                    |None -> mzIdentMLWithName
        let mzIdentMLWithPersons =
                match mzIdentMLXML.AuditCollection with
                    |Some x -> MzIdentMLDocumentHandler.addPersons
                                ((x.Persons
                                    |> Array.map (fun person -> (PersonHandler.tryFindByID dbContext person.Id|> (fun item -> match item with
                                                                                                                                | Some x -> x
                                                                                                                                | None -> null
                                                                                                                )))
                                )
                                |> Seq.filter (fun item -> item<>null)
                                )
                                mzIdentMLWithAnalysisSoftwares
                    |None -> mzIdentMLWithAnalysisSoftwares
        let mzIdentMLWithOrganizations =
                match mzIdentMLXML.AuditCollection with
                    |Some x -> MzIdentMLDocumentHandler.addOrganizations
                                ((x.Organizations
                                    |> Array.map (fun organization -> (OrganizationHandler.tryFindByID dbContext organization.Id|> (fun item -> match item with
                                                                                                                                                | Some x -> x
                                                                                                                                                | None -> null
                                                                                                                                    )))
                                )
                                |> Seq.filter (fun item -> item<>null)
                                )
                                mzIdentMLWithPersons
                    |None -> mzIdentMLWithPersons
        let mzIdentMLWithSamples =
                match mzIdentMLXML.AnalysisSampleCollection with
                    |Some x -> MzIdentMLDocumentHandler.addSamples
                                ((x.Samples
                                    |>Array.map (fun sample -> (SampleHandler.tryFindByID dbContext sample.Id|> (fun item -> match item with
                                                                                                                             | Some x -> x
                                                                                                                             | None -> null
                                                                                                                )))
                                )
                                |> Seq.filter (fun item -> item<>null)
                                )
                                mzIdentMLWithOrganizations
                    |None -> mzIdentMLWithOrganizations
        let mzIdentMLWithDBSequences =
                match mzIdentMLXML.SequenceCollection with
                    |Some x -> MzIdentMLDocumentHandler.addDBSequences
                                ((x.DbSequences
                                    |> Array.map (fun dbSequence -> (DBSequenceHandler.tryFindByID dbContext dbSequence.Id|> (fun item -> match item with
                                                                                                                                             | Some x -> x
                                                                                                                                             | None -> null
                                                                                                                            )))
                                ) 
                                |> Seq.filter (fun item -> item<>null)
                                )
                                mzIdentMLWithSamples
                    |None -> mzIdentMLWithSamples
        let mzIdentMLWithPeptides =
                match mzIdentMLXML.SequenceCollection with
                    |Some x -> MzIdentMLDocumentHandler.addPeptides
                                ((x.Peptides
                                    |> Array.map (fun peptide -> (PeptideHandler.tryFindByID dbContext peptide.Id|> (fun item -> match item with
                                                                                                                                    | Some x -> x
                                                                                                                                    | None -> null
                                                                                                                    )))
                                )
                                |> Seq.filter (fun item -> item<>null)
                                )
                                mzIdentMLWithDBSequences
                    |None -> mzIdentMLWithDBSequences
        let mzIdentMLWithPeptideEvidences =
                match mzIdentMLXML.SequenceCollection with
                    |Some x -> MzIdentMLDocumentHandler.addPeptideEvidences
                                ((x.PeptideEvidences
                                    |> Array.map (fun peptideEvidence -> (PeptideEvidenceHandler.tryFindByID dbContext peptideEvidence.Id|> (fun item -> match item with
                                                                                                                                                         | Some x -> x
                                                                                                                                                         | None -> null
                                                                                                                                            )))
                                )
                                |> Seq.filter (fun item -> item<>null)
                                )
                                mzIdentMLWithPeptides
                    |None -> mzIdentMLWithPeptides
        let mzIdentMLWithBiblioGraphicReference =
            MzIdentMLDocumentHandler.addBiblioGraphicReferences
                (mzIdentMLXML.BibliographicReferences
                 |> Array.map (fun bibliographicReference -> convertToEntity_BiblioGraphicReference dbContext bibliographicReference)
                )
                mzIdentMLWithPeptideEvidences
        MzIdentMLDocumentHandler.addToContext dbContext mzIdentMLWithBiblioGraphicReference

    let convertToEntity_AnalysisData (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.AnalysisData) =
        let analysisDataBasic = 
            AnalysisDataHandler.init(
                                     mzIdentMLXML.SpectrumIdentificationList
                                     |> Array.map (fun spectrumIdentificationList -> convertToEntity_SpectrumIdentificationList dbContext spectrumIdentificationList)
                                    )
        let analysisDataWithProteinDetectionList = 
            (match mzIdentMLXML.ProteinDetectionList with
             |Some x -> AnalysisDataHandler.addProteinDetectionList
                            (convertToEntity_ProteinDetectionList dbContext x)
                            analysisDataBasic
             |None -> analysisDataBasic
            )
        let analysisDataWithMzIdentMLDocument =
            AnalysisDataHandler.addMzIdentMLDocument (MzIdentMLDocumentHandler.tryFindByID dbContext "PeptideShaker v1.12.2"|> (fun item -> match item with
                                                                                                                                            | Some x -> x
                                                                                                                                            | None -> null
                                                                                                                               )) analysisDataWithProteinDetectionList
        AnalysisDataHandler.addToContext dbContext analysisDataWithMzIdentMLDocument

    let convertToEntity_Inputs (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.Inputs) =
        let inputsBasic = 
            InputsHandler.init(
                               mzIdentMLXML.SpectraDatas
                               |> Array.map (fun spectraData -> (SpectraDataHandler.tryFindByID dbContext spectraData.Id)|> (fun item -> match item with
                                                                                                                                            | Some x -> x
                                                                                                                                            | None -> null
                                                                                                                            ))
                              )
        let inputsWithSourceFiles =
            InputsHandler.addSourceFiles
                (mzIdentMLXML.SourceFiles
                 |> Array.map (fun sourceFile -> (SourceFileHandler.tryFindByID dbContext sourceFile.Id)|> (fun item -> match item with
                                                                                                                        | Some x -> x
                                                                                                                        | None -> null
                                                                                                                    ))
                )
                inputsBasic
        let inputsWithSearchDatabases =
            InputsHandler.addSearchDatabases
                (mzIdentMLXML.SearchDatabases
                 |> Array.map (fun searchDatabase -> (SearchDatabaseHandler.tryFindByID dbContext searchDatabase.Id)|> (fun item -> match item with
                                                                                                                                    | Some x -> x
                                                                                                                                    | None -> null
                                                                                                                    ))
                )
                inputsWithSourceFiles
        let inputsWithMzIdentMLDocument =
            InputsHandler.addMzIdentMLDocument (MzIdentMLDocumentHandler.tryFindByID dbContext "PeptideShaker v1.12.2"|> (fun item -> match item with
                                                                                                                                        | Some x -> x
                                                                                                                                        | None -> null
                                                                                                                            )) inputsWithSearchDatabases
        InputsHandler.addToContext dbContext inputsWithMzIdentMLDocument

    let convertToEntity_Provider (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.Provider) =
        let providerBasic = ProviderHandler.init(mzIdentMLXML.Id)
        let providerWithName = setOption ProviderHandler.addName mzIdentMLXML.Name providerBasic
        let providerWithAnalysisSoftware =   
            (match mzIdentMLXML.AnalysisSoftwareRef with
             |Some x -> ProviderHandler.addAnalysisSoftware 
                            (AnalysisSoftwareHandler.tryFindByID dbContext x|> (fun item -> match item with
                                                                                            | Some x -> x
                                                                                            | None -> null
                                                                               ))
                            providerWithName
             |None -> providerWithName
            )
        let providerWithContactRole = 
            (match mzIdentMLXML.ContactRole with
             |Some x -> ProviderHandler.addContactRole 
                            (convertToEntity_ContactRole dbContext x)
                            providerWithAnalysisSoftware
             |None -> providerWithAnalysisSoftware
            )
        let providerWithMzIdentML =
            ProviderHandler.addMzIdentMLDocument (MzIdentMLDocumentHandler.tryFindByID dbContext "PeptideShaker v1.12.2"|> (fun item -> match item with
                                                                                                                                        | Some x -> x
                                                                                                                                        | None -> null
                                                                                                                           )) providerWithAnalysisSoftware
        ProviderHandler.addToContext dbContext providerWithMzIdentML

    let convertToEntity_ProteinDetectionProtocol (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.ProteinDetectionProtocol) =
        let proteinDetectionProtocolBasic = 
            ProteinDetectionProtocolHandler.init(
                                                 (AnalysisSoftwareHandler.tryFindByID dbContext mzIdentMLXML.AnalysisSoftwareRef|> (fun item -> match item with
                                                                                                                                                | Some x -> x
                                                                                                                                                | None -> null
                                                                                                                                   )),
                                                 (convertCVandUserParamCollections dbContext mzIdentMLXML.Threshold.CvParams mzIdentMLXML.Threshold.UserParams
                                                  |> Array.map (fun cvParamItem -> ThresholdParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                                                 ),
                                                 mzIdentMLXML.Id
                                                )
        let proteinDetectionProtocolWithName = setOption ProteinDetectionProtocolHandler.addName mzIdentMLXML.Name proteinDetectionProtocolBasic
        let proteinDetectionProtocolWithAnalysisParams =
                match mzIdentMLXML.AnalysisParams with 
                 |Some x -> ProteinDetectionProtocolHandler.addAnalysisParams
                                (convertCVandUserParamCollections dbContext x.CvParams x.UserParams
                                 |> Array.map (fun cvParamItem -> AnalysisParamHandler.init(cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit))
                                )
                                proteinDetectionProtocolWithName
                 |None -> proteinDetectionProtocolWithName
        let proteinDetectionProtocolWithMzIdentMLDocument =
            ProteinDetectionProtocolHandler.addMzIdentMLDocument (MzIdentMLDocumentHandler.tryFindByID dbContext "PeptideShaker v1.12.2"|> (fun item -> match item with
                                                                                                                                                        | Some x -> x
                                                                                                                                                        | None -> null
                                                                                                                                            )) proteinDetectionProtocolWithAnalysisParams
        ProteinDetectionProtocolHandler.addToContext dbContext proteinDetectionProtocolWithMzIdentMLDocument

    let convertToEntity_ProteinDetection (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.ProteinDetection) =
        let proteinDetectionBasic = 
            ProteinDetectionHandler.init(
                                         (ProteinDetectionListHandler.tryFindByID dbContext mzIdentMLXML.ProteinDetectionListRef|> (fun item -> match item with
                                                                                                                                                | Some x -> x
                                                                                                                                                | None -> null
                                                                                                                                    )),
                                         (ProteinDetectionProtocolHandler.tryFindByID dbContext mzIdentMLXML.ProteinDetectionProtocolRef|> (fun item -> match item with
                                                                                                                                                        | Some x -> x
                                                                                                                                                        | None -> null
                                                                                                                                            )),
                                         mzIdentMLXML.InputSpectrumIdentifications
                                         |> Array.map (fun spectrumIdentificationList -> (SpectrumIdentificationListHandler.tryFindByID dbContext spectrumIdentificationList.SpectrumIdentificationListRef|> (fun item -> match item with
                                                                                                                                                                                                                            | Some x -> x
                                                                                                                                                                                                                            | None -> null
                                                                                                                                                                                                               ))),
                                         mzIdentMLXML.Id
                                        )
        let proteinDetectionWithName = 
            setOption ProteinDetectionHandler.addName mzIdentMLXML.Name proteinDetectionBasic
        let proteinDetectionWithActivityDate = 
            setOption ProteinDetectionHandler.addActivityDate mzIdentMLXML.ActivityDate proteinDetectionWithName
        let proteinDetectionWithMzIdentML =
            ProteinDetectionHandler.addMzIdentMLDocument (MzIdentMLDocumentHandler.tryFindByID dbContext "PeptideShaker v1.12.2"|> (fun item -> match item with
                                                                                                                                                | Some x -> x
                                                                                                                                                | None -> null
                                                                                                                                   )) proteinDetectionWithActivityDate
        ProteinDetectionHandler.addToContext dbContext proteinDetectionWithMzIdentML

    let removeDoubleTerms (xmlMzIdentML : SchemePeptideShaker.MzIdentMl) =
        let cvParamAccessionListOfXMLFile =
            [|
            (if xmlMzIdentML.AnalysisProtocolCollection.ProteinDetectionProtocol.IsSome 
                then xmlMzIdentML.AnalysisProtocolCollection.ProteinDetectionProtocol.Value.Threshold.CvParams
                    |> Array.map (fun cvParamItem -> cvParamItem.Accession)
                else [|null|])

            (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
                |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.AdditionalSearchParams.IsSome 
                                                                            then spectrumIdentificationProtocolItem.AdditionalSearchParams. Value.CvParams
                                                                                |> Array.map (fun cvParamItem -> cvParamItem.Accession)

                                                                            else [|null|]))
                                        |> Array.concat

            (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
                |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.Enzymes.IsSome 
                                                                            then 
                                                                                if spectrumIdentificationProtocolItem.Enzymes.IsSome 
                                                                                    then spectrumIdentificationProtocolItem.Enzymes.Value.Enzymes
                                                                                         |> Array.map (fun enzymeItem -> enzymeItem.EnzymeName.Value.CvParams
                                                                                                                         |> Array.map (fun cvParamitem -> cvParamitem.Accession))
                                                                                    else [|[|null|]|]
                                                                            else [|[|null|]|]))
                                        |> Array.concat
                                        |> Array.concat

            (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
                |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.FragmentTolerance.IsSome
                                                                            then spectrumIdentificationProtocolItem.FragmentTolerance.Value.CvParams
                                                                                 |> Array.map (fun cvParamItem -> cvParamItem.Accession)
                                                                            else [|null|]))
                                        |> Array.concat

            (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
                |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.ModificationParams.IsSome 
                                                                            then spectrumIdentificationProtocolItem.ModificationParams.Value.SearchModifications
                                                                                 |> Array.map (fun searchModificationItem -> searchModificationItem.SpecificityRules
                                                                                                                                |> Array.map (fun specificityRuleItem -> specificityRuleItem.CvParams
                                                                                                                                                                        |> Array.map (fun cvParamitem -> cvParamitem.Accession)))
                                                                            else [|[|[|null|]|]|]))
                                        |> Array.concat
                                        |> Array.concat
                                        |> Array.concat

            (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
                |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.ParentTolerance.IsSome 
                                                                            then spectrumIdentificationProtocolItem.ParentTolerance.Value.CvParams
                                                                                |> Array.map (fun cvParamItem -> cvParamItem.Accession)
                                                                            else [|null|]))
                                        |> Array.concat

            (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
            |> Array.map (fun spectrumIdentificationProtocolItem -> spectrumIdentificationProtocolItem.Threshold.CvParams
                                                                    |> Array.map (fun cvParamItem -> cvParamItem.Accession)))
                                        |> Array.concat

            (if xmlMzIdentML.AnalysisSoftwareList.IsSome 
                then xmlMzIdentML.AnalysisSoftwareList.Value.AnalysisSoftwares
                    |> Array.map (fun analysisSoftwareItem -> if analysisSoftwareItem.ContactRole.IsSome 
                                                                 then analysisSoftwareItem.ContactRole.Value.Role.CvParam.Accession
                                                                 else null)
                else [|null|])

            (if xmlMzIdentML.AnalysisSoftwareList.IsSome 
            then xmlMzIdentML.AnalysisSoftwareList.Value.AnalysisSoftwares
              |> Array.map (fun analysisSoftwareItem -> if analysisSoftwareItem.SoftwareName.CvParam.IsSome 
                                                            then analysisSoftwareItem.SoftwareName.CvParam.Value.Accession
                                                            else null)
            else [|null|])

            (if xmlMzIdentML.AuditCollection.IsSome 
            then xmlMzIdentML.AuditCollection.Value.Organizations
              |> Array.map (fun orgItem -> orgItem.CvParams
                                            |> Array.map (fun cvParamItem -> cvParamItem.Accession))
                                |> Array.concat
            else [|null|])

            (if xmlMzIdentML.AuditCollection.IsSome 
                then xmlMzIdentML.AuditCollection.Value.Persons
                    |> Array.map (fun personItem -> personItem.CvParams
                                                    |> Array.map (fun cvParamItem -> cvParamItem.Accession))
                                                    |> Array.concat
            else [|null|])

            (if xmlMzIdentML.DataCollection.AnalysisData.ProteinDetectionList.IsSome 
                then xmlMzIdentML.DataCollection.AnalysisData.ProteinDetectionList.Value.CvParams
                    |> Array.map (fun cvParamItem -> cvParamItem.Accession)
                else [|null|])

            (if xmlMzIdentML.DataCollection.AnalysisData.ProteinDetectionList.IsSome 
                then xmlMzIdentML.DataCollection.AnalysisData.ProteinDetectionList.Value.ProteinAmbiguityGroups
                    |> Array.map (fun proteinAmbiguityGroupsItem -> proteinAmbiguityGroupsItem.CvParams
                                                                    |> Array.map (fun cvParamItem -> cvParamItem.Accession))
                                                                    |> Array.concat
                else [|null|])

            (xmlMzIdentML.DataCollection.AnalysisData.SpectrumIdentificationList
                |> Array.map (fun spectrumIdentificationitem -> if spectrumIdentificationitem.FragmentationTable.IsSome
                                                                    then spectrumIdentificationitem.FragmentationTable.Value.Measures
                                                                        |> Array.map (fun measureItem -> measureItem.CvParams
                                                                                                         |> Array.map (fun cvParamItem -> cvParamItem.Accession))
                                                                    else [|[|null|]|])
                                                     |> Array.concat
                                                     |> Array.concat)

            (xmlMzIdentML.DataCollection.AnalysisData.SpectrumIdentificationList
                |> Array.map (fun spectrumIdentificationItem -> spectrumIdentificationItem.SpectrumIdentificationResults
                                                                |> Array.map (fun spectrumIdentificationResultItem -> spectrumIdentificationResultItem.CvParams
                                                                                                                        |> Array.map (fun cvParamItem -> cvParamItem.Accession)))
                                                                  |> Array.concat
                                                                  |> Array.concat)

            (if xmlMzIdentML.SequenceCollection.IsSome
                then xmlMzIdentML.SequenceCollection.Value.DbSequences
                     |> Array.map (fun dbSeqItem -> dbSeqItem.CvParams
                                                    |> Array.map (fun cvParamitem -> cvParamitem.Accession))
                                                    |> Array.concat
                else [|null|])

            (if xmlMzIdentML.SequenceCollection.IsSome 
                then xmlMzIdentML.SequenceCollection.Value.Peptides
                    |> Array.map (fun peptideItem -> peptideItem.Modifications
                                                        |> Array.map (fun modificationItem -> modificationItem.CvParams
                                                                                                |> Array.map (fun cvParamItem -> cvParamItem.Accession)))
                                                    |> Array.concat
                                                    |> Array.concat
            else [|null|])
        |]
        cvParamAccessionListOfXMLFile
        |> Array.concat

    let keepDuplicates collection =
        let d = System.Collections.Generic.Dictionary()
        [| for i in collection do match d.TryGetValue i with
                                  | (false,_) -> d.[i] <- (); yield i
                                  | _ -> () |]