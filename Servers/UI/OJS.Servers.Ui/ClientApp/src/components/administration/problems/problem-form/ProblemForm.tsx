import React, { useEffect, useMemo, useState } from 'react';
import { Autocomplete, Box, FormControl, MenuItem, TextField, Typography } from '@mui/material';
import downloadFile from 'src/utils/file-download-utils';

import { ContestVariation } from '../../../../common/contest-types';
import { ADDITIONAL_FILES, SUBMISSION_TYPES, TESTS } from '../../../../common/labels';
import { IProblemAdministration, IProblemGroupDropdownModel, IProblemSubmissionType, ISubmissionTypeInProblem } from '../../../../common/types';
import useDelayedSuccessEffect from '../../../../hooks/common/use-delayed-success-effect';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import {
    useGetIdsByContestIdQuery,
} from '../../../../redux/services/admin/problemGroupsAdminService';
import {
    useCreateProblemMutation,
    useDownloadAdditionalFilesQuery,
    useGetProblemByIdQuery,
    useUpdateProblemMutation,
} from '../../../../redux/services/admin/problemsAdminService';
import { useGetForProblemQuery } from '../../../../redux/services/admin/submissionTypesAdminService';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import clearSuccessMessages from '../../../../utils/success-messages-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import AdministrationFormButtons from '../../common/administration-form-buttons/AdministrationFormButtons';
import FileUpload from '../../common/file-upload/FileUpload';
import ProblemFormBasicInfo from '../problem-form-basic-info.tsx/ProblemFormBasicInfo';
import ProblemSubmissionTypes from '../problem-submission-types/ProblemSubmissionTypes';

import formStyles from '../../common/styles/FormStyles.module.scss';
import styles from '../../contests/contest-edit/ContestEdit.module.scss';

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
        sourceCodeSizeLimit: defaultSourceCodeSizeLimit,
        submissionTypes: [],
        timeLimit: defaultTimeLimit,
        tests: null,
        contestType: contestType || ContestVariation.Exercise,
        problemGroupOrderBy: -1,
        problemGroupId: 0,
        defaultSubmissionTypeId: null,
        additionalFiles: null,
        hasAdditionalFiles: false,
    });

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ skipDownload, setSkipDownload ] = useState<boolean>(true);

    const {
        data: problemData,
        isLoading: isGettingData,
        error: gettingDataError,
    } = useGetProblemByIdQuery({ id: Number(problemId) }, { skip: problemId === null });

    const { data: submissionTypes } = useGetForProblemQuery(null);

    const [
        updateProblem,
        {
            data: updateData,
            error: updateError,
            isSuccess: isSuccessfullyUpdated,
            isLoading: isUpdating,
        },
    ] = useUpdateProblemMutation();

    const [
        createProblem,
        {
            data: createData,
            error: createError,
            isSuccess: isSuccessfullyCreated,
            isLoading: isCreating,
        },
    ] = useCreateProblemMutation();

    const {
        data: downloadData,
        error: downloadError,
    } = useDownloadAdditionalFilesQuery(Number(problemId), { skip: skipDownload });

    const { data: problemGroupData } = useGetIdsByContestIdQuery(currentProblem.contestId, { skip: currentProblem.contestId <= 0 });

    const isDefaultStrategySelected = useMemo(() => currentProblem
        .submissionTypes
        .some((st) => currentProblem.defaultSubmissionTypeId === st.id), [
        currentProblem.defaultSubmissionTypeId,
        currentProblem.submissionTypes,
    ]);

    useDelayedSuccessEffect({ isSuccess: isSuccessfullyCreated || isSuccessfullyUpdated, onSuccess });

    useSuccessMessageEffect({
        data: [
            { message: createData, shouldGet: isSuccessfullyCreated },
            { message: updateData, shouldGet: isSuccessfullyUpdated },
        ],
        setParentSuccessMessage,
        setSuccessMessage,
        clearFlags: [ isCreating, isUpdating ],
    });

    useEffect(() => {
        if (downloadData?.blob) {
            downloadFile(downloadData.blob, downloadData.filename);
        }
    }, [ downloadData ]);

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
        getAndSetExceptionMessage([ gettingDataError, createError, updateError, downloadError ], setErrorMessages);
        clearSuccessMessages({ setSuccessMessage, setParentSuccessMessage });
    }, [ updateError, createError, gettingDataError, downloadError, setParentSuccessMessage ]);

    const onChange = (e: any) => {
        const { target } = e;
        const { name, type, value, checked } = target;

        setCurrentProblem((prevState) => ({
            ...prevState,
            [name]: type === 'checkbox'
                ? checked
                : type === 'number'
                    ? value
                        ? Number(value)
                        : null
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
        formData.append('problemGroupId', currentProblem.problemGroupId?.toString() || '');
        formData.append('defaultSubmissionTypeId', currentProblem.defaultSubmissionTypeId?.toString() || '');
        currentProblem.submissionTypes?.forEach((type, index) => {
            formData.append(`SubmissionTypes[${index}].Id`, type.id.toString());
            formData.append(`SubmissionTypes[${index}].Name`, type.name.toString());

            if (type.id === Number(formData.get('defaultSubmissionTypeId'))) {
                formData.append('DefaultSubmissionType.Id', type.id.toString());
                formData.append('DefaultSubmissionType.Name', type.name.toString());
            }

            if (type.solutionSkeleton) {
                formData.append(
                    `SubmissionTypes[${index}].SolutionSkeleton`,
                    type?.solutionSkeleton.toString(),
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

        if (currentProblem.additionalFiles) {
            formData.append('additionalFiles', currentProblem.additionalFiles);
        }
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
            const problemSubmissionTypes = [ ...currentProblem.submissionTypes ];
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
                } else if (propName === 'defaultSubmissionTypeId') {
                    onChange({
                        target: {
                            value: value
                                ? item.id
                                : null,
                            name: propName,
                            type: 'number',
                            checked: false,
                        },
                    });
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

    const handleFileUpload = (e: any, propName: keyof IProblemAdministration) => {
        const file = e.target.files[0];
        setCurrentProblem((prevState) => ({
            ...prevState,
            [propName]: file,
        }));
        setSkipDownload(true);
    };

    const handleFileClearance = (propName: keyof IProblemAdministration) => {
        setCurrentProblem((prevState) => ({
            ...prevState,
            [propName]: null,
        }));
    };

    if (isGettingData) {
        return <SpinningLoader />;
    }

    return (
        <Box className={`${styles.flex}`}>
            {renderErrorMessagesAlert(errorMessages)}
            {renderSuccessfullAlert(successMessage)}
            {currentProblem?.name && <Typography className={formStyles.centralize} variant="h4">{currentProblem?.name}</Typography>}
            <form className={formStyles.form}>
                <ProblemFormBasicInfo currentProblem={currentProblem} onChange={onChange} problemGroups={problemGroupIds} />
                {!isEditMode &&
                    <Box className={formStyles.fieldBox}>
                        <Typography className={formStyles.fieldBoxTitle} variant="h5">
                            Tests
                        </Typography>
                        <div className={formStyles.fieldBoxDivider} />
                        <Box className={formStyles.fieldBoxElement}>
                            <Box className={formStyles.row}>
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
                            </Box>
                        </Box>
                    </Box>
                }
                <Box className={formStyles.fieldBox}>
                    <Typography className={formStyles.fieldBoxTitle} variant="h5">
                        {SUBMISSION_TYPES}
                    </Typography>
                    <div className={formStyles.fieldBoxDivider} />
                    <Box className={formStyles.fieldBoxElement}>
                        <Box className={formStyles.row}>
                            <FormControl className={formStyles.row}>
                                <Autocomplete
                                  className={formStyles.inputRow}
                                  options={filteredSubmissionTypes}
                                  renderInput={(params) => <TextField {...params} label="Select submission type" key={params.id} />}
                                  onChange={(event, newValue) => onStrategyAdd(newValue!)}
                                  value={null}
                                  isOptionEqualToValue={(option, value) => option.id === value.id}
                                  getOptionLabel={(option) => option?.name}
                                  renderOption={(properties, option) =>
                                      <MenuItem {...properties} key={option.id} value={option.id}>
                                          {option.name}
                                      </MenuItem>
                                  }
                                />
                            </FormControl>
                        </Box>
                    </Box>
                </Box>
                {
            currentProblem?.submissionTypes.map((st : IProblemSubmissionType) =>
                <ProblemSubmissionTypes
                  key={st.id}
                  onPropChange={onPropChangeInSubmissionType}
                  onStrategyRemoved={onStrategyRemoved}
                  strategy={st}
                  isDefaultStrategySelected={isDefaultStrategySelected}
                  defaultSubmissionTypeId={currentProblem.defaultSubmissionTypeId ?? 0}
                />)
        }
                <Box className={formStyles.fieldBox}>
                    <Typography className={formStyles.fieldBoxTitle} variant="h5">
                        {ADDITIONAL_FILES}
                    </Typography>
                    <div className={formStyles.fieldBoxDivider} />
                    <Box className={formStyles.fieldBoxElement}>
                        <Box className={formStyles.row}>
                            <FormControl className={formStyles.inputRow}>
                                <FileUpload
                                  handleFileUpload={handleFileUpload}
                                  propName="additionalFiles"
                                  setSkipDownload={setSkipDownload}
                                  uploadButtonName={currentProblem.additionalFiles?.name}
                                  showDownloadButton={currentProblem.hasAdditionalFiles}
                                  onClearSelectionClicked={handleFileClearance}
                                  disableClearButton={currentProblem.additionalFiles === null}
                                />
                            </FormControl>
                        </Box>
                    </Box>
                </Box>
                <AdministrationFormButtons
                  isEditMode={isEditMode}
                  onCreateClick={() => submitForm()}
                  onEditClick={() => submitForm()}
                />
            </form>
        </Box>
    );
};

export default ProblemForm;
