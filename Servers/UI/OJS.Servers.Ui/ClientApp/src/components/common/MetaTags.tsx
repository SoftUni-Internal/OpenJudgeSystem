import React from 'react';
import { Helmet } from '@dr.pogodin/react-helmet';

interface IMetaTagsProps {
    title: string;
    description: string;
}

const MetaTags = ({ title, description }: IMetaTagsProps) => (
    <Helmet>
        <title>{title}</title>
        <meta name="description" content={description} />
    </Helmet>
);

export default MetaTags;
