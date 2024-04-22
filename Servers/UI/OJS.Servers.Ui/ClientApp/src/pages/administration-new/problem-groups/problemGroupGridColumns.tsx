/* eslint-disable @typescript-eslint/ban-types */
/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable react/self-closing-comp */
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { ProblemGroupTypes } from '../../../common/enums';
import { CREATED_ON, EDIT, MODIFIED_ON, PROBLEM_GROUP } from '../../../common/labels';
import { DELETE_CONFIRMATION_MESSAGE } from '../../../common/messages';
import { IEnumType } from '../../../common/types';
import { NEW_ADMINISTRATION_PATH, PROBLEM_GROUPS_PATH } from '../../../common/urls/administration-urls';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import QuickEditButton from '../../../components/administration/common/edit/QuickEditButton';
import RedirectButton from '../../../components/administration/common/edit/RedirectButton';
import { useDeleteProblemGroupMutation } from '../../../redux/services/admin/problemGroupsAdminService';
import { adminFormatDate } from '../../../utils/administration/administration-dates';
import { getStringObjectKeys } from '../../../utils/object-utils';

const filterableColumns: GridColDef[] = [
    {
        field: 'id',
        headerName: 'Id',
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'contest',
        headerName: 'Contest',
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
        flex: 1,
        type: 'boolean',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'orderBy',
        headerName: 'Order By',
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'type',
        headerName: 'Type',
        flex: 1,
        type: 'enum',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        enumValues: getStringObjectKeys(ProblemGroupTypes),
        valueFormatter: (params) => ProblemGroupTypes[params.value],
    } as GridColDef & IEnumType,
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

export const returnNonFilterableColumns = (onEditClick: Function) => [
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
                <QuickEditButton onEdit={() => onEditClick(Number(params.row.id))} />
                <RedirectButton
                  path={`/${NEW_ADMINISTRATION_PATH}/${PROBLEM_GROUPS_PATH}/${Number(params.row.id)}`}
                  location={`${EDIT} page`}
                />
                <DeleteButton
                  id={Number(params.row.id)}
                  name={`${PROBLEM_GROUP}`}
                  text={DELETE_CONFIRMATION_MESSAGE}
                  mutation={useDeleteProblemGroupMutation}
                />
            </div>
        ),
    } ] as GridColDef[];

export default filterableColumns;
