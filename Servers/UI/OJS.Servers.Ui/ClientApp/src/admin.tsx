 
 
import { Suspense } from 'react';
import { createRoot } from 'react-dom/client';

import SpinningLoader from './components/guidelines/spinning-loader/SpinningLoader';
import AdminApp from './AdminApp';

const container = document.getElementById('root');
const root = createRoot(container!);

const comp = 
    <Suspense fallback={<SpinningLoader />}>
        <AdminApp />
    </Suspense>
;

root
    .render(comp);
