import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { TextField, Typography } from '@mui/material';

import {
    IExamGroupAdministration,
} from '../../../../common/types';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import {
    useAddBulkUsersInExamGroupByIdMutation,
    useGetExamGroupByIdQuery,
} from '../../../../redux/services/admin/examGroupsAdminService';
import isNilOrEmpty from '../../../../utils/check-utils';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import FormActionButton from '../../form-action-button/FormActionButton';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';

interface IAddUsersInExamGroupProps {
    examGroupId: number;
}

interface IAddUserInExamGroupUrlParams {
    examGroupId: number;
    userNames: string;
}

const AddBulkUsersInGroupModal = (props:IAddUsersInExamGroupProps) => {
    const { examGroupId } = props;

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ isValidForm, setIsValidForm ] = useState<boolean>(false);
    const [ addBulkUsersInGroupUrlParams, setAddBulkUsersInGroupUrlParams ] = useState<IAddUserInExamGroupUrlParams>({
        examGroupId,
        userNames: '',
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

    const [
        addBulkUsersInExamGroup, {
            data: addingData,
            isSuccess: isSuccessfullyAdded,
            error: addingError,
            isLoading: isAdding,
        } ] = useAddBulkUsersInExamGroupByIdMutation();

    useSuccessMessageEffect({
        data: [
            { message: addingData, shouldGet: isSuccessfullyAdded },
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
        getAndSetExceptionMessage([ addingError ], setErrorMessages);
    }, [ addingError ]);

    const add = () => {
        if (isValidForm) {
            addBulkUsersInExamGroup(addBulkUsersInGroupUrlParams);
        }
    };

    const onChange = (e: any) => {
        const { value } = e.target;

        const usernames = value;
        setIsValidForm(!isNilOrEmpty(usernames));
        setAddBulkUsersInGroupUrlParams((prevState) => ({
            ...prevState,
            examGroupId,
            userNames: usernames,
        }));
    };

    if (isFetching || isLoading || isAdding) {
        <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            {renderErrorMessagesAlert(errorMessages)}
            <Typography className={formStyles.centralize} variant="h4">
                Add multiple users in exam group
            </Typography>
            <form className={`${formStyles.form}`}>
                <TextField
                  className={formStyles.inputRow}
                  label="Exam group"
                  variant="standard"
                  name="examgroup"
                  value={examGroup.name}
                  disabled
                />
                <TextField
                  className={formStyles.inputRow}
                  label="Add users"
                  variant="filled"
                  name="usernames"
                  onChange={(e) => onChange(e)}
                  multiline
                  rows={15}
                  placeholder="Enter the usernames of the users you want to add, separated by comma, space or new line"
                />
                <FormActionButton
                  onClick={() => add()}
                  disabled={!isValidForm}
                  className={formStyles.buttonsWrapper}
                  buttonClassName={formStyles.button}
                  name="Add"
                />
                <Typography>
                    Users which are registered only in the
                    <Link to="https://dev.platform.softuni.bg/"> Softuni Platform</Link>
                    , will be added with delay.
                </Typography>
            </form>

        </>
    );
};

export default AddBulkUsersInGroupModal;
