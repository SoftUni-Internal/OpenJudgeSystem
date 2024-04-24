import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router';
import { useNavigate } from 'react-router-dom';

import { ContestParticipationType } from '../../../common/constants';
import ContestCompeteModal from '../../../components/contests/contest-compete-modal/ContestCompeteModal';
import ContestPasswordForm from '../../../components/contests/contest-password-form/ContestPasswordForm';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { useRegisterUserForContestMutation } from '../../../redux/services/contestsService';
import { flexCenterObjectStyles } from '../../../utils/object-utils';

import styles from './ContestRegister.module.scss';

const ContestRegister = () => {
    const navigate = useNavigate();
    const { contestId, participationType } = useParams();
    const [ hasAcceptedOnlineModal, setHasAcceptedOnlineModal ] = useState<boolean>(false);

    const [
        registerUserForContest, {
            data,
            isLoading,
            error,
        },
    ] = useRegisterUserForContestMutation();

    const {
        id,
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

        if (!isRegisteredSuccessfully) {
            registerUserForContest({
                id: Number(contestId),
                isOfficial: participationType === ContestParticipationType.Compete,
                password: '',
                hasConfirmedParticipation: hasAcceptedOnlineModal,
            });
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    useEffect(() => {
        if (!shouldConfirmParticipation && data) {
            setHasAcceptedOnlineModal(true);
        }
    }, [ shouldConfirmParticipation, data ]);

    useEffect(() => {
        if (isLoading) {
            return;
        }
        if (isRegisteredSuccessfully && !shouldConfirmParticipation && !requirePassword) {
            navigate(`/contests/${contestId}/${participationType}`);
        }
    }, [ isLoading, isRegisteredSuccessfully, navigate, contestId, participationType, shouldConfirmParticipation, requirePassword ]);

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
                          navigate(`/contests/${id}/${participationType}`);
                      }
                  }}
                  onDecline={() => navigate('/contests')}
                />
            );
        }
        if (requirePassword) {
            return (
                <ContestPasswordForm
                  contestName={name!}
                  hasConfirmedParticipation={hasAcceptedOnlineModal}
                  onSuccess={() => navigate(`/contests/${id}/${participationType}`)}
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
        id,
        participationType,
        requirePassword,
        setHasAcceptedOnlineModal,
        registerUserForContest,
        navigate,
    ]);

    if (isLoading) {
        return <div style={{ ...flexCenterObjectStyles }}><SpinningLoader /></div>;
    }
    if (error) {
        return <div>Error fetching user register information! PLease try again.</div>;
    }
    return (
        <div className={styles.contestRegisterWrapper}>
            {renderContestRegisterBody()}
        </div>
    );
};

export default ContestRegister;
