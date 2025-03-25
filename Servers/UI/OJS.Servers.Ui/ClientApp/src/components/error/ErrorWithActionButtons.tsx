import { Link } from 'react-router-dom';
import { defaultErrorMessage } from 'src/utils/http-utils';

import useTheme from '../../hooks/use-theme';
import isNilOrEmpty from '../../utils/check-utils';
import concatClassNames from '../../utils/class-names';
import Button, { LinkButton } from '../guidelines/buttons/Button';

import styles from './ErrorWithActionButtons.module.scss';

interface IErrorProps {
    message?: string;
    backToUrl?: string;
    backToText?: string;
}

const ErrorWithActionButtons = ({ message, backToUrl, backToText }: IErrorProps) => {
    const { getColorClassName, themeColors } = useTheme();

    const getMessage = () => {
        if (isNilOrEmpty(message) || !message) {
            return defaultErrorMessage;
        }

        try {
            // There are a few cases where the error message is returned as json
            JSON.parse(message);

            // JSON.stringify adds quotes to regular strings.
            // Only JSON objects should be in quotes.
            return JSON.stringify(message);
        } catch {
            // Message is string so no quotes should be added
            return message;
        }
    };

    return (
        <div className={concatClassNames(getColorClassName(themeColors.textColor), styles.errorWrapper)}>
            <div className={styles.message}>
                {getMessage()}
            </div>
            <div className={styles.buttonsWrapper}>
                <LinkButton
                  to={`${isNilOrEmpty(backToUrl)
                      ? '/'
                      : backToUrl}`}
                  text={`${isNilOrEmpty(backToText)
                      ? 'Back to home'
                      : backToText}`}
                />
                { }
                <Button onClick={() => location.reload()} text="reload page" />
            </div>
            <div className={styles.needHelpWrapper}>
                Need help? Contact us at:
                {' '}
                {' '}
                <Link to="https://softuni.bg/contacts">softuni.bg/contacts</Link>
            </div>
        </div>
    );
};

export default ErrorWithActionButtons;
