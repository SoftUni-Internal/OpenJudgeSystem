import { useCallback } from 'react';
import isEmpty from 'lodash/isEmpty';
import isNil from 'lodash/isNil';

import { IPagedResultType, IPublicSubmission } from '../../../common/types';
import useTheme from '../../../hooks/use-theme';
import concatClassNames from '../../../utils/class-names';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
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
}: ISubmissionsGridProps) => {
    const { isDarkMode, getColorClassName, themeColors } = useTheme();

    const onPageChange = (page: number) => {
        handlePageChange(page);
    };

    const headerClassName = concatClassNames(
        styles.submissionsGridHeader,
        isDarkMode
            ? styles.darkSubmissionsGridHeader
            : styles.lightSubmissionsGridHeader,
        getColorClassName(themeColors.textColor),
    );

    const renderSubmissionsGrid = useCallback(
        () => {
            if (!isDataLoaded) {
                return (
                    <div style={{ ...flexCenterObjectStyles, marginTop: '10px' }}>
                        <SpinningLoader />
                    </div>
                );
            }

            if (isEmpty(submissions?.items)) {
                return (
                    // TODO: Make this reusable (centered themed text)
                    <div className={concatClassNames(
                        styles.noSubmissionsFound,
                        getColorClassName(themeColors.textColor),
                    )}
                    >
                        No submissions found.
                    </div>
                );
            }

            return (
                <table className={concatClassNames(className, styles.submissionsGrid)}>
                    <thead>
                        <tr className={headerClassName}>
                            <td>ID</td>
                            <td>
                                {options.showTaskDetails
                                    ? 'Task'
                                    : ''}
                            </td>
                            <td>From</td>
                            {
                            options.showCompeteMarker && <td />
                        }
                            {
                            options.showDetailedResults && <td>Time and Memory Used</td>
                        }
                            <td className={styles.tdRight}>Result</td>
                            {
                            options.showSubmissionTypeInfo && <td className={styles.tdRight}>Strategy</td>
                        }
                            <td />
                        </tr>
                    </thead>
                    <tbody>
                        {
                        !isNil(submissions?.items) && !isEmpty(submissions?.items)
                            ? submissions.items.map((s) => (
                                <SubmissionGridRow
                                  submission={s}
                                  options={options}
                                  key={s.id}
                                />
                            ))
                            : null
                        }
                    </tbody>
                </table>
            );
        },
        [ isDataLoaded, submissions, className, headerClassName, options, getColorClassName, themeColors.textColor ],
    );

    return (
        <>
            {renderSubmissionsGrid()}
            {!isEmpty(submissions) && submissions?.pagesCount !== 0 && (
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
