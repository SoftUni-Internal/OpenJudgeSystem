/* eslint-disable @typescript-eslint/ban-types */
import { useEffect, useState } from 'react';
import { IoMdTrash } from 'react-icons/io';
import { IconButton, Tooltip } from '@mui/material';

import { DELETE } from '../../../../common/labels';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import ConfirmDialog from '../../../guidelines/dialog/ConfirmDialog';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';

interface IDeleteUserFromGroupButtonProps {
    onSuccess?: Function;
    id: string;
    examGroupId: number;
    name: string;
    style?: object;
    text: string;
    mutation: any;
}

const DeleteUserFromGroupButton = (props: IDeleteUserFromGroupButtonProps) => {
    const { onSuccess, id, examGroupId, name, style = {}, text, mutation } = props;
    const [ showConfirmDelete, setShowConfirmDelete ] = useState<boolean>(false);
    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ deleteRequest, { data, isLoading, isSuccess, error } ] = mutation();

    const confirmDelete = () => {
        setShowConfirmDelete(!showConfirmDelete);
    };

    useSuccessMessageEffect({
        data: [
            { message: data, shouldGet: isSuccess },
        ],
        setSuccessMessage,
    });

    useEffect(() => {
        getAndSetExceptionMessage([ error ], setErrorMessages);
    }, [ error ]);

    useEffect(() => {
        if (isSuccess && onSuccess) {
            onSuccess();
        }
    }, [ isSuccess, onSuccess ]);

    if (isLoading) {
        return <SpinningLoader />;
    }
    return (
        <div style={style
            ? { ...style }
            : {}}
        >
            {renderSuccessfullAlert(successMessage)}
            {renderErrorMessagesAlert(errorMessages)}
            <Tooltip title={DELETE}>
                <IconButton onClick={confirmDelete}>
                    <IoMdTrash color="red" />
                </IconButton>
            </Tooltip>
            {showConfirmDelete && (
            <ConfirmDialog
              title={`Delete: ${name}`}
              text={text}
              confirmButtonText="Delete"
              declineButtonText="Cancel"
              onClose={() => setShowConfirmDelete(!showConfirmDelete)}
              confirmFunction={() => deleteRequest({ examGroupId, userId: id })}
            />
            )}
        </div>
    );
};

export default DeleteUserFromGroupButton;
