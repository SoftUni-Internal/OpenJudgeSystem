import React, { useEffect, useState } from 'react';
import { Box, FormControl, InputLabel, MenuItem, Select, TextField, Typography } from '@mui/material';
import { DateTimePicker } from '@mui/x-date-pickers';
import { ContestVariation } from 'src/common/contest-types';
import {
    COMPETE_END_TIME,
    COMPETE_START_TIME,
    EDIT,
    LIMIT_BETWEEN_SUBMISSIONS,
    PRACTICE_END_TIME,
    PRACTICE_START_TIME,
    TYPE,
} from 'src/common/labels';
import { CONTEST_LIMIT_BETWEEN_SUBMISSIONS_VALIDATION, CONTEST_TYPE_VALIDATION } from 'src/common/messages';
import { IContestsBulkEdit } from 'src/common/types';
import FormActionButton from 'src/components/administration/form-action-button/FormActionButton';
import { handleDateTimePickerChange } from 'src/components/administration/utils/mui-utils';
import ConfirmDialog from 'src/components/guidelines/dialog/ConfirmDialog';
import useDelayedSuccessEffect from 'src/hooks/common/use-delayed-success-effect';
import useSuccessMessageEffect from 'src/hooks/common/use-success-message-effect';
import { useBulkEditContestsMutation } from 'src/redux/services/admin/contestsAdminService';
import { convertToUtc, getDateAsLocal } from 'src/utils/administration/administration-dates';
import { getAndSetExceptionMessage } from 'src/utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from 'src/utils/render-utils';
import clearSuccessMessages from 'src/utils/success-messages-utils';

import formStyles from '../../../../components/administration/common/styles/FormStyles.module.scss';
import styles from './ContestsBulkEdit.module.scss';

interface IContestsBulkEditProps {
    categoryId?: number;
    categoryName?: string;
    setParentSuccessMessage: Function;
    onSuccess: Function;
}

const ContestsBulkEdit = ({ categoryId, categoryName, setParentSuccessMessage, onSuccess }: IContestsBulkEditProps) => {
    const [
        bulkEditContests,
        {
            data: updateData,
            error: updateError,
            isSuccess: isSuccessfullyUpdated,
            isLoading: isUpdating,
        },
    ] = useBulkEditContestsMutation();

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ showConfirmationDialog, setConfirmationDialog ] = useState<boolean>(false);
    const [ formData, setFormData ] = useState<IContestsBulkEdit>({
        startTime: null,
        endTime: null,
        practiceStartTime: null,
        practiceEndTime: null,
        type: null,
        limitBetweenSubmissions: null,
        categoryId: categoryId ?? 0,
    });

    const [ formDataValidations, setFormValidations ] = useState({
        isTypeValid: true,
        isTypeTouched: false,
        isLimitBetweenSubmissionsValid: true,
        isLimitBetweenSubmissionsTouched: false,
        isOrderByValid: true,
        isOrderByTouched: false,
    });

    useDelayedSuccessEffect({
        isSuccess: isSuccessfullyUpdated,
        onSuccess,
    });

    useSuccessMessageEffect({
        data: [
            { message: updateData, shouldGet: isSuccessfullyUpdated },
        ],
        setSuccessMessage,
        setParentSuccessMessage,
        clearFlags: [ isUpdating ],
    });

    const onChange = (e: any) => {
        const { name, value } = e.target;
        let {
            startTime,
            endTime,
            practiceStartTime,
            practiceEndTime,
            type,
            limitBetweenSubmissions,
        } = formData;

        const currentValidations = formDataValidations;

        switch (name) {
        case 'type': {
            type = value === ''
                ? null
                : value;
            currentValidations.isTypeTouched = true;
            currentValidations.isTypeValid = true;
            break;
        }
        case 'limitBetweenSubmissions': {
            currentValidations.isLimitBetweenSubmissionsTouched = true;
            currentValidations.isLimitBetweenSubmissionsValid = true;
            limitBetweenSubmissions = value === ''
                ? null
                : Number(value);
            if (limitBetweenSubmissions !== null && limitBetweenSubmissions < 0) {
                currentValidations.isLimitBetweenSubmissionsValid = false;
            }
            break;
        }
        case 'startTime': {
            startTime = null;
            if (value) {
                startTime = convertToUtc(value);
            }
            break;
        }
        case 'endTime': {
            endTime = null;
            if (value) {
                endTime = convertToUtc(value);
            }
            break;
        }
        case 'practiceStartTime': {
            practiceStartTime = null;
            if (value) {
                practiceStartTime = convertToUtc(value);
            }
            break;
        }
        case 'practiceEndTime': {
            practiceEndTime = null;
            if (value) {
                practiceEndTime = convertToUtc(value);
            }
            break;
        }
        default: {
            break;
        }
        }

        setFormValidations(currentValidations);
        setFormData((prevState) => ({
            ...prevState,
            startTime,
            endTime,
            practiceStartTime,
            practiceEndTime,
            type,
            limitBetweenSubmissions,
        }));
    };

    useEffect(() => {
        getAndSetExceptionMessage([ updateError ], setErrorMessages);
        clearSuccessMessages({ setParentSuccessMessage });
    }, [ setParentSuccessMessage, updateError ]);

    return (
        <Box className={styles.flex}>
            {renderErrorMessagesAlert(errorMessages)}
            {renderSuccessfullAlert(successMessage)}
            {showConfirmationDialog &&
            <ConfirmDialog
              title={`Are you sure you want to edit all contests${categoryName
                  ? ` in ${categoryName}`
                  : ''}?`}
              text={(
                  <>
                      Each contest in the category will be updated with the provided values.
                      <strong className={styles.confirmationImportant}> Note that required fields </strong>
                      {' '}
                      for a contest like
                      <strong className={styles.confirmationImportant}> Type</strong>
                      {' '}
                      and
                      <strong className={styles.confirmationImportant}> Time Between Submissions</strong>
                      ,
                      {' '}
                      are
                      {' '}
                      <strong className={styles.confirmationUnderlined}>optional</strong>
                      {' '}
                      here.
                      This means that if no value is provided, the existing values will be
                      {' '}
                      <strong className={styles.confirmationUnderlined}>preserved</strong>
                      .
                      The fields
                      {' '}
                      <strong className={styles.confirmationImportant}>Compete Start Time</strong>
                      ,
                      {' '}
                      <strong className={styles.confirmationImportant}>Compete End Time</strong>
                      ,
                      {' '}
                      <strong className={styles.confirmationImportant}>Participation Start Time</strong>
                      {' '}
                      and
                      {' '}
                      <strong className={styles.confirmationImportant}>Participation End Time</strong>
                      {' '}
                      will be
                      {' '}
                      <strong className={styles.confirmationUnderlined}>cleared</strong>
                      {' '}
                      if no value is provided.
                  </>
                )}
              confirmButtonText="Edit"
              declineButtonText="Cancel"
              onClose={() => setConfirmationDialog(!showConfirmationDialog)}
              confirmFunction={() => bulkEditContests(formData)}
            />
            }
            <Typography className={formStyles.centralize} variant="h4">
                Contests Edit
            </Typography>
            {categoryName &&
            <Typography className={formStyles.centralize} variant="h5">
                {categoryName}
            </Typography>
            }
            <Box className={formStyles.fieldBox}>
                <Typography className={formStyles.fieldBoxTitle} variant="h6">
                    General Information
                </Typography>
                <div className={formStyles.fieldBoxDivider} />

                <Box className={formStyles.fieldBoxElement}>
                    <Box className={formStyles.row}>
                        <FormControl className={formStyles.inputRow}>
                            <InputLabel id="formData-type" sx={{ alignSelf: 'start', left: '-14px' }}>{TYPE}</InputLabel>
                            <Select
                              variant="standard"
                              value={formData.type ?? ''}
                              name="type"
                              labelId="formData-type"
                              onChange={(e) => onChange(e)}
                              onBlur={onChange}
                              color={formDataValidations.isTypeValid && formDataValidations.isTypeTouched
                                  ? 'success'
                                  : 'primary'}
                              error={formDataValidations.isTypeTouched && !formDataValidations.isTypeValid}
                            >
                                {Object.keys(ContestVariation).filter((key) => Number.isNaN(Number(key))).map((key) =>
                                    <MenuItem key={key} value={key}>{key}</MenuItem>)}
                            </Select>
                            {formDataValidations.isTypeTouched && !formDataValidations.isTypeValid &&
                                <Typography variant="caption" color="error">{CONTEST_TYPE_VALIDATION}</Typography>
                            }
                        </FormControl>
                    </Box>
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
                          label={COMPETE_START_TIME}
                          value={formData.startTime
                              ? getDateAsLocal(formData.startTime)
                              : null}
                          onChange={(newValue) => handleDateTimePickerChange('startTime', newValue, onChange)}
                        />
                        <DateTimePicker
                          className={styles.competeBorder}
                          label={COMPETE_END_TIME}
                          value={formData.endTime
                              ? getDateAsLocal(formData.endTime)
                              : null}
                          onChange={(newValue) => handleDateTimePickerChange('endTime', newValue, onChange)}
                        />
                    </Box>

                    <Box className={formStyles.row}>
                        <DateTimePicker
                          className={styles.practiceBorder}
                          label={PRACTICE_START_TIME}
                          value={formData.practiceStartTime
                              ? getDateAsLocal(formData.practiceStartTime)
                              : null}
                          onChange={(newValue) => handleDateTimePickerChange('practiceStartTime', newValue, onChange)}
                        />
                        <DateTimePicker
                          className={styles.practiceBorder}
                          label={PRACTICE_END_TIME}
                          value={formData.practiceEndTime
                              ? getDateAsLocal(formData.practiceEndTime)
                              : null}
                          onChange={(newValue) => handleDateTimePickerChange('practiceEndTime', newValue, onChange)}
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
                        <TextField
                          className={formStyles.inputRow}
                          type="number"
                          name="limitBetweenSubmissions"
                          label={LIMIT_BETWEEN_SUBMISSIONS}
                          variant="standard"
                          onChange={onChange}
                          value={formData.limitBetweenSubmissions ?? ''}
                          InputLabelProps={{ shrink: true }}
                          color={formDataValidations.isLimitBetweenSubmissionsValid && formDataValidations.isLimitBetweenSubmissionsTouched
                              ? 'success'
                              : 'primary'}
                          error={formDataValidations.isLimitBetweenSubmissionsTouched && !formDataValidations.isLimitBetweenSubmissionsValid}
                          helperText={formDataValidations.isLimitBetweenSubmissionsTouched && !formDataValidations.isLimitBetweenSubmissionsValid && CONTEST_LIMIT_BETWEEN_SUBMISSIONS_VALIDATION}
                        />
                    </Box>
                </Box>
            </Box>

            <FormActionButton
              className={formStyles.buttonsWrapper}
              buttonClassName={formStyles.button}
              onClick={() => setConfirmationDialog(true)}
              disabled={isUpdating}
              name={EDIT}
            />
        </Box>
    );
};

export default ContestsBulkEdit;
