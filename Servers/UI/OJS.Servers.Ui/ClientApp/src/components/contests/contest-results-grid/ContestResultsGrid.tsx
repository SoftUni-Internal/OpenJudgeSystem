import React, { useCallback } from 'react';
import { useSelector } from 'react-redux';
import { useParams } from 'react-router-dom';
import { Tooltip } from '@mui/material';
import isEmpty from 'lodash/isEmpty';
import isNil from 'lodash/isNil';

import { ContestParticipationType } from '../../../common/constants';
import { getContestsSolutionSubmitPageUrl } from '../../../common/urls/compose-client-urls';
import { IContestResultsParticipationType, IContestResultsType } from '../../../hooks/contests/types';
import useTheme from '../../../hooks/use-theme';
import { IAuthorizationReduxState } from '../../../redux/features/authorizationSlice';
import isNilOrEmpty from '../../../utils/check-utils';
import concatClassNames from '../../../utils/class-names';
import { ButtonSize, LinkButton, LinkButtonType } from '../../guidelines/buttons/Button';

import styles from './ContestResultsGrid.module.scss';

interface IContestResultsGridProps {
    items: IContestResultsType | null;
}

const ContestResultsGrid = ({ items }: IContestResultsGridProps) => {
    const { isDarkMode, getColorClassName, themeColors } = useTheme();
    const { participationType } = useParams();
    const isCompete = participationType === ContestParticipationType.Compete;

    const { internalUser } = useSelector((state: {authorization: IAuthorizationReduxState}) => state.authorization);

    const getColumns = useCallback((results: IContestResultsType) => {
        const problemResultColumns = results.problems?.map((p) => p.name);

        return [ '№' ]
            .concat('Participants')
            .concat(problemResultColumns)
            .concat('Total Result');
    }, []);

    const headerClassName = concatClassNames(
        styles.contestResultsGridHeader,
        isDarkMode
            ? styles.darkContestResultsGridHeader
            : styles.lightContestResultsGridHeader,
        getColorClassName(themeColors.textColor),
    );

    const rowClassName = concatClassNames(
        styles.row,
        isDarkMode
            ? styles.darkRow
            : styles.lightRow,
        getColorClassName(themeColors.textColor),
    );

    const renderParticipantResult = useCallback((participantResult: IContestResultsParticipationType, problemId: number) => {
        const problemResult = participantResult
            .problemResults
            .find((pr) => pr.problemId === problemId);

        const bestSubmission = problemResult?.bestSubmission;

        return (items!.userIsInRoleForContest || participantResult.participantUsername === internalUser.userName) && !isNil(bestSubmission)
            ? <td key={`p-r-i-${problemId}`}>
                <LinkButton
                      className={styles.resultLink}
                      type={LinkButtonType.plain}
                      size={ButtonSize.small}
                      text={`${bestSubmission.points}`}
                      to={`/submissions/${bestSubmission.id}/details`}
                    />
            </td>

            : <td
                  key={`p-r-i-${problemId}`}
                >
                {bestSubmission?.points || '-'}
            </td>
        ;
    }, [ items, internalUser.userName ]);

    return (
        <div className={styles.tableContainer}>
            <table className={styles.contestResultsGrid}>
                <thead>
                    <tr className={headerClassName}>
                        {
                        getColumns(items!).map((column, idx) => {
                            if (idx <= 1) { // Skip the first two columns (№ and Participants)
                                return (<td key={`t-r-i-${idx}`}>{column}</td>);
                            }

                            if (idx === getColumns(items!).length - 1) { // Skip the last column (Total Result)
                                return (<td key={`t-r-i-${idx}`}>{column}</td>);
                            }

                            // For problem names, create a link button
                            const problem = items!.problems![idx - 2]; // -2 because we skipped the first two columns
                            if (column.length > 20) {
                                return (
                                    <td key={`t-r-i-${idx}`}>
                                        <Tooltip title={column}>
                                            <LinkButton
                                              type={LinkButtonType.plain}
                                              text={`${column.substring(0, 19)}...`}
                                              to={getContestsSolutionSubmitPageUrl({
                                                  isCompete,
                                                  contestId: items!.id,
                                                  contestName: items!.name,
                                                  problemId: problem.id,
                                                  orderBy: problem.orderBy,
                                              })}
                                              className={styles.problemLink}
                                            />
                                        </Tooltip>
                                    </td>
                                );
                            }

                            return (
                                <td key={`t-r-i-${idx}`}>
                                    <LinkButton
                                      type={LinkButtonType.plain}
                                      text={column}
                                      to={getContestsSolutionSubmitPageUrl({
                                          isCompete,
                                          contestId: items!.id,
                                          contestName: items!.name,
                                          problemId: problem.id,
                                          orderBy: problem.orderBy,
                                      })}
                                      className={styles.problemLink}
                                    />
                                </td>
                            );
                        })
                    }
                    </tr>
                </thead>
                <tbody>
                    {
                    !isNil(items) && !isEmpty(items) && (items.pagedResults.items ?? []).map((participantResult, index) =>
                        <tr
                          key={`t-r-i-${participantResult.participantUsername}`}
                          className={concatClassNames(
                              rowClassName,
                              participantResult.participantUsername === internalUser.userName
                                  ? styles.userRow
                                  : '',
                          )}
                        >
                            <td>{items.pagedResults.itemsPerPage * (items.pagedResults.pageNumber - 1) + index + 1}</td>
                            <td>{participantResult.participantUsername}</td>
                            {
                                    items?.problems
                                        .map((p) => renderParticipantResult(participantResult, p.id))
                                }
                            {
                                    isNilOrEmpty(participantResult.problemResults)
                                        ? <td>0</td>
                                        : <td>{participantResult.total}</td>
                                }
                        </tr>)
                    }
                </tbody>
            </table>
        </div>
    );
};

export default ContestResultsGrid;
