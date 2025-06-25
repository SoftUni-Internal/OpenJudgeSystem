import { ExcelFilterOperators } from 'src/common/enums';

import { IGetAllAdminParams } from '../../../../common/types';
import { getColors, useAdministrationTheme } from '../../../../hooks/use-administration-theme-provider';
import AdministrationGridView from '../../../../pages/administration-new/AdministrationGridView';
import problemFilterableColumns from '../../../../pages/administration-new/problems/problemGridColumns';
import { useGetAllAdminProblemsQuery, useLazyExportProblemsToExcelQuery } from '../../../../redux/services/admin/problemsAdminService';
import { DEFAULT_ITEMS_PER_PAGE } from '../../../../utils/constants';

interface IProblemsInProblemGroupViewProps {
    problemGroupId: number;
}

const ProblemsInProblemGroupView = (props: IProblemsInProblemGroupViewProps) => {
    const { problemGroupId } = props;
    const { themeMode } = useAdministrationTheme();
    const queryParams: IGetAllAdminParams = {
        page: 1,
        itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
        filter: `problemgroupid~equals~${problemGroupId}`,
        sorting: '',
    };

    const {
        data: problemData,
        error: problemDataError,
    } = useGetAllAdminProblemsQuery(queryParams);

    return (
        <AdministrationGridView
          filterableGridColumnDef={problemFilterableColumns}
          notFilterableGridColumnDef={[]}
          data={problemData}
          error={problemDataError}
          queryParams={queryParams}
          showFiltersAndSorters={false}
          legendProps={[ { color: getColors(themeMode).palette.deleted, message: 'Problem is deleted.' } ]}
          excelMutation={useLazyExportProblemsToExcelQuery}
          excelFilters={[ { propertyName: 'problemGroupId', operator: ExcelFilterOperators.Equals, value: problemGroupId } ]}
        />
    );
};
export default ProblemsInProblemGroupView;
