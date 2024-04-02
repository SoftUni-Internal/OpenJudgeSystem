/* eslint-disable @typescript-eslint/ban-types */
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { EDIT } from '../../../common/labels';
import { EXAM_GROUPS_PATH, NEW_ADMINISTRATION_PATH } from '../../../common/urls/administration-urls';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import QuickEditButton from '../../../components/administration/common/edit/QuickEditButton';
import RedirectButton from '../../../components/administration/common/edit/RedirectButton';

const examGroupsFilterableColumns: GridColDef[] = [
    {
        field: 'id',
        headerName: 'Id',
        headerAlign: 'center',
        flex: 0.5,
        width: 5,
        align: 'center',
        type: 'number',
        filterable: false,
        sortable: false,
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'name',
        headerName: 'Name',
        headerAlign: 'center',
        align: 'center',
        width: 200,
        type: 'string',
        filterable: false,
        sortable: false,
        flex: 2,
    },
    {
        field: 'contestName',
        headerName: 'Contest Name',
        headerAlign: 'center',
        align: 'center',
        type: 'string',
        filterable: false,
        sortable: false,
        flex: 2,
    },
    {
        field: 'externalAppId',
        headerName: 'External App Id',
        headerAlign: 'center',
        align: 'center',
        type: 'string',
        filterable: false,
        sortable: false,
        flex: 0.5,
    },
    {
        field: 'externalExamGroupId',
        headerName: 'External ExamGroup Id',
        headerAlign: 'center',
        align: 'center',
        type: 'number',
        filterable: false,
        sortable: false,
        flex: 0.5,
    },
];

export const returnExamGroupsNonFilterableColumns = (
    onEditClick: Function,
    deleteMutation: any,
) => [
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
                <QuickEditButton onEdit={() => onEditClick(Number(params.row.id))} />
                <RedirectButton
                  path={`/${NEW_ADMINISTRATION_PATH}/${EXAM_GROUPS_PATH}/${Number(params.row.id)}`}
                  location={`${EDIT} page`}
                />
                <DeleteButton
                  id={Number(params.row.id)}
                  name={params.row.name}
                  text="Are you sure that you want to delete the exam group."
                  mutation={deleteMutation}
                />
            </div>
        ),
    },
] as GridColDef[];

export default examGroupsFilterableColumns;
