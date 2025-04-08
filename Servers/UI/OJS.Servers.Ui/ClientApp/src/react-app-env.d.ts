 

/// <reference types="react-scripts" />
declare module '*.css';

declare module '*.svg?react' {
    import * as React from 'react';

    export const ReactComponent: React.FunctionComponent<React.SVGProps<SVGSVGElement>>;

    export default ReactComponent;
}
