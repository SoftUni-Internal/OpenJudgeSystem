import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../../common/types';
import CreateButton from '../../../components/administration/common/create/CreateButton';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import SubmissionTypesForm from '../../../components/administration/submission-types/form/SubmissionTypeForm';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { useGetAllSubmissionTypesQuery, useLazyExportSubmissionTypesToExcelQuery } from '../../../redux/services/admin/submissionTypesAdminService';
import { renderSuccessfullAlert } from '../../../utils/render-utils';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultSorterToAdd } from '../AdministrationGridView';

import submissionTypesFilterableColumns, { returnNonFilterableColumns } from './submissionTypesGridColumns';

const AdministrationSubmissionTypesPage = () => {
    const [ searchParams ] = useSearchParams();

    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ showCreateModal, setShowCreateModal ] = useState<boolean>(false);
    const [ showEditModal, setShowEditModal ] = useState<boolean>(false);
    const [ submissionTypeId, setSubmissionTypeId ] = useState<number | null>(null);

    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString('', defaultSorterToAdd, searchParams));

    const { refetch, data: submissionTypesData, isLoading: isGettingData, error } = useGetAllSubmissionTypesQuery(queryParams);

    const onEditClick = (id: number) => {
        setSubmissionTypeId(id);
        setShowEditModal(true);
    };

    if (isGettingData) {
        return <SpinningLoader />;
    }
    const onModalClose = (isEditMode : boolean) => {
        if (isEditMode) {
            setShowEditModal(false);
        } else {
            setShowCreateModal(false);
        }
        refetch();
    };
    const onSuccessFullDelete = () => {
        refetch();
    };

    const renderFormModal = (index: number, isEditMode: boolean) => (
        <AdministrationModal
          key={index}
          index={index}
          open={isEditMode
              ? showEditModal
              : showCreateModal}
          onClose={() => onModalClose(isEditMode)}
        >
            <SubmissionTypesForm
              id={submissionTypeId}
              isEditMode={isEditMode}
              onSuccess={() => onModalClose(isEditMode)}
              setParentSuccessMessage={setSuccessMessage}
            />
        </AdministrationModal>
    );

    const renderGridSettings = () => (
        <CreateButton
          showModal={showCreateModal}
          showModalFunc={setShowCreateModal}
          styles={{ width: '40px', height: '40px' }}
        />
    );

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            <AdministrationGridView
              filterableGridColumnDef={submissionTypesFilterableColumns}
              notFilterableGridColumnDef={returnNonFilterableColumns(onEditClick, onSuccessFullDelete)}
              data={submissionTypesData}
              error={error}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              modals={[
                  { showModal: showEditModal, modal: (i) => renderFormModal(i, true) },
                  { showModal: showCreateModal, modal: (i) => renderFormModal(i, false) },
              ]}
              renderActionButtons={renderGridSettings}
              excelMutation={useLazyExportSubmissionTypesToExcelQuery}
            />
        </>
    );
};
export default AdministrationSubmissionTypesPage;
