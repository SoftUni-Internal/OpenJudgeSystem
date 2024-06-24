import React, { useEffect } from 'react';
import { useNavigate } from 'react-router';
import { Link, useParams } from 'react-router-dom';

import { IProblemResourceType } from '../../../common/types';
import { CONTESTS_PATH } from '../../../common/urls/administration-urls';
import { getAllContestsUrl, getContestsResultsUrl } from '../../../common/urls/compose-client-urls';
import ContestBreadcrumbs from '../../../components/contests/contest-breadcrumbs/ContestBreadcrumbs';
import ContestButton from '../../../components/contests/contest-button/ContestButton';
import ErrorWithActionButtons from '../../../components/error/ErrorWithActionButtons';
import AdministrationLink from '../../../components/guidelines/buttons/AdministrationLink';
import Button, { ButtonSize, ButtonType } from '../../../components/guidelines/buttons/Button';
import Heading, { HeadingType } from '../../../components/guidelines/headings/Heading';
import LegacyInfoMessage from '../../../components/guidelines/legacy-info-message/LegacyInfoMessage';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import ProblemResource from '../../../components/problem-resources/ProblemResource';
import useTheme from '../../../hooks/use-theme';
import { setContestDetails } from '../../../redux/features/contestsSlice';
import { useGetContestByIdQuery } from '../../../redux/services/contestsService';
import { useAppDispatch, useAppSelector } from '../../../redux/store';
import { getErrorMessage } from '../../../utils/http-utils';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
import { setLayout } from '../../shared/set-layout';

import styles from './ContestDetailsPage.module.scss';

const ContestDetailsPage = () => {
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const { contestId } = useParams();
    const { internalUser: user, isLoggedIn } = useAppSelector((state) => state.authorization);
    const { themeColors, getColorClassName } = useTheme();
    const { contestDetails, selectedCategory } = useAppSelector((state) => state.contests);
    const { data, isLoading, error } = useGetContestByIdQuery({ id: Number(contestId) });

    const textColorClassName = getColorClassName(themeColors.textColor);

    const {
        id,
        name,
        allowedSubmissionTypes,
        description,
        problems,
        competeParticipantsCount,
        practiceParticipantsCount,
        canBeCompeted,
        canBePracticed,
    } = data ?? {};

    useEffect(() => {
        if (contestDetails?.id !== data?.id) {
            dispatch(setContestDetails({ contest: data ?? null }));
        }
    }, [ data, contestDetails, dispatch ]);

    const renderAllowedLanguages = () => allowedSubmissionTypes?.map((allowedSubmissionType) => (
        <span key={`contest-sub-strategy-btn-${allowedSubmissionType.id}`}>
            <Link
              className={styles.allowedLanguageLink}
              to={getAllContestsUrl(selectedCategory?.id, allowedSubmissionType.id)}
            >
                {allowedSubmissionType.name}
            </Link>
            {' | '}
        </span>
    ));

    const renderProblemsNames = () => {
        if (!problems || problems.length === 0) {
            return 'The problems for this contest are not public.';
        }
        return problems.map((problem) => (
            <div key={`contest-problem-${problem.id}`} className={styles.problemNameItem}>
                <span>{problem.name}</span>
                <div className={styles.problemResources}>
                    { problem.resources.map((resource: IProblemResourceType) => (
                        <ProblemResource
                          key={`p-r-${resource.id}`}
                          resource={resource}
                          problem={problem.name}
                        />
                    ))}
                </div>
            </div>
        ));
    };

    const renderAdministrationButtons = () => (
        <div>
            <AdministrationLink
              text="Edit"
              to={`/${CONTESTS_PATH}/${id}`}
            />
            <Button
              className={styles.adminBtn}
              type={ButtonType.secondary}
              size={ButtonSize.small}
              onClick={() => navigate(getContestsResultsUrl(id!, 'compete', true))}
            >
                Full Results
            </Button>
        </div>
    );

    const renderContestActionButton = (isCompete: boolean) => {
        const isDisabled = isCompete
            ? !canBeCompeted
            : !canBePracticed;

        return (
            <div className={styles.actionBtnWrapper}>
                <ContestButton isCompete={isCompete} isDisabled={isDisabled} id={id!} />
                <Link
                  className={`${isCompete
                      ? styles.greenColor
                      : styles.blueColor}`}
                  to={getContestsResultsUrl(id!, isCompete
                      ? 'compete'
                      : 'practice', true)}
                >
                    <i className="fas fa-user" />
                    <div className={`${styles.underlinedBtnText}`}>
                        { isCompete
                            ? 'Compete'
                            : 'Practice'}
                        {' '}
                        results
                        {' '}
                        {isCompete
                            ? competeParticipantsCount
                            : practiceParticipantsCount}
                    </div>
                </Link>
            </div>
        );
    };

    if (error) {
        return (
            <ErrorWithActionButtons
              message={getErrorMessage(error)}
              backToText="Back to contests"
              backToUrl="/contests"
            />
        );
    }
    if (isLoading) {
        return (
            <div style={{ ...flexCenterObjectStyles, marginTop: 24 }}>
                <SpinningLoader />
            </div>
        );
    }
    return (
        <div className={`${styles.contestDetailsWrapper} ${textColorClassName}`}>
            <ContestBreadcrumbs />
            <Heading className={styles.heading} type={HeadingType.primary}>{name}</Heading>
            { isLoggedIn && <LegacyInfoMessage />}
            <div className={styles.descriptionBoxWrapper}>
                <div>
                    <div className={`${styles.title} ${textColorClassName}`}>Contest Details</div>
                    <div dangerouslySetInnerHTML={{
                        __html: description ||
                            'There is no description for the selected contest.',
                    }}
                    />
                    <div className={styles.languagesWrapper}>
                        <span className={styles.allowedLanguages}>Allowed languages:</span>
                        {' '}
                        {' '}
                        {renderAllowedLanguages()}
                    </div>
                    <div>
                        {user.canAccessAdministration && renderAdministrationButtons()}
                    </div>
                    <div>
                        {renderContestActionButton(true)}
                        {renderContestActionButton(false)}
                    </div>
                </div>
                <div>
                    <div className={styles.title}>Problems</div>
                    <div>{renderProblemsNames()}</div>
                </div>
            </div>
        </div>
    );
};

export default setLayout(ContestDetailsPage);
