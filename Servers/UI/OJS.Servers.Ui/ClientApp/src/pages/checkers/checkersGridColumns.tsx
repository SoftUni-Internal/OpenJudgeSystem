/* eslint-disable @typescript-eslint/ban-types */
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { CREATED_ON, MODIFIED_ON } from '../../common/labels';
import { DELETE_CONFIRMATION_MESSAGE } from '../../common/messages';
import DeleteButton from '../../components/administration/common/delete/DeleteButton';
import QuickEditButton from '../../components/administration/common/edit/QuickEditButton';
import { AdministrationGridColDef } from '../../components/administration/utils/mui-utils';
import { adminFormatDate } from '../../utils/administration/administration-dates';

const checkersFilterableColumns: AdministrationGridColDef[] = [
    {
        field: 'id',
        headerName: 'Id',
        flex: 0,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (_, row) => row.value?.toString(),
    },
    {
        field: 'name',
        headerName: 'Name',
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'dllFile',
        headerName: 'DLL File',
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'className',
        headerName: 'Class name',
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'parameter',
        headerName: 'Parameter',
        flex: 2,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'isDeleted',
        headerName: 'Is Deleted',
        type: 'boolean',
        flex: 0.5,
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
        field: 'modifiedOn',
        headerName: `${MODIFIED_ON}`,
        type: 'date',
        flex: 1,
        filterable: false,
        sortable: false,
        valueFormatter: (_, row) => adminFormatDate(row.value),
    },
];

export const returnCheckersNonFilterableColumns = (
    onEditClick: Function,
    deleteMutation: any,
    onSuccessfulDelete: () => void,
    setParentSuccessMessage: Function,
) => [
    {
        field: 'actions',
        headerName: 'Actions',
        flex: 0.5,
        minWidth: 100,
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <QuickEditButton onEdit={() => onEditClick(Number(params.row.id))} />
                <DeleteButton
                  id={Number(params.row.id)}
                  name={params.row.name}
                  text={DELETE_CONFIRMATION_MESSAGE}
                  mutation={deleteMutation}
                  onSuccess={onSuccessfulDelete}
                  setParentSuccessMessage={setParentSuccessMessage}
                />
            </div>
        ),
    },
] as GridColDef[];

export default checkersFilterableColumns;
