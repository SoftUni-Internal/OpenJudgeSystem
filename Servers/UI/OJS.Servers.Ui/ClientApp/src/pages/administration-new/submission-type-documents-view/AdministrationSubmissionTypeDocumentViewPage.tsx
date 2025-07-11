/* eslint-disable max-len */
import { useLocation, useNavigate } from 'react-router-dom';
import { useAdministrationTheme } from 'src/hooks/use-administration-theme-provider';

import { ThemeMode } from '../../../common/enums';
import { ISubmissionTypeInSubmissionDocumentInViewModel } from '../../../common/types';
import { NEW_ADMINISTRATION_PATH, SUBMISSION_TYPE_DOCUMENTS_PATH } from '../../../common/urls/administration-urls';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import {
    useGetAllSubmissionTypesInSubmissionDocumentsByMultipleSubmissionTypeIdsQuery,
} from '../../../redux/services/admin/submissionTypesInSubmissionDocumentsAdminService';

import styles from './AdministrationSubmissionTypeDocumentViewPage.module.scss';

const AdministrationSubmissionTypeDocumentViewPage = () => {
    const location = useLocation();
    const navigate = useNavigate();
    const { themeMode } = useAdministrationTheme();

    const queryParams = new URLSearchParams(location.search);

    const submissionTypeIdsParam = queryParams.get('submissionTypeIds') || '';
    const submissionTypeIds = submissionTypeIdsParam
        .split(',')
        .map(Number)
        .filter((id) => !Number.isNaN(id));

    const {
        data: submissionTypeDocumentsData,
        isLoading: isDataLoading,
        isFetching: isDataFetching,
    } = useGetAllSubmissionTypesInSubmissionDocumentsByMultipleSubmissionTypeIdsQuery(
        submissionTypeIds,
        { skip: submissionTypeIds.length === 0 },
    );

    if (isDataLoading || isDataFetching) {
        return <SpinningLoader />;
    }

    return (
        <div className={`${styles.container} ${themeMode === ThemeMode.Dark
            ? styles.dark
            : styles.light}`}
        >
            {!submissionTypeDocumentsData || submissionTypeDocumentsData.length === 0
                ? <div className={styles.noDocumentsMessage}>No documents were found for the given submission type(s).</div>
                
                : submissionTypeDocumentsData.map((submissionType: ISubmissionTypeInSubmissionDocumentInViewModel, typeIndex: number) => 
                    <div key={submissionType.submissionTypeDocumentId}>
                        <ol className={styles.submissionTypeList}>
                            {submissionType.submissionTypes.map((type, index) => 
                                <button
                                  key={index}
                                  className={styles.submissionTypeItem}
                                  type="button"
                                  onClick={() => navigate(`/${NEW_ADMINISTRATION_PATH}/${SUBMISSION_TYPE_DOCUMENTS_PATH}/${submissionType.submissionTypeDocumentId}?isEditMode=true`)}
                                >
                                    {type}
                                </button>)}
                        </ol>
                        <div className={styles.entityContainer}>
                            <h2 className={styles.title}>{submissionType.submissionTypeDocumentTitle}</h2>
                            <div
                              className={`ql-editor ${themeMode === ThemeMode.Dark
                                  ? 'quill-dark-theme'
                                  : 'quill-light-theme'} ${styles.content}`}
                              dangerouslySetInnerHTML={{ __html: submissionType.submissionTypeDocumentContent }}
                            />
                        </div>
                        {typeIndex < submissionTypeDocumentsData.length - 1 && 
                        <hr className={styles.divider} />
                        }
                    </div>)}
        </div>
    );
};

export default AdministrationSubmissionTypeDocumentViewPage;
