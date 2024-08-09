/* eslint-disable @typescript-eslint/ban-types */
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Box, FormControl, FormControlLabel, FormLabel, InputLabel, MenuItem, Radio, RadioGroup, Select, TextField, Typography } from '@mui/material';
import { DateTimePicker } from '@mui/x-date-pickers';
import isNaN from 'lodash/isNaN';

import { SettingTypeEnums } from '../../../../common/enums';
import { CREATE, EDIT, ID, NAME, RECORD, TYPE, VALUE } from '../../../../common/labels';
import { DELETE_CONFIRMATION_MESSAGE } from '../../../../common/messages';
import { ISettingAdministrationModel } from '../../../../common/types';
import { NEW_ADMINISTRATION_PATH, SETTINGS_PATH } from '../../../../common/urls/administration-urls';
import useDelayedSuccessEffect from '../../../../hooks/common/use-delayed-success-effect';
import useDisableMouseWheelOnNumberInputs from '../../../../hooks/common/use-disable-mouse-wheel-on-number-inputs';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { useCreateSettingMutation, useDeleteSettingMutation, useGetSettingByIdQuery, useUpdateSettingMutation } from '../../../../redux/services/admin/settingsAdminService';
import { getDateAsLocal } from '../../../../utils/administration/administration-dates';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import { getEnumMemberName } from '../../../../utils/string-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import DeleteButton from '../../common/delete/DeleteButton';
import FormActionButton from '../../form-action-button/FormActionButton';
import { handleDateTimePickerChange } from '../../utils/mui-utils';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';

interface ISettingFormProps {
    isEditMode: boolean;
    id?: number;
    onSuccess?: Function;
    setParentSuccessMessage?: Function;
}

const SettingForm = (props: ISettingFormProps) => {
    const { id = 0, isEditMode = true, onSuccess, setParentSuccessMessage } = props;

    const navigate = useNavigate();

    const [ exceptionMessages, setExceptionMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);

    const [ setting, setSetting ] = useState<ISettingAdministrationModel>({
        id,
        name: '',
        value: '',
        type: getEnumMemberName(SettingTypeEnums, SettingTypeEnums.Numeric).toString(),
    });

    const {
        data: settingData,
        error: getError,
        isLoading: isGettingSetting,
    } = useGetSettingByIdQuery(id!, { skip: !isEditMode });

    const [
        create,
        {
            data: createData,
            isSuccess: isSuccessfullyCreated,
            error: createError,
            isLoading: isCreating,
        },
    ] = useCreateSettingMutation();

    const [
        update,
        {
            data: updateData,
            isSuccess: isSuccessfullyUpdated,
            error: updateError,
            isLoading: isUpdating,
        },
    ] = useUpdateSettingMutation();

    useDisableMouseWheelOnNumberInputs();

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
        if (settingData) {
            setSetting(settingData);
        }
    }, [ settingData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ getError, updateError, createError ], setExceptionMessages);
    }, [ getError, updateError, createError ]);

    const onChange = (e: any) => {
        const { target } = e;
        const { name, value } = target;
        if (name === 'type') {
            const newValue = value === getEnumMemberName(SettingTypeEnums, SettingTypeEnums.Boolean).toString()
                ? 'False'
                : '';
            setSetting((prevState) => ({
                ...prevState,
                type: value,
                value: newValue,
            }));
        }

        setSetting((prevState) => ({
            ...prevState,
            [name]: value,
        }));
    };

    const renderFormSubmitButtons = () => (
        isEditMode
            ? (
                <>
                    <FormActionButton
                      className={formStyles.buttonsWrapper}
                      buttonClassName={formStyles.button}
                      onClick={() => update(setting)}
                      name={EDIT}
                    />
                    <Box sx={{ alignSelf: 'flex-end' }}>
                        <DeleteButton
                          id={Number(id!)}
                          name={RECORD}
                          onSuccess={() => navigate(`/${NEW_ADMINISTRATION_PATH}/${SETTINGS_PATH}`)}
                          mutation={useDeleteSettingMutation}
                          text={DELETE_CONFIRMATION_MESSAGE}
                        />
                    </Box>
                </>
            )
            : (
                <FormActionButton
                  className={formStyles.buttonsWrapper}
                  buttonClassName={formStyles.button}
                  onClick={() => create(setting)}
                  name={CREATE}
                />
            )
    );

    const renderValueField = () => {
        // eslint-disable-next-line default-case
        switch (setting.type) {
        case getEnumMemberName(SettingTypeEnums, SettingTypeEnums.Numeric).toString():
            return (
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      label={VALUE}
                      value={setting?.value}
                      InputLabelProps={{ shrink: true }}
                      type="number"
                      name="value"
                      onChange={(e) => onChange(e)}
                    />
                </FormControl>
            );
        case getEnumMemberName(SettingTypeEnums, SettingTypeEnums.ShortString).toString():
            return (
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      label={VALUE}
                      value={setting?.value}
                      InputLabelProps={{ shrink: true }}
                      type="text"
                      name="value"
                      onChange={(e) => onChange(e)}
                    />
                </FormControl>
            );
        case getEnumMemberName(SettingTypeEnums, SettingTypeEnums.LongString).toString():
            return (
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      label={VALUE}
                      value={setting?.value}
                      variant="filled"
                      name="value"
                      onChange={(e) => onChange(e)}
                      multiline
                      rows={15}
                    />
                </FormControl>
            );
        case getEnumMemberName(SettingTypeEnums, SettingTypeEnums.DateTime).toString():
            return (
                <FormControl className={formStyles.inputRow}>
                    <DateTimePicker
                      name="value"
                      label={VALUE}
                      value={getDateAsLocal(setting.value)}
                      onChange={(newValue) => handleDateTimePickerChange('value', newValue, onChange)}
                    />
                </FormControl>
            );
        case getEnumMemberName(SettingTypeEnums, SettingTypeEnums.Boolean).toString():
            return (
                <FormControl className={formStyles.inputRow}>
                    <FormLabel id="type">Choose variant</FormLabel>
                    <RadioGroup
                      aria-labelledby="type"
                      value={setting.value === 'True'
                          ? 'True'
                          : 'False'}
                      onChange={onChange}
                      name="value"
                    >
                        <FormControlLabel value="True" control={<Radio />} label="True" />
                        <FormControlLabel value="False" control={<Radio />} label="False" />
                    </RadioGroup>
                </FormControl>
            );
        }
        return (
            <div />
        );
    };

    if (isGettingSetting || isCreating || isUpdating) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            {renderErrorMessagesAlert(exceptionMessages)}
            <Typography className={formStyles.centralize} variant="h4">Setting administration form</Typography>
            <form className={formStyles.form}>
                {isEditMode && (
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      label={ID}
                      value={setting?.id ?? ''}
                      InputLabelProps={{ shrink: true }}
                      type="text"
                      disabled
                    />
                </FormControl>
                )}

                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      label={NAME}
                      value={setting?.name ?? ''}
                      InputLabelProps={{ shrink: true }}
                      type="text"
                      name="name"
                      onChange={(e) => onChange(e)}
                    />
                </FormControl>
                <FormControl
                  className={formStyles.inputRow}
                >
                    <InputLabel id="setting-type">{TYPE}</InputLabel>
                    <Select
                      sx={{ width: '100%' }}
                      variant="standard"
                      value={setting.type}
                      className={formStyles.inputRow}
                      name="type"
                      labelId="setting-type"
                      onChange={(e) => onChange(e)}
                      onBlur={(e) => onChange(e)}
                    >
                        {Object.keys(SettingTypeEnums).filter((key) => isNaN(Number(key))).map((key) => (
                            <MenuItem key={key} value={key} disabled={setting.type === key}>
                                {key}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
                {renderValueField()}
                {renderFormSubmitButtons()}
            </form>
        </>
    );
};

export default SettingForm;
