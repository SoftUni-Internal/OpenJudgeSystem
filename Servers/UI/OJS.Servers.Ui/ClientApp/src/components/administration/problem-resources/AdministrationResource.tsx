import React from 'react';
import { useLocation } from 'react-router';

import TabsInView from '../common/tabs/TabsInView';

import ResourceForm from './problem-resource-form/ResourceForm';

const AdministrationResource = () => {
    const { pathname } = useLocation();
    const [ , , , parentId ] = pathname.split('/');
    const returnProblemForm = () => (
        <ResourceForm id={Number(parentId)} />
    );

    return (
        <TabsInView
          form={returnProblemForm}
        />
    );
};

export default AdministrationResource;
