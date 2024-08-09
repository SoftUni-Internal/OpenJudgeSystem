/* eslint-disable @typescript-eslint/ban-types */
import { ChangeEvent, useEffect, useState } from 'react';
import { Box, FormControl, FormGroup, TextField, Typography } from '@mui/material';
import { DateTimePicker } from '@mui/x-date-pickers';
import isNaN from 'lodash/isNaN';

import { AGE, CITY, COMPANY, DATE_OF_BIRTH, EDIT, EDUCATIONAL_INSTITUTE, EMAIL, FACULTY_NUMBER, FIRSTNAME, ID, JOB_TITLE, LASTNAME, USERNAME } from '../../../../common/labels';
import { IUserAdministrationModel } from '../../../../common/types';
import useDisableMouseWheelOnNumberInputs from '../../../../hooks/common/use-disable-mouse-wheel-on-number-inputs';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { useGetUserByIdQuery, useUpdateUserMutation } from '../../../../redux/services/admin/usersAdminService';
import { convertToUtc, getDateAsLocal } from '../../../../utils/administration/administration-dates';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import FormActionButton from '../../form-action-button/FormActionButton';
import { handleDateTimePickerChange } from '../../utils/mui-utils';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';

interface IUserFormProps {
    id: string;

    providedUser?: IUserAdministrationModel;
    onSuccessfullyUpdated?: Function;
}

const UserForm = (props: IUserFormProps) => {
    const { id, providedUser, onSuccessfullyUpdated } = props;
    const [ exceptionMessages, setExceptionMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);

    const [ user, setUser ] = useState<IUserAdministrationModel>({
        id,
        email: '',
        userName: '',
        userSettings: {
            age: providedUser?.userSettings.age || 0,
            city: null,
            company: null,
            dateOfBirth: null,
            educationalInstitution: null,
            facultyNumber: null,
            firstName: null,
            jobTitle: null,
            lastName: null,
        },
        roles: [],
    });

    const {
        refetch,
        data: getData,
        error: getError,
        isLoading: isGetting,
    } = useGetUserByIdQuery(id, { skip: !!providedUser });

    const [
        update, {
            data: updateData,
            isSuccess: isSuccessfullyUpdated,
            error: updateError,
            isLoading: isUpdating,
        },
    ] = useUpdateUserMutation();

    useDisableMouseWheelOnNumberInputs();

    useSuccessMessageEffect({
        data: [
            { message: updateData, shouldGet: isSuccessfullyUpdated },
        ],
        setSuccessMessage,
    });

    useEffect(() => {
        if (providedUser) {
            setUser(providedUser);
        }
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);
    useEffect(() => {
        if (getData) {
            setUser(getData);
        }
    }, [ getData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ getError, updateError ], setExceptionMessages);
    }, [ updateError, getError ]);

    useEffect(() => {
        if (isSuccessfullyUpdated) {
            if (onSuccessfullyUpdated) {
                onSuccessfullyUpdated();
            } else {
                refetch();
            }
        }
    }, [ updateData, isSuccessfullyUpdated, onSuccessfullyUpdated, refetch ]);

    const onChange = (event: ChangeEvent<HTMLInputElement>) => {
        const { name, value } = event.target;
        const nameParts = name.split('.');

        setUser((prevState) => {
            const updateState = (path: string[], val: any, obj: any): any => {
                const key = path[0];
                if (path.length === 1) {
                    return { ...obj, [key]: val };
                }
                let currVal = val;
                if (currVal === '') {
                    currVal = null;
                }
                if (!isNaN(new Date(currVal).getDate())) {
                    currVal = convertToUtc(currVal);
                }
                return { ...obj, [key]: updateState(path.slice(1), currVal, obj[key] ?? {}) };
            };

            return updateState(nameParts, value, prevState);
        });
    };

    if (isGetting || isUpdating) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            {renderErrorMessagesAlert(exceptionMessages)}
            <Typography className={formStyles.centralize} variant="h4">
                {user.userName}
                {' '}
                administration form
            </Typography>
            <form className={formStyles.form}>
                <Box className={formStyles.inputRow}>
                    <FormGroup className={formStyles.inputRow}>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={ID}
                              value={user?.id}
                              InputLabelProps={{ shrink: true }}
                              type="text"
                              disabled
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={USERNAME}
                              name="userName"
                              value={user?.userName}
                              InputLabelProps={{ shrink: true }}
                              type="text"
                              onChange={onChange}
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={EMAIL}
                              name="email"
                              value={user?.email}
                              InputLabelProps={{ shrink: true }}
                              type="email"
                              onChange={onChange}
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={FIRSTNAME}
                              name="userSettings.firstName"
                              value={user?.userSettings.firstName ?? ''}
                              InputLabelProps={{ shrink: true }}
                              type="text"
                              onChange={onChange}
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={LASTNAME}
                              name="userSettings.lastName"
                              value={user?.userSettings.lastName ?? ''}
                              InputLabelProps={{ shrink: true }}
                              type="text"
                              onChange={onChange}
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={AGE}
                              name="userSettings.age"
                              value={user?.userSettings.age}
                              InputLabelProps={{ shrink: true }}
                              type="number"
                              onChange={onChange}
                              disabled
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <DateTimePicker
                              name="userSettings.dateOfBirth"
                              label={DATE_OF_BIRTH}
                              value={getDateAsLocal(user.userSettings.dateOfBirth)}
                              onChange={(newValue) => handleDateTimePickerChange('userSettings.dateOfBirth', newValue, onChange)}
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={CITY}
                              name="userSettings.city"
                              value={user?.userSettings.city ?? ''}
                              InputLabelProps={{ shrink: true }}
                              type="text"
                              onChange={onChange}
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={COMPANY}
                              name="userSettings.company"
                              value={user?.userSettings.company ?? ''}
                              InputLabelProps={{ shrink: true }}
                              type="text"
                              onChange={onChange}
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={JOB_TITLE}
                              name="userSettings.jobTitle"
                              value={user?.userSettings.jobTitle ?? ''}
                              InputLabelProps={{ shrink: true }}
                              type="text"
                              onChange={onChange}
                            />
                        </FormControl>

                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={EDUCATIONAL_INSTITUTE}
                              name="userSettings.educationalInstitution"
                              value={user?.userSettings.educationalInstitution ?? ''}
                              InputLabelProps={{ shrink: true }}
                              type="number"
                              onChange={onChange}
                              inputProps={{ min: '0', step: '1' }}
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={FACULTY_NUMBER}
                              name="userSettings.facultyNumber"
                              value={user?.userSettings.facultyNumber ?? ''}
                              InputLabelProps={{ shrink: true }}
                              type="number"
                              onChange={onChange}
                              inputProps={{ min: '0', step: '1' }}
                            />
                        </FormControl>
                    </FormGroup>
                </Box>
                <FormActionButton
                  className={formStyles.buttonsWrapper}
                  buttonClassName={formStyles.button}
                  onClick={() => update(user)}
                  name={EDIT}
                />
            </form>
        </>
    );
};

export default UserForm;
