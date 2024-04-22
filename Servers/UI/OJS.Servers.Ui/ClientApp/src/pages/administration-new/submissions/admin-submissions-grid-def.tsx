/* eslint-disable @typescript-eslint/ban-types */
import { Link } from 'react-router-dom';
import { IconButton, Tooltip } from '@mui/material';
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { CREATED_ON, MODIFIED_ON, VIEW } from '../../../common/labels';
import { CONTESTS_PATH, NEW_ADMINISTRATION_PATH, PROBLEMS_PATH } from '../../../common/urls/administration-urls';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import ViewRedirectButton from '../../../components/administration/common/edit/ViewRedirectButton';
import IconSize from '../../../components/guidelines/icons/common/icon-sizes';
import DownloadIcon from '../../../components/guidelines/icons/DownloadIcon';
import RefreshIcon from '../../../components/guidelines/icons/RefreshIcon';
import { useDeleteSubmissionMutation } from '../../../redux/services/admin/submissionsAdminService';
import { adminFormatDate } from '../../../utils/administration/administration-dates';

const dataColumns: GridColDef[] = [
    {
        field: 'id',
        headerName: 'Id',
        type: 'number',
        align: 'center',
        headerAlign: 'center',
        flex: 1,
        filterable: false,
        sortable: false,
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'participantName',
        headerName: 'Participant Name',
        align: 'center',
        headerAlign: 'center',
        type: 'string',
        flex: 1.5,
        filterable: false,
        sortable: false,
    },
    {
        field: 'problemName',
        headerName: 'Problem Name',
        align: 'center',
        headerAlign: 'center',
        type: 'string',
        flex: 2,
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <Link
              to={`/${NEW_ADMINISTRATION_PATH}/${PROBLEMS_PATH}/${Number(params.row?.problemId)}`}
            >
                {params.row?.problemName}
            </Link>
        ),
    },
    {
        field: 'contestName',
        headerName: 'Contest Name',
        headerAlign: 'center',
        type: 'string',
        flex: 2,
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <Link
              to={`/${NEW_ADMINISTRATION_PATH}/${CONTESTS_PATH}/${Number(params.row?.contestId)}`}
            >
                {params.row?.contestName}
            </Link>
        ),
    },
    {
        field: 'submissionTypeName',
        headerName: 'Submission Type Name',
        align: 'center',
        headerAlign: 'center',
        type: 'string',
        flex: 2,
        filterable: false,
        sortable: false,
    },
    {
        field: 'processed',
        headerName: 'Processed',
        align: 'center',
        headerAlign: 'center',
        type: 'boolean',
        flex: 2,
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                {params.value === true
                    ? 'Processed'
                    : 'Pending'}
            </div>
        ),
    },
    {
        field: 'points',
        headerName: 'Points',
        align: 'center',
        headerAlign: 'center',
        type: 'number',
        flex: 0.5,
        filterable: false,
        sortable: false,
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
        valueFormatter: (params) => adminFormatDate(params.value),
        sortable: false,
    },
];

export const returnSubmissionsNonFilterableColumns = (
    retest: Function,
    downloadClicked: Function,
    onSuccessFullyDelete:() => void,
) => [
    {
        field: 'actions',
        headerName: 'Actions',
        flex: 2,
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <div style={{ display: 'flex', alignItems: 'start', justifyContent: 'space-between' }}>
                <Tooltip title="Retest">
                    <IconButton
                      onClick={() => retest(Number(params.row.id))}
                    >
                        <RefreshIcon size={IconSize.Large} />
                    </IconButton>
                </Tooltip>
                <ViewRedirectButton
                  path={`/submissions/${Number(params.row.id)}/details`}
                  location={VIEW}
                />
                <DeleteButton
                  id={Number(params.row.id)}
                  name="Submission"
                  text={`Are you sure that you want to delete submission #${params.row.id}?`}
                  mutation={useDeleteSubmissionMutation}
                  onSuccess={onSuccessFullyDelete}
                />

                <Tooltip title="Download">
                    <span>
                        <IconButton
                          disabled={!params.row.isBinaryFile}
                          onClick={() => downloadClicked(Number(params.row.id))}
                        >
                            <DownloadIcon size={IconSize.Large} />
                        </IconButton>
                    </span>
                </Tooltip>

            </div>
        ),
    },
] as GridColDef[];

export default dataColumns;
