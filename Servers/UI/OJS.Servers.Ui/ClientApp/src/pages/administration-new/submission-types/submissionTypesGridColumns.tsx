
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { CompilerType, SubmissionStrategyType } from '../../../common/enums';
import {
    ALLOW_BINARY_FILES_UPLOAD,
    ALLOWED_FILE_EXTENSIONS,
    BASE_MEMORY_USED,
    BASE_TIME_USED,
    COMPILER, EXECUTION_STRATEGY, ID, MAX_ALLOWED_MEMORY_LIMIT, MAX_ALLOWED_TIME_LIMIT, NAME,
    PROBLEM_GROUP,
} from '../../../common/labels';
import { DELETE_CONFIRMATION_MESSAGE } from '../../../common/messages';
import { IEnumType } from '../../../common/types';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import QuickEditButton from '../../../components/administration/common/edit/QuickEditButton';
import { AdministrationGridColDef } from '../../../components/administration/utils/mui-utils';
import { useDeleteSubmissionTypeMutation } from '../../../redux/services/admin/submissionTypesAdminService';
import { getStringObjectKeys } from '../../../utils/object-utils';

const submissionTypesFilterableColumns: AdministrationGridColDef[] = [
    {
        field: 'id',
        headerName: ID,
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'name',
        headerName: NAME,
        flex: 3,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'executionStrategyType',
        headerName: EXECUTION_STRATEGY,
        flex: 3,
        type: 'enum',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        enumValues: getStringObjectKeys(SubmissionStrategyType),
        valueFormatter: (params) => SubmissionStrategyType[params.value],

    } as GridColDef & IEnumType,
    {
        field: 'compilerType',
        headerName: COMPILER,
        flex: 1,
        type: 'enum',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        enumValues: getStringObjectKeys(CompilerType),
        valueFormatter: (params) => CompilerType[params.value],
    } as GridColDef & IEnumType,
    {
        field: 'allowedFileExtensions',
        headerName: ALLOWED_FILE_EXTENSIONS,
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'baseTimeUsedInMilliseconds',
        headerName: BASE_TIME_USED,
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        hidden: true,
    },
    {
        field: 'baseMemoryUsedInBytes',
        headerName: BASE_MEMORY_USED,
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        hidden: true,
    },
    {
        field: 'maxAllowedTimeLimitInMilliseconds',
        headerName: MAX_ALLOWED_TIME_LIMIT,
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        hidden: true,
    },
    {
        field: 'maxAllowedMemoryLimitInBytes',
        headerName: MAX_ALLOWED_MEMORY_LIMIT,
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        hidden: true,
    },
    {
        field: 'allowBinaryFilesUpload',
        headerName: ALLOW_BINARY_FILES_UPLOAD,
        flex: 1,
        type: 'boolean',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        hidden: true,
    },
];

export const returnNonFilterableColumns = (
    onEditClick: Function,
    onSuccessFullDelete: () => void,
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
        renderCell: (params: GridRenderCellParams) =>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <QuickEditButton onEdit={() => onEditClick(Number(params.row.id))} />
                <DeleteButton
                  id={Number(params.row.id)}
                  name={`${PROBLEM_GROUP}`}
                  text={DELETE_CONFIRMATION_MESSAGE}
                  mutation={useDeleteSubmissionTypeMutation}
                  onSuccess={onSuccessFullDelete}
                  setParentSuccessMessage={setParentSuccessMessage}
                />
            </div>
        ,
    } ] as GridColDef[];

export default submissionTypesFilterableColumns;
