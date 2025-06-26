 
 
import { Suspense } from 'react';
import { createRoot } from 'react-dom/client';

import SpinningLoader from './components/guidelines/spinning-loader/SpinningLoader';
import { flexCenterObjectStyles } from './utils/object-utils';
import App from './App';

import './styles/global.scss';

const container = document.getElementById('root');
const root = createRoot(container!);

const comp = 
    <Suspense fallback={<div style={{ ...flexCenterObjectStyles }}><SpinningLoader /></div>}>
        <App />
    </Suspense>
;

root
    .render(comp);
