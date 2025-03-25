import React from 'react';
import { useLocation } from 'react-router';

import TabsInView from '../common/tabs/TabsInView';

import ProblemResourceForm from './problem-resource-form/ProblemResourceForm';

const AdministrationProblemResource = () => {
    const { pathname } = useLocation();
    const [ , , , problemId ] = pathname.split('/');
    const returnProblemForm = () => 
        <ProblemResourceForm id={Number(problemId)} />
    ;

    return (
        <TabsInView
          form={returnProblemForm}
        />
    );
};

export default AdministrationProblemResource;
