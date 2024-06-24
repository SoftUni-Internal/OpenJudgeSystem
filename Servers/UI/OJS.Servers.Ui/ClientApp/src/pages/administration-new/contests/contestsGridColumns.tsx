/* eslint-disable @typescript-eslint/ban-types */
import { FaCloudDownloadAlt } from 'react-icons/fa';
import { SiMicrosoftexcel } from 'react-icons/si';
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import {
    ALLOW_PARALLEL_SUBMISSIONS_IN_TASKS,
    CATEGORY,
    CATEGORY_ID,
    COMPETE_END_TIME,
    COMPETE_PASSWORD,
    COMPETE_START_TIME,
    CREATED_ON,
    EDIT,
    ID,
    IS_DELETED,
    IS_VISIBLE,
    LIMIT_BETWEEN_SUBMISSIONS,
    MODIFIED_ON,
    NAME,
    VISIBLE_FROM,
} from '../../../common/labels';
import { DELETE_CONFIRMATION_MESSAGE } from '../../../common/messages';
import { CONTESTS_PATH, NEW_ADMINISTRATION_PATH } from '../../../common/urls/administration-urls';
import AdministrationGridDropdown from '../../../components/administration/common/administration-grid-dropdown/AdministrationGridDropdown';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import QuickEditButton from '../../../components/administration/common/edit/QuickEditButton';
import RedirectButton from '../../../components/administration/common/edit/RedirectButton';
import ExternalLink from '../../../components/guidelines/buttons/ExternalLink';
import { useDeleteContestMutation } from '../../../redux/services/admin/contestsAdminService';
import { adminFormatDate } from '../../../utils/administration/administration-dates';

const contestFilterableColumns: GridColDef[] = [
    {
        field: 'id',
        headerName: `${ID}`,
        flex: 0.5,
        type: 'number',
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params) => (
            <ExternalLink to={`/${CONTESTS_PATH}/${params.row.id}`} text={params.value.toString()} />
        ),
    },
    {
        field: 'name',
        headerName: `${NAME}`,
        flex: 2,
        headerAlign: 'center',
        type: 'string',
        align: 'center',
        filterable: false,
        sortable: false,
    },
    {
        field: 'category',
        headerName: `${CATEGORY}`,
        align: 'center',
        type: 'string',
        filterable: false,
        headerAlign: 'center',
        sortable: false,
        flex: 2,
    },
    {
        field: 'categoryId',
        headerName: `${CATEGORY_ID}`,
        flex: 0.5,
        align: 'center',
        type: 'number',
        filterable: false,
        sortable: false,
    },
    {
        field: 'contestPassword',
        headerName: `${COMPETE_PASSWORD}`,
        flex: 1,
        align: 'center',
        type: 'string',
        filterable: false,
        sortable: false,
    },
    {
        field: 'startTime',
        headerName: `${COMPETE_START_TIME}`,
        flex: 1.5,
        align: 'center',
        type: 'date',
        headerAlign: 'center',
        filterable: false,
        sortable: false,
        valueFormatter: (params) => adminFormatDate(params.value),
    },
    {
        field: 'endTime',
        headerName: `${COMPETE_END_TIME}`,
        flex: 1.5,
        align: 'center',
        type: 'date',
        headerAlign: 'center',
        filterable: false,
        sortable: false,
        valueFormatter: (params) => adminFormatDate(params.value),
    },
    {
        field: 'limitBetweenSubmissions',
        headerName: `${LIMIT_BETWEEN_SUBMISSIONS}`,
        flex: 0,
        type: 'number',
        align: 'center',
        filterable: false,
        sortable: false,
    },
    {
        field: 'allowParallelSubmissionsInTasks',
        headerName: `${ALLOW_PARALLEL_SUBMISSIONS_IN_TASKS}`,
        type: 'boolean',
        flex: 0,
        filterable: false,
        sortable: false,
    },
    {
        field: 'isDeleted',
        headerName: `${IS_DELETED}`,
        type: 'boolean',
        flex: 0,
        filterable: false,
        sortable: false,
    },
    {
        field: 'isVisible',
        headerName: `${IS_VISIBLE}`,
        type: 'boolean',
        flex: 0,
        filterable: false,
        sortable: false,
    },
    {
        field: 'visibleFrom',
        headerName: `${VISIBLE_FROM}`,
        flex: 1.5,
        align: 'center',
        type: 'date',
        headerAlign: 'center',
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
        hideable: true,
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

export const returnContestsNonFilterableColumns = (
    onEditClick: Function,
    onSuccessDelete: () => void,
    onMoreClick: Function,
    onDownloadSubmissionClick: Function,
) => [
    {
        field: 'actions',
        headerName: 'Actions',
        flex: 1.5,
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <QuickEditButton onEdit={() => onEditClick(Number(params.row.id))} />
                <RedirectButton path={`/${NEW_ADMINISTRATION_PATH}/${CONTESTS_PATH}/${Number(params.row.id)}`} location={`${EDIT} page`} />
                <DeleteButton
                  id={Number(params.row.id)}
                  name={params.row.name}
                  text={DELETE_CONFIRMATION_MESSAGE}
                  mutation={useDeleteContestMutation}
                  onSuccess={onSuccessDelete}
                />
                <AdministrationGridDropdown
                  sections={
                    [
                        {
                            icon: <SiMicrosoftexcel />,
                            label: 'Export results',
                            handleClick: onMoreClick,
                        },
                        {
                            icon: <FaCloudDownloadAlt />,
                            label: 'Download submissions',
                            handleClick: onDownloadSubmissionClick,
                        },
                    ]
                }
                  id={Number(params.row.id)}
                />
            </div>
        ),
    },
] as GridColDef[];

export default contestFilterableColumns;
