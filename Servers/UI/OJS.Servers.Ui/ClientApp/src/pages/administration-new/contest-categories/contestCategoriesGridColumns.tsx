/* eslint-disable @typescript-eslint/ban-types */
import EditIcon from '@mui/icons-material/Edit';
import { IconButton } from '@mui/material';
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { CREATED_ON, MODIFIED_ON } from '../../../common/labels';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import { useDeleteContestCategoryMutation } from '../../../redux/services/admin/contestCategoriesAdminService';
import { adminFormatDate } from '../../../utils/administration/administration-dates';

const categoriesFilterableColumns: GridColDef[] = [
    {
        field: 'id',
        headerName: 'Id',
        headerAlign: 'center',
        flex: 0.5,
        width: 10,
        type: 'number',
        filterable: false,
        align: 'center',
        sortable: false,
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'isDeleted',
        headerName: 'Is Deleted',
        headerAlign: 'center',
        type: 'boolean',
        flex: 0,
        filterable: false,
        align: 'center',
        sortable: false,
    },
    {
        field: 'isVisible',
        headerName: 'Is Visible',
        headerAlign: 'center',
        type: 'boolean',
        flex: 0,
        filterable: false,
        align: 'center',
        sortable: false,
    },
    {
        field: 'name',
        headerName: 'Name',
        headerAlign: 'center',
        width: 200,
        flex: 2,
        type: 'string',
        align: 'center',
        filterable: false,
        sortable: false,
    },
    {
        field: 'orderBy',
        headerName: 'Order By',
        headerAlign: 'center',
        flex: 0.5,
        align: 'center',
        type: 'number',
        filterable: false,
        sortable: false,
    },
    {
        field: 'parent',
        headerName: 'Parent',
        headerAlign: 'center',
        align: 'center',
        width: 150,
        flex: 2,
        type: 'string',
        filterable: false,
        sortable: false,
    },
    {
        field: 'parentId',
        headerName: 'Parent Id',
        headerAlign: 'center',
        align: 'center',
        width: 150,
        flex: 2,
        type: 'number',
        filterable: false,
        sortable: false,
    },
    {
        field: 'deletedOn',
        headerName: 'Deleted On',
        headerAlign: 'center',
        width: 105,
        flex: 1,
        align: 'center',
        type: 'date',
        filterable: false,
        sortable: false,
        valueFormatter: (params) => adminFormatDate(params.value),
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

export const returnCategoriesNonFilterableColumns = (onEditClick: Function) => [
    {
        field: 'actions',
        headerName: 'Actions',
        width: 140,
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <IconButton onClick={() => onEditClick(params.row.id)}>
                    <EditIcon color="warning" />
                </IconButton>
                <DeleteButton
                  id={Number(params.row.id)}
                  name={params.row.name}
                  text="Are you sure that you want to delete the contest category?"
                  mutation={useDeleteContestCategoryMutation}
                />
            </div>
        ),
    },
] as GridColDef[];

export default categoriesFilterableColumns;
