
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import QuickEditButton from 'src/components/administration/common/edit/QuickEditButton';

import {
    MENTOR_PROMPT_TEMPLATE,
    MENTOR_PROMPT_TEMPLATE_ID,
    MENTOR_PROMPT_TEMPLATE_TITLE,
} from '../../../common/labels';
import { AdministrationGridColDef } from '../../../components/administration/utils/mui-utils';

const mentorPromptTemplatesFilterableColumns: AdministrationGridColDef[] = [
    {
        field: 'id',
        headerName: MENTOR_PROMPT_TEMPLATE_ID,
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (_, row) => row.value?.toString(),
    },
    {
        field: 'title',
        headerName: MENTOR_PROMPT_TEMPLATE_TITLE,
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'template',
        headerName: MENTOR_PROMPT_TEMPLATE,
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
];

export const returnNonFilterableColumns = (onEditClick: (id: number) => void) => [
    {
        field: 'actions',
        headerName: 'Actions',
        flex: 1,
        minWidth: 150,
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) =>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <QuickEditButton onEdit={() => onEditClick(params.row.id)} />
            </div>
        ,
    },
] as GridColDef[];

export default mentorPromptTemplatesFilterableColumns;
