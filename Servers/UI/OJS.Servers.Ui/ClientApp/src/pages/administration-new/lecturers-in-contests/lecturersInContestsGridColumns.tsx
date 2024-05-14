/* eslint-disable @typescript-eslint/ban-types */
import { IconButton, Tooltip } from '@mui/material';
import { GridColDef, GridDeleteIcon, GridRenderCellParams } from '@mui/x-data-grid';

const lecturerInContestFilterableColumns: GridColDef[] = [
    {
        field: 'contestId',
        headerName: 'Contest Id',
        width: 120,
        align: 'center',
        headerAlign: 'center',
        type: 'number',
        filterable: false,
        sortable: false,
        flex: 0.5,
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'contestName',
        headerName: 'Contest Name',
        width: 120,
        align: 'center',
        headerAlign: 'center',
        type: 'string',
        filterable: false,
        sortable: false,
        flex: 1,
    },
];

export const returnLecturerInContestNonFilterableColumns = (removeFromRoleFunc?: Function) => [
    {
        field: 'actions',
        headerName: 'Actions',
        flex: 0.5,
        width: 100,
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                {removeFromRoleFunc && (
                <Tooltip title="Remove from Contest">
                    <IconButton onClick={() => removeFromRoleFunc(Number(params.row.contestId))}>
                        <GridDeleteIcon color="error" />
                    </IconButton>
                </Tooltip>
                )}
            </div>
        ),
    },
] as GridColDef[];

export default lecturerInContestFilterableColumns;
