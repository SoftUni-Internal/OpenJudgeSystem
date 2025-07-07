import { useEffect, useState } from 'react';
import { FormControl, TextField, Typography } from '@mui/material';

import { NAME } from '../../../../common/labels';
import { IRoleAdministrationModel } from '../../../../common/types';
import useDelayedSuccessEffect from '../../../../hooks/common/use-delayed-success-effect';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { useCreateRoleMutation, useGetRoleByIdQuery, useUpdateRoleMutation } from '../../../../redux/services/admin/rolesAdminService';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import clearSuccessMessages from '../../../../utils/success-messages-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import AdministrationFormButtons from '../../common/administration-form-buttons/AdministrationFormButtons';

import formStyles from '../../common/styles/FormStyles.module.scss';

interface IRoleFormProps {
    id?: string | null;
    isEditMode?: boolean;
    getRoleName?: Function;
    onSuccess?: Function;
    setParentSuccessMessage?: Function;
}

const RoleForm = (props: IRoleFormProps) => {
    const { id, isEditMode = true, getRoleName, onSuccess, setParentSuccessMessage } = props;
    const [ exceptionMessages, setExceptionMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);

    const [ role, setRole ] = useState<IRoleAdministrationModel>({
        id: null,
        name: '',
    });

    const {
        data: roleData,
        error: getError,
        isLoading: isGettingRole,
    } = useGetRoleByIdQuery(id!, { skip: !isEditMode });

    const [
        create,
        {
            data: createData,
            isSuccess: isSuccessfullyCreated,
            error: createError,
            isLoading: isCreating,
        },
    ] = useCreateRoleMutation();

    const [
        update,
        {
            data: updateData,
            isSuccess: isSuccessfullyUpdated,
            error: updateError,
            isLoading: isUpdating,
        },
    ] = useUpdateRoleMutation();

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
        if (roleData) {
            setRole(roleData);
            if (getRoleName) {
                getRoleName(roleData.name);
            }
        }
    }, [ getRoleName, roleData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ getError, updateError, createError ], setExceptionMessages);
        clearSuccessMessages({ setSuccessMessage, setParentSuccessMessage });
    }, [ getError, updateError, createError, setParentSuccessMessage ]);

    const onChange = (e: any) => {
        const { target } = e;
        const { name, value } = target;
        setRole((prevState) => ({
            ...prevState,
            [name]: value,
        }));
    };

    if (isGettingRole || isCreating || isUpdating) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            {renderErrorMessagesAlert(exceptionMessages)}
            <Typography className={formStyles.centralize} variant="h4">Role administration form</Typography>
            <form className={formStyles.form}>
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      label={NAME}
                      value={role?.name ?? ''}
                      InputLabelProps={{ shrink: true }}
                      type="text"
                      name="name"
                      onChange={(e) => onChange(e)}
                    />
                </FormControl>
                <AdministrationFormButtons
                  isEditMode={isEditMode}
                  onCreateClick={() => create(role)}
                  onEditClick={() => update(role)}
                />
            </form>
        </>
    );
};
export default RoleForm;
