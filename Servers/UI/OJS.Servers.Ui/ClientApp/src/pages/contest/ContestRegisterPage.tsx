import React, { useCallback, useEffect, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import isEmpty from 'lodash/isEmpty';
import isNil from 'lodash/isNil';

import { ContestParticipationType } from '../../common/constants';
import ContestPasswordForm from '../../components/contests/contest-password-form/ContestPasswordForm';
import { useHashUrlParams } from '../../hooks/common/use-hash-url-params';
import { useRouteUrlParams } from '../../hooks/common/use-route-url-params';
import { useCurrentContest } from '../../hooks/use-current-contest';
import { makePrivate } from '../shared/make-private';
import { setLayout } from '../shared/set-layout';

import styles from './ContestRegisterPage.module.scss';

const ContestRegisterPage = () => {
    const { state: { params } } = useRouteUrlParams();
    const { state: { params: hashParam } } = useHashUrlParams();

    const {
        contestId,
        participationType,
    } = params;

    const navigate = useNavigate();

    const contestIdToNumber = useMemo(() => Number(contestId), [ contestId ]);
    const isParticipationOfficial = useMemo(() => participationType === ContestParticipationType.Compete, [ participationType ]);
    const internalContest = useMemo(
        () => ({
            id: contestIdToNumber,
            isOfficial: isParticipationOfficial,
        }),
        [ contestIdToNumber, isParticipationOfficial ],
    );

    const {
        state: {
            requirePassword,
            isPasswordValid,
        },
        actions: { register },
    } = useCurrentContest();

    const doesNotRequirePassword = useMemo(
        () => !isNil(requirePassword) && !requirePassword,
        [ requirePassword ],
    );
    const isSubmittedPasswordValid = useMemo(() => !isNil(isPasswordValid) && isPasswordValid, [ isPasswordValid ]);

    const navigateToPage = useCallback(
        () => isEmpty(hashParam)
            ? navigate(`/contests/${contestId}/${participationType}`)
            : navigate(`/contests/${contestId}/${participationType}#${hashParam}`),
        [ contestId, navigate, hashParam, participationType ],
    );

    useEffect(() => {
        (async () => {
            if (isEmpty(contestId)) {
                return;
            }

            await register(internalContest);
        })();
    }, [ internalContest, contestIdToNumber, isParticipationOfficial, participationType, register, contestId ]);

    useEffect(
        () => {
            if (isNil(contestId) || isNil(participationType)) {
                return;
            }

            if (doesNotRequirePassword || isSubmittedPasswordValid) {
                navigateToPage();
            }
        },
        [ contestId, doesNotRequirePassword, isSubmittedPasswordValid, navigateToPage, participationType ],
    );

    return (
        <div className={styles.container}>
            {
                requirePassword
                    ? (
                        <ContestPasswordForm
                          id={contestIdToNumber}
                          isOfficial={isParticipationOfficial}
                        />
                    )
                    : <p>No password required. Redirecting to contest.</p>
            }
        </div>
    );
};

export default makePrivate(setLayout(ContestRegisterPage));
