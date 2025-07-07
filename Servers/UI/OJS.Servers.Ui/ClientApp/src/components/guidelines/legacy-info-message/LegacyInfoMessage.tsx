import concatClassNames from 'src/utils/class-names';

import useTheme from '../../../hooks/use-theme';
import AlertBox, { AlertBoxType } from '../alert-box/AlertBox';
import { ButtonSize, LinkButton, LinkButtonType } from '../buttons/Button';

import styles from './LegacyInfoMessage.module.scss';

const LegacyInfoMessage = () => {
    const { getColorClassName, themeColors, isDarkMode } = useTheme();

    const getLegacySubmissionsInfoMessage = () => 
        <p className={getColorClassName(themeColors.textColor)}>
            Contest history, submissions, and scores were not transferred to the current platform.
            To view this information, access your
            <LinkButton
              type={LinkButtonType.plain}
              size={ButtonSize.none}
              to="https://judge.softuni.org/Users/Profile"
              text=" profile"
              internalClassName={styles.profileButton}
              isToExternal
            />
            {' '}
            on the legacy system.
        </p>
    ;

    const className = concatClassNames(styles.legacyInfoMessage, isDarkMode
        ? ''
        : styles.lightLegacyInfoMessage);

    return (
        <AlertBox
          type={AlertBoxType.info}
          isClosable={false}
          className={className}
          message={getLegacySubmissionsInfoMessage()}
        />
    );
};

export default LegacyInfoMessage;
