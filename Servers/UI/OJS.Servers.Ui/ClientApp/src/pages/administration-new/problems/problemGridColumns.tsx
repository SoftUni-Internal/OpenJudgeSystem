/* eslint-disable @typescript-eslint/ban-types */
/* eslint-disable react/react-in-jsx-scope */
import { FaCopy } from 'react-icons/fa';
import ReplayIcon from '@mui/icons-material/Replay';
import { IconButton, Tooltip } from '@mui/material';
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { getContestsDetailsPageUrl } from 'src/common/urls/compose-client-urls';
import ExternalLink from 'src/components/guidelines/buttons/ExternalLink';

import { ProblemGroupTypes } from '../../../common/enums';
import { CREATED_ON, EDIT, MODIFIED_ON } from '../../../common/labels';
import { DELETE_CONFIRMATION_MESSAGE } from '../../../common/messages';
import { IEnumType } from '../../../common/types';
import { NEW_ADMINISTRATION_PATH, PROBLEMS_PATH } from '../../../common/urls/administration-urls';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import QuickEditButton from '../../../components/administration/common/edit/QuickEditButton';
import RedirectButton from '../../../components/administration/common/edit/RedirectButton';
import { AdministrationGridColDef } from '../../../components/administration/utils/mui-utils';
import { useDeleteProblemMutation } from '../../../redux/services/admin/problemsAdminService';
import { adminFormatDate } from '../../../utils/administration/administration-dates';
import { getStringObjectKeys } from '../../../utils/object-utils';

const problemFilterableColumns: AdministrationGridColDef[] = [
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
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'orderBy',
        headerName: 'Order By',
        flex: 0.5,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'contestId',
        headerName: 'Contest Id',
        flex: 2,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
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
        renderCell: (params) => (
            <ExternalLink
              to={getContestsDetailsPageUrl({
                  contestId: params.row.contestId,
                  contestName: params.row.contest,
              })}
              text={params.value.toString()}
            />
        ),
    },
    {
        field: 'problemGroupId',
        headerName: 'Problem Group Id',
        flex: 0.5,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'problemGroupOrderBy',
        headerName: 'Problem Group Order By',
        flex: 0.5,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'problemGroupType',
        headerName: 'Problem Group Type',
        flex: 1,
        type: 'enum',
        filterable: false,
        align: 'center',
        sortable: false,
        headerAlign: 'center',
        enumValues: getStringObjectKeys(ProblemGroupTypes),
        valueFormatter: (params) => ProblemGroupTypes[params.value],
    } as GridColDef & IEnumType,
    {
        field: 'practiceTestsCount',
        headerName: 'Practice Tests',
        flex: 0.5,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'competeTestsCount',
        headerName: 'Compete Tests',
        flex: 0.5,
        type: 'number',
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

export const returnProblemsNonFilterableColumns = (
    onEditClick: Function,
    setParentSuccessMessage: Function,
    onCopyProblem?: Function,
    retestProblem?: Function,
    onSuccessfulDelete?:() => void,
) => [
    {
        field: 'actions',
        headerName: 'Actions',
        flex: 1.5,
        minWidth: 250,
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <QuickEditButton onEdit={() => onEditClick(Number(params.row.id))} />
                <RedirectButton path={`/${NEW_ADMINISTRATION_PATH}/${PROBLEMS_PATH}/${Number(params.row.id)}`} location={`${EDIT} page`} />
                <DeleteButton
                  id={Number(params.row.id)}
                  name={params.row.name}
                  text={DELETE_CONFIRMATION_MESSAGE}
                  mutation={useDeleteProblemMutation}
                  onSuccess={onSuccessfulDelete}
                  setParentSuccessMessage={setParentSuccessMessage}
                />
                {retestProblem && (
                <Tooltip title="Retest">
                    <IconButton onClick={() => retestProblem(Number(params.row.id))}>
                        <ReplayIcon />
                    </IconButton>
                </Tooltip>
                )}
                {onCopyProblem && (
                <Tooltip title="Copy">
                    <IconButton onClick={() => onCopyProblem(Number(params.row.id))}>
                        <FaCopy />
                    </IconButton>
                </Tooltip>
                )}
            </div>
        ),
    },
] as GridColDef[];

export default problemFilterableColumns;
