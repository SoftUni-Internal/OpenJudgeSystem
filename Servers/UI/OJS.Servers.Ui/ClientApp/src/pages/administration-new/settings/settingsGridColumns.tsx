/* eslint-disable @typescript-eslint/ban-types */
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { SettingTypeEnums } from '../../../common/enums';
import { DELETE_CONFIRMATION_MESSAGE } from '../../../common/messages';
import { IEnumType } from '../../../common/types';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import QuickEditButton from '../../../components/administration/common/edit/QuickEditButton';
import { getStringObjectKeys } from '../../../utils/object-utils';

const settingsFilterableColumns: GridColDef[] = [
    {
        field: 'id',
        headerName: 'Id',
        flex: 2,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => params.value.toString(),
    },
    {
        field: 'name',
        headerName: 'Name',
        flex: 3,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'value',
        headerName: 'Value',
        flex: 3,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'type',
        headerName: 'Type',
        flex: 3,
        type: 'enum',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        enumValues: getStringObjectKeys(SettingTypeEnums),
        valueFormatter: (params) => SettingTypeEnums[params.value],
    } as GridColDef & IEnumType,
];

export const returnSettingsNonFilterableColumns = (
    onEditClick: Function,
    deleteMutation: any,
    onDeleteSuccess?:() => void,
) => [
    {
        field: 'actions',
        headerName: 'Actions',
        flex: 1,
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <QuickEditButton onEdit={() => onEditClick(params.row.id)} />
                <DeleteButton
                  id={params.row.id}
                  name={params.row.name}
                  text={DELETE_CONFIRMATION_MESSAGE}
                  mutation={deleteMutation}
                  onSuccess={onDeleteSuccess}
                />
            </div>
        ),
    },
] as GridColDef[];

export default settingsFilterableColumns;
