/* eslint-disable @typescript-eslint/ban-types */
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { getContestsDetailsPageUrl } from 'src/common/urls/compose-client-urls';
import ExternalLink from 'src/components/guidelines/buttons/ExternalLink';

import { CREATED_ON, MODIFIED_ON } from '../../../common/labels';
import { DELETE_CONFIRMATION_MESSAGE } from '../../../common/messages';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import { AdministrationGridColDef } from '../../../components/administration/utils/mui-utils';
import { useDeleteParticipantMutation } from '../../../redux/services/admin/participantsAdminService';
import { adminFormatDate } from '../../../utils/administration/administration-dates';

const participantsFilteringColumns: AdministrationGridColDef[] = [
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
        valueFormatter: (_, row) => row.value?.toString(),
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
        field: 'contestId',
        headerName: 'Contest Id',
        width: 10,
        headerAlign: 'center',
        align: 'center',
        type: 'string',
        flex: 1,
        filterable: false,
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
        renderCell: (params) => (
            <ExternalLink
              to={getContestsDetailsPageUrl({
                  contestId: params.row.contestId,
                  contestName: params.row.contestName,
              })}
              text={params.value.toString()}
            />
        ),
    },
    {
        field: 'isOfficial',
        headerName: 'Is Official',
        headerAlign: 'center',
        width: 10,
        align: 'center',
        type: 'boolean',
        flex: 1,
        filterable: false,
        sortable: false,
    },
    {
        field: 'isInvalidated',
        headerName: 'Is Invalidated',
        headerAlign: 'center',
        width: 10,
        align: 'center',
        type: 'boolean',
        flex: 1,
        filterable: false,
        sortable: false,
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
    {
        field: 'participationStartTime',
        headerName: 'Participation Start Time',
        type: 'date',
        flex: 1,
        filterable: false,
        sortable: false,
        valueFormatter: (_, row) => adminFormatDate(row.value),
    },
    {
        field: 'participationEndTime',
        headerName: 'Participation End Time',
        type: 'date',
        flex: 1,
        filterable: false,
        sortable: false,
        valueFormatter: (_, row) => adminFormatDate(row.value),
    },
    {
        field: 'lastSubmissionTime',
        headerName: 'Last Submission Time',
        type: 'date',
        flex: 1,
        filterable: false,
        sortable: false,
        valueFormatter: (_, row) => adminFormatDate(row.value),
    },
];

export const returnparticipantsNonFilterableColumns = (
    onSuccessfulDelete: () => void,
    setParentSuccessMessage: Function,
) => [
    {
        field: 'actions',
        headerName: 'Actions',
        flex: 1,
        minWidth: 50,
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
                  mutation={useDeleteParticipantMutation}
                  onSuccess={onSuccessfulDelete}
                  setParentSuccessMessage={setParentSuccessMessage}
                />
            </div>
        ),
    },
] as GridColDef[];

export default participantsFilteringColumns;
