import React, { useCallback } from 'react';
import GitHubIcon from '@mui/icons-material/GitHub';

import logo from '../../assets/softuni-logo-horizontal-white.svg';
import { LinkButton, LinkButtonType } from '../../components/guidelines/buttons/Button';

import styles from './FooterNavigation.module.scss';

const FooterNavigation = () => {
    const renderSystemInfoAndLinksSection = useCallback(
        () => 
            <div className={styles.systemInfoAndLinksContainer}>
                <span>
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
                        <GitHubIcon />
                    </LinkButton>
                </span>
            </div>
        , [],
    );

    return (
        <div className={styles.content}>
            <LinkButton
              to="https://platform.softuni.bg/"
              type={LinkButtonType.image}
              className={styles.footerLogo}
              altText="Softuni logo"
              imgSrc={logo}
              isToExternal
            />
            {renderSystemInfoAndLinksSection()}
        </div>
    );
};

export default FooterNavigation;
