import React, { useEffect, useState } from 'react';
import { Box, TextField, Typography } from '@mui/material';
import { DateTimePicker } from '@mui/x-date-pickers';
import {
    QUOTA_LIMIT,
    QUOTA_RESET_TIME,
    REQUESTS_MADE, USERNAME,
} from 'src/common/labels';
import { IUserMentorAdministrationModel } from 'src/common/types';
import SpinningLoader from 'src/components/guidelines/spinning-loader/SpinningLoader';
import useDelayedSuccessEffect from 'src/hooks/common/use-delayed-success-effect';
import useDisableMouseWheelOnNumberInputs from 'src/hooks/common/use-disable-mouse-wheel-on-number-inputs';
import useSuccessMessageEffect from 'src/hooks/common/use-success-message-effect';
import {
    useGetUserMentorByIdQuery,
    useUpdateUserMentorMutation,
} from 'src/redux/services/admin/usersMentorsAdminService';
import { convertToUtc, getDateAsLocal } from 'src/utils/administration/administration-dates';
import { getAndSetExceptionMessage } from 'src/utils/messages-utils';
import { renderErrorMessagesAlert } from 'src/utils/render-utils';
import clearSuccessMessages from 'src/utils/success-messages-utils';

import AdministrationFormButtons from '../common/administration-form-buttons/AdministrationFormButtons';

import formStyles from '../common/styles/FormStyles.module.scss';

interface IUserMentorEditProps {
    userMentorId?: string;
    currentUserMentor?: IUserMentorAdministrationModel;
    onSuccess?: Function;
    setParentSuccessMessage: Function;
}
const UserMentorEdit = (props: IUserMentorEditProps) => {
    const {
        userMentorId,
        currentUserMentor,
        onSuccess,
        setParentSuccessMessage,
    } = props;

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ isValidForm, setIsValidForm ] = useState<boolean>(true);

    const [ userMentor, setUserMentor ] = useState<IUserMentorAdministrationModel>({
        id: '',
        quotaResetTime: new Date(),
        requestsMade: 0,
        quotaLimit: null,
        createdOn: new Date(),
        modifiedOne: new Date(),
        userUserName: '',
    });

    const [ userMentorValidations, setUserMentorValidations ] = useState({
        isQuotaLimitTouched: false,
        isQuotaLimitValid: true,
    });

    const { data, isLoading } = useGetUserMentorByIdQuery(userMentorId!, { skip: userMentorId === undefined });

    const [
        updateUserMentor,
        {
            data: updateData,
            isLoading: isUpdating,
            error: updateError,
            isSuccess: isSuccessfullyUpdated,
        },
    ] = useUpdateUserMentorMutation();

    useDisableMouseWheelOnNumberInputs();

    useSuccessMessageEffect({
        data: [
            { message: updateData, shouldGet: isSuccessfullyUpdated },
        ],
        setParentSuccessMessage,
        clearFlags: [ isUpdating ],
    });

    useDelayedSuccessEffect({ isSuccess: isSuccessfullyUpdated, onSuccess });

    useEffect(() => {
        if (currentUserMentor) {
            setUserMentor(currentUserMentor);
        }
    }, [ currentUserMentor ]);

    useEffect(() => {
        if (data) {
            setUserMentor(data);
        }
    }, [ data ]);

    useEffect(() => {
        getAndSetExceptionMessage([ updateError ], setErrorMessages);
        clearSuccessMessages({ setParentSuccessMessage });
    }, [ updateError, setParentSuccessMessage ]);

    const validateForm = () => {
        setIsValidForm(userMentorValidations.isQuotaLimitValid);
    };

    const onChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | { name?: string; value: unknown }>) => {
        const { name, value } = e.target as HTMLInputElement;
        const updatedUserMentor = { ...userMentor };
        const updatedValidations = { ...userMentorValidations };

        switch (name) {
        case 'quotaLimit': {
            updatedValidations.isQuotaLimitTouched = true;
            const parsedValue = value !== ''
                ? Number(value)
                : null;

            if (parsedValue === null || parsedValue >= 0) {
                updatedValidations.isQuotaLimitValid = true;
                updatedUserMentor.quotaLimit = parsedValue;
            } else {
                updatedValidations.isQuotaLimitValid = false;
            }
            break;
        }

        case 'quotaResetTime': {
            updatedUserMentor.quotaResetTime = value
                ? convertToUtc(new Date(value)) ?? new Date()
                : new Date();
            break;
        }

        default:
            break;
        }

        setUserMentor(updatedUserMentor);
        setUserMentorValidations(updatedValidations);
        validateForm();
    };

    const handleDateChange = (name: string, newValue: Date | undefined) => {
        if (newValue) {
            setUserMentor((prevState) => ({
                ...prevState,
                [name]: convertToUtc(newValue),
            }));
        }
    };

    const edit = () => {
        if (isValidForm) {
            updateUserMentor(userMentor);
        }
    };

    if (isLoading || isUpdating) {
        return <SpinningLoader />;
    }

    return (
        <Box className={formStyles.form}>
            {renderErrorMessagesAlert(errorMessages)}
            <Typography className={formStyles.centralize} variant="h5">
                User Mentor Form
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
                              type="text"
                              label={USERNAME}
                              variant="standard"
                              name="userUserName"
                              onChange={onChange}
                              value={userMentor.userUserName}
                              InputLabelProps={{ shrink: true }}
                              disabled
                            />
                        </Box>
                        <Box className={formStyles.row}>
                            <DateTimePicker
                              className={formStyles.inputRow}
                              name="quotaResetTime"
                              label={QUOTA_RESET_TIME}
                              value={getDateAsLocal(userMentor.quotaResetTime)}
                              onChange={(newValue) => handleDateChange('quotaResetTime', newValue?.toDate())}
                            />
                        </Box>
                        <Box className={formStyles.row}>
                            <TextField
                              className={formStyles.inputRow}
                              type="number"
                              label={REQUESTS_MADE}
                              variant="standard"
                              name="requestsMade"
                              onChange={onChange}
                              value={userMentor.requestsMade}
                              InputLabelProps={{ shrink: true }}
                              disabled
                            />
                        </Box>
                        <Box className={formStyles.row}>
                            <TextField
                              className={formStyles.inputRow}
                              type="number"
                              label={QUOTA_LIMIT}
                              variant="standard"
                              name="quotaLimit"
                              onChange={onChange}
                              value={userMentor.quotaLimit !== null
                                  ? userMentor.quotaLimit
                                  : ''}
                              InputLabelProps={{ shrink: true }}
                              color={
                                    userMentorValidations.isQuotaLimitValid && userMentorValidations.isQuotaLimitTouched
                                        ? 'success'
                                        : 'primary'
                                }
                              error={
                                    userMentorValidations.isQuotaLimitTouched && !userMentorValidations.isQuotaLimitValid
                                }
                              helperText={
                                    userMentorValidations.isQuotaLimitTouched && !userMentorValidations.isQuotaLimitValid
                                        ? 'Quota Limit must be a non-negative number or null.'
                                        : ''
                                }
                            />
                        </Box>
                    </Box>
                </Box>
            </form>

            <AdministrationFormButtons
              isEditMode
              onEditClick={edit}
              disabled={!isValidForm}
            />
        </Box>
    );
};

export default UserMentorEdit;
