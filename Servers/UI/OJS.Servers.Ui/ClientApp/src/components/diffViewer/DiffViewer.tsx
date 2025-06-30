/* eslint-disable no-plusplus */
import React from 'react';
import { diffChars } from 'diff';

import useTheme from '../../hooks/use-theme';

import styles from './DiffViewer.module.scss';

interface ITestsRunDiffProps {
    expectedStr: string;
    actualStr: string;
}

const ensureNewline = (str: string) => (str.endsWith('\n')
    ? str
    : `${str}\n`);

const renderSplitCharDiff = (expectedLine = '', actualLine = '', isDarkMode = false) => {
    const charDiff = diffChars(expectedLine, actualLine);

    const expected = charDiff.map((part, idx) => {
        if (part.removed) {
            return (
                <span
                  key={`exp-removed-${idx}`}
                  className={isDarkMode
                      ? `${styles.wordAdded} ${styles.darkDiff}`
                      : styles.wordAdded}
                >
                    {part.value}
                </span>
            );
        }

        if (!part.added) {
            return <span key={`exp-common-${idx}`}>{part.value}</span>;
        }

        return null;
    });

    const actual = charDiff.map((part, idx) => {
        if (part.added) {
            return (
                <span
                  key={`act-added-${idx}`}
                  className={isDarkMode
                      ? `${styles.wordRemoved} ${styles.darkDiff}`
                      : styles.wordRemoved}
                >
                    {part.value}
                </span>
            );
        }

        if (!part.removed) {
            return <span key={`act-common-${idx}`}>{part.value}</span>;
        }

        return null;
    });
    return { expected, actual };
};

const DiffViewer = ({ expectedStr = '', actualStr = '' }: ITestsRunDiffProps) => {
    const { isDarkMode } = useTheme();

    const expected = ensureNewline(expectedStr);
    const actual = ensureNewline(actualStr);

    // Compute line diff
    const expectedLines = expected.split('\n');
    const actualLines = actual.split('\n');
    const maxLines = Math.max(expectedLines.length, actualLines.length) - 1; // -1 because split adds an empty last line

    const rows: Array<{
        expectedLine: React.ReactNode;
        actualLine: React.ReactNode;
        key: string;
        expectedClass?: string;
        actualClass?: string;
        expectedLineNum?: number;
        actualLineNum?: number;
    }> = [];

    for (let i = 0, expNum = 1, actNum = 1; i < maxLines; i++) {
        const expLine = expectedLines[i] ?? '';
        const actLine = actualLines[i] ?? '';
        if (expLine === '' && actLine !== '') {
            // Only in actual (added line)
            rows.push({
                expectedLine: '',
                actualLine: <span>{actLine}</span>,
                key: `added-${actNum}`,
                expectedClass: styles.emptyCell,
                actualClass: isDarkMode
                    ? `${styles.lineAdded} ${styles.darkDiff}`
                    : styles.lineAdded,
                expectedLineNum: undefined,
                actualLineNum: actNum,
            });
            actNum++;
        } else if (actLine === '' && expLine !== '') {
            // Only in expected (removed line)
            rows.push({
                expectedLine: <span>{expLine}</span>,
                actualLine: '',
                key: `removed-${expNum}`,
                expectedClass: isDarkMode
                    ? `${styles.lineRemoved} ${styles.darkDiff}`
                    : styles.lineRemoved,
                actualClass: styles.emptyCell,
                expectedLineNum: expNum,
                actualLineNum: undefined,
            });
            expNum++;
        } else if (expLine !== actLine) {
            // Both exist, but different: highlight only changed chars
            const { expected: expectedText, actual: actualText } = renderSplitCharDiff(expLine, actLine, isDarkMode);
            rows.push({
                expectedLine: <span>{expectedText}</span>,
                actualLine: <span>{actualText}</span>,
                key: `mod-${expNum}-${actNum}`,
                expectedClass: isDarkMode
                    ? `${styles.lineRemoved} ${styles.darkDiff}`
                    : styles.lineRemoved,
                actualClass: isDarkMode
                    ? `${styles.lineAdded} ${styles.darkDiff}`
                    : styles.lineAdded,
                expectedLineNum: expNum,
                actualLineNum: actNum,
            });
            expNum++;
            actNum++;
        } else {
            // Unchanged line
            rows.push({
                expectedLine: <span>{expLine}</span>,
                actualLine: <span>{actLine}</span>,
                key: `common-${expNum}-${actNum}`,
                expectedLineNum: expNum,
                actualLineNum: actNum,
            });
            expNum++;
            actNum++;
        }
    }

    return (
        <div
          className={`${styles.diffWrapper} ${isDarkMode
              ? styles.darkDiff
              : styles.lightDiff}`}
        >
            <div className={styles.diffHeaderRow}>
                <div className={styles.diffHeaderCell}>Expected output</div>
                <div className={styles.diffHeaderCell}>Your output</div>
            </div>
            <div className={styles.diffScrollableBody}>
                <table className={styles.splitDiffTable}>
                    <tbody>
                        {rows.map(({ expectedLine, actualLine, key, expectedClass, actualClass, expectedLineNum, actualLineNum }) => (
                            <tr className={styles.splitDiffLine} key={key}>
                                <td className={styles.splitDiffLineNumberCell}>
                                    {expectedLineNum !== undefined
                                        ? (
                                            <span className={styles.splitDiffLineNumber}>{expectedLineNum}</span>
                                        )
                                        : ''}
                                </td>
                                <td className={styles.splitDiffCell + (expectedClass
                                    ? ` ${expectedClass}`
                                    : '')}
                                >
                                    {expectedLine}
                                </td>
                                <td className={`${styles.splitDiffLineNumberCell} ${styles.splitDiffSeparator}`}>
                                    {actualLineNum !== undefined
                                        ? (
                                            <span className={styles.splitDiffLineNumber}>{actualLineNum}</span>
                                        )
                                        : ''}
                                </td>
                                <td className={styles.splitDiffCell + (actualClass
                                    ? ` ${actualClass}`
                                    : '')}
                                >
                                    {actualLine}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default DiffViewer;
