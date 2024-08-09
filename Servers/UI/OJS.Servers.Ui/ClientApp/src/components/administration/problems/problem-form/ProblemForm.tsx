/* eslint-disable @typescript-eslint/ban-types */
import React, { useEffect, useState } from 'react';
import { Autocomplete, Divider, FormControl, FormGroup, MenuItem, TextField, Typography } from '@mui/material';

import { ContestVariation } from '../../../../common/contest-types';
import { SUBMISSION_TYPES, TESTS } from '../../../../common/labels';
import { IProblemAdministration, IProblemGroupDropdownModel, IProblemSubmissionType, ISubmissionTypeInProblem } from '../../../../common/types';
import useDelayedSuccessEffect from '../../../../hooks/common/use-delayed-success-effect';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { useGetIdsByContestIdQuery } from '../../../../redux/services/admin/problemGroupsAdminService';
import { useCreateProblemMutation, useGetProblemByIdQuery, useUpdateProblemMutation } from '../../../../redux/services/admin/problemsAdminService';
import { useGetForProblemQuery } from '../../../../redux/services/admin/submissionTypesAdminService';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import AdministrationFormButtons from '../../common/administration-form-buttons/AdministrationFormButtons';
import FileUpload from '../../common/file-upload/FileUpload';
import ProblemFormBasicInfo from '../problem-form-basic-info.tsx/ProblemFormBasicInfo';
import ProblemSubmissionTypes from '../problem-submission-types/ProblemSubmissionTypes';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';

interface IProblemFormProps {
    isEditMode?: boolean;
    getName?: Function;
    getContestId?: Function;
}

interface IProblemFormCreateProps extends IProblemFormProps{
    contestId: number;
    contestName: string | undefined;
    contestType: ContestVariation;
    problemId: null;
    onSuccess: Function;
    setParentSuccessMessage: Function;
}

interface IProblemFormEditProps extends IProblemFormProps{
    contestId?: null;
    contestName?: null;
    contestType?: null;
    problemId: number;
    onSuccess?: Function;
    setParentSuccessMessage?: Function;
}

const defaultMaxPoints = 100;
const defaultMemoryLimit = 16777216;
const defaultTimeLimit = 100;
const defaultSourceCodeSizeLimit = 16384;

const ProblemForm = (props: IProblemFormCreateProps | IProblemFormEditProps) => {
    const {
        problemId,
        isEditMode = true,
        contestId,
        contestName,
        contestType,
        getName,
        getContestId,
        onSuccess,
        setParentSuccessMessage,
    } = props;

    const [ filteredSubmissionTypes, setFilteredSubmissionTypes ] = useState<Array<ISubmissionTypeInProblem>>([]);
    const [ problemGroupIds, setProblemGroupsIds ] = useState<Array<IProblemGroupDropdownModel>>([]);
    const [ currentProblem, setCurrentProblem ] = useState<IProblemAdministration>({
        checkerId: '2',
        contestId: contestId ?? -1,
        contestName: contestName ?? '',
        id: 0,
        maximumPoints: defaultMaxPoints,
        memoryLimit: defaultMemoryLimit,
        name: '',
        orderBy: 0,
        problemGroupType: 'None',
        showDetailedFeedback: false,
        showResults: true,
        sourceCodeSizeLimit: defaultSourceCodeSizeLimit,
        submissionTypes: [],
        timeLimit: defaultTimeLimit,
        tests: null,
        contestType: contestType || ContestVariation.Exercise,
        problemGroupOrderBy: -1,
        problemGroupId: 0,
    });

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);

    const {
        data: problemData,
        isLoading: isGettingData,
        error: gettingDataError,
    } = useGetProblemByIdQuery({ id: Number(problemId) }, { skip: problemId === null });

    const { data: submissionTypes } = useGetForProblemQuery(null);

    const [ updateProblem, { data: updateData, error: updateError, isSuccess: isSuccessfullyUpdated } ] = useUpdateProblemMutation();
    const [ createProblem, { data: createData, error: createError, isSuccess: isSuccessfullyCreated } ] = useCreateProblemMutation();

    const { data: problemGroupData } = useGetIdsByContestIdQuery(currentProblem.contestId, { skip: currentProblem.contestId <= 0 });

    useDelayedSuccessEffect({ isSuccess: isSuccessfullyCreated, onSuccess });

    useSuccessMessageEffect({
        data: [
            { message: createData, shouldGet: isSuccessfullyCreated },
            { message: updateData, shouldGet: isSuccessfullyUpdated },
        ],
        setParentSuccessMessage,
        setSuccessMessage,
    });

    useEffect(() => {
        if (problemGroupData) {
            setProblemGroupsIds(problemGroupData);
        }
    }, [ problemGroupData ]);

    useEffect(() => {
        if (submissionTypes) {
            setFilteredSubmissionTypes(submissionTypes.filter((st) => !problemData?.submissionTypes.some((x) => x.id === st.id)));
        }
    }, [ problemData?.submissionTypes, submissionTypes ]);

    useEffect(() => {
        if (problemData) {
            setCurrentProblem(problemData);
            if (getName) {
                getName(problemData.name);
            }
            if (getContestId) {
                getContestId(problemData.contestId);
            }
        }
    }, [ getContestId, getName, problemData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ gettingDataError, createError, updateError ], setErrorMessages);
        setSuccessMessage(null);
    }, [ updateError, createError, gettingDataError ]);

    const onChange = (e: any) => {
        const { target } = e;
        const { name, type, value, checked } = target;
        setCurrentProblem((prevState) => ({
            ...prevState,
            [name]: type === 'checkbox'
                ? checked
                : type === 'number'
                    ? value === ''
                        ? ''
                        : Number(value)
                    : value,
        }));
    };

    const submitForm = () => {
        const formData = new FormData();
        formData.append('name', currentProblem.name);
        formData.append('id', currentProblem.id?.toString() ?? '');
        formData.append('orderBy', currentProblem.orderBy?.toString() || '');
        formData.append('contestId', currentProblem.contestId?.toString() || '');
        formData.append('contestName', currentProblem.contestName?.toString() || '');
        formData.append('maximumPoints', currentProblem.maximumPoints?.toString() || '');
        formData.append('memoryLimit', currentProblem.memoryLimit?.toString() || '');
        formData.append('sourceCodeSizeLimit', currentProblem.sourceCodeSizeLimit?.toString() || '');
        formData.append('timeLimit', currentProblem.timeLimit?.toString() || '');
        formData.append('problemGroupType', currentProblem.problemGroupType?.toString());
        formData.append('checkerId', currentProblem.checkerId?.toString() || '');
        formData.append('showDetailedFeedback', currentProblem.showDetailedFeedback?.toString() || '');
        formData.append('showResults', currentProblem.showResults?.toString() || '');
        formData.append('problemGroupId', currentProblem.problemGroupId?.toString() || '');
        currentProblem.submissionTypes?.forEach((type, index) => {
            formData.append(`SubmissionTypes[${index}].Id`, type.id.toString());
            formData.append(`SubmissionTypes[${index}].Name`, type.name.toString());

            if (type.solutionSkeleton) {
                formData.append(
                    `SubmissionTypes[${index}].SolutionSkeleton`,
                    type?.solutionSkeleton!.toString(),
                );
            }
            if (type.timeLimit) {
                formData.append(
                    `SubmissionTypes[${index}].TimeLimit`,
                    type?.timeLimit.toString(),
                );
            }
            if (type.memoryLimit) {
                formData.append(
                    `SubmissionTypes[${index}].MemoryLimit`,
                    type?.memoryLimit.toString(),
                );
            }
        });
        if (currentProblem.tests) {
            formData.append('tests', currentProblem.tests);
        }

        if (isEditMode) {
            updateProblem(formData);
        } else {
            createProblem(formData);
        }
    };

    const onStrategyAdd = (submissionType: ISubmissionTypeInProblem) => {
        if (submissionType === null) {
            return;
        }
        const hasSubmissionType = filteredSubmissionTypes.some((st) => st.id === submissionType.id);

        if (hasSubmissionType) {
            const removedSubmissionType = filteredSubmissionTypes.find((st) => st.id === submissionType.id);

            let newSubmissionTypes = filteredSubmissionTypes;
            const problemSubmissionTypes = [ ...currentProblem!.submissionTypes ];
            if (removedSubmissionType) {
                problemSubmissionTypes.push({
                    id: submissionType.id,
                    name: removedSubmissionType.name,
                    solutionSkeleton: null,
                    memoryLimit: null,
                    timeLimit: null,
                });

                newSubmissionTypes = newSubmissionTypes.filter((x) => x.id !== submissionType.id);

                setFilteredSubmissionTypes(newSubmissionTypes);
                setCurrentProblem((prevState) => ({
                    ...prevState,
                    submissionTypes: problemSubmissionTypes,
                }));
            }
        }
    };

    const onStrategyRemoved = (id: number) => {
        const hasSubmissionType = currentProblem?.submissionTypes.some((st) => st.id === id);

        if (hasSubmissionType) {
            const removedSubmissionType = currentProblem?.submissionTypes.find((st) => st.id === id);

            const newSubmissionTypes = filteredSubmissionTypes;
            let problemSubmissionTypes = currentProblem?.submissionTypes;
            if (removedSubmissionType) {
                newSubmissionTypes.push({
                    id,
                    name: removedSubmissionType.name,
                });

                problemSubmissionTypes = problemSubmissionTypes.filter((x) => x.id !== id);

                setFilteredSubmissionTypes(newSubmissionTypes);
                setCurrentProblem((prevState) => ({
                    ...prevState,
                    submissionTypes: problemSubmissionTypes,
                }));
            }
        }
    };

    const onPropChangeInSubmissionType = (value: string | number | null, submissionTypeId: number, propName: string) => {
        const index = currentProblem.submissionTypes.findIndex((st) => st.id === submissionTypeId);

        const newSubmissionTypes = currentProblem.submissionTypes.map((item, idx) => {
            if (idx === index) {
                // Check the type of the property to determine how to parse the value
                let updatedValue = value;
                if (propName === 'timeLimit' || propName === 'memoryLimit') {
                    let number = null;

                    if (Number(value) > 0) {
                        number = Number(value);
                    }
                    // Assuming you want to convert these to numbers
                    updatedValue = number;
                }

                return { ...item, [propName]: updatedValue };
            }
            return item;
        });

        setCurrentProblem((prevState) => ({
            ...prevState,
            submissionTypes: newSubmissionTypes,
        }));
    };

    const handleFileUpload = (e: any) => {
        let { tests } = currentProblem;
        tests = e.target.files[0];
        setCurrentProblem((prevState) => ({
            ...prevState,
            tests,
        }));
    };

    const handleFileClearance = () => {
        let { tests } = currentProblem;
        tests = null;
        setCurrentProblem((prevState) => ({
            ...prevState,
            tests,
        }));
    };

    if (isGettingData) {
        return <SpinningLoader />;
    }
    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            {renderSuccessfullAlert(successMessage)}

            <Typography className={formStyles.centralize} variant="h3">{currentProblem?.name}</Typography>
            <form className={formStyles.form}>
                <ProblemFormBasicInfo currentProblem={currentProblem} onChange={onChange} problemGroups={problemGroupIds} />
                <FormGroup className={formStyles.row}>
                    {!isEditMode && (
                        <>
                            <Typography className={formStyles.spacing} variant="h4">{TESTS}</Typography>
                            <Divider className={formStyles.inputRow} />
                            <FileUpload
                              handleFileUpload={handleFileUpload}
                              propName="tests"
                              setSkipDownload={() => {}}
                              uploadButtonName={currentProblem.tests?.name}
                              showDownloadButton={false}
                              disableClearButton={!currentProblem.tests}
                              onClearSelectionClicked={handleFileClearance}
                              buttonLabel={TESTS}
                            />
                        </>
                    )}
                </FormGroup>
                <FormGroup className={formStyles.inputRow}>
                    <Typography className={formStyles.dividerLabel} variant="h4">{SUBMISSION_TYPES}</Typography>
                    <Divider className={formStyles.inputRow} />
                    <FormControl className={formStyles.row}>
                        <Autocomplete
                          options={filteredSubmissionTypes!}
                          renderInput={(params) => <TextField {...params} label="Select submission type" key={params.id} />}
                          onChange={(event, newValue) => onStrategyAdd(newValue!)}
                          value={null}
                          isOptionEqualToValue={(option, value) => option.id === value.id}
                          getOptionLabel={(option) => option?.name}
                          renderOption={(properties, option) => (
                              <MenuItem {...properties} key={option.id} value={option.id}>
                                  {option.name}
                              </MenuItem>
                          )}
                        />
                    </FormControl>
                </FormGroup>
                {
            currentProblem?.submissionTypes.map((st : IProblemSubmissionType) => (
                <ProblemSubmissionTypes
                  key={st.id}
                  onPropChange={onPropChangeInSubmissionType}
                  onStrategyRemoved={onStrategyRemoved}
                  strategy={st}
                />
            ))
        }
                <AdministrationFormButtons
                  isEditMode={isEditMode}
                  onCreateClick={() => submitForm()}
                  onEditClick={() => submitForm()}
                />

            </form>
        </>
    );
};

export default ProblemForm;
