/* eslint-disable @typescript-eslint/ban-types */
import { Link } from 'react-router-dom';
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { EDIT, TEST } from '../../../common/labels';
import { DELETE_CONFIRMATION_MESSAGE } from '../../../common/messages';
import { NEW_ADMINISTRATION_PATH, PROBLEMS_PATH, TESTS_PATH } from '../../../common/urls/administration-urls';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import QuickEditButton from '../../../components/administration/common/edit/QuickEditButton';
import RedirectButton from '../../../components/administration/common/edit/RedirectButton';
import { testTypesToFEStringsMapping } from '../../../components/administration/tests/types';
import { AdministrationGridColDef } from '../../../components/administration/utils/mui-utils';
import { useDeleteTestMutation } from '../../../redux/services/admin/testsAdminService';

const testsFilterableColums: AdministrationGridColDef[] = [
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
        field: 'orderBy',
        headerName: 'Order by',
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'isTrialTest',
        headerName: 'Is Trial Test',
        flex: 1,
        type: 'boolean',
        hidden: true,
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'isOpenTest',
        flex: 1,
        type: 'boolean',
        filterable: false,
        sortable: false,
        hidden: true,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'type',
        headerName: 'Test Type',
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => testTypesToFEStringsMapping[params.value],
    },
    {
        field: 'problemName',
        headerName: 'Problem Name',
        flex: 1,
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
        field: 'problemId',
        headerName: 'Problem Id',
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => params.value.toString(),
    },
];

export const returnTestsNonFilterableColumns = (
    onEditClick: Function,
    onSuccessfullDelete: () => void,
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
                <QuickEditButton onEdit={() => onEditClick(Number(params.row.id))} />
                <RedirectButton path={`/${NEW_ADMINISTRATION_PATH}/${TESTS_PATH}/${Number(params.row.id)}`} location={`${EDIT} page`} />
                <DeleteButton
                  id={Number(params.row.id)}
                  name={`${TEST}`}
                  text={DELETE_CONFIRMATION_MESSAGE}
                  mutation={useDeleteTestMutation}
                  onSuccess={onSuccessfullDelete}
                  setParentSuccessMessage={setParentSuccessMessage}
                />
            </div>
        ),
    },
] as GridColDef[];

export default testsFilterableColums;
