import React from 'react';

import MetaTags from '../../components/common/MetaTags';
import Breadcrumbs, { IPageBreadcrumbsItem } from '../../components/guidelines/breadcrumb/Breadcrumbs';
import RecentSubmissions from '../../components/submissions/recent-submissions/RecentSubmissions';
import setLayout from '../shared/set-layout';

const SubmissionsPage = () => (
    <>
        <MetaTags
          title="Recent Submissions - SoftUni Judge"
          description={
                'View the latest coding submissions on SoftUni Judge. Track progress, ' +
                'analyze recent solutions, and improve your programming skills.'
            }
        />
        <Breadcrumbs
          keyPrefix="submissions"
          items={[
               { text: 'Submissions' } as IPageBreadcrumbsItem,
          ]}
        />
        <RecentSubmissions />
    </>

);

export default setLayout(SubmissionsPage);
