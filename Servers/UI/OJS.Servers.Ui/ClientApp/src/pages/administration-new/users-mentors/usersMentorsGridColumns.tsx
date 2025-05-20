/* eslint-disable @typescript-eslint/ban-types,max-len */
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import QuickEditButton from 'src/components/administration/common/edit/QuickEditButton';
import { adminFormatDate } from 'src/utils/administration/administration-dates';

import {
    QUOTA_LIMIT,
    QUOTA_RESET_TIME,
    REQUESTS_MADE,
    TOTAL_REQUESTS_MADE,
    USER_ID, USERNAME,
} from '../../../common/labels';
import { AdministrationGridColDef } from '../../../components/administration/utils/mui-utils';

const usersMentorsFilterableColumns: AdministrationGridColDef[] = [
    {
        field: 'id',
        headerName: USER_ID,
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'userUserName',
        headerName: USERNAME,
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'quotaResetTime',
        headerName: QUOTA_RESET_TIME,
        flex: 1,
        type: 'date',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => adminFormatDate(params.value),
    },
    {
        field: 'requestsMade',
        headerName: REQUESTS_MADE,
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'totalRequestsMade',
        headerName: TOTAL_REQUESTS_MADE,
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'quotaLimit',
        headerName: QUOTA_LIMIT,
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => params.value?.toString(),
    },
];

export const returnNonFilterableColumns = (onEditClick: (id: string) => void) => [
    {
        field: 'actions',
        headerName: 'Actions',
        flex: 1,
        minWidth: 150,
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <QuickEditButton onEdit={() => onEditClick(params.row.id)} />
            </div>
        ),
    },
] as GridColDef[];

export default usersMentorsFilterableColumns;
