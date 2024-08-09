/* eslint-disable @typescript-eslint/ban-types */
import { useEffect, useState } from 'react';
import { FaLongArrowAltRight } from 'react-icons/fa';
import { Autocomplete, Box, Button, debounce, MenuItem, Modal, TextField, Typography } from '@mui/material';

import { IContestAutocomplete } from '../../../../common/types';
import useDisableMouseWheelOnNumberInputs from '../../../../hooks/common/use-disable-mouse-wheel-on-number-inputs';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { useGetContestAutocompleteQuery } from '../../../../redux/services/admin/contestsAdminService';
import { useCopyAllMutation, useCopyMutation } from '../../../../redux/services/admin/problemsAdminService';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { modalStyles } from '../../../../utils/object-utils';
import { renderErrorMessagesAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';

 enum AllowedOperations {
    Copy = 'copy',
    CopyAll = 'copyAll',
}

interface ICopyModalProps{
    index :number;
    operation: AllowedOperations;
    sourceId : number;
    sourceName: string;
    problemToCopy?: number | null;
    setShowModal: Function;
    setParentSuccessMessage: Function;
}

const CopyModal = (props: ICopyModalProps) => {
    const { index, setShowModal, operation, sourceId, sourceName, problemToCopy = null, setParentSuccessMessage } = props;
    const [ contestToCopy, setContestToCopy ] = useState<IContestAutocomplete| null>(null);
    const [ contestSearchString, setContestSearchString ] = useState<string>('');
    const [ problemGroupId, setNewProblemGroup ] = useState<number | undefined>(undefined);
    const [ errorMessages, setErrorMessages ] = useState <Array<string>>([]);
    const [ contestAutocomplete, setContestsAutocomplete ] = useState<Array<IContestAutocomplete>>([]);

    const { data, isLoading } = useGetContestAutocompleteQuery(contestSearchString);

    const onSelect = (contest: IContestAutocomplete) => {
        setContestToCopy(contest);
    };

    const [ copy,
        {
            data: copyData,
            isSuccess: isSuccesfullyCoppied,
            isLoading: isCoppying,
            error: copyError,
        } ] = useCopyMutation();

    const [ copyAll,
        {
            data: copyAllData,
            isSuccess: isSuccessfullyCopiedAll,
            isLoading: isCopyingAll,
            error: copyAllError,
        } ] = useCopyAllMutation();

    useDisableMouseWheelOnNumberInputs();

    useSuccessMessageEffect({
        data: [
            { message: copyData, shouldGet: isSuccesfullyCoppied },
            { message: copyAllData, shouldGet: isSuccessfullyCopiedAll },
        ],
        setParentSuccessMessage,
    });

    useEffect(() => {
        if (data) {
            setContestsAutocomplete(data);
        }
    }, [ data ]);

    useEffect(() => {
        getAndSetExceptionMessage([ copyError, copyAllError ], setErrorMessages);
    }, [ copyError, copyAllError ]);

    useEffect(() => {
        if (isSuccesfullyCoppied || isSuccessfullyCopiedAll) {
            setShowModal(false);
        }
    }, [ isSuccesfullyCoppied, isSuccessfullyCopiedAll, setShowModal ]);

    useEffect(() => {
        getAndSetExceptionMessage([ copyError ], setErrorMessages);
    }, [ copyError ]);

    const onInputChange = debounce((e: any) => {
        setContestSearchString(e.target.value);
    }, 300);

    const onSubmit = () => {
        if (operation === AllowedOperations.Copy) {
            copy({ destinationContestId: contestToCopy!.id, problemId: problemToCopy!, problemGroupId });
        } else {
            copyAll({ sourceContestId: sourceId, destinationContestId: contestToCopy!.id });
        }
        setContestSearchString('');
    };

    if (isCoppying || isCopyingAll) {
        return <SpinningLoader />;
    }

    return (
        <Modal
          key={index}
          open
          onClose={() => setShowModal(false)}
        >
            <Box sx={modalStyles}>
                {isLoading
                    ? <SpinningLoader />
                    : (
                        <>
                            {renderErrorMessagesAlert(errorMessages)}
                            <Typography variant="h5" padding="0.5rem">Copy Problems</Typography>
                            <Autocomplete
                              options={contestAutocomplete}
                              renderInput={(params) => <TextField {...params} label="Select Contest" key={params.id} />}
                              onChange={(event, newValue) => onSelect(newValue!)}
                              onInputChange={(event) => onInputChange(event)}
                              value={null}
                              isOptionEqualToValue={(option, value) => option.id === value.id}
                              getOptionLabel={(option) => option?.name}
                              renderOption={(properties, option) => (
                                  <MenuItem {...properties} key={option.id} value={option.id}>
                                      {option.name}
                                  </MenuItem>
                              )}
                            />
                            {
                          operation === AllowedOperations.Copy && (
                          <TextField
                            sx={{ mt: 2 }}
                            label="Copy To new Problem Group"
                            type="number"
                            onChange={(e) => setNewProblemGroup(Number(e.target.value))}
                          />
                          )
}
                            {contestToCopy !== null && (
                            <Box sx={{ padding: '4rem' }}>
                                <Typography sx={{ display: 'flex', justifyContent: 'space-around' }}>
                                    {sourceName}
                                    {' '}
                                    <FaLongArrowAltRight />
                                    {contestToCopy?.name}
                                </Typography>
                            </Box>
                            )}
                            <Box sx={{ display: 'flex', justifyContent: 'flex-end', marginTop: '1rem' }}>
                                <Button variant="contained" disabled={contestToCopy === null} onClick={() => onSubmit()}>Copy</Button>
                            </Box>
                        </>
                    )}
            </Box>
        </Modal>
    );
};

export
{
    AllowedOperations,
};

export default CopyModal;
