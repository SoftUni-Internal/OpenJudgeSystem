/* eslint-disable @typescript-eslint/naming-convention */
/* eslint-disable import/no-unused-modules */
/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_UI_SERVER_URL: string;
  readonly VITE_ADMINISTRATION_URL: string;
  readonly VITE_PLATFORM_URL: string;
  readonly VITE_YOUTUBE_VIDEO_ID: string;
}

declare module '*.css';

/* Optional: Vite “?react” loader */
declare module '*.svg?react' {
    import * as React from 'react';

    const ReactComponent: React.FunctionComponent<React.SVGProps<SVGSVGElement>>;

    export default ReactComponent;
}
