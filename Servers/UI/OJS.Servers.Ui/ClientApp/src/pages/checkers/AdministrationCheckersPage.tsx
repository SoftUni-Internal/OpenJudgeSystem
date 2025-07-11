import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../common/types';
import CheckerForm from '../../components/administration/checkers/checker-form/CheckerForm';
import CreateButton from '../../components/administration/common/create/CreateButton';
import AdministrationModal from '../../components/administration/common/modals/administration-modal/AdministrationModal';
import SpinningLoader from '../../components/guidelines/spinning-loader/SpinningLoader';
import { getColors, useAdministrationTheme } from '../../hooks/use-administration-theme-provider';
import { useDeleteCheckerMutation, useGetAllCheckersQuery, useLazyExportCheckersToExcelQuery } from '../../redux/services/admin/checkersAdminService';
import { renderSuccessfullAlert } from '../../utils/render-utils';
import { applyDefaultFilterToQueryString } from '../administration-new/administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd, defaultSorterToAdd } from '../administration-new/AdministrationGridView';

import checkersFilterableColumns, { returnCheckersNonFilterableColumns } from './checkersGridColumns';

const AdministrationCheckersPage = () => {
    const [ searchParams ] = useSearchParams();

    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(defaultFilterToAdd, defaultSorterToAdd, searchParams));
    const { themeMode } = useAdministrationTheme();
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ openEditModal, setOpenEditModal ] = useState(false);
    const [ checkerId, setCheckerId ] = useState<number | null>(null);
    const [ openCreateModal, setOpenCreateModal ] = useState<boolean>(false);

    const {
        refetch: retakeCheckers,
        data: checkersData,
        isLoading: isLoadingCheckers,
        error: checkersError,
    } = useGetAllCheckersQuery(queryParams);

    const onEditClick = (id: number) => {
        setCheckerId(id);
        setOpenEditModal(true);
    };

    const onCloseModal = (isEditMode: boolean) => {
        if (isEditMode) {
            setOpenEditModal(false);
        } else {
            setOpenCreateModal(false);
        }
        retakeCheckers();
    };

    const renderModal = (index: number, isEditMode: boolean) => 
        <AdministrationModal
          key={index}
          index={index}
          open={isEditMode
              ? openEditModal
              : openCreateModal}
          onClose={() => onCloseModal(isEditMode)}
        >
            <CheckerForm
              id={checkerId}
              isEditMode={isEditMode}
              onSuccess={() => onCloseModal(isEditMode)}
              setParentSuccessMessage={setSuccessMessage}
            />
        </AdministrationModal>
    ;

    const renderGridSettings = () => 
        <CreateButton
          showModal={openCreateModal}
          showModalFunc={setOpenCreateModal}
          styles={{ width: '40px', height: '40px' }}
        />
    ;

    if (isLoadingCheckers) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage, 7000)}
            <AdministrationGridView
              data={checkersData}
              error={checkersError}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              renderActionButtons={renderGridSettings}
              filterableGridColumnDef={checkersFilterableColumns}
              notFilterableGridColumnDef={returnCheckersNonFilterableColumns(
                  onEditClick,
                  useDeleteCheckerMutation,
                  retakeCheckers,
                  setSuccessMessage,
              )}
              modals={[
                  { showModal: openEditModal, modal: (i) => renderModal(i, true) },
                  { showModal: openCreateModal, modal: (i) => renderModal(i, false) },
              ]}
              legendProps={[ { color: getColors(themeMode).palette.deleted, message: 'Checker is deleted.' } ]}
              excelMutation={useLazyExportCheckersToExcelQuery}
            />
        </>
    );
};

export default AdministrationCheckersPage;
