namespace MzIdentMLDataBase

open System
open DataContext.EntityTypes
open DataContext.DataContext
open InsertStatements.ObjectHandlers
//open System.Collections.Generic
//open FSharp.Care.IO
//open BioFSharp.IO


module XMLParsing =

    open FSharp.Data

    type SchemePeptideShaker = XmlProvider<Schema = "..\MzIdentMLDB_Library\XML_Files\MzIdentMLScheme1_2.xsd">
    //let  samplePeptideShaker = SchemePeptideShaker.Load("..\ExampleFile\PeptideShaker_mzid_1_2_example.txt") 



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
        let initialized = CVParamHandler.init(mzIdentMLXML.Name, TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))
        let addedValue = setOption CVParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = CVParamHandler.addUnit (match mzIdentMLXML.UnitAccession with
                                                |Some x -> TermHandler.tryFindByID dbContext x 
                                                |None -> TermHandler.tryFindByID dbContext ""
                                               ) addedValue
        let addedUnitName = setOption CVParamHandler.addUnitName mzIdentMLXML.UnitName addedUnit
        addedUnitName

    let convertToEntityModificationParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = ModificationParamHandler.init(mzIdentMLXML.Name, TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))
        let addedValue = setOption ModificationParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = ModificationParamHandler.addUnit (match mzIdentMLXML.UnitAccession with
                                                                 |Some x -> TermHandler.tryFindByID dbContext x 
                                                                 |None -> TermHandler.tryFindByID dbContext ""
                                                                ) addedValue
        let addedUnitName = setOption ModificationParamHandler.addUnitName mzIdentMLXML.UnitName addedUnit
        addedUnitName

    let convertToEntityTranslationTableParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = TranslationTableParamHandler.init(mzIdentMLXML.Name, TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))
        let addedValue = setOption TranslationTableParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = TranslationTableParamHandler.addUnit (match mzIdentMLXML.UnitAccession with
                                                              |Some x -> TermHandler.tryFindByID dbContext x 
                                                              |None -> TermHandler.tryFindByID dbContext ""
                                                             ) addedValue
        let addUnitName = setOption TranslationTableParamHandler.addUnitName mzIdentMLXML.UnitName addedUnit
        addUnitName

    let convertToEntityMeasureParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = MeasureParamHandler.init(mzIdentMLXML.Name, TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))
        let addedValue = setOption MeasureParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = MeasureParamHandler.addUnit (match mzIdentMLXML.UnitAccession with
                                                     |Some x -> TermHandler.tryFindByID dbContext x
                                                     |None -> TermHandler.tryFindByID dbContext ""
                                                    ) addedValue
        let addedUnitName = setOption MeasureParamHandler.addUnitName mzIdentMLXML.UnitName addedUnit
        addedUnitName

    let convertToEntitySearchModificationParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = SearchModificationParamHandler.init(mzIdentMLXML.Name, TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))
        let addedValue = setOption SearchModificationParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = SearchModificationParamHandler.addUnit (match mzIdentMLXML.UnitAccession with
                                                                |Some x -> TermHandler.tryFindByID dbContext x
                                                                |None -> TermHandler.tryFindByID dbContext ""
                                                               ) addedValue
        let addedUnitName = setOption SearchModificationParamHandler.addUnitName mzIdentMLXML.UnitName addedUnit
        addedUnitName

    let convertToEntitySpecificityRuleParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = SpecificityRuleParamHandler.init(mzIdentMLXML.Name, TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))
        let addedValue = setOption SpecificityRuleParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = SpecificityRuleParamHandler.addUnit (match mzIdentMLXML.UnitAccession with
                                                             |Some x -> TermHandler.tryFindByID dbContext x
                                                             |None -> TermHandler.tryFindByID dbContext ""
                                                            ) addedValue
        let addedUnitName = setOption SpecificityRuleParamHandler.addUnitName mzIdentMLXML.UnitName addedUnit
        addedUnitName

    let convertToEntityFragmentToleranceParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = FragmentToleranceParamHandler.init(mzIdentMLXML.Name, TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))
        let addedValue = setOption FragmentToleranceParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = FragmentToleranceParamHandler.addUnit (match mzIdentMLXML.UnitAccession with
                                                               |Some x -> TermHandler.tryFindByID dbContext x
                                                               |None -> TermHandler.tryFindByID dbContext ""
                                                              ) addedValue
        let addedUnitName = setOption FragmentToleranceParamHandler.addUnitName mzIdentMLXML.UnitName addedUnit
        addedUnitName

    let convertToEntityParentToleranceParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = ParentToleranceParamHandler.init(mzIdentMLXML.Name, TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))
        let addedValue = setOption ParentToleranceParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = ParentToleranceParamHandler.addUnit (match mzIdentMLXML.UnitAccession with
                                                             |Some x -> TermHandler.tryFindByID dbContext x
                                                             |None -> TermHandler.tryFindByID dbContext ""
                                                            ) addedValue
        let addedUnitName = setOption ParentToleranceParamHandler.addUnitName mzIdentMLXML.UnitName addedUnit
        addedUnitName

    let convertToEntitySearchDatabaseParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.CvParam) =
        let initialized = SearchDatabaseParamHandler.init(mzIdentMLXML.Name, TermHandler.tryFindByID dbContext (mzIdentMLXML.Accession))
        let addedValue = setOption SearchDatabaseParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = SearchDatabaseParamHandler.addUnit (match mzIdentMLXML.UnitAccession with
                                                            |Some x -> TermHandler.tryFindByID dbContext x
                                                            |None -> TermHandler.tryFindByID dbContext ""
                                                           ) addedValue
        let addedUnitName = setOption SearchDatabaseParamHandler.addUnitName mzIdentMLXML.UnitName addedUnit
        addedUnitName

    let convertToEntity_UserParam (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.UserParam) =
        let initialized = CVParamHandler.init(mzIdentMLXML.Name, (match mzIdentMLXML.UnitAccession with
                                                                  |Some x ->  TermHandler.tryFindByID dbContext x
                                                                  |None -> TermHandler.tryFindByID dbContext ""
                                                                 )
                                             )
        let addedValue = setOption CVParamHandler.addValue mzIdentMLXML.Value initialized
        let addedUnit = CVParamHandler.addUnit (takeTermEntryOption dbContext mzIdentMLXML.UnitAccession) addedValue
        let addedUnitName = setOption CVParamHandler.addUnitName mzIdentMLXML.UnitName addedUnit
        addedUnitName

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
                  |> Array.map (fun cvParamItem -> OrganizationParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                )
                organizationWithName
        let organizationWithParent = 
            OrganizationHandler.addParent 
                (match mzIdentMLXML.Parent with
                 |Some x -> x.OrganizationRef
                 |None -> ""
                )
                organizationWithDetails
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
                 |> Array.map (fun cvParamItem -> PersonParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                )
                personWithLastName
        let personWithOrganizations = 
            PersonHandler.addOrganizations 
                (mzIdentMLXML.Affiliations 
                    |> Array.map (fun item -> OrganizationHandler.tryFindByID dbContext item.OrganizationRef)
                )
                personWithDetails
        PersonHandler.addToContext dbContext personWithOrganizations

    let convertToEntity_ContactRole (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.ContactRole) =
        let contactRoleBasic = ContactRoleHandler.init(PersonHandler.tryFindByID dbContext mzIdentMLXML.ContactRef, 
                                                       convertToEntityCVParam dbContext mzIdentMLXML.Role.CvParam,
                                                       mzIdentMLXML.ContactRef
                                                      )
        contactRoleBasic
        //ContactRoleHandler.addToContext dbContext contactRoleBasic

    let convertToEntity_AnalysisSoftware (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.AnalysisSoftware) =
        let analysisSoftwareBasic = AnalysisSoftwareHandler.init((chooseUserOrCVParamOption dbContext mzIdentMLXML.SoftwareName.CvParam mzIdentMLXML.SoftwareName.UserParam))
        let analysisSoftwareWithname = setOption AnalysisSoftwareHandler.addName mzIdentMLXML.Name analysisSoftwareBasic
        let analysisSoftwareWithURI = setOption AnalysisSoftwareHandler.addURI mzIdentMLXML.Uri analysisSoftwareWithname
        let analysisSoftwareWithVersion = setOption AnalysisSoftwareHandler.addVersion mzIdentMLXML.Version analysisSoftwareWithURI
        let analysisSoftwareWithCustomizations = setOption AnalysisSoftwareHandler.addCustomization mzIdentMLXML.Customizations analysisSoftwareWithVersion
        let analysisSoftwareWithSoftwareDeveloper = AnalysisSoftwareHandler.addAnalysisSoftwareDeveloper (convertOptionToEntity convertToEntity_ContactRole dbContext mzIdentMLXML.ContactRole) analysisSoftwareWithCustomizations
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
                 |> Array.map (fun cvParamItem -> SampleParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                )
                sampleWithContactRoles
        SampleHandler.addToContext dbContext sampleWithDetails

    let convertToEntity_SubSample (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SubSample) =
        let subSampleBasic = SubSampleHandler.init()
        let subSampleWithSample = SubSampleHandler.addSample (SampleHandler.tryFindByID dbContext mzIdentMLXML.SampleRef) subSampleBasic
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
                 |> Array.map (fun cvParamItem ->PeptideParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
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
                                                             |> Array.map (fun cvParamItem -> AmbiguousResidueParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
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
                 |> Array.map (fun cvParamItem -> MassTableParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                )
                massTableWithAmbigousResidues
        MassTableHandler.addToContext dbContext massTableWithDetails

//Stup because there is a problem with the xmlFile with the values of the FragmentArray/////////////////
    let convertToEntity_FragmentArray (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.FragmentArray) =
        let fragmentArrayBasic = FragmentArrayHandler.init(
                                                           MeasureHandler.tryFindByID dbContext mzIdentMLXML.MeasureRef, 
                                                           [ValueHandler.init (0.)]
                                                          )
        fragmentArrayBasic 

    let convertToEntity_IonType (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.IonType) =
        let ionTypeBasic = IonTypeHandler.init(
                                               (convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams
                                                |> Array.map (fun cvParamItem -> IonTypeParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
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
                                                   PeptideHandler.tryFindByID dbContext mzIdentMLXML.PeptideRef,
                                                   mzIdentMLXML.ChargeState, 
                                                   mzIdentMLXML.ExperimentalMassToCharge, 
                                                   mzIdentMLXML.PassThreshold,
                                                   mzIdentMLXML.Rank,
                                                   mzIdentMLXML.Id
                                                  )
        let spectrumIdentificationItemWithName = setOption SpectrumIdentificationItemHandler.addName mzIdentMLXML.Name spectrumIdentificationItemBasic
        let spectrumIdentificationItemWithSample = 
            SpectrumIdentificationItemHandler.addSample  
                (match mzIdentMLXML.SampleRef with 
                 |Some x -> SampleHandler.tryFindByID dbContext x
                 |None -> Unchecked.defaultof<Sample>
                )
                spectrumIdentificationItemWithName
        let spectrumIdentificationItemWithMassTable = 
            SpectrumIdentificationItemHandler.addMassTable 
                (match mzIdentMLXML.MassTableRef with
                 |Some x -> MassTableHandler.tryFindByID dbContext x
                 |None -> Unchecked.defaultof<MassTable>
                )
                spectrumIdentificationItemWithSample
        let spectrumIdentificationItemWithPeptideEvidence =
            SpectrumIdentificationItemHandler.addPeptideEvidences
                (mzIdentMLXML.PeptideEvidenceRefs
                 |> Array.map (fun peptideEvidenceRef -> PeptideEvidenceHandler.tryFindByID dbContext peptideEvidenceRef.PeptideEvidenceRef)
                )
                spectrumIdentificationItemWithMassTable
        let spectrumIdentificationItemWithFragmentation =
                match mzIdentMLXML.Fragmentation with
                 |Some x -> SpectrumIdentificationItemHandler.addFragmentations 
                                (x.IonTypes
                                 |> Array.map (fun ionType -> convertToEntity_IonType dbContext ionType)
                                )
                                spectrumIdentificationItemWithPeptideEvidence
                 |None -> Unchecked.defaultof<SpectrumIdentificationItem>
        let spectrumIdentificationItemWithCalculatedMassToCharge =
            setOption SpectrumIdentificationItemHandler.addCalculatedMassToCharge mzIdentMLXML.CalculatedMassToCharge spectrumIdentificationItemWithFragmentation
        let spectrumIdentificationItemWithCalculatedPI =
            SpectrumIdentificationItemHandler.addCalculatedPI 
                    (match mzIdentMLXML.CalculatedPi with
                     |Some x -> float x
                     |None -> Unchecked.defaultof<float>
                    )
                    spectrumIdentificationItemWithCalculatedMassToCharge
        let spectrumIdentificationItemWithDetails =
            SpectrumIdentificationItemHandler.addDetails
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> SpectrumIdentificationItemParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                )
                spectrumIdentificationItemWithCalculatedPI
        spectrumIdentificationItemWithDetails

    let convertToEntity_SpectrumIdentificationResult (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SpectrumIdentificationResult) =
        let spectrumIdentificationResultBasic = 
            SpectrumIdentificationResultHandler.init(
                                                     SpectraDataHandler.tryFindByID dbContext mzIdentMLXML.SpectraDataRef,
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
                 |> Array.map (fun cvParamItem -> SpectrumIdentificationResultParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
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
                 |None -> Unchecked.defaultof<SpectrumIdentificationList>
        let spectrumIdentificationListWithDetails =
            SpectrumIdentificationListHandler.addDetails   
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> SpectrumIdentificationListParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                )
                spectrumIdentificationListWithFragmentationTable
        spectrumIdentificationListWithDetails

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
        spectraDataWithexternalDataFormat

    let convertToEntity_SpecificityRule (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SpecificityRules) =
        let specificityRuleBasic = SpecificityRulesHandler.init(
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
        let enzymeWithSiteReg = setOption EnzymeHandler.addSiteRegexc mzIdentMLXML.SiteRegexp enzymeWithSemiSpecific
        let enzymeWithEnzymeNames = 
                match mzIdentMLXML.EnzymeName with
                 |Some x -> EnzymeHandler.addEnzymeNames
                                (convertCVandUserParamCollections dbContext x.CvParams x.UserParams
                                 |> Array.map (fun cvParamItem -> EnzymeNameParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                                )
                                enzymeWithSiteReg
                 |None -> Unchecked.defaultof<Enzyme>
        EnzymeHandler.addToContext dbContext enzymeWithEnzymeNames

    let convertToEntity_Filter (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.Filter) =
        let filterBasic = 
            FilterHandler.init(chooseUserOrCVParamOption dbContext mzIdentMLXML.FilterType.CvParam mzIdentMLXML.FilterType.UserParam)
        let filterWithIncludes = 
            FilterHandler.addIncludes 
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.Include.Value.CvParams mzIdentMLXML.Include.Value.UserParams)
                 |> Array.map (fun cvParamItem -> IncludeParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                )
                filterBasic
        let filterWithExcludes = 
            FilterHandler.addExcludes
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.Exclude.Value.CvParams mzIdentMLXML.Exclude.Value.UserParams)
                 |> Array.map (fun cvParamItem -> ExcludeParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                )
                filterWithIncludes
        filterWithExcludes

    let convertToEntity_SpectrumIdentificationProtocol (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SpectrumIdentificationProtocol) =
        let spectrumIdentificationProtocolBasic =
            SpectrumIdentificationProtocolHandler.init
                (
                 AnalysisSoftwareHandler.tryFindByID dbContext mzIdentMLXML.AnalysisSoftwareRef, 
                 chooseUserOrCVParamOption dbContext mzIdentMLXML.SearchType.CvParam mzIdentMLXML.SearchType.UserParam, 
                 (convertCVandUserParamCollections dbContext mzIdentMLXML.Threshold.CvParams mzIdentMLXML.Threshold.UserParams
                  |> Array.map (fun cvParamItem -> ThresholdParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                 ),
                 mzIdentMLXML.Id
                )
        let spectrumIdentificationProtocolWithName = setOption SpectrumIdentificationProtocolHandler.addName mzIdentMLXML.Name spectrumIdentificationProtocolBasic
        let spectrumIdentificationProtocolWithAdditionalSearchParams = 
                match mzIdentMLXML.AdditionalSearchParams with
                 |Some x -> SpectrumIdentificationProtocolHandler.addEnzymeAdditionalSearchParams
                                (convertCVandUserParamCollections dbContext x.CvParams x.UserParams
                                 |> Array.map (fun cvParamItem -> AdditionalSearchParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                                )
                                spectrumIdentificationProtocolWithName
                 |None -> Unchecked.defaultof<SpectrumIdentificationProtocol>
        let spectrumIdentificationProtocolWithModificationParams = 
                match mzIdentMLXML.ModificationParams with
                 |Some x -> SpectrumIdentificationProtocolHandler.addModificationParams
                                (x.SearchModifications 
                                 |> Array.map (fun searchModification -> convertToEntity_SearchModification dbContext searchModification)
                                )
                                spectrumIdentificationProtocolWithAdditionalSearchParams
                 |None -> Unchecked.defaultof<SpectrumIdentificationProtocol>
        let spectrumIdentificationProtocolWithEnzymes =
                match mzIdentMLXML.Enzymes with 
                 |Some x -> SpectrumIdentificationProtocolHandler.addEnzymes 
                                (x.Enzymes 
                                 |> Array.map (fun enzyme -> EnzymeHandler.tryFindByID dbContext enzyme.Id)
                                )
                                spectrumIdentificationProtocolWithModificationParams
                 |None -> Unchecked.defaultof<SpectrumIdentificationProtocol>
        let spectrumIdentificationProtocolWithEnzymesWithIndepent_Enzymes = 
            SpectrumIdentificationProtocolHandler.addIndependent_Enzymes  
                (match mzIdentMLXML.Enzymes with
                    |Some x -> match x.Independent with
                               |Some x -> x
                               |None -> Unchecked.defaultof<bool>
                    |None -> Unchecked.defaultof<bool>
                )
                spectrumIdentificationProtocolWithEnzymes
        let spectrumIdentificationProtocolWithMassTables =
            SpectrumIdentificationProtocolHandler.addMassTables
                (mzIdentMLXML.MassTables
                 |> Array.map (fun massTable -> MassTableHandler.tryFindByID dbContext massTable.Id)
                )
                spectrumIdentificationProtocolWithEnzymesWithIndepent_Enzymes
        let spectrumIdentificationProtocolWithFragmentTolerances =
                match mzIdentMLXML.FragmentTolerance with
                 |Some x -> SpectrumIdentificationProtocolHandler.addFragmentTolerances
                                (x.CvParams 
                                 |> Array.map (fun cvParam -> convertToEntityFragmentToleranceParam dbContext cvParam)
                                )
                                spectrumIdentificationProtocolWithMassTables
                 |None -> Unchecked.defaultof<SpectrumIdentificationProtocol>
        let spectrumIdentificationProtocolWithParentTolerances =
                match mzIdentMLXML.ParentTolerance with
                 |Some x -> SpectrumIdentificationProtocolHandler.addParentTolerances
                                (x.CvParams 
                                 |> Array.map (fun cvParam -> convertToEntityParentToleranceParam dbContext cvParam)
                                )
                                spectrumIdentificationProtocolWithFragmentTolerances
                 |None -> Unchecked.defaultof<SpectrumIdentificationProtocol>
        let spectrumIdentificationProtocolWithDatabaseFilter = 
                match mzIdentMLXML.DatabaseFilters with
                 |Some x -> SpectrumIdentificationProtocolHandler.addDatabaseFilters
                                (x.Filters 
                                 |> Array.map (fun filter -> convertToEntity_Filter dbContext filter)
                                )
                                spectrumIdentificationProtocolWithParentTolerances
                 |None -> Unchecked.defaultof<SpectrumIdentificationProtocol>
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
                 |None -> Unchecked.defaultof<SpectrumIdentificationProtocol>
        SpectrumIdentificationProtocolHandler.addToContext dbContext spectrumIdentificationProtocolWithTranslationTable

    let convertToEntity_SearchDatabase (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SearchDatabase) =
        let searchDatabaseBasic = 
            SearchDatabaseHandler.init(
                                       mzIdentMLXML.Location, 
                                       convertToEntityCVParam dbContext mzIdentMLXML.FileFormat.CvParam,
                                       chooseUserOrCVParamOption dbContext mzIdentMLXML.DatabaseName.CvParam mzIdentMLXML.DatabaseName.UserParam,
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
        searchDatabaseWithDetails


    let convertToEntity_DBSequence (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.DbSequence) =
        let dbSequenceBasic = 
            DBSequenceHandler.init(
                                   mzIdentMLXML.Accession, 
                                   SearchDatabaseHandler.tryFindByID dbContext mzIdentMLXML.SearchDatabaseRef,
                                   mzIdentMLXML.Id
                                  )
        let dbSequenceWithName = setOption DBSequenceHandler.addName mzIdentMLXML.Name dbSequenceBasic
        let dbSequenceWithSequence = setOption DBSequenceHandler.addSequence mzIdentMLXML.Seq dbSequenceWithName
        let dbSequenceWithLength = setOption DBSequenceHandler.addLength mzIdentMLXML.Length dbSequenceWithSequence
        let dbSequenceWithDetails = 
            DBSequenceHandler.addDetails 
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> DBSequenceParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                )
                dbSequenceWithLength
        DBSequenceHandler.addToContext dbContext dbSequenceWithDetails

    let convertToEntity_PeptideEvidence (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.PeptideEvidence) =
        let peptideEvidenceBasic = 
            PeptideEvidenceHandler.init(
                                        DBSequenceHandler.tryFindByID dbContext mzIdentMLXML.DBSequenceRef,
                                        PeptideHandler.tryFindByID dbContext mzIdentMLXML.PeptideRef,
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
                                   |None -> Unchecked.defaultof<int>
                                  )
                )
                peptideEvidenceWithPost
        let peptideEvidenceWithIsDecoy = setOption PeptideEvidenceHandler.addIsDecoy mzIdentMLXML.IsDecoy peptideEvidenceWithFrame
        let peptideEvidenceWithTranslationTable = 
            PeptideEvidenceHandler.addTranslationTable 
                (match mzIdentMLXML.TranslationTableRef with
                 |Some x -> TranslationTableHandler.tryFindByID dbContext x
                 |None -> Unchecked.defaultof<TranslationTable>
                )
                peptideEvidenceWithIsDecoy
        let peptideEvidenceWithDetails =
            PeptideEvidenceHandler.addDetails 
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> PeptideEvidenceParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
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
                 |> Array.map (fun cvParamItem -> SourceFileParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                )
                sourceFileWithExternalFormatDocumentation
        sourceFileWithDetails

    let convertToEntity_Inputs (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.Inputs) =
        let inputsBasic = 
            InputsHandler.init(
                               mzIdentMLXML.SpectraDatas
                               |> Array.map (fun spectraData -> convertToEntity_SpectraData dbContext spectraData)
                              )
        let inputsWithSourceFiles =
            InputsHandler.addSourceFiles
                (mzIdentMLXML.SourceFiles
                 |> Array.map (fun sourceFile -> convertToEntity_SourceFile dbContext sourceFile)
                )
                inputsBasic
        let inputsWithSearchDatabases =
            InputsHandler.addSearchDatabases
                (mzIdentMLXML.SearchDatabases
                 |> Array.map (fun searchDatabase -> convertToEntity_SearchDatabase dbContext searchDatabase)
                )
                inputsWithSourceFiles
        inputsWithSearchDatabases

    let convertToEntity_SpectrumIdentification (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.SpectrumIdentification) =
        let spectrumIdentificationBasic = 
            SpectrumIdentificationHandler.init(
                                               SpectrumIdentificationListHandler.tryFindByID dbContext mzIdentMLXML.SpectrumIdentificationListRef,
                                               SpectrumIdentificationProtocolHandler.tryFindByID dbContext mzIdentMLXML.SpectrumIdentificationProtocolRef,
                                               mzIdentMLXML.InputSpectras
                                               |> Array.map (fun inputSpectra -> match inputSpectra.SpectraDataRef with
                                                                                 |Some x -> SpectraDataHandler.tryFindByID dbContext x
                                                                                 |None -> Unchecked.defaultof<SpectraData>
                                                            ),
                                               mzIdentMLXML.SearchDatabaseRefs
                                               |> Array.map (fun searchDatabaseRef -> match searchDatabaseRef.SearchDatabaseRef with
                                                                                      |Some x -> SearchDatabaseHandler.tryFindByID dbContext x
                                                                                      |None -> Unchecked.defaultof<SearchDatabase>
                                                            ),
                                               mzIdentMLXML.Id
                                              )
        let spectrumIdentificationWithName = setOption SpectrumIdentificationHandler.addName mzIdentMLXML.Name spectrumIdentificationBasic
        let spectrumIdentificationWithActivityDate = setOption SpectrumIdentificationHandler.addActivityDate mzIdentMLXML.ActivityDate spectrumIdentificationWithName
        spectrumIdentificationWithActivityDate

    let convertToEntity_ProteinDetectionProtocol (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.ProteinDetectionProtocol) =
        let proteinDetectionProtocolBasic = 
            ProteinDetectionProtocolHandler.init(
                                                 AnalysisSoftwareHandler.tryFindByID dbContext mzIdentMLXML.AnalysisSoftwareRef,
                                                 (convertCVandUserParamCollections dbContext mzIdentMLXML.Threshold.CvParams mzIdentMLXML.Threshold.UserParams
                                                  |> Array.map (fun cvParamItem -> ThresholdParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                                                 ),
                                                 mzIdentMLXML.Id
                                                )
        let proteinDetectionProtocolWithName = setOption ProteinDetectionProtocolHandler.addName mzIdentMLXML.Name proteinDetectionProtocolBasic
        let proteinDetectionProtocolWithAnalysisParams =
                match mzIdentMLXML.AnalysisParams with 
                 |Some x -> ProteinDetectionProtocolHandler.addAnalysisParams
                                (convertCVandUserParamCollections dbContext x.CvParams x.UserParams
                                 |> Array.map (fun cvParamItem -> AnalysisParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                                )
                                proteinDetectionProtocolWithName
                 |None -> Unchecked.defaultof<ProteinDetectionProtocol>
        ProteinDetectionProtocolHandler.addToContext dbContext proteinDetectionProtocolWithAnalysisParams

    let convertToEntity_PeptideHypothesis (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.PeptideHypothesis) =
        let peptideHypothesisBasic = 
            PeptideHypothesisHandler.init(
                                          PeptideEvidenceHandler.tryFindByID dbContext mzIdentMLXML.PeptideEvidenceRef,
                                          mzIdentMLXML.SpectrumIdentificationItemRefs
                                          |> Array.map (fun spectrumIdentificationItemRef -> SpectrumIdentificationItemHandler.tryFindByID dbContext spectrumIdentificationItemRef.SpectrumIdentificationItemRef)
                                         )
        peptideHypothesisBasic

    let convertToEntity_ProteinDetectionHypothesis (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.ProteinDetectionHypothesis) =
        let proteinDetectionHypothesisBasic = 
            ProteinDetectionHypothesisHandler.init(
                                                   mzIdentMLXML.PassThreshold, 
                                                   DBSequenceHandler.tryFindByID dbContext mzIdentMLXML.DBSequenceRef,
                                                   mzIdentMLXML.PeptideHypotheses
                                                   |> Array.map (fun peptideHypothesis -> convertToEntity_PeptideHypothesis dbContext peptideHypothesis),
                                                   mzIdentMLXML.Id
                                                  )
        let proteinDetectionHypothesisWithName = setOption ProteinDetectionHypothesisHandler.addName mzIdentMLXML.Name proteinDetectionHypothesisBasic
        let proteinDetectionHypothesisWithDetails = 
            ProteinDetectionHypothesisHandler.addDetails 
                ((convertCVandUserParamCollections dbContext mzIdentMLXML.CvParams mzIdentMLXML.UserParams)
                 |> Array.map (fun cvParamItem -> ProteinDetectionHypothesisParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
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
                 |> Array.map (fun cvParamItem -> ProteinAmbiguityGroupParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
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
                 |> Array.map (fun cvParamItem -> ProteinDetectionListParamHandler.init(cvParamItem.Name, cvParamItem.Term, cvParamItem.ID, cvParamItem.Value, cvParamItem.Unit, cvParamItem.UnitName))
                )
                poteinDetectionListWithProteinAmbiguityGroups
        poteinDetectionListWithDetails

    let convertToEntity_AnalysisData (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.AnalysisData) =
        let analysisDataBasic = 
            AnalysisDataHandler.init(
                                     mzIdentMLXML.SpectrumIdentificationList
                                     |> Array.map (fun spectrumIdentificationList -> convertToEntity_SpectrumIdentificationList dbContext spectrumIdentificationList)
                                    )
        let analysisDataWithProteinDetectionList = 
            AnalysisDataHandler.addProteinDetectionList
                (match mzIdentMLXML.ProteinDetectionList with
                 |Some x -> convertToEntity_ProteinDetectionList dbContext x
                 |None -> Unchecked.defaultof<ProteinDetectionList>
                )
                analysisDataBasic
        analysisDataWithProteinDetectionList

    let convertToEntity_ProteinDetection (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.ProteinDetection) =
        let proteinDetectionBasic = 
            ProteinDetectionHandler.init(
                                         ProteinDetectionListHandler.tryFindByID dbContext mzIdentMLXML.ProteinDetectionListRef,
                                         ProteinDetectionProtocolHandler.tryFindByID dbContext mzIdentMLXML.ProteinDetectionProtocolRef,
                                         mzIdentMLXML.InputSpectrumIdentifications
                                         |> Array.map (fun spectrumIdentificationList -> SpectrumIdentificationListHandler.tryFindByID dbContext spectrumIdentificationList.SpectrumIdentificationListRef),
                                         mzIdentMLXML.Id
                                        )
        let proteinDetectionWithName = setOption ProteinDetectionHandler.addName mzIdentMLXML.Name proteinDetectionBasic
        let proteinDetectionWithActivityDate = setOption ProteinDetectionHandler.addActivityDate mzIdentMLXML.ActivityDate proteinDetectionWithName
        proteinDetectionWithActivityDate

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

    let convertToEntity_Provider (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.Provider) =
        let providerBasic = ProviderHandler.init(mzIdentMLXML.Id)
        let providerWithName = setOption ProviderHandler.addName mzIdentMLXML.Name providerBasic
        let providerWithAnalysisSoftware = 
            ProviderHandler.addAnalysisSoftware 
                (match mzIdentMLXML.AnalysisSoftwareRef with
                 |Some x -> AnalysisSoftwareHandler.tryFindByID dbContext x
                 |None -> Unchecked.defaultof<AnalysisSoftware>
                )
                providerWithName
        let providerWithContactRole = 
            ProviderHandler.addContactRole 
                (match mzIdentMLXML.ContactRole with
                 |Some x -> convertToEntity_ContactRole dbContext x
                 |None -> Unchecked.defaultof<ContactRole>
                )
                providerWithAnalysisSoftware
        providerWithContactRole

    let convertToEntity_MzIdentML (dbContext:MzIdentML) (mzIdentMLXML:SchemePeptideShaker.MzIdentMl) =
        let mzIdentMLBasic =  
            MzIdentMLHandler.init(
                                  mzIdentMLXML.Version,
                                  mzIdentMLXML.AnalysisCollection.SpectrumIdentifications
                                  |> Array.map (fun spectrumIdentification -> convertToEntity_SpectrumIdentification dbContext spectrumIdentification),
                                  mzIdentMLXML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
                                  |> Array.map (fun spectrumIdentificationProtocol -> SpectrumIdentificationProtocolHandler.tryFindByID dbContext spectrumIdentificationProtocol.Id),
                                  convertToEntity_Inputs dbContext mzIdentMLXML.DataCollection.Inputs,
                                  convertToEntity_AnalysisData dbContext mzIdentMLXML.DataCollection.AnalysisData
                                 )
        let mzIdentMLWithName = setOption MzIdentMLHandler.addName mzIdentMLXML.Name mzIdentMLBasic
        let mzIdentMLWithAnalysisSoftwares =
                match mzIdentMLXML.AnalysisSoftwareList with
                 |Some x -> MzIdentMLHandler.addAnalysisSoftwares      
                                (x.AnalysisSoftwares
                                 |> Array.map (fun analysisSoftware -> AnalysisSoftwareHandler.tryFindByID dbContext analysisSoftware.Id)
                                )
                                mzIdentMLWithName
                 |None -> Unchecked.defaultof<MzIdentML>
        let mzIDentMLWithProvider =
            MzIdentMLHandler.addProvider
                (match mzIdentMLXML.Provider with
                 |Some x -> convertToEntity_Provider dbContext x
                 |None -> Unchecked.defaultof<Provider>
                )
                mzIdentMLWithAnalysisSoftwares
        let mzIdentMLWithPersons =
                match mzIdentMLXML.AuditCollection with
                 |Some x -> MzIdentMLHandler.addPersons
                                (x.Persons
                                 |> Array.map (fun person -> PersonHandler.tryFindByID dbContext person.Id)
                                )
                                mzIDentMLWithProvider
                 |None -> Unchecked.defaultof<MzIdentML>
        let mzIdentMLWithOrganizations =
                match mzIdentMLXML.AuditCollection with
                 |Some x -> MzIdentMLHandler.addOrganizations
                                (x.Organizations
                                 |> Array.map (fun organization -> OrganizationHandler.tryFindByID dbContext organization.Id)
                                )
                                mzIdentMLWithPersons
                 |None -> Unchecked.defaultof<MzIdentML>
        let mzIdentMLWithSamples =
                match mzIdentMLXML.AnalysisSampleCollection with
                 |Some x -> MzIdentMLHandler.addSamples 
                                (x.Samples
                                 |>Array.map (fun sample -> SampleHandler.tryFindByID dbContext sample.Id)
                                )
                                mzIdentMLWithOrganizations
                 |None -> Unchecked.defaultof<MzIdentML>
        let mzIdentMLWithDBSequences =
                match mzIdentMLXML.SequenceCollection with
                 |Some x -> MzIdentMLHandler.addDBSequences
                                (x.DbSequences
                                 |> Array.map (fun dbSequence -> DBSequenceHandler.tryFindByID dbContext dbSequence.Id)
                                )
                                mzIdentMLWithSamples
                 |None -> Unchecked.defaultof<MzIdentML>
        let mzIdentMLWithPeptides =
                match mzIdentMLXML.SequenceCollection with
                 |Some x -> MzIdentMLHandler.addPeptides
                                (x.Peptides
                                 |> Array.map (fun peptide -> PeptideHandler.tryFindByID dbContext peptide.Id)
                                )
                                mzIdentMLWithDBSequences
                 |None -> Unchecked.defaultof<MzIdentML>
        let mzIdentMLWithPeptideEvidences =
                match mzIdentMLXML.SequenceCollection with
                 |Some x -> MzIdentMLHandler.addPeptideEvidences
                                (x.PeptideEvidences
                                 |> Array.map (fun peptideEvidence -> PeptideEvidenceHandler.tryFindByID dbContext peptideEvidence.Id)
                                )
                                mzIdentMLWithPeptides
                 |None -> Unchecked.defaultof<MzIdentML>
        let mzIdentMLWithProteinDetection =
            MzIdentMLHandler.addProteinDetection
                (match mzIdentMLXML.AnalysisCollection.ProteinDetection with
                 |Some x -> convertToEntity_ProteinDetection dbContext x
                 |None -> Unchecked.defaultof<ProteinDetection>
                )
                mzIdentMLWithPeptideEvidences
        let mzIdentMLWithProteinDetectionProtocol =
            MzIdentMLHandler.addProteinDetectionProtocol
                (match mzIdentMLXML.AnalysisProtocolCollection.ProteinDetectionProtocol with
                 |Some x -> MzIdentMLHandler.findProteinDetectionProtocolByID dbContext x.Id
                 |None -> Unchecked.defaultof<ProteinDetectionProtocol>
                )
                mzIdentMLWithProteinDetection
        let mzIdentMLWithBiblioGraphicReference =
            MzIdentMLHandler.addBiblioGraphicReferences
                (mzIdentMLXML.BibliographicReferences
                 |> Array.map (fun bibliographicReference -> convertToEntity_BiblioGraphicReference dbContext bibliographicReference)
                )
                mzIdentMLWithProteinDetectionProtocol
        MzIdentMLHandler.addToContext dbContext mzIdentMLWithBiblioGraphicReference



    //let removeDoubleTerms (pathXML : string) =
    //    let xmlMzIdentML = SchemePeptideShaker.Load(pathXML)
    //    let cvParamAccessionListOfXMLFile =
    //        [|
    //        (if xmlMzIdentML.AnalysisProtocolCollection.ProteinDetectionProtocol.IsSome 
    //            then xmlMzIdentML.AnalysisProtocolCollection.ProteinDetectionProtocol.Value.Threshold.CvParams
    //                |> Array.map (fun cvParamItem -> cvParamItem.Accession)
    //            else [|null|])

    //        (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
    //            |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.AdditionalSearchParams.IsSome 
    //                                                                        then spectrumIdentificationProtocolItem.AdditionalSearchParams. Value.CvParams
    //                                                                            |> Array.map (fun cvParamItem -> cvParamItem.Accession)

    //                                                                        else [|null|]))
    //                                    |> Array.concat

    //        (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
    //            |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.Enzymes.IsSome 
    //                                                                        then 
    //                                                                            if spectrumIdentificationProtocolItem.Enzymes.IsSome 
    //                                                                                then spectrumIdentificationProtocolItem.Enzymes.Value.Enzymes
    //                                                                                     |> Array.map (fun enzymeItem -> enzymeItem.EnzymeName.Value.CvParams
    //                                                                                                                     |> Array.map (fun cvParamitem -> cvParamitem.Accession))
    //                                                                                else [|[|null|]|]
    //                                                                        else [|[|null|]|]))
    //                                    |> Array.concat
    //                                    |> Array.concat

    //        (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
    //            |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.FragmentTolerance.IsSome
    //                                                                        then spectrumIdentificationProtocolItem.FragmentTolerance.Value.CvParams
    //                                                                             |> Array.map (fun cvParamItem -> cvParamItem.Accession)
    //                                                                        else [|null|]))
    //                                    |> Array.concat

    //        (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
    //            |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.ModificationParams.IsSome 
    //                                                                        then spectrumIdentificationProtocolItem.ModificationParams.Value.SearchModifications
    //                                                                             |> Array.map (fun searchModificationItem -> searchModificationItem.SpecificityRules
    //                                                                                                                            |> Array.map (fun specificityRuleItem -> specificityRuleItem.CvParams
    //                                                                                                                                                                    |> Array.map (fun cvParamitem -> cvParamitem.Accession)))
    //                                                                        else [|[|[|null|]|]|]))
    //                                    |> Array.concat
    //                                    |> Array.concat
    //                                    |> Array.concat

    //        (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
    //            |> Array.map (fun spectrumIdentificationProtocolItem -> if spectrumIdentificationProtocolItem.ParentTolerance.IsSome 
    //                                                                        then spectrumIdentificationProtocolItem.ParentTolerance.Value.CvParams
    //                                                                            |> Array.map (fun cvParamItem -> cvParamItem.Accession)
    //                                                                        else [|null|]))
    //                                    |> Array.concat

    //        (xmlMzIdentML.AnalysisProtocolCollection.SpectrumIdentificationProtocols
    //        |> Array.map (fun spectrumIdentificationProtocolItem -> spectrumIdentificationProtocolItem.Threshold.CvParams
    //                                                                |> Array.map (fun cvParamItem -> cvParamItem.Accession)))
    //                                    |> Array.concat

    //        (if xmlMzIdentML.AnalysisSoftwareList.IsSome 
    //            then xmlMzIdentML.AnalysisSoftwareList.Value.AnalysisSoftwares
    //                |> Array.map (fun analysisSoftwareItem -> if analysisSoftwareItem.ContactRole.IsSome 
    //                                                             then analysisSoftwareItem.ContactRole.Value.Role.CvParam.Accession
    //                                                             else null)
    //            else [|null|])

    //        (if xmlMzIdentML.AnalysisSoftwareList.IsSome 
    //        then xmlMzIdentML.AnalysisSoftwareList.Value.AnalysisSoftwares
    //          |> Array.map (fun analysisSoftwareItem -> if analysisSoftwareItem.SoftwareName.CvParam.IsSome 
    //                                                        then analysisSoftwareItem.SoftwareName.CvParam.Value.Accession
    //                                                        else null)
    //        else [|null|])

    //        (if xmlMzIdentML.AuditCollection.IsSome 
    //        then xmlMzIdentML.AuditCollection.Value.Organizations
    //          |> Array.map (fun orgItem -> orgItem.CvParams
    //                                        |> Array.map (fun cvParamItem -> cvParamItem.Accession))
    //                            |> Array.concat
    //        else [|null|])

    //        (if xmlMzIdentML.AuditCollection.IsSome 
    //            then xmlMzIdentML.AuditCollection.Value.Persons
    //                |> Array.map (fun personItem -> personItem.CvParams
    //                                                |> Array.map (fun cvParamItem -> cvParamItem.Accession))
    //                                                |> Array.concat
    //        else [|null|])

    //        (if xmlMzIdentML.DataCollection.AnalysisData.ProteinDetectionList.IsSome 
    //            then xmlMzIdentML.DataCollection.AnalysisData.ProteinDetectionList.Value.CvParams
    //                |> Array.map (fun cvParamItem -> cvParamItem.Accession)
    //            else [|null|])

    //        (if xmlMzIdentML.DataCollection.AnalysisData.ProteinDetectionList.IsSome 
    //            then xmlMzIdentML.DataCollection.AnalysisData.ProteinDetectionList.Value.ProteinAmbiguityGroups
    //                |> Array.map (fun proteinAmbiguityGroupsItem -> proteinAmbiguityGroupsItem.CvParams
    //                                                                |> Array.map (fun cvParamItem -> cvParamItem.Accession))
    //                                                                |> Array.concat
    //            else [|null|])

    //        (xmlMzIdentML.DataCollection.AnalysisData.SpectrumIdentificationList
    //            |> Array.map (fun spectrumIdentificationitem -> if spectrumIdentificationitem.FragmentationTable.IsSome
    //                                                                then spectrumIdentificationitem.FragmentationTable.Value.Measures
    //                                                                    |> Array.map (fun measureItem -> measureItem.CvParams
    //                                                                                                     |> Array.map (fun cvParamItem -> cvParamItem.Accession))
    //                                                                else [|[|null|]|])
    //                                                 |> Array.concat
    //                                                 |> Array.concat)

    //        (xmlMzIdentML.DataCollection.AnalysisData.SpectrumIdentificationList
    //            |> Array.map (fun spectrumIdentificationItem -> spectrumIdentificationItem.SpectrumIdentificationResults
    //                                                            |> Array.map (fun spectrumIdentificationResultItem -> spectrumIdentificationResultItem.CvParams
    //                                                                                                                    |> Array.map (fun cvParamItem -> cvParamItem.Accession)))
    //                                                              |> Array.concat
    //                                                              |> Array.concat)

    //        (if xmlMzIdentML.SequenceCollection.IsSome
    //            then xmlMzIdentML.SequenceCollection.Value.DbSequences
    //                 |> Array.map (fun dbSeqItem -> dbSeqItem.CvParams
    //                                                |> Array.map (fun cvParamitem -> cvParamitem.Accession))
    //                                                |> Array.concat
    //            else [|null|])

    //        (if xmlMzIdentML.SequenceCollection.IsSome 
    //            then xmlMzIdentML.SequenceCollection.Value.Peptides
    //                |> Array.map (fun peptideItem -> peptideItem.Modifications
    //                                                    |> Array.map (fun modificationItem -> modificationItem.CvParams
    //                                                                                            |> Array.map (fun cvParamItem -> cvParamItem.Accession)))
    //                                                |> Array.concat
    //                                                |> Array.concat
    //        else [|null|])
    //    |]
    //    cvParamAccessionListOfXMLFile
    //    |> Array.concat


    //let takeallTermIDs pathDB =
    //    let db = (configureSQLiteServerContext pathDB)
    //    query {
    //        for i in db.Term do
    //                select (i.ID)
    //           }
    //    |> Seq.toArray

    //let keepDuplicates is =
    //    let d = System.Collections.Generic.Dictionary()
    //    [| for i in is do match d.TryGetValue i with
    //                         | (false,_) -> d.[i] <- (); yield i
    //                         | _ -> () |]

    //let removeExistingEntries pathDB xmlTermIDList =
    //    let db = (configureSQLiteServerContext pathDB)
    //    xmlTermIDList
    //    |> Array.map (fun xmlTermItem ->
    //    query {
    //        for i in db.Term do
    //            if i.ID = xmlTermItem then
    //                select (i)
    //          }
    //    |> Seq.item 0
    //    |> (fun item -> db.Term.Remove(item) )) |> ignore
    //    db.SaveChanges()

    //let takeTermEntries (dbContext : MzIdentML) xmlTermIDList =
    //    xmlTermIDList
    //    |> Array.map (fun xmlTermItem ->
    //    query {
    //        for i in dbContext.Term do
    //            if i.ID = xmlTermItem then
    //                select (i)
    //          }
    //    |> (fun item -> if (Seq.length item) > 0 
    //                        then Seq.item 0 item
    //                        else createTermCustom "NoEntry" "NoEntry"))    

    //let takeTermID (dbContext : MzIdentML) xmlTermIDList =
    //    xmlTermIDList
    //    |> (fun xmlTermItem ->
    //    query {
    //        for i in dbContext.Term do
    //            if i.ID = xmlTermItem 
    //               then select (i.ID)
    //          }
    //    |> (fun item -> if (Seq.length item) > 0
    //                        then Seq.item 0 item
    //                        else "No Entry"
    //       ))

    //let convertToEntityInputSpectrumIdentifications (mzIdentMLXML : SchemePeptideShaker.InputSpectrumIdentifications []) =
    //    mzIdentMLXML
    //        |> Array.map (fun inputSpectrumIdentificationsItem -> createInputSpectrumIdentification inputSpectrumIdentificationsItem.SpectrumIdentificationListRef) |> List
    
    //let convertToEntityProteinDetection  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.ProteinDetection option) =
    //    if mzIdentMLXML.IsSome
    //        then mzIdentMLXML.Value 
    //            |> (fun proteinDetectionItem -> (createProteinDetection proteinDetectionItem.Id 
    //                                                                    //(if proteinDetectionItem.Name.IsSome then proteinDetectionItem.Name.Value else null) 
    //                                                                    proteinDetectionItem.ProteinDetectionProtocolRef
    //                                                                    proteinDetectionItem.ProteinDetectionListRef
    //                                                                    (proteinDetectionItem.InputSpectrumIdentifications 
    //                                                                        |> convertToEntityInputSpectrumIdentifications
    //                                                                    )
    //                                                                    (if proteinDetectionItem.ActivityDate.IsSome 
    //                                                                        then proteinDetectionItem.ActivityDate.Value 
    //                                                                        else DateTime.UtcNow)))
    //        else createProteinDetection null null null null DateTime.UtcNow

    //let convertToEntityInputSpectras (mzIdentMLXML : SchemePeptideShaker.InputSpectra []) =
    //    mzIdentMLXML
    //        |> Array.map (fun inputSpectraItem -> createInputSpectra (if inputSpectraItem.SpectraDataRef.IsSome 
    //                                                                     then inputSpectraItem.SpectraDataRef.Value 
    //                                                                     else null)) |> List

    //let convertToEntitySearchDatabaseRef (mzIdentMLXML : SchemePeptideShaker.SearchDatabaseRef []) =  
    //    mzIdentMLXML
    //        |> Array.map (fun searchDatabaseRefItem -> createSearchDatabaseRef (if searchDatabaseRefItem.SearchDatabaseRef.IsSome 
    //                                                                               then searchDatabaseRefItem.SearchDatabaseRef.Value 
    //                                                                               else null)) |> List

    //let convertToEntitySpectrumIdentifications (mzIdentMLXML : SchemePeptideShaker.SpectrumIdentification []) =
    //    mzIdentMLXML
    //    |> Array.map (fun spectrumIdentificationItem -> createSpectrumIdentification  spectrumIdentificationItem.Id
    //                                                                                  (if spectrumIdentificationItem.ActivityDate.IsSome 
    //                                                                                    then spectrumIdentificationItem.ActivityDate.Value 
    //                                                                                    else DateTime.UtcNow)
    //                                                                                  //(if spectrumIdentificationItem.Name.IsSome then spectrumIdentificationItem.Name.Value else null)
    //                                                                                  spectrumIdentificationItem.SpectrumIdentificationListRef
    //                                                                                  spectrumIdentificationItem.SpectrumIdentificationProtocolRef 
    //                                                                                  (spectrumIdentificationItem.InputSpectras
    //                                                                                   |> convertToEntityInputSpectras)
    //                                                                                  (spectrumIdentificationItem.SearchDatabaseRefs
    //                                                                                    |> convertToEntitySearchDatabaseRef
    //                                                                                  )
    //                 ) |> List

    //let convertToEntityAdditionalSearchParamsCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =   
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createAdditionalSearchParamsParam (*cvParamItem.Name*)
    //                                                                           //cvParamItem.Accession 
    //                                                                           (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                                           (takeTermID dbContext cvParamItem.CvRef)
    //                                                                           (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                                           (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
    //                     ) |> List

    //let convertToEntityAdditionalSearchParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.AdditionalSearchParams option) =
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value 
    //            |> (fun additionalSearchParamItem -> (convertToEntityAdditionalSearchParamsCVParams dbContext additionalSearchParamItem.CvParams
    //                                                 ),
    //                                                 (convertToEntityUserParams dbContext additionalSearchParamItem.UserParams
    //                                                 )
    //               )
    //        else null, null

    //let convertToEntitySearchTypeCVParam  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam option) =
    //     if mzIdentMLXML.IsSome
    //        then mzIdentMLXML.Value
    //             |> (fun cvParamItem -> (createSearchTypeParam (*cvParamItem.Name*)
    //                                                           //cvParamItem.Accession 
    //                                                           (if cvParamItem.Value.IsSome 
    //                                                               then cvParamItem.Value.Value 
    //                                                               else null)
    //                                                           (takeTermID dbContext cvParamItem.CvRef)
    //                                                           (if cvParamItem.UnitCvRef.IsSome 
    //                                                               then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                               else null)
    //                                                           (if cvParamItem.UnitName.IsSome 
    //                                                               then cvParamItem.UnitName.Value 
    //                                                               else null)
    //                                    )
    //                )
    //        else createSearchTypeParam null null null null

    //let convertToEntitySearchType  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.SearchType) =
    //    mzIdentMLXML
    //        |> (fun searchTypeItem -> (convertToEntitySearchTypeCVParam dbContext searchTypeItem.CvParam
    //                                  ),
    //                                  (convertToEntityUserParam dbContext searchTypeItem.UserParam
    //                                  )                                                                                                                                                                                                                
    //           )

    //let convertToEntitySpecificityRulesCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createSpecificityRulesParam (*cvParamItem.Name*)
    //                                                                      //cvParamItem.Accession 
    //                                                                      (if cvParamItem.Value.IsSome 
    //                                                                          then cvParamItem.Value.Value 
    //                                                                          else null)
    //                                                                      (takeTermID dbContext cvParamItem.CvRef)
    //                                                                      (if cvParamItem.UnitCvRef.IsSome 
    //                                                                          then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                          else null)
    //                                                                      (if cvParamItem.UnitName.IsSome 
    //                                                                          then cvParamItem.UnitName.Value 
    //                                                                          else null)
                                                                                                                                                                                        
    //                                         )
    //                     )

    //let convertToEntitySpecificityRules  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.SpecificityRules []) =
    //    mzIdentMLXML
    //        |> Array.map (fun specificityRuleItem -> (convertToEntitySpecificityRulesCVParams dbContext specificityRuleItem.CvParams)
    //                     ) 
    //        |> Array.concat
    //        |> List

    //let convertToEntitySearchModificationsCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createSearchModificationParam (*cvParamItem.Name*)
    //                                                                        //cvParamItem.Accession 
    //                                                                        (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                                        (takeTermID dbContext cvParamItem.CvRef)
    //                                                                        (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                                        (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
                                                                                                                                                                                        
    //                                         )
    //                     ) |> List

    //let convertToEntitySearchModifications  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.SearchModification []) =
    //    mzIdentMLXML
    //        |> Array.map (fun searchModificationItem -> createSearchModification    searchModificationItem.FixedMod
    //                                                                                searchModificationItem.MassDelta 
    //                                                                                searchModificationItem.Residues 
    //                                                                                (convertToEntitySpecificityRules dbContext searchModificationItem.SpecificityRules
    //                                                                                )
    //                                                                                (convertToEntitySearchModificationsCVParams dbContext searchModificationItem.CvParams
    //                                                                                )
                                                                                                                                                                                
    //                     ) |> List

    //let convertToEntityModificationParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.ModificationParams option) =
    //    if mzIdentMLXML.IsSome 
    //        then mzIdentMLXML.Value
    //             |> (fun modificationParamItem -> (convertToEntitySearchModifications dbContext modificationParamItem.SearchModifications))
    //        else null


    //let convertToEntityEnzymeNameCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createEnzymeNameParam (*cvParamItem.Name*)
    //                                                                //cvParamItem.Accession 
    //                                                                (if cvParamItem.Value.IsSome 
    //                                                                    then cvParamItem.Value.Value 
    //                                                                    else null
    //                                                                )
    //                                                                (takeTermID dbContext cvParamItem.CvRef)
    //                                                                (if cvParamItem.UnitCvRef.IsSome 
    //                                                                    then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                    else null
    //                                                                )
    //                                                                (if cvParamItem.UnitName.IsSome 
    //                                                                    then cvParamItem.UnitName.Value 
    //                                                                    else null
    //                                                                )
                                                                                                                                                                                       
    //                                         )
    //                     ) |> List

    //let convertToEntityEnzymeName  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.EnzymeName option) =
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value 
    //            |> (fun enzymeNameItem -> (convertToEntityEnzymeNameCVParams dbContext enzymeNameItem.CvParams
    //                                      ),
    //                                      (convertToEntityUserParams dbContext enzymeNameItem.UserParams
    //                                      )
    //               )
    //        else null, null

    //let convertToEntityEnzyme  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Enzyme []) =   
    //    mzIdentMLXML
    //        |> Array.map (fun enzymeItem -> createEnzyme enzymeItem.Id 
    //                                                        //(if enzymeItem.Name.IsSome then enzymeItem.Name.Value else null) 
    //                                                        (if enzymeItem.CTermGain.IsSome 
    //                                                            then enzymeItem.CTermGain.Value 
    //                                                            else null
    //                                                        )
    //                                                        (if enzymeItem.NTermGain.IsSome 
    //                                                            then enzymeItem.NTermGain.Value 
    //                                                            else null
    //                                                        ) 
    //                                                        (if enzymeItem.MinDistance.IsSome 
    //                                                            then enzymeItem.MinDistance.Value 
    //                                                            else -1
    //                                                        )
    //                                                        (if enzymeItem.MissedCleavages.IsSome 
    //                                                            then enzymeItem.MissedCleavages.Value.ToString()
    //                                                            else null
    //                                                        )
    //                                                        (if enzymeItem.SemiSpecific.IsSome 
    //                                                            then enzymeItem.SemiSpecific.Value.ToString()
    //                                                            else null
    //                                                        )
    //                                                        (if enzymeItem.SiteRegexp.IsSome 
    //                                                            then enzymeItem.SiteRegexp.Value
    //                                                            else  null
    //                                                        )
    //                                                        (fst (convertToEntityEnzymeName dbContext enzymeItem.EnzymeName
    //                                                        ))
    //                                                        (snd(convertToEntityEnzymeName dbContext enzymeItem.EnzymeName
    //                                                        ))
    //                     ) |> List

    //let convertToEntityEnzymes  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Enzymes option) = 
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value
    //            |> (fun enzymesItem -> createEnzymes (if enzymesItem.Independent.IsSome 
    //                                                     then enzymesItem.Independent.Value.ToString() 
    //                                                     else null
    //                                                 )
    //                                                 (convertToEntityEnzyme dbContext enzymesItem.Enzymes
    //                                                 )
    //               )
    //       else createEnzymes null null

    //let convertToEntityAmbiguousResiduesCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createAmbiguousResidueParam (*cvParamItem.Name*)
    //                                                                      //cvParamItem.Accession 
    //                                                                      (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                                      (takeTermID dbContext cvParamItem.CvRef)
    //                                                                      (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                                      (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
                                                                                                                                                                                        
    //                                         )
    //                     ) |> List

    //let convertToEntityAmbiguousResidues  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.AmbiguousResidue []) =
    //    mzIdentMLXML
    //        |> Array.map (fun ambiguousResidueItem-> (createAmbiguousResidue ambiguousResidueItem.Code 
    //                                                                         (convertToEntityAmbiguousResiduesCVParams dbContext ambiguousResidueItem.CvParams
    //                                                                         )
    //                                                                         (convertToEntityUserParams dbContext ambiguousResidueItem.UserParams
    //                                                                         )
    //                                                 ) 
    //                     ) |> List

    //let convertToEntityResiduesOfMassTable (mzIdentMLXML : SchemePeptideShaker.Residue []) = 
    //    mzIdentMLXML
    //        |> Array.map (fun residueItem -> createResidue residueItem.Code 
    //                                                       residueItem.Mass
    //                     ) |> List

    //let massTablesCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createMassTableParam (*cvParamItem.Name*)
    //                                                               //cvParamItem.Accession 
    //                                                               (if cvParamItem.Value.IsSome 
    //                                                                   then cvParamItem.Value.Value 
    //                                                                   else null)
    //                                                               (takeTermID dbContext cvParamItem.CvRef)
    //                                                               (if cvParamItem.UnitCvRef.IsSome 
    //                                                                   then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                   else null)
    //                                                               (if cvParamItem.UnitName.IsSome 
    //                                                                   then cvParamItem.UnitName.Value 
    //                                                                   else null)
                                                                                                                                                                                        
    //                                         )
    //                     ) |> List

    //let convertToEntityMassTables  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.MassTable []) =
    //    mzIdentMLXML
    //        |> Array.map (fun massTableItem -> createMassTable (massTableItem.Id) 
    //                                                           //(if massTableItem.Name.IsSome then massTableItem.Name.Value else null)
    //                                                           (massTableItem.MsLevel)
    //                                                           (convertToEntityAmbiguousResidues dbContext massTableItem.AmbiguousResidues
    //                                                           )
    //                                                           (convertToEntityResiduesOfMassTable massTableItem.Residues
    //                                                           )
    //                                                           (massTablesCVParams dbContext massTableItem.CvParams
    //                                                           )
    //                                                           (convertToEntityUserParams dbContext massTableItem.UserParams
    //                                                           )
                                                                                                
    //                     ) |> List

    //let convertToEntityFragmentToleranceCVParams (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createFragmentToleranceParam (*cvParamItem.Name*)
    //                                                                       //cvParamItem.Accession 
    //                                                                       (if cvParamItem.Value.IsSome 
    //                                                                           then cvParamItem.Value.Value 
    //                                                                           else null
    //                                                                       )
    //                                                                       (takeTermID dbContext cvParamItem.CvRef)
    //                                                                       (if cvParamItem.UnitCvRef.IsSome 
    //                                                                           then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                           else null)
    //                                                                       (if cvParamItem.UnitName.IsSome 
    //                                                                           then cvParamItem.UnitName.Value 
    //                                                                           else null
    //                                                                       )
                                                                                                                                                                                        
    //                                         )
    //                     ) |> List

    //let convertToEntityFragmentTolerance (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.FragmentTolerance option) =
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value
    //            |> (fun fragmentToleranceItem -> (convertToEntityFragmentToleranceCVParams dbContext fragmentToleranceItem.CvParams)
    //               )
    //       else null

    //let convertToEntityExcludeCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =   
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createExcludeParam (*cvParamItem.Name*)
    //                                                             //cvParamItem.Accession 
    //                                                             (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                             (takeTermID dbContext cvParamItem.CvRef)
    //                                                             (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                             (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
    //                                         )
    //                     ) |> List

    //let convertToEntityExclude  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Exclude option) = 
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value
    //            |> (fun excludeItem -> (convertToEntityExcludeCVParams dbContext excludeItem.CvParams),
    //                                   (convertToEntityUserParams dbContext excludeItem.UserParams) 
    //               )
    //        else null, null

    //let convertToEntityIncludeCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createIncludeParam (*cvParamItem.Name*)
    //                                                             //cvParamItem.Accession 
    //                                                             (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                             (takeTermID dbContext cvParamItem.CvRef)
    //                                                             (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                             (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
    //                                         )
    //                     ) |> List

    //let convertToEntityIncludes  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Include option) = 
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value
    //            |> (fun includeItem -> (convertToEntityIncludeCVParams dbContext includeItem.CvParams),
    //                                   (convertToEntityUserParams dbContext includeItem.UserParams) 
    //               )
    //        else null, null

    //let convertToEntityFilterTypeCVParam  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam option) =
    //    if mzIdentMLXML.IsSome
    //       then mzIdentMLXML.Value
    //            |> (fun cvParamItem -> (createFilterTypeParam (*cvParamItem.Name*)
    //                                                            //cvParamItem.Accession 
    //                                                            (if cvParamItem.Value.IsSome 
    //                                                                then cvParamItem.Value.Value 
    //                                                                else null)
    //                                                            (takeTermID dbContext cvParamItem.CvRef)
    //                                                            (if cvParamItem.UnitCvRef.IsSome 
    //                                                                then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                else null)
    //                                                            (if cvParamItem.UnitName.IsSome 
    //                                                                then cvParamItem.UnitName.Value 
    //                                                                else null)
    //                                   )
    //               )
    //        else createFilterTypeParam null null null null
        
    //let convertToEntityFilterType  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.FilterType) = 
    //    mzIdentMLXML
    //    |> (fun filterTypeItem -> (convertToEntityFilterTypeCVParam dbContext filterTypeItem.CvParam
    //                              ), 
    //                              (convertToEntityUserParam dbContext filterTypeItem.UserParam
    //                              )
    //       )

    //let convertToEntityFilters  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Filter []) = 
    //    mzIdentMLXML
    //        |> Array.map (fun filterItem -> (fst(convertToEntityFilterType dbContext filterItem.FilterType
    //                                        )),
    //                                        (snd(convertToEntityFilterType dbContext filterItem.FilterType
    //                                        )),
    //                                        (fst(convertToEntityExclude dbContext filterItem.Exclude
    //                                        )),
    //                                        (snd(convertToEntityExclude dbContext filterItem.Exclude
    //                                        )),
    //                                        (fst(convertToEntityIncludes dbContext filterItem.Include
    //                                        )),
    //                                        (snd(convertToEntityIncludes dbContext filterItem.Include
    //                                        ))
    //                     )

    //let convertToEntityDatabaseFilters  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.DatabaseFilters option) = 
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value
    //            |> (fun databaseFilterItem -> (convertToEntityFilters dbContext databaseFilterItem.Filters)
    //                                                        |> Array.map (fun (cvFiltertype, userFiltertype, cvExclude, userExclude, cvInclude, userInclude) -> createFilter cvFiltertype userFiltertype cvExclude userExclude cvInclude userInclude)
    //                                                                     ) |> List
    //       else null

    //let convertToEntityTranslationTablesCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) = 
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createTranslationTableParam (*cvParamItem.Name*)
    //                                                                      //cvParamItem.Accession 
    //                                                                      (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                                      (takeTermID dbContext cvParamItem.CvRef)
    //                                                                      (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                                      (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
                                                                                                                                                                                       
    //                                         )
    //                     ) 

    //let convertToEntityTranslationTables  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.TranslationTable []) =  
    //    mzIdentMLXML
    //        |> Array.map (fun translationTable -> (*translationTable.Id,*)
    //                                              //(if translationTable.Name.IsSome then translationTable.Name.Value else null) 
    //                                              (convertToEntityTranslationTablesCVParams dbContext translationTable.CvParams
    //                                              ) 
    //                     ) 
    //                     |> Array.concat
    //                     |> List

    //let convertToEntityDatabaseTranslation  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.DatabaseTranslation option) =
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value
    //            |> (fun databaseTranslationItem ->  (if databaseTranslationItem.Frames.IsSome 
    //                                                    then databaseTranslationItem.Frames.Value
    //                                                    else null
    //                                                ), 
    //                                                (convertToEntityTranslationTables dbContext databaseTranslationItem.TranslationTables
    //                                                )
    //               )
    //       else null, null

    //let convertToEntityParentToleranceCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createParentToleranceParam (*cvParamItem.Name*)
    //                                                                     //cvParamItem.Accession 
    //                                                                     (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                                     (takeTermID dbContext cvParamItem.CvRef)
    //                                                                     (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                                     (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
                                                                                                                                                                                         
    //                                         )
    //                     ) |> List

    //let convertToEntityParentTolerance  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.ParentTolerance option) =
    //    if mzIdentMLXML.IsSome 
    //        then mzIdentMLXML.Value
    //             |> (fun parentToleranceItem -> (convertToEntityParentToleranceCVParams dbContext parentToleranceItem.CvParams)
    //                ) 
    //        else null

    //let convertToEntityThresholdOfSpectumIdentificationprotocolCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =  
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createThresholdParam (*cvParamItem.Name*)
    //                                                               //cvParamItem.Accession 
    //                                                               (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                               (takeTermID dbContext cvParamItem.CvRef)
    //                                                               (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                               (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
                                                                                                                                                                                         
    //                                         )
    //                     ) |> List

    //let convertToEntityThreshold  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Threshold) = 
    //    mzIdentMLXML
    //        |> (fun thresholdItem ->  (convertToEntityThresholdOfSpectumIdentificationprotocolCVParams dbContext thresholdItem.CvParams
    //                                  ),
    //                                  (thresholdItem.UserParams
    //                                      |> convertToEntityUserParams dbContext
    //                                  )
    //           )

    //let convertToEntitySpectrumIdentificationProtocols  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.SpectrumIdentificationProtocol []) =  
    //    mzIdentMLXML
    //    |> Array.map (fun spectrumIdentificationProtocolItem -> createSpectrumIdentificationProtocol (spectrumIdentificationProtocolItem.Id
    //                                                                                                 ) 
    //                                                                                                 //(if spectrumIdentificationProtocolItem.Name.IsSome 
    //                                                                                                 //    then spectrumIdentificationProtocolItem.Name.Value 
    //                                                                                                 //    else null) 
    //                                                                                                 (spectrumIdentificationProtocolItem.AnalysisSoftwareRef
    //                                                                                                 )
    //                                                                                                 (fst (convertToEntitySearchType dbContext spectrumIdentificationProtocolItem.SearchType
    //                                                                                                 ))
    //                                                                                                 (snd (convertToEntitySearchType dbContext spectrumIdentificationProtocolItem.SearchType
    //                                                                                                 ))
    //                                                                                                 (if spectrumIdentificationProtocolItem.AdditionalSearchParams.IsSome 
    //                                                                                                     then fst (convertToEntityAdditionalSearchParams dbContext spectrumIdentificationProtocolItem.AdditionalSearchParams)
    //                                                                                                     else null
    //                                                                                                 )
    //                                                                                                 (if spectrumIdentificationProtocolItem.AdditionalSearchParams.IsSome 
    //                                                                                                     then snd (convertToEntityAdditionalSearchParams dbContext spectrumIdentificationProtocolItem.AdditionalSearchParams)
    //                                                                                                     else null
    //                                                                                                 )
    //                                                                                                 (if spectrumIdentificationProtocolItem.ModificationParams.IsSome 
    //                                                                                                     then convertToEntityModificationParams dbContext spectrumIdentificationProtocolItem.ModificationParams
    //                                                                                                     else null
    //                                                                                                 )  
    //                                                                                                 (convertToEntityEnzymes dbContext spectrumIdentificationProtocolItem.Enzymes
    //                                                                                                 ) 
    //                                                                                                 (convertToEntityMassTables dbContext spectrumIdentificationProtocolItem.MassTables
    //                                                                                                 )
    //                                                                                                 (fst(convertToEntityThreshold dbContext spectrumIdentificationProtocolItem.Threshold
    //                                                                                                 ))
    //                                                                                                 (snd(convertToEntityThreshold dbContext spectrumIdentificationProtocolItem.Threshold
    //                                                                                                 ))
    //                                                                                                 (convertToEntityFragmentTolerance dbContext spectrumIdentificationProtocolItem.FragmentTolerance
    //                                                                                                 )
    //                                                                                                 (convertToEntityParentTolerance dbContext spectrumIdentificationProtocolItem.ParentTolerance
    //                                                                                                 )
    //                                                                                                 (convertToEntityDatabaseFilters dbContext spectrumIdentificationProtocolItem.DatabaseFilters
    //                                                                                                 )
    //                                                                                                 (fst(convertToEntityDatabaseTranslation dbContext spectrumIdentificationProtocolItem.DatabaseTranslation
    //                                                                                                 ))
    //                                                                                                 (snd(convertToEntityDatabaseTranslation dbContext spectrumIdentificationProtocolItem.DatabaseTranslation
    //                                                                                                 )) 
    //                 ) |> List

    //let convertToEntityProteinDetectionProtocol  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.ProteinDetectionProtocol option) = 
    //    (if mzIdentMLXML.IsSome 
    //        then mzIdentMLXML.Value
    //            |> (fun proteinDetectionProtocolItem -> createProteinDetectionProtocol (proteinDetectionProtocolItem.Id) 
    //                                                                                   (proteinDetectionProtocolItem.AnalysisSoftwareRef)
    //                                                                                   (fst(convertToEntityAnalysisParams dbContext proteinDetectionProtocolItem.AnalysisParams))
    //                                                                                   (snd(convertToEntityAnalysisParams dbContext proteinDetectionProtocolItem.AnalysisParams))
    //                                                                                   (fst(convertToEntityThresholdOfProteinDetection dbContext proteinDetectionProtocolItem.Threshold))
    //                                                                                   (snd(convertToEntityThresholdOfProteinDetection dbContext proteinDetectionProtocolItem.Threshold))
    //                                                                                   (*(if proteinDetectionProtocolItem.Name.IsSome then proteinDetectionProtocolItem.Name.Value else null)*)                 
    //               )
    //        else createProteinDetectionProtocol null null  null null null null)
   
    //let convertToEntityAnalysisProtocolCollection  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.AnalysisProtocolCollection) =
    //     mzIdentMLXML
    //        |> (fun analysisProtocolCollectionItem -> (convertToEntitySpectrumIdentificationProtocols dbContext analysisProtocolCollectionItem.SpectrumIdentificationProtocols
    //                                                  ),
    //                                                  (convertToEntityProteinDetectionProtocol dbContext analysisProtocolCollectionItem.ProteinDetectionProtocol
    //                                                  )
    //           )

    //let convertToEntityRoleCVParam  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam) =
    //    mzIdentMLXML
    //        |> (fun cvParamItem -> createRoleParam (*cvParamItem.Name*)
    //                                               //cvParamItem.Accession 
    //                                               (if cvParamItem.Value.IsSome 
    //                                                   then cvParamItem.Value.Value 
    //                                                   else null
    //                                               )
    //                                               (takeTermID dbContext cvParamItem.CvRef)
    //                                               (if cvParamItem.UnitCvRef.IsSome 
    //                                                   then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                   else null
    //                                               )
    //                                               (if cvParamItem.UnitName.IsSome 
    //                                                   then cvParamItem.UnitName.Value 
    //                                                   else null
    //                                               )
    //           )

    //let convertToEntityRole  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Role) =
    //    mzIdentMLXML
    //        |> (fun rolteItem -> (convertToEntityRoleCVParam dbContext rolteItem.CvParam)
    //           )

    //let convertToEntityContactRoleOption  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.ContactRole option) =
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value
    //            |> (fun contactRoleItem ->  contactRoleItem.ContactRef,
    //                                        (convertToEntityRole dbContext contactRoleItem.Role)
    //               )
    //       else null, (createRoleParam null null null null)

    //let convertToEntitySoftwareNameCVParam  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam option) =
    //     if mzIdentMLXML.IsSome 
    //        then mzIdentMLXML.Value
    //                    |> (fun cvParamItem -> (createSoftwareNameParam (*cvParamItem.Name*)
    //                                                                    //cvParamItem.Accession 
    //                                                                    (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                                    (takeTermID dbContext cvParamItem.CvRef)
    //                                                                    (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                                    (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
    //                                            )
    //                        )
    //        else createSoftwareNameParam null null null null

    //let convertToEntitySoftwareName  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.SoftwareName) =
    //    mzIdentMLXML
    //        |> (fun softwareNameItem -> (convertToEntitySoftwareNameCVParam dbContext softwareNameItem.CvParam
    //                                    ),
    //                                    (convertToEntityUserParam dbContext softwareNameItem.UserParam
    //                                    )               
    //           )

    //let convertToEntityAnalysisSoftWares  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.AnalysisSoftware []) =
    //    mzIdentMLXML
    //        |> Array.map (fun analysisSoftareListItem -> createAnalysisSoftware analysisSoftareListItem.Id 
    //                                                                            //(if analysisSoftareListItem.Name.IsSome 
    //                                                                            //    then analysisSoftareListItem.Name.Value 
    //                                                                            //    else null), 
    //                                                                            (if analysisSoftareListItem.Uri.IsSome 
    //                                                                                then analysisSoftareListItem.Uri.Value 
    //                                                                                else null)
    //                                                                            (fst(convertToEntityContactRoleOption dbContext analysisSoftareListItem.ContactRole
    //                                                                            ))
    //                                                                            (snd(convertToEntityContactRoleOption dbContext analysisSoftareListItem.ContactRole
    //                                                                            ))
    //                                                                            (fst(convertToEntitySoftwareName dbContext analysisSoftareListItem.SoftwareName
    //                                                                            ))
    //                                                                            (snd(convertToEntitySoftwareName dbContext analysisSoftareListItem.SoftwareName
    //                                                                            ))
    //                                                                            (if analysisSoftareListItem.Customizations.IsSome 
    //                                                                                then analysisSoftareListItem.Customizations.Value 
    //                                                                                else null
    //                                                                            ) 
    //                                                                            (if analysisSoftareListItem.Version.IsSome 
    //                                                                                then analysisSoftareListItem.Version.Value 
    //                                                                                else null
    //                                                                            )                                 
    //                     ) |> List

    //let convertToEntityAnalysisSoftwareList  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =
    //    if mzIdentMLXML.AnalysisSoftwareList.IsSome 
    //       then (convertToEntityAnalysisSoftWares dbContext mzIdentMLXML.AnalysisSoftwareList.Value.AnalysisSoftwares)
    //       else null
    
    ////let convertToEntityParent (mzIdentMLXML : SchemePeptideShaker.Parent option) =
    ////    if mzIdentMLXML.IsSome 
    ////       then mzIdentMLXML.Value
    ////            |> (fun parentItem -> createParent parentItem.OrganizationRef)
    ////        else createParent null

    //let convertToEntityOrganizationCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) = 
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createOrganizationParam (*cvParamItem.Name*)
    //                                                                  //cvParamItem.Accession 
    //                                                                  (if cvParamItem.Value.IsSome 
    //                                                                      then cvParamItem.Value.Value 
    //                                                                      else null)
    //                                                                  (takeTermID dbContext cvParamItem.CvRef)
    //                                                                  (if cvParamItem.UnitCvRef.IsSome 
    //                                                                      then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                      else null)
    //                                                                  (if cvParamItem.UnitName.IsSome
    //                                                                      then cvParamItem.UnitName.Value 
    //                                                                      else null)
    //                                         )
    //                     ) |> List

    //let convertToEntityOrganizations  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Organization []) =
    //    mzIdentMLXML
    //        |> Array.map (fun orgItem -> createOrganization orgItem.Id
    //                                                        (if orgItem.Parent.IsSome
    //                                                            then orgItem.Parent.Value.OrganizationRef
    //                                                            else null
    //                                                        )
    //                                                        //(if orgItem.Name.IsSome 
    //                                                        //    then orgItem.Name.Value 
    //                                                        //    else null)
    //                                                        (convertToEntityOrganizationCVParams dbContext orgItem.CvParams
    //                                                        )
    //                                                        (convertToEntityUserParams dbContext orgItem.UserParams
    //                                                        )
    //                     ) |> List

    //let convertToEntityPersonCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =  
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createPersonParam (*cvParamItem.Name*)
    //                                                           //cvParamItem.Accession 
    //                                                           (if cvParamItem.Value.IsSome 
    //                                                               then cvParamItem.Value.Value 
    //                                                               else null)
    //                                                           (takeTermID dbContext cvParamItem.CvRef)
    //                                                           (if cvParamItem.UnitCvRef.IsSome 
    //                                                               then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                               else null)
    //                                                           (if cvParamItem.UnitName.IsSome 
    //                                                               then cvParamItem.UnitName.Value 
    //                                                               else null)   
    //                     ) |> List

    //let convertToEntityAffiliations (mzIdentMLXML : SchemePeptideShaker.Affiliation []) = 
    //    mzIdentMLXML
    //        |> Array.map (fun affiliationItem -> (createAffiliation affiliationItem.OrganizationRef)
    //                     ) |> List

    //let convertToEntityPersons  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Person []) =    
    //    mzIdentMLXML
    //        |> Array.map (fun personItem -> createPerson personItem.Id
    //                                                        //(if personItem.Name.IsSome then personItem.Name.Value else null), 
    //                                                        (if personItem.FirstName.IsSome 
    //                                                            then personItem.FirstName.Value 
    //                                                            else null)
    //                                                        (if personItem.MidInitials.IsSome 
    //                                                            then personItem.MidInitials.Value 
    //                                                            else null)
    //                                                        (if personItem.LastName.IsSome 
    //                                                            then personItem.LastName.Value 
    //                                                            else null)  
    //                                                        (convertToEntityAffiliations personItem.Affiliations
    //                                                        )
    //                                                        (convertToEntityPersonCVParams dbContext personItem.CvParams
    //                                                        )
    //                                                        (convertToEntityUserParams dbContext personItem.UserParams
    //                                                        )
    //                        ) |> List

    //let convertToEntityAuditCollection  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =
    //    if mzIdentMLXML.AuditCollection.IsSome
    //       then mzIdentMLXML.AuditCollection.Value
    //            |> (fun auditCollectionItem -> (convertToEntityOrganizations dbContext auditCollectionItem.Organizations
    //                                           ),
    //                                           (convertToEntityPersons dbContext  auditCollectionItem.Persons
    //                                           )
    //               )
    //       else null, null

    //let convertToEntitySpectrumIdentificationItemRefs (mzIdentMLXML : SchemePeptideShaker.SpectrumIdentificationItemRef []) = 
    //    mzIdentMLXML
    //        |> Array.map (fun spectrumIdentificationItemRefItem -> createSpectrumIdentificationItemRef spectrumIdentificationItemRefItem.SpectrumIdentificationItemRef
    //                     ) |> List

    //let convertToEntityPeptideHypothesis (mzIdentMLXML : SchemePeptideShaker.PeptideHypothesis []) =   
    //    mzIdentMLXML
    //        |> Array.map (fun peptideHypothesisItem -> createPeptideHypothesis  peptideHypothesisItem.PeptideEvidenceRef 
    //                                                                            (convertToEntitySpectrumIdentificationItemRefs peptideHypothesisItem.SpectrumIdentificationItemRefs
    //                                                                            )                                                        
    //                     ) |> List

    //let convertToEntityProteinDetectionHypthesisCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =   
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createProteinDetectionHypothesisParam (*cvParamItem.Name*)
    //                                                                                //cvParamItem.Accession 
    //                                                                                (if cvParamItem.Value.IsSome 
    //                                                                                    then cvParamItem.Value.Value 
    //                                                                                    else null)
    //                                                                                (takeTermID dbContext cvParamItem.CvRef)
    //                                                                                (if cvParamItem.UnitCvRef.IsSome 
    //                                                                                    then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                                    else null)
    //                                                                                (if cvParamItem.UnitName.IsSome 
    //                                                                                    then cvParamItem.UnitName.Value 
    //                                                                                    else null)
    //                                         )
    //                     ) |> List

    //let convertToEntityProteinDetectionHypthesis  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.ProteinDetectionHypothesis []) =
    //    mzIdentMLXML
    //        |> Array.map (fun proteinDetectionHypothesisItem -> createProteinDetectionHypothesis    proteinDetectionHypothesisItem.Id 
    //                                                                                                //(if proteinDetectionHypothesisItem.Name.IsSome
    //                                                                                                //    then proteinDetectionHypothesisItem.Name.Value 
    //                                                                                                //    else null), 
    //                                                                                                (proteinDetectionHypothesisItem.DBSequenceRef)
    //                                                                                                (proteinDetectionHypothesisItem.PassThreshold)
    //                                                                                                (convertToEntityPeptideHypothesis proteinDetectionHypothesisItem.PeptideHypotheses
    //                                                                                                )
    //                                                                                                (convertToEntityProteinDetectionHypthesisCVParams dbContext proteinDetectionHypothesisItem.CvParams
    //                                                                                                ) 
    //                                                                                                (convertToEntityUserParams dbContext proteinDetectionHypothesisItem.UserParams
    //                                                                                                ) 
                                                                                                                                                                                                             
    //                     ) |> List

    //let convertToEntityProteinAmbiguityGroupsCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =  
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createProteinAmbiguityGroupParam (*cvParamItem.Name*)
    //                                                                           //cvParamItem.Accession 
    //                                                                           (if cvParamItem.Value.IsSome 
    //                                                                               then cvParamItem.Value.Value 
    //                                                                               else null)
    //                                                                           (takeTermID dbContext cvParamItem.CvRef)
    //                                                                           (if cvParamItem.UnitCvRef.IsSome 
    //                                                                               then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                               else null)
    //                                                                           (if cvParamItem.UnitName.IsSome 
    //                                                                               then cvParamItem.UnitName.Value 
    //                                                                               else null)
    //                                         )
    //                     ) |> List

    //let convertToEntityProteinAmbiguityGroups  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.ProteinAmbiguityGroup []) =
    //    mzIdentMLXML
    //        |> Array.map (fun proteinAmbiguityGroupItem -> createProteinAmbiguityGroup proteinAmbiguityGroupItem.Id
    //                                                                                   //(if proteinAmbiguityGroupItem.Name.IsSome 
    //                                                                                   //    then proteinAmbiguityGroupItem.Name.Value 
    //                                                                                   //    else null),
    //                                                                                   (convertToEntityProteinDetectionHypthesis dbContext proteinAmbiguityGroupItem.ProteinDetectionHypotheses
    //                                                                                   )
    //                                                                                   (convertToEntityProteinAmbiguityGroupsCVParams dbContext proteinAmbiguityGroupItem.CvParams
    //                                                                                   )
    //                                                                                   (convertToEntityUserParams dbContext proteinAmbiguityGroupItem.UserParams
    //                                                                                   )
                                                                                                                                                   
    //                     ) |> List

    //let convertToEntityProteinDetectionListCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =   
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createProteinDetectionListParam (*cvParamItem.Name*)
    //                                                                         //cvParamItem.Accession 
    //                                                                         (if cvParamItem.Value.IsSome 
    //                                                                            then cvParamItem.Value.Value 
    //                                                                            else null)
    //                                                                         (takeTermID dbContext cvParamItem.CvRef)
    //                                                                         (if cvParamItem.UnitCvRef.IsSome 
    //                                                                            then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                            else null)
    //                                                                         (if cvParamItem.UnitName.IsSome 
    //                                                                             then cvParamItem.UnitName.Value 
    //                                                                             else null)   
    //                     ) |> List

    //let convertToEntityProteinDetectionList  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.ProteinDetectionList option) = 
    //     if mzIdentMLXML.IsSome 
    //        then mzIdentMLXML.Value
    //            |> (fun proteinDetectionItem -> (*proteinDetectionItem.Id*)
    //                                            //(if proteinDetectionItem.Name.IsSome 
    //                                            //    then proteinDetectionItem.Name.Value 
    //                                            //    else null)
    //                                            (convertToEntityProteinAmbiguityGroups dbContext proteinDetectionItem.ProteinAmbiguityGroups),
    //                                            (convertToEntityProteinDetectionListCVParams dbContext proteinDetectionItem.CvParams),
    //                                            (convertToEntityUserParams dbContext proteinDetectionItem.UserParams)
                                                                            
    //               )
    //        else null, null, null               

    //let convertToEntityMeasureCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createMeasureParam (*cvParamItem.Name*)
    //                                                            //cvParamItem.Accession 
    //                                                            (if cvParamItem.Value.IsSome 
    //                                                                then cvParamItem.Value.Value 
    //                                                                else null)
    //                                                            (takeTermID dbContext cvParamItem.CvRef)
    //                                                            (if cvParamItem.UnitCvRef.IsSome 
    //                                                                then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                else null)
    //                                                            (if cvParamItem.UnitName.IsSome 
    //                                                                then cvParamItem.UnitName.Value 
    //                                                                else null)
    //                     )

    //let convertToEntityMesasure  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Measure []) =
    //    mzIdentMLXML
    //        |> Array.map (fun measureItem -> (*measureItem.Id*)
    //                                         //(if measureItem.Name.IsSome 
    //                                         //    then measureItem.Name.Value 
    //                                         //    else null)
    //                                         (convertToEntityMeasureCVParams dbContext measureItem.CvParams
    //                                         )
    //                     ) 
    //                     |> Array.concat
    //                     |> List

    //let convertToEntityFragmentationTable  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.FragmentationTable option) =
    //     if mzIdentMLXML.IsSome 
    //        then mzIdentMLXML.Value
    //             |> (fun fragmentationTableItem -> createFragmentationTable (convertToEntityMesasure dbContext fragmentationTableItem.Measures
    //                                                                        )
    //                )
    //        else createFragmentationTable null


    //let convertToEntityFragmentArray (mzIdentMLXML : SchemePeptideShaker.FragmentArray []) =    
    //    mzIdentMLXML
    //        |> Array.map (fun fragmentItem -> createFragmentArray fragmentItem.Values
    //                                                              fragmentItem.MeasureRef
    //                     ) |> List 

    //let convertToEntityIonTypeCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) = 
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createIonTypeParam (*cvParamItem.Name*)
    //                                                            //cvParamItem.Accession 
    //                                                            (if cvParamItem.Value.IsSome 
    //                                                                then cvParamItem.Value.Value 
    //                                                                else null)
    //                                                            (takeTermID dbContext cvParamItem.CvRef)
    //                                                            (if cvParamItem.UnitCvRef.IsSome 
    //                                                                then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                else null)
    //                                                            (if cvParamItem.UnitName.IsSome 
    //                                                                then cvParamItem.UnitName.Value 
    //                                                                else null)
    //                     ) |> List 

    //let convertToEntityIonTypes  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.IonType []) =
    //    mzIdentMLXML
    //        |> Array.map (fun ionTypeItem -> createIonType (if ionTypeItem.Index.IsSome 
    //                                                           then ionTypeItem.Index.Value 
    //                                                           else null)
    //                                                       ionTypeItem.Charge
    //                                                       (convertToEntityFragmentArray ionTypeItem.FragmentArray
    //                                                       )
    //                                                       (convertToEntityIonTypeCVParams dbContext ionTypeItem.CvParams
    //                                                       )
    //                                                       (convertToEntityUserParams dbContext ionTypeItem.UserParams
    //                                                       )
                                                                                                                                                                                                                                               
    //                     ) |> List

    //let convertToEntityFragmentation  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Fragmentation option) =
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value
    //            |> (fun fragmentationItem -> createFragmentation (convertToEntityIonTypes dbContext fragmentationItem.IonTypes
    //                                                             )
    //               )
    //        else createFragmentation null

    //let convertToEntityPeptideEvidenceRef (mzIdentMLXML : SchemePeptideShaker.PeptideEvidenceRef []) =  
    //    mzIdentMLXML
    //        |> Array.map (fun peptideEvidenceItem -> createPeptideEvidenceRef peptideEvidenceItem.PeptideEvidenceRef
    //                     ) |> List

    //let convertToEntitySpectrumIdentificationItemCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =  
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createSpectrumIdentificationItemParam  (*cvParamItem.Name*)
    //                                                                                //cvParamItem.Accession 
    //                                                                                (if cvParamItem.Value.IsSome 
    //                                                                                    then cvParamItem.Value.Value 
    //                                                                                    else null)
    //                                                                                (takeTermID dbContext cvParamItem.CvRef)
    //                                                                                (if cvParamItem.UnitCvRef.IsSome 
    //                                                                                    then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                                    else null)
    //                                                                                (if cvParamItem.UnitName.IsSome 
    //                                                                                    then cvParamItem.UnitName.Value 
    //                                                                                    else null)
                                                                                                                                                                                                 
    //                     ) |> List 

    //let convertToEntitySpectrumIdentificationItems  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.SpectrumIdentificationItem []) =  
    //    mzIdentMLXML
    //        |> Array.map (fun spectrumIdentificationItem -> createSpectrumIdentificationItem spectrumIdentificationItem.Id
    //                                                                                         //(if spectrumIdentificationItem.Name.IsSome 
    //                                                                                         //    then spectrumIdentificationItem.Name.Value 
    //                                                                                         //    else null)
    //                                                                                         spectrumIdentificationItem.PeptideRef
    //                                                                                         spectrumIdentificationItem.ChargeState
    //                                                                                         (if spectrumIdentificationItem.SampleRef.IsSome 
    //                                                                                             then spectrumIdentificationItem.SampleRef.Value 
    //                                                                                             else null
    //                                                                                         )
    //                                                                                         spectrumIdentificationItem.PassThreshold
    //                                                                                         (convertToEntityFragmentation dbContext spectrumIdentificationItem.Fragmentation
    //                                                                                         )
    //                                                                                         spectrumIdentificationItem.Rank
    //                                                                                         (if spectrumIdentificationItem.MassTableRef.IsSome 
    //                                                                                             then spectrumIdentificationItem.MassTableRef.Value 
    //                                                                                             else null
    //                                                                                         )
    //                                                                                         (if spectrumIdentificationItem.CalculatedPi.IsSome 
    //                                                                                             then spectrumIdentificationItem.CalculatedPi.Value 
    //                                                                                             else null
    //                                                                                         )
    //                                                                                         (if spectrumIdentificationItem.CalculatedMassToCharge.IsSome 
    //                                                                                             then spectrumIdentificationItem.CalculatedMassToCharge.Value 
    //                                                                                             else -1.
    //                                                                                         )
    //                                                                                         spectrumIdentificationItem.ExperimentalMassToCharge
    //                                                                                         (convertToEntityPeptideEvidenceRef spectrumIdentificationItem.PeptideEvidenceRefs
    //                                                                                         )
    //                                                                                         (convertToEntitySpectrumIdentificationItemCVParams dbContext spectrumIdentificationItem.CvParams
    //                                                                                         )
    //                                                                                         (convertToEntityUserParams dbContext spectrumIdentificationItem.UserParams
    //                                                                                         )
                                                                                                                                                                    
    //                     ) |> List


    //let convertToEntitySpectrumIdentificationResultsCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createSpectrumIdentificationResultParam (*cvParamItem.Name*)
    //                                                                                 //cvParamItem.Accession 
    //                                                                                 (if cvParamItem.Value.IsSome 
    //                                                                                     then cvParamItem.Value.Value 
    //                                                                                     else null)
    //                                                                                 (takeTermID dbContext cvParamItem.CvRef)
    //                                                                                 (if cvParamItem.UnitCvRef.IsSome  
    //                                                                                     then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                                     else null)
    //                                                                                 (if cvParamItem.UnitName.IsSome 
    //                                                                                     then cvParamItem.UnitName.Value 
    //                                                                                     else null)
    //                     ) |> List

    //let convertToEntitySpectrumIdentificationResults  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.SpectrumIdentificationResult []) =
    //    mzIdentMLXML
    //        |> Array.map (fun spectrumIdentificationResultItem -> createSpectrumIdentificationResult spectrumIdentificationResultItem.Id
    //                                                                                                 spectrumIdentificationResultItem.SpectrumId
    //                                                                                                 //(if spectrumIdentificationResultItem.Name.IsSome 
    //                                                                                                 //    then spectrumIdentificationResultItem.Name.Value 
    //                                                                                                 //    else null), 
    //                                                                                                 spectrumIdentificationResultItem.SpectraDataRef
    //                                                                                                 (convertToEntitySpectrumIdentificationItems dbContext spectrumIdentificationResultItem.SpectrumIdentificationItems
    //                                                                                                 )
    //                                                                                                 (convertToEntitySpectrumIdentificationResultsCVParams dbContext spectrumIdentificationResultItem.CvParams
    //                                                                                                 )
    //                                                                                                 (convertToEntityUserParams dbContext spectrumIdentificationResultItem.UserParams
    //                                                                                                 )
    //                     ) |> List
 

    //let convertToEntitySpectrumIdentificationListCVParam  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createSpectrumIdentificationListParam (*cvParamItem.Name*)
    //                                                                               //cvParamItem.Accession 
    //                                                                               (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                                               (takeTermID dbContext cvParamItem.CvRef)
    //                                                                               (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                                               (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
                                          
    //                     ) |> List

    //let convertToEntitySpectrumIdentificationList  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.SpectrumIdentificationList []) =
    //    mzIdentMLXML
    //        |> Array.map (fun spectrumIdentificationItem -> createSpectrumIdentificationList spectrumIdentificationItem.Id
    //                                                                                         //(if spectrumIdentificationItem.Name.IsSome 
    //                                                                                         //    then spectrumIdentificationItem.Name.Value 
    //                                                                                         //    else null), 
    //                                                                                         (if spectrumIdentificationItem.NumSequencesSearched.IsSome 
    //                                                                                             then int spectrumIdentificationItem.NumSequencesSearched.Value 
    //                                                                                             else int -1
    //                                                                                         )
    //                                                                                         (convertToEntityFragmentationTable dbContext spectrumIdentificationItem.FragmentationTable
    //                                                                                         )
    //                                                                                         (convertToEntitySpectrumIdentificationResults dbContext spectrumIdentificationItem.SpectrumIdentificationResults
    //                                                                                         )
    //                                                                                         (convertToEntitySpectrumIdentificationListCVParam dbContext spectrumIdentificationItem.CvParams
    //                                                                                         ) 
    //                                                                                         (convertToEntityUserParams dbContext spectrumIdentificationItem.UserParams
    //                                                                                         )
    //                     ) |> List
 
    //let convertToEntityAnalysisData  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.AnalysisData) =    
    //    mzIdentMLXML
    //    |> (fun analysisDataItem -> createAnalysisData (convertToEntitySpectrumIdentificationList dbContext analysisDataItem.SpectrumIdentificationList)
    //                                                   (if analysisDataItem.ProteinDetectionList.IsSome 
    //                                                       then (convertToEntityProteinDetectionList dbContext analysisDataItem.ProteinDetectionList
    //                                                             |> (fun (proteinAmbiguitiyGroups, cvProteinDetections, userProteinDetections) -> proteinAmbiguitiyGroups  
    //                                                                )
    //                                                   )
    //                                                       else null) 
    //                                                   (if analysisDataItem.ProteinDetectionList.IsSome 
    //                                                       then (convertToEntityProteinDetectionList dbContext analysisDataItem.ProteinDetectionList
    //                                                             |> (fun (proteinAmbiguitiyGroups, cvProteinDetections, userProteinDetections) -> cvProteinDetections 
    //                                                                )
    //                                                   )
    //                                                       else null)
    //                                                   (if analysisDataItem.ProteinDetectionList.IsSome 
    //                                                       then (convertToEntityProteinDetectionList dbContext analysisDataItem.ProteinDetectionList
    //                                                             |> (fun (proteinAmbiguitiyGroups, cvProteinDetections, userProteinDetections) -> userProteinDetections
    //                                                            )    
    //                                                   )
    //                                                       else null)
                                                   
    //       ) 

    //let convertToEntitySearchDatabasesCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createSearchDatabaseParam (*cvParamItem.Name*)
    //                                                                   //cvParamItem.Accession 
    //                                                                   (if cvParamItem.Value.IsSome 
    //                                                                       then cvParamItem.Value.Value 
    //                                                                       else null)
    //                                                                   (takeTermID dbContext cvParamItem.CvRef)
    //                                                                   (if cvParamItem.UnitCvRef.IsSome 
    //                                                                       then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                       else null)
    //                                                                   (if cvParamItem.UnitName.IsSome 
    //                                                                       then cvParamItem.UnitName.Value 
    //                                                                       else null)
    //                                                                    ) |> List

    //let convertToEntityDatabaseNameCVParam  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam option) =  
    //    if mzIdentMLXML.IsSome 
    //        then mzIdentMLXML.Value
    //                |> (fun cvParamItem -> (createDatabaseNameParam (*cvParamItem.Name*)
    //                                                                //cvParamItem.Accession 
    //                                                                (if cvParamItem.Value.IsSome 
    //                                                                    then cvParamItem.Value.Value 
    //                                                                    else null)
    //                                                                (takeTermID dbContext cvParamItem.CvRef)
    //                                                                (if cvParamItem.UnitCvRef.IsSome 
    //                                                                    then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                    else null)
    //                                                                (if cvParamItem.UnitName.IsSome 
    //                                                                    then cvParamItem.UnitName.Value 
    //                                                                    else null)
    //                                        )
    //                    )
    //        else createDatabaseNameParam null null null null

    //let convertToEntityDatabaseName  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.DatabaseName) =  
    //    mzIdentMLXML
    //    |> (fun databaseNameItem -> (convertToEntityDatabaseNameCVParam dbContext databaseNameItem.CvParam
    //                                ),
    //                                (convertToEntityUserParam dbContext databaseNameItem.UserParam
    //                                )
    //       )

    //let convertToEntityFileFormatSearchDataBase (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) = 
    //    mzIdentMLXML.DataCollection.Inputs
    //        |> (fun inputItem -> inputItem.SearchDatabases
    //                            |> Array.map (fun searchDatabaseItem -> searchDatabaseItem.FileFormat
    //                                                                    |> (fun fileFormatItem -> fileFormatItem.CvParam)))

    //let convertToEntitySourceFileCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =  
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createSourceFileParam (*cvParamItem.Name*)
    //                                                               //cvParamItem.Accession 
    //                                                               (if cvParamItem.Value.IsSome 
    //                                                                   then cvParamItem.Value.Value 
    //                                                                   else null
    //                                                               )
    //                                                               (takeTermID dbContext cvParamItem.CvRef)
    //                                                               (if cvParamItem.UnitCvRef.IsSome 
    //                                                                   then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                   else null
    //                                                               )
    //                                                               (if cvParamItem.UnitName.IsSome 
    //                                                                   then cvParamItem.UnitName.Value 
    //                                                                   else null
    //                                                               )
    //                     ) |> List

    //let convertToEntityFileFormatCVParam  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam) =
    //    mzIdentMLXML
    //        |> (fun cvParamItem -> createFileFormatParam (*cvParamItem.Name*)
    //                                                     //cvParamItem.Accession 
    //                                                     (if cvParamItem.Value.IsSome 
    //                                                         then cvParamItem.Value.Value 
    //                                                         else null
    //                                                     )
    //                                                     (takeTermID dbContext cvParamItem.CvRef)
    //                                                     (if cvParamItem.UnitCvRef.IsSome 
    //                                                         then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                         else null
    //                                                     )
    //                                                     (if cvParamItem.UnitName.IsSome 
    //                                                         then cvParamItem.UnitName.Value 
    //                                                         else null
    //                                                     )
    //           )

    //let convertToEntityFileFormat  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.FileFormat) =
    //    mzIdentMLXML
    //        |> (fun fileFormatItem -> (convertToEntityFileFormatCVParam dbContext fileFormatItem.CvParam)
    //           )

    //let convertToEntitySourceFiles  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.SourceFile []) = 
    //    mzIdentMLXML
    //        |> Array.map (fun sourceFileItem -> createSourceFile (sourceFileItem.Id
    //                                                             )
    //                                                             //(if sourceFileItem.Name.IsSome 
    //                                                             //    then sourceFileItem.Name.Value 
    //                                                             //    else null
    //                                                             //)
    //                                                             (sourceFileItem.Location
    //                                                             )
    //                                                             (convertToEntityFileFormat dbContext sourceFileItem.FileFormat
    //                                                             )
    //                                                             (if sourceFileItem.ExternalFormatDocumentation.IsSome 
    //                                                                 then sourceFileItem.ExternalFormatDocumentation.Value 
    //                                                                 else null
    //                                                             )    
    //                                                             (convertToEntitySourceFileCVParams dbContext sourceFileItem.CvParams
    //                                                             ) 
    //                                                             (convertToEntityUserParams dbContext sourceFileItem.UserParams
    //                                                             )
    //                     ) |> List

    //let convertToEntitySpectrumIDFormatCvParam  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam) =
    //    mzIdentMLXML
    //        |> (fun cvParamItem -> createSpectrumIDFormatParam (*cvParamItem.Name*)
    //                                                           //cvParamItem.Accession 
    //                                                           (if cvParamItem.Value.IsSome 
    //                                                               then cvParamItem.Value.Value 
    //                                                               else null
    //                                                           )
    //                                                           (takeTermID dbContext cvParamItem.CvRef)
    //                                                           (if cvParamItem.UnitCvRef.IsSome 
    //                                                               then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                               else null
    //                                                           )
    //                                                           (if cvParamItem.UnitName.IsSome 
    //                                                               then cvParamItem.UnitName.Value 
    //                                                               else null
    //                                                           )
    //           )

    //let convertToEntitySpectrumIDFormat  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.SpectrumIdFormat) =
    //    mzIdentMLXML
    //        |> (fun spectrumIDFormatItem -> (convertToEntitySpectrumIDFormatCvParam dbContext spectrumIDFormatItem.CvParam)
    //           )

    //let convertToEntitySpectraDatas  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.SpectraData []) =
    //    mzIdentMLXML
    //        |> Array.map (fun spectraDataItem -> createSpectraData (spectraDataItem.Id
    //                                                               )
    //                                                               //(if spectraDataItem.Name.IsSome 
    //                                                               //    then spectraDataItem.Name.Value 
    //                                                               //    else null)
    //                                                               (spectraDataItem.Location
    //                                                               )
    //                                                               (convertToEntityFileFormat dbContext spectraDataItem.FileFormat
    //                                                               )
    //                                                               (if spectraDataItem.ExternalFormatDocumentation.IsSome 
    //                                                                   then spectraDataItem.ExternalFormatDocumentation.Value 
    //                                                                   else null
    //                                                               ) 
    //                                                               (convertToEntitySpectrumIDFormat dbContext spectraDataItem.SpectrumIdFormat
    //                                                               )
    //                     ) |> List

    //let convertToEntitySearchDatabases  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.SearchDatabase []) =   
    //    mzIdentMLXML
    //        |> Array.map (fun searchDatabaseItem -> createSearchDatabase (searchDatabaseItem.Id
    //                                                                     )
    //                                                                     (searchDatabaseItem.Location
    //                                                                     )
    //                                                                     (if searchDatabaseItem.NumResidues.IsSome 
    //                                                                         then int searchDatabaseItem.NumResidues.Value 
    //                                                                         else -1
    //                                                                     )
    //                                                                     (if searchDatabaseItem.NumDatabaseSequences.IsSome 
    //                                                                         then int searchDatabaseItem.NumDatabaseSequences.Value 
    //                                                                         else -1
    //                                                                     )
    //                                                                     (if searchDatabaseItem.ReleaseDate.IsSome 
    //                                                                         then searchDatabaseItem.ReleaseDate.Value 
    //                                                                         else DateTime.UtcNow
    //                                                                     )
    //                                                                     (if searchDatabaseItem.Version.IsSome 
    //                                                                         then searchDatabaseItem.Version.Value 
    //                                                                         else null
    //                                                                     )
    //                                                                     //(if searchDatabaseItem.Name.IsSome 
    //                                                                     //    then searchDatabaseItem.Name.Value 
    //                                                                     //    else null), 
    //                                                                     (if searchDatabaseItem.ExternalFormatDocumentation.IsSome 
    //                                                                         then searchDatabaseItem.ExternalFormatDocumentation.Value 
    //                                                                         else null
    //                                                                     )
    //                                                                     (convertToEntityFileFormat dbContext searchDatabaseItem.FileFormat
    //                                                                     )
    //                                                                     (fst(convertToEntityDatabaseName dbContext searchDatabaseItem.DatabaseName
    //                                                                     ))
    //                                                                     (snd(convertToEntityDatabaseName dbContext searchDatabaseItem.DatabaseName
    //                                                                     ))
    //                                                                     (convertToEntitySearchDatabasesCVParams dbContext searchDatabaseItem.CvParams
    //                                                                     ) 
    //                     ) |> List

    //let convertToEntityInputs  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Inputs) =
    //    mzIdentMLXML
    //        |> (fun inputItem -> createInputs (convertToEntitySearchDatabases dbContext inputItem.SearchDatabases
    //                                          )
    //                                          (convertToEntitySourceFiles dbContext inputItem.SourceFiles
    //                                          )
    //                                          (convertToEntitySpectraDatas dbContext inputItem.SpectraDatas
    //                                          )
    //           )

    //let convertToEntityDataCollection  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.DataCollection) =
    //    mzIdentMLXML
    //        |> (fun dataCollectionItem -> (convertToEntityInputs dbContext dataCollectionItem.Inputs
    //                                      ),
    //                                      (convertToEntityAnalysisData dbContext dataCollectionItem.AnalysisData
    //                                      )
                                                           
    //           )

    //let convertToEntityProvider  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =
    //    if mzIdentMLXML.Provider.IsSome 
    //       then mzIdentMLXML.Provider.Value
    //           |> (fun providerItem -> createProvider (providerItem.Id
    //                                                  )
    //                                                  //(if providerItem.Name.IsSome 
    //                                                  //    then providerItem.Name.Value 
    //                                                  //    else null),
    //                                                  ((convertToEntityContactRoleOption dbContext providerItem.ContactRole) 
    //                                                    |> (fun (contactRef, roleParam) -> createContactRole contactRef roleParam)
    //                                                  )
    //                                                  (if providerItem.AnalysisSoftwareRef.IsSome 
    //                                                      then providerItem.AnalysisSoftwareRef.Value 
    //                                                      else null
    //                                                  )
    //               )
    //        else createProvider null (createContactRole null (createRoleParam null null null null)) null

    //let convertToEntityModificationCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) = 
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createModificationParam (*cvParamItem.Name*)
    //                                                                 //cvParamItem.Accession 
    //                                                                 (if cvParamItem.Value.IsSome 
    //                                                                     then cvParamItem.Value.Value 
    //                                                                     else null
    //                                                                 )
    //                                                                 (takeTermID dbContext cvParamItem.CvRef)
    //                                                                 (if cvParamItem.UnitCvRef.IsSome 
    //                                                                     then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                     else null
    //                                                                 )                                                                (if cvParamItem.UnitName.IsSome 
    //                                                                     then cvParamItem.UnitName.Value 
    //                                                                     else null
    //                                                                 )
    //                     ) |> List

    //let convertToEntityModifications  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Modification []) = 
    //    mzIdentMLXML  
    //        |> Array.map (fun modificationItem -> createModification (if modificationItem.AvgMassDelta.IsSome 
    //                                                                     then modificationItem.AvgMassDelta.Value 
    //                                                                     else -1.
    //                                                                 )
    //                                                                 (if modificationItem.MonoisotopicMassDelta.IsSome 
    //                                                                     then modificationItem.MonoisotopicMassDelta.Value 
    //                                                                     else -1.
    //                                                                 )
    //                                                                 (if modificationItem.Residues.IsSome 
    //                                                                     then modificationItem.Residues.Value 
    //                                                                     else null
    //                                                                 )
    //                                                                 (if modificationItem.Location.IsSome 
    //                                                                     then modificationItem.Location.Value 
    //                                                                     else -1
    //                                                                 )
    //                                                                 (convertToEntityModificationCVParams dbContext modificationItem.CvParams
    //                                                                 )
    //                     ) |> List

    //let convertToEntitySubstitutionModification (mzIdentMLXML : SchemePeptideShaker.SubstitutionModification []) =   
    //    mzIdentMLXML
    //        |> Array.map (fun substitutionModificationItem -> createSubstitutionModification (if substitutionModificationItem.MonoisotopicMassDelta.IsSome 
    //                                                                                             then substitutionModificationItem.MonoisotopicMassDelta.Value 
    //                                                                                             else -1.
    //                                                                                         ) 
    //                                                                                         (if substitutionModificationItem.AvgMassDelta.IsSome 
    //                                                                                             then substitutionModificationItem.AvgMassDelta.Value 
    //                                                                                             else -1.
    //                                                                                         )
    //                                                                                         (if substitutionModificationItem.Location.IsSome 
    //                                                                                             then substitutionModificationItem.Location.Value 
    //                                                                                             else -1
    //                                                                                         )
    //                                                                                         (substitutionModificationItem.OriginalResidue
    //                                                                                         )
    //                                                                                         (substitutionModificationItem.ReplacementResidue
    //                                                                                         )
    //                     ) |> List

    //let convertToEntityPeptideCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =   
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createPeptideParam (*cvParamItem.Name*)
    //                                                            //cvParamItem.Accession 
    //                                                            (if cvParamItem.Value.IsSome 
    //                                                                then cvParamItem.Value.Value 
    //                                                                else null
    //                                                            )
    //                                                            (takeTermID dbContext cvParamItem.CvRef)
    //                                                            (if cvParamItem.UnitCvRef.IsSome 
    //                                                                then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                else null
    //                                                            )
    //                                                            (if cvParamItem.UnitName.IsSome 
    //                                                                then cvParamItem.UnitName.Value 
    //                                                                else null
    //                                                            )
    //                     ) |> List

    //let convertToEntityPeptides  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Peptide []) =    
    //    mzIdentMLXML
    //        |> Array.map (fun peptideItem -> createPeptide (peptideItem.Id
    //                                                       )
    //                                                       //(if peptideItem.Name.IsSome 
    //                                                       //    then peptideItem.Name.Value 
    //                                                       //    else null
    //                                                       //)
    //                                                       (peptideItem.PeptideSequence
    //                                                       )
    //                                                       (convertToEntityModifications dbContext peptideItem.Modifications
    //                                                       )
    //                                                       (convertToEntitySubstitutionModification peptideItem.SubstitutionModifications
    //                                                       )
    //                                                       (convertToEntityPeptideCVParams dbContext peptideItem.CvParams
    //                                                       )
    //                                                       (convertToEntityUserParams dbContext peptideItem.UserParams
    //                                                       )
    //                     ) |> List

    //let convertToEntityPeptideEvidenceCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createPeptideEvidenceParam (*cvParamItem.Name*)
    //                                                                    //cvParamItem.Accession 
    //                                                                    (if cvParamItem.Value.IsSome 
    //                                                                        then cvParamItem.Value.Value 
    //                                                                        else null
    //                                                                    )
    //                                                                    (takeTermID dbContext cvParamItem.CvRef)
    //                                                                    (if cvParamItem.UnitCvRef.IsSome 
    //                                                                        then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                        else null
    //                                                                    )
    //                                                                    (if cvParamItem.UnitName.IsSome 
    //                                                                        then cvParamItem.UnitName.Value 
    //                                                                        else null
    //                                                                    )
    //                     ) |> List

    //let convertToEntityPeptideEvidence  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.PeptideEvidence []) = 
    //    mzIdentMLXML
    //        |> Array.map (fun peptideEvidenceItem -> createPeptideEvidence  (peptideEvidenceItem.Id
    //                                                                        )
    //                                                                        //(if peptideEvidenceItem.Name.IsSome 
    //                                                                        //    then peptideEvidenceItem.Name.Value 
    //                                                                        //    else null
    //                                                                        //),  
    //                                                                        (peptideEvidenceItem.PeptideRef
    //                                                                        )
    //                                                                        (peptideEvidenceItem.DBSequenceRef
    //                                                                        )
    //                                                                        (if peptideEvidenceItem.IsDecoy.IsSome 
    //                                                                            then peptideEvidenceItem.IsDecoy.Value.ToString() 
    //                                                                            else null
    //                                                                        )
    //                                                                        (if peptideEvidenceItem.Frame.IsSome 
    //                                                                            then peptideEvidenceItem.Frame.Value 
    //                                                                            else -1
    //                                                                        )
    //                                                                        (if peptideEvidenceItem.TranslationTableRef.IsSome 
    //                                                                            then peptideEvidenceItem.TranslationTableRef.Value 
    //                                                                            else null
    //                                                                        )
    //                                                                        (if peptideEvidenceItem.Start.IsSome 
    //                                                                            then peptideEvidenceItem.Start.Value 
    //                                                                            else -1
    //                                                                        )
    //                                                                        (if peptideEvidenceItem.End.IsSome 
    //                                                                            then peptideEvidenceItem.End.Value 
    //                                                                            else -1
    //                                                                        )
    //                                                                        (if peptideEvidenceItem.Pre.IsSome 
    //                                                                            then peptideEvidenceItem.Pre.Value 
    //                                                                            else null
    //                                                                        )
    //                                                                        (if peptideEvidenceItem.Post.IsSome 
    //                                                                            then peptideEvidenceItem.Post.Value 
    //                                                                            else null
    //                                                                        )
    //                                                                        (convertToEntityPeptideEvidenceCVParams dbContext peptideEvidenceItem.CvParams
    //                                                                        )
    //                                                                        (convertToEntityUserParams dbContext peptideEvidenceItem.UserParams
    //                                                                        )
    //                     ) |> List

    //let convertToEntityDBSequenceCVParams  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createDBSequenceParam (*cvParamItem.Name*)
    //                                                               //cvParamItem.Accession 
    //                                                               (if cvParamItem.Value.IsSome 
    //                                                                   then cvParamItem.Value.Value 
    //                                                                   else null
    //                                                               )
    //                                                               (takeTermID dbContext cvParamItem.CvRef)
    //                                                               (if cvParamItem.UnitCvRef.IsSome 
    //                                                                   then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                   else null
    //                                                               )
    //                                                               (if cvParamItem.UnitName.IsSome 
    //                                                                   then cvParamItem.UnitName.Value 
    //                                                                   else null
    //                                                               )
    //                     ) |> List

    //let convertToEntityDBSequence  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.DbSequence []) =  
    //    mzIdentMLXML
    //        |> Array.map (fun dbSequenceItem -> createDBSequence (dbSequenceItem.Id
    //                                                             )
    //                                                             //(if dbSequenceItem.Name.IsSome 
    //                                                             //    then dbSequenceItem.Name.Value 
    //                                                             //    else null
    //                                                             //)
    //                                                             (dbSequenceItem.Accession
    //                                                             )
    //                                                             (if dbSequenceItem.Length.IsSome 
    //                                                                 then dbSequenceItem.Length.Value 
    //                                                                 else -1
    //                                                             )
    //                                                             (if dbSequenceItem.Seq.IsSome 
    //                                                                 then dbSequenceItem.Seq.Value 
    //                                                                 else null
    //                                                             )  
    //                                                             (dbSequenceItem.SearchDatabaseRef
    //                                                             )
    //                                                             (convertToEntityDBSequenceCVParams dbContext dbSequenceItem.CvParams
    //                                                             )
    //                                                             (convertToEntityUserParams dbContext dbSequenceItem.UserParams
    //                                                             )
    //                     ) |> List

    //let convertToEntitySequenceCollection  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =    
    //    if mzIdentMLXML.SequenceCollection.IsSome 
    //       then mzIdentMLXML.SequenceCollection.Value
    //            |> (fun sequenceCollectionItem -> (convertToEntityPeptides dbContext sequenceCollectionItem.Peptides), 
    //                                              (convertToEntityPeptideEvidence dbContext sequenceCollectionItem.PeptideEvidences),
    //                                              (convertToEntityDBSequence dbContext sequenceCollectionItem.DbSequences)
    //               )
    //        else null, null, null 

    //let convertToEntityContactRoles  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.ContactRole []) =
    //    mzIdentMLXML
    //        |> Array.map (fun contactRoleItem -> createContactRole contactRoleItem.ContactRef (convertToEntityRole dbContext contactRoleItem.Role)
    //                     ) |> List

    //let convertToEntitySubSamples  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.SubSample []) =
    //    mzIdentMLXML
    //        |> Array.map (fun subSampleItem -> createSubSample subSampleItem.SampleRef
    //                     ) |> List

    //let convertToEntitySampleCVParam  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createSampleParam (*cvParamItem.Name*)
    //                                                           //cvParamItem.Accession 
    //                                                           (if cvParamItem.Value.IsSome 
    //                                                               then cvParamItem.Value.Value 
    //                                                               else null
    //                                                           )
    //                                                           (takeTermID dbContext cvParamItem.CvRef)
    //                                                           (if cvParamItem.UnitCvRef.IsSome 
    //                                                               then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                               else null
    //                                                           )
    //                                                           (if cvParamItem.UnitName.IsSome 
    //                                                               then cvParamItem.UnitName.Value 
    //                                                               else null
    //                                                           )
    //                     ) |> List

    //let convertToEntitySamples  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.Sample []) =
    //    mzIdentMLXML
    //        |> Array.map (fun sampleItem -> createSample (sampleItem.Id
    //                                                     )
    //                                                     //(if sampleItem.Name.IsSome
    //                                                     //    then sampleItem.Name.Value
    //                                                     //    else null
    //                                                     //),
    //                                                     (convertToEntityContactRoles dbContext sampleItem.ContactRoles
    //                                                     )
    //                                                     (convertToEntitySubSamples dbContext sampleItem.SubSamples
    //                                                     )
    //                                                     (convertToEntitySampleCVParam dbContext sampleItem.CvParams
    //                                                     )
    //                                                     (convertToEntityUserParams dbContext sampleItem.UserParams
    //                                                     )
    //                     ) |> List

    //let convertToEntityAnalysisSampleCollection  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =
    //    if mzIdentMLXML.AnalysisSampleCollection.IsSome 
    //       then mzIdentMLXML.AnalysisSampleCollection.Value
    //            |> (fun analysisSampleCollectionItem -> (convertToEntitySamples dbContext analysisSampleCollectionItem.Samples)
    //               )
    //       else null

    //let convertToEntityAnalyisCollection  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =
    //    mzIdentMLXML.AnalysisCollection
    //        |> (fun analyisCollectionItem -> (convertToEntityProteinDetection dbContext analyisCollectionItem.ProteinDetection
    //                                         ),
    //                                         (convertToEntitySpectrumIdentifications analyisCollectionItem.SpectrumIdentifications
    //                                         )
    //           )

    //let convertToEntityBibliographicReferences (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =
    //    mzIdentMLXML.BibliographicReferences
    //        |> Array.map (fun bibliographicReferenceItem -> 
    //            createBibliographicReference 
    //                (
    //                bibliographicReferenceItem.Id
    //                )
    //                //(if bibliographicReferenceItem.Name.IsSome
    //                //    then bibliographicReferenceItem.Name.Value
    //                //    else null
    //                //)
    //                (if bibliographicReferenceItem.Issue.IsSome
    //                    then bibliographicReferenceItem.Issue.Value
    //                    else null
    //                )
    //                (if bibliographicReferenceItem.Title.IsSome
    //                    then bibliographicReferenceItem.Title.Value
    //                    else null
    //                )
    //                (if bibliographicReferenceItem.Pages.IsSome
    //                    then bibliographicReferenceItem.Pages.Value
    //                    else null
    //                )
    //                (if bibliographicReferenceItem.Volume.IsSome
    //                    then bibliographicReferenceItem.Volume.Value
    //                    else null
    //                )
    //                (if bibliographicReferenceItem.Doi.IsSome
    //                    then bibliographicReferenceItem.Doi.Value
    //                    else null
    //                )
    //                (if bibliographicReferenceItem.Editor.IsSome
    //                    then bibliographicReferenceItem.Editor.Value
    //                    else null
    //                )
    //                (if bibliographicReferenceItem.Publication.IsSome
    //                    then bibliographicReferenceItem.Publication.Value
    //                    else null
    //                )
    //                (if bibliographicReferenceItem.Publisher.IsSome 
    //                    then bibliographicReferenceItem.Publisher.Value
    //                    else null
    //                )
    //                (if bibliographicReferenceItem.Year.IsSome
    //                    then bibliographicReferenceItem.Year.Value
    //                    else -1
    //                )
    //                (if bibliographicReferenceItem.Authors.IsSome
    //                    then bibliographicReferenceItem.Authors.Value
    //                    else null
    //                )
    //                     ) |> List


    //let convertToEntityMzIdentML  (dbContext : MzIdentML) (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =
    //    createMzIdentML (mzIdentMLXML.Id
    //                    )
    //                    //(if mzIdentMLXML.Name.IsSome 
    //                    //    then mzIdentMLXML.Name.Value 
    //                    //    else null
    //                    //)
    //                    (mzIdentMLXML.Version
    //                    )
    //                    (if mzIdentMLXML.CreationDate.IsSome 
    //                        then mzIdentMLXML.CreationDate.Value 
    //                        else DateTime.UtcNow
    //                    )
    //                    (convertToEntityCVs mzIdentMLXML
    //                    )
    //                    (convertToEntityAnalysisSoftwareList dbContext mzIdentMLXML
    //                    )
    //                    (convertToEntityProvider dbContext mzIdentMLXML
    //                    )
    //                    (snd(convertToEntityAuditCollection dbContext mzIdentMLXML
    //                    ))
    //                    (fst(convertToEntityAuditCollection dbContext mzIdentMLXML
    //                    ))
    //                    (convertToEntityAnalysisSampleCollection dbContext mzIdentMLXML
    //                    )
    //                    ((convertToEntitySequenceCollection dbContext mzIdentMLXML
    //                    ) |> (fun (_,_,dbSequences) -> dbSequences)
    //                    )
    //                    ((convertToEntitySequenceCollection dbContext mzIdentMLXML
    //                    ) |> (fun (peptides,_,_) -> peptides)
    //                    )
    //                    ((convertToEntitySequenceCollection dbContext mzIdentMLXML
    //                    ) |> (fun (_,peptideEvidence,_) -> peptideEvidence)
    //                    )
    //                    (snd(convertToEntityAnalyisCollection dbContext mzIdentMLXML
    //                    ))
    //                    (fst(convertToEntityAnalyisCollection dbContext mzIdentMLXML
    //                    ))
    //                    (fst(convertToEntityAnalysisProtocolCollection dbContext mzIdentMLXML.AnalysisProtocolCollection
    //                    ))
    //                    (snd(convertToEntityAnalysisProtocolCollection dbContext mzIdentMLXML.AnalysisProtocolCollection
    //                    ))
    //                    (fst(convertToEntityDataCollection dbContext mzIdentMLXML.DataCollection
    //                    ))
    //                    (snd(convertToEntityDataCollection dbContext mzIdentMLXML.DataCollection
    //                    ))
    //                    (convertToEntityBibliographicReferences mzIdentMLXML
    //                    ) 

    ////Testing insertStatements

    ////takeTermEntries

    ////let newPath = fileDir + "\Ontologies_Terms\MSDatenbank2.db"
    //let newPath = fileDir + "\Ontologies_Terms\MSDatenbank3.db"
    //let newContext = configureSQLiteServerContext newPath
    //initDB newPath

    //let dbMzIdentML = convertToEntityMzIdentML newContext (SchemePeptideShaker.Load("..\ExampleFile\PeptideShaker_mzid_1_2_example.mzid"))
    //1+1
    ////let testi =
    ////    removeDoubleTerms "..\ExampleFile\PeptideShaker_mzid_1_2_example.mzid"
    ////    |> keepDuplicates
    ////    |> removeExistingEntries standardDBPath


    //context.Add(dbMzIdentML)
    //context.SaveChanges()


    ////Write SQLServerDB

    ////let initSQLServer dbPath =
    ////    let dbContext = configureSQLServerContext dbPath
    ////    dbContext.Database.EnsureCreated()

    ////initSQLServer "SQLServer"


    ////Abstraction of the MZIdentML-DB


    /////CVParams of MzIdentML.
    //type CVParam =
    //    { 
    //     mutable IsCVParam   : bool
    //     mutable TermRef     : string
    //     mutable UnitRef     : string
    //     mutable UnitName    : string
    //     mutable Value       : string
    //    }

    ///////CVParams of MzIdentML.
    ////type [<CLIMutable>] 
    ////    AnalysisSoftware_Abstract =
    ////    {
    ////     mutable Name           : string
    ////     mutable URI            : string
    ////     mutable Version        : string
    ////     mutable Contact_Ref    : string
    ////     mutable Customizations : string
    ////     mutable CVParam_Role   : CVParam
    ////     mutable SoftwareName   : CVParam
    ////    }


    //type CvParam_Handler =
    //       static member init_CvParam
    //            (
    //                origin   : bool,

    //                ?termRef  : string,

    //                ?unitRef  : string,

    //                ?unitName : string,
                
    //                ?value    : string
    //            ) = 
    //            let termRef'  = defaultArg termRef  null
    //            let unitRef'  = defaultArg unitRef  null
    //            let unitName' = defaultArg unitName null
    //            let value'    = defaultArg value    null
    //            {CVParam.IsCVParam = origin; CVParam.TermRef = termRef'; CVParam.UnitRef = unitRef'; CVParam.UnitName = unitName'; CVParam.Value = value'}

    //let test = CvParam_Handler.init_CvParam(true, "tralala", null, null, "Wert")

    //type AnalysisSoftware_Abstract_Handler =
    //     static member init_AnalysisSoftware_Abstract
    //            (
    //                softwareName    : CVParam,

    //                ?name           : string,

    //                ?uri            : string,

    //                ?version        : string,

    //                ?contact_ref    : string,

    //                ?customizations : string,

    //                ?cvRole         : CVParam

    //            ) =
    //            let name'           = defaultArg name null
    //            let uri'            = defaultArg uri  null
    //            let version'        = defaultArg version null
    //            let contact_ref'    = defaultArg contact_ref null
    //            let customizations' = defaultArg customizations null
    //            let cvRole'         = defaultArg cvRole Unchecked.defaultof<CVParam>
    //            {Name = name'; URI = uri'; Version = version';
    //             ContactRef = contact_ref'; Customizations = customizations';
    //             CVParamRole = cvRole'; SoftwareName = softwareName
    //            }

    //let aAH = AnalysisSoftware_Abstract_Handler.init_AnalysisSoftware_Abstract(test, null, null, null, null, null)

    //let addToAnalysisSoftware (firstItem : AnalysisSoftware_Abstract) =
    //    createAnalysisSoftware
    //        firstItem.Name
    //        firstItem.URI
    //        firstItem.Contact_Ref
    //        (createRoleParam 
    //            firstItem.CVParam_Role.Value
    //            firstItem.CVParam_Role.TermRef
    //            firstItem.CVParam_Role.UnitRef
    //            firstItem.CVParam_Role.UnitName
    //        )
    //        (if firstItem.SoftwareName.IsCVParam = true 
    //            then createSoftwareNameParam 
    //                    firstItem.SoftwareName.Value
    //                    firstItem.SoftwareName.TermRef
    //                    firstItem.SoftwareName.UnitRef
    //                    firstItem.SoftwareName.UnitName
    //            else createSoftwareNameParam null null null null
    //        )
    //        (if firstItem.SoftwareName.IsCVParam = false 
    //            then createUserParam
    //                    firstItem.SoftwareName.Value
    //                    firstItem.SoftwareName.TermRef
    //                    firstItem.SoftwareName.UnitRef
    //                    firstItem.SoftwareName.UnitName
    //            else createUserParam null null null null
    //        )
    //        firstItem.Customizations
    //        firstItem.Version


    ////type [<CLIMutable>] 
    ////    AnalysisSoftware =
    ////    {
    ////    [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
    ////    ID                     : int
    ////    Name                   : string
    ////    URI                    : string
    ////    ContactRef             : string
    ////    CVParam_Role           : RoleParam
    ////    CVParam_SoftwareName   : SoftwareNameParam
    ////    UserParam_SoftwareName : UserParam
    ////    Customizations         : string
    ////    Version                : string
    ////    RowVersion             : DateTime
    ////    //AnalysisSoftwareParams : List<AnalysisSoftwareParam>  
    ////    }

    //type AnalysisSoftwareHandler =
    //       static member init
    //            (
    //                id   : int,

    //                ?Name: string,
    //                ?Role : RoleParam,
    //                ?SoftwareName :  SoftwareNameParam                
    //            ) = 
    //            let name'      = defaultArg Name null
    //            let role' = defaultArg Role Unchecked.defaultof<RoleParam>
    //            let software' = defaultArg SoftwareName Unchecked.defaultof<SoftwareNameParam>
    //            {
    //                ID                     = id;
    //                Name                   = name';
    //                URI                    = "";
    //                ContactRef             = "";
    //                CVParam_Role           = role';
    //                CVParam_SoftwareName   = software';
    //                UserParam_SoftwareName = Unchecked.defaultof<UserParam>;
    //                Customizations         = ""
    //                Version                = ""
    //                RowVersion             = DateTime.Now
    //                //AnalysisSoftwareParams : List<AnalysisSoftwareParam>  
    //            }


    //AnalysisSoftwareHandler.init(1)

