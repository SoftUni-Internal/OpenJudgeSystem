import { ChangeEvent, useEffect, useState } from 'react';

import { CONTEST_IS_DELETED, CONTEST_IS_NOT_VISIBLE } from '../../../../common/messages';
import { IContestAutocomplete, IGetAllAdminParams } from '../../../../common/types';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { applyDefaultFilterToQueryString } from '../../../../pages/administration-new/administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd, defaultSorterToAdd } from '../../../../pages/administration-new/AdministrationGridView';
import lecturerInContestFilterableColumns, { returnLecturerInContestNonFilterableColumns } from '../../../../pages/administration-new/lecturers-in-contests/lecturersInContestsGridColumns';
import { useGetContestAutocompleteQuery } from '../../../../redux/services/admin/contestsAdminService';
import { useAddLecturerToContestMutation, useGetLecturerContestsQuery, useRemoveLecturerFromContestMutation } from '../../../../redux/services/admin/usersAdminService';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import ConfirmDialog from '../../../guidelines/dialog/ConfirmDialog';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import CreateButton from '../../common/create/CreateButton';
import LecturerForm from '../../common/lecturer/lecturer-form/LecturerForm';
import AdministrationModal from '../../common/modals/administration-modal/AdministrationModal';
import { onAutocompleteInputChange } from '../../utils/mui-utils';

interface ILeturerInContestsProps {
    userId: string;
}
const LecturerInContests = (props: ILeturerInContestsProps) => {
    const { userId } = props;

    const [ contestSearchString, setContestSearchString ] = useState<string>('');

    const [ contestId, setContestId ] = useState<number>(0);
    const [ showConfirmDialog, setShowConfirmDialog ] = useState<boolean>(false);
    const [ contestToAdd, setContestToAdd ] = useState<number>(0);

    const [ errorMessages, setErrorMessages ] = useState <Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState <string | null>(null);

    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(defaultFilterToAdd, defaultSorterToAdd));

    const [ contestAutocomplete, setContestsAutocomplete ] = useState<Array<IContestAutocomplete>>([
        {
            id: 0,
            name: '',
        },
    ]);

    const [ showCreateModal, setShowCreateModal ] = useState<boolean>(false);

    const {
        refetch,
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
        },
    ] = useAddLecturerToContestMutation();

    const {
        data: contestsAutocompleteData,
        error: getContestDataError,
    } = useGetContestAutocompleteQuery(contestSearchString);

    const [
        removeLecturerFromContest,
        {
            data: removeData,
            error: removeError,
            isSuccess: isSuccessfullyRemoved,
            isLoading: isRemoving,
        },
    ] = useRemoveLecturerFromContestMutation();

    useSuccessMessageEffect({
        data: [
            { message: addData, shouldGet: isSuccessfullyAdded },
            { message: removeData, shouldGet: isSuccessfullyRemoved },
        ],
        setSuccessMessage,
    });

    useEffect(() => {
        if (isSuccessfullyRemoved || isSuccessfullyAdded) {
            refetch();
        }
    }, [ isSuccessfullyAdded, isSuccessfullyRemoved, refetch ]);

    useEffect(() => {
        if (contestsAutocompleteData) {
            setContestsAutocomplete(contestsAutocompleteData);
        }
    }, [ contestsAutocompleteData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ getContestDataError, addError, removeError ], setErrorMessages);
        setSuccessMessage(null);
    }, [ addError, getContestDataError, removeError ]);

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
              name="Add lecturer to contest"
              onChange={onSelect}
              onInputChange={(e: ChangeEvent<HTMLInputElement>) => onAutocompleteInputChange(e, setContestSearchString)}
              onClick={() => addLecturerToContest({ lecturerId: userId, contestId: contestToAdd })}
            />
        </AdministrationModal>
    );

    if (isLoading || isAdding || isRemoving) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            {renderSuccessfullAlert(successMessage)}
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
              specificRowIdName="contestId"
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
