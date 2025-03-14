/* eslint-disable @typescript-eslint/ban-types */
import React, { useCallback, useEffect, useState } from 'react';
import { FaLongArrowAltRight } from 'react-icons/fa';
import {
    Autocomplete,
    Box,
    Button, Checkbox,
    createFilterOptions,
    debounce, FormControlLabel,
    MenuItem,
    Modal,
    TextField,
    Typography,
} from '@mui/material';
import isNil from 'lodash/isNil';
import { COPY_INTO_NEW_PROBLEM_GROUP } from 'src/common/labels';
import isNilOrEmpty from 'src/utils/check-utils';

import { IContestAutocomplete, IProblemGroupDropdownModel } from '../../../../common/types';
import useDisableMouseWheelOnNumberInputs from '../../../../hooks/common/use-disable-mouse-wheel-on-number-inputs';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { useGetContestAutocompleteQuery } from '../../../../redux/services/admin/contestsAdminService';
import { useLazyGetIdsByContestIdQuery } from '../../../../redux/services/admin/problemGroupsAdminService';
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
    onClose?: () => {};
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
        onClose,
    } = props;

    const [ contestToCopy, setContestToCopy ] = useState<IContestAutocomplete | null>(null);
    const [ contestSearchString, setContestSearchString ] = useState<string>('');
    const [ problemGroupId, setNewProblemGroup ] = useState<number | null>(null);
    const [ errorMessages, setErrorMessages ] = useState <Array<string>>([]);
    const [ contestAutocomplete, setContestsAutocomplete ] = useState<Array<IContestAutocomplete>>([]);
    const [ copyIntoNewProblemGroup, setCopyIntoNewProblemGroup ] = useState<boolean>(false);
    const [ isFormValid, setIsFormValid ] = useState<boolean>(false);

    const { data, isLoading } = useGetContestAutocompleteQuery(contestSearchString);

    const [ getProblemGroups, {
        data: problemGroupsData,
        isLoading: problemGroupsAreLoading,
    } ] = useLazyGetIdsByContestIdQuery();

    const onSelectContest = (contest: IContestAutocomplete) => {
        setContestToCopy(contest);
        setNewProblemGroup(null);

        if (!isNil(contest)) {
            getProblemGroups(contest.id);
        }
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

    useEffect(() => {
        // Reset problem group when checkbox is checked
        if (copyIntoNewProblemGroup) {
            setNewProblemGroup(null);
        }
    }, [ copyIntoNewProblemGroup, setNewProblemGroup ]);

    useEffect(() => {
        if (operation === AllowedOperations.CopyAll && !isNil(contestToCopy)) {
            setIsFormValid(true);
            return;
        }

        if (!isNil(contestToCopy) &&
            (
                // Copy into existing problem group and problem group id is set
                (!copyIntoNewProblemGroup && !isNil(problemGroupId)) ||
                // Copy into new problem group and problem group id is reset to null
                (copyIntoNewProblemGroup && isNil(problemGroupId))
            ) &&
            !isNilOrEmpty(sourceContestName)
        ) {
            setIsFormValid(true);
            return;
        }

        setIsFormValid(false);
    }, [ contestToCopy, copyIntoNewProblemGroup, operation, problemGroupId, sourceContestName ]);

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

        if (onClose) {
            onClose();
        }
    }, [ isSuccessfullyCopied, isSuccessfullyCopiedAll, setShowModal, onClose ]);

    const onInputChange = debounce((e: any) => {
        if (e?.target) {
            setContestSearchString(e.target.value);
        }
    }, 300);

    const onSubmit = () => {
        if (operation === AllowedOperations.Copy) {
            copy({
                destinationContestId: contestToCopy!.id,
                problemId: problemToCopyId!,
                problemGroupId: copyIntoNewProblemGroup
                    ? null
                    : problemGroupId,
            });
        } else {
            copyAll({ sourceContestId, destinationContestId: contestToCopy!.id });
        }
        setContestSearchString('');
    };

    const getModalTitle = useCallback(() => {
        const text = 'Copy';

        if (operation === AllowedOperations.Copy) {
            return `${text} problem "${problemToCopyName}"`;
        }

        return `${text} all problems`;
    }, [ operation, problemToCopyName ]);

    if (isCopying || isCopyingAll) {
        return <SpinningLoader />;
    }

    const problemGroupsFormatFilterOptions = createFilterOptions({
        stringify: (option: IProblemGroupDropdownModel) => {
            const { id, orderBy } = option;

            return `${orderBy} ${id}`;
        },
    });

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
                                {getModalTitle()}
                            </Typography>
                            <Autocomplete<IContestAutocomplete>
                              sx={{ marginTop: '1rem' }}
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
                              operation === AllowedOperations.Copy && !copyIntoNewProblemGroup && (
                              <Autocomplete<IProblemGroupDropdownModel>
                                sx={{ marginTop: '1rem' }}
                                disabled={problemGroupsAreLoading || isNil(contestToCopy)}
                                options={problemGroupsData || []}
                                filterOptions={problemGroupsFormatFilterOptions}
                                renderInput={(params) => <TextField {...params} label="Select Problem Group" key={params.id} />}
                                onChange={(event, newValue) => {
                                    setNewProblemGroup(newValue
                                        ? Number(newValue.id)
                                        : null);
                                }}
                                value={problemGroupId !== null
                                    ? problemGroupsData?.find((pg) => pg.id === problemGroupId)
                                    : null}
                                isOptionEqualToValue={(option, value) => option.id === value.id}
                                getOptionLabel={(option) => option?.orderBy.toString()}
                                renderOption={(properties, option) => (
                                    <MenuItem {...properties} key={option.id} value={option.id}>
                                        {option.orderBy}
                                    </MenuItem>
                                )}
                              />
                              )
                            }
                            {
                                operation === AllowedOperations.Copy && (
                                    <FormControlLabel
                                      control={(
                                          <Checkbox
                                            checked={copyIntoNewProblemGroup}
                                          />
                                        )}
                                      name="copyIntoNewProblemGroup"
                                      onChange={() => setCopyIntoNewProblemGroup(!copyIntoNewProblemGroup)}
                                      label={COPY_INTO_NEW_PROBLEM_GROUP}
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
                                  disabled={!isFormValid}
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
