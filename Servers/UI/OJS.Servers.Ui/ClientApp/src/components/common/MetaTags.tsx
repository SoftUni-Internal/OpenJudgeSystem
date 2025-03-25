import React from 'react';
import { Helmet } from 'react-helmet-async';

interface IMetaTagsProps {
    title: string;
    description: string;
}

const MetaTags = ({ title, description }: IMetaTagsProps) => 
    <Helmet>
        <title>{title}</title>
        <meta name="description" content={description} />
    </Helmet>
;

export default MetaTags;
