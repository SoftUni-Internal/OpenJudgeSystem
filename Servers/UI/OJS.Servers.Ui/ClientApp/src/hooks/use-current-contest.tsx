import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import isNil from 'lodash/isNil';
import sum from 'lodash/sum';

import {
    IContestType,
    IRegisterForContestResponseType,
    IStartParticipationResponseType,
} from '../common/types';
import {
    IGetContestParticipationScoresForParticipantUrlParams,
    IRegisterForContestUrlParams,
    IStartContestParticipationUrlParams,
    ISubmitContestPasswordUrlParams,
} from '../common/url-types';
import { IHaveChildrenProps } from '../components/common/Props';

import { IErrorDataType, useHttp } from './use-http';
import { useLoading } from './use-loading';
import { useUrls } from './use-urls';

interface IStartContestArgs {
    id: number;
    isOfficial: boolean;
}

interface ISubmitContestPasswordArgs {
    id: number;
    isOfficial: boolean;
    password: string;
}

interface ICurrentContestContext {
    state: {
        contest: IContestType | null;
        contestPassword: string | null;
        currentContestParticipantScores: IMaximumParticipantScores[];
        score: number;
        maxScore: number;
        isOfficial: boolean;
        requirePassword: boolean | null;
        submitContestPasswordErrorMessage: string | null;
        isPasswordValid: boolean | null;
        remainingTimeInMilliseconds: number;
        userSubmissionsTimeLimit: number;
        totalParticipantsCount: number;
        activeParticipantsCount: number;
        isSubmitAllowed: boolean;
        contestError: IErrorDataType | null;
    };
    actions: {
        setContestPassword: (password: string) => void;
        start: (info: IStartContestArgs) => void;
        register: (info: IStartContestArgs) => void;
        submitPassword: (info: ISubmitContestPasswordArgs) => void;
        loadParticipantScores: () => void;
        setIsSubmitAllowed: (isSubmitAllowed: boolean) => void;
    };
}

const defaultState = {
    state: {
        contest: null,
        contestPassword: null,
        score: 0,
        maxScore: 0,
        isOfficial: false,
        requirePassword: false,
        remainingTimeInMilliseconds: 0.0,
        userSubmissionsTimeLimit: 0,
        totalParticipantsCount: 0,
        activeParticipantsCount: 0,
    },
};

const CurrentContestsContext = createContext<ICurrentContestContext>(defaultState as ICurrentContestContext);

type ICurrentContestsProviderProps = IHaveChildrenProps

interface IContestToStartType {
    id: number;
    isOfficial: boolean;
}

type IMaximumParticipantScores = {
    problemId: number;
    points: number;
}

const CurrentContestsProvider = ({ children }: ICurrentContestsProviderProps) => {
    const [ contest, setContest ] = useState<IContestType | null>(defaultState.state.contest);
    const [ contestPassword, setContestPassword ] = useState<string | null>(defaultState.state.contest);
    const [ participantId, setParticipantId ] = useState<number | null>(null);
    const [ score, setScore ] = useState(defaultState.state.score);
    const [ maxScore, setMaxScore ] = useState(defaultState.state.maxScore);
    const [ isOfficial, setIsOfficial ] = useState(defaultState.state.isOfficial);
    const [ requirePassword, setRequirePassword ] = useState<boolean | null>(null);
    const [ currentContestParticipantScores, setCurrentContestParticipantScores ] =
        useState<IMaximumParticipantScores[]>([]);
    const [ contestToStart, setContestToStart ] = useState<IContestToStartType | null>(null);
    const [ registerForContestParams, setRegisterForContestParams ] = useState<IRegisterForContestUrlParams | null>(null);
    const [
        getCurrentParticipantParticipantScoresParams,
        setGetCurrentParticipantParticipantScoresParams,
    ] = useState<IGetContestParticipationScoresForParticipantUrlParams | null>(null);
    const [ submitContestPasswordUrlParams, setSubmitContestPasswordUrlParams ] = useState<ISubmitContestPasswordUrlParams | null>(null);
    const [ submitContestPasswordErrorMessage, setSubmitContestPasswordErrorMessage ] = useState<string | null>(null);
    const [ isPasswordValid, setIsPasswordValid ] = useState<boolean | null>(null);
    const [ userSubmissionsTimeLimit, setUserSubmissionsTimeLimit ] = useState<number>(0);
    const [ remainingTimeInMilliseconds, setRemainingTimeInMilliseconds ] = useState(defaultState.state.remainingTimeInMilliseconds);
    const [ totalParticipantsCount, setTotalParticipantsCount ] = useState(defaultState.state.totalParticipantsCount);
    const [ activeParticipantsCount, setActiveParticipantsCount ] = useState(defaultState.state.activeParticipantsCount);
    const [ isSubmitAllowed, setIsSubmitAllowed ] = useState<boolean>(true);
    const [ contestError, setContestError ] = useState<IErrorDataType | null>(null);

    const {
        startLoading,
        stopLoading,
    } = useLoading();

    const {
        getStartContestParticipationUrl,
        getContestParticipantScoresForParticipantUrl,
        getRegisterForContestUrl,
        getSubmitContestPasswordUrl,
    } = useUrls();

    const {
        get: startContest,
        data: startContestData,
        error: startContestError,
    } = useHttp<IStartContestParticipationUrlParams, IStartParticipationResponseType>({
        url: getStartContestParticipationUrl,
        parameters: contestToStart,
    });

    const {
        get: registerForContest,
        data: registerForContestData,
        error: registerContestError,
    } = useHttp<IRegisterForContestUrlParams, IRegisterForContestResponseType>({
        url: getRegisterForContestUrl,
        parameters: registerForContestParams,
    });

    const {
        post: submitContestPassword,
        data: submitContestPasswordData,
        response: submitContestPasswordResponse,
    } = useHttp<ISubmitContestPasswordUrlParams, null>({
        url: getSubmitContestPasswordUrl,
        parameters: submitContestPasswordUrlParams,
    });

    const {
        get: getParticipantScores,
        data: getParticipantScoresData,
    } = useHttp<IGetContestParticipationScoresForParticipantUrlParams, null>({
        url: getContestParticipantScoresForParticipantUrl,
        parameters: getCurrentParticipantParticipantScoresParams,
    });

    const start = useCallback((obj: IContestToStartType) => {
        setContestToStart(obj);
    }, []);

    const register = useCallback((obj: IStartContestArgs) => {
        const { id, isOfficial: official } = obj;

        setRegisterForContestParams({ id, isOfficial: official } as IRegisterForContestUrlParams);
    }, []);

    const submitPassword = useCallback(({ id, isOfficial: official, password }: ISubmitContestPasswordArgs) => {
        setSubmitContestPasswordUrlParams({
            id,
            isOfficial: official,
        } as ISubmitContestPasswordUrlParams);
        setContestPassword(password);
    }, []);

    const loadParticipantScores = useCallback(async () => {
        if (isNil(participantId)) {
            return;
        }

        setGetCurrentParticipantParticipantScoresParams({ participantId } as IGetContestParticipationScoresForParticipantUrlParams);
    }, [ participantId ]);

    useEffect(() => {
        if (isNil(getCurrentParticipantParticipantScoresParams)) {
            return;
        }

        (async () => {
            startLoading();
            await getParticipantScores();
            stopLoading();
        })();
    }, [
        getCurrentParticipantParticipantScoresParams,
        getParticipantScores,
        startLoading, stopLoading ]);

    useEffect(() => {
        if (isNil(getParticipantScoresData)) {
            return;
        }

        setCurrentContestParticipantScores(getParticipantScoresData);
        setGetCurrentParticipantParticipantScoresParams(null);
    }, [
        getParticipantScores,
        getParticipantScoresData,
        startLoading, stopLoading,
    ]);

    useEffect(
        () => {
            if (isNil(contestToStart)) {
                return;
            }

            const { id } = contestToStart;
            if (Number.isNaN(id)) {
                return;
            }

            (async () => {
                startLoading();
                await startContest();
                stopLoading();
            })();
        },
        [ contestToStart, startContest, startLoading, stopLoading ],
    );

    useEffect(
        () => {
            if (isNil(startContestData)) {
                return;
            }
            if (!isNil(startContestError)) {
                setContestError(startContestError);
                return;
            }

            const {
                contest: newContest,
                contestIsCompete,
                participantId: currentParticipantId,
                remainingTimeInMilliseconds: newRemainingTimeInMilliseconds,
                totalParticipantsCount: newTotalParticipants,
                activeParticipantsCount: newActiveParticipants,
            } = startContestData;

            setContest(newContest);
            setIsOfficial(contestIsCompete);
            setParticipantId(currentParticipantId);
            setRemainingTimeInMilliseconds(newRemainingTimeInMilliseconds);
            setUserSubmissionsTimeLimit(startContestData.userSubmissionsTimeLimit);
            setTotalParticipantsCount(newTotalParticipants);
            setActiveParticipantsCount(newActiveParticipants);
        },
        [ startContestData, startContestError ],
    );

    useEffect(() => {
        if (isNil(registerForContestParams)) {
            return;
        }

        const { id } = registerForContestParams;
        if (Number.isNaN(id)) {
            return;
        }

        (async () => {
            startLoading();
            await registerForContest();
            stopLoading();
        })();
    }, [ registerForContest, registerForContestParams, startLoading, stopLoading ]);

    useEffect(
        () => {
            if (isNil(registerForContestData)) {
                return;
            }

            if (!isNil(registerContestError)) {
                setContestError(registerContestError);
                setRequirePassword(false);
                return;
            }

            const { requirePassword: responseRequirePassword } = registerForContestData;

            setContest({ id: registerForContestData.id, name: registerForContestData.name } as IContestType);
            setRequirePassword(responseRequirePassword);
        },
        [ registerForContestData, registerContestError ],
    );

    useEffect(() => {
        if (isNil(submitContestPasswordUrlParams)) {
            return;
        }

        (async () => {
            startLoading();
            await submitContestPassword({ password: contestPassword } as any);
            stopLoading();
        })();
    }, [ contestPassword, submitContestPassword, submitContestPasswordUrlParams, startLoading, stopLoading ]);

    useEffect(() => {
        if (isNil(submitContestPasswordResponse)) {
            return;
        }

        // TODO: fix this https://github.com/SoftUni-Internal/exam-systems-issues/issues/224
        if (!isNil(submitContestPasswordResponse) && submitContestPasswordResponse.status !== 200) {
            setSubmitContestPasswordErrorMessage('Incorrect password');
            setIsPasswordValid(false);

            return;
        }

        setIsPasswordValid(true);
        setSubmitContestPasswordErrorMessage(null);
    }, [ registerForContestData, submitContestPasswordData, submitContestPasswordResponse ]);

    useEffect(
        () => {
            const { problems } = contest || {};

            if (isNil(problems)) {
                return;
            }

            setScore(sum(problems.map((p) => isNil(p.points)
                ? 0
                : p.points)));
            setMaxScore(sum(problems.map((p) => p.maximumPoints)));
        },
        [ contest ],
    );

    useEffect(() => {
        if (isNil(participantId)) {
            return;
        }

        (async () => {
            await loadParticipantScores();
        })();
    }, [ loadParticipantScores, participantId ]);

    const value = useMemo(
        () => ({
            state: {
                contest,
                currentContestParticipantScores,
                contestPassword,
                score,
                maxScore,
                isOfficial,
                requirePassword,
                submitContestPasswordErrorMessage,
                isPasswordValid,
                remainingTimeInMilliseconds,
                userSubmissionsTimeLimit,
                totalParticipantsCount,
                activeParticipantsCount,
                isSubmitAllowed,
                contestError,
            },
            actions: {
                setContestPassword,
                register,
                start,
                submitPassword,
                loadParticipantScores,
                setIsSubmitAllowed,
            },
        }),
        [
            contest,
            contestPassword,
            currentContestParticipantScores,
            isOfficial,
            isPasswordValid,
            maxScore,
            register,
            remainingTimeInMilliseconds,
            userSubmissionsTimeLimit,
            requirePassword,
            score,
            start,
            submitContestPasswordErrorMessage,
            submitPassword,
            loadParticipantScores,
            totalParticipantsCount,
            activeParticipantsCount,
            isSubmitAllowed,
            setIsSubmitAllowed,
            contestError,
        ],
    );

    return (
        <CurrentContestsContext.Provider value={value}>
            {children}
        </CurrentContestsContext.Provider>
    );
};

const useCurrentContest = () => useContext(CurrentContestsContext);

export default CurrentContestsProvider;

export {
    useCurrentContest,
};
