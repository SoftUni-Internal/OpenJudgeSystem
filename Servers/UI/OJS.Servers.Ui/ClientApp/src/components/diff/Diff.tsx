/* eslint-disable react/no-danger */

import React, { useCallback } from 'react';
import ReactDiffViewer, { DiffMethod } from 'react-diff-viewer';
import Prism from 'prismjs';
import concatClassNames from 'src/utils/class-names';

import 'prismjs/components/prism-javascript';

import useTheme from '../../hooks/use-theme';

import 'prismjs/themes/prism-okaidia.css';
import styles from './Diff.module.scss';

interface ITestsRunDiffProps {
    expectedStr: string;
    actualStr: string;
}

const Diff = ({ expectedStr, actualStr } : ITestsRunDiffProps) => {
    const { isDarkMode } = useTheme();

    const highlightSyntax = useCallback((str: string) => {
        if (str) {
            return (
                <pre
                  style={{ display: 'inline' }}
                  dangerouslySetInnerHTML={{ __html: Prism.highlight(str, Prism.languages.text, 'apex') }}
                />
            );
        }

        return (
            <>
                {null}
                {null}
            </>
        );
    }, []);

    return (
        <>
            { expectedStr !== actualStr && (
                <>
                    <div className={styles.diffDetailsText}>
                        <h4>Expected output:</h4>
                    </div>
                    <div className={styles.diffDetailsText}>
                        <h4>Your output:</h4>
                    </div>
                </>
            )}
            <div className={concatClassNames(
                styles.diffWrapper,
                isDarkMode
                    ? styles.darkDiff
                    : styles.lightDiff,
            )}
            >
                <ReactDiffViewer
                  oldValue={expectedStr}
                  newValue={actualStr}
                  compareMethod={DiffMethod.CHARS}
                  splitView
                  showDiffOnly
                  renderContent={highlightSyntax}
                  useDarkTheme={isDarkMode}
                  styles={{ line: { wordBreak: 'break-all' } }}
                />
            </div>
        </>

    );
};

export default Diff;
