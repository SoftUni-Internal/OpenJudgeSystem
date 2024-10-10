import React, { useCallback, useEffect } from 'react';
import { FaUser } from 'react-icons/fa';
import { useNavigate } from 'react-router';
import { Link, useParams } from 'react-router-dom';

import { ContestParticipationType } from '../../../common/constants';
import { ContestVariation } from '../../../common/contest-types';
import { IProblemResourceType } from '../../../common/types';
import { CONTESTS_PATH, PROBLEM_GROUPS_PATH } from '../../../common/urls/administration-urls';
import { getAllContestsPageUrl, getContestsResultsPageUrl } from '../../../common/urls/compose-client-urls';
import MetaTags from '../../../components/common/MetaTags';
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
import concatClassNames from '../../../utils/class-names';
import { getErrorMessage } from '../../../utils/http-utils';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
import setLayout from '../../shared/set-layout';

import styles from './ContestDetailsPage.module.scss';

const ContestDetailsPage = () => {
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const { contestId } = useParams();
    const { internalUser: user, isLoggedIn } = useAppSelector((state) => state.authorization);
    const { themeColors, getColorClassName } = useTheme();
    const { contestDetails } = useAppSelector((state) => state.contests);
    const { data, isLoading, error } = useGetContestByIdQuery({ id: Number(contestId) });

    const textColorClassName = getColorClassName(themeColors.textColor);

    const {
        id,
        name,
        type,
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
              to={getAllContestsPageUrl({ strategyId: allowedSubmissionType.id })}
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
        <div className={styles.administrationButtonsWrapper}>
            <AdministrationLink
              text="Edit"
              to={`/${CONTESTS_PATH}/${id}`}
            />
            <Button
              type={ButtonType.secondary}
              size={ButtonSize.small}
              onClick={() => navigate(getContestsResultsPageUrl({
                  contestName: name,
                  contestId: id!,
                  participationType: ContestParticipationType.Compete,
                  isSimple: true,
              }))}
            >
                Full Results
            </Button>
            <AdministrationLink
              to={`/${CONTESTS_PATH}/${contestId}#tab-problems`}
              text="Problems"
            />
            {type === ContestVariation.OnlinePracticalExam && (
            <AdministrationLink
              to={`/${PROBLEM_GROUPS_PATH}?filter=contestid~equals~${contestId}%26%26%3Bisdeleted~equals~false&sorting=id%3DDESC`}
              text="Problem Groups"
            />
            )}
            {!canBeCompeted && (competeParticipantsCount ?? 0) > 0 &&
                (<AdministrationLink text="Transfer" to={`/${CONTESTS_PATH}/${id}?openTransfer=true`} />)}
        </div>
    );

    const renderResultsText = useCallback((isCompete: boolean) => {
        const participationModeTextColorClassName = isCompete
            ? styles.greenColor
            : styles.blueColor;

        return (
            <div className={styles.resultsTextContainer}>
                <FaUser className={participationModeTextColorClassName} />
                <div className={concatClassNames(styles.underlinedBtnText, participationModeTextColorClassName)}>
                    {isCompete
                        ? 'Compete'
                        : 'Practice'}
                    {' '}
                    results
                    {' '}
                    {isCompete
                        ? competeParticipantsCount
                        : practiceParticipantsCount}
                </div>
            </div>
        );
    }, [ competeParticipantsCount, practiceParticipantsCount ]);

    const renderResultsAsLink = useCallback((isCompete: boolean) => (
        <Link
          className={`${isCompete
              ? styles.greenColor
              : styles.blueColor}`}
          to={getContestsResultsPageUrl({
              contestName: name,
              contestId: id!,
              participationType: isCompete
                  ? ContestParticipationType.Compete
                  : ContestParticipationType.Practice,
              isSimple: true,
          })}
        >
            {renderResultsText(isCompete)}
        </Link>
    ), [ id, name, renderResultsText ]);

    const renderContestActionButton = (isCompete: boolean) => {
        const isDisabled = isCompete
            ? !canBeCompeted
            : !canBePracticed;

        const canViewResults = isCompete
            ? contestDetails?.canViewCompeteResults
            : contestDetails?.canViewPracticeResults;

        return (
            <div className={styles.actionBtnWrapper}>
                <ContestButton isCompete={isCompete} isDisabled={isDisabled} id={id!} name={name ?? ''} />
                {
                    canViewResults
                        ? renderResultsAsLink(isCompete)
                        : renderResultsText(isCompete)
                }
            </div>
        );
    };

    if (error) {
        return (
            <ErrorWithActionButtons
              message={getErrorMessage(error)}
              backToText="Back to contests"
              backToUrl={getAllContestsPageUrl({})}
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
            <MetaTags
              title={`Contest #${contestId} - SoftUni Judge`}
              description={
                    `Join Contest #${contestId} on SoftUni Judge. Solve challenging problems, ` +
                    'compete with others, and enhance your coding skills. Explore contest details.'
                }
            />
            <ContestBreadcrumbs />
            <Heading className={styles.heading} type={HeadingType.primary}>{name}</Heading>
            { isLoggedIn &&
                // Legacy message will probably be removed in the near future
                // This value is based contests from current moment
                // seems like there are no future contests created before this id
                (id && id < 4973) &&
                <LegacyInfoMessage />}
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
