import { useEffect, useState } from 'react';
import { Box, FormControl, FormGroup, FormLabel, TextareaAutosize, TextField, Typography } from '@mui/material';

import { CLASS_NAME, DESCRIPTION, DLL_FILE, ID, NAME, PARAMETER } from '../../../../common/labels';
import { ICheckerAdministrationModel } from '../../../../common/types';
import { useCreateCheckerMutation, useGetCheckerByIdQuery, useUpdateCheckerMutation } from '../../../../redux/services/admin/checkersAdminService';
import { getAndSetExceptionMessage, getAndSetSuccesfullMessages } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import AdministrationFormButtons from '../../common/administration-form-buttons/AdministrationFormButtons';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';

interface ICheckerFormProps {
    isEditMode?: boolean;
    id?: number | null;
}

const CheckerForm = (props: ICheckerFormProps) => {
    const { isEditMode = true, id = null } = props;
    const [ exceptionMessages, setExceptionMessages ] = useState<Array<string>>([]);
    const [ successfullMessage, setSuccessfullMessage ] = useState<string | null>(null);
    const [ checker, setChecker ] = useState<ICheckerAdministrationModel>({
        id: 0,
        className: null,
        description: null,
        dllFile: null,
        name: '',
        parameter: null,
    });

    const {
        data: checkerData,
        isLoading: isGettingChecker,
        error: checkerError,
    } = useGetCheckerByIdQuery(id!, { skip: !isEditMode });

    const [
        createChecker,
        {
            data: createData,
            isLoading: isCreating,
            isSuccess: isSuccessfullyCreated,
            error: createError,
        },
    ] = useCreateCheckerMutation();

    const [
        updateChecker,
        {
            data: updateData,
            isLoading: isUpdating,
            isSuccess: isSuccessfullyUpdated,
            error: updateError,
        },
    ] = useUpdateCheckerMutation();

    useEffect(() => {
        if (checkerData) {
            setChecker(checkerData);
        }
    }, [ checkerData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ checkerError, createError, updateError ], setExceptionMessages);
    }, [ checkerError, createError, updateError ]);

    useEffect(() => {
        const message = getAndSetSuccesfullMessages([
            { message: createData, shouldGet: isSuccessfullyCreated },
            { message: updateData, shouldGet: isSuccessfullyUpdated },
        ]);
        setSuccessfullMessage(message);
    }, [ createData, isSuccessfullyCreated, isSuccessfullyUpdated, updateData ]);

    const onChange = (e: any) => {
        const { target } = e;
        const { name, value } = target;
        setChecker((prevState) => ({
            ...prevState,
            [name]: value === ''
                ? null
                : value,
        }));
    };

    if (isGettingChecker || isCreating || isUpdating) {
        <SpinningLoader />;
    }
    return (
        <>
            {renderSuccessfullAlert(successfullMessage)}
            {renderErrorMessagesAlert(exceptionMessages)}
            <Typography className={formStyles.centralize} variant="h4">Checker administration form</Typography>
            <form className={formStyles.form}>
                <Box className={formStyles.inputRow}>
                    <FormGroup className={formStyles.inputRow}>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={ID}
                              value={checker?.id}
                              InputLabelProps={{ shrink: true }}
                              type="text"
                              disabled
                            />
                        </FormControl>
                    </FormGroup>
                    <FormGroup className={formStyles.inputRow}>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={NAME}
                              value={checker?.name}
                              InputLabelProps={{ shrink: true }}
                              type="text"
                              name="name"
                              onChange={(e) => onChange(e)}
                            />
                        </FormControl>
                    </FormGroup>
                    <FormGroup className={formStyles.inputRow}>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={DLL_FILE}
                              value={checker?.dllFile}
                              InputLabelProps={{ shrink: true }}
                              name="dllFile"
                              type="text"
                              onChange={(e) => onChange(e)}
                            />
                        </FormControl>
                    </FormGroup>
                    <FormGroup className={formStyles.inputRow}>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={CLASS_NAME}
                              value={checker?.className}
                              InputLabelProps={{ shrink: true }}
                              name="className"
                              type="text"
                              onChange={(e) => onChange(e)}
                            />
                        </FormControl>
                    </FormGroup>
                    <FormGroup className={formStyles.inputRow}>
                        <FormControl className={formStyles.spacing}>
                            <FormLabel>{DESCRIPTION}</FormLabel>
                            <TextareaAutosize
                              minRows={15}
                              maxRows={15}
                              value={checker.description ?? undefined}
                              name="description"
                              onChange={(e) => onChange(e)}
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <FormLabel>{PARAMETER}</FormLabel>
                            <TextareaAutosize
                              minRows={15}
                              maxRows={15}
                              value={checker.parameter ?? undefined}
                              name="parameter"
                              onChange={(e) => onChange(e)}
                            />
                        </FormControl>
                    </FormGroup>
                </Box>
                <AdministrationFormButtons
                  isEditMode={isEditMode}
                  onCreateClick={() => createChecker(checker)}
                  onEditClick={() => updateChecker(checker)}
                />
            </form>
        </>
    );
};
export default CheckerForm;
