import React, { useCallback, useMemo } from 'react';

import logo from '../../assets/softuni-logo-horizontal-white.svg';
import { LinkButton, LinkButtonType } from '../../components/guidelines/buttons/Button';
import GithubIcon from '../../components/guidelines/icons/GitHubIcon';

import styles from './FooterNavigation.module.scss';

interface IFooterLinkType {
    text: string;
    url: string;
}

const FooterNavigation = () => {
    const learnLinks = useMemo(() => Array.from<IFooterLinkType>([
        {
            text: 'Professional Programs',
            url: 'https://learn.softuni.org/catalog#program',
        },
        {
            text: 'Courses',
            url: 'https://learn.softuni.org/catalog#opencourse',
        },
        {
            text: 'Open Lessons',
            url: 'https://learn.softuni.org/catalog#openlesson',
        } ]), []);

    const renderSystemInfoAndLinksSection = useCallback(() => (
        <div className={styles.systemInfoAndLinksContainer}>
            <span className={styles.systemInfo}>
                © 2011 -
                {' '}
                {new Date().getFullYear()}
                {' '}
                - Open Judge System (OJS)
            </span>
            <span className={styles.links}>
                <LinkButton
                  to="https://github.com/SoftUni-Internal/OpenJudgeSystem"
                  type={LinkButtonType.plain}
                  isToExternal
                >
                    <GithubIcon />
                </LinkButton>
            </span>
        </div>
    ), []);

    return (
        <div className={styles.content}>
            <LinkButton
              to="https://platform.softuni.bg/"
              type={LinkButtonType.image}
              className={styles.footerLogo}
              altText="Softuni logo"
              imgSrc={logo}
            />
            {renderSystemInfoAndLinksSection()}
        </div>
    );
};

export default FooterNavigation;
