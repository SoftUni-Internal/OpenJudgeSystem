import React, { useCallback } from 'react';
import { useSelector } from 'react-redux';
import { Tooltip } from '@mui/material';
import isEmpty from 'lodash/isEmpty';
import isNil from 'lodash/isNil';

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

        const underlinedPointsResultStyle = isDarkMode
            ? styles.underlinedPointsResultDark
            : styles.underlinedPointsResultLight;

        const notUnderlinedPointsResultStyle = isDarkMode
            ? styles.notUnderlinedPointsResultDark
            : styles.notUnderlinedPointsResultLight;

        return (items!.userIsInRoleForContest || participantResult.participantUsername === internalUser.userName) && !isNil(bestSubmission)
            ? (
                <td key={`p-r-i-${problemId}`}>
                    <LinkButton
                      className={underlinedPointsResultStyle}
                      type={LinkButtonType.plain}
                      size={ButtonSize.small}
                      text={`${bestSubmission.points}`}
                      to={`/submissions/${bestSubmission.id}/details`}
                    />
                </td>
            )
            : (
                <td
                  key={`p-r-i-${problemId}`}
                  className={notUnderlinedPointsResultStyle}
                >
                    {bestSubmission?.points || '-'}
                </td>
            );
    }, [ items, internalUser.userName, isDarkMode ]);

    return (
        <div className={styles.tableContainer}>
            <table className={styles.contestResultsGrid}>
                <thead>
                    <tr className={headerClassName}>
                        {
                        getColumns(items!).map((column, idx) => {
                            if (column.length > 20) {
                                return (
                                    <td key={`t-r-i-${idx}`}>
                                        <Tooltip title={column}>
                                            <p>{`${column.substring(0, 19)}...`}</p>
                                        </Tooltip>
                                    </td>
                                );
                            }

                            return (<td key={`t-r-i-${idx}`}>{column}</td>);
                        })
                    }
                    </tr>
                </thead>
                <tbody>
                    {
                    !isNil(items) && !isEmpty(items) && items.results.map((participantResult, index) => (
                        <tr
                          key={`t-r-i-${participantResult.participantUsername}`}
                          className={concatClassNames(
                              rowClassName,
                              participantResult.participantUsername === internalUser.userName
                                  ? isDarkMode
                                      ? styles.userRowDark
                                      : styles.userRowLight
                                  : '',
                          )}
                        >
                            <td>{index + 1}</td>
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
                        </tr>
                    ))
                }
                </tbody>
            </table>
        </div>
    );
};

export default ContestResultsGrid;
