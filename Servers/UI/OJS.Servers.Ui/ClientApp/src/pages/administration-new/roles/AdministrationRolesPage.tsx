import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../../common/types';
import CreateButton from '../../../components/administration/common/create/CreateButton';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import RoleForm from '../../../components/administration/roles/form/RoleForm';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { useGetAllRolesQuery, useLazyExportRolesToExcelQuery } from '../../../redux/services/admin/rolesAdminService';
import { DEFAULT_ITEMS_PER_PAGE } from '../../../utils/constants';
import AdministrationGridView from '../AdministrationGridView';

import rolesFilterableColumns, { returnRolesNonFilterableColumns } from './rolesGridColumns';

const AdministrationRolesPage = () => {
    const [ searchParams ] = useSearchParams();

    const [ roleId, setRoleId ] = useState<string | null>(null);
    const [ showEditModal, setShowEditModal ] = useState<boolean>(false);
    const [ showCreateModal, setShowCreateModal ] = useState<boolean>(false);
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>({
        page: 1,
        itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
        filter: searchParams.get('filter') ?? '',
        sorting: searchParams.get('sorting') ?? '',
    });

    const { refetch, data, isLoading, error } = useGetAllRolesQuery(queryParams);

    const onEditClick = (id: string) => {
        setRoleId(id);
        setShowEditModal(true);
    };

    if (isLoading) {
        <SpinningLoader />;
    }

    const onDeleteSuccess = () => {
        refetch();
    };

    const onModalClose = (isEditMode: boolean) => {
        if (isEditMode) {
            setShowEditModal(false);
        } else {
            setShowCreateModal(false);
        }
        refetch();
    };

    const renderRoleModal = (index: number, isEditMode: boolean) => (
        <AdministrationModal
          key={index}
          index={index}
          onClose={() => onModalClose(isEditMode)}
          open={isEditMode
              ? showEditModal
              : showCreateModal}
        >
            <RoleForm
              isEditMode={isEditMode}
              id={isEditMode
                  ? roleId
                  : null}
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
          filterableGridColumnDef={rolesFilterableColumns}
          notFilterableGridColumnDef={returnRolesNonFilterableColumns(onEditClick, onDeleteSuccess)}
          data={data}
          error={error}
          queryParams={queryParams}
          setQueryParams={setQueryParams}
          renderActionButtons={renderGridActions}
          withSearchParams={false}
          showFiltersAndSorters={false}
          modals={[
              { showModal: showEditModal, modal: (i) => renderRoleModal(i, true) },
              { showModal: showCreateModal, modal: (i) => renderRoleModal(i, false) },
          ]}
          excelMutation={useLazyExportRolesToExcelQuery}
        />
    );
};
export default AdministrationRolesPage;
