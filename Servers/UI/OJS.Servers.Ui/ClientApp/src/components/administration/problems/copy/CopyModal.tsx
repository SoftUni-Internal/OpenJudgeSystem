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
import { autocompleteNameIdFormatFilterOptions } from '../../utils/mui-utils';

 enum AllowedOperations {
    Copy = 'copy',
    CopyAll = 'copyAll',
}

interface ICopyModalProps{
    index: number;
    operation: AllowedOperations;
    sourceContestId : number;
    sourceContestName: string;
    problemToCopyId?: number | null;
    problemToCopyName: string | null;
    setShowModal: Function;
    setParentSuccessMessage: Function;
}

const CopyModal = (props: ICopyModalProps) => {
    const {
        index,
        setShowModal,
        operation,
        sourceContestId,
        sourceContestName,
        problemToCopyName,
        problemToCopyId = null,
        setParentSuccessMessage,
    } = props;

    const [ contestToCopy, setContestToCopy ] = useState<IContestAutocomplete | null>(null);
    const [ contestSearchString, setContestSearchString ] = useState<string>('');
    const [ problemGroupId, setNewProblemGroup ] = useState<number | undefined>(undefined);
    const [ errorMessages, setErrorMessages ] = useState <Array<string>>([]);
    const [ contestAutocomplete, setContestsAutocomplete ] = useState<Array<IContestAutocomplete>>([]);

    const { data, isLoading } = useGetContestAutocompleteQuery(contestSearchString);

    const onSelectContest = (contest: IContestAutocomplete) => {
        setContestToCopy(contest);
    };

    const [ copy,
        {
            data: copyData,
            isSuccess: isSuccessfullyCopied,
            isLoading: isCopying,
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
            { message: copyData, shouldGet: isSuccessfullyCopied },
            { message: copyAllData, shouldGet: isSuccessfullyCopiedAll },
        ],
        setParentSuccessMessage,
        clearFlags: [ isCopying, isCopyingAll ],
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
        if (isSuccessfullyCopied || isSuccessfullyCopiedAll) {
            setShowModal(false);
        }
    }, [ isSuccessfullyCopied, isSuccessfullyCopiedAll, setShowModal ]);

    const onInputChange = debounce((e: any) => {
        setContestSearchString(e.target.value);
    }, 300);

    const onSubmit = () => {
        if (operation === AllowedOperations.Copy) {
            copy({ destinationContestId: contestToCopy!.id, problemId: problemToCopyId!, problemGroupId });
        } else {
            copyAll({ sourceContestId, destinationContestId: contestToCopy!.id });
        }
        setContestSearchString('');
    };

    if (isCopying || isCopyingAll) {
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
                            <Typography variant="h5" padding="0.5rem">
                                Copy
                                {' '}
                                {problemToCopyName}
                            </Typography>
                            <Autocomplete<IContestAutocomplete>
                              disabled={sourceContestName === ''}
                              options={contestAutocomplete}
                              filterOptions={autocompleteNameIdFormatFilterOptions}
                              renderInput={(params) => <TextField {...params} label="Select Contest" key={params.id} />}
                              onChange={(event, newValue) => onSelectContest(newValue!)}
                              onInputChange={(event) => onInputChange(event)}
                              value={contestToCopy !== null
                                  ? contestToCopy
                                  : null}
                              isOptionEqualToValue={(option, value) => option.id === value.id}
                              getOptionLabel={(option) => option?.name}
                              renderOption={(properties, option) => (
                                  <MenuItem {...properties} key={option.id} value={option.id}>
                                      #
                                      {option.id}
                                      {' '}
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
                            <Box sx={{ marginTop: '1rem' }}>
                                <Typography sx={{ display: 'flex', justifyContent: 'space-around' }}>
                                    {sourceContestName}
                                    {' '}
                                    <FaLongArrowAltRight />
                                    {contestToCopy !== null
                                        ? contestToCopy?.name
                                        : 'Select contest'}
                                </Typography>
                            </Box>
                            <Box sx={{ display: 'flex', justifyContent: 'flex-end', marginTop: '1rem' }}>
                                <Button
                                  variant="contained"
                                  disabled={contestToCopy === null || sourceContestName === ''}
                                  onClick={() => onSubmit()}
                                >
                                    Copy
                                </Button>
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
