
import { IconButton, Tooltip } from '@mui/material';
import { GridColDef, GridDeleteIcon, GridRenderCellParams } from '@mui/x-data-grid';

import { CREATED_ON, EDIT, MODIFIED_ON } from '../../../common/labels';
import { NEW_ADMINISTRATION_PATH, USERS_PATH } from '../../../common/urls/administration-urls';
import QuickEditButton from '../../../components/administration/common/edit/QuickEditButton';
import RedirectButton from '../../../components/administration/common/edit/RedirectButton';
import { AdministrationGridColDef } from '../../../components/administration/utils/mui-utils';
import { adminFormatDate } from '../../../utils/administration/administration-dates';

const usersFilterableColumns: AdministrationGridColDef[] = [
    {
        field: 'id',
        headerName: 'Id',
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'userName',
        headerName: 'User Name',
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'email',
        headerName: 'Email',
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'firstName',
        headerName: 'First Name',
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'lastName',
        headerName: 'Last Name',
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'city',
        headerName: 'City',
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'dateOfBirth',
        headerName: 'Date of birth',
        flex: 1,
        type: 'date',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => adminFormatDate(params.value),
    },
    {
        field: 'age',
        headerName: 'Age',
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'createdOn',
        headerName: `${CREATED_ON}`,
        type: 'date',
        flex: 1,
        filterable: false,
        sortable: false,
        valueFormatter: (params) => adminFormatDate(params.value),
    },
    {
        field: 'modifiedOn',
        headerName: `${MODIFIED_ON}`,
        type: 'date',
        flex: 1,
        filterable: false,
        sortable: false,
        valueFormatter: (params) => adminFormatDate(params.value),
    },
];

export const returnUsersNonFilterableColumns = (
    onEditClick: Function,
    removeFromRoleFunc?: Function,
) => [
    {
        field: 'actions',
        headerName: 'Actions',
        flex: 1,
        minWidth: 100,
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) =>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <QuickEditButton onEdit={() => onEditClick(params.row.id)} />
                <RedirectButton path={`/${NEW_ADMINISTRATION_PATH}/${USERS_PATH}/${params.row.id}`} location={`${EDIT} page`} />
                {removeFromRoleFunc &&
                <Tooltip title="Remove from Role">
                    <IconButton onClick={() => removeFromRoleFunc(params.row.id)}>
                        <GridDeleteIcon color="error" />
                    </IconButton>
                </Tooltip>
                }
            </div>
        ,
    },
] as GridColDef[];

export default usersFilterableColumns;
