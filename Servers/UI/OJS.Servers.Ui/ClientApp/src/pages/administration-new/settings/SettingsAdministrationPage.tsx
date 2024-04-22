import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../../common/types';
import CreateButton from '../../../components/administration/common/create/CreateButton';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import SettingForm from '../../../components/administration/settings/form/SettingForm';
import { useDeleteSettingMutation, useGetAllSettingsQuery, useLazyExportSettingsToExcelQuery } from '../../../redux/services/admin/settingsAdminService';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultSorterToAdd } from '../AdministrationGridView';

import settingsFilterableColumns, { returnSettingsNonFilterableColumns } from './settingsGridColumns';

const AdministrationSettingsPage = () => {
    const [ searchParams ] = useSearchParams();

    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString('', defaultSorterToAdd, searchParams));

    const [ showEditModal, setShowEditModal ] = useState<boolean>(false);
    const [ settingId, setSettingId ] = useState<number | undefined>(undefined);

    const [ showCreateModal, setShowCreateModal ] = useState<boolean>(false);

    const { refetch, data: settingsData, error } = useGetAllSettingsQuery(queryParams);

    const onEditClick = (id: number) => {
        setShowEditModal(true);
        setSettingId(id);
    };

    const onModalClose = (isEditMode: boolean) => {
        if (isEditMode) {
            setShowEditModal(false);
        } else {
            setShowCreateModal(false);
        }
        refetch();
    };

    const renderSettingModal = (index: number, isEditMode: boolean) => (
        <AdministrationModal
          key={index}
          index={index}
          onClose={() => onModalClose(isEditMode)}
          open={isEditMode
              ? showEditModal
              : showCreateModal}
        >
            <SettingForm
              isEditMode={isEditMode}
              id={isEditMode
                  ? settingId
                  : undefined}
            />
        </AdministrationModal>
    );

    const renderGridActions = () => (
        <CreateButton
          showModal={showCreateModal}
          showModalFunc={setShowCreateModal}
          styles={{ width: '40px', height: '40px' }}
        />
    );

    return (
        <AdministrationGridView
          filterableGridColumnDef={settingsFilterableColumns}
          notFilterableGridColumnDef={returnSettingsNonFilterableColumns(
              onEditClick,
              useDeleteSettingMutation,
              () => refetch(),
          )}
          data={settingsData}
          error={error}
          queryParams={queryParams}
          setQueryParams={setQueryParams}
          excelMutation={useLazyExportSettingsToExcelQuery}
          renderActionButtons={renderGridActions}
          modals={[
              { showModal: showEditModal, modal: (i) => renderSettingModal(i, true) },
              { showModal: showCreateModal, modal: (i) => renderSettingModal(i, false) },
          ]}
        />
    );
};

export default AdministrationSettingsPage;
