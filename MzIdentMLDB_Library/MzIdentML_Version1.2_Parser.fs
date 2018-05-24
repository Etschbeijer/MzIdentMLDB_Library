namespace MzIdentMLDataBase

open DataContext.DataContext
open InsertStatements.ObjectHandlers
open System.Collections.Generic
//open FSharp.Care.IO
//open BioFSharp.IO


module xmlParsing =

    //#r @"..\packages\FSharp.Data.2.4.6\lib\net45\FSharp.Data.dll"
    //#r @"..\packages\FSharp.Data.Xsd.1.0.2\lib\net45\FSharp.Data.Xsd.dll"
    //#r "System.Xml.Linq.dll"

    open FSharp.Data

    type SchemePeptideShaker = XmlProvider<Schema = "..\MzIdentMLDB_Library\XML_Files\MzIdentMLScheme1_2.xsd">
    //let  samplePeptideShaker = SchemePeptideShaker.Load("..\ExampleFile\PeptideShaker_mzid_1_2_example.txt") 


    let removeDoubleTerms (pathXML : string) =
        let xmlMzIdentML = SchemePeptideShaker.Load(pathXML)
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

    //let takeTermEntries (dbContext : MzIdentMLContext) xmlTermIDList =
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

    //let takeTermID (dbContext : MzIdentMLContext) xmlTermIDList =
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

    //Define InsertStatements////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    //let convertToEntityUserParams (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.UserParam []) =   
    //    mzIdentMLXML
    //        |> Array.map (fun userParamItem -> (createUserParam (if userParamItem.Value.IsSome 
    //                                                                then userParamItem.Value.Value 
    //                                                                else null)
    //                                                            //(if userParamItem.Type.IsSome 
    //                                                            //    then userParamItem.Type.Value 
    //                                                            //    else null)
    //                                                            (if userParamItem.UnitCvRef.IsSome 
    //                                                                then takeTermID dbContext userParamItem.UnitCvRef.Value
    //                                                                else null)
    //                                                            //(if userParamItem.UnitAccession.IsSome 
    //                                                            //    then takeTermID dbContext userParamItem.UnitAccession.Value
    //                                                            //    else null)
    //                                                            (if userParamItem.UnitName.IsSome 
    //                                                                then userParamItem.UnitName.Value 
    //                                                                else null)
    //                                                            userParamItem.Name
    //                                           )
    //                     ) |> List

    //let convertToEntityUserParam  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.UserParam option) =   
    //    if mzIdentMLXML.IsSome
    //       then mzIdentMLXML.Value
    //            |> (fun userParamItem -> (createUserParam (if userParamItem.Value.IsSome 
    //                                                          then userParamItem.Value.Value 
    //                                                          else null)
    //                                                      //(if userParamItem.Type.IsSome 
    //                                                      //    then userParamItem.Type.Value 
    //                                                      //    else null)
    //                                                      (if userParamItem.UnitCvRef.IsSome 
    //                                                          then takeTermID dbContext userParamItem.UnitCvRef.Value
    //                                                          else null)
    //                                                      //(if userParamItem.UnitAccession.IsSome 
    //                                                      //    then takeTermID dbContext userParamItem.UnitAccession.Value
    //                                                      //    else null)
    //                                                      (if userParamItem.UnitName.IsSome 
    //                                                          then userParamItem.UnitName.Value 
    //                                                          else null)
    //                                                      userParamItem.Name
    //                                 )
    //           )
    //       else createUserParam null null null null

    //let convertToEntityCVs (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =  
    //    mzIdentMLXML.CvList
    //        |> Array.map (fun cvItem -> (createCV  cvItem.FullName
    //                                               cvItem.Uri
    //                                               (if cvItem.Version.IsSome 
    //                                                  then cvItem.Version.Value 
    //                                                  else null
    //                                               )
                                              
    //                                    )
    //                      ) |> List

    //let convertToEntityInputSpectrumIdentifications (mzIdentMLXML : SchemePeptideShaker.InputSpectrumIdentifications []) =
    //    mzIdentMLXML
    //        |> Array.map (fun inputSpectrumIdentificationsItem -> createInputSpectrumIdentification inputSpectrumIdentificationsItem.SpectrumIdentificationListRef) |> List

    //let convertToEntityProteinDetection  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.ProteinDetection option) =
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

    //let convertToEntityAnalysisParamsCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createAnalysisParamsParam (*cvParamItem.Name*)
    //                                                                    //cvParamItem.Accession 
    //                                                                    (if cvParamItem.Value.IsSome 
    //                                                                        then cvParamItem.Value.Value 
    //                                                                        else null)
    //                                                                    (takeTermID dbContext cvParamItem.CvRef)
    //                                                                    //(if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
    //                                                                    (if cvParamItem.UnitCvRef.IsSome 
    //                                                                        then (takeTermID dbContext cvParamItem.UnitCvRef.Value) 
    //                                                                        else null)
    //                                                                    //(if cvParamItem.UnitAccession.IsSome then cvParamItem.UnitAccession.Value else null)
    //                                         )
    //                     ) |> List

    //let convertToEntityAnalysisParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.AnalysisParams option) = 
    //    (if mzIdentMLXML.IsSome 
    //        then (convertToEntityAnalysisParamsCVParams dbContext mzIdentMLXML.Value.CvParams),
    //             (convertToEntityUserParams dbContext mzIdentMLXML.Value.UserParams)
    //        else null,null
    //    )
                                                    

    //let convertToEntityThresholdOfProteinDetectionCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createThresholdParam  (*cvParamItem.Name*)
    //                                                                //cvParamItem.Accession 
    //                                                                (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                                (takeTermID dbContext cvParamItem.CvRef)
    //                                                                (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                                (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
    //                                          )
    //                     ) |> List


    //let convertToEntityThresholdOfProteinDetection  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Threshold) =  
    //    mzIdentMLXML
    //        |> (fun thresholdItem -> (convertToEntityThresholdOfProteinDetectionCVParams dbContext thresholdItem.CvParams),
    //                                 (convertToEntityUserParams dbContext thresholdItem.UserParams)
    //           )

    //let convertToEntityAdditionalSearchParamsCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =   
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createAdditionalSearchParamsParam (*cvParamItem.Name*)
    //                                                                           //cvParamItem.Accession 
    //                                                                           (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                                           (takeTermID dbContext cvParamItem.CvRef)
    //                                                                           (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                                           (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
    //                     ) |> List

    //let convertToEntityAdditionalSearchParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.AdditionalSearchParams option) =
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value 
    //            |> (fun additionalSearchParamItem -> (convertToEntityAdditionalSearchParamsCVParams dbContext additionalSearchParamItem.CvParams
    //                                                 ),
    //                                                 (convertToEntityUserParams dbContext additionalSearchParamItem.UserParams
    //                                                 )
    //               )
    //        else null, null

    //let convertToEntitySearchTypeCVParam  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam option) =
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

    //let convertToEntitySearchType  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.SearchType) =
    //    mzIdentMLXML
    //        |> (fun searchTypeItem -> (convertToEntitySearchTypeCVParam dbContext searchTypeItem.CvParam
    //                                  ),
    //                                  (convertToEntityUserParam dbContext searchTypeItem.UserParam
    //                                  )                                                                                                                                                                                                                
    //           )

    //let convertToEntitySpecificityRulesCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
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

    //let convertToEntitySpecificityRules  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.SpecificityRules []) =
    //    mzIdentMLXML
    //        |> Array.map (fun specificityRuleItem -> (convertToEntitySpecificityRulesCVParams dbContext specificityRuleItem.CvParams)
    //                     ) 
    //        |> Array.concat
    //        |> List

    //let convertToEntitySearchModificationsCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createSearchModificationParam (*cvParamItem.Name*)
    //                                                                        //cvParamItem.Accession 
    //                                                                        (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                                        (takeTermID dbContext cvParamItem.CvRef)
    //                                                                        (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                                        (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
                                                                                                                                                                                        
    //                                         )
    //                     ) |> List

    //let convertToEntitySearchModifications  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.SearchModification []) =
    //    mzIdentMLXML
    //        |> Array.map (fun searchModificationItem -> createSearchModification    searchModificationItem.FixedMod
    //                                                                                searchModificationItem.MassDelta 
    //                                                                                searchModificationItem.Residues 
    //                                                                                (convertToEntitySpecificityRules dbContext searchModificationItem.SpecificityRules
    //                                                                                )
    //                                                                                (convertToEntitySearchModificationsCVParams dbContext searchModificationItem.CvParams
    //                                                                                )
                                                                                                                                                                                
    //                     ) |> List

    //let convertToEntityModificationParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.ModificationParams option) =
    //    if mzIdentMLXML.IsSome 
    //        then mzIdentMLXML.Value
    //             |> (fun modificationParamItem -> (convertToEntitySearchModifications dbContext modificationParamItem.SearchModifications))
    //        else null


    //let convertToEntityEnzymeNameCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
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

    //let convertToEntityEnzymeName  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.EnzymeName option) =
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value 
    //            |> (fun enzymeNameItem -> (convertToEntityEnzymeNameCVParams dbContext enzymeNameItem.CvParams
    //                                      ),
    //                                      (convertToEntityUserParams dbContext enzymeNameItem.UserParams
    //                                      )
    //               )
    //        else null, null

    //let convertToEntityEnzyme  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Enzyme []) =   
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

    //let convertToEntityEnzymes  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Enzymes option) = 
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

    //let convertToEntityAmbiguousResiduesCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createAmbiguousResidueParam (*cvParamItem.Name*)
    //                                                                      //cvParamItem.Accession 
    //                                                                      (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                                      (takeTermID dbContext cvParamItem.CvRef)
    //                                                                      (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                                      (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
                                                                                                                                                                                        
    //                                         )
    //                     ) |> List

    //let convertToEntityAmbiguousResidues  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.AmbiguousResidue []) =
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

    //let massTablesCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
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

    //let convertToEntityMassTables  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.MassTable []) =
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

    //let convertToEntityFragmentToleranceCVParams (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
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

    //let convertToEntityFragmentTolerance (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.FragmentTolerance option) =
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value
    //            |> (fun fragmentToleranceItem -> (convertToEntityFragmentToleranceCVParams dbContext fragmentToleranceItem.CvParams)
    //               )
    //       else null

    //let convertToEntityExcludeCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =   
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createExcludeParam (*cvParamItem.Name*)
    //                                                             //cvParamItem.Accession 
    //                                                             (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                             (takeTermID dbContext cvParamItem.CvRef)
    //                                                             (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                             (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
    //                                         )
    //                     ) |> List

    //let convertToEntityExclude  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Exclude option) = 
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value
    //            |> (fun excludeItem -> (convertToEntityExcludeCVParams dbContext excludeItem.CvParams),
    //                                   (convertToEntityUserParams dbContext excludeItem.UserParams) 
    //               )
    //        else null, null

    //let convertToEntityIncludeCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createIncludeParam (*cvParamItem.Name*)
    //                                                             //cvParamItem.Accession 
    //                                                             (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                             (takeTermID dbContext cvParamItem.CvRef)
    //                                                             (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                             (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
    //                                         )
    //                     ) |> List

    //let convertToEntityIncludes  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Include option) = 
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value
    //            |> (fun includeItem -> (convertToEntityIncludeCVParams dbContext includeItem.CvParams),
    //                                   (convertToEntityUserParams dbContext includeItem.UserParams) 
    //               )
    //        else null, null

    //let convertToEntityFilterTypeCVParam  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam option) =
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
        
    //let convertToEntityFilterType  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.FilterType) = 
    //    mzIdentMLXML
    //    |> (fun filterTypeItem -> (convertToEntityFilterTypeCVParam dbContext filterTypeItem.CvParam
    //                              ), 
    //                              (convertToEntityUserParam dbContext filterTypeItem.UserParam
    //                              )
    //       )

    //let convertToEntityFilters  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Filter []) = 
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

    //let convertToEntityDatabaseFilters  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.DatabaseFilters option) = 
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value
    //            |> (fun databaseFilterItem -> (convertToEntityFilters dbContext databaseFilterItem.Filters)
    //                                                        |> Array.map (fun (cvFiltertype, userFiltertype, cvExclude, userExclude, cvInclude, userInclude) -> createFilter cvFiltertype userFiltertype cvExclude userExclude cvInclude userInclude)
    //                                                                     ) |> List
    //       else null

    //let convertToEntityTranslationTablesCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) = 
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createTranslationTableParam (*cvParamItem.Name*)
    //                                                                      //cvParamItem.Accession 
    //                                                                      (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                                      (takeTermID dbContext cvParamItem.CvRef)
    //                                                                      (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                                      (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
                                                                                                                                                                                       
    //                                         )
    //                     ) 

    //let convertToEntityTranslationTables  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.TranslationTable []) =  
    //    mzIdentMLXML
    //        |> Array.map (fun translationTable -> (*translationTable.Id,*)
    //                                              //(if translationTable.Name.IsSome then translationTable.Name.Value else null) 
    //                                              (convertToEntityTranslationTablesCVParams dbContext translationTable.CvParams
    //                                              ) 
    //                     ) 
    //                     |> Array.concat
    //                     |> List

    //let convertToEntityDatabaseTranslation  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.DatabaseTranslation option) =
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

    //let convertToEntityParentToleranceCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createParentToleranceParam (*cvParamItem.Name*)
    //                                                                     //cvParamItem.Accession 
    //                                                                     (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                                     (takeTermID dbContext cvParamItem.CvRef)
    //                                                                     (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                                     (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
                                                                                                                                                                                         
    //                                         )
    //                     ) |> List

    //let convertToEntityParentTolerance  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.ParentTolerance option) =
    //    if mzIdentMLXML.IsSome 
    //        then mzIdentMLXML.Value
    //             |> (fun parentToleranceItem -> (convertToEntityParentToleranceCVParams dbContext parentToleranceItem.CvParams)
    //                ) 
    //        else null

    //let convertToEntityThresholdOfSpectumIdentificationprotocolCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =  
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> (createThresholdParam (*cvParamItem.Name*)
    //                                                               //cvParamItem.Accession 
    //                                                               (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                               (takeTermID dbContext cvParamItem.CvRef)
    //                                                               (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                               (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
                                                                                                                                                                                         
    //                                         )
    //                     ) |> List

    //let convertToEntityThreshold  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Threshold) = 
    //    mzIdentMLXML
    //        |> (fun thresholdItem ->  (convertToEntityThresholdOfSpectumIdentificationprotocolCVParams dbContext thresholdItem.CvParams
    //                                  ),
    //                                  (thresholdItem.UserParams
    //                                      |> convertToEntityUserParams dbContext
    //                                  )
    //           )

    //let convertToEntitySpectrumIdentificationProtocols  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.SpectrumIdentificationProtocol []) =  
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

    //let convertToEntityProteinDetectionProtocol  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.ProteinDetectionProtocol option) = 
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
   
    //let convertToEntityAnalysisProtocolCollection  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.AnalysisProtocolCollection) =
    //     mzIdentMLXML
    //        |> (fun analysisProtocolCollectionItem -> (convertToEntitySpectrumIdentificationProtocols dbContext analysisProtocolCollectionItem.SpectrumIdentificationProtocols
    //                                                  ),
    //                                                  (convertToEntityProteinDetectionProtocol dbContext analysisProtocolCollectionItem.ProteinDetectionProtocol
    //                                                  )
    //           )

    //let convertToEntityRoleCVParam  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam) =
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

    //let convertToEntityRole  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Role) =
    //    mzIdentMLXML
    //        |> (fun rolteItem -> (convertToEntityRoleCVParam dbContext rolteItem.CvParam)
    //           )

    //let convertToEntityContactRoleOption  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.ContactRole option) =
    //    if mzIdentMLXML.IsSome 
    //       then mzIdentMLXML.Value
    //            |> (fun contactRoleItem ->  contactRoleItem.ContactRef,
    //                                        (convertToEntityRole dbContext contactRoleItem.Role)
    //               )
    //       else null, (createRoleParam null null null null)

    //let convertToEntitySoftwareNameCVParam  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam option) =
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

    //let convertToEntitySoftwareName  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.SoftwareName) =
    //    mzIdentMLXML
    //        |> (fun softwareNameItem -> (convertToEntitySoftwareNameCVParam dbContext softwareNameItem.CvParam
    //                                    ),
    //                                    (convertToEntityUserParam dbContext softwareNameItem.UserParam
    //                                    )               
    //           )

    //let convertToEntityAnalysisSoftWares  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.AnalysisSoftware []) =
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

    //let convertToEntityAnalysisSoftwareList  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =
    //    if mzIdentMLXML.AnalysisSoftwareList.IsSome 
    //       then (convertToEntityAnalysisSoftWares dbContext mzIdentMLXML.AnalysisSoftwareList.Value.AnalysisSoftwares)
    //       else null
    
    ////let convertToEntityParent (mzIdentMLXML : SchemePeptideShaker.Parent option) =
    ////    if mzIdentMLXML.IsSome 
    ////       then mzIdentMLXML.Value
    ////            |> (fun parentItem -> createParent parentItem.OrganizationRef)
    ////        else createParent null

    //let convertToEntityOrganizationCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) = 
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

    //let convertToEntityOrganizations  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Organization []) =
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

    //let convertToEntityPersonCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =  
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

    //let convertToEntityPersons  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Person []) =    
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

    //let convertToEntityAuditCollection  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =
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

    //let convertToEntityProteinDetectionHypthesisCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =   
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

    //let convertToEntityProteinDetectionHypthesis  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.ProteinDetectionHypothesis []) =
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

    //let convertToEntityProteinAmbiguityGroupsCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =  
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

    //let convertToEntityProteinAmbiguityGroups  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.ProteinAmbiguityGroup []) =
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

    //let convertToEntityProteinDetectionListCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =   
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

    //let convertToEntityProteinDetectionList  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.ProteinDetectionList option) = 
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

    //let convertToEntityMeasureCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
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

    //let convertToEntityMesasure  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Measure []) =
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

    //let convertToEntityFragmentationTable  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.FragmentationTable option) =
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

    //let convertToEntityIonTypeCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) = 
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

    //let convertToEntityIonTypes  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.IonType []) =
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

    //let convertToEntityFragmentation  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Fragmentation option) =
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

    //let convertToEntitySpectrumIdentificationItemCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =  
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

    //let convertToEntitySpectrumIdentificationItems  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.SpectrumIdentificationItem []) =  
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


    //let convertToEntitySpectrumIdentificationResultsCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
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

    //let convertToEntitySpectrumIdentificationResults  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.SpectrumIdentificationResult []) =
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
 

    //let convertToEntitySpectrumIdentificationListCVParam  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
    //    mzIdentMLXML
    //        |> Array.map (fun cvParamItem -> createSpectrumIdentificationListParam (*cvParamItem.Name*)
    //                                                                               //cvParamItem.Accession 
    //                                                                               (if cvParamItem.Value.IsSome then cvParamItem.Value.Value else null)
    //                                                                               (takeTermID dbContext cvParamItem.CvRef)
    //                                                                               (if cvParamItem.UnitCvRef.IsSome then (takeTermID dbContext cvParamItem.UnitCvRef.Value) else null)
    //                                                                               (if cvParamItem.UnitName.IsSome then cvParamItem.UnitName.Value else null)
                                          
    //                     ) |> List

    //let convertToEntitySpectrumIdentificationList  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.SpectrumIdentificationList []) =
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
 
    //let convertToEntityAnalysisData  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.AnalysisData) =    
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

    //let convertToEntitySearchDatabasesCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
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

    //let convertToEntityDatabaseNameCVParam  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam option) =  
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

    //let convertToEntityDatabaseName  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.DatabaseName) =  
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

    //let convertToEntitySourceFileCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =  
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

    //let convertToEntityFileFormatCVParam  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam) =
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

    //let convertToEntityFileFormat  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.FileFormat) =
    //    mzIdentMLXML
    //        |> (fun fileFormatItem -> (convertToEntityFileFormatCVParam dbContext fileFormatItem.CvParam)
    //           )

    //let convertToEntitySourceFiles  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.SourceFile []) = 
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

    //let convertToEntitySpectrumIDFormatCvParam  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam) =
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

    //let convertToEntitySpectrumIDFormat  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.SpectrumIdFormat) =
    //    mzIdentMLXML
    //        |> (fun spectrumIDFormatItem -> (convertToEntitySpectrumIDFormatCvParam dbContext spectrumIDFormatItem.CvParam)
    //           )

    //let convertToEntitySpectraDatas  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.SpectraData []) =
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

    //let convertToEntitySearchDatabases  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.SearchDatabase []) =   
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

    //let convertToEntityInputs  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Inputs) =
    //    mzIdentMLXML
    //        |> (fun inputItem -> createInputs (convertToEntitySearchDatabases dbContext inputItem.SearchDatabases
    //                                          )
    //                                          (convertToEntitySourceFiles dbContext inputItem.SourceFiles
    //                                          )
    //                                          (convertToEntitySpectraDatas dbContext inputItem.SpectraDatas
    //                                          )
    //           )

    //let convertToEntityDataCollection  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.DataCollection) =
    //    mzIdentMLXML
    //        |> (fun dataCollectionItem -> (convertToEntityInputs dbContext dataCollectionItem.Inputs
    //                                      ),
    //                                      (convertToEntityAnalysisData dbContext dataCollectionItem.AnalysisData
    //                                      )
                                                           
    //           )

    //let convertToEntityProvider  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =
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

    //let convertToEntityModificationCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) = 
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

    //let convertToEntityModifications  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Modification []) = 
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

    //let convertToEntityPeptideCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =   
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

    //let convertToEntityPeptides  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Peptide []) =    
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

    //let convertToEntityPeptideEvidenceCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
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

    //let convertToEntityPeptideEvidence  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.PeptideEvidence []) = 
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

    //let convertToEntityDBSequenceCVParams  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
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

    //let convertToEntityDBSequence  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.DbSequence []) =  
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

    //let convertToEntitySequenceCollection  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =    
    //    if mzIdentMLXML.SequenceCollection.IsSome 
    //       then mzIdentMLXML.SequenceCollection.Value
    //            |> (fun sequenceCollectionItem -> (convertToEntityPeptides dbContext sequenceCollectionItem.Peptides), 
    //                                              (convertToEntityPeptideEvidence dbContext sequenceCollectionItem.PeptideEvidences),
    //                                              (convertToEntityDBSequence dbContext sequenceCollectionItem.DbSequences)
    //               )
    //        else null, null, null 

    //let convertToEntityContactRoles  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.ContactRole []) =
    //    mzIdentMLXML
    //        |> Array.map (fun contactRoleItem -> createContactRole contactRoleItem.ContactRef (convertToEntityRole dbContext contactRoleItem.Role)
    //                     ) |> List

    //let convertToEntitySubSamples  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.SubSample []) =
    //    mzIdentMLXML
    //        |> Array.map (fun subSampleItem -> createSubSample subSampleItem.SampleRef
    //                     ) |> List

    //let convertToEntitySampleCVParam  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.CvParam []) =
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

    //let convertToEntitySamples  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.Sample []) =
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

    //let convertToEntityAnalysisSampleCollection  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =
    //    if mzIdentMLXML.AnalysisSampleCollection.IsSome 
    //       then mzIdentMLXML.AnalysisSampleCollection.Value
    //            |> (fun analysisSampleCollectionItem -> (convertToEntitySamples dbContext analysisSampleCollectionItem.Samples)
    //               )
    //       else null

    //let convertToEntityAnalyisCollection  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =
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


    //let convertToEntityMzIdentML  (dbContext : MzIdentMLContext) (mzIdentMLXML : SchemePeptideShaker.MzIdentMl) =
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

