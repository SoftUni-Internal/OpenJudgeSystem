import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../../common/types';
import CreateButton from '../../../components/administration/common/create/CreateButton';
import LecturerActions
    from '../../../components/administration/common/lecturer-actions/LecturerActions';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import RoleForm from '../../../components/administration/roles/form/RoleForm';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { useGetAllRolesQuery, useLazyExportRolesToExcelQuery } from '../../../redux/services/admin/rolesAdminService';
import { DEFAULT_ITEMS_PER_PAGE } from '../../../utils/constants';
import { renderSuccessfullAlert } from '../../../utils/render-utils';
import AdministrationGridView from '../AdministrationGridView';

import rolesFilterableColumns, { returnRolesNonFilterableColumns } from './rolesGridColumns';

const AdministrationRolesPage = () => {
    const [ searchParams ] = useSearchParams();

    const [ roleId, setRoleId ] = useState<string | null>(null);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ showEditModal, setShowEditModal ] = useState<boolean>(false);
    const [ showCreateModal, setShowCreateModal ] = useState<boolean>(false);
    const [ showAddLecturerToCategoryModal, setShowAddLecturerToCategoryModal ] = useState<boolean>(false);
    const [ showRemoveLecturerFromCategoryModal, setShowRemoveLecturerFromCategoryModal ] = useState<boolean>(false);
    const [ showAddLecturerToContestModal, setShowAddLecturerToContestModal ] = useState<boolean>(false);
    const [ showRemoveLecturerFromContestModal, setShowRemoveLecturerFromContestModal ] = useState<boolean>(false);
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

    const onAddLecturerToCategory = (id: string) => {
        setRoleId(id);
        setShowAddLecturerToCategoryModal(true);
    };

    const onRemoveLecturerFromCategory = (id: string) => {
        setRoleId(id);
        setShowRemoveLecturerFromCategoryModal(true);
    };

    const onAddLecturerToContest = (id: string) => {
        setRoleId(id);
        setShowAddLecturerToContestModal(true);
    };

    const onRemoveLecturerFromContest = (id: string) => {
        setRoleId(id);
        setShowRemoveLecturerFromContestModal(true);
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

    const renderRoleModal = (index: number, isEditMode: boolean) => 
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
              onSuccess={() => onModalClose(isEditMode)}
              setParentSuccessMessage={setSuccessMessage}
            />
        </AdministrationModal>
    ;

    const renderLecturerActions = (index: number, isRemove: boolean, isContest: boolean) => {
        const showModal = isContest
            ? isRemove
                ? showRemoveLecturerFromContestModal
                : showAddLecturerToContestModal
            : isRemove
                ? showRemoveLecturerFromCategoryModal
                : showAddLecturerToCategoryModal;

        const setShowModal = isContest
            ? isRemove
                ? setShowRemoveLecturerFromContestModal
                : setShowAddLecturerToContestModal
            : isRemove
                ? setShowRemoveLecturerFromCategoryModal
                : setShowAddLecturerToCategoryModal;

        return (
            <LecturerActions
              index={index}
              roleId={roleId}
              showModal={showModal}
              setShowModal={setShowModal}
              isRemove={isRemove}
              isContest={isContest}
              setParentSuccessMessage={setSuccessMessage}
              onSuccess={() => setShowModal(false)}
            />
        );
    };

    const renderGridActions = () => 
        <CreateButton
          showModal={showCreateModal}
          showModalFunc={setShowCreateModal}
          styles={{ width: '40px', height: '40px' }}
        />
    ;

    return (
        <>
            {renderSuccessfullAlert(successMessage, 7000)}
            <AdministrationGridView
              filterableGridColumnDef={rolesFilterableColumns}
              notFilterableGridColumnDef={returnRolesNonFilterableColumns(
                  onEditClick,
                  onAddLecturerToCategory,
                  onRemoveLecturerFromCategory,
                  onAddLecturerToContest,
                  onRemoveLecturerFromContest,
                  onDeleteSuccess,
                  setSuccessMessage,
              )}
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
                  { showModal: showAddLecturerToCategoryModal, modal: (i) => renderLecturerActions(i, false, false) },
                  { showModal: showRemoveLecturerFromCategoryModal, modal: (i) => renderLecturerActions(i, true, false) },
                  { showModal: showAddLecturerToContestModal, modal: (i) => renderLecturerActions(i, false, true) },
                  { showModal: showRemoveLecturerFromContestModal, modal: (i) => renderLecturerActions(i, true, true) },
              ]}
              excelMutation={useLazyExportRolesToExcelQuery}
            />
        </>
    );
};
export default AdministrationRolesPage;
