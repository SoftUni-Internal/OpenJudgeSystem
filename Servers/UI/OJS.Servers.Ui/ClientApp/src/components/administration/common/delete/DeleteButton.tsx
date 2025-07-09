
import React, { useEffect, useState } from 'react';
import { IoMdTrash } from 'react-icons/io';
import { IconButton, Tooltip } from '@mui/material';

import { DELETE } from '../../../../common/labels';
import { IMappingEntityId } from '../../../../common/types';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert } from '../../../../utils/render-utils';
import ConfirmDialog from '../../../guidelines/dialog/ConfirmDialog';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';

interface IDeleteButtonProps {
    setParentSuccessMessage?: Function;
    onSuccess?: () => void;
    id: number | string | IMappingEntityId;
    name: string;
    style?: object;
    text: string;
    mutation: any;
}

const DeleteButton = (props: IDeleteButtonProps) => {
    const {
        setParentSuccessMessage,
        onSuccess,
        id,
        name,
        style = {},
        text,
        mutation,
    } = props;
    const [ showConfirmDelete, setShowConfirmDelete ] = useState<boolean>(false);
    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);

    const [ deleteRequest, { data, isLoading, isSuccess, error, reset } ] = mutation();
    const confirmDelete = () => {
        setShowConfirmDelete(!showConfirmDelete);
    };

    useSuccessMessageEffect({
        data: [
            { message: data as string, shouldGet: isSuccess },
        ],
        setParentSuccessMessage,
        clearFlags: [ isLoading ],
    });

    useEffect(() => {
        getAndSetExceptionMessage([ error ], setErrorMessages);
    }, [ error ]);

    useEffect(() => {
        if (data && onSuccess) {
            reset();
            onSuccess();
        }
    }, [ data, onSuccess, reset ]);

    return (
        isLoading
            ? <SpinningLoader />
            : <div style={style
                ? { ...style }
                : {}}
                >
                {renderErrorMessagesAlert(errorMessages) }
                <Tooltip title={DELETE}>
                    <IconButton onClick={confirmDelete}>
                        <IoMdTrash color="red" />
                    </IconButton>
                </Tooltip>
                { showConfirmDelete &&
                <ConfirmDialog
                          title={`Delete: ${name}`}
                          text={text}
                          confirmButtonText="Delete"
                          declineButtonText="Cancel"
                          onClose={() => setShowConfirmDelete(!showConfirmDelete)}
                          confirmFunction={() => deleteRequest(id)}
                        />
                    }
            </div>

    );
};

export default DeleteButton;
