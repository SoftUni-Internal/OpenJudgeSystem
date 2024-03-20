import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { DELETE_CONFIRMATION_MESSAGE } from '../../../common/messages';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';

const participantsFilteringColumns: GridColDef[] = [
    {
        field: 'id',
        headerName: 'Id',
        width: 10,
        align: 'center',
        headerAlign: 'center',
        type: 'number',
        filterable: false,
        sortable: false,
        flex: 0.5,
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'userName',
        headerName: 'UserName',
        width: 10,
        headerAlign: 'center',
        align: 'center',
        type: 'string',
        filterable: false,
        flex: 2,
        sortable: false,
    },
    {
        field: 'contestName',
        headerName: 'Contest Name',
        width: 10,
        headerAlign: 'center',
        align: 'center',
        type: 'string',
        flex: 2,
        filterable: false,
        sortable: false,
    },
    {
        field: 'isOfficial',
        headerName: 'Is Official',
        headerAlign: 'center',
        width: 10,
        align: 'center',
        type: 'boolean',
        flex: 2,
        filterable: false,
        sortable: false,
    },
];

export const returnparticipantsNonFilterableColumns = (
    deleteMutation: any,
    onSuccessFullDelete: () => void,
) => [
    {
        field: 'actions',
        headerName: 'Actions',
        flex: 1,
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <DeleteButton
                  id={Number(params.row.id)}
                  name={params.row.name}
                  text={DELETE_CONFIRMATION_MESSAGE}
                  mutation={deleteMutation}
                  onSuccess={onSuccessFullDelete}
                />
            </div>
        ),
    },
] as GridColDef[];

export default participantsFilteringColumns;