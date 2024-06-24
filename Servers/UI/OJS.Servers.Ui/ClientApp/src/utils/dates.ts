import { differenceInDays, intervalToDuration } from 'date-fns';
import moment, { Duration, unitOfTime } from 'moment';

interface IConvertToTwoDigitValuesParamType {
    days: number;
    hours: number;
    minutes: number;
    seconds: number;
}

const defaultDateTimeFormat = 'HH:MM, DD/MMM/YYYY';
const defaultDateTimeFormatPreciseTime = 'HH:MM:ss, DD/MMM/YYYY';
const defaultDateTimeFormatReverse = 'DD/MMM/YYYY, HH:MM';
const defaultPreciseDateTimeFormat = 'DD/MMM/YYYY, HH:mm:ss';

const dateTimeFormatWithSpacing = 'D MMM YY, HH:mm';

const calculateTimeBetweenTwoDates = (startDate: Date, endDate: Date) => moment(startDate).diff(moment(endDate), 'second');

const calculatedTimeFormatted =
    (duration: Duration) => `${Math.floor(duration.asDays())} d, ${duration.hours()} h, ${duration.minutes()} m`;

const convertTimeIntervalToHoursMinutesAndSeconds =
    (duration: Duration) => `${Math.floor(duration.asHours())}:${duration.minutes()}:${duration.seconds()}`;

const calculateTimeUntil = (date: Date, unit: unitOfTime.Diff = 'milliseconds'):
    Duration => moment.duration(moment(date)
    .utc(true)
    .local()
    .diff(moment()
        .local()), unit);

const preciseFormatDate = (
    date: Date,
    formatString = defaultPreciseDateTimeFormat,
) => moment(date).utc(true).local().format(formatString);

const formatDate = (
    date: Date,
    formatString = defaultDateTimeFormat,
) => (moment().diff(date, 'days') > 3
    ? preciseFormatDate(date, formatString)
    : moment(date).utc(true).local().fromNow());

const getCurrentTimeInUTC = () => {
    const now = moment().utc();
    return new Date(
        now.year(),
        now.month(),
        now.date(),
        now.hour(),
        now.minute(),
        now.second(),
        now.millisecond(),
    );
};

const convertToSecondsRemaining = (date: Date) => {
    const currentDate = getCurrentTimeInUTC();
    const { hours, minutes, seconds } = intervalToDuration({
        start: currentDate,
        end: date,
    });

    const daysRemaining = differenceInDays(date, currentDate);

    const hoursRemaining = daysRemaining * 24 + (hours ?? 0);

    const minutesRemaining = hoursRemaining * 60 + (minutes ?? 0);

    return minutesRemaining * 60 + (seconds ?? 0);
};

// This function converts a given duration in seconds into a time object that includes
// days, hours, minutes, and seconds.
// The function first calculates the number of whole days within the duration
// The remaining seconds are computed using the modulus operator (%)
// The function 'intervalToDuration' is then used to break down the remaining seconds into h, m, s
const secondsToFullTime = (duration: number) => {
    // Number of seconds in a day: 86400 (60 seconds * 60 minutes * 24 hours)
    const days = Math.floor(duration / 86400);
    const remainingSeconds = duration % 86400;
    const { hours: hoursInitial, minutes: minutesInitial, seconds: secondsInitial } =
        intervalToDuration({ start: 0, end: remainingSeconds * 1000 });

    const hours = hoursInitial ?? 0;

    const minutes = minutesInitial ?? 0;

    const seconds = secondsInitial ?? 0;

    return { days, hours, minutes, seconds };
};

const timeToWords = (time: string) => {
    // Split the time string into hours, minutes, and seconds
    const parts = time.split(':');
    const hours = parseInt(parts[0], 10);
    const minutes = parseInt(parts[1], 10);

    // Create the time description in words
    let description = '';
    if (hours > 0) {
        description += hours + (hours === 1
            ? ' hour'
            : ' hours');
    }
    if (minutes > 0) {
        if (description.length > 0) {
            description += ' and ';
        }
        description += minutes + (minutes === 1
            ? ' minute'
            : ' minutes');
    }

    return description;
};

const transformDaysHoursMinutesTextToMinutes = (durationString: string) => {
    const components = durationString.split(', ');

    let days = 0; let hours = 0; let minutes = 0;

    components.forEach((component) => {
        const [ value, unit ] = component.split(' ');
        if (unit === 'd') {
            days = parseInt(value, 10);
        } else if (unit === 'h') {
            hours = parseInt(value, 10);
        } else if (unit === 'm') {
            minutes = parseInt(value, 10);
        }
    });

    return days * 24 * 60 + hours * 60 + minutes;
};

const transformSecondsToTimeSpan = (seconds: number) => {
    const duration = moment.duration(seconds, 'seconds');

    return moment.utc(duration.asMilliseconds()).format('mm:ss');
};

const convertToTwoDigitValues = ({
    days: daysValue,
    hours: hoursValue,
    minutes: minutesValue,
    seconds: secondsValue,
}: IConvertToTwoDigitValuesParamType) => {
    const days = daysValue >= 10
        ? daysValue.toString()
        : `0${daysValue}`;
    const hours = hoursValue >= 10
        ? hoursValue.toString()
        : `0${hoursValue}`;

    const minutes = minutesValue >= 10
        ? minutesValue.toString()
        : `0${minutesValue}`;

    const seconds = secondsValue >= 10
        ? secondsValue.toString()
        : `0${secondsValue}`;

    return {
        days,
        hours,
        minutes,
        seconds,
    };
};

export {
    defaultDateTimeFormat,
    defaultDateTimeFormatPreciseTime,
    defaultDateTimeFormatReverse,
    defaultPreciseDateTimeFormat,
    dateTimeFormatWithSpacing,
    formatDate,
    preciseFormatDate,
    secondsToFullTime,
    calculateTimeUntil,
    convertToSecondsRemaining,
    convertToTwoDigitValues,
    getCurrentTimeInUTC,
    calculateTimeBetweenTwoDates,
    convertTimeIntervalToHoursMinutesAndSeconds,
    calculatedTimeFormatted,
    timeToWords,
    transformSecondsToTimeSpan,
    transformDaysHoursMinutesTextToMinutes,
};
