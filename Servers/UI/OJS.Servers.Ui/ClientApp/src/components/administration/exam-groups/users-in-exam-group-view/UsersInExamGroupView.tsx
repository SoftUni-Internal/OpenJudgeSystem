/* eslint-disable react-hooks/exhaustive-deps */
import { useEffect, useState } from 'react';
import QueueIcon from '@mui/icons-material/Queue';
import { IconButton, Tooltip } from '@mui/material';
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { DISABLED_USER_TO_EXAM_GROUP_BUTTON } from '../../../../common/messages';
import { IGetAllAdminParams } from '../../../../common/types';
import {
    IAdministrationFilter,
    mapFilterParamsToQueryString,
} from '../../../../pages/administration-new/administration-filters/AdministrationFilters';
import {
    IAdministrationSorter,
    mapSorterParamsToQueryString,
} from '../../../../pages/administration-new/administration-sorting/AdministrationSorting';
import AdministrationGridView from '../../../../pages/administration-new/AdministrationGridView';
import { useDeleteUserFromExamGroupMutation } from '../../../../redux/services/admin/examGroupsAdminService';
import { useGetByExamGroupIdQuery } from '../../../../redux/services/admin/usersAdminService';
import { DEFAULT_ITEMS_PER_PAGE } from '../../../../utils/constants';
import { flexCenterObjectStyles } from '../../../../utils/object-utils';
import CreateButton from '../../common/create/CreateButton';
import AdministrationModal from '../../common/modals/administration-modal/AdministrationModal';
import AddBulkUsersInGroupModal from '../add-bulk-users-in-group-modal/AddBulkUserInGroupModal';
import AddUserInExamGroupModal from '../add-user-in-group-modal/AddUserInGroupModal';
import DeleteUserFromGroupButton from '../delete-user-from-group-button/DeleteUserFromGroupButton';

interface IUsersInExamGroupViewProps {
    examGroupId: number;
    isAllowedToAddUsers: boolean;
}

const UsersInExamGroupView = (props: IUsersInExamGroupViewProps) => {
    const { examGroupId, isAllowedToAddUsers } = props;

    const [ selectedFilters, setSelectedFilters ] = useState<Array<IAdministrationFilter>>([]);
    const [ selectedSorters, setSelectedSorters ] = useState<Array<IAdministrationSorter>>([]);

    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>({
        page: 1,
        itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
        filter: mapFilterParamsToQueryString(selectedFilters),
        sorting: mapSorterParamsToQueryString(selectedSorters),
    });

    const { refetch, data, error } = useGetByExamGroupIdQuery({ examGroupId: Number(examGroupId), ...queryParams });
    const [ openShowAddUserModal, setOpenShowAddUserModal ] = useState<boolean>(false);
    const [ openShowAddBulkUsersModal, setOpenShowAddBulkUsersModal ] = useState<boolean>(false);
    const filtersQueryParams = mapFilterParamsToQueryString(selectedFilters);
    const sortersQueryParams = mapSorterParamsToQueryString(selectedSorters);

    useEffect(() => {
        setQueryParams({ ...queryParams, filter: filtersQueryParams });
    }, [ filtersQueryParams ]);

    useEffect(() => {
        setQueryParams({ ...queryParams, sorting: sortersQueryParams });
    }, [ sortersQueryParams ]);

    const dataColumns: GridColDef[] = [
        {
            field: 'id',
            headerName: 'Id',
            headerAlign: 'center',
            align: 'center',
            type: 'string',
            filterable: false,
            sortable: false,
            flex: 1,
            valueFormatter: (params) => params.value.toString(),
        },
        {
            field: 'userName',
            headerName: 'Username',
            headerAlign: 'center',
            align: 'center',
            type: 'string',
            filterable: false,
            flex: 2,
            sortable: false,
        },
        {
            field: 'firstName',
            headerName: 'First Name',
            headerAlign: 'center',
            align: 'center',
            type: 'string',
            filterable: false,
            flex: 2,
            sortable: false,
        },
        {
            field: 'lastName',
            headerName: 'Last Name',
            headerAlign: 'center',
            align: 'center',
            type: 'string',
            filterable: false,
            flex: 2,
            sortable: false,
        },
        {
            field: 'email',
            headerName: 'Email',
            headerAlign: 'center',
            align: 'center',
            type: 'string',
            filterable: false,
            flex: 2,
            sortable: false,
        },
    ];

    const notFilterableGridColumns: GridColDef[] = [
        {
            field: 'actions',
            headerName: 'Actions',
            width: 160,
            headerAlign: 'center',
            align: 'center',
            filterable: false,
            sortable: false,
            renderCell: (params: GridRenderCellParams) => (
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    <DeleteUserFromGroupButton
                      id={String(params.row.id)}
                      examGroupId={Number(examGroupId)}
                      name={params.row.userName}
                      text="Are you sure that you want to delete the user?"
                      mutation={useDeleteUserFromExamGroupMutation}
                      onSuccess={() => refetch()}
                    />
                </div>
            ),
        },
    ];

    const renderAddUserModal = (index: number) => (
        <AdministrationModal
          index={index}
          key={index}
          open={openShowAddUserModal}
          onClose={() => {
              refetch();
              setOpenShowAddUserModal(!openShowAddUserModal);
          }}
        >
            <AddUserInExamGroupModal examGroupId={examGroupId} />
        </AdministrationModal>
    );

    const renderAddBulkUsersModal = (index: number) => (
        <AdministrationModal
          index={index}
          key={index}
          open={openShowAddBulkUsersModal}
          onClose={() => {
              refetch();
              setOpenShowAddBulkUsersModal(!openShowAddBulkUsersModal);
          }}
        >
            <AddBulkUsersInGroupModal examGroupId={examGroupId} />
        </AdministrationModal>
    );
    const renderActions = () => (
        <div style={{ ...flexCenterObjectStyles, justifyContent: 'space-between' }}>
            <CreateButton
              showModal={openShowAddUserModal}
              showModalFunc={setOpenShowAddUserModal}
              styles={{
                  width: '40px',
                  height: '40px',
                  color: !isAllowedToAddUsers
                      ? 'grey'
                      : 'rgb(25,118,210)',
              }}
              disabled={!isAllowedToAddUsers}
              disabledMessage={DISABLED_USER_TO_EXAM_GROUP_BUTTON}
            />
            <Tooltip title={!isAllowedToAddUsers
                ? DISABLED_USER_TO_EXAM_GROUP_BUTTON
                : 'Add multiple users'}
            >
                <span>
                    <IconButton
                      disabled={!isAllowedToAddUsers}
                      onClick={() => setOpenShowAddBulkUsersModal(!openShowAddBulkUsersModal)}
                    >
                        <QueueIcon
                          sx={{ width: '40px', height: '40px' }}
                          color={!isAllowedToAddUsers
                              ? 'disabled'
                              : 'primary'}
                        />
                    </IconButton>
                </span>
            </Tooltip>
        </div>
    );

    return (
        <AdministrationGridView
          data={data}
          error={error}
          filterableGridColumnDef={dataColumns}
          notFilterableGridColumnDef={notFilterableGridColumns}
          queryParams={queryParams}
          renderActionButtons={renderActions}
          selectedFilters={selectedFilters}
          selectedSorters={selectedSorters}
          setSorterStateAction={setSelectedSorters}
          setFilterStateAction={setSelectedFilters}
          modals={[
              { showModal: openShowAddUserModal, modal: (i) => renderAddUserModal(i) },
              { showModal: openShowAddBulkUsersModal, modal: (i) => renderAddBulkUsersModal(i) },
          ]}
          setQueryParams={setQueryParams}
          withSearchParams={false}
        />
    );
};

export default UsersInExamGroupView;