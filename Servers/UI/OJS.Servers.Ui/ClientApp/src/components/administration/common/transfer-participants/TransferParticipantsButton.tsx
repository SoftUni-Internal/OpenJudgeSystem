import React, { useEffect, useState } from 'react';
import { BiTransfer } from 'react-icons/bi';
import { IconButton, Tooltip } from '@mui/material';

import { TRANSFER } from '../../../../common/labels';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import ConfirmDialog from '../../../guidelines/dialog/ConfirmDialog';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';

interface ITransferParticipantsButtonProps {
    onSuccess?: () => void;
    contestId: number;
    contestName: string;
    contestOfficialParticipants: number;
    categoryName?: string;
    style?: object;
    mutation: any;
}

const TransferParticipantsButton = (props: ITransferParticipantsButtonProps) => {
    const { onSuccess, contestId, contestName, contestOfficialParticipants, categoryName, style, mutation } = props;

    const [ transfer, { data, isLoading, isSuccess, isFetching, error, reset } ] = mutation();

    const [ showConfirmTransfer, setShowConfirmTransfer ] = useState<boolean>(false);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);

    const confirmTransfer = () => {
        setShowConfirmTransfer(!showConfirmTransfer);
    };

    useSuccessMessageEffect({
        data: [
            { message: data as string, shouldGet: isSuccess },
        ],
        setSuccessMessage,
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
        isLoading || isFetching
            ? <SpinningLoader />
            : (
                <div style={style
                    ? { ...style }
                    : {}}
                >
                    {renderSuccessfullAlert(successMessage)}
                    {renderErrorMessagesAlert(errorMessages) }
                    <Tooltip title={TRANSFER}>
                        <IconButton onClick={confirmTransfer}>
                            <BiTransfer />
                        </IconButton>
                    </Tooltip>
                    { showConfirmTransfer && (
                        <ConfirmDialog
                          title="Transfer participants"
                          text={(
                              <span>
                                  {/* eslint-disable-next-line react/jsx-one-expression-per-line */}
                                  Are you sure you want to transfer <b>{contestOfficialParticipants}</b>
                                  {/* eslint-disable-next-line react/jsx-one-expression-per-line */}
                                  {' '}participants from <b>Compete</b>{' '}
                                  {/* eslint-disable-next-line react/jsx-one-expression-per-line */}
                                  to <b>Practice</b> for the contest <b>{contestName}</b> from the category <b>{categoryName}</b>?
                              </span>
                            )}
                          confirmButtonText="Transfer"
                          declineButtonText="Cancel"
                          onClose={() => setShowConfirmTransfer(!showConfirmTransfer)}
                          confirmFunction={() => transfer(contestId)}
                        />
                    )}
                </div>
            )
    );
};

export default TransferParticipantsButton;
