
import React, { useEffect, useState } from 'react';
import { IconButton } from '@mui/material';
import Tooltip from '@mui/material/Tooltip';
import { UseQuery } from '@reduxjs/toolkit/dist/query/react/buildHooks';
import { QueryDefinition } from '@reduxjs/toolkit/query';

import downloadFile from '../../../../utils/file-download-utils';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert } from '../../../../utils/render-utils';
import IconSize from '../../../guidelines/icons/common/icon-sizes';
import DownloadIcon from '../../../guidelines/icons/DownloadIcon';

interface IDownloadIconButtonProps {
    mutation: UseQuery<QueryDefinition<number, any, any, { blob: Blob; filename: string }, any>>;
    onSuccess?: Function;
    onError?: Function;
    args: number;
    disabled?: boolean;
}
const DownloadIconButton = (props: IDownloadIconButtonProps) => {
    const { mutation, onSuccess, onError, args, disabled } = props;
    const [ skip, setSkip ] = useState<boolean>(true);
    const { refetch, data, error, isSuccess } = mutation(args, { skip });
    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);

    useEffect(() => {
        if (isSuccess && !skip) {
            if (onSuccess) {
                onSuccess();
            }
            downloadFile(data.blob, data.filename);
        }
    }, [ data, isSuccess, onSuccess, skip ]);

    useEffect(() => {
        if (error) {
            getAndSetExceptionMessage([ error ], setErrorMessages);
            if (onError) {
                onError();
            }
        }
    }, [ error, onError ]);

    useEffect(() => {
        if (!skip) {
            refetch();
        }
    }, [ refetch, skip ]);

    const onClick = () => {
        if (skip) {
            setSkip(false);
        } else {
            refetch();
        }
    };

    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            <Tooltip title="Download" onClick={onClick}>
                <span>
                    <IconButton disabled={disabled}>
                        <DownloadIcon size={IconSize.Large} />
                    </IconButton>
                </span>
            </Tooltip>
        </>
    );
};

export default DownloadIconButton;
