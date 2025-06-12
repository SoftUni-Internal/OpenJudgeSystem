import { useLocation } from 'react-router-dom';

import TabsInView from '../common/tabs/TabsInView';

import SubmissionTypeDocumentForm from './submission-type-document-form/SubmissionTypeDocumentForm';

const AdministrationSubmissionTypeDocumentPage = () => {
    const location = useLocation();
    const [ , , , submissionTypeDocumentId ] = location.pathname.split('/');

    const params = new URLSearchParams(location.search);

    const isEditMode = params.get('isEditMode') === 'true';

    const renderSubmissionTypeDocumentForm = () => (
        <SubmissionTypeDocumentForm id={Number(submissionTypeDocumentId)} isEditMode={isEditMode} />
    );

    return (
        <TabsInView form={renderSubmissionTypeDocumentForm} />
    );
};

export default AdministrationSubmissionTypeDocumentPage;
