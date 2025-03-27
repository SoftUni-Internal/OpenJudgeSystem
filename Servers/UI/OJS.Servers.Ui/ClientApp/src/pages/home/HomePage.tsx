import { useEffect } from 'react';
import { Outlet, useLocation, useParams } from 'react-router-dom';
import BackToTop from 'src/components/common/back-to-top/BackToTop';

import ContestBreadcrumbs from '../../components/contests/contest-breadcrumbs/ContestBreadcrumbs';
import { ContestCategories } from '../../components/contests/contest-categories/ContestCategories';
import {
    clearContestCategoryBreadcrumbItems,
    setContestDetails,
} from '../../redux/features/contestsSlice';
import { useAppDispatch } from '../../redux/store';
import setLayout from '../shared/set-layout';

import 'react-lite-youtube-embed/dist/LiteYouTubeEmbed.css';
import styles from './HomePage.module.scss';

const HomePage = () => {
    const location = useLocation();
    const { categoryId } = useParams();
    const dispatch = useAppDispatch();

    useEffect(() => {
        if (!categoryId) {
            /**
             * Clear contest details and breadcrumb items when no category is selected
             * so categories tree is collapsed properly (as it depends on category in the store)
             */
            dispatch(setContestDetails({ contest: null }));
            dispatch(clearContestCategoryBreadcrumbItems());
        }
    }, [ categoryId, dispatch ]);

    return (
        <div>
            <ContestBreadcrumbs isHidden={location.pathname === '/'} />
            <div className={styles.homePageWrapper}>
                <ContestCategories />
                <Outlet />
                <BackToTop />
            </div>
        </div>
    );
};

export default setLayout(HomePage);
