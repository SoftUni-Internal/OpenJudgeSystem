import React, { useCallback, useState } from 'react';
import { Popover } from '@mui/material';

import { TestRunResultType } from '../../../common/constants';
import { getResultTypeText } from '../../../common/submissions-utils';
import { ITestRunIcon, ITestRunType } from '../../../hooks/submissions/types';
import useTheme from '../../../hooks/use-theme';
import ErrorIcon from '../../guidelines/icons/ErrorIcon';
import MemoryLimitIcon from '../../guidelines/icons/MemoryLimitIcon';
import RuntimeErrorIcon from '../../guidelines/icons/RuntimeErrorIcon';
import TickIcon from '../../guidelines/icons/TickIcon';
import TimeLimitIcon from '../../guidelines/icons/TimeLimitIcon';
import WrongAnswerIcon from '../../guidelines/icons/WrongAnswerIcon';

import styles from './TestRunIcon.module.scss';

interface ITestRunIconProps {
    testRun: ITestRunType | ITestRunIcon;
}

const TestRunIcon = ({ testRun }: ITestRunIconProps) => {
    const { getColorClassName, themeColors } = useTheme();
    const [ competeIconAnchorElement, setCompeteIconAnchorElement ] = useState<HTMLElement | null>(null);

    const isIconModalOpen = Boolean(competeIconAnchorElement);

    const backgroundColorClassName = getColorClassName(themeColors.baseColor100);

    const iconClassName = testRun.isTrialTest
        ? styles.trialTestIcon
        : '';

    const onPopoverOpen = (event: React.MouseEvent<HTMLElement>) => {
        setCompeteIconAnchorElement(event.currentTarget);
    };

    const renderTestRunIcon = useCallback(
        () => {
            switch (testRun.resultType) {
            case TestRunResultType.CorrectAnswer: return <TickIcon className={iconClassName} key={testRun.id} />;
            case TestRunResultType.WrongAnswer: return <WrongAnswerIcon className={iconClassName} key={testRun.id} />;
            case TestRunResultType.MemoryLimit: return <MemoryLimitIcon className={iconClassName} key={testRun.id} />;
            case TestRunResultType.TimeLimit: return <TimeLimitIcon className={iconClassName} key={testRun.id} />;
            case TestRunResultType.RunTimeError: return <RuntimeErrorIcon className={iconClassName} key={testRun.id} />;
            default: return (
                <ErrorIcon />
            );
            }
        },
        [ iconClassName, testRun.id, testRun.resultType ],
    );

    return (
        <span
          onMouseEnter={(e) => onPopoverOpen(e)}
          onMouseLeave={() => setCompeteIconAnchorElement(null)}
        >
            <Popover
              open={isIconModalOpen}
              anchorEl={competeIconAnchorElement}
              anchorOrigin={{
                  vertical: 'top',
                  horizontal: 'center',
              }}
              transformOrigin={{
                  vertical: 'top',
                  horizontal: 'left',
              }}
              sx={{ pointerEvents: 'none' }}
              onClose={() => setCompeteIconAnchorElement(null)}
              disableRestoreFocus
            >
                <div className={`${styles.competeIconModal} ${backgroundColorClassName}`}>
                    {getResultTypeText(testRun.resultType)}
                </div>
            </Popover>
            {renderTestRunIcon()}
        </span>
    );
};

export default TestRunIcon;
