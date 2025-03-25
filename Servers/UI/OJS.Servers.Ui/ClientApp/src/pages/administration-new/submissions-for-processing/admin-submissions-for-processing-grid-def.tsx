import { Link } from 'react-router-dom';
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { AdministrationGridColDef } from '../../../components/administration/utils/mui-utils';
import { adminFormatDate, adminPreciseFormatDate } from '../../../utils/administration/administration-dates';

const dataColumns: AdministrationGridColDef[] = [
    {
        field: 'id',
        headerName: 'Id',
        type: 'number',
        align: 'center',
        headerAlign: 'center',
        flex: 1,
        filterable: false,
        sortable: false,
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'submissionId',
        headerName: 'Submission Id',
        align: 'center',
        headerAlign: 'center',
        type: 'number',
        width: 100,
        flex: 0.8,
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => 
            <Link
              to={`/submissions/${Number(params.row?.submissionId)}/details`}
              target="_blank"
            >
                {params.row?.submissionId}
            </Link>
        ,
    },
    {
        field: 'state',
        headerName: 'State',
        align: 'center',
        headerAlign: 'center',
        type: 'string',
        flex: 0.8,
        filterable: false,
        sortable: false,
    },
    {
        field: 'enqueuedAt',
        headerName: 'Enqueued At',
        align: 'center',
        headerAlign: 'center',
        type: 'date',
        width: 200,
        flex: 1,
        filterable: true,
        sortable: true,
        valueFormatter: (params) => adminPreciseFormatDate(params.value),
    },
    {
        field: 'processingStartedAt',
        headerName: 'Processing Started At',
        align: 'center',
        headerAlign: 'center',
        type: 'date',
        width: 200,
        flex: 1,
        filterable: true,
        sortable: true,
        valueFormatter: (params) => adminPreciseFormatDate(params.value),
    },
    {
        field: 'processedAt',
        headerName: 'Processed At',
        align: 'center',
        headerAlign: 'center',
        type: 'date',
        width: 200,
        flex: 1,
        filterable: true,
        sortable: true,
        valueFormatter: (params) => adminPreciseFormatDate(params.value),
    },
    {
        field: 'createdOn',
        headerName: 'Created On',
        align: 'center',
        headerAlign: 'center',
        type: 'date',
        width: 200,
        filterable: true,
        sortable: true,
        valueFormatter: (params) => adminFormatDate(params.value),
    },
    {
        field: 'modifiedOn',
        headerName: 'Modified On',
        align: 'center',
        headerAlign: 'center',
        type: 'date',
        width: 200,
        filterable: true,
        sortable: true,
        valueFormatter: (params) => adminFormatDate(params.value),
    },
];

export const returnSubmissionsForProcessingNonFilterableColumns = () => [] as GridColDef[];

export default dataColumns;
