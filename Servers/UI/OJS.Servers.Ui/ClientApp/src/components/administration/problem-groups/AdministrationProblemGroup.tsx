import { SyntheticEvent, useState } from 'react';
import { useLocation } from 'react-router-dom';

import useScrollToTab from '../../../hooks/common/use-scroll-to-tab';
import TabsInView from '../common/tabs/TabsInView';

import ProblemGroupForm from './problem-group-form/ProblemGroupForm';
import ProblemsInProblemGroupView from './problems-in-problem-group-view/ProblemsInProblemGroupView';

enum PROBLEM_GROUP_LISTED_DATA {
    PROBLEMS = 'problems',
}

const AdministrationProblemGroup = () => {
    const { pathname, hash } = useLocation();
    const [ , , , problemGroupId ] = pathname.split('/');

    const [ tabName, setTabName ] = useState(PROBLEM_GROUP_LISTED_DATA.PROBLEMS);

    const onTabChange = (event: SyntheticEvent, newValue: PROBLEM_GROUP_LISTED_DATA) => {
        setTabName(newValue);
    };

    useScrollToTab({ hash, tabName, setTabName, tabNames: Object.values(PROBLEM_GROUP_LISTED_DATA) });

    const returnProblemGroupForm = () => 
        <ProblemGroupForm id={Number(problemGroupId)} />
    ;

    const returnProblems = (key:string) => 
        <div id={PROBLEM_GROUP_LISTED_DATA.PROBLEMS}>
            <ProblemsInProblemGroupView key={key} problemGroupId={Number(problemGroupId)} />
        </div>
    ;

    return (
        <TabsInView
          form={returnProblemGroupForm}
          onTabChange={onTabChange}
          tabName={tabName}
          tabs={[
              { value: PROBLEM_GROUP_LISTED_DATA.PROBLEMS, label: 'Problems', node: returnProblems },
          ]}
        />
    );
};
export default AdministrationProblemGroup;
