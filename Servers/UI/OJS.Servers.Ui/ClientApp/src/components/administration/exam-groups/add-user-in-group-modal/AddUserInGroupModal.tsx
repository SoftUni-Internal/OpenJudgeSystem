/* eslint-disable @typescript-eslint/ban-types */
/* eslint-disable react/jsx-props-no-spreading */
import React, { useEffect, useState } from 'react';
import { Autocomplete, FormControl, MenuItem, TextField, Typography } from '@mui/material';
import debounce from 'lodash/debounce';

import {
    IExamGroupAdministration,
    IUserAutocomplete,
} from '../../../../common/types';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import {
    useAddUserInExamGroupByIdMutation,
    useGetExamGroupByIdQuery,
} from '../../../../redux/services/admin/examGroupsAdminService';
import { useGetUsersAutocompleteQuery } from '../../../../redux/services/admin/usersAdminService';
import isNilOrEmpty from '../../../../utils/check-utils';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import FormActionButton from '../../form-action-button/FormActionButton';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';

interface IAddUserInExamGroupProps {
    examGroupId: number;
}

interface IAddUserInExamGroupUrlParams {
    examGroupId: number;
    userId: string;
}

const AddUserInGroupModal = (props:IAddUserInExamGroupProps) => {
    const { examGroupId } = props;

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ isValidForm, setIsValidForm ] = useState<boolean>(false);
    const [ userId, setUserId ] = useState<string>('');
    const [ userSearchString, setUserSearchString ] = useState<string>('');
    const [ addUserInGroupUrlParams, setAddUserInGroupUrlParams ] = useState<IAddUserInExamGroupUrlParams>({
        examGroupId,
        userId,
    });
    const [ examGroup, setExamGroup ] = useState<IExamGroupAdministration>({
        id: 0,
        name: '',
        contestName: '',
        contestId: 0,
        externalAppId: '',
        externalExamGroupId: 0,
    });

    const { data, isFetching, isLoading } = useGetExamGroupByIdQuery({ id: Number(examGroupId) });
    const { data: usersForDropdown } = useGetUsersAutocompleteQuery(userSearchString);

    const [
        addUserToExamGroup, {
            data: createData,
            isSuccess: isSuccessfullyCreated,
            error: createError,
            isLoading: isCreating,
        } ] = useAddUserInExamGroupByIdMutation();

    useSuccessMessageEffect({
        data: [
            { message: createData, shouldGet: isSuccessfullyCreated },
        ],
        setSuccessMessage,
    });

    useEffect(
        () => {
            if (data) {
                setExamGroup(data);
            }
        },
        [ data ],
    );

    useEffect(() => {
        getAndSetExceptionMessage([ createError ], setErrorMessages);
    }, [ createError ]);

    const handleAutocompleteChange = (user: IUserAutocomplete) => {
        const selectedUser = usersForDropdown?.find((u) => u.id === user.id);
        if (selectedUser) {
            setIsValidForm(true);
            setUserId(user.id);
            setAddUserInGroupUrlParams({ ...addUserInGroupUrlParams, userId: selectedUser.id });
        }
        setIsValidForm(false);
    };

    const handleOnInputChange = debounce((e: any) => {
        setUserSearchString(e.target.value);
    }, 300);

    const create = () => {
        if (isValidForm || userId !== null) {
            addUserToExamGroup(addUserInGroupUrlParams);
        }
    };

    if (isFetching || isLoading || isCreating) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            {renderSuccessfullAlert(successMessage)}
            <Typography className={formStyles.centralize} variant="h4">
                Add new user in exam group
            </Typography>
            <form className={`${formStyles.form}`}>
                <TextField
                  className={formStyles.inputRow}
                  label="Exam group"
                  variant="standard"
                  name="examgroup"
                  value={examGroup.name}
                />
                <FormControl className={formStyles.inputRow} sx={{ margin: '20px 0' }}>
                    <Autocomplete
                      sx={{ width: '100%' }}
                      className={formStyles.inputRow}
                      options={usersForDropdown!}
                      renderInput={(params) => <TextField {...params} label="User" key={params.id} />}
                      onChange={(event, newValue) => handleAutocompleteChange(newValue!)}
                      onInputChange={(event) => handleOnInputChange(event)}
                      value={null}
                      isOptionEqualToValue={(option, value) => option.id === value.id}
                      getOptionLabel={(option) => option?.userName}
                      renderOption={(properties, option) => (
                          <MenuItem {...properties} key={option.id} value={option.id}>
                              {option.userName}
                          </MenuItem>
                      )}
                    />
                </FormControl>
                <FormActionButton
                  className={formStyles.buttonsWrapper}
                  buttonClassName={formStyles.button}
                  onClick={() => create()}
                  disabled={isNilOrEmpty(addUserInGroupUrlParams.userId)}
                  name="Add"
                />
            </form>
        </>
    );
};

export default AddUserInGroupModal;
