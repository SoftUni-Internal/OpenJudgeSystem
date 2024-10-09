import moment, { Duration, unitOfTime } from 'moment';

const defaultDateTimeFormat = 'HH:MM, DD/MMM/YYYY';
const defaultDateTimeFormatReverse = 'DD/MMM/YYYY, HH:MM';
const defaultPreciseDateTimeFormat = 'DD/MMM/YYYY, HH:mm:ss';

const submissionsGridDateFormat = 'DD.MM.YYYY';
const submissionsGridTimeFormat = 'HH:mm:ss';

const dateTimeFormatWithSpacing = 'D MMM YY, HH:mm';

const calculatedTimeFormatted =
    (duration: Duration) => `${Math.floor(duration.asDays())} d, ${duration.hours()} h, ${duration.minutes()} m`;

const isCurrentTimeAfterOrEqualTo = (date: Date):
    boolean => moment().local().isSameOrAfter(moment(date).utc(true).local());

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

export {
    defaultDateTimeFormatReverse,
    dateTimeFormatWithSpacing,
    submissionsGridDateFormat,
    submissionsGridTimeFormat,
    formatDate,
    preciseFormatDate,
    isCurrentTimeAfterOrEqualTo,
    calculateTimeUntil,
    calculatedTimeFormatted,
    timeToWords,
    transformSecondsToTimeSpan,
    transformDaysHoursMinutesTextToMinutes,
};
