/* eslint-disable @typescript-eslint/ban-types */
import { Link } from 'react-router-dom';
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { ProblemResourceType } from '../../../common/enums';
import { CREATED_ON, MODIFIED_ON, PROBLEM_RESOURCE } from '../../../common/labels';
import { DELETE_CONFIRMATION_MESSAGE } from '../../../common/messages';
import { IEnumType } from '../../../common/types';
import { NEW_ADMINISTRATION_PATH, PROBLEM_RESOURCES_PATH, PROBLEMS_PATH } from '../../../common/urls/administration-urls';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import DownloadIconButton from '../../../components/administration/common/download/DownloadIconButton';
import QuickEditButton from '../../../components/administration/common/edit/QuickEditButton';
import RedirectButton from '../../../components/administration/common/edit/RedirectButton';
import { useDeleteProblemResourceMutation, useDownloadResourceQuery } from '../../../redux/services/admin/problemResourcesAdminService';
import { adminFormatDate } from '../../../utils/administration/administration-dates';
import { getStringObjectKeys } from '../../../utils/object-utils';

const problemResourceFilterableColumns: GridColDef[] = [
    {
        field: 'id',
        headerName: 'Id',
        flex: 0,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'name',
        headerName: 'Name',
        flex: 0.5,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'type',
        headerName: 'Type',
        flex: 0,
        filterable: false,
        sortable: false,
        align: 'center',
        type: 'enum',
        headerAlign: 'center',
        enumValues: getStringObjectKeys(ProblemResourceType),
        valueFormatter: (params) => ProblemResourceType[params.value],
    } as GridColDef & IEnumType,
    {
        field: 'fileExtension',
        headerName: 'File Extension',
        flex: 0,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'link',
        headerName: 'Link',
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        headerAlign: 'center',
        renderCell: (params) => <Link to={`${params.value}`}>{params.value}</Link>,
    },
    {
        field: 'orderBy',
        headerName: 'Order By',
        flex: 0,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'problemId',
        headerName: 'Problem Id',
        flex: 0,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'problemName',
        headerName: 'Problem Name',
        flex: 0.5,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        renderCell: (params) => (
            <Link to={`/${NEW_ADMINISTRATION_PATH}/${PROBLEMS_PATH}/${params.row.problemId}`}>
                {params.row.problemName}
            </Link>
        ),
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

export const returnProblemResourceNonFilterableColumns = (onEditClick: Function, onSuccessFullyDeleted:Function) => [
    {
        field: 'actions',
        headerName: 'Actions',
        flex: 0.5,
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <QuickEditButton onEdit={() => onEditClick(Number(params.row.id))} />
                <RedirectButton
                  location="Edit page"
                  path={`/${NEW_ADMINISTRATION_PATH}/${PROBLEM_RESOURCES_PATH}/${Number(params.row.id)}`}
                />
                <DeleteButton
                  id={Number(params.row.id)}
                  name={`${PROBLEM_RESOURCE}`}
                  text={DELETE_CONFIRMATION_MESSAGE}
                  mutation={useDeleteProblemResourceMutation}
                  onSuccess={() => onSuccessFullyDeleted()}
                />
                <DownloadIconButton
                  args={params.row.id}
                  mutation={useDownloadResourceQuery}
                />
            </div>
        ),
    } ] as GridColDef[];
export default problemResourceFilterableColumns;
