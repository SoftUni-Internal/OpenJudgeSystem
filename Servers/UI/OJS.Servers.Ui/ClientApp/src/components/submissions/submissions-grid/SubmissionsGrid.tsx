import { Dispatch, SetStateAction, useCallback, useMemo, useState } from 'react';
import { SetURLSearchParams } from 'react-router-dom';
import isEmpty from 'lodash/isEmpty';
import { IDictionary } from 'src/common/common-types';
import { FilterColumnTypeEnum } from 'src/common/enums';
import { IGetSubmissionsUrlParams } from 'src/common/url-types';
import ColumnFilters, { IFilter, mapUrlToFilters } from 'src/components/filters/ColumnFilters';

import { IPagedResultType, IPublicSubmission } from '../../../common/types';
import useTheme from '../../../hooks/use-theme';
import concatClassNames from '../../../utils/class-names';
import { IHaveOptionalClassName } from '../../common/Props';
import PaginationControls from '../../guidelines/pagination/PaginationControls';
import SpinningLoader from '../../guidelines/spinning-loader/SpinningLoader';
import SubmissionGridRow from '../submission-grid-row/SubmissionGridRow';

import styles from './SubmissionsGrid.module.scss';

interface ISubmissionsGridProps extends IHaveOptionalClassName {
    isDataLoaded: boolean;
    submissions?: IPagedResultType<IPublicSubmission>;
    handlePageChange: (page: number) => void;
    options: ISubmissionsGridOptions;
    searchParams: URLSearchParams;
    setSearchParams: SetURLSearchParams;
    setQueryParams: Dispatch<SetStateAction<IGetSubmissionsUrlParams>>;
}

interface ISubmissionsGridOptions {
    showTaskDetails: boolean;
    showDetailedResults: boolean;
    showCompeteMarker: boolean;
    showSubmissionTypeInfo: boolean;
    showParticipantUsername: boolean;
}

const SubmissionsGrid = ({
    className,
    isDataLoaded,
    submissions,
    handlePageChange,
    options,
    searchParams,
    setSearchParams,
    setQueryParams,
}: ISubmissionsGridProps) => {
    const { isDarkMode, getColorClassName, themeColors } = useTheme();

    const [ selectedFilters, setSelectedFilters ] = useState<IDictionary<Array<IFilter>>>(mapUrlToFilters(searchParams, [
        { name: 'Id', id: 'Id', columnType: FilterColumnTypeEnum.NUMBER },
        { name: 'ProblemName', id: 'Problem.Name', columnType: FilterColumnTypeEnum.STRING },
        { name: 'StrategyName', id: 'StrategyName', columnType: FilterColumnTypeEnum.STRING },
        { name: 'CreatedOn', id: 'CreatedOn', columnType: FilterColumnTypeEnum.DATE },
    ]));
    const [ openFilter, setOpenFilter ] = useState<string | null>(null);
    const areItemsAvailable = useMemo(() => !isEmpty(submissions?.items), [ submissions?.items ]);

    const onPageChange = (page: number) => {
        handlePageChange(page);
    };

    const handleToggleFilter = (filterId: string | null) => {
        setOpenFilter((prevId) => (prevId === filterId
            ? null
            : filterId));
    };

    const headerClassName = concatClassNames(
        styles.submissionsGridHeader,
        isDarkMode
            ? styles.darkSubmissionsGridHeader
            : styles.lightSubmissionsGridHeader,
        getColorClassName(themeColors.textColor),
    );

    const getColspan = useCallback(() => {
        let colspan = 4;
        if (options.showCompeteMarker) { colspan += 1; }
        if (options.showDetailedResults) { colspan += 1; }
        if (options.showSubmissionTypeInfo) { colspan += 1; }
        if (areItemsAvailable) { colspan += 1; }
        return colspan;
    }, [ options.showCompeteMarker, options.showDetailedResults, options.showSubmissionTypeInfo, submissions?.items ]);

    const renderSubmissionsGrid = useCallback(() => (
        <table className={concatClassNames(className, styles.submissionsGrid)}>
            <thead>
                <tr className={headerClassName}>
                    <td>
                        <div className={styles.header}>
                            <ColumnFilters
                              filterColumn={{ id: 'Id', name: 'Id', columnType: FilterColumnTypeEnum.NUMBER }}
                              allFilters={selectedFilters}
                              setSearchParams={setSearchParams}
                              searchParams={searchParams}
                              setAllFilters={setSelectedFilters}
                              setQueryParams={setQueryParams}
                              openFilter={openFilter}
                              onToggleFilter={handleToggleFilter}
                            />
                            ID
                        </div>
                    </td>
                    <td>
                        <div className={styles.header}>
                            <ColumnFilters
                              filterColumn={{
                                  id: 'Problem.Name',
                                  name: 'ProblemName',
                                  columnType: FilterColumnTypeEnum.STRING,
                              }}
                              allFilters={selectedFilters}
                              setSearchParams={setSearchParams}
                              searchParams={searchParams}
                              setAllFilters={setSelectedFilters}
                              setQueryParams={setQueryParams}
                              openFilter={openFilter}
                              onToggleFilter={handleToggleFilter}
                            />
                            {options.showTaskDetails
                                ? 'Task'
                                : ''}
                        </div>
                    </td>
                    <td>
                        <div className={styles.header}>
                            <ColumnFilters
                              filterColumn={{
                                  id: 'CreatedOn',
                                  name: 'CreatedOn',
                                  columnType: FilterColumnTypeEnum.DATE,
                              }}
                              allFilters={selectedFilters}
                              setSearchParams={setSearchParams}
                              searchParams={searchParams}
                              setAllFilters={setSelectedFilters}
                              setQueryParams={setQueryParams}
                              openFilter={openFilter}
                              onToggleFilter={handleToggleFilter}
                            />
                            From
                        </div>
                    </td>
                    {options.showCompeteMarker && <td />}
                    {options.showDetailedResults && <td>Time and Memory Used</td>}
                    <td>Result</td>
                    {options.showSubmissionTypeInfo && (
                    <td>
                        <div className={styles.header}>
                            <ColumnFilters
                              filterColumn={{
                                  id: 'StrategyName',
                                  name: 'StrategyName',
                                  columnType: FilterColumnTypeEnum.STRING,
                              }}
                              allFilters={selectedFilters}
                              setSearchParams={setSearchParams}
                              searchParams={searchParams}
                              setAllFilters={setSelectedFilters}
                              setQueryParams={setQueryParams}
                              openFilter={openFilter}
                              onToggleFilter={handleToggleFilter}
                            />
                            Strategy
                        </div>
                    </td>
                    )}
                    {areItemsAvailable && <td />}
                </tr>
            </thead>
            <tbody>
                {!isDataLoaded
                    ? (
                        <tr>
                            <td colSpan={getColspan()} style={{ textAlign: 'center', padding: '10px' }}>
                                <SpinningLoader />
                            </td>
                        </tr>
                    )
                    : !areItemsAvailable
                        ? (
                            <tr className={styles.noSubmissionsRow}>
                                <td colSpan={getColspan()}>
                                    <div className={styles.noSubmissionsContainer}>
                                        No submissions yet.
                                    </div>
                                </td>
                            </tr>
                        )
                        : submissions?.items?.map((s) => <SubmissionGridRow submission={s} options={options} key={s.id} />)}
            </tbody>
        </table>
    ), [
        className,
        headerClassName,
        selectedFilters,
        setSearchParams,
        searchParams,
        setQueryParams,
        openFilter,
        options,
        isDataLoaded,
        getColspan,
        submissions?.items,
        areItemsAvailable ]);

    return (
        <>
            {renderSubmissionsGrid()}
            {submissions && areItemsAvailable && submissions?.pagesCount !== 0 && (
                <PaginationControls
                  count={submissions.pagesCount}
                  page={submissions.pageNumber}
                  onChange={onPageChange}
                />
            )}
        </>
    );
};

export type { ISubmissionsGridOptions };

export default SubmissionsGrid;
