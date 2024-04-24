/* eslint-disable import/no-unused-modules */
/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { Suspense } from 'react';
import { createRoot } from 'react-dom/client';

import SpinningLoader from './components/guidelines/spinning-loader/SpinningLoader';
import App from './App';
import register from './registerServiceWorker';

const container = document.getElementById('root');
const root = createRoot(container!);

const comp = (
    <Suspense fallback={<SpinningLoader />}>
        <App />
    </Suspense>
);

root
    .render(comp);

register();
