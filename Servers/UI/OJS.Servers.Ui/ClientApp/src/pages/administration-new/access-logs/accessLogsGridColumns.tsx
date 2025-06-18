/* eslint-disable @typescript-eslint/ban-types,max-len */
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { adminFormatDate } from 'src/utils/administration/administration-dates';

import {
    CREATED_ON,
    IP_ADDRESS, POST_PARAMS, REQUEST_TYPE, REQUEST_URL,
    USER_ID, USERNAME,
} from '../../../common/labels';
import ViewButton from '../../../components/administration/common/view/ViewButton';
import { AdministrationGridColDef } from '../../../components/administration/utils/mui-utils';

const accessLogsFilterableColumns: AdministrationGridColDef[] = [
    {
        field: 'userId',
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
        field: 'createdOn',
        headerName: `${CREATED_ON}`,
        type: 'date',
        flex: 1,
        filterable: false,
        sortable: false,
        valueFormatter: (_, row) => adminFormatDate(row.value),
    },
    {
        field: 'ipAddress',
        headerName: IP_ADDRESS,
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'requestType',
        headerName: REQUEST_TYPE,
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'url',
        headerName: REQUEST_URL,
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'postParams',
        headerName: POST_PARAMS,
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
];

export const returnNonFilterableColumns = (onViewClick: Function) => [
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
                <ViewButton
                  onClick={() => onViewClick(params.row.id)}
                  text="View access log"
                />
            </div>
        ),
    },
] as GridColDef[];

export default accessLogsFilterableColumns;
