import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router';
import { useNavigate } from 'react-router-dom';
import BackToTop from 'src/components/common/back-to-top/BackToTop';

import { ContestParticipationType } from '../../../common/constants';
import { getAllContestsPageUrl, getContestsSolutionSubmitPageUrl } from '../../../common/urls/compose-client-urls';
import ContestCompeteModal from '../../../components/contests/contest-compete-modal/ContestCompeteModal';
import ContestPasswordForm from '../../../components/contests/contest-password-form/ContestPasswordForm';
import ErrorWithActionButtons from '../../../components/error/ErrorWithActionButtons';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { useGetRegisteredUserForContestQuery, useRegisterUserForContestMutation } from '../../../redux/services/contestsService';
import { getErrorMessage } from '../../../utils/http-utils';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
import makePrivate from '../../shared/make-private';
import withTitle from '../../shared/with-title';

import styles from './ContestRegister.module.scss';

const ContestRegister = () => {
    const navigate = useNavigate();
    const { contestId, participationType, slug } = useParams();
    const [ hasAcceptedOnlineModal, setHasAcceptedOnlineModal ] = useState<boolean>(false);

    const {
        data,
        isLoading,
        error,
    } = useGetRegisteredUserForContestQuery({ id: Number(contestId), isOfficial: participationType === ContestParticipationType.Compete });

    const [ registerUserForContest, { isError, error: registerError } ] = useRegisterUserForContestMutation();

    const {
        name,
        requirePassword,
        shouldConfirmParticipation,
        duration,
        numberOfProblems,
        isRegisteredSuccessfully,
    } = data || {};

    useEffect(() => {
        if (isLoading) {
            return;
        }
        if (isRegisteredSuccessfully && !shouldConfirmParticipation && !requirePassword) {
            navigate(getContestsSolutionSubmitPageUrl({
                isCompete: participationType === ContestParticipationType.Compete,
                contestId,
                contestName: slug,
            }), { replace: true });
        }
    }, [ isLoading, isRegisteredSuccessfully, navigate, contestId, participationType, shouldConfirmParticipation, requirePassword, slug ]);

    // Register user automatically if no password or modal confirmation is required
    useEffect(() => {
        if (isLoading || !data) {
            return;
        }
        if (!requirePassword && !shouldConfirmParticipation && !isRegisteredSuccessfully) {
            // eslint-disable-next-line promise/catch-or-return
            registerUserForContest({
                id: Number(contestId),
                isOfficial: participationType === ContestParticipationType.Compete,
                password: '',
                hasConfirmedParticipation: true,
                // eslint-disable-next-line promise/prefer-await-to-then,promise/always-return
            }).then(() => {
                navigate(getContestsSolutionSubmitPageUrl({
                    isCompete: participationType === ContestParticipationType.Compete,
                    contestId,
                    contestName: slug,
                }), { replace: true });
            });
        }
    }, [
        isLoading,
        data,
        requirePassword,
        shouldConfirmParticipation,
        isRegisteredSuccessfully,
        contestId,
        participationType,
        registerUserForContest,
        navigate,
        slug,
    ]);

    useEffect(() => {
        if (!shouldConfirmParticipation && data) {
            setHasAcceptedOnlineModal(true);
        }
    }, [ shouldConfirmParticipation, data ]);

    const renderContestRegisterBody = useCallback(() => {
        if (!hasAcceptedOnlineModal) {
            return (
                <ContestCompeteModal
                  examName={name || ''}
                  time={duration?.toString() || '-'}
                  problemsCount={numberOfProblems!}
                  onAccept={async () => {
                      setHasAcceptedOnlineModal(true);
                      if (!requirePassword) {
                          await registerUserForContest({
                              id: Number(contestId),
                              isOfficial: participationType === ContestParticipationType.Compete,
                              password: '',
                              hasConfirmedParticipation: true,
                          });
                          navigate(getContestsSolutionSubmitPageUrl({
                              isCompete: participationType === ContestParticipationType.Compete,
                              contestId,
                              contestName: slug,
                          }), { replace: true });
                      }
                  }}
                  onDecline={() => navigate(getAllContestsPageUrl({}), { replace: true })}
                />
            );
        }
        if (requirePassword) {
            return (
                <ContestPasswordForm
                  contestName={name!}
                  hasConfirmedParticipation={hasAcceptedOnlineModal}
                  onSuccess={() => navigate(getContestsSolutionSubmitPageUrl({
                      isCompete: participationType === ContestParticipationType.Compete,
                      contestId,
                      contestName: slug,
                  }), { replace: true })}
                />
            );
        }
        return <div />;
    }, [
        hasAcceptedOnlineModal,
        name,
        duration,
        numberOfProblems,
        contestId,
        participationType,
        requirePassword,
        setHasAcceptedOnlineModal,
        registerUserForContest,
        navigate,
        slug,
    ]);

    if (isLoading) {
        return <div style={{ ...flexCenterObjectStyles }}><SpinningLoader /></div>;
    }

    if (error) {
        return (
            <ErrorWithActionButtons
              message={getErrorMessage(error)}
              backToUrl={getAllContestsPageUrl({})}
              backToText="Back to contests"
            />
        );
    }

    if (isError && registerError) {
        return (
            <ErrorWithActionButtons
              message={getErrorMessage(registerError)}
              backToUrl={getAllContestsPageUrl({})}
              backToText="Back to contests"
            />
        );
    }

    return (
        <>
            <BackToTop />
            <div className={styles.contestRegisterWrapper}>
                {renderContestRegisterBody()}
            </div>
        </>
    );
};

export default makePrivate(withTitle(ContestRegister, (params) => `Contest Register #${params.contestId}`));
