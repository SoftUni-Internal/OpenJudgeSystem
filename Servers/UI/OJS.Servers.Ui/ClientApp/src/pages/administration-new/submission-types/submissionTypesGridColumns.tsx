/* eslint-disable @typescript-eslint/ban-types */
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { CompilerType, SubmissionStrategyType } from '../../../common/enums';
import { PROBLEM_GROUP } from '../../../common/labels';
import { DELETE_CONFIRMATION_MESSAGE } from '../../../common/messages';
import { IEnumType } from '../../../common/types';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import QuickEditButton from '../../../components/administration/common/edit/QuickEditButton';
import { useDeleteSubmissionTypeMutation } from '../../../redux/services/admin/submissionTypesAdminService';
import { getStringObjectKeys } from '../../../utils/object-utils';

const submissionTypesFilterableColumns: GridColDef[] = [
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
        field: 'name',
        headerName: 'Name',
        flex: 2,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'executionStrategyType',
        headerName: 'Execution Strategy Type',
        flex: 2,
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
        headerName: 'Compiler Type',
        flex: 2,
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
        headerName: 'Allowed File Extensions',
        flex: 2,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'allowBinaryFilesUpload',
        headerName: 'Allow Binary Files Upload',
        flex: 2,
        type: 'boolean',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
];

export const returnNonFilterableColumns = (onEditClick: Function, onSuccessFullDelete: () => void) => [
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
                <DeleteButton
                  id={Number(params.row.id)}
                  name={`${PROBLEM_GROUP}`}
                  text={DELETE_CONFIRMATION_MESSAGE}
                  mutation={useDeleteSubmissionTypeMutation}
                  onSuccess={onSuccessFullDelete}
                />
            </div>
        ),
    } ] as GridColDef[];

export default submissionTypesFilterableColumns;
