import React, { useState } from 'react';
import { useLocation } from 'react-router-dom';
import { renderSuccessfullAlert } from 'src/utils/render-utils';

import { USERS } from '../../../common/labels';
import useScrollToTab from '../../../hooks/common/use-scroll-to-tab';
import TabsInView from '../common/tabs/TabsInView';

import ExamGroupEdit from './exam-group-edit/ExamGroupEdit';
import UsersInExamGroupView from './users-in-exam-group-view/UsersInExamGroupView';

enum EXAM_GROUPS_LISTED_DATA {
    USERS_TAB = 'users',
}
const AdministrationExamGroupPage = () => {
    const { pathname, hash } = useLocation();
    const [ , , , examGroupId ] = pathname.split('/');
    const [ tabName, setTabName ] = useState(EXAM_GROUPS_LISTED_DATA.USERS_TAB);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);

    const [ contestId, setContestId ] = useState<number | null>(null);

    const getExamGroupContestId = (contestIdToSave: number | null) => {
        setContestId(contestIdToSave);
    };

    useScrollToTab({ hash, tabName, setTabName, tabNames: Object.values(EXAM_GROUPS_LISTED_DATA) });

    const returnExamGroupForm = () => 
        <ExamGroupEdit examGroupId={Number(examGroupId)} getContestId={getExamGroupContestId} />
    ;

    const onTabChange = (event: React.SyntheticEvent, newValue: EXAM_GROUPS_LISTED_DATA) => {
        setTabName(newValue);
    };

    const returnUsers = (key: string) => 
        <div id={EXAM_GROUPS_LISTED_DATA.USERS_TAB}>
            <UsersInExamGroupView
              key={key}
              examGroupId={Number(examGroupId)}
              isAllowedToAddUsers={contestId !== null}
              setParentSuccessMessage={setSuccessMessage}
            />
        </div>
    ;

    return (
        <>
            {renderSuccessfullAlert(successMessage, 7000)}
            <TabsInView
              form={returnExamGroupForm}
              onTabChange={onTabChange}
              tabName={tabName}
              tabs={[
                  { value: EXAM_GROUPS_LISTED_DATA.USERS_TAB, label: USERS, node: returnUsers },
              ]}
            />
        </>
    );
};
export default AdministrationExamGroupPage;
