/* eslint-disable @typescript-eslint/ban-types */
import React, { useCallback, useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import {
    Autocomplete,
    Box,
    Checkbox,
    FormControl,
    FormControlLabel,
    FormLabel,
    InputLabel,
    MenuItem,
    Select,
    TextareaAutosize,
    TextField,
    Tooltip,
    Typography,
} from '@mui/material';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import isNaN from 'lodash/isNaN';

import { ContestVariation } from '../../../../common/contest-types';
import {
    ALLOW_PARALLEL_SUBMISSIONS_IN_TASKS,
    ALLOWED_IPS,
    COMPETE_END_TIME,
    COMPETE_PASSWORD,
    COMPETE_START_TIME,
    DESCRIPTION,
    DURATION,
    IS_VISIBLE,
    LIMIT_BETWEEN_SUBMISSIONS,
    NAME,
    NEW_IP_PASSWORD,
    NUMBER_OF_PROBLEM_GROUPS,
    ORDER_BY,
    PRACTICE_END_TIME,
    PRACTICE_PASSWORD,
    PRACTICE_START_TIME,
    SELECT_CATEGORY,
    TYPE,
    VISIBLE_FROM,
} from '../../../../common/labels';
import { CONTEST_DESCRIPTION_PLACEHOLDER_MESSAGE, CONTEST_DURATION_VALIDATION, CONTEST_LIMIT_BETWEEN_SUBMISSIONS_VALIDATION, CONTEST_NAME_VALIDATION, CONTEST_NEW_IP_PASSWORD_VALIDATION, CONTEST_NUMBER_OF_PROBLEM_GROUPS, CONTEST_ORDER_BY_VALIDATION, CONTEST_TYPE_VALIDATION, DELETE_CONFIRMATION_MESSAGE } from '../../../../common/messages';
import { IContestAdministration } from '../../../../common/types';
import { CONTESTS_PATH, NEW_ADMINISTRATION_PATH } from '../../../../common/urls/administration-urls';
import { getContestsDetailsPageUrl } from '../../../../common/urls/compose-client-urls';
import useDelayedSuccessEffect from '../../../../hooks/common/use-delayed-success-effect';
import useDisableMouseWheelOnNumberInputs from '../../../../hooks/common/use-disable-mouse-wheel-on-number-inputs';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { useGetCategoriesQuery } from '../../../../redux/services/admin/contestCategoriesAdminService';
import {
    useCreateContestMutation,
    useDeleteContestMutation,
    useGetContestByIdQuery,
    useTransferParticipantsMutation,
    useUpdateContestMutation,
} from '../../../../redux/services/admin/contestsAdminService';
import { convertToUtc, getDateAsLocal } from '../../../../utils/administration/administration-dates';
import concatClassNames from '../../../../utils/class-names';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert } from '../../../../utils/render-utils';
import { getEnumMemberName } from '../../../../utils/string-utils';
import clearSuccessMessages from '../../../../utils/success-messages-utils';
import ExternalLink from '../../../guidelines/buttons/ExternalLink';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import AdministrationFormButtons from '../../common/administration-form-buttons/AdministrationFormButtons';
import DeleteButton from '../../common/delete/DeleteButton';
import TransferParticipantsButton from '../../common/transfer-participants/TransferParticipantsButton';
import {
    autocompleteNameIdFormatFilterOptions,
    handleAutocompleteChange,
    handleDateTimePickerChange,
} from '../../utils/mui-utils';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';
import styles from './ContestEdit.module.scss';

interface IContestEditProps {
    contestId: number | null;
    isEditMode?: boolean;
    currentContest?: IContestAdministration;
    onSuccess?: Function;
    setParentSuccessMessage: Function;
    onDeleteSuccess? : Function;
    skipGettingContest?: boolean;
}

const NAME_PROP = 'name';
const ContestEdit = (props:IContestEditProps) => {
    const {
        contestId,
        isEditMode = true,
        currentContest,
        onSuccess,
        setParentSuccessMessage,
        onDeleteSuccess,
        skipGettingContest = false,
    } = props;

    const navigate = useNavigate();
    const location = useLocation();
    const queryParams = new URLSearchParams(location.search);

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ isValidForm, setIsValidForm ] = useState<boolean>(isEditMode);

    const [ contest, setContest ] = useState<IContestAdministration>({
        allowedIps: '',
        allowParallelSubmissionsInTasks: false,
        categoryId: 0,
        categoryName: '',
        contestPassword: null,
        description: null,
        endTime: null,
        name: '',
        id: 0,
        isVisible: false,
        visibleFrom: null,
        limitBetweenSubmissions: 0,
        autoChangeLimitBetweenSubmissions: false,
        newIpPassword: null,
        orderBy: 0,
        practiceEndTime: null,
        practicePassword: null,
        practiceStartTime: null,
        startTime: null,
        type: getEnumMemberName(ContestVariation, ContestVariation.Exercise).toString(),
        numberOfProblemGroups: 0,
        duration: undefined,
        canBeCompeted: false,
        officialParticipants: 0,
    });

    const [ contestValidations, setContestValidations ] = useState({
        isNameTouched: false,
        isNameValid: isEditMode,
        isTypeTouched: false,
        isTypeValid: true,
        isLimitBetweenSubmissionsTouched: false,
        isLimitBetweenSubmissionsValid: true,
        isOrderByTouched: false,
        isOrderByValid: true,
        isNewIpPasswordTouched: false,
        isNewIpPasswordValid: true,
        isDurationTouched: false,
        isDurationValid: true,
        isNumberOfProblemGroupsTouched: false,
        isNUmberOfProblemGroupsValid: true,
    });

    const { data, isLoading } = useGetContestByIdQuery(
        { id: Number(contestId) },
        { skip: skipGettingContest },
    );

    const { isFetching: isGettingCategories, data: contestCategories } = useGetCategoriesQuery(null);

    const [
        updateContest, {
            data: updateData,
            isLoading: isUpdating,
            error: updateError,
            isSuccess: isSuccessfullyUpdated,
        } ] = useUpdateContestMutation();

    const [
        createContest, {
            data: createData,
            isSuccess: isSuccessfullyCreated,
            error: createError,
            isLoading: isCreating,
        } ] = useCreateContestMutation();

    const isVariation =
        (value: string, variation: ContestVariation) => value === getEnumMemberName(ContestVariation, variation).toString();

    const isOnlineExam = isVariation(contest.type, ContestVariation.OnlinePracticalExam);

    const isWithRandomTasks = isOnlineExam || isVariation(contest.type, ContestVariation.OnsitePracticalExamWithRandomTasks);

    const getDefaultContestCategory = useCallback(() => {
        const defaultCategory = contestCategories![0];
        const categoryName = defaultCategory.name;
        const categoryId = defaultCategory.id;
        /*
            Set the contest's 'categoryId' and 'categoryName' to the default contest category's
            values, because otherwise they will remain 0 and an empty string ('') respectively.
            If a user tries to create a new contest without selecting a category, an error
            would occur since 0 and '' are not valid values for categoryId and categoryName.
            This ensures a valid default category is always set when creating a new contest.
        */
        setContest((prevState) => ({
            ...prevState,
            categoryId,
            categoryName,
        }));

        return defaultCategory;
    }, [ contestCategories ]);

    const openTransferDialog = (setOpenModal: React.Dispatch<React.SetStateAction<boolean>>) => {
        if (queryParams.get('openTransfer') === 'true') {
            setOpenModal(true);
            queryParams.delete('openTransfer');
            navigate(`${location.pathname}?${queryParams.toString()}`, { replace: true });
        }
    };

    useDisableMouseWheelOnNumberInputs();

    useSuccessMessageEffect({
        data: [
            { message: createData, shouldGet: isSuccessfullyCreated },
            { message: updateData, shouldGet: isSuccessfullyUpdated },
        ],
        setParentSuccessMessage,
        clearFlags: [ isCreating, isUpdating ],
    });

    useDelayedSuccessEffect({ isSuccess: isSuccessfullyCreated || isSuccessfullyUpdated, onSuccess });

    useEffect(
        () => {
            if (isEditMode && currentContest) {
                setContest(currentContest!);
            }
        },
        // eslint-disable-next-line react-hooks/exhaustive-deps
        [ ],
    );

    useEffect(() => {
        if (data) {
            setContest(data);
        }
    }, [ data ]);

    useEffect(() => {
        getAndSetExceptionMessage([ createError, updateError ], setErrorMessages);
        clearSuccessMessages({ setParentSuccessMessage });
    }, [ updateError, createError, setParentSuccessMessage ]);

    const validateForm = () => {
        const isValid = contestValidations.isNameValid &&
        contestValidations.isTypeValid &&
        contestValidations.isLimitBetweenSubmissionsValid &&
        contestValidations.isOrderByValid &&
        contestValidations.isNewIpPasswordValid &&
        contestValidations.isDurationValid &&
        contestValidations.isNUmberOfProblemGroupsValid;
        setIsValidForm(isValid);
    };

    const onChange = (e: any) => {
        const { name, value, checked } = e.target;
        let {
            name: contestName,
            type: contestType,
            limitBetweenSubmissions,
            autoChangeLimitBetweenSubmissions,
            orderBy,
            contestPassword,
            practicePassword,
            allowedIps,
            newIpPassword,
            description,
            startTime,
            endTime,
            practiceStartTime,
            practiceEndTime,
            isVisible,
            visibleFrom,
            allowParallelSubmissionsInTasks,
            categoryId,
            categoryName,
            numberOfProblemGroups,
            duration,
        } = contest;
        const currentContestValidations = contestValidations;
        // eslint-disable-next-line default-case
        switch (name) {
        case NAME_PROP: {
            contestName = value;
            currentContestValidations.isNameTouched = true;
            currentContestValidations.isNameValid = true;
            if (value.length < 4 || value.length > 100) {
                currentContestValidations.isNameValid = false;
            }
            break;
        }
        case 'type': {
            contestType = value;
            currentContestValidations.isTypeTouched = true;
            const isValid = !!Object.keys(ContestVariation).filter((key) => isNaN(Number(key))).some((x) => x === value);
            currentContestValidations.isTypeValid = isValid;

            const isOnlineExamValue = isVariation(value, ContestVariation.OnlinePracticalExam);
            const isWithRandomTasksValue = isVariation(value, ContestVariation.OnsitePracticalExamWithRandomTasks);
            if (isOnlineExamValue || isWithRandomTasksValue) {
                numberOfProblemGroups = 2;
            }
            break;
        }
        case 'limitBetweenSubmissions': {
            currentContestValidations.isLimitBetweenSubmissionsTouched = true;
            currentContestValidations.isLimitBetweenSubmissionsValid = true;
            limitBetweenSubmissions = value;
            if (value < 0) {
                currentContestValidations.isLimitBetweenSubmissionsValid = false;
            }
            break;
        }
        case 'orderBy': {
            currentContestValidations.isOrderByTouched = true;
            currentContestValidations.isOrderByValid = true;
            orderBy = value;
            if (value < 0) {
                currentContestValidations.isOrderByValid = false;
            }
            break;
        }
        case 'contestPassword': {
            contestPassword = value;
            if (!value) {
                contestPassword = null;
            }
            break;
        }
        case 'practicePassword': {
            practicePassword = value;
            if (!value) {
                practicePassword = null;
            }
            break;
        }
        case 'allowedIps': {
            allowedIps = value;
            break;
        }
        case 'newIpPassword': {
            currentContestValidations.isNewIpPasswordTouched = true;
            currentContestValidations.isNewIpPasswordValid = true;
            newIpPassword = value;
            if (!value) {
                newIpPassword = null;
            }
            if (newIpPassword!.length > 20) {
                currentContestValidations.isNewIpPasswordValid = false;
            }
            break;
        }
        case 'description': {
            description = value;
            if (!value) {
                description = null;
            }
            break;
        }
        case 'startTime': {
            startTime = null;
            if (value) {
                startTime = convertToUtc(e.target.value);
            }
            break;
        }
        case 'endTime': {
            endTime = null;
            if (value) {
                endTime = convertToUtc(e.target.value);
            }
            break;
        }
        case 'practiceStartTime': {
            practiceStartTime = null;
            if (value) {
                practiceStartTime = convertToUtc(e.target.value);
            }
            break;
        }
        case 'practiceEndTime': {
            practiceEndTime = null;
            if (value) {
                practiceEndTime = convertToUtc(e.target.value);
            }
            break;
        }
        case 'isVisible': {
            isVisible = checked;
            break;
        }
        case 'autoChangeLimitBetweenSubmissions': {
            autoChangeLimitBetweenSubmissions = checked;
            break;
        }
        case 'visibleFrom': {
            visibleFrom = null;
            if (value) {
                visibleFrom = convertToUtc(e.target.value);
            }
            break;
        }
        case 'allowParallelSubmissionsInTasks': {
            allowParallelSubmissionsInTasks = checked;
            break;
        }
        case 'category': {
            const category = contestCategories?.find((cc) => cc.id === value);
            if (category) {
                categoryId = category.id;
                categoryName = category.name;
            }
            break;
        }
        case 'numberOfProblemGroups':
            currentContestValidations.isNumberOfProblemGroupsTouched = true;
            if (value) {
                currentContestValidations.isNUmberOfProblemGroupsValid = value > 0;
                numberOfProblemGroups = Number(value);
            }
            break;
        case 'duration': {
            let currentValue = value;

            if (currentValue === '') {
                currentValue = undefined;
            }

            const timeSpanRegex = /^-?(\d+\.)?(\d{1,2}):(\d{2}):(\d{2})(\.\d{1,7})?$/;
            currentContestValidations.isDurationValid = timeSpanRegex.test(value) || currentValue === undefined;
            currentContestValidations.isDurationTouched = true;
            duration = currentValue;
            break;
        }
        default: { break; }
        }
        setContestValidations(currentContestValidations);
        setContest((prevState) => ({
            ...prevState,
            name: contestName,
            type: contestType,
            limitBetweenSubmissions,
            autoChangeLimitBetweenSubmissions,
            orderBy,
            contestPassword,
            practicePassword,
            allowedIps,
            newIpPassword,
            description,
            startTime,
            endTime,
            practiceStartTime,
            practiceEndTime,
            isVisible,
            visibleFrom,
            allowParallelSubmissionsInTasks,
            categoryId,
            categoryName,
            numberOfProblemGroups,
            duration,
        }));
        validateForm();
    };

    const edit = () => {
        if (isValidForm) {
            updateContest(contest);
        }
    };

    const create = () => {
        if (isValidForm) {
            createContest(contest);
        }
    };

    const onDelete = () => {
        if (onDeleteSuccess) {
            onDeleteSuccess();
        }
        navigate(`/${NEW_ADMINISTRATION_PATH}/${CONTESTS_PATH}`);
    };

    if (isGettingCategories || isUpdating || isCreating || isLoading) {
        return (<SpinningLoader />);
    }

    return (
        <Box className={`${styles.flex}`}>
            {renderErrorMessagesAlert(errorMessages)}
            <Typography className={formStyles.centralize} variant="h5">
                {(contest.name && (
                <ExternalLink
                  to={getContestsDetailsPageUrl({
                      contestId: contest.id,
                      contestName: contest.name,
                  })}
                  text={contest.name}
                />
                )) || 'Contest form'}
            </Typography>
            <form className={formStyles.form}>
                <Box className={formStyles.fieldBox}>
                    <Typography className={formStyles.fieldBoxTitle} variant="h6">
                        General Information
                    </Typography>
                    <div className={formStyles.fieldBoxDivider} />
                    <Box className={formStyles.fieldBoxElement}>
                        <Box className={formStyles.row}>
                            <TextField
                              className={formStyles.inputRow}
                              label={NAME}
                              variant="standard"
                              name="name"
                              InputLabelProps={{ shrink: true }}
                              onChange={(e) => onChange(e)}
                              value={contest.name}
                              color={contestValidations.isNameValid && contestValidations.isNameTouched
                                  ? 'success'
                                  : 'primary'}
                              error={(contestValidations.isNameTouched && !contestValidations.isNameValid)}
                              helperText={(contestValidations.isNameTouched &&
                                    !contestValidations.isNameValid) && CONTEST_NAME_VALIDATION}
                            />
                        </Box>
                        <Box className={formStyles.row}>
                            <FormControl
                              className={formStyles.inputRow}
                            >
                                <InputLabel
                                  id="contest-type"
                                  sx={{ alignSelf: 'start', left: '-14px' }}
                                >
                                    {TYPE}
                                </InputLabel>
                                <Select
                                  sx={{ width: '100%' }}
                                  variant="standard"
                                  value={contest.type}
                                  className={formStyles.inputRow}
                                  name="type"
                                  labelId="contest-type"
                                  onChange={(e) => onChange(e)}
                                  onBlur={(e) => onChange(e)}
                                  color={contestValidations.isTypeValid && contestValidations.isTypeTouched
                                      ? 'success'
                                      : 'primary'}
                                  error={(contestValidations.isTypeTouched && !contestValidations.isTypeValid)}
                                >
                                    {Object.keys(ContestVariation).filter((key) => isNaN(Number(key))).map((key) => (
                                        <MenuItem key={key} value={key}>
                                            {key}
                                        </MenuItem>
                                    ))}
                                    helperText=
                                    {(contestValidations.isTypeTouched && !contestValidations.isTypeValid) &&
                                        CONTEST_TYPE_VALIDATION}
                                </Select>
                            </FormControl>
                        </Box>
                        <Box className={formStyles.row}>
                            { isWithRandomTasks && (
                                <TextField
                                  className={formStyles.inputRow}
                                  type="number"
                                  label={NUMBER_OF_PROBLEM_GROUPS}
                                  variant="standard"
                                  value={contest.numberOfProblemGroups}
                                  onChange={(e) => onChange(e)}
                                  InputLabelProps={{ shrink: true }}
                                  name="numberOfProblemGroups"
                                  disabled={isEditMode}
                                  error={(contestValidations.isNumberOfProblemGroupsTouched &&
                                      !contestValidations.isNUmberOfProblemGroupsValid)}
                                  helperText={(
                                      contestValidations.isNumberOfProblemGroupsTouched && !contestValidations.isNUmberOfProblemGroupsValid
                                  ) &&
                                        CONTEST_NUMBER_OF_PROBLEM_GROUPS}
                                />
                            )}
                        </Box>
                        <FormControl className={styles.textArea}>
                            <FormLabel>{DESCRIPTION}</FormLabel>
                            <TextareaAutosize
                              placeholder={CONTEST_DESCRIPTION_PLACEHOLDER_MESSAGE}
                              value={contest.description === null
                                  ? ''
                                  : contest.description}
                              minRows={10}
                              name="description"
                              onChange={(e) => onChange(e)}
                            />
                        </FormControl>
                        <FormControl className={styles.textArea} sx={{ margin: '20px 0' }}>
                            <Autocomplete
                              sx={{ width: '100%' }}
                              className={formStyles.inputRow}
                              filterOptions={autocompleteNameIdFormatFilterOptions}
                              onChange={(event, newValue) => handleAutocompleteChange('category', newValue!, 'id', onChange)}
                              value={contestCategories?.find((category) => category.id === contest.categoryId) ??
                                  getDefaultContestCategory()}
                              options={contestCategories!}
                              renderInput={(params) => <TextField {...params} label={SELECT_CATEGORY} key={params.id} />}
                              getOptionLabel={(option) => option?.name}
                              renderOption={(properties, option) => (
                                  <MenuItem {...properties} key={option.id} value={option.id}>
                                      #
                                      {option.id}
                                      {' '}
                                      {option.name}
                                  </MenuItem>
                              )}
                            />
                        </FormControl>
                    </Box>
                </Box>
                <Box className={formStyles.fieldBox}>
                    <Typography className={formStyles.fieldBoxTitle} variant="h6">
                        Duration Information
                    </Typography>
                    <div className={formStyles.fieldBoxDivider} />
                    <Box className={formStyles.fieldBoxElement}>
                        <Box className={formStyles.row}>
                            <DateTimePicker
                              className={styles.competeBorder}
                              name="startTime"
                              label={COMPETE_START_TIME}
                              value={getDateAsLocal(contest.startTime)}
                              onChange={(newValue) => handleDateTimePickerChange('startTime', newValue, onChange)}
                            />
                            <DateTimePicker
                              className={styles.competeBorder}
                              name="endTime"
                              label={COMPETE_END_TIME}
                              value={getDateAsLocal(contest.endTime)}
                              onChange={(newValue) => handleDateTimePickerChange('endTime', newValue, onChange)}
                            />
                        </Box>
                        <Box className={formStyles.row}>
                            <DateTimePicker
                              className={styles.practiceBorder}
                              name="practiceStartTime"
                              label={PRACTICE_START_TIME}
                              value={getDateAsLocal(contest.practiceStartTime)}
                              onChange={(newValue) => handleDateTimePickerChange('practiceStartTime', newValue, onChange)}
                            />
                            <DateTimePicker
                              className={styles.practiceBorder}
                              name="practiceEndTime"
                              label={PRACTICE_END_TIME}
                              value={getDateAsLocal(contest.practiceEndTime)}
                              onChange={(newValue) => handleDateTimePickerChange('practiceEndTime', newValue, onChange)}
                            />
                        </Box>
                        <Box className={formStyles.row}>
                            {isOnlineExam && (
                                <TextField
                                  className={formStyles.inputRow}
                                  type="string"
                                  label={DURATION}
                                  placeholder="hh:mm:ss"
                                  variant="standard"
                                  value={contest.duration
                                      ? contest.duration
                                      : undefined}
                                  name="duration"
                                  onChange={(e) => onChange(e)}
                                  disabled={!isOnlineExam}
                                  InputLabelProps={{ shrink: true }}
                                  error={(contestValidations.isDurationTouched && !contestValidations.isDurationValid)}
                                  helperText={(contestValidations.isDurationTouched && !contestValidations.isDurationValid) &&
                                        CONTEST_DURATION_VALIDATION}
                                />
                            )}
                        </Box>
                    </Box>
                </Box>
                <Box className={formStyles.fieldBox}>
                    <Typography className={formStyles.fieldBoxTitle} variant="h6">
                        Passwords
                    </Typography>
                    <div className={formStyles.fieldBoxDivider} />
                    <Box className={formStyles.fieldBoxElement}>
                        <Box className={formStyles.row}>
                            <TextField
                              className={concatClassNames(formStyles.inputRow, styles.competeUnderline)}
                              type="text"
                              label={COMPETE_PASSWORD}
                              variant="standard"
                              value={contest.contestPassword || ''}
                              name="contestPassword"
                              onChange={(e) => onChange(e)}
                              InputLabelProps={{ shrink: true }}
                            />
                            <TextField
                              className={concatClassNames(formStyles.inputRow, styles.practiceUnderline)}
                              type="text"
                              label={PRACTICE_PASSWORD}
                              variant="standard"
                              name="practicePassword"
                              onChange={(e) => onChange(e)}
                              value={contest.practicePassword || ''}
                              InputLabelProps={{ shrink: true }}
                            />
                        </Box>
                        <Box className={formStyles.row}>
                            <TextField
                              className={formStyles.inputRow}
                              label={NEW_IP_PASSWORD}
                              variant="standard"
                              value={contest.newIpPassword || ''}
                              name="newIpPassword"
                              onChange={(e) => onChange(e)}
                              type="text"
                              color={contestValidations.isNewIpPasswordValid && contestValidations.isNewIpPasswordTouched
                                  ? 'success'
                                  : 'primary'}
                              InputLabelProps={{ shrink: true }}
                              error={(contestValidations.isNewIpPasswordTouched && !contestValidations.isNewIpPasswordValid)}
                              helperText={(contestValidations.isNewIpPasswordTouched &&
                                    !contestValidations.isNewIpPasswordValid) && CONTEST_NEW_IP_PASSWORD_VALIDATION}
                            />
                            <TextField
                              className={formStyles.inputRow}
                              type="text"
                              label={ALLOWED_IPS}
                              variant="standard"
                              placeholder="Split by ;"
                              value={contest.allowedIps}
                              onChange={(e) => onChange(e)}
                              InputLabelProps={{ shrink: true }}
                              name="allowedIps"
                            />
                        </Box>
                    </Box>
                </Box>
                <Box className={formStyles.fieldBox}>
                    <Typography className={formStyles.fieldBoxTitle} variant="h6">
                        Options
                    </Typography>
                    <div className={formStyles.fieldBoxDivider} />
                    <Box className={formStyles.fieldBoxElement}>
                        <Box className={formStyles.row}>
                            <Box className={formStyles.row}>
                                <TextField
                                  className={formStyles.inputRow}
                                  type="number"
                                  name="limitBetweenSubmissions"
                                  disabled={contest.autoChangeLimitBetweenSubmissions}
                                  label={`${LIMIT_BETWEEN_SUBMISSIONS}`}
                                  variant="standard"
                                  onChange={(e) => onChange(e)}
                                  value={contest.limitBetweenSubmissions}
                                  InputLabelProps={{ shrink: true }}
                                  color={contestValidations.isLimitBetweenSubmissionsValid &&
                                    contestValidations.isLimitBetweenSubmissionsTouched
                                      ? 'success'
                                      : 'primary'}
                                  error={(contestValidations.isLimitBetweenSubmissionsTouched &&
                                        !contestValidations.isLimitBetweenSubmissionsValid)}
                                  helperText={(contestValidations.isLimitBetweenSubmissionsTouched &&
                                            !contestValidations.isLimitBetweenSubmissionsValid) &&
                                        CONTEST_LIMIT_BETWEEN_SUBMISSIONS_VALIDATION}
                                />
                                <Tooltip
                                  title="Automatically adjust the limit between submissions during a contest,
                                    based on the number of submissions."
                                >
                                    <FormControlLabel
                                      control={<Checkbox checked={contest.autoChangeLimitBetweenSubmissions} />}
                                      label="Auto"
                                      name="autoChangeLimitBetweenSubmissions"
                                      onChange={(e) => onChange(e)}
                                    />
                                </Tooltip>
                            </Box>
                            <TextField
                              className={formStyles.inputRow}
                              type="number"
                              label={ORDER_BY}
                              variant="standard"
                              value={contest.orderBy}
                              onChange={(e) => onChange(e)}
                              InputLabelProps={{ shrink: true }}
                              name="orderBy"
                              color={contestValidations.isOrderByValid && contestValidations.isOrderByTouched
                                  ? 'success'
                                  : 'primary'}
                              error={(contestValidations.isOrderByTouched && !contestValidations.isOrderByValid)}
                              helperText={(contestValidations.isOrderByTouched && !contestValidations.isOrderByValid) &&
                                CONTEST_ORDER_BY_VALIDATION}
                            />
                        </Box>
                        <Box className={formStyles.fieldBoxCheckBoxes}>
                            <FormControlLabel
                              control={<Checkbox checked={contest.isVisible} />}
                              label={IS_VISIBLE}
                              name="isVisible"
                              onChange={(e) => onChange(e)}
                            />
                            <DateTimePicker
                              sx={{ width: '100%', margin: '20px 0' }}
                              name="visibleFrom"
                              label={VISIBLE_FROM}
                              value={getDateAsLocal(contest.visibleFrom)}
                              onChange={(newValue) => handleDateTimePickerChange('visibleFrom', newValue, onChange)}
                            />
                            <FormControlLabel
                              control={(
                                  <Checkbox
                                    checked={contest.allowParallelSubmissionsInTasks}
                                  />
                                )}
                              name="allowParallelSubmissionsInTasks"
                              onChange={(e) => onChange(e)}
                              label={ALLOW_PARALLEL_SUBMISSIONS_IN_TASKS}
                            />
                        </Box>
                    </Box>
                </Box>
            </form>

            <AdministrationFormButtons
              isEditMode={isEditMode}
              onCreateClick={() => create()}
              onEditClick={() => edit()}
              disabled={!isValidForm}
            />

            <Box sx={{ alignSelf: 'flex-end', display: 'flex' }}>
                <TransferParticipantsButton
                  contestId={Number(contestId!)}
                  contestName={contest.name}
                  contestOfficialParticipants={contest.officialParticipants}
                  categoryName={contestCategories?.find((category) => category.id === contest.categoryId)?.name}
                  mutation={useTransferParticipantsMutation}
                  openDialog={openTransferDialog}
                />
                <DeleteButton
                  id={Number(contestId!)}
                  name={contest.name}
                  onSuccess={onDelete}
                  mutation={useDeleteContestMutation}
                  text={DELETE_CONFIRMATION_MESSAGE}
                />
            </Box>
        </Box>
    );
};

export default ContestEdit;
