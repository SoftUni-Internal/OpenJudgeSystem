/* eslint-disable @typescript-eslint/ban-types */
import BallotIcon from '@mui/icons-material/Ballot';
import EditIcon from '@mui/icons-material/Edit';
import { IconButton } from '@mui/material';
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { getContestsByCategoryUrl } from 'src/common/urls/compose-client-urls';
import ExternalLink from 'src/components/guidelines/buttons/ExternalLink';

import { CREATED_ON, MODIFIED_ON } from '../../../common/labels';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import { AdministrationGridColDef } from '../../../components/administration/utils/mui-utils';
import { useDeleteContestCategoryMutation } from '../../../redux/services/admin/contestCategoriesAdminService';
import { adminFormatDate } from '../../../utils/administration/administration-dates';

const categoriesFilterableColumns: AdministrationGridColDef[] = [
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
        valueFormatter: (_, row) => row.value?.toString(),
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
        field: 'hasChildren',
        headerName: 'Has Children',
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
        renderCell: (params) => (
            <ExternalLink
              to={getContestsByCategoryUrl({
                  categoryId: params.row.id,
                  categoryName: params.row.name,
              })}
              text={params.value.toString()}
            />
        ),
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
        renderCell: (params) => params.value && (
            <ExternalLink
              to={getContestsByCategoryUrl({
                  categoryId: params.row.parentId,
                  categoryName: params.row.parent,
              })}
              text={params.value.toString()}
            />
        ),
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
        field: 'allowMentor',
        headerName: 'Allow Mentor',
        headerAlign: 'center',
        type: 'boolean',
        flex: 0,
        filterable: false,
        align: 'center',
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
        valueFormatter: (_, row) => adminFormatDate(row.value),
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

export const returnCategoriesNonFilterableColumns = (
    onEditClick: Function,
    onContestsBulkEditClick: Function,
    onSuccessfulDelete: () => void,
    setParentSuccessMessage: Function,
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
                  onSuccess={onSuccessfulDelete}
                  setParentSuccessMessage={setParentSuccessMessage}
                />
                <IconButton onClick={() => onContestsBulkEditClick(params.row.id, params.row.name)} disabled={params.row.hasChildren}>
                    <BallotIcon color={params.row.hasChildren
                        ? 'disabled'
                        : 'primary'}
                    />
                </IconButton>
            </div>
        ),
    },
] as GridColDef[];

export default categoriesFilterableColumns;
