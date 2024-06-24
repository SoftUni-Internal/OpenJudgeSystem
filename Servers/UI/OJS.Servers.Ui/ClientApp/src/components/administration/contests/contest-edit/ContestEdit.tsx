/* eslint-disable @typescript-eslint/ban-types */
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Autocomplete, Box, Checkbox, FormControl, FormControlLabel, FormLabel, InputLabel, MenuItem, Select, TextareaAutosize, TextField, Typography } from '@mui/material';
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
    ID,
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
import { useGetCategoriesQuery } from '../../../../redux/services/admin/contestCategoriesAdminService';
import { useCreateContestMutation, useDeleteContestMutation, useGetContestByIdQuery, useUpdateContestMutation } from '../../../../redux/services/admin/contestsAdminService';
import { convertToUtc, getDateAsLocal } from '../../../../utils/administration/administration-dates';
import { getAndSetExceptionMessage, getAndSetSuccesfullMessages } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import { getEnumMemberName } from '../../../../utils/string-utils';
import ExternalLink from '../../../guidelines/buttons/ExternalLink';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import AdministrationFormButtons from '../../common/administration-form-buttons/AdministrationFormButtons';
import DeleteButton from '../../common/delete/DeleteButton';
import { handleAutocompleteChange, handleDateTimePickerChange } from '../../utils/mui-utils';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';
import styles from './ContestEdit.module.scss';

interface IContestEditProps {
    contestId: number | null;
    isEditMode?: boolean;
    currentContest?: IContestAdministration;
    onSuccess?: Function;
    onDeleteSuccess? : Function;
    skipGettingContest?: boolean;
}

const NAME_PROP = 'name';
const ContestEdit = (props:IContestEditProps) => {
    const { contestId, isEditMode = true, currentContest, onSuccess, onDeleteSuccess, skipGettingContest = false } = props;

    const navigate = useNavigate();

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ isValidForm, setIsValidForm ] = useState<boolean>(!!isEditMode);

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
    });

    const [ contestValidations, setContestValidations ] = useState({
        isNameTouched: false,
        isNameValid: !!isEditMode,
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
            isSuccess: isSuccessfullyUpdating,
        } ] = useUpdateContestMutation();

    const [
        createContest, {
            data: createData,
            isSuccess: isSuccessfullyCreating,
            error: createError,
            isLoading: isCreating,
        } ] = useCreateContestMutation();

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
        const message = getAndSetSuccesfullMessages([
            { message: updateData, shouldGet: isSuccessfullyUpdating },
            { message: createData, shouldGet: isSuccessfullyCreating } ]);
        setSuccessMessage(message);
    }, [ updateData, createData, isSuccessfullyUpdating, isSuccessfullyCreating ]);

    useEffect(() => {
        getAndSetExceptionMessage([ createError, updateError ], setErrorMessages);
        setSuccessMessage(null);
    }, [ updateError, createError ]);

    useEffect(() => {
        if (isSuccessfullyUpdating && onSuccess) {
            /* The function is called in timeout,
            because we want to show success message before calling onSuccess,
            since it can cause re-rendering of the component and the message will not be visible
            */
            setTimeout(() => {
                onSuccess();
            }, 500);
        }
    }, [ isSuccessfullyUpdating, onSuccess ]);

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

            if (value === getEnumMemberName(ContestVariation, ContestVariation.OnlinePracticalExam).toString()) {
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
            {renderSuccessfullAlert(successMessage)}
            <Typography className={formStyles.centralize} variant="h4">
                {(contest.name && <ExternalLink to={`/contests/${contest.id}`} text={contest.name} />) || 'Contest form'}
            </Typography>
            <form className={`${formStyles.form}`}>
                <Box className={`${styles.fieldBox}`}>
                    <Box>
                        <TextField
                          className={formStyles.inputRow}
                          label={ID}
                          variant="standard"
                          value={contest.id}
                          disabled
                        />
                        <TextField
                          className={formStyles.inputRow}
                          label={NAME}
                          variant="standard"
                          name="name"
                          onChange={(e) => onChange(e)}
                          value={contest.name}
                          color={contestValidations.isNameValid && contestValidations.isNameTouched
                              ? 'success'
                              : 'primary'}
                          error={(contestValidations.isNameTouched && !contestValidations.isNameValid)}
                          helperText={(contestValidations.isNameTouched &&
                                    !contestValidations.isNameValid) && CONTEST_NAME_VALIDATION}
                        />
                        <TextField
                          className={formStyles.inputRow}
                          type="number"
                          name="limitBetweenSubmissions"
                          label={LIMIT_BETWEEN_SUBMISSIONS}
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
                        { contest.type === getEnumMemberName(ContestVariation, ContestVariation.OnlinePracticalExam) && (
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
                          error={(contestValidations.isNumberOfProblemGroupsTouched && !contestValidations.isNUmberOfProblemGroupsValid)}
                          helperText={(
                              contestValidations.isNumberOfProblemGroupsTouched && !contestValidations.isNUmberOfProblemGroupsValid
                          ) &&
                          CONTEST_NUMBER_OF_PROBLEM_GROUPS}
                        />
                        )}
                    </Box>
                    <Box>
                        <TextField
                          className={formStyles.inputRow}
                          type="text"
                          label={COMPETE_PASSWORD}
                          variant="standard"
                          value={contest.contestPassword || ''}
                          name="contestPassword"
                          onChange={(e) => onChange(e)}
                          InputLabelProps={{ shrink: true }}
                        />
                        <TextField
                          className={formStyles.inputRow}
                          type="text"
                          label={PRACTICE_PASSWORD}
                          variant="standard"
                          name="practicePassword"
                          onChange={(e) => onChange(e)}
                          value={contest.practicePassword || ''}
                          InputLabelProps={{ shrink: true }}
                        />
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
                        {contest.type === getEnumMemberName(ContestVariation, ContestVariation.OnlinePracticalExam) && (
                        <TextField
                          className={formStyles.inputRow}
                          type="string"
                          label={DURATION}
                          variant="standard"
                          value={contest.duration
                              ? contest.duration
                              : undefined}
                          name="duration"
                          onChange={(e) => onChange(e)}
                          disabled={contest.type !== getEnumMemberName(ContestVariation, ContestVariation.OnlinePracticalExam)}
                          InputLabelProps={{ shrink: true }}
                          error={(contestValidations.isDurationTouched && !contestValidations.isDurationValid)}
                          helperText={(contestValidations.isDurationTouched && !contestValidations.isDurationValid) &&
                            CONTEST_DURATION_VALIDATION}
                        />
                        )}
                    </Box>
                </Box>
                <FormControl
                  className={formStyles.inputRow}
                >
                    <InputLabel id="contest-type">{TYPE}</InputLabel>
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
                      onChange={(event, newValue) => handleAutocompleteChange('category', newValue!, 'id', onChange)}
                      value={contestCategories?.find((category) => category.id === contest.categoryId) ?? contestCategories![0]}
                      options={contestCategories!}
                      renderInput={(params) => <TextField {...params} label={SELECT_CATEGORY} key={params.id} />}
                      getOptionLabel={(option) => option?.name}
                      renderOption={(properties, option) => (
                          <MenuItem {...properties} key={option.id} value={option.id}>
                              {option.name}
                          </MenuItem>
                      )}
                    />
                </FormControl>
                <Box className={formStyles.row}>
                    <DateTimePicker
                      sx={{ width: '48%' }}
                      name="startTime"
                      label={COMPETE_START_TIME}
                      value={getDateAsLocal(contest.startTime)}
                      onChange={(newValue) => handleDateTimePickerChange('startTime', newValue, onChange)}
                    />
                    <DateTimePicker
                      sx={{ width: '48%' }}
                      name="endTime"
                      label={COMPETE_END_TIME}
                      value={getDateAsLocal(contest.endTime)}
                      onChange={(newValue) => handleDateTimePickerChange('endTime', newValue, onChange)}
                    />
                </Box>
                <Box className={formStyles.row}>
                    <DateTimePicker
                      sx={{ width: '48%', margin: '20px 0' }}
                      name="practiceStartTime"
                      label={PRACTICE_START_TIME}
                      value={getDateAsLocal(contest.practiceStartTime)}
                      onChange={(newValue) => handleDateTimePickerChange('practiceStartTime', newValue, onChange)}
                    />
                    <DateTimePicker
                      sx={{ width: '48%', margin: '20px 0' }}
                      name="practiceEndTime"
                      label={PRACTICE_END_TIME}
                      value={getDateAsLocal(contest.practiceEndTime)}
                      onChange={(newValue) => handleDateTimePickerChange('practiceEndTime', newValue, onChange)}
                    />
                </Box>
                <Box className={formStyles.row}>
                    <DateTimePicker
                      sx={{ width: '48%', margin: '20px 0' }}
                      name="visibleFrom"
                      label={VISIBLE_FROM}
                      value={getDateAsLocal(contest.visibleFrom)}
                      onChange={(newValue) => handleDateTimePickerChange('visibleFrom', newValue, onChange)}
                    />
                </Box>
                <Box className={styles.checkboxes}>
                    <FormControlLabel
                      control={<Checkbox checked={contest.isVisible} />}
                      label={IS_VISIBLE}
                      name="isVisible"
                      onChange={(e) => onChange(e)}
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
            </form>

            <AdministrationFormButtons
              isEditMode={isEditMode}
              onCreateClick={() => create()}
              onEditClick={() => edit()}
              disabled={!isValidForm}
            />

            <Box sx={{ alignSelf: 'flex-end' }}>
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
