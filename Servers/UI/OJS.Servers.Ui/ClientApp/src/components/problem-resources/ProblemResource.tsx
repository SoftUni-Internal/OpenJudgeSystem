import React, { useEffect, useState } from 'react';
import { IoDocumentText } from 'react-icons/io5';
import { Link, useLocation, useNavigate } from 'react-router-dom';

import { IProblemResourceType } from '../../common/types';
import { LOGIN_PATH } from '../../common/urls/client-urls';
import { useLazyDownloadContestProblemResourceQuery } from '../../redux/services/contestsService';
import downloadFile from '../../utils/file-download-utils';

import styles from './ProblemResource.module.scss';

interface IProblemResourceProps {
    resource: IProblemResourceType;
    problem: string;
}

const ProblemResource = ({ resource, problem }: IProblemResourceProps) => {
    const { link, name: linkName, id } = resource;
    const [ isUnauthorized, setIsUnauthorized ] = useState(false);
    const navigate = useNavigate();
    const location = useLocation();

    const [ downloadResourceFile, {
        data: problemResourceDownloadData,
        isError: problemResourceDownloadErrorState,
        error: problemResourceDownloadError,
        isLoading: problemResourceDownloadIsLoading,
    } ] = useLazyDownloadContestProblemResourceQuery();

    useEffect(() => {
        if (!problemResourceDownloadData) {
            return;
        }

        downloadFile(problemResourceDownloadData.blob, problemResourceDownloadData.fileName);
    }, [ problem, problemResourceDownloadData ]);

    // eslint-disable-next-line consistent-return
    useEffect(() => {
        if (problemResourceDownloadErrorState &&
            'status' in problemResourceDownloadError! &&
        problemResourceDownloadError.status === 401) {
            setIsUnauthorized(true);
            const timeout = setTimeout(() => {
                navigate(`/${LOGIN_PATH}`, { state: { from: location } });
            }, 3000);

            return () => clearTimeout(timeout);
        }
    }, [ problemResourceDownloadErrorState, navigate, location, problemResourceDownloadError ]);

    return (
        <>
            {
                resource.link
                    ? <Link key={`resource-problem-${id}`} className={styles.resourceElement} target="_blank" to={link}>
                        <IoDocumentText size={20} />
                        {' '}
                        {linkName}
                    </Link>
                    
                    : <div
                          key={`resource-problem-${id}`}
                          className={styles.resourceElement}
                          onClick={() => downloadResourceFile({ id })}
                        >
                        <IoDocumentText size={20} />
                        {linkName}
                    </div>
                    
            }
            {problemResourceDownloadErrorState || problemResourceDownloadIsLoading
                ? <div className={styles.problemResourceIndicator}>
                    {problemResourceDownloadErrorState
                        ? <div className={styles.problemResourceDownloadErrorState}>
                            {isUnauthorized
                                ? 'Unable to download the resource because you are not logged in. Please log in and try again.'
                                : 'Unable to download the resource. Please try again later.'}
                        </div>
                            
                        : ''}
                    {problemResourceDownloadIsLoading
                        ? <div className={styles.problemResourceLoading}>Downloading resource...</div>
                        : ''}
                </div>
                
                : null}
        </>
    );
};

export default ProblemResource;
