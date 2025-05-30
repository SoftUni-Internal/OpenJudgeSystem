import React from 'react';
import { useLocation } from 'react-router';

import TabsInView from '../common/tabs/TabsInView';

import ResourceForm from './problem-resource-form/ResourceForm';

const AdministrationResource = () => {
    const { pathname } = useLocation();
    const [ , , , problemId ] = pathname.split('/');
    const returnProblemForm = () => (
        <ResourceForm id={Number(problemId)} />
    );

    return (
        <TabsInView
          form={returnProblemForm}
        />
    );
};

export default AdministrationResource;
