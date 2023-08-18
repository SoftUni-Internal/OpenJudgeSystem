import React, { useCallback, useEffect, useMemo, useState } from 'react';

import { convertToTwoDigitValues, secondsToFullTime } from '../../../utils/dates';
import Text, { TextType } from '../text/Text';

enum Metric {
    seconds = 1,
    minutes = 2,
    hours = 3,
    days = 4,
}

interface ICountdownRemainingType {
    days: number;
    hours: number;
    minutes: number;
    seconds: number;
}

interface ICountdownProps {
    duration: number;
    metric: Metric;
    renderRemainingTime?: (countdownRemaining: ICountdownRemainingType) => React.ReactElement;
    handleOnCountdownEnd?: () => void;
    handleOnCountdownChange?: (seconds: number) => void;
}

const remainingTimeClassName = 'remainingTime';

const defaultRender = (remainingTime: ICountdownRemainingType) => {
    const { days, hours, minutes, seconds } = convertToTwoDigitValues(remainingTime);

    if (!Number(days)) {
        return (
            <p className={remainingTimeClassName}>
                Remaining time:
                {' '}
                <Text type={TextType.Bold}>
                    {' '}
                    {hours}
                    {' '}
                    h,
                    {' '}
                    {minutes}
                    {' '}
                    m,
                    {' '}
                    {seconds}
                    {' '}
                    s
                </Text>
            </p>
        );
    }

    return (
        <p className={remainingTimeClassName}>
            Remaining time:
            {' '}
            <Text type={TextType.Bold}>
                {' '}
                {days}
                {' '}
                d,
                {' '}
                {hours}
                {' '}
                h,
                {' '}
                {minutes}
                {' '}
                m
            </Text>
        </p>
    );
};

const Countdown = ({
    duration,
    metric,
    renderRemainingTime = defaultRender,
    handleOnCountdownEnd = () => null,
    handleOnCountdownChange = () => null,
}: ICountdownProps) => {
    const metricsToSecondsDelta = useMemo(() => ({
        [Metric.seconds]: 1,
        [Metric.minutes]: 60,
        [Metric.hours]: 60 * 60,
        [Metric.days]: 24 * 60 * 60,
    }), []);

    const [ remainingInSeconds, setRemainingInSeconds ] = useState(0);

    useEffect(() => {
        setRemainingInSeconds(duration * metricsToSecondsDelta[metric]);
    }, [ duration, metric, metricsToSecondsDelta ]);

    const decreaseRemainingTime = useCallback(
        () => setRemainingInSeconds(remainingInSeconds - 1),
        [ remainingInSeconds ],
    );

    useEffect(() => {
        if (remainingInSeconds < 0) {
            handleOnCountdownEnd();
        }

        const timer = setTimeout(decreaseRemainingTime, 1000);

        return () => clearTimeout(timer);
    }, [ decreaseRemainingTime, handleOnCountdownEnd, remainingInSeconds ]);

    useEffect(() => {
        handleOnCountdownChange(remainingInSeconds);
    }, [ handleOnCountdownChange, remainingInSeconds ]);

    return (
        <>
            {renderRemainingTime(secondsToFullTime(remainingInSeconds))}
        </>
    );
};

export default Countdown;

export {
    Metric,
};

export type {
    ICountdownRemainingType,
};
