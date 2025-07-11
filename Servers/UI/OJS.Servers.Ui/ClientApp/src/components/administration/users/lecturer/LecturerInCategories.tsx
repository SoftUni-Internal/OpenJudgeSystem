
import { useEffect, useState } from 'react';

import { SELECT_CATEGORY } from '../../../../common/labels';
import { IContestCategories, IGetAllAdminParams } from '../../../../common/types';
import useDelayedSuccessEffect from '../../../../hooks/common/use-delayed-success-effect';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { applyDefaultFilterToQueryString } from '../../../../pages/administration-new/administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd } from '../../../../pages/administration-new/AdministrationGridView';
import lecturerInCategoriesFilterableColumns, { returnLecturerInCategoriesNonFilterableColumns } from '../../../../pages/administration-new/lecturers-in-categories/lecturersInCategoriesGridColumns';
import { useGetCategoriesQuery } from '../../../../redux/services/admin/contestCategoriesAdminService';
import { useAddLecturerToCategoryMutation, useGetLecturerCategoriesQuery, useRemoveLecturerFromCategoryMutation } from '../../../../redux/services/admin/usersAdminService';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert } from '../../../../utils/render-utils';
import clearSuccessMessages from '../../../../utils/success-messages-utils';
import ConfirmDialog from '../../../guidelines/dialog/ConfirmDialog';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import CreateButton from '../../common/create/CreateButton';
import LecturerForm from '../../common/lecturer/lecturer-form/LecturerForm';
import AdministrationModal from '../../common/modals/administration-modal/AdministrationModal';

interface ILecturerInCategoriesProps {
    userId: string;
    setParentSuccessMessage: Function;
}

const LecturerInCategories = (props: ILecturerInCategoriesProps) => {
    const defaultSorter = 'contestCategoryId=DESC';
    const { userId, setParentSuccessMessage } = props;

    const [ showConfirmDialog, setShowConfirmDialog ] = useState<boolean>(false);

    const [ categoryId, setCategoryId ] = useState<number>(0);
    const [ errorMessages, setErrorMessages ] = useState <Array<string>>([]);
    const [ categoryToAdd, setCategoryToAdd ] = useState<number>(0);

    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(
        defaultFilterToAdd,
        defaultSorter,
    ));

    const [ showCreateModal, setShowCreateModal ] = useState<boolean>(false);

    const {
        refetch,
        isFetching,
        data,
        isLoading,
        error,
    } = useGetLecturerCategoriesQuery({ userId, ...queryParams });

    const [
        addLecturerToCategory,
        {
            data: addData,
            error: addError,
            isSuccess: isSuccessfullyAdded,
            isLoading: isAdding,
            reset: resetAdd,
        },
    ] = useAddLecturerToCategoryMutation();

    const [
        removeLecturerFromCategory,
        {
            data: removeData,
            error: removeError,
            isSuccess: isSuccessfullyRemoved,
            isLoading: isRemoving,
            reset: resetRemove,
        },
    ] = useRemoveLecturerFromCategoryMutation();

    const { data: contestCategories } = useGetCategoriesQuery(null);

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

            refetch();
        },
    });

    useEffect(() => {
        getAndSetExceptionMessage([ addError, removeError ], setErrorMessages);
        clearSuccessMessages({ setParentSuccessMessage });
    }, [ addError, removeError, setParentSuccessMessage ]);

    const onRemoveFromRowClicked = (uId: number) => {
        setCategoryId(uId);
        setShowConfirmDialog(true);
    };

    const onChange = (category?: IContestCategories) => {
        setCategoryToAdd(category?.id ?? 0);
    };

    const renderCreateModal = (i: number) =>
        <AdministrationModal
          key={i}
          index={i}
          open={showCreateModal}
          onClose={() => {
              setShowCreateModal(false);
          }}
        >
            <LecturerForm
              data={contestCategories!}
              disabled={categoryToAdd === 0}
              heading="Add lecturer to contest"
              label={SELECT_CATEGORY}
              onChange={onChange}
              onClick={() => {
                  addLecturerToCategory({ lecturerId: userId, categoryId: categoryToAdd });
                  setCategoryToAdd(0);
              }}
            />
        </AdministrationModal>
    ;

    const renderConfirmDialog = (index: number) =>
        <ConfirmDialog
          key={index}
          text="Are you sure you want to remove lecturer from contest."
          title="Remove from role"
          declineButtonText="Close"
          confirmButtonText="Remove"
          declineFunction={() => setShowConfirmDialog(false)}
          confirmFunction={() => {
              removeLecturerFromCategory({ lecturerId: userId, categoryId });
              setShowConfirmDialog(false);
          }}
        />
    ;

    const renderGridSettings = () =>
        <CreateButton
          showModal={showCreateModal}
          showModalFunc={setShowCreateModal}
          styles={{ width: '40px', height: '40px', color: 'rgb(25,118,210)' }}
          tooltipLabel="Add lecturer to category"
        />
    ;

    if (isLoading || isFetching && !showConfirmDialog) {
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
              filterableGridColumnDef={lecturerInCategoriesFilterableColumns}
              renderActionButtons={renderGridSettings}
              notFilterableGridColumnDef={
            returnLecturerInCategoriesNonFilterableColumns(onRemoveFromRowClicked)
        }
              specificRowIdName={[ 'contestCategoryId' ]}
              legendProps={[ { color: '#FFA1A1', message: 'Contest category is deleted.' } ]}
              modals={[
                  { showModal: showCreateModal, modal: (i) => renderCreateModal(i) },
                  { showModal: showConfirmDialog, modal: (i) => renderConfirmDialog(i) },
              ]}
            />
        </>
    );
};
export default LecturerInCategories;
