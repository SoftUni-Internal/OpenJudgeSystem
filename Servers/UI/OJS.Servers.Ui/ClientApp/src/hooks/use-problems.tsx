import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import first from 'lodash/first';
import isNil from 'lodash/isNil';

import { IProblemType } from '../common/types';
import { IDownloadProblemResourceUrlParams } from '../common/url-types';
import { IHaveChildrenProps } from '../components/common/Props';

import { useHashUrlParams } from './common/use-hash-url-params';
import { useCurrentContest } from './use-current-contest';
import { useHttp } from './use-http';
import { useLoading } from './use-loading';
import { useUrls } from './use-urls';

interface IProblemsContext {
    state: {
        problems: IProblemType[];
        currentProblem: IProblemType | null;
    };
    actions: {
        selectProblemById: (id: number) => void;
        downloadProblemResourceFile: (resourceId: number) => Promise<void>;
        initiateProblems: () => void;
    };
}

type IProblemsProviderProps = IHaveChildrenProps

const defaultState = {
    state: {
        problems: [] as IProblemType[],
        currentProblem: {} as IProblemType,
    },
};

const normalizeOrderBy = (problems: IProblemType[]) => {
    problems.sort((x, y) => x.orderBy - y.orderBy);

    return problems.map((p, i) => ({
        ...p,
        orderBy: i + 1,
    }));
};

const ProblemsContext = createContext<IProblemsContext>(defaultState as IProblemsContext);

const ProblemsProvider = ({ children }: IProblemsProviderProps) => {
    const { state: { contest } } = useCurrentContest();
    const {
        state: { hashParam },
        actions: { setHash },
    } = useHashUrlParams();
    const [ problems, setProblems ] = useState(defaultState.state.problems);
    const [ currentProblem, setCurrentProblem ] = useState<IProblemType | null>(defaultState.state.currentProblem);
    const [ problemResourceIdToDownload, setProblemResourceIdToDownload ] = useState<number | null>(null);
    const { getDownloadProblemResourceUrl } = useUrls();

    const {
        startLoading,
        stopLoading,
    } = useLoading();

    const {
        get: downloadProblemResource,
        response: downloadProblemResourceResponse,
        saveAttachment,
    } = useHttp<IDownloadProblemResourceUrlParams, Blob>({
        url: getDownloadProblemResourceUrl,
        parameters: { id: problemResourceIdToDownload },
    });

    const normalizedProblems = useMemo(
        () => normalizeOrderBy(problems),
        [ problems ],
    );

    const selectProblemById = useCallback(
        (problemId: number) => {
            const newProblem = normalizedProblems.find((p) => p.id === problemId);

            if (isNil(newProblem)) {
                return;
            }

            setCurrentProblem(newProblem);
            const { orderBy } = newProblem;
            setHash(orderBy.toString());
        },
        [ setHash, normalizedProblems ],
    );

    const problemFromHash = useMemo(
        () => {
            const hashIndex = Number(hashParam) - 1;
            return normalizedProblems[hashIndex];
        },
        [ normalizedProblems, hashParam ],
    );

    const isLoadedFromHash = useMemo(
        () => !isNil(problemFromHash),
        [ problemFromHash ],
    );

    const initiateProblems = useCallback(
        () => {
            const { problems: newProblems } = contest || {};

            if (!newProblems) {
                return;
            }

            setProblems(newProblems);
            const { id } = first(normalizedProblems) || {};

            if (isNil(id)) {
                return;
            }

            if (isLoadedFromHash) {
                setCurrentProblem(problemFromHash);
            } else {
                selectProblemById(id);
            }
        },
        [ contest, isLoadedFromHash, normalizedProblems, problemFromHash, selectProblemById ],
    );

    const downloadProblemResourceFile = useCallback(async (resourceId: number) => {
        setProblemResourceIdToDownload(resourceId);
    }, []);

    useEffect(() => {
        if (isNil(downloadProblemResourceResponse)) {
            return;
        }

        saveAttachment();
    }, [ downloadProblemResourceResponse, problemResourceIdToDownload, saveAttachment ]);

    useEffect(
        () => {
            if (isNil(problemResourceIdToDownload)) {
                return;
            }

            (async () => {
                startLoading();
                await downloadProblemResource('blob');
                stopLoading();
            })();

            setProblemResourceIdToDownload(null);
        },
        [ downloadProblemResource, problemResourceIdToDownload, startLoading, stopLoading ],
    );

    const value = useMemo(
        () => ({
            state: {
                problems,
                currentProblem,
            },
            actions: {
                selectProblemById,
                downloadProblemResourceFile,
                initiateProblems,
            },
        }),
        [ currentProblem, downloadProblemResourceFile, initiateProblems, problems, selectProblemById ],
    );

    return (
        <ProblemsContext.Provider value={value}>
            {children}
        </ProblemsContext.Provider>
    );
};

const useProblems = () => useContext(ProblemsContext);

export {
    useProblems,
};

export default ProblemsProvider;
