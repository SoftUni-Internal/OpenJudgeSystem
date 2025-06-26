import React, { useState } from 'react';
import { BiMemoryCard } from 'react-icons/bi';
import { FaRegClock } from 'react-icons/fa';
import { Popover } from '@mui/material';
import { useLazyGetTestDetailsQuery } from 'src/redux/services/testsService';
import isNilOrEmpty from 'src/utils/check-utils';
import concatClassNames from 'src/utils/class-names';

import { TestRunResultType } from '../../../common/constants';
import { getResultTypeText, getTestResultColorId } from '../../../common/submissions-utils';
import { ITestRunType } from '../../../hooks/submissions/types';
import useTheme from '../../../hooks/use-theme';
import CodeEditor from '../../code-editor/CodeEditor';
import MultiLineTextDisplay from '../../common/MultiLineTextDisplay';
import DiffViewer from '../../diffViewer/DiffViewer';
import AdministrationLink from '../../guidelines/buttons/AdministrationLink';
import Button, { ButtonSize, ButtonType, LinkButtonType } from '../../guidelines/buttons/Button';

import styles from './SubmissionTestRun.module.scss';

interface ISubmissionTestRunProps {
    testRun: ITestRunType;
    idx: number;
    shouldRenderAdminData?: boolean;
}

const SubmissionTestRun = (props: ISubmissionTestRunProps) => {
    const { testRun, idx, shouldRenderAdminData = false } = props;

    const { isDarkMode, themeColors, getColorClassName } = useTheme();

    const [ testShowInput, setTestShowInput ] = useState<boolean>(false);
    const [ memoryAnchorEl, setMemoryAnchorEl ] = useState<HTMLElement | null>(null);
    const [ timeAnchorEl, setTimeAnchorEl ] = useState<HTMLElement | null>(null);
    const [ input, setInput ] = useState(testRun.input);
    const [ getTestDetails ] = useLazyGetTestDetailsQuery();

    const textColorClassName = getColorClassName(themeColors.textColor);
    const backgroundColorClassName = getColorClassName(isDarkMode
        ? themeColors.baseColor200
        : themeColors.baseColor100);

    const isMemoryModalOpen = Boolean(memoryAnchorEl);
    const isTimeModalOpen = Boolean(timeAnchorEl);

    const {
        testId,
        resultType,
        isTrialTest,
        memoryUsed,
        showInput,
        timeUsed,
        expectedOutputFragment,
        userOutputFragment,
    } = testRun;

    const onPopoverClose = (popover: string) => {
        if (popover === 'memory') {
            setMemoryAnchorEl(null);
        } if (popover === 'time') {
            setTimeAnchorEl(null);
        }
    };

    const onPopoverOpen = (popover: string, event: React.MouseEvent<HTMLElement>) => {
        if (popover === 'memory') {
            setMemoryAnchorEl(event.currentTarget);
        } if (popover === 'time') {
            setTimeAnchorEl(event.currentTarget);
        }
    };

    const onShowHideInputButtonClick = async () => {
        if (isNilOrEmpty(input)) {
            const details = await getTestDetails({ id: testId, submissionId: testRun.submissionId });
            setInput(details?.data?.input ?? '');
        }

        setTestShowInput(!testShowInput);
    };

    // eslint-disable-next-line @typescript-eslint/no-unsafe-enum-comparison
    const isCorrectAnswer = resultType === TestRunResultType.CorrectAnswer;

    return (
        <div
          key={`test-run-${testRun.id}`}
          id={`test-run-${testRun.id}`}
          className={`${backgroundColorClassName} ${textColorClassName} ${styles.testRunWrapper}`}
        >
            <div className={styles.testRunTitleWrapper}>
                <div className={styles.testNameButtonWrapper}>
                    <div style={{ color: getTestResultColorId(resultType) }}>
                        {isTrialTest && 'Zero '}
                        Test #
                        {idx}
                        {!isCorrectAnswer && ` (${getResultTypeText(resultType)})`}
                    </div>
                </div>
                <div className={styles.testDetailsAndMemoryWrapper}>
                    {showInput &&
                        <Button
                          onClick={() => onShowHideInputButtonClick()}
                          text={testShowInput
                              ? 'HIDE INPUT'
                              : 'SHOW INPUT'}
                          type={ButtonType.neutral}
                          size={ButtonSize.small}
                        />
                    }
                    <div className={styles.timeAndMemoryWrapper}>
                        {shouldRenderAdminData &&
                            <AdministrationLink
                              text={`Test #${testId}`}
                              to={`/tests/${testId}`}
                              type={LinkButtonType.plain}
                              className={styles.testLink}
                            />
                        }
                        {shouldRenderAdminData &&
                            <span>
                                Run #
                                {testRun.id}
                            </span>
                        }
                        <span
                          onMouseEnter={(e) => onPopoverOpen('memory', e)}
                          onMouseLeave={() => onPopoverClose('memory')}
                          className={styles.iconAndTextWrapper}
                        >
                            <BiMemoryCard size={20} />
                            <span>
                                {(memoryUsed / 1000000).toFixed(2)}
                                {' '}
                                MB
                            </span>
                            <Popover
                              open={isMemoryModalOpen}
                              anchorEl={memoryAnchorEl}
                              anchorOrigin={{
                                  vertical: 'bottom',
                                  horizontal: 'left',
                              }}
                              transformOrigin={{
                                  vertical: 'top',
                                  horizontal: 'left',
                              }}
                              sx={{ pointerEvents: 'none' }}
                              onClose={() => onPopoverClose('memory')}
                              disableRestoreFocus
                            >
                                <div className={`${styles.timeAndMemoryModal} ${backgroundColorClassName}`}>
                                    Memory used
                                </div>
                            </Popover>
                        </span>
                        <span
                          onMouseEnter={(e) => onPopoverOpen('time', e)}
                          onMouseLeave={() => onPopoverClose('time')}
                          className={styles.iconAndTextWrapper}
                        >
                            <FaRegClock size={20} />
                            <span>
                                {timeUsed / 1000}
                                {' '}
                                s.
                            </span>
                            <Popover
                              open={isTimeModalOpen}
                              anchorEl={timeAnchorEl}
                              anchorOrigin={{
                                  vertical: 'bottom',
                                  horizontal: 'left',
                              }}
                              transformOrigin={{
                                  vertical: 'top',
                                  horizontal: 'left',
                              }}
                              sx={{ pointerEvents: 'none' }}
                              onClose={() => onPopoverClose('time')}
                              disableRestoreFocus
                            >
                                <div className={`${styles.timeAndMemoryModal} ${backgroundColorClassName}`}>
                                    Time used
                                </div>
                            </Popover>
                        </span>
                    </div>
                </div>

            </div>
            {isTrialTest &&
                <div
                  style={{ color: themeColors.baseColor100 }}
                  className={concatClassNames(styles.zeroTestsInfoMessage, !isDarkMode
                      ? getColorClassName(themeColors.textColor)
                      : '')}
                >
                    The zero tests are not included in the final result
                </div>
            }
            {testShowInput &&
                <>
                    <div>Test input:</div>
                    <CodeEditor code={input} readOnly customEditorStyles={{ height: '150px', marginTop: '12px' }} />
                </>
            }
            {(expectedOutputFragment || userOutputFragment) &&
                <div className={styles.outputWrapper}>
                    <DiffViewer expectedStr={expectedOutputFragment} actualStr={userOutputFragment} />
                </div>
            }
            {
                // eslint-disable-next-line @typescript-eslint/no-unsafe-enum-comparison
                testRun.resultType === TestRunResultType.RunTimeError && testRun.executionComment &&
                    <MultiLineTextDisplay
                      text={testRun.executionComment}
                      maxVisibleLines={5}
                      className={styles.runtimeExecutionComment}
                    />

            }
        </div>
    );
};

export default SubmissionTestRun;
