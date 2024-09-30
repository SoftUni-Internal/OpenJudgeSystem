/* eslint-disable @typescript-eslint/ban-types */
import { ChangeEvent, useEffect, useState } from 'react';

import { CONTEST_IS_DELETED, CONTEST_IS_NOT_VISIBLE } from '../../../../common/messages';
import { IContestAutocomplete, IGetAllAdminParams } from '../../../../common/types';
import useDelayedSuccessEffect from '../../../../hooks/common/use-delayed-success-effect';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { applyDefaultFilterToQueryString } from '../../../../pages/administration-new/administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd } from '../../../../pages/administration-new/AdministrationGridView';
import lecturerInContestFilterableColumns, { returnLecturerInContestNonFilterableColumns } from '../../../../pages/administration-new/lecturers-in-contests/lecturersInContestsGridColumns';
import { useGetContestAutocompleteQuery } from '../../../../redux/services/admin/contestsAdminService';
import { useAddLecturerToContestMutation, useGetLecturerContestsQuery, useRemoveLecturerFromContestMutation } from '../../../../redux/services/admin/usersAdminService';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert } from '../../../../utils/render-utils';
import clearSuccessMessages from '../../../../utils/success-messages-utils';
import ConfirmDialog from '../../../guidelines/dialog/ConfirmDialog';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import CreateButton from '../../common/create/CreateButton';
import LecturerForm from '../../common/lecturer/lecturer-form/LecturerForm';
import AdministrationModal from '../../common/modals/administration-modal/AdministrationModal';
import { onAutocompleteInputChange } from '../../utils/mui-utils';

interface ILecturerInContestsProps {
    userId: string;
    setParentSuccessMessage: Function;
}
const LecturerInContests = (props: ILecturerInContestsProps) => {
    const defaultSorter = 'contestId=DESC';
    const { userId, setParentSuccessMessage } = props;

    const [ contestSearchString, setContestSearchString ] = useState<string>('');

    const [ contestId, setContestId ] = useState<number>(0);
    const [ showConfirmDialog, setShowConfirmDialog ] = useState<boolean>(false);
    const [ contestToAdd, setContestToAdd ] = useState<number>(0);

    const [ errorMessages, setErrorMessages ] = useState <Array<string>>([]);

    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(
        defaultFilterToAdd,
        defaultSorter,
    ));

    const [ contestAutocomplete, setContestsAutocomplete ] = useState<Array<IContestAutocomplete>>([
        {
            id: 0,
            name: '',
        },
    ]);

    const [ showCreateModal, setShowCreateModal ] = useState<boolean>(false);

    const {
        refetch,
        isFetching,
        data,
        isLoading,
        error,
    } = useGetLecturerContestsQuery({ userId, ...queryParams });

    const [
        addLecturerToContest,
        {
            data: addData,
            error: addError,
            isSuccess: isSuccessfullyAdded,
            isLoading: isAdding,
            reset: resetAdd,
        },
    ] = useAddLecturerToContestMutation();

    const {
        data: contestsAutocompleteData,
        error: getContestDataError,
    } = useGetContestAutocompleteQuery(contestSearchString, { skip: contestSearchString === undefined });

    const [
        removeLecturerFromContest,
        {
            data: removeData,
            error: removeError,
            isSuccess: isSuccessfullyRemoved,
            isLoading: isRemoving,
            reset: resetRemove,
        },
    ] = useRemoveLecturerFromContestMutation();

    useSuccessMessageEffect({
        data: [
            { message: addData, shouldGet: isSuccessfullyAdded },
            { message: removeData, shouldGet: isSuccessfullyRemoved },
        ],
        setParentSuccessMessage,
        clearFlags: [ isAdding, isRemoving ],
    });

    useDelayedSuccessEffect({
        isSuccess: isSuccessfullyAdded || isSuccessfullyRemoved,
        onSuccess: () => {
            setShowCreateModal(false);

            if (isSuccessfullyAdded) {
                resetAdd();
            }

            if (isSuccessfullyRemoved) {
                resetRemove();
            }
        },
    });

    useEffect(() => {
        if (isSuccessfullyRemoved || isSuccessfullyAdded) {
            refetch();
        },
    });

    useEffect(() => {
        if (contestsAutocompleteData) {
            setContestsAutocomplete(contestsAutocompleteData);
        }
    }, [ contestsAutocompleteData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ getContestDataError, addError, removeError ], setErrorMessages);
        clearSuccessMessages({ setParentSuccessMessage });
    }, [ addError, getContestDataError, removeError, setParentSuccessMessage ]);

    const onSelect = (contest: IContestAutocomplete) => {
        let currContestId = 0;
        if (contest) {
            currContestId = contest.id;
        }
        setContestToAdd(currContestId);
    };

    const onRemoveFromRowClicked = (uId: number) => {
        setContestId(uId);
        setShowConfirmDialog(true);
    };

    const renderConfirmDialog = (index: number) => (
        <ConfirmDialog
          key={index}
          text="Are you sure you want to remove lecturer from contest."
          title="Remove from role"
          declineButtonText="Close"
          confirmButtonText="Remove"
          declineFunction={() => setShowConfirmDialog(false)}
          confirmFunction={() => {
              removeLecturerFromContest({ lecturerId: userId, contestId });
              setShowConfirmDialog(false);
          }}
        />
    );

    const renderGridSettings = () => (
        <CreateButton
          showModal={showCreateModal}
          showModalFunc={setShowCreateModal}
          styles={{ width: '40px', height: '40px', color: 'rgb(25,118,210)' }}
          tooltipLabel="Add lecturer to contest"
        />
    );

    const renderCreateModal = (i: number) => (
        <AdministrationModal
          key={i}
          index={i}
          open={showCreateModal}
          onClose={() => {
              setShowCreateModal(false);
          }}
        >
            <LecturerForm
              data={contestAutocomplete}
              disabled={contestToAdd === 0}
              label="Select Contest"
              heading="Add lecturer to contest"
              onChange={onSelect}
              onInputChange={(e: ChangeEvent<HTMLInputElement>) => onAutocompleteInputChange(e, setContestSearchString)}
              onClick={() => {
                  addLecturerToContest({ lecturerId: userId, contestId: contestToAdd });
                  setContestToAdd(0);
              }}
            />
        </AdministrationModal>
    );

    if (isLoading || (isFetching && !showConfirmDialog)) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            <AdministrationGridView
              data={data}
              error={error}
              setQueryParams={setQueryParams}
              queryParams={queryParams}
              withSearchParams={false}
              filterableGridColumnDef={lecturerInContestFilterableColumns}
              renderActionButtons={renderGridSettings}
              notFilterableGridColumnDef={
            returnLecturerInContestNonFilterableColumns(onRemoveFromRowClicked)
        }
              specificRowIdName={[ 'contestId' ]}
              legendProps={[ { color: '#FFA1A1', message: CONTEST_IS_DELETED }, { color: '#C0C0C0', message: CONTEST_IS_NOT_VISIBLE } ]}
              modals={[
                  { showModal: showCreateModal, modal: (i) => renderCreateModal(i) },
                  { showModal: showConfirmDialog, modal: (i) => renderConfirmDialog(i) },
              ]}
            />
        </>
    );
};
export default LecturerInContests;
