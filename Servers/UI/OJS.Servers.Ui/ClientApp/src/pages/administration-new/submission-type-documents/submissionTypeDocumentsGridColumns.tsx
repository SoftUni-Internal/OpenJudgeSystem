/* eslint-disable @typescript-eslint/ban-types,max-len */
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import {
    EDIT,
    SUBMISSION_TYPE_DOCUMENT,
    SUBMISSION_TYPE_DOCUMENT_ID,
    SUBMISSION_TYPE_DOCUMENT_TITLE, SUBMISSION_TYPE_ID,
    SUBMISSION_TYPE_NAME,
} from '../../../common/labels';
import {
    DELETE_CONFIRMATION_MESSAGE,
    SUBMISSION_TYPE_DOCUMENT_DELETE_CONFIRMATION_MESSAGE,
} from '../../../common/messages';
import {
    NEW_ADMINISTRATION_PATH,
    SUBMISSION_TYPE_DOCUMENTS_PATH, SUBMISSION_TYPE_DOCUMENTS_VIEW_PATH,
} from '../../../common/urls/administration-urls';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import RedirectButton from '../../../components/administration/common/edit/RedirectButton';
import ViewButton from '../../../components/administration/common/view/ViewButton';
import { AdministrationGridColDef } from '../../../components/administration/utils/mui-utils';
import {
    useDeleteSubmissionTypeDocumentByIdAndSubmissionTypeIdMutation,
} from '../../../redux/services/admin/submissionTypeDocumentsAdminService';

const submissionTypeDocumentsFilterableColumns: AdministrationGridColDef[] = [
    {
        field: 'submissionTypeDocumentId',
        headerName: SUBMISSION_TYPE_DOCUMENT_ID,
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (_, row) => row.value?.toString(),
    },
    {
        field: 'submissionTypeDocumentTitle',
        headerName: SUBMISSION_TYPE_DOCUMENT_TITLE,
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'submissionTypeId',
        headerName: SUBMISSION_TYPE_ID,
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'submissionTypeName',
        headerName: SUBMISSION_TYPE_NAME,
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
];

export const returnNonFilterableColumns = (
    onSuccessfulDelete: () => void,
    setParentSuccessMessage: Function,
) => [
    {
        field: 'actions',
        headerName: 'Actions',
        flex: 1,
        minWidth: 150,
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <ViewButton
                  path={`/${NEW_ADMINISTRATION_PATH}/${SUBMISSION_TYPE_DOCUMENTS_VIEW_PATH}?submissionTypeIds=${params.row.submissionTypeId}`}
                  text={`View '${params.row.submissionTypeDocumentTitle}'`}
                />
                <RedirectButton
                  path={`/${NEW_ADMINISTRATION_PATH}/${SUBMISSION_TYPE_DOCUMENTS_PATH}/${Number(params.row.submissionTypeDocumentId)}?isEditMode=true`}
                  location={`${EDIT} page`}
                />
                <DeleteButton
                  id={{
                      firstEntityId: params.row.submissionTypeId,
                      secondEntityId: params.row.submissionTypeDocumentId,
                  }}
                  name={`${SUBMISSION_TYPE_DOCUMENT}`}
                  text={`${DELETE_CONFIRMATION_MESSAGE} ${SUBMISSION_TYPE_DOCUMENT_DELETE_CONFIRMATION_MESSAGE}`}
                  mutation={useDeleteSubmissionTypeDocumentByIdAndSubmissionTypeIdMutation}
                  onSuccess={onSuccessfulDelete}
                  setParentSuccessMessage={setParentSuccessMessage}
                />
            </div>
        ),
    },
] as GridColDef[];

export default submissionTypeDocumentsFilterableColumns;
