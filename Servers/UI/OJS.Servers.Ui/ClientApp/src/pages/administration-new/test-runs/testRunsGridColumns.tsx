/* eslint-disable @typescript-eslint/ban-types */
import { Link } from 'react-router-dom';
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { TestRunResultType } from '../../../common/constants';
import { IEnumType } from '../../../common/types';
import { AdministrationGridColDef } from '../../../components/administration/utils/mui-utils';
import IconSize from '../../../components/guidelines/icons/common/icon-sizes';
import ErrorIcon from '../../../components/guidelines/icons/ErrorIcon';
import MemoryIcon from '../../../components/guidelines/icons/MemoryIcon';
import RuntimeErrorIcon from '../../../components/guidelines/icons/RuntimeErrorIcon';
import TickIcon from '../../../components/guidelines/icons/TickIcon';
import TimeLimitIcon from '../../../components/guidelines/icons/TimeLimitIcon';
import WrongAnswerIcon from '../../../components/guidelines/icons/WrongAnswerIcon';
import { getStringObjectKeys } from '../../../utils/object-utils';

const testRunsFilterableColumns: AdministrationGridColDef[] = [
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
        field: 'timeUsed',
        headerName: 'Time Used',
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => `${(params.value * 0.001).toFixed(3)} sec`,
    },
    {
        field: 'memoryUsed',
        headerName: 'Memory Used',
        flex: 1,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        valueFormatter: (params) => `${(params.value * 0.000001).toFixed(2)} MB`,
    },
    {
        field: 'executionComment',
        headerName: 'Execution Comment',
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'checkerComment',
        headerName: 'Checker Comment',
        flex: 1,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'resultType',
        headerName: 'Result Type',
        flex: 1,
        type: 'enum',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        enumValues: getStringObjectKeys(TestRunResultType),
        renderCell: (params: GridRenderCellParams) => {
            switch (params.row.resultType) {
            case TestRunResultType.CorrectAnswer: return <TickIcon size={IconSize.Large} />;
            case TestRunResultType.WrongAnswer: return <WrongAnswerIcon size={IconSize.Large} />;
            case TestRunResultType.MemoryLimit: return <MemoryIcon size={IconSize.Large} />;
            case TestRunResultType.TimeLimit: return <TimeLimitIcon size={IconSize.Large} />;
            case TestRunResultType.RunTimeError: return <RuntimeErrorIcon size={IconSize.Large} />;
            default: return (
                <ErrorIcon size={IconSize.Large} />
            );
            }
        },
    } as GridColDef & IEnumType,
    {
        field: 'submissionId',
        headerName: 'Submission Id',
        flex: 1,
        minWidth: 150,
        type: 'number',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
        renderCell: (params: GridRenderCellParams) => (
            <Link to={`/submissions/${params.row.submissionId}/details`} target="_blank">
                {params.row.submissionId}
            </Link>
        ),
    },
];

export default testRunsFilterableColumns;
