import React, { useCallback } from 'react';
import { IconType } from 'react-icons';
import { FaCode, FaDeezer, FaPuzzlePiece, FaTasks, FaTrophy, FaUsers } from 'react-icons/fa';
import LiteYouTubeEmbed from 'react-lite-youtube-embed';
import concatClassNames from 'src/utils/class-names';

import MetaTags from '../../components/common/MetaTags';
import IconSize from '../../components/guidelines/icons/common/icon-sizes';
import Icon from '../../components/guidelines/icons/Icon';
import SpinningLoader from '../../components/guidelines/spinning-loader/SpinningLoader';
import useTheme from '../../hooks/use-theme';
import { useGetHomeStatisticsQuery } from '../../redux/services/homeStatisticsService';
import { flexCenterObjectStyles } from '../../utils/object-utils';

import 'react-lite-youtube-embed/dist/LiteYouTubeEmbed.css';
import styles from './HomeInfoPage.module.scss';

interface IHomePageStatistic {
    title: string;
    iconType: IconType;
    count: string;
}

const HomePageStatistic = (props: IHomePageStatistic) => {
    const { title, iconType, count } = props;

    const { isDarkMode, themeColors, getColorClassName } = useTheme();
    const textColorClassName = getColorClassName(themeColors.textColor);

    return (
        <div className={styles.homePageStatisticWrapper}>
            <Icon
              size={IconSize.ExtraLarge}
              Component={iconType}
              color={isDarkMode
                  ? themeColors.baseColor100
                  : themeColors.textColor}
            />
            <div className={styles.homePageStatisticTextWrapper}>
                <div style={{ color: '#44a9f8' }}>{title}</div>
                <div className={`${styles.homeStatisticCount} ${textColorClassName}`}>{count}</div>
            </div>
        </div>
    );
};

const HOME_STATISTICS = [
    { iconType: FaTrophy, title: 'Contests', dataKey: 'contestsCount' },
    { iconType: FaUsers, title: 'Users', dataKey: 'usersCount' },
    { iconType: FaTasks, title: 'Test strategies', dataKey: 'strategiesCount' },
    { iconType: FaPuzzlePiece, title: 'Problems', dataKey: 'problemsCount' },
    { iconType: FaDeezer, title: 'Submissions per day', dataKey: 'submissionsPerDayCount' },
    { iconType: FaCode, title: 'Submissions', dataKey: 'submissionsCount' },
];

const HomePage = () => {
    const { data, isLoading, error } = useGetHomeStatisticsQuery();
    const { isDarkMode, themeColors, getColorClassName } = useTheme();

    const YOUTUBE_VIDEO_ID = `${import.meta.env.VITE_YOUTUBE_VIDEO_ID}`;

    const textColorClassName = getColorClassName(themeColors.textColor);

    const mapStatisticsNumber = (value: number) => {
        if (value > 1000000) {
            return `${(value / 1000000).toFixed(1)} M`;
        }

        if (value > 1000) {
            return `${(value / 1000).toFixed(1)} K`;
        }

        return value.toString();
    };

    const renderHomeStatisticIcons = useCallback(() => {
        if (isLoading) {
            return <div style={{ ...flexCenterObjectStyles }}><SpinningLoader /></div>;
        }
        if (error) {
            return <div className={textColorClassName}>Error fetching statistics data. Please try again!</div>;
        }

        return (
            <div className={styles.gridWrapper}>
                <div className={concatClassNames(styles.homeStatistics)}>
                    {HOME_STATISTICS.map((el, idx) => {
                        const { iconType, title, dataKey } = el;
                         
                        return (
                             
                            <HomePageStatistic
                              key={`home-page-statistic-item-${idx}`}
                              title={title}
                              iconType={iconType}
                              count={mapStatisticsNumber(Number(data[dataKey]))}
                            />
                        );
                    })}
                </div>
            </div>
        );
    }, [ data, error, isLoading, textColorClassName ]);

    return (
        <div className={styles.homePageContentWrapper}>
            <MetaTags
              title="SoftUni Judge Platform"
              description={
                    'Improve your programming skills on SoftUni Judge. Solve coding challenges, ' +
                    'submit solutions, and track your progress with detailed statistics.'
                }
            />
            <div className={styles.homePageHeader}>How to use SoftUni Judge Platform</div>
            <LiteYouTubeEmbed
              id={YOUTUBE_VIDEO_ID}
              title="Guidelines for working with the SoftUni Judge platform"
              poster="maxresdefault"
              wrapperClass={`yt-lite ${styles.youTubeVideoWrapper} ${isDarkMode
                  ? styles.darkTheme
                  : ''}`}
            />
            <hr />
            {renderHomeStatisticIcons()}
        </div>
    );
};

export default HomePage;
